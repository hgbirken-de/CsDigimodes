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
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Die Last-Heard-Liste übernimmt die Schriftgröße der Felder (Vorbild ist die Beschriftung "CALL"). Die Breite der Liste
    /// hängt an der Schriftgröße (monospace), deshalb erfährt auch das ViewModel die Größe.
    /// </summary>
    private void OnLoaded(object? sender, EventArgs e)
    {
        double size = RefLabel.FontSize;
        if (size <= 0)
            return;

        Resources["ListFontSize"] = size;   // wirkt über DynamicResource auf alle Zeilen der Liste

        if (BindingContext is DmrViewModel vm)
            vm.ListFontSize = size;
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
