﻿using System;
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

namespace MpWpfApp {
    public class MpWpfPasteHelper : MpIExternalPasteHandler, MpIErrorHandler, MpIPasteObjectBuilder {
        private Queue<MpPasteItem> _pasteQueue = new Queue<MpPasteItem>();
        private static MpWpfPasteHelper _instance;
        public static MpWpfPasteHelper Instance => _instance ?? (_instance = new MpWpfPasteHelper());

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


        public string GetFormat(MpClipboardFormatType format, string data, string fileNameWithoutExtension = "", string directory = "", string textFormat = ".rtf", string imageFormat = ".png", bool isTemporary = false) {
            return new MpWpfPasteObjectBuilder().GetFormat(format, data, fileNameWithoutExtension, directory, textFormat, imageFormat, isTemporary);
        }

        public string GetFormat(MpClipboardFormatType format, string[] data, string[] fileNameWithoutExtension = null, string directory = "", string textFormat = ".rtf", string imageFormat = ".png", bool isTemporary = false, bool isCopy = false) {
            return new MpWpfPasteObjectBuilder().GetFormat(format, data, fileNameWithoutExtension, directory, textFormat, imageFormat, isTemporary, isCopy);
        }

        public async Task PasteDataObject(MpDataObject mpdo, IntPtr handle, bool finishWithEnterKey = false) {
            var pi = new MpProcessInfo() {
                Handle = handle
            };
            await PasteDataObject(mpdo, pi, finishWithEnterKey);
        }

        private void Mwvm_OnMainWindowHide(object sender, EventArgs e) {
            if (_pasteQueue == null || _pasteQueue.IsNullOrEmpty()) {
                return;
            }
            int pasteCount = _pasteQueue.Count;
            while(pasteCount > 0) {
                var pasteItem = _pasteQueue.Dequeue();

                var processInfo = MpProcessHelper.MpProcessAutomation.SetActiveProcess(pasteItem.ProcessInfo);

                if(processInfo != null && processInfo.Handle != IntPtr.Zero) {

                    var ido = (System.Windows.Forms.IDataObject)MpClipboardHelper.MpClipboardManager.InteropService.ConvertToNativeFormat(pasteItem.DataObject);

                    System.Windows.Forms.Clipboard.SetDataObject(ido);
                    Thread.Sleep(100);
                    System.Windows.Forms.SendKeys.SendWait("^v");
                    Thread.Sleep(100);
                    if (pasteItem.FinishWithEnterKey) {
                        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                    }
                }
                pasteCount--;
            }
        }

        public async Task PasteCopyItem(MpCopyItem ci, MpProcessInfo pi, bool finishWithEnterKey = false) {
            MpDataObject cido = await GetCopyItemDataObject(ci, false, pi.Handle);
            await PasteDataObject(cido, pi, finishWithEnterKey);
        }


        public async Task PasteDataObject(MpDataObject mpdo, MpProcessInfo pi, bool finishWithEnterKey = false) {
            var pasteItem = new MpPasteItem() {
                DataObject = mpdo,
                ProcessInfo = pi,
                FinishWithEnterKey = finishWithEnterKey
            };
            _pasteQueue.Enqueue(pasteItem);

            MpConsole.WriteLine("Attempting to paste data object: ");
            MpConsole.WriteLine(mpdo.ToJson().ToPrettyPrintJson());

            if (MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                MpMainWindowViewModel.Instance.IsMainWindowLocked = false;
                
                MpMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
            } else {
                Mwvm_OnMainWindowHide(this, null);
            }
            while(!_pasteQueue.IsNullOrEmpty()) {
                await Task.Delay(100);
            }
        }


        public MpDataObject ConvertToDataObject(string format, object data) {
            throw new NotImplementedException();
        }

