
using Avalonia.Controls;
using Avalonia.Input;
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

        public static IDataObject SourceDataObject { get; private set; }

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
            // wait for source data
            bool use_placeholders = MpAvExternalDropWindowViewModel.Instance.IsDropWidgetEnabled;
            SourceDataObject = await dragSource.GetDataObjectAsync(_dragSource.GetDragFormats(), use_placeholders) as MpAvDataObject;

            if (SourceDataObject == null) {
                // is none selected?
                MpDebug.Break();
                FinishDrag(null);
                return DragDropEffects.None;
            }

            await ApplyClipboardPresetOrSourceUpdateToDragDataAsync();

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

        public static async Task ApplyClipboardPresetOrSourceUpdateToDragDataAsync() {
            if (SourceDataObject == null) {
                // no drag in progress
                return;
            }
            if (DragDataObject == null) {
                // initial case
                DragDataObject = new MpAvDataObject();
                SourceDataObject.CopyTo(DragDataObject);
            }

            // clone source or processing will overwrite original data (so drop widget changes have no affect)
            var source_clone = SourceDataObject.Clone();
            var temp = await Mp.Services.DataObjectTools.WriteDragDropDataObjectAsync(source_clone);
            if (temp is IDataObject temp_ido &&
                DragDataObject is MpAvDataObject ddo) {
                // update actual drag ido while retaining its ref (must be same as in StartDrag)
                temp_ido.CopyTo(DragDataObject);
                await ddo.MapAllPseudoFormatsAsync();
                MpConsole.WriteLine("DragDataObject updated");
            }
        }
        #endregion

        #region Private Methods

        private static void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                //case MpMessageType.ItemDragBegin:
                case MpMessageType.ClipboardPresetEnabledChanged:
                    ApplyClipboardPresetOrSourceUpdateToDragDataAsync().FireAndForgetSafeAsync();
                    break;
            }
        }



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
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += OnGlobalKeyPrssedOrReleasedHandler;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += OnGlobalKeyPrssedOrReleasedHandler;

        }

        private static void UnhookDragEvents() {
            MpMessenger.UnregisterGlobal(ReceivedGlobalMessage);
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
            SourceDataObject = null;
            DragDataObject = null;
            _dragSource = null;
        }


        #endregion
    }
}
