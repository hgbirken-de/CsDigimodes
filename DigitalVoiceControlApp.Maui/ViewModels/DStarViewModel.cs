using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalVoice.Common;
using DigitalVoice.DStar.Common;
using NLog;
using System.Collections.ObjectModel;

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// ViewModel der D-STAR-Ansicht (DCS, REF, XRF; Gegenstück zum Avalonia-<c>DStarViewModel</c>): RPTR1, RPTR2, MYCALL, URCALL,
/// Text, GPS und die Last-Heard-Liste. Der Client ruft <see cref="ConsumeDStarData"/> auf seinem eigenen Thread auf (für jeden
/// Frame), die Oberfläche wird mit <c>MainThread.BeginInvokeOnMainThread</c> angefasst.
/// </summary>
public partial class DStarViewModel : ObservableObject
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [ObservableProperty] private string rpt1 = "";
    [ObservableProperty] private string rpt2 = "";
    [ObservableProperty] private string src = "";   // MYCALL
    [ObservableProperty] private string dst = "";   // URCALL
    [ObservableProperty] private string message = "";
    [ObservableProperty] private string gpsData = "";

    public ObservableCollection<LastHeardItemDStar> LastHeard { get; } = [];

    // Die Zeilen werden nicht umbrochen, sondern waagerecht gescrollt: Breite nach der längsten Zeile
    private const double MonoCharWidth = 7.4;
    private const double MinListWidth = 400;

    [ObservableProperty]
    private double lastHeardWidth = MinListWidth;

    public IRelayCommand ClearLastHeardCommand { get; }

    // Nur auf dem UI-Thread benutzt: Für den laufenden Stream gibt es schon einen Last-Heard-Eintrag.
    bool _streamRecorded;

    public DStarViewModel()
    {
        ClearLastHeardCommand = new RelayCommand(ClearLastHeard);
    }

    /// <summary>Vom Client aufgerufen (nicht der UI-Thread), und zwar für jeden Frame.</summary>
    public void ConsumeDStarData(DStarSessionContext s)
    {
        if (s.RxStreamState is StreamState.End or StreamState.Idle or StreamState.Lost)
        {
            MainThread.BeginInvokeOnMainThread(Clear);
            return; // kein Last-Heard-Eintrag
        }

        // Werte jetzt kopieren: Der Client benutzt dasselbe Objekt weiter. Rufzeichen und Text sind mit Leerzeichen aufgefüllt.
        TransceiveMode tm = s.TransceiveMode;
        string reflector = s.Reflector ?? "";
        string rpt1 = "", rpt2 = "", src = "", dst = "", gps = "", msg = "";

        switch (tm)
        {
            case TransceiveMode.Rx:
                rpt1 = Clean(s.RxRptr1);
                rpt2 = Clean(s.RxRptr2);
                src = Clean(s.RxSrc);
                dst = Clean(s.RxUrCall);   // in der Desktop-App wird URCALL hier gesetzt und gleich danach mit "" überschrieben
                gps = Clean(s.RxGpsData);
                msg = Clean(s.RxUsrMsg);
                break;
            case TransceiveMode.Tx:
                rpt1 = Clean(s.TxRptr1);
                src = Clean(s.TxMyCall);
                dst = Clean(s.TxUrCall);
                msg = Clean(s.TxUsrMsg);
                break;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Rpt1 = rpt1;
            Src = src;
            Dst = dst;
            Message = msg;

            if (tm == TransceiveMode.Tx)
                return; // beim Senden kein Last-Heard-Eintrag

            Rpt2 = rpt2;
            GpsData = gps;

            if (src.Length == 0)
                return; // noch kein Rufzeichen dekodiert

            // Pro Stream ein Eintrag (der Client meldet jeden Frame); spätere Frames tragen Gateway und Ziel nach
            if (_streamRecorded && LastHeard.Count > 0 && LastHeard[0].Src == src)
            {
                if (LastHeard[0].Update(rpt1, dst))
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

            LastHeard.Insert(0, new LastHeardItemDStar(dst, rpt1, src, reflector)); // neuester Eintrag oben
            _streamRecorded = true;
            logger.Debug($"New last-heard entry: src={src}, dst={dst}, rpt1={rpt1}, reflector={reflector}");

            if (LastHeard.Count > 50)
                LastHeard.RemoveAt(50); // höchstens 50 Einträge

            UpdateLastHeardWidth();
        });
    }

    /// <summary>Entfernt Steuerzeichen und Leerraum (die Rufzeichen und der Text sind mit Leerzeichen aufgefüllt).</summary>
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
        Rpt1 = "";
        Rpt2 = "";
        Dst = "";
        Src = "";
        GpsData = "";
        Message = "";
        _streamRecorded = false; // der nächste Stream bekommt wieder einen Eintrag
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
        UpdateLastHeardWidth();
    }
}
