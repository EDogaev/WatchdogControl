using System.Globalization;
using System.Windows.Data;
using WatchdogControl.Models.Watchdog;

namespace WatchdogControl.Converters
{
    public class StateConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {

            var valueType = value?.GetType().Name;

            if (valueType is null)
                return null;

            return valueType switch
            {
                nameof(WatchdogState) => value switch
                {
                    WatchdogState.Initialization or WatchdogState.Unknown or WatchdogState.TurnedOff =>
                        "/Images/GrayCircle.png",
                    WatchdogState.NotWork => "/Images/RedCircle.png",
                    WatchdogState.TurnedOn => "/Images/YellowCircle.png",
                    WatchdogState.Work => "/Images/GreenCircle.png",
                    _ => throw new ArgumentOutOfRangeException()
                },
                nameof(DbState) => value switch
                {
                    DbState.Connecting or DbState.Unknown => "/Images/UnknownConnectionState.png",
                    DbState.Connected => "/Images/Connected.png",
                    DbState.Disconnected => "/Images/Disconnected.png",
                    _ => throw new ArgumentOutOfRangeException()
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}