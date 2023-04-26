using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Diagnostics;
using System.Text;

namespace AvCoreClipboardHandler {
    public static class AvCoreClipboardReader {
        #region Private Variables

        #endregion

        #region Public Methods

        public static async Task<MpClipboardReaderResponse> ProcessReadRequestAsync(MpClipboardReaderRequest request, int retryCount = 10) {
            IDataObject avdo = null;
            IEnumerable<string> availableFormats = null;
            // only actually read formats found for data
            if (request.forcedClipboardDataObject == null) {
                // clipboard read
                //await Util.WaitForClipboard();
                availableFormats = await AvCoreClipboardHandler.ClipboardRef.GetFormatsSafeAsync();
                //Util.CloseClipboard();
            } else if (request.forcedClipboardDataObject is IDataObject) {
                avdo = request.forcedClipboardDataObject as IDataObject;

                try {

                    availableFormats = avdo.GetAllDataFormats();//.Where(x => avdo.Get(x) != null).ToArray();
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error reading dnd formats retrying (attempt {10 - retryCount + 1})", ex);
                    await Task.Delay(100);

                    if (retryCount == 0) {
                        MpConsole.WriteLine("Retry attempts reached, failed");
                        return null;
                    }
                    var retry_result = await ProcessReadRequestAsync(request, retryCount--);
                    return retry_result;
                }

            }

            List<MpPluginUserNotificationFormat> nfl = new List<MpPluginUserNotificationFormat>();
            List<Exception> exl = new List<Exception>();
            var read_output = new MpAvDataObject();
            var readFormats = request.readFormats.Where(x => availableFormats.Contains(x));

            foreach (var read_format in readFormats) {
                object data = await ReadDataObjectFormat(read_format, avdo);
                if (!request.ignoreParams) {
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

        #endregion

        #region Private Methods

        private static async Task<object> ReadDataObjectFormat(string format, IDataObject avdo) {
            object format_data = null;

            if (avdo == null) {
                //await Util.WaitForClipboard();
                format_data = await AvCoreClipboardHandler.ClipboardRef.GetDataSafeAsync(format);
                if (OperatingSystem.IsWindows() &&
                    format == MpPortableDataFormats.AvHtml_bytes && format_data is byte[] htmlBytes) {
                    var detected_encoding = htmlBytes.DetectTextEncoding(out string detected_text);
                    format_data = Encoding.UTF8.GetBytes(detected_text);
                    if (detected_text.Contains("Â")) {
                        Debugger.Break();
                    }
                }
                //Util.CloseClipboard();

            } else {
                if (format == MpPortableDataFormats.AvFileNames) {
                    if (avdo.GetFilesAsPaths() is IEnumerable<string> paths &&
                        paths.Any()) {
                        format_data = paths;
                    }
                } else {
                    format_data = avdo.Get(format);
                }
            }
            string dataStr = null;

            if (format_data is string) {
                dataStr = format_data as string;
            } else if (format_data is IEnumerable<string> strArr) {
                // should only happen for files
                dataStr = string.Join(Environment.NewLine, strArr);
            } else if (format_data is byte[] bytes) {
                return bytes;
            } else if (format_data is IEnumerable<IStorageItem> paths &&
                paths.Select(x => x.TryGetLocalPath()) is IEnumerable<string> pl) {
                dataStr = string.Join(Environment.NewLine, pl);
            }
            return dataStr;
        }

        private static object ProcessReaderParam(
            MpParameterRequestItemFormat pkvp,
            string format,
            object data,
            out Exception ex,
            out List<MpPluginUserNotificationFormat> nfl) {
            ex = null;
            nfl = null;
            if (data == null || pkvp == null) {
                // already omitted
                return data;
            }
            string paramVal = pkvp.value;
            try {
                // NOTE by internal convention 'paramId' is an int.
                // plugin creator has to manage mapping internally
                CoreClipboardParamType paramType = (CoreClipboardParamType)Convert.ToInt32(pkvp.paramId);
                switch (format) {
                    case MpPortableDataFormats.AvRtf_bytes:
                        switch (paramType) {
                            case CoreClipboardParamType.R_MaxCharCount_Rtf:
                                if (data is string rtf) {
                                    int max_length = int.Parse(paramVal);
                                    if (rtf.Length > max_length) {
                                        nfl = new List<MpPluginUserNotificationFormat>() {
                                            Util.CreateNotification(
                                                MpPluginNotificationType.PluginResponseWarning,
                                                "Max Char Count Reached",
                                                $"{format} limit is '{max_length}' and data was '{rtf.Length}'",
                                                "CoreClipboardWriter")
                                        };
                                        data = rtf.Substring(0, max_length);
                                    }
                                }
                                break;
                            case CoreClipboardParamType.R_Ignore_Rtf:
                                if (paramVal.ParseOrConvertToBool(false) is bool ignoreRtf &&
                                    ignoreRtf) {
                                    AddIgnoreNotification(ref nfl, format);
                                    data = null;

                                } else {
                                    return data;
                                }
                                break;
                        }
                        break;
                    case MpPortableDataFormats.Text:
                        switch (paramType) {
                            case CoreClipboardParamType.R_MaxCharCount_Text:
                                if (data is string text) {
                                    int max_length = int.Parse(paramVal);
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
                                if (paramVal.ParseOrConvertToBool(false) is bool ignText &&
                                    ignText) {
                                    AddIgnoreNotification(ref nfl, format);
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
                    case MpPortableDataFormats.AvPNG:
                        switch (paramType) {
                            case CoreClipboardParamType.R_Ignore_Image:
                                if (paramVal.ParseOrConvertToBool(false) is bool ignImg &&
                                    ignImg) {
                                    data = null;
                                }
                                break;
                        }
                        break;

                    case MpPortableDataFormats.AvFileNames:
                        switch (paramType) {
                            case CoreClipboardParamType.R_IgnoreAll_FileDrop:
                                if (paramVal.ParseOrConvertToBool(false) is bool ignore_fd &&
                                    ignore_fd) {

                                    data = null;
                                    AddIgnoreNotification(ref nfl, format);
                                }
                                break;
                            case CoreClipboardParamType.R_IgnoredExt_FileDrop:
                                if (!string.IsNullOrWhiteSpace(paramVal) &&
                                    paramVal.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value) is List<string> iel &&
                                    data is IEnumerable<string> fpl) {
                                    var files_to_ignore = fpl.Where(x => iel.Any(y => x.ToLower().EndsWith(y.ToLower())));
                                    MpConsole.WriteLine($"Clipboard or drag File rejected by extension: {string.Join(Environment.NewLine, files_to_ignore)}");
                                    // null ignored exts
                                    files_to_ignore
                                        .ForEach(x => x = null);
                                    if (fpl.All(x => x == null)) {
                                        // all omitted remove format
                                        data = null;
                                        break;
                                    }
                                    if (fpl is string[] fpArr) {

                                        fpArr.RemoveNullsInPlace();
                                        // pretty sure setting this isn't necessary but jic
                                        data = fpArr;
                                    } else {
                                        MpDebug.Break("Files is NOT an array!");
                                    }
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

        private static void AddIgnoreNotification(ref List<MpPluginUserNotificationFormat> nfl, string format) {
            if (nfl == null) {
                nfl = new List<MpPluginUserNotificationFormat>();
            }
            nfl.Add(Util.CreateNotification(
                MpPluginNotificationType.PluginResponseWarning,
                "Format Ignored",
                $"{format} Format is flagged as 'ignored'",
                "CoreClipboardWriter"));
        }
        #endregion
    }
}