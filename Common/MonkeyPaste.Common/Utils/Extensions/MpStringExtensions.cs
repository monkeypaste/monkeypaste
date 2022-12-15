using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MonkeyPaste.Common;
using System.Globalization;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Web;

namespace MonkeyPaste.Common {
    public static class MpStringExtensions {
        #region Private Variables

        private static List<string> _resourceNames;

        #endregion

        #region Encoding Extensions
        public static string ToDecodedString(this byte[] bytes, Encoding enc = null) {
            // TODO should use local encoding here
            if(enc == null) {
                bytes.DetectTextEncoding(out string decodedStr);
                return decodedStr;
            }
            return enc.GetString(bytes);
        }

        public static byte[] ToBytesFromString(this string str, Encoding enc = null) {
            // NOTE str intended to be text not base64
            // TODO should use local encoding here
            enc = enc == null ? Encoding.UTF8 : enc;
            return enc.GetBytes(str);
        }
        public static byte[] ToBytesFromBase64String(this string str, Encoding enc = null) {
            // NOTE intended for str to be base64
            if (!str.IsStringBase64()) {
                return str.ToBytesFromString(enc);
            }

            var bytes = Convert.FromBase64String(str);
            return bytes;
        }

        public static string ToStringFromBase64(this string str, Encoding enc = null) {
            var bytes = Convert.FromBase64String(str);
            var text = bytes.ToDecodedString(enc);
            return text;
        }

        public static string ToBase64String(this string str, Encoding enc = null) {
            return str.ToBytesFromString(enc).ToBase64String();
        }

        #endregion

        #region Converters

        public static string ToRichHtmlTable(this string csvStr, MpCsvFormatProperties csvProps = null) {
            return MpCsvToRichHtmlTableConverter.CreateRichHtmlTableFromCsv(csvStr, csvProps);
        }

        public static string ToCsv(this string str) {
            // (currently) this assumes csvStr is html table and down converting 
            string csvStr = MpCsvToRichHtmlTableConverter.RichHtmlTableToCsv(str);
            return csvStr;
        }
        public static string ToPlainText(this string text, string sourceFormat = "") {
            sourceFormat = sourceFormat.ToLower();
            if (sourceFormat == "text") {
                return text;
            }
            if (text.IsStringRichHtml()) {
                return MpRichHtmlToPlainTextConverter.Convert(text);
            }
            
            return text;
        }        

        #endregion

        public static string AppendData(this string str, string data, string dataFormat, bool insertNewLine) {
            if (str.IsStringRichHtml()) {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(str);
                var textNode = htmlDoc.CreateTextNode(data);
                var textSpan = htmlDoc.CreateElement("span");
                textSpan.AppendChild(textNode);
                if(insertNewLine) {
                    var textPar = htmlDoc.CreateElement("p");
                    textPar.AppendChild(textSpan);
                    htmlDoc.DocumentNode.AppendChild(textPar);
                } else {
                    htmlDoc.DocumentNode.LastChild.AppendChild(textSpan);
                }
                return htmlDoc.DocumentNode.OuterHtml;
            } else {
                return str + (insertNewLine ? Environment.NewLine : string.Empty) + data;
            }
        }
        public static string ReplaceRange(this string str, int index, int length, string text) {
            int preStrLength = index + 1;
            if(str.Length < preStrLength) {
                throw new Exception("invalid range replace");
            }
            string preStr = str.Substring(0, preStrLength);
            int postStrIdx = index + length;
            if(postStrIdx >= str.Length) {
                throw new Exception("invalid range replace");
            }
            int postStrLength = str.Length - (index + length);
            if(postStrIdx + postStrLength > str.Length) {
                throw new Exception("invalid range replace");
            }
            string postStr = str.Substring(postStrIdx, postStrLength);
            return preStr + text + postStr;
        }

        public static bool IsStringUrl(this string str) {
            if(string.IsNullOrWhiteSpace(str) || !str.ToLower().StartsWith("http")) {
                return false;
            }
            return Uri.IsWellFormedUriString(str,UriKind.Absolute);
        }

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

