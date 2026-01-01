namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Adds a new order to the system after validation.
    /// </summary>
    public void AddOrder(int requesterId, Order order)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);

        if (order == null)
            throw new BO.BlNotNullableException("Cannot add null object");

        Helpers.OrderManager.CheckCorrectnessVariables(order);
        Helpers.OrderManager.AddOrder(requesterId, order);
    }

    /// <summary>
    /// Cancels an existing order if allowed by logic constraints.
    /// </summary>
    public void CancelOrder(int requesterId, int orderId)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        Helpers.OrderManager.CancelOrder(orderId);
    }

    /// <summary>
    /// Marks a specific delivery as closed/provided by the courier.
    /// </summary>
    public void CloseOrder(int requesterId, int courierId, int deliveryId, ShipmentCompletionStatus status)
    {
        if (requesterId != courierId)
            throw new BO.BlAccessPermission("Requester must be the Courier.");

        Helpers.OrderManager.CloseOrder(courierId, deliveryId,status);
    }

    /// <summary>
    /// Retrieves the history of closed deliveries for a specific courier.
    /// </summary>
    public IEnumerable<ClosedDeliveryInList> CloseOrderByCourier(int requesterId, int courierId, OrderType? filterBy, ClosedDeliveryInListEnum? sortBy)
    {
        if (requesterId != courierId)
            throw new BO.BlAccessPermission("Requester must be the Courier.");

        return Helpers.OrderManager.GetClosedOrdersForCourier(courierId, (ClosedDeliveryInListEnum?)filterBy, null, sortBy);
    }

    /// <summary>
    /// Retrieves a list of orders with optional filtering and sorting.
    /// </summary>
    public IEnumerable<OrderInList> ListOfOrder(int requesterId, OrderInListEnum? filterBy, object? filterValue, OrderInListEnum? sortBy)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        return Helpers.OrderManager.ListOfOrder(filterBy, filterValue, sortBy);
    }

    /// <summary>
    /// Retrieves detailed information for a specific order.
    /// </summary>
    public Order OrderDetails(int requesterId, int orderId)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        return Helpers.OrderManager.GetOrderDetails(orderId);
    }

    /// <summary>
    /// Assigns a selected order to a courier.
    /// </summary>
    public void OrderSelection(int requesterId, int courierId, int orderId)
    {
        if (requesterId != courierId)
            throw new BO.BlAccessPermission("Requester must be the Courier.");

        Helpers.OrderManager.OrderSelection(courierId, orderId);
    }

    /// <summary>
    /// Calculates total order statistics categorized by status.
    /// </summary>
    public int[] SumAmountOfOrders(int requesterId)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        return Helpers.OrderManager.SumAmountOfOrders();
    }

    /// <summary>
    /// Updates the details of an existing order.
    /// </summary>
    public void UpdateOrder(int requesterId, BO.Order order)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        Helpers.OrderManager.UpdateOrder(order);
    }

    /// <summary>
    /// Retrieves a list of open orders suitable for a specific courier based on distance.
    /// </summary>
    public IEnumerable<OpenOrderInList> GetOpenOrdersForCourier(int requesterId, int courierId, OrderType? filterBy, OpenOrderInListEnum? sortBy)
    {
        if (requesterId != courierId)
            throw new BO.BlAccessPermission("Requester must be the Courier.");

        return Helpers.OrderManager.GetOpenOrdersForCourier(courierId, filterBy, sortBy);
    }

    /// <summary>
    /// Deletes an order from the system (Testing/Admin use only).
    /// </summary>
    public void DeleteOrder(int requesterId, int orderId)
    {
        Helpers.Tools.AccessPermissionToManager(requesterId);
        Helpers.OrderManager.DeleteOrder(orderId);
    }

    #region Stage 5
    public void AddObserver(Action listObserver) =>
    Helpers.OrderManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
    Helpers.OrderManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
    Helpers.OrderManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
    Helpers.OrderManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5
}