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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MpWpfApp {
    public class MpRtfToHtmlConverter {
        #region Singleton
        private static readonly Lazy<MpRtfToHtmlConverter> _Lazy = new Lazy<MpRtfToHtmlConverter>(() => new MpRtfToHtmlConverter());
        public static MpRtfToHtmlConverter Instance { get { return _Lazy.Value; } }

        private MpRtfToHtmlConverter() { }
        #endregion

        #region Private Variables
        private double _indentCharCount = 5;
        #endregion

        #region Properties

        #endregion

        public string ConvertRtfToHtml(string rtf) {
            var fd = rtf.ToFlowDocument();
            var sb = new StringBuilder();
            foreach(Block b in fd.Blocks) {
                sb.Append(ConvertTextElementToHtml(b));
            }
            return sb.ToString();
        }

        private string ConvertTextElementToHtml(TextElement te) {
            string html = string.Empty;
            var cl = GetChildren(te);
            foreach (var cte in cl) {
                html += ConvertTextElementToHtml(cte);
            }
            return ConvertTextElementToHtmlHelper(te,html);
        }

        private string ConvertTextElementToHtmlHelper(TextElement te,string content) { 
            if (te is List) {
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
        private string WrapWithList(List l, string content) {
            return WrapWithTag("ol", content);
        }

        private string WrapWithListItem(ListItem li, string content) {
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

        private string WrapWithParagraph(Paragraph p,string content) {
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
            sb.Append(GetParagraphIndent(p) + "'");
            sb.AppendFormat(@">{0}</p>",content);
            return sb.ToString();
        }        

        private string WrapWithSpan(Span s, string content) {
            var sb = new StringBuilder(@"<span");
            if(s.Parent is Paragraph) {
                sb.AppendFormat(@" {0}>",GetSpanAttributes(s as Span));
            } else {
                sb.Append(@">");
            }            
            sb.AppendFormat(@"{0}</span>", content);
            return sb.ToString();
        }

        private string WrapWithTag(string tag, string content) {
            return string.Format(@"<{0}>{1}</{0}>", tag, content);
        }

        private string GetParagraphIndent(Paragraph p) {
            if (p.TextIndent > 0) {
                int indentLevel = (int)(p.TextIndent / _indentCharCount);
                return @" ql-indent-" + indentLevel; ;
            }
            return string.Empty;
        }
        private string GetSpanAttributes(Span s) {
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

        private string GetFontSize(Span s) {
            double fs = (double)new FontSizeConverter().ConvertFrom(s.FontSize+"pt");
            MpRichTextFormatProperties.Instance.AddFontSize(fs);
            return string.Format(@" style='font-size: {0}px;", fs);
        }

        private string GetHtmlColor(Color c) {
            MpRichTextFormatProperties.Instance.AddFontColor(c);
            return string.Format(@"rgb({0},{1},{2})", c.R, c.G, c.B);
        }

        private string GetHtmlFont(Span s) {
            string ff = s.FontFamily.ToString().ToLower();
            MpRichTextFormatProperties.Instance.AddFont(ff);
            return ff.Replace(" ", "-");
        }

        private List<TextElement> GetChildren(TextElement te) {
            var cl = new List<TextElement>();
            if (te is List) {
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
        

        public void Test() {
            MpHtmlToRtfConverter.Instance.Test();

            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpRtfToHtmlConverter)).Assembly;
            var stream = assembly.GetManifestResourceStream("MpWpfApp.Resources.TestData.quillFormattedTextSample1.html");
            using (var reader = new System.IO.StreamReader(stream)) {
                string rtf = MpHtmlToRtfConverter.Instance.ConvertHtmlToRtf(reader.ReadToEnd());
                string html = MpRtfToHtmlConverter.Instance.ConvertRtfToHtml(rtf);
                MpHelpers.Instance.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtf2html.html", html, false);
            }
        }
    }
}
