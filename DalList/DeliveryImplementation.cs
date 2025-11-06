namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// Implements the IDelivery interface for managing Delivery data
/// using an in-memory list (DataSource).
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Creates a new delivery. 
    /// Note: This implementation copies all data from 'item'
    /// but assigns a *new* unique ID from Config.NextDeliveryId.
    /// The ID on the passed 'item' is ignored.
    /// </summary>
    /// <param name="item">The delivery object to copy data from.</param>
    public void Create(Delivery item)
    {
        // 'with' creates a shallow copy of 'item', 
        // while replacing the 'Id' property with a new value.
        Delivery temp = item with { Id = Config.NextDeliveryId }; // Assuming typo in Config is fixed

        // Add the new object (with the new ID) to the data source.
        DataSource.Deliveries.Add(temp);
    }

    /// <summary>
    /// Deletes a delivery from the data source by its ID.
    /// </summary>
    /// <param name="id">The ID of the delivery to delete.</param>
    /// <exception cref="Exception">Throws if a delivery with the ID is not found.</exception>
    public void Delete(int id)
    {
        // Find the item to delete, throw exception if not found
        Delivery? temp = Read(id) ?? throw new Exception($"Delivery with ID={id} does Not exists");

        // else
        DataSource.Deliveries.Remove(temp);
    }

    /// <summary>
    /// Clears the entire list of deliveries from the data source.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Deliveries.Clear();
    }

    /// <summary>
    /// Finds and returns a delivery by its ID.
    /// </summary>
    /// <param name="id">The ID of the delivery to find.</param>
    /// <returns>The found delivery object, or null if not found.</returns>
    public Delivery? Read(int id)
    {
        return DataSource.Deliveries.Find(T => T.Id == id);
    }

    /// <summary>
    /// Returns a copy of the entire list of deliveries.
    /// </summary>
    /// <returns>A new list containing all delivery objects.</returns>
    public List<Delivery> ReadAll()
    {
        // Creates a (shallow) copy of the list.
        return new List<DO.Delivery>(DataSource.Deliveries);
    }

    /// <summary>
    /// Updates an existing delivery by replacing it.
    /// This implementation performs a Delete operation followed by an Add operation,
    /// preserving the original ID.
    /// </summary>
    /// <param name="item">The delivery object with updated information.</param>
    /// <exception cref="Exception">Throws if a delivery with the item's ID does not exist (from Delete).</exception>
    public void Update(Delivery item)
    {
        // Delete the old version of the item (throws if not found)
        Delete(item.Id);

        // Add the new version of the item (with the same ID)
        DataSource.Deliveries.Add(item);
    }
}