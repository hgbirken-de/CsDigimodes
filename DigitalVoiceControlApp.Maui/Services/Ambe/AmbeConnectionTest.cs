using Android.Content;
using Android.Hardware.Usb;
using NLog;

namespace DigitalVoiceControlApp.Maui.Services.Ambe;

/// <summary>
/// Einmaliger End-to-End-Verbindungstest für den AMBE3000R-USB-Stick:
/// Gerät finden -> Permission -> Öffnen (inkl. FTDI-Init) -> Vocoder-Reset -> ProductId/Version abfragen.
/// Jeder Schritt wird über NLog geloggt, damit du bei einem Fehlschlag genau siehst,
/// auf welcher Ebene es hakt (USB-Erkennung, Permission, FTDI, oder DV3000-Protokoll).
/// </summary>
public static class AmbeConnectionTest
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private const int FtdiVendorId = 0x0403;

    /// <summary>
    /// Führt den kompletten Verbindungstest aus. Rückgabe true, wenn ProductId
    /// und Version erfolgreich gelesen werden konnten.
    /// </summary>
    public static async Task<bool> RunAsync(Context context)
    {
        Log.Info("=== AMBE3000 Verbindungstest gestartet ===");

        var usbManager = (UsbManager)context.GetSystemService(Context.UsbService)!;

        // Schritt 1: Gerät finden
        var device = usbManager.DeviceList.Values.FirstOrDefault(d => d.VendorId == FtdiVendorId);

        if (device == null)
        {
            Log.Error("Kein FTDI-Gerät (VID 0x0403) gefunden. Ist der Stick eingesteckt " +
                      "und der OTG-Adapter korrekt verbunden? Angeschlossene Geräte:");

            foreach (var d in usbManager.DeviceList.Values)
                Log.Info($"  - {d.DeviceName} VID=0x{d.VendorId:X4} PID=0x{d.ProductId:X4} {d.ProductName}");

            return false;
        }

        Log.Info($"Gerät gefunden: {device.DeviceName}, VID=0x{device.VendorId:X4}, PID=0x{device.ProductId:X4}, " +
                 $"Product={device.ProductName}, Manufacturer={device.ManufacturerName}");

        using var ambe = new Ambe3000Usb(context);

        // NLog an das Log-Event der Ambe3000Usb-Klasse hängen
        ambe.Log += msg => Log.Info($"[Ambe3000Usb] {msg}");

        // Schritt 2: Permission
        Log.Info("Fordere USB-Permission an (falls nötig)...");
        bool granted = await ambe.RequestPermissionAsync(device);

        if (!granted)
        {
            Log.Error("USB-Permission wurde nicht erteilt (Nutzer hat abgelehnt oder Timeout).");
            return false;
        }

        Log.Info("USB-Permission erteilt.");

        // Schritt 3: Öffnen (inkl. FTDI-Init: Reset, Baudrate 460800, 8N1)
        if (!ambe.Open(device, out var failReason))
        {
            Log.Error($"Öffnen fehlgeschlagen: {failReason}");
            return false;
        }

        Log.Info("USB-Verbindung geöffnet, FTDI initialisiert (460800 Baud, 8N1).");

        try
        {
            // Schritt 4: DV3000-Protokoll-Reset (End-to-End-Test der Kommunikation)
            Log.Info("Sende DV3000-Reset und warte auf 'ready'...");
            await ambe.ResetVocoderAsync();
            Log.Info("DV3000 hat 'ready' gemeldet -> Protokoll-Ebene funktioniert.");

            // Schritt 5: ProductId und Version abfragen
            var productId = await ambe.GetProductIdAsync();
            Log.Info($"ProductId: {productId}");

            var version = await ambe.GetVersionAsync();
            Log.Info($"Version: {version}");

            Log.Info("=== AMBE3000 Verbindungstest ERFOLGREICH ===");
            return true;
        }
        catch (TimeoutException ex)
        {
            Log.Error(ex, "Timeout während des Protokoll-Tests. Mögliche Ursachen: " +
                          "falsche Baudrate, FTDI-Statusbyte-Handling fehlerhaft, oder Stick antwortet nicht.");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unerwarteter Fehler während des Verbindungstests.");
            return false;
        }
        finally
        {
            ambe.Close();
            Log.Info("Verbindung geschlossen.");
        }
    }
}
