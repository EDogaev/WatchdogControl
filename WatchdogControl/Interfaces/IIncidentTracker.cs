using WatchdogControl.Models.MemoryLog;

namespace WatchdogControl.Interfaces
{
    /// <summary> Интерфейс для управления предупреждениями и ошибками </summary>
    public interface IIncidentTracker
    {
        /// <summary> Активные (не квитированные) ошибки </summary>
        IReadOnlyList<MemoryLog> ActiveErrors { get; }

        /// <summary> Активные (не квитированные) предупреждения </summary>
        IReadOnlyList<MemoryLog> ActiveWarnings { get; }

        /// <summary> Квитировать одну ошибку </summary>
        /// <param name="log"></param>
        void ResetActiveError(MemoryLog log);

        /// <summary> Квитировать одно предупреждение</summary>
        /// <param name="log"></param>
        void ResetActiveWarning(MemoryLog log);

        /// <summary> Квитировать все ошибки и предупреждения </summary>
        void ResetAllIncidents();

        /// <summary> Событие после изменения списков ошибок и предупреждений </summary>
        event EventHandler Changed;

    }
}
