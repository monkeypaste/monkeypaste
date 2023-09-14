using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Text;

namespace AltOleHandler {
    public class AltOleReader : MpIOleReaderComponent {
        #region Private Variables
        #endregion
        static AltOleReader() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        #region Public Methods

        public Task<MpOlePluginResponse> ProcessOleRequestAsync(MpOlePluginRequest request) =>
            ProcessReadRequestAsync_internal(request);

        #endregion

        #region Private Methods
        private async Task<MpOlePluginResponse> ProcessReadRequestAsync_internal(MpOlePluginRequest request, int retryCount = 10) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                return await Dispatcher.UIThread.InvokeAsync(async () => {
                    return await ProcessReadRequestAsync_internal(request, retryCount);
                });
            }
            IDataObject avdo = null;
            IEnumerable<string> availableFormats = null;
            // only actually read formats found for data
            if (request.dataObjectLookup == null) {
                // clipboard read
                //await Util.WaitForClipboard();
                availableFormats = await AltOleHelpers.ClipboardRef.GetFormatsSafeAsync();
                //Util.CloseClipboard();
            } else {
                avdo = request.dataObjectLookup.ToDataObject();

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
                    var retry_result = await ProcessReadRequestAsync_internal(request, --retryCount);
                    return retry_result;
                }

            }

            List<MpPluginUserNotificationFormat> nfl = new List<MpPluginUserNotificationFormat>();
            List<Exception> exl = new List<Exception>();
            var read_output = new MpAvDataObject();
            var readFormats = request.formats.Where(x => availableFormats.Contains(x));

            foreach (var read_format in readFormats) {
                object data = await ReadDataObjectFormat(read_format, avdo);
                if (!request.ignoreParams) {
                    foreach (var param in request.items) {
                        data = AltParamProcessor.ProcessParam(param, read_format, data, out var ex, out var param_nfl);
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
            if (read_output.ContainsData("Dat funky format")) {
                read_output.SetData(MpPortableDataFormats.Text, read_output.GetData("Dat funky format"));
            }

            return new MpOlePluginResponse() {
                dataObjectLookup = read_output.ToDictionary(),
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
            };
        }
        private async Task<object> ReadDataObjectFormat(string format, IDataObject avdo) {
            object format_data = null;

            if (avdo == null) {
                //await Util.WaitForClipboard();
                format_data = await AltOleHelpers.ClipboardRef.GetDataSafeAsync(format);
                if (format == "Dat funky format" && format_data is byte[] byteData) {
                    format_data = byteData.ToDecodedString();
                }
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