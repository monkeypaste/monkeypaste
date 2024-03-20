using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvColorToContrastColorConverter : IValueConverter {
        public static readonly MpAvColorToContrastColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            string hexStr = value.ToHex();
            if (!hexStr.IsStringHexColor()) {
                return null;
            }
            bool flip = false;
            bool use_hex = false;
            string contrast_type = "fg";

            string result_hex = hexStr;

            if (parameter is string paramStr &&
                !string.IsNullOrEmpty(paramStr) &&
                paramStr.SplitNoEmpty("|") is string[] paramParts) {
                flip = paramParts.Any(x => x == "flip");
                use_hex = paramParts.Any(x => x == "hex");
                if (paramParts.FirstOrDefault(x => x != "flip") is string paramContrastType &&
                    !string.IsNullOrEmpty(paramContrastType)) {
                    contrast_type = paramContrastType;
                }
            }
            switch (contrast_type) {
                case "compliment":
                    result_hex = hexStr.ToContrastHexColor();
                    break;
                case "lighter":
                    result_hex = MpColorHelpers.GetLighterHexColor(hexStr);
                    break;
                case "darker":
                    result_hex = MpColorHelpers.GetDarkerHexColor(hexStr);
                    break;
                case "fg":
                default:
                    result_hex = hexStr.ToContrastForegoundColor(flip);
                    break;
            }
            if (use_hex) {
                // return hex
                return result_hex;
            }
            if (targetType == typeof(Color)) {
                return result_hex.ToAvColor();
            }
            return result_hex.ToAvBrush();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
