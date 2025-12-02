using System;
using Helpers;

namespace BO;

/// <summary>
/// Represents an active order available for selection or processing, optimized for list views.
/// Includes calculated metrics like distance and time remaining.
/// </summary>
public class OpenOrderInList
{
    /// <summary>
    /// The unique identifier of the order.
    /// </summary>
    public int OrderId { get; init; }

    /// <summary>
    /// The identifier of the assigned courier, if one has already been assigned.
    /// </summary>
    public int? CourierId { get; init; }

    /// <summary>
    /// The urgency or service level of the order.
    /// </summary>
    public OrderType OrderType { get; init; }

    /// <summary>
    /// Indicates if the order involves heavy items.
    /// </summary>
    public bool IsHeavy { get; init; }

    /// <summary>
    /// The full delivery destination address.
    /// </summary>
    public required string FullAddress { get; init; }

    /// <summary>
    /// Calculated aerial distance from the reference point (Hub or Courier location).
    /// </summary>
    public double AirDistance { get; init; }

    /// <summary>
    /// Actual road distance to the destination (nullable if calculation unavailable).
    /// </summary>
    public double? ActualDistance { get; init; }

    /// <summary>
    /// Estimated time required to reach the destination.
    /// </summary>
    public TimeSpan? ActualTimeEstimation { get; init; }

    /// <summary>
    /// The deadline by which the order must be delivered.
    /// </summary>
    public DateTime MaxArrivalTime { get; init; }

    /// <summary>
    /// The remaining time until the delivery deadline.
    /// </summary>
    public TimeSpan TimeRemaining { get; init; }

    /// <summary>
    /// The current timeliness status (e.g., OnTime, Late).
    /// </summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>
    /// Returns a string representation of the entity properties.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}