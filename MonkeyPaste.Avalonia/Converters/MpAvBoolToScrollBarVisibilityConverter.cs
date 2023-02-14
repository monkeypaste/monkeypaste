using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToScrollBarVisibilityConverter : IValueConverter {
        public static readonly MpAvBoolToScrollBarVisibilityConverter Instance = new();

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool boolVal) {
                var paramParts = parameter == null ? "AUTO|HIDDEN".SplitNoEmpty("|") : parameter.ToString().SplitNoEmpty("|");
                ScrollBarVisibility trueVal = paramParts.ElementAt(0) == "AUTO" ? ScrollBarVisibility.Auto : ScrollBarVisibility.Visible;
                ScrollBarVisibility falseVal = paramParts.ElementAt(1) == "HIDDEN" ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Disabled;
                return boolVal ? trueVal : falseVal;
            }
            return ScrollBarVisibility.Auto;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
