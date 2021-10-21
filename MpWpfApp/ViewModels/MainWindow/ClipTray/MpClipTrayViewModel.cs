using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MonkeyPaste;
using System.Collections.Concurrent;

namespace MpWpfApp {
    public class MpClipTrayViewModel : MpViewModelBase<object>  {
        #region Singleton Definition
        private static readonly Lazy<MpClipTrayViewModel> _Lazy = new Lazy<MpClipTrayViewModel>(() => new MpClipTrayViewModel());
        public static MpClipTrayViewModel Instance { get { return _Lazy.Value; } }

        public void Init() {
            
        }
        #endregion

        #region Private Variables      
        private object _tileLockObject = null;

        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;

        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private List<MpClipTileViewModel> _hiddenTiles = new List<MpClipTileViewModel>();

        private List<MpCopyItem> _newModels = new List<MpCopyItem>();
        
        private List<MpClipTileViewModel> _availableTiles = new List<MpClipTileViewModel>();

        private MpCopyItem _appendModeCopyItem = null;

        private bool _isLoading = false;

        private List<MpCopyItem> _prevPage = new List<MpCopyItem>();
        private List<MpCopyItem> _nextPage = new List<MpCopyItem>();
        private int _pageCount = 0;
        private int _totalItemsInQuery = 0;
        private int _headIdxInTotal = 0;
        private int _tailIdxInTotal {
            get {
                int idx = _headIdxInTotal + _pageCount - 1;
                return idx < _totalItemsInQuery ? idx : _totalItemsInQuery - 1;
            }
        }
        private int _remainingItemsCount = 0;
        private Dictionary<int, int> _manualSortOrderLookup = null;
        private List<int> _selectedHeadContentIds = new List<int>();

        private List<int> _filterIds;

        private MpClipTileViewModelProvider _viewModelProvider; 
        
        #endregion

        #region Properties
        public string SelectedClipTilesMergedPlainText, SelectedClipTilesCsv;
        public string[] SelectedClipTilesFileList, SelectedClipTilesMergedPlainTextFileList, SelectedClipTilesMergedRtfFileList;

        public MpWpfQueryInfo QueryInfo { get; set; }
        #region View Models
        
        [MpChildViewModel(typeof(MpClipTileViewModel),true)]
        public ObservableCollection<MpClipTileViewModel> Items { get; set; } = new ObservableCollection<MpClipTileViewModel>();

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

        private ObservableCollection<MpContextMenuItemViewModel> _translateLanguageMenuItems = null;
        public ObservableCollection<MpContextMenuItemViewModel> TranslateLanguageMenuItems {
            get {
                if (_translateLanguageMenuItems == null) {
                    _translateLanguageMenuItems = new ObservableCollection<MpContextMenuItemViewModel>();
                    foreach (var languageName in MpLanguageTranslator.Instance.LanguageList) {
                        _translateLanguageMenuItems.Add(new MpContextMenuItemViewModel(languageName, TranslateSelectedClipTextAsyncCommand, languageName, false));
                    }
                }
                return _translateLanguageMenuItems;
            }
        }

        public List<MpContextMenuItemViewModel> TagMenuItems {
            get {
                var tmil = new List<MpContextMenuItemViewModel>();

                foreach (var tagTile in MpTagTrayViewModel.Instance.TagTileViewModels) {
                    if (tagTile.IsSudoTag) {
                        continue;
                    }
                    int isCheckedCount = 0;
                    foreach (var sm in SelectedModels) {
                        if(tagTile.IsLinked(sm)) {
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
                            tagTile.TagColor));
                }
                return tmil;
            }
        }
        #endregion

        #region Layout
        public Point DropTopPoint { get; set; }
        public Point DropBottomPoint { get; set; }

        public HorizontalAlignment ItemsPanelAlignment => IsAnyTileExpanded ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        #endregion

        #region Selection 
        //public BitmapSource SelectedClipTilesBmp {
        //    get {
        //        var bmpList = new List<BitmapSource>();
        //        foreach (var sctvm in SelectedItems) {
        //            bmpList.Add(sctvm.CopyItemBmp);
        //        }
        //        return MpHelpers.Instance.CombineBitmap(bmpList, false);
        //    }
        //}

        public int SelectedIndex {
            get {
                if (SelectedItems.Count > 0) {
                    return Items.IndexOf(SelectedItems[0]);
                }
                return -1;
            }
        }


