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