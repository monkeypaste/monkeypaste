using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MonkeyPaste;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static MpWpfApp.MpWpfRichDocumentExtensions;

namespace MpWpfApp {
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
            if (IsStringRichText(text)) {
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
                    double flt = MpHelpers.Rand.NextDouble();
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

        public static bool IsStringRichTextFileItem(this string text) {
            if(text == null) {
                return false;
            }
            return text.Contains("shppict") && text.Contains("pngblip") && text.Contains("HYPERLINK");
        }

        public static bool IsStringRichTextImage(this string text) {
            if (text == null) {
                return false;
            }
            return !text.IsStringRichTextFileItem() && text.Contains("shppict") && text.Contains("pngblip");
        }

        public static bool IsStringRichTextTable(this string text) {
            if (!text.IsStringRichText()) {
                return false; 
            }
            string rtfTableCheckToken = @"{\trowd";
            return text.IndexOf(rtfTableCheckToken) >= 0;
        }

        public static bool IsStringCsv(this string text) {
            if (string.IsNullOrEmpty(text) || IsStringRichText(text)) {
                return false;
            }
            return text.Contains(",");
        }

        public static bool IsStringRichText(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"{\rtf");
        }

        public static bool IsStringXaml(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=") || text.StartsWith(@"<Span xmlns=");
        }

        public static bool IsStringSpan(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Span xmlns=");
        }

        public static bool IsStringSection(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=");
        }

        public static bool IsStringQuillText(this string text) {
            if(MpQuillFormatProperties.QuillTagNames.Any(x=>text.StartsWith("<"+x))) {
                return true;
            }
            return false;
        }

        public static bool IsStringPlainText(this string text) {
            //returns true for csv
            if (text == null) {
                return false;
            }
            if (text == string.Empty) {
                return true;
            }
            if (IsStringRichText(text) || IsStringSection(text) || IsStringSpan(text) || IsStringXaml(text)) {
                return false;
            }
            return true;
        }

        public static string ToRichTextTable(this string csvStr) {
            if (string.IsNullOrEmpty(csvStr) || !csvStr.IsStringCsv()) {
                return csvStr;
            }
            //return new MpCsvReader(csvStr).FlowDocument.ToRichText();
            return MpCsvToRtfTableConverter.GetFlowDocument(csvStr).ToRichText();
        }

        public static string ToQuillText(this string text) {
            if (text.IsStringQuillText()) {
                return text;
            }
            return MpRtfToHtmlConverter.ConvertRtfToHtml(text.ToRichText());
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


        public static string CombineRichText(string from, string to, bool insertNewLine = false) {
            return CombineFlowDocuments(
                from.ToFlowDocument(),
                to.ToFlowDocument(),null,
                insertNewLine).ToRichText();
        }

        
    }
}
