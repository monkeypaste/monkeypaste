using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

        public bool IsDragging { get; set; } = false;

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
                    if(sctvm.IsEditingTitle) {
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
                    outStr += sctvm.PlainText + Environment.NewLine;
                }
                return outStr;
            }
        }

        public string SelectedClipTilesRichText {
            get {
                string outStr = MpHelpers.ConvertPlainTextToRichText(string.Empty);
                foreach (var sctvm in SelectedClipTiles) {
                    outStr = MpHelpers.CombineRichText(outStr, sctvm.CopyItem.ItemRichText);
                }
                return outStr;
            }
        }

        public BitmapSource SelectedClipTilesBmp {
            get {
                var bmpList = new List<BitmapSource>();
                foreach (var sctvm in SelectedClipTiles) {
                    bmpList.Add(sctvm.Bmp);
                }
                return MpHelpers.CombineBitmap(bmpList, false);
            }
        }

        public string SelectedClipTilesCsv {
            get {
                string outStr = string.Empty;
                foreach (var sctvm in SelectedClipTiles) {
                    outStr = sctvm.CopyItem.ItemPlainText + ",";
                }
                return outStr;
            }
        }

        public string[] SelectedClipTilesFileList {
            get {
                var fl = new List<string>();
                foreach (var sctvm in SelectedClipTiles) {
                    foreach (string f in sctvm.FileDropList) {
                        fl.Add(f);
                    }
                }
                return fl.ToArray();
            }
        }

        public IDataObject SelectedClipTilesDropDataObject {
            get {
                IDataObject d = new DataObject();
                d.SetData(DataFormats.FileDrop, SelectedClipTilesFileList);
                d.SetData(DataFormats.Bitmap, SelectedClipTilesBmp);
                d.SetData(DataFormats.CommaSeparatedValue, SelectedClipTilesCsv);
                d.SetData(DataFormats.Rtf, SelectedClipTilesRichText);
                d.SetData(DataFormats.Text, SelectedClipTilesPlainText);
                d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedClipTiles.ToList());
                return d;
            }
        }

        public IDataObject SelectedClipTilesPasteDataObject {
            get {
                IDataObject d = new DataObject();
                //only when pasting into explorer must have file drop
                if(string.IsNullOrEmpty(ClipboardMonitor.LastWindowWatcher.LastTitle.Trim())) {
                    d.SetData(DataFormats.FileDrop, SelectedClipTilesFileList);
                }                
                d.SetData(DataFormats.Bitmap, SelectedClipTilesBmp);
                d.SetData(DataFormats.CommaSeparatedValue, SelectedClipTilesCsv);
                d.SetData(DataFormats.Rtf, SelectedClipTilesRichText);
                d.SetData(DataFormats.Text, SelectedClipTilesPlainText);
                d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedClipTiles.ToList());
                return d;
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
            var clipTray = (MpMultiSelectListBox)sender;
            clipTray.DragEnter += (s, e1) => {
                //used for resorting
                e1.Effects = e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName) ? DragDropEffects.Move : DragDropEffects.None;
            };
            clipTray.Drop += (s, e2) => {
                //retrieve custom dataformat object (cliptileviewmodel)
                var dragClipViewModel = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                
                //using current mp if drag is to the right (else part) adjust point to locate next tile, otherwise adjust to point to previous tile
                var mpo = e2.GetPosition(clipTray);
                bool isDragLeft = true;
                if (mpo.X - StartDragPoint.X > 0) {
                    mpo.X -= MpMeasurements.Instance.ClipTileMargin * 5;
                } else {
                    mpo.X += MpMeasurements.Instance.ClipTileMargin * 5;
                    isDragLeft = false;
                }

                MpClipTileViewModel dropVm = null;
                var item = VisualTreeHelper.HitTest(clipTray, mpo).VisualHit;
                if(item.GetType() != typeof(MpClipBorder)) {
                    var clipBorder = item.GetVisualAncestor<MpClipBorder>();
                    //handle case if tile is dragged to end of list
                    if(clipBorder == null) {
                        dropVm = VisibileClipTiles[VisibileClipTiles.Count - 1];
                    } else {
                        dropVm = (MpClipTileViewModel)clipBorder.DataContext;
                    }
                } else {
                    dropVm = (MpClipTileViewModel)((MpClipBorder)item).DataContext;
                }
                //var dropClipBorder = (MpClipBorder)ItemsControl.ItemsControlFromItemContainer(clipTray).ItemContainerGenerator.ContainerFromItem(dropVm);
                int dropIdx = item == null || item == clipTray ? 0 : this.IndexOf(dropVm);
                if (dropIdx >= 0) {
                    ClearClipSelection();
                    for (int i = 0; i < dragClipViewModel.Count; i++) {
                        int dragIdx = this.IndexOf(dragClipViewModel[i]);
                        this.Move(dragIdx, dropIdx);
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

                foreach(var ttvm in MainWindowViewModel.TagTrayViewModel) {
                    if(ttvm == MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel() || ttvm.IsSelected) {
                        continue;
                    }
                    bool isTagLinkedToAllSelectedClips = true;
                    foreach(var sctvm in SelectedClipTiles) {
                        if(!ttvm.Tag.IsLinkedWithCopyItem(sctvm.CopyItem)) {
                            isTagLinkedToAllSelectedClips = false;
                        }
                    }
                    ttvm.IsHovering = isTagLinkedToAllSelectedClips;
                }
            };
            clipTray.PreviewMouseWheel += (s, e3) => {
                e3.Handled = true;

                var clipTrayListBox = (ListBox)sender;
                var scrollViewer = clipTrayListBox.GetChildOfType<ScrollViewer>();
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (e3.Delta * -1) / 5);
            };

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

            SortAndFilterClipTiles();
        }

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTileBorder = (MpClipBorder)sender;

            clipTileBorder.PreviewMouseLeftButtonDown += (s, e6) => {
                if (e6.ClickCount == 2) {
                    PasteSelectedClipsCommand.Execute(null);
                    return;
                }
                var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");
                IsMouseDown = true;
                StartDragPoint = e6.GetPosition(clipTray);
            };
            //Initiate Selected Clips Drag/Drop, Copy/Paste and Export (to file or csv)
            //Strategy: ALL selected items, regardless of type will have text,rtf,img, and file representations
            //          that are appended as text and filelists but  merged into images (by default)
            // TODO Have option to append items to one long image
            clipTileBorder.PreviewMouseMove += (s, e7) => {
                var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");
                var curDragPoint = e7.GetPosition(clipTray);

                if (IsMouseDown && 
                    !IsDragging &&
                    e7.MouseDevice.LeftButton == MouseButtonState.Pressed && 
                    (Math.Abs(curDragPoint.Y - StartDragPoint.Y) > 5 || Math.Abs(curDragPoint.X - StartDragPoint.X) > 5)) {
                    DragDrop.DoDragDrop(clipTray, SelectedClipTilesDropDataObject, DragDropEffects.Copy | DragDropEffects.Move);
                    IsDragging = true;
                } else if(IsDragging) {
                    IsMouseDown = false;
                    IsDragging = false;
                    StartDragPoint = new Point ();
                }
            };
            clipTileBorder.PreviewMouseUp += (s, e8) => {
                IsMouseDown = false;
                IsDragging = false;
                StartDragPoint = new Point();
            };
        }

        public MpCopyItemType GetSelectedClipsType() {
            //returns none if all clips aren't the same type
            if(SelectedClipTiles.Count == 0) {
                return MpCopyItemType.None;
            }
            MpCopyItemType firstType = SelectedClipTiles[0].CopyItemType;
            foreach(var sctvm in SelectedClipTiles) {
                if(sctvm.CopyItemType != firstType) {
                    return MpCopyItemType.None;
                }
            }
            return firstType;
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
                if(!MainWindowViewModel.SearchBoxViewModel.IsFocused) {
                    VisibileClipTiles[0].IsFocused = true;
                }
            }
        }

        public new void Add(MpClipTileViewModel ctvm) {
            if (ctvm.IsNew) {
                ctvm.CopyItem.WriteToDatabase();
                MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
            }

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

        //public new void Move(int oldIdx,int newIdx) {
        //    var clipTray = (ListBox)Application.Current.MainWindow.FindName("ClipTray");

        //    DoubleAnimation ta = new DoubleAnimation();
        //    Point p = clipTray.Items[newIdx].TranslatePoint(new Point(0.0, 0.0), Window.GetWindow(listboxItem));
        //    ta.From = _startMainWindowTop;
        //    ta.To = _endMainWindowTop;
        //    ta.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
        //    CubicEase easing = new CubicEase();
        //    easing.EasingMode = EasingMode.EaseIn;
        //    ta.EasingFunction = easing;
        //    ta.Completed += (s, e1) => {
        //        IsLoading = false;
        //    };
        //    mw.BeginAnimation(Window.TopProperty, ta);
        //}

        public void SortAndFilterClipTiles(bool doSort = true,bool doFilter = true) {
            if(MainWindowViewModel.IsLoading) {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            
            ClearClipSelection();
            var cvs = CollectionViewSource.GetDefaultView(VisibileClipTiles);
            var tempSearchText = MainWindowViewModel.SearchBoxViewModel.SearchText;
            if (doFilter) {
                cvs.Filter += item => {
                    var ctvm = (MpClipTileViewModel)item;

                    if (tempSearchText.Trim() == string.Empty || tempSearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                        return true;
                    }

                    if (ctvm.CopyItemType == MpCopyItemType.Image) {
                        return false;
                    }

                    if (Properties.Settings.Default.IsSearchCaseSensitive) {
                        return ctvm.CopyItem.ItemPlainText.Contains(tempSearchText);
                    }
                    return ctvm.CopyItem.ItemPlainText.ToLower().Contains(tempSearchText.ToLower());
                };
            }

            if(doSort) {
                ListSortDirection sortDir = MainWindowViewModel.ClipTileSortViewModel.AscSortOrderButtonImageVisibility == Visibility.Visible ? ListSortDirection.Ascending : ListSortDirection.Descending;
                string sortBy = string.Empty;
                switch (MainWindowViewModel.ClipTileSortViewModel.SelectedSortType.Header) {
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
                //cvs.SortDescriptions.Clear();
                //cvs.SortDescriptions.Add(new SortDescription(sortBy, sortDir));
                this.Sort(x => x[sortBy], sortDir == ListSortDirection.Descending);
            }
            sw.Stop();
            Console.WriteLine("Sort for " + VisibileClipTiles.Count + " items: " + sw.ElapsedMilliseconds + " ms");
            ResetClipSelection();
        }

        public void PerformPasteSelectedClips() {
            Console.WriteLine("Pasting " + SelectedClipTiles.Count + " items");
            ClipboardMonitor.IgnoreClipboardChangeEvent = true;
            try {
                Clipboard.Clear();
                Clipboard.SetDataObject(SelectedClipTilesPasteDataObject);
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
            }
            catch (Exception e) {
                Console.WriteLine("ClipboardMonitor error during paste: " + e.ToString());
            }
            ClipboardMonitor.IgnoreClipboardChangeEvent = false;
        }

        public MpCopyItemType GetTargetFileType() {
            string targetTitle = ClipboardMonitor?.LastWindowWatcher.LastTitle.ToLower();

            //when targetTitle is empty assume it is explorer and paste as filedrop
            if (string.IsNullOrEmpty(targetTitle)) {
                return MpCopyItemType.FileList;
            }
            foreach (var imgApp in Properties.Settings.Default.PasteAsImageDefaultAppTitleCollection) {
                if (targetTitle.ToLower().Contains(imgApp.ToLower())) {
                    return MpCopyItemType.Image;
                }
            }
            foreach (var fileApp in Properties.Settings.Default.PasteAsFileDropDefaultAppTitleCollection) {
                if (targetTitle.ToLower().Contains(fileApp.ToLower())) {
                    return MpCopyItemType.FileList;
                }
            }
            foreach (var csvApp in Properties.Settings.Default.PasteAsCsvDefaultAppTitleCollection) {
                if (targetTitle.ToLower().Contains(csvApp.ToLower())) {
                    return MpCopyItemType.Csv;
                }
            }
            foreach (var textApp in Properties.Settings.Default.PasteAsTextFileDefaultAppTitleCollection) {
                if (targetTitle.ToLower().Contains(textApp.ToLower())) {
                    return MpCopyItemType.RichText;
                }
            }
            //paste as rtf by default
            return MpCopyItemType.None;
        }

        public string ExportClipsToFile(List<MpClipTileViewModel> clipList, string rootPath) {
            string outStr = string.Empty;
            foreach (MpClipTileViewModel ctvm in clipList) {
                foreach(string f in ctvm.CopyItem.GetFileList(rootPath)) {
                    outStr += f + Environment.NewLine;
                }
            }
            return outStr;
        }

        public string ExportClipsToCsvFile(List<MpClipTileViewModel> clipList, string filePath) {
            string csvText = string.Empty;
            foreach (MpClipTileViewModel ctvm in clipList) {
                csvText += ctvm.CopyItem.ItemPlainText + ",";
            }
            using (StreamWriter of = new StreamWriter(filePath)) {
                of.Write(csvText);
                of.Close();
            }
            return filePath;
        }

        public string ExportClipsToZipFile(List<MpClipTileViewModel> clipList, string filePath) {
            using (ZipArchive zip = ZipFile.Open(filePath, ZipArchiveMode.Create)) {
                foreach (var ctvm in clipList) {
                    foreach (var p in ctvm.FileDropList) {
                        zip.CreateEntryFromFile(p, Path.GetFileName(p));
                    }
                }
            }
            return filePath;
        }

        #endregion

        #region Private Methods

        private int GetClipTileFromDrag(Point startLoc,Point curLoc) {
            return 0;
        }

        private MpClipTileViewModel FindClipTileByModel(MpCopyItem ci) {
            foreach(var ctvm in this) {
                if(ctvm.CopyItemType != ci.CopyItemType) {
                    continue;
                }
                switch(ci.CopyItemType) {
                    case MpCopyItemType.RichText:
                        if (string.Compare((string)ctvm.CopyItem.ItemRichText, ci.ItemRichText) == 0) {
                            return ctvm;
                        }
                        break;
                    case MpCopyItemType.FileList:
                        if (string.Compare((string)ctvm.CopyItem.ItemPlainText, ci.ItemPlainText) == 0) {
                            return ctvm;
                        }
                        break;
                    case MpCopyItemType.Image:
                        if(MpHelpers.ByteArrayCompare(MpHelpers.ConvertBitmapSourceToByteArray(ctvm.CopyItem.ItemBitmapSource), MpHelpers.ConvertBitmapSourceToByteArray(ci.ItemBitmapSource))) {
                            return ctvm;
                        }
                        break;
                }                
            }
            return null;
        }

        #endregion

        #region Commands

        private RelayCommand<int> _convertSelectedClipsCommand;
        public ICommand ConvertSelectedClipsCommand {
            get {
                if(_convertSelectedClipsCommand == null) {
                    _convertSelectedClipsCommand = new RelayCommand<int>(ConvertSelectedClips);
                }
                return _convertSelectedClipsCommand;
            }
        }
        private void ConvertSelectedClips(int conversionType) {
            MpCopyItemType ct = (MpCopyItemType)conversionType;
            foreach(var sctvm in SelectedClipTiles) {
                if(sctvm.CopyItemType == ct) {
                    continue;
                }
                sctvm.Convert(ct);
            }
        }

        private RelayCommand _changeSelectedClipsColorCommand;
        public ICommand ChangeSelectedClipsColorCommand {
            get {
                if (_changeSelectedClipsColorCommand == null) {
                    _changeSelectedClipsColorCommand = new RelayCommand(ChangeSelectedClipsColor);
                }
                return _changeSelectedClipsColorCommand;
            }
        }
        private void ChangeSelectedClipsColor() {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            cd.ShowHelp = true;
            cd.Color = MpHelpers.ConvertSolidColorBrushToWinFormsColor((SolidColorBrush)SelectedClipTiles[0].TitleColor);
            cd.CustomColors = Properties.Settings.Default.UserCustomColorIdxArray;

            var mw = (MpMainWindow)Application.Current.MainWindow;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = true;
            // Update the text box color if the user clicks OK 
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                BitmapSource sharedSwirl = null;
                foreach(var sctvm in SelectedClipTiles) {
                    sctvm.TitleColor = MpHelpers.ConvertWinFormsColorToSolidColorBrush(cd.Color);
                    if(sharedSwirl == null) {
                        sctvm.InitSwirl();
                        sharedSwirl = sctvm.TitleSwirl;
                    } else {
                        sctvm.InitSwirl(sharedSwirl);
                    }
                    sctvm.CopyItem.WriteToDatabase();
                }
            }
            Properties.Settings.Default.UserCustomColorIdxArray = cd.CustomColors;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = false;
        }

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
            ((MpMainWindow)Application.Current.MainWindow).Visibility = Visibility.Collapsed;

            //this triggers hidewindow to paste selected items
            //DoPaste = true;
            //MainWindowViewModel.HideWindowCommand.Execute(null);

            PerformPasteSelectedClips();
            for (int i = SelectedClipTiles.Count - 1; i >= 0; i--) {
                var sctvm = SelectedClipTiles[i];
                Move(IndexOf(sctvm), 0);
            }
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
            if(VisibileClipTiles.Count == 0) {
                return false;
            }
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
            foreach (var sctvm in SelectedClipTiles) {
                this.Move(this.IndexOf(sctvm), 0);
            }
        }

        private RelayCommand _sendSelectedClipTilesToBackCommand;
        public ICommand SendSelectedClipTilesToBackCommand {
            get {
                if (_sendSelectedClipTilesToBackCommand == null) {
                    _sendSelectedClipTilesToBackCommand = new RelayCommand(SendSelectedClipTilesToBack, CanSendSelectedClipTilesToBack);
                }
                return _sendSelectedClipTilesToBackCommand;
            }
        }
        private bool CanSendSelectedClipTilesToBack() {
            if (VisibileClipTiles.Count == 0) {
                return false;
            }
            bool canSendBack = false;
            for (int i = 0; i < SelectedClipTiles.Count; i++) {
                if (!SelectedClipTiles.Contains(VisibileClipTiles[VisibileClipTiles.Count - 1 - i])) {
                    canSendBack = true;
                    break;
                }
            }
            return canSendBack;
        }
        private void SendSelectedClipTilesToBack() {
            foreach(var sctvm in SelectedClipTiles) {
                this.Move(this.IndexOf(sctvm), this.Count - 1);
            }
            //for (int i = 0; i < SelectedClipTiles.Count; i++) {
            //    this.Move(VisibileClipTiles.IndexOf(SelectedClipTiles[i]), VisibileClipTiles.Count - 1 - i);
            //}
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
            SelectedClipTiles[0].IsEditingTitle = true;
            SelectedClipTiles[0].IsTitleTextBoxFocused = true;
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

        private RelayCommand<int> _exportSelectedClipTilesCommand;
        public ICommand ExportSelectedClipTilesCommand {
            get {
                if (_exportSelectedClipTilesCommand == null) {
                    _exportSelectedClipTilesCommand = new RelayCommand<int>(ExportSelectedClipTiles);
                }
                return _exportSelectedClipTilesCommand;
            }
        }
        private void ExportSelectedClipTiles(int exportType) {
            CommonFileDialog dlg = ((MpExportType)exportType == MpExportType.Csv || (MpExportType)exportType == MpExportType.Zip) ? new CommonSaveFileDialog() as CommonFileDialog : new CommonOpenFileDialog();
            dlg.Title = (MpExportType)exportType == MpExportType.Csv ? "Export CSV" : (MpExportType)exportType == MpExportType.Zip ? "Export Zip":"Export Items to Directory...";
            if ((MpExportType)exportType != MpExportType.Files) {
                dlg.DefaultFileName = "Mp_Exported_Data_" + MpHelpers.RemoveSpecialCharacters(DateTime.Now.ToString());
                dlg.DefaultExtension = (MpExportType)exportType == MpExportType.Csv ? "csv" : "zip";
            } else {
                ((CommonOpenFileDialog)dlg).IsFolderPicker = true;
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
                if ((MpExportType)exportType == MpExportType.Csv) {
                    ExportClipsToCsvFile(SelectedClipTiles.ToList(), dlg.FileName);
                } else if ((MpExportType)exportType == MpExportType.Zip) {
                    ExportClipsToZipFile(SelectedClipTiles.ToList(), dlg.FileName);
                } else {
                    ExportClipsToFile(SelectedClipTiles.ToList(), dlg.FileName + @"\");
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
            return true;
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
                        speechSynthesizer.Speak(sctvm.PlainText);
                    }
                }
            }));            
        }

        #endregion
    }
    public enum MpExportType {
        None = 0,
        Files,
        Csv,
        Zip
    }
}
