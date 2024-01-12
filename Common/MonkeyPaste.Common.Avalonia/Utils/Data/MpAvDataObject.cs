using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin;


#if WINDOWS
using MonkeyPaste.Common.Wpf;
using static MonkeyPaste.Common.Avalonia.MpAvDataObjectPInvokes;

#endif

namespace MonkeyPaste.Common.Avalonia {

    public class MpAvDataObject : MpPortableDataObject, IDataObject {
        #region Statics       

        public static MpAvDataObject Parse(string json) {
            var mpdo = new MpAvDataObject();
            var req_lookup = MpJsonExtensions.DeserializeObject<Dictionary<string, object>>(json);
            foreach (var kvp in req_lookup) {
                try {
                    mpdo.SetData(kvp.Key, kvp.Value);
                }
                catch (MpUnregisteredDataFormatException udfe) {
                    // ignore
                    MpConsole.WriteTraceLine($"Ignoring data object format '{kvp.Key}' parsed in json: ", udfe);
                    continue;
                }
            }
            return mpdo;
        }
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
            if (format == MpPortableDataFormats.Files) {
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
            } else if ((format == MpPortableDataFormats.Xhtml || format == MpPortableDataFormats.Rtf) && data is string portableDecodedFormattedTextStr) {
                // avalona like rtf and html to be stored as bytes
                data = portableDecodedFormattedTextStr.ToBytesFromString();
            } else if (format == MpPortableDataFormats.Html && data is byte[] html_bytes) {
                data = html_bytes.ToDecodedString();
            } else if (format == MpPortableDataFormats.Image && data is string png64) {
                data = png64.ToBytesFromBase64String();
            }
            base.SetData(format, data);
        }
        //public override bool TryGetData<T>(string format, out T data) {
        //    if (GetData(format) is IEnumerable<IStorageItem> sil) {
        //        if (typeof(T) == typeof(IEnumerable<string>)) {
        //            data = sil.Select(x => x.TryGetLocalPath()) as T;
        //            return true;
        //        }
        //        if (typeof(T) == typeof(string)) {
        //            data = (T)(object)string.Join(Environment.NewLine, sil.Select(x => x.TryGetLocalPath()).Where(x => !string.IsNullOrEmpty(x)));
        //            return true;
        //        }
        //        if (typeof(T) == typeof(IEnumerable<IStorageItem>)) {
        //            data = (T)(object)sil;
        //            return true;
        //        }

        //    }
        //    return base.TryGetData(format, out data);
        //}

        public async Task MapAllPseudoFormatsAsync() {
            if (ContainsData(MpPortableDataFormats.Xhtml) &&
                !ContainsData(MpPortableDataFormats.Html) &&
                GetData(MpPortableDataFormats.Xhtml) is byte[] html_bytes) {
                // convert html bytes to string and map to cef html
                string htmlStr = html_bytes.ToDecodedString();
                SetData(MpPortableDataFormats.Html, htmlStr);
            }
            if (ContainsData(MpPortableDataFormats.Html) &&
                !ContainsData(MpPortableDataFormats.Xhtml) &&
                GetData(MpPortableDataFormats.Html) is string cef_html_str) {
                // convert html sring to to bytes
                byte[] htmlBytes = cef_html_str.ToBytesFromString();
                SetData(MpPortableDataFormats.Xhtml, htmlBytes);
            }

            if (ContainsData(MpPortableDataFormats.Text) &&
                !ContainsData(MpPortableDataFormats.MimeText)) {
                // ensure cef style text is in formats
                SetData(MpPortableDataFormats.MimeText, GetData(MpPortableDataFormats.Text));
            }
            if (ContainsData(MpPortableDataFormats.MimeText) &&
                !ContainsData(MpPortableDataFormats.Text)) {
                // ensure avalonia style text is in formats
                SetData(MpPortableDataFormats.Text, GetData(MpPortableDataFormats.MimeText));
            }

            if (OperatingSystem.IsLinux()) {
                // TODO this should only be for gnome based linux

                if (ContainsData(MpPortableDataFormats.Files) &&
                    !ContainsData(MpPortableDataFormats.LinuxGnomeFiles) &&
                    GetData(MpPortableDataFormats.Files) is IEnumerable<string> files &&
                    string.Join(Environment.NewLine, files) is string av_files_str) {
                    // ensure cef style text is in formats
                    SetData(MpPortableDataFormats.LinuxGnomeFiles, av_files_str);
                }
                if (ContainsData(MpPortableDataFormats.LinuxGnomeFiles) &&
                    !ContainsData(MpPortableDataFormats.Files) &&
                    GetData(MpPortableDataFormats.LinuxGnomeFiles) is string gn_files_str &&
                    gn_files_str.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) is IEnumerable<string> gn_files
                    ) {
                    // ensure avalonia style text is in formats
                    SetData(MpPortableDataFormats.Files, gn_files);
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

            if (this.TryGetData(MpPortableDataFormats.Files, out object fn_obj)) {
                IEnumerable<string> fpl = null;
                if (fn_obj is IEnumerable<string>) {
                    fpl = fn_obj as IEnumerable<string>;
                } else if (fn_obj is string fpl_str) {
                    fpl = fpl_str.SplitNoEmpty(Environment.NewLine);
                } else {

                }
                if (fpl != null) {

                    var av_fpl = await fpl.ToAvFilesObjectAsync();
                    SetData(MpPortableDataFormats.Files, av_fpl);
                }
            }
            // TODO should add unicode, oem, etc. here for greater compatibility
            await Task.Delay(1);
        }


        // private  uint CF_BITMAP = 0;
#pragma warning disable CA1416 // Validate platform compatibility
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
#pragma warning restore CA1416 // Validate platform compatibility



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
