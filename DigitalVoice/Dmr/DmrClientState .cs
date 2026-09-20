
using DigitalVoice.Common;

namespace DigitalVoice.Dmr;

/// <summary>
/// Represents DMR client session state.
/// 
/// This record holds both RX (receive) and TX (transmit) properties used in 
/// Digital Mobile Radio (DMR) operation. It tracks identifiers, slot assignments, 
/// frame sequencing, and protocol-level state such as stream handling and 
/// connection status.
/// 
/// Key responsibilities:
/// - RX properties: store source/destination IDs, repeater ID, slot number, 
///   frame type, and FLCO information for the current inbound frame.
///   Session state: includes stream ID, salt value (for security/auth), 
///   voice/data sequencing, connection status, and transceive mode.
/// - TX properties: maintain transmit IDs, slot, frame count, and frame type 
///   for outbound communication.
/// 
/// This state object is typically updated continuously as frames are exchanged 
/// with a DMR network or repeater.
/// </summary>
public record DmrSessionContext
{
    public DmrProtocol Protocol { get; set; } = DmrProtocol.MmdvmHost;
    public Status Status { get; set; } = Status.Disconnected; // TODO: move to TransceiveMode                                                          
    public TransceiveMode TransceiveMode { get; set; } = TransceiveMode.Rx;

    #region "RX Properties"
    public int RxColorCode { get; set; } = 0;
    public Flco RxFlco { get; set; }
    public int RxFrameNo { get; set; }
    public FrameType RxFrameType { get; set; }
    public int RxPongCount { get; set; } = 0;
    public int RxDstId { get; set; }
    public int RxRptId { get; set; }
    public int RxSrcId { get; set; }
    public uint RxStreamId { get; set; } = 0;
    public StreamState RxStreamState { get; set; } = StreamState.End;
    public int RxTimeSlot { get; set; }
    public int RxVoiceOrDataSeq { get; set; }
    public byte[] Salt { get; set; } = [];

    #endregion

    #region "TX Properties"
    public int TxColorCode { get; set; } = 1;
    public Flco TxFlco { get; set; }
    public int TxFrameCount { get; set; } = 0;
    public int TxPingCount { get; set; }  = 0;
    public FrameType TxFrameType { get; set; }
    public int TxDstId { get; set; }
    public int TxRptId { get; set; }
    public int TxSrcId { get; set; }
    public uint TxStreamId { get; set; }
    public int TxTimeSlot { get; set; } = 1;

    #endregion

}