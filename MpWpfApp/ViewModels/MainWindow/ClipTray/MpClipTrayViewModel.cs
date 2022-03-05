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
using FFImageLoading.Helpers.Exif;
using MpProcessHelper;

namespace MpWpfApp {
    public class MpClipTrayViewModel : 
        MpViewModelBase, 
        MpISingletonViewModel<MpClipTrayViewModel>, 
        MpITriggerActionViewModel, 
        MpIMenuItemViewModel {
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
                            ShortcutType = MpShortcutType.CopySelectedItems
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Paste",
                            IconResourceKey = Application.Current.Resources["PasteIcon"] as string,
                            Command = PasteSelectedClipsCommand,
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
                                    Header = $"'{PrimaryItem.PrimaryItem.CopyItem.Source.App.AppName}' to _Excluded App",
                                    IconId = PrimaryItem.PrimaryItem.CopyItem.Source.AppId,
                                    Command = ExcludeSubSelectedItemApplicationCommand
                                },
                                new MpMenuItemViewModel() {
                                    Header = PrimaryItem.PrimaryItem.CopyItem.Source.Url == null ? 
                                                null :
                                                $"'{PrimaryItem.PrimaryItem.CopyItem.Source.Url.UrlDomainPath}' to _Excluded Domain",
                                    IconId = PrimaryItem.PrimaryItem.CopyItem.Source.Url == null ?
                                                0 :
                                                PrimaryItem.PrimaryItem.CopyItem.Source.Url.IconId,
                                    IsVisible = PrimaryItem.PrimaryItem.CopyItem.Source.Url != null,
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

        public double ClipTrayTotalTileWidth {
            get {
                int totalTileCount = TotalTilesInQuery;
                int uniqueWidthTileCount = PersistentUniqueWidthTileLookup.Count;
                int defaultWidthTileCount = totalTileCount - uniqueWidthTileCount;

                double defaultWidth = MpMeasurements.Instance.ClipTileMinSize;

                double totalUniqueWidth = PersistentUniqueWidthTileLookup.Sum(x => x.Value);
                double totalTileWidth = totalUniqueWidth + (defaultWidthTileCount * defaultWidth);

                return FindTileOffsetX(TotalTilesInQuery - 1) + defaultWidth;
            }
        }

        public double ClipTrayTotalWidth => Math.Max(ClipTrayScreenWidth, ClipTrayTotalTileWidth);

        public double MaximumScrollOfset {
            get {
                if (TotalTilesInQuery > MpMeasurements.Instance.DefaultTotalVisibleClipTiles) {
                    return ClipTrayTotalWidth - ClipTrayScreenWidth; //(MpMeasurements.Instance.TotalVisibleClipTiles * MpMeasurements.Instance.ClipTileMinSize);
                }
                return 0;
            }
        }
        #endregion

        #region Appearance

        public int TotalVisibleClipTiles {
            get {
                return (int)(ClipTrayScreenWidth / MpMeasurements.Instance.ClipTileMinSize);
            }
        }

        #endregion

        #region Business Logic

        public int RemainingItemsOnRight { get; set; }

        public int RemainingItemsOnLeft { get; set; }

        public int RemainingItemsCountThreshold { get; private set; }

        public int TotalTilesInQuery => MpDataModelProvider.TotalTilesInQuery;

        public int DefaultLoadCount => TotalVisibleClipTiles + 2;

        public SelectionMode SelectionMode => SelectionMode.Single;

        #endregion

        #region State

        #region Virtual

        //set in civm IsSelected property change, DragDrop.Drop (copy mode)
        public List<MpCopyItem> PersistentSelectedModels { get; set; } = new List<MpCopyItem>();

        //<HeadCopyItemId, Unique ItemWidth> unique is != to MpMeausrements.Instance.ClipTileMinSize
        public Dictionary<int, double> PersistentUniqueWidthTileLookup { get; set; } = new Dictionary<int, double>();
        #endregion

        public bool IsThumbDragging { get; set; } = false;

        public bool IsAnyTilePinned => PinnedItems.Count > 0;

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

        public bool IsHorizontalScrollBarVisible => TotalTilesInQuery > MpMeasurements.Instance.DefaultTotalVisibleClipTiles;

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

        public bool IsAnyResizing => Items.Any(x => x.IsResizing);

        public bool CanAnyResize => Items.Any(x => x.CanResize);

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

        public void RegisterTrigger(MpActionViewModelBase mvm) {
            OnCopyItemItemAdd += mvm.OnActionTriggered;
            MpConsole.WriteLine($"ClipTray Registered {mvm.Label} matcher");
        }

        public void UnregisterTrigger(MpActionViewModelBase mvm) {
            OnCopyItemItemAdd -= mvm.OnActionTriggered;
            MpConsole.WriteLine($"Matcher {mvm.Label} Unregistered from OnCopyItemAdded");
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

        

        public void ClipboardChanged(object sender, MpDataObject mpdo) {
            if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }
            MpHelpers.RunOnMainThread(async () => {
                await AddItemFromClipboard(mpdo);
            });
        }

        public async Task<MpClipTileViewModel> CreateClipTileViewModel(MpCopyItem ci,int queryOffsetIdx = -1) {
            var nctvm = new MpClipTileViewModel(this);
            await nctvm.InitializeAsync(ci,queryOffsetIdx);
            return nctvm;
        }

        public MpDataObject GetDataObjectByCopyItems(List<MpCopyItem> selectedModels, bool isDragDrop, bool isToExternalApp) {
            MpDataObject d = new MpDataObject();
            string rtf = string.Empty.ToRichText();
            string pt = string.Empty;

            //var selectedModels = await MpDataModelProvider.GetCopyItemsByIdList(ciidArray.ToList());                       

            if (isToExternalApp) {
                //gather rtf and text NOT setdata it needs file drop first
                foreach (var sctvm in selectedModels) {
                    string itemData = sctvm.ItemData;
                    if (sctvm.ItemType == MpCopyItemType.FileList) {
                        itemData = itemData.ToRichText();
                    } else if (sctvm.ItemType == MpCopyItemType.Image) {
                        continue;
                    }
                    rtf = MpWpfStringExtensions.CombineRichText(itemData, rtf);
                }
                pt = rtf.ToPlainText();
            }

            //set file drop (always must set so when dragged out of application user doesn't get no-drop cursor)
            if (MpExternalDropBehavior.Instance.IsProcessNeedFileDrop(MpProcessManager.LastProcessPath) &&
                isDragDrop) {
                //only when pasting into explorer or notepad must have file drop
                var sctfl = new List<string>();
                if (selectedModels.All(x => x.ItemType != MpCopyItemType.FileList) &&
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) {
                    //external drop w/ ctrl down merges all selected items (unless file list)
                    // TODO maybe for multiple files w/ ctrl down compress into zip?
                    if (MpExternalDropBehavior.Instance.IsProcessLikeNotepad(MpProcessManager.LastProcessPath)) {
                        //merge as plain text
                        string fp = MpHelpers.GetUniqueFileName(MpExternalDropFileType.Txt, selectedModels[0].Title);
                        sctfl.Add(MpHelpers.WriteTextToFile(fp, pt, true));
                    } else {
                        //merge as rich text
                        string fp = MpHelpers.GetUniqueFileName(MpExternalDropFileType.Rtf, selectedModels[0].Title);
                        sctfl.Add(MpHelpers.WriteTextToFile(fp, rtf, true));
                    }
                } else {
                    foreach (var sci in selectedModels) {
                        if (sci.ItemType == MpCopyItemType.FileList) {
                            sctfl.AddRange(sci.ItemData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        } else if (sci.ItemType == MpCopyItemType.Image) {
                            string fp = MpHelpers.GetUniqueFileName(MpExternalDropFileType.Png, sci.Title);
                            sctfl.Add(MpHelpers.WriteBitmapSourceToFile(fp, sci.ItemData.ToBitmapSource(), true));
                        } else if (MpExternalDropBehavior.Instance.IsProcessLikeNotepad(MpProcessManager.LastProcessPath)) {
                            string fp = MpHelpers.GetUniqueFileName(MpExternalDropFileType.Txt, sci.Title);
                            sctfl.Add(MpHelpers.WriteTextToFile(fp, sci.ItemData.ToPlainText(), true));
                        } else {
                            string fp = MpHelpers.GetUniqueFileName(MpExternalDropFileType.Rtf, sci.Title);
                            sctfl.Add(MpHelpers.WriteTextToFile(fp, sci.ItemData.ToRichText(), true));
                        }
                    }
                }

                d.DataFormatLookup.AddOrReplace(MpClipboardFormat.FileDrop,string.Join(Environment.NewLine,sctfl));
                // d.SetData(MpClipboardFormat.FileDrop, sctfl.ToStringCollection());
            }

            if (isToExternalApp) {
                //set rtf and text
                if (!string.IsNullOrEmpty(rtf)) {
                    d.DataFormatLookup.AddOrReplace(MpClipboardFormat.Rtf, rtf);
                }
                if (!string.IsNullOrEmpty(pt)) {
                    d.DataFormatLookup.AddOrReplace(MpClipboardFormat.Text, rtf.ToPlainText());
                }
                //set image
                if (selectedModels.Count == 1 && selectedModels[0].ItemType == MpCopyItemType.Image) {
                    d.DataFormatLookup.AddOrReplace(MpClipboardFormat.Bitmap, selectedModels[0].ItemData);
                }

                //set csv
                string sctcsv = string.Join(Environment.NewLine, selectedModels.Select(x => x.ItemData.ToCsv()));
                if (!string.IsNullOrWhiteSpace(sctcsv)) {
                    d.DataFormatLookup.AddOrReplace(MpClipboardFormat.Csv, sctcsv);
                }

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
            //    //d.SetData(MpPreferences.ClipTileDragDropFormatName, SelectedItems.ToList());
            //}

            return d;
            //awaited in MpMainWindowViewModel.Instance.HideWindow
        }

        public async Task<MpDataObject> GetDataObjectFromSelectedClips(bool isDragDrop = false, bool isToExternalApp = false) {
            //selection (if all subitems are dragging select host if no subitems are selected select all)
            List<MpCopyItem> selectedModels = new List<MpCopyItem>();
            if (SelectedItems.Count == 0) {
                selectedModels = PersistentSelectedModels;
            } else {
                SelectedItems.ForEach(x => x.DoCommandSelection());

                foreach (var sctvm in SelectedItems) {
                    string sctrtf = await sctvm.GetSubSelectedPastableRichText(isToExternalApp);
                    selectedModels.Add(new MpCopyItem() {
                        ItemType = sctvm.ItemType,
                        ItemData = sctrtf,
                        Title = sctvm.PrimaryItem.CopyItemTitle
                    });
                }
            }
            if (selectedModels.Count == 0) {
                return new MpDataObject();
            }

            selectedModels.Reverse();

            var result = GetDataObjectByCopyItems(selectedModels, isDragDrop, isToExternalApp);
            return result;
        }

        public async Task PasteDataObject(object pasteDataObject, bool fromHotKey = false) {
            if (IsAnyPastingTemplate) {
                MpMainWindowViewModel.Instance.IsMainWindowLocked = false;
            }

            //called in the oncompleted of hide command in mwvm
            if (pasteDataObject != null && pasteDataObject is MpDataObject mpdo) {
                MpConsole.WriteLine("Pasting " + SelectedItems.Count + " items");

                IntPtr pasteToWindowHandle = IntPtr.Zero;
                if (_selectedPasteToAppPathViewModel != null) {
                    pasteToWindowHandle = MpProcessAutomation.SetActiveProcess(
                        _selectedPasteToAppPathViewModel.AppPath,
                        _selectedPasteToAppPathViewModel.IsAdmin,
                        _selectedPasteToAppPathViewModel.IsSilent,
                        _selectedPasteToAppPathViewModel.Args,
                        IntPtr.Zero,
                        _selectedPasteToAppPathViewModel.WindowState);
                } else if (_selectedPasteToAppPathWindowHandle != IntPtr.Zero) {
                    var windowState = WinApi.SW_SHOWMAXIMIZED;
                    if (MpProcessManager.LastWindowStateHandleDictionary.ContainsKey(_selectedPasteToAppPathWindowHandle)) {
                        windowState = MpProcessManager.GetShowWindowValue(MpProcessManager.LastWindowStateHandleDictionary[_selectedPasteToAppPathWindowHandle]);
                    }
                    WinApi.ShowWindowAsync(_selectedPasteToAppPathWindowHandle, windowState);
                    pasteToWindowHandle = _selectedPasteToAppPathWindowHandle;
                } else {
                    //pasteToWindowHandle = MpResolver.Resolve<MpProcessHelper.MpProcessManager>().LastHandle;
                    pasteToWindowHandle = MpProcessManager.LastHandle;
                }
                bool finishWithEnterKey = _selectedPasteToAppPathViewModel != null && _selectedPasteToAppPathViewModel.PressEnter;
                await MpClipboardHelper.MpClipboardManager.PasteDataObject(mpdo, pasteToWindowHandle,finishWithEnterKey);

                if (!fromHotKey) {
                    //resort list so pasted items are in front and paste is tracked
                    for (int i = SelectedItems.Count - 1; i >= 0; i--) {
                        var sctvm = SelectedItems[i];

                        var a = await MpDataModelProvider.GetAppByPath(MpProcessManager.GetProcessPath(MpProcessManager.LastHandle));
                        var aid = a == null ? 0 : a.Id;
                        foreach (var ivm in sctvm.ItemViewModels) {
                            ivm.CopyItem.PasteCount++;
                            await ivm.CopyItem.WriteToDatabaseAsync();
                            await MpPasteHistory.Create(
                                copyItemId: ivm.CopyItemId,
                                appId: aid);
                            
                        }
                    }
                    
                }
            } else if (pasteDataObject == null) {
                MpConsole.WriteLine("MainWindow Hide Command pasteDataObject was null, ignoring paste");
            }
            _selectedPasteToAppPathViewModel = null;
            //if (!fromHotKey) {
            //    ResetClipSelection();
            //}

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
            foreach (var ctvm in PinnedItems) {
                foreach (var civm in ctvm.ItemViewModels) {
                    var ortbvm = ctvm.ItemViewModels.Where(x => x.CopyItemId == ciid).FirstOrDefault();
                    if (ortbvm != null) {
                        return ortbvm;
                    }
                }
            }
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
            //        MpHelpers.ConvertPlainTextToRichText("Take a moment to look through the available features in the following tiles, which are always available in the 'Help' pinboard"));

            //var introItem2 = new MpCopyItem(
            //    MpCopyItemType.RichText,
            //    "One place for your clipboard",
            //    MpHelpers.ConvertPlainTextToRichText(""));
            //MpPreferences.IsInitialLoad = false;
            //
        }

        public void NotifySelectionChanged() {
            MpMessenger.Send(MpMessageType.TraySelectionChanged);
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

        public double FindTileOffsetX(int queryOffsetIdx) {
            int totalTileCount = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Count;
            if (totalTileCount <= 0) {
                return 0;
            }
            queryOffsetIdx = Math.Max(0, Math.Min(queryOffsetIdx, totalTileCount - 1));

            var headItemIds = MpDataModelProvider.AllFetchedAndSortedCopyItemIds;
            var uniqueWidthLookup = MpClipTrayViewModel.Instance.PersistentUniqueWidthTileLookup;

            double offsetX = 0;// MpMeasurements.Instance.ClipTileMargin;
            for (int i = 1; i <= queryOffsetIdx; i++) {
                int tileHeadId = headItemIds[i - 1];
                if (MpClipTrayViewModel.Instance.PinnedItems.Any(x => x.HeadItem?.CopyItemId == tileHeadId)) {
                    continue;
                }
                offsetX += MpMeasurements.Instance.ClipTileMargin * 2;

                if (uniqueWidthLookup.ContainsKey(tileHeadId)) {
                    offsetX += uniqueWidthLookup[tileHeadId];
                    offsetX -= MpMeasurements.Instance.ClipTileMargin;
                } else {
                    offsetX += MpClipTileViewModel.DefaultBorderWidth;
                }
            }

            return offsetX;
        }

        public void AdjustScrollOffsetToResize(double oldHeadTrayX, double oldScrollOfset) {
            double oldScrollOffsetDiffWithHead = oldScrollOfset - oldHeadTrayX;

            double newHeadTrayX = HeadItem == null ? 0 : HeadItem.TrayX;
            double headOffsetRatio = newHeadTrayX / oldHeadTrayX;
            headOffsetRatio = double.IsNaN(headOffsetRatio) ? 0 : headOffsetRatio;
            double newScrollOfsetDiffWithHead = headOffsetRatio * oldScrollOffsetDiffWithHead;
            double newScrollOfset = FindTileOffsetX(HeadQueryIdx) + newScrollOfsetDiffWithHead;

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
                //if (PinnedItems.Any(x => x.ItemViewModels.Any(y => y.CopyItemId == ci.Id))) {
                //    var pctvm = PinnedItems.FirstOrDefault(x => x.ItemViewModels.Any(y => y.CopyItemId == ci.Id));
                //    pctvm.ItemViewModels.Remove(pctvm.ItemViewModels.FirstOrDefault(x => x.CopyItemId == ci.Id));
                //}
            } else if (e is MpCopyItemTag cit && Items.Any(x=>x.ItemViewModels.Any(y => y.CopyItemId == cit.CopyItemId))) {
                var ctvm = Items.FirstOrDefault(x => x.ItemViewModels.Any(y => y.CopyItemId == cit.CopyItemId));
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
                Items.Remove(ctvm);
                Items.Where(x => x.QueryOffsetIdx > ctvm.QueryOffsetIdx).ForEach(x => x.QueryOffsetIdx--);
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
            MpHelpers.RunOnMainThread((Action)(() => {
            }));
        }

        private void MpDbObject_SyncAdd(object sender, MpDbSyncEventArgs e) {
            MpHelpers.RunOnMainThread(async () => {
                if (sender is MpCopyItem ci) {
                    ci.StartSync(e.SourceGuid);
                    ci.Source.App.StartSync(e.SourceGuid);
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
                    ci.Source.App.EndSync();
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

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsTrayEmpty));
            //return;
            //if (IsAnyTileExpanded) {
            //    return;
            //}
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

                    if(MpMainWindowViewModel.Instance.IsResizing) {
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
                        if (PrimaryItem.CanResize) {
                            Items.
                                Where(x => x.QueryOffsetIdx >= PrimaryItem.QueryOffsetIdx).
                                ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
                        }
                    }

                    AdjustScrollOffsetToResize(oldHeadTrayX, oldScrollOffset);

                    break;
                case MpMessageType.ResizeContentCompleted:
                    _oldMainWindowHeight = MpMainWindowViewModel.Instance.MainWindowHeight;
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
                    IsRequery = true;
                    RequeryCommand.Execute(null);
                    break;
                case MpMessageType.SubQueryChanged:
                    IsRequery = true;
                    RequeryCommand.Execute(HeadQueryIdx);
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
                    MpMessenger.Send<MpMessageType>(MpMessageType.TrayScrollChanged);
                    break;
            }
        }

        private async Task AddItemFromClipboard(MpDataObject cd) {
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
            }

            bool isDup = newCopyItem.Id < 0;
            newCopyItem.Id = isDup ? -newCopyItem.Id : newCopyItem.Id;

            if (MpSidebarViewModel.Instance.IsAppendMode) {
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

                if (MpSidebarViewModel.Instance.IsAppendMode) {
                    AppendNewItemsCommand.Execute(null);
                } else {
                    AddNewItemsCommand.Execute(null);
                }                    
            }

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
                if (pctvm.IsPinned) {
                    PinnedItems.Remove(pctvm);
                    resultTile = Items.FirstOrDefault(x => x.QueryOffsetIdx == pctvm.QueryOffsetIdx);
                } else {
                    resultTile = await CreateClipTileViewModel(pctvm.HeadItem.CopyItem);
                    resultTile.QueryOffsetIdx = pctvm.QueryOffsetIdx;
                    PinnedItems.Add(resultTile);
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
            },
            (args) => args != null && 
                      (args is MpClipTileViewModel || 
                       args is List<MpClipTileViewModel>));

        public ICommand PasteCopyItemByIdCommand => new RelayCommand<object>(
            async (args) => {
                int[] ciidl = args is int[]? args as int[] : new int[] { (int)args };

                var cil = await MpDataModelProvider.GetCopyItemsByIdList(ciidl.ToList());
                var pasteDataObject = GetDataObjectByCopyItems(cil, false, true);
                MpMainWindowViewModel.Instance.HideWindowCommand.Execute(pasteDataObject);
            },
            (args) => args != null && (args is int || args is int[]));

        public ICommand DuplicateSelectedClipsCommand => new RelayCommand(
            async () => {
                IsBusy = true;


                if (MpDataModelProvider.QueryInfo.TagId != MpTag.AllTagId) {
                    //this occurs when appending within non-default tag

                    foreach (var nci in _newModels) {
                        await MpCopyItemTag.Create(
                                MpDataModelProvider.QueryInfo.TagId,
                                nci.Id);
                    }
                }

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

        public ICommand AppendNewItemsCommand => new RelayCommand(
            async() => {
                IsBusy = true;

                var amctvm = GetClipTileViewModelById(_appendModeCopyItem.Id);
                if (amctvm != null) {
                    await amctvm.InitializeAsync(amctvm.HeadItem.CopyItem, amctvm.QueryOffsetIdx);
                }

                IsBusy = false;
            },
            _appendModeCopyItem != null);

        public ICommand AddNewItemsCommand => new RelayCommand(
            async () => {
                IsBusy = true;                

                if(MpDataModelProvider.QueryInfo.TagId == MpTag.AllTagId) {
                    //instead of handling all unique cases manual insert new items in head of current query if which may not be 
                    //accurate but allows to continue workflow

                    MpClipTileSortViewModel.Instance.SetToManualSort();
                }
                

                foreach (var nci in _newModels) {
                    int idx = HeadQueryIdx < 0 ? 0 : HeadQueryIdx; // check is for empty tag

                    MpClipTileViewModel nctvm = await CreateClipTileViewModel(nci, idx);
                    MpDataModelProvider.InsertQueryItem(nctvm.HeadItem.CopyItemId, idx);
                    OnPropertyChanged(nameof(TotalTilesInQuery));

                    Items.ForEach(x => x.QueryOffsetIdx++);
                    Items.Insert(0, nctvm);
                    //var civm = GetContentItemViewModelById(nci.Id);
                    //if (civm != null && civm.Parent != null && civm.Parent.QueryOffsetIdx > HeadQueryIdx) {
                    //    //when duplicate detected and is already on tray (like on reload and last item is in list)
                    //    nctvm = civm.Parent;
                    //    Items.Where(x => x.QueryOffsetIdx < nctvm.QueryOffsetIdx).ForEach(x => x.QueryOffsetIdx++);
                    //    MpDataModelProvider.MoveQueryItem(nctvm.HeadItem.CopyItemId, HeadQueryIdx - 1);
                    //    nctvm.QueryOffsetIdx = HeadQueryIdx - 1;
                    //    Items.Move(Items.IndexOf(nctvm), 0);
                    //} else {
                    //    nctvm = await CreateClipTileViewModel(nci, HeadQueryIdx);
                    //    MpDataModelProvider.InsertQueryItem(nctvm.HeadItem.CopyItemId, HeadQueryIdx);
                    //    OnPropertyChanged(nameof(TotalTilesInQuery));

                    //    Items.ForEach(x => x.QueryOffsetIdx++);
                    //    Items.Insert(0, nctvm);
                    //}
                }

                _newModels.Clear();

                IsBusy = false;

                //using tray scroll changed so tile drop behaviors update their drop rects
                MpMessenger.Send<MpMessageType>(MpMessageType.TrayScrollChanged);
            },
            () => {
                if (_newModels.Count == 0) {
                    return false;
                }
                if(!string.IsNullOrEmpty(MpSearchBoxViewModel.Instance.LastSearchText)) {
                    return false;
                }
                if (MpDataModelProvider.QueryInfo.SortType == MpContentSortType.Manual) {
                    return false;
                }
                if (MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                    return true;
                }
                return false;
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

                    MpDataModelProvider.ResetQuery();

                    await MpDataModelProvider.QueryForTotalCount();
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
                    var cil = await MpDataModelProvider.FetchCopyItemRangeAsync(offsetIdx, loadCount);

                    for (int i = 0; i < cil.Count; i++) {
                        if(PinnedItems.Any(x=>x.HeadItem.CopyItemId == cil[i].Id)) {
                            continue;
                        }
                        await Items[i].InitializeAsync(cil[i], i + offsetIdx);

                        if (isDragDropRequery) {
                            RestoreSelectionState(Items[i]);
                        }
                    }
                }

                if (SelectedItems.Count == 0 && PersistentSelectedModels.Count == 0 && TotalTilesInQuery > 0) {
                    ResetClipSelection();
                } 

                IsBusy = false;
                IsRequery = false;

                MpMessenger.Send<MpMessageType>(MpMessageType.RequeryCompleted);

                sw.Stop();
                MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);

                PrintInstanceCount();
            });

        public ICommand LoadMoreClipsCommand => new RelayCommand<object>(
            async (isLoadMore) => {
                IsBusy = IsLoadingMore = true;

                if (IsAnyTileFlipped) {
                    UnFlipAllTiles();
                }

                bool isLeft = ((int)isLoadMore) >= 0;

                //int loadCount = Math.Min(DefaultLoadCount, TotalTilesInQuery);

                ////keep item count in sync with main window size
                //int itemCountDiff = Items.Count - loadCount;
                //if (itemCountDiff > 0) {
                    
                //    while (itemCountDiff > 0) {
                //        int removeIdx = isLeft ? Items.Count - 1 : 0;
                //        Items.RemoveAt(removeIdx);
                //        itemCountDiff--;
                //    }
                //} else if (itemCountDiff < 0) {
                //    while (itemCountDiff < 0) {
                //        var ctvm = await CreateClipTileViewModel(null);
                //        if(!isLeft) {
                //            Items.Add(ctvm);
                //        } else {
                //            Items.Insert(0, ctvm);
                //        }
                //        itemCountDiff++;
                //    }
                //}

                if (isLeft && TailQueryIdx < TotalTilesInQuery - 1) {
                    int offsetIdx = TailQueryIdx + 1;
                    int fetchCount = _pageSize;
                    if (offsetIdx + fetchCount >= TotalTilesInQuery) {
                        fetchCount = TotalTilesInQuery - offsetIdx;
                    }
                    var cil = await MpDataModelProvider.FetchCopyItemRangeAsync(offsetIdx, fetchCount);

                    for (int i = 0; i < cil.Count; i++) {
                        if (PinnedItems.Any(x => x.HeadItem.CopyItemId == cil[i].Id)) {
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
                        if (PinnedItems.Any(x => x.HeadItem.CopyItemId == cil[i].Id)) {
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
                       !IsBusy &&
                       !IsLoadingMore &&
                       !IsScrollJumping &&
                       //!IsAnyTileExpanded &&
                       !MpMainWindowViewModel.Instance.IsMainWindowLoading;
            });

        public ICommand JumpToQueryIdxCommand => new RelayCommand<int>(
            async (idx) => {
                if (idx < TailQueryIdx && idx > HeadQueryIdx) {
                    MpMessenger.Send<MpMessageType>(MpMessageType.JumpToIdxCompleted);
                    return;
                }

                IsBusy = true;
                IsScrollJumping = true;


                int loadCount = DefaultLoadCount;
                if (idx + loadCount > TotalTilesInQuery) {
                    //loadCount = MpMeasurements.Instance.TotalVisibleClipTiles;
                    idx = TotalTilesInQuery - loadCount;
                }
                ScrollOffset = LastScrollOfset = FindTileOffsetX(idx);

                var cil = await MpDataModelProvider.FetchCopyItemRangeAsync(idx, loadCount);

                for (int i = 0; i < cil.Count; i++) {
                    if (PinnedItems.Any(x => x.HeadItem.CopyItemId == cil[i].Id) ||
                        i >= Items.Count) {
                        // NOTE checking i w/ item count is probably a side affect bug
                        //of resizing window and tile/scoll offset and tray dimensions
                        //not all updating
                        continue;
                    }
                    if (Items[i].IsSelected) {
                        StoreSelectionState(Items[i]);
                        Items[i].ClearSelection();
                    }
                    await Items[i].InitializeAsync(cil[i], idx + i);
                    RestoreSelectionState(Items[i]);
                }

                MpMessenger.Send<MpMessageType>(MpMessageType.JumpToIdxCompleted);

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

        public ICommand ExcludeSubSelectedItemApplicationCommand => new RelayCommand(
            async () => {
                var avm = MpAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == PrimaryItem.PrimaryItem.CopyItem.Source.AppId);
                if(avm == null) {
                    return;
                }
                await avm.RejectApp();
            },
            PrimaryItem != null && PrimaryItem.PrimaryItem != null);

        public ICommand ExcludeSubSelectedItemUrlDomainCommand => new RelayCommand(
            async () => {
                var uvm = MpUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == PrimaryItem.PrimaryItem.CopyItem.Source.UrlId);
                if(uvm == null) {
                    MpConsole.WriteTraceLine("Error cannot find url id: " + PrimaryItem.PrimaryItem.CopyItem.Source.UrlId);
                    return;
                }
                await uvm.RejectUrlOrDomain(true);
            },
            PrimaryItem != null && PrimaryItem.PrimaryItem != null);

        public ICommand SearchWebCommand => new RelayCommand<object>(
            (args) => {
                string pt = string.Join(
                            Environment.NewLine, 
                            PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));

                MpHelpers.OpenUrl(args.ToString() + Uri.EscapeDataString(pt));
            }, (args) => args != null && args is string);

        public ICommand ScrollToHomeCommand => new RelayCommand(
             () => {
                RequeryCommand.Execute(null);
            },
            () => !IsBusy);

        public ICommand ScrollToEndCommand => new RelayCommand(
            async () => {
                JumpToQueryIdxCommand.Execute(TotalTilesInQuery - 1);
                await Task.Delay(100);
                while (IsScrollJumping) { await Task.Delay(10); }
                ScrollOffset = LastScrollOfset = ClipTrayTotalWidth;
                Items[Items.Count - 1].IsSelected = true;
            },
            () => !IsBusy);

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

                if (nextSelectQueryIdx < TotalTilesInQuery) {
                    if (needJump) {
                        JumpToQueryIdxCommand.Execute(Math.Max(0,nextSelectQueryIdx - 3));
                        while (IsBusy) { await Task.Delay(10); }
                    }

                    if (TailQueryIdx - curRightMostSelectQueryIdx <= RemainingItemsCountThreshold + 1) {
                        double nextOffset = FindTileOffsetX(nextSelectQueryIdx);
                        double curOffset =
                            curRightMostSelectQueryIdx >= 0 ?
                                 FindTileOffsetX(curRightMostSelectQueryIdx) : 0;
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
                    int nextItemIdx = Math.Min(TotalTilesInQuery-1, curItemIdx + 1);

                    ClearClipSelection();
                    Items[nextItemIdx].ResetSubSelection();
                    StoreSelectionState(Items[nextItemIdx]);
                }
            },
            () => !IsBusy);

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

                if (prevSelectQueryIdx >= 0) {
                    if (needJump) {
                        JumpToQueryIdxCommand.Execute(Math.Max(0, prevSelectQueryIdx - 3));
                        while (IsBusy) { await Task.Delay(10); }
                    }

                    if ((HeadQueryIdx == 0 && Math.Abs(ScrollOffset) > 0.01) ||
                        (curLeftMostSelectQueryIdx - HeadQueryIdx <= 1)) {
                        double prevOffset = FindTileOffsetX(prevSelectQueryIdx);
                        double curOffset =
                            curLeftMostSelectQueryIdx >= 0 ?
                                 FindTileOffsetX(curLeftMostSelectQueryIdx) : 0;
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
                    int prevItemIdx = Math.Max(0,curItemIdx - 1);

                    ClearClipSelection();
                    Items[prevItemIdx].ResetSubSelection();
                    StoreSelectionState(Items[prevItemIdx]);
                }
            },
            ()=>!IsBusy);

        public ICommand SelectAllCommand => new RelayCommand(
            () => {
                ClearClipSelection();
                foreach (var ctvm in Items) {
                    ctvm.IsSelected = true;
                }
            });

        public ICommand ChangeSelectedClipsColorCommand => new RelayCommand<object>(
             (hexStr) => {
                PrimaryItem.PrimaryItem.SetColorCommand.Execute(hexStr.ToString());
            });

        public ICommand CopySelectedClipsCommand => new RelayCommand(
            async () => {
                var ido = await GetDataObjectFromSelectedClips(false, false);
                MpClipboardHelper.MpClipboardManager.SetDataObjectWrapper(ido);
            });

        public ICommand PasteSelectedClipsCommand => new RelayCommand<object>(
            (args) => {
                if (args != null && args is int appId && appId > 0) {
                    //when pasting to a user defined application
                    _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                    _selectedPasteToAppPathViewModel = MpPasteToAppPathViewModelCollection.Instance.FindById(appId);
                } else if (args != null && args is IntPtr handle && handle != IntPtr.Zero) {
                    //when pasting to a running application
                    _selectedPasteToAppPathWindowHandle = handle;
                    _selectedPasteToAppPathViewModel = null;
                } else {
                    _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                    _selectedPasteToAppPathViewModel = null;
                }
                SelectedItems.ForEach(x => x.SubSelectAll());
                //In order to paste the app must hide first 
                //this triggers hidewindow to paste selected items
                IsPastingSelected = true;
                MpMainWindowViewModel.Instance.HideWindowCommand.Execute(true);
                IsPastingSelected = false;
            },
            (args) => {
                return MpMainWindowViewModel.Instance.IsShowingDialog == false &&
                    //!IsAnyTileExpanded &&
                    !IsAnyEditingClipTile &&
                    !IsAnyEditingClipTitle &&
                    !IsAnyPastingTemplate &&
                    !MpPreferences.IsTrialExpired;

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
                while(IsBusy) { await Task.Delay(100); }

                IsBusy = true;

                await Task.WhenAll(SelectedModels.Select(x => x.DeleteFromDatabaseAsync()).ToArray());

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


                await civm.UpdateColorPallete();
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
                SelectedItems[0].IsReadOnly = false;
            },
            () => {
                if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return false;
                }
                return SelectedItems.Count == 1 && SelectedItems[0].SelectedItems.Count == 1;
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
                    MpClipboardHelper.MpClipboardManager.SetDataObjectWrapper(
                        new MpDataObject() {
                            DataFormatLookup = new Dictionary<MpClipboardFormat, string>() { 
                                { 
                                    MpClipboardFormat.Bitmap, 
                                    bmpSrc.ToBase64String() 
                                } 
                            }
                    });
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

        #endregion
    }
    public enum MpExportType {
        None = 0,
        Files,
        Csv,
        Zip
    }
}
