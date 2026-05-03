using System;
using Helpers;

namespace BO;

/// <summary>
/// Represents a snapshot of a delivery currently being executed by a courier.
/// Contains real-time calculations regarding location, time, and schedule adherence.
/// </summary>
public class OrderInProgress
{
    // --- Identifiers & Links ---

    /// <summary>
    /// The unique identifier of the active delivery.
    /// </summary>
    public int DeliveryId { get; init; }

    /// <summary>
    /// The unique identifier of the order being delivered.
    /// </summary>
    public int OrderId { get; init; }

    /// <summary>
    /// The classification of the order (e.g., Express, Standard).
    /// </summary>
    public OrderType OrderType { get; init; }

    // --- Customer & Location Details ---

    /// <summary>
    /// The name of the customer receiving the order.
    /// </summary>
    public required string CustomerName { get; init; }

    /// <summary>
    /// The contact phone number of the customer.
    /// </summary>
    public required string CustomerPhone { get; init; }

    /// <summary>
    /// The delivery destination address.
    /// </summary>
    public required string Address { get; init; }

    /// <summary>
    /// Additional notes or instructions regarding the order.
    /// </summary>
    public string? Description { get; init; }

    // --- Distances ---

    /// <summary>
    /// Calculated aerial distance from the source (Hub) to the destination.
    /// </summary>
    public double AirDistance { get; init; }

    /// <summary>
    /// The actual distance covered or planned for the delivery route (nullable).
    /// </summary>
    public double? Distance { get; init; }

    // --- Time Metrics ---

    /// <summary>
    /// The timestamp when the order was originally placed.
    /// </summary>
    public DateTime StartOrderTime { get; init; }

    /// <summary>
    /// The timestamp when the courier began the delivery process.
    /// </summary>
    public DateTime StartDeliveryTime { get; init; }

    // --- Calculated Status and Times ---

    /// <summary>
    /// The predicted arrival time based on distance and vehicle speed.
    /// </summary>
    public DateTime EstimatedArrivalTime { get; init; }

    /// <summary>
    /// The deadline for the delivery based on the Service Level Agreement (SLA).
    /// </summary>
    public DateTime MaxArrivalTime { get; init; }

    /// <summary>
    /// The time remaining until the delivery deadline (calculated against current clock).
    /// </summary>
    public TimeSpan TimeLeft { get; init; }

    /// <summary>
    /// The estimated total duration required to complete the delivery from start to finish.
    /// </summary>
    public TimeSpan TimeToComplete { get; init; }

    // --- Status Enums ---

    /// <summary>
    /// The current schedule status (OnTime, Late, etc.) relative to the deadline.
    /// </summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>
    /// Returns a string representation of the entity using the helper tool.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}