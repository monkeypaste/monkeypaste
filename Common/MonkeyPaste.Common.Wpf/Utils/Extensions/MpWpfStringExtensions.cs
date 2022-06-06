using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Wpf {
    public static class MpWpfStringExtensions {
        public static int GetColCount(string text) {
            int maxCols = int.MinValue;
            foreach (string row in text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
                if (row.Length > maxCols) {
                    maxCols = row.Length;
                }
            } 
            return maxCols;
        }

        public static int GetRowCount(string text) {
            if (string.IsNullOrEmpty(text)) {
                return 0;
            }
            if (text.IsStringRichText()) {
                int nlCount = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                int parCount = text.Split(new string[] { @"\par" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                if (nlCount + parCount == 0) {
                    return 1;
                }
                return nlCount + parCount;
            }
            return text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Length;
        }

        public static string GetRandomString(int charsPerLine = 32, int lines = 1) {
            StringBuilder str_build = new StringBuilder();

            for (int i = 0; i < lines; i++) {
                for (int j = 0; j < charsPerLine; j++) {
                    double flt = MpRandom.Rand.NextDouble();
                    int shift = Convert.ToInt32(Math.Floor(25 * flt));
                    char letter = Convert.ToChar(shift + 65);
                    str_build.Append(letter);
                }
                if (i + 1 < lines) {
                    str_build.Append('\n');
                }
            }
            return str_build.ToString();
        }

        public static string RemoveSpecialCharacters(string str) {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", string.Empty, RegexOptions.Compiled);
        }

        public static string ToCsv(this string str) {
            if (string.IsNullOrWhiteSpace(str)) {
                return str == null ? string.Empty : str;
            }
            return MpCsvToRtfTableConverter.GetCsv(str);
        }

        public static string ToPlainText(this string str) {
            if (str == null) {
                return string.Empty;
            }
            if(str.IsStringBase64()) {
                return str.ToBitmapSource().ToAsciiImage();
            }
            if (str.IsStringPlainText()) {
                // NOTE plain text implies file or directory path
                return str;
            }
            return str.ToFlowDocument().ToPlainText();
        }

        public static string EscapeExtraOfficeRtfFormatting(this string str) {
            string extraFormatToken = @"{\*\themedata";
            int tokenIdx = str.IndexOf(extraFormatToken);
            if (tokenIdx >= 0) {
                str = str.Substring(0, tokenIdx);
            }
            return str;
        }

        public static string EscapeExtraOfficeHTMLFormatting(this string str) {
            string extraFormatToken = @"{\*\themedata";
            int tokenIdx = str.IndexOf(extraFormatToken);
            if (tokenIdx >= 0) {
                str = str.Substring(0, tokenIdx);
            }
            return str;
        }                
    }
}
