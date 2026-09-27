using Avalonia.Threading;
using DigitalVoice.Common;
using DigitalVoice.Dmr;
using DigitalVoiceControlApp.Commands;
using DigitalVoiceControlApp.Config;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

public class DmrViewModel : ViewModelBase
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    string _callsign = string.Empty;
    string _srcId = string.Empty;
    string _dstId = string.Empty;
    string _rptId = string.Empty;
    string _rxTa = string.Empty;

    string _flco = string.Empty;

    public ICommand ClearLastHeardCommand { get; }

    public ObservableCollection<LastHeardItemDmr> _lastHeard = [];
    public ObservableCollection<LastHeardItemDmr> LastHeard
    {
        get => _lastHeard;
        set { _lastHeard = value; OnPropertyChanged(); }
    }

    public string Callsign
    {
        get => _callsign;
        set { _callsign = value; OnPropertyChanged(); }
    }

    public string Flco
    {
        get => _flco;
        set { _flco = value; OnPropertyChanged(); }
    }

    public string DstId
    {
        get => _dstId;
        set { _dstId = value; OnPropertyChanged(); }
    }

    public string RptId
    {
        get => _rptId;
        set { _rptId = value; OnPropertyChanged(); }
    }

    public string SrcId
    {
        get => _srcId;
        set { _srcId = value; OnPropertyChanged(); }
    }
    public string RxTa
    {
        get => _rxTa;
        set { _rxTa = value; OnPropertyChanged(); }
    }

    readonly ConcurrentDictionary<int, DmrUserData> _dmrUserCache = [];

    // Verhindert, dass für dieselbe srcId mehrfach parallel gefetcht wird (z.B. weil
    // ConsumeDmrData bei einer laufenden Übertragung alle ~20ms erneut aufgerufen wird).
    readonly ConcurrentDictionary<int, byte> _pendingFetches = [];

    static readonly HashSet<int> _unregisteredDmrId = [];

    /// <summary>
    /// Constructor.
    /// </summary>
    public DmrViewModel()
    {
        ClearLastHeardCommand = new RelayCommand<object?>(_ => ClearLastHeard());

        // Get all User specific TGs into internal DmrId lookup table.
        //foreach (var kvp in DmrTalkgroups.All) 
        //{
        //    _dmrUserCache[kvp.Key] = (kvp.Value.Name, string.Empty);
        //}
    }

    public void ConsumeDmrData(DmrSessionContext sessionCtx)
    {
        logger.Debug($"sessionCtx = {sessionCtx}");

        if (sessionCtx.TransceiveMode == TransceiveMode.Rx && (sessionCtx.RxStreamState is StreamState.End or StreamState.Idle or StreamState.Lost))
        {
            // Clear model/view
            Dispatcher.UIThread.Post(() => { Clear(); });
            sessionCtx.RxTalkerAlias = "";
            return; // no last heard
        }

        // Copy/save session attributes
        int dstId = 0;
        int rptId = 0;
        int srcId = 0;
        string rxTa = string.Empty;
        TransceiveMode tm = sessionCtx.TransceiveMode;
        DmrUserData? userData = null;

        switch (tm)
        {
            case TransceiveMode.Rx:
                dstId = sessionCtx.RxDstId;
                rptId = sessionCtx.RxRptId;
                srcId = sessionCtx.RxSrcId;
                rxTa = sessionCtx.RxTalkerAlias;

                if (_dmrUserCache.TryGetValue(srcId, out var cached))
                {
                    userData = cached; // schon bekannt -> sofort verfügbar, kein Fetch nötig
                }
                else
                {
                    // userData bleibt null -> Anzeige/Last-Heard-Eintrag zeigt zunächst NOCALL.
                    // Fetch läuft im Hintergrund, Update erfolgt separat (analog Java), sobald fertig.
                    // TryAdd verhindert, dass bei laufender Übertragung (alle ~20ms erneuter Aufruf)
                    // mehrfach parallel für dieselbe srcId gefetcht wird.
                    if (_pendingFetches.TryAdd(srcId, 0))
                    {
                        _ = FetchAndUpdateUserDataAsync(srcId);
                    }
                }
                break;
            case TransceiveMode.Tx:
                dstId = sessionCtx.TxDstId;
                rptId = sessionCtx.TxRptId;
                srcId = sessionCtx.TxSrcId;
                break;
        }

        Dispatcher.UIThread.Post(() =>
        {
            Flco = sessionCtx.RxFlco.ToString();
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

            var newItem = new LastHeardItemDmr(dstId, srcId, userData);
            LastHeard.Insert(0, newItem); // insert at the top

            // Limit to max 50 entries
            if (LastHeard.Count > 50)
            {
                LastHeard.RemoveAt(50); // remove from the end
            }
        });
    }

    /// <summary>
    /// Lädt die Nutzerdaten für eine srcId im Hintergrund und aktualisiert - falls der
    /// zugehörige Last-Heard-Eintrag noch existiert - dessen Anzeige (analog zur Java-Version).
    /// Existiert der Eintrag nicht mehr (z.B. durch die 50er-Begrenzung verdrängt), passiert
    /// nichts weiter; der Cache wird trotzdem befüllt.
    /// </summary>
    private async Task FetchAndUpdateUserDataAsync(int srcId)
    {
        logger.Debug($"Fetch gestartet für srcId={srcId}");
        try
        {
            var fetched = await DmrUserDataReader.GetUserAsync(srcId);
            logger.Debug($"Fetch-Ergebnis für srcId={srcId}: Callsign={fetched?.Callsign ?? "null (kein Treffer)"}");

            if (fetched != null)
            {
                _dmrUserCache[srcId] = fetched; // cache it

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
                            // Verlässlicher als PropertyChanged, unabhängig von der XAML-Bindungsart
                            // (z.B. falls direkt auf ToString() statt auf einzelne Properties gebunden wird).
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
    internal void Clear()
    {
        logger.Debug($"");
        Callsign = "";
        Flco = string.Empty;
        DstId = string.Empty;
        RptId = string.Empty;
        SrcId = string.Empty;
        RxTa = string.Empty;
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
    }
}