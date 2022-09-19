using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Avalonia.Utils.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        #region Private Variables

        private MpAvDataObject _subSelectDragOataObject = null;

        #endregion

        #region Window Binding Handlers

        public void UpdateSubSelectDragDataObject(MpQuillDragDropDataObjectMessage subSelectDataObject) {
            _subSelectDragOataObject = new MpAvDataObject();
            foreach (var di in subSelectDataObject.items) {
                _subSelectDragOataObject.SetData(di.format, di.data);
            }
            _subSelectDragOataObject.MapAllPseudoFormats();
        }

        #endregion

        #region Drag
        private void StartSelect(PointerPressedEventArgs e) {
            if(BindingContext.GetContentView() is MpAvCefNetWebView wv) {
                var editor_mp = e.GetPosition(BindingContext.GetContentView() as Control);
                var selFromPointMsg = new MpQuillSetSelectionFromEditorPointMessage() {
                    state =wv.CanDrag ? "drag":"down",
                    x = editor_mp.X,
                    y = editor_mp.Y,
                    modkeyBase64Msg = GetModKeyMsg()
                };
                wv.ExecuteJavascript($"setSelectionFromEdiotrPoint_ext('{selFromPointMsg.SerializeJsonObjectToBase64()}')");
            }
            

        }
        private void ContinueSelect(PointerEventArgs e) {
            if (BindingContext.GetContentView() is MpAvCefNetWebView wv) {
                var editor_mp = e.GetPosition(BindingContext.GetContentView() as Control);
                var selFromPointMsg = new MpQuillSetSelectionFromEditorPointMessage() {
                    state = wv.CanDrag ? "drag":"move",
                    x = editor_mp.X,
                    y = editor_mp.Y,
                    modkeyBase64Msg = GetModKeyMsg()
                };
                wv.ExecuteJavascript($"setSelectionFromEdiotrPoint_ext('{selFromPointMsg.SerializeJsonObjectToBase64()}')");
            }
        }
        private void EndSelect() {
            
            if (BindingContext.GetContentView() is MpAvCefNetWebView wv) {
                var selFromPointMsg = new MpQuillSetSelectionFromEditorPointMessage() {
                    state = wv.CanDrag ? "drop" : "up",
                    x = 0,
                    y = 0
                };
                wv.ExecuteJavascript($"setSelectionFromEdiotrPoint_ext('{selFromPointMsg.SerializeJsonObjectToBase64()}')");
            }
        }

        private void MpAvClipTileView_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (BindingContext.IsHitTestEnabled) {

                this.DragCheckAndStart(e, StartSelect, ContinueSelect, EndSelect,0);

                // let webview handle when hit testable (ie subselection is enabled)
                return;
            }
            //if (//!BindingContext.GetContentView().CanDrag ||
            //    isDragWaiting ||
            //    BindingContext is MpIResizableViewModel rvm && rvm.CanResize) {
            //    return;
            //}
            this.DragCheckAndStart(e, StartDrag, ContinueDrag, EndDrag);

        }

        private void StartDrag(PointerPressedEventArgs e) {
            MpAvIContentView cv = BindingContext.GetContentView();
            cv.SelectAll();

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
                var avmpdo = await BindingContext.ConvertToDataObject(false);
                // for tile drag all is always selected so add tile to formats
                avmpdo.SetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT, BindingContext);
                var result = await DragDrop.DoDragDrop(e, avmpdo, DragDropEffects.Copy | DragDropEffects.Move);
                EndDrag();
            });

        }

        //private bool isDragWaiting = false;
        //private void StartDrag_both(PointerPressedEventArgs e) {
        //    if(isDragWaiting) {
        //        return;
        //    }
        //    bool isTileDrag = !BindingContext.IsHitTestEnabled;
        //    bool isSubSelectWaiting = !isTileDrag && _subSelectDragOataObject == null;

        //    if (isSubSelectWaiting) {
        //        isDragWaiting = true;
        //        // implies sub-selection so wait for sub-select ntf from editor
        //        Dispatcher.UIThread.Post(async () => {
        //            while(_subSelectDragOataObject == null) {
        //                // wait for editor to handle drag start
        //                if(!BindingContext.IsHitTestEnabled) {
        //                    //sub-selection disabled exit
        //                    isDragWaiting = false;
        //                    return;
        //                }
        //                await Task.Delay(100);
        //            }
        //            isDragWaiting = false;
        //            StartDrag(e);
        //            return;
        //        });
        //        return;
        //    }

        //    MpAvIContentView cv = BindingContext.GetContentView();
        //    if(isTileDrag) {
        //        cv.SelectAll();
        //    }
            

        //    // add temp key down listener for notifying editor for visual feedback
        //    MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += Global_DragKeyUpOrDown;
        //    MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += Global_DragKeyUpOrDown;

        //    MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();
        //    MpAvMainWindowViewModel.Instance.IsDropOverMainWindow = true;
        //    BindingContext.IsTileDragging = true;

        //    if(cv is MpAvCefNetWebView wv) {
        //        // notify editor that its dragging and not just in a drop state
        //        var dragStartMsg = new MpQuillIsHostDraggingMessage() { isDragging = true };
        //        wv.ExecuteJavascript($"updateIsDraggingFromHost_ext('{dragStartMsg.SerializeJsonObjectToBase64()}')");
        //    }

        //    Dispatcher.UIThread.Post(async () => {
        //        MpAvDataObject avmpdo = null;
        //        if(isTileDrag) {
        //            avmpdo = await BindingContext.ConvertToDataObject(false);
        //            // for tile drag all is always selected so add tile to formats
        //            avmpdo.SetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_DATA_FORMAT, BindingContext);
        //        } else {
        //            avmpdo = _subSelectDragOataObject;
        //        }
        //        var result = await DragDrop.DoDragDrop(e, avmpdo, DragDropEffects.Copy | DragDropEffects.Move);
        //        EndDrag();
        //    });

        //}


        private void ContinueDrag(PointerEventArgs e) {
            MpAvMainWindowViewModel.Instance.DragMouseMainWindowLocation = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();

        }
        private void EndDrag() {
            if(BindingContext.IsTileDragging == false) {
                // can be called twice when esc-canceled (first from StartDrag handler then from the checker pointer up so ignore 2nd
                return;
            }

            MpAvIContentView cv = BindingContext.GetContentView();
            cv.DeselectAll();
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
            _subSelectDragOataObject = null;
        }

        private string GetModKeyMsg() {
            var modKeyMsg = new MpQuillModifierKeysNotification() {
                ctrlKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                altKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown,
                shiftKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown,
                escKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown
            };
            return modKeyMsg.SerializeJsonObjectToBase64();
        }

        private void Global_DragKeyUpOrDown(object sender, string e) {
            if(BindingContext.GetContentView() is MpAvCefNetWebView wv) {
                var modKeyMsg = new MpQuillModifierKeysNotification() {
                    ctrlKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                    altKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown,
                    shiftKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown,
                    escKey = MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown
                };
                wv.ExecuteJavascript($"updateModifierKeysFromHost_ext('{modKeyMsg.SerializeJsonObjectToBase64()}')");
            }            
        }

        #endregion

        public MpAvClipTileView() {
            InitializeComponent();
            this.PointerPressed += MpAvClipTileView_PointerPressed;
            //var ctv = this.FindControl<MpAvClipTileContentView>("ClipTileContentView");
            //ctv.PointerEnter += Ctv_PointerEnter;
            //ctv.PointerMoved += Ctv_PointerMoved;
            //ctv.PointerLeave += Ctv_PointerLeave;

        }

        private void Ctv_PointerLeave(object sender, PointerEventArgs e) {
            Cursor = new Cursor(StandardCursorType.Arrow);
        }

        private void Ctv_PointerMoved(object sender, PointerEventArgs e) {
            if (BindingContext.IsSubSelectionEnabled) {
                Cursor = new Cursor(StandardCursorType.Ibeam);
            } else {
                Cursor = new Cursor(StandardCursorType.Arrow);
            }
        }

        private void Ctv_PointerEnter(object sender, PointerEventArgs e) {
            if (BindingContext.IsSubSelectionEnabled) {
                Cursor = new Cursor(StandardCursorType.Ibeam);
            } else {
                Cursor = new Cursor(StandardCursorType.Arrow);
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
