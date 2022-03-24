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
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public static class MpHtmlToRtfConverter {
        #region private static Variables
        private static double _indentCharCount = 5;
        #endregion

        #region Properties

        #endregion

        public static string ConvertHtmlToRtf(string html) {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var fd = string.Empty.ToFlowDocument();
            fd.Blocks.Clear();
            foreach (var htmlBlockNode in htmlDoc.DocumentNode.ChildNodes) {
                fd.Blocks.Add(ConvertHtmlNode(htmlBlockNode) as Block);
            }
            return fd.ToRichText();
        }
        
        private static TextElement ConvertHtmlNode(HtmlNode n) {
            var cel = new List<TextElement>();
            foreach (var c in n.ChildNodes) {
                cel.Add(ConvertHtmlNode(c));
            }
            return CreateTextElement(n, cel.ToArray());
        }

        private static TextElement CreateTextElement(HtmlNode n, TextElement[] cl) {
            var te = GetTextElement(n);
            foreach (var c in cl) {
                te = AddChildToElement(te, c);
            }
            return FormatTextElement(n.Attributes,te);
        }

        private static TextElement GetTextElement(HtmlNode n) {
            TextElement te = null;
            switch (n.Name) {
                case "#text":
                    te = new Run(n.InnerText);
                    break;
                case "img":
                    var ic = new InlineUIContainer();
                    var img = new System.Windows.Controls.Image();
                    ic.Child = img;
                    return ic;
                case "em":
                    te = new Italic();
                    break;
                case "span":
                    te = new Span();
                    break;
                case "strong":
                    te = new Bold();
                    break;
                case "u":
                    te = new Underline();
                    break;
                case "br":
                    te = new LineBreak();
                    break;
                case "a":
                    te = new Hyperlink();
                    break;
                case "p":
                    te = new Paragraph();
                    break;
                case "li":
                    te = new ListItem();
                    break;
                case "ol":
                    te = new List();
                    //since wpf handles list types by list and not list items (like quill)
                    //this is a special case
                    if(n.ChildNodes.Count > 0) {
                        string listTypeName = n.FirstChild.GetAttributeValue("data-list", string.Empty);
                        if (listTypeName == "unordered") {
                            (te as List).MarkerStyle = TextMarkerStyle.Disc;
                        } else if (listTypeName == "ordered") {
                            (te as List).MarkerStyle = TextMarkerStyle.Decimal;
                        }
                    }
                    break;
                // add table types
                default:
                    throw new Exception("Unhanlded html doc element: " + n.Name);
            }
            return te;
        }

        private static TextElement FormatTextElement(HtmlAttributeCollection ac, TextElement te) {
            foreach (var a in ac) {
                switch (a.Name) {
                    case "class":
                        var cvl = a.Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var cv in cvl) {
                            te = ApplyClassFormatting(te, cv.Trim());
                        }                        
                        break;
                    case "style":
                        var svl = a.Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var sv in svl) {
                            te = ApplyStyleFormatting(te, sv.Trim());
                        }
                        break;
                    case "src":                        
                        te = ApplyImgSrcFormatting(te, a.Value);
                        break;
                    case "href":
                        te = ApplyHrefFormatting(te, a.Value);
                        break;
                    case "data-list":
                        //special case since html handles list types differently
                        break;
                    case "contenteditable":

                        break;
                    case "data-row":

                        break;
                }
                
            }
            return te;
        }

        private static TextElement ApplyHrefFormatting(TextElement te, string hv) {
            (te as Hyperlink).NavigateUri = new Uri(hv);
            return te;
        }
        private static TextElement ApplyImgSrcFormatting(TextElement te, string sv) {
            var srcvl = sv.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string imgStr = srcvl[1];//.Replace(@"\", string.Empty);
            int mod4 = imgStr.Length % 4;
            if (mod4 > 0) {
                imgStr += new string('=', 4 - mod4);
            }
            var img = te.FindChildren<Image>().FirstOrDefault();
            //byte[] data = Convert.FromBase64String(imgStr);
            //string decodedString = Encoding.UTF8.GetString(data);
            img.Source = imgStr.ToBitmapSource();
            img.Height = 100;
            img.Width = 100;
            return te;
        }

        private static TextElement ApplyClassFormatting(TextElement te, string cv) {
            if (cv.StartsWith("ql-font-")) {
                te.FontFamily = GetFontFamily(cv);                
            } else if (cv.Contains("ql-align-left")) {
                (te as Block).TextAlignment = TextAlignment.Left;
            } else if (cv.Contains("ql-align-center")) {
                (te as Paragraph).TextAlignment = TextAlignment.Center;
            } else if (cv.Contains("ql-align-right")) {
                 (te as Block).TextAlignment = TextAlignment.Right;
            } else if (cv.Contains("ql-align-justify")) {
                (te as Block).TextAlignment = TextAlignment.Justify;
            } else if(cv.Contains("ql-indent-")) {
                (te as Paragraph).TextIndent = GetIndentLevel(cv) * _indentCharCount;
            }
            return te;
        }

        private static TextElement ApplyStyleFormatting(TextElement te, string sv) {
            if (sv.StartsWith("color")) {
                var itemColorBrush = ParseRgb(sv);
                te.Foreground = itemColorBrush;
            } else if (sv.StartsWith("background-color")) {
                var itemColorBrush = ParseRgb(sv);
                te.Background = itemColorBrush;
            } else if(sv.StartsWith("font-size")) {
                te.FontSize = GetFontSize(sv);
            }
            return te;
        }
        private static TextElement AddChildToElement(TextElement te, TextElement cte) {
            if (te is List) {
                (te as List).ListItems.Add(cte as ListItem);
            } else if (te is ListItem) {
                if((te as ListItem).Blocks.Count == 0) {
                    //special case since wpf requires list items to be in a paragraph
                    //we must add them implicitly
                    (te as ListItem).Blocks.Add(new Paragraph());
                }
                ((te as ListItem).Blocks.FirstBlock as Paragraph).Inlines.Add(cte as Inline);
            } else if (te is Paragraph) {
                (te as Paragraph).Inlines.Add(cte as Inline);
            } else if(te is Span) {
                (te as Span).Inlines.Add(cte as Inline);
            }
            return te;
        }

        public static Brush ParseRgb(string text) {
            Brush defaultBrush = Brushes.Black;

            int rgbOpenIdx = text.IndexOf("(");
            if(rgbOpenIdx < 0) {
                return defaultBrush;
            }

            string commaReplacement = string.Empty;
            if(!text.Substring(rgbOpenIdx + 1).Contains(" ")) {
                commaReplacement = " ";
            }
            string rgbColors = text.Substring(rgbOpenIdx + 1).Replace(",", commaReplacement).Replace(")", string.Empty).Replace(";",string.Empty);
            var rgbItemList = rgbColors.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            
            var color = new Color();
            color.A = 255;
            color.R = Convert.ToByte(rgbItemList[0]);
            color.G = Convert.ToByte(rgbItemList[1]);
            color.B = Convert.ToByte(rgbItemList[2]);

            MpRichTextFormatProperties.Instance.AddFontColor(color);

            return new SolidColorBrush(color);
        }

        public static double GetFontSize(string styleValue) {
            //for some reason wpf will not accept px values and converts to 3/4 size (for 96DPI)
            //but giving pt will use the displays DIP
            //string fontSizeStr = styleValue.Replace("font-size: ", string.Empty).Replace("px","pt");
            //double fs = (double)new FontSizeConverter().ConvertFrom(fontSizeStr);
            string fontSizeStr = styleValue.Replace("font-size: ", string.Empty).Replace("px", string.Empty);
            double fs = 12;
            try {
                fs = (double)Convert.ToDouble(fontSizeStr);
            } catch(Exception ex) {
                MpConsole.WriteTraceLine(ex);
            }
            MpRichTextFormatProperties.Instance.AddFontSize(fs);
            return fs;
        }

        public static double GetIndentLevel(string classValue) {
            return Convert.ToDouble(classValue.Replace("ql-indent-", string.Empty));
        }

        private static FontFamily GetFontFamily(string classValue) {
            string defaultFontName = "arial";
            FontFamily defaultFontFamily = null;
            FontFamily closestFontFamily = null;
            string fontName = classValue.Replace("ql-font-", string.Empty).Replace("-"," ");
            foreach (var ff in Fonts.SystemFontFamilies) {
                string ffName = ff.ToString().ToLower();
                if(ffName.Contains(fontName)) {
                    closestFontFamily = ff;
                }
                if(ffName == fontName) {
                    closestFontFamily = ff;
                    break;
                }
                if(ffName == defaultFontName) {
                    defaultFontFamily = ff;
                }
            }

            if (closestFontFamily != null) {
                //MpConsole.WriteLine("Could not find exact system font: " + fontName + " using "+closestFontFamily.ToString()+" instead");
                MpRichTextFormatProperties.Instance.AddFont(closestFontFamily.ToString().ToLower());
                return closestFontFamily;
            }
            MpConsole.WriteLine("Could not find system font: " + fontName);
            return defaultFontFamily;
        }

        public static void Test() {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpHtmlToRtfConverter)).Assembly;
            var stream = assembly.GetManifestResourceStream("MpWpfApp.Resources.TestData.quillFormattedTextSample1.html");
            using (var reader = new System.IO.StreamReader(stream)) {
                var html = reader.ReadToEnd();
                string rtf = ConvertHtmlToRtf(html);
                MpHelpers.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtftest.rtf", rtf, false);
            }
        }
    }
}
