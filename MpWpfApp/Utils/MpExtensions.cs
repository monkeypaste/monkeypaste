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
        public static void ClearHyperlinks(this RichTextBox rtb) {
            // Reapply event handling to hyperlinks after loading, since these are not saved:
            foreach (var paragraph in rtb.Document.Blocks.OfType<Paragraph>()) {
                foreach (var hyperlink in paragraph.Inlines.OfType<Hyperlink>()) {
                    rtb.Selection.Select(hyperlink.ContentStart, hyperlink.ContentEnd);
                    rtb.Selection.Text = rtb.Selection.Text;
                }
            }
        }

        public static List<Hyperlink> GetTemplateHyperlinks(this RichTextBox rtb) {
            var hyperlinkList = new List<Hyperlink>();
            foreach (var paragraph in rtb.Document.Blocks.OfType<Paragraph>()) {
                foreach (var hyperlink in paragraph.Inlines.OfType<Hyperlink>()) { 
                    if ((MpSubTextTokenType)hyperlink.Tag != MpSubTextTokenType.TemplateSegment) {
                        continue;
                    }
                    hyperlinkList.Add(hyperlink);
                }
            }
            return hyperlinkList;
        }

        public static List<Hyperlink> AddHyperlinks(this RichTextBox rtb) {        
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
                @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b"
            };
            List<Hyperlink> linkList = new List<Hyperlink>();
            TextRange fullDocRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            for (int i = 0; i < regExGroupList.Count; i++) {
                var regExStr = regExGroupList[i];
                MatchCollection mc = Regex.Matches(fullDocRange.Text, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            var matchRange = MpHelpers.FindStringRangeFromPosition(rtb.Document.ContentStart, c.Value);
                            var hyperlink = new Hyperlink(matchRange.Start, matchRange.End);
                            hyperlink.Tag = (MpSubTextTokenType)(i + 1);
                            linkList.Add(hyperlink);
                        }
                    }
                }
            }
            rtb.Tag = linkList;
            return linkList;
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

        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject {
            if (depObj == null) {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) {
                    return result;
                }
            }
            return null;
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
