using DigitalVoiceControlApp.Commands;
using DigitalVoiceControlApp.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

/// <summary>
/// Modell des Talkgroup-Editors (<c>TalkgroupEditorDialog</c>) für die Datei <c>Dmr\Data\DmrTalkGroups.csv</c>. Es hält den Text und
/// die Befehle (Speichern, Abbrechen, Standard, Import, Export); alles, was eine Oberfläche braucht (Meldungen, Dateiauswahl,
/// Schließen), geht über <see cref="ITalkgroupEditorHost"/>. Gespeichert wird erst, wenn jede Zeile gültig ist.
/// </summary>
internal class TalkgroupEditorDialogModel : ViewModelBase
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly ITalkgroupEditorHost _host;
    private readonly Action? _onSaved;
    private string _originalText = "";

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DefaultCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand ExportCommand { get; }

    /// <summary>Fehlertext, wenn die Datei beim Öffnen nicht gelesen werden konnte (sonst <c>null</c>).</summary>
    public string? LoadError { get; }

    string _text = "";

    /// <summary>Der Inhalt des Editors.</summary>
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
                OnPropertyChanged(nameof(HasChanges));
            }
        }
    }

    /// <summary>Hat sich der Text gegenüber der gespeicherten Fassung geändert (Zeilenenden zählen nicht)?</summary>
    public bool HasChanges => Normalize(Text) != Normalize(_originalText);

    /// <param name="host">Das Fenster.</param>
    /// <param name="onSaved">Wird nach erfolgreichem Speichern aufgerufen (lädt die Liste neu und füllt die Auswahl neu).</param>
    public TalkgroupEditorDialogModel(ITalkgroupEditorHost host, Action? onSaved = null)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _onSaved = onSaved;

        SaveCommand = new RelayCommand<object?>(async _ => await SaveAsync());
        CancelCommand = new RelayCommand<object?>(_ => _host.Close(false));
        DefaultCommand = new RelayCommand<object?>(async _ => await LoadDefaultAsync());
        ImportCommand = new RelayCommand<object?>(async _ => await ImportAsync());
        ExportCommand = new RelayCommand<object?>(async _ => await ExportAsync());

        try
        {
            _originalText = TalkgroupFile.ReadText();
            Text = _originalText;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unable to read the talk group file.");
            LoadError = $"{ex.GetType().Name}: {ex.Message}";
        }
    }

    /// <summary>Fragt vor dem Schließen nach, wenn es ungespeicherte Änderungen gibt. <c>true</c> = schließen.</summary>
    public async Task<bool> ConfirmCloseAsync()
    {
        if (!HasChanges)
            return true;

        return await _host.ConfirmAsync("Discard changes?", "The list has not been saved.");
    }

    private async Task SaveAsync()
    {
        try
        {
            List<string> errors = TalkgroupFile.Validate(Text);
            if (errors.Count > 0)
            {
                await _host.ShowMessageAsync("Check your input", FormatErrors(errors));
                return;
            }

            TalkgroupFile.Save(Text);
            _originalText = Text;
            OnPropertyChanged(nameof(HasChanges));
            _onSaved?.Invoke();   // Liste neu laden, Auswahl neu füllen

            _host.Close(true);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Saving the talk group file failed.");
            await _host.ShowMessageAsync("Error", $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private async Task LoadDefaultAsync()
    {
        try
        {
            bool replace = await _host.ConfirmAsync("Load default list",
                "The editor content will be replaced by the bundled default list. Nothing is saved until you click \"Save\".");
            if (!replace)
                return;

            Text = TalkgroupFile.ReadDefaultText();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Loading the default talk group list failed.");
            await _host.ShowMessageAsync("Error", $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private async Task ImportAsync()
    {
        try
        {
            string? text = await _host.PickImportTextAsync();
            if (text == null)
                return;   // abgebrochen

            Text = text;   // erst mit "Save" übernehmen

            List<string> errors = TalkgroupFile.Validate(text);
            if (errors.Count == 0)
                await _host.ShowMessageAsync("Import", "The file was read. Click \"Save\" to apply it.");
            else
                await _host.ShowMessageAsync("Import", $"The file was read but contains errors. Please correct them:\n{FormatErrors(errors)}");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Importing the talk group file failed.");
            await _host.ShowMessageAsync("Import failed", ex.Message);
        }
    }

    private async Task ExportAsync()
    {
        try
        {
            // Exportiert wird der aktuelle Editorinhalt (nicht nur die gespeicherte Fassung)
            await _host.ExportTextAsync(TalkgroupFile.ToFileText(Text));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Exporting the talk group file failed.");
            await _host.ShowMessageAsync("Export failed", ex.Message);
        }
    }

    private static string Normalize(string? text) => (text ?? "").Replace("\r\n", "\n").Replace('\r', '\n');

    private static string FormatErrors(List<string> errors)
    {
        const int max = 10;
        string text = string.Join("\n", errors.Take(max));
        return errors.Count > max ? $"{text}\n… and {errors.Count - max} more" : text;
    }
}