        public static string[] SplitNoEmpty(this string str, string separator) {
            return str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string TrimTrailingLineEnding(this string str) {
            if(str.EndsWith(Environment.NewLine)) {
                return str.Substring(0, str.Length - 1);
            }
            return str;
        }

        public static bool IsStringResourcePath(this string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            if (text.StartsWith("pack:") || text.StartsWith("avares:")) {
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
            TimeSpan ts = DateTime.Now - dt;
            int totalYears = (int)(ts.TotalDays / 365);
            int totalMonths = DateTime.Now.MonthDifference(dt);
            int totalWeeks = DateTime.Now.WeekDifference(dt);
            int totalDays = (int)ts.TotalDays;
            int totalHours = (int)ts.TotalHours;
            int totalMinutes = (int)ts.TotalMinutes;

            if (totalYears > 1) {
                return string.Format($"{totalYears} years ago");
            }
            if (totalMonths >= 1 && totalWeeks >= 4) {
                return string.Format($"{(totalMonths == 1 ? "Last":totalMonths.ToString())} month{(totalMonths == 1 ? string.Empty : "s ago")}");
            }
            if (totalWeeks >= 1) {
                return string.Format($"{(totalWeeks == 1 ? "Last":totalWeeks.ToString())} week{(totalWeeks == 1 ? string.Empty : "s ago")}");
            }
            if (totalDays >= 1) {
                return string.Format($"{(totalDays == 1 ? "Yesterday":totalDays.ToString())} {(totalDays == 1 ? string.Empty : "days ago")}");
            }
            if (totalHours >= 1) {
                return string.Format($"{totalHours} hour{(totalHours == 1 ? string.Empty : "s")} ago");
            }
            if (totalMinutes >= 1) {
                return string.Format($"{totalMinutes} minute{(totalMinutes == 1 ? string.Empty : "s")} ago");
            }
            return "Just Now";
        }

        public static int WeekDifference(this DateTime lValue, DateTime rValue) {
            double weeks = (lValue - rValue).TotalDays / 7;
            return (int)weeks;
        }

        public static int MonthDifference(this DateTime lValue, DateTime rValue) {
            return (lValue.Month - rValue.Month) + (12 * (lValue.Year - rValue.Year));
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

        public static async Task<string> CheckSumAsync(this string str) {
            string result = await Task<string>.Run(() => {
                return CheckSum(str);
            });
            return result;
        }

        
        public static string CheckSum(this string str) {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
                string hash = BitConverter.ToString(
                    md5.ComputeHash(
                        Encoding.UTF8.GetBytes(str))).Replace("-", String.Empty);
                return hash;
            }
        }
        public static string ToBase64String(this byte[] bytes) {
            if (bytes == null) {
                return string.Empty;
            }
            return Convert.ToBase64String(bytes);
        }


        public static async Task<byte[]> ReadUriBytesAsync(this string uri, int timeoutMs = 5000) {
            using (var httpClient = new HttpClient()) {
                try {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", System.Guid.NewGuid().ToString());
                    byte[] bytes = await httpClient.GetByteArrayAsync(uri).TimeoutAfter(TimeSpan.FromMilliseconds(timeoutMs));
                    return bytes;
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                }
            }
            return null;
        }

        public static async Task<string> ToBase64FromRichHtmlImageString(this string htmlStr, string fallbackBase64Str = null, int timeoutMs = 5000) {
            fallbackBase64Str = fallbackBase64Str == null ? string.Empty : fallbackBase64Str;
            if (!htmlStr.IsStringRichHtmlImage()) {
                return fallbackBase64Str;
            }
            var htmlDoc = new HtmlDocument();
            var imgNode = htmlDoc.DocumentNode.FirstChild.FirstChild;
            if (imgNode.Name.ToLower() != "img") {
                // what is the string how'd it happen?
                Debugger.Break();
                return fallbackBase64Str;
            }
            string srcAttrVal = imgNode.GetAttributeValue("src", string.Empty);
            int base64_token_idx = srcAttrVal.IndexOf("data:image/png;base64,");
            if(base64_token_idx >= 0) {
                return srcAttrVal.Substring(base64_token_idx + 1).Trim();
            }
            if(srcAttrVal.IsStringUrl()) {
                var src_bytes = await srcAttrVal.ReadUriBytesAsync(timeoutMs);
                if(src_bytes == null || src_bytes.Length == 0) {
                    return fallbackBase64Str;
                }
                return src_bytes.ToBase64String();
            }
            return fallbackBase64Str;
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


        public static TEnum ToEnum<TEnum>(this object obj, bool ignoreCase = true, TEnum notFoundValue = default) where TEnum: struct {
            if(obj != null) {
                try {
                    if (obj is string str) {
                        if (Enum.TryParse<TEnum>(str, ignoreCase, out TEnum result)) {
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
            return notFoundValue;
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
        public static bool IsStringEscapedHtml(this string str) {
            if(str.Contains("&lt;") && str.Contains("&gt;")) {
                // only check if both '<' and '>' are present otherwise it wouldn't be valid html
                return true;
            }
            return false;
        }

        public static Dictionary<char, string> HtmlEntityLookup =>
            new Dictionary<char, string>() {
                {' ',"&nbsp;" },
                {'&',"&amp;" },
                {'\"',"&quot;" }, {'\'',"&apos;" }, {'>',"&gt;" },
                {'¢',"&cent;" },
                {'£',"&pound;" },
                {'¥',"&yen;" },
                {'€',"&euro;" },
                {'©',"&copy;" },
                {'®',"&reg;" },
                {'™',"&trade;" },
                {'<',"&lt;" }
            };
        public static string EncodeSpecialHtmlEntities(this string str) {
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++) {
                if (HtmlEntityLookup.ContainsKey(str[i])) {
                    var already_encoded_kvp = HtmlEntityLookup.FirstOrDefault(x => str.Substring(i).StartsWith(x.Value));
                    if(!already_encoded_kvp.Equals(default(KeyValuePair<char,string>))) {
                        sb.Append(already_encoded_kvp.Value);
                        i += already_encoded_kvp.Value.Length - 1;
                        continue;
                    }
                    sb.Append(HtmlEntityLookup[str[i]]);
                } else {
                    sb.Append(str[i]);
                }
            }
            return sb.ToString();
        }

        public static string DecodeSpecialHtmlEntities(this string str) {
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++) {
                var kvp = HtmlEntityLookup.FirstOrDefault(x => str.Substring(i).StartsWith(x.Value));
                if(!kvp.Equals(default(KeyValuePair<char,string>))) {
                    sb.Append(kvp.Key);
                    i += kvp.Value.Length - 1;
                } else {
                    sb.Append(str[i]);
                }
            }
            return sb.ToString();
        }

        public static bool ContainsSpecialHtmlEntities(this string str) {
            return HtmlEntityLookup.Any(x => str.Contains(x.Key));
        }

        public static bool ContainsEncodedSpecialHtmlEntities(this string str) {
            return MpRegEx.RegExLookup[MpRegExType.EncodedHtmlEntity].IsMatch(str);
        }

        public static string EscapeMenuItemHeader(this string str, int altNavIdx = -1) {
            if(str == null) {
                return null;
            }
            // NOTE underscores are ommitted from menu items and when dynamic this is problematic, allow for 0 idx though cause that's the shor
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++) {
                if(i == altNavIdx) {
                    sb.Append("_");
                }
                if (str[i] == '_') {
                    sb.Append("_");
                }
                sb.Append(str[i]);
            }
            return sb.ToString();
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

        public static string FindParentDirectory(this string path, int level = 0) {
            if (!path.IsFileOrDirectory()) {
                return null;
            }
            string dir = Path.GetDirectoryName(path);
            while (level > 0) {
                dir = new DirectoryInfo(dir).Parent.FullName;
                level--;
            }
            return dir;
        }

        public static string FindParentDirectory(this string path, string dirName) {
            string rootPath = Path.GetPathRoot(path);
            string curDirName = Path.GetFileName(path);
            while (curDirName != dirName) {
                if (path == rootPath) {
                    throw new DirectoryNotFoundException("Could not find the project directory.");
                }
                path = Directory.GetParent(path).FullName;
                curDirName = Path.GetFileName(path);
            }
            return path;
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

        public static bool IsStringRichHtml(this string str) {
            if (string.IsNullOrEmpty(str) || !str.StartsWith("<")) {
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

        public static bool IsStringRichHtmlImage(this string str) {
            if(string.IsNullOrWhiteSpace(str) || !str.ToLower().StartsWith("<p>")) {
                return false;
            }
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(str);
            if(htmlDoc.DocumentNode.ChildNodes.Count == 1 &&
                htmlDoc.DocumentNode.FirstChild.Name.ToLower() == "p" &&
                htmlDoc.DocumentNode.FirstChild.ChildNodes.Count == 1 &&
                htmlDoc.DocumentNode.FirstChild.FirstChild.Name.ToLower() == "img") {
                return true;
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
                    "img",
                    "br"
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
