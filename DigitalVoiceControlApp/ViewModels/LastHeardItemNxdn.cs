using System;
using System.Text;

namespace DigitalVoiceControlApp.ViewModels;

public class LastHeardItemNxdn
{
    public int GwId { get; } = 0;
    public int SrcId { get; } = 0;
    public int DstId { get; } = 0;
    public string Callsign { get; } = "";
    public string Name { get; } = "";

    private readonly string displayValue;

    public LastHeardItemNxdn(int gwId, int srcId, int dstId, string callsign, string name)
    {
        GwId = gwId;
        SrcId = srcId;
        DstId = dstId;
        Callsign = callsign;
        Name = name;

        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("HH:mm:ss MMM/dd", System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(',').Append(GwId);
        sb.Append(',').Append(SrcId);
        //sb.Append(',').Append(DstId);
        sb.Append(',').Append(string.IsNullOrEmpty(Callsign) ? "N0CALL" : Callsign);
        if (!string.IsNullOrEmpty(name))
            sb.Append(',').Append(name);

        displayValue = sb.ToString();
    }

    public override string ToString()
    {
        return displayValue;
    }
}
