
using MpWinFormsClassLibrary;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using static MpWinFormsClassLibrary.ShellEx;

namespace System.Windows.Media {
    /// <summary>
    /// One-way converter from System.Drawing.Image to System.Windows.Media.ImageSource
    /// </summary>
    [ValueConversion(typeof(System.Drawing.Image), typeof(System.Windows.Media.ImageSource))]
    public class MpImageConverter : IValueConverter {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture) {
            // empty images are empty...
            if(value == null) { return null; }

            var image = (System.Drawing.Image)value;
            // Winforms Image we want to get the WPF Image from...
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            MemoryStream memoryStream = new MemoryStream();
            // Save to a memory stream...
            image.Save(memoryStream, ImageFormat.Bmp);
            // Rewind the stream...
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            return bitmap;
        }
        public string GetProcessPath(IntPtr hwnd) {
            uint pid = 0;
            WinApi.GetWindowThreadProcessId(hwnd, out pid);
            //return MpHelperSingleton.Instance.GetMainModuleFilepath((int)pid);
            Process proc = Process.GetProcessById((int)pid);
            return proc.MainModule.FileName.ToString();
        }
        private Image WindowScreenshotWithoutClass() {
            Rectangle bounds = GetScreenBoundsWithMouse();

            using(Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height)) {
                using(Graphics g = Graphics.FromImage(bitmap)) {
                    g.CopyFromScreen(new System.Drawing.Point(bounds.Left, bounds.Top), System.Drawing.Point.Empty, bounds.Size);
                }
                return bitmap;
            }
        }
        public Bitmap GetBitmapFromFilePath(string filepath, IconSizeEnum iconsize) {
            return ShellEx.GetBitmapFromFilePath(filepath, iconsize);
        }
        public Image GetIconImage(IntPtr sourceHandle) {
            return GetBitmapFromFilePath(MpHelperSingleton.Instance.GetProcessPath(sourceHandle), IconSizeEnum.ExtraLargeIcon);
            //return IconReader.GetFileIcon(MpHelperSingleton.Instance.GetProcessPath(sourceHandle),IconReader.IconSize.Large,false).ToBitmap();
        }
        public ImageSource GetIconImage(string sourcePath) {
            return ConvertImageToImageSource(GetBitmapFromFilePath(sourcePath, IconSizeEnum.ExtraLargeIcon));
            //return IconReader.GetFileIcon(MpHelperSingleton.Instance.GetProcessPath(sourceHandle),IconReader.IconSize.Large,false).ToBitmap();
        }
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) {
            return null;
        }
        public Image ConvertImageSourceToImage(ImageSource imageSource) {
            return ConvertByteArrayToImage(ConvertImageSourceToByteArray(imageSource));
        }
        public ImageSource ConvertImageToImageSource(Image image) {
            return ConvertByteArrayToImageSource(ConvertImageToByteArray(image));
        }
        public byte[] ConvertImageToByteArray(Image img) {
            using (MemoryStream ms = new MemoryStream()) {
                img.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }                
        }
        public Image ConvertByteArrayToImage(byte[] rawBytes) {
            return Image.FromStream(new MemoryStream(rawBytes), true);
        }
        public byte[] ConvertImageSourceToByteArray(ImageSource imageSource) {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;

            if(bitmapSource != null) {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using(var stream = new MemoryStream()) {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }
        public ImageSource ConvertByteArrayToImageSource(byte[] rawBytes) {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(rawBytes);
            bi.EndInit();
            return bi;
        }
        public Rectangle GetScreenWorkingAreaWithMouse() {
            foreach(Screen screen in Screen.AllScreens) {
                //get cursor pos
                WinApi.PointInter lpPoint;
                WinApi.GetCursorPos(out lpPoint);
                System.Drawing.Point mp = (System.Drawing.Point)lpPoint;
                if(screen.WorkingArea.Contains(mp)) {
                    return screen.WorkingArea;
                }
            }
            return Screen.FromHandle(Process.GetCurrentProcess().Handle).WorkingArea;
        }
        public Rectangle GetScreenBoundsWithMouse() {
            foreach(Screen screen in Screen.AllScreens) {
                //get cursor pos
                WinApi.PointInter lpPoint;
                WinApi.GetCursorPos(out lpPoint);
                System.Drawing.Point mp = (System.Drawing.Point)lpPoint;
                if(screen.WorkingArea.Contains(mp)) {
                    return screen.Bounds;
                }
            }
            return Screen.FromHandle(Process.GetCurrentProcess().Handle).Bounds;
        }
    }
}
