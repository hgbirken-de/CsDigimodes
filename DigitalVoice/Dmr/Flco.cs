namespace DigitalVoice.Dmr;

/// <summary>
/// Full Link Control Opcode values. Source: ETSI TS 102 361-2 V2.5.1 (2023-05).
/// </summary>
public enum Flco
{
    GROUP = 0,
    USER_USER = 3,
    TA_HEADER = 4,
    TA_BLOCK1 = 5,
    TA_BLOCK2 = 6,
    TA_BLOCK3 = 7,
    GPS_INFO = 8
}