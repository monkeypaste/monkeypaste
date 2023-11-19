using Avalonia.Input;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#if WINDOWS
using MonkeyPaste.Common.Wpf;
using static MonkeyPaste.Common.Avalonia.MpAvDataObjectPInvokes;

#endif

namespace MonkeyPaste.Common.Avalonia {

    public class MpAvDataObject : MpPortableDataObject, IDataObject {
        #region Statics       

        #endregion
        public MpAvDataObject() : base() { }
        public MpAvDataObject(string format, object data) : base(format, data) { }
        public MpAvDataObject(Dictionary<string, object> items) : base(items) { }
        public MpAvDataObject(MpPortableDataObject mpdo) :
            this(mpdo.DataFormatLookup.ToDictionary(x => x.Key.Name, x => x.Value)) { }

        public override void SetData(string format, object data) {
            // NOTE this wrapper just ensures formats are saved properly 
            // mapping is done after obj created so nothing is overwritten if it was populated
            // this ensures: 
            // 1. 'FileNames' is list of strings
            // 2. 'HTML Format' is stored as byte[] and 'text/html' 
            // 3. 'text/html' is stored as string

            if (data == null) {
                if (ContainsData(format)) {
                    // this a workaround to remove formats through interface in writedndobject
                    base.SetData(format, null);
                }
                // will cause error for sometypes
                return;
            }
            if (format == MpPortableDataFormats.AvFiles) {
                if (data is string portablePathStr) {
                    // convert portable single line-separated string to enumerable of strings for avalonia
                    data = portablePathStr.SplitNoEmpty(Environment.NewLine);
                    //if (portablePathStr.SplitNoEmpty(Environment.NewLine) is string[] fpl &&
                    //    fpl.Any() && Uri.IsWellFormedUriString(fpl.First(), UriKind.Absolute)) {
                    //    // path uri from cef so convert to local path
                    //    data = fpl
                    //        .Where(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
                    //        .Select(x => new Uri(x, UriKind.Absolute).LocalPath)
                    //        .ToArray();
                    //}

                }
            } else if ((format == MpPortableDataFormats.AvHtml_bytes || format == MpPortableDataFormats.AvRtf_bytes) && data is string portableDecodedFormattedTextStr) {
                // avalona like rtf and html to be stored as bytes
                data = portableDecodedFormattedTextStr.ToBytesFromString();
            } else if (format == MpPortableDataFormats.CefHtml && data is byte[] html_bytes) {
                data = html_bytes.ToDecodedString();
            } else if (format == MpPortableDataFormats.AvPNG && data is string png64) {
                data = png64.ToBytesFromBase64String();
            }
            base.SetData(format, data);
        }
        public override bool TryGetData<T>(string format, out T data) {
            if (GetData(format) is IEnumerable<IStorageItem> sil) {
                if (typeof(T) == typeof(IEnumerable<string>)) {
                    data = sil.Select(x => x.TryGetLocalPath()) as T;
                    return true;
                }
                if (typeof(T) == typeof(string)) {
                    data = (T)(object)string.Join(Environment.NewLine, sil.Select(x => x.TryGetLocalPath()).Where(x => !string.IsNullOrEmpty(x)));
                    return true;
                }
                if (typeof(T) == typeof(IEnumerable<IStorageItem>)) {
                    data = (T)(object)sil;
                    return true;
                }

            }
            return base.TryGetData(format, out data);
        }

        public async Task MapAllPseudoFormatsAsync() {
            if (ContainsData(MpPortableDataFormats.AvHtml_bytes) &&
                !ContainsData(MpPortableDataFormats.CefHtml) &&
                GetData(MpPortableDataFormats.AvHtml_bytes) is byte[] html_bytes) {
                // convert html bytes to string and map to cef html
                string htmlStr = html_bytes.ToDecodedString();
                SetData(MpPortableDataFormats.CefHtml, htmlStr);
            }
            if (ContainsData(MpPortableDataFormats.CefHtml) &&
                !ContainsData(MpPortableDataFormats.AvHtml_bytes) &&
                GetData(MpPortableDataFormats.CefHtml) is string cef_html_str) {
                // convert html sring to to bytes
                byte[] htmlBytes = cef_html_str.ToBytesFromString();
                SetData(MpPortableDataFormats.AvHtml_bytes, htmlBytes);
            }

            if (ContainsData(MpPortableDataFormats.Text) &&
                !ContainsData(MpPortableDataFormats.CefText)) {
                // ensure cef style text is in formats
                SetData(MpPortableDataFormats.CefText, GetData(MpPortableDataFormats.Text));
            }
            if (ContainsData(MpPortableDataFormats.CefText) &&
                !ContainsData(MpPortableDataFormats.Text)) {
                // ensure avalonia style text is in formats
                SetData(MpPortableDataFormats.Text, GetData(MpPortableDataFormats.CefText));
            }

            if (OperatingSystem.IsLinux()) {
                // TODO this should only be for gnome based linux

                if (ContainsData(MpPortableDataFormats.AvFiles) &&
                    !ContainsData(MpPortableDataFormats.LinuxGnomeFiles) &&
                    GetData(MpPortableDataFormats.AvFiles) is IEnumerable<string> files &&
                    string.Join(Environment.NewLine, files) is string av_files_str) {
                    // ensure cef style text is in formats
                    SetData(MpPortableDataFormats.LinuxGnomeFiles, av_files_str);
                }
                if (ContainsData(MpPortableDataFormats.LinuxGnomeFiles) &&
                    !ContainsData(MpPortableDataFormats.AvFiles) &&
                    GetData(MpPortableDataFormats.LinuxGnomeFiles) is string gn_files_str &&
                    gn_files_str.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) is IEnumerable<string> gn_files
                    ) {
                    // ensure avalonia style text is in formats
                    SetData(MpPortableDataFormats.AvFiles, gn_files);
                }
            } else if (OperatingSystem.IsWindows()) {
                //if (ContainsData(MpPortableDataFormats.AvPNG) &&
                //    GetData(MpPortableDataFormats.AvPNG) is string png64) {
                //    //SetData(MpPortableDataFormats.AvPNG, png64.ToBytesFromBase64String());
                //}
                //if (ContainsData(MpPortableDataFormats.AvPNG) &&
                //    GetData(MpPortableDataFormats.AvPNG) is byte[] pngBytes) {
                //    //#if WINDOWS
                //    //                    //SetData(MpPortableDataFormats.WinBitmap, pngBytes);
                //    //                    //SetData(MpPortableDataFormats.WinDib, pngBytes);
                //    //                    SetBitmap(pngBytes);
                //    //#endif

                //}

                // TODO should pass req formats into this and only create rtf if contianed
                //if (ContainsData(MpPortableDataFormats.CefHtml) &&
                //    !ContainsData(MpPortableDataFormats.AvRtf_bytes) &&
                //    GetData(MpPortableDataFormats.CefHtml) is string htmlStr) {
                //    // TODO should check if content is csv here (or in another if?) and create rtf table 
                //    string rtf = htmlStr.ToRtfFromRichHtml();
                //    SetData(MpPortableDataFormats.AvRtf_bytes, rtf.ToBytesFromString());
                //}
            }

