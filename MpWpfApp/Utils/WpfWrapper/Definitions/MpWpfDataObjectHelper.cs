using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using MonkeyPaste;
using System.Windows;
using MpProcessHelper;
using System.Windows.Input;
using System.IO;
using System.Threading;
using MpClipboardHelper;
using System.Collections.Specialized;
using System.Diagnostics;

namespace MpWpfApp {
    public class MpWpfDataObjectHelper :
        MpIExternalPasteHandler,
        MpIErrorHandler, 
        MpIPlatformDataObjectHelper {
        #region Private Variables

        private Queue<MpPasteItem> _pasteQueue = new Queue<MpPasteItem>();

        private List<string> _tempFiles = new List<string>();

        #endregion

        #region Statics

        private static MpWpfDataObjectHelper _instance;
        public static MpWpfDataObjectHelper Instance => _instance ?? (_instance = new MpWpfDataObjectHelper());


        #endregion



        #region Constructors

        private MpWpfDataObjectHelper() { }

        #endregion

        #region Public Methods

        public void Init() {
            MpHelpers.RunOnMainThread(async () => {
                MpMainWindowViewModel mwvm = null;
                while(mwvm == null) {
                    if(Application.Current.MainWindow != null) {
                        if(Application.Current.MainWindow is MpMainWindow mw) {
                            if(mw.DataContext != null) {
                                mwvm = mw.DataContext as MpMainWindowViewModel;
                            }
                        }
                    }

                    await Task.Delay(100);
                }
                mwvm.OnMainWindowHidden += Mwvm_OnMainWindowHide;

            }).FireAndForgetSafeAsync(this);
        }

        #region MpIExternalPasteHandler Implementation
        public async Task PasteDataObject(MpPortableDataObject mpdo, object handleOrProcessInfo, bool finishWithEnterKey = false) {
            if(handleOrProcessInfo == null) {
                Debugger.Break();
            }
            MpProcessInfo pi = null;
            if(handleOrProcessInfo is IntPtr handle) {
                pi = new MpProcessInfo(handle);
            } else if(handleOrProcessInfo is MpProcessInfo) {
                pi = handleOrProcessInfo as MpProcessInfo;
            } else {
                Debugger.Break();
            }

            await PasteDataObject(mpdo, pi, finishWithEnterKey);
        }

        #endregion


        #region MpIPlatformDataObjectHelper Implementation

        public MpPortableDataObject ConvertToSupportedPortableFormats(object nativeDataObj, int retryCount = 5) {
            return ConvertWpfDataObjectToPortableFormat(nativeDataObj as IDataObject, retryCount);
        }

        public object ConvertToPlatformClipboardDataObject(MpPortableDataObject mpdo) {
            DataObject dobj = new DataObject();
            foreach (var kvp in mpdo.DataFormatLookup) {
                SetDataWrapper(ref dobj, kvp.Key, kvp.Value);
            }
            return dobj;
        }


        public void SetPlatformClipboard(MpPortableDataObject portableObj, bool ignoreChange) {
            MpPlatformWrapper.Services.ClipboardMonitor.IgnoreNextClipboardChangeEvent = ignoreChange;

            DataObject wpfDataObject = ConvertToPlatformClipboardDataObject(portableObj) as DataObject;
            Clipboard.SetDataObject(wpfDataObject);
        }

        public MpPortableDataObject GetPlatformClipboardDataObject() {
            var result = Clipboard.GetDataObject();
            var mpdo = ConvertToSupportedPortableFormats(result);
            return mpdo;
        }


        public async Task PasteDataObject(MpPortableDataObject mpdo, MpProcessInfo pi, bool finishWithEnterKey = false) {
            string pasteCmdKeyString = "^v";
            var avm = MpAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppPath.ToLower() == pi.ProcessPath.ToLower());
            if(avm != null && avm.PasteShortcutViewModel != null) {
                pasteCmdKeyString = MpWpfKeyboardInputHelpers.ConvertKeyStringToSendKeysString(
                                        avm.PasteShortcutViewModel.PasteCmdKeyString);
            }
            var pasteItem = new MpPasteItem() {
                PortableDataObject = mpdo,
                ProcessInfo = pi,
                FinishWithEnterKey = finishWithEnterKey,
                PasteCmdKeyString = pasteCmdKeyString
            };
            _pasteQueue.Enqueue(pasteItem);

            if (MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                MpMainWindowViewModel.Instance.IsMainWindowLocked = false;

                MpMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
            } else {
                Mwvm_OnMainWindowHide(this, null);
            }
            while (!_pasteQueue.IsNullOrEmpty()) {
                await Task.Delay(100);
            }
        }



        #endregion

