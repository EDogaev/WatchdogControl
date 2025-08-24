using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Utilities;
using WatchdogControl.Enums;
using WatchdogControl.Interfaces;
using WatchdogControl.Models.Watchdog;

namespace WatchdogControl.Services
{
    internal abstract class WatchdogManager(ILogger<Watchdog> logger, IMemoryLogStore memoryLogStore) : IWatchdogManager
    {
        protected readonly ILogger<Watchdog> Logger = logger;
        protected readonly IMemoryLogStore MemoryLogStore = memoryLogStore;

        /// <summary>Проверка введенных данных таблицы</summary>
        public async Task<bool> TestWatchDogDbData(Watchdog watchdog)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var dbConnection = new OracleConnection(watchdog.DbData.ConnectionString);
                    try
                    {
                        dbConnection.Open();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[{watchdog.Name}] - Тест. Ошибка при подключении к базе данных: \n{ex.Message}");
                    }

                    try
                    {
                        using var dataAdapter = new OracleDataAdapter(watchdog.DbData.SqlStatement, dbConnection);
                        var dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);
                        // попытка получить значение из результата запроса
                        var value = dataTable.Rows[0][0];
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"[{watchdog.Name}] - Тест. Ошибка при запросе данных: \n{watchdog.DbData.SqlStatement}. \n{ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Messages.ShowMsgErr(ex.Message, true);
                    MemoryLogStore.Add(ex.Message, WarningType.Error);

                    return false;
                }

                return true;
            });
        }

        /// <summary>Обновить данные Watchdog</summary>
        /// <returns></returns>
        public async void GetWatchdogData(Watchdog watchdog)
        {
            // если не опрашивать Watchdog
            if (!watchdog.DoRequest)
                return;

            // если Watchdog в процессе подключения к БД
            if (watchdog.DbData.IsConnecting)
                return;

            await Task.Run(() =>
            {

                var prevError = watchdog.DbData.LastError;

                try
                {
                    using var dbConnection = new OracleConnection(watchdog.DbData.ConnectionString);
                    try
                    {
                        watchdog.DbData.SetWatchdogDbState(DbState.Connecting);
                        dbConnection.Open();
                        watchdog.DbData.SetWatchdogDbState(DbState.Connected);
                    }
                    catch (Exception ex)
                    {
                        watchdog.DbData.SetWatchdogDbState(DbState.Disconnected);

                        throw new Exception($"""[{watchdog.Name}] Не удалось соединиться с "{watchdog.DbData.ConnectionStringNoPassword}": {ex.Message}""");
                    }

                    try
                    {
                        using var dataAdapter = new OracleDataAdapter(watchdog.DbData.SqlStatement, dbConnection);
                        using var watchdogDataTable = new DataTable();
                        dataAdapter.Fill(watchdogDataTable);
                        var row = watchdogDataTable.Rows[0];

                        // текущее значение Watchdog
                        watchdog.Values.Value = row.Field<long>(watchdog.DbData.WatchdogFieldName);

                        // если есть колонка с датой обновления Watchdog, то взять дату из нее
                        if (!string.IsNullOrWhiteSpace(watchdog.DbData.LastWatchdogDateFieldName))
                            watchdog.Values.LastValueChangeDate =
                                row.Field<DateTime>(watchdog.DbData.LastWatchdogDateFieldName);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{watchdog.DbData.SqlStatement} : {ex.Message}");
                    }

                    watchdog.DbData.LastError = string.Empty;
                }
                catch (Exception ex)
                {
                    watchdog.SetWatchdogState(WatchdogState.Unknown);

                    watchdog.DbData.LastError = ex.Message;

                    // условие, чтобы сообщение не забивало лог
                    if (!string.Equals(prevError, watchdog.DbData.LastError))
                    {
                        var err = $"{watchdog.DbData.LastError}";

                        Logger.LogError(err);
                        MemoryLogStore.Add(err, WarningType.Error);
                    }
                }

                CheckConditions(watchdog);
            });
        }

        /// <summary> Проверить условия для изменения состояния Watchdog </summary>
        /// <param name="watchdog"></param>
        private static void CheckConditions(Watchdog watchdog)
        {
            if (watchdog.DbData.State == DbState.Disconnected ||
                !string.IsNullOrWhiteSpace(watchdog.DbData.LastError) ||
                !watchdog.DoRequest)
                return;

            if (DateTime.Now - watchdog.Values.LastValueChangeDate > TimeSpan.FromSeconds(watchdog.Condition.TimeAfterLastChangeValue))
            {
                watchdog.SetWatchdogState(WatchdogState.NotWork);
                return;
            }

            watchdog.SetWatchdogState(WatchdogState.Work);
        }

        /// <summary>Загрузить данные</summary>
        public abstract IEnumerable<Watchdog> Load();


        /// <summary>Сохранить данные</summary>
        /// <param name="watchdog"></param>
        /// <returns></returns>
        public abstract bool Save(Watchdog watchdog);

        /// <summary>Удалить данные</summary>
        /// <param name="watchdog"></param>
        /// <returns></returns>
        public abstract bool Remove(Watchdog watchdog);
    }
}
