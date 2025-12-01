namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{
    public void AddOrder(int requesterId, Order order)
    {
        Helpers.OrderManager.AccessPermissionToManager(requesterId);
        if (order == null)
            throw new BO.BLNotNullableException("Cannot add null object");

        Helpers.OrderManager.CheckCorrectnessVariables(order);
        Helpers.OrderManager.AddOrder(requesterId, order);
    }

    public void CancelOrder(int requesterId, int orderId)
    {
        Helpers.OrderManager.AccessPermissionToManager(requesterId);
        Helpers.OrderManager.CancelOrder(orderId);
    }
    public void CloseOrder(int requesterId, int courierId, int orderId)
    {
        Helpers.OrderManager.AccessPermissionToManager(requesterId);
        Helpers.OrderManager.CloseOrder(courierId, orderId);
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

    void IOrder.AddOrder(int requesterId, BO.Order order)
    {
        throw new NotImplementedException();
    }

    void IOrder.DeleteOrder(int requesterId, int orderId)
    {
        throw new NotImplementedException();
    }

    BO.Order IOrder.OrderDetails(int requesterId, int orderId)
    {
        throw new NotImplementedException();
    }

    void IOrder.UpdateOrder(int requesterId, BO.Order order)
    {
        throw new NotImplementedException();
    }
}
