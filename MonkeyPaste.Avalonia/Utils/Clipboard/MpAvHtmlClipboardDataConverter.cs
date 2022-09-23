using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using CefNet.Avalonia;
using MonkeyPaste.Avalonia.Utils.ToolWindow.Win;
using MonkeyPaste.Common;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MonkeyPaste.Avalonia {
    public class MpAvHtmlClipboardDataConverter {
        public static WebView RootWebView { get; private set; }

        public string Version { get; private set; }
        public string SourceUrl { get; private set; }
        public string Html { get; private set; }

        public static void Init() {
            if(!MpAvCefNetApplication.UseCefNet) {
                return;
            }

            var quillWindow = new Window() {
                Width = 300,
                Height = 300,
                ShowInTaskbar = false,
                SystemDecorations = SystemDecorations.None,
                Position = new PixelPoint(808080, 808080)
            };
            RootWebView = new WebView() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsVisible = false
            };
            quillWindow.Content = RootWebView;
            
            RootWebView.BrowserCreated += (s, e) => {
                RootWebView.Navigated += (s, e) => {
                    if (s is WebView wv) {
                        var converter_init_msg = new MpQuillLoadRequestMessage() {
                            isEditorPlainHtmlConverter = true
                        };
                        RootWebView.ExecuteJavascript($"init_ext('{converter_init_msg.SerializeJsonObjectToBase64}')");
                    }
                };
                RootWebView.Navigate(MpAvClipTrayViewModel.EditorPath);
            };

            if(OperatingSystem.IsWindows()) {
                // hide converter window from windows alt-tab menu
                quillWindow.AttachedToVisualTree += (s, e) => {
                    MpAvToolWindow_Win32.InitToolWindow(quillWindow.PlatformImpl.Handle.Handle);
                    quillWindow.Hide();
                };
            }

            quillWindow.Show();
        }


        public static async Task<MpAvHtmlClipboardDataConverter> ParseAsync(string htmlClipboardData) {
            await Task.Delay(1);
            if(string.IsNullOrWhiteSpace(htmlClipboardData)) {
                return null;
            }

            var hcd = new MpAvHtmlClipboardDataConverter();

            //string versionToken = @"Version:";
            //string startHtmlToken = @"StartHTML:";
            //string endHtmlToken = @"EndHTML:";

            string htmlStartToken = @"<!--StartFragment-->";
            string htmlEndToken = @"<!--EndFragment-->";
            

            int html_start_idx = htmlClipboardData.IndexOf(htmlStartToken) + htmlStartToken.Length;
            if(html_start_idx >= 0) {
                int html_end_idx = htmlClipboardData.IndexOf(htmlEndToken);
                int html_length = html_end_idx - html_start_idx;

                if(html_length > 0) {
                    string plainHtml = htmlClipboardData.Substring(html_start_idx, html_length);
                    plainHtml = HttpUtility.HtmlDecode(plainHtml);
                    if(RootWebView == null) {
                        // occurs when CefNet is disabled (hidden window not created in init)
                        hcd.Html = plainHtml;
                    } else {

                        var plainHtmlToRichHtmlRequest = new MpQuillConvertPlainHtmlToQuillHtmlRequestMessage() { plainHtml = plainHtml };
                        string qhtml = await RootWebView.EvaluateJavascriptAsync($"convertPlainHtml_ext('{plainHtmlToRichHtmlRequest.SerializeJsonObjectToBase64()}')");

                        if (qhtml.IsStringEscapedHtml()) {
                            // pretty sure this can't happen since its base64 encoded but curious to see, this means need to call HtmlDecode again..
                            Debugger.Break();
                            qhtml = HttpUtility.HtmlDecode(qhtml);
                        }
                        hcd.Html = qhtml;
                        //hcd.Rtf = string.Empty;// await MpQuillHtmlToRtfConverter.ConvertStandardHtmlToRtf(hcd.Html);
                    }
                }
            }
            string sourceUrlToken = "SourceURL:";
            int source_url_start_idx = htmlClipboardData.IndexOf(sourceUrlToken) + sourceUrlToken.Length;
            if(source_url_start_idx >= 0) {
                int source_url_length = htmlClipboardData.Substring(source_url_start_idx).IndexOf(Environment.NewLine);
                if(source_url_length >= 0) {
                    string parsed_url = htmlClipboardData.Substring(source_url_start_idx, source_url_length);
                    if(Uri.IsWellFormedUriString(parsed_url,UriKind.Absolute)) {
                        hcd.SourceUrl = parsed_url;
                    } else {
                        MpConsole.WriteTraceLine("Malformed uri: " + parsed_url);
                        hcd.SourceUrl = null;
                    }
                }
            }
            return hcd;
        }

        /// <summary>
        /// Extracts Html string from clipboard data by parsing header information in htmlDataString
        /// </summary>
        /// <param name="htmlDataString">
        /// String representing Html clipboard data. This includes Html header
        /// </param>
        /// <returns>
        /// String containing only the Html data part of htmlDataString, without header
        /// </returns>
        internal static string ExtractHtmlFromClipboardData(string htmlDataString) {
            int startHtmlIndex = htmlDataString.IndexOf("StartHTML:");
            if (startHtmlIndex < 0) {
                return "ERROR: Urecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            startHtmlIndex = Int32.Parse(htmlDataString.Substring(startHtmlIndex + "StartHTML:".Length, "0123456789".Length));
            if (startHtmlIndex < 0 || startHtmlIndex > htmlDataString.Length) {
                return "ERROR: Urecognized html header";
            }

            int endHtmlIndex = htmlDataString.IndexOf("EndHTML:");
            if (endHtmlIndex < 0) {
                return "ERROR: Urecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            endHtmlIndex = Int32.Parse(htmlDataString.Substring(endHtmlIndex + "EndHTML:".Length, "0123456789".Length));
            if (endHtmlIndex > htmlDataString.Length) {
                endHtmlIndex = htmlDataString.Length;
            }

            return htmlDataString.Substring(startHtmlIndex, endHtmlIndex - startHtmlIndex);
        }

        /// <summary>
        /// Extracts selected Html fragment string from clipboard data by parsing header information 
        /// in htmlDataString
        /// </summary>
        /// <param name="htmlDataString">
        /// String representing Html clipboard data. This includes Html header
        /// </param>
        /// <returns>
        /// String containing only the Html selection part of htmlDataString, without header
        /// </returns>
        internal static string ExtractHtmlFragmentFromClipboardData(string htmlDataString) {
            // HTML Clipboard Format
            // (https://msdn.microsoft.com/en-us/library/aa767917(v=vs.85).aspx)

            // The fragment contains valid HTML representing the area the user has selected. This 
            // includes the information required for basic pasting of an HTML fragment, as follows:
            //  - Selected text. 
            //  - Opening tags and attributes of any element that has an end tag within the selected text. 
            //  - End tags that match the included opening tags. 

            // The fragment should be preceded and followed by the HTML comments <!--StartFragment--> and 
            // <!--EndFragment--> (no space allowed between the !-- and the text) to indicate where the 
            // fragment starts and ends. So the start and end of the fragment are indicated by these 
            // comments as well as by the StartFragment and EndFragment byte counts. Though redundant, 
            // this makes it easier to find the start of the fragment (from the byte count) and mark the 
            // position of the fragment directly in the HTML tree.

            // Byte count from the beginning of the clipboard to the start of the fragment.
            int startFragmentIndex = htmlDataString.IndexOf("StartFragment:");
            if (startFragmentIndex < 0) {
                return "ERROR: Unrecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            startFragmentIndex = Int32.Parse(htmlDataString.Substring(startFragmentIndex + "StartFragment:".Length, 10));
            if (startFragmentIndex < 0 || startFragmentIndex > htmlDataString.Length) {
                return "ERROR: Unrecognized html header";
            }

            // Byte count from the beginning of the clipboard to the end of the fragment.
            int endFragmentIndex = htmlDataString.IndexOf("EndFragment:");
            if (endFragmentIndex < 0) {
                return "ERROR: Unrecognized html header";
            }
            // TODO: We assume that indices represented by strictly 10 zeros ("0123456789".Length),
            // which could be wrong assumption. We need to implement more flrxible parsing here
            endFragmentIndex = Int32.Parse(htmlDataString.Substring(endFragmentIndex + "EndFragment:".Length, 10));
            if (endFragmentIndex > htmlDataString.Length) {
                endFragmentIndex = htmlDataString.Length;
            }

            // CF_HTML is entirely text format and uses the transformation format UTF-8
            byte[] bytes = Encoding.UTF8.GetBytes(htmlDataString);
            return Encoding.UTF8.GetString(bytes, startFragmentIndex, endFragmentIndex - startFragmentIndex);
        }
    }
}
