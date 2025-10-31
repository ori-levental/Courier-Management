namespace Dal;

/// <summary>
/// Static internal class holding global configuration values and state
/// for the Data Access Layer (Dal).
/// Note: 'internal' means it's accessible only within this project (assembly).
/// </summary>
static internal class Config
{
    // --- Order ID Generation ---
    internal const int startOrderId = 1;
    private static int s_nextOrderId = startOrderId;
    /// <summary>
    /// Gets the next sequential Order ID and increments the counter.
    /// </summary>
    internal static int NextOrderId { get => s_nextOrderId++; }
    internal static int MaxRange { get; set; } = 4;

    // --- Delivery ID Generation ---
    internal const int startDeliveryId = 1;
    private static int s_nextDeliveryId = startDeliveryId;
    /// <summary>
    /// Gets the next sequential Delivery ID and increments the counter.
    /// </summary>
    internal static int NextDeliveryType { get => s_nextDeliveryId++; }

    // --- System Simulated Clock ---
    /// <summary>
    /// A static clock, likely used for simulating time in the application.
    /// </summary>
    internal static DateTime Clock;

    // --- Manager Credentials ---
    internal static int ManagerId;
    internal static int ManagerPassword;

    // --- Company Location Details ---
    /// <summary>
    /// The main address of the company (HQ).
    /// </summary>
    internal static string? CompanyAddress;
    /// <summary>
    /// Latitude coordinate of the company's address.
    /// </summary>
    internal static Double? Latitude;
    /// <summary>
    /// Longitude coordinate of the company's address.
    /// </summary>
    internal static Double? Longitude;

    // --- Business Logic Constants ---
    /// <summary>
    /// Maximum allowed air distance (direct line) for a delivery.
    /// </summary>
    internal static double? MaxAirDistance;

    // Average speeds for different transport modes (for ETA calculations)
    internal static Double AvgCarSpeed;
    internal static Double AvgMotorcycleSpeed;
    internal static Double AvgBicycleSpeed;
    internal static Double AvgWalkSpeed;

    // Time-based rules
    /// <summary>
    /// Max allowed time for a delivery (SLA - Service Level Agreement).
    /// </summary>
    internal static TimeSpan MaxDeliveryTime;
    /// <summary>
    /// A time window before the deadline to flag at-risk deliveries.
    /// </summary>
    internal static TimeSpan RiskRange;
    /// <summary>
    /// Maximum time allowed for a courier to be inactive before being flagged.
    /// </summary>
    internal static TimeSpan CourierInactivityTime;

    /// <summary>
    /// Resets all static configuration values to their default state.
    /// Useful for re-initializing the system or for unit testing.
    /// </summary>
    internal static void Reset()
    {
        // Reset ID counters
        s_nextOrderId = startOrderId;
        s_nextDeliveryId = startDeliveryId;
        // Reset clock
        Clock = default(DateTime);
        // Reset credentials
        ManagerId = ManagerPassword = 0;
        // Resetting all numeric and location values to 0
        Latitude = Longitude = MaxAirDistance = AvgCarSpeed = AvgMotorcycleSpeed = AvgBicycleSpeed = AvgWalkSpeed = 0;
        // Resetting all TimeSpans to zero
        MaxDeliveryTime = RiskRange = CourierInactivityTime = default(TimeSpan);
    }

}