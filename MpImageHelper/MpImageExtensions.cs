using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpImageHelper {
    public static class MpImageExtensions {
        //faster version but needs unsafe thing
        //public static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset) {
        //    fixed (PixelColor* buffer = &pixels[0, 0])
        //        source.CopyPixels(
        //          new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
        //          (IntPtr)(buffer + offset),
        //          pixels.GetLength(0) * pixels.GetLength(1) * sizeof(PixelColor),
        //          stride);
        //}
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
                    MonkeyPaste.MpConsole.WriteLine("MpHelpers.ConvertBitmapSourceToByteArray exception: " + ex);
                    return null;
                }

            }
        }

        public static Brush ToSolidColorBrush(this string hex, double opacity = 1.0) {
            if (string.IsNullOrEmpty(hex)) {
                return Brushes.Red;
            }
            var br = (Brush)new SolidColorBrush(hex.ToWinMediaColor());
            br.Opacity = opacity;
            return br;
        }

        public static Color ToWinMediaColor(this string hex) {
            if (string.IsNullOrEmpty(hex)) {
                return Colors.Red;
            }
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        public static string ToHex(this Color c, byte forceAlpha = 255) {
            if (c == null) {
                return "#FF0000";
            }
            c.A = forceAlpha;
            return c.ToString();
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
            if (string.IsNullOrEmpty(base64Str) || !base64Str.IsBase64String()) {
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

        public static bool IsBase64String(this string str) {
            if (str.IsStringResourcePath()) {
                return false;
            }
            try {
                // If no exception is caught, then it is possibly a base64 encoded string
                byte[] data = Convert.FromBase64String(str);
                // The part that checks if the string was properly padded to the
                // correct length was borrowed from d@anish's solution
                return (str.Replace(" ", "").Length % 4 == 0);
            }
            catch {
                // If exception is caught, then it is not a base64 encoded string
                return false;
            }
        }

        public static bool IsStringResourcePath(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith("pack:");
        }
    }
}
