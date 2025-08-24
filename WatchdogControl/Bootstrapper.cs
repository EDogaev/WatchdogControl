using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.MemoryLog;
using WatchdogControl.Models.Watchdog;
using WatchdogControl.Services;
using WatchdogControl.ViewModels;
using WatchdogControl.Views;

namespace WatchdogControl
{
    internal static class Bootstrapper
    {
        public static readonly IServiceProvider Container = ConfigureServices();

        public static void RunApp()
        {
            // создать главное окно
            var mainWindow = Container.GetRequiredService<MainWindow>();
            // и запустить
            mainWindow.Show();
        }

        /// <summary> Сконфигурировать IoC-контейнер </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddLogging(builder => builder.AddSerilog(CreateLogger(), dispose: true))
                .AddSingleton<IFilterMemoryLog, FilterMemoryLog>()
                .AddSingleton<IMemoryLogStore, MemoryLogStore>()
                .AddSingleton<IIncidentTracker, IncidentTracker>()
                // работать с данными из БД Sqlite (Watchdogs.db)
                .AddSingleton<IWatchdogManager, ManageWatchdogBySqlite>()
                // работать с данными из папки Watchdogs (XML-файлы)
                //.AddSingleton<IWatchdogManager, ManageWatchdogByXml>()
                .AddSingleton<IWatchdogFactory, WatchdogFactory>()
                .AddSingleton<MemoryLogViewModel>()
                .AddSingleton<MainWindowViewModel>()
                .AddSingleton<MainWindow>()
                .AddTransient<Watchdog>();

            return services.BuildServiceProvider();
        }

        private static ILogger CreateLogger()
        {
            var logPath = $@"Logs\{Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName)}_.log";

            return new LoggerConfiguration()
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .WriteTo.File(logPath,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Hour,
                    retainedFileCountLimit: 6)
                .CreateLogger();
        }
    }
}
