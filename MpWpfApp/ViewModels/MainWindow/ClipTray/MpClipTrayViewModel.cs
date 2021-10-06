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
using static OpenTK.Graphics.OpenGL.GL;

namespace MpWpfApp {
    public class MpClipTrayViewModel : MpViewModelBase<object>, MpIContentCommands  {
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


        private List<MpClipTileViewModel> _newTileList = new List<MpClipTileViewModel>();

        private int _remainingItemsCount = 0;
        private List<MpClipTileViewModel> _availableTiles = new List<MpClipTileViewModel>();

        private MpCopyItem _appendModeCopyItem = null;
        #endregion

        #region Properties
        public string SelectedClipTilesMergedPlainText, SelectedClipTilesCsv;
        public string[] SelectedClipTilesFileList, SelectedClipTilesMergedPlainTextFileList, SelectedClipTilesMergedRtfFileList;

        public int ItemsAdded { get; set; }

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
                return ClipTileViewModels.OrderBy(x=>ClipTileViewModels.IndexOf(x)).Where(ct => ct.ItemVisibility == Visibility.Visible).ToList();
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
                    return FirstItem;
                }
                return SelectedItems[0];
            }
        }

        public MpClipTileViewModel FirstItem {
            get {
                if(ClipTileViewModels.Count == 0) {
                    return null;
                }
                return ClipTileViewModels[0];
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

        public bool IgnoreSelectionReset { get; set; } = false;

        public int TagId { get; set; } = MpTag.RecentTagId;

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
        public event EventHandler<object> OnFocusRequest;

        public event EventHandler OnUiRefreshRequest;

        public event EventHandler<object> OnScrollIntoViewRequest;
        public event EventHandler OnScrollToHomeRequest;
        #endregion

        #region Public Methods

        public MpClipTrayViewModel() : base(null) {
            MonkeyPaste.MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;
            _tileLockObject = new object();
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
                if (MpTagTrayViewModel.Instance.SelectedTagTile.IsSudoTag) {
                    //ignore sorting for sudo tags
                    return;
                }
                Task.Run(async()=>{
                    bool isDesc = MpClipTileSortViewModel.Instance.IsSortDescending;
                    int tagId = MpTagTrayViewModel.Instance.SelectedTagTile.Tag.Id;
                    var citl = await MpCopyItemTag.GetAllCopyItemsForTagIdAsync(tagId);

                    int count = isDesc ? citl.Count : 1;
                    //loop through available tiles and reset tag's sort order, 
                    //removing existing items from known ones and creating new ones if that's the case (it shouldn't)
                    foreach(var ctvm in ClipTileViewModels) {
                        foreach(var civm in ctvm.ItemViewModels) {
                            MpCopyItemTag cit = citl.Where(x => x.CopyItemId == civm.CopyItem.Id).FirstOrDefault();
                            if(cit == null) {
                                cit = MpCopyItemTag.Create(tagId, civm.CopyItem.Id, count);
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
            int tagId = MpTagTrayViewModel.Instance.SelectedTagTile.TagId;
            var sortColumn = MpClipTileSortViewModel.Instance.SelectedSortType.SortType;
            bool isDescending = MpClipTileSortViewModel.Instance.IsSortDescending;

            //int totalCount = MpCopyItemTag.GetAllCopyItemsForTagIdAsync(tagId);

            if (count == 0) {
                count = MpMeasurements.Instance.TotalVisibleClipTiles + 1;
            }
            Dictionary<int, int> manualSortOrderLookup = null;

            if(sortColumn == MpClipTileSortType.Manual) {
                manualSortOrderLookup = new Dictionary<int, int>();
                foreach(var ctvm in ClipTileViewModels) {
                    if(manualSortOrderLookup.ContainsKey(ctvm.HeadItem.CopyItemId)) {
                        continue;
                    }
                    manualSortOrderLookup.Add(ctvm.HeadItem.CopyItemId, ClipTileViewModels.IndexOf(ctvm));
                }
            }
            IsBusy = true;
            var page_cil = await MpCopyItem.GetPageAsync(tagId, start, count, sortColumn, isDescending, manualSortOrderLookup);

            //int placeHoldersToAdd = MpMeasurements.Instance.TrayPageSize - page_cil.Count;
            //while(placeHoldersToAdd > 0) {
            //    page_cil.Add(null);
            //    placeHoldersToAdd--;
            //}

            ClipTileViewModels = new ObservableCollection<MpClipTileViewModel>(page_cil.Select(x => CreateClipTileViewModel(x)));
            BindingOperations.EnableCollectionSynchronization(ClipTileViewModels, _tileLockObject);

            _remainingItemsCount = ClipTileViewModels.Count - MpMeasurements.Instance.TotalVisibleClipTiles;

            ResetClipSelection();
            IsBusy = false;

            OnViewModelLoaded();
            sw.Stop();
            MpConsole.WriteLine($"Refresh clips took {sw.ElapsedMilliseconds} ms");

            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                
            });

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
                    if (IsTrialExpired) {
                        MpStandardBalloonViewModel.ShowBalloon(
                            "Trial Expired",
                            "Please update your membership to use Monkey Paste",
                            Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                    }

                    int tileCount = ClipTileViewModels.Count;
                    var newTile = ClipTileViewModels[tileCount - 1];
                    //ClipTileViewModels.Move(tileCount - 1, 0);
                    //MpHelpers.Instance.RunOnMainThreadAsync(async () => { 
                    //    await newTile.Initialize(newCopyItem); 
                    //});                            
                }
            }
            totalAddSw.Stop();
            MonkeyPaste.MpConsole.WriteLine("Time to create new copyitem: " + totalAddSw.ElapsedMilliseconds + " ms");

            ItemsAdded++;
        }

        public void AddItemFromClipboard() {
            var workThread = new Thread(new ThreadStart(AddTileThread));
            workThread.SetApartmentState(ApartmentState.STA);
            workThread.IsBackground = true;
            workThread.Start(); 
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
                    //   !MpHelpers.Instance.IsMultiSelectKeyDown() &&
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
            //awaited in MainWindowViewModel.HideWindow
        }

        public void PasteDataObject(IDataObject pasteDataObject, bool fromHotKey = false) {
            if (IsAnyPastingTemplate) {
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

        public MpContentItemViewModel GetContentItemViewModelById(int ciid) {
            foreach (var ctvm in ClipTileViewModels) {
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
                var ivm = GetContentItemViewModelById(ci.Id);
                //ivm.CopyItem = ci;
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                //Task.Run(async () => {
                //    //await MpHelpers.Instance.RunOnMainThreadAsync(() => RefreshTiles());
                //   // MpTagTrayViewModel.Instance.RefreshAllCounts();
                //});
            }
        }

        #region Sync Events

        private void MpDbObject_SyncDelete(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (sender is MpCopyItem ci) {
                    var ctvmToRemove = GetContentItemViewModelById(ci.Id);
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

                    var dupCheck = GetContentItemViewModelById(ci.Id);
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
                !IsAnyEditingClipTile &&
                !IsAnyEditingClipTitle &&
                !IsAnyPastingTemplate &&
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

        public IAsyncCommand DeleteSelectedClipsCommand => new AsyncCommand(
            async () => {
                foreach (var ci in SelectedModels) {
                    await ci.DeleteFromDatabaseAsync();
                }
            },
            (args) => {
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
            SelectedItems[0].PrimaryItem.ToggleEditSubContentCommand.Execute(null);
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
            return !IsAnyEditingClipTile && SelectedItems.Count > 0;
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
            await Task.Delay(1);
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

        public IAsyncCommand DuplicateSelectedClipsCommand => new AsyncCommand(
            async () => {
                var tempSelectedClipTiles = SelectedItems;
                ClearClipSelection();
                foreach (var sctvm in tempSelectedClipTiles) {
                    foreach (var ivm in sctvm.SelectedItems) {
                        var clonedCopyItem = (MpCopyItem)ivm.CopyItem.Clone();
                        clonedCopyItem.WriteToDatabase();
                        //var ctvm = new MpClipTileViewModel(clonedCopyItem);
                        //MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
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

        public ICommand EditContentCommand => new RelayCommand<object>(
            (civm) => {
                MpSelectionBehavior.IgnoreSelection = !MpSelectionBehavior.IgnoreSelection;
                ClearClipSelection();
                (civm as MpContentItemViewModel).ToggleEditSubContentCommand.Execute(null);
            },
            (civm) => { return true; });

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
