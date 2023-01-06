using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common {
    public class MpCsvFormatProperties {
        public static string EXCEL_EOF_MARKER = "\0"; // tested in excel 365 on windows csv terminates with \0 on a trailing line
        public static string CSV_DEFAULT_COLUMN_SEPARATOR = ",";
        public static string CSV_DEFAULT_ROW_SEPARATOR = Environment.NewLine;

        public static double CSV_DEFAULT_FORMATTED_FONT_SIZE = 14.0d;
        public static string CSV_DEFAULT_FORMATTED_FONT_FAMILY = "Consolas";

        public static MpCsvFormatProperties Default => new MpCsvFormatProperties();
        public static MpCsvFormatProperties DefaultBase64Value => new MpCsvFormatProperties() { IsValueBase64 = true };

        public string EocSeparator { get; set; } = CSV_DEFAULT_COLUMN_SEPARATOR;
        public string EorSeparator { get; set; } = CSV_DEFAULT_ROW_SEPARATOR;

        public double FormattedFontSize { get; set; } = CSV_DEFAULT_FORMATTED_FONT_SIZE;
        public string FormattedFontFamily { get; set; } = CSV_DEFAULT_FORMATTED_FONT_FAMILY;

        public bool IsValueBase64 { get; set; } = false;

        public Encoding ValueEncoding { get; set; } = null; // default/null resolves to UTF-8 (or tentatively based on locale)

        public string EncodeValue(string value) {
            return IsValueBase64 ? value.ToBase64String(ValueEncoding) : value;
        }

        public string DecodeValue(string value) {
            if(IsValueBase64) {
                if(!value.IsStringBase64()) {
                    // predefined values may not be encoded..
                    return value;
                }
                return value.ToStringFromBase64(ValueEncoding);
            }
            return value;
        }

    }
    public static class MpCsvToRichHtmlTableConverter {

       public static string RichHtmlTableToCsv(string richHtmlTableStr, MpCsvFormatProperties csvProps = null) {
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;

            var sb = new StringBuilder();
            //string outCsv = string.Empty;
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(richHtmlTableStr);

            var rows = htmlDoc.DocumentNode.SelectNodes("tr");
            foreach(var rowNode in rows) {
                var cols = rowNode.SelectNodes("td");
                foreach(var colNode in cols) {
                    string colText = colNode.InnerText;
                    sb.Append(colText + csvProps.EocSeparator);
                }
                sb.Append(csvProps.EorSeparator);
            }

            string outCsv = sb.ToString();
            return outCsv;
        }

        public static string CreateRichHtmlTableFromCsv(string csvStr, MpCsvFormatProperties csvProps = null) {
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;
            csvStr = csvStr == null ? string.Empty : csvStr;

            string tableBodyHtmlStr = @"<tbody>";
            List<double> colWidths = new List<double>();
            var csvRows = csvStr.Split(new string[] { csvProps.EorSeparator }, StringSplitOptions.None).ToList();
            for (int r = 0; r < csvRows.Count; r++) {
                string csvRowStr = csvRows[r];
                if (r == csvRows.Count - 1) {
                    Debugger.Break();
                    if(string.IsNullOrEmpty(csvRowStr) || csvRowStr == MpCsvFormatProperties.EXCEL_EOF_MARKER) {
                        // ignoring trailing end line
                        continue;
                    }
                }
                var csvCols = csvRowStr.Split(new string[] { csvProps.EocSeparator }, StringSplitOptions.None);
                string curRowCellHtmlStr = string.Empty;

                for (int c = 0; c < csvCols.Length; c++) {
                    // get cell text
                    string csvColStr = csvCols[c];

                    // est cell width and update column def if larger than current
                    double cur_estimated_col_width = csvColStr.Length * csvProps.FormattedFontSize;                    
                    if(colWidths.Count <= c) {
                        // new column def
                        colWidths.Add(cur_estimated_col_width);
                    } else if (colWidths[c] < cur_estimated_col_width){
                        colWidths[c] = cur_estimated_col_width;
                    }

                    string curCellHtmlStr = string.Format(@"<td data-row='{0}' rowspan='1' colspan='1'>{1}</td>", r + 1, csvColStr);
                    curRowCellHtmlStr += curCellHtmlStr;

                }
                string curRowHtmlStr = string.Format(@"<tr data-row='{0}'>{1}</tr>",r + 1,curRowCellHtmlStr);
                tableBodyHtmlStr += curRowHtmlStr;
            }
            tableBodyHtmlStr += "</tbody>";

            string colGroupHtml = string.Format(@"<colgroup>{0}</colgroup>", string.Join(string.Empty,colWidths.Select(x=> string.Format(@"<col width='{0}px'>", x))));
            double tableWidth = colWidths.Sum();

            string tableHtmlStr = string.Format(
                @"<div class='quill-better-table-wrapper'><table class='quill-better-table' style='width: {0}px'>{1}{2}</table></div>",
                tableWidth,
                colGroupHtml,
                tableBodyHtmlStr);

            return tableHtmlStr;
        }
    }
}
