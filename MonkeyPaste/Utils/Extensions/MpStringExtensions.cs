using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MonkeyPaste.Plugin;
using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration;

namespace MonkeyPaste {
    public static class MpStringExtensions {
        private static Random _Rand;
                

        public static string RemoveSpecialCharacters(this string str) {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static string ToPrettyPrintJson(this string jsonStr) {
            JToken jt = JToken.Parse(jsonStr);
            return jt.ToString();
        }

        public static string Escape(this string badString) {
            return badString.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace(">", "&gt;").Replace("<", "&lt;");
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

        public static List<int> IndexListOfAll(this string text, string matchStr) {
            var idxList = new List<int>();
            int curIdx = text.IndexOf(matchStr);
            int offset = 0;
            while (curIdx >= 0 && curIdx < text.Length) {
                idxList.Add(curIdx + offset);
                if (curIdx + matchStr.Length + 1 >= text.Length) {
                    break;
                }
                text = text.Substring(curIdx + matchStr.Length + 1);
                offset = curIdx + 1;
                curIdx = text.IndexOf(matchStr);
            }
            return idxList;
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

        public static string ToFile(this string str, string forceDir = "", string forceNamePrefix = "", string forceExt = "", bool overwrite = false) {
            if(string.IsNullOrEmpty(forceExt)) {
                if(str.IsStringRichText()) {
                    forceExt = "rtf";
                } else if(str.IsStringBase64()) {
                    forceExt = "png";
                } else if(str.IsStringCsv()) {
                    forceExt = "csv";
                } else if(!str.IsFileOrDirectory()) {
                    forceExt = "txt";
                }
            }  else {
                if (forceExt.ToLower().Contains("rtf")) {
                    str = MpNativeWrapper.Services.StringTools.ToRichText(str);
                } else if (forceExt.ToLower().Contains("txt")) {
                    str = MpNativeWrapper.Services.StringTools.ToPlainText(str);
                } else if (forceExt.ToLower().Contains("csv")) {
                    str = MpNativeWrapper.Services.StringTools.ToCsv(str);
                }
            }

            string tfp;
            if (str.IsFileOrDirectory()) {
                tfp = str;
            } else if (forceExt == "png" || 
                       forceExt.ToLower().Contains("bmp") || 
                       forceExt.ToLower().Contains("jpg") || 
                       forceExt.ToLower().Contains("jpeg")) {
                tfp = MpFileIo.WriteByteArrayToFile(Path.GetTempFileName(), str.ToByteArray());
            } else {
                tfp = MpFileIo.WriteTextToFile(Path.GetTempFileName(), str);
            }
            string ofp = tfp;

            if(!string.IsNullOrEmpty(forceNamePrefix)) {
                string tfnwe = Path.GetFileName(tfp);
                string ofnwe = Path.GetFileNameWithoutExtension(tfp) + "." + Path.GetExtension(tfp);
                ofp = ofp.Replace(tfnwe, ofnwe);
            }

            if(!string.IsNullOrEmpty(forceExt)) {
                forceExt = forceExt.Contains(".") ? forceExt : "." + forceExt;
                string tfe = Path.GetExtension(tfp);
                ofp = ofp.Replace("." + tfe, forceExt);
            }

            if(!string.IsNullOrEmpty(forceDir)) {
                if(!Directory.Exists(forceDir)) {
                    throw new Exception("Directory not found: " + forceDir);
                }
                string tfd = Path.GetDirectoryName(tfp);
                ofp = ofp.Replace(tfd, forceDir);
            }
            if(ofp.ToLower() != tfp.ToLower()) {
                if(ofp.IsFileOrDirectory() && !overwrite) {
                    ofp = MpFileIo.GetUniqueFileOrDirectoryName(Path.GetDirectoryName(ofp), Path.GetFileNameWithoutExtension(ofp));
                }
                File.Copy(tfp, ofp,overwrite);
                File.Delete(tfp);
            }
            return ofp;
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
            return MpRegEx.IsMatch(MpSubTextTokenType.HexColor, str);
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

        private static Regex _IsNotBase64RegEx;
        public static bool IsStringBase64(this string str) {
            // Check that the length is a multiple of 4 characters
            //Check that every character is in the set A - Z, a - z, 0 - 9, +, / except for padding at the end which is 0, 1 or 2 '=' characters

            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            if (str.Length % 4 != 0) {
                return false;
            }
            if(_IsNotBase64RegEx == null) {
                _IsNotBase64RegEx = new Regex(@"[^a-zA-Z0-9+/=]",RegexOptions.Compiled);
            }
            if(_IsNotBase64RegEx.IsMatch(str)) {
                return false;
            }
            return true;
        }

        public static bool IsStringQuillText(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            str = str.ToLower();
            foreach (var quillTag in _quillTags) {
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
            if(_Rand == null) {
                _Rand = new Random((int)DateTime.Now.Ticks);
            }
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_Rand.Next(s.Length)]).ToArray());
        }

        public static string GetNewAccessToken() {
            if (_Rand == null) {
                _Rand = new Random((int)DateTime.Now.Ticks);
            }
            return GetRandomString(_Rand.Next(20, 50), AlphaNumericChars);
        }

        private static string[] _quillTags = new string[] {
            "p",
            "ol",
            "li",
            "#text",
            "img",
            "em",
            "span",
            "strong",
            "u",
            "br",
            "a"
        };
    }
}
