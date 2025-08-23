using System.Globalization;
using System.Windows.Data;

namespace WatchdogControl.Converters
{
    internal class NullableIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
                return null;

            if (int.TryParse(value.ToString(), out var result))
                return result;

            return null;
        }
    }
}
