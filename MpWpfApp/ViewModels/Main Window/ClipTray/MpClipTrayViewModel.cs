using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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
        #region Private Variables      
        private ListBox _clipTrayRef = null;
        //private object _dragClipBorderElement = null;

        Stopwatch sw = new Stopwatch();

        private List<MpCopyItem> _testList = new List<MpCopyItem>();
        #endregion

        #region Properties
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

        public MpClipboardManager ClipboardManager { get; private set; }

        public bool IsDragging { get; set; } = false;

        public bool DoPaste { get; set; } = false;

        public Point StartDragPoint;

        private bool _isFilteringByApp = false;
        public bool IsFilteringByApp {
            get {
                return _isFilteringByApp;
            }
            set {
                if (_isFilteringByApp != value) {
                    _isFilteringByApp = value;
                    OnPropertyChanged(nameof(IsFilteringByApp));
                }
            }
        }

        private bool _isTrayFocused = false;
        public bool IsClipTrayFocused {
            get {
                return _isTrayFocused;
            }
            set {
                if (_isTrayFocused != value) {
                    _isTrayFocused = value;
                    OnPropertyChanged(nameof(IsClipTrayFocused));
                }
            }
        }

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

        public bool IsEditingClipTile {
            get {
                foreach (var sctvm in SelectedClipTiles) {
                    if (sctvm.IsEditingTile) {
                        return true;
                    }
                }
                return false;
            }
        }

        public Visibility EmptyListMessageVisibility {
            get {
                if(VisibileClipTiles.Count == 0) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ClipTrayVisibility {
            get {
                if (VisibileClipTiles.Count == 0) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
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
                    if (sctvm.HasTemplate) {
                        outStr += MpHelpers.ConvertRichTextToPlainText(sctvm.TemplateRichText) + Environment.NewLine;
                    } else {
                        outStr += sctvm.CopyItemPlainText + Environment.NewLine;
                    }
                }
                return outStr.Trim('\r','\n');
            }
        }

        //public string SelectedClipTilesRichText {
        //    get {
        //        string outStr = MpHelpers.ConvertPlainTextToRichText(string.Empty);
        //        foreach (var sctvm in SelectedClipTiles) {
        //            outStr = MpHelpers.CombineRichText2(outStr, sctvm.CopyItemRichText);                    
        //        }
        //        return outStr;
        //    }
        //}
        
        public BitmapSource SelectedClipTilesBmp {
            get {
                var bmpList = new List<BitmapSource>();
                foreach (var sctvm in SelectedClipTiles) {
                    bmpList.Add(sctvm.CopyItemBmp);
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
                    foreach (string f in sctvm.CopyItemFileDropList) {
                        fl.Add(f);
                    }
                }
                return fl.ToArray();
            }
        }

        //public IDataObject SelectedClipTilesDropDataObject {
        //    get {
        //        IDataObject d = new DataObject();
        //        d.SetData(DataFormats.Rtf, SelectedClipTilesRichText);
        //        d.SetData(DataFormats.Text, SelectedClipTilesPlainText);
        //        d.SetData(DataFormats.FileDrop, SelectedClipTilesFileList);
        //        d.SetData(DataFormats.Bitmap, SelectedClipTilesBmp);
        //        d.SetData(DataFormats.CommaSeparatedValue, SelectedClipTilesCsv);
        //        d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedClipTiles.ToList());
        //        return d;
        //    }
        //}

        //public IDataObject SelectedClipTilesPasteDataObject {
        //    get {
        //        IDataObject d = new DataObject();
        //        //only when pasting into explorer must have file drop
        //        if(string.IsNullOrEmpty(ClipboardManager.LastWindowWatcher.LastTitle.Trim())) {
        //            d.SetData(DataFormats.FileDrop, SelectedClipTilesFileList);
        //        }                
        //        d.SetData(DataFormats.Bitmap, SelectedClipTilesBmp);
        //        d.SetData(DataFormats.CommaSeparatedValue, SelectedClipTilesCsv);
        //        d.SetData(DataFormats.Rtf, SelectedClipTilesRichText);
        //        d.SetData(DataFormats.Text, SelectedClipTilesPlainText);
        //        d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedClipTiles.ToList());
        //        return d;
        //    }
        //}

        #endregion

        #region Events
        public event EventHandler ItemsVisibilityChanged;
        public virtual void OnItemsVisibilityChanged() => ItemsVisibilityChanged?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Public Methods

        public MpClipTrayViewModel() : base() {
            //BindingOperations.DisableCollectionSynchronization(this);
            CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(EmptyListMessageVisibility));
                OnPropertyChanged(nameof(ClipTrayVisibility));
            };            

            //create tiles for all clips in the database
            foreach (MpCopyItem ci in MpCopyItem.GetAllCopyItems()) {
                this.Add(new MpClipTileViewModel(ci));
            }
        }
                

        public void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            //for (int i = 0; i < 1000; i++) {
            //    _testList.Add(MpCopyItem.CreateRandomItem(MpCopyItemType.RichText));
            //}
            var clipTray = (MpMultiSelectListView)sender;
            _clipTrayRef = clipTray;

            clipTray.DragEnter += (s, e1) => {
                //used for resorting
                e1.Effects = e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName) ? DragDropEffects.Move : DragDropEffects.None;
            };
            clipTray.Drop += (s, e2) => {
                //retrieve custom dataformat object (cliptileviewmodel)
                var dragClipViewModel = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                
                //using current mp if drag is to the right (else part) adjust point to locate next tile, otherwise adjust to point to previous tile
                var mpo = e2.GetPosition(clipTray);
                if (mpo.X - StartDragPoint.X > 0) {
                    mpo.X -= MpMeasurements.Instance.ClipTileMargin * 5;
                } else {
                    mpo.X += MpMeasurements.Instance.ClipTileMargin * 5;
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
                if(dragClipViewModel == null || dragClipViewModel.Contains(dropVm)) {
                    e2.Effects = DragDropEffects.None;
                    e2.Handled = true;
                    return;
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
                            dragClipViewModel[i].IsClipItemFocused = true;
                        }
                    }
                } else {
                    Console.WriteLine("MainWindow drop error cannot find lasrt moused over tile");
                }
            };
            clipTray.SelectionChanged += (s, e8) => {
                MergeClipsCommandVisibility = MergeSelectedClipsCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;
                foreach (var ttvm in MainWindowViewModel.TagTrayViewModel) {
                    if (ttvm == MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel() || ttvm.IsSelected) {
                        continue;
                    }

                    bool isTagLinkedToAllSelectedClips = true;
                    foreach (var sctvm in SelectedClipTiles) {                        
                        if (!ttvm.IsLinkedWithClipTile(sctvm)) {
                            isTagLinkedToAllSelectedClips = false;
                        }
                    }
                    ttvm.IsHovering = isTagLinkedToAllSelectedClips && VisibileClipTiles.Count > 0;

                }
            };
            clipTray.PreviewMouseWheel += (s, e3) => {
                if(IsEditingClipTile) {
                    return;
                }
                e3.Handled = true;

                var clipTrayListBox = (ListBox)sender;
                var scrollViewer = clipTrayListBox.GetDescendantOfType<ScrollViewer>();
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (e3.Delta * -1) / 5);
            };
            clipTray.PreviewMouseLeftButtonUp += (s, e4) => {
                var p = e4.MouseDevice.GetPosition(clipTray);
                var hitTestResult = VisualTreeHelper.HitTest(clipTray, p);
                if (hitTestResult == null || hitTestResult.VisualHit.GetVisualAncestor<ListBoxItem>() == null) {
                    MainWindowViewModel.ClearEdits();
                    e4.Handled = true;
                }
            };

            ClipboardManager = new MpClipboardManager((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));

            // Attach the handler to the event raising on WM_DRAWCLIPBOARD message is received
            ClipboardManager.ClipboardChanged += (s, e53) => {
                var sw = new Stopwatch();
                sw.Start();
                var newCopyItem = MpCopyItem.CreateFromClipboard(MainWindowViewModel.ClipTrayViewModel.ClipboardManager.LastWindowWatcher.LastHandle);
                if (MainWindowViewModel.AppModeViewModel.IsInAppendMode) {
                    //when in append mode just append the new items text to selecteditem
                    SelectedClipTiles[0].AppendContent(new MpClipTileViewModel(newCopyItem));
                    return;
                }
                if (newCopyItem.CopyItemId > 0) {
                    //item is a duplicate
                    var existingClipTile = this.Where(x => x.CopyItemId == newCopyItem.CopyItemId).ToList();
                    if (existingClipTile != null && existingClipTile.Count > 0) {
                        Console.WriteLine("Ignoring duplicate copy item");
                        existingClipTile[0].CopyCount++;
                        existingClipTile[0].CopyDateTime = DateTime.Now;
                        this.Move(this.IndexOf(existingClipTile[0]), 0);
                    }
                } else {
                    var nctvm = new MpClipTileViewModel(newCopyItem);
                    this.Add(nctvm);
                    MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(nctvm);
                }
                sw.Stop();
                Console.WriteLine("Time to create new copyitem: " + sw.ElapsedMilliseconds + " ms");
                ResetClipSelection();
            };

            if (Properties.Settings.Default.IsInitialLoad) {
                var introItem1 = new MpCopyItem(
                    MpCopyItemType.RichText,
                    "Welcome to MonkeyPaste!",
                    MpHelpers.ConvertPlainTextToRichText("Take a moment to look through the available features in the following tiles, which are always available in the 'Help' pinboard"));

                var introItem2 = new MpCopyItem(
                    MpCopyItemType.RichText,
                    "One place for your clipboard",
                    MpHelpers.ConvertPlainTextToRichText(""));
                Properties.Settings.Default.IsInitialLoad = false;
                Properties.Settings.Default.Save();
            }

            SortAndFilterClipTiles();            
        }

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTileBorder = (Grid)sender;
            var ctvm = (MpClipTileViewModel)clipTileBorder.DataContext;

            clipTileBorder.PreviewMouseLeftButtonDown += (s, e6) => {
                var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");
                IsMouseDown = true;
                StartDragPoint = e6.GetPosition(clipTray);

                //_dragClipBorderElement = (MpClipBorder)VisualTreeHelper.HitTest(clipTray, StartDragPoint).VisualHit.GetVisualAncestor<MpClipBorder>(); ;
            };
            //Initiate Selected Clips Drag/Drop, Copy/Paste and Export (to file or csv)
            //Strategy: ALL selected items, regardless of type will have text,rtf,img, and file representations
            //          that are appended as text and filelists but  merged into images (by default)
            // TODO Have option to append items to one long image
            clipTileBorder.PreviewMouseMove += (s, e7) => {
                var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");
                var curDragPoint = e7.GetPosition(clipTray);
                //these tests ensure tile is not being dragged INTO another clip tile or outside tray
                //var testBorder = (MpClipBorder)VisualTreeHelper.HitTest(clipTray, curDragPoint).VisualHit.GetVisualAncestor<MpClipBorder>();
                //var testTray = (ListBox)VisualTreeHelper.HitTest(clipTray, curDragPoint).VisualHit.GetVisualAncestor<ListBox>();
                if (IsMouseDown && 
                    !IsDragging && 
                    !IsEditingClipTile &&
                    !IsEditingClipTitle &&
                    e7.MouseDevice.LeftButton == MouseButtonState.Pressed && 
                    (Math.Abs(curDragPoint.Y - StartDragPoint.Y) > 5 || Math.Abs(curDragPoint.X - StartDragPoint.X) > 5) /*&&
                   // s.GetType() == typeof(MpClipBorder) &&
                    //_dragClipBorderElement != testBorder &&
                    testBorder == null &&
                    testTray != null*/) {
                    DragDrop.DoDragDrop(clipTray, GetDataObjectFromSelectedClips(true), DragDropEffects.Copy | DragDropEffects.Move);
                    IsDragging = true;
                } else if(IsDragging) {
                    IsMouseDown = false;
                    IsDragging = false;
                    StartDragPoint = new Point ();
                    //_dragClipBorderElement = null;
                }
            };
            clipTileBorder.PreviewMouseUp += (s, e8) => {
                IsMouseDown = false;
                IsDragging = false;
                StartDragPoint = new Point();
                //_dragClipBorderElement = null;
            };
            clipTileBorder.IsVisibleChanged += (s, e9) => {
                //_clipTrayRef.Items.Refresh();
                OnPropertyChanged(nameof(EmptyListMessageVisibility));
                OnPropertyChanged(nameof(ClipTrayVisibility));
                OnItemsVisibilityChanged();
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
                clip.IsEditingTile = false;
                clip.IsEditingTitle = false;
                clip.IsPastingTemplateTile = false;
                clip.IsEditingTemplate = false;
                clip.IsSelected = false;
            }
        }

        public void ResetClipSelection() {
            ClearClipSelection();
            if (VisibileClipTiles.Count > 0) {
                VisibileClipTiles[0].IsSelected = true;
                if(!MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                    VisibileClipTiles[0].IsClipItemFocused = true;
                }
            }
        }

        public new void Add(MpClipTileViewModel ctvm) {
            this.Insert(0, ctvm);
            _clipTrayRef?.Items.Refresh();
        }

        public new void Remove(MpClipTileViewModel clipTileToRemove) {            
            base.Remove(clipTileToRemove);
            if(clipTileToRemove.CopyItem == null) {
                //occurs when duplicate detected on background thread
                return;
            }
            foreach (var ttvm in MainWindowViewModel.TagTrayViewModel) {
                if (ttvm.IsLinkedWithClipTile(clipTileToRemove)) {
                    ttvm.TagClipCount--;
                }
            }
            clipTileToRemove.CopyItem.DeleteFromDatabase();

            //remove any shortcuts associated with clip
            var scvmToRemoveList = new List<MpShortcutViewModel>();
            foreach(var scvmToRemove in MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == clipTileToRemove.CopyItem.CopyItemId).ToList()) {
                scvmToRemoveList.Add(scvmToRemove);
            }
            foreach(var scvmToRemove in scvmToRemoveList) {
                MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
            }
            clipTileToRemove = null;
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
            var tempSearchText = MainWindowViewModel.SearchBoxViewModel.Text;
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

        public async Task<IDataObject> GetDataObjectFromSelectedClips(bool isDragDrop = false) {
            IDataObject d = new DataObject();
            //only when pasting into explorer must have file drop
            if (string.IsNullOrEmpty(ClipboardManager.LastWindowWatcher.LastTitle.Trim()) && isDragDrop) {
                d.SetData(DataFormats.FileDrop, SelectedClipTilesFileList);
            } 
            d.SetData(DataFormats.Bitmap, SelectedClipTilesBmp);
            d.SetData(DataFormats.CommaSeparatedValue, SelectedClipTilesCsv);

            string rtf =string.Empty;
            foreach (var sctvm in SelectedClipTiles) {
                var task = sctvm.GetPastableRichText();
                string rt = await task;
                if(string.IsNullOrEmpty(rtf)) {
                    rtf = rt;
                } else {
                    rtf = MpHelpers.CombineRichText(rtf, rt);
                }
            }
            d.SetData(DataFormats.Rtf, rtf);
            d.SetData(DataFormats.Text, MpHelpers.ConvertRichTextToPlainText(rtf));
            d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedClipTiles.ToList());
            return d;

            //awaited in MainWindowViewModel.HideWindow
        }

        public MpCopyItemType GetTargetFileType() {
            string targetTitle = ClipboardManager?.LastWindowWatcher.LastTitle.ToLower();

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
                    foreach (var p in ctvm.CopyItemFileDropList) {
                        zip.CreateEntryFromFile(p, Path.GetFileName(p));
                    }
                }
            }
            return filePath;
        }

        public ListBox GetTray() {
            return _clipTrayRef;
        }
        #endregion

        #region Private Methods

    private int GetClipTileFromDrag(Point startLoc,Point curLoc) {
            return 0;
        }

        //private MpClipTileViewModel FindClipTileByModel(MpCopyItem ci) {
        //    foreach(var ctvm in this) {
        //        if(ctvm.CopyItemType != ci.CopyItemType) {
        //            continue;
        //        }
        //        if(ctvm.CopyItem.GetData() == ci.GetData()) {
        //            return ctvm;
        //        }
        //        //switch(ci.CopyItemType) {
        //        //    case MpCopyItemType.RichText:
        //        //        if (string.Compare((string)ctvm.CopyItem.ItemXaml, ci.ItemXaml) == 0) {
                            
        //        //            return ctvm;
        //        //        }
        //        //        break;
        //        //    case MpCopyItemType.FileList:
        //        //        if (string.Compare((string)ctvm.CopyItem.ItemPlainText, ci.ItemPlainText) == 0) {
        //        //            return ctvm;
        //        //        }
        //        //        break;
        //        //    case MpCopyItemType.Image:
        //        //        if(MpHelpers.ByteArrayCompare(MpHelpers.ConvertBitmapSourceToByteArray(ctvm.CopyItem.ItemBitmapSource), MpHelpers.ConvertBitmapSourceToByteArray(ci.ItemBitmapSource))) {
        //        //            return ctvm;
        //        //        }
        //        //        break;
        //        //}                
        //    }
        //    return null;
        //}

        #endregion

        #region Commands
        private RelayCommand _selectAllCommand;
        public ICommand SelectAllCommand {
            get {
                if (_selectAllCommand == null) {
                    _selectAllCommand = new RelayCommand(SelectAll);
                }
                return _selectAllCommand;
            }
        }
        private void SelectAll() {
            foreach (var ctvm in VisibileClipTiles) {
                ctvm.IsSelected = true;
            }
        }

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
                sctvm.ConvertContent(ct);
            }
        }

        private RelayCommand<Brush> _changeSelectedClipsColorCommand;
        public ICommand ChangeSelectedClipsColorCommand {
            get {
                if (_changeSelectedClipsColorCommand == null) {
                    _changeSelectedClipsColorCommand = new RelayCommand<Brush>(ChangeSelectedClipsColor);
                }
                return _changeSelectedClipsColorCommand;
            }
        }
        private void ChangeSelectedClipsColor(Brush brush) {
            var result = brush != null ? brush : MpHelpers.ShowColorDialog(SelectedClipTiles[0].TitleColor);
            if(result != null) {
                BitmapSource sharedSwirl = null;
                foreach (var sctvm in SelectedClipTiles) {
                    sctvm.TitleColor = result;
                    if (sharedSwirl == null) {
                        sctvm.TitleSwirl = sctvm.CopyItem.InitSwirl();
                        sharedSwirl = sctvm.TitleSwirl;
                    } else {
                        sctvm.TitleSwirl = sctvm.CopyItem.InitSwirl(sharedSwirl);
                    }
                    sctvm.CopyItem.WriteToDatabase();
                }
            }
        }

        private RelayCommand _pasteSelectedClipsCommand;
        public ICommand PasteSelectedClipsCommand {
            get {
                if (_pasteSelectedClipsCommand == null) {
                    _pasteSelectedClipsCommand = new RelayCommand(PasteSelectedClips, CanPasteSelectedClips);
                }
                return _pasteSelectedClipsCommand;
            }
        }
        private bool CanPasteSelectedClips() {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false;
        }
        private void PasteSelectedClips() {
            //In order to paste the app must hide first 
            //this triggers hidewindow to paste selected items

            MainWindowViewModel.HideWindowCommand.Execute(true);                        
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
            //SelectedClipTiles[0].IsTitleTextBoxFocused = true;
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
            bool isLastClipTileLinked = tagToLink.IsLinkedWithClipTile(SelectedClipTiles[0]);
            foreach (var selectedClipTile in SelectedClipTiles) {
                if (tagToLink.IsLinkedWithClipTile(selectedClipTile) != isLastClipTileLinked) {
                    return false;
                }
            }
            return true;
        }
        private void LinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            bool isUnlink = tagToLink.IsLinkedWithClipTile(SelectedClipTiles[0]);
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
            //for some reason not making this run on another thread
            //MAY throw a com error or it was from AddToken
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
                    focusedClip.IsClipItemFocused = true;
                    //this breaks mvvm but no way to refresh tokens w/o
                    _clipTrayRef.Items.Refresh();
                })
            );   
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
            using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer()) {
                foreach (var sctvm in SelectedClipTiles) {
                    speechSynthesizer.SpeakAsync(sctvm.CopyItemPlainText);
                }
            }
        }

        private RelayCommand _duplicateSelectedClipsCommand;
        public ICommand DuplicateSelectedClipsCommand {
            get {
                if (_duplicateSelectedClipsCommand == null) {
                    _duplicateSelectedClipsCommand = new RelayCommand(DuplicateSelectedClips);
                }
                return _duplicateSelectedClipsCommand;
            }
        }
        private void DuplicateSelectedClips() {
            var tempSelectedClipTiles = SelectedClipTiles;
            ClearClipSelection();
            foreach(var sctvm in tempSelectedClipTiles) {
                var clonedCopyItem = (MpCopyItem)sctvm.CopyItem.Clone();
                clonedCopyItem.WriteToDatabase();
                var ctvm = new MpClipTileViewModel(clonedCopyItem);
                MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
                this.Add(ctvm);
                ctvm.IsSelected = true;
            }
        }

        private RelayCommand _runSelectedClipsInShellCommand;
        public ICommand RunSelectedClipsInShellCommand {
            get {
                if (_runSelectedClipsInShellCommand == null) {
                    _runSelectedClipsInShellCommand = new RelayCommand(RunSelectedClipsInShell, CanRunSelectedClipsInShell);
                }
                return _runSelectedClipsInShellCommand;
            }
        }
        private bool CanRunSelectedClipsInShell() {
            foreach(var sctvm in SelectedClipTiles) {
                if(!sctvm.RunClipInShellCommand.CanExecute(null)) {
                    return false;
                }
            }
            return true;
        }
        private void RunSelectedClipsInShell() {
            foreach (var sctvm in SelectedClipTiles) {
                sctvm.RunClipInShellCommand.Execute(null);
            }
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
