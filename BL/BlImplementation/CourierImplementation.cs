using BlApi;
using BO;
using DalApi;
using System.Globalization;

namespace BlImplementation;

internal class CourierImplementation : BlApi.ICourier
{
    public void AddCourier(int requesterId, Courier courier)
    {
        Helpers.CourierManager.AccessPermissionToManager(requesterId);
        if (courier == null)
            throw new BO.BlNotNullableException("Cannot add null object");

        Helpers.CourierManager.CheckCorrectnessVariables(courier);
        Helpers.CourierManager.AddCourier(courier);
    }

    public void DeleteCourier(int requesterId, int courierId)
    {
        Helpers.CourierManager.AccessPermissionToManager(requesterId);
        if (Helpers.CourierManager.CheckIfOrderOpen(courierId))
            throw new BO.BlDeletionImpossibleException($"Cannot delete courier - {courierId} because he have an active order.");
        Helpers.CourierManager.DeleteCourier(courierId);
    }


    public EmployType EnterToSystem(int id, string password)
    {
        Helpers.CourierManager.CheckId(id);
        Helpers.CourierManager.CheckPasswordEntry(id, password);
        return Helpers.CourierManager.GetEmployType(id);
    }

    public IEnumerable<CourierInList> ListOfCourier(int requesterId, bool? isActive, CourierInListEnum? sortBy)
    {
        dHelpers.CourierManager.AccessPermissionToManager(requesterId);
        IEnumerable<CourierInList> couriers = Helpers.CourierManager.FilterByActive(isActive);
        return Helpers.CourierManager.SortBy(couriers, sortBy);
    }

    ///////////////////----------------------------------------------------------------------------////////////////////
    public Courier SearchCourier(int requesterId, int CourierId)
    {
        Helpers.CourierManager.AccessPermissionToManager(requesterId);
        return Helpers. Factory.Get.Courier.Read(CourierId);
    }

    public void UpdateCourier(int requesterId, Courier courier)
    {
        throw new NotImplementedException();
    }
}
