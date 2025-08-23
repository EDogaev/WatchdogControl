using System.Globalization;
using WatchdogControl.Interfaces;

namespace WatchdogControl.Models.MemoryLog
{
    internal class FilterMemoryLog : IFilterMemoryLog
    {
        private string _filterText = string.Empty;
        private bool _showErrors = true;
        private bool _showWarnings = true;
        private bool _showOthers = true;

        public event Action Changed;

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                Changed?.Invoke();
            }
        }

        public bool ShowErrors
        {
            get => _showErrors;
            set
            {
                if (value == _showErrors)
                    return;

                _showErrors = value;
                Changed?.Invoke();
            }
        }

        public bool ShowWarnings
        {
            get => _showWarnings;
            set
            {
                if (value == _showWarnings)
                    return;

                _showWarnings = value;
                Changed?.Invoke();
            }
        }

        public bool ShowOthers
        {
            get => _showOthers;
            set
            {
                if (value == _showOthers)
                    return;

                _showOthers = value;
                Changed?.Invoke();
            }
        }

        public bool Filter(object obj)
        {
            if (!(obj is MemoryLog log))
                return false;

            var result = log.IsError ? ShowErrors : (log.IsWarning ? ShowWarnings : ShowOthers);

            if (!string.IsNullOrEmpty(FilterText))
                result &= CultureInfo.InvariantCulture.CompareInfo.IndexOf(log.Text, FilterText, CompareOptions.IgnoreCase) >= 0;

            return result;
        }
    }
}
