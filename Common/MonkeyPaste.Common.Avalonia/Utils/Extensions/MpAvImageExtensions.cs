using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Visuals.Media.Imaging;
//using MonkeyPaste.Common.Wpf;

namespace MonkeyPaste.Common.Avalonia {
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }

    public static class MpAvImageExtensions {
        #region Converters        

        public static Bitmap? ToAvBitmap(this string base64Str, double scale=1.0) {
            if(!base64Str.IsStringBase64()) {
                return null;
            }
            var bmp = Convert.FromBase64String(base64Str).ToAvBitmap();
            if(scale == 1.0) {
                return bmp;
            }
            return bmp.Scale(new MpSize(scale,scale));
        }

        public static Bitmap ToAvBitmap(this byte[] bytes) {
            using (var stream = new MemoryStream(bytes)) {
                return new Bitmap(stream);
            }
        }
        public static byte[] ToByteArray(this Bitmap bmp) {
            using(var stream = new MemoryStream()) {
                bmp.Save(stream);
                return stream.ToArray();
            }
        }

        public static string ToBase64String(this Bitmap bmp) {
            return Convert.ToBase64String(bmp.ToByteArray());
        }

        public static Bitmap ToAvBitmap(this WriteableBitmap wbmp) {
            using (var outStream = new MemoryStream()) {
                wbmp.Save(outStream);
                outStream.Seek(0, SeekOrigin.Begin);
                var outBmp = new Bitmap(outStream);
                return outBmp;
            }
        }

        public static Bitmap ToAvBitmap(this RenderTargetBitmap rtbmp) {
            return new Bitmap(rtbmp.PlatformImpl);
        }

        public static string ToRichHtmlImage(this Bitmap bmp) {
            string qhtml = string.Format(@"<p><img src='data:image/png;base64,{0}'></p>", bmp.ToBase64String());
            return qhtml;
        }

        #endregion

        #region Effects

        public static unsafe Bitmap? Tint(this Bitmap bmp, string hexColor, bool retainAlpha = true) {
            //BitmapSource formattedBmpSrc = null;
            //if (bmpSrc.Width != bmpSrc.PixelWidth || bmpSrc.Height != bmpSrc.PixelHeight) {
            //    //means bmp dpi isn't 96
            //    double dpi = 96;
            //    int width = bmpSrc.PixelWidth;
            //    int height = bmpSrc.PixelHeight;

            //    int stride = width * 4; // 4 bytes per pixel
            //    byte[] pixelData = new byte[stride * height];
            //    bmpSrc.CopyPixels(pixelData, stride, 0);

            //    formattedBmpSrc = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, stride);
            //} else {
            //    formattedBmpSrc = bmpSrc;
            //}
            var tint = hexColor.ToAvColor();
            var tintPixelColor = new PixelColor { Alpha = tint.A, Red = tint.R, Green = tint.G, Blue = tint.B };

            var pixels = GetPixels(bmp);
            using (var memoryStream = new MemoryStream()) {
                bmp.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var writeableBitmap = WriteableBitmap.Decode(memoryStream);
                using (var lockedBitmap = writeableBitmap.Lock()) {
                    byte* bmpPtr = (byte*)lockedBitmap.Address;
                    int width = writeableBitmap.PixelSize.Width;
                    int height = writeableBitmap.PixelSize.Height;
                    //byte* tempPtr;

                    for (int row = 0; row < height; row++) {
                        for (int col = 0; col < width; col++) {
                            //tempPtr = bmpPtr;
                            //byte red = *bmpPtr++;
                            //byte green = *bmpPtr++;
                            //byte blue = *bmpPtr++;
                            //byte alpha = *bmpPtr++;

                            //byte result = (byte)(0.2126 * red + 0.7152 * green + 0.0722 * blue);
                            //// byte result = (byte)((red + green + blue) / 3);

                            //bmpPtr = tempPtr;

                            PixelColor c = pixels[col, row];
                            if (c.Alpha > 0) {
                                // only write non-transparent pixels so color analysis is accurate

                                //if (retainAlpha) {
                                //    pixelColor[0, 0].Alpha = c.Alpha;
                                //}
                                //bmpPtr = PutPixels(writeableBitmap, pixelColor, bmpPtr);
                                c = tintPixelColor;
                                if (!retainAlpha) {
                                    c.Alpha = 255;
                                }
                            } else {
                                c = new PixelColor();
                            }
                            bmpPtr = PutPixel(writeableBitmap, c, bmpPtr);
                        }
                    }
                }
                return writeableBitmap.ToAvBitmap();
            }
        }

        public static Bitmap? Scale(this Bitmap bmpSrc, MpSize newScale) {
            MpSize size = new MpSize(bmpSrc.PixelSize.Width * (int)newScale.Width, bmpSrc.PixelSize.Height * (int)newScale.Height);
            return Resize(bmpSrc, size);
        }

        public static Bitmap? Resize(this Bitmap bmpSrc, MpSize size) {
            var bmpTarget = bmpSrc.CreateScaledBitmap(new PixelSize((int)size.Width, (int)size.Height));
            return bmpTarget;
        }

        public static unsafe string ToAsciiImage(this Bitmap bmpSrc, MpSize docSize = null) {
            //Size size = docSize.HasValue ? docSize.Value : new Size(50, 50);
            MpSize size = new MpSize(100, 100);
            if (docSize != null) {
                size = docSize;
            } else {
                MpSize pixelSize = bmpSrc.PixelSize.ToPortableSize();
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
            var pixels = GetPixels(bmpSrc);
            using (var memoryStream = new MemoryStream()) {
                bmpSrc.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var writeableBitmap = WriteableBitmap.Decode(memoryStream);
                using (var lockedBitmap = writeableBitmap.Lock()) {
                    byte* bmpPtr = (byte*)lockedBitmap.Address;
                    int width = writeableBitmap.PixelSize.Width;
                    int height = writeableBitmap.PixelSize.Height;

                    string outStr = string.Empty;
                    for (int row = 0; row < height; row++) {
                        for (int col = 0; col < width; col++) {
                            PixelColor c = pixels[col, row];
                            byte avg = (byte)((double)(c.Red + c.Green + c.Blue) / 3.0d);
                            PixelColor grayColor = new PixelColor() { Alpha = 255, Red = avg, Green = avg, Blue = avg }; 
                            int index = (int)((double)(grayColor.Red * 10) / 255.0d);
                            outStr += asciiChars[index];
                            bmpPtr = PutPixel(writeableBitmap, c, bmpPtr);
                        }
                    }
                    return outStr;
                }
            }
        }

        #endregion

        #region Operations

        public static List<KeyValuePair<PixelColor, int>> GetStatistics(this Bitmap bmpSource) {
            var countDictionary = new Dictionary<PixelColor, int>();
            var pixels = GetPixels(bmpSource);
            for (int x = 0; x < bmpSource.PixelSize.Width; x++) {
                for (int y = 0; y < bmpSource.PixelSize.Height; y++) {
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
        #endregion

        #region Read/Write

        public static unsafe PixelColor[,] GetPixels(this Bitmap bitmap) {
            using (var memoryStream = new MemoryStream()) {
                bitmap.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var writeableBitmap = WriteableBitmap.Decode(memoryStream);
                using var lockedBitmap = writeableBitmap.Lock();

                byte* bmpPtr = (byte*)lockedBitmap.Address;
                int width = writeableBitmap.PixelSize.Width;
                int height = writeableBitmap.PixelSize.Height;

                PixelColor[,] pixels = new PixelColor[width, height];

                for (int row = 0; row < height; row++) {
                    for (int col = 0; col < width; col++) {
                        byte blue = *bmpPtr++; 
                        byte green = *bmpPtr++; 
                        byte red = *bmpPtr++;
                        byte alpha = *bmpPtr++;

                        pixels[col, row] = new PixelColor() {
                            Alpha = alpha,
                            Red = red,
                            Green = green,
                            Blue = blue
                        };
                    }
                }

                return pixels;
            }
        }
        public static unsafe byte* PutPixel(WriteableBitmap bitmap, PixelColor pixel, byte* bmpPtr) {
            *bmpPtr++ = pixel.Blue; 
            *bmpPtr++ = pixel.Green;
            *bmpPtr++ = pixel.Red;
            *bmpPtr++ = pixel.Alpha;

            return bmpPtr;
        }

        #endregion

        public static unsafe Bitmap ToGrayScale(this Bitmap bitmap) {
            using (var memoryStream = new MemoryStream()) {
                bitmap.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var writeableBitmap = WriteableBitmap.Decode(memoryStream);
                using var lockedBitmap = writeableBitmap.Lock();

                byte* bmpPtr = (byte*)lockedBitmap.Address;
                int width = writeableBitmap.PixelSize.Width;
                int height = writeableBitmap.PixelSize.Height;
                byte* tempPtr;

                for (int row = 0; row < height; row++) {
                    for (int col = 0; col < width; col++) {
                        tempPtr = bmpPtr;
                        byte red = *bmpPtr++;
                        byte green = *bmpPtr++;
                        byte blue = *bmpPtr++;
                        byte alpha = *bmpPtr++;

                        byte result = (byte)(0.2126 * red + 0.7152 * green + 0.0722 * blue);
                        // byte result = (byte)((red + green + blue) / 3);

                        bmpPtr = tempPtr;
                        *bmpPtr++ = result; // red
                        *bmpPtr++ = result; // green
                        *bmpPtr++ = result; // blue
                        *bmpPtr++ = alpha; // alpha
                    }
                }

                return writeableBitmap;
            }
        }

        
    }
}
