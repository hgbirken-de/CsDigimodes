using System.ComponentModel;

namespace FusionCodec;

/// <summary>
/// Fusion Call Modes.
/// </summary>
public enum CallMode
{
    [Description("00:Group/CQ mode")]
    GroupMode = 0,

    [Description("Radio ID mode")]
    RadioMode = 1,

    [Description("Individual mode")]
    IndividualMode = 3,
}