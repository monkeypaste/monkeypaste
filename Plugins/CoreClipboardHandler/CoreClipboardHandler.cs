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
        MpIClipboardPluginComponent,
        MpIPlatformDataObjectHelper {
        #region Private Variables

        private IntPtr _mainWindowHandle;

        private uint CF_HTML, CF_RTF, CF_CSV, CF_TEXT = 1, CF_BITMAP = 2, CF_DIB = 8, CF_HDROP = 15, CF_UNICODE_TEXT, CF_OEM_TEXT;
        #endregion

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

        #region MpIClipboardPluginComponent

        public MpPortableDataObject GetClipboardData() {
            var currentOutput = new MpPortableDataObject();
            
            if(!CanHandleDataObject()) {
                return currentOutput;
            }

            foreach(var nativeTypeName in MpPortableDataFormats.Formats) {
                var data = GetClipboardData(nativeTypeName);

                if (!string.IsNullOrEmpty(data)) {
                    currentOutput.SetData(nativeTypeName, data);
                }
            }
            return currentOutput;
        }

        public void SetClipboardData(MpPortableDataObject input) {
            if(input == null) {
                return;
            }

            DataObject dataObj = null;
            foreach(var kvp in input.DataFormatLookup) {
                string format = kvp.Key.Name;
                object data = kvp.Value;
                switch (format) {
                    case MpPortableDataFormats.Bitmap:
                        var bmpSrc = data.ToString().ToBitmapSource(false);

                        var winforms_dataobject = MpClipoardImageHelpers.GetClipboardImage_WinForms(bmpSrc.ToBitmap(), null, null);

                        //Clipboard.SetData(DataFormats.Bitmap, bmpSrc);
                        //Clipboard.SetData("PNG", winforms_dataobject.GetData("PNG"));
                        //Clipboard.SetData(DataFormats.Dib, winforms_dataobject.GetData(DataFormats.Dib));
                        //dobj.SetImage(data.ToString().ToBitmapSource());

                        //IDataObject ido = new DataObject();
                        //ido.SetData(DataFormats.Bitmap, new Image() { Source = bmpSrc },true); // true means autoconvert

                        //dobj.SetData(DataFormats.Bitmap, ido.GetData(DataFormats.Bitmap));
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

            Clipboard.SetDataObject(dataObj, true);
        }

        #endregion


        private string GetClipboardData(string nativeFormatStr) {
            while(WinApi.IsClipboardOpen(true)) {
                Thread.Sleep(100);
            }
            if(nativeFormatStr == DataFormats.FileDrop &&
                //WinApi.IsClipboardFormatAvailable(CF_HDROP)
                Clipboard.ContainsFileDropList()) {

                //WinApi.OpenClipboard(_mainWindowHandle);
                string[] sa = Clipboard.GetData(nativeFormatStr) as string[];                
                if (sa != null && sa.Length > 0) {
                    return string.Join(Environment.NewLine, sa);
                }
                //WinApi.CloseClipboard();

            } else if(nativeFormatStr == DataFormats.Bitmap &&
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
