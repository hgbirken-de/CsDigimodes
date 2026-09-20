using Avalonia.Controls.ApplicationLifetimes;
using DigitalVoice.Config;
using DigitalVoice.AmbeSupport;
using DigitalVoice.Dmr;
using DigitalVoice.Fusion;
using DigitalVoiceControlApp.Commands;
using DigitalVoiceControlApp.Views;
using DynamicData;
using NLog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

internal class SettingsDialogModel : ViewModelBase
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    #region "General Properties"

    string _callsign = string.Empty;
    public string Callsign
    {
        get => _callsign;
        set
        {
            if (_callsign != value)
            {
                _callsign = value.ToUpper();
                OnPropertyChanged(nameof(Callsign));
            }
        }
    }

    string _town = string.Empty;
    public string Town
    {
        get => _town;
        set
        {
            if (_town != value)
            {
                _town = value;
                OnPropertyChanged();
            }
        }
    }

    string _locator = string.Empty;
    public string Locator
    {
        get => _locator;
        set
        {
            // TODO: verify locator
            if (_locator != value)
            {
                _locator = value;
                OnPropertyChanged();
            }
        }
    }

    public bool RecordRcvdUdpPackets { get; set; }

    public bool TextToSpeech { get; set; }

    public string SelectedLanguage { get; set; }

    #endregion

    #region "DMR Properties"

    public ObservableCollection<DmrProtocol> DmrProtocolList { get; } = [DmrProtocol.Homebrew, DmrProtocol.MmdvmHost];
    public DmrProtocol SelectedDmrProtocol { get; set; }
    
    public ObservableCollection<int> ColorCodeList { get; } = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
    int SelectedColorCode { get; set; }

    string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged();
            }
        }
    }

    string _myDmrId = string.Empty;
    public string MyDmrId
    {
        get => _myDmrId;
        set
        {
            if (IsDigitOnly(value) && int.TryParse(value, out int iValue) && iValue <= 9999999)
            {
                _myDmrId = value; // valid input
                OnPropertyChanged(); // forces TextBox update
            }
            else
            {
                //throw new DataValidationException("Invalid DMR Id");
            }
        }
    }

    string _essid = string.Empty;
    public string Essid
    {
        get => _essid;
        set
        {
            if (IsDigitOnly(value) && int.TryParse(value, out int iValue) && iValue <= 99)
            {
                _essid = value; // valid input
                OnPropertyChanged();
            }
            else
            {
                //throw new DataValidationException("Invalid ESSID");
            }
        }
    }

    int SelectedTimeSlot { get; set; }

    string _bmServerAddr1 = string.Empty;
    public string BmServerAddr1
    {
        get => _bmServerAddr1;
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                _bmServerAddr1 = value;
                OnPropertyChanged();
            }
            else
            {
                //throw new DataValidationException("Invalid Srv Addr");
            }
        }
    }

    string _bmServerPort1 = string.Empty;
    public string BmServerPort1
    {
        get => _bmServerPort1;
        set
        {
            if (IsDigitOnly(value) && int.TryParse(value, out int iValue) && iValue > 1024 && iValue <= 65535)
            {
                _bmServerPort1 = value; // valid input
                OnPropertyChanged();
            }
            else
            {
                //throw new DataValidationException("Invalid Srv Port");
            }
        }
    }

    string _bmMaster = string.Empty;
    public string BmMaster
    {
        get => _bmMaster;
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                _bmMaster = value;
                OnPropertyChanged();
            }
            else
            {
                //throw new DataValidationException("Invalid Srv Addr");
            }
        }
    }
    public ObservableCollection<string> DmrMasterList { get; } = [];
    public string SelectedDmrMaster { get; set; }

    #endregion

    #region "NXDN Properties"
    string _nxdnId = string.Empty;
    public string NxdnId
    {
        get => _nxdnId;
        set
        {
            if (IsDigitOnly(value) && int.TryParse(value, out int iValue) && iValue <= 9999999)
            {
                _nxdnId = value; // valid input
                OnPropertyChanged(); // forces TextBox update
            }
            else
            {
                //throw new DataValidationException("Invalid NXDN Id");
            }
        }
    }
    #endregion

    #region "FCS Properties"

    public ObservableCollection<string> FcsReflectorNameList { get; } = [];
    public string SelectedFcsReflectorName { get; set; }

    string _fcsReflectorPort = string.Empty;
    public string FcsReflectorPort
    {
        get => _fcsReflectorPort;
        set
        {
            if (IsDigitOnly(value) && int.TryParse(value, out int iValue) && iValue > 1024 && iValue <= 65535)
            {
                _fcsReflectorPort = value; // valid input
                OnPropertyChanged();
            }
            else
            {
                //throw new DataValidationException("Invalid Port");
            }
        }
    }

    #endregion

    #region "AMBE Data"

    private AmbeServiceType _ambeServiceType;
    public AmbeServiceType AmbeServiceType
    {
        get => _ambeServiceType;
        set
        {
            if (_ambeServiceType != value)
            {
                _ambeServiceType = value;
                OnPropertyChanged(nameof(AmbeServiceType));
                OnPropertyChanged(nameof(IsServer));
                OnPropertyChanged(nameof(IsStick));
            }
        }
    }

    public bool IsServer
    {
        get => AmbeServiceType == AmbeServiceType.Server;
        set
        {
            if (value) AmbeServiceType = AmbeServiceType.Server;
        }
    }

    public bool IsStick
    {
        get => AmbeServiceType == AmbeServiceType.Stick;
        set
        {
            if (value) AmbeServiceType = AmbeServiceType.Stick;
        }
    }

    public string AmbeServerAddr { get; set; } = string.Empty;
    
    string _ambeServerPort = string.Empty;
    public string AmbeServerPort
    {
        get => _ambeServerPort;
        set
        {
            if (IsDigitOnly(value) && int.TryParse(value, out int iValue) && iValue > 1024 && iValue <= 65535)
            {
                _ambeServerPort = value; // valid input
                OnPropertyChanged();
            }
            else
            {
                //throw new DataValidationException("Invalid Port");
            }
        }
    }
    

    public ObservableCollection<string> AmbeStickPorts { get; } = [];

    public string SelectedAmbeStickPort { get; set; }

    public int SelectedAmbeStickBaudrate { get; set; }


    #endregion

    /// <summary>
    /// Constructor.
    /// </summary>
    public SettingsDialogModel()
    {
        OkCommand = new RelayCommand<object?>(_ => OnOk());
        CancelCommand = new RelayCommand<object?>(_ => OnCancel());

        // Load all FCS master
        FcsReflectorNameList.AddRange(FcsMasters.All.Keys);

        UserSettings us = UserSettings.Instance();

        // General
        Callsign = us.Common.Callsign;
        SelectedLanguage = us.Common.Language;
        Locator = us.Common.Locator;
        Town = us.Common.Town;
        TextToSpeech = us.Common.TextToSpeech;
        RecordRcvdUdpPackets = us.Common.RecordRcvdUdpPackets;

        // DMR
        Password = us.Dmr.Password;
        MyDmrId = us.Dmr.MyDmrId.ToString();
        Essid = us.Dmr.Essid.ToString();
        BmServerAddr1 = us.Dmr.BmServerAddr1;
        BmServerPort1 = us.Dmr.BmServerPort1.ToString();
        
        SelectedDmrProtocol = us.Dmr.Protocol;
        
        SelectedDmrMaster = us.Dmr.Master;
        SelectedColorCode = us.Dmr.ColorCode;
        SelectedTimeSlot = us.Dmr.TimeSlot;
        
        DmrHosts.All.Keys.ToList().ForEach(x => DmrMasterList.Add(x));

        // NXDN
        NxdnId = us.Nxdn.NxdnId.ToString();

        // FCS
        SelectedFcsReflectorName = us.Fcs.Master;
        FcsReflectorPort = us.Fcs.Port.ToString();

        // YSF

        // AMBE
        AmbeServiceType = us.Ambe.ServiceType;

        for (int i = 0; i < 20; i++)
        {
            AmbeStickPorts.Add($"COM{i}");
        }
        SelectedAmbeStickPort = us.Ambe.StickPort;
        SelectedAmbeStickBaudrate = us.Ambe.StickBaudrate;

        AmbeServerAddr = us.Ambe.ServerAddr;
        AmbeServerPort = us.Ambe.ServerPort.ToString();

    }

    /// <summary>
    /// Determines whether a given string contains only digit characters ('0'-'9').
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <returns><c>true</c> if <paramref name="s"/> is not null, not empty, and consists entirely of digit characters; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Returns <c>false</c> for null or empty strings.
    /// </remarks>

    public static bool IsDigitOnly(string s)
    {
        return !string.IsNullOrEmpty(s) && s.All(char.IsDigit);
    }

    private void OnOk()
    {
        UserSettings us = UserSettings.Instance();

        // General
        us.Common.Callsign = Callsign;
        us.Common.Locator = Locator;
        us.Common.Town = Town;
        us.Common.Language = SelectedLanguage;
        us.Common.TextToSpeech = TextToSpeech;
        us.Common.RecordRcvdUdpPackets = RecordRcvdUdpPackets;

        // DMR
        us.Dmr.Password = Password;
        us.Dmr.MyDmrId = int.Parse(MyDmrId);
        us.Dmr.Essid = int.Parse(Essid);
        us.Dmr.Protocol = SelectedDmrProtocol;
        us.Dmr.BmServerAddr1 = BmServerAddr1;
        us.Dmr.BmServerPort1 = int.Parse(BmServerPort1);
        us.Dmr.Master = SelectedDmrMaster;
        us.Dmr.ColorCode = SelectedColorCode;
        us.Dmr.TimeSlot = SelectedTimeSlot;

        // NXDN
        us.Nxdn.NxdnId = int.Parse(NxdnId);

        // FCS
        us.Fcs.Master = SelectedFcsReflectorName;
        us.Fcs.Port = int.Parse(FcsReflectorPort);

        // AMBE
        us.Ambe.ServiceType = AmbeServiceType;
        us.Ambe.ServerAddr = AmbeServerAddr;
        us.Ambe.ServerPort = int.Parse(AmbeServerPort);
        us.Ambe.StickPort = SelectedAmbeStickPort;
        us.Ambe.StickBaudrate = SelectedAmbeStickBaudrate;

        UserSettings.Save();

        // Close dialog with true result
        CloseDialog(true);
    }

    private void OnCancel()
    {
        // Close dialog with false result
        CloseDialog(false);
    }

    private static void CloseDialog(bool result)
    {
        logger.Debug($"{result}");
        // Access the window through DataContext or another mechanism
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.Windows.OfType<SettingsDialog>().FirstOrDefault();
            window?.Close(result);
        }
    }

}
