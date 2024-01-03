
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Common {
    public static class MpCsvExtensions {


        public static string CsvStrToRichHtmlTable(this string csvStr, MpCsvFormatProperties csvProps = null) {
            return MpCsvToRichHtmlTableConverter.CreateRichHtmlTableFromCsv(csvStr, csvProps);
        }

        public static string RichHtmlToCsv(this string str) {
            // (currently) this assumes csvStr is html table and down converting 
            string csvStr = MpCsvToRichHtmlTableConverter.RichHtmlTableToCsv(str);
            return csvStr;
        }

        public static bool IsStringRichHtmlTable(this string str) {
            return str.StartsWith(MpCsvToRichHtmlTableConverter.RICH_HTML_TABLE_PREFIX);
        }
    }
}
