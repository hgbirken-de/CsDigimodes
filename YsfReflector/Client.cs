using System.Net;

namespace YsfReflector;

/// <summary>
/// Class that holds YSF client data.
/// </summary>
/// <param name="endPoint"></param>
/// <param name="callsign"></param>
public class Client(IPEndPoint endPoint, string callsign)
{
    //c = [
    //addr[0],               # 0 -> IP address
    //addr[1],               # 1 -> Port
    //data[4:14].decode().strip(),  # 2 -> Gateway callsign
    //0,                     # 3 -> t_corr (correction timer)
    //gw_id,                 # 4 -> Unique gateway ID
    //lonly,                 # 5 -> Listen-only flag
    //time.time(),           # 6 -> Time connected
    //locked,                # 7 -> Lock flag
    //dgid,                  # 8 -> DG-ID
    //t_hold,                # 9 -> DG-ID hold status
    //0,                     # 10 -> Busy flag
    //sock,                  # 11 -> Socket object (for multi-DGID support)
    //dgid_home,             # 12 -> Home DG-ID (if back-to-home feature active)
    //dgid_time,             # 13 -> Time to stay in home DG-ID
    //0                      # 14 -> Active timer / runtime counter
    //]

    public string Callsign { get; private set; } = callsign;

    public IPEndPoint EndPoint { get; private set; } = endPoint;

    public int DgId { get; set; } = 0;

    public int DgIdHome { get; set; } = 0;

    public int DgIdTimeMinutes { get; set; } = 0;

    public int GwId { get; set; } = 0;

    public DateTime LastHeard { get; set; } = DateTime.UtcNow; // TODO: ??? last YSF voice data frame

    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    public bool IsListenOnly { get; set; } = false;

    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Represents the client's transmit hold (THold) status, controlling how the DG-ID is used or fixed.
    /// <list type="bullet">
    ///   <item><description><see cref="THoldStatus.Dynamic"/> (0) — dynamic transmit, eligible for database or default DG-ID assignment.</description></item>
    ///   <item><description><see cref="THoldStatus.Static"/> (-1) — static transmit, DG-ID fixed from callsign or auxDgid.</description></item>
    ///   <item><description><see cref="THoldStatus.FixedHome"/> (-2) — fixed home transmit, permanent DG-ID overriding other assignments.</description></item>
    /// </list>
    /// Default is <see cref="THoldStatus.Dynamic"/>.
    /// </summary>
    public THoldStatus THold { get; set; } = THoldStatus.Dynamic;

    public bool IsBusy { get; set; } = false;

    public bool IsActive => (DateTime.UtcNow - LastSeen).TotalSeconds < 30;

    public bool HasBackToHome => DgIdHome > 0 && DgIdTimeMinutes > 0;

    public override string ToString()
    {
        return $"{EndPoint} {Callsign}";
    }
}