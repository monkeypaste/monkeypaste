using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste.Common {

    public static class MpColorHelpers {
        public static string RgbaToHex(byte r, byte g, byte b, byte a = 255) {
            byte[] argb = new byte[] { a, r, g, b };
            return "#" + BitConverter.ToString(argb).Replace("-", "");
        }

        public static byte[] GetHexColorBytes(string hexString) {
            if (!hexString.IsStringHexColor()) {
                throw new Exception("Not hex color");
            }
            if (hexString.IndexOf('#') != -1) {
                hexString = hexString.Replace("#", string.Empty);
            }
            //
            int x = hexString.Length == 8 ? 2 : 0;
            byte r = byte.Parse(hexString.Substring(x, 2), NumberStyles.AllowHexSpecifier);
            byte g = byte.Parse(hexString.Substring(x + 2, 2), NumberStyles.AllowHexSpecifier);
            byte b = byte.Parse(hexString.Substring(x + 4, 2), NumberStyles.AllowHexSpecifier);
            byte a = x > 0 ? byte.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier) : (byte)255;

            return new byte[] { a, r, g, b };
        }

        public static string GetRandomHexColor() {
            int idx = MpRandom.Rand.Next(0, MpSystemColors.ContentColors.Count);
            return MpSystemColors.ContentColors[idx];
        }


        public static bool IsBright(string hexStr, int brightThreshold = 150) {
            MpColor c = new MpColor(GetHexColorBytes(hexStr));
            int grayVal = (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114);
            return grayVal > brightThreshold;
        }

        public static string ChangeBrushBrightness(string hexStr, double correctionFactor) {
            if (correctionFactor == 0.0f) {
                return hexStr;
            }
            var c = new MpColor(hexStr);
            double red = (double)c.R;
            double green = (double)c.G;
            double blue = (double)c.B;

            if (correctionFactor < 0) {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            } else {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return new MpColor(c.A, (byte)red, (byte)green, (byte)blue).ToHex();
        }

        public static string GetDarkerHexColor(string hexStr, double factor = -0.5) {
            return ChangeBrushBrightness(hexStr, factor);
        }

        public static string GetLighterHexColor(string hexStr, double factor = 0.5) {
            return ChangeBrushBrightness(hexStr, factor);
        }
    }
}
