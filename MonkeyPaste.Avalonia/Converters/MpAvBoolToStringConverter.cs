using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToStringConverter : IValueConverter {
        public static readonly MpAvBoolToStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is bool boolVal) {
                string true_val = string.Empty;
                string false_val = string.Empty;
                if (parameter is string paramStr &&
                    paramStr.Split("|") is string[] paramParts) {
                    true_val = paramParts[0];
                    false_val = paramParts[1];
                }
                return boolVal ? true_val : false_val;
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
