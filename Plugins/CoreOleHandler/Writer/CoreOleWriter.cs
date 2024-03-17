using Avalonia.Controls;
using Avalonia.Input;
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
            //IDataObject write_output = ido_dict == null ? new MpAvDataObject() : ido_dict.ToDataObject();
            Dictionary<string, object> write_output = ido_dict == null ? [] : ido_dict;
            var writeFormats =
                request.formats
                .Where(x => ido_dict.ContainsKey(x))
                .OrderBy(x => GetWriterPriority(x));

            string source_type = null;
            if (ido_dict.TryGetValue(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT, out object source_type_obj)) {
                source_type = source_type_obj as string;
            }
            bool needs_pseudo_file = false;
            if (source_type != "FileList" && // NOTE using 'FileList' to avoid moving MpCopyItemType into common
                request.items.FirstOrDefault(x => x.paramId.ToEnum<CoreOleParamType>() == CoreOleParamType.FILES_W_IGNORE) is MpParameterRequestItemFormat prif &&
                !bool.Parse(prif.paramValue)) {
                // when file type is enabled but source is not a file,
                // add file as a format and flag that it needs to be created
                // AFTER all runtime formats have been processed so best format is written

                needs_pseudo_file = true;
            }

            foreach (var write_format in writeFormats) {
                object data = null;
                if (needs_pseudo_file && write_format == MpPortableDataFormats.Files) {
                    // called last 
                    data = PreProcessFileFormat(write_output);
                } else {
                    if (write_output.TryGetValue(write_format, out string dataStr)) {
                        data = dataStr;
                    }
                }

                foreach (var param in request.items) {
                    data = CoreOleParamProcessor.ProcessParam(
                        paramInfo: param,
                        format: write_format,
                        data: data,
                        all_formats: writeFormats,
                        req: request,
                        convData: out _,
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
                }
                if (data == null) {
                    continue;
                }
                write_output.AddOrReplace(write_format, data);
            }

            // NOTE not sure if this is needed omitted for now
            //PostProcessImage(write_output);
            await PrepareForOutputAsync(write_output, request.isDnd, writeFormats);

            return new MpOlePluginResponse() {
                //dataObjectLookup = write_output,
                dataObjectLookup = write_output,
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
            };
        }

        #endregion

        #region Private Methods

        #region File Pre-Processor
        private object PreProcessFileFormat(Dictionary<string, object> ido) {
            string source_type = null;
            if (ido.TryGetValue(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT, out string itemType)) {
                source_type = itemType.ToLowerInvariant();
            }
            if (source_type == null) {
                source_type = "text";
            }

            string fn = null;
            if (ido.TryGetValue(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, out string title)) {
                fn = title;
            }
            if (string.IsNullOrWhiteSpace(fn)) {
                fn = Resources.UntitledLabel;
            }
            object data_to_write = null;
            string fe = null;
            // NOTE basically avoiding writing text screen shot as file or
            // image ascii and writing tabular csv when available or
            // falling back to either html or rtf when plain text isn't wanted

            if (source_type == "text") {
                string pref_text_format = GetPreferredTextFileFormat(ido);
                if (ido.TryGetValue(pref_text_format, out string text)) {
                    // text as text
                    data_to_write = text;
                    fe = GetTextFileFormatExt(pref_text_format);
                } else if (ido.TryGetValue(MpPortableDataFormats.Image, out byte[] imgBytes)) {
                    // text as image
                    data_to_write = imgBytes;
                    fe = CoreOleParamProcessor.CurImageExtVal;
                }
            } else if (source_type == "image") {
                if (ido.TryGetValue(MpPortableDataFormats.Image, out byte[] imgBytes)) {
                    // image as image
                    data_to_write = imgBytes;
                    fe = CoreOleParamProcessor.CurImageExtVal;
                } else {
                    string pref_text_format = GetPreferredTextFileFormat(ido);
                    if (ido.TryGetValue(pref_text_format, out string text)) {
                        // image as text
                        data_to_write = text;
                        fe = GetTextFileFormatExt(pref_text_format);
                    }
                }
            }
            if (data_to_write == null || string.IsNullOrEmpty(fe)) {
                return null;
            }

            string output_path = MpFileIo.GetUniqueFileOrDirectoryPath(
                force_name: $"{fn}.{fe}");

            if (data_to_write is byte[] bytes_to_write) {
                output_path = MpFileIo.WriteByteArrayToFile(output_path, bytes_to_write);
            } else if (data_to_write is string str_to_write) {
                output_path = MpFileIo.WriteTextToFile(output_path, str_to_write);
            } else {
                return null;
            }
            return new[] { output_path };
        }
        private int GetWriterPriority(string format) {
            if (format == MpPortableDataFormats.Files) {
                // process files last
                return int.MaxValue;
            }
            return 0;
        }
        private string GetPreferredTextFileFormat(Dictionary<string, object> ido) {
            if (ido.ContainsKey(MpPortableDataFormats.Csv)) {
                return MpPortableDataFormats.Csv;
            }
            if (ido.ContainsKey(MpPortableDataFormats.Text)) {
                return MpPortableDataFormats.Text;
            }
            if (ido.ContainsKey(MpPortableDataFormats.Html)) {
                return MpPortableDataFormats.Html;
            }
            if (ido.ContainsKey(MpPortableDataFormats.Rtf)) {
                return MpPortableDataFormats.Rtf;
            }
            return null;
        }
        private string GetTextFileFormatExt(string format) {
            if (format == MpPortableDataFormats.Csv) {
                return "csv";
            }
            if (format == MpPortableDataFormats.Text) {
                return "txt";
            }
            if (format == MpPortableDataFormats.Html) {
                return "html";
            }
            if (format == MpPortableDataFormats.Rtf) {
                return "rtf";
            }
            return null;
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
        private async Task PrepareForOutputAsync(Dictionary<string, object> ido, bool isDnd, IEnumerable<string> formats) {

            await MapAllPseudoFormatsAsync(ido);

            // NOTE need to make sure empty formats are removed or clipboard will bark
            var empty_formats = ido.Keys.Where(x => !formats.Contains(x) || ido[x] == null || (ido[x] is string && string.IsNullOrEmpty(((string)ido[x]))));
            empty_formats.ForEach(x => ido.Remove(x));

            if (isDnd) {
#if MAC && False
                await ido.WriteToPasteboardAsync(true);
#endif
                return;
            }
            //await clipboard.SetDataObjectAsync(ido);
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
                    !ido.ContainsKey(MpPortableDataFormats.MimeGnomeFiles) &&
                    ido.TryGetValue(MpPortableDataFormats.Files, out IEnumerable<string> files) &&
                    string.Join(Environment.NewLine, files) is string av_files_str) {
                    // ensure cef style text is in formats
                    ido.AddOrReplace(MpPortableDataFormats.MimeGnomeFiles, av_files_str);
                }
                if (ido.ContainsKey(MpPortableDataFormats.MimeGnomeFiles) &&
                    !ido.ContainsKey(MpPortableDataFormats.Files) &&
                    ido.TryGetValue(MpPortableDataFormats.MimeGnomeFiles, out string gn_files_str) &&
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