using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvStringExtensions {
        public static string ToCsv(this string str) {
            return str;
        }
        public static string ToPlainText(this string str) {
            return str;
        }

        public static string ToRichText(this string str) {
            return str;
        }
        public static string ToContentRichText(this string str) {
            return str;
        }
        public static string ToRichTextTable(this string str) {
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
