using DigitalVoice.Common;
using FusionCodec;

namespace DigitalVoice.Fusion;

/// <summary>
/// Represents the current state of a YSF/FCS (Yaesu System Fusion) client session, including callsign information, 
/// frame header (FICH) fields, transmission state, and stream status.
/// </summary>
/// <param name="isYsf">Defines if this instance is related to a YSF or FCS client: true -> YSF, false -> FCS</param>
public class YsfSessionContext(bool isYsf)
{
    public bool IsYsf { get; } = isYsf;

    #region "Callsign properties"
    public string? Dst { get; set; }
    public string? Gw { get; set; }
    public string? Src { get; set; }

    #endregion

    #region "FICH properties"

    public CallMode Cm { get; set; }
    public DataType Dt { get; set; }
    public FrameInformation Fi { get; set; }
    public int Fn { get; set; } = -1;
    public int Ft { get; set; } = 0;
    #endregion

    public int FrameCount { get; set; } = 0;

    public bool IsLastFrame { get; set; } = false;

    public List<byte[]> AmbeData { get; set; } = [];

    public StreamState StreamState { get; set; } = StreamState.End;

    public int RxPingCount { get; set; } = 0;
    public int TxPingCount { get; set; } = 0;


    public TransceiveMode TransceiveMode { get; set; } = TransceiveMode.Rx;
    public List<byte[]> TxAmbeData { get; } = new List<byte[]>(5);
    public int TxFrameNumber { get; set; } = 0;

    /// <summary>
    /// Updates this instance with values of the argument <see cref="DecodeResult"/> object.
    /// </summary>
    /// <param name="decodeResult">the object whose values ​​are used for the enrichment</param>
    internal void Update(DecodeResult decodeResult)
    {
        if (!string.IsNullOrEmpty(decodeResult.Dst))
            Dst = decodeResult.Dst;

        if (!string.IsNullOrEmpty(decodeResult.Gw))
            Gw = decodeResult.Gw;
        
        if (!string.IsNullOrEmpty(decodeResult.Src))
            Src = decodeResult.Src;

        AmbeData.Clear();
        AmbeData.AddRange(decodeResult.AmbeData);

        Cm = decodeResult.Cm;
        Dt = decodeResult.Dt;
        Fi = decodeResult.Fi;
        Fn = decodeResult.Fn;
        Ft = decodeResult.Ft;
        FrameCount = decodeResult.FrameCount;
        IsLastFrame = decodeResult.IsLastFrame;
    }

    public override string ToString()
    {
        return $"Gw={Gw}, Src={Src}, Dst={Dst}, FrameCount={FrameCount:D3}, IsLastFrame={IsLastFrame}, Cm={Cm}, Dt={Dt}, Fi={Fi}, Fn={Fn}, Ft={Ft}, {TransceiveMode}";
    }
}