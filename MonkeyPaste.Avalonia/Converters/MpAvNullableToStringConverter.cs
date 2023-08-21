using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvNullableToStringConverter : IValueConverter {
        public static readonly MpAvNullableToStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is int intVal &&
                parameter is string paramStr &&
                paramStr == "abbr") {
                return intVal.ToAbbreviatedIntString();
            }
            return value == null ? string.Empty : value.ToString();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
