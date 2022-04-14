using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public static class MpWpfRichDocumentExtensions {
        public static bool HasTable(this RichTextBox rtb) {
            return rtb.Document.Blocks.Any(x => x is Table);
        }

        public static void FitDocToRtb(this RichTextBox rtb) {
            bool isReadOnly = false;
            if (rtb.DataContext is MpContentItemViewModel civm) {
                isReadOnly = civm.IsContentReadOnly;
            }
            if (!isReadOnly) {
                var clv = rtb.GetVisualAncestor<MpContentListView>();
                double w = clv == null ? rtb.ActualWidth : clv.ActualWidth;
                double h = clv == null ? rtb.ActualHeight : clv.ActualHeight;
                rtb.Document.PageWidth = Math.Max(0, w - rtb.Margin.Left - rtb.Margin.Right - rtb.Padding.Left - rtb.Padding.Right);
                rtb.Document.PageHeight = Math.Max(0, rtb.ActualHeight - rtb.Margin.Top - rtb.Margin.Bottom - rtb.Padding.Top - rtb.Padding.Bottom);
            } else {
                rtb.Document.PageWidth = Math.Max(0, rtb.ActualWidth - rtb.Margin.Left - rtb.Margin.Right - rtb.Padding.Left - rtb.Padding.Right);
                rtb.Document.PageHeight = Math.Max(0, rtb.ActualHeight - rtb.Margin.Top - rtb.Margin.Bottom - rtb.Padding.Top - rtb.Padding.Bottom);
            }

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

        public static TextRange Clone(this TextSelection ts) {
            return new TextRange(ts.Start, ts.End);
        }

        public static void SetRtf(this System.Windows.Controls.RichTextBox rtb, string document) {
            //var rtbSelection = rtb.Selection;
            var documentBytes = UTF8Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Load(reader, System.Windows.DataFormats.Rtf);
                //rtb.SelectAll();
                //rtb.Selection.Load(reader, System.Windows.DataFormats.Rtf);
                //rtb.CaretPosition = rtb.Document.ContentStart;
                //if (rtbSelection != null) {
                //    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                //}
            }
        }

        public static void SetRtf(this FlowDocument fd, string document) {
            //var rtbSelection = rtb.Selection;
            var documentBytes = UTF8Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                new TextRange(fd.ContentStart, fd.ContentEnd).Load(reader, System.Windows.DataFormats.Rtf);
                //rtb.SelectAll();
                //rtb.Selection.Load(reader, System.Windows.DataFormats.Rtf);
                //rtb.CaretPosition = rtb.Document.ContentStart;
                //if (rtbSelection != null) {
                //    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                //}
            }
        }

        public static string GetRtf(this RichTextBox rtb) {
            return rtb.Document.ToRichText();
        }

        public static void SetXaml(this System.Windows.Controls.RichTextBox rtb, string document) {
            var documentBytes = Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                rtb.SelectAll();
                rtb.Selection.Load(reader, System.Windows.DataFormats.Xaml);
            }
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
                    if(position.Parent is TextElement te) {
                        yield return te;
                    }
                }
            }
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



        public static string ToRichText(this FlowDocument fd) {
            RichTextBox rtb = null;
            TextSelection rtbSelection = null;
            if (fd.Parent != null && fd.Parent.GetType() == typeof(RichTextBox)) {
                rtb = (RichTextBox)fd.Parent;
                rtbSelection = rtb.Selection;
            }
            string rtf = string.Empty;
            using (var ms = new MemoryStream()) {
                try {
                    var range2 = new TextRange(fd.ContentStart, fd.ContentEnd);
                    range2.Save(ms, System.Windows.DataFormats.Rtf);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(ms)) {
                        rtf = sr.ReadToEnd();
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error converting flow document to text: ", ex);
                    return rtf;
                }
            }
            if (rtb != null && rtbSelection != null) {
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return rtf;
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public static string ToRichText(this string str, int iconId = 0) {
            // NOTE iconId is only used for converting file path's icons to rtf
            if(str == null) {
                str = string.Empty;
            }
            if(str.IsStringRichText() || str.IsStringRichTextTable()) {
                return str;
            }
            if(str.IsStringQuillText()) {
                return MpHtmlToRtfConverter.ConvertHtmlToRtf(str);
            }
            if(str.IsStringXaml()) {
                using (var stringReader = new StringReader(str)) {
                    var xmlReader = XmlReader.Create(stringReader);
                    //if (!IsStringFlowSection(xaml)) {
                    //    return (FlowDocument)XamlReader.Load(xmlReader);
                    //}
                    var doc = new FlowDocument();
                    var data = XamlReader.Load(xmlReader);
                    if (data.GetType() == typeof(Span)) {
                        Span span = (Span)data;
                        while (span.Inlines.Count > 0) {
                            //doc.Blocks.Add(sec.Blocks.FirstBlock);
                            var inline = span.Inlines.FirstInline;
                            span.Inlines.Remove(inline);
                            doc.Blocks.Add(new Paragraph(inline));
                        }
                    } else if (data.GetType() == typeof(Section)) {
                        Section sec = (Section)data;
                        while (sec.Blocks.Count > 0) {
                            //doc.Blocks.Add(sec.Blocks.FirstBlock);
                            var block = sec.Blocks.FirstBlock;
                            sec.Blocks.Remove(block);
                            doc.Blocks.Add(block);
                        }
                    } else {
                        doc = (FlowDocument)data;
                    }

                    // alternative:
                    /*
                        var richTextBox = new System.Windows.Controls.RichTextBox();
                        if (string.IsNullOrEmpty(xaml)) {
                            return string.Empty;
                        }

                        var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);

                        using (var xamlMemoryStream = new MemoryStream()) {
                            using (var xamlStreamWriter = new StreamWriter(xamlMemoryStream)) {
                                xamlStreamWriter.Write(xaml);
                                xamlStreamWriter.Flush();
                                xamlMemoryStream.Seek(0, SeekOrigin.Begin);

                                textRange.Load(xamlMemoryStream, DataFormats.Xaml);
                            }
                        }

                        using (var rtfMemoryStream = new MemoryStream()) {
                            textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                            textRange.Save(rtfMemoryStream, DataFormats.Rtf);
                            rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                            using (var rtfStreamReader = new StreamReader(rtfMemoryStream)) {
                                return rtfStreamReader.ReadToEnd();
                            }
                        }

                    */

                    return doc.ToRichText();
                }
            }
            if (str.IsStringBase64()) {
                return str.ToImageRtf();
            }
            if (str.IsStringFileOrPathFormat()) {
                return str.ToFileDropItemRtf(iconId);
            }
            if (str.IsStringPlainText()) {
                using (System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox()) {
                    rtb.Text = str;
                    rtb.Font = new System.Drawing.Font(MpPreferences.DefaultFontFamily, (float)MpPreferences.DefaultFontSize);
                    return rtb.Rtf;
                }
            }

            return string.Empty;
        }

        public static string ToPlainText(this FlowDocument fd) {
            return new TextRange(fd.ContentStart, fd.ContentEnd).Text;
        }

        public static string ToPlainText(this TextElement te) {
            if (te == null) {
                return string.Empty;
            }
            return new TextRange(te.ContentStart, te.ContentEnd).Text;
        }

        public static FlowDocument Combine(this FlowDocument fd, FlowDocument ofd, bool insertNewline = true) {
            return CombineFlowDocuments(ofd, fd, insertNewline);
        }

        public static FlowDocument Combine(this FlowDocument fd, string text, bool insertNewline = true) {
            return CombineFlowDocuments(text.ToRichText().ToFlowDocument(), fd, insertNewline);
        }

        public static FlowDocument ToFlowDocument(this string str, int iconId = 0) {
            // NOTE iconId is only used to convert file path's to rtf w/ icon 

            if (string.IsNullOrEmpty(str)) {
                return string.Empty.ToRichText().ToFlowDocument();
            }
            if(!str.IsStringRichText()) {
                str = str.ToRichText(iconId);
            }
            using (var stream = new MemoryStream(UTF8Encoding.Default.GetBytes(str))) {
                try {
                    var flowDocument = new FlowDocument();
                    var range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                    range.Load(stream, System.Windows.DataFormats.Rtf);

                    var tr = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                    var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                    if (rtbAlignment == null || rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}") {
                        //ignore to r
                    } else if ((TextAlignment)rtbAlignment == TextAlignment.Justify) {
                        tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                    }
                    var ps = flowDocument.GetDocumentSize();
                    flowDocument.PageWidth = ps.Width;
                    flowDocument.PageHeight = ps.Height;
                    flowDocument.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                    flowDocument.ConfigureLineHeight();
                    return flowDocument;
                }
                catch (Exception ex) {
                    MpConsole.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...");
                    MpConsole.WriteLine("Exception Details: " + ex);
                    return str.ToPlainText().ToFlowDocument();
                }
            }        
        }

        public static FlowDocument ToFlowDocument(this string str, out Size docSize) {
            FlowDocument fd = str.ToFlowDocument() as FlowDocument;
            docSize = new Size(fd.PageWidth, fd.PageHeight);
            return fd;
        }

        //public static FlowDocument TokenizeMatches(this FlowDocument fd, string matchValue, Uri uri, bool isCaseSensitive = false) {
        //    var trl = MpHelpers.FindStringRangesFromPosition(fd.ContentStart, matchValue, isCaseSensitive);

        //}

        //public static Hyperlink ToHyperlink(this TextRange tr, Uri uri) {
        //    var hl = new Hyperlink(tr.Start, tr.End);
        //    hl.NavigateUri = uri;
        //    hl.IsEnabled = true;
            
        //}

        public static string ToRichText(this TextRange tr) {
            //if(tr == null) {
            //    return string.Empty;
            //}
            //using (var rangeStream = new MemoryStream()) {
            //    using(var writerStream = new StreamWriter(rangeStream)) {
            //        try {
            //            if (tr.CanLoad(DataFormats.Rtf)) {
            //                tr.Load(rangeStream, DataFormats.Rtf);

            //                rangeStream.Seek(0, SeekOrigin.Begin);
            //                using (var rtfStreamReader = new StreamReader(rangeStream)) {
            //                    return rtfStreamReader.ReadToEnd();
            //                }
            //            }
            //        }
            //        catch (Exception ex) {
            //            MpConsole.WriteTraceLine(ex);
            //            return tr.Text;
            //        }
            //    }
            //}
            //return tr.Text;
            using (MemoryStream ms = new MemoryStream()) {
                tr.Save(ms, DataFormats.Rtf);
                return Encoding.Default.GetString(ms.ToArray());
            }
                
        }
        public static string ToXamlPackage(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return string.Empty.ToRichText().ToXamlPackage();
            }
            if (str.IsStringQuillText()) {
                return str.ToRichText().ToXamlPackage();
            }
            if (str.IsStringPlainText()) {
                return str.ToRichText().ToXamlPackage();
            }
            if (str.IsStringRichText()) {
                var assembly = Assembly.GetAssembly(typeof(System.Windows.FrameworkElement));
                var xamlRtfConverterType = assembly.GetType("System.Windows.Documents.XamlRtfConverter");
                var xamlRtfConverter = Activator.CreateInstance(xamlRtfConverterType, true);
                var convertRtfToXaml = xamlRtfConverterType.GetMethod("ConvertRtfToXaml", BindingFlags.Instance | BindingFlags.NonPublic);
                var xamlContent = (string)convertRtfToXaml.Invoke(xamlRtfConverter, new object[] { str });
                return xamlContent;
            }
            throw new Exception("ToXaml exception string must be plain or rich text. Its content is: " + str);
        }

        public static string ToXamlPackage(this FlowDocument fd) {
            //TextRange range = new TextRange(fd.ContentStart, fd.ContentEnd);
            //using (MemoryStream stream = new MemoryStream()) {
            //    range.Save(stream, DataFormats.Xaml);
            //    //return ASCIIEncoding.Default.GetString(stream.ToArray());
            //    return UTF8Encoding.Default.GetString(stream.ToArray());
            //}
            return fd.ToRichText().ToXamlPackage();
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
            this TextPointer findContainerStartPosition, 
            TextPointer findContainerEndPosition, 
            string input, 
            FindFlags flags = FindFlags.FindWholeWordsOnly | FindFlags.MatchCase, 
            CultureInfo cultureInfo = null) {
            cultureInfo = cultureInfo == null ? CultureInfo.CurrentCulture : cultureInfo;

            TextRange textRange = null;
            if (findContainerStartPosition.CompareTo(findContainerEndPosition) < 0) {
                try {
                    if (findMethod == null) {
                        findMethod = typeof(FrameworkElement).Assembly
                                        .GetType("System.Windows.Documents.TextFindEngine")
                                        .GetMethod("Find", BindingFlags.Static | BindingFlags.Public);
                    }
                    object result = findMethod.Invoke(null, new object[] { 
                        findContainerStartPosition,
                        findContainerEndPosition,
                        input, flags, cultureInfo });
                    textRange = result as TextRange;
                }
                catch (ApplicationException) {
                    textRange = null;
                }
            }

            return textRange;
        }

        public static Size GetDocumentSize(this FlowDocument doc) {
            //Table docTable = doc.GetVisualDescendent<Table>();
            //if (docTable != null) {
            //    // TODO may need to uniquely find table dimensions
            //}
            var ft = doc.GetFormattedText();
            var ds = new Size(ft.Width, ft.Height);

            return ds;
        }

        public static void ConfigureLineHeight(this FlowDocument doc) {
            if (doc == null) {
                throw new ArgumentNullException("doc");
            }

            foreach (var b in doc.Blocks) {
                if (b is Paragraph p) {
                    p.LineHeight = p.FontSize + (p.FontSize * 0.333);
                }
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
              MpPreferences.ThisAppDip);

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

        public static FlowDocument CombineFlowDocuments(FlowDocument from, FlowDocument to, bool insertNewLine = false) {
            RichTextBox fromRtb = null, toRtb = null;
            TextSelection fromSelection = null, toSelection = null;
            if (from.Parent != null && from.Parent.GetType() == typeof(RichTextBox)) {
                fromRtb = (RichTextBox)from.Parent;
                fromSelection = fromRtb.Selection;
            }
            if (to.Parent != null && to.Parent.GetType() == typeof(RichTextBox)) {
                toRtb = (RichTextBox)to.Parent;
                toSelection = toRtb.Selection;
            }
            using (MemoryStream stream = new MemoryStream()) {
                var rangeFrom = new TextRange(from.ContentStart, from.ContentEnd);

                System.Windows.Markup.XamlWriter.Save(rangeFrom, stream);
                rangeFrom.Save(stream, DataFormats.XamlPackage);

                //if(insertNewLine) {
                //    var lb = new LineBreak();
                //    var p = (Paragraph)to.Blocks.LastBlock;
                //    p.LineHeight = 1;
                //    p.Inlines.Add(lb);
                //}

                var rangeTo = new TextRange(to.ContentEnd, to.ContentEnd);
                rangeTo.Load(stream, DataFormats.XamlPackage);

                if (fromRtb != null && fromSelection != null) {
                    fromRtb.Selection.Select(fromSelection.Start, fromSelection.End);
                }
                if (toRtb != null && toSelection != null) {
                    toRtb.Selection.Select(toSelection.Start, toSelection.End);
                }

                var tr = new TextRange(to.ContentStart, to.ContentEnd);
                var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                if (rtbAlignment == null ||
                    rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}" ||
                    (TextAlignment)rtbAlignment == TextAlignment.Justify) {
                    tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                }

                var ps = to.GetDocumentSize();
                to.PageWidth = ps.Width;
                to.PageHeight = ps.Height;
                return to;
            }
        }

        public static async Task<FlowDocument> CombineFlowDocumentsAsync(FlowDocument from, FlowDocument to, bool insertNewLine = false, DispatcherPriority priority = DispatcherPriority.Background) {
            FlowDocument fd = null;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                using (MemoryStream stream = new MemoryStream()) {
                    var rangeFrom = new TextRange(from.ContentStart, from.ContentEnd);

                    System.Windows.Markup.XamlWriter.Save(rangeFrom, stream);
                    rangeFrom.Save(stream, DataFormats.XamlPackage);

                    if (insertNewLine) {
                        var lb = new LineBreak();
                        var p = (Paragraph)to.Blocks.LastBlock;
                        p.LineHeight = 1;
                        p.Inlines.Add(lb);
                    }

                    var rangeTo = new TextRange(to.ContentEnd, to.ContentEnd);
                    rangeTo.Load(stream, DataFormats.XamlPackage);

                    fd = to;
                }
            }, priority);
            return fd;
        }

        public static FlowDocument InsertFlowDocument(this FlowDocument to, FlowDocument from, TextRange toInsertRange) {
            using (MemoryStream stream = new MemoryStream()) {
                var rangeFrom = new TextRange(from.ContentStart, from.ContentEnd);
                XamlWriter.Save(rangeFrom, stream);
                rangeFrom.Save(stream, DataFormats.XamlPackage);
                toInsertRange.Load(stream, DataFormats.XamlPackage);
                return to;
            }
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
    }
}
