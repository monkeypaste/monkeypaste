using System;
using MonkeyPaste.Common.Plugin;


#if WINDOWS

using MonkeyPaste.Common.Wpf;

#endif
namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvStringExtensions {
        
        public static bool IsAvResourceString(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            return str.ToLowerInvariant().StartsWith("avares://");
        }
        public static bool IsRichHtmlMixedMedia(this string qhtml) {
            string imgTagStartStr = @"<img src='";
            int img_tag_start_idx = qhtml.IndexOf(imgTagStartStr);
            if (img_tag_start_idx >= 0) {
                //html has an img 

                // check before img tag
                string pre_img_only_qhtml = "<p>";
                string pre_img_qhtml = qhtml.Substring(0, img_tag_start_idx);
                bool is_pre_img_only = pre_img_qhtml == pre_img_only_qhtml;

                if (is_pre_img_only) {
                    // check after img tag
                    int post_img_tag_start_idx = img_tag_start_idx + imgTagStartStr.Length;
                    string post_img_tag_start_str = qhtml.Substring(post_img_tag_start_idx);

                    if (post_img_tag_start_str.Contains(">")) {
                        //ensure img tag is closed or false positive occurs

                        // get actual end of img tag idx
                        int img_tag_end_idx = post_img_tag_start_str.IndexOf(">") + post_img_tag_start_idx;
                        string post_img_only_qhtml = "</p>";
                        string post_img_qhtml = qhtml.Substring(img_tag_end_idx);
                        bool is_post_img_only = post_img_qhtml == post_img_only_qhtml;
                        if (is_post_img_only) {
                            // qhtml is an img item and not mixed content
                            return false;
                        }
                    }

                }
                // qhtml either has text before or after an image so its mixed content and not 100% format compatible
                return true;
            }
            return false;
        }

        public static string RtfToHtml(this string str) {
            if (!str.IsStringRtf()) {
                return str;
            }

#if WINDOWS
            string qhtml = MonkeyPaste.Common.Wpf.MpWpfRtfToHtmlConverter.ConvertFormatToHtml(str);
            return qhtml;
#elif MAC
            string qhtml = MpAvMacHelpers.RtfToHtml(str);
            return qhtml;
#else
            return str;
#endif
        }


        public static string HtmlToRtf(this string str) {
            if (!str.IsStringRtf()) {
                return str;
            }
#if WINDOWS
            string rtf = MpWpfHtmlToRtfConverter.ConvertQuillHtmlToRtf(str);
            return rtf;
#elif MAC
            string rtf = MpAvMacHelpers.Html2Rtf(str);
            return rtf;
#else 
            return str;
#endif
        }

        public static string ToRtfFromHtmlFragment(this string str) {
            if (!str.IsStringRichHtml() ||
                MpRichHtmlContentConverterResult.Parse(str) is not { } hccr) {
                return str;
            }
            string rtf = HtmlToRtf(hccr.InputHtml);
            return rtf;
        }

        public static string EscapeExtraOfficeRtfFormatting(this string str) {
            string extraFormatToken = @"{\*\themedata";
            int tokenIdx = str.IndexOf(extraFormatToken);
            if (tokenIdx >= 0) {
                str = str.Substring(0, tokenIdx);
            }
            return str;
        }

        public static string ToRichHtmlDocument(this string source_data, string source_format) {
            string result = source_data;
            if (MpPortableDataFormats.IsRtfFormat(source_format) is true) {
                result = RtfToHtml(source_data);
            } else if (MpPortableDataFormats.IsCsvFormat(source_format) is true) {
                result = MpCsvRichHtmlTableConverter.CsvToRichHtmlTable(source_data).ToHtmlDocumentFromTextOrPartialHtml();
            } else if (MpPortableDataFormats.IsImageFormat(source_format) is true) {
                result = source_data.ToHtmlImageDoc();
            } else if (MpPortableDataFormats.IsFilesFormat(source_format) is true) {
                if(MpCommonTools.Services != null &&
                    MpCommonTools.Services.FilesToHtmlConverter != null &&
                    source_data.Split(new string[] {Environment.NewLine},StringSplitOptions.None) is { } paths) {
                    result = MpCommonTools.Services.FilesToHtmlConverter.ConvertToHtml(paths);
                }
            } else if (MpPortableDataFormats.IsHtmlFormat(source_format) is not true) {
                // should be some plain text format
                result = source_data.ToHtmlDocumentFromTextOrPartialHtml();
            } else {
                // assert to keep formats in order
                MpDebug.Assert(MpPortableDataFormats.IsHtmlFormat(source_format) is true, $"Warning, unhandled text format '{source_format}'");
            }
            return result;
        }
    }
}
