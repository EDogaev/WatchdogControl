using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using WatchdogControl.Enums;
using WatchdogControl.Interfaces;

namespace WatchdogControl.Models.MemoryLog
{
    internal class MemoryLogStore : IMemoryLogStore
    {
        public ObservableCollection<MemoryLog> Logs { get; } = [];

        public ICollectionView View { get; }
        public MemoryLogStore(IFilterMemoryLog filter)
        {
            View = CollectionViewSource.GetDefaultView(Logs);
            View.Filter = filter.Filter;
        }

        public void Add(string mess, WarningType warningType)
        {
            Application.Current.Dispatcher.Invoke(() => Logs.Add(new MemoryLog($"{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff} {mess}", warningType)));
        }
    }
}
