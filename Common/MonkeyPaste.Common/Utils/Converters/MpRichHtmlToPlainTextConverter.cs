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

            var pre_nodes = doc.DocumentNode.SafeSelectNodes("//pre");
            if(pre_nodes.Any()) {
                // for pre nodes, strip any internal <br> and add line break to end of div
                var pre_divs = pre_nodes.SelectMany(x => x.SafeSelectNodes("//div")).Where(x=>x.HasClass("ql-code-block"));
                pre_divs.SelectMany(x => x.SafeSelectNodes("//br")).ForEach(x => x.Remove());
                foreach(var pre_div in pre_divs) {
                    pre_div.InsertAfter(doc.CreateTextNode(envNewLine), pre_div.LastChild);
                }
                
                var pre_codes = pre_nodes.SelectMany(x => x.SafeSelectNodes("//code")).Where(x=>x.HasClass("ql-code-block"));
                pre_codes.SelectMany(x => x.SafeSelectNodes("//br")).ForEach(x => x.Remove());
                foreach(var pre_code in pre_codes) {
                    pre_code.InsertAfter(doc.CreateTextNode(envNewLine), pre_code.LastChild);
                }
            }
            var break_nodes = doc.DocumentNode.SafeSelectNodes("//br");
            foreach (HtmlNode node in break_nodes) {
                node.ParentNode.ReplaceChild(doc.CreateTextNode(envNewLine), node);
            }
            if (!break_nodes.Any() &&
                doc.DocumentNode.SafeSelectNodes("//p") is HtmlNodeCollection pars && pars.Count > 1) {
                // TODO? may need to make lookup list of block elements besides p's here, not sure

                // NOTE1 special case for plain text mode rtf convert
                // rtf->html doesn't insert <br> at end of <p> because quill does it automatically
                // so when there are p's but no br's it wasn't pre-processed by quill convert

                // NOTE2 not adding line break when there's only 1 paragraph
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
