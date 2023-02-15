using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using WinformsBitmap = System.Drawing.Bitmap;
using WinformsBitmapData = System.Drawing.Imaging.BitmapData;
using WinformsImageLockMode = System.Drawing.Imaging.ImageLockMode;
using WinformsPixelFormat = System.Drawing.Imaging.PixelFormat;
using WinformsPoint = System.Drawing.Point;
using WinformsRectangle = System.Drawing.Rectangle;

namespace WpfRtfClipboardHandler {
    public static class MpWpfImageExtensions {

        //public static Point ToWpfPoint(this MpPoint p) {
        //    return new Point() { X = p.X, Y = p.Y };
        //}

        //public static Size ToWpfSize(this MpSize s) {
        //    return new Size() { Width = s.Width, Height = s.Height };
        //}

        //public static Rect ToWpfRect(this MpRect rect) {
        //    return new Rect(rect.Location.ToWpfPoint(), rect.Size.ToWpfSize());
        //}

        //public static MpRect ToPortableRect(this System.Drawing.Rectangle rect) {
        //    return new MpRect(new MpPoint(rect.X, rect.Y), new MpSize(rect.Width, rect.Height));
        //}

        //public static MpRect ToPortableRect(this Rect rect) {
        //    return new MpRect(rect.Location.ToPortablePoint(), rect.Size.ToPortableSize());
        //}

        //public static MpSize ToPortableSize(this Size size) {
        //    return new MpSize(size.Width, size.Height);
        //}

        public static Size PixelSize(this BitmapSource bmpSrc) {
            if (bmpSrc == null) {
                return new Size();
            }
            return new Size(bmpSrc.PixelWidth, bmpSrc.PixelHeight);
        }

        public static Size PixelSize(this ImageSource imgSrc) {
            if (imgSrc == null) {
                return new Size();
            }
            if (imgSrc is BitmapSource bmpSrc) {
                return bmpSrc.PixelSize();
            }
            Debugger.Break();
            return new Size();
        }
        //faster version but needs unsafe thing
        //public static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset) {
        //    fixed (PixelColor* buffer = &pixels[0, 0])
        //        source.CopyPixels(
        //          new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
        //          (IntPtr)(buffer + offset),
        //          pixels.GetLength(0) * pixels.GetLength(1) * sizeof(PixelColor),
        //          stride);
        //}
        //public static MpPoint ToPortablePoint(this Point p) {
        //    return new MpPoint(p.X, p.Y);
        //}
        //public static Point ToPoint(this MpPoint p) {
        //    return new Point(p.X, p.Y);
        //}

        public static BitmapSource ReadImageFromFile(string filePath) {
            return new BitmapImage(new Uri(filePath));
        }

