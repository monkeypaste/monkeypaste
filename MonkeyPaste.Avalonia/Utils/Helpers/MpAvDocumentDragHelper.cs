
using Avalonia.Input;
using MonkeyPaste.Common;
using Avalonia.Threading;
using System.Threading.Tasks;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Controls;
using System.Diagnostics;
using Gtk;
using Avalonia.Interactivity;
using Xamarin.Essentials;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDocumentDragHelper {
        #region Private Variables

        private static MpAvIDragSource _dragSource;

        #endregion

        #region Properties
        public static IDataObject DragDataObject { get; private set; }

        public static IDataObject SourceDataObject { get; private set; }

        public static bool IsDragging => DragDataObject != null;

        #endregion

        #region Public Methods
        public static async Task PerformDragAsync(
            MpAvIDragSource dragSource,
            PointerEventArgs pointerEventArgs,
            DragDropEffects allowedEffects) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(() => PerformDragAsync(dragSource,pointerEventArgs, allowedEffects));
                return;
            }            

            ResetDragState();
            HookDragEvents();

            _dragSource = dragSource;

            SourceDataObject = await dragSource.GetDataObjectAsync(true);

            if (SourceDataObject == null) {
                // is none selected?
                Debugger.Break();
                FinishDrag(null);
                return;
            }
            DragDataObject = SourceDataObject.Clone();
            MpMessenger.SendGlobal(MpMessageType.ItemDragBegin);

            ApplyClipboardPresetToDragDataAsync().FireAndForgetSafeAsync();
            var result = await DragDrop.DoDragDrop(pointerEventArgs, DragDataObject, allowedEffects);

            // wait for possible dragEnd.wasCanceled == true msg
            await Task.Delay(300);
            if(result == DragDropEffects.None ||
                dragSource.WasDragCanceled) {
                // NOTE make sure to reset cancel property...
                dragSource.WasDragCanceled = false;
                MpConsole.WriteLine("Drag canceled");
                FinishDrag(result);
                return;
            }

            if (DragDataObject is MpPortableDataObject mpdo &&
                    dragSource is Control control &&
                    control.DataContext is MpAvClipTileViewModel ctvm) {
                string drop_app_url = null;
                if (MpPlatform.Services.ProcessWatcher.LastProcessInfo is MpPortableProcessInfo drop_pi &&
                    MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(drop_pi) is MpAvAppViewModel drop_avm) {
                    drop_app_url = MpPlatform.Services.SourceRefBuilder.ConvertToRefUrl(drop_avm.App);
                } else {
                    drop_app_url = MpPlatform.Services.SourceRefBuilder.ConvertToRefUrl(
                        MpAvAppCollectionViewModel.Instance.ThisAppViewModel.App);
                }
                if (string.IsNullOrEmpty(drop_app_url)) {
                    // maybe we should lax ref url req in report transaction...
                    Debugger.Break();
                } else {

                    // report drop transaction
                    MpPlatform.Services.TransactionBuilder.ReportTransactionAsync(
                                copyItemId: ctvm.CopyItemId,
                                reqType: MpJsonMessageFormatType.DataObject,
                                req: mpdo.SerializeData(),
                                respType: MpJsonMessageFormatType.None,
                                resp: null,
                                ref_urls: new[] { drop_app_url },
                                transType: MpTransactionType.Dragged).FireAndForgetSafeAsync(ctvm);
                }
            }

            FinishDrag(result);

            MpConsole.WriteLine("Cef Drag Result: " + result);
        }

        #endregion

        #region Private Methods

        //private static void ReceivedGlobalMessage(MpMessageType msg) {
        //    switch(msg) {
        //        case MpMessageType.ItemDragBegin:
        //        case MpMessageType.ClipboardPresetsChanged:
        //            ApplyClipboardPresetToDragDataAsync().FireAndForgetSafeAsync();
        //            break;
        //        case MpMessageType.ItemDragEnd:
        //            ResetDragState();
        //            break;
        //    }
        //}

        private static async Task ApplyClipboardPresetToDragDataAsync() {
            // seems excessive...but ultimately all ole pref's come
            // from plugins so pass everthing through cb plugin system just like writing to clipboard
            await MpPlatform.Services.DataObjectHelperAsync
                .UpdateDragDropDataObjectAsync(SourceDataObject,DragDataObject);
           
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
            //MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed += OnGlobalKeyPrssedOrReleasedHandler;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased += OnGlobalKeyPrssedOrReleasedHandler;

        }

        private static void UnhookDragEvents() {
            //MpMessenger.UnregisterGlobal(ReceivedGlobalMessage);
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyPressed -= OnGlobalKeyPrssedOrReleasedHandler;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalKeyReleased -= OnGlobalKeyPrssedOrReleasedHandler;

        }

        private static void FinishDrag(DragDropEffects? dropEffect) {
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
