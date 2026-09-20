using Avalonia.Threading;
using DigitalVoice.Common;
using DigitalVoice.DStar.Common;
using DigitalVoiceControlApp.Commands;
using NLog;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

public class DStarViewModel : ViewModelBase
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public ICommand ClearLastHeardCommand { get; }

    public ObservableCollection<LastHeardItemDStar> LastHeard { get; } = [];

    string _src = string.Empty;
    public string Src
    {
        get => _src;
        set { _src = value; OnPropertyChanged(); }
    }

    string _dst = string.Empty;
    public string Dst
    {
        get => _dst;
        set { _dst = value; OnPropertyChanged(); }
    }

    string _rpt1 = string.Empty;
    public string Rpt1
    {
        get => _rpt1;
        set { _rpt1 = value; OnPropertyChanged(); }
    }

    string _rpt2 = string.Empty;
    public string Rpt2
    {
        get => _rpt2;
        set { _rpt2 = value; OnPropertyChanged(); }
    }

    string _message = string.Empty;
    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    string _gpsData = string.Empty;
    public string GpsData
    {
        get => _gpsData;
        set { _gpsData = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public DStarViewModel()
    {
        ClearLastHeardCommand = new RelayCommand<object?>(_ => ClearLastHeard());
    }

    public void ConsumeDStarData(DStarSessionContext sessionCtx)
    {
        //logger.Debug($"sessionCtx = {sessionCtx}");

        if (sessionCtx.RxStreamState is StreamState.End or StreamState.Idle or StreamState.Lost)
        {
            Dispatcher.UIThread.Post(() => { Clear(); });
            return; // no last heard handling
        }

        // Copy/save session attributes
        TransceiveMode tm = sessionCtx.TransceiveMode;
        string? reflector = sessionCtx.Reflector;
        string rptr1 = string.Empty;
        string rptr2 = string.Empty;
        string src = string.Empty;
        string dst = string.Empty;
        string gpsData = string.Empty;
        string usrMsg = string.Empty;

        switch (tm)
        {
            case TransceiveMode.Rx:
                rptr1 = sessionCtx.RxRptr1;
                rptr2 = sessionCtx.RxRptr2;
                src = sessionCtx.RxSrc;
                Dst = sessionCtx.RxUrCall;
                gpsData = sessionCtx.RxGpsData;
                usrMsg = sessionCtx.RxUsrMsg;
                break;
            case TransceiveMode.Tx:
                rptr1 = sessionCtx.TxRptr1;
                src = sessionCtx.TxMyCall;
                dst = sessionCtx.TxUrCall;
                usrMsg = sessionCtx.TxUsrMsg;
                break;
        }

        Dispatcher.UIThread.Post(() =>
        {
            switch (tm)
            {
                case TransceiveMode.Rx:
                    Rpt1 = rptr1;
                    Rpt2 = rptr2;
                    Src = src;
                    Dst = dst;
                    GpsData = gpsData;
                    Message = usrMsg;
                    break;
                case TransceiveMode.Tx:
                    Rpt1 = rptr1;
                    Src = src;
                    Dst = dst;
                    Message = usrMsg;
                    return; // no last heard
            }

            //// Find index of the first item in the first 2 with the same src callsign
            //var existingIndex = LastHeard.Take(2).Select((item, index) => new { item, index }).FirstOrDefault(x => x.item.Src == Src)?.index;
            //if (existingIndex != null)
            //{
            //    LastHeard.RemoveAt(existingIndex.Value);
            //}

            //var newItem = new LastHeardItemDStar {Gw = Gw, Src = Src, Dst = Dst};
            //LastHeard.Insert(0, newItem); // Always insert the new item at the top

            for (int i = 0; i < LastHeard.Count; i++)
            {
                if (LastHeard[i].Src == Src)
                {
                    LastHeard.RemoveAt(i);
                    break;
                }
            }
            var newItem = new LastHeardItemDStar(Dst, Rpt1, Src, reflector);
            LastHeard.Insert(0, newItem); // Always insert the new item at the top

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
        Rpt1 = string.Empty;
        Rpt2 = string.Empty;
        Dst = string.Empty;
        Src = string.Empty;
        GpsData = string.Empty;
        Message = string.Empty;
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
    }
}