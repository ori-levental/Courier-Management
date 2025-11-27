namespace BlApi;
public interface IBl
{
    ICourier courier { get; }
    IOrder order { get; }
    IAdmin Admin { get; }
}
