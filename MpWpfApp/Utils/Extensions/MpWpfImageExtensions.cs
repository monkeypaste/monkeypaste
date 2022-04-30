using MonkeyPaste;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public static class MpWpfImageExtensions {
        //faster version but needs unsafe thing
        //public static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset) {
        //    fixed (PixelColor* buffer = &pixels[0, 0])
        //        source.CopyPixels(
        //          new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
        //          (IntPtr)(buffer + offset),
        //          pixels.GetLength(0) * pixels.GetLength(1) * sizeof(PixelColor),
        //          stride);
        //}
        public static MpPoint ToMpPoint(this Point p) {
            return new MpPoint(p.X, p.Y);
        }
        public static Point ToPoint(this MpPoint p) {
            return new Point(p.X, p.Y);
        }

        public static BitmapSource ReadImageFromFile(string filePath) {
            return new BitmapImage(new Uri(filePath));
        }
        public static BitmapSource ToBitmapSource(this FlowDocument fd, Brush bgBrush = null) {
            return MpHelpers.ConvertFlowDocumentToBitmap(
                                fd.Clone(),
                                fd.GetDocumentSize(),
                                bgBrush);
        }

        public static BitmapSource Scale(this BitmapSource bmpSrc, Size newScale) {
            try {
                var sbmpSrc = new TransformedBitmap(bmpSrc, new ScaleTransform(newScale.Width, newScale.Height));
                return sbmpSrc;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Error scaling bmp", ex);
                return bmpSrc;
            }
        }

        public static BitmapSource Resize(this BitmapSource bmpSrc, Size size) {
            Size scale = new Size(size.Width / (double)bmpSrc.PixelWidth, size.Height / (double)bmpSrc.PixelHeight);
            return bmpSrc.Scale(scale);
        }
        public static bool IsEqual(this BitmapSource image1, BitmapSource image2) {
            if (image1 == null || image2 == null) {
                return false;
            }
            return image1.ToByteArray().SequenceEqual(image2.ToByteArray());
        }

        public static byte[] ToByteArray(this BitmapSource bs) {
            if (bs == null) {
                return null;
            }
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            using (MemoryStream stream = new MemoryStream()) {
                try {
                    var bf = System.Windows.Media.Imaging.BitmapFrame.Create(bs);
                    encoder.Frames.Add(bf);
                    encoder.Save(stream);
                    byte[] bit = stream.ToArray();
                    stream.Close();
                    return bit;
                }
                catch (Exception ex) {
                    MpConsole.WriteLine("MpHelpers.ConvertBitmapSourceToByteArray exception: " + ex);
                    return null;
                }

            }
        }

        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap bitmap) {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width,
                bitmapData.Height,
                bitmap.HorizontalResolution,
                bitmap.VerticalResolution,
                PixelFormats.Bgra32,
                null,
                bitmapData.Scan0,
                bitmapData.Stride * bitmapData.Height,
                bitmapData.Stride);
            bitmap.UnlockBits(bitmapData);
            //bitmap.Dispose();
            return bitmapSource;
        }

        public static BitmapSource ToBitmapSource(this DrawingImage source) {
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
                drawingContext.DrawImage(source, new Rect(new Point(0, 0), new Size(source.Width, source.Height)));
                drawingContext.Close();
            }
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)source.Width, (int)source.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            return bmp;
        }

        public static System.Drawing.Bitmap ToBitmap(this BitmapSource bitmapsource) {
            using (MemoryStream outStream = new MemoryStream()) {
                System.Windows.Media.Imaging.BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                var bmp = new System.Drawing.Bitmap(outStream);

                bmp.MakeTransparent();

                double dpiX = MpScreenInformation.DpiX;
                double dpiY = MpScreenInformation.DpiY;
                bmp.SetResolution((float)dpiX, (float)dpiY);

                return bmp;
                
            }
        }
        
        public static string ToBase64String(this BitmapSource bmpSrc) {
            return Convert.ToBase64String(bmpSrc.ToByteArray());
        }
        public static BitmapSource ToBitmapSource(this byte[] bytes) {
            var bmpSrc = (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);
            bmpSrc.Freeze();
            return bmpSrc;
        }

        public static BitmapSource ToBitmapSource(this string base64Str) {
            if (string.IsNullOrEmpty(base64Str) || !base64Str.IsStringBase64()) {
                return new BitmapImage();
            }
            var bytes = System.Convert.FromBase64String(base64Str);
            return bytes.ToBitmapSource();
        }
        public static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset, bool dummy) {
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    pixels[x + x0, y + y0] = new PixelColor {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
                }
            }
        }

        public static System.Drawing.Icon ToIcon(this ImageSource imageSource) {
            if (imageSource == null) {
                return null;
            }

            Uri uri = new Uri(imageSource.ToString());
            StreamResourceInfo streamInfo = Application.GetResourceStream(uri);

            if (streamInfo == null) {
                throw new ArgumentException(
                    string.Format(
                        @"The supplied image source '{0}' could not be resolved.",
                        imageSource));
            }

            return new System.Drawing.Icon(streamInfo.Stream);
        }

        public static System.Drawing.Icon ToIcon(this System.Drawing.Bitmap bmp) {
            IntPtr hIcon = bmp.GetHicon();
            return System.Drawing.Icon.FromHandle(hIcon);
        }

        public static BitmapSource ToGrayScale(this BitmapSource bmpSrc) {
            var grayScaleSsBmp = new FormatConvertedBitmap();

            // BitmapSource objects like FormatConvertedBitmap can only have their properties
            // changed within a BeginInit/EndInit block.
            grayScaleSsBmp.BeginInit();

            // Use the BitmapSource object defined above as the source for this new
            // BitmapSource (chain the BitmapSource objects together).
            grayScaleSsBmp.Source = bmpSrc;

            // Set the new format to Gray32Float (grayscale).
            grayScaleSsBmp.DestinationFormat = PixelFormats.Gray32Float;
            grayScaleSsBmp.EndInit();
            return grayScaleSsBmp;
        }

        
    }
}
