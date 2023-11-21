using System;
#if WINDOWS

using MonkeyPaste.Common.Wpf;
using TheArtOfDev.HtmlRenderer.Avalonia;

#endif
namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvStringExtensions {
        public static string ToHtmlImageDoc(this string imgBase64) {
            return $"<html><head></head><body><img src=\"data:image/png;base64,{imgBase64}\"></body></html>";
        }
        public static bool IsAvResourceString(this string str) {
            if (str.IsNullOrEmpty()) {
                return false;
            }
            return str.ToLower().StartsWith("avares://");
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

#if WINDOWs
            string qhtml = MonkeyPaste.Common.Wpf.MpWpfRtfToHtmlConverter2.ConvertFormatToHtml(str, strFormat);
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
    }
}
