using BO;
using System.Collections.Generic;

namespace BlApi;

/// <summary>
/// Defines the core API for managing customer orders and subsequent delivery operations.
/// </summary>
public interface IOrder
{
    // --- General Management ---

    /// <summary>
    /// Calculates total order amounts categorized by their status.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the calculation.</param>
    /// <returns>An array containing the aggregated sums per status index.</returns>
    int[] SumAmountOfOrders(int requesterId);

    /// <summary>
    /// Retrieves a list of orders based on filtering and sorting criteria.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the list.</param>
    /// <param name="filterBy">The field used to filter the list (nullable).</param>
    /// <param name="filterValue">The value to match for the filter (nullable).</param>
    /// <param name="sortBy">The field used to sort the list (nullable).</param>
    /// <returns>A collection of simplified order objects (OrderInList).</returns>
    IEnumerable<BO.OrderInList> ListOfOrder(int requesterId, OrderInListEnum? filterBy, object? filterValue, OrderInListEnum? sortBy);

    /// <summary>
    /// Retrieves detailed information for a specific order.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the details.</param>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <returns>The detailed BO.Order object.</returns>
    BO.Order OrderDetails(int requesterId, int orderId);

    /// <summary>
    /// Updates the details of an existing order.
    /// </summary>
    /// <param name="requesterId">The ID of the user performing the update.</param>
    /// <param name="order">The BO.Order object with updated data.</param>
    void UpdateOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Adds a new order to the system.
    /// </summary>
    /// <param name="requesterId">The ID of the user adding the order.</param>
    /// <param name="order">The BO.Order object containing the new order data.</param>
    void AddOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Marks a specific order as canceled.
    /// </summary>
    /// <param name="requesterId">The ID of the user performing the cancellation.</param>
    /// <param name="orderId">The ID of the order to cancel.</param>
    void CancelOrder(int requesterId, int orderId);


    // --- Courier Operations ---

    /// <summary>
    /// Marks a specific delivery as closed/provided by a courier.
    /// </summary>
    /// <param name="requesterId">The ID of the user (must be the courier).</param>
    /// <param name="courierId">The ID of the courier closing the delivery.</param>
    /// <param name="deliveryId">The ID of the delivery being closed.</param>
    void CloseOrder(int requesterId, int courierId, int deliveryId);

    /// <summary>
    /// Assigns an order to a courier (Courier selects an order to handle).
    /// </summary>
    /// <param name="requesterId">The ID of the user (must be the courier).</param>
    /// <param name="courierId">The ID of the courier assigning the order.</param>
    /// <param name="orderId">The ID of the order to be assigned.</param>
    void OrderSelection(int requesterId, int courierId, int orderId);

    /// <summary>
    /// Retrieves a history list of closed deliveries for a specific courier.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the history.</param>
    /// <param name="courierId">The ID of the courier.</param>
    /// <param name="filterBy">The OrderType used to filter the list.</param>
    /// <param name="sortBy">The field used to sort the closed deliveries list.</param>
    /// <returns>A collection of ClosedDeliveryInList objects.</returns>
    IEnumerable<BO.ClosedDeliveryInList> CloseOrderByCourier(int requesterId, int courierId, OrderType? filterBy, ClosedDeliveryInListEnum? sortBy);

    /// <summary>
    /// Retrieves a list of open orders suitable for a specific courier, filtered by distance and criteria.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the list.</param>
    /// <param name="courierId">The ID of the courier.</param>
    /// <param name="filterBy">The OrderType used to filter the open orders.</param>
    /// <param name="sortBy">The criteria to sort the results.</param>
    /// <returns>A collection of OpenOrderInList objects.</returns>
    IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(int requesterId, int courierId, OrderType? filterBy, OpenOrderInListEnum? sortBy);

    // ---------------- for test BL ---------------------


        /// <summary>
    /// Deletes an order from the system (For testing purposes only).
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting deletion.</param>
    /// <param name="orderId">The ID of the order to delete.</param>
    internal void DeleteOrder(int requesterId, int orderId);
}