using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Wpf {
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }

    public static class MpWpfImagingHelper {
        
        public static BitmapSource MergeImages2(IList<BitmapSource> bmpSrcList, bool scaleToSmallestSize = false, bool scaleToLargestDpi = true) {
            // if not scaled to smallest, will be scaled to largest
            int w = scaleToSmallestSize ? bmpSrcList.Min(x => x.PixelWidth) : bmpSrcList.Max(x => x.PixelWidth);
            int h = scaleToSmallestSize ? bmpSrcList.Min(x => x.PixelHeight) : bmpSrcList.Max(x => x.PixelHeight);

            double dpiX = scaleToLargestDpi ? bmpSrcList.Max(x => x.DpiX) : bmpSrcList.Min(x => x.DpiX);
            double dpiY = scaleToLargestDpi ? bmpSrcList.Max(x => x.DpiY) : bmpSrcList.Max(x => x.DpiY);

            for (int i = 0; i < bmpSrcList.Count; i++) {
                BitmapSource bmp = bmpSrcList[i];
                if (bmp.PixelWidth != w || bmp.PixelHeight != h) {
                    bmpSrcList[i] = bmp.Scale(new Size(w / bmp.PixelWidth, h / bmp.PixelHeight));
                }
            }

            var renderTargetBitmap = new RenderTargetBitmap(w, h, dpiX, dpiY, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen()) {
                foreach (var image in bmpSrcList) {
                    drawingContext.DrawImage(image, new Rect(0, 0, w, h));
                }
            }
            renderTargetBitmap.Render(drawingVisual);
            
            return ConvertRenderTargetBitmapToBitmapSource(renderTargetBitmap);
        }

        public static BitmapSource MergeImages(IList<BitmapSource> bmpSrcList, Size size = default, double xstep = 0, double ystep = 0) {
            // from https://stackoverflow.com/a/14661969/105028
            size = size == default ? new Size(32, 32) : size;

            // Gets the size of the images (I assume each image has the same size)

            // Draws the images into a DrawingVisual component
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
                double x = 0, y = 0;
                foreach (BitmapSource bmpSrc in bmpSrcList) {
                    Size scale = new Size(size.Width / (double)bmpSrc.PixelWidth, size.Height / (double)bmpSrc.PixelHeight);
                    var rbmpSrc = bmpSrc.Scale(scale);
                    drawingContext.DrawImage(rbmpSrc, new Rect(x, y, (int)size.Width, (int)size.Width));
                    x += xstep;
                    y += ystep;
                }
            }

            // Converts the Visual (DrawingVisual) into a BitmapSource
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            return ConvertRenderTargetBitmapToBitmapSource(bmp);
        }

        public static BitmapSource ConvertRenderTargetBitmapToBitmapSource(RenderTargetBitmap rtb) {
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var stream = new MemoryStream()) {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static BitmapSource CombineBitmap(IList<BitmapSource> bmpSrcList, bool tileHorizontally = true) {
            if (bmpSrcList.Count == 0) {
                return new BitmapImage();
            }
            if (bmpSrcList.Count == 1) {
                return bmpSrcList[0];
            }
            //read all images into memory
            List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;

            try {
                int width = 0;
                int height = 0;

                foreach (var bmpSrc in bmpSrcList) {
                    //create a Bitmap from the file and add it to the list
                    System.Drawing.Bitmap bitmap = bmpSrc.ToBitmap();

                    //update the size of the final bitmap
                    if (tileHorizontally) {
                        width += bitmap.Width;
                        height = Math.Max(bitmap.Height, height);
                    } else {
                        width = Math.Max(bitmap.Width, width);
                        height += bitmap.Height;
                    }
                    images.Add(bitmap);
                }

                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage)) {
                    //set background color
                    g.Clear(System.Drawing.Color.Transparent);

                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap image in images) {
                        g.DrawImage(image, new System.Drawing.Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                    g.Dispose();
                }
                return finalImage.ToBitmapSource();
            }
            catch (Exception ex) {
                if (finalImage != null) {
                    finalImage.Dispose();
                }
                throw ex;
            }
            finally {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images) {
                    image.Dispose();
                }
            }
        }

        

        public static BitmapSource CopyScreen() {
            double left = 0;//System.Windows.Forms.Screen.AllScreens.Min(screen => screen.Bounds.X);
            double top = 0;// System.Windows.Forms.Screen.AllScreens.Min(screen => screen.Bounds.Y);
            double right = SystemParameters.PrimaryScreenWidth;// MpMeasurements.Instance.ScreenWidth * MpPreferences.ThisAppDip;//System.Windows.Forms.Screen.AllScreens.Max(screen => screen.Bounds.X + screen.Bounds.Width);
            double bottom = SystemParameters.PrimaryScreenHeight; // MpMeasurements.Instance.ScreenHeight * MpPreferences.ThisAppDip;//System.Windows.Forms.Screen.AllScreens.Max(screen => screen.Bounds.Y + screen.Bounds.Height);
            int width = (int)(right - left);
            int height = (int)(bottom - top);

            using (var screenBmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
                using (var bmpGraphics = System.Drawing.Graphics.FromImage(screenBmp)) {
                    bmpGraphics.CopyFromScreen((int)left, (int)top, 0, 0, new System.Drawing.Size(width, height));
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        public static PixelColor[,] GetPixels(BitmapSource source) {
            if (source.Format != PixelFormats.Bgra32) {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixels(result, width * 4, 0, false);
            return result;
        }

        public static void PutPixels(WriteableBitmap bitmap, PixelColor[,] pixels, int x, int y) {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, x, y);
        }

        public static List<KeyValuePair<PixelColor, int>> GetStatistics(BitmapSource bmpSource) {
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

        public static List<string> CreatePrimaryColorList(BitmapSource bmpSource, int palleteSize = 5) {
            //var sw = new Stopwatch();
            //sw.Start();
            var primaryIconColorList = new List<string>();
            var hist = GetStatistics(bmpSource);
            foreach (var kvp in hist) {
                var c = Color.FromArgb(255, kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue);

                //MpConsole.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
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
                var relativeDist = primaryIconColorList.Count == 0 ? 1 : primaryIconColorList[primaryIconColorList.Count - 1].ToWinMediaColor().ColorDistance(c);
                if (totalDiff > 50 && grayScaleValue < 200 && relativeDist > 0.15) {
                    primaryIconColorList.Add(MpWpfColorHelpers.ConvertColorToHex(c));
                }
            }

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < palleteSize; i++) {
                primaryIconColorList.Add(MpWpfColorHelpers.ConvertColorToHex(MpWpfColorHelpers.GetRandomColor()));
            }
            //sw.Stop();
            //MpConsole.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }

        

    }
}
