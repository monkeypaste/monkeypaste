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
using static OpenTK.Graphics.OpenGL.GL;


namespace MpWpfApp {
    public class MpClipTrayViewModel : MpSingletonViewModel<MpClipTrayViewModel> {
        #region Private Variables      

        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;

        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private List<MpCopyItem> _newModels = new List<MpCopyItem>();

        private MpCopyItem _appendModeCopyItem = null;

        private int _pageSize = 0;

        private Dictionary<int, int> _manualSortOrderLookup = null;

        #endregion

        #region Properties

        #region View Models

        [MpChildViewModel(typeof(MpClipTileViewModel), true)]
        public ObservableCollection<MpClipTileViewModel> Items { get; set; } = new ObservableCollection<MpClipTileViewModel>();

        [MpAffectsChild]
        public List<MpClipTileViewModel> SelectedItems {
            get {
                return Items.Where(ct => ct.IsSelected).OrderBy(x => x.LastSelectedDateTime).ToList();
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
                if (Items.Count == 0) {
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
                return Items[Items.Count - 1];
            }
        }

        public List<MpContentItemViewModel> SelectedContentItemViewModels {
            get {
                var scivml = new List<MpContentItemViewModel>();
                foreach (var sctvm in SelectedItems) {
                    scivml.AddRange(sctvm.SelectedItems);
                }
                return scivml;
            }
        }

        public List<MpCopyItem> SelectedModels {
            get {
                var cil = new List<MpCopyItem>();
                foreach (var sctvm in SelectedItems) {
                    cil.AddRange(sctvm.SelectedItems.OrderBy(y => y.LastSubSelectedDateTime).Select(x => x.CopyItem));
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

        public double ClipTrayHeight => MpMainWindowViewModel.Instance.MainWindowHeight - MpMeasurements.Instance.TitleMenuHeight - MpMeasurements.Instance.FilterMenuHeight;
                

        //public double ClipTrayScreenHeight => ClipTrayHeight;

        public double ClipTrayScreenWidth {
            get {
                if (IsAnyTileExpanded) {
                    return MpMainWindowViewModel.Instance.MainWindowWidth;
                }
                return MpMeasurements.Instance.ClipTrayDefaultWidth;
            }
        }

        public double ClipTrayTotalTileWidth {
            get {
                int totalTileCount = TotalTilesInQuery;
                int uniqueWidthTileCount = PersistentUniqueWidthTileLookup.Count;
                int defaultWidthTileCount = totalTileCount - uniqueWidthTileCount;

                double defaultWidth = MpMeasurements.Instance.ClipTileMinSize;

                double totalUniqueWidth = PersistentUniqueWidthTileLookup.Sum(x => x.Value);
                double totalTileWidth = totalUniqueWidth + (defaultWidthTileCount * defaultWidth);

                return totalTileWidth;
            }
        }

        public double ClipTrayTotalWidth => Math.Max(ClipTrayScreenWidth, ClipTrayTotalTileWidth);

        public double MaximumScrollOfset {
            get {
                if (TotalTilesInQuery > MpMeasurements.Instance.TotalVisibleClipTiles) {
                    return ClipTrayTotalWidth - (MpMeasurements.Instance.TotalVisibleClipTiles * MpMeasurements.Instance.ClipTileMinSize);
                }
                return 0;
            }
        }
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

        public int RemainingItemsOnRight { get; set; }

        public int RemainingItemsOnLeft { get; set; }

        public int RemainingItemsCountThreshold { get; private set; }

        public int TotalTilesInQuery => MpDataModelProvider.Instance.TotalItems;

        public int DefaultLoadCount { get; private set; } = 0;

        public SelectionMode SelectionMode => SelectionMode.Single;

        #endregion

        #region State

        #region Virtual

        //set in civm IsSelected property change, DragDrop.Drop (copy mode)
        public List<MpCopyItem> PersistentSelectedModels { get; set; } = new List<MpCopyItem>();

        //<HeadCopyItemId, Unique ItemWidth> unique is != to MpMeausrements.Instance.ClipTileMinSize
        public Dictionary<int, double> PersistentUniqueWidthTileLookup { get; set; } = new Dictionary<int, double>();
        #endregion

        public bool HasScrollVelocity { get; set; }

        public bool IsRestoringSelection { get; private set; } = false;        

        public double LastScrollOfset { get; set; } = 0;

        private double _scrollOfset = 0;
        public double ScrollOffset {
            get { 
                return _scrollOfset;
            }
            set {
                if (_scrollOfset != value) {
                    LastScrollOfset = _scrollOfset;
                    _scrollOfset = value;
                    OnPropertyChanged(nameof(ScrollOffset));
                }
            }
        }

        public bool IsHorizontalScrollBarVisible {
            get {
                if (IsAnyTileExpanded) {
                    return false;
                }
                return TotalTilesInQuery > MpMeasurements.Instance.TotalVisibleClipTiles;
            }
        }

        public int HeadQueryIdx {
            get {
                if (Items.Count == 0) {
                    return -1;
                }
                return Items[0].QueryOffsetIdx;
            }
        }

        public int TailQueryIdx {
            get {
                if (Items.Count == 0) {
                    return -1;
                }
                return Items[Items.Count - 1].QueryOffsetIdx;
            }
        }

        //public bool IsLastItemVisible {
        //    get {
        //        return NextQueryOffsetIdx >= TotalItemsInQuery;
        //    }
        //}

        //public bool IsFirstItemVisible {
        //    get {
        //        return LastQueryOffsetIdx == 0;
        //    }
        //}

        public bool IsLoadingMore { get; set; } = false;

        public bool IsScrollJumping { get; set; } = false;

        public bool IsAnyBusy => Items.Any(x => x.IsAnyBusy) || IsBusy;

        public bool IsRequery { get; private set; } = false;

        public bool IsTrayEmpty => Items.Count == 0 && !IsRequery && !MpMainWindowViewModel.Instance.IsMainWindowLoading;// || Items.All(x => x.IsPlaceholder);

        public bool IsSelectionReset { get; set; } = false;

        public bool IgnoreSelectionReset { get; set; } = false;

        public bool IsPastingHotKey { get; set; } = false;

        public bool IsPastingSelected { get; set; } = false;

        public bool IsAnyTileContextMenuOpened => Items.Any(x => x.IsAnyItemContextMenuOpened);


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

        public MpClipTrayViewModel() : base() { }

        public async Task Init() {
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                PropertyChanged += MpClipTrayViewModel_PropertyChanged;
                Items.CollectionChanged += Items_CollectionChanged;
                MpDataModelProvider.Instance.AllFetchedAndSortedCopyItemIds.CollectionChanged += AllFetchedAndSortedCopyItemIds_CollectionChanged;
                MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
                MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
                MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;

                _pageSize = 1;
                RemainingItemsCountThreshold = 1;

                DefaultLoadCount = MpMeasurements.Instance.TotalVisibleClipTiles * 1 + 2;

                MpMessenger.Instance.Register<MpMessageType>(
                    MpDataModelProvider.Instance.QueryInfo, ReceivedQueryInfoMessage);

            });
        }

        private void AllFetchedAndSortedCopyItemIds_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if(IsRequery || IsLoadingMore || IsScrollJumping) {
                return;
            }
            switch(e.Action) {
                case NotifyCollectionChangedAction.Remove:
                    if(e.OldItems != null) {
                        foreach(int removedQueryItemId in e.OldItems) {

                        }
                    }
                    break;
            }
        }

        #endregion

        #region Public Methods

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsTrayEmpty));
            //return;
            if (IsAnyTileExpanded) {
                return;
            }
            if(e.OldItems != null) { //if (e.Action == NotifyCollectionChangedAction.Move && IsLoadingMore) {
                foreach (MpClipTileViewModel octvm in e.OldItems) {
                    octvm.Dispose();
                }
            }
        }

        private void ReceivedQueryInfoMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.QueryChanged:
                    IsRequery = true;
                    RequeryCommand.Execute(null);
                    break;
                case MpMessageType.SubQueryChanged:
                    IsRequery = true;
                    RequeryCommand.Execute(HeadQueryIdx);
                    break;
            }
        }

        private void MpClipTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasScrollVelocity):
                    if (!HasScrollVelocity) {
                        var hctvm = Items.FirstOrDefault(x => x.IsHovering);
                        if (hctvm != null) {
                            hctvm.OnPropertyChanged(nameof(hctvm.TileBorderBrush));
                        }
                    }
                    break;
                case nameof(ScrollOffset):
                    foreach (MpClipTileViewModel nctvm in Items) {
                        nctvm.OnPropertyChanged(nameof(nctvm.TrayX));
                    }
                    MpMessenger.Instance.Send<MpMessageType>(MpMessageType.TrayScrollChanged);
                    break;
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
                foreach (var ctvm in Items) {
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
                if (ctvm == null) {
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

                PersistentSelectedModels.Clear();
            }));
        }

        public void ResetClipSelection(bool clearEditing = true) {
            IsSelectionReset = true;
            MpHelpers.Instance.RunOnMainThread(() => {
                ClearClipSelection(clearEditing);

                if (Items.Count > 0 && Items[0] != null) {

                    Items[0].IsSelected = true;
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
            foreach (var ctvm in Items) {
                if (ctvm.IsFlipped) {
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
                if (_appendModeCopyItem == null) {
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

        public async Task<MpClipTileViewModel> CreateClipTileViewModel(MpCopyItem ci) {
            var nctvm = new MpClipTileViewModel(this);
            await nctvm.InitializeAsync(ci);
            return nctvm;
        }

        public async Task<IDataObject> GetDataObjectFromSelectedClips(bool isDragDrop = false, bool isToExternalApp = false) {
            IDataObject d = new DataObject();

            //selection (if all subitems are dragging select host if no subitems are selected select all)
            List<MpCopyItem> selectedModels = new List<MpCopyItem>();
            if (SelectedItems.Count == 0) {
                selectedModels = PersistentSelectedModels;
            } else {
                SelectedItems.ForEach(x => x.DoCommandSelection());
                foreach (var sctvm in SelectedItems) {
                    foreach (var scivm in sctvm.SelectedItems) {
                        selectedModels.Add(scivm.CopyItem);
                    }
                }
            }
            if (selectedModels.Count == 0) {
                return d;
            }

            selectedModels.Reverse();

            string rtf = string.Empty.ToRichText();
            string pt = string.Empty;

            if (isToExternalApp) {
                //gather rtf and text NOT setdata it needs file drop first
                foreach (var sctvm in SelectedItems) {
                    string sctrtf = await sctvm.GetSubSelectedPastableRichText(isToExternalApp);
                    rtf = MpHelpers.Instance.CombineRichText(sctrtf, rtf);
                }
                pt = rtf.ToPlainText();
            }

            //set file drop (always must set so when dragged out of application user doesn't get no-drop cursor)
            if (MpExternalDropBehavior.Instance.IsProcessNeedFileDrop(MpRunningApplicationManager.Instance.ActiveProcessPath) &&
                isDragDrop) {
                //only when pasting into explorer or notepad must have file drop
                var sctfl = new List<string>();
                if (selectedModels.All(x => x.ItemType != MpCopyItemType.FileList) &&
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                    //external drop w/ ctrl down merges all selected items (unless file list)
                    // TODO maybe for multiple files w/ ctrl down compress into zip?
                    if (MpExternalDropBehavior.Instance.IsProcessLikeNotepad(MpRunningApplicationManager.Instance.ActiveProcessPath)) {
                        //merge as plain text
                        string fp = MpHelpers.Instance.GetUniqueFileName(MpExternalDropFileType.Txt, selectedModels[0].Title);
                        sctfl.Add(MpHelpers.Instance.WriteTextToFile(fp, pt, true));
                    } else {
                        //merge as rich text
                        string fp = MpHelpers.Instance.GetUniqueFileName(MpExternalDropFileType.Rtf, selectedModels[0].Title);
                        sctfl.Add(MpHelpers.Instance.WriteTextToFile(fp, rtf, true));
                    }
                } else {
                    foreach (var sci in selectedModels) {
                        if (sci.ItemType == MpCopyItemType.FileList) {
                            sctfl.AddRange(sci.ItemData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        } else if (sci.ItemType == MpCopyItemType.Image) {
                            string fp = MpHelpers.Instance.GetUniqueFileName(MpExternalDropFileType.Png, sci.Title);
                            sctfl.Add(MpHelpers.Instance.WriteBitmapSourceToFile(fp, sci.ItemData.ToBitmapSource(), true));
                        } else if (MpExternalDropBehavior.Instance.IsProcessLikeNotepad(MpRunningApplicationManager.Instance.ActiveProcessPath)) {
                            string fp = MpHelpers.Instance.GetUniqueFileName(MpExternalDropFileType.Txt, sci.Title);
                            sctfl.Add(MpHelpers.Instance.WriteTextToFile(fp, sci.ItemData.ToPlainText(), true));
                        } else {
                            string fp = MpHelpers.Instance.GetUniqueFileName(MpExternalDropFileType.Rtf, sci.Title);
                            sctfl.Add(MpHelpers.Instance.WriteTextToFile(fp, sci.ItemData.ToRichText(), true));
                        }
                    }
                }

                (d as DataObject).SetFileDropList(sctfl.ToStringCollection());
                // d.SetData(DataFormats.FileDrop, sctfl.ToStringCollection());
            }

            if (isToExternalApp) {
                //set rtf and text
                if (!string.IsNullOrEmpty(rtf)) {
                    d.SetData(DataFormats.Rtf, rtf);
                }
                if (!string.IsNullOrEmpty(pt)) {
                    d.SetData(DataFormats.Text, rtf.ToPlainText());
                }
                //set image
                if (SelectedItems.Count == 1 && SelectedItems[0].HeadItem.CopyItem.ItemType == MpCopyItemType.Image) {
                    d.SetData(DataFormats.Bitmap, SelectedItems[0].HeadItem.CopyItem.ItemData.ToBitmapSource());
                }

                //set csv
                string sctcsv = string.Join(Environment.NewLine, selectedModels.Select(x => x.ItemData.ToCsv()));
                if (!string.IsNullOrWhiteSpace(sctcsv)) {
                    d.SetData(DataFormats.CommaSeparatedValue, sctcsv);
                }

                //update metrics
                selectedModels.ForEach(x => x.PasteCount++);
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
            //    //d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedItems.ToList());
            //}

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
                        foreach (var ivm in sctvm.ItemViewModels) {
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
                if (ctvm.ItemViewModels.Any(x => x.CopyItem.Source.AppId == appId)) {
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

        public MpClipTileViewModel GetClipTileViewModelById(int ciid) {
            var civm = GetContentItemViewModelById(ciid);
            if(civm == null) {
                return null;
            }
            return civm.Parent;
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

        public void StoreSelectionState(MpClipTileViewModel tile) {
            if (!tile.IsSelected) {
                return;
            }

            if (SelectionMode == SelectionMode.Single) {
                PersistentSelectedModels = tile.SelectedItems.Select(x => x.CopyItem).ToList();
            } else {
                PersistentSelectedModels.AddRange(tile.SelectedItems.Select(x => x.CopyItem).ToList());
            }
        }

        public void RestoreSelectionState(MpClipTileViewModel tile) {
            var prevSelectedItems = tile.ItemViewModels.Where(x =>
                                                PersistentSelectedModels.Any(y =>
                                                    y.Id == x.CopyItemId)).ToList();
            if (prevSelectedItems.Count == 0) {
                tile.ClearSelection();
                return;
            }

            IsRestoringSelection = true;

            tile.ItemViewModels.ForEach(x => x.IsSelected = PersistentSelectedModels.Any(y=>y.Id == x.CopyItemId));

            IsRestoringSelection = false;
        }
        #endregion

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
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
                MpHelpers.Instance.RunOnMainThread((Action)(async () => {
                    var civm = GetContentItemViewModelById(ci.Id);
                    if (civm == null ||
                        civm.Parent == null) {
                        return;
                    }
                    var ctvm = civm.Parent;
                    int tileIdx = Items.IndexOf(ctvm);
                    int itemIdx = ctvm.ItemViewModels.IndexOf(civm);
                    civm.IsSelected = false;
                    if (ctvm.ItemViewModels.Count == 1) {
                        //when removing multiple items wait until the last one to reset selection, prefering next item
                        ctvm.IsSelected = false;
                        int newSelectedIdx = tileIdx + 1;
                        if (newSelectedIdx >= Items.Count) {
                            //this was the last item 
                            if (Items.Count == 1) {
                                //there are no other items to select
                                newSelectedIdx = -1;
                            } else {
                                //select previous item
                                newSelectedIdx = tileIdx - 1;
                            }
                        }
                        if (newSelectedIdx >= 0) {
                            Items[newSelectedIdx].IsSelected = true;
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
                        Items.Move(tileIdx, (int)(Items.Count - 1));
                        await ctvm.InitializeAsync(null);
                    }

                    MpDataModelProvider.Instance.QueryInfo.NotifyQueryChanged(false);
                }));
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
            MpHelpers.Instance.RunOnMainThread(async () => {
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

        public ICommand AddNewItemsCommand => new RelayCommand<bool?>(
            async (addToCurrentTag) => {
                IsBusy = true;

                if (addToCurrentTag != null &&
                   addToCurrentTag.Value == true &&
                   MpDataModelProvider.Instance.QueryInfo.TagId != MpTag.AllTagId) {
                    foreach (var nci in _newModels) {
                        await MpCopyItemTag.Create(
                                MpDataModelProvider.Instance.QueryInfo.TagId,
                                nci.Id
                            );
                    }
                }

                RequeryCommand.Execute(null);

                _newModels.Clear();
                await MpTagTrayViewModel.Instance.RefreshAllCounts();
            },
            (addToCurrentTag) => {
                if (_newModels.Count == 0) {
                    return false;
                }
                if (MpDataModelProvider.Instance.QueryInfo.SortType == MpContentSortType.Manual) {
                    return false;
                }
                if (MpDataModelProvider.Instance.QueryInfo.TagId == MpTag.AllTagId) {
                    return true;
                }
                if (addToCurrentTag == null || ((bool)addToCurrentTag) == false) {
                    return false;
                }
                return true;
            });

        public ICommand RequeryCommand => new RelayCommand<object>(
            async (offsetIdxArg) => {
                var sw = new Stopwatch();
                sw.Start();
                bool isDragDropRequery = offsetIdxArg != null;
                int offsetIdx = offsetIdxArg == null ? 0 : (int)offsetIdxArg;

                IsBusy = true;

                if (offsetIdxArg == null) {
                    ScrollOffset = LastScrollOfset = 0;

                    MpDataModelProvider.Instance.ResetQuery();

                    await MpDataModelProvider.Instance.QueryForTotalCount();
                }
                OnPropertyChanged(nameof(TotalTilesInQuery));
                OnPropertyChanged(nameof(ClipTrayTotalWidth));
                OnPropertyChanged(nameof(MaximumScrollOfset));
                OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));

                int loadCount = Math.Min(DefaultLoadCount, TotalTilesInQuery);

                // Cleanup Tray item count depending on last query
                int itemCountDiff = Items.Count - loadCount;
                if (itemCountDiff > 0) {
                    while (itemCountDiff > 0) {
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

                if (loadCount > 0) {
                    var cil = await MpDataModelProvider.Instance.FetchCopyItemRangeAsync(offsetIdx, loadCount);

                    for (int i = 0; i < cil.Count; i++) {
                        await Items[i].InitializeAsync(cil[i], i + offsetIdx);

                        if (isDragDropRequery) {
                            RestoreSelectionState(Items[i]);
                        }
                    }
                }

                if (isDragDropRequery) {

                } else {
                    ResetClipSelection();
                }

                IsBusy = false;
                IsRequery = false;

                MpMessenger.Instance.Send<MpMessageType>(MpMessageType.RequeryCompleted);

                sw.Stop();
                MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);

            });

        public ICommand LoadMoreClipsCommand => new RelayCommand<object>(
            async (isLoadMore) => {
                IsBusy = IsLoadingMore = true;

                if (IsAnyTileFlipped) {
                    UnFlipAllTiles();
                }

                bool isLeft = ((int)isLoadMore) >= 0;
                if (isLeft && TailQueryIdx < TotalTilesInQuery - 1) {
                    int offsetIdx = TailQueryIdx + 1;
                    int fetchCount = _pageSize;
                    if (offsetIdx + fetchCount >= TotalTilesInQuery) {
                        fetchCount = TotalTilesInQuery - offsetIdx;
                    }
                    var cil = await MpDataModelProvider.Instance.FetchCopyItemRangeAsync(offsetIdx, fetchCount);

                    for (int i = 0; i < cil.Count; i++) {
                        if (Items[0].IsSelected) {
                            StoreSelectionState(Items[0]);
                            Items[0].ClearSelection();
                        }
                        Items.Move(0, Items.Count - 1);
                        await Items[Items.Count - 1].InitializeAsync(cil[i], offsetIdx++);
                        RestoreSelectionState(Items[Items.Count - 1]);
                    }
                } else if (!isLeft && HeadQueryIdx > 0) {
                    int fetchCount = _pageSize;

                    if (HeadQueryIdx - fetchCount < 0) {
                        fetchCount = HeadQueryIdx;
                    }
                    int offsetIdx = HeadQueryIdx - fetchCount;
                    var cil = await MpDataModelProvider.Instance.FetchCopyItemRangeAsync(offsetIdx, fetchCount);

                    for (int i = cil.Count - 1; i >= 0; i--) {
                        if (Items[Items.Count - 1].IsSelected) {
                            StoreSelectionState(Items[Items.Count - 1]);
                            Items[Items.Count - 1].ClearSelection();
                        }
                        Items.Move(Items.Count - 1, 0);
                        await Items[0].InitializeAsync(cil[i], offsetIdx + i);
                        RestoreSelectionState(Items[0]);
                    }
                }

                IsBusy = IsLoadingMore = false;
            },
            (itemsToLoad) => {
                return itemsToLoad != null &&
                       !IsBusy &&
                       !IsLoadingMore &&
                       !IsScrollJumping &&
                       !IsAnyTileExpanded &&
                       !MpMainWindowViewModel.Instance.IsMainWindowLoading;
            });

        public ICommand JumpToQueryIdxCommand => new RelayCommand<int>(
            async (idx) => {
                if (idx < TailQueryIdx && idx > HeadQueryIdx) {
                    MpMessenger.Instance.Send<MpMessageType>(MpMessageType.JumpToIdxCompleted);
                    return;
                }

                IsBusy = true;
                IsScrollJumping = true;


                int loadCount = DefaultLoadCount;
                if (idx + loadCount > TotalTilesInQuery) {
                    //loadCount = MpMeasurements.Instance.TotalVisibleClipTiles;
                    idx = TotalTilesInQuery - loadCount;
                }
                ScrollOffset = LastScrollOfset = MpPagingListBoxBehavior.Instance.FindTileOffsetX(idx);

                var cil = await MpDataModelProvider.Instance.FetchCopyItemRangeAsync(idx, loadCount);

                for (int i = 0; i < cil.Count; i++) {
                    if (Items[i].IsSelected) {
                        StoreSelectionState(Items[i]);
                        Items[i].ClearSelection();
                    }
                    await Items[i].InitializeAsync(cil[i], idx + i);
                    RestoreSelectionState(Items[i]);
                }

                MpMessenger.Instance.Send<MpMessageType>(MpMessageType.JumpToIdxCompleted);

                IsScrollJumping = false;
                IsBusy = false;
            },
            (idx) => {
                return idx >= 0 && idx <= TotalTilesInQuery;
            });

        public ICommand FlipTileCommand => new RelayCommand<object>(
            async (tileToFlip) => {
                var ctvm = tileToFlip as MpClipTileViewModel;
                ctvm.IsBusy = true;
                if (ctvm.IsFlipped) {
                    //ClearClipSelection();
                    //ctvm.IsSelected = true;
                    ctvm.IsFlipping = true;
                    while (ctvm.IsFlipping) {
                        await Task.Delay(100);
                    }
                } else {
                    var flippedCtvm = Items.FirstOrDefault(x => x.IsFlipped);
                    if (flippedCtvm != null) {
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
            string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
            MpHelpers.Instance.OpenUrl(args.ToString() + System.Uri.EscapeDataString(pt));
        }

        public ICommand ScrollToHomeCommand => new RelayCommand(
             () => {
                RequeryCommand.Execute(null);
                //JumpToPageIdxCommand.Execute(0);
                //await Task.Delay(100);
                //while (IsScrollJumping) { await Task.Delay(10); }
                //ScrollOffset = LastScrollOfset = 0;
                //Items[0].IsSelected = true;
            });

        public ICommand ScrollToEndCommand => new RelayCommand(
            async () => {
                JumpToQueryIdxCommand.Execute(TotalTilesInQuery - 1);
                await Task.Delay(100);
                while (IsScrollJumping) { await Task.Delay(10); }
                ScrollOffset = LastScrollOfset = ClipTrayTotalWidth;
                Items[Items.Count - 1].IsSelected = true;
            });

        public ICommand SelectNextItemCommand => new RelayCommand(
            async () => {
                bool needJump = false;
                int curRightMostSelectQueryIdx = -1;
                int nextSelectQueryIdx = -1;
                if (SelectedItems.Count > 0) {
                    curRightMostSelectQueryIdx = SelectedItems.Max(x => x.QueryOffsetIdx);
                    nextSelectQueryIdx = curRightMostSelectQueryIdx + 1;
                } else if (PersistentSelectedModels.Count > 0) {
                    needJump = true;
                    curRightMostSelectQueryIdx = PersistentSelectedModels.
                        Select(x => 
                            MpDataModelProvider.Instance.AllFetchedAndSortedCopyItemIds.IndexOf(x.Id))
                                .Max();
                    nextSelectQueryIdx = curRightMostSelectQueryIdx + 1;
                } else {
                    nextSelectQueryIdx = 0;
                }

                if (nextSelectQueryIdx < TotalTilesInQuery) {
                    if (needJump) {
                        JumpToQueryIdxCommand.Execute(Math.Max(0,nextSelectQueryIdx - 3));
                        while (IsBusy) { await Task.Delay(10); }
                    }

                    if (TailQueryIdx - curRightMostSelectQueryIdx <= RemainingItemsCountThreshold + 1) {
                        double nextOffset = MpPagingListBoxBehavior.Instance.FindTileOffsetX(nextSelectQueryIdx);
                        double curOffset =
                            curRightMostSelectQueryIdx >= 0 ?
                                 MpPagingListBoxBehavior.Instance.FindTileOffsetX(curRightMostSelectQueryIdx) : 0;
                        double offsetDiff = nextOffset - curOffset;

                        //adding 10 to ensure loadmore
                        ScrollOffset += offsetDiff + 10;
                        //triggers load more...
                        await Task.Delay(100);
                        while (IsBusy) { await Task.Delay(10); }
                    }                    
                }
                
                if (nextSelectQueryIdx < TotalTilesInQuery) {
                    int curItemIdx = curRightMostSelectQueryIdx < 0 ? -1 : Items.IndexOf(
                        Items.FirstOrDefault(x => x.QueryOffsetIdx == curRightMostSelectQueryIdx));
                    int nextItemIdx = curItemIdx + 1;

                    ClearClipSelection();
                    Items[nextItemIdx].ResetSubSelection();
                    StoreSelectionState(Items[nextItemIdx]);
                }
            });

        public ICommand SelectPreviousItemCommand => new RelayCommand(
            async () => {
                bool needJump = false;
                int curLeftMostSelectQueryIdx = -1;
                int prevSelectQueryIdx = -1;
                if (SelectedItems.Count > 0) {
                    curLeftMostSelectQueryIdx = SelectedItems.Min(x => x.QueryOffsetIdx);
                    prevSelectQueryIdx = curLeftMostSelectQueryIdx - 1;
                } else if (PersistentSelectedModels.Count > 0) {
                    needJump = true;
                    curLeftMostSelectQueryIdx = PersistentSelectedModels.
                        Select(x =>
                            MpDataModelProvider.Instance.AllFetchedAndSortedCopyItemIds.IndexOf(x.Id))
                                .Min();
                    prevSelectQueryIdx = curLeftMostSelectQueryIdx - 1;
                } else {
                    prevSelectQueryIdx = TotalTilesInQuery - 1;
                }

                if (prevSelectQueryIdx >= 0) {
                    if (needJump) {
                        JumpToQueryIdxCommand.Execute(Math.Max(0, prevSelectQueryIdx - 3));
                        while (IsBusy) { await Task.Delay(10); }
                    }

                    if ((HeadQueryIdx == 0 && Math.Abs(ScrollOffset) > 0.01) ||
                        (curLeftMostSelectQueryIdx - HeadQueryIdx <= 1)) {
                        double prevOffset = MpPagingListBoxBehavior.Instance.FindTileOffsetX(prevSelectQueryIdx);
                        double curOffset =
                            curLeftMostSelectQueryIdx >= 0 ?
                                 MpPagingListBoxBehavior.Instance.FindTileOffsetX(curLeftMostSelectQueryIdx) : 0;
                        double offsetDiff = prevOffset - curOffset;

                        ScrollOffset = Math.Max(0,ScrollOffset + offsetDiff);
                        //triggers load more...
                        await Task.Delay(100);
                        while (IsBusy) { await Task.Delay(10); }
                    }
                }

                if (prevSelectQueryIdx >= 0) {
                    int curItemIdx = curLeftMostSelectQueryIdx < 0 ? 1 : Items.IndexOf(
                        Items.FirstOrDefault(x => x.QueryOffsetIdx == curLeftMostSelectQueryIdx));
                    int prevItemIdx = curItemIdx - 1;

                    ClearClipSelection();
                    Items[prevItemIdx].ResetSubSelection();
                    StoreSelectionState(Items[prevItemIdx]);
                }
            });

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
            foreach (var ctvm in Items) {
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

                if (MpMainWindowViewModel.Instance.IsMainWindowOpen) {
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
                return !MpMainWindowViewModel.Instance.IsMainWindowOpen;
            });

        public ICommand CopySelectedClipsCommand => new RelayCommand(
            async () => {
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
            async () => {
                IsBusy = true;
                var sil = PrimaryItem.SelectedItems;
                sil.Reverse();
                foreach (var scivm in sil) {
                    PrimaryItem.ItemViewModels.Move(PrimaryItem.ItemViewModels.IndexOf(scivm), 0);
                }
                await PrimaryItem.UpdateSortOrderAsync();
                IsBusy = false;
            },
            () => {
                if (IsBusy ||
                     MpMainWindowViewModel.Instance.IsMainWindowLoading ||
                     Items.Count == 0 ||
                     SelectedItems.Count == 0 ||
                     SelectedItems.Count > 1) {
                    return false;
                }
                bool canBringForward = false;

                for (int i = 0; i < PrimaryItem.Count && i < PrimaryItem.SelectedItems.Count; i++) {
                    if (!PrimaryItem.SelectedItems.Contains(PrimaryItem.ItemViewModels[i])) {
                        canBringForward = true;
                        break;
                    }
                }
                return canBringForward;
            });

        public ICommand SendSelectedClipTilesToBackCommand => new RelayCommand(
            async () => {
                IsBusy = true;

                var sil = PrimaryItem.SelectedItems;
                sil.Reverse();
                foreach (var scivm in sil) {
                    PrimaryItem.ItemViewModels.Move(PrimaryItem.ItemViewModels.IndexOf(scivm), PrimaryItem.Count - 1);
                }
                await PrimaryItem.UpdateSortOrderAsync();
                IsBusy = false;
            },
            () => {
                if (IsBusy ||
                     MpMainWindowViewModel.Instance.IsMainWindowLoading ||
                     Items.Count == 0 ||
                     SelectedItems.Count == 0 ||
                     SelectedItems.Count > 1) {
                    return false;
                }
                bool canSendBack = false;
                for (int i = 0; i < PrimaryItem.Count && i < PrimaryItem.SelectedItems.Count; i++) {
                    if (!PrimaryItem.SelectedItems.Contains(PrimaryItem.ItemViewModels[PrimaryItem.ItemViewModels.Count - 1 - i])) {
                        canSendBack = true;
                        break;
                    }
                }
                return canSendBack;
            });

        public ICommand DeleteSelectedClipsCommand => new RelayCommand(
            async () => {
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

        public ICommand InvertSelectionCommand => new RelayCommand(
            () => {
                var sctvml = SelectedItems;
                ClearClipSelection();
                foreach (var vctvm in Items) {
                    if (!sctvml.Contains(vctvm)) {
                        vctvm.IsSelected = true;
                    }
                }
            },
            () => SelectedItems.Count != Items.Count);

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

        public ICommand SendToEmailCommand => new RelayCommand(
            () => {
                string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                MpHelpers.Instance.OpenUrl(
                    string.Format("mailto:{0}?subject={1}&body={2}",
                    string.Empty, SelectedItems[0].HeadItem.CopyItem.Title,
                    pt));
                //MpClipTrayViewModel.Instance.ClearClipSelection();
                //IsSelected = true;
                //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
            },
            () => {
                return !IsAnyEditingClipTile && SelectedItems.Count > 0;
            });

        public ICommand MergeSelectedClipsCommand => new RelayCommand(
            () => {
                PrimaryItem.PrimaryItem.RequestMerge();
            },
            () => {
                return SelectedItems.Count > 0 &&
                       SelectedItems.All(x => x.ItemType == SelectedItems[0].ItemType) &&
                       (SelectedItems[0].ItemType == MpCopyItemType.RichText);
            });

        public ICommand TranslateSelectedClipTextAsyncCommand => new RelayCommand<string>(
            async (toLanguage) => {
                var translatedText = await MpLanguageTranslator.Instance.TranslateAsync(SelectedItems[0].HeadItem.CopyItem.ItemData.ToPlainText(), toLanguage, "", false);
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
                    string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                    bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(pt);
                });
                MpClipboardManager.Instance.SetImageWrapper(bmpSrc);
            },
            () => {
                string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                return (GetSelectedClipsType() == MpCopyItemType.RichText) &&
                    pt.Length <= Properties.Settings.Default.MaxQrCodeCharLength;
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
                IsBusy = true;

                foreach (var sctvm in SelectedItems) {
                    foreach (var ivm in sctvm.SelectedItems) {
                        var clonedCopyItem = (MpCopyItem)await ivm.CopyItem.Clone(true);
                        await clonedCopyItem.WriteToDatabaseAsync();
                        _newModels.Add(clonedCopyItem);
                    }
                }

                AddNewItemsCommand.Execute(true);

                IsBusy = false;
            });

        public ICommand SelectItemCommand => new RelayCommand<object>(
            (arg) => {
                MpClipTileViewModel ctvm = null;
                if (arg is int ctvmIdx) {
                    if (ctvmIdx >= 0 && ctvmIdx < Items.Count) {
                        ctvm = Items[ctvmIdx];
                    }
                } else if (arg is MpClipTileViewModel) {
                    ctvm = arg as MpClipTileViewModel;
                } else if (arg is MpContentItemViewModel civm) {
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
                if (presetId > 0) {
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
