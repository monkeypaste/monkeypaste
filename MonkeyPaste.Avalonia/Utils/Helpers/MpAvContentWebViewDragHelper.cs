
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvContentWebViewDragHelper {
        #region Private Variables

        private static MpAvIContentWebViewDragSource _dragSource;
        #endregion

        #region Properties
        public static IDataObject DragDataObject { get; set; }

        public static bool IsDragging => DragDataObject != null;

        #endregion

        #region Public Methods
        public static async Task<DragDropEffects> StartDragAsync(
            MpAvIContentWebViewDragSource dragSource,
            DragDropEffects allowedEffects) {
            if (dragSource == null ||
                dragSource is not Control drag_control ||
                drag_control.DataContext is not MpAvClipTileViewModel drag_ctvm ||
                dragSource.LastPointerPressedEventArgs is not PointerPressedEventArgs ppe_args) {
                MpDebug.Break($"Content drag error. Must provide pointer press event to start drag.");
                return DragDropEffects.None;
            }
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(() => StartDragAsync(dragSource, allowedEffects));
                return DragDropEffects.None;
            }

            StartDrag(dragSource);
            dragSource.IsDragging = true;
            //SourceDataObject = await dragSource.GetDataObjectAsync(_dragSource.GetDragFormats(), use_placeholders) as MpAvDataObject;
            // NOTE since using ALL formats drop widget won't work here
            DragDataObject = await dragSource.GetDataObjectAsync(null) as MpAvDataObject;

            if (DragDataObject == null) {
                // is none selected?
                MpDebug.Break();
                FinishDrag(null);
                return DragDropEffects.None;
            }

            DragDataObject = await Mp.Services.DataObjectTools.WriteDragDropDataObjectAsync(DragDataObject) as IDataObject;

            MpMessenger.SendGlobal(MpMessageType.ItemDragBegin);

            // signal drag start in sub-ui task
            var result = await MpAvDoDragDropWrapper.DoDragDropAsync(drag_control, ppe_args, DragDataObject, allowedEffects);
            MpConsole.WriteLine($"Content drop effect: '{result}'");

            // wait for possible dragEnd.wasCanceled == true msg
            await Task.Delay(300);
            if (result == DragDropEffects.None ||
                dragSource.WasDragCanceled) {
                // NOTE make sure to reset cancel property...
                MpConsole.WriteLine($"Drag canceled: '{dragSource.WasDragCanceled}'");
                MpMessenger.SendGlobal(MpMessageType.ItemDragCanceled);
                dragSource.WasDragCanceled = false;
                FinishDrag(result);
                return DragDropEffects.None;
            }

            // process drop result
            if (DragDataObject is MpPortableDataObject mpdo &&
                    dragSource is Control control &&
                    control.DataContext is MpAvClipTileViewModel ctvm) {
                // gather transaction refs
                string drop_app_url = null;
                if (Mp.Services.DropProcessWatcher.DropProcess is MpPortableProcessInfo drop_pi) {
                    drop_app_url = await Mp.Services.SourceRefTools.FetchOrCreateAppRefUrlAsync(drop_pi);
                }
                if (drop_app_url == null) {
                    // (SHOULD BE) internal drop
                    drop_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(
                        MpAvAppCollectionViewModel.Instance.ThisAppViewModel.App);
                }


                // report drop transaction
                Mp.Services.TransactionBuilder.ReportTransactionAsync(
                            copyItemId: ctvm.CopyItemId,
                            reqType: MpJsonMessageFormatType.DataObject,
                            req: null,//mpdo.SerializeData(),
                            respType: MpJsonMessageFormatType.None,
                            resp: null,
                            ref_uris: new[] { drop_app_url },
                            transType: MpTransactionType.Dragged).FireAndForgetSafeAsync(ctvm);
            }

            FinishDrag(result);

            MpConsole.WriteLine("Cef Drag Result: " + result);
            return result;
        }

        #endregion

        #region Private Methods
        private static void OnGlobalKeyPrssedOrReleasedHandler(object sender, string key) {
            if (MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown) {
                FinishDrag(null);
                return;
            }
            if (_dragSource == null) {
                return;
            }
            _dragSource.NotifyModKeyStateChanged(
                MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                MpAvShortcutCollectionViewModel.Instance.GlobalIsAltDown,
                MpAvShortcutCollectionViewModel.Instance.GlobalIsShiftDown,
                MpAvShortcutCollectionViewModel.Instance.GlobalIsEscapeDown,
                MpAvShortcutCollectionViewModel.Instance.GlobalIsMetaDown);
        }

        private static void HookDragEvents() {
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += OnGlobalKeyPrssedOrReleasedHandler;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += OnGlobalKeyPrssedOrReleasedHandler;

        }

        private static void UnhookDragEvents() {
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= OnGlobalKeyPrssedOrReleasedHandler;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= OnGlobalKeyPrssedOrReleasedHandler;

        }

        private static void StartDrag(MpAvIContentWebViewDragSource ds) {
            ResetDragState();
            HookDragEvents();
            _dragSource = ds;
        }

        private static void FinishDrag(DragDropEffects? dropEffect) {
            if (_dragSource != null) {
                _dragSource.IsDragging = false;
            }
            if (_dragSource is MpAvContentWebView wv) {
                // this makes sure dnd state is reset for drag item
                wv.SendMessage($"resetDragAndDrop_ext()");
            }
            MpMessenger.SendGlobal(MpMessageType.ItemDragEnd);
            ResetDragState();
        }
        private static void ResetDragState() {
            UnhookDragEvents();
            DragDataObject = null;
            DragDataObject = null;
            _dragSource = null;
        }


        #endregion
    }
}
