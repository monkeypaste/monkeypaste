using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;
namespace MpWpfApp {
    public static class MpWpfColorHelpers {
        private static Random _Rand;

        private static List<List<Brush>> _colors = new List<List<Brush>> {
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(248, 160, 174)),
                    new SolidColorBrush(Color.FromRgb(243, 69, 68)),
                    new SolidColorBrush(Color.FromRgb(229, 116, 102)),
                    new SolidColorBrush(Color.FromRgb(211, 159, 161)),
                    new SolidColorBrush(Color.FromRgb(191, 53, 50))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(252, 168, 69)),
                    new SolidColorBrush(Color.FromRgb(251, 108, 40)),
                    new SolidColorBrush(Color.FromRgb(253, 170, 130)),
                    new SolidColorBrush(Color.FromRgb(189, 141, 103)),
                    new SolidColorBrush(Color.FromRgb(177, 86, 55))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(215, 157, 60)),
                    new SolidColorBrush(Color.FromRgb(168, 123, 82)),
                    new SolidColorBrush(Color.FromRgb(214, 182, 133)),
                    new SolidColorBrush(Color.FromRgb(162, 144, 122)),
                    new SolidColorBrush(Color.FromRgb(123, 85, 72))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(247, 245, 144)),
                    new SolidColorBrush(Color.FromRgb(252, 240, 78)),
                    new SolidColorBrush(Color.FromRgb(239, 254, 185)),
                    new SolidColorBrush(Color.FromRgb(198, 193, 127)),
                    new SolidColorBrush(Color.FromRgb(224, 200, 42))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(189, 254, 40)),
                    new SolidColorBrush(Color.FromRgb(143, 254, 115)),
                    new SolidColorBrush(Color.FromRgb(217, 231, 170)),
                    new SolidColorBrush(Color.FromRgb(172, 183, 38)),
                    new SolidColorBrush(Color.FromRgb(140, 157, 45))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(50, 255, 76)),
                    new SolidColorBrush(Color.FromRgb(68, 199, 33)),
                    new SolidColorBrush(Color.FromRgb(193, 214, 135)),
                    new SolidColorBrush(Color.FromRgb(127, 182, 99)),
                    new SolidColorBrush(Color.FromRgb(92, 170, 58))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(54, 255, 173)),
                    new SolidColorBrush(Color.FromRgb(32, 195, 178)),
                    new SolidColorBrush(Color.FromRgb(170, 206, 160)),
                    new SolidColorBrush(Color.FromRgb(160, 201, 197)),
                    new SolidColorBrush(Color.FromRgb(32, 159, 148))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(96, 255, 227)),
                    new SolidColorBrush(Color.FromRgb(46, 238, 249)),
                    new SolidColorBrush(Color.FromRgb(218, 253, 233)),
                    new SolidColorBrush(Color.FromRgb(174, 193, 208)),
                    new SolidColorBrush(Color.FromRgb(40, 103, 146))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(149, 204, 243)),
                    new SolidColorBrush(Color.FromRgb(43, 167, 237)),
                    new SolidColorBrush(Color.FromRgb(215, 244, 248)),
                    new SolidColorBrush(Color.FromRgb(153, 178, 198)),
                    new SolidColorBrush(Color.FromRgb(30, 51, 160))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(99, 141, 227)),
                    new SolidColorBrush(Color.FromRgb(22, 127, 193)),
                    new SolidColorBrush(Color.FromRgb(201, 207, 233)),
                    new SolidColorBrush(Color.FromRgb(150, 163, 208)),
                    new SolidColorBrush(Color.FromRgb(52, 89, 170))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(157, 176, 255)),
                    new SolidColorBrush(Color.FromRgb(148, 127, 220)),
                    new SolidColorBrush(Color.FromRgb(216, 203, 233)),
                    new SolidColorBrush(Color.FromRgb(180, 168, 192)),
                    new SolidColorBrush(Color.FromRgb(109, 90, 179))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(221, 126, 230)),
                    new SolidColorBrush(Color.FromRgb(186, 141, 200)),
                    new SolidColorBrush(Color.FromRgb(185, 169, 231)),
                    new SolidColorBrush(Color.FromRgb(203, 178, 200)),
                    new SolidColorBrush(Color.FromRgb(170, 90, 179))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(225, 103, 164)),
                    new SolidColorBrush(Color.FromRgb(252, 74, 210)),
                    new SolidColorBrush(Color.FromRgb(238, 233, 237)),
                    new SolidColorBrush(Color.FromRgb(195, 132, 163)),
                    new SolidColorBrush(Color.FromRgb(205, 60, 117))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(223, 223, 223)),
                    new SolidColorBrush(Color.FromRgb(187, 187, 187)),
                    new SolidColorBrush(Color.FromRgb(137, 137, 137)),
                    new SolidColorBrush(Color.FromRgb(65, 65, 65))
                }
            };
        public static double ColorDistance(Color e1, Color e2) {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = ((long)e1.R + (long)e2.R) / 2;
            long r = (long)e1.R - (long)e2.R;
            long g = (long)e1.G - (long)e2.G;
            long b = (long)e1.B - (long)e2.B;
            double max = 764.83331517396653;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
}

        public static Color GetRandomColor() {
            if (_Rand == null) {
                _Rand = new Random((int)DateTime.Now.Ticks);
}
            int x = _Rand.Next(0, _colors.Count);
            int y = _Rand.Next(0, _colors[0].Count);

            return ((SolidColorBrush)_colors[x][y]).Color;
        }

        public static Color ConvertHexToColor(string hexString) {
            if (hexString.IndexOf('#') != -1) {
                hexString = hexString.Replace("#", string.Empty);
            }
            //
            int x = hexString.Length == 8 ? 2 : 0;
            byte r = byte.Parse(hexString.Substring(x, 2), NumberStyles.AllowHexSpecifier);
            byte g = byte.Parse(hexString.Substring(x + 2, 2), NumberStyles.AllowHexSpecifier);
            byte b = byte.Parse(hexString.Substring(x + 4, 2), NumberStyles.AllowHexSpecifier);
            byte a = x > 0 ? byte.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier) : (byte)255;
            return Color.FromArgb(a, r, g, b);
        }

        public static string ConvertColorToHex(Color c, byte forceAlpha = 255) {
            if (c == null) {
                return "#FF0000";
            }
            c.A = forceAlpha;
            return c.ToString();
        }

        public static System.Drawing.Color GetDominantColor(System.Drawing.Bitmap bmp) {
            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < bmp.Width; x++) {
                for (int y = 0; y < bmp.Height; y++) {
                    System.Drawing.Color clr = bmp.GetPixel(x, y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            return System.Drawing.Color.FromArgb((byte)r, (byte)g, (byte)b);
        }

        public static void ColorToHSV(System.Drawing.Color color, out double hue, out double saturation, out double value) {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static System.Drawing.Color ColorFromHSV(double hue, double saturation, double value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = (hue / 60) - Math.Floor(hue / 60);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - (f * saturation)));
            int t = Convert.ToInt32(value * (1 - ((1 - f) * saturation)));

            if (hi == 0) {
                return System.Drawing.Color.FromArgb(255, (byte)v, (byte)t, (byte)p);
            } else if (hi == 1) {
                return System.Drawing.Color.FromArgb(255, (byte)q, (byte)v, (byte)p);
            } else if (hi == 2) {
                return System.Drawing.Color.FromArgb(255, (byte)p, (byte)v, (byte)t);
            } else if (hi == 3) {
                return System.Drawing.Color.FromArgb(255, (byte)p, (byte)q, (byte)v);
            } else if (hi == 4) {
                return System.Drawing.Color.FromArgb(255, (byte)t, (byte)p, (byte)v);
            } else {
                return System.Drawing.Color.FromArgb(255, (byte)v, (byte)p, (byte)q);
            }
        }

        public static System.Drawing.Color GetInvertedColor(System.Drawing.Color c) {
            ColorToHSV(c, out double h, out double s, out double v);
            h = (h + 180) % 360;
            return ColorFromHSV(h, s, v);
        }

        public static bool IsBright(Color c, int brightThreshold = 150) {
            int grayVal = (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114);
            return grayVal > brightThreshold;
        }

        public static SolidColorBrush ChangeBrushAlpha(SolidColorBrush solidColorBrush, byte alpha) {
            var c = solidColorBrush.Color;
            c.A = alpha;
            solidColorBrush.Color = c;
            return solidColorBrush;
        }

        public static SolidColorBrush ChangeBrushBrightness(SolidColorBrush b, double correctionFactor) {
            if (correctionFactor == 0.0f) {
                return b.Clone();
            }
            double red = (double)b.Color.R;
            double green = (double)b.Color.G;
            double blue = (double)b.Color.B;

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

            return new SolidColorBrush(Color.FromArgb(b.Color.A, (byte)red, (byte)green, (byte)blue));
        }

        public static Brush GetDarkerBrush(Brush b, double factor = -0.5) {
            return ChangeBrushBrightness((SolidColorBrush)b, factor);
        }

        public static Brush GetLighterBrush(Brush b, double factor = 0.5) {
            return ChangeBrushBrightness((SolidColorBrush)b, factor);
        }

        public static Color GetRandomColor(byte alpha = 255) {
            //if (alpha == 255) {
            //    return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            //}
            //return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));

            //int x = Rand.Next(0, _ContentColors.Count);
            //int y = Rand.Next(0, _ContentColors[0].Count);
            //return ((SolidColorBrush)_ContentColors[x][y]).Color;

            return new MpContentColors().GetRandomColor();
        }

        public static Brush GetRandomBrushColor(byte alpha = 255) {
            return (Brush)new SolidColorBrush() { Color = GetRandomColor(alpha) };
        }

        public static string GetColorString(Color c) {
            return (int)c.A + "," + (int)c.R + "," + (int)c.G + "," + (int)c.B;
        }

        public static System.Drawing.Color GetColorFromString(string colorStr) {
            if (string.IsNullOrEmpty(colorStr)) {
                colorStr = GetColorString(GetRandomColor());
            }

            int[] c = new int[colorStr.Split(',').Length];
            for (int i = 0; i < c.Length; i++) {
                c[i] = Convert.ToInt32(colorStr.Split(',')[i]);
            }

            if (c.Length == 3) {
                return System.Drawing.Color.FromArgb(255/*c[3]*/, c[0], c[1], c[2]);
            }

            return System.Drawing.Color.FromArgb(c[3], c[0], c[1], c[2]);
        }
    }
}
