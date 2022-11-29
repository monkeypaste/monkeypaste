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

        public static string[] _AvReaderFormats = new string[]{
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

                availableFormats = avdo.GetDataFormats().Where(x=>avdo.Get(x) != null).ToArray();
            }

            List<MpPluginUserNotificationFormat> nfl = new List<MpPluginUserNotificationFormat>();
            List<Exception> exl = new List<Exception>();
            var read_output = new MpAvDataObject();
            var readFormats = request.readFormats.Where(x => availableFormats.Contains(x));

            foreach (var read_format in readFormats) {
                object data = await ReadDataObjectFormat(read_format, avdo);
                if(!request.ignoreParams) {
                    foreach (var param in request.items) {
                        data = ProcessReaderParam(param, read_format, data, out var ex, out var param_nfl);
                        if (ex != null) {
                            exl.Add(ex);
                        }
                        if (param_nfl != null) {
                            nfl.AddRange(param_nfl);
                        }
                        if (data == null) {
                            // param omitted format, don't process rest of params
                            break;
                        }
                    }
                }
                if (data == null) {
                    continue;
                }
                read_output.SetData(read_format, data);
            }

            return new MpClipboardReaderResponse() {
                dataObject = read_output,
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
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


        private static object ProcessReaderParam(MpIParameterKeyValuePair pkvp, string format, object data, out Exception ex, out List<MpPluginUserNotificationFormat> nfl) {
            ex = null;
            nfl = null;
            if (data == null) {
                // already omitted
                return null;
            }
            try {
                CoreClipboardParamType paramType = (CoreClipboardParamType)int.Parse(pkvp.paramName);
                switch (format) {
                    case MpPortableDataFormats.Text:
                        switch (paramType) {
                            case CoreClipboardParamType.R_MaxCharCount_Text:
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
                            case CoreClipboardParamType.R_Ignore_Text:
                                bool ignoreText = bool.Parse(pkvp.value);
                                if(ignoreText) {
                                    nfl = new List<MpPluginUserNotificationFormat>() {
                                        Util.CreateNotification(
                                            MpPluginNotificationType.PluginResponseWarning,
                                            "Format Ignored",
                                            $"Text Format is flagged as 'ignored'",
                                            "CoreClipboardWriter")
                                    };
                                    data = null;

                                } else {
                                    //nfl = new List<MpPluginUserNotificationFormat>() {
                                    //    Util.CreateNotification(
                                    //        MpPluginNotificationType.PluginResponseMessage,
                                    //        "Test",
                                    //        $"Text Copied: '{data.ToString()}'",
                                    //        "CoreClipboardWriter")
                                    //};
                                    return data;
                                }
                                break;
                        }
                        break;
                    default:
                        // TODO process other types

                        break;
                }
                return data;
            }
            catch (Exception e) {
                ex = e;
            }
            return data;
        }
    }
}