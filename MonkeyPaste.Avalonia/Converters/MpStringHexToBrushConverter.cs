using System;
using System.Globalization;
using MonkeyPaste.Common;
using Avalonia.Data.Converters;
using Avalonia.Data;
using Avalonia.Media;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToBrushConverter : IValueConverter {
        public static readonly MpAvStringHexToBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is string valStr && valStr.IsStringHexColor()) {
                var b = valStr.ToAvBrush();
                if (parameter is string paramStr) {
                    double opacity = 1.0;
                    try {
                        opacity = System.Convert.ToDouble(opacity);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Couldn't convert param '{paramStr}' to a double for opacity", ex);
                        opacity = 1.0;
                    }
                    b.Opacity = opacity;
                }

                return b;
            }
            return MpSystemColors.Transparent.ToAvBrush();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
