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
                    bool flip = parameter is string str && str == "flip";

                    var c = new MpColor(valStr);
                    if(MpColorHelpers.IsBright(valStr)) {
                        return flip ? Colors.White : Colors.Black;
                    }
                    return flip ? Colors.Black : Colors.White;
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
