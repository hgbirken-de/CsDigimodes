namespace YsfReflector;

/// <summary>
/// Represents the transmit hold (THold) status of a YSF client, controlling how the DG-ID
/// is used, fixed, or overridden.
/// </summary>
public enum THoldStatus
{
    /// <summary>
    /// Dynamic transmit: the client's DG-ID is unassigned or dynamically determined,
    /// eligible for database or default assignment.
    /// </summary>
    Dynamic = 0,

    /// <summary>
    /// Static transmit: the client's DG-ID is fixed from callsign suffix or auxiliary assignment,
    /// cannot be automatically changed.
    /// </summary>
    Static = -1,

    /// <summary>
    /// Fixed home transmit: the client's DG-ID is permanent and overrides dynamic or static assignments.
    /// </summary>
    FixedHome = -2
}
