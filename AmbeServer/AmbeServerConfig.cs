
namespace AmbeServer;

/// <summary>
/// Class to hold config params of the AMBE server.
/// </summary>
public class AmbeServerConfig
{
    /// <summary>
    /// Host/IP address used by the AMBE server
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Port used by the AMBE server
    /// </summary>
    public int Port { get; set; } = 2460;

    /// <summary>
    /// Record incomming UDP packets
    /// </summary>
    public bool RecordPacket { get; set; } = false;

    /// <summary>
    /// Record PCM packets
    /// </summary>
    public bool RecordPcm { get; set; } = false;

    /// <summary>
    /// Serial port used by the AMBE3000R stick
    /// </summary>
    public string SerialPort { get; set; } = string.Empty; // used for AMBE3000R stick

    public bool Debug { get; set; } = false;
}
