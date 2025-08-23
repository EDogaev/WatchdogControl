using WatchdogControl.Enums;
using WatchdogControl.RealizedInterfaces;

namespace WatchdogControl.Models.MemoryLog
{
    public class MemoryLog : NotifyPropertyChanged
    {
        public string Text { get; }

        public WarningType WarningType { get; }

        public bool IsError { get; set; }

        /// <summary>Не квитированная ошибка</summary>
        public bool IsActiveError { get; set; }

        public bool IsWarning { get; set; }

        /// <summary>Не квитированное предупреждение</summary>
        public bool IsActiveWarning { get; set; }

        public MemoryLog()
        {
        }

        public MemoryLog(string text, WarningType warningType)
        {
            Text = text;
            WarningType = warningType;
            IsError = WarningType == WarningType.Error;
            IsActiveError = IsError;
            IsWarning = WarningType == WarningType.Warning;
            IsActiveWarning = IsWarning;
        }
    }
}
