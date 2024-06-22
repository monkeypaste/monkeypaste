using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTrayViewModel :
        MpAvSelectorViewModelBase<object, MpAvTagTileViewModel>,
        MpAvIMenuItemViewModel,
        MpIAsyncCollectionObject,
        MpIBadgeCountViewModel,
        MpIHasDragOverProperty,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpISidebarItemViewModel,
        MpITagQueryTools,
        MpIPopupMenuPicker {
        #region Private Variables
        #endregion

        #region Statics


        private static MpAvTagTrayViewModel _instance;
        public static MpAvTagTrayViewModel Instance => _instance ?? (_instance = new MpAvTagTrayViewModel());

        #endregion

        #region Constants

        public const int DEFAULT_SELECTED_TAG_ID = MpTag.TodayTagId;

        #endregion

        #region Interfaces

        #region MpAvIMenuItemViewModel Implementation

        string MpAvIMenuItemViewModel.IconTintHexStr =>
            null;
        string MpAvIMenuItemViewModel.IconBorderHexColor =>
            MpSystemColors.Transparent;


        ICommand MpAvIMenuItemViewModel.Command => null;
        object MpAvIMenuItemViewModel.CommandParameter => null;
        string MpAvIMenuItemViewModel.Header => UiStrings.ClipTileTagContextMenuLabel;
        object MpAvIMenuItemViewModel.IconSourceObj => "TagColorImage";
        string MpAvIMenuItemViewModel.InputGestureText { get; }
        bool MpAvIMenuItemViewModel.StaysOpenOnClick => false;
        bool MpAvIMenuItemViewModel.HasLeadingSeparator => true;
        bool MpAvIIsVisibleViewModel.IsVisible => true;
        bool? MpAvIMenuItemViewModel.IsChecked => false;
        bool MpAvIMenuItemViewModel.IsThreeState => false;
        bool MpAvIMenuItemViewModel.IsSubMenuOpen { get; set; }
        MpMenuItemType MpAvIMenuItemViewModel.MenuItemType =>
            MpMenuItemType.Default;
        IEnumerable<MpAvIMenuItemViewModel> MpAvIMenuItemViewModel.SubItems =>
            RootLinkableItems;

        #endregion

        #region MpIBadgeCountViewModel Implementation

        // NOTE since badges
        public int BadgeCount =>
            Items.Any(x => x.BadgeCount > 0) ? 1 : 0;

        #endregion

        #region MpIPopupMenuPicker Implementation

        public MpAvMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedTagIds, bool recursive) {
            // used for component parameter picker
            return new MpAvMenuItemViewModel() {
                SubItems =
                    RootLinkableItems
                        .Select(x => x.GetMenu(cmd, cmdArg, selectedTagIds, recursive))
                        .Cast<MpAvIMenuItemViewModel>().ToList()
            };
        }

        #endregion

        #region MpIHasDragOverProperty Implementation

        bool MpIHasDragOverProperty.IsDragOver {
            get => IsPinTrayDragOver;
            set => IsPinTrayDragOver = value;
        }
        #endregion

        #region MpITagQueryTools Implementation

        IEnumerable<int> MpITagQueryTools.GetSelfAndAllAncestorTagIds(int tagId) {
            if (Items.FirstOrDefault(x => x.TagId == tagId) is MpAvTagTileViewModel ttvm) {
                return
                    ttvm
                    .SelfAndAllAncestors
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.TagId);
            }
            return [tagId];
        }
        IEnumerable<int> MpITagQueryTools.GetSelfAndAllDescendantsTagIds(int tagId) {
            if (Items.FirstOrDefault(x => x.TagId == tagId) is MpAvTagTileViewModel ttvm) {
                return
                    ttvm
                    .SelfAndAllDescendants
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.TagId);
            }
            return [tagId];
        }
        #endregion

        #region MpISidebarItemViewModel Implementation
        public double SidebarWidth { get; set; } = 0;
        public double SidebarHeight { get; set; }

        public double DefaultSidebarWidth {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return 225;// MpMeasurements.Instance.DefaultTagTreePanelWidth;
                } else {
                    return MpAvMainView.Instance.MainWindowTrayGrid.Bounds.Width; //MpAvMainWindowViewModel.Instance.MainWindowWidth;
                }
            }
        }
        public double DefaultSidebarHeight {
            get {
                if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                    return MpAvMainView.Instance.MainWindowTrayGrid.Bounds.Height;
                } else {
                    return 250;
                }
            }
        }

        public string SidebarBgHexColor =>
            (Mp.Services.PlatformResource.GetResource("TagTreeBgBrush") as IBrush).ToHex();

        bool MpISidebarItemViewModel.CanResize =>
            true;// MpAvMainWindowViewModel.Instance.IsVerticalOrientation;
        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }


        #endregion

        #endregion

        #region Properties

        #region View Models

        public override MpAvTagTileViewModel LastSelectedItem =>
            base.LastSelectedItem != null &&
            base.LastSelectedItem.LastSelectedDateTime >= Mp.Services.StartupState.LoadedDateTime ?
                base.LastSelectedItem :
                null;
        public override MpAvTagTileViewModel SelectedItem {
            get {
                return Items.FirstOrDefault(x => x.TagId == SelectedItemId);
            }
            set {
                // NOTE always set for requery
                _selectedItemId = value == null ? 0 : value.TagId;
                Items.ForEach(x => x.IsSelected = x.TagId == _selectedItemId);
            }
        }

        public MpAvTagTileViewModel DefaultItem =>
            Items.FirstOrDefault(x => x.TagId == DEFAULT_SELECTED_TAG_ID);
        public MpAvTagTileViewModel LastSelectedActiveItem =>
            Items
            .Where(x => x.IsNotGroupTag && x.LastSelectedDateTime > MpAvPrefViewModel.Instance.StartupDateTime)
            .OrderByDescending(x => x.LastSelectedDateTime)
            .FirstOrDefault();

        public IEnumerable<MpAvTagTileViewModel> RootLinkableItems =>
            Items
            .Where(x => !x.IsAllTag && x.ParentTagId == MpTag.CollectionsTagId)
            .ToList();

        public IEnumerable<MpAvTagTileViewModel> PinnedItems =>
            Items
            .Where(x => x.IsModelPinned).OrderBy(x => x.PinSortIdx)
            .ToList();

        public MpAvTagTileViewModel SelectedPinnedItem {
            get => PinnedItems.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != SelectedPinnedItem) {
                    PinnedItems.ForEach(x => x.IsSelected = x == value);
                }
            }
        }

        public IEnumerable<MpAvTagTileViewModel> RootItems =>
            Items
            .Where(x => x.ParentTagId == 0)
            .ToList();
        public MpAvTagTileViewModel FiltersTagViewModel =>
            Items.FirstOrDefault(x => x.TagId == MpTag.FiltersTagId);
        public MpAvTagTileViewModel TrashTagViewModel =>
            Items.FirstOrDefault(x => x.TagId == MpTag.TrashTagId);
        public MpAvTagTileViewModel AllTagViewModel =>
            Items.FirstOrDefault(x => x.TagId == MpTag.AllTagId);

        #endregion

        #region State

        public bool IsAnyDragging =>
            Items.Any(x => x.IsDragging);
        public bool IsAnyPinTagDragging =>
            Items.Any(x => x.IsPinTagDragging);

        public bool IsPinTrayDragOver { get; set; }

        private int _selectedItemId;
        public int SelectedItemId {
            // NOTE only used w/ QueryInfo
            get {
                return _selectedItemId;
            }
            set {
                SelectTagCommand.Execute(value);
            }
        }

        public int LastActiveId =>
            LastSelectedActiveItem == null ? 0 : LastSelectedActiveItem.TagId;

        public bool CanSelect =>
            !MpAvMainWindowViewModel.Instance.IsAnyItemDragging;
        public bool IsSelecting { get; set; } = false;
        //public bool IsNavButtonsVisible => MpAvMainWindowViewModel.Instance.IsHorizontalOrientation && 
        //                                    TagTrayTotalWidth > MaxTagTrayScreenWidth;
        public bool IsNavButtonsVisible { get; set; } = false;

        public bool IsAnyBusy =>
            IsBusy ||
            Items.Any(x => x.IsBusy);

        public List<int> TrashedCopyItemIds { get; set; } = new List<int>();


        public bool IsAnyTagActive =>
            LastSelectedActiveItem != null;

        public bool IsAnyTagSelected =>
            SelectedItem != null;

        #endregion

        #region Layout        
        public double NavButtonSize => 25;
        //public double TagTrayTotalWidth => PinnedItems.Sum(x => x.TagTileTrayWidth);
        public double TagTrayTotalWidth { get; set; }

        public MpRect TagTrayBounds { get; set; }
        #endregion

        #region Appearance

        public string LastSelectedTagName =>
            LastSelectedActiveItem == null ? "All" : LastSelectedActiveItem.TagName;
        public bool ShowAddTagButton {
            get {
                if (SelectedItem == null) {
                    return false;
                }
                return SelectedItem.CanAddChild;
            }
        }
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors

        private static int createCount = 0;
        public MpAvTagTrayViewModel() : base(null) {
            createCount++;
            PropertyChanged += MpTagTrayViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task InitAsync() {
            IsBusy = true;

            Mp.Services.TagQueryTools = this;

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            MpDb.SyncAdd += MpDbObject_SyncAdd;
            MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.SyncDelete += MpDbObject_SyncDelete;

            Items.Clear();
            var root_tags = await MpDataModelProvider.GetChildTagsAsync(0);
            foreach (var root_tag in root_tags.OrderBy(x => x.TreeSortIdx)) {
                _ = await CreateTagTileViewModelAsync(root_tag);
            }

            while (RootItems.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            TrashedCopyItemIds = await MpDataModelProvider.GetCopyItemIdsForTagAsync(MpTag.TrashTagId);

            Items.CollectionChanged += TagTileViewModels_CollectionChanged;

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PinnedItems));
            OnPropertyChanged(nameof(RootItems));

            if (FiltersTagViewModel != null) {
                FiltersTagViewModel.IsExpanded = true;
            }

            IsBusy = false;
        }

        private void TagTileViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        }

        public void ClearTagEditing() {
            Items.ForEach(x => x.IsTagNameReadOnly = true);
            //PinnedItems.ForEach(x => x.IsTagNameReadOnly = true);
        }
        public void ClearTagSelection() {
            ClearTagEditing();
            Items.ForEach(x => x.IsSelected = false);
            //PinnedItems.ForEach(x => x.IsSelected = false);
        }

        public async Task UpdateAllClipCountsAsync() {
            // NOTE1 only used for trash link/unlinking
            // NOTE2 only call from outermost command to avoid successively calling if its a mass restore/delete
            // NOTE3 including all link tags AND active if its a query tag so that updates too
            await Task.WhenAll(
                Items
                .Where(x => x.IsLinkTag || LastActiveId == x.TagId)
                .Select(x => x.UpdateClipCountAsync()));
        }

        public void SetNextTrashCleanupDate() {
            switch (MpAvPrefViewModel.Instance.TrashCleanupModeType) {
                case MpTrashCleanupModeType.Daily:
                    MpAvPrefViewModel.Instance.NextTrashEmptyDateTime = DateTime.Now + TimeSpan.FromDays(1);
                    break;
                case MpTrashCleanupModeType.Weekly:
                    MpAvPrefViewModel.Instance.NextTrashEmptyDateTime = DateTime.Now + TimeSpan.FromDays(7);
                    break;
                case MpTrashCleanupModeType.Monthly:
                    MpAvPrefViewModel.Instance.NextTrashEmptyDateTime = DateTime.Now + TimeSpan.FromDays(30);
                    break;
                case MpTrashCleanupModeType.Never:
                    MpAvPrefViewModel.Instance.NextTrashEmptyDateTime = DateTime.MaxValue;
                    break;
            }
        }
        #endregion

        #region Protected Methods

        #region Db Events
        #endregion

        #endregion

        #region Private Methods

        private void MpTagTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        if (SelectedItem != null) {
                            // only scroll selected into view when tag tree is loaded
                            SelectedItem.BringIntoTreeViewCommand.Execute(null);
                        }
                    }
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(RootItems));
                    break;
                case nameof(SelectedItem):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsActiveTag)));
                    OnPropertyChanged(nameof(ShowAddTagButton));
                    OnPropertyChanged(nameof(ShowAddTagButton));
                    OnPropertyChanged(nameof(LastSelectedTagName));
                    MpMessenger.SendGlobal(MpMessageType.FocusItemChanged);
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.MainWindowOrientationChangeEnd:
                    OnPropertyChanged(nameof(TagTrayTotalWidth));
                    break;
                //case MpMessageType.MainWindowOpened:

                //    OnPropertyChanged(nameof(PinnedItems));
                //    OnPropertyChanged(nameof(MaxTagTrayScreenWidth));
                //    break;

                case MpMessageType.TraySelectionChanged:
                    HandleClipTraySelectionChange();//.FireAndForgetSafeAsync(this);
                    break;

                case MpMessageType.QueryChanged:
                    if (SelectedItem == null || !SelectedItem.IsQueryTag) {
                        break;
                    }
                    SelectedItem.SortType = MpAvClipTileSortFieldViewModel.Instance.SelectedSortType;
                    SelectedItem.IsSortDescending = MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending;
                    break;
                case MpMessageType.MainWindowLoadComplete:
                    if (DateTime.Now >= MpAvPrefViewModel.Instance.NextTrashEmptyDateTime) {
                        Dispatcher.UIThread.Post(async () => {
                            await Task.Delay(3_000);
                            EmptyTrashCommand.Execute(null);
                            SetNextTrashCleanupDate();
                        });
                    }
                    break;
            }
        }

        private void HandleClipTraySelectionChange() {
            var ctrvm = MpAvClipTrayViewModel.Instance;

            if (ctrvm.SelectedItem == null) {
                Items.ForEach(x => x.IsLinkedToSelectedClipTile = false);
                return;
            }

            var tag_ids_for_selected_copy_item = MpDataModelProvider.GetTagIdsForCopyItem(ctrvm.SelectedItem.CopyItemId);
            Items.ForEach(x => x.UpdateLinkToSelectedClipTile(tag_ids_for_selected_copy_item));

            // Notify clip tray context menu changed if was selected w/ right click
            MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.ContextMenuViewModel));
        }

        private async Task<MpAvTagTileViewModel> CreateTagTileViewModelAsync(MpTag tag) {
            var ttvm = new MpAvTagTileViewModel(this);
            await ttvm.InitializeAsync(tag);
            return ttvm;
        }

        #region Model Sync Events
        private void MpDbObject_SyncDelete(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            Dispatcher.UIThread.Post(() => {
                if (sender is MpTag t) {
                    var ttvmToRemove = Items.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
                    if (ttvmToRemove != null) {
                        ttvmToRemove.Tag.StartSync(e.SourceGuid);
                        ttvmToRemove.DeleteThisTagCommand.Execute(null);
                        ttvmToRemove.Tag.EndSync();
                    }
                }
            });
        }

        private void MpDbObject_SyncUpdate(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            //Dispatcher.UIThread.Post(() => {
            //});
        }

        private void MpDbObject_SyncAdd(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            // Dispatcher.UIThread.Post(async() => {
            //if (sender is MpTag t) {
            //    t.StartSync(e.SourceGuid);
            //    var dupCheck = Items.Where(x => x.Tag.Guid == t.Guid).FirstOrDefault();
            //    if (dupCheck == null) {
            //        var ttvm = await CreateTagTileViewModel(t);
            //        Items.Add(ttvm);
            //    } else {
            //        MpConsole.WriteTraceLine(@"Warning, attempting to add existing tag: " + dupCheck.TagName + " ignoring and updating existing.");
            //        dupCheck.Tag = t;
            //    }
            //    t.EndSync();
            //}
            // });
        }

        #endregion

        #endregion

        #region Commands

        private async Task EmptyOrRestoreAllTrashedItemsAsync(bool isRestore) {
            var confirm_result = await Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                    title: UiStrings.CommonConfirmLabel,
                    //message: $"Are you sure you want to {(isRestore ? "restore" : "permanently delete")} all {TrashedCopyItemIds.Count} item(s)?",
                    message: isRestore ?
                        UiStrings.RestoreTrashNtfText.Format(TrashedCopyItemIds.Count) :
                        UiStrings.EmptyTrashNtfText.Format(TrashedCopyItemIds.Count),
                    iconResourceObj: "WarningImage"); ;

            if (!confirm_result) {
                // canceled empty trash
                return;
            }

            // either MpCopyItemTag or MpCopyItem
            IEnumerable<MpDbModelBase> to_delete =
                isRestore ?
                await MpDataModelProvider.GetCopyItemTagsForTagAsync(MpTag.TrashTagId) :
                await MpDataModelProvider.GetCopyItemsByIdListAsync(TrashedCopyItemIds);



            await Task.WhenAll(to_delete.Cast<MpDbModelBase>().Select(x => x.DeleteFromDatabaseAsync()));
            if (TrashTagViewModel.IsActiveTag) {
                Mp.Services.Query.NotifyQueryChanged(true);
            }
            await UpdateAllClipCountsAsync();

        }
        public ICommand RestoreAllTrashCommand => new MpAsyncCommand(
            async () => {
                await EmptyOrRestoreAllTrashedItemsAsync(true);
            },
            () => {
                bool can_restore = TrashedCopyItemIds.Any() && !MpAvAccountTools.Instance.IsContentAddPausedByAccount;
                if (!can_restore) {
                    MpConsole.WriteLine("EmptyTrashCommand CanExecute: " + can_restore);
                    MpConsole.WriteLine($"TrashedCopyItemId Count: {TrashedCopyItemIds.Count}");
                    MpConsole.WriteLine($"IsContentAddPausedByAccount: {MpAvAccountTools.Instance.IsContentAddPausedByAccount}");
                }
                return can_restore;
            });

        public ICommand EmptyTrashCommand => new MpAsyncCommand(
            async () => {
                await EmptyOrRestoreAllTrashedItemsAsync(false);
            },
            () => {
                bool can_empty = TrashedCopyItemIds.Any();
                if (!can_empty) {
                    MpConsole.WriteLine("EmptyTrashCommand CanExecute: " + can_empty);
                    MpConsole.WriteLine($"TrashedCopyItemId Count: {TrashedCopyItemIds.Count}");
                }
                return can_empty;
            });

        public MpIAsyncCommand<object> SelectTagAndBringIntoTreeViewCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpAvTagTileViewModel ttvm_to_select = null;
                if (args is MpAvTagTileViewModel) {
                    ttvm_to_select = args as MpAvTagTileViewModel;
                } else if (args is int tagId) {
                    ttvm_to_select = Items.FirstOrDefault(x => x.TagId == tagId);
                }
                if (ttvm_to_select == null) {
                    return;
                }
                if (MpAvSearchCriteriaItemCollectionViewModel.Instance.IsCriteriaWindowOpen &&
                    !MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    // when saving from float window show mw to rename/confirm new query tag
                    if (!MpAvMainWindowViewModel.Instance.ShowMainWindowCommand.CanExecute(null)) {
                        // why not?
                        MpDebug.Break();
                    } else {
                        MpAvMainWindowViewModel.Instance.ShowMainWindowCommand.Execute(null);
                        while (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                            await Task.Delay(100);
                        }
                    }
                }
                // NOTE this should only occur for new searches, onced created saving is by HasModelChanged
                var ttrvm = MpAvTagTrayViewModel.Instance;
                int waitTimeMs = MpAvSidebarItemCollectionViewModel.Instance.SelectedItem == ttrvm ? 0 : 500;
                MpAvSidebarItemCollectionViewModel.Instance.SelectSidebarItemCommand.Execute(ttrvm);
                // wait for panel open
                await Task.Delay(waitTimeMs);

                while (!ttrvm.SelectTagCommand.CanExecute(ttvm_to_select)) {
                    // wait to ensure selection occurs (is rejected when duplicating a tag for some reason)
                    await Task.Delay(100);
                }
                ttrvm.SelectTagCommand.Execute(ttvm_to_select);
                while (ttrvm.IsSelecting) {
                    await Task.Delay(100);
                }
            });
        public ICommand ToggleTileIsPinnedCommand => new MpCommand<object>(
            (args) => {
                var ttvm = args as MpAvTagTileViewModel;
                if (ttvm == null) {
                    return;
                }
                if (ttvm.IsModelPinned) {
                    // unpin
                    ttvm.PinSortIdx = -1;
                } else {
                    // pin
                    ttvm.PinSortIdx = PinnedItems.Count();
                }
                OnPropertyChanged(nameof(ttvm.IsModelPinned));
                OnPropertyChanged(nameof(PinnedItems));
                OnPropertyChanged(nameof(IsNavButtonsVisible));
                MpMessenger.SendGlobal(MpMessageType.FilterItemSizeChanged);

                PinnedItems.OrderBy(x => x.PinSortIdx).ForEach((x, idx) => x.PinSortIdx = idx);
            }, (args) => {
                return args is MpAvTagTileViewModel ttvm && ttvm.CanPin;
            });

        public MpIAsyncCommand<object> SelectTagCommand => new MpAsyncCommand<object>(
            async (args) => {
                IsSelecting = true;

                int tagId;
                if (args == null) {
                    tagId = DEFAULT_SELECTED_TAG_ID;
                } else if (args is MpAvTagTileViewModel ttvm) {
                    tagId = ttvm.TagId;
                } else if (args is int) {
                    tagId = (int)args;
                } else if (args is string shorcutCommandParamTagIdStr &&
                    int.Parse(shorcutCommandParamTagIdStr) is int cmdTagId) {
                    // from shortcut
                    tagId = cmdTagId;
                } else {
                    MpDebug.Break();
                    tagId = MpTag.AllTagId;
                }
                if (Items.FirstOrDefault(x => x.TagId == tagId) is MpAvTagTileViewModel sttvm &&
                    !sttvm.CanSelect) {
                    MpConsole.WriteLine($"Rejecting select tag '{sttvm}'.");
                    IsSelecting = false;
                    return;
                }
                bool has_active_established = LastSelectedActiveItem != null;
                ClearTagSelection();


                _selectedItemId = tagId;
                Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                OnPropertyChanged(nameof(LastSelectedActiveItem));
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(SelectedPinnedItem));
                OnPropertyChanged(nameof(IsAnyTagActive));

                if (SelectedItem != null) {
                    SelectedItem.AllAncestors.ForEach(x => x.IsExpanded = true);
                    SelectedItem.BringIntoTreeViewCommand.Execute(null);
                }

                MpMessenger.SendGlobal(MpMessageType.TagSelectionChanged);

                if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading &&
                    SelectedItem != null && SelectedItem.IsNotGroupTag) {
                    // NOTE only force requery when its initial tag select
                    // or requery is rejected when no filters have been changed (common case)
                    bool is_initial_select = !has_active_established;
                    Mp.Services.Query.NotifyQueryChanged(is_initial_select);
                    // wait for query to execute
                    await Task.Delay(300);
                    while (MpAvClipTrayViewModel.Instance.IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                        // expand query tray on mobile
                        MpAvClipTrayViewModel.Instance.ExpandQueryTrayCommand.Execute(null);
                    }
                }
                IsSelecting = false;
            },
            (args) => {
                return
                    args != null &&
                    !IsSelecting &&
                    CanSelect; // rejects select during dnd (only condition)
            });


        #endregion
    }
}
