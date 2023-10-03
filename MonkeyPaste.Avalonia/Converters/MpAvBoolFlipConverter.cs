using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolFlipConverter : IValueConverter {
        public static readonly MpAvBoolFlipConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool boolVal) {
                return !boolVal;
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

    public class MpAvObjectToCommaFormatedValueConverter : IValueConverter {
        public static readonly MpAvObjectToCommaFormatedValueConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool boolVal) {
                return !boolVal;
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
