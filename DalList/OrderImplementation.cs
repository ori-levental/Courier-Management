namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Implements the IOrder interface for managing Order data
/// using an in-memory list (DataSource).
/// </summary>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Creates a new order. 
    /// Note: This implementation copies all data from 'item'
    /// but assigns a *new* unique ID from Config.NextOrderId.
    /// The ID on the passed 'item' is ignored.
    /// </summary>
    /// <param name="item">The order object to copy data from.</param>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Order item)
    {
        // 'with' creates a shallow copy of 'item', 
        // while replacing the 'Id' property with a new value.
        Order temp = item with { Id = Config.NextOrderId };

        // Add the new object (with the new ID) to the data source.
        DataSource.Orders.Add(temp);
    }

    /// <summary>
    /// Deletes an order from the data source by its ID.
    /// </summary>
    /// <param name="id">The ID of the order to delete.</param>
    /// <exception cref="Exception">Throws if an order with the ID is not found.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        Order? temp = Read(id) ?? throw new DalDoesNotExistException($"Order with ID={id} does Not exists");
        // else
        DataSource.Orders.Remove(temp);
    }

    /// <summary>
    /// Clears the entire list of orders from the data source.
    /// </summary>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    /// <summary>
    /// Finds and returns an order by its ID.
    /// </summary>
    /// <param name="id">The ID of the order to find.</param>
    /// <returns>The found order object, or null if not found.</returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Order? Read(int id)
    {
        return DataSource.Orders.FirstOrDefault(T => T.Id == id);
    }
    /// <summary>
    /// Finds and returns the first entity that matches a specific condition.
    /// </summary>
    /// <param name="filter">A lambda expression (predicate) to filter the entities.</param>
    /// <returns>The first matching entity, or null if no entity is found.</returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Order? Read(Func<Order, bool> filter)
    {
        return DataSource.Orders.FirstOrDefault(filter);
    }

    /// <summary>
    /// Returns a copy of the entire list of orders.
    /// </summary>
    /// <returns>A new list containing all order objects.</returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null) //stage 2
    {
        // without filter - return the list
        if (filter == null)
            return DataSource.Orders;

        // whith filter - use at him
        return DataSource.Orders.Where(filter);
    }

    /// <summary>
    /// Updates an existing order by replacing it.
    /// This implementation performs a Delete operation followed by an Add operation,
    /// preserving the original ID.
    /// </summary>
    /// <param name="item">The order object with updated information.</param>
    /// <exception cref="Exception">Throws if an order with the item's ID does not exist (from Delete).</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Order item)
    {
        // Delete the old version of the item (throws if not found)
        Delete(item.Id);

        // Add the new version of the item (with the same ID)
        DataSource.Orders.Add(item);
    }
}