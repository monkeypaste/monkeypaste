using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MonkeyPaste;
using MpProcessHelper;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpClipTrayViewModel : 
        MpViewModelBase, 
        MpISingletonViewModel<MpClipTrayViewModel>, 
        MpIActionComponent, 
        MpIMenuItemViewModel {
        #region Private Variables      

        //private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;

        //private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private List<MpCopyItem> _newModels = new List<MpCopyItem>();

        private MpCopyItem _appendModeCopyItem = null;

        private int _pageSize = 0;

        private Dictionary<int, int> _manualSortOrderLookup = null;
                

        private MpCopyItem _currentClipboardItem;

        #endregion

        #region Properties

        #region View Models

        [MpChildViewModel(typeof(MpClipTileViewModel), true)]
        public ObservableCollection<MpClipTileViewModel> Items { get; set; } = new ObservableCollection<MpClipTileViewModel>();

        public ObservableCollection<MpClipTileViewModel> PinnedItems { get; set; } = new ObservableCollection<MpClipTileViewModel>();

        [MpAffectsChild]
        public List<MpClipTileViewModel> SelectedItems {
            get {
                if(PinnedSelectedItems.Count > 0) {
                    return PinnedSelectedItems;
                }
                return Items.Where(ct => ct.IsSelected).OrderBy(x => x.LastSelectedDateTime).ToList();
            }
        }

        public List<MpClipTileViewModel> PinnedSelectedItems {
            get {
                return PinnedItems.Where(ct => ct.IsSelected).OrderBy(x => x.LastSelectedDateTime).ToList();
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

        #region MpIContextMenuItemViewModel Implementation

        public MpMenuItemViewModel MenuItemViewModel { 
            get {
                if(PrimaryItem == null || PrimaryItem.PrimaryItem == null) {
                    return new MpMenuItemViewModel();
                }
                var tagItems = MpTagTrayViewModel.Instance.AllTagViewModel.ContentMenuItemViewModel.SubItems;
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = @"_Copy",
                            IconResourceKey = Application.Current.Resources["CopyIcon"] as string,
                            Command = CopySelectedClipsCommand,
                            ShortcutType = MpShortcutType.CopySelection
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Paste",
                            IconResourceKey = Application.Current.Resources["PasteIcon"] as string,
                            Command = PasteSelectedClipsCommand,
                            ShortcutType = MpShortcutType.PasteSelectedItems
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Paste _Here",
                            IconResourceKey = Application.Current.Resources["PasteIcon"] as string,
                            Command = PasteCurrentClipboardIntoSelectedTileCommand,
                            ShortcutType = MpShortcutType.PasteSelectedItems
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },

                        new MpMenuItemViewModel() {
                            Header = @"_Delete",
                            IconResourceKey = Application.Current.Resources["DeleteIcon"] as string,
                            Command = DeleteSelectedClipsCommand,
                            ShortcutType = MpShortcutType.DeleteSelectedItems
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Rename",
                            IconResourceKey = Application.Current.Resources["RenameIcon"] as string,
                            Command = EditSelectedTitleCommand,
                            ShortcutType = MpShortcutType.EditTitle
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Edit",
                            IconResourceKey = Application.Current.Resources["EditContentIcon"] as string,
                            Command = EditSelectedContentCommand,
                            ShortcutType = MpShortcutType.EditContent
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Transform",
                            IconResourceKey = Application.Current.Resources["ToolsIcon"] as string,
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = "_Duplicate",
                                    IconResourceKey = Application.Current.Resources["DuplicateIcon"] as string,
                                    Command = DuplicateSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.Duplicate
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Merge",
                                    IconResourceKey = Application.Current.Resources["MergeIcon"] as string,
                                    Command = MergeSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.MergeSelectedItems
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Email",
                                    IconResourceKey = Application.Current.Resources["EmailIcon"] as string,
                                    Command = SendToEmailCommand,
                                    ShortcutType = MpShortcutType.SendToEmail
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Qr Code",
                                    IconResourceKey = Application.Current.Resources["QrIcon"] as string,
                                    Command = CreateQrCodeFromSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.CreateQrCode
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Audio",
                                    IconResourceKey = Application.Current.Resources["SpeakIcon"] as string,
                                    Command = SpeakSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.SpeakSelectedItem
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Web Search",
                                    IconResourceKey = Application.Current.Resources["SearchIcon"] as string,
                                    SubItems = new List<MpMenuItemViewModel>() {
                                        new MpMenuItemViewModel() {
                                            Header = "_Google",
                                            IconResourceKey = Application.Current.Resources["GoogleIcon"] as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.google.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "_Bing",
                                            IconResourceKey = Application.Current.Resources["BingIcon"] as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.bing.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "_DuckDuckGo",
                                            IconResourceKey = Application.Current.Resources["DuckGo"] as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://duckduckgo.com/?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "_Yandex",
                                            IconResourceKey = Application.Current.Resources["YandexIcon"] as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://yandex.com/search/?text="
                                        },
                                        new MpMenuItemViewModel() { IsSeparator = true},
                                        new MpMenuItemViewModel() {
                                            Header = "_Manage...",
                                            IconResourceKey = Application.Current.Resources["CogIcon"] as string
                                        },
                                    }
                                },
                                new MpMenuItemViewModel() {
                                    Header = $"'{PrimaryItem.PrimaryItem.SourceViewModel.AppViewModel.AppName}' to _Excluded App",
                                    IconId = PrimaryItem.PrimaryItem.SourceViewModel.AppViewModel.AppId,
                                    Command = ExcludeSubSelectedItemApplicationCommand
                                },
                                new MpMenuItemViewModel() {
                                    Header = PrimaryItem.PrimaryItem.SourceViewModel == null ||
                                             PrimaryItem.PrimaryItem.SourceViewModel.UrlViewModel == null? 
                                                null :
                                                $"'{PrimaryItem.PrimaryItem.SourceViewModel.UrlViewModel.UrlDomainPath}' to _Excluded Domain",
                                    IconId = PrimaryItem.PrimaryItem.SourceViewModel.UrlViewModel == null ?
                                                0 :
                                                PrimaryItem.PrimaryItem.SourceViewModel.UrlViewModel.IconId,
                                    IsVisible = PrimaryItem.PrimaryItem.SourceViewModel.UrlViewModel != null,
                                    Command = ExcludeSubSelectedItemUrlDomainCommand
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Into _Macro",
                                    IconResourceKey = Application.Current.Resources["RobotClawIcon"] as string,
                                    Command = MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand,
                                    CommandParameter = this
                                },
                                new MpMenuItemViewModel() {
                                    Header = string.IsNullOrEmpty(PrimaryItem.PrimaryItem.ShortcutKeyString) ? 
                                                "To _Shorcut" : $"Paste '{PrimaryItem.PrimaryItem.ShortcutKeyString}'",
                                    IconResourceKey = Application.Current.Resources["HotkeyIcon"] as string,
                                    Command = MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand,
                                    CommandParameter = this
                                },
                            }
                        },
                        MpAnalyticItemCollectionViewModel.Instance.MenuItemViewModel,
                        new MpMenuItemViewModel() {
                            Header = "_Select",
                            IconResourceKey = Application.Current.Resources["SelectionIcon"] as string,
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = "_Bring to Front",
                                    IconResourceKey = Application.Current.Resources["BringToFrontIcon"] as string,
                                    Command = BringSelectedClipTilesToFrontCommand,
                                    ShortcutType = MpShortcutType.BringSelectedToFront
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Send to Back",
                                    IconResourceKey = Application.Current.Resources["SendToBackIcon"] as string,
                                    Command = SendSelectedClipTilesToBackCommand,
                                    ShortcutType = MpShortcutType.SendSelectedToBack
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Select _All",
                                    IconResourceKey = Application.Current.Resources["SelectAllIcon"] as string,
                                    Command = SelectAllCommand,
                                    ShortcutType = MpShortcutType.SelectAll
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Invert Selection",
                                    IconResourceKey = Application.Current.Resources["InvertSelectionIcon"] as string,
                                    Command = InvertSelectionCommand,
                                    ShortcutType = MpShortcutType.InvertSelection
                                },
                            }
                        },
                        new MpMenuItemViewModel() {IsSeparator = true},
                        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(PrimaryItem.PrimaryItem),
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                            Header = @"Pin To _Collection",
                            IconResourceKey = Application.Current.Resources["PinToCollectionIcon"] as string,
                            SubItems = tagItems
                        }
                    },
                };
            }
        }

        #endregion

        #endregion

        #region Layout

        public double ClipTrayHeight => MpMainWindowViewModel.Instance.MainWindowHeight - MpMeasurements.Instance.TitleMenuHeight - MpMeasurements.Instance.FilterMenuHeight - MpSearchBoxViewModel.Instance.SearchCriteriaListBoxHeight;

        public double PinTrayScreenWidth { get; set; }

        public double PinTrayTotalWidth { get; set; } = 0;
        //public double ClipTrayScreenHeight => ClipTrayHeight;

        public double ClipTrayScreenWidth => MpMeasurements.Instance.ClipTrayDefaultWidth - PinTrayTotalWidth;

        public double MaxTileWidth => ClipTrayScreenWidth - (MpMeasurements.Instance.ClipTileMaxWidthPadding * 2);

        public double ClipTrayTotalTileWidth {
            get {
                if(TotalTilesInQuery == 0) {
                    return 0;
                }
                double lastTileWidth = 0;
                int lastTrayTileIdx = TotalTilesInQuery - 1;
                while(lastTrayTileIdx >= 0) {

                    int lastId = MpDataModelProvider.AllFetchedAndSortedCopyItemIds[lastTrayTileIdx];
                    if(PinnedItems.Any(x=>x.HeadItem.CopyItemId == lastId)) {
                        lastTrayTileIdx--;
                        continue;
                    }
                    lastTileWidth = PersistentUniqueWidthTileLookup.ContainsKey(lastId) ?
                                                PersistentUniqueWidthTileLookup[lastId] : MpMeasurements.Instance.ClipTileMinSize;
                    break;
                }
                
                if(lastTrayTileIdx < 0) {
                    return 0;
                }
                // subtract last tile width so at max last tile is fully visible at left
                return FindTileOffsetX(lastTrayTileIdx) + lastTileWidth + (MpMeasurements.Instance.ClipTileMargin * 2);// + ClipTrayScreenWidth;
            }
        }

        public double ClipTrayTotalWidth => Math.Max(ClipTrayScreenWidth, ClipTrayTotalTileWidth);

        public double MaximumScrollOfset {
            get {
                if (TotalTilesInQuery > MpMeasurements.Instance.DefaultTotalVisibleClipTiles) {
                    return ClipTrayTotalWidth - ClipTrayScreenWidth;// - PinTrayScreenWidth; //(MpMeasurements.Instance.TotalVisibleClipTiles * MpMeasurements.Instance.ClipTileMinSize);
                }
                return 0;
            }
        }
        #endregion

        #region Appearance

        public int MaxTotalVisibleClipTiles {
            get {
                return (int)Math.Ceiling(ClipTrayScreenWidth / MpMeasurements.Instance.ClipTileBorderMinWidth);
            }
        }

        #endregion

        #region Business Logic


        public int RemainingItemsCountThreshold { get; private set; }

        public int TotalTilesInQuery => MpDataModelProvider.TotalTilesInQuery;

        public int DefaultLoadCount => MaxTotalVisibleClipTiles + 3;

        public SelectionMode SelectionMode => SelectionMode.Single;

        #endregion

        #region State

        public bool IsAddingClipboardItem { get; private set; } = false;

        private bool _isPasting = false;
        public bool IsPasting { 
            get {
                if(_isPasting) {
                    return true;
                }
                if(Items.Any(x=>x.IsAnyPasting)) {
                    // NOTE since copy items can be pasted from hot key and aren't in tray
                    // IsPasting cannot be auto-property
                    _isPasting = true;
                }
                return _isPasting;
            }
            set {
                if(IsPasting != value) {
                    _isPasting = value;
                    OnPropertyChanged(nameof(IsPasting));
                }
            }
        }

        #region Virtual

        //set in civm IsSelected property change, DragDrop.Drop (copy mode)
        public List<MpCopyItem> PersistentSelectedModels { get; set; } = new List<MpCopyItem>();

        //<HeadCopyItemId, Unique ItemWidth> unique is != to MpMeausrements.Instance.ClipTileMinSize
        public Dictionary<int, double> PersistentUniqueWidthTileLookup { get; set; } = new Dictionary<int, double>();

        #endregion

        #region Mouse Modes

        public bool IsAnyMouseModeEnabled => IsAutoCopyMode || IsRightClickPasteMode;

        public bool IsAppendMode { get; set; }

        public bool IsAppendLineMode { get; set; }

        public bool IsAnyAppendMode => IsAppendMode || IsAppendLineMode;

        public bool IsAutoCopyMode { get; set; }

        public bool IsRightClickPasteMode { get; set; }

        public string MouseModeImageSourcePath {
            get {
                if (IsRightClickPasteMode && IsAutoCopyMode) {
                    return Application.Current.Resources["BothClickIcon"] as string;
                }
                if (IsRightClickPasteMode) {
                    return Application.Current.Resources["RightClickIcon"] as string;
                }
                if (IsAutoCopyMode) {
                    return Application.Current.Resources["LeftClickIcon"] as string;
                }
                return Application.Current.Resources["NoneClickIcon"] as string;
            }
        }

        #endregion

        public bool IsAppPaused { get; set; } = false;

        public bool IsThumbDragging { get; set; } = false;

        public bool IsAnyTilePinned => PinnedItems.Count > 0;

        public bool IsScrollingIntoView { get; set; } = false;
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

        public bool IsHorizontalScrollBarVisible => ClipTrayTotalTileWidth > ClipTrayScreenWidth; //TotalTilesInQuery > MpMeasurements.Instance.DefaultTotalVisibleClipTiles;

        public int HeadQueryIdx => Items.Count == 0 ? -1 : Items.Where(x=>x.QueryOffsetIdx >= 0).Min(x => x.QueryOffsetIdx);// Math.Max(0, Items.Min(x => x.QueryOffsetIdx));

        public int TailQueryIdx => Items.Count == 0 ? -1 : Items.Max(x => x.QueryOffsetIdx);// Math.Min(TotalTilesInQuery - 1, Items.Max(x => x.QueryOffsetIdx));

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

        public bool IsAnyResizing => Items.Any(x => x.IsResizing);

        public bool CanAnyResize => Items.Any(x => x.CanResize);

        public bool IsAnyEditing => Items.Any(x => x.IsContentAndTitleReadOnly == false);

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

        public bool CanScroll {
            get {
                if(PrimaryItem == null || PrimaryItem.PrimaryItem == null) {
                    return false;
                }
                if(!PrimaryItem.IsContentReadOnly && PrimaryItem.IsHovering) {
                    return false;
                }
                return true;
            }
        }

        public bool IsAnyEditingClipTitle => Items.Any(x => x.IsTitleReadOnly == false);

        public bool IsAnyEditingClipTile => Items.Any(x => x.IsContentReadOnly == false);

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

        public event EventHandler<MpCopyItem> OnCopyItemItemAdd;

        #endregion

        #region Constructors

        private static MpClipTrayViewModel _instance;
        public static MpClipTrayViewModel Instance => _instance ?? (_instance = new MpClipTrayViewModel());


        public MpClipTrayViewModel() : base(null) { }

        public async Task Init() {
            await MpHelpers.RunOnMainThreadAsync(() => {
                IsBusy = true;

                PropertyChanged += MpClipTrayViewModel_PropertyChanged;
                Items.CollectionChanged += Items_CollectionChanged;
                MpDataModelProvider.AllFetchedAndSortedCopyItemIds.CollectionChanged += AllFetchedAndSortedCopyItemIds_CollectionChanged;
                MpDb.SyncAdd += MpDbObject_SyncAdd;
                MpDb.SyncUpdate += MpDbObject_SyncUpdate;
                MpDb.SyncDelete += MpDbObject_SyncDelete;

                _pageSize = 1;
                RemainingItemsCountThreshold = 1;
                _oldMainWindowHeight = MpMainWindowViewModel.Instance.MainWindowHeight;
                //DefaultLoadCount = MpMeasurements.Instance.DefaultTotalVisibleClipTiles * 1 + 2;

                MpMessenger.Register<MpMessageType>(
                    MpDataModelProvider.QueryInfo, ReceivedQueryInfoMessage);


                MpClipboardHelper.MpClipboardManager.OnClipboardChange += ClipboardChanged;


                IsBusy = false;
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

        #region MpIMatchTrigger Implementation

        public void Register(MpActionViewModelBase mvm) {
            OnCopyItemItemAdd += mvm.OnActionTriggered;
            MpConsole.WriteLine($"ClipTray Registered {mvm.Label} matcher");
        }

        public void Unregister(MpActionViewModelBase mvm) {
            OnCopyItemItemAdd -= mvm.OnActionTriggered;
            MpConsole.WriteLine($"Matcher {mvm.Label} Unregistered from OnCopyItemAdded");
        }

        #endregion

        #region MpIClipboardContentDataProvider Implementation

        public async Task<string> GetClipboardContentData() {
            while(IsAddingClipboardItem) {
                try {
                    await Task.Delay(100).TimeoutAfter(TimeSpan.FromMilliseconds(3000));
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    return null;
                }                
            }
            if(_currentClipboardItem == null) {
                return null;
            }
            return _currentClipboardItem.ItemData;
        }
        
        #endregion

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
                bool isManualSort = MpDataModelProvider.QueryInfo.SortType == MpContentSortType.Manual;

                if (isManualSort) {
                    _manualSortOrderLookup = new Dictionary<int, int>();
                    foreach (var ctvm in Items) {
                        if (_manualSortOrderLookup.ContainsKey(ctvm.HeadItem.CopyItemId)) {
                            continue;
                        }
                        _manualSortOrderLookup.Add(ctvm.HeadItem.CopyItemId, Items.IndexOf(ctvm));
                    }
                }

                bool isDesc = MpDataModelProvider.QueryInfo.IsDescending;
                int tagId = MpDataModelProvider.QueryInfo.TagId;
                var citl = await MpDataModelProvider.GetCopyItemTagsForTagAsync(tagId);

                if (tagId == MpTag.AllTagId || tagId == MpTag.RecentTagId) {
                    //ignore sorting for sudo tags
                    return;
                }

                int count = isDesc ? citl.Count : 1;
                //loop through available tiles and reset tag's sort order, 
                //removing existing items from known ones and creating new ones if that's the case (it shouldn't)
                foreach (var ctvm in Items) {
                    foreach (var civm in ctvm.Items) {
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
            foreach (var ctvm in Items.ToList()) {
                if (ctvm == null) {
                    //occurs on first hide w/ async virtal items
                    continue;
                }
                ctvm.ClearEditing();
            }
        }

        public void ClearPinnedEditing() {
            foreach (var ctvm in PinnedItems) {
                if (ctvm == null) {
                    //occurs on first hide w/ async virtal items
                    continue;
                }
                ctvm.ClearEditing();
            }
        }

        public void ClearClipSelection(bool clearEditing = true) {
            MpHelpers.RunOnMainThread((Action)(() => {
                if (clearEditing) {
                    ClearClipEditing();
                }
                foreach (var ctvm in Items.ToList()) {
                    if (ctvm == null) {
                        //occurs on first hide w/ async virtal items
                        continue;
                    }
                    ctvm.ClearSelection();
                }

                PersistentSelectedModels.Clear();
            }));
        }

        public void ClearPinnedSelection(bool clearEditing = true) {
            MpHelpers.RunOnMainThread((Action)(() => {
                if (clearEditing) {
                    ClearPinnedEditing();
                }
                foreach (var ctvm in PinnedItems) {
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
            MpHelpers.RunOnMainThread(() => {
                ClearClipSelection(clearEditing);
                ClearPinnedSelection(clearEditing);

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
        

        public void ClipboardChanged(object sender, MpPortableDataObject mpdo) {
            if(MpMainWindowViewModel.Instance.IsMainWindowLoading || IsAppPaused) {
                return;
            }
            if(MpPlatformWrapper.Services.ClipboardMonitor.IgnoreNextClipboardChangeEvent) {
                MpPlatformWrapper.Services.ClipboardMonitor.IgnoreNextClipboardChangeEvent = false;
                return;
            }
            MpHelpers.RunOnMainThread(async () => {
                await AddItemFromClipboard(mpdo);
            });
        }

        public async Task<MpClipTileViewModel> CreateClipTileViewModel(List<MpCopyItem> cil, int queryOffsetIdx = -1) {
            var nctvm = new MpClipTileViewModel(this);
            await nctvm.InitializeAsync(cil,queryOffsetIdx);
            return nctvm;
        }


        public List<MpClipTileViewModel> GetClipTilesByAppId(int appId) {
            var ctvml = new List<MpClipTileViewModel>();
            foreach (MpClipTileViewModel ctvm in Items) {
                if (ctvm.Items.Any(x => x.SourceViewModel != null && x.SourceViewModel.AppViewModel.AppId == appId)) {
                    ctvml.Add(ctvm);
                }
            }
            return ctvml;
        }

        public MpContentItemViewModel GetContentItemViewModelById(int ciid) {
            foreach (var ctvm in PinnedItems) {
                foreach (var civm in ctvm.Items) {
                    var ortbvm = ctvm.Items.Where(x => x.CopyItemId == ciid).FirstOrDefault();
                    if (ortbvm != null) {
                        return ortbvm;
                    }
                }
            }
            foreach (var ctvm in Items) {
                foreach (var civm in ctvm.Items) {
                    var ortbvm = ctvm.Items.Where(x => x.CopyItemId == ciid).FirstOrDefault();
                    if (ortbvm != null) {
                        return ortbvm;
                    }
                }
            }
            return null;
        }

        public MpContentItemViewModel GetContentItemViewModelByGuid(string ciguid) {
            foreach (var ctvm in PinnedItems) {
                foreach (var civm in ctvm.Items) {
                    var ortbvm = ctvm.Items.Where(x => x.CopyItemGuid == ciguid).FirstOrDefault();
                    if (ortbvm != null) {
                        return ortbvm;
                    }
                }
            }
            foreach (var ctvm in Items) {
                foreach (var civm in ctvm.Items) {
                    var ortbvm = ctvm.Items.Where(x => x.CopyItemGuid == ciguid).FirstOrDefault();
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

        public MpClipTileViewModel GetClipTileViewModelByGuid(string ciguid) {
            var civm = GetContentItemViewModelByGuid(ciguid);
            if (civm == null) {
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
            //        MpHelpers.ConvertPlainTextToRichText("Take a moment to look through the available features in the following tiles, which are always available in the 'Help' pinboard"));

            //var introItem2 = new MpCopyItem(
            //    MpCopyItemType.RichText,
            //    "One place for your clipboard",
            //    MpHelpers.ConvertPlainTextToRichText(""));
            //MpPreferences.IsInitialLoad = false;
            //
        }

        public void NotifySelectionChanged() {
            MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
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
            var prevSelectedItems = tile.Items.Where(x =>
                                                PersistentSelectedModels.Any(y =>
                                                    y.Id == x.CopyItemId)).ToList();
            if (prevSelectedItems.Count == 0) {
                tile.ClearSelection();
                return;
            }

            IsRestoringSelection = true;

            tile.Items.ForEach(x => x.IsSelected = PersistentSelectedModels.Any(y=>y.Id == x.CopyItemId));

            IsRestoringSelection = false;
        }
        public double FindTileOffsetX2(int queryOffsetIdx) {
            int totalTileCount = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Count;
            if (totalTileCount <= 0) {
                return 0;
            }
            queryOffsetIdx = Math.Max(0, Math.Min(queryOffsetIdx, totalTileCount - 1));

            var headItemIds = MpDataModelProvider.AllFetchedAndSortedCopyItemIds;
            var uniqueWidthLookup = PersistentUniqueWidthTileLookup;

            double offsetX = 0;// MpMeasurements.Instance.ClipTileMargin;
            for (int i = 1; i <= queryOffsetIdx; i++) {
                int tileHeadId = headItemIds[i - 1];
                if (PinnedItems.Any(x => x.HeadItem?.CopyItemId == tileHeadId)) {
                    continue;
                }
                offsetX += MpMeasurements.Instance.ClipTileMargin * 2;

                if (uniqueWidthLookup.ContainsKey(tileHeadId)) {
                    offsetX += uniqueWidthLookup[tileHeadId];
                    //offsetX -= MpMeasurements.Instance.ClipTileMargin;
                } else {
                    offsetX += MpClipTileViewModel.DefaultBorderWidth;
                }
            }
            // NOTE after testing and comparing list
            return offsetX;
        }
        public double FindTileOffsetX(int queryOffsetIdx) {
            int totalTileCount = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Count;
            if (totalTileCount <= 0 || queryOffsetIdx <= 0) {
                return 0;
            }
            queryOffsetIdx = Math.Max(0, Math.Min(queryOffsetIdx, totalTileCount-1));

            var headItemIds = MpDataModelProvider.AllFetchedAndSortedCopyItemIds;
            var relevantUniqueWidthLookup = PersistentUniqueWidthTileLookup.Where(x => headItemIds.FastIndexOf(x.Key) < queryOffsetIdx && PinnedItems.All(y=>y.HeadItem.CopyItemId != x.Key)).ToDictionary(k=>k.Key,v=>v.Value);            
            int uniqueWidthCount = relevantUniqueWidthLookup.Count;

            int relevantPinnedDefaultWidthCount = PinnedItems.Where(x => headItemIds.FastIndexOf(x.HeadItem.CopyItemId) >= 0 && headItemIds.FastIndexOf(x.HeadItem.CopyItemId) < queryOffsetIdx).Count();
            double uniqueWidthSum = (uniqueWidthCount * (MpMeasurements.Instance.ClipTileMargin * 2)) + relevantUniqueWidthLookup.Sum(x => x.Value);

            int defaultWidthCount = Math.Max(0, queryOffsetIdx - uniqueWidthCount - relevantPinnedDefaultWidthCount);

            double defaultItemWidthWithMargin = (MpMeasurements.Instance.ClipTileMargin * 2) + MpClipTileViewModel.DefaultBorderWidth;
            double defaultWidthSum = defaultWidthCount * defaultItemWidthWithMargin;

            double offsetX = uniqueWidthSum + defaultWidthSum;
            return offsetX;
        }


        public int FindJumpTileIdx(double trackValue) {
            int totalTileCount = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Count;
            var headItemIds = MpDataModelProvider.AllFetchedAndSortedCopyItemIds;
            var uniqueWidthLookup = MpClipTrayViewModel.Instance.PersistentUniqueWidthTileLookup;

            double offsetX = 0;
            for (int i = 0; i < totalTileCount; i++) {
                offsetX += MpMeasurements.Instance.ClipTileMargin;
                int tileHeadId = headItemIds[i];


                if (uniqueWidthLookup.ContainsKey(tileHeadId)) {
                    offsetX += uniqueWidthLookup[tileHeadId];
                    //offsetX -= MpMeasurements.Instance.ClipTileMargin * 2;
                } else {
                    offsetX += MpClipTileViewModel.DefaultBorderWidth;
                }

                if (offsetX >= trackValue) {
                    return i;
                }
                offsetX += MpMeasurements.Instance.ClipTileMargin;
            }

            return totalTileCount - 1;
        }

        public void AdjustScrollOffsetToResize(double oldHeadTrayX, double oldScrollOfset) {
            double oldScrollOffsetDiffWithHead = oldScrollOfset - oldHeadTrayX;

            double newHeadTrayX = HeadItem == null ? 0 : HeadItem.TrayX;
            double headOffsetRatio = newHeadTrayX / oldHeadTrayX;
            headOffsetRatio = double.IsNaN(headOffsetRatio) ? 0 : headOffsetRatio;
            double newScrollOfsetDiffWithHead = headOffsetRatio * oldScrollOffsetDiffWithHead;
            double newScrollOfset = FindTileOffsetX(HeadQueryIdx) + newScrollOfsetDiffWithHead;

            //if(newScrollOfset < 100 || Math.Abs(newScrollOfset - oldScrollOfset) > 200) {
            //    Debugger.Break();
            //}
            ScrollOffset = newScrollOfset;
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

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                // NOTE content item is removed from tile in MpClipTileViewModel OnDelete db event handler
                if (PersistentSelectedModels.Any(x => x.Id == ci.Id)) {
                    PersistentSelectedModels.Remove(PersistentSelectedModels.FirstOrDefault(x => x.Id == ci.Id));
                }
                if (PersistentUniqueWidthTileLookup.Any(x => x.Key == ci.Id)) {
                    PersistentUniqueWidthTileLookup.Remove(ci.Id);
                }
                if (PinnedItems.Any(x => x.Items.Any(y => y.CopyItemId == ci.Id))) {
                    var pctvm = PinnedItems.FirstOrDefault(x => x.Items.Any(y => y.CopyItemId == ci.Id));
                    pctvm.Items.Remove(pctvm.Items.FirstOrDefault(x => x.CopyItemId == ci.Id));
                    if(pctvm.Items.Count == 0) {
                        PinnedItems.Remove(pctvm);
                    }
                    OnPropertyChanged(nameof(PinnedItems));
                }

                if (MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Contains(ci.Id)) {
                    //await MpDataModelProvider.RemoveQueryItem(ci.Id);
                    
                    //MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);

                    MpDataModelProvider.TotalItemCount--;
                }
                //OnPropertyChanged(nameof(TotalTilesInQuery));                
            } else if (e is MpCopyItemTag cit && Items.Any(x=>x.Items.Any(y => y.CopyItemId == cit.CopyItemId))) {
                var ctvm = Items.FirstOrDefault(x => x.Items.Any(y => y.CopyItemId == cit.CopyItemId));
                if(ctvm == null) {
                    return;
                }
                var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == cit.TagId);
                if(ttvm == null || !ttvm.IsSelected) {
                    return;
                }
                bool isAssociated = await ttvm.IsLinkedAsync(ctvm);
                if(isAssociated) {
                    return;
                }
                await MpDataModelProvider.RemoveQueryItem(cit.CopyItemId);
                MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);                
            }
        }

        #region Sync Events

        private void MpDbObject_SyncDelete(object sender, MpDbSyncEventArgs e) {
            MpHelpers.RunOnMainThread((Action)(() => {
                if (sender is MpCopyItem ci) {
                    var ctvmToRemove = GetContentItemViewModelById(ci.Id);
                    if (ctvmToRemove != null) {
                        ctvmToRemove.CopyItem.StartSync(e.SourceGuid);
                        //ctvmToRemove.CopyItem.Color.StartSync(e.SourceGuid);
                        ctvmToRemove.Parent.Items.Remove(ctvmToRemove);
                        if (ctvmToRemove.Parent.Items.Count == 0) {
                            Items.Remove(ctvmToRemove.Parent);
                        }
                        ctvmToRemove.CopyItem.EndSync();
                        //ctvmToRemove.CopyItem.Color.EndSync();
                    }
                }
            }));
        }

        private void MpDbObject_SyncUpdate(object sender, MpDbSyncEventArgs e) {
            MpHelpers.RunOnMainThread((Action)(() => {
            }));
        }

        private void MpDbObject_SyncAdd(object sender, MpDbSyncEventArgs e) {
            MpHelpers.RunOnMainThread(async () => {
                if (sender is MpCopyItem ci) {
                    ci.StartSync(e.SourceGuid);

                    var svm = MpSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == ci.SourceId);

                    var app = svm.AppViewModel.App;
                    app.StartSync(e.SourceGuid);
                    //ci.Source.App.Icon.StartSync(e.SourceGuid);
                    //ci.Source.App.Icon.IconImage.StartSync(e.SourceGuid);

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
                    app.EndSync();
                    //ci.Source.App.Icon.EndSync();
                    //ci.Source.App.Icon.IconImage.EndSync();
                    ci.EndSync();

                    ResetClipSelection();
                }
            }, DispatcherPriority.Background);
        }

        #endregion

        #endregion

        #region Private Methods

        private void CleanupAfterPasteSelected() {
            IsPastingHotKey = IsPastingSelected = IsPasting = false;
            foreach (var sctvm in SelectedItems) {
                //clean up pasted items state after paste
                if (sctvm.HasTemplates) {
                    sctvm.ClearEditing();
                    foreach (var rtbvm in sctvm.Items) {
                        rtbvm.TemplateCollection.ResetAll();
                        rtbvm.TemplateRichText = string.Empty;
                        rtbvm.RequestUiReset();
                    }
                    sctvm.RequestUiUpdate();
                    sctvm.RequestScrollToHome();
                }
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsTrayEmpty));
            if (e.OldItems != null) { //if (e.Action == NotifyCollectionChangedAction.Move && IsLoadingMore) {
                foreach (MpClipTileViewModel octvm in e.OldItems) {
                    octvm.Dispose();
                }
            }
        }

        private double _oldMainWindowHeight = 0;

        public void ReceivedResizerBehaviorMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ResizingContent:

                    double oldHeadTrayX = HeadItem == null ? 0 : HeadItem.TrayX;
                    double oldScrollOffset = ScrollOffset;

                    if (MpMainWindowViewModel.Instance.IsResizing) {
                        //main window resize


                        double deltaHeight = MpMainWindowViewModel.Instance.MainWindowHeight - _oldMainWindowHeight;
                        _oldMainWindowHeight = MpMainWindowViewModel.Instance.MainWindowHeight;

                        MpMeasurements.Instance.ClipTileMinSize += deltaHeight;
                        MpMeasurements.Instance.OnPropertyChanged(nameof(MpMeasurements.Instance.ClipTileTitleHeight));

                        MpClipTileViewModel.DefaultBorderWidth += deltaHeight;
                        MpClipTileViewModel.DefaultBorderHeight += deltaHeight;


                        Items.ForEach(x => x.TileBorderHeight = MpMeasurements.Instance.ClipTileMinSize);
                        Items.ForEach(x => x.TileBorderWidth += deltaHeight);

                        OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                        OnPropertyChanged(nameof(ClipTrayScreenWidth));
                        OnPropertyChanged(nameof(ClipTrayTotalWidth));
                        OnPropertyChanged(nameof(MaximumScrollOfset));

                        Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));




                    } else if(PrimaryItem != null && PrimaryItem.HeadItem != null) {
                        //tile resize

                        PersistentUniqueWidthTileLookup
                            .AddOrReplace(PrimaryItem.HeadItem.CopyItemId, PrimaryItem.TileBorderWidth);
                        //if (PrimaryItem.CanResize) {
                            Items.
                                Where(x => x.QueryOffsetIdx > PrimaryItem.QueryOffsetIdx).
                                ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
                        //}
                    }

                    //OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                    //OnPropertyChanged(nameof(ClipTrayScreenWidth));
                    //OnPropertyChanged(nameof(ClipTrayTotalWidth));
                    //OnPropertyChanged(nameof(MaximumScrollOfset));
                    AdjustScrollOffsetToResize(oldHeadTrayX, oldScrollOffset);


                    //OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                    //OnPropertyChanged(nameof(ClipTrayScreenWidth));
                    //OnPropertyChanged(nameof(ClipTrayTotalWidth));
                    //OnPropertyChanged(nameof(MaximumScrollOfset));
                    break;
                case MpMessageType.ResizeContentCompleted:

                    _oldMainWindowHeight = MpMainWindowViewModel.Instance.MainWindowHeight;
                    //double oldHeadTrayX2 = HeadItem == null ? 0 : HeadItem.TrayX;
                    //double oldScrollOffset2 = ScrollOffset;

                    //AdjustScrollOffsetToResize(oldHeadTrayX2, oldScrollOffset2);
                    //OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                    //OnPropertyChanged(nameof(ClipTrayScreenWidth));
                    //OnPropertyChanged(nameof(ClipTrayTotalWidth));
                    //OnPropertyChanged(nameof(MaximumScrollOfset));

                    //MpDragDropManager.DropTargets.ForEach(x => x.Reset());
                    break;
            }
        }



        private async Task OnPostMainWindowLoaded() {
            while (IsBusy || MpTagTrayViewModel.Instance.IsBusy) { await Task.Delay(100); }


            if (!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                // this ensures this only gets called once
                return;
            }

            int totalItems =  MpTagTrayViewModel.Instance.AllTagViewModel.TagClipCount;

            //MpNotificationCollectionView.Instance.BindingContext.PostLoadedMessage =
            //    $"Successfully loaded w/ {totalItems} items";

            //while(MpSoundPlayerCollectionViewModel.Instance.IsVisible) {
            //    await Task.Delay(100);
            //}
            MpSystemTrayViewModel.Instance.TotalItemCountLabel = string.Format(@"{0} total entries", totalItems);
            MpMainWindowViewModel.Instance.IsMainWindowLoading = false;

            await Task.Delay(3000);

            //MpNotificationCollectionView.Instance.CloseBalloon();
        }


        private void ReceivedQueryInfoMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.QueryChanged:
                    QueryCommand.Execute(null);
                    break;
                case MpMessageType.SubQueryChanged:
                    QueryCommand.Execute(HeadQueryIdx);
                    break;
            }

            if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                
                MpHelpers.RunOnMainThreadAsync(OnPostMainWindowLoaded);
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
                    if (IsThumbDragging) {
                        break;
                    }
                    foreach (MpClipTileViewModel nctvm in Items) {
                        nctvm.OnPropertyChanged(nameof(nctvm.TrayX));
                    }
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayScrollChanged);
                    break;
                case nameof(IsAppendMode):
                    MpNotificationCollectionViewModel.Instance.ShowMessage(string.Format("APPEND MODE: {0}", IsAppendMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
                    break;
            }
        }

        private async Task AddItemFromClipboard(MpPortableDataObject cd) {
            IsAddingClipboardItem = true;

            var totalAddSw = new Stopwatch();
            totalAddSw.Start();

            var createItemSw = new Stopwatch();
            createItemSw.Start();
            var newCopyItem = await MpCopyItemBuilder.CreateFromDataObject(cd);

            MpConsole.WriteLine("CreateFromClipboardAsync: " + createItemSw.ElapsedMilliseconds + "ms");

            if (newCopyItem == null) {
                //this occurs if the copy item is not a known format or app init
                MpConsole.WriteTraceLine("Unable to create copy item from clipboard!");
                IsAddingClipboardItem = false;
                return;
            }

            bool isDup = newCopyItem.Id < 0;
            newCopyItem.Id = isDup ? -newCopyItem.Id : newCopyItem.Id;

            if (IsAppendMode) {
                if(isDup) {
                    //when duplicate copied in append mode treat item as new and don't unlink original 
                    isDup = false;
                    newCopyItem.Id = 0;
                    newCopyItem.CopyDateTime = DateTime.Now;
                    await newCopyItem.WriteToDatabaseAsync();
                }
                //when in append mode just append the new items text to selecteditem
                if (_appendModeCopyItem == null) {
                    if (PrimaryItem == null) {
                        _appendModeCopyItem = newCopyItem;
                    } else {
                        _appendModeCopyItem = PrimaryItem.HeadItem.CopyItem;
                    }
                }

                if (_appendModeCopyItem != newCopyItem) {
                    int compositeChildCount = await MpDataModelProvider.GetCompositeChildCountAsync(_appendModeCopyItem.Id);
                    newCopyItem.CompositeParentCopyItemId = _appendModeCopyItem.Id;
                    newCopyItem.CompositeSortOrderIdx = compositeChildCount + 1;
                    await newCopyItem.WriteToDatabaseAsync();

                    if (MpPreferences.NotificationShowAppendBufferToast) {
                        // TODO now composite item doesn't roll up children so the buffer needs to be created here
                        // if I use this at all
                        MpStandardBalloonViewModel.ShowBalloon(
                            "Append Buffer",
                            SelectedItems[0].TailItem.CopyItem.ItemData.ToPlainText(),
                            MpPreferences.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                    }

                    if (MpPreferences.NotificationDoCopySound) {
                        MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                    }
                }

            } else {
                _appendModeCopyItem = null;
                if (MpPreferences.NotificationDoCopySound) {
                    MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                }
                if (MpPreferences.IsTrialExpired) {
                    MpStandardBalloonViewModel.ShowBalloon(
                        "Trial Expired",
                        "Please update your membership to use Monkey Paste",
                        MpPreferences.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                }
            }
            if (isDup) {
                //item is a duplicate
                MpConsole.WriteLine("Duplicate item detected, incrementing copy count and updating copydatetime");
                newCopyItem.CopyCount++;
                // reseting CopyDateTime will move item to top of recent list
                newCopyItem.CopyDateTime = DateTime.Now;
                await newCopyItem.WriteToDatabaseAsync();
            } else if (!MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                _newModels.Add(newCopyItem);

                MpTagTrayViewModel.Instance.AllTagViewModel.TagClipCount++;

                if (IsAppendMode) {
                    AppendNewItemsCommand.Execute(null);
                } else {
                    AddNewItemsCommand.Execute(null);
                }                    
            }

            while(IsBusy) {
                await Task.Delay(100);
            }
            if(_appendModeCopyItem != null) {
                _currentClipboardItem = _appendModeCopyItem;
            } else {
                _currentClipboardItem = newCopyItem;
            }
            IsAddingClipboardItem = false;

            OnCopyItemItemAdd?.Invoke(this, newCopyItem);
            totalAddSw.Stop();
            MpConsole.WriteLine("Time to create new copyitem: " + totalAddSw.ElapsedMilliseconds + " ms");

            
        }

        #region Sync Events

        #endregion

        #endregion

        #region Commands

        public ICommand ToggleTileIsPinnedCommand => new RelayCommand<object>(
            async(args) => {
                var pctvm = args as MpClipTileViewModel;
                MpClipTileViewModel resultTile = null;
                bool wasPinned = false;
                if (pctvm.IsPinned) {
                    PinnedItems.Remove(pctvm);
                    resultTile = Items.FirstOrDefault(x => x.QueryOffsetIdx == pctvm.QueryOffsetIdx);
                } else {
                    if(pctvm.QueryOffsetIdx >= 0) {
                        // only recreate tiles that are already in the tray (they won't be if new)
                        resultTile = await CreateClipTileViewModel(pctvm.Items.Select(x => x.CopyItem).ToList(), pctvm.QueryOffsetIdx);                        
                    } else {
                        resultTile = pctvm;
                    }
                    PinnedItems.Add(resultTile);
                    wasPinned = true;
                }
                
                if(resultTile != null) {
                    ClearClipSelection(false);
                    resultTile.IsSelected = true;
                    resultTile.OnPropertyChanged(nameof(resultTile.IsPinned));
                    resultTile.OnPropertyChanged(nameof(resultTile.IsPlaceholder));
                }

                pctvm.OnPropertyChanged(nameof(pctvm.IsPinned));
                Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));

                
                if(!IsAnyTilePinned) {
                    PinTrayTotalWidth = PinTrayScreenWidth = 0;
                }
                OnPropertyChanged(nameof(IsAnyTilePinned));
                OnPropertyChanged(nameof(ClipTrayScreenWidth));

                OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                OnPropertyChanged(nameof(ClipTrayScreenWidth));
                OnPropertyChanged(nameof(ClipTrayTotalWidth));
                OnPropertyChanged(nameof(MaximumScrollOfset));

                if(wasPinned) {
                    MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                }
            },
            (args) => args != null && 
                      (args is MpClipTileViewModel || 
                       args is List<MpClipTileViewModel>));


        public ICommand DuplicateSelectedClipsCommand => new RelayCommand(
            async () => {
                IsBusy = true;



                foreach (var sctvm in SelectedItems) {
                    foreach (var ivm in sctvm.SelectedItems) {
                        var clonedCopyItem = (MpCopyItem)await ivm.CopyItem.Clone(true);
                        if(ivm.CompositeSortOrderIdx > 0) {
                            ivm.Parent.Items
                                .Where(x => x.CompositeSortOrderIdx > ivm.CompositeSortOrderIdx)
                                .ForEach(x => x.CompositeSortOrderIdx++);
                        }

                        clonedCopyItem.CompositeSortOrderIdx = ivm.CompositeSortOrderIdx + 1;
                        clonedCopyItem.CompositeParentCopyItemId = ivm.CopyItemId;

                        await clonedCopyItem.WriteToDatabaseAsync();                        
                        _newModels.Add(clonedCopyItem);
                    }
                }


                //if (MpDataModelProvider.QueryInfo.TagId != MpTag.AllTagId) {
                //    //this occurs when appending within non-default tag

                //    foreach (var nci in _newModels) {
                //        await MpCopyItemTag.Create(
                //                MpDataModelProvider.QueryInfo.TagId,
                //                nci.Id);
                //    }
                //}

                AddNewItemsCommand.Execute(true);

                IsBusy = false;
            });

        public ICommand AppendNewItemsCommand => new RelayCommand(
            async() => {
                IsBusy = true;

                var amctvm = GetClipTileViewModelById(_appendModeCopyItem.Id);
                if (amctvm != null) {
                    await amctvm.InitializeAsync(new List<MpCopyItem>() { amctvm.HeadItem.CopyItem }, amctvm.QueryOffsetIdx);
                }

                IsBusy = false;
            },
            _appendModeCopyItem != null);

        public ICommand AddNewItemsCommand => new RelayCommand(
            async () => {
                IsBusy = MpMainWindowViewModel.Instance.IsMainWindowOpen;
                foreach (var ci in _newModels) {
                    var nctvm = await CreateClipTileViewModel(new List<MpCopyItem>() { ci });
                    while(nctvm.IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    ToggleTileIsPinnedCommand.Execute(nctvm);
                }

                _newModels.Clear();

                IsBusy = false;

                //using tray scroll changed so tile drop behaviors update their drop rects
                //MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayScrollChanged);
            },
            () => {
                if (_newModels.Count == 0) {
                    return false;
                }
                //if(!string.IsNullOrEmpty(MpSearchBoxViewModel.Instance.LastSearchText)) {
                //    return false;
                //}
                //if (MpDataModelProvider.QueryInfo.SortType == MpContentSortType.Manual) {
                //    return false;
                //}
                if (MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                    return true;
                }
                return false;
            });

        public ICommand QueryCommand => new RelayCommand<object>(
            async (offsetIdx_Or_ScrollOffset_Arg) => {
                if (!MpHelpers.IsOnMainThread()) {
                    MpHelpers.RunOnMainThread(()=>QueryCommand.Execute(offsetIdx_Or_ScrollOffset_Arg));
                    return;
                }

                IsBusy = IsRequery = true;
                var sw = new Stopwatch();
                sw.Start();
                bool isInPlaceRequery = offsetIdx_Or_ScrollOffset_Arg != null;
                int offsetIdx = 0;

                if(offsetIdx_Or_ScrollOffset_Arg != null) {
                    IsScrollJumping = true;
                    if (offsetIdx_Or_ScrollOffset_Arg is int) {
                        offsetIdx = (int)offsetIdx_Or_ScrollOffset_Arg;
                        //ScrollOffset = LastScrollOfset = FindTileOffsetX(offsetIdx);
                    } else if(offsetIdx_Or_ScrollOffset_Arg is double) {
                        //ScrollOffset = LastScrollOfset = (double)offsetIdx_Or_ScrollOffset_Arg;
                        offsetIdx = FindJumpTileIdx((double)offsetIdx_Or_ScrollOffset_Arg);
                    }
                    if(offsetIdx + DefaultLoadCount >= TotalTilesInQuery) {
                        offsetIdx = TotalTilesInQuery - DefaultLoadCount;
                        //ScrollOffset = LastScrollOfset = FindTileOffsetX(offsetIdx);
                    } 
                } else {
                    ClearClipSelection();

                    //ScrollOffset = LastScrollOfset = 0;

                    MpDataModelProvider.ResetQuery();

                    await MpDataModelProvider.QueryForTotalCount();
                }
                ScrollOffset = LastScrollOfset = FindTileOffsetX(offsetIdx);


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
                    var cil = await MpDataModelProvider.FetchCopyItemRangeAsync(offsetIdx, loadCount);

                    for (int i = 0; i < cil.Count; i++) {
                        if(PinnedItems.Any(x=>x.Items.Any(y => cil[i].Any(z=>z.Id == y.CopyItemId)))) {
                            continue;
                        }
                        await Items[i].InitializeAsync(cil[i], i + offsetIdx);


                        if (isInPlaceRequery) {
                            RestoreSelectionState(Items[i]);
                        }
                    }
                }

                while(Items.Any(x=>x.IsAnyBusy)) {
                    await Task.Delay(100);
                }

                if (SelectedItems.Count == 0 && 
                    PersistentSelectedModels.Count == 0 && 
                    TotalTilesInQuery > 0) {
                    ResetClipSelection();
                }

                if (IsScrollJumping) {
                    //MpMessenger.SendGlobal<MpMessageType>(MpMessageType.JumpToIdxCompleted);
                    IsScrollJumping = false;
                }
                

                OnPropertyChanged(nameof(TotalTilesInQuery));
                OnPropertyChanged(nameof(ClipTrayTotalWidth));
                OnPropertyChanged(nameof(MaximumScrollOfset));
                OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));

                IsBusy = IsRequery = false;

                MpMessenger.SendGlobal<MpMessageType>(MpMessageType.RequeryCompleted);

                sw.Stop();
                MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);

                //PrintInstanceCount();
            },(offsetIdx_Or_ScrollOffset_Arg) => !IsAnyBusy);

        public ICommand LoadMoreClipsCommand => new RelayCommand<object>(
            async (isLoadMore) => {
                IsBusy = IsLoadingMore = true;

                //if (IsAnyTileFlipped) {
                //    UnFlipAllTiles();
                //}

                bool isLeft = ((int)isLoadMore) >= 0;

                int loadCount = Math.Min(DefaultLoadCount, TotalTilesInQuery);

                //keep item count in sync with main window size
                int itemCountDiff = Items.Count - loadCount;
                if (itemCountDiff > 0) {
                    while (itemCountDiff > 0) {
                        int removeIdx = isLeft ? Items.Count - 1 : 0;
                        Items.RemoveAt(removeIdx);
                        itemCountDiff--;
                    }
                } else if (itemCountDiff < 0) {
                    while (itemCountDiff < 0) {
                        var ctvm = await CreateClipTileViewModel(null);
                        if (!isLeft) {
                            Items.Add(ctvm);
                        } else {
                            Items.Insert(0, ctvm);
                        }
                        itemCountDiff++;
                    }
                }

                if (isLeft && TailQueryIdx < TotalTilesInQuery - 1) {
                    int offsetIdx = TailQueryIdx + 1;
                    int fetchCount = _pageSize;

                    if (offsetIdx + fetchCount >= TotalTilesInQuery) {
                        fetchCount = TotalTilesInQuery - offsetIdx;
                    }
                    var cil = await MpDataModelProvider.FetchCopyItemRangeAsync(offsetIdx, fetchCount);

                    for (int i = 0; i < cil.Count; i++) {
                        if (PinnedItems.Any(x => x.Items.Any(y => cil[i].Any(z => z.Id == y.CopyItemId)))) {
                            continue;
                        }

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
                    var cil = await MpDataModelProvider.FetchCopyItemRangeAsync(offsetIdx, fetchCount);

                    for (int i = cil.Count - 1; i >= 0; i--) {
                        if (PinnedItems.Any(x => x.Items.Any(y => cil[i].Any(z => z.Id == y.CopyItemId)))) {
                            continue;
                        }

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
                       !IsAnyBusy &&
                       !IsLoadingMore &&
                       !IsScrollJumping &&
                       !MpMainWindowViewModel.Instance.IsMainWindowLoading;
            });

        public ICommand JumpToQueryIdxCommand => QueryCommand;
            //new RelayCommand<int>(
            //async (idx) => {
            //    if (idx < TailQueryIdx && idx > HeadQueryIdx) {
            //        MpMessenger.SendGlobal<MpMessageType>(MpMessageType.JumpToIdxCompleted);
            //        return;
            //    }

            //    IsBusy = true;
            //    IsScrollJumping = true;

            //    int loadCount = DefaultLoadCount;
            //    if (idx + loadCount > TotalTilesInQuery) {
            //        //loadCount = TotalVisibleClipTiles;
            //        idx = TotalTilesInQuery - loadCount;
            //    }
            //    ScrollOffset = LastScrollOfset = FindTileOffsetX(idx);

            //    var cil = await MpDataModelProvider.FetchCopyItemRangeAsync(idx, loadCount);

            //    for (int i = 0; i < cil.Count; i++) {
            //        if (PinnedItems.Any(x => x.Items.Any(y => cil[i].Any(z => z.Id == y.CopyItemId))) ||
            //            i >= Items.Count) {
            //            // NOTE checking i w/ item count is probably a side affect bug
            //            //of resizing window and tile/scoll offset and tray dimensions
            //            //not all updating
            //            continue;
            //        }
            //        if (Items[i].IsSelected) {
            //            StoreSelectionState(Items[i]);
            //            Items[i].ClearSelection();
            //        }
            //        await Items[i].InitializeAsync(cil[i], idx + i);
            //        RestoreSelectionState(Items[i]);
            //    }

            //    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.JumpToIdxCompleted);

            //    IsScrollJumping = false;
            //    IsBusy = false;
            //},
            //(idx) => {
            //    return idx >= 0 && idx <= TotalTilesInQuery && !IsAnyBusy;
            //});

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

        public ICommand ExcludeSubSelectedItemApplicationCommand => new RelayCommand(
            async () => {
                var avm = MpAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == PrimaryItem.PrimaryItem.SourceViewModel.AppViewModel.AppId);
                if(avm == null) {
                    return;
                }
                await avm.RejectApp();
            },
            PrimaryItem != null && PrimaryItem.PrimaryItem != null);

        public ICommand ExcludeSubSelectedItemUrlDomainCommand => new RelayCommand(
            async () => {
                var uvm = MpUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == PrimaryItem.PrimaryItem.SourceViewModel.UrlViewModel.UrlId);
                if(uvm == null) {
                    MpConsole.WriteTraceLine("Error cannot find url id: " + PrimaryItem.PrimaryItem.SourceViewModel.UrlViewModel.UrlId);
                    return;
                }
                await uvm.RejectUrlOrDomain(true);
            },
            PrimaryItem != null && PrimaryItem.PrimaryItem != null && PrimaryItem.PrimaryItem.SourceViewModel.UrlViewModel != null);

        public ICommand SearchWebCommand => new RelayCommand<object>(
            (args) => {
                string pt = string.Join(
                            Environment.NewLine, 
                            PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));

                MpHelpers.OpenUrl(args.ToString() + Uri.EscapeDataString(pt));
            }, (args) => args != null && args is string);

        public ICommand ScrollToHomeCommand => new RelayCommand(
             async() => {
                 while(IsAnyBusy) { await Task.Delay(100); }
                 //ClearClipSelection(false);
                 //RequeryCommand.Execute(null);
                 int firstTrayIdx = 0;
                 while (PinnedItems.Any(x => x.HeadItem.CopyItemId == MpDataModelProvider.AllFetchedAndSortedCopyItemIds[firstTrayIdx])) {
                     firstTrayIdx++;
                     if (firstTrayIdx >= TotalTilesInQuery) {
                         return;
                     }
                 }
                 QueryCommand.Execute(firstTrayIdx);
                 

                 await Task.Delay(100);
                 while (IsAnyBusy) { await Task.Delay(10); }

                 var fctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == firstTrayIdx);
                 if(fctvm != null) {
                     Items[firstTrayIdx].IsSelected = true;
                 }                 

             },
            () => !IsTrayEmpty);

        public ICommand ScrollToEndCommand => new RelayCommand(
            async () => {
                while (IsAnyBusy) { await Task.Delay(100); }

                int lastTrayIdx = TotalTilesInQuery - 1;
                while(PinnedItems.Any(x=>x.HeadItem.CopyItemId == MpDataModelProvider.AllFetchedAndSortedCopyItemIds[lastTrayIdx])) {
                    lastTrayIdx--;
                    if(lastTrayIdx < 0) {
                        return;
                    }
                }
                QueryCommand.Execute(lastTrayIdx);
                await Task.Delay(100);
                while (IsAnyBusy) { await Task.Delay(100); }

                //var last_ctvm = Items.Where(x=>!x.IsPlaceholder).Aggregate((a, b) => a.QueryOffsetIdx > b.QueryOffsetIdx ? a : b);

                ////ScrollOffset = LastScrollOfset = MaximumScrollOfset + PinTrayScreenWidth;
                //ClearClipSelection(false);
                //last_ctvm.IsSelected = true;
                var lctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == lastTrayIdx);
                if(lctvm != null) {
                    lctvm.IsSelected = true;
                }
            },
            () => !IsTrayEmpty);

        public ICommand ScrollToNextPageCommand => new RelayCommand(
             async() => {
                 int nextPageOffset = Math.Min(TotalTilesInQuery - 1, TailQueryIdx + 1);
                 JumpToQueryIdxCommand.Execute(nextPageOffset);
                 await Task.Delay(100);
                 while (IsScrollJumping) { await Task.Delay(10); }
                 if(Items.Where(x=>!x.IsPlaceholder).Count() == 0) {
                     return;
                 }
                 Items[0].IsSelected = true;                 
             },
            () => !IsBusy && !IsTrayEmpty);

        public ICommand ScrollToPreviousPageCommand => new RelayCommand(
            async() => {
                int prevPageOffset = Math.Max(0, HeadQueryIdx - 1);
                JumpToQueryIdxCommand.Execute(prevPageOffset);
                await Task.Delay(100);
                while (IsScrollJumping) { await Task.Delay(10); }
                //ScrollOffset = LastScrollOfset = ClipTrayTotalWidth;
                if (Items.Where(x => !x.IsPlaceholder).Count() == 0) {
                    return;
                }
                Items[0].IsSelected = true;
            },
            () => !IsBusy && !IsTrayEmpty);

        public ICommand ScrollUpCommand => new RelayCommand(
             () => {
                 PrimaryItem.ScrollUpCommand.Execute(null);
             },()=>SelectedItems.Count == 1);

        public ICommand ScrollDownCommand => new RelayCommand(
             () => {
                PrimaryItem.ScrollDownCommand.Execute(null);
            }, () => SelectedItems.Count == 1);

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
                            MpDataModelProvider.AllFetchedAndSortedCopyItemIds.IndexOf(x.Id))
                                .Max();
                    nextSelectQueryIdx = curRightMostSelectQueryIdx + 1;
                } else {
                    nextSelectQueryIdx = 0;
                }

                //if (nextSelectQueryIdx < TotalTilesInQuery) {
                //    if (needJump) {
                //        JumpToQueryIdxCommand.Execute(Math.Max(0,nextSelectQueryIdx - 3));
                //        while (IsAnyBusy) { await Task.Delay(10); }
                //    }

                //    if (TailQueryIdx - curRightMostSelectQueryIdx <= RemainingItemsCountThreshold + 1) {
                //        double nextOffset = FindTileOffsetX(nextSelectQueryIdx);
                //        double curOffset =
                //            curRightMostSelectQueryIdx >= 0 ?
                //                 FindTileOffsetX(curRightMostSelectQueryIdx) : 0;
                //        double offsetDiff = nextOffset - curOffset;

                //        //adding 10 to ensure loadmore
                //        ScrollOffset += offsetDiff + 10;
                //        //triggers load more...
                //        await Task.Delay(100);
                //        while (IsAnyBusy) { await Task.Delay(10); }
                //    }                    
                //}

                if (nextSelectQueryIdx < TotalTilesInQuery) {
                    int curItemIdx = curRightMostSelectQueryIdx < 0 ? -1 : Items.IndexOf(
                        Items.Where(x=>!x.IsPlaceholder).FirstOrDefault(x => x.QueryOffsetIdx == curRightMostSelectQueryIdx));
                    if (curItemIdx == Items.Where(x => !x.IsPlaceholder).Count() - 1) {
                        LoadMoreClipsCommand.Execute(1);
                        while (IsAnyBusy || HasScrollVelocity) {
                            await Task.Delay(100);
                        }
                        curItemIdx--;
                    }
                    int nextItemIdx = Math.Min(TotalTilesInQuery - 1, curItemIdx + 1);
                    if (Items[nextItemIdx].IsPinned) { 
                        while(Items[nextItemIdx].IsPinned) {
                            nextItemIdx++;
                            if(nextItemIdx >= TotalTilesInQuery) {
                                return;
                            }
                            if(nextItemIdx >= Items.Count) {
                                LoadMoreClipsCommand.Execute(1);
                                while (IsAnyBusy || HasScrollVelocity) {
                                    await Task.Delay(100);
                                }
                                nextItemIdx--;
                            }
                        }
                    }
                    //ClearClipSelection();
                    //Items[nextItemIdx].ResetSubSelection();
                    //StoreSelectionState(Items[nextItemIdx]);
                    Items[nextItemIdx].IsSelected = true;
                }
            },
            () => !IsAnyBusy && !HasScrollVelocity && !IsScrollingIntoView);

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
                            MpDataModelProvider.AllFetchedAndSortedCopyItemIds.IndexOf(x.Id))
                                .Min();
                    prevSelectQueryIdx = curLeftMostSelectQueryIdx - 1;
                } else {
                    prevSelectQueryIdx = TotalTilesInQuery - 1;
                }

                //if (prevSelectQueryIdx >= 0) {
                //    if (needJump) {
                //        JumpToQueryIdxCommand.Execute(Math.Max(0, prevSelectQueryIdx - 3));
                //        while (IsBusy) { await Task.Delay(10); }
                //    }

                //    if ((HeadQueryIdx == 0 && Math.Abs(ScrollOffset) > 0.01) ||
                //        (curLeftMostSelectQueryIdx - HeadQueryIdx <= 1)) {
                //        double prevOffset = FindTileOffsetX(prevSelectQueryIdx);
                //        double curOffset =
                //            curLeftMostSelectQueryIdx >= 0 ?
                //                 FindTileOffsetX(curLeftMostSelectQueryIdx) : 0;
                //        double offsetDiff = prevOffset - curOffset;

                //        ScrollOffset = Math.Max(0,ScrollOffset + offsetDiff);
                //        //triggers load more...
                //        await Task.Delay(100);
                //        while (IsBusy) { await Task.Delay(10); }
                //    }
                //}

                if (prevSelectQueryIdx >= 0) {

                    int curItemIdx = curLeftMostSelectQueryIdx < 0 ? 1 : Items.IndexOf(
                        Items.Where(x=>!x.IsPlaceholder).FirstOrDefault(x => x.QueryOffsetIdx == curLeftMostSelectQueryIdx));
                    if (curItemIdx == 0) {
                        LoadMoreClipsCommand.Execute(-1);
                        while (IsAnyBusy || HasScrollVelocity) {
                            await Task.Delay(100);
                        }
                    }
                    int prevItemIdx = Math.Max(0, curItemIdx - 1);
                    if (Items[prevItemIdx].IsPinned) {
                        while (Items[prevItemIdx].IsPinned) {
                            if(Items[prevItemIdx].QueryOffsetIdx == 0) {
                                if(PinnedItems.Count == 0) {
                                    return;
                                }
                                PinnedItems[PinnedItems.Count - 1].IsSelected = true;
                                return;
                            }
                            prevItemIdx--;
                            if (prevItemIdx < 0) {
                                LoadMoreClipsCommand.Execute(-1);
                                while (IsAnyBusy || HasScrollVelocity) {
                                    await Task.Delay(100);
                                }
                                prevItemIdx++;
                            }
                        }
                    }
                    //ClearClipSelection();
                    //Items[prevItemIdx].ResetSubSelection();
                    //StoreSelectionState(Items[prevItemIdx]);
                    Items[prevItemIdx].IsSelected = true;
                }
            },
            () => !IsAnyBusy && !HasScrollVelocity && !IsScrollingIntoView);

        public ICommand SelectAllCommand => new RelayCommand(
            () => {
                ClearClipSelection();
                foreach (var ctvm in Items) {
                    ctvm.IsSelected = true;
                }
            });

        public ICommand ChangeSelectedClipsColorCommand => new RelayCommand<object>(
             (hexStrOrBrush) => {
                 string hexStr = string.Empty;
                 if(hexStrOrBrush is Brush b) {
                     hexStr = b.ToHex();
                 } else if(hexStrOrBrush is string) {
                     hexStr = (string)hexStrOrBrush;
                 }
                PrimaryItem.PrimaryItem.ChangeColorCommand.Execute(hexStr.ToString());
            });

        public ICommand CopySelectedClipsCommand => new RelayCommand(
            async () => {
                //var ido = await GetDataObjectFromSelectedClips(false, false);
                //MpClipboardHelper.MpClipboardManager.SetDataObjectWrapper(ido);
                var ido = await MpWpfDataObjectHelper.Instance.GetCopyItemDataObjectAsync(PrimaryItem.PrimaryItem.CopyItem, false, null);
                MpPlatformWrapper.Services.DataObjectHelper.ConvertToPlatformClipboardDataObject(ido);
            });

        public ICommand PasteSelectedClipsCommand => new RelayCommand<object>(
            async(args) => {
                IsPasting = true;

                var pi = new MpProcessInfo() {
                    Handle = MpProcessManager.LastHandle,
                    ProcessPath = MpProcessManager.LastProcessPath
                };
                MpPasteToAppPathViewModel ptapvm = null;
                if (args != null && args is int appId && appId > 0) {
                    //when pasting to a user defined application
                    pi.Handle = IntPtr.Zero;
                    ptapvm = MpPasteToAppPathViewModelCollection.Instance.FindById(appId);
                    if(ptapvm != null) {
                        pi.ProcessPath = ptapvm.AppPath;
                        pi.IsAdmin = ptapvm.IsAdmin;
                        pi.IsSilent = ptapvm.IsSilent;
                        pi.ArgumentList = new List<string>() { ptapvm.Args };
                        pi.WindowState = ptapvm.WindowState;
                    }
                } else if (args != null && args is IntPtr handle && handle != IntPtr.Zero) {
                    //when pasting to a running application
                    pi.Handle = handle;
                    ptapvm = null;
                } 
                
                //SelectedItems.ForEach(x => x.SubSelectAll());
                //In order to paste the app must hide first 
                //this triggers hidewindow to paste selected items
                IsPastingSelected = true;
                await MpWpfDataObjectHelper.Instance.PasteCopyItem(PrimaryItem.PrimaryItem.CopyItem, pi, ptapvm == null ? false : ptapvm.PressEnter);
                //MpMainWindowViewModel.Instance.HideWindowCommand.Execute(true);
                CleanupAfterPasteSelected();
            },
            (args) => {
                return MpMainWindowViewModel.Instance.IsShowingDialog == false &&
                    !IsAnyEditingClipTile &&
                    !IsAnyEditingClipTitle &&
                    !IsAnyPastingTemplate &&
                    !MpPreferences.IsTrialExpired;
            });

        public ICommand PasteCurrentClipboardIntoSelectedTileCommand => new RelayCommand(
            async() => {
                while (IsAddingClipboardItem) {
                    // wait in case tray is still processing the data
                    await Task.Delay(100);
                }

                // NOTE even though re-creating paste object here the copy item
                // builder should recognize it as a duplicate and use original (just created)
                var wpfdo = MpPlatformWrapper.Services.DataObjectHelper.GetDataObjectWrapper();
                var mpdo = MpPlatformWrapper.Services.DataObjectHelper.ConvertToSupportedPortableFormats(wpfdo);

                PrimaryItem.RequestPastePortableDataObject(mpdo);
            }, PrimaryItem != null && !PrimaryItem.IsPlaceholder);

        public ICommand PasteCopyItemByIdCommand => new RelayCommand<object>(
            async (args) => {
                IsPasting = true;

                var pi = new MpProcessInfo() {
                    Handle = MpProcessManager.LastHandle,
                    ProcessPath = MpProcessManager.LastProcessPath
                };
                int[] ciidl = args is int[]? args as int[] : new int[] { (int)args };

                var cil = await MpDataModelProvider.GetCopyItemsByIdList(ciidl.ToList());
                //var pasteDataObject = GetDataObjectByCopyItems(cil, false, true);
                //MpMainWindowViewModel.Instance.HideWindowCommand.Execute(pasteDataObject);
                await MpWpfDataObjectHelper.Instance.PasteCopyItem(cil[0], pi, false);

                IsPasting = false;
            },
            (args) => args != null && (args is int || args is int[]));

        public ICommand BringSelectedClipTilesToFrontCommand => new RelayCommand(
            async () => {
                IsBusy = true;
                var sil = PrimaryItem.SelectedItems;
                sil.Reverse();
                foreach (var scivm in sil) {
                    PrimaryItem.Items.Move(PrimaryItem.Items.IndexOf(scivm), 0);
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
                    if (!PrimaryItem.SelectedItems.Contains(PrimaryItem.Items[i])) {
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
                    PrimaryItem.Items.Move(PrimaryItem.Items.IndexOf(scivm), PrimaryItem.Count - 1);
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
                    if (!PrimaryItem.SelectedItems.Contains(PrimaryItem.Items[PrimaryItem.Items.Count - 1 - i])) {
                        canSendBack = true;
                        break;
                    }
                }
                return canSendBack;
            });

        public ICommand DeleteSelectedClipsCommand => new RelayCommand(
            async () => {
                while(IsBusy) { await Task.Delay(100); }

                IsBusy = true;

                //await MpDataModelProvider.RemoveQueryItem(PrimaryItem.PrimaryItem.CopyItemId);
                

                await Task.WhenAll(SelectedModels.Select(x => x.DeleteFromDatabaseAsync()));

                //db delete event is handled in clip tile
                IsBusy = false;
            },
            () => {
                return MpMainWindowViewModel.Instance.IsShowingDialog == false &&
                        SelectedModels.Count > 0 &&
                        !IsAnyEditingClipTile &&
                        !IsAnyEditingClipTitle &&
                        !IsAnyPastingTemplate;
            });

        public ICommand LinkTagToCopyItemCommand => new RelayCommand<MpTagTileViewModel>(
            async (tagToLink) => {
                var ctvm = PrimaryItem;
                var civm = PrimaryItem.PrimaryItem;
                bool isUnlink = await tagToLink.IsLinkedAsync(civm);

                if(isUnlink) {
                    // NOTE item is removed from ui from db ondelete event
                    await tagToLink.RemoveContentItem(civm.CopyItemId);
                } else {
                    await tagToLink.AddContentItem(civm.CopyItemId);
                }


                await civm.TitleSwirlViewModel.InitializeAsync();
                await MpTagTrayViewModel.Instance.UpdateTagAssociation();
            },
            (tagToLink) => {
                //this checks the selected clips association with tagToLink
                //and only returns if ALL selecteds clips are linked or unlinked 
                if (tagToLink == null || SelectedItems == null || SelectedItems.Count != 1 || PrimaryItem.SelectedItems.Count != 1) {
                    return false;
                }
                return true;
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
                SelectedItems[0].IsContentReadOnly = false;
            },
            () => {
                if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return false;
                }
                return SelectedItems.Count == 1 && (!SelectedItems[0].IsPlaceholder || SelectedItems[0].IsPinned);
            });

        public ICommand SendToEmailCommand => new RelayCommand(
            () => {
                // for gmail see https://stackoverflow.com/a/60741242/105028
                string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                MpHelpers.OpenUrl(
                    string.Format("mailto:{0}?subject={1}&body={2}",
                    string.Empty, SelectedItems[0].HeadItem.CopyItem.Title,
                    pt));
                //MpClipTrayViewModel.Instance.ClearClipSelection();
                //IsSelected = true;
                //MpHelpers.CreateEmail(MpPreferences.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
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
                       SelectedItems[0].Items.Count > 1 &&
                       SelectedItems.All(x => x.ItemType == SelectedItems[0].ItemType) &&
                       (SelectedItems[0].ItemType == MpCopyItemType.Text);
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
             () => {
                MpHelpers.RunOnMainThreadAsync(() => {
                    BitmapSource bmpSrc = null;
                    string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                    bmpSrc = MpHelpers.ConvertUrlToQrCode(pt);
                    //MpClipboardHelper.MpClipboardManager.SetDataObjectWrapper(
                    //    new MpDataObject() {
                    //        DataFormatLookup = new Dictionary<MpClipboardFormatType, string>() { 
                    //            { 
                    //                MpClipboardFormatType.Bitmap, 
                    //                bmpSrc.ToBase64String() 
                    //            } 
                    //        }
                    //});
                });
            },
            () => {
                string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                return (GetSelectedClipsType() == MpCopyItemType.Text) &&
                    pt.Length <= MpPreferences.MaxQrCodeCharLength;
            });

        public ICommand SpeakSelectedClipsCommand => new RelayCommand(
            async () => {
                await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                    var speechSynthesizer = new SpeechSynthesizer();
                    speechSynthesizer.SetOutputToDefaultAudioDevice();
                    if (string.IsNullOrEmpty(MpPreferences.SpeechSynthVoiceName)) {
                        speechSynthesizer.SelectVoice(speechSynthesizer.GetInstalledVoices()[0].VoiceInfo.Name);
                    } else {
                        speechSynthesizer.SelectVoice(MpPreferences.SpeechSynthVoiceName);
                    }
                    speechSynthesizer.Rate = 0;
                    speechSynthesizer.SpeakCompleted += (s, e) => {
                        speechSynthesizer.Dispose();
                    };
                    // Create a PromptBuilder object and append a text string.
                    PromptBuilder promptBuilder = new PromptBuilder();

                    foreach (var sctvm in SelectedItems) {
                        foreach (var ivm in sctvm.Items) {
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
                var preset = await MpDb.GetItemAsync<MpAnalyticItemPreset>((int)presetId);
                var analyticItemVm = MpAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AnalyzerPluginGuid == preset.AnalyzerPluginGuid);
                var presetVm = analyticItemVm.Items.FirstOrDefault(x => x.Preset.Id == preset.Id);                

                var prevSelectedPresetVm = analyticItemVm.SelectedItem;
                analyticItemVm.SelectPresetCommand.Execute(presetVm);
                analyticItemVm.ExecuteAnalysisCommand.Execute(PrimaryItem.PrimaryItem);
            });

        public ICommand ToggleIsAppPausedCommand => new RelayCommand(
            () => {
                IsAppPaused = !IsAppPaused;
            });

        public ICommand ToggleRightClickPasteCommand => new RelayCommand(
            () => {
                IsRightClickPasteMode = !IsRightClickPasteMode;
            }, !IsAppPaused);

        public ICommand ToggleAutoCopyModeCommand => new RelayCommand(
            () => {
                IsAutoCopyMode = !IsAutoCopyMode;
            }, !IsAppPaused);

        public ICommand ToggleAppendModeCommand => new RelayCommand(
            () => {
                IsAppendMode = !IsAppendMode;
                if (IsAppendMode && IsAppendLineMode) {
                    IsAppendLineMode = false;
                }
            }, !IsAppPaused);

        public ICommand ToggleAppendLineModeCommand => new RelayCommand(
            () => {
                IsAppendLineMode = !IsAppendLineMode;
                if (IsAppendLineMode && IsAppendMode) {
                    IsAppendMode = false;
                }
            }, !IsAppPaused);


        #endregion
    }
    public enum MpExportType {
        None = 0,
        Files,
        Csv,
        Zip
    }
}
