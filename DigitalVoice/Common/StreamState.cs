namespace DigitalVoice.Common;

/// <summary>
/// Represents the possible states of a digital voice stream.
/// </summary>
public enum StreamState
{
    /// <summary>
    /// The stream has ended normally and no more data is expected.
    /// </summary>
    End,

    /// <summary>
    /// No stream is currently active; the system is idle.
    /// </summary>
    Idle,

    /// <summary>
    /// The stream was interrupted unexpectedly or connection was lost.
    /// </summary>
    Lost,

    /// <summary>
    /// A new stream has started.
    /// </summary>
    New,

    /// <summary>
    /// A stream is actively receiving data.
    /// </summary>
    Streaming,

    /// <summary>
    /// A stream is actively transmitting data.
    /// </summary>
    Transmitting
}
