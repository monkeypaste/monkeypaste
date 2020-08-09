
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static MpWpfApp.MpShellEx;

namespace MpWpfApp {
    public static class MpHelpers {
        //private static readonly Lazy<MpHelperSingleton> lazy = new Lazy<MpHelperSingleton>(() => new MpHelperSingleton());
        //public static static MpHelperSingleton Instance { get { return lazy.Value; } }
        public static Random Rand = new Random();

        public static bool ApplicationIsActivated() {
            var activatedHandle = WinApi.GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            uint activeProcId;
            WinApi.GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return (int)activeProcId == procId;
        }
        /// <summary>
        /// Take the screenshot of the active window using the CopyFromScreen method relative to the bounds of the form.
        /// // Use it like : 
        //WindowScreenshotWithoutClass(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "window_screen_noclass.jpg", ImageFormat.Jpeg);
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public static string GetRandomString(int maxCharsPerLine = 50, int maxLines = 50) {
            StringBuilder str_build = new StringBuilder();
            int numLines = Rand.Next(1, maxLines);

            for(int i = 0;i < numLines;i++) {
                int numCharsOnLine = Rand.Next(1, maxCharsPerLine);
                for(int j = 0;j < numCharsOnLine;j++) {
                    double flt = Rand.NextDouble();
                    int shift = Convert.ToInt32(Math.Floor(25 * flt));
                    char letter = Convert.ToChar(shift + 65);
                    str_build.Append(letter);
                }
                str_build.Append('\n');
            }
            return str_build.ToString();
        }
        
