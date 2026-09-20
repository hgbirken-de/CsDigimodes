
using DigitalVoice.Common;

namespace DigitalVoice.Nxdn;

public class NxdnSessionContext
{
    public NxdnSessionContext() { }

    public int DstId { get; set; } = 0;

    public int GwId { get; set; } = 0;

    public int SrcId { get; set; } = 0;

    public int NxdnId { get; set; } = 0;

    public bool Eot {  get; set; }

    /// <summary>
    /// Parsed LICH information for the current frame.
    /// </summary>
    public NxdnLich Lich { get; set; }

    public List<byte[]> RxAmbeData { get; } = new List<byte[]>(4);
    
    public int RxFrameNumber { get; set; } = 0;

    public int RxNxdnpCount { get; set; }

    public int StreamId { get; set; } = 0;

    public StreamState StreamState { get; set; } = StreamState.End;

    public TransceiveMode TransceiveMode { get; set; } = TransceiveMode.Rx;

    public byte[] TxAmbeData { get; } = new byte[28]; // 4*7 AMBE blocks
   
    public int TxFrameNumber { get; set; }

    public int TxNxdnpCount { get; set; }

    public override string ToString()
    {
        return $"fn={RxFrameNumber}, tm={TransceiveMode}, gw={GwId}, src={SrcId}, dst={DstId}, eot={Eot}, lich={Convert.ToString(Lich.Raw, 2).PadLeft(8, '0')}";
    }

}