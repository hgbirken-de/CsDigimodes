using NLog;

namespace DigitalVoice.Dmr;

/// <summary>
/// Provides access to DMR talk group information.
/// </summary>
/// <remarks>
public static class DmrTalkgroups
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static readonly Dictionary<int, (string Name, char CallType)> _talkgroups = [];


    public static (string Name, char CallType) GetHostInfo(int dmrId)
    {
        if (_talkgroups.TryGetValue(dmrId, out var data))
        {
            return data;
        }
        else
        {
            return default;
        }
    }

    public static bool TryGetHostInfo(int dmrId, out (string Name, char CallType) data)
    {
        if (_talkgroups.TryGetValue(dmrId, out data))
        {
            return true;
        }
        data = default;
        return false;
    }

    public static void LoadData(string filePath)
    {
        logger.Debug($"file_path = {filePath}");

        _talkgroups.Clear();

        foreach (var line in File.ReadLines(filePath))
        {
            var trimmedLine = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                continue;

            var parts = trimmedLine.Split(';');
            if (parts.Length == 3)
            {
                var id = int.Parse(parts[0].Trim());
                var name = parts[1].Trim();
                var callType = parts[2].Trim().ToUpper();
                if (callType.Length == 1 && (callType[0] is 'G' or 'P'))
                {
                    _talkgroups[id] = (name, callType[0]);
                }
            }
            else
            {
                logger.Error($"Invalid talk group definition: {line} - record ignored.");
            }
        }
    }

    /// <summary>
    /// Returns all known DMR talkgroups.
    /// </summary>
    public static IReadOnlyDictionary<int, (string Name, char CallType)> All => _talkgroups;
}