        public async Task<MpDataObject> GetCopyItemDataObject(MpCopyItem ci, bool isDragDrop, object targetHandleObj) {
            IntPtr targetHandle = targetHandleObj == null ? IntPtr.Zero : (IntPtr)targetHandleObj;
            bool isToExternalApp = targetHandle != IntPtr.Zero && targetHandle != MpProcessManager.GetThisApplicationMainWindowHandle();

            MpDataObject d = new MpDataObject();
            string rtf = string.Empty.ToRichText();
            string pt = string.Empty;
            var sctfl = new List<string>();

            //check for model templates
            var templates = await MpDataModelProvider.GetTemplatesAsync(ci.Id);
            bool hasTemplates = templates != null && templates.Count > 0;

            if(hasTemplates) {
                // trigger query change before showing main window may need to tweak...
                MpDataModelProvider.SetManualQuery(new List<int>() { ci.Id });
                if (MpMainWindowViewModel.Instance.IsMainWindowOpen == false) {
                    MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    while(MpMainWindowViewModel.Instance.IsMainWindowOpen == false) {
                        await Task.Delay(100);
                    }
                }
                await Task.Delay(50); //wait for clip tray to get query changed message
                while (MpClipTrayViewModel.Instance.IsRequery) {
                    await Task.Delay(100);
                }
                var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(ci.Id);
                if(civm != null) {
                    while(civm.IsBusy || civm.Parent.IsBusy) {
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

            if(isToExternalApp) {
                if(isDragDrop) {
                    string targetProcessPath = MpProcessManager.GetProcessPath(targetHandle);
                    var app = await MpDataModelProvider.GetAppByPath(targetProcessPath);
                    List<MpAppInteropSetting> targetInteropSettings = null;
                    if(app != null) {
                        targetInteropSettings = await MpDataModelProvider.GetInteropSettingsByAppId(app.Id);
                    }
                    if(targetInteropSettings != null) {
                        targetInteropSettings = targetInteropSettings.Where(x=>x.Priority >= 0).OrderByDescending(x => x.Priority).ToList();
                        foreach(var targetSetting in targetInteropSettings.OrderByDescending(x=>x.Priority)) {
                            switch(targetSetting.FormatType) {
                                case MpClipboardFormatType.FileDrop:
                                    if(!string.IsNullOrEmpty(targetSetting.FormatInfo)) {
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

                    ////set file drop (always must set so when dragged out of application user doesn't get no-drop cursor)
                    //if (MpExternalDropBehavior.Instance.IsProcessNeedFileDrop(targetProcessPath)) {
                    //    //only when pasting into explorer or notepad must have file drop

                    //    if (ci.ItemType != MpCopyItemType.FileList &&
                    //        (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                    //        //external drop w/ ctrl down merges all selected items (unless file list)
                    //        // TODO maybe for multiple files w/ ctrl down compress into zip?
                    //        if (MpExternalDropBehavior.Instance.IsProcessLikeNotepad(MpProcessManager.LastProcessPath)) {
                    //            //merge as plain text
                    //            string fp = MpHelpers.GetUniqueFileName(MpExternalDropFileType.Txt, ci.Title);
                    //            sctfl.Add(MpHelpers.WriteTextToFile(fp, pt, true));
                    //        } else {
                    //            //merge as rich text
                    //            string fp = MpHelpers.GetUniqueFileName(MpExternalDropFileType.Rtf, ci.Title);
                    //            sctfl.Add(MpHelpers.WriteTextToFile(fp, rtf, true));
                    //        }
                    //    } else {

                    //    }


                    //    // d.SetData(MpClipboardFormat.FileDrop, sctfl.ToStringCollection());
                    //}
                    d.DataFormatLookup.AddOrReplace(MpClipboardFormatType.FileDrop, string.Join(Environment.NewLine, sctfl));
                }
            }

            if (isToExternalApp) {
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
            //awaited in MpMainWindowViewModel.Instance.HideWindow
        }

        public void HandleError(Exception ex) {
            MpConsole.WriteTraceLine(ex);
        }

        #region Private Methods

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

        #endregion

        internal class MpPasteItem {
            internal MpDataObject DataObject { get; set; }
            internal MpProcessInfo ProcessInfo { get; set; }
            internal bool FinishWithEnterKey { get; set; }
        }

    }
}