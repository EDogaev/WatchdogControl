using System.Windows;
using Utilities;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.Watchdog;
using WatchdogControl.RealizedInterfaces;

namespace WatchdogControl.ViewModels
{
    public class EditWatchdogViewModel : NotifyPropertyChanged
    {
        private bool _testingInProgress;
        private readonly IWatchdogManager _watchdogManager;

        /// <summary>Идет проверка соединения с БД</summary>
        public bool TestingInProgress
        {
            get => _testingInProgress;
            set
            {
                if (value == _testingInProgress) return;
                _testingInProgress = value;
                OnPropertyChanged();
            }
        }

        public Watchdog Watchdog { get; }

        public RelayCommandAsync<string>? TestWatchdogCommand { get; }

        public RelayCommand<Window>? ConfirmCommand { get; }

        public EditWatchdogViewModel() { }

        public EditWatchdogViewModel(Watchdog watchdog, IWatchdogManager watchdogManager)
        {
            Watchdog = watchdog;
            _watchdogManager = watchdogManager;

            // проверка введенных данных
            TestWatchdogCommand = new RelayCommandAsync<string>(async (_) =>
            {
                TestingInProgress = true;
                var testSuccessful = await _watchdogManager.TestWatchDogDbData(Watchdog); ;
                TestingInProgress = false;

                if (testSuccessful)
                    Messages.ShowMsg("Успешно!");
            });

            ConfirmCommand = new RelayCommand<Window>(SaveWatchdog, _ => CanConfirm());
        }

        /// <summary>Сохранить Watchdog</summary>
        private void SaveWatchdog(Window window)
        {
            if (!Messages.ShowMsgQstn($"Сохранить данные {Watchdog.Name}?"))
                return;

            if (!_watchdogManager.Save(Watchdog))
                return;

            // при удачном сохранении сделать предыдущее наименование текущим
            Watchdog.PreviousName = Watchdog.Name;
            window.DialogResult = true;
        }

        private bool CanConfirm()
        {
            return !string.IsNullOrWhiteSpace(Watchdog.Name) &&
                   !string.IsNullOrWhiteSpace(Watchdog.DbData.DataSource) &&
                   !string.IsNullOrWhiteSpace(Watchdog.DbData.User) &&
                   !string.IsNullOrWhiteSpace(Watchdog.DbData.TableName) &&
                   !string.IsNullOrWhiteSpace(Watchdog.DbData.WatchdogFieldName) &&
                   Watchdog.Condition.TimeAfterLastChangeValue > 30;
        }
    }
}
