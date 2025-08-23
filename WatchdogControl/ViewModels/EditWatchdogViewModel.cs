using System.Windows;
using Utilities;
using WatchdogControl.Enums;
using WatchdogControl.Models.Watchdog;
using WatchdogControl.RealizedInterfaces;
using WatchdogControl.Services;

namespace WatchdogControl.ViewModels
{
    public class EditWatchdogViewModel : NotifyPropertyChanged
    {
        private bool _testingInProgress;

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

        public EditWatchdogViewModel(Watchdog watchdog)
        {
            Watchdog = watchdog;

            // проверка введенных данных
            TestWatchdogCommand = new RelayCommandAsync<string>(async (_) =>
            {
                TestingInProgress = true;
                var testSuccessful = await WatchdogService.TestWatchDogDbData(Watchdog); ;
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

            if (!WatchdogService.SaveWatchdog(Watchdog))
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
