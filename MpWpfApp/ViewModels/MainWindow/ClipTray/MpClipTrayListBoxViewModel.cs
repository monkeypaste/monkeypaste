﻿using System;
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
using AlphaChiTech.Virtualization.Pageing;
using AlphaChiTech.VirtualizingCollection;
using AsyncAwaitBestPractices.MVVM;
using DataGridAsyncDemoMVVM.filtersort;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MpWpfApp {
    public class MpClipTrayViewModel : MpUndoableViewModelBase<MpClipTrayViewModel>, IDropTarget {
        #region Private Variables      
        private MpMultiSelectListView _clipTrayRef = null;
        //private object _dragClipBorderElement = null;

        Stopwatch sw = new Stopwatch();

        private List<MpCopyItem> _testList = new List<MpCopyItem>();

        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;

        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private int _filterWaitingCount;

        private int _totalItemsAtLoad = 0;
        //private CancellationTokenSource _highlightCancellationTokenSource = null;

        //private MpClipTileViewModelPagedSourceProviderAsync _clipTileViewModelPagedSourceProviderAsync = null;

        //private MpClipTileViewModelDataSource _clipTileViewModelDataSource = null;
        #endregion

        #region Properties

        #region Items Source
        private MpObservableCollection<MpClipTileViewModel> _clipTileViewModels = new MpObservableCollection<MpClipTileViewModel>();
        public MpObservableCollection<MpClipTileViewModel> ClipTileViewModels {
            get {
                return _clipTileViewModels;
            }
            set {
                if(_clipTileViewModels != value) {
                    _clipTileViewModels = value;
                    OnPropertyChanged(nameof(ClipTileViewModels));
                }
            }
        }
        #endregion

        public List<MpClipTileViewModel> SelectedClipTiles {
            get {
                return ClipTileViewModels.Where(ct => ct.IsSelected).ToList();
            }
        }

        public List<MpClipTileViewModel> VisibileClipTiles {
            get {
                return ClipTileViewModels.Where(ct => ct.TileVisibility == Visibility.Visible).ToList();
            }
        }

        public MpClipTileViewModel PrimarySelectedClipTile {
            get {
                if(SelectedClipTiles.Count == 0) {
                    return null;
                }
                if(SelectedClipTiles.Count == 1) {
                    return SelectedClipTiles[0];
                }
                foreach(var sctvm in SelectedClipTiles) {
                    if(sctvm.IsPrimarySelected) {
                        return sctvm;
                    }
                }
                return null;
            }
        }

        public MpClipboardManager ClipboardManager { get; private set; }

        public bool IsDragging { get; set; } = false;

        public bool DoPaste { get; set; } = false;

        public Point StartDragPoint;

        public bool IsAnyTileExpanded {
            get {
                foreach(var ctvm in ClipTileViewModels) {
                    if(ctvm.IsExpanded) {
                        return true;
                    }
                }
                return false;
            }
        }

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

        //private bool _isLoading = true;
        //public bool IsLoading {
        //    get {
        //        return _isLoading;
        //    }
        //    set {
        //        if (_isLoading != value) {
        //            _isLoading = value;
        //            OnPropertyChanged(nameof(IsLoading));
        //        }
        //    }
        //}

        //public bool IsItemLoading {
        //    get {
        //        foreach(var ctvm in ClipTileViewModels) {
        //            if(ctvm.IsLoading) {
        //                return true;
        //            }
        //        }
        //        return false;
        //    }
        //}

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
                    foreach(var subctvm in sctvm.RichTextBoxViewModelCollection) {
                        if(subctvm.IsEditingSubTitle) {
                            return true;
                        }
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
                    foreach (var subctvm in sctvm.RichTextBoxViewModelCollection) {
                        if (subctvm.IsEditingContent) {
                            return true;
                        }
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
                    foreach (var subctvm in sctvm.RichTextBoxViewModelCollection) {
                        if (subctvm.IsPastingTemplate) {
                            return true;
                        }
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
            //IsLoading = true;
            CanAcceptChildren = true;
            ClipTileViewModels.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(EmptyListMessageVisibility));
                OnPropertyChanged(nameof(ClipTrayVisibility));
            };
            var allItems = MpCopyItem.GetAllCopyItems(out _totalItemsAtLoad);            
            foreach (var ci in allItems) {
                if(ci.IsSubCompositeItem) {
                    continue;
                }
                Add(new MpClipTileViewModel(ci));
            }
            //FilterCommand = new RelayCommand<MemberPathFilterText>(async o => await Filter(o));
            //SortCommand = new RelayCommand<MemberPathSortingDirection>(async o => await Sort(o));
        }

        public void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.PropertyChanged += (s, e8) => {
                switch (e8.PropertyName) {
                    case nameof(MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.SearchText):
                        var hlt = MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.SearchText;

                        //wait till all highlighting is complete then hide non-matching tiles at the same time
                        var newVisibilityDictionary = new Dictionary<MpClipTileViewModel, Visibility>();
                        bool showMatchNav = false;
                        foreach(MpClipTileViewModel ctvm in ClipTileViewModels) {                            
                            var newVisibility = ctvm.HighlightTextRangeViewModelCollection.PerformHighlighting(hlt).Result;
                            newVisibilityDictionary.Add(ctvm, newVisibility);
                            if(ctvm.HighlightTextRangeViewModelCollection.Count > 1) {
                                showMatchNav = true;
                            }
                        }
                        foreach(var kvp in newVisibilityDictionary) {
                            kvp.Key.TileVisibility = kvp.Value;
                        }
                        MainWindowViewModel.SearchBoxViewModel.SearchNavigationButtonPanelVisibility = showMatchNav ? Visibility.Visible:Visibility.Collapsed;
                        break;
                }
            };

            var clipTray = (MpMultiSelectListView)sender;
            var scrollViewer = clipTray.GetDescendantOfType<ScrollViewer>();

            _clipTrayRef = clipTray;

            #region Drag/Drop
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
                int dropIdx = item == null || item == clipTray ? 0 : ClipTileViewModels.IndexOf(dropVm);
                if (dropIdx >= 0) {
                    ClearClipSelection();
                    for (int i = 0; i < dragClipViewModel.Count; i++) {
                        int dragIdx = ClipTileViewModels.IndexOf(dragClipViewModel[i]);
                        ClipTileViewModels.Move(dragIdx, dropIdx);
                        dragClipViewModel[i].IsSelected = true;
                        if (i == 0) {
                            dragClipViewModel[i].IsClipItemFocused = true;
                        }
                    }
                } else {
                    Console.WriteLine("MainWindow drop error cannot find lasrt moused over tile");
                }
            };
            #endregion

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
                if (!IsPastingTemplate && (hitTestResult == null || hitTestResult.VisualHit.GetVisualAncestor<ListViewItem>() == null)) {
                    MainWindowViewModel.ClearEdits();
                    //e4.Handled = true;
                }
            };

            ClipboardManager = new MpClipboardManager((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow));

            ClipboardManager.ClipboardChanged += (s, e53) => AddItemFromClipboard();

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

            //Task.Run(() => {
            //    while(ClipTileViewModels.Count < this._totalItemsAtLoad && !IsItemLoading) {
            //        Thread.Sleep(15);
            //    }
            //    IsLoading = false;
            //});
            
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
        public void AddItemFromClipboard() {
            var sw = new Stopwatch();
            sw.Start();

            //var priority = DispatcherPriority.Background;
            var newCopyItem = MpCopyItem.CreateFromClipboard(MainWindowViewModel.ClipTrayViewModel.ClipboardManager.LastWindowWatcher.LastHandle);

            if (newCopyItem == null) {
                //this occurs if the copy item is not a known format
                return;
            }
            if (MainWindowViewModel.AppModeViewModel.IsInAppendMode && SelectedClipTiles.Count > 0) {
                //when in append mode just append the new items text to selecteditem
                PrimarySelectedClipTile.MergeClip(new MpClipTileViewModel(newCopyItem));
                 
                if (Properties.Settings.Default.NotificationShowAppendBufferToast) {
                    MpStandardBalloonViewModel.ShowBalloon(
                    "Append Buffer",
                    SelectedClipTiles[0].CopyItemPlainText,
                    Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                }
                if (Properties.Settings.Default.NotificationDoCopySound) {
                    MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                }
                return;
            }
            if (newCopyItem.CopyItemId > 0) {
                //item is a duplicate
                var existingClipTile = GetClipTileByCopyItemId(newCopyItem.CopyItemId);
                if (existingClipTile != null) {
                    Console.WriteLine("Ignoring duplicate copy item");
                    existingClipTile.CopyCount++;
                    existingClipTile.CopyDateTime = DateTime.Now;
                    ClipTileViewModels.Move(ClipTileViewModels.IndexOf(existingClipTile), 0);
                    ClearClipSelection();
                    existingClipTile.IsSelected = true;
                }
            } else {
                //VirtualizationManager.Instance.RunOnUI(() => {

                //});
                var nctvm = new MpClipTileViewModel(newCopyItem);
                this.Add(nctvm);
                //nctvm.SetCopyItem(newCopyItem);
                if (Properties.Settings.Default.NotificationDoCopySound) {
                    MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                }
                if (IsTrialExpired) {
                    MpStandardBalloonViewModel.ShowBalloon(
                    "Trial Expired",
                    "Please update your membership to use Monkey Paste",
                    Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                }
                MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(nctvm);
                //GetClipTray().Items.Refresh();
            }
            ResetClipSelection();

            sw.Stop();
            Console.WriteLine("Time to create new copyitem: " + sw.ElapsedMilliseconds + " ms");
        }

        public void Add(MpClipTileViewModel ctvm) {
            ClipTileViewModels.Insert(0, ctvm);
            //MainWindowViewModel.ClipTileSortViewModel.PerformSelectedSortCommand.Execute(null);
            Refresh();
        }

        public void Refresh() {
            //if(MainWindowViewModel == null || MainWindowViewModel.IsLoading) {
            //    return;
            //}
           _clipTrayRef?.Items.Refresh();
        }

        public void Remove(MpClipTileViewModel clipTileToRemove) {
            ClipTileViewModels.Remove(clipTileToRemove);
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
                IntPtr pasteToWindowHandle = ClipboardManager.LastWindowWatcher.LastHandle;
                if(_selectedPasteToAppPathViewModel != null) {
                    pasteToWindowHandle = MpRunningApplicationManager.Instance.SetActiveProcess(
                        _selectedPasteToAppPathViewModel.AppPath, _selectedPasteToAppPathViewModel.IsAdmin);
                }
                ClipboardManager.PasteDataObject(pasteDataObject, pasteToWindowHandle);

                //resort list so pasted items are in front and paste is tracked
                for (int i = SelectedClipTiles.Count - 1; i >= 0; i--) {
                    var sctvm = SelectedClipTiles[i];
                    ClipTileViewModels.Move(ClipTileViewModels.IndexOf(sctvm), 0);
                    new MpPasteHistory(sctvm.CopyItem, ClipboardManager.LastWindowWatcher.LastHandle);
                }
                Refresh();
            } else if (pasteDataObject == null) {
                Console.WriteLine("MainWindow Hide Command pasteDataObject was null, ignoring paste");
            }
            _selectedPasteToAppPathViewModel = null;
            ResetClipSelection();
        }

        public List<MpClipTileViewModel> GetClipTilesByAppId(int appId) {
            var ctvml = new List<MpClipTileViewModel>();
            foreach(MpClipTileViewModel ctvm in ClipTileViewModels) {
                if(ctvm.CopyItemAppId == appId) {
                    ctvml.Add(ctvm);
                }
            }
            return ctvml;
        }

        public MpClipTileViewModel GetClipTileByCopyItemId(int copyItemId) {
            foreach (MpClipTileViewModel ctvm in ClipTileViewModels) {
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

        #region Drag & Drop
        void IDropTarget.DragOver(IDropInfo dropInfo) {
            var sourceItem = dropInfo.Data as MpRtbListBoxItemRichTextBoxViewModel;
            MpClipTileRichTextBoxViewModelCollection targetRtbVmCollection = null;
            MpRtbListBoxItemRichTextBoxViewModel targetRtbVm = null;
            if (dropInfo.TargetItem is MpRtbListBoxItemRichTextBoxViewModel) {
                targetRtbVm = dropInfo.TargetItem as MpRtbListBoxItemRichTextBoxViewModel;
                targetRtbVmCollection = targetRtbVm.RichTextBoxViewModelCollection;
            } else if (dropInfo.TargetItem is MpClipTileRichTextBoxViewModelCollection) {
                targetRtbVmCollection = dropInfo.TargetItem as MpClipTileRichTextBoxViewModelCollection;
                if (targetRtbVmCollection.Count > 0) {
                    if (dropInfo.DropPosition.Y < 0) {
                        targetRtbVm = targetRtbVmCollection[0];
                    } else {
                        targetRtbVm = targetRtbVmCollection[targetRtbVmCollection.Count - 1];
                    }
                }
            }

            if (sourceItem != null && targetRtbVm != null) {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo) {
            var sourceItem = dropInfo.Data as MpRtbListBoxItemRichTextBoxViewModel;
            MpClipTileRichTextBoxViewModelCollection targetRtbVmCollection = null;
            MpRtbListBoxItemRichTextBoxViewModel targetRtbVm = null;
            if (dropInfo.TargetItem is MpRtbListBoxItemRichTextBoxViewModel) {
                targetRtbVm = dropInfo.TargetItem as MpRtbListBoxItemRichTextBoxViewModel;
                targetRtbVmCollection = targetRtbVm.RichTextBoxViewModelCollection;
            } else if (dropInfo.TargetItem is MpClipTileRichTextBoxViewModelCollection) {
                targetRtbVmCollection = dropInfo.TargetItem as MpClipTileRichTextBoxViewModelCollection;
                if (targetRtbVmCollection.Count > 0) {
                    if (dropInfo.DropPosition.Y < 0) {
                        targetRtbVm = targetRtbVmCollection[0];
                    } else {
                        targetRtbVm = targetRtbVmCollection[targetRtbVmCollection.Count - 1];
                    }
                }
            }

            if (targetRtbVmCollection != null) {
                targetRtbVmCollection.Add(sourceItem);
            }
        }
        #endregion

        #region Private Methods

        private int GetClipTileFromDrag(Point startLoc,Point curLoc) {
            return 0;
        }       

        #endregion

        #region Commands
        private RelayCommand _selectNextItemCommand;
        public ICommand SelectNextItemCommand {
            get {
                if(_selectNextItemCommand == null) {
                    _selectNextItemCommand = new RelayCommand(SelectNextItem, CanSelectNextItem);
                }
                return _selectNextItemCommand;
            }
        }
        private bool CanSelectNextItem() {
            return SelectedClipTiles.Count > 0 && SelectedClipTiles.Any(x => VisibileClipTiles.IndexOf(x) != VisibileClipTiles.Count - 1);
        }
        private void SelectNextItem() {
            var maxItem = SelectedClipTiles.Max(x => VisibileClipTiles.IndexOf(x));
            ClearClipSelection();
            VisibileClipTiles[maxItem + 1].IsSelected = true;
        }

        private RelayCommand _selectPreviousItemCommand;
        public ICommand SelectPreviousItemCommand {
            get {
                if (_selectPreviousItemCommand == null) {
                    _selectPreviousItemCommand = new RelayCommand(SelectPreviousItem, CanSelectPreviousItem);
                }
                return _selectPreviousItemCommand;
            }
        }
        private bool CanSelectPreviousItem() {
            return SelectedClipTiles.Count > 0 && SelectedClipTiles.Any(x => VisibileClipTiles.IndexOf(x) != 0);
        }
        private void SelectPreviousItem() {
            var minItem = SelectedClipTiles.Min(x => VisibileClipTiles.IndexOf(x));
            ClearClipSelection();
            VisibileClipTiles[minItem - 1].IsSelected = true;
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
            if(brush == null) {
                return;
            }
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            BitmapSource sharedSwirl = null;
                            foreach (var sctvm in SelectedClipTiles) {
                                sctvm.TitleBackgroundColor = brush;
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

        private RelayCommand<object> _pasteSelectedClipsCommand;
        public ICommand PasteSelectedClipsCommand {
            get {
                if (_pasteSelectedClipsCommand == null) {
                    _pasteSelectedClipsCommand = new RelayCommand<object>(PasteSelectedClips, CanPasteSelectedClips);
                }
                return _pasteSelectedClipsCommand;
            }
        }
        private bool CanPasteSelectedClips(object ptapId) {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false && 
                !IsEditingClipTile && 
                !IsEditingClipTitle && 
                !IsPastingTemplate &&
                !IsTrialExpired;
        }
        private void PasteSelectedClips(object ptapId) {
            if(ptapId != null && ptapId.GetType() == typeof(int) && (int)ptapId > 0) {
                //when pasting to a user defined application
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = MpPasteToAppPathViewModelCollection.Instance.Where(x => x.PasteToAppPathId == (int)ptapId).ToList()[0];                
            } else if(ptapId != null && ptapId.GetType() == typeof(IntPtr) && (IntPtr)ptapId != IntPtr.Zero) {
                //when pasting to a running application
                _selectedPasteToAppPathWindowHandle = (IntPtr)ptapId;
                _selectedPasteToAppPathViewModel = null;
            }
              else {
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = null;
            }
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
            for (int i = 0; i < SelectedClipTiles.Count && i < VisibileClipTiles.Count; i++) {
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
                                ClipTileViewModels.Move(ClipTileViewModels.IndexOf(sctvm), 0);
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
            for (int i = 0; i < SelectedClipTiles.Count && i < VisibileClipTiles.Count; i++) {
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
                                ClipTileViewModels.Move(ClipTileViewModels.IndexOf(sctvm), ClipTileViewModels.Count - 1);
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
            SelectedClipTiles[0].ShortcutKeyString = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(
                this, 
                "Paste " + SelectedClipTiles[0].CopyItemTitle, 
                SelectedClipTiles[0].ShortcutKeyString, 
                SelectedClipTiles[0].PasteClipCommand,null);
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
                    List<MpClipTileViewModel> clipTilesToRemove = new List<MpClipTileViewModel>();
                    foreach (MpClipTileViewModel selectedClipTile in SelectedClipTiles) {
                        if (selectedClipTile == PrimarySelectedClipTile) {
                            continue;
                        }
                        PrimarySelectedClipTile.MergeClip(selectedClipTile);
                        clipTilesToRemove.Add(selectedClipTile);
                    }
                    foreach (MpClipTileViewModel tileToRemove in clipTilesToRemove) {
                        ClipTileViewModels.Remove(tileToRemove);
                    }
                    var psctvm = PrimarySelectedClipTile;
                    ClearClipSelection();
                    ClipTileViewModels.Move(ClipTileViewModels.IndexOf(psctvm), 0);
                    psctvm.IsSelected = true;
                    psctvm.IsClipItemFocused = true;
                    //this breaks mvvm but no way to refresh tokens w/o
                    Refresh();
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
                ClipTileViewModels.Add(ctvm);
                ctvm.IsSelected = true;
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