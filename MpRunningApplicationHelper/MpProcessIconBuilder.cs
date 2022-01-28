using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using static MpProcessHelper.WinApi;
using MonkeyPaste;
using System.Collections.Generic;
using System.Drawing;

namespace MpProcessHelper {
    public static class MpProcessIconBuilder {


        public static string GetBase64BitmapFromPath(string fileOrFolderpath, IconSizeEnum iconSize = IconSizeEnum.MediumIcon32) {
            IntPtr hIcon = IntPtr.Zero;
            if (Directory.Exists(fileOrFolderpath)) {
                hIcon = GetIconHandleFromFolderPath(fileOrFolderpath, iconSize);
            } 
            if(File.Exists(fileOrFolderpath)) {
                hIcon = GetIconHandleFromFilePath(fileOrFolderpath, iconSize);
            }
            if(hIcon == IntPtr.Zero) {
                return MpBase64Images.Warning;
            }
            return GetIconBase64FromHandle(hIcon);
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

        private static string GetIconBase64FromHandle(IntPtr hIcon) {
            if (hIcon == IntPtr.Zero) {
                throw new System.IO.FileNotFoundException();
            }
            using (var myIcon = System.Drawing.Icon.FromHandle(hIcon)) {
                using (Bitmap b = new Bitmap(myIcon.Width, myIcon.Height)) {
                    using (Graphics g = Graphics.FromImage(b)) {
                        g.DrawIcon(SystemIcons.Information, 0, 0);

                    }
                    myIcon.Dispose();
                    DestroyIcon(hIcon);
                    SendMessage(hIcon, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                    using (MemoryStream memoryStream = new MemoryStream()) {
                        b.Save(memoryStream, ImageFormat.Bmp);
                        byte[] imageBytes = memoryStream.ToArray();
                        return Convert.ToBase64String(imageBytes);
                    }
                }
                //using (var img = myIcon.ToBitmap()) {
                //    myIcon.Dispose();
                //    DestroyIcon(hIcon);
                //    SendMessage(hIcon, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                    

                //    // note this converts image format to Format32bppArgb
                //    img.MakeTransparent(System.Drawing.Color.Black);
                //    using (MemoryStream memoryStream = new MemoryStream()) {
                //        img.Save(memoryStream, ImageFormat.Bmp);
                //        byte[] imageBytes = memoryStream.ToArray();
                //        return Convert.ToBase64String(imageBytes);
                //    }
                //}
            }
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
