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
            IEnumerable<string> availableFormats = null;
            // only actually read formats found for data
            if (request.forcedClipboardDataObject == null) {
                // clipboard read
                availableFormats = await Application.Current.Clipboard.GetFormatsAsync();
            } else if(request.forcedClipboardDataObject is IDataObject) {
                avdo = request.forcedClipboardDataObject as IDataObject;

                availableFormats = await avdo.GetDataFormats_safe(_readLock);
                //availableFormats = avdo.GetDataFormats().Where(x=>avdo.Get(x) != null).ToArray();
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

        #endregion
    }
}