using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
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

        /// <summary> Сконфигурировать DI-контейнер </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            ConfigureLogger(services);
            ConfigureMemoryLog(services);
            ConfigureWatchdog(services);
            ConfigureViewsAndViewModels(services);

            return services.BuildServiceProvider();
        }

        /// <summary> Сконфигурировать логирование </summary>
        /// <param name="services"></param>
        private static void ConfigureLogger(IServiceCollection services)
        {
            var logPath = $@"Logs\{Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName)}_.log";

            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .WriteTo.File(logPath,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Month,
                    retainedFileCountLimit: 6)
                .CreateLogger();

            services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));
            services.AddSingleton(typeof(ILoggingService<>), typeof(LoggingService<>));

        }

        /// <summary> Регистрация классов для работы с логированием в память (синхронник) </summary>
        /// <param name="services"></param>
        private static void ConfigureMemoryLog(IServiceCollection services)
        {
            services.AddSingleton<IFilterMemoryLog, FilterMemoryLog>();
            services.AddSingleton<IMemoryLogStore, MemoryLogStore>();
            services.AddSingleton<IIncidentTracker, IncidentTracker>();
        }

        /// <summary> Регистрация классов для работы с Watchdog </summary>
        /// <param name="services"></param>
        private static void ConfigureWatchdog(IServiceCollection services)
        {
            services.AddSingleton<IWatchdogFactory, WatchdogFactory>();
            // работать с данными из БД Sqlite (Watchdogs.db)
            services.AddSingleton<IWatchdogManager, ManageWatchdogBySqlite>();
            // работать с данными из папки Watchdogs (XML-файлы)
            //.AddSingleton<IWatchdogManager, ManageWatchdogByXml>()
            services.AddTransient<Watchdog>();

        }

        /// <summary> Регистрация представлений и моделей-представлений </summary>
        /// <param name="services"></param>
        private static void ConfigureViewsAndViewModels(IServiceCollection services)
        {
            services.AddTransient<EditWatchdogWindowFactory>();
            services.AddSingleton<MemoryLogViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
        }
    }
}
