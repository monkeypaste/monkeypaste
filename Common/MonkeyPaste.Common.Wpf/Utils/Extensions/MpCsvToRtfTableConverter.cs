using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MonkeyPaste.Common.Wpf {
    public static class MpCsvToRtfTableConverter {
        ////from https://github.com/jcoehoorn/EasyCSV
        ////ref'd from https://stackoverflow.com/questions/1544721/reading-csv-files-in-c-sharp/1544743#1544743
        //public static IEnumerable<IList<string>> FromString(string str, bool ignoreFirstLine = false) {
        //    byte[] byteArray = Encoding.Default.GetBytes(str);
        //    //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
        //    using (MemoryStream ms = new MemoryStream(byteArray)) {
        //        using (StreamReader rdr = new StreamReader(ms)) {
        //            foreach (IList<string> item in FromReader(rdr, ignoreFirstLine)) yield return item;
        //        }
        //    }
        //}

        //public static IEnumerable<IList<string>> FromFile(string fileName, bool ignoreFirstLine = false) {
        //    using (StreamReader rdr = new StreamReader(fileName)) {
        //        foreach (IList<string> item in FromReader(rdr, ignoreFirstLine)) yield return item;
        //    }
        //}

        //public static IEnumerable<IList<string>> FromStream(Stream csv, bool ignoreFirstLine = false) {
        //    using (var rdr = new StreamReader(csv)) {
        //        foreach (IList<string> item in FromReader(rdr, ignoreFirstLine)) yield return item;
        //    }
        //}

        //public static IEnumerable<IList<string>> FromReader(TextReader csv, bool ignoreFirstLine = false) {
        //    if (ignoreFirstLine) csv.ReadLine(); 

        //    IList<string> result = new List<string>();

        //    StringBuilder curValue = new StringBuilder();
        //    char c;
        //    c = (char)csv.Read();
        //    while (csv.Peek() != -1) {
        //        switch (c) {
        //            case ',': //empty field
        //                result.Add("");
        //                c = (char)csv.Read();
        //                break;
        //            case '"': //qualified text
        //            case '\'':
        //                char q = c;
        //                c = (char)csv.Read();
        //                bool inQuotes = true;
        //                while (inQuotes && csv.Peek() != -1) {
        //                    if (c == q) {
        //                        c = (char)csv.Read();
        //                        if (c != q)
        //                            inQuotes = false;
        //                    }

        //                    if (inQuotes) {
        //                        curValue.Append(c);
        //                        c = (char)csv.Read();
        //                    }
        //                }
        //                result.Add(curValue.ToString());
        //                curValue = new StringBuilder();
        //                if (c == ',') c = (char)csv.Read(); // either ',', newline, or endofstream
        //                break;
        //            case '\n': //end of the record
        //            case '\r':
        //                //potential bug here depending on what your line breaks look like
        //                if (result.Count > 0) // don't return empty records
        //                {
        //                    yield return result;
        //                    result = new List<string>();
        //                }
        //                c = (char)csv.Read();
        //                break;
        //            default: //normal unqualified text
        //                while (c != ',' && c != '\r' && c != '\n' && csv.Peek() != -1) {
        //                    curValue.Append(c);
        //                    c = (char)csv.Read();
        //                }
        //                result.Add(curValue.ToString());
        //                curValue = new StringBuilder();
        //                if (c == ',') c = (char)csv.Read(); //either ',', newline, or endofstream
        //                break;
        //        }

        //    }
        //    if (curValue.Length > 0) //potential bug: I don't want to skip on a empty column in the last record if a caller really expects it to be there
        //        result.Add(curValue.ToString());
        //    if (result.Count > 0)
        //        yield return result;
        //}

        public static string GetCsv(string str) {
            //var sb = new StringBuilder();
            string outStr = string.Empty;
            var fd = str.ToFlowDocument();

            foreach(var block in fd.Blocks) {
                if(block is Table t) {
                    foreach(var rowGroup in t.RowGroups) {
                        foreach(var row in rowGroup.Rows) {
                            foreach(var c in row.Cells) {
                                outStr += c.ToPlainText() + ",";
                            }
                            outStr += Environment.NewLine;
                        }
                    }
                }
            }

            return outStr;
        }

        public static Table LoadTable(this TextRange tr, string csv) {
            tr.Text = string.Empty;
            var fd = tr.Start.Parent.FindParentOfType<FlowDocument>();
            var table = CreateTable(csv);
            
            fd.Blocks.InsertAfter(tr.Start.Paragraph, table);
            return table;
        }

        public static FlowDocument GetFlowDocument(string csv) {
            FlowDocument fd = new FlowDocument();
            var table = CreateTable(csv);
            fd.Blocks.Add(table);
            //add a blank trailing paragraph for merging in case table goes outside tile bounds
            fd.Blocks.Add(new Paragraph());
            return fd;
        }

        private static Table CreateTable(string csvStr) {
            csvStr = csvStr == null ? string.Empty : csvStr;

            var csvRows = csvStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            List<List<string>> csvCells = csvRows.Select(x => x.Split(new string[] { "," }, StringSplitOptions.None).ToList()).ToList();

            var table = new Table();

            //table.CellSpacing = double.NaN;
            table.Background = Brushes.White;

            var tableRowGroup = new TableRowGroup();
            table.RowGroups.Add(tableRowGroup);

            int totalColumns = csvCells.Max(x => x.Count);
            for (int c = 0; c < totalColumns; c++) {
                table.Columns.Add(new TableColumn());
            }

            for (int r = 0; r < csvCells.Count; r++) {
                TableRow row = new TableRow();

                row.Background = Brushes.White;
                row.FontSize = 16;

                for (int rc = 0; rc < totalColumns; rc++) {
                    string val = string.Empty;
                    if (rc < csvCells[r].Count) {
                        val = csvCells[r][rc].ToString();
                    }
                    var tableCell = new TableCell(new Paragraph(new Run(val)));
                    tableCell.BorderBrush = Brushes.LightGray;
                    tableCell.BorderThickness = new Thickness(1);
                    row.Cells.Add(tableCell);
                }

                tableRowGroup.Rows.Add(row);
            }

            AutoResizeColumns(table);
            return table;
        }
        static void AutoResizeColumns(Table table) {
            TableColumnCollection columns = table.Columns;
            TableRowCollection rows = table.RowGroups[0].Rows;
            TableCellCollection cells;
            TableRow row;
            TableCell cell;

            int columnCount = columns.Count;
            int rowCount = rows.Count;
            int cellCount = 0;

            double[] columnWidths = new double[columnCount];
            double columnWidth;

            // loop through all rows
            for (int r = 0; r < rowCount; r++) {
                row = rows[r];
                cells = row.Cells;
                cellCount = cells.Count;

                // loop through all cells in the row    
                for (int c = 0; c < columnCount && c < cellCount; c++) {
                    cell = cells[c];
                    columnWidth = GetDesiredWidth(new TextRange(cell.ContentStart, cell.ContentEnd)) + 19;

                    if (columnWidth > columnWidths[c]) {
                        columnWidths[c] = columnWidth;
                    }
                }
            }

            // set the columns width to the widest cell
            for (int c = 0; c < columnCount; c++) {
                columns[c].Width = new GridLength(columnWidths[c]);
            }
        }
        static double GetDesiredWidth(TextRange textRange) {
            return new FormattedText(
                textRange.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    textRange.GetPropertyValue(TextElement.FontFamilyProperty) as FontFamily,
                    (FontStyle)textRange.GetPropertyValue(TextElement.FontStyleProperty),
                    (FontWeight)textRange.GetPropertyValue(TextElement.FontWeightProperty),
                    FontStretches.Normal),
                    (double)textRange.GetPropertyValue(TextElement.FontSizeProperty),
                Brushes.Black,
                new NumberSubstitution(),
                MpScreenInformation.ThisAppDip).Width;
        }
    }
}
