using BlApi;
using BO;

namespace BlImplementation;

internal class CourierImplementation : ICourier
{
    public void AddCourier(int requesterId, Courier courier)
    {
        Helpers.CourierManager.AccessPermissionToManager(requesterId);
        if (courier == null)
            throw new BO.BLNotNullableException("Cannot add null object");

        Helpers.CourierManager.CheckCorrectnessVariables(courier);
        Helpers.CourierManager.AddCourier(requesterId, courier);
    }

    public void DeleteCourier(int requesterId, int courierId)
    {
        Helpers.CourierManager.AccessPermissionToManager(requesterId);
        Helpers.CourierManager.CheckIfOrderOpen(courierId);
        Helpers.CourierManager.DeleteCourier(courierId);
    }


    public EmployType EnterToSystem(int id, string password)
    {
        Helpers.CourierManager.CheckId(id);
        Helpers.CourierManager.CheckPasswordEntry(id, password);
        return Helpers.CourierManager.GetEmployType(id);
    }

    ///////////////////----------------------------------------------------------------------------////////////////////

    public IEnumerable<CourierInList> ListOfCourier(int requesterId, bool? isActive, CourierInListEnum? SortBy)
    {
        throw new NotImplementedException();
    }

    public Courier SearchCourier(int requesterId, int CourierId)
    {
        throw new NotImplementedException();
    }

    public void UpdateCourier(int requesterId, Courier courier)
    {
        throw new NotImplementedException();
    }
}
