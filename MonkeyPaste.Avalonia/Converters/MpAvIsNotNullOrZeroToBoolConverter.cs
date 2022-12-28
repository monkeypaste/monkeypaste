using Avalonia.Data.Converters;
using System;
using System.Diagnostics;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvIsNotNullOrZeroToBoolConverter : IValueConverter {
        public static readonly MpAvIsNotNullOrZeroToBoolConverter Instance = new();

        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture) {
            bool flip = false;
            if(parameter is string paramStr && paramStr.ToLower() == "flip") {
                flip = true;
            }
            if(value is int intVal) {
                return intVal == 0 ? flip : !flip;
            }
            if (value is double doubleVal) {
                return doubleVal == 0 ? flip : !flip;
            }
            return value == null ? flip : !flip;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
