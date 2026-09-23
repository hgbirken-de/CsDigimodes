using Avalonia.Threading;
using DigitalVoice.Common;
using DigitalVoice.Nxdn;
using DigitalVoiceControlApp.Commands;
using DigitalVoiceControlApp.Config;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

public class NxdnViewModel : ViewModelBase
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
        //logger.Debug($"sessionContext = {sessionContext}");

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

        (string callsign, string name, _, _, _, _) = NxdnUsers.GetUserInfo(srcId);

        Dispatcher.UIThread.Post(() =>
        {
            DstId = dstId > 0 ? dstId.ToString() : string.Empty;
            GwId = gwId > 0 ? gwId.ToString() : string.Empty;
            SrcId = srcId.ToString();
            
            switch (tm)
            {
                case TransceiveMode.Rx:
                    Callsign = callsign ?? "N0CALL";
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

            var newItem = new LastHeardItemNxdn(gwId, srcId, dstId, Callsign, name);
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
    private void Clear()
    {
        logger.Debug($"");
        Callsign = " ";
        DstId = string.Empty;
        GwId = string.Empty;
        SrcId = string.Empty;
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
    }
}