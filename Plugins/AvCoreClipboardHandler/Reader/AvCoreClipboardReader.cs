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
    public static class AvCoreClipboardReader {

        private static string[] _AvReaderFormats = new string[]{
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
        public static async Task<MpClipboardReaderResponse> ProcessReadRequestAsync(MpClipboardReaderRequest request) {
            IDataObject avdo = null;
            IEnumerable<string> availableFormats = null;
            // only actually read formats found for data
            if (request.forcedClipboardDataObject == null) {
                // clipboard read
                await Util.WaitForClipboard();
                availableFormats = await Application.Current.Clipboard.GetFormatsAsync();
                Util.CloseClipboard();
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

            return new MpClipboardReaderResponse() {
                dataObject = currentOutput
            };
        }
       
        private static async Task<object> ReadDataObjectFormat(string format, IDataObject avdo) {
            object dataObj;

            if(avdo == null) {
                await Util.WaitForClipboard();
                dataObj = await Application.Current.Clipboard.GetDataAsync(format);
                if(OperatingSystem.IsWindows() &&
                    format == MpPortableDataFormats.AvHtml_bytes && dataObj is byte[] htmlBytes) {
                    var detected_encoding = htmlBytes.DetectTextEncoding(out string detected_text);
                    dataObj = Encoding.UTF8.GetBytes(detected_text);
                    if(detected_text.Contains("Â")) {
                        Debugger.Break();
                    }
                }
                Util.CloseClipboard();

            } else {
                if (format == "FileNames") {
                    if (avdo.GetFileNames() == null) {
                        return String.Empty;
                    }
                    //dataObj = await avdo.GetFileNames_safe(_readLock);
                    dataObj = avdo.GetFileNames();
                } else {
                    dataObj = avdo.Get(format);
                    //dataObj = await avdo.Get_safe(_readLock,format);
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
    }
}