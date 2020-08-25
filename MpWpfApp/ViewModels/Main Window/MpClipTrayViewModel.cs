using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MpWpfApp {
    public class MpClipTrayViewModel : MpObservableCollectionViewModel<MpClipTileViewModel> {
        #region View Models
        private MpMainWindowViewModel _mainWindowViewModel = null;
        public MpMainWindowViewModel MainWindowViewModel {
            get {
                return _mainWindowViewModel;
            }
            set {
                if(_mainWindowViewModel != value) {
                    _mainWindowViewModel = value;
                    OnPropertyChanged(nameof(MainWindowViewModel));
                }
            }
        }

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

        public MpClipboardMonitor ClipboardMonitor { get; private set; }

        public bool DoPaste { get; set; } = false;

        public Point StartDragPoint;

        private bool _isMouseDown = false;
        public bool IsMouseDown {
            get {
                return _isMouseDown;
            }
            set {
                if (_isMouseDown != value) {
                    _isMouseDown = value;
                    OnPropertyChanged(nameof(IsMouseDown));
                }
            }
        }

        public bool IsEditingClipTitle {
            get {
                foreach(var sctvm in SelectedClipTiles) {
                    if(sctvm.ClipTileTitleViewModel.IsEditingTitle) {
                        return true;
                    }
                }
                return false;
            }
        }

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

        public string SelectedClipTilesPlainText {
            get {
                string outStr = string.Empty;
                foreach (var sctvm in SelectedClipTiles) {
                    outStr += sctvm.ClipTileContentViewModel.PlainText + Environment.NewLine;
                }
                return outStr;
            }
        }

        public string SelectedClipTilesRichText {
            get {
                string outStr = MpHelpers.ConvertPlainTextToRichText(string.Empty);
                foreach (var sctvm in SelectedClipTiles) {
                    outStr = MpHelpers.CombineRichText(outStr, sctvm.CopyItem.GetRichText());
                }
                return outStr;
            }
        }

        public BitmapSource SelectedClipTilesBmp {
            get {
                var bmpList = new List<BitmapSource>();
                foreach (var sctvm in SelectedClipTiles) {
                    bmpList.Add(sctvm.ClipTileContentViewModel.Bmp);
                }
                return MpHelpers.CombineBitmap(bmpList, false);
            }
        }

        public string SelectedClipTilesCsv {
            get {
                string outStr = string.Empty;
                foreach (var sctvm in SelectedClipTiles) {
                    outStr = sctvm.CopyItem.GetPlainText() + ",";
                }
                return outStr;
            }
        }

        public string[] SelectedClipTilesFileList {
            get {
                var fl = new List<string>();
                foreach (var sctvm in SelectedClipTiles) {
                    foreach (string f in sctvm.ClipTileContentViewModel.FileDropList) {
                        fl.Add(f);
                    }
                }
                return fl.ToArray();
            }
        }

        #endregion

        #region Public Methods
        public MpClipTrayViewModel(MpMainWindowViewModel parent) {
            MainWindowViewModel = parent;

            CollectionChanged += (s, e1) => {
                if (VisibileClipTiles.Count > 0) {
                    ClipListVisibility = Visibility.Visible;
                    EmptyListMessageVisibility = Visibility.Collapsed;
                } else {
                    //update cliptray visibility if this is the first cliptile added
                    ClipListVisibility = Visibility.Collapsed;
                    EmptyListMessageVisibility = Visibility.Visible;
                }
            };

            //create tiles for all clips in the database
            foreach (MpCopyItem ci in MpCopyItem.GetAllCopyItems()) {
                this.Add(new MpClipTileViewModel(ci, this));
            }
        }

        public void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            var clipTray = (ListBox)sender;
            clipTray.DragEnter += (s, e1) => {
                //used for resorting
                e1.Effects = e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName) ? DragDropEffects.Move : DragDropEffects.None;
            };
            clipTray.Drop += (s, e2) => {
                var dragClipViewModel = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);

                var mpo = e2.GetPosition(clipTray);
                if (mpo.X - StartDragPoint.X > 0) {
                    mpo.X -= MpMeasurements.Instance.ClipTileMargin * 5;
                } else {
                    mpo.X += MpMeasurements.Instance.ClipTileMargin * 5;
                }

                MpClipTileViewModel dropVm = null;
                var item = VisualTreeHelper.HitTest(clipTray, mpo).VisualHit;
                if(item.GetType() != typeof(Border)) {
                    dropVm = (MpClipTileViewModel)item.GetVisualAncestor<Border>().DataContext;
                } else {
                    dropVm = (MpClipTileViewModel)((Border)item).DataContext;
                }
                int dropIdx = item == null || item == clipTray ? 0 : this.IndexOf(dropVm);
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
                MergeClipsCommandVisibility = MergeSelectedClipsCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;
            };
            clipTray.PreviewMouseWheel += (s, e3) => {
                e3.Handled = true;

                var clipTrayListBox = (ListBox)sender;
                var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (e3.Delta * -1) / 5);
            };

            InitClipboard();          

            SortAndFilterClipTiles();
        }

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTileBorder = (Border)sender;

            clipTileBorder.PreviewMouseLeftButtonDown += (s, e6) => {
                if (e6.ClickCount == 2) {
                    PasteSelectedClipsCommand.Execute(null);
                }
                IsMouseDown = true;
                StartDragPoint = e6.GetPosition((ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray"));
            };
            //Initiate Selected Clips Drag/Drop, Copy/Paste and Export (to file or csv)
            //Strategy: ALL selected items, regardless of type will have text,rtf,img, and file representations
            //          that are appended as text and filelists but  merged into images (by default)
            // TODO Have option to append items to one long image
            clipTileBorder.PreviewMouseMove += (s, e7) => {
                var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");
                var curDragPoint = e7.GetPosition(clipTray);

                if (IsMouseDown && e7.MouseDevice.LeftButton == MouseButtonState.Pressed
                /*&& Math.Abs(curDragPoint.X-StartDragPoint.X) > 50*/) {
                    IDataObject d = new DataObject();
                    d.SetData(DataFormats.FileDrop, SelectedClipTilesFileList);
                    d.SetData(DataFormats.Bitmap, SelectedClipTilesBmp);
                    d.SetData(DataFormats.CommaSeparatedValue, SelectedClipTilesCsv);
                    d.SetData(DataFormats.Rtf, SelectedClipTilesRichText);
                    d.SetData(DataFormats.Text, SelectedClipTilesPlainText);
                    d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedClipTiles.ToList());
                    DragDrop.DoDragDrop(clipTray, d, DragDropEffects.Copy | DragDropEffects.Move);
                } else {
                    IsMouseDown = false;
                    StartDragPoint = new Point ();
                }
            };
            clipTileBorder.PreviewMouseUp += (s, e8) => {
                IsMouseDown = false;
                StartDragPoint = new Point();
            };
        }

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
                VisibileClipTiles[0].IsFocused = true;
            }
        }

        public new void Add(MpClipTileViewModel ctvm) {
            if (ctvm.IsNew) {
                ctvm.CopyItem.WriteToDatabase();
            }

            //MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
            this.Insert(0, ctvm);
        }

        public new void Remove(MpClipTileViewModel clipTileToRemove) {
            foreach (var ttvm in MainWindowViewModel.TagTrayViewModel) {
                if (ttvm.Tag.IsLinkedWithCopyItem(clipTileToRemove.CopyItem)) {
                    ttvm.TagClipCount--;
                }
            }
            base.Remove(clipTileToRemove);
            clipTileToRemove.CopyItem.DeleteFromDatabase();
        }

        public void PerformSearch() {
            if (MainWindowViewModel.SearchBoxViewModel.SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                FilterTiles(string.Empty);
            } else {
                FilterTiles(MainWindowViewModel.SearchBoxViewModel.SearchText);
            }
            foreach (var vctvm in this) {
                //triggers highlight in tokenized rtb
                vctvm.ClipTileContentViewModel.SearchText = MainWindowViewModel.SearchBoxViewModel.SearchText;
            }
            if (VisibileClipTiles.Count > 0) {
                MainWindowViewModel.SearchBoxViewModel.SearchTextBoxBorderBrush = Brushes.Transparent;
                EmptyListMessageVisibility = Visibility.Collapsed;
                SortAndFilterClipTiles();
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
                    foreach (var path in ci.GetFileList()) {
                        if (path.ToLower().Contains(searchStr.ToLower())) {
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

        public void SortAndFilterClipTiles() {
            var sw = new Stopwatch();
            sw.Start();
            ListSortDirection sortDir = MainWindowViewModel.ClipTileSortViewModel.AscSortOrderButtonImageVisibility == Visibility.Visible ? ListSortDirection.Ascending:ListSortDirection.Descending;
            string sortBy = string.Empty;
            switch(MainWindowViewModel.ClipTileSortViewModel.SelectedSortType.Header) {
                case "Date":
                    sortBy = "CopyItemCreatedDateTime";
                    break;
                case "Application":
                    sortBy = "CopyItemAppId";
                    break;
                case "Title":
                    sortBy = "Title";
                    break;
                case "Content":
                    sortBy = "Content";
                    break;
                case "Type":
                    sortBy = "CopyItemType";
                    break;
                case "Usage":
                    sortBy = "CopyItemUsageScore";
                    break;
            }
            ClearClipSelection();
            var cvs = CollectionViewSource.GetDefaultView(this);
            //cvs.SortDescriptions.Clear();
            //cvs.SortDescriptions.Add(new SortDescription(sortBy, sortDir));
            cvs.Filter += item => {
                if (MainWindowViewModel.SearchBoxViewModel.SearchText.Trim() == string.Empty || MainWindowViewModel.SearchBoxViewModel.SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                    return true;
                }
                var ctvm = (MpClipTileViewModel)item;

                if (ctvm.CopyItemType == MpCopyItemType.Image) {
                    return false;
                }
                if (Properties.Settings.Default.IsSearchCaseSensitive) {
                    return ctvm.CopyItem.GetPlainText().Contains(MainWindowViewModel.SearchBoxViewModel.SearchText);
                }
                return ctvm.CopyItem.GetPlainText().ToLower().Contains(MainWindowViewModel.SearchBoxViewModel.SearchText.ToLower());
            };
            this.Sort(x => x[sortBy], sortDir == ListSortDirection.Descending);
            //var sortDur = DateTime.Now - sortStart;
            sw.Stop();
            Console.WriteLine("Sort for " + VisibileClipTiles.Count + " items: " + sw.ElapsedMilliseconds + " ms");
            ResetClipSelection();
        }

        public void PerformPasteSelectedClips() {
            Console.WriteLine("Pasting " + SelectedClipTiles.Count + " items");
            ClipboardMonitor.IgnoreClipboardChangeEvent = true;
            try {
                IDataObject d = new DataObject();
                switch(GetTargetFileType()) {
                    case MpCopyItemType.FileList:
                        d.SetData(DataFormats.FileDrop, SelectedClipTilesFileList);
                        break;
                    case MpCopyItemType.Image:
                        d.SetData(DataFormats.Bitmap, SelectedClipTilesBmp);
                        break;
                    case MpCopyItemType.RichText:
                        d.SetData(DataFormats.Text, SelectedClipTilesPlainText);
                        break;
                    case MpCopyItemType.Csv:
                        d.SetData(DataFormats.CommaSeparatedValue, SelectedClipTilesCsv);
                        break;
                }
                //d.SetData(DataFormats.Rtf, clipTray.SelectedClipTilesRichText);
                
                Clipboard.Clear();
                Clipboard.SetDataObject(d);
                //Clipboard.SetData(DataFormats.FileDrop, clipTray.SelectedClipTilesFileList);
                //Clipboard.SetData(DataFormats.Bitmap, clipTray.SelectedClipTilesBmp);
                //Clipboard.SetData(DataFormats.Rtf, clipTray.SelectedClipTilesRichText);
                //Clipboard.SetData(DataFormats.CommaSeparatedValue, clipTray.SelectedClipTilesCsv);
                //Clipboard.SetData(DataFormats.Text, clipTray.SelectedClipTilesPlainText);
                //WinApi.SetActiveWindow(GetLastWindowWatcher().LastHandle);
                WinApi.SetForegroundWindow(ClipboardMonitor.LastWindowWatcher.LastHandle);
                //System.Windows.Forms.SendKeys.Send("^v");
                System.Windows.Forms.SendKeys.SendWait("^v");
                //PressKey(Keys.ControlKey, false);
                //PressKey(Keys.V, false);
                //PressKey(Keys.V, true);
                //PressKey(Keys.ControlKey, true);


                //creating history item automatically saves it to the db
                foreach (var sctvm in SelectedClipTiles) {
                    new MpPasteHistory(sctvm.CopyItem, ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).ClipTrayViewModel.ClipboardMonitor.LastWindowWatcher.LastHandle);
                }

                //MpSingletonController.Instance.AppendItem = null;
            }
            catch (Exception e) {
                Console.WriteLine("ClipboardMonitor error during paste: " + e.ToString());
            }
            ClipboardMonitor.IgnoreClipboardChangeEvent = false;
        }
        #endregion

        #region Private Methods

        private void InitClipboard() {
            ClipboardMonitor = new MpClipboardMonitor((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));

            // Attach the handler to the event raising on WM_DRAWCLIPBOARD message is received
            ClipboardMonitor.ClipboardChanged += (s, e53) => {
                MpCopyItem newCopyItem = MpCopyItem.CreateFromClipboard(ClipboardMonitor.LastWindowWatcher.LastHandle);
                if (MainWindowViewModel.AppModeViewModel.IsInAppendMode) {
                    //when in append mode just append the new items text to selecteditem
                    SelectedClipTiles[0].AppendContent(new MpClipTileViewModel(newCopyItem, this));
                    return;
                }

                if (newCopyItem != null) {
                    //check if copyitem is duplicate
                    var existingClipTile = FindClipTileByModel(newCopyItem);
                    if (existingClipTile == null) {
                        this.Add(new MpClipTileViewModel(newCopyItem, this));
                    } else {
                        Console.WriteLine("Ignoring duplicate copy item");
                        existingClipTile.CopyItem.CopyCount++;
                        existingClipTile.CopyItem.CopyDateTime = DateTime.Now;
                        this.Move(this.IndexOf(existingClipTile), 0);
                    }

                    ResetClipSelection();
                }
            };
        }

        private MpCopyItemType GetTargetFileType() {
            string targetTitle = ClipboardMonitor.LastWindowWatcher.LastTitle.ToLower();
            foreach (var imgApp in Properties.Settings.Default.PasteAsImageDefaultAppTitleCollection) {
                if (imgApp.ToLower().Contains(targetTitle)) {
                    return MpCopyItemType.Image;
                }
            }
            foreach (var imgApp in Properties.Settings.Default.PasteAsFileDropDefaultAppTitleCollection) {
                if (imgApp.ToLower().Contains(targetTitle)) {
                    return MpCopyItemType.FileList;
                }
            }
            foreach (var imgApp in Properties.Settings.Default.PasteAsCsvDefaultAppTitleCollection) {
                if (imgApp.ToLower().Contains(targetTitle)) {
                    return MpCopyItemType.Csv;
                }
            }
            //paste as rtf by default
            return MpCopyItemType.RichText;
        }

        private void WriteClipsToFile(List<MpClipTileViewModel> clipList, string rootPath) {
            foreach (MpClipTileViewModel ctvm in clipList) {
                ctvm.CopyItem.GetFileList(rootPath);
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

        private MpClipTileViewModel FindClipTileByModel(MpCopyItem ci) {
            foreach(var ctvm in this) {
                if(ctvm.CopyItemType != ci.CopyItemType) {
                    continue;
                }
                switch(ci.CopyItemType) {
                    case MpCopyItemType.RichText:
                    case MpCopyItemType.FileList:
                        if (string.Compare((string)ctvm.CopyItem.DataObject, (string)ci.DataObject) == 0) {
                            return ctvm;
                        }
                        break;
                    case MpCopyItemType.Image:
                        if(MpHelpers.ByteArrayCompare((byte[])ctvm.CopyItem.DataObject,(byte[])ci.DataObject)) {
                            return ctvm;
                        }
                        break;
                }                
            }
            return null;
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
            for (int i = SelectedClipTiles.Count - 1; i >= 0; i--) {
                this.Move(this.IndexOf(SelectedClipTiles[i]), 0);
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
                this.Remove(ct);
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
                if (tagToLink.Tag.IsLinkedWithCopyItem(selectedClipTile.CopyItem) != isLastClipTileLinked) {
                    return false;
                }
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

        private RelayCommand _mergeSelectedClipsCommand;
        public ICommand MergeSelectedClipsCommand {
            get {
                if (_mergeSelectedClipsCommand == null) {
                    _mergeSelectedClipsCommand = new RelayCommand(MergeSelectedClips, CanMergeSelectedClips);
                }
                return _mergeSelectedClipsCommand;
            }
        }
        private bool CanMergeSelectedClips() {
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
        private void MergeSelectedClips() {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                (Action)(() => {
                    var focusedClip = SelectedClipTiles[0];
                    List<MpClipTileViewModel> clipTilesToRemove = new List<MpClipTileViewModel>();
                    foreach (MpClipTileViewModel selectedClipTile in SelectedClipTiles) {
                        if (selectedClipTile == focusedClip) {
                            continue;
                        }
                        focusedClip.AppendContent(selectedClipTile);
                        clipTilesToRemove.Add(selectedClipTile);
                    }
                    foreach (MpClipTileViewModel tileToRemove in clipTilesToRemove) {
                        this.Remove(tileToRemove);
                    }
                    ClearClipSelection();
                    focusedClip.IsSelected = true;
                    focusedClip.IsFocused = true;
                }));            
        }

        private RelayCommand _speakSelectedClipsCommand;
        public ICommand SpeakSelectedClipsCommand {
            get {
                if (_speakSelectedClipsCommand == null) {
                    _speakSelectedClipsCommand = new RelayCommand(SpeakSelectedClips);
                }
                return _speakSelectedClipsCommand;
            }
        }
        private void SpeakSelectedClips() {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background, 
                (Action)(() => {
                using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer()) {
                    foreach (var sctvm in SelectedClipTiles) {
                        speechSynthesizer.Speak(sctvm.ClipTileContentViewModel.PlainText);
                    }
                }
            }));            
        }

        #endregion
    }
}
