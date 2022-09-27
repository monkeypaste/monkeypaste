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
using CsvHelper;
using Avalonia;
using MonkeyPaste.Common;
using Avalonia.Interactivity;
using CefNet.Internal;
using Avalonia.Controls;
using Avalonia.Platform;
using Cairo;

namespace MonkeyPaste.Avalonia {

    public enum MpAvEditorBindingFunctionType {
        getDragData,
        getAllTemplatesFromDb,
        notifyEditorSelectionChanged,
        notifyContentLengthChanged,
        notifySubSelectionEnabledChanged,
        notifyContentDraggableChanged,
        notifyDropEffectChanged,
        notifyException,
        notifyDragStartOrEnd,
        notifyReadOnlyChanged,
        notifyDomLoaded,
        notifyDropCompleted,
        notifyDragEnter,
        notifyDragLeave
    }
    [DoNotNotify]
    public class MpAvCefNetWebView : 
        WebView, 
        MpAvIContentView {
        #region Private Variables
        private DragDropEffects _curDropEffects { get; set; } = DragDropEffects.None;

        #endregion

        #region Statics
        public static MpAvCefNetWebView DraggingRtb { get; private set; } = null;

        #endregion

        #region Properties


        public MpAvClipTileViewModel BindingContext => this.DataContext as MpAvClipTileViewModel;
        public bool IsEditorInitialized { get; set; } = false;
        public bool IsDomLoaded { get; set; } = false;

        

        public bool WasDragStartedFromEditor { get; set; } = false;
        public bool CanDrag { get; private set; } = true;

        public MpAvTextSelection Selection { get; private set; }

        public MpAvHtmlDocument Document { get; set; }

        MpAvIContentDocument MpAvIContentView.Document => Document;

        #endregion

        #region Constructors

        public MpAvCefNetWebView() : base() {
            //this.CreateWindow += MpAvCefNetWebView_CreateWindow;
            Document = new MpAvHtmlDocument(this);
            Selection = new MpAvTextSelection(Document);

            //CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Copy, OnCopy, OnCanExecuteClipboardCommand));
            //CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Cut, OnCut, OnCanExecuteClipboardCommand));
            //CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Paste, OnPaste, OnCanExecuteClipboardCommand));

        }

        private void MpAvCefNetWebView_CreateWindow(object sender, CreateWindowEventArgs e) {
            IPlatformHandle platformHandle = MpAvMainWindow.Instance.PlatformImpl.Handle;
            if (platformHandle is IMacOSTopLevelPlatformHandle macOSHandle)
                e.WindowInfo.SetAsWindowless(macOSHandle.GetNSWindowRetained());
            else
                e.WindowInfo.SetAsWindowless(platformHandle.Handle);

            e.Client = this.Client;
        }

        #endregion

        #region WebView Binding Methods
        public void HandleBindingNotification(MpAvEditorBindingFunctionType notificationTYpe, string msgJsonBase64Str) {
            var ctvm =DataContext as MpAvClipTileViewModel;
            if(ctvm == null && notificationTYpe != MpAvEditorBindingFunctionType.notifyDomLoaded) {
                // converter doesn't have data context but needs to notify dom loaded which doesn't need it
                return;
            }
            switch (notificationTYpe) {
                case MpAvEditorBindingFunctionType.notifyContentDraggableChanged:
                    var draggableChanged = MpJsonObject.DeserializeBase64Object<MpQuillContentDraggableChangedMessage>(msgJsonBase64Str);
                   UpdateDraggable(draggableChanged.isDraggable);
                    break;
                case MpAvEditorBindingFunctionType.notifyEditorSelectionChanged:
                    var selChangedJsonMsgObj = MpJsonObject.DeserializeBase64Object<MpQuillContentSelectionChangedMessage>(msgJsonBase64Str);
                   UpdateSelection(selChangedJsonMsgObj.index, selChangedJsonMsgObj.length, selChangedJsonMsgObj.selText, true, selChangedJsonMsgObj.isChangeBegin);
                    break;
                case MpAvEditorBindingFunctionType.notifyContentLengthChanged:
                    var contentLengthMsgObj = MpJsonObject.DeserializeBase64Object<MpQuillContentLengthChangedMessage>(msgJsonBase64Str);
                   Document.ContentEnd.Offset = contentLengthMsgObj.length;
                    break;
                case MpAvEditorBindingFunctionType.notifyDropEffectChanged:
                    var dropEffectChangedNtf = MpJsonObject.DeserializeBase64Object<MpQuillDropEffectChangedNotification>(msgJsonBase64Str);
                   UpdateDropEffect(dropEffectChangedNtf.dropEffect);
                    MpConsole.WriteLine($"{ctvm.CopyItemTitle} dropEffects: {dropEffectChangedNtf.dropEffect}");
                    break;
                case MpAvEditorBindingFunctionType.notifyDragStartOrEnd:
                    //if(wv.GetVisualAncestor<MpAvClipTileView>() is MpAvClipTileView ctv) {
                    //    var dddmsg = MpJsonObject.DeserializeBase64Object<MpQuillDragDropDataObjectMessage>(msgJsonStr);
                    //    ctv.UpdateSubSelectDragDataObject(dddmsg);
                    //}
                    var dragStartOrEndMsg = MpJsonObject.DeserializeBase64Object<MpQuillDragStartOrEndNotification>(msgJsonBase64Str);
                   WasDragStartedFromEditor = dragStartOrEndMsg.isStart;
                    break;
                case MpAvEditorBindingFunctionType.notifyReadOnlyChanged:
                    // TODO coordinate readOnly and sub-selection processing w/ webview extension..

                    if (ctvm.IsContentReadOnly) {
                        var disableReadOnlyMsg = MpJsonObject.DeserializeBase64Object<MpQuillDisableReadOnlyResponseMessage>(msgJsonBase64Str);
                        ctvm.IsContentReadOnly = false;
                    } else {
                        var enableReadOnlyMsg = MpJsonObject.DeserializeBase64Object<MpQuillEnableReadOnlyResponseMessage>(msgJsonBase64Str);
                        ctvm.IsContentReadOnly = true;
                    }
                    break;

                case MpAvEditorBindingFunctionType.notifySubSelectionEnabledChanged:
                    var subSelChangedNtf = MpJsonObject.DeserializeBase64Object<MpQuillSubSelectionChangedNotification>(msgJsonBase64Str);
                    ctvm.IsSubSelectionEnabled = subSelChangedNtf.isSubSelectionEnabled;
                    break;
                case MpAvEditorBindingFunctionType.notifyDomLoaded:
                   IsDomLoaded = true;
                    break;
                case MpAvEditorBindingFunctionType.notifyDropCompleted:
                    Dispatcher.UIThread.Post(async () => {
                        // when  drop completes wait for drag tile (if internal) to reload before updating selection to drop tile
                        await Task.Delay(300);
                        while (MpAvClipTrayViewModel.Instance.AllItems.Any(x => x.IsReloading)) {
                            await Task.Delay(100);
                        }
                        await Task.Delay(300);
                        MpAvMainWindow.Instance.Activate();
                        MpAvClipTrayViewModel.Instance.ClearAllSelection();
                        MpAvClipTrayViewModel.Instance.SelectedItem = BindingContext;
                        MpAvClipTrayViewModel.Instance.AllItems.ForEach(x => x.IsHovering = x.CopyItemId == BindingContext.CopyItemId);

                        //if (!BindingContext.IsPinned) {
                        //    var lb = this.GetVisualAncestor<ListBox>();

                        //    var lbi = this.GetVisualAncestor<ListBoxItem>();
                        //    lb.SelectedItem = BindingContext;
                        //}
                       
                        ////BindingContext.IsSelected = true;
                        //if (!this.IsKeyboardFocusWithin) {
                        //    this.Focus();
                        //}
                    });
                    break;
                case MpAvEditorBindingFunctionType.notifyDragEnter:
                    BindingContext.IsHovering = true;
                    break;
                case MpAvEditorBindingFunctionType.notifyDragLeave:
                    BindingContext.IsHovering = false;
                    break;
                case MpAvEditorBindingFunctionType.notifyException:
                    var exceptionMsgObj = MpJsonObject.DeserializeBase64Object<MpQuillExceptionMessage>(msgJsonBase64Str);
                    MpConsole.WriteLine(exceptionMsgObj);
                    break;
            }
        }
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

        public void SelectAll() {
            this.ExecuteJavascript("selectAll_ext()");
        }
        public void DeselectAll() {
            this.ExecuteJavascript($"deselectAll_ext()");
        }
        #endregion

        protected override WebViewGlue CreateWebViewGlue() {
            return new MpAvCefNetWebViewGlue(this);
        }

        protected override void OnPointerEnter(PointerEventArgs e) {
            base.OnPointerEnter(e);
            if(!e.IsLeftDown(this) && WasDragStartedFromEditor) {                
                this.ExecuteJavascript($"resetDragDrop_ext()");
                MpConsole.WriteLine("Dangling drag start canceled from mouse enter check");
                WasDragStartedFromEditor = false;
            }
        }

        

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
                var dragStartMsg = new MpQuillIsHostDraggingMessage() { isDragging = true };
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
                var dragEndMsg = new MpQuillIsHostDraggingMessage() { isDragging = false };
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


    }
}
