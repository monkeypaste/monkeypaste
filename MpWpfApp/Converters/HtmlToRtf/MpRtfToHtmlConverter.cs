using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HtmlAgilityPack;
using System.Diagnostics;

namespace MpWpfApp {
    public static class MpRtfToHtmlConverter {

        #region private static Variables
        private static double _indentCharCount = 5;

        private static string[] _inlineTags = new string[] {"span", "a", "em", "strong", "u", "s", "sub", "sup", "img"};
        private static string[] _blockTags = new string[] { "p", "ol", "ul", "li", "div", "table", "colgroup", "col", "tbody", "tr", "td", "iframe" };

        #endregion

        #region Properties

        #endregion

        public static string ConvertRtfToHtml(string rtf, Dictionary<string,string> globalBlockAttributes = null, Dictionary<string, string> globalInlineAttributes = null) {
            if(rtf == null) {
                return string.Empty;
            }
            return ConvertFlowDocumentToHtml(rtf.ToFlowDocument(), globalBlockAttributes,globalInlineAttributes);
        }

        public static string ConvertFlowDocumentToHtml(FlowDocument fd, Dictionary<string, string> globalBlockAttributes = null, Dictionary<string, string> globalInlineAttributes = null) {
            var sb = new StringBuilder();
            foreach (Block b in fd.Blocks) {
                if(b is Table t) {
                    // quill-better-table requires each row and cell to have unique identifiers
                    // and since html is generated from inside out the id's need to be generated before recursing
                    foreach(var trg in t.RowGroups) {
                        foreach(var tr in trg.Rows) {
                            tr.Tag = GetNewTableItemIdentifier("row");
                            foreach(var tc in tr.Cells) {
                                tc.Tag = GetNewTableItemIdentifier("cell");
                            }
                        }
                    }
                }
                sb.Append(ConvertTextElementToHtml(b));
            }
            
            string html = sb.ToString();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            SetGlobalAttributes(htmlDoc, globalBlockAttributes,globalInlineAttributes);
            return htmlDoc.DocumentNode.InnerHtml;
        }

        #region Global Attributes

        private static void SetGlobalAttributes(HtmlDocument htmlDoc, Dictionary<string,string> blockAttributes, Dictionary<string,string> inlineAttributes) {
            if(blockAttributes == null) {
                return;
            }

            foreach(var n in htmlDoc.DocumentNode.Descendants()) {
                if(n.NodeType == HtmlNodeType.Text) {
                    continue;
                }
                if(_blockTags.Contains(n.Name.ToLower())) {
                    foreach(var bkvp in blockAttributes) {
                        n.SetAttributeValue(bkvp.Key, bkvp.Value);
                    }
                } else if (_inlineTags.Contains(n.Name.ToLower())) {
                    foreach (var ikvp in inlineAttributes) {
                        n.SetAttributeValue(ikvp.Key, ikvp.Value);
                    }
                }
            }
        }

        #endregion

        private static string ConvertTextElementToHtml(TextElement te) {
            string html = string.Empty;
            var cl = GetChildren(te);
            foreach (var cte in cl) {
                html += ConvertTextElementToHtml(cte);
            }
            return ConvertTextElementToHtmlHelper(te,html);
        }

        private static string ConvertTextElementToHtmlHelper(TextElement te,string content) { 
            if(te is Table) {
                return WrapWithTable(te as Table, content);
            } else if (te is TableRowGroup trg) {
                return WrapWithTableRowGroup(trg, content);
            } else if (te is TableRow tr) {
                return WrapWithTableRow(tr, content);
            } else if (te is TableCell tc) {
                return WrapWithTableCell(tc, content);
            } else if (te is List) {
                return WrapWithList(te as List, content);
            } else if (te is ListItem) {
                return WrapWithListItem(te as ListItem, content);
            } else if (te is Paragraph) {
                return WrapWithParagraph(te as Paragraph,content);
            }  else if (te is LineBreak) {
                return @"<br>";
            } else if (te is Span) {
                if(content == "<br>") {
                    //when adding linebreak inside a span quill internally converts
                    //the one linebreak into two empty paragraphs
                    //so do not wrap linebreak with a span
                    return string.Empty;
                }
                if(content == " ") {
                    //wrapping unformatted spaces with a span makes quill ignore the space
                    //so just return the space
                    return content;
                }
                if((te as Inline).TextDecorations.Equals(TextDecorations.Underline)) {
                    content = WrapWithTag("u", content);
                }
                if (te.FontStyle.Equals(FontStyles.Italic)) {
                    content = WrapWithTag("em", content);
                }
                if (te.FontWeight.Equals(FontWeights.Bold)) {
                    content = WrapWithTag("strong", content);
                }
                return WrapWithSpan(te as Span, content);
            } else if(te is Run) {
                return (te as Run).Text;
            } else {
                throw new Exception(@"Unknown text element: " + te.ToString());
            }
        }

