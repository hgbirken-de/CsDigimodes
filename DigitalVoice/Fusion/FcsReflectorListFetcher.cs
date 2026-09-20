
namespace DigitalVoice.Fusion;

public class FcsReflector
{
    public string Id { get; set; } = "";
    public string Hostname { get; set; } = "";

    public override string ToString() => $"{Id} {Hostname}";
}

public class FcsReflectorListFetcher
{
    private const string FcsHostsUrl = "https://www.pistar.uk/downloads/FCS_Hosts.txt";

    public static async Task<List<FcsReflector>> FetchFcsReflectorsAsync()
    {
        using var httpClient = new HttpClient();
        var list = new List<FcsReflector>();

        try
        {
            string content = await httpClient.GetStringAsync(FcsHostsUrl);
            var lines = content.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Skip comments or empty lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                var parts = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    FcsReflector reflector = new()
                    {
                        Id = parts[0],
                        Hostname = parts[1],
                    };
                    list.Add(reflector);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch FCS reflectors: {ex.Message}");
        }

        return list;
    }
}
// nslookup 8.8.8.8

//using System;
//using System.Net;
//using System.Threading.Tasks;

//class Program
//{
//    static async Task Main()
//    {
//        string ipString = "8.8.8.8";
//        try
//        {
//            IPAddress ip = IPAddress.Parse(ipString);
//            IPHostEntry entry = await Dns.GetHostEntryAsync(ip);
//            Console.WriteLine($"Hostname for {ipString}: {entry.HostName}");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Failed to get hostname: {ex.Message}");
//        }
//    }
//}

