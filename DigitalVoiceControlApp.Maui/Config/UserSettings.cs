using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoice.Fusion;
using DigitalVoice.Nxdn;
using NLog;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace DigitalVoiceControlApp.Maui.Config;

/// <summary>
/// Benutzereinstellungen der Android-App, gespeichert als YAML in <c>FileSystem.AppDataDirectory</c>.
/// <para>
/// Bewusst eine eigene, MAUI-spezifische Klasse (keine Abhängigkeit zur Avalonia-App). Abschnitts- und
/// Property-Namen entsprechen aber der Desktop-<c>UserSettings</c>, und beim Laden werden unbekannte
/// Einträge ignoriert: Eine Desktop-Datei <c>UserSettings.yaml</c> lässt sich deshalb importieren
/// (<see cref="Import"/>), fehlende Werte bleiben auf den Standardwerten.
/// </para>
/// <para>
/// Gegenüber dem Desktop fehlen bewusst: <c>Ambe.StickBaudrate</c> und alles in <c>Gui</c> außer <c>Theme</c>.
/// </para>
/// </summary>
public class UserSettings
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static readonly object _sync = new();
    private static UserSettings? _instance;

    /// <summary>
    /// Basisordner der App (Gegenstück zu <c>%APPDATA%\CsDigimodes</c> unter Windows): privat für diese App,
    /// unter Android <c>/data/user/0/&lt;Paket&gt;/files</c>. Dort liegen auch UserSettings.yaml und logs.
    /// </summary>
    public static string HomeDir { get; } = FileSystem.AppDataDirectory;

    /// <summary>Pfad der Einstellungsdatei.</summary>
    public static string FileName { get; } = Path.Combine(HomeDir, "UserSettings.yaml");

    /// <summary>Art der Dateien in einem Mode-Ordner (wie in der Desktop-App).</summary>
    public enum FileType { Audio, Data }

    private static readonly Dictionary<FileType, string> AppSpecificFolder = new() { [FileType.Audio] = "audio", [FileType.Data] = "Data" };

    private static bool _foldersCreated;

    /// <summary>
    /// Legt die Ordnerstruktur wie unter Windows an (falls nicht vorhanden): <c>logs</c> und je Mode die Ordner
    /// <c>audio</c> und <c>Data</c>.
    /// </summary>
    private static void EnsureFolders()
    {
        lock (_sync)
        {
            if (_foldersCreated)
                return;

            Directory.CreateDirectory(Path.Combine(HomeDir, "logs")); // das Logging schreibt hierhin
            foreach (Mode mode in Enum.GetValues<Mode>())
            {
                foreach (string folder in AppSpecificFolder.Values)
                    Directory.CreateDirectory(Path.Combine(HomeDir, mode.ToString(), folder));
            }

            _foldersCreated = true;
        }
    }

    /// <summary>Liefert den Ordner für eine Dateiart eines Modes, z.B. <c>.../files/Dmr/Data</c> (legt die Struktur bei Bedarf an).</summary>
    public static string Dir(Mode mode, FileType fileType)
    {
        EnsureFolders();
        return Path.Combine(HomeDir, mode.ToString(), AppSpecificFolder[fileType]);
    }

    public AmbeSettings Ambe { get; set; } = new();
    public CommonSettings Common { get; set; } = new();
    public DcsSettings Dcs { get; set; } = new();
    public DmrSettings Dmr { get; set; } = new();
    public FcsSettings Fcs { get; set; } = new();
    public GuiSettings Gui { get; set; } = new();
    public HotspotSettings Hotspot { get; set; } = new();
    public NxdnSettings Nxdn { get; set; } = new();
    public RefSettings Ref { get; set; } = new();
    public XrfSettings Xrf { get; set; } = new();
    public YsfSettings Ysf { get; set; } = new();

    // ------------------------------------------------------------------
    // Laden / Speichern
    // ------------------------------------------------------------------

    /// <summary>Liefert die einzige Instanz (lädt die Datei beim ersten Aufruf; fehlt sie, gelten die Standardwerte).</summary>
    public static UserSettings Instance()
    {
        lock (_sync)
        {
            return _instance ??= Load();
        }
    }

    /// <summary>Schreibt die aktuellen Einstellungen in die Datei (über eine temporäre Datei, damit ein Abbruch sie nicht zerstört).</summary>
    public static void Save()
    {
        lock (_sync)
        {
            if (_instance == null)
                return; // nichts geladen, nichts zu speichern

            string yaml = new SerializerBuilder().Build().Serialize(_instance);
            string tmp = FileName + ".tmp";
            File.WriteAllText(tmp, yaml);
            File.Move(tmp, FileName, overwrite: true);
            logger.Debug($"Settings saved to {FileName}");
        }
    }

    /// <summary>Übernimmt eine (z.B. im Settings-Dialog bearbeitete) Kopie als aktuelle Einstellungen und speichert sie.</summary>
    public static void Apply(UserSettings settings)
    {
        settings.Normalize();
        lock (_sync)
        {
            _instance = settings;
        }
        Save();
    }

    /// <summary>
    /// Ersetzt die aktuellen Einstellungen durch den Inhalt einer YAML-Datei (z.B. der Desktop-<c>UserSettings.yaml</c>)
    /// und speichert sie. Unbekannte Einträge werden ignoriert.
    /// </summary>
    /// <exception cref="FormatException">Die Datei ist leer oder kein gültiges YAML.</exception>
    public static UserSettings Import(string yamlText)
    {
        UserSettings imported;
        try
        {
            imported = Deserialize(yamlText) ?? throw new FormatException("The file contains no settings.");
        }
        catch (FormatException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FormatException($"The file is not a valid settings YAML: {ex.Message}", ex);
        }

        imported.Normalize();

        Apply(imported);
        return imported;
    }

    /// <summary>Tiefe Kopie (z.B. als Arbeitskopie für den Settings-Dialog, damit "Discard" nichts verändert).</summary>
    public UserSettings Clone()
    {
        string yaml = new SerializerBuilder().Build().Serialize(this);
        UserSettings clone = Deserialize(yaml) ?? new UserSettings();
        clone.Normalize();
        return clone;
    }

    private static UserSettings? Deserialize(string yaml) =>
        new DeserializerBuilder().IgnoreUnmatchedProperties().Build().Deserialize<UserSettings>(yaml);

    private static UserSettings Load()
    {
        try
        {
            EnsureFolders();

            if (!File.Exists(FileName))
            {
                logger.Info("No settings file yet, using defaults.");
                return new UserSettings();
            }

            UserSettings? loaded = Deserialize(File.ReadAllText(FileName));
            if (loaded != null)
            {
                loaded.Normalize();
                logger.Debug($"Settings loaded from {FileName}");
                return loaded;
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Unable to read {FileName}, using defaults.");
            try { File.Copy(FileName, FileName + ".bad", overwrite: true); } // nicht stillschweigend überschreiben
            catch (Exception copyEx) { logger.Warn(copyEx, "Could not keep a copy of the unreadable settings file."); }
        }

        return new UserSettings();
    }

    /// <summary>Stellt sicher, dass kein Abschnitt <c>null</c> ist (z.B. wenn die YAML-Datei nur "Dmr:" enthält).</summary>
    private void Normalize()
    {
        Ambe ??= new();
        Common ??= new();
        Dcs ??= new();
        Dmr ??= new();
        Fcs ??= new();
        Gui ??= new();
        Hotspot ??= new();
        Nxdn ??= new();
        Ref ??= new();
        Xrf ??= new();
        Ysf ??= new();

        // Mikrofon-/Lautstärkewerte sind dB-Werte der Slider (-40..+30). Werte außerhalb (z.B. der Desktop-Vorgabewert 50)
        // werden auf die Grenzen geklemmt. Das entspricht der Desktop-App: Dort setzt der Slider (Maximum 30) einen
        // Wert von 50 auf 30 und schreibt ihn zurück, die Desktop-Wiedergabe läuft dann tatsächlich mit +30 dB.
        Common.MicGain = SanitizeGain(Common.MicGain);
        Common.RxVolume = SanitizeGain(Common.RxVolume);
        Dcs.MicGain = SanitizeGain(Dcs.MicGain);   Dcs.RxVolume = SanitizeGain(Dcs.RxVolume);
        Dmr.MicGain = SanitizeGain(Dmr.MicGain);   Dmr.RxVolume = SanitizeGain(Dmr.RxVolume);
        Fcs.MicGain = SanitizeGain(Fcs.MicGain);   Fcs.RxVolume = SanitizeGain(Fcs.RxVolume);
        Nxdn.MicGain = SanitizeGain(Nxdn.MicGain); Nxdn.RxVolume = SanitizeGain(Nxdn.RxVolume);
        Ref.MicGain = SanitizeGain(Ref.MicGain);   Ref.RxVolume = SanitizeGain(Ref.RxVolume);
        Xrf.MicGain = SanitizeGain(Xrf.MicGain);   Xrf.RxVolume = SanitizeGain(Xrf.RxVolume);
        Ysf.MicGain = SanitizeGain(Ysf.MicGain);   Ysf.RxVolume = SanitizeGain(Ysf.RxVolume);
    }

    /// <summary>Bereich der Mikrofon-/Lautstärke-Slider in dB (wie in der Desktop-App).</summary>
    public const double GainMinDb = -40;
    public const double GainMaxDb = 30;

    private static double SanitizeGain(double db) => double.IsNaN(db) ? 0 : Math.Clamp(db, GainMinDb, GainMaxDb);

    // ------------------------------------------------------------------
    // Validierung
    // ------------------------------------------------------------------

    private static readonly Regex LocatorPattern = new("^[A-Ra-r]{2}[0-9]{2}([A-Xa-x]{2}([0-9]{2})?)?$", RegexOptions.Compiled);

    private static void CheckRange(List<string> errors, string name, double value, double min, double max)
    {
        if (value < min || value > max)
            errors.Add($"{name} must be between {min} and {max}.");
    }

    /// <summary>
    /// Prüft alle Werte auf gültige Bereiche (nicht, ob sie für einen Verbindungsaufbau vollständig sind, siehe
    /// <see cref="ValidateForDmr"/>). Die Regeln entsprechen denen der Desktop-Eingabefelder bzw. <c>DmrClientConfig.Validate()</c>.
    /// </summary>
    /// <returns>Fehlermeldungen (leer = alles in Ordnung).</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        // Common
        if (string.IsNullOrWhiteSpace(Common.Callsign) || Common.Callsign.Contains(' '))
            errors.Add("Callsign is missing or contains spaces.");
        if (!string.IsNullOrWhiteSpace(Common.Locator) && !LocatorPattern.IsMatch(Common.Locator.Trim()))
            errors.Add("Locator is invalid (e.g. JO54 or JO54OL).");
        CheckRange(errors, "Microphone (general, dB)", Common.MicGain, GainMinDb, GainMaxDb);
        CheckRange(errors, "Volume (general, dB)", Common.RxVolume, GainMinDb, GainMaxDb);

        // DMR
        CheckRange(errors, "DMR ID", Dmr.MyDmrId, 1, 9999999);
        CheckRange(errors, "ESSID", Dmr.Essid, 0, 99);
        CheckRange(errors, "Color Code", Dmr.ColorCode, 1, 15);
        CheckRange(errors, "Timeslot", Dmr.TimeSlot, 1, 2);
        CheckRange(errors, "BM server port", Dmr.BmServerPort1, 1025, 65535);
        if (string.IsNullOrWhiteSpace(Dmr.BmServerAddr1))
            errors.Add("BM server address is missing.");
        if (string.IsNullOrWhiteSpace(Dmr.Master))
            errors.Add("No DMR master selected.");
        if (Dmr.LastTgInUse < 0)
            errors.Add("Talkgroup must not be negative.");
        CheckRange(errors, "Microphone (DMR, dB)", Dmr.MicGain, GainMinDb, GainMaxDb);
        CheckRange(errors, "Volume (DMR, dB)", Dmr.RxVolume, GainMinDb, GainMaxDb);

        // NXDN
        CheckRange(errors, "NXDN ID", Nxdn.NxdnId, 1, 65535);

        // FCS
        CheckRange(errors, "FCS port", Fcs.Port, 1025, 65535);
        if (string.IsNullOrWhiteSpace(Fcs.Master))
            errors.Add("No FCS master selected.");

        // D-STAR / YSF-Ports
        CheckRange(errors, "DCS port", Dcs.HostPort, 1025, 65535);
        CheckRange(errors, "REF port", Ref.HostPort, 1025, 65535);
        CheckRange(errors, "XRF port", Xrf.HostPort, 1025, 65535);

        // AMBE
        if (Ambe.ServiceType == AmbeServiceType.Server)
        {
            if (string.IsNullOrWhiteSpace(Ambe.ServerAddr))
                errors.Add("AMBE server address is missing.");
            else if (!System.Net.IPAddress.TryParse(Ambe.ServerAddr.Trim(), out _))
                errors.Add("AMBE server address must be an IP address (a host name is not supported).");
            CheckRange(errors, "AMBE server port", Ambe.ServerPort, 1025, 65535);
        }

        return errors;
    }

    /// <summary>
    /// Wie <see cref="Validate"/>, zusätzlich die Angaben, ohne die ein DMR-Verbindungsaufbau keinen Sinn hat:
    /// echtes Rufzeichen (nicht NOCALL) und ein gesetztes BrandMeister-Passwort.
    /// </summary>
    public List<string> ValidateForDmr()
    {
        var errors = Validate();

        if (Common.Callsign.Equals("NOCALL", StringComparison.OrdinalIgnoreCase))
            errors.Add("Callsign is still set to NOCALL.");
        if (string.IsNullOrWhiteSpace(Dmr.Password) || Dmr.Password == "unknown")
            errors.Add("BrandMeister password is not set.");

        return errors;
    }

    /// <summary>
    /// Wie <see cref="Validate"/>, zusätzlich die Angaben, ohne die ein NXDN-Verbindungsaufbau keinen Sinn hat: echtes
    /// Rufzeichen (nicht NOCALL, höchstens 10 Zeichen wie in <c>NxdnClientConfig.Validate()</c>) und ein bekannter Reflektor.
    /// </summary>
    public List<string> ValidateForNxdn()
    {
        var errors = Validate();

        if (Common.Callsign.Equals("NOCALL", StringComparison.OrdinalIgnoreCase))
            errors.Add("Callsign is still set to NOCALL.");
        if (Common.Callsign.Length > 10)
            errors.Add("Callsign must not be longer than 10 characters.");
        if (!NxdnHosts.TryGetHostInfo(Nxdn.LastReflectorId, out _))
            errors.Add($"NXDN reflector {Nxdn.LastReflectorId} is unknown. Please select one in the list.");

        return errors;
    }

    /// <summary>
    /// Wie <see cref="Validate"/>, zusätzlich die Angaben, ohne die ein YSF-Verbindungsaufbau keinen Sinn hat. Die Grenzen
    /// stammen aus dem YSFI-Paket des Clients (<c>SendYsfi</c>): Rufzeichen höchstens 10 Zeichen, Locator höchstens 6, Ort
    /// höchstens 20, Hotspot-Typ höchstens 12 Zeichen, Frequenzen höchstens 9 Stellen.
    /// </summary>
    public List<string> ValidateForYsf()
    {
        var errors = Validate();

        if (Common.Callsign.Equals("NOCALL", StringComparison.OrdinalIgnoreCase))
            errors.Add("Callsign is still set to NOCALL.");
        if (Common.Callsign.Length > 10)
            errors.Add("Callsign must not be longer than 10 characters.");
        if (!YsfHosts.TryGetHostInfo(Ysf.LastReflector ?? "", out _))
            errors.Add($"YSF reflector '{Ysf.LastReflector}' is unknown. Please select one in the list.");
        if ((Common.Locator ?? "").Length > 6)
            errors.Add("For YSF the locator must not be longer than 6 characters.");
        if ((Common.Town ?? "").Length > 20)
            errors.Add("For YSF the town must not be longer than 20 characters.");
        if ((Hotspot.Type ?? "").Length > 12)
            errors.Add("For YSF the hotspot type must not be longer than 12 characters.");
        CheckRange(errors, "Hotspot RX frequency", Hotspot.RxFrequency, 0, 999999999);
        CheckRange(errors, "Hotspot TX frequency", Hotspot.TxFrequency, 0, 999999999);

        return errors;
    }

    /// <summary>
    /// Wie <see cref="Validate"/>, zusätzlich die Angaben, ohne die ein FCS-Verbindungsaufbau keinen Sinn hat. Die Grenzen
    /// stammen aus <c>FcsClientConfig</c>: Das Rufzeichen darf höchstens 6 Zeichen haben (sonst wirft der Setter), die
    /// Reflektor-ID muss 8 Zeichen lang sein und in der Liste vorkommen (aus ihr ergibt sich auch der Server).
    /// </summary>
    public List<string> ValidateForFcs()
    {
        var errors = Validate();

        if (Common.Callsign.Equals("NOCALL", StringComparison.OrdinalIgnoreCase))
            errors.Add("Callsign is still set to NOCALL.");
        if (Common.Callsign.Length > 6)
            errors.Add("For FCS the callsign must not be longer than 6 characters.");

        string reflectorId = Fcs.LastReflector ?? "";
        if (reflectorId.Length != 8 || !FcsHosts.TryGetHostInfo(reflectorId, out _))
            errors.Add($"FCS reflector '{reflectorId}' is unknown. Please select one in the list.");

        return errors;
    }

    /// <summary>
    /// Wie <see cref="Validate"/>, zusätzlich die Angaben, ohne die ein D-STAR-Verbindungsaufbau (DCS, REF, XRF) keinen Sinn
    /// hat: Rufzeichen (nicht NOCALL, höchstens 8 Zeichen wie das D-STAR-Feld), ein gewählter Reflektor, ein Modul A bis Z
    /// und eine Nachricht von höchstens 20 Zeichen. Ob der Reflektor in der Hostliste steht, prüft der Client-Aufbau.
    /// </summary>
    public List<string> ValidateForDStar(Mode mode)
    {
        var errors = Validate();
        string name = mode.ToString().ToUpperInvariant();

        string callsign = (Common.Callsign ?? "").Trim();
        if (callsign.Equals("NOCALL", StringComparison.OrdinalIgnoreCase))
            errors.Add("Callsign is still set to NOCALL.");
        if (callsign.Length > 8)
            errors.Add("For D-STAR the callsign must not be longer than 8 characters.");

        (string reflector, char module, string message) = mode switch
        {
            Mode.Dcs => (Dcs.LastReflector, Dcs.LastModule, Dcs.UserMessage),
            Mode.Ref => (Ref.LastReflector, Ref.LastModule, Ref.UserMessage),
            Mode.Xrf => (Xrf.LastReflector, Xrf.LastModule, Xrf.UserMessage),
            _ => ("", ' ', ""),
        };

        if (string.IsNullOrWhiteSpace(reflector))
            errors.Add($"No {name} reflector selected.");

        char upper = char.ToUpperInvariant(module);
        if (upper < 'A' || upper > 'Z')
            errors.Add($"Invalid {name} module '{module}'. Please select a module from A to Z.");

        if ((message ?? "").Length > 20)
            errors.Add($"The {name} user message must not be longer than 20 characters.");

        return errors;
    }
}

