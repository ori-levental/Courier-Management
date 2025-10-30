using System;

namespace DO;

/// <summary>
/// Record representing a Delivery entity in the Data Object layer.
/// This links an Order to a Courier and tracks its progress.
/// </summary>
/// <param name="Id">The unique identifier for the delivery itself.</param>
/// <param name="OrderId">The ID of the order being delivered.</param>
/// <param name="CourierId">The ID of the courier assigned to this delivery.</param>
/// <param name="DeliveryType">The type of delivery, based on the Enums class.</param>
/// <param name="StartOrderTime">The date and time when the order was picked up by the courier.</param>
/// <param name="Distance">The total distance of the delivery route, calculated based on the courier's mode of transport. Nullable.</param>
/// <param name="EndType">The type or status of the delivery's completion (e.g., delivered, returned). Nullable.</param>
/// <param name="EndOrderTime">The date and time when the delivery was completed (or finalized). Nullable.</param>
public record Delivery
(
    int Id,
    int OrderId,
    int CourierId,
    Enums DeliveryType,
    DateTime StartOrderTime,
    double? Distance,        // Nullable
    Enums? EndType,          // Nullable
    DateTime? EndOrderTime   // Nullable
)
{
    /// <summary>
    /// Default constructor for Delivery.
    /// Initializes a new instance with default values.
    /// </summary>
    public Delivery() : this(0, 0, 0, new Enums(), default(DateTime), 0, null, null) { }
}