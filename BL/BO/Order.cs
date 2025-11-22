namespace BO;

public class Order
{
    // --- IDENTIFIERS & CORE IMMUTABLES ---
    public int Id { get; init; }

    // --- SPECIFICATIONS & CUSTOMER DETAILS ---
    public OrderType OrderType { get; set; }
    public string? Description { get; set; }
    public string OrderingName { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsHeavy { get; set; } // Load attribute example

    // --- LOCATION & COORDINATES (Mutable for update) ---
    public string FullAddress { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double AirDistance { get; set; } // Air distance from HQ

    // --- TIME METRICS (Calculated & Immutable Status) ---
    public DateTime StartOrderTime { get; init; }
    public DateTime EstimatedArrivalTime { get; init; }
    public DateTime MaxArrivalTime { get; init; }
    public ShipmentCompletionStatus OrderStatus { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TimeRemaining { get; init; }

    // --- WORKFLOW & HISTORY ---
    public List<DeliveryPerOrderInList>? DeliveryHistory { get; init; } // List of linked deliveries
}