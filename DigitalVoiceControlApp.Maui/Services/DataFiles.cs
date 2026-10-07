using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoiceControlApp.Maui.Config;
using NLog;
using System.Text;

namespace DigitalVoiceControlApp.Maui.Services;

/// <summary>
/// Datendateien der App (Gegenstück zum Laden in <c>MainView.axaml.cs</c> der Desktop-App). Die Dateien liegen wie unter
/// Windows in <c>&lt;HomeDir&gt;/&lt;Mode&gt;/Data</c>. Fehlt eine Datei, wird die im App-Paket mitgelieferte Standarddatei
/// (<c>Resources/Raw</c>, als Android-Asset) dorthin kopiert; eine vorhandene Datei wird nie überschrieben.
/// </summary>
public static class DataFiles
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public const string DmrTalkGroupsFile = "DmrTalkGroups.csv";

    /// <summary>Pfad der (vom Benutzer bearbeitbaren) Talkgroup-Datei: <c>Dmr/Data/DmrTalkGroups.csv</c>.</summary>
    public static string DmrTalkGroupsPath() =>
        Path.Combine(UserSettings.Dir(Mode.Dmr, UserSettings.FileType.Data), DmrTalkGroupsFile);

    /// <summary>Liest den Text der Talkgroup-Datei; fehlt sie, wird vorher die Standarddatei aus dem App-Paket kopiert.</summary>
    public static string ReadDmrTalkGroupsText()
    {
        string path = DmrTalkGroupsPath();
        EnsureDefaultFile(path, DmrTalkGroupsFile);
        return File.ReadAllText(path);
    }

    /// <summary>Liest den Text der mitgelieferten Standarddatei aus dem App-Paket (ohne die Datei im Ordner anzufassen).</summary>
    public static string ReadDefaultDmrTalkGroupsText()
    {
#if ANDROID
        using Stream input = Android.App.Application.Context.Assets!.Open(DmrTalkGroupsFile);
        using var reader = new StreamReader(input, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
#else
        throw new PlatformNotSupportedException("Available on Android only.");
#endif
    }

    /// <summary>
    /// Prüft den Text einer Talkgroup-Datei Zeile für Zeile (Format <c>ID;Name;G|P</c>, Zeilen mit <c>#</c> und Leerzeilen
    /// werden übersprungen), mit denselben Regeln, die <c>DmrTalkgroups.LoadData</c> beim Lesen anwendet, nur strenger:
    /// Dort würde eine fehlerhafte Zeile verworfen bzw. eine ungültige ID die ganze Datei abbrechen.
    /// </summary>
    /// <returns>Fehlermeldungen mit Zeilennummer (leer = in Ordnung).</returns>
    public static List<string> ValidateDmrTalkGroups(string text)
    {
        var errors = new List<string>();
        string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim().TrimStart('\uFEFF');
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            string[] parts = line.Split(';');
            if (parts.Length != 3)
            {
                errors.Add($"Line {i + 1}: expected ID;Name;G or P, found: \"{line}\"");
                continue;
            }

            if (!int.TryParse(parts[0].Trim(), out int id) || id < 1 || id > 16777215)
                errors.Add($"Line {i + 1}: invalid DMR ID \"{parts[0].Trim()}\" (allowed: 1 to 16777215).");

            if (parts[1].Trim().Length == 0)
                errors.Add($"Line {i + 1}: the name is missing.");

            string callType = parts[2].Trim().ToUpperInvariant();
            if (callType != "G" && callType != "P")
                errors.Add($"Line {i + 1}: the call type must be G (group) or P (private), found: \"{parts[2].Trim()}\".");
        }

        return errors;
    }

    /// <summary>
    /// Schreibt die Talkgroup-Datei (UTF-8 mit BOM, Zeilenende CRLF wie die mitgelieferte Datei) über eine temporäre Datei,
    /// damit ein Abbruch sie nicht zerstört. Der Text sollte vorher mit <see cref="ValidateDmrTalkGroups"/> geprüft sein.
    /// </summary>
    public static void SaveDmrTalkGroups(string text)
    {
        string path = DmrTalkGroupsPath();
        string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n");
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, normalized, new UTF8Encoding(true));
        File.Move(tmp, path, overwrite: true);
        Log.Info($"DMR talk group file saved: {path}");
    }

    /// <summary>Lädt die DMR-Talkgroups (<c>DmrTalkgroups.All</c>) aus <c>Dmr/Data/DmrTalkGroups.csv</c>.</summary>
    public static void LoadDmrTalkgroups()
    {
        try
        {
            string path = Path.Combine(UserSettings.Dir(Mode.Dmr, UserSettings.FileType.Data), DmrTalkGroupsFile);
            EnsureDefaultFile(path, DmrTalkGroupsFile);
            DmrTalkgroups.LoadData(path);
            Log.Info($"{DmrTalkgroups.All.Count} DMR talk groups loaded from {path}");
        }
        catch (Exception ex)
        {
            // Die App läuft ohne Talkgroup-Liste weiter
            Log.Error(ex, "Unable to load the DMR talk group file.");
        }
    }

    /// <summary>Kopiert die Standarddatei aus dem App-Paket nach <paramref name="targetPath"/>, falls sie dort fehlt.</summary>
    private static void EnsureDefaultFile(string targetPath, string assetName)
    {
        if (File.Exists(targetPath))
            return;

#if ANDROID
        using Stream input = Android.App.Application.Context.Assets!.Open(assetName);
        using FileStream output = File.Create(targetPath);
        input.CopyTo(output);
        Log.Info($"Default file {assetName} copied to {targetPath}");
#else
        throw new FileNotFoundException($"File missing: {targetPath}");
#endif
    }
}
