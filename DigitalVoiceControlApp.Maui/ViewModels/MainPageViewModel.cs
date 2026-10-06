using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoiceControlApp.Maui.Config;
using DigitalVoiceControlApp.Maui.Services;
using NLog;

#if ANDROID
using Android.Content;
using Maui.AmbeSupport;
using Maui.AudioSupport;
#endif

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// ViewModel für die Hauptseite (Gegenstück zum Avalonia-<c>MainViewModel</c>): Mode-Auswahl, Connect/Disconnect,
/// Start/Stop der Clients (<see cref="StartStopDmrAsync"/> entspricht <c>StartStopDmr(bool)</c> der Desktop-App),
/// Statusanzeige. Bisher ist nur DMR angebunden.
/// </summary>
public partial class MainPageViewModel : ObservableObject
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    // ---- DMR-Client (wie in der Avalonia-MainViewModel: _dmrClient1/_dmrClient2, _audioPlayer, _microphoneReader) ----

    DmrClient1? _dmrClient1;
    DmrClient2? _dmrClient2;

#if ANDROID
    Ambe3000UsbController? _ambeController;
    AndroidAudioPlayer? _audioPlayer;
    AndroidMicrophoneReader? _microphoneReader;
#endif

    // Start und Stop werden serialisiert; ein Stop während des Starts beendet den Start sofort nach dem Verbinden.
    readonly SemaphoreSlim _dmrGate = new(1, 1);
    volatile bool _dmrStopRequested;

    /// <summary>true, sobald ein Client angelegt ist (Start läuft oder Client läuft).</summary>
    bool IsClientActive => _dmrClient1 != null || _dmrClient2 != null;

    /// <summary>Ergebnis eines Startversuchs (Success = Client läuft, Cancelled = durch Pause abgebrochen, ohne Meldung).</summary>
    private sealed record StartResult(bool Success, string? Error = null, bool Cancelled = false);

    private string _lastRxText = "";

    /// <summary>Letzte Empfangsmeldung (wer spricht gerade bzw. zuletzt), vorerst als einfache Textzeile.</summary>
    [ObservableProperty]
    private string rxText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionIcon))]
    [NotifyPropertyChangedFor(nameof(ConnectionTooltip))]
    [NotifyPropertyChangedFor(nameof(ConnectionBackgroundColor))]
    [NotifyPropertyChangedFor(nameof(IsModePickerEnabled))]
    [NotifyPropertyChangedFor(nameof(IsSettingsEnabled))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool isServerConnected;

    // ---- Mode-Auswahl --------------------------------------------------------------------

    // Reihenfolge wie in der Desktop-App; Werte, die dort (noch) nicht aufgeführt sind, werden hinten angehängt.
    private static readonly Mode[] PreferredModeOrder = [Mode.Dmr, Mode.Fcs, Mode.Ysf, Mode.Dcs, Mode.Ref, Mode.Xrf, Mode.Nxdn];

    private readonly Dictionary<string, Mode> _modeByName;

    private static string ToName(Mode mode) => mode.ToString().ToUpperInvariant();

    /// <summary>Alle Werte aus <c>Mode.cs</c> als Anzeigenamen (DMR, FCS, YSF, ...).</summary>
    public IReadOnlyList<string> ModeNames { get; }

    /// <summary>Der gewählte Mode. Wird in den Settings gemerkt (<c>Common.LastMode</c>).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedModeName))]
    [NotifyPropertyChangedFor(nameof(ConnectionTooltip))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private Mode selectedMode;

    /// <summary>Anzeigename des gewählten Modes (für den Picker).</summary>
    public string SelectedModeName
    {
        get => ToName(SelectedMode);
        set
        {
            if (value != null && _modeByName.TryGetValue(value, out Mode mode))
                SelectedMode = mode;
        }
    }

    partial void OnSelectedModeChanged(Mode value)
    {
        UserSettings.Instance().Common.LastMode = value;
        UserSettings.Save();
        ConnectCommand.NotifyCanExecuteChanged(); // Connect ist nur für DMR freigegeben
    }

    /// <summary>Während der Verbindung ist der Mode gesperrt (wie in der Desktop-App).</summary>
    public bool IsModePickerEnabled => !IsServerConnected;

    /// <summary>Während der Verbindung sind die Settings gesperrt (wie in der Desktop-App).</summary>
    public bool IsSettingsEnabled => !IsServerConnected;

    /// <summary>Verbinden ist derzeit nur für DMR möglich; Trennen immer, solange verbunden.</summary>
    private bool CanConnect() => IsServerConnected || SelectedMode == Mode.Dmr;

    partial void OnIsServerConnectedChanged(bool value)
    {
        ConnectCommand.NotifyCanExecuteChanged();

        // Während verbunden darf der Bildschirm nicht von selbst ausgehen: Bildschirm aus = App pausiert = Client wird gestoppt.
        MainThread.BeginInvokeOnMainThread(() => DeviceDisplay.Current.KeepScreenOn = value);
    }

    /// <summary>Statuszeile, die auch erklärt, warum Connect gesperrt ist.</summary>
    public string StatusText =>
        IsServerConnected ? $"{ToName(SelectedMode)}: verbunden"
        : SelectedMode == Mode.Dmr ? "DMR: bereit zum Verbinden"
        : $"{ToName(SelectedMode)}: unter Android noch nicht verfügbar (derzeit nur DMR)";

    // TODO: Dateinamen anpassen, sobald die tatsächlichen Icon-Dateinamen im Projekt feststehen
    // (Resources/Images/, Kleinschreibung, z.B. connect_16x.png / disconnect_16x.png).
    public string ConnectionIcon => IsServerConnected ? "disconnect_16x.png" : "connect_16x.png";

    public string ConnectionTooltip =>
        IsServerConnected ? "Disconnect from Server"
        : CanConnect() ? "Connect to Server"
        : "Connect ist derzeit nur für DMR verfügbar";

    public Color ConnectionBackgroundColor => IsServerConnected ? Colors.Red : Colors.Transparent;

    public IAsyncRelayCommand ConnectCommand { get; }

    /// <summary>Wird ausgelöst, wenn die View einen Popup-Hinweis anzeigen soll (Fehler etc.).</summary>
    public event Func<string, string, Task>? ShowAlertRequested;

    public MainPageViewModel()
    {
        // Mode-Liste aus Mode.cs (alle Werte), zuletzt benutzten Mode wiederherstellen.
        // Das Feld wird direkt gesetzt, damit OnSelectedModeChanged nicht schon speichert.
        List<Mode> ordered = PreferredModeOrder.Concat(Enum.GetValues<Mode>().Except(PreferredModeOrder)).ToList();
        _modeByName = ordered.ToDictionary(ToName);
        ModeNames = ordered.Select(ToName).ToList();
        selectedMode = UserSettings.Instance().Common.LastMode;

        ConnectCommand = new AsyncRelayCommand(ConnectDisconnectAsync, CanConnect);

        AppLifecycle.Pausing += OnAppPausing; // Home, Übersicht (Viereck), Bildschirm aus -> Client sofort stoppen
    }

    // ---- Connect / Disconnect -------------------------------------------------------------

    private async Task ConnectDisconnectAsync()
    {
        if (!IsServerConnected)
        {   // C O N N E C T
            StartResult result;
            switch (SelectedMode)
            {
                case Mode.Dmr:
                    result = await StartStopDmrAsync(true);
                    break;
                default:
                    return; // CanConnect() sperrt alle anderen Modes bereits, hier nur als Sicherheitsnetz
            }

            if (result.Cancelled)
            {
                Log.Info("Verbindungsaufbau abgebrochen (App war nicht im Vordergrund).");
                return;
            }
            if (!result.Success)
            {
                await RaiseAlertAsync("Verbindung fehlgeschlagen", result.Error ?? "Unbekannter Fehler");
                return;
            }

            IsServerConnected = true;
            Log.Info($"{ToName(SelectedMode)} verbunden.");
        }
        else
        {   // D I S C O N N E C T
            await DisconnectAsync("Benutzer");
        }
    }

    private async Task DisconnectAsync(string reason)
    {
        switch (SelectedMode)
        {
            case Mode.Dmr:
                await StartStopDmrAsync(false, reason);
                break;
        }

        _lastRxText = "";
        MainThread.BeginInvokeOnMainThread(() =>
        {
            RxText = "";
            IsServerConnected = false;
        });
        Log.Info($"Verbindung getrennt ({reason}).");
    }

    /// <summary>
    /// Die App verliert den Vordergrund (Home, Übersicht/Viereck, Bildschirm aus, anderer Dialog): Client sofort stoppen.
    /// Läuft auf dem UI-Thread und kehrt sofort zurück, das eigentliche Stop() läuft auf dem Thread-Pool.
    /// </summary>
    private void OnAppPausing()
    {
        if (!IsClientActive)
            return; // nichts gestartet (z.B. nur ein System-Dialog während des Verbindungsaufbaus)

        Log.Info("App verliert den Vordergrund: Client wird sofort gestoppt.");
        _ = DisconnectOnPauseAsync();
    }

    private async Task DisconnectOnPauseAsync()
    {
        try
        {
            await DisconnectAsync("App im Hintergrund");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Stopping the client on pause failed.");
        }
    }

    // ---- DMR -------------------------------------------------------------------------------

    /// <summary>
    /// Startet bzw. stoppt den DMR-Client (Gegenstück zu <c>StartStopDmr(bool)</c> in der Avalonia-MainViewModel).
    /// Alle blockierenden Aufrufe (DNS, Chip-Init, Stop) laufen auf dem Thread-Pool, nie auf dem UI-Thread.
    /// </summary>
    /// <param name="arg">true = starten, false = stoppen.</param>
    /// <param name="reason">Grund des Stopps (nur fürs Log).</param>
    private async Task<StartResult> StartStopDmrAsync(bool arg, string reason = "")
    {
        Log.Debug($"arg = {arg}");

        if (!arg)
        {   // S T O P: kann jederzeit kommen, auch während der Start noch läuft
            _dmrStopRequested = true;
            await _dmrGate.WaitAsync();
            try
            {
                await StopDmrCoreAsync(reason);
            }
            finally
            {
                _dmrGate.Release();
            }
            return new StartResult(true);
        }

        // S T A R T
        await _dmrGate.WaitAsync();
        try
        {
            if (IsClientActive)
                return new StartResult(false, "DMR läuft bereits.");

            _dmrStopRequested = false;

            StartResult result;
            try
            {
                result = await StartDmrCoreAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Starting the DMR client failed.");
                result = new StartResult(false, $"{ex.GetType().Name}: {ex.Message}");
            }

            if (!result.Success)
                await StopDmrCoreAsync("Start fehlgeschlagen oder abgebrochen"); // räumt auch teilweise Angelegtes auf

            return result;
        }
        finally
        {
            _dmrGate.Release();
        }
    }

    private async Task<StartResult> StartDmrCoreAsync()
    {
#if ANDROID
        UserSettings us = UserSettings.Instance();

        List<string> errors = us.ValidateForDmr();
        if (errors.Count > 0)
            return new StartResult(false, "Bitte die Settings prüfen:\n" + string.Join("\n", errors));

        Context? context = Platform.CurrentActivity;
        if (context == null)
            return new StartResult(false, "Keine Activity verfügbar.");

        // 1) AMBE-Stick finden und USB-Berechtigung einholen. Der System-Dialog pausiert die App kurz; es läuft noch
        //    kein Client, ein Pause-Ereignis ist an dieser Stelle also harmlos.
        var device = AmbeUsb.FindDevice(context);
        if (device == null)
            return new StartResult(false, "Kein AMBE-Stick (FTDI) gefunden. OTG-Adapter und Stick eingesteckt?");

        if (!await AmbeUsb.RequestPermissionAsync(context, device))
            return new StartResult(false, "USB-Berechtigung wurde nicht erteilt.");

        // Nach dem Dialog muss die App wieder im Vordergrund sein. Sonst (Home/Übersicht gedrückt) nicht verbinden.
        if (!await AppLifecycle.WaitForForegroundAsync(TimeSpan.FromSeconds(2)) || _dmrStopRequested)
            return new StartResult(false, Cancelled: true);

        // 2) Hardware-Objekte (der Chip wird erst von Client.Start() geöffnet)
        _ambeController = new Ambe3000UsbController(context, device);
        _audioPlayer = new AndroidAudioPlayer { GainDb = (float)us.Dmr.RxVolume };
        _microphoneReader = new AndroidMicrophoneReader { GainDb = (float)us.Dmr.MicGain }; // für TX später; wird hier nicht gestartet

        // 3) Client-Konfiguration (wie StartStopDmr in der Avalonia-App)
        DmrClientConfig cfg = new()
        {
            AmbeController = _ambeController,
            Callsign = us.Common.Callsign,
            Password = us.Dmr.Password,
            MyDmrId = us.Dmr.MyDmrId,
            EssId = us.Dmr.Essid,
            Flco = Flco.GROUP,            // nur für TX relevant; die Talkgroup-Auswahl kommt später
            ColorCode = us.Dmr.ColorCode,
            TimeSlot = us.Dmr.TimeSlot,
            RecordAudio = false,
            RecordDmrPackets = false,
            MicrophoneReader = _microphoneReader,
            AudioPlayer = _audioPlayer,
        };

        Action start;
        switch (us.Dmr.Protocol)
        {
            case DmrProtocol.Homebrew:
                cfg.BmServerAddress = us.Dmr.BmServerAddr1;
                cfg.BmServerPort = us.Dmr.BmServerPort1;
                _dmrClient1 = new DmrClient1(cfg) { ExternalDmrDataConsumer = OnDmrData };
                start = _dmrClient1.Start;
                break;
            case DmrProtocol.MmdvmHost:
                (_, string host, int port, _) = DmrHosts.GetHostInfo(us.Dmr.Master);
                if (string.IsNullOrEmpty(host))
                    return new StartResult(false, $"DMR-Master '{us.Dmr.Master}' ist unbekannt.");

                cfg.BmServerAddress = host;
                cfg.BmServerPort = port;
                _dmrClient2 = new DmrClient2(cfg) { ExternalDmrDataConsumer = OnDmrData };
                start = _dmrClient2.Start;
                break;
            default:
                return new StartResult(false, $"Unbekanntes DMR-Protokoll: {us.Dmr.Protocol}");
        }

        // 4) Start blockiert (DNS, Chip-Init, Login senden) -> Thread-Pool
        Log.Info($"Starting DMR ({us.Dmr.Protocol}), callsign={cfg.Callsign}, id={cfg.FullDmrId}, server={cfg.BmServerAddress}:{cfg.BmServerPort}");
        await Task.Run(start);

        // Während des Starts kam ein Stopp (z.B. Home gedrückt): sofort wieder beenden
        if (_dmrStopRequested)
            return new StartResult(false, Cancelled: true);

        return new StartResult(true);
#else
        await Task.CompletedTask;
        return new StartResult(false, "Nur auf Android verfügbar.");
#endif
    }

    private async Task StopDmrCoreAsync(string reason)
    {
        DmrClient1? client1 = _dmrClient1;
        DmrClient2? client2 = _dmrClient2;
        _dmrClient1 = null;
        _dmrClient2 = null;

#if ANDROID
        Ambe3000UsbController? ambe = _ambeController;
        AndroidAudioPlayer? player = _audioPlayer;
        AndroidMicrophoneReader? mic = _microphoneReader;
        _ambeController = null;
        _audioPlayer = null;
        _microphoneReader = null;

        if (client1 == null && client2 == null && ambe == null && player == null && mic == null)
            return; // nichts zu tun
#else
        if (client1 == null && client2 == null)
            return;
#endif

        Log.Info($"Stopping DMR: {reason}");

        // Client.Stop() zuerst: stoppt Timer und TX, sendet RPTCL und schließt den Chip. Danach Audio und USB freigeben.
        await Task.Run(() =>
        {
            TryRun(() => client1?.Stop(), "DmrClient1.Stop()");
            TryRun(() => client2?.Stop(), "DmrClient2.Stop()");
#if ANDROID
            TryRun(() => mic?.Dispose(), "microphone dispose");
            TryRun(() => player?.Dispose(), "audio player dispose");
            TryRun(() => ambe?.Dispose(), "AMBE controller dispose"); // Close() ist idempotent
#endif
        });

        Log.Info("DMR stopped.");
    }

    private static void TryRun(Action action, string what)
    {
        try { action(); }
        catch (Exception ex) { Log.Error(ex, $"{what} failed."); }
    }

    /// <summary>Vom Empfangs-Thread des Clients aufgerufen (nicht der UI-Thread). Bildet die Frames auf eine Textzeile ab.</summary>
    private void OnDmrData(DmrSessionContext s)
    {
        string? text = s.RxStreamState switch
        {
            StreamState.New => $"RX: {s.RxSrcId} → {s.RxDstId} (TS{s.RxTimeSlot})",
            StreamState.End or StreamState.Lost => $"Zuletzt: {s.RxSrcId} → {s.RxDstId}",
            _ => null,
        };

        // Der Client meldet jeden Frame (alle 60 ms): nur bei Änderung die Oberfläche anfassen
        if (text == null || text == _lastRxText)
            return;

        _lastRxText = text;
        MainThread.BeginInvokeOnMainThread(() => RxText = text);
    }

    private Task RaiseAlertAsync(string title, string message) =>
        ShowAlertRequested?.Invoke(title, message) ?? Task.CompletedTask;
}
