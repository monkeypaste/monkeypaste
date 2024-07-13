using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Dictionary<string, object> avdo = null;
            // only actually read formats found for data
            if (request.dataObjectLookup == null) {
                avdo = await MpAvClipboardExtensions.ReadClipboardAsync(formatFilter: request.formats.ToArray());
            } else {
                avdo = request.dataObjectLookup;

            }
            CoreOleHelpers.SetCulture(request);

            List<MpUserNotification> nfl = [];
            List<Exception> exl = [];
            Dictionary<string, object> conversion_results = [];
            Dictionary<string, object> read_output = [];
            var readFormats = request.formats.Where(x => avdo.ContainsKey(x));

            // PRE PROCESSING (only need for linux atm)
            if(!request.ignoreParams) {
                await PreProcessAsync(avdo, request.formats);
            }
            

            foreach (var read_format in readFormats) {
                if (MpDataFormatRegistrar.RegisteredInternalFormats.Contains(read_format)) {
                    // ignore internal formats but let them pass through as output
                    read_output.AddOrReplace(read_format, avdo[read_format]);
                    continue;
                }
                // store data in object but read all data as strings
                object data = null;
                if (avdo.TryGetValue(read_format, out string dataStr)) {
                    data = dataStr;
                }
                if (data == null) {
                    continue;
                }
                data = dataStr;

                bool is_valid = true;
                if (!request.ignoreParams) {
                    foreach (var param in request.items) {
                        try {
                            data = CoreOleParamProcessor.ProcessParam(
                                paramInfo: param,
                                format: read_format,
                                data: dataStr,
                                all_source_data: avdo,
                                all_target_data: read_output,
                                req: request,
                                allow_null_data: false,
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
                read_output.AddOrReplace(read_format, data);
            }
            if (conversion_results != null) {
                conversion_results.ForEach(x => read_output.AddOrReplace(x.Key, x.Value));
            }

            return new MpOlePluginResponse() {
                dataObjectLookup = read_output,
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
            };
        }
        #endregion

        private async Task PreProcessAsync(Dictionary<string,object> avdo, IEnumerable<string> request_formats) {
            // (LINUX)
            // Files
            if (OperatingSystem.IsLinux() &&
                request_formats.Contains(MpPortableDataFormats.Files) &&
                !avdo.ContainsKey(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT) &&
                !avdo.ContainsKey(MpPortableDataFormats.Files) &&
                avdo.ContainsKey(MpPortableDataFormats.LinuxFiles2) &&
                avdo.TryGetValue(MpPortableDataFormats.Text, out string fpl_str) &&
                fpl_str.SplitByLineBreak() is { } fpl) {
                // BUG avalonia 'Files' doesn't work on linux but the can inferred from the available formats
                // when not an internal ido and gnome-file flag format is present, text is a path list of files

                var sil = await fpl.ToAvFilesObjectAsync();
                avdo.Add(MpPortableDataFormats.Files, sil);
            }else if (OperatingSystem.IsLinux() &&
                request_formats.Contains(MpPortableDataFormats.Files) &&
                !avdo.ContainsKey(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT) &&
                !avdo.ContainsKey(MpPortableDataFormats.Files) &&
                avdo.ContainsKey(MpPortableDataFormats.MimeUriList) &&
                avdo.TryGetValue(MpPortableDataFormats.MimeUriList, out byte[] uril_bytes) &&
                uril_bytes.ToDecodedString() is { } uril_str &&
                uril_str.SplitByLineBreak() is { } uril &&
                uril.Select(x=>x.ToPathFromUri()) is { } uri_fpl) {
                // same as gnome files but its weird, sometimes gnome-files isn't there and this is text/uri-list is instead

                var sil = await uri_fpl.ToAvFilesObjectAsync();
                avdo.Add(MpPortableDataFormats.Files, sil);
            }

            // PNG
            if (OperatingSystem.IsLinux() &&
                request_formats.Contains(MpPortableDataFormats.AvImage) &&
                !avdo.ContainsKey(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT) &&
                !avdo.ContainsKey(MpPortableDataFormats.AvImage) &&
                MpPortableDataFormats.ImageFormats.FirstOrDefault(x=>avdo.ContainsKey(x)) is { } plat_img_format) {
                // to stay consistent map platform images to common "PNG" format if not present
                avdo.Add(MpPortableDataFormats.AvImage, avdo[plat_img_format]);
            }

            // end linux
        }
    }
}