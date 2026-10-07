using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoiceControlApp.Maui.Config;
using NLog;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// ViewModel der DMR-Ansicht (Gegenstück zum Avalonia-<c>DmrViewModel</c>): Anzeige von Rufzeichen, Quelle, Ziel,
/// Talker Alias und der Last-Heard-Liste. Der Client ruft <see cref="ConsumeDmrData"/> auf seinem eigenen
/// Thread auf; alles, was die Oberfläche berührt, wird mit <c>MainThread.BeginInvokeOnMainThread</c> übergeben.
/// Ohne async/await: Das Nachladen der Nutzerdaten (radioid.net) läuft synchron auf dem Thread-Pool.
/// </summary>
public partial class DmrViewModel : ObservableObject
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [ObservableProperty] private string callsign = "";
    [ObservableProperty] private string flco = "";
    [ObservableProperty] private string dstId = "";
    [ObservableProperty] private string rptId = "";
    [ObservableProperty] private string srcId = "";
    [ObservableProperty] private string rxTa = "";

    public ObservableCollection<LastHeardItemDmr> LastHeard { get; } = [];

    // Die Zeilen der Liste werden nicht umbrochen, sondern waagerecht gescrollt. Die Breite richtet sich nach der längsten
    // Zeile (Schrift "monospace", 12: rund 7,2 dp pro Zeichen, mit etwas Reserve).
    private const double MonoCharWidth = 7.4;
    private const double MinListWidth = 400;

    [ObservableProperty]
    private double lastHeardWidth = MinListWidth;

    public IRelayCommand ClearLastHeardCommand { get; }

    readonly ConcurrentDictionary<int, DmrUserData> _dmrUserCache = [];

    // Verhindert, dass für dieselbe srcId mehrfach parallel nachgeladen wird.
    readonly ConcurrentDictionary<int, byte> _pendingFetches = [];

    // srcIds, für die radioid.net keinen Treffer hatte: nicht erneut fragen.
    readonly ConcurrentDictionary<int, byte> _failedLookups = [];

    // Nur auf dem UI-Thread benutzt: Für den laufenden Stream gibt es schon einen Last-Heard-Eintrag.
    bool _streamRecorded;

    public DmrViewModel()
    {
        ClearLastHeardCommand = new RelayCommand(ClearLastHeard);
    }

    /// <summary>
    /// Vom Client aufgerufen (nicht der UI-Thread), und zwar für jedes empfangene Paket. Die Felder werden bei jedem Aufruf
    /// gesetzt (unveränderte Werte lösen keine Oberflächenänderung aus); der Last-Heard-Eintrag entsteht einmal pro Stream.
    /// </summary>
    public void ConsumeDmrData(DmrSessionContext s)
    {
        TransceiveMode tm = s.TransceiveMode;

        if (tm == TransceiveMode.Rx && (s.RxStreamState is StreamState.End or StreamState.Idle or StreamState.Lost))
        {
            logger.Debug($"Stream ended (state={s.RxStreamState}), clearing the fields.");
            s.RxTalkerAlias = "";
            MainThread.BeginInvokeOnMainThread(Clear);
            return; // kein Last-Heard-Eintrag
        }

        // Werte jetzt kopieren: Der Client benutzt dasselbe Objekt weiter
        int dstId = 0;
        int rptId = 0;
        int srcId = 0;
        string rxTa = "";
        string flco = "";

        switch (tm)
        {
            case TransceiveMode.Rx:
                dstId = s.RxDstId;
                rptId = s.RxRptId;
                srcId = s.RxSrcId;
                rxTa = s.RxTalkerAlias;
                flco = s.RxFlco.ToString();
                break;
            case TransceiveMode.Tx:
                dstId = s.TxDstId;
                rptId = s.TxRptId;
                srcId = s.TxSrcId;
                break;
        }

        DmrUserData? userData = null;
        if (tm == TransceiveMode.Rx)
        {
            if (_dmrUserCache.TryGetValue(srcId, out var cached))
            {
                userData = cached; // schon bekannt, kein Nachladen nötig
            }
            else if (srcId > 0 && !_failedLookups.ContainsKey(srcId) && _pendingFetches.TryAdd(srcId, 0))
            {
                // Anzeige zeigt zunächst NOCALL; das Nachladen läuft im Hintergrund und ergänzt später
                int id = srcId;
                ThreadPool.QueueUserWorkItem(_ => FetchUserData(id));
            }
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Flco = flco;
            DstId = dstId > 0 ? dstId.ToString() : string.Empty;
            RptId = rptId > 0 ? rptId.ToString() : string.Empty;
            SrcId = srcId.ToString();
            RxTa = rxTa;

            switch (tm)
            {
                case TransceiveMode.Rx:
                    Callsign = userData != null ? (userData.Callsign ?? "NOCALL") : "NOCALL";
                    break;
                case TransceiveMode.Tx:
                    Callsign = UserSettings.Instance().Common.Callsign;
                    return; // kein Last-Heard-Eintrag beim Senden
            }

            // Pro Stream einmal eintragen (der Client meldet jeden Frame)
            if (_streamRecorded && LastHeard.Count > 0 && LastHeard[0].SrcId == srcId)
                return;

            for (int i = 0; i < LastHeard.Count; i++)
            {
                if (LastHeard[i].SrcId == srcId)
                {
                    LastHeard.RemoveAt(i);
                    break;
                }
            }

            LastHeard.Insert(0, new LastHeardItemDmr(dstId, srcId, userData)); // neuester Eintrag oben
            _streamRecorded = true;
            logger.Debug($"New last-heard entry: src={srcId}, dst={dstId}");

            if (LastHeard.Count > 50)
                LastHeard.RemoveAt(50); // höchstens 50 Einträge

            UpdateLastHeardWidth();
        });
    }

    /// <summary>
    /// Lädt die Nutzerdaten einer srcId synchron (läuft auf dem Thread-Pool) und ergänzt danach die Anzeige, falls der
    /// Eintrag noch existiert. Der Cache wird in jedem Fall befüllt.
    /// </summary>
    private void FetchUserData(int srcId)
    {
        logger.Debug($"Fetch started for srcId={srcId}");
        try
        {
            DmrUserData? fetched = DmrUserDataReader.GetUser(srcId); // blockiert, daher nicht auf dem UI-Thread
            logger.Debug($"Fetch result for srcId={srcId}: Callsign={fetched?.Callsign ?? "null (no match)"}");

            if (fetched != null)
            {
                _dmrUserCache[srcId] = fetched;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    for (int i = 0; i < LastHeard.Count; i++)
                    {
                        if (LastHeard[i].SrcId == srcId)
                        {
                            LastHeard[i].Update(fetched);
                            break;
                        }
                    }

                    // Das Feld oben: Die Frames lösen keine Aktualisierung mehr aus, wenn sich nichts ändert
                    if (SrcId == srcId.ToString())
                        Callsign = fetched.Callsign ?? "NOCALL";

                    UpdateLastHeardWidth();
                });
            }
            else
            {
                _failedLookups.TryAdd(srcId, 0);
                logger.Warn($"Unable to read user data, srcId = {srcId} (will not be queried again)");
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Error during read of user data, srcId = {srcId}");
        }
        finally
        {
            _pendingFetches.TryRemove(srcId, out _);
        }
    }

    /// <summary>Passt die Breite der Liste an die längste Zeile an (Aufruf nur auf dem UI-Thread).</summary>
    private void UpdateLastHeardWidth()
    {
        int longest = LastHeard.Count == 0 ? 0 : LastHeard.Max(i => i.Display.Length);
        LastHeardWidth = Math.Max(MinListWidth, longest * MonoCharWidth + 24);
    }

    /// <summary>Leert die einfachen Felder (Aufruf nur auf dem UI-Thread).</summary>
    internal void Clear()
    {
        Callsign = "";
        Flco = "";
        DstId = "";
        RptId = "";
        SrcId = "";
        RxTa = "";
        _streamRecorded = false; // der nächste Stream bekommt wieder einen Eintrag
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
        UpdateLastHeardWidth();
    }
}
