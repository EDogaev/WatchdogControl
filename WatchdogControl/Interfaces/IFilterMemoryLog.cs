namespace WatchdogControl.Interfaces
{
    public interface IFilterMemoryLog
    {
        /// <summary> Текст фильтра </summary>
        string FilterText { get; set; }

        /// <summary> Показывать ошибки </summary>
        bool ShowErrors { get; set; }

        /// <summary> Показывать предупреждения </summary>
        bool ShowWarnings { get; set; }

        /// <summary> Показывать все (кроме ошибок и предупреждений) </summary>
        bool ShowOthers { get; set; }

        /// <summary> Условия фильтрации </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Filter(object obj);

        /// <summary> Событие после изменения условий фильтрации </summary>
        event Action Changed;
    }
}