            if (TryGetData(MpPortableDataFormats.AvFiles, out object fn_obj)) {
                IEnumerable<string> fpl = null;
                if (fn_obj is IEnumerable<string>) {
                    fpl = fn_obj as IEnumerable<string>;
                } else if (fn_obj is string fpl_str) {
                    fpl = fpl_str.SplitNoEmpty(Environment.NewLine);
                } else {

                }
                if (fpl != null) {

                    var av_fpl = await fpl.ToAvFilesObjectAsync();
                    SetData(MpPortableDataFormats.AvFiles, av_fpl);
                }
            }
            // TODO should add unicode, oem, etc. here for greater compatibility
            await Task.Delay(1);
        }


        // private  uint CF_BITMAP = 0;
        public void SetBitmap(byte[] bytes) {
            if (!OperatingSystem.IsWindows()) {
                return;
            }
#if WINDOWS
            //if(CF_BITMAP == 0) {
            //    CF_BITMAP = WinApi.RegisterClipboardFormatA("Bitmap");
            //}

            //if (bitmap == null)
            //    throw new ArgumentNullException(nameof(bitmap));

            //// Convert from Avalonia Bitmap to System Bitmap
            //var memoryStream = new MemoryStream(1000000);
            //bitmap.Save(memoryStream); // this returns a png from Skia (we could save/load it from the system bitmap to convert it to a bmp first, but this seems to work well already)

            var systemBitmap = new System.Drawing.Bitmap(new MemoryStream(bytes));
            //var systemBitmap = MpWpfClipoardImageHelper.GetSysDrawingBitmap(bytes);

            var hBitmap = systemBitmap.GetHbitmap();

            var screenDC = GetDC(IntPtr.Zero);

            var sourceDC = CreateCompatibleDC(screenDC);
            var sourceBitmapSelection = SelectObject(sourceDC, hBitmap);

            var destDC = CreateCompatibleDC(screenDC);
            var compatibleBitmap = CreateCompatibleBitmap(screenDC, systemBitmap.Width, systemBitmap.Height);

            var destinationBitmapSelection = SelectObject(destDC, compatibleBitmap);

            BitBlt(
                destDC,
                0,
                0,
                systemBitmap.Width,
                systemBitmap.Height,
                sourceDC,
                0,
                0,
                0x00CC0020); // SRCCOPY

            try {
                //WinApi.OpenClipboard(IntPtr.Zero);
                //WinApi.EmptyClipboard();

                //IntPtr result = SetClipboardData(CF_BITMAP, compatibleBitmap);

                //if (result == IntPtr.Zero) {
                //    int errno = Marshal.GetLastWin32Error();
                //}

                SetData(MpPortableDataFormats.WinBitmap, compatibleBitmap);
            }
            catch (Exception) {

            }
            finally {
                WinApi.CloseClipboard();
            }
#endif
        }



        #region Avalonia.Input.IDataObject Implementation

        IEnumerable<string> IDataObject.GetDataFormats() {
            return DataFormatLookup.Select(x => x.Key.Name);
        }

        bool IDataObject.Contains(string dataFormat) {
            return ContainsData(dataFormat);
        }

        //string IDataObject.GetText() {
        //    return GetData(MpPortableDataFormats.Text) as string;
        //}

        //IEnumerable<string> IDataObject.GetFileNames() {

        //    if (GetData(MpPortableDataFormats.AvFileNames) is IEnumerable<string> files) {
        //        return files;
        //    }

        //    return null;
        //}

        object IDataObject.Get(string dataFormat) {
            return GetData(dataFormat);
        }

        #endregion

    }

    internal static partial class MpAvDataObjectPInvokes {
        [LibraryImport("user32.dll")]
        public static partial IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [LibraryImport("user32.dll")]
        public static partial IntPtr GetDC(IntPtr hWnd);


        [LibraryImport("gdi32.dll")]
        public static partial IntPtr CreateCompatibleDC(IntPtr hDC);


        [LibraryImport("gdi32.dll")]//, ExactSpelling = true)]
        public static partial IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        public static partial IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [LibraryImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool BitBlt(
            IntPtr hdc,
            int x,
            int y,
            int cx,
            int cy,
            IntPtr hdcSrc,
            int x1,
            int y1,
            uint rop);
    }
}
