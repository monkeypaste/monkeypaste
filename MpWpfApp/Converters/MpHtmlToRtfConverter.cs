using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using HtmlAgilityPack;
using static System.Net.Mime.MediaTypeNames;

namespace MpWpfApp {
    public class MpHtmlToRtfConverter {
        #region Singleton
        private static readonly Lazy<MpHtmlToRtfConverter> _Lazy = new Lazy<MpHtmlToRtfConverter>(() => new MpHtmlToRtfConverter());
        public static MpHtmlToRtfConverter Instance { get { return _Lazy.Value; } }

        private MpHtmlToRtfConverter() {
        }
        #endregion

        #region Private Variables
        private RichTextBox _rtb;
        //private Dictionary<string, int> _subStringMatchCountLookUp = new Dictionary<string, int>();

        private Dictionary<string, int> _subStringMatchLastProcessedLookUp = new Dictionary<string, int>();
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
                if (htmlBlockNode.Name == "p") {
                    var p = CreateParagraph(htmlBlockNode);
                    _rtb.Document.Blocks.Add(p);
                } else if (htmlBlockNode.Name == "ol") {
                    var list = CreateList(htmlBlockNode);
                    _rtb.Document.Blocks.Add(list);
                } else if (htmlBlockNode.Name == "table") {

                } else if (htmlBlockNode.Name.StartsWith("h")) {

                }

                _subStringMatchLastProcessedLookUp.Clear();
            }

            return _rtb.Document.ToRichText();
        }

        private Paragraph CreateParagraph(HtmlNode node) {
            var p = new Paragraph();
            p.Inlines.Add(new Run(node.InlineInnerText()));
            ApplyAttributeFormatting(node, p);

            foreach (var subBlockNode in node.ChildNodes) {
                CreateSubParagraphElement(p, subBlockNode);
            }
            return p;
        }
        private List CreateList(HtmlNode node) {
            var list = new List();
            if (node.FirstChild.GetAttributeValue("data-list",string.Empty) == "ordered") {
                list.MarkerStyle = TextMarkerStyle.Decimal;
            }
            ApplyAttributeFormatting(node, list);
            
            foreach (var subBlockNode in node.ChildNodes) {
                CreateListItem(list, subBlockNode);
            }
            return list;
        }
        private ListItem CreateListItem(List l,HtmlNode subNode) {
            var nodeListItem = new ListItem(CreateParagraph(subNode));//new Paragraph(new Run(nodeText, nodeRange.Start)));
            return nodeListItem;
        }

        private TextElement CreateSubParagraphElement(TextElement p, HtmlNode subBlockNode) {
            //UpdateMatchLookUp(new TextRange(b.ContentStart,b.ContentEnd).Text, subBlockNode.InlineInnerText());

            //since paragraph may multiple sub-strings of this sub-nodes text we need 
            //to ensure we get the range representing htmlnode
            string parentText = new TextRange(p.ContentStart, p.ContentEnd).Text;
            string text = subBlockNode.InlineInnerText();
            TextRange nodeRange = null;
            if(string.IsNullOrEmpty(text)) {
                nodeRange = new TextRange(p.ContentStart, p.ContentStart);
            } else {
                if (!_subStringMatchLastProcessedLookUp.ContainsKey(parentText)) {
                    int matchCount = parentText.IndexListOfAll(text).Count;
                    //_subStringMatchCountLookUp.Add(matchStr, matchCount);
                    _subStringMatchLastProcessedLookUp.Add(text, 0);
                } else {
                    _subStringMatchLastProcessedLookUp[text]++;
                }

                var allNodeMatchRanges = MpHelpers.Instance.FindStringRangesFromPosition(p.ContentStart, text);
                int matchIdx = _subStringMatchLastProcessedLookUp[text];
                if (matchIdx >= allNodeMatchRanges.Count) {
                    matchIdx = allNodeMatchRanges.Count - 1;
                }
                nodeRange = allNodeMatchRanges[matchIdx];
            }
            
            string nodeText = nodeRange.Text;
            nodeRange.Text = "";

            var nodeRun = new Run(nodeText, nodeRange.Start);
            var span = new Span(nodeRun, nodeRange.Start);
            return (Span)FormatSubBlockElement(subBlockNode, span);
        }

        private TextElement FormatSubBlockElement(HtmlNode htmlCurNode, TextElement element) {
            switch (htmlCurNode.Name) {
                case "#text":
                    // no formatting so ignore
                    break;
                case "span":
                    // only formats from attributes
                    break;
                case "strong":
                    element.FontWeight = FontWeights.Bold;
                    //_rtb.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    break;
                case "em":
                    element.FontStyle = FontStyles.Italic;
                    //_rtb.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                    break;
                case "u":
                    new TextRange(element.ContentStart, element.ContentEnd).ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                    break;
                case "li":
                    //only formats from attributes
                    break;
            }

            element = ApplyAttributeFormatting(htmlCurNode, element);
            foreach(var htmlSubNode in htmlCurNode.ChildNodes) {
                CreateSubParagraphElement(element, htmlSubNode);
            }
            return element;
        }

        public TextElement ApplyAttributeFormatting(HtmlNode htmlNode, TextElement element) {
            foreach (var nodeAttribute in htmlNode.GetAttributes()) {
                string attributeValue = nodeAttribute.Value;
                switch (nodeAttribute.Name) {
                    case "class":
                        if (attributeValue.StartsWith("ql-font-")) {
                            string fontName = attributeValue.Replace("ql-font-", string.Empty);
                            var ff = GetFontFamily(fontName);
                            element.FontFamily = ff;
                            //_rtb.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, ff);
                        } else if (attributeValue.Contains("ql-align-left")) {
                            //_rtb.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Left);
                            (element as Block).TextAlignment = TextAlignment.Left;
                        } else if (attributeValue.Contains("ql-align-center")) {
                            //_rtb.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Center);
                            (element as Block).TextAlignment = TextAlignment.Center;
                        } else if (attributeValue.Contains("ql-align-right")) {
                            //_rtb.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Right);
                            (element as Block).TextAlignment = TextAlignment.Right;
                        } else if (attributeValue.Contains("ql-align-justify")) {
                            //_rtb.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Justify);
                            (element as Block).TextAlignment = TextAlignment.Justify;
                        }
                        break;
                    case "style":
                        var styleItemList = attributeValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var rawStyleItem in styleItemList) {
                            string styleItem = rawStyleItem.Trim();
                            if (attributeValue.StartsWith("color")) {
                                var itemColorBrush = ParseRgb(attributeValue);
                                element.Foreground = itemColorBrush;
                                //_rtb.Selection.ApplyPropertyValue(FlowDocument.ForegroundProperty, itemColorBrush);
                            } else if (attributeValue.StartsWith("background-color")) {
                                var itemColorBrush = ParseRgb(attributeValue);
                                element.Background = itemColorBrush;
                               // _rtb.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, itemColorBrush);
                            }
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
            return element;
        }

        public bool IsLineBreak(string text) {            
            return text == "\r" || text == "\n" || text == "\r\n";
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
            color.R = Convert.ToByte(rgbItemList[0]);
            color.G = Convert.ToByte(rgbItemList[1]);
            color.B = Convert.ToByte(rgbItemList[2]);

            return new SolidColorBrush(color);
        }

        private FontFamily GetFontFamily(string fontName) {
            string defaultFontName = "arial";
            FontFamily defaultFontFamily = null;
            FontFamily closestFontFamily = null;
            foreach(var ff in Fonts.SystemFontFamilies) {
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
