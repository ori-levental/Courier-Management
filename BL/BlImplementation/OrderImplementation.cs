namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{
    public void AddOrder(int requesterId, Order order)
    {
        try
        {
            if (order == null)
                throw new BO.BLTemporaryNotAvailableException("Cannot add null object");
        }
        catch (BO.BLTemporaryNotAvailableException ex)
        {
            throw new BO.BLGeneralException($"ERROR : occurred while try to add new Order: {ex.Message}");
        }
        
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

    public void UpdateOrder(int requesterId, Order order)
    {
        throw new NotImplementedException();
    }

    void IOrder.DeleteOrder(int requesterId, int orderId)
    {
        throw new NotImplementedException();
    }
}
