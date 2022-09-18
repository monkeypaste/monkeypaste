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
using AvaloniaEdit.Utils;
using Avalonia.Interactivity;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvCefNetWebView : 
        WebView, 
        MpAvIContentView {
        #region Private Variables
        private DragDropEffects _curDropEffects { get; set; } = DragDropEffects.None;
        //private string _lastResult;
        private ConcurrentDictionary<string, string> _evalResultLookup = new ConcurrentDictionary<string, string>();

        private int _maxRetryCount = 10;

        #endregion

        #region Statics
        private static MpAvCefNetWebView _openerWebView { get; set; }
        public static MpAvCefNetWebView DraggingRtb { get; private set; } = null;

        public static void InitOpener() {
            var opener = new MpAvCefNetWebView();
            _openerWebView = opener;
        }

        static MpAvCefNetWebView() {
            //IsHitTestVisibleProperty.Changed.AddClassHandler<MpAvCefNetWebView>((x, y) => HandleIsHitTestVisibleChanged(x, y));
        }

        private static void HandleIsHitTestVisibleChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isHitTestVisible &&
                element is MpAvCefNetWebView wv) {
                if(isHitTestVisible) {
                    // when wv has hit test CanDrag is updated through window binding
                    return;
                }
                wv.UpdateDraggable(true);
            }
        }
        #endregion

        #region MpAvIDropHost Implementation

        //bool MpAvIDropHost.IsDropEnabled => true;

        //bool MpAvIDropHost.IsDropValid(IDataObject avdo, MpPoint host_mp, DragDropEffects dragEffects) {
        //    return true;
        //}

        //void MpAvIDropHost.DragOver(MpPoint host_mp, IDataObject avdo, DragDropEffects dragEffects) {
        //    //throw new NotImplementedException();
        //}

        //void MpAvIDropHost.DragLeave() {
        //    this.ExecuteJavascript($"resetDragDrop()");
        //    //throw new NotImplementedException();
        //}

        //async Task<DragDropEffects> MpAvIDropHost.DropDataObjectAsync(IDataObject avdo, MpPoint host_mp, DragDropEffects dragEffects) {
        //    //throw new NotImplementedException();
        //    await Task.Delay(1);
        //    return dragEffects;
        //}


        #endregion

        #region Properties

        public MpAvClipTileViewModel BindingContext => this.DataContext as MpAvClipTileViewModel;
        public bool IsEditorInitialized { get; set; } = false;

        public bool SuppressRightClick { get; set; } = true;

        public bool CanDrag { get; private set; } = true;

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

        #region WebView Binding Methods

        public void UpdateSelection(int index, int length,string text, bool isFromEditor, bool isChangeBegin) {
            var newStart = new MpAvTextPointer(Document, index);
            var newEnd = new MpAvTextPointer(Document, index + length);
            if (isFromEditor) {
                Selection.Start = newStart;
                Selection.End = newEnd;
                Selection.UpdateSelectedTextFromEditor(text);
            } else {
                Selection.Select(newStart, newEnd);
            }
            MpMessageType selChangeMsg = isChangeBegin ? MpMessageType.ContentSelectionChangeBegin : MpMessageType.ContentSelectionChangeEnd;
            MpMessenger.Send(selChangeMsg, DataContext);

            MpConsole.WriteLine($"Tile: '{(DataContext as MpAvClipTileViewModel).CopyItemTitle}' Selection Changed: '{Selection}'");
        }

        public void UpdateDraggable(bool isDraggable) {
            CanDrag = isDraggable;
            MpConsole.WriteLine($"Tile: '{(DataContext as MpAvClipTileViewModel).CopyItemTitle}' Draggable: '{(CanDrag ? "YES":"NO")}'");
        }

        public void UpdateDropEffect(string dropEffectStr) {
            if(string.IsNullOrEmpty(dropEffectStr)) {
                _curDropEffects = DragDropEffects.None;
                return;
            }
            _curDropEffects = dropEffectStr.ToTitleCase().ToEnum<DragDropEffects>();
            MpConsole.WriteLine($"Tile: '{(DataContext as MpAvClipTileViewModel).CopyItemTitle}' Drop Effect: '{_curDropEffects}'");
        }

        public void UpdateSelectionRects(IEnumerable<MpRect> selRects) {
            Selection.RangeRects.Clear();
            if(selRects == null) {
                return;
            }
            Selection.RangeRects.AddRange(selRects);
        }

        public void SelectAll() {
            this.ExecuteJavascript("selectAll_ext()");
        }
        public void DeselectAll() {
            this.ExecuteJavascript($"deselectAll_ext()");
        }
        #endregion

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
            base.OnAttachedToVisualTree(e);
            DragDrop.SetAllowDrop(this, true);
            //this.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            //this.AddHandler(DragDrop.DragOverEvent, DragOver);
            //this.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            //this.AddHandler(DragDrop.DragLeaveEvent, Drop);
        }

        //#region Drag
        //private void DragCheckAndStart_old(PointerPressedEventArgs e) {
        //    MpPoint dc_down_pos = e.GetClientMousePoint(this);
        //    bool is_pointer_dragging = false;
        //    bool was_drag_started = false;

        //    EventHandler<PointerReleasedEventArgs> dragControl_PointerReleased_Handler = null;
        //    EventHandler<PointerEventArgs> dragControl_PointerMoved_Handler = null;

        //    // Drag Control PointerMoved Handler
        //    dragControl_PointerMoved_Handler = (s, e) => {
        //        MpPoint dc_move_pos = e.GetClientMousePoint(this);

        //        var drag_dist = dc_down_pos.Distance(dc_move_pos);
        //        is_pointer_dragging = drag_dist >= MpAvShortcutCollectionViewModel.MIN_GLOBAL_DRAG_DIST;
        //        if (is_pointer_dragging) {

        //            // DRAG START

        //            this.PointerMoved -= dragControl_PointerMoved_Handler;

        //            Dispatcher.UIThread.Post(async () => {
        //                if (CanDrag) {
        //                    was_drag_started = true;
        //                    StartDrag();

        //                    IDataObject avmpdo = await GetWebViewDragDataAsync(false);
        //                    var result = await DragDrop.DoDragDrop(e, avmpdo, DragDropEffects.Copy | DragDropEffects.Move);

        //                    EndDrag();
        //                    this.PointerReleased -= dragControl_PointerReleased_Handler;
        //                    MpConsole.WriteLine("Drag End. Result effect: " + result);
        //                }
        //            });
        //        }
        //    };

        //    // Drag Control PointerReleased Handler
        //    dragControl_PointerReleased_Handler = (s, e) => {
        //        if (was_drag_started) {
        //            // this should not happen, or release is called before drop (if its called at all during drop
        //            // release should be removed after drop
        //            //Debugger.Break();
        //        }

        //        // DRAG END

        //        this.PointerMoved -= dragControl_PointerMoved_Handler;
        //        this.PointerReleased -= dragControl_PointerReleased_Handler;
        //        MpConsole.WriteLine("DragCheck pointer released (was not drag)");

        //        EndDrag();

        //    };

        //    this.PointerReleased += dragControl_PointerReleased_Handler;
        //    this.PointerMoved += dragControl_PointerMoved_Handler;
        //}

        private void StartDrag(PointerPressedEventArgs e) {
            MpAvIContentView cv = BindingContext.GetContentView();
            // add temp key down listener for notifying editor for visual feedback
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += Global_DragKeyUpOrDown;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += Global_DragKeyUpOrDown;

            MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();
            MpAvMainWindowViewModel.Instance.IsDropOverMainWindow = true;
            BindingContext.IsTileDragging = true;

            if (cv is MpAvCefNetWebView wv) {
                // notify editor that its dragging and not just in a drop state
                var dragStartMsg = new MpQuillIsDraggingNotification() { isDragging = true };
                wv.ExecuteJavascript($"updateIsDraggingFromHost_ext('{dragStartMsg.SerializeJsonObjectToBase64()}')");
            }
            Dispatcher.UIThread.Post(async () => {
                IDataObject avmpdo = await GetWebViewDragDataAsync(false);
                var result = await DragDrop.DoDragDrop(e, avmpdo, DragDropEffects.Copy | DragDropEffects.Move);
                EndDrag();
            });
        }
        private void ContinueDrag(PointerEventArgs e) {
            MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();
        }
        private void EndDrag() {
            if (BindingContext.IsTileDragging == false) {
                // can be called twice when esc-canceled (first from StartDrag handler then from the checker pointer up so ignore 2nd
                return;
            }
            MpAvIContentView cv = BindingContext.GetContentView();
            if (cv is MpAvCefNetWebView wv) {
                // notify editor that its dragging and not just in a drop state
                var dragEndMsg = new MpQuillIsDraggingNotification() { isDragging = false };
                wv.ExecuteJavascript($"updateIsDraggingFromHost_ext('{dragEndMsg.SerializeJsonObjectToBase64()}')");
            }
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= Global_DragKeyUpOrDown;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= Global_DragKeyUpOrDown;

            MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = null;
            MpAvMainWindowViewModel.Instance.IsDropOverMainWindow = false;
            BindingContext.IsTileDragging = false;
        }

        private void Global_DragKeyUpOrDown(object sender, string e) {
            if (BindingContext.GetContentView() is MpAvCefNetWebView wv) {
                var modKeyMsg = new MpQuillModifierKeysNotification() {
                    ctrlKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                    altKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown,
                    shiftKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown,
                    escKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown
                };
                wv.ExecuteJavascript($"updateModifierKeysFromHost_ext('{modKeyMsg.SerializeJsonObjectToBase64()}')");
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            // NOTE This only occurs when sub-selection is enabled (bound in container view)
            if (e.IsRightPress(this) && SuppressRightClick) {
                return;
            }

            base.OnPointerPressed(e);
            
            return;


            if (!CanDrag || BindingContext is MpIResizableViewModel rvm && rvm.CanResize) {
                return;
            }
            this.DragCheckAndStart(e, StartDrag, ContinueDrag, EndDrag);
        }

        private async Task<IDataObject> GetWebViewDragDataAsync(bool fillTemplates) {            
            MpAvDataObject avdo = await BindingContext.ConvertToDataObject(fillTemplates);
            if (IsAllSelected()) {
                // only attach internal data format for entire tile
                avdo.SetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT, BindingContext);
            }

            if (avdo.ContainsData(MpPortableDataFormats.Html)) {
                var bytes = avdo.GetData(MpPortableDataFormats.Html) as byte[];
                string htmlStr = Encoding.UTF8.GetString(bytes);
                avdo.SetData("text/html", htmlStr);
            }
            if (avdo.ContainsData(MpPortableDataFormats.Text)) {
                avdo.SetData("text/plain", avdo.GetData(MpPortableDataFormats.Text));
            }
            return avdo;
        }

        private bool IsAllSelected() {
            bool is_all_selected;
            if (BindingContext.IsSubSelectionEnabled) {
                is_all_selected = Selection.Start.Offset == Document.ContentStart.Offset &&
                                    Selection.End.Offset == Document.ContentEnd.Offset;
            } else {
                is_all_selected = true;
            }
            return is_all_selected;
        }

        private bool IsNoneSelected() {
            return Selection.Length == 0;
        }
        //#endregion


        #region Drop
        private void DragEnter(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragEnter] CurDropEffects: " + _curDropEffects);
            SendDropMsg(e.Data, "dragenter");
            e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
            //base.OnDragEnter(e);
        }

        private void DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragOver] CurDropEffects: " + _curDropEffects);
            SendDropMsg(e.Data, "dragover");
            e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
           // base.OnDragOver(e);
        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            SendDropMsg(null, "dragleave");
            MpConsole.WriteLine("[DragLeave] CurDropEffects: " + _curDropEffects);
           // base.OnDragLeave(e);
        }

        private void Drop(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[Drop] CurDropEffects: " + _curDropEffects);
            SendDropMsg(e.Data, "dragleave");
            e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
          //  base.OnDrop(e);
        }

        private void SendDropMsg(IDataObject avdo, string evtType) {
            var jsonMsg = GetDragDropMessage(avdo, evtType);
            this.ExecuteJavascript($"onDragEvent_ext('{jsonMsg.SerializeJsonObjectToBase64()}')");
        }

        private MpQuillDragDropDataObjectMessage GetDragDropMessage(IDataObject avdo, string evtType) {
            var hdobjMsg = new MpQuillDragDropDataObjectMessage() {
                eventType = evtType,
                items = avdo == null ? null : avdo.GetDataFormats()
                                        .Where(x => MpPortableDataFormats.RegisteredFormats.Contains(x))
                                        .Select(x =>
                                            new MpQuillDragDropDataObjectItemFragment() {
                                                format = x,
                                                data = x != MpPortableDataFormats.Html ? avdo.Get(x) as string : (avdo.Get(x) as byte[]).ToBase64String()
                                            }).ToList()
            };
            return hdobjMsg;
        }

        #endregion


        #region Javascript Evaluation

        public void SetJavascriptResult(string evalKey, string result) {
            //if(result == MpAvCefNetApplication.JS_REF_ERROR) {
            //    // ignore 
            //}
            if(_evalResultLookup.ContainsKey(evalKey)) {
                //MpConsole.WriteLine("js eval key " + evalKey + " already has a result pending (replacing).");
                //MpConsole.WriteLine("existing: " + _evalResultLookup[evalKey]);
                //MpConsole.WriteLine("new: " + result);
                _evalResultLookup[evalKey] = result;
                return;
            }
            if(!_evalResultLookup.TryAdd(evalKey,result)) {
               // MpConsole.WriteTraceLine("Js Eval error, couldn't write to lookup, if happens should probably loop here..");
                Debugger.Break();
            }
        }
        public async Task<string> EvaluateJavascriptAsync(string script) {
            string evalKey = System.Guid.NewGuid().ToString();
            if(_evalResultLookup.ContainsKey(evalKey)) {
                // shouldn't happen
                Debugger.Break();
            }
            _evalResultLookup.TryAdd(evalKey, null);

            int max_attempts = 100;
            int attempt = 0;
            
            while (attempt <= max_attempts) {                
                string resp = await EvaluateJavascriptAsync_helper(script, evalKey);
                bool is_valid = resp != null && resp != MpAvCefNetApplication.JS_REF_ERROR;
                if(is_valid) {
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
        private async Task<string> EvaluateJavascriptAsync_helper(string script,string evalKey, int retryAttempt = 0) {
            var frame = GetMainFrame();
            if (frame == null) {
                return null;
            }

            string resp = null;
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    resp = await EvaluateJavascriptAsync(script);
                });
                return resp;
            }
            
            CefProcessMessage cefMsg = new CefProcessMessage("EvaluateScript");
            evalKey = string.IsNullOrEmpty(evalKey) ? System.Guid.NewGuid().ToString() : evalKey;
            cefMsg.ArgumentList.SetString(0, evalKey);
            cefMsg.ArgumentList.SetString(1, script);
            frame.SendProcessMessage(CefProcessId.Renderer, cefMsg);

            while(_evalResultLookup[evalKey] == null) {
                await Task.Delay(100);
            }
            return _evalResultLookup[evalKey];
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
