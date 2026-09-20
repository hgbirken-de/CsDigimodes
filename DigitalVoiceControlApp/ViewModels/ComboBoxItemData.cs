using DigitalVoice.Common;
using DigitalVoice.Dmr;
using System;

namespace DigitalVoiceControlApp.ViewModels;

public class ComboBoxItemData : IEquatable<ComboBoxItemData>
{
    public string? DisplayName { get; set; }
    public Mode Mode { get; set; }
    public string DcsReflectorId { get; set; } = string.Empty;
    public int NxdnReflectorId { get; set; } = 0;
    public string RefReflectorId { get; set; } = string.Empty;
    public string XrfReflectorId { get; set; } = string.Empty;
    public int DmrId { get; set; }
    public Flco Flco { get; set; }
    public string? FcsReflectorId { get; set; }
    public string? YsfDesignator { get; set; }

    public override string ToString() => DisplayName ?? string.Empty;

    public override bool Equals(object? obj) => Equals(obj as ComboBoxItemData);

    public bool Equals(ComboBoxItemData? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Mode != other.Mode)
            return false;

        return Mode switch
        {
            Mode.Dcs => string.Equals(DcsReflectorId, other.DcsReflectorId, StringComparison.OrdinalIgnoreCase),
            Mode.Dmr => DmrId == other.DmrId,
            Mode.Fcs => string.Equals(FcsReflectorId, other.FcsReflectorId, StringComparison.OrdinalIgnoreCase),
            Mode.Nxdn => NxdnReflectorId == other.NxdnReflectorId,
            Mode.Ref => string.Equals(RefReflectorId, other.RefReflectorId, StringComparison.OrdinalIgnoreCase),
            Mode.Xrf => string.Equals(XrfReflectorId, other.XrfReflectorId, StringComparison.OrdinalIgnoreCase),
            Mode.Ysf => string.Equals(YsfDesignator, other.YsfDesignator, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    public override int GetHashCode()
    {
        return Mode switch
        {
            Mode.Dmr => HashCode.Combine(Mode, DmrId),
            Mode.Dcs => HashCode.Combine(Mode, DcsReflectorId?.ToLowerInvariant()),
            Mode.Fcs => HashCode.Combine(Mode, FcsReflectorId?.ToLowerInvariant()),
            Mode.Nxdn => HashCode.Combine(Mode, NxdnReflectorId),
            Mode.Ref => HashCode.Combine(Mode, RefReflectorId?.ToLowerInvariant()),
            Mode.Xrf => HashCode.Combine(Mode, XrfReflectorId?.ToLowerInvariant()),
            Mode.Ysf => HashCode.Combine(Mode, YsfDesignator?.ToLowerInvariant()),
            _ => Mode.GetHashCode()
        };
    }
}
