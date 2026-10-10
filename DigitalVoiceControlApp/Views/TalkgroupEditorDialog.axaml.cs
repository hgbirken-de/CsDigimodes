using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DigitalVoiceControlApp.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DigitalVoiceControlApp.Views;

/// <summary>
/// Editor für die DMR-Talkgroup-Liste (<c>Dmr\Data\DmrTalkGroups.csv</c>). Die Logik steckt im <see cref="TalkgroupEditorDialogModel"/>;
/// das Fenster stellt ihm Meldungen, Rückfragen, die Dateiauswahl und das Schließen zur Verfügung
/// (<see cref="ITalkgroupEditorHost"/>) und fragt beim Schließen mit ungespeicherten Änderungen nach.
/// </summary>
public partial class TalkgroupEditorDialog : Window, ITalkgroupEditorHost
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly TalkgroupEditorDialogModel _model;
    private bool _closeConfirmed;

    /// <summary>Für den Designer.</summary>
    public TalkgroupEditorDialog() : this(null)
    {
    }

    /// <param name="onSaved">Wird nach erfolgreichem Speichern aufgerufen (lädt die Liste neu und füllt die Auswahl neu).</param>
    public TalkgroupEditorDialog(Action? onSaved)
    {
        InitializeComponent();

        _model = new TalkgroupEditorDialogModel(this, onSaved);
        DataContext = _model;

        if (_model.LoadError != null)
        {
            string error = _model.LoadError;
            Opened += async (_, _) => await ((ITalkgroupEditorHost)this).ShowMessageAsync("Talkgroups", $"The file could not be read:\n{error}");
        }
    }

    // ------------------------------------------------------------------
    // Schließen: bei ungespeicherten Änderungen erst nachfragen
    // ------------------------------------------------------------------

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (_closeConfirmed || !_model.HasChanges)
            return;

        e.Cancel = true;   // Das Fenster bleibt offen, bis die Antwort da ist
        try
        {
            if (await _model.ConfirmCloseAsync())
            {
                _closeConfirmed = true;
                Close(false);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Closing the talk group editor failed.");
        }
    }

    // ------------------------------------------------------------------
    // ITalkgroupEditorHost (explizit, damit die Methoden nicht öffentlich werden)
    // ------------------------------------------------------------------

    async Task ITalkgroupEditorHost.ShowMessageAsync(string title, string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok,
            windowStartupLocation: WindowStartupLocation.CenterOwner);
        await box.ShowWindowDialogAsync(this);
    }

    async Task<bool> ITalkgroupEditorHost.ConfirmAsync(string title, string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNo,
            windowStartupLocation: WindowStartupLocation.CenterOwner);
        return await box.ShowWindowDialogAsync(this) == ButtonResult.Yes;
    }

    async Task<string?> ITalkgroupEditorHost.PickImportTextAsync()
    {
        IReadOnlyList<IStorageFile> files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select DmrTalkGroups.csv",
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("Talkgroup lists") { Patterns = ["*.csv", "*.txt"] }, FilePickerFileTypes.All],
        });
        if (files.Count == 0)
            return null;   // abgebrochen

        await using Stream stream = await files[0].OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync();
    }

    async Task<bool> ITalkgroupEditorHost.ExportTextAsync(string text)
    {
        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export talk groups",
            SuggestedFileName = "DmrTalkGroups.csv",
            DefaultExtension = "csv",
            FileTypeChoices = [new FilePickerFileType("Talkgroup lists") { Patterns = ["*.csv"] }],
        });
        if (file == null)
            return false;   // abgebrochen

        await using Stream stream = await file.OpenWriteAsync();
        stream.SetLength(0);
        var encoding = new UTF8Encoding(true);
        byte[] preamble = encoding.GetPreamble();
        byte[] content = encoding.GetBytes(text);
        await stream.WriteAsync(preamble);
        await stream.WriteAsync(content);
        return true;
    }

    void ITalkgroupEditorHost.Close(bool result) => Close(result);
}
