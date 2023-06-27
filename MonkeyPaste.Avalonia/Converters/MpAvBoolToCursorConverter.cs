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
            StandardCursorType? cursor_type = StandardCursorType.Arrow;
            if (value is bool boolVal) {
                if (parameter is string paramStr &&
                    paramStr.SplitNoEmpty("|") is string[] paramParts) {
                    string result_part = boolVal ? paramParts[0] : paramParts[1];
                    // for unset return null to not alter cursor
                    if (result_part.ToLower() == "unset") {
                        cursor_type = null;
                    } else {
                        cursor_type = result_part.ToEnum<StandardCursorType>();
                    }
                }
            }
            return cursor_type.HasValue ? new Cursor(cursor_type.Value) : null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
