using System;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Common {

    public static class MpColorHelpers {
        public static string RgbaToHex(byte r, byte g, byte b, byte a = 255) {
            byte[] argb = new byte[] { a, r, g, b };
            return "#" + BitConverter.ToString(argb).Replace("-", "");
        }

        public static string ParseHexFromString(string hexChannelsOrNamedColorStr, string fallBack = "#00000000", bool includeAlpha = true) {
            if (string.IsNullOrWhiteSpace(hexChannelsOrNamedColorStr)) {
                return fallBack;
            }
            if (hexChannelsOrNamedColorStr.IsStringHexColor()) {
                return hexChannelsOrNamedColorStr;
            }
            if (hexChannelsOrNamedColorStr.Contains(",") &&
                hexChannelsOrNamedColorStr.Replace("(", string.Empty).Replace(")", string.Empty) is string cleanStr &&
                cleanStr.SplitNoEmpty(",") is string[] colorParts &&
                (colorParts.Length == 3 || colorParts.Length == 4)) {
                // NOTE presumes color is 3-4 decimal elements from 0.0-1.0 or 3-4 int elements from 0-255
                // channels should be in ARGB format
                try {
                    byte[] argb_channels = null;
                    if (colorParts.Any(x => x.Contains("."))) {
                        // normalized decimal color

                        argb_channels = colorParts.Select(x => (byte)(double.Parse(x) * 255)).ToArray();
                    } else {
                        // byte color
                        argb_channels = colorParts.Select(x => byte.Parse(x)).ToArray();
                    }
                    return new MpColor(argb_channels).ToHex();

                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"error converting string to color '{hexChannelsOrNamedColorStr}'", ex);
                    return fallBack;
                }

            }
            var hexColorPropInfo = typeof(MpSystemColors)
                .GetProperties()
                .FirstOrDefault(x => x.Name.ToLower() == hexChannelsOrNamedColorStr.ToLower());

            if (hexColorPropInfo == null) {
                hexColorPropInfo = typeof(MpSystemColors)
                .GetProperties()
                .FirstOrDefault(x => x.Name.ToLower().StartsWith(hexChannelsOrNamedColorStr.ToLower()));
            }
            if (hexColorPropInfo == null) {
                MpConsole.WriteTraceLine($"Color named {hexChannelsOrNamedColorStr} not found, returning {fallBack}");
                return fallBack;
            }
            return hexColorPropInfo.GetValue(null, null) as string;
        }
        public static byte[] GetHexColorBytes(string hexString) {
            if (!hexString.IsStringHexColor()) {
                throw new Exception("Not hex color");
            }
            if (hexString.IndexOf('#') != -1) {
                hexString = hexString.Replace("#", string.Empty);
            }

            int x = hexString.Length == 8 ? 2 : 0;
            byte a = x > 0 ? byte.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier) : (byte)255;
            byte r = byte.Parse(hexString.Substring(x, 2), NumberStyles.AllowHexSpecifier);
            byte g = byte.Parse(hexString.Substring(x + 2, 2), NumberStyles.AllowHexSpecifier);
            byte b = byte.Parse(hexString.Substring(x + 4, 2), NumberStyles.AllowHexSpecifier);

            return new byte[] { a, r, g, b };
        }

        public static string GetRandomHexColor() {
            int idx = MpRandom.Rand.Next(0, MpSystemColors.ContentColors.Count);
            return MpSystemColors.ContentColors[idx];
        }


        public static bool IsBright(string hexStr, int brightThreshold = 150) {
            MpColor c = new MpColor(hexStr);
            int grayVal = (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114);
            return grayVal > brightThreshold;
        }


        public static bool IsHexStringBright(this string hexStr, int brightThreshold = 150) {
            return IsBright(hexStr, brightThreshold);
        }
        public static bool IsHexStringTransparent(this string hexStr) {
            if (hexStr.IsStringHexColor()) {
                return new MpColor(hexStr).A == 0;
            }
            return false;
        }

        public static string HexColorToContrastingFgHexColor(
            this string hexStr,
            string darkHexColor = null,
            string lightHexColor = null,
            int brighThreshold = 150) {
            if (string.IsNullOrEmpty(hexStr)) {
                return darkHexColor ?? MpSystemColors.Black;
            }
            if (hexStr.IsHexStringBright(brighThreshold)) {
                return darkHexColor ?? MpSystemColors.Black;
            }
            return lightHexColor ?? MpSystemColors.White;
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

        #region HSV
        public static void ColorToHsv(MpColor color, out double hue, out double saturation, out double value) {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static MpColor ColorFromHsv(double hue, double saturation, double value) {
            var c = DrawingColorFromHSV(hue, saturation, value);
            return new MpColor(c.R, c.G, c.B);
        }
        private static Color DrawingColorFromHSV(double hue, double saturation, double value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
        #endregion
    }
}
