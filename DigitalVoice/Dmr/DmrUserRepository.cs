namespace DigitalVoice.Dmr;

public record DmrUserRecord
{
    public int DmrId { get; set; }
    public string Callsign { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public static class DmrUserRepository
{
    readonly static Dictionary<int, string> _byId = [];

    //readonly static Dictionary<string, int> _byCallsign = new(StringComparer.OrdinalIgnoreCase);

    public static void LoadData(string filePath)
    {
        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Split(',');
            if (parts.Length < 7) continue;

            if (!int.TryParse(parts[0], out int dmrId)) continue;

            //_byId[dmrId] = record;
            _byId[dmrId] = line;

            //string callsign = record.Callsign.Trim();
            //if (!_byCallsign.ContainsKey(callsign))
            //    _byCallsign[callsign] = dmrId;
        }
    }

    public static DmrUserRecord? GetByDmrId(int dmrId)
    {
        if (_byId.TryGetValue(dmrId, out string? s))
        {
            var parts = s.Split(',');
            var result = new DmrUserRecord
            {
                DmrId = dmrId,
                Callsign = parts[1],
                FirstName = parts[2],
                LastName = parts[3],
                City = parts[4],
                State = parts[5],
                Country = parts[6],
            };

            return result;
        }
        else
        {
            return null;
        }
    }

    //public static bool TryGetById(int dmrId, out DmrRecord? record) => _byId.TryGetValue(dmrId, out record);

    //public static bool TryGetByCallsign(string callsign, out int dmrId) => _byCallsign.TryGetValue(callsign, out dmrId);
}