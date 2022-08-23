using Avalonia.Threading;
using CefNet;
using CefNet.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvCefNetWebView : WebView {
        private CancellationTokenSource _cts;

        private ConcurrentDictionary<string, string> _msgLookup = new ConcurrentDictionary<string, string>();
        //private object _lockObj = new object();

        public string _lastResult;
        public bool IsEditorInitialized { get; set; } = false;

        public bool SuppressRightClick { get; set; } = true;
        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            if(e.IsRightPress(this) && SuppressRightClick) {
                return;
            }
            base.OnPointerPressed(e);
        }
        public void SetJavascriptResult(string evalGuid, string result) {
            //lock(_lockObj) {
                //if(_lastResult != null) {
                //    // this should be cleared already
                //    //Debugger.Break();
                //}
                _lastResult = result;
                
                //if (string.IsNullOrEmpty(evalGuid)) {
                //    //should have a key
                //    Debugger.Break();
                //}
                //if (!_msgLookup.ContainsKey(evalGuid)) {
                //    // should have a ref
                //    Debugger.Break();
                //}
                //_msgLookup[evalGuid] = result;
            //}
            
        }        

        public async Task<string> EvaluateJavascriptAsync(string script, string guid = "") {
            var frame = GetMainFrame();
            if (frame == null) {
                return null;
            }
            if (_cts == null) {
                _cts = new CancellationTokenSource();
            }

            if (!Dispatcher.UIThread.CheckAccess()) {
                string result = string.Empty;
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    result = await EvaluateJavascriptAsync(script, guid);
                });
                return result;
            }

            CefProcessMessage cefMsg = new CefProcessMessage("EvaluateScript");

            cefMsg.ArgumentList.SetString(0, guid);
            cefMsg.ArgumentList.SetString(1, script);
            frame.SendProcessMessage(CefProcessId.Renderer, cefMsg);

            while (_lastResult == null) {
                await Task.Delay(100);
            }
            string resp = _lastResult;
            _lastResult = null;
            return resp;
        }
        public void ExecuteJavascript(string script) {
            var frame = GetMainFrame();
            if (frame == null) {
                throw new Exception("frame must be initialized");
            }
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => {
                    ExecuteJavascript(script);
                });
                return;
            }
            frame.ExecuteJavaScript(script, frame.Url, 0);
        }
    }
}
