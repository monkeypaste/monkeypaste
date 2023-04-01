using Avalonia;
using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace AvCoreClipboardHandler {
    public static class AvCoreClipboardWriter {
        // manifest writer formats (NOTE not used but to keep track of references)
        public static string[] AvWriterFormats = new string[]{
            MpPortableDataFormats.Text,
            MpPortableDataFormats.CefText,
            MpPortableDataFormats.AvRtf_bytes,
            MpPortableDataFormats.AvHtml_bytes,
            MpPortableDataFormats.CefHtml,
            MpPortableDataFormats.LinuxSourceUrl,
            MpPortableDataFormats.AvPNG,
            MpPortableDataFormats.AvFileNames,
            MpPortableDataFormats.LinuxGnomeFiles, // only needed for write
            MpPortableDataFormats.AvCsv,
            MpPortableDataFormats.LinuxUriList
        };

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

                needs_pseudo_file = true;
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

            if (request.writeToClipboard) {
                //await Util.WaitForClipboard();
                await Application.Current.Clipboard.SetDataObjectSafeAsync(write_output);
                //Util.CloseClipboard();
            }

            return new MpClipboardWriterResponse() {
                processedDataObject = write_output,
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
            };
        }

        private static object ProcessWriterParam(MpParameterRequestItemFormat pkvp, string format, object data, out Exception ex, out List<MpPluginUserNotificationFormat> nfl) {
            ex = null;
            nfl = null;
            if (data == null) {
                // already omitted
                return null;
            }
            try {
                CoreClipboardParamType paramType = (CoreClipboardParamType)Convert.ToInt32(pkvp.paramId);
                switch (format) {
                    case MpPortableDataFormats.Text:
                        switch (paramType) {
                            case CoreClipboardParamType.W_MaxCharCount_Text:
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
                            case CoreClipboardParamType.W_Ignore_Text:
                                data = null;
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

        #region File Pre-Processor
        private static async Task<object> PreProcessFileFormatAsync(IDataObject ido) {
            string fn = null;
            if (ido.Contains(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT)) {
                fn = ido.Get(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT) as string;
            }
            if (string.IsNullOrWhiteSpace(fn)) {
                fn = "untitled";
            }

            string source_type = null;
            if (ido.Contains(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT)) {
                source_type = ido.Get(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT) as string;
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
                if (!string.IsNullOrEmpty(pref_text_format) &&
                    GetTextFileData(ido, pref_text_format) is string text &&
                    !string.IsNullOrEmpty(text)) {
                    data_to_write = text;
                    fe = GetTextFileFormatExt(pref_text_format);
                } else if (ido.Contains(MpPortableDataFormats.AvPNG) &&
                    ido.Get(MpPortableDataFormats.AvPNG) is byte[] imgBytes &&
                    imgBytes.ToBase64String() is string imgStr) {
                    data_to_write = imgStr;
                    fe = "png";
                }
            } else if (source_type == "image") {
                if (ido.Contains(MpPortableDataFormats.AvPNG) &&
                    ido.Get(MpPortableDataFormats.AvPNG) is byte[] imgBytes &&
                    imgBytes.Length > 0 &&
                    imgBytes.ToBase64String() is string imgStr) {
                    data_to_write = imgStr;
                    fe = "png";
                } else {
                    string pref_text_format = GetPreferredTextFileFormat(ido);
                    if (!string.IsNullOrEmpty(pref_text_format) &&
                        GetTextFileData(ido, pref_text_format) is string text) {
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
            return new List<string> { output_path };
        }
        private static int GetWriterPriority(string format) {
            if (format == MpPortableDataFormats.AvFileNames) {
                // process files last
                return int.MaxValue;
            }
            return 0;
        }

        private static string GetPreferredTextFileFormat(IDataObject ido) {
            if (ido.Contains(MpPortableDataFormats.AvCsv) &&
                ido.Get(MpPortableDataFormats.AvCsv) != null) {
                return MpPortableDataFormats.AvCsv;
            }
            if (ido.Contains(MpPortableDataFormats.Text) &&
                ido.Get(MpPortableDataFormats.Text) != null) {
                return MpPortableDataFormats.Text;
            }
            if (ido.Contains(MpPortableDataFormats.CefHtml) &&
                ido.Get(MpPortableDataFormats.CefHtml) != null) {
                return MpPortableDataFormats.CefHtml;
            }
            if (ido.Contains(MpPortableDataFormats.AvRtf_bytes) &&
                ido.Get(MpPortableDataFormats.AvRtf_bytes) != null) {
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

        private static string GetTextFileData(IDataObject ido, string format) {
            if (format == MpPortableDataFormats.AvRtf_bytes &&
                ido.Get(MpPortableDataFormats.AvRtf_bytes) is byte[] rtfBytes &&
                rtfBytes.ToDecodedString() is string rtfStr) {
                return rtfStr;
            }
            if (ido.Get(format) is string dataStr) {
                return dataStr;
            }
            return null;
        }

        #endregion
    }
}