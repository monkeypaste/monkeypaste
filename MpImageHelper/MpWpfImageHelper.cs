using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpImageHelper {
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }
    public class MpWpfImageHelper : MonkeyPaste.MpIconBuilder, MpISingleton<MpWpfImageHelper> {
        private List<List<Brush>> _colors = new List<List<Brush>> {
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

        #region MpISingleton Definition
        //private static readonly Lazy<MpImageHelper> _Lazy = new Lazy<MpImageHelper>(() => new MpImageHelper());
        //public static MpImageHelper Instance { get { return _Lazy.Value; } }
        private static MpWpfImageHelper _instance;
        public static MpWpfImageHelper Instance => _instance ?? (_instance = new MpWpfImageHelper()); 
        
        public async Task Init() {
            await Task.Delay(1);
        }

        #region Constructor

        public MpWpfImageHelper() { }

        #endregion

        #endregion



        public BitmapSource TintBitmapSource(BitmapSource bmpSrc, Color tint, bool retainAlpha = false) {
            BitmapSource formattedBmpSrc = null;
            if (bmpSrc.Width != bmpSrc.PixelWidth || bmpSrc.Height != bmpSrc.PixelHeight) {
                //means bmp dpi isn't 96
                double dpi = 96;
                int width = bmpSrc.PixelWidth;
                int height = bmpSrc.PixelHeight;

                int stride = width * 4; // 4 bytes per pixel
                byte[] pixelData = new byte[stride * height];
                bmpSrc.CopyPixels(pixelData, stride, 0);

                formattedBmpSrc = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, stride);
            } else {
                formattedBmpSrc = bmpSrc;
            }
            var bmp = new WriteableBitmap(formattedBmpSrc);
            var pixels = GetPixels(bmp);
            var pixelColor = new PixelColor[1, 1];
            pixelColor[0, 0] = new PixelColor { Alpha = tint.A, Red = tint.R, Green = tint.G, Blue = tint.B };

            for (int x = 0; x < bmp.Width; x++) {
                for (int y = 0; y < bmp.Height; y++) {
                    PixelColor c = pixels[x, y];
                    if (c.Alpha > 0) {
                        if (retainAlpha) {
                            pixelColor[0, 0].Alpha = c.Alpha;
                        }
                        PutPixels(bmp, pixelColor, x, y);
                    }
                }
            }
            return bmp;
        }

        public BitmapSource ScaleBitmapSource(BitmapSource bmpSrc, Size newScale) {
            try {
                var sbmpSrc = new TransformedBitmap(bmpSrc, new ScaleTransform(newScale.Width, newScale.Height));
                return sbmpSrc;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Error scaling bmp", ex);
                return bmpSrc;
            }
        }

        public PixelColor[,] GetPixels(BitmapSource source) {
            if (source.Format != PixelFormats.Bgra32) {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixels(result, width * 4, 0, false);
            return result;
        }

        private void PutPixels(WriteableBitmap bitmap, PixelColor[,] pixels, int x, int y) {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, x, y);
        }

        public List<KeyValuePair<PixelColor, int>> GetStatistics(BitmapSource bmpSource) {
            var countDictionary = new Dictionary<PixelColor, int>();
            var pixels = GetPixels(bmpSource);

            for (int x = 0; x < bmpSource.PixelWidth; x++) {
                for (int y = 0; y < bmpSource.PixelHeight; y++) {
                    PixelColor currentColor = pixels[x, y];
                    if (currentColor.Alpha == 0) {
                        continue;
                    }
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

        public  List<string> CreatePrimaryColorList(BitmapSource bmpSource, int palleteSize = 5) {
            //var sw = new Stopwatch();
            //sw.Start();
            var primaryIconColorList = new List<string>();
            var hist = GetStatistics(bmpSource);
            foreach (var kvp in hist) {
                var c = Color.FromArgb(255, kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue);

                //MonkeyPaste.MpConsole.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
                if (primaryIconColorList.Count == palleteSize) {
                    break;
                }
                //between 0-255 where 0 is black 255 is white
                var rgDiff = Math.Abs((int)c.R - (int)c.G);
                var rbDiff = Math.Abs((int)c.R - (int)c.B);
                var gbDiff = Math.Abs((int)c.G - (int)c.B);
                var totalDiff = rgDiff + rbDiff + gbDiff;

                //0-255 0 is black
                var grayScaleValue = 0.2126 * (int)c.R + 0.7152 * (int)c.G + 0.0722 * (int)c.B;
                var relativeDist = primaryIconColorList.Count == 0 ? 1 : ColorDistance(ConvertHexToColor(primaryIconColorList[primaryIconColorList.Count - 1]), c);
                if (totalDiff > 50 && grayScaleValue < 200 && relativeDist > 0.15) {
                    primaryIconColorList.Add(ConvertColorToHex(c));
                }
            }

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < palleteSize; i++) {
                primaryIconColorList.Add(ConvertColorToHex(GetRandomColor()));
            }
            //sw.Stop();
            //MonkeyPaste.MpConsole.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }

        public double ColorDistance(Color e1, Color e2) {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = ((long)e1.R + (long)e2.R) / 2;
            long r = (long)e1.R - (long)e2.R;
            long g = (long)e1.G - (long)e2.G;
            long b = (long)e1.B - (long)e2.B;
            double max = 764.83331517396653;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }
        public Color GetRandomColor() {
            var rand = new Random((int)DateTime.Now.Ticks);
            int x = rand.Next(0, _colors.Count);
            int y = rand.Next(0, _colors[0].Count);

            return ((SolidColorBrush)_colors[x][y]).Color;
        }
        public Color ConvertHexToColor(string hexString) {
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

        public string ConvertColorToHex(Color c, byte forceAlpha = 255) {
            if (c == null) {
                return "#FF0000";
            }
            c.A = forceAlpha;
            return c.ToString();
        }

        public BitmapSource CreateBorder(BitmapSource img, double scale, Color bgColor) {
            var borderBmpSrc = TintBitmapSource(img, bgColor, true);
            //var borderSize = new Size(borderBmpSrc.Width * scale, bordherBmpSrc.Height * scale);
            return ScaleBitmapSource(borderBmpSrc, new Size(scale, scale));
        }

        public override string CreateBorder(string iconBase64, double scale, string hexColor) {
            return CreateBorder(iconBase64.ToBitmapSource(), scale, hexColor.ToWinMediaColor()).ToBase64String();
        }
        
            
        public override List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5) {
            return CreatePrimaryColorList(iconBase64.ToBitmapSource(), palleteSize);
        }
    }
}
