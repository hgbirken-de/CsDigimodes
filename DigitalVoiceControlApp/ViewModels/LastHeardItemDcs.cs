using System;
using System.Text;

namespace DigitalVoiceControlApp.ViewModels;

public class LastHeardItemDcs
{
    public string Dst { get; } = string.Empty;
    public string Gw { get; } = string.Empty;
    public string Src { get; } = string.Empty;
    public string Type { get; } = string.Empty;

    private readonly string displayValue;

    public LastHeardItemDcs(string dst, string gw, string src, string type)
    {
        Dst = dst;
        Gw = gw;
        Src = src;
        Type = type;

        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("HH:mm:ss MMM/dd", System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(',').Append(Gw);
        sb.Append(',').Append(Src);
        sb.Append(',').Append(Dst);
        displayValue = sb.ToString();
    }

    public override string ToString()
    {
        return displayValue;
    }
}
