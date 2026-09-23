using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalVoiceControlApp.Maui.Services.Ambe;
using NLog;

#if ANDROID
using Android.Content;
#endif

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// ViewModel für die Hauptseite. Namensgebung an DigitalVoiceControlApp (Avalonia,
/// Desktop) MainViewModel angelehnt: IsServerConnected / ConnectCommand / ConnectionIcon /
/// ConnectionTooltip. Bezieht sich aktuell nur auf die AMBE-Stick-Verbindung (Vorstufe);
/// sobald ein Android-DMR-Client existiert, wandert die eigentliche Server-Verbindung
/// vermutlich hier mit rein, analog zu DmrClient2.Start() in der Desktop-Version.
/// </summary>
public partial class MainPageViewModel : ObservableObject
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private Ambe3000Usb? _ambe;

    /// <summary>Die aktuell offene AMBE-Verbindung, falls vorhanden (z.B. für den AMBE-Test-Menüpunkt).</summary>
    public Ambe3000Usb? AmbeDevice => _ambe;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ConnectionIcon))]
    [NotifyPropertyChangedFor(nameof(ConnectionTooltip))]
    [NotifyPropertyChangedFor(nameof(ConnectionBackgroundColor))]
    private bool isServerConnected;

    // TODO: Dateinamen anpassen, sobald die tatsächlichen Icon-Dateinamen im Projekt feststehen
    // (Resources/Images/, Kleinschreibung, z.B. connect_16x.png / disconnect_16x.png).
    public string ConnectionIcon => IsServerConnected ? "disconnect_16x.png" : "connect_16x.png";

    public string ConnectionTooltip => IsServerConnected ? "Disconnect from Server" : "Connect to Server";

    public Color ConnectionBackgroundColor => IsServerConnected ? Colors.Red : Colors.Transparent;

    public IAsyncRelayCommand ConnectCommand { get; }

    /// <summary>Wird ausgelöst, wenn die View einen Popup-Hinweis anzeigen soll (Fehler etc.).</summary>
    public event Func<string, string, Task>? ShowAlertRequested;

    public MainPageViewModel()
    {
        ConnectCommand = new AsyncRelayCommand(ConnectDisconnectAsync);
    }

    private async Task ConnectDisconnectAsync()
    {
#if ANDROID
        var activity = Platform.CurrentActivity;
        if (activity == null)
        {
            await RaiseAlertAsync("AMBE", "Keine Activity verfügbar.");
            return;
        }

        if (!IsServerConnected)
        {
            var result = await Ambe3000Usb.ConnectAsync(activity);

            if (result.Device == null)
            {
                await RaiseAlertAsync("Verbindung fehlgeschlagen", result.ErrorMessage ?? "Unbekannter Fehler");
                return;
            }

            _ambe = result.Device;
            IsServerConnected = true;
            Log.Info($"Verbunden: ProductId={result.ProductId}, Version={result.Version}");
        }
        else
        {
            _ambe?.Close();
            _ambe?.Dispose();
            _ambe = null;
            IsServerConnected = false;
            Log.Info("Verbindung getrennt.");
        }
#else
        await RaiseAlertAsync("AMBE", "Nur auf Android verfügbar.");
#endif
    }

    private Task RaiseAlertAsync(string title, string message) =>
        ShowAlertRequested?.Invoke(title, message) ?? Task.CompletedTask;
}
