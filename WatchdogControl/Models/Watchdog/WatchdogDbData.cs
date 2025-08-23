using System.ComponentModel;
using System.Configuration;
using System.Xml.Serialization;
using WatchdogControl.RealizedInterfaces;

namespace WatchdogControl.Models.Watchdog
{
    public enum DbState
    {
        [Description("Неизвестно")]
        Unknown,
        [Description("Отключено")]
        Disconnected,
        [Description("Подключение...")]
        Connecting,
        [Description("Подключено")]
        Connected
    }

    /// <summary>
    /// Данные для подключения к БД и запроса данных из таблицы с Watchdog
    /// </summary>
    public class WatchdogDbData : NotifyPropertyChanged
    {
        private const string ConnectionStringFormat = "Provider={0}; Data Source={1}; Password={2};User ID={3}";
        private static readonly string DefaultProvider = ConfigurationManager.AppSettings.Get("DefaultProvider");
        private Provider _provider;
        private DbState _state;
        private string _lastError;

        public string ConnectionString => string.Format(ConnectionStringFormat, Provider.Name, DataSource, Password, User);
        public string ConnectionStringNoPassword => string.Format(ConnectionStringFormat, Provider.Name, DataSource, "*****", User);

        public Provider Provider
        {
            get
            {
                if (_provider == null)
                    _provider = new Provider() { Name = DefaultProvider };

                if (string.IsNullOrWhiteSpace(_provider.Name))
                    _provider.Name = DefaultProvider;

                return _provider;
            }
            set => _provider = value;
        }

        /// <summary>Признак, что в данный момент идет попытка подключения к БД</summary>
        public bool IsConnecting => _state == DbState.Connecting;

        /// <summary>Источник данных (описатель в tnsnames)</summary>
        public string DataSource { get; set; }

        /// <summary>Пользователь для поключения к БД</summary>
        public string User { get; set; }

        /// <summary>Пароль для поключения к БД</summary>
        public DbPassword Password { get; set; } = new DbPassword();

        /// <summary>Номер агрегата</summary>
        public int? StationNo { get; set; }

        /// <summary>Наименование таблицы с Watchdog</summary>
        public string TableName { get; set; }

        /// <summary>Наименование столбца со значением Watchdog</summary>
        public string WatchdogFieldName { get; set; }

        /// <summary>Наименование параметра в столбце со значением Watchdog (если в столбце несколько записей)</summary>
        public string WatchdogParamName { get; set; }

        /// <summary>Наименование колонки с параметром (если в столбце несколько записей)</summary>
        public string WatchdogParamFieldName { get; set; }

        /// <summary>Наименование столбца со значением времени последнего изменения Watchdog</summary>
        public string LastWatchdogDateFieldName { get; set; }

        [XmlIgnore]
        public DbState State
        {
            get => _state;
            private set
            {
                if (value == _state) return;
                _state = value;

                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public string LastError
        {
            get => _lastError;
            set
            {
                if (value == _lastError) return;
                _lastError = value;

                OnPropertyChanged();
            }
        }

        public string SqlStatement => $"select {WatchdogFieldName}" +
                                      (string.IsNullOrWhiteSpace(LastWatchdogDateFieldName) ? string.Empty : $", {LastWatchdogDateFieldName}") +
                                      $" from {TableName}" +
                                      (string.IsNullOrWhiteSpace(WatchdogParamName) || string.IsNullOrWhiteSpace(WatchdogParamFieldName) ?
                                          string.Empty : $" where {WatchdogParamFieldName} = '{WatchdogParamName}'") +
                                      (StationNo == null ? string.Empty : $" and station_code = {StationNo}");

        public void SetWatchdogDbState(DbState dbState)
        {
            if (State == dbState)
                return;

            var prevState = State;

            switch (dbState)
            {
                case DbState.Unknown:
                    LastError = "";
                    break;
                case DbState.Disconnected:
                    break;
                case DbState.Connecting:
                    break;
                case DbState.Connected:
                    break;
            }

            State = dbState;

            if (prevState == DbState.Connecting && State == DbState.Connected || State == DbState.Connecting)
                return;

            //var mess = $"Изменилось состояние подключения к {DataSource}: {new EnumDescriptionConverter().Convert(State, null, null, null)}";

            //Logger2.LogToFile(mess);
            //CommonService.LogToMemory(mess, null);
        }
    }
}
