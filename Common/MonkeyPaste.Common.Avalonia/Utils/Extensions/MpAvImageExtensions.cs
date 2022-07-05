using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvImageExtensions {

        public static IBitmap Tint(this IImage iimg, Brush tint, bool retainAlpha = true) {
            if(iimg is Bitmap bmpSrc) {
                //Bitmap formattedBmpSrc = null;
                //if (bmpSrc.Dpi.X != 96 || bmpSrc.Dpi.Y != 96) {
                //    //means bmp dpi isn't 96
                //    double dpi = 96;
                //    int width = bmpSrc.PixelSize.Width;
                //    int height = bmpSrc.PixelSize.Height;

                //    int stride = width * 4; // 4 bytes per pixel
                //    byte[] pixelData = new byte[stride * height]; 

                //    bmpSrc.CopyPixels(pixelData, stride, 0);

                //    formattedBmpSrc = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, stride);
                //} else {
                //    formattedBmpSrc = bmpSrc;
                //}

                var wbmp = new WriteableBitmap(bmpSrc.PixelSize, bmpSrc.Dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);

                //using(Stream stream = new MemoryStream()) {
                //    bmpSrc.Save(stream);

                //    using(var fb = wbmp.Lock()) {
                //        var data = new int[fb.Size.Width * fb.Size.Height];

                //        for (int y = 0; y < fb.Size.Height; y++) {
                //            for (int x = 0; x < fb.Size.Width; x++) {
                //                var color = new Color(fillAlpha, 0, 255, 0);

                //                if (premul) {
                //                    byte r = (byte)(color.R * color.A / 255);
                //                    byte g = (byte)(color.G * color.A / 255);
                //                    byte b = (byte)(color.B * color.A / 255);

                //                    color = new Color(fillAlpha, r, g, b);
                //                }

                //                data[y * fb.Size.Width + x] = (int)color.ToUint32();
                //            }
                //        }

                //        Marshal.Copy(data, 0, fb.Address, fb.Size.Width * fb.Size.Height);
                //    }
                //}
            }

            return null;
            

        }
        
    }
}
