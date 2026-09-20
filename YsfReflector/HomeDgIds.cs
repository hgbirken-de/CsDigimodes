using NLog;

namespace YsfReflector;

public class HomeDgidEntry(string callsign, int dgid, int time)
{
    public string Callsign { get; set; } = callsign;
    public int Dgid { get; set; } = dgid;
    public int Time { get; set; } = time;
}

public class HomeDgIds
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly string _filePath;
    private readonly Dictionary<string, HomeDgidEntry> _homeDgids = [];
    private readonly object _lock = new();

    public HomeDgIds(string filePath)
    {
        _filePath = filePath;
        Load();
        WatchForChanges();
    }
    public HomeDgidEntry? GetHomeDgidEntry(string callsign)
    {
        lock (_lock)
            return _homeDgids[callsign];
    }

    public void Load()
    {
        lock (_lock)
        {
            _homeDgids.Clear();

            if (!File.Exists(_filePath))
            {
                logger.Warn($"Home DG-ID file not found: {_filePath}");
                return;
            }

            var lines = File.ReadAllLines(_filePath).Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith('#'));

            foreach (var line in lines)
            {
                string[] v = line.Split(';');
                if (v.Length > 2)
                {
                    string callsign = v[0];
                    int dgid = int.Parse(v[1]);
                    int time = int.Parse(v[2]);
                    _homeDgids[callsign] = new(callsign, dgid, time);
                }
            }

            logger.Info($"Loaded {_homeDgids.Count} home DG-ID entries from {_filePath}");
        }
    }

    private void Reload()
    {
        const int n = 5;
        for (int i = 0; i < n; i++)
        {
            try
            {
                Load();
                return;
            }
            catch (IOException)
            {
                logger.Warn("File locked, retrying...");
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unexpected error while reloading home DG-IDs.");
                return;
            }
        }
        logger.Error($"Failed to reload home dgid after {n} attempts.");
    }

    private void WatchForChanges()
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (dir is null) return;

        var watcher = new FileSystemWatcher(dir)
        {
            Filter = Path.GetFileName(_filePath),
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        watcher.Changed += (s, e) =>
        {
            logger.Info("Home DG-ID file changed, reloading...");
            Thread.Sleep(200); // wait a bit for the editor to finish writing
            Reload();
        };
        watcher.EnableRaisingEvents = true;
    }
}