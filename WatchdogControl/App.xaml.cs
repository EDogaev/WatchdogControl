using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using WatchdogControl.Services;

namespace WatchdogControl
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary> Переменная, указывающая, что приложение находится в Design-time </summary>
        public static bool IsDesignMode { get; private set; } = true;

        private readonly ILogger _logger = Bootstrapper.Container.GetRequiredService<ILogger<App>>();

        protected override void OnStartup(StartupEventArgs e)
        {
            AppService.OnStartup();

            // выходим из Design-time (переходим в Run-time)
            IsDesignMode = false;

            _logger.LogInformation("Запуск приложения");

            Bootstrapper.RunApp();

            // перехват необработанных ошибок
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                _logger.LogInformation($"Ошибка: {ex.Message}");
            };

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                _logger.LogInformation($"Ошибка: {e.Exception.Message}");
            };


            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogInformation("Завершение приложения");
            AppService.OnExit();
            base.OnExit(e);
        }
    }
}
