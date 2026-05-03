namespace DalApi;
public interface IConfig
{
    // Manager order
    DateTime Clock { get; set; }
    int ManagerId { get; set; }
    string ManagerPassword { get; set; }

    // Distances
    string? CompanyAddress { get; set; }
    double? Latitude { get; set; }
    double? Longitude { get; set; }
    double? MaxAirDistance { get; set; }

    // Avrage speed
    double AvgCarSpeed { get; set; }
    double AvgMotorcycleSpeed { get; set; }
    double AvgBicycleSpeed { get; set; }
    double AvgWalkSpeed { get; set; }

    // Time range
    TimeSpan MaxDeliveryTime { get; set; }
    TimeSpan RiskRange { get; set; }
    TimeSpan CourierInactivityTime { get; set; }

    // Method
    void Reset();

}
