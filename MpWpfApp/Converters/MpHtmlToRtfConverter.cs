using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HtmlAgilityPack;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MpWpfApp {
    public class MpHtmlToRtfConverter {
        #region Singleton
        private static readonly Lazy<MpHtmlToRtfConverter> _Lazy = new Lazy<MpHtmlToRtfConverter>(() => new MpHtmlToRtfConverter());
        public static MpHtmlToRtfConverter Instance { get { return _Lazy.Value; } }

        private MpHtmlToRtfConverter() { }
        #endregion

        #region Private Variables
        private RichTextBox _rtb;
        private double _indentCharCount = 5;
        #endregion

        #region Properties

        #endregion

        public string ConvertHtmlToRtf(string html) {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            _rtb = new RichTextBox();
            _rtb.Document = string.Empty.ToFlowDocument();
            _rtb.Document.Blocks.Clear();
            foreach (var htmlBlockNode in htmlDoc.DocumentNode.ChildNodes) {
                _rtb.Document.Blocks.Add(ConvertHtmlNode(htmlBlockNode) as Block);
            }
            return _rtb.Document.ToRichText();
        }
        
        private TextElement ConvertHtmlNode(HtmlNode n) {
            var cel = new List<TextElement>();
            foreach (var c in n.ChildNodes) {
                cel.Add(ConvertHtmlNode(c));
            }
            return CreateTextElement(n, cel.ToArray());
        }

        private TextElement CreateTextElement(HtmlNode n, TextElement[] cl) {
            var te = GetTextElement(n);
            foreach (var c in cl) {
                te = AddChildToELement(te, c);
            }
            return FormatTextElement(n.Attributes,te);
        }

        private TextElement GetTextElement(HtmlNode n) {
            TextElement te = null;
            switch (n.Name) {
                case "#text":
                    te = new Run(n.InnerText);
                    break;
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
                    break;
            }
            return te;
        }

        private TextElement FormatTextElement(HtmlAttributeCollection ac, TextElement te) {
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

        private TextElement ApplyClassFormatting(TextElement te, string cv) {
            if (cv.StartsWith("ql-font-")) {
                te.FontFamily = GetFontFamily(cv);
            } else if (cv.Contains("ql-align-left")) {
                (te as Block).TextAlignment = TextAlignment.Left;
            } else if (cv.Contains("ql-align-center")) {
                (te as Block).TextAlignment = TextAlignment.Center;
            } else if (cv.Contains("ql-align-right")) {
                 (te as Block).TextAlignment = TextAlignment.Right;
            } else if (cv.Contains("ql-align-justify")) {
                (te as Block).TextAlignment = TextAlignment.Justify;
            } else if(cv.Contains("ql-indent-")) {
                (te as Paragraph).TextIndent = GetIndentLevel(cv) * _indentCharCount;
            }
            return te;
        }

        private TextElement ApplyStyleFormatting(TextElement te, string sv) {
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
        private TextElement AddChildToELement(TextElement te, TextElement cte) {
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

        public Brush ParseRgb(string text) {
            Brush defaultBrush = Brushes.Black;

            int rgbOpenIdx = text.IndexOf("(");
            if(rgbOpenIdx < 0) {
                return defaultBrush;
            }

            string rgbColors = text.Substring(rgbOpenIdx + 1).Replace(",", string.Empty).Replace(")", string.Empty).Replace(";",string.Empty);
            var rgbItemList = rgbColors.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            
            var color = new Color();
            color.A = 255;
            color.R = Convert.ToByte(rgbItemList[0]);
            color.G = Convert.ToByte(rgbItemList[1]);
            color.B = Convert.ToByte(rgbItemList[2]);

            return new SolidColorBrush(color);
        }

        public double GetFontSize(string styleValue) {
            //for some reason wpf will not accept px values and converts to 3/4 size (for 96DPI)
            //but giving pt will use the displays DIP
            string fontSizeStr = styleValue.Replace("font-size: ", string.Empty).Replace("px","pt");
            double fs = (double)new FontSizeConverter().ConvertFrom(fontSizeStr);
            return fs;
        }

        public double GetIndentLevel(string classValue) {
            return Convert.ToDouble(classValue.Replace("ql-indent-", string.Empty));
        }

        private FontFamily GetFontFamily(string classValue) {
            string defaultFontName = "arial";
            FontFamily defaultFontFamily = null;
            FontFamily closestFontFamily = null;
            string fontName = classValue.Replace("ql-font-", string.Empty);
            foreach (var ff in Fonts.SystemFontFamilies) {
                string ffName = ff.ToString().ToLower();
                if(ffName.Contains(fontName)) {
                    closestFontFamily = ff;
                }
                if(ffName == fontName) {
                    return ff;
                }
                if(ffName == defaultFontName) {
                    defaultFontFamily = ff;
                }
            }

            if (closestFontFamily != null) {
                Console.WriteLine("Could not find exact system font: " + fontName + " using "+closestFontFamily.ToString()+" instead");
                return closestFontFamily;
            }
            Console.WriteLine("Could not find system font: " + fontName);
            return defaultFontFamily;
        } 

        //private void ClearMatchLookUp() {
        //    //_subStringMatchCountLookUp.Clear();
        //    _subStringMatchLastProcessedLookUp.Clear();
        //}
        //private void UpdateMatchLookUp(string text,string matchStr) {
        //    //since paragraph may multiple sub-strings of this sub-nodes text we need 
        //    //to ensure we get the range representing htmlnode
        //    if (!_subStringMatchLastProcessedLookUp.ContainsKey(matchStr)) {
        //        int matchCount = text.IndexListOfAll(matchStr).Count;
        //        //_subStringMatchCountLookUp.Add(matchStr, matchCount);
        //        _subStringMatchLastProcessedLookUp.Add(matchStr, 0);
        //    } else {
        //        _subStringMatchLastProcessedLookUp[matchStr]++;
        //    }
        //}

        public void Test() {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpHtmlToRtfConverter)).Assembly;
            var stream = assembly.GetManifestResourceStream("MpWpfApp.Resources.TestData.quillFormattedTextSample1.html");
            using (var reader = new System.IO.StreamReader(stream)) {
                var html = reader.ReadToEnd();
                string rtf = ConvertHtmlToRtf(html);

                //Clipboard.SetData(DataFormats.Rtf, rtf);

                MpHelpers.Instance.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtftest.rtf", rtf, false);
            }
        }
    }
}
