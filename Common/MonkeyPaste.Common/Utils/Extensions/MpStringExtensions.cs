using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MonkeyPaste.Common;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpStringExtensions {
        #region Private Variables

        private static List<string> _resourceNames;

        #endregion

        public static bool IsStringNullOrEmpty(this string str) {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsStringNullOrWhiteSpace(this string str) {
            return string.IsNullOrWhiteSpace(str);
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

        public static bool IsStringResourcePath(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            if (text.StartsWith("pack:")) {
                return true;
            }
            if (_resourceNames == null) {
                //add executing resource names
                _resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames().Select(x => x.ToLower()).ToList();
                //add shared resource names
                _resourceNames.AddRange(Assembly.GetCallingAssembly().GetManifestResourceNames().Select(x => x.ToLower()));
            }

            return _resourceNames.Contains(text.ToLower());
        }

        public static string ToLabel(this string titleCaseStr, string noneText = "") {
            // TODO when automating UI language need to parameterize low vs up case logic
            //Converts 'ThisIsALabel" to 'This Is A Label'
            string outStr = string.Empty;
            for (int i = 0; i < titleCaseStr.Length; i++) {
                if (i > 0 && titleCaseStr[i - 1] > 'Z' && titleCaseStr[i] <= 'Z') {
                    outStr += " ";
                }
                outStr += titleCaseStr[i];
            }
            if (outStr.ToLower() == "none") {
                return noneText;
            }
            return outStr;
        }

        public static string ToReadableTimeSpan(this DateTime dt) {
            int totalYears, totalMonths, totalWeeks, totalDays, totalHours, totalMinutes;

            var ts = DateTime.Now - dt;
            string outStr = string.Empty;
            totalYears = (int)(ts.TotalDays / 365);
            totalMonths = DateTime.Now.MonthDifference(dt);
            totalWeeks = DateTime.Now.WeekDifference(dt);
            totalDays = (int)ts.TotalDays;
            totalHours = (int)ts.TotalHours;
            totalMinutes = (int)ts.TotalMinutes;

            if (totalYears > 1) {
                return string.Format($"{totalYears} years ago");
            }
            if (totalMonths >= 1) {
                return string.Format($"{totalMonths} month{(totalMonths == 1 ? string.Empty : "s")} ago");
            }
            if (totalWeeks >= 1) {
                return string.Format($"{totalWeeks} week{(totalWeeks == 1 ? string.Empty : "s")} ago");
            }
            if (totalDays >= 1) {
                return string.Format($"{totalDays} day{(totalDays == 1 ? string.Empty : "s")} ago");
            }
            if (totalHours >= 1) {
                return string.Format($"{totalHours} hour{(totalHours == 1 ? string.Empty : "s")} ago");
            }
            if (totalMinutes >= 1) {
                return string.Format($"{totalMinutes} minute{(totalMinutes == 1 ? string.Empty : "s")} ago");
            }
            return "Less than a minute ago";
        }

        public static int WeekDifference(this DateTime lValue, DateTime rValue) {
            double weeks = (lValue - rValue).TotalDays / 7;
            return (int)weeks;
        }

        public static int MonthDifference(this DateTime lValue, DateTime rValue) {
            return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
        }

        public static string ToTitleCase(this string str) {
            TextInfo textInfo = new CultureInfo(CultureInfo.CurrentCulture.Name, false).TextInfo;
            return textInfo.ToTitleCase(str);
        }


        public static bool ContainsByCase(this string str, string compareStr, bool isCaseSensitive) {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(compareStr)) {
                return false;
            }
            if (isCaseSensitive) {
                return str.Contains(compareStr);
            }
            return str.ToLowerInvariant().Contains(compareStr.ToLowerInvariant());
        }

        public static async Task<string> CheckSum(this string str) {
            string result = await Task<string>.Run(() => {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
                    string hash = BitConverter.ToString(
                        md5.ComputeHash(
                            Encoding.UTF8.GetBytes(str))).Replace("-", String.Empty);
                    return hash;
                }
            });
            return result;
        }

        public static string ToBase64String(this byte[] bytes) {
            if (bytes == null) {
                return string.Empty;
            }
            return Convert.ToBase64String(bytes);
        }
        public static string SerializeToJsonByteString(this object obj) {
            if (obj == null) {
                return string.Empty;
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));
        }

        public static string ToCsv(this List<string> strList) {
            if(strList == null || strList.Count == 0) {
                return string.Empty;
            }
            return string.Join(",", strList);

            //using (var mem = new MemoryStream())
            //using (var writer = new StreamWriter(mem))
            //using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) {
            //    Delimiter = ",",
            //})) {
            //    foreach (var str in strList) {
            //        csvWriter.WriteField(str);
            //        csvWriter.NextRecord();
            //    }
            //    writer.Flush();
            //    return Encoding.UTF8.GetString(mem.ToArray());
            //}
        }
        public static List<string> ToListFromCsv(this string csvStr) {
            return csvStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //List<string> result = new List<string>();
            //string value;
            //using (var strStream = new StreamReader(csvStr.ToStream(), Encoding.Default)) {
            //    using (var csv = new CsvReader(
            //        strStream,
            //        new CsvConfiguration(CultureInfo.InvariantCulture) {
            //            MissingFieldFound = null, Delimiter=","
            //        })) {
            //        while (csv.Read()) {
            //            for (int i = 0; csv.TryGetField<string>(i, out value); i++) {
            //                result.Add(value);
            //            }
            //        }
            //    }
            //}
            //return result;
        }

        public static TEnum ToEnum<TEnum>(this object obj) where TEnum: struct {
            if(obj != null) {
                try {
                    if (obj is string str) {
                        if (Enum.TryParse<TEnum>(str, true, out TEnum result)) {
                            return result;
                        }
                    } else {
                        var eobj = Enum.ToObject(typeof(TEnum), obj);
                        if (eobj != null) {
                            return (TEnum)eobj;
                        }
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                }
            }
            return default;
        }

        public static string RemoveSpecialCharacters(this string str) {
            //return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
            return MpRegEx.RegExLookup[MpRegExType.HasSpecialCharacters]
                          .Replace(str, string.Empty);
        }

        public static bool HasSpecialCharacters(this string str) {
            //return Regex.IsMatch(str, "[^a-zA-Z0-9_.]+", RegexOptions.Compiled);
            return MpRegEx.RegExLookup[MpRegExType.HasSpecialCharacters]
                          .IsMatch(str);
        }

        public static string ToPrettyPrintJson(this string jsonStr) {
            JToken jt = JToken.Parse(jsonStr);
            return jt.ToString();
        }

        public static bool HasInvalidFileNameChars(this string filename) {
            return Path.GetInvalidFileNameChars().Any(x => filename.Contains(x));
        }
        public static string RemoveInvalidFileNameChars(this string filename) {
            return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
        }
        public static string ReplaceInvalidFileNameChars(string filename, string replacementText = "_") {
            return string.Join(replacementText, filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string ToPrettyPrintXml(this string xmlStr) {
            try {
                XDocument doc = XDocument.Parse(xmlStr);
                return doc.ToString();
            }
            catch (Exception) {
                // Handle and throw if fatal exception here; don't just ignore them
                return xmlStr;
            }
        }
             
        public static string Escape(this string badString) {
            return badString.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace(">", "&gt;").Replace("<", "&lt;");
        }

        public static bool HasGuid(this string str) {
            return str.ParseGuid() != null;
        }

        public static string ParseGuid(this string str) {
            var m = MpRegEx.RegExLookup[MpRegExType.Guid].Match(str);
            if(m.Success) {
                return m.Value;
            }
            return null;
        }

        public static bool IsFile(this string str) {
            return File.Exists(str);
        }

        public static bool IsDirectory(this string str) {
            return Directory.Exists(str);
        }

        public static bool IsFileOrDirectory(this string str) {
            return str.IsFile() || str.IsDirectory();
        }

        public static List<int> IndexListOfAll(this string text, string str, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase) {List<int> allIndexOf = new List<int>();
            int index = text.IndexOf(str, comparisonType);
            while (index != -1) {
                allIndexOf.Add(index);
                index = text.IndexOf(str, index + 1, comparisonType);
            }
            return allIndexOf;
        }

        public static string ToMultiLineString(this StringCollection sc) {
            var sb = new StringBuilder();
            foreach (var s in sc) {
                sb.AppendLine(s);
            }
            return sb.ToString();
        }

        public static byte[] ToByteArray(this string str) {
            if(!str.IsStringBase64()) {
                return new byte[] { };
            }

            var bytes = Convert.FromBase64String(str);
            return bytes;
        }

        public static string ToStringFromBase64(this string str) {
            var bytes = Convert.FromBase64String(str);
            var text = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return text;
        }

        public static string ExpandEnvVars(this string text) {
            string envTokenRegExStr = @"%\w*%{1}";
            var mc = Regex.Matches(text, envTokenRegExStr);
            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        string expEnvVarStr = Environment.ExpandEnvironmentVariables(c.Value);
                        if (!string.IsNullOrEmpty(expEnvVarStr)) {
                            text = text.Replace(c.Value, expEnvVarStr);
                        }
                    }
                }
            }
            return text;
        }

        public static bool IsStringMayContainEnvVars(this string text) {
            string envTokenRegExStr = @"%\w*%{1}";
            return Regex.IsMatch(text, envTokenRegExStr);
        }

        public static string[] ToArray(this StringCollection sc) {
            if (sc == null || sc.Count == 0) {
                return new string[0];
            }
            string[] strArray = new string[sc.Count];
            try {
                sc.CopyTo(strArray, 0);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(ex);
                return new string[0];
            }
            return strArray;
        }


        public static StringCollection ToStringCollection(this IEnumerable<string> strings) {
            var stringCollection = new StringCollection();
            foreach (string s in strings) {
                stringCollection.Add(s);
            }
            return stringCollection;
        }

        public static bool IsStringHexColor(this string str) {
            if(string.IsNullOrWhiteSpace(str)) {
                return false;
            }
            return MpRegEx.RegExLookup[MpRegExType.HexColor].IsMatch(str);
        }

        public static bool IsStringNamedColor(this string str) {
            if (string.IsNullOrWhiteSpace(str)) {
                return false;
            }
            return MpSystemColors.X11ColorNames.Contains(str.ToLower());
        }

        public static string NamedColorToHex(this string str) {
            if (!IsStringNamedColor(str)) {
                throw new Exception($"'{str}' is not an X11 color name sowwy");
            }
            var propInfo = typeof(MpSystemColors).GetProperty(str.ToLower());
            return propInfo.GetValue(null) as string;
        }

        public static bool IsStringBase64(this string str) {
            // Check that the length is a multiple of 4 characters
            //Check that every character is in the set A - Z, a - z, 0 - 9, +, / except for padding at the end which is 0, 1 or 2 '=' characters

            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            if (str.Length % 4 != 0 || str.Length < 100) {
                return false;
            }
            return !MpRegEx.RegExLookup[MpRegExType.Is_NOT_Base64Str].IsMatch(str);
        }

        public static bool IsStringBase64_FromException(this string str) {
            if(string.IsNullOrWhiteSpace(str)) {
                return false;
            }
            try {
                // If no exception is caught, then it is possibly a base64 encoded string
                byte[] data = Convert.FromBase64String(str);
                // The part that checks if the string was properly padded to the
                // correct length was borrowed from d@anish's solution
                return (str.Replace(" ", "").Length % 4 == 0);
            }
            catch (FormatException) {
                // If exception is caught, then it is not a base64 encoded string

                return false;
            }
        }

        public static bool IsStringWindowsFileOrPathFormat(this string path) {
            if(string.IsNullOrWhiteSpace(path) || path.Length < 3) {
                return false;
            }
            if (!MpRegEx.RegExLookup[MpRegExType.StartsWithWindowsStyleDirectory].IsMatch(path.Substring(0, 3))) {
                return false;
            }

            if (MpRegEx.RegExLookup[MpRegExType.ContainsInvalidFileNameChar].IsMatch(path.Substring(3, path.Length - 3))) {
                return false;
            }
            return true;
        }

        public static bool IsStringHtmlText(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            str = str.ToLower();
            foreach (var quillTag in QuillTagNames) {
                if (str.Contains($"</{quillTag}>")) {
                    return true;
                }
            }
            return false;
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


        public static bool IsStringRichTextFileItem(this string text) {
            if (text == null) {
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

        public const string AlphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const string OtherChars = @"`~!@#$%^*()_-+[{]}\|;':,<./";
        private static string _passwordChars = null;
        public static string PasswordChars {
            get {
                if (_passwordChars == null) {
                    var sb = new StringBuilder();
                    for (int i = char.MinValue; i <= char.MaxValue; i++) {
                        char c = Convert.ToChar(i);
                        if (!char.IsControl(c)) {
                            sb.Append(c);
                        }
                    }
                    _passwordChars = sb.ToString();
                }
                return _passwordChars;
            }
        }

        public static string GetRandomString(int length, string chars = AlphaNumericChars) {
            return new string(Enumerable.Repeat(chars, length).Select(s => s[MpRandom.Rand.Next(s.Length)]).ToArray());
        }

        public static string GetNewAccessToken() {
            return GetRandomString(MpRandom.Rand.Next(20, 50), AlphaNumericChars);
        }

        public static string[] QuillInlineTagNames {
            get {
                return new string[] {
                    "#text",
                    "span",
                    "a",
                    "em",
                    "strong",
                    "u",
                    "s",
                    "sub",
                    "sup",
                    "img"
                };
            }
        }

        public static string[] QuillBlockTagNames {
            get {
                return new string[] {
                    "p",
                    "ol",
                    "ul",
                    "li",
                    "div",
                    "table",
                    "colgroup",
                    "col",
                    "tbody",
                    "tr",
                    "td",
                    "iframe",
                    "blockquote"
                };
            }
        }

        public static string[] QuillTagNames {
            get {
                var allTags = new List<string>();
                allTags.AddRange(QuillBlockTagNames);
                allTags.AddRange(QuillInlineTagNames);
                return allTags.ToArray();
            }
        }
    }
}
