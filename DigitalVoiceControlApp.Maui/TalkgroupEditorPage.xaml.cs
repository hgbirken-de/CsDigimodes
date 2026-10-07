using DigitalVoiceControlApp.Maui.Services;
using NLog;
using System.Text;

namespace DigitalVoiceControlApp.Maui;

/// <summary>
/// Editor für die DMR-Talkgroup-Liste (<c>Dmr/Data/DmrTalkGroups.csv</c>). Speichern prüft jede Zeile und übernimmt die
/// Liste erst, wenn alles gültig ist. "Standard" lädt die mitgelieferte Liste in den Editor (gespeichert wird erst mit
/// "Speichern"), "Export…" gibt den Editorinhalt über das Android-Teilen-Fenster aus (z.B. nach Drive oder Downloads),
/// "Import…" liest eine Datei in den Editor (so überlebt die Liste eine Neuinstallation der App).
/// </summary>
public partial class TalkgroupEditorPage : ContentPage
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly Action? _onSaved;
    private string _originalText = "";
    private string? _loadError;

    /// <param name="onSaved">Wird nach erfolgreichem Speichern aufgerufen (lädt die Liste neu und füllt den Picker).</param>
    public TalkgroupEditorPage(Action? onSaved = null)
    {
        InitializeComponent();
        _onSaved = onSaved;

        try
        {
            _originalText = DataFiles.ReadDmrTalkGroupsText();
            TextEditor.Text = _originalText;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unable to read the talk group file.");
            _loadError = $"{ex.GetType().Name}: {ex.Message}";
        }

        // Der Zurück-Pfeil der Titelzeile soll bei ungespeicherten Änderungen nachfragen
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior { Command = new Command(async () => await RequestCloseAsync()) });
    }

    private bool HasChanges => (TextEditor.Text ?? "") != _originalText;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_loadError != null)
        {
            string message = _loadError;
            _loadError = null;
            await DisplayAlert("Talkgroups", $"The file could not be read:\n{message}", "OK");
        }
    }

    // Die Zurück-Taste des Geräts
    protected override bool OnBackButtonPressed()
    {
        if (!HasChanges)
            return base.OnBackButtonPressed();

        _ = RequestCloseAsync();
        return true; // die Seite wird erst nach der Rückfrage geschlossen
    }

    private async Task RequestCloseAsync()
    {
        try
        {
            if (HasChanges && !await DisplayAlert("Discard changes?", "The list has not been saved.", "Discard", "Keep editing"))
                return;

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Closing the talk group editor failed.");
        }
    }

    private static string FormatErrors(List<string> errors)
    {
        const int max = 10;
        string text = string.Join("\n", errors.Take(max));
        return errors.Count > max ? $"{text}\n… and {errors.Count - max} more" : text;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            string text = TextEditor.Text ?? "";

            List<string> errors = DataFiles.ValidateDmrTalkGroups(text);
            if (errors.Count > 0)
            {
                await DisplayAlert("Check your input", FormatErrors(errors), "OK");
                return;
            }

            DataFiles.SaveDmrTalkGroups(text);
            _originalText = text;
            _onSaved?.Invoke(); // Liste neu laden, Picker neu füllen

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Saving the talk group file failed.");
            await DisplayAlert("Error", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e) => await RequestCloseAsync();

    private async void OnDefaultClicked(object? sender, EventArgs e)
    {
        try
        {
            bool replace = await DisplayAlert("Load default list",
                "The editor content will be replaced by the bundled default list. Nothing is saved until you tap \"Save\".",
                "Replace", "Cancel");
            if (!replace)
                return;

            TextEditor.Text = DataFiles.ReadDefaultDmrTalkGroupsText();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Loading the default talk group list failed.");
            await DisplayAlert("Error", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        try
        {
            // Exportiert wird der aktuelle Editorinhalt (nicht nur die gespeicherte Fassung)
            string path = Path.Combine(FileSystem.CacheDirectory, DataFiles.DmrTalkGroupsFile);
            File.WriteAllText(path, TextEditor.Text ?? "", new UTF8Encoding(true));

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = DataFiles.DmrTalkGroupsFile,
                File = new ShareFile(path),
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exporting the talk group file failed.");
            await DisplayAlert("Export failed", ex.Message, "OK");
        }
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select DmrTalkGroups.csv" });
            if (file == null)
                return; // abgebrochen

            using Stream stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            string text = await reader.ReadToEndAsync();

            TextEditor.Text = text; // erst mit "Speichern" übernehmen

            List<string> errors = DataFiles.ValidateDmrTalkGroups(text);
            if (errors.Count == 0)
                await DisplayAlert("Import", "The file was read. Tap \"Save\" to apply it.", "OK");
            else
                await DisplayAlert("Import", $"The file was read but contains errors. Please correct them:\n{FormatErrors(errors)}", "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Importing the talk group file failed.");
            await DisplayAlert("Import failed", ex.Message, "OK");
        }
    }
}
