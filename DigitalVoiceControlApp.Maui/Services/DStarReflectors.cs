using DigitalVoice.DStar.Dcs;
using DigitalVoice.DStar.Ref;
using DigitalVoice.DStar.Xrf;
using NLog;

namespace DigitalVoiceControlApp.Maui.Services;

/// <summary>
/// Die Reflektorlisten für DCS, REF und XRF, aus denselben Quellen wie in der Desktop-App (<c>DcsHosts</c>,
/// <c>DPlusHostRepository</c> und <c>XrfHosts</c>). Die Anzeigenamen sind die Reflektor-IDs selbst (z.B. <c>DCS001</c>).
/// Die Namensräume der D-STAR-Klassen stehen nur hier, damit sie das große ViewModel nicht berühren.
/// </summary>
public static class DStarReflectors
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>Die DCS-Reflektoren in der Reihenfolge der Hostliste.</summary>
    public static List<string> GetDcsIds() => Collect("DCS", () =>
    {
        List<string> ids = [];
        foreach ((string id, string ipAddr) in DcsHosts.All)
            ids.Add(id);
        return ids;
    });

    /// <summary>Die REF-Reflektoren (DPlus) in der Reihenfolge der Hostliste.</summary>
    public static List<string> GetRefIds() => Collect("REF", () =>
    {
        List<string> ids = [];
        foreach ((string id, string ipAddr) in DPlusHostRepository.All)
            ids.Add(id);
        return ids;
    });

    /// <summary>Die XRF-Reflektoren (DExtra) in der Reihenfolge der Hostliste.</summary>
    public static List<string> GetXrfIds() => Collect("XRF", () =>
    {
        List<string> ids = [];
        foreach ((string id, string ipAddr) in XrfHosts.All)
            ids.Add(id);
        return ids;
    });

    // Eine fehlende oder defekte Hostliste darf die App nicht beim Start oder Moduswechsel zum Absturz bringen
    private static List<string> Collect(string name, Func<List<string>> read)
    {
        try
        {
            return read();
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Reading the {name} reflector list failed.");
            return [];
        }
    }
}
