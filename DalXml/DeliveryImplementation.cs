namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Implements the IDelivery interface using XML serialization (XmlSerializer).
/// This implementation loads the entire list into memory, modifies it, and saves it back.
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Adds a new Delivery to the XML file, assigning the next sequential ID from Config.
    /// </summary>
    /// <param name="item">The delivery object to add (ID is ignored and assigned by DAL).</param>
    public void Create(Delivery item)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        Delivery temp = item with { Id = Config.NextDeliveryId };
        Deliveries.Add(temp);
        XMLTools.SaveListToXMLSerializer(Deliveries, Config.s_deliveries_xml);
    }

    /// <summary>
    /// Deletes a Delivery from the XML list by ID.
    /// </summary>
    /// <param name="id">The ID of the Delivery to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the Delivery is not found.</exception>
    public void Delete(int id)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        // RemoveAll returns the count of removed items. Checks if 0 were removed.
        if (Deliveries.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Deliveries, Config.s_deliveries_xml);
    }

    /// <summary>
    /// Clears the entire list of Deliveries from the XML file.
    /// </summary>
    public void DeleteAll()
    {
        // Overwrites the XML file with a new, empty list
        XMLTools.SaveListToXMLSerializer(new List<Delivery>(), Config.s_deliveries_xml);
    }

    /// <summary>
    /// Retrieves a Delivery by ID, or null if not found.
    /// </summary>
    public Delivery? Read(int id)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        return Deliveries.FirstOrDefault(d => d.Id == id);
    }

    /// <summary>
    /// Retrieves the first Delivery matching the provided filter condition.
    /// </summary>
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        return Deliveries.FirstOrDefault(filter);
    }

    /// <summary>
    /// Retrieves all Deliveries, optionally filtered by a predicate.
    /// </summary>
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        // Loads the entire list
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);

        if (filter == null)
            return Deliveries;

        // Returns an IEnumerable filtered by the predicate
        return Deliveries.Where(filter);
    }

    /// <summary>
    /// Updates an existing Delivery by removing the old instance and adding the new one.
    /// </summary>
    /// <param name="item">The updated Delivery object.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the Delivery is not found.</exception>
    public void Update(Delivery item)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        // Remove the old item (and check for existence)
        if (Deliveries.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist");
        // Add the new item
        Deliveries.Add(item);
        XMLTools.SaveListToXMLSerializer(Deliveries, Config.s_deliveries_xml);
    }
}