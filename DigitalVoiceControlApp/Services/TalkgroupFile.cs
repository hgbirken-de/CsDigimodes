using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoiceControlApp.Config;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DigitalVoiceControlApp.Services;

/// <summary>
/// Zugriff auf die Talkgroup-Datei der DMR-Betriebsart (<c>Dmr\Data\DmrTalkGroups.csv</c> im Datenordner der App): Lesen,
/// Prüfen, Speichern und Neuladen. Gegenstück zu <c>DataFiles</c> der MAUI-App und Grundlage des Talkgroup-Editors.
/// </summary>
public static class TalkgroupFile
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public const string FileName = "DmrTalkGroups.csv";

    /// <summary>Ordner der Datei: <c>&lt;Datenordner&gt;\Dmr\Data</c>.</summary>
    public static string DataDirectory => UserSettings.Dir(Mode.Dmr, UserSettings.FileType.Data);

    /// <summary>Voller Pfad der (vom Anwender bearbeitbaren) Talkgroup-Datei.</summary>
    public static string FilePath => Path.Combine(DataDirectory, FileName);

    /// <summary>
    /// Liest den Text der Datei. Fehlt sie, wird vorher die mitgelieferte Standardliste kopiert
    /// (<see cref="DefaultDataFiles"/>). Gibt es auch diese nicht, ist das Ergebnis nur ein Kommentarkopf.
    /// </summary>
    public static string ReadText()
    {
        DefaultDataFiles.EnsureInstalled(DataDirectory, FileName);

        if (File.Exists(FilePath))
            return File.ReadAllText(FilePath);

        return "# DMRID; Name; Call Type (G -> Group; P -> Private)\r\n";
    }

    /// <summary>Liest die mitgelieferte Standardliste aus der Anwendung (ohne die Datei im Datenordner anzufassen).</summary>
    public static string ReadDefaultText()
    {
        using Stream? input = typeof(TalkgroupFile).Assembly.GetManifestResourceStream(FileName);
        if (input == null)
            throw new FileNotFoundException("The default talk group list is not part of the application.");

        using var reader = new StreamReader(input, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Prüft den Text Zeile für Zeile (Format <c>ID;Name;G|P</c>, Zeilen mit <c>#</c> und Leerzeilen werden übersprungen) mit denselben
    /// Regeln wie <c>DmrTalkgroups.LoadData</c> beim Lesen, nur strenger: Beim Lesen würde eine fehlerhafte Zeile verworfen,
    /// eine ungültige ID sogar die ganze Datei abbrechen.
    /// </summary>
    /// <returns>Fehlermeldungen mit Zeilennummer (leer = in Ordnung).</returns>
    public static List<string> Validate(string text)
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
    /// Schreibt die Datei (UTF-8 mit BOM, Zeilenende CRLF wie die mitgelieferte Datei) über eine temporäre Datei, damit ein
    /// Abbruch sie nicht zerstört. Der Text sollte vorher mit <see cref="Validate"/> geprüft sein.
    /// </summary>
    public static void Save(string text)
    {
        Directory.CreateDirectory(DataDirectory);
        string path = FilePath;
        string tmp = path + ".tmp";

        File.WriteAllText(tmp, ToFileText(text), new UTF8Encoding(true));
        File.Move(tmp, path, overwrite: true);
        logger.Info($"DMR talk group file saved: {path}");
    }

    /// <summary>Wandelt beliebige Zeilenenden des Textes in CRLF um (so wie die Datei sie enthält).</summary>
    public static string ToFileText(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n");

    /// <summary>
    /// Lädt die Talkgroups aus der Datei neu in <c>DmrTalkgroups</c>. Fehler werden protokolliert: Die App läuft auch ohne die
    /// Liste weiter.
    /// </summary>
    /// <returns><c>true</c>, wenn die Liste geladen werden konnte.</returns>
    public static bool Reload()
    {
        try
        {
            DmrTalkgroups.LoadData(FilePath);
            logger.Info($"{DmrTalkgroups.All.Count} DMR talk groups loaded from {FilePath}");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unable to load the DMR talk group file.");
            return false;
        }
    }
}
