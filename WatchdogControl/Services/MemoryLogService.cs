using System.Windows;
using WatchdogControl.Enums;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.MemoryLog;

namespace WatchdogControl.Services
{
    public static class MemoryLogService
    {
        private static IMemoryLogStore _memoryLogStore;

        /// <summary>
        /// Коллекция, в которой будут храниться сообщения
        /// </summary>
        /// <param name="memoryLog"></param>
        public static void SetMemoryLog(IMemoryLogStore memoryLog)
        {
            _memoryLogStore = memoryLog;
        }

        /// <summary>
        /// Добавить сообщение в коллекцию
        /// </summary>
        /// <param name="text"></param>
        /// <param name="warningType"></param>
        public static void Add(string text, WarningType warningType)
        {
            Application.Current.Dispatcher.Invoke(() => _memoryLogStore?.Add(new MemoryLog($"{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff} {text}", warningType)));
        }
    }
}
