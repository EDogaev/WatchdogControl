using System.Collections.ObjectModel;
using System.ComponentModel;
using WatchdogControl.Enums;
using WatchdogControl.Models.MemoryLog;

namespace WatchdogControl.Interfaces
{
    /// <summary> Интерфейс для работы с логом, хранящимся в памяти </summary>
    public interface IMemoryLogStore
    {
        /// <summary> Коллекция для хранения записей </summary>
        ObservableCollection<MemoryLog> Logs { get; }

        /// <summary> View для отображения записей </summary>
        ICollectionView LogsView { get; }

        /// <summary> Добавление записи </summary>
        /// <param name="mess"></param>
        /// <param name="warningType"></param>
        void Add(string mess, WarningType warningType);
    }
}
