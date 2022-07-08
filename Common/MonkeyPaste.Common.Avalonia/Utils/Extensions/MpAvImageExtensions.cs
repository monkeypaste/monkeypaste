using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

        public static Bitmap? ToBitmap(this string base64Str) {
            if(!base64Str.IsStringBase64()) {
                return null;
            }
            return Convert.FromBase64String(base64Str).ToBitmap();
        }

        public static Bitmap ToBitmap(this byte[] bytes) {
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

        public static string ToBase64(this Bitmap bmp) {
            return Convert.ToBase64String(bmp.ToByteArray());
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

            var pixels = GetPixels(bmp);
            var pixelColor = new PixelColor[1, 1];
            var tint = hexColor.ToAvColor();
            pixelColor[0, 0] = new PixelColor { Alpha = tint.A, Red = tint.R, Green = tint.G, Blue = tint.B };
            using (var memoryStream = new MemoryStream()) {
                bmp.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var writeableBitmap = WriteableBitmap.Decode(memoryStream);
                using var lockedBitmap = writeableBitmap.Lock();

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

                            if(!retainAlpha) {
                                c.Alpha = 255;
                            }
                        } else {
                            c = new PixelColor();
                        }
                        bmpPtr = PutPixel(writeableBitmap, c, bmpPtr);       
                    }
                }

                return writeableBitmap;
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
                        byte red = *bmpPtr++;
                        byte green = *bmpPtr++;
                        byte blue = *bmpPtr++;
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

        public static unsafe byte* PutPixels(WriteableBitmap bitmap, PixelColor[,] pixels, byte* bmpPtr) {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);

            //bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, x, y);
            return bmpPtr;
        }
        public static unsafe byte* PutPixel(WriteableBitmap bitmap, PixelColor pixel, byte* bmpPtr) {
            *bmpPtr++ = pixel.Red; // red
            *bmpPtr++ = pixel.Green; // green
            *bmpPtr++ = pixel.Blue; // blue
            *bmpPtr++ = pixel.Alpha; // alpha

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
