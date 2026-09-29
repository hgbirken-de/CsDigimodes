using Avalonia.Threading;
using DigitalVoice.Common;
using DigitalVoice.Nxdn;
using DigitalVoiceControlApp.Commands;
using DigitalVoiceControlApp.Config;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

public class NxdnViewModel : ViewModelBase
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    readonly ConcurrentDictionary<int, NxdnUserData> _nxdnUserCache = [];

    // Verhindert, dass für dieselbe srcId mehrfach parallel gefetcht wird (z.B. weil
    // ConsumeNxdnData bei einer laufenden Übertragung alle ~20ms erneut aufgerufen wird).
    readonly ConcurrentDictionary<int, byte> _pendingFetches = [];

    string _callsign = string.Empty;
    string _srcId = string.Empty;
    string _dstId = string.Empty;
    string _gwid = string.Empty;

    public ICommand ClearLastHeardCommand { get; }

    public ObservableCollection<LastHeardItemNxdn> _lastHeard = [];
    public ObservableCollection<LastHeardItemNxdn> LastHeard
    {
        get => _lastHeard;
        set { _lastHeard = value; OnPropertyChanged(); }
    }

    public string Callsign
    {
        get => _callsign;
        set { _callsign = value; OnPropertyChanged(); }
    }

    public string DstId
    {
        get => _dstId;
        set { _dstId = value; OnPropertyChanged(); }
    }

    public string GwId
    {
        get => _gwid;
        set { _gwid = value; OnPropertyChanged(); }
    }

    public string SrcId
    {
        get => _srcId;
        set { _srcId = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public NxdnViewModel()
    {
        ClearLastHeardCommand = new RelayCommand<object?>(_ => ClearLastHeard());
    }

    public void ConsumeNxdnData(NxdnSessionContext sessionCtx)
    {
        if (sessionCtx.TransceiveMode == TransceiveMode.Rx && (sessionCtx.StreamState is StreamState.End or StreamState.Idle or StreamState.Lost))
        {
            Dispatcher.UIThread.Post(() => { Clear(); });
            return; // no last heard
        }

        // Copy/save session attributes
        TransceiveMode tm = sessionCtx.TransceiveMode;
        int srcId = sessionCtx.SrcId;
        int dstId = sessionCtx.DstId;
        int gwId = sessionCtx.GwId;

        NxdnUserData? userData = null;

        if (tm == TransceiveMode.Rx)
        {
            if (_nxdnUserCache.TryGetValue(srcId, out var cached))
            {
                userData = cached; // schon bekannt -> sofort verfügbar, kein Fetch nötig
            }
            else
            {
                // userData bleibt null -> Anzeige/Last-Heard-Eintrag zeigt zunächst NOCALL.
                // Fetch läuft im Hintergrund, Update erfolgt separat, sobald fertig.
                // TryAdd verhindert, dass bei laufender Übertragung (alle ~20ms erneuter Aufruf)
                // mehrfach parallel für dieselbe srcId gefetcht wird.
                if (_pendingFetches.TryAdd(srcId, 0))
                {
                    _ = FetchAndUpdateUserDataAsync(srcId);
                }
            }
        }

        Dispatcher.UIThread.Post(() =>
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
                    var us = UserSettings.Instance();
                    Callsign = us.Common.Callsign;
                    return; // no last heard with TX
            }

            for (int i = 0; i < LastHeard.Count; i++)
            {
                if (LastHeard[i].SrcId == srcId)
                {
                    LastHeard.RemoveAt(i);
                    break;
                }
            }

            var newItem = new LastHeardItemNxdn(gwId, srcId, dstId, userData);
            LastHeard.Insert(0, newItem); // insert the new item at the top

            // Limit to max 50 entries
            if (LastHeard.Count > 50)
            {
                LastHeard.RemoveAt(50); // remove from the end
            }
        });
    }

    /// <summary>
    /// Lädt die Nutzerdaten für eine NXDN-ID im Hintergrund und aktualisiert - falls der
    /// zugehörige Last-Heard-Eintrag noch existiert - dessen Anzeige.
    /// Existiert der Eintrag nicht mehr (z.B. durch die 50er-Begrenzung verdrängt), passiert
    /// nichts weiter; der Cache wird trotzdem befüllt.
    /// </summary>
    private async Task FetchAndUpdateUserDataAsync(int srcId)
    {
        logger.Debug($"Fetch gestartet für srcId={srcId}");
        try
        {
            var fetched = await NxdnUserDataReader.GetUserAsync(srcId);
            logger.Debug($"Fetch-Ergebnis für srcId={srcId}: Callsign={fetched?.Callsign ?? "null (kein Treffer)"}");

            if (fetched != null)
            {
                _nxdnUserCache[srcId] = fetched; // cache it

                Dispatcher.UIThread.Post(() =>
                {
                    bool found = false;
                    for (int i = 0; i < LastHeard.Count; i++)
                    {
                        if (LastHeard[i].SrcId == srcId)
                        {
                            LastHeard[i].Update(fetched);
                            // Erzwingt ein CollectionChanged(Replace)-Event, auch wenn es dasselbe
                            // Objekt ist - das zwingt die ListBox zum Neuzeichnen dieser Zeile.
                            LastHeard[i] = LastHeard[i];
                            found = true;
                            break;
                        }
                    }
                    logger.Debug($"Update für srcId={srcId} im LastHeard-Eintrag gefunden={found}");
                });
            }
            else
            {
                logger.Warn($"Unable to read user data, srcId = {srcId}");
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

    /// <summary>
    /// Clear simple properties of this model.
    /// </summary>
    private void Clear()
    {
        logger.Debug($"");
        Callsign = "";
        DstId = string.Empty;
        GwId = string.Empty;
        SrcId = string.Empty;
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
    }
}
