using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoice.Fusion;
using DigitalVoice.Nxdn;
using DigitalVoiceControlApp.Maui.Config;
using DigitalVoiceControlApp.Maui.Services;
using NLog;
using System.Collections.Concurrent;
using DigitalVoice.AmbeSupport;


#if ANDROID
using Android.Content;
using Maui.AmbeSupport;
using Maui.AudioSupport;
#endif

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// ViewModel für die Hauptseite (Gegenstück zum Avalonia-<c>MainViewModel</c>): Mode-Auswahl, Connect/Disconnect,
/// Start/Stop der Clients (<see cref="StartStopDmr"/> entspricht <c>StartStopDmr(bool)</c> der Desktop-App), RX-/MIC-Gain,
/// PTT. Angebunden sind DMR, NXDN, YSF, FCS und D-STAR (DCS, REF, XRF).
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
    NxdnClient? _nxdnClient;
    YsfClient? _ysfClient;
    FcsClient? _fcsClient;
    IDStarClient? _dstarClient; // DCS, REF oder XRF (jeweils nur einer)

#if ANDROID
    IAmbe3000RController? _ambeController;
    AndroidAudioPlayer? _audioPlayer;
    AndroidMicrophoneReader? _microphoneReader;
#endif

    // Start und Stop laufen nacheinander (für alle Modes); ein Stop während des Starts beendet den Start sofort nach dem Verbinden.
    readonly object _clientLock = new();
    volatile bool _stopRequested;

    /// <summary>true, sobald ein Client angelegt ist (Start läuft oder Client läuft).</summary>
    bool IsClientActive => _dmrClient1 != null || _dmrClient2 != null || _nxdnClient != null || _ysfClient != null || _fcsClient != null || _dstarClient != null;

    /// <summary>Ergebnis eines Startversuchs (Success = Client läuft, Cancelled = durch Pause abgebrochen, ohne Meldung).</summary>
    private sealed record StartResult(bool Success, string? Error = null, bool Cancelled = false);

    /// <summary>Die DMR-Ansicht (Rufzeichen, Quelle, Ziel, Last Heard). Der Client meldet seine Daten dorthin.</summary>
    public DmrViewModel Dmr { get; } = new();

    /// <summary>Die NXDN-Ansicht (Rufzeichen, Quelle, Ziel, Gateway, Last Heard).</summary>
    public NxdnViewModel Nxdn { get; } = new();

    /// <summary>Die Fusion-Ansicht (YSF, später FCS): Quelle, Gateway, Ziel, Datentyp, Last Heard.</summary>
    public FusionViewModel Fusion { get; } = new();

    /// <summary>Die D-STAR-Ansicht (DCS, REF, XRF): RPTR1/2, MYCALL, URCALL, Text, GPS, Last Heard.</summary>
    public DStarViewModel DStar { get; } = new();

    /// <summary>true, solange ein Connect/Disconnect auf seinem Thread läuft (der Knopf ist dann gesperrt).</summary>
    [ObservableProperty]
    private bool isBusy;

    partial void OnIsBusyChanged(bool value) => ConnectCommand.NotifyCanExecuteChanged();

    /// <summary>Statusmeldung des D-STAR-Clients (z.B. Verbindungsmeldungen des Reflektors), solange verbunden.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private string netMessage = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionIcon))]
    [NotifyPropertyChangedFor(nameof(ConnectionTooltip))]
    [NotifyPropertyChangedFor(nameof(ConnectionBackgroundColor))]
    [NotifyPropertyChangedFor(nameof(IsModePickerEnabled))]
    [NotifyPropertyChangedFor(nameof(IsSettingsEnabled))]
    [NotifyPropertyChangedFor(nameof(IsPttEnabled))]
    [NotifyPropertyChangedFor(nameof(PttBackgroundColor))]
    [NotifyPropertyChangedFor(nameof(IsLinkTargetPickerEnabled))]
    [NotifyPropertyChangedFor(nameof(IsModulePickerEnabled))]
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
    [NotifyPropertyChangedFor(nameof(IsNxdnViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsFusionViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsDStarViewVisible))]
    [NotifyPropertyChangedFor(nameof(IsPttEnabled))]
    [NotifyPropertyChangedFor(nameof(PttBackgroundColor))]
    [NotifyPropertyChangedFor(nameof(IsLinkTargetPickerEnabled))]
    [NotifyPropertyChangedFor(nameof(IsModulePickerEnabled))]
    [NotifyPropertyChangedFor(nameof(IsModulePickerVisible))]
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
        ConnectCommand.NotifyCanExecuteChanged(); // Connect ist in allen Modes freigegeben

        RxVolume = GetRxVolume(value); // die für diesen Mode gemerkten Werte
        MicGain = GetMicGain(value);

        LoadModuleForMode(value);
        UpdateLinkTargets(value);
    }

    /// <summary>Die DMR-Ansicht wird nur im DMR-Mode gezeigt.</summary>
    public bool IsDmrViewVisible => SelectedMode == Mode.Dmr;

    /// <summary>Die NXDN-Ansicht wird nur im NXDN-Mode gezeigt.</summary>
    public bool IsNxdnViewVisible => SelectedMode == Mode.Nxdn;

    /// <summary>Die Fusion-Ansicht wird im YSF- und im FCS-Mode gezeigt (wie in der Desktop-App dieselbe Ansicht).</summary>
    public bool IsFusionViewVisible => SelectedMode is Mode.Ysf or Mode.Fcs;

    /// <summary>Die D-STAR-Ansicht wird in den Modes DCS, REF und XRF gezeigt.</summary>
    public bool IsDStarViewVisible => SelectedMode is Mode.Dcs or Mode.Ref or Mode.Xrf;

    /// <summary>Während der Verbindung ist der Mode gesperrt (wie in der Desktop-App).</summary>
    public bool IsModePickerEnabled => !IsServerConnected;

    /// <summary>Während der Verbindung sind die Settings gesperrt (wie in der Desktop-App).</summary>
    public bool IsSettingsEnabled => !IsServerConnected;

    /// <summary>Die Modes, die auf Android schon angebunden sind.</summary>
    private static bool IsSupportedMode(Mode mode) => mode is Mode.Dmr or Mode.Nxdn or Mode.Ysf or Mode.Fcs or Mode.Dcs or Mode.Ref or Mode.Xrf;

    /// <summary>Verbinden ist in allen Modes möglich; Trennen immer, solange verbunden. Während Connect/Disconnect läuft: gesperrt.</summary>
    private bool CanConnect() => !IsBusy && (IsServerConnected || IsSupportedMode(SelectedMode));

    partial void OnIsServerConnectedChanged(bool value)
    {
        ConnectCommand.NotifyCanExecuteChanged();

        // Während verbunden darf der Bildschirm nicht von selbst ausgehen: Bildschirm aus = App pausiert = Client wird gestoppt.
        DeviceDisplay.Current.KeepScreenOn = value; // läuft immer auf dem UI-Thread (siehe SetConnectedUi)
    }

    /// <summary>Statuszeile, die auch erklärt, warum Connect gesperrt ist.</summary>
    public string StatusText =>
        IsServerConnected ? $"{ToName(SelectedMode)}: {(string.IsNullOrEmpty(NetMessage) ? "connected" : NetMessage)}"
        : IsSupportedMode(SelectedMode) ? $"{ToName(SelectedMode)}: ready to connect"
        : $"{ToName(SelectedMode)}: not yet available on Android";

    // TODO: Dateinamen anpassen, sobald die tatsächlichen Icon-Dateinamen im Projekt feststehen
    // (Resources/Images/, Kleinschreibung, z.B. connect_16x.png / disconnect_16x.png).
    public string ConnectionIcon => IsServerConnected ? "disconnect_16x.png" : "connect_16x.png";

    public string ConnectionTooltip =>
        IsServerConnected ? "Disconnect from Server"
        : IsSupportedMode(SelectedMode) ? "Connect to Server"
        : "Connect is not yet available for this mode";

    public Color ConnectionBackgroundColor => IsServerConnected ? Colors.Red : Colors.Transparent;

    public IRelayCommand ConnectCommand { get; }

    /// <summary>Wird ausgelöst, wenn die View einen Popup-Hinweis anzeigen soll (Fehler etc.). Die View zeigt ihn async an.</summary>
    public event Func<string, string, Task>? ShowAlertRequested;

    // ---- D-STAR-Modul (A-Z) --------------------------------------------------------------------

    /// <summary>Die wählbaren Module A bis Z (wie in der Desktop-App).</summary>
    public IReadOnlyList<string> ModuleNames { get; } = Enumerable.Range('A', 26).Select(i => ((char)i).ToString()).ToList();

    private string _selectedModule = "A";

    /// <summary>
    /// Das gewählte Modul. Im DCS-, REF- und XRF-Mode wird die Änderung in den Einstellungen des jeweiligen Modes gemerkt
    /// (<c>Dcs/Ref/Xrf.LastModule</c>); in den anderen Modes ist die Auswahl gesperrt.
    /// </summary>
    public string SelectedModule
    {
        get => _selectedModule;
        set
        {
            if (string.IsNullOrEmpty(value) || value == _selectedModule)
                return;

            SetProperty(ref _selectedModule, value);

            char module = value[0];
            UserSettings us = UserSettings.Instance();
            switch (SelectedMode)
            {
                case Mode.Dcs: us.Dcs.LastModule = module; break;
                case Mode.Ref: us.Ref.LastModule = module; break;
                case Mode.Xrf: us.Xrf.LastModule = module; break;
                default: return; // nur D-STAR hat ein Modul
            }
            UserSettings.Save();
        }
    }

    /// <summary>Das Modul ist nur in den D-STAR-Modes (DCS, REF, XRF) und nur im getrennten Zustand wählbar.</summary>
    public bool IsModulePickerEnabled => SelectedMode is Mode.Dcs or Mode.Ref or Mode.Xrf && !IsServerConnected;

    /// <summary>Das Modul wird nur in den D-STAR-Modes (DCS, REF, XRF) gezeigt, in den anderen Modes ist es ausgeblendet.</summary>
    public bool IsModulePickerVisible => SelectedMode is Mode.Dcs or Mode.Ref or Mode.Xrf;

    /// <summary>
    /// Zeigt das für den Mode gespeicherte Modul an (nur DCS, REF, XRF). Setzt das Feld direkt, damit dabei nichts
    /// zurückgeschrieben wird.
    /// </summary>
    private void LoadModuleForMode(Mode mode)
    {
        UserSettings us = UserSettings.Instance();
        char? module = mode switch
        {
            Mode.Dcs => us.Dcs.LastModule,
            Mode.Ref => us.Ref.LastModule,
            Mode.Xrf => us.Xrf.LastModule,
            _ => null,
        };

        if (module == null)
            return; // die Anzeige bleibt (gesperrt) beim zuletzt gezeigten Modul

        string name = char.ToUpperInvariant(module.Value).ToString();
        _selectedModule = ModuleNames.Contains(name) ? name : "A";
        OnPropertyChanged(nameof(SelectedModule));
    }

    // ---- Talkgroup-/Reflektor-Auswahl ------------------------------------------------------------

    // Anzeigename -> Ziel (DMR: Talkgroup und Typ, NXDN: Reflektor-ID; die anderen Modes folgen)
    private readonly Dictionary<string, (int DmrId, Flco Flco)> _dmrLinkTargets = [];
    private readonly Dictionary<string, int> _nxdnLinkTargets = [];
    private readonly Dictionary<string, string> _ysfLinkTargets = []; // Anzeigename -> Designator
    private readonly Dictionary<string, string> _fcsLinkTargets = []; // Anzeigename -> Reflektor-ID

    /// <summary>Die Einträge des Pickers (im DMR-Mode die Talkgroups aus <c>DmrTalkGroups.csv</c>).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLinkTargetPickerEnabled))]
    private IReadOnlyList<string> linkTargetNames = [];

    /// <summary>Überschrift des Pickers, solange nichts gewählt ist.</summary>
    [ObservableProperty]
    private string linkTargetTitle = "Talkgroup";

    // Beim Senden gesperrt. Bei allen Modes außer DMR ist der Reflektor die Verbindung selbst: Er lässt sich nur im getrennten Zustand wechseln.
    /// <summary>
    /// Wird ausgelöst (immer auf dem UI-Thread), wenn sich die Liste des Pickers und damit die Auswahl geändert hat, z.B. beim
    /// Moduswechsel. Die Seite setzt dann Liste und Auswahl des Pickers selbst, in fester Reihenfolge (siehe
    /// <c>MainPage.RefreshLinkTargetPicker</c>). Das ist verlässlicher als die Bindung von <c>ItemsSource</c> und
    /// <c>SelectedItem</c>, denn der Picker setzt seine Auswahl beim Austausch der Liste zurück.
    /// </summary>
    public event Action? LinkTargetsChanged;

    /// <summary>Index des gewählten Eintrags in <see cref="LinkTargetNames"/>, oder -1.</summary>
    public int SelectedLinkTargetIndex
    {
        get
        {
            for (int i = 0; i < LinkTargetNames.Count; i++)
            {
                if (LinkTargetNames[i] == _selectedLinkTargetName)
                    return i;
            }
            return -1;
        }
    }

    public bool IsLinkTargetPickerEnabled =>
        LinkTargetNames.Count > 0 && !IsPttActive && (!IsServerConnected || SelectedMode == Mode.Dmr);

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
            }
            else if (SelectedMode == Mode.Nxdn && _nxdnLinkTargets.TryGetValue(value, out int reflectorId))
            {
                UserSettings.Instance().Nxdn.LastReflectorId = reflectorId; // beim nächsten Verbinden wieder vorgewählt
                UserSettings.Save();
            }
            else if (SelectedMode == Mode.Ysf && _ysfLinkTargets.TryGetValue(value, out string? designator))
            {
                UserSettings.Instance().Ysf.LastReflector = designator; // beim nächsten Verbinden wieder vorgewählt
                UserSettings.Save();
            }
            else if (SelectedMode == Mode.Fcs && _fcsLinkTargets.TryGetValue(value, out string? fcsReflectorId))
            {
                UserSettings.Instance().Fcs.LastReflector = fcsReflectorId; // beim nächsten Verbinden wieder vorgewählt
                UserSettings.Save();
            }
            else if (SelectedMode is Mode.Dcs or Mode.Ref or Mode.Xrf)
            {
                // D-STAR: Der Anzeigename ist die Reflektor-ID selbst (z.B. DCS001)
                UserSettings dstarSettings = UserSettings.Instance();
                switch (SelectedMode)
                {
                    case Mode.Dcs: dstarSettings.Dcs.LastReflector = value; break;
                    case Mode.Ref: dstarSettings.Ref.LastReflector = value; break;
                    case Mode.Xrf: dstarSettings.Xrf.LastReflector = value; break;
                }
                UserSettings.Save(); // beim nächsten Verbinden wieder vorgewählt
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
        _nxdnLinkTargets.Clear();
        _ysfLinkTargets.Clear();
        _fcsLinkTargets.Clear();
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
        else if (mode == Mode.Nxdn)
        {
            foreach (var kvp in NxdnHosts.All)
            {
                string name = $"{kvp.Key} - {kvp.Value.Host}"; // wie in der Desktop-App
                _nxdnLinkTargets[name] = kvp.Key;
                names.Add(name);
            }

            int last = UserSettings.Instance().Nxdn.LastReflectorId;
            selected = names.FirstOrDefault(n => _nxdnLinkTargets[n] == last) ?? "";
        }
        else if (mode == Mode.Ysf)
        {
            // Wie in der Desktop-App nach Namen sortiert. Kommt ein Name mehrfach vor (derzeit 4 Fälle in der Liste), wird der
            // Designator angehängt, sonst wären die Einträge nicht zu unterscheiden.
            var hosts = YsfHosts.All.OrderBy(kvp => kvp.Value.FullName, StringComparer.Ordinal).ToList();
            HashSet<string> duplicates = hosts.GroupBy(kvp => kvp.Value.FullName).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();

            foreach (var kvp in hosts)
            {
                string name = duplicates.Contains(kvp.Value.FullName) ? $"{kvp.Value.FullName} [{kvp.Key}]" : kvp.Value.FullName;
                _ysfLinkTargets[name] = kvp.Key;
                names.Add(name);
            }

            string last = UserSettings.Instance().Ysf.LastReflector;
            selected = names.FirstOrDefault(n => _ysfLinkTargets[n] == last) ?? "";
        }
        else if (mode == Mode.Fcs)
        {
            foreach (var kvp in FcsHosts.All)
            {
                string name = $"{kvp.Key}-{kvp.Value.FullName}"; // wie in der Desktop-App
                _fcsLinkTargets[name] = kvp.Key;
                names.Add(name);
            }

            string last = UserSettings.Instance().Fcs.LastReflector;
            selected = names.FirstOrDefault(n => _fcsLinkTargets[n] == last) ?? "";
        }
        else if (mode is Mode.Dcs or Mode.Ref or Mode.Xrf)
        {
            // D-STAR: Reflektor-IDs aus den Hostlisten (wie die Desktop-App); der Anzeigename ist die ID
            names.AddRange(mode switch
            {
                Mode.Dcs => DStarReflectors.GetDcsIds(),
                Mode.Ref => DStarReflectors.GetRefIds(),
                _ => DStarReflectors.GetXrfIds(),
            });

            UserSettings dstarSettings = UserSettings.Instance();
            string last = mode switch
            {
                Mode.Dcs => dstarSettings.Dcs.LastReflector,
                Mode.Ref => dstarSettings.Ref.LastReflector,
                _ => dstarSettings.Xrf.LastReflector,
            };
            selected = names.Contains(last) ? last : "";
        }

        LinkTargetTitle = mode == Mode.Dmr ? "Talkgroup" : "Reflector";
        _selectedLinkTargetName = selected; // Feld direkt: nichts speichern
        LinkTargetNames = names;

        // Die Seite überträgt Liste und gemerkte Auswahl in den Picker
        LinkTargetsChanged?.Invoke();
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

    /// <summary>PTT ist nur verbunden (DMR oder NXDN) und mit Mikrofon-Berechtigung bedienbar.</summary>
    public bool IsPttEnabled => IsServerConnected && IsSupportedMode(SelectedMode) && IsMicrophoneAllowed;

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

            // Nur DMR hat ein wählbares Sendeziel (Talkgroup); bei NXDN ist das Ziel der verbundene Reflektor
            if (SelectedMode == Mode.Dmr)
            {
                if (!_dmrLinkTargets.TryGetValue(SelectedLinkTargetName, out var target))
                {
                    ShowAlert("PTT", "Please select a talkgroup first.");
                    return;
                }

                dmrId = target.DmrId;
                flco = target.Flco;
            }
        }

        IsPttActive = on; // sofort sichtbar (rot/blau)
        if (on)
            StartTxTimeout();
        else
            StopTxTimeout();

        Log.Info(!on ? "PTT off"
            : SelectedMode == Mode.Dmr ? $"PTT on, destination {dmrId} ({flco})"
            : $"PTT on ({ToName(SelectedMode)})");
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
        lock (_clientLock)
        {
            DmrClient1? client1 = _dmrClient1;
            DmrClient2? client2 = _dmrClient2;
            NxdnClient? nxdn = _nxdnClient;
            YsfClient? ysf = _ysfClient;
            FcsClient? fcs = _fcsClient;
            IDStarClient? dstar = _dstarClient;

            if (client1 == null && client2 == null && nxdn == null && ysf == null && fcs == null && dstar == null)
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
                nxdn?.StartStopTransmit(command.On);
                ysf?.StartStopTransmit(command.On);
                fcs?.StartStopTransmit(command.On);
                dstar?.StartStopTransmit(command.On);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"StartStopTransmit({command.On}) failed.");

                // Teilweise Gestartetes aufräumen (z.B. wenn das Mikrofon nicht starten wollte)
                if (command.On)
                {
                    TryRun(() => client1?.StartStopTransmit(false), "DmrClient1.StartStopTransmit(false)");
                    TryRun(() => client2?.StartStopTransmit(false), "DmrClient2.StartStopTransmit(false)");
                    TryRun(() => nxdn?.StartStopTransmit(false), "NxdnClient.StartStopTransmit(false)");
                    TryRun(() => ysf?.StartStopTransmit(false), "YsfClient.StartStopTransmit(false)");
                    TryRun(() => fcs?.StartStopTransmit(false), "FcsClient.StartStopTransmit(false)");
                    TryRun(() => dstar?.StartStopTransmit(false), "D-STAR client StartStopTransmit(false)");
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
        LoadModuleForMode(selectedMode);

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
                    Disconnect("user");
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
            case Mode.Nxdn:
                result = StartStopNxdn(true);
                break;
            case Mode.Ysf:
                result = StartStopYsf(true);
                break;
            case Mode.Fcs:
                result = StartStopFcs(true);
                break;
            case Mode.Dcs:
            case Mode.Ref:
            case Mode.Xrf:
                result = StartStopDStar(true);
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
            case Mode.Nxdn:
                StartStopNxdn(false, reason);
                break;
            case Mode.Ysf:
                StartStopYsf(false, reason);
                break;
            case Mode.Fcs:
                StartStopFcs(false, reason);
                break;
            case Mode.Dcs:
            case Mode.Ref:
            case Mode.Xrf:
                StartStopDStar(false, reason);
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
                Disconnect("app in the background");
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
    private StartResult StartStopDmr(bool arg, string reason = "") => StartStopClient("DMR", StartDmrCore, arg, reason);

    /// <summary>
    /// Startet bzw. stoppt den NXDN-Client (Gegenstück zu <c>StartStopNxdn(bool)</c> in der Avalonia-MainViewModel).
    /// Gleiche Regeln wie <see cref="StartStopDmr"/>.
    /// </summary>
    private StartResult StartStopNxdn(bool arg, string reason = "") => StartStopClient("NXDN", StartNxdnCore, arg, reason);

    /// <summary>
    /// Startet bzw. stoppt den YSF-Client (Gegenstück zu <c>StartStopYsf(bool)</c> in der Avalonia-MainViewModel).
    /// Gleiche Regeln wie <see cref="StartStopDmr"/>.
    /// </summary>
    private StartResult StartStopYsf(bool arg, string reason = "") => StartStopClient("YSF", StartYsfCore, arg, reason);

    /// <summary>
    /// Startet bzw. stoppt den FCS-Client (Gegenstück zu <c>StartStopFcs(bool)</c> in der Avalonia-MainViewModel).
    /// Gleiche Regeln wie <see cref="StartStopDmr"/>.
    /// </summary>
    private StartResult StartStopFcs(bool arg, string reason = "") => StartStopClient("FCS", StartFcsCore, arg, reason);

    /// <summary>
    /// Startet bzw. stoppt den D-STAR-Client des gewählten Modes (DCS, REF oder XRF; Gegenstück zu <c>StartStopDcs/Ref/Xrf</c>
    /// in der Avalonia-MainViewModel). Gleiche Regeln wie <see cref="StartStopDmr"/>.
    /// </summary>
    private StartResult StartStopDStar(bool arg, string reason = "") =>
        StartStopClient(ToName(SelectedMode), StartDStarCore, arg, reason);

    /// <summary>Gemeinsamer Ablauf für Start und Stop der Clients (Sperre, Abbruch, Aufräumen, Zustand der Oberfläche).</summary>
    private StartResult StartStopClient(string name, Func<StartResult> startCore, bool arg, string reason)
    {
        Log.Debug($"{name}: arg = {arg}");

        if (!arg)
        {   // S T O P: kann jederzeit kommen, auch während der Start noch läuft (wartet dann auf die Sperre)
            _stopRequested = true;
            lock (_clientLock)
            {
                StopClientsCore(reason);
                SetConnectedUi(false);
            }
            return new StartResult(true);
        }

        // S T A R T
        lock (_clientLock)
        {
            if (IsClientActive)
                return new StartResult(false, $"{name} is already running.");

            _stopRequested = false;

            StartResult result;
            try
            {
                result = startCore();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Starting the {name} client failed.");
                result = new StartResult(false, $"{ex.GetType().Name}: {ex.Message}");
            }

            if (result.Success)
                SetConnectedUi(true);
            else
                StopClientsCore("Start failed or was cancelled"); // räumt auch teilweise Angelegtes auf

            return result;
        }
    }

#if ANDROID
    /// <summary>
    /// Gemeinsamer Teil des Starts für alle Modes: AMBE-Stick finden, USB- und Mikrofon-Berechtigung einholen, auf die Rückkehr
    /// in den Vordergrund warten und die Hardware-Objekte (Chip, Wiedergabe, Mikrofon) anlegen. Der Chip wird erst vom Client
    /// geöffnet.
    /// </summary>
    /// <returns><c>null</c>, wenn alles bereit ist, sonst das Ergebnis, mit dem der Start abgebrochen wird.</returns>
    private StartResult? PrepareHardware()
    {
        UserSettings us = UserSettings.Instance();
        bool useServer = us.Ambe.ServiceType == AmbeServiceType.Server; // AMBE-Server im Netz statt USB-Stick

        Context? context = Platform.CurrentActivity;
        if (context == null)
            return new StartResult(false, "No activity available.");

        // 1) Nur im Stick-Betrieb: AMBE-Stick finden und USB-Berechtigung einholen (blockiert, solange der System-Dialog offen
        //    ist). Der Dialog pausiert die App kurz; es läuft noch kein Client, ein Pause-Ereignis ist an dieser Stelle also
        //    harmlos. Beim AMBE-Server entfällt das ganz.
        var device = useServer ? null : AmbeUsb.FindDevice(context);
        if (!useServer)
        {
            if (device == null)
                return new StartResult(false, "No AMBE stick (FTDI) found. Are the OTG adapter and the stick plugged in?");

            if (!AmbeUsb.RequestPermission(context, device))
                return new StartResult(false, "USB permission was not granted.");
        }

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
        if (!AppLifecycle.WaitForForeground(TimeSpan.FromSeconds(2)) || _stopRequested)
            return new StartResult(false, Cancelled: true);

        // 2) Hardware-Objekte (der Chip bzw. der Server wird erst von Client.Start() geöffnet)
        if (useServer)
        {
            Log.Info($"AMBE server: {us.Ambe.ServerAddr}:{us.Ambe.ServerPort}");
            _ambeController = new AmbeUdpClient(ip: us.Ambe.ServerAddr.Trim(), port: us.Ambe.ServerPort);
        }
        else
        {
            _ambeController = new Ambe3000UsbController(context, device!);
        }

        _audioPlayer = new AndroidAudioPlayer { GainDb = (float)RxVolume };
        _microphoneReader = new AndroidMicrophoneReader { GainDb = (float)MicGain }; // für TX später; wird hier nicht gestartet

        return null;
    }
#endif

    private StartResult StartDmrCore()
    {
#if ANDROID
        UserSettings us = UserSettings.Instance();

        List<string> errors = us.ValidateForDmr();
        if (errors.Count > 0)
            return new StartResult(false, "Please check the settings:\n" + string.Join("\n", errors));

        StartResult? failure = PrepareHardware();
        if (failure != null)
            return failure;

        // Client-Konfiguration (wie StartStopDmr in der Avalonia-App)
        DmrClientConfig cfg = new()
        {
            AmbeController = _ambeController,
            Callsign = us.Common.Callsign,
            Password = us.Dmr.Password,
            MyDmrId = us.Dmr.MyDmrId,
            EssId = us.Dmr.Essid,
            Flco = Flco.GROUP,            // wird beim Senden mit SetTxDst überschrieben
            ColorCode = us.Dmr.ColorCode,
            TimeSlot = us.Dmr.TimeSlot,
            RecordAudio = false,
            RecordRcvdUdpPackets = false,
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
                return new StartResult(false, $"Unknown DMR protocol: {us.Dmr.Protocol}");
        }

        // 4) Start blockiert (DNS, Chip-Init, Login senden): läuft hier auf dem Connect-Thread
        Log.Info($"Starting DMR ({us.Dmr.Protocol}), callsign={cfg.Callsign}, id={cfg.FullDmrId}, server={cfg.BmServerAddress}:{cfg.BmServerPort}, " +
                 $"rxVolume={RxVolume:F1} dB, micGain={MicGain:F1} dB");
        start();

        // Während des Starts kam ein Stopp (z.B. Home gedrückt): sofort wieder beenden
        if (_stopRequested)
            return new StartResult(false, Cancelled: true);

        return new StartResult(true);
#else
        return new StartResult(false, "Available on Android only.");
#endif
    }

    private StartResult StartNxdnCore()
    {
#if ANDROID
        UserSettings us = UserSettings.Instance();

        List<string> errors = us.ValidateForNxdn();
        if (errors.Count > 0)
            return new StartResult(false, "Please check the settings:\n" + string.Join("\n", errors));

        StartResult? failure = PrepareHardware();
        if (failure != null)
            return failure;

        // Reflektor aus den Einstellungen (wird im Picker gewählt und dort gemerkt)
        int reflectorId = us.Nxdn.LastReflectorId;
        (string host, int port) = NxdnHosts.GetHostInfo(reflectorId);
        if (string.IsNullOrEmpty(host))
            return new StartResult(false, $"NXDN reflector {reflectorId} is unknown.");

        // Client-Konfiguration (wie StartStopNxdn in der Avalonia-App)
        NxdnClientConfig cfg = new()
        {
            AmbeController = _ambeController,
            NxdnReflectorAddr = host,
            NxdnReflectorPort = port,
            NxdnReflectorId = reflectorId,
            Callsign = us.Common.Callsign,
            MyNxdnId = us.Nxdn.NxdnId,
            AudioPlayer = _audioPlayer,
            MicrophoneReader = _microphoneReader,
            RecordAudio = false,
            RecordRxPackets = false,
            SimulationMode = false,
        };

        _nxdnClient = new NxdnClient(cfg) { ExternalNxdnDataConsumer = Nxdn.ConsumeNxdnData };

        // Start blockiert (DNS, Chip-Init, Anmeldung): läuft hier auf dem Connect-Thread
        Log.Info($"Starting NXDN, callsign={cfg.Callsign}, id={cfg.MyNxdnId}, reflector={reflectorId} ({host}:{port}), " +
                 $"rxVolume={RxVolume:F1} dB, micGain={MicGain:F1} dB");
        _nxdnClient.StartClient();

        // Während des Starts kam ein Stopp (z.B. Home gedrückt): sofort wieder beenden
        if (_stopRequested)
            return new StartResult(false, Cancelled: true);

        return new StartResult(true);
#else
        return new StartResult(false, "Available on Android only.");
#endif
    }

    private StartResult StartYsfCore()
    {
#if ANDROID
        UserSettings us = UserSettings.Instance();

        List<string> errors = us.ValidateForYsf();
        if (errors.Count > 0)
            return new StartResult(false, "Please check the settings:\n" + string.Join("\n", errors));

        StartResult? failure = PrepareHardware();
        if (failure != null)
            return failure;

        // Reflektor aus den Einstellungen (wird im Picker gewählt und dort gemerkt)
        string designator = us.Ysf.LastReflector;
        if (!YsfHosts.TryGetHostInfo(designator, out var info))
            return new StartResult(false, $"YSF reflector '{designator}' is unknown.");

        // Client-Konfiguration (wie StartStopYsf in der Avalonia-App)
        YsfClientConfig cfg = new()
        {
            AmbeController = _ambeController,
            ReflectorAddress = info.Host,
            ReflectorPort = info.Port,
            SimulationFile = null,
            SimulationMode = false,
            Callsign = us.Common.Callsign,
            Town = us.Common.Town,
            Locator = us.Common.Locator,
            HotspotType = us.Hotspot.Type,
            RxFrequency = us.Hotspot.RxFrequency,
            TxFrequency = us.Hotspot.TxFrequency,
            MicrophoneReader = _microphoneReader,
            AudioPlayer = _audioPlayer,
            RecordYsfPackets = false,
        };

        _ysfClient = new YsfClient(cfg) { ExternalYsfDataConsumer = Fusion.ConsumeYsfData };

        // Start blockiert (DNS, Chip-Init, Anmeldung): läuft hier auf dem Connect-Thread
        Log.Info($"Starting YSF, callsign={cfg.Callsign}, reflector={designator} ({info.FullName}, {info.Host}:{info.Port}), " +
                 $"rxVolume={RxVolume:F1} dB, micGain={MicGain:F1} dB");
        _ysfClient.StartClient();

        // Während des Starts kam ein Stopp (z.B. Home gedrückt): sofort wieder beenden
        if (_stopRequested)
            return new StartResult(false, Cancelled: true);

        return new StartResult(true);
#else
        return new StartResult(false, "Available on Android only.");
#endif
    }

    private StartResult StartFcsCore()
    {
#if ANDROID
        UserSettings us = UserSettings.Instance();

        List<string> errors = us.ValidateForFcs();
        if (errors.Count > 0)
            return new StartResult(false, "Please check the settings:\n" + string.Join("\n", errors));

        StartResult? failure = PrepareHardware();
        if (failure != null)
            return failure;

        // Reflektor aus den Einstellungen (wird im Picker gewählt und dort gemerkt). Wie in der Desktop-App ergibt sich der
        // Server aus den ersten 6 Zeichen der ID: FCS001xx -> fcs001.xreflector.net
        string reflectorId = us.Fcs.LastReflector;
        string host = $"{reflectorId[..6].ToLower()}.xreflector.net";

        // Client-Konfiguration (wie StartStopFcs in der Avalonia-App)
        FcsClientConfig cfg = new()
        {
            AmbeController = _ambeController,
            ReflectorAddress = host,
            ReflectorPort = us.Fcs.Port,
            ReflectorId = reflectorId,
            Callsign = us.Common.Callsign,
            Locator = us.Common.Locator,
            Town = us.Common.Town,
            HotspotType = us.Hotspot.Type,
            RxFrequency = us.Hotspot.RxFrequency,
            TxFrequency = us.Hotspot.TxFrequency,
            MicrophoneReader = _microphoneReader,
            AudioPlayer = _audioPlayer,
            RecordAudio = false,
            RecordFcsPackets = false,
            SimulationFile = null,
            SimulationMode = false,
        };

        _fcsClient = new FcsClient(cfg) { ExternalFcsDataConsumer = Fusion.ConsumeYsfData };

        // Start blockiert (DNS, Chip-Init, Anmeldung): läuft hier auf dem Connect-Thread
        Log.Info($"Starting FCS, callsign={cfg.Callsign}, reflector={reflectorId} ({host}:{cfg.ReflectorPort}), " +
                 $"rxVolume={RxVolume:F1} dB, micGain={MicGain:F1} dB");
        _fcsClient.StartClient();

        // Während des Starts kam ein Stopp (z.B. Home gedrückt): sofort wieder beenden
        if (_stopRequested)
            return new StartResult(false, Cancelled: true);

        return new StartResult(true);
#else
        return new StartResult(false, "Available on Android only.");
#endif
    }

    private StartResult StartDStarCore()
    {
#if ANDROID
        UserSettings us = UserSettings.Instance();
        Mode mode = SelectedMode;

        List<string> errors = us.ValidateForDStar(mode);
        if (errors.Count > 0)
            return new StartResult(false, "Please check the settings:\n" + string.Join("\n", errors));

        StartResult? failure = PrepareHardware();
        if (failure != null)
            return failure;

        // Reflektor, Modul, Port und Nachricht stammen aus den Einstellungen (Auswahl auf der Hauptseite bzw. YAML)
        IDStarClient? client = DStarClients.Create(mode, us, _ambeController!, _microphoneReader, _audioPlayer,
            DStar.ConsumeDStarData, ConsumeNetMessage, out string? error);
        if (client == null)
            return new StartResult(false, error ?? "Unable to create the D-STAR client.");

        _dstarClient = client;

        (string reflector, char module) = mode switch
        {
            Mode.Dcs => (us.Dcs.LastReflector, us.Dcs.LastModule),
            Mode.Ref => (us.Ref.LastReflector, us.Ref.LastModule),
            _ => (us.Xrf.LastReflector, us.Xrf.LastModule),
        };

        // Start blockiert (Chip-Init, Anmeldung): läuft hier auf dem Connect-Thread
        Log.Info($"Starting {ToName(mode)}, callsign={us.Common.Callsign}, reflector={reflector}, module={module}, " +
                 $"rxVolume={RxVolume:F1} dB, micGain={MicGain:F1} dB");
        _dstarClient.Start();

        // Während des Starts kam ein Stopp (z.B. Home gedrückt): sofort wieder beenden
        if (_stopRequested)
            return new StartResult(false, Cancelled: true);

        return new StartResult(true);
#else
        return new StartResult(false, "Available on Android only.");
#endif
    }

    /// <summary>Eine Statusmeldung des D-STAR-Clients (Aufruf von einem Client-Thread): erscheint in der Statuszeile.</summary>
    private void ConsumeNetMessage(string msg) =>
        MainThread.BeginInvokeOnMainThread(() => NetMessage = msg ?? "");

    private void StopClientsCore(string reason)
    {
        DmrClient1? client1 = _dmrClient1;
        DmrClient2? client2 = _dmrClient2;
        NxdnClient? nxdn = _nxdnClient;
        YsfClient? ysf = _ysfClient;
        FcsClient? fcs = _fcsClient;
        IDStarClient? dstar = _dstarClient;
        _dmrClient1 = null;
        _dmrClient2 = null;
        _nxdnClient = null;
        _ysfClient = null;
        _fcsClient = null;
        _dstarClient = null;

#if ANDROID
        IAmbe3000RController? ambe = _ambeController; // USB-Stick oder AMBE-Server
        AndroidAudioPlayer? player = _audioPlayer;
        AndroidMicrophoneReader? mic = _microphoneReader;
        _ambeController = null;
        _audioPlayer = null;
        _microphoneReader = null;

        if (client1 == null && client2 == null && nxdn == null && ysf == null && fcs == null && dstar == null && ambe == null && player == null && mic == null)
            return; // nichts zu tun
#else
        if (client1 == null && client2 == null && nxdn == null && ysf == null && fcs == null && dstar == null)
            return;
#endif

        Log.Info($"Stopping the client: {reason}");

        // Client.Stop() zuerst: stoppt Timer und TX, meldet sich ab und schließt den Chip. Danach Audio und USB freigeben.
        TryRun(() => client1?.Stop(), "DmrClient1.Stop()");
        TryRun(() => client2?.Stop(), "DmrClient2.Stop()");
        TryRun(() => nxdn?.StopClient(), "NxdnClient.StopClient()");
        TryRun(() => ysf?.StopClient(), "YsfClient.StopClient()");
        TryRun(() => fcs?.StopClient(), "FcsClient.StopClient()");
        TryRun(() => dstar?.Stop(), "D-STAR client Stop()");
#if ANDROID
        TryRun(() => mic?.Dispose(), "microphone dispose");
        TryRun(() => player?.Dispose(), "audio player dispose");
        TryRun(() => (ambe as IDisposable)?.Dispose(), "AMBE controller dispose"); // Stick und Server sind IDisposable
#endif

        Log.Info("Client stopped.");
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
                Nxdn.Clear();
                Fusion.Clear();
                DStar.Clear();
                NetMessage = "";
            }
        });
    }

    /// <summary>Meldung an die View (Fire-and-forget): Die View zeigt sie mit DisplayAlert an.</summary>
    private void ShowAlert(string title, string message) =>
        MainThread.BeginInvokeOnMainThread(() => { _ = ShowAlertRequested?.Invoke(title, message); });
}
