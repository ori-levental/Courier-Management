using BlApi;
using BO;

namespace BlImplementation;

internal class CourierImplementationcs : ICourier
{
    public void AddCourier(int requesterId, Courier courier)
    {
        Helpers.CourierManager.AccessPermissionToManager(requesterId);
        if (courier == null)
            throw new BO.BLNotNullableException("Cannot add null object");

        Helpers.CourierManager.CheckCorrectnessVariables(courier);
        Helpers.CourierManager.AddCourier(requesterId, courier);
    }



    ///////////////////----------------------------------------------------------------------------////////////////////
    public void DeleteCourier(int requesterId, int CourierId)
    {
        throw new NotImplementedException();
    }

    public EmployType EnterToSystem(int id, string password)
    {
        throw new NotImplementedException();
    }

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
