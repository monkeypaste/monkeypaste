using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Runtime.InteropServices;

#if WINDOWS
using MonkeyPaste.Common.Wpf;
#endif

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvWinPathIconHelper {
        #region Public Methods
        public static string GetIconBase64FromWindowsPath(string path, int iconSize) {
            var bmp = GetBitmapFromPath(path, iconSize);
            return bmp.ToBase64String();
        }

        #endregion

        #region Private Methods

        private static Bitmap GetBitmapFromPath(string path, int iconsize) {
            IntPtr hIcon = IntPtr.Zero;
            if (Directory.Exists(path)) {
                hIcon = GetIconHandleFromFolderPath(path, iconsize);
            } else {
                if (File.Exists(path)) {
                    hIcon = GetIconHandleFromFilePath(path, iconsize);
                }
            }
            return GetBitmapFromIconHandle(hIcon);
        }

        private static Bitmap GetBitmapFromIconHandle(IntPtr hIcon) {
            if (hIcon == IntPtr.Zero || !OperatingSystem.IsWindows()) {
                return null;
            }
#if WINDOWS

            using (var myIcon = System.Drawing.Icon.FromHandle(hIcon)) {
                using (var bitmap = myIcon.ToBitmap()) {
                    myIcon.Dispose();
                    WinApi.DestroyIcon(hIcon);
                    int WM_CLOSE = 0x0010;
                    WinApi.SendMessage(hIcon, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                    return bitmap.ToAvBitmap();
                }
            }
#else
            return null;
#endif
        }

        private static IntPtr GetIconHandleFromFilePath(string filepath, int iconsize) {
            const uint SHGFI_SYSICONINDEX = 0x4000;
            const int FILE_ATTRIBUTE_NORMAL = 0x80;
            uint flags = SHGFI_SYSICONINDEX;
            return GetIconHandleFromFilePathWithFlags(filepath, iconsize, FILE_ATTRIBUTE_NORMAL, flags);
        }

        private static IntPtr GetIconHandleFromFolderPath(string folderpath, int iconsize) {
            const uint SHGFI_ICON = 0x000000100;
            const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
            const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            return GetIconHandleFromFilePathWithFlags(folderpath, iconsize, FILE_ATTRIBUTE_DIRECTORY, flags);
        }

        private static IntPtr GetIconHandleFromFilePathWithFlags(
            string filepath,
            int iconsize,
            int fileAttributeFlag,
            uint flags) {
#if WINDOWS
            const int ILD_TRANSPARENT = 1;
            var shinfo = new WinApi.SHFILEINFO();
            var retval = WinApi.SHGetFileInfo(filepath, fileAttributeFlag, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (retval == 0) {
                // This occurs from a COM exception likely from the AddTileThread so in this case just return the app icon handle
                return IntPtr.Zero;
            }
            var iconIndex = shinfo.iIcon;
            var iImageListGuid = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

            _ = WinApi.SHGetImageList((int)iconsize, ref iImageListGuid, out WinApi.IImageList iml);
            var hIcon = IntPtr.Zero;
            _ = iml.GetIcon(iconIndex, ILD_TRANSPARENT, ref hIcon);
            return hIcon;
#else
            return IntPtr.Zero;
#endif
        }
        #endregion
    }
}
