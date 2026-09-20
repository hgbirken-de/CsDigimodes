using DigitalVoice.Common;
using NLog;

namespace DigitalVoice.Dmr;

/// <summary>
/// Implements encoding and decoding of DMR BPTC (Binary Phase-Shift Keying Transport Codec) frames,
/// including bit-level interleaving, Hamming error correction, and payload extraction.
/// <para>
/// The codec handles conversion between raw bytes and interleaved bit arrays, applies Hamming codes
/// to rows and columns for error correction, and supports both encoding and decoding operations
/// according to the BPTC19696 specification.
/// </para>
/// <para>
/// This class relies on the <see cref="Hamming"/> utility class for Hamming (15,11,3), (13,9,3),
/// (16,11,4), and (17,12,3) encoding and decoding routines.
/// </para>
/// </summary>
public static class DmrBptcCodec
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly bool [] m_rawData = new bool[196];
    static readonly bool [] m_deInterData = new bool[196];


    /// <summary>
    /// Decodes a BPTC 19696 block.
    /// This method performs the full decode process:
    /// 1. Extracts the raw binary from the input.
    /// 2. Deinterleaves the data.
    /// 3. Performs error checking (Hamming/FEC).
    /// 4. Extracts the decoded output data.
    /// </summary>
    /// <param name="input">Input byte array (encoded BPTC block).</param>
    /// <param name="output">Output byte array to receive decoded data.</param>
    public static void Decode(byte[] input, byte[] output)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));
        ArgumentNullException.ThrowIfNull(output, nameof(output));

        // Get the raw binary
        DecodeExtractBinary(input);

        // Deinterleave
        DecodeDeInterleave();

        // Error check
        DecodeErrorCheck();

        // Extract data
        DecodeExtractData(output);
    }

    /// <summary>
    /// Encodes a BPTC 196,96 block.
    /// This method performs the full encode process:
    /// 1. Extracts the input data to a working format.
    /// 2. Performs error checking (Hamming/FEC).
    /// 3. Interleaves the data.
    /// 4. Produces the raw binary output.
    /// </summary>
    /// <param name="input">
    /// Input byte array (must be exactly 12 bytes = 96 bits of information).
    /// </param>
    /// <param name="output">
    /// Output byte array (must be at least 33 bytes = ?? bits, of which the first 196 are valid).
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="input"/> is not 12 bytes long,
    /// or <paramref name="output"/> is not 25 bytes long.
    /// </exception>
    public static void Encode(byte[] input, byte[] output, int offset)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));
        if (input.Length != 12)
            throw new ArgumentException("Input must be exactly 12 bytes (96 bits).", nameof(input));

        // Extract data
        EncodeExtractData(input);

        // Error check
        EncodeErrorCheck();

        // Interleave
        EncodeInterleave();

        // Get the raw binary
        EncodeExtractBinary(output, offset);
    }

    /// <summary>
    /// Converts 8 bits (big-endian) to a byte.
    /// </summary>
    /// <param name="bits">The bits to convert.</param>
    /// <returns>A byte with the corresponding bits set.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte BitsToByteBE(Span<bool> bits)
    {
        if (bits.Length < 8)
            throw new ArgumentException("Span must be at least 8 bits.", nameof(bits));

        byte value = 0x00;
        for (int i = 0; i < 8; i++)
        {
            if (bits[i])
                value |= (byte)(0x80 >> i);
        }
        return value;
    }

    /// <summary>
    /// Converts an array of 8 boolean bits in big-endian order (MSB first) into a byte.
    /// </summary>
    /// <param name="bits">An array of 8 booleans representing the bits.</param>
    /// <returns>The resulting byte.</returns>
    public static byte BitsToByteBE(bool[] bits)
    {
        ArgumentNullException.ThrowIfNull(bits, nameof(bits));
        if (bits.Length < 8) throw new ArgumentException("bits array must have at least 8 elements", nameof(bits));

        byte result = 0;
        result |= bits[0] ? (byte)0x80 : (byte)0x00;
        result |= bits[1] ? (byte)0x40 : (byte)0x00;
        result |= bits[2] ? (byte)0x20 : (byte)0x00;
        result |= bits[3] ? (byte)0x10 : (byte)0x00;
        result |= bits[4] ? (byte)0x08 : (byte)0x00;
        result |= bits[5] ? (byte)0x04 : (byte)0x00;
        result |= bits[6] ? (byte)0x02 : (byte)0x00;
        result |= bits[7] ? (byte)0x01 : (byte)0x00;

        return result;
    }

    /// <summary>
    /// Converts a byte to an array of 8 boolean bits in big-endian order (MSB first).
    /// </summary>
    /// <param name="b">The byte to convert.</param>
    /// <param name="bits">An array of 8 booleans to receive the bit values.</param>
    public static void ByteToBitsBE(byte b, bool[] bits)
    {
        ArgumentNullException.ThrowIfNull(bits, nameof(bits));
        if (bits.Length < 8) throw new ArgumentException("bits array must have at least 8 elements", nameof(bits));

        bits[0] = (b & 0x80) != 0;
        bits[1] = (b & 0x40) != 0;
        bits[2] = (b & 0x20) != 0;
        bits[3] = (b & 0x10) != 0;
        bits[4] = (b & 0x08) != 0;
        bits[5] = (b & 0x04) != 0;
        bits[6] = (b & 0x02) != 0;
        bits[7] = (b & 0x01) != 0;
    }

    /// <summary>
    /// Converts a byte to 8 bits in big-endian order and stores them in the destination array.
    /// </summary>
    /// <param name="b">Input byte.</param>
    /// <param name="dest">Destination bool array. Must be at least 8 elements long or have enough room from given offset.</param>
    /// <param name="offset">Optional offset in the destination array. Default 0.</param>
    private static void ByteToBitsBE(byte b, bool[] dest, int offset = 0)
    {
        ArgumentNullException.ThrowIfNull(dest, nameof(dest));
        if (dest.Length < offset + 8) throw new ArgumentException("Destination array is too small for the given offset.", nameof(dest));

        dest[offset + 0] = (b & 0x80) != 0;
        dest[offset + 1] = (b & 0x40) != 0;
        dest[offset + 2] = (b & 0x20) != 0;
        dest[offset + 3] = (b & 0x10) != 0;
        dest[offset + 4] = (b & 0x08) != 0;
        dest[offset + 5] = (b & 0x04) != 0;
        dest[offset + 6] = (b & 0x02) != 0;
        dest[offset + 7] = (b & 0x01) != 0;
    }

    /// <summary>
    /// Extracts the raw binary bits from the input byte array into the internal m_rawData array.
    /// The first block uses bytes 0–12, plus 2 bits from byte 20, and the second block uses bytes 21–32.
    /// </summary>
    /// <param name="input">Input byte array containing the encoded data.</param>
    public static void DecodeExtractBinary(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Length < 33) throw new ArgumentException("Input array must have at least 33 bytes.", nameof(input));

        // First block
        ByteToBitsBE(input[0], m_rawData, 0);
        ByteToBitsBE(input[1], m_rawData, 8);
        ByteToBitsBE(input[2], m_rawData, 16);
        ByteToBitsBE(input[3], m_rawData, 24);
        ByteToBitsBE(input[4], m_rawData, 32);
        ByteToBitsBE(input[5], m_rawData, 40);
        ByteToBitsBE(input[6], m_rawData, 48);
        ByteToBitsBE(input[7], m_rawData, 56);
        ByteToBitsBE(input[8], m_rawData, 64);
        ByteToBitsBE(input[9], m_rawData, 72);
        ByteToBitsBE(input[10], m_rawData, 80);
        ByteToBitsBE(input[11], m_rawData, 88);
        ByteToBitsBE(input[12], m_rawData, 96);

        // Handle the two bits from byte 20
        bool[] bits = new bool[8];
        ByteToBitsBE(input[20], bits);
        m_rawData[98] = bits[6];
        m_rawData[99] = bits[7];

        // Second block
        ByteToBitsBE(input[21], m_rawData, 100);
        ByteToBitsBE(input[22], m_rawData, 108);
        ByteToBitsBE(input[23], m_rawData, 116);
        ByteToBitsBE(input[24], m_rawData, 124);
        ByteToBitsBE(input[25], m_rawData, 132);
        ByteToBitsBE(input[26], m_rawData, 140);
        ByteToBitsBE(input[27], m_rawData, 148);
        ByteToBitsBE(input[28], m_rawData, 156);
        ByteToBitsBE(input[29], m_rawData, 164);
        ByteToBitsBE(input[30], m_rawData, 172);
        ByteToBitsBE(input[31], m_rawData, 180);
        ByteToBitsBE(input[32], m_rawData, 188);
    }

    /// <summary>
    /// Deinterleaves the raw data stored in <c>m_rawData</c> into <c>m_deInterData</c>.
    /// Implements the specific interleave sequence used by the CBPTC19696 algorithm.
    /// </summary>
    public static void DecodeDeInterleave()
    {
        // Initialize deinterleaved data
        for (int i = 0; i < 196; i++)
            m_deInterData[i] = false;

        // The first bit is R(3) which is not used, so can be ignored
        for (int a = 0; a < 196; a++)
        {
            // Calculate the interleave sequence
            int interleaveSequence = (a * 181) % 196;
            // Shuffle the data
            m_deInterData[a] = m_rawData[interleaveSequence];
        }
    }

    /// <summary>
    /// Performs error checking and correction on the deinterleaved data using
    /// Hamming codes: (15,11,3) for rows and (13,9,3) for columns.
    /// This process iterates until no further corrections are needed or up to 5 passes.
    /// </summary>
    public static void DecodeErrorCheck()
    {
        bool fixing;
        int count = 0;

        do
        {
            fixing = false;

            // Check each of the 15 columns
            bool[] col = new bool[13];
            for (int c = 0; c < 15; c++)
            {
                int pos = c + 1; // Skip first bit
                for (int a = 0; a < 13; a++)
                {
                    col[a] = m_deInterData[pos];
                    pos += 15;
                }

                if (Hamming.Decode1393(col))
                {
                    pos = c + 1;
                    for (int a = 0; a < 13; a++)
                    {
                        m_deInterData[pos] = col[a];
                        pos += 15;
                    }
                    fixing = true;
                }
            }

            // Check each of the 9 rows containing data
            for (int r = 0; r < 9; r++)
            {
                int pos = (r * 15) + 1; // Skip first bit
                if (Hamming.Decode15113_2(m_deInterData.AsSpan(pos, 15)))
                    fixing = true;
            }

            count++;
        } while (fixing && count < 5);
    }

    /// <summary>
    /// Extracts the 96 payload bits from the deinterleaved data into 12 bytes.
    /// </summary>
    /// <param name="data">Output array of at least 12 bytes.</param>
    public static void DecodeExtractData(byte[] data)
    {
        if (data == null || data.Length < 12)
            throw new ArgumentException("Output array must be at least 12 bytes.", nameof(data));

        bool[] bData = new bool[96];
        int pos = 0;

        // Copy the bit ranges from deinterleaved data
        foreach (var (start, end) in new (int start, int end)[] { (4, 11), (16, 26), (31, 41), (46, 56), (61, 71), (76, 86), (91, 101), (106, 116), (121, 131) })
        {
            for (int i = start; i <= end; i++, pos++)
            {
                bData[pos] = m_deInterData[i];
            }
        }

        // Convert each 8 bits to a byte
        for (int i = 0; i < 12; i++)
        {
            data[i] = BitsToByteBE(bData.AsSpan(i * 8, 8));
        }
    }

    /// <summary>
    /// Encodes 12 bytes of input into the deinterleaved bit array (m_deInterData) using the CBPTC19696 scheme.
    /// </summary>
    /// <param name="input">Input byte array of at least 12 bytes.</param>
    public static void EncodeExtractData(byte[] input)
    {
        if (input == null || input.Length < 12)
            throw new ArgumentException("Input array must be at least 12 bytes.", nameof(input));

        // Convert each byte to 8 bits in big-endian order
        bool[] bData = new bool[96];
        for (int i = 0; i < 12; i++)
        {
            ByteToBitsBE(input[i], bData.AsSpan(i * 8, 8));
        }

        // Clear deInterData
        Array.Clear(m_deInterData, 0, m_deInterData.Length);

        int pos = 0;

        // Copy bits into m_deInterData using the same ranges as in C++ code
        foreach (var (start, end) in new (int start, int end)[] { (4, 11), (16, 26), (31, 41), (46, 56), (61, 71), (76, 86), (91, 101), (106, 116), (121, 131) })
        {
            for (int a = start; a <= end; a++, pos++)
            {
                m_deInterData[a] = bData[pos];
            }
        }
    }

    /// <summary>
    /// Converts a byte into 8 bits in big-endian order (MSB first) and writes them to the provided span.
    /// </summary>
    /// <param name="value">The byte to convert.</param>
    /// <param name="bits">Span of length 8 to hold the resulting bits.</param>
    public static void ByteToBitsBE(byte value, Span<bool> bits)
    {
        if (bits.Length < 8)
            throw new ArgumentException("Span must have length of at least 8.", nameof(bits));

        bits[0] = (value & 0x80) != 0;
        bits[1] = (value & 0x40) != 0;
        bits[2] = (value & 0x20) != 0;
        bits[3] = (value & 0x10) != 0;
        bits[4] = (value & 0x08) != 0;
        bits[5] = (value & 0x04) != 0;
        bits[6] = (value & 0x02) != 0;
        bits[7] = (value & 0x01) != 0;
    }

    /// <summary>
    /// Performs Hamming encoding on each row (15,11,3) and column (13,9,3) 
    /// of the de-interleaved data array.
    /// </summary>
    public static void EncodeErrorCheck()
    {
        bool[] row = new bool[15];
        // Encode each of the 9 rows containing data
        for (int r = 0; r < 9; r++)
        {
            int pos = (r * 15) + 1;
            for (int i = 0; i < row.Length; i++)
                row[i] = m_deInterData[pos + i];

            Hamming.Encode15113_2(row);

            // Copy back encoded row
            pos = (r * 15) + 1;
            for (int i = 0; i < 15; i++)
            {
                m_deInterData[pos + i] = row[i];
            }
        }

        // Encode each of the 15 columns
        bool[] col = new bool[13];
        for (int c = 0; c < 15; c++)
        {
            int pos = c + 1;
            for (int a = 0; a < 13; a++)
            {
                col[a] = m_deInterData[pos];
                pos += 15;
            }

            Hamming.Encode1393(col);

            pos = c + 1;
            for (int a = 0; a < 13; a++)
            {
                m_deInterData[pos] = col[a];
                pos += 15;
            }
        }
    }

    /// <summary>
    /// Interleave the raw data from m_deInterData into m_rawData using the (196,?) interleave pattern.
    /// </summary>
    public static void EncodeInterleave()
    {
        // Clear the raw data first
        //for (int i = 0; i < 196; i++)
        //    m_rawData[i] = false;

        // The first bit is R(3) which is not used, can be ignored
        for (int i = 0; i < 196; i++)
        {
            // Calculate the interleave sequence
            int interleaveSequence = (i * 181) % 196;
            // Unshuffle the data
            m_rawData[interleaveSequence] = m_deInterData[i];
        }
    }

    /// <summary>
    /// Extracts the raw data bits and writes them into the output byte array.
    /// </summary>
    /// <param name="data">Output array of at least 33 bytes.</param>
    /// <param name="offset">Offset of the payload in data array</param>
    public static void EncodeExtractBinary(byte[] data, int offset)
    {
        ArgumentNullException.ThrowIfNull(data);

        // First block
        data[offset + 0] = BitsToByteBE(m_rawData.AsSpan(0, 8));
        data[offset + 1] = BitsToByteBE(m_rawData.AsSpan(8, 8));
        data[offset + 2] = BitsToByteBE(m_rawData.AsSpan(16, 8));
        data[offset + 3] = BitsToByteBE(m_rawData.AsSpan(24, 8));
        data[offset + 4] = BitsToByteBE(m_rawData.AsSpan(32, 8));
        data[offset + 5] = BitsToByteBE(m_rawData.AsSpan(40, 8));
        data[offset + 6] = BitsToByteBE(m_rawData.AsSpan(48, 8));
        data[offset + 7] = BitsToByteBE(m_rawData.AsSpan(56, 8));
        data[offset + 8] = BitsToByteBE(m_rawData.AsSpan(64, 8));
        data[offset + 9] = BitsToByteBE(m_rawData.AsSpan(72, 8));
        data[offset + 10] = BitsToByteBE(m_rawData.AsSpan(80, 8));
        data[offset + 11] = BitsToByteBE(m_rawData.AsSpan(88, 8));

        // Handle the two extra bits
        byte extra = BitsToByteBE(m_rawData.AsSpan(96, 8));
        data[offset + 12] = (byte)((data[offset + 12] & 0x3F) | (extra & 0xC0));
        data[offset + 20] = (byte)((data[offset + 20] & 0xFC) | ((extra >> 4) & 0x03));

        // Second block
        data[offset + 21] = BitsToByteBE(m_rawData.AsSpan(100, 8));
        data[offset + 22] = BitsToByteBE(m_rawData.AsSpan(108, 8));
        data[offset + 23] = BitsToByteBE(m_rawData.AsSpan(116, 8));
        data[offset + 24] = BitsToByteBE(m_rawData.AsSpan(124, 8));
        data[offset + 25] = BitsToByteBE(m_rawData.AsSpan(132, 8));
        data[offset + 26] = BitsToByteBE(m_rawData.AsSpan(140, 8));
        data[offset + 27] = BitsToByteBE(m_rawData.AsSpan(148, 8));
        data[offset + 28] = BitsToByteBE(m_rawData.AsSpan(156, 8));
        data[offset + 29] = BitsToByteBE(m_rawData.AsSpan(164, 8));
        data[offset + 30] = BitsToByteBE(m_rawData.AsSpan(172, 8));
        data[offset + 31] = BitsToByteBE(m_rawData.AsSpan(180, 8));
        data[offset + 32] = BitsToByteBE(m_rawData.AsSpan(188, 8));
    }

}