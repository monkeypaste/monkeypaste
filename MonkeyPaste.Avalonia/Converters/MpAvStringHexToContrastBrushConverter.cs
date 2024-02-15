using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToContrastBrushConverter : IValueConverter {
        public static readonly MpAvStringHexToContrastBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
            if (MpAvStringHexToContrastHexColorConverter.Instance.Convert(value, targetType, parameter, culture) is not string contrast_hex_color ||
                contrast_hex_color.ToAvBrush() is not IBrush contrast_brush) {
                return Brushes.Transparent;
            }
            return contrast_brush;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
