using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;

namespace Maui.AmbeSupport;

/// <summary>
/// Hilfsfunktionen rund um den AMBE3000R-USB-Stick unter Android: Gerät finden, USB-Permission
/// einholen, Verbindung komfortabel aufbauen.
/// <para>
/// Die USB-Permission lässt sich unter Android nur asynchron (Dialog + Broadcast) einholen. Deshalb
/// passiert das VOR dem Erzeugen des Controllers; <see cref="Ambe3000UsbController.Open"/> selbst ist
/// synchron, wie es <c>DmrClient2.Start()</c> erwartet.
/// </para>
/// </summary>
public static class AmbeUsb
{
    public const int FtdiVendorId = 0x0403;
    public const int Ft230xProductId = 0x6015;

    private const string ActionUsbPermission = "com.digitalvoicecontrolapp.USB_PERMISSION";

    /// <summary>
    /// Sucht den AMBE3000R-Stick (FTDI, VID 0x0403). Bevorzugt den FT230X (PID 0x6015),
    /// nimmt sonst irgendein FTDI-Gerät.
    /// </summary>
    public static UsbDevice? FindDevice(Context context)
    {
        var usbManager = (UsbManager)context.GetSystemService(Context.UsbService)!;
        var devices = usbManager.DeviceList?.Values?.ToList() ?? [];

        return devices.FirstOrDefault(d => d.VendorId == FtdiVendorId && d.ProductId == Ft230xProductId)
            ?? devices.FirstOrDefault(d => d.VendorId == FtdiVendorId);
    }

    /// <summary>
    /// Fordert die USB-Permission für das Gerät an (zeigt ggf. den System-Dialog).
    /// </summary>
    /// <returns>true, wenn die Permission vorliegt bzw. erteilt wurde.</returns>
    public static async Task<bool> RequestPermissionAsync(Context context, UsbDevice device, TimeSpan? timeout = null)
    {
        var usbManager = (UsbManager)context.GetSystemService(Context.UsbService)!;
        if (usbManager.HasPermission(device))
            return true;

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var receiver = new PermissionReceiver(granted => tcs.TrySetResult(granted));
        var filter = new IntentFilter(ActionUsbPermission);

        if (OperatingSystem.IsAndroidVersionAtLeast(33))
            context.RegisterReceiver(receiver, filter, ReceiverFlags.NotExported);
        else
            context.RegisterReceiver(receiver, filter);

        try
        {
            var flags = OperatingSystem.IsAndroidVersionAtLeast(31)
                ? PendingIntentFlags.Mutable
                : PendingIntentFlags.UpdateCurrent;

            var intent = new Intent(ActionUsbPermission);
            // Package explizit setzen, sonst wird der Broadcast ab Android 14 ggf. nicht zugestellt.
            intent.SetPackage(context.PackageName);

            var pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, flags);
            usbManager.RequestPermission(device, pendingIntent);

            var finished = await Task.WhenAny(tcs.Task, Task.Delay(timeout ?? TimeSpan.FromSeconds(60)));
            return finished == tcs.Task && tcs.Task.Result;
        }
        finally
        {
            try { context.UnregisterReceiver(receiver); }
            catch (Java.Lang.IllegalArgumentException) { /* war nicht (mehr) registriert */ }
        }
    }

    /// <summary>
    /// Blockierende Variante von <see cref="RequestPermissionAsync"/> für Code, der auf einem Hintergrund-Thread läuft
    /// (kein async/await nötig). DARF NICHT auf dem UI-Thread aufgerufen werden: Die Antwort des System-Dialogs wird auf dem
    /// UI-Thread zugestellt, ein Warten dort wäre ein Deadlock.
    /// </summary>
    /// <returns>true, wenn die Permission vorliegt bzw. erteilt wurde.</returns>
    public static bool RequestPermission(Context context, UsbDevice device, TimeSpan? timeout = null)
    {
        if (Looper.MyLooper() == Looper.MainLooper)
            throw new InvalidOperationException("RequestPermission() must not be called on the UI thread.");

        var usbManager = (UsbManager)context.GetSystemService(Context.UsbService)!;
        if (usbManager.HasPermission(device))
            return true;

        bool granted = false;
        using var answered = new ManualResetEventSlim(false);
        var receiver = new PermissionReceiver(g => { granted = g; answered.Set(); });
        var filter = new IntentFilter(ActionUsbPermission);

        if (OperatingSystem.IsAndroidVersionAtLeast(33))
            context.RegisterReceiver(receiver, filter, ReceiverFlags.NotExported);
        else
            context.RegisterReceiver(receiver, filter);

        try
        {
            var flags = OperatingSystem.IsAndroidVersionAtLeast(31)
                ? PendingIntentFlags.Mutable
                : PendingIntentFlags.UpdateCurrent;

            var intent = new Intent(ActionUsbPermission);
            intent.SetPackage(context.PackageName);

            var pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, flags);
            usbManager.RequestPermission(device, pendingIntent);

            return answered.Wait(timeout ?? TimeSpan.FromSeconds(60)) && granted;
        }
        finally
        {
            try { context.UnregisterReceiver(receiver); }
            catch (Java.Lang.IllegalArgumentException) { /* war nicht (mehr) registriert */ }
        }
    }

    /// <summary>
    /// Findet den Stick, holt die Permission ein und öffnet die Verbindung (FTDI-Init). Der Controller
    /// wird danach geöffnet zurückgegeben; <c>DmrClient2.Start()</c> darf trotzdem <c>Open()</c> aufrufen
    /// (idempotent).
    /// </summary>
    public static async Task<(Ambe3000UsbController? Controller, string? Error)> ConnectAsync(Context context)
    {
        var device = FindDevice(context);
        if (device == null)
            return (null, "No AMBE stick (FTDI, VID 0x0403) found. Are the OTG adapter and the stick plugged in?");

        if (!await RequestPermissionAsync(context, device))
            return (null, "USB permission was not granted.");

        var controller = new Ambe3000UsbController(context, device);
        try
        {
            await Task.Run(controller.Open); // blockiert kurz (OpenDevice, FTDI-Init)
            return (controller, null);
        }
        catch (Exception ex)
        {
            controller.Dispose();
            return (null, $"Opening failed: {ex.Message}");
        }
    }

    private sealed class PermissionReceiver : BroadcastReceiver
    {
        private readonly Action<bool> _callback;

        public PermissionReceiver(Action<bool> callback) => _callback = callback;

        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent?.Action != ActionUsbPermission)
                return;

            _callback(intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false));
        }
    }
}
