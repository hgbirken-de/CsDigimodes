
namespace FusionCodec;

public class DecodeResult
{
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

    public List<byte[]> AmbeData { get; } = new List<byte[]>(5);

    public override string ToString()
    {
        return $"Gw={Gw}, Src={Src}, Dst={Dst}, FrameCount={FrameCount:D3}, IsLastFrame={IsLastFrame}, Cm={Cm}, Dt={Dt}, Fi={Fi}, Fn={Fn}, Ft={Ft}";
    }
}
