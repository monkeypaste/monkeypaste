using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using static MpProcessHelper.WinApi;
using MonkeyPaste;
using System.Collections.Generic;

namespace MpProcessHelper {
    public class MpProcessIconBuilder : MpIProcessIconBuilder {
        private static MpIconBuilderBase _iconBuilder;

        public MpIconBuilderBase IconBuilder { get => _iconBuilder; set => _iconBuilder = value; }

        public MpProcessIconBuilder(MpIconBuilderBase ib) {
            _iconBuilder = ib;
        }
        public string GetBase64BitmapFromFolderPath(string filepath) {
            return GetBitmapFromFolderPath(filepath, IconSizeEnum.MediumIcon32);
        }

        public string GetBase64BitmapFromFilePath(string filepath) {
            return GetBitmapFromFilePath(filepath, IconSizeEnum.MediumIcon32);
        }

        public string GetBase64BitmapFromPath(string fileOrFolderpath) {
            if(Directory.Exists(fileOrFolderpath)) {
                return GetBase64BitmapFromFolderPath(fileOrFolderpath);
            }
            return GetBitmapFromFilePath(fileOrFolderpath, IconSizeEnum.MediumIcon32);
        }


        private static string GetBitmapFromFolderPath(string filepath, IconSizeEnum iconsize) {
            IntPtr hIcon = GetIconHandleFromFolderPath(filepath, iconsize);
            return GetIconBase64FromHandle(hIcon);
        }

        private static string GetBitmapFromFilePath(string filepath, IconSizeEnum iconsize) {
            IntPtr hIcon = GetIconHandleFromFilePath(filepath, iconsize);
            return GetIconBase64FromHandle(hIcon);
        }

        private static string GetBitmapFromPath(string filepath, IconSizeEnum iconsize) {
            IntPtr hIcon = IntPtr.Zero;
            if (Directory.Exists(filepath)) {
                hIcon = GetIconHandleFromFolderPath(filepath, iconsize);
            } else {
                if (File.Exists(filepath)) {
                    hIcon = GetIconHandleFromFilePath(filepath, iconsize);
                }
            }
            return GetIconBase64FromHandle(hIcon);
        }

        private static string GetIconBase64FromHandle(IntPtr hIcon) {
            if (hIcon == IntPtr.Zero) {
                throw new System.IO.FileNotFoundException();
            }
            using (var myIcon = System.Drawing.Icon.FromHandle(hIcon)) {
                using (var img = myIcon.ToBitmap()) {
                    myIcon.Dispose();
                    DestroyIcon(hIcon);
                    SendMessage(hIcon, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                    using (MemoryStream memoryStream = new MemoryStream()) {
                        img.Save(memoryStream, ImageFormat.Bmp);
                        byte[] imageBytes = memoryStream.ToArray();
                        return Convert.ToBase64String(imageBytes);
                    }
                }                    
            }
        }

        private static IntPtr GetIconHandleFromFilePath(string filepath, IconSizeEnum iconsize) {
            var shinfo = new WinApi.SHFILEINFO();
            const uint SHGFI_SYSICONINDEX = 0x4000;
            const int FILE_ATTRIBUTE_NORMAL = 0x80;
            uint flags = SHGFI_SYSICONINDEX;
            return GetIconHandleFromFilePathWithFlags(filepath, iconsize, ref shinfo, FILE_ATTRIBUTE_NORMAL, flags);
        }

        private static IntPtr GetIconHandleFromFolderPath(string folderpath, IconSizeEnum iconsize) {
            var shinfo = new WinApi.SHFILEINFO();

            const uint SHGFI_ICON = 0x000000100;
            const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
            const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            return GetIconHandleFromFilePathWithFlags(folderpath, iconsize, ref shinfo, FILE_ATTRIBUTE_DIRECTORY, flags);
        }

        private static IntPtr GetIconHandleFromFilePathWithFlags(
            string filepath,
            IconSizeEnum iconsize,
            ref SHFILEINFO shinfo,
            int fileAttributeFlag,
            uint flags) {
            const int ILD_TRANSPARENT = 1;
            var retval = SHGetFileInfo(filepath, fileAttributeFlag, ref shinfo, Marshal.SizeOf(shinfo), flags);
            if (retval == 0) {
                // This occurs from a COM exception likely from the AddTileThread so in this case just return the app icon handle
                //return MpClipboardManager.Instance.LastWindowWatcher.ThisAppHandle;
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
