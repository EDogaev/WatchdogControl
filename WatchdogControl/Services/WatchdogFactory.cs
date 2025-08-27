using Microsoft.Extensions.DependencyInjection;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.Watchdog;

namespace WatchdogControl.Services
{
    internal class WatchdogFactory(IServiceProvider serviceProvider) : IWatchdogFactory
    {
        public Watchdog CreateWatchdog()
        {
            return serviceProvider.GetRequiredService<Watchdog>();
        }
    }
}
