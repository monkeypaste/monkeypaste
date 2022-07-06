using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
