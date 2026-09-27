using DigitalVoice.Dmr;
using NLog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DigitalVoiceControlApp.ViewModels;

public class LastHeardItemDmr : INotifyPropertyChanged
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public string Callsign { get; set; } = "";
    public int DstId { get; set; } = 0;
    public int SrcId { get; set; } = 0;

    private string displayValue;

    public LastHeardItemDmr(int dstId, int srcId, DmrUserData? userData)
    {
        DstId = dstId;
        SrcId = srcId;

        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("HH:mm:ss MMM/dd", System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(',').Append(dstId);
        sb.Append(',').Append(srcId);

        if (userData == null)
        {
            sb.Append(',').Append("NOCALL");
        }
        else
        {
            Callsign = userData.Callsign ?? "NOCALL";
            sb.Append(',').Append(Callsign);
            sb.Append(',').Append(userData.Name ?? "NONAME");
            sb.Append(',').Append(userData.City ?? "NOCITY");
            sb.Append(',').Append(userData.Country ?? "NOCOUNTRY");
        }

        displayValue = sb.ToString();
    }

    public override string ToString()
    {
        return displayValue;
    }

    /// <summary>
    /// Update this LastHeardItemDmr with user data
    /// </summary>
    /// <param name="userData">current user data</param>
    public void Update(DmrUserData? userData)
    {
        if (userData == null) return;

        int i = displayValue.IndexOf(',');
        if (i != -1)
        {
            var sb = new StringBuilder();
            sb.Append(displayValue.AsSpan(0, i));
            sb.Append(',').Append(DstId);
            sb.Append(',').Append(SrcId);
            Callsign = userData.Callsign ?? "NOCALL";
            sb.Append(',').Append(Callsign);
            sb.Append(',').Append(userData.Name ?? "NONAME");
            sb.Append(',').Append(userData.City ?? "NOCITY");
            sb.Append(',').Append(userData.Country ?? "NOCOUNTRY");

            displayValue = sb.ToString();

            logger.Debug($"srcId={SrcId} -> Callsign jetzt = {Callsign}");

            OnPropertyChanged(nameof(Callsign));
            // Leerer/nullter Propertyname signalisiert Avalonia (wie WPF) "alle Bindings
            // auf diesem Objekt neu auswerten" - wichtig falls die UI z.B. per {Binding}
            // direkt auf ToString() statt auf einzelne Properties bindet.
            OnPropertyChanged(string.Empty);
        }
    }
}
