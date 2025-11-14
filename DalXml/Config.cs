using System.Xml.Linq;

namespace Dal;

internal static class Config
{
    // --- XML File Definitions ---
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_couriers_xml = "couriers.xml";
    internal const string s_deliveries_xml = "deliveries.xml"; 
    internal const string s_orders_xml = "orders.xml";        
    // --- ID Generation ---
    internal static int NextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }
    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    // --- Manager and System ---
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }
    internal static int ManagerId
    {
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "ManagerId");
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "ManagerId", value);
    }
    internal static string ManagerPassword
    {
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "ManagerPassword");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "ManagerPassword", value);
    }
    // For treatment of nullable, I used Gemini - Prompt - the code of the method and the warning of the visual
    internal static string? CompanyAddress
    {
        get
        {
            XElement root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            return root.ToStringNullable("CompanyAddress");
        }
        set
        {
            XElement root = XMLTools.LoadListFromXMLElement(s_data_config_xml);
            root.Element("CompanyAddress")?.SetValue(value ?? string.Empty);
            XMLTools.SaveListToXMLElement(root, s_data_config_xml);
        }
    }
    internal static double? Latitude
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "Latitude");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "Latitude", value);
    }
    internal static double? Longitude
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "Longitude");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "Longitude", value);
    }
    internal static double? MaxAirDistance
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "MaxAirDistance");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "MaxAirDistance", value);
    }

    // --- Average Speeds ---
    internal static double AvgCarSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgCarSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgCarSpeed", value);
    }
    internal static double AvgMotorcycleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgMotorcycleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgMotorcycleSpeed", value);
    }
    internal static double AvgBicycleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgBicycleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgBicycleSpeed", value);
    }
    internal static double AvgWalkSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AvgWalkSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AvgWalkSpeed", value);
    }

    // --- Time Ranges ---
    internal static TimeSpan MaxDeliveryTime
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTime");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTime", value);
    }
    internal static TimeSpan RiskRange
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskRange", value);
    }
    internal static TimeSpan CourierInactivityTime
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "CourierInactivityTime");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "CourierInactivityTime", value);
    }

    // --- Methods ---
    internal static void Reset()
    {
        // Reset ID counters
        NextOrderId = 1;
        NextDeliveryId = 1;
        // Reset clock
        Clock = default(DateTime);
        // Reset credentials
        ManagerId = 0;
        ManagerPassword = "";
        // Resetting all numeric and location values to 0
        Latitude = Longitude = MaxAirDistance = AvgCarSpeed = AvgMotorcycleSpeed = AvgBicycleSpeed = AvgWalkSpeed = 0;
        // Resetting all TimeSpans to zero
        MaxDeliveryTime = RiskRange = CourierInactivityTime = default(TimeSpan);
    }
}