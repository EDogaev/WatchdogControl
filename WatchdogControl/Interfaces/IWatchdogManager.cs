using WatchdogControl.Models.Watchdog;

namespace WatchdogControl.Interfaces
{
    /// <summary>
    /// Интерфейс для управления Watchdog`ом (загрузка списка Watchdog, сохранение и удаление Watchdog)
    /// </summary>
    public interface IWatchdogManager
    {
        IEnumerable<Watchdog> Load();
        bool Save(Watchdog watchdog);
        bool Remove(Watchdog watchdog);
        Task<bool> TestWatchDogDbData(Watchdog watchdog);
        void GetWatchdogData(Watchdog watchdog);
    }
}
