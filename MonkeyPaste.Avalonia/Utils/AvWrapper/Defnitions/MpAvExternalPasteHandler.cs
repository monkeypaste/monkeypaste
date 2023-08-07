//using MonkeyPaste.Common.Wpf;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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
            MpPortableDataObject mpdo,
            MpPortableProcessInfo processInfo) {
            if (processInfo == null) {
                // shouldn't happen
                //MpDebug.Break();
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
            if (custom_paste_app_vm != null && custom_paste_app_vm.PasteShortcutViewModel.HasShortcut) {
                pasteCmd = custom_paste_app_vm.PasteShortcutViewModel.ShortcutCmdKeyString;
            }

            bool success = await PasteDataObjectAsync_internal_async(mpdo.ToAvDataObject(), pasteToHandle, pasteCmd);
            return success;
        }

        #endregion
        private async Task<bool> PasteDataObjectAsync_internal_async(
            MpAvDataObject avdo,
            IntPtr pasteToHandle,
            string pasteCmdKeyString) {
            if (pasteToHandle == IntPtr.Zero) {
                // somethings terribly wrong_lastInternalProcessInfo
                MpDebug.Break();
                return false;
            }
            MpConsole.WriteLine("Pasting to process: " + pasteToHandle);

            // STORE PREVIOUS CLIPBOARD (IF RESTORE REQ'D)

            MpPortableDataObject last_mpdo_to_restore =
                MpAvPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste ?
                    Mp.Services.ClipboardMonitor.LastClipboardDataObject : null;


            // SET CLIPBOARD

            await Mp.Services.DataObjectTools.WriteToClipboardAsync(avdo, true);

            // ACTIVATE TARGET
            bool set_active_success = Mp.Services.ProcessWatcher.SetActiveProcess(pasteToHandle) == pasteToHandle;
            //if (MpAvWindowManager.IsAnyActive) {
            //    // NOTE this maybe only a windows req' where this app must be active to change active
            //    // details here https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow#remarks
            //    //bool set_ac = Mp.Services.ProcessWatcher.SetActiveProcess(pasteToHandle);
            //    //if (!MpAvMainWindowViewModel.Instance.IsMainWindowLocked) {
            //    //    MpAvMainWindowViewModel.Instance.FinishMainWindowHide();
            //    //}
            //} else {
            //    // assume target is active (if was start process info needs to be activated earlier)
            //}

            // SIMULATE PASTE CMD
            bool sim_input_success = Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence(pasteCmdKeyString);

            // RESTORE PREVIOUS CLIPBOARD
            if (last_mpdo_to_restore != null) {
                await Mp.Services.DataObjectTools.WriteToClipboardAsync(last_mpdo_to_restore, true);
            }
            return set_active_success && sim_input_success;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
