using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
    public static class MpColorExtensions {
        public static Brush ToSolidColorBrush(this string hex, double opacity = 1.0) {
            if (string.IsNullOrEmpty(hex)) {
                return Brushes.Red;
            }
            var br = (Brush)new SolidColorBrush(hex.ToWinMediaColor());
            br.Opacity = opacity;
            return br;
        }

        public static Color ToWinMediaColor(this string hex) {
            if (string.IsNullOrEmpty(hex)) {
                return Colors.Red;
            }
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        public static System.Drawing.Color ToWinFormsColor(this Brush b) {
            if (b.GetType() != typeof(SolidColorBrush)) {
                throw new Exception("Brush must be solid color brush");
            }
            var scb = (SolidColorBrush)b;
            return System.Drawing.Color.FromArgb(scb.Color.A, scb.Color.R, scb.Color.G, scb.Color.B);
        }

        public static SolidColorBrush ToSolidColorBrush(this System.Drawing.Color c) {
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public static string ToHex(this Brush b) {
            if (b is SolidColorBrush scb) {
                return scb.Color.ToHex();
            }
            throw new Exception("Brush must be solid color brush but is " + b.GetType().ToString());
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