// ----------------------------------------------------------------------
// Abschnitte (Namen und Standardwerte wie in der Desktop-UserSettings)
// ----------------------------------------------------------------------

public class AmbeSettings
{
    public string ServerAddr { get; set; } = "127.0.0.1";
    public int ServerPort { get; set; } = 2460;
    public AmbeServiceType ServiceType { get; set; } = AmbeServiceType.Stick; // Android: USB-Stick
    public string StickPort { get; set; } = "COM9"; // nur Desktop (unter Android nicht benutzt)
}

public class CommonSettings
{
    public string Callsign { get; set; } = "NOCALL";
    public string Language { get; set; } = "en-US";
    public Mode LastMode { get; set; } = Mode.Fcs;
    public string Locator { get; set; } = "JO54OL";
    public double MicGain { get; set; } = 0; // dB
    public string Name { get; set; } = "Unknown";
    public string Theme { get; set; } = "Dark"; // "Dark" oder "Light"
    public bool RecordRcvdUdpPackets { get; set; } = false;
    public double RxVolume { get; set; } = 0; // dB
    public bool TextToSpeech { get; set; } = false;
    public string Town { get; set; } = "Unknown";
}

public class DcsSettings
{
    public string LastReflector { get; set; } = "DCS001";
    public char LastModule { get; set; } = 'C';
    public int HostPort { get; set; } = 30051;
    public string UserMessage { get; set; } = "DVC by DL1HGB";
    public double MicGain { get; set; } = 0; // dB
    public double RxVolume { get; set; } = 0; // dB
}

