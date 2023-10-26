using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MonkeyPaste.Common {
    public enum MpRegExType {
        None = 0,
        FileOrFolder,
        Uri,
        Email,
        PhoneNumber,
        Currency,
        HexColor,
        StreetAddress,
        //TemplateSegment,
        Guid,
        EncodedTextTemplate,
        Is_NOT_Base64Str,
        HasSpecialCharacters,
        Is_NOT_Number,
        StartsWithWindowsStyleDirectory,
        ContainsInvalidFileNameChar,
        HexEncodedHtmlEntity,
        ExactEmail
        // HtmlTag
    }

    public static class MpRegEx {
        public static string KnownFileExtensions =>
            MpFileExtensionsHelper.AllExtPsv;

        public static string InvalidFileNameChars {
            get {
                string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidPathChars());
                strTheseAreInvalidFileNameChars += @":/?*" + "\"";
                return strTheseAreInvalidFileNameChars;
            }
        }

        public static int MAX_ANNOTATION_REGEX_TYPE => (int)MpRegExType.StreetAddress;

        const string EMAIL_REGEX = @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})";

        private static Dictionary<MpRegExType, string> _regExStrings = new Dictionary<MpRegExType, string>{
            {MpRegExType.None,string.Empty },
            // NOTE!! fileOrFolder doesnt work
            //@"^(?:[\w]\:|\\)(\\[a-zA-Z_\-\s0-9\.()~!@#$%^&=+';,{}\[\]]+)+(\.("+KnownFileExtensions+@")|(\\|\w))$",
            {MpRegExType.FileOrFolder, "(?:\\/|[a-zA-Z]:\\\\)(?:[\\w\\-]+(?:\\/|\\\\))*[\\w\\-]+(?:\\.[\\w]+)?"},
            
            //WebLink ( NOTE for '"https://url.com"' this includes the last '"' in the match )
            //@"(https?://|www|https?://www).\S+", 
            //@"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
            {MpRegExType.Uri, @"(https?://|www|https?://www|file://).\S+"},
            {MpRegExType.Email, EMAIL_REGEX},
            {MpRegExType.PhoneNumber, @"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})"},
            {MpRegExType.Currency, @"[$£€¥][\d|\.]([0-9]{0,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.\d{0,2})?"},
            //HexColor (w/ or w/o alpha)
            {MpRegExType.HexColor,@"#([0-9]|[a-fA-F]){8}|#([0-9]|[a-fA-F]){6}" },
            
            //@"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b",            
            {MpRegExType.StreetAddress,@"\d+\s+((\w+\.?\s+)*\w+)?\s*(street|st|avenue|ave|road|rd|boulevard|blvd|way|drive|dr|court|ct|circle|cir|lane|ln|place|plaza|pl)\.?\s+(north|n|south|s|east|e|west|w)?\s*,?\s*(suite|ste|apartment|apt)?\.?\s*[a-zA-Z0-9\s]+\s*,?\s*[a-zA-Z]{2}\s+\d{5}(-\d{4})?" },
            {MpRegExType.Guid, @"[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}"},
            // NOTE would be better if this only matched the internal guid
            // For now using \\{t\\{.*\\}t\\} and replacing .* with Guid regex
            {MpRegExType.EncodedTextTemplate,@"\\{t\\{[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}\\}t\\}" },
            {MpRegExType.Is_NOT_Base64Str, @"[^a-zA-Z0-9+/=]"},
            {MpRegExType.HasSpecialCharacters, @"[^a-zA-Z0-9_.]+"},
            {MpRegExType.Is_NOT_Number,@"[^0-9.-]" },
            {MpRegExType.StartsWithWindowsStyleDirectory,@"^[a-zA-Z]:\\$" },
            {MpRegExType.ContainsInvalidFileNameChar,@"["+Regex.Escape(InvalidFileNameChars)+"]" },
            {MpRegExType.HexEncodedHtmlEntity,@"&(#)?([a-zA-Z0-9]*);" },
            {MpRegExType.ExactEmail, $"^{EMAIL_REGEX}$" }
        };

        private static Dictionary<MpRegExType, Regex> _regExLookup;
        public static Dictionary<MpRegExType, Regex> RegExLookup {
            get {
                if (_regExLookup == null) {
                    _regExLookup = _regExStrings.ToDictionary(
                                        x => x.Key,
                                        x => new Regex(x.Value));
                }
                return _regExLookup;
            }
        }
    }
}
