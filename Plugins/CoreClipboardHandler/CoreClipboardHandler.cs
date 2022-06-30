using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System.Collections.Specialized;
using System.Windows.Documents;

namespace CoreClipboardHandler {
    public class CoreClipboardHandler : 
        MpIClipboardReaderComponent,
        MpIClipboardWriterComponent {
        #region Private Variables

        private IntPtr _mainWindowHandle;

        private uint CF_HTML, CF_RTF, CF_CSV, CF_TEXT = 1, CF_BITMAP = 2, CF_DIB = 8, CF_HDROP = 15, CF_UNICODE_TEXT, CF_OEM_TEXT;

        private enum CoreClipboardParamType {
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
            W_Ignore_Csv, //22

            //Added
            R_IgnoredDir_FileDrop
        }

        private const string TEXT_FORMAT = "Text";
        private const string RTF_FORMAT = "Rich Text Format";
        private const string HTML_FORMAT = "HTML Format";
        private const string BMP_FORMAT = "Bitmap";
        private const string FILE_DROP_FORMAT = "FileDrop";
        private const string CSV_FORMAT = "CSV";

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

        private string GetClipboardData(string format, MpClipboardReaderRequest request) {
            IntPtr handle_with_cb = WinApi.IsClipboardOpen(true);
            bool triedToClose = false;
            while (handle_with_cb != IntPtr.Zero) {
                if(triedToClose) {
                    Debugger.Break();
                }
                if(handle_with_cb == _mainWindowHandle) {
                    bool isClosed = WinApi.CloseClipboard();
                    MpConsole.WriteTraceLine("Clipboard is hung by this app while trying to read it, invoking close cliboard: " + (isClosed ? "SUCCESS" : "FAIL"));
                    triedToClose = true;
                }
                Thread.Sleep(100);
                handle_with_cb = WinApi.IsClipboardOpen(true);
            }
            string data = null;
            switch(format) {
                case FILE_DROP_FORMAT:
                    if(Clipboard.ContainsFileDropList()) {
                        string[] sa = Clipboard.GetData(format) as string[];
                        if (sa != null && sa.Length > 0) {
                            // NOTE ToLowering paths here ensures all item paths are lower case (which is app-wide convention)
                            data = string.Join(Environment.NewLine, sa.Select(x=>x.ToLower()));

                        }
                    }
                    break;
                case BMP_FORMAT:
                    if(Clipboard.ContainsImage()) {
                        var bmpSrc = Clipboard.GetImage();
                        if (bmpSrc != null) {
                            data = bmpSrc.ToBase64String();
                        }
                    }
                    break;
                default:
                    //if(Clipboard.ContainsData(format)) {
                    //    object raw_data = Clipboard.GetData(format);
                    //    data = raw_data.ToString();
                    //    if(string.IsNullOrEmpty(data)) {
                    //        Debugger.Break();
                    //    }
                    //}
                    uint formatId = GetWin32FormatId(format);
                    if (formatId != 0) {
                        if (WinApi.IsClipboardFormatAvailable(formatId)) {
                            if (WinApi.OpenClipboard(_mainWindowHandle)) {
                                IntPtr hGMem = WinApi.GetClipboardData(formatId);
                                IntPtr pMFP = WinApi.GlobalLock(hGMem);
                                uint len = WinApi.GlobalSize(hGMem);
                                byte[] bytes = new byte[len];
                                Marshal.Copy(pMFP, bytes, 0, (int)len);

                                string strMFP = System.Text.Encoding.UTF8.GetString(bytes);
                                WinApi.GlobalUnlock(hGMem);
                                WinApi.CloseClipboard();

                                data = strMFP;
                            }
                        }
                    }
                    break;
            }
            return ProcessReaderFormatParamsOnData(request, format, data); ;
        }

        private string ProcessReaderFormatParamsOnData(MpClipboardReaderRequest req, string format, string data) {
            if(string.IsNullOrEmpty(data)) {
                return null;
            }

            switch(format) {
                case TEXT_FORMAT: {
                        if(req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Text, out string ignoreStr)
                            && bool.Parse(ignoreStr)) {
                            return null;
                        }

                        if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_MaxCharCount_Text, out string maxCharCountStr)) {
                            int maxCharCount = int.Parse(maxCharCountStr);
                            if (data.Length > maxCharCount) {
                                return data.Substring(0, maxCharCount);
                            }
                        }
                        
                        return data;
                    }
                case RTF_FORMAT: {
                        if(req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Rtf, out string ignoreStr)
                            && bool.Parse(ignoreStr)) {
                            return null;
                        }

