using Avalonia.Threading;
using DigitalVoice.Common;
using DigitalVoice.DStar.Common;
using DigitalVoiceControlApp.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

public class DcsViewModel : ViewModelBase
{

    public ICommand ClearLastHeardCommand { get; }

    public ObservableCollection<LastHeardItemDcs> LastHeard { get; } = [];

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

    string _gw = string.Empty;
    public string Gw
    {
        get => _gw;
        set { _gw = value; OnPropertyChanged(); }
    }

    string _message = string.Empty;
    public string Message
    {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public DcsViewModel()
    {
        ClearLastHeardCommand = new RelayCommand<object?>(_ => ClearLastHeard());
    }

    public void ConsumeDcsData(DStarSessionContext state)
    {
        Dispatcher.UIThread.Post(() =>
        {

            if (state.RxStreamState is StreamState.End or StreamState.Lost)
            {
                Clear();
            }
            else
            {
                switch(state.TransceiveMode)
                {
                    case TransceiveMode.Rx:
                        Gw = state.RxRptr1;
                        Src = state.RxSrc;
                        Dst = state.RxUrCall;
                        Message = state.RxUsrMsg;
                        break;
                    case TransceiveMode.Tx:
                        Gw = state.TxRptr1;
                        Src = state.TxMyCall;
                        Dst = state.TxUrCall;
                        Message = state.TxUsrMsg;
                        return; // no last heard
                }

                // Find index of the first item in the first 2 with the same src callsign
                var existingIndex = LastHeard.Take(2).Select((item, index) => new { item, index }).FirstOrDefault(x => x.item.Src == Src)?.index;
                if (existingIndex != null)
                {
                    LastHeard.RemoveAt(existingIndex.Value);
                }

                var newItem = new LastHeardItemDcs(Dst, Gw, Src, "");
                LastHeard.Insert(0, newItem); // Always insert the new item at the top

                // Limit to max 100 entries
                if (LastHeard.Count > 100)
                {
                    LastHeard.RemoveAt(100); // remove from the end
                }
            }
        });
    }

    /// <summary>
    /// Clear simple properties of this model.
    /// </summary>
    internal void Clear()
    {
        Gw = string.Empty;
        Dst = string.Empty;
        Src = string.Empty;
        Message = string.Empty;
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
    }
}