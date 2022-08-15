using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBoolToScrollBarVisibilityConverter : IValueConverter {
        public static readonly MpAvBoolToScrollBarVisibilityConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            var boolVal = (bool?)value;
            if(boolVal.HasValue) {
                return boolVal.Value ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
            }
            return ScrollBarVisibility.Auto;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
