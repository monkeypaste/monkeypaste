using MonkeyPaste.Common;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using CefNet;
using CefNet.Avalonia;
using System.Collections.Generic;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public static class MpAvWebViewJsMessageExtensions {
        #region Private Variables
        private static ConcurrentDictionary<MpAvCefNetWebView, ConcurrentDictionary<string, string>> _webViewEvalJsLookup = new ConcurrentDictionary<MpAvCefNetWebView, ConcurrentDictionary<string, string>>();

        #endregion


        #region MpAvCefNetWebView Extensions

        public static async Task<string> EvaluateJavascriptAsync(this MpAvCefNetWebView wv, string script) {
            while(!wv.IsDomLoaded) {
                await Task.Delay(100);
            }

            string evalKey = System.Guid.NewGuid().ToString();
            var _evalResultLookup = GetJsPendingMessageLookup(wv);
            if (_evalResultLookup.ContainsKey(evalKey)) {
                // shouldn't happen
                Debugger.Break();
            }
            _evalResultLookup.TryAdd(evalKey, null);

            int max_attempts = 100;
            int attempt = 0;

            while (attempt <= max_attempts) {
                string resp = await wv.EvaluateJavascriptAsync_helper(script, evalKey);
                bool is_valid = resp != null && resp != MpAvCefNetApplication.JS_REF_ERROR;
                if (is_valid) {
                    _evalResultLookup.Remove(evalKey, out string rmStr);
                    return resp;
                }
                _evalResultLookup[evalKey] = null;
                attempt++;
                //MpConsole.WriteLine($"retrying '{script}' w/ key:'{evalKey}' attempt#:{attempt}");
                await Task.Delay(100);
            }

            MpConsole.WriteLine($"retry count exceeded for '{script}' w/ key:'{evalKey}' attempts#:{attempt}");

            if (wv.DataContext is MpAvClipTileViewModel ctvm) {
                MpConsole.WriteLine($"Attempting reload of item: {ctvm.CopyItemTitle}");
                var stateMsg = MpAvCefNetWebViewExtension.GetEditorStateFromClipTile(ctvm);
                if(wv.GetVisualAncestor<MpAvClipTileView>() is MpAvClipTileView ctv) {
                    await ctv.ReloadContentAsync(stateMsg.SerializeJsonObjectToBase64());
                    // should probably try to re eval script here but not sure depending on what it was so keep
                    // looking at the case of failure but it gets here that shows this at least works :)
                    Debugger.Break();
                    var reloaded_result = await wv.EvaluateJavascriptAsync(script);
                    return reloaded_result;
                } else {
                    MpConsole.WriteLine("Reload failed, webview container not found");
                }
            } else {
                MpConsole.WriteLine("Reload failed, webview data context lost");
            }
            
            
            Debugger.Break();
            return MpAvCefNetApplication.JS_REF_ERROR;
        }


        public static void ExecuteJavascript(this WebView wv, string script) {
            var frame = wv.GetMainFrame();
            if (frame == null) {
                throw new Exception("frame must be initialized");
            }
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => {
                    wv.ExecuteJavascript(script);
                });
                return;
            }
            frame.ExecuteJavaScript(script, frame.Url, 0);
        }

        public static void SetJavascriptResult(this MpAvCefNetWebView wv, string evalKey, string result) {
            var _evalResultLookup = GetJsPendingMessageLookup(wv);
            if (_evalResultLookup.ContainsKey(evalKey)) {
                //MpConsole.WriteLine("js eval key " + evalKey + " already has a result pending (replacing).");
                //MpConsole.WriteLine("existing: " + _evalResultLookup[evalKey]);
                //MpConsole.WriteLine("new: " + result);
                _evalResultLookup[evalKey] = result;
                return;
            }
            if (!_evalResultLookup.TryAdd(evalKey, result)) {
                // MpConsole.WriteTraceLine("Js Eval error, couldn't write to lookup, if happens should probably loop here..");
                Debugger.Break();
            }
        }

        #endregion

        #region Private Methods

        private static async Task<string> EvaluateJavascriptAsync_helper(this MpAvCefNetWebView wv, string script, string evalKey, int retryAttempt = 0) {
            // create evaljs request to be picked up by HandleRequest on cef renderer thread
            var frame = wv.GetMainFrame();
            if (frame == null) {
                return null;
            }

            string resp = null;
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    resp = await wv.EvaluateJavascriptAsync(script);
                });
                return resp;
            }

            CefProcessMessage cefMsg = new CefProcessMessage("EvaluateScript");
            evalKey = string.IsNullOrEmpty(evalKey) ? System.Guid.NewGuid().ToString() : evalKey;
            cefMsg.ArgumentList.SetString(0, evalKey);
            cefMsg.ArgumentList.SetString(1, script);
            frame.SendProcessMessage(CefProcessId.Renderer, cefMsg);

            var _evalResultLookup = GetJsPendingMessageLookup(wv);
            while (_evalResultLookup[evalKey] == null) {
                await Task.Delay(100);
            }
            return _evalResultLookup[evalKey];
        }
        private static ConcurrentDictionary<string, string> GetJsPendingMessageLookup(MpAvCefNetWebView wv) {
            if (!_webViewEvalJsLookup.ContainsKey(wv)) {
                // remove lookup if wv disposed
                wv.DetachedFromVisualTree += Wv_DetachedFromVisualTree;
                _webViewEvalJsLookup.TryAdd(wv, new ConcurrentDictionary<string, string>());
            }
            return _webViewEvalJsLookup[wv];
        }

        private static void Wv_DetachedFromVisualTree(object sender, global::Avalonia.VisualTreeAttachmentEventArgs e) {
            var wv = sender as MpAvCefNetWebView;
            if (wv == null) {
                return;
            }
            wv.DetachedFromVisualTree -= Wv_DetachedFromVisualTree;
            if (_webViewEvalJsLookup.Remove(wv, out var pendingEvalLookup)) {
                MpConsole.WriteLine($"MpAvCefNetWebView w/ datacontext: '{wv.DataContext}' disposed with '{pendingEvalLookup.Count}' js evaluations pending: ");
                pendingEvalLookup.ForEach(x => MpConsole.WriteLine($"Key: '{x.Key}' Script: '{x.Value}'"));
                return;
            }
        }

        

        #endregion
    }
}
