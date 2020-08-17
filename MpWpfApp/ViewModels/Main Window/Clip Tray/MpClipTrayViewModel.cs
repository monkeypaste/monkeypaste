using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpClipTrayViewModel : MpObservableCollectionViewModel<MpClipTileViewModel> {
        
        #region View Models
        public MpMainWindowViewModel MainWindowViewModel { get; set; }

        public List<MpClipTileViewModel> SelectedClipTiles {
            get {
                return this.Where(ct => ct.IsSelected).ToList();
            }
        }

        public List<MpClipTileViewModel> VisibileClipTiles {
            get {
                return this.Where(ct => ct.TileVisibility == Visibility.Visible).ToList();
            }
        }

        public MpClipTileViewModel FocusedClipTile {
            get {
                var tempList = this.Where(ct => ct.IsSelected && ct.IsFocused).ToList();
                if (tempList == null || tempList.Count == 0) {
                    return null;
                }
                return tempList[0];
            }
        }
        #endregion

        #region Properties

        public bool DoPaste { get; set; } = false;

        private Visibility _emptyListMessageVisibility = Visibility.Collapsed;
        public Visibility EmptyListMessageVisibility {
            get {
                return _emptyListMessageVisibility;
            }
            set {
                if (_emptyListMessageVisibility != value) {
                    _emptyListMessageVisibility = value;
                    OnPropertyChanged(nameof(EmptyListMessageVisibility));
                }
            }
        }

        private Visibility _clipListVisibility = Visibility.Visible;
        public Visibility ClipListVisibility {
            get {
                return _clipListVisibility;
            }
            set {
                if (_clipListVisibility != value) {
                    _clipListVisibility = value;
                    OnPropertyChanged(nameof(ClipListVisibility));
                }
            }
        }

        private Visibility _mergeClipsCommandVisibility = Visibility.Collapsed;
        public Visibility MergeClipsCommandVisibility {
            get {
                return _mergeClipsCommandVisibility;
            }
            set {
                if (_mergeClipsCommandVisibility != value) {
                    _mergeClipsCommandVisibility = value;
                    OnPropertyChanged(nameof(MergeClipsCommandVisibility));
                }
            }
        }
        #endregion

        #region Public Methods
        public void ClearClipSelection() {
            foreach (var clip in this) {
                clip.IsSelected = false;
                clip.IsFocused = false;
            }
        }

        public void ResetClipSelection() {
            ClearClipSelection();
            if (VisibileClipTiles.Count > 0) {
                VisibileClipTiles[0].IsSelected = true;
            }
        }

        public void AddClipTile(MpCopyItem ci, bool isNew = false) {
            if (isNew) {
                ci.WriteToDatabase();
                // TODO move this to mainwindow property changed
                MpTag historyTag = new MpTag(1);
                historyTag.LinkWithCopyItem(ci);
                MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().TagClipCount++;
            }

            MpClipTileViewModel newClipTile = new MpClipTileViewModel(ci, this);

            this.Insert(0, newClipTile);

            //update cliptray visibility if this is the first cliptile added
            ClipListVisibility = Visibility.Visible;
            EmptyListMessageVisibility = Visibility.Collapsed;
        }

        private void RemoveClipTile(MpClipTileViewModel clipTileToRemove) {
            foreach (var ttvm in MainWindowViewModel.TagTrayViewModel) {
                if (ttvm.Tag.IsLinkedWithCopyItem(clipTileToRemove.CopyItem)) {
                    ttvm.TagClipCount--;
                }
            }
            this.Remove(clipTileToRemove);
            clipTileToRemove.CopyItem.DeleteFromDatabase();

            //if this was the last visible clip update the cliptray visibility
            if (VisibileClipTiles.Count == 0) {
                ClipListVisibility = Visibility.Collapsed;
                EmptyListMessageVisibility = Visibility.Visible;
            }
        }

        public void MoveClipTile(MpClipTileViewModel clipTile, int newIdx) {
            if (newIdx > this.Count || newIdx < 0) {
                throw new Exception("Cannot insert tile clip tile at index: " + newIdx + " with list of length: " + this.Count);
            }
            int removeIdx = VisibileClipTiles.IndexOf(clipTile);
            if (removeIdx < 0) {
                throw new Exception("MoveClipTile error can only move visible clip tiles");
            }
            this.Remove(clipTile);
            //SortClipTiles();
            this.Insert(newIdx, clipTile);
        }

        public void PerformSearch() {
            if (MainWindowViewModel.SearchBoxViewModel.SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                FilterTiles(string.Empty);
            } else {
                FilterTiles(MainWindowViewModel.SearchBoxViewModel.SearchText);
            }

            foreach (var vctvm in VisibileClipTiles) {
                vctvm.Highlight(MainWindowViewModel.SearchBoxViewModel.SearchText);
            }
            if (VisibileClipTiles.Count > 0) {
                MainWindowViewModel.SearchBoxViewModel.SearchTextBoxBorderBrush = Brushes.Transparent;
                EmptyListMessageVisibility = Visibility.Collapsed;
                SortClipTiles();
            } else {
                MainWindowViewModel.SearchBoxViewModel.SearchTextBoxBorderBrush = Brushes.Red;
                EmptyListMessageVisibility = Visibility.Visible;
            }
        }

        public void FilterTiles(string searchStr) {
            List<int> filteredTileIdxList = new List<int>();
            //search ci's from newest to oldest for filterstr, adding idx to list
            for (int i = this.Count - 1; i >= 0; i--) {
                //when search string is empty add each item to list so all shown
                if (string.IsNullOrEmpty(searchStr)) {
                    filteredTileIdxList.Add(i);
                    continue;
                }
                MpCopyItem ci = this[i].CopyItem;
                //add clips where searchStr is in clip title or part of the app path ( TODO also check application name since usually different than exe)
                if (ci.Title.ToLower().Contains(searchStr.ToLower()) || ci.App.AppPath.ToLower().Contains(searchStr.ToLower())) {
                    filteredTileIdxList.Add(i);
                    continue;
                }
                //do not search through image tiles
                if (ci.CopyItemType == MpCopyItemType.Image) {
                    continue;
                }
                //add clips where search is part of clip's content
                if (ci.CopyItemType == MpCopyItemType.RichText) {
                    if (ci.GetPlainText().ToLower().Contains(searchStr.ToLower())) {
                        filteredTileIdxList.Add(i);
                    }
                }
                //lastly add filelist clips if search string found in it's path(s)
                else if (ci.CopyItemType == MpCopyItemType.FileList) {
                    foreach (string p in (string[])ci.DataObject) {
                        if (p.ToLower().Contains(searchStr.ToLower())) {
                            filteredTileIdxList.Add(i);
                        }
                    }
                }
            }
            //only show tiles w/ an idx in list
            int vcount = 0;
            for (int i = this.Count - 1; i >= 0; i--) {
                if (filteredTileIdxList.Contains(i)) {
                    this[i].TileVisibility = Visibility.Visible;
                    vcount++;
                } else {
                    this[i].TileVisibility = Visibility.Collapsed;
                }
            }
        }

        public void SortClipTiles() {
            string sortBy = string.Empty;
            bool ascending = MainWindowViewModel.ClipTileSortViewModel.AscSortOrderButtonImageVisibility == Visibility.Visible;

            if (MainWindowViewModel.ClipTileSortViewModel.SelectedSortType.Header == "Date") {
                sortBy = "CopyItemCreatedDateTime";
            } else if (MainWindowViewModel.ClipTileSortViewModel.SelectedSortType.Header == "Application") {
                sortBy = "CopyItemAppId";
            } else if (MainWindowViewModel.ClipTileSortViewModel.SelectedSortType.Header == "Title") {
                sortBy = "Title";
            } else if (MainWindowViewModel.ClipTileSortViewModel.SelectedSortType.Header == "Content") {
                sortBy = "Text";
            } else if (MainWindowViewModel.ClipTileSortViewModel.SelectedSortType.Header == "Type") {
                sortBy = "CopyItemType";
            } else if (MainWindowViewModel.ClipTileSortViewModel.SelectedSortType.Header == "Usage") {
                sortBy = "CopyItemUsageScore";
            }
            ClearClipSelection();
            var sortStart = DateTime.Now;
            this.Sort(x => x[sortBy], !ascending);
            var sortDur = DateTime.Now - sortStart;
            Console.WriteLine("Sort for " + VisibileClipTiles.Count + " items: " + sortDur.TotalMilliseconds + " ms");
            ResetClipSelection();
        }

        private void WriteClipsToFile(List<MpClipTileViewModel> clipList, string rootPath) {
            foreach (MpClipTileViewModel ctvm in clipList) {
                ctvm.WriteCopyItemToFile(rootPath);
            }
        }

        private void WriteClipsToCsvFile(List<MpClipTileViewModel> clipList, string filePath) {
            string csvText = string.Empty;
            foreach (MpClipTileViewModel ctvm in clipList) {
                csvText += ctvm.CopyItem.GetPlainText() + ",";
            }
            StreamWriter of = new StreamWriter(filePath);
            of.Write(csvText);
            of.Close();
        }

        #endregion

        #region Constructor/Init
        public MpClipTrayViewModel(MpMainWindowViewModel parent) {
            MainWindowViewModel = parent;
            
        }

        public void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            var clipTray = (ListBox)sender;
            clipTray.DragEnter += (s,e1)=> {
                e1.Effects = e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName) ? DragDropEffects.Move : DragDropEffects.None;
            };
            clipTray.Drop += (s,e2)=> {
                var dragClipViewModel = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                
                var mpo = e2.GetPosition(clipTray);
                if (mpo.X - dragClipViewModel[0].StartDragPoint.X > 0) {
                    mpo.X -= MpMeasurements.Instance.ClipTileMargin * 5;
                } else {
                    mpo.X += MpMeasurements.Instance.ClipTileMargin * 5;
                }

                MpClipTileViewModel dropVm = null;
                var item = VisualTreeHelper.HitTest(clipTray, mpo).VisualHit;
                dropVm = (MpClipTileViewModel)item.GetVisualAncestor<MpClipBorder>().DataContext;
                int dropIdx = item == null || item == clipTray ? 0 : this.IndexOf(dropVm);
                //if(item.GetType() == typeof(ScrollViewer)) {
                //    dropVm = (MpClipTileViewModel)((ItemsPresenter)((ScrollViewer)item).Content).DataContext;
                //} else if(item.GetType() == typeof(MpClipBorder)) {
                //    dropVm = (MpClipTileViewModel)((MpClipBorder)item).DataContext;
                //}
                if (dropIdx >= 0) {
                    ClearClipSelection();
                    for (int i = 0; i < dragClipViewModel.Count; i++) {
                        this.Remove(dragClipViewModel[i]);
                        this.Insert(dropIdx, dragClipViewModel[i]);
                        dragClipViewModel[i].IsSelected = true;
                        if (i == 0) {
                            dragClipViewModel[i].IsFocused = true;
                        }
                    }
                } else {
                    Console.WriteLine("MainWindow drop error cannot find lasrt moused over tile");
                }
            };
            clipTray.SelectionChanged += (s, e8) => {
                MergeClipsCommandVisibility = MergeClipsCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;
            };
            clipTray.PreviewMouseWheel += (s, e3) => {
                e3.Handled = true;

                var clipTrayListBox = (ListBox)sender;
                var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (e3.Delta * -1) / 5);
            };

            //create tiles for all clips in the database
            foreach (MpCopyItem c in MpCopyItem.GetAllCopyItems()) {
                AddClipTile(c, false);
            }

            SortClipTiles();
        }
        #endregion

        #region Commands
        private RelayCommand _pasteSelectedClipsCommand;
        public ICommand PasteSelectedClipsCommand {
            get {
                if (_pasteSelectedClipsCommand == null) {
                    _pasteSelectedClipsCommand = new RelayCommand(PasteSelectedClips);
                }
                return _pasteSelectedClipsCommand;
            }
        }
        private void PasteSelectedClips() {
            //In order to paste the app must hide first
            //((MpMainWindow)Application.Current.MainWindow).Visibility = Visibility.Collapsed;

            //this triggers hidewindow to paste selected items
            DoPaste = true;
            MainWindowViewModel.HideWindowCommand.Execute(null);
        }

        private RelayCommand _bringSelectedClipTilesToFrontCommand;
        public ICommand BringSelectedClipTilesToFrontCommand {
            get {
                if (_bringSelectedClipTilesToFrontCommand == null) {
                    _bringSelectedClipTilesToFrontCommand = new RelayCommand(BringSelectedClipTilesToFront, CanBringSelectedClipTilesToFront);
                }
                return _bringSelectedClipTilesToFrontCommand;
            }
        }
        private bool CanBringSelectedClipTilesToFront() {
            bool canBringForward = false;
            for (int i = 0; i < SelectedClipTiles.Count; i++) {
                if (!SelectedClipTiles.Contains(VisibileClipTiles[i])) {
                    canBringForward = true;
                    break;
                }
            }
            return canBringForward;
        }
        private void BringSelectedClipTilesToFront() {
            //MpClipTileViewModel[] selectedClipTile_copies = new MpClipTileViewModel[SelectedClipTiles.Count];
            //SelectedClipTiles.CopyTo(selectedClipTile_copies);
            //for (int i = selectedClipTile_copies.Length - 1; i >= 0; i--) {
            //    MoveClipTile(SelectedClipTiles[i], 0);
            //}
            for (int i = SelectedClipTiles.Count - 1; i >= 0; i--) {
                MoveClipTile(SelectedClipTiles[i], 0);
            }
        }

        private RelayCommand _deleteSelectedClipsCommand;
        public ICommand DeleteSelectedClipsCommand {
            get {
                if (_deleteSelectedClipsCommand == null) {
                    _deleteSelectedClipsCommand = new RelayCommand(DeleteSelectedClips);
                }
                return _deleteSelectedClipsCommand;
            }
        }
        private void DeleteSelectedClips() {
            int lastSelectedClipTileIdx = -1;
            foreach (var ct in SelectedClipTiles) {
                lastSelectedClipTileIdx = VisibileClipTiles.IndexOf(ct);
                RemoveClipTile(ct);
            }
            if (VisibileClipTiles.Count > 0) {
                if (lastSelectedClipTileIdx == 0) {
                    VisibileClipTiles[0].IsSelected = true;
                } else {
                    VisibileClipTiles[lastSelectedClipTileIdx - 1].IsSelected = true;
                }
            }
        }

        private RelayCommand _renameClipCommand;
        public ICommand RenameClipCommand {
            get {
                if (_renameClipCommand == null) {
                    _renameClipCommand = new RelayCommand(RenameClip, CanRenameClip);
                }
                return _renameClipCommand;
            }
        }
        private bool CanRenameClip() {
            return SelectedClipTiles.Count == 1;
        }
        private void RenameClip() {
            SelectedClipTiles[0].ClipTileTitleViewModel.IsEditingTitle = true;
            SelectedClipTiles[0].ClipTileTitleViewModel.IsTitleTextBoxFocused = true;
        }

        private RelayCommand<MpTagTileViewModel> _linkTagToCopyItemCommand;
        public ICommand LinkTagToCopyItemCommand {
            get {
                if (_linkTagToCopyItemCommand == null) {
                    _linkTagToCopyItemCommand = new RelayCommand<MpTagTileViewModel>(LinkTagToCopyItem, CanLinkTagToCopyItem);
                }
                return _linkTagToCopyItemCommand;
            }
        }
        private bool CanLinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            //this checks the selected clips association with tagToLink
            //and only returns if ALL selecteds clips are linked or unlinked 
            if (tagToLink == null || SelectedClipTiles == null || SelectedClipTiles.Count == 0) {
                return false;
            }
            if (SelectedClipTiles.Count == 1) {
                return true;
            }
            bool isLastClipTileLinked = tagToLink.Tag.IsLinkedWithCopyItem(SelectedClipTiles[0].CopyItem);
            foreach (var selectedClipTile in SelectedClipTiles) {
                if (tagToLink.Tag.IsLinkedWithCopyItem(selectedClipTile.CopyItem) != isLastClipTileLinked)
                    return false;
            }
            return true;
        }
        private void LinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            bool isUnlink = tagToLink.Tag.IsLinkedWithCopyItem(SelectedClipTiles[0].CopyItem);
            foreach (var selectedClipTile in SelectedClipTiles) {
                if (isUnlink) {
                    tagToLink.Tag.UnlinkWithCopyItem(selectedClipTile.CopyItem);
                    tagToLink.TagClipCount--;
                } else {
                    tagToLink.Tag.LinkWithCopyItem(selectedClipTile.CopyItem);
                    tagToLink.TagClipCount++;
                }
            }
            //tags and clips have 1-to-1 relationship so remove all other links if it exists before creating new one
            //so loop through all selected clips and sub-loop through all tags and remove links if found            
            //foreach (var clipToRemoveOldLink in SelectedClipTiles) {
            //    foreach (var tagTile in TagTiles) {
            //        if (tagTile.Tag.IsLinkedWithCopyItem(clipToRemoveOldLink.CopyItem) && tagTile.TagName != Properties.Settings.Default.HistoryTagTitle) {
            //            tagTile.Tag.UnlinkWithCopyItem(clipToRemoveOldLink.CopyItem);
            //            tagTile.TagClipCount--;
            //            //if tagToLink was already linked this is an unlink so don't do linking loop
            //            if(tagTile == tagToLink) {
            //                return;
            //            }
            //        }
            //    }
            //}
            ////now loop over all selected clips and link to this tag
            //foreach (var clipToLink in SelectedClipTiles) {
            //    tagToLink.Tag.LinkWithCopyItem(clipToLink.CopyItem);
            //    tagToLink.TagClipCount++;
            //    clipToLink.TitleColor = new SolidColorBrush(tagToLink.Tag.TagColor.Color);
            //}
        }

        private RelayCommand<bool> _exportSelectedClipTilesCommand;
        public ICommand ExportSelectedClipTilesCommand {
            get {
                if (_exportSelectedClipTilesCommand == null) {
                    _exportSelectedClipTilesCommand = new RelayCommand<bool>(ExportSelectedClipTiles, CanExportSelectedClipTiles);
                }
                return _exportSelectedClipTilesCommand;
            }
        }
        private bool CanExportSelectedClipTiles(bool toCsv) {
            if (!toCsv) {
                return true;
            }
            foreach (var sctvm in SelectedClipTiles) {
                if (sctvm.CopyItemType != MpCopyItemType.RichText) {
                    return false;
                }
            }
            return true;
        }
        private void ExportSelectedClipTiles(bool toCsv) {
            CommonFileDialog dlg = toCsv ? new CommonSaveFileDialog() as CommonFileDialog : new CommonOpenFileDialog();
            dlg.Title = toCsv ? "Export CSV" : "Export Items to Directory...";
            if (toCsv) {
                dlg.DefaultFileName = "Mp_Exported_Data_" + DateTime.Now.ToString().Replace(@"/", "-");
                dlg.DefaultExtension = "csv";
            } else {
                ((CommonOpenFileDialog)dlg).IsFolderPicker = !toCsv;
            }
            dlg.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            dlg.AddToMostRecentlyUsedList = false;
            //dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            //dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
                if (toCsv) {
                    WriteClipsToCsvFile(SelectedClipTiles.ToList(), dlg.FileName);
                } else {
                    WriteClipsToFile(SelectedClipTiles.ToList(), dlg.FileName);
                }
            }
        }

        private RelayCommand _mergeClipsCommand;
        public ICommand MergeClipsCommand {
            get {
                if (_mergeClipsCommand == null) {
                    _mergeClipsCommand = new RelayCommand(MergeClips, CanMergeClips);
                }
                return _mergeClipsCommand;
            }
        }
        private bool CanMergeClips() {
            if (SelectedClipTiles.Count <= 1) {
                return false;
            }
            bool areAllSameType = true;
            foreach (var sctvm in SelectedClipTiles) {
                if (sctvm.CopyItemType != SelectedClipTiles[0].CopyItemType) {
                    areAllSameType = false;
                }
            }
            return areAllSameType;
        }
        private void MergeClips() {
            var focusedClip = SelectedClipTiles[0];
            List<MpClipTileViewModel> clipTilesToRemove = new List<MpClipTileViewModel>();
            foreach (MpClipTileViewModel selectedClipTile in SelectedClipTiles) {
                if (selectedClipTile == focusedClip) {
                    continue;
                }
                focusedClip.RichText += Environment.NewLine + selectedClipTile.RichText;
                clipTilesToRemove.Add(selectedClipTile);
            }
            foreach (MpClipTileViewModel tileToRemove in clipTilesToRemove) {
                RemoveClipTile(tileToRemove);
            }
            focusedClip.IsSelected = true;
            focusedClip.IsFocused = true;
        }
        #endregion
    }
}
