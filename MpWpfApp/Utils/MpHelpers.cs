using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static MpWpfApp.MpShellEx;
using static QRCoder.PayloadGenerator;

namespace MpWpfApp {
    public static class MpHelpers {
        public static Random Rand { get; set; } = new Random();

        public static bool IsInDesignMode {
            get {
                return DesignerProperties.GetIsInDesignMode(new DependencyObject());
            }
        }

        public static bool ApplicationIsActivated() {
            var activatedHandle = WinApi.GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            WinApi.GetWindowThreadProcessId(activatedHandle, out uint activeProcId);

            return (int)activeProcId == procId;
        }

        public static string GetRandomString(int maxCharsPerLine = 50, int maxLines = 50) {
            StringBuilder str_build = new StringBuilder();
            int numLines = Rand.Next(1, maxLines);

            for (int i = 0; i < numLines; i++) {
                int numCharsOnLine = Rand.Next(1, maxCharsPerLine);
                for (int j = 0; j < numCharsOnLine; j++) {
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

            for (int x = 0; x < bmp.Width; x++) {
                for (int y = 0; y < bmp.Height; y++) {
                    System.Drawing.Color clr = bmp.GetPixel(x, y);

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

        public static string RemoveSpecialCharacters(string str) {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", string.Empty, RegexOptions.Compiled);
        }

        public static string GetProcessMainWindowTitle(IntPtr hWnd) {
            if (hWnd == null) {
                throw new Exception("MpHelpers error hWnd is null");
            }
            uint processId;
            WinApi.GetWindowThreadProcessId(hWnd, out processId);
            Process proc = Process.GetProcessById((int)processId);
            return proc.MainWindowTitle;
        }
        public static string GetProcessPath(IntPtr hwnd) {
            try {
                WinApi.GetWindowThreadProcessId(hwnd, out uint pid);
                Process proc = Process.GetProcessById((int)pid);
                return proc.MainModule.FileName.ToString();
            }
            catch (Exception e) {
                Console.WriteLine("MpHelpers.GetProcessPath error (likely cannot find process path: " + e.ToString());
                return GetProcessPath(((MpClipTrayViewModel)((MpMainWindowViewModel)((MpMainWindow)App.Current.MainWindow).DataContext).ClipTrayViewModel).ClipboardMonitor.LastWindowWatcher.ThisAppHandle);
            }
        }        
        
        public static Point GetMousePosition() {
            WinApi.Win32Point w32Mouse = new WinApi.Win32Point();
            WinApi.GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public static string GetMainModuleFilepath(int processId) {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using (var searcher = new ManagementObjectSearcher(wmiQueryString)) {
                using (var results = searcher.Get()) {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if (mo != null) {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }

        public static void ColorToHSV(System.Drawing.Color color, out double hue, out double saturation, out double value) {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static System.Drawing.Color ColorFromHSV(double hue, double saturation, double value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = (hue / 60) - Math.Floor(hue / 60);

            value *= 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - (f * saturation)));
            int t = Convert.ToInt32(value * (1 - ((1 - f) * saturation)));

            if (hi == 0) {
                return System.Drawing.Color.FromArgb(255, (byte)v, (byte)t, (byte)p);
            } else if (hi == 1) {
                return System.Drawing.Color.FromArgb(255, (byte)q, (byte)v, (byte)p);
            } else if (hi == 2) {
                return System.Drawing.Color.FromArgb(255, (byte)p, (byte)v, (byte)t);
            } else if (hi == 3) {
                return System.Drawing.Color.FromArgb(255, (byte)p, (byte)q, (byte)v);
            } else if (hi == 4) {
                return System.Drawing.Color.FromArgb(255, (byte)t, (byte)p, (byte)v);
            } else {
                return System.Drawing.Color.FromArgb(255, (byte)v, (byte)p, (byte)q);
            }
        }

        public static System.Drawing.Color GetInvertedColor(System.Drawing.Color c) {
            ColorToHSV(c, out double h, out double s, out double v);
            h = (h + 180) % 360;
            return ColorFromHSV(h, s, v);
        }

        public static bool IsBright(Color c, int brightThreshold = 130) {
            return (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114) > brightThreshold;
        }

        public static Brush ChangeBrushAlpha(Brush brush, byte alpha) {
            var b = (SolidColorBrush)brush;
            var c = b.Color;
            c.A = alpha;
            b.Color = c;
            return b;
        }

        public static Brush ChangeBrushBrightness(Brush brush, float correctionFactor) {
            if (correctionFactor == 0.0f) {
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
            if (alpha == 255) {
                return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            }
            return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
        }

        public static System.Drawing.Icon GetIconFromBitmap(System.Drawing.Bitmap bmp) {
            IntPtr hIcon = bmp.GetHicon();
            return System.Drawing.Icon.FromHandle(hIcon);
        }

        public static string GetColorString(Color c) {
            return (int)c.A + "," + (int)c.R + "," + (int)c.G + "," + (int)c.B;
        }

        public static System.Drawing.Color GetColorFromString(string colorStr) {
            if (string.IsNullOrEmpty(colorStr)) {
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

            if (replay.Status == IPStatus.Success) {
                return replay.Address;
            }
            return null;
        }

        public static bool CheckForInternetConnection() {
            try {
                using (var client = new WebClient())
                using (client.OpenRead("http://www.google.com/")) {
                    return true;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static int GetColCount(string text) {
            int maxCols = int.MinValue;
            foreach (string row in text.Split(Environment.NewLine.ToCharArray())) {
                if (row.Length > maxCols) {
                    maxCols = row.Length;
                }
            }
            return maxCols;
        }

        public static int GetRowCount(string text) {
            return text.Split(Environment.NewLine.ToCharArray()).Length;
        }

        public static Size GetTextDimensions(string text) {
            return new Size((double)GetRowCount(text), (double)GetColCount(text));
        }

        public static long FileListSize(string[] paths) {
            long total = 0;
            foreach (string path in paths) {
                if (Directory.Exists(path)) {
                    total += CalcDirSize(path, true);
                } else if (File.Exists(path)) {
                    total += new FileInfo(path).Length;
                }
            }
            return total;
        }

        public static string GetUniqueFileName(string fullPath) {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath)) {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }

        public static string CombineRichText(string rt1,string rt2) {
            using (System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox()) {
                rtb.Rtf = rt1;
                rtb.Text += Environment.NewLine;
                rtb.Select(rtb.TextLength, 0);
                rtb.SelectedRtf = rt2;
                return rtb.Rtf;
            }
        }

        public static string WriteTextToFile(string filePath, string text, bool isTemporary = false) {
            StreamWriter of = new StreamWriter(filePath);
            of.Write(text);
            of.Close();
            return filePath;
        }

        public static string WriteBitmapSourceToFile(string filePath, BitmapSource bmpSrc, bool isTemporary = false) {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(MpHelpers.ConvertBitmapSourceToBitmap(bmpSrc));
            bmp.Save(filePath, ImageFormat.Png);
            return filePath;
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

            foreach (ManagementObject mo in moc) {
                if (string.IsNullOrEmpty(cpuInfo)) {
                    //Get only the first CPU's ID
                    cpuInfo = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return cpuInfo;
        }

        public static ImageSource GetIconImage(string sourcePath) {
            if (!File.Exists(sourcePath)) {
                return ConvertBitmapToBitmapSource(System.Drawing.SystemIcons.Warning.ToBitmap());
            }
            return ConvertBitmapToBitmapSource(GetBitmapFromFilePath(sourcePath, IconSizeEnum.MediumIcon32));
        }

        public static BitmapSource ResizeBitmapSource(BitmapSource bmpSrc, Size newSize) {
            System.Drawing.Bitmap result = new System.Drawing.Bitmap((int)newSize.Width, (int)newSize.Height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage((System.Drawing.Image)result)) {
                //The interpolation mode produces high quality images
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(ConvertBitmapSourceToBitmap(bmpSrc), 0, 0, (int)newSize.Width, (int)newSize.Height);
                g.Dispose();
                return ConvertBitmapToBitmapSource(result);
            }
        }

        public static bool ByteArrayCompare(byte[] b1, byte[] b2) {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && WinApi.memcmp(b1, b2, b1.Length) == 0;
        }
        public static string ConvertBitmapSourceToPlainText(BitmapSource bmpSource) {
            string[] asciiChars = { "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", "&nbsp;" };
            System.Drawing.Bitmap image = ConvertBitmapSourceToBitmap(ResizeBitmapSource(bmpSource,new Size(MpMeasurements.Instance.ClipTileBorderSize,MpMeasurements.Instance.ClipTileContentHeight)));

            string outStr = string.Empty;
            for (int h = 0; h < image.Height; h++) {
                for (int w = 0; w < image.Width; w++) {
                    System.Drawing.Color pixelColor = image.GetPixel(w, h);
                    //Average out the RGB components to find the Gray Color
                    int red = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int green = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    int blue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    System.Drawing.Color grayColor = System.Drawing.Color.FromArgb(red, green, blue);
                    int index = (grayColor.R * 10) / 255;
                    outStr += asciiChars[index];
                }
                outStr += Environment.NewLine;
            }

            return outStr;
        }

        public static string ReadCharactersFromBitmapSource(BitmapSource bmpSource) {
            
            return string.Empty;
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
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

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

        public static System.Drawing.Color ConvertSolidColorBrushToWinFormsColor(SolidColorBrush scb) {
            return System.Drawing.Color.FromArgb(scb.Color.A, scb.Color.R, scb.Color.G, scb.Color.B);
        }

        public static SolidColorBrush ConvertWinFormsColorToSolidColorBrush(System.Drawing.Color c) {
            return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }

        public static BitmapSource ConvertRichTextToImage(string rt, int fontSize = 12) {
            //return null;
            string pt = ConvertRichTextToPlainText(rt);
            int w = GetColCount(pt) * fontSize;
            int h = GetRowCount(pt) * fontSize;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(w, h);           
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bmp)) {
                graphics.DrawRtfText(rt, new System.Drawing.RectangleF(0, 0, bmp.Width, bmp.Height), 1f);
                graphics.Flush();
                graphics.Dispose();
            }
            return ConvertBitmapToBitmapSource(bmp);
        }

        public static string ConvertPlainTextToRichText(string plainText) {
            string escapedPlainText = plainText.Replace(@"\", @"\\").Replace("{", @"\{").Replace("}", @"\}");
            string rtf = @"{\rtf1\ansi{\fonttbl\f0\fswiss Helvetica;}\f0\pard ";
            rtf += escapedPlainText.Replace(Environment.NewLine, @" \par ");
            rtf += " }";
            return rtf;
        }

        public static string ConvertRichTextToPlainText(string richText) {
            System.Windows.Controls.RichTextBox rtb = new System.Windows.Controls.RichTextBox();
            rtb.SetRtf(richText);
            return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.Replace("''", "'");
        }

        public static FlowDocument ConvertRtfToFlowDocument(string rtf) {
            using (MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(rtf))) {
                FlowDocument flowDocument = new FlowDocument();
                TextRange range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                range.Load(stream, System.Windows.DataFormats.Rtf);
                return flowDocument;
            }
        }

        public static string ConvertFlowDocumentToRtf(FlowDocument fd) {
            string rtf = string.Empty;
            using (MemoryStream ms = new MemoryStream()) {
                TextRange range2 = new TextRange(fd.ContentStart, fd.ContentEnd);
                range2.Save(ms, System.Windows.DataFormats.Rtf);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader sr = new StreamReader(ms)) {
                    rtf = sr.ReadToEnd();
                }
            }
            return rtf;
        }

        public static async Task<string> ShortenUrl(string url) {
            string bitlyToken = @"f6035b9ed05ac82b42d4853c984e34a4f1ba05d8";
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api-ssl.bitly.com/v4/shorten") {
                Content = new StringContent($"{{\"long_url\":\"{url}\"}}",
                                                Encoding.UTF8,
                                                "application/json")
            };

            try {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bitlyToken);
                var response = await client.SendAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) {
                    Console.WriteLine("Minify error: " + response.Content.ToString());
                    return string.Empty;
                }

                var responsestr = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responsestr);
                return jsonResponse["link"];
            }
            catch (Exception ex) {
                Console.WriteLine("Minify exception: " + ex.ToString());
                return string.Empty;
            }
        }

        public static TextRange FindStringRangeFromPosition(TextPointer position, string lowerCaseStr) {
            while (position != null) {
                var dir = LogicalDirection.Forward;
                if (position.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text) {
                    dir = LogicalDirection.Backward;
                }
                string textRun = position.GetTextInRun(dir).ToLower();

                // Find the starting index of any substring that matches "word".
                int indexInRun = textRun.IndexOf(lowerCaseStr);
                if (indexInRun >= 0) {
                    if (dir == LogicalDirection.Forward) {
                        return new TextRange(position.GetPositionAtOffset(indexInRun), position.GetPositionAtOffset(indexInRun + lowerCaseStr.Length));
                    } else {
                        return new TextRange(position.GetPositionAtOffset(indexInRun), position.GetPositionAtOffset(indexInRun - lowerCaseStr.Length));
                    }
                }
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
            // position will be null if "word" is not found.
            return null;
        }

        public static string PlainTextToRtf2(string input) {
            //first take care of special RTF chars
            StringBuilder backslashed = new StringBuilder(input);
            backslashed.Replace(@"\", @"\\");
            backslashed.Replace(@"{", @"\{");
            backslashed.Replace(@"}", @"\}");

            // then convert the string char by char
            StringBuilder sb = new StringBuilder();
            foreach (char character in backslashed.ToString()) {
                if (character <= 0x7f) {
                    sb.Append(character);
                } else {
                    sb.Append("\\u" + Convert.ToUInt32(character) + "?");
                }
            }
            return sb.ToString();
        }

        public static bool IsStringRichText(string text) {
            return text.StartsWith(@"{\rtf");
        }

        public static BitmapSource MergeImages(IList<BitmapSource> bmpSrcList) {
            int width = 0;
            int height = 0;
            int dpiX = 0;
            int dpiY = 0;
            // Get max width and height of the image
            foreach (var image in bmpSrcList) {
                width = Math.Max(image.PixelWidth, width);
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

            return ConvertRenderTargetBitmapToBitmapSource(renderTargetBitmap);
        }

        public static BitmapSource ConvertRenderTargetBitmapToBitmapSource(RenderTargetBitmap rtb) {
            var bitmapImage = new BitmapImage();
            var bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var stream = new MemoryStream()) {
                bitmapEncoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
        public static BitmapSource CombineBitmap(IList<BitmapSource> bmpSrcList, bool tileHorizontally = true) {
            if (bmpSrcList.Count == 1) {
                return bmpSrcList[0];
            }
            //read all images into memory
            List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;

            try {
                int width = 0;
                int height = 0;

                foreach (var bmpSrc in bmpSrcList) {
                    //create a Bitmap from the file and add it to the list
                    System.Drawing.Bitmap bitmap = ConvertBitmapSourceToBitmap(bmpSrc);

                    //update the size of the final bitmap
                    if (tileHorizontally) {
                        width += bitmap.Width;
                        height = Math.Max(bitmap.Height, height);
                    } else {
                        width = Math.Max(bitmap.Width, width);
                        height += bitmap.Height;
                    }
                    images.Add(bitmap);
                }

                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage)) {
                    //set background color
                    g.Clear(System.Drawing.Color.Transparent);

                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap image in images) {
                        g.DrawImage(image, new System.Drawing.Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                    g.Dispose();
                }
                return ConvertBitmapToBitmapSource(finalImage);
            }
            catch (Exception ex) {
                if (finalImage != null) {
                    finalImage.Dispose();
                }
                throw ex;
            } finally {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images) {
                    image.Dispose();
                }
            }
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

        public static BitmapSource ConvertUrlToQrCode(string url) {
            Url generator = new Url(url);
            string payload = generator.ToString();

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator()) {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData)) {
                    var qrCodeAsBitmap = qrCode.GetGraphic(20);
                    return MpHelpers.ConvertBitmapToBitmapSource(qrCodeAsBitmap);
                }
            }
        }

        public static bool IsPathDirectory(string str) {
            // get the file attributes for file or directory
            return File.GetAttributes(str).HasFlag(FileAttributes.Directory);
        }

        #region Private Methods

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

        private static long CalcDirSize(string sourceDir, bool recurse = true) {
            return CalcDirSizeHelper(new DirectoryInfo(sourceDir), recurse);
        }

        private static long CalcDirSizeHelper(DirectoryInfo di, bool recurse = true) {
            long size = 0;
            FileInfo[] fiEntries = di.GetFiles();
            foreach (var fiEntry in fiEntries) {
                Interlocked.Add(ref size, fiEntry.Length);
            }

            if (recurse) {
                DirectoryInfo[] diEntries = di.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
                System.Threading.Tasks.Parallel.For<long>(
                    0,
                    diEntries.Length,
                    () => 0,
                    (i, loop, subtotal) => {
                        if ((diEntries[i].Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) {
                            return 0;
                        }
                        subtotal += CalcDirSizeHelper(diEntries[i], true);
                        return subtotal;
                    },
                    (x) => Interlocked.Add(ref size, x));
            }
            return size;
        }

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PixelColor {
        public byte Blue;
        public byte Green;
        public byte Red;
        public byte Alpha;
    }
}
