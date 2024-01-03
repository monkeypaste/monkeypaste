using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common.Plugin {
    public interface MpICustomCsvFormat {
        MpCsvFormatProperties CsvFormat { get; }
    }
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
            //return IsValueBase64 ? value.ToBase64String(ValueEncoding) : value;
            return IsValueBase64 ?
                Convert.ToBase64String(Encoding.UTF8.GetBytes(value)) : value;
        }

        public string DecodeValue(string value) {
            if (IsValueBase64) {
                //if (!value.IsStringBase64()) {
                //    // predefined values may not be encoded..
                //    return value;
                //}
                //return value.ToStringFromBase64(ValueEncoding);
                return Encoding.UTF8.GetString(Convert.FromBase64String(value));
            }
            return value;
        }
    }
    public static class MpPluginExtensions {
        #region Csv
        public static string ToCsv(this IEnumerable<string> strList, MpICustomCsvFormat csvObj) {
            return ToCsv(strList, csvObj.CsvFormat);
        }
        public static string ToCsv(this IEnumerable<string> strList, MpCsvFormatProperties csvProps = null) {
            if (strList == null || !strList.Any()) {
                return string.Empty;
            }
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;
            return
                string.Join(
                    csvProps.EocSeparator,
                    strList.Select(x => csvProps.EncodeValue(x)));
        }
        public static List<string> ToListFromCsv(this string csvStr, MpICustomCsvFormat csvObj) {
            return ToListFromCsv(csvStr, csvObj.CsvFormat);
        }
        public static List<string> ToListFromCsv(this string csvStr, MpCsvFormatProperties csvProps = null) {
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;
            return
                csvStr.Split(new string[] { csvProps.EocSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => csvProps.DecodeValue(x))
                .ToList();
        }

        public static string AddCsvItem(this string csvStr, string item, bool allowDup = true, MpCsvFormatProperties csvProps = null) {
            csvProps = csvProps ?? MpCsvFormatProperties.Default;
            var items = csvStr.ToListFromCsv(csvProps);
            if (allowDup || !items.Contains(item)) {
                items.Add(item);
            }
            return items.ToCsv(csvProps);
        }

        public static string RemoveCsvItem(this string csvStr, string item, bool removeAll = true, MpCsvFormatProperties csvProps = null) {
            csvProps = csvProps ?? MpCsvFormatProperties.Default;
            var items = csvStr.ToListFromCsv(csvProps);
            List<string> to_remove = new();
            foreach (var curitem in items) {
                if (curitem == item && (removeAll || !to_remove.Any())) {
                    to_remove.Add(curitem);
                }
            }
            to_remove.ForEach(x => items.Remove(x));
            return items.ToCsv(csvProps);
        }

        #endregion



        public static DateTime ParseOrConvertToDateTime(this object obj, object fallback = null) {
            if (obj == default) {
                if (obj == fallback) {
                    return default;
                }
                return fallback.ParseOrConvertToDateTime(fallback);
            }
            if (obj == fallback) {
                if (fallback == null) {
                    return default;
                }
            }
            if (obj is DateTime dt) {
                return dt;
            }

            // stored as ticks
            long ticks = obj.ParseOrConvertToLong(fallback);
            return new DateTime(ticks);
        }
        public static TimeSpan ParseOrConvertToTimeSpan(this object obj, object fallback = null) {
            if (obj == default) {
                if (obj == fallback) {
                    return default;
                }
                return fallback.ParseOrConvertToTimeSpan(fallback);
            }
            if (obj == fallback) {
                if (fallback == null) {
                    return default;
                }
            }
            // stored as ticks
            long ticks = obj.ParseOrConvertToLong(fallback);
            return TimeSpan.FromTicks(ticks);
        }
        public static double ParseOrConvertToDouble(this object obj, object fallback = null) {
            if (obj == null) {
                if (obj == fallback) {
                    return 0;
                }
                return fallback.ParseOrConvertToDouble(fallback);
            }
            if (obj == fallback) {
                if (fallback == null) {
                    return 0;
                }
            }
            if (obj is double dblObj) {
                return dblObj;
            }
            if (obj is float fltObj) {
                return (double)fltObj;
            }

            if (obj is int intObj) {
                return (double)intObj;
            }

            if (obj is byte byteObj) {
                return (double)byteObj;
            }
            if (obj is bool boolObj) {
                return boolObj ? 1 : 0;
            }

            if (obj is TimeSpan tsObj) {
                return (double)tsObj.Ticks;
            }
            if (obj is string strObj) {
                if (string.IsNullOrEmpty(strObj)) {
                    if (obj == fallback) {
                        return 0;
                    }
                    return fallback.ParseOrConvertToDouble(fallback);
                }
                if (double.TryParse(strObj, out double dblVal)) {
                    return dblVal;
                }
                if (obj == fallback) {
                    return 0;
                }
                return fallback.ParseOrConvertToDouble(fallback);
            }
            Console.WriteLine($"Unknown obj type '{obj.GetType()}', cannot convert double. Returning 0");
            Debugger.Break();
            return 0;
        }
        public static long ParseOrConvertToLong(this object obj, object fallback = null) {
            if (obj == null) {
                if (obj == fallback) {
                    return 0;
                }
                return fallback.ParseOrConvertToLong(fallback);
            }
            if (obj == fallback) {
                if (fallback == null) {
                    return 0;
                }
            }
            if (obj is long lngObj) {
                return lngObj;
            }
            if (obj is TimeSpan ts) {
                return ts.Ticks;
            }
            if (obj is DateTime dt) {
                return dt.Ticks;
            }
            if (obj is string strObj && long.TryParse(strObj, out long lngVal)) {
                return lngVal;
            }

            return obj.ParseOrConvertToInt(fallback);
        }

        public static int ParseOrConvertToInt(this object obj, object fallback = null) {
            if (obj == null) {
                if (obj == fallback) {
                    return 0;
                }
                return fallback.ParseOrConvertToInt(fallback);
            }
            if (obj == fallback) {
                if (fallback == null) {
                    return 0;
                }
            }

            if (obj is int intObj) {
                return intObj;
            }
            if (obj is char charObj) {
                return (int)char.GetNumericValue(charObj);
            }
            if (obj is double dblObj) {
                return (int)dblObj;
            }
            if (obj is float fltObj) {
                return (int)fltObj;
            }
            if (obj is byte byteObj) {
                return (int)byteObj;
            }
            if (obj is bool boolObj) {
                return boolObj ? 1 : 0;
            }
            if (obj is TimeSpan tsObj) {
                return (int)tsObj.Ticks;
            }
            if (obj is long lngObj) {
                return (int)lngObj;
            }
            if (obj is string strObj) {
                return (int)obj.ParseOrConvertToDouble(fallback);
            }

            Console.WriteLine($"Unknown obj type '{obj.GetType()}', cannot convert int. Returning 0");
            Debugger.Break();
            return 0;
        }


        public static bool ParseOrConvertToBool(this object obj, object fallback = null) {
            if (obj == null) {
                if (obj == fallback) {
                    return false;
                }
                return fallback.ParseOrConvertToBool(fallback);
            }
            if (obj == fallback) {
                if (fallback == null) {
                    return false;
                }
            }
            if (obj is bool boolObj) {
                return boolObj;
            }
            if (obj is string strObj) {
                if (string.IsNullOrEmpty(strObj)) {
                    if (obj == fallback) {
                        return false;
                    }
                    return fallback.ParseOrConvertToBool(fallback);
                }
                if (strObj == "0") {
                    return false;
                }
                if (strObj == "1") {
                    return true;
                }
                if (bool.TryParse(strObj, out bool boolVal)) {
                    return boolVal;
                }

                if (obj == fallback) {
                    return false;
                }
                return fallback.ParseOrConvertToBool(fallback);
            }

            Console.WriteLine($"Unknown obj type '{obj.GetType()}', cannot convert bool. Returning 0");
            Debugger.Break();
            return false;
        }

        private static MpParameterRequestItemFormat ValidateGet(MpPluginParameterRequestFormat req, object paramId) {
            if (paramId == null) {
                throw new NullReferenceException("paramId is null, must have value");
            }
            if (req == null ||
                req.items == null) {
                throw new NullReferenceException($"Request error, no data");
            }

            var kvp = req.items.FirstOrDefault(x => x.paramId.ToString().Equals(paramId.ToString()));
            if (kvp == null) {
                throw new NullReferenceException($"param '{paramId}' not found");
            }
            return kvp;
        }
        public static bool GetRequestParamBoolValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value.ParseOrConvertToBool();
        }

        public static int GetRequestParamIntValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value.ParseOrConvertToInt();
        }

        public static double GetRequestParamDoubleValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value.ParseOrConvertToDouble();
        }

        public static string GetRequestParamStringValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value;
        }

        public static List<string> GetRequestParamStringListValue(this MpPluginParameterRequestFormat req, object paramId) {
            var kvp = ValidateGet(req, paramId);
            return kvp.value.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value);
        }

        public static bool HasParam(this MpPluginParameterRequestFormat req, object paramId) {
            if (req == null || req.items == null || req.items.All(x => !x.paramId.Equals(paramId))) {
                return false;
            }
            return true;
        }
    }
}
