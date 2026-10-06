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

    private async void OnApplyClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_viewModel.TrySave(out List<string> errors))
                await Navigation.PopAsync();
            else
                await DisplayAlert("Eingaben prüfen", string.Join("\n", errors), "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Saving settings failed.");
            await DisplayAlert("Fehler", $"{ex.GetType().Name}: {ex.Message}", "OK");
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
            bool proceed = await DisplayAlert("Einstellungen importieren",
                "Alle aktuellen Einstellungen werden durch den Inhalt der gewählten Datei ersetzt und sofort gespeichert " +
                "(z.B. die UserSettings.yaml vom PC). Dieser Schritt lässt sich mit \"Discard\" nicht rückgängig machen. Fortfahren?",
                "Datei wählen", "Abbrechen");
            if (!proceed)
                return;

            var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "UserSettings.yaml auswählen" });
            if (file == null)
                return; // abgebrochen

            using Stream stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            string yaml = await reader.ReadToEndAsync();

            _viewModel.Import(yaml);
            await DisplayAlert("Import", "Die Einstellungen wurden übernommen und gespeichert.", "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Importing settings failed.");
            await DisplayAlert("Import fehlgeschlagen", ex.Message, "OK");
        }
    }
}
