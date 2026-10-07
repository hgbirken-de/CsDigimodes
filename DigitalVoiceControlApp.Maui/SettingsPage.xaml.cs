using DigitalVoiceControlApp.Maui.ViewModels;
using NLog;

namespace DigitalVoiceControlApp.Maui;

/// <summary>
/// Settings-Dialog (Gegenstück zum Avalonia-<c>SettingsDialog</c>): Common, DMR, NXDN, FCS, AMBE.
/// "Apply" prüft und speichert, "Discard" verwirft, "Import…" liest eine (Desktop-)UserSettings.yaml ein.
/// </summary>
public partial class SettingsPage : ContentPage
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly SettingsViewModel _viewModel = new();

    public SettingsPage()
    {
        InitializeComponent();
        BindingContext = _viewModel;
    }

    /// <summary>DMR-Master wählen (Auswahlseite mit Suchfeld; die Liste hat rund 1200 Einträge).</summary>
    private async void OnDmrMasterClicked(object? sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new SelectionPage("Select DMR master", _viewModel.DmrMasterList,
                _viewModel.SelectedDmrMaster, name => _viewModel.SelectedDmrMaster = name));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Opening the selection page failed.");
            await DisplayAlert("Error", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnApplyClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_viewModel.TrySave(out List<string> errors))
                await Navigation.PopAsync();
            else
                await DisplayAlert("Check your input", string.Join("\n", errors), "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Saving settings failed.");
            await DisplayAlert("Error", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnDiscardClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        try
        {
            bool proceed = await DisplayAlert("Import settings",
                "All current settings will be replaced by the contents of the selected file and saved immediately " +
                "(e.g. the UserSettings.yaml from the PC). \"Discard\" cannot undo this step. Continue?",
                "Choose file", "Cancel");
            if (!proceed)
                return;

            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select UserSettings.yaml" });
            if (file == null)
                return; // abgebrochen

            using Stream stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            string yaml = await reader.ReadToEndAsync();

            _viewModel.Import(yaml);
            await DisplayAlert("Import", "The settings were imported and saved.", "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Importing settings failed.");
            await DisplayAlert("Import failed", ex.Message, "OK");
        }
    }
}
