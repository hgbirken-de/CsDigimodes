using System.Security.Cryptography;
using System.Text;

namespace DigitalVoice.Common;

/// <summary>
/// Class that provides comprehensive helper methods.
/// </summary>
public static class Helpers
{

    /// <summary>
    /// Compare two byte arrays.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns>True if both arrays are equal, otherwise false.</returns>
    public static bool CompareBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Determines whether a specified sequence of bytes (<paramref name="b"/>) 
    /// appears at a given <paramref name="offset"/> within another byte array (<paramref name="a"/>).
    /// </summary>
    /// <param name="a">The byte array to search within.</param>
    /// <param name="offset">The zero-based index in <paramref name="a"/> where the comparison starts.</param>
    /// <param name="b">The byte sequence to compare against.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="b"/> matches the bytes in <paramref name="a"/> starting at <paramref name="offset"/>; 
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Returns <c>false</c> if <paramref name="a"/> or <paramref name="b"/> is <c>null</c>, 
    /// if <paramref name="offset"/> is negative, 
    /// or if there are not enough bytes remaining in <paramref name="a"/> from <paramref name="offset"/> to match <paramref name="b"/>.
    /// </remarks>
    public static bool ContainsBytes(byte[] a, int offset, byte[] b)
    {
        // Check that there's enough space in 'a' from 'offset' to match 'b'
        if (a == null || b == null) return false;
        if (offset < 0 || b.Length == 0) return false;
        if (offset + b.Length > a.Length) return false;

        for (int i = 0; i < b.Length; i++)
        {
            if (a[i + offset] != b[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Formats a string to a fixed length by either truncating it or padding it with spaces on the right.
    /// </summary>
    /// <param name="input">The input string to format. If null, an empty string is used.</param>
    /// <param name="length">The desired fixed length of the output string.</param>
    /// <returns>
    /// A string of exactly <paramref name="length"/> characters, truncated or space-padded as needed.
    /// </returns>
    public static string FormatField(string? input, int length)
    {
        string safeInput = input ?? string.Empty;
        return safeInput.Length > length ? safeInput[..length] : safeInput.PadRight(length);
    }

    /// <summary>
    /// Computes the SHA-256 hash of the given text and returns it as an uppercase hexadecimal string.
    /// </summary>
    /// <param name="text">The input text to hash.</param>
    /// <returns>SHA-256 hash as a 64-character uppercase hexadecimal string.</returns>
    public static string GetSha256Hash(string text)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(text);
        byte[] hash = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Checks whether the specified byte array starts with the given ASCII string prefix.
    /// </summary>
    /// <param name="data">The byte array to check.</param>
    /// <param name="prefix">The ASCII string to compare against the start of the byte array.</param>
    /// <returns>True if the byte array begins with the given ASCII prefix; otherwise, false.</returns>
    public static bool HasPrefix(byte[] data, string prefix)
    {
        if (data.Length < prefix.Length) return false;

        for (int i = 0; i < prefix.Length; i++)
        {
            if (data[i] != (byte)prefix[i])
                return false;
        }
        return true;
    }

    /// <summary>
    /// Converts a 32-bit signed integer to a big-endian byte array.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>A 4-byte array representing the integer in big-endian byte order.</returns>
    public static byte[] IntToBigEndianBytes(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return bytes;
    }

    /// <summary>
    /// Replaces sequences of multiple spaces in the input string with a single space,
    /// effectively normalizing whitespace within the string.
    /// </summary>
    /// <param name="input">The string to normalize.</param>
    /// <returns>A new string with consecutive spaces replaced by a single space.</returns>
    /// <example>
    /// string result = NormalizeSpaces("This   is   a  test"); result == "This is a test"
    /// </example>
    public static string NormalizeSpaces(string input)
    {
        var sb = new StringBuilder(input.Length);
        bool previousWasSpace = false;
        foreach (char c in input)
        {
            if (c == ' ')
            {
                if (!previousWasSpace)
                {
                    sb.Append(c); // append first space
                    previousWasSpace = true;
                }
            }
            else
            {
                sb.Append(c);
                previousWasSpace = false;
            }
        }

        return sb.ToString();
    }

}
