using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToColorConverter : IValueConverter {
        public static readonly MpAvStringHexToColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            var b = MpAvStringHexToBrushConverter.Instance.Convert(value, targetType, parameter, culture) as SolidColorBrush;
            if (b != null) {
                if (parameter is string paramStr && double.Parse(paramStr) is double paramVal) {
                    var result = b.AdjustOpacity(paramVal).GetColor();
                    return result;
                }
                return b.Color;
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is Color c) {
                return c.ToPortableColor().ToHex();
            }
            return null;
        }
    }

}
