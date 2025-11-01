namespace Dal;
using DalApi;
using DO;
using System.ComponentModel;

/// <summary>
/// Implements the ICourier interface for managing Courier data
/// using an in-memory list (DataSource).
/// </summary>
public class CourierImplementation : ICourier
{
    /// <summary>
    /// Adds a new Courier to the data source.
    /// </summary>
    /// <param name="item">The Courier object to add.</param>
    /// <exception cref="Exception">Throws an exception if a Courier with the same ID already exists.</exception>
    public void Create(Courier item)
    {
        if (Read(item.Id) != null)
        {
            throw new Exception(@"An object of type Courier with such ID already exists.");
        }
        // else
        DataSource.Couriers.Add(item);
    }

    /// <summary>
    /// Finds and returns a Courier by its ID.
    /// </summary>
    /// <param name="id">The ID of the Courier to find.</param>
    /// <returns>The found Courier object, or null if not found.</returns>
    public Courier? Read(int id)
    {
        return DataSource.Couriers.Find(T => T.Id == id);
    }

    /// <summary>
    /// Returns a copy of the entire list of Couriers.
    /// </summary>
    /// <returns>A new list containing all Courier objects.</returns>
    public List<DO.Courier> ReadAll()
    {
        return new List<DO.Courier>(DataSource.Couriers);
    }

    /// <summary>
    /// Updates an existing Courier's details.
    /// This implementation performs a Delete operation followed by a Create operation.
    /// </summary>
    /// <param name="item">The Courier object with updated information (ID is used to find the original).</param>
    /// <exception cref="Exception">Throws an exception if a Courier with the specified ID does not exist (propagated from Delete).</exception>
    public void Update(Courier item)
    {
        Delete(item.Id);
        Create(item);
    }

    /// <summary>
    /// Deletes a Courier from the data source by its ID.
    /// </summary>
    /// <param name="id">The ID of the Courier to delete.</param>
    /// <exception cref="Exception">Throws an exception if a Courier with the specified ID does not exist.</exception>
    public void Delete(int id)
    {
        Courier? temp = Read(id) ?? throw new Exception(@"An object of type Courier with such ID not exists.");
        // else
        DataSource.Couriers.Remove(temp);
    }

    /// <summary>
    /// Clears the entire list of Couriers from the data source.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Couriers.Clear();
    }
}