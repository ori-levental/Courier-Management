using System;
using Helpers; 
namespace BO;

/// <summary>
/// Represents a lightweight Courier entity optimized for list views and summaries.
/// </summary>
public class DeliveryInList
{
    /// <summary>
    /// The unique identifier of the courier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The full name of the courier.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// Indicates whether the courier is currently active in the system.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// The type of vehicle or shipping method used by the courier.
    /// </summary>
    public ShippingType DeliveryType { get; init; }

    /// <summary>
    /// The date the courier began employment.
    /// </summary>
    public DateTime? EmploymentStartDate { get; init; }

    /// <summary>
    /// The total number of orders successfully delivered on time.
    /// </summary>
    public int SumOrderInTime { get; init; }

    /// <summary>
    /// The total number of orders delivered after the deadline.
    /// </summary>
    public int SumOrderInLate { get; init; }

    /// <summary>
    /// The ID of the order currently being handled by the courier (null if idle).
    /// </summary>
    public int? IdOrderInCare { get; init; }

    /// <summary>
    /// Returns a string representation of the entity using the helper tool.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}