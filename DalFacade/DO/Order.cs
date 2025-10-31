namespace DO;


/// <summary>
/// Record representing an Order entity in the Data Object layer.
/// </summary>
/// <param name="Id">The unique identifier for the order.</param>
/// <param name="Type">The type of order. Based on the Enums class.</param>
/// <param name="Description">An optional description for the order. Nullable.</param>
/// <param name="Addres">The delivery address for the order.</param>
/// <param name="Latitude">The geographic latitude of the delivery address.</param>
/// <param name="Longitude">The geographic longitude of the delivery address.</param>
/// <param name="OrderingName">The name of the customer who placed the order.</param>
/// <param name="phoneNumber">The phone number of the customer.</param>
/// <param name="StartOrderTime">The date and time when the order was placed.</param>
public record Order
(
    int Id,
    Enums.OrderType Type,
    string? Description,    // Nullable
    string Addres,
    double Latitude,
    double Longitude,
    string OrderingName,
    string phoneNumber,
    DateTime StartOrderTime
)
{
    /// <summary>
    /// Default constructor for Order.
    /// Initializes a new instance with default values.
    /// </summary>
    public Order() : this(0, new Enums.OrderType(), "", "", 0, 0, "", "", default(DateTime)) { }
}
