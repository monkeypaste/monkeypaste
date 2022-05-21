using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;
using MonkeyPaste;
using System.Windows;
using MpProcessHelper;
using System.Windows.Input;
using System.IO;
using System.Threading;
using MpClipboardHelper;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Drawing.Imaging;

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

        public MpPortableDataObject ConvertNativeClipboardObjectToPortableFormat(object nativeClipboardObject) {
            return ConvertWpfDataObjectToPortableFormat(nativeClipboardObject as IDataObject);
        }

        public async Task PasteDataObject(MpPortableDataObject mpdo, IntPtr handle, bool finishWithEnterKey = false) {
            var pi = new MpProcessInfo() {
                Handle = handle
            };
            await PasteDataObject(mpdo, pi, finishWithEnterKey);
        }

        public object GetDataObjectWrapper() {
            var result = Clipboard.GetDataObject();
            return result;
        }

        public async Task PasteCopyItem(MpCopyItem ci, MpProcessInfo pi, bool finishWithEnterKey = false) {
            MpPortableDataObject cido = await GetCopyItemDataObjectAsync(ci, false, pi.Handle);
            await PasteDataObject(cido, pi, finishWithEnterKey);
        }

        public async Task PasteDataObject(MpPortableDataObject mpdo, MpProcessInfo pi, bool finishWithEnterKey = false) {
            string pasteCmdKeyString = "^V";
            var avm = MpAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppPath.ToLower() == pi.ProcessPath.ToLower());
            if(avm != null && avm.PasteShortcutViewModel != null) {
                pasteCmdKeyString = avm.PasteShortcutViewModel.PasteCmdKeyString;
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


        #region MpIClipboardInterop Implementation
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
                        if (ido.GetDataPresent(nativeTypeName, autoConvert) == false) {
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

        public async Task<MpPortableDataObject> GetCopyItemDataObjectAsync(MpCopyItem ci, bool isDragDrop, object targetHandleObj) {
            // NOTE this is NOT part of data object interface (which is in MonkeyPaste.Plugin)
            // because it needs MpCopyItem
            // and am trying to isolate data object for pluggability

            IntPtr targetHandle = targetHandleObj == null ? IntPtr.Zero : (IntPtr)targetHandleObj;
            bool isToExternalApp = targetHandle != IntPtr.Zero && targetHandle != MpProcessManager.GetThisApplicationMainWindowHandle();

            MpPortableDataObject d = new MpPortableDataObject();
            string rtf = string.Empty.ToRichText();
            string pt = string.Empty;
            var sctfl = new List<string>();

            //check for model templates
            var templates = await MpDataModelProvider.GetTextTemplatesAsync(ci.Id);
            bool hasTemplates = templates != null && templates.Count > 0;

            if (hasTemplates) {
                // trigger query change before showing main window may need to tweak...
                MpDataModelProvider.SetManualQuery(new List<int>() { ci.Id });
                if (MpMainWindowViewModel.Instance.IsMainWindowOpen == false) {
                    MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    while (MpMainWindowViewModel.Instance.IsMainWindowOpen == false) {
                        await Task.Delay(100);
                    }
                }
                await Task.Delay(50); //wait for clip tray to get query changed message
                while (MpClipTrayViewModel.Instance.IsRequery) {
                    await Task.Delay(100);
                }
                var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(ci.Id);
                if (civm != null) {
                    while (civm.IsBusy || civm.Parent.IsBusy) {
                        await Task.Delay(100);
                    }
                    civm.Parent.ClearSelection();
                    civm.IsSelected = true;

                    rtf = await civm.Parent.GetSubSelectedPastableRichText(isToExternalApp);
                }
            } else {
                rtf = ci.ItemData.ToRichText();
            }
            pt = rtf.ToPlainText();

            if (isToExternalApp) {
                if (isDragDrop) {

                    string targetProcessPath = MpProcessManager.GetProcessPath(targetHandle);
                    var app = await MpDataModelProvider.GetAppByPath(targetProcessPath);
                    MpAppClipboardFormatInfoCollectionViewModel targetInteropSettings = null;
                    if (app != null) {
                        targetInteropSettings = MpAppCollectionViewModel.Instance.GetInteropSettingByAppId(app.Id);
                        MpConsole.WriteLine("Dragging over " + targetProcessPath);
                    }

                    bool ignoreFileDrop = false;
                    if (targetInteropSettings != null) {
                        // order and set data object entry by priority (ignoring < 0) and formatInfo 
                        var targetFormats = targetInteropSettings.Items
                                                .Where(x => !x.IgnoreFormat).ToList();

                        ignoreFileDrop = targetInteropSettings.Items
                                            .Where(x => x.ClipboardFormatType == MpClipboardFormatType.FileDrop)
                                            .All(x => x.IgnoreFormat);

                        foreach (var targetSetting in targetFormats.OrderByDescending(x => x.IgnoreFormat)) {
                            switch (targetSetting.ClipboardFormatType) {
                                case MpClipboardFormatType.FileDrop:
                                    if (!string.IsNullOrEmpty(targetSetting.FormatInfo)) {
                                        sctfl.Add(ci.ItemData.ToFile(null, ci.Title, targetSetting.FormatInfo));
                                    } else {
                                        sctfl.Add(ci.ItemData.ToFile(null, ci.Title));
                                    }
                                    break;
                                default:
                                    sctfl.Add(ci.ItemData.ToFile(null, ci.Title, targetSetting.FormatInfo));
                                    break;
                            }
                        }
                    } else {
                        // NOTE using plain text here for more compatibility
                        sctfl.Add(ci.ItemData.ToFile(null, ci.Title));
                    }

                    if(!ignoreFileDrop) {
                        d.DataFormatLookup.AddOrReplace(MpClipboardFormatType.FileDrop, string.Join(Environment.NewLine, sctfl));
                    }
                }

                //set rtf and text
                if (!string.IsNullOrEmpty(rtf)) {
                    d.DataFormatLookup.AddOrReplace(MpClipboardFormatType.Rtf, rtf);
                }
                if (!string.IsNullOrEmpty(pt)) {
                    d.DataFormatLookup.AddOrReplace(MpClipboardFormatType.Text, pt);
                }
                //set image
                if (ci.ItemType == MpCopyItemType.Image) {
                    d.DataFormatLookup.AddOrReplace(MpClipboardFormatType.Bitmap, ci.ItemData);
                }
                //set csv
                string sctcsv = string.Join(Environment.NewLine, ci.ItemData.ToCsv());
                if (!string.IsNullOrWhiteSpace(sctcsv)) {
                    d.DataFormatLookup.AddOrReplace(MpClipboardFormatType.Csv, sctcsv);
                }
            } 

            //set resorting
            //if (isDragDrop && SelectedItems != null && SelectedItems.Count > 0) {
            //    foreach (var dctvm in SelectedItems) {
            //        if (dctvm.Count == 0 ||
            //            dctvm.SelectedItems.Count == dctvm.Count ||
            //            dctvm.SelectedItems.Count == 0) {
            //            //dctvm.IsClipDragging = true;
            //        }
            //    }
            //    //d.SetData(MpPreferences.ClipTileDragDropFormatName, SelectedItems.ToList());
            //}

            return d;
        }

        public async Task<MpPortableDataObject> GetClipTileDataObjectAsync(MpClipTileViewModel ctvm, bool isDragDrop, object targetHandleObj) {
            // NOTE this is NOT part of data object interface (which is in MonkeyPaste.Plugin)
            // because it needs MpCopyItem
            // and am trying to isolate data object for pluggability

            IntPtr targetHandle = targetHandleObj == null ? IntPtr.Zero : (IntPtr)targetHandleObj;
            bool isToExternalApp = targetHandle != IntPtr.Zero && targetHandle != MpProcessManager.GetThisApplicationMainWindowHandle();

            MpPortableDataObject d = new MpPortableDataObject();
            string rtf = string.Empty.ToRichText();
            string pt = string.Empty;
            var sctfl = new List<string>();

            foreach(var civm in ctvm.Items.OrderBy(x=>x.CompositeSortOrderIdx)) {
                var sub_d = await GetCopyItemDataObjectAsync(civm.CopyItem, isDragDrop, targetHandleObj);
                foreach(var sub_d_kvp in sub_d.DataFormatLookup) {
                    if(!d.DataFormatLookup.ContainsKey(sub_d_kvp.Key)) {
                        d.DataFormatLookup.Add(sub_d_kvp.Key, sub_d_kvp.Value);
                    } else {

                    }
                }
            }

            return d;
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
                case MpClipboardFormatType.InternalContent:
                    return MpPortableDataObject.InternalContentFormat;
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
