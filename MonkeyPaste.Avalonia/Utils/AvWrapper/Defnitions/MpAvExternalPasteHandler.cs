﻿//using MonkeyPaste.Common.Wpf;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia;
using System.Threading;
using Avalonia.Input;
using static MonkeyPaste.Avalonia.MpAvExternalPasteHandler;

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

        async Task MpIExternalPasteHandler.PasteDataObject(MpPortableDataObject mpdo, MpPortableProcessInfo processInfo) {
            if(processInfo == null) {
                // shouldn't happen
                //Debugger.Break();
                MpConsole.WriteTraceLine("Can't paste, if not lost focus somethings wrong");
                return;
            }

            IntPtr pasteToHandle = processInfo.Handle;
            
            if (processInfo is MpPortableStartProcessInfo startProcessInfo) {
                // TODO put ProcessAutomator stuff here 
                // NOTE needs to have non-zero handle when complete
            }

            string pasteCmd = "Control+v";
            bool finishWithEnter = false;
            var custom_paste_app_vm = MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppPath.ToLower() == processInfo.ProcessPath.ToLower() && x.PasteShortcutViewModel != null);
            
            if (custom_paste_app_vm != null) {
                pasteCmd = custom_paste_app_vm.PasteShortcutViewModel.PasteCmdKeyString;
                finishWithEnter = custom_paste_app_vm.PasteShortcutViewModel.EnterAfterPaste;
            }

            await PasteDataObjectAsync_internal(mpdo, pasteToHandle, pasteCmd, finishWithEnter);
        }

        #endregion
        private async Task PasteDataObjectAsync_internal(
            MpPortableDataObject mpdo, 
            IntPtr pasteToHandle,
            string pasteCmdKeyString,
            bool finishWithEnterKey) {
            if(pasteToHandle == IntPtr.Zero) {
                // somethings terribly wrong
                Debugger.Break();
            }
            MpConsole.WriteLine("Pasting to process: " + pasteToHandle);


            // SET CLIPBOARD

            MpPlatformWrapper.Services.ClipboardMonitor.IgnoreClipboardChanges = true;

            await MpPlatformWrapper.Services.DataObjectHelperAsync.SetPlatformClipboardAsync(mpdo);

            // ACTIVATE TARGET
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                IntPtr lastActive = MpPlatformWrapper.Services.ProcessWatcher.SetActiveProcess(pasteToHandle);
                if (!MpAvMainWindowViewModel.Instance.IsMainWindowLocked) {
                    MpAvMainWindowViewModel.Instance.FinishMainWindowHide(null);
                }

            } else {
                // assume target is active (if was start process info needs to be activated earlier)
            }

            await Task.Delay(200);
            // SIMULATE PASTE CMD
            await MpAvShortcutCollectionViewModel.Instance.SimulateKeyStrokeSequenceAsync(pasteCmdKeyString);

            if (finishWithEnterKey) {
                await MpAvShortcutCollectionViewModel.Instance.SimulateKeyStrokeSequenceAsync(MpKeyLiteralStringHelpers.ENTER_KEY_LITERAL);
            }

            await Task.Delay(300);

            MpPlatformWrapper.Services.ClipboardMonitor.IgnoreClipboardChanges = false;
        }

        #endregion

        #region Private Methods

        #endregion

    }
}