        private static string WrapWithTable(Table t, string content) {
            double tableWidth = 0;
            string colGroupInnerHtml = string.Empty;

            foreach(var tc in t.Columns) {
                double colWidth = 100;
                if(tc.Width.GridUnitType == GridUnitType.Pixel) {
                    colWidth = tc.Width.Value;
                } else {
                    Debugger.Break();
                    
                }
                colGroupInnerHtml += string.Format(@"<col width='{0}px'>", colWidth);
                tableWidth += colWidth;
            }
            string colGroupHtml = string.Format(@"<colgroup>{0}</colgroup>", colGroupInnerHtml);
            return string.Format(
                @"<div class='quill-better-table-wrapper'><table class='quill-better-table' style='width: {0}px'>{1}{2}</table></div>",
                tableWidth,
                colGroupHtml,
                content);
        }

        private static string WrapWithTableRowGroup(TableRowGroup trg, string content) {
            return WrapWithTag("tbody", content);
        }

        private static string WrapWithTableRow(TableRow tr, string content) {
            var sb = new StringBuilder(@"<tr");
            sb.AppendFormat(@" {0}>", string.Format(@"data-row='{0}'",tr.Tag as string));
            sb.AppendFormat(@"{0}</tr>", content);
            return sb.ToString();
        }

        private static string WrapWithTableCell(TableCell tc, string content) {
            var sb = new StringBuilder(@"<td");
            sb.AppendFormat(@" {0}>", GetTableCellAttributes(tc));
            sb.AppendFormat(@"{0}</td>", content);
            return sb.ToString();
        }

        private static string WrapWithList(List l, string content) {
            return WrapWithTag("ol", content);
        }

        private static string WrapWithListItem(ListItem li, string content) {
            var l = li.FindParentOfType<List>();
            string listType = @"bullet";
            if(l.MarkerStyle == TextMarkerStyle.Decimal) {
                listType = @"ordered";
            }
            return string.Format(
                @"<li data-list='{0}'><span class='ql-ui' contenteditable='false'></span>{1}</li>", 
                listType, 
                content);
        }

        private static string WrapWithParagraph(Paragraph p,string content) {
            if(p.Parent is ListItem) {
                //rtf list items are parents of paragraphs but quills are the direct content
                return content;
            }
            
            var sb = new StringBuilder(@"<p ");
            switch (p.TextAlignment) {
                case TextAlignment.Left:
                    sb.Append(@"class='ql-align-left");
                    break;
                case TextAlignment.Center:
                    sb.Append(@"class='ql-align-center");
                    break;
                case TextAlignment.Right:
                    sb.Append(@"class='ql-align-right");
                    break;
                case TextAlignment.Justify:
                    sb.Append(@"class='ql-align-justify");
                    break;
            }
            if(p.Parent is TableCell) {
                sb.Append(@" qlbt-cell-line");
            }
            sb.Append(GetParagraphIndent(p) + "'");
            if(p.Parent is TableCell tc) {
                string tcAttr = GetTableCellAttributes(tc);
                tcAttr = tcAttr
                    .Replace("rowspan", "data-rowspan")
                    .Replace("colspan", "data-colspan");
                string tcId = tc.Tag as string;
                string trId = (tc.Parent as TableRow).Tag as string;
                tcAttr += string.Format(@" data-row='{0}' data-cell='{1}'", trId, tcId);
                sb.Append(" " + tcAttr);
            }
            if(string.IsNullOrWhiteSpace(content)) {
                content = @"<br>";
            }
            sb.AppendFormat(@">{0}</p>",content);
            return sb.ToString();
        }        

