namespace DigitalVoice.Dmr;

internal class TalkerAliasBuffer
{
    public int ExpectedLength { get; set; }
    public TalkerAliasEncoding Encoding { get; set; } = TalkerAliasEncoding.SevenBitAscii;
    public List<byte> Bytes { get; } = [];
}
