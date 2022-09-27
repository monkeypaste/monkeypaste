using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;

namespace AvCoreClipboardHandler {
    public class AvCoreClipboardHandler :
        MpIClipboardReaderComponentAsync,
        MpIClipboardWriterComponentAsync {
        #region Private Variables

        private IntPtr _mainWindowHandle;

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

        // (for now at least) use this to map av cb formats to manfiest types
        private Dictionary<string, List<string>> _winToAvDataFormatMap => new Dictionary<string, List<string>>() {
            {
                FILE_DROP_FORMAT,
                new List<string>() {
                    //"FileItem",
                    //"FileItems",
                    //"FileNameW"
                    "FileNames"
                }
            }
        };

        #endregion

        #region MpIClipboardReaderComponentAsync Implementation

        async Task<MpClipboardReaderResponse> MpIClipboardReaderComponentAsync.ReadClipboardDataAsync(MpClipboardReaderRequest request) {
            MpClipboardReaderResponse hasError = CanHandleDataObject(request);
            if (hasError != null) {
                return hasError;
            }
            var currentOutput = new MpAvDataObject();

            foreach (var supportedTypeName in request.readFormats) {
                object data = await ReadDataObjectFormat(supportedTypeName, request.forcedClipboardDataObject as IDataObject);
                currentOutput.SetData(supportedTypeName, data);
            }
            return new MpClipboardReaderResponse() {
                dataObject = currentOutput
            };
        }

        private async Task<object> ReadDataObjectFormat(string format, IDataObject avdo) {
            object dataObj;
            if(avdo == null) {
                if (OperatingSystem.IsWindows()) {
                    bool wasOpen = false;
                    while (WinApi.IsClipboardOpen(true) != IntPtr.Zero) {
                        wasOpen = true;
                        MpConsole.WriteLine("Waiting on windows clipboard...");
                        await Task.Delay(100);
                    }
                    if (wasOpen) {
                        // if it was open other things maybe waiting also so let them 
                        // go first...
                        await Task.Delay(1000);
                    }
                }
                dataObj = await Application.Current.Clipboard.GetDataAsync(format);
                if(format == "FileNames") {
                    Debugger.Break();
                }
            } else {
                if (format == "FileNames") {
                    if (avdo.GetFileNames() == null) {
                        return String.Empty;
                    }
                    return string.Join(Environment.NewLine, avdo.GetFileNames());
                }
                dataObj = avdo.Get(format);
            }
            string dataStr = null;

            if (dataObj is string) {
                dataStr = dataObj as string;
            } else if (dataObj is string[] strArr) {
                dataStr = string.Join(Environment.NewLine, strArr);
            } else if (dataObj is byte[] bytes) {
                if(OperatingSystem.IsWindows() && format == MpPortableDataFormats.Html) {
                    if(avdo == null) {
                        string htmlData = MpAvWin32HtmlClipboardHelper.GetHTMLWin32Native(_mainWindowHandle);
                        return htmlData;

                        //var bytes2 = htmlData.ToByteArray();
                        //return bytes2;
                    }
                } 
                //if (format == MpPortableDataFormats.Html) {
                //    return dataObj;
                //}
                //dataStr = Encoding.UTF8.GetString(bytes);                

                //dataStr = bytes.ToBase64String();
                return bytes;
            }
            return dataStr;
        }

        private string ProcessReaderFormatParamsOnData_windows(MpClipboardReaderRequest req, string format, string data) {
            if (string.IsNullOrEmpty(data)) {
                return null;
            }

            switch (format) {
                case TEXT_FORMAT: {
                        if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Text, out string ignoreStr)
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
                        if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Rtf, out string ignoreStr)
                            && bool.Parse(ignoreStr)) {
                            return null;
                        }

                        //if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_MaxCharCount_Rtf, out string maxCharCountStr)) {
                        //    int maxCharCount = int.Parse(maxCharCountStr);

                        //    string pt = data.ToPlainText();
                        //    if (pt.Length > maxCharCount) {
                        //        // NOTE for rtf 
                        //        var fd = data.ToFlowDocument();
                        //        var ctp = fd.ContentEnd;
                        //        while (new TextRange(fd.ContentStart, ctp).Text.Length > maxCharCount) {
                        //            ctp = ctp.GetPositionAtOffset(-1);
                        //            if (ctp == fd.ContentStart || ctp == null) {
                        //                ctp = null;
                        //                break;
                        //            }
                        //        }
                        //        if (ctp != null) {
                        //            return new TextRange(fd.ContentStart, ctp).ToRichText();
                        //        }
                        //        return data.Substring(0, maxCharCount);
                        //    }
                        //}

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
                                                    .value.ToListFromCsv().Select(x => x.ToLower());

                        IEnumerable<string> fpl_filesToRemove = fpl.Where(x => ignoreExt.Any(y => x.EndsWith(y)));
                        for (int i = 0; i < fpl_filesToRemove.Count(); i++) {
                            fpl.Remove(fpl_filesToRemove.ElementAt(i));
                        }

                        IEnumerable<string> ignoreDir = req.items.FirstOrDefault(x => (CoreClipboardParamType)x.paramId == CoreClipboardParamType.R_IgnoredDir_FileDrop)
                                                    .value.ToListFromCsv().Select(x => x.ToLower());

                        IEnumerable<string> fpl_dirToRemove = fpl.Where(x => ignoreDir.Any(y => x.StartsWith(y)));
                        for (int i = 0; i < fpl_dirToRemove.Count(); i++) {
                            fpl.Remove(fpl_dirToRemove.ElementAt(i));
                        }
                        if (fpl.Count == 0) {
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

        private MpClipboardReaderResponse CanHandleDataObject(MpClipboardReaderRequest request) {
            if (request == null || !OperatingSystem.IsWindows()) {
                return null;
            }
            IntPtr mwHandle = new IntPtr(request.mainWindowImplicitHandle);
            //MpConsole.WriteLine("mw handle - int: " + request.mainWindowImplicitHandle + " intPtr: " + mwHandle);
            uint CF_HTML;//, CF_RTF, CF_CSV, CF_TEXT = 1, CF_BITMAP = 2, CF_DIB = 8, CF_HDROP = 15, CF_UNICODE_TEXT, CF_OEM_TEXT;
            if (mwHandle != IntPtr.Zero &&
                _mainWindowHandle == IntPtr.Zero) {
                // isolate first posssible request (need handle for WinApi.IsClipboardOpen)

                //CF_UNICODE_TEXT = WinApi.RegisterClipboardFormatA("UnicodeText");
                //CF_BITMAP = WinApi.RegisterClipboardFormatA("Bitmap");
                //CF_OEM_TEXT = WinApi.RegisterClipboardFormatA("OemText");
                CF_HTML = WinApi.RegisterClipboardFormatA("HTML Format");
                //CF_RTF = WinApi.RegisterClipboardFormatA("Rich Text Format");
                //CF_CSV = WinApi.RegisterClipboardFormatA(DataFormats.CommaSeparatedValue);
                //CF_DIB = WinApi.RegisterClipboardFormatA("DeviceIndependentBitmap");
                //CF_HDROP = WinApi.RegisterClipboardFormatA(DataFormats.FileDrop);

                _mainWindowHandle = mwHandle;
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

        async Task<MpClipboardWriterResponse> MpIClipboardWriterComponentAsync.WriteClipboardDataAsync(MpClipboardWriterRequest request) {
            if (request == null) {
                return null;
            }

            MpAvDataObject dataObj = new MpAvDataObject();
            foreach (var kvp in request.data.DataFormatLookup) {
                string format = kvp.Key.Name;
                object data = kvp.Value;
                switch (format) {
                    case MpPortableDataFormats.Bitmap:
                        //var bmpSrc = data.ToString().ToBitmapSource(false);

                        //var winforms_dataobject = MpClipoardImageHelpers.GetClipboardImage_WinForms(bmpSrc.ToBitmap(), null, null);
                        //var pngData = winforms_dataobject.GetData("PNG");
                        //var dibData = winforms_dataobject.GetData(DataFormats.Dib);
                        //dataObj.Set(MpPortableDataFormats.Bitmap, bmpSrc);
                        //dataObj.Set("PNG", pngData);
                        //dataObj.Set(DataFormats.Dib, dibData);
                        break;
                    case MpPortableDataFormats.FileDrop:
                        var fl = data.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        //var sc = new StringCollection();
                        //sc.AddRange(fl);
                        //dataObj.Set("FileNames", fl.ToList());
                        dataObj.SetData("FileNames", fl.ToList());
                        break;
                    case MpPortableDataFormats.Html:
                        var bytes = Encoding.Default.GetBytes(data.ToString());
                        dataObj.SetData(MpPortableDataFormats.Html, bytes);
                        break;
                    default:
                        dataObj.SetData(format, data);
                        break;
                }
            }
            if (request.writeToClipboard) {
                bool wasOpen = false;
                while (WinApi.IsClipboardOpen() != IntPtr.Zero) {
                    wasOpen = true;
                    await Task.Delay(10);
                }
                if(wasOpen) {
                    await Task.Delay(100);
                }
                await Application.Current.Clipboard.SetDataObjectAsync(dataObj);
            }

            return new MpClipboardWriterResponse() {
                platformDataObject = dataObj
            };
        }

        #endregion
    }
}