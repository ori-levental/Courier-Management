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
            throw new BO.BlNotNullableException("Cannot add null object");

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

    public IEnumerable<ClosedDeliveryInList> CloseOrderByCourier(int requesterId, int courierId, OrderType? filteredBy, ClosedDeliveryInListEnum? sortBy)
    {
        Helpers.OrderManager.AccessPermissionToManager(requesterId);
        return Helpers.OrderManager.CloseOrderByCourier(requesterId, courierId, filteredBy, sortBy);
    }

    public IEnumerable<OpenOrderInList> ListOfOrder(int requesterId,int courierId ,OrderInListEnum? filteredBy, OrderInListEnum? sortBy)
    {
       return Helpers.OrderManager.ListOfOrder(requesterId,courierId, filteredBy, sortBy);
    }

    public Order OrderDetails(int requesterId, int orderId)
    {
        Helpers.OrderManager.AccessPermissionToManager(requesterId);
        return Helpers.OrderManager.OrderDetails(orderId);
    }

    public void OrderProcessing(int requesterId, int courierId, int orderId)
    {
      Helpers.OrderManager.AccessPermissionToManager(requesterId);
        Helpers.OrderManager.OrderProcessing(requesterId,courierId, orderId);
    }

    public int[] SumAmountOfOrders(int requesterId)
    {
        Helpers.OrderManager.AccessPermissionToManager(requesterId);
        return  Helpers.OrderManager.SumAmoutOfOrders();
    }

    void IOrder.DeleteOrder(int requesterId, int orderId)
    {
        Helpers.OrderManager.AccessPermissionToManager(requesterId);
        Helpers.OrderManager.DeleteOrder(orderId);
    }

    void IOrder.UpdateOrder(int requesterId, BO.Order order)
    {
        Helpers.OrderManager.AccessPermissionToManager(requesterId);
        Helpers.OrderManager.UpdateOrder(order);
    }
}
