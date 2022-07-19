using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using WebViewControl;

namespace MonkeyPaste.Avalonia {
    public class MpAvWebViewContentView : MpAvContentViewBase {
        private WebView _webview;

        public string EditorPath {
            get {
                //file:///Volumes/BOOTCAMP/Users/tkefauver/Source/Repos/MonkeyPaste/MonkeyPaste/Resources/Html/Editor/index.html
                //string editorPath = Path.Combine(Environment.CurrentDirectory, "Resources", "Html", "Editor", "index.html");
                string editorPath = @"file:///C:/Users/tkefauver/Source/Repos/MonkeyPaste/MonkeyPaste/Resources/Html/Editor/index.html";
                if (OperatingSystem.IsWindows()) {
                    return editorPath;
                }
                var uri = new Uri(editorPath, UriKind.Absolute);
                string uriStr = uri.AbsoluteUri;
                return uriStr;
            }
        }

        public override Control ContentControl => _webview;

        public MpAvWebViewContentView() {
            _webview = new WebView() {
                Address = EditorPath
            };
        }


        public override void SetContent(string content) {
            Dispatcher.UIThread.Post(async () => {
                while(!_webview.IsJavascriptEngineInitialized) {
                    await Task.Delay(100);
                }
                
                await Task.Delay(3000);
                try {
                    bool isLoaded = await _webview.EvaluateScriptFunctionInFrame<bool>("checkIsLoaded", string.Empty);
                    while (!isLoaded) {
                        isLoaded = await _webview.EvaluateScriptFunctionInFrame<bool>("checkIsLoaded", string.Empty); //_webview.EvaluateScriptFunction<bool>("checkIsLoaded");
                    }
                    _webview.ExecuteScriptFunctionInFrame($"setText", $"{content}", string.Empty);
                }catch(Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    SetContent(content);
                    return;
                }
                
            });
        }
    }
}
