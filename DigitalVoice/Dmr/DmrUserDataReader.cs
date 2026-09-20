
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
/// Provides a helper method to retrieve DMR user information from the <c>https://radioid.net</c> API.
/// </summary>
public static class DmrUserDataReader
{
    static readonly HttpClient http = new();

    /// <summary>
    /// Retrieves user information from the radioid.net API for a given DMR ID.
    /// </summary>
    /// <param name="dmrId">The DMR user ID to look up.</param>
    /// <returns>A <see cref="DmrUserData"/> instance with the user data if found; otherwise <c>null</c>.</returns>
    /// <remarks>
    /// <para>
    /// This method calls <c>https://radioid.net/api/dmr/user/?id={dmrId}</c> and parses the JSON response into a <see cref="DmrUserData"/> object.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// var user = await DmrUserInfoHelper.GetUserAsync(2622363);
    /// if (user != null)
    ///     Console.WriteLine($"{user.Callsign} ({user.Fname} {user.Surname}), {user.City}, {user.Country}");
    /// </code>
    /// </para>
    /// </remarks>
    public static async Task<DmrUserData?> GetUserAsync(int dmrId)
    {
        string url = $"https://radioid.net/api/dmr/user/?id={dmrId}";

        var json = await http.GetStringAsync(url);
        using var doc = JsonDocument.Parse(json);

        var results = doc.RootElement.GetProperty("results");
        if (results.GetArrayLength() == 0)
            return null;

        var u = results[0];
        return new DmrUserData
        {
            Id = u.GetProperty("id").GetInt32(),
            Callsign = u.GetProperty("callsign").GetString(),
            Name = u.GetProperty("name").GetString(),
            Surname = u.GetProperty("surname").GetString(),
            City = u.GetProperty("city").GetString(),
            State = u.GetProperty("state").GetString(),
            Country = u.GetProperty("country").GetString(),
        };
    }

    public static DmrUserData? GetUser(int dmrId)
    {
        using var http = new HttpClient();
        var url = $"https://radioid.net/api/dmr/user/?id={dmrId}";

        var response = http.GetAsync(url).GetAwaiter().GetResult(); // blocks here
        response.EnsureSuccessStatusCode();

        var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var doc = JsonDocument.Parse(json);
        var result = doc.RootElement.GetProperty("results")[0];

        return new DmrUserData
        {
            Id = result.GetProperty("id").GetInt32(),
            Callsign = result.GetProperty("callsign").GetString(),
            Name = result.GetProperty("fname").GetString(),
            Surname = result.GetProperty("surname").GetString(),
            City = result.GetProperty("city").GetString(),
            State = result.GetProperty("state").GetString(),
            Country = result.GetProperty("country").GetString(),
        };
    }

}