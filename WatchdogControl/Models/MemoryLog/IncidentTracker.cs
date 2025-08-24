using System.Collections.Specialized;
using WatchdogControl.Interfaces;

namespace WatchdogControl.Models.MemoryLog
{
    /// <summary> Отслеживание состояния записей в логе (наблюдение за ошибками и предупреждениями) </summary>
    internal class IncidentTracker : IIncidentTracker, IDisposable
    {
        private readonly IMemoryLogStore _memoryLogStore;

        /// <summary> Список не квитированных ошибок </summary>
        private readonly List<MemoryLog> _activeErrors = [];

        /// <summary> Список не квитированных предупреждений </summary>
        private readonly List<MemoryLog> _activeWarnings = [];

        public IReadOnlyList<MemoryLog> ActiveErrors => _activeErrors;

        public IReadOnlyList<MemoryLog> ActiveWarnings => _activeWarnings;

        /// <summary> Событие при изменении списков не квитированных ошибок и предупреждений </summary>
        public event EventHandler Changed;

        public IncidentTracker(IMemoryLogStore memoryLogStore)
        {
            _memoryLogStore = memoryLogStore;
            _memoryLogStore.Logs.CollectionChanged += LogCollectionChanged;
        }

        /// <summary> Событие при изменении коллекции </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
                return;

            foreach (MemoryLog log in e.NewItems)
            {
                if (log.IsActiveWarning)
                    _activeWarnings.Add(log);
                if (log.IsActiveError)
                    _activeErrors.Add(log);
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void ResetActiveError(MemoryLog log)
        {
            if (!_activeErrors.Contains(log))
                return;

            log.IsActiveError = false;
            _activeErrors.Remove(log);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void ResetActiveWarning(MemoryLog log)
        {
            if (!_activeWarnings.Contains(log))
                return;

            log.IsActiveWarning = false;
            _activeWarnings.Remove(log);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void ResetAllIncidents()
        {
            foreach (var activeError in _activeErrors.ToList())
            {
                ResetActiveError(activeError);
            }

            foreach (var activeWarning in _activeWarnings.ToList())
            {
                ResetActiveWarning(activeWarning);
            }
        }

        public void Dispose()
        {
            _memoryLogStore.Logs.CollectionChanged -= LogCollectionChanged;
        }
    }
}
