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
            string hexStr = null;
            if (value is IBrush brush) {
                hexStr = brush.ToHex();
            }
            if (value is Color color) {
                hexStr = color.ToPortableColor().ToHex();
            }
            if (value is string valStr) {
                hexStr = valStr;

            }
            if (hexStr.IsStringHexColor()) {
                bool flip = false;
                bool is_fg = true;
                if (parameter is string paramStr &&
                    !string.IsNullOrEmpty(paramStr) &&
                    paramStr.SplitNoEmpty("|") is string[] paramParts) {
                    flip = paramParts.Any(x => x == "flip");
                    is_fg = !paramParts.Any(x => x == "bg");
                }
                if (is_fg) {
                    return hexStr.ToContrastForegoundColor(flip).ToAvColor();
                }
                return hexStr.ToComplementHexColor().ToAvColor();

            }
            return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeBlack.ToString()).ToAvColor();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
