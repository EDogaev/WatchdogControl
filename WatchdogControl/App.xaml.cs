using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Utilities;

namespace WatchdogControl
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary> Переменная, указывающая, что приложение находится в Design-time </summary>
        public static bool IsDesignMode { get; private set; } = true;

        // Импорт функций WinAPI для работы с окнами
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private static Mutex _mutex;

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

            Logger.LogToFile("Запуск приложения");

            var bootstrapper = new Bootstrapper();
            bootstrapper.RunApp();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.LogToFile("Завершение приложения");
            _mutex.ReleaseMutex();
            base.OnExit(e);
        }

        private static void ActivateExistingInstance()
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id && process.MainWindowHandle != IntPtr.Zero)
                {
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
}
