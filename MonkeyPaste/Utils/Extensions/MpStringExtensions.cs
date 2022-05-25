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
using System.Xml.Linq;

namespace MonkeyPaste {
    public static class MpStringExtensions {
        private static Random _Rand;
        
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
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }

        public static bool HasSpecialCharacters(this string str) {
            return Regex.IsMatch(str, "[^a-zA-Z0-9_.]+", RegexOptions.Compiled);
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

        public static string ToStringFromBase64(this string str) {
            var bytes = Convert.FromBase64String(str);
            var text = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return text;
        }

        public static string ToFile(this string fileData, string forceDir = "", string forceNamePrefix = "", string forceExt = "", bool overwrite = false) {
            if(string.IsNullOrEmpty(forceExt)) {
                // when ext is not given infer from content
                if(fileData.IsStringRichText()) {
                    forceExt = "rtf";
                } else if(fileData.IsStringBase64()) {
                    forceExt = "png";
                } else if(fileData.IsStringCsv()) {
                    forceExt = "csv";
                } else if(!fileData.IsFileOrDirectory()) {
                    forceExt = "txt";
                }
            }  else {
                if (forceExt.ToLower().Contains("rtf")) {
                    fileData = MpPlatformWrapper.Services.StringTools.ToRichText(fileData);
                } else if (forceExt.ToLower().Contains("txt")) {
                    fileData = MpPlatformWrapper.Services.StringTools.ToPlainText(fileData);
                } else if (forceExt.ToLower().Contains("csv")) {
                    fileData = MpPlatformWrapper.Services.StringTools.ToCsv(fileData);
                }
            }

            string tfp;
            if (fileData.IsFileOrDirectory()) {
                tfp = fileData;
            } else if (forceExt == "png" || 
                       forceExt.ToLower().Contains("bmp") || 
                       forceExt.ToLower().Contains("jpg") || 
                       forceExt.ToLower().Contains("jpeg")) {
                tfp = MpFileIo.WriteByteArrayToFile(Path.GetTempFileName(), fileData.ToByteArray());
            } else {
                tfp = MpFileIo.WriteTextToFile(Path.GetTempFileName(), fileData);
            }
            string ofp = tfp;

            if(!string.IsNullOrEmpty(forceNamePrefix)) {
                forceNamePrefix = forceNamePrefix.RemoveInvalidFileNameChars();
                string tfnwe = Path.GetFileName(tfp);
                string ofnwe = forceNamePrefix + Path.GetExtension(tfp);
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
                    if(string.IsNullOrEmpty(forceDir)) {
                        // this means file is going to write to temp folder and to avoid IO or name issues preserve name
                        // but put in random subdirectory of temp folder
                        string randomSubDirPath = Path.Combine(Path.GetDirectoryName(ofp), Path.GetRandomFileName());
                        try {
                            Directory.CreateDirectory(randomSubDirPath);
                            ofp = Path.Combine(randomSubDirPath, Path.GetFileName(ofp));
                        }
                        catch (Exception ex) {
                            MpConsole.WriteTraceLine("Error creating random temp subdirectory: "+ex);
                            ofp = MpFileIo.GetUniqueFileOrDirectoryName(Path.GetDirectoryName(ofp), Path.GetFileName(ofp));
                        }
                    } else {
                        ofp = MpFileIo.GetUniqueFileOrDirectoryName(Path.GetDirectoryName(ofp), Path.GetFileName(ofp));
                    }
                    
                }
                // move temporary file to processed output file path and delete temporary
                try {
                    ofp = MpFileIo.CopyFileOrDirectory(tfp, ofp);
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Error copying temp file '{tfp}' to '{ofp}', returning temporary. Exception: " + ex);
                    return tfp;
                }
                if(MpFileIo.IsUnderTemporaryFolder(tfp)) {
                    MpTempFileManager.AddTempFilePath(tfp);
                }
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
            if (str.Length % 4 != 0 || str.Length < 100) {
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

        public static bool IsStringFileOrPathFormat(this string path) {
            if(string.IsNullOrWhiteSpace(path) || path.Length < 3) {
                return false;
            }
            Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3))) 
                return false;
            string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*" + "\"";
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
                return false;
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
