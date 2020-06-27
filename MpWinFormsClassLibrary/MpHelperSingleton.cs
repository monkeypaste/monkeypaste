
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using static MpWinFormsClassLibrary.ShellEx;

namespace MpWinFormsClassLibrary {
    public static class Extensions {
        public static void DoubleBuffered(this Control control, bool enabled) {
            var prop = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(control, enabled, null);
        }
        public static IEnumerable<T> GetAll<T>(this Control control) {
            var controls = control.Controls.Cast<Control>();
            return controls.SelectMany(ctrl => ctrl.GetAll<T>()).Concat(controls.OfType<T>());
        }
        public static bool IsNamedObject(this object obj) {
            return obj.GetType().FullName == "MS.Internal.NamedObject";
        }
        public static T GetChildOfType<T>(this DependencyObject depObj)  where T : DependencyObject {
            if(depObj == null) return null;

            for(int i = 0;i < VisualTreeHelper.GetChildrenCount(depObj);i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if(result != null) return result;
            }
            return null;
        }
    }
    public class MpHelperSingleton {
        private static readonly Lazy<MpHelperSingleton> lazy = new Lazy<MpHelperSingleton>(() => new MpHelperSingleton());
        public static MpHelperSingleton Instance { get { return lazy.Value; } }
        public static Random Rand;
        private MpHelperSingleton() {
            Rand = new Random();
        }
        public MpImageConverter ImageConverter { get; set; } = new MpImageConverter();

        public bool ApplicationIsActivated() {
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
        public string GetRandomString(int maxCharsPerLine = 50, int maxLines = 50) {
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
        
        public System.Drawing.Color GetDominantColor(System.Drawing.Bitmap bmp) {            
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


        public ImageSource GetIconImage(string sourcePath) {
            return ImageConverter.ConvertImageToImageSource(GetBitmapFromFilePath(sourcePath, IconSizeEnum.ExtraLargeIcon));
            //return IconReader.GetFileIcon(MpHelperSingleton.Instance.GetProcessPath(sourceHandle),IconReader.IconSize.Large,false).ToBitmap();
        }
        public string GetProcessPath(IntPtr hwnd) {
            uint pid = 0;
            WinApi.GetWindowThreadProcessId(hwnd,out pid);
            //return MpHelperSingleton.Instance.GetMainModuleFilepath((int)pid);
            Process proc = Process.GetProcessById((int)pid);
            return proc.MainModule.FileName.ToString();
        }
        public string GetMainModuleFilepath(int processId) {
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
        public void ColorToHSV(System.Drawing.Color color,out double hue,out double saturation,out double value) {
            int max = Math.Max(color.R,Math.Max(color.G,color.B));
            int min = Math.Min(color.R,Math.Min(color.G,color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }
        public System.Drawing.Color ColorFromHSV(double hue,double saturation,double value) {
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
        public System.Drawing.Color GetInvertedColor(System.Drawing.Color c) {
            double h, s, v;
            ColorToHSV(c,out h,out s,out v);
            h = (h + 180) % 360;
            return ColorFromHSV(h,s,v);
        }
        public Color GetRandomColor(byte alpha = 255) {
            if(alpha == 255) {
                return  Color.FromArgb(alpha,(byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            }
            return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256),(byte)Rand.Next(256));
        }
        public IPAddress GetCurrentIPAddress() {
            Ping ping = new Ping();
            var replay = ping.Send(Dns.GetHostName());

            if(replay.Status == IPStatus.Success) {
                return replay.Address;
            }
            return null;
        }
        public bool CheckForInternetConnection() {
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
        public int GetMaxLine(string text) {
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
        public int GetRowCount(string text,int lineNum) {
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
        public Size GetTextDimensions(string text) {
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
        public bool IsBright(Color c,int brightThreshold = 130) {
            return (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114) > brightThreshold;
        }
        public System.Drawing.Color ChangeColorBrightness(System.Drawing.Color color,float correctionFactor) {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if(correctionFactor < 0) {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return System.Drawing.Color.FromArgb(color.A,(byte)red,(byte)green,(byte)blue);
        }
        public long FileListSize(string[] paths) {
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
        private long CalcDirSize(string sourceDir,bool recurse = true) {
            return _CalcDirSize(new DirectoryInfo(sourceDir),recurse);
        }
        private long _CalcDirSize(DirectoryInfo di,bool recurse = true) {
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
       /* public long DirSize(string sourceDir,bool recurse) {
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
        public int GetLineCount(string str) {
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
        
        public System.Drawing.Icon GetIconFromBitmap(System.Drawing.Bitmap bmp) {
           IntPtr Hicon = bmp.GetHicon();
           return System.Drawing.Icon.FromHandle(Hicon);
        }
        public string GetColorString(Color c) {
            return (int)c.A + "," + (int)c.R + "," + (int)c.G + "," + (int)c.B;
        }
        public System.Drawing.Color GetColorFromString(string colorStr) {
            if(colorStr == null || colorStr == String.Empty) {
                colorStr = GetColorString(GetRandomColor());
            }
            int[] c = new int[colorStr.Split(',').Length];
            for(int i = 0;i < c.Length;i++) {
                c[i] = Convert.ToInt32(colorStr.Split(',')[i]);
            }
            if(c.Length == 3) {
                return System.Drawing.Color.FromArgb(255/*c[3]*/,c[0],c[1],c[2]);
            }
            return System.Drawing.Color.FromArgb(c[3],c[0],c[1],c[2]);
        }
        //public Image GetIconImage(string path) {
        //    return (Image)IconReader.GetFileIcon(path,IconReader.IconSize.Large,false).ToBitmap();//Icon.ExtractAssociatedIcon(path).ToBitmap();
        
        /*public string GeneratePassword() {
            var generator = new MpPasswordGenerator(minimumLengthPassword: 8,
                                      maximumLengthPassword: 12,
                                      minimumUpperCaseChars: 2,
                                      minimumSpecialChars: 2);
            return generator.Generate();
        }*/
        public string GetCPUInfo() {
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
        
    }
}
