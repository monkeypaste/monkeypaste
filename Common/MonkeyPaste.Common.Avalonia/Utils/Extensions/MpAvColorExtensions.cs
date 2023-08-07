using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvColorExtensions {

        public static string ToHex(this object colorBrushHexOrNamedColor, string fallback = default) {
            if (colorBrushHexOrNamedColor is IBrush brush) {
                return brush.ToHex();
            }
            if (colorBrushHexOrNamedColor is Color color) {
                return color.ToPortableColor().ToHex();
            }
            if (colorBrushHexOrNamedColor is string valStr) {
                return MpColorHelpers.ParseHexFromString(valStr);
            }
            if (colorBrushHexOrNamedColor is DynamicResourceExtension dre) {
                return MpCommonTools.Services.PlatformResource.GetResource<string>(dre.ResourceKey.ToString());
            }
            //MpDebug.Break($"Could not convert '{colorBrushHexOrNamedColor.GetType()}' to hex");
            return fallback;
        }

        public static Color GetColor(this IBrush b) {
            if (b is IImmutableSolidColorBrush iscb) {
                return iscb.Color;
            }
            if (b is SolidColorBrush scb) {
                return scb.Color;
            }
            MpDebug.Break("Unsupported brush type for color");
            return MpColorHelpers.GetRandomHexColor().ToAvColor();
        }
        public static Color ToAvColor(this string hexOrNamedColor, string fallback = default, byte forceAlpha = default) {
            var c = new MpColor(hexOrNamedColor, fallback, forceAlpha);
            return c.ToAvColor();
        }

        public static Brush ToAvBrush(this string hexOrNamedColor, string fallback = default, double force_alpha = -1) {
            hexOrNamedColor = force_alpha < 0 ? hexOrNamedColor : hexOrNamedColor.AdjustAlpha(force_alpha);
            return new SolidColorBrush(hexOrNamedColor.ToAvColor(fallback));
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
                new DashStyle(dashStyle, dashOffset),
                lineCap.ToEnum<PenLineCap>(),
                lineJoin.ToEnum<PenLineJoin>(),
                miterLimit);
        }
        public static MpColor ToPortableColor(this Color color) {
            return new MpColor(color.A, color.R, color.G, color.B);
        }

        public static IBrush ToSolidColorBrush(this Color color) {
            return new SolidColorBrush(color);
        }

        public static MpColor ToPortableColor(this IBrush brush) {
            if (brush is ISolidColorBrush scb) {
                return scb.Color.ToPortableColor();
            }
            // what is it?
            MpDebug.Break();
            return MpSystemColors.Red.ToPortableColor();
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
            if (b == null) {
                return MpSystemColors.Red;
            }

            return b.ToPortableColor().ToHex();
        }

        public static string ToHex(this IBrush b) {
            if (b is Brush brush) {
                return brush.ToHex();
            }
            if (b is ImmutableSolidColorBrush iscb) {
                return iscb.Color.ToPortableColor().ToHex();
            }
            // what type is it?
            MpDebug.Break();
            return null;
        }

        public static Color AdjustOpacity(this Color color, double opacity) {
            if (color.A != 255 && opacity != 1.0d) {
                // warning, color already has adjusted opacity
                MpDebug.Break();
            }
            var adjustedColor = new Color(
                Math.Max((byte)255, (byte)((double)255 * opacity)),
                color.R,
                color.G,
                color.B);
            return adjustedColor;
        }
        public static IBrush AdjustOpacity(this IBrush brush, double opacity) {
            if (brush is SolidColorBrush scb) {
                scb.Color = scb.Color.AdjustOpacity(opacity);
                return scb;
            }
            // warning, not solid color brush
            MpDebug.Break();
            return brush;
        }
    }
}
