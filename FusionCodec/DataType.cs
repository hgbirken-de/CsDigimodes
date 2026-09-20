using System.ComponentModel;

namespace FusionCodec;

/// <summary>
/// YSF Data types.
/// </summary>
public enum DataType
{
    [Description("V/D mode type 1 (simultaneous voice data communication mode 1)")]
    VD_MODE_1 = 0,

    [Description("Data FR mode (high speed data transmission mode)")]
    DATA_FR = 1,

    [Description("V/D mode type 2 (simultaneous voice/data communication mode 2)")]
    VD_MODE_2 = 2,

    [Description("Voice FR mode (high quality voice full rate mode)")]
    VOICE_FR = 3
}