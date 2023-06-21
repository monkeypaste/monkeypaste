
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvContentDragHelper {
        #region Private Variables

        private static MpAvIDragSource _dragSource;

        #endregion

        #region Properties
        public static IDataObject DragDataObject { get; private set; }

        public static IDataObject SourceDataObject { get; private set; }

        public static bool IsDragging => DragDataObject != null;

        #endregion

        #region Public Methods
        public static async Task StartDragAsync(
            MpAvIDragSource dragSource,
            PointerEventArgs pointerEventArgs,
            DragDropEffects allowedEffects) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(() => StartDragAsync(dragSource, pointerEventArgs, allowedEffects));
                return;
            }

            ResetDragState();
            HookDragEvents();

            _dragSource = dragSource;

            dragSource.IsDragging = true;
            // wait for source data
            bool use_placeholders = MpAvExternalDropWindowViewModel.Instance.IsDropWidgetEnabled;
            SourceDataObject = await dragSource.GetDataObjectAsync(_dragSource.GetDragFormats(), use_placeholders);

            if (SourceDataObject == null) {
                // is none selected?
                Debugger.Break();
                FinishDrag(null);
                return;
            }

            await ApplyClipboardPresetOrSourceUpdateToDragDataAsync();

            MpMessenger.SendGlobal(MpMessageType.ItemDragBegin);

            // signal drag start in sub-ui task
            var result = await DragDrop.DoDragDrop(pointerEventArgs, DragDataObject, allowedEffects);

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
                return;
            }

            // process drop result
            if (DragDataObject is MpPortableDataObject mpdo &&
                    dragSource is Control control &&
                    control.DataContext is MpAvClipTileViewModel ctvm) {
                // gather transaction refs
                string drop_app_url = null;
                if (Mp.Services.ProcessWatcher.LastProcessInfo is MpPortableProcessInfo drop_pi &&
                    MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(drop_pi) is MpAvAppViewModel drop_avm) {
                    drop_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(drop_avm.App);
                } else {
                    drop_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(
                        MpAvAppCollectionViewModel.Instance.ThisAppViewModel.App);
                }
                if (string.IsNullOrEmpty(drop_app_url)) {
                    // maybe we should lax ref url req in report transaction...
                    Debugger.Break();
                } else {

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
            }

            FinishDrag(result);

            MpConsole.WriteLine("Cef Drag Result: " + result);

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
            var temp = await Mp.Services.DataObjectHelperAsync.ProcessDragDropDataObjectAsync(source_clone);
            if (temp is IDataObject temp_ido &&
                DragDataObject is MpAvDataObject ddo) {
                // update actual drag ido while retaining its ref (must be same as in StartDrag)
                temp_ido.CopyTo(DragDataObject);
                await ddo.MapAllPseudoFormatsAsync();
                MpConsole.WriteLine("DragDataObject updated");

                //var phl = DragDataObject.GetPlaceholderFormats();
                //MpConsole.WriteLine($"Placeholder formats: {string.Join(",", phl)}");
                //if (DragDataObject.TryGetData(MpPortableDataFormats.AvFileNames, out IEnumerable<string> fnl)) {
                //    MpConsole.WriteLine($"dnd obj updated. target fns:");
                //    fnl.ForEach(x => MpConsole.WriteLine(x));
                //}

            }
        }
        #endregion

        #region Private Methods

        private static void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                //case MpMessageType.ItemDragBegin:
                case MpMessageType.ClipboardPresetsChanged:
                    ApplyClipboardPresetOrSourceUpdateToDragDataAsync().FireAndForgetSafeAsync();
                    break;
                    //case MpMessageType.ItemDragEnd:
                    //    ResetDragState();
                    //    break;
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

        private static void FinishDrag(DragDropEffects? dropEffect) {
            if (_dragSource != null) {
                _dragSource.IsDragging = false;
            }
            ResetDragState();
            MpMessenger.SendGlobal(MpMessageType.ItemDragEnd);


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
