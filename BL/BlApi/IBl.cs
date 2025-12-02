namespace BlApi;

/// <summary>
/// Main Business Logic Interface.
/// Serves as the entry point for all BL operations, exposing specific functionality through sub-interfaces.
/// </summary>
public interface IBl
{
    /// <summary>
    /// Access to Courier management operations (CRUD, Tracking, Lists).
    /// </summary>
    ICourier Courier { get; }

    /// <summary>
    /// Access to Order management operations (CRUD, Status updates, Calculations).
    /// </summary>
    IOrder Order { get; }

    /// <summary>
    /// Access to System Administration operations (Config, Clock, Reset).
    /// </summary>
    IAdmin Admin { get; }
}