                        if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_MaxCharCount_Rtf, out string maxCharCountStr)) {
                            int maxCharCount = int.Parse(maxCharCountStr);

                            string pt = data.ToPlainText();
                            if (pt.Length > maxCharCount) {
                                // NOTE for rtf 
                                var fd = data.ToFlowDocument();
                                var ctp = fd.ContentEnd;
                                while(new TextRange(fd.ContentStart,ctp).Text.Length > maxCharCount) {
                                    ctp = ctp.GetPositionAtOffset(-1);
                                    if(ctp == fd.ContentStart || ctp == null) {
                                        ctp = null;
                                        break;
                                    }
                                }
                                if(ctp != null) {
                                    return new TextRange(fd.ContentStart, ctp).ToRichText();
                                }
                                return data.Substring(0, maxCharCount);
                            }
                        }

                        return data;
                    }
                case HTML_FORMAT: {
                        if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Html, out string ignoreStr)
                            && bool.Parse(ignoreStr)) {
                            return null;
                        }

                        if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_MaxCharCount_Html, out string maxCharCountStr)) {
                            int maxCharCount = int.Parse(maxCharCountStr);
                            if (data.Length > maxCharCount) {
                                return data.Substring(0, maxCharCount);
                            }
                        }

                        return data;
                    }
                case BMP_FORMAT: {
                        if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Bitmap, out string ignoreStr)
                            && bool.Parse(ignoreStr)) {
                            return null;
                        }
                        return data;
                    }
                case FILE_DROP_FORMAT: {
                        if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_IgnoreAll_FileDrop, out string ignoreStr)
                            && bool.Parse(ignoreStr)) {
                            return null;
                        }
                        // NOTE path's are all lower cased after read from clipboard

                        List<string> fpl = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        IEnumerable<string> ignoreExt = req.items
                                                    .FirstOrDefault(x => (CoreClipboardParamType)x.paramId == CoreClipboardParamType.R_IgnoredExt_FileDrop)
                                                    .value.ToListFromCsv().Select(x=>x.ToLower());
                        
                        IEnumerable<string> fpl_filesToRemove = fpl.Where(x => ignoreExt.Any(y => x.EndsWith(y)));
                        for (int i = 0; i < fpl_filesToRemove.Count(); i++) {
                            fpl.Remove(fpl_filesToRemove.ElementAt(i));
                        }

                        IEnumerable<string> ignoreDir = req.items.FirstOrDefault(x => (CoreClipboardParamType)x.paramId == CoreClipboardParamType.R_IgnoredDir_FileDrop)
                                                    .value.ToListFromCsv().Select(x=>x.ToLower());

                        IEnumerable<string> fpl_dirToRemove = fpl.Where(x => ignoreDir.Any(y => x.StartsWith(y)));
                        for (int i = 0; i < fpl_dirToRemove.Count(); i++) {
                            fpl.Remove(fpl_dirToRemove.ElementAt(i));
                        }
                        if(fpl.Count == 0) {
                            return null;
                        }
                        return String.Join(Environment.NewLine, fpl);
                    }
                case CSV_FORMAT: {

                        if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Csv, out string ignoreStr)
                            && bool.Parse(ignoreStr)) {
                            return null;
                        }

                        return data;
                    }
                default:
                    MpConsole.WriteTraceLine($"Warning, format '{format}' has no clipboard read parameter handler specified, returning raw clipboard data");
                    return data;
            }
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
