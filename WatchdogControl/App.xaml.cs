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
        public static bool IsDesignMode { get; } = Application.Current is not App;

        private IHost _host;
        private ILogger _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppService.OnStartup();
            base.OnStartup(e);

            CreateHost();

            _logger = _host.Services.GetRequiredService<ILogger<App>>();

            _logger.LogInformation("Запуск приложения");

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
            _logger.LogInformation("Завершение приложения");
            AppService.OnExit();
            await _host.StopAsync();
            base.OnExit(e);
        }

        private async Task CreateHost()
        {
            _host = Host.CreateDefaultBuilder()
                .UseSerilog((_, config) =>
                {
                    config
                        .MinimumLevel.Verbose()
                        .Enrich.WithProperty("MachineName", Environment.MachineName)
                        .WriteTo.File(
                            path: $@"Logs\{System.Diagnostics.Process.GetCurrentProcess().ProcessName}_.log",
                            outputTemplate:
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] {Message:lj}{NewLine}{Exception}",
                            rollingInterval: RollingInterval.Month,
                            retainedFileCountLimit: 6);
                })
                .ConfigureServices((_, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();

            await _host.StartAsync();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // === логирование ===
            services.AddSingleton(typeof(ILoggingService<>), typeof(LoggingService<>));

            // === Memory Log ===
            services.AddSingleton<IFilterMemoryLog, FilterMemoryLog>();
            services.AddSingleton<IMemoryLogStore, MemoryLogStore>();
            services.AddSingleton<IIncidentTracker, IncidentTracker>();

            // === Watchdog ===
            services.AddSingleton<IWatchdogFactory, WatchdogFactory>();
            // работать с данными из БД Sqlite (Watchdogs.db)
            services.AddSingleton<IWatchdogManager, ManageWatchdogBySqlite>();
            // работать с данными из папки Watchdogs (XML-файлы)
            //.AddSingleton<IWatchdogManager, ManageWatchdogByXml>()
            services.AddTransient<Watchdog>();

            // === View-model и View
            services.AddTransient<EditWatchdogWindowFactory>();
            services.AddSingleton<MemoryLogViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();

            // === фоновые задачи ===
            services.AddHostedService<MyBackgroundService>();
        }
    }
}
