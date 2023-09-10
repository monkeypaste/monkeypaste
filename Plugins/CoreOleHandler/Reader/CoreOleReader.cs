using Avalonia.Input;
using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Text;

namespace CoreOleHandler {
    public static class CoreOleReader {
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
                availableFormats = await CoreOleHandler.ClipboardRef.GetFormatsSafeAsync();
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
                    var retry_result = await ProcessReadRequestAsync(request, --retryCount);
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
                        data = CoreParamProcessor.ProcessParam(param, read_format, data, out var ex, out var param_nfl);
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
                        if (data is object[] dataParts &&
                            dataParts[0] is object origData &&
                            dataParts[1] is object[] convParts &&
                            convParts[0] is string convFormat &&
                            convParts[1] is object convData) {
                            data = origData;
                            if (!string.IsNullOrEmpty(convFormat) &&
                                convData != null) {
                                // add converted format to output
                                read_output.SetData(convFormat, convData);
                            }
                        }
                    }
                }
                if (data == null) {
                    continue;
                }
                read_output.SetData(read_format, data);
            }

            return new MpClipboardReaderResponse() {
                dataObject = read_output.DataFormatLookup.ToDictionary(x => x.Key.Name, x => x.Value),
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
                format_data = await CoreOleHandler.ClipboardRef.GetDataSafeAsync(format);
                if (OperatingSystem.IsWindows() &&
                    format == MpPortableDataFormats.AvHtml_bytes && format_data is byte[] htmlBytes) {
                    var detected_encoding = htmlBytes.DetectTextEncoding(out string detected_text);
                    format_data = Encoding.UTF8.GetBytes(detected_text);
                    if (detected_text.Contains("Â")) {
                        MpDebug.Break();
                    }
                }
                //Util.CloseClipboard();

            } else {
                if (format == MpPortableDataFormats.AvFiles) {
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

        #endregion
    }
}