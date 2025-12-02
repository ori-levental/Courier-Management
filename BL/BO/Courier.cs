using DO;
using Helpers;

namespace BO;

public class Courier
{
    public int Id { get; init; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public bool IsActive { get; set; }
    public double? DistanceToDelivery { get; set; }
    public ShippingType? DeliveryType { get; init; }
    public DateTime? EmploymentStartDate { get; init; }
    public int SumOrderInTime { get; init; }
    public int SumOrderInLate { get; init; }
    public OrderInProgress? OrderInCare { get; init; }

    public override string ToString() => this.ToStringProperty();
}
