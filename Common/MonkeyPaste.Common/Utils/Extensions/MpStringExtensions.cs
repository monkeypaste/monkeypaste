using HtmlAgilityPack;
using MonkeyPaste.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MonkeyPaste.Common {
    public static class MpStringExtensions {
        #region Private Variables

        private static List<string> _resourceNames;
        private static (string, int)[] _abbreviatedIntLookup = new (string, int)[] {
            ("B",1_000_000_000),
            ("M",1_000_000),
            ("K",1000),
        };

        // NOTE use these in this order to avoid matching \n when its \r\n
        public static string[] LineBreakTypes = new[] { "\r\n", "\n" };

        #endregion

        #region Encoding Extensions
        public static string ToBase64ImageUrl(this string base64Str) {
            if (base64Str.Contains(",")) {
                MpDebug.Break($"Base64 conv error, string already is url");
                return base64Str;
            }
            string prefix = @"data:image/png;base64,";
            return prefix + base64Str;
        }
        public static string ToDecodedString(this byte[] bytes, Encoding enc = null, bool stripNulls = false) {
            if (bytes == null || bytes.Length == 0) {
                return string.Empty;
            }
            // TODO should use local encoding here
            string out_str;
            if (enc == null) {
                bytes.DetectTextEncoding(out string decodedStr);
                out_str = decodedStr;
            } else {
                out_str = enc.GetString(bytes);
            }
            if (stripNulls) {
                // some cases the string has '\0' chars, which will invalidate URI as a prob
                // i think its when the byte count is longer than the actual string or 
                // some kind of padding isn't right so '\0' is added to the byte[]
                // code from https://stackoverflow.com/a/35182252/105028

                // s == "heresastring\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0(etc)"    
                out_str = out_str.Split(new[] { '\0' }, 2)[0];
                // s == "heresastring"
            }
            return out_str;
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

        public static string ToCommaSeperatedIntString(this int value) {
            return $"{value:n0}";
        }
        public static string ToCommaSeperatedDoubleString(this double value) {
            return $"{value:n}";
        }
        public static string ToAbbreviatedIntString(this int value, int maxDigits = 4) {
            string valStr = value.ToString();
            if (valStr.Length <= maxDigits) {
                return valStr;
            }
            for (int i = 0; i < _abbreviatedIntLookup.Length; i++) {
                var ail_tup = _abbreviatedIntLookup[i];
                int abbr_value = value / ail_tup.Item2;
                if (Math.Abs(abbr_value) < 1) {
                    continue;
                }
                return $"{abbr_value}{ail_tup.Item1}";
            }
            MpDebug.Break($"Value {value} is larger than max abbrv lookup (billion)");
            return "...";
        }

        public static string ToStringOrDefault(this object obj) {
            return obj == null ? default : obj.ToString();
        }
        public static string ToStringOrEmpty(this object obj, string emptyText = "") {
            return obj == null ? emptyText : obj.ToString();
        }
        public static string ToFileSystemUriFromPath(this string path) {
            if (Uri.TryCreate(path, UriKind.Absolute, out Uri fp_uri)) {
                return fp_uri.AbsoluteUri;
            }
            return string.Empty;
        }
        public static string ToPlainText(this string text, string sourceFormat = "") {
            sourceFormat = sourceFormat.ToLower();
            if (sourceFormat == "text") {
                return text;
            }
            if (text.IsStringRichHtml() || sourceFormat == "html") {
                return MpRichHtmlToPlainTextConverter.Convert(text);
            }

            return text;
        }

        #endregion

        public static Version ToVersion(this string ver_str) {
            try {
                return new Version(ver_str);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error creating version from string '{ver_str}':", ex);
                return new Version();
            }
        }

        public static string RemoveGuidFormat(this string guid) {
            if (guid == null) {
                return string.Empty;
            }
            var format_chars = new char[] { '_', '-', ',', '{', '}', '"' };
            string clean_guid = guid;
            foreach (char format_char in format_chars) {
                clean_guid = clean_guid.Replace(format_char.ToString(), string.Empty);
            }
            return clean_guid;
        }

        public static string ToTestResultLabel(this bool success) {
            return success ? "SUCCEEDED" : "FAILED";
        }
        public static int ComputeLevenshteinDistance(this string s, string t) {
            // from 'https://stackoverflow.com/a/6944095/105028' 
            if (string.IsNullOrEmpty(s)) {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t)) {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++) {
                for (int j = 1; j <= m; j++) {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }

        public static IEnumerable<(int, int)> QueryText(
            this string search_text,
            string match_value,
            bool case_sensitive,
            bool whole_word,
            bool use_regex) {
            Regex regex;
            RegexOptions flags = RegexOptions.None;
            if (!case_sensitive) {
                flags |= RegexOptions.IgnoreCase;
            }

            if (use_regex) {
                regex = new Regex(match_value, flags);
            } else {
                var word_str = whole_word ? "\\b" : "";
                regex = new Regex($"{word_str}{Regex.Escape(match_value)}{word_str}", flags);
            }
            var mc = regex.Matches(search_text);
            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        yield return (c.Index, c.Length);
                    }
                }
            }
        }
        public static bool IsNullOrWhitespaceHtmlString(this string htmlStr) {
            if (htmlStr == null) {
                return true;
            }
            string pt = htmlStr.ToPlainText();
            return
                string.IsNullOrWhiteSpace(pt) ||
                htmlStr.ToLower() == "<p><br></p>" ||
                htmlStr.ToLower() == "<p><br/></p>";
        }

        public static bool IsStringImageResourceKey(this string str) {
            if (string.IsNullOrWhiteSpace(str)) {
                return false;
            }
            if (str.ToLower().EndsWith("image")) {
                return true;
            }
            return false;
        }

        public static bool IsStringImageResourcePathOrKey(this string str) {
            if (string.IsNullOrWhiteSpace(str)) {
                return false;
            }
            if (str.IsStringImageResourceKey()) {
                return true;
            }
            if (str.ToLower().EndsWith("png")) {
                return true;
            }
            return false;
        }
        public static string AppendData(this string str, string data, string dataFormat, bool insertNewLine) {
            if (str.IsStringRichHtml()) {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(str);
                var textNode = htmlDoc.CreateTextNode(data);
                var textSpan = htmlDoc.CreateElement("span");
                textSpan.AppendChild(textNode);
                if (insertNewLine) {
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
            if (str.Length < preStrLength) {
                throw new Exception("invalid range replace");
            }
            string preStr = str.Substring(0, preStrLength);
            int postStrIdx = index + length;
            if (postStrIdx >= str.Length) {
                throw new Exception("invalid range replace");
            }
            int postStrLength = str.Length - (index + length);
            if (postStrIdx + postStrLength > str.Length) {
                throw new Exception("invalid range replace");
            }
            string postStr = str.Substring(postStrIdx, postStrLength);
            return preStr + text + postStr;
        }

        public static bool IsStringUrl(this string str) {
            if (string.IsNullOrWhiteSpace(str) || !str.ToLower().StartsWith("http")) {
                return false;
            }
            return Uri.IsWellFormedUriString(str, UriKind.Absolute);
        }

        public static bool IsStringNullOrEmpty(this string str) {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsStringNullOrWhiteSpace(this string str) {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string StripLineBreaks(this string str) {
            if (str == null) {
                return str;
            }
            return str.Replace(LineBreakTypes[0], string.Empty).Replace(LineBreakTypes[1], string.Empty);
        }

        public static string RemoveLastLineEnding(this string str) {
            foreach (var lb in LineBreakTypes) {
                if (str.EndsWith(lb)) {
                    return str.Substring(0, str.Length - lb.Length);
                }
            }

            return str;
        }
        public static string[] SplitByLineBreak(this string str) {
            if (str == null) {
                return new string[] { };
            }
            foreach (var lb in LineBreakTypes) {
                if (str.Contains(lb)) {
                    return str.SplitNoEmpty(lb);
                }
            }
            return new[] { str };
        }

        public static string[] SplitNoEmpty(this string str, string separator) {
            if (str == null) {
                return new string[] { };
            }
            return str.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }
        public static string[] SplitWithEmpty(this string str, string separator) {
            if (str == null) {
                return new string[] { };
            }
            return str.Split(new string[] { separator }, StringSplitOptions.None);
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
        public static bool IsCapitalCaseChar(char let) {
            if (let == default) {
                return false;
            }
            if (let >= 'A' && let <= 'Z') {
                return true;
            }
            return false;
        }

        public static bool IsLowerCaseChar(char let) {
            if (let == default) {
                return false;
            }
            if (let >= 'a' && let <= 'z') {
                return true;
            }
            return false;
        }

        public static string ToProperCase(this string titleCaseStr, string noneText = "", string spaceStr = " ") {
            // TODO when automating UI language need to parameterize low vs up case logic
            //Converts 'ThisIsALabel" to 'This Is A Label'
            var sb = new StringBuilder();
            for (int i = 0; i < titleCaseStr.Length; i++) {
                if (i > 0 &&
                    (IsLowerCaseChar(titleCaseStr[i - 1]) && IsCapitalCaseChar(titleCaseStr[i]) ||
                    IsCapitalCaseChar(titleCaseStr[i - 1]) && IsCapitalCaseChar(titleCaseStr[i]))) {
                    sb.Append(spaceStr);
                }
                sb.Append(titleCaseStr[i]);
            }
            string result = sb.ToString();
            if (result.ToLower() == "none") {
                return noneText;
            }
            return result;
        }



        public static int WeekDifference(this DateTime lValue, DateTime rValue) {
            double weeks = (lValue - rValue).TotalDays / 7;
            return (int)weeks;
        }

        public static int MonthDifference(this DateTime lValue, DateTime rValue) {
            return (lValue.Month - rValue.Month) + (12 * (lValue.Year - rValue.Year));
        }

        public static string ToReadableTimeSpan(this TimeSpan ts) {
            int totalDays = (int)ts.TotalDays;
            int totalHours = (int)ts.TotalHours;
            int totalMinutes = (int)ts.TotalMinutes;
            int totalSeconds = (int)ts.TotalSeconds;
            int totalMilliseconds = (int)ts.TotalMilliseconds;

            string out_str = string.Empty;
            if (totalDays >= 1) {
                out_str += string.Format($"{totalDays} Day{(totalDays > 1 ? "s" : "")} ");
            }
            if (totalHours >= 1) {
                out_str += string.Format($"{totalHours} Hour{(totalHours > 1 ? "s" : "")} ");
            }
            if (totalMinutes >= 1) {
                out_str += string.Format($"{totalMinutes} Min{(totalMinutes > 1 ? "s" : "")} ");
            }
            if (totalSeconds >= 1) {
                out_str += string.Format($"{totalSeconds} Sec{(totalSeconds > 1 ? "s" : "")} ");
            }
            if (totalMilliseconds >= 1) {
                out_str += string.Format($"{totalMilliseconds} Ms ");
            }
            return out_str;
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


        public static string CheckSum(this string str, Encoding enc = default) {
            using MD5 md5 = MD5.Create();
            string hash =
                BitConverter.ToString(
                    md5.ComputeHash(str.ToStringOrEmpty().ToBytesFromString(enc)))
                .Replace("-", string.Empty);
            return hash;
        }
        public static string ToBase64String(this byte[] bytes) {
            if (bytes == null) {
                return string.Empty;
            }
            var str = Convert.ToBase64String(bytes);
            return str;
        }


        #region Enums

        public static TEnum ToEnum<TEnum>(this object obj, bool ignoreCase = true, TEnum notFoundValue = default) where TEnum : struct {
            if (obj != null) {
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
        public static object ToEnum(this object enumValObj, Type enumType, bool ignoreCase = true, object notFoundValue = default) {
            if (enumValObj != null) {
                try {
                    if (enumValObj is string str) {
                        if (Enum.GetNames(enumType).IndexOf(str) is int enumValIdx) {
                            enumValObj = Enum.ToObject(enumType, enumValIdx);
                        } else {
                            MpDebug.Break($"Error cannot find enum val '{enumValObj}' in enum '{enumType}'");
                            enumValObj = 0;
                        }
                    } else if (enumValObj is not int) {
                        MpDebug.Break($"Error unhandled enum val type '{enumValObj.GetType()}'");
                        enumValObj = 0;
                    }
                    return Enum.ToObject(enumType, enumValObj);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                }
            }
            return notFoundValue;
        }

        public static TEnum ToEnumFlags<TEnum>(this string csvStr, bool ignoreCase = true, MpCsvFormatProperties csvProps = null, TEnum notFoundValue = default) where TEnum : struct {
            if (string.IsNullOrEmpty(csvStr)) {
                return notFoundValue;
            }
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;

            try {
                if (csvStr.ToListFromCsv(csvProps) is List<string> valueNames) {
                    long resultVal = default;
                    foreach (string valueName in valueNames) {
                        if (Enum.TryParse<TEnum>(valueName, ignoreCase, out TEnum cur_result)) {
                            if (cur_result is long cur_val) {
                                resultVal |= cur_val;
                            }
                        }
                    }
                    return resultVal.ToEnum(ignoreCase, notFoundValue);
                }

            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(ex);
            }
            return notFoundValue;
        }

        public static string ToFlagNamesCsvString<TEnum>(this TEnum flagEnum, MpCsvFormatProperties csvProps = null) where TEnum : struct {
            csvProps = csvProps == null ? MpCsvFormatProperties.Default : csvProps;
            List<string> flagNames = new List<string>();
            //var eobj = Enum.ToObject(typeof(TEnum), flagEnum);
            //if (eobj == null) {
            //    return string.Empty;
            //}
            if (typeof(TEnum).GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0) {
                flagNames.Add(flagEnum.ToString());
            } else {
                foreach (string curName in typeof(TEnum).GetEnumNames()) {
                    if (Enum.TryParse(curName, false, out TEnum curEnum)) {
                        flagNames.Add(curName);
                    }
                }
            }
            return flagNames.ToCsv(csvProps);
        }

        #endregion


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
            if (string.IsNullOrWhiteSpace(jsonStr)) {
                return jsonStr;
            }
            try {
                JToken jt = JToken.Parse(jsonStr);
                return jt.ToString();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error pretty printing json '{jsonStr}'", ex);
                return jsonStr;
            }
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
            if (str.Contains("&lt;") && str.Contains("&gt;")) {
                // only check if both '<' and '>' are present otherwise it wouldn't be valid html
                return true;
            }
            return false;
        }

        public static Dictionary<char, string> HtmlEntityLookup =>
            new Dictionary<char, string>() {
                {'&',"&amp;" },
                {' ',"&nbsp;" },
                {'\"',"&quot;" },
                {'\'',"&apos;" },
                {'>',"&gt;" },
                {'¢',"&cent;" },
                {'£',"&pound;" },
                {'¥',"&yen;" },
                {'€',"&euro;" },
                {'©',"&copy;" },
                {'®',"&reg;" },
                {'™',"&trade;" },
                {'<',"&lt;" }
            };

        private static Regex _AmpNotSpecialEntityRegex;
        public static string EncodeSpecialHtmlEntities(this string str) {
            foreach (var pattern in HtmlEntityLookup) {
                if (pattern.Key == '&') {
                    // special case for & to avoid double encoding
                    if (_AmpNotSpecialEntityRegex == null) {
                        _AmpNotSpecialEntityRegex = new Regex(@"&(?!(#[0-9]{2,4}|[A-z]{2,6});)/g", RegexOptions.Compiled);
                    }
                    str = _AmpNotSpecialEntityRegex.Replace(str, pattern.Value);
                } else {
                    str = str.Replace(pattern.Key.ToString(), pattern.Value);
                }
            }
            return str;
        }

        public static string DecodeSpecialHtmlEntities(this string str) {
            foreach (var pattern in HtmlEntityLookup) {
                str = str.Replace(pattern.Value, pattern.Key.ToString());
            }
            return str;
        }

        public static string DecodeHtmlHexCharacters(this string html) {
            //var replacements = new Dictionary<string, string>();
            ////var regex = new Regex("(&[a-zA-Z]{2,11};)");
            //var regex = MpRegEx.RegExLookup[MpRegExType.HexEncodedHtmlEntity];
            //foreach (Match match in regex.Matches(str)) {
            //    if (!replacements.ContainsKey(match.Value)) {
            //        var unicode = HttpUtility.HtmlDecode(match.Value);
            //        if (unicode.Length == 1) {
            //            replacements.Add(match.Value, string.Concat("&#", Convert.ToInt32(unicode[0]), ";"));
            //        }
            //    }
            //}
            //foreach (var replacement in replacements) {
            //    str = str.Replace(replacement.Key, replacement.Value);
            //}
            //return str;
            if (html.IsNullOrEmpty()) {
                return html;
            }
            //var regex = MpRegEx.RegExLookup[MpRegExType.HexEncodedHtmlEntity];
            //return regex.Replace(
            //    html,
            //    x => x.Groups[1].Value == "#" ?
            //        ((char)int.Parse(x.Groups[2].Value)).ToString() :
            //        WebUtility.HtmlDecode(x.Groups[0].Value));
            return WebUtility.HtmlDecode(html);
        }

        public static bool ContainsSpecialHtmlEntities(this string str) {
            return HtmlEntityLookup.Any(x => str.Contains(x.Key));
        }

        public static bool ContainsEncodedSpecialHtmlEntities(this string str) {
            return MpRegEx.RegExLookup[MpRegExType.HexEncodedHtmlEntity].IsMatch(str);
        }

        public static string EscapeMenuItemHeader(this string str, int altNavIdx = -1) {
            if (str == null) {
                return null;
            }
            // NOTE underscores are ommitted from menu items and when dynamic this is problematic, allow for 0 idx though cause that's the shor
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++) {
                if (i == altNavIdx) {
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
            if (m.Success) {
                return m.Value;
            }
            return null;
        }

        public static bool IsSupportedImageFileType(this string str) {
            if (!str.IsFile()) {
                return false;
            }
            string[] image_exts = new string[] { "png", "gif", "jpg", "jpeg", "bmp" };
            return image_exts.Any(x => x == Path.GetExtension(str).ToLower());
        }

        public static bool IsFile(this string str) {
#if WINDOWS
            if (str != null && str.Length > MpFileIo.MAX_WIN_PATH_LENGTH) {
                // prevent PathLength exception (happens checking base64)
                return false;
            }
#endif
            return File.Exists(str);
        }

        public static bool IsDirectory(this string str) {
#if WINDOWS
            if (str != null && str.Length > MpFileIo.MAX_WIN_PATH_LENGTH) {
                // prevent PathLength exception (happens checking base64)
                return false;
            }
#endif
            return Directory.Exists(str);
        }
        public static string GetDir(this string path) {
            return Path.GetDirectoryName(path);
        }
        public static bool IsFileOrDirectory(this string str) {
            return str.IsFile() || str.IsDirectory();
        }

        public static bool IsStringPathUri(this string str) {
            if (Uri.IsWellFormedUriString(str, UriKind.Absolute) &&
                            new Uri(str) is { } uri &&
                            uri.Scheme == Uri.UriSchemeFile) {
                return true;
            }
            return false;
        }
        public static string ToPathFromUri(this string str) {
            if (Uri.IsWellFormedUriString(str, UriKind.Absolute) &&
                            new Uri(str) is { } uri &&
                            uri.Scheme == Uri.UriSchemeFile &&
                            uri.LocalPath is string lp) {
                return lp;
            }
            return str;
        }
        public static bool IsShortcutPath(this string str) {
            if (!str.IsFile()) {
                return false;
            }
            if (MpCommonTools.Services.PlatformInfo.OsType == MpUserDeviceType.Windows) {
                return str.ToLower().EndsWith("lnk");
            }
            // TODO not sure if symbolic links need to be resolved 
            return false;
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


        public static List<int> IndexListOfAll(this string text, string str, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase) {
            List<int> allIndexOf = new List<int>();
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
        public static bool IsStringHtmlDocument(this string text) {
            if (text == null) {
                return false;
            }
            return
                text.StartsWith("<html>", StringComparison.InvariantCultureIgnoreCase) &&
                text.EndsWith("</html>", StringComparison.InvariantCultureIgnoreCase);
        }
        public static string ToHtmlDocumentFromTextOrPartialHtml(this string text) {
            return $"<html><body>{text}</body></html>";
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

        public static string IncludeOrRemoveHexAlpha(this string hexStr, bool remove, string alpha_to_include = "FF", bool force_included = false) {
            if (remove) {
                return hexStr.RemoveHexAlpha();
            }
            if (!force_included && hexStr.Length == 9) {
                return hexStr;
            }
            hexStr = hexStr.RemoveHexAlpha(out string removed_alpha);
            if (!force_included && !string.IsNullOrEmpty(removed_alpha)) {
                alpha_to_include = removed_alpha;
            }

            return $"#{alpha_to_include}{hexStr}";
        }

        public static string RemoveHexAlpha(this string hexStr) {
            return hexStr.RemoveHexAlpha(out string _);
        }

        public static string RemoveHexAlpha(this string hexStr, out string removedAlpha) {
            removedAlpha = string.Empty;

            if (string.IsNullOrEmpty(hexStr) ||
                !hexStr.StartsWith("#") ||
                hexStr.Length < 9) {
                return hexStr;
            }
            var hex_chars = hexStr.ToCharArray();
            removedAlpha = string.Join(string.Empty, hex_chars.Skip(1).Take(2));
            string result = "#" + hexStr.Substring(3);
            return result;
        }

        public static bool IsStringHexColor(this string str) {
            if (string.IsNullOrWhiteSpace(str)) {
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
        public static bool IsStringHexOrNamedColor(this string str) {
            return str.IsStringHexColor() || str.IsStringNamedColor();
        }
        public static bool IsStringGuid(this string str) {
            if (string.IsNullOrWhiteSpace(str)) {
                return false;
            }
            return MpRegEx.RegExLookup[MpRegExType.Guid].IsMatch(str);
        }


        public static string NamedColorToHex(this string str) {
            if (!IsStringNamedColor(str)) {
                throw new Exception($"'{str}' is not an X11 color name sowwy");
            }
            var propInfo = typeof(MpSystemColors).GetProperty(str.ToLower());
            return propInfo.GetValue(null) as string;
        }

        public static bool IsNullEmptyWhitespaceOrAlphaNumeric(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return true;
            }

            return str.All(x => char.IsLetterOrDigit(x) || char.IsWhiteSpace(x));
        }
        public static bool IsStringBase64(this string str) {
            // Check that the length is a multiple of 4 characters
            //Check that every character is in the set A - Z, a - z, 0 - 9, +, / except for padding at the end which is 0, 1 or 2 '=' characters

            if (string.IsNullOrEmpty(str) ||
                str.Contains(".") ||
                str.EndsWith("Image") ||
                str.EndsWith("Icon") ||
                str.EndsWith("Svg")) {
                return false;
            }
            if (str.Length % 4 != 0) {
                return false;
            }
            return !MpRegEx.RegExLookup[MpRegExType.Is_NOT_Base64Str].IsMatch(str);
        }

        public static bool IsStringBase64_FromException(this string str) {
            if (string.IsNullOrWhiteSpace(str)) {
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
            if (string.IsNullOrWhiteSpace(path) || path.Length < 3) {
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
            if (string.IsNullOrWhiteSpace(str) || !str.ToLower().StartsWith("<p>")) {
                return false;
            }
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(str);
            if (htmlDoc.DocumentNode.ChildNodes.Count == 1 &&
                htmlDoc.DocumentNode.FirstChild.Name.ToLower() == "p" &&
                htmlDoc.DocumentNode.FirstChild.ChildNodes.Count == 1 &&
                htmlDoc.DocumentNode.FirstChild.FirstChild.Name.ToLower() == "img") {
                return true;
            }
            return false;
        }

        public static bool IsStringCsv(this string text) {
            if (string.IsNullOrEmpty(text) || IsStringRtf(text)) {
                return false;
            }
            return text.Contains(",");
        }

        public static bool IsStringRtf(this string text) {
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
            if (IsStringRtf(text) || IsStringSection(text) || IsStringSpan(text) || IsStringXaml(text)) {
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
            if (!text.IsStringRtf()) {
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

        public static int ToPseudoRandomInt(this string str, int max) {
            //int sum = str.ToCharArray().Sum(x => x.ParseOrConvertToInt());
            //return sum % max;
            if (str == "TrashCanImage" ||
                str.EndsWith("trashcan.png")) {

            }
            return (int)((double)str.Length).Wrap(0, max);
        }
    }
}
