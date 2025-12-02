using BO;
using System.Collections.Generic;

namespace BlApi;

/// <summary>
/// Defines the core API for managing customer orders and subsequent delivery operations.
/// </summary>
public interface IOrder
{
    /// <summary>
    /// Calculates total order amounts categorized by type.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the calculation.</param>
    /// <returns>An array containing the aggregated sums.</returns>
    public int[] SumAmountOfOrders(int requesterId);

    /// <summary>
    /// Retrieves a list of orders based on filtering and sorting criteria.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the list.</param>
    /// <param name="filteredBy">The field used to filter the order list.</param>
    /// <param name="sortBy">The field used to sort the order list.</param>
    /// <returns>A list of simplified order objects (BO.OrderInList).</returns>
    public IEnumerable<BO.OpenOrderInList> ListOfOrder(int requesterId,int courierId, OrderInListEnum? filteredBy, OrderInListEnum? sortBy);

    /// <summary>
    /// Retrieves detailed information for a specific order.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the details.</param>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <returns>The detailed BO.Order object.</returns>
    public BO.Order OrderDetails(int requesterId, int orderId);

    /// <summary>
    /// Updates the details of an existing order.
    /// </summary>
    /// <param name="requesterId">The ID of the user performing the update.</param>
    /// <param name="order">The BO.Order object with updated data.</param>
    public void UpdateOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Marks a specific order as canceled.
    /// </summary>
    /// <param name="requesterId">The ID of the user performing the cancellation.</param>
    /// <param name="orderId">The ID of the order to cancel.</param>
    public void CancelOrder(int requesterId, int orderId);

    /// <summary>
    /// Adds a new order to the system.
    /// </summary>
    /// <param name="requesterId">The ID of the user adding the order.</param>
    /// <param name="order">The BO.Order object containing the new order data.</param>
    public void AddOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Marks a specific order as closed by a courier.
    /// </summary>
    /// <param name="requesterId">The ID of the user initiating the closure.</param>
    /// <param name="courierId">The ID of the courier who completed the delivery.</param>
    /// <param name="orderId">The ID of the order being closed.</param>
    public void CloseOrder(int requesterId, int courierId, int orderId);

    /// <summary>
    /// Initiates the processing phase for a specific order.
    /// </summary>
    /// <param name="requesterId">The ID of the user initiating the processing.</param>
    /// <param name="courierId">The ID of the courier assigned to process the order.</param>
    /// <param name="orderId">The ID of the order to process.</param>
    public void OrderProcessing(int requesterId, int courierId, int orderId);

    /// <summary>
    /// Retrieves a list of closed deliveries by a specific courier, with filtering and sorting options.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the list.</param>
    /// <param name="courierId">The ID of the courier whose deliveries are retrieved.</param>
    /// <param name="filteredBy">The OrderType used to filter the list.</param>
    /// <param name="sortBy">The field used to sort the closed deliveries list.</param>
    /// <returns>A list of simplified closed delivery objects (BO.ClosedDeliveryInList).</returns>
    public IEnumerable<BO.ClosedDeliveryInList> CloseOrderByCourier(int requesterId, int courierId, OrderType? filteredBy, ClosedDeliveryInListEnum? sortBy);

    // ---------------- for test BL ---------------------

    internal void DeleteOrder(int requesterId, int orderId);
}