using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace MpWpfApp {
    public class MpTokenizedRichTextBox : RichTextBox {
        #region Private variables

        private TextRange _lastTokenRange = null;
        private MpSubTextToken _lastToken = null;

        #endregion

        #region Properties

        public string SearchText {
            get {
                return (string)GetValue(SearchTextProperty);
            }
            set {
                if ((string)GetValue(SearchTextProperty) != value) {
                    SetValue(SearchTextProperty, value);
                }
            }
        }
        public static readonly DependencyProperty SearchTextProperty =
          DependencyProperty.RegisterAttached(
            "SearchText",
            typeof(string),
            typeof(MpTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    var trtb = (MpTokenizedRichTextBox)s;
                    trtb.HighlightSearchText(Brushes.Yellow);
                },
            });

        //public string RichText {
        //    get {
        //        return (string)GetValue(RichTextProperty);
        //    }
        //    set {
        //        if ((string)GetValue(RichTextProperty) != value) {
        //            SetValue(RichTextProperty, value);
        //        }
        //    }
        //}
        //public static readonly DependencyProperty RichTextProperty =
        //  DependencyProperty.RegisterAttached(
        //    "RichText",
        //    typeof(string),
        //    typeof(MpTokenizedRichTextBox),
        //    new FrameworkPropertyMetadata {
        //        BindsTwoWayByDefault = true,
        //        PropertyChangedCallback = (s, e) => {
        //            var trtb = (MpTokenizedRichTextBox)s;
        //            string rt = (string)e.NewValue;
        //            if (!string.IsNullOrEmpty(rt)) {
        //                //when content has uielements embedded it will be stored as xaml not rt
        //                if(MpHelpers.IsStringRichText(rt)) {
        //                    trtb.SetRtf(rt);
        //                } else {
        //                    //trtb.Document = (MpEventEnabledFlowDocument)XamlReader.Parse(rt);
        //                    trtb.Document = MpHelpers.ConvertXamlToFlowDocument(rt);
        //                }                     
        //            }
        //        }
        //    });

        public string DocumentRtf {
            get {
                return (string)GetValue(DocumentRtfProperty);
            }
            set {
                if ((string)GetValue(DocumentRtfProperty) != value) {
                    SetValue(DocumentRtfProperty, value);
                }
            }
        }
        public static string GetDocumentRtf(DependencyObject obj) {
            return (string)obj.GetValue(DocumentRtfProperty);
        }
        public static void SetDocumentRtf(DependencyObject obj, string value) {
            obj.SetValue(DocumentRtfProperty, value);
        }
        public static readonly DependencyProperty DocumentRtfProperty =
          DependencyProperty.RegisterAttached(
            "DocumentRtf",
            typeof(string),
            typeof(MpTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    var trtb = (MpTokenizedRichTextBox)s;
                    trtb.Document = (MpEventEnabledFlowDocument)MpHelpers.ConvertXamlToFlowDocument((string)e.NewValue);
                    //var newDocument = (MpEventEnabledFlowDocument)e.NewValue;
                    ////instead of directly setting document this workaround ensures document reassignment doesn't fail
                    //TextRange newRange = new TextRange(newDocument.ContentStart, newDocument.ContentEnd);
                    //MemoryStream stream = new MemoryStream();
                    //System.Windows.Markup.XamlWriter.Save(newRange, stream);
                    //newRange.Save(stream, DataFormats.XamlPackage);

                    //var doc = new MpEventEnabledFlowDocument();
                    //var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                    //range.Load(stream, DataFormats.XamlPackage);

                    //// Set the document
                    //trtb.Document = doc;
                }
            });

        public ObservableCollection<MpSubTextToken> Tokens {
            get {
                return (ObservableCollection<MpSubTextToken>)GetValue(TokensProperty);
            }
            set {
                if ((ObservableCollection<MpSubTextToken>)GetValue(TokensProperty) != value) {
                    SetValue(TokensProperty, value);
                }
            }
        } 

        public static readonly DependencyProperty TokensProperty =
          DependencyProperty.RegisterAttached(
            "Tokens",
            typeof(ObservableCollection<MpSubTextToken>),
            typeof(MpTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    if (e.NewValue != null) {
                        //((MpTokenizedRichTextBox)s).Tokens = (ObservableCollection<MpSubTextToken>)e.NewValue;
                        foreach (var token in (ObservableCollection<MpSubTextToken>)e.NewValue) {
                            ((MpTokenizedRichTextBox)s).AddSubTextToken(token);
                        }
                    }
                },
            });

        #endregion

        #region Public Methods

        public MpTokenizedRichTextBox() : base() {
            Document = new MpEventEnabledFlowDocument();
            Tokens = new ObservableCollection<MpSubTextToken>();
            
        }

        public MpEventEnabledFlowDocument GetTemplateDocument() {
            var str = XamlWriter.Save(Document);
            var stringReader = new StringReader(str);
            var xmlReader = XmlReader.Create(stringReader);
            var doc = (MpEventEnabledFlowDocument)XamlReader.Load(xmlReader);

            foreach (var token in Tokens.Where(x => x.TokenType == MpSubTextTokenType.TemplateSegment)) {
                var container = (InlineUIContainer)doc.FindName(token.TokenText);
                var containerRange = new TextRange(container.ElementStart, container.ElementEnd);
                Span s = new Span(container.ElementStart, container.ElementEnd);
                s.Inlines.Add(token.TokenText);
            }
            return doc;
        }

        public void AddSubTextToken(MpSubTextToken token) {
            try {
                if (token.TokenType == MpSubTextTokenType.CopyItemSegment) {

                } else if (token.TokenType == MpSubTextTokenType.TemplateSegment) {
                    MpSubTextTemplateTokenClipBorder stttcb = new MpSubTextTemplateTokenClipBorder();
                    stttcb.DataContext = new MpSubTextTokenViewModel(token);
                    var tokenRange = MpHelpers.FindStringRangeFromPosition(Document.ContentStart, token.TokenText.ToLower());
                    if(tokenRange == null) {
                        token.DeleteFromDatabase();
                        return;
                    }
                    tokenRange.Text = string.Empty;
                    var container = new InlineUIContainer(stttcb, tokenRange.Start);//, TokenizedRichTextBox.CaretPosition.GetInsertionPosition(LogicalDirection.Forward)); ;
                    container.Name = token.TokenText;
                    Document.RegisterName(container.Name, container);
                } else {
                    Hyperlink tokenLink = GetTokenLink(token);
                    if (tokenLink == null) {
                        Console.WriteLine("TokenizedTextbox error, GetTokenLink null for token: " + token.ToString());
                        return;
                    }
                    tokenLink.IsEnabled = true;
                    tokenLink.RequestNavigate += (s, e) => {
                        System.Diagnostics.Process.Start(e.Uri.ToString());
                    };

                    MenuItem convertToQrCodeMenuItem = new MenuItem();
                    convertToQrCodeMenuItem.Header = "Convert to QR Code";
                    convertToQrCodeMenuItem.Click += (s, e1) => {
                        var hyperLink = (Hyperlink)((MenuItem)s).Tag;
                        Clipboard.SetImage(MpHelpers.ConvertUrlToQrCode(hyperLink.NavigateUri.ToString()));
                    };
                    convertToQrCodeMenuItem.Tag = tokenLink;
                    tokenLink.ContextMenu = new ContextMenu();
                    tokenLink.ContextMenu.Items.Add(convertToQrCodeMenuItem);

                    switch (token.TokenType) {
                        case MpSubTextTokenType.StreetAddress:
                            tokenLink.NavigateUri = new Uri("https://google.com/maps/place/" + token.TokenText.Replace(' ', '+'));
                            break;
                        case MpSubTextTokenType.Uri:
                            if (!token.TokenText.Contains("https://")) {
                                tokenLink.NavigateUri = new Uri("https://" + token.TokenText);
                            } else {
                                tokenLink.NavigateUri = new Uri(token.TokenText);
                            }
                            MenuItem minifyUrl = new MenuItem();
                            minifyUrl.Header = "Minify with bit.ly";
                            minifyUrl.Click += (s, e2) => {
                                Hyperlink link = (Hyperlink)((MenuItem)s).Tag;
                                string minifiedLink = MpHelpers.ShortenUrl(link.NavigateUri.ToString()).Result;
                                Clipboard.SetText(minifiedLink);
                            };
                            minifyUrl.Tag = tokenLink;
                            tokenLink.ContextMenu.Items.Add(minifyUrl);
                            break;
                        case MpSubTextTokenType.Email:
                            tokenLink.NavigateUri = new Uri("mailto:" + token.TokenText);
                            break;
                        case MpSubTextTokenType.PhoneNumber:
                            tokenLink.NavigateUri = new Uri("tel:" + token.TokenText);
                            break;
                        case MpSubTextTokenType.Currency:
                            //"https://www.google.com/search?q=%24500.80+to+yen"
                            MenuItem convertCurrencyMenuItem = new MenuItem();
                            convertCurrencyMenuItem.Header = "Convert Currency To";
                            foreach (MpCurrencyType ct in Enum.GetValues(typeof(MpCurrencyType))) {
                                if (ct == MpCurrencyType.None || ct == MpHelpers.GetCurrencyTypeFromString(token.TokenText)) {
                                    continue;
                                }
                                MenuItem subItem = new MenuItem();
                                subItem.Header = Enum.GetName(typeof(MpCurrencyType), ct);
                                subItem.Click += (s, e2) => {
                                    // use https://free.currencyconverterapi.com/ instead of google
                                    //string convertedCurrency = MpHelpers.CurrencyConvert(
                                    //    (decimal)MpHelpers.GetCurrencyValueFromString(token.TokenText),
                                    //    Enum.GetName(typeof(MpCurrencyType), MpHelpers.GetCurrencyTypeFromString(token.TokenText)),
                                    //    Enum.GetName(typeof(MpCurrencyType), ct));
                                    //tokenLink.Inlines.Clear();
                                    //tokenLink.Inlines.Add(new Run(convertedCurrency));
                                    ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).HideWindowCommand.Execute(null);
                                    System.Diagnostics.Process.Start(@"https://www.google.com/search?q=" + token.TokenText + "+to+" + subItem.Header);
                                };
                                convertCurrencyMenuItem.Items.Add(subItem);
                            }

                            tokenLink.ContextMenu.Items.Add(convertCurrencyMenuItem);
                            break;
                        default:

                            break;
                    }
                } 
            }
            catch (Exception ex) {
                Console.WriteLine("TokenizedTextbox error, cannot add token text: " + token.TokenText + " of type: " + Enum.GetName(typeof(MpSubTextTokenType), token.TokenType) + Environment.NewLine + "with exception: " + ex.ToString());
            }
            if(!Tokens.Contains(token)) {
                Tokens.Add(token);
            }
        }

        #endregion

        #region Private Methods
        private Hyperlink GetTokenLink(MpSubTextToken token) {
            Block block = Document.Blocks.ToArray()[token.BlockIdx];
            TextPointer searchStartPointer = block.ContentStart;
            if (_lastToken != null) {        
                if (token.BlockIdx == _lastToken.BlockIdx) {
                    searchStartPointer = _lastTokenRange.End;
                }
            }
            TextRange tokenRange = MpHelpers.FindStringRangeFromPosition(searchStartPointer, token.TokenText);
            if (tokenRange == null) {
                Console.WriteLine("TokenizedRichTextBox error, cannot find textrange for token: " + token.ToString());
                return null;
            }
            _lastTokenRange = tokenRange;
            _lastToken = token;
            return new Hyperlink(tokenRange.Start, tokenRange.End);
        }

        private void HighlightSearchText(SolidColorBrush highlightColor) {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                (Action)(() => {                    
                    var cb = (MpClipBorder)this.GetVisualAncestor<MpClipBorder>();
                    if (cb == null) {
                        Console.WriteLine("TokenizedRichTextBox error, cannot find clipborder");
                        return;
                    }
                    if (cb.DataContext.GetType() != typeof(MpClipTileViewModel)) {
                        return;
                    }
                    var ctvm = (MpClipTileViewModel)cb.DataContext;
                    if (ctvm == null) {
                        Console.WriteLine("TokenizedRichTextBox error, cannot find cliptile viewmodel");
                        return;
                    }
                    var sttvm = ctvm.MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;

                    BeginChange();
                    new TextRange(Document.ContentStart, Document.ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                    ctvm.TileVisibility = Visibility.Collapsed;
                    if (!sttvm.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                        ctvm.TileVisibility = Visibility.Collapsed;
                        EndChange();
                        //return;
                    } else if (SearchText == null ||
                        string.IsNullOrEmpty(SearchText.Trim()) ||
                        SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                        ctvm.TileVisibility = Visibility.Visible;
                        EndChange();
                        //return;
                    } else {
                        TextRange lastSearchTextRange = null;
                        for (TextPointer position = Document.ContentStart;
                         position != null && position.CompareTo(Document.ContentEnd) <= 0;
                         position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                            if (position.CompareTo(Document.ContentEnd) == 0) {
                                break;
                            }
                            string textRun = string.Empty;
                            int indexInRun = -1;
                            if (Properties.Settings.Default.IsSearchCaseSensitive) {
                                textRun = position.GetTextInRun(LogicalDirection.Forward);
                                indexInRun = textRun.IndexOf(SearchText, StringComparison.CurrentCulture);
                            } else {
                                textRun = position.GetTextInRun(LogicalDirection.Forward).ToLower();
                                indexInRun = textRun.IndexOf(SearchText.ToLower(), StringComparison.CurrentCulture);
                            }
                            if (indexInRun >= 0) {
                                position = position.GetPositionAtOffset(indexInRun);
                                if (position != null) {
                                    TextPointer nextPointer = position.GetPositionAtOffset(SearchText.Length);
                                    lastSearchTextRange = new TextRange(position, nextPointer);
                                    lastSearchTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, highlightColor);
                                }
                            }
                        }

                        if (lastSearchTextRange != null) {
                            ctvm.TileVisibility = Visibility.Visible;
                            ScrollToHome();
                            CaretPosition = Document.ContentStart;
                            Rect r = lastSearchTextRange.End.GetCharacterRect(LogicalDirection.Backward);
                            ScrollToVerticalOffset(500);// VerticalOffset r.Y - (FontSize * 0.5));
                                                        //var characterRect = lastTokenPointer.GetCharacterRect(LogicalDirection.Forward);
                                                        //this.ScrollToHorizontalOffset(this.HorizontalOffset + characterRect.Left - this.ActualWidth / 2d);
                                                        //this.ScrollToVerticalOffset(this.VerticalOffset + characterRect.Top - this.ActualHeight / 2d);
                                                        //ScrollToEnd();
                        } else {
                            ctvm.TileVisibility = Visibility.Collapsed;
                        }
                        EndChange();
                    }
                    //var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;
                    //if (mwvm.ClipTrayViewModel.VisibileClipTiles.Count == 0 &&
                    //   !string.IsNullOrEmpty(SearchText) &&
                    //   SearchText != Properties.Settings.Default.SearchPlaceHolderText) {
                    //    mwvm.SearchBoxViewModel.SearchTextBoxBorderBrush = Brushes.Red;
                    //    mwvm.ClipTrayViewModel.ClipTrayVisibility = Visibility.Collapsed;
                    //    mwvm.ClipTrayViewModel.EmptyListMessageVisibility = Visibility.Visible;
                    //} else {
                    //    mwvm.SearchBoxViewModel.SearchTextBoxBorderBrush = Brushes.Transparent;
                    //    mwvm.ClipTrayViewModel.ClipTrayVisibility = Visibility.Visible;
                    //    mwvm.ClipTrayViewModel.EmptyListMessageVisibility = Visibility.Collapsed;
                    //}
                    //var fullDocRange = new TextRange(Document.ContentStart, Document.ContentEnd);
                    ////fullDocRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);

                    //ScrollToHome();
                    //if (SearchText != Properties.Settings.Default.SearchPlaceHolderText && !string.IsNullOrEmpty(SearchText)) {
                    //    string rtbt = fullDocRange.Text.ToLower();
                    //    SearchText = SearchText.ToLower();
                    //    var tokenIdxList = rtbt.AllIndexesOf(SearchText);
                    //    TextRange lastTokenRange = null;
                    //    CaretPosition = Document.ContentStart;
                    //    foreach (int idx in tokenIdxList) {
                    //        TextPointer startPoint = lastTokenRange == null ? Document.ContentStart : lastTokenRange.End;
                    //        startPoint.Po
                    //        var range = MpHelpers.FindStringRangeFromPosition(startPoint, SearchText);
                    //        if (range == null) {
                    //            Console.WriteLine("Cannot find '" + SearchText + "' in tile");
                    //        }
                    //        range?.ApplyPropertyValue(TextElement.BackgroundProperty, highlightColor);
                    //        lastTokenRange = range;
                    //    }
                    //    if (lastTokenRange != null) {
                    //        Rect r = lastTokenRange.End.GetCharacterRect(LogicalDirection.Backward);
                    //        ScrollToVerticalOffset(r.Y - (FontSize * 0.5));
                    //    }
                    //}
                    //EndChange();
                }));            
        }

        #endregion

    }
}
