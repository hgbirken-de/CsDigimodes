using DigitalVoice.Common;
using System.Text;

namespace DigitalVoice.DStar.Common;

public record DStarSessionContext
{
    public string? Reflector {  get; set; } // ref name + module
    public bool IsRunning { get; set; } = false;
    public TransceiveMode TransceiveMode { get; set; } = TransceiveMode.Rx;

    #region "RX Properties"

    public int FrameNo { get; set; } = 0;
    public string? RxNetMsg { get; set; }

    public int RxPingCnt { get; set; } = 0;

    public ushort RxStreamId { get; set; } = 0;

    public StreamState RxStreamState { get; set; } = StreamState.End;

    public string RxRptr1 { get; set; } = string.Empty;

    public string RxRptr2 { get; set; } = string.Empty;

    public string RxUrCall { get; set; } = string.Empty;

    public string RxSrc { get; set; } = string.Empty;

    public int SdGpsCnt { get; set; } = 0;
    public int SdHdrCnt { get; set; } = 0;
    public int SdSeq { get; set; } = 0;
    public bool SdSync { get; set; } = false;

    public string RxGpsData { get; set; } = string.Empty;
    public StringBuilder RxGpsDataBuffer { get; } = new();

    public string RxUsrMsg {  get; set; } = string.Empty;
    public byte[] RxUserMsgBuffer { get; set; } = new byte[20];

    #endregion

    #region "TX Properties"

    public int TxFrameCnt { get; set; } = 0;

    public string TxMyCall { get; set; } = string.Empty;

    public string TxUrCall { get; set; } = string.Empty;

    public string TxRptr1 { get; set; } = string.Empty;

    public string TxRptr2 { get; set; } = string.Empty;

    public int TxPingCnt { get; set; } = 0;

    public uint TxStreamId { get; set; } = 0;

    public string TxUsrMsg { get; set; } = "".PadRight(20, ' ');

    #endregion
}
