using DalApi;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace Dal;

internal class ConfigImplementation : IConfig
{
    //  Manager order 
    public DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get => Config.Clock;
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set => Config.Clock = value;
    }
    public int ManagerId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.ManagerId;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.ManagerId = value;
    }
    public string ManagerPassword
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.ManagerPassword;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.ManagerPassword = value;
    }

    //  Distances 
    public string? CompanyAddress
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.CompanyAddress;
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set => Config.CompanyAddress = value;
    }
    public double? Latitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.Latitude;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.Latitude = value;
    }
    public double? Longitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.Longitude;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.Longitude = value;
    }
    public double? MaxAirDistance
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.MaxAirDistance;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.MaxAirDistance = value;
    }

    //  Avrage speed 
    public double AvgCarSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        get => Config.AvgCarSpeed;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.AvgCarSpeed = value;
    }
    public double AvgMotorcycleSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.AvgMotorcycleSpeed;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.AvgMotorcycleSpeed = value;
    }
    public double AvgBicycleSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.AvgBicycleSpeed;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.AvgBicycleSpeed = value;
    }
    public double AvgWalkSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.AvgWalkSpeed;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.AvgWalkSpeed = value;
    }

    //  Time range 
    public TimeSpan MaxDeliveryTime
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.MaxDeliveryTime;
        [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
        set => Config.MaxDeliveryTime = value;
    }
    public TimeSpan RiskRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.RiskRange;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.RiskRange = value;
    }
    public TimeSpan CourierInactivityTime
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.CourierInactivityTime;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.CourierInactivityTime = value;
    }

    //  Method 
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Reset()
    {
        Config.Reset();
    }
}