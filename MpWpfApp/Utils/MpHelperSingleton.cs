using ExtractLargeIconFromFile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static ExtractLargeIconFromFile.ShellEx;

namespace MpWpfApp {
    public class MpHelperSingleton {
        private static readonly Lazy<MpHelperSingleton> lazy = new Lazy<MpHelperSingleton>(() => new MpHelperSingleton());
        public static MpHelperSingleton Instance { get { return lazy.Value; } }
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
        public string GetProcessPath(IntPtr hwnd) {
            uint pid = 0;
            WinApi.GetWindowThreadProcessId(hwnd, out pid);
            //return MpHelperSingleton.Instance.GetMainModuleFilepath((int)pid);
            Process proc = Process.GetProcessById((int)pid);
            return proc.MainModule.FileName.ToString();
        }
        public System.Drawing.Image GetIconImage(IntPtr sourceHandle) {
            return GetBitmapFromFilePath(MpHelperSingleton.Instance.GetProcessPath(sourceHandle), IconSizeEnum.ExtraLargeIcon);
            //return IconReader.GetFileIcon(MpHelperSingleton.Instance.GetProcessPath(sourceHandle),IconReader.IconSize.Large,false).ToBitmap();
        }
        public System.Drawing.Image GetIconImage(string sourcePath) {
            return GetBitmapFromFilePath(sourcePath, IconSizeEnum.ExtraLargeIcon);
            //return IconReader.GetFileIcon(MpHelperSingleton.Instance.GetProcessPath(sourceHandle),IconReader.IconSize.Large,false).ToBitmap();
        }
        public byte[] ConvertImageToByteArray(Image img) {
            MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }
        public Image ConvertByteArrayToImage(byte[] rawBytes) {
            return Image.FromStream(new MemoryStream(rawBytes), true);
        }
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
        public long FileListSize(string[] paths) {
            long total = 0;
            foreach(string path in paths) {
                if(Directory.Exists(path)) {
                    total += CalcDirSize(path, true);
                } else if(File.Exists(path)) {
                    total += (new FileInfo(path)).Length;
                }
            }
            return total;
        }
        private long CalcDirSize(string sourceDir, bool recurse = true) {
            return _CalcDirSize(new DirectoryInfo(sourceDir), recurse);
        }
        private long _CalcDirSize(DirectoryInfo di, bool recurse = true) {
            long size = 0;
            FileInfo[] fiEntries = di.GetFiles();
            foreach(var fiEntry in fiEntries) {
                Interlocked.Add(ref size, fiEntry.Length);
            }

            if(recurse) {
                DirectoryInfo[] diEntries = di.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
                System.Threading.Tasks.Parallel.For<long>(0, diEntries.Length, () => 0, (i, loop, subtotal) => {
                    if((diEntries[i].Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) return 0;
                    subtotal += _CalcDirSize(diEntries[i], true);
                    return subtotal;
                },
                    (x) => Interlocked.Add(ref size, x)
                );

            }
            return size;
        }
        public Color GetRandomColor(int alpha = 255) {
            Random Rand = new Random(Convert.ToInt32(DateTime.Now.Second));
            if(alpha == 255) {
                return Color.FromArgb(Rand.Next(256), Rand.Next(256), Rand.Next(256));
            }
            return Color.FromArgb(alpha, Rand.Next(256), Rand.Next(256), Rand.Next(256));
        }
    }
}
