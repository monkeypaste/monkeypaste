using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentDataConverter : IValueConverter {
        public static readonly MpAvContentDataConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is string valStr) {
                if (parameter is string paramStr) {
                    if (paramStr.ToLowerInvariant() == "plaintext") {
                        return Mp.Services.StringTools.ToPlainText(valStr, "html");
                    }
                }
                return valStr;
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is string valStr) {
                return valStr;
            }
            return value;
        }
    }
}
