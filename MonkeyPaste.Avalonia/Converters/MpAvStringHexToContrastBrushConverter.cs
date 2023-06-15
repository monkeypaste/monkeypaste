using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;
using System.Linq;

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
                var paramParts = parameter.ToStringOrEmpty().Split("|");
                bool ignoreAlpha = paramParts.Any(x => x == "true");
                bool darker = paramParts.Any(x => x == "darker");
                bool lighter = paramParts.Any(x => x == "lighter");
                valStr = MpColorHelpers.ParseHexFromString(valStr, includeAlpha: !ignoreAlpha);
                if (valStr.IsStringHexColor()) {
                    if (darker) {
                        return MpColorHelpers.GetDarkerHexColor(valStr).ToAvBrush();
                    }
                    if (lighter) {
                        return MpColorHelpers.GetLighterHexColor(valStr).ToAvBrush();
                    }
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
