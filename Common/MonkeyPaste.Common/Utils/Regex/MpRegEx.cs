using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        ContainsInvalidFileNameChar
       // HtmlTag
    }

    public static class MpRegEx {        
        public static string KnownFileExtensions { get; set; } = @"rtf|txt|jpg|jpeg|png|svg|zip|csv|gif|pdf|doc|docx|xls|xlsx";

        public static string InvalidFileNameChars {
            get {
                string strTheseAreInvalidFileNameChars = new string(System.IO.Path.GetInvalidPathChars());
                strTheseAreInvalidFileNameChars += @":/?*" + "\"";
                return strTheseAreInvalidFileNameChars;
            }
        }
        private static List<string> _regExStrings = new List<string>{
            
            //none
            string.Empty,
            
            //File or folder path
            @"^(?:[\w]\:|\\)(\\[a-zA-Z_\-\s0-9\.()~!@#$%^&=+';,{}\[\]]+)+(\.("+KnownFileExtensions+@")|(\\|\w))$",
            
            //WebLink ( NOTE for '"https://url.com"' this includes the last '"' in the match )
            @"(?:https?://|www\.)\S+", 
            //@"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
            
            //Email
            @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})",
            
            //PhoneNumber
            @"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})",
            
            //Currency
            @"[$|£|€|¥][\d|\.]([0-9]{0,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.\d{0,2})?",
            
            //HexColor (w/ or w/o alpha)
            @"#([0-9]|[a-fA-F]){8}|#([0-9]|[a-fA-F]){6}",
            
            //StreetAddress
            @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b",                
            
            //Guid
            @"[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}",
            
            //EncodedTextTemplate 
            // NOTE would be better if this only matched the internal guid
            // For now using \\{t\\{.*\\}t\\} and replacing .* with Guid regex
            @"\\{t\\{[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}\\}t\\}",

            //Is_NOT_Base64Str
            @"[^a-zA-Z0-9+/=]",

            //HasSpecialCharacters
            @"[^a-zA-Z0-9_.]+",

            //Is_NOT_Number
            @"[^0-9.-]",

            //StartsWithWindowsStyleDirectory
            @"^[a-zA-Z]:\\$",

            //ContainsInvalidFileNameChar
            @"["+Regex.Escape(InvalidFileNameChars)+"]"
        };

        private static Dictionary<MpRegExType, Regex> _regExLookup;
        public static Dictionary<MpRegExType, Regex> RegExLookup { 
            get {
                if(_regExLookup == null) {
                    _regExLookup = _regExStrings.ToDictionary(
                                        x => (MpRegExType)_regExStrings.IndexOf(x),
                                        x => new Regex(x));
                }
                return _regExLookup;
            }
        }
    }
}
