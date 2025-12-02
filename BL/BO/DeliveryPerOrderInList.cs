namespace BO;

public class DeliveryPerOrderInList
{
    public int DeliveryId { get; init; }
    public int? CourierId { get; init; } 
    public required string CourierName { get; init; }
    public ShippingType ShippingType { get; init; }
    public DateTime DeliveryStartTime { get; init; }
    public ShipmentCompletionStatus? DeliveryEndType { get; init; }
    public DateTime? DeliveryEndTime { get; init; }
    public override string ToString() => this.ToStringProperty();


}
