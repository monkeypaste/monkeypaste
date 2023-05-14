using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvDateTimeToStringConverter : IValueConverter {
        public static readonly MpAvDateTimeToStringConverter Instance = new();

        public const string NUMERIC_DATE_TIME_FORMAT = "MM/dd/yyyy hh:mm tt";
        public const string LITERAL_DATE_TIME_FORMAT = "MMM d, h:mm tt";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                string format = parameter is string ? parameter as string : LITERAL_DATE_TIME_FORMAT;
                return dt.ToString(format);
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
