using DigitalVoiceControlApp.Maui.Services.Ambe;
using DigitalVoiceControlApp.Maui.ViewModels;
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

    private MainPageViewModel ViewModel => (MainPageViewModel)BindingContext;

    public MainPage()
    {
        InitializeComponent();

        BindingContext = new MainPageViewModel();
        ViewModel.ShowAlertRequested += async (title, message) => await DisplayAlert(title, message, "OK");
    }

    private async void OnMenuButtonClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Menu", "Cancel", null, "Settings", "Help", "AMBE-Test", "Export Log", "Exit");

        switch (action)
        {
            case "Settings":
                // TODO: Settings-Dialog öffnen
                break;
            case "Help":
                // TODO: Help/About anzeigen
                break;
            case "AMBE-Test":
                await RunAmbeTestAsync();
                break;
            case "Export Log":
                await ExportLogAsync();
                break;
            case "Exit":
                // TODO: App beenden
                break;
        }
    }

    /// <summary>
    /// Kombinierter AMBE-Test: listet zuerst alle USB-Geräte (Erkennungscheck), führt danach,
    /// falls ein AMBE-Stick gefunden wurde, den vollen Verbindungstest durch. Falls bereits
    /// über den Connect-Button (ViewModel.AmbeDevice) eine Verbindung offen ist, wird
    /// stattdessen nur auf der bestehenden Verbindung ProductId/Version erneut abgefragt
    /// (keine zweite Verbindung, das würde an ClaimInterface scheitern).
    /// </summary>
    private async Task RunAmbeTestAsync()
    {
#if ANDROID
        if (ViewModel.AmbeDevice != null)
        {
            try
            {
                var pid = await ViewModel.AmbeDevice.GetProductIdAsync();
                var ver = await ViewModel.AmbeDevice.GetVersionAsync();
                await DisplayAlert("AMBE-Test", $"Bereits verbunden (Connect-Button).\nProductId: {pid}\nVersion: {ver}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("AMBE-Test", $"Fehler auf bestehender Verbindung: {ex.Message}", "OK");
            }
            return;
        }

        var activity = Platform.CurrentActivity;

        if (activity == null)
        {
            await DisplayAlert("AMBE-Test", "Keine Activity verfügbar.", "OK");
            return;
        }

        var usbManager = (UsbManager)activity.GetSystemService(Context.UsbService)!;
        var devices = usbManager.DeviceList.Values.ToList();

        Log.Info($"USB-Suche: {devices.Count} Gerät(e) gefunden.");

        var deviceLines = new List<string>();
        foreach (var device in devices)
        {
            bool isAmbe = device.VendorId == FtdiVendorId;
            string marker = isAmbe ? "✓ AMBE-Stick (FTDI)" : "?";
            string line = $"{marker}  {device.ProductName ?? device.DeviceName}  " +
                          $"VID=0x{device.VendorId:X4} PID=0x{device.ProductId:X4}";
            deviceLines.Add(line);
            Log.Info($"  {line}");
        }

        string deviceSection = devices.Count == 0
            ? "Keine USB-Geräte gefunden.\nOTG-Adapter und Stick eingesteckt?"
            : string.Join("\n", deviceLines);

        if (!devices.Any(d => d.VendorId == FtdiVendorId))
        {
            await DisplayAlert("AMBE-Test", deviceSection, "OK");
            return;
        }

        var result = await Ambe3000Usb.TestConnectionAsync(activity);

        string resultSection = result.Success
            ? $"Verbindung erfolgreich!\nProductId: {result.ProductId}\nVersion: {result.Version}"
            : $"Fehlgeschlagen: {result.ErrorMessage}\n(Details siehe Log)";

        await DisplayAlert("AMBE-Test", $"{deviceSection}\n\n{resultSection}", "OK");
#else
        await DisplayAlert("AMBE-Test", "Nur auf Android verfügbar.", "OK");
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
