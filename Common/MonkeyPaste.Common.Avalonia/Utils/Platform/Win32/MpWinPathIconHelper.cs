using System;
using Avalonia.Media.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using MonoMac.AppKit;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpWinPathIconHelper {
        #region Public Methods
        public static string GetIconBase64FromWindowsPath(string path, int iconSize) {
            var bmp = GetBitmapFromPath(path, iconSize);
            return bmp.ToBase64String();
        }

        #endregion

        #region Private Methods

        private static Bitmap GetBitmapFromFolderPath(string filepath, int iconsize) {
            IntPtr hIcon = GetIconHandleFromFolderPath(filepath, iconsize);
            return GetBitmapFromIconHandle(hIcon);
        }

        private static Bitmap GetBitmapFromFilePath(string filepath, int iconsize) {
            IntPtr hIcon = GetIconHandleFromFilePath(filepath, iconsize);
            return GetBitmapFromIconHandle(hIcon);
        }

        private static Bitmap GetBitmapFromPath(string filepath, int iconsize) {
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

        private static Bitmap GetBitmapFromIconHandle(IntPtr hIcon) {
            if (hIcon == IntPtr.Zero) {
                return null;
            }
            using (var myIcon = System.Drawing.Icon.FromHandle(hIcon)) {
                using (var bitmap = myIcon.ToBitmap()) {
                    myIcon.Dispose();
                    WinApi.DestroyIcon(hIcon);
                    WinApi.SendMessage(hIcon, WinApi.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                    return bitmap.ToAvBitmap();
                }
            }
        }

        private static IntPtr GetIconHandleFromFilePath(string filepath, int iconsize) {
            var shinfo = new WinApi.SHFILEINFO();
            const uint SHGFI_SYSICONINDEX = 0x4000;
            const int FILE_ATTRIBUTE_NORMAL = 0x80;
            uint flags = SHGFI_SYSICONINDEX;
            return GetIconHandleFromFilePathWithFlags(filepath, iconsize, ref shinfo, FILE_ATTRIBUTE_NORMAL, flags);
        }

        private static IntPtr GetIconHandleFromFolderPath(string folderpath, int iconsize) {
            var shinfo = new WinApi.SHFILEINFO();

            const uint SHGFI_ICON = 0x000000100;
            const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
            const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            return GetIconHandleFromFilePathWithFlags(folderpath, iconsize, ref shinfo, FILE_ATTRIBUTE_DIRECTORY, flags);
        }

        private static IntPtr GetIconHandleFromFilePathWithFlags(
            string filepath,
            int iconsize,
            ref WinApi.SHFILEINFO shinfo,
            int fileAttributeFlag,
            uint flags) {
            const int ILD_TRANSPARENT = 1;
            var retval = WinApi.SHGetFileInfo(filepath, fileAttributeFlag, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (retval == 0) {
                // This occurs from a COM exception likely from the AddTileThread so in this case just return the app icon handle
                return IntPtr.Zero;
            }
            var iconIndex = shinfo.iIcon;
            var iImageListGuid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            int hres = WinApi.SHGetImageList((int)iconsize, ref iImageListGuid, out WinApi.IImageList iml);
            var hIcon = IntPtr.Zero;
            hres = iml.GetIcon(iconIndex, ILD_TRANSPARENT, ref hIcon);
            return hIcon;
        }
        #endregion
    }
}
