using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace AltOleHandler {
    public static class AltParamProcessor {
        public static string CurImageExtVal { get; private set; } = "png";
        public static object ProcessParam(
            MpParameterRequestItemFormat pkvp,
            string format,
            object data,
            out Exception ex,
            out List<MpPluginUserNotificationFormat> nfl) {
            ex = null;
            nfl = null;
            if (data == null || pkvp == null) {
                // already omitted
                return data;
            }
            string paramVal = pkvp.value;
            try {
                // NOTE by internal convention 'paramId' is an int.
                // plugin creator has to manage mapping internally
                AltOleParamType paramType = pkvp.paramId.ToEnum<AltOleParamType>();
                switch (format) {
                    case MpPortableDataFormats.Rtf:
                        switch (paramType) {
                            case AltOleParamType.RICHTEXTFORMAT_R_MAXCHARCOUNT: {
                                    if (data is string rtf) {
                                        int max_length = int.Parse(paramVal);
                                        if (rtf.Length > max_length) {
                                            data = rtf.Substring(0, max_length);
                                        };
                                    }
                                }

                                break;
                            case AltOleParamType.RICHTEXTFORMAT_R_IGNORE:
                                if (paramVal.ParseOrConvertToBool(false) is bool ignoreRtf &&
                                    ignoreRtf) {
                                    data = null;

                                } else {
                                    return data;
                                }
                                break;
                            case AltOleParamType.RICHTEXTFORMAT_R_TOHTML: {
                                    if (data is string rtf &&
                                        rtf.RtfToHtml() is string html &&
                                        html.ToBytesFromString() is byte[] html_bytes) {
                                        return
                                            new object[] {
                                                data,
                                                new object[] {
                                                    MpPortableDataFormats.Xhtml,
                                                    html_bytes } };
                                    }
                                }
                                break;
                        }
                        break;
                    case MpPortableDataFormats.Text:
                        switch (paramType) {
                            case AltOleParamType.TEXT_R_MAXCHARCOUNT: {
                                    if (data is string text) {
                                        int max_length = int.Parse(paramVal);
                                        if (text.Length > max_length) {
                                            data = text.Substring(0, max_length);
                                        }
                                    }
                                }

                                break;
                            case AltOleParamType.TEXT_R_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignText &&
                                    ignText) {
                                        data = null;

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
                            case AltOleParamType.TEXT_W_MAXCHARCOUNT: {
                                    if (data is string text) {
                                        int max_length = paramVal.ParseOrConvertToInt(int.MaxValue);
                                        if (text.Length > max_length) {
                                            data = text.Substring(0, max_length);
                                        }
                                    }
                                }

                                break;
                            case AltOleParamType.TEXT_W_IGNORE: {
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

                            case AltOleParamType.PNG_R_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignImg &&
                                    ignImg) {
                                        data = null;
                                    }
                                }

                                break;
                            case AltOleParamType.PNG_W_EXPORTTYPE: {
                                    if (!string.IsNullOrWhiteSpace(paramVal)) {
                                        // NOTE used for file creation
                                        CurImageExtVal = paramVal;
                                    }
                                }


                                break;
                            case AltOleParamType.PNG_W_IGNORE: {
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
                            case AltOleParamType.FILES_R_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignore_fd &&
                                        ignore_fd) {
                                        data = null;
                                    }
                                }

                                break;
                            case AltOleParamType.FILES_R_IGNOREEXTS: {
                                    if (!string.IsNullOrWhiteSpace(paramVal) &&
                                    paramVal.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value) is List<string> iel &&
                                    data is IEnumerable<string> fpl) {
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

                            case AltOleParamType.FILES_W_IGNORE: {
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
                ex = e;
            }
            return data;
        }
    }
}