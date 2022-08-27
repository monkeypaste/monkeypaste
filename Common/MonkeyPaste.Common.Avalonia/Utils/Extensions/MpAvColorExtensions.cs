using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using System.Diagnostics;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvColorExtensions {
        public static Color ToAvColor(this string hexColor) {
            if(!hexColor.IsStringHexColor()) {
                return Colors.Transparent;
            }
            var c = new MpColor(hexColor);
            return c.ToAvColor();
        }

        public static Brush ToAvBrush(this string hexColor, double opacity = 1.0d) {            
            return new SolidColorBrush(hexColor.AdjustAlpha(opacity).ToAvColor());
        }
        public static MpColor ToPortableColor(this Color color) {
            return new MpColor(color.A, color.R, color.G, color.B);
        }

        public static Color ToAvColor(this MpColor color) {
            return new Color(color.A, color.R, color.G, color.B);
        }

        public static MpColor ToPortableColor(this PixelColor color) {
            return new MpColor(color.Red, color.Green, color.Blue);
        }

        public static Color ToAvPixelColor(this MpColor color) {
            return new Color(255, color.R, color.G, color.B);
        }

        public static string ToHex(this Brush b) {
            if(b is SolidColorBrush scb) {
                if(!scb.ToString().IsStringHexColor()) {
                    Debugger.Break();
                }
                return scb.ToString();
            }
            Debugger.Break();
            return MpSystemColors.Transparent;
        }
    }
}
