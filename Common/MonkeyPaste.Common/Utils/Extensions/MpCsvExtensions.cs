﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common {
    public static class MpCsvExtensions {
        public static string ToCsv(this List<string> strList, MpCsvFormatProperties csvProps = null) {
            if (strList == null || strList.Count == 0) {
                return string.Empty;
            }
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;
            return 
                string.Join(
                    csvProps.EocSeparator, 
                    strList.Select(x=>csvProps.EncodeValue(x)));
        }
        public static List<string> ToListFromCsv(this string csvStr, MpCsvFormatProperties csvProps = null) {
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;
            return 
                csvStr.SplitNoEmpty(csvProps.EocSeparator)
                .Select(x=>csvProps.DecodeValue(x))
                .ToList();
        }

        public static string CsvStrToRichHtmlTable(this string csvStr, MpCsvFormatProperties csvProps = null) {
            return MpCsvToRichHtmlTableConverter.CreateRichHtmlTableFromCsv(csvStr, csvProps);
        }

        public static string RichHtmlToCsv(this string str) {
            // (currently) this assumes csvStr is html table and down converting 
            string csvStr = MpCsvToRichHtmlTableConverter.RichHtmlTableToCsv(str);
            return csvStr;
        }
    }
}