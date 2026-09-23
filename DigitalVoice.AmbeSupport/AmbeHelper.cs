namespace DigitalVoice.AmbeSupport;

/// <summary>
/// Provides helper methods for working with AMBE/DV3000 audio packets.
/// </summary>
/// <remarks>
/// This class contains utility functions for:
/// <list type="bullet">
///   <item>
///     <description>
///     Identifying whether a given byte array represents a valid DV3000 speech packet.
///     </description>
///   </item>
///   <item>
///     <description>
///     Converting the PCM portion of DV3000 audio packets from big-endian
///     (network order) to little-endian (host order).
///     </description>
///   </item>
/// </list>
/// </remarks>
public static class AmbeHelper
{

    /// <summary>
    /// Check if the argument packet is a AMBE packet.
    /// </summary>
    /// <param name="packet">The AMBE packet to check.</param>
    /// <returns><c>True</c> if AMBE packet, otherwise <c>false</c></returns>
    public static bool IsAmbePacket(byte[]? packet)
    {
        return packet != null && packet.Length > 6 && packet[0] == 0x61 && packet[3] == 0x01 && packet[4] == 0x01;
    }

    /// <summary>
    /// Check if the argument packet is a speech packet.
    /// </summary>
    /// <param name="packet">The AMBE packet to check.</param>
    /// <returns><c>True</c> if speech packet, otherwise <c>false</c></returns>
    public static bool IsSpeechPacket(byte[]? packet)
    {
        return packet != null && packet.Length > 6 && packet[0] == 0x61 && packet[3] == 0x02;
    }

    /// <summary>
    /// Swaps the PCM portion of a DV3000 audio packet.
    /// </summary>
    /// <param name="pcm">
    /// A byte array containing a DV3000 audio packet. Must be exactly 326 bytes in length:
    /// 6 header bytes followed by 320 bytes of PCM audio data.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="pcm"/> is null or does not have a length of 326 bytes.
    /// </exception>
    /// <remarks>
    /// The first 6 bytes (packet header) are left unchanged.  
    /// Starting at byte 6, each pair of PCM bytes is swapped in-place.
    /// </remarks>
    public static void SwapPcmBytes(byte[] pcm)
    {
        if (pcm == null || pcm.Length != 326)
        {
            throw new ArgumentException("Expected DV3000 audio packet of 326 bytes (6 header + 320 PCM).", nameof(pcm));
        }
        for (int i = 6; i < pcm.Length; i += 2)
        {
            int i1 = i + 1;
            (pcm[i1], pcm[i]) = (pcm[i], pcm[i1]); // swap to little endian format
        }
    }
}
