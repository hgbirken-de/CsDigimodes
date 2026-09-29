using DigitalVoice.Nxdn;
using NLog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DigitalVoiceControlApp.ViewModels;

public class LastHeardItemNxdn : INotifyPropertyChanged
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public int GwId { get; } = 0;
    public int SrcId { get; } = 0;
    public int DstId { get; } = 0;
    public string Callsign { get; set; } = "";

    private string displayValue;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="gwId">gateway/reflector id</param>
    /// <param name="srcId">source NXDN-ID</param>
    /// <param name="dstId">destination id</param>
    /// <param name="ud">user data (optional - null solange der radioid.net-Fetch noch läuft)</param>
    public LastHeardItemNxdn(int gwId, int srcId, int dstId, NxdnUserData? ud)
    {
        GwId = gwId;
        SrcId = srcId;
        DstId = dstId;

        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("HH:mm:ss MMM/dd", System.Globalization.CultureInfo.InvariantCulture));
        sb.Append(',').Append(GwId);
        sb.Append(',').Append(SrcId);

        if (ud == null)
        {
            Callsign = "NOCALL";
            sb.Append(',').Append(Callsign);
        }
        else
        {
            Callsign = ud.Callsign ?? "NOCALL";
            sb.Append(',').Append(Callsign);
            sb.Append(',').Append(ud.Name ?? "NONAME");
            sb.Append(',').Append(ud.City ?? "NOCITY");
            sb.Append(',').Append(ud.Country ?? "NOCOUNTRY");
        }

        displayValue = sb.ToString();
    }

    public override string ToString()
    {
        return displayValue;
    }

    /// <summary>
    /// Update this LastHeardItemNxdn with user data (z.B. nachdem der radioid.net-Fetch
    /// asynchron durchgelaufen ist).
    /// </summary>
    /// <param name="userData">current user data</param>
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

            logger.Debug($"srcId={SrcId} -> Callsign jetzt = {Callsign}");

            OnPropertyChanged(nameof(Callsign));
            // Leerer/nullter Propertyname signalisiert Avalonia (wie WPF) "alle Bindings
            // auf diesem Objekt neu auswerten" - wichtig falls die UI z.B. per {Binding}
            // direkt auf ToString() statt auf einzelne Properties bindet.
            OnPropertyChanged(string.Empty);
        }
    }
}
