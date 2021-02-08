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
using AlphaChiTech.Virtualization;
using AsyncAwaitBestPractices.MVVM;
using DataGridAsyncDemoMVVM.filtersort;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MpWpfApp {
    public class MpClipTrayViewModel : MpViewModelBase {
        #region Private Variables      
        private MpMultiSelectListView _clipTrayRef = null;
        //private object _dragClipBorderElement = null;

        Stopwatch sw = new Stopwatch();

        private List<MpCopyItem> _testList = new List<MpCopyItem>();

        private int _filterWaitingCount;

        private CancellationTokenSource _highlightCancellationTokenSource = null;
        #endregion

        #region Properties
        private int _highlightTaskCount = 0;
        public int HighlightTaskCount {
            get {
                return _highlightTaskCount;
            }
            set {
                if(_highlightTaskCount != value) {
                    _highlightTaskCount = value;
                    OnPropertyChanged(nameof(HighlightTaskCount));
                }
            }
        }

        private MpClipTileViewModelDataSource _clipTileViewModelDataSource = null;
        public MpClipTileViewModelDataSource ClipTileViewModelDataSource {
            get {
                return _clipTileViewModelDataSource;
            }
            set {
                if (_clipTileViewModelDataSource != value) {
                    _clipTileViewModelDataSource = value;
                    OnPropertyChanged(nameof(ClipTileViewModelDataSource));
                }
            }
        }

        private PaginationManager<MpClipTileViewModel> _clipTileViewModelPaginationManager = null;
        public PaginationManager<MpClipTileViewModel> ClipTileViewModelPaginationManager { 
            get {
                if(_clipTileViewModelPaginationManager == null) {
                    ClipTileViewModelDataSource = new MpClipTileViewModelDataSource(1);
                    _clipTileViewModelPaginationManager = new PaginationManager<MpClipTileViewModel>(
                                new MpClipTileViewModelPagedSourceProviderAsync(ClipTileViewModelDataSource), pageSize: 8, maxPages: 2);
                }
                return _clipTileViewModelPaginationManager;
            }  
        }

        private VirtualizingObservableCollection<MpClipTileViewModel> _clipTileViewModels = null;

        private ICollectionView _clipTileCollectionView = null;
        public ICollectionView ClipTileViewModels {
            get {
                if (_clipTileViewModels == null) {                    
                    _clipTileViewModels = new VirtualizingObservableCollection<MpClipTileViewModel>(ClipTileViewModelPaginationManager);
                }
                if(_clipTileCollectionView == null) {
                    _clipTileCollectionView = CollectionViewSource.GetDefaultView(_clipTileViewModels);
                }
                return _clipTileCollectionView;
            }
        }

        public List<MpClipTileViewModel> SelectedClipTiles {
            get {
                return _clipTileViewModels.Where(ct => ct.IsSelected).ToList();
            }
        }

        public List<MpClipTileViewModel> VisibileClipTiles {
            get {
                return _clipTileViewModels.Where(ct => ct.TileVisibility == Visibility.Visible).ToList();
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

        private bool _isScrolling = false;
        public bool IsScrolling {
            get {
                return _isScrolling;
            }
            set {
                if (_isScrolling != value) {
                    _isScrolling = value;
                    OnPropertyChanged(nameof(IsScrolling));
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

        public bool IsPastingTemplate { 
            get {
                foreach (var sctvm in SelectedClipTiles) {
                    if (sctvm.IsPastingTemplateTile) { 
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
                        outStr += MpHelpers.Instance.ConvertRichTextToPlainText(sctvm.TemplateRichText) + Environment.NewLine;
                    } else {
                        outStr += sctvm.CopyItemPlainText + Environment.NewLine;
                    }
                }
                return outStr.Trim('\r','\n');
            }
        }

        //public string SelectedClipTilesRichText {
        //    get {
        //        string outStr = MpHelpers.Instance.ConvertPlainTextToRichText(string.Empty);
        //        foreach (var sctvm in SelectedClipTiles) {
        //            outStr = MpHelpers.Instance.CombineRichText2(outStr, sctvm.CopyItemRichText);                    
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
                return MpHelpers.Instance.CombineBitmap(bmpList, false);
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
            ClipTileViewModels.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(EmptyListMessageVisibility));
                OnPropertyChanged(nameof(ClipTrayVisibility));
            };
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(HighlightTaskCount):
                        MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.IsSearching = HighlightTaskCount > 0;
                        if (HighlightTaskCount < 0) {
                            HighlightTaskCount = 0;
                        }
                        break;
                }
            };
            FilterCommand = new RelayCommand<MemberPathFilterText>(async o => await Filter(o));
            SortCommand = new RelayCommand<MemberPathSortingDirection>(async o => await Sort(o));
        }

        public void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.PropertyChanged += (s, e8) => {
                switch (e8.PropertyName) {
                    case nameof(MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.SearchText):
                        var hlt = MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.SearchText;
                        if (_highlightCancellationTokenSource == null) {
                            _highlightCancellationTokenSource = new CancellationTokenSource();
                        } else {
                            //_highlightCancellationTokenSource.Cancel();
                        }
                        foreach(MpClipTileViewModel ctvm in ClipTileViewModels) {
                            ctvm.PerformHighlight(ctvm, hlt,_highlightCancellationTokenSource.Token);
                        }
                        
                        break;
                }
            };

            var clipTray = (MpMultiSelectListView)sender;
            var scrollViewer = clipTray.GetDescendantOfType<ScrollViewer>();

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
                int dropIdx = item == null || item == clipTray ? 0 : _clipTileViewModels.IndexOf(dropVm);
                if (dropIdx >= 0) {
                    ClearClipSelection();
                    for (int i = 0; i < dragClipViewModel.Count; i++) {
                        int dragIdx = _clipTileViewModels.IndexOf(dragClipViewModel[i]);
                        _clipTileViewModels.Move(dragIdx, dropIdx);
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

                MainWindowViewModel.TagTrayViewModel.UpdateTagAssociation();
                
                if(SelectedClipTiles.Count > 1) {
                    //order selected tiles by ascending datetime 
                    var selectedTileList = SelectedClipTiles.OrderBy(x => x.LastSelectedDateTime).ToList();
                    foreach(var sctvm in selectedTileList) {
                        if(sctvm == selectedTileList[0]) {
                            sctvm.IsPrimarySelected = true;
                        } else {
                            sctvm.IsPrimarySelected = false;
                        }
                    }
                } else if(SelectedClipTiles.Count == 1) {
                    SelectedClipTiles[0].IsPrimarySelected = false;
                }
                foreach(var osctvm in e8.RemovedItems) {
                    if(osctvm.GetType() == typeof(MpClipTileViewModel)) {
                        ((MpClipTileViewModel)osctvm).IsPrimarySelected = false;
                    }                    
                }
            };

            clipTray.MouseLeftButtonUp += (s, e4) => {
                var p = e4.MouseDevice.GetPosition(clipTray);
                var hitTestResult = VisualTreeHelper.HitTest(clipTray, p);
                if (!IsPastingTemplate && (hitTestResult == null || hitTestResult.VisualHit.GetVisualAncestor<ListBoxItem>() == null)) {
                    MainWindowViewModel.ClearEdits();
                    //e4.Handled = true;
                }
            };

            ClipboardManager = new MpClipboardManager((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));

            // Attach the handler to the event raising on WM_DRAWCLIPBOARD message is received
            //ClipboardManager.ClipboardChanged += async (s, e53) => {
            //    var sw = new Stopwatch();
            //    sw.Start();

            //    await Dispatcher.CurrentDispatcher.InvokeAsync(async () => {
            //        VirtualizationManager.Instance.RunOnUI(
            //        () => {
            //            var nctvm = new MpClipTileViewModel();
            //            if (_clipTileViewModels.Count == 0) {
            //                ClipTileViewModelDataSource.InsertAt(0, nctvm);
            //            }
            //            this.Add(nctvm);
            //        });
            //        var newCopyItem = await MpCopyItem.CreateFromClipboardAsync(MainWindowViewModel.ClipTrayViewModel.ClipboardManager.LastWindowWatcher.LastHandle);

            //        VirtualizationManager.Instance.RunOnUI(
            //        () => {
            //            if (newCopyItem == null) {
            //                //this occurs if the copy item is not a known format
            //                ClipTileViewModelDataSource.RemoveAt(0);
            //                return;
            //            }
            //            if (MainWindowViewModel.AppModeViewModel.IsInAppendMode && SelectedClipTiles.Count > 0) {
            //                //when in append mode just append the new items text to selecteditem
            //                ClipTileViewModelDataSource.RemoveAt(0);
            //                SelectedClipTiles[0].AppendContent(new MpClipTileViewModel(newCopyItem));
            //                return;
            //            }
            //            if (newCopyItem.CopyItemId > 0) {
            //                //item is a duplicate
            //                ClipTileViewModelDataSource.RemoveAt(0);
            //                var existingClipTile = _clipTileViewModels.Where(x => x.CopyItemId == newCopyItem.CopyItemId).ToList();
            //                if (existingClipTile != null && existingClipTile.Count > 0) {
            //                    Console.WriteLine("Ignoring duplicate copy item");
            //                    existingClipTile[0].CopyCount++;
            //                    existingClipTile[0].CopyDateTime = DateTime.Now;
            //                    _clipTileViewModels.Move(_clipTileViewModels.IndexOf(existingClipTile[0]), 0);
            //                    ClearClipSelection();
            //                    existingClipTile[0].IsSelected = true;
            //                }
            //            } else {
            //                var nctvm = ClipTileViewModelDataSource.FilteredOrderedItems[0];
            //                nctvm.SetCopyItem(newCopyItem);
            //                MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(nctvm);
            //            }
            //        });
            //    }, DispatcherPriority.Background);

            //    sw.Stop();
            //    Console.WriteLine("Time to create new copyitem: " + sw.ElapsedMilliseconds + " ms");
            //    ResetClipSelection();
            //};
            ClipboardManager.ClipboardChanged += (s, e53) => {
                var sw = new Stopwatch();
                sw.Start();

                var nctvm = new MpClipTileViewModel();
                if (_clipTileViewModels.Count == 0) {
                    ClipTileViewModelDataSource.InsertAt(0, nctvm);
                }
                this.Add(nctvm);
                var newCopyItem = MpCopyItem.CreateFromClipboardAsync(MainWindowViewModel.ClipTrayViewModel.ClipboardManager.LastWindowWatcher.LastHandle).Result;
                if (newCopyItem == null) {
                    //this occurs if the copy item is not a known format
                    ClipTileViewModelDataSource.RemoveAt(0);
                    return;
                }
                if (MainWindowViewModel.AppModeViewModel.IsInAppendMode && SelectedClipTiles.Count > 0) {
                    //when in append mode just append the new items text to selecteditem
                    ClipTileViewModelDataSource.RemoveAt(0);
                    SelectedClipTiles[0].AppendContent(new MpClipTileViewModel(newCopyItem));
                    return;
                }
                if (newCopyItem.CopyItemId > 0) {
                    //item is a duplicate
                    ClipTileViewModelDataSource.RemoveAt(0);
                    var existingClipTile = _clipTileViewModels.Where(x => x.CopyItemId == newCopyItem.CopyItemId).ToList();
                    if (existingClipTile != null && existingClipTile.Count > 0) {
                        Console.WriteLine("Ignoring duplicate copy item");
                        existingClipTile[0].CopyCount++;
                        existingClipTile[0].CopyDateTime = DateTime.Now;
                        _clipTileViewModels.Move(_clipTileViewModels.IndexOf(existingClipTile[0]), 0);
                        ClearClipSelection();
                        existingClipTile[0].IsSelected = true;
                    }
                } else {
                    //var nctvm = ClipTileViewModelDataSource.FilteredOrderedItems[0];
                    nctvm.SetCopyItem(newCopyItem);
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
                    MpHelpers.Instance.ConvertPlainTextToRichText("Take a moment to look through the available features in the following tiles, which are always available in the 'Help' pinboard"));

                var introItem2 = new MpCopyItem(
                    MpCopyItemType.RichText,
                    "One place for your clipboard",
                    MpHelpers.Instance.ConvertPlainTextToRichText(""));
                Properties.Settings.Default.IsInitialLoad = false;
                Properties.Settings.Default.Save();
            }        
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
                    !IsPastingTemplate &&                    
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
            clipTileBorder.PreviewMouseLeftButtonUp += (s, e8) => {
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
            foreach (MpClipTileViewModel clip in ClipTileViewModels) {                
                clip.IsEditingTile = false;
                clip.IsEditingTitle = false;
                clip.IsPastingTemplateTile = false;
                clip.IsEditingTemplate = false;
                clip.IsSelected = false;
                clip.IsPrimarySelected = false;
            }
        }

        public void ResetClipSelection() {
            ClearClipSelection();

            if (VisibileClipTiles.Count > 0) {
                VisibileClipTiles[0].IsSelected = true;
                if(!MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                    VisibileClipTiles[0].IsClipItemFocused = true;
                    if(GetClipTray() != null) {
                        ((ListViewItem)GetClipTray().ItemContainerGenerator.ContainerFromItem(VisibileClipTiles[0]))?.Focus();
                    }
                }
                if(GetClipTray() != null) {
                    GetClipTray().ScrollViewer.ScrollToHorizontalOffset(0);
                }
            }
        }

        public void RefreshAllCommands() {
            foreach(MpClipTileViewModel ctvm in ClipTileViewModels) {
                ctvm.RefreshCommands();
            }
        }
        public void Add(MpClipTileViewModel ctvm) {
            _clipTileViewModels.Insert(0, ctvm);
            _clipTrayRef?.Items.Refresh();
        }

        public void Remove(MpClipTileViewModel clipTileToRemove) {
            _clipTileViewModels.Remove(clipTileToRemove);
            if (clipTileToRemove.CopyItem == null) {
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
            foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == clipTileToRemove.CopyItem.CopyItemId).ToList()) {
                scvmToRemoveList.Add(scvmToRemove);
            }
            foreach (var scvmToRemove in scvmToRemoveList) {
                MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
            }
            clipTileToRemove = null;
        }

        //public void SortClipTiles() {
        //    var sw = new Stopwatch();
        //    sw.Start();
            
        //    ClearClipSelection();
        //    //var cvs = CollectionViewSource.GetDefaultView(VisibileClipTiles);
        //    //var tempSearchText = MainWindowViewModel.SearchBoxViewModel.Text;
        //    //if (doFilter) {
        //    //    cvs.Filter += item => {
        //    //        var ctvm = (MpClipTileViewModel)item;

        //    //        if (tempSearchText.Trim() == string.Empty || tempSearchText == Properties.Settings.Default.SearchPlaceHolderText) {
        //    //            return true;
        //    //        }

        //    //        if (ctvm.CopyItemType == MpCopyItemType.Image) {
        //    //            return false;
        //    //        }

        //    //        if (Properties.Settings.Default.IsSearchCaseSensitive) {
        //    //            return ctvm.CopyItem.ItemPlainText.Contains(tempSearchText);
        //    //        }
        //    //        return ctvm.CopyItem.ItemPlainText.ToLower().Contains(tempSearchText.ToLower());
        //    //    };
        //    //}

        //    if (true) {
        //        ListSortDirection sortDir = MainWindowViewModel.ClipTileSortViewModel.AscSortOrderButtonImageVisibility == Visibility.Visible ? ListSortDirection.Ascending : ListSortDirection.Descending;
        //        string sortBy = string.Empty;
        //        switch (MainWindowViewModel.ClipTileSortViewModel.SelectedSortType.Header) {
        //            case "Date":
        //                sortBy = "CopyItemCreatedDateTime";
        //                break;
        //            case "Application":
        //                sortBy = "CopyItemAppId";
        //                break;
        //            case "Title":
        //                sortBy = "CopyItemTitle";
        //                break;
        //            case "Content":
        //                sortBy = "CopyItemPlainText";
        //                break;
        //            case "Type":
        //                sortBy = "CopyItemType";
        //                break;
        //            case "Usage":
        //                sortBy = "CopyItemUsageScore";
        //                break;
        //        }
        //        //cvs.SortDescriptions.Clear();
        //        //cvs.SortDescriptions.Add(new SortDescription(sortBy, sortDir));
        //        _clipTileViewModels.Sort(x => x[sortBy], sortDir == ListSortDirection.Descending);
        //        GetClipTray().Items.Refresh();
        //    }
        //    sw.Stop();
        //    Console.WriteLine("Sort for " + VisibileClipTiles.Count + " items: " + sw.ElapsedMilliseconds + " ms");
        //    ResetClipSelection();
        //}

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
                    rtf = MpHelpers.Instance.CombineRichText(rtf, rt);
                }
            }
            d.SetData(DataFormats.Rtf, rtf);
            d.SetData(DataFormats.Text, MpHelpers.Instance.ConvertRichTextToPlainText(rtf));
            d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedClipTiles.ToList());
            return d;

            //awaited in MainWindowViewModel.HideWindow
        }

        public void PerformPaste(IDataObject pasteDataObject) {
            //called in the oncompleted of hide command in mwvm
            if (pasteDataObject != null) {
                Console.WriteLine("Pasting " + SelectedClipTiles.Count + " items");
                ClipboardManager.PasteDataObject(pasteDataObject);

                //resort list so pasted items are in front and paste is tracked
                for (int i = SelectedClipTiles.Count - 1; i >= 0; i--) {
                    var sctvm = SelectedClipTiles[i];
                    _clipTileViewModels.Move(_clipTileViewModels.IndexOf(sctvm), 0);
                    new MpPasteHistory(sctvm.CopyItem, ClipboardManager.LastWindowWatcher.LastHandle);
                }
            } else if (pasteDataObject == null) {
                Console.WriteLine("MainWindow Hide Command pasteDataObject was null, ignoring paste");
            }
            ResetClipSelection();
        }

        public List<MpClipTileViewModel> GetClipTilesByAppId(int appId) {
            var ctvml = new List<MpClipTileViewModel>();
            foreach(MpClipTileViewModel ctvm in _clipTileViewModels) {
                if(ctvm.CopyItemAppId == appId) {
                    ctvml.Add(ctvm);
                }
            }
            return ctvml;
        }

        public MpClipTileViewModel GetClipTileByCopyItemId(int copyItemId) {
            foreach (MpClipTileViewModel ctvm in _clipTileViewModels) {
                if (ctvm.CopyItemId == copyItemId) {
                    return ctvm;
                }
            }
            return null;
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

        public MpMultiSelectListView GetClipTray() {
            return _clipTrayRef;
        }
        #endregion

        #region Private Methods

        //private async void InitData() {
        //    ClipTileViewModelDataSource = await MpClipTileViewModelDataSource.GetDataSoure(1);
        //}

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
        //        //        if(MpHelpers.Instance.ByteArrayCompare(MpHelpers.Instance.ConvertBitmapSourceToByteArray(ctvm.CopyItem.ItemBitmapSource), MpHelpers.Instance.ConvertBitmapSourceToByteArray(ci.ItemBitmapSource))) {
        //        //            return ctvm;
        //        //        }
        //        //        break;
        //        //}                
        //    }
        //    return null;
        //}

        #endregion

        #region Commands
        public RelayCommand<MemberPathFilterText> FilterCommand { get; }
        private async Task Filter(MemberPathFilterText memberPathFilterText) {
            if (string.IsNullOrWhiteSpace(memberPathFilterText.FilterText)) {
                ClipTileViewModelDataSource.FilterDescriptionList.Remove(memberPathFilterText.MemberPath);
            }
            else {
                ClipTileViewModelDataSource.FilterDescriptionList.Add(
                    new FilterDescription(memberPathFilterText.MemberPath, memberPathFilterText.FilterText));
            }
            Interlocked.Increment(ref this._filterWaitingCount);
            await Task.Delay(500);
            if (Interlocked.Decrement(ref this._filterWaitingCount) != 0) {
                return;
            }
            ClipTileViewModelDataSource.FilterDescriptionList.OnCollectionReset();
            _clipTileViewModels.Clear();
            ResetClipSelection();
        }

        public RelayCommand<MemberPathSortingDirection> SortCommand { get; }
        private async Task Sort(MemberPathSortingDirection memberPathSortingDirection) {
            while (this._filterWaitingCount != 0) {
                await Task.Delay(500);
            }
            var sortDirection = memberPathSortingDirection.SortDirection;
            var sortMemberPath = memberPathSortingDirection.MemberPath;
            switch (sortDirection) {
                case null:
                    ClipTileViewModelDataSource.SortDescriptionList.Remove(sortMemberPath);
                    break;
                case ListSortDirection.Ascending:
                    ClipTileViewModelDataSource.SortDescriptionList.Add(
                        new DataGridAsyncDemoMVVM.filtersort.SortDescription(sortMemberPath, ListSortDirection.Ascending));
                    break;
                case ListSortDirection.Descending:
                    ClipTileViewModelDataSource.SortDescriptionList.Add(
                        new DataGridAsyncDemoMVVM.filtersort.SortDescription(sortMemberPath, ListSortDirection.Descending));
                    break;
            }

            ClipTileViewModelDataSource.FilterDescriptionList.OnCollectionReset();
            _clipTileViewModels.Clear();
            ResetClipSelection();
        }

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
            ClearClipSelection();
            foreach (var ctvm in VisibileClipTiles) {
                ctvm.IsSelected = true;
            }
        }

        private AsyncCommand<Brush> _changeSelectedClipsColorCommand;
        public IAsyncCommand<Brush> ChangeSelectedClipsColorCommand {
            get {
                if (_changeSelectedClipsColorCommand == null) {
                    _changeSelectedClipsColorCommand = new AsyncCommand<Brush>(ChangeSelectedClipsColor);
                }
                return _changeSelectedClipsColorCommand;
            }
        }
        private async Task ChangeSelectedClipsColor(Brush brush) {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            BitmapSource sharedSwirl = null;
                            foreach (var sctvm in SelectedClipTiles) {
                                sctvm.TitleColor = brush;
                                if (sharedSwirl == null) {
                                    sctvm.TitleSwirl = sctvm.CopyItem.InitSwirl(null,true);
                                    sharedSwirl = sctvm.TitleSwirl;
                                } else {
                                    sctvm.TitleSwirl = sctvm.CopyItem.InitSwirl(sharedSwirl);
                                }
                                //sctvm.CopyItem.WriteToDatabase();
                            }
                        }));
            } finally {
                IsBusy = false;
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
            return MpAssignShortcutModalWindowViewModel.IsOpen == false && 
                !IsEditingClipTile && 
                !IsEditingClipTitle && 
                !IsPastingTemplate;
        }
        private void PasteSelectedClips() {
            //In order to paste the app must hide first 
            //this triggers hidewindow to paste selected items

            MainWindowViewModel.HideWindowCommand.Execute(true);                        
        }

        private AsyncCommand _bringSelectedClipTilesToFrontCommand;
        public IAsyncCommand BringSelectedClipTilesToFrontCommand {
            get {
                if (_bringSelectedClipTilesToFrontCommand == null) {
                    _bringSelectedClipTilesToFrontCommand = new AsyncCommand(BringSelectedClipTilesToFront, CanBringSelectedClipTilesToFront);
                } 
                return _bringSelectedClipTilesToFrontCommand;
            }
        }
        private bool CanBringSelectedClipTilesToFront(object arg) {
            if(IsBusy || MainWindowViewModel.IsLoading || VisibileClipTiles.Count == 0) {
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
        private async Task BringSelectedClipTilesToFront() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SelectedClipTiles;
                            ClearClipSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                _clipTileViewModels.Move(_clipTileViewModels.IndexOf(sctvm), 0);
                                sctvm.IsSelected = true;                                
                            }
                            _clipTrayRef.ScrollIntoView(SelectedClipTiles[0]);
                        }));
            }
            finally {
                IsBusy = false;
            }
        }

        private AsyncCommand _sendSelectedClipTilesToBackCommand;
        public IAsyncCommand SendSelectedClipTilesToBackCommand {
            get {
                if (_sendSelectedClipTilesToBackCommand == null) {
                    _sendSelectedClipTilesToBackCommand = new AsyncCommand(SendSelectedClipTilesToBack, CanSendSelectedClipTilesToBack);
                }
                return _sendSelectedClipTilesToBackCommand;
            }
        }
        private bool CanSendSelectedClipTilesToBack(object args) {
            if (IsBusy || MainWindowViewModel.IsLoading || VisibileClipTiles.Count == 0) {
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
        private async Task SendSelectedClipTilesToBack() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SelectedClipTiles;
                            ClearClipSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                _clipTileViewModels.Move(_clipTileViewModels.IndexOf(sctvm), _clipTileViewModels.Count - 1);
                                sctvm.IsSelected = true;
                            }
                            _clipTrayRef.ScrollIntoView(SelectedClipTiles[SelectedClipTiles.Count-1]);
                        }));
            } finally {
                IsBusy = false;
            }
        }

        private RelayCommand _deleteSelectedClipsCommand;
        public ICommand DeleteSelectedClipsCommand {
            get {
                if (_deleteSelectedClipsCommand == null) {
                    _deleteSelectedClipsCommand = new RelayCommand(DeleteSelectedClips,CanDeleteSelectedClips);
                }
                return _deleteSelectedClipsCommand;
            }
        }
        private bool CanDeleteSelectedClips() {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsEditingClipTile &&
                !IsEditingClipTitle &&
                !IsPastingTemplate;
        }
        private void DeleteSelectedClips() {
            int lastSelectedClipTileIdx = -1;
            foreach (var ct in SelectedClipTiles) {
                lastSelectedClipTileIdx = VisibileClipTiles.IndexOf(ct);
                this.Remove(ct);
            }
            ClearClipSelection();
            if (VisibileClipTiles.Count > 0) {
                if (lastSelectedClipTileIdx <= 0) {
                    VisibileClipTiles[0].IsSelected = true;
                } else if(lastSelectedClipTileIdx < VisibileClipTiles.Count) {
                    VisibileClipTiles[lastSelectedClipTileIdx].IsSelected = true;
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
            if(MainWindowViewModel.IsLoading) {
                return false;
            }
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
            MainWindowViewModel.TagTrayViewModel.UpdateTagAssociation();
        }

        private RelayCommand _assignHotkeyCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if (_assignHotkeyCommand == null) {
                    _assignHotkeyCommand = new RelayCommand(AssignHotkey, CanAssignHotkey);
                }
                return _assignHotkeyCommand;
            }
        }
        private bool CanAssignHotkey() {
            return SelectedClipTiles.Count == 1;
        }
        private void AssignHotkey() {
            SelectedClipTiles[0].ShortcutKeyList = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(
                this, 
                "Paste " + SelectedClipTiles[0].CopyItemTitle, 
                SelectedClipTiles[0].ShortcutKeyList, 
                SelectedClipTiles[0].PasteClipCommand);
        }

        private RelayCommand _invertSelectionCommand;
        public ICommand InvertSelectionCommand {
            get {
                if (_invertSelectionCommand == null) {
                    _invertSelectionCommand = new RelayCommand(InvertSelection, CanInvertSelection);
                }
                return _invertSelectionCommand;
            }
        }
        private bool CanInvertSelection() {
            return SelectedClipTiles.Count != VisibileClipTiles.Count;
        }
        private void InvertSelection() {
            var sctvml = SelectedClipTiles;
            ClearClipSelection();
            foreach(var vctvm in VisibileClipTiles) {
                if(!sctvml.Contains(vctvm)) {
                    vctvm.IsSelected = true;
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
                dlg.DefaultFileName = "Mp_Exported_Data_" + MpHelpers.Instance.RemoveSpecialCharacters(DateTime.Now.ToString());
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
                        _clipTileViewModels.Remove(tileToRemove);
                    }
                    ClearClipSelection();
                    focusedClip.IsSelected = true;
                    focusedClip.IsClipItemFocused = true;
                    //this breaks mvvm but no way to refresh tokens w/o
                    _clipTrayRef.Items.Refresh();
                })
            );   
        }

        private AsyncCommand _speakSelectedClipsAsyncCommand;
        public IAsyncCommand SpeakSelectedClipsAsyncCommand {
            get {
                if (_speakSelectedClipsAsyncCommand == null) {
                    _speakSelectedClipsAsyncCommand = new AsyncCommand(SpeakSelectedClipsAsync, CanSpeakSelectedClipsAsync);
                }
                return _speakSelectedClipsAsyncCommand;
            }
        }
        private bool CanSpeakSelectedClipsAsync(object args) {
            foreach(var sctvm in SelectedClipTiles) {
                if(!string.IsNullOrEmpty(sctvm.CopyItemPlainText)) {
                    return true;
                }
            }
            return false;
        }
        private async Task SpeakSelectedClipsAsync() {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(); 
                speechSynthesizer.SetOutputToDefaultAudioDevice();
                var installedVoices = new List<InstalledVoice>();
                foreach (InstalledVoice voice in speechSynthesizer.GetInstalledVoices()) {
                    installedVoices.Add(voice);
                    Console.WriteLine(voice.VoiceInfo.Name);
                }
                speechSynthesizer.SelectVoice(installedVoices[0].VoiceInfo.Name);
                speechSynthesizer.Rate = 0;
                speechSynthesizer.SpeakCompleted += (s, e) => {
                    speechSynthesizer.Dispose();
                };
                // Create a PromptBuilder object and append a text string.
                PromptBuilder promptBuilder = new PromptBuilder();                   

                foreach (var sctvm in SelectedClipTiles) {
                    //speechSynthesizer.SpeakAsync(sctvm.CopyItemPlainText);
                    promptBuilder.AppendText(Environment.NewLine + sctvm.CopyItemPlainText);
                }

                // Speak the contents of the prompt asynchronously.
                speechSynthesizer.SpeakAsync(promptBuilder);
                
            }, DispatcherPriority.Background);            
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
                _clipTileViewModels.Add(ctvm);
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
            if(MainWindowViewModel.IsLoading) {
                return false;
            }
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
