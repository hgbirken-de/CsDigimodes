
using AmbeServer;

namespace AmbeServerRunner;

/// <summary>
/// Helper class to run the AmbeServer.
/// </summary>
public class AmbeServerRunner
{
    static readonly ManualResetEventSlim quitEvent = new(false);

    static void Main(string[] args)
    {
        //Console.CancelKeyPress += (sender, e) =>
        //{
        //    Console.WriteLine("Ctrl+C pressed, exiting...");
        //    quitEvent.Set();
        //    client.Stop();
        //    e.Cancel = true; // prevents immediate termination, lets us clean up
        //};

        // parse program args ...
        var argDict = ParseArgs(args);

        bool useq = false; 
        bool record = false;
        int port = 2460; 

        _ = argDict.TryGetValue("useq", out var useqStr) && bool.TryParse(useqStr, out useq);
        _ = argDict.TryGetValue("record", out var recordStr) && bool.TryParse(recordStr, out record);
        string serialPort = argDict.TryGetValue("serialport", out string? serialPortValue) ? serialPortValue : "COM13";
        if (argDict.TryGetValue("port", out string? portStr))
        {
            port = int.TryParse(portStr, out int portValue) ? portValue : port;
        }

        //var cfg = AmbeServer.AmbeServer.LoadConfig("AmbeServerConfig.yaml");
        
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            Console.WriteLine("Process exit event triggered...");
        };

        Console.WriteLine("AMBE Server running. Press Ctrl+C to stop.");

        //AmbeServer.AmbeServer server = new(cfg);
        AmbeServer.AmbeServer server = new(null);

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Ctrl+C pressed, exiting...");
            //server.Stop();
            quitEvent.Set();
            e.Cancel = true; // prevents immediate termination, lets us clean up
        };


        // Wait here until Ctrl+C pressed
        quitEvent.Wait();

        server.Shutdown();

        Console.WriteLine("Cleanup done. Bye!");
    }

    static Dictionary<string, string> ParseArgs(string[] args)
    {
        return args
            .Select(arg => arg.Split('=', 2)) // handle key=value only
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1]);
    }
}
