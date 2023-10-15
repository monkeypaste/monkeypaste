using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvDateTimeToDateTimeOffsetConverter : IValueConverter {
        public static readonly MpAvDateTimeToDateTimeOffsetConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                return (DateTimeOffset)dt;
            }
            if (value is DateTimeOffset dto) {
                return dto;
            }
            return default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime dt) {
                return dt;
            }
            if (value is DateTimeOffset dto) {
                return dto.UtcDateTime;
            }
            return default;
        }
    }
}
