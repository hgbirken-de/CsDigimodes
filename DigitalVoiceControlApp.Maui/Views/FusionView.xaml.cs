using DigitalVoiceControlApp.Maui.ViewModels;
using NLog;

namespace DigitalVoiceControlApp.Maui.Views;

/// <summary>
/// Fusion-Ansicht für YSF (später auch FCS; Gegenstück zum Avalonia-<c>FusionModeControl</c>). Bindet an das
/// <see cref="FusionViewModel"/>.
/// </summary>
public partial class FusionView : ContentView
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public FusionView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Doppeltipp auf einen Eintrag: Das Rufzeichen (Quelle) kommt in die Zwischenablage. qrz.com wird bewusst nicht geöffnet:
    /// Der Browser würde die App in den Hintergrund schicken, und dabei wird die Verbindung sofort getrennt.
    /// </summary>
    private async void OnLastHeardDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (sender is BindableObject { BindingContext: LastHeardItemFusion item } && !string.IsNullOrEmpty(item.Src))
                await Clipboard.Default.SetTextAsync(item.Src);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Copying the callsign failed.");
        }
    }
}
