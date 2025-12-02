using System;
using Helpers;

namespace BO;

/// <summary>
/// Represents a summary of an order entity, optimized for list displays and grids.
/// Contains calculated metrics and current status.
/// </summary>
public class OrderInList
{
    /// <summary>
    /// The identifier of the current or latest associated delivery (nullable).
    /// </summary>
    public int? DeliveryId { get; init; }

    /// <summary>
    /// The unique identifier of the order.
    /// </summary>
    public int OrderId { get; init; }

    /// <summary>
    /// The urgency or priority classification of the order.
    /// </summary>
    public OrderType OrderType { get; init; }

    /// <summary>
    /// Calculated aerial distance from the hub/source to the destination.
    /// </summary>
    public double AirDistance { get; init; }

    /// <summary>
    /// The current processing or completion status of the order.
    /// </summary>
    public ShipmentCompletionStatus OrderStatus { get; init; }

    /// <summary>
    /// Indicates if the order is on time, at risk, or late relative to the deadline.
    /// </summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>
    /// The remaining time until the delivery deadline.
    /// </summary>
    public TimeSpan TimeRemaining { get; init; }

    /// <summary>
    /// The total duration the order has been in processing.
    /// </summary>
    public TimeSpan TotalProcessingTime { get; init; }

    /// <summary>
    /// The total count of delivery attempts made for this order.
    /// </summary>
    public int TotalDeliveries { get; init; }

    /// <summary>
    /// Returns a string representation of the entity properties.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}