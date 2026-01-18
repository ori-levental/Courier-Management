using BlImplementation;
using BO;
using DalApi;
using System.Runtime.CompilerServices;
using System.Threading.Tasks; // Required for Task

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager
{
    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get;

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; }

    internal static event Action? ConfigUpdatedObservers;
    internal static event Action? ClockUpdatedObservers;

    private static Task? _periodicTask = null;

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock)
    {
        var oldClock = s_dal.Config.Clock;
        s_dal.Config.Clock = newClock;

        // --- Logic for Stage 4 Implementation ---
        // 1. Update Order Statuses (Simulate delivery progress)
        // This remains synchronous as it performs calculations, not network requests
        Helpers.OrderManager.PeriodicOrdersUpdate(oldClock, newClock);

        // 2. Deactivate Idle Couriers (Cleanup logic)
        Helpers.CourierManager.DeactivateIdleCouriers();
        // ----------------------------------------

        // Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke();
    }

    /// <summary>
    /// Helper method to calculate new time based on TimeUnit enum
    /// </summary>
    internal static DateTime ForwardClock(DateTime current, BO.TimeUnit timeUnit)
    {
        return timeUnit switch
        {
            TimeUnit.Minute => current.AddMinutes(1),
            TimeUnit.Hour => current.AddHours(1),
            TimeUnit.Day => current.AddDays(1),
            TimeUnit.Month => current.AddMonths(1),
            TimeUnit.Year => current.AddYears(1),
            _ => current.AddHours(0)
        };
    }

    /// <summary>
    /// Method for providing current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] // Safe to keep here (Sync method)
    internal static BO.Config GetConfig()
    {
        return new BO.Config()
        {
            // Basic Settings
            MaxRange = s_dal.Config.MaxAirDistance,
            Clock = s_dal.Config.Clock,
            CompanyAddress = s_dal.Config.CompanyAddress,
            ManagerPassword = s_dal.Config.ManagerPassword,
            ManagerId = s_dal.Config.ManagerId,

            // Speeds
            AvgCarSpeed = s_dal.Config.AvgCarSpeed,
            AvgMotorcycleSpeed = s_dal.Config.AvgMotorcycleSpeed,
            AvgBicycleSpeed = s_dal.Config.AvgBicycleSpeed,
            AvgWalkSpeed = s_dal.Config.AvgWalkSpeed,

            // Times (SLA)
            MaxDeliveryTime = s_dal.Config.MaxDeliveryTime,
            RiskRange = s_dal.Config.RiskRange,
            CourierInactivityTime = s_dal.Config.CourierInactivityTime
        };
    }

    /// <summary>
    /// Async Method for setting current configuration.
    /// MUST be Async because it calls Tools.GetCoordinatesAsync (Network Request).
    /// </summary>
    internal static async Task SetConfigAsync(BO.Config configuration)
    {
        bool configChanged = false;

        // --- Basic Settings ---
        if (s_dal.Config.MaxAirDistance != configuration.MaxRange)
        {
            s_dal.Config.MaxAirDistance = configuration.MaxRange;
            configChanged = true;
        }
        if (s_dal.Config.Clock != configuration.Clock)
        {
            s_dal.Config.Clock = configuration.Clock;
            configChanged = true;
        }
        if (s_dal.Config.ManagerPassword != configuration.ManagerPassword)
        {
            s_dal.Config.ManagerPassword = configuration.ManagerPassword;
            configChanged = true;
        }

        // --- Company Address - Get Coordinates Automatically (ASYNC) ---
        if (s_dal.Config.CompanyAddress != configuration.CompanyAddress)
        {
            // Automatically get coordinates from the new address
            if (!string.IsNullOrWhiteSpace(configuration.CompanyAddress))
            {
                try
                {
                    // UPDATE: Using 'await' for the network request
                    var coords = await Helpers.Tools.GetCoordinatesAsync(configuration.CompanyAddress);

                    if (coords != null)
                    {
                        s_dal.Config.Latitude = coords.Value.Latitude;
                        s_dal.Config.Longitude = coords.Value.Longitude;
                    }
                    else
                    {
                        // Handle failure (keep old or set to 0)
                        s_dal.Config.Longitude = s_dal.Config.Latitude = 0;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
                s_dal.Config.Longitude = s_dal.Config.Latitude = 0;


            s_dal.Config.CompanyAddress = configuration.CompanyAddress;
            configChanged = true;
        }

        // --- Speeds ---
        if (s_dal.Config.AvgCarSpeed != configuration.AvgCarSpeed)
        {
            s_dal.Config.AvgCarSpeed = configuration.AvgCarSpeed;
            configChanged = true;
        }
        if (s_dal.Config.AvgMotorcycleSpeed != configuration.AvgMotorcycleSpeed)
        {
            s_dal.Config.AvgMotorcycleSpeed = configuration.AvgMotorcycleSpeed;
            configChanged = true;
        }
        if (s_dal.Config.AvgBicycleSpeed != configuration.AvgBicycleSpeed)
        {
            s_dal.Config.AvgBicycleSpeed = configuration.AvgBicycleSpeed;
            configChanged = true;
        }
        if (s_dal.Config.AvgWalkSpeed != configuration.AvgWalkSpeed)
        {
            s_dal.Config.AvgWalkSpeed = configuration.AvgWalkSpeed;
            configChanged = true;
        }

        // --- Times (SLA) ---
        if (s_dal.Config.MaxDeliveryTime != configuration.MaxDeliveryTime)
        {
            s_dal.Config.MaxDeliveryTime = configuration.MaxDeliveryTime;
            configChanged = true;
        }
        if (s_dal.Config.RiskRange != configuration.RiskRange)
        {
            s_dal.Config.RiskRange = configuration.RiskRange;
            configChanged = true;
        }
        if (s_dal.Config.CourierInactivityTime != configuration.CourierInactivityTime)
        {
            s_dal.Config.CourierInactivityTime = configuration.CourierInactivityTime;
            configChanged = true;
        }

        // Calling all the observers of configuration update
        if (configChanged)
            ConfigUpdatedObservers?.Invoke();
    }

    internal static async Task ResetDBAsync()
    {
        // 1. Reset DAL (Synchronous operation, inside lock)
        lock (BlMutex)
        {
            s_dal.ResetDB();
            AdminManager.UpdateClock(AdminManager.Now);
        }

        // 2. Set Config (Async operation, MUST be outside lock)
        // We cannot use 'await' inside a lock statement.
        await AdminManager.SetConfigAsync(AdminManager.GetConfig());

        // 3. Notify Observers
        Helpers.CourierManager.Observers.NotifyListUpdated();
        Helpers.OrderManager.Observers.NotifyListUpdated();
    }

    internal static async Task InitializeDBAsync()
    {
        // 1. Initialize DAL (Synchronous operation, inside lock)
        lock (BlMutex)
        {
            DalTest.Initialization.Do();
            AdminManager.UpdateClock(AdminManager.Now);
        }

        // 2. Set Config (Async operation, MUST be outside lock)
        await AdminManager.SetConfigAsync(AdminManager.GetConfig());

        // 3. Notify Observers
        Helpers.CourierManager.Observers.NotifyListUpdated();
        Helpers.OrderManager.Observers.NotifyListUpdated();
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>     
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new();
    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;
    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start())     
    /// </summary>
    private static int s_interval = 1;
    /// <summary>
    /// The flag that signs whether simulator is running
    /// </summary>
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BlNotNullableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(ClockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    private static void ClockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) { }
        }
    }

    public static bool CheackEnter(int id, string password)
    {
        if (id == s_dal.Config.ManagerId && password == s_dal.Config.ManagerPassword)
            return true;
        return false;
    }

    #endregion Stage 7 base
}