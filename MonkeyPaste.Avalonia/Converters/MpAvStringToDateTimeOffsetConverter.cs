using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringToDateTimeOffsetConverter : IValueConverter {
        public static readonly MpAvStringToDateTimeOffsetConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string valueStr && !string.IsNullOrEmpty(valueStr)) {
                if (DateTimeOffset.TryParse(valueStr, out DateTimeOffset result)) {
                    return result;
                }
            }
            return DateTimeOffset.Now;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                return dt.ToString();
            }
            if (value is DateTimeOffset dto) {
                return dto.DateTime.ToString();
            }
            return null;
        }
    }
}
