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

        public static Brush ToAvBrush(this string hexColor) {            
            return new SolidColorBrush(hexColor.ToAvColor());
        }

        public static Pen ToAvPen(
            this string octColor, 
            double thickness = 1.0d, 
            double[] dashStyle = null, 
            double dashOffset = 0,
            string lineCap = "Flat",
            string lineJoin = "Miter",
            double miterLimit = 10.0) {
            dashStyle = dashStyle == null ? new double[] { 1, 1, 0 } : dashStyle;
            return new Pen(
                octColor.ToAvBrush(), 
                thickness, 
                new DashStyle(dashStyle,dashOffset),
                lineCap.ToEnum<PenLineCap>(),
                lineJoin.ToEnum<PenLineJoin>(),
                miterLimit);
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

        public static Color AdjustOpacity(this Color color, double opacity) {
            if(color.A != 255 && opacity != 1.0d) {
                // warning, color already has adjusted opacity
                Debugger.Break();
            }
            var adjustedColor = new Color(
                Math.Max((byte)255, (byte)((double)255 * opacity)),
                color.R,
                color.G,
                color.B);
            return adjustedColor;
        }
        public static IBrush AdjustOpacity(this IBrush brush, double opacity) {
            if(brush is SolidColorBrush scb) {
                scb.Color = scb.Color.AdjustOpacity(opacity);
                return scb;
            }
            // warning, not solid color brush
            Debugger.Break();
            return brush;
        }
    }
}
