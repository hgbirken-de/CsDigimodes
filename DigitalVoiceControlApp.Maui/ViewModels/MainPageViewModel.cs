using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoiceControlApp.Maui.Config;
using DigitalVoiceControlApp.Maui.Services;
using NLog;
using System.Collections.Concurrent;

#if ANDROID
using Android.Content;
using Maui.AmbeSupport;
using Maui.AudioSupport;
#endif

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// ViewModel für die Hauptseite (Gegenstück zum Avalonia-<c>MainViewModel</c>): Mode-Auswahl, Connect/Disconnect,
/// Start/Stop der Clients (<see cref="StartStopDmr"/> entspricht <c>StartStopDmr(bool)</c> der Desktop-App), RX-/MIC-Gain,
/// PTT. Bisher ist nur DMR angebunden.
/// <para>
/// Ohne async/await: Alles, was blockiert (USB-Dialog, DNS, Chip-Init, Stop), läuft synchron auf einem eigenen Thread, nie auf
/// dem UI-Thread. Änderungen an der Oberfläche werden mit <c>MainThread.BeginInvokeOnMainThread</c> übergeben. Die
/// MAUI-Dialoge (DisplayAlert &amp; Co.) sind nur async verfügbar und bleiben deshalb in der View (<c>ShowAlertRequested</c>).
/// </para>
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

    // Start und Stop laufen nacheinander; ein Stop während des Starts beendet den Start sofort nach dem Verbinden.
    readonly object _dmrLock = new();
    volatile bool _dmrStopRequested;

    /// <summary>true, sobald ein Client angelegt ist (Start läuft oder Client läuft).</summary>
    bool IsClientActive => _dmrClient1 != null || _dmrClient2 != null;

    /// <summary>Ergebnis eines Startversuchs (Success = Client läuft, Cancelled = durch Pause abgebrochen, ohne Meldung).</summary>
    private sealed record StartResult(bool Success, string? Error = null, bool Cancelled = false);

    /// <summary>Die DMR-Ansicht (Rufzeichen, Quelle, Ziel, Last Heard). Der Client meldet seine Daten dorthin.</summary>
    public DmrViewModel Dmr { get; } = new();

    /// <summary>true, solange ein Connect/Disconnect auf seinem Thread läuft (der Knopf ist dann gesperrt).</summary>
    [ObservableProperty]
    private bool isBusy;

    partial void OnIsBusyChanged(bool value) => ConnectCommand.NotifyCanExecuteChanged();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionIcon))]
    [NotifyPropertyChangedFor(nameof(ConnectionTooltip))]
    [NotifyPropertyChangedFor(nameof(ConnectionBackgroundColor))]
    [NotifyPropertyChangedFor(nameof(IsModePickerEnabled))]
    [NotifyPropertyChangedFor(nameof(IsSettingsEnabled))]
    [NotifyPropertyChangedFor(nameof(IsPttEnabled))]
    [NotifyPropertyChangedFor(nameof(PttBackgroundColor))]
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
    [NotifyPropertyChangedFor(nameof(IsDmrViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsPttEnabled))]
    [NotifyPropertyChangedFor(nameof(PttBackgroundColor))]
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

        RxVolume = GetRxVolume(value); // die für diesen Mode gemerkten Werte
        MicGain = GetMicGain(value);

        UpdateLinkTargets(value);
    }

    /// <summary>Die DMR-Ansicht wird nur im DMR-Mode gezeigt.</summary>
    public bool IsDmrViewVisible => SelectedMode == Mode.Dmr;

    /// <summary>Während der Verbindung ist der Mode gesperrt (wie in der Desktop-App).</summary>
    public bool IsModePickerEnabled => !IsServerConnected;

    /// <summary>Während der Verbindung sind die Settings gesperrt (wie in der Desktop-App).</summary>
    public bool IsSettingsEnabled => !IsServerConnected;

    /// <summary>Verbinden ist derzeit nur für DMR möglich; Trennen immer, solange verbunden. Während Connect/Disconnect läuft: gesperrt.</summary>
    private bool CanConnect() => !IsBusy && (IsServerConnected || SelectedMode == Mode.Dmr);

    partial void OnIsServerConnectedChanged(bool value)
    {
        ConnectCommand.NotifyCanExecuteChanged();

        // Während verbunden darf der Bildschirm nicht von selbst ausgehen: Bildschirm aus = App pausiert = Client wird gestoppt.
        DeviceDisplay.Current.KeepScreenOn = value; // läuft immer auf dem UI-Thread (siehe SetConnectedUi)
    }

    /// <summary>Statuszeile, die auch erklärt, warum Connect gesperrt ist.</summary>
    public string StatusText =>
        IsServerConnected ? $"{ToName(SelectedMode)}: connected"
        : SelectedMode == Mode.Dmr ? "DMR: ready to connect"
        : $"{ToName(SelectedMode)}: not yet available on Android (DMR only for now)";

    // TODO: Dateinamen anpassen, sobald die tatsächlichen Icon-Dateinamen im Projekt feststehen
    // (Resources/Images/, Kleinschreibung, z.B. connect_16x.png / disconnect_16x.png).
    public string ConnectionIcon => IsServerConnected ? "disconnect_16x.png" : "connect_16x.png";

    public string ConnectionTooltip =>
        IsServerConnected ? "Disconnect from Server"
        : SelectedMode == Mode.Dmr ? "Connect to Server"
        : "Connect is currently available for DMR only";

    public Color ConnectionBackgroundColor => IsServerConnected ? Colors.Red : Colors.Transparent;

    public IRelayCommand ConnectCommand { get; }

    /// <summary>Wird ausgelöst, wenn die View einen Popup-Hinweis anzeigen soll (Fehler etc.). Die View zeigt ihn async an.</summary>
    public event Func<string, string, Task>? ShowAlertRequested;

    // ---- Talkgroup-/Reflektor-Auswahl ------------------------------------------------------------

    // Anzeigename -> Ziel (nur DMR; die anderen Modes folgen)
    private readonly Dictionary<string, (int DmrId, Flco Flco)> _dmrLinkTargets = [];

    /// <summary>Die Einträge des Pickers (im DMR-Mode die Talkgroups aus <c>DmrTalkGroups.csv</c>).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLinkTargetPickerEnabled))]
    private IReadOnlyList<string> linkTargetNames = [];

    /// <summary>Überschrift des Pickers, solange nichts gewählt ist.</summary>
    [ObservableProperty]
    private string linkTargetTitle = "Talkgroup";

    public bool IsLinkTargetPickerEnabled => LinkTargetNames.Count > 0 && !IsPttActive; // beim Senden gesperrt

    private string _selectedLinkTargetName = "";

    public string SelectedLinkTargetName
    {
        get => _selectedLinkTargetName;
        set
        {
            // Der Picker meldet null/leer, wenn die Liste neu gesetzt wird: ignorieren
            if (string.IsNullOrEmpty(value) || value == _selectedLinkTargetName)
                return;

            SetProperty(ref _selectedLinkTargetName, value);

            if (SelectedMode == Mode.Dmr && _dmrLinkTargets.TryGetValue(value, out var target))
            {
                UserSettings.Instance().Dmr.LastTgInUse = target.DmrId; // beim nächsten Start wieder vorgewählt
                UserSettings.Save();
                // TODO: beim Senden SetTxDst(target.DmrId, target.Flco) am Client
            }
        }
    }

    /// <summary>
    /// Lädt <c>DmrTalkGroups.csv</c> neu (nach dem Bearbeiten im Editor) und baut die Auswahlliste neu auf.
    /// Aufruf auf dem UI-Thread.
    /// </summary>
    public void ReloadTalkgroups()
    {
        DataFiles.LoadDmrTalkgroups();
        UpdateLinkTargets(SelectedMode);
    }

    /// <summary>Füllt die Auswahlliste passend zum Mode und wählt den zuletzt benutzten Eintrag vor.</summary>
    private void UpdateLinkTargets(Mode mode)
    {
        _dmrLinkTargets.Clear();
        List<string> names = [];
        string selected = "";

        if (mode == Mode.Dmr)
        {
            foreach (var kvp in DmrTalkgroups.All)
            {
                string name = $"{kvp.Key} {kvp.Value.Name} ({kvp.Value.CallType})";
                _dmrLinkTargets[name] = (kvp.Key, kvp.Value.CallType == 'G' ? Flco.GROUP : Flco.USER_USER);
                names.Add(name);
            }

            int last = UserSettings.Instance().Dmr.LastTgInUse;
            selected = names.FirstOrDefault(n => _dmrLinkTargets[n].DmrId == last) ?? "";
        }

        LinkTargetTitle = mode == Mode.Dmr ? "Talkgroup" : "Reflector";
        _selectedLinkTargetName = selected; // Feld direkt: nichts speichern
        LinkTargetNames = names;            // der Picker setzt dabei seine Auswahl zurück ...

        // ... deshalb die gemerkte Auswahl anschließend erneut an den Picker melden
        MainThread.BeginInvokeOnMainThread(() => OnPropertyChanged(nameof(SelectedLinkTargetName)));
    }

    // ---- RX-Lautstärke und Mikrofon-Gain (dB, Slider -40..+30 wie in der Desktop-App, pro Mode gemerkt) ----

    [ObservableProperty]
    private double rxVolume;

    [ObservableProperty]
    private double micGain;

    private static double GetRxVolume(Mode mode)
    {
        UserSettings us = UserSettings.Instance();
        return mode switch
        {
            Mode.Dcs => us.Dcs.RxVolume,
            Mode.Dmr => us.Dmr.RxVolume,
            Mode.Fcs => us.Fcs.RxVolume,
            Mode.Nxdn => us.Nxdn.RxVolume,
            Mode.Ref => us.Ref.RxVolume,
            Mode.Xrf => us.Xrf.RxVolume,
            Mode.Ysf => us.Ysf.RxVolume,
            _ => 0,
        };
    }

    private static void SetRxVolume(Mode mode, double value)
    {
        UserSettings us = UserSettings.Instance();
        switch (mode)
        {
            case Mode.Dcs: us.Dcs.RxVolume = value; break;
            case Mode.Dmr: us.Dmr.RxVolume = value; break;
            case Mode.Fcs: us.Fcs.RxVolume = value; break;
            case Mode.Nxdn: us.Nxdn.RxVolume = value; break;
            case Mode.Ref: us.Ref.RxVolume = value; break;
            case Mode.Xrf: us.Xrf.RxVolume = value; break;
            case Mode.Ysf: us.Ysf.RxVolume = value; break;
        }
    }

    private static double GetMicGain(Mode mode)
    {
        UserSettings us = UserSettings.Instance();
        return mode switch
        {
            Mode.Dcs => us.Dcs.MicGain,
            Mode.Dmr => us.Dmr.MicGain,
            Mode.Fcs => us.Fcs.MicGain,
            Mode.Nxdn => us.Nxdn.MicGain,
            Mode.Ref => us.Ref.MicGain,
            Mode.Xrf => us.Xrf.MicGain,
            Mode.Ysf => us.Ysf.MicGain,
            _ => 0,
        };
    }

    private static void SetMicGain(Mode mode, double value)
    {
        UserSettings us = UserSettings.Instance();
        switch (mode)
        {
            case Mode.Dcs: us.Dcs.MicGain = value; break;
            case Mode.Dmr: us.Dmr.MicGain = value; break;
            case Mode.Fcs: us.Fcs.MicGain = value; break;
            case Mode.Nxdn: us.Nxdn.MicGain = value; break;
            case Mode.Ref: us.Ref.MicGain = value; break;
            case Mode.Xrf: us.Xrf.MicGain = value; break;
            case Mode.Ysf: us.Ysf.MicGain = value; break;
        }
    }

    // Gespeichert wird beim Loslassen des Sliders (siehe MainPage) und beim Pausieren der App.
    partial void OnRxVolumeChanged(double value)
    {
        SetRxVolume(SelectedMode, value);

#if ANDROID
        if (_audioPlayer is { } player)
            player.GainDb = (float)value; // wirkt sofort auf die laufende Wiedergabe
#endif
    }

    partial void OnMicGainChanged(double value)
    {
        SetMicGain(SelectedMode, value);

#if ANDROID
        if (_microphoneReader is { } mic)
            mic.GainDb = (float)value;
#endif
    }

    // ---- PTT: einmal tippen = senden, nochmal tippen = Ende (wie in der Desktop-App) -----------------------

    /// <summary>true, wenn das Mikrofon erlaubt ist (wird beim Connect abgefragt). Ohne Berechtigung kann man nur hören.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPttEnabled))]
    [NotifyPropertyChangedFor(nameof(PttBackgroundColor))]
    private bool isMicrophoneAllowed;

    /// <summary>PTT ist nur im verbundenen DMR-Mode mit Mikrofon-Berechtigung bedienbar.</summary>
    public bool IsPttEnabled => IsServerConnected && SelectedMode == Mode.Dmr && IsMicrophoneAllowed;

    /// <summary>true, solange gesendet wird (PTT ausgelöst).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PttBackgroundColor))]
    [NotifyPropertyChangedFor(nameof(PttText))]
    [NotifyPropertyChangedFor(nameof(IsLinkTargetPickerEnabled))]
    private bool isPttActive;

    private static readonly Color PttIdleColor = Color.FromArgb("#1565C0");   // bereit (blau)
    private static readonly Color PttActiveColor = Color.FromArgb("#B00020"); // sendet (rot)

    /// <summary>Grau = gesperrt, blau = bereit, rot = PTT ausgelöst.</summary>
    public Color PttBackgroundColor => !IsPttEnabled ? Colors.Gray : IsPttActive ? PttActiveColor : PttIdleColor;

    public string PttText => IsPttActive ? "TX - tap to stop" : "PTT - tap to talk";

    public IRelayCommand TogglePttCommand { get; }

    // Das Starten/Stoppen des Sendens blockiert (Mikrofon-Start, Header-Frame) und läuft deshalb nicht auf dem UI-Thread,
    // sondern auf einem eigenen Thread. Die Warteschlange hält Einschalten und Ausschalten in der richtigen Reihenfolge.
    private sealed record PttCommand(bool On, int DmrId, Flco Flco);

    private readonly BlockingCollection<PttCommand> _pttQueue = new();

    // Sicherheitsnetz (z.B. Auto): Spätestens nach dieser Zeit wird das Senden beendet
    private const int TxTimeLimitSeconds = 180;
    private System.Threading.Timer? _txTimeoutTimer;

    /// <summary>Der PTT-Knopf (UI-Thread): schaltet das Senden um.</summary>
    private void TogglePtt()
    {
        bool on = !IsPttActive;
        int dmrId = 0;
        Flco flco = Flco.GROUP;

        if (on)
        {
            if (!IsPttEnabled)
                return;

            if (!_dmrLinkTargets.TryGetValue(SelectedLinkTargetName, out var target))
            {
                ShowAlert("PTT", "Please select a talkgroup first.");
                return;
            }

            dmrId = target.DmrId;
            flco = target.Flco;
        }

        IsPttActive = on; // sofort sichtbar (rot/blau)
        if (on)
            StartTxTimeout();
        else
            StopTxTimeout();

        Log.Info(on ? $"PTT on, destination {dmrId} ({flco})" : "PTT off");
        _pttQueue.Add(new PttCommand(on, dmrId, flco));
    }

    private void StartTxTimeout()
    {
        _txTimeoutTimer?.Dispose();
        _txTimeoutTimer = new System.Threading.Timer(_ => OnTxTimeout(), null,
            TimeSpan.FromSeconds(TxTimeLimitSeconds), System.Threading.Timeout.InfiniteTimeSpan);
    }

    private void StopTxTimeout()
    {
        _txTimeoutTimer?.Dispose();
        _txTimeoutTimer = null;
    }

    private void OnTxTimeout()
    {
        Log.Warn($"TX time limit ({TxTimeLimitSeconds} s) reached: stopping the transmission.");
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!IsPttActive)
                return;

            IsPttActive = false;
            StopTxTimeout();
            _pttQueue.Add(new PttCommand(false, 0, Flco.GROUP));
            ShowAlert("Transmission stopped", $"The time limit of {TxTimeLimitSeconds / 60} minutes was reached.");
        });
    }

    // Läuft auf dem PTT-Thread
    private void PttLoop()
    {
        foreach (PttCommand command in _pttQueue.GetConsumingEnumerable())
        {
            try
            {
                ApplyPtt(command);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PTT command failed.");
            }
        }
    }

    /// <summary>
    /// Schaltet das Senden am laufenden Client ein/aus (Gegenstück zu <c>PttActive</c> in der Avalonia-MainViewModel:
    /// <c>SetTxDst</c> und <c>StartStopTransmit</c>). Gleiche Sperre wie Start/Stop, damit der Client währenddessen
    /// nicht gestoppt wird.
    /// </summary>
    private void ApplyPtt(PttCommand command)
    {
        lock (_dmrLock)
        {
            DmrClient1? client1 = _dmrClient1;
            DmrClient2? client2 = _dmrClient2;

            if (client1 == null && client2 == null)
            {
                if (command.On)
                    SetPttUi(false); // der Client wurde inzwischen gestoppt (Pause, Trennen)
                return;
            }

            try
            {
                if (command.On)
                {
                    client1?.SetTxDst(command.DmrId, command.Flco);
                    client2?.SetTxDst(command.DmrId, command.Flco);
                }

                client1?.StartStopTransmit(command.On);
                client2?.StartStopTransmit(command.On);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"StartStopTransmit({command.On}) failed.");

                // Teilweise Gestartetes aufräumen (z.B. wenn das Mikrofon nicht starten wollte)
                if (command.On)
                {
                    TryRun(() => client1?.StartStopTransmit(false), "DmrClient1.StartStopTransmit(false)");
                    TryRun(() => client2?.StartStopTransmit(false), "DmrClient2.StartStopTransmit(false)");
                }

                SetPttUi(false);
                ShowAlert("Transmit failed", $"{ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    /// <summary>Setzt den PTT-Zustand auf dem UI-Thread (z.B. nach einem Fehler).</summary>
    private void SetPttUi(bool active)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsPttActive = active;
            if (!active)
                StopTxTimeout();
        });
    }

    // ---- Konstruktor ----------------------------------------------------------------------------

    public MainPageViewModel()
    {
        // Mode-Liste aus Mode.cs (alle Werte), zuletzt benutzten Mode wiederherstellen.
        // Die Felder werden direkt gesetzt, damit die OnXxxChanged-Hooks nicht schon speichern.
        List<Mode> ordered = PreferredModeOrder.Concat(Enum.GetValues<Mode>().Except(PreferredModeOrder)).ToList();
        _modeByName = ordered.ToDictionary(ToName);
        ModeNames = ordered.Select(ToName).ToList();
        selectedMode = UserSettings.Instance().Common.LastMode;
        rxVolume = GetRxVolume(selectedMode);
        micGain = GetMicGain(selectedMode);

        DataFiles.LoadDmrTalkgroups(); // Dmr/Data/DmrTalkGroups.csv (beim ersten Start aus dem App-Paket kopiert)
        UpdateLinkTargets(selectedMode);

        ConnectCommand = new RelayCommand(ConnectDisconnect, CanConnect);
        TogglePttCommand = new RelayCommand(TogglePtt);

        new Thread(PttLoop) { IsBackground = true, Name = "PttControl" }.Start();

        AppLifecycle.Pausing += OnAppPausing; // Home, Übersicht (Viereck), Bildschirm aus -> Client sofort stoppen
    }

    // ---- Connect / Disconnect -------------------------------------------------------------

    /// <summary>
    /// Der Knopf: läuft auf dem UI-Thread und kehrt sofort zurück. Die eigentliche Arbeit (blockierend, synchron) läuft auf
    /// einem eigenen Thread; <see cref="IsBusy"/> sperrt den Knopf, bis er fertig ist.
    /// </summary>
    private void ConnectDisconnect()
    {
        bool connect = !IsServerConnected;
        IsBusy = true;

        new Thread(() =>
        {
            try
            {
                if (connect)
                    Connect();
                else
                    Disconnect("Benutzer");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Connect/Disconnect failed.");
                ShowAlert("Error", $"{ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => IsBusy = false);
            }
        })
        { IsBackground = true, Name = "ConnectDisconnect" }.Start();
    }

    // Läuft auf dem Connect-Thread
    private void Connect()
    {
        StartResult result;
        switch (SelectedMode)
        {
            case Mode.Dmr:
                result = StartStopDmr(true);
                break;
            default:
                return; // CanConnect() sperrt alle anderen Modes bereits, hier nur als Sicherheitsnetz
        }

        if (result.Cancelled)
        {
            Log.Info("Connect cancelled (the app was not in the foreground).");
            return;
        }
        if (!result.Success)
        {
            ShowAlert("Connection failed", result.Error ?? "Unknown error");
            return;
        }

        Log.Info($"{ToName(SelectedMode)} connected.");
    }

    // Läuft auf einem Hintergrund-Thread (Disconnect-Knopf oder Pause)
    private void Disconnect(string reason)
    {
        switch (SelectedMode)
        {
            case Mode.Dmr:
                StartStopDmr(false, reason);
                break;
        }

        Log.Info($"Disconnected ({reason}).");
    }

    /// <summary>
    /// Die App verliert den Vordergrund (Home, Übersicht/Viereck, Bildschirm aus, anderer Dialog): Client sofort stoppen.
    /// Läuft auf dem UI-Thread und kehrt sofort zurück, das eigentliche Stop() läuft auf dem Thread-Pool.
    /// </summary>
    private void OnAppPausing()
    {
        if (!IsClientActive)
            return; // nichts gestartet (z.B. nur ein System-Dialog während des Verbindungsaufbaus)

        Log.Info("App is losing the foreground: stopping the client immediately.");
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                Disconnect("App im Hintergrund");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Stopping the client on pause failed.");
            }
        });
    }

    // ---- DMR -------------------------------------------------------------------------------

    /// <summary>
    /// Startet bzw. stoppt den DMR-Client (Gegenstück zu <c>StartStopDmr(bool)</c> in der Avalonia-MainViewModel).
    /// Synchron und blockierend: nur auf einem Hintergrund-Thread aufrufen. Die Oberfläche wird innerhalb der Sperre
    /// aktualisiert, damit die Reihenfolge der Meldungen (verbunden/getrennt) immer der des Clients entspricht.
    /// </summary>
    /// <param name="arg">true = starten, false = stoppen.</param>
    /// <param name="reason">Grund des Stopps (nur fürs Log).</param>
    private StartResult StartStopDmr(bool arg, string reason = "")
    {
        Log.Debug($"arg = {arg}");

        if (!arg)
        {   // S T O P: kann jederzeit kommen, auch während der Start noch läuft (wartet dann auf die Sperre)
            _dmrStopRequested = true;
            lock (_dmrLock)
            {
                StopDmrCore(reason);
                SetConnectedUi(false);
            }
            return new StartResult(true);
        }

        // S T A R T
        lock (_dmrLock)
        {
            if (IsClientActive)
                return new StartResult(false, "DMR is already running.");

            _dmrStopRequested = false;

            StartResult result;
            try
            {
                result = StartDmrCore();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Starting the DMR client failed.");
                result = new StartResult(false, $"{ex.GetType().Name}: {ex.Message}");
            }

            if (result.Success)
                SetConnectedUi(true);
            else
                StopDmrCore("Start failed or was cancelled"); // räumt auch teilweise Angelegtes auf

            return result;
        }
    }

    private StartResult StartDmrCore()
    {
#if ANDROID
        UserSettings us = UserSettings.Instance();

        List<string> errors = us.ValidateForDmr();
        if (errors.Count > 0)
            return new StartResult(false, "Please check the settings:\n" + string.Join("\n", errors));

        Context? context = Platform.CurrentActivity;
        if (context == null)
            return new StartResult(false, "No activity available.");

        // 1) AMBE-Stick finden und USB-Berechtigung einholen (blockiert, solange der System-Dialog offen ist). Der Dialog
        //    pausiert die App kurz; es läuft noch kein Client, ein Pause-Ereignis ist an dieser Stelle also harmlos.
        var device = AmbeUsb.FindDevice(context);
        if (device == null)
            return new StartResult(false, "No AMBE stick (FTDI) found. Are the OTG adapter and the stick plugged in?");

        if (!AmbeUsb.RequestPermission(context, device))
            return new StartResult(false, "USB permission was not granted.");

        // 1b) Mikrofon-Berechtigung (nur für das Senden nötig). Der Systemdialog pausiert die App ebenfalls kurz; hier läuft
        //     noch kein Client. Wird sie verweigert, bleibt Hören möglich, PTT ist dann gesperrt.
        bool micAllowed = EnsureMicrophonePermission();
        MainThread.BeginInvokeOnMainThread(() => IsMicrophoneAllowed = micAllowed);
        if (!micAllowed)
        {
            ShowAlert("Microphone",
                "Microphone permission was not granted. You can listen, but transmitting is disabled. " +
                "You can allow it in the Android app settings.");
        }

        // Nach den Dialogen muss die App wieder im Vordergrund sein. Sonst (Home/Übersicht gedrückt) nicht verbinden.
        if (!AppLifecycle.WaitForForeground(TimeSpan.FromSeconds(2)) || _dmrStopRequested)
            return new StartResult(false, Cancelled: true);

        // 2) Hardware-Objekte (der Chip wird erst von Client.Start() geöffnet)
        _ambeController = new Ambe3000UsbController(context, device);
        _audioPlayer = new AndroidAudioPlayer { GainDb = (float)RxVolume };
        _microphoneReader = new AndroidMicrophoneReader { GainDb = (float)MicGain }; // für TX später; wird hier nicht gestartet

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
                _dmrClient1 = new DmrClient1(cfg) { ExternalDmrDataConsumer = Dmr.ConsumeDmrData };
                start = _dmrClient1.Start;
                break;
            case DmrProtocol.MmdvmHost:
                (_, string host, int port, _) = DmrHosts.GetHostInfo(us.Dmr.Master);
                if (string.IsNullOrEmpty(host))
                    return new StartResult(false, $"DMR master '{us.Dmr.Master}' is unknown.");

                cfg.BmServerAddress = host;
                cfg.BmServerPort = port;
                _dmrClient2 = new DmrClient2(cfg) { ExternalDmrDataConsumer = Dmr.ConsumeDmrData };
                start = _dmrClient2.Start;
                break;
            default:
                return new StartResult(false, $"Unbekanntes DMR-Protokoll: {us.Dmr.Protocol}");
        }

        // 4) Start blockiert (DNS, Chip-Init, Login senden): läuft hier auf dem Connect-Thread
        Log.Info($"Starting DMR ({us.Dmr.Protocol}), callsign={cfg.Callsign}, id={cfg.FullDmrId}, server={cfg.BmServerAddress}:{cfg.BmServerPort}, " +
                 $"rxVolume={RxVolume:F1} dB, micGain={MicGain:F1} dB");
        start();

        // Während des Starts kam ein Stopp (z.B. Home gedrückt): sofort wieder beenden
        if (_dmrStopRequested)
            return new StartResult(false, Cancelled: true);

        return new StartResult(true);
