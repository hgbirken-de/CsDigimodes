using DigitalVoiceControlApp.Maui.Config;
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

    /// <summary>Slider losgelassen: Lautstärke in die Einstellungsdatei schreiben (beim Ziehen nur im Speicher).</summary>
    private void OnRxVolumeDragCompleted(object? sender, EventArgs e) => UserSettings.Save();

    /// <summary>Slider losgelassen: Mikrofon-Gain in die Einstellungsdatei schreiben.</summary>
    private void OnMicGainDragCompleted(object? sender, EventArgs e) => UserSettings.Save();

    private async void OnMenuButtonClicked(object sender, EventArgs e)
    {
        // Ein Action-Sheet kennt keine deaktivierten Einträge: Während der Verbindung bleibt "Settings" sichtbar,
        // ist aber als gesperrt gekennzeichnet und erklärt beim Antippen, warum.
        string settingsItem = ViewModel.IsSettingsEnabled ? "Settings" : "Settings (locked)";
        string talkgroupsItem = ViewModel.IsSettingsEnabled ? "Talkgroups…" : "Talkgroups (locked)";
        string logItem = ViewModel.IsSettingsEnabled ? "Log management…" : "Log management (locked)";
        string action = await DisplayActionSheet("Menu", "Cancel", null, settingsItem, talkgroupsItem, logItem, "Help", "AMBE-Test", "Audio-Test", "Exit");

        // async void: eine unbehandelte Exception würde hier die ganze App beenden -> abfangen und anzeigen
        try
        {
            switch (action)
            {
                case "Settings":
                    await Navigation.PushAsync(new SettingsPage());
                    break;
                case "Settings (locked)":
                    await DisplayAlert("Settings", "The settings are locked while connected. Please disconnect first.", "OK");
                    break;
                case "Talkgroups…":
                    await Navigation.PushAsync(new TalkgroupEditorPage(ViewModel.ReloadTalkgroups));
                    break;
                case "Talkgroups (locked)":
                    await DisplayAlert("Talkgroups", "The list is locked while connected. Please disconnect first.", "OK");
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
                case "Log management…":
                    await Navigation.PushAsync(new LogManagementPage());
                    break;
                case "Log management (locked)":
                    await DisplayAlert("Log management", "Log management is locked while connected. Please disconnect first.", "OK");
                    break;
                case "Exit":
                    // TODO: App beenden
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Menu action '{action}' failed.");
            await DisplayAlert("Error", $"{ex.GetType().Name}: {ex.Message}", "OK");
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
            await DisplayAlert("AMBE-Test", "No activity available.", "OK");
            return;
        }

        // Erkennungscheck: alle USB-Geräte auflisten
        var usbManager = (UsbManager)activity.GetSystemService(Context.UsbService)!;
        var devices = usbManager.DeviceList?.Values.ToList() ?? [];

        Log.Info($"USB search: {devices.Count} device(s) found.");

        var deviceLines = new List<string>();
        foreach (var device in devices)
        {
            bool isAmbe = device.VendorId == AmbeUsb.FtdiVendorId;
            string marker = isAmbe ? "✓ AMBE stick (FTDI)" : "?";
            string line = $"{marker}  {device.ProductName ?? device.DeviceName}  " +
                          $"VID=0x{device.VendorId:X4} PID=0x{device.ProductId:X4}";
            deviceLines.Add(line);
            Log.Info($"  {line}");
        }

        string deviceSection = devices.Count == 0
            ? "No USB devices found.\nAre the OTG adapter and the stick plugged in?"
            : string.Join("\n", deviceLines);

        // Der Test greift auf den Stick zu, den der DMR-Client benutzt: nur im getrennten Zustand
        if (ViewModel.IsServerConnected)
        {
            await DisplayAlert("AMBE-Test", $"{deviceSection}\n\nThe stick is currently used by the DMR client. Please disconnect first.", "OK");
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
        await DisplayAlert("AMBE-Test", "Available on Android only.", "OK");
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
            await DisplayAlert("Audio-Test", "Microphone and speaker are currently used by the DMR client. Please disconnect first.", "OK");
            return;
        }

        try
        {
            // Wirft eine PermissionException, wenn RECORD_AUDIO nicht in AndroidManifest.xml deklariert ist
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Audio-Test", "Microphone permission was not granted.", "OK");
                return;
            }

            await DisplayAlert("Audio-Test", "After you tap OK, 3 seconds are recorded. Please speak. The recording is then played back.", "OK");

            string report = await Task.Run(() => AudioSelfTest.RunAsync(3));
            Log.Info(report);
            await DisplayAlert("Audio-Test", report, "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Audio test failed.");
            await DisplayAlert("Audio-Test", $"Error: {ex.GetType().Name}: {ex.Message}", "OK");
        }
#else
        await DisplayAlert("Audio-Test", "Available on Android only.", "OK");
#endif
    }
}
