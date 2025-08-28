using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WatchdogControl.Interfaces;
using WatchdogControl.ViewModels;

namespace WatchdogControl.Services;

public class MyBackgroundService(MainWindowViewModel mainWindowViewModel, ILoggingService<MyBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

                mainWindowViewModel.CurrentDateTime = DateTime.Now;

                if (!mainWindowViewModel.IsPause)
                    mainWindowViewModel.TimeUntilUpdate--;

                if (mainWindowViewModel.TimeUntilUpdate > 0)
                    continue;

                mainWindowViewModel.TimeUntilUpdate = 30;

                // используется ToLIst() для создания копии списка Watchdogs,
                // чтобы при изменении элемента коллекции во время выполнения этого цикла
                // не возникало ошибки "Collection was modified; enumeration operation may not execute"
                foreach (var watchdog in mainWindowViewModel.Watchdogs.ToList())
                {
                    mainWindowViewModel.UpdateWatchdog(watchdog);
                }

                mainWindowViewModel.UpdateProgress = 0;
            }
            catch (Exception ex)
            {
                logger.Logger.LogError($"Ошибка в {nameof(MyBackgroundService)}: {ex.Message}");
            }
        }
    }
}