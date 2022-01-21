using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpSubTextTokenType {
        None = 0,
        FileOrFolder,
        Uri,
        Email,
        PhoneNumber,
        Currency,
        HexColor,
        StreetAddress,
        TemplateSegment,
       // HtmlTag
    }

    public static class MpRegEx {
        private static bool _isLoaded = false;

        public static List<Regex> RegExList { get; set; }

        private static string[] _regExStrings = new string[]{
            //none
            string.Empty,
            //File or folder path
            @"^(?:[\w]\:|\\)(\\[a-zA-Z_\-\s0-9\.()~!@#$%^&=+';,{}\[\]]+)+(\.("+MpPreferences.KnownFileExtensionsPsv+@")|(\\|\w))$",
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
            //Text Template (dynamically matching from CopyItemTemplate.TemplateName)
            //CopyItem.TemplateRegExMatchString,
            string.Empty
        };

        private static void Init() {
            RegExList = _regExStrings.Select(x =>
                new Regex(x, RegexOptions.ExplicitCapture | RegexOptions.Multiline)).ToList();
            _isLoaded = true;
        }

        public static Regex GetRegExForTokenType(MpSubTextTokenType tokenType) {
            if(!_isLoaded) {
                Init();
            }
            return RegExList[(int)tokenType];
        }

        public static bool IsMatch(MpSubTextTokenType tokenType, string compareStr) {
            return GetRegExForTokenType(tokenType).IsMatch(compareStr);
        }
    }
}
