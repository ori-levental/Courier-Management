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
    NotFound,
    Failed
}
public enum ScheduleStatus
{
    OnTime,
    InRisk,
    Late
}
public enum OrderType
{
    Express,
    Standard,
    Scheduled,
    SameDay
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