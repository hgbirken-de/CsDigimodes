using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalVoice.Common;
using DigitalVoice.Fusion;
using DigitalVoiceControlApp.Maui.Config;
using NLog;
using System.Collections.ObjectModel;

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// ViewModel der Fusion-Ansicht (YSF, später auch FCS; Gegenstück zum Avalonia-<c>FusionViewModel</c>): Gateway, Datentyp, Quelle,
/// Ziel und die Last-Heard-Liste. Der Client ruft <see cref="ConsumeYsfData"/> auf seinem eigenen Thread auf (für jeden
/// Frame), die Oberfläche wird mit <c>MainThread.BeginInvokeOnMainThread</c> angefasst. Die Rufzeichen stehen im Stream selbst,
/// ein Nachladen von Nutzerdaten gibt es hier nicht.
/// </summary>
public partial class FusionViewModel : ObservableObject
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [ObservableProperty] private string src = "";
    [ObservableProperty] private string gw = "";
    [ObservableProperty] private string dst = "";
    [ObservableProperty] private string dataType = "";

    public ObservableCollection<LastHeardItemFusion> LastHeard { get; } = [];

    // Die Zeilen werden nicht umbrochen, sondern waagerecht gescrollt: Breite nach der längsten Zeile
    private const double MonoCharWidth = 7.4;
    private const double MinListWidth = 400;

    [ObservableProperty]
    private double lastHeardWidth = MinListWidth;

    public IRelayCommand ClearLastHeardCommand { get; }

    // Nur auf dem UI-Thread benutzt: Für den laufenden Stream gibt es schon einen Last-Heard-Eintrag.
    bool _streamRecorded;

    public FusionViewModel()
    {
        ClearLastHeardCommand = new RelayCommand(ClearLastHeard);
    }

    /// <summary>Vom Client aufgerufen (nicht der UI-Thread), und zwar für jeden empfangenen Frame.</summary>
    public void ConsumeYsfData(YsfSessionContext s)
    {
        TransceiveMode tm = s.TransceiveMode;

        if (tm == TransceiveMode.Rx && (s.StreamState is StreamState.End or StreamState.Idle or StreamState.Lost))
        {
            logger.Debug($"Stream ended (state={s.StreamState}), clearing the fields.");
            MainThread.BeginInvokeOnMainThread(Clear);
            return; // kein Last-Heard-Eintrag
        }

        if (tm == TransceiveMode.Tx)
            return; // beim Senden gibt es hier nichts anzuzeigen

        // Der erste Frame eines Streams ist der Header (HC). Er enthält nur Quelle und Ziel; der Decoder setzt dort das Gateway
        // gleich dem Ziel. Das Gateway wird deshalb erst ab den Sprachframes (CC) übernommen, dort steht das echte.
        bool isHeader = s.Fi == global::FusionCodec.FrameInformation.HC;

        // Werte jetzt kopieren: Der Client benutzt dasselbe Objekt weiter. Die Rufzeichen sind auf 10 Zeichen aufgefüllt,
        // aus dem Datenkanal können auch Steuerzeichen (NUL) kommen.
        string src = Clean(s.Src);
        string gw = isHeader ? "" : Clean(s.Gw);
        string dst = Clean(s.Dst);
        string dataType = s.Dt.ToString();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Src = src;
            Gw = gw;
            Dst = dst;
            DataType = dataType;

            if (src.Length == 0)
                return; // noch kein Rufzeichen dekodiert

            // Pro Stream ein Eintrag (der Client meldet jeden Frame); spätere Frames tragen Gateway und Ziel nach
            if (_streamRecorded && LastHeard.Count > 0 && LastHeard[0].Src == src)
            {
                if (LastHeard[0].Update(gw, dst))
                    UpdateLastHeardWidth();
                return;
            }

            for (int i = 0; i < LastHeard.Count; i++)
            {
                if (LastHeard[i].Src == src)
                {
                    LastHeard.RemoveAt(i);
                    break;
                }
            }

            LastHeard.Insert(0, new LastHeardItemFusion(gw, src, dst)); // neuester Eintrag oben
            _streamRecorded = true;
            logger.Debug($"New last-heard entry: src={src}, dst={dst}, gw={gw}");

            if (LastHeard.Count > 50)
                LastHeard.RemoveAt(50); // höchstens 50 Einträge

            UpdateLastHeardWidth();
        });
    }

    /// <summary>Entfernt Steuerzeichen und Leerraum (die Rufzeichen im Stream sind mit Leerzeichen und NUL aufgefüllt).</summary>
    private static string Clean(string? value) =>
        value == null ? "" : new string(value.Where(c => !char.IsControl(c)).ToArray()).Trim();

    private void UpdateLastHeardWidth()
    {
        int longest = LastHeard.Count == 0 ? 0 : LastHeard.Max(i => i.Display.Length);
        LastHeardWidth = Math.Max(MinListWidth, longest * MonoCharWidth + 24);
    }

    /// <summary>Leert die einfachen Felder (Aufruf nur auf dem UI-Thread).</summary>
    internal void Clear()
    {
        Src = "";
        Gw = "";
        Dst = "";
        DataType = "";
        _streamRecorded = false; // der nächste Stream bekommt wieder einen Eintrag
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
        UpdateLastHeardWidth();
    }
}
