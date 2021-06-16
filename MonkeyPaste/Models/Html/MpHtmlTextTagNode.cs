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

    public class MpHtmlTextTagNode {
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

        public MpHtmlTextTagNode Parent { get; set; }
        public List<MpHtmlTextTagNode> ChildList { get; private set; } = new List<MpHtmlTextTagNode>();

        public MpHtmlTagType TagType { get; set; } = MpHtmlTagType.None;

        public List<MpHtmlTextTagAttribute> TagAttributeList { get; set; } = new List<MpHtmlTextTagAttribute>();

        public string Contents { get; set; } = string.Empty;

        public int Offset { get; set; } = 0;

        public static MpHtmlTextTagNode Create(string tagName, int offset, string html) {
            if (string.IsNullOrEmpty(tagName)) {
                return null;
            }
            int tagIdx = new List<string>(Enum.GetNames(typeof(MpHtmlTagType))).IndexOf(tagName);
            if (tagIdx < 1) {
                return null;
            }

            var newTag = new MpHtmlTextTagNode();
            newTag.TagType = (MpHtmlTagType)tagIdx;
            newTag.Offset = offset;

            string offsetHtml = html.Substring(offset);
            int asIdx = offsetHtml.IndexOf(" ");
            if(asIdx > 0) {
                int tseIdx = offsetHtml.IndexOf(">");
                int aeIdx = tseIdx - asIdx - 1;
                if (aeIdx > 0) {
                    string attributeStr = offsetHtml.Substring(asIdx + 1, aeIdx);
                    newTag.TagAttributeList = MpHtmlAttributeReader.Instance.ReadAttributes(attributeStr);
                    MpConsole.WriteLine(attributeStr);
                }
            }
            return newTag;
        }

        public MpHtmlTextTagNode() { }
        
        public void AddChild(MpHtmlTextTagNode child) {
            ChildList.Add(child);
        }


        public override string ToString() {
            var sb = new StringBuilder();
            string tagName = Enum.GetName(typeof(MpHtmlTagType), TagType);
            sb.Append($"<{tagName}>{Contents}</{tagName}>");
            foreach(var c in ChildList) {
                sb.Append(c.ToString());
            }
            return sb.ToString();
        }
    }
}
