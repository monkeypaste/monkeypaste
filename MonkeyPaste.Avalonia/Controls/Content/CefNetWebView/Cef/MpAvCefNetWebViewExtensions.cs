using Avalonia.Controls;
using Avalonia.Threading;
using CefNet;
using CefNet.Avalonia;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvCefNetWebViewExtensions {
        #region Javascript Evaluation

        public static void SetJavascriptResult(this WebView wv, string evalKey, string result) {
            //if(result == MpAvCefNetApplication.JS_REF_ERROR) {
            //    // ignore 
            //}

            var _evalResultLookup = wv.GetJsPendingMessageLookup();
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
        public static async Task<string> EvaluateJavascriptAsync(this WebView wv, string script) {
            string evalKey = System.Guid.NewGuid().ToString();
            var _evalResultLookup = wv.GetJsPendingMessageLookup();
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
            Debugger.Break();
            return MpAvCefNetApplication.JS_REF_ERROR;
        }
        private static async Task<string> EvaluateJavascriptAsync_helper(this WebView wv, string script, string evalKey, int retryAttempt = 0) {
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

            var _evalResultLookup = wv.GetJsPendingMessageLookup();
            while (_evalResultLookup[evalKey] == null) {
                await Task.Delay(100);
            }
            return _evalResultLookup[evalKey];
        }


        public static void ExecuteJavascript(this WebView wv,string script) {
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

        private static ConcurrentDictionary<string,string> GetJsPendingMessageLookup(this WebView wv) {
            if(wv.Tag is not ConcurrentDictionary<string,string>) {
                wv.Tag = new ConcurrentDictionary<string, string>();
            }
            return wv.Tag as ConcurrentDictionary<string, string>;
        }

        #endregion

        public static MpAvITextRange ContentRange(this MpAvHtmlDocument doc) {
            return new MpAvTextRange(doc.ContentStart, doc.ContentEnd);
        }

        public static MpAvITextRange ToTextRange(this MpAvITextPointer tp) {
            return new MpAvTextRange(tp,tp);
        }       


        public static void LoadImage(this MpAvITextRange tr,string base64Str, out MpSize size) {
            size = new MpSize();
        }

        public static void LoadItemData(this MpAvITextRange tr, string data, MpCopyItemType itemType, out MpSize size) {
            size = new MpSize();
            tr.SetTextAsync(data).FireAndForgetSafeAsync((tr.Start.Document.Owner as Control).DataContext as MpViewModelBase);
        }
        public static void LoadTable(this MpAvITextRange tr, string csvStr) {

            tr.SetTextAsync(csvStr).FireAndForgetSafeAsync((tr.Start.Document.Owner as Control).DataContext as MpViewModelBase);
        }

        public static async Task<string> ToEncodedPlainTextAsync(this MpAvITextRange tr) {
            //if (tr.IsEmpty) {
            //    return string.Empty;
            //}
            //var templatesToEncode = tr.GetAllTextElements()
            //                                    .Where(x => x is InlineUIContainer && x.Tag is MpTextTemplate)
            //                                    .OrderBy(x => tr.Start.GetOffsetToPosition(x.ContentStart))
            //                                    .ToList();
            //if (templatesToEncode.Count() == 0) {
            //    return tr.Text;
            //}

            //var sb = new StringBuilder();
            //var ctp = tr.Start;
            //foreach (var te in templatesToEncode) {
            //    var ntp = te.ElementStart;
            //    sb.Append(new TextRange(ctp, ntp).Text);
            //    sb.Append((te.Tag as MpTextTemplate).EncodedTemplate);
            //    ctp = te.ElementEnd;
            //    if (te == templatesToEncode.Last()) {
            //        sb.Append(new TextRange(ctp, tr.End).Text);
            //    }
            //}
            //return sb.ToString();
            string text = await tr.GetTextAsync();
            return text;
        }

        public static async Task<string> ToEncodedRichTextAsync(this MpAvITextRange tr) {
            //if (tr.IsEmpty) {
            //    return string.Empty;
            //}
            //var templatesToEncode = tr.GetAllTextElements()
            //                                    .Where(x => x is MpTextTemplateInlineUIContainer && x.Tag is MpTextTemplate)
            //                                    .OrderBy(x => tr.Start.GetOffsetToPosition(x.ContentStart))
            //                                    .Cast<MpTextTemplateInlineUIContainer>()
            //                                    .ToList();
            //var doc = tr.Start.Parent.FindParentOfType<FlowDocument>();
            //var clonedDoc = doc.Clone(tr, out TextRange encodedRange);

            //return encodedRange.ToRichText();
            string text = await tr.GetTextAsync();
            return text;
        }
    }
}
