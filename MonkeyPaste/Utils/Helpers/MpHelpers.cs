using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using FFImageLoading.Work;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MonkeyPaste {
    public static class MpHelpers {

        private static Random _rand;
        public static Random Rand { 
            get {
                if(_rand == null) {
                    _rand = new Random((int)DateTime.Now.Ticks);
                }
                return _rand;
            }
        }

        #region Documents

        public static string Diff(string str1, string str2) {
            if (str1 == null) {
                return str2;
            }
            if (str2 == null) {
                return str1;
            }

            List<string> set1 = str1.Split(' ').Distinct().ToList();
            List<string> set2 = str2.Split(' ').Distinct().ToList();

            var diff = set2.Count() > set1.Count() ? set2.Except(set1).ToList() : set1.Except(set2).ToList();

            return string.Join("", diff);
        }

        public static string LoadTextResource(string resourcePath) {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpCopyItem)).Assembly;
            var stream = assembly.GetManifestResourceStream(resourcePath); 
            using (var reader = new System.IO.StreamReader(stream)) {
                var res = reader.ReadToEnd();
                return res;
            }
        }
        public static SKBitmap LoadBitmapResource(string resourcePath) {
            // Ensure "this" is an object that is part of your implementation within your Xamarin forms project
            var assembly = typeof(MpHelpers).GetTypeInfo().Assembly;
            byte[] buffer = null;

            using (System.IO.Stream s = assembly.GetManifestResourceStream(resourcePath)) {
                if (s != null) {
                    long length = s.Length;
                    buffer = new byte[length];
                    s.Read(buffer, 0, (int)length);
                }
            }

            return new MpImageConverter().Convert(buffer,typeof(SKBitmap)) as SKBitmap;
        }

        
        #endregion

        #region System

        public static int ParseEnumValue(Type enumType, string typeStr) {
            for (int i = 0; i < Enum.GetValues(enumType).Length; i++) {
                if (Enum.GetName(enumType, i).ToLower() == typeStr.ToLower()) {
                    return i;
                }
            }
            return 0;
        }

        #endregion

        #region Visual

        private static List<List<Brush>> _ContentColors = new List<List<Brush>> {
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

        public static Brush GetContentColor(int c, int r) {
            return _ContentColors[c][r];
        }
        public static List<KeyValuePair<SKColor, int>> GetHistogram(SKBitmap bitmap) {
            var countDictionary = new Dictionary<SKColor, int>();
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    SKColor currentColor = bitmap.GetPixel(x, y);
                    //if (currentColor.Alpha == 0) {
                    //    continue;
                    //}
                    //If a record already exists for this color, set the count, otherwise just set it as 0
                    int currentCount = countDictionary.ContainsKey(currentColor) ? countDictionary[currentColor] : 0;

                    if (currentCount == 0) {
                        //If this color doesnt already exists in the dictionary, add it
                        countDictionary.Add(currentColor, 1);
                    } else {
                        //If it exists, increment the value and update it
                        countDictionary[currentColor] = currentCount + 1;
                    }
                }
            }
            //order the list from most used to least used before returning
            return countDictionary.OrderByDescending(o => o.Value).ToList();
        }
        public static List<string> CreatePrimaryColorList(SKBitmap skbmp, int listCount = 5) {
            //var sw = new Stopwatch();
            //sw.Start();

            var primaryIconColorList = new List<string>();
            List<KeyValuePair<SKColor, int>> hist = GetHistogram(skbmp);
            //foreach (var kvp in hist) {
            //    //var c = Color.FromRgba(kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, 255);
            //    SKColor c = kvp.Key;
            //    c = new SKColor(c.Red, c.Green, c.Blue, 255);
            //    //Console.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
            //    if (primaryIconColorList.Count == listCount) {
            //        break;
            //    }

            //    //between 0-255 where 0 is black 255 is white
            //    var rgDiff = Math.Abs(c.Red - c.Green);
            //    var rbDiff = Math.Abs(c.Red - c.Blue);
            //    var gbDiff = Math.Abs(c.Green - c.Blue);
            //    var totalDiff = rgDiff + rbDiff + gbDiff;

            //    //0-255 0 is black
            //    var grayScaleValue = c.ToGrayScale().Red; //0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
            //    var relativeDist = 100;// primaryIconColorList.Count == 0 ? 100 : primaryIconColorList[primaryIconColorList.Count - 1].ToSkColor().ColorDistance(c);// ColorDistance(Color.FromHex(), c);
            //    if (totalDiff > 50 &&
            //        grayScaleValue > 50 &&
            //        relativeDist > 15) {
            //        primaryIconColorList.Add(c.ToString());
            //    }
            //}

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < listCount; i++) {
                primaryIconColorList.Add(GetRandomColor().ToHex());
            }

            foreach(var c in primaryIconColorList) {
                Console.WriteLine(c);
            }
            //sw.Stop();
            //Console.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }

        public static double ColorDistance(SKColor e1, SKColor e2) {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = (long)((e1.Red + e2.Red) / 2);
            long r = (long)(e1.Red - e2.Red);
            long g = (long)(e1.Green - e2.Green);
            long b = (long)(e1.Blue - e2.Blue);
            double max = 764.83331517396653;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }

        public static double ColorDistance(Color e1, Color e2) {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = ((long)(e1.R*255) + (long)(e2.R*255)) / 2;
            long r = (long)(e1.R * 255) - (long)(e2.R * 255);
            long g = (long)(e1.G * 255) - (long)(e2.G * 255);
            long b = (long)(e1.B * 255) - (long)(e2.B * 255);
            double max = 764.83331517396653;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }

        public static bool IsBright(Color c, int brightThreshold = 150) {
            double s = c.R < 1 || c.G < 1 || c.B < 1 ? 255 : 1;
            int grayVal = (int)Math.Sqrt(
                (c.R * s) * (c.R * s) * .299 +
                (c.G * s) * (c.G * s) * .587 +
                (c.B * s) * (c.B * s) * .114);
            return grayVal > brightThreshold;
        }

        public static SolidColorBrush ChangeBrushAlpha(SolidColorBrush solidColorBrush, byte alpha) {
            var c = solidColorBrush.Color;
            solidColorBrush.Color = Color.FromRgba(c.R, c.G, c.B, (double)alpha);
            return solidColorBrush;
        }

        public static SolidColorBrush ChangeBrushBrightness(SolidColorBrush b, double correctionFactor) {
            if (correctionFactor == 0.0f) {
                return b;
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

            return new SolidColorBrush(Color.FromRgba((byte)red, (byte)green, (byte)blue, b.Color.A));
        }

        public static Brush GetDarkerBrush(Brush b) {
            return ChangeBrushBrightness((SolidColorBrush)b, -0.5);
        }

        public static Brush GetLighterBrush(Brush b) {
            return ChangeBrushBrightness((SolidColorBrush)b, 0.5);
        }

        public static Color GetRandomColor(byte alpha = 255) {
            //if (alpha == 255) {
            //    return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            //}
            //return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            int x = Rand.Next(0, _ContentColors.Count);
            int y = Rand.Next(0, _ContentColors[0].Count-1);
            return ((SolidColorBrush)GetContentColor(x, y)).Color;
        }

        public static Brush GetRandomBrushColor(byte alpha = 255) {
            return (Brush)new SolidColorBrush() { Color = GetRandomColor(alpha) };
        }
        #endregion

        #region Converters
        #endregion
    }
}