#else
        return new StartResult(false, "Available on Android only.");
#endif
    }

    private void StopDmrCore(string reason)
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
        TryRun(() => client1?.Stop(), "DmrClient1.Stop()");
        TryRun(() => client2?.Stop(), "DmrClient2.Stop()");
#if ANDROID
        TryRun(() => mic?.Dispose(), "microphone dispose");
        TryRun(() => player?.Dispose(), "audio player dispose");
        TryRun(() => ambe?.Dispose(), "AMBE controller dispose"); // Close() ist idempotent
#endif

        Log.Info("DMR stopped.");
    }

#if ANDROID
    /// <summary>
    /// Prüft die Mikrofon-Berechtigung und fragt sie bei Bedarf ab. Die Permissions-API gibt es nur async und nur auf dem
    /// UI-Thread: Der Aufruf springt dorthin und wartet HIER (auf dem Connect-Thread, nie auf dem UI-Thread), deshalb
    /// gibt es keinen Deadlock.
    /// </summary>
    private static bool EnsureMicrophonePermission()
    {
        PermissionStatus status = MainThread.InvokeOnMainThreadAsync(() => Permissions.CheckStatusAsync<Permissions.Microphone>())
            .GetAwaiter().GetResult();

        if (status != PermissionStatus.Granted)
        {
            status = MainThread.InvokeOnMainThreadAsync(() => Permissions.RequestAsync<Permissions.Microphone>())
                .GetAwaiter().GetResult();
        }

        return status == PermissionStatus.Granted;
    }
#endif

    private static void TryRun(Action action, string what)
    {
        try { action(); }
        catch (Exception ex) { Log.Error(ex, $"{what} failed."); }
    }

    // ---- Hilfen für die Oberfläche ---------------------------------------------------------------

    /// <summary>Setzt den Verbindungszustand auf dem UI-Thread (und leert beim Trennen die DMR-Ansicht).</summary>
    private void SetConnectedUi(bool connected)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsServerConnected = connected;
            if (!connected)
            {
                IsPttActive = false; // eine beim Trennen noch aktive Übertragung gilt als beendet
                StopTxTimeout();
                Dmr.Clear();
            }
        });
    }

    /// <summary>Meldung an die View (Fire-and-forget): Die View zeigt sie mit DisplayAlert an.</summary>
    private void ShowAlert(string title, string message) =>
        MainThread.BeginInvokeOnMainThread(() => { _ = ShowAlertRequested?.Invoke(title, message); });
}
