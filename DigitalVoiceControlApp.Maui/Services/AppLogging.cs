using NLog;
using NLog.Config;
using NLog.Targets;
using System.IO.Compression;

namespace DigitalVoiceControlApp.Maui.Services;

/// <summary>
/// Logging der App: ein fester Dateiname (<c>logs/app.log</c>) mit begrenzter Größe, umschaltbarer Log-Level zur Laufzeit,
/// Export (Zip) und Löschen der Logdateien. Die Konfiguration steckt hier und nicht mehr in <c>MauiProgram</c>
/// (dort ruft <c>InitializeLogging()</c> nur noch <see cref="Initialize"/> auf).
/// <para>
/// Obergrenze: eine Datei höchstens <see cref="MaxFileBytes"/>, dazu höchstens <see cref="MaxArchiveFiles"/> Archivdateien
/// (die ältesten fallen weg). Der gewählte Level (außer Debug) wird gespeichert. Debug gilt nur für die laufende Sitzung und
/// fällt beim nächsten Start auf Info zurück, damit es nicht versehentlich dauerhaft an bleibt.
/// </para>
/// </summary>
public static class AppLogging
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private const string LevelKey = "LogLevel";
    private const string DefaultLevel = "Info";

    /// <summary>Größe, ab der die aktuelle Logdatei archiviert wird.</summary>
    public const long MaxFileBytes = 2 * 1024 * 1024;

    /// <summary>So viele Archivdateien bleiben erhalten.</summary>
    public const int MaxArchiveFiles = 5;

    /// <summary>Die wählbaren Level, vom leisesten zum gesprächigsten.</summary>
    public static IReadOnlyList<string> LevelNames { get; } = ["Error", "Warning", "Info", "Debug"];

    /// <summary>Der gerade wirksame Level.</summary>
    public static string CurrentLevelName { get; private set; } = DefaultLevel;

    public static string LogDirectory => Path.Combine(FileSystem.AppDataDirectory, "logs");

    // Namen der Logger, die geschrieben werden (die der App und der eigenen Bibliotheken, nicht die von Microsoft.*)
    private static readonly string[] LoggerPatterns = ["DigitalVoiceControlApp.*", "DigitalVoice.*", "Maui.*"];

    /// <summary>Richtet das Logging beim Start der App ein (gespeicherter Level, Debug wird zu Info).</summary>
    public static void Initialize()
    {
        string stored = Preferences.Default.Get(LevelKey, DefaultLevel);
        string level = LevelNames.Contains(stored) && stored != "Debug" ? stored : DefaultLevel;
        Configure(level);
    }

    private static LogLevel ToNLogLevel(string name) => name switch
    {
        "Error" => LogLevel.Error,
        "Warning" => LogLevel.Warn,
        "Debug" => LogLevel.Debug,
        _ => LogLevel.Info,
    };

    private static void Configure(string levelName)
    {
        Directory.CreateDirectory(LogDirectory);
        CurrentLevelName = levelName;

        const string layout = "${longdate} ${uppercase:${level}} ${threadid} ${callsite}(): ${message} ${exception:format=toString,StackTrace}";

        var config = new LoggingConfiguration();

        var fileTarget = new FileTarget("file")
        {
            FileName = Path.Combine(LogDirectory, "app.log"),
            Layout = layout,
            ArchiveAboveSize = MaxFileBytes,
            MaxArchiveFiles = MaxArchiveFiles,
        };

#if DEBUG
        // Nur beim Entwickeln: Ausgabe zusätzlich in Konsole/Debugger (Visual Studio, logcat)
        var consoleTarget = new ConsoleTarget("console") { Layout = layout };
        var debuggerTarget = new DebuggerTarget("debugger") { Layout = layout };
#endif

        LogLevel min = ToNLogLevel(levelName);
        foreach (string pattern in LoggerPatterns)
        {
            config.AddRule(min, LogLevel.Fatal, fileTarget, pattern);
#if DEBUG
            config.AddRule(min, LogLevel.Fatal, consoleTarget, pattern);
            config.AddRule(min, LogLevel.Fatal, debuggerTarget, pattern);
#endif
        }

        LogManager.Configuration = config; // schließt dabei die Ziele einer vorherigen Konfiguration
    }

    /// <summary>
    /// Schaltet den Log-Level sofort um (ohne Neustart). Error, Warning und Info werden gespeichert, Debug gilt nur für diese
    /// Sitzung.
    /// </summary>
    public static void SetLevel(string levelName)
    {
        if (!LevelNames.Contains(levelName) || levelName == CurrentLevelName)
            return;

        Log.Info($"Log level changes from {CurrentLevelName} to {levelName}.");

        LogLevel min = ToNLogLevel(levelName);
        LoggingConfiguration? config = LogManager.Configuration;
        if (config != null)
        {
            foreach (LoggingRule rule in config.LoggingRules)
                rule.SetLoggingLevels(min, LogLevel.Fatal);
            LogManager.ReconfigExistingLoggers();
        }

        CurrentLevelName = levelName;
        if (levelName != "Debug")
            Preferences.Default.Set(LevelKey, levelName);

        Log.Info($"Log level is now {levelName}.");
    }

    /// <summary>Alle Dateien im Logordner (aktuelle Datei, Archive und Dateien früherer Versionen).</summary>
    private static List<FileInfo> GetLogFiles() =>
        Directory.Exists(LogDirectory)
            ? new DirectoryInfo(LogDirectory).GetFiles().OrderByDescending(f => f.LastWriteTimeUtc).ToList()
            : new List<FileInfo>();

    /// <summary>Anzahl und Gesamtgröße der Logdateien.</summary>
    public static (int Count, long Bytes) GetLogSummary()
    {
        LogManager.Flush();
        List<FileInfo> files = GetLogFiles();
        return (files.Count, files.Sum(f => f.Length));
    }

    /// <summary>
    /// Packt alle Logdateien in eine Zip-Datei im Cache-Ordner (zum Teilen). Die laufende Datei wird dabei gelesen, ohne
    /// sie zu sperren.
    /// </summary>
    /// <returns>Pfad der Zip-Datei oder <c>null</c>, wenn es keine Logdateien gibt.</returns>
    public static string? CreateExportZip()
    {
        LogManager.Flush();
        List<FileInfo> files = GetLogFiles();
        if (files.Count == 0)
            return null;

        // frühere Exporte aufräumen
        foreach (string old in Directory.GetFiles(FileSystem.CacheDirectory, "DotNetDigimodes-logs-*.zip"))
        {
            try { File.Delete(old); } catch (IOException) { /* nicht schlimm */ }
        }

        string zipPath = Path.Combine(FileSystem.CacheDirectory, $"DotNetDigimodes-logs-{DateTime.Now:yyyyMMdd-HHmmss}.zip");
        using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (FileInfo file in files)
            {
                ZipArchiveEntry entry = zip.CreateEntry(file.Name, CompressionLevel.Optimal);
                using var source = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using Stream target = entry.Open();
                source.CopyTo(target);
            }
        }

        return zipPath;
    }

    /// <summary>
    /// Löscht alle Logdateien. Die Ziele werden dafür kurz geschlossen und danach mit dem aktuellen Level neu geöffnet,
    /// damit die laufende Datei nicht offen gelöscht wird.
    /// </summary>
    /// <returns>Anzahl der gelöschten Dateien.</returns>
    public static int DeleteLogs()
    {
        LogManager.Flush();
        string level = CurrentLevelName;

        LogManager.Configuration = new LoggingConfiguration(); // schließt die Logdatei

        int deleted = 0;
        foreach (FileInfo file in GetLogFiles())
        {
            try
            {
                file.Delete();
                deleted++;
            }
            catch (IOException) { /* wird beim nächsten Mal erneut versucht */ }
        }

        Configure(level);
        Log.Info($"Log files deleted: {deleted}");
        return deleted;
    }
}
