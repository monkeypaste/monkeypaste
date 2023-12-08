using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvDateTimeToStringConverter : IValueConverter {
        public static readonly MpAvDateTimeToStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                string format = parameter is string ? parameter as string : UiStrings.CommonDateFormat;
                return dt.ToString(format);
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
