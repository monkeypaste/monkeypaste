using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Web;

namespace MonkeyPaste.Common {
    public static class MpRichHtmlToPlainTextConverter {
        public static string Convert(string html, string envNewLine = null) {
            string pt = FormatLineBreaks(html, envNewLine).DecodeSpecialHtmlEntities();
            return pt;
        }

        private static string FormatLineBreaks(string html, string envNewLine = null) {
            envNewLine = envNewLine == null ? Environment.NewLine : envNewLine;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (HtmlNode node in doc.DocumentNode.SafeSelectNodes("//br")) {
                node.ParentNode.ReplaceChild(doc.CreateTextNode(envNewLine), node);
            }
            return doc.DocumentNode.InnerText.Trim();
        }

        //here's the extension method I use
        private static HtmlNodeCollection SafeSelectNodes(this HtmlNode node, string selector) {
            return (node.SelectNodes(selector) ?? new HtmlNodeCollection(node));
        }
    }
}
