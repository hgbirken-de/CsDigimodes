using System.Text;
using System.Text.RegularExpressions;

namespace YsfReflector;

/// <summary>
/// Represents a YSFD transmit stream for a gateway client.
/// </summary>
public class YsfdStream
{
    /// <summary>Gateway ID</summary>
    public int GwId { get; set; }

    /// <summary>Timeout counter or timestamp</summary>
    public float Timeout { get; set; }

    /// <summary>Gateway callsign</summary>
    public string Gateway { get; set; } = string.Empty;

    /// <summary>Source callsign from packet</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Destination callsign</summary>
    public string Destination { get; set; } = string.Empty;

    /// <summary>Stream ID</summary>
    public int StreamId { get; set; }

    /// <summary>Stream start timestamp</summary>
    public float StartTime { get; set; }

    /// <summary>Latitude (degrees)</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude (degrees)</summary>
    public double Longitude { get; set; }

    /// <summary>DG-ID assigned to this stream</summary>
    public int DgId { get; set; }

    /// <summary>Flag for local stream</summary>
    public bool IsLocal { get; set; } = false;

    /// <summary>Rem12 field (payload-specific)</summary>
    public string Rem12 { get; set; } = string.Empty;

    /// <summary>Rem34 field (payload-specific)</summary>
    public string Rem34 { get; set; } = string.Empty;

    /// <summary>Radio code for the stream</summary>
    public int RadioCode { get; set; }

    /// <summary>Coordinate send flag</summary>
    public bool CoordSend { get; set; }

    /// <summary>Cleaned source callsign (suffix removed)</summary>
    public string SourceClean { get; set; } = string.Empty;

    /// <summary>Stream type (0 = unknown, 1 = voice, etc.)</summary>
    public int StreamType { get; set; }

    /// <summary>Optional additional fields / counters</summary>
    public int Extra1 { get; set; }
    public int Extra2 { get; set; }
    public int Extra3 { get; set; }

    /// <summary>
    /// Creates a new empty YsfdStream.
    /// </summary>
    public YsfdStream() {}

    /// <summary>
    /// Creates a <see cref="YsfdStream"/> instance by decoding callsign fields from a YSF packet.
    /// </summary>
    /// <param name="packet">The raw YSF data packet containing header information.</param>
    /// <returns>A populated <see cref="YsfdStream"/> with gateway, source, and destination callsigns.</returns>
    public static YsfdStream FromPacket(byte[] packet)
    {
        string callsigns = Encoding.ASCII.GetString(packet, 4, 30);
        var stream = new YsfdStream
        {   
            Gateway = callsigns[..10].Trim(),
            Source = Encoding.ASCII.GetString(packet, 14, 10).Trim(),
            SourceClean = Regex.Split(Encoding.ASCII.GetString(packet, 14, 10), "[-/]")[0].Trim(),
            Destination = callsigns.Substring(24, 10).Trim()
        };

        return stream;
    }
}