        private MpPortableDataObject ConvertWpfDataObjectToPortableFormat(IDataObject ido, int retryCount = 5) {            
            if (retryCount == 0) {
                MpConsole.WriteLine("Exceeded retry limit accessing clipboard, ignoring");
                return null;
            }
            if(ido == null) {
                return null;
            }
            var ndo = new MpPortableDataObject();
            try {
                bool autoConvert = false;
                foreach (MpClipboardFormatType supportedType in MpPortableDataObject.SupportedFormats) {
                    if(supportedType == MpClipboardFormatType.UnicodeText) {
                        int b = 5;
                    }
                    string nativeTypeName = MpWinFormsDataFormatConverter.Instance.GetNativeFormatName(supportedType);
                    if (ido != null) {
                        if (string.IsNullOrEmpty(nativeTypeName) || 
                            ido.GetDataPresent(nativeTypeName, autoConvert) == false) {
                            continue;
                        }
                    }
                    string data = null;
                    switch (supportedType) {
                        case MpClipboardFormatType.Bitmap:
                            var bmpSrc = Clipboard.GetImage();
                            if (bmpSrc != null) {
                                data = bmpSrc.ToBase64String();
                            }
                            break;
                        case MpClipboardFormatType.FileDrop:
                            string[] sa = ido.GetData(DataFormats.FileDrop, autoConvert) as string[];
                            if (sa != null && sa.Length > 0) {
                                data = string.Join(Environment.NewLine, sa);
                            }
                            break;
                        default:
                            data = ido.GetData(nativeTypeName, autoConvert) as string;
                            break;
                    }
                    if (!string.IsNullOrEmpty(data)) {
                        ndo.DataFormatLookup.Add(supportedType, data);
                    }
                }
                if(ndo.DataFormatLookup.Count == 0) {
                    return null;
                }
                return ndo;
            }
            catch (Exception ex) {
                MpConsole.WriteLine($"Error accessing clipboard {retryCount} attempts remaining", ex);
                Thread.Sleep((5 - retryCount) * 100);
                return ConvertWpfDataObjectToPortableFormat(ido, retryCount--);
            }
        }

        

        public void HandleError(Exception ex) {
            MpConsole.WriteTraceLine(ex);
        }

        #endregion

        #region Private Methods

        private void Mwvm_OnMainWindowHide(object sender, EventArgs e) {
            if (_pasteQueue == null || _pasteQueue.IsNullOrEmpty()) {
                return;
            }
            int pasteCount = _pasteQueue.Count;
            while(pasteCount > 0) {
                var pasteItem = _pasteQueue.Dequeue();

                var processInfo = MpProcessHelper.MpProcessAutomation.SetActiveProcess(pasteItem.ProcessInfo);
               
                if(processInfo != null && processInfo.Handle != IntPtr.Zero) {

                    SetPlatformClipboard(pasteItem.PortableDataObject, true);
                    Thread.Sleep(100);
                    System.Windows.Forms.SendKeys.SendWait(pasteItem.PasteCmdKeyString);
                    Thread.Sleep(100);
                    if (pasteItem.FinishWithEnterKey) {
                        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                    }
                }
                pasteCount--;
            }
        }

        private bool IsProcessLikeNotepad(string processPath) {
            if (string.IsNullOrEmpty(processPath) || !File.Exists(processPath)) {
                return false;
            }

            try {
                string processName = Path.GetFileNameWithoutExtension(processPath).ToLower();
                if (processName == null) {
                    return false;
                }
                switch (processName) {
                    case "notepad":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteLine("IsProcessLikeNotepad GetFileName exception: " + ex);
                return false;
            }
        }

        private bool IsProcessNeedFileDrop(string processPath) {
            if (string.IsNullOrEmpty(processPath) || !File.Exists(processPath)) {
                return false;
            }

            try {
                string processName = Path.GetFileNameWithoutExtension(processPath).ToLower();
                if (processName == null) {
                    return false;
                }
                switch (processName) {
                    case "explorer":
                    case "mspaint":
                    case "notepad":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteLine("IsProcessNeedFileDrop GetFileName exception: " + ex);
                return false;
            }
        }

        private void SetDataWrapper(ref DataObject dobj, MpClipboardFormatType format, string dataStr) {
            if(string.IsNullOrEmpty(dataStr)) {
                return;
            }
            string nativeTypeName = GetWpfFormatName(format);
            switch (format) {
                case MpClipboardFormatType.Bitmap:
                    dobj.SetImage(dataStr.ToBitmapSource());
                    break;
                case MpClipboardFormatType.FileDrop:
                    var fl = dataStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    var sc = new StringCollection();
                    sc.AddRange(fl);
                    dobj.SetFileDropList(sc);
                    break;
                default:
                    dobj.SetData(nativeTypeName, dataStr);
                    break;
            }
        }


        private string GetWpfFormatName(MpClipboardFormatType portableType) {
            switch (portableType) {
                case MpClipboardFormatType.Text:
                    return DataFormats.Text;
                case MpClipboardFormatType.Html:
                    return DataFormats.Html;
                case MpClipboardFormatType.Rtf:
                    return DataFormats.Rtf;
                case MpClipboardFormatType.Bitmap:
                    return DataFormats.Bitmap;
                case MpClipboardFormatType.FileDrop:
                    return DataFormats.FileDrop;
                case MpClipboardFormatType.Csv:
                    return DataFormats.CommaSeparatedValue;
                default:
                    throw new Exception("Unknown portable format: " + portableType.ToString());
            }
        }

        #endregion

        internal class MpPasteItem {
            internal MpPortableDataObject PortableDataObject { get; set; }
            internal MpProcessInfo ProcessInfo { get; set; }
            internal bool FinishWithEnterKey { get; set; }
            internal string PasteCmdKeyString { get; set; }
        }

    }
}
