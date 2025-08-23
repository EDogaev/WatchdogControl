using WatchdogControl.RealizedInterfaces;

namespace WatchdogControl.Models.Watchdog
{
    public class WatchdogValues : NotifyPropertyChanged
    {
        private decimal _value;
        private DateTime? _lastValueChangeDate;
        private DateTime? _lastRequestDate;

        /// <summary>Значение Watchdog</summary>
        public decimal Value
        {
            get => _value;
            set
            {
                LastRequestDate = DateTime.Now;
                if (value == _value) return;
                _value = value;
                LastValueChangeDate = DateTime.Now;

                OnPropertyChanged();
            }
        }

        /// <summary>Дата последнего изменения Watchdog</summary>
        public DateTime? LastValueChangeDate
        {
            get => _lastValueChangeDate;
            set
            {
                if (value.Equals(_lastValueChangeDate)) return;
                _lastValueChangeDate = value;

                OnPropertyChanged();
            }
        }

        /// <summary>Дата опроса значения Watchdog</summary>
        public DateTime? LastRequestDate
        {
            get => _lastRequestDate;
            set
            {
                if (value.Equals(_lastRequestDate)) return;
                _lastRequestDate = value;

                OnPropertyChanged();
            }
        }

    }
}
