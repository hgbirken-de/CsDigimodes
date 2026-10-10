using System.Threading.Tasks;

namespace DigitalVoiceControlApp.ViewModels;

/// <summary>
/// Was das Modell des Talkgroup-Editors vom Fenster braucht: Meldungen, Rückfragen, Dateiauswahl und das Schließen. So kennt das
/// Modell keine Oberflächenklassen und lässt sich ohne Fenster prüfen.
/// </summary>
internal interface ITalkgroupEditorHost
{
    /// <summary>Zeigt eine Meldung mit OK.</summary>
    Task ShowMessageAsync(string title, string message);

    /// <summary>Stellt eine Ja/Nein-Frage; <c>true</c> bei "Ja".</summary>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>Lässt den Anwender eine Datei wählen und liefert ihren Text, oder <c>null</c> bei Abbruch.</summary>
    Task<string?> PickImportTextAsync();

    /// <summary>Lässt den Anwender ein Ziel wählen und schreibt den Text (UTF-8 mit BOM) dorthin. <c>false</c> bei Abbruch.</summary>
    Task<bool> ExportTextAsync(string text);

    /// <summary>Schließt den Dialog mit dem Ergebnis.</summary>
    void Close(bool result);
}
