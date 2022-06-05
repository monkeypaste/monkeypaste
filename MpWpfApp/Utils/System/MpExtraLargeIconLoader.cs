using MonkeyPaste;
using MonkeyPaste.Common.Wpf;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public struct SHFILEINFO {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 254)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szTypeName;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        public int X;
        public int Y;

        public POINT(int x, int y) {
            this.X = x;
            this.Y = y;
        }

        public POINT(System.Drawing.Point pt) : this(pt.X, pt.Y) { }

        public static implicit operator System.Drawing.Point(POINT p) {
            return new System.Drawing.Point(p.X, p.Y);
        }

        public static implicit operator POINT(System.Drawing.Point p) {
            return new POINT(p.X, p.Y);
        }
    }

    public struct IMAGELISTDRAWPARAMS {
        public int cbSize;
        public IntPtr himl;
        public int i;
        public IntPtr hdcDst;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public int xBitmap;
        public int yBitmap;
        public int rgbBk;
        public int rgbFg;
        public int fStyle;
        public int dwRop;
        public int fState;
        public int Frame;
        public int crEffect;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGEINFO {
        public IntPtr hbmImage;
        public IntPtr hbmMask;
        public int Unused1;
        public int Unused2;
        public RECT rcImage;
    }

    [ComImportAttribute]
    [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IImageList {
        [PreserveSig]
        int Add(
            IntPtr hbmImage,
            IntPtr hbmMask,
            ref int pi);

        [PreserveSig]
        int ReplaceIcon(
            int i,
            IntPtr hicon,
            ref int pi);

        [PreserveSig]
        int SetOverlayImage(
            int iImage,
            int iOverlay);

        [PreserveSig]
        int Replace(
            int i,
            IntPtr hbmImage,
            IntPtr hbmMask);

        [PreserveSig]
        int AddMasked(
            IntPtr hbmImage,
            int crMask,
            ref int pi);

        [PreserveSig]
        int Draw(
            ref IMAGELISTDRAWPARAMS pimldp);

        [PreserveSig]
        int Remove(
            int i);

        [PreserveSig]
        int GetIcon(
            int i,
            int flags,
            ref IntPtr picon);
    };


    public class MpShellEx {
        private const int WM_CLOSE = 0x0010;


        [DllImport("user32")]
        private static extern
            IntPtr SendMessage(
            IntPtr handle,
            int Msg,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("shell32.dll")]
        private static extern int SHGetImageList(
            int iImageList,
            ref Guid riid,
            out IImageList ppv);

        [DllImport("Shell32.dll")]
        public static extern int SHGetFileInfo(
            string pszPath,
            int dwFileAttributes,
            ref SHFILEINFO psfi,
            int cbFileInfo,
            uint uFlags);

        [DllImport("user32")]
        public static extern int DestroyIcon(
            IntPtr hIcon);

        public static BitmapSource GetBitmapFromFolderPath(string filepath, MpIconSize iconsize) {
            IntPtr hIcon = GetIconHandleFromFolderPath(filepath, iconsize);
            return GetBitmapFromIconHandle(hIcon);
        }

        public static BitmapSource GetBitmapFromFilePath(string filepath, MpIconSize iconsize) {
            IntPtr hIcon = GetIconHandleFromFilePath(filepath, iconsize);
            return GetBitmapFromIconHandle(hIcon);
        }

        public static BitmapSource GetBitmapFromPath(string filepath, MpIconSize iconsize) {
            IntPtr hIcon = IntPtr.Zero;
            if (Directory.Exists(filepath)) {
                hIcon = GetIconHandleFromFolderPath(filepath, iconsize);
            } else {
                if (File.Exists(filepath)) {
                    hIcon = GetIconHandleFromFilePath(filepath, iconsize);
                }
            }
            return GetBitmapFromIconHandle(hIcon);
        }

        private static BitmapSource GetBitmapFromIconHandle(IntPtr hIcon) {
            if (hIcon == IntPtr.Zero) {
                return null;
            }
            using (var myIcon = System.Drawing.Icon.FromHandle(hIcon)) {
                using (var bitmap = myIcon.ToBitmap()) {
                    myIcon.Dispose();
                    DestroyIcon(hIcon);
                    SendMessage(hIcon, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    return bitmap.ToBitmapSource();                    
                }                    
            }
        }

        private static IntPtr GetIconHandleFromFilePath(string filepath, MpIconSize iconsize) {
            var shinfo = new SHFILEINFO();
            const uint SHGFI_SYSICONINDEX = 0x4000;
            const int FILE_ATTRIBUTE_NORMAL = 0x80;
            uint flags = SHGFI_SYSICONINDEX;
            return GetIconHandleFromFilePathWithFlags(filepath, iconsize, ref shinfo, FILE_ATTRIBUTE_NORMAL, flags);
        }

        private static IntPtr GetIconHandleFromFolderPath(string folderpath, MpIconSize iconsize) {
            var shinfo = new SHFILEINFO();

            const uint SHGFI_ICON = 0x000000100;
            const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
            const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            return GetIconHandleFromFilePathWithFlags(folderpath, iconsize, ref shinfo, FILE_ATTRIBUTE_DIRECTORY, flags);
        }

        private static IntPtr GetIconHandleFromFilePathWithFlags(
            string filepath,
            MpIconSize iconsize,
            ref SHFILEINFO shinfo,
            int fileAttributeFlag,
            uint flags) {
            const int ILD_TRANSPARENT = 1;
            var retval = SHGetFileInfo(filepath, fileAttributeFlag, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (retval == 0) {
                // This occurs from a COM exception likely from the AddTileThread so in this case just return the app icon handle
                return IntPtr.Zero;
            }
            var iconIndex = shinfo.iIcon;
            var iImageListGuid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            var hres = SHGetImageList((int)iconsize, ref iImageListGuid, out IImageList iml);
            var hIcon = IntPtr.Zero;
            hres = iml.GetIcon(iconIndex, ILD_TRANSPARENT, ref hIcon);
            return hIcon;
        }
    }
}
