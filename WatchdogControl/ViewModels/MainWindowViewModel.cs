using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;
using WatchdogControl.Enums;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.Watchdog;
using WatchdogControl.RealizedInterfaces;
using WatchdogControl.Views;

namespace WatchdogControl.ViewModels
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private DateTime _currentDateTime;
        private Watchdog? _selectedWatchdog;
        private int _timeUntilUpdate;
        private int _updateProgress;
        private Watchdog? _currentUpdatingWatchdog;
        private bool _isPause;
        private bool _correctPasswordInputed;
        private int _countInWork;
        private readonly ILogger _logger;
        private readonly IWatchdogFactory _watchdogFactory;
        private readonly IMemoryLogStore _memoryLogStore;
        private readonly IWatchdogManager _watchdogManager;

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
                    _correctPasswordInputed = ShowPassword();
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

        public MainWindowViewModel(MemoryLogViewModel memoryLogViewModel, IWatchdogManager watchdogManager, ILogger<MainWindowViewModel> logger, IWatchdogFactory watchdogFactory, IMemoryLogStore memoryLogStore)
        {
            // если находимся в режиме дизайна, то выйти
            if (App.IsDesignMode)
                return;

            MemoryLogVm = memoryLogViewModel;

            _logger = logger;
            _watchdogFactory = watchdogFactory;
            _memoryLogStore = memoryLogStore;
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

            CreateTimer();
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

        private void CreateTimer()
        {
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (_, _) => UpdateWatchdogsTimer();
            _timer.Start();
        }

        /// <summary> Показать окно ввода пароля </summary>
        /// <returns></returns>
        private static bool ShowPassword()
        {
            var passwordView = new PasswordView
            {
                Owner = Application.Current.MainWindow
            };

            return passwordView.ShowDialog() == true;
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

            _logger.LogInformation($"[{newWatchdog.Name}] добавлен!");
            _memoryLogStore.Add($"[{newWatchdog.Name}] добавлен!", WarningType.Warning);
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

            _logger.LogWarning($"[{editedWatchdog.Name}] изменен!");
            _memoryLogStore.Add($"[{editedWatchdog.Name}] изменен!", WarningType.Warning);

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

            _logger.LogError($"[{SelectedWatchdog.Name}] удален!");
            _memoryLogStore.Add($"[{SelectedWatchdog.Name}] удален!", WarningType.Warning);
            Watchdogs.Remove(SelectedWatchdog);
        }

        /// <summary> Окно добавления/редактирования Watchdog </summary>
        /// <param name="watchdog"></param>
        /// <returns></returns>
        private bool CreateEditWatchdogWindow(Watchdog watchdog)
        {
            var editWatchdogWindow = new EditWatchdogView(watchdog, _watchdogManager)
            {
                Owner = Application.Current.MainWindow
            };

            return editWatchdogWindow.ShowDialog() ?? false;
        }

        /// <summary> Таймер обновления Watchdog-ов </summary>
        private void UpdateWatchdogsTimer()
        {
            CurrentDateTime = DateTime.Now;

            if (!IsPause)
                TimeUntilUpdate--;

            if (TimeUntilUpdate > 0) return;

            TimeUntilUpdate = 30;

            // используется ToLIst() для создания копии списка Watchdogs,
            // чтобы при изменении элемента коллекции во время выполнения этого цикла
            // не возникало ошибки "Collection was modified; enumeration operation may not execute"
            foreach (var watchdog in Watchdogs.ToList())
            {
                UpdateWatchdog(watchdog);
            }

            UpdateProgress = 0;
        }

        private void UpdateWatchdog(Watchdog? watchdog)
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
