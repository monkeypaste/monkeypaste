//using MonkeyPaste.Common.Wpf;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
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
            MpPortableProcessInfo pi) {
            if (pi == null) {
                // shouldn't happen
                //MpDebug.Break();
                MpConsole.WriteTraceLine("Can't paste, if not lost focus somethings wrong");
                return false;
            }


            string pasteCmd = MpAvAppCollectionViewModel.Instance.GetAppClipboardKeysByProcessInfo(pi, false);
            int[] writer_preset_ids = MpAvAppCollectionViewModel.Instance.GetAppCustomOlePresetsByProcessInfo(pi, false);

            bool success = await PasteDataObjectAsync_internal_async(mpdo.ToAvDataObject(), pi, pasteCmd, writer_preset_ids);
            MpConsole.WriteLine($"Paste to '{pi}' with keys '{pasteCmd}' was successful: {success}");
            return success;
        }

        #endregion
        private async Task<bool> PasteDataObjectAsync_internal_async(
            MpAvDataObject avdo,
            MpPortableProcessInfo pi,
            string pasteCmdKeyString,
            int[] custom_writer_preset_ids = null) {
            if (pi == null || pi.Handle == nint.Zero) {
                // somethings terribly wrong_lastInternalProcessInfo
                MpDebug.Break();
                return false;
            }
            MpConsole.WriteLine($"Pasting to process: {pi}");

            // STORE PREVIOUS CLIPBOARD (IF RESTORE REQ'D)

            MpPortableDataObject last_mpdo_to_restore =
                MpAvPrefViewModel.Instance.ResetClipboardAfterMonkeyPaste ?
                    Mp.Services.ClipboardMonitor.LastClipboardDataObject : null;


            // SET CLIPBOARD

            await Mp.Services.DataObjectTools.WriteToClipboardAsync(avdo, true);

            // ACTIVATE TARGET
            nint activate_result = Mp.Services.ProcessWatcher.SetActiveProcess(pi);
            bool set_active_success = activate_result == pi.Handle;

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