public class DmrSettings
{
    public int MyDmrId { get; set; } = 2622363;
    public int Essid { get; set; } = 15;
    public string Password { get; set; } = "unknown";
    public DmrProtocol Protocol { get; set; } = DmrProtocol.MmdvmHost;
    public int LastTgInUse { get; set; } = 262997;
    public string BmServerAddr1 { get; set; } = "master1.bm262.de"; // for Homebrew
    public int BmServerPort1 { get; set; } = 62030; // for Homebrew
    public string Master { get; set; } = "BM_2621_Germany";
    public int TimeSlot { get; set; } = 1;
    public int ColorCode { get; set; } = 1;
    public double MicGain { get; set; } = 0; // dB
    public double RxVolume { get; set; } = 0; // dB
}

public class FcsSettings
{
    public string Master { get; set; } = "FCS001 (DE)";
    public int Port { get; set; } = 62500;
    public string LastReflector { get; set; } = "FCS00199";
    public double MicGain { get; set; } = 0; // dB
    public double RxVolume { get; set; } = 0; // dB
}

/// <summary>Nur das Theme (alles andere im Desktop-Abschnitt "Gui" ist fensterspezifisch).</summary>
public class GuiSettings
{
    public string Theme { get; set; } = "Dark"; // "Dark" oder "Light"
}

