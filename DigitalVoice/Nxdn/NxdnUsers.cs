
using NLog;

namespace DigitalVoice.Nxdn;

/// <summary>
/// Utility class to load and query NXDN user records. The records are stored 
/// in a static dictionary keyed by the NXDN id.
/// </summary>
public static class NxdnUsers
{
    const string fileName = "NXDNUsers.txt";

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static NxdnUsers()
    {
        //LoadDataFromEmbeddedResource();
    }

    /// <summary>
    /// Internal storage for all parsed users.
    /// Key: RADIOID
    /// Value: Tuple containing (CALLSIGN, FIRST_NAME, LAST_NAME, CITY, STATE, COUNTRY)
    /// </summary>
    public readonly static Dictionary<int, (string callsign, string firstName, string lastName, string city, string state, string country)> _users = [];

    /// <summary>
    /// Returns user information given the user's NXDN Id.
    /// If the key does not exist, returns a tuple of default values.
    /// </summary>
    /// <param name="id">the NXDN radio id</param>
    /// <returns>Tuple (string callsign, string firstName, string lastName, string city, string state, string country).</returns>
    public static (string callsign, string firstName, string lastName, string city, string state, string country) GetUserInfo(int id)
    {
        if (_users.TryGetValue(id, out var info))
        {
            return info;
        }
        else 
        {
            return default;
        }
    }

    /// <summary>
    /// Attempts to retrieve user information given the user's NXDN Id.
    /// </summary>
    /// <param name="id">The user's NXDN Id.</param>
    /// <param name="info">A tuple with user infromation, or a default tuple if not found.</param>
    /// <returns><c>true</c> if the user was found in the dictionary, otherwise <c>false</c>.</returns>
    public static bool TryGetUserInfo(int id, out (string callsign, string firstName, string lastName, string city, string state, string country) info)
    {
        if (_users.TryGetValue(id, out info))
        {
            return true;
        }

        info = default;
        return false;
    }

    /// <summary>
    /// Loads a NXDN user file and parses its entries into the internal dictionary.
    /// Expected file format (comma separated):
    /// ID   44.148.230.201  62031
    ///
    /// Columns: (RADIOID, CALLSIGN, FIRST_NAME, LAST_NAME, CITY, STATE, COUNTRY)
    /// </summary>
    /// <param name="filePath">Path to the user file.</param>
    public static void LoadData(string filePath)
    {
        logger.Debug($"filePath = {filePath}");

        _users.Clear();
        foreach (var line in File.ReadLines(filePath))
        {
            var trimmedLine = line.Trim();

            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                continue;

            var parts = trimmedLine.Split(',');
            if (parts.Length >= 7)
            {
                if (int.TryParse(parts[0].Trim(), out var id))
                {
                    var callsign = parts[1].Trim();
                    var firstName = parts[2].Trim();
                    var lastName = parts[3].Trim();
                    var city = parts[4].Trim();
                    var state = parts[5].Trim();
                    var country = parts[6].Trim();
                    _users[id] = (callsign, firstName, lastName, city, state, country);
                }
                else
                {
                    logger.Error($"Invalid Id in line: {line}");
                }
            }
        }
        logger.Debug($"{_users.Count} user records loaded.");
    }

    /// <summary>
    /// Returns all known NXDN user mappings.
    /// </summary>
    public static IReadOnlyDictionary<int, (string callsign, string firstName, string lastName, string city, string state, string country)> All => _users;
}