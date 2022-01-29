using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public static class MpColorExtensions {
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
            var scb = hex.ToSolidColorBrush();
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

        public static string ToHex(this Brush b) {
            if (b is SolidColorBrush scb) {
                return scb.Color.ToHex();
            }
            return MpSystemColors.Transparent;
        }
        public static string ToHex(this Color c, byte forceAlpha = 255) {
            if (c == null) {
                return "#FF0000";
            }
            c.A = forceAlpha;
            return c.ToString();
        }
    }
}
