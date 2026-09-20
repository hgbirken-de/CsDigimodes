
namespace DigitalVoice.Nxdn;

/// <summary>
/// NXDN LICH (Link Information Channel Header) byte structure.
///
/// Bits:
/// 7-6 : RFCT (Radio Frequency Channel Type)
/// 5-4 : USC (Signalling / SACCH type)
/// 3-2 : Steal (FACCH or not)
/// 1-0 : Direction (Inbound/Outbound)
/// </summary>
public readonly struct NxdnLich
{
    public byte Raw { get; }
    public NxdnRfct Rfct { get; }
    public NxdnUsc Usc { get; }
    public NxdnSteal Steal { get; }
    public NxdnDirection Direction { get; }

    private NxdnLich(byte raw)
    {
        Raw = raw;

        Rfct = (NxdnRfct)((raw >> 6) & 0b11);
        Usc = (NxdnUsc)((raw >> 4) & 0b11);
        Steal = (NxdnSteal)((raw >> 2) & 0b11);
        Direction = (NxdnDirection)(raw & 0b11);
    }

    public static NxdnLich FromFields(NxdnRfct rfct, NxdnUsc usc, NxdnSteal steal, NxdnDirection direction)
    {
        byte raw = (byte)(((byte)rfct << 6) | ((byte)usc << 4) | ((byte)steal << 2) | (byte)direction);
        return new NxdnLich(raw);
    }

    public static NxdnLich Parse(byte lich) => new(lich);

    public override string ToString() => $"RFCT={Rfct}, USC={Usc}, Steal={Steal}, Dir={Direction}, Raw=0x{Raw:X2}";
}

public enum NxdnRfct : byte
{
    Reserved = 0,
    Rcch = 1,
    Rdch = 2,
    Reserved3 = 3
}

public enum NxdnUsc : byte
{
    SacchNs = 0,   // No Steal
    Reserved1 = 1,
    SacchSs = 2,   // Steal
    Reserved3 = 3,
}

public enum NxdnSteal : byte
{
    Facch = 0,
    Reserved1 = 1,
    Reserved2 = 2,
    None = 3
}

public enum NxdnDirection : byte
{
    Inbound = 0,
    Outbound = 1,
    Reserved2 = 2,
    Reserved3 = 3
}
