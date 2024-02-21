using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace CoreOleHandler {
    public static class CoreOleParamProcessor {
        public static string CurImageExtVal { get; private set; } = "png";
        public static object ProcessParam(
            MpParameterRequestItemFormat paramInfo,
            string format,
            object data,
            IEnumerable<string> all_formats,
            MpOlePluginRequest req,
            out Dictionary<string, object> convData,
            out Exception ex,
            out List<MpUserNotification> ntfl) {
            convData = null;
            ex = null;
            ntfl = null;

            if (data == null || paramInfo == null) {
                // already omitted
                return data;
            }
            string paramVal = paramInfo.paramValue;
            try {
                // NOTE by internal convention 'paramId' is an int.
                // plugin creator has to manage mapping internally
                CoreOleParamType paramType = paramInfo.paramId.ToEnum<CoreOleParamType>();
                switch (format) {
                    case MpPortableDataFormats.Rtf:
                        switch (paramType) {
                            case CoreOleParamType.RICHTEXTFORMAT_R_MAXCHARCOUNT: {
                                    if (data is string rtf) {
                                        HandleMaxNotification(ref data, ref ntfl, rtf, format, paramVal.ParseOrConvertToInt());
                                    }
                                }

                                break;
                            case CoreOleParamType.RICHTEXTFORMAT_R_IGNORE:
                                if (paramVal.ParseOrConvertToBool(false) is bool ignoreRtf &&
                                    ignoreRtf) {
                                    AddIgnoreNotification(ref ntfl, format);
                                    data = null;

                                } else {
                                    return data;
                                }
                                break;
                            case CoreOleParamType.RICHTEXTFORMAT_R_TOHTML: {
                                    if (!all_formats.Contains(MpPortableDataFormats.INTERNAL_HTML_TO_RTF_FORMAT) &&
                                        data is string rtf &&
                                        MpAvCommonTools.Services != null &&
                                        MpAvCommonTools.Services.Html2Rtf != null &&
                                        MpAvCommonTools.Services.Html2Rtf.RtfToHtml(rtf) is string html) {
                                        convData = new() {
                                            { MpPortableDataFormats.Html, html },
                                            { MpPortableDataFormats.INTERNAL_RTF_TO_HTML_FORMAT, true } };
                                        return data;
                                    }
                                }
                                break;
                        }
                        break;
                    case MpPortableDataFormats.Xhtml:
                        switch (paramType) {
                            case CoreOleParamType.HTMLFORMAT_R_MAXCHARCOUNT: {
                                    if (data is string html_str) {
                                        HandleMaxNotification(ref data, ref ntfl, html_str, format, paramVal.ParseOrConvertToInt());
                                    }
                                }

                                break;
                            case CoreOleParamType.HTMLFORMAT_R_IGNORE:
                                if (paramVal.ParseOrConvertToBool(false) is bool ignoreRtf &&
                                    ignoreRtf) {
                                    AddIgnoreNotification(ref ntfl, format);
                                    data = null;

                                } else {
                                    return data;
                                }
                                break;
                            case CoreOleParamType.HTMLFORMAT_R_TORTF: {
                                    if (!all_formats.Contains(MpPortableDataFormats.INTERNAL_RTF_TO_HTML_FORMAT) &&
                                        data is string html_str &&
                                        MpAvCommonTools.Services != null &&
                                        MpAvCommonTools.Services.Html2Rtf != null &&
                                        MpAvCommonTools.Services.Html2Rtf.HtmlFragmentToRtf(html_str) is string rtf) {
                                        convData = new() {
                                            { MpPortableDataFormats.Rtf, rtf },
                                            { MpPortableDataFormats.INTERNAL_HTML_TO_RTF_FORMAT, true } };
                                        return data;
                                    }
                                }
                                break;
                        }
                        break;
                    case MpPortableDataFormats.Html:
                        switch (paramType) {
                            case CoreOleParamType.TEXTHTML_R_MAXCHARCOUNT: {
                                    if (data is string html_str) {
                                        HandleMaxNotification(ref data, ref ntfl, html_str, format, paramVal.ParseOrConvertToInt());
                                    }
                                }

                                break;
                            case CoreOleParamType.TEXTHTML_R_IGNORE:
                                if (paramVal.ParseOrConvertToBool(false) is bool ignoreRtf &&
                                    ignoreRtf) {
                                    AddIgnoreNotification(ref ntfl, format);
                                    data = null;

                                } else {
                                    return data;
                                }
                                break;
                            case CoreOleParamType.TEXTHTML_R_TORTF: {
                                    if (!all_formats.Contains(MpPortableDataFormats.INTERNAL_RTF_TO_HTML_FORMAT) &&
                                        data is string html_str &&
                                        MpAvCommonTools.Services != null &&
                                        MpAvCommonTools.Services.Html2Rtf != null &&
                                        MpAvCommonTools.Services.Html2Rtf.RtfToHtml(html_str) is string rtf) {
                                        convData = new() {
                                            { MpPortableDataFormats.Rtf, rtf },
                                            { MpPortableDataFormats.INTERNAL_HTML_TO_RTF_FORMAT, true } };
                                        return data;
                                    }
                                }
                                break;
                        }
                        break;
                    case MpPortableDataFormats.Text:
                        switch (paramType) {
                            case CoreOleParamType.TEXT_R_MAXCHARCOUNT: {
                                    if (data is string text) {
                                        HandleMaxNotification(ref data, ref ntfl, text, format, paramVal.ParseOrConvertToInt());
                                    }
                                }

                                break;
                            case CoreOleParamType.TEXT_R_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignText &&
                                    ignText) {
                                        data = null;
                                        AddIgnoreNotification(ref ntfl, format);

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
                                }

                                break;
                            case CoreOleParamType.TEXT_W_MAXCHARCOUNT: {
                                    if (data is string text) {
                                        HandleMaxNotification(ref data, ref ntfl, text, format, paramVal.ParseOrConvertToInt(), false);
                                    }
                                }

                                break;
                            case CoreOleParamType.TEXT_W_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool textImg &&
                                    textImg) {
                                        AddIgnoreNotification(ref ntfl, format, false);
                                        data = null;
                                    }
                                }

                                break;
                        }
                        break;
                    case MpPortableDataFormats.MimeText:
                        // BUG somehow text/plain is getting converted to bytes
                        // when setting clipboard (like editor clipboard copy)
                        // so if bytes convert to text...
                        if (data is byte[] cefTextBytes &&
                            cefTextBytes.ToDecodedString() is string cefPlainText) {
                            data = cefPlainText;
                        }
                        switch (paramType) {
                            case CoreOleParamType.TEXTPLAIN_R_MAXCHARCOUNT: {
                                    if (data is string text) {
                                        HandleMaxNotification(ref data, ref ntfl, text, format, paramVal.ParseOrConvertToInt());
                                    }
                                }

                                break;
                            case CoreOleParamType.TEXTPLAIN_R_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignText && ignText) {
                                        data = null;
                                        AddIgnoreNotification(ref ntfl, format);

                                    } else {
                                        return data;
                                    }
                                }

                                break;
                            case CoreOleParamType.TEXTPLAIN_W_MAXCHARCOUNT: {
                                    if (data is string text) {
                                        HandleMaxNotification(ref data, ref ntfl, text, format, paramVal.ParseOrConvertToInt());
                                    }
                                }

                                break;
                            case CoreOleParamType.TEXT_W_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool textImg &&
                                        textImg) {
                                        data = null;
                                    }
                                }

                                break;
                        }
                        break;
                    case MpPortableDataFormats.Image:
                        switch (paramType) {
                            case CoreOleParamType.PNG_R_SCALEOVERSIZED: {
                                    // NOTE this also handles maxw,maxh,scale,empty since they are dependant and for perf
                                    if (data is not string base64 || base64.ToAvBitmap() is not { } bmp) {
                                        break;
                                    }
                                    bool ignore_empty = req.GetParamValue<bool>(CoreOleParamType.PNG_R_IGNORE_EMPTY.ToString());
                                    bool do_scale = paramVal.ParseOrConvertToBool(false);

                                    double max_w = req.GetParamValue<double>(CoreOleParamType.PNG_R_MAXW.ToString());
                                    double max_h = req.GetParamValue<double>(CoreOleParamType.PNG_R_MAXH.ToString());

                                    MpSize bmp_size = bmp.Size.ToPortableSize();
                                    MpSize adj_size = bmp_size.ResizeKeepAspect(max_w, max_h);
                                    bool needs_scale = !bmp_size.IsValueEqual(adj_size);
                                    if (!needs_scale) {
                                        // no resize needed
                                        if (ignore_empty && bmp.IsEmptyOrTransprent()) {
                                            data = null;
                                            AddIgnoreNotification(ref ntfl, format);
                                        }
                                        break;
                                    }
                                    if (!do_scale) {
                                        // too big ignore
                                        data = null;
                                        AddEmptyOrTransparentNotification(ref ntfl, format);
                                        break;
                                    }
                                    data = bmp.Resize(adj_size).ToBase64String();

                                    if (ignore_empty && bmp.IsEmptyOrTransprent()) {
                                        data = null;
                                        AddIgnoreNotification(ref ntfl, format);
                                        break;
                                    }

                                    if (adj_size.Width < bmp_size.Width) {
                                        AddMaxNotification(ref ntfl, format, (int)max_w, (int)bmp_size.Width);
                                    }
                                    if (adj_size.Height < bmp_size.Height) {
                                        AddMaxNotification(ref ntfl, format, (int)max_w, (int)bmp_size.Height);
                                    }
                                }
                                break;
                            case CoreOleParamType.PNG_R_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignImg && ignImg) {
                                        data = null;
                                        AddIgnoreNotification(ref ntfl, format);
                                    }
                                }

                                break;
                            case CoreOleParamType.PNG_W_EXPORTTYPE: {
                                    if (!string.IsNullOrWhiteSpace(paramVal)) {
                                        // NOTE used for file creation
                                        CurImageExtVal = paramVal;
                                    }
                                }


                                break;
                            case CoreOleParamType.PNG_W_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignImg &&
                                    ignImg) {
                                        data = null;
                                    }
                                }

                                break;
                        }
                        break;
                    case MpPortableDataFormats.Files:
                        switch (paramType) {
                            case CoreOleParamType.FILES_R_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignore_fd &&
                                        ignore_fd) {
                                        data = null;
                                        AddIgnoreNotification(ref ntfl, format);
                                    }
                                }

                                break;
                            case CoreOleParamType.FILES_R_IGNOREEXTS: {
                                    if (!string.IsNullOrWhiteSpace(paramVal) &&
                                    paramVal.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value) is List<string> iel &&
                                    data is string dataStr &&
                                    dataStr.SplitByLineBreak() is IEnumerable<string> fpl) {
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
                                }

                                break;

                            case CoreOleParamType.FILES_W_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignFiles &&
                                    ignFiles) {
                                        data = null;
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
                if (e is CoreOleException) {
                    throw;
                }
                ex = e;
            }
            return data;
        }


        private static string AddIgnoreNotification(ref List<MpUserNotification> nfl, string format, bool isReader = true) {
            return AddNotification(
                ref nfl,
                Resources.NtfFormatIgnoredTitle,
                string.Format(Resources.NtfFormatIgnoredText, format),
                isReader ? Resources.NtfReaderDetail : Resources.NtfWriterDetail);
        }

        private static string AddNotification(ref List<MpUserNotification> nfl, string title, string msg = default, string detail = default, MpPluginNotificationType ntfType = MpPluginNotificationType.PluginResponseWarning) {
#if DEBUG
            if (nfl == null) {
                nfl = new List<MpUserNotification>();
            }
            nfl.Add(new MpUserNotification() {
                NotificationType = ntfType,
                Title = title,
                Body = msg,
                Detail = detail
            });
            return msg;
#else
            return string.Empty;
#endif
        }

        private static void HandleMaxNotification(ref object data, ref List<MpUserNotification> nfl, string text, string format, int max, bool isReader = true) {
            if (text.Length < max) {
                return;
            }
            string msg = AddMaxNotification(ref nfl, format, max, text.Length);
            data = text.Substring(0, max);
            throw new CoreOleMaxLengthException(msg);
        }
        private static string AddMaxNotification(ref List<MpUserNotification> nfl, string format, int max, int actual, bool isReader = true) {
            return AddNotification(
                ref nfl,
                Resources.NtfMaxSizeTitle,
                string.Format(Resources.NtfMaxSizeText, format, max, actual),
                isReader ? Resources.NtfReaderDetail : Resources.NtfWriterDetail);

        }
        private static string AddEmptyOrTransparentNotification(ref List<MpUserNotification> nfl, string format, bool isReader = true) {
            return AddNotification(
                ref nfl,
                Resources.NtfEmptyImgTitle,
                string.Format(Resources.NtfEmptyImgText, format),
                isReader ? Resources.NtfReaderDetail : Resources.NtfWriterDetail);

        }
    }
}