using Avalonia.Data.Converters;
using Avalonia.Input;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToCursorConverter : IValueConverter {
        public static readonly MpAvBoolToCursorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            StandardCursorType cursor_type = StandardCursorType.Arrow;
            if (value is bool boolVal) {
                if (parameter is string paramStr &&
                    paramStr.SplitNoEmpty("|") is string[] paramParts) {
                    if (boolVal) {
                        cursor_type = paramParts[0].ToEnum<StandardCursorType>();
                    } else {
                        cursor_type = paramParts[1].ToEnum<StandardCursorType>();
                    }
                }
            }
            if (cursor_type == StandardCursorType.None) {
                // NOTE treating None as unset, not lack of cursor
                return null;
            }
            return new Cursor(cursor_type);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
