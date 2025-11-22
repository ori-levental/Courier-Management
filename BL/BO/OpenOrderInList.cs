namespace BO;

public class OpenOrderInList
{
    public int OrderId { get; init; }
    public int? CourierId { get; init; }
    public OrderType OrderType { get; init; }
    public bool IsHeavy { get; init; }
    public string FullAddress { get; init; }
    public double AirDistance { get; init; }
    public double? ActualDistance { get; init; }
    public TimeSpan? ActualTimeEstimation { get; init; }
    public DateTime MaxArrivalTime { get; init; }
    public TimeSpan TimeRemaining { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
}

