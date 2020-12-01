using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MpWpfApp {
    public static class MpExtensions {
        //public static string GetText(this Hyperlink hyperlink) {
        //    var run = hyperlink.Inlines.FirstInline as Run;
        //    return run == null ? string.Empty : run.Text;
        //}
        public static void SetHyperlinkText(this Hyperlink hyperlink, string text) {
            var run = hyperlink.Inlines.FirstInline as Run;
            if (run == null) {
                run = new Run();
                hyperlink.Inlines.Add(run);
            }
            run.Text = text;
        }
        public static List<Hyperlink> GetAllHyperlinkList(this RichTextBox rtb) {
            if (rtb.Tag == null) {
                return new List<Hyperlink>();
            }
            return (List<Hyperlink>)rtb.Tag;

            var hyperlinkList = (List<Hyperlink>)rtb.Tag;
            foreach (var paragraph in rtb.Document.Blocks.OfType<Paragraph>()) {
                foreach (var hyperlink in paragraph.Inlines.OfType<Hyperlink>()) {
                    hyperlinkList.Add(hyperlink);
                }
            }
            return hyperlinkList;
        }

        public static List<Hyperlink> GetTemplateHyperlinkList(this RichTextBox rtb) {
            return rtb.GetAllHyperlinkList().Where(x => x.NavigateUri.OriginalString == Properties.Settings.Default.TemplateTokenUri).ToList();
            //var hyperlinkList = (List<Hyperlink>)rtb.Tag;//new List<Hyperlink>();
            ////string test = MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);
            //foreach (var paragraph in rtb.Document.Blocks.OfType<Paragraph>()) {
            //    foreach (var hyperlink in paragraph.Inlines.OfType<Hyperlink>()) {
            //        if (hyperlink.NavigateUri.OriginalString == Properties.Settings.Default.TemplateTokenUri) {
            //            hyperlinkList.Add(hyperlink);
            //        }
            //    }
            //}
            //return hyperlinkList;
        }

        public static void ClearHyperlinks(this RichTextBox rtb) {
            //replaces hyperlinks with runs of there textrange text
            //if hl is templatee it decodes the run into #templatename#templatecolor# 
            var hll = rtb.GetAllHyperlinkList();
            foreach (var hl in hll) {
                rtb.Selection.Select(hl.ContentStart, hl.ContentStart);
                rtb.Selection.Text = rtb.Selection.Text;
                if (hl.NavigateUri.OriginalString == Properties.Settings.Default.TemplateTokenUri) {
                    rtb.Selection.Text = string.Format(@"{0}{1}{0}", Properties.Settings.Default.TemplateTokenMarker, rtb.Selection.Text);
                }
            }
            rtb.Tag = null;
        }
        public static void CreateHyperlinks(this RichTextBox rtb) {        
            var regExGroupList = new List<string> {
                //WebLink
                @"(?:https?://|www\.)\S+", 
                //Email
                @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})",
                //PhoneNumber
                @"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})",
                //Currency
                @"[$|£|€|¥]([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.[0-9][0-9])?",
                //HexColor
                @"#([0-9]|[a-fA-F]){6}",
                //StreetAddress
                @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b",                
                //Text Template
                string.Format(
                    @"[{0}].*?[{0}]", 
                    Properties.Settings.Default.TemplateTokenMarker)
            };
            List<Hyperlink> linkList = new List<Hyperlink>();
            //rtb.ClearHyperlinks();
            TextRange fullDocRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            for (int i = 0; i < regExGroupList.Count; i++) {
                var regExStr = regExGroupList[i];
                MatchCollection mc = Regex.Matches(fullDocRange.Text, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                foreach (Match m in mc) {
                    TextPointer lastRangeEnd = rtb.Document.ContentStart;
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            var matchRange = MpHelpers.FindStringRangeFromPosition(lastRangeEnd, c.Value);
                            lastRangeEnd = matchRange.End;
                            if (matchRange == null) {
                                continue;
                            }
                            Hyperlink hl = null;
                            if ((MpSubTextTokenType)(i + 1) == MpSubTextTokenType.TemplateSegment) {
                                rtb.Selection.Select(matchRange.Start, matchRange.End);
                                rtb.Selection.Text = rtb.Selection.Text.Replace(Properties.Settings.Default.TemplateTokenMarker, string.Empty);
                                hl = new Hyperlink(rtb.Selection.Start, rtb.Selection.End);
                                hl.TargetName = rtb.Selection.Text;
                                //hl.Inlines.Clear();
                                //hl.Inlines.Add(new Run(c.Value.Replace(Properties.Settings.Default.TemplateTokenMarkerStart, string.Empty).Replace(Properties.Settings.Default.TemplateTokenMarkerEnd, string.Empty)));
                                hl.Tag = MpSubTextTokenType.TemplateSegment;
                                hl.IsEnabled = true;
                                //hl.TargetName = ((Run)hl.Inlines.FirstInline).Text;
                                hl.NavigateUri = new Uri(Properties.Settings.Default.TemplateTokenUri);
                                hl.RequestNavigate += (s4, e4) => {
                                    MessageBox.Show("Sup");
                                };
                                Console.WriteLine("Creating template link w/ taget name: " + hl.TargetName);
                            } else {
                                var linkText = c.Value;
                                hl = new Hyperlink(matchRange.Start, matchRange.End);
                                hl.Tag = (MpSubTextTokenType)(i + 1);
                                hl.IsEnabled = true;
                                hl.RequestNavigate += (s4, e4) => {
                                    System.Diagnostics.Process.Start(e4.Uri.ToString());
                                };

                                MenuItem convertToQrCodeMenuItem = new MenuItem();
                                convertToQrCodeMenuItem.Header = "Convert to QR Code";
                                convertToQrCodeMenuItem.Click += (s5, e1) => {
                                    var hyperLink = (Hyperlink)((MenuItem)s5).Tag;
                                    Clipboard.SetImage(MpHelpers.ConvertUrlToQrCode(hyperLink.NavigateUri.ToString()));
                                };
                                convertToQrCodeMenuItem.Tag = hl;
                                hl.ContextMenu = new ContextMenu();
                                hl.ContextMenu.Items.Add(convertToQrCodeMenuItem);

                                switch ((MpSubTextTokenType)hl.Tag) {
                                    case MpSubTextTokenType.StreetAddress:
                                        hl.NavigateUri = new Uri("https://google.com/maps/place/" + linkText.Replace(' ', '+'));
                                        break;
                                    case MpSubTextTokenType.Uri:
                                        if (!linkText.Contains("https://")) {
                                            hl.NavigateUri = new Uri("https://" + linkText);
                                        } else {
                                            hl.NavigateUri = new Uri(linkText);
                                        }
                                        MenuItem minifyUrl = new MenuItem();
                                        minifyUrl.Header = "Minify with bit.ly";
                                        minifyUrl.Click += (s1, e2) => {
                                            Hyperlink link = (Hyperlink)((MenuItem)s1).Tag;
                                            string minifiedLink = MpHelpers.ShortenUrl(link.NavigateUri.ToString()).Result;
                                            Clipboard.SetText(minifiedLink);
                                        };
                                        minifyUrl.Tag = hl;
                                        hl.ContextMenu.Items.Add(minifyUrl);
                                        break;
                                    case MpSubTextTokenType.Email:
                                        hl.NavigateUri = new Uri("mailto:" + linkText);
                                        break;
                                    case MpSubTextTokenType.PhoneNumber:
                                        hl.NavigateUri = new Uri("tel:" + linkText);
                                        break;
                                    case MpSubTextTokenType.Currency:
                                        //"https://www.google.com/search?q=%24500.80+to+yen"
                                        MenuItem convertCurrencyMenuItem = new MenuItem();
                                        convertCurrencyMenuItem.Header = "Convert Currency To";
                                        foreach (MpCurrencyType ct in Enum.GetValues(typeof(MpCurrencyType))) {
                                            if (ct == MpCurrencyType.None || ct == MpHelpers.GetCurrencyTypeFromString(linkText)) {
                                                continue;
                                            }
                                            MenuItem subItem = new MenuItem();
                                            subItem.Header = Enum.GetName(typeof(MpCurrencyType), ct);
                                            subItem.Click += (s2, e2) => {
                                                // use https://free.currencyconverterapi.com/ instead of google
                                                //string convertedCurrency = MpHelpers.CurrencyConvert(
                                                //    (decimal)MpHelpers.GetCurrencyValueFromString(linkText),
                                                //    Enum.GetName(typeof(MpCurrencyType), MpHelpers.GetCurrencyTypeFromString(linkText)),
                                                //    Enum.GetName(typeof(MpCurrencyType), ct));
                                                //hyperlink.Inlines.Clear();
                                                //hyperlink.Inlines.Add(new Run(convertedCurrency));
                                                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).HideWindowCommand.Execute(null);
                                                System.Diagnostics.Process.Start(@"https://www.google.com/search?q=" + linkText + "+to+" + subItem.Header);
                                            };
                                            convertCurrencyMenuItem.Items.Add(subItem);
                                        }

                                        hl.ContextMenu.Items.Add(convertCurrencyMenuItem);
                                        break;
                                    default:

                                        break;
                                }
                            }
                            linkList.Add(hl);
                        }
                    }
                }
            }
            rtb.Tag = linkList;
        }
        public static FlowDocument Clone(this FlowDocument doc) {
            using (MemoryStream stream = new MemoryStream()) {
                var clonedDoc = new FlowDocument();
                TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
                System.Windows.Markup.XamlWriter.Save(range, stream);
                range.Save(stream, DataFormats.XamlPackage);
                TextRange range2 = new TextRange(clonedDoc.ContentEnd, clonedDoc.ContentEnd);
                range2.Load(stream, DataFormats.XamlPackage);
                return clonedDoc;
            }                
        }
        public static TextRange FindStringRangeFromPosition2(this RichTextBox rtb, string findText, bool isCaseSensitive = false) {
            var fullText = MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);
            if (string.IsNullOrEmpty(findText) || string.IsNullOrEmpty(fullText) || findText.Length > fullText.Length)
                return null;

            var textbox = rtb;
            var leftPos = textbox.CaretPosition;
            var rightPos = textbox.CaretPosition;

            while (true) {
                var previous = leftPos.GetNextInsertionPosition(LogicalDirection.Backward);
                var next = rightPos.GetNextInsertionPosition(LogicalDirection.Forward);
                if (previous == null && next == null)
                    return null; //can no longer move outward in either direction and text wasn't found

                if (previous != null)
                    leftPos = previous;
                if (next != null)
                    rightPos = next;

                var range = new TextRange(leftPos, rightPos);
                var offset = range.Text.IndexOf(findText, StringComparison.InvariantCultureIgnoreCase);
                if (offset < 0)
                    continue; //text not found, continue to move outward

                //rtf has broken text indexes that often come up too low due to not considering hidden chars.  Increment up until we find the real position
                var findTextLower = findText.ToLower();
                var endOfDoc = textbox.Document.ContentEnd.GetNextInsertionPosition(LogicalDirection.Backward);
                for (var start = range.Start.GetPositionAtOffset(offset); start != endOfDoc; start = start.GetPositionAtOffset(1)) {
                    var result = new TextRange(start, start.GetPositionAtOffset(findText.Length));
                    if (result.Text?.ToLower() == findTextLower) {
                        return result;
                    }
                }
            }
        }

        public static StringCollection ToStringCollection(this IEnumerable<string> strings) {
            var stringCollection = new StringCollection();
            foreach (string s in strings) {
                stringCollection.Add(s);
            }
            return stringCollection;
        }

        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector, bool desc = false) {
            if (source == null) {
                return;
            }

            Comparer<TKey> comparer = Comparer<TKey>.Default;

            for (int i = source.Count - 1; i >= 0; i--) {
                for (int j = 1; j <= i; j++) {
                    TSource o1 = source[j - 1];
                    TSource o2 = source[j];
                    int comparison = comparer.Compare(keySelector(o1), keySelector(o2));
                    //(source as IEditableCollectionView).EditItem(o1);
                    //(source as IEditableCollectionView).EditItem(o2);
                    if (desc && comparison < 0) {
                        //var temp = source[j];
                        //source.RemoveAt(j);
                        //source.Insert(j - 1, temp);
                        source.Move(j, j - 1);
                    } else if (!desc && comparison > 0) {
                        //var temp = source[j-1];
                        //source.RemoveAt(j-1);
                        //source.Insert(j, temp);
                        source.Move(j - 1, j);
                    }

                    //(source as IEditableCollectionView).CommitEdit();
                }
            }
        }

        public static List<int> AllIndexesOf(this string str, string value) {
            if (string.IsNullOrEmpty(value)) {
                return new List<int>();
            }
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length) {
                index = str.IndexOf(value, index);
                if (index == -1) {
                    return indexes;
                }
                indexes.Add(index);
            }
        }

        //faster version but needs unsafe thing
        //public unsafe static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset) {
        //    fixed (PixelColor* buffer = &pixels[0, 0])
        //        source.CopyPixels(
        //          new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight),
        //          (IntPtr)(buffer + offset),
        //          pixels.GetLength(0) * pixels.GetLength(1) * sizeof(PixelColor),
        //          stride);
        //}
        public static void CopyPixels(this BitmapSource source, PixelColor[,] pixels, int stride, int offset, bool dummy) {
            var height = source.PixelHeight;
            var width = source.PixelWidth;
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            int y0 = offset / width;
            int x0 = offset - width * y0;
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    pixels[x + x0, y + y0] = new PixelColor {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
                }
            }
        }

        public static bool IsNamedObject(this object obj) {
            return obj.GetType().FullName == "MS.Internal.NamedObject";
        }

        public static T GetDescendantOfType<T>(this DependencyObject depObj) where T : DependencyObject {
            if (depObj == null) {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetDescendantOfType<T>(child);
                if (result != null) {
                    return result;
                }
            }
            return null;
        }

        private static T GetDescendantOfType<T>(this DependencyObject depObj, List<T> curList) where T : DependencyObject {
            if (depObj == null) {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetDescendantOfType<T>(child, curList);
                if (result != null && curList.Contains(result)) {
                    return result;
                }
            }
            return null;
        }

        public static List<T> GetDescendantListOfType<T>(this DependencyObject depObj) where T : DependencyObject {
            var descendentList = new List<T>();
            T newDescendant = null;
            do {
                newDescendant = depObj.GetDescendantOfType<T>(descendentList);
                if (newDescendant != null) {
                    descendentList.Add(newDescendant);
                }
            } while (newDescendant != null);
            return descendentList;
        }

        public static void SetRtf(this System.Windows.Controls.RichTextBox rtb, string document) {
            var documentBytes = Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                rtb.SelectAll();
                rtb.Selection.Load(reader, System.Windows.DataFormats.Rtf);
            }
        }

        public static void SetXaml(this System.Windows.Controls.RichTextBox rtb, string document) {
            var documentBytes = Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                rtb.SelectAll();
                rtb.Selection.Load(reader, System.Windows.DataFormats.Xaml);
            }
        }

        public static IEnumerable<TextElement> GetRunsAndParagraphs(FlowDocument doc) {
            for (TextPointer position = doc.ContentStart;
              position != null && position.CompareTo(doc.ContentEnd) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    Run run = position.Parent as Run;

                    if (run != null) {
                        yield return run;
                    } else {
                        Paragraph para = position.Parent as Paragraph;

                        if (para != null) {
                            yield return para;
                        }
                    }
                }
            }
        }

        public static FormattedText GetFormattedText(this FlowDocument doc) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            FormattedText output = new FormattedText(
              GetText(doc),
              CultureInfo.CurrentCulture,
              doc.FlowDirection,
              new Typeface(doc.FontFamily, doc.FontStyle, doc.FontWeight, doc.FontStretch),
              doc.FontSize,
              doc.Foreground);

            int offset = 0;

            foreach (TextElement el in GetRunsAndParagraphs(doc)) {
                Run run = el as Run;

                if (run != null) {
                    int count = run.Text.Length;

                    output.SetFontFamily(run.FontFamily, offset, count);
                    output.SetFontStyle(run.FontStyle, offset, count);
                    output.SetFontWeight(run.FontWeight, offset, count);
                    output.SetFontSize(run.FontSize, offset, count);
                    output.SetForegroundBrush(run.Foreground, offset, count);
                    output.SetFontStretch(run.FontStretch, offset, count);
                    output.SetTextDecorations(run.TextDecorations, offset, count);

                    offset += count;
                } else {
                    offset += Environment.NewLine.Length;
                }
            }

            return output;
        }

        public static string GetText(FlowDocument doc) {
            StringBuilder sb = new StringBuilder();

            foreach (TextElement el in GetRunsAndParagraphs(doc)) {
                Run run = el as Run;
                sb.Append(run == null ? Environment.NewLine : run.Text);
            }
            return sb.ToString();
        }
    }
}
