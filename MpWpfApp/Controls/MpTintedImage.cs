using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpTintedImage : Image {
        private bool _isImageTinted = false;

        public MpTintedImage() : base() {
            Loaded += (s, e1) => {
                if (_isImageTinted == false) {
                    WriteableBitmap writeableBitmap = new WriteableBitmap((BitmapSource)Source);
                    Source = (ImageSource)TintBitmapSource(writeableBitmap, ((SolidColorBrush)TintBrush).Color);
                    _isImageTinted = true;
                }
            };
        }

        public Brush TintBrush {
            get {
                return (SolidColorBrush)GetValue(TintBrushProperty);
            }
            set {
                SetValue(TintBrushProperty, value);
            }
        }

        private static void OnDataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            //var image = (MpTintedImage)source;
            //image.
            
        }

        private static BitmapSource TintBitmapSource(WriteableBitmap bmp,Color tint) {
            var pixels = GetPixels(bmp);
            var pixelColor = new PixelColor[1, 1];
            pixelColor[0,0] = new PixelColor { Alpha = tint.A, Red = tint.R, Green = tint.G, Blue = tint.B };

            for (int x = 0; x < bmp.Width; x++) {
                for (int y = 0; y < bmp.Height; y++) {
                    PixelColor c = pixels[x, y];
                    //Color gotColor = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
                    if(c.Alpha > 0) {
                        PutPixels(bmp, pixelColor, x, y);
                    }
                }
            }
            return bmp;
        }

        private static PixelColor[,] GetPixels(BitmapSource source) {
            if (source.Format != PixelFormats.Bgra32) {
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }
            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixels(result, width * 4, 0,false);
            return result;
        }

        private static void PutPixels(WriteableBitmap bitmap, PixelColor[,] pixels, int x, int y) {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, x, y);
        }

        
        public static readonly DependencyProperty TintBrushProperty =
            DependencyProperty.Register("TintBrush",
                                        typeof(SolidColorBrush),
                                        typeof(MpTintedImage),
                                        new FrameworkPropertyMetadata(null, OnDataChanged));

    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }
}
