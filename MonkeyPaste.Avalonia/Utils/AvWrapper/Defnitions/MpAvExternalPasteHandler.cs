//using MonkeyPaste.Common.Wpf;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
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

            string pasteCmd = MpAvAppCollectionViewModel.Instance.GetAppClipboardKeysByProcessInfo(processInfo, false);
            int[] writer_preset_ids = MpAvAppCollectionViewModel.Instance.GetAppCustomOlePresetsByProcessInfo(processInfo, false);

            bool success = await PasteDataObjectAsync_internal_async(mpdo.ToAvDataObject(), pasteToHandle, pasteCmd, writer_preset_ids);
            MpConsole.WriteLine($"Paste to '{processInfo}' with keys '{pasteCmd}' was successful: {success}");
            return success;
        }

        #endregion
        private async Task<bool> PasteDataObjectAsync_internal_async(
            MpAvDataObject avdo,
            IntPtr pasteToHandle,
            string pasteCmdKeyString,
            int[] custom_writer_preset_ids = null) {
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

            await Mp.Services.DataObjectTools.WriteToClipboardAsync(avdo, true, custom_writer_preset_ids);

            // ACTIVATE TARGET
            nint activate_result = Mp.Services.ProcessWatcher.SetActiveProcess(pasteToHandle);
            bool set_active_success = activate_result == pasteToHandle;

            // SIMULATE PASTE CMD
            bool sim_input_success = Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence(pasteCmdKeyString);

            // RESTORE PREVIOUS CLIPBOARD
            if (last_mpdo_to_restore != null) {
                await Mp.Services.DataObjectTools.WriteToClipboardAsync(last_mpdo_to_restore, true, null);
            }
            return set_active_success && sim_input_success;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}
