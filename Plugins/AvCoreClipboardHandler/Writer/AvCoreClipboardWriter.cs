using Avalonia;
using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace AvCoreClipboardHandler {
    public static class AvCoreClipboardWriter {
        #region Private Variables

        private static string _cur_img_ext = "png";
        //private static int _cur_img_quality = 100;

        #endregion

        #region Public Methods

        public static async Task<MpClipboardWriterResponse> PerformWriteRequestAsync(MpClipboardWriterRequest request) {
            if (request == null) {
                return null;
            }
            var ido = request.data as IDataObject;
            if (ido == null) {
                return null;
            }
            List<MpPluginUserNotificationFormat> nfl = new List<MpPluginUserNotificationFormat>();
            List<Exception> exl = new List<Exception>();
            IDataObject write_output = ido ?? new MpAvDataObject();
            var writeFormats =
                request.writeFormats
                .Where(x => ido.GetAllDataFormats().Contains(x))
                .OrderBy(x => GetWriterPriority(x));

            string source_type = null;
            if (ido.Contains(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT)) {
                source_type = ido.Get(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT) as string;
            }
            bool needs_pseudo_file = false;
            if (source_type != "FileList" &&
                request.items.FirstOrDefault(x => Convert.ToInt32(x.paramId) == (int)CoreClipboardParamType.W_IgnoreAll_FileDrop) is MpParameterRequestItemFormat prif &&
                !bool.Parse(prif.value)) {
                // when file type is enabled but source is not a file,
                // add file as a format and flag that it needs to be created
                // AFTER all runtime formats have been processed so best format is written

                //needs_pseudo_file = true;
            }

            foreach (var write_format in writeFormats) {
                object data = null;
                if (needs_pseudo_file && write_format == MpPortableDataFormats.AvFileNames) {
                    // called last 
                    data = await PreProcessFileFormatAsync(write_output);
                } else {
                    data = write_output.Get(write_format);
                }

                foreach (var param in request.items) {
                    data = ProcessWriterParam(param, write_format, data, out var ex, out var param_nfl);
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


            if (request.writeToClipboard) {
                //await Util.WaitForClipboard();
                var empty_formats = write_output.GetAllDataFormats().Where(x => !write_output.ContainsData(x));
                if (empty_formats.Any() && write_output is MpAvDataObject avdo) {
                    // NOTE need to make sure empty formats are removed or clipboard will bark
                    empty_formats.ForEach(x => avdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(x)));
                    var test = avdo.GetAllDataFormats().Where(x => !avdo.ContainsData(x));
                    if (test.Any()) {

                    }
                    await Application.Current.Clipboard.SetDataObjectSafeAsync(avdo);
                } else {
                    await Application.Current.Clipboard.SetDataObjectSafeAsync(write_output);
                }

                //Util.CloseClipboard();
            }

            return new MpClipboardWriterResponse() {
                processedDataObject = write_output,
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
            };
        }


        #endregion
        private static object ProcessWriterParam(MpParameterRequestItemFormat pkvp, string format, object data, out Exception ex, out List<MpPluginUserNotificationFormat> nfl) {
            ex = null;
            nfl = null;
            if (data == null || pkvp == null) {
                // already omitted
                return data;
            }
            string paramVal = pkvp.value;
            try {
                CoreClipboardParamType paramType = (CoreClipboardParamType)Convert.ToInt32(pkvp.paramId);
                switch (format) {
                    case MpPortableDataFormats.Text:
                        switch (paramType) {
                            case CoreClipboardParamType.W_MaxCharCount_Text:
                                if (data is string text) {
                                    int max_length = paramVal.ParseOrConvertToInt(int.MaxValue);
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
                                if (paramVal.ParseOrConvertToBool(false) is bool textImg &&
                                    textImg) {
                                    data = null;
                                }
                                break;
                        }
                        break;
                    case MpPortableDataFormats.AvPNG:
                        switch (paramType) {
                            case CoreClipboardParamType.W_Format_Image:

                                if (!string.IsNullOrWhiteSpace(paramVal)) {
                                    // NOTE used for file creation
                                    _cur_img_ext = paramVal;
                                }
                                break;
                            case CoreClipboardParamType.W_Ignore_Image:
                                if (paramVal.ParseOrConvertToBool(false) is bool ignImg &&
                                    ignImg) {
                                    data = null;
                                }
                                break;
                        }
                        break;
                    case MpPortableDataFormats.AvFileNames:
                        switch (paramType) {
                            case CoreClipboardParamType.W_IgnoreAll_FileDrop:
                                if (paramVal.ParseOrConvertToBool(false) is bool ignFiles &&
                                    ignFiles) {
                                    data = null;
                                }
                                break;
                        }
                        break;

                    case MpPortableDataFormats.LinuxGnomeFiles:
                        switch (paramType) {
                            case CoreClipboardParamType.W_IgnoreAll_FileDrop_Linux:
                                if (paramVal.ParseOrConvertToBool(false) is bool linuxFilesImg &&
                                    linuxFilesImg) {
                                    data = null;
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

        #region Private Methods

        #region File Pre-Processor
        private static async Task<object> PreProcessFileFormatAsync(IDataObject ido) {

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
                    fe = _cur_img_ext;
                }
            } else if (source_type == "image") {
                if (ido.TryGetData(MpPortableDataFormats.AvPNG, out byte[] imgBytes) &&
                    imgBytes.ToBase64String() is string imgStr) {
                    // image as image
                    data_to_write = imgStr;
                    fe = _cur_img_ext;
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
            string output_path = await data_to_write.ToFileAsync(
                                forceNamePrefix: fn,
                                forceExt: fe,
                                isTemporary: true);
            return new[] { output_path };
        }
        private static int GetWriterPriority(string format) {
            if (format == MpPortableDataFormats.AvFileNames) {
                // process files last
                return int.MaxValue;
            }
            return 0;
        }
        private static string GetPreferredTextFileFormat(IDataObject ido) {
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
        private static string GetTextFileFormatExt(string format) {
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

        private static void PostProcessImage(IDataObject ido) {
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