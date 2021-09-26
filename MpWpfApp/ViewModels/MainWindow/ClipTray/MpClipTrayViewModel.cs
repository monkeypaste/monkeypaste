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
using AsyncAwaitBestPractices.MVVM;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;
using MonkeyPaste;
using Microsoft.WindowsAPICodePack.Shell;
using static SQLite.SQLite3;

namespace MpWpfApp {
    public class MpClipTrayViewModel : MpViewModelBase<object>, MpIContentCommands  {
        #region Singleton Definition
        private static readonly Lazy<MpClipTrayViewModel> _Lazy = new Lazy<MpClipTrayViewModel>(() => new MpClipTrayViewModel());
        public static MpClipTrayViewModel Instance { get { return _Lazy.Value; } }

        public void Init() { }
        #endregion

        #region Private Variables      
        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;

        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private List<MpClipTileViewModel> _hiddenTiles = new List<MpClipTileViewModel>();

        //private double _originalExpandedTileX = 0;
        //private int _expandedTileVisibleIdx = 0;

        private int _itemsAdded = 0;
        private int _pageSize = 20;

        private List<MpClipTileViewModel> _newTileList = new List<MpClipTileViewModel>();

        private int _remainingItemsCount = 0;
        private List<MpClipTileViewModel> _availableTiles = new List<MpClipTileViewModel>();
        //private List<MpClipTileViewModel> _allTiles = new List<MpClipTileViewModel>();
        #endregion

        #region Properties
        public string SelectedClipTilesMergedPlainText, SelectedClipTilesCsv;
        public string[] SelectedClipTilesFileList, SelectedClipTilesMergedPlainTextFileList, SelectedClipTilesMergedRtfFileList;

        public bool WasItemAdded { get; set; } = false;

        #region View Models

        private ObservableCollection<MpClipTileViewModel> _clipTileViewModels = new ObservableCollection<MpClipTileViewModel>();
        public ObservableCollection<MpClipTileViewModel> ClipTileViewModels {
            get {
                return _clipTileViewModels;
            }
            set {
                if (_clipTileViewModels != value) {
                    _clipTileViewModels = value;
                    OnPropertyChanged(nameof(ClipTileViewModels));
                }
            }
        }

