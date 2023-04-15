//using MonkeyPaste.Common.Wpf;
using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvExternalPasteHandler : MpIExternalPasteHandler {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvExternalPasteHandler _instance;
        public static MpAvExternalPasteHandler Instance => _instance ?? (_instance = new MpAvExternalPasteHandler());


        #endregion

        #region Constructors

        private MpAvExternalPasteHandler() { }

        #endregion

        #region Public Methods

        public void Init() {
            //MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            //MpAvMainWindowViewModel.Instance.OnMainWindowClosed += Instance_OnMainWindowClosed;
        }



        #region MpIExternalPasteHandler Implementation

        async Task<bool> MpIExternalPasteHandler.PasteDataObjectAsync(
            MpPortableDataObject mpdo, MpPortableProcessInfo processInfo) {
            if (processInfo == null) {
                // shouldn't happen
                //Debugger.Break();
                MpConsole.WriteTraceLine("Can't paste, if not lost focus somethings wrong");
                return false;
            }

            IntPtr pasteToHandle = processInfo.Handle;

            if (processInfo is MpPortableStartProcessInfo startProcessInfo) {
                // TODO put ProcessAutomator stuff here 
                // NOTE needs to have non-zero handle when complete
            }

            string pasteCmd = Mp.Services.PlatformShorcuts.PasteKeys;
            var custom_paste_app_vm =
                MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(processInfo);
            //MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppPath.ToLower() == processInfo.ProcessPath.ToLower() && x.PasteShortcutViewModel != null);

            if (custom_paste_app_vm != null) {
                pasteCmd = custom_paste_app_vm.PasteShortcutViewModel.PasteCmdKeyString;
            }

            bool success = await PasteDataObjectAsync_internal_async(mpdo, pasteToHandle, pasteCmd);
            return success;
        }

        #endregion
        private async Task<bool> PasteDataObjectAsync_internal_async(
            MpPortableDataObject mpdo,
            IntPtr pasteToHandle,
            string pasteCmdKeyString) {
            if (pasteToHandle == IntPtr.Zero) {
                // somethings terribly wrong
                Debugger.Break();
                return false;
            }
            MpConsole.WriteLine("Pasting to process: " + pasteToHandle);


            // SET CLIPBOARD

            await Mp.Services.DataObjectHelperAsync.SetPlatformClipboardAsync(mpdo, true);

            // ACTIVATE TARGET
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                IntPtr lastActive = Mp.Services.ProcessWatcher.SetActiveProcess(pasteToHandle);
                if (!MpAvMainWindowViewModel.Instance.IsMainWindowLocked) {
                    MpAvMainWindowViewModel.Instance.FinishMainWindowHide();
                }

                // } //else if (MpAvAppendNotificationWindow.Instance != null &&
                // IntPtr lastActive = Mp.Services.ProcessWatcher.SetActiveProcess(pasteToHandle);
                //MpAvNotificationWindowManager.Instance.HideNotification(MpAppendNotificationViewModel.Instance);
            } else {
                // assume target is active (if was start process info needs to be activated earlier)
            }

            // SIMULATE PASTE CMD
            await Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequenceAsync(pasteCmdKeyString);

            //await Task.Delay(300);

            //MpPlatformWrapper.Services.ClipboardMonitor.IgnoreClipboardChanges = false;
            return true;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
