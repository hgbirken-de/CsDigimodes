using DigitalVoiceControlApp.Maui.ViewModels;
using NLog;

namespace DigitalVoiceControlApp.Maui.Views;

/// <summary>
/// D-STAR-Ansicht für DCS, REF und XRF (Gegenstück zum Avalonia-<c>DStarModeControl</c>). Bindet an das
/// <see cref="DStarViewModel"/>.
/// </summary>
public partial class DStarView : ContentView
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public DStarView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Doppeltipp auf einen Eintrag: Das Rufzeichen (MYCALL) kommt in die Zwischenablage. qrz.com wird bewusst nicht geöffnet:
    /// Der Browser würde die App in den Hintergrund schicken, und dabei wird die Verbindung sofort getrennt.
    /// </summary>
    private async void OnLastHeardDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (sender is BindableObject { BindingContext: LastHeardItemDStar item } && !string.IsNullOrEmpty(item.Src))
                await Clipboard.Default.SetTextAsync(item.Src);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Copying the callsign failed.");
        }
    }
}
