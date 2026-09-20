using System;
using System.Text;

namespace DigitalVoiceControlApp.ViewModels;

public class LastHeardItemDStar
{
    public string Dst { get; } = string.Empty;
    public string Gw { get; } = string.Empty;
    public string Src { get; } = string.Empty;
    public string? Reflector { get; }

    private readonly string displayValue;

    public LastHeardItemDStar(string dst, string gw, string src, string? reflector)
    {
        Dst = dst;
        Gw = gw;
        Src = src;
        Reflector = reflector;

        var sb = new StringBuilder();
        sb.Append(DateTime.UtcNow.ToString("HH:mm:ss MMM/dd", System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(',').Append(Reflector);
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