namespace DO;

/// <summary>
/// Record representing a Courier entity in the Data Object layer.
/// </summary>
/// <param name="Id">The unique identifier for the courier (e.g., ID number).</param>
/// <param name="FullName">The firsr and last name of the courier.</param>
/// <param name="PhoneNumber">The courier's phone number.</param>
/// <param name="Email">The courier's email address.</param>
/// <param name="password">The courier's login password.</param>
/// <param name="Active">Indicates whether the courier is currently active.</param>
/// <param name="DistanceToDelivery">The maximum distance the courier is willing to travel for a delivery,
///                                  defined from the company HQ to the customer. Nullable.</param>
/// /// <param name="DeliveryType">The type of delivery the courier handles (e.g., standard, express). Nullable.</param>
/// <param name="EmploymentStartDate">The date the courier started working. Nullable.</param>
public record Courier
(
    int Id,
    string FullName,
    string PhoneNumber,
    string Email,
    string password,
    bool Active,
    double? DistanceToDelivery,   // Nullable 
    Enums? DeliveryType,          // Nullable 
    DateTime? EmploymentStartDate // Nullable 
)
{
    /// <summary>
    /// Default constructor for Courier.
    /// Initializes a new instance with default values.
    /// </summary>
    // This constructor chains to the primary (record) constructor with default values.
    public Courier() : this(0, "", "", "", "", false, 0, null, null) { }
}
