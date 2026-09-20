using DigitalVoice.Dmr;
using System;
using System.Text;

namespace DigitalVoiceControlApp.ViewModels;

public class LastHeardItemDmr
{
    public string Callsign { get; } = "";
    public int DstId { get; } = 0;
    public int SrcId { get; } = 0;
   
    public string Name { get; } = "";

    private readonly string displayValue;

    public LastHeardItemDmr(int dstId, int srcId, DmrUserData? userData)
    {
        SrcId = srcId;
        DstId = dstId;
        
        Callsign = userData != null ? (userData.Callsign ?? "NOCALL") : "NOCALL";
        Name = userData != null ? (userData.Callsign ?? "NONAME") : "NONAME";
        
        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("HH:mm:ss MMM/dd", System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(',').Append(DstId);
        sb.Append(',').Append(SrcId);
        if (!string.IsNullOrEmpty(Callsign))
        {
            sb.Append(',').Append(Callsign);
            if (!string.IsNullOrEmpty(Name))
            {
                sb.Append(',').Append(Name);
            }
        }

        displayValue = sb.ToString();
    }

    public override string ToString()
    {
        return displayValue;
    }
}
