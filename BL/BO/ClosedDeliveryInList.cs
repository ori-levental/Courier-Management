using System;
using Helpers; 
namespace BO;

/// <summary>
/// Represents a summary entity of a completed delivery, optimized for list views and history.
/// </summary>
public class ClosedDeliveryInList
{
    // --- IDENTIFIERS & LINKS ---

    /// <summary>
    /// The unique identifier of the delivery operation.
    /// </summary>
    public int DeliveryId { get; init; }

    /// <summary>
    /// The unique identifier of the associated order.
    /// </summary>
    public int OrderId { get; init; }

    // --- STATIC ORDER INFO ---

    /// <summary>
    /// The urgency or priority classification of the order.
    /// </summary>
    public OrderType OrderType { get; init; }

    /// <summary>
    /// The full destination address for the delivery.
    /// </summary>
    public required string FullAddress { get; init; }

    // --- DELIVERY METRICS (From DO.Delivery) ---

    /// <summary>
    /// The type of vehicle utilized for this delivery.
    /// </summary>
    public ShippingType ShippingType { get; init; }

    /// <summary>
    /// The actual distance traveled during the delivery (in Kilometers).
    /// </summary>
    public double? ActualDistanceKm { get; init; }

    // --- STATUS AND TIMES ---

    /// <summary>
    /// The total duration calculated from the start of the delivery until completion.
    /// </summary>
    public TimeSpan TotalProcessingTime { get; init; }

    /// <summary>
    /// The final outcome status of the delivery (e.g., Provided, Delivered).
    /// </summary>
    public ShipmentCompletionStatus? DeliveryEndType { get; init; }

    /// <summary>
    /// The specific timestamp when the delivery was completed.
    /// </summary>
    public DateTime? DeliveryEndTime { get; init; }

    /// <summary>
    /// Returns a string representation of the entity using the helper tool.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}