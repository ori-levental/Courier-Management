using BlApi;
using BO;
using Helpers;
using System.Collections.Generic;

namespace BlImplementation;

internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Adds a new courier to the system.
    /// Validates manager permissions and checks data correctness before saving.
    /// </summary>
    public void AddCourier(int requesterId, Courier courier)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);

        if (courier == null)
            throw new BO.BlNotNullableException("Cannot add null object");

        Helpers.CourierManager.CheckCorrectnessVariables(courier);
        Helpers.CourierManager.AddCourier(requesterId, courier);
    }

    /// <summary>
    /// Deletes a courier from the system.
    /// Ensures the courier is not currently handling an active order before deletion.
    /// </summary>
    public void DeleteCourier(int requesterId, int courierId)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);

        if (Helpers.CourierManager.CheckIfOrderOpen(courierId))
            throw new BO.BlDeletionImpossibleException($"Cannot delete courier - {courierId} because they have an active order.");

        Helpers.CourierManager.DeleteCourier(courierId);
    }

    /// <summary>
    /// Authenticates a user entering the system.
    /// </summary>
    public EmployType EnterToSystem(int id, string password)
    {
        Helpers.CourierManager.CheckId(id);
        Helpers.CourierManager.CheckPasswordEntry(id, password);
        return Helpers.CourierManager.GetEmployType(id);
    }

    /// <summary>
    /// Retrieves a list of couriers, optionally filtered by status and sorted.
    /// </summary>
    public IEnumerable<DeliveryInList> ListOfCourier(int requesterId, bool? isActive, CourierInListEnum? sortBy)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        IEnumerable<DeliveryInList> couriers = Helpers.CourierManager.FilterByActive(isActive);
        return Helpers.CourierManager.SortBy(couriers, sortBy);
    }

    /// <summary>
    /// Searches for a specific courier and returns their full details.
    /// Allows access to the courier themselves or a manager.
    /// </summary>
    public Courier SearchCourier(int requesterId, int courierId)
    {
        // Permission Check: Allow if Manager OR if the requester is the courier looking at their own profile
        if (!Helpers.CourierManager.AccessCourier(requesterId, courierId))
           Helpers.Tools.AccessPermissionToManager(requesterId);


        return Helpers.CourierManager.SearchCourier(courierId);
    }

    /// <summary>
    /// Updates the details of an existing courier.
    /// A courier can update their own details except for 'IsActive'.
    /// A manager can update all details.
    /// </summary>
    public void UpdateCourier(int requesterId, Courier courier)
    {
        // Scenario 1: The Courier is updating themselves
        if (Helpers.CourierManager.AccessCourier(requesterId, courier.Id))
        {
            // Fetch current state to compare restricted fields
            BO.Courier original = Helpers.CourierManager.SearchCourier(courier.Id);

            // Constraint: Courier cannot change their own activity status
            if (courier.IsActive != original.IsActive)
                throw new BO.BlAccessPermission("ERROR: A courier cannot change their own activity status.");

        }
        // Scenario 2: Someone else is updating (Must be Manager)
        else
            Helpers.Tools.AccessPermissionToManager(requesterId);


        Helpers.CourierManager.CheckCorrectnessVariables(courier);
        Helpers.CourierManager.UpdateCourier(courier);
    }

    #region Stage 5
    public void AddObserver(Action listObserver) =>
    Helpers.CourierManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
    Helpers.CourierManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
    Helpers.CourierManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
    Helpers.CourierManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5

}