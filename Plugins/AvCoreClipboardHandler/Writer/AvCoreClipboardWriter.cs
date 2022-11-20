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
            var ido = request.data as IDataObject;
            if(ido == null) {
                return null;
            }
            var sb = new StringBuilder();
            IDataObject dataObj = ido ?? new MpAvDataObject();
            var writeFormats = request.writeFormats.Where(x => ido.GetAllDataFormats().Contains(x));

            foreach(var write_format in writeFormats) {

                foreach (var param in request.items) {
                    if(ProcessWriterParam(param, dataObj) is string errors &&
                        !string.IsNullOrWhiteSpace(errors)) {
                        sb.AppendLine(errors);
                    }
                }
            }
           
            if (request.writeToClipboard) {
                await Util.WaitForClipboard();
                await Application.Current.Clipboard.SetDataObjectAsync(dataObj);
                Util.CloseClipboard();
            }

            return new MpClipboardWriterResponse() {
                processedDataObject = dataObj,
                errorMessage = sb.ToString()
            };
        }

        private static string ProcessWriterParam(MpIParameterKeyValuePair pkvp, IDataObject dataObj) {
            string errors = null;
            CoreClipboardParamType paramType = (CoreClipboardParamType)int.Parse(pkvp.paramName);
            switch(paramType) {
                case CoreClipboardParamType.W_MaxCharCount_Text:
                    if(dataObj.Contains(MpPortableDataFormats.Text) &&
                        dataObj.Get(MpPortableDataFormats.Text) is string text) {
                        int max_length = int.Parse(pkvp.value);
                        if(text.Length > max_length) {
                            text = text.Substring(0, max_length);
                            dataObj.Set(MpPortableDataFormats.Text, text);
                        }
                    }
                    break;
                case CoreClipboardParamType.W_MaxCharCount_WebText:
                    if (dataObj.Contains(MpPortableDataFormats.CefText) &&
                        dataObj.Get(MpPortableDataFormats.CefText) is string cefText) {
                        int max_length = int.Parse(pkvp.value);
                        if (cefText.Length > max_length) {
                            cefText = cefText.Substring(0, max_length);
                            dataObj.Set(MpPortableDataFormats.CefText, cefText);
                        }
                    }
                    break;
            }
            return errors;
        }
    }
}