namespace BO;

public class OrderInList
{
    public int? DeliveryId { get; init; }
    public int OrderId { get; init; }
    public OrderType OrderType { get; init; }
    public double AirDistance { get; init; }
    public ShipmentCompletionStatus OrderStatus { get; init; }
    public ScheduleStatus ScheduleStatus { get; init; }
    public TimeSpan TimeRemaining { get; init; }
    public TimeSpan TotalProcessingTime { get; init; }
    public int TotalDeliveries { get; init; }
    public override string ToString() => this.ToStringProperty();

}