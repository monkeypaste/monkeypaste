using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace MpWpfApp {
    public class MpSubTextTokenListExtension : DependencyObject {
        public static List<MpSubTextToken> GetSubTextTokenList(DependencyObject obj) {
            return (List<MpSubTextToken>)obj.GetValue(SubTextTokenListProperty);
        }
        public static void SetSubTextTokenList(DependencyObject obj, List<MpSubTextToken> value) {
            obj.SetValue(SubTextTokenListProperty, value);
        }

        public List<MpSubTextToken> SubTextTokenList {
            get {
                return (List<MpSubTextToken>)GetValue(SubTextTokenListProperty);
            }
            set {
                if ((List<MpSubTextToken>)GetValue(SubTextTokenListProperty) != value) {
                    SetValue(SubTextTokenListProperty, value);
                }
            }
        }

        public static readonly DependencyProperty SubTextTokenListProperty =
          DependencyProperty.RegisterAttached(
            "SubTextTokenList",
            typeof(List<MpSubTextToken>),
            typeof(MpSubTextTokenListExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (s, e) => {
                    TextRange lastTokenRange = null;
                    MpSubTextToken lastToken = null;
                    var rtb = (RichTextBox)s;
                    var oldTokenList = (List<MpSubTextToken>)e.OldValue;
                    var newTokenList = (List<MpSubTextToken>)e.NewValue;

                    if(oldTokenList != null) {
                        //foreach (var token in oldTokenList.Where(x => x.TokenType == MpSubTextTokenType.TemplateSegment)) {
                        //    var container = (InlineUIContainer)rtb.Document.FindName(token.TokenText);
                        //    var containerRange = new TextRange(container.ElementStart, container.ElementEnd);
                        //    Span sp = new Span(container.ElementStart, container.ElementEnd);
                        //    sp.Inlines.Add(token.TokenText);
                        //}
                    }
                    
                    if(newTokenList != null) {
                        foreach (var token in newTokenList) {
                            try {
                                if (token.TokenType == MpSubTextTokenType.CopyItemSegment) {

                                } else if (token.TokenType == MpSubTextTokenType.TemplateSegment) {
                                    return;
                                    MpSubTextTemplateTokenClipBorder stttcb = new MpSubTextTemplateTokenClipBorder();
                                    stttcb.DataContext = new MpSubTextTokenViewModel(token);
                                    var tokenRange = MpHelpers.FindStringRangeFromPosition(rtb.Document.ContentStart, token.TokenText.ToLower());
                                    if (tokenRange == null) {
                                        //token.DeleteFromDatabase();
                                        //return;

                                        //token already exists so continue
                                        continue;
                                    }
                                    tokenRange.Text = string.Empty;
                                    var button = new Button() { Content = "Test" };
                                    button.Click += (s1, e3) => {
                                        MessageBox.Show("BOO! " + token.TokenText);
                                    };
                                    var container = new InlineUIContainer(button, tokenRange.Start);//, TokenizedRichTextBox.CaretPosition.GetInsertionPosition(LogicalDirection.Forward)); ;
                                    container.Name = token.TokenText;
                                    rtb.Document.RegisterName(container.Name, container);
                                } else {
                                    Block block = rtb.Document.Blocks.ToArray()[token.BlockIdx];
                                    TextPointer searchStartPointer = block.ContentStart;
                                    if (lastToken != null) {
                                        if (token.BlockIdx == lastToken.BlockIdx) {
                                            searchStartPointer = lastTokenRange.End;
                                        }
                                    }
                                    TextRange tokenRange = MpHelpers.FindStringRangeFromPosition(searchStartPointer, token.TokenText);
                                    if (tokenRange == null) {
                                        Console.WriteLine("TokenizedRichTextBox error, cannot find textrange for token: " + token.ToString());
                                        return;
                                    }
                                    lastTokenRange = tokenRange;
                                    lastToken = token;
                                    Hyperlink tokenLink = new Hyperlink(tokenRange.Start, tokenRange.End);
                                    if (tokenLink == null) {
                                        Console.WriteLine("TokenizedTextbox error, GetTokenLink null for token: " + token.ToString());
                                        return;
                                    }
                                    tokenLink.IsEnabled = true;
                                    tokenLink.RequestNavigate += (s4, e4) => {
                                        System.Diagnostics.Process.Start(e4.Uri.ToString());
                                    };

                                    MenuItem convertToQrCodeMenuItem = new MenuItem();
                                    convertToQrCodeMenuItem.Header = "Convert to QR Code";
                                    convertToQrCodeMenuItem.Click += (s5, e1) => {
                                        var hyperLink = (Hyperlink)((MenuItem)s5).Tag;
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
                                            minifyUrl.Click += (s1, e2) => {
                                                Hyperlink link = (Hyperlink)((MenuItem)s1).Tag;
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
                                                subItem.Click += (s2, e2) => {
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
                        }

                        //clone and sub uielements for tokentext
                        //var str = MpHelpers.ConvertFlowDocumentToXaml((MpEventEnabledFlowDocument)rtb.Document);//XamlWriter.Save(rtb.Document);
                        //var doc = (MpEventEnabledFlowDocument)MpHelpers.ConvertXamlToFlowDocument(str);//XamlReader.Load(xmlReader);

                        //foreach (var token in newTokenList.Where(x => x.TokenType == MpSubTextTokenType.TemplateSegment)) {
                        //    var container = (InlineUIContainer)doc.FindName(token.TokenText);
                        //    var containerRange = new TextRange(container.ElementStart, container.ElementEnd);
                        //    Span sp = new Span(container.ElementStart, container.ElementEnd);
                        //    sp.Inlines.Add(token.TokenText);
                        //}
                        //rtb.Tag = MpHelpers.ConvertFlowDocumentToXaml((MpEventEnabledFlowDocument)doc);
                    }
                }
            });

        public void AddToken(MpSubTextToken newToken) {

        }
    }
}
