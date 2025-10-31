namespace Dal;

/// <summary>
/// Static class to hold the in-memory database (lists) for the DalList layer.
/// </summary>
internal static class DataSource
{
    /// <summary>
    /// List of all Couriers. nullable.
    /// </summary>
    internal static List<DO.Courier?> Couriers { get; } = new();

    /// <summary>
    /// List of all Deliveries. nullable.
    /// </summary>
    internal static List<DO.Delivery?> Deliveries { get; } = new();

    /// <summary>
    /// List of all Orders. nullable.
    /// </summary>
    internal static List<DO.Order?> Orders { get; } = new();
}