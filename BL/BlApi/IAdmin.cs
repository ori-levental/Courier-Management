using BO;

namespace BlApi;

/// <summary>
/// Defines the API for administrative and configuration tasks within the Business Logic (BL) layer.
/// </summary>
public interface IAdmin
{
    /// <summary>
    /// Resets the entire data layer (database) to an empty state.
    /// </summary>
    public void ResetDB();

    /// <summary>
    /// Initializes the data layer (database) with testing data.
    /// </summary>
    public void InitializeDB();

    /// <summary>
    /// Retrieves the current system clock time.
    /// </summary>
    /// <returns>The current DateTime value of the system clock.</returns>
    public DateTime GetClock();

    /// <summary>
    /// Advances the system clock by a specified time unit.
    /// </summary>
    /// <param name="timeUnit">The unit and quantity of time to advance.</param>
    public void ForwardClock(TimeUnit timeUnit);

    /// <summary>
    /// Retrieves the current general system configuration variables.
    /// </summary>
    /// <returns>A BO.Config object containing system settings.</returns>
    public BO.Config GetConfig();

    /// <summary>
    /// Updates the general system configuration variables.
    /// </summary>
    /// <param name="config">The BO.Config object containing the updated settings.</param>
    public void SetConfig(BO.Config config);

    bool CheackEnter(int id, string password);


    #region Stage 5
    void AddConfigObserver(Action configObserver);
    void RemoveConfigObserver(Action configObserver);
    void AddClockObserver(Action clockObserver);
    void RemoveClockObserver(Action clockObserver);
    #endregion Stage 5
}