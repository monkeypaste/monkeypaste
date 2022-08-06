using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using MonkeyPaste.Common;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvEnumToKeyLiteralConverter : IValueConverter {
        public static readonly MpAvEnumToKeyLiteralConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is string valueStr) {
                if (string.IsNullOrEmpty(valueStr)) {
                    return string.Empty;
                }
                return MpAvKeyboardInputHelpers.GetKeyLiteral(
                        MpAvKeyboardInputHelpers.ConvertStringToKey(valueStr));
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }


}
