using System.Text.Json;

namespace DigitalVoice.Dmr;

/// <summary>
/// Represents a DMR user record retrieved from radioid.net.
/// </summary>
public record DmrUserData
{
    public int Id { get; set; }
    public string? Callsign { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// Provides helper methods to retrieve DMR user information from the <c>https://radioid.net</c> API.
/// </summary>
public static class DmrUserDataReader
{

    static readonly HttpClient http = new();

    /// <summary>
    /// Asynchronously retrieves user information from the radioid.net API for a given DMR ID.
    /// </summary>
    public static async Task<DmrUserData?> GetUserAsync(int dmrId)
    {
        string url = $"https://radioid.net/api/dmr/user/?id={dmrId}";
        var json = await http.GetStringAsync(url);
        return ParseResponse(json);
    }

    /// <summary>
    /// Synchronously retrieves user information from the radioid.net API for a given DMR ID.
    /// Blocks the calling thread - prefer <see cref="GetUserAsync"/> where possible.
    /// </summary>
    public static DmrUserData? GetUser(int dmrId)
    {
        string url = $"https://radioid.net/api/dmr/user/?id={dmrId}";
        var json = http.GetStringAsync(url).GetAwaiter().GetResult();
        return ParseResponse(json);
    }

    static DmrUserData? ParseResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var results = doc.RootElement.GetProperty("results");

        if (results.GetArrayLength() == 0)
            return null;

        var u = results[0];

        return new DmrUserData
        {
            Id = u.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
            Callsign = u.TryGetProperty("callsign", out var cs) ? cs.GetString() : null,
            Name = u.TryGetProperty("name", out var n) ? n.GetString() : null,
            Surname = u.TryGetProperty("surname", out var sn) ? sn.GetString() : null,
            City = u.TryGetProperty("city", out var c) ? c.GetString() : null,
            State = u.TryGetProperty("state", out var st) ? st.GetString() : null,
            Country = u.TryGetProperty("country", out var co) ? co.GetString() : null,
            Remarks = u.TryGetProperty("remarks", out var r) ? r.GetString() : null,
        };
    }
}