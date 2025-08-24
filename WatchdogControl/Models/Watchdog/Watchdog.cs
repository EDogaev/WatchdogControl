using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Xml.Serialization;
using Utilities;
using WatchdogControl.Converters;
using WatchdogControl.Enums;
using WatchdogControl.Interfaces;
using WatchdogControl.RealizedInterfaces;

namespace WatchdogControl.Models.Watchdog
{
    public enum WatchdogState
    {
        [Description("Инициализация...")]
        Initialization,
        [Description("Неизвестно")]
        Unknown,
        [Description("Не работает!")]
        NotWork,
        [Description("Работает")]
        Work,
        [Description("Опрос включен")]
        TurnedOn,
        [Description("Опрос отключен")]
        TurnedOff
    }

    public class Watchdog : NotifyPropertyChanged
    {
        private WatchdogState _state;
        private bool _doRequest = true;
        private ILoggingService<Watchdog> _loggingService;

        public static event Action AfterChangeWatchdogState;

        /// <summary>Данные для подключения к БД и запроса из таблицы</summary>
        public WatchdogDbData DbData { get; set; } = new WatchdogDbData();

        /// <summary>Текущие значения Watchdog</summary>
        [XmlIgnore]
        public WatchdogValues Values { get; set; } = new WatchdogValues();

        /// <summary>Условия, при которых изменяется состояние Watchdog</summary>
        public WatchdogCondition Condition { get; set; } = new WatchdogCondition();

        /// <summary>Наименование Watchdog</summary>
        public string Name { get; set; }

        /// <summary>Предыдущее наименование Watchdog (используется для переименования)</summary>
        public string PreviousName { get; set; }

        /// <summary>Признак, что Watchdog будет опрашиватьсф</summary>
        public bool DoRequest
        {
            get => _doRequest;
            set
            {
                if (value == _doRequest)
                    return;

                _doRequest = value;

                SetWatchdogState(DoRequest ? WatchdogState.TurnedOn : WatchdogState.TurnedOff);

                OnPropertyChanged();
            }
        }

        /// <summary>Состояние Watchdog</summary>
        [XmlIgnore]
        public WatchdogState State
        {
            get => _state;
            private set
            {
                if (value == _state)
                    return;

                _state = value;

                OnPropertyChanged();
            }
        }

        /// <summary>Имя файла Watchdog</summary>
        [XmlIgnore]
        public string FilePath { get; set; }

        public Watchdog() { }

        public Watchdog(ILoggingService<Watchdog> loggingService)
        {
            _loggingService = loggingService;
        }

        public Watchdog Clone()
        {
            var clone = XmlSerializer<Watchdog>.CopyObject(this);
            clone._loggingService = _loggingService;
            return clone;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>Переключить состояние опроса</summary>
        public void ToggleDoRequest()
        {
            // если идет подключение к БД, не переключать состояние опроса
            if (DbData.IsConnecting)
                return;

            DoRequest = !DoRequest;
        }

        public void SetWatchdogState(WatchdogState state)
        {
            if (State == state)
                return;

            var prevState = State;
            State = state;

            AfterChangeWatchdogState?.Invoke();

            var warningType = WarningType.Unknown;

            switch (State)
            {
                case WatchdogState.Unknown:
                    warningType = WarningType.Unknown;
                    break;
                case WatchdogState.NotWork:
                    warningType = WarningType.Error;
                    break;
                case WatchdogState.Work:
                    warningType = WarningType.Ok;
                    break;
                case WatchdogState.TurnedOn:
                    warningType = WarningType.Unknown;
                    break;
                case WatchdogState.TurnedOff:
                    warningType = WarningType.Unknown;
                    DbData.SetWatchdogDbState(DbState.Unknown);
                    break;
            }

            var mess = $"[{Name}] Изменилось состояние: {new EnumDescriptionConverter().Convert(State, null, null, null)}";

            if (prevState != WatchdogState.Initialization)
                _loggingService.Logger.LogInformation(mess);

            _loggingService.MemoryLogStore.Add(mess, warningType);
        }
    }
}
