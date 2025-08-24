using Utilities;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.MemoryLog;
using WatchdogControl.RealizedInterfaces;

namespace WatchdogControl.ViewModels
{
    public class MemoryLogViewModel : NotifyPropertyChanged
    {
        /// <summary> Трекер отслеживания ошибок и предупреждений </summary>
        private readonly IIncidentTracker _incidentTracker;

        private MemoryLog _selectedItem;

        /// <summary> Фильтр отслеживания ошибок и предупреждений </summary>
        public IFilterMemoryLog Filter { get; }

        /// <summary> Список сообщений </summary>
        public IMemoryLogStore MemoryLogStore { get; }

        /// <summary> Кол-во не квитированных ошибок </summary>
        public int ActiveErrorsCount => _incidentTracker.ActiveErrors.Count;

        /// <summary> Кол-во не квитированных предупреждений </summary>
        public int ActiveWarningsCount => _incidentTracker.ActiveWarnings.Count;

        /// <summary> Наличие не квитированных ошибок </summary>
        public bool HasErrors => ActiveErrorsCount > 0;

        /// <summary> Наличие не квитированных предупреждений </summary>
        public bool HasWarnings => ActiveWarningsCount > 0;

        /// <summary> Квитирование всех ошибок и предупреждений </summary>
        public RelayCommand<string> ResetAllIncidentCommand { get; }

        public MemoryLogViewModel(IMemoryLogStore memoryLogStore, IFilterMemoryLog filterMemoryLog, IIncidentTracker incidentTracker)
        {
            Filter = filterMemoryLog;
            Filter.Changed += RefreshView;
            MemoryLogStore = memoryLogStore;

            //MemoryLogService.SetMemoryLog(MemoryLogStore);


            _incidentTracker = incidentTracker;
            _incidentTracker.Changed += RefreshAll;

            ResetAllIncidentCommand = new RelayCommand<string>((_) =>
            {
                if (!Messages.ShowMsgQstn("Сбросить все предупреждения и ошибки?"))
                    return;

                _incidentTracker.ResetAllIncidents();
            }, (_) => ActiveErrorsCount > 0 || ActiveWarningsCount > 0);
        }

        public MemoryLog SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (Equals(value, _selectedItem))
                    return;

                _selectedItem = value;

                if (_selectedItem == null)
                    return;

                // если выделенное сообщение помечено как ошибка, то сбросить пометку
                if (_selectedItem.IsActiveError)
                {
                    _incidentTracker.ResetActiveError(_selectedItem);
                }

                // если выделенное сообщение помечено как предупреждение, то сбросить пометку
                if (_selectedItem.IsActiveWarning)
                {
                    _incidentTracker.ResetActiveWarning(_selectedItem);
                }
            }
        }

        private void RefreshView()
        {
            MemoryLogStore.View.Refresh();
        }

        private void RefreshAll(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(ActiveWarningsCount));
            OnPropertyChanged(nameof(HasWarnings));
            OnPropertyChanged(nameof(ActiveErrorsCount));
            OnPropertyChanged(nameof(HasErrors));
            RefreshView();
        }
    }
}
