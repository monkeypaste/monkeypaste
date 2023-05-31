using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;


namespace MonkeyPaste.Avalonia {
    public class MpAvBrushToContrastBrushConverter : IValueConverter {
        public static readonly MpAvBrushToContrastBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (MpAvColorToContrastColorConverter.Instance.Convert(value, targetType, parameter, culture) is Color color) {
                return new SolidColorBrush() {
                    Color = color
                };
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
