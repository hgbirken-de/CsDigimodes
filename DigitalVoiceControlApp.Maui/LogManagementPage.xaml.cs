using DigitalVoiceControlApp.Maui.Services;
using NLog;

namespace DigitalVoiceControlApp.Maui;

/// <summary>
/// Log-Verwaltung: Anzahl und Größe der Logdateien, Log-Level umschalten, alle Logdateien als Zip teilen oder löschen.
/// Die Logik steckt in <see cref="AppLogging"/>; diese Seite zeigt nur an und fragt nach. Aus dem Menü nur erreichbar,
/// solange die App nicht verbunden ist.
/// </summary>
public partial class LogManagementPage : ContentPage
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private bool _initializing = true;

    public LogManagementPage()
    {
        InitializeComponent();

        LevelPicker.ItemsSource = AppLogging.LevelNames.ToList();
        LevelPicker.SelectedItem = AppLogging.CurrentLevelName;
        _initializing = false;

        RefreshInfo();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshInfo();
    }

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024.0):F1} MB",
    };

    private void RefreshInfo()
    {
        var (count, bytes) = AppLogging.GetLogSummary();
        InfoLabel.Text = count == 0 ? "No log files." : $"{count} file(s), {FormatSize(bytes)} in total";
    }

    private void OnLevelChanged(object? sender, EventArgs e)
    {
        if (_initializing || LevelPicker.SelectedItem is not string level)
            return;

        AppLogging.SetLevel(level);
        RefreshInfo();
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        try
        {
            string? zipPath = AppLogging.CreateExportZip();
            if (zipPath == null)
            {
                await DisplayAlert("Export logs", "No log files found.", "OK");
                return;
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Share log files",
                File = new ShareFile(zipPath),
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exporting the logs failed.");
            await DisplayAlert("Export failed", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        try
        {
            var (count, bytes) = AppLogging.GetLogSummary();
            if (count == 0)
            {
                await DisplayAlert("Delete logs", "There are no log files.", "OK");
                return;
            }

            bool delete = await DisplayAlert("Delete logs",
                $"Delete all {count} log file(s) ({FormatSize(bytes)})? This cannot be undone.", "Delete", "Cancel");
            if (!delete)
                return;

            int deleted = AppLogging.DeleteLogs();
            RefreshInfo();
            await DisplayAlert("Delete logs", $"{deleted} file(s) deleted.", "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Deleting the logs failed.");
            await DisplayAlert("Delete failed", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }
}
