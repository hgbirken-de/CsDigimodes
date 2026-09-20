namespace YsfReflector;

using System;
using System.Net;

public static class NetUtils
{
    /// <summary>
    /// Converts an IPv4 address string (and optional port) into a 64-bit value.
    /// If port = 0, only the IP is encoded.
    /// </summary>
    public static ulong IpPortToLong(string ipString, int port = 0)
    {
        if (string.IsNullOrWhiteSpace(ipString))
            throw new ArgumentException("IP address cannot be null or empty.", nameof(ipString));

        if (!IPAddress.TryParse(ipString, out var ip))
            throw new FormatException($"Invalid IP address format: {ipString}");

        // Convert IP to 32-bit integer
        byte[] bytes = ip.GetAddressBytes();
        if (bytes.Length != 4)
            throw new NotSupportedException("Only IPv4 addresses are supported.");

        uint ipValue = ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];

        // Pack into 64-bit value: IP in high bits, port in low bits
        return ((ulong)ipValue << 16) | (ushort)Math.Max(0, Math.Min(port, 65535));
    }

    public static ulong IpPortToLong(IPAddress ip, int port = 0)
    {
        ArgumentNullException.ThrowIfNull(ip);

        byte[] bytes = ip.GetAddressBytes();
        if (bytes.Length != 4)
            throw new NotSupportedException("Only IPv4 addresses are supported.");

        uint ipValue = ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
        return ((ulong)ipValue << 16) | (ushort)Math.Max(0, Math.Min(port, 65535));
    }

    /// <summary>
    /// Extracts the IP and port from a 64-bit encoded value.
    /// </summary>
    public static (string Ip, int Port) LongToIpPort(ulong value)
    {
        uint ipValue = (uint)(value >> 16);
        ushort port = (ushort)(value & 0xFFFF);

        string ipString = string.Join(".",
            (ipValue >> 24) & 0xFF,
            (ipValue >> 16) & 0xFF,
            (ipValue >> 8) & 0xFF,
            ipValue & 0xFF);

        return (ipString, port);
    }
}
