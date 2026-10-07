using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigitalVoice.Common;
using DigitalVoice.Nxdn;
using DigitalVoiceControlApp.Maui.Config;
using NLog;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// ViewModel der NXDN-Ansicht (Gegenstück zum Avalonia-<c>NxdnViewModel</c>): Rufzeichen, Quelle, Ziel, Gateway und die
/// Last-Heard-Liste. Aufbau wie <see cref="DmrViewModel"/>: Der Client ruft <see cref="ConsumeNxdnData"/> auf seinem eigenen
/// Thread auf (für jeden Frame), die Oberfläche wird mit <c>MainThread.BeginInvokeOnMainThread</c> angefasst, und das
/// Nachladen der Nutzerdaten (radioid.net) läuft synchron auf dem Thread-Pool.
/// </summary>
public partial class NxdnViewModel : ObservableObject
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [ObservableProperty] private string callsign = "";
    [ObservableProperty] private string srcId = "";
    [ObservableProperty] private string dstId = "";
    [ObservableProperty] private string gwId = "";

    public ObservableCollection<LastHeardItemNxdn> LastHeard { get; } = [];

    // Die Zeilen werden nicht umbrochen, sondern waagerecht gescrollt: Breite nach der längsten Zeile
    private const double MonoCharWidth = 7.4;
    private const double MinListWidth = 400;

    [ObservableProperty]
    private double lastHeardWidth = MinListWidth;

    public IRelayCommand ClearLastHeardCommand { get; }

    readonly ConcurrentDictionary<int, NxdnUserData> _userCache = [];

    // Verhindert, dass für dieselbe srcId mehrfach parallel nachgeladen wird.
    readonly ConcurrentDictionary<int, byte> _pendingFetches = [];

    // srcIds, für die radioid.net keinen Treffer hatte: nicht erneut fragen.
    readonly ConcurrentDictionary<int, byte> _failedLookups = [];

    // Nur auf dem UI-Thread benutzt: Für den laufenden Stream gibt es schon einen Last-Heard-Eintrag.
    bool _streamRecorded;

    public NxdnViewModel()
    {
        ClearLastHeardCommand = new RelayCommand(ClearLastHeard);
    }

    /// <summary>Vom Client aufgerufen (nicht der UI-Thread), und zwar für jeden empfangenen Frame.</summary>
    public void ConsumeNxdnData(NxdnSessionContext s)
    {
        TransceiveMode tm = s.TransceiveMode;

        if (tm == TransceiveMode.Rx && (s.StreamState is StreamState.End or StreamState.Idle or StreamState.Lost))
        {
            logger.Debug($"Stream ended (state={s.StreamState}), clearing the fields.");
            MainThread.BeginInvokeOnMainThread(Clear);
            return; // kein Last-Heard-Eintrag
        }

        // Werte jetzt kopieren: Der Client benutzt dasselbe Objekt weiter
        int srcId = s.SrcId;
        int dstId = s.DstId;
        int gwId = s.GwId;

        NxdnUserData? userData = null;
        if (tm == TransceiveMode.Rx)
        {
            if (_userCache.TryGetValue(srcId, out var cached))
            {
                userData = cached;
            }
            else if (srcId > 0 && !_failedLookups.ContainsKey(srcId) && _pendingFetches.TryAdd(srcId, 0))
            {
                int id = srcId;
                ThreadPool.QueueUserWorkItem(_ => FetchUserData(id));
            }
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            DstId = dstId > 0 ? dstId.ToString() : string.Empty;
            GwId = gwId > 0 ? gwId.ToString() : string.Empty;
            SrcId = srcId.ToString();

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

            LastHeard.Insert(0, new LastHeardItemNxdn(gwId, srcId, dstId, userData)); // neuester Eintrag oben
            _streamRecorded = true;
            logger.Debug($"New last-heard entry: src={srcId}, dst={dstId}, gw={gwId}");

            if (LastHeard.Count > 50)
                LastHeard.RemoveAt(50); // höchstens 50 Einträge

            UpdateLastHeardWidth();
        });
    }

    /// <summary>Lädt die Nutzerdaten einer NXDN-ID synchron (Thread-Pool) und ergänzt danach die Anzeige.</summary>
    private void FetchUserData(int srcId)
    {
        logger.Debug($"Fetch started for srcId={srcId}");
        try
        {
            NxdnUserData? fetched = NxdnUserDataReader.GetUser(srcId); // blockiert, daher nicht auf dem UI-Thread
            logger.Debug($"Fetch result for srcId={srcId}: Callsign={fetched?.Callsign ?? "null (no match)"}");

            if (fetched != null)
            {
                _userCache[srcId] = fetched;

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

    private void UpdateLastHeardWidth()
    {
        int longest = LastHeard.Count == 0 ? 0 : LastHeard.Max(i => i.Display.Length);
        LastHeardWidth = Math.Max(MinListWidth, longest * MonoCharWidth + 24);
    }

    /// <summary>Leert die einfachen Felder (Aufruf nur auf dem UI-Thread).</summary>
    internal void Clear()
    {
        Callsign = "";
        DstId = "";
        GwId = "";
        SrcId = "";
        _streamRecorded = false; // der nächste Stream bekommt wieder einen Eintrag
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
        UpdateLastHeardWidth();
    }
}
