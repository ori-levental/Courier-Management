using DalApi;
using System;

namespace Dal;

/// <summary>
/// Implements the IConfig interface, acting as a public proxy for the internal static Config file reader/writer (DalXml).
/// </summary>
internal class ConfigImplementation : IConfig
{
    // --- SYSTEM DATA ---

    /// <summary>
    /// Gets or sets the simulated system clock value.
    /// </summary>
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    /// <summary>
    /// Gets or sets the Manager's ID.
    /// </summary>
    public int ManagerId
    {
        get => Config.ManagerId;
        set => Config.ManagerId = value;
    }

    /// <summary>
    /// Gets or sets the Manager's login password.
    /// </summary>
    public string ManagerPassword
    {
        get => Config.ManagerPassword;
        set => Config.ManagerPassword = value;
    }

    // --- LOCATION & DISTANCES ---

    /// <summary>
    /// Gets or sets the physical address of the company headquarters.
    /// </summary>
    public string? CompanyAddress
    {
        get => Config.CompanyAddress;
        set => Config.CompanyAddress = value;
    }

    /// <summary>
    /// Gets or sets the Latitude coordinate of the company headquarters.
    /// </summary>
    public double? Latitude
    {
        get => Config.Latitude;
        set => Config.Latitude = value;
    }

    /// <summary>
    /// Gets or sets the Longitude coordinate of the company headquarters.
    /// </summary>
    public double? Longitude
    {
        get => Config.Longitude;
        set => Config.Longitude = value;
    }

    /// <summary>
    /// Gets or sets the maximum allowed air distance for any delivery from the HQ.
    /// </summary>
    public double? MaxAirDistance
    {
        get => Config.MaxAirDistance;
        set => Config.MaxAirDistance = value;
    }

    // --- AVERAGE SPEEDS (Km/h) ---

    /// <summary>
    /// Gets or sets the average speed for car shipments.
    /// </summary>
    public double AvgCarSpeed
    {
        get => Config.AvgCarSpeed;
        set => Config.AvgCarSpeed = value;
    }

    /// <summary>
    /// Gets or sets the average speed for motorcycle shipments.
    /// </summary>
    public double AvgMotorcycleSpeed
    {
        get => Config.AvgMotorcycleSpeed;
        set => Config.AvgMotorcycleSpeed = value;
    }

    /// <summary>
    /// Gets or sets the average speed for bicycle shipments.
    /// </summary>
    public double AvgBicycleSpeed
    {
        get => Config.AvgBicycleSpeed;
        set => Config.AvgBicycleSpeed = value;
    }

    /// <summary>
    /// Gets or sets the average speed for walk shipments.
    /// </summary>
    public double AvgWalkSpeed
    {
        get => Config.AvgWalkSpeed;
        set => Config.AvgWalkSpeed = value;
    }

    // --- TIME RULES ---

    /// <summary>
    /// Gets or sets the maximum allowed time for a single delivery (SLA).
    /// </summary>
    public TimeSpan MaxDeliveryTime
    {
        get => Config.MaxDeliveryTime;
        set => Config.MaxDeliveryTime = value;
    }

    /// <summary>
    /// Gets or sets the time window used to flag at-risk deliveries.
    /// </summary>
    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }

    /// <summary>
    /// Gets or sets the maximum allowed time for courier inactivity.
    /// </summary>
    public TimeSpan CourierInactivityTime
    {
        get => Config.CourierInactivityTime;
        set => Config.CourierInactivityTime = value;
    }

    // --- METHOD ---

    /// <summary>
    /// Resets all configuration values to their initial default state in the XML file.
    /// </summary>
    public void Reset()
    {
        Config.Reset();
    }
}