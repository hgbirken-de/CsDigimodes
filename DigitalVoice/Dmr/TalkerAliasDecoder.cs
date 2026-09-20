using NLog;
using System.Text;

namespace DigitalVoice.Dmr;

public class TalkerAliasState
{
    public int Format { get; set; }
    public int Size { get; set; } // number of characters
    public List<byte> DataBytes { get; } = new();
    public int BytesCollected => DataBytes.Count;
}

public class TalkerAliasDecoder
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<int, TalkerAliasState> _states = [];

    public void ProcessPayload(int srcId, byte[] payload, int offset = 20)
    {
        //if (payload.Length < offset + 2) return;

        int flcoValue = payload[offset] & 0x1F;
        if (!Enum.IsDefined(typeof(Flco), flcoValue))
        {
            logger.Debug($"unexpected flcoValue = {flcoValue}");
            return;
        }

        Flco flco = (Flco)flcoValue;

        switch (flco)
        {
            case Flco.TA_HEADER:
                {
                    int taFormat = (payload[offset + 1] >> 6) & 0x03;
                    int taSize = (payload[offset + 1] >> 1) & 0x1F;
                    
                    logger.Debug($"[{flco}] format={taFormat}, size={taSize}, srcId={srcId}");

                    var state = new TalkerAliasState { Format = taFormat, Size = taSize };
                    _states[srcId] = state;

                    // Header data: first 49 bits
                    byte[] headerData = new byte[7];
                    Array.Copy(payload, offset + 2, headerData, 0, 7);

                    // Mask reserved bits for non-7-bit formats
                    if (taFormat != 0)
                        headerData[0] &= 0x7F; // first bit reserved

                    state.DataBytes.AddRange(headerData);

                    // Check if header alone is enough to complete alias
                    if (IsAliasComplete(state))
                    {
                        string alias = DecodeAlias(state);
                        logger.Debug($"[TA_COMPLETE] srcId={srcId}, alias=\"{alias}\"");
                        _states.Remove(srcId);
                    }
                }
                break;

            case Flco.TA_BLOCK1:
            case Flco.TA_BLOCK2:
            case Flco.TA_BLOCK3:
                {
                    logger.Debug($"[{flco}] srcId = {srcId}");
                    if (_states.TryGetValue(srcId, out var state))
                    {
                        byte[] blockData = new byte[7];
                        Array.Copy(payload, offset + 2, blockData, 0, 7);
                        state.DataBytes.AddRange(blockData);

                        // check if we have enough bytes to decode all characters
                        if (IsAliasComplete(state))
                        {
                            string alias = DecodeAlias(state);
                            logger.Debug($"[TA_COMPLETE] srcId={srcId}, alias=\"{alias}\"");
                            _states.Remove(srcId);
                        }
                    }
                }
                break;
        }
    }

    private static bool IsAliasComplete(TalkerAliasState state)
    {
        switch (state.Format)
        {
            case 0: // 7-bit
                return state.DataBytes.Count * 8 / 7 >= state.Size;
            case 1: // ISO-8859-1
            case 2: // UTF-8
                return state.DataBytes.Count >= state.Size;
            case 3: // UTF-16BE
                return state.DataBytes.Count >= state.Size * 2;
            default:
                return true;
        }
    }

    private static string DecodeAlias(TalkerAliasState state)
    {
        byte[] raw = state.DataBytes.ToArray();

        return state.Format switch
        {
            0 => Decode7Bit(raw, state.Size),
            1 => Encoding.GetEncoding("ISO-8859-1").GetString(raw, 0, Math.Min(raw.Length, state.Size)),
            2 => Encoding.UTF8.GetString(raw, 0, Math.Min(raw.Length, state.Size)), // collect full UTF-8 bytes
            3 => Encoding.BigEndianUnicode.GetString(raw, 0, Math.Min(raw.Length, state.Size * 2)),
            _ => BitConverter.ToString(raw)
        };
    }

    private static string Decode7Bit(byte[] raw, int size)
    {
        var chars = new List<char>();
        int acc = 0, bits = 0;

        foreach (byte b in raw)
        {
            acc |= (b << bits);
            bits += 8;

            while (bits >= 7 && chars.Count < size)
            {
                chars.Add((char)(acc & 0x7F));
                acc >>= 7;
                bits -= 7;
            }
        }

        return new string(chars.ToArray());
    }
}
