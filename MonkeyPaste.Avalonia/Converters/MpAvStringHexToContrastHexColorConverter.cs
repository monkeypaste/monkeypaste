using Avalonia.Data.Converters;
using MonkeyPaste.Common;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringHexToContrastHexColorConverter : IValueConverter {

        public static readonly MpAvStringHexToContrastHexColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
            bool has_value = value != null && !string.IsNullOrWhiteSpace(value.ToString());
            string fallback_hex = MpSystemColors.Transparent;
            if (!has_value &&
                    parameter is string fparamStr &&
                !string.IsNullOrEmpty(fparamStr)) {
                return Convert(fparamStr, null, null, null);
            }
            if (value is string valStr) {
                var paramParts = parameter.ToStringOrEmpty().Split("|");
                bool ignoreAlpha = paramParts.Any(x => x == "true");
                bool darker = paramParts.Any(x => x == "darker");
                bool lighter = paramParts.Any(x => x == "lighter");
                valStr = MpColorHelpers.ParseHexFromString(valStr, includeAlpha: !ignoreAlpha);
                if (valStr.IsStringHexColor()) {
                    if (darker) {
                        return MpColorHelpers.GetDarkerHexColor(valStr);
                    }
                    if (lighter) {
                        return MpColorHelpers.GetLighterHexColor(valStr);
                    }
                    return valStr.ToContrastForegoundColor();
                }
            }
            return fallback_hex;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
