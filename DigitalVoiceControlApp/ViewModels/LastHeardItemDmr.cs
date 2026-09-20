using DigitalVoice.Dmr;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace DigitalVoiceControlApp.ViewModels;

public class LastHeardItemDmr
{
    public string Callsign { get; set; } = "";
    public int SrcId { get; set; } = 0;

    private readonly string displayValue;

    public LastHeardItemDmr(int dstId, int srcId, DmrUserData? userData)
    {
        
        SrcId  = srcId;

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
}
