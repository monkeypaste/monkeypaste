using HtmlAgilityPack;
using System;
using System.Linq;
using System.Xml.Linq;

namespace MonkeyPaste.Common {
    public static class MpRichHtmlToPlainTextConverter {
        public static string Convert(string html, string envNewLine = null) {
            string pt = FormatLineBreaks(html, envNewLine)
                .DecodeSpecialHtmlEntities();
            pt = pt
                .DecodeHtmlHexCharacters();
            return pt;
        }

        private static string FormatLineBreaks(string html, string envNewLine = null) {
            envNewLine = envNewLine == null ? Environment.NewLine : envNewLine;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var break_nodes = doc.DocumentNode.SafeSelectNodes("//br");
            foreach (HtmlNode node in break_nodes) {
                node.ParentNode.ReplaceChild(doc.CreateTextNode(envNewLine), node);
            }
            if (!break_nodes.Any() &&
                doc.DocumentNode.SafeSelectNodes("//p") is HtmlNodeCollection pars && pars.Count > 0) {
                // special case for plain text mode rtf convert
                // rtf->html doesn't insert <br> at end of <p> because quill does it automatically
                // so when there are p's but no br's it wasn't pre-processed by quill convert
                foreach (var par in pars) {
                    par.InsertAfter(doc.CreateTextNode(envNewLine), par.LastChild);
                }
            }
            return doc.DocumentNode.InnerText;
        }

        //here's the extension method I use
        private static HtmlNodeCollection SafeSelectNodes(this HtmlNode node, string selector) {
            return (node.SelectNodes(selector) ?? new HtmlNodeCollection(node));
        }
    }
}
