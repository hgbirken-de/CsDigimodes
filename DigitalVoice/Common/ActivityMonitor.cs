using NLog;

namespace DigitalVoice.Common;

/// <summary>
/// Generic activity monitor (watchdog) that calls a callback
/// if no activity was seen for a specified timeout period.
/// </summary>
public class ActivityMonitor(Action onInactivityCallback, double timeoutSeconds = 2.0, double checkIntervalSeconds = 0.1) : IDisposable
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly Action _onInactivityCallback = onInactivityCallback ?? throw new ArgumentNullException(nameof(onInactivityCallback));
    private readonly double _timeoutSeconds = timeoutSeconds;
    private readonly double _checkIntervalSeconds = checkIntervalSeconds;

    private Thread? _thread;
    private bool _stopRequested;
    private bool _paused;
    private readonly object _lock = new();

    private DateTime _lastActivity = DateTime.UtcNow;

    /// <summary>
    /// Call this whenever activity occurs (resets timer).
    /// </summary>
    public void NotifyActivity()
    {
        lock (_lock)
        {
            _lastActivity = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Starts or resumes the background watchdog thread.
    /// </summary>
    public void Start()
    {
        if (_thread != null && _thread.IsAlive)
        {
            // resume if paused
            _paused = false;
            logger.Debug("ActivityMonitor resumed");
            return;
        }

        _stopRequested = false;
        _paused = false;
        _thread = new Thread(Loop)
        {
            IsBackground = true,
            Name = "ActivityMonitorThread"
        };
        _thread.Start();
        logger.Debug("ActivityMonitor started");
    }

    /// <summary>
    /// Temporarily pauses the watchdog (inactivity ignored).
    /// </summary>
    public void Pause()
    {
        _paused = true;
        logger.Debug("ActivityMonitor paused");
    }

    /// <summary>
    /// Stops the background watchdog thread.
    /// </summary>
    public void Stop()
    {
        _stopRequested = true;
        if (_thread != null && _thread.IsAlive)
        {
            if (!_thread.Join(TimeSpan.FromSeconds(1)))
                _thread.Interrupt();
            _thread = null;
        }
        logger.Debug("ActivityMonitor stopped");
    }

    private void Loop()
    {
        while (!_stopRequested)
        {
            Thread.Sleep(TimeSpan.FromSeconds(_checkIntervalSeconds));
            if (_paused)
                continue;

            double elapsed;
            lock (_lock)
            {
                elapsed = (DateTime.UtcNow - _lastActivity).TotalSeconds;
            }

            if (elapsed > _timeoutSeconds)
            {
                try
                {
                    logger.Debug("Inactivity detected - triggering callback.");
                    _onInactivityCallback();
                }
                catch (Exception ex)
                {
                    logger.Debug(ex, "Error in inactivity callback.");
                }

                lock (_lock)
                {
                    _lastActivity = DateTime.UtcNow;
                }
            }
        }
    }

    public void Dispose()
    {
        Stop();
    }
}