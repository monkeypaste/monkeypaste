using HtmlAgilityPack;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common {

    public static class MpCsvRichHtmlTableConverter {
        public const string RICH_HTML_TABLE_PREFIX = "<div class='quill-better-table-wrapper'>";
        public static string RichHtmlTableToCsv(string richHtmlTableStr, MpCsvFormatProperties csvProps = null) {
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;

            var sb = new StringBuilder();
            //string outCsv = string.Empty;
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(richHtmlTableStr);

            var rows = htmlDoc.DocumentNode.SelectNodes("tr");
            foreach (var rowNode in rows) {
                var cols = rowNode.SelectNodes("td");
                foreach (var colNode in cols) {
                    string colText = colNode.InnerText;
                    sb.Append(colText + csvProps.EocSeparator);
                }
                sb.Append(csvProps.EorSeparator);
            }

            string outCsv = sb.ToString();
            return outCsv;
        }

        public static string CsvToRichHtmlTable(string csvStr, MpCsvFormatProperties csvProps = null) {
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;
            csvStr = csvStr == null ? string.Empty : csvStr;

            string tableBodyHtmlStr = @"<tbody>";
            List<double> colWidths = new List<double>();
            var csvRows = csvStr.Split(new string[] { csvProps.EorSeparator }, StringSplitOptions.None).ToList();
            for (int r = 0; r < csvRows.Count; r++) {
                string csvRowStr = csvRows[r];
                if (r == csvRows.Count - 1) {
                    MpDebug.Break();
                    if (string.IsNullOrEmpty(csvRowStr) || csvRowStr == MpCsvFormatProperties.EXCEL_EOF_MARKER) {
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
                    if (colWidths.Count <= c) {
                        // new column def
                        colWidths.Add(cur_estimated_col_width);
                    } else if (colWidths[c] < cur_estimated_col_width) {
                        colWidths[c] = cur_estimated_col_width;
                    }

                    string curCellHtmlStr = string.Format(@"<td data-row='{0}' rowspan='1' colspan='1'>{1}</td>", r + 1, csvColStr);
                    curRowCellHtmlStr += curCellHtmlStr;

                }
                string curRowHtmlStr = string.Format(@"<tr data-row='{0}'>{1}</tr>", r + 1, curRowCellHtmlStr);
                tableBodyHtmlStr += curRowHtmlStr;
            }
            tableBodyHtmlStr += "</tbody>";

            string colGroupHtml = string.Format(@"<colgroup>{0}</colgroup>", string.Join(string.Empty, colWidths.Select(x => string.Format(@"<col width='{0}px'>", x))));
            double tableWidth = colWidths.Sum();

            string tableHtmlStr = string.Format(
                @"{0}<table class='quill-better-table' style='width: {1}px'>{2}{3}</table></div>",
                RICH_HTML_TABLE_PREFIX,
                tableWidth,
                colGroupHtml,
                tableBodyHtmlStr);

            return tableHtmlStr;
        }
    }
}
