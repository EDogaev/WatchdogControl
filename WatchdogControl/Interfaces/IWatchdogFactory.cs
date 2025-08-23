using WatchdogControl.Models.Watchdog;

namespace WatchdogControl.Interfaces
{
    public interface IWatchdogFactory
    {
        Watchdog CreateWatchdog();
    }
}
