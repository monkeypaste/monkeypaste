using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Newtonsoft.Json;
using QRCoder;
using static QRCoder.PayloadGenerator;

namespace MpWpfApp {
    public class MpTokenizedRichTextBox : RichTextBox {
        private TextRange _lastTokenRange = null;
        private MpSubTextToken _lastToken = null;

        public MpTokenizedRichTextBox() : base() { }

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

        public void HighlightSearchText(SolidColorBrush highlightColor) {
            BeginChange();
            {
                var fullDocRange = new TextRange(Document.ContentStart, Document.ContentEnd);
                fullDocRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);

                ScrollToHome();
                if (SearchText != Properties.Settings.Default.SearchPlaceHolderText && !string.IsNullOrEmpty(SearchText)) {
                    string rtbt = fullDocRange.Text.ToLower();
                    SearchText = SearchText.ToLower();
                    var tokenIdxList = rtbt.AllIndexesOf(SearchText);
                    TextRange lastTokenRange = null;
                    CaretPosition = Document.ContentStart;
                    foreach (int idx in tokenIdxList) {
                        TextPointer startPoint = lastTokenRange == null ? Document.ContentStart : lastTokenRange.End;
                        var range = MpHelpers.FindStringRangeFromPosition(startPoint, SearchText);
                        if (range == null) {
                            Console.WriteLine("Cannot find '" + SearchText + "' in tile");
                        }
                        range?.ApplyPropertyValue(TextElement.BackgroundProperty, highlightColor);
                        lastTokenRange = range;
                    }
                    if (lastTokenRange != null) {
                        Rect r = lastTokenRange.End.GetCharacterRect(LogicalDirection.Backward);
                        ScrollToVerticalOffset(r.Y - (FontSize * 0.5));
                    }
                }
            }
            EndChange();
        }

        public static string GetRichText(DependencyObject obj) {
            return (string)obj.GetValue(RichTextProperty);
        }
        public static void SetRichText(DependencyObject obj, string value) {
            obj.SetValue(RichTextProperty, value);
        }

        public static readonly DependencyProperty RichTextProperty =
          DependencyProperty.RegisterAttached(
            "RichText",
            typeof(string),
            typeof(MpTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    var richTextBox = (RichTextBox)s;
                    richTextBox.SetRtf((string)e.NewValue);
                }
            });

        public static string GetSearchText(DependencyObject obj) {
            return (string)obj.GetValue(SearchTextProperty);
        }
        public static void SetSearchText(DependencyObject obj, string value) {
            obj.SetValue(SearchTextProperty, value);
        }

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
    }
}
