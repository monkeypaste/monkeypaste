using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using CefNet.Avalonia;
using MonkeyPaste.Avalonia.Utils.ToolWindow.Win;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
//using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvHtmlClipboardData {
        public static MpAvCefNetWebView ConverterWebView { get; private set; }

        public string Version { get; private set; }
        public string SourceUrl { get; set; }
        public string Html { get; set; }

        public static void Init() {
            if(!MpAvCefNetApplication.UseCefNet) {
                return;
            }
            ConverterWebView = new MpAvCefNetWebView() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            ConverterWebView.BrowserCreated += (s, e) => {
                ConverterWebView.Navigated += async (s, e) => {
                    if(e.Url == MpAvCefNetApplication.BLANK_URL) {
                        return;
                    }
                    if (s is MpAvCefNetWebView wv) {
                        while (!wv.IsDomLoaded) {
                            await Task.Delay(100);
                        }
                        var converter_init_msg = new MpQuillInitMainRequestMessage() {
                            isPlainHtmlConverter = true,
                            envName = MpPlatformWrapper.Services.OsInfo.OsType.ToString()
                        };
                        string msg64 = converter_init_msg.SerializeJsonObjectToBase64();
                        wv.ExecuteJavascript($"initMain_ext('{msg64}')");
                        MpConsole.WriteLine("Plain Html Converter Initialized.");
                    }
                };
                ConverterWebView.Navigate(MpAvClipTrayViewModel.EditorPath+"?converter=true");
            };

            var quillWindow = new Window() {
                Width = 300,
                Height = 300,
                ShowInTaskbar = false,
                SystemDecorations = SystemDecorations.None,
                //Position = new PixelPoint(808080, 808080)
            };
            
            quillWindow.Content = ConverterWebView;

            quillWindow.AttachedToVisualTree += (s, e) => {
                if(OperatingSystem.IsWindows()) {
                    // hide converter window from windows alt-tab menu
                
                    MpAvToolWindow_Win32.InitToolWindow(quillWindow.PlatformImpl.Handle.Handle);
                }
                quillWindow.Hide();       
            };
        }


        public static async Task<MpAvHtmlClipboardData> ParseAsync(string htmlDataStr, string inputFormatType, MpCsvFormatProperties csvProps = null) {
            if(htmlDataStr == null) {
                return null;
            }
            if (!MpAvCefNetApplication.UseCefNet) {
                return ParseWithoutCefFallback(htmlDataStr.ToString());
            }
            if(inputFormatType == "rtf") {
                // create 'dirty' quill html w/ internal converter and treat as plain for quill to parse
                htmlDataStr = htmlDataStr.ToRichHtmlText(MpPortableDataFormats.AvRtf_bytes);
                inputFormatType = "html";
            } else if(inputFormatType == "csv") {
                htmlDataStr = htmlDataStr.ToRichHtmlTable(csvProps);
                inputFormatType = "html";
            } else if(inputFormatType == "text") {
                htmlDataStr = htmlDataStr.Replace(Environment.NewLine, "\n");
            }
            htmlDataStr = htmlDataStr.ToString().ToBase64String();

            if (string.IsNullOrWhiteSpace(htmlDataStr)) {
                MpConsole.WriteTraceLine("Error parsing html data obj, no data found");
                Debugger.Break();
                return null;
            }

            var req = new MpQuillConvertPlainHtmlToQuillHtmlRequestMessage() { 
                data = htmlDataStr,
                dataFormatType = inputFormatType,
                isBase64 = true
            };
            string respStr = await ConverterWebView.EvaluateJavascriptAsync($"convertPlainHtml_ext('{req.SerializeJsonObjectToBase64()}')");
            var resp = MpJsonObject.DeserializeBase64Object<MpQuillConvertPlainHtmlToQuillHtmlResponseMessage>(respStr);
            return new MpAvHtmlClipboardData() {
                Html = resp.quillHtml,
                SourceUrl = resp.sourceUrl
            };
        }

        private static MpAvHtmlClipboardData ParseWithoutCefFallback(string htmlClipboardData) {
            var hcd = new MpAvHtmlClipboardData();

            //string versionToken = @"Version:";
            //string startHtmlToken = @"StartHTML:";
            //string endHtmlToken = @"EndHTML:";

            string htmlStartToken = @"<!--StartFragment-->";
            string htmlEndToken = @"<!--EndFragment-->";


            int html_start_idx = htmlClipboardData.IndexOf(htmlStartToken) + htmlStartToken.Length;
            if (html_start_idx >= 0) {
                int html_end_idx = htmlClipboardData.IndexOf(htmlEndToken);
                int html_length = html_end_idx - html_start_idx;

                if (html_length > 0) {
                    string plainHtml = htmlClipboardData.Substring(html_start_idx, html_length);
                    // BUG if plain html is set and quill is later loaded setHtml won't parse it correctly
                    // (theres escape issues once in js and it just doesn't follow same conversion flow and its annoying, pointless to change) without using dangerouslyPaste
                    // which makes problems w/ templates and since this is plain text mode there's no need to keep html anyways so downsample to plain text
                    hcd.Html = MpAvContentDataConverter.Instance.Convert(plainHtml, null, "plaintext", null) as string;
                }
            }
            string sourceUrlToken = "SourceURL:";
            int source_url_start_idx = htmlClipboardData.IndexOf(sourceUrlToken) + sourceUrlToken.Length;
            if (source_url_start_idx >= 0) {
                int source_url_length = htmlClipboardData.Substring(source_url_start_idx).IndexOf(Environment.NewLine);
                if (source_url_length >= 0) {
                    string parsed_url = htmlClipboardData.Substring(source_url_start_idx, source_url_length);
                    if (Uri.IsWellFormedUriString(parsed_url, UriKind.Absolute)) {
                        hcd.SourceUrl = parsed_url;
                    } else {
                        MpConsole.WriteTraceLine("Malformed uri: " + parsed_url);
                        hcd.SourceUrl = null;
                    }
                }
            }
            return hcd;
        }

        public static ICommand ShowConverterDevTools => new MpCommand(
            () => {
                ConverterWebView.ShowDevTools(); 
            });

    }
}
