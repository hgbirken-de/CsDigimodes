using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalVoice.AmbeSupport;
using DigitalVoice.Dmr;
using DigitalVoice.Fusion;
using DigitalVoiceControlApp.Maui.Config;

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// ViewModel des Settings-Dialogs (Gegenstück zum Avalonia-<c>SettingsDialogModel</c>). Arbeitet auf einer Kopie der
/// <see cref="UserSettings"/>: "Discard" verwirft die Kopie, "Apply" prüft sie und übernimmt sie erst, wenn alles gültig ist.
/// Zahlenfelder sind Strings, damit ungültige Eingaben mit einer Meldung abgelehnt werden können (statt sie still zu ignorieren).
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private UserSettings _working = UserSettings.Instance().Clone();

    // ---- Auswahllisten ----
    public List<string> LanguageList { get; } = ["de-DE", "en-GB", "en-US"];
    public List<DmrProtocol> DmrProtocolList { get; } = [DmrProtocol.Homebrew, DmrProtocol.MmdvmHost];
    public List<string> DmrMasterList { get; } = DmrHosts.All.Keys.ToList();
    public List<int> TimeSlotList { get; } = [1, 2];
    public List<int> ColorCodeList { get; } = Enumerable.Range(1, 15).ToList();
    public List<string> FcsMasterList { get; } = FcsMasters.All.Keys.ToList();

    // ---- Common ----
    [ObservableProperty] private string callsign = "";
    [ObservableProperty] private string locator = "";
    [ObservableProperty] private string town = "";
    [ObservableProperty] private string selectedLanguage = "en-US";
    [ObservableProperty] private bool textToSpeech;
    [ObservableProperty] private bool recordRcvdUdpPackets;

    // ---- DMR ----
    [ObservableProperty] private string password = "";

    /// <summary>true = Passwort wird als Punkte angezeigt, false = im Klartext (Auge im Dialog).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordToggleText))]
    private bool isPasswordHidden = true;

    /// <summary>Symbol auf dem Auge-Knopf: Auge = "anzeigen", durchgestrichen/verdeckt = "verbergen".</summary>
    public string PasswordToggleText => IsPasswordHidden ? "👁" : "🙈";

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

    [ObservableProperty] private string myDmrId = "";
    [ObservableProperty] private string essid = "";
    [ObservableProperty] private DmrProtocol selectedDmrProtocol;
    [ObservableProperty] private string bmServerAddr1 = "";
    [ObservableProperty] private string bmServerPort1 = "";
    [ObservableProperty] private string? selectedDmrMaster;
    [ObservableProperty] private int selectedTimeSlot;
    [ObservableProperty] private int selectedColorCode;

    // ---- NXDN ----
    [ObservableProperty] private string nxdnId = "";

    // ---- FCS ----
    [ObservableProperty] private string? selectedFcsMaster;
    [ObservableProperty] private string fcsPort = "";

    // ---- AMBE ----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStick))]
    [NotifyPropertyChangedFor(nameof(IsServer))]
    private AmbeServiceType ambeServiceType;

    [ObservableProperty] private string ambeServerAddr = "";
    [ObservableProperty] private string ambeServerPort = "";

    public bool IsStick
    {
        get => AmbeServiceType == AmbeServiceType.Stick;
        set { if (value) AmbeServiceType = AmbeServiceType.Stick; }
    }

    public bool IsServer
    {
        get => AmbeServiceType == AmbeServiceType.Server;
        set { if (value) AmbeServiceType = AmbeServiceType.Server; }
    }

    public SettingsViewModel()
    {
        Load();
    }

    /// <summary>Füllt die Eingabefelder aus den aktuellen Einstellungen (verwirft ungespeicherte Änderungen).</summary>
    public void Load()
    {
        _working = UserSettings.Instance().Clone();
        UserSettings us = _working;

        // Common
        Callsign = us.Common.Callsign;
        Locator = us.Common.Locator;
        Town = us.Common.Town;
        SelectedLanguage = us.Common.Language;
        TextToSpeech = us.Common.TextToSpeech;
        RecordRcvdUdpPackets = us.Common.RecordRcvdUdpPackets;

        // DMR
        Password = us.Dmr.Password;
        MyDmrId = us.Dmr.MyDmrId.ToString();
        Essid = us.Dmr.Essid.ToString();
        SelectedDmrProtocol = us.Dmr.Protocol;
        BmServerAddr1 = us.Dmr.BmServerAddr1;
        BmServerPort1 = us.Dmr.BmServerPort1.ToString();
        SelectedDmrMaster = us.Dmr.Master;
        SelectedTimeSlot = us.Dmr.TimeSlot;
        SelectedColorCode = us.Dmr.ColorCode;

        // NXDN
        NxdnId = us.Nxdn.NxdnId.ToString();

        // FCS
        SelectedFcsMaster = us.Fcs.Master;
        FcsPort = us.Fcs.Port.ToString();

        // AMBE
        AmbeServiceType = us.Ambe.ServiceType;
        AmbeServerAddr = us.Ambe.ServerAddr;
        AmbeServerPort = us.Ambe.ServerPort.ToString();
    }

    /// <summary>
    /// Prüft die Eingaben und übernimmt sie (Speichern in die Datei) nur, wenn alles gültig ist.
    /// </summary>
    /// <param name="errors">Fehlermeldungen für den Anwender, wenn die Rückgabe <c>false</c> ist.</param>
    public bool TrySave(out List<string> errors)
    {
        errors = [];
        UserSettings us = _working;

        // Common
        us.Common.Callsign = Callsign.Trim().ToUpperInvariant();
        us.Common.Locator = Locator.Trim().ToUpperInvariant();
        us.Common.Town = Town.Trim();
        us.Common.Language = SelectedLanguage;
        us.Common.TextToSpeech = TextToSpeech;
        us.Common.RecordRcvdUdpPackets = RecordRcvdUdpPackets;

        // DMR
        us.Dmr.Password = Password;
        if (TryParse(MyDmrId, "DMR ID", errors, out int dmrId)) us.Dmr.MyDmrId = dmrId;
        if (TryParse(Essid, "ESSID", errors, out int essidValue)) us.Dmr.Essid = essidValue;
        us.Dmr.Protocol = SelectedDmrProtocol;
        us.Dmr.BmServerAddr1 = BmServerAddr1.Trim();
        if (TryParse(BmServerPort1, "BM server port", errors, out int bmPort)) us.Dmr.BmServerPort1 = bmPort;
        us.Dmr.Master = SelectedDmrMaster ?? "";
        us.Dmr.TimeSlot = SelectedTimeSlot;
        us.Dmr.ColorCode = SelectedColorCode;

        // NXDN
        if (TryParse(NxdnId, "NXDN ID", errors, out int nxdn)) us.Nxdn.NxdnId = nxdn;

        // FCS
        us.Fcs.Master = SelectedFcsMaster ?? "";
        if (TryParse(FcsPort, "FCS port", errors, out int fcsPortValue)) us.Fcs.Port = fcsPortValue;

        // AMBE
        us.Ambe.ServiceType = AmbeServiceType;
        us.Ambe.ServerAddr = AmbeServerAddr.Trim();
        if (TryParse(AmbeServerPort, "AMBE server port", errors, out int ambePort)) us.Ambe.ServerPort = ambePort;

        if (errors.Count == 0)
            errors.AddRange(us.Validate());

        if (errors.Count > 0)
            return false;

        UserSettings.Apply(us);
        _working = UserSettings.Instance().Clone(); // frische Arbeitskopie, falls der Dialog offen bleibt
        return true;
    }

    /// <summary>Übernimmt den Inhalt einer YAML-Datei (z.B. Desktop-UserSettings.yaml) als Einstellungen und zeigt ihn an.</summary>
    /// <exception cref="FormatException">Die Datei ist kein gültiges Settings-YAML.</exception>
    public void Import(string yamlText)
    {
        UserSettings.Import(yamlText);
        Load();
    }

    private static bool TryParse(string text, string name, List<string> errors, out int value)
    {
        if (int.TryParse(text.Trim(), out value))
            return true;

        errors.Add($"{name}: please enter a whole number.");
        return false;
    }
}
