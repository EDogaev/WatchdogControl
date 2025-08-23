using System.Windows;
using Utilities;
using WatchdogControl.Enums;
using WatchdogControl.Models;
using WatchdogControl.Models.Watchdog;
using WatchdogControl.RealizedInterfaces;
using WatchdogControl.Services;

namespace WatchdogControl.ViewModels
{
    public class EditWatchdogViewModel : NotifyPropertyChanged
    {
        private readonly EditType _editType;
        private bool _testingInProgress;

        /// <summary>Список провайдеров</summary>
        public static List<Provider> Providers { get; }

        /// <summary>Окно открыто для редактирования</summary>
        public bool IsEditing => _editType == EditType.Edit;

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

        public RelayCommandAsync<string> TestWatchdogCommand { get; }

        public RelayCommand<Window> ConfirmCommand { get; }

        public EditWatchdogViewModel() { }

        public EditWatchdogViewModel(EditType editType, Watchdog watchdog)
        {
            _editType = editType;
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

            ConfirmCommand = new RelayCommand<Window>(SaveWatchdog, w => CanConfirm());
        }

        static EditWatchdogViewModel()
        {
            Providers = DbService.GetProviders();
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
