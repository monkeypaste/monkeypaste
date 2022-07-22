using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using MonkeyPaste.Common;
using Avalonia.Media;

namespace MonkeyPaste.Avalonia {
    public class MpAvIsBrightToBrushConverter : IValueConverter {
        public static readonly MpAvIsBrightToBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            var color = (Color)new MpAvIsBrightToColorConverter().Convert(value, targetType, parameter, culture);
            if(color == null) {
                return null;
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
