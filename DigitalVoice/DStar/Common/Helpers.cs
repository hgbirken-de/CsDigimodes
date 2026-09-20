using NLog;

namespace DigitalVoice.DStar.Common;

// CRC-16/X-25 (reflected) implementation
public static class Helpers
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static int CalcCcittCrc(byte[] buffer, int startPos, int length)
    {
        int icomcrc = 0xFFFF;

        for (int j = startPos; j < startPos + length; j++)
        {
            int ch = buffer[j] & 0xFF;

            for (int i = 0; i < 8; i++)
            {
                bool xorflag = ((icomcrc ^ ch) & 0x01) == 0x01;
                icomcrc >>= 1; // unsigned shift, same as >>> in Java
                if (xorflag)
                    icomcrc ^= 0x8408;
                ch >>= 1;
            }
        }

        return (~icomcrc) & 0xFFFF;
    }


    /// <summary>
    /// Compute CRC-16/X-25 for the given byte array.
    /// Polynomial 0x1021 (reflected 0x8408), init 0xFFFF, xorout 0xFFFF.
    /// Returns CRC as 16-bit value (0..0xFFFF).
    /// </summary>
    public static ushort ComputeCrc16X25(byte[] data, int offset = 0, int length = -1)
    {
        ArgumentNullException.ThrowIfNull(data);
        
        if (length < 0) 
            length = data.Length - offset;
        
        uint crc = 0xFFFFu;
        for (int i = 0; i < length; i++)
        {
            crc ^= (uint)data[offset + i];
            for (int b = 0; b < 8; b++)
            {
                if ((crc & 0x0001) != 0)
                {
                    crc = (crc >> 1) ^ 0x8408u; // reversed poly 0x1021
                }
                else
                {
                    crc >>= 1;
                }
            }
        }
        crc ^= 0xFFFFu;
        return (ushort)(crc & 0xFFFFu);
    }


    public static ushort ComputeCrc16X25_old(byte[] data)
    {
        int num = 65535;
        for (int i = 0; i < data.Length; i++)
        {
            int num2 = (int)(data[i] & byte.MaxValue);
            for (int j = 0; j < 8; j++)
            {
                bool flag = ((num ^ num2) & 1) == 1;
                num = (int)((uint)num >> 1);
                if (flag)
                {
                    num ^= 33800;
                }
                num2 = (int)((uint)num2 >> 1);
            }
        }
        return (ushort)(~num & 65535);

    }


    /// <summary>
    /// Try to parse and validate a DCS GPS slow-data string of the form
    ///    $$CRChhhh,<payload...>
    /// where hhhh is the 4 hex digits of the CRC (big-endian representation in the message).
    /// This method extracts the CRC from the message and computes CRC-16/X25 over the chosen payload
    /// (by default the bytes after the comma) and returns (isValid, parsedCrc, computedCrc).
    /// </summary>
    public static (bool IsValid, ushort ParsedCrc, ushort ComputedCrc) ValidateCrcInGpsMessage(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        // locate "CRC" token
        int idx = message.IndexOf("CRC", StringComparison.OrdinalIgnoreCase);

        if (idx < 0 || idx + 3 + 4 > message.Length)
        {
            logger.Error($"Message does not contain 'CRC' followed by 4 hex digits: {message}");
            return default;
        }

        string hex = message.Substring(idx + 3, 4); // the 4 hex digits e.g. "95EC"
        if (!ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out ushort parsedCrc))
        {
            logger.Error($"Unable to parse CRC hex: {message}");
            return default;
        }

        // decide which bytes to compute the CRC over:
        // common choice: payload after the comma that follows the CRC field
        int comma = message.IndexOf(',', 9); // look for comma after "$$CRChhhh"
        string payload;
        if (comma >= 0)
            payload = message.Substring(comma + 1); // everything after comma
        else
            // fallback: everything after the CRC field
            payload = message.Substring(idx + 7);

        // compute CRC over payload's ASCII bytes
        byte[] payloadBytes = System.Text.Encoding.ASCII.GetBytes(payload);
        ushort computed = ComputeCrc16X25(payloadBytes);

        return (computed == parsedCrc, parsedCrc, computed);
    }
}

