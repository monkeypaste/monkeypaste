using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public enum MpHtmlAttributeType {
        None = 0,
        Class,
        DataList,
        Style,
        DataRow
    }

    public class MpHtmlTextTagAttribute {
        public static string [] AttributeNames = new string [] { 
            string.Empty,
            "class",
            "data-list",
            "style",
            "data-row"
        };


    }
}
