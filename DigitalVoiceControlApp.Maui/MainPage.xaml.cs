using DigitalVoiceControlApp.Maui.Services.Ambe;
using NLog;

#if ANDROID
using Android.Content;
using Android.Hardware.Usb;
#endif

namespace DigitalVoiceControlApp.Maui;

public partial class MainPage : ContentPage
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private const int FtdiVendorId = 0x0403;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnFindStickClicked(object sender, EventArgs e)
    {
#if ANDROID
        var activity = Platform.CurrentActivity;

        if (activity == null)
        {
            UsbResultLabel.Text = "Keine Activity verfügbar.";
            return;
        }

        var usbManager = (UsbManager)activity.GetSystemService(Context.UsbService)!;
        var devices = usbManager.DeviceList.Values.ToList();

        Log.Info($"USB-Suche: {devices.Count} Gerät(e) gefunden.");

        if (devices.Count == 0)
        {
            UsbResultLabel.Text = "Keine USB-Geräte gefunden.\nOTG-Adapter und Stick eingesteckt?";
            return;
        }

        var lines = new List<string>();

        foreach (var device in devices)
        {
            bool isAmbe = device.VendorId == FtdiVendorId;
            string marker = isAmbe ? "✓ AMBE-Stick (FTDI)" : "?";

            string line = $"{marker}  {device.ProductName ?? device.DeviceName}  " +
                          $"VID=0x{device.VendorId:X4} PID=0x{device.ProductId:X4}";

            lines.Add(line);
            Log.Info($"  {line}");
        }

        UsbResultLabel.Text = string.Join("\n", lines);
#else
        UsbResultLabel.Text = "Nur auf Android verfügbar.";
#endif
    }

    private async void OnMenuButtonClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Menu", "Cancel", null, "Settings", "Help", "Test AMBE", "Export Log", "Exit");

        switch (action)
        {
            case "Settings":
                // TODO: Settings-Dialog öffnen
                break;
            case "Help":
                // TODO: Help/About anzeigen
                break;
            case "Test AMBE":
                await TestAmbeAsync();
                break;
            case "Export Log":
                await ExportLogAsync();
                break;
            case "Exit":
                // TODO: App beenden
                break;
        }
    }

    private async Task TestAmbeAsync()
    {
#if ANDROID
        var activity = Platform.CurrentActivity;

        if (activity == null)
        {
            await DisplayAlert("AMBE Test", "Keine Activity verfügbar.", "OK");
            return;
        }

        bool ok = await AmbeConnectionTest.RunAsync(activity);

        await DisplayAlert(
            "AMBE Test",
            ok ? "Verbindung erfolgreich – ProductId und Version gelesen. Details siehe Log."
               : "Fehlgeschlagen. Details siehe Log (Export Log).",
            "OK");
#else
        await DisplayAlert("AMBE Test", "Nur auf Android verfügbar.", "OK");
#endif
    }

    private async Task ExportLogAsync()
    {
        var logDir = Path.Combine(FileSystem.AppDataDirectory, "logs");

        if (!Directory.Exists(logDir))
        {
            await DisplayAlert("Export Log", "Keine Logdateien vorhanden.", "OK");
            return;
        }

        var latestLog = Directory.GetFiles(logDir, "*.log")
            .OrderByDescending(File.GetLastWriteTime)
            .FirstOrDefault();

        if (latestLog == null)
        {
            await DisplayAlert("Export Log", "Keine Logdateien vorhanden.", "OK");
            return;
        }

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Log-Datei teilen",
            File = new ShareFile(latestLog)
        });
    }
}
