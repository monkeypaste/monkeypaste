using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace CoreOleHandler {
    public class CoreOleWriter : MpIOleWriterComponent {
        #region Private Variables

        //private int _cur_img_quality = 100;

        #endregion

        #region Public Methods

        public async Task<MpOlePluginResponse> ProcessOleWriteRequestAsync(MpOlePluginRequest request) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                return await Dispatcher.UIThread.InvokeAsync(async () => {
                    return await ProcessOleWriteRequestAsync(request);
                });
            }
            if (request == null ||
                request.dataObjectLookup is not Dictionary<string, object> ido_dict) {
                return null;
            }
            
            List<MpUserNotification> nfl = new List<MpUserNotification>();
            CoreOleHelpers.SetCulture(request);
            List<Exception> exl = new List<Exception>();
            Dictionary<string, object> conversion_results = [];
            Dictionary<string, object> write_output = ido_dict == null ? [] : ido_dict;
            var writeFormats =
                request.formats
                .Where(x => ido_dict.ContainsKey(x))
                .OrderBy(x => GetWriterPriority(x))
                .ToList();

            string source_type = null;
            if (ido_dict.TryGetValue(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT, out object source_type_obj)) {
                source_type = source_type_obj as string;
            }
            bool needs_pseudo_file = false;
            if (source_type != "FileList" && // NOTE using 'FileList' to avoid moving MpCopyItemType into common
                request.GetParamValue<bool>(CoreOleParamType.FILES_W_IGNORE.ToString()) is false) {
                // when file type is enabled but source is not a file,
                // add file as a format and flag that it needs to be created
                // AFTER all runtime formats have been processed so best format is written
                                
                needs_pseudo_file = true;
                if(!writeFormats.Contains(MpPortableDataFormats.Files)) {
                    writeFormats.Add(MpPortableDataFormats.Files);
                }
            }
            bool needs_pseudo_image = false;
            if (!request.GetParamValue(CoreOleParamType.PNG_W_FROMTEXTFORMATS.ToString()).IsNullOrEmpty() &&
                !writeFormats.Contains(MpPortableDataFormats.Image)) {
                // when text2img param has any formats and image is not present,
                // add image format so its picked up by ascii param
                writeFormats.Add(MpPortableDataFormats.Image);
                needs_pseudo_image = true;
            }

            foreach (var write_format in writeFormats) {
                object data = null;
                if (write_output.TryGetValue(write_format, out string dataStr)) {
                    data = dataStr;
                }

                bool allow_null = 
                    (needs_pseudo_image && write_format == MpPortableDataFormats.Image) ||
                    (needs_pseudo_file && write_format == MpPortableDataFormats.Files);

                foreach (var param in request.items) {
                    data = CoreOleParamProcessor.ProcessParam(
                        paramInfo: param,
                        format: write_format,
                        data: data,
                        all_source_data: ido_dict,
                        all_target_data: write_output,
                        req: request,
                        allow_null_data: allow_null,
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
                if (data == null) {
                    continue;
                }
                write_output.AddOrReplace(write_format, data);
            }
            if (conversion_results != null) {
                conversion_results.ForEach(x => write_output.AddOrReplace(x.Key, x.Value));
            }
            // NOTE not sure if this is needed omitted for now
            //PostProcessImage(write_output);
            await PrepareForOutputAsync(write_output, request.isDnd, writeFormats);

            return new MpOlePluginResponse() {
                dataObjectLookup = write_output,
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
            };
        }

        #endregion

        #region Private Methods

        #region File Pre-Processor
        private int GetWriterPriority(string format) {
            if (format == MpPortableDataFormats.Files) {
                // process files last
                return int.MaxValue;
            }
            return 0;
        }

        #endregion

        #region Windows Image Post-processor

        private void PostProcessImage(IDataObject ido) {
#if WINDOWS
            if (ido.TryGetData(MpPortableDataFormats.Image, out byte[] pngBytes)) {
                object dib = MonkeyPaste.Common.Wpf.MpWpfClipoardImageHelper.GetWpfDib(pngBytes);
                ido.Set(MpPortableDataFormats.WinDib, dib);

                object bmp = MonkeyPaste.Common.Wpf.MpWpfClipoardImageHelper.GetSysDrawingBitmap(pngBytes);
                ido.Set(MpPortableDataFormats.WinBitmap, bmp);
            }

#endif
        }
        #endregion

        #region Platform DataObject Post-Process
        private async Task PrepareForOutputAsync(
            Dictionary<string, object> ido, 
            bool isDnd, 
            IEnumerable<string> formats) {

            await MapAllPseudoFormatsAsync(ido);

            // NOTE need to make sure empty formats are removed or clipboard will bark
            var empty_formats = ido.Keys.Where(x => !formats.Contains(x) || ido[x] == null || (ido[x] is string && string.IsNullOrEmpty(((string)ido[x]))));
            empty_formats.ForEach(x => ido.Remove(x));

            // use common to map everything to platform formats
            await MpAvClipboardExtensions.FinalizePlatformDataObjectAsync(ido);

            if (isDnd) {
#if MAC && false
                await ido.WriteToPasteboardAsync(true);
#endif
                return;
            }
            await MpAvClipboardExtensions.WriteToClipboardAsync(ido);
        }

        private async Task MapAllPseudoFormatsAsync(Dictionary<string, object> ido) {

            if (ido.ContainsKey(MpPortableDataFormats.Xhtml) &&
                !ido.ContainsKey(MpPortableDataFormats.Html) &&
                ido.TryGetValue(MpPortableDataFormats.Xhtml, out byte[] html_bytes)) {
                // convert html bytes to string and map to cef html
                string htmlStr = html_bytes.ToDecodedString();
                ido.AddOrReplace(MpPortableDataFormats.Html, htmlStr);
            }
            if (ido.ContainsKey(MpPortableDataFormats.Html) &&
                !ido.ContainsKey(MpPortableDataFormats.Xhtml) &&
                ido.TryGetValue(MpPortableDataFormats.Html, out string cef_html_str)) {
                // convert html sring to to bytes
                byte[] htmlBytes = cef_html_str.ToBytesFromString();
                ido.AddOrReplace(MpPortableDataFormats.Xhtml, htmlBytes);
            }

            if (ido.ContainsKey(MpPortableDataFormats.Text) &&
                !ido.ContainsKey(MpPortableDataFormats.MimeText) &&
                ido.TryGetValue(MpPortableDataFormats.Text, out string pt)) {
                // ensure cef style text is in formats
                ido.AddOrReplace(MpPortableDataFormats.MimeText, pt);
            }
            if (ido.ContainsKey(MpPortableDataFormats.MimeText) &&
                !ido.ContainsKey(MpPortableDataFormats.Text) &&
                ido.TryGetValue(MpPortableDataFormats.MimeText, out string mt)) {
                // ensure avalonia style text is in formats
                ido.AddOrReplace(MpPortableDataFormats.Text, mt);
            }

            if (OperatingSystem.IsLinux()) {
                // TODO this should only be for gnome based linux

                if (ido.ContainsKey(MpPortableDataFormats.Files) &&
                    !ido.ContainsKey(MpPortableDataFormats.LinuxFiles2) &&
                    ido.TryGetValue(MpPortableDataFormats.Files, out IEnumerable<string> files) &&
                    string.Join(Environment.NewLine, files) is string av_files_str) {
                    // ensure cef style text is in formats
                    ido.AddOrReplace(MpPortableDataFormats.LinuxFiles2, av_files_str);
                }
                if (ido.ContainsKey(MpPortableDataFormats.LinuxFiles2) &&
                    !ido.ContainsKey(MpPortableDataFormats.Files) &&
                    ido.TryGetValue(MpPortableDataFormats.LinuxFiles2, out string gn_files_str) &&
                    gn_files_str.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) is IEnumerable<string> gn_files
                    ) {
                    // ensure avalonia style text is in formats
                    ido.AddOrReplace(MpPortableDataFormats.Files, gn_files);
                }
            } else if (OperatingSystem.IsWindows()) {
                //if (ido.ContainsKey(MpPortableDataFormats.AvPNG) &&
                //    GetData(MpPortableDataFormats.AvPNG) is string png64) {
                //    //ido.AddOrReplace(MpPortableDataFormats.AvPNG, png64.ToBytesFromBase64String());
                //}
                //if (ido.ContainsKey(MpPortableDataFormats.AvPNG) &&
                //    GetData(MpPortableDataFormats.AvPNG) is byte[] pngBytes) {
                //    //#if WINDOWS
                //    //                    //ido.AddOrReplace(MpPortableDataFormats.WinBitmap, pngBytes);
                //    //                    //ido.AddOrReplace(MpPortableDataFormats.WinDib, pngBytes);
                //    //                    SetBitmap(pngBytes);
                //    //#endif

                //}

                // TODO should pass req formats into this and only create rtf if contianed
                //if (ido.ContainsKey(MpPortableDataFormats.CefHtml) &&
                //    !ido.ContainsKey(MpPortableDataFormats.AvRtf_bytes) &&
                //    GetData(MpPortableDataFormats.CefHtml) is string htmlStr) {
                //    // TODO should check if content is csv here (or in another if?) and create rtf table 
                //    string rtf = htmlStr.ToRtfFromRichHtml();
                //    ido.AddOrReplace(MpPortableDataFormats.AvRtf_bytes, rtf.ToBytesFromString());
                //}
            }


            if (ido.TryGetValue(MpPortableDataFormats.Files, out object fn_obj)) {
                IEnumerable<string> fpl = null;
                if (fn_obj is IEnumerable<string>) {
                    fpl = fn_obj as IEnumerable<string>;
                } else if (fn_obj is string fpl_str) {
                    fpl = fpl_str.SplitNoEmpty(Environment.NewLine);
                } else {

                }
                if (fpl != null) {
                    var av_fpl = await fpl.ToAvFilesObjectAsync();
                    ido.AddOrReplace(MpPortableDataFormats.Files, av_fpl);
                    MpConsole.WriteLine($"Files set");
                }
            }
            // TODO should add unicode, oem, etc. here for greater compatibility
        }
        #endregion

        #endregion
    }
}