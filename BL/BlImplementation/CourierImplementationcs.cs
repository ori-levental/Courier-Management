using BlApi;
using BO;

namespace BlImplementation;

internal class CourierImplementationcs : ICourier
{
    public void AddCourier(int requesterId, Courier courier)
    {
        try
        {
            if (courier == null)
                throw new BO.BLTemporaryNotAvailableException("Cannot add null object");
        }
        catch (BO.BLTemporaryNotAvailableException ex)
        {
            throw new BO.BLGeneralException($"ERROR : occurred while try to add new Courier: {ex.Message}");
        }
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
