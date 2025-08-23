using System.Globalization;
using System.Windows.Data;
using WatchdogControl.Models.Watchdog;

namespace WatchdogControl.Converters
{
    public class StateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var valueType = value?.GetType().Name;

            if (valueType is null)
                return null;

            switch (valueType)
            {
                case nameof(WatchdogState):
                    switch (value)
                    {
                        case WatchdogState.Initialization:
                        case WatchdogState.Unknown:
                        case WatchdogState.TurnedOff:
                            return "/Images/GrayCircle.png";
                        case WatchdogState.NotWork:
                            return "/Images/RedCircle.png";
                        case WatchdogState.TurnedOn:
                            return "/Images/YellowCircle.png";
                        case WatchdogState.Work:
                            return "/Images/GreenCircle.png";
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                case nameof(DbState):
                    switch (value)
                    {
                        case DbState.Connecting:
                        case DbState.Unknown:
                            return "/Images/UnknownConnectionState.png";
                        case DbState.Connected:
                            return "/Images/Connected.png";
                        case DbState.Disconnected:
                            return "/Images/Disconnected.png";
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}