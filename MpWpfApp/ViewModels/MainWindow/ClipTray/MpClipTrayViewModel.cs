using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MonkeyPaste;
using System.Collections.Concurrent;


namespace MpWpfApp {
    public class MpClipTrayViewModel : MpSingletonViewModel<MpClipTrayViewModel>  {
        #region Private Variables      

        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;

        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private List<MpCopyItem> _newModels = new List<MpCopyItem>();

        private MpCopyItem _appendModeCopyItem = null;

        private int _initialLoadCount = 0;
        private int _pageSize = 0;
        private int _nextQueryOffsetIdx = 0;
        private int _lastQueryOffsetIdx = 0;

        private Dictionary<int, int> _manualSortOrderLookup = null;
                
        #endregion

        #region Properties
        public string SelectedClipTilesMergedPlainText, SelectedClipTilesCsv;
        public string[] SelectedClipTilesFileList, SelectedClipTilesMergedPlainTextFileList, SelectedClipTilesMergedRtfFileList;
        
        #region View Models
        
        [MpChildViewModel(typeof(MpClipTileViewModel),true)]
        public ObservableCollection<MpClipTileViewModel> Items { get; set; } = new ObservableCollection<MpClipTileViewModel>();

        [MpAffectsChild]
        public List<MpClipTileViewModel> SelectedItems {
            get {
                return Items.Where(ct => ct.IsSelected).OrderBy(x => x.LastSelectedDateTime).ToList();
            }
        }
        public List<MpClipTileViewModel> VisibleItems {
            get {
                return Items.Where(x=>!x.IsPlaceholder).OrderBy(x=>Items.IndexOf(x)).ToList();
            }
        }

        public MpClipTileViewModel LastSelectedClipTile {
            get {
                if (SelectedItems.Count == 0) {
                    return null;
                }
                return SelectedItems[SelectedItems.Count - 1];
            }
        }

        [MpAffectsChild]
        public MpClipTileViewModel PrimaryItem {
            get {
                if (SelectedItems.Count == 0) {
                    return HeadItem;
                }
                return SelectedItems[0];
            }
        }

        public MpClipTileViewModel HeadItem {
            get {
                if(Items.Count == 0) {
                    return null;
                }
                return Items[0];
            }
        }

        public MpClipTileViewModel TailItem {
            get {
                if (Items.Count == 0) {
                    return null;
                }
                return Items[Items.Count-1];
            }
        }

        public List<MpContentItemViewModel> SelectedContentItemViewModels {
            get {
                var scivml = new List<MpContentItemViewModel>();
                foreach(var sctvm in SelectedItems) {
                    scivml.AddRange(sctvm.SelectedItems);
                }
                return scivml;
            }
        }

        public List<MpCopyItem> SelectedModels {
            get {
                var cil = new List<MpCopyItem>();
                foreach(var sctvm in SelectedItems) {
                    cil.AddRange(sctvm.SelectedItems.OrderBy(y=>y.LastSubSelectedDateTime).Select(x=>x.CopyItem));
                }
                return cil;
            }
        }

        public MpClipTileViewModel ExpandedTile => Items.FirstOrDefault(x => x.IsExpanded);

        #region Context Menu Item View Models

        public ObservableCollection<MpContextMenuItemViewModel> TranslateLanguageMenuItems { get; set; } = new ObservableCollection<MpContextMenuItemViewModel>();

        public ObservableCollection<MpContextMenuItemViewModel> TagMenuItems { get; set; } = new ObservableCollection<MpContextMenuItemViewModel>();

        #endregion

        #endregion

        #region Layout
        #endregion

        #region Appearance

        public Brush ClipTrayBackgroundBrush {
            get {
                if (MpTagTrayViewModel.Instance.SelectedTagTile == null) {
                    return Brushes.Transparent;
                }
                return MpTagTrayViewModel.Instance.SelectedTagTile.TagBrush;
            }
        }

        #endregion

        #region Business Logic

        public int RemainingItemsCountThreshold { get; private set; }

        public int TotalItemsInQuery { get; private set; } = 0;

        #endregion

        #region State

        public bool IsAnyBusy => Items.Any(x => x.IsAnyBusy) || IsBusy;

        public bool IsRequery { get; private set; } = false;

        public bool IsTrayEmpty => Items.Count == 0 && !IsRequery;// || Items.All(x => x.IsPlaceholder);

        public bool IsSelectionReset { get; set; } = false;

        public bool IgnoreSelectionReset { get; set; } = false;

        public bool IsPastingHotKey { get; set; } = false;

        public bool IsPastingSelected { get; set; } = false;

        public bool IsAnyContextMenuOpened => Items.Any(x => x.IsAnyContextMenuOpened);

        public bool IsTrayDropping { get; set; } = false;

        public bool IsAnyTileItemDragging => Items.Any(x => x.IsAnyItemDragging);

        public bool IsAnyDropOnTile => Items.Any(x => x.IsDroppingOnTile);

        public bool IsAnyTileFlipped => Items.Any(x => x.IsFlipped || x.IsFlipping);

        [MpAffectsChild]
        public bool IsAnyTileExpanded => Items.Any(x => x.IsExpanded);

        public bool IsAnyEditing => Items.Any(x => x.IsAnyEditingContent);

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

        public bool IsAnyHovering => Items.Any(x => x.IsHovering);

        [MpAffectsChild]
        public bool IsScrolling { get; set; }

        public bool IsAnyEditingClipTitle => Items.Any(x => x.IsAnyEditingTitle);

        public bool IsAnyEditingClipTile => Items.Any(x => x.IsAnyEditingContent);

        public bool IsAnyPastingTemplate => Items.Any(x => x.IsAnyPastingTemplate);

        //public bool IsPreSelection { get; set; } = false;
        #endregion

        #region Visibility
        
        #endregion

        #endregion

        #region Events
        public event EventHandler<object> OnFocusRequest;
        public event EventHandler OnUiRefreshRequest;
        public event EventHandler<object> OnScrollIntoViewRequest;
        public event EventHandler<double> OnScrollToXRequest;
        public event EventHandler OnScrollToHomeRequest;
        #endregion

        #region Constructors

