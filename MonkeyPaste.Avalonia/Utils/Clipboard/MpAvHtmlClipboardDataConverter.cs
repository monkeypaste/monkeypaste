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
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvHtmlClipboardDataConverter {
        public static MpAvCefNetWebView RootWebView { get; private set; }

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
            RootWebView = new MpAvCefNetWebView() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsVisible = false
            };
            quillWindow.Content = RootWebView;
            
            RootWebView.BrowserCreated += (s, e) => {
                RootWebView.Navigated += async(s, e) => {
                    if (s is MpAvCefNetWebView wv) {
                        while(!wv.IsDomLoaded) {
                            await Task.Delay(100);
                        }
                        var converter_init_msg = new MpQuillInitMainRequestMessage() {
                            isPlainHtmlConverter = true,
                            envName = MpPlatformWrapper.Services.OsInfo.OsType.ToString(),
                            useBetterTable = true
                        };
                        string msg64 = converter_init_msg.SerializeJsonObjectToBase64();
                        RootWebView.ExecuteJavascript($"initMain_ext('{msg64}')");
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

        }


        public static async Task<MpAvHtmlClipboardDataConverter> ParseAsync(object htmlData, bool isBase64 = false) {
            if(htmlData == null) {
                return null;
            }
            string htmlDataStr = null;
            if(htmlData is byte[] htmlDataBytes) {
                isBase64 = true;
                htmlDataStr = htmlDataBytes.ToBase64String();
            } else {
                htmlDataStr = htmlData.ToString();
                //htmlDataStr = htmlDataStr.ToByteArray(Encoding.UTF32).ToBase64String();
                //isBase64 = true;
            }
            if(string.IsNullOrWhiteSpace(htmlDataStr)) {
                MpConsole.WriteTraceLine("Error parsing html data obj, no data found");
                Debugger.Break();
                return null;
            }

            var req = new MpQuillConvertPlainHtmlToQuillHtmlRequestMessage() { 
                data = htmlDataStr,
                isBase64 = isBase64,
                isHtmlClipboardFormat = true
            };
            string respStr = await RootWebView.EvaluateJavascriptAsync($"convertPlainHtml_ext('{req.SerializeJsonObjectToBase64()}')");
            var resp = MpJsonObject.DeserializeBase64Object<MpQuillConvertPlainHtmlToQuillHtmlResponseMessage>(respStr);
            return new MpAvHtmlClipboardDataConverter() {
                Html = resp.quillHtml,
                SourceUrl = resp.sourceUrl
            };
        }

        public static ICommand ShowConverterDevTools => new MpCommand(
            () => {
                RootWebView.ShowDevTools(); 
            }, () => { return RootWebView != null; });
    }
}
