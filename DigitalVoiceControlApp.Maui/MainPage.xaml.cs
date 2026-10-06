using DigitalVoiceControlApp.Maui.ViewModels;
using Maui.AmbeSupport;
using Maui.AudioSupport;
using NLog;

#if ANDROID
using Android.Content;
using Android.Hardware.Usb;
#endif

namespace DigitalVoiceControlApp.Maui;

public partial class MainPage : ContentPage
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private MainPageViewModel ViewModel => (MainPageViewModel)BindingContext;

    public MainPage()
    {
        InitializeComponent();

        BindingContext = new MainPageViewModel();
        ViewModel.ShowAlertRequested += async (title, message) => await DisplayAlert(title, message, "OK");
    }

    private async void OnMenuButtonClicked(object sender, EventArgs e)
    {
        // Ein Action-Sheet kennt keine deaktivierten Einträge: Während der Verbindung bleibt "Settings" sichtbar,
        // ist aber als gesperrt gekennzeichnet und erklärt beim Antippen, warum.
        string settingsItem = ViewModel.IsSettingsEnabled ? "Settings" : "Settings (gesperrt)";
        string action = await DisplayActionSheet("Menu", "Cancel", null, settingsItem, "Help", "AMBE-Test", "Audio-Test", "Export Log", "Exit");

        // async void: eine unbehandelte Exception würde hier die ganze App beenden -> abfangen und anzeigen
        try
        {
            switch (action)
            {
                case "Settings":
                    await Navigation.PushAsync(new SettingsPage());
                    break;
                case "Settings (gesperrt)":
                    await DisplayAlert("Settings", "Während der Verbindung sind die Settings gesperrt. Bitte zuerst trennen.", "OK");
                    break;
                case "Help":
                    // TODO: Help/About anzeigen
                    break;
                case "AMBE-Test":
                    await RunAmbeTestAsync();
                    break;
                case "Audio-Test":
                    await RunAudioTestAsync();
                    break;
                case "Export Log":
                    await ExportLogAsync();
                    break;
                case "Exit":
                    // TODO: App beenden
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Menu action '{action}' failed.");
            await DisplayAlert("Fehler", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Kombinierter AMBE-Test: listet zuerst alle USB-Geräte (Erkennungscheck), baut danach (falls ein
    /// AMBE-Stick gefunden wurde) die Verbindung auf bzw. nutzt die vom Connect-Button bereits offene
    /// (keine zweite Verbindung, das würde an ClaimInterface scheitern) und führt den Selbsttest
    /// <see cref="AmbeSelfTest"/> aus: ProductId/Version, DMR-Init, Encode, Decode und Zeit pro Block.
    /// </summary>
    private async Task RunAmbeTestAsync()
    {
#if ANDROID
        var activity = Platform.CurrentActivity;
        if (activity == null)
        {
            await DisplayAlert("AMBE-Test", "Keine Activity verfügbar.", "OK");
            return;
        }

        // Erkennungscheck: alle USB-Geräte auflisten
        var usbManager = (UsbManager)activity.GetSystemService(Context.UsbService)!;
        var devices = usbManager.DeviceList?.Values.ToList() ?? [];

        Log.Info($"USB-Suche: {devices.Count} Gerät(e) gefunden.");

        var deviceLines = new List<string>();
        foreach (var device in devices)
        {
            bool isAmbe = device.VendorId == AmbeUsb.FtdiVendorId;
            string marker = isAmbe ? "✓ AMBE-Stick (FTDI)" : "?";
            string line = $"{marker}  {device.ProductName ?? device.DeviceName}  " +
                          $"VID=0x{device.VendorId:X4} PID=0x{device.ProductId:X4}";
            deviceLines.Add(line);
            Log.Info($"  {line}");
        }

        string deviceSection = devices.Count == 0
            ? "Keine USB-Geräte gefunden.\nOTG-Adapter und Stick eingesteckt?"
            : string.Join("\n", deviceLines);

        // Der Test greift auf den Stick zu, den der DMR-Client benutzt: nur im getrennten Zustand
        if (ViewModel.IsServerConnected)
        {
            await DisplayAlert("AMBE-Test", $"{deviceSection}\n\nDer Stick wird gerade vom DMR-Client benutzt. Bitte zuerst trennen.", "OK");
            return;
        }

        if (!devices.Any(d => d.VendorId == AmbeUsb.FtdiVendorId))
        {
            await DisplayAlert("AMBE-Test", deviceSection, "OK");
            return;
        }

        var (ctrl, error) = await AmbeUsb.ConnectAsync(activity);
        if (ctrl == null)
        {
            await DisplayAlert("AMBE-Test", $"{deviceSection}\n\n{error ?? "Verbindung fehlgeschlagen."}", "OK");
            return;
        }

        try
        {
            // Blockierende Chip-Zugriffe: nicht auf dem UI-Thread
            string report = await Task.Run(() => AmbeSelfTest.Run(ctrl));
            Log.Info(report);
            await DisplayAlert("AMBE-Test", $"{deviceSection}\n\n{report}", "OK");
        }
        finally
        {
            ctrl.Close();
        }
#else
        await DisplayAlert("AMBE-Test", "Nur auf Android verfügbar.", "OK");
#endif
    }

    /// <summary>
    /// Audio-Test: holt die Mikrofon-Berechtigung, nimmt 3 Sekunden auf (Takt und Pegel werden ausgewertet)
    /// und spielt die Aufnahme danach über den Lautsprecher ab.
    /// </summary>
    private async Task RunAudioTestAsync()
    {
#if ANDROID
        if (ViewModel.IsServerConnected)
        {
            await DisplayAlert("Audio-Test", "Mikrofon und Lautsprecher werden gerade vom DMR-Client benutzt. Bitte zuerst trennen.", "OK");
            return;
        }

        try
        {
            // Wirft eine PermissionException, wenn RECORD_AUDIO nicht in AndroidManifest.xml deklariert ist
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Audio-Test", "Mikrofon-Berechtigung wurde nicht erteilt.", "OK");
                return;
            }

            await DisplayAlert("Audio-Test", "Nach dem Tippen auf OK werden 3 Sekunden aufgenommen. Bitte sprechen. Danach wird die Aufnahme abgespielt.", "OK");

            string report = await Task.Run(() => AudioSelfTest.RunAsync(3));
            Log.Info(report);
            await DisplayAlert("Audio-Test", report, "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Audio test failed.");
            await DisplayAlert("Audio-Test", $"Fehler: {ex.GetType().Name}: {ex.Message}", "OK");
        }
#else
        await DisplayAlert("Audio-Test", "Nur auf Android verfügbar.", "OK");
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
