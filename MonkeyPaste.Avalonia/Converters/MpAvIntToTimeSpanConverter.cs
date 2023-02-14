using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvIntToTimeSpanConverter : IValueConverter {
        public static readonly MpAvIntToTimeSpanConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int valMs) {
                string unit = parameter is string ? parameter as string : "ms";
                if (unit == "ms") {
                    return TimeSpan.FromMilliseconds(valMs);
                }
            }
            return TimeSpan.FromMilliseconds(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
