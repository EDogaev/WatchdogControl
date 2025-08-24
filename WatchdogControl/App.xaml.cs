using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

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

        // Импорт функций WinAPI для работы с окнами
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private static Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "WatchdogControlMutex";

            _mutex = new Mutex(true, mutexName, out var createdNew);

            if (!createdNew)
            {
                ActivateExistingInstance();
                Environment.Exit(0);
                return;
            }

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
            _mutex?.ReleaseMutex();
            base.OnExit(e);
        }

        private static void ActivateExistingInstance()
        {
            var current = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id == current.Id || process.MainWindowHandle == IntPtr.Zero)
                    continue;

                // Если окно свернуто - восстановить
                if (IsIconic(process.MainWindowHandle))
                {
                    ShowWindow(process.MainWindowHandle, SW_RESTORE);
                }

                // Активировать окно
                SetForegroundWindow(process.MainWindowHandle);
                break;
            }
        }
    }
}
