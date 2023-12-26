using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Text;

namespace CoreOleHandler {
    public class CoreOleReader : MpIOleReaderComponent {
        #region Private Variables
        #endregion
        static CoreOleReader() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        #region Public Methods

        public Task<MpOlePluginResponse> ProcessOleReadRequestAsync(MpOlePluginRequest request) =>
            ProcessReadRequestAsync_internal(request);

        #endregion

        #region Private Methods
        private async Task<MpOlePluginResponse> ProcessReadRequestAsync_internal(MpOlePluginRequest request, int retryCount = 10) {

            if (request.isDnd && !Dispatcher.UIThread.CheckAccess()) {
                return await Dispatcher.UIThread.InvokeAsync(async () => {
                    return await ProcessReadRequestAsync_internal(request, retryCount);
                });
            }

            IDataObject avdo = null;
            IEnumerable<string> availableFormats = null;
            // only actually read formats found for data
            if (request.dataObjectLookup == null) {
                // clipboard read
                avdo = await Dispatcher.UIThread.InvokeAsync(() => CoreOleHelpers.ClipboardRef.ToDataObjectAsync(formatFilter: request.formats.ToArray()));
                availableFormats = avdo.GetAllDataFormats();
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
            CoreOleHelpers.SetCulture(request);

            List<MpPluginUserNotificationFormat> nfl = new List<MpPluginUserNotificationFormat>();
            List<Exception> exl = new List<Exception>();
            Dictionary<string, object> conversion_results = new();
            var read_output = new MpAvDataObject();
            var readFormats = request.formats.Where(x => availableFormats.Contains(x));

            foreach (var read_format in readFormats) {
                if (MpPortableDataFormats.InternalFormats.Contains(read_format)) {
                    // ignore internal formats but let them pass through as output
                    read_output.SetData(read_format, avdo.Get(read_format));
                    continue;
                }
                // store data in object but read all data as strings
                object data = null;
                if (avdo.TryGetData(read_format, out string dataStr)) {
                    data = dataStr;
                }
                if (data == null) {
                    continue;
                }

                bool is_valid = true;
                if (!request.ignoreParams) {
                    foreach (var param in request.items) {
                        try {
                            data = CoreParamProcessor.ProcessParam(
                                paramInfo: param,
                                format: read_format,
                                data: dataStr,
                                all_formats: readFormats,
                                req: request,
                                convData: out Dictionary<string, object> conv_result,
                                ex: out var ex,
                                ntfl: out var param_nfl);

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
                            if (conv_result != null) {
                                conv_result.ForEach(x => conversion_results.AddOrReplace(x.Key, x.Value));
                            }
                        }
                        catch (CoreOleException cex) {
                            MpConsole.WriteTraceLine($"Param exception! format: '{read_format}'.", cex);
                            is_valid = false;
                        }

                    }
                }
                if (data == null || !is_valid) {
                    continue;
                }
                read_output.SetData(read_format, data);
            }
            if (conversion_results != null) {
                conversion_results.ForEach(x => read_output.SetData(x.Key, x.Value));
            }

            return new MpOlePluginResponse() {
                dataObjectLookup = read_output.ToDictionary(),
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
            };
        }

        #endregion
    }
}