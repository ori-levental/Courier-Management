namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices; // Added for MethodImpl

/// <summary>
/// Implements the IOrder interface using XML serialization (XmlSerializer).
/// This implementation loads the entire list into memory, modifies it, and saves it back.
/// </summary>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Adds a new Order to the XML file, assigning the next sequential ID from Config.
    /// </summary>
    /// <param name="item">The order object to add (ID is ignored and assigned by DAL).</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Order item)
    {
        List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        // Create a copy with the new sequential ID
        Order temp = item with { Id = Config.NextOrderId };
        Orders.Add(temp);
        XMLTools.SaveListToXMLSerializer(Orders, Config.s_orders_xml);
    }

    /// <summary>
    /// Deletes an Order from the XML list by ID.
    /// </summary>
    /// <param name="id">The ID of the Order to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the Order is not found.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        // RemoveAll returns the count of removed items. Checks if 0 were removed.
        if (Orders.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Order with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Orders, Config.s_orders_xml);
    }

    /// <summary>
    /// Clears the entire list of Orders from the XML file.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        // Overwrites the XML file with a new, empty list
        XMLTools.SaveListToXMLSerializer(new List<Order>(), Config.s_orders_xml);
    }

    /// <summary>
    /// Retrieves an Order by ID, or null if not found.
    /// </summary>
    /// <param name="id">The ID of the Order to find.</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(int id)
    {
        List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return Orders.FirstOrDefault(o => o.Id == id);
    }

    /// <summary>
    /// Retrieves the first Order matching the provided filter condition.
    /// </summary>
    /// <param name="filter">The predicate (condition) to apply.</param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(Func<Order, bool> filter)
    {
        List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return Orders.FirstOrDefault(filter);
    }

    /// <summary>
    /// Retrieves all Orders, optionally filtered by a predicate.
    /// </summary>
    /// <param name="filter">Optional predicate to filter the list.</param>
    /// <returns>An IEnumerable containing the filtered or complete list.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        // Loads the entire list
        List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        if (filter == null)
            return Orders;

        // Returns an IEnumerable filtered by the predicate
        return Orders.Where(filter);
    }

    /// <summary>
    /// Updates an existing Order by removing the old instance and adding the new one.
    /// </summary>
    /// <param name="item">The updated Order object.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the Order is not found.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Order item)
    {
        List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        // Remove the old item (and check for existence)
        if (Orders.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Order with ID={item.Id} does Not exist");
        // Add the new item
        Orders.Add(item);
        XMLTools.SaveListToXMLSerializer(Orders, Config.s_orders_xml);
    }
}