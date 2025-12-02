using System;
using DO;
using Helpers;

namespace BO;

/// <summary>
/// Represents a Courier entity containing personal details, operational status, and performance statistics.
/// </summary>
public class Courier
{
    /// <summary>
    /// The unique identifier of the courier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The full name of the courier.
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// The courier's contact phone number.
    /// </summary>
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// The courier's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// The password used for system authentication.
    /// </summary>
    public required string Password { get; set; }

    /// <summary>
    /// Indicates whether the courier is currently active in the system.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The maximum air distance the courier is willing to travel for a delivery.
    /// </summary>
    public double? DistanceToDelivery { get; set; }

    /// <summary>
    /// The type of vehicle or shipping method used by the courier.
    /// </summary>
    public ShippingType? DeliveryType { get; init; }

    /// <summary>
    /// The date the courier started working at the company.
    /// </summary>
    public DateTime? EmploymentStartDate { get; init; }

    /// <summary>
    /// The total count of orders successfully delivered on time.
    /// </summary>
    public int SumOrderInTime { get; init; }

    /// <summary>
    /// The total count of orders delivered with a delay.
    /// </summary>
    public int SumOrderInLate { get; init; }

    /// <summary>
    /// Details of the order currently being handled by the courier (null if idle).
    /// </summary>
    public OrderInProgress? OrderInCare { get; init; }

    /// <summary>
    /// Returns a string representation of the entity using the helper tool.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}