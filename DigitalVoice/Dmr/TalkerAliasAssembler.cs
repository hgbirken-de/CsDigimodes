using System.Text;

namespace DigitalVoice.Dmr;

/// <summary>
/// Accumulates Talker Alias fragments (header + up to 3 blocks) across
/// consecutive superframes of the same RX stream and assembles the final
/// alias text. Per ETSI TS 102 361-2, tables 7.4/7.5.
/// </summary>
public class TalkerAliasAssembler
{
    int? _dataFormat;
    int? _dataLength;
    StringBuilder _bitBuffer = new();

    /// <summary>
    /// Resets any partially-accumulated fragments, discarding them.
    /// </summary>
    public void Reset()
    {
        _dataFormat = null;
        _dataLength = null;
        _bitBuffer = new StringBuilder();
    }

    /// <summary>
    /// Feed one reconstructed TA header/block LC PDU (9 bytes, LcData[0] &amp; 0x3F
    /// already confirmed to be one of Flco.TA_HEADER/TA_BLOCK1/2/3).
    /// </summary>
    /// <returns>
    /// The fully assembled Talker Alias text, once enough fragments have
    /// arrived to satisfy DataLength - otherwise <c>null</c>.
    /// </returns>
    public string? AddFragment(byte[] lcData)
    {
        Flco flco = (Flco)(lcData[0] & 0x3F);

        if (flco == Flco.TA_HEADER)
        {
            Reset();
            int byte2 = lcData[2];
            _dataFormat = (byte2 >> 6) & 0x03;
            _dataLength = (byte2 >> 1) & 0x1F;

            if (_dataFormat == 0) // nur bei 7-Bit-ASCII zaehlt dieses Bit mit
            {
                int firstBit = byte2 & 0x01;
                _bitBuffer.Append(firstBit);
            }
            // bei 8-Bit/16-Bit-Formaten: Bit wird bewusst verworfen (reserviert)
            for (int i = 3; i <= 8; i++)
            {
                _bitBuffer.Append(ByteToBitString(lcData[i]));
            }
        }
        else
        {
            if (_dataFormat == null)
            {
                return null; // block arrived without a preceding header - ignore
            }
            for (int i = 2; i <= 8; i++)
            {
                _bitBuffer.Append(ByteToBitString(lcData[i]));
            }
        }

        return TryDecode();
    }

    string? TryDecode()
    {
        if (_dataFormat == null || _dataLength == null)
            return null;

        int codeUnitBits = _dataFormat switch
        {
            0 => 7,
            1 or 2 => 8,
            _ => 16
        };
        int neededBits = _dataLength.Value * codeUnitBits;

        if (_bitBuffer.Length < neededBits)
            return null; // not enough fragments yet

        string usedBits = _bitBuffer.ToString(0, neededBits);

        string text;
        if (_dataFormat == 0) // 7-bit ASCII
        {
            StringBuilder sb = new();
            for (int i = 0; i < neededBits; i += 7)
            {
                int charCode = Convert.ToInt32(usedBits.Substring(i, 7), 2);
                sb.Append((char)charCode);
            }
            text = sb.ToString();
        }
        else
        {
            byte[] raw = BitsToBytes(usedBits);
            text = _dataFormat switch
            {
                1 => Encoding.Latin1.GetString(raw),
                2 => Encoding.UTF8.GetString(raw),
                _ => Encoding.BigEndianUnicode.GetString(raw)
            };
        }

        Reset();
        return text;
    }

    static string ByteToBitString(byte b) => Convert.ToString(b, 2).PadLeft(8, '0');

    static byte[] BitsToBytes(string bits)
    {
        int byteCount = bits.Length / 8;
        byte[] result = new byte[byteCount];
        for (int i = 0; i < byteCount; i++)
        {
            result[i] = Convert.ToByte(bits.Substring(i * 8, 8), 2);
        }
        return result;
    }
}