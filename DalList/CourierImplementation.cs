namespace Dal;
using DalApi;
using DO;
using System.Runtime.CompilerServices;

/// <summary>
/// Implements the ICourier interface for managing Courier data
/// using an in-memory list (DataSource).
/// </summary>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Adds a new Courier to the data source.
    /// </summary>
    /// <param name="item">The Courier object to add.</param>
    /// <exception cref="Exception">Throws an exception if a Courier with the same ID already exists.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Courier item)
    {
        if (Read(item.Id) != null)
        {
            throw new DalAlreadyExistsException($"Courier with ID={item.Id} already exists");
        }
        // else
        DataSource.Couriers.Add(item);
    }

    /// <summary>
    /// Finds and returns a Courier by its ID.
    /// </summary>
    /// <param name="id">The ID of the Courier to find.</param>
    /// <returns>The found Courier object, or null if not found.</returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Courier? Read(int id)
    {
        return DataSource.Couriers.FirstOrDefault(T => T.Id == id);
    }
    /// <summary>
    /// Finds and returns the first entity that matches a specific condition.
    /// </summary>
    /// <param name="filter">A lambda expression (predicate) to filter the entities.</param>
    /// <returns>The first matching entity, or null if no entity is found.</returns>
    
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Courier? Read(Func<Courier, bool> filter)
    {
        return DataSource.Couriers.FirstOrDefault(filter);
    }

    /// <summary>
    /// Returns a copy of the entire list of Couriers.
    /// </summary>
    /// <returns>A new list containing all Courier objects.</returns>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null) //stage 2
    {
        // without filter - return the list
        if (filter == null)
            return DataSource.Couriers;

        // whith filter - use at him
        return DataSource.Couriers.Where(filter);
    }


    /// <summary>
    /// Updates an existing Courier's details.
    /// This implementation performs a Delete operation followed by a Create operation.
    /// </summary>
    /// <param name="item">The Courier object with updated information (ID is used to find the original).</param>
    /// <exception cref="Exception">Throws an exception if a Courier with the specified ID does not exist (propagated from Delete).</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Courier item)
    {
        Delete(item.Id);
        DataSource.Couriers.Add(item);
    }

    /// <summary>
    /// Deletes a Courier from the data source by its ID.
    /// </summary>
    /// <param name="id">The ID of the Courier to delete.</param>
    /// <exception cref="Exception">Throws an exception if a Courier with the specified ID does not exist.</exception>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        Courier? temp = Read(id) ?? throw new DalDoesNotExistException($"Courier with ID={id} does Not exists");
        // else
        DataSource.Couriers.Remove(temp);
    }

    /// <summary>
    /// Clears the entire list of Couriers from the data source.
    /// </summary>

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        DataSource.Couriers.Clear();
    }

}