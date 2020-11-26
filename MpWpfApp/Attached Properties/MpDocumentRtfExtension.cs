using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

namespace MpWpfApp {
    public class MpDocumentRtfExtension : DependencyObject {
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
            typeof(MpDocumentRtfExtension),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (obj, e) => {
                    if(string.IsNullOrEmpty((string)e.NewValue)) {
                        return;
                    }
                    var rtb = (RichTextBox)obj;                    
                    rtb.SetRtf((string)e.NewValue);
                    rtb.ClearHyperlinks();
                    foreach (var hyperlink in rtb.AddHyperlinks()) {
                        var linkText = (hyperlink.Inlines.FirstInline as Run).Text;

                        hyperlink.IsEnabled = true;
                        hyperlink.RequestNavigate += (s4, e4) => {
                            System.Diagnostics.Process.Start(e4.Uri.ToString());
                        };

                        MenuItem convertToQrCodeMenuItem = new MenuItem();
                        convertToQrCodeMenuItem.Header = "Convert to QR Code";
                        convertToQrCodeMenuItem.Click += (s5, e1) => {
                            var hyperLink = (Hyperlink)((MenuItem)s5).Tag;
                            Clipboard.SetImage(MpHelpers.ConvertUrlToQrCode(hyperLink.NavigateUri.ToString()));
                        };
                        convertToQrCodeMenuItem.Tag = hyperlink;
                        hyperlink.ContextMenu = new ContextMenu();
                        hyperlink.ContextMenu.Items.Add(convertToQrCodeMenuItem);

                        switch ((MpSubTextTokenType)hyperlink.Tag) {
                            case MpSubTextTokenType.StreetAddress:
                                hyperlink.NavigateUri = new Uri("https://google.com/maps/place/" + linkText.Replace(' ', '+'));
                                break;
                            case MpSubTextTokenType.Uri:
                                if (!linkText.Contains("https://")) {
                                    hyperlink.NavigateUri = new Uri("https://" + linkText);
                                } else {
                                    hyperlink.NavigateUri = new Uri(linkText);
                                }
                                MenuItem minifyUrl = new MenuItem();
                                minifyUrl.Header = "Minify with bit.ly";
                                minifyUrl.Click += (s1, e2) => {
                                    Hyperlink link = (Hyperlink)((MenuItem)s1).Tag;
                                    string minifiedLink = MpHelpers.ShortenUrl(link.NavigateUri.ToString()).Result;
                                    Clipboard.SetText(minifiedLink);
                                };
                                minifyUrl.Tag = hyperlink;
                                hyperlink.ContextMenu.Items.Add(minifyUrl);
                                break;
                            case MpSubTextTokenType.Email:
                                hyperlink.NavigateUri = new Uri("mailto:" + linkText);
                                break;
                            case MpSubTextTokenType.PhoneNumber:
                                hyperlink.NavigateUri = new Uri("tel:" + linkText);
                                break;
                            case MpSubTextTokenType.Currency:
                                //"https://www.google.com/search?q=%24500.80+to+yen"
                                MenuItem convertCurrencyMenuItem = new MenuItem();
                                convertCurrencyMenuItem.Header = "Convert Currency To";
                                foreach (MpCurrencyType ct in Enum.GetValues(typeof(MpCurrencyType))) {
                                    if (ct == MpCurrencyType.None || ct == MpHelpers.GetCurrencyTypeFromString(linkText)) {
                                        continue;
                                    }
                                    MenuItem subItem = new MenuItem();
                                    subItem.Header = Enum.GetName(typeof(MpCurrencyType), ct);
                                    subItem.Click += (s2, e2) => {
                                        // use https://free.currencyconverterapi.com/ instead of google
                                        //string convertedCurrency = MpHelpers.CurrencyConvert(
                                        //    (decimal)MpHelpers.GetCurrencyValueFromString(linkText),
                                        //    Enum.GetName(typeof(MpCurrencyType), MpHelpers.GetCurrencyTypeFromString(linkText)),
                                        //    Enum.GetName(typeof(MpCurrencyType), ct));
                                        //hyperlink.Inlines.Clear();
                                        //hyperlink.Inlines.Add(new Run(convertedCurrency));
                                        ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).HideWindowCommand.Execute(null);
                                        System.Diagnostics.Process.Start(@"https://www.google.com/search?q=" + linkText + "+to+" + subItem.Header);
                                    };
                                    convertCurrencyMenuItem.Items.Add(subItem);
                                }

                                hyperlink.ContextMenu.Items.Add(convertCurrencyMenuItem);
                                break;
                            default:

                                break;
                        }
                    }
                    //return;
                    //var newDocument = (MpEventEnabledFlowDocument)MpHelpers.ConvertRtfToFlowDocument((string)e.NewValue);
                    ////instead of directly setting document this workaround ensures document reassignment doesn't fail
                    //TextRange newRange = new TextRange(newDocument.ContentStart, newDocument.ContentEnd);
                    //using (MemoryStream stream = new MemoryStream()) {
                    //    System.Windows.Markup.RtfWriter.Save(newRange, stream);
                    //    newRange.Save(stream, DataFormats.RtfPackage);

                    //    var doc = new MpEventEnabledFlowDocument();
                    //    var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                    //    range.Load(stream, DataFormats.RtfPackage);

                    //    // Set the document and refresh its links
                    //    rtb.Document =  (MpEventEnabledFlowDocument)doc;

                    //}                    
                }
            });
    }
}
