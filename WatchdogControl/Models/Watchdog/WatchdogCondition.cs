namespace WatchdogControl.Models.Watchdog
{
    public class WatchdogCondition
    {
        /// <summary>Время, прошедшее после последнего изменениея значения (сек)</summary>
        public int TimeAfterLastChangeValue { get; set; } = 60;
    }
}
