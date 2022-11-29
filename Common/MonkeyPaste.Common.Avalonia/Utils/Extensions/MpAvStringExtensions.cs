using HtmlAgilityPack;
using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvStringExtensions {
        public static bool IsRichHtmlMixedMedia(this string qhtml) {
            string imgTagStartStr = @"<img src='";
            int img_tag_start_idx = qhtml.IndexOf(imgTagStartStr);
            if (img_tag_start_idx >= 0) {
                //html has an img 

                // check before img tag
                string pre_img_only_qhtml = "<p>";
                string pre_img_qhtml = qhtml.Substring(0, img_tag_start_idx);
                bool is_pre_img_only = pre_img_qhtml == pre_img_only_qhtml;

                if(is_pre_img_only) {
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
                        if(is_post_img_only) {
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

        public static string ToRichHtmlText(this string str, string strFormat) {
            if(str.IsStringRichText() && OperatingSystem.IsWindows()) {
                string qhtml = MonkeyPaste.Common.Wpf.MpWpfRtfToHtmlConverter.ConvertFormatToHtml(str, strFormat);
                return qhtml;
            }  else if(str.IsStringCsv()) {
                // TODO create quill tables here

            }
            // TODO add image and file stuff here
            return str;
        }

        public static string ToContentRichText(this string str) {
            if(str.IsStringRichHtml() && OperatingSystem.IsWindows()) {
                string rtf = MpWpfHtmlToRtfConverter.ConvertQuillHtmlToRtf(str);
                return rtf;
            }
            return str;
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
