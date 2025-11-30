namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{
    public void AddOrder(int requesterId, Order order)
    {
       Helpers.OrderManager.AddOrder(requesterId, order);
    }

    public void CancelOrder(int requesterId, int orderId)
    {
        try
        {
           if(ListOfOrder.Read(requesterId, orderId) == null)
            {

            }
        }
        catch (BO.BLTemporaryNotAvailableException ex)
        {
            throw new BO.BLGeneralException($"ERROR : occurred while try to add delete Courier: {ex.Message}");
        }
    }

    public void CloseOrder(int requesterId, int courierId, int orderId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ClosedDeliveryInList> CloseOrderByCourier(int requesterId, int courierId, OrderType filteredBy, ClosedDeliveryInListEnum sortBy)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<OrderInList> ListOfOrder(int requesterId, OrderInListEnum filteredBy, OrderInListEnum sortBy)
    {
        throw new NotImplementedException();
    }

    public Order OrderDetails(int requesterId, int orderId)
    {
        throw new NotImplementedException();
    }

    public void OrderProcessing(int requesterId, int courierId, int orderId)
    {
        throw new NotImplementedException();
    }

    public int[] SumAmountOfOrders(int requesterId)
    {
        throw new NotImplementedException();
    }

    public void UpdateOrder(int requesterId, int OrderId)
    {
       Helpers.OrderManager.AccessPermissionToManager(requesterId);
        try
        {
            dalOrder = 
            if (dalOrder == null)
                throw new BO.BLItemNotFoundException($"Order with ID {order.Id} not found");
            // Update fields
            dalOrder.OrderType = (DalApi.OrderType).get();
            dalOrder.Description = order.Description;
            dalOrder.OrderingName = order.OrderingName;
            dalOrder.PhoneNumber = order.PhoneNumber;
            dalOrder.IsHeavy = order.IsHeavy;
            dalOrder.FullAddress = order.FullAddress;
            dalOrder.Latitude = order.Latitude;
            dalOrder.Longitude = order.Longitude;
            DalApi.Factory.Get.Order.Update(dalOrder);
        }
        catch (BO.BLTemporaryNotAvailableException ex)
        {
            throw new BO.BLGeneralException($"ERROR : occurred while try to update Order: {ex.Message}");
        }
    }

    void IOrder.DeleteOrder(int requesterId, int orderId)
    {
        throw new NotImplementedException();
    }
}
