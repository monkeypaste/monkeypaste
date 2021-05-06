using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public enum MpSubTextTokenType {
        None = 0,
        FileOrFolder,
        Uri,
        Email,
        PhoneNumber,
        Currency,
        HexColor6,
        StreetAddress,
        TemplateSegment,
        HexColor8
    }

    public class MpRegEx {
        private static readonly Lazy<MpRegEx> _Lazy = new Lazy<MpRegEx>(() => new MpRegEx());
        public static MpRegEx Instance { get { return _Lazy.Value; } }

        public List<string> RegExList { get; private set; } = new List<string> {
            //none
            string.Empty,
                //File or folder path
                @"^(?:[\w]\:|\\)(\\[a-zA-Z_\-\s0-9\.()~!@#$%^&=+';,{}\[\]]+)+(\.("+Properties.Settings.Default.KnownFileExtensionsPsv+@")|(\\|\w))$",
                //WebLink
                @"(?:https?://|www\.)\S+", 
                //@"[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
                //Email
                @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})",
                //PhoneNumber
                @"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})",
                //Currency
                @"[$|£|€|¥][\d|\.]([0-9]{0,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.\d{0,2})?",
                //HexColor (no alpha)
                @"#([0-9]|[a-fA-F]){6}",
                //StreetAddress
                @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b",                
                //Text Template (dynamically matching from CopyItemTemplate.TemplateName)
                //CopyItem.TemplateRegExMatchString,
                string.Empty,
                //HexColor (with alpha)
                @"#([0-9]|[a-fA-F]){8}",
        };

        public string GetRegExForTokenType(MpSubTextTokenType tokenType) {
            return RegExList[(int)tokenType];
        }
    }
}