        public static BitmapSource Scale(this BitmapSource bmpSrc, Size newScale) {
            try {
                var sbmpSrc = new TransformedBitmap(bmpSrc, new ScaleTransform(newScale.Width, newScale.Height));
                return sbmpSrc;
            }
            catch (Exception ex) {
                //Console.WriteLine("Error scaling bmp", ex);
                Console.WriteLine("Error scaling bmp");
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
        public static BitmapSource Tint(this BitmapSource bmpSrc, Brush brush, bool retainAlpha = true) {
            return bmpSrc.Tint(((SolidColorBrush)brush).Color, retainAlpha);
        }
        public static BitmapSource Tint(this BitmapSource bmpSrc, Color tint, bool retainAlpha = true) {
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
            var pixels = MpWpfImagingHelper.GetPixels(bmp);
            var pixelColor = new PixelColor[1, 1];
            pixelColor[0, 0] = new PixelColor { Alpha = tint.A, Red = tint.R, Green = tint.G, Blue = tint.B };

            for (int x = 0; x < bmp.Width; x++) {
                for (int y = 0; y < bmp.Height; y++) {
                    PixelColor c = pixels[x, y];
                    if (c.Alpha > 0) {
                        if (retainAlpha) {
                            pixelColor[0, 0].Alpha = c.Alpha;
                        }
                        MpWpfImagingHelper.PutPixels(bmp, pixelColor, x, y);
                    }
                }
            }
            return bmp;
        }

        public static byte[] ToByteArray(this BitmapSource bs) {
            if (bs == null) {
                return null;
            }
            if (!bs.IsFrozen && bs.CanFreeze) {
                bs.Freeze();
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
                    Console.WriteLine("MpHelpers.ConvertBitmapSourceToByteArray exception: " + ex);
                    return null;
                }

            }
        }

        public static BitmapSource ToBitmapSource(this WinformsBitmap bitmap) {
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

        public static WinformsBitmap ToBitmap(this BitmapSource bitmapsource, System.Drawing.Color? transColor = null) {
            transColor = !transColor.HasValue ? System.Drawing.Color.Black : transColor.Value;
            using (MemoryStream outStream = new MemoryStream()) {
                System.Windows.Media.Imaging.BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                var bmp = new WinformsBitmap(outStream);

                bmp.MakeTransparent(transColor.Value);

                double dpiX = MpScreenInformation.DpiX;
                double dpiY = MpScreenInformation.DpiY;
                dpiX = dpiX == 0 ? 96 : dpiX;
                dpiY = dpiY == 0 ? 96 : dpiY;
                bmp.SetResolution((float)dpiX, (float)dpiY);

                return bmp;
            }
        }



        public static WinformsBitmap BitmapSourceToBitmap(BitmapSource source) {
            // from https://weblog.west-wind.com/posts/2020/Sep/16/Retrieving-Images-from-the-Clipboard-and-WPF-Image-Control-Woes#clipboardgetimage-isnt-reliable
            if (source == null)
                return null;

            var pixelFormat = WinformsPixelFormat.Format32bppArgb;  //Bgr32 equiv default
            if (source.Format == PixelFormats.Bgr24)
                pixelFormat = WinformsPixelFormat.Format24bppRgb;
            else if (source.Format == PixelFormats.Pbgra32)
                pixelFormat = WinformsPixelFormat.Format32bppPArgb;
            else if (source.Format == PixelFormats.Prgba64)
                pixelFormat = WinformsPixelFormat.Format64bppPArgb;

            WinformsBitmap bmp = new WinformsBitmap(
                source.PixelWidth,
                source.PixelHeight,
                pixelFormat);

            WinformsBitmapData data = bmp.LockBits(
                new WinformsRectangle(WinformsPoint.Empty, bmp.Size),
                WinformsImageLockMode.WriteOnly,
                pixelFormat);

            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);

            bmp.UnlockBits(data);

            return bmp;
        }
        public static BitmapSource BitmapToBitmapSource(WinformsBitmap bmp) {
            // from https://weblog.west-wind.com/posts/2020/Sep/16/Retrieving-Images-from-the-Clipboard-and-WPF-Image-Control-Woes#clipboardgetimage-isnt-reliable

            var hBitmap = bmp.GetHbitmap();
            var imageSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            WinApi.DeleteObject(hBitmap);
            return imageSource;
        }

        public static string ToAsciiImage(this BitmapSource bmpSrc, Size? docSize = null) {
            //Size size = docSize.HasValue ? docSize.Value : new Size(50, 50);
            Size size = new Size(100, 100);
            if (docSize.HasValue) {
                size = docSize.Value;
            } else {
                Size pixelSize = bmpSrc.PixelSize();
                if (pixelSize.Width >= pixelSize.Height) {
                    double ar = pixelSize.Height / pixelSize.Width;
                    size.Height *= ar;
                } else {
                    double ar = pixelSize.Width / pixelSize.Height;
                    size.Width *= ar;
                }
            }
            string[] asciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
            bmpSrc = bmpSrc.Resize(size);
            using (WinformsBitmap image = bmpSrc.ToBitmap()) {
                string outStr = string.Empty;
                for (int h = 0; h < image.Height; h++) {
                    for (int w = 0; w < image.Width; w++) {
                        System.Drawing.Color pixelColor = image.GetPixel(w, h);
                        //Average out the RGB components to find the Gray Color
                        int avg = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                        //int red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                        //int green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                        //int blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                        System.Drawing.Color grayColor = System.Drawing.Color.FromArgb(avg, avg, avg);
                        int index = (grayColor.R * 10) / 255;
                        outStr += asciiChars[index];
                    }
                    outStr += Environment.NewLine;
                }
                return outStr;
            }
        }

        public static string ToRtfImage(this BitmapSource bmpSrc, Size? docSize = null) {
            Size size = docSize.HasValue ? docSize.Value : new Size(50, 50);
            string[] asciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };
            bmpSrc = bmpSrc.Resize(size);
            var fd = new FlowDocument();
            fd.Blocks.Clear();
            var p = new Paragraph();
            fd.Blocks.Add(p);
            var ctp = fd.ContentStart;
            using (WinformsBitmap image = bmpSrc.ToBitmap()) {
                string outStr = string.Empty;
                for (int h = 0; h < image.Height; h++) {
                    for (int w = 0; w < image.Width; w++) {
                        System.Drawing.Color pixelColor = image.GetPixel(w, h);
                        var r = new Run("0", ctp) {
                            Foreground = pixelColor.ToSolidColorBrush()
                        };
                        ctp = r.ContentEnd.GetInsertionPosition(LogicalDirection.Forward);
                    }
                    ctp = ctp.InsertLineBreak();
                }
                return fd.ToRichText();
            }
        }

        public static string ToBase64String(this ImageSource imgSrc) {
            return ((BitmapSource)imgSrc).ToBase64String();
        }
        public static string ToBase64String(this BitmapSource bmpSrc) {
            return Convert.ToBase64String(bmpSrc.ToByteArray());
        }
        public static BitmapSource ToBitmapSource(this byte[] bytes, bool freeze = true) {


            //using (var stream = new MemoryStream(bytes)) {
            //    var frame = new BitmapImage();
            //    frame.BeginInit();
            //    frame.CacheOption = BitmapCacheOption.OnLoad;
            //    frame.StreamSource = stream;
            //    frame.EndInit();
            //    frame.Freeze();
            //    //image newimage = new image() { source = frame };
            //    return frame;
            //}


            var bmpSrc = (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);
            if (freeze) {
                bmpSrc.Freeze();
            }
            return bmpSrc;
        }


        public static BitmapSource ToBitmapSource(this string base64Str, bool freeze = true) {
            if (string.IsNullOrEmpty(base64Str)) {
                return new BitmapImage();
            }
            try {
                var bytes = System.Convert.FromBase64String(base64Str);
                return bytes.ToBitmapSource(freeze);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error converting base64 to bitmapsource. {Environment.NewLine} {ex}");
                return new BitmapImage();
            }
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

        public static System.Drawing.Icon ToIcon(this WinformsBitmap bmp) {
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