        #endregion

        #region State

        public bool IgnoreSelectionReset { get; set; } = false;

        public int TagId { get; set; } = MpTag.RecentTagId;

        public bool IsPastingHotKey { get; set; } = false;

        public bool IsPastingSelected { get; set; } = false;

        public bool IsAnyContextMenuOpened => Items.Any(x => x.IsAnyContextMenuOpened);

        public bool IsTrayDropping { get; set; } = false;

        public bool IsAnyTileItemDragging => Items.Any(x => x.IsAnyItemDragging);

        public bool IsAnyDropOnTile => Items.Any(x => x.IsDroppingOnTile);

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

        public bool IsAnyEditingClipTitle {
            get {
                foreach (var sctvm in SelectedItems) {
                    if (sctvm.IsAnyEditingTitle) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsAnyEditingClipTile {
            get {
                foreach (var sctvm in SelectedItems) {
                    if (sctvm.IsAnyEditingContent) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsAnyPastingTemplate {
            get {
                foreach (var sctvm in SelectedItems) {
                    if (sctvm.IsAnyPastingTemplate) {
                        return true;
                    }
                }
                return false;
            }
        }

        //public bool IsPreSelection { get; set; } = false;
        #endregion

        #region Visibility

        public Visibility EmptyListMessageVisibility {
            get {
                if (VisibleItems.Count == 0) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private Visibility _clipTrayVisibility = Visibility.Visible;
        public Visibility ClipTrayVisibility {
            get {
                return _clipTrayVisibility;
            }
            set {
                if (_clipTrayVisibility != value) {
                    _clipTrayVisibility = value;
                    OnPropertyChanged(nameof(ClipTrayVisibility));
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

        #endregion

        #region Events
        public event EventHandler<object> OnFocusRequest;
        public event EventHandler OnUiRefreshRequest;
        public event EventHandler<object> OnScrollIntoViewRequest;
        public event EventHandler OnScrollToHomeRequest;
        #endregion

        #region Public Methods

        public MpClipTrayViewModel() : base(null) {
            PropertyChanged += MpClipTrayViewModel_PropertyChanged;
            MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
            MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;
            _tileLockObject = new object();

            _pageCount = MpMeasurements.Instance.TotalVisibleClipTiles + 1;// * 2;
            _viewModelProvider = new MpClipTileViewModelProvider(_pageCount);            

            //BindingOperations.EnableCollectionSynchronization(Items, _tileLockObject);
            for (int i = 0; i < _pageCount; i++) {
                Items.Insert(0,CreateClipTileViewModel(null));
            }
        }

        private void MpClipTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedItems):
                    if(_isLoading) {

                    }
                    _selectedHeadContentIds = SelectedItems.Select(x => x.HeadItem.CopyItemId).ToList();
                    break;
            }
        }

        public void InitQueryInfo() {
            if(QueryInfo == null) {
                QueryInfo = new MpWpfQueryInfo();
                QueryInfo.InfoChanged += _queryInfo_InfoChanged;
            }
        }


        private void _queryInfo_InfoChanged(object sender, EventArgs e) {
            MpConsole.WriteLine("Query info changed");
            _viewModelProvider.SetQueryInfo(QueryInfo);

            Task.Run(async () => {
                await Requery();
            });
        }

        public async Task Requery() {
            await MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                var sw = new Stopwatch();
                sw.Start();
                int itemsToRemove = Items.Count - _pageCount;
                while(itemsToRemove > 0) {
                    //item is addeds when drop partial composite (creating new tile) at end of page so remove extra
                    Items.RemoveAt(0);
                    itemsToRemove--;
                }

                ClearClipSelection();

                _totalItemsInQuery = await MpDataModelProvider.Instance.FetchCopyItemCountAsync();
                _headIdxInTotal = 0;
               // _tailIdxInTotal = _totalItemsInQuery < _pageCount ? _totalItemsInQuery : _pageCount - 1;

                var initTasks = new List<Task>();
                var cil = await _viewModelProvider.ModelProvider.FetchCopyItemRangeAsync(0, _pageCount);
                for (int i = 0; i < cil.Count; i++) {
                    await Items[i].InitializeAsync(cil[i]);
                }
                for (int i = cil.Count; i < _pageCount; i++) {
                    await Items[i].InitializeAsync(null);
                }
                //await Task.WhenAll(initTasks);

                ResetClipSelection();

                MpMessenger.Instance.Send(MpMessageType.Requery);

                sw.Stop();
                MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);
            });
        }

        public ICommand RecycleItemsCommand => new RelayCommand<object>(
             (countToRecycle) => {
                int fetchCount = Math.Abs((int)countToRecycle);
                bool isLeft = ((int)countToRecycle) > 0;
                if (isLeft) {
                    if (_tailIdxInTotal >= _totalItemsInQuery - 1) {
                        //no more items to fetch
                        return;
                    }
                    
                     for (int i = 0; i < fetchCount; i++) {
                        Items.Move(0, Items.Count - 1);
                    }
                     _headIdxInTotal += fetchCount;
                 } else {
                    if (_headIdxInTotal <= 0) {
                        //no more items to fetch
                        return;
                    }
                    
                    for (int i = 0; i < fetchCount; i++) {
                        Items.Move(Items.Count - 1, 0);
                    }
                     _headIdxInTotal -= fetchCount;
                 }
            },
            (countToRecycle) => {
                return countToRecycle != null;
            });

        public ICommand LoadMoreClipsCommand => new AsyncRelayCommand<object>(
            async (itemsToLoad) => {
                int fetchCount = Math.Abs((int)itemsToLoad);
                bool isLeft = ((int)itemsToLoad) > 0;
                var loadTasks = new List<Task>();
                var cil = await _viewModelProvider.ModelProvider.FetchCopyItemRangeAsync(_headIdxInTotal, fetchCount);
                for (int i = 0; i < cil.Count; i++) {
                    loadTasks.Add(Items[i].InitializeAsync(cil[i]));
                }
                for (int i = cil.Count; i < VisibleItems.Count; i++) {
                    loadTasks.Add(Items[i].InitializeAsync(null));
                }
                await Task.WhenAll(loadTasks.ToArray());
            },
            (itemsToLoad) => {
                return itemsToLoad != null;
            });
        CancellationTokenSource[] curCtsl;
        public ConcurrentDictionary<int, MpCopyItem> CurPageLookup = new ConcurrentDictionary<int, MpCopyItem>();

        public ICommand LoadAndRecycleMoreClipsCommand => new AsyncRelayCommand<object>(
            async (itemsToLoad) => {

                if(curCtsl != null) {
                    curCtsl.ForEach(x => x.Dispose());
                    curCtsl = null;
                }

                int fetchCount = Math.Abs((int)itemsToLoad);
                bool isLeft = ((int)itemsToLoad) > 0;
                var loadAndMoveTasks = new List<Task<CancellationTokenSource>>();
                if (isLeft) {
                    if (_tailIdxInTotal >= _totalItemsInQuery - 1) {
                        //no more items to fetch
                        return;
                    }
                    _headIdxInTotal += fetchCount;
                    var cil = await _viewModelProvider.ModelProvider.FetchCopyItemRangeAsync(_tailIdxInTotal, fetchCount);
                    for (int i = 0; i < cil.Count; i++) {
                        loadAndMoveTasks.Add(PreLoadAndMoveItem(0, _pageCount - 1, cil[i].Id));
                        CurPageLookup.TryAdd(cil[i].Id, cil[i]);

                        //await Items[0].InitializeAsync(cil[i]);
                        //Items.Move(0, Items.Count - 1);
                    }
                } else {
                    if (_headIdxInTotal <= 0) {
                        //no more items to fetch
                        return;
                    }
                    _headIdxInTotal -= fetchCount;
                    var cil = await _viewModelProvider.ModelProvider.FetchCopyItemRangeAsync(_headIdxInTotal, fetchCount);
                    for (int i = 0; i < cil.Count; i++) {

                        loadAndMoveTasks.Add(PreLoadAndMoveItem(_pageCount - 1,0, cil[i].Id));

                        CurPageLookup.TryAdd(cil[i].Id, cil[i]);
                        //await Items[Items.Count - 1].InitializeAsync(cil[i]);
                        //Items.Move(Items.Count - 1, 0);
                    }
                }
                curCtsl = await Task.WhenAll(loadAndMoveTasks.ToArray());
            },
            (itemsToLoad) => {
                return itemsToLoad != null;
            });

        private async Task LoadAndMoveItem(int oldIdx,int newIdx, MpCopyItem ci) {
            await Items[oldIdx].InitializeAsync(ci);
            Items.Move(oldIdx, newIdx);
        }

        private async Task<CancellationTokenSource> PreLoadAndMoveItem(int oldIdx, int newIdx, int ciid) {
            var cts = new CancellationTokenSource();
            await Items[oldIdx].PreInitializeAsync(ciid,cts.Token);
            Items.Move(oldIdx, newIdx);
            return cts;
        }

        public async Task AddNewModels() {
            if (_newModels.Count == 0) {
                return;
            }
            foreach (var nci in _newModels) {
                await Items[Items.Count - 1].InitializeAsync(nci);
                Items.Move(Items.Count - 1, 0);
            }
            _newModels.Clear();
            MpTagTrayViewModel.Instance.RefreshAllCounts();
            ResetClipSelection();
        }

        #region View Invokers
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

        public void UpdateSortOrder(bool fromModel = false) {            
            if (fromModel) {
                //ClipTileViewModels.Sort(x => x.CopyItem.CompositeSortOrderIdx);
            } else {
                Task.Run(async()=> {
                    bool isManualSort = QueryInfo.SortType == MpContentSortType.Manual;

                    if (isManualSort) {
                        _manualSortOrderLookup = new Dictionary<int, int>();
                        foreach (var ctvm in Items) {
                            if (_manualSortOrderLookup.ContainsKey(ctvm.HeadItem.CopyItemId)) {
                                continue;
                            }
                            _manualSortOrderLookup.Add(ctvm.HeadItem.CopyItemId, Items.IndexOf(ctvm));
                        }
                    }
                    
                    bool isDesc = QueryInfo.IsDescending;
                    int tagId = QueryInfo.TagId;
                    var citl = await MpCopyItemTag.GetAllCopyItemsForTagIdAsync(tagId);

                    if (tagId == MpTag.AllTagId || tagId == MpTag.RecentTagId) {
                        //ignore sorting for sudo tags
                        return;
                    }

                    int count = isDesc ? citl.Count : 1; 
                    //loop through available tiles and reset tag's sort order, 
                    //removing existing items from known ones and creating new ones if that's the case (it shouldn't)
                    foreach(var ctvm in this.Items) {
                        foreach(var civm in ctvm.ItemViewModels) {
                            MpCopyItemTag cit = citl.Where(x => x.CopyItemId == civm.CopyItem.Id).FirstOrDefault();
                            if(cit == null) {
                                cit = MpCopyItemTag.Create(tagId, (int)civm.CopyItem.Id, count);
                            } else {
                                cit.CopyItemSortIdx = count;
                                citl.Remove(cit);
                            }
                            cit.WriteToDatabase();
                            if(isDesc) {
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
                    foreach(var cit in citl) {
                        cit.CopyItemSortIdx = count;
                        cit.WriteToDatabase();
                        if (isDesc) {
                            count--;
                        } else {
                            count++;
                        }
                    }
                });
                
            }
        }

        public void RefreshSelectedTiles() {
            SelectedItems.ForEach(x => x.RefreshTile());
        }

        public async Task RefreshTiles(int start = 0, int count = 0) {
            var sw = new Stopwatch();
            sw.Start();

            //var clearTasks = ClipTileViewModels.Select(x => x.ClearContent());
            //Task.WaitAll(clearTasks.ToArray());

            int tagId = MpTagTrayViewModel.Instance.SelectedTagTile.TagId;
            var sortColumn = MpClipTileSortViewModel.Instance.SelectedSortType.SortType;
            bool isDescending = MpClipTileSortViewModel.Instance.IsSortDescending;


            if (count == 0) {
                if(tagId == MpTag.RecentTagId) {
                    count = MpMeasurements.Instance.TotalVisibleClipTiles + 1;
                } else {
                    count = int.MaxValue;//_pageCount;// //
                }                
            }
            Dictionary<int, int> manualSortOrderLookup = null;

            if(sortColumn == MpContentSortType.Manual) {
                manualSortOrderLookup = new Dictionary<int, int>();
                foreach(var ctvm in Items) {
                    if(manualSortOrderLookup.ContainsKey(ctvm.HeadItem.CopyItemId)) {
                        continue;
                    }
                    manualSortOrderLookup.Add(ctvm.HeadItem.CopyItemId, Items.IndexOf(ctvm));
                }
            }
            IsBusy = true;

            _filterIds = await MpDataModelProvider.Instance.QueryForIds(tagId, sortColumn, isDescending, manualSortOrderLookup);
            
            var page_cil = await MpDataModelProvider.Instance.GetPageAsync(tagId, start, count, sortColumn, isDescending, manualSortOrderLookup);
                                   
            //Items = new ObservableCollection<MpClipTileViewModel>(page_cil.Select(x => CreateClipTileViewModel(x)));
            BindingOperations.EnableCollectionSynchronization(Items, _tileLockObject);

            _remainingItemsCount = _filterIds.Count - page_cil.Count;

            //ResetClipSelection();
            IsBusy = false;
            sw.Stop();
            MpConsole.WriteLine($"Refresh clips took {sw.ElapsedMilliseconds} ms");
        }
        //public void HideVisibleTiles(double ms = 1000) {
        //    double delay = 0;
        //    double curDelay = 0;
        //    foreach (var ctvm in VisibileClipTiles) {
        //        _hiddenTiles.Add(ctvm);
        //        ctvm.FadeOut(Visibility.Hidden, curDelay, ms);
        //        curDelay += delay;
        //    }
        //}

        //public void ShowVisibleTiles(double ms = 1000) {
        //    double delay = 0;
        //    double curDelay = 0;
        //    foreach (var ctvm in _hiddenTiles) {
        //        ctvm.FadeIn(delay, ms);
        //        curDelay += delay;
        //    }
        //    _hiddenTiles.Clear();
        //}

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

        private void AddTileThread(System.Windows.Forms.IDataObject ido) {
            var totalAddSw = new Stopwatch();
            totalAddSw.Start();

            var createItemSw = new Stopwatch();
            createItemSw.Start();
            var newCopyItem = MpCopyItemBuilder.CreateFromClipboard(ido);

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
                    newCopyItem.CompositeParentCopyItemId = _appendModeCopyItem.Id;
                    newCopyItem.CompositeSortOrderIdx = MpCopyItem.GetCompositeChildren(_appendModeCopyItem).Count + 1;
                    newCopyItem.WriteToDatabase();

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

        public void AddItemFromClipboard(object sender, System.Windows.Forms.IDataObject ido) {
            //var workThread = new Thread(new ThreadStart(AddTileThread));
            //workThread.SetApartmentState(ApartmentState.STA);
            //workThread.IsBackground = true;
            //workThread.Start(); 
            //Task.Run(async () => {
            //    await Task.Delay(500);
            //    MpHelpers.Instance.RunOnMainThread(AddTileThread);
            //});
            AddTileThread(ido);
        }


        public MpClipTileViewModel CreateClipTileViewModel(MpCopyItem ci) {
            var nctvm = new MpClipTileViewModel(this,ci);
            nctvm.PropertyChanged += Nctvm_PropertyChanged;
            return nctvm;
        }

        private void Nctvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var ctvm = sender as MpClipTileViewModel;
            switch(e.PropertyName) {
                case nameof(ctvm.IsSelected):
                    //if(ctvm.IsSelected && 
                    //   !MpShortcutCollectionViewModel.Instance.IsMultiSelectKeyDown &&
                    //   !IgnoreSelectionReset) {
                    //    //ignoreSelectionReset is set in refreshclips and drop behavior (probably others)
                    //    foreach(var octvm in SelectedItems) {
                    //        if(octvm != ctvm) {
                    //            octvm.ClearClipSelection();
                    //            if(octvm.HeadItem != null) {
                    //                MpConsole.WriteLine($"Tile with Head Item {octvm.HeadItem.CopyItemTitle} was canceled selection by {ctvm.HeadItem.CopyItemTitle}");
                    //            }                                
                    //        }
                    //    }
                    //} else if(!ctvm.IsSelected) {
                    //    if(ctvm.IsFlipped) {
                    //        FlipTileCommand.Execute(ctvm);
                    //    }
                    //}
                    break;
            }
        }

        public async Task<IDataObject> GetDataObjectFromSelectedClips(bool isDragDrop = false, bool isToExternalApp = false) {
            IDataObject d = new DataObject();

            //selection (if all subitems are dragging select host if no subitems are selected select all)
            foreach (var sctvm in SelectedItems) {
                //if (sctvm.SelectedItems.Count == sctvm.Count ||
                //    sctvm.Count <= 1) {
                //    sctvm.IsClipDragging = true;
                //}
                if (sctvm.SelectedItems.Count == 0) {
                    sctvm.SubSelectAll();
                }
            }

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

        public void PasteDataObject(IDataObject pasteDataObject, bool fromHotKey = false) {
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
                        //Items.Move(Items.IndexOf(sctvm), 0);

                        var a = MpApp.GetAppByPath(MpHelpers.Instance.GetProcessPath(MpClipboardManager.Instance.LastWindowWatcher.LastHandle));
                        var aid = a == null ? 0 : a.Id;
                        foreach(var ivm in sctvm.ItemViewModels) {
                            new MpPasteHistory() {
                                AppId = aid,
                                CopyItemId = ivm.CopyItem.Id,
                                UserDeviceId = MpUserDevice.GetUserDeviceByGuid(MpPreferences.Instance.ThisDeviceGuid).Id,
                                PasteDateTime = DateTime.Now
                            }.WriteToDatabase();
                        }
                    }
                    //Refresh();
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
                    sctvm.ItemVisibility = Visibility.Visible;
                    sctvm.TemplateRichText = string.Empty;
                    sctvm.ClearEditing();
                    foreach (var rtbvm in sctvm.ItemViewModels) {
                        rtbvm.ItemVisibility = Visibility.Visible;
                        rtbvm.TemplateCollection.ResetAll();
                        rtbvm.TemplateRichText = string.Empty;
                        rtbvm.RequestUiReset();
                        //rtbvm.Rtb.ScrollToHorizontalOffset(0);
                        //rtbvm.Rtb.ScrollToVerticalOffset(0);
                        //rtbvm.UpdateLayout();
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
                Task.Run(async () => {
                    var ctvm = Items.Where(x => x.ItemViewModels.Any(y => y.CopyItemId == ci.Id)).FirstOrDefault();
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
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
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
                            ci.WriteToDatabase();
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
            }), DispatcherPriority.Background);
        }

        #endregion

        #endregion

        #region Private Methods

        #region Sync Events


        #endregion

        #endregion

        #region Commands

        public ICommand FlipTileCommand => new RelayCommand<object>(
            (tileToFlip) => {
                var ctvm = tileToFlip as MpClipTileViewModel;
                if(ctvm.IsFlipped) {
                    ClearClipSelection();
                    ctvm.IsSelected = true;
                    ctvm.IsFlipping = true;
                } else {
                    UnFlipAllTiles();
                    ClearClipSelection();
                    ctvm.IsSelected = true;
                    ctvm.IsFlipping = true;
                }
            },
            (tileToFlip) => {
                return tileToFlip != null;
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

        private AsyncRelayCommand<object> _hotkeyPasteCommand;
        public ICommand PerformHotkeyPasteCommand {
            get {
                if (_hotkeyPasteCommand == null) {
                    _hotkeyPasteCommand = new AsyncRelayCommand<object>(HotkeyPaste, CanHotkeyPaste);
                }
                return _hotkeyPasteCommand;
            }
        }
        private bool CanHotkeyPaste(object args) {
            return !MpMainWindowViewModel.IsMainWindowOpen;
        }
        private async Task HotkeyPaste(object args) {
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
                PasteDataObject(pasteDataObject);
                ResetClipSelection();
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
        private bool CanPasteSelectedClips(object args) {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsAnyTileExpanded &&
                !IsAnyEditingClipTile &&
                !IsAnyEditingClipTitle &&
                !IsAnyPastingTemplate &&
                !MpPreferences.Instance.IsTrialExpired;
        }
        private void PasteSelectedClips(object args) {
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
        }

        private AsyncRelayCommand _bringSelectedClipTilesToFrontCommand;
        public ICommand BringSelectedClipTilesToFrontCommand {
            get {
                if (_bringSelectedClipTilesToFrontCommand == null) {
                    _bringSelectedClipTilesToFrontCommand = new AsyncRelayCommand(BringSelectedClipTilesToFront, CanBringSelectedClipTilesToFront);
                }
                return _bringSelectedClipTilesToFrontCommand;
            }
        }
        private bool CanBringSelectedClipTilesToFront() {
            if (IsBusy ||
                MpMainWindowViewModel.IsMainWindowLoading ||
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
        }
        private async Task BringSelectedClipTilesToFront() {
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
        }

        private AsyncRelayCommand _sendSelectedClipTilesToBackCommand;
        public ICommand SendSelectedClipTilesToBackCommand {
            get {
                if (_sendSelectedClipTilesToBackCommand == null) {
                    _sendSelectedClipTilesToBackCommand = new AsyncRelayCommand(SendSelectedClipTilesToBack, CanSendSelectedClipTilesToBack);
                }
                return _sendSelectedClipTilesToBackCommand;
            }
        }
        private bool CanSendSelectedClipTilesToBack() {
            if (IsBusy ||
                MpMainWindowViewModel.IsMainWindowLoading ||
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
        }
        private async Task SendSelectedClipTilesToBack() {
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
        }

        public ICommand DeleteSelectedClipsCommand => new AsyncRelayCommand(
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
            if (tagToLink == null || SelectedItems == null || SelectedItems.Count == 0) {
                return false;
            }
            if (SelectedItems.Count == 1) {
                return true;
            }
            bool isLastClipTileLinked = tagToLink.IsLinked(SelectedItems[0]);
            foreach (var selectedClipTile in SelectedItems) {
                if (tagToLink.IsLinked(selectedClipTile) != isLastClipTileLinked) {
                    return false;
                }
            }
            return true;
        }
        private void LinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            bool isUnlink = tagToLink.IsLinked(SelectedItems[0]);
            foreach (var selectedClipTile in SelectedItems) {
                foreach(var ivm in selectedClipTile.ItemViewModels) {
                    if (isUnlink) {
                        tagToLink.RemoveClip(ivm);
                    } else {
                        tagToLink.AddClip(ivm);
                    }
                }
            }
            MpMainWindowViewModel.Instance.TagTrayViewModel.RefreshAllCounts();
            MpMainWindowViewModel.Instance.TagTrayViewModel.UpdateTagAssociation();
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
            return SelectedItems.Count == 1;
        }
        private void AssignHotkey() {
            SelectedItems[0].AssignHotkeyCommand.Execute(null);
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

        private RelayCommand _editSelectedTitleCommand;
        public ICommand EditSelectedTitleCommand {
            get {
                if (_editSelectedTitleCommand == null) {
                    _editSelectedTitleCommand = new RelayCommand(EditSelectedTitle, CanEditSelectedTitle);
                }
                return _editSelectedTitleCommand;
            }
        }
        private bool CanEditSelectedTitle() {
            if (MpMainWindowViewModel.IsMainWindowLoading) {
                return false;
            }
            return SelectedItems.Count == 1 &&
                  SelectedItems[0].SelectedItems.Count <= 1;
        }
        private void EditSelectedTitle() {
            SelectedItems[0].EditTitleCommand.Execute(null);
        }

        public ICommand EditSelectedContentCommand => new RelayCommand(
            () => {
                SelectedItems[0].IsExpanded = true;
            },
            () => {
                if (MpMainWindowViewModel.IsMainWindowLoading) {
                    return false;
                }
                return SelectedItems.Count == 1 && SelectedItems[0].SelectedItems.Count == 1;
            });

        private RelayCommand _sendSelectedClipsToEmailCommand;
        public ICommand SendSelectedClipsToEmailCommand {
            get {
                if (_sendSelectedClipsToEmailCommand == null) {
                    _sendSelectedClipsToEmailCommand = new RelayCommand(SendSelectedClipsToEmail, CanSendSelectedClipsToEmail);
                }
                return _sendSelectedClipsToEmailCommand;
            }
        }
        private bool CanSendSelectedClipsToEmail() {
            return !IsAnyEditingClipTile && SelectedItems.Count > 0;
        }
        private void SendSelectedClipsToEmail() {
            MpHelpers.Instance.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, SelectedItems[0].HeadItem.CopyItem.Title, SelectedClipTilesMergedPlainText));
            //MpClipTrayViewModel.Instance.ClearClipSelection();
            //IsSelected = true;
            //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        }



        private AsyncRelayCommand _mergeSelectedClipsCommand;
        public ICommand MergeSelectedClipsCommand {
            get {
                if (_mergeSelectedClipsCommand == null) {
                    _mergeSelectedClipsCommand = new AsyncRelayCommand(MergeSelectedClips, CanMergeSelectedClips);
                }
                return _mergeSelectedClipsCommand;
            }
        }
        private bool CanMergeSelectedClips() {
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
        }
        private async Task MergeSelectedClips() {
            await Task.Delay(1);
        }

        private AsyncRelayCommand<string> _translateSelectedClipTextAsyncCommand;
        public ICommand TranslateSelectedClipTextAsyncCommand {
            get {
                if (_translateSelectedClipTextAsyncCommand == null) {
                    _translateSelectedClipTextAsyncCommand = new AsyncRelayCommand<string>(TranslateSelectedClipTextAsync, CanTranslateSelectedClipText);
                }
                return _translateSelectedClipTextAsyncCommand;
            }
        }
        private bool CanTranslateSelectedClipText(object args) {
            return SelectedItems.Count == 1 && SelectedItems[0].IsTextItem;
        }
        private async Task TranslateSelectedClipTextAsync(string toLanguage) {
            var translatedText = await MpLanguageTranslator.Instance.TranslateAsync(SelectedItems[0].HeadItem.CopyItem.ItemData.ToPlainText(), toLanguage, false);
            if (!string.IsNullOrEmpty(translatedText)) {
                SelectedItems[0].HeadItem.CopyItem.ItemData = MpHelpers.Instance.ConvertPlainTextToRichText(translatedText);
            }
        }

        private RelayCommand _createQrCodeFromSelectedClipsCommand;
        public ICommand CreateQrCodeFromSelectedClipsCommand {
            get {
                if (_createQrCodeFromSelectedClipsCommand == null) {
                    _createQrCodeFromSelectedClipsCommand = new RelayCommand(CreateQrCodeFromSelectedClips, CanCreateQrCodeFromSelectedClips);
                }
                return _createQrCodeFromSelectedClipsCommand;
            }
        }
        private bool CanCreateQrCodeFromSelectedClips() {
            return (GetSelectedClipsType() == MpCopyItemType.RichText) &&
                    SelectedClipTilesMergedPlainText.Length <= Properties.Settings.Default.MaxQrCodeCharLength;
        }
        private void CreateQrCodeFromSelectedClips() {
            var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(SelectedClipTilesMergedPlainText);
            MpClipboardManager.Instance.SetImageWrapper(bmpSrc);
        }

        private AsyncRelayCommand _speakSelectedClipsCommand;
        public ICommand SpeakSelectedClipsCommand {
            get {
                if (_speakSelectedClipsCommand == null) {
                    _speakSelectedClipsCommand = new AsyncRelayCommand(SpeakSelectedClipsAsync, CanSpeakSelectedClips);
                }
                return _speakSelectedClipsCommand;
            }
        }
        private bool CanSpeakSelectedClips() {
            return SelectedItems.All(x => x.IsTextItem);
        }
        private async Task SpeakSelectedClipsAsync() {
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
                    foreach(var ivm in sctvm.ItemViewModels) {
                        //speechSynthesizer.SpeakAsync(sctvm.CopyItemPlainText);
                        promptBuilder.AppendText(Environment.NewLine + ivm.CopyItem.ItemData.ToPlainText());
                    }
                }

                // Speak the contents of the prompt asynchronously.
                speechSynthesizer.SpeakAsync(promptBuilder);

            }, DispatcherPriority.Background);
        }

        public ICommand DuplicateSelectedClipsCommand => new AsyncRelayCommand(
            async () => {
                var tempSelectedClipTiles = SelectedItems;
                ClearClipSelection();
                foreach (var sctvm in tempSelectedClipTiles) {
                    foreach (var ivm in sctvm.SelectedItems) {
                        var clonedCopyItem = (MpCopyItem)ivm.CopyItem.Clone();
                        clonedCopyItem.WriteToDatabase();
                        //var ctvm = new MpClipTileViewModel(clonedCopyItem);
                        //MpMainWindowViewModel.Instance.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
                        //this.Add(ctvm);
                        await RefreshTiles();
                        var ctvm = GetContentItemViewModelById(clonedCopyItem.Id);
                        ctvm.IsSelected = true;
                    }

                }
            });


        #region MpIContentCommands 

        public ICommand CopyCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand PasteCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand DeleteCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand BringToFrontCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand ChangeColorCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand CreateQrCodeCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand DuplicateCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand EditTitleCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand ExcludeApplicationCommand {
            get {
                return new RelayCommand<object>((excludeDomain)=> { });
            }
        }

        public ICommand HotkeyPasteCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand LinkTagToContentCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }


        public ICommand MergeCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand SelectNextCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand SelectPreviousCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand SendToEmailCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand SendToBackCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand SpeakCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }

        public ICommand TranslateCommand {
            get {
                return new RelayCommand(()=> { });
            }
        }
        #endregion

        #endregion


    }
    public enum MpExportType {
        None = 0,
        Files,
        Csv,
        Zip
    }
}
