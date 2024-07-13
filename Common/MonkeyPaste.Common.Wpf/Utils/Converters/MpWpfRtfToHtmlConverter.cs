using HtmlAgilityPack;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MonkeyPaste.Common.Wpf {
    public class MpWpfRtfToHtmlConverterProps {
        public string ParagraphTagName { get; set; } = "p";
    }
    public static class MpWpfRtfToHtmlConverter {
        #region Private Variables
        private static double _indentCharCount { get; set; } = 5;
        private static HtmlDocument _htmlDoc;
        private static MpWpfRtfToHtmlConverterProps _curProps;
        #endregion

        #region Constants
        #endregion

        #region Public Methods

        public static string ConvertFormatToHtml(string formatData, MpWpfRtfToHtmlConverterProps props = null) {
            // allow empty document creation
            _curProps = props == null ? new MpWpfRtfToHtmlConverterProps() : props;
            formatData = formatData == null ? string.Empty : formatData;
            var fd = formatData.ToFlowDocument();
            return ConvertFlowDocumentToHtml(fd);
        }

        #endregion

        #region Private Methods

        private static string ConvertFlowDocumentToHtml(FlowDocument fd) {
            _htmlDoc = new HtmlDocument();
            foreach (Block b in fd.Blocks) {
                if (b is Table t) {
                    // quill-better-table requires each row and cell to have unique identifiers
                    // and since html is generated from inside out the id's need to be generated before recursing
                    foreach (var trg in t.RowGroups) {
                        foreach (var tr in trg.Rows) {
                            tr.Tag = GetNewTableItemIdentifier("row");
                            foreach (var tc in tr.Cells) {
                                tc.Tag = GetNewTableItemIdentifier("cell");
                            }
                        }
                    }
                }
                var node = ConvertTextElementToHtml(b);
                _htmlDoc.DocumentNode.AppendChild(node);
            }

            try {
                string encoded_html = _htmlDoc.DocumentNode.InnerHtml;
                var errors = _htmlDoc.ParseErrors;
                foreach (var error in errors) {
                    MpConsole.WriteLine("rtf2html parse error: " + error);
                }
                return encoded_html;//HttpUtility.HtmlDecode(encoded_html);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error converting rtf to html.", ex);
            }
            return fd.ContentRange().Text;
        }

        private static HtmlNode ConvertTextElementToHtml(TextElement te) {
            List<HtmlNode> children = new List<HtmlNode>();
            var cl = GetChildren(te);
            foreach (var cte in cl) {
                var child_node = ConvertTextElementToHtml(cte);
                children.Add(child_node);
            }
            return ConvertTextElementToHtmlHelper(te, children);
        }

        private static HtmlNode ConvertTextElementToHtmlHelper(TextElement te, List<HtmlNode> children) {
            if (te is Table t) {
                return WrapWithTable(t, children);
            } else if (te is TableRowGroup trg) {
                return WrapWithTableRowGroup(trg, children);
            } else if (te is TableRow tr) {
                return WrapWithTableRow(tr, children);
            } else if (te is TableCell tc) {
                return WrapWithTableCell(tc, children);
            } else if (te is List l) {
                return WrapWithList(l, children);
            } else if (te is ListItem li) {
                return WrapWithListItem(li, children);
            } else if (te is InlineUIContainer iuic) {
                return UnwrapInlineUIContainer(iuic, children);
            } else if (te is Paragraph p) {
                return WrapWithParagraph(p, children);
            } else if (te is LineBreak) {
                return _htmlDoc.CreateElement("br");
            } else if (te is Span s) {
                return WrapWithSpan(s, children);
            } else if (te is Run r) {
                return ProcessRun(r, children);
            } else {
                throw new Exception(@"Unknown text element: " + te.ToString());
            }
        }

        private static string GetRunText(string text) {
            return text.EncodeSpecialHtmlEntities();
        }

        private static HtmlNode ProcessRun(Run r, List<HtmlNode> children) {
            if (!r.Text.ContainsEncodedSpecialHtmlEntities()) {
                // valid example "if (CopyItemData == "<p><br></p>" || CopyItemData == null)"

                // no encoded entities to wrap with code tag so return encoded run
                return _htmlDoc.CreateTextNode(GetRunText(r.Text));
            }
            // example "{'>',"&gt;" },"
            // since there's encoded entities will need return a container node (span)
            HtmlNode span_node = _htmlDoc.CreateElement("span");
            int cur_idx = 0;
            Match m = MpRegEx.RegExLookup[MpRegExType.HexEncodedHtmlEntity].Match(r.Text);
            while (m.Success) {
                int match_idx = r.Text.Substring(cur_idx).IndexOf(m.Value);

                if (match_idx > 0) {
                    // create lead run (in example "{'>',"")
                    string lead_text = r.Text.Substring(cur_idx, match_idx);
                    HtmlNode lead_text_node = _htmlDoc.CreateTextNode(GetRunText(lead_text));
                    HtmlNode lead_text_node_wrapper_span = _htmlDoc.CreateElement("span");
                    lead_text_node_wrapper_span.AppendChild(lead_text_node);
                    span_node.AppendChild(lead_text_node_wrapper_span);
                }

                // wrap encoded special entity in code tag
                HtmlNode match_text_node = _htmlDoc.CreateTextNode(m.Value);
                HtmlNode code_node = _htmlDoc.CreateElement("code");

                //
                code_node.AppendChild(match_text_node);
                span_node.AppendChild(code_node);

                cur_idx += match_idx + m.Value.Length;
                m = MpRegEx.RegExLookup[MpRegExType.HexEncodedHtmlEntity].Match(r.Text.Substring(cur_idx));
            }
            if (cur_idx < r.Text.Length) {
                // create trailing run after encoded special entities
                string trailing_text = r.Text.Substring(cur_idx);
                HtmlNode trail_text_node = _htmlDoc.CreateTextNode(GetRunText(trailing_text));
                HtmlNode trailing_text_node_wrapper_span = _htmlDoc.CreateElement("span");
                trailing_text_node_wrapper_span.AppendChild(trail_text_node);
                span_node.AppendChild(trailing_text_node_wrapper_span);
            }

            string valid_check = GetRunText(r.Text);
            string test_check = span_node.InnerText;
            if (valid_check != test_check) {
                MpConsole.WriteLine("Error encoding run.", true);
                MpConsole.WriteLine($"Actual Text:");
                MpConsole.WriteLine(r.Text);
                MpConsole.WriteLine($"Encoded Text:");
                MpConsole.WriteLine(valid_check);
                MpConsole.WriteLine($"Processed Text:");
                MpConsole.WriteLine(test_check, false, true);
                MpDebug.Break();
            }

            return span_node;
        }

        private static HtmlNode WrapWithTable(Table t, List<HtmlNode> children) {
            double tableWidth = 0;
            HtmlNode colGroupNode = _htmlDoc.CreateElement("colgroup");
            foreach (var tc in t.Columns) {
                double colWidth = 100;
                if (tc.Width.GridUnitType == GridUnitType.Pixel) {
                    colWidth = tc.Width.Value;
                } else {
                    MpDebug.Break();

                }
                HtmlNode colNode = _htmlDoc.CreateElement("col");
                colNode.SetAttributeValue("width", $"{colWidth}px");
                colGroupNode.AppendChild(colNode);
                tableWidth += colWidth;
            }

            HtmlNode tableNode = _htmlDoc.CreateElement("table");
            tableNode.AddClass("quill-better-table");
            tableNode.SetAttributeValue("style", $"width: {tableWidth}px;");
            tableNode.AppendChild(colGroupNode);
            children.ForEach(x => tableNode.AppendChild(x));

            HtmlNode tableContainerNode = _htmlDoc.CreateElement("div");
            tableContainerNode.AddClass("quill-better-table-wrapper");
            tableContainerNode.AppendChild(tableNode);
            return tableContainerNode;
        }

        private static HtmlNode WrapWithTableRowGroup(TableRowGroup trg, List<HtmlNode> children) {
            return WrapWithTag("tbody", children);
        }

        private static HtmlNode WrapWithTableRow(TableRow tr, List<HtmlNode> children) {
            var tr_node = _htmlDoc.CreateElement("tr");
            tr_node.SetAttributeValue("data-row", tr.Tag.ToString());
            children.ForEach(x => tr_node.AppendChild(x));
            return tr_node;
        }

        private static HtmlNode WrapWithTableCell(TableCell tc, List<HtmlNode> children) {
            var td_node = _htmlDoc.CreateElement("td");
            td_node = SetTableCellAttributes(tc, td_node);
            children.ForEach(x => td_node.AppendChild(x));
            return td_node;
        }

        private static HtmlNode WrapWithList(List l, List<HtmlNode> children) {
            if (l.MarkerStyle == TextMarkerStyle.Decimal) {
                return WrapWithTag("ol", children);
            }
            return WrapWithTag("ul", children);
        }

        private static HtmlNode WrapWithListItem(ListItem li, List<HtmlNode> children) {
            var l = li.FindParentOfType<List>();
            string listType = @"bullet";
            if (l.MarkerStyle == TextMarkerStyle.Decimal) {
                listType = @"ordered";
            } else if (l.MarkerStyle == TextMarkerStyle.Square) {
                listType = "unchecked";
            } else if (l.MarkerStyle == TextMarkerStyle.Box) {
                listType = "checked";
            }
            var bullet_span = _htmlDoc.CreateElement("span");
            bullet_span.SetAttributeValue("contenteditable", false.ToString());
            bullet_span.AddClass("ql-ui");
            var li_node = _htmlDoc.CreateElement("li");
            li_node.SetAttributeValue("data-list", listType);
            li_node.AppendChild(bullet_span);
            foreach (var child in children) {
                if (child.Name.ToLowerInvariant() == _curProps.ParagraphTagName) {
                    //rtf list items are parents of paragraphs but quills are the direct content
                    child.GetClasses().ForEach(x => li_node.AddClass(x));
                    child.GetAttributes().ForEach(x => li_node.SetAttributeValue(x.Name, x.Value));
                    child.ChildNodes.ForEach(x => li_node.AppendChild(x));
                } else {
                    li_node.AppendChild(child);
                }
            }
            children.ForEach(x => li_node.AppendChild(x));
            return li_node;
        }

        private static HtmlNode WrapWithParagraph(Paragraph p, List<HtmlNode> children) {
            string paragraph_tag_name = _curProps.ParagraphTagName.ToLowerInvariant();
            var p_node = _htmlDoc.CreateElement(paragraph_tag_name);
            string align_class = "ql-align-left";
            switch (p.TextAlignment) {
                case TextAlignment.Left:
                    align_class = "ql-align-left";
                    break;
                case TextAlignment.Center:
                    align_class = "ql-align-center";
                    break;
                case TextAlignment.Right:
                    align_class = "ql-align-right";
                    break;
                case TextAlignment.Justify:
                    align_class = "ql-align-justify";
                    break;
            }
            p_node.AddClass(align_class);
            p_node = SetParagraphIndent(p, p_node);
            if (p.Parent is TableCell tc) {
                p_node.AddClass("qlbt-cell-line");
                p_node = SetTableCellParagraphAttributes(tc, p_node);
            }
            children.ForEach(x => p_node.AppendChild(x));
            return p_node;
        }

        private static HtmlNode WrapWithSpan(Span span, List<HtmlNode> children) {
            HtmlNode span_node = null;
            if (span is Hyperlink hl) {
                span_node = WrapWithHyperlink(hl, children, span_node);
            }

            if (span.TextDecorations.Equals(TextDecorations.Underline) || span is Underline) {
                span_node = WrapWithTag("u", children, span_node);
            }
            if (span.FontStyle.Equals(FontStyles.Italic) || span is Italic) {
                span_node = WrapWithTag("em", children, span_node);
            }
            if (span.FontWeight.Equals(FontWeights.Bold) || span is Bold) {
                span_node = WrapWithTag("strong", children, span_node);
            }
            span_node = SetSpanAttributes(span, children, span_node);
            return span_node;
        }

        private static HtmlNode WrapWithTag(string tag, List<HtmlNode> children, HtmlNode cur_node = null) {
            var node = _htmlDoc.CreateElement(tag);
            if (cur_node == null) {
                children.ForEach(x => node.AppendChild(x));
            } else {
                // span should already have children attached
                node.AppendChild(cur_node);
            }
            return node;
        }

        private static HtmlNode WrapWithHyperlink(Hyperlink hl, List<HtmlNode> children, HtmlNode cur_node = null) {
            var a_node = WrapWithTag("a", children, cur_node);
            a_node.SetAttributeValue("href", hl.NavigateUri.AbsoluteUri);
            return a_node;
        }

        private static HtmlNode WrapWithImage(Image img, List<HtmlNode> children) {
            var bmpSrc = img.Source as BitmapSource;

            string srcAttrbValue = string.Format(@"data:image/png;base64,{0}", bmpSrc.ToBase64String());
            var img_node = WrapWithTag("img", children);
            img_node.SetAttributeValue("src", srcAttrbValue);
            return img_node;
        }

        private static HtmlNode UnwrapInlineUIContainer(InlineUIContainer ioc, List<HtmlNode> children) {
            var child = ioc.Child;
            if (child is Image img) {
                return WrapWithImage(img, children);
            }
            throw new Exception(@"Unknown InlineUIContainer child: " + child.ToString());
        }

        private static HtmlNode SetParagraphIndent(Paragraph p, HtmlNode p_node) {
            if (p.TextIndent > 0) {
                int indentLevel = (int)(p.TextIndent / _indentCharCount);
                p_node.AddClass($"ql-indent-{indentLevel}");
            }
            return p_node;
        }

        private static HtmlNode SetTableCellAttributes(TableCell tc, HtmlNode tr_node) {
            tr_node.SetAttributeValue("rowspan", tc.RowSpan.ToString());
            tr_node.SetAttributeValue("colspan", tc.ColumnSpan.ToString());
            tr_node.SetAttributeValue("data-row", (tc.Parent as TableRow).Tag.ToString());
            return tr_node;
        }
        private static HtmlNode SetTableCellParagraphAttributes(TableCell tc, HtmlNode p_node) {
            p_node.SetAttributeValue("data-rowspan", tc.RowSpan.ToString());
            p_node.SetAttributeValue("data-colspan", tc.ColumnSpan.ToString());
            p_node.SetAttributeValue("data-row", (tc.Parent as TableRow).Tag.ToString());
            p_node.SetAttributeValue("data-cell", tc.Tag.ToString());
            return p_node;
        }

        private static string GetNewTableItemIdentifier(string prefix) {
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            string id = string.Empty;
            for (int i = 0; i < 4; i++) {
                id += chars[MpRandom.Rand.Next(0, chars.Length - 1)];
            }
            return string.Format(@"{0}-{1}", prefix, id);
        }

        private static HtmlNode SetSpanAttributes(Span s, List<HtmlNode> children, HtmlNode span_node) {
            if (span_node == null) {
                span_node = WrapWithTag("span", children);
            }
            span_node.AddClass($"ql-font-{GetHtmlFont(s)}");
            var style_parts = new List<string>();
            if (s.FontSize.IsNumber()) {
                string fs = GetFontSize(s);
                style_parts.Add($"font-size: {fs};");
                span_node.AddClass($"ql-size-{fs}");
            }
            if (s.Foreground is SolidColorBrush fg_scb) {
                style_parts.Add($"color: {GetHtmlColor(fg_scb.Color)};");
            }
            if (s.Background is SolidColorBrush bg_scb) {
                style_parts.Add($"background-color: {GetHtmlColor(bg_scb.Color)};");
            }
            span_node.SetAttributeValue("style", string.Join(" ", style_parts));
            return span_node;
        }

        private static string GetFontSize(Span s) {
            double fs = (double)((int)s.FontSize);
            return $"{fs}px";
        }

        private static string GetHtmlColor(Color c) {
            return string.Format(@"rgb({0},{1},{2})", c.R, c.G, c.B);
        }

        private static string GetHtmlFont(Span s) {
            string ff = s.FontFamily.ToString().ToLowerInvariant().Trim();
            return ff.Replace(" ", "-");
        }

        private static List<TextElement> GetChildren(TextElement te) {
            var cl = new List<TextElement>();
            if (te is Table t) {
                foreach (var tbrg in t.RowGroups) {
                    foreach (var tbr in tbrg.Rows) {
                        cl.Add(tbr);
                    }
                }
            } else if (te is TableRow tbr) {
                foreach (var tc in tbr.Cells) {
                    cl.Add(tc);
                }
            } else if (te is TableRowGroup trg) {
                foreach (var tr in trg.Rows) {
                    cl.Add(tr);
                }
            } else if (te is TableCell tc) {
                foreach (var b in tc.Blocks) {
                    cl.Add(b);
                }
            } else if (te is List) {
                foreach (var li in (te as List).ListItems) {
                    cl.Add(li);
                }
            } else if (te is ListItem) {
                foreach (var b in (te as ListItem).Blocks) {
                    cl.Add(b);
                }
            } else if (te is Paragraph) {
                foreach (var i in (te as Paragraph).Inlines) {
                    cl.Add(i);
                }
            } else if (te is Span) {
                foreach (var i in (te as Span).Inlines) {
                    cl.Add(i);
                }
            }
            return cl;
        }


        public static string Test() {
            string rtf = ReadTextFromFile(@"C:\Users\tkefauver\Desktop\rtf_sample_short.rtf");
            string plain_html = ConvertFormatToHtml(rtf);

            WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtf_sample_to_plain_html_test.html", plain_html);
            return plain_html;
        }

        private static string ReadTextFromFile(string filePath) {
            try {
                using (StreamReader f = new StreamReader(filePath)) {
                    string outStr = string.Empty;
                    outStr = f.ReadToEnd();
                    f.Close();
                    return outStr;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return null;
            }
        }
        private static string WriteTextToFile(string filePath, string text) {
            try {
                using (var of = new StreamWriter(filePath)) {
                    of.Write(text);
                    of.Close();
                    return filePath;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path '{filePath}' with text '{text}'", ex);
                return null;
            }
        }

        #endregion
    }
}
