using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.MemoryLog;
using WatchdogControl.Models.Watchdog;
using WatchdogControl.Services;
using WatchdogControl.ViewModels;
using WatchdogControl.Views;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WatchdogControl
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary> Переменная, указывающая, что приложение находится в Design-time </summary>
        public static bool IsDesignMode { get; } = Current is not App;

        private IHost _host;
        private ILogger _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppService.OnStartup();
            base.OnStartup(e);

            CreateHost();

            _logger = _host.Services.GetRequiredService<ILogger<App>>();

            // перехват необработанных ошибок
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                _logger.LogError($"Ошибка: {ex.Message}");
            };

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                _logger.LogError($"Ошибка: {e.Exception.Message}");
            };

            // создать главное окно и запустить
            _host.Services.GetRequiredService<MainWindow>().Show();

        }

        protected override async void OnExit(ExitEventArgs e)
        {
            AppService.OnExit();
            await _host.StopAsync();
            base.OnExit(e);
        }

        private void CreateHost()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((_, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .UseSerilog((_, config) =>
                {
                    config
                        .MinimumLevel.Verbose()
                        .Enrich.WithProperty("MachineName", Environment.MachineName)
                        .WriteTo.File(
                            path: $@"Logs\{System.Diagnostics.Process.GetCurrentProcess().ProcessName}_.log",
                            outputTemplate:
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{MachineName}] {Message:lj}{NewLine}{Exception}",
                            rollingInterval: RollingInterval.Month,
                            retainedFileCountLimit: 6);
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services, context.Configuration);
                })
                .Build();

            _host.Start();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // === логирование ===
            services.AddSingleton(typeof(ILoggingService<>), typeof(LoggingService<>));

            // === Memory Log ===
            services.AddSingleton<IFilterMemoryLog, FilterMemoryLog>();
            services.AddSingleton<IMemoryLogStore, MemoryLogStore>();
            services.AddSingleton<IIncidentTracker, IncidentTracker>();

            // === Watchdog ===
            services.AddSingleton<IWatchdogFactory, WatchdogFactory>();
            services.AddTransient<Watchdog>();
            SetWatchdogManager(services, configuration);

            // === View-model и View
            services.AddTransient<EditWatchdogWindowFactory>();
            services.AddSingleton<MemoryLogViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();

            // === фоновые задачи ===
            services.AddHostedService<MyBackgroundService>();
        }

        private static void SetWatchdogManager(IServiceCollection services, IConfiguration configuration)
        {
            Enum.TryParse(configuration.GetValue<string>("WatchdogStorage:Storage"), ignoreCase: true, out WatchdogStorage storage);
            switch (storage)
            {
                case WatchdogStorage.Sqlite:
                    // работать с данными из БД Sqlite (Watchdogs.db)
                    services.AddSingleton<IWatchdogManager, ManageWatchdogBySqlite>();
                    break;
                case WatchdogStorage.Xml:
                    // работать с данными из папки Watchdogs (XML-файлы)
                    services.AddSingleton<IWatchdogManager, ManageWatchdogByXml>();
                    break;
                default:
                    services.AddSingleton<IWatchdogManager, ManageWatchdogBySqlite>();
                    break;
            }
        }
    }
}
