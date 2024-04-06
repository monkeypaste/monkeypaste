using MonkeyPaste.Common.Plugin;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Common {
    public enum MpColorComplimentType {
        None = 0,
        Complimentary,
        SplitComplimentary,
        Triadic,
        Tetradic,
        Analagous,
        Monochromatic
    }
    public static class MpColorHelpers {
        public static string RgbaToHex(byte r, byte g, byte b, byte a = 255) {
            byte[] argb = new byte[] { a, r, g, b };
            return "#" + BitConverter.ToString(argb).Replace("-", "");
        }

        public static string ParseHexFromString(string hexChannelsOrNamedColorStr, string fallBack = "#00000000", bool includeAlpha = true) {
            if (string.IsNullOrWhiteSpace(hexChannelsOrNamedColorStr)) {
                return fallBack.IncludeOrRemoveHexAlpha(!includeAlpha);
            }
            if (hexChannelsOrNamedColorStr.IsStringHexColor()) {
                return hexChannelsOrNamedColorStr.IncludeOrRemoveHexAlpha(!includeAlpha);
            }
            if (hexChannelsOrNamedColorStr.Contains(",") &&
                hexChannelsOrNamedColorStr
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace("rgba", string.Empty)
                .Replace("rgb", string.Empty) is string cleanStr &&
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
                    return new MpColor(argb_channels).ToHex(!includeAlpha);

                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"error converting string to color '{hexChannelsOrNamedColorStr}'", ex);
                    return fallBack;
                }

            }
            var hexColorPropInfo = typeof(MpSystemColors)
                .GetProperties()
                .FirstOrDefault(x => x.Name.ToLowerInvariant() == hexChannelsOrNamedColorStr.ToLowerInvariant());

            if (hexColorPropInfo == null) {
                hexColorPropInfo = typeof(MpSystemColors)
                .GetProperties()
                .FirstOrDefault(x => x.Name.ToLowerInvariant().StartsWith(hexChannelsOrNamedColorStr.ToLowerInvariant()));
            }
            if (hexColorPropInfo == null) {
                MpConsole.WriteTraceLine($"Color named {hexChannelsOrNamedColorStr} not found, returning {fallBack}");
                return fallBack;
            }
            return (hexColorPropInfo.GetValue(null, null) as string).IncludeOrRemoveHexAlpha(!includeAlpha);
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

        public static string GetRandomHexColor(bool includeAlpha = true, bool exclude_grays = true) {
            int total_cols = exclude_grays ? MpSystemColors.COLOR_PALETTE_COLS - 1 : MpSystemColors.COLOR_PALETTE_COLS;
            int rand_col = MpRandom.Rand.Next(0, total_cols);
            int rand_row = MpRandom.Rand.Next(0, MpSystemColors.COLOR_PALETTE_ROWS);

            int idx = (rand_row * MpSystemColors.COLOR_PALETTE_COLS) + rand_col;
            string hex = MpSystemColors.ContentColors[idx];
            if (includeAlpha) {
                return hex;
            }
            return hex.Replace("#FF", "#");
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


        public static string ChangeHexBrightness(string hexStr, double correctionFactor, bool removeAlpha = true) {
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

            return new MpColor(removeAlpha ? (byte)255 : c.A, (byte)red, (byte)green, (byte)blue).ToHex();
        }

        public static string GetDarkerHexColor(string hexStr, double factor = -0.5) {
            return ChangeHexBrightness(hexStr, factor);
        }

        public static string GetLighterHexColor(string hexStr, double factor = 0.5) {
            return ChangeHexBrightness(hexStr, factor);
        }
        public static string MakeBright(string hexStr, double delta_factor = 0.1, double factor = 0, int brightThreshold = 150) {
            if (IsBright(hexStr, brightThreshold)) {
                return hexStr;
            }
            factor += delta_factor;
            hexStr = GetLighterHexColor(hexStr, factor);
            return MakeBright(hexStr, delta_factor, factor, brightThreshold);
        }
        public static string MakeDark(string hexStr, double delta_factor = 0.1, double factor = 0, int brightThreshold = 150) {
            if (!IsBright(hexStr, brightThreshold)) {
                return hexStr;
            }
            factor -= delta_factor;
            hexStr = GetDarkerHexColor(hexStr, factor);
            return MakeDark(hexStr, delta_factor, factor, brightThreshold);
        }

        #region HSV
        // H [0..360]
        // S [0..1]
        // V [0..1]
        public static void ColorToHsv(this MpColor color, out double hue, out double saturation, out double value) {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static double GetHue(this MpColor c) {
            Color col = Color.FromArgb(c.A, c.R, c.G, c.B);
            return (double)col.GetHue();
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

        #region HSL
        public static void ColorToHsl(this MpColor color, out double h, out double s, out double l) {
            // from https://www.programmingalgorithms.com/algorithm/rgb-to-hsl/

            var norm_channels = color.NormalizeChannels();
            int r_idx = norm_channels.Length == 3 ? 0 : 1;
            double r = norm_channels[r_idx];
            double g = norm_channels[r_idx + 1];
            double b = norm_channels[r_idx + 2];

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));

            double delta = max - min;

            l = (max + min) / 2;

            if (delta == 0) {
                h = 0;
                s = 0.0d;
            } else {
                s = (l <= 0.5) ? (delta / (max + min)) : (delta / (2 - max - min));

                double hue;

                if (r == max) {
                    hue = ((g - b) / 6) / delta;
                } else if (g == max) {
                    hue = (1.0f / 3) + ((b - r) / 6) / delta;
                } else {
                    hue = (2.0f / 3) + ((r - g) / 6) / delta;
                }

                if (hue < 0) {
                    hue += 1;
                }
                if (hue > 1) {
                    hue -= 1;
                }
                h = (hue * 360);
            }
            // sample output:
            // H: 296
            //S: 1
            //L: 0.17058824f
        }

        public static MpColor ColorFromHsl(double h, double s, double l) {
            byte r, g, b;

            if (s == 0) {
                r = g = b = (byte)(l * 255);
            } else {
                double v1, v2;
                double hue = h / 360;

                v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
                v1 = 2 * l - v2;

                r = (byte)(255 * HueToRGB(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * HueToRGB(v1, v2, hue));
                b = (byte)(255 * HueToRGB(v1, v2, hue - (1.0f / 3)));
            }

            return new MpColor(r, g, b);
        }

        private static double HueToRGB(double v1, double v2, double vH) {
            if (vH < 0) {
                vH += 1;
            }
            if (vH > 1) {
                vH -= 1;
            }
            if ((6 * vH) < 1) {
                return (v1 + (v2 - v1) * 6 * vH);
            }
            if ((2 * vH) < 1) {
                return v2;
            }
            if ((3 * vH) < 2) {
                return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);
            }
            return v1;
        }

        #endregion
    }
}
