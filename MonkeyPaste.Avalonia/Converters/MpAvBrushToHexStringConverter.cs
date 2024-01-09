using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBrushToHexStringConverter : IValueConverter {
        public static readonly MpAvBrushToHexStringConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is IBrush b) {
                return b.ToPortableColor().ToHex(true);
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
