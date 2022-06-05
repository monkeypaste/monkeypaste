using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste.Common {
    public static class MpColorExtensions {
        public static int ColorDistance(this SKColor a, SKColor b) {
            // from https://stackoverflow.com/a/3968341/105028

            byte a_intensity = a.ToGrayScale().Red;
            byte b_intensity = b.ToGrayScale().Red;
            return (int)(((a_intensity - b_intensity) * 100) / 255);
        }

        public static string ToHex(this byte[] bytes) {
            if (bytes == null) {
                throw new Exception("Bytes are null");
            }
            return "#" + BitConverter.ToString(bytes).Replace("-", string.Empty);
        }
        
        public static string GetHexString(this Xamarin.Forms.Color color) {
            var red = (int)(color.R * 255);
            var green = (int)(color.G * 255);
            var blue = (int)(color.B * 255);
            var alpha = (int)(color.A * 255);
            var hexString = color.ToHex();
            return hexString;
        }

        public static SKColor ToGrayScale(this SKColor c) {
            // from https://stackoverflow.com/a/3968341/105028
            byte intensity = (byte)((double)c.Blue * 0.11 + (double)c.Green * 0.59 + (double)c.Red * 0.3);
            return new SKColor(intensity, intensity, intensity);
        }

        public static SKColor ToSkColor(this string hexColor) {
            return Color.FromHex(hexColor).ToSKColor();
        }

        public static SKColor ToSKColor(this Color c) {
            return new SKColor((byte)(c.R * 255), (byte)(c.G * 255), (byte)(c.B * 255), (byte)(c.A * 255));
        }

        public static Color GetColor(this string hexString) {
            if (hexString.IndexOf('#') != -1) {
                hexString = hexString.Replace("#", "");
            }

            int r = int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            int g = int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            int b = int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);
            int a = hexString.Length == 8 ?
                        int.Parse(hexString.Substring(6, 2), NumberStyles.AllowHexSpecifier) :
                        255;
            return Color.FromRgba(r, g, b, a);
        }

        public static string AdjustAlpha(this string hexStr, double opacity) {
            // opacity is 0-1
            if(!hexStr.IsStringHexColor()) {
                throw new Exception("Not a hex color");
            }
            var c = new MpColor(hexStr);
            c.A = (byte)(255.0 * opacity);
            return c.ToHex();
        }
    }
}
