using BlApi;
using BO;
using Helpers;

namespace BlImplementation;

internal class AdminImplementation : IAdmin
{
    public void ResetDB()
    {
        Helpers.AdminManager.ResetDB();
    }

    public void InitializeDB()
    {
        Helpers.AdminManager.InitializeDB();
    }

    public DateTime GetClock()
    {
        return Helpers.AdminManager.Now;
    }

    public void ForwardClock(TimeUnit timeUnit)
    {
        DateTime current = Helpers.AdminManager.Now;

        DateTime newTime = Helpers.AdminManager.ForwardClock(current, timeUnit);
        Helpers.AdminManager.UpdateClock(newTime);
    }
    public BO.Config GetConfig()
    {
        return Helpers.AdminManager.GetConfig();
    }

    public void SetConfig(BO.Config config)
    {
        Helpers.AdminManager.SetConfig(config);
    }

    #region Stage 5
    public void AddClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers += clockObserver;
    public void RemoveClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers -= clockObserver;
    public void AddConfigObserver(Action configObserver) =>
   AdminManager.ConfigUpdatedObservers += configObserver;
    public void RemoveConfigObserver(Action configObserver) =>
    AdminManager.ConfigUpdatedObservers -= configObserver;
    #endregion Stage 5
}