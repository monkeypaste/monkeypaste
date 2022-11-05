using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using MonkeyPaste.Common.Wpf;

namespace AvCoreClipboardHandler {
    public class AvCoreClipboardHandler :
        MpIClipboardReaderComponentAsync,
        MpIClipboardWriterComponentAsync {
        #region Private Variables
        private object _readLock = System.Guid.NewGuid().ToString();

        private const int MAX_READ_RETRY_COUNT = 5;

        private IntPtr _mainWindowHandle;

        private static bool _isReadingOrWriting = false;
        private enum CoreClipboardParamType {
            //readers
            R_MaxCharCount_Text = 1,
            R_Ignore_Text,

            R_MaxCharCount_WebText,
            R_Ignore_WebText,

            R_MaxCharCount_Rtf,
            R_Ignore_Rtf,

            R_MaxCharCount_Html,
            R_Ignore_Html,

            R_MaxCharCount_WebHtml,
            R_Ignore_WebHtml,

            R_Ignore_WebUrl_Linux, 

            R_Ignore_Image,

            R_IgnoreAll_FileDrop,
            R_IgnoredExt_FileDrop,
            R_IgnoredDirs_FileDrop,

            R_Ignore_Csv, //16

            //writers
            W_MaxCharCount_Text,
            W_Ignore_Text,

            W_MaxCharCount_WebText,
            W_Ignore_WebText,

            W_MaxCharCount_Rtf,
            W_Ignore_Rtf,

            W_MaxCharCount_Html,
            W_Ignore_Html,

            W_MaxCharCount_WebHtml,
            W_Ignore_WebHtml,

            W_Ignore_WebUrl_Linux, // don't think is used...

            W_Format_Image,
            W_Ignore_Image,

            W_IgnoreAll_FileDrop,
            W_IgnoredExt_FileDrop,

            W_IgnoreAll_FileDrop_Linux,
            W_IgnoreExt_FileDrop_Linux,

            W_Ignore_Csv // 34
        }

        private string[] AvReaderFormats = new string[]{
            MpPortableDataFormats.Text,
            MpPortableDataFormats.CefText,
            MpPortableDataFormats.AvRtf_bytes,
            MpPortableDataFormats.AvHtml_bytes,
            MpPortableDataFormats.CefHtml,
            MpPortableDataFormats.LinuxSourceUrl,
            MpPortableDataFormats.AvPNG,
            MpPortableDataFormats.AvFileNames,
            MpPortableDataFormats.AvCsv
        };

        private string[] AvWriterFormats = new string[]{
            MpPortableDataFormats.Text,
            MpPortableDataFormats.CefText,
            MpPortableDataFormats.AvRtf_bytes,
            MpPortableDataFormats.AvHtml_bytes,
            MpPortableDataFormats.CefHtml,
            MpPortableDataFormats.LinuxSourceUrl,
            MpPortableDataFormats.AvPNG,
            MpPortableDataFormats.AvFileNames,
            MpPortableDataFormats.LinuxGnomeFiles, // only needed for write
            MpPortableDataFormats.AvCsv
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
            IEnumerable<string> availableFormats = null;
            // only actually read formats found for data
            if (request.forcedClipboardDataObject == null) {
                // clipboard read
                await WaitForClipboard();
                availableFormats = await Application.Current.Clipboard.GetFormatsAsync();
                CloseClipboard();
            } else if(request.forcedClipboardDataObject is IDataObject) {
                avdo = request.forcedClipboardDataObject as IDataObject;

                //availableFormats = await avdo.GetDataFormats_safe(_readLock);
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
                await WaitForClipboard();
                dataObj = await Application.Current.Clipboard.GetDataAsync(format);
                CloseClipboard();

            } else {
                if (format == "FileNames") {
                    if (avdo.GetFileNames() == null) {
                        return String.Empty;
                    }
                    dataObj = await avdo.GetFileNames_safe(_readLock);
                    //dataObj = avdo.GetFileNames();
                } else {
                    //dataObj = avdo.Get(format);
                    dataObj = await avdo.Get_safe(_readLock,format);
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

        private async Task WaitForClipboard() {
            if (OperatingSystem.IsWindows()) {
                bool canOpen = MonkeyPaste.Common.Avalonia.WinApi.IsClipboardOpen() == IntPtr.Zero;
                while (!canOpen) {
                    await Task.Delay(50);
                    canOpen = MonkeyPaste.Common.Avalonia.WinApi.IsClipboardOpen() == IntPtr.Zero;
                }
            }
        }

        private void CloseClipboard() {
            if(OperatingSystem.IsWindows()) {
                MonkeyPaste.Common.Avalonia.WinApi.CloseClipboard();
            }
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
            foreach(var param in request.items) {
                ProcessWriterParam(param, dataObj);
            }
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
                await WaitForClipboard();
                await Application.Current.Clipboard.SetDataObjectAsync(dataObj);
                CloseClipboard();

                //if (OperatingSystem.IsWindows()) {
                //    if (dataObj.ContainsData(MpPortableDataFormats.AvPNG) &&
                //        dataObj.GetData(MpPortableDataFormats.AvPNG) is byte[] pngBytes) {
                //        MpWpfClipoardImageHelper.SetWinImageDataObjects(pngBytes);
                //        //var win_img_obj_parts = MpWpfClipoardImageHelper.GetWinImageDataObjects(pngBytes);

                //        //dataObj.SetData(MpPortableDataFormats.WinBitmap, win_img_obj_parts[0]);
                //        //dataObj.SetData(MpPortableDataFormats.WinDib, win_img_obj_parts[1]);
                //        //if (request.writeToClipboard) {
                //        //    imgSet = true;
                //        //    ClipboardAvalonia.SetImage(pngBytes.ToAvBitmap());
                //        //}

                //    }

                //    // TODO add HTML->RTF convertsion here
                //}
            }


            _isReadingOrWriting = false;
            return new MpClipboardWriterResponse() {
                platformDataObject = dataObj
            };
        }

        private void ProcessWriterParam(MpIParameterKeyValuePair pkvp, MpAvDataObject dataObj) {
            CoreClipboardParamType paramType = (CoreClipboardParamType)int.Parse(pkvp.paramName);
            switch(paramType) {
                case CoreClipboardParamType.W_MaxCharCount_Text:
                    if(dataObj.ContainsData(MpPortableDataFormats.Text) &&
                        dataObj.GetData(MpPortableDataFormats.Text) is string text) {
                        int max_length = int.Parse(pkvp.value);
                        if(text.Length > max_length) {
                            text = text.Substring(0, max_length);
                            dataObj.SetData(MpPortableDataFormats.Text, text);
                        }
                    }
                    break;
                case CoreClipboardParamType.W_MaxCharCount_WebText:
                    if (dataObj.ContainsData(MpPortableDataFormats.CefText) &&
                        dataObj.GetData(MpPortableDataFormats.CefText) is string cefText) {
                        int max_length = int.Parse(pkvp.value);
                        if (cefText.Length > max_length) {
                            cefText = cefText.Substring(0, max_length);
                            dataObj.SetData(MpPortableDataFormats.CefText, cefText);
                        }
                    }
                    break;
            }
        }
        //private string ProcessReaderFormatParamsOnData_windows(MpClipboardReaderRequest req, string format, string data) {
        //    if (string.IsNullOrEmpty(data)) {
        //        return null;
        //    }

        //    switch (format) {
        //        case TEXT_FORMAT: {
        //                if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Text, out string ignoreStr)
        //                    && bool.Parse(ignoreStr)) {
        //                    return null;
        //                }

        //                if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_MaxCharCount_Text, out string maxCharCountStr)) {
        //                    int maxCharCount = int.Parse(maxCharCountStr);
        //                    if (data.Length > maxCharCount) {
        //                        return data.Substring(0, maxCharCount);
        //                    }
        //                }

        //                return data;
        //            }
        //        case RTF_FORMAT: {
        //                if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Rtf, out string ignoreStr)
        //                    && bool.Parse(ignoreStr)) {
        //                    return null;
        //                }

        //                if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_MaxCharCount_Rtf, out string maxCharCountStr)) {
        //                    int maxCharCount = int.Parse(maxCharCountStr);

        //                    string pt = data.ToPlainText();
        //                    if (pt.Length > maxCharCount) {
        //                        // NOTE for rtf 
        //                        var fd = data.ToFlowDocument();
        //                        var ctp = fd.ContentEnd;
        //                        while (new TextRange(fd.ContentStart, ctp).Text.Length > maxCharCount) {
        //                            ctp = ctp.GetPositionAtOffset(-1);
        //                            if (ctp == fd.ContentStart || ctp == null) {
        //                                ctp = null;
        //                                break;
        //                            }
        //                        }
        //                        if (ctp != null) {
        //                            return new TextRange(fd.ContentStart, ctp).ToRichText();
        //                        }
        //                        return data.Substring(0, maxCharCount);
        //                    }
        //                }

        //                return data;
        //            }
        //        case HTML_FORMAT: {
        //                if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Html, out string ignoreStr)
        //                    && bool.Parse(ignoreStr)) {
        //                    return null;
        //                }

        //                if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_MaxCharCount_Html, out string maxCharCountStr)) {
        //                    int maxCharCount = int.Parse(maxCharCountStr);
        //                    if (data.Length > maxCharCount) {
        //                        return data.Substring(0, maxCharCount);
        //                    }
        //                }

        //                return data;
        //            }
        //        case BMP_FORMAT: {
        //                if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Image, out string ignoreStr)
        //                    && bool.Parse(ignoreStr)) {
        //                    return null;
        //                }
        //                return data;
        //            }
        //        case FILE_DROP_FORMAT: {
        //                if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_IgnoreAll_FileDrop, out string ignoreStr)
        //                    && bool.Parse(ignoreStr)) {
        //                    return null;
        //                }
        //                // NOTE path's are all lower cased after read from clipboard

        //                List<string> fpl = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

        //                IEnumerable<string> ignoreExt = req.items
        //                                            .FirstOrDefault(x => (CoreClipboardParamType)x.paramName == CoreClipboardParamType.R_IgnoredExt_FileDrop)
        //                                            .value.ToListFromCsv().Select(x => x.ToLower());

        //                IEnumerable<string> fpl_filesToRemove = fpl.Where(x => ignoreExt.Any(y => x.EndsWith(y)));
        //                for (int i = 0; i < fpl_filesToRemove.Count(); i++) {
        //                    fpl.Remove(fpl_filesToRemove.ElementAt(i));
        //                }

        //                IEnumerable<string> ignoreDir = req.items.FirstOrDefault(x => (CoreClipboardParamType)x.paramName == CoreClipboardParamType.R_IgnoredDir_FileDrop)
        //                                            .value.ToListFromCsv().Select(x => x.ToLower());

        //                IEnumerable<string> fpl_dirToRemove = fpl.Where(x => ignoreDir.Any(y => x.StartsWith(y)));
        //                for (int i = 0; i < fpl_dirToRemove.Count(); i++) {
        //                    fpl.Remove(fpl_dirToRemove.ElementAt(i));
        //                }
        //                if (fpl.Count == 0) {
        //                    return null;
        //                }
        //                return String.Join(Environment.NewLine, fpl);
        //            }
        //        case CSV_FORMAT: {

        //                if (req.ParamLookup.TryGetValue((int)CoreClipboardParamType.R_Ignore_Csv, out string ignoreStr)
        //                    && bool.Parse(ignoreStr)) {
        //                    return null;
        //                }

        //                return data;
        //            }
        //        default:
        //            MpConsole.WriteTraceLine($"Warning, format '{format}' has no clipboard read parameter handler specified, returning raw clipboard data");
        //            return data;
        //    }
        //}
        #endregion
    }
}