using DigitalVoiceControlApp.Maui.ViewModels;
using NLog;

namespace DigitalVoiceControlApp.Maui.Views;

/// <summary>
/// NXDN-Ansicht (Gegenstück zum Avalonia-<c>NxdnModeControl</c>). Bindet an das <see cref="NxdnViewModel"/>.
/// </summary>
public partial class NxdnView : ContentView
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public NxdnView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Doppeltipp auf einen Eintrag: Das Rufzeichen kommt in die Zwischenablage. qrz.com wird bewusst nicht geöffnet:
    /// Der Browser würde die App in den Hintergrund schicken, und dabei wird die Verbindung sofort getrennt.
    /// </summary>
    private async void OnLastHeardDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (sender is BindableObject { BindingContext: LastHeardItemNxdn item } && !string.IsNullOrEmpty(item.Callsign))
                await Clipboard.Default.SetTextAsync(item.Callsign);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Copying the callsign failed.");
        }
    }
}
