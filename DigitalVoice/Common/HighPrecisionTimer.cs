using System.Diagnostics;

namespace DigitalVoice.Common;

/// <summary>
/// A high-precision, software-based timer that repeatedly invokes a callback 
/// at a specified interval, with better accuracy than <see cref="Timer"/>.
/// </summary>
/// <remarks>
/// This timer uses a dedicated background thread and <see cref="Stopwatch"/> 
/// for precise time measurement. It is suitable for intervals down to 1 ms, 
/// depending on system performance and CPU availability.
/// 
/// Unlike hardware timers, accuracy can still be affected by OS scheduling, 
/// but it typically outperforms standard .NET timers for short intervals.
/// </remarks>
public class HighPrecisionTimer : IDisposable
{
    private readonly Action _callback;
    private readonly int _intervalMs;
    private Thread? _thread;
    private bool _running;
    private readonly object _lock = new();

    /// <summary>
    /// Creates a new <see cref="HighPrecisionTimer"/> instance.
    /// </summary>
    /// <param name="intervalMs">The interval between ticks, in milliseconds.</param>
    /// <param name="callback">The method to call each time the interval elapses.</param>
    public HighPrecisionTimer(int intervalMs, Action callback)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(intervalMs, nameof(intervalMs));
        _intervalMs = intervalMs;
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    /// <summary>
    /// Gets a value indicating whether the timer is currently running.
    /// </summary>
    public bool IsRunning => _running;

    /// <summary>
    /// Starts the timer. If already running, it will be restarted.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            if (_running) return; // already running
            _running = true;
            _thread = new Thread(Run) { IsBackground = true };
            _thread.Start();
        }
    }

    /// <summary>
    /// Stops the timer. Does nothing if it is not running.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            _running = false;
        }
        _thread?.Join();
    }

    /// <summary>
    /// Executes the timer loop in a background thread, invoking the callback
    /// at fixed intervals until the timer is stopped.
    /// </summary>
    /// <remarks>
    /// Uses a <see cref="Stopwatch"/> for high-precision timing and sleeps briefly
    /// between iterations to prevent a busy-wait loop.
    /// This method runs continuously while <c>_running</c> is <c>true</c>.
    /// </remarks>
    private void Run()
    {
        var sw = Stopwatch.StartNew();
        long nextTick = 0;

        while (_running)
        {
            long now = sw.ElapsedMilliseconds;
            if (now >= nextTick)
            {
                _callback.Invoke();
                nextTick += _intervalMs;
            }
            Thread.Sleep(1); // small sleep to avoid busy loop
        }
    }

    /// <summary>
    /// Runs the timer loop on a background thread, invoking the callback at fixed intervals.
    /// </summary>
    /// <remarks>
    /// Uses a <see cref="Stopwatch"/> for high-resolution timing and attempts to invoke the callback
    /// approximately every <c>_intervalMs</c> milliseconds.
    ///
    /// If the callback execution takes longer than the interval, this method skips missed intervals
    /// to avoid running callbacks back-to-back and prevents timing drift by scheduling
    /// future ticks relative to the intended timeline.
    ///
    /// A short <see cref="Thread.Sleep(int)"/> call is used to reduce CPU usage without
    /// significantly affecting timer precision.
    ///
    /// The loop continues running until the <c>_running</c> flag is set to false.
    /// </remarks>
    /// 
    //private void Run()
    //{
    //    var sw = Stopwatch.StartNew();
    //    long nextTick = _intervalMs; // first tick after the interval

    //    while (_running)
    //    {
    //        long now = sw.ElapsedMilliseconds;

    //        if (now >= nextTick)
    //        {
    //            _callback.Invoke();

    //            // Schedule the next tick relative to when it *should* happen,
    //            // not when the callback ended, to avoid drift.
    //            nextTick += _intervalMs;

    //            // If we are already behind the next scheduled tick,
    //            // jump ahead to the next aligned tick.
    //            if (now > nextTick)
    //            {
    //                long missedTicks = (now - nextTick) / _intervalMs + 1;
    //                nextTick += missedTicks * _intervalMs;
    //            }
    //        }

    //        Thread.Sleep(1); // avoid busy-waiting
    //    }
    //}


    public void Dispose()
    {
        Stop();
    }
}

