namespace DigitalVoice.AmbeSupport;

/// <summary>
/// Konfiguration für den Zugriff auf einen AMBE3000R-Vocoder, entweder lokal über einen
/// physisch angeschlossenen Stick (serielle Verbindung) oder remote über einen AMBE-Server
/// (UDP). Welcher Zugriffsweg tatsächlich genutzt wird, steuert <see cref="AmbeServiceType"/>.
/// </summary>
public record AmbeConfig
{
    /// <summary>
    /// Legt fest, ob der AMBE3000R lokal über den seriell angeschlossenen Stick oder remote
    /// über einen AMBE-Server angesprochen wird.
    /// </summary>
    public AmbeServiceType AmbeServiceType { get; set; } = AmbeServiceType.Stick;

    /// <summary>
    /// IP-Adresse des AMBE-Servers. Nur relevant, wenn <see cref="AmbeServiceType"/>
    /// auf den netzwerkbasierten Zugriff (UDP) gesetzt ist.
    /// </summary>
    public string ServerAddr { get; set; } = "127.0.0.1";

    /// <summary>
    /// Port des AMBE-Servers. Nur relevant, wenn <see cref="AmbeServiceType"/>
    /// auf den netzwerkbasierten Zugriff (UDP) gesetzt ist.
    /// </summary>
    public int ServerPort { get; set; } = 2460;

    /// <summary>
    /// Baudrate für die serielle Verbindung zum AMBE3000R-Stick. Nur relevant, wenn
    /// <see cref="AmbeServiceType"/> auf den lokalen Stick-Zugriff gesetzt ist.
    /// </summary>
    public int StickBaudrate { get; set; } = 460800;

    /// <summary>
    /// Name des COM-Ports, an dem der AMBE3000R-Stick angeschlossen ist (z.B. "COM13").
    /// Nur relevant, wenn <see cref="AmbeServiceType"/> auf den lokalen Stick-Zugriff gesetzt ist.
    /// </summary>
    public string StickComport { get; set; } = "COM13";

}