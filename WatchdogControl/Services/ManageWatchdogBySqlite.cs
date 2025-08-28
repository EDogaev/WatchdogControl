using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.IO;
using Utilities;
using WatchdogControl.Enums;
using WatchdogControl.Interfaces;
using WatchdogControl.Models;
using WatchdogControl.Models.Watchdog;

namespace WatchdogControl.Services
{
    internal class ManageWatchdogBySqlite(ILoggingService<Watchdog> loggingService, IWatchdogFactory watchdogFactory, IMemoryLogStore memoryLogStore) : WatchdogManager(loggingService)

    {
        private static readonly string DatabasePath = Path.Combine(AppContext.BaseDirectory, "Watchdogs.db");
        private static readonly string ConnectionString = $"Data Source = {DatabasePath}";

        /// <summary> Получение списка Watchdog </summary>
        /// <returns></returns>
        public override List<Watchdog> Load()
        {
            try
            {
                var result = new List<Watchdog>();

                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                if (!(TableExist(connection, "Watchdogs")))
                    CreateWatchdogsTable(connection);

                using var command = connection.CreateCommand();
                command.CommandText = "select * from Watchdogs";

                using var reader = command.ExecuteReader();
                if (!reader.HasRows)
                    return result;

                while (reader.Read())
                {
                    try
                    {
                        result.Add(CreateWatchdog(reader));
                    }
                    catch (Exception ex)
                    {
                        var err = $"Ошибка при создании Watchdog'a {reader.SafeGetString("Watchdog_Name")}: \n{ex.Message}";
                        LoggingService.Logger.LogError(err);
                        Messages.ShowMsgErr(err, true);
                        LoggingService.MemoryLogStore.Add(err, WarningType.Error);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                var err = $"Ошибка при запросе данных из {DatabasePath}: \n{ex.Message}";
                LoggingService.Logger.LogError(err);
                Messages.ShowMsgErr(err, true);
                LoggingService.MemoryLogStore.Add(err, WarningType.Error);
            }

            return [];
        }

        private static bool TableExist(SqliteConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "select count(*) from sqlite_master where type='table' and name = @table_name";
            command.Parameters.AddWithValue("@table_name", tableName);
            var count = (long)(command.ExecuteScalar() ?? 0);
            return count > 0;
        }

        private static void CreateWatchdogsTable(SqliteConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = """
                                  CREATE TABLE "Watchdogs" (
                                    "Watchdog_Name" TEXT NOT NULL,
                                    "Do_Request" INTEGER NOT NULL,
                                    "Provider" TEXT,
                                    "Data_Source" TEXT NOT NULL,
                                    "User" TEXT NOT NULL,
                                    "Password" TEXT NOT NULL,
                                    "Station_No" INTEGER,
                                    "Table_Name" TEXT NOT NULL,
                                    "Watchdog_Field_Name" TEXT NOT NULL,
                                    "Watchdog_Param_Name" TEXT,
                                    "Watchdog_Param_Field_Name" TEXT,
                                    "Last_Watchdog_Date_Field_Name" TEXT,
                                    "Time_After_Last_Change_Value" INTEGER NOT NULL,
                                    PRIMARY KEY("Watchdog_Name")
                                    );
                                  """;
            command.ExecuteNonQuery();
        }

        private Watchdog CreateWatchdog(SqliteDataReader reader)
        {
            var watchdog = watchdogFactory.CreateWatchdog();

            watchdog.Name = reader.SafeGetString("Watchdog_Name");
            watchdog.PreviousName = reader.SafeGetString("Watchdog_Name");
            watchdog.DoRequest = reader.SafeGetInt32("Do_Request", 0) == 1;
            watchdog.DbData = new WatchdogDbData
            {
                DataSource = reader.SafeGetString("Data_Source"),
                User = reader.SafeGetString("User"),
                Password = new DbPassword
                {
                    EncryptedPassword = Convert.FromBase64String(reader.SafeGetString("Password"))
                },
                StationNo = reader.SafeGetInt32("Station_No", 0),
                TableName = reader.SafeGetString("Table_Name"),
                WatchdogFieldName = reader.SafeGetString("Watchdog_Field_Name"),
                WatchdogParamName = reader.SafeGetString("Watchdog_Param_Name"),
                WatchdogParamFieldName = reader.SafeGetString("Watchdog_Param_Field_Name"),
                LastWatchdogDateFieldName = reader.SafeGetString("Last_Watchdog_Date_Field_Name")
            };
            watchdog.Condition = new WatchdogCondition
            {
                TimeAfterLastChangeValue = reader.SafeGetInt32("Time_After_Last_Change_Value", 60)
            };

            return watchdog;
        }

        /// <summary> Сохранение данных Watchdog </summary>
        /// <param name="watchdog"></param>
        /// <returns></returns>
        public override bool Save(Watchdog watchdog)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();


                using var command = connection.CreateCommand();
                command.CommandText = $"select * from watchdogs where watchdog_name = '{watchdog.PreviousName}'";
                return command.ExecuteScalar() == null ? InsertWatchdog(watchdog, command) : UpdateWatchdog(watchdog, command);
            }
            catch (Exception ex)
            {
                var err = $"Ошибка при сохранении данных в {DatabasePath}: \n{ex.Message}";
                LoggingService.Logger.LogError(err);
                Messages.ShowMsgErr(err);
                LoggingService.MemoryLogStore.Add(err, WarningType.Error);
                return false;
            }
        }