        public List<MpClipTileViewModel> SelectedItems {
            get {
                return ClipTileViewModels.Where(ct => ct.IsSelected).OrderBy(x => x.LastSelectedDateTime).ToList();
            }
        }
        public List<MpClipTileViewModel> VisibileClipTiles {
            get {
                return ClipTileViewModels.Where(ct => ct.ItemVisibility == Visibility.Visible).ToList();
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
                    return null;
                }
                return SelectedItems[0];
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
        #endregion

        #region Controls
        // public Grid ClipTrayContainerGrid;

        //public VirtualizingStackPanel ClipTrayVirtualizingStackPanel;

        //public AdornerLayer ClipTrayAdornerLayer;
        #endregion

        #region Layout
        public Point DropTopPoint { get; set; }
        public Point DropBottomPoint { get; set; }
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
                    return ClipTileViewModels.IndexOf(SelectedItems[0]);
                }
                return -1;
            }
        }


        #endregion

        #region State
        private int _tagId = 2;
        public int TagId {
            get {
                return _tagId;
            }
            set {
                if (_tagId != value) {
                    _tagId = value;
                    OnPropertyChanged(nameof(TagId));
                }
            }
        }
        public bool IsPastingHotKey { get; set; } = false;
        public bool IsPastingSelected { get; set; } = false;

        public bool IsAnyContextMenuOpened {
            get {
                return ClipTileViewModels.Any(x => x.IsAnyContextMenuOpened);
            }
        }

        

        public bool IsTrayDropping { get; set; } = false;

        public bool IsAnyClipOrSubItemDragging {
            get {
                foreach (var ctvm in VisibileClipTiles) {
                    if (ctvm.IsClipDragging || ctvm.IsAnySubItemDragging) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsAnyClipDropping {
            get {
                foreach (var ctvm in VisibileClipTiles) {
                    if (ctvm.IsClipDropping) {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool IsAnyTileExpanded {
            get {
                foreach (var ctvm in ClipTileViewModels) {
                    if (ctvm.IsExpanded) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsAnyEditing {
            get {
                return ClipTileViewModels.Any(x => x.IsAnyEditingContent);
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

        public bool IsAnyHovering {
            get {
                return VisibileClipTiles.Where(x => x.IsHovering).ToList().Count > 0;
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
                foreach (var sctvm in SelectedItems) {
                    if (sctvm.IsAnyEditingTitle) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsEditingClipTile {
            get {
                foreach (var sctvm in SelectedItems) {
                    if (sctvm.IsAnyEditingContent) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsPastingTemplate {
            get {
                foreach (var sctvm in SelectedItems) {
                    if (sctvm.IsAnyPastingTemplate) {
                        return true;
                    }
                }
                return false;
            }
        }
        #endregion

        #region Visibility

        public Visibility EmptyListMessageVisibility {
            get {
                if (VisibileClipTiles.Count == 0) {
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
        public event EventHandler ItemsVisibilityChanged;
        public virtual void OnItemsVisibilityChanged() => ItemsVisibilityChanged?.Invoke(this, EventArgs.Empty);

        public event EventHandler<object> OnFocusRequest;

        public event EventHandler OnUiRefreshRequest;

        public event EventHandler<object> OnTilesChanged;

        public event EventHandler<object> OnScrollIntoViewRequest;
        public event EventHandler OnScrollToHomeRequest;
        #endregion

        #region Public Methods

        public MpClipTrayViewModel() : base(null) {
            MonkeyPaste.MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;


            ClipTileViewModels.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(EmptyListMessageVisibility));
                OnPropertyChanged(nameof(ClipTrayVisibility));
            };
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(IsFilteringByApp):
                        foreach (var ctvm in VisibileClipTiles) {
                            //ctvm.OnPropertyChanged(nameof(ctvm.AppIcon));
                        }
                        break;
                    case nameof(ClipTileViewModels):
                        OnTilesChanged?.Invoke(this, ClipTileViewModels);
                        break;
                }
            };

            //_allTiles = MpDb.Instance.GetItems<MpCopyItem>()
            //                         .Where(x => x.CompositeParentCopyItemId == 0)
            //                         .Select(y => CreateClipTileViewModel(y))
            //                         .ToList();

            //RefreshClips(MpTag.RecentTagId);
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


        public MpClipTileViewModel GetNextItem() {
            int nextIdx = _availableTiles.IndexOf(ClipTileViewModels[ClipTileViewModels.Count - 1]) + 1;
            if(nextIdx >= _availableTiles.Count) {
                return null;
            }
            return _availableTiles[nextIdx];
        }

        public void RefreshClipsAsync(bool isDescending = true, string sortColumn = "CopyDateTime", int start = 0, int count = 0) {
            Task.Run(() => {//MpHelpers.Instance.RunOnMainThread(() => {
                int tagId = MpTagTrayViewModel.Instance.SelectedTagTile.TagId;
                if(count == 0) {
                    count = MpMeasurements.Instance.TotalVisibleClipTiles;
                }

                var sw = new Stopwatch();
                sw.Start();
                var page_cil = MpCopyItem.GetPage(tagId, start, count, sortColumn, isDescending);
                sw.Stop();
                MpConsole.WriteLine("Model Page time: " + sw.ElapsedMilliseconds);
                sw.Start();
                var page_vml = page_cil.Where(x => x.CompositeParentCopyItemId == 0).Select(y => CreateClipTileViewModel(y)).ToList();
                sw.Stop();
                MpConsole.WriteLine("Create vm time: " + sw.ElapsedMilliseconds);
            
                sw.Start();
                ClipTileViewModels = new ObservableCollection<MpClipTileViewModel>(page_vml);
                _remainingItemsCount = ClipTileViewModels.Count - MpMeasurements.Instance.TotalVisibleClipTiles;

                if (!MpMainWindowViewModel.IsMainWindowLoading) {
                    ResetClipSelection();
                }
                sw.Stop();
                MpConsole.WriteLine("Load list time: " + sw.ElapsedMilliseconds);
            });
        }

        public void RefreshClips(bool isDescending = true, string sortColumn = "CopyDateTime", int start = 0, int count = 0) {
            int tagId = MpTagTrayViewModel.Instance.SelectedTagTile.TagId;
            if (count == 0) {
                count = MpMeasurements.Instance.TotalVisibleClipTiles;
            }
            var page_cil = MpCopyItem.GetPage(tagId, start, count, sortColumn, isDescending);

            var page_vml = page_cil.Where(x => x.CompositeParentCopyItemId == 0).Select(y => CreateClipTileViewModel(y)).ToList();


            MpHelpers.Instance.RunOnMainThreadAsync(() => {
                ClipTileViewModels = new ObservableCollection<MpClipTileViewModel>(page_vml);
                _remainingItemsCount = ClipTileViewModels.Count - MpMeasurements.Instance.TotalVisibleClipTiles;

                if (!MpMainWindowViewModel.IsMainWindowLoading) {
                    ResetClipSelection();
                }
            });
        }

        public void IsolateClipTile(MpClipTileViewModel tileToIsolate) {
            if (!VisibileClipTiles.Contains(tileToIsolate)) {
                MonkeyPaste.MpConsole.WriteLine("Warning tile to isolate was hidden and is now being shown");
                tileToIsolate.ItemVisibility = Visibility.Visible;
            }
            //var subSelectedItems = tileToIsolate.RichTextBoxViewModelCollection.SubSelectedContentItems;
            //ClearClipSelection(false);
            _hiddenTiles = VisibileClipTiles.ToList();
            _hiddenTiles.Remove(tileToIsolate);
            foreach (var ctvm in _hiddenTiles) {
                ctvm.IsSelected = false;
                //ctvm.IsPrimarySelected = false;
                ctvm.ItemVisibility = Visibility.Collapsed;
            }
            if (!tileToIsolate.IsSelected) {
                tileToIsolate.IsSelected = true;
                //foreach(var rtbvm in tileToIsolate.RichTextBoxViewModelCollection) {
                //    if(subSelectedItems.Contains(rtbvm)) {
                //        rtbvm.IsSelected = true;
                //    }
                //}
            }

            //ClipTrayVirtualizingStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
        }
        public void RestoreVisibleTiles() {
            //var _hiddenTileCanvasList = new List<FrameworkElement>();
            foreach (var ctvm in _hiddenTiles) {
                //_hiddenTileCanvasList.Add(ctvm.ClipBorder);
                ctvm.IsSelected = false;
                //ctvm.IsPrimarySelected = false;
                ctvm.ItemVisibility = Visibility.Visible;
            }

            //ClipTrayVirtualizingStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
            //Refresh();
            //if (_hiddenTileCanvasList.Count > 0) {
            //    MpHelpers.Instance.AnimateVisibilityChange(
            //        _hiddenTileCanvasList,
            //        Visibility.Visible,
            //        (s, e) => Refresh(),
            //        animMs,
            //        animMs);
            //}
        }

        public void Resize(
            MpClipTileViewModel tileToResize,
            double deltaWidth,
            double deltaHeight,
            double deltaEditToolbarTop) {
            MainWindowViewModel.ClipTrayHeight += deltaHeight;
            //ClipTileViewModels.ListBox.Height = MainWindowViewModel.ClipTrayHeight;
            //ClipTileViewModels.ListBox.UpdateLayout();

            tileToResize.Resize(
                deltaWidth,
                deltaHeight,
                deltaEditToolbarTop);
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
            bool wasExpanded = IsAnyTileExpanded;

            foreach (var ctvm in ClipTileViewModels) {
                ctvm.ClearEditing();
            }
            
            if(wasExpanded) {
                MpHelpers.Instance.RunOnMainThread((Action)delegate {
                    MainWindowViewModel.OnPropertyChanged(nameof(MainWindowViewModel.AppModeButtonGridWidth));
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.AppModeColumnVisibility));
                });
            }
        }

        public void ClearClipSelection(bool clearEditing = true) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (clearEditing) {
                    ClearClipEditing();
                }
                foreach (var ctvm in ClipTileViewModels) {
                    ctvm.ClearClipSelection();
                }
            }));
        }

        public void ResetClipSelection(bool clearEditing = true) {
            MpHelpers.Instance.RunOnMainThread(() => {
                ClearClipSelection(clearEditing);

                if (VisibileClipTiles.Count > 0) {
                    VisibileClipTiles[0].IsSelected = true;
                    //if (!MpSearchBoxViewModel.Instance.IsTextBoxFocused) {
                    //    RequestFocus(SelectedItems[0]);
                    //}
                }
            });
        }

        public void UnFlipAllTiles() {
            // TODO make async and do Unflip here
            foreach(var ctvm in ClipTileViewModels) {
                if(ctvm.IsFlipped) {
                    FlipTileCommand.Execute(ctvm);
                }
            }
        }

        public void ClearAllDragDropStates() {
            IsTrayDropping = false;
            foreach (var ctvm in ClipTileViewModels) {
                ctvm.ClearDragDropState();
            }
        }

        public void RefreshAllCommands() {
            foreach (MpClipTileViewModel ctvm in ClipTileViewModels) {
                ctvm.RefreshAsyncCommands();
            }
        }

        private void AddTileThread() {
            var totalAddSw = new Stopwatch();
            totalAddSw.Start();

            var createItemSw = new Stopwatch();
            createItemSw.Start();
            var newCopyItem = MpCopyItemBuilder.CreateFromClipboard();

            MonkeyPaste.MpConsole.WriteLine("CreateFromClipboardAsync: " + createItemSw.ElapsedMilliseconds + "ms");

            if (newCopyItem == null) {
                //this occurs if the copy item is not a known format or app init
                MpConsole.WriteTraceLine("Unable to create copy item from clipboard!");
                return;
            } else if (MpAppModeViewModel.Instance.IsInAppendMode) {
                //when in append mode just append the new items text to selecteditem
                PrimaryItem.InsertRange(PrimaryItem.Count, new List<MpCopyItem>() { newCopyItem });
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
            } else {
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
                    if (IsTrialExpired) {
                        MpStandardBalloonViewModel.ShowBalloon(
                            "Trial Expired",
                            "Please update your membership to use Monkey Paste",
                            Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                    }
                }
            }
            totalAddSw.Stop();
            MonkeyPaste.MpConsole.WriteLine("Time to create new copyitem: " + totalAddSw.ElapsedMilliseconds + " ms");

            WasItemAdded = true;
        }

        public void AddItemFromClipboard() {
            var workThread = new Thread(new ThreadStart(AddTileThread));
            workThread.SetApartmentState(ApartmentState.STA);
            workThread.IsBackground = true;
            workThread.Start(); 
        }

        public void Refresh() {
            var sw = new Stopwatch();
            sw.Start();
            RequestUiRefresh();
            sw.Stop();
            MonkeyPaste.MpConsole.WriteLine("ClipTray Refreshed (" + sw.ElapsedMilliseconds + "ms)");
        }


        public void Select(List<int> visibleTileIdxList) {
            int vcount = VisibileClipTiles.Count;
            ClearClipSelection();
            for (int i = 0; i < visibleTileIdxList.Count; i++) {
                int idx = visibleTileIdxList[i];
                if (idx < 0 || idx >= vcount) {
                    throw new Exception($"Cannot select idx: {idx} with Visible List of size {vcount}");
                }
                VisibileClipTiles[idx].IsSelected = true;
            }
        }

        public void Unselect(List<int> visibleTileIdxList) {
            int vcount = VisibileClipTiles.Count;
            for (int i = 0; i < visibleTileIdxList.Count; i++) {
                int idx = visibleTileIdxList[i];
                if (idx < 0 || idx >= vcount) {
                    throw new Exception($"Cannot select idx: {idx} with Visible List of size {vcount}");
                }
                VisibileClipTiles[idx].IsSelected = false;
            }
        }

        public MpClipTileViewModel CreateClipTileViewModel(MpCopyItem ci) {
            var nctvm = new MpClipTileViewModel(this,ci);
            nctvm.OnTileSelected += ClipTileViewModel_OnTileSelected;
            return nctvm;
        }

        private void ClipTileViewModel_OnTileSelected(object sender, EventArgs e) {
            if (!MpHelpers.Instance.IsMultiSelectKeyDown()) {
                foreach (var ctvm in ClipTileViewModels) {
                    if (ctvm != sender) {
                        ctvm.IsSelected = false;
                    }
                }
            }
        }

        public void Remove(MpClipTileViewModel clipTileToRemove, bool isMerge = false) {
            ClipTileViewModels.Remove(clipTileToRemove);

            if (clipTileToRemove.HeadItem == null) {
                //occurs when duplicate detected on background thread
                return;
            }

            if (isMerge) {
                clipTileToRemove.IsClipDragging = false;
                foreach (var rtbvm in clipTileToRemove.ItemViewModels) {
                    rtbvm.IsSubDragging = false;
                }
            } else {
                clipTileToRemove.Dispose();
                clipTileToRemove = null;


                MpHelpers.Instance.RunOnMainThread(() => {
                    RefreshClips();
                });
            }
        }

        public async Task RemoveAsync(MpClipTileViewModel ctvm, bool isMerge, DispatcherPriority priority = DispatcherPriority.Background) {
            IsBusy = true;
            await Application.Current.Dispatcher.BeginInvoke(priority,
                (Action)(() => {
                    this.Remove(ctvm, isMerge);
                }));
            IsBusy = false;
        }

        public async Task<IDataObject> GetDataObjectFromSelectedClips(bool isDragDrop = false, bool isToExternalApp = false) {
            IDataObject d = new DataObject();

            //selection (if all subitems are dragging select host if no subitems are selected select all)
            foreach (var sctvm in SelectedItems) {
                if (sctvm.SelectedItems.Count == sctvm.Count ||
                    sctvm.Count <= 1) {
                    sctvm.IsClipDragging = true;
                }
                if (sctvm.SelectedItems.Count == 0) {
                    sctvm.SubSelectAll();
                }

                foreach (var srtbvm in sctvm.SelectedItems) {
                    srtbvm.IsSubDragging = true;
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
                        dctvm.IsClipDragging = true;
                    }
                }
                d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedItems.ToList());
            }

            return d;
            //awaited in MainWindowViewModel.HideWindow
        }

        public void PasteDataObject(IDataObject pasteDataObject, bool fromHotKey = false) {
            if (IsPastingTemplate) {
                MainWindowViewModel.IsMainWindowLocked = false;
            }

            //called in the oncompleted of hide command in mwvm
            if (pasteDataObject != null) {
                MonkeyPaste.MpConsole.WriteLine("Pasting " + SelectedItems.Count + " items");
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
                        ClipTileViewModels.Move(ClipTileViewModels.IndexOf(sctvm), 0);

                        var a = MpApp.GetAppByPath(MpHelpers.Instance.GetProcessPath(MpClipboardManager.Instance.LastWindowWatcher.LastHandle));
                        var aid = a == null ? 0 : a.Id;
                        foreach(var ivm in sctvm.ItemViewModels) {
                            new MpPasteHistory() {
                                AppId = aid,
                                CopyItemId = ivm.CopyItem.Id,
                                UserDeviceId = MpUserDevice.GetUserDeviceByGuid(MpPreferences.Instance.ThisDeviceGuid).Id
                            }.WriteToDatabase();
                        }
                    }
                    //Refresh();
                }
            } else if (pasteDataObject == null) {
                MonkeyPaste.MpConsole.WriteLine("MainWindow Hide Command pasteDataObject was null, ignoring paste");
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
            foreach (MpClipTileViewModel ctvm in ClipTileViewModels) {
                if (ctvm.ItemViewModels.Any(x=>x.CopyItem.Source.AppId == appId)) {
                    ctvml.Add(ctvm);
                }
            }
            return ctvml;
        }

        public MpContentItemViewModel GetCopyItemViewModelById(int ciid) {
            foreach (var ctvm in ClipTileViewModels) {
                foreach (var civm in ctvm.ItemViewModels) {
                    var ortbvm = ctvm.ItemViewModels.Where(x => x.CopyItem.Id == ciid).FirstOrDefault();
                    if (ortbvm != null) {
                        return ortbvm;
                    }
                }
            }
            return null;
        }

        public MpContentItemViewModel GetCopyItemViewModelByCopyItem(MpCopyItem ci) {
            foreach (var ctvm in ClipTileViewModels) {
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
                var ivm = GetCopyItemViewModelById(ci.Id);
                //ivm.CopyItem = ci;
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                var ivm = GetCopyItemViewModelByCopyItem(ci);
                ivm.Parent.ItemViewModels.Remove(ivm);
                if(ivm.Parent.ItemViewModels.IsEmpty()) {
                   // _allTiles.Remove(ivm.Parent);
                }                
            }
        }

        #region Sync Events

        private void MpDbObject_SyncDelete(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (sender is MpCopyItem ci) {
                    var ctvmToRemove = GetCopyItemViewModelById(ci.Id);
                    if (ctvmToRemove != null) {
                        ctvmToRemove.CopyItem.StartSync(e.SourceGuid);
                        //ctvmToRemove.CopyItem.Color.StartSync(e.SourceGuid);
                        ctvmToRemove.Parent.ItemViewModels.Remove(ctvmToRemove);
                        if (ctvmToRemove.Parent.ItemViewModels.Count == 0) {
                            ClipTileViewModels.Remove(ctvmToRemove.Parent);
                        }
                        ctvmToRemove.CopyItem.EndSync();
                        //ctvmToRemove.CopyItem.Color.EndSync();
                    }
                }
            }));
        }

        private void MpDbObject_SyncUpdate(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
            }));
        }

        private void MpDbObject_SyncAdd(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (sender is MpCopyItem ci) {
                    ci.StartSync(e.SourceGuid);
                    ci.Source.App.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.IconImage.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.IconBorderImage.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.IconBorderHighlightImage.StartSync(e.SourceGuid);
                    ci.Source.App.Icon.IconBorderHighlightSelectedImage.StartSync(e.SourceGuid);

                    var dupCheck = GetCopyItemViewModelById(ci.Id);
                    if (dupCheck == null) {
                        if (ci.Id == 0) {
                            ci.WriteToDatabase();
                        }
                        var nctvm = new MpClipTileViewModel(this, ci);
                        _newTileList.Add(nctvm);
                        //AddNewTiles();
                    } else {
                        MonkeyPaste.MpConsole.WriteTraceLine(@"Warning, attempting to add existing copy item: " + dupCheck.CopyItem.ItemData + " ignoring and updating existing.");
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

        private void ClipTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.NewItems != null && e.NewItems.Count > 0) {
                IsBusy = false;
                ClipTileViewModels.CollectionChanged -= ClipTileViewModels_CollectionChanged;
            }
        }

        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args) {
            foreach (MpCopyItem ci in args.NewItems) {
                _itemsAdded++;
                ClipTileViewModels.Add(new MpClipTileViewModel(this,ci));
            }
            if (_itemsAdded == _pageSize) {
                var collection = (ObservableCollection<MpCopyItem>)sender;
                collection.CollectionChanged -= Collection_CollectionChanged;
            }
        }


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
                   SelectedItems.Any(x => VisibileClipTiles.IndexOf(x) != VisibileClipTiles.Count - 1);
        }
        private void SelectNextItem() {
            var maxItem = SelectedItems.Max(x => VisibileClipTiles.IndexOf(x));
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
            return SelectedItems.Count > 0 && SelectedItems.Any(x => VisibileClipTiles.IndexOf(x) != 0);
        }
        private void SelectPreviousItem() {
            var minItem = SelectedItems.Min(x => VisibileClipTiles.IndexOf(x));
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
            if (brush == null) {
                return;
            }
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            //BitmapSource sharedSwirl = null;
                            foreach (var sctvm in SelectedItems) {
                                sctvm.PrimaryItem.TitleBackgroundColor = brush;
                                sctvm.PrimaryItem.TitleSwirlViewModel.ForceBrush(brush);
                                //if (sharedSwirl == null) {
                                //    sctvm.TitleSwirl = sctvm.CopyItem.InitSwirl(null,true);
                                //    sharedSwirl = sctvm.TitleSwirl;
                                //} else {
                                //    sctvm.TitleSwirl = sctvm.CopyItem.InitSwirl(sharedSwirl);
                                //}
                                //sctvm.CopyItem.WriteToDatabase();
                            }
                        }));
            }
            finally {
                IsBusy = false;
            }
        }

        private AsyncCommand<object> _hotkeyPasteCommand;
        public IAsyncCommand<object> PerformHotkeyPasteCommand {
            get {
                if (_hotkeyPasteCommand == null) {
                    _hotkeyPasteCommand = new AsyncCommand<object>(HotkeyPaste, CanHotkeyPaste);
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
            MonkeyPaste.MpConsole.WriteLine("HotKey pasting copyitemid: " + (int)args);
            IsPastingHotKey = true;
            int copyItemId = (int)args;
            IDataObject pasteDataObject = null;
            var pctvm = GetCopyItemViewModelById(copyItemId);
            if (pctvm != null) {
                ClearClipSelection();
                pctvm.IsSelected = true;
                pctvm.Parent.SubSelectAll();
                pasteDataObject = await GetDataObjectFromSelectedClips(false, true);
                ClearClipSelection();
            } else {
                //otherwise check if it is a composite within a tile
                MpContentItemViewModel prtbvm = GetCopyItemViewModelById(copyItemId) as MpContentItemViewModel;
                //foreach (var ctvm in MpClipTrayViewModel.Instance.ClipTileViewModels) {
                //    prtbvm = ctvm.ContentContainerViewModel.GetRtbItemByCopyItemId(copyItemId);
                //    if (prtbvm != null) {
                //        break;
                //    }
                //}
                if (prtbvm != null) {
                    ClearClipSelection();
                    prtbvm.Parent.IsSelected = true;
                    prtbvm.Parent.ClearClipSelection();
                    prtbvm.IsSelected = true;
                    pasteDataObject = await GetDataObjectFromSelectedClips(false, true);
                    prtbvm.Parent.ClearClipSelection();
                    ClearClipSelection();
                }
            }

            if (MpMainWindowViewModel.IsMainWindowOpen) {
                //occurs during hotkey paste and set in ctvm.GetPastableRichText
                MainWindowViewModel.HideWindowCommand.Execute(pasteDataObject);
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
                !IsEditingClipTile &&
                !IsEditingClipTitle &&
                !IsPastingTemplate &&
                !IsTrialExpired;
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
            MainWindowViewModel.HideWindowCommand.Execute(true);
            IsPastingSelected = false;
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
            if (IsBusy ||
                MpMainWindowViewModel.IsMainWindowLoading ||
                VisibileClipTiles.Count == 0 ||
                SelectedItems.Count == 0) {
                return false;
            }
            bool canBringForward = false;
            for (int i = 0; i < SelectedItems.Count && i < VisibileClipTiles.Count; i++) {
                if (!SelectedItems.Contains(VisibileClipTiles[i])) {
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
                                ClipTileViewModels.Move(ClipTileViewModels.IndexOf(sctvm), 0);
                                sctvm.IsSelected = true;
                            }
                            RequestScrollIntoView(SelectedItems[0]);
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
            if (IsBusy ||
                MpMainWindowViewModel.IsMainWindowLoading ||
                VisibileClipTiles.Count == 0 ||
                SelectedItems.Count == 0) {
                return false;
            }
            bool canSendBack = false;
            for (int i = 0; i < SelectedItems.Count && i < VisibileClipTiles.Count; i++) {
                if (!SelectedItems.Contains(VisibileClipTiles[VisibileClipTiles.Count - 1 - i])) {
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
                                ClipTileViewModels.Move(ClipTileViewModels.IndexOf(sctvm), ClipTileViewModels.Count - 1);
                                sctvm.IsSelected = true;
                            }
                            RequestScrollIntoView(SelectedItems[SelectedItems.Count - 1]);
                        }));
            }
            finally {
                IsBusy = false;
            }
        }

        private RelayCommand _deleteSelectedClipsCommand;
        public ICommand DeleteSelectedClipsCommand {
            get {
                if (_deleteSelectedClipsCommand == null) {
                    _deleteSelectedClipsCommand = new RelayCommand(DeleteSelectedClips, CanDeleteSelectedClips);
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
            foreach (var ct in SelectedItems) {
                lastSelectedClipTileIdx = VisibileClipTiles.IndexOf(ct);
                this.Remove(ct);
            }
            ClearClipSelection();
            if (VisibileClipTiles.Count > 0) {
                if (lastSelectedClipTileIdx <= 0) {
                    VisibileClipTiles[0].IsSelected = true;
                } else if (lastSelectedClipTileIdx < VisibileClipTiles.Count) {
                    VisibileClipTiles[lastSelectedClipTileIdx].IsSelected = true;
                } else {
                    VisibileClipTiles[lastSelectedClipTileIdx - 1].IsSelected = true;
                }
            }
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
            MainWindowViewModel.TagTrayViewModel.RefreshAllCounts();
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
            return SelectedItems.Count != VisibileClipTiles.Count;
        }
        private void InvertSelection() {
            var sctvml = SelectedItems;
            ClearClipSelection();
            foreach (var vctvm in VisibileClipTiles) {
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

        private RelayCommand _editSelectedContentCommand;
        public ICommand EditSelectedContentCommand {
            get {
                if (_editSelectedContentCommand == null) {
                    _editSelectedContentCommand = new RelayCommand(EditSelectedContent, CanEditSelectedContent);
                }
                return _editSelectedContentCommand;
            }
        }
        private bool CanEditSelectedContent() {
            if (MpMainWindowViewModel.IsMainWindowLoading) {
                return false;
            }
            return SelectedItems.Count == 1 &&
                  SelectedItems[0].SelectedItems.Count <= 1 &&
                  SelectedItems[0].IsTextItem;
        }
        private void EditSelectedContent() {
            SelectedItems[0].PrimaryItem.EditSubContentCommand.Execute(null);
        }

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
            return !IsEditingClipTile && SelectedItems.Count > 0;
        }
        private void SendSelectedClipsToEmail() {
            MpHelpers.Instance.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, SelectedItems[0].HeadItem.CopyItem.Title, SelectedClipTilesMergedPlainText));
            //MpClipTrayViewModel.Instance.ClearClipSelection();
            //IsSelected = true;
            //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        }



        private AsyncCommand _mergeSelectedClipsCommand;
        public IAsyncCommand MergeSelectedClipsCommand {
            get {
                if (_mergeSelectedClipsCommand == null) {
                    _mergeSelectedClipsCommand = new AsyncCommand(MergeSelectedClips, CanMergeSelectedClips);
                }
                return _mergeSelectedClipsCommand;
            }
        }
        private bool CanMergeSelectedClips(object args) {
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
            var sctvml = SelectedItems;
            var ocil = new List<MpCopyItem>();
            foreach (var sctvm in sctvml) {
                if (sctvm == PrimaryItem) {
                    continue;
                }
                ocil.AddRange(sctvm.ItemViewModels.Select(x=>x.CopyItem).ToList());
            }

            await PrimaryItem.MergeCopyItemListAsync(ocil);
        }

        private AsyncCommand<string> _translateSelectedClipTextAsyncCommand;
        public IAsyncCommand<string> TranslateSelectedClipTextAsyncCommand {
            get {
                if (_translateSelectedClipTextAsyncCommand == null) {
                    _translateSelectedClipTextAsyncCommand = new AsyncCommand<string>(TranslateSelectedClipTextAsync, CanTranslateSelectedClipText);
                }
                return _translateSelectedClipTextAsyncCommand;
            }
        }
        private bool CanTranslateSelectedClipText(object args) {
            return SelectedItems.Count == 1 && SelectedItems[0].IsTextItem;
        }
        private async Task TranslateSelectedClipTextAsync(string toLanguage) {
            var translatedText = await MpLanguageTranslator.Instance.Translate(SelectedItems[0].HeadItem.CopyItem.ItemData.ToPlainText(), toLanguage, false);
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
            System.Windows.Clipboard.SetImage(bmpSrc);
        }

        private AsyncCommand _speakSelectedClipsCommand;
        public IAsyncCommand SpeakSelectedClipsCommand {
            get {
                if (_speakSelectedClipsCommand == null) {
                    _speakSelectedClipsCommand = new AsyncCommand(SpeakSelectedClipsAsync, CanSpeakSelectedClips);
                }
                return _speakSelectedClipsCommand;
            }
        }
        private bool CanSpeakSelectedClips(object args) {
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
            var tempSelectedClipTiles = SelectedItems;
            ClearClipSelection();
            foreach (var sctvm in tempSelectedClipTiles) {
                foreach(var ivm in sctvm.SelectedItems) {
                    var clonedCopyItem = (MpCopyItem)ivm.CopyItem.Clone();
                    clonedCopyItem.WriteToDatabase();
                    //var ctvm = new MpClipTileViewModel(clonedCopyItem);
                    //MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
                    //this.Add(ctvm);
                    RefreshClips();
                    var ctvm = GetCopyItemViewModelById(clonedCopyItem.Id);
                    ctvm.IsSelected = true;
                }

            }
        }

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

        public ICommand EditContentCommand {
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

        public ICommand LoadMoreClipsCommand {
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
