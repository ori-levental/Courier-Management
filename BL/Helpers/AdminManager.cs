using BO;
using DalApi;
using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{
    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; } //stage 4

    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers

    private static Task? _periodicTask = null; //stage 7

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage 4-7
    {
        var oldClock = s_dal.Config.Clock; //stage 4
        s_dal.Config.Clock = newClock; //stage 4

        // --- Logic for Stage 4 Implementation ---
        // 1. Update Order Statuses (Simulate delivery progress)
        Helpers.OrderManager.PeriodicOrdersUpdate(oldClock, newClock);

        // 2. Deactivate Idle Couriers (Cleanup logic)
        Helpers.CourierManager.DeactivateIdleCouriers();
        // ----------------------------------------

        //Add calls here to any logic method that should be called periodically,
        //after each clock update
        //for example, Periodic students' updates:
        // - Go through all students to update properties that are affected by the clock update
        // - (students become not active after 5 years etc.)

        //TO_DO: //stage 4
        //   StudentManager.PeriodicStudentsUpdates(oldClock, newClock); //stage 4. to be removed in stage 7 and replaced as below
        //...

        //TO_DO: //stage 7
        //if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
        //    _periodicTask = Task.Run(() => StudentManager.PeriodicStudentsUpdates(oldClock, newClock));
        //...

        //Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke(); //prepared for stage 5
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
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static BO.Config GetConfig() //stage 4
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
    /// Method for setting current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static void SetConfig(BO.Config configuration) //stage 4
    {
        bool configChanged = false; // stage 5

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
        if (s_dal.Config.CompanyAddress != configuration.CompanyAddress)
        {
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

        //TO_DO: //stage 4
        //add a condition+assignment for each configuration property
        //...

        //Calling all the observers of configuration update
        if (configChanged) // stage 5
            ConfigUpdatedObservers?.Invoke(); // stage 5
    }

    internal static void ResetDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
            AdminManager.SetConfig(AdminManager.GetConfig()); //stage 5 - needed to update PL 
        }
    }

    internal static void InitializeDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            DalTest.Initialization.Do(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated            
            AdminManager.SetConfig(AdminManager.GetConfig()); //stage 5 - needed for update the PL
        }
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks
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
    /// 
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                                
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BlNotNullableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                                
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                                
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

    private static Task? _simulateTask = null;

    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            //TO_DO: //stage 7
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation

            //etc...

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) { }
        }
    }

    #endregion Stage 7 base
}