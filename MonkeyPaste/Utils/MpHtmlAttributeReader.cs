using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MonkeyPaste {
    public class MpHtmlAttributeReader {
        private static readonly Lazy<MpHtmlAttributeReader> _Lazy = new Lazy<MpHtmlAttributeReader>(() => new MpHtmlAttributeReader());
        public static MpHtmlAttributeReader Instance { get { return _Lazy.Value; } }

        public List<MpHtmlTextTagAttribute> ReadAttributes(string attrStr) {
            var attrList = new List<MpHtmlTextTagAttribute>();

            var attrItemList = attrStr.Split(new string[] { "\" " }, StringSplitOptions.RemoveEmptyEntries);

            return attrList;
        }        
    }
}
