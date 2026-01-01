namespace BO;
public enum ShippingType
{
    Car,
    Motorcycle,
    Bicycle,
    Walk
}
public enum ShipmentCompletionStatus
{
    Provided,
    Refused,
    Cancelled,
    Open,
    OnCare
}
public enum ScheduleStatus
{
    OnTime,
    InRisk,
    Late
}
public enum OrderType
{
    Private,
    Business,
    Wholesale,
    PublicSector
}
public enum EmployType
{
    Manager,
    Courier
}

public enum CourierInListEnum
{
    Id,
    FullName,
    IsActive,
    DeliveryType,
    EmploymentStartDate,
    SumOrderInTime,
    SumOrderInLate,
    IdOrderInCare
}

public enum OrderInListEnum
{
    DeliveryId,
    OrderId,
    OrderType,
    AirDistance,
    OrderStatus,
    ScheduleStatus,
    TimeRemaining,
    TotalProcessingTime,
    TotalDeliveries
}

/// <summary>
/// Enumerates the properties of an OpenOrderInList object available for sorting or filtering.
/// </summary>
public enum OpenOrderInListEnum
{
    OrderId,
    CourierId,
    OrderType,
    IsHeavy,
    FullAddress,
    AirDistance,
    ActualDistance,
    ActualTimeEstimation,
    MaxArrivalTime,
    TimeRemaining,
    ScheduleStatus
}
public enum ClosedDeliveryInListEnum
{
    DeliveryId,
    OrderId,
    OrderType,
    FullAddress,
    ShippingType,
    ActualDistanceKm,
    TotalProcessingTime,
    DeliveryEndType,
    DeliveryEndTime
}

public enum TimeUnit
{
    Minute,
    Hour,
    Day,
    Month,
    Year
}