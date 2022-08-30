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
    public class MpAvCefNetWebView : WebView, MpAvIContentView {
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

        //public MpAvTextSelection Selection { 
        //    get {
        //        string selJsonStr = this.EvaluateJavascript("getSelection()");
        //        var selParts = MpJsonObject.DeserializeObject<List<int>>(selJsonStr);
        //        return new MpAvTextRange(
        //            new MpAvTextPointer(Document, selParts[0]),
        //            new MpAvTextPointer(Document, selParts[0] + selParts[1])) as MpAvTextSelection;
        //    }
        //    private set {
        //        int[] selVal = value == null ? new int[] { 0, 0 } : new int[] { value.Start.Offset, value.End.Offset };
        //        string selJsonStr = string.Format(@"{index:{0}, length:{1}}", selVal[0], selVal[1] - selVal[0]);
        //        this.ExecuteJavascript($"setSelection('{selJsonStr}')");
        //    }
        //}

        public MpAvTextSelection Selection { get; private set; }

        public MpAvHtmlDocument Document { get; set; }

        MpAvIContentDocument MpAvIContentView.Document => Document;

        public IList<RoutedCommandBinding> CommandBindings { get; } = new List<RoutedCommandBinding>();
        #endregion

        #region Constructors
        public MpAvCefNetWebView() : base() {
            Document = new MpAvHtmlDocument(this);
            Selection = new MpAvTextSelection(Document);

            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Copy, OnCopy, OnCanExecuteClipboardCommand));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Cut, OnCut, OnCanExecuteClipboardCommand));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Paste, OnPaste, OnCanExecuteClipboardCommand));
        }

        #endregion

        #region Public Methods

        public void UpdateSelection(int index, int length, bool isFromEditor) {
            var newStart = new MpAvTextPointer(Document, index);
            var newEnd = new MpAvTextPointer(Document, index + length);
            if (isFromEditor) {
                Selection.Start = newStart;
                Selection.End = newEnd;
            } else {
                Selection.Select(newStart, newEnd);
            }
            MpConsole.WriteLine($"Tile: '{(DataContext as MpAvClipTileViewModel).CopyItemTitle}' Selection Changed: '{Selection}'");
        }

        void MpAvIContentView.SelectAll() {
            this.ExecuteJavascript("selectAll()");
        }

        #endregion


        #region Overrides


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
            e.Handled = true;
            wv.SetClipboardDataAsync(true).FireAndForgetSafeAsync(wv.DataContext as MpViewModelBase);
        }

        private static void OnCut(object sender, ExecutedRoutedEventArgs e) {
            MpAvCefNetWebView wv = (MpAvCefNetWebView)sender;
            e.Handled = true;
            wv.SetClipboardDataAsync(false).FireAndForgetSafeAsync(wv.DataContext as MpViewModelBase);
        }

        private static void OnPaste(object sender, ExecutedRoutedEventArgs e) {
            MpAvCefNetWebView wv = (MpAvCefNetWebView)sender;

            e.Handled = true;
            Dispatcher.UIThread.Post(async () => {
                string cb_text = await Application.Current.Clipboard.GetTextAsync();
                await wv.Selection.SetTextAsync(cb_text);

                if (wv.DataContext is MpAvClipTileViewModel ctvm) {
                    MpAvCefNetWebViewExtension.SaveTextContentAsync(wv)
                        .FireAndForgetSafeAsync(ctvm);
                }
            });            
        }

        #endregion

        protected async Task SetClipboardDataAsync(bool isCopy) {
            var ctvm = DataContext as MpAvClipTileViewModel;
            if (Selection.IsEmpty) {
                MpAvTextBoxSelectionExtension.SelectAll(ctvm);
            }
            string selectedText = await MpAvCefNetWebViewExtension.GetEncodedContentAsync(this, false, true);

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
