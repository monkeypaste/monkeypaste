using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToBrushConverter : IValueConverter {
        public static readonly MpAvStringHexToBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            bool has_value = value != null && !string.IsNullOrWhiteSpace(value.ToString());
            IBrush fallback = MpSystemColors.Transparent.ToAvBrush();
            if (!has_value &&
                    parameter is string fparamStr &&
                    fparamStr.IsStringHexColor()) {
                return Convert(fparamStr, null, null, null) as IBrush;
            }
            if (value is string valStr) {
                if (!valStr.IsStringHexColor()) {
                    valStr = MpColorHelpers.ParseHexFromString(valStr);
                }
                if (!valStr.IsStringHexColor()) {
                    return fallback;
                }
                Brush b = valStr.ToAvBrush();
                if (parameter is string paramStr && !paramStr.IsStringHexColor()) {
                    if(paramStr.StartsWith("muted")) {
                        b = MpAvBrushToMutedBrushConverter.Instance.Convert(
                            b, targetType, paramStr.Replace("muted", string.Empty), culture) as Brush;
                    } else {
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
                }

                return b;
            }
            return fallback;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
