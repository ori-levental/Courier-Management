namespace Helpers;

using System;
using System.Collections.Generic;

/// <summary>
/// This class is a helper class allowing to manage observers for different logical entities
/// in the Business Logic (BL) layer.
/// It offers infrastructure to support observers with Thread-Safety for async operations.
/// </summary>
class ObserverManager
{
    // Lock object to ensure thread safety when accessing the observers dictionary
    private readonly object _observerLock = new();

    /// <summary>
    /// event delegate for list observers - it's called whenever there may be need to update the presentation
    /// of the list of entities
    /// </summary>
    private event Action? _listObservers;

    /// <summary>
    /// Hash table (Dictionary) of individual entity delegates.
    /// The index (key) is the ID of an entity.
    /// </summary>
    private readonly Dictionary<int, Action?> _specificObservers = new();

    /// <summary>
    /// Add an observer on change in list of entities that may effect the list presentation
    /// </summary>
    /// <param name="observer">Observer method (usually from Presentation Layer) to be added</param>
    internal void AddListObserver(Action observer)
    {
        lock (_observerLock)
        {
            _listObservers += observer;
        }
    }

    /// <summary>
    /// Remove an observer on change in list of entities that may effect the list presentation
    /// </summary>
    /// <param name="observer">Observer method (usually from Presentation Layer) to be removed</param>
    internal void RemoveListObserver(Action observer)
    {
        lock (_observerLock)
        {
            _listObservers -= observer;
        }
    }

    /// <summary>
    /// Add an observer on change in an instance of entity that may effect the entity instance presentation
    /// </summary>
    /// <param name="id">the ID value for the entity instance to be observed</param>
    /// <param name="observer">Observer method (usually from Presentation Layer) to be added</param>
    internal void AddObserver(int id, Action observer)
    {
        lock (_observerLock)
        {
            if (_specificObservers.ContainsKey(id)) // if there are already observers for the ID
                _specificObservers[id] += observer; // add the given observer
            else // there is the first observer for the ID
                _specificObservers[id] = observer; // create hash table entry for the ID with the given observer
        }
    }

    /// <summary>
    /// Remove an observer on change in an instance of entity that may effect the entity instance presentation
    /// </summary>
    /// <param name="id">the ID value for the observed entity instance</param>
    /// <param name="observer">Observer method (usually from Presentation Layer) to be removed</param>
    internal void RemoveObserver(int id, Action observer)
    {
        lock (_observerLock)
        {
            // First, lets check that there are any observers for the ID
            if (_specificObservers.ContainsKey(id) && _specificObservers[id] is not null)
            {
                Action? specificObserver = _specificObservers[id]; // Reference to the delegate element for the ID
                specificObserver -= observer; // Remove the given observer from the delegate

                // Update the dictionary with the modified delegate
                _specificObservers[id] = specificObserver;

                if (specificObserver?.GetInvocationList().Length == 0) // if there are no more observers for the ID
                    _specificObservers.Remove(id); // then remove the hash table entry for the ID
            }
        }
    }

    /// <summary>
    /// Notify all the observers that there is a change for one or more entities in the list
    /// that may affect the whole list presentation
    /// </summary>
    internal void NotifyListUpdated()
    {
        // Copy the delegate to avoid locking during the invocation (best practice)
        Action? observersToNotify;
        lock (_observerLock)
        {
            observersToNotify = _listObservers;
        }
        observersToNotify?.Invoke();
    }

    /// <summary>
    /// Notify observers of a specific entity that there was some change in the entity
    /// </summary>
    /// <param name="id">a specific entity ID</param>
    internal void NotifyItemUpdated(int id)
    {
        Action? observerToNotify = null;

        lock (_observerLock)
        {
            if (_specificObservers.ContainsKey(id))
            {
                observerToNotify = _specificObservers[id];
            }
        }

        // Invoke outside the lock to prevent deadlocks
        observerToNotify?.Invoke();
    }
}