        private bool InsertWatchdog(Watchdog watchdog, SqliteCommand command)
        {
            try
            {
                command.CommandText = "insert into watchdogs (" +
                                      "Watchdog_Name, Do_Request, Provider, Data_Source, User, Password, Station_No, Table_Name, Watchdog_Field_Name, " +
                                      "Watchdog_Param_Name, Watchdog_Param_Field_Name, Last_Watchdog_Date_Field_Name, Time_After_Last_Change_Value) " +
                                      "values(" +
                                      $"'{watchdog.Name}', " +
                                      (watchdog.DoRequest ? "1, " : "0, ") +
                                      $"'{watchdog.DbData.DataSource}', " +
                                      $"'{watchdog.DbData.User}', " +
                                      $"'{Convert.ToBase64String(watchdog.DbData.Password.EncryptedPassword)}', " +
                                      $"{watchdog.DbData.StationNo}, " +
                                      $"'{watchdog.DbData.TableName}', " +
                                      $"'{watchdog.DbData.WatchdogFieldName}', " +
                                      $"'{watchdog.DbData.WatchdogParamName}', " +
                                      $"'{watchdog.DbData.WatchdogParamFieldName}', " +
                                      $"'{watchdog.DbData.LastWatchdogDateFieldName}', " +
                                      $"{watchdog.Condition.TimeAfterLastChangeValue} " +
                                      ")";
                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                LoggingService.Logger.LogError($"""Ошибка при добавлении watchdog`а "{watchdog.Name}": {ex.Message}""");
                throw;
            }
        }

        private bool UpdateWatchdog(Watchdog watchdog, SqliteCommand command)
        {
            try
            {

                command.CommandText = "update watchdogs set " +
                                      $"Watchdog_Name = '{watchdog.Name}', " +
                                      "Do_Request = " + (watchdog.DoRequest ? "1" : "0") +
                                      $"Data_Source = '{watchdog.DbData.DataSource}', " +
                                      $"User = '{watchdog.DbData.User}', " +
                                      $"Password = '{Convert.ToBase64String(watchdog.DbData.Password.EncryptedPassword)}', " +
                                      $"Station_No = {watchdog.DbData.StationNo}, " +
                                      $"Table_Name = '{watchdog.DbData.TableName}', " +
                                      $"Watchdog_Field_Name = '{watchdog.DbData.WatchdogFieldName}', " +
                                      $"Watchdog_Param_Name = '{watchdog.DbData.WatchdogParamName}', " +
                                      $"Watchdog_Param_Field_Name = '{watchdog.DbData.WatchdogParamFieldName}', " +
                                      $"Last_Watchdog_Date_Field_Name = '{watchdog.DbData.LastWatchdogDateFieldName}', " +
                                      $"Time_After_Last_Change_Value = {watchdog.Condition.TimeAfterLastChangeValue} " +
                                      $"where watchdog_name = '{watchdog.PreviousName}'";
                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                LoggingService.Logger.LogError($"""Ошибка при обновлении watchdog`а "{watchdog.PreviousName}": {ex.Message}""");
                throw;
            }
        }

        /// <summary> Удаление Watchdog </summary>
        /// <param name="watchdog"></param>
        /// <returns></returns>
        public override bool Remove(Watchdog watchdog)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "delete from watchdogs where watchdog_name = @name";
                command.Parameters.AddWithValue("@name", watchdog.Name);
                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                var err = $"Ошибка при удалении данных из {DatabasePath}: \n{ex.Message}";
                LoggingService.Logger.LogError(err);
                Messages.ShowMsgErr(err, true);
                LoggingService.MemoryLogStore.Add(err, WarningType.Error);
                return false;
            }
        }
    }

    /// <summary> Методы расширения для SqliteDataReader </summary>
    public static class SqliteSafeGetValue
    {
        /// <summary> Безопасное получение строкового значения поля </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string SafeGetString(this SqliteDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal(columnName));
        }

        /// <summary> Безопасное получение целочисленного значения поля </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <param name="defaultValue">Значание по-умолчанию</param>
        /// <returns></returns>
        public static int SafeGetInt32(this SqliteDataReader reader, string columnName, int defaultValue)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
                ? defaultValue
                : reader.GetInt32(reader.GetOrdinal(columnName));
        }
    }
}