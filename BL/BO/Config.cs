using Helpers;

namespace BO;

/// <summary>
/// Configuration object containing system-wide settings, operational parameters, and logical constraints.
/// </summary>
public class Config
{
    #region System Settings

    /// <summary>
    /// The maximum aerial distance (in km) a courier is allowed to travel from the hub.
    /// </summary>
    public double? MaxRange { get; set; }

    /// <summary>
    /// The current simulated time of the system.
    /// </summary>
    public DateTime Clock { get; set; }

    /// <summary>
    /// The password required for administrative access to the system.
    /// </summary>
    public required string ManagerPassword { get; set; }

    /// <summary>
    /// The ID required for administrative access to the system.
    /// </summary>
    public required int ManagerId { get; set; }

    /// <summary>
    /// The physical address of the company headquarters.
    /// </summary>
    public string? CompanyAddress { get; set; }

    #endregion System Settings

    #region Operational Parameters (Speeds)

    /// <summary>
    /// Average speed for car deliveries in km/h.
    /// </summary>
    public double AvgCarSpeed { get; set; }

    /// <summary>
    /// Average speed for motorcycle deliveries in km/h.
    /// </summary>
    public double AvgMotorcycleSpeed { get; set; }

    /// <summary>
    /// Average speed for bicycle deliveries in km/h.
    /// </summary>
    public double AvgBicycleSpeed { get; set; }

    /// <summary>
    /// Average walking speed for foot couriers in km/h.
    /// </summary>
    public double AvgWalkSpeed { get; set; }

    #endregion Operational Parameters (Speeds)

    #region Service Level Agreement (Time)

    /// <summary>
    /// The maximum allowed time duration for a standard delivery (SLA).
    /// </summary>
    public TimeSpan MaxDeliveryTime { get; set; }

    /// <summary>
    /// The time window before a deadline when an order is flagged as "In Risk".
    /// </summary>
    public TimeSpan RiskRange { get; set; }

    /// <summary>
    /// The duration of inactivity after which a courier might be flagged or reset.
    /// </summary>
    public TimeSpan CourierInactivityTime { get; set; }

    #endregion Service Level Agreement (Time)
    public override string ToString() => this.ToStringProperty();

}