using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace CoreClipboardHandler {
    public class CoreClipboardHandler : MpIClipboardPluginComponent {
        #region Private Variables

        private IntPtr _mainWindowHandle;

        private uint CF_HTML, CF_RTF, CF_CSV, CF_TEXT = 1, CF_BITMAP = 2, CF_DIB = 8, CF_HDROP = 15, CF_UNICODE_TEXT, CF_OEM_TEXT;
        #endregion
        public MpPortableDataObject HandleDataObject(MpPortableDataObject pdo) {
            if(pdo == null) {
                return null;
            }
            
            if(!CanHandleDataObject()) {
                return pdo;
            }

            foreach(var nativeTypeName in MpPortableDataFormats.Formats) {
                var data = GetClipboardData(nativeTypeName);

                if (!string.IsNullOrEmpty(data)) {
                    pdo.SetData(nativeTypeName, data);
                }
            }
            return pdo;
        }


        private string GetClipboardData(string nativeFormatStr) {
            while(IsClipboardOpen()) {
                Thread.Sleep(10);
            }
            if(nativeFormatStr == DataFormats.FileDrop &&
                WinApi.IsClipboardFormatAvailable(CF_HDROP)) {

                WinApi.OpenClipboard(_mainWindowHandle);
                string[] sa = Clipboard.GetData(nativeFormatStr) as string[];                
                if (sa != null && sa.Length > 0) {
                    return string.Join(Environment.NewLine, sa);
                }
            } else if(nativeFormatStr == DataFormats.Bitmap &&
                      WinApi.IsClipboardFormatAvailable(CF_BITMAP)) {

                WinApi.OpenClipboard(_mainWindowHandle);
                var bmpSrc = Clipboard.GetImage();
                if (bmpSrc != null) {
                    byte[] bytes = null;
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    using (MemoryStream stream = new MemoryStream()) {
                        try {
                            var bf = System.Windows.Media.Imaging.BitmapFrame.Create(bmpSrc);
                            encoder.Frames.Add(bf);
                            encoder.Save(stream);
                            bytes = stream.ToArray();
                            stream.Close();
                        }
                        catch (Exception ex) {
                            MpConsole.WriteLine("MpHelpers.ConvertBitmapSourceToByteArray exception: " + ex);
                            return null;
                        }
                    }
                    if(bytes != null) {
                        return Convert.ToBase64String(bytes);
                    }
                }
            } else {
                uint format = GetWin32FormatId(nativeFormatStr);
                if (format != 0) {
                    if (WinApi.IsClipboardFormatAvailable(format)) {
                        WinApi.OpenClipboard(_mainWindowHandle);

                        //Get pointer to clipboard data in the selected format
                        IntPtr ClipboardDataPointer = WinApi.GetClipboardData(format);

                        //Do a bunch of crap necessary to copy the data from the memory
                        //the above pointer points at to a place we can access it.
                        UIntPtr byteCount = WinApi.GlobalSize(ClipboardDataPointer);
                        IntPtr gLock = WinApi.GlobalLock(ClipboardDataPointer);
                        if (gLock == IntPtr.Zero) {
                            return string.Empty;
                        }
                        //Init a buffer which will contain the clipboard data
                        byte[] bytes = new byte[(int)byteCount];

                        //Copy clipboard data to buffer
                        Marshal.Copy(gLock, bytes, 0, (int)byteCount);

                        WinApi.GlobalUnlock(gLock); //unlock gLock

                        WinApi.CloseClipboard();

                        if (format == CF_BITMAP) {
                            Debugger.Break();
                        }
                        if (nativeFormatStr == DataFormats.FileDrop) {
                            var test = Encoding.ASCII.GetString(bytes);
                            Debugger.Break();
                        }

                        return System.Text.Encoding.UTF8.GetString(bytes);
                    }
                }
            }
            return null;
        }

        private uint GetWin32FormatId(string nativeFormatStr) {
            if (nativeFormatStr == DataFormats.Text) {
                return CF_TEXT;
            }
            if (nativeFormatStr == DataFormats.Bitmap) {
                return CF_BITMAP;
            }
            if (nativeFormatStr == DataFormats.CommaSeparatedValue) {
                return CF_CSV;
            }
            if (nativeFormatStr == DataFormats.FileDrop) {
                return CF_HDROP;
            }
            if (nativeFormatStr == DataFormats.Html) {
                return CF_HTML;
            }
            if (nativeFormatStr == DataFormats.Rtf) {
                return CF_RTF;
            }
            return 0;
        }

        private bool IsClipboardOpen() {
            var hwnd = WinApi.GetOpenClipboardWindow();
            return hwnd != IntPtr.Zero;

            //if (hwnd == IntPtr.Zero) {
            //    return "Unknown";
            //}
            //Debugger.Break();
            //var int32Handle = hwnd.ToInt32();
            //var len = GetWindowTextLength(int32Handle);
            //var sb = new StringBuilder(len);
            //GetWindowText(int32Handle, sb, len);
            //return sb.ToString();
        }

        private string GetNativeFormatName(MpClipboardFormatType portableType, string fallbackName = "") {
            switch (portableType) {
                case MpClipboardFormatType.Text:
                    return DataFormats.Text;
                case MpClipboardFormatType.Html:
                    return DataFormats.Html;
                case MpClipboardFormatType.Rtf:
                    return DataFormats.Rtf;
                case MpClipboardFormatType.Bitmap:
                    return DataFormats.Bitmap;
                case MpClipboardFormatType.FileDrop:
                    return DataFormats.FileDrop;
                case MpClipboardFormatType.Csv:
                    return DataFormats.CommaSeparatedValue;
                case MpClipboardFormatType.UnicodeText:
                    return DataFormats.UnicodeText;
                case MpClipboardFormatType.OemText:
                    return DataFormats.OemText;
                default:
                    return fallbackName;
            }
        }

        private bool CanHandleDataObject() {
            if (_mainWindowHandle == null || _mainWindowHandle == IntPtr.Zero) {
                Application.Current.Dispatcher.Invoke(() => {
                    _mainWindowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
                });

                if (_mainWindowHandle != null && _mainWindowHandle != IntPtr.Zero) {
                    CF_UNICODE_TEXT = WinApi.RegisterClipboardFormatA("UnicodeText");
                    CF_BITMAP = WinApi.RegisterClipboardFormatA("Bitmap");
                    CF_OEM_TEXT = WinApi.RegisterClipboardFormatA("OemText");
                    CF_HTML = WinApi.RegisterClipboardFormatA("HTML Format");
                    CF_RTF = WinApi.RegisterClipboardFormatA("Rich Text Format");
                    CF_CSV = WinApi.RegisterClipboardFormatA(DataFormats.CommaSeparatedValue);
                    CF_DIB = WinApi.RegisterClipboardFormatA("DeviceIndependentBitmap");
                    CF_HDROP = WinApi.RegisterClipboardFormatA(DataFormats.FileDrop);
                }
            }
            if (_mainWindowHandle == null || _mainWindowHandle == IntPtr.Zero) {
                MpConsole.WriteLine("Cannot check clipboard until main window is initalized");
                return false;
            }
            return true;
        }
    }
}
