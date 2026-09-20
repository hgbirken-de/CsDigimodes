using Avalonia.Threading;
using DigitalVoice.Common;
using DigitalVoice.Fusion;
using DigitalVoiceControlApp.Commands;
using FusionCodec;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace DigitalVoiceControlApp.ViewModels;

public class FusionViewModel : ViewModelBase
{

    public ICommand ClearLastHeardCommand { get; }

    public ObservableCollection<LastHeardItemYsf> LastHeard { get; } = [];

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

    string _dataType = string.Empty;
    public string DataType
    {
        get => _dataType;
        set { _dataType = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public FusionViewModel()
    {
        ClearLastHeardCommand = new RelayCommand<object?>(_ => ClearLastHeard());
    }

    public void ConsumeYsfData(YsfSessionContext sessionCtx)
    {

        if (sessionCtx.TransceiveMode == TransceiveMode.Rx && (sessionCtx.StreamState is StreamState.End or StreamState.Idle or StreamState.Lost))
        {
            Dispatcher.UIThread.Post(() => { Clear(); });
            return; // no last heard
        }

        // Copy/save data from session
        TransceiveMode tm = sessionCtx.TransceiveMode;
        string? gw = sessionCtx.Gw;
        string? dst = sessionCtx.Dst;
        string? src = sessionCtx.Src;
        DataType dt = sessionCtx.Dt;

        Dispatcher.UIThread.Post(() =>
        {
            if (!string.IsNullOrEmpty(src))
                Src = src;
            if (!string.IsNullOrEmpty(dst))
                Dst = dst;

            Gw = gw ?? "";
            DataType = dt.ToString();

            // Find index of the first item in the first 2 with the same src callsign
            //var existingIndex = LastHeard.Take(2).Select((item, index) => new { item, index }).FirstOrDefault(x => x.item.Src == Src)?.index;
            //if (existingIndex != null)
            //{
            //    LastHeard.RemoveAt(existingIndex.Value);
            //}
            for (int i = 0; i < LastHeard.Count; i++)
            {
                if (LastHeard[i].Src == Src)
                {
                    LastHeard.RemoveAt(i);
                    break;
                }
            }

            var newItem = new LastHeardItemYsf(Dst, Gw, Src, DataType);
            LastHeard.Insert(0, newItem); // Always insert the new item at the top

            // Limit to max 100 entries
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
        Gw = string.Empty;
        Dst = string.Empty;
        Src = string.Empty;
        DataType = string.Empty;
    }

    public void ClearLastHeard()
    {
        LastHeard.Clear();
    }
}