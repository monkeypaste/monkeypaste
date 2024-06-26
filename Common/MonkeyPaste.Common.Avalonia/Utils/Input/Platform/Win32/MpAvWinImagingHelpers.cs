﻿using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace MonkeyPaste.Common.Avalonia {
#pragma warning disable CA1416 // Validate platform compatibility
    public static class MpAvWinImagingHelpers {

        #region System.Drawing

        public static Bitmap? ToAvBitmap(this System.Drawing.Image img) {
            if (img == null || !OperatingSystem.IsWindows()) {
                return null;
            }
            using (System.Drawing.Bitmap bmp = new(img)) {
                return bmp.ToAvBitmap();
            }
        }

        public static Bitmap? ToAvBitmap(this System.Drawing.Bitmap bmp) {
            if (bmp == null || !OperatingSystem.IsWindows()) {
                return null;
            }
            System.Drawing.Imaging.BitmapData? bitmapData =
                    bmp.LockBits(
                        new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Bitmap avBmp = new Bitmap(
                PixelFormat.Bgra8888,
                AlphaFormat.Premul,
                bitmapData.Scan0,
                new PixelSize(bitmapData.Width, bitmapData.Height),
                new Vector(96, 96),
                bitmapData.Stride);

            bmp.UnlockBits(bitmapData);
            //bmp.Dispose();
            return avBmp;
        }

        #endregion
    }
#pragma warning restore CA1416 // Validate platform compatibility
}
