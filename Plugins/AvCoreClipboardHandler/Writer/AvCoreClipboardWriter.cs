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
        // manifest writer formats (NOTE not used but to keep track of references)
        public static string[] AvWriterFormats = new string[]{
            MpPortableDataFormats.Text,
            MpPortableDataFormats.CefText,
            MpPortableDataFormats.AvRtf_bytes,
            MpPortableDataFormats.AvHtml_bytes,
            MpPortableDataFormats.CefHtml,
            MpPortableDataFormats.LinuxSourceUrl,
            MpPortableDataFormats.AvPNG,
            MpPortableDataFormats.AvFileNames,
            MpPortableDataFormats.LinuxGnomeFiles, // only needed for write
            MpPortableDataFormats.AvCsv,
            MpPortableDataFormats.LinuxUriList
        };

        public static async Task<MpClipboardWriterResponse> PerformWriteRequestAsync(MpClipboardWriterRequest request) {
            if (request == null) {
                return null;
            }
            var ido = request.data as IDataObject;
            if(ido == null) {
                return null;
            }
            List<MpPluginUserNotificationFormat> nfl = new List<MpPluginUserNotificationFormat>();
            List<Exception> exl = new List<Exception>();
            IDataObject write_output = ido ?? new MpAvDataObject();
            var writeFormats = request.writeFormats.Where(x => ido.GetAllDataFormats().Contains(x));

            foreach(var write_format in writeFormats) {
                object data = write_output.Get(write_format);
                foreach (var param in request.items) {
                    data = ProcessWriterParam(param, write_format, data, out var ex, out var param_nfl);
                    if(ex != null) {
                        exl.Add(ex);
                    }
                    if(param_nfl != null) {
                        nfl.AddRange(param_nfl);
                    }
                    if(data == null) {
                        // param omitted format, don't process rest of params
                        break;
                    }
                }
                if(data == null) {
                    continue;
                }
                write_output.Set(write_format, data);
            }
           
            if (request.writeToClipboard) {
                //await Util.WaitForClipboard();
                await Application.Current.Clipboard.SetDataObjectSafeAsync(write_output);
                //Util.CloseClipboard();
            }

            return new MpClipboardWriterResponse() {
                processedDataObject = write_output,
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine,exl)
            };
        }

        private static object ProcessWriterParam(MpIParameterKeyValuePair pkvp, string format, object data, out Exception ex, out List<MpPluginUserNotificationFormat> nfl) {
            ex = null;
            nfl = null;
            if(data == null) {
                // already omitted
                return null;
            }
            try {
                CoreClipboardParamType paramType = (CoreClipboardParamType)Convert.ToInt32(pkvp.paramId);
                switch(format) {
                    case MpPortableDataFormats.Text:
                        switch (paramType) {
                            case CoreClipboardParamType.W_MaxCharCount_Text:
                                if (data is string text) {
                                    int max_length = int.Parse(pkvp.value);
                                    if (text.Length > max_length) {
                                        nfl = new List<MpPluginUserNotificationFormat>() {
                                            Util.CreateNotification(
                                                MpPluginNotificationType.PluginResponseWarning,
                                                "Max Char Count Reached",
                                                $"Text limit is '{max_length}' and data was '{text.Length}'",
                                                "CoreClipboardWriter")
                                        };
                                        data = text.Substring(0, max_length);
                                    }
                                }
                                break;
                            case CoreClipboardParamType.W_Ignore_Text:
                                data = null;
                                break;
                        }
                        break;
                    default:
                        // TODO process other types

                        break;
                }
                return data;
            }
            catch(Exception e) {
                ex = e;
            }
            return data;
        }
    }
}