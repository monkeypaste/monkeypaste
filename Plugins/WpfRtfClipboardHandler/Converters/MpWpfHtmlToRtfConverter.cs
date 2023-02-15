using HtmlAgilityPack;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfRtfClipboardHandler {
    public static class MpWpfHtmlToRtfConverter {
        #region private static Variables

        private static string[] _ignoredHtmlTagNames = new string[] {
            "colgroup",
            "svg"
        };

        public const string FALLBACK_DEFAULT_FONT_FAMILY = "Arial";
        public const string FALLBACK_DEFAULT_CODE_BLOCK_FONT_FAMILY = "Arial";
        public const double FALLBACK_DEFAULT_FONT_SIZE = 12.0d;
        public const int FALLBACK_DEFAULT_INDENT_CHAR_COUNT = 5;
        public static string CurrentDefaultFontFamily { get; private set; }
        public static string CurrentDefaultCodeBlockFontFamily { get; private set; }
        public static double CurrentDefaultFontSize { get; private set; }
        public static int CurrentIndentCharCount { get; private set; }

        public static FlowDocument CurrentFd { get; set; }
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public static string ConvertQuillHtmlToRtf(
            string html,
            string defaultFontFamily = FALLBACK_DEFAULT_FONT_FAMILY,
            string defaultCodeBlockFontFamily = FALLBACK_DEFAULT_CODE_BLOCK_FONT_FAMILY,
            double defaultFontSize = FALLBACK_DEFAULT_FONT_SIZE,
            int indentCharCount = FALLBACK_DEFAULT_INDENT_CHAR_COUNT) {
            CurrentDefaultFontFamily = defaultFontFamily;
            CurrentDefaultCodeBlockFontFamily = defaultCodeBlockFontFamily;
            CurrentDefaultFontSize = defaultFontSize;
            CurrentIndentCharCount = indentCharCount;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            CurrentFd = new FlowDocument {
                FontFamily = new FontFamily(CurrentDefaultFontFamily),

            };
            CurrentFd.Blocks.Clear();
            foreach (var htmlBlockNode in htmlDoc.DocumentNode.ChildNodes) {
                var docNode = htmlBlockNode;
                if (docNode.Name == "div") {
                    // this occurs for table parent and code-block 
                    // and for code-block, i'm not sure but there maybe multiple blocks so process all 1st level children
                    docNode = htmlBlockNode.FirstChild;
                    while (docNode != null) {
                        CurrentFd.Blocks.Add(ConvertHtmlNode(docNode) as Block);
                        docNode = docNode.NextSibling;
                    }
                    continue;
                }
                var te = ConvertHtmlNode(docNode);
                if (te == null) {
                    continue;
                }
                if (te is List l) {
                    var actual_lists = FinishListFormatting(htmlBlockNode, l);
                    foreach (var actual_list in actual_lists) {
                        CurrentFd.Blocks.Add(actual_list);
                    }
                    continue;
                }
                if (te is Block b) {
                    CurrentFd.Blocks.Add(b);
                    continue;
                } else if (te is Inline i) {
                    // NOTE this occurs when html is a sub-selection fragment
                }
                // _ownerFd.Blocks.Add(ConvertHtmlNode(docNode) as Block);
            }
            return CurrentFd.ToRichText();
        }

        public static void Test() {
            string html = "<p style='color: rgb(76, 85, 90); font-family: Lato, sans-serif; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>MvvmCross uses<span>&nbsp;</span><code class='language-plaintext highlighter-rouge'	style='font-family: Menlo, Monaco, Consolas, &quot;DejaVu Sans Mono&quot;, &quot;Lucida Console&quot;, monospace; font-size: 0.9375rem; background-color: rgb(242, 242, 242) !important; border: 1px solid rgb(230, 230, 230); border-radius: 4px; overflow-x: auto; padding: 1px 5px;'>ViewModel first navigation</code>. Meaning that we navigate from ViewModel to ViewModel and not from View to View. In MvvmCross the ViewModel will lookup its corresponding View. By doing so we don’t have to write platform specific navigation and we can manage everything from within our core.</p><h1 id='introducing-the-mvxnavigationservice'	style='font-size: 2em; margin: 0.67em 0px; font-weight: 400; color: rgb(76, 85, 90); font-family: Lato, sans-serif; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>Introducing the MvxNavigationService<a class='anchorjs-link '	href='https://www.mvvmcross.com/documentation/fundamentals/navigation#introducing-the-mvxnavigationservice'	aria-label='Anchor link for: introducing the mvxnavigationservice'	data-anchorjs-icon=''	style='background-color: transparent; opacity: 0; text-decoration: none; -webkit-font-smoothing: antialiased; color: rgb(47, 182, 129); font: 1em / 1 anchorjs-icons; padding-left: 0.375em;'/></h1><p style='color: rgb(76, 85, 90); font-family: Lato, sans-serif; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>The navigation enables you to inject it into your ViewModels, which makes it more testable, and gives you the ability to implement your own navigation! Other main features are that it is fully async and type safe. For more details see<span>&nbsp;</span><a href='https://github.com/MvvmCross/MvvmCross/issues/1634'	style='background-color: transparent; color: rgb(47, 182, 129); text-decoration: none;'>#1634</a></p><p style='color: rgb(76, 85, 90); font-family: Lato, sans-serif; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;'>The following Api is available to use. (See<span>&nbsp;</span><a href='https://github.com/MvvmCross/MvvmCross/blob/develop/MvvmCross/Navigation/IMvxNavigationService.cs'	style='background-color: transparent; color: rgb(47, 182, 129); text-decoration: none;'>IMvxNavigationService code for latest definition</a>):</p>";
            html = HttpUtility.HtmlDecode(html);
            string rtf = ConvertQuillHtmlToRtf(html);
            MessageBox.Show(rtf);
            //MpFileIo.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtftest.rtf", rtf, false);
            //var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpWpfHtmlToRtfConverter)).Assembly;
            //var stream = assembly.GetManifestResourceStream("MpWpfApp.Resources.TestData.quillFormattedTextSample5.html");
            //using (var reader = new System.IO.StreamReader(stream)) {
            //    var html = reader.ReadToEnd();
            //    string rtf = ConvertQuillHtmlToRtf(html);
            //    MpHelpers.WriteTextToFile(@"C:\Users\tkefauver\Desktop\rtftest.rtf", rtf, false);
            //}
        }
        #endregion

        #region Private Methods
        private static TextElement ConvertHtmlNode(HtmlNode n) {
            var cel = new List<TextElement>();
            foreach (var c in n.ChildNodes) {
                if (_ignoredHtmlTagNames.Contains(c.Name)) {
                    continue;
                }
                cel.Add(ConvertHtmlNode(c));
            }
            return CreateTextElement(n, cel.ToArray());
        }

        private static TextElement CreateTextElement(HtmlNode n, TextElement[] cl) {
            var te = GetTextElement(n);
            if (te == null) {
                Debugger.Break();
            }
            foreach (var c in cl) {
                if (c == null) {
                    continue;
                }
                te = AddChildToElement(te, c);
            }
            if (te is Table t) {
                // since wpf TableColumns aren't TextElements need to post-process
                // column width definitions
                te = FinishTableFormatting(n, t);
            }

            return FormatTextElement(n.Attributes, te);
        }

        private static TextElement GetTextElement(HtmlNode n) {
            TextElement te = null;
            switch (n.Name) {
                case "#text":
                    te = new Run(n.InnerText);
                    break;
                case "img":
                    var ic = new InlineUIContainer();
                    var img = new Image();
                    ic.Child = img;
                    return ic;
                case "em":
                    te = new Italic();
                    break;
                case "span":
                case "code":
                case "pre":
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
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    te = new Paragraph() {
                        FontSize = GetHeaderFontSize(n.Name)
                    };
                    break;
                case "li":
                    te = new ListItem();
                    break;
                case "ul":
                case "ol":
                    te = new List();
                    break;
                case "td":
                    te = new TableCell();
                    break;
                case "tr":
                    te = new TableRow();
                    break;
                case "tbody":
                    te = new TableRowGroup();
                    break;
                case "table":
                    te = new Table();
                    break;
                //case "colgroup":
                //    // colgroup and col nodes are ignored until the end of creating the table text element in 'FinishTableFormatting'
                //    n = n.NextSibling;
                //    te = new TableRowGroup();
                //    break;

                case "div":
                    // should only occur for code-block
                    if (n.HasClass("ql-code-block")) {
                        te = new Paragraph() {
                            Padding = new Thickness(3),
                            Background = Brushes.LightGray,
                            Foreground = Brushes.DimGray,
                            FontFamily = new FontFamily(CurrentDefaultCodeBlockFontFamily)
                        };
                    } else {
                        throw new Exception("Unhanlded html doc element: " + n.ToString());
                    }
                    break;
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
                    case "rowspan":
                        te = ApplyRowSpanFormatting(te, a.Value);
                        break;
                    case "colspan":
                        te = ApplyColSpanFormatting(te, a.Value);
                        break;
                }
            }
            return te;
        }

        private static Table FinishTableFormatting(HtmlNode tn, Table t) {
            bool has_unknown_col_widths = false;
            if (tn.FirstChild.Name == "colgroup") {
                var colDefList = tn.FirstChild.ChildNodes;
                for (int i = 0; i < colDefList.Count; i++) {
                    int columnsToAdd = -(i - t.Columns.Count - 1);
                    if (columnsToAdd > 0) {
                        while (columnsToAdd > 0) {
                            t.Columns.Add(new TableColumn());
                            columnsToAdd--;
                        }
                    }

                    double colWidth = -1.0d;
                    var colDef = colDefList[i];
                    string colWidthAttrVal = colDef.GetAttributeValue("width", string.Empty);
                    if (colWidthAttrVal != string.Empty) {
                        try {
                            colWidth = double.Parse(colWidthAttrVal);
                        }
                        catch (Exception ex) {
                            Console.WriteLine(ex);
                        }
                    }
                    if (!double.IsNaN(colWidth) && colWidth >= 0) {
                        t.Columns[i].Width = new GridLength(colWidth);
                    } else {
                        t.Columns[i].Width = new GridLength(0, GridUnitType.Auto);
                        has_unknown_col_widths = true;
                    }
                }
            }
            if (has_unknown_col_widths) {
                // try this if col's still messed up
                t = MpCsvToRtfTableConverter.CalculateColumnWidths(t);
                //t.Columns.ForEach(x => x.Width = new GridLength(0, GridUnitType.Auto));
            }
            return t;
        }

        private static TextElement ApplyRowSpanFormatting(TextElement te, string hv) {
            int rowSpanVal = 1;
            try {
                rowSpanVal = Convert.ToInt32(hv);
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            (te as TableCell).RowSpan = rowSpanVal;
            return te;
        }

        private static TextElement ApplyColSpanFormatting(TextElement te, string hv) {
            int colSpanVal = 1;
            try {
                colSpanVal = Convert.ToInt32(hv);
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }

            (te as TableCell).ColumnSpan = colSpanVal;
            return te;
        }

        private static List<List> FinishListFormatting(HtmlNode n, List l) {
            // quill groups any bullets types into single list 
            // this creates unique lists for each common bullet type and returns the set
            List<List> bc = new List<List>();
            bc.Add(l);

            var last_list_type = GetListMarkerStyle(n.FirstChild);

            for (int i = 0; i < n.ChildNodes.Count; i++) {
                if (i == 0) {
                    l.MarkerStyle = last_list_type;
                    continue;
                }
                var cn = n.ChildNodes[i];
                var cur_list_type = GetListMarkerStyle(cn);

                if (cur_list_type != last_list_type) {
                    var nl = new List() {
                        MarkerStyle = cur_list_type
                    };
                    for (int j = i; j < n.ChildNodes.Count; j++) {
                        ListItem li = l.ListItems.ElementAt(j);
                        l.ListItems.Remove(li);
                        nl.ListItems.Add(li);
                    }
                    bc.Add(nl);

                    last_list_type = cur_list_type;
                }
            }

            return bc;
        }

        public static TextMarkerStyle GetListMarkerStyle(HtmlNode li_node) {
            if (li_node.ParentNode.Name == "ol") {
                return TextMarkerStyle.Decimal;
            }
            if (li_node.ParentNode.Name != "ul") {
                // what is it?
                Debugger.Break();
                return TextMarkerStyle.None;
            }
            string dataListValue = li_node.GetAttributeValue("data-checked", string.Empty);
            if (string.IsNullOrEmpty(dataListValue)) {
                return TextMarkerStyle.Disc;
            }
            if (dataListValue == "false") {
                return TextMarkerStyle.Square;
            }
            if (dataListValue == "true") {
                return TextMarkerStyle.Box;
            }

            // what the hecks going on!?
            Debugger.Break();
            return TextMarkerStyle.None;
        }

        private static TextElement ApplyHrefFormatting(TextElement te, string hv) {
            (te as Hyperlink).NavigateUri = new Uri(hv);
            return te;
        }

        private static TextElement ApplyImgSrcFormatting(TextElement te, string sv) {
            BitmapSource bmpSrc = null;
            if (Uri.IsWellFormedUriString(sv, UriKind.Absolute)) {
                if (sv.StartsWith("data:image/png;base64,")) {
                    bmpSrc = sv.Replace("data:image/png;base64,", string.Empty).ToBitmapSource();
                } else {
                    bmpSrc = (BitmapSource)new BitmapImage(new Uri(sv));
                }

            } else if (sv.Contains(",")) {
                var srcvl = sv.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (srcvl.Length > 1) {
                    string imgStr = srcvl[1];//.Replace(@"\", string.Empty);
                    int mod4 = imgStr.Length % 4;
                    if (mod4 > 0) {
                        imgStr += new string('=', 4 - mod4);
                    }
                    bmpSrc = imgStr.ToBitmapSource();
                }
            }

            if (bmpSrc != null) {
                var img = te.FindChildren<Image>().FirstOrDefault();
                //byte[] data = Convert.FromBase64String(imgStr);
                //string decodedString = Encoding.UTF8.GetString(data);
                img.Source = bmpSrc;
                img.Height = 100;
                img.Width = 100;
            }
            return te;
        }

        private static TextElement ApplyClassFormatting(TextElement te, string cv) {
            if (cv.StartsWith("ql-font-")) {
                te.FontFamily = GetFontFamily(cv);
            } else {
                Block b = te as Block;
                if (b == null && te.Parent is Block) {
                    b = te.Parent as Block;
                }
                if (b != null) {
                    if (cv.Contains("ql-align-left")) {
                        b.TextAlignment = TextAlignment.Left;
                    } else if (cv.Contains("ql-align-center")) {
                        b.TextAlignment = TextAlignment.Center;
                    } else if (cv.Contains("ql-align-right")) {
                        b.TextAlignment = TextAlignment.Right;
                    } else if (cv.Contains("ql-align-justify")) {
                        b.TextAlignment = TextAlignment.Justify;
                    } else if (cv.Contains("ql-indent-")) {
                        if (b is Paragraph p) {
                            p.TextIndent = GetIndentLevel(cv) * CurrentIndentCharCount;
                        }
                    }
                }
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
            } else if (sv.StartsWith("font-size")) {
                te.FontSize = GetFontSize(sv, te);
            }
            return te;
        }
        private static TextElement AddChildToElement(TextElement te, TextElement cte) {
            if (te is Table t) {
                t.RowGroups.Add(cte as TableRowGroup);
            } else if (te is TableRowGroup trg) {
                trg.Rows.Add(cte as TableRow);
            } else if (te is TableRow tr) {
                tr.Cells.Add(cte as TableCell);
            } else if (te is TableCell tc) {
                tc.Blocks.Add(cte as Block);
            } else if (te is List) {
                (te as List).ListItems.Add(cte as ListItem);
            } else if (te is ListItem) {
                if ((te as ListItem).Blocks.Count == 0) {
                    //special case since wpf requires list items to be in a paragraph
                    //we must add them implicitly
                    (te as ListItem).Blocks.Add(new Paragraph());
                }
                ((te as ListItem).Blocks.FirstBlock as Paragraph).Inlines.Add(cte as Inline);
            } else if (te is Paragraph) {
                (te as Paragraph).Inlines.Add(cte as Inline);
            } else if (te is Span) {
                (te as Span).Inlines.Add(cte as Inline);
            }
            return te;
        }

        public static Brush ParseRgb(string text) {
            Brush defaultBrush = Brushes.Transparent;

            var color = new Color();
            if (text.Contains("var")) {
                // occured where color: var(--color-accent-fg)'
                //no idea how to handle this so just return black
                return Brushes.Black;
            }

            int rgbOpenIdx = text.IndexOf("(");
            if (rgbOpenIdx < 0) {
                string hex = null;
                int preNameIdx = text.IndexOf(":");
                if (preNameIdx >= 0) {
                    if (text.Contains("#")) {
                        hex = "#" + text.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    } else if (text.Contains(" ")) {
                        string colorName = text.Substring(preNameIdx + 1)
                                        .Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                                        .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("!"));
                        if (string.IsNullOrWhiteSpace(colorName)) {
                            return defaultBrush;
                        }
                        hex = MpSystemColors.ConvertFromString(colorName.Trim(), defaultBrush.ToHex());
                        if (string.IsNullOrWhiteSpace(hex) || !hex.IsStringHexColor()) {
                            return defaultBrush;
                        }
                    }
                }
                if (hex != null) {
                    return hex.ToWpfBrush();
                }
            }

            string commaReplacement = string.Empty;
            if (!text.Substring(rgbOpenIdx + 1).Contains(" ")) {
                commaReplacement = " ";
            }
            string rgbColors = text.Substring(rgbOpenIdx + 1).Replace(",", commaReplacement).Replace(")", string.Empty).Replace(";", string.Empty);
            var rgbItemList = rgbColors.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            color.A = 255;
            color.R = Convert.ToByte(rgbItemList[0]);
            color.G = Convert.ToByte(rgbItemList[1]);
            color.B = Convert.ToByte(rgbItemList[2]);

            MpWpfRtfDefaultProperties.Instance.AddFontColor(color);

            return new SolidColorBrush(color);
        }

        public static double GetHeaderFontSize(string headerTag) {
            switch (headerTag) {
                case "h1":
                    return 32;
                case "h2":
                    return 24;
                case "h3":
                    return 18.72;
                case "h4":
                    return 16;
                case "h5":
                    return 13.28;
                case "h6":
                    return 10.72;
            }
            throw new Exception("Unknown header tag: " + headerTag);
        }
        public static double GetFontSize(string styleValue, TextElement te) {
            //for some reason wpf will not accept px values and converts to 3/4 size (for 96DPI)
            //but giving pt will use the displays DIP
            //string fontSizeStr = styleValue.Replace("font-size: ", string.Empty).Replace("px","pt");
            //double fs = (double)new FontSizeConverter().ConvertFrom(fontSizeStr);

            // NOTE non px types may need adjustment when DPI is not 96
            string fontSizeStr = string.Empty;
            double fs = CurrentDefaultFontSize;
            if (styleValue.Contains("px")) {
                fontSizeStr = styleValue.Replace("font-size: ", string.Empty).Replace("px", string.Empty);
            } else if (styleValue.Contains("rem")) {
                fontSizeStr = styleValue.Replace("font-size: ", string.Empty).Replace("rem", string.Empty);
            } else if (styleValue.Contains("em")) {
                fontSizeStr = styleValue.Replace("font-size: ", string.Empty).Replace("em", string.Empty);
            } else if (styleValue.Contains("pt")) {
                fontSizeStr = styleValue.Replace("font-size: ", string.Empty).Replace("pt", string.Empty);
            }
            try {
                fs = (double)Convert.ToDouble(fontSizeStr);
                if (styleValue.Contains("rem") || styleValue.Contains("em")) {
                    fs = te.FontSize * fs;
                } else if (styleValue.Contains("pt")) {
                    fs = fs * 1.333;
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
            MpWpfRtfDefaultProperties.Instance.AddFontSize(fs);
            return (double)((int)fs);
        }

        public static double GetIndentLevel(string classValue) {
            return Convert.ToDouble(classValue.Replace("ql-indent-", string.Empty));
        }

        private static FontFamily GetFontFamily(string classValue) {
            string defaultFontName = CurrentDefaultFontFamily.ToLower();
            FontFamily defaultFontFamily = null;
            FontFamily closestFontFamily = null;
            string fontName = classValue.Replace("ql-font-", string.Empty).Replace("-", " ");
            foreach (var ff in Fonts.SystemFontFamilies) {
                string ffName = ff.ToString().ToLower();
                if (ffName.Contains(fontName)) {
                    closestFontFamily = ff;
                }
                if (ffName == fontName) {
                    closestFontFamily = ff;
                    break;
                }
                if (ffName == defaultFontName) {
                    defaultFontFamily = ff;
                }
            }

            if (closestFontFamily != null) {
                //Console.WriteLine("Could not find exact system font: " + fontName + " using "+closestFontFamily.ToString()+" instead");
                MpWpfRtfDefaultProperties.Instance.AddFont(closestFontFamily.ToString().ToLower());
                return closestFontFamily;
            }
            Console.WriteLine("Could not find system font: " + fontName);
            return defaultFontFamily;
        }

        #endregion

    }
}