        public static System.Drawing.Color GetDominantColor(System.Drawing.Bitmap bmp) {            
            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for(int x = 0;x < bmp.Width;x++) {
                for(int y = 0;y < bmp.Height;y++) {
                    System.Drawing.Color clr = bmp.GetPixel(x,y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            return System.Drawing.Color.FromArgb((byte)r, (byte)g, (byte)b);
        }


        public static string GetProcessPath(IntPtr hwnd) {
            try {
                uint pid = 0;
                WinApi.GetWindowThreadProcessId(hwnd, out pid);
                Process proc = Process.GetProcessById((int)pid);
                return proc.MainModule.FileName.ToString();
            }
            catch(Exception e) {
                return GetProcessPath(((MpMainWindowViewModel)((MpMainWindow)App.Current.MainWindow).DataContext).ClipboardMonitor.LastWindowWatcher.ThisAppHandle);
            }
        }
        public static string GetMainModuleFilepath(int processId) {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using(var searcher = new ManagementObjectSearcher(wmiQueryString)) {
                using(var results = searcher.Get()) {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if(mo != null) {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }
        public static void ColorToHSV(System.Drawing.Color color,out double hue,out double saturation,out double value) {
            int max = Math.Max(color.R,Math.Max(color.G,color.B));
            int min = Math.Min(color.R,Math.Min(color.G,color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }
        public static System.Drawing.Color ColorFromHSV(double hue,double saturation,double value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if(hi == 0)
                return System.Drawing.Color.FromArgb(255,(byte)v,(byte)t,(byte)p);
            else if(hi == 1)
                return System.Drawing.Color.FromArgb(255,(byte)q,(byte)v,(byte)p);
            else if(hi == 2)
                return System.Drawing.Color.FromArgb(255,(byte)p,(byte)v,(byte)t);
            else if(hi == 3)
                return System.Drawing.Color.FromArgb(255,(byte)p,(byte)q,(byte)v);
            else if(hi == 4)
                return System.Drawing.Color.FromArgb(255,(byte)t,(byte)p,(byte)v);
            else
                return System.Drawing.Color.FromArgb(255,(byte)v,(byte)p,(byte)q);
        }
        public static System.Drawing.Color GetInvertedColor(System.Drawing.Color c) {
            double h, s, v;
            ColorToHSV(c,out h,out s,out v);
            h = (h + 180) % 360;
            return ColorFromHSV(h,s,v);
        }
        public static bool IsBright(Color c, int brightThreshold = 130) {
            return (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114) > brightThreshold;
        }
        public static Brush ChangeBrushAlpha(Brush brush,byte alpha) {
            var b = (SolidColorBrush)brush;
            var c = b.Color;
            c.A = alpha;
            b.Color = c;
            return b;
        }
        public static Brush ChangeBrushBrightness(Brush brush, float correctionFactor) {
            if(correctionFactor == 0.0f) {
                return brush;
            }
            SolidColorBrush b = (SolidColorBrush)brush;
            float red = (float)b.Color.R;
            float green = (float)b.Color.G;
            float blue = (float)b.Color.B;

            if (correctionFactor < 0) {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            } else {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return new SolidColorBrush(Color.FromArgb(b.Color.A, (byte)red, (byte)green, (byte)blue));
        }
        public static Color GetRandomColor(byte alpha = 255) {
            if(alpha == 255) {
                return  Color.FromArgb(alpha,(byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            }
            return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256),(byte)Rand.Next(256));
        }
        public static System.Drawing.Icon GetIconFromBitmap(System.Drawing.Bitmap bmp) {
            IntPtr Hicon = bmp.GetHicon();
            return System.Drawing.Icon.FromHandle(Hicon);
        }
        public static string GetColorString(Color c) {
            return (int)c.A + "," + (int)c.R + "," + (int)c.G + "," + (int)c.B;
        }
        public static System.Drawing.Color GetColorFromString(string colorStr) {
            if (colorStr == null || colorStr == String.Empty) {
                colorStr = GetColorString(GetRandomColor());
            }
            int[] c = new int[colorStr.Split(',').Length];
            for (int i = 0; i < c.Length; i++) {
                c[i] = Convert.ToInt32(colorStr.Split(',')[i]);
            }
            if (c.Length == 3) {
                return System.Drawing.Color.FromArgb(255/*c[3]*/, c[0], c[1], c[2]);
            }
            return System.Drawing.Color.FromArgb(c[3], c[0], c[1], c[2]);
        }

        public static IPAddress GetCurrentIPAddress() {
            Ping ping = new Ping();
            var replay = ping.Send(Dns.GetHostName());

            if(replay.Status == IPStatus.Success) {
                return replay.Address;
            }
            return null;
        }
        public static bool CheckForInternetConnection() {
            try {
                using(var client = new WebClient())
                using(client.OpenRead("http://www.google.com/")) {
                    return true;
                }
            } catch(Exception e) {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        public static int GetMaxLine(string text) {
            int cr = 0, mr = 0,mc = int.MinValue,cc = 0;
            char LF = '\n';
            foreach(char c in text.ToCharArray()) {
                if(c == LF) {
                    if(cc > mc) {
                        mc = cc;
                        mr = cr;
                    }
                    cc = 0;
                    cr++;
                } else {
                    cc++;
                }
            }
            return mr;
        }
        public static int GetRowCount(string text,int lineNum) {
            int ccount = 0,cr = 0;
            char LF = '\n';
            foreach(char c in text.ToCharArray()) {
                if(cr == lineNum) {
                    ccount++;
                }
                if(c == LF) {
                    if(cr == lineNum) {
                        return ccount;
                    }
                    cr++;
                }
            }
            return ccount;
        }
        public static Size GetTextDimensions(string text) {
            int rcount = 0, ccount = int.MinValue;
            char LF = '\n';
            //cur col count
            int  ccc = 0;
            foreach(char c in text.ToCharArray()) {
                ccc++;
                if(c == LF) {
                    rcount++;
                    if(ccc > ccount) {
                        ccount = ccc;
                    }
                    ccc = 0;
                }
            }
            ccount = ccount == 0 ? 1 : ccount;
            rcount = rcount == 0 ? 1 : rcount;
            return new Size(ccount,rcount);
        }

        public static long FileListSize(string[] paths) {
            long total = 0;
            foreach(string path in paths) {
                if(Directory.Exists(path)) {
                    total += CalcDirSize(path,true);
                } else if(File.Exists(path)) {
                    total += (new FileInfo(path)).Length;
                }
            }
            return total;
        }
        private static long CalcDirSize(string sourceDir,bool recurse = true) {
            return _CalcDirSize(new DirectoryInfo(sourceDir),recurse);
        }
        private static long _CalcDirSize(DirectoryInfo di,bool recurse = true) {
            long size = 0;
            FileInfo[] fiEntries = di.GetFiles();
            foreach(var fiEntry in fiEntries) {
                Interlocked.Add(ref size,fiEntry.Length);
            }

            if(recurse) {
                DirectoryInfo[] diEntries = di.GetDirectories("*.*",SearchOption.TopDirectoryOnly);
                System.Threading.Tasks.Parallel.For<long>(0,diEntries.Length,() => 0,(i,loop,subtotal) =>
                {
                    if((diEntries[i].Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) return 0;
                    subtotal += _CalcDirSize(diEntries[i],true);
                    return subtotal;
                }, 
                    (x) => Interlocked.Add(ref size,x)
                );

            }
            return size;
        }
       /* public static long DirSize(string sourceDir,bool recurse) {
            long size = 0;
            string[] fileEntries = Directory.GetFiles(sourceDir);

            foreach(string fileName in fileEntries) {
                Interlocked.Add(ref size,(new FileInfo(fileName)).Length);
            }

            if(recurse) {
                string[] subdirEntries = Directory.GetDirectories(sourceDir);

                Parallel.For<long>(0,subdirEntries.Length,() => 0,(i,loop,subtotal) =>
                {
                    if((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint) {
                        subtotal += DirSize(subdirEntries[i],true);
                        return subtotal;
                    }
                    return 0;
                },
                    (x) => Interlocked.Add(ref size,x)
                );
            }
            return size;
        }*/
        public static int GetLineCount(string str) {
            char CR = '\r';
            char LF = '\n';
            //line count
            int lc = 0;
            //previous char
            char pc = '\0';
            //pending termination
            bool pt = false;
            foreach(char c in str.ToCharArray()) {
                if(c == CR || c == LF) {
                    if(pc == CR && c == LF) {
                        continue;
                    }
                    lc++;
                    pt = false;
                } else if(!pt) {
                    pt = true;
                }
                pc = c;
            }
            if(pt) {
                lc++;
            }
            return lc;
        }
        
        /*public static string GeneratePassword() {
            var generator = new MpPasswordGenerator(minimumLengthPassword: 8,
                                      maximumLengthPassword: 12,
                                      minimumUpperCaseChars: 2,
                                      minimumSpecialChars: 2);
            return generator.Generate();
        }*/
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files) {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs) {

                foreach (DirectoryInfo subdir in dirs) {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        public static string GetCPUInfo() {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach(ManagementObject mo in moc) {
                if(cpuInfo == "") {
                    //Get only the first CPU's ID
                    cpuInfo = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return cpuInfo;
        }

        public static ImageSource GetIconImage(string sourcePath) {
            return ConvertBitmapToBitmapSource(GetBitmapFromFilePath(sourcePath, IconSizeEnum.LargeIcon48));
        }

        public static byte[] ConvertBitmapSourceToByteArray(BitmapSource bs) {
            PngBitmapEncoder encoder = new PngBitmapEncoder(); 
            //encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            // byte[] bit = new byte[0];
            using (MemoryStream stream = new MemoryStream()) {
                encoder.Frames.Add(BitmapFrame.Create(bs));
                encoder.Save(stream);
                byte[] bit = stream.ToArray();
                stream.Close();
                return bit;
            }
        }

        public static BitmapSource ConvertByteArrayToBitmapSource(byte[] bytes) {
            return (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);
        }

        public static BitmapSource ConvertBitmapToBitmapSource(System.Drawing.Bitmap bitmap) {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public static System.Drawing.Bitmap ConvertBitmapSourceToBitmap(BitmapSource bitmapsource) {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream()) {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        public static BitmapSource MergeImages(IList<BitmapSource> bmpSrcList) {
            int width = 0;
            int height = 0;
            int dpiX = 0;
            int dpiY = 0;
            // Get max width and height of the image
            foreach (var image in bmpSrcList) {
                width = Math.Max(image.PixelWidth,width);
                height = Math.Max(image.PixelHeight, height);
                dpiX = Math.Max((int)image.DpiX, dpiX);
                dpiY = Math.Max((int)image.DpiY, dpiY);
            }
            var renderTargetBitmap = new RenderTargetBitmap(width, height, dpiX, dpiY, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen()) {
                foreach (var image in bmpSrcList) {
                    drawingContext.DrawImage(image, new Rect(0, 0, width, height));
                }
            }
            renderTargetBitmap.Render(drawingVisual);

            return renderTargetBitmap;
        }

        public static BitmapSource TintBitmapSource(BitmapSource bmpSrc, Color tint) {
            var bmp = new WriteableBitmap(bmpSrc);
            var pixels = GetPixels(bmp);
            var pixelColor = new PixelColor[1, 1];
            pixelColor[0, 0] = new PixelColor { Alpha = tint.A, Red = tint.R, Green = tint.G, Blue = tint.B };

            for (int x = 0; x < bmp.Width; x++) {
                for (int y = 0; y < bmp.Height; y++) {
                    PixelColor c = pixels[x, y];
                    //Color gotColor = Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
                    if (c.Alpha > 0) {
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

            source.CopyPixels(result, width * 4, 0, false);
            return result;
        }

        private static void PutPixels(WriteableBitmap bitmap, PixelColor[,] pixels, int x, int y) {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, x, y);
        }


        public static bool IsPathDirectory(string str) {
            // get the file attributes for file or directory
            return File.GetAttributes(str).HasFlag(FileAttributes.Directory);
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }
}
