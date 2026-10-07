using DigitalVoiceControlApp.Maui.ViewModels;
using NLog;

namespace DigitalVoiceControlApp.Maui.Views;

/// <summary>
/// DMR-Ansicht (Gegenstück zum Avalonia-<c>DmrModeControl</c>). Bindet an das <see cref="DmrViewModel"/>.
/// </summary>
public partial class DmrView : ContentView
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public DmrView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Doppeltipp auf einen Eintrag: Das Rufzeichen kommt in die Zwischenablage. Die Desktop-App öffnet zusätzlich
    /// qrz.com. Das geht hier bewusst nicht: Der Browser würde die App in den Hintergrund schicken, und dabei wird die
    /// DMR-Verbindung sofort getrennt.
    /// </summary>
    private async void OnLastHeardDoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            if (sender is BindableObject { BindingContext: LastHeardItemDmr item } && !string.IsNullOrEmpty(item.Callsign))
                await Clipboard.Default.SetTextAsync(item.Callsign);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Copying the callsign failed.");
        }
    }
}
