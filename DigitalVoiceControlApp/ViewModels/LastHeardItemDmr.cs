using System;
using System.Text;

namespace DigitalVoiceControlApp.ViewModels;

public class LastHeardItemDmr
{
    public int DmrId { get; } = 0;
    public int DstId { get; } = 0;
    public string Callsign { get; } = "";
    public string Name { get; } = "";

    private readonly string displayValue;

    public LastHeardItemDmr(int dmrId, int dstId, string callsign, string name)
    {
        DmrId = dmrId;
        DstId = dstId;
        Callsign = callsign;
        Name = name;

        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("HH:mm:ss MMM/dd", System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(',').Append(DmrId);
        sb.Append(',').Append(DstId);
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
