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
    public static class AvCoreClipboardWriter {

        private static string[] _AvWriterFormats = new string[]{
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
        public static async Task<MpClipboardWriterResponse> PerformWriteRequestAsync(MpClipboardWriterRequest request) {
            if (request == null) {
                return null;
            }
            MpAvDataObject dataObj = request.data as MpAvDataObject ?? new MpAvDataObject();
            var writeFormats = request.writeFormats.Where(x => request.data.ContainsData(x));
            foreach(var write_format in writeFormats) {

                foreach (var param in request.items) {
                    ProcessWriterParam(param, dataObj);
                }
            }
           
            if (request.writeToClipboard) {
                await Util.WaitForClipboard();
                await Application.Current.Clipboard.SetDataObjectAsync(dataObj);
                Util.CloseClipboard();
            }

            return new MpClipboardWriterResponse() {
                platformDataObject = dataObj
            };
        }

        private static void ProcessWriterParam(MpIParameterKeyValuePair pkvp, MpAvDataObject dataObj) {
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
    }
}