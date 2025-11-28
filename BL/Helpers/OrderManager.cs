using DalApi;

namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get; //stage 4
    private static DO.Order BOToDOOrder(BO.Order BoOrder)
    {
        DO.Order doCourier = new DO.Order()
        {
            Id = BoOrder.Id,
            Description = BoOrder.Description,
            Address = BoOrder.FullAddress,
            PhoneNumber = BoOrder.PhoneNumber,
            Latitude = BoOrder.Latitude,
            Type = (DO.Enums.OrderType)BoOrder.OrderType!,
            Longitude = BoOrder.Longitude
        };
        return doCourier;
    }
}
