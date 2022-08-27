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
using AvaloniaEdit;
using CsvHelper;
using Avalonia;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvCefNetWebView : WebView {
        #region Private Variables

        private string _lastResult;
        private int _maxRetryCount = 10;

        #endregion

        #region Statics

        public static MpAvCefNetWebView DraggingRtb { get; private set; } = null;

        #endregion

        #region Properties

        public bool IsEditorInitialized { get; set; } = false;

        public bool SuppressRightClick { get; set; } = true;

        public MpAvTextSelection Selection { get; private set; }

        public MpAvHtmlDocument Document { get; set; }

        public IList<RoutedCommandBinding> CommandBindings { get; } = new List<RoutedCommandBinding>();
        #endregion

        #region Overrides

        public MpAvCefNetWebView() : base() {
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Copy, OnCopy,OnCanExecuteClipboardCommand));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Cut, OnCut, OnCanExecuteClipboardCommand));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Paste, OnPaste, OnCanExecuteClipboardCommand));
        }

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            if(e.IsRightPress(this) && SuppressRightClick) {
                return;
            }
            base.OnPointerPressed(e);
        }

        #endregion

        #region Javascript Evaluation

        public void SetJavascriptResult(string result) {
            _lastResult = result;
        }        

        public async Task<string> EvaluateJavascriptAsync(string script, int retryAttempt = 0) {
            var frame = GetMainFrame();
            if (frame == null) {
                return null;
            }

            if (!Dispatcher.UIThread.CheckAccess()) {
                string result = string.Empty;
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    result = await EvaluateJavascriptAsync(script);
                });
                return result;
            }

            CefProcessMessage cefMsg = new CefProcessMessage("EvaluateScript");
            cefMsg.ArgumentList.SetString(0, script);
            frame.SendProcessMessage(CefProcessId.Renderer, cefMsg);

            while (_lastResult == null) {
                if(_lastResult == MpCefNetApplication.JS_REF_ERROR) {
                    _lastResult = null;
                    EvaluateJavascriptAsync(script, retryAttempt + 1).FireAndForgetSafeAsync(DataContext as MpViewModelBase);
                }
                await Task.Delay(100);
            }
            string resp = _lastResult;
            if(retryAttempt == 0) {
                // only let the initial call clear the result
                _lastResult = null;
            }
            return resp;
        }

        public string EvaluateJavascript(string script) {
            string result = MpAsyncHelpers.RunSync<string>(()=>EvaluateJavascriptAsync(script));
            return result;
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

        #endregion

        #region Application Commands

        private static void OnCanExecuteClipboardCommand(object target, CanExecuteRoutedEventArgs args) {
            MpAvCefNetWebView wv = (MpAvCefNetWebView)target;
            args.CanExecute = wv.IsEnabled;
        }

        private static void OnCopy(object sender, ExecutedRoutedEventArgs e) {
            MpAvCefNetWebView wv = (MpAvCefNetWebView)sender;
            wv.SetClipboardData(true);
            e.Handled = true;
        }

        private static void OnCut(object sender, ExecutedRoutedEventArgs e) {
            MpAvCefNetWebView wv = (MpAvCefNetWebView)sender;
            wv.SetClipboardData(false);
            e.Handled = true;
        }

        private static void OnPaste(object sender, ExecutedRoutedEventArgs e) {
            MpAvCefNetWebView wv = (MpAvCefNetWebView)sender;

            e.Handled = true;
            Dispatcher.UIThread.Post(async () => {
                wv.Selection.Text = await Application.Current.Clipboard.GetTextAsync();


                if (wv.DataContext is MpAvClipTileViewModel ctvm) {
                    MpAvCefNetWebViewExtension.SaveTextContentAsync(wv)
                        .FireAndForgetSafeAsync(ctvm);
                }
            });            
        }

        #endregion

        protected void SetClipboardData(bool isCopy) {
            var ctvm = DataContext as MpAvClipTileViewModel;
            if (Selection.IsEmpty) {
                MpAvTextBoxSelectionExtension.SelectAll(ctvm);
            }
            string selectedText = MpAvCefNetWebViewExtension.GetEncodedContent(this, false, true);

            MpPlatformWrapper.Services.ClipboardMonitor.IgnoreNextClipboardChangeEvent = true;
            Application.Current.Clipboard.SetTextAsync(selectedText)
                .FireAndForgetSafeAsync(ctvm);

            if (!isCopy) {
                MpAvCefNetWebViewExtension.FinishContentCutAsync(ctvm)
                    .FireAndForgetSafeAsync(ctvm);
            }
        }
    }
}
