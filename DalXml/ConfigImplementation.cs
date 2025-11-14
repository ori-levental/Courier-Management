using DalApi;

namespace Dal;

internal class ConfigImplementation : IConfig
{
    //  Manager order 
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }
    public int ManagerId
    {
        get => Config.ManagerId;
        set => Config.ManagerId = value;
    }
    public string ManagerPassword
    {
        get => Config.ManagerPassword;
        set => Config.ManagerPassword = value;
    }

    //  Distances 
    public string? CompanyAddress
    {
        get => Config.CompanyAddress;
        set => Config.CompanyAddress = value;
    }
    public double? Latitude
    {
        get => Config.Latitude;
        set => Config.Latitude = value;
    }
    public double? Longitude
    {
        get => Config.Longitude;
        set => Config.Longitude = value;
    }
    public double? MaxAirDistance
    {
        get => Config.MaxAirDistance;
        set => Config.MaxAirDistance = value;
    }

    //  Avrage speed 
    public double AvgCarSpeed
    {
        get => Config.AvgCarSpeed;
        set => Config.AvgCarSpeed = value;
    }
    public double AvgMotorcycleSpeed
    {
        get => Config.AvgMotorcycleSpeed;
        set => Config.AvgMotorcycleSpeed = value;
    }
    public double AvgBicycleSpeed
    {
        get => Config.AvgBicycleSpeed;
        set => Config.AvgBicycleSpeed = value;
    }
    public double AvgWalkSpeed
    {
        get => Config.AvgWalkSpeed;
        set => Config.AvgWalkSpeed = value;
    }

    //  Time range 
    public TimeSpan MaxDeliveryTime
    {
        get => Config.MaxDeliveryTime;
        set => Config.MaxDeliveryTime = value;
    }
    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }
    public TimeSpan CourierInactivityTime
    {
        get => Config.CourierInactivityTime;
        set => Config.CourierInactivityTime = value;
    }

    //  Method 
    public void Reset()
    {
        Config.Reset();
    }
}