using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
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

    /// <summary>Pfad der Einstellungsdatei.</summary>
    public static string FileName { get; } = Path.Combine(FileSystem.AppDataDirectory, "UserSettings.yaml");

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
            imported = Deserialize(yamlText) ?? throw new FormatException("Die Datei enthält keine Einstellungen.");
        }
        catch (FormatException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Die Datei ist kein gültiges Settings-YAML: {ex.Message}", ex);
        }

        imported.Normalize();

        // Unter Android gibt es (noch) nur den USB-Stick; ein "Server" aus der Desktop-Datei wäre hier unbenutzbar.
        imported.Ambe.ServiceType = AmbeServiceType.Stick;

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

        // Mikrofon-/Lautstärkewerte sind dB-Werte der Slider (-40..+30). Werte außerhalb (z.B. der Desktop-Vorgabewert 50,
        // der nie über einen Slider gesetzt wurde) würden extrem laut/leise machen -> auf 0 dB (unverändert) zurücksetzen.
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

    private static double SanitizeGain(double db) => db is >= GainMinDb and <= GainMaxDb ? db : 0;

    // ------------------------------------------------------------------
    // Validierung
    // ------------------------------------------------------------------

    private static readonly Regex LocatorPattern = new("^[A-Ra-r]{2}[0-9]{2}([A-Xa-x]{2}([0-9]{2})?)?$", RegexOptions.Compiled);

    private static void CheckRange(List<string> errors, string name, double value, double min, double max)
    {
        if (value < min || value > max)
            errors.Add($"{name} muss zwischen {min} und {max} liegen.");
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
            errors.Add("Rufzeichen fehlt oder enthält Leerzeichen.");
        if (!string.IsNullOrWhiteSpace(Common.Locator) && !LocatorPattern.IsMatch(Common.Locator.Trim()))
            errors.Add("Locator ist ungültig (z.B. JO54 oder JO54OL).");
        CheckRange(errors, "Mikrofon (Allgemein, dB)", Common.MicGain, GainMinDb, GainMaxDb);
        CheckRange(errors, "Lautstärke (Allgemein, dB)", Common.RxVolume, GainMinDb, GainMaxDb);

        // DMR
        CheckRange(errors, "DMR-ID", Dmr.MyDmrId, 1, 9999999);
        CheckRange(errors, "ESSID", Dmr.Essid, 0, 99);
        CheckRange(errors, "Color Code", Dmr.ColorCode, 1, 15);
        CheckRange(errors, "Timeslot", Dmr.TimeSlot, 1, 2);
        CheckRange(errors, "BM-Server-Port", Dmr.BmServerPort1, 1025, 65535);
        if (string.IsNullOrWhiteSpace(Dmr.BmServerAddr1))
            errors.Add("BM-Server-Adresse fehlt.");
        if (string.IsNullOrWhiteSpace(Dmr.Master))
            errors.Add("Es ist kein DMR-Master ausgewählt.");
        if (Dmr.LastTgInUse < 0)
            errors.Add("Talkgroup darf nicht negativ sein.");
        CheckRange(errors, "Mikrofon (DMR, dB)", Dmr.MicGain, GainMinDb, GainMaxDb);
        CheckRange(errors, "Lautstärke (DMR, dB)", Dmr.RxVolume, GainMinDb, GainMaxDb);

        // NXDN
        CheckRange(errors, "NXDN-ID", Nxdn.NxdnId, 1, 65535);

        // FCS
        CheckRange(errors, "FCS-Port", Fcs.Port, 1025, 65535);
        if (string.IsNullOrWhiteSpace(Fcs.Master))
            errors.Add("Es ist kein FCS-Master ausgewählt.");

        // D-STAR / YSF-Ports
        CheckRange(errors, "DCS-Port", Dcs.HostPort, 1025, 65535);
        CheckRange(errors, "REF-Port", Ref.HostPort, 1025, 65535);
        CheckRange(errors, "XRF-Port", Xrf.HostPort, 1025, 65535);

        // AMBE
        if (Ambe.ServiceType == AmbeServiceType.Server)
        {
            if (string.IsNullOrWhiteSpace(Ambe.ServerAddr))
                errors.Add("AMBE-Server-Adresse fehlt.");
            CheckRange(errors, "AMBE-Server-Port", Ambe.ServerPort, 1025, 65535);
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
            errors.Add("Rufzeichen ist noch auf NOCALL gesetzt.");
        if (string.IsNullOrWhiteSpace(Dmr.Password) || Dmr.Password == "unknown")
            errors.Add("BrandMeister-Passwort ist nicht gesetzt.");

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
