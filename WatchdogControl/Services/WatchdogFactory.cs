using Microsoft.Extensions.DependencyInjection;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.Watchdog;

namespace WatchdogControl.Services
{
    internal class WatchdogFactory : IWatchdogFactory
    {
        private IServiceProvider _serviceProvider;

        public WatchdogFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Watchdog CreateWatchdog()
        {
            return _serviceProvider.GetRequiredService<Watchdog>();
        }
    }
}
