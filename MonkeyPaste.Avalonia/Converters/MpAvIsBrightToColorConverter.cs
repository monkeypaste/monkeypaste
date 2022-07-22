using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using MonkeyPaste.Common;
using Avalonia.Media;

namespace MonkeyPaste.Avalonia {
    public class MpAvIsBrightToColorConverter : IValueConverter {
        public static readonly MpAvIsBrightToColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is string valStr) {
                if(valStr.IsStringHexColor()) {
                    var c = new MpColor(valStr);
                    if(MpColorHelpers.IsBright(valStr)) {
                        return Colors.Black;
                    }
                    return Colors.White;
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
