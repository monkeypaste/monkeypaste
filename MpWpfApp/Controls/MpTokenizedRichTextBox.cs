using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
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
        public static string GetSearchText(DependencyObject obj) {
            return (string)obj.GetValue(SearchTextProperty);
        }
        public static void SetSearchText(DependencyObject obj, string value) {
            obj.SetValue(SearchTextProperty, value);
        }

        public string RichText {
            get {
                return (string)GetValue(RichTextProperty);
            }
            set {
                if ((string)GetValue(RichTextProperty) != value) {
                    SetValue(RichTextProperty, value);
                }
            }
        }
        public static string GetRichText(DependencyObject obj) {
            return (string)obj.GetValue(RichTextProperty);
        }
        public static void SetRichText(DependencyObject obj, string value) {
            obj.SetValue(RichTextProperty, value);
        }

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
        public static ObservableCollection<MpSubTextToken> GetTokens(DependencyObject obj) {
            return (ObservableCollection<MpSubTextToken>)obj.GetValue(TokensProperty);
        }
        public static void SetTokens(DependencyObject obj, ObservableCollection<MpSubTextToken> value) {
            obj.SetValue(TokensProperty, value);
        }

        #endregion

        #region Public Methods

        public MpTokenizedRichTextBox() : base() { }

        public void AddSubTextToken(MpSubTextToken token) {
            Block block = Document.Blocks.ToArray()[token.BlockIdx];
            //find and remove the inline with the token

            //Span inline = (Span)block.Inlines.ToArray()[token.InlineIdx];
            //para.Inlines.Remove(inline);
            TextRange runRange = new TextRange(block.ContentStart, block.ContentEnd);
            string tokenText = runRange.Text.Substring(token.StartIdx, token.EndIdx - token.StartIdx);
            TextPointer searchStartPointer = block.ContentStart;
            if (_lastToken != null) {
                if (token.BlockIdx == _lastToken.BlockIdx) {
                    searchStartPointer = _lastTokenRange.End;
                }
            }

            //Paragraph para = (Paragraph)doc.Blocks.ToArray()[token.BlockIdx];
            //Span inline = (Span)para.Inlines.ToArray()[token.InlineIdx];
            ////para.Inlines.Remove(inline);
            //TextRange runRange = new TextRange(inline.ContentStart, inline.ContentEnd);            
            //string tokenText = runRange.Text.Substring(token.StartIdx,token.EndIdx-token.StartIdx);
            //TextPointer searchStartPointer = inline.ContentStart;
            //if(i > 0) {
            //    var lastToken = sortedTokenList[i - 1];
            //    if(token.BlockIdx == lastToken.BlockIdx && token.InlineIdx == lastToken.InlineIdx) {
            //        searchStartPointer = lastTokenRange.End;
            //    }
            //}

            TextRange tokenRange = MpHelpers.FindStringRangeFromPosition(searchStartPointer, tokenText);
            if (tokenRange == null) {
                return;
            }
            Hyperlink tokenLink = new Hyperlink(tokenRange.Start, tokenRange.End);
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
                    tokenLink.NavigateUri = new Uri("https://google.com/maps/place/" + tokenText.Replace(' ', '+'));
                    break;
                case MpSubTextTokenType.Uri:
                    if (!tokenText.Contains("https://")) {
                        tokenLink.NavigateUri = new Uri("https://" + tokenText);
                    } else {
                        tokenLink.NavigateUri = new Uri(tokenText);
                    }
                    MenuItem minifyUrl = new MenuItem();
                    minifyUrl.Header = "Minify with bit.ly";
                    minifyUrl.Click += (s, e2) => {
                        Hyperlink link = (Hyperlink)((MenuItem)s).Tag;
                        string minifiedLink = MpHelpers.ShortenUrl(link.NavigateUri.ToString()).Result;
                        Clipboard.SetText(minifiedLink);
                        //MpCopyItem newCopyItem = MpCopyItem.CreateCopyItem(MpCopyItemType.RichText, MpCopyItem.PlainTextToRtf(minifiedLink), MainWindowViewModel.ClipboardMonitor.LastWindowWatcher.ThisAppPath, MpHelpers.GetRandomColor());
                        //newCopyItem.WriteToDatabase();
                        //MpTag historyTag = new MpTag(1);
                        //historyTag.LinkWithCopyItem(newCopyItem);
                        //MainWindowViewModel.ClearSelection();
                        //MainWindowViewModel.CreateClipTile(newCopyItem);
                    };
                    minifyUrl.Tag = tokenLink;
                    tokenLink.ContextMenu.Items.Add(minifyUrl);
                    break;
                case MpSubTextTokenType.Email:
                    tokenLink.NavigateUri = new Uri("mailto:" + tokenText);
                    break;
                case MpSubTextTokenType.PhoneNumber:
                    tokenLink.NavigateUri = new Uri("tel:" + tokenText);
                    break;
                default:

                    break;
            }
            _lastTokenRange = tokenRange;
            _lastToken = token;
        }

        #endregion

        #region Private Methods

        private void HighlightSearchText(SolidColorBrush highlightColor) {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                (Action)(() => {
                    
                    var cb = (MpClipBorder)this.GetVisualAncestor<MpClipBorder>();
                    if (cb == null) {
                        throw new Exception("TokenizedRichTextBox error, cannot find clipborder");
                    }
                    if (cb.DataContext.GetType() != typeof(MpClipTileViewModel)) {
                        return;
                    }
                    var ctvm = (MpClipTileViewModel)cb.DataContext;
                    if (ctvm == null) {
                        throw new Exception("TokenizedRichTextBox error, cannot find cliptile viewmodel");
                    }
                    var sttvm = ctvm.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;

                    BeginChange();
                    new TextRange(Document.ContentStart, Document.ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                    if (!sttvm.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                        ctvm.TileVisibility = Visibility.Collapsed;
                        EndChange();
                        return;
                    }
                    if (SearchText == null || string.IsNullOrEmpty(SearchText.Trim()) || SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                        ctvm.TileVisibility = Visibility.Visible;
                        EndChange();
                        return;
                    }
                    
                    TextRange lastTokenPointer = null;
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
                                lastTokenPointer = new TextRange(position, nextPointer);
                                lastTokenPointer.ApplyPropertyValue(TextElement.BackgroundProperty, highlightColor);
                            }
                        }
                    }

                    if (lastTokenPointer != null) {
                        ctvm.TileVisibility = Visibility.Visible;
                        ScrollToHome();
                        CaretPosition = Document.ContentStart;
                        Rect r = lastTokenPointer.End.GetCharacterRect(LogicalDirection.Backward);
                        ScrollToVerticalOffset(500);// VerticalOffset r.Y - (FontSize * 0.5));
                        //var characterRect = lastTokenPointer.GetCharacterRect(LogicalDirection.Forward);
                        //this.ScrollToHorizontalOffset(this.HorizontalOffset + characterRect.Left - this.ActualWidth / 2d);
                        //this.ScrollToVerticalOffset(this.VerticalOffset + characterRect.Top - this.ActualHeight / 2d);
                        //ScrollToEnd();
                    } else {
                        ctvm.TileVisibility = Visibility.Collapsed;
                    }
                    EndChange();



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

        #region Dependency Property Registrations

        public static readonly DependencyProperty TokensProperty =
          DependencyProperty.RegisterAttached(
            "Tokens",
            typeof(ObservableCollection<MpSubTextToken>),
            typeof(MpTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    if(e.NewValue != null) {
                        foreach (var token in (ObservableCollection<MpSubTextToken>)e.NewValue) {
                            ((MpTokenizedRichTextBox)s).AddSubTextToken(token);
                        }
                    }                    
                },
            });

        public static readonly DependencyProperty RichTextProperty =
          DependencyProperty.RegisterAttached(
            "RichText",
            typeof(string),
            typeof(MpTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    if (!string.IsNullOrEmpty((string)e.NewValue)) {
                        var richTextBox = (RichTextBox)s;
                        richTextBox.SetRtf((string)e.NewValue);
                    }
                }
            });

        public static readonly DependencyProperty SearchTextProperty =
          DependencyProperty.RegisterAttached(
            "SearchText",
            typeof(string),
            typeof(MpTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    ((MpTokenizedRichTextBox)s).HighlightSearchText(Brushes.Yellow);
                },
            });

        #endregion
    }
}
