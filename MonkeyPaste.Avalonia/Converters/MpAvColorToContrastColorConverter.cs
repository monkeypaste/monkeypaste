using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions;
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
            if (hexStr != null && (hexStr.ToLower() == "#ff252226" || hexStr.ToLower() == "#252226")) {

            }
            if (hexStr.IsStringHexColor()) {
                bool flip = false;
                string contrast_type = "fg";
                if (parameter is string paramStr &&
                    !string.IsNullOrEmpty(paramStr) &&
                    paramStr.SplitNoEmpty("|") is string[] paramParts) {
                    flip = paramParts.Any(x => x == "flip");
                    if (paramParts.FirstOrDefault(x => x != "flip") is string paramContrastType &&
                        !string.IsNullOrEmpty(paramContrastType)) {
                        contrast_type = paramContrastType;
                    }
                }
                switch (contrast_type) {
                    case "compliment":
                        return hexStr.ToContrastHexColor().ToAvColor();
                    case "lighter":
                        return MpColorHelpers.GetLighterHexColor(hexStr).ToAvColor();
                    case "darker":
                        return MpColorHelpers.GetDarkerHexColor(hexStr).ToAvColor();
                    case "fg":
                    default:
                        return hexStr.ToContrastForegoundColor(flip).ToAvColor();
                }

            }
            return null;
            //MpDebug.Break($"Unhandled color '{hexStr}'");
            //return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeBlackColor.ToString()).ToAvColor();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
