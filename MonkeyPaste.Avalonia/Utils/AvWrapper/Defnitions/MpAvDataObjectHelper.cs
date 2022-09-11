using MonkeyPaste.Common.Wpf;
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

namespace MonkeyPaste.Avalonia {
    public class MpAvDataObjectHelper :
        MpIExternalPasteHandler,
        MpIErrorHandler
        //MpIPlatformDataObjectHelper 
        {
        #region Private Variables

        private Queue<MpPasteItem> _pasteQueue = new Queue<MpPasteItem>();

        private List<string> _tempFiles = new List<string>();

        #endregion

        #region Statics

        private static MpAvDataObjectHelper _instance;
        public static MpAvDataObjectHelper Instance => _instance ?? (_instance = new MpAvDataObjectHelper());


        #endregion

        #region Constructors

        private MpAvDataObjectHelper() { }

        #endregion

        #region Public Methods

        public void Init() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }


        #region MpIPlatformDataObjectHelper Implementation

        public MpPortableDataObject ConvertToSupportedPortableFormats(object nativeDataObj, int retryCount = 5) {
            var mpdo = MpAvClipboardHandlerCollectionViewModel.Instance.ReadClipboardOrDropObject(nativeDataObj);
            return mpdo;
        }

        public object ConvertToPlatformClipboardDataObject(MpPortableDataObject mpdo) {
            object pdo = MpAvClipboardHandlerCollectionViewModel.Instance.WriteClipboardOrDropObject(mpdo, false);
            return pdo;
        }


        public void SetPlatformClipboard(MpPortableDataObject portableObj, bool ignoreChange) {
            MpPlatformWrapper.Services.ClipboardMonitor.IgnoreNextClipboardChangeEvent = ignoreChange;
            MpAvClipboardHandlerCollectionViewModel.Instance.WriteClipboardOrDropObject(portableObj, true);
        }

        public MpPortableDataObject GetPlatformClipboardDataObject() {
            return MpAvClipboardHandlerCollectionViewModel.Instance.ReadClipboardOrDropObject(null);
        }



        #endregion


        #region MpIExternalPasteHandler Implementation

        async Task MpIExternalPasteHandler.PasteDataObject(MpPortableDataObject mpdo, object handleOrProcessInfo, bool finishWithEnterKey = false) {
            if (handleOrProcessInfo == null) {
                Debugger.Break();
            }
            //MpProcessInfo pi = null;
            //if (handleOrProcessInfo is IntPtr handle) {
            //    pi = new MpProcessInfo(handle);
            //} else if (handleOrProcessInfo is MpProcessInfo) {
            //    pi = handleOrProcessInfo as MpProcessInfo;
            //} else {
            //    Debugger.Break();
            //}
            IntPtr pasteToHandle = IntPtr.Zero;
            if (handleOrProcessInfo is IntPtr handle) {
                pasteToHandle = handle;
            } else if (handleOrProcessInfo is string processPath) {
                // todo check running apps and use last active handle for path, when none exist will need to use open process stuff

                Debugger.Break();
            } else if (handleOrProcessInfo is MpPortableProcessInfo) {
                // need this?
                // pi = handleOrProcessInfo as MpProcessInfo;

                Debugger.Break();
            } else {
                Debugger.Break();
            }

            await PasteDataObjectAsync_internal(mpdo, pasteToHandle, finishWithEnterKey);
        }

        #endregion
        private async Task PasteDataObjectAsync_internal(MpPortableDataObject mpdo, IntPtr pasteToHandle, bool finishWithEnterKey = false) {
            // when pasteHandle is zero use last active app handle as default

            pasteToHandle = pasteToHandle == IntPtr.Zero ? MpPlatformWrapper.Services.ProcessWatcher.LastHandle : pasteToHandle;
            //string pasteCmdKeyString = "^v";
            string pasteCmdKeyString = "Control+v";
            string handlePath = MpPlatformWrapper.Services.ProcessWatcher.GetProcessPath(pasteToHandle);

            // update pasteCmd key's if app has defined unqiue paste shortcut for sendKeys (on windows unknown for others atm)

            var avm = MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppPath.ToLower() == handlePath.ToLower());
            if (avm != null && avm.PasteShortcutViewModel != null) {
                pasteCmdKeyString = MpWpfKeyboardInputHelpers.ConvertKeyStringToSendKeysString(
                                        avm.PasteShortcutViewModel.PasteCmdKeyString);
            }
            var pasteItem = new MpPasteItem() {
                PortableDataObject = mpdo,
                //ProcessInfo = pi,
                PasteToProcessHandle = pasteToHandle,
                FinishWithEnterKey = finishWithEnterKey,
                PasteCmdKeyString = pasteCmdKeyString
            };
            _pasteQueue.Enqueue(pasteItem);

            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                MpAvMainWindowViewModel.Instance.IsMainWindowLocked = false;

                MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
            } else {
                ProcessPasteQueue();
            }
            while (!_pasteQueue.IsNullOrEmpty()) {
                await Task.Delay(100);
            }
        }

        public void HandleError(Exception ex) {
            MpConsole.WriteTraceLine(ex);
        }

        #endregion

        #region Private Methods

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowHid:
                    ProcessPasteQueue();
                    break;
            }
        }

        private void ProcessPasteQueue() {
            if (_pasteQueue == null || _pasteQueue.IsNullOrEmpty()) {
                return;
            }
            int pasteCount = _pasteQueue.Count;
            while (pasteCount > 0) {
                var pasteItem = _pasteQueue.Dequeue();

                if (pasteItem.PasteToProcessHandle != IntPtr.Zero) {
                    MpPlatformWrapper.Services.ProcessWatcher.SetActiveProcess(pasteItem.PasteToProcessHandle);

                    MpPlatformWrapper.Services.ClipboardMonitor.IgnoreNextClipboardChangeEvent = true;

                    SetPlatformClipboard(pasteItem.PortableDataObject, true);
                    Thread.Sleep(100);
                    MpAvShortcutCollectionViewModel.Instance.SimulateKeyStrokeSequence(pasteItem.PasteCmdKeyString);
                    //System.Windows.Forms.SendKeys.SendWait(pasteItem.PasteCmdKeyString);

                    Thread.Sleep(100);
                    if (pasteItem.FinishWithEnterKey) {
                        //System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                        MpAvShortcutCollectionViewModel.Instance.SimulateKeyStrokeSequence(MpKeyLiteralStringHelpers.ENTER_KEY_LITERAL);
                    }
                }
                pasteCount--;
            }
        }
        private void SetDataWrapper(ref DataObject dobj, string format, object data) {
            if (data == null) {
                return;
            }

            //switch (format) {
            //    case MpPortableDataFormats.Bitmap:
            //        var bmpSrc = data.ToString().ToBitmapSource(false);

            //        var winforms_dataobject = new DataObject();// MpClipboardHelper.MpClipoardImageHelpers.GetClipboardImage_WinForms(bmpSrc.ToBitmap(), null, null);

            //        //Clipboard.SetData(DataFormats.Bitmap, bmpSrc);
            //        //Clipboard.SetData("PNG", winforms_dataobject.GetData("PNG"));
            //        //Clipboard.SetData(DataFormats.Dib, winforms_dataobject.GetData(DataFormats.Dib));
            //        //dobj.SetImage(data.ToString().ToBitmapSource());

            //        //IDataObject ido = new DataObject();
            //        //ido.SetData(DataFormats.Bitmap, new Image() { Source = bmpSrc },true); // true means autoconvert

            //        //dobj.SetData(DataFormats.Bitmap, ido.GetData(DataFormats.Bitmap));
            //        var pngData = winforms_dataobject.GetData("PNG");
            //        var dibData = winforms_dataobject.GetData(DataFormats.Dib);
            //        dobj.SetImage(bmpSrc);
            //        dobj.SetData("PNG", pngData);
            //        dobj.SetData(DataFormats.Dib, dibData);
            //        break;
            //    case MpPortableDataFormats.FileDrop:
            //        var fl = data.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            //        var sc = new StringCollection();
            //        sc.AddRange(fl);
            //        dobj.SetFileDropList(sc);
            //        break;
            //    default:
            //        dobj.SetData(format, data);
            //        break;
            //}
        }

        #endregion

        internal class MpPasteItem {
            internal MpPortableDataObject PortableDataObject { get; set; }
            //internal MpProcessInfo ProcessInfo { get; set; }
            internal IntPtr PasteToProcessHandle { get; set; }
            internal bool FinishWithEnterKey { get; set; }
            internal string PasteCmdKeyString { get; set; }
        }

    }
}