public class HotspotSettings
{
    public int RxFrequency { get; set; } = 434300000;
    public int TxFrequency { get; set; } = 434300000;
    public string Type { get; set; } = "DVC";
}

public class NxdnSettings
{
    public int NxdnId { get; set; } = 39251;
    public int LastReflectorId { get; set; } = 20000;
    public double MicGain { get; set; } = 0; // dB
    public double RxVolume { get; set; } = 0; // dB
}

public class RefSettings
{
    public string LastReflector { get; set; } = "REF000";
    public char LastModule { get; set; } = 'A';
    public int HostPort { get; set; } = 20001;
    public string UserMessage { get; set; } = "DVC by DL1HGB";
    public double MicGain { get; set; } = 0; // dB
    public double RxVolume { get; set; } = 0; // dB
}

public class XrfSettings
{
    public string LastReflector { get; set; } = "XRF002";
    public char LastModule { get; set; } = 'C';
    public int HostPort { get; set; } = 30001;
    public string UserMessage { get; set; } = "DVC by DL1HGB";
    public double MicGain { get; set; } = 0; // dB
    public double RxVolume { get; set; } = 0; // dB
}

public class YsfSettings
{
    public string LastReflector { get; set; } = "99999";
    public double MicGain { get; set; } = 0; // dB
    public double RxVolume { get; set; } = 0; // dB
}
