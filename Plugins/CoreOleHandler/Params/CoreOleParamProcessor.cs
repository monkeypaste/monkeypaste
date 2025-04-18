﻿using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace CoreOleHandler {
    public static class CoreOleParamProcessor {
        #region Private Variables
        #endregion

        public static object ProcessParam(
            MpParameterRequestItemFormat paramInfo,
            string format,
            object data,
            Dictionary<string, object> all_source_data,
            Dictionary<string, object> all_target_data,
            MpOlePluginRequest req,
            bool allow_null_data,
            out Dictionary<string, object> convData,
            out Exception ex,
            out List<MpUserNotification> ntfl) {
            convData = null;
            ex = null;
            ntfl = null;

            if (data == null || paramInfo == null) {
                if(allow_null_data) {
                    // must be a pseudo format
                } else {
                    // already omitted
                    return data;
                }
            }
            IEnumerable<string> all_formats = all_source_data.Select(x => x.Key);
            string paramVal = paramInfo.paramValue;
            try {
                // NOTE by internal convention 'paramId' is an int.
                // plugin creator has to manage mapping internally
                CoreOleParamType paramType = paramInfo.paramId.ToEnum<CoreOleParamType>();
                switch (format) {
                    case var _ when format == MpPortableDataFormats.Rtf:
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
                                        data is string rtf) {
                                        string par_tag_name = OperatingSystem.IsWindows() ?
                                            req.GetParamValue(CoreOleParamType.RICHTEXTFORMAT_R_HTMLPARTAGNAME.ToString()) :
                                            "p";
                                        string html = rtf.RtfToHtml(par_tag_name);
                                        convData = new() {
                                            { MpPortableDataFormats.Html, html },
                                            { MpPortableDataFormats.INTERNAL_RTF_TO_HTML_FORMAT, true } };
                                        return data;
                                    }
                                }
                                break;
                        }
                        break;
                    case var _ when format == MpPortableDataFormats.Xhtml:
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
                                        html_str.ToRtfFromHtmlFragment() is { } rtf) {
                                        convData = new() {
                                            { MpPortableDataFormats.Rtf, rtf },
                                            { MpPortableDataFormats.INTERNAL_HTML_TO_RTF_FORMAT, true } };
                                        return data;
                                    }
                                }
                                break;
                        }
                        break;
                    case var _ when format == MpPortableDataFormats.Html:
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
                                        html_str.HtmlToRtf() is { } rtf) {
                                        convData = new() {
                                            { MpPortableDataFormats.Rtf, rtf },
                                            { MpPortableDataFormats.INTERNAL_HTML_TO_RTF_FORMAT, true } };
                                        return data;
                                    }
                                }
                                break;
                        }
                        break;
                    case var _ when format == MpPortableDataFormats.Text:
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
                    case var _ when format == MpPortableDataFormats.MimeText:
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
                    case var _ when format == MpPortableDataFormats.Image:
                        switch (paramType) {
                            case CoreOleParamType.PNG_W_FROMTEXTFORMATS: {
                                    // text->image
                                    if(data != null) {
                                        // avoid replacing any present images
                                        break;
                                    }
                                    // select highest fidelity text format that is available
                                    string text_format_to_conv =
                                        all_formats
                                        .Where(x => MpDataFormatRegistrar.IsTextFormat(x) is true)
                                        .OrderByDescending(x => MpDataFormatRegistrar.SortedTextFormats.IndexOf(x))
                                        .FirstOrDefault();
                                    if(!all_source_data.TryGetValue(text_format_to_conv,out string text_data)) {
                                        break;
                                    }
                                    text_data = text_data.ToRichHtmlDocument(text_format_to_conv);
                                    data = HtmlRender.RenderToImage(text_data).ToBase64String();
                                    break;
                                }
                                
                            case CoreOleParamType.PNG_W_ASCIIART: {
                                    // Image->text
                                    if (all_formats.Any(x => MpDataFormatRegistrar.IsPlainTextFormat(x) is true)) {
                                        // already has text ignore ascii art
                                        break;
                                    }
                                    if (data is not string base64 || base64.ToAvBitmap() is not { } bmp) {
                                        // no image or corrupt
                                        break;
                                    }
                                    // add ascii plain text to conv_results
                                    convData = new() {
                                    { MpPortableDataFormats.Text, bmp.ToAsciiImage() }
                                };

                                    break;
                                }
                                
                            case CoreOleParamType.PNG_R_SCALEOVERSIZED: {
                                    // NOTE this also handles maxw,maxh,scale,empty since they are dependant and for perf
                                    if (data is not string base64 || base64.ToAvBitmap() is not { } bmp) {
                                        break;
                                    }
                                    bool ignore_empty = req.GetParamValue<bool>(CoreOleParamType.PNG_R_IGNORE_EMPTY.ToString());
                                    bool do_scale = paramVal.ParseOrConvertToBool(false);

                                    double max_w = req.GetParamValue<double>(CoreOleParamType.PNG_R_MAXW.ToString());
                                    double max_h = req.GetParamValue<double>(CoreOleParamType.PNG_R_MAXH.ToString());

                                    MpSize bmp_size = new MpSize(bmp.Size.Width,bmp.Size.Height);
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
                            case CoreOleParamType.PNG_W_IGNORE: {
                                    if (paramVal.ParseOrConvertToBool(false) is bool ignImg &&
                                    ignImg) {
                                        data = null;
                                    }
                                }

                                break;
                        }
                        break;
                    case var _ when format == MpPortableDataFormats.Files:
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
                                        break;
                                    }
                                    if(data != null || !allow_null_data) {
                                        break;
                                    }
                                    // get highest priority format/ext with processed data
                                    var pseudo_file_format_ext_kvp =
                                        all_target_data
                                        .Where(x => x.Value is not null && CoreOleParamBuilder.IsPseudoFileFormat(x.Key))
                                        .Select(x => (x.Key, req.GetParamValue<int>(CoreOleParamBuilder.GetParamId(x.Key, false, "filepriority"))))
                                        .Where(x => x.Item2 > 0)
                                        .OrderBy(x => x.Item2)
                                        .Select(x => (x.Key, req.GetParamValue(CoreOleParamBuilder.GetParamId(x.Key, false, "fileext"))))
                                        .FirstOrDefault();
                                    if(pseudo_file_format_ext_kvp.IsDefault()) {
                                        // none available
                                        break;
                                    }
                                    var format_data = all_target_data.TryGetValue(pseudo_file_format_ext_kvp.Key, out byte[] bytes);
                                    if(bytes is null || bytes.Length == 0) {
                                        MpConsole.WriteLine($"Error conv '{pseudo_file_format_ext_kvp.Key}' to file data. Data is '{all_target_data[pseudo_file_format_ext_kvp.Key]}'");
                                        break;
                                    }
                                    string fn = null;
                                    if (all_source_data.TryGetValue(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, out string title)) {
                                        fn = title;
                                    }
                                    if (string.IsNullOrWhiteSpace(fn)) {
                                        fn = Resources.UntitledLabel;
                                    }
                                    string fe = pseudo_file_format_ext_kvp.Item2;
                                    string output_path = 
                                        MpFileIo.GetUniqueFileOrDirectoryPath(
                                            force_name: $"{fn}.{fe}");
                                    output_path = MpFileIo.WriteByteArrayToFile(output_path, bytes);
                                    // set data to string[] to be conv to IStorageItem in post-process
                                    data = new[] { output_path };

                                    break;
                                }

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
            if(!Debugger.IsAttached) {
                return string.Empty;
            }
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