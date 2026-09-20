namespace DigitalVoice.Dmr;

/// <summary>
/// Data Type information.
/// See: ETSI TS 102 361-1 V2.6.1 (2023-05), 9.3.6 Data Type
/// </summary>
public enum DataType
{
  VoicePiHeader = 0x00,
  VoiceLcHeader = 0x01,
  TerminatorWithLc = 0x02,
  Csbk = 0x03,
  DataHeader = 0x06,
  Rate12Data = 0x07,
  Rate34Data = 0x08,
  Idle = 0x09,
  Rate1Data = 0x0A,
  Mask = 0x0F,
}