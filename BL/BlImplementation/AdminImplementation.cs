using BlApi;
using BO;

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
}