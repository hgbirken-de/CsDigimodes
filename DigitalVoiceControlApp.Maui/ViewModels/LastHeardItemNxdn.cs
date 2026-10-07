using DigitalVoice.Nxdn;
using NLog;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>
/// Ein Eintrag der NXDN-Last-Heard-Liste. Die Zeile entspricht der Java-Fassung: Zeit, Gateway, Quelle, Rufzeichen, Name, Ort,
/// Land.
/// </summary>
public class LastHeardItemNxdn : INotifyPropertyChanged
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public string Callsign { get; set; } = "";
    public int DstId { get; set; } = 0;
    public int GwId { get; set; } = 0;
    public int SrcId { get; set; } = 0;

    private string displayValue;

    /// <summary>Die Anzeigezeile. Daran bindet die Liste.</summary>
    public string Display => displayValue;

    public LastHeardItemNxdn(int gwId, int srcId, int dstId, NxdnUserData? userData)
    {
        GwId = gwId;
        SrcId = srcId;
        DstId = dstId;

        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("HH:mm:ss MMM/dd", System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(',').Append(gwId);
        sb.Append(',').Append(srcId);

        if (userData == null)
        {
            Callsign = "NOCALL";
            sb.Append(',').Append(Callsign);
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

    public override string ToString() => displayValue;

    /// <summary>Ergänzt den Eintrag um die nachgeladenen Nutzerdaten (die Zeit der Zeile bleibt erhalten).</summary>
    public void Update(NxdnUserData? userData)
    {
        if (userData == null) return;

        int i = displayValue.IndexOf(',');
        if (i != -1)
        {
            var sb = new StringBuilder();
            sb.Append(displayValue.AsSpan(0, i));
            sb.Append(',').Append(GwId);
            sb.Append(',').Append(SrcId);
            Callsign = userData.Callsign ?? "NOCALL";
            sb.Append(',').Append(Callsign);
            sb.Append(',').Append(userData.Name ?? "NONAME");
            sb.Append(',').Append(userData.City ?? "NOCITY");
            sb.Append(',').Append(userData.Country ?? "NOCOUNTRY");

            displayValue = sb.ToString();

            logger.Debug($"srcId={SrcId} -> Callsign now = {Callsign}");

            OnPropertyChanged(nameof(Callsign));
            OnPropertyChanged(nameof(Display));
        }
    }
}
