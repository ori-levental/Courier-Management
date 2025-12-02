namespace BO;

public class CourierInList
{
    public int Id { get; init; }
    public required string FullName { get; init; }
    public bool IsActive { get; init; }
    public ShippingType DeliveryType { get; init; } 
    public DateTime? EmploymentStartDate { get; init; }
    public int SumOrderInTime { get; init; }
    public int SumOrderInLate { get; init; }
    public int? IdOrderInCare {  get; init; }
    public override string ToString() => this.ToStringProperty();

}
