namespace BO;

public class ClosedDeliveryInList
{
    // --- IDENTIFIERS & LINKS ---
    public int DeliveryId { get; init; } // From DO.Delivery
    public int OrderId { get; init; }    // From DO.Delivery

    // --- STATIC ORDER INFO ---
    public OrderType OrderType { get; init; }
    public required string FullAddress { get; init; } // From DO.Order

    // --- DELIVERY METRICS (From DO.Delivery) ---
    public ShippingType ShippingType { get; init; }
    public double? ActualDistanceKm { get; init; } // From DO.Delivery (can be null if not tracked/not relevant)

    // --- STATUS AND TIMES ---
    public TimeSpan TotalProcessingTime { get; init; } // Calculated: DeliveryEndTime - DeliveryStartTime
    public ShipmentCompletionStatus? DeliveryEndType { get; init; } // From DO.Delivery (can be null if status is not final or not relevant)
    public DateTime? DeliveryEndTime { get; init; } // From DO.Delivery
}