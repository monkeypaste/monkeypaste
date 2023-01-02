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
                string key_literal = MpAvKeyboardInputHelpers.GetKeyLiteral(
                        MpAvKeyboardInputHelpers.ConvertStringToKey(valueStr));
                if(parameter is string paramStr && paramStr == "label") {
                    key_literal = key_literal.ToLabel();
                }
                return key_literal;
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }


}
