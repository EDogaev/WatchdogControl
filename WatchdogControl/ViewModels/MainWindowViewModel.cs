using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Utilities;
using WatchdogControl.Enums;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.Watchdog;
using WatchdogControl.RealizedInterfaces;
using WatchdogControl.Services;

namespace WatchdogControl.ViewModels
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {
        private DateTime _currentDateTime;
        private Watchdog? _selectedWatchdog;
        private int _timeUntilUpdate;
        private int _updateProgress;
        private Watchdog? _currentUpdatingWatchdog;
        private bool _isPause;
        private bool _correctPasswordInputed;
        private int _countInWork;

        private readonly ILoggingService<MainWindowViewModel> _logService;
        private readonly IWatchdogFactory _watchdogFactory;
        private readonly IWatchdogManager _watchdogManager;
        private readonly EditWatchdogWindowFactory _editWatchdogWindowFactory;

        /// <summary>Список Watchdog-ов</summary>
        public ObservableCollection<Watchdog> Watchdogs { get; }

        public MemoryLogViewModel MemoryLogVm { get; private set; }

        /// <summary>Опрос на паузе</summary>
        public bool IsPause
        {
            get => _isPause;
            set
            {
                if (value == _isPause) return;
                _isPause = value;
                OnPropertyChanged();
            }
        }

        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                if (value.Equals(_currentDateTime)) return;
                _currentDateTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Время до обновления Watchdog-ов</summary>
        public int TimeUntilUpdate
        {
            get => _timeUntilUpdate;
            set
            {
                if (value == _timeUntilUpdate) return;
                _timeUntilUpdate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Прогресс обновления</summary>
        public int UpdateProgress
        {
            get => _updateProgress;
            set
            {
                if (value == _updateProgress) return;
                _updateProgress = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Текущий обновляемый Watchdog (для вывода его наименования в StatusBar)</summary>
        public Watchdog? CurrentUpdatingWatchdog
        {
            get => _currentUpdatingWatchdog;
            set
            {
                if (Equals(value, _currentUpdatingWatchdog)) return;
                _currentUpdatingWatchdog = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Выбранный Watchdog</summary>
        public Watchdog? SelectedWatchdog
        {
            get => _selectedWatchdog;
            set
            {
                if (Equals(value, _selectedWatchdog)) return;
                _selectedWatchdog = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Признак, что пароль введен правильно (если не был введен, то ввести пароль)</summary>
        private bool CorrectPasswordInputed
        {
            get
            {
                if (!_correctPasswordInputed)
                {
                    _correctPasswordInputed = AppService.ShowPassword();
                }

                return _correctPasswordInputed;
            }
        }

        public ICollectionView WatchdogCollectionView { get; }

        /// <summary>Добавить данные Watchdog</summary>
        public RelayCommand<Watchdog> AddWatchdogCommand { get; private set; }

        /// <summary>Добавить данные Watchdog</summary>
        public RelayCommand<Watchdog> EditWatchdogCommand { get; private set; }

        /// <summary>Добавить данные Watchdog</summary>
        public RelayCommand<Watchdog> RemoveWatchdogCommand { get; private set; }

        /// <summary>Опросить Watchdog</summary>
        public RelayCommand<Watchdog> RefreshWatchdogCommand { get; private set; }

        /// <summary>Опросить все Watchdog</summary>
        public RelayCommand<string> RefreshWatchdogsCommand { get; private set; }

        /// <summary>Старт/пауза обновления выбранного Watchdog</summary>
        public RelayCommand<Watchdog> StartStopRequestWatchdogCommand { get; private set; }

        /// <summary>Старт/пауза обновления всех Watchdog-ов</summary>
        public RelayCommand<bool> StartStopRequestAllWatchdogsCommand { get; private set; }

        /// <summary>Кол-во работающих Watchdog</summary>
        public int CountInWork
        {
            get => _countInWork;
            set
            {
                if (value == _countInWork)
                    return;
                _countInWork = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel() { }

        public MainWindowViewModel(EditWatchdogWindowFactory editWatchdogWindowFactory, MemoryLogViewModel memoryLogViewModel, IWatchdogManager watchdogManager,
            ILoggingService<MainWindowViewModel> logService, IWatchdogFactory watchdogFactory)
        {
            // если находимся в режиме дизайна, то выйти
            if (App.IsDesignMode)
                return;
            _editWatchdogWindowFactory = editWatchdogWindowFactory;
            MemoryLogVm = memoryLogViewModel;
            _logService = logService;
            _watchdogFactory = watchdogFactory;
            _watchdogManager = watchdogManager;

            CreateCommands();

            Watchdogs = new ObservableCollection<Watchdog>(_watchdogManager.Load());

            // событие при изменении коллекции
            Watchdogs.CollectionChanged += (_, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    return;

                // обновить watchdog при добавлении/редактировании
                if (e.NewItems is null)
                    return;

                foreach (Watchdog watchdog in e.NewItems)
                    UpdateWatchdog(watchdog);
            };

            WatchdogCollectionView = CollectionViewSource.GetDefaultView(Watchdogs);
            WatchdogCollectionView.SortDescriptions.Add(new SortDescription(nameof(Watchdog.Name), ListSortDirection.Ascending));

            Watchdog.AfterChangeWatchdogState += () => { CountInWork = Watchdogs.Count(w => w.State == WatchdogState.Work); };
        }

        private void CreateCommands()
        {
            AddWatchdogCommand = new RelayCommand<Watchdog>(_ => AddWatchdog());
            EditWatchdogCommand = new RelayCommand<Watchdog>(_ => EditWatchdog());
            RemoveWatchdogCommand = new RelayCommand<Watchdog>(_ => RemoveWatchdog());
            RefreshWatchdogCommand = new RelayCommand<Watchdog>(_ =>
            {
                UpdateWatchdog(SelectedWatchdog);
                UpdateProgress = 0;
            });
            RefreshWatchdogsCommand = new RelayCommand<string>(_ => TimeUntilUpdate = 0);
            StartStopRequestWatchdogCommand = new RelayCommand<Watchdog>(_ => SelectedWatchdogDoRequestChange());
            StartStopRequestAllWatchdogsCommand = new RelayCommand<bool>(_ => IsPause = !IsPause);
        }

        /// <summary> Вкл/выкл опрос выделенного Watchdog-а </summary>
        private void SelectedWatchdogDoRequestChange()
        {
            if (SelectedWatchdog is null)
                return;

            SelectedWatchdog.ToggleDoRequest();

            // сохранить состояние
            _watchdogManager.Save(SelectedWatchdog);

            // если был включен опрос Watchdog, то опросить принудительно
            if (SelectedWatchdog.DoRequest)
                _watchdogManager.GetWatchdogData(SelectedWatchdog);
        }

        /// <summary> Добавить Watchdog </summary>
        private void AddWatchdog()
        {
            if (!CorrectPasswordInputed)
                return;

            var newWatchdog = _watchdogFactory.CreateWatchdog();

            if (!CreateEditWatchdogWindow(newWatchdog))
                return;

            Watchdogs.Add(newWatchdog);

            var message = $"[{newWatchdog.Name}] добавлен!";
            _logService.Logger.LogInformation(message);
            _logService.MemoryLogStore.Add(message, WarningType.Warning);
        }

        /// <summary> Редактировать выделенный Watchdog </summary>
        private void EditWatchdog()
        {
            if (!CorrectPasswordInputed)
                return;

            if (SelectedWatchdog is null)
                return;

            var editedWatchdog = SelectedWatchdog.Clone();

            // т.к. путь к файлу не сериализуется, то присвоить значение напрямую из выбранного Watchdog
            // (если изменится наименование Watchdog, то удалить файл по этому пути)
            editedWatchdog.FilePath = SelectedWatchdog.FilePath;

            if (!CreateEditWatchdogWindow(editedWatchdog))
                return;

            _logService.Logger.LogWarning($"[{editedWatchdog.Name}] изменен!");
            _logService.MemoryLogStore.Add($"[{editedWatchdog.Name}] изменен!", WarningType.Warning);

            var editedWatchdogIndex = Watchdogs.IndexOf(SelectedWatchdog);

            if (editedWatchdogIndex != -1)
            {
                Watchdogs[editedWatchdogIndex] = editedWatchdog;
                UpdateProgress = 0;
            }

            WatchdogCollectionView.Refresh();
        }

        /// <summary> Удалить Watchdog </summary>
        private void RemoveWatchdog()
        {
            if (!CorrectPasswordInputed)
                return;

            if (SelectedWatchdog is null)
                return;

            if (!Messages.ShowMsgQstn($"Удалить {SelectedWatchdog}?"))
                return;

            if (!_watchdogManager.Remove(SelectedWatchdog))
                return;

            var message = $"[{SelectedWatchdog.Name}] удален!";
            _logService.Logger.LogError(message);
            _logService.MemoryLogStore.Add(message, WarningType.Warning);
            Watchdogs.Remove(SelectedWatchdog);
        }

        /// <summary> Окно добавления/редактирования Watchdog </summary>
        /// <param name="watchdog"></param>
        /// <returns></returns>
        private bool CreateEditWatchdogWindow(Watchdog watchdog)
        {
            var editWatchdogWindow = _editWatchdogWindowFactory.Create(watchdog);
            editWatchdogWindow.Owner = Application.Current.MainWindow;

            return editWatchdogWindow.ShowDialog() ?? false;
        }

        public void UpdateWatchdog(Watchdog? watchdog)
        {
            if (watchdog is null)
                return;

            UpdateProgress++;
            CurrentUpdatingWatchdog = watchdog;
            _watchdogManager.GetWatchdogData(watchdog);
            CurrentUpdatingWatchdog = null;
        }
    }
}
