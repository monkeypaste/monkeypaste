using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MonkeyPaste.Common.Wpf {
    public static class MpWpfRichDocumentExtensions {
        public static void CloneNeighborFormatting(this TextElement te, LogicalDirection prefDir = LogicalDirection.Backward) {
            if (prefDir == LogicalDirection.Backward && te.PreviousElement() == null) {
                prefDir = LogicalDirection.Forward;
            } else if (prefDir == LogicalDirection.Forward && te.NextElement() == null) {
                prefDir = LogicalDirection.Backward;
            }
            TextElement neighbor = null;
            if (prefDir == LogicalDirection.Forward) {
                neighbor = te.NextElement();
            } else {
                neighbor = te.PreviousElement();
            }
            if (neighbor == null) {
                return;
            }
            neighbor.CloneFormatting(ref te);
        }

        public static void CloneFormatting(this TextElement from, ref TextElement to) {
            to.FontFamily = from.FontFamily;
            to.FontStyle = from.FontStyle;
            to.FontWeight = from.FontWeight;
            to.FontStretch = from.FontStretch;
            to.FontSize = from.FontSize;
            to.Foreground = from.Foreground;
            to.Background = from.Background;
            to.TextEffects = from.TextEffects;
        }


        public static TextElement PreviousElement(this TextElement te) {
            return te.ContentStart.GetAdjacentElement(LogicalDirection.Backward) as TextElement;
        }

        public static TextElement NextElement(this TextElement te) {
            return te.ContentEnd.GetAdjacentElement(LogicalDirection.Forward) as TextElement;
        }

        public static TextPointer GetLineEndPosition(this TextPointer tp, int count) {
            var next_line_start_tp = tp.GetLineStartPosition(count + 1);
            if (next_line_start_tp == null) {
                // tp is DocumentEnd pointer
                return tp.DocumentEnd;
            }

            var line_end_tp = next_line_start_tp.GetNextInsertionPosition(LogicalDirection.Backward);
            if (line_end_tp == null) {
                // doc is empty and tp is both Document Start/End
                return tp.DocumentEnd;
            }
            return line_end_tp;
        }

        public static TextRange ToTextRange(this TextPointer tp) {
            return new TextRange(tp, tp);
        }

        public static FlowDocument GetFlowDocument(this TextPointer tp) {
            return tp.Parent.FindParentOfType<FlowDocument>();
        }

        public static RichTextBox GetRichTextBox(this TextPointer tp) {
            var fd = tp.GetFlowDocument();
            if (fd == null) {
                return null;
            }
            return fd.GetVisualAncestor<RichTextBox>();
        }

        public static int ToOffset(this TextPointer tp, TextPointer refPointer = null) {
            refPointer = refPointer == null ? tp.DocumentStart : refPointer;
            return refPointer.GetOffsetToPosition(tp);
        }

        public static TextPointer ToPointer(this int offset, TextPointer refPointer) {
            return refPointer.GetPositionAtOffset(offset);
        }

        public static TextRange ContentRange(this TextElement te) {
            return new TextRange(te.ContentStart, te.ContentEnd);
        }
        public static TextRange ElementRange(this TextElement te) {
            return new TextRange(te.ElementStart, te.ElementEnd);
        }

        public static TextRange ContentRange(this FlowDocument fd) {
            return new TextRange(fd.ContentStart, fd.ContentEnd);
        }

        public static bool IsRangeInSameDocument(this TextRange tr, TextRange otr) {
            return tr.Start.IsInSameDocument(otr.Start) &&
                   tr.End.IsInSameDocument(otr.End);
        }

        public static bool IsPointInRange(this TextRange tr, Point p) {
            var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();

            var ptp = rtb.GetPositionFromPoint(p, false);
            if (ptp == null) {
                return false;
            }
            return tr.Contains(ptp);
        }

        public static bool IsImageDocument(this FlowDocument fd) {
            return fd.Blocks.FirstBlock is Paragraph p &&
                    p.Inlines.FirstInline is InlineUIContainer iuic &&
                    iuic.Child is Image;
        }
        public static bool HasTable(this RichTextBox rtb) {
            return rtb.Document.Blocks.Any(x => x is Table);
        }

        public static void FitDocToRtb(this RichTextBox rtb, bool needsDragDropPadding = false) {
            if (!rtb.IsLoaded || rtb.DataContext == null) {
                return;
            }

            bool isReadOnly = rtb.IsReadOnly;
            Size ds = rtb.DataContext is MpISizeViewModel ?
                        new Size((rtb.DataContext as MpISizeViewModel).Width,
                                 (rtb.DataContext as MpISizeViewModel).Height)
                        : rtb.Document.GetDocumentSize();


            if (needsDragDropPadding) {
                var fd = rtb.Document;
                double pad = 15;

                fd.PageWidth = Math.Max(fd.PageWidth, ds.Width + pad);
                fd.PageHeight = Math.Max(fd.PageHeight, ds.Height + pad);


                var p = rtb.Document.PagePadding;
                p.Top = 3;
                rtb.Document.PagePadding = p;
                //double w = 1000;
                //double h = ds.Height;
                //rtb.Document.PageWidth = Math.Max(0, w - rtb.Margin.Left - rtb.Margin.Right - rtb.Padding.Left - rtb.Padding.Right);
                //rtb.Document.PageHeight = Math.Max(0, h - rtb.Margin.Top - rtb.Margin.Bottom - rtb.Padding.Top - rtb.Padding.Bottom);


            } else if (!isReadOnly) {
                var cv = rtb.GetVisualAncestor<UserControl>();
                double w = cv == null ? rtb.ActualWidth : cv.ActualWidth;
                double h = cv == null ? rtb.ActualHeight : cv.ActualHeight;
                rtb.Document.PageWidth = Math.Max(0, w - rtb.Margin.Left - rtb.Margin.Right - rtb.Padding.Left - rtb.Padding.Right);
                rtb.Document.PageHeight = Math.Max(0, h - rtb.Margin.Top - rtb.Margin.Bottom - rtb.Padding.Top - rtb.Padding.Bottom);
            } else {
                rtb.Document.PageWidth = Math.Max(0, rtb.ActualWidth - rtb.Margin.Left - rtb.Margin.Right - rtb.Padding.Left - rtb.Padding.Right);
                rtb.Document.PageHeight = Math.Max(0, rtb.ActualHeight - rtb.Margin.Top - rtb.Margin.Bottom - rtb.Padding.Top - rtb.Padding.Bottom);
            }

            rtb.Document.ConfigureLineHeight();
            rtb.UpdateLayout();
        }

        public static bool Equals(this TextRange tra, TextRange trb) {
            if (!tra.Start.IsInSameDocument(trb.Start)) {
                return false;
            }
            if (tra.Start.CompareTo(trb.Start) == 0 && tra.End.CompareTo(trb.End) == 0) {
                return true;
            }
            return false;
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

        public static FlowDocument Clone(this FlowDocument doc, TextRange rangeToEncode, out TextRange encodedRange) {
            using (MemoryStream stream = new MemoryStream()) {
                var clonedDoc = new FlowDocument();
                TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
                System.Windows.Markup.XamlWriter.Save(range, stream);
                range.Save(stream, DataFormats.XamlPackage);
                TextRange range2 = new TextRange(clonedDoc.ContentEnd, clonedDoc.ContentEnd);
                range2.Load(stream, DataFormats.XamlPackage);

                //create clone range using decoded range offsets
                encodedRange = new TextRange(
                    clonedDoc.ContentStart.GetPositionAtOffset(rangeToEncode.Start.ToOffset()),
                    clonedDoc.ContentStart.GetPositionAtOffset(rangeToEncode.End.ToOffset()));

                //select all iuic's from source doc
                var iuicl = doc.GetAllTextElements()
                                .Where(x => x is InlineUIContainer);

                Dictionary<TextRange, object> clonedIuic_lookup = new Dictionary<TextRange, object>();
                foreach (var iuic in iuicl) {
                    //create range's in cloned doc at each iuic offset paired w/ the iuic tag
                    var clone_iuic_start_tp = clonedDoc.ContentStart.GetPositionAtOffset(iuic.ContentStart.ToOffset());
                    var clone_iuic_end_tp = clonedDoc.ContentStart.GetPositionAtOffset(iuic.ContentEnd.ToOffset());

                    clonedIuic_lookup.Add(new TextRange(clone_iuic_start_tp, clone_iuic_end_tp), iuic.Tag);
                }

                //encode cloned iuic ranges with paired object ToString (which is encoded template string)
                clonedIuic_lookup.ForEach(x => x.Key.Text = x.Value.ToString());

                return clonedDoc;
            }
        }


        public static FlowDocument CloneDocument(FlowDocument document) {
            var copy = new FlowDocument();
            var sourceRange = new TextRange(document.ContentStart, document.ContentEnd);
            var targetRange = new TextRange(copy.ContentStart, copy.ContentEnd);

            using (var stream = new MemoryStream()) {
                sourceRange.Save(stream, DataFormats.XamlPackage);
                targetRange.Load(stream, DataFormats.XamlPackage);
            }

            return copy;
        }

        public static IEnumerable<TextElement> GetTextElementsOfTypes(this FlowDocument doc, params object[] types) {
            for (TextPointer position = doc.ContentStart;
              position != null && position.CompareTo(doc.ContentEnd) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    foreach (var type in types.Cast<Type>()) {
                        dynamic elm = Convert.ChangeType(position.Parent, type);
                        if (elm != null) {
                            yield return elm;
                        }
                    }
                }
            }
        }

        public static IEnumerable<TextElement> GetAllTextElements(this FlowDocument doc) {
            for (TextPointer position = doc.ContentStart;
              position != null && position.CompareTo(doc.ContentEnd) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    if (position.Parent is TextElement te) {
                        yield return te;
                    }
                }
            }
        }

        public static IEnumerable<TextElement> GetAllTextElements(this TextRange tr) {
            var tel = new List<TextElement>();
            for (TextPointer position = tr.Start;
              position != null && position.CompareTo(tr.End) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    if (position.Parent is TextElement te) {
                        tel.Add(te);
                    }
                }
            }
            if (tr.Start.Parent is TextElement ste) {
                // inlcude param element (different from FlowDocument version)
                if (tr.End.Parent is TextElement ete) {
                    if (ste != ete) {
                        tel.Add(ete);
                    }
                }
                tel.Add(ste);
            }
            return tel;
        }

        public static TextRange ToTextRange(this IEnumerable<TextElement> tel) {
            if (!tel.Any()) {
                return null;
            }


            var docStart = tel.ElementAt(0).ContentStart.DocumentStart;
            var toRemove = tel.Where(x => !x.ContentStart.IsInSameDocument(docStart) || !x.ContentEnd.IsInSameDocument(docStart));
            if (toRemove.Count() > 0) {
                Debugger.Break();
                tel = tel.Where(x => !toRemove.Contains(x));
                if (tel.Count() > 0) {
                    docStart = tel.ElementAt(0).ContentStart.DocumentStart;
                } else {
                    Debugger.Break();
                }
            }
            var itemRangeStart = tel.Aggregate((a, b) =>
                                        docStart.GetOffsetToPosition(a.ContentStart) <
                                        docStart.GetOffsetToPosition(b.ContentStart) ? a : b).ContentStart;
            var itemRangeEnd = tel.Aggregate((a, b) =>
                                        docStart.GetOffsetToPosition(a.ContentEnd) >
                                        docStart.GetOffsetToPosition(b.ContentEnd) ? a : b).ContentEnd;

            return new TextRange(itemRangeStart, itemRangeEnd);
        }
        public static IEnumerable<TextElement> GetRunsAndParagraphs(this FlowDocument doc) {
            for (TextPointer position = doc.ContentStart;
              position != null && position.CompareTo(doc.ContentEnd) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    Run run = position.Parent as Run;

                    if (run != null) {
                        yield return run;
                    } else {
                        //Paragraph para = position.Parent as Paragraph;

                        //if (para != null) {
                        //    yield return para;
                        //} 
                        Block block = position.Parent as Block;

                        if (block != null) {
                            yield return block;
                        }
                    }
                }
            }
        }
        public static string ToPlainText(this FlowDocument fd, bool removeLastLineEnding = true) {
            // NOTE this always adds a trailing line break so remove last two characters
            string pt = new TextRange(fd.ContentStart, fd.ContentEnd).Text;
            if (removeLastLineEnding) {
                return pt.RemoveLastLineEnding();
            }
            return pt;

        }

        public static string ToPlainText(this TextElement te) {
            // NOTE this always adds a trailing line break so remove last two characters

            if (te == null) {
                return string.Empty;
            }
            return new TextRange(te.ContentStart, te.ContentEnd).Text.RemoveLastLineEnding();
        }

        private static MethodInfo findMethod = null;
        [Flags]
        public enum FindFlags {
            FindInReverse = 2,
            FindWholeWordsOnly = 4,
            MatchAlefHamza = 0x20,
            MatchCase = 1,
            MatchDiacritics = 8,
            MatchKashida = 0x10,
            None = 0
        }

        public static TextRange FindText(
            this TextPointer start,
            TextPointer end,
            string input,
            bool isCaseSensitive = false,
            bool matchWholeWord = false,
            bool useRegEx = false,
            CultureInfo cultureInfo = null) {
            if (string.IsNullOrEmpty(input) || start == null || end == null) {
                return null;
            }
            cultureInfo = cultureInfo == null ? CultureInfo.CurrentCulture : cultureInfo;

            //var textRange = MpWpfFlowDocumentSearchExtensions.FindStringRangeFromPosition(start, input, isCaseSensitive);
            if (findMethod == null) {
                findMethod = typeof(FrameworkElement).Assembly
                                .GetType("System.Windows.Documents.TextFindEngine")
                                .GetMethod("Find", BindingFlags.Static | BindingFlags.Public);
            }
            FindFlags ff = FindFlags.None;
            ff |= isCaseSensitive ? FindFlags.MatchCase : ff;
            ff |= matchWholeWord ? FindFlags.FindWholeWordsOnly : ff;


            TextRange textRange = null;
            if (start.CompareTo(end) < 0) {
                try {

                    object result = findMethod.Invoke(null, new object[] {
                        start,
                        end,
                        input, ff, cultureInfo });
                    textRange = result as TextRange;
                }
                catch (ApplicationException) {
                    textRange = null;
                }
            }

            return textRange;
        }

        private static IEnumerable<TextRange> FindAllText(
             this TextPointer start,
             TextPointer end,
             string input,
             bool isCaseSensitive = false,
             bool matchWholeWord = false,
             bool useRegEx = false) {
            if (start == null) {
                yield return null;
            }
            while (start != null && start != end) {
                if (!start.IsInSameDocument(end)) {
                    break;
                }

                var matchRange = start.FindText(end, input, isCaseSensitive, matchWholeWord, useRegEx);
                if (matchRange == null) {
                    break;
                }
                start = matchRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                yield return matchRange;
            }
        }

        public static List<TextRange> FindText(
            this FlowDocument fd,
            string input,
            bool isCaseSensitive = false,
            bool matchWholeWord = false,
            bool useRegEx = false) {

            input = input.Replace(Environment.NewLine, string.Empty);


            if (useRegEx) {
                string pattern = input;
                string pt = fd.ToPlainText();
                var mc = Regex.Matches(pt, pattern, isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);

                var trl = new List<TextRange>();
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            var c_trl = fd.ContentStart.FindAllText(fd.ContentEnd, c.Value);
                            trl.AddRange(c_trl);
                        }
                    }
                }
                trl = trl.Distinct().ToList();
                if (useRegEx && matchWholeWord) {
                    trl = trl.Where(x => Regex.IsMatch(x.Text, $"\b{x.Text}\b")).ToList();
                }
                return trl;
            }

            return fd.ContentStart.FindAllText(fd.ContentEnd, input, isCaseSensitive, matchWholeWord).ToList();
        }

        public static Size GetDocumentSize(this FlowDocument doc, double padToAdd = 0) {
            //Table docTable = doc.GetVisualDescendent<Table>();
            //if (docTable != null) {
            //    // TODO may need to uniquely find table dimensions
            //}
            var ft = doc.GetFormattedText();
            var ds = new Size(ft.Width + padToAdd, ft.Height + padToAdd);
            return ds;
        }

        public static void ConfigureLineHeight(
           this FlowDocument doc) {
            doc.LineStackingStrategy = LineStackingStrategy.MaxHeight;
            doc.LineHeight = Double.NaN;
            foreach (var b in doc.Blocks) {
                b.LineStackingStrategy = LineStackingStrategy.MaxHeight;
                b.LineHeight = Double.NaN;
            }
            if (doc.Parent is FrameworkElement fe) {
                fe.UpdateLayout();
            }
        }

        public static FormattedText GetFormattedText(this FlowDocument doc) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            var output = new FormattedText(
              GetText(doc),
              CultureInfo.CurrentCulture,
              doc.FlowDirection,
              new Typeface(doc.FontFamily, doc.FontStyle, doc.FontWeight, doc.FontStretch),
              doc.FontSize,
              doc.Foreground,
              new NumberSubstitution(),
              TextFormattingMode.Display);//MpScreenInformation.ThisAppDip);

            int offset = 0;
            var runsAndParagraphsList = doc.GetRunsAndParagraphs().ToList();
            for (int i = 0; i < runsAndParagraphsList.Count; i++) {
                TextElement el = runsAndParagraphsList[i];
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

        public static void AppendBitmapSourceToFlowDocument(FlowDocument flowDocument, BitmapSource bitmapSource) {
            Image image = new Image() {
                Source = bitmapSource,
                Width = 300,
                Height = 300,
                Stretch = Stretch.Fill
            };
            Paragraph para = new Paragraph();
            para.Inlines.Add(image);
            flowDocument.Blocks.Add(para);
        }

        public static BitmapSource ToBitmapSource(
            this FlowDocument document,
            Size? docSize = null) {
            var size = docSize.HasValue ? docSize.Value : document.GetDocumentSize();

            if (size.Width <= 0) {
                size.Width = 1;
            }
            if (size.Height <= 0) {
                size.Height = 1;
            }
            //var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow);
            //size.Width *= dpi.DpiScaleX;
            //size.Height *= dpi.DpiScaleY;

            document.PagePadding = new Thickness(0);
            document.ColumnWidth = size.Width;
            document.PageWidth = size.Width;
            document.PageHeight = size.Height;
            document.Background = document.Background == null ? Brushes.White : document.Background;

            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            paginator.PageSize = size;

            var visual = new DrawingVisual();
            using (var drawingContext = visual.RenderOpen()) {
                // draw white background
                drawingContext.DrawRectangle(document.Background, null, new Rect(size));
            }
            visual.Children.Add(paginator.GetPage(0).Visual);
            var bitmap = new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                //dpi.PixelsPerInchX,
                //dpi.PixelsPerInchY,
                96, 96,
                PixelFormats.Pbgra32);

            bitmap.Render(visual);
            RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.HighQuality);
            return bitmap;
        }


        public static FlowDocument ToFlowDocument(this string str, string strFormat = "") {
            strFormat = strFormat == string.Empty ? DataFormats.Rtf : strFormat;
            if (strFormat != DataFormats.Text && strFormat != DataFormats.Rtf && strFormat != DataFormats.Xaml && strFormat != DataFormats.XamlPackage) {
                // invalid format for tr.Load
                Debugger.Break();
                return new FlowDocument();
            }
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(str))) {
                try {
                    var fd = new FlowDocument();
                    var range = new TextRange(fd.ContentStart, fd.ContentEnd);
                    range.Load(stream, strFormat);

                    var tr = new TextRange(fd.ContentStart, fd.ContentEnd);
                    var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                    if (rtbAlignment == null || rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}") {
                        //ignore to r
                    } else if ((TextAlignment)rtbAlignment == TextAlignment.Justify) {
                        tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                    }

                    var ps = fd.GetDocumentSize();
                    fd.PageWidth = ps.Width;
                    fd.PageHeight = ps.Height;
                    fd.ConfigureLineHeight();

                    return fd;
                }
                catch (Exception ex) {
                    Console.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...");
                    Console.WriteLine("Exception Details: " + ex);
                    return str.RtfToPlainText().ToFlowDocument();
                }
            }
        }

        public static string ToRichText(this FlowDocument fd) {
            //RichTextBox rtb = null;
            //TextSelection rtbSelection = null;
            //if (fd.Parent != null && fd.Parent.GetType() == typeof(RichTextBox)) {
            //    rtb = (RichTextBox)fd.Parent;
            //    rtbSelection = rtb.Selection;
            //}
            string rtf = string.Empty;
            using (var ms = new MemoryStream()) {
                try {
                    var range2 = new TextRange(fd.ContentStart, fd.ContentEnd);
                    range2.Save(ms, System.Windows.DataFormats.Rtf, true);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(ms)) {
                        rtf = sr.ReadToEnd();
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Error converting flow document to text: ", ex);
                    return rtf;
                }
            }
            //if (rtb != null && rtbSelection != null) {
            //    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            //}
            return rtf;
        }

        public static int Row(this TableCell tc) {
            if (tc == null) {
                return -1;
            }
            var tableRow = tc.Parent.FindParentOfType<TableRow>();
            if (tableRow == null) {
                return -1;
            }
            var tableRowGroup = tableRow.Parent.FindParentOfType<TableRowGroup>();
            if (tableRowGroup == null) {
                return -1;
            }
            var table = tableRowGroup.Parent.FindParentOfType<Table>();
            if (table == null) {
                return -1;
            }


            int curRowIdx = 0;
            for (int i = 0; i < table.RowGroups.Count; i++) {
                int rowIdx = table.RowGroups[i].Rows.IndexOf(tableRow);
                if (rowIdx < 0) {
                    curRowIdx += table.RowGroups[i].Rows.Count;
                } else {
                    curRowIdx += rowIdx;
                    return curRowIdx;
                }
            }
            return -1;
        }

        public static int Col(this TableCell tc) {
            if (tc == null) {
                return -1;
            }
            var tableRow = tc.Parent.FindParentOfType<TableRow>();
            if (tableRow == null) {
                return -1;
            }
            return tableRow.Cells.IndexOf(tc);
        }
    }
}
