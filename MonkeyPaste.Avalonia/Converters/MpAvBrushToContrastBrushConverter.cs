using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using MonkeyPaste.Common;
using Avalonia.Media;

namespace MonkeyPaste.Avalonia {
    public class MpAvBrushToContrastBrushConverter : IValueConverter {
        public static readonly MpAvBrushToContrastBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            var color = (Color)new MpAvColorToContrastColorConverter().Convert(value, targetType, parameter, culture);
            if(color == null) {
                return Brushes.Red;
            }
            return new SolidColorBrush() {
                Color = color
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
