using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using MonkeyPaste.Common;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvColorToContrastColorConverter : IValueConverter {
        public static readonly MpAvColorToContrastColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            string hexStr = null;
            if(value is IBrush brush) {
                hexStr = brush.ToHex();
            }
            if(value is Color color) {
                hexStr = color.ToPortableColor().ToHex();
            }
            if(value is string valStr) {
                hexStr = valStr;
                
            }
            if (hexStr.IsStringHexColor()) {
                bool flip = parameter is string str && str == "flip";

                if (MpColorHelpers.IsBright(hexStr)) {
                    return flip ? Colors.White : Colors.Black;
                }
                return flip ? Colors.Black : Colors.White;
            }
            return Colors.Black;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
