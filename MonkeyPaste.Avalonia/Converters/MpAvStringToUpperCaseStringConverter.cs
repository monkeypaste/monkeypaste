using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringToUpperCaseStringConverter : IValueConverter {
        public static readonly MpAvStringToUpperCaseStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is not string strVal) {
                return string.Empty;
            }
            return strVal.ToUpperInvariant();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
