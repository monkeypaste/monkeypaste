using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvIconSourceObjToRandomBrushConverter : IValueConverter {
        public static readonly MpAvIconSourceObjToRandomBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is not string valueStr) {
                return Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveColor);
            }
            int max = MpSystemColors.COLOR_PALETTE_COLS - 2;
            int randColorSeed = valueStr.ToCharArray().Sum(x=>(int)x);
            if (valueStr.IsStringImageResourceKey() &&
                Mp.Services.PlatformResource.GetResource<string>(valueStr) is string res_path) {
                // when paramValue is key add its resource path's length to vary color more
                randColorSeed += res_path.Length;
            }
            int rand_idx = (randColorSeed % max) * MpSystemColors.COLOR_PALETTE_ROWS;
            string hex = MpSystemColors.ContentColors[rand_idx].RemoveHexAlpha();
            if (MpAvThemeViewModel.Instance.IsThemeDark) {
                hex = MpColorHelpers.MakeBright(hex);
            } else {
                hex = MpColorHelpers.MakeDark(hex);
            }
            return hex.ToAvBrush(force_alpha: 1);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
