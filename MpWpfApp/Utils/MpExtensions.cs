using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MpWpfApp {
    public static class MpExtensions {
        #region Collections
        public static void Move<T>(this IList<T> collection, int oldIdx, int newIdx) where T : class {
            var item = collection[oldIdx];
            collection.RemoveAt(oldIdx);
            collection.Insert(newIdx, item);
        }

        public static void Sort<TSource, TKey>(
            this MpObservableCollection<TSource> source,
            Func<TSource, TKey> keySelector,
            bool desc = false) where TSource : class {
            if (source == null) {
                return;
            }
            Comparer<TKey> comparer = Comparer<TKey>.Default;

            for (int i = source.Count - 1; i >= 0; i--) {
                for (int j = 1; j <= i; j++) {
                    TSource o1 = source[j - 1];
                    TSource o2 = source[j];
                    int comparison = comparer.Compare(keySelector(o1), keySelector(o2));
                    if (desc && comparison < 0) {
                        source.Move(j, j - 1);
                    } else if (!desc && comparison > 0) {
                        source.Move(j - 1, j);
                    }
                }
            }
        }
        #endregion

        #region Visual Tree
        public static bool IsVisualDescendant(this DependencyObject parent, DependencyObject child) {
            if(parent == null || child == null) {
                return false;
            }
            foreach(var descendant in parent.FindChildren<UIElement>()) {
                if(descendant == child) {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<T> FindChildren<T>(this DependencyObject source)
                                             where T : DependencyObject {
            if (source != null) {
                var childs = GetChildObjects(source);
                foreach (DependencyObject child in childs) {
                    //analyze if children match the requested type
                    if (child != null && child is T) {
                        yield return (T)child;
                    }

                    //recurse tree
                    foreach (T descendant in FindChildren<T>(child)) {
                        yield return descendant;
                    }
                }
            }
        }


        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetChild"/> method, which also
        /// supports content elements. Do note, that for content elements,
        /// this method falls back to the logical tree of the element.
        /// </summary>
        /// <param name="parent">The item to be processed.</param>
        /// <returns>The submitted item's child elements, if available.</returns>
        public static IEnumerable<DependencyObject> GetChildObjects(
                                                    this DependencyObject parent) {
            if (parent == null) yield break;


            if (parent is ContentElement || parent is FrameworkElement) {
                //use the logical tree for content / framework elements
                foreach (object obj in LogicalTreeHelper.GetChildren(parent)) {
                    var depObj = obj as DependencyObject;
                    if (depObj != null) yield return (DependencyObject)obj;
                }
            } else {
                //use the visual tree per default
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++) {
                    yield return VisualTreeHelper.GetChild(parent, i);
                }
            }
        }
        public static T FindParentOfType<T>(this DependencyObject dpo) where T : class {
            if (dpo == null) {
                return default;
            }
            if (dpo.GetType() == typeof(T)) {
                return (dpo as T);
            }
            if (dpo.GetType().IsSubclassOf(typeof(FrameworkContentElement))) {
                if(((FrameworkContentElement)dpo).Parent != null) {
                    return FindParentOfType<T>(((FrameworkContentElement)dpo).Parent);
                } 
                if(((FrameworkContentElement)dpo).TemplatedParent != null) {
                    return FindParentOfType<T>(((FrameworkContentElement)dpo).TemplatedParent);
                }
                
            } else if (dpo.GetType().IsSubclassOf(typeof(FrameworkElement))) {
                if (((FrameworkElement)dpo).Parent != null) {
                    return FindParentOfType<T>(((FrameworkElement)dpo).Parent);
                } 
                if (((FrameworkElement)dpo).TemplatedParent != null) {
                    return FindParentOfType<T>(((FrameworkElement)dpo).TemplatedParent);
                }
            }

            return null;
        }
        public static T FindParentDataContextWithType<T>(this DependencyObject dpo) where T : class {
            if (dpo == null) {
                return default;
            }
            if (dpo.GetType().IsSubclassOf(typeof(FrameworkContentElement)) &&
                ((FrameworkContentElement)dpo).DataContext != null &&
                ((FrameworkContentElement)dpo).DataContext.GetType() == typeof(T)) {
                return (((FrameworkContentElement)dpo).DataContext as T);
            }

            if (dpo.GetType().IsSubclassOf(typeof(FrameworkElement)) && 
                ((FrameworkElement)dpo).DataContext != null  &&
                ((FrameworkElement)dpo).DataContext.GetType() == typeof(T)) {
                return (((FrameworkElement)dpo).DataContext as T);
            }

            if (dpo.GetType().IsSubclassOf(typeof(FrameworkContentElement))) {
                if (((FrameworkContentElement)dpo).Parent != null) {
                    return FindParentOfType<T>(((FrameworkContentElement)dpo).Parent);
                } 
                if (((FrameworkContentElement)dpo).TemplatedParent != null) {
                    return FindParentOfType<T>(((FrameworkContentElement)dpo).TemplatedParent);
                }

            } else if (dpo.GetType().IsSubclassOf(typeof(FrameworkElement))) {
                if (((FrameworkElement)dpo).Parent != null) {
                    return FindParentOfType<T>(((FrameworkElement)dpo).Parent);
                } 
                if (((FrameworkElement)dpo).TemplatedParent != null) {
                    return FindParentOfType<T>(((FrameworkElement)dpo).TemplatedParent);
                }
            }
            return null;
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
        #endregion

        #region Documents
        public static BitmapSource ToBitmapSource(this FlowDocument fd, Brush bgBrush = null) {
            return MpHelpers.Instance.ConvertFlowDocumentToBitmap(
                                fd.Clone(),
                                fd.GetDocumentSize(),
                                bgBrush);
        }

        public static bool Equals(this TextRange tra, TextRange trb) {
            if(!tra.Start.IsInSameDocument(trb.Start)) {
                return false;
            }
            if (tra.Start.CompareTo(trb.Start) == 0 && tra.End.CompareTo(trb.End) == 0) {
                return true;
            }
            return false;
        }

        //public static List<Hyperlink> GetHyperlinkList(this RichTextBox rtb) {
        //    var hlList = new List<Hyperlink>();
        //    for (TextPointer position = rtb.Document.ContentStart;
        //        position != null && position.CompareTo(rtb.Document.ContentEnd) <= 0;
        //        position = position.GetNextContextPosition(LogicalDirection.Forward)) {
        //        if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
        //            var hl = MpHelpers.Instance.FindParentOfType(position.Parent, typeof(Hyperlink)) as Hyperlink;
        //            if (hl != null && !hlList.Contains(hl)) {
        //                hlList.Add(hl);
        //            }
        //        }
        //    }
        //    return hlList;
        //}

        //public static void ClearHyperlinks(this RichTextBox rtb, bool ignoreTemplates = false) {
        //    //replaces hyperlinks with spans of there textrange text
        //    var rtbvm = (MpClipTileRichTextBoxViewModel)rtb.DataContext;
        //    var ctvm = rtbvm.ClipTileViewModel;
        //    var hlList = rtb.GetHyperlinkList();
        //    foreach (var hl in hlList) {
        //        string linkText = string.Empty;
        //        if (hl.DataContext == null || hl.DataContext is MpClipTileViewModel) {
        //            linkText = new TextRange(hl.ElementStart, hl.ElementEnd).Text;
        //        } else if(!ignoreTemplates) {
        //            var thlvm = (MpTemplateHyperlinkViewModel)hl.DataContext;
        //            linkText = thlvm.TemplateName;
        //        }
        //        hl.Inlines.Clear();
        //        new Span(new Run(linkText), hl.ContentStart);
        //    }
        //    if(!ignoreTemplates) {
        //        ctvm.TemplateHyperlinkCollectionViewModel.Clear();
        //    }
        //}

        //public static void CreateHyperlinks(this RichTextBox rtb, bool ignoreTemplates = false) {
        //    var rtbvm = (MpClipTileRichTextBoxViewModel)rtb.DataContext;
        //    var ctvm = rtbvm.ClipTileViewModel;
        //    var regExGroupList = new List<string> {
        //        //WebLink
        //        @"(?:https?://|www\.)\S+", 
        //        //Email
        //        @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})",
        //        //PhoneNumber
        //        @"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})",
        //        //Currency
        //        @"[$|£|€|¥][\d|\.]([0-9]{0,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.\d{0,2})?",
        //        //HexColor (no alpha)
        //        @"#([0-9]|[a-fA-F]){5}([^" + Properties.Settings.Default.TemplateTokenMarker + "][ ])",
        //        //StreetAddress
        //        @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b",                
        //        //Text Template (dynamically matching from CopyItemTemplate.TemplateName)
        //        ctvm.CopyItem.TemplateRegExMatchString,                
        //        //HexColor (with alpha)
        //        @"#([0-9]|[a-fA-F]){7}([^" + Properties.Settings.Default.TemplateTokenMarker + "][ ])",
        //    };
        //    var docPlainText = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
        //    for (int i = 0; i < regExGroupList.Count; i++) {
        //        var linkType = i + 1 > (int)MpSubTextTokenType.TemplateSegment ? MpSubTextTokenType.HexColor : (MpSubTextTokenType)(i + 1);
        //        if(linkType == MpSubTextTokenType.TemplateSegment && ignoreTemplates) {
        //            continue;
        //        }
        //        if (linkType == MpSubTextTokenType.StreetAddress) {
        //            //doesn't consistently work and presents bugs so disabling for now
        //            continue;
        //        }
        //        var lastRangeEnd = rtb.Document.ContentStart;
        //        var regExStr = regExGroupList[i];
        //        if (string.IsNullOrEmpty(regExStr)) {
        //            //this occurs for templates when copyitem has no templates
        //            continue;
        //        }
        //        var mc = Regex.Matches(docPlainText, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
        //        foreach (Match m in mc) {
        //            foreach (Group mg in m.Groups) {
        //                foreach (Capture c in mg.Captures) {
        //                    Hyperlink hl = null;
        //                    var matchRange = MpHelpers.Instance.FindStringRangeFromPosition(lastRangeEnd, c.Value, true);
        //                    if (matchRange == null) {
        //                        continue;
        //                    }
        //                    lastRangeEnd = matchRange.End;
        //                    if (linkType == MpSubTextTokenType.TemplateSegment) {
        //                        var copyItemTemplate = ctvm.CopyItem.GetTemplateByName(matchRange.Text);
        //                        var thlvm = new MpTemplateHyperlinkViewModel(ctvm, copyItemTemplate);
        //                        hl =  MpHelpers.Instance.CreateTemplateHyperlink(thlvm, matchRange);
        //                        ctvm.TemplateHyperlinkCollectionViewModel.Add(thlvm);
        //                    } else {
        //                        matchRange.Text = matchRange.Text;
        //                        hl = new Hyperlink(matchRange.Start, matchRange.End);
        //                        var linkText = c.Value;
        //                        hl.Tag = linkType;
        //                        MpHelpers.Instance.CreateBinding(ctvm, new PropertyPath(nameof(ctvm.IsSelected)), hl, Hyperlink.IsEnabledProperty);
        //                        hl.MouseEnter += (s3, e3) => {
        //                            hl.Cursor = ctvm.IsSelected ? Cursors.Hand : Cursors.Arrow;
        //                        };
        //                        hl.MouseLeave += (s3, e3) => {
        //                            hl.Cursor = Cursors.Arrow;
        //                        };
        //                        hl.MouseLeftButtonDown += (s4, e4) => {
        //                            if (hl.NavigateUri != null && ctvm.IsSelected) {
        //                                MpHelpers.Instance.OpenUrl(hl.NavigateUri.ToString());
        //                            }
        //                        };

        //                        MenuItem convertToQrCodeMenuItem = new MenuItem();
        //                        convertToQrCodeMenuItem.Header = "Convert to QR Code";
        //                        convertToQrCodeMenuItem.Click += async (s5, e1) => {
        //                            var hyperLink = (Hyperlink)((MenuItem)s5).Tag;
        //                            var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(hyperLink.NavigateUri.ToString());
        //                            Clipboard.SetImage(bmpSrc);
        //                        };
        //                        convertToQrCodeMenuItem.Tag = hl;
        //                        hl.ContextMenu = new ContextMenu();
        //                        hl.ContextMenu.Items.Add(convertToQrCodeMenuItem);

        //                        switch ((MpSubTextTokenType)hl.Tag) {
        //                            case MpSubTextTokenType.StreetAddress:
        //                                hl.NavigateUri = new Uri("https://google.com/maps/place/" + linkText.Replace(' ', '+'));
        //                                break;
        //                            case MpSubTextTokenType.Uri:
        //                                if (!linkText.Contains("https://")) {
        //                                    hl.NavigateUri = new Uri("https://" + linkText);
        //                                } else {
        //                                    hl.NavigateUri = new Uri(linkText);
        //                                }
        //                                MenuItem minifyUrl = new MenuItem();
        //                                minifyUrl.Header = "Minify with bit.ly";
        //                                minifyUrl.Click += async (s1, e2) => {
        //                                    Hyperlink link = (Hyperlink)((MenuItem)s1).Tag;
        //                                    string minifiedLink = await MpMinifyUrl.Instance.ShortenUrl(link.NavigateUri.ToString());
        //                                    if (!string.IsNullOrEmpty(minifiedLink)) {
        //                                        matchRange.Text = minifiedLink;
        //                                        rtb.ClearHyperlinks();
        //                                        rtb.CreateHyperlinks();
        //                                    }
        //                                    //Clipboard.SetText(minifiedLink);
        //                                };
        //                                minifyUrl.Tag = hl;
        //                                hl.ContextMenu.Items.Add(minifyUrl);
        //                                break;
        //                            case MpSubTextTokenType.Email:
        //                                hl.NavigateUri = new Uri("mailto:" + linkText);
        //                                break;
        //                            case MpSubTextTokenType.PhoneNumber:
        //                                hl.NavigateUri = new Uri("tel:" + linkText);
        //                                break;
        //                            case MpSubTextTokenType.Currency:
        //                                //"https://www.google.com/search?q=%24500.80+to+yen"
        //                                MenuItem convertCurrencyMenuItem = new MenuItem();
        //                                convertCurrencyMenuItem.Header = "Convert Currency To";
        //                                var fromCurrencyType = MpHelpers.Instance.GetCurrencyTypeFromString(linkText);
        //                                foreach (MpCurrency currency in MpCurrencyConverter.Instance.CurrencyList) {
        //                                    if (currency.Id == Enum.GetName(typeof(CurrencyType), fromCurrencyType)) {
        //                                        continue;
        //                                    }
        //                                    MenuItem subItem = new MenuItem();
        //                                    subItem.Header = currency.CurrencyName + "(" + currency.CurrencySymbol + ")";
        //                                    subItem.Click += async (s2, e2) => {
        //                                        Enum.TryParse(currency.Id, out CurrencyType toCurrencyType);
        //                                        var convertedValue = await MpCurrencyConverter.Instance.ConvertAsync(
        //                                            MpHelpers.Instance.GetCurrencyValueFromString(linkText),
        //                                            fromCurrencyType,
        //                                            toCurrencyType);
        //                                        convertedValue = Math.Round(convertedValue, 2);
        //                                        if (rtb.Tag != null && ((List<Hyperlink>)rtb.Tag).Contains(hl)) {
        //                                            ((List<Hyperlink>)rtb.Tag).Remove(hl);
        //                                        }
        //                                        Run run = new Run(currency.CurrencySymbol + convertedValue);
        //                                        hl.Inlines.Clear();
        //                                        hl.Inlines.Add(run);
        //                                    };

        //                                    convertCurrencyMenuItem.Items.Add(subItem);
        //                                }

        //                                hl.ContextMenu.Items.Add(convertCurrencyMenuItem);
        //                                break;
        //                            case MpSubTextTokenType.HexColor:
        //                                var rgbColorStr = linkText;
        //                                if (rgbColorStr.Length > 7) {
        //                                    rgbColorStr = rgbColorStr.Substring(0, 7);
        //                                }
        //                                hl.NavigateUri = new Uri(@"https://www.hexcolortool.com/" + rgbColorStr);

        //                                MenuItem changeColorItem = new MenuItem();
        //                                changeColorItem.Header = "Change Color";
        //                                changeColorItem.Click += (s, e) => {
        //                                    var result = MpHelpers.Instance.ShowColorDialog((Brush)new BrushConverter().ConvertFrom(linkText));
        //                                };
        //                                hl.ContextMenu.Items.Add(changeColorItem);
        //                                break;
        //                            default:
        //                                Console.WriteLine("Unhandled token type: " + Enum.GetName(typeof(MpSubTextTokenType), (MpSubTextTokenType)hl.Tag) + " with value: " + linkText);
        //                                break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        public static MpEventEnabledFlowDocument Clone(this FlowDocument doc) {
            using (MemoryStream stream = new MemoryStream()) {
                var clonedDoc = new MpEventEnabledFlowDocument();
                TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
                System.Windows.Markup.XamlWriter.Save(range, stream);
                range.Save(stream, DataFormats.XamlPackage);
                TextRange range2 = new TextRange(clonedDoc.ContentEnd, clonedDoc.ContentEnd);
                range2.Load(stream, DataFormats.XamlPackage);
                return clonedDoc;
            }
        }

        public static TextRange FindStringRangeFromPosition2(this RichTextBox rtb, string findText, bool isCaseSensitive = false) {
            var fullText = MpHelpers.Instance.ConvertFlowDocumentToRichText(rtb.Document);
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

        public static TextRange Clone(this TextSelection ts) {
            return new TextRange(ts.Start, ts.End);
        }

        public static void SetRtf(this System.Windows.Controls.RichTextBox rtb, string document) {
            var rtbSelection = rtb.Selection;
            var documentBytes = UTF8Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                rtb.SelectAll();
                rtb.Selection.Load(reader, System.Windows.DataFormats.Rtf);
                //rtb.CaretPosition = rtb.Document.ContentStart;
                if (rtbSelection != null) {
                    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                }
            }
        }

        public static string GetRtf(this RichTextBox rtb) {
            return MpHelpers.Instance.ConvertFlowDocumentToRichText(rtb.Document);
        }

        public static void SetXaml(this System.Windows.Controls.RichTextBox rtb, string document) {
            var documentBytes = Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                rtb.SelectAll();
                rtb.Selection.Load(reader, System.Windows.DataFormats.Xaml);
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
                        Paragraph para = position.Parent as Paragraph;

                        if (para != null) {
                            yield return para;
                        }
                    }
                }
            }
        }

        public static string ToRichText(this string str) {
            if(str == null) {
                str = string.Empty;
            }
            if(MpHelpers.Instance.IsStringRichText(str)) {
                return str;
            }
            return MpHelpers.Instance.ConvertPlainTextToRichText(str);
        }

        public static string ToPlainText(this string str) {
            if (str == null) {
                return string.Empty;
            }
            if (MpHelpers.Instance.IsStringPlainText(str)) {
                return str;
            }
            return MpHelpers.Instance.ConvertRichTextToPlainText(str);
        }

        public static string ToRichText(this FlowDocument doc) {
            return MpHelpers.Instance.ConvertFlowDocumentToRichText(doc);
        }

        public static string ToPlainText(this FlowDocument doc) {
            return doc.ToRichText().ToPlainText();
        }

        public static MpEventEnabledFlowDocument ToFlowDocument(this string str) {
            if(string.IsNullOrEmpty(str)) {
                return MpHelpers.Instance.ConvertRichTextToFlowDocument(MpHelpers.Instance.ConvertPlainTextToRichText(string.Empty));
            }
            if(MpHelpers.Instance.IsStringPlainText(str)) {
                return MpHelpers.Instance.ConvertRichTextToFlowDocument(MpHelpers.Instance.ConvertPlainTextToRichText(str));
            }
            if(MpHelpers.Instance.IsStringRichText(str)) {
                return MpHelpers.Instance.ConvertRichTextToFlowDocument(str);
            }
            throw new Exception("ToFlowDocument exception string must be plain or rich text. Its content is: " + str);
        }

        public static Size GetDocumentSize(this FlowDocument doc) {
            var ft = doc.GetFormattedText();
            return new Size(ft.Width, ft.Height);
            //var tp = doc.ContentStart;
            //Rect lastRect = tp.GetCharacterRect(LogicalDirection.Forward);
            //double w = lastRect.Width, h = lastRect.Height, y = lastRect.Y;
            //while(tp != null && tp.CompareTo(doc.ContentEnd) < 0) {
            //    tp = tp.GetPositionAtOffset(1, LogicalDirection.Forward);
            //    Rect newRect = tp.GetCharacterRect(LogicalDirection.Forward);
            //    if(newRect.Y > lastRect.Y) {
            //        //w = Math.Max(w, newRect.X);
            //        h += newRect.Y;// Math.Max(h, newRect.Y);
            //    }
            //    lastRect = newRect;
            //}
            //return new Size(w, h);
        }

        public static List<KeyValuePair<TextRange, Brush>> FindNonTransparentRangeList(this RichTextBox rtb) {
            var matchRangeList = new List<KeyValuePair<TextRange, Brush>>();
            TextSelection rtbSelection = rtb.Selection;
            var doc = rtb.Document;
            for (TextPointer position = doc.ContentStart;
              position != null && position.CompareTo(doc.ContentEnd) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    Run run = position.Parent as Run;

                    if (run != null) {
                        if (run.Background != null && run.Background != Brushes.Transparent) {
                            matchRangeList.Add(new KeyValuePair<TextRange, Brush>(new TextRange(run.ContentStart, run.ContentEnd), run.Background));
                        }
                    } else {
                        Paragraph para = position.Parent as Paragraph;

                        if (para != null) {
                            if (para.Background != null && para.Background != Brushes.Transparent) {
                                matchRangeList.Add(new KeyValuePair<TextRange, Brush>(new TextRange(para.ContentStart, para.ContentEnd), para.Background));
                            }
                        } else {
                            var span = position.Parent as Span;
                            if(span != null) {
                                if (span.Background != null && span.Background != Brushes.Transparent) {
                                    matchRangeList.Add(new KeyValuePair<TextRange, Brush>(new TextRange(span.ContentStart, span.ContentEnd), span.Background));
                                }
                            }
                        }
                    }
                }
            }
            if (rtbSelection != null) {
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return matchRangeList;
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
              VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

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

        public static bool ContainsByCaseSetting(this string str, string compareStr) {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(compareStr)) {
                return false;
            }
            if (Properties.Settings.Default.SearchByIsCaseSensitive) {
                return str.Contains(compareStr);
            }
            return str.ToLower().Contains(compareStr.ToLower());
        }
        #endregion

        #region Images
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
        #endregion

        #region Mail
        //Extension method for MailMessage to save to a file on disk
        public static void Save(this MailMessage message, string filename, bool addUnsentHeader = true) {
            using (var filestream = File.Open(filename, FileMode.Create)) {
                if (addUnsentHeader) {
                    var binaryWriter = new BinaryWriter(filestream);
                    //Write the Unsent header to the file so the mail client knows this mail must be presented in "New message" mode
                    //binaryWriter.Write(System.Text.Encoding.UTF8.GetBytes("X-Unsent: 1" + Environment.NewLine));
                    binaryWriter.Write(System.Text.Encoding.UTF8.GetBytes("X-Unsent: 1" + Environment.NewLine));
                }

                var assembly = typeof(SmtpClient).Assembly;
                var mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

                // Get reflection info for MailWriter contructor
                var mailWriterContructor = mailWriterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Stream) }, null);

                // Construct MailWriter object with our FileStream
                var mailWriter = mailWriterContructor.Invoke(new object[] { filestream });

                // Get reflection info for Send() method on MailMessage
                var sendMethod = typeof(MailMessage).GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic);

                sendMethod.Invoke(message, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { mailWriter, true, true }, null);

                // Finally get reflection info for Close() method on our MailWriter
                var closeMethod = mailWriter.GetType().GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic);

                // Call close method
                closeMethod.Invoke(mailWriter, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { }, null);
            }
        }
        #endregion
    }
}
