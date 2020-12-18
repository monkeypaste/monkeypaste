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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
        public static Hyperlink ConvertToTemplateHyperlink(
            this Hyperlink hl,
            RichTextBox rtb,
            string templateName,
            Brush templateColor) {
            var ctvm = (MpClipTileViewModel)rtb.DataContext;
            hl.DataContext = new MpTemplateHyperlinkViewModel(
                ctvm,
                templateName,
                templateColor,
                hl.FontSize,
                rtb);

            var thlvm = (MpTemplateHyperlinkViewModel)hl.DataContext;

            Binding borderBrushBinding = new Binding();
            borderBrushBinding.Source = thlvm;
            borderBrushBinding.Path = new PropertyPath(nameof(thlvm.TemplateBorderBrush));

            Binding fontSizeBinding = new Binding();
            fontSizeBinding.Source = thlvm;
            fontSizeBinding.Path = new PropertyPath(nameof(thlvm.TemplateFontSize));
            fontSizeBinding.Mode = BindingMode.TwoWay;
            fontSizeBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            Binding foregroundBinding = new Binding();
            foregroundBinding.Source = thlvm;
            foregroundBinding.Path = new PropertyPath(nameof(thlvm.TemplateForegroundBrush));

            Binding backgroundBinding = new Binding();
            backgroundBinding.Source = thlvm;
            backgroundBinding.Path = new PropertyPath(nameof(thlvm.TemplateBackgroundBrush));
            backgroundBinding.Mode = BindingMode.TwoWay;
            backgroundBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            Binding templateDisplayValueBinding = new Binding();
            templateDisplayValueBinding.Source = thlvm;
            templateDisplayValueBinding.Path = new PropertyPath(nameof(thlvm.TemplateDisplayValue));

            Binding templateTextBlockVisibilityBinding = new Binding();
            templateTextBlockVisibilityBinding.Source = thlvm;
            templateTextBlockVisibilityBinding.Path = new PropertyPath(nameof(thlvm.TemplateTextBlockVisibility));

            Binding templateDeleteTemplateTextButtonVisibilityBinding = new Binding();
            templateDeleteTemplateTextButtonVisibilityBinding.Source = thlvm;
            templateDeleteTemplateTextButtonVisibilityBinding.Path = new PropertyPath(nameof(thlvm.DeleteTemplateTextButtonVisibility));

            Binding isEnabledBinding = new Binding();
            isEnabledBinding.Source = ctvm;
            isEnabledBinding.Path = new PropertyPath(nameof(ctvm.IsEditingTile));

            BindingOperations.SetBinding(hl, Hyperlink.TargetNameProperty, templateDisplayValueBinding);
            BindingOperations.SetBinding(hl, Hyperlink.BackgroundProperty, backgroundBinding);
            BindingOperations.SetBinding(hl, Hyperlink.ForegroundProperty, foregroundBinding);
            BindingOperations.SetBinding(hl, Hyperlink.FontSizeProperty, fontSizeBinding);
            BindingOperations.SetBinding(hl, Hyperlink.IsEnabledProperty, isEnabledBinding);

            TextBlock tb = new TextBlock();
            tb.Background = Brushes.Transparent;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.FontSize = 10;            
            BindingOperations.SetBinding(tb, TextBlock.ForegroundProperty, foregroundBinding);
            BindingOperations.SetBinding(tb, TextBlock.TextProperty, templateDisplayValueBinding);
            BindingOperations.SetBinding(tb, TextBlock.HeightProperty, fontSizeBinding);

            var path = @"pack://application:,,,/Resources/Images/";
            Image dbImg = new Image(); 
            dbImg.Source = (BitmapSource)new BitmapImage(new Uri(path + "close2.png"));
            //Button db = new Button();
            //db.Background = new ImageBrush((BitmapSource)new BitmapImage(new Uri(path + "close2.png")));
            dbImg.Margin = new Thickness(5, 0, 0, 0);
            //db.BorderThickness = new Thickness(0);
            dbImg.Width = 15;
            dbImg.Height = 15;
            //dbImg.Background = Brushes.Transparent;
            //db.Content = dbImg;
            dbImg.MouseEnter += (s, e) => {
                //db.Background = Brushes.Transparent;
                dbImg.Source = new BitmapImage(new Uri(path + "close1.png"));
            };
            dbImg.MouseLeave += (s, e) => {
                //db.Background = Brushes.Transparent;
                dbImg.Source = new BitmapImage(new Uri(path + "close2.png"));
            };
            dbImg.MouseLeftButtonUp += (s, e) => {
                rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                rtb.Selection.Text = string.Empty;
                thlvm.Dispose();
            };
            BindingOperations.SetBinding(dbImg, Button.VisibilityProperty, templateDeleteTemplateTextButtonVisibilityBinding);

            DockPanel dp = new DockPanel();
            dp.Children.Add(tb);
            dp.Children.Add(dbImg);
            DockPanel.SetDock(tb, Dock.Left);
            DockPanel.SetDock(dbImg, Dock.Right);

            Border b = new Border();
            b.Focusable = true;
            b.Background = hl.Background;
            b.BorderBrush = hl.Foreground;
            b.BorderThickness = new Thickness(1);
            b.CornerRadius = new CornerRadius(5);
            b.VerticalAlignment = VerticalAlignment.Stretch;
            b.Padding = new Thickness(0.5);
            b.Child = dp;
            b.MouseEnter += (s, e) => {
                if (thlvm.IsFocused) {
                    return;
                }
                thlvm.IsHovering = true;
            };
            b.MouseLeave += (s, e) => {
                if (thlvm.IsFocused) {
                    return;
                }
                thlvm.IsHovering = false;
            };
            b.PreviewMouseLeftButtonDown += (s, e) => {
                thlvm.IsFocused = true;
            };
            BindingOperations.SetBinding(b, Border.BackgroundProperty, backgroundBinding);
            BindingOperations.SetBinding(b, Border.BorderBrushProperty, borderBrushBinding);
            //BindingOperations.SetBinding(b, Border.HeightProperty, fontSizeBinding);

            InlineUIContainer container = new InlineUIContainer(b);
            //Run run = new Run(hl.TargetName);
            //run.Background = hl.Background;
            //run.Foreground = hl.Foreground;

            hl.Inlines.Clear();
            hl.Inlines.Add(container);
            hl.Tag = MpSubTextTokenType.TemplateSegment;
            hl.TextDecorations = null;
            hl.NavigateUri = new Uri(Properties.Settings.Default.TemplateTokenUri);
            hl.Unloaded += (s, e) => {
                if (hl.DataContext != null) {
                    ((MpTemplateHyperlinkViewModel)hl.DataContext).Dispose();
                }
            };
            hl.RequestNavigate += (s4, e4) => {
                // TODO Add logic to convert to editable region if in paste mode on click
                rtb.Selection.Select(hl.ContentStart, hl.ContentEnd);
                MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(rtb, hl, true);
            };

            var editTemplateMenuItem = new MenuItem();
            editTemplateMenuItem.Header = "Edit";
            editTemplateMenuItem.PreviewMouseDown += (s4, e4) => {
                e4.Handled = true;
                rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(rtb, hl, true);
            };

            var deleteTemplateMenuItem = new MenuItem();
            deleteTemplateMenuItem.Header = "Delete";
            deleteTemplateMenuItem.Click += (s4, e4) => {
                rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                rtb.Selection.Text = string.Empty;
            };
            hl.ContextMenu = new ContextMenu();
            hl.ContextMenu.Items.Add(editTemplateMenuItem);
            hl.ContextMenu.Items.Add(deleteTemplateMenuItem);

            return hl;
        }

        public static List<Hyperlink> FindHyperlinks(this RichTextBox rtb) {
            List<Hyperlink> hlList = new List<Hyperlink>();
            TextPointer tp = rtb.Document.ContentStart;
            while (tp != null && tp != rtb.Document.ContentEnd) {
                var nextLink = rtb.FindNextHyperlink(tp);
                if(nextLink == null) {
                    break;
                }
                hlList.Add(nextLink);
                tp = nextLink.ElementEnd;
            }
            return hlList;
        }

        public static Hyperlink FindNextHyperlink(this RichTextBox rtb, TextPointer fromTextPointer) {
            // This method returns the position just inside of the first text Run (if any) in a 
            // specified text container.
            TextPointer position = fromTextPointer;

            // Traverse content in forward direction until the position is immediately after the opening 
            // tag of a Run element, or the end of content is encountered.
            while (position != null) {
                // Is the current position just after an opening element tag?
                if (position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.ElementStart) {
                    // If so, is the tag a Run?
                    if (position.Parent is Hyperlink) {
                        return (Hyperlink)position.Parent;
                    }
                }

                // Not what we're looking for; on to the next position.
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            // This will be either null if no Run is found, or a position just inside of the first Run element in the
            // specifed text container.  Because position is formed from ContentStart, it will have a logical direction
            // of Backward.
            return null;
        }

        public static List<Hyperlink> GetAllHyperlinkList(this RichTextBox rtb) {
            if (rtb.Tag == null) {
                return new List<Hyperlink>();
            }
            return (List<Hyperlink>)rtb.Tag;
            //return rtb.FindHyperlinks();
        }

        public static List<Hyperlink> GetTemplateHyperlinkList(this RichTextBox rtb, bool unique = false) {
            var templateLinkList = rtb.GetAllHyperlinkList().Where(x => x.NavigateUri?.OriginalString == Properties.Settings.Default.TemplateTokenUri).ToList();
            if(templateLinkList == null) {
                templateLinkList = new List<Hyperlink>();
            }
            if (unique) {
                var toRemove = new List<Hyperlink>();
                foreach(var hl in templateLinkList) {
                    foreach(var hl2 in templateLinkList) {
                        if(hl == hl2 || toRemove.Contains(hl) || toRemove.Contains(hl2)) {
                            continue;
                        }
                        if(hl.TargetName == hl2.TargetName) {
                            toRemove.Add(hl2);
                        }
                    }
                }
                foreach (var hlr in toRemove) {
                    templateLinkList.Remove(hlr);
                }
            }
            return templateLinkList;
        }

        public static void ClearHyperlinks(this RichTextBox rtb) {
            //replaces hyperlinks with runs of there textrange text
            //if hl is templatee it decodes the run into #templatename#templatecolor# 
            //TextRange ts = rtb.Selection;
            var hll = rtb.GetAllHyperlinkList();
            foreach (var hl in hll) {
                if(hl.ElementStart.IsInSameDocument(rtb.Document.ContentStart)){
                    var hlRange = new TextRange(hl.ElementStart, hl.ElementEnd);
                    //rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                    if (hl.NavigateUri != null && hl.NavigateUri.OriginalString == Properties.Settings.Default.TemplateTokenUri) {
                        hlRange.Text = string.Format(
                            @"{0}{1}{0}{2}{0}", 
                            Properties.Settings.Default.TemplateTokenMarker, 
                            hl.TargetName, 
                            ((SolidColorBrush)hl.Background).ToString());
                    } else {
                        //hlRange.Text = hlRange.Text;
                    }
                } else {
                    Console.WriteLine("Error clearing templates for rtf: ");
                    Console.WriteLine(rtb.GetRtf());
                    continue;
                }          
            }
            rtb.Tag = null;
        }

        public static void CreateHyperlinks(this RichTextBox rtb, string searchText = "") {
            //rtb.ClearHyperlinks();
            var regExGroupList = new List<string> {
                //WebLink
                @"(?:https?://|www\.)\S+", 
                //Email
                @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})",
                //PhoneNumber
                @"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})",
                //Currency
                @"[$|£|€|¥][\d|\.]([0-9]{0,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.\d{0,2})?",
                //HexColor (no alpha)
                @"#([0-9]|[a-fA-F]){5}([^" + Properties.Settings.Default.TemplateTokenMarker + "][ ])",
                //StreetAddress
                @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b",                
                //Text Template
                string.Format(
                    @"[{0}].*?[{0}].*?[{0}]", 
                    Properties.Settings.Default.TemplateTokenMarker),                
                //HexColor (with alpha)
                @"#([0-9]|[a-fA-F]){7}([^" + Properties.Settings.Default.TemplateTokenMarker + "][ ])",
            };
            List<Hyperlink> linkList = new List<Hyperlink>();
            TextRange fullDocRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            for (int i = 0; i < regExGroupList.Count; i++) {
                TextPointer lastRangeEnd = rtb.Document.ContentStart;
                var regExStr = regExGroupList[i];
                MatchCollection mc = Regex.Matches(fullDocRange.Text, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);                                
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            Hyperlink hl = null; 
                            var matchRange = MpHelpers.FindStringRangeFromPosition(lastRangeEnd, c.Value);                            
                            if (matchRange == null) {
                                continue;
                            }
                            lastRangeEnd = matchRange.End;
                            if ((MpSubTextTokenType)(i + 1) == MpSubTextTokenType.TemplateSegment) {
                                var tokenProps = matchRange.Text.Split(new string[] { Properties.Settings.Default.TemplateTokenMarker }, System.StringSplitOptions.RemoveEmptyEntries);
                                matchRange.Text = tokenProps[0];
                                hl = new Hyperlink(matchRange.Start, matchRange.End).ConvertToTemplateHyperlink(rtb,matchRange.Text, (SolidColorBrush)(new BrushConverter().ConvertFrom(tokenProps[1])));
                                
                                //this ensures highlighting has an effective textrange since template ranges alter document
                                matchRange = new TextRange(hl.ContentStart, hl.ContentEnd);
                            } else {
                                hl = new Hyperlink(matchRange.Start, matchRange.End);
                                var linkText = c.Value;
                                //account for special case for hexcolor w/ alpha
                                hl.Tag = i + 1 > (int)MpSubTextTokenType.TemplateSegment ? MpSubTextTokenType.HexColor : (MpSubTextTokenType)(i + 1);
                                hl.IsEnabled = true;
                                hl.MouseLeftButtonDown += (s4, e4) => {
                                    MpHelpers.OpenUrl(hl.NavigateUri.ToString());
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
                                        var fromCurrencyType = MpHelpers.GetCurrencyTypeFromString(linkText);
                                        foreach (MpCurrency currency in MpCurrencyConverter.Instance.CurrencyList) {
                                            if (currency.Id == Enum.GetName(typeof(CurrencyType),fromCurrencyType)) {
                                                continue;
                                            }
                                            MenuItem subItem = new MenuItem();
                                            subItem.Header = currency.CurrencyName + "(" + currency.CurrencySymbol + ")";
                                            subItem.Click += (s2, e2) => {
                                                Enum.TryParse(currency.Id, out CurrencyType toCurrencyType);
                                                var convertedValue = MpCurrencyConverter.Instance.Convert(
                                                    MpHelpers.GetCurrencyValueFromString(linkText), 
                                                    fromCurrencyType, 
                                                    toCurrencyType);
                                                convertedValue = Math.Round(convertedValue, 2);                                                
                                                if(rtb.Tag != null && ((List<Hyperlink>)rtb.Tag).Contains(hl)) {
                                                    ((List<Hyperlink>)rtb.Tag).Remove(hl);
                                                }
                                                Run run = new Run(currency.CurrencySymbol + convertedValue);
                                                hl.Inlines.Clear();
                                                hl.Inlines.Add(run);
                                                rtb.ClearHyperlinks();
                                                ((MpClipTileViewModel)rtb.DataContext).CopyItemRichText = MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);
                                                rtb.CreateHyperlinks();
                                            };
                                            convertCurrencyMenuItem.Items.Add(subItem);
                                        }

                                        hl.ContextMenu.Items.Add(convertCurrencyMenuItem);
                                        break;
                                    case MpSubTextTokenType.HexColor:
                                        var rgbColorStr = linkText;
                                        if(rgbColorStr.Length > 7) {
                                            rgbColorStr = rgbColorStr.Substring(0, 7);
                                        }
                                        hl.NavigateUri = new Uri(@"https://www.hexcolortool.com/" + rgbColorStr);

                                        MenuItem changeColorItem = new MenuItem();
                                        changeColorItem.Header = "Change Color";
                                        changeColorItem.Click += (s, e) => {
                                            var result = MpHelpers.ShowColorDialog((Brush)new BrushConverter().ConvertFrom(linkText));
                                        };
                                        hl.ContextMenu.Items.Add(changeColorItem);
                                        break;
                                    default:
                                        Console.WriteLine("Unhandled token type: " + Enum.GetName(typeof(MpSubTextTokenType), (MpSubTextTokenType)hl.Tag) + " with value: " + linkText);
                                        break;
                                }
                            }

                            if(!string.IsNullOrEmpty(searchText) && hl != null) {
                                //only occurs at end of highlighttext when tokens are refreshed
                                if((MpSubTextTokenType)hl.Tag == MpSubTextTokenType.TemplateSegment) {
                                    string linkText = ((MpTemplateHyperlinkViewModel)hl.DataContext).TemplateName;
                                    if (linkText.ToLower().Contains(searchText.ToLower())) {
                                        ((MpClipTileViewModel)rtb.DataContext).TileVisibility = Visibility.Visible;
                                        var tb = (TextBlock)((DockPanel)((Border)((InlineUIContainer)hl.Inlines.FirstInline).Child).Child).Children[0];
                                        
                                        var highlightRange = MpHelpers.FindStringRangeFromPosition(tb.ContentStart, searchText);
                                        if (highlightRange != null) {
                                            highlightRange.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString));
                                        }
                                    }
                                } else {
                                    string linkText = new TextRange(hl.ContentStart, hl.ContentEnd).Text; 
                                    if (linkText.ToLower().Contains(searchText.ToLower())) {
                                        var highlightRange = MpHelpers.FindStringRangeFromPosition(hl.ContentStart, searchText);
                                        if (highlightRange != null) {
                                            highlightRange.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString));
                                        }
                                    }
                                }
                                
                            }

                            linkList.Add(hl);
                        }
                    }
                }
            }
            rtb.Tag = linkList;
            rtb.ScrollToHome();
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
            var documentBytes = UTF8Encoding.Default.GetBytes(document);
            using (var reader = new MemoryStream(documentBytes)) {
                reader.Position = 0;
                rtb.SelectAll();
                rtb.Selection.Load(reader, System.Windows.DataFormats.Rtf);
            }
        }

        public static string GetRtf(this RichTextBox rtb) {
            return MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);
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
    }
}
