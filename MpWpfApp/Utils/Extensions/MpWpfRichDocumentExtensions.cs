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
using System.Windows.Navigation;
using System.Windows.Input;
using System.Windows.Data;
using System.Text.RegularExpressions;

namespace MpWpfApp {
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
            if(line_end_tp == null) {
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
            if(fd == null) {
                return null;
            }
            return fd.GetVisualAncestor<RichTextBox>();
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
            if(ptp == null) {
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

        public static void FitDocToRtb(this RichTextBox rtb) {
            if(!rtb.IsLoaded || rtb.DataContext == null) {
                return;            
            }

            var ctvm = rtb.DataContext as MpClipTileViewModel;
            bool isReadOnly = ctvm.IsContentReadOnly;
            bool isDropping = false;
            Size ds = ctvm.UnformattedAndDecodedContentSize;

            if(rtb.GetVisualAncestor<MpContentView>() != null) {
                isDropping = MpDragDropManager.CurDropTarget == rtb.GetVisualAncestor<MpContentView>().ContentViewDropBehavior;
            }

            if (isDropping) {
                var fd = rtb.Document;
                double pad = 15;
                ds = fd.GetDocumentSize(pad);
                fd.PageWidth = Math.Max(fd.PageWidth,ds.Width);
                fd.PageHeight = Math.Max(fd.PageHeight,ds.Height);


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


            //if(isDropping || !isReadOnly) {
            //    if (ds.Width > rtb.ActualWidth) {
            //        //rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            //    } else {
            //        rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //    }

            //    if (ds.Height > rtb.ActualHeight) {
            //        rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            //    } else {
            //        rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //    }

            //} else {
            //    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //}

            ctvm.OnPropertyChanged(nameof(ctvm.IsVerticalScrollbarVisibile));
            ctvm.OnPropertyChanged(nameof(ctvm.IsHorizontalScrollbarVisibile));

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

                //int docLength = doc.ContentStart.GetOffsetToPosition(doc.ContentEnd);
                //int cdocLength = docLength;
                //for (int i = 0, ic = 0; i <= docLength && ic <= cdocLength; i++, ic++) {
                //    var doc_tp = doc.ContentStart.GetPositionAtOffset(i);
                //    var clone_tp = clonedDoc.ContentStart.GetPositionAtOffset(ic);
                //    if(doc_tp.Parent is TextElement doc_tp_te) {
                //        if(doc_tp.Parent is InlineUIContainer iuic && iuic.Tag is MpTextTemplate cit) {
                //            int sIdx = doc.ContentStart.GetOffsetToPosition(iuic.ContentStart);
                //            int eIdx = doc.ContentStart.GetOffsetToPosition(iuic.ContentEnd);

                //            var span = new Span(new Run(cit.EncodedTemplate), clone_tp);
                //            int csIdx = clonedDoc.ContentStart.GetOffsetToPosition(span.ContentStart);
                //            int ceIdx = clonedDoc.ContentStart.GetOffsetToPosition(span.ContentEnd);

                //            int iuicLength = eIdx - sIdx;
                //            int spanLength = ceIdx - csIdx;

                //            ic = clonedDoc.ContentStart.GetOffsetToPosition(span.ContentEnd);
                //            i = doc.ContentStart.GetOffsetToPosition(iuic.ContentEnd);

                //            cdocLength = clonedDoc.ContentStart.GetOffsetToPosition(clonedDoc.ContentEnd);
                //        } else if(clone_tp.Parent is TextElement) {
                //            (clone_tp.Parent as TextElement).Tag = doc_tp_te.Tag;
                //        }                        
                //    }
                //}
                
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
            if(tel.Count() == 0) {
                return null;
            }

            
            var docStart = tel.ElementAt(0).ContentStart.DocumentStart;
            var toRemove = tel.Where(x => !x.ContentStart.IsInSameDocument(docStart) || !x.ContentEnd.IsInSameDocument(docStart));
            if(toRemove.Count() > 0) {
                Debugger.Break();
                tel = tel.Where(x => !toRemove.Contains(x));
                if(tel.Count() > 0) {
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
        public static InlineUIContainer LoadImage(this TextRange tr, string base64Str, Size? docSize = null) {
            if (!base64Str.IsStringBase64()) {
                Debugger.Break();
                return null;
            }

            docSize = docSize.HasValue ? docSize : MpMeasurements.Instance.ClipTileContentDefaultSize;
            BitmapSource bmpSrc = base64Str.ToBitmapSource();//.Resize(docSize.Value);

            var img = new Image() {
                Source = bmpSrc,
                Width = docSize.Value.Width,
                Height = docSize.Value.Height,
                Stretch = System.Windows.Media.Stretch.Uniform
            };

            //tr.Text = string.Empty;

            using (var subStream = new MemoryStream()) {
                var subDoc = new FlowDocument();
                //var sub_tr = new TextRange(subDoc.ContentStart, subDoc.ContentEnd);
                
                var iuc = new InlineUIContainer(img);
                var p = new Paragraph(iuc);
                subDoc.Blocks.Add(p);
                p.ContentRange().Save(subStream, DataFormats.XamlPackage, true);
                subStream.Seek(0, SeekOrigin.Begin);
                tr.Load(subStream, DataFormats.XamlPackage);
                tr.Start.Parent.FindParentOfType<FlowDocument>().LineStackingStrategy = LineStackingStrategy.MaxHeight;

                return tr.Start.Parent as InlineUIContainer;
            }

            // return new InlineUIContainer(img,tr.Start);
        }
        
        public static FlowDocument ToImageDocument(this string base64Str, Size? docSize = null) {
            if (!base64Str.IsStringBase64()) {
                Debugger.Break();
                return string.Empty.ToFlowDocument();
            }

            //BitmapSource bmpSrc = base64Str.ToBitmapSource();

            //var img = new Image() {
            //    Source = bmpSrc,
            //    Width = bmpSrc.Width,
            //    Height = bmpSrc.Height,
            //    Stretch = System.Windows.Media.Stretch.Uniform
            //};

            var fd = string.Empty.ToFlowDocument();
            var p = fd.Blocks.FirstBlock as Paragraph;
            p.ContentRange().LoadImage(base64Str, docSize);

            fd.ConfigureLineHeight();
            p.ContentRange().ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Center);

            return fd;
        }

        public static Paragraph LoadFileItem(this TextRange tr, string path, int iconId = 0, double iconSize = 16) {
            string iconBase64 = string.Empty;

            if (iconId > 0 && path.IsFileOrDirectory()) {
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

            var iuc = new InlineUIContainer(pathIcon) {
                BaselineAlignment = BaselineAlignment.Bottom
            };
            
            Paragraph p = new Paragraph(iuc) { 
                Margin = new Thickness(iconSize, 0, 0, 0),
                TextIndent = -iconSize
            };

            ToolTipService.SetInitialShowDelay(p, 300);

            string pathDir = path;
            if (File.Exists(pathDir)) {
                pathDir = Path.GetDirectoryName(pathDir);
            }

            var pathRun = new Run(Path.GetFileName(path));
            var pathLink = new Hyperlink(pathRun) {
                IsEnabled = true,
                NavigateUri = new Uri(pathDir, UriKind.Absolute)
            };

            p.Inlines.Add(new Run(" "));
            p.Inlines.Add(pathLink);

            double fontSize = 16;
            p.FontFamily = new FontFamily(MpPreferences.DefaultFontFamily);
            p.FontSize = fontSize;


            using (var stream = new MemoryStream()) {
                var subDoc = new FlowDocument();
                subDoc.Blocks.Add(p);
                p.ContentRange().Save(stream, DataFormats.XamlPackage, true);
                stream.Seek(0, SeekOrigin.Begin);
                tr.Load(stream, DataFormats.XamlPackage);

                var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();

                var hl = tr.GetAllTextElements()
                           .FirstOrDefault(x => x is Hyperlink) as Hyperlink;

                var fileItemParagraph = tr.GetAllTextElements().FirstOrDefault(x => x is Paragraph) as Paragraph;
                

                fileItemParagraph.TextAlignment = TextAlignment.Left;
                fileItemParagraph.Padding = new Thickness(3);

                RequestNavigateEventHandler hl_nav_handler = (s, e) => {
                    System.Diagnostics.Process.Start(e.Uri.ToString());
                };
                MouseButtonEventHandler hl_mouseLeftButtonUp_handler = (s, e) => {
                    Process.Start(hl.NavigateUri.ToString());
                };

                RoutedEventHandler hl_Unload_handler = null;
                hl_Unload_handler = (s, e) => {
                    hl.RequestNavigate -= hl_nav_handler;
                    hl.Unloaded -= hl_Unload_handler;
                    hl.MouseLeftButtonUp -= hl_mouseLeftButtonUp_handler;
                };

                MouseEventHandler p_mouseEnter_handler = (s, e) => {
                    if (MpDragDropManager.IsDragAndDrop) {
                        return;
                    }
                    fileItemParagraph.Background = Brushes.Gainsboro;
                    fileItemParagraph.BorderBrush = Brushes.Black;
                    fileItemParagraph.BorderThickness = new Thickness(0.5);

                    //var ctvm = MpClipTrayViewModel.Instance.Items.FirstOrDefault(x => x.CopyItemData.Contains(path));
                    var ctvm = tr.Start.Parent.FindParentOfType<FlowDocument>().DataContext as MpClipTileViewModel;
                    if (ctvm == null) {
                        Debugger.Break();
                    } else {
                        ctvm.IsHovering = true;

                        hl.IsEnabled = true;

                        rtb.ToolTip = path;

                        var selectionRange = fileItemParagraph.ContentRange();
                        if(!rtb.Document.ContentRange().IsRangeInSameDocument(selectionRange)) {
                            Debugger.Break();
                            return;
                        }
                        rtb.Selection.Select(selectionRange.Start, selectionRange.End);
                        //MpConsole.WriteLine("Hover ItemData: " + civm.Parent.HoverItem.CopyItemData);
                    }
                };
                MouseEventHandler p_mouseLeave_handler = (s, e) => {
                    if(MpDragDropManager.IsDragAndDrop) {
                        return;
                    }
                    fileItemParagraph.Background = Brushes.Transparent;
                    fileItemParagraph.BorderBrush = Brushes.Transparent;
                    fileItemParagraph.BorderThickness = new Thickness(0);

                    //var ctvm = MpClipTrayViewModel.Instance.Items.FirstOrDefault(x => x.CopyItemData.Contains(path));
                    var ctvm = tr.Start.Parent.FindParentOfType<FlowDocument>().DataContext as MpClipTileViewModel;
                    if (ctvm == null) {
                        Debugger.Break();
                    } else {
                        ctvm.IsHovering = false;
                        rtb.ToolTip = null;

                        hl.IsEnabled = false;

                        var selectionRange = fileItemParagraph.ContentStart.ToTextRange();
                        if (!rtb.Document.ContentRange().IsRangeInSameDocument(selectionRange)) {
                            Debugger.Break();
                            return;
                        }
                        rtb.Selection.Select(selectionRange.Start, selectionRange.End);
                    }
                };

                RoutedEventHandler p_Unload_handler = null;

                p_Unload_handler = (s, e) => {
                    fileItemParagraph.MouseEnter -= p_mouseEnter_handler;
                    fileItemParagraph.MouseLeave -= p_mouseLeave_handler;
                    fileItemParagraph.Unloaded -= p_Unload_handler;
                };

                hl.RequestNavigate += hl_nav_handler;
                hl.Unloaded += hl_Unload_handler;
                hl.MouseLeftButtonUp += hl_mouseLeftButtonUp_handler;

                fileItemParagraph.MouseEnter += p_mouseEnter_handler;
                fileItemParagraph.MouseLeave += p_mouseLeave_handler;

                fileItemParagraph.Unloaded += p_Unload_handler;

                return fileItemParagraph;
            }

            //MpContentItemViewModel civm = null;
            //var fce = tr.Start.Parent as FrameworkContentElement;
            //if (fce.DataContext is MpClipTileViewModel ctvm) {
            //    civm = ctvm.Items.FirstOrDefault(x => x.CopyItemData == path);

            //    if (civm == null) {
            //        Debugger.Break();
            //    }
            //} else if (fce.DataContext is MpContentItemViewModel) {
            //    civm = (fce.DataContext as MpContentItemViewModel).Parent.Items.FirstOrDefault(x => x.CopyItemData == path);
            //} else {
            //    Debugger.Break();
            //}
            //var fileItemParagraph = new MpFileItemParagraph();
            //fileItemParagraph.DataContext = civm;



            //var pToReplace = tr.GetAllTextElements().FirstOrDefault(x => x is Paragraph) as Paragraph;
            //var fd = tr.Start.Parent.FindParentOfType<FlowDocument>();
            //if (pToReplace == null) {
            //    fd.Blocks.Add(fileItemParagraph);
            //} 
            //else {
            //    //fd.Blocks.InsertAfter(pToReplace, fileItemParagraph);
            //    //fd.Blocks.Remove(pToReplace);
            //    pToReplace = fileItemParagraph;
            //    pToReplace.DataContext = civm;
            //    return pToReplace;
            //    //using (var subStream = new MemoryStream()) {
            //    //    var subDoc = new FlowDocument();
            //    //    var tfip = new MpFileItemParagraph() { DataContext = civm };
            //    //    subDoc.Blocks.Add(tfip);
            //    //    tfip.ContentRange().Save(subStream, DataFormats.XamlPackage, true);
            //    //    subStream.Seek(0, SeekOrigin.Begin);
            //    //    tr.Load(subStream, DataFormats.XamlPackage);
            //    //}
            //}
            //return fileItemParagraph;
        }

        public static FlowDocument ToFilePathDocument(this string path, int iconId = 0, double iconSize = 16) {
            var fd = string.Empty.ToFlowDocument();
            fd.Blocks.FirstBlock.ContentRange().LoadFileItem(path, iconId, iconSize);
            return fd;
        }
        public static void LoadRtf(this TextRange tr, string str) {
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(str))) {
                try {
                    using (var subStream = new MemoryStream()) {
                        var subDoc = new FlowDocument();
                        var sub_tr = new TextRange(subDoc.ContentStart, subDoc.ContentEnd);
                        sub_tr.Load(stream, DataFormats.Rtf);
                        sub_tr.Save(subStream, DataFormats.Rtf);
                        subStream.Seek(0, SeekOrigin.Begin);
                        tr.Load(subStream, DataFormats.Rtf);
                    }
                    var rtbAlignment = tr.GetPropertyValue(FlowDocument.TextAlignmentProperty);
                    if (rtbAlignment == null || rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}") {
                        //ignore to tr
                    } else if ((TextAlignment)rtbAlignment == TextAlignment.Justify) {
                        tr.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteLine("Exception converting richtext to flowdocument, attempting to fall back to plaintext...", ex);
                    return;
                }
            }
        }

        public static void LoadTextTemplate(this TextRange tr, MpTextTemplate cit) {
            // TODO (maybe) when cit is new (and no formatting is stored) need to check font formatting of beginning of tr
            // and clone (and also switch to FLowDocument if a list or something) here
            // because as long as this done on initial creation the formatting will persist (or is loaded from cit)
            var tr_parent_te = tr.Start.Parent as TextElement;
            var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();

            if(tr_parent_te == null) {
                tr_parent_te = tr.Start.Paragraph;
            }

            tr.Text = string.Empty;
            //var r = new Run(cit.TemplateName) {
            //    Tag = cit
            //};

            var tb = new TextBlock() {
                Text = cit.TemplateName,
                FontSize = tr_parent_te.FontSize,
                FontFamily = tr_parent_te.FontFamily,
                FontStretch = tr_parent_te.FontStretch,
                FontWeight = tr_parent_te.FontWeight,
                FontStyle = tr_parent_te.FontStyle,
                Background = Brushes.Transparent,
                Tag = cit
            };
            var b = new Border() {
                Child = tb,
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(1),
                Tag = cit
            };
            
            var iuic = new InlineUIContainer(b,tr.Start) {
                Tag = cit,            
                BaselineAlignment = BaselineAlignment.Center
            };

            MpHelpers.RunOnMainThread(async () => {
                while(!b.IsLoaded) {
                    await Task.Delay(100);
                }
                if (iuic.ContentStart.Paragraph != null) {
                    if (iuic.ContentStart.Paragraph.LineHeight < b.ActualHeight) {
                        iuic.ContentStart.Paragraph.LineHeight = b.ActualHeight;
                    }
                }
            });
            

            //var thl = new Hyperlink(tr.Start,tr.End) {
            //    Tag = cit,
            //    TextDecorations = null
            //}; 
            //thl.Inlines.Clear();
            //thl.Inlines.Add(iuic);

            MpClipTileViewModel ctvm = MpClipTrayViewModel.Instance.GetClipTileViewModelById(cit.CopyItemId);
            if(ctvm == null) {
                Debugger.Break();
            }

            MpTextTemplateViewModel tvm = ctvm.TemplateCollection.Items.FirstOrDefault(x => x.TextTemplateId == cit.Id);

            #region Events

            MouseEventHandler thl_mouseEnter_handler = (s, e) => {
                tvm.IsHovering = true;
                if (!ctvm.IsContentReadOnly) {
                    MpCursor.SetCursor(tvm, MpCursorType.Hand);
                }
            };

            MouseEventHandler thl_mouseLeave_handler = (s, e) => {
                tvm.IsHovering = false;
                if (!ctvm.IsContentReadOnly) {
                    MpCursor.UnsetCursor(tvm);
                }
            };

            MouseButtonEventHandler thl_previewMouseLeftButtonDown_handler = (s, e) => {
                if(!tvm.Parent.Parent.IsSelected) {
                    tvm.Parent.Parent.IsSelected = true;
                }
                if(!ctvm.IsContentReadOnly) {
                    tvm.Parent.SelectedItem = tvm;
                    if(!ctvm.IsPasting) {
                        tvm.EditTemplateCommand.Execute(null);
                        e.Handled = true;
                    }
                }
            };

            MouseButtonEventHandler thl_previewMouseRightButtonDown_handler = (s, e) => {
                if (!tvm.Parent.Parent.IsSelected) {
                    tvm.Parent.Parent.IsSelected = true;
                }
                if (!ctvm.IsContentReadOnly) {
                    tvm.Parent.SelectedItem = tvm;
                    if (!ctvm.IsPasting) {
                        var origin = tr.Start.GetCharacterRect(LogicalDirection.Forward).Location;
                        origin = iuic.FindParentOfType<RichTextBox>().TranslatePoint(origin, Application.Current.MainWindow);
                        
                        MpContextMenuView.Instance.DataContext = tvm.MenuItemViewModel;
                        //MpContextMenuView.Instance.PlacementRectangle = new Rect(origin,new Size(200,50));

                        iuic.ContextMenu = MpContextMenuView.Instance;
                        MpContextMenuView.Instance.IsOpen = true;
                    }
                    e.Handled = true;
                }
            };


            RoutedEventHandler thl_unloaded_handler = null;
            thl_unloaded_handler = (s, e) => {
                MpHelpers.RunOnMainThread(async () => {
                    if (rtb != null && 
                        !rtb.IsReadOnly && 
                        !ctvm.IsPastingTemplate &&
                        Mouse.LeftButton == MouseButtonState.Released &&
                        !MpClipTrayViewModel.Instance.HasScrollVelocity && 
                        !MpClipTrayViewModel.Instance.IsRequery) {
                        //while editing if template is removed check if its the only one if so remove from db and tcvm
                        var iuicl = rtb.Document.GetAllTextElements().Where(x => x is InlineUIContainer && x.Tag is MpTextTemplate);
                        if (iuicl != null && iuicl.All(x => (x.Tag as MpTextTemplate).Id != cit.Id)) {
                            var tcvm = (rtb.DataContext as MpClipTileViewModel).TemplateCollection;
                            var toRemove_tvml = tcvm.Items.Where(x => x.TextTemplateGuid == cit.Guid).ToList();
                            foreach (var toRemove_tvm in toRemove_tvml) {
                                MpConsole.WriteTraceLine($"Template {toRemove_tvm} unloaded in delete state, so its gone now.");
                                tcvm.Items.Remove(toRemove_tvm);
                            }
                            await Task.WhenAll(toRemove_tvml.Select(x => x.TextTemplate.DeleteFromDatabaseAsync()));

                            tcvm.OnPropertyChanged(nameof(tcvm.Items));
                        }
                    } 
                });
                
                iuic.Unloaded -= thl_unloaded_handler;
                iuic.MouseEnter -= thl_mouseEnter_handler;
                iuic.MouseLeave -= thl_mouseLeave_handler;
                iuic.PreviewMouseLeftButtonDown -= thl_previewMouseLeftButtonDown_handler;
                iuic.PreviewMouseRightButtonDown -= thl_previewMouseRightButtonDown_handler;
            };

            iuic.Unloaded += thl_unloaded_handler;
            iuic.MouseEnter += thl_mouseEnter_handler;
            iuic.MouseLeave += thl_mouseLeave_handler;
            iuic.PreviewMouseLeftButtonDown += thl_previewMouseLeftButtonDown_handler;
            iuic.PreviewMouseRightButtonDown += thl_previewMouseRightButtonDown_handler;
            #endregion

            #region Bindings

            MpHelpers.CreateBinding(
                   source: tvm,
                   sourceProperty: new PropertyPath(
                                        nameof(tvm.TemplateDisplayValue)),
                   target: tb,
                   targetProperty: TextBlock.TextProperty);

            MpHelpers.CreateBinding(
                   source: tvm,
                   sourceProperty: new PropertyPath(
                                        nameof(tvm.TemplateForegroundHexColor)),
                   target: tb,
                   targetProperty: TextBlock.ForegroundProperty,
                   converter: new MpStringHexToBrushConverter());

            MpHelpers.CreateBinding(
                   source: tvm,
                   sourceProperty: new PropertyPath(
                                        nameof(tvm.TemplateBackgroundHexColor)),
                   target: b,
                   targetProperty: Border.BackgroundProperty,
                   converter: new MpStringHexToBrushConverter());

            MpHelpers.CreateBinding(
                   source: tvm,
                   sourceProperty: new PropertyPath(
                                        nameof(tvm.TemplateBorderHexColor)),
                   target: b,
                   targetProperty: Border.BorderBrushProperty,
                   converter: new MpStringHexToBrushConverter());

            #endregion
        }
        public static void LoadItemData(this TextRange tr, string str, MpCopyItemType strItemDataType, int iconId = 0) {
            // NOTE iconId is only used to convert file path's to rtf w/ icon 

            if (string.IsNullOrEmpty(str)) {
                tr.Text = str;
                return;
            }
            switch(strItemDataType) {
                case MpCopyItemType.Text:
                    tr.LoadRtf(str);
                    break;
                case MpCopyItemType.Image:
                    tr.LoadImage(str);
                    break;
                case MpCopyItemType.FileList:
                    var ctp = tr.Start.GetInsertionPosition(LogicalDirection.Forward);
                    foreach(var fip in str.Split(new string[] {Environment.NewLine},StringSplitOptions.RemoveEmptyEntries)) {
                        if(ctp == null) {
                            ctp = tr.Start.DocumentEnd;
                        }
                        ctp = ctp.InsertParagraphBreak();
                        var p = ctp.Parent.FindParentOfType<Paragraph>();
                        var pcr = p.ContentRange();
                        var new_p = pcr.LoadFileItem(fip);

                        ctp = new_p.ContentEnd.GetNextInsertionPosition(LogicalDirection.Forward);
                    }
                    break;
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
            // NOTE this always adds a trailing line break so remove last two characters
            return new TextRange(fd.ContentStart, fd.ContentEnd).Text.RemoveLastLineEnding();
            
        }

        public static string ToPlainText(this TextElement te) {
            // NOTE this always adds a trailing line break so remove last two characters

            if (te == null) {
                return string.Empty;
            }
            return new TextRange(te.ContentStart, te.ContentEnd).Text.RemoveLastLineEnding();
        }

        public static FlowDocument Combine(this FlowDocument fd, FlowDocument ofd, TextPointer insertPointer = null, bool insertNewline = true) {
            return CombineFlowDocuments(ofd, fd, insertPointer, insertNewline);
        }

        public static FlowDocument Combine(this FlowDocument fd, string text, TextPointer insertPointer = null, bool insertNewline = true) {
            return CombineFlowDocuments(text.ToRichText().ToFlowDocument(), fd, insertPointer, insertNewline);
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
                start = matchRange.End.GetNextInsertionPosition(LogicalDirection.Forward);
                yield return matchRange;
            }

            //return matchRangeList;
        }
        public static List<TextRange> FindText(
            this FlowDocument fd,
            string input,
            bool isCaseSensitive = false,
            bool matchWholeWord = false,
            bool useRegEx = false) {
            
            input = input.Replace(Environment.NewLine, string.Empty);


            if(matchWholeWord || useRegEx) {
                string pattern;
                if(useRegEx) {
                    pattern = input;
                } else {
                    pattern = $"\b{input}\b";
                }
                string pt = fd.ToPlainText();
                var mc = Regex.Matches(pt, pattern, isCaseSensitive ? RegexOptions.None:RegexOptions.IgnoreCase);

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
                if(useRegEx && matchWholeWord) {
                    trl = trl.Where(x => Regex.IsMatch(x.Text, $"\b{x.Text}\b")).ToList();
                }
                return trl;
            }

            return fd.ContentStart.FindAllText(fd.ContentEnd, input, isCaseSensitive).ToList();
        }

        public static List<TextRange> FindText(
            this FlowDocument fd,
            string input,
            FindFlags flags = FindFlags.MatchCase,
            CultureInfo cultureInfo = null) {
            input = input.Replace(Environment.NewLine, string.Empty);
            return fd.ContentStart.FindAllText(fd.ContentEnd, input, flags.HasFlag(FindFlags.MatchCase)).ToList();
            var trl = new List<TextRange>();
            //var tp = fd.ContentStart;

            //var inputParts = input.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            //while (tp != null && tp != fd.ContentEnd) {
            //    var ctp = tp;
            //    tp = null;
            //    int i;
            //    for(i = 0;i < inputParts.Length && ctp != null && ctp != fd.ContentEnd;i++) {
            //        string inputPart = inputParts[i];
            //        var tr = ctp.FindText(fd.ContentEnd, inputPart, flags, cultureInfo);
            //        if (tr == null) {
            //            break;
            //        }
            //        if(tp == null) {
            //            tp = tr.Start;
            //        }
            //        ctp = tr.End.GetNextInsertionPosition(LogicalDirection.Forward);
            //    }
            //    if(i != inputParts.Length) {
            //        break;
            //    }
            //    trl.Add(new TextRange(tp, ctp));

            //    tp = ctp.GetNextInsertionPosition(LogicalDirection.Forward);
            //}
            return trl;
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

        public static void ConfigureLineHeight2(
            this FlowDocument doc, 
            LineStackingStrategy lss = LineStackingStrategy.MaxHeight,
            double linePad = 2) {
            if(doc.DataContext is MpClipTileViewModel ctvm && ctvm.ItemType == MpCopyItemType.Image) {
                doc.LineStackingStrategy = LineStackingStrategy.MaxHeight;
                return;
            }
            doc.LineStackingStrategy = lss;

            var ctp = doc.ContentStart;
            foreach (var b in doc.Blocks) {
                b.LineStackingStrategy = lss;
                if (b is Paragraph p) {
                    p.Margin = new Thickness(0);
                    p.LineHeight = p.FontSize + linePad;// (p.FontSize * 0.333);
                    doc.LineHeight = p.LineHeight; 
                }
            }
        }

        public static void ConfigureLineHeight(
           this FlowDocument doc) {
            doc.LineStackingStrategy = LineStackingStrategy.MaxHeight;
            doc.LineHeight = Double.NaN;
            foreach (var b in doc.Blocks) {
                b.LineStackingStrategy = LineStackingStrategy.MaxHeight;
                b.LineHeight = Double.NaN;
            }
            if(doc.Parent is FrameworkElement fe) {
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

        public static FlowDocument CombineFlowDocuments(
            FlowDocument from, 
            FlowDocument to, 
            TextPointer toInsertPointer = null, bool insertNewLine = false) {
            toInsertPointer = toInsertPointer == null ? to.ContentEnd : toInsertPointer;

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

                var rangeTo = new TextRange(toInsertPointer, toInsertPointer);
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
            Size? docSize = null, 
            Brush bgBrush = null) {
            var size = docSize.HasValue ? docSize.Value : document.GetDocumentSize();
            bgBrush = bgBrush == null ? Brushes.White : bgBrush;

            if (size.Width <= 0) {
                size.Width = 1;
            }
            if (size.Height <= 0) {
                size.Height = 1;
            }
            var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow);
            size.Width *= dpi.DpiScaleX;
            size.Height *= dpi.DpiScaleY;

            document.PagePadding = new Thickness(0);
            document.ColumnWidth = size.Width;
            document.PageWidth = size.Width;
            document.PageHeight = size.Height;

            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            paginator.PageSize = size;

            var visual = new DrawingVisual();
            using (var drawingContext = visual.RenderOpen()) {
                // draw white background
                drawingContext.DrawRectangle(bgBrush ?? Brushes.White, null, new Rect(size));
            }
            visual.Children.Add(paginator.GetPage(0).Visual);
            var bitmap = new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                dpi.PixelsPerInchX,
                dpi.PixelsPerInchY,
                PixelFormats.Pbgra32);

            bitmap.Render(visual);
            RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.HighQuality);
            return bitmap;
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
    }
}
