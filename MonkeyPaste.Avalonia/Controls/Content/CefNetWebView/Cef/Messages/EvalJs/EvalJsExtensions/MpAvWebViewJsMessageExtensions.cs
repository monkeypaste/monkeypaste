#if CEFNET_WV

using Avalonia.Threading;
using CefNet;
using CefNet.Avalonia;
using MonkeyPaste.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvWebViewJsMessageExtensions {
        #region Private Variables
        private static readonly ConcurrentDictionary<WebView, ConcurrentDictionary<string, string>> _webViewEvalJsLookup = new();

        #endregion

        #region WebView Extensions

        public static int PendingEvalCount(this WebView wv) {
            var kvp = GetJsPendingMessageLookup(wv);
            if (kvp == null) {
                return 0;
            }
            return kvp.Values.Count;
        }

        public static async Task<string> EvaluateJavascriptAsync(this WebView wv, string script) {
            if (wv is MpAvIDomStateAwareWebView dom_state_wv) {
                while (!dom_state_wv.IsDomLoaded) {
                    await Task.Delay(100);
                }
            }

            string evalKey = System.Guid.NewGuid().ToString();
            var _evalResultLookup = GetJsPendingMessageLookup(wv);
            if (_evalResultLookup.ContainsKey(evalKey)) {
                // shouldn't happen
                MpDebug.Break();
            }
            _evalResultLookup.TryAdd(evalKey, null);

            int max_attempts = 100;
            int attempt = 0;

            while (attempt <= max_attempts) {
                string resp = await wv.EvaluateJavascriptAsync_helper(script, evalKey);
                bool is_valid = resp != null && resp != MpAvCefNetApplication.JS_REF_ERROR;

                if (wv is MpAvIAsyncJsEvalWebView async_wv &&
                    async_wv.NeedsEvalJsCleared) {
                    // wv is being recycled so clear evals and breakout here
                    ClearWebViewEvals(wv);

                    return null;
                }
                if (is_valid) {
                    _ = _evalResultLookup.TryRemove(evalKey, out _);
                    return resp;
                }
                _evalResultLookup[evalKey] = null;
                attempt++;
                //MpConsole.WriteLine($"retrying '{script}' w/ key:'{evalKey}' attempt#:{attempt}");
                await Task.Delay(100);
            }

            // NOTE Reasons for failures:
            // 1. I think this happens from drag/drop using fileItem and redirecting to file 
            // what else?
            MpConsole.WriteLine($"retry count exceeded for '{script}' w/ key:'{evalKey}' attempts#:{attempt}");
            MpDebug.Break();
            if (wv is MpAvIReloadableContentWebView reloadable_wv) {
                reloadable_wv.ReloadContentAsync().FireAndForgetSafeAsync();
            }


            //MpDebug.Break();
            return null;
        }


        public static void ExecuteJavascript(this WebView wv, string script) {
            var frame = wv.GetMainFrame();
            if (frame == null) {
                TimeSpan exec_timeout = TimeSpan.FromSeconds(5);
                // give frame 5 seconds to init
                Dispatcher.UIThread.Invoke(async () => {
                    var sw = Stopwatch.StartNew();
                    while (frame == null) {
                        if (sw.Elapsed >= exec_timeout) {
                            MpConsole.WriteLine($"frame must be initialized. initialization timed out on item '{wv.DataContext}' for script '{script}'");
                            if (wv.DataContext is MpAvClipTileViewModel ctvm) {
                                bool test = wv == ctvm.GetContentView();
                            }
                            return;
                        }
                        await Task.Delay(100);
                        frame = wv.GetMainFrame();
                    }
                    wv.ExecuteJavascript(script);
                    return;
                });
                return;
            }
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => {
                    wv.ExecuteJavascript(script);
                });
                return;
            }
            frame.ExecuteJavaScript(script, frame.Url, 0);
        }

        public static void SetJavascriptResult(this WebView wv, string evalKey, string result) {
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
                MpDebug.Break();
            }
        }

        #endregion

        #region Private Methods

        private static async Task<string> EvaluateJavascriptAsync_helper(this WebView wv, string script, string evalKey) {
            // create evaljs request to be picked up by HandleRequest on cef renderer thread
            var frame = wv.GetMainFrame();
            if (frame == null) {
                return null;
            }

            string resp = null;
            if (!Dispatcher.UIThread.CheckAccess()) {
                resp = await Dispatcher.UIThread.InvokeAsync<string>(async () => {
                    return await wv.EvaluateJavascriptAsync(script);
                });
                return resp;
            }

            CefProcessMessage cefMsg = new("EvaluateScript");
            evalKey = string.IsNullOrEmpty(evalKey) ? System.Guid.NewGuid().ToString() : evalKey;
            cefMsg.ArgumentList.SetString(0, evalKey);
            cefMsg.ArgumentList.SetString(1, script);
            frame.SendProcessMessage(CefProcessId.Renderer, cefMsg);

            var _evalResultLookup = GetJsPendingMessageLookup(wv);
            while (true) {
                if (_evalResultLookup == null || !_evalResultLookup.ContainsKey(evalKey)) {
                    // one case for ths is wv is being recycled and calling method has already acknowledge and removed this requeset
                    return null;
                }
                if (wv is MpAvIAsyncJsEvalWebView async_wv &&
                    async_wv.NeedsEvalJsCleared) {
                    // break out of this eval
                    MpConsole.WriteLine("wv unload caught in eval helper, returning null");
                    return null;
                }
                if (_evalResultLookup[evalKey] == null) {
                    await Task.Delay(100);
                } else {
                    return _evalResultLookup[evalKey];
                }

            }
        }
        private static ConcurrentDictionary<string, string> GetJsPendingMessageLookup(WebView wv) {
            if (!_webViewEvalJsLookup.ContainsKey(wv)) {
                // remove lookup if wv disposed
                AddWebView(wv);
            }
            return _webViewEvalJsLookup[wv];
        }

        private static void Wv_DetachedFromVisualTree(object sender, global::Avalonia.VisualTreeAttachmentEventArgs e) {
            if (sender is not WebView wv) {
                return;
            }
            RemoveWebView(wv);
        }

        private static void AddWebView(WebView wv) {
            wv.DetachedFromVisualTree += Wv_DetachedFromVisualTree;
            _webViewEvalJsLookup.TryAdd(wv, new ConcurrentDictionary<string, string>());
        }

        private static void RemoveWebView(WebView wv) {

            wv.DetachedFromVisualTree -= Wv_DetachedFromVisualTree;

            Dispatcher.UIThread.Post(() => {
                if (_webViewEvalJsLookup.Remove(wv, out var pendingEvalLookup)) {
                    MpConsole.WriteLine($"WebView w/ datacontext: '{wv.DataContext}' disposed with '{pendingEvalLookup.Count}' js evaluations pending: ");
                    pendingEvalLookup.ForEach(x => MpConsole.WriteLine($"Key: '{x.Key}' Script: '{x.Value}'"));
                    return;
                }
            });
        }

        private static void ClearWebViewEvals(WebView wv) {
            if (!_webViewEvalJsLookup.ContainsKey(wv)) {
                return;
            }
            int eval_count = _webViewEvalJsLookup[wv].Count;
            _webViewEvalJsLookup[wv].Clear();

            // NOTE reseting this flag here since its the only place that needs to know
            if (wv is MpAvIAsyncJsEvalWebView async_wv) {
                async_wv.NeedsEvalJsCleared = false;
            }
            MpConsole.WriteLine($"{eval_count} pending eval's cleared from wv '{wv.DataContext}' and wv unload flag was reset");
        }
        #endregion
    }
}
#endif