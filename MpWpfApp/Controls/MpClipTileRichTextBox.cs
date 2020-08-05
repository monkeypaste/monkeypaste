using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using static QRCoder.PayloadGenerator;

namespace MpWpfApp {
    public class MpClipTileRichTextBox : RichTextBox {
        private TextRange _lastTokenRange = null;
        private MpSubTextToken _lastToken = null;

        public MpClipTileRichTextBox() : base() { 
            
        }

        public String SearchText {
            get { 
                return (String)GetValue(SearchTextProperty); 
            }
            set { 
                SetValue(SearchTextProperty, value); 
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

            TextRange tokenRange = FindStringRangeFromPosition(searchStartPointer, tokenText);
            if(tokenRange == null) {
                return;
            }
            Hyperlink tokenLink = new Hyperlink(tokenRange.Start, tokenRange.End);
            tokenLink.IsEnabled = true;
            tokenLink.RequestNavigate += (s,e) => {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            };
            MenuItem convertToQrCodeMenuItem = new MenuItem();
            convertToQrCodeMenuItem.Header = "Convert to QR Code";
            convertToQrCodeMenuItem.Click += (s,e1) => {
                var hyperLink = (Hyperlink)(((MenuItem)s).Tag);

                Url generator = new Url(hyperLink.NavigateUri.ToString());
                string payload = generator.ToString();

                using (QRCodeGenerator qrGenerator = new QRCodeGenerator()) {
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                    using (QRCode qrCode = new QRCode(qrCodeData)) {
                        var qrCodeAsBitmap = qrCode.GetGraphic(20);
                        Clipboard.SetImage(MpHelpers.ConvertBitmapToBitmapSource(qrCodeAsBitmap));
                        //MpCopyItem qrCopyItem = MpCopyItem.CreateCopyItem(MpCopyItemType.Image, MpHelpers.ConvertBitmapToBitmapSource(qrCodeAsBitmap), MainWindowViewModel.ClipboardMonitor.LastWindowWatcher.ThisAppPath, MpHelpers.GetRandomColor());
                        //qrCopyItem.WriteToDatabase();
                        //MpTag historyTag = new MpTag(1);
                        //historyTag.LinkWithCopyItem(qrCopyItem);
                        //MainWindowViewModel.ClearSelection();
                        //MainWindowViewModel.CreateClipTile(qrCopyItem);
                    }
                }
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
                        Hyperlink link = ((Hyperlink)((MenuItem)s).Tag);
                        string minifiedLink = ShortenUrl(link.NavigateUri.ToString()).Result;
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
        private async Task<string> ShortenUrl(string url) {
            string _bitlyToken = @"f6035b9ed05ac82b42d4853c984e34a4f1ba05d8";
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                "https://api-ssl.bitly.com/v4/shorten") {
                Content = new StringContent($"{{\"long_url\":\"{url}\"}}",
                                                Encoding.UTF8,
                                                "application/json")
            };

            try {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bitlyToken);
                var response = await client.SendAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) {
                    Console.WriteLine("Minify error: " + response.Content.ToString());
                    return string.Empty;
                }

                var responsestr = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responsestr);
                return jsonResponse["link"];
            }
            catch (Exception ex) {
                Console.WriteLine("Minify exception: " + ex.ToString());
                return string.Empty;
            }
        }
        private static void OnDataChanged(DependencyObject source, DependencyPropertyChangedEventArgs e) {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() => {
                    OnDataChangedHelper((RichTextBox)source, (string)e.NewValue,Brushes.Yellow);
                })
            );
        }
        private static void OnDataChangedHelper(RichTextBox rtb, string updatedSearchText,SolidColorBrush highlightColor) {
            var fullDocRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            fullDocRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);

            
            if(updatedSearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                return;
            }
            string rtbt = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.ToLower();
            updatedSearchText = updatedSearchText.ToLower();
            var tokenIdxList = rtbt.AllIndexesOf(updatedSearchText);
            TextRange lastTokenRange = null;
            rtb.CaretPosition = rtb.Document.ContentStart;
            foreach (int idx in tokenIdxList) {
                TextPointer startPoint = lastTokenRange == null ? rtb.Document.ContentStart : lastTokenRange.End;
                var range = FindStringRangeFromPosition(startPoint, updatedSearchText);
                if(range == null) {
                    //i don't know why range is coming back null but to avoid the exception just returtn
                    Console.WriteLine("Can't find range for highlight: " + rtbt);
                    return;
                }
                range.ApplyPropertyValue(TextElement.BackgroundProperty, highlightColor);
                lastTokenRange = range;
            }
        }

        public static TextRange FindStringRangeFromPosition(TextPointer position, string lowerCaseStr) {
            while (position != null) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text) {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward).ToLower();

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(lowerCaseStr);
                    if (indexInRun >= 0) {
                        return new TextRange(position.GetPositionAtOffset(indexInRun), position.GetPositionAtOffset(indexInRun + lowerCaseStr.Length));
                    }
                } 
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
            // position will be null if "word" is not found.
            return null;
        }
        public static readonly DependencyProperty SearchTextProperty = 
            DependencyProperty.Register("SearchText",
                                        typeof(string),
                                        typeof(MpClipTileRichTextBox),
                                        new FrameworkPropertyMetadata(null,OnDataChanged));
    }
}
