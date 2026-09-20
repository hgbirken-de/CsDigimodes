using DigitalVoice.Common;

namespace DigitalVoice.DStar.Xrf;

public record XrfClientState
{
    public bool IsRunning { get; set; } = false;

    public TransceiveMode TransceiveMode { get; set; } = TransceiveMode.Rx;

    #region "RX Properties"

    public int FrameNo { get; set; } = 0;
    public string? RxNetMsg { get; set; }

    public int RxPingCount { get; set; } = 0;

    public ushort RxStreamId { get; set; } = 0;

    public StreamState RxStreamState { get; set; } = StreamState.End;

    public string Gw { get; set; } = string.Empty;

    public string Gw2 { get; set; } = string.Empty;

    public string Dst { get; set; } = string.Empty;

    public string Src { get; set; } = string.Empty;

    public bool SdSync { get; set; } = false;

    public int SdSeq { get; set; } = 0;

    public string RxUsrMsg {  get; set; } = string.Empty;

    #endregion

    #region "TX Properties"

    public int TxFrameCnt { get; set; } = 0;

    public string TxMyCall { get; set; } = string.Empty;

    public string TxUrCall { get; set; } = string.Empty;

    public string TxRptr1 { get; set; } = string.Empty;

    public string TxRptr2 { get; set; } = string.Empty;

    public uint TxStreamId { get; set; } = 0;

    public string TxUsrMsg { get; set; } = "".PadRight(20, ' ');

    #endregion
}
