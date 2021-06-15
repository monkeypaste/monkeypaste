using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public enum MpHtmlTagType {
        None = 0,
        p,
        strong,
        em,
        u,
        a,
        ol,
        li,
        span,
        table,
        tbody,
        tr,
        td,
        h1,
        h2,
        h3,
        h4,
        h5,
        DocumentRoot //document root
    }

    public class MpHtmlTextTag {
        public static List<string> TagNames = new List<string> {
            string.Empty,
            "p",
            "strong",
            "em",
            "u",
            "a",
            "ol",
            "li",
            "span",
            "table",
            "tbody",
            "tr",
            "td",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "dr" //document root
        };

        public MpHtmlTextTag Parent { get; set; }
        public MpHtmlTextTag Child { get; set; }

        public MpHtmlTagType TagType { get; set; } = MpHtmlTagType.None;

        public List<MpHtmlTextTagAttribute> TagAttributeList { get; set; } = new List<MpHtmlTextTagAttribute>();

        public string Contents { get; set; } = string.Empty;

        public int Offset { get; set; } = 0;

        public static MpHtmlTextTag Create(string tagName, string contents, int offset) {
            var newTag = new MpHtmlTextTag();
            if (string.IsNullOrEmpty(tagName)) {
                return newTag;
            }
            int tagIdx = TagNames.IndexOf(tagName);
            if(tagIdx < 1) {
                return newTag;
            }

            newTag.TagType = (MpHtmlTagType)tagIdx;
            newTag.Contents = contents;
            newTag.Offset = offset;
            return newTag;
        }

        public MpHtmlTextTag() { }

        
        public void SetChild(MpHtmlTextTag child) {

        }

        public override string ToString() {
            var sb = new StringBuilder();
            string tagName = Enum.GetName(typeof(MpHtmlTagType), TagType);
            sb.Append($"<{tagName}>{Contents}</{tagName}>");
            if(Child != null) {
                sb.Append(Child.ToString());
            }
            return sb.ToString();
        }
    }
}
