using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvObjectToObjectArrayConverter : IValueConverter {
        public static readonly MpAvObjectToObjectArrayConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return new object[] { value, parameter };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
