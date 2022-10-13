using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using MonkeyPaste.Common.Wpf;
using Clowd.Clipboard;

namespace AvCoreClipboardHandler {
    public class AvCoreClipboardHandler :
        MpIClipboardReaderComponentAsync,
        MpIClipboardWriterComponentAsync {
        #region Private Variables

        private const int MAX_READ_RETRY_COUNT = 5;

        private IntPtr _mainWindowHandle;

        private static bool _isReadingOrWriting = false;
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
            //MpConsole.WriteLine("Reading clipboard...");

           while(_isReadingOrWriting) {
                MpConsole.WriteLine("waiting for clipboard (READER)...");
                await Task.Delay(100);
            }
            
            // MpClipboardReaderResponse hasError = CanHandleDataObject(request);
            // if (hasError != null) {
            //     return hasError;
            // }
            _isReadingOrWriting = true;
            IDataObject avdo = null;
            string[] availableFormats = null;
            // only actually read formats found for data
            if (request.forcedClipboardDataObject == null) {
                // clipboard read
                availableFormats = await Application.Current.Clipboard.GetFormatsAsync();
            } else if(request.forcedClipboardDataObject is IDataObject) {
                avdo = request.forcedClipboardDataObject as IDataObject;
                availableFormats = avdo.GetDataFormats().Where(x=>avdo.Get(x) != null).ToArray();
            }

            var readFormats = request.readFormats.Where(x => availableFormats.Contains(x));
            var currentOutput = new MpAvDataObject();

            foreach (var supportedTypeName in readFormats) {
                object data = await ReadDataObjectFormat(supportedTypeName, avdo);
                currentOutput.SetData(supportedTypeName, data);
            }
            _isReadingOrWriting = false;
            return new MpClipboardReaderResponse() {
                dataObject = currentOutput
            };
        }

        private async Task<object> ReadDataObjectFormat(string format, IDataObject avdo) {
            object dataObj;
            if(avdo == null) {
                dataObj = await Application.Current.Clipboard.GetDataAsync(format);

            } else {
                if (format == "FileNames") {
                    if (avdo.GetFileNames() == null) {
                        return String.Empty;
                    }
                    dataObj = avdo.GetFileNames();
                } else {
                    dataObj = avdo.Get(format);
                }                
            }
            string dataStr = null;

            if (dataObj is string) {
                dataStr = dataObj as string;
            } else if (dataObj is IEnumerable<string> strArr) {
                // should only happen for files
                dataStr = string.Join(Environment.NewLine, strArr);
            } else if (dataObj is byte[] bytes) {
                return bytes;
            }
            return dataStr;
        }

