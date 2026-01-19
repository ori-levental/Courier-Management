using System;
using System.Collections.Generic;
using Helpers;

namespace BO;

/// <summary>
/// Represents a full Order entity containing specifications, location data, calculated metrics, and delivery history.
/// </summary>
public class Order
{
    // --- IDENTIFIERS & CORE IMMUTABLES ---

    /// <summary>
    /// The unique identifier of the order.
    /// </summary>
    public int Id { get; init; }

    // --- SPECIFICATIONS & CUSTOMER DETAILS ---

    /// <summary>
    /// The urgency or priority level of the order.
    /// </summary>
    public OrderType OrderType { get; set; }

    /// <summary>
    /// Optional textual description or special instructions for the order.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The name of the customer placing the order.
    /// </summary>
    public required string OrderingName { get; set; }

    /// <summary>
    /// The contact phone number of the customer.
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Indicates whether the order contains heavy items (affects shipping logic).
    /// </summary>
    public bool IsHeavy { get; set; }

    // --- LOCATION & COORDINATES (Mutable for update) ---

    /// <summary>
    /// The full text address for delivery.
    /// </summary>
    public required string FullAddress { get; set; }

    /// <summary>
    /// The geographical latitude of the delivery address.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// The geographical longitude of the delivery address.
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Calculated aerial distance from the company headquarters/hub.
    /// </summary>
    public double AirDistance { get; set; }

    // --- TIME METRICS (Calculated & Immutable Status) ---

    /// <summary>
    /// The timestamp when the order was created.
    /// </summary>
    public DateTime StartOrderTime { get; init; }

    /// <summary>
    /// The predicted time of arrival based on current status and distance.
    /// </summary>
    public DateTime EstimatedArrivalTime { get; init; }

    /// <summary>
    /// The maximum allowable arrival time based on the Service Level Agreement (SLA).
    /// </summary>
    public DateTime MaxArrivalTime { get; init; }

    /// <summary>
    /// The current processing or completion status of the order.
    /// </summary>
    public OrderStatus OrderStatus { get; init; }

    /// <summary>
    /// Indicates if the order is on time, at risk, or late relative to the deadline.
    /// </summary>
    public ScheduleStatus ScheduleStatus { get; init; }

    /// <summary>
    /// The remaining time until the deadline (negative values indicate delay).
    /// </summary>
    public TimeSpan TimeRemaining { get; init; }

    // --- WORKFLOW & HISTORY ---

    /// <summary>
    /// A list of all delivery attempts associated with this order.
    /// </summary>
    public List<DeliveryPerOrderInList>? DeliveryHistory { get; init; }

    /// <summary>
    /// Returns a string representation of the entity properties.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}