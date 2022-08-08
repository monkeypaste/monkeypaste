using Avalonia.Threading;
using CefNet;
using CefNet.Avalonia;
using CefNet.JSInterop;
using MonkeyPaste.Common;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class CefNetExtensions {
        public static string LastResult { get; set; }

        //public static void ExecuteJavascript(this WebView wv, string script) {
        //    var frame = wv.GetMainFrame();
        //    if (frame == null) {
        //        throw new Exception("frame must be initialized");
        //    }
        //    if (!Dispatcher.UIThread.CheckAccess()) {
        //         Dispatcher.UIThread.Post( () => {
        //            wv.ExecuteJavascript(script);
        //         });
        //        return;
        //    }
        //    //frame.ExecuteJavaScript(script, frame.Url, 0);

        //    var cefMsg = new CefProcessMessage("EvaluateScript");
        //    cefMsg.ArgumentList.SetString(0, script);
        //    frame.SendProcessMessage(CefProcessId.Renderer, cefMsg);
        //}

        //public static async Task<string> EvaluateJavascriptAsync(this WebView wv, string script) {
        //    var frame = wv.GetMainFrame();
        //    if(frame == null) {
        //        return null;
        //    }
        //    if(!Dispatcher.UIThread.CheckAccess()) {
        //        string result = string.Empty;
        //        await Dispatcher.UIThread.InvokeAsync(async () => {
        //            result = await wv.EvaluateJavascriptAsync(script);
        //        });
        //        return result;
        //    }
            
        //    frame.ExecuteJavaScript(script, frame.Url, 0);

        //    while (LastResult == null) {
        //        await Task.Delay(100);
        //    }
        //    string response = LastResult;
        //    LastResult = null;
        //    return response;

        //    //string response = await GetComOutputAsync(frame);
        //    //return response;

        //    //string response = await GetComOutputFromSource(frame);
        //    //return response;
        //}

        private static async Task<string> GetComOutputFromSource(CefFrame frame) {
            CancellationToken cancellationToken = CancellationToken.None;
            string html = await frame.GetSourceAsync(cancellationToken).ConfigureAwait(false);

            string patternStart = "<textarea id=\"comOutputTextArea\" style=\"display: none\">";
            string patternEnd = "</textarea>";
            var match = Regex.Match(html, patternStart + ".*" + patternEnd, RegexOptions.Multiline);
            if (match.Success) {
                string result = match.Value.Replace(patternStart, string.Empty).Replace(patternEnd, string.Empty);
                if (string.IsNullOrEmpty(result)) {
                    MpConsole.WriteLine("no com output, waiting...");
                    await Task.Delay(100);
                    return await GetComOutputFromSource(frame);
                }
                Dispatcher.UIThread.Post(() => {
                    frame.ExecuteJavaScript("clearComOutput()", frame.Url, 0);
                });
                return result;
            }
            return string.Empty;
        }

        private static async Task<string> GetComOutputAsync(CefFrame frame) {
            CancellationToken cancellationToken = CancellationToken.None;
            dynamic scriptableObject = await frame.GetScriptableObjectAsync(cancellationToken).ConfigureAwait(false);
            // This code must be executed on any background thread. Execution on UI or any CEF thread will result in a deadlock.
            string html = scriptableObject.window.document.documentElement.outerHTML;
            string patternStart = "<textarea id=\"comOutputTextArea\" style=\"display: none\">";
            string patternEnd = "</textarea>";
            var match = Regex.Match(html, patternStart + ".*" + patternEnd, RegexOptions.Multiline);
            if (match.Success) {
                string result = match.Value.Replace(patternStart, string.Empty).Replace(patternEnd, string.Empty);
                if (string.IsNullOrEmpty(result)) {
                    MpConsole.WriteLine("no com output, waiting...");
                    await Task.Delay(100);
                    return await GetComOutputAsync(frame);
                }
                Dispatcher.UIThread.Post(() => {
                    frame.ExecuteJavaScript("clearComOutput()", frame.Url, 0);
                });
                return result;
            }
            return string.Empty;
        }
    }
}