        private async Task<object> ReadDataObjectFormatHelper_windows(string format, IDataObject avdo, int retryCount = MAX_READ_RETRY_COUNT) { 
            if (retryCount < 0) {
                return null;
            }

            object dataObj;
            //bool wasOpen = false;
            //while (WinApi.IsClipboardOpen(true) != IntPtr.Zero) {
            //    wasOpen = true;
            //    MpConsole.WriteLine("Waiting on windows clipboard...");
            //    await Task.Delay(100);
            //}
            //if (wasOpen) {
            //    // if it was open other things maybe waiting also so let them 
            //    // go first...
            //    await Task.Delay(1000);
            //}
            //WinApi.OpenClipboard(_mainWindowHandle);
            try {
                if(format == TEXT_FORMAT) {
                    dataObj = await Application.Current.Clipboard.GetTextAsync();
                } else {
                    dataObj = await Application.Current.Clipboard.GetDataAsync(format);
                }
                
                //bool wasClosed = WinApi.CloseClipboard();
            }
            catch (Exception ex) {
                //bool wasClosed = WinApi.CloseClipboard();
                MpConsole.WriteTraceLine($"Error reading clipboard! Retry attempt #{5 - retryCount} of 5 starting..", ex);
                if (retryCount == MAX_READ_RETRY_COUNT) {
                    // only retry from initial call
                    object result;
                    while (retryCount > 0) {
                        await Task.Delay(100);
                        result = await ReadDataObjectFormatHelper_windows(format, avdo, retryCount--);
                        if (result != null) {
                            return result;
                        }
                    }
                    return null;
                } else {
                    return null;
                }
            }
            return dataObj;
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
            if (_isReadingOrWriting) {
                return new MpClipboardReaderResponse() {
                    errorMessage = "Already reading clipboard"
                };
            }
            IntPtr mwHandle = new IntPtr(request.mainWindowImplicitHandle);
            //MpConsole.WriteLine("mw handle - int: " + request.mainWindowImplicitHandle + " intPtr: " + mwHandle);
            //uint CF_HTML;//, CF_RTF, CF_CSV, CF_TEXT = 1, CF_BITMAP = 2, CF_DIB = 8, CF_HDROP = 15, CF_UNICODE_TEXT, CF_OEM_TEXT;
            if (mwHandle != IntPtr.Zero &&
                _mainWindowHandle == IntPtr.Zero) {
                // isolate first posssible request (need handle for WinApi.IsClipboardOpen)

                //CF_UNICODE_TEXT = WinApi.RegisterClipboardFormatA("UnicodeText");
                //CF_BITMAP = WinApi.RegisterClipboardFormatA("Bitmap");
                //CF_OEM_TEXT = WinApi.RegisterClipboardFormatA("OemText");
                //CF_HTML = WinApi.RegisterClipboardFormatA("HTML Format");
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

            while (_isReadingOrWriting) {
                //Debugger.Break();
                MpConsole.WriteLine("waiting for clipboard (WRITER)...");
                await Task.Delay(10);
            }
            MpAvDataObject dataObj = request.data as MpAvDataObject ?? new MpAvDataObject();
            _isReadingOrWriting = true;
            //foreach (var kvp in request.data.DataFormatLookup) {
            //    string format = kvp.Key.Name;
            //    object data = kvp.Value;
            //    switch (format) {
            //        case MpPortableDataFormats.AvPNG:
            //            //var bmpSrc = data.ToString().ToBitmapSource(false);

            //            //var winforms_dataobject = MpClipoardImageHelpers.GetClipboardImage_WinForms(bmpSrc.ToBitmap(), null, null);
            //            //var pngData = winforms_dataobject.GetData("PNG");
            //            //var dibData = winforms_dataobject.GetData(DataFormats.Dib);
            //            //dataObj.Set(MpPortableDataFormats.Bitmap, bmpSrc);
            //            dataObj.SetData(MpPortableDataFormats.AvPNG, data.ToString().ToByteArray());
            //            //dataObj.Set(DataFormats.Dib, dibData);
            //            break;
            //        case MpPortableDataFormats.AvFileNames:
            //            var fl = data.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            //            dataObj.SetData("FileNames", fl.ToList());
            //            break;
            //        case MpPortableDataFormats.AvHtml_bytes:
            //            var bytes = data.ToString().ToEncodedBytes();
            //            dataObj.SetData(MpPortableDataFormats.AvHtml_bytes, bytes);
            //            break;
            //        default:
            //            dataObj.SetData(format, data);
            //            break;
            //    }
            //}

           
            if (request.writeToClipboard) {
                await Application.Current.Clipboard.SetDataObjectAsync(dataObj);


                if (OperatingSystem.IsWindows()) {
                    if (dataObj.ContainsData(MpPortableDataFormats.AvPNG) &&
                        dataObj.GetData(MpPortableDataFormats.AvPNG) is byte[] pngBytes) {
                        MpWpfClipoardImageHelper.SetWinImageDataObjects(pngBytes);
                        //var win_img_obj_parts = MpWpfClipoardImageHelper.GetWinImageDataObjects(pngBytes);

                        //dataObj.SetData(MpPortableDataFormats.WinBitmap, win_img_obj_parts[0]);
                        //dataObj.SetData(MpPortableDataFormats.WinDib, win_img_obj_parts[1]);
                        //if (request.writeToClipboard) {
                        //    imgSet = true;
                        //    ClipboardAvalonia.SetImage(pngBytes.ToAvBitmap());
                        //}

                    }

                    // TODO add HTML->RTF convertsion here
                }
            }


            _isReadingOrWriting = false;
            return new MpClipboardWriterResponse() {
                platformDataObject = dataObj
            };
        }

        #endregion
    }
}