using BO;

namespace BlApi;
/// <summary>
/// Defines the API for managing courier entities within the Business Logic (BL) layer.
/// </summary>
public interface ICourier : IObservable
{
    /// <summary>
    /// Attempts to log a user (e.g., Courier or Admin) into the system.
    /// </summary>
    /// <param name="id">The identification number of the user.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>The type of user successfully logged in.</returns>
    public EmployType EnterToSystem(int id, string password);

    /// <summary>
    /// Retrieves a list of couriers based on specified criteria.
    /// </summary>
    /// <param name="requesterId">ID of the user requesting the list (for authorization).</param>
    /// <param name="isActive">Optional filter to include only active or inactive couriers.</param>
    /// <param name="SortBy">Optional field to sort the list by.</param>
    /// <returns>An enumerable collection of simplified courier objects.</returns>
    public IEnumerable<BO.DeliveryInList> ListOfCourier(int requesterId, bool? isActive, CourierInListEnum? SortBy);

    /// <summary>
    /// Retrieves detailed information for a specific courier.
    /// </summary>
    /// <param name="requesterId">ID of the user requesting the information (for authorization).</param>
    /// <param name="CourierId">The ID of the courier to retrieve.</param>
    /// <returns>The detailed BO.Courier object.</returns>
    public BO.Courier SearchCourier(int requesterId, int CourierId);

    /// <summary>
    /// Updates the details of an existing courier.
    /// </summary>
    /// <param name="requesterId">ID of the user performing the update (for authorization).</param>
    /// <param name="courier">The BO.Courier object containing the updated data.</param>
    public void UpdateCourier(int requesterId, BO.Courier courier);

    /// <summary>
    /// Deletes a courier from the system.
    /// </summary>
    /// <param name="requesterId">ID of the user performing the deletion (for authorization).</param>
    /// <param name="CourierId">The ID of the courier to delete.</param>
    public void DeleteCourier(int requesterId, int CourierId);

    /// <summary>
    /// Adds a new courier to the system.
    /// </summary>
    /// <param name="requesterId">ID of the user performing the addition (for authorization).</param>
    /// <param name="courier">The BO.Courier object containing the new courier's data.</param>
    public void AddCourier(int requesterId, BO.Courier courier);
}