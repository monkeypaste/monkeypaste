using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToContrastBrushConverter : IValueConverter {
        public static readonly MpAvStringHexToContrastBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
            bool has_value = value != null && !string.IsNullOrWhiteSpace(value.ToString());
            IBrush fallback = MpSystemColors.Transparent.ToAvBrush();
            if (!has_value &&
                    parameter is string fparamStr &&
                !string.IsNullOrEmpty(fparamStr)) {
                return Convert(fparamStr, null, null, null) as IBrush;
            }
            if (value is string valStr) {
                bool ignoreAlpha = parameter.ToStringOrDefault() == "true";
                valStr = MpColorHelpers.ParseHexFromString(valStr, includeAlpha: !ignoreAlpha);
                if (valStr.IsStringHexColor()) {
                    return valStr.ToContrastForegoundColor().ToAvBrush();
                }
            }
            return fallback;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
