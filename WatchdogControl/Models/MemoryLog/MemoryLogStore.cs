using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using WatchdogControl.Interfaces;

namespace WatchdogControl.Models.MemoryLog
{
    internal class MemoryLogStore : IMemoryLogStore
    {
        public ObservableCollection<MemoryLog> Logs { get; } = new ObservableCollection<MemoryLog>();

        public ICollectionView LogsView { get; }
        public MemoryLogStore(IFilterMemoryLog filter)
        {
            LogsView = CollectionViewSource.GetDefaultView(Logs);
            LogsView.Filter = filter.Filter;
        }

        public void Add(MemoryLog log)
        {
            Logs.Add(log);
        }
    }
}