        public async Task Init() {
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                PropertyChanged += MpClipTrayViewModel_PropertyChanged;
                Items.CollectionChanged += Items_CollectionChanged;
                MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
                MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
                MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;

                _pageSize = (int)(MpMeasurements.Instance.TotalVisibleClipTiles / 2);
                RemainingItemsCountThreshold = 0;// _pageSize;
                MpDataModelProvider.Instance.Init(new MpWpfQueryInfo(), _pageSize);

                _initialLoadCount = _pageSize * 3;

                MpMessenger.Instance.Register<MpMessageType>(MpDataModelProvider.Instance.QueryInfo, ReceivedQueryInfoMessage);
            });
        }

        public MpClipTrayViewModel() : base() { }

        #endregion

        #region Public Methods

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsTrayEmpty));
            return;
            if(e.OldItems != null && e.OldItems.Count > 0 && !IsAnyTileItemDragging) {
                foreach(MpClipTileViewModel octvm in e.OldItems) {
                    octvm.Dispose();
                }
            }
        }

        private async void ReceivedQueryInfoMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.QueryChanged:
                    IsRequery = true;
                    await Task.Delay(300);

                    await Requery();
                    break;
            }
        }

        private void MpClipTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsScrolling):
                    var ctvm = Items.Where(x => x.IsHovering).FirstOrDefault();
                    if (ctvm != null) {
                        ctvm.OnPropertyChanged(nameof(ctvm.TileBorderBrush));
                        //ctvm.ItemViewModels.ForEach(x => x.OnPropertyChanged(nameof(x.TileBorderBrush)));
                    }
                    break;
                case nameof(IsAnyTileItemDragging):
                    if(IsAnyTileItemDragging) {
                        MpMessenger.Instance.Send<MpMessageType>(MpMessageType.ItemDragBegin);
                    } else {
                        MpMessenger.Instance.Send<MpMessageType>(MpMessageType.ItemDragEnd);
                    }
                    break;
            }
        }

        public async Task Requery() {
            var sw = new Stopwatch();
            sw.Start();

            IsBusy = true;
            ClearClipSelection();

            MpDataModelProvider.Instance.ResetQuery();

            TotalItemsInQuery = await MpDataModelProvider.Instance.FetchCopyItemCountAsync();
            _nextQueryOffsetIdx = 0;

            int itemCountDiff = Items.Count - Math.Min(_initialLoadCount,TotalItemsInQuery);
            if (itemCountDiff > 0) {
                while (itemCountDiff > 0) {
                    //extra item is added when dropping partial composite (creating new tile) at end of page so remove extra
                    //items need to be removed when query total count is less than initialLoadCount
                    Items.RemoveAt(0);
                    itemCountDiff--;
                }
            } else if (itemCountDiff < 0) {
                while (itemCountDiff < 0) {
                    var ctvm = await CreateClipTileViewModel(null);
                    Items.Add(ctvm);
                    itemCountDiff++;
                }
            }


            var citl = await MpDataModelProvider.Instance.FetchCopyItemRangeAsync(0, _initialLoadCount);
            if (citl.Count >= Items.Count) {
                //occurs when tags switched quickly
                int itemsToAdd = citl.Count - Items.Count;
                while(itemsToAdd > 0) {
                    var ctvm = await CreateClipTileViewModel(null);
                    Items.Add(ctvm);
                }
            }
            for (int i = 0; i < citl.Count; i++) {                
                await Items[i].InitializeAsync(citl[i].Item1,citl[i].Item2);
            }

            _lastQueryOffsetIdx = 0;
            _nextQueryOffsetIdx = citl.Count;

            IsBusy = false;
            ResetClipSelection();

            MpMessenger.Instance.Send<MpMessageType>(MpMessageType.RequeryCompleted);

            sw.Stop();
            MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);
            
            IsRequery = false;
        }

        public ICommand LoadMoreClipsCommand => new RelayCommand<object>(
            async (isLoadMore) => {
                IsBusy = true;
                bool isLeft = ((int)isLoadMore) >= 0;
                if (isLeft && _nextQueryOffsetIdx < TotalItemsInQuery) {
                    IList<Tuple<MpCopyItem,int>> citl = await MpDataModelProvider.Instance.FetchCopyItemRangeAsync(_nextQueryOffsetIdx, _pageSize);

                    for (int i = 0; i < citl.Count; i++) {
                        var ctvm = await CreateClipTileViewModel(citl[i].Item1, citl[i].Item2);
                        Items.Add(ctvm);
                    }

                    _nextQueryOffsetIdx += citl.Count;

                    int itemsToRemove = Items.Count - _initialLoadCount;
                    Items.RemoveRange(0, itemsToRemove);

                    _lastQueryOffsetIdx = _nextQueryOffsetIdx - _initialLoadCount - _pageSize;
                    //MpConsole.WriteLine($"Loaded {cil.Count} items, offsetIdx: {_nextQueryOffsetIdx}");
                } else if(_lastQueryOffsetIdx >= 0) {
                    IList<Tuple<MpCopyItem,int>> citl = await MpDataModelProvider.Instance.FetchCopyItemRangeAsync(_lastQueryOffsetIdx, _pageSize);

                    for (int i = 0; i < citl.Count; i++) {
                        var ctvm = await CreateClipTileViewModel(citl[i].Item1, citl[i].Item2);
                        Items.Insert(i,ctvm);
                    }

                    _lastQueryOffsetIdx -= citl.Count;

                    int itemsToRemove = Items.Count - _initialLoadCount;
                    if(itemsToRemove > 0) {
                        Items.RemoveRange(_initialLoadCount - _pageSize - 1, itemsToRemove);

                        _nextQueryOffsetIdx = _lastQueryOffsetIdx + _initialLoadCount + _pageSize;
                    }
                }
                //await Task.Delay(100);
                IsBusy = false;
            },
            (itemsToLoad) => {
                return itemsToLoad != null && !IsBusy;
            });


        public ICommand JumpToPageIdxCommand => new RelayCommand<int>(
            async (idx) => {
                IList<Tuple<MpCopyItem,int>> citl = await MpDataModelProvider.Instance.FetchCopyItemRangeAsync(idx, _initialLoadCount);

                int firstPlaceHolderIdx = Items.IndexOf(Items.Where(x => x.IsPlaceholder).FirstOrDefault());

                for (int i = 0; i < citl.Count; i++) {
                    if (i + firstPlaceHolderIdx >= Items.Count - 1) {
                        var phctvm2 = await CreateClipTileViewModel(null);
                        Items.Add(phctvm2);
                    }
                    await Items[i + firstPlaceHolderIdx].InitializeAsync(citl[i].Item1, citl[i].Item2);

                    var phctvm = await CreateClipTileViewModel(null);
                    Items.Add(phctvm);
                }

                _nextQueryOffsetIdx = idx + citl.Count - 1;

                MpConsole.WriteLine($"Loaded {citl.Count} items, offsetIdx: {_nextQueryOffsetIdx}");
            },
            (idx) => {
                return idx >= 0 && idx < TotalItemsInQuery;
            });
        

        public async Task AddNewModels(bool selectAll = false) {
            if (_newModels.Count == 0) {
                return;
            }
            ClearClipSelection();
            foreach (var nci in _newModels) {
                //var ctvm = await CreateClipTileViewModel(nci);
                //Items.Insert(0, ctvm);

                await Items[Items.Count - 1].InitializeAsync(nci);
                Items.Move(Items.Count - 1, 0);
                Items[0].IsSelected = true;
            }
            _newModels.Clear();
            await MpTagTrayViewModel.Instance.RefreshAllCounts();
           if(!selectAll) {
                ResetClipSelection();
            }
        }

        public async Task<ObservableCollection<MpContextMenuItemViewModel>> GetTagMenuItemsForSelectedItems() {
            var tmil = new ObservableCollection<MpContextMenuItemViewModel>();

            foreach (var tagTile in MpTagTrayViewModel.Instance.TagTileViewModels) {
                if (tagTile.IsSudoTag) {
                    continue;
                }
                int isCheckedCount = 0;
                foreach (var sm in SelectedModels) {
                    bool isLinked = await tagTile.IsLinked(sm);
                    if (isLinked) {
                        isCheckedCount++;
                    }
                }
                bool? isChecked;
                if (isCheckedCount == SelectedModels.Count) {
                    isChecked = true;
                } else if (isCheckedCount > 0) {
                    isChecked = null;
                } else {
                    isChecked = false;
                }
                tmil.Add(
                    new MpContextMenuItemViewModel(
                        tagTile.TagName,
                        MpClipTrayViewModel.Instance.LinkTagToCopyItemCommand,
                        tagTile,
                        isChecked,
                        string.Empty,
                        null,
                        tagTile.ShortcutKeyString,
                        tagTile.TagBrush));
            }
            return tmil;
        }


        #region View Invokers
        public void RequestScrollToX(double xoffset) {
            OnScrollToXRequest?.Invoke(this, xoffset);
        }

        public void RequestScrollIntoView(object obj) {
            OnScrollIntoViewRequest?.Invoke(this, obj);
        }

        public void RequestScrollToHome() {
            OnScrollToHomeRequest?.Invoke(this, null);
        }

        public void RequestFocus(object obj) {
            OnFocusRequest?.Invoke(this, obj);
        }

        public void RequestUiRefresh() {
            OnUiRefreshRequest?.Invoke(this, null);
        }
        #endregion      

        public async Task UpdateSortOrder(bool fromModel = false) {            
            if (fromModel) {
                //ClipTileViewModels.Sort(x => x.CopyItem.CompositeSortOrderIdx);
            } else {
                bool isManualSort = MpDataModelProvider.Instance.QueryInfo.SortType == MpContentSortType.Manual;

                if (isManualSort) {
                    _manualSortOrderLookup = new Dictionary<int, int>();
                    foreach (var ctvm in Items) {
                        if (_manualSortOrderLookup.ContainsKey(ctvm.HeadItem.CopyItemId)) {
                            continue;
                        }
                        _manualSortOrderLookup.Add(ctvm.HeadItem.CopyItemId, Items.IndexOf(ctvm));
                    }
                }

                bool isDesc = MpDataModelProvider.Instance.QueryInfo.IsDescending;
                int tagId = MpDataModelProvider.Instance.QueryInfo.TagId;
                var citl = await MpDataModelProvider.Instance.GetCopyItemTagsForTagAsync(tagId);

                if (tagId == MpTag.AllTagId || tagId == MpTag.RecentTagId) {
                    //ignore sorting for sudo tags
                    return;
                }

                int count = isDesc ? citl.Count : 1;
                //loop through available tiles and reset tag's sort order, 
                //removing existing items from known ones and creating new ones if that's the case (it shouldn't)
                foreach (var ctvm in this.Items) {
                    foreach (var civm in ctvm.ItemViewModels) {
                        MpCopyItemTag cit = citl.Where(x => x.CopyItemId == civm.CopyItem.Id).FirstOrDefault();
                        if (cit == null) {
                            cit = await MpCopyItemTag.Create(tagId, (int)civm.CopyItem.Id, count);
                        } else {
                            cit.CopyItemSortIdx = count;
                            citl.Remove(cit);
                        }
                        await cit.WriteToDatabaseAsync();
                        if (isDesc) {
                            count--;
                        } else {
                            count++;
                        }
                    }
                }
                //sort remaining unavailables by their sort order
                if (isDesc) {
                    citl = citl.OrderByDescending(x => x.CopyItemSortIdx).ToList();
                } else {
                    citl = citl.OrderBy(x => x.CopyItemSortIdx).ToList();
                }
                //update remaining unavailable items going with last count
                foreach (var cit in citl) {
                    cit.CopyItemSortIdx = count;
                    await cit.WriteToDatabaseAsync();
                    if (isDesc) {
                        count--;
                    } else {
                        count++;
                    }
                }
            }
        }
        public MpCopyItemType GetSelectedClipsType() {
            //returns none if all clips aren't the same type
            if (SelectedItems.Count == 0) {
                return MpCopyItemType.None;
            }
            MpCopyItemType firstType = SelectedItems[0].HeadItem.CopyItem.ItemType;
            foreach (var sctvm in SelectedItems) {
                if (sctvm.HeadItem.CopyItem.ItemType != firstType) {
                    return MpCopyItemType.None;
                }
            }
            return firstType;
        }

        public void ClearClipEditing() {
            foreach (var ctvm in Items) {
                if(ctvm == null) {
                    //occurs on first hide w/ async virtal items
                    continue;
                }
                ctvm.ClearEditing();
            }            
        }

        public void ClearClipSelection(bool clearEditing = true) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (clearEditing) {
                    ClearClipEditing();
                }
                foreach (var ctvm in Items) {
                    if (ctvm == null) {
                        //occurs on first hide w/ async virtal items
                        continue;
                    }
                    ctvm.ClearSelection();
                }
            }));
        }

        public void ResetClipSelection(bool clearEditing = true) {
            IsSelectionReset = true;
            MpHelpers.Instance.RunOnMainThread(() => {
                ClearClipSelection(clearEditing);

                if (VisibleItems.Count > 0 && VisibleItems[0] != null) {

                    VisibleItems[0].IsSelected = true;
                    //if (!MpSearchBoxViewModel.Instance.IsTextBoxFocused) {
                    //    RequestFocus(SelectedItems[0]);
                    //}
                }
                RequestScrollToHome();
            });
            IsSelectionReset = false;
        }

        public void UnFlipAllTiles() {
            // TODO make async and do Unflip here
            foreach(var ctvm in Items) {
                if(ctvm.IsFlipped) {
                    FlipTileCommand.Execute(ctvm);
                }
            }
        }

        public void RefreshAllCommands() {
            foreach (MpClipTileViewModel ctvm in Items) {
                ctvm.RefreshAsyncCommands();
            }
        }

        private async Task AddItemFromClipboard(Dictionary<string, string> cd) {
            var totalAddSw = new Stopwatch();
            totalAddSw.Start();

            var createItemSw = new Stopwatch();
            createItemSw.Start();
            var newCopyItem = await MpCopyItemBuilder.CreateFromClipboard(cd);

            MpConsole.WriteLine("CreateFromClipboardAsync: " + createItemSw.ElapsedMilliseconds + "ms");

            if (newCopyItem == null) {
                //this occurs if the copy item is not a known format or app init
                MpConsole.WriteTraceLine("Unable to create copy item from clipboard!");
                return;
            } else if (MpAppModeViewModel.Instance.IsInAppendMode) {
                //when in append mode just append the new items text to selecteditem
                if(_appendModeCopyItem == null) {
                    if (PrimaryItem == null) {
                        _appendModeCopyItem = newCopyItem;
                    } else {
                        _appendModeCopyItem = PrimaryItem.HeadItem.CopyItem;
                    }
                }              

                if (_appendModeCopyItem != newCopyItem) {
                    int compositeChildCount = await MpDataModelProvider.Instance.GetCompositeChildCountAsync(_appendModeCopyItem.Id);
                    newCopyItem.CompositeParentCopyItemId = _appendModeCopyItem.Id;
                    newCopyItem.CompositeSortOrderIdx = compositeChildCount + 1;
                    await newCopyItem.WriteToDatabaseAsync();

                    if (Properties.Settings.Default.NotificationShowAppendBufferToast) {
                        // TODO now composite item doesn't roll up children so the buffer needs to be created here
                        // if I use this at all
                        MpStandardBalloonViewModel.ShowBalloon(
                            "Append Buffer",
                            SelectedItems[0].TailItem.CopyItem.ItemData.ToPlainText(),
                            Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                    }

                    if (Properties.Settings.Default.NotificationDoCopySound) {
                        MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                    }
                }
                
            } else {
                _appendModeCopyItem = null;
                if (newCopyItem.Id < 0) {
                    //item is a duplicate
                    newCopyItem.Id *= -1;
                    MpConsole.WriteLine("Ignoring duplicate copy item");
                    newCopyItem.CopyCount++;
                    // reseting CopyDateTime will move item to top of recent list
                    newCopyItem.CopyDateTime = DateTime.Now;
                } else {
                    if (MpPreferences.Instance.NotificationDoCopySound) {
                        MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                    }
                    if (MpPreferences.Instance.IsTrialExpired) {
                        MpStandardBalloonViewModel.ShowBalloon(
                            "Trial Expired",
                            "Please update your membership to use Monkey Paste",
                            Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                    }                    
                }
            }
            totalAddSw.Stop();
            MpConsole.WriteLine("Time to create new copyitem: " + totalAddSw.ElapsedMilliseconds + " ms");

            _newModels.Add(newCopyItem);
        }

        public void OnClipboardChanged(object sender, Dictionary<string, string> cd) {
            MpHelpers.Instance.RunOnMainThread(async () => {
                await AddItemFromClipboard(cd);
            });            
        }

        public async Task<MpClipTileViewModel> CreateClipTileViewModel(MpCopyItem ci, int queryPrimaryItemId = 0) {
            var nctvm = new MpClipTileViewModel(this);
            await nctvm.InitializeAsync(ci,queryPrimaryItemId);
            return nctvm;
        }

        public async Task<IDataObject> GetDataObjectFromSelectedClips(bool isDragDrop = false, bool isToExternalApp = false) {
            IDataObject d = new DataObject();

            //selection (if all subitems are dragging select host if no subitems are selected select all)
            SelectedItems.ForEach(x => x.DoCommandSelection());

            string rtf = string.Empty.ToRichText();
            if (isToExternalApp) {
                //gather rtf and text NOT setdata it needs file drop first
                foreach (var sctvm in SelectedItems) {
                    string sctrtf = await sctvm.GetSubSelectedPastableRichText(isToExternalApp);
                    rtf = MpHelpers.Instance.CombineRichText(sctrtf, rtf);
                }

            }

            //set file drop (always must set so when dragged out of application user doesn't get no-drop cursor)
            if (MpHelpers.Instance.IsProcessNeedFileDrop(MpRunningApplicationManager.Instance.ActiveProcessPath) &&
                isDragDrop) {
                //only when pasting into explorer or notepad must have file drop
                var sctfl = SelectedClipTilesFileList;
                if (sctfl != null) {
                    if (MpHelpers.Instance.IsProcessLikeNotepad(MpRunningApplicationManager.Instance.ActiveProcessPath)) {
                        d.SetData(DataFormats.FileDrop, SelectedClipTilesMergedPlainTextFileList);
                    } else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                        d.SetData(DataFormats.FileDrop, SelectedClipTilesMergedRtfFileList);
                    } else {
                        d.SetData(DataFormats.FileDrop, sctfl);
                    }
                }
            }

            if (isToExternalApp) {
                //set rtf and text
                if (!string.IsNullOrEmpty(rtf)) {
                    d.SetData(DataFormats.Rtf, rtf);
                    string pt = rtf.ToPlainText();
                    d.SetData(DataFormats.Text, rtf.ToPlainText());
                }

                //set image
                if (SelectedItems.Count == 1 && SelectedItems[0].HeadItem.CopyItem.ItemType == MpCopyItemType.Image) {
                    d.SetData(DataFormats.Bitmap, SelectedItems[0].HeadItem.CopyItem.ItemData.ToBitmapSource());
                }

                //set csv
                var sctcsv = SelectedClipTilesCsv;
                if (sctcsv != null) {
                    d.SetData(DataFormats.CommaSeparatedValue, sctcsv);
                }

                //update metrics
                foreach (var ctvm in SelectedItems) {
                    if (ctvm.SelectedItems.Count == 0) {
                        ctvm.HeadItem.CopyItem.PasteCount++;
                    } else {
                        foreach (var rtbvm in ctvm.SelectedItems) {
                            rtbvm.CopyItem.PasteCount++;
                        }
                    }
                }
            }

            //set resorting
            if (isDragDrop && SelectedItems != null && SelectedItems.Count > 0) {
                foreach (var dctvm in SelectedItems) {
                    if (dctvm.Count == 0 ||
                        dctvm.SelectedItems.Count == dctvm.Count ||
                        dctvm.SelectedItems.Count == 0) {
                        //dctvm.IsClipDragging = true;
                    }
                }
                d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedItems.ToList());
            }

            return d;
            //awaited in MpMainWindowViewModel.Instance.HideWindow
        }

        public async Task PasteDataObject(IDataObject pasteDataObject, bool fromHotKey = false) {
            if (IsAnyPastingTemplate) {
                MpMainWindowViewModel.Instance.IsMainWindowLocked = false;
            }

            //called in the oncompleted of hide command in mwvm
            if (pasteDataObject != null) {
                MpConsole.WriteLine("Pasting " + SelectedItems.Count + " items");
                IntPtr pasteToWindowHandle = IntPtr.Zero;
                if (_selectedPasteToAppPathViewModel != null) {
                    pasteToWindowHandle = MpRunningApplicationManager.Instance.SetActiveProcess(
                        _selectedPasteToAppPathViewModel.AppPath,
                        _selectedPasteToAppPathViewModel.IsAdmin,
                        _selectedPasteToAppPathViewModel.IsSilent,
                        _selectedPasteToAppPathViewModel.Args,
                        IntPtr.Zero,
                        _selectedPasteToAppPathViewModel.WindowState);
                } else if (_selectedPasteToAppPathWindowHandle != IntPtr.Zero) {
                    var windowState = WinApi.SW_SHOWMAXIMIZED;
                    if (MpRunningApplicationManager.Instance.LastWindowStateHandleDictionary.ContainsKey(_selectedPasteToAppPathWindowHandle)) {
                        windowState = MpHelpers.Instance.GetShowWindowValue(MpRunningApplicationManager.Instance.LastWindowStateHandleDictionary[_selectedPasteToAppPathWindowHandle]);
                    }
                    WinApi.ShowWindowAsync(_selectedPasteToAppPathWindowHandle, windowState);
                    pasteToWindowHandle = _selectedPasteToAppPathWindowHandle;
                } else {
                    pasteToWindowHandle = MpClipboardManager.Instance.LastWindowWatcher.LastHandle;
                }

                MpClipboardManager.Instance.PasteDataObject(pasteDataObject, pasteToWindowHandle);

                if (_selectedPasteToAppPathViewModel != null && _selectedPasteToAppPathViewModel.PressEnter) {
                    System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                }

                if (!fromHotKey) {
                    //resort list so pasted items are in front and paste is tracked
                    for (int i = SelectedItems.Count - 1; i >= 0; i--) {
                        var sctvm = SelectedItems[i];

                        var a = await MpDataModelProvider.Instance.GetAppByPath(MpHelpers.Instance.GetProcessPath(MpClipboardManager.Instance.LastWindowWatcher.LastHandle));
                        var aid = a == null ? 0 : a.Id;
                        foreach(var ivm in sctvm.ItemViewModels) {
                            var ud = await MpDataModelProvider.Instance.GetUserDeviceByGuid(MpPreferences.Instance.ThisDeviceGuid);
                            await new MpPasteHistory() {
                                AppId = aid,
                                PasteHistoryGuid = Guid.NewGuid(),
                                CopyItemId = ivm.CopyItem.Id,
                                UserDeviceId = ud.Id,
                                PasteDateTime = DateTime.Now
                            }.WriteToDatabaseAsync();
                        }
                    }
                }
            } else if (pasteDataObject == null) {
                MpConsole.WriteLine("MainWindow Hide Command pasteDataObject was null, ignoring paste");
            }
            _selectedPasteToAppPathViewModel = null;
            if (!fromHotKey) {
                ResetClipSelection();
            }

            IsPastingHotKey = IsPastingSelected = false;
            foreach (var sctvm in SelectedItems) {
                //clean up pasted items state after paste
                if (sctvm.HasTemplates) {
                    sctvm.ClearEditing();
                    foreach (var rtbvm in sctvm.ItemViewModels) {
                        rtbvm.IsPastingTemplate = false;
                        rtbvm.TemplateCollection.ResetAll();
                        rtbvm.TemplateRichText = string.Empty;
                        rtbvm.RequestUiReset();
                    }
                    sctvm.RequestUiUpdate();
                    sctvm.RequestScrollToHome();
                }
            }
        }

        public List<MpClipTileViewModel> GetClipTilesByAppId(int appId) {
            var ctvml = new List<MpClipTileViewModel>();
            foreach (MpClipTileViewModel ctvm in Items) {
                if (ctvm.ItemViewModels.Any(x=>x.CopyItem.Source.AppId == appId)) {
                    ctvml.Add(ctvm);
                }
            }
            return ctvml;
        }

        public MpContentItemViewModel GetContentItemViewModelById(int ciid) {
            foreach (var ctvm in Items) {
                foreach (var civm in ctvm.ItemViewModels) {
                    var ortbvm = ctvm.ItemViewModels.Where(x => x.CopyItemId == ciid).FirstOrDefault();
                    if (ortbvm != null) {
                        return ortbvm;
                    }
                }
            }
            return null;
        }

        public MpContentItemViewModel GetCopyItemViewModelByCopyItem(MpCopyItem ci) {
            foreach (var ctvm in Items) {
                foreach (var civm in ctvm.ItemViewModels) {
                    var ortbvm = ctvm.ItemViewModels.Where(x => x.CopyItem == ci).FirstOrDefault();
                    if (ortbvm != null) {
                        return ortbvm;
                    }
                }
            }
            return null;
        }

        public int GetSelectionOrderIdxForItem(object vm) {
            //returns -1 if vm is not associated with selection
            //returns -2 if vm is a ctvm with sub-selected rtbvms
            if (vm == null) {
                return -1;
            }
            int vmIdx = -1;
            for (int i = 0; i < SelectedItems.Count; i++) {
                var sctvm = SelectedItems[i];
                if (sctvm.SelectedItems.Count <= 1 &&
                   sctvm.Count <= 1) {
                    vmIdx++;
                    if (sctvm == vm) {
                        return vmIdx;
                    }
                    continue;
                }
                for (int j = 0; j < sctvm.SelectedItems.Count; j++) {
                    var srtbvm = sctvm.SelectedItems[j];
                    vmIdx++;
                    if (srtbvm == vm) {
                        return vmIdx;
                    }
                    if (srtbvm.Parent == vm) {
                        return -2;
                    }
                }
            }
            return -1;
        }

        public void InitIntroItems() {
            //var introItem1 = new MpCopyItem(
            //        MpCopyItemType.RichText,
            //        "Welcome to MonkeyPaste!",
            //        MpHelpers.Instance.ConvertPlainTextToRichText("Take a moment to look through the available features in the following tiles, which are always available in the 'Help' pinboard"));

            //var introItem2 = new MpCopyItem(
            //    MpCopyItemType.RichText,
            //    "One place for your clipboard",
            //    MpHelpers.Instance.ConvertPlainTextToRichText(""));
            //Properties.Settings.Default.IsInitialLoad = false;
            //Properties.Settings.Default.Save();
        }
        #endregion

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpCopyItem ci) {
                //_allTiles.Add(CreateClipTileViewModel(ci));
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                var ivm = GetContentItemViewModelById(ci.Id);
                //ivm.CopyItem = ci;
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                MpHelpers.Instance.RunOnMainThread(async () => {
                    var ctvm = Items.Where(x => x.ItemViewModels.Any(y => y.CopyItemId == ci.Id)).FirstOrDefault();
                    if(ctvm == null) {
                        return;
                    }
                    int tileIdx = Items.IndexOf(ctvm);
                    var civm = ctvm.ItemViewModels.Where(x => x.CopyItemId == ci.Id).FirstOrDefault();
                    int itemIdx = ctvm.ItemViewModels.IndexOf(civm);
                    civm.IsSelected = false;
                    if (ctvm.ItemViewModels.Count == 1) {
                        //when removing multiple items wait until the last one to reset selection, prefering next item
                        ctvm.IsSelected = false;
                        int newSelectedIdx = tileIdx + 1;
                        if (newSelectedIdx >= VisibleItems.Count) {
                            //this was the last item 
                            if (VisibleItems.Count == 1) {
                                //there are no other items to select
                                newSelectedIdx = -1;
                            } else {
                                //select previous item
                                newSelectedIdx = tileIdx - 1;
                            }
                        }
                        if (newSelectedIdx >= 0) {
                            VisibleItems[newSelectedIdx].IsSelected = true;
                        }
                    } else if (ctvm.ItemViewModels.Count > 1 && ctvm.SelectedItems.Count == 0) {
                        //when removing a composite item wait and not all removed, wait until last one and prefer selecting next item
                        int newSubSelectedIdx = itemIdx + 1;
                        if (newSubSelectedIdx >= ctvm.ItemViewModels.Count) {
                            if (ctvm.ItemViewModels.Count == 1) {
                                //no more items to sub select
                                newSubSelectedIdx = -1;
                            } else {
                                newSubSelectedIdx = itemIdx - 1;
                            }
                            if (newSubSelectedIdx >= 0) {
                                ctvm.ItemViewModels[newSubSelectedIdx].IsSelected = true;
                            }
                        }
                    }
                    ctvm.ItemViewModels.Remove(civm);
                    if (ctvm.ItemViewModels.Count == 0) {
                        Items.Move(tileIdx, Items.Count - 1);
                        await ctvm.InitializeAsync(null);
                    }

                    MpDataModelProvider.Instance.QueryInfo.NotifyQueryChanged();
                });
            }
        }

        #region Sync Events

        private void MpDbObject_SyncDelete(object sender, MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (sender is MpCopyItem ci) {
                    var ctvmToRemove = GetContentItemViewModelById(ci.Id);
                    if (ctvmToRemove != null) {
                        ctvmToRemove.CopyItem.StartSync(e.SourceGuid);
                        //ctvmToRemove.CopyItem.Color.StartSync(e.SourceGuid);
                        ctvmToRemove.Parent.ItemViewModels.Remove(ctvmToRemove);
                        if (ctvmToRemove.Parent.ItemViewModels.Count == 0) {
                            Items.Remove(ctvmToRemove.Parent);
                        }
                        ctvmToRemove.CopyItem.EndSync();
                        //ctvmToRemove.CopyItem.Color.EndSync();
                    }
                }
            }));
        }

        private void MpDbObject_SyncUpdate(object sender, MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
            }));
        }

        private void MpDbObject_SyncAdd(object sender, MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread(async() => {
                if (sender is MpCopyItem ci) {
                    ci.StartSync(e.SourceGuid);
                    ci.Source.App.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.IconImage.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.IconBorderImage.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.IconBorderHighlightImage.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.IconBorderHighlightSelectedImage.StartSync(e.SourceGuid);

                    var dupCheck = GetContentItemViewModelById(ci.Id);
                    if (dupCheck == null) {
                        if (ci.Id == 0) {
                            await ci.WriteToDatabaseAsync();
                        }
                        _newModels.Add(ci);
                        //AddNewTiles();
                    } else {
                        MpConsole.WriteTraceLine(@"Warning, attempting to add existing copy item: " + dupCheck.CopyItem.ItemData + " ignoring and updating existing.");
                        //dupCheck.CopyItem = ci;
                    }
                    ci.Source.App.EndSync();
                    ci.Source.App.Icon.EndSync();
                    ci.Source.App.Icon.IconImage.EndSync();
                    ci.Source.App.Icon.IconBorderImage.EndSync();
                    ci.Source.App.Icon.IconBorderHighlightImage.EndSync();
                    ci.Source.App.Icon.IconBorderHighlightSelectedImage.EndSync();
                    ci.EndSync();

                    ResetClipSelection();
                }
            }, DispatcherPriority.Background);
        }

        #endregion

        #endregion

        #region Private Methods

        #region Sync Events


        #endregion

        #endregion

        #region Commands

        public ICommand FlipTileCommand => new RelayCommand<object>(
            async (tileToFlip) => {
                var ctvm = tileToFlip as MpClipTileViewModel;
                ctvm.IsBusy = true;
                if (ctvm.IsFlipped) {
                    //ClearClipSelection();
                    //ctvm.IsSelected = true;
                    ctvm.IsFlipping = true;
                    while(ctvm.IsFlipping) {
                        await Task.Delay(100);
                    }
                } else {
                    var flippedCtvm = Items.FirstOrDefault(x => x.IsFlipped);
                    if(flippedCtvm != null) {
                        flippedCtvm.IsFlipping = true;
                        while (flippedCtvm.IsFlipping) {
                            await Task.Delay(100);
                        }
                    }
                    ClearClipSelection();
                    ctvm.IsFlipping = true;
                    while (ctvm.IsFlipping) {
                        await Task.Delay(100);
                    }
                    ctvm.IsSelected = true;
                    //await ctvm.PrimaryItem.AnalyticItemCollectionViewModel.Init();
                    //ctvm.IsFlipping = true;
                }

                ctvm.IsBusy = false;
            });


        public ICommand ExcludeSubSelectedItemApplicationCommand => new RelayCommand<object>(
            async (args) => {
                await MpAppCollectionViewModel.Instance.UpdateRejection(MpAppCollectionViewModel.Instance.GetAppViewModelByAppId(PrimaryItem.PrimaryItem.CopyItem.Source.AppId), true);
            },
            (args) => {
                return MpClipTrayViewModel.Instance.SelectedItems.Count == 1;
            });

        private RelayCommand<object> _searchWebCommand;
        public ICommand SearchWebCommand {
            get {
                if (_searchWebCommand == null) {
                    _searchWebCommand = new RelayCommand<object>(SearchWeb);
                }
                return _searchWebCommand;
            }
        }
        private void SearchWeb(object args) {
            if (args == null || args.GetType() != typeof(string)) {
                return;
            }
            MpHelpers.Instance.OpenUrl(args.ToString() + System.Uri.EscapeDataString(SelectedClipTilesMergedPlainText));
        }

        private RelayCommand _selectNextItemCommand;
        public ICommand SelectNextItemCommand {
            get {
                if (_selectNextItemCommand == null) {
                    _selectNextItemCommand = new RelayCommand(SelectNextItem, CanSelectNextItem);
                }
                return _selectNextItemCommand;
            }
        }
        private bool CanSelectNextItem() {
            return SelectedItems.Count > 0 &&
                   SelectedItems.Any(x => VisibleItems.IndexOf(x) != VisibleItems.Count - 1);
        }
        private void SelectNextItem() {
            var maxItem = SelectedItems.Max(x => VisibleItems.IndexOf(x));
            ClearClipSelection();
            VisibleItems[maxItem + 1].IsSelected = true;
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
            return SelectedItems.Count > 0 && SelectedItems.Any(x => VisibleItems.IndexOf(x) != 0);
        }
        private void SelectPreviousItem() {
            var minItem = SelectedItems.Min(x => VisibleItems.IndexOf(x));
            ClearClipSelection();
            VisibleItems[minItem - 1].IsSelected = true;
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
            foreach (var ctvm in VisibleItems) {
                ctvm.IsSelected = true;
            }
        }

        public ICommand ChangeSelectedClipsColorCommand => new RelayCommand<object>(
            (brush) => {
                if (brush == null) {
                    return;
                }
                var b = brush as Brush;
                foreach (var scivm in SelectedContentItemViewModels) {
                    scivm.CopyItemColorBrush = b;
                    scivm.TitleSwirlViewModel.ForceBrush(b);
                }
            });

        public ICommand PerformHotkeyPasteCommand => new RelayCommand<object>(
            async (args) => {
                if (args == null) {
                    return;
                }
                MpConsole.WriteLine("HotKey pasting copyitemid: " + (int)args);
                IsPastingHotKey = true;
                int copyItemId = (int)args;
                IDataObject pasteDataObject = null;
                var pctvm = GetContentItemViewModelById(copyItemId);
                if (pctvm != null) {
                    ClearClipSelection();
                    pctvm.IsSelected = true;
                    pctvm.Parent.SubSelectAll();
                    pasteDataObject = await GetDataObjectFromSelectedClips(false, true);
                    ClearClipSelection();
                } else {
                    //otherwise check if it is a composite within a tile
                    MpContentItemViewModel prtbvm = GetContentItemViewModelById(copyItemId) as MpContentItemViewModel;
                    //foreach (var ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                    //    prtbvm = ctvm.ContentContainerViewModel.GetRtbItemByCopyItemId(copyItemId);
                    //    if (prtbvm != null) {
                    //        break;
                    //    }
                    //}
                    if (prtbvm != null) {
                        ClearClipSelection();
                        prtbvm.Parent.IsSelected = true;
                        prtbvm.Parent.ClearSelection();
                        prtbvm.IsSelected = true;
                        pasteDataObject = await GetDataObjectFromSelectedClips(false, true);
                        prtbvm.Parent.ClearSelection();
                        ClearClipSelection();
                    }
                }

                if (MpMainWindowViewModel.IsMainWindowOpen) {
                    //occurs during hotkey paste and set in ctvm.GetPastableRichText
                    MpMainWindowViewModel.Instance.HideWindowCommand.Execute(pasteDataObject);
                } else if (pasteDataObject != null) {
                    //In order to paste the app must hide first 
                    //this triggers hidewindow to paste selected items
                    await PasteDataObject(pasteDataObject);
                    ResetClipSelection();
                }
            },
            (args) => {
                return !MpMainWindowViewModel.IsMainWindowOpen;
            });

        public ICommand CopySelectedClipsCommand => new RelayCommand(
            async() => {
                var ido = await GetDataObjectFromSelectedClips(false, false);
                MpClipboardManager.Instance.SetDataObject(ido);
            });

        public ICommand PasteSelectedClipsCommand => new RelayCommand<object>(
            (args) => {

                if (args != null && args.GetType() == typeof(int) && (int)args > 0) {
                    //when pasting to a user defined application
                    _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                    _selectedPasteToAppPathViewModel = MpPasteToAppPathViewModelCollection.Instance.FindById((int)args);
                } else if (args != null && args.GetType() == typeof(IntPtr) && (IntPtr)args != IntPtr.Zero) {
                    //when pasting to a running application
                    _selectedPasteToAppPathWindowHandle = (IntPtr)args;
                    _selectedPasteToAppPathViewModel = null;
                } else {
                    _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                    _selectedPasteToAppPathViewModel = null;
                }
                //In order to paste the app must hide first 
                //this triggers hidewindow to paste selected items
                IsPastingSelected = true;
                MpMainWindowViewModel.Instance.HideWindowCommand.Execute(true);
                IsPastingSelected = false;
            },
            (args) => {
                return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                    !IsAnyTileExpanded &&
                    !IsAnyEditingClipTile &&
                    !IsAnyEditingClipTitle &&
                    !IsAnyPastingTemplate &&
                    !MpPreferences.Instance.IsTrialExpired;

            });

        public ICommand BringSelectedClipTilesToFrontCommand => new RelayCommand(
            async() => {
                try {
                    IsBusy = true;
                    await Dispatcher.CurrentDispatcher.BeginInvoke(
                            DispatcherPriority.Normal,
                            (Action)(() => {
                                var tempSelectedClipTiles = SelectedItems;
                                ClearClipSelection();

                                foreach (var sctvm in tempSelectedClipTiles) {
                                //Items.Move(Items.IndexOf(sctvm), 0);
                                sctvm.IsSelected = true;
                                }
                                RequestScrollIntoView(SelectedItems[0]);
                            }));
                }
                finally {
                    IsBusy = false;
                }
            },
            () => {
                if (IsBusy ||
                     MpMainWindowViewModel.Instance.IsMainWindowLoading ||
                     VisibleItems.Count == 0 ||
                     SelectedItems.Count == 0) {
                            return false;
                }
                bool canBringForward = false;
                for (int i = 0; i < SelectedItems.Count && i < VisibleItems.Count; i++) {
                    if (!SelectedItems.Contains(VisibleItems[i])) {
                        canBringForward = true;
                        break;
                    }
                }
                return canBringForward;
            });

        public ICommand SendSelectedClipTilesToBackCommand => new RelayCommand(
            async () => {
                try {
                    IsBusy = true;
                    await Dispatcher.CurrentDispatcher.BeginInvoke(
                            DispatcherPriority.Normal,
                            (Action)(() => {
                                var tempSelectedClipTiles = SelectedItems;
                                ClearClipSelection();

                                foreach (var sctvm in tempSelectedClipTiles) {
                                //Items.Move(Items.IndexOf(sctvm), Items.Count - 1);
                                sctvm.IsSelected = true;
                                }
                                RequestScrollIntoView(SelectedItems[SelectedItems.Count - 1]);
                            }));
                }
                finally {
                    IsBusy = false;
                }
            },
            () => {
                if (IsBusy ||
                MpMainWindowViewModel.Instance.IsMainWindowLoading ||
                VisibleItems.Count == 0 ||
                SelectedItems.Count == 0) {
                    return false;
                }
                bool canSendBack = false;
                for (int i = 0; i < SelectedItems.Count && i < VisibleItems.Count; i++) {
                    if (!SelectedItems.Contains(VisibleItems[VisibleItems.Count - 1 - i])) {
                        canSendBack = true;
                        break;
                    }
                }
                return canSendBack;
            });

        public ICommand DeleteSelectedClipsCommand => new RelayCommand(
            async () => {
                //int lastSelectedClipTileIdx = SelectedItems.Max(x => Items.IndexOf(x));
                //foreach (var ct in SelectedItems) {
                //    lastSelectedClipTileIdx = VisibileClipTiles.IndexOf(ct);
                //    if()
                //}
                //ClearClipSelection();
                //if (VisibileClipTiles.Count > 0) {
                //    if (lastSelectedClipTileIdx <= 0) {
                //        VisibileClipTiles[0].IsSelected = true;
                //    } else if (lastSelectedClipTileIdx < VisibileClipTiles.Count) {
                //        VisibileClipTiles[lastSelectedClipTileIdx].IsSelected = true;
                //    } else {
                //        VisibileClipTiles[lastSelectedClipTileIdx - 1].IsSelected = true;
                //    }
                //}
                var deleteTasks = SelectedModels.Select(x => x.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks.ToArray());
            },
            () => {
                return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                        SelectedModels.Count > 0 &&
                        !IsAnyEditingClipTile &&
                        !IsAnyEditingClipTitle &&
                        !IsAnyPastingTemplate;
            });

        public ICommand LinkTagToCopyItemCommand => new RelayCommand<MpTagTileViewModel>(
            async (tagToLink) => {
                bool isUnlink = await tagToLink.IsLinked(SelectedItems[0]);
                foreach (var selectedClipTile in SelectedItems) {
                    foreach (var ivm in selectedClipTile.ItemViewModels) {
                        if (isUnlink) {
                            await tagToLink.RemoveContentItem(ivm);
                        } else {
                            await tagToLink.AddContentItem(ivm);
                        }
                    }
                }
                await MpTagTrayViewModel.Instance.RefreshAllCounts();
                await MpTagTrayViewModel.Instance.UpdateTagAssociation();
            },
            (tagToLink) => {
                //this checks the selected clips association with tagToLink
                //and only returns if ALL selecteds clips are linked or unlinked 
                if (tagToLink == null || SelectedItems == null || SelectedItems.Count == 0) {
                    return false;
                }
                return true;
                //if (SelectedItems.Count == 1) {
                //    return true;
                //}
                //bool isLastClipTileLinked = tagToLink.IsLinked(SelectedItems[0]);
                //foreach (var selectedClipTile in SelectedItems) {
                //    if (tagToLink.IsLinked(selectedClipTile) != isLastClipTileLinked) {
                //        return false;
                //    }
                //}
                //return true;
            });

        public ICommand AssignHotkeyCommand => new RelayCommand(
            () => {
                SelectedItems[0].AssignHotkeyCommand.Execute(null);
            },
            () => SelectedItems.Count == 1);

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
            return SelectedItems.Count != VisibleItems.Count;
        }
        private void InvertSelection() {
            var sctvml = SelectedItems;
            ClearClipSelection();
            foreach (var vctvm in VisibleItems) {
                if (!sctvml.Contains(vctvm)) {
                    vctvm.IsSelected = true;
                }
            }
        }

        public ICommand EditSelectedTitleCommand => new RelayCommand(
            () => {

                SelectedItems[0].EditTitleCommand.Execute(null);
            },
            () => {
                if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return false;
                }
                return SelectedItems.Count == 1 &&
                      SelectedItems[0].SelectedItems.Count <= 1;
            });

        public ICommand EditSelectedContentCommand => new RelayCommand(
            () => {
                SelectedItems[0].IsExpanded = true;
            },
            () => {
                if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return false;
                }
                return SelectedItems.Count == 1 && SelectedItems[0].SelectedItems.Count == 1;
            });

        public ICommand SendSelectedClipsToEmailCommand => new RelayCommand(
            () => {
                MpHelpers.Instance.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, SelectedItems[0].HeadItem.CopyItem.Title, SelectedClipTilesMergedPlainText));
                //MpClipTrayViewModel.Instance.ClearClipSelection();
                //IsSelected = true;
                //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
            },
            () => {
                return !IsAnyEditingClipTile && SelectedItems.Count > 0;
            });

        public ICommand MergeSelectedClipsCommand => new RelayCommand(
            async () => {
                await Task.Delay(1);
            },
            () => {
                //return true;
                if (SelectedItems.Count <= 1) {
                    return false;
                }
                bool areAllSameType = true;
                foreach (var sctvm in SelectedItems) {
                    if (sctvm.IsTextItem) {
                        areAllSameType = false;
                    }
                }
                return areAllSameType;
            });

        public ICommand TranslateSelectedClipTextAsyncCommand => new RelayCommand<string>(
            async (toLanguage) => {
                var translatedText = await MpLanguageTranslator.Instance.TranslateAsync(SelectedItems[0].HeadItem.CopyItem.ItemData.ToPlainText(), toLanguage,"", false);
                if (!string.IsNullOrEmpty(translatedText)) {
                    SelectedItems[0].HeadItem.CopyItem.ItemData = MpHelpers.Instance.ConvertPlainTextToRichText(translatedText);
                }
            },
            (args) => {
                return SelectedItems.Count == 1 && SelectedItems[0].IsTextItem;
            });

        public ICommand SummarizeCommand => new RelayCommand(
            async () => {
                var result = await MpOpenAi.Instance.Summarize(SelectedModels[0].ItemData.ToPlainText());
                SelectedModels[0].ItemDescription = result;
                await SelectedModels[0].WriteToDatabaseAsync();
            },
            () => {
                return SelectedItems.Count == 1 &&
                       SelectedItems[0].IsTextItem &&
                       SelectedItems[0].Count == 1;
            });

        public ICommand CreateQrCodeFromSelectedClipsCommand => new RelayCommand(
            async () => {
                BitmapSource bmpSrc = null;
                await Task.Run(() => {
                    bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(SelectedClipTilesMergedPlainText);
                });                
                MpClipboardManager.Instance.SetImageWrapper(bmpSrc);
            },
            () => {
                return (GetSelectedClipsType() == MpCopyItemType.RichText) &&
                    SelectedClipTilesMergedPlainText.Length <= Properties.Settings.Default.MaxQrCodeCharLength;
            });

        public ICommand SpeakSelectedClipsCommand => new RelayCommand(
            async () => {
                await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                    var speechSynthesizer = new SpeechSynthesizer();
                    speechSynthesizer.SetOutputToDefaultAudioDevice();
                    if (string.IsNullOrEmpty(Properties.Settings.Default.SpeechSynthVoiceName)) {
                        speechSynthesizer.SelectVoice(speechSynthesizer.GetInstalledVoices()[0].VoiceInfo.Name);
                    } else {
                        speechSynthesizer.SelectVoice(Properties.Settings.Default.SpeechSynthVoiceName);
                    }
                    speechSynthesizer.Rate = 0;
                    speechSynthesizer.SpeakCompleted += (s, e) => {
                        speechSynthesizer.Dispose();
                    };
                    // Create a PromptBuilder object and append a text string.
                    PromptBuilder promptBuilder = new PromptBuilder();

                    foreach (var sctvm in SelectedItems) {
                        foreach (var ivm in sctvm.ItemViewModels) {
                            //speechSynthesizer.SpeakAsync(sctvm.CopyItemPlainText);
                            promptBuilder.AppendText(Environment.NewLine + ivm.CopyItem.ItemData.ToPlainText());
                        }
                    }

                    // Speak the contents of the prompt asynchronously.
                    speechSynthesizer.SpeakAsync(promptBuilder);

                }, DispatcherPriority.Background);
            },
            () => {
                return SelectedItems.All(x => x.IsTextItem);
            });


        public ICommand DuplicateSelectedClipsCommand => new RelayCommand(
            async () => {
                foreach (var sctvm in SelectedItems) {
                    foreach (var ivm in sctvm.SelectedItems) {
                        var clonedCopyItem = (MpCopyItem)await ivm.CopyItem.Clone();
                        await clonedCopyItem.WriteToDatabaseAsync();
                        _newModels.Add(clonedCopyItem);
                    }
                }

                await AddNewModels(true);                
            });

        public ICommand SelectItemCommand => new RelayCommand<object>(
            (arg) => {
                MpClipTileViewModel ctvm = null;
                if (arg is int ctvmIdx) {                    
                    if(ctvmIdx >= 0 && ctvmIdx < Items.Count) {
                        ctvm = Items[ctvmIdx];
                    }
                } else if(arg is MpClipTileViewModel) {
                    ctvm = arg as MpClipTileViewModel;
                } else if(arg is MpContentItemViewModel civm) {
                    ctvm = civm.Parent;
                } else {
                    throw new Exception("Cannot select " + arg.ToString());
                }
                ctvm.IsSelected = true;
            },
            (arg) => {
                return arg != null;
            });

        public ICommand AnalyzeSelectedItemCommand => new RelayCommand<int>(
            async (presetId) => {
                MpAnalyticItemViewModel analyticItemVm = null;
                MpAnalyticItemPresetViewModel presetVm = null;
                if(presetId > 0) {
                    var preset = await MpDataModelProvider.Instance.GetAnalyzerPresetById(presetId);
                    analyticItemVm = MpAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AnalyticItemId == preset.AnalyticItemId);
                    presetVm = analyticItemVm.PresetViewModels.FirstOrDefault(x => x.Preset.Id == preset.Id);
                } else {
                    //default preset is negated pk of analytic item
                    presetId *= -1;
                    analyticItemVm = MpAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AnalyticItemId == presetId);
                    presetVm = analyticItemVm.DefaultPresetViewModel;
                }

                var prevSelectedPresetVm = analyticItemVm.SelectedPresetViewModel;
                analyticItemVm.SelectPresetCommand.Execute(presetVm);
                analyticItemVm.ExecuteAnalysisCommand.Execute(PrimaryItem.PrimaryItem);

                MessageBox.Show(analyticItemVm.ResultViewModel.Result);

            });

        #endregion


    }
    public enum MpExportType {
        None = 0,
        Files,
        Csv,
        Zip
    }
}
