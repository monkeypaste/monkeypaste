using Avalonia;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common.Avalonia {

    public static class MpAvImageExtensions {
        static object _getPixelsLock = new object();
        static object _tintLock = new object();
        #region Converters        

        public static Bitmap? ToAvBitmap(this string base64Str, double scale = 1.0, string tint_hex_color = "") {
            if (!base64Str.IsStringBase64()) {
                return null;
            }
            try {
                var bytes = Convert.FromBase64String(base64Str);
                return bytes.ToAvBitmap(scale, tint_hex_color);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error converting '{base64Str}' to bitmap.", ex);
            }
            return null;
        }

        public static Bitmap ToAvBitmap(this byte[] bytes, double scale = 1.0, string tint_hex_color = "") {
            using (var stream = new MemoryStream(bytes)) {
                try {
                    var bmp = new Bitmap(stream);
                    if (bmp == null) {
                        return null;
                    }
                    if (!string.IsNullOrEmpty(tint_hex_color)) {
                        bmp = bmp.Tint(tint_hex_color);
                    }
                    if (scale == 1.0) {
                        return bmp;
                    }
                    return bmp.Scale(new MpSize(scale, scale));
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error creating bitmap from bytes ", ex);
                    return null;
                }
            }
        }
        public static byte[] ToByteArray(this Bitmap bmp, int quality = 100) {
            using (var stream = new MemoryStream()) {
                bmp.Save(stream, quality);
                return stream.ToArray();
            }
        }

        public static string ToBase64String(this Bitmap bmp, int quality = 100) {
            if (bmp == null) {
                return string.Empty;
            }
            return Convert.ToBase64String(bmp.ToByteArray(quality));
        }

        public static Bitmap ToAvBitmap(this WriteableBitmap wbmp, int quality = 100) {
            using (var outStream = new MemoryStream()) {
                wbmp.Save(outStream, quality);
                outStream.Seek(0, SeekOrigin.Begin);
                var outBmp = new Bitmap(outStream);
                return outBmp;
            }
        }


        public static string ToRichHtmlImage(this Bitmap bmp) {
            string qhtml = $"<p><img src='{bmp.ToBase64String().ToBase64ImageUrl()}'></p>";
            return qhtml;
        }

        #endregion

        #region Effects

        public static unsafe Bitmap? Tint(this Bitmap bmp, string hexColor, bool retainAlpha = true, int quality = 100) {
            if (bmp == null) {
                return null;
            }
            var tint = hexColor.ToAvColor();
            var tintPixelColor = new PixelColor { Alpha = tint.A, Red = tint.R, Green = tint.G, Blue = tint.B };
            if (tintPixelColor.Alpha == 0) {
                // don't change image if tint is transparent
                return bmp;
            }
            MpConsole.WriteLine($"[Tint] Unsafe BEGIN ", true);

            var pixels = GetPixels(bmp);
            try {
                lock (_tintLock) {
                    using (var memoryStream = new MemoryStream()) {
                        bmp.Save(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        var writeableBitmap = WriteableBitmap.Decode(memoryStream);
                        using (var lockedBitmap = writeableBitmap.Lock()) {
                            byte* bmpPtr = (byte*)lockedBitmap.Address;
                            int width = writeableBitmap.PixelSize.Width;
                            int height = writeableBitmap.PixelSize.Height;

                            for (int row = 0; row < height; row++) {
                                for (int col = 0; col < width; col++) {
                                    PixelColor c = pixels[col, row];
                                    if (c.Alpha > 0) {
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
                        MpConsole.WriteLine($"[Tint] Unsafe END ", false, true);
                        return writeableBitmap.ToAvBitmap(quality);
                    }
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Tint error. ", ex);
                return bmp;
            }
        }

        public static Bitmap? Scale(this Bitmap bmpSrc, MpSize newScale) {
            if (bmpSrc == null) {
                return null;
            }
            MpSize size = new MpSize(bmpSrc.PixelSize.Width * (int)newScale.Width, bmpSrc.PixelSize.Height * (int)newScale.Height);
            return Resize(bmpSrc, size);
        }

        public static Bitmap? Resize(this Bitmap bmpSrc, MpSize size) {

            var bmpTarget = bmpSrc.CreateScaledBitmap(new PixelSize((int)size.Width, (int)size.Height));
            return bmpTarget;
        }
        public static MpSize ResizeKeepAspect(this MpSize src, double maxWidth, double maxHeight, bool enlarge = false) {
            maxWidth = enlarge ? maxWidth : Math.Min(maxWidth, src.Width);
            maxHeight = enlarge ? maxHeight : Math.Min(maxHeight, src.Height);

            double rnd = Math.Min(maxWidth / src.Width, maxHeight / src.Height);
            return new MpSize(Math.Round(src.Width * rnd), Math.Round(src.Height * rnd));
        }
        public static unsafe string ToAsciiImage(this Bitmap bmpSrc) {
            MpConsole.WriteLine($"[ToAsciiImage] Unsafe BEGIN ", true);
            // since this doesn't create accurate images in text (
            MpSize size = bmpSrc.PixelSize.ToPortableSize(1).ResizeKeepAspect(100, 100);
            // FIX SCALE ISSUE
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
                    var sb = new StringBuilder();
                    for (int row = 0; row < height; row++) {
                        for (int col = 0; col < width; col++) {
                            PixelColor c = pixels[col, row];
                            byte avg = (byte)((double)(c.Red + c.Green + c.Blue) / 3.0d);
                            PixelColor grayColor = new PixelColor() { Alpha = 255, Red = avg, Green = avg, Blue = avg };
                            int index = (int)((double)(grayColor.Red * 10) / 255.0d);
                            sb.Append(asciiChars[index]);
                            bmpPtr = PutPixel(writeableBitmap, c, bmpPtr);
                        }
                        sb.AppendLine();
                    }
                    MpConsole.WriteLine($"[ToAsciiImage] Unsafe END ", false, true);
                    return sb.ToString();
                }
            }
        }

        #endregion

        #region Operations

        public static int GetPixelColorCount(this Bitmap bmpSource, MpColor color, double max_dist) {
            int count = 0;
            PixelColor match_color = color.ToPixelColor();

            var pixels = GetPixels(bmpSource);
            for (int x = 0; x < bmpSource.PixelSize.Width; x++) {
                for (int y = 0; y < bmpSource.PixelSize.Height; y++) {
                    PixelColor currentColor = pixels[x, y];
                    if (match_color.ColorDistance(currentColor) <= max_dist) {
                        count++;
                    }
                }
            }

            //order the list from most used to least used before returning
            return count;
        }

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
                        //If it exists, increment the paramValue and update it
                        countDictionary[currentColor] = currentCount + 1;
                    }
                }
            }

            //order the list from most used to least used before returning
            return countDictionary.OrderByDescending(o => o.Value).ToList();
        }

        public static bool IsEmptyOrTransprent(this Bitmap bmp, byte min_alpha = 0) {
            return bmp.IsEmpty() || bmp.IsTransparent(min_alpha);
        }
        public static bool IsEmpty(this Bitmap bmp) {
            // NOTE this returns true for individual dimensions since if either are 0 it won't be visible
            if (bmp == null) {
                return true;
            }
            return bmp.PixelSize.Width == 0 || bmp.PixelSize.Height == 0;
        }
        public static unsafe bool IsTransparent(this Bitmap bmp, byte min_alpha = 0) {
            if (bmp == null) {
                return true;
            }
            MpConsole.WriteLine($"[IsTransparent] Unsafe BEGIN ", true);
            try {
                var pixels = GetPixels(bmp);
                using (var memoryStream = new MemoryStream()) {
                    bmp.Save(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var writeableBitmap = WriteableBitmap.Decode(memoryStream);
                    using (var lockedBitmap = writeableBitmap.Lock()) {
                        byte* bmpPtr = (byte*)lockedBitmap.Address;
                        int width = writeableBitmap.PixelSize.Width;
                        int height = writeableBitmap.PixelSize.Height;

                        for (int row = 0; row < height; row++) {
                            for (int col = 0; col < width; col++) {
                                PixelColor c = pixels[col, row];
                                if (c.Alpha > min_alpha) {
                                    MpConsole.WriteLine($"[IsTransparent] Unsafe END ", false, true);
                                    return false;
                                }
                            }
                        }
                    }
                    MpConsole.WriteLine($"[IsTransparent] Unsafe END ", true);
                    return true;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"IsTansprent error. ", ex);
                return false;
            }
        }
        #endregion

        #region Read/Write
        public static unsafe PixelColor[,] GetPixels(this Bitmap bitmap) {

            MpConsole.WriteLine($"[GetPixels] Unsafe BEGIN ", true);
            lock (_getPixelsLock) {
                using (var memoryStream = new MemoryStream()) {
                    try {
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

                        MpConsole.WriteLine($"[GetPixels] Unsafe END ", false, true);
                        return pixels;
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error getting bmp pixels. ", ex);
                        return new PixelColor[,] { };
                    }
                }
            }

        }
        private static unsafe byte* PutPixel(WriteableBitmap bitmap, PixelColor pixel, byte* bmpPtr) {
            //MpConsole.WriteLine($"[PutPixel] Unsafe BEGIN ", true);
            *bmpPtr++ = pixel.Blue;
            *bmpPtr++ = pixel.Green;
            *bmpPtr++ = pixel.Red;
            *bmpPtr++ = pixel.Alpha;

            //MpConsole.WriteLine($"[PutPixel] Unsafe END ", false, true);
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
