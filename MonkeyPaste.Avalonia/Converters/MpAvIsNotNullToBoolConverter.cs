using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvIsNotNullToBoolConverter : IValueConverter {
        public static readonly MpAvIsNotNullToBoolConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return value != null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
