namespace BO;

public class OrderInProgress // Represents a delivery currently being handled by a courier.
{
    // --- Identifiers & Links (From DO.Delivery and DO.Order) ---
    public int DeliveryId { get; init; }
    public int OrderId { get; init; }
    public OrderType OrderType { get; init; }

    // --- Customer & Location Details (From DO.Order) ---
    public string CustomerName { get; init; }
    public string CustomerPhone { get; init; }
    public string Address { get; init; }
    public string? Description { get; init; }

    // --- Distances and Measured Data ---
    public double AirDistance { get; init; }     // Calculated: Air distance from HQ to Address
    public double? Distance { get; init; }       // Actual distance traveled (Road/Walk)

    // --- Time Metrics ---
    public DateTime StartOrderTime { get; init; }     // Time order was created (DO.Order)
    public DateTime StartDeliveryTime { get; init; }  // Time courier started trip (DO.Delivery)

    // --- Calculated Status and Times ---
    public DateTime EstimatedArrivalTime { get; init; }   // Calculated based on current speed/distance
    public DateTime MaxArrivalTime { get; init; }         // Calculated based on OrderTime + SLA Max
    public TimeSpan TimeLeft { get; init; }             // Calculated: MaxArrivalTime - Clock
    public TimeSpan TimeToComplete { get; init; }       // Calculated: Difference between start and expected end (used for BO.ClosedDeliveryInList logic)

    // --- Status Enums ---
    public ScheduleStatus ScheduleStatus { get; init; } // Calculated schedule status

    // --- To string ---
    public override string ToString() => this.ToStringProperty();
}