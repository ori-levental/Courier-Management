using System;
using Helpers;

namespace BO;

/// <summary>
/// Represents a specific delivery attempt within an order's history list.
/// </summary>
public class DeliveryPerOrderInList
{
    /// <summary>
    /// The unique identifier of the delivery.
    /// </summary>
    public int DeliveryId { get; init; }

    /// <summary>
    /// The unique identifier of the courier assigned (nullable).
    /// </summary>
    public int? CourierId { get; init; }

    /// <summary>
    /// The name of the courier assigned to the delivery.
    /// </summary>
    public required string CourierName { get; init; }

    /// <summary>
    /// The vehicle type used for this delivery.
    /// </summary>
    public ShippingType ShippingType { get; init; }

    /// <summary>
    /// The timestamp when the delivery started.
    /// </summary>
    public DateTime DeliveryStartTime { get; init; }

    /// <summary>
    /// The final outcome of the delivery (e.g., Delivered, Cancelled).
    /// </summary>
    public ShipmentCompletionStatus? DeliveryEndType { get; init; }

    /// <summary>
    /// The timestamp when the delivery ended.
    /// </summary>
    public DateTime? DeliveryEndTime { get; init; }

    /// <summary>
    /// Returns a string representation of the entity properties.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}