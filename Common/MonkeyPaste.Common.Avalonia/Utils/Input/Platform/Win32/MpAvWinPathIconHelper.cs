using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;

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
        public static string GetIconBase64FromHandle(nint hwnd) {
            IntPtr iconHandle = SendMessage(hwnd, WM_GETICON, ICON_SMALL2, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd, WM_GETICON, ICON_SMALL, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = SendMessage(hwnd, WM_GETICON, ICON_BIG, 0);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, GCL_HICON);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, GCL_HICONSM);

            if (iconHandle == IntPtr.Zero)
                return MpBase64Images.QuestionMark;
            return GetBitmapFromIconHandle(iconHandle).ToBase64String();
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

#pragma warning disable CA1416 // Validate platform compatibility
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
#pragma warning restore CA1416 // Validate platform compatibility

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
            try {
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
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error retrieving icon handle from path '{filepath}'.", ex);
                return IntPtr.Zero;
            }
#else
            return IntPtr.Zero;
#endif
        }
        #endregion


        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;

        public const int WM_GETICON = 0x7F;

        public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex) {
            if (IntPtr.Size > 4)
                return GetClassLongPtr64(hWnd, nIndex);
            else
                return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
        }

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    }
}
