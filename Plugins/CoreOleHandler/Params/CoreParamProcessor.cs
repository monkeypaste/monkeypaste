using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace CoreOleHandler {
    public static class CoreParamProcessor {
        public static string CurImageExtVal { get; private set; } = "png";
        public static object ProcessParam(
            MpParameterRequestItemFormat paramInfo,
            string format,
            object data,
            IEnumerable<string> all_formats,
            out Dictionary<string, object> convData,
            out Exception ex,
            out List<MpPluginUserNotificationFormat> ntfl) {
            convData = null;
            ex = null;
            ntfl = null;

            if (data == null || paramInfo == null) {
                // already omitted
                return data;
            }
            string paramVal = paramInfo.value;
            try {
                // NOTE by internal convention 'paramId' is an int.
                // plugin creator has to manage mapping internally
                CoreOleParamType paramType = paramInfo.paramId.ToEnum<CoreOleParamType>();
                switch (format) {
                    case MpPortableDataFormats.Rtf:
                        switch (paramType) {
                            case CoreOleParamType.RICHTEXTFORMAT_R_MAXCHARCOUNT: {
                                    if (data is string rtf) {
                                        int max_length = int.Parse(paramVal);
                                        if (rtf.Length > max_length) {
                                            ntfl = new List<MpPluginUserNotificationFormat>() {
                                            Util.CreateNotification(
                                                MpPluginNotificationType.PluginResponseWarning,
                                                "Max Char Count Reached",
                                                $"{format} limit is '{max_length}' and data was '{rtf.Length}'",
                                                "CoreClipboardWriter")};
                                            data = rtf.Substring(0, max_length);

                                            throw new CoreOleMaxLengthException($"Text limit is '{max_length}' and data was '{rtf.Length}'");
                                        }
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
                                        rtf.RtfToHtml() is string html) {
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
                                        int max_length = int.Parse(paramVal);
                                        if (html_str.Length > max_length) {
                                            ntfl = new List<MpPluginUserNotificationFormat>() {
                                            Util.CreateNotification(
                                                MpPluginNotificationType.PluginResponseWarning,
                                                "Max Char Count Reached",
                                                $"{format} limit is '{max_length}' and data was '{html_str.Length}'",
                                                "CoreClipboardWriter")};
                                            data = html_str.Substring(0, max_length);
                                            throw new CoreOleMaxLengthException($"Text limit is '{max_length}' and data was '{html_str.Length}'");
                                        }
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
                                        html_str.ToRtfFromHtmlFragment() is string rtf) {
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
                                        int max_length = int.Parse(paramVal);
                                        if (html_str.Length > max_length) {
                                            ntfl = new List<MpPluginUserNotificationFormat>() {
                                            Util.CreateNotification(
                                                MpPluginNotificationType.PluginResponseWarning,
                                                "Max Char Count Reached",
                                                $"{format} limit is '{max_length}' and data was '{html_str.Length}'",
                                                "CoreClipboardWriter")};
                                            data = html_str.Substring(0, max_length);
                                            throw new CoreOleMaxLengthException($"Text limit is '{max_length}' and data was '{html_str.Length}'");
                                        }
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
                                        html_str.HtmlToRtf() is string rtf) {
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
                                        int max_length = int.Parse(paramVal);
                                        if (text.Length > max_length) {
                                            ntfl = new List<MpPluginUserNotificationFormat>() {
                                            Util.CreateNotification(
                                                MpPluginNotificationType.PluginResponseWarning,
                                                    "Max Char Count Reached",
                                                    $"Text limit is '{max_length}' and data was '{text.Length}'",
                                                    "CoreClipboardWriter")
                                            };
                                            data = text.Substring(0, max_length);
                                            throw new CoreOleMaxLengthException($"Text limit is '{max_length}' and data was '{text.Length}'");
                                        }
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
                                        int max_length = paramVal.ParseOrConvertToInt(int.MaxValue);
                                        if (text.Length > max_length) {
                                            ntfl = new List<MpPluginUserNotificationFormat>() {
                                            Util.CreateNotification(
                                                MpPluginNotificationType.PluginResponseWarning,
                                                "Max Char Count Reached",
                                                $"Text limit is '{max_length}' and data was '{text.Length}'",
                                                "CoreClipboardWriter")
                                        };
                                            data = text.Substring(0, max_length);

                                            throw new CoreOleMaxLengthException($"Text limit is '{max_length}' and data was '{text.Length}'");
                                        }
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
                                        int max_length = int.Parse(paramVal);
                                        if (text.Length > max_length) {
                                            ntfl = new List<MpPluginUserNotificationFormat>() {
                                            Util.CreateNotification(
                                                MpPluginNotificationType.PluginResponseWarning,
                                                "Max Char Count Reached",
                                                $"{format} limit is '{max_length}' and data was '{text.Length}'",
                                                "CoreClipboardWriter")
                                        };
                                            data = text.Substring(0, max_length);

                                            throw new CoreOleMaxLengthException("Max Length");
                                        }
                                    }
                                }

                                break;
                            case CoreOleParamType.TEXTPLAIN_R_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignText &&
                                    ignText) {
                                        data = null;
                                        AddIgnoreNotification(ref ntfl, format);

                                    } else {
                                        return data;
                                    }
                                }

                                break;
                            case CoreOleParamType.TEXTPLAIN_W_MAXCHARCOUNT: {
                                    if (data is string text) {
                                        int max_length = paramVal.ParseOrConvertToInt(int.MaxValue);
                                        if (text.Length > max_length) {
                                            ntfl = new List<MpPluginUserNotificationFormat>() {
                                            Util.CreateNotification(
                                                MpPluginNotificationType.PluginResponseWarning,
                                                "Max Char Count Reached",
                                                $"{format} limit is '{max_length}' and data was '{text.Length}'",
                                                "CoreClipboardWriter")
                                        };
                                            data = text.Substring(0, max_length);

                                            throw new CoreOleMaxLengthException("Max Length");
                                        }
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

                            case CoreOleParamType.PNG_R_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignImg &&
                                    ignImg) {
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

        private static void AddIgnoreNotification(ref List<MpPluginUserNotificationFormat> nfl, string format) {
#if DEBUG
            if (nfl == null) {
                nfl = new List<MpPluginUserNotificationFormat>();
            }
            nfl.Add(Util.CreateNotification(
                MpPluginNotificationType.PluginResponseWarning,
                "Format Ignored",
                $"{format} Format is flagged as 'ignored'",
                "CoreClipboardWriter"));
#else
            return;
#endif
        }
    }
}