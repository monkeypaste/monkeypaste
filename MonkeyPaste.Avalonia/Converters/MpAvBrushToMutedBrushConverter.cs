using Avalonia.Data.Converters;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;

namespace MonkeyPaste.Avalonia {
    public class MpAvBrushToMutedBrushConverter : IValueConverter {
        static double L_HI = 0.8d;
        static double L_LO = 0.2d;
        static double L_MID = 0.5d;
        public static double DEF_MUTE_S => 0.4d;
        public static double DEF_MUTE_L_HI => 
            MpAvThemeViewModel.Instance.IsThemeDark ? L_HI : L_LO;
        public static double DEF_MUTE_L_LO =>
            MpAvThemeViewModel.Instance.IsThemeDark ? L_LO : L_HI;
        public static readonly MpAvBrushToMutedBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if(value is not IBrush b ||
                b.ToHex() is not { } hex) {
                return value;
            }
            MpColorHelpers.ColorToHsl(hex.ToPortableColor(), out double h, out double s, out double l);
            l = DEF_MUTE_L_HI;
            if(parameter is string paramStr) {
                if(paramStr.ToLower() == "lo") {
                    l = DEF_MUTE_L_LO;
                } else if(paramStr.ToLower() == "mid") {
                    l = L_MID;
                } else if(paramStr == "none") {
                    return value;
                }
            }
            s = DEF_MUTE_S;
            return MpColorHelpers.ColorFromHsl(h, s, l).ToAvColor().ToSolidColorBrush();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
