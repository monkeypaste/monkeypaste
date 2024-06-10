using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBrushToDropShadowEffectConverter : IValueConverter {
        public static readonly MpAvBrushToDropShadowEffectConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is not IBrush b ||
                b.ToHex() is not { } brush_hex) {
                return null;
            }
            var result = brush_hex.IsHexStringBright() ?
                Mp.Services.PlatformResource.GetResource<Effect>(MpThemeResourceKey.ThemeBlackDropShadow) :
                Mp.Services.PlatformResource.GetResource<Effect>(MpThemeResourceKey.ThemeWhiteDropShadow);
            return result;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
