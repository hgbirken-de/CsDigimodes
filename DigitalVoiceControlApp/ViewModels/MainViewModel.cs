using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DigitalVoice.AudioSupport;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoice.DStar.Dcs;
using DigitalVoice.DStar.Ref;
using DigitalVoice.DStar.Xrf;
using DigitalVoice.Fusion;
using DigitalVoice.Nxdn;
using DigitalVoice.Config;
using DigitalVoiceControlApp.Commands;
using DigitalVoiceControlApp.Services;
using MsBox.Avalonia.ViewModels.Commands;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public ICommand ConnectCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand OpenUrlCommand { get; }
    public ICommand ShowAboutCommand { get; }
    public RelayCommand<object?> ShowSettingsDialogCommand { get; }
    public ICommand TogglePttCommand { get; }

    public delegate void AdjustMicGain(float value);
    public delegate void AdjustRxVolumn(float value);

    public AdjustMicGain? ExternalAdjustMicGain;
    public AdjustRxVolumn? ExternalAdjustRxVolumn;

    #region "DCS Properties"

    internal ObservableCollection<char> Modules { get; } = new (Enumerable.Range('A', 26).Select(i => (char)i));
    
    char _selectedModule = 'A';
    public char SelectedModule
    {
        get => _selectedModule;
        set
        {
            if (_selectedModule != value)
            {
                _selectedModule = value;
                OnPropertyChanged(nameof(SelectedModule));
                UserSettings us = UserSettings.Instance();
                switch (SelectedMode)
                {
                    case Mode.Dcs:
                        us.Dcs.LastModule = value;
                        break;
                    case Mode.Ref:
                        us.Ref.LastModule = value;
                        break;
                    case Mode.Xrf:
                        us.Xrf.LastModule = value;
                        break;
                }
            }
        }
    }

    internal bool IsModuleComboBoxEnabled => (SelectedMode is Mode.Dcs or Mode.Ref or Mode.Xrf) && !IsServerConnected;

    //bool IsModuleComboBoxVisible => (SelectedMode is Mode.Dcs);
    internal bool IsModuleComboBoxVisible => true;

    #endregion"

    #region "Properties"

    internal static List<Mode> Modes { get; } = [Mode.Dmr, Mode.Fcs, Mode.Ysf, Mode.Dcs, Mode.Ref, Mode.Xrf, Mode.Nxdn];

    Mode _selectedMode;
    public Mode SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (_selectedMode != value)
            {
                _selectedMode = value;
                OnPropertyChanged(nameof(SelectedMode));

                OnPropertyChanged(nameof(IsModuleComboBoxEnabled));
                OnPropertyChanged(nameof(IsModuleComboBoxVisible));

                UserSettings us = UserSettings.Instance();
                us.Common.LastMode = value;

                switch (value)
                {
                    case Mode.Dcs:
                        SelectedModule = us.Dcs.LastModule;
                        break;
                    case Mode.Ref:
                        SelectedModule = us.Ref.LastModule;
                        break;
                    case Mode.Xrf:
                        SelectedModule = us.Xrf.LastModule;
                        break;
                }

                LoadReflector();

                // Switch the Current VM so the ContentControl updates
                SwitchMode(value);
            }
        }
    }

    ComboBoxItemData? _selectedLinkTarget;
    public ComboBoxItemData? SelectedLinkTarget
    {
        get => _selectedLinkTarget;
        set
        {
            if (value != null && _selectedLinkTarget != value)
            {
                _selectedLinkTarget = value;
                OnPropertyChanged(nameof(SelectedLinkTarget));

                UserSettings us = UserSettings.Instance();
                switch (SelectedMode)
                {
                    case Mode.Dcs:
                        us.Dcs.LastReflector = value.DcsReflectorId;
                        break;
                    case Mode.Dmr:
                        us.Dmr.LastTgInUse = value.DmrId;
                        break;
                    case Mode.Fcs:
                        us.Fcs.LastReflector = value.FcsReflectorId;
                        break;
                    case Mode.Nxdn:
                        us.Nxdn.LastReflectorId = value.NxdnReflectorId;
                        break;
                    case Mode.Ref:
                        us.Ref.LastReflector = value.RefReflectorId;
                        break;
                    case Mode.Xrf:
                        us.Xrf.LastReflector = value.XrfReflectorId;
                        break;
                    case Mode.Ysf:
                        us.Ysf.LastReflector = value.YsfDesignator;
                        break;
                }
            }
        }
    }

    string? _linkTargetToolTip = "";
    public string? LinkTargetToolTip
    {
        get => _linkTargetToolTip;
        set
        {
            _linkTargetToolTip = value;
        }
    }

    private bool _isServerConnected;
    public bool IsServerConnected
    {
        get => _isServerConnected;
        set
        {
            if (_isServerConnected == value)
                return; // avoid redundant updates

            _isServerConnected = value;
            OnPropertyChanged(nameof(IsServerConnected));

            // Notify dependent properties
            OnPropertyChanged(nameof(ConnectionIcon));
            OnPropertyChanged(nameof(ConnectionTooltip));
            OnPropertyChanged(nameof(IsModeComboBoxEnabled));
            OnPropertyChanged(nameof(IsModuleComboBoxEnabled));
            OnPropertyChanged(nameof(IsReflectorsComboBoxEnabled));

            // Update command availability
            if (ShowSettingsDialogCommand is RelayCommand<object?> cmd)
                cmd.RaiseCanExecuteChanged();
        }
    }

    private double _micGain = 50;
    public double MicGain
    {
        get => _micGain;
        set
        {
            if (_micGain != value)
            {
                _micGain = value;
                OnPropertyChanged(nameof(MicGain));

                _microphoneReader.GainDb = (float)value;

                // TODO: implement this
                UserSettings.Instance().Common.MicGain = value;
            }
        }
    }

    private double _rxVolume = 50;
    public double RxVolume
    {
        get => _rxVolume;
        set
        {
            if (_rxVolume != value)
            {
                _rxVolume = value;
                OnPropertyChanged(nameof(RxVolume));

                _audioPlayer.GainDb = (float)value;

                UserSettings.Instance().Common.RxVolume = value;
            }
        }
    }

    private string _statusMessage = Messages.not_connected;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }
    }

    private bool _pttActive;
    public bool PttActive
    {
        get => _pttActive;
        set
        {
            if (_pttActive != value)
            {
                _pttActive = value;
                OnPropertyChanged(nameof(PttActive));
                OnPropertyChanged(nameof(PttButtonBackground));

                // Handle transmission logic
                switch (SelectedMode)
                {
                    case Mode.Dcs:
                         _dcsClient?.StartStopTransmit(_pttActive);
                        break;
                    case Mode.Dmr:
                        UserSettings us = UserSettings.Instance();
                        switch (us.Dmr.Protocol)
                        {
                            case DmrProtocol.Homebrew:
                                _dmrClient1?.SetTxDst(SelectedLinkTarget.DmrId, SelectedLinkTarget.Flco);
                                _dmrClient1?.StartStopTransmit(_pttActive);
                                break;
                            case DmrProtocol.MmdvmHost:
                                _dmrClient2?.SetTxDst(SelectedLinkTarget.DmrId, SelectedLinkTarget.Flco);
                                _dmrClient2?.StartStopTransmit(_pttActive);
                                break;
                        }
                        break;
                    case Mode.Fcs:
                        _fcsClient?.StartStopTransmit(_pttActive);
                        break;
                    case Mode.Nxdn:
                        _nxdnClient?.StartStopTransmit(_pttActive);
                        break;
                    case Mode.Ysf:
                        _ysfClient?.StartStopTransmit(_pttActive);
                        break;
                }
            }
        }
    }

    public ObservableCollection<ComboBoxItemData> LinkTargets { get; } = [];

    public DmrViewModel DmrViewModel { get; } = new();
    public DStarViewModel DStarViewModel { get; } = new();
    public FusionViewModel FusionViewModel { get; } = new();
    public NxdnViewModel NxdnViewModel { get; } = new();

    private ViewModelBase? _current;
    public ViewModelBase? Current
    {
        get => _current;
        set { _current = value; OnPropertyChanged(); }
    }

    #endregion

    DcsClient? _dcsClient;
    DmrClient1? _dmrClient1;
    DmrClient2? _dmrClient2;
    FcsClient? _fcsClient;
    NxdnClient? _nxdnClient;
    RefClient? _refClient;
    YsfClient? _ysfClient;
    XrfClient? _xrfClient;

    readonly MicrophoneReader _microphoneReader = new();
    readonly AudioPlayer _audioPlayer = new();

    readonly ConcurrentDictionary<int, (string, string)> _dmridToCallsign = [];

    bool _textToSpeech = false;


    /// <summary>
    /// Constructor
    /// </summary>
    public MainViewModel()
    {
        UserSettings us = UserSettings.Instance();
        SelectedMode = us.Common.LastMode;

        _textToSpeech = us.Common.TextToSpeech;

        // select the corresponding mode specific view model 
        SwitchMode(_selectedMode);

        MicGain = us.Common.MicGain;
        RxVolume = us.Common.RxVolume;

        switch (SelectedMode)
        {
            case Mode.Dcs:
                SelectedModule = us.Dcs.LastModule;
                break;
            case Mode.Ref:
                SelectedModule = us.Ref.LastModule;
                break;
            case Mode.Xrf:
                SelectedModule = us.Xrf.LastModule;
                break;
        }

        ConnectCommand = new RelayCommand<object?>(_ => ConnectDisconnect());

        ExitCommand = new RelayCommand<object?>(_ => ExitApp());

        OpenUrlCommand = new RelayCommand<string?>(url => OpenUrl(url));
        
        ShowAboutCommand = new RelayCommand(_ => ShowAbout());

        ShowSettingsDialogCommand = new RelayCommand<object?>(async param => await ShowSettings(param), _ => !IsServerConnected);

        TogglePttCommand = new RelayCommand<object?>(_ => PttActive = !PttActive);

        IsServerConnected = false;

    }

    static readonly Bitmap _connectIcon = new(AssetLoader.Open(new Uri("avares://DigitalVoiceControlApp/Assets/Connect_16x.png")));
    static readonly Bitmap _disconnectIcon = new(AssetLoader.Open(new Uri("avares://DigitalVoiceControlApp/Assets/Disconnect_16x.png")));
    public Bitmap ConnectionIcon => IsServerConnected ? _disconnectIcon : _connectIcon;
    public string ConnectionTooltip => IsServerConnected ? "Disconnect from Server" : "Connect to Server";

    public bool IsModeComboBoxEnabled => !IsServerConnected;
    public bool IsReflectorsComboBoxEnabled => !IsServerConnected || (SelectedMode is Mode.Dmr);
    public IBrush PttButtonBackground => _pttActive ? Brushes.Red : Brushes.LightGray;


    /// <summary>
    /// Connects to the server of disconnects from the server.
    /// </summary>
    private void ConnectDisconnect()
    {
        if (IsServerConnected)
        {   // C O N N E C T
            UserSettings us = UserSettings.Instance();
            string? msg = null;
            switch (SelectedMode)
            {
                case Mode.Dcs:
                    StartStopDcs(true);
                    msg = string.Format(Messages.connected_dcs, us.Dcs.LastReflector, us.Dcs.LastModule);
                    break;
                case Mode.Dmr:
                    StartStopDmr(true);
                    msg = us.Dmr.Protocol switch
                    {
                        DmrProtocol.Homebrew => string.Format(Messages.connected_dmr, us.Dmr.BmServerAddr1),
                        DmrProtocol.MmdvmHost => string.Format(Messages.connected_dmr, us.Dmr.Master),
                        _ => throw new NotImplementedException(),
                    };
                    break;
                case Mode.Fcs:
                    StartStopFcs(true);
                    msg = string.Format(Messages.connected_fcs, SelectedLinkTarget?.DisplayName);
                    break;
                case Mode.Nxdn:
                    StartStopNxdn(true);
                    msg = string.Format(Messages.connected_nxdn, SelectedLinkTarget?.DisplayName);
                    break;
                case Mode.Ref:
                    StartStopRef(true);
                    msg = string.Format(Messages.connected_ref, us.Ref.LastReflector, us.Ref.LastModule);
                    break;
                case Mode.Xrf:
                    StartStopXrf(true);
                    msg = string.Format(Messages.connected_xrf, us.Xrf.LastReflector, us.Xrf.LastModule);
                    break;
                case Mode.Ysf:
                    StartStopYsf(true);
                    msg = string.Format(Messages.connected_ysf, SelectedLinkTarget?.DisplayName);
                    break;
            }
            StatusMessage = msg ?? string.Empty;
            //SpeakAsync(msg);
        }
        else
        {   // D I S C O N N E C T
            switch (SelectedMode)
            {
                case Mode.Dcs:
                    StartStopDcs(false);
                    break;
                case Mode.Dmr:
                    StartStopDmr(false);
                    break;
                case Mode.Fcs:
                    StartStopFcs(false);
                    break;
                case Mode.Nxdn:
                    StartStopNxdn(false);
                    break;
                case Mode.Ref:
                    StartStopRef(false);
                    break;
                case Mode.Xrf:
                    StartStopXrf(false);
                    break;
                case Mode.Ysf:
                    StartStopYsf(false);
                    break;
            }

            StatusMessage = Messages.disconnected;

            //SpeakAsync(msg);
        }

    }

    private void ConsumeNetMsg(string msg)
    {
        StatusMessage = msg ?? "";
    }

    private static void ExitApp()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var win = desktop.MainWindow;
            if (win == null && desktop.Windows.Count > 0)
                win = desktop.Windows[0];

            win?.Close(); // will raise Window.Closing
        }
    }

    /// <summary>
    /// Populates the <see cref="LinkTargets"/> collection with entries corresponding 
    /// to the currently selected mode (<see cref="SelectedMode"/>).
    /// </summary>
    /// <remarks>
    /// - For <see cref="Mode.Dcs"/>, entries are taken from DcsHosts dictionary./>.
    /// - For <see cref="Mode.Dmr"/>, entries are taken from ???/>.
    /// - For <see cref="Mode.Fcs"/>, entries are taken from the internal FCS reflector dictionary.
    /// - For <see cref="Mode.Ysf"/>, entries are taken from the internal YSF hosts dictionary and sorted alphabetically by full name.
    /// 
    /// Each entry is added as a <see cref="ComboBoxItemData"/> with a <c>Key</c> and a <c>DisplayName</c>.
    /// The collection is cleared before adding new items.
    /// </remarks>
    private void LoadReflector()
    {
        UserSettings us = UserSettings.Instance();
        LinkTargets.Clear();

        switch (SelectedMode)
        {
            case Mode.Dcs:
                {
                    foreach ((string id, string ipAddr) in DcsHosts.All)
                    {
                         LinkTargets.Add(new() { DcsReflectorId = id, DisplayName = id, Mode = SelectedMode });
                    }
                    SelectedLinkTarget = new ComboBoxItemData() { DcsReflectorId = us.Dcs.LastReflector, Mode = SelectedMode };
                    LinkTargetToolTip = "Select DCS Reflector";
                }
                break;
            case Mode.Dmr:
                {
                    StringBuilder sb = new();
                    foreach (var kvp in DmrTalkgroups.All)
                    {
                        sb.Clear();
                        sb.Append(kvp.Key).Append(' ').Append(kvp.Value.Name).Append(" (").Append(kvp.Value.CallType).Append(')');
                        Flco flco = kvp.Value.CallType == 'G' ? Flco.GROUP : Flco.USER_USER;
                        LinkTargets.Add(new() { DmrId = kvp.Key, Flco = flco, DisplayName = sb.ToString(), Mode = SelectedMode });
                    }
                    if (DmrTalkgroups.TryGetHostInfo(us.Dmr.LastTgInUse, out (string, char) data))
                    {
                        SelectedLinkTarget = new ComboBoxItemData() { DmrId = us.Dmr.LastTgInUse, Flco = (data.Item2 == 'G' ? Flco.GROUP : Flco.USER_USER), Mode = SelectedMode };
                    }
                    LinkTargetToolTip = "Select Talkgroup";
                }
                break;
            case Mode.Fcs:
                {
                    StringBuilder sb = new();
                    foreach (var kvp in FcsHosts.All)
                    {
                        sb.Clear();
                        sb.Append(kvp.Key).Append('-').Append(kvp.Value.FullName);
                        LinkTargets.Add(new() { FcsReflectorId = kvp.Key, DisplayName = sb.ToString(), Mode = SelectedMode });
                    }
                    SelectedLinkTarget = new ComboBoxItemData() { FcsReflectorId = us.Fcs.LastReflector, Mode = SelectedMode };
                    LinkTargetToolTip = "Select FCS Reflector";
                }
                break;
            case Mode.Nxdn:
                {
                    StringBuilder sb = new();
                    foreach (var kvp in NxdnHosts.All)
                    {
                        sb.Clear();
                        sb.Append(kvp.Key).Append(" - ").Append(kvp.Value.Host);
                        LinkTargets.Add(new() { NxdnReflectorId = kvp.Key, DisplayName = sb.ToString(), Mode = SelectedMode });
                    }
                    SelectedLinkTarget = new ComboBoxItemData() { NxdnReflectorId = us.Nxdn.LastReflectorId, Mode = SelectedMode };
                    LinkTargetToolTip = "Select NXDN Reflector";
                }
                break;
            case Mode.Ref:
                {
                    foreach ((string id, string ipAddr) in DPlusHostRepository.All)
                    {
                        LinkTargets.Add(new() { RefReflectorId = id, DisplayName = id, Mode = SelectedMode });
                    }
                    SelectedLinkTarget = new ComboBoxItemData() { RefReflectorId = us.Ref.LastReflector, Mode = SelectedMode };
                    LinkTargetToolTip = "Select REF Reflector";
                }
                break;
            case Mode.Xrf:
                {
                    foreach ((string id, string ipAddr) in XrfHosts.All)
                    {
                        LinkTargets.Add(new() { XrfReflectorId = id, DisplayName = id, Mode = SelectedMode });
                    }
                    SelectedLinkTarget = new ComboBoxItemData() { XrfReflectorId = us.Xrf.LastReflector, Mode = SelectedMode };
                    LinkTargetToolTip = "Select XRF Reflector";
                }
                break;
            case Mode.Ysf:
                {
                    var items = YsfHosts.All.Select(kvp => new ComboBoxItemData { YsfDesignator = kvp.Key, DisplayName = kvp.Value.FullName, Mode = SelectedMode })
                        .OrderBy(i => i.DisplayName, StringComparer.Ordinal).ToList();

                    items.ForEach(i => LinkTargets.Add(i));
                    SelectedLinkTarget = new() { YsfDesignator = us.Ysf.LastReflector, Mode = SelectedMode };
                    LinkTargetToolTip = "Select YSF Reflector";
                }
                break;
        }
    }


    private static void OpenUrl(string? url)
    {
        logger.Debug($"url = '{url}'");
        if (string.IsNullOrEmpty(url)) return;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch { }
    }

    private static void ShowAbout()
    {
        logger.Debug("");
        Window? owner = null;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            owner = desktop.MainWindow;
        SettingsService.ShowAboutDialog(owner);
    }

    private static async Task ShowSettings(object? parameter)
    {
        Window? owner = null;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            owner = desktop.MainWindow;
        bool result = await SettingsService.ShowSettingsDialogAsync(owner);
        if (result)
        {
            // Settings saved
        }
        else
        {
            // Settings canceled
        }
    }

    public void SwitchMode(Mode mode)
    {
        Current = mode switch
        {
            Mode.Dcs => DStarViewModel,
            Mode.Dmr => DmrViewModel,
            Mode.Fcs => FusionViewModel,
            Mode.Nxdn => NxdnViewModel,
            Mode.Ref => DStarViewModel,
            Mode.Xrf => DStarViewModel,
            Mode.Ysf => FusionViewModel,
            _ => throw new NotImplementedException($"Mode {mode} is not implemented."),
        };
        OnPropertyChanged(nameof(Current));
    }

    private void StartStopDcs(bool arg)
    {
        logger.Debug($"arg = {arg}");
        if (arg)
        {
            UserSettings us = UserSettings.Instance();
            string? reflectorId = us.Dcs.LastReflector;
            if (string.IsNullOrEmpty(reflectorId))
            {
                logger.Error($"Invalid {nameof(reflectorId)}: {reflectorId}");
                return;
            }

            string? refAddr = DcsHosts.GetAddress(reflectorId);
            DcsClientConfig cfg = new()
            {
                RefAddress = refAddr!,
                RefPort = us.Dcs.HostPort,
                RefName = reflectorId,
                Module = us.Dcs.LastModule,
                Callsign = us.Common.Callsign,
                MicrophoneReader = _microphoneReader,
                AudioPlayer = _audioPlayer,
                RecordRcvdUdpPackets = us.Common.RecordRcvdUdpPackets,
                RecordRcvdUdpPacketsFile = us.Common.RecordRcvdUdpPackets ? Path.Combine(UserSettings.Dir(Mode.Dcs, UserSettings.FileType.Data), $"{SelectedMode}_packets_{DateTime.Now:yyyyMMddHHmmss}.bin") : null,
                UserMessage = us.Dcs.UserMessage,
            };
            _dcsClient = new(cfg) { ExternalDcsDataConsumer = DStarViewModel.ConsumeDStarData, ExternalNetMsgConsumer = ConsumeNetMsg };
            _dcsClient.Start();
        }
        else
        {
            _dcsClient?.Stop();
            _dcsClient = null;
            DStarViewModel.Clear();
        }
    }

    private void StartStopDmr(bool arg)
    {
        logger.Debug($"arg = {arg}");
        UserSettings us = UserSettings.Instance();
        if (arg)
        {
            DmrClientConfig cfg = new()
            {
                Callsign = us.Common.Callsign,
                Password = us.Dmr.Password,
                MyDmrId = us.Dmr.MyDmrId,
                EssId = us.Dmr.Essid,
                Flco = SelectedLinkTarget!.Flco,
                ColorCode = us.Dmr.ColorCode,
                TimeSlot = us.Dmr.TimeSlot,
                RecordAudio = false,
                RecordDmrPackets = us.Common.RecordRcvdUdpPackets,
                MicrophoneReader = _microphoneReader,
                AudioPlayer = _audioPlayer,
            };
            switch (us.Dmr.Protocol)
            {
                case DmrProtocol.Homebrew:
                    cfg.BmServerAddress = us.Dmr.BmServerAddr1;
                    cfg.BmServerPort = us.Dmr.BmServerPort1;
                    cfg.RecordDmrPacketsFile = us.Common.RecordRcvdUdpPackets ? Path.Combine(UserSettings.Dir(Mode.Dmr, UserSettings.FileType.Data), $"DmrClient1_packets_{DateTime.Now:yyyyMMddHHmmss}.bin") : null;
                   
                    _dmrClient1 = new(cfg) { ExternalDmrDataConsumer = DmrViewModel.ConsumeDmrData };
                    _dmrClient1.Start();
                    break;
                case DmrProtocol.MmdvmHost:
                    (_, string Host, int Port, _) = DmrHosts.GetHostInfo(us.Dmr.Master);
                    cfg.BmServerAddress = Host;
                    cfg.BmServerPort = Port;
                    cfg.RecordDmrPacketsFile = us.Common.RecordRcvdUdpPackets ? Path.Combine(UserSettings.Dir(Mode.Dmr, UserSettings.FileType.Data), $"DmrClient2_packets_{DateTime.Now:yyyyMMddHHmmss}.bin") : null;

                    _dmrClient2 = new(cfg) { ExternalDmrDataConsumer = DmrViewModel.ConsumeDmrData };
                    _dmrClient2.Start();
                    break;
            }
        }
        else
        {
            switch (us.Dmr.Protocol)
            {
                case DmrProtocol.Homebrew:
                    _dmrClient1?.Stop();
                    _dmrClient1 = null;
                    break;
                case DmrProtocol.MmdvmHost:
                    _dmrClient2?.Stop();
                    _dmrClient2 = null;
                    break;
            }
            DmrViewModel.Clear();
        }
    }

    private void StartStopFcs(bool arg)
    {
        logger.Debug($"arg = {arg}");
        if (arg)
        {
            UserSettings us = UserSettings.Instance();
            string? reflectorId = SelectedLinkTarget?.FcsReflectorId;
            if (string.IsNullOrEmpty(reflectorId))
            {
                logger.Error($"Invalid {nameof(reflectorId)}: {reflectorId}");
                return;
            }

            FcsClientConfig cfg = new()
            {
                ReflectorAddress = $"{reflectorId[..6].ToLower()}.xreflector.net",
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
                RecordFcsPackets = us.Common.RecordRcvdUdpPackets,
                RecordFcsPacketsFile = us.Common.RecordRcvdUdpPackets ? Path.Combine(UserSettings.Dir(Mode.Fcs, UserSettings.FileType.Data), $"fcs_packets_{DateTime.Now:yyyyMMddHHmmss}.bin") : null,
                SimulationFile = null,
                SimulationMode = false,
                
            };

            _fcsClient = new(cfg) { ExternalFcsDataConsumer = FusionViewModel.ConsumeYsfData };
            _fcsClient.StartClient();
        }
        else
        {
            _fcsClient?.StopClient();
            _fcsClient = null;
            FusionViewModel.Clear();
        }
    }

    private void StartStopNxdn(bool arg)
    {
        logger.Debug($"arg = {arg}");
        if (arg)
        {
            UserSettings us = UserSettings.Instance();
            int reflectorId = us.Nxdn.LastReflectorId;
            var (Host, Port) = NxdnHosts.GetHostInfo( reflectorId );

            NxdnClientConfig cfg = new()
            {
                NxdnReflectorAddr = Host,
                NxdnReflectorPort = Port,
                NxdnReflectorId = reflectorId,
                Callsign = us.Common.Callsign,
                MyNxdnId = us.Nxdn.NxdnId,
               
                AudioPlayer = _audioPlayer,
                MicrophoneReader = _microphoneReader,
                
                RecordAudio = false,
                RecordRxPackets = us.Common.RecordRcvdUdpPackets,
                RecordRxPacketsFile = us.Common.RecordRcvdUdpPackets ? Path.Combine(UserSettings.Dir(Mode.Fcs, UserSettings.FileType.Data), $"nxdn_packets_{DateTime.Now:yyyyMMddHHmmss}.bin") : null,
                SimulationModeFile = null,
                SimulationMode = false,
            };

            _nxdnClient = new(cfg) { ExternalNxdnDataConsumer = NxdnViewModel.ConsumeNxdnData };
            _nxdnClient.StartClient();
        }
        else
        {
            _nxdnClient?.StopClient();
            _nxdnClient = null;
        }
    }

    private void StartStopRef(bool arg)
    {
        logger.Debug($"arg = {arg}");
        if (arg)
        {
            UserSettings us = UserSettings.Instance();
            string? reflectorId = us.Ref.LastReflector;
            if (string.IsNullOrEmpty(reflectorId))
            {
                logger.Error($"Invalid {nameof(reflectorId)}: {reflectorId}");
                return;
            }
            string? refAddr = DPlusHostRepository.GetAddress(reflectorId);
            RefClientConfig cfg = new()
            {
                RefAddress = refAddr!,
                RefPort = us.Ref.HostPort,
                RefName = reflectorId,
                Module = us.Ref.LastModule,
                Callsign = us.Common.Callsign,
                MicrophoneReader = _microphoneReader,
                AudioPlayer = _audioPlayer,
            };
            _refClient = new(cfg) { ExternalRefDataConsumer = DStarViewModel.ConsumeDStarData, ExternalNetMsgConsumer = ConsumeNetMsg };
            _refClient.Start();
        }
        else
        {
            _refClient?.Stop();
            _refClient = null;
            DStarViewModel.Clear();
        }
    }

    private void StartStopXrf(bool arg)
    {
        logger.Debug($"arg = {arg}");
        if (arg)
        {
            UserSettings us = UserSettings.Instance();
            string? reflectorId = us.Xrf.LastReflector;
            if (string.IsNullOrEmpty(reflectorId))
            {
                logger.Error($"Invalid {nameof(reflectorId)}: {reflectorId}");
                return;
            }
            string? refAddr = XrfHosts.GetAddress(reflectorId);
            XrfClientConfig cfg = new()
            {
                RefAddress = refAddr!,
                RefPort = us.Xrf.HostPort,
                RefName = reflectorId,
                Module = us.Xrf.LastModule,
                Callsign = us.Common.Callsign,
                MicrophoneReader = _microphoneReader,
                AudioPlayer = _audioPlayer,
                UserMessage = us.Xrf.UserMessage,
            };
            _xrfClient = new(cfg) { ExternalXrfDataConsumer = DStarViewModel.ConsumeDStarData, ExternalNetMsgConsumer = ConsumeNetMsg };
            _xrfClient.Start();
        }
        else
        {
            _xrfClient?.Stop();
            _xrfClient = null;
            DStarViewModel.Clear();
        }
    }

    private void StartStopYsf(bool arg)
    {
        logger.Debug($"arg = {arg}");
        if (arg)
        {
            UserSettings us = UserSettings.Instance();
            string? key = SelectedLinkTarget?.YsfDesignator;
            (string _, string _, string Host, int Port, string _) = YsfHosts.GetHostInfo(key!);

            YsfClientConfig cfg = new()
            {
                ReflectorAddress = Host,
                ReflectorPort = Port,
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
                RecordYsfPackets= us.Common.RecordRcvdUdpPackets,
                RecordYsfPacketsFile = us.Common.RecordRcvdUdpPackets ? Path.Combine(UserSettings.Dir(Mode.Ysf, UserSettings.FileType.Data), $"ysf_packets_{DateTime.Now:yyyyMMddHHmmss}.bin") : null,
            };

            _ysfClient = new(cfg) { ExternalYsfDataConsumer = FusionViewModel.ConsumeYsfData };
            _ysfClient.StartClient();
        }
        else
        {
            _ysfClient?.StopClient();
            _ysfClient = null;
            FusionViewModel.Clear();
        }
    }
}