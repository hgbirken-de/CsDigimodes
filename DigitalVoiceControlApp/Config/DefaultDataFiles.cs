using NLog;
using System;
using System.IO;

namespace DigitalVoiceControlApp.Config;

/// <summary>
/// Standard-Datendateien, die beim ersten Start in die Datenordner der App kopiert werden (z. B. <c>DmrTalkGroups.csv</c>).
/// Die Dateien stecken als eingebettete Ressource in der Assembly und kommen damit unabhängig davon an, wie die App gestartet
/// wird (Entwicklungsumgebung, publish-Ordner, Installer). Eine vorhandene Datei wird <b>nie</b> überschrieben, damit
/// Änderungen des Anwenders erhalten bleiben.
/// </summary>
public static class DefaultDataFiles
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Kopiert die eingebettete Ressource <paramref name="fileName"/> nach <paramref name="targetDirectory"/>, falls die Datei dort
    /// fehlt. Fehler werden protokolliert und nicht weitergegeben: Die App startet auch ohne die Datei.
    /// </summary>
    /// <returns><c>true</c>, wenn die Datei danach vorhanden ist.</returns>
    public static bool EnsureInstalled(string targetDirectory, string fileName)
    {
        try
        {
            string target = Path.Combine(targetDirectory, fileName);
            if (File.Exists(target))
                return true;

            using Stream? input = typeof(DefaultDataFiles).Assembly.GetManifestResourceStream(fileName);
            if (input == null)
            {
                logger.Warn($"Default file '{fileName}' is not embedded in the application: '{target}' cannot be created.");
                return false;
            }

            Directory.CreateDirectory(targetDirectory);
            using (FileStream output = File.Create(target))
            {
                input.CopyTo(output);
            }

            logger.Info($"Default file '{fileName}' copied to '{target}'.");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Default file '{fileName}' could not be installed into '{targetDirectory}'.");
            return false;
        }
    }
}