        private static string WrapWithSpan(Span s, string content) {
            var sb = new StringBuilder(@"<span");
            if(s.Parent is Paragraph) {
                sb.AppendFormat(@" {0}>",GetSpanAttributes(s as Span));
            } else {
                sb.Append(@">");
            }            
            sb.AppendFormat(@"{0}</span>", content);
            return sb.ToString();
        }

        private static string WrapWithTag(string tag, string content) {
            return string.Format(@"<{0}>{1}</{0}>", tag, content);
        }

        private static string GetParagraphIndent(Paragraph p) {
            if (p.TextIndent > 0) {
                int indentLevel = (int)(p.TextIndent / _indentCharCount);
                return @" ql-indent-" + indentLevel; ;
            }
            return string.Empty;
        }

        private static string GetTableCellAttributes(TableCell tc) {
            return string.Format(@"rowspan='{0}' colspan='{1}' data-row='{2}'", tc.RowSpan, tc.ColumnSpan, (tc.Parent as TableRow).Tag as string);
        }


        private static string GetNewTableItemIdentifier(string prefix) {
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            string id = string.Empty;
            for (int i = 0; i < 4; i++) {
                id += chars[MpHelpers.Rand.Next(0, chars.Length - 1)];
            }
            return string.Format(@"{0}-{1}", prefix, id);
        }

        private static string GetSpanAttributes(Span s) {
            var sb = new StringBuilder();
            sb.AppendFormat(@"class='ql-font-{0}'", GetHtmlFont(s));
            sb.AppendFormat(GetFontSize(s));
            if(s.Foreground != null) {
                sb.AppendFormat(@" color: {0};", GetHtmlColor((s.Foreground as SolidColorBrush).Color));
            }
            if(s.Background != null) {
                sb.AppendFormat(@" background-color: {0};'", GetHtmlColor((s.Background as SolidColorBrush).Color));
            } else {
                sb.Append(@"'");
            }           
            
            return sb.ToString();
        }

        private static string GetFontSize(Span s) {
            double fs = (double)s.FontSize;//new FontSizeConverter().ConvertFrom(s.FontSize+"pt");
            MpRichTextFormatProperties.Instance.AddFontSize(fs);
            return string.Format(@" style='font-size: {0}px;", fs);
        }

        private static string GetHtmlColor(Color c) {
            MpRichTextFormatProperties.Instance.AddFontColor(c);
            return string.Format(@"rgb({0},{1},{2})", c.R, c.G, c.B);
        }

        private static string GetHtmlFont(Span s) {
            string ff = s.FontFamily.ToString().ToLower().Trim();
            MpRichTextFormatProperties.Instance.AddFont(ff);
            return ff.Replace(" ", "-");
        }

        private static List<TextElement> GetChildren(TextElement te) {
            var cl = new List<TextElement>();
            if (te is Table t) {
                foreach (var tbrg in t.RowGroups) {
                    foreach(var tbr in tbrg.Rows) {
                        cl.Add(tbr);
                    }
                }
            } else if (te is TableRow tbr) {
                foreach (var tc in tbr.Cells) {
                    cl.Add(tc);
                }
            } else if (te is TableRowGroup trg) {
                foreach(var tr in trg.Rows) {
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
                foreach(var i in (te as Span).Inlines) {
                    cl.Add(i);
                }
            }
            return cl;
        }       
        

        public static void Test() {
            //MpHtmlToRtfConverter.Test();

            //var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpRtfToHtmlConverter)).Assembly;
            //var stream = assembly.GetManifestResourceStream("MpWpfApp.Resources.TestData.quillFormattedTextSample1.html");
            //using (var reader = new System.IO.StreamReader(stream)) {
            //    string rtf = MpHtmlToRtfConverter.ConvertHtmlToRtf(reader.ReadToEnd());
            //    string html = MpRtfToHtmlConverter.ConvertRtfToHtml(rtf);
            //    MpHelpers.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtf2html.html", html, false);
            //}
            string itemGuid = System.Guid.NewGuid().ToString();
            string html = MpRtfToHtmlConverter.ConvertRtfToHtml(
                MpHelpers.ReadTextFromFile(@"C:\Users\tkefauver\Desktop\rtfTableSample.rtf"),
                new Dictionary<string, string>() { { "copyItemBlockGuid", itemGuid } },
                    new Dictionary<string, string>() { { "copyItemInlineGuid", itemGuid } });
            MpHelpers.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtf2html2.html", html, false);
        }
    }
}
