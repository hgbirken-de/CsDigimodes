using Avalonia.Threading;
using DigitalVoice.Common;
using DigitalVoice.Config;
using DigitalVoice.Dmr;
using DigitalVoiceControlApp.Commands;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

public class DmrViewModel : ViewModelBase
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    string _callsign = string.Empty;
    string _srcId = string.Empty;
    string _dstId = string.Empty;
    string _rptId = string.Empty;

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

    readonly ConcurrentDictionary<int, (string, string)> _dmrUserCache = []; // value is tuple of (callsign, name)

    int _lastRequestId = 0;

    static readonly HashSet<int> _unregisteredDmrId = []; 

    /// <summary>
    /// Constructor.
    /// </summary>
    public DmrViewModel()
    {
        ClearLastHeardCommand = new RelayCommand<object?>(_ => ClearLastHeard());

        // Get all User specific TGs into internal DmrId lookup table.
        foreach (var kvp in DmrTalkgroups.All) 
        {
            _dmrUserCache[kvp.Key] = (kvp.Value.Name, string.Empty);
        }
    }

    public async void ConsumeDmrData(DmrSessionContext sessionCtx)
    {
        logger.Debug($"sessionCtx = {sessionCtx}");

        if (sessionCtx.TransceiveMode == TransceiveMode.Rx && (sessionCtx.RxStreamState is StreamState.End or StreamState.Idle or StreamState.Lost))
        {
            // Clear model/view
            Dispatcher.UIThread.Post(() => { Clear(); });
            return; // no last heard
        }

        int requestId = Interlocked.Increment(ref _lastRequestId);

        string callsign = string.Empty;
        string name = string.Empty;

        // Copy/save session attributes
        int dstId = 0;
        int rptId = 0;
        int srcId = 0;
        TransceiveMode tm = sessionCtx.TransceiveMode;
        
        switch (tm)
        {
            case TransceiveMode.Rx:
                dstId = sessionCtx.RxDstId;
                rptId = sessionCtx.RxRptId;
                srcId = sessionCtx.RxSrcId;
                if (_dmrUserCache.TryGetValue(srcId, out var value))
                {
                    callsign = value.Item1;
                    name = value.Item2;
                }
                else
                {
                    try
                    {
                        var user = await DmrUserInfoReader.GetUserAsync(srcId);
                        if (user != null)
                        {
                            callsign = user.Callsign ?? "N0CALL";
                            name = user.Name ?? "None";
                        }
                        else
                        {
                            logger.Warn($"Unable to read user data, srcId = {srcId}");
                            callsign = sessionCtx.RxSrcId.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"Error during read of user data, srcId = {srcId}");
                        callsign = srcId.ToString();
                    }
                    _dmrUserCache[srcId] = (callsign, name); // cache it

                    // The following code is required because async call of "DmrUserInfoReader.GetUserAsync"
                    if (requestId != _lastRequestId)
                        return; // this result is outdated -> do not update the view model
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

            switch (tm)
            {
                case TransceiveMode.Rx:
                    Callsign = callsign;
                    break;
                case TransceiveMode.Tx:
                    var us = UserSettings.Instance();
                    Callsign = us.Common.Callsign;
                    return; // no last heard with TX
            }

            // Find index of the first item in the first 2 with the same DmrId
            //var existingIndex = LastHeard.Take(2).Select((item, index) => new { item, index }).FirstOrDefault(x => x.item.DmrId == clientState.RxSrcId)?.index;
            //if (existingIndex != null)
            //{
            //    LastHeard.RemoveAt(existingIndex.Value);
            //}
            for (int i = 0; i < LastHeard.Count; i++)
            {
                if (LastHeard[i].DmrId == srcId)
                {
                    LastHeard.RemoveAt(i);
                    break;
                }
            }

            var newItem = new LastHeardItemDmr(srcId, callsign, name);
            LastHeard.Insert(0, newItem); // insert the new item at the top

            // Limit to max 50 entries
            if (LastHeard.Count > 50)
            {
                LastHeard.RemoveAt(50); // remove from the end
            }
        });
    }

    /// <summary>
    /// Clear simple properties of this model.
    /// </summary>
    internal void Clear()
    {
        logger.Debug($"");
        Callsign = " ";
        Flco = string.Empty;
        DstId = string.Empty;
        RptId = string.Empty;
        SrcId = string.Empty;
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
    }
}