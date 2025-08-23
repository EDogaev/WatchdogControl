using System.IO;
using Utilities;
using WatchdogControl.Enums;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.Watchdog;

namespace WatchdogControl.Services
{
    internal class ManageWatchdogByXml : IWatchdogManager
    {
        private const string WatchdogsFolder = "Watchdogs";
        private static string WatchdogsPath => Path.Combine(Directory.GetCurrentDirectory(), WatchdogsFolder);

        public IEnumerable<Watchdog> Load()
        {
            var watchdogs = new List<Watchdog>();

            if (!Directory.Exists(WatchdogsPath))
                return watchdogs;

            foreach (var filePath in Directory.GetFiles(WatchdogsPath))
            {
                try
                {
                    var watchdog = XmlSerializer<Watchdog>.LoadObject(filePath);
                    watchdog.FilePath = filePath;
                    watchdog.Name = Path.GetFileNameWithoutExtension(filePath);
                    watchdog.PreviousName = watchdog.Name;
                    watchdogs.Add(watchdog);
                }
                catch (Exception ex)
                {
                    var err = $"Ошибка при извлечении данных из {filePath}: \n{ex.Message}";
                    Messages.ShowMsgErr(err, true);
                    MemoryLogService.Add(err, WarningType.Error);
                }
            }

            return watchdogs;
        }

        public bool Save(Watchdog watchdog)
        {
            try
            {
                if (!Directory.Exists(WatchdogsPath))
                    Directory.CreateDirectory(WatchdogsPath);

                var filePath = Path.Combine(WatchdogsFolder, watchdog.Name);

                // удалить предыдущий файл (возможно, что наименование Watchdog было изменено)
                Remove(watchdog);
                // обновить путь к файлу
                watchdog.FilePath = filePath;
                watchdog.PreviousName = watchdog.Name;

                XmlSerializer<Watchdog>.SaveObject(watchdog, filePath);

                return true;
            }
            catch (Exception ex)
            {
                var err = $"[{watchdog}] Ошибка при сохранении: \n{ex.Message}";
                Messages.ShowMsgErr(err, true);
                MemoryLogService.Add(err, WarningType.Error);

                return false;
            }
        }

        public bool Remove(Watchdog watchdog)
        {
            try
            {
                if (File.Exists(watchdog.FilePath))
                    File.Delete(watchdog.FilePath);

                return true;
            }
            catch (Exception ex)
            {
                var err = $"[{watchdog}] Ошибка при удалении: \n{ex.Message}";

                Messages.ShowMsgErr(err, true);
                MemoryLogService.Add(err, WarningType.Error);

                return false;
            }
        }
    }
}
