using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToColorConverter : IValueConverter {
        public static readonly MpAvStringHexToColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            var b = MpAvStringHexToBrushConverter.Instance.Convert(value, targetType, parameter, culture) as SolidColorBrush;
            if (b != null) {
                return b.Color;
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
