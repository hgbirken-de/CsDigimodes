using NLog;

namespace AmbeServer;

/// <summary>
/// Provides functionality to record and replay binary packets with a length-prefixed format.
/// </summary>
/// <remarks>
/// Packets are stored in a binary file with the following format:
/// - 2-byte little-endian length prefix (unsigned)
/// - Packet data of the specified length
/// </remarks>
public class PacketRecorder : IDisposable
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly string _filePath;
    private FileStream? _fileStream;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketRecorder"/> class.
    /// </summary>
    /// <param name="filePath">The path of the file used to store packets.</param>
    /// <param name="mode">The file mode to use when opening the file.</param>
    /// <param name="access">The file access level (read, write, or both).</param>
    public PacketRecorder(string filePath, FileMode mode, FileAccess access)
    {
        _filePath = filePath;
        _fileStream = new FileStream(_filePath, mode, access);
    }

    /// <summary>
    /// Writes a packet to the file with a 2-byte little-endian length prefix.
    /// </summary>
    /// <param name="data">The binary packet data to write.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the recorder has been disposed.</exception>
    /// <exception cref="ArgumentException">Thrown if the packet length exceeds 65535 bytes.</exception>
    public void WritePacket(byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (data.Length > ushort.MaxValue)
            throw new ArgumentException("Data too long for 2-byte length prefix (max 65535 bytes)");

        // Write length as 2 bytes (little-endian)
        _fileStream!.WriteByte((byte)(data.Length & 0xFF));         // low byte
        _fileStream.WriteByte((byte)((data.Length >> 8) & 0xFF));  // high byte

        _fileStream.Write(data, 0, data.Length);
        _fileStream.Flush();
    }

    /// <summary>
    /// Reads the next packet from the file.
    /// </summary>
    /// <returns>The packet data, or <c>null</c> if the end of the file is reached.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the recorder has been disposed.</exception>
    /// <exception cref="IOException">Thrown if the file is corrupted or truncated.</exception>
    public byte[]? ReadNextPacket()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Read length (2 bytes, little-endian)
        int low = _fileStream!.ReadByte();
        if (low == -1) return null; // EOF
        int high = _fileStream.ReadByte();
        if (high == -1) throw new IOException("Unexpected EOF reading length");

        int length = (high << 8) | low;

        byte[] buffer = new byte[length];
        int bytesRead = _fileStream.Read(buffer, 0, length);

        if (bytesRead < length)
            throw new IOException("Unexpected end of file or corrupt record");

        return buffer;
    }

    /// <summary>
    /// Resets the read pointer to the beginning of the file.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the recorder has been disposed.</exception>
    public void ResetReadPointer()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _fileStream?.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Closes the recorder and releases any file resources.
    /// </summary>
    public void Close()
    {
        Dispose();
    }

    /// <summary>
    /// Releases all resources used by the <see cref="PacketRecorder"/>.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _fileStream?.Flush();
        _fileStream?.Dispose();
        _fileStream = null;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}