using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace MpWpfApp {
    public class MpCopyHelper {
        


        //SelectedClipTilesMergedPlainText
        //SelectedClipTilesCsv

        //SelectedClipTilesFileList
        //SelectedClipTilesMergedPlainTextFileList
        //SelectedClipTilesMergedRtfFileList


        //private RelayCommand<int> _exportSelectedClipTilesCommand;
        //public ICommand ExportSelectedClipTilesCommand {
        //    get {
        //        if (_exportSelectedClipTilesCommand == null) {
        //            _exportSelectedClipTilesCommand = new RelayCommand<int>(ExportSelectedClipTiles);
        //        }
        //        return _exportSelectedClipTilesCommand;
        //    }
        //}
        //private void ExportSelectedClipTiles(int exportType) {
        //    CommonFileDialog dlg = ((MpExportType)exportType == MpExportType.Csv || (MpExportType)exportType == MpExportType.Zip) ? new CommonSaveFileDialog() as CommonFileDialog : new CommonOpenFileDialog();
        //    dlg.Title = (MpExportType)exportType == MpExportType.Csv ? "Export CSV" : (MpExportType)exportType == MpExportType.Zip ? "Export Zip" : "Export Items to Directory...";
        //    if ((MpExportType)exportType != MpExportType.Files) {
        //        dlg.DefaultFileName = "Mp_Exported_Data_" + MpHelpers.Instance.RemoveSpecialCharacters(DateTime.Now.ToString());
        //        dlg.DefaultExtension = (MpExportType)exportType == MpExportType.Csv ? "csv" : "zip";
        //    } else {
        //        ((CommonOpenFileDialog)dlg).IsFolderPicker = true;
        //    }
        //    dlg.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

        //    dlg.AddToMostRecentlyUsedList = false;
        //    //dlg.AllowNonFileSystemItems = false;
        //    dlg.DefaultDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        //    dlg.EnsureFileExists = true;
        //    dlg.EnsurePathExists = true;
        //    dlg.EnsureReadOnly = false;
        //    dlg.EnsureValidNames = true;
        //    //dlg.Multiselect = false;
        //    dlg.ShowPlacesList = true;

        //    if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
        //        if ((MpExportType)exportType == MpExportType.Csv) {
        //            ExportClipsToCsvFile(SelectedClipTiles.ToList(), dlg.FileName);
        //        } else if ((MpExportType)exportType == MpExportType.Zip) {
        //            ExportClipsToZipFile(SelectedClipTiles.ToList(), dlg.FileName);
        //        } else {
        //            ExportClipsToFile(SelectedClipTiles.ToList(), dlg.FileName + @"\");
        //        }
        //    }
        //}

        //public async Task FillAllTemplates() {
        //    bool hasExpanded = false;
        //    foreach (var rtbvm in SubSelectedContentItems) {
        //        if (rtbvm.HasTokens) {
        //            rtbvm.IsSelected = true;
        //            rtbvm.IsPastingTemplate = true;
        //            if (!hasExpanded) {
        //                //tile will be shrunk in on completed of hide window
        //                MainWindowViewModel.ExpandClipTile(HostClipTileViewModel);
        //                if (!MpClipTrayViewModel.Instance.IsPastingHotKey) {
        //                    PasteTemplateToolbarViewModel.IsBusy = true;
        //                }
        //                hasExpanded = true;
        //            }
        //            PasteTemplateToolbarViewModel.SetSubItem(rtbvm);
        //            await Application.Current.Dispatcher.BeginInvoke((Action)(() => {
        //                while (!PasteTemplateToolbarViewModel.HaveAllSubItemTemplatesBeenVisited) {
        //                    System.Threading.Thread.Sleep(100);
        //                }
        //            }), DispatcherPriority.Background);

        //            //await Task.Run(() => {
        //            //    while (!HostClipTileViewModel.PasteTemplateToolbarViewModel.HaveAllSubItemTemplatesBeenVisited) {
        //            //        System.Threading.Thread.Sleep(100);
        //            //    }
        //            //    //TemplateRichText is set in PasteTemplateCommand
        //            //});
        //            rtbvm.TemplateHyperlinkCollectionViewModel.ClearSelection();
        //        }

        //    }
        //}



        //public MpCopyItemType GetTargetFileType() {
        //    //string targetTitle = MpClipboardManager.Instance?.LastWindowWatcher.LastTitle.ToLower();
        //    string activeProcessPath = MpRunningApplicationManager.Instance.ActiveProcessPath;
        //    //when targetTitle is empty assume it is explorer and paste as filedrop
        //    if (string.IsNullOrEmpty(activeProcessPath)) {
        //        return MpCopyItemType.FileList;
        //    }
        //    foreach (var imgApp in Properties.Settings.Default.PasteAsImageDefaultProcessNameCollection) {
        //        if (activeProcessPath.ToLower().Contains(imgApp.ToLower())) {
        //            return MpCopyItemType.Image;
        //        }
        //    }
        //    foreach (var fileApp in Properties.Settings.Default.PasteAsFileDropDefaultProcessNameCollection) {
        //        if (activeProcessPath.ToLower().Contains(fileApp.ToLower())) {
        //            return MpCopyItemType.FileList;
        //        }
        //    }
        //    foreach (var csvApp in Properties.Settings.Default.PasteAsCsvDefaultProcessNameCollection) {
        //        if (activeProcessPath.ToLower().Contains(csvApp.ToLower())) {
        //            return MpCopyItemType.Csv;
        //        }
        //    }
        //    foreach (var textApp in Properties.Settings.Default.PasteAsTextFileDefaultProcessNameCollection) {
        //        if (activeProcessPath.ToLower().Contains(textApp.ToLower())) {
        //            return MpCopyItemType.RichText;
        //        }
        //    }
        //    //paste as rtf by default
        //    return MpCopyItemType.None;
        //}

        //public string ExportClipsToFile(List<MpClipTileViewModel> clipList, string rootPath) {
        //    string outStr = string.Empty;
        //    foreach (MpClipTileViewModel ctvm in clipList) {
        //        foreach (string f in ctvm.GetFileList(rootPath)) {
        //            outStr += f + Environment.NewLine;
        //        }
        //    }
        //    return outStr;
        //}

        //public string ExportClipsToCsvFile(List<MpClipTileViewModel> clipList, string filePath) {
        //    string csvText = string.Empty;
        //    foreach (MpClipTileViewModel ctvm in clipList) {
        //        csvText += ctvm.ContentContainerViewModel[0].CopyItemPlainText + ",";
        //    }
        //    using (StreamWriter of = new StreamWriter(filePath)) {
        //        of.Write(csvText);
        //        of.Close();
        //    }
        //    return filePath;
        //}

        //public string ExportClipsToZipFile(List<MpClipTileViewModel> clipList, string filePath) {
        //    using (ZipArchive zip = ZipFile.Open(filePath, ZipArchiveMode.Create)) {
        //        foreach (var ctvm in clipList) {
        //            foreach (var p in ctvm.CopyItemFileDropList) {
        //                zip.CreateEntryFromFile(p, Path.GetFileName(p));
        //            }
        //        }
        //    }
        //    return filePath;
        //}


        //public string SelectedClipTilesCsv {
        //    get {
        //        var sb = new StringBuilder();
        //        foreach (var sctvm in SelectedClipTiles) {
        //            if ((sctvm.CopyItemType != MpCopyItemType.RichText) &&
        //                MpHelpers.Instance.IsStringCsv(sctvm.CopyItem.ItemData)) {
        //                sb.Append(sctvm.CopyItem.ItemData + ",");
        //                continue;
        //            } else {
        //                sb.Append(sctvm.RtbItemCollectionViewModel.SubSelectedClipTilesCsv + ",");
        //            }
        //        }
        //        return sb.ToString();
        //    }
        //}

        //public string[] SelectedClipTilesFileList {
        //    get {
        //        var fl = new List<string>();
        //        foreach (var sctvm in SelectedClipTiles) {
        //            if (sctvm.CopyItemType != MpCopyItemType.RichText) {
        //                foreach (string f in sctvm.CopyItemFileDropList) {
        //                    fl.Add(f);
        //                }
        //                continue;
        //            } else {
        //                foreach (var srtbvm in sctvm.RtbItemCollectionViewModel.SubSelectedContentItems) {
        //                    foreach (string f in srtbvm.CopyItemFileDropList) {
        //                        fl.Add(f);
        //                    }
        //                }
        //            }

        //        }
        //        return fl.ToArray();
        //    }
        //}

        //public string SelectedClipTilesMergedPlainText {
        //    get {
        //        var sb = new StringBuilder();
        //        foreach (var sctvm in SelectedClipTiles) {
        //            if (sctvm.CopyItemType == MpCopyItemType.RichText) {
        //                sb.Append(sctvm.RtbItemCollectionViewModel.SubSelectedClipTilesMergedPlainText + Environment.NewLine);
        //            } else {
        //                sb.Append(sctvm.CopyItemPlainText + Environment.NewLine);
        //            }
        //        }
        //        return sb.ToString().Trim('\r', '\n');
        //    }
        //}

        //public string[] SelectedClipTilesMergedPlainTextFileList {
        //    get {
        //        string mergedPlainTextFilePath = MpHelpers.Instance.WriteTextToFile(
        //            Path.GetTempFileName(), SelectedClipTilesMergedPlainText, true);

        //        return new string[] { mergedPlainTextFilePath };
        //    }
        //}

        //public string SelectedClipTilesMergedRtf {
        //    get {
        //        MpEventEnabledFlowDocument fd = string.Empty.ToRichText().ToFlowDocument();
        //        foreach (var sctvm in SelectedClipTiles) {
        //            if (sctvm.CopyItemType == MpCopyItemType.RichText) {
        //                fd = MpHelpers.Instance.CombineFlowDocuments(
        //                    sctvm.RtbItemCollectionViewModel.SubSelectedClipTilesMergedRtf.ToFlowDocument(),
        //                    fd);
        //            } else {
        //                fd = MpHelpers.Instance.CombineFlowDocuments(sctvm.CopyItemRichText.ToFlowDocument(), fd);
        //            }
        //        }
        //        return fd.ToRichText();
        //    }
        //}


        //public string[] SelectedClipTilesMergedRtfFileList {
        //    get {
        //        string mergedRichTextFilePath = MpHelpers.Instance.WriteTextToFile(
        //            Path.GetTempFileName(), SelectedClipTilesMergedRtf, true);

        //        return new string[] { mergedRichTextFilePath };
        //    }
        //}



        //public BitmapSource SubSelectedClipTilesBmp {
        //    get {
        //        bool wasEmptySelection = SubSelectedRtbvmList.Count == 0;
        //        if (wasEmptySelection) {
        //            SubSelectAll();
        //        }
        //        var bmpList = new List<BitmapSource>();
        //        foreach (var srtbvm in SubSelectedRtbvmList.OrderBy(x => x.LastSubSelectedDateTime)) {
        //            bmpList.Add(srtbvm.CopyItemBmp);
        //        }
        //        if (wasEmptySelection) {
        //            ClearSubSelection();
        //        }
        //        return MpHelpers.Instance.CombineBitmap(bmpList, false);
        //    }
        //}

        //public string SubSelectedClipTilesCsv {
        //    get {
        //        bool wasEmptySelection = SubSelectedContentItems.Count == 0;
        //        if (wasEmptySelection) {
        //            SubSelectAll();
        //        }
        //        var sb = new StringBuilder();
        //        foreach (var srtbvm in SubSelectedContentItems) {
        //            sb.Append(srtbvm.CopyItem.ItemData + ",");
        //        }
        //        if (wasEmptySelection) {
        //            ClearSubSelection();
        //        }
        //        return sb.ToString();
        //    }
        //}

        //public string[] SubSelectedClipTilesFileList {
        //    get {
        //        bool wasEmptySelection = SubSelectedContentItems.Count == 0;
        //        if (wasEmptySelection) {
        //            SubSelectAll();
        //        }
        //        var fl = new List<string>();
        //        foreach (var srtbvm in SubSelectedContentItems) {
        //            foreach (string f in srtbvm.CopyItemFileDropList) {
        //                fl.Add(f);
        //            }
        //        }
        //        if (wasEmptySelection) {
        //            ClearSubSelection();
        //        }
        //        return fl.ToArray();
        //    }
        //}

        //public string SubSelectedClipTilesMergedPlainText {
        //    get {
        //        bool wasEmptySelection = SubSelectedContentItems.Count == 0;
        //        if (wasEmptySelection) {
        //            SubSelectAll();
        //        }
        //        var sb = new StringBuilder();
        //        foreach (var sctvm in SubSelectedContentItems) {
        //            if (sctvm.HasTokens) {
        //                sb.Append(
        //                    MpHelpers.Instance.ConvertRichTextToPlainText(sctvm.TemplateRichText) + Environment.NewLine);
        //            } else {
        //                sb.Append(sctvm.CopyItemPlainText + Environment.NewLine);
        //            }
        //        }
        //        if (wasEmptySelection) {
        //            ClearSubSelection();
        //        }
        //        return sb.ToString().Trim('\r', '\n');
        //    }
        //}

        //public string[] SubSelectedClipTilesMergedPlainTextFileList {
        //    get {

        //        string mergedPlainTextFilePath = MpHelpers.Instance.WriteTextToFile(
        //            System.IO.Path.GetTempFileName(), SubSelectedClipTilesMergedPlainText, true);

        //        return new string[] { mergedPlainTextFilePath };
        //    }
        //}

        //public string SubSelectedClipTilesMergedRtf {
        //    get {
        //        bool wasEmptySelection = SubSelectedContentItems.Count == 0;
        //        if (wasEmptySelection) {
        //            SubSelectAll();
        //        }
        //        MpEventEnabledFlowDocument fd = string.Empty.ToRichText().ToFlowDocument();
        //        foreach (var sctvm in SubSelectedContentItems.OrderBy(x => x.LastSubSelectedDateTime)) {
        //            if (sctvm.HasTokens) {
        //                fd = MpHelpers.Instance.CombineFlowDocuments(sctvm.TemplateRichText.ToFlowDocument(), fd);
        //            } else {
        //                fd = MpHelpers.Instance.CombineFlowDocuments(sctvm.CopyItemRichText.ToFlowDocument(), fd);
        //            }
        //        }
        //        if (wasEmptySelection) {
        //            ClearSubSelection();
        //        }
        //        return fd.ToRichText();
        //    }
        //}

        //public string[] SubSelectedClipTilesMergedRtfFileList {
        //    get {
        //        string mergedRichTextFilePath = MpHelpers.Instance.WriteTextToFile(
        //            System.IO.Path.GetTempFileName(), SubSelectedClipTilesMergedRtf, true);

        //        return new string[] { mergedRichTextFilePath };
        //    }
        //}
    }
}
