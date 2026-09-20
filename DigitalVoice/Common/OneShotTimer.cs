namespace DigitalVoice.Common;

/// <summary>
/// Thread safe one shot timer.
/// </summary>
public class OneShotTimer : System.Timers.Timer
{
    private readonly object _lock = new();

    public bool HasExpired { get; private set; } = false;

    public bool IsRunning { get; private set; } = false;

    private readonly Action? _onExpiredCallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="OneShotTimer"/> class with the specified interval and optional expiration callback.
    /// </summary>
    /// <param name="intervalMs">The interval in milliseconds after which the timer will expire.</param>
    /// <param name="onExpired">An optional callback that will be invoked once when the timer elapses.</param>
    /// <remarks>
    /// The timer is configured as a one-shot timer (non-repeating). To start the timer, call <see cref="Start"/> explicitly.
    /// </remarks>
    public OneShotTimer(double intervalMs, Action? onExpired = null) : base(intervalMs)
    {
        _onExpiredCallback = onExpired;
        AutoReset = false;
        Elapsed += OnElapsedInternal;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Elapsed -= OnElapsedInternal;
        base.Dispose(disposing);
    }

    /// <summary>
    /// Handles the <see cref="System.Timers.Timer.Elapsed"/> event when the timer interval elapses.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="System.Timers.ElapsedEventArgs"/> that contains the event data.</param>
    /// <remarks>
    /// This method marks the timer as expired, sets <see cref="IsRunning"/> to <c>false</c>,
    /// and invokes the optional expiration callback, if provided. Access is thread-safe.
    /// </remarks>
    private void OnElapsedInternal(object? sender, System.Timers.ElapsedEventArgs e)
    {
        lock (_lock)
        {
            HasExpired = true;
            IsRunning = false;
            _onExpiredCallback?.Invoke();
        }
    }

    /// <summary>
    /// Resets the timer with a new interval and restarts it.
    /// </summary>
    /// <param name="newIntervalMs">The new timer interval in milliseconds.</param>
    /// <remarks>
    /// Stops the timer if running, sets the <see cref="Interval"/> to the new value,
    /// and then starts the timer again.
    /// </remarks>
    public void Reset(double newIntervalMs)
    {
        Stop();
        Interval = newIntervalMs;
        Start();
    }

    /// <summary>
    /// Starts the timer and resets the expiration state.
    /// </summary>
    /// <remarks>
    /// Thread-safe: uses a lock to ensure safe access to internal state.
    /// Resets <see cref="HasExpired"/> to false and sets <see cref="IsRunning"/> to true.
    /// </remarks>
    public new void Start()
    {
        lock (_lock)
        {
            HasExpired = false;
            base.Start();
            IsRunning = true;
        }
    }

    /// <summary>
    /// Stops the timer and marks it as not running.
    /// </summary>
    /// <remarks>
    /// Thread-safe: uses a lock to ensure safe access to internal state.
    /// Sets <see cref="IsRunning"/> to false.
    /// Does not change the <see cref="HasExpired"/> flag.
    /// </remarks>
    public new void Stop()
    {
        lock (_lock)
        {
            base.Stop();
            IsRunning = false;
        }
    }
}