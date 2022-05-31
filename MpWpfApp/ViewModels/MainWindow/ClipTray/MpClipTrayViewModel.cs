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
using static OpenTK.Graphics.OpenGL.GL;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpClipTrayViewModel : 
        MpSelectorViewModelBase<object,MpClipTileViewModel>, 
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
        //public ObservableCollection<MpClipTileViewModel> Items { get; set; } = new ObservableCollection<MpClipTileViewModel>();

        public ObservableCollection<MpClipTileViewModel> PinnedItems { get; set; } = new ObservableCollection<MpClipTileViewModel>();


        public List<MpClipTileViewModel> PinnedSelectedItems {
            get {
                return PinnedItems.Where(ct => ct.IsSelected).OrderBy(x => x.LastSelectedDateTime).ToList();
            }
        }

        public MpClipTileViewModel HeadItem => Items.Count > 0 ? Items[0] : null;

        public MpClipTileViewModel TailItem => Items.Count > 0 ? Items[Items.Count - 1] : null;


        public List<MpCopyItem> SelectedModels {
            get {
                if(SelectedItem == null) {
                    return new List<MpCopyItem>();
                }
                return new List<MpCopyItem>() {
                    SelectedItem.CopyItem
                };
            }
        }        

        #region MpIContextMenuItemViewModel Implementation

        public MpMenuItemViewModel MenuItemViewModel { 
            get {
                if(SelectedItem == null) {
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
                                    Header = @"_Find and Replace",
                                    IconResourceKey = Application.Current.Resources["SearchIcon"] as string,
                                    Command = FindAndReplaceSelectedItem,
                                    ShortcutType = MpShortcutType.FindAndReplaceSelectedItem
                                },
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
                                    IconResourceKey = Application.Current.Resources["WebIcon"] as string,
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
                                    Header = $"'{SelectedItem.AppViewModel.AppName}' to _Excluded App",
                                    IconId = SelectedItem.AppViewModel.AppId,
                                    Command = ExcludeSubSelectedItemApplicationCommand
                                },
                                new MpMenuItemViewModel() {
                                    Header = SelectedItem.SourceViewModel == null ||
                                             SelectedItem.UrlViewModel == null? 
                                                null :
                                                $"'{SelectedItem.UrlViewModel.UrlDomainPath}' to _Excluded Domain",
                                    IconId = SelectedItem.UrlViewModel == null ?
                                                0 :
                                                SelectedItem.UrlViewModel.IconId,
                                    IsVisible = SelectedItem.UrlViewModel != null,
                                    Command = ExcludeSubSelectedItemUrlDomainCommand
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Into _Macro",
                                    IconResourceKey = Application.Current.Resources["RobotClawIcon"] as string,
                                    Command = MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand,
                                    CommandParameter = this
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Shorcut",
                                    ShortcutType = MpShortcutType.PasteCopyItem,
                                    ShortcutObjId = SelectedItem.CopyItemId,
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
                        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(SelectedItem),
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

        public double PinTrayMaxDefaultMaxWidth => MpMeasurements.Instance.ClipTrayDefaultWidth / 2;
        //public double ClipTrayScreenHeight => ClipTrayHeight;

        // NOTE ClipTrayScreenWidth is only set on initial load but then set by OneWayToSource Binding in MpClipTrayContainerView
        public double ClipTrayScreenWidth { get; set; } = MpMeasurements.Instance.ClipTrayDefaultWidth;

        public double MaxTileWidth => ClipTrayScreenWidth - (MpMeasurements.Instance.ClipTileMaxWidthPadding * 2);

        public double ClipTrayTotalTileWidth {
            get {
                int lastTrayTileIdx = MaxClipTrayQueryIdx;
                if(TotalTilesInQuery == 0 || lastTrayTileIdx < 0) {
                    return 0;
                }
                double lastTileWidth = PersistentUniqueWidthTileLookup.ContainsKey(MaxClipTrayQueryIdx) ?
                                                PersistentUniqueWidthTileLookup[MaxClipTrayQueryIdx] : 
                                                MpMeasurements.Instance.ClipTileMinSize;
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


        #endregion

        #region State

        public bool HasUserAlteredPinTrayWidth { get; set; } = false;

        public bool IsAddingClipboardItem { get; private set; } = false;

        private bool _isPasting = false;
        public bool IsPasting { 
            get {
                if(_isPasting) {
                    return true;
                }
                if(Items.Any(x=>x.IsPasting)) {
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

        public double LastScrollOffset { get; set; } = 0;

        private double _scrollOffset = 0;
        public double ScrollOffset {
            get { 
                return _scrollOffset;
            }
            set {
                if (_scrollOffset != value) {
                    LastScrollOffset = _scrollOffset;
                    _scrollOffset = value;
                    OnPropertyChanged(nameof(ScrollOffset));
                }
            }
        }

        public bool IsHorizontalScrollBarVisible => ClipTrayTotalTileWidth > ClipTrayScreenWidth; //TotalTilesInQuery > MpMeasurements.Instance.DefaultTotalVisibleClipTiles;

        public int HeadQueryIdx => Items.Count == 0 ? -1 : Items.Where(x=>x.QueryOffsetIdx >= 0).Min(x => x.QueryOffsetIdx);// Math.Max(0, Items.Min(x => x.QueryOffsetIdx));

        public int TailQueryIdx => Items.Count == 0 ? -1 : Items.Max(x => x.QueryOffsetIdx);// Math.Min(TotalTilesInQuery - 1, Items.Max(x => x.QueryOffsetIdx));

        public int MaxLoadQueryIdx => Math.Max(0,MaxClipTrayQueryIdx - DefaultLoadCount + 1);

        public int MaxClipTrayQueryIdx {
            get {
                int maxClipTrayQueryIdx = TotalTilesInQuery - 1;
                if(maxClipTrayQueryIdx < 0) {
                    return maxClipTrayQueryIdx;
                }
                while(PinnedItems.Any(x=>x.QueryOffsetIdx == maxClipTrayQueryIdx)) {
                    if(maxClipTrayQueryIdx < 0) {
                        return maxClipTrayQueryIdx;
                    }
                    maxClipTrayQueryIdx--;
                }
                return maxClipTrayQueryIdx;
            }
        }

        public int MinClipTrayQueryIdx {
            get {
                if(TotalTilesInQuery == 0) {
                    return -1;
                }
                int minClipTrayQueryIdx = 0;
                
                while (PinnedItems.Any(x => x.QueryOffsetIdx == minClipTrayQueryIdx)) {
                    if (minClipTrayQueryIdx >= TotalTilesInQuery) {
                        return -1;
                    }
                    minClipTrayQueryIdx++;
                }
                return minClipTrayQueryIdx;
            }
        }

        public bool IsArrowSelecting { get; set; } = false;


        public bool IsAnyBusy => Items.Any(x => x.IsAnyBusy) || PinnedItems.Any(x=>x.IsAnyBusy) || IsBusy;

        public bool IsRequery { get; private set; } = false;

        public bool IsTrayEmpty => Items.Count == 0 && 
                                   !IsRequery && !MpMainWindowViewModel.Instance.IsMainWindowLoading;// || Items.All(x => x.IsPlaceholder);

        public bool IsSelectionReset { get; set; } = false;

        public bool IgnoreSelectionReset { get; set; } = false;


        public bool IsAnyTileContextMenuOpened => Items.Any(x => x.IsContextMenuOpen);

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

        public bool IsAnyHovering => Items.Any(x => x.IsHovering);

        public bool CanScroll {
            get {
                if(SelectedItem == null) {
                    return true;
                }
                if(SelectedItem.IsVerticalScrollbarVisibile &&
                    SelectedItem.IsHovering &&
                    SelectedItem.IsVisible) {
                    return false;
                }
                return true;
            }
        }

        public bool IsAnyEditingClipTitle => Items.Any(x => x.IsTitleReadOnly == false);

        public bool IsAnyEditingClipTile => Items.Any(x => x.IsContentReadOnly == false);

        public bool IsAnyPastingTemplate => Items.Any(x => x.IsPastingTemplate);
        
            
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

        #endregion

        #region Public Methods

        #region MpIMatchTrigger Implementation

        public void RegisterActionComponent(MpIActionComponentHandler mvm) {
            OnCopyItemItemAdd += mvm.OnActionTriggered;
            MpConsole.WriteLine($"ClipTray Registered {mvm.Label} matcher");
        }

        public void UnregisterActionComponent(MpIActionComponentHandler mvm) {
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
                        if (_manualSortOrderLookup.ContainsKey(ctvm.CopyItemId)) {
                            continue;
                        }
                        _manualSortOrderLookup.Add(ctvm.CopyItemId, Items.IndexOf(ctvm));
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
                    MpCopyItemTag cit = citl.Where(x => x.CopyItemId == ctvm.CopyItem.Id).FirstOrDefault();
                    if (cit == null) {
                        cit = await MpCopyItemTag.Create(tagId, (int)ctvm.CopyItem.Id, count);
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
            if (SelectedItem == null) {
                return MpCopyItemType.None;
            }
            return SelectedItem.ItemType;
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

        public async Task<MpClipTileViewModel> CreateClipTileViewModel(MpCopyItem ci, int queryOffsetIdx = -1) {
            var nctvm = new MpClipTileViewModel(this);
            await nctvm.InitializeAsync(ci,queryOffsetIdx);
            return nctvm;
        }


        public MpClipTileViewModel GetClipTileViewModelById(int ciid) {
            var pctvm = PinnedItems.FirstOrDefault(x => x.CopyItemId == ciid);
            if(pctvm != null) {
                return pctvm;
            }
            return Items.FirstOrDefault(x => x.CopyItemId == ciid);
        }

        public MpClipTileViewModel GetClipTileViewModelByGuid(string ciguid) {
            var pctvm = PinnedItems.FirstOrDefault(x => x.CopyItemGuid == ciguid);
            if (pctvm != null) {
                return pctvm;
            }
            return Items.FirstOrDefault(x => x.CopyItemGuid == ciguid);
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

            PersistentSelectedModels = new List<MpCopyItem>() { tile.CopyItem };
        }

        public void RestoreSelectionState(MpClipTileViewModel tile) {
            var prevSelectedItems = PersistentSelectedModels
                                        .Where(y => y.Id == tile.CopyItemId).ToList();
            if (prevSelectedItems.Count == 0) {
                tile.ClearSelection();
                return;
            }

            IsRestoringSelection = true;

            SelectedItem = tile;

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

            double offsetX = 0;
            for (int i = 1; i <= queryOffsetIdx; i++) {
                int tileHeadId = headItemIds[i - 1];
                if (PinnedItems.Any(x => x.CopyItemId == tileHeadId)) {
                    continue;
                }
                offsetX += MpMeasurements.Instance.ClipTileMargin * 2;

                if (uniqueWidthLookup.ContainsKey(tileHeadId)) {
                    offsetX += uniqueWidthLookup[tileHeadId];
                } else {
                    offsetX += MpClipTileViewModel.DefaultBorderWidth;
                }
            }
            return offsetX;
        }
        public double FindTileOffsetX(int queryOffsetIdx) {
            int totalTileCount = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Count;
            if (totalTileCount <= 0 || queryOffsetIdx <= 0) {
                return 0;
            }
            queryOffsetIdx = Math.Max(0, Math.Min(queryOffsetIdx, totalTileCount-1));

            var relevantUniqueWidthLookup = PersistentUniqueWidthTileLookup.Where(x => x.Key < queryOffsetIdx && PinnedItems.All(y=>y.QueryOffsetIdx != x.Key)).ToDictionary(k=>k.Key,v=>v.Value);            
            int uniqueWidthCount = relevantUniqueWidthLookup.Count;

            int relevantPinnedDefaultWidthCount = PinnedItems.Where(x => x.QueryOffsetIdx >= 0 && x.QueryOffsetIdx < queryOffsetIdx).Count();
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
                if(PinnedItems.Any(x=>x.QueryOffsetIdx == i)) {
                    continue;
                }
                offsetX += MpMeasurements.Instance.ClipTileMargin;


                if (uniqueWidthLookup.ContainsKey(i)) {
                    offsetX += uniqueWidthLookup[i];
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
            headOffsetRatio = double.IsNaN(headOffsetRatio) ? 1 : headOffsetRatio;
            double newScrollOfsetDiffWithHead = headOffsetRatio * oldScrollOffsetDiffWithHead;
            double newScrollOfset = FindTileOffsetX(HeadQueryIdx) + newScrollOfsetDiffWithHead;

            //if(newScrollOfset < 100 || Math.Abs(newScrollOfset - oldScrollOfset) > 200) {
            //    Debugger.Break();
            //}
            ScrollOffset = newScrollOfset;
        }

        public void ForceScrollOffset(double newOffset) {
            _scrollOffset = LastScrollOffset = newOffset;
            OnPropertyChanged(nameof(ScrollOffset));
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
                var ivm = GetClipTileViewModelById(ci.Id);
                //ivm.CopyItem = ci;
            }
        }

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                // NOTE content item is removed from tile in MpClipTileViewModel OnDelete db event handler
                if (PersistentSelectedModels.Any(x => x.Id == ci.Id)) {
                    PersistentSelectedModels.Remove(PersistentSelectedModels.FirstOrDefault(x => x.Id == ci.Id));
                }
                int queryOffset = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.FastIndexOf(ci.Id);
                if (PersistentUniqueWidthTileLookup.ContainsKey(queryOffset)) {
                    PersistentUniqueWidthTileLookup.Remove(queryOffset);
                }

                if (MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Contains(ci.Id)) {
                    //await MpDataModelProvider.RemoveQueryItem(ci.Id);
                    
                    //MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);

                    MpDataModelProvider.TotalItemCount--;
                }
                OnPropertyChanged(nameof(TotalTilesInQuery));                
            } else if (e is MpCopyItemTag cit && Items.Any(x=>x.CopyItemId == cit.CopyItemId)) {
                var ctvm = Items.FirstOrDefault(x => x.CopyItemId == cit.CopyItemId);
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
                    var ctvmToRemove = GetClipTileViewModelById(ci.Id);
                    if (ctvmToRemove != null) {
                        ctvmToRemove.CopyItem.StartSync(e.SourceGuid);
                        //ctvmToRemove.CopyItem.Color.StartSync(e.SourceGuid);
                        Items.Remove(ctvmToRemove);
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
            MpHelpers.RunOnMainThread((Func<Task>)(async () => {
                if (sender is MpCopyItem ci) {
                    ci.StartSync(e.SourceGuid);

                    var svm = MpSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == ci.SourceId);

                    var app = svm.AppViewModel.App;
                    app.StartSync(e.SourceGuid);
                    //ci.Source.App.Icon.StartSync(e.SourceGuid);
                    //ci.Source.App.Icon.IconImage.StartSync(e.SourceGuid);

                    var dupCheck = this.GetClipTileViewModelById((int)ci.Id);
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
            }), DispatcherPriority.Background);
        }

        #endregion

        #endregion

        #region Private Methods

        private void CleanupAfterPaste(MpClipTileViewModel sctvm) {
            IsPasting = false;
            //clean up pasted items state after paste

            sctvm.IsPasting = false;
            if (sctvm.HasTemplates) {                
                sctvm.ClearEditing();
                sctvm.TemplateCollection.Reset();
                sctvm.TemplateRichText = string.Empty;
                sctvm.RequestUiReset();
                sctvm.RequestUiUpdate();
                sctvm.RequestScrollToHome();
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
                    bool isTileResize = false;

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
                    } else if(SelectedItem != null) {
                        //tile resize
                        isTileResize = true;
                        PersistentUniqueWidthTileLookup
                            .AddOrReplace(SelectedItem.QueryOffsetIdx, SelectedItem.TileBorderWidth);
                        Items.
                            Where(x => x.QueryOffsetIdx > SelectedItem.QueryOffsetIdx).
                            ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));                        
                    }
                    AdjustScrollOffsetToResize(oldHeadTrayX, oldScrollOffset);
                    RequestScrollIntoView(SelectedItem);
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
            var newCopyItem = await MpCopyItemBuilder.CreateFromDataObject(cd, IsAppendMode || IsAppendLineMode);

            MpConsole.WriteLine("CreateFromClipboardAsync: " + createItemSw.ElapsedMilliseconds + "ms");

            if (newCopyItem == null) {
                //this occurs if the copy item is not a known format or app init
                MpConsole.WriteTraceLine("Unable to create copy item from clipboard!");
                IsAddingClipboardItem = false;
                return;
            }

            bool isDup = newCopyItem.Id < 0;
            newCopyItem.Id = isDup ? -newCopyItem.Id : newCopyItem.Id;

            if (IsAppendMode || IsAppendLineMode) {
                if(isDup) {
                    //when duplicate copied in append mode treat item as new and don't unlink original 
                    isDup = false;
                    newCopyItem.Id = 0;
                    newCopyItem.CopyDateTime = DateTime.Now;
                    await newCopyItem.WriteToDatabaseAsync();
                }
                //when in append mode just append the new items text to selecteditem
                if (_appendModeCopyItem == null) {
                    if (SelectedItem == null) {
                        _appendModeCopyItem = newCopyItem;
                    } else {
                        _appendModeCopyItem = SelectedItem.CopyItem;
                    }
                }

                if (_appendModeCopyItem != newCopyItem) {                    
                    var am_ctvm = GetClipTileViewModelById(_appendModeCopyItem.Id);
                    var am_cv = Application.Current.MainWindow.GetVisualDescendents<MpContentView>().FirstOrDefault(x => x.DataContext == am_ctvm);
                   
                    am_cv.Rtb.Document = MpWpfRichDocumentExtensions.CombineFlowDocuments(
                        from: newCopyItem.ItemData.ToFlowDocument(),
                        to: am_cv.Rtb.Document,
                        insertNewLine: IsAppendLineMode);

                    MpContentDocumentRtfExtension.SaveTextContent(am_cv.Rtb).FireAndForgetSafeAsync(am_ctvm);

                    if (MpPreferences.NotificationShowAppendBufferToast) {
                        // TODO now composite item doesn't roll up children so the buffer needs to be created here
                        // if I use this at all

                        MpNotificationCollectionViewModel.Instance.ShowMessage(
                            title: "Append Buffer",
                            msg: SelectedItem.CopyItem.ItemData.ToPlainText(),
                            msgType: MpNotificationDialogType.AppendBuffer)
                            .FireAndForgetSafeAsync(this);
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
                    MpNotificationCollectionViewModel.Instance.ShowMessage(
                        title: "Trial Expired",
                        msg: "Please update your membership to use Monkey Paste",
                        msgType: MpNotificationDialogType.TrialExpired,
                        iconResourcePathOrBase64: MpPreferences.AbsoluteResourcesPath + @"/Images/monkey (2).png")
                        .FireAndForgetSafeAsync(this);
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
                bool needsRequery = false;
                bool wasPinned = false;
                if (pctvm.IsPinned) {
                    PinnedItems.Remove(pctvm);
                    if (pctvm.QueryOffsetIdx >= 0) {
                        needsRequery = true;
                        resultTile = pctvm;
                        //resultTile = await CreateClipTileViewModel(pctvm.Items.Select(x => x.CopyItem).ToList(), pctvm.QueryOffsetIdx);
                        //Items.Insert(pctvm.QueryOffsetIdx, pctvm);
                    }
                     
                } else {
                    if (pctvm.QueryOffsetIdx >= 0) {
                        // only recreate tiles that are already in the tray (they won't be if new)
                        resultTile = await CreateClipTileViewModel(pctvm.CopyItem, pctvm.QueryOffsetIdx);
                    } else {
                        resultTile = pctvm;
                    }
                    PinnedItems.Add(resultTile);
                    if (Items.Contains(pctvm)) {
                        Items.Remove(pctvm);
                        //int oldIdx = Items.IndexOf(pctvm);
                        pctvm = await CreateClipTileViewModel(null);
                        //Items.Move(oldIdx, Items.Count - 1);
                        Items.Add(pctvm);
                    }
                    wasPinned = true;
                }
                
                if(resultTile != null && wasPinned) {
                    //ClearClipSelection(false);
                    resultTile.IsSelected = true;
                    resultTile.OnPropertyChanged(nameof(resultTile.IsPinned));
                    resultTile.OnPropertyChanged(nameof(resultTile.IsPlaceholder));
                }

                if (needsRequery) {
                    //MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                    QueryCommand.Execute(ScrollOffset);
                    await Task.Delay(100);
                    while(IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    if(resultTile != null) {
                        var ctvm = Items.Where(x => !x.IsPlaceholder).FirstOrDefault(x => x.CopyItemId == resultTile.CopyItemId);
                        if(ctvm != null) {
                            int idx = Items.IndexOf(ctvm);
                            ClearClipSelection(false);
                            Items[idx].ResetSubSelection();
                            StoreSelectionState(Items[idx]);
                        }
                    }
                }
                pctvm.OnPropertyChanged(nameof(pctvm.IsPinned));
                Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));


                if (!IsAnyTilePinned) {
                    PinTrayTotalWidth = PinTrayScreenWidth = 0;
                } else if(!HasUserAlteredPinTrayWidth && PinTrayScreenWidth > PinTrayMaxDefaultMaxWidth) {
                    var pinTrayView = Application.Current.MainWindow
                                        .GetVisualDescendent<MpClipTrayContainerView>()
                                        .PinTrayView;
                    pinTrayView.Width = PinTrayMaxDefaultMaxWidth;
                }
                OnPropertyChanged(nameof(IsAnyTilePinned));
                OnPropertyChanged(nameof(ClipTrayScreenWidth));

                OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                OnPropertyChanged(nameof(ClipTrayScreenWidth));
                OnPropertyChanged(nameof(ClipTrayTotalWidth));
                OnPropertyChanged(nameof(MaximumScrollOfset));
            },
            (args) => args != null && 
                      (args is MpClipTileViewModel || 
                       args is List<MpClipTileViewModel>));


        public ICommand DuplicateSelectedClipsCommand => new RelayCommand(
            async () => {
                IsBusy = true;
                var clonedCopyItem = (MpCopyItem)await SelectedItem.CopyItem.Clone(true);

                await clonedCopyItem.WriteToDatabaseAsync();
                _newModels.Add(clonedCopyItem);

                AddNewItemsCommand.Execute(true);

                IsBusy = false;
            });

        public ICommand AppendNewItemsCommand => new RelayCommand(
            async() => {
                IsBusy = true;

                var amctvm = GetClipTileViewModelById(_appendModeCopyItem.Id);
                if (amctvm != null) {
                    await amctvm.InitializeAsync(amctvm.CopyItem, amctvm.QueryOffsetIdx);
                }

                IsBusy = false;
            },
            _appendModeCopyItem != null);

        public ICommand AddNewItemsCommand => new RelayCommand(
            async () => {
                IsBusy = MpMainWindowViewModel.Instance.IsMainWindowOpen;
                foreach (var ci in _newModels) {
                    var nctvm = await CreateClipTileViewModel(ci);
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
            async (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) => {
                if (!MpHelpers.IsOnMainThread()) {
                    MpHelpers.RunOnMainThread(()=>QueryCommand.Execute(offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg));
                    return;
                }

                IsBusy = IsRequery = true;
                var sw = new Stopwatch();
                sw.Start();

                bool isSubQuery = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg != null;
                bool isScrollJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is double;
                bool isOffsetJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is int;
                bool isLoadMore = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is bool;

                int loadOffsetIdx = 0;
                double newScrollOffset = default;
                int loadCount = 0;

                if(offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg != null) {
                    if (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is int) {
                        loadOffsetIdx = (int)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg;
                    } else if(offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is double) {
                        newScrollOffset = (double)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg;

                        loadOffsetIdx = FindJumpTileIdx(newScrollOffset);                        
                    } else if(offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is bool) {
                        newScrollOffset = ScrollOffset;

                        loadCount = _pageSize;

                        if((bool)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) {
                            //load More to tail
                            loadOffsetIdx = TailQueryIdx + 1;
                            if(loadOffsetIdx > MaxClipTrayQueryIdx) {
                                IsRequery = IsBusy = false;
                                return;
                            }
                        } else {
                            //load more to head
                            loadOffsetIdx = HeadQueryIdx - 1;
                            if(loadOffsetIdx < MinClipTrayQueryIdx) {
                                IsRequery = IsBusy = false;
                                return;
                            }
                        }
                    }

                    if(loadOffsetIdx + DefaultLoadCount > MaxClipTrayQueryIdx) {
                        loadOffsetIdx = MaxLoadQueryIdx;
                    } 
                } else {
                    newScrollOffset = 0;
                    ClearClipSelection();
                    MpDataModelProvider.ResetQuery();
                    await MpDataModelProvider.QueryForTotalCount();

                    Items.Clear();
                    PersistentUniqueWidthTileLookup.Clear();

                    OnPropertyChanged(nameof(TotalTilesInQuery));
                    OnPropertyChanged(nameof(ClipTrayTotalWidth));
                    OnPropertyChanged(nameof(MaximumScrollOfset));
                    OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                }


                if(loadCount == 0) {
                    // is not an LoadMore Query
                    loadCount = Math.Min(DefaultLoadCount, TotalTilesInQuery);
                } else if(loadOffsetIdx < 0) {
                    loadCount = 0;
                }

                
                List<int> fetchQueryIdxList = Enumerable.Range(loadOffsetIdx, loadCount).ToList();
                if (fetchQueryIdxList.Count > 0) {
                    // make list of select idx's

                    //clean up pinned items if requerying and not present in this query
                    PinnedItems.ForEach(x => x.QueryOffsetIdx = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.FastIndexOf(x.CopyItemId));

                    foreach(var pctvm in PinnedItems.Where(x=>x.QueryOffsetIdx >= 0)) {
                        //when pinned item is part of this fetch remove and next to
                        //tail if available or prev to head so load count is constant
                        int pinLoadOffsetIdx = fetchQueryIdxList.FastIndexOf(pctvm.QueryOffsetIdx);
                        if(pinLoadOffsetIdx < 0) {
                            continue;
                        }
                        fetchQueryIdxList.RemoveAt(pinLoadOffsetIdx);

                        if (fetchQueryIdxList.Count == 0) {
                            IsBusy = IsRequery = false;
                            return;
                        }
                        int newTailFetchIdx = fetchQueryIdxList.Last() + 1;
                        while(newTailFetchIdx <= MaxClipTrayQueryIdx && PinnedItems.Any(x=>x.QueryOffsetIdx == newTailFetchIdx)) {
                            newTailFetchIdx++;
                        }
                        if(newTailFetchIdx > MaxClipTrayQueryIdx) {
                            int newHeadFetchIdx = fetchQueryIdxList.First() - 1;
                            while(newHeadFetchIdx >= MinClipTrayQueryIdx && PinnedItems.Any(x=>x.QueryOffsetIdx == newHeadFetchIdx)) {
                                newHeadFetchIdx--;
                            }
                            if(newHeadFetchIdx >= MinClipTrayQueryIdx) {
                                fetchQueryIdxList.Insert(0, newHeadFetchIdx);
                            }
                        } else {
                            fetchQueryIdxList.Add(newTailFetchIdx);
                        }
                    }
                    if(!isLoadMore) {
                        // Cleanup Tray item count depending on last query
                        int itemCountDiff = Items.Count - fetchQueryIdxList.Count;
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
                    }

                    var cil = await MpDataModelProvider.FetchCopyItemsByQueryIdxList(fetchQueryIdxList);

                    for (int i = 0; i < cil.Count; i++) {
                        //if (isSubQuery && Items[i].IsSelected) {
                        //    StoreSelectionState(Items[i]);
                        //    Items[i].ClearSelection();
                        //}

                        if (isLoadMore) {
                            int loadMoreSwapIdx_from = fetchQueryIdxList[i] > TailQueryIdx ? 0 : Items.Count - 1;
                            int loadMoreSwapIdx_to = fetchQueryIdxList[i] > TailQueryIdx ? Items.Count - 1 : 0;

                            if (Items[loadMoreSwapIdx_from].IsSelected) {
                                StoreSelectionState(Items[loadMoreSwapIdx_from]);
                                Items[loadMoreSwapIdx_from].ClearSelection();
                            } 
                            //if (Items[loadMoreSwapIdx_to].IsSelected) {
                            //    StoreSelectionState(Items[loadMoreSwapIdx_to]);
                            //    Items[loadMoreSwapIdx_to].ClearSelection();
                            //}
                            Items.Move(loadMoreSwapIdx_from, loadMoreSwapIdx_to);
                            await Items[loadMoreSwapIdx_to].InitializeAsync(cil[i], fetchQueryIdxList[i]);

                            RestoreSelectionState(Items[loadMoreSwapIdx_to]);
                            //RestoreSelectionState(Items[loadMoreSwapIdx_from]);
                        } else {
                            
                            await Items[i].InitializeAsync(cil[i], fetchQueryIdxList[i]);

                            if (isSubQuery) {
                                RestoreSelectionState(Items[i]);
                            }

                        }                        
                    }
                }

                while(Items.Any(x=>x.IsAnyBusy)) {
                    await Task.Delay(100);
                }

                if(isSubQuery) {
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
                } else {
                    HasUserAlteredPinTrayWidth = false;

                    if (SelectedItem == null &&
                        PersistentSelectedModels.Count == 0 &&
                        TotalTilesInQuery > 0) {
                            ResetClipSelection();
                    }
                }
                

                OnPropertyChanged(nameof(TotalTilesInQuery));
                OnPropertyChanged(nameof(ClipTrayTotalWidth));
                OnPropertyChanged(nameof(MaximumScrollOfset));
                OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                if (Items.Count == 0) {
                    ScrollOffset = LastScrollOffset = 0;
                } 
                if (!isSubQuery) {
                    _scrollOffset = LastScrollOffset = 0;
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.RequeryCompleted);

                } else if (isOffsetJump) {
                    _scrollOffset = LastScrollOffset = FindTileOffsetX(HeadQueryIdx);
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.JumpToIdxCompleted);
                } else if (isScrollJump) {
                    _scrollOffset = LastScrollOffset = Math.Min(MaximumScrollOfset,newScrollOffset);
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.JumpToIdxCompleted);
                }
                OnPropertyChanged(nameof(ScrollOffset));

                IsBusy = IsRequery = false;
                sw.Stop();
                MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);
            },(offsetIdx_Or_ScrollOffset_Arg) => !IsAnyBusy && !IsRequery);

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
                var avm = MpAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == SelectedItem.AppViewModel.AppId);
                if(avm == null) {
                    return;
                }
                await avm.RejectApp();
            },
            SelectedItem != null);

        public ICommand ExcludeSubSelectedItemUrlDomainCommand => new RelayCommand(
            async () => {
                var uvm = MpUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == SelectedItem.UrlViewModel.UrlId);
                if(uvm == null) {
                    MpConsole.WriteTraceLine("Error cannot find url id: " + SelectedItem.UrlViewModel.UrlId);
                    return;
                }
                await uvm.RejectUrlOrDomain(true);
            },
            SelectedItem != null && SelectedItem.UrlViewModel != null);

        public ICommand SearchWebCommand => new RelayCommand<object>(
            (args) => {
                string pt = string.Join(
                            Environment.NewLine, 
                            PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));

                MpHelpers.OpenUrl(args.ToString() + Uri.EscapeDataString(pt));
            }, (args) => args != null && args is string);

        public ICommand ScrollToHomeCommand => new RelayCommand(
             () => {           
                 QueryCommand.Execute(0d);
             },
            () => ScrollOffset > 0 && !IsAnyBusy);

        public ICommand ScrollToEndCommand => new RelayCommand(
            () => {
                QueryCommand.Execute(MaximumScrollOfset);
            },
            () => ScrollOffset < MaximumScrollOfset && !IsAnyBusy);

        public ICommand ScrollToNextPageCommand => new RelayCommand(
             () => {
                 //int nextPageOffset = Math.Min(TotalTilesInQuery - 1, TailQueryIdx + 1);
                 //JumpToQueryIdxCommand.Execute(nextPageOffset);
                 double nextPageOffset = Math.Min(ScrollOffset + ClipTrayScreenWidth, MaximumScrollOfset);
                 QueryCommand.Execute(nextPageOffset);
                 //await Task.Delay(100);
                 //while (IsAnyBusy) { await Task.Delay(10); }
                 //if(Items.Where(x=>!x.IsPlaceholder).Count() == 0) {
                 //    return;
                 //}
                 //Items[0].IsSelected = true;                 
             },
            () => ScrollOffset < MaximumScrollOfset && !IsAnyBusy);

        public ICommand ScrollToPreviousPageCommand => new RelayCommand(
            async() => {
                //int prevPageOffset = Math.Max(0, HeadQueryIdx - 1);
                //JumpToQueryIdxCommand.Execute(prevPageOffset);
                //await Task.Delay(100);
                //while (IsScrollJumping) { await Task.Delay(10); }
                ////ScrollOffset = LastScrollOfset = ClipTrayTotalWidth;
                //if (Items.Where(x => !x.IsPlaceholder).Count() == 0) {
                //    return;
                //}
                //Items[0].IsSelected = true;
                double prevPageOffset = Math.Max(0, ScrollOffset - ClipTrayScreenWidth);
                QueryCommand.Execute(prevPageOffset);
            },
            () => ScrollOffset > 0 && !IsAnyBusy);


        public ICommand SelectNextItemCommand => new RelayCommand(
            async () => {
                IsArrowSelecting = true;

                bool needJump = false;
                int curRightMostSelectQueryIdx = -1;
                int nextSelectQueryIdx = -1;

                if (SelectedItem != null) {
                    curRightMostSelectQueryIdx = SelectedItem.QueryOffsetIdx;
                    nextSelectQueryIdx = curRightMostSelectQueryIdx + 1;
                    
                } else if (PersistentSelectedModels.Count > 0) {
                    needJump = true;
                    curRightMostSelectQueryIdx = PersistentSelectedModels.
                        Select(x =>
                            MpDataModelProvider.AllFetchedAndSortedCopyItemIds.IndexOf(x.Id))
                                .Max();
                    nextSelectQueryIdx = curRightMostSelectQueryIdx + 1;
                } else if (SelectedItem == null) {
                    nextSelectQueryIdx = 0;
                } else {
                    // should be caught by CanExecute
                    Debugger.Break();
                }
                

                if (nextSelectQueryIdx < 0) {
                    // selected item is in a different query so select query head
                    nextSelectQueryIdx = 0;
                }
                while (PinnedItems.Any(x => x.QueryOffsetIdx == nextSelectQueryIdx)) {
                    nextSelectQueryIdx++;
                }
                if (nextSelectQueryIdx <= MaxClipTrayQueryIdx) {
                    if (needJump) {
                        QueryCommand.Execute(nextSelectQueryIdx);
                    } else if(nextSelectQueryIdx > TailQueryIdx) {
                        //QueryCommand.Execute(true);                        
                        ScrollOffset = Math.Min(MaximumScrollOfset, ScrollOffset + 0.1);
                    }

                    while (IsAnyBusy) { await Task.Delay(100); }

                    var nctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == nextSelectQueryIdx);
                    if(nctvm != null) {
                        //ClearClipSelection(false);
                        //nctvm.IsSelected = true;
                        int idx = Items.IndexOf(nctvm);
                        ClearClipSelection(false);
                        Items[idx].ResetSubSelection();
                        StoreSelectionState(Items[idx]);
                    }
                }
                await Task.Delay(100);
                IsArrowSelecting = false;
            },
            () => !IsAnyBusy && !IsArrowSelecting &&
                  !HasScrollVelocity && 
                  !IsScrollingIntoView && 
                  (SelectedItem == null || (SelectedItem != null && !SelectedItem.IsPinned)));

        public ICommand SelectPreviousItemCommand => new RelayCommand(
            async () => {
                IsArrowSelecting = true;

                bool needJump = false;
                int curLeftMostSelectQueryIdx = -1;
                int prevSelectQueryIdx = -1;
                if (SelectedItem != null) {
                    curLeftMostSelectQueryIdx = SelectedItem.QueryOffsetIdx;
                    prevSelectQueryIdx = curLeftMostSelectQueryIdx - 1;
                } else if (PersistentSelectedModels.Count > 0) {
                    needJump = true;
                    curLeftMostSelectQueryIdx = PersistentSelectedModels.
                        Select(x =>
                            MpDataModelProvider.AllFetchedAndSortedCopyItemIds.IndexOf(x.Id))
                                .Min();
                    prevSelectQueryIdx = curLeftMostSelectQueryIdx - 1;
                } else {
                    // always if none is selected selected query head
                    prevSelectQueryIdx = 0;
                }

                if(prevSelectQueryIdx < 0) {
                    // last selected must be in another query so use default
                    prevSelectQueryIdx = 0;
                }
                while (PinnedItems.Any(x => x.QueryOffsetIdx == prevSelectQueryIdx)) {
                    prevSelectQueryIdx--;
                }

                if (prevSelectQueryIdx >= MinClipTrayQueryIdx) {
                    if (needJump) {
                        QueryCommand.Execute(prevSelectQueryIdx);
                        
                    } else if(prevSelectQueryIdx < HeadQueryIdx) {
                        //QueryCommand.Execute(false);
                        ScrollOffset = Math.Max(0, ScrollOffset - 0.1);
                    }
                    while (IsAnyBusy) { await Task.Delay(100); }


                    var pctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == prevSelectQueryIdx);
                    if (pctvm != null) {
                        int idx = Items.IndexOf(pctvm);
                        ClearClipSelection(false);
                        Items[idx].ResetSubSelection();
                        StoreSelectionState(Items[idx]);
                    }
                }
                await Task.Delay(100);
                IsArrowSelecting = false;
            },
            () => !IsAnyBusy && !HasScrollVelocity && !IsScrollingIntoView && !IsArrowSelecting &&
            (SelectedItem == null || (SelectedItem != null && !SelectedItem.IsPinned)));

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
                SelectedItem.ChangeColorCommand.Execute(hexStr.ToString());
            });

        public ICommand CopySelectedClipsCommand => new RelayCommand(
            async () => {
                var mpdo = await SelectedItem.ConvertToPortableDataObject(false, null, false, false);
                MpPlatformWrapper.Services.DataObjectHelper.SetPlatformClipboard(mpdo, true);
            },()=>SelectedItem != null);

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

                //In order to paste the app must hide first 
                //this triggers hidewindow to paste selected items

                var mpdo = await SelectedItem.ConvertToPortableDataObject(false, pi.Handle);

                await MpPlatformWrapper.Services.ExternalPasteHandler.PasteDataObject(
                    mpdo, pi, ptapvm == null ? false : ptapvm.PressEnter);
                //MpMainWindowViewModel.Instance.HideWindowCommand.Execute(true);
                
                CleanupAfterPaste(SelectedItem);
            },
            (args) => {
                return MpMainWindowViewModel.Instance.IsShowingDialog == false &&
                        SelectedItem != null &&
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
                var wpfdo = MpPlatformWrapper.Services.DataObjectHelper.GetPlatformClipboardDataObject();
                var mpdo = MpPlatformWrapper.Services.DataObjectHelper.ConvertToSupportedPortableFormats(wpfdo);

                SelectedItem.RequestPastePortableDataObject(mpdo);
            }, SelectedItem != null && !SelectedItem.IsPlaceholder);

        public ICommand PasteCopyItemByIdCommand => new RelayCommand<object>(
            async (args) => {
                if(args is int ciid) {
                    IsPasting = true;
                    var pi = new MpProcessInfo() {
                        Handle = MpProcessManager.LastHandle,
                        ProcessPath = MpProcessManager.LastProcessPath
                    };

                    MpClipTileViewModel ctvm = GetClipTileViewModelById(ciid);
                    if (ctvm == null) {
                        var templates = await MpDataModelProvider.GetTextTemplatesAsync(ciid);
                        if(templates != null && templates.Count > 0) {
                            // this item needs to be loaded into ui in order to paste it
                            // trigger query change before showing main window may need to tweak...

                            MpDataModelProvider.SetManualQuery(new List<int>() { ciid });
                            if (MpMainWindowViewModel.Instance.IsMainWindowOpen == false) {
                                MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                                while (MpMainWindowViewModel.Instance.IsMainWindowOpen == false) {
                                    await Task.Delay(100);
                                }
                            }
                            await Task.Delay(50); //wait for clip tray to get query changed message
                            while (IsAnyBusy) {
                                await Task.Delay(100);
                            }
                            ctvm = GetClipTileViewModelById(ciid);
                        } else {
                            var ci = await MpDataModelProvider.GetCopyItemById(ciid);
                            ctvm = await CreateClipTileViewModel(ci);
                        }                        
                    }

                    if(ctvm == null) {
                        Debugger.Break();
                    }
                    var mpdo = await ctvm.ConvertToPortableDataObject(false, pi, true);
                    await MpPlatformWrapper.Services.ExternalPasteHandler.PasteDataObject(mpdo, pi);

                    CleanupAfterPaste(ctvm);

                }
            },
            (args) => args is int);

        public ICommand BringSelectedClipTilesToFrontCommand => new RelayCommand(
            () => {
                
            });

        public ICommand SendSelectedClipTilesToBackCommand => new RelayCommand(
             () => {
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
                var civm = SelectedItem;
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
                if (tagToLink == null || SelectedItem == null) {
                    return false;
                }
                return true;
            });


        public ICommand AssignHotkeyCommand => new RelayCommand(
            () => {
                MpShortcutCollectionViewModel.Instance.ShowAssignShortcutDialogCommand.Execute(SelectedItem);
            },
            () => SelectedItem != null);

        public ICommand InvertSelectionCommand => new RelayCommand(
            () => {
            });

        public ICommand EditSelectedTitleCommand => new RelayCommand(
            () => {
                SelectedItem.IsTitleReadOnly = false;
            },
            () => {
                if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return false;
                }
                return SelectedItem != null;
            });

        public ICommand EditSelectedContentCommand => new RelayCommand(
            () => {
                SelectedItem.ToggleEditContentCommand.Execute(null);
            },
            () => SelectedItem != null && SelectedItem.IsContentReadOnly);

        public ICommand SendToEmailCommand => new RelayCommand(
            () => {
                // for gmail see https://stackoverflow.com/a/60741242/105028
                string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                MpHelpers.OpenUrl(
                    string.Format("mailto:{0}?subject={1}&body={2}",
                    string.Empty, SelectedItem.CopyItem.Title,
                    pt));
                //MpClipTrayViewModel.Instance.ClearClipSelection();
                //IsSelected = true;
                //MpHelpers.CreateEmail(MpPreferences.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
            },
            () => {
                return !IsAnyEditingClipTile && SelectedItem != null;
            });

        public ICommand MergeSelectedClipsCommand => new RelayCommand(
            () => {
                SelectedItem.RequestMerge();
            },
            () => SelectedItem != null);

        public ICommand SummarizeCommand => new RelayCommand(
            async () => {
                var result = await MpOpenAi.Instance.Summarize(SelectedModels[0].ItemData.ToPlainText());
                SelectedModels[0].ItemDescription = result;
                await SelectedModels[0].WriteToDatabaseAsync();
            },
            () => SelectedItem != null && SelectedItem.IsTextItem);

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

                    promptBuilder.AppendText(Environment.NewLine + SelectedItem.CopyItem.ItemData.ToPlainText());

                    // Speak the contents of the prompt asynchronously.
                    speechSynthesizer.SpeakAsync(promptBuilder);

                }, DispatcherPriority.Background);
            },
            () => {
                return SelectedItem != null && SelectedItem.IsTextItem;
            });

        public ICommand AnalyzeSelectedItemCommand => new RelayCommand<int>(
            async (presetId) => {
                var preset = await MpDb.GetItemAsync<MpAnalyticItemPreset>((int)presetId);
                var analyticItemVm = MpAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AnalyzerPluginGuid == preset.AnalyzerPluginGuid);
                var presetVm = analyticItemVm.Items.FirstOrDefault(x => x.Preset.Id == preset.Id);                

                var prevSelectedPresetVm = analyticItemVm.SelectedItem;
                analyticItemVm.SelectPresetCommand.Execute(presetVm);
                analyticItemVm.ExecuteAnalysisCommand.Execute(SelectedItem);
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

        public ICommand FindAndReplaceSelectedItem => new RelayCommand(
            () => {
                SelectedItem.ToggleFindAndReplaceVisibleCommand.Execute(null);
            }, SelectedItem != null && !SelectedItem.IsFindAndReplaceVisible && SelectedItem.IsTextItem);
        #endregion
    }
    public enum MpExportType {
        None = 0,
        Files,
        Csv,
        Zip
    }
}
