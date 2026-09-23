
namespace DigitalVoice.AmbeSupport;

/// <summary>
/// Abstraktion für den Zugriff auf einen AMBE3000R-Vocoder-Chip, unabhängig davon,
/// ob die Kommunikation lokal über eine serielle/USB-Verbindung (<c>Ambe3000RController</c>
/// unter Windows, <c>Ambe3000Usb</c> unter Android) oder remote über HTTP gegen einen
/// AmbeServer (<c>AmbeClient</c>) erfolgt.
/// </summary>
public interface IAmbe3000RController
{
    /// <summary>
    /// Gibt an, ob die Verbindung zum AMBE3000R-Chip aktuell geöffnet und nutzbar ist.
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// Öffnet die Verbindung zum AMBE3000R-Chip.
    /// </summary>
    void Open();

    /// <summary>
    /// Schließt die Verbindung zum AMBE3000R-Chip.
    /// </summary>
    void Close();

    /// <summary>
    /// Liest die Produktkennung (Product ID) des AMBE3000R-Chips aus.
    /// </summary>
    /// <returns>Die Produktkennung, oder <c>null</c> bei Timeout/Fehler.</returns>
    /// <exception cref="InvalidOperationException">Die Verbindung ist nicht geöffnet.</exception>
    string? GetProductId();

    /// <summary>
    /// Liest die Firmware-/Versionsinformation des AMBE3000R-Chips aus.
    /// </summary>
    /// <returns>Die Versionsinformation, oder <c>null</c> bei Timeout/Fehler.</returns>
    /// <exception cref="InvalidOperationException">Die Verbindung ist nicht geöffnet.</exception>
    string? GetVersion();

    /// <summary>
    /// Setzt den AMBE3000R-Chip zurück.
    /// </summary>
    /// <exception cref="InvalidOperationException">Die Verbindung ist nicht geöffnet.</exception>
    void Reset();

    /// <summary>
    /// Dekodiert einen AMBE-Datenblock zu PCM-Samples.
    /// </summary>
    /// <param name="ambeData">Der komprimierte AMBE-Datenblock.</param>
    /// <param name="blockSize">Länge des AMBE-Datenblocks (z.B. 7).</param>
    /// <returns>PCM-Daten als Array von <see cref="short"/> (little endian).</returns>
    /// <exception cref="InvalidOperationException">Die Verbindung ist nicht geöffnet.</exception>
    short[] Decode(byte[] ambeData, int blockSize);

    /// <summary>
    /// Kodiert PCM-Samples zu einem AMBE-Datenblock.
    /// </summary>
    /// <param name="pcmSamples">160 PCM-Samples.</param>
    /// <returns>Der komprimierte AMBE-Datenblock.</returns>
    /// <exception cref="ArgumentException"><paramref name="pcmSamples"/> fehlt oder hat nicht die erwartete Länge.</exception>
    byte[] Encode(short[] pcmSamples);

    /// <summary>
    /// Sendet ein Paket an den Chip, ohne auf eine Antwort zu warten.
    /// </summary>
    /// <param name="packet">Das zu sendende Paket.</param>
    /// <exception cref="InvalidOperationException">Die Verbindung ist nicht geöffnet.</exception>
    void SendPacket(byte[] packet);

    /// <summary>
    /// Empfängt ein Paket vom Chip, ohne vorher selbst etwas zu senden.
    /// </summary>
    /// <returns>Das empfangene Paket, oder <c>null</c> bei Timeout.</returns>
    /// <exception cref="InvalidOperationException">Die Verbindung ist nicht geöffnet.</exception>
    byte[]? ReceivePacket();

    /// <summary>
    /// Sendet ein Paket an den Chip und wartet synchron auf die Antwort. Reines
    /// Pass-Through ohne Dateninterpretation.
    /// </summary>
    /// <param name="packet">Das zu sendende Paket.</param>
    /// <returns>Das empfangene Antwortpaket, oder <c>null</c> bei Timeout.</returns>
    /// <exception cref="InvalidOperationException">Die Verbindung ist nicht geöffnet.</exception>
    byte[]? SendReceivePacket(byte[] packet);

    /// <summary>
    /// Sendet ein Paket an den Chip und wartet asynchron auf die Antwort. Reines
    /// Pass-Through ohne Dateninterpretation.
    /// </summary>
    /// <param name="packet">Das zu sendende Paket.</param>
    /// <param name="cancellationToken">Token zum Abbrechen der Wartezeit.</param>
    /// <returns>Das empfangene Antwortpaket, oder <c>null</c> bei Timeout/Abbruch.</returns>
    /// <exception cref="InvalidOperationException">Die Verbindung ist nicht geöffnet.</exception>
    Task<byte[]?> SendReceivePacketAsync(byte[] packet, CancellationToken cancellationToken = default);
}