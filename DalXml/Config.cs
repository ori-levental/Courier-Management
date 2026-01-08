using System.Xml.Linq;

namespace Dal;

/// <summary>
/// Static class holding global configuration values and state for the DalXml project.
/// All properties read from and write to the external data-config.xml file via XMLTools.
/// </summary>
internal static class Config
{
    // --- XML File Definitions ---
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_couriers_xml = "couriers.xml";
    internal const string s_deliveries_xml = "deliveries.xml";
    internal const string s_orders_xml = "orders.xml";

    #region ID Generation
    /// <summary>
    /// Gets the next sequential Order ID from the config file and automatically increments it.
    /// </summary>
    internal static int NextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }
    /// <summary>
    /// Gets the next sequential Delivery ID from the config file and automatically increments it.
    /// </summary>
    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }
    #endregion

    #region Manager and System
    /// <summary>
    /// Gets or sets the simulated system clock (DateTime).
    /// </summary>
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }
    /// <summary>
    /// Gets or sets the Manager's login ID.
    /// </summary>
    internal static int ManagerId
    {
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "ManagerId");
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "ManagerId", value);
    }
    /// <summary>
    /// Gets or sets the Manager's login password.
    /// </summary>
    internal static string ManagerPassword
    {
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "ManagerPassword");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "ManagerPassword", value);
    }
    #endregion

    #region Distances and Location
    /// <summary>
    /// Gets or sets the Company's headquarters address (nullable string).
    /// </summary>
    internal static string? CompanyAddress
    {
        get
        {
            // Uses ToStringNullable to safely return null if the XML element is missing.
            XElement root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            return root.ToStringNullable("CompanyAddress");
        }
        set
        {
            // Must handle null explicitly for XElement.SetValue
            XElement root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("CompanyAddress")?.SetValue(value ?? string.Empty);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }
    /// <summary>
    /// Gets or sets the Headquarters Latitude coordinate (nullable double).
    /// </summary>
    internal static double? Latitude
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "Latitude");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "Latitude", value);
    }
    /// <summary>
    /// Gets or sets the Headquarters Longitude coordinate (nullable double).
    /// </summary>
    internal static double? Longitude
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "Longitude");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "Longitude", value);
    }
    /// <summary>
    /// Gets or sets the maximum allowed air distance for deliveries (nullable double).
    /// </summary>
    internal static double? MaxAirDistance
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "MaxAirDistance");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "MaxAirDistance", value);
    }
    #endregion

    #region Average Speeds
    /// <summary>
    /// Gets or sets the average speed for car shipments (km/h).
    /// </summary>
    internal static double AvgCarSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgCarSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgCarSpeed", value);
    }
    /// <summary>
    /// Gets or sets the average speed for motorcycle shipments (km/h).
    /// </summary>
    internal static double AvgMotorcycleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgMotorcycleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgMotorcycleSpeed", value);
    }
    /// <summary>
    /// Gets or sets the average speed for bicycle shipments (km/h).
    /// </summary>
    internal static double AvgBicycleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgBicycleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgBicycleSpeed", value);
    }
    /// <summary>
    /// Gets or sets the average speed for walk shipments (km/h).
    /// </summary>
    internal static double AvgWalkSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgWalkSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgWalkSpeed", value);
    }
    #endregion

    #region Time Ranges
    /// <summary>
    /// Gets or sets the maximum allowed time for a single delivery (TimeSpan).
    /// </summary>
    internal static TimeSpan MaxDeliveryTime
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTime");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTime", value);
    }
    /// <summary>
    /// Gets or sets the time window used to flag at-risk deliveries (TimeSpan).
    /// </summary>
    internal static TimeSpan RiskRange
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskRange", value);
    }
    /// <summary>
    /// Gets or sets the maximum time allowed for courier inactivity (TimeSpan).
    /// </summary>
    internal static TimeSpan CourierInactivityTime
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "CourierInactivityTime");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "CourierInactivityTime", value);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Resets all configuration values to their hardcoded initial default state.
    /// </summary>
    internal static void Reset()
    {
        // Reset ID counters
        NextOrderId = 1;
        NextDeliveryId = 1;
        // Reset clock
        Clock = default(DateTime).AddYears(1999);
        // Reset credentials
        ManagerId = 0;
        CompanyAddress = ManagerPassword = "";
        // Resetting all numeric and location values to 0
        Latitude = Longitude = MaxAirDistance = AvgCarSpeed = AvgMotorcycleSpeed = AvgBicycleSpeed = AvgWalkSpeed = 1;
        // Resetting all TimeSpans to zero
        MaxDeliveryTime = RiskRange = CourierInactivityTime = default(TimeSpan);
    }
    #endregion
}