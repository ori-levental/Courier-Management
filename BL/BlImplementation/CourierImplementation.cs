using BlApi;
using BO;
using DalApi;
using System.Globalization;

namespace BlImplementation;

internal class CourierImplementation : BlApi.ICourier
{
    /// <summary>
    /// Adds a new courier to the system.
    /// Validates manager permissions and checks data correctness before saving.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the action (must be a Manager).</param>
    /// <param name="courier">The courier object to add.</param>
    /// <exception cref="BO.BLNotNullableException">Thrown if the courier object is null.</exception>
    /// <exception cref="BO.BLAccessPermission">Thrown if the requester is not a manager.</exception>
    public void AddCourier(int requesterId, Courier courier)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        if (courier == null)
            throw new BO.BLNotNullableException("Cannot add null object");

        Helpers.CourierManager.CheckCorrectnessVariables(courier);
        Helpers.CourierManager.AddCourier(courier);
    }

    /// <summary>
    /// Deletes a courier from the system.
    /// Ensures the courier is not currently handling an active order before deletion.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the action (must be a Manager).</param>
    /// <param name="courierId">The ID of the courier to delete.</param>
    /// <exception cref="BO.BlDeletionImpossibleException">Thrown if the courier has an active order.</exception>
    public void DeleteCourier(int requesterId, int courierId)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        if (Helpers.CourierManager.CheckIfOrderOpen(courierId))
            throw new BO.BlDeletionImpossibleException($"Cannot delete courier - {courierId} because he have an active order.");
        Helpers.CourierManager.DeleteCourier(courierId);
    }

    /// <summary>
    /// Authenticates a user entering the system.
    /// Validates ID and password, then returns the user's role.
    /// </summary>
    /// <param name="id">The user's ID.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>The employment type (Manager or Courier) upon successful login.</returns>
    public EmployType EnterToSystem(int id, string password)
    {
        Helpers.CourierManager.CheckId(id);
        Helpers.CourierManager.CheckPasswordEntry(id, password);
        return Helpers.CourierManager.GetEmployType(id);
    }

    /// <summary>
    /// Retrieves a list of couriers, optionally filtered by status and sorted by a specific criterion.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the action (must be a Manager).</param>
    /// <param name="isActive">Filter: null for all, true for active, false for inactive.</param>
    /// <param name="sortBy">The criteria to sort the list by (e.g., ID, Name, Performance).</param>
    /// <returns>A collection of CourierInList objects.</returns>
    public IEnumerable<CourierInList> ListOfCourier(int requesterId, bool? isActive, CourierInListEnum? sortBy)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        IEnumerable<CourierInList> couriers = Helpers.CourierManager.FilterByActive(isActive);
        return Helpers.CourierManager.SortBy(couriers, sortBy);
    }

    /// <summary>
    /// Searches for a specific courier and returns their full details.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the action (must be a Manager).</param>
    /// <param name="CourierId">The ID of the courier to retrieve.</param>
    /// <returns>The full Courier business object.</returns>
    public Courier SearchCourier(int requesterId, int CourierId)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        return Helpers.CourierManager.SearchCourier(CourierId);
    }

    /// <summary>
    /// Updates the details of an existing courier.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the action (must be a Manager).</param>
    /// <param name="courier">The updated courier object.</param>
    public void UpdateCourier(int requesterId, Courier courier)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        Helpers.CourierManager.UpdateCourier(courier);
    }
}