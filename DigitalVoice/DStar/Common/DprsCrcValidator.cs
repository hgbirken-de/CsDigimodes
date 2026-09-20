using System.Text;

namespace DigitalVoice.DStar.Common;


public static class DprsCrcValidator
{
    public static bool Validate(string dprsPacket)
    {
        // Must start with $$CRC
        if (!dprsPacket.StartsWith("$$CRC", StringComparison.OrdinalIgnoreCase))
            return false;

        // Extract CRC part (4 hex digits after $$CRC)
        string crcText = dprsPacket.Substring(5, 4);
        if (!ushort.TryParse(crcText, System.Globalization.NumberStyles.HexNumber,
                             null, out ushort expectedCrc))
            return false;

        // Extract payload after the comma
        int commaIndex = dprsPacket.IndexOf(',');
        if (commaIndex < 0 || commaIndex + 1 >= dprsPacket.Length)
            return false;

        string payload = dprsPacket.Substring(commaIndex + 1);

        // Compute CRC of payload
        ushort computedCrc = ComputeCrc16X25(Encoding.ASCII.GetBytes(payload));

        return expectedCrc == computedCrc;
    }

    private static ushort ComputeCrc16X25(byte[] data)
    {
        const ushort poly = 0x1021;
        ushort crc = 0xFFFF;

        foreach (byte b in data)
        {
            byte curByte = ReverseBits(b); // input reflected
            for (int i = 0; i < 8; i++)
            {
                bool bit = ((curByte >> (7 - i)) & 1) == 1;
                bool c15 = ((crc >> 15) & 1) == 1;
                crc <<= 1;
                if (c15 ^ bit)
                    crc ^= poly;
            }
        }

        crc = ReverseBits(crc);  // output reflected
        return (ushort)(crc ^ 0xFFFF);
    }

    private static byte ReverseBits(byte b)
    {
        b = (byte)((b >> 4) | (b << 4));
        b = (byte)(((b & 0xCC) >> 2) | ((b & 0x33) << 2));
        b = (byte)(((b & 0xAA) >> 1) | ((b & 0x55) << 1));
        return b;
    }

    private static ushort ReverseBits(ushort x)
    {
        ushort y = 0;
        for (int i = 0; i < 16; i++)
        {
            y <<= 1;
            y |= (ushort)(x & 1);
            x >>= 1;
        }
        return y;
    }
}