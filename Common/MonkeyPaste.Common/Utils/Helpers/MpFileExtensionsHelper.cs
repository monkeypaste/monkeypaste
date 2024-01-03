using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common {
    // HACK this enum's out of place (search criteria option enums) but 
    // is tied to KnowFileOrFolderExt in RegEx which is visible to plugins so here she be
    public enum MpFileOptionType {
        None = 0,
        Audio, //
        Compressed,
        DiscAndMedia,
        DataAndDatabase,
        Document, //
        Email,
        Executable,
        Font,
        Image, //
        Internet,
        Presentation,
        Programming,
        Spreadsheet, //
        System,
        Video, //
        UserDefined,
        Custom //
    }

    public static class MpFileExtensionsHelper {
        // NOTE dynamic parts will require restart to take affect atm

        // types from https://www.computerhope.com/issues/ch001789.htm


        private static Dictionary<MpFileOptionType, IEnumerable<string>> _extLookup;
        public static Dictionary<MpFileOptionType, IEnumerable<string>> ExtLookup {
            get {
                if (_extLookup == null) {
                    var result = new Dictionary<MpFileOptionType, IEnumerable<string>>() {
                        {MpFileOptionType.Audio,new string[]{ "aif", "cda", "mid", "midi", "mp3", "mpa", "ogg", "wav", "wma", "wpl" } },
                        {MpFileOptionType.Compressed,new string[]{ "7z", "arj", "deb", "pkg", "rar", "rpm", "tar", "gz", "z"} },
                        {MpFileOptionType.DiscAndMedia,new string[]{ "7z", "arj", "deb", "pkg", "rar", "rpm", "tar", "gz", "z" } },
                        {MpFileOptionType.DataAndDatabase,new string[]{"bin", "dmg", "iso", "toast", "vcd"} },
                        {MpFileOptionType.Document,new string[]{ "doc", "docx", "odt", "pdf", "rtf", "tex", "txt", "wpd" } },
                        {MpFileOptionType.Email,new string[]{"eml", "emlx", "msg", "oft", "ost", "pst", "vcf" } },
                        {MpFileOptionType.Executable,new string[]{"apk", "bat", "bin", "cgi", "pl", "com", "exe", "gadget", "jar", "msi", "py", "wsf"} },
                        {MpFileOptionType.Font,new string[]{"fnt", "fon", "otf", "ttf"} },
                        {MpFileOptionType.Image,new string[]{ "ai", "bmp", "gif", "ico", "jpg", "jpeg", "png", "ps", "psd", "svg", "tif", "tiff", "webp" } },
                        {MpFileOptionType.Internet,new string[]{"asp", "aspx", "cer", "cfm", "cgi", "pl", "css", "htm", "html", "js", "part", "php", "py", "rss", "xhtml", ""} },
                        {MpFileOptionType.Presentation,new string[]{"key", "odp", "pps", "ppt", "pptx"} },
                        {MpFileOptionType.Programming,new string[]{"c", "cgi", "pl", "class", "cpp", "cs", "h", "java", "php", "py", "sh", "swift", "vb"} },
                        {MpFileOptionType.Spreadsheet,new string[]{ "ods", "xls", "xlsm", "xlsx" } },
                        {MpFileOptionType.System,new string[]{"bak", "cab", "cfg", "cpl", "cur", "dll", "dmp", "drv", "icns", "ico", "ini", "lnk", "msi", "sys", "tmp"} },
                        {MpFileOptionType.Video,new string[]{"3g2", "3gp", "avi", "flv", "h264", "m4v", "mkv", "mov", "mp4", "mpg", "mpeg", "rm", "swf", "vob", "webm","wmv"} }
                    };
                    if (MpCommonTools.Services == null ||
                        MpCommonTools.Services.UserProvidedFileExts == null) {
                        return result;
                    }
                    // NOTE custom extensions is NOT base64 encoded cause it shouldn't have weird
                    result.Add(MpFileOptionType.UserDefined, MpCommonTools.Services.UserProvidedFileExts.UserDefinedFileExtensionsCsv.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value));
                    _extLookup = result;
                }
                return _extLookup;
            }
        }

        private static string _allExtPst = null;
        public static string AllExtPsv {
            get {
                if (_allExtPst == null) {
                    _allExtPst = string.Join("|", ExtLookup.SelectMany(x => x.Value).Distinct());
                }
                return _allExtPst;
            }
        }
    }
}
