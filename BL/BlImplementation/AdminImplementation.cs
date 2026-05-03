using BlApi;
using BO;
using Helpers;
using System.Threading.Tasks;

namespace BlImplementation;

internal class AdminImplementation : IAdmin
{
    public async Task ResetDBAsync()
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        await Helpers.AdminManager.ResetDBAsync();
    }

    public async Task InitializeDBAsync()
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        await Helpers.AdminManager.InitializeDBAsync();
    }

    public DateTime GetClock()
    {
        return Helpers.AdminManager.Now;
    }

    public void ForwardClock(TimeUnit timeUnit)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        DateTime current = Helpers.AdminManager.Now;

        DateTime newTime = Helpers.AdminManager.ForwardClock(current, timeUnit);
        Helpers.AdminManager.UpdateClock(newTime);
    }
    public BO.Config GetConfig()
    {
        return Helpers.AdminManager.GetConfig();
    }

    public async Task SetConfigAsync(BO.Config config)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        await Helpers.AdminManager.SetConfigAsync(config);
    }

    public bool CheackEnter(int id, string password)
    {
        return Helpers.AdminManager.CheackEnter(id, password);
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
    #region Stage 7

    public void StartSimulator(int interval)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.Start(interval); //stage 7
    }

    public void StopSimulator()
        => AdminManager.Stop(); //stage 7

    #endregion Stage 7
}