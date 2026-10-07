using DigitalVoice.Dmr;
using NLog;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DigitalVoiceControlApp.Maui.ViewModels;

/// <summary>Ein Eintrag der Last-Heard-Liste (Gegenstück zur gleichnamigen Klasse der Avalonia-App).</summary>
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

    /// <summary>Die Anzeigezeile (Zeit, Ziel, Quelle, Rufzeichen, Name, Ort, Land). Daran bindet die Liste.</summary>
    public string Display => displayValue;

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

    public override string ToString() => displayValue;

    /// <summary>Ergänzt den Eintrag um die nachgeladenen Nutzerdaten (die Zeit der Zeile bleibt erhalten).</summary>
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
            OnPropertyChanged(nameof(Display));
        }
    }
}
