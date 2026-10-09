using DigitalVoiceControlApp.Maui.ViewModels;
using NLog;

namespace DigitalVoiceControlApp.Maui;

/// <summary>About-Seite: Version, Build, System, Copyright, Lizenz und Fremdsoftware.</summary>
public partial class AboutPage : ContentPage
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly AboutInfo _info = new();

    public AboutPage()
    {
        InitializeComponent();
        BindingContext = _info;
    }

    private async void OnCopyInfoClicked(object? sender, EventArgs e)
    {
        try
        {
            await Clipboard.Default.SetTextAsync(_info.ToClipboardText());
            await DisplayAlert("Copied", "The build information was copied to the clipboard.", "OK");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Copying the build information failed.");
            await DisplayAlert("Error", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnProjectPageClicked(object? sender, EventArgs e) => await OpenUrlAsync(AboutInfo.ProjectUrl);

    private async void OnLicenseUrlClicked(object? sender, EventArgs e) => await OpenUrlAsync(AboutInfo.LicenseUrl);

    /// <summary>Zeigt oder verbirgt den vollständigen Lizenztext der mbelib.</summary>
    private void OnToggleIscClicked(object? sender, EventArgs e)
    {
        IscLabel.IsVisible = !IscLabel.IsVisible;
        IscButton.Text = IscLabel.IsVisible ? "Hide license text" : "Show license text";
    }

    private async Task OpenUrlAsync(string url)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri(url));
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Opening '{url}' failed.");
            await DisplayAlert("Error", $"The page could not be opened:\n{url}", "OK");
        }
    }
}
