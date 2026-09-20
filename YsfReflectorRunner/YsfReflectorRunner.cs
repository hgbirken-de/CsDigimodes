using YsfReflector;

namespace YsfReflectorRunner;

public class YsfReflectorRunner
{

    static readonly ManualResetEventSlim quitEvent = new(false);

    static void Main(string[] args)
    {
        Reflector reflector = new();

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            Console.WriteLine("Process exit event triggered...");
        };

        Console.WriteLine($"Running YsfReflector. Press Ctrl+C to stop.");

        reflector.Start();

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Ctrl+C pressed, exiting...");
            quitEvent.Set();
            e.Cancel = true; // prevents immediate termination, lets us clean up
        };

        // Wait here until Ctrl+C pressed
        quitEvent.Wait();

        reflector.Stop();

        Thread.Sleep(1000);
    }
}