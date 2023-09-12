using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace AltOleHandler {
    public class AltOleWriter : MpIOleWriterComponent {
        #region Private Variables

        //private int _cur_img_quality = 100;

        #endregion

        #region Public Methods

        public async Task<MpOlePluginResponse> ProcessOleRequestAsync(MpOlePluginRequest request) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                return await Dispatcher.UIThread.InvokeAsync(async () => {
                    return await ProcessOleRequestAsync(request);
                });
            }
            if (request == null ||
                request.dataObjectLookup is not Dictionary<string, object> ido_dict) {
                return null;
            }
            List<MpPluginUserNotificationFormat> nfl = new List<MpPluginUserNotificationFormat>();
            List<Exception> exl = new List<Exception>();
            IDataObject write_output = ido_dict == null ? new MpAvDataObject() : ido_dict.ToDataObject();
            var writeFormats =
                request.formats
                .Where(x => ido_dict.ContainsKey(x))
                .OrderBy(x => GetWriterPriority(x));

            string source_type = null;
            if (ido_dict.TryGetValue(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT, out object source_type_obj)) {
                source_type = source_type_obj as string;
            }
            bool needs_pseudo_file = false;
            if (source_type != "FileList" &&
                request.items.FirstOrDefault(x => x.paramId.ToEnum<AltOleParamType>() == AltOleParamType.FILES_W_IGNORE) is MpParameterRequestItemFormat prif &&
                !bool.Parse(prif.value)) {
                // when file type is enabled but source is not a file,
                // add file as a format and flag that it needs to be created
                // AFTER all runtime formats have been processed so best format is written

                needs_pseudo_file = true;
            }

            foreach (var write_format in writeFormats) {
                if (write_format == "UniformResourceLocator") {

                }
                object data = null;
                if (needs_pseudo_file && write_format == MpPortableDataFormats.AvFiles) {
                    // called last 
                    data = PreProcessFileFormat(write_output);
                } else {
                    data = write_output.Get(write_format);
                }

                foreach (var param in request.items) {
                    data = AltParamProcessor.ProcessParam(param, write_format, data, out var ex, out var param_nfl);
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
                write_output.Set(write_format, data);
            }

            // NOTE not sure if this is needed omitted for now
            //PostProcessImage(write_output);

            if (!request.isDnd) {
                // for non-dnd write, set clipboard to resp
                if (write_output is MpAvDataObject avdo) {
                    var empty_formats = write_output.GetAllDataFormats().Where(x => !write_output.ContainsData(x));
                    // NOTE need to make sure empty formats are removed or clipboard will bark
                    empty_formats.ForEach(x => avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(x)));
                    var test = avdo.GetAllDataFormats().Where(x => !avdo.ContainsData(x));
                    if (test.Any()) {

                    }
                    //
                    if (avdo.TryGetData(MpPortableDataFormats.AvFiles, out object fpl_obj)) {
                        IEnumerable<string> fpl = null;
                        if (fpl_obj is IEnumerable<string>) {
                            fpl = fpl_obj as IEnumerable<string>;
                        } else if (fpl_obj is string fpl_str) {
                            fpl = fpl_str.SplitNoEmpty(Environment.NewLine);
                        } else {

                        }
                        if (fpl != null) {
                            var av_fpl = await fpl.ToAvFilesObjectAsync();
                            avdo.SetData(MpPortableDataFormats.AvFiles, av_fpl);
                        }
                    }
                }
                await AltOleHelpers.ClipboardRef.SetDataObjectSafeAsync(write_output);
            }

            return new MpOlePluginResponse() {
                dataObjectLookup = write_output.ToDictionary(),
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
            };
        }

        #endregion

        #region Private Methods

        #region File Pre-Processor
        private object PreProcessFileFormat(IDataObject ido) {

            string fn = null;
            if (ido.TryGetData<string>(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, out string title)) {
                fn = title;
            }
            if (string.IsNullOrWhiteSpace(fn)) {
                fn = "untitled";
            }

            string source_type = null;
            if (ido.TryGetData(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT, out string itemType)) {
                source_type = itemType;
            }
            if (source_type == null) {
                source_type = "text";
            } else {
                source_type = source_type.ToLower();
            }

            string data_to_write = null;
            string fe = null;
            // NOTE basically avoiding writing text screen shot as file or
            // image ascii and writing tabular csv when available or
            // falling back to either html or rtf when plain text isn't wanted

            if (source_type == "text") {
                string pref_text_format = GetPreferredTextFileFormat(ido);
                if (ido.TryGetData(pref_text_format, out string text)) {
                    // text as text
                    data_to_write = text;
                    fe = GetTextFileFormatExt(pref_text_format);
                } else if (ido.TryGetData(MpPortableDataFormats.AvPNG, out byte[] imgBytes) &&
                    imgBytes.ToBase64String() is string imgStr) {
                    // text as image
                    data_to_write = imgStr;
                    fe = AltParamProcessor.CurImageExtVal;
                }
            } else if (source_type == "image") {
                if (ido.TryGetData(MpPortableDataFormats.AvPNG, out byte[] imgBytes) &&
                    imgBytes.ToBase64String() is string imgStr) {
                    // image as image
                    data_to_write = imgStr;
                    fe = AltParamProcessor.CurImageExtVal;
                } else {
                    string pref_text_format = GetPreferredTextFileFormat(ido);
                    if (ido.TryGetData(pref_text_format, out string text)) {
                        // image as text
                        data_to_write = text;
                        fe = GetTextFileFormatExt(pref_text_format);
                    }
                }
            }
            if (string.IsNullOrEmpty(data_to_write) || string.IsNullOrEmpty(fe)) {
                return null;
            }
            string output_path = data_to_write.ToFile(
                                forceNamePrefix: fn,
                                forceExt: fe,
                                isTemporary: true);
            return new[] { output_path };
        }
        private int GetWriterPriority(string format) {
            if (format == MpPortableDataFormats.AvFiles) {
                // process files last
                return int.MaxValue;
            }
            return 0;
        }
        private string GetPreferredTextFileFormat(IDataObject ido) {
            if (ido.ContainsData(MpPortableDataFormats.AvCsv)) {
                return MpPortableDataFormats.AvCsv;
            }
            if (ido.ContainsData(MpPortableDataFormats.Text)) {
                return MpPortableDataFormats.Text;
            }
            if (ido.ContainsData(MpPortableDataFormats.CefHtml)) {
                return MpPortableDataFormats.CefHtml;
            }
            if (ido.ContainsData(MpPortableDataFormats.AvRtf_bytes)) {
                return MpPortableDataFormats.AvRtf_bytes;
            }
            return null;
        }
        private string GetTextFileFormatExt(string format) {
            if (format == MpPortableDataFormats.AvCsv) {
                return "csv";
            }
            if (format == MpPortableDataFormats.Text) {
                return "txt";
            }
            if (format == MpPortableDataFormats.CefHtml) {
                return "html";
            }
            if (format == MpPortableDataFormats.AvRtf_bytes) {
                return "rtf";
            }
            return null;
        }

        #endregion

        #region Windows Image Post-processor

        private void PostProcessImage(IDataObject ido) {
#if WINDOWS
            if (ido.TryGetData(MpPortableDataFormats.AvPNG, out byte[] pngBytes)) {
                object dib = MonkeyPaste.Common.Wpf.MpWpfClipoardImageHelper.GetWpfDib(pngBytes);
                ido.Set(MpPortableDataFormats.WinDib, dib);

                object bmp = MonkeyPaste.Common.Wpf.MpWpfClipoardImageHelper.GetSysDrawingBitmap(pngBytes);
                ido.Set(MpPortableDataFormats.WinBitmap, bmp);
            }

#endif
        }
        #endregion

        #endregion
    }
}