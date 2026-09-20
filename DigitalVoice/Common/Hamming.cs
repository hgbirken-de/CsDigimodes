using NLog;

namespace DigitalVoice.Common;

/// <summary>
/// Provides encoding and decoding methods for various Hamming codes used in digital voice protocols.  
/// 
/// The class supports multiple Hamming code configurations:
/// - (15,11,3) with encode/decode methods: Encode15113_1, Decode15113_1, Encode15113_2, Decode15113_2  
/// - (13,9,3) with encode/decode methods: Encode1393, Decode1393  
/// - (10,6,3) with encode/decode methods: Encode1063, Decode1063  
/// - (16,11,4) with encode/decode methods: Encode16114, Decode16114  
/// - (17,12,3) with encode/decode methods: Encode17123, Decode17123  
/// 
/// Each method operates on a boolean array representing bits of the codeword.  
/// Data bits must occupy the first N positions of the array, while parity bits occupy the last M positions
/// depending on the code configuration. The decode methods also attempt to correct single-bit errors
/// based on the Hamming code syndrome.
/// </summary>
internal static class Hamming
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Decodes and corrects a (15,11,3) Hamming codeword in place.
    /// 
    /// The input array <paramref name="d"/> must contain exactly 15 bits:
    /// - d[0..10]  = data bits
    /// - d[11..14] = parity bits
    ///
    /// If a single-bit error is detected, it will be corrected in place.
    /// If no error is found, <c>false</c> is returned.
    /// If a single-bit error was corrected, <c>true</c> is returned.
    /// </summary>
    /// <param name="d">Boolean array of length 15 representing the Hamming codeword.</param>
    /// <returns>
    /// True if a single-bit error was corrected, false if no error was found.
    /// </returns>
    public static bool Decode15113_1(bool[] d)
    {
        if (d == null || d.Length != 15)
            throw new ArgumentException("Input must be a 15-bit array.");

        // Calculate expected parity checks
        bool c0 = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[6];
        bool c1 = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[7] ^ d[8] ^ d[9];
        bool c2 = d[0] ^ d[1] ^ d[4] ^ d[5] ^ d[7] ^ d[8] ^ d[10];
        bool c3 = d[0] ^ d[2] ^ d[4] ^ d[6] ^ d[7] ^ d[9] ^ d[10];

        byte n = 0;
        if (c0 != d[11]) n |= 0x01;
        if (c1 != d[12]) n |= 0x02;
        if (c2 != d[13]) n |= 0x04;
        if (c3 != d[14]) n |= 0x08;

        switch (n)
        {
            // Parity bit errors
            case 0x01: d[11] = !d[11]; return true;
            case 0x02: d[12] = !d[12]; return true;
            case 0x04: d[13] = !d[13]; return true;
            case 0x08: d[14] = !d[14]; return true;

            // Data bit errors
            case 0x0F: d[0] = !d[0]; return true;
            case 0x07: d[1] = !d[1]; return true;
            case 0x0B: d[2] = !d[2]; return true;
            case 0x03: d[3] = !d[3]; return true;
            case 0x0D: d[4] = !d[4]; return true;
            case 0x05: d[5] = !d[5]; return true;
            case 0x09: d[6] = !d[6]; return true;
            case 0x0E: d[7] = !d[7]; return true;
            case 0x06: d[8] = !d[8]; return true;
            case 0x0A: d[9] = !d[9]; return true;
            case 0x0C: d[10] = !d[10]; return true;

            // No bit errors
            default: return false;
        }
    }

    /// <summary>
    /// Encodes a (15,11,3) Hamming codeword in place by calculating the 4 parity bits.
    /// 
    /// The input array <paramref name="d"/> must contain at least 15 elements.
    /// - d[0..10] must contain the 11 data bits
    /// - d[11..14] will be overwritten with computed parity bits
    /// </summary>
    /// <param name="d">Boolean array of length 15 to hold the codeword.</param>
    public static void Encode15113_1(bool[] d)
    {
        if (d == null || d.Length != 15)
            throw new ArgumentException("Input must be a 15-bit array.");

        // Calculate parity bits
        d[11] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[6];
        d[12] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[7] ^ d[8] ^ d[9];
        d[13] = d[0] ^ d[1] ^ d[4] ^ d[5] ^ d[7] ^ d[8] ^ d[10];
        d[14] = d[0] ^ d[2] ^ d[4] ^ d[6] ^ d[7] ^ d[9] ^ d[10];
    }

    /// <summary>
    /// Decodes and error-corrects a (15,11,3) Hamming codeword (variant 2).
    /// 
    /// This checks the parity bits against the data bits and, if possible,
    /// corrects a single-bit error in-place.
    /// 
    /// Returns:
    /// - true if a correction was made
    /// - false if no correction was needed
    /// </summary>
    /// <param name="d">Boolean array of length 15 containing the codeword.</param>
    /// <returns>True if a correction was applied, otherwise false.</returns>
    public static bool Decode15113_2(Span<bool> d)
    {
        if (d == null || d.Length != 15)
            throw new ArgumentException("Input must be a 15-bit array.");

        // Calculate expected parity
        bool c0 = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8];
        bool c1 = d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[6] ^ d[8] ^ d[9];
        bool c2 = d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[7] ^ d[9] ^ d[10];
        bool c3 = d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7] ^ d[10];

        byte n = 0x00;
        n |= (c0 != d[11]) ? (byte)0x01 : (byte)0x00;
        n |= (c1 != d[12]) ? (byte)0x02 : (byte)0x00;
        n |= (c2 != d[13]) ? (byte)0x04 : (byte)0x00;
        n |= (c3 != d[14]) ? (byte)0x08 : (byte)0x00;

        switch (n)
        {
            // Parity bit errors
            case 0x01: d[11] = !d[11]; return true;
            case 0x02: d[12] = !d[12]; return true;
            case 0x04: d[13] = !d[13]; return true;
            case 0x08: d[14] = !d[14]; return true;

            // Data bit errors
            case 0x09: d[0] = !d[0]; return true;
            case 0x0B: d[1] = !d[1]; return true;
            case 0x0F: d[2] = !d[2]; return true;
            case 0x07: d[3] = !d[3]; return true;
            case 0x0E: d[4] = !d[4]; return true;
            case 0x05: d[5] = !d[5]; return true;
            case 0x0A: d[6] = !d[6]; return true;
            case 0x0D: d[7] = !d[7]; return true;
            case 0x03: d[8] = !d[8]; return true;
            case 0x06: d[9] = !d[9]; return true;
            case 0x0C: d[10] = !d[10]; return true;

            // No bit errors detected
            default: return false;
        }
    }

    /// <summary>
    /// Encodes a (15,11,3) Hamming codeword (variant 2) by calculating
    /// and setting the four parity bits at positions 11–14.
    /// </summary>
    /// <param name="d">
    /// Boolean array of length 15. 
    /// The first 11 entries (0–10) must contain the data bits. 
    /// The last 4 entries (11–14) will be overwritten with parity bits.
    /// </param>
    public static void Encode15113_2(bool[] d)
    {
        if (d == null || d.Length != 15)
            throw new ArgumentException("Input must be a 15-bit array.");

        // Calculate parity bits
        d[11] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8];
        d[12] = d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[6] ^ d[8] ^ d[9];
        d[13] = d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[7] ^ d[9] ^ d[10];
        d[14] = d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7] ^ d[10];
    }

    /// <summary>
    /// Decodes and corrects a single-bit error in a (13,9,3) Hamming codeword.
    /// </summary>
    /// <param name="d">
    /// Boolean array of length 13. 
    /// The first 9 entries (0–8) are data bits, the last 4 entries (9–12) are parity bits.
    /// </param>
    /// <returns>
    /// True if a bit was corrected, false if no correction was needed.
    /// </returns>
    public static bool Decode1393(bool[] d)
    {
        if (d == null || d.Length != 13)
            throw new ArgumentException("Input must be a 13-bit array.");

        // Calculate parity check bits
        bool c0 = d[0] ^ d[1] ^ d[3] ^ d[5] ^ d[6];
        bool c1 = d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7];
        bool c2 = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8];
        bool c3 = d[0] ^ d[2] ^ d[4] ^ d[5] ^ d[8];

        // Syndrome value (error locator)
        byte n = 0;
        n |= (c0 != d[9]) ? (byte)0x01 : (byte)0x00;
        n |= (c1 != d[10]) ? (byte)0x02 : (byte)0x00;
        n |= (c2 != d[11]) ? (byte)0x04 : (byte)0x00;
        n |= (c3 != d[12]) ? (byte)0x08 : (byte)0x00;

        switch (n)
        {
            // Parity bit errors
            case 0x01: d[9] = !d[9]; return true;
            case 0x02: d[10] = !d[10]; return true;
            case 0x04: d[11] = !d[11]; return true;
            case 0x08: d[12] = !d[12]; return true;

            // Data bit errors
            case 0x0F: d[0] = !d[0]; return true;
            case 0x07: d[1] = !d[1]; return true;
            case 0x0E: d[2] = !d[2]; return true;
            case 0x05: d[3] = !d[3]; return true;
            case 0x0A: d[4] = !d[4]; return true;
            case 0x0D: d[5] = !d[5]; return true;
            case 0x03: d[6] = !d[6]; return true;
            case 0x06: d[7] = !d[7]; return true;
            case 0x0C: d[8] = !d[8]; return true;

            // No error
            default: return false;
        }
    }

    /// <summary>
    /// Encodes a (13,9,3) Hamming codeword by calculating parity bits.
    /// </summary>
    /// <param name="d">
    /// Boolean array of length 13.  
    /// The first 9 entries (0–8) must be filled with data bits.  
    /// The last 4 entries (9–12) will be overwritten with computed parity bits.
    /// </param>
    public static void Encode1393(bool[] d)
    {
        if (d == null || d.Length != 13)
            throw new ArgumentException("Input must be a 13-bit array.");

        // Calculate parity check bits
        d[9] = d[0] ^ d[1] ^ d[3] ^ d[5] ^ d[6];
        d[10] = d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7];
        d[11] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8];
        d[12] = d[0] ^ d[2] ^ d[4] ^ d[5] ^ d[8];
    }

    /// <summary>
    /// Decodes and corrects a (10,6,3) Hamming codeword.  
    /// Detects and corrects single-bit errors in place.
    /// </summary>
    /// <param name="d">
    /// Boolean array of length 10.  
    /// First 6 entries (0–5) are data bits, last 4 entries (6–9) are parity bits.  
    /// </param>
    /// <returns>
    /// True if a bit was corrected (error detected and fixed),  
    /// false if no error was found.
    /// </returns>
    public static bool Decode1063(bool[] d)
    {
        if (d == null || d.Length != 10)
            throw new ArgumentException("Input must be a 10-bit array.");

        // Calculate expected parity bits
        bool c0 = d[0] ^ d[1] ^ d[2] ^ d[5];
        bool c1 = d[0] ^ d[1] ^ d[3] ^ d[5];
        bool c2 = d[0] ^ d[2] ^ d[3] ^ d[4];
        bool c3 = d[1] ^ d[2] ^ d[3] ^ d[4];

        byte n = 0;
        n |= (c0 != d[6]) ? (byte)0x01 : (byte)0x00;
        n |= (c1 != d[7]) ? (byte)0x02 : (byte)0x00;
        n |= (c2 != d[8]) ? (byte)0x04 : (byte)0x00;
        n |= (c3 != d[9]) ? (byte)0x08 : (byte)0x00;

        switch (n)
        {
            // Parity bit errors
            case 0x01: d[6] = !d[6]; return true;
            case 0x02: d[7] = !d[7]; return true;
            case 0x04: d[8] = !d[8]; return true;
            case 0x08: d[9] = !d[9]; return true;

            // Data bit errors
            case 0x07: d[0] = !d[0]; return true;
            case 0x0B: d[1] = !d[1]; return true;
            case 0x0D: d[2] = !d[2]; return true;
            case 0x0E: d[3] = !d[3]; return true;
            case 0x0C: d[4] = !d[4]; return true;
            case 0x03: d[5] = !d[5]; return true;

            // No errors
            default: return false;
        }
    }

    /// <summary>
    /// Encodes a (10,6,3) Hamming codeword by calculating and setting the parity bits.  
    /// </summary>
    /// <param name="d">
    /// Boolean array of length 10.  
    /// The first 6 entries (0–5) must contain the data bits.  
    /// The last 4 entries (6–9) will be overwritten with computed parity bits.  
    /// </param>
    public static void Encode1063(bool[] d)
    {
        if (d == null || d.Length != 10)
            throw new ArgumentException("Input must be a 10-bit array.");

        // Calculate parity bits
        d[6] = d[0] ^ d[1] ^ d[2] ^ d[5];
        d[7] = d[0] ^ d[1] ^ d[3] ^ d[5];
        d[8] = d[0] ^ d[2] ^ d[3] ^ d[4];
        d[9] = d[1] ^ d[2] ^ d[3] ^ d[4];
    }

    /// <summary>
    /// Attempts to decode and correct a (16,11,4) Hamming codeword.  
    /// Can detect and correct single-bit errors.  
    /// </summary>
    /// <param name="d">
    /// Boolean array of length 16.  
    /// The first 11 entries (0–10) contain data bits.  
    /// The last 5 entries (11–15) contain parity bits.  
    /// The method may modify the array in place to correct a detected error.  
    /// </param>
    /// <returns>
    /// True if an error was corrected or if no errors were found.  
    /// False if an unrecoverable error was detected.  
    /// </returns>
    public static bool Decode16114(bool[] d)
    {
        if (d == null || d.Length != 16)
            throw new ArgumentException("Input must be a 16-bit array.");

        // Calculate expected parity bits
        bool c0 = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8];
        bool c1 = d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[6] ^ d[8] ^ d[9];
        bool c2 = d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[7] ^ d[9] ^ d[10];
        bool c3 = d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7] ^ d[10];
        bool c4 = d[0] ^ d[2] ^ d[5] ^ d[6] ^ d[8] ^ d[9] ^ d[10];

        // Compare expected with actual
        byte n = 0x00;
        n |= (c0 != d[11]) ? (byte)0x01 : (byte)0x00;
        n |= (c1 != d[12]) ? (byte)0x02 : (byte)0x00;
        n |= (c2 != d[13]) ? (byte)0x04 : (byte)0x00;
        n |= (c3 != d[14]) ? (byte)0x08 : (byte)0x00;
        n |= (c4 != d[15]) ? (byte)0x10 : (byte)0x00;

        switch (n)
        {
            // Parity bit errors
            case 0x01: d[11] = !d[11]; return true;
            case 0x02: d[12] = !d[12]; return true;
            case 0x04: d[13] = !d[13]; return true;
            case 0x08: d[14] = !d[14]; return true;
            case 0x10: d[15] = !d[15]; return true;

            // Data bit errors
            case 0x19: d[0] = !d[0]; return true;
            case 0x0B: d[1] = !d[1]; return true;
            case 0x1F: d[2] = !d[2]; return true;
            case 0x07: d[3] = !d[3]; return true;
            case 0x0E: d[4] = !d[4]; return true;
            case 0x15: d[5] = !d[5]; return true;
            case 0x1A: d[6] = !d[6]; return true;
            case 0x0D: d[7] = !d[7]; return true;
            case 0x13: d[8] = !d[8]; return true;
            case 0x16: d[9] = !d[9]; return true;
            case 0x1C: d[10] = !d[10]; return true;

            // No errors
            case 0x00: return true;

            // Unrecoverable errors
            default: return false;
        }
    }

    /// <summary>
    /// Encodes a (16,11,4) Hamming codeword by calculating parity bits.  
    /// The first 11 entries of <paramref name="d"/> (indices 0–10) must contain data bits.  
    /// The last 5 entries (indices 11–15) are calculated and filled with parity bits.  
    /// </summary>
    /// <param name="d">
    /// Boolean array of length 16.  
    /// The method modifies indices 11–15 in place.  
    /// </param>
    public static void Encode16114(bool[] d)
    {
        if (d == null || d.Length != 16)
            throw new ArgumentException("Input must be a 16-bit array.");

        d[11] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[5] ^ d[7] ^ d[8];
        d[12] = d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[6] ^ d[8] ^ d[9];
        d[13] = d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[7] ^ d[9] ^ d[10];
        d[14] = d[0] ^ d[1] ^ d[2] ^ d[4] ^ d[6] ^ d[7] ^ d[10];
        d[15] = d[0] ^ d[2] ^ d[5] ^ d[6] ^ d[8] ^ d[9] ^ d[10];
    }

    /// <summary>
    /// Decodes a (17,12,3) Hamming codeword by checking and correcting single-bit errors.  
    /// The first 12 entries of <paramref name="d"/> (indices 0–11) must contain data bits.  
    /// The last 5 entries (indices 12–16) contain parity bits.  
    /// </summary>
    /// <param name="d">
    /// Boolean array of length 17.  
    /// The method modifies bits in place if a correctable error is detected.  
    /// </param>
    /// <returns>
    /// True if no errors or a single-bit error was corrected;  
    /// False if an unrecoverable error occurred.  
    /// </returns>
    public static bool Decode17123(bool[] d)
    {
        if (d == null || d.Length != 17)
            throw new ArgumentException("Input must be a 17-bit array.");

        // Calculate the expected parity bits
        bool c0 = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[6] ^ d[7] ^ d[9];
        bool c1 = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[7] ^ d[8] ^ d[10];
        bool c2 = d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[8] ^ d[9] ^ d[11];
        bool c3 = d[0] ^ d[1] ^ d[4] ^ d[5] ^ d[7] ^ d[10];
        bool c4 = d[0] ^ d[1] ^ d[2] ^ d[5] ^ d[6] ^ d[8] ^ d[11];

        // Compare these with the actual parity bits
        byte n = 0;
        if (c0 != d[12]) n |= 0x01;
        if (c1 != d[13]) n |= 0x02;
        if (c2 != d[14]) n |= 0x04;
        if (c3 != d[15]) n |= 0x08;
        if (c4 != d[16]) n |= 0x10;

        switch (n)
        {
            // Parity bit errors
            case 0x01: d[12] = !d[12]; return true;
            case 0x02: d[13] = !d[13]; return true;
            case 0x04: d[14] = !d[14]; return true;
            case 0x08: d[15] = !d[15]; return true;
            case 0x10: d[16] = !d[16]; return true;

            // Data bit errors
            case 0x1B: d[0] = !d[0]; return true;
            case 0x1F: d[1] = !d[1]; return true;
            case 0x17: d[2] = !d[2]; return true;
            case 0x07: d[3] = !d[3]; return true;
            case 0x0E: d[4] = !d[4]; return true;
            case 0x1C: d[5] = !d[5]; return true;
            case 0x11: d[6] = !d[6]; return true;
            case 0x0B: d[7] = !d[7]; return true;
            case 0x16: d[8] = !d[8]; return true;
            case 0x05: d[9] = !d[9]; return true;
            case 0x0A: d[10] = !d[10]; return true;
            case 0x14: d[11] = !d[11]; return true;

            // No bit errors
            case 0x00: return true;

            // Unrecoverable errors
            default: return false;
        }
    }

    /// <summary>
    /// Encodes a (17,12,3) Hamming codeword by computing the parity bits.  
    /// The first 12 entries of <paramref name="d"/> (indices 0–11) must contain data bits.  
    /// The last 5 entries (indices 12–16) will be filled with calculated parity bits.  
    /// </summary>
    /// <param name="d">
    /// Boolean array of length 17.  
    /// The method modifies parity bits in place (indices 12–16).  
    /// </param>
    public static void Encode17123(bool[] d)
    {
        if (d == null || d.Length != 17)
            throw new ArgumentException("Input must be a 17-bit array.");

        d[12] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[6] ^ d[7] ^ d[9];
        d[13] = d[0] ^ d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[7] ^ d[8] ^ d[10];
        d[14] = d[1] ^ d[2] ^ d[3] ^ d[4] ^ d[5] ^ d[8] ^ d[9] ^ d[11];
        d[15] = d[0] ^ d[1] ^ d[4] ^ d[5] ^ d[7] ^ d[10];
        d[16] = d[0] ^ d[1] ^ d[2] ^ d[5] ^ d[6] ^ d[8] ^ d[11];
    }

}