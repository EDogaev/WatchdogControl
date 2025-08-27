using WatchdogControl.Interfaces;
using WatchdogControl.Models.Watchdog;
using WatchdogControl.ViewModels;
using WatchdogControl.Views;

namespace WatchdogControl.Services;

public class EditWatchdogWindowFactory(IWatchdogManager manager)
{
    public EditWatchdogView Create(Watchdog watchdog)
    {
        var vm = new EditWatchdogViewModel(watchdog, manager);
        return new EditWatchdogView(vm);
    }
}