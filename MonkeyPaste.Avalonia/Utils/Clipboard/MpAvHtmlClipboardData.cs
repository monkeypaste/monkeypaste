using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using CefNet.Avalonia;
using MonkeyPaste.Avalonia.Utils.ToolWindow.Win;
using MonkeyPaste.Common;
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
                ConverterWebView.Navigate(MpAvClipTrayViewModel.EditorPath);
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


        public static async Task<MpAvHtmlClipboardData> ParseAsync(object htmlData, bool isBase64 = false) {
            if(htmlData == null) {
                return null;
            }
            string htmlDataStr = null;
            if (htmlData is byte[] htmlDataBytes) {
                isBase64 = true;
                htmlDataStr = htmlDataBytes.ToBase64String();
            } else {
                if(htmlData is not string) {
                    // what format is it?
                    Debugger.Break();
                    return null;
                }
                if(isBase64) {
                    htmlDataStr = htmlData.ToString();
                } else {
                    htmlDataStr = htmlData.ToString().ToBase64String();
                    isBase64 = true;
                }
                
            }
            if(string.IsNullOrWhiteSpace(htmlDataStr)) {
                MpConsole.WriteTraceLine("Error parsing html data obj, no data found");
                Debugger.Break();
                return null;
            }

            var req = new MpQuillConvertPlainHtmlToQuillHtmlRequestMessage() { 
                data = htmlDataStr,
                isBase64 = isBase64
            };
            string respStr = await ConverterWebView.EvaluateJavascriptAsync($"convertPlainHtml_ext('{req.SerializeJsonObjectToBase64()}')");
            var resp = MpJsonObject.DeserializeBase64Object<MpQuillConvertPlainHtmlToQuillHtmlResponseMessage>(respStr);
            return new MpAvHtmlClipboardData() {
                Html = resp.quillHtml,
                SourceUrl = resp.sourceUrl
            };
        }

        public static ICommand ShowConverterDevTools => new MpCommand(
            () => {
                ConverterWebView.ShowDevTools(); 
            });

    }
}
