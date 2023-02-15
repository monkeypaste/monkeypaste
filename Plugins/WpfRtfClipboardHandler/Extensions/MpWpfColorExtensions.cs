using MonkeyPaste.Common;
using System;
using System.Windows.Media;

namespace WpfRtfClipboardHandler {
    public static class MpWpfColorExtensions {

        public static double ColorDistance(this Color e1, Color e2) {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = ((long)e1.R + (long)e2.R) / 2;
            long r = (long)e1.R - (long)e2.R;
            long g = (long)e1.G - (long)e2.G;
            long b = (long)e1.B - (long)e2.B;
            double max = 764.83331517396655;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }
        public static Brush ToWpfBrush(this string hex, double opacity = 1.0) {
            return hex.ToSolidColorBrush(opacity);
        }

        public static Brush ToSolidColorBrush(this string hex, double opacity = 1.0) {
            if (!hex.IsStringHexColor()) {
                return Brushes.Transparent;
            }
            var br = (Brush)new SolidColorBrush(hex.ToWinMediaColor());
            br.Opacity = opacity;
            return br;
        }

        public static Color ToWinMediaColor(this string hex) {
            if (!hex.IsStringHexColor()) {
                return Colors.Transparent;
            }
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        public static System.Drawing.Color ToWinFormsColor(this string hex) {
            if (!hex.IsStringHexColor()) {
                return System.Drawing.Color.Transparent;
            }
            var scb = hex.ToWpfBrush();
            return scb.ToWinFormsColor();
        }

        public static System.Drawing.Color ToWinFormsColor(this Brush b) {
            if (b.GetType() != typeof(SolidColorBrush)) {
                throw new Exception("Brush must be solid color brush");
            }
            var scb = (SolidColorBrush)b;
            return System.Drawing.Color.FromArgb(scb.Color.A, scb.Color.R, scb.Color.G, scb.Color.B);
        }
        public static string ToHex(this System.Drawing.Color c) {
            return c.ToSolidColorBrush().ToHex();
        }

        public static SolidColorBrush ToSolidColorBrush(this System.Drawing.Color c) {
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public static string ToHex(this Brush b, bool omitAlpha = false) {
            if (b is SolidColorBrush scb) {
                return scb.Color.ToHex(omitAlpha: omitAlpha);
            }
            return MpSystemColors.Transparent;
        }
        public static string ToHex(this Color c, byte forceAlpha = 255, bool omitAlpha = false) {
            if (c == null) {
                return "#FF0000";
            }
            c.A = forceAlpha;
            string hex = c.ToString();
            if (omitAlpha && hex.Length == 9) {
                hex = "#" + c.ToString().Substring(3);
            }
            return hex;
        }
    }
}
