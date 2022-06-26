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
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System.Collections.Specialized;

namespace CoreClipboardHandler {
    public class CoreClipboardHandler : 
        MpIClipboardReaderComponent,
        MpIClipboardWriterComponent,
        MpIPlatformDataObjectHelper {
        #region Private Variables

        private IntPtr _mainWindowHandle;

        private uint CF_HTML, CF_RTF, CF_CSV, CF_TEXT = 1, CF_BITMAP = 2, CF_DIB = 8, CF_HDROP = 15, CF_UNICODE_TEXT, CF_OEM_TEXT;
        #endregion

        public enum CoreClipboardParamType {
            None = 0,
            //readers
            R_MaxCharCount_Text = 1,
            R_Ignore_Text,
            R_MaxCharCount_Rtf,
            R_Ignore_Rtf,
            R_MaxCharCount_Html,
            R_Ignore_Html,
            R_BitmapWriteFormat,
            R_Ignore_Bitmap,
            R_IgnoreAll_FileDrop,
            R_IgnoredExt_FileDrop,
            R_Ignore_Csv, //11
            //writers
            W_MaxCharCount_Text,
            W_Ignore_Text,
            W_MaxCharCount_Rtf,
            W_Ignore_Rtf,
            W_MaxCharCount_Html,
            W_Ignore_Html,
            W_BitmapWriteFormat,
            W_Ignore_Bitmap,
            W_IgnoreAll_FileDrop,
            W_IgnoredExt_FileDrop,
            W_Ignore_Csv //22
        }

        #region MpIPlatformDataObjectHelper
        
        public MpPortableDataObject ConvertToSupportedPortableFormats(object nativeDataObj, int retryCount = 5) {
            throw new NotImplementedException();
        }

        public object ConvertToPlatformClipboardDataObject(MpPortableDataObject portableObj) {
            throw new NotImplementedException();
        }

        public void SetPlatformClipboard(MpPortableDataObject portableObj, bool ignoreClipboardChange) {
            throw new NotImplementedException();
        }

        public MpPortableDataObject GetPlatformClipboardDataObject() {
            throw new NotImplementedException();
        }

        #endregion

        #region MpIClipboardReaderComponent Implementation

        public MpClipboardReaderResponse ReadClipboardData(MpClipboardReaderRequest request) {
            var hasError = CanHandleDataObject();
            if (hasError != null) {
                return hasError;
            }

            var currentOutput = new MpPortableDataObject();
            
            foreach (var nativeTypeName in request.readFormats) {
                var data = GetClipboardData(nativeTypeName,request);

                if (!string.IsNullOrEmpty(data)) {
                    currentOutput.SetData(nativeTypeName, data);
                }
            }
            return new MpClipboardReaderResponse() {
                dataObject = currentOutput
            };
        }

        private string GetClipboardData(string nativeFormatStr, MpClipboardReaderRequest request) {
            while (WinApi.IsClipboardOpen(true)) {
                Thread.Sleep(100);
            }
            if (nativeFormatStr == DataFormats.FileDrop &&
                //WinApi.IsClipboardFormatAvailable(CF_HDROP)
                Clipboard.ContainsFileDropList()) {

                //WinApi.OpenClipboard(_mainWindowHandle);
                string[] sa = Clipboard.GetData(nativeFormatStr) as string[];
                if (sa != null && sa.Length > 0) {
                    return string.Join(Environment.NewLine, sa);
                }
                //WinApi.CloseClipboard();

            } else if (nativeFormatStr == DataFormats.Bitmap &&
                      //WinApi.IsClipboardFormatAvailable(CF_BITMAP)
                      Clipboard.ContainsImage()) {
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
                            // WinApi.CloseClipboard();
                        }
                        catch (Exception ex) {
                            MpConsole.WriteLine("MpHelpers.ConvertBitmapSourceToByteArray exception: " + ex);
                            //WinApi.CloseClipboard();
                            return null;
                        }
                    }
                    if (bytes != null) {
                        return Convert.ToBase64String(bytes);
                    }
                }
            } else {
                uint format = GetWin32FormatId(nativeFormatStr);
                if (format != 0) {
                    if (WinApi.IsClipboardFormatAvailable(format)) {
                        if (WinApi.IsClipboardFormatAvailable(format)) {
                            if (WinApi.OpenClipboard(_mainWindowHandle)) {
                                IntPtr hGMem = WinApi.GetClipboardData(format);
                                IntPtr pMFP = WinApi.GlobalLock(hGMem);
                                uint len = WinApi.GlobalSize(hGMem);
                                byte[] bytes = new byte[len];
                                Marshal.Copy(pMFP, bytes, 0, (int)len);

                                string strMFP = System.Text.Encoding.UTF8.GetString(bytes);
                                WinApi.GlobalUnlock(hGMem);
                                WinApi.CloseClipboard();

                                return strMFP;
                            }
                        }
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

        private MpClipboardReaderResponse CanHandleDataObject() {
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
                return new MpClipboardReaderResponse() {
                    errorMessage = "No Window Handle"
                };
            }
            return null;
        }

        #endregion

        #region MpClipboardWriterComponent Implementation

        public MpClipboardWriterResponse WriteClipboardData(MpClipboardWriterRequest request) {
            if (request == null) {
                return null;
            }

            DataObject dataObj = new DataObject();
            foreach (var kvp in request.data.DataFormatLookup) {
                string format = kvp.Key.Name;
                object data = kvp.Value;
                switch (format) {
                    case MpPortableDataFormats.Bitmap:
                        var bmpSrc = data.ToString().ToBitmapSource(false);

                        var winforms_dataobject = MpClipoardImageHelpers.GetClipboardImage_WinForms(bmpSrc.ToBitmap(), null, null);
                        var pngData = winforms_dataobject.GetData("PNG");
                        var dibData = winforms_dataobject.GetData(DataFormats.Dib);
                        dataObj.SetImage(bmpSrc);
                        dataObj.SetData("PNG", pngData);
                        dataObj.SetData(DataFormats.Dib, dibData);
                        break;
                    case MpPortableDataFormats.FileDrop:
                        var fl = data.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        var sc = new StringCollection();
                        sc.AddRange(fl);
                        dataObj.SetFileDropList(sc);
                        break;
                    default:
                        dataObj.SetData(format, data);
                        break;
                }
            }
            if(request.writeToClipboard) {
                Clipboard.SetDataObject(dataObj, true);
            }
            
            return new MpClipboardWriterResponse() {
                platformDataObject = dataObj
            };
        }

        #endregion




        

    }
}
