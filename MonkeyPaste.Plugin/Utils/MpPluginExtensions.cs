using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MonkeyPaste.Plugin {
    public static class MpPluginExtensions {
        public static string SerializeToJsonByteString(this object obj) {
            if(obj == null) {
                return string.Empty;
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));
        }

        public static bool AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, TValue value) {
            //returns true if kvp was added
            //returns false if kvp was replaced
            if (d.ContainsKey(key)) {
                d[key] = value;
                return false;
            }
            d.Add(key, value);
            return true;
        }

        #region Csv

        public static List<string> ToListFromCsv(this string csvStr) {
            List<string> result = new List<string>();
            string value;
            using (var strStream = new StreamReader(csvStr.ToStream(), Encoding.Default)) {
                using (var csv = new CsvReader(strStream, CultureInfo.InvariantCulture)) {
                    while (csv.Read()) {
                        for (int i = 0; csv.TryGetField<string>(i, out value); i++) {
                            result.Add(value);
                        }
                    }
                }
            }
            return result;
        }

        public static string ToCsv(this List<string> strList) {
            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(mem))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) {
                Delimiter = ",",
            })) {
                foreach (var str in strList) {
                    csvWriter.WriteField(str);
                    csvWriter.NextRecord();
                }
                writer.Flush();
                return Encoding.UTF8.GetString(mem.ToArray());
            }
        }



        #endregion

        public static Stream ToStream(this string value) {
            return value.ToStream(Encoding.UTF8);
        }

        public static Stream ToStream(this string value, System.Text.Encoding encoding) {
            var bytes = encoding.GetBytes(value);
            return new MemoryStream(bytes);
        }

        public static string RemoveLastLineEnding(this string str) {
            if (str.EndsWith(Environment.NewLine)) {
                return str.Substring(0, str.Length - Environment.NewLine.Length);
            }
            return str;
        }

        public static string TrimTrailingLineEndings(this string str) {
            return str.TrimEnd(System.Environment.NewLine.ToCharArray());
        }
    }
}
