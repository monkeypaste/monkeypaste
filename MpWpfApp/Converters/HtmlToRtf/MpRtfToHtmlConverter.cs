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
using System.Linq;
using CefSharp;
using System.Runtime.InteropServices;
using System.Web.UI.WebControls.WebParts;

namespace MpWpfApp {
    public static class MpRtfToHtmlConverter {

        #region private static Variables
        private static double _indentCharCount = 5;
        #endregion

        #region Properties

        #endregion

        public static string ConvertRtfToHtml(string rtf, Dictionary<string,string> globalAttributes = null) {
            if(rtf == null) {
                return string.Empty;
            }
            return ConvertFlowDocumentToHtml(rtf.ToFlowDocument(), globalAttributes);
        }

        public static string ConvertFlowDocumentToHtml(FlowDocument fd, Dictionary<string, string> globalAttributes = null) {
            var sb = new StringBuilder();
            foreach (Block b in fd.Blocks) {
                sb.Append(ConvertTextElementToHtml(b));
            }
            
            string html = sb.ToString();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            SetGlobalAttributes(htmlDoc, globalAttributes);
            return htmlDoc.DocumentNode.InnerHtml;
        }

        #region Global Attributes

        private static void SetGlobalAttributes(HtmlDocument htmlDoc, Dictionary<string,string> attributes) {
            if(attributes == null) {
                return;
            }

            foreach(var n in htmlDoc.DocumentNode.ChildNodes.Where(x=>x.NodeType == HtmlNodeType.Element)) {
                SetGlobalAttributesHelper(n, attributes);
            }
        }

        private static void SetGlobalAttributesHelper(HtmlNode n, Dictionary<string, string> attributes) {
            foreach(var kvp in attributes) {
                SetAttribute(n, kvp);
            }
            foreach(var cn in n.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element)) {
                SetGlobalAttributesHelper(cn, attributes);
            }
        }

        private static void SetAttribute(HtmlNode n, KeyValuePair<string,string> kvp) {
            n.SetAttributeValue(kvp.Key, kvp.Value);
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
            sb.Append(GetParagraphIndent(p) + "'");
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
            string ff = s.FontFamily.ToString().ToLower();
            MpRichTextFormatProperties.Instance.AddFont(ff);
            return ff.Replace(" ", "-");
        }

        private static List<TextElement> GetChildren(TextElement te) {
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
        

        public static void Test() {
            //MpHtmlToRtfConverter.Test();

            //var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpRtfToHtmlConverter)).Assembly;
            //var stream = assembly.GetManifestResourceStream("MpWpfApp.Resources.TestData.quillFormattedTextSample1.html");
            //using (var reader = new System.IO.StreamReader(stream)) {
            //    string rtf = MpHtmlToRtfConverter.ConvertHtmlToRtf(reader.ReadToEnd());
            //    string html = MpRtfToHtmlConverter.ConvertRtfToHtml(rtf);
            //    MpHelpers.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtf2html.html", html, false);
            //}
            string rtf = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman; } {\f2\fcharset0 Georgia; } {\f3\fcharset0 Consolas; } }
{\colortbl\red0\green0\blue0;\red255\green255\blue255;\red0\green128\blue0;\red0\green0\blue255;\red43\green145\blue175; }\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs19\f3\cf0 \cf0\qj{\f3 {\cf2\ltrch //rtbvm.HasViewChanged = true;}\li0\ri0\sa0\sb0\fi0\ql\par}
{\f3 {\ltrch rtbvm.OnPropertyChanged(} {\cf3\ltrch nameof} {\ltrch(rtbvm.CurrentSize)); }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 {\ltrch                 } {\cf3\ltrch var} {\ltrch cilv = } {\cf3\ltrch this} {\ltrch.GetVisualAncestor <} {\cf4\ltrch MpContentListView} {\ltrch > (); }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 {\ltrch cilv.UpdateAdorner(); }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 \li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 {\ltrch                 } {\cf3\ltrch var} {\ltrch rtbl = cilv.GetVisualDescendents <} {\cf4\ltrch RichTextBox} {\ltrch > (); }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 {\ltrch                 } {\cf3\ltrch double} {\ltrch totalHeight = rtbl.Sum(x => x.ActualHeight) + } {\cf4\ltrch MpMeasurements} {\ltrch.Instance.ClipTileEditToolbarHeight + } {\cf4\ltrch MpMeasurements} {\ltrch.Instance.ClipTileDetailHeight; }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 \li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 {\ltrch                 } {\cf3\ltrch var} {\ltrch ctcv = } {\cf3\ltrch this} {\ltrch.GetVisualAncestor <} {\cf4\ltrch MpClipTileContainerView} {\ltrch > (); }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 {\ltrch ctcv.ExpandBehavior.Resize(totalHeight); }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 {\ltrch                 }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 {\ltrch                 } {\cf3\ltrch var} {\ltrch sv = cilv.ContentListBox.GetScrollViewer(); }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 {\ltrch sv.InvalidateScrollInfo(); }\li0\ri0\sa0\sb0\fi0\ql\par}
                            {\f3 \li0\ri0\sa0\sb0\fi0\ql\par}
                        }
                    }";

            string html = MpRtfToHtmlConverter.ConvertRtfToHtml(
                rtf,
                new Dictionary<string, string>() {
                    {"test1", "yo this is test1 value"},
                    {"test2", null }
                });
            MpHelpers.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtf2html.html", html, false);
        }
    }
}
