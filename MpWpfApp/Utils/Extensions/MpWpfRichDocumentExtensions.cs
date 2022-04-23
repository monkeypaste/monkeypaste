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
using System.Diagnostics;

namespace MpWpfApp {
    public static class MpWpfRichDocumentExtensions {
        public static TextPointer GetLineEndPosition(this TextPointer tp, int count) {
            var next_line_start_tp = tp.GetLineStartPosition(count + 1);
            if (next_line_start_tp == null) {
                // tp is DocumentEnd pointer
                return tp;
            }
            
            var line_end_tp = next_line_start_tp.GetNextInsertionPosition(LogicalDirection.Backward);
            if(line_end_tp == null) {
                // doc is empty and tp is both Document Start/End
                return tp;
            }
            return line_end_tp;
        }

        public static TextRange ContentRange(this TextElement te) {
            return new TextRange(te.ContentStart, te.ContentEnd);
        }
        public static TextRange ElementRange(this TextElement te) {
            return new TextRange(te.ElementStart, te.ElementEnd);
        }

        public static bool IsPointInRange(this TextRange tr, Point p) {
            var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();

            var ptp = rtb.GetPositionFromPoint(p, true);

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

        public static void FitDocToRtb(this RichTextBox rtb) {
            bool isReadOnly = rtb.IsReadOnly;
            bool isDropping = false;
            Size ds = new Size();

            if(rtb.GetVisualAncestor<MpContentView>() != null) {
                isDropping = MpDragDropManager.CurDropTarget == rtb.GetVisualAncestor<MpContentView>().ContentViewDropBehavior;
            }

            if (isDropping) {
                var fd = rtb.Document;
                double pad = 15;
                ds = fd.GetDocumentSize(pad);
                fd.PageWidth = ds.Width;
                fd.PageHeight = ds.Height;
                fd.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                fd.ConfigureLineHeight();


                var p = rtb.Document.PagePadding;
                p.Top = 3;
                rtb.Document.PagePadding = p;
                //double w = 1000;
                //double h = ds.Height;
                //rtb.Document.PageWidth = Math.Max(0, w - rtb.Margin.Left - rtb.Margin.Right - rtb.Padding.Left - rtb.Padding.Right);
                //rtb.Document.PageHeight = Math.Max(0, h - rtb.Margin.Top - rtb.Margin.Bottom - rtb.Padding.Top - rtb.Padding.Bottom);


            } else if (!isReadOnly) {
                ds = rtb.Document.GetDocumentSize();

                var cv = rtb.GetVisualAncestor<MpContentView>();
                double w = cv == null ? rtb.ActualWidth : cv.ActualWidth;
                double h = cv == null ? rtb.ActualHeight : cv.ActualHeight;
                rtb.Document.PageWidth = Math.Max(0, w - rtb.Margin.Left - rtb.Margin.Right - rtb.Padding.Left - rtb.Padding.Right);
                rtb.Document.PageHeight = Math.Max(0, h - rtb.Margin.Top - rtb.Margin.Bottom - rtb.Padding.Top - rtb.Padding.Bottom);
            } else {
                rtb.Document.PageWidth = Math.Max(0, rtb.ActualWidth - rtb.Margin.Left - rtb.Margin.Right - rtb.Padding.Left - rtb.Padding.Right);
                rtb.Document.PageHeight = Math.Max(0, rtb.ActualHeight - rtb.Margin.Top - rtb.Margin.Bottom - rtb.Padding.Top - rtb.Padding.Bottom);
            }

            rtb.UpdateLayout();

            if(isDropping || !isReadOnly) {
                if (ds.Width > rtb.ActualWidth || isDropping) {
                    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                } else {
                    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }

                if (ds.Height > rtb.ActualHeight) {
                    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                } else {
                    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
            } else {

                rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
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

        public static IEnumerable<TextElement> GetAllTextElements(this TextRange tr) {
            for (TextPointer position = tr.Start;
              position != null && position.CompareTo(tr.End) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    if (position.Parent is TextElement te) {
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

        
        public static void LoadImage(this TextRange tr, string base64Str, Size? docSize = null) {
            if (!base64Str.IsStringBase64()) {
                Debugger.Break();
                return;
            }

            BitmapSource bmpSrc = base64Str.ToBitmapSource();

            var img = new Image() {
                Source = bmpSrc,
                Width = bmpSrc.Width,
                Height = bmpSrc.Height,
                Stretch = System.Windows.Media.Stretch.None
            };

            docSize = docSize.HasValue ? docSize : MpMeasurements.Instance.ClipTileContentDefaultSize;
            double pad = 0;

            var vb = new Viewbox() {
                VerticalAlignment = VerticalAlignment.Top,
                Stretch = Stretch.Uniform,
                Width = docSize.Value.Width - pad,
                Height = docSize.Value.Width - pad,
                //Margin = new Thickness(5),
                Child = img
            };

            tr.Text = string.Empty;
            new InlineUIContainer(vb,tr.Start);
        }
        
        public static FlowDocument ToImageDocument(this string base64Str, Size? docSize = null) {
            if (!base64Str.IsStringBase64()) {
                Debugger.Break();
                return string.Empty.ToFlowDocument();
            }

            BitmapSource bmpSrc = base64Str.ToBitmapSource();

            var img = new Image() {
                Source = bmpSrc,
                Width = bmpSrc.Width,
                Height = bmpSrc.Height,
                Stretch = System.Windows.Media.Stretch.Uniform
            };

            var fd = string.Empty.ToFlowDocument();
            var p = fd.Blocks.FirstBlock as Paragraph;
            p.ContentRange().LoadImage(base64Str, docSize);

            //p.Inlines.Clear();

            //docSize = docSize.HasValue ? docSize : MpMeasurements.Instance.ClipTileContentDefaultSize;

            //var vb = new Viewbox() {
            //    VerticalAlignment = VerticalAlignment.Top,
            //    Stretch = Stretch.Uniform,
            //    Width = docSize.Value.Width,
            //    Height = docSize.Value.Width,
            //    Margin = new Thickness(5),
            //    Child = img
            //};
            //var iuic = new InlineUIContainer(vb);
            //p.Inlines.Add(iuic);

            fd.LineStackingStrategy = LineStackingStrategy.MaxHeight;
            fd.ConfigureLineHeight();
            p.ContentRange().ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Center);

            return fd;
        }

        public static void LoadFileItem(this TextRange tr, string path, int iconId = 0, double iconSize = 16) {
            string iconBase64 = string.Empty;

            if (iconId > 0) {
                var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);
                if (ivm == default) {
                    iconBase64 = MpBase64Images.Warning;
                } else {
                    iconBase64 = ivm.IconBase64;
                }
            } else if (path.IsFileOrDirectory()) {
                iconBase64 = MpShellEx.GetBitmapFromPath(path, MpIconSize.SmallIcon16).ToBase64String();
            }
            if (string.IsNullOrEmpty(iconBase64)) {
                iconBase64 = MpBase64Images.Warning;
            }

            BitmapSource bmpSrc = iconBase64.ToBitmapSource();
            var pathIcon = new Image() {
                Source = bmpSrc,
                Width = iconSize,
                Height = iconSize,
                Stretch = System.Windows.Media.Stretch.Fill
            };

            tr.Text = string.Empty;
            //var fd = string.Empty.ToFlowDocument();
            var iuc = new InlineUIContainer(pathIcon) {
                BaselineAlignment = BaselineAlignment.Bottom
            };

            Paragraph p = null;
            if (tr.Start.Parent is Block b) {
                if (b is Paragraph) {
                    p = b as Paragraph;
                    p.Inlines.Clear();
                    p.Inlines.Add(iuc);
                }
            } else {
                Debugger.Break();
            }
            //else if(tr.Start.Parent is Inline i) {

            //} else if(tr.Start.Parent is FlowDocument fd) {

            //}


            string pathDir = path;
            if (File.Exists(pathDir)) {
                pathDir = Path.GetDirectoryName(pathDir);
            }

            var pathLink = new Hyperlink(new Run(Path.GetFileName(pathDir))) {
                IsEnabled = true,
                NavigateUri = new Uri(pathDir, UriKind.Absolute)
            };
            pathLink.RequestNavigate += (s, e) => {
                return;
            };
            pathLink.PreviewMouseDown += (s, e) => {
                return;
            };
            pathLink.MouseEnter += (s, e) => {
                return;
            };

            p.Inlines.Add(pathLink);
            //fd.Blocks.Clear();
            //fd.Blocks.Add(p);

            return;
        }

        public static FlowDocument ToFilePathDocument(this string path, int iconId = 0, double iconSize = 16) {
            var fd = string.Empty.ToFlowDocument();
            fd.Blocks.FirstBlock.ContentRange().LoadFileItem(path, iconId, iconSize);
            return fd;
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
                    range2.Save(ms, System.Windows.DataFormats.Rtf,true);
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
            //if (rtb != null && rtbSelection != null) {
            //    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            //}
            return rtf;
        }

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
                return str.ToImageDocument().ToRichText();
            }
            if (str.IsStringFileOrPathFormat()) {
                return str.ToFilePathDocument(iconId).ToRichText();
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

        public static FlowDocument Combine(this FlowDocument fd, FlowDocument ofd, TextPointer insertPointer = null, bool insertNewline = true) {
            return CombineFlowDocuments(ofd, fd, insertPointer, insertNewline);
        }

        public static FlowDocument Combine(this FlowDocument fd, string text, TextPointer insertPointer = null, bool insertNewline = true) {
            return CombineFlowDocuments(text.ToRichText().ToFlowDocument(), fd, insertPointer, insertNewline);
        }

        public static void LoadAsRtf(this TextRange tr, string str, int iconId = 0) {
            // NOTE iconId is only used to convert file path's to rtf w/ icon 

            if (string.IsNullOrEmpty(str)) {
                tr.Text = str;
                return;
            }
            if (str.IsStringRichText()) {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(str))) {
                    try {
                        tr.Load(stream, System.Windows.DataFormats.Rtf);

                        var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                        if (rtbAlignment == null || rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}") {
                            //ignore to r
                        } else if ((TextAlignment)rtbAlignment == TextAlignment.Justify) {
                            tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                        }

                        //var fd = tr.Start.Parent.FindParentOfType<FlowDocument>();
                        //var ps = fd.GetDocumentSize();
                        //fd.PageWidth = ps.Width;
                        //fd.PageHeight = ps.Height;
                        //fd.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                        //fd.ConfigureLineHeight();

                        //return fd;
                    }
                    catch (Exception ex) {
                        MpConsole.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...");
                        MpConsole.WriteLine("Exception Details: " + ex);
                        return;// str.ToPlainText().ToFlowDocument();
                    }
                }
                return;
            }
            if (str.IsStringBase64()) {
                tr.LoadImage(str);
                return;
                //return str.ToImageDocument();
            }
            if (str.IsStringFileOrPathFormat()) {
                tr.LoadFileItem(str, iconId);
                return;
                //return str.ToFilePathDocument();
            }
            tr.LoadAsRtf(str.ToRichText(iconId), iconId);
        }
        public static FlowDocument ToFlowDocument(this string str, int iconId = 0) {
            // NOTE iconId is only used to convert file path's to rtf w/ icon 

            if (string.IsNullOrEmpty(str)) {
                return string.Empty.ToRichText().ToFlowDocument();
            }
            if(str.IsStringRichText()) {
                using (var stream = new MemoryStream(Encoding.Default.GetBytes(str))) {
                    try {
                        var fd = new FlowDocument();
                        var range = new TextRange(fd.ContentStart, fd.ContentEnd);
                        range.Load(stream, System.Windows.DataFormats.Rtf);

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
                        fd.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                        fd.ConfigureLineHeight();

                        return fd;
                    }
                    catch (Exception ex) {
                        MpConsole.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...");
                        MpConsole.WriteLine("Exception Details: " + ex);
                        return str.ToPlainText().ToFlowDocument();
                    }
                }
            }
            if(str.IsStringBase64()) {
                return str.ToImageDocument();
            }
            if(str.IsStringFileOrPathFormat()) {
                return str.ToFilePathDocument();
            }
            return str.ToRichText(iconId).ToFlowDocument();
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

        public static IEnumerable<TextRange> FindAllText(
            this TextPointer start,
            TextPointer end,
            string input,
            bool isCaseSensitive = true) {
            if (start == null) {
                yield return null;
            }

            //var matchRangeList = new List<TextRange>();
            while (start != null && start != end) {
                var matchRange = start.FindText(end, input, isCaseSensitive ? FindFlags.MatchCase : FindFlags.None);
                if (matchRange == null) {
                    break;
                }
                //matchRangeList.Add(matchRange);
                start = matchRange.End.GetNextContextPosition(LogicalDirection.Forward);
                yield return matchRange;
            }

            //return matchRangeList;
        }

        public static TextRange FindText(
            this TextPointer start, 
            TextPointer end, 
            string input, 
            FindFlags flags = FindFlags.MatchCase, 
            CultureInfo cultureInfo = null) {
            if(string.IsNullOrEmpty(input) || start == null || end == null) {
                return null;
            }
            cultureInfo = cultureInfo == null ? CultureInfo.CurrentCulture : cultureInfo;

            TextRange textRange = null;
            if (start.CompareTo(end) < 0) {
                try {
                    if (findMethod == null) {
                        findMethod = typeof(FrameworkElement).Assembly
                                        .GetType("System.Windows.Documents.TextFindEngine")
                                        .GetMethod("Find", BindingFlags.Static | BindingFlags.Public);
                    }
                    object result = findMethod.Invoke(null, new object[] { 
                        start,
                        end,
                        input, flags, cultureInfo });
                    textRange = result as TextRange;
                }
                catch (ApplicationException) {
                    textRange = null;
                }
            }

            return textRange;
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

        public static void ConfigureLineHeight(this FlowDocument doc) {
            //return;
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

        public static FlowDocument CombineFlowDocuments(FlowDocument from, FlowDocument to, TextPointer toInsertPointer = null, bool insertNewLine = false) {
            using (MemoryStream stream = new MemoryStream()) {
                var rangeFrom = new TextRange(from.ContentStart, from.ContentEnd);

                XamlWriter.Save(rangeFrom, stream);
                rangeFrom.Save(stream, DataFormats.XamlPackage);

                //if(insertNewLine) {
                //    var lb = new LineBreak();
                //    var p = (Paragraph)to.Blocks.LastBlock;
                //    p.LineHeight = 1;
                //    p.Inlines.Add(lb);
                //}

                var rangeTo = new TextRange(to.ContentEnd, to.ContentEnd);
                rangeTo.Load(stream, DataFormats.XamlPackage);

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
