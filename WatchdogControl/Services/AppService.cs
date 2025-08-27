using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WatchdogControl.Services;

public static class AppService
{
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

    public static void OnStartup()
    {
        const string mutexName = "WatchdogControlMutex";

        _mutex = new Mutex(true, mutexName, out var createdNew);

        if (!createdNew)
        {
            ActivateExistingInstance();
            Environment.Exit(0);
            return;
        }

    }

    public static void OnExit()
    {
        _mutex?.ReleaseMutex();
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