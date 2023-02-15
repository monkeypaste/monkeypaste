using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTileViewModel :
        MpAvTreeSelectorViewModelBase<MpAvTagTrayViewModel, MpAvTagTileViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpAvIShortcutCommandViewModel,
        MpIUserColorViewModel,
        MpIActionComponent,
        MpIContextMenuViewModel,
        MpIPopupMenuPicker {

        #region Private Variables
        private object _notifierLock = new object();

        private string _originalTagName = string.Empty;
        private bool _wasEditingName = false;

        #endregion

        #region Statics

        public static MpShape GROUP_SHAPE = new MpEllipse(MpPoint.Zero, new MpSize(20, 20));
        public static MpShape QUERY_SHAPE = MpTriangle.CreateEqualateralTriangle(MpPoint.Zero, 20);

        #endregion

        #region Interfaces

        #region MpAvTreeSelectorViewModelBase Implementation

        public override MpAvTagTileViewModel ParentTreeItem => Parent.Items.FirstOrDefault(x => x.TagId == ParentTagId);
        public override IEnumerable<MpAvTagTileViewModel> Children => SortedItems;

        #endregion

        #region MpIPopupMenuPicker Implementation

        public MpMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedTagIds, bool recursive) {
            return new MpMenuItemViewModel() {
                Header = TagName,
                Command = cmd,
                CommandParameter = TagId,
                IsChecked = selectedTagIds.Contains(TagId),
                IconHexStr = TagHexColor,
                SubItems = recursive ? Items.Cast<MpIPopupMenuPicker>().Select(x => x.GetMenu(cmd, cmdArg, selectedTagIds, recursive)).ToList() : null
            };
        }

        #endregion

        #region MpISelectableViewModel Implementation

        //private bool _isSelected = false;
        public bool IsSelected {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.SelectedItemId == TagId;
            }
            set {
                if (Parent == null) {
                    return;
                }
                if (value) {
                    if (Parent.IsSelecting) {
                        // break here or stack overflows
                        return;
                    }
                    // setter calls SelectTagCommand
                    Parent.SelectedItemId = TagId;
                }
            }
        }

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region MpIUserColorViewModel Implementation

        public string UserHexColor {
            get => TagHexColor;
            set => TagHexColor = value;
        }

        #endregion

        #region MpIContextMenuItemViewModel Implementation

        //content menu item
        public MpMenuItemViewModel ContentMenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    Header = TagName,
                    Command = ToggleLinkToSelectedClipTileCommand,
                    IsChecked = IsLinkedToSelectedClipTile,
                    IconHexStr = TagHexColor,
                    IconShape = MenuIconShape,
                    ShortcutArgs = new object[] { MpShortcutType.SelectTag, TagId },
                    SubItems = Items.Select(x => x.ContentMenuItemViewModel).ToList()
                };
                //return GetTagMenu(ToggleLinkToSelectedClipTileCommand, IsLinkedToSelectedClipTile, new object[] { MpShortcutType.SelectTag, TagId });
            }
        }

        public MpMenuItemViewModel ContextMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            IsVisible = !IsTagReadOnly,
                            Header = "Rename",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("RenameImage") as string, //MpPlatformWrapper.Services.PlatformResource.GetResource("RenameIcon") as string,
                            Command = RenameTagCommand,
                        },
                        new MpMenuItemViewModel() {
                            IsVisible = CanHotkey,
                            Header = "Assign Hotkey",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("HotkeyImage") as string,
                            Command = AssignHotkeyCommand,
                            ShortcutArgs = new object[] { MpShortcutType.SelectTag, TagId },
                        },
                        new MpMenuItemViewModel() {
                            IsVisible = CanPin,
                            Header = IsModelPinned ? "Unpin" : "Pin",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("PinImage") as string,
                            Command = Parent.ToggleTileIsPinnedCommand,
                            CommandParameter = this
                        },
                        new MpMenuItemViewModel() { IsSeparator = true},
                        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(this),
                        new MpMenuItemViewModel() {
                            IsSeparator = true,
                            IsVisible = !IsTagReadOnly
                        },
                        new MpMenuItemViewModel() {
                            IsVisible = !IsTagReadOnly,
                            Header = "Delete",
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("DeleteImage") as string,
                            Command = DeleteThisTagCommand
                        }
                    }
                };
            }
        }
        public bool IsContextMenuOpen { get; set; } = false;

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpITriggerActionViewModel Implementation

        public void RegisterActionComponent(MpIInvokableAction mvm) {
            OnCopyItemLinked += mvm.OnActionInvoked;
            MpConsole.WriteLine($"TagTile {TagName} Registered {mvm.Label} matcher");
        }

        public void UnregisterActionComponent(MpIInvokableAction mvm) {
            OnCopyItemLinked -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"Matcher {mvm.Label} Unregistered from {TagName} TagAdd");
        }

        #endregion

        #region MpAvIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType => MpShortcutType.SelectTag;

        public MpAvShortcutViewModel ShortcutViewModel {
            get {
                if (Parent == null || Tag == null) {
                    return null;
                }
                var scvm = MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.CommandParameter == TagId.ToString() && x.ShortcutType == ShortcutType);

                if (scvm == null) {
                    scvm = new MpAvShortcutViewModel(MpAvShortcutCollectionViewModel.Instance);
                }

                return scvm;
            }
        }

        public string ShortcutKeyString => ShortcutViewModel.KeyString;

        public ICommand AssignCommand => AssignHotkeyCommand;

        #endregion
        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvTagTileViewModel> SortedItems => Items.OrderBy(x => x.TreeSortIdx);

        public MpMenuItemViewModel AddChildPopupMenuItemViewModel {
            get {
                if (!IsGroupTag) {
                    return null;
                }
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            IconSourceObj = "FolderImage",
                            Header = "Group",
                            Command = AddNewChildTagCommand,
                            CommandParameter = MpTagType.Group
                        },
                        new MpMenuItemViewModel() {
                            IconSourceObj = "BinocularsTiltedImage",
                            Header = "Search",
                            Command = AddNewChildTagCommand,
                            CommandParameter = MpTagType.Query
                        },
                    }
                };
            }
        }
        #endregion

        #region State

        //private int? _totalCopyItemCount = null; // only used by all tag
        //public int LinkedCopyItemCount {
        //    get {
        //        if(IsAllTag) {
        //            return _totalCopyItemCount == null ? 0 : _totalCopyItemCount.Value;
        //        }
        //        if(_linkedCopyItemIds == null) {
        //            return 0;
        //        }
        //        return _linkedCopyItemIds.Count();
        //    }
        //    set {
        //        if(!IsAllTag) {
        //            // should only by called for all tag
        //            Debugger.Break();
        //            return;
        //        }
        //        _totalCopyItemCount = value;
        //        OnPropertyChanged(nameof(LinkedCopyItemCount));
        //    }
        //}

        //private IEnumerable<int> _linkedCopyItemIds; // used by all link tags except all
        //public IEnumerable<int> LinkedCopyItemIds {
        //    get {
        //        if(IsAllTag) {
        //            // shouldn't be accessed
        //            Debugger.Break();
        //            return null;
        //        }
        //        return _linkedCopyItemIds;
        //    }
        //    set {
        //        if(_linkedCopyItemIds != value) {
        //            _linkedCopyItemIds = value;
        //            OnPropertyChanged(nameof(LinkedCopyItemIds));
        //        }
        //    }
        //}
        //public int LinkedCopyItemCount { get; set; } = -1;

        public bool IsActiveTag {
            get {
                if (IsGroupTag || Parent == null) {
                    return false;
                }
                return Parent.LastSelectedActiveItem == this;
            }
        }

        public bool IsAnyBusy => IsBusy || Children.Any(x => x.IsAnyBusy);

        public ObservableCollection<int> CopyItemIdsNeedingView { get; set; } = new ObservableCollection<int>();
        public int BadgeCount => CopyItemIdsNeedingView.Count;

        public bool CanAddChild {
            get {
                if (IsHelpTag || IsQueryTag) {
                    return false;
                }
                return true;
            }
        }

        public bool CanPin =>
            !IsGroupTag;
        public bool CanHotkey =>
            !IsGroupTag;

        public bool CanLinkContent => IsLinkTag;

        public bool IsNew {
            get {
                return Tag == null || Tag.Id <= 0;
            }
        }

        public bool IsTagReadOnly {
            get {
                return IsAllTag || IsHelpTag || IsFavoriteTag;
            }
        }

        public bool IsSudoTag {
            get {
                if (Tag == null) {
                    return false;
                }
                return Tag.Id == MpTag.AllTagId;
            }
        }

        public bool IsUserTag => !IsSudoTag;

        public bool IsDragOverTag { get; set; }

        public bool IsDragOverTagValid { get; set; }
        public bool IsAllTag => TagId == MpTag.AllTagId;
        public bool IsFavoriteTag => TagId == MpTag.FavoritesTagId;
        public bool IsHelpTag => TagId == MpTag.HelpTagId;

        public bool IsLinkTag => !IsQueryTag && !IsGroupTag && !IsHelpTag;
        public bool IsQueryTag => TagType == MpTagType.Query;
        public bool IsGroupTag => TagType == MpTagType.Group;
        public bool IsNotGroupTag => IsLinkTag || IsQueryTag;

        public bool IsTagNameReadOnly { get; set; } = true;
        public bool IsTagNameTextBoxFocused { get; set; } = false;
        public bool? IsLinkedToSelectedClipTile { get; set; } = false;

        public int? TagClipCount { get; set; }

        public string TagClipCountText =>
            TagClipCount == null ? string.Empty : TagClipCount.Value.ToString();

        public MpShape MenuIconShape =>
            IsLinkTag ? null : IsQueryTag ? QUERY_SHAPE : IsGroupTag ? GROUP_SHAPE : null;

        #endregion

        #region Appearance

        public string TagTextHexColor {
            get {
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return MpSystemColors.White;
                }
                if (!IsSelected && IsLinkedToSelectedClipTile.IsTrueOrNull() && !IsAllTag) {
                    return MpSystemColors.Yellow;
                }
                //if (IsSelected) {
                //    return MpSystemColors.White;
                //}
                //if(IsHovering) {
                //    return MpSystemColors.Black;
                //}
                return MpSystemColors.White;
            }
        }
        public MpRect ObservedTagTrayBounds { get; set; }

        #endregion

        #region Model

        public MpTagType TagType {
            get {
                if (Tag == null) {
                    return MpTagType.None;
                }
                return Tag.TagType;
            }
        }

        public MpContentSortType SortType {
            get {
                if (Tag == null || !IsQueryTag) {
                    if (Parent != null && IsGroupTag) {
                        // group tags pass through last selection
                        return Parent.LastSelectedActiveItem.SortType;
                    }
                    return MpAvClipTileSortFieldViewModel.Instance.SelectedSortType;
                }
                return Tag.SortType.Value;
            }
            set {
                if (Tag == null || !IsQueryTag) {
                    return;
                }
                if (SortType != value) {
                    Tag.SortType = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SortType));
                }
            }
        }
        public bool IsSortDescending {
            get {
                if (Tag == null || !IsQueryTag) {
                    if (Parent != null && IsGroupTag) {
                        // group tags pass through last selection
                        return Parent.LastSelectedActiveItem.IsSortDescending;
                    }
                    return MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending;
                }
                return Tag.IsSortDescending.IsTrue();
            }
            set {
                if (Tag == null || !IsQueryTag) {
                    return;
                }
                if (IsSortDescending != value) {
                    Tag.IsSortDescending = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsSortDescending));
                }
            }
        }

        public bool IsModelPinned {
            get {
                if (Tag == null) {
                    return false;
                }
                return Tag.PinSortIdx >= 0;
            }
        }

        public int ParentTagId {
            get {
                if (Tag == null) {
                    return 0;
                }
                return Tag.ParentTagId;
            }
            set {
                if (ParentTagId != value) {
                    Tag.ParentTagId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ParentTagId));
                }
            }
        }

        public int TagId {
            get {
                if (Tag == null) {
                    return 0;
                }
                return Tag.Id;
            }
            set {
                if (Tag.Id != value) {
                    Tag.Id = value;
                    OnPropertyChanged(nameof(TagId));
                }
            }
        }

        public int TreeSortIdx {
            get {
                if (Tag == null) {
                    return 0;
                }
                return Tag.TreeSortIdx;
            }
            set {
                if (Tag.TreeSortIdx != value) {
                    Tag.TreeSortIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TreeSortIdx));
                }
            }
        }

        public int PinSortIdx {
            get {
                if (Tag == null) {
                    return 0;
                }
                return Tag.PinSortIdx;
            }
            set {
                if (Tag.PinSortIdx != value) {
                    Tag.PinSortIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PinSortIdx));
                }
            }
        }
        public string TagName {
            get {
                if (Tag == null) {
                    return string.Empty;
                }
                return Tag.TagName;
            }
            set {
                if (TagName != value) {
                    Tag.TagName = value;
                    if (Tag.TagName.Trim() == string.Empty) {
                        Tag.TagName = "Untitled";
                        if (IsTagNameTextBoxFocused) {
                            IsTagNameReadOnly = false;
                        }
                    }
                    //HasModelChanged = true;
                    OnPropertyChanged(nameof(TagName));
                }
            }
        }

        public string TagHexColor {
            get {
                if (Tag == null) {
                    return string.Empty;
                }
                return Tag.HexColor;
            }
            set {
                if (TagHexColor != value) {
                    Tag.HexColor = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TagHexColor));
                }
            }
        }

        public MpTag Tag { get; set; }


        #endregion

        #endregion

        #region Events

        public event EventHandler<MpCopyItem> OnCopyItemLinked;
        public event EventHandler<MpCopyItem> OnCopyItemUnlinked;

        #endregion

        #region Public Methods

        public MpAvTagTileViewModel() : base(null) { }

        public MpAvTagTileViewModel(MpAvTagTrayViewModel parent) : base(parent) {
            MpDb.SyncAdd += MpDbObject_SyncAdd;
            MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.SyncDelete += MpDbObject_SyncDelete;

            PropertyChanged += MpTagTileViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            CopyItemIdsNeedingView.CollectionChanged += CopyItemIdsNeedingView_CollectionChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }


        public virtual async Task InitializeAsync(MpTag tag) {
            IsBusy = true;

            Tag = tag;
            var ctl = await MpDataModelProvider.GetChildTagsAsync(TagId);

            foreach (var ct in ctl.OrderBy(x => x.TreeSortIdx)) {
                var ttvm = await CreateChildTagTileViewModel(ct);
                Items.Add(ttvm);
            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            await UpdateClipCountAsync();

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(IsTagNameReadOnly));

            if (Parent.Items.All(x => x.TagId != TagId)) {
                // if not pinned item add to flat list
                Parent.Items.Add(this);
            }

            UpdateBadge();

            IsBusy = false;
        }

        public async Task<MpAvTagTileViewModel> CreateChildTagTileViewModel(MpTag tag) {
            MpAvTagTileViewModel ttvm = new MpAvTagTileViewModel(Parent);
            await ttvm.InitializeAsync(tag);
            return ttvm;
        }

        public async Task<bool> IsCopyItemLinkedAsync(int ciid) {
            if (ciid == 0 || Tag == null || Tag.Id == 0) {
                return false;
            }
            if (IsAllTag) {
                return true;
            }
            bool isLinked = await MpDataModelProvider.IsTagLinkedWithCopyItemAsync(Tag.Id, ciid);
            return isLinked;
        }

        public void UpdateLinkToSelectedClipTile(IEnumerable<int> assocTagIds) {
            if (IsAllTag) {
                // always linked
                IsLinkedToSelectedClipTile = true;
                return;
            }
            if (assocTagIds.Any(x => x == TagId)) {
                IsLinkedToSelectedClipTile = true;
            } else if (AllDescendants.Cast<MpAvTagTileViewModel>().Any(x => assocTagIds.Any(y => y == x.TagId))) {
                IsLinkedToSelectedClipTile = null;
            } else {
                IsLinkedToSelectedClipTile = false;
            }
        }

        public void UpdateTreeSortOrder() {
            SortedItems.ForEach((x, idx) => x.TreeSortIdx = idx);
        }

        public override void DisposeViewModel() {
            base.DisposeViewModel();
            MpDb.SyncAdd -= MpDbObject_SyncAdd;
            MpDb.SyncUpdate -= MpDbObject_SyncUpdate;
            MpDb.SyncDelete -= MpDbObject_SyncDelete;

            PropertyChanged -= MpTagTileViewModel_PropertyChanged;
        }

        public override string ToString() {
            return $"[{TagId}] {TagName}";
        }


        #endregion

        #region Protected Methods

        #region Db Events
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == TagId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if (e is MpCopyItem && IsAllTag) {
                Dispatcher.UIThread.Post(() => {
                    TagClipCount++;
                });
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == TagId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if (e is MpTag t && t.Id == TagId) {
                Dispatcher.UIThread.Post(() => {
                    //await InitializeAsync(t);
                    OnPropertyChanged(nameof(TagName));
                    OnPropertyChanged(nameof(TagHexColor));
                    OnPropertyChanged(nameof(TagTextHexColor));
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == TagId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if (e is MpCopyItemTag cit && cit.TagId == TagId) {
                UnlinkCopyItemCommand.Execute(cit.CopyItemId);
            } else if (e is MpCopyItem && IsAllTag) {
                Dispatcher.UIThread.Post(() => {
                    TagClipCount--;
                });
            }
        }
        #endregion

        #endregion

        #region Private Methods

        protected virtual void MpTagTileViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsTagNameReadOnly):
                    if (!IsTagNameReadOnly) {
                        _wasEditingName = true;
                        _originalTagName = TagName;
                    } else {
                        if (_wasEditingName) {
                            _wasEditingName = false;
                            if (TagName != _originalTagName) {
                                Task.Run(async () => {
                                    await Tag.WriteToDatabaseAsync();
                                });
                            }
                        }
                    }
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        //if (!IsExpanded) {
                        //    IsExpanded = true;
                        //}
                        if (IsQueryTag) {
                            MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending = IsSortDescending;
                            MpAvClipTileSortFieldViewModel.Instance.SelectedSortType = SortType;
                        }
                        Parent.SelectTagCommand.Execute(this);
                    } else {
                        IsTagNameReadOnly = true;
                        OnPropertyChanged(nameof(IsActiveTag));
                    }
                    break;
                case nameof(IsActiveTag):
                    if (IsActiveTag) {
                        break;
                    }
                    if (IsQueryTag) {
                        UpdateClipCountAsync().FireAndForgetSafeAsync(this);
                    }
                    break;
                case nameof(IsTagNameTextBoxFocused):

                    if (!IsTagNameTextBoxFocused) {
                        FinishRenameTagCommand.Execute(null);
                    }
                    break;
                case nameof(IgnoreHasModelChanged):
                    if (!IgnoreHasModelChanged && HasModelChanged) {
                        OnPropertyChanged(nameof(HasModelChanged));
                    }
                    break;
                case nameof(HasModelChanged):
                    if (IsBusy) {
                        return;
                    }
                    if (HasModelChanged) {
                        //if(SuprressNextHasModelChangedHandling) {
                        //    HasModelChanged = false;
                        //    SuprressNextHasModelChangedHandling = false;
                        //}
                        if (IgnoreHasModelChanged) {
                            break;
                        }
                        Task.Run(async () => {
                            IsBusy = true;
                            await Tag.WriteToDatabaseAsync();
                            HasModelChanged = false;
                            IsBusy = false;
                        });
                    }

                    break;
                case nameof(TagHexColor):
                    Dispatcher.UIThread.Post(async () => {
                        while (HasModelChanged) {
                            await Task.Delay(100);
                        }
                        await Task.WhenAll(MpAvClipTrayViewModel.Instance.Items.Select(x => x.InitTitleLayers()));
                    });
                    break;
                //case nameof(TagTileTrayWidth):
                case nameof(ObservedTagTrayBounds):
                    if (Parent == null) {
                        return;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsNavButtonsVisible));
                    Parent.OnPropertyChanged(nameof(Parent.TagTrayScreenWidth));
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(SortedItems));
                    break;
                case nameof(TreeSortIdx):
                    if (ParentTreeItem == null) {
                        break;
                    }
                    ParentTreeItem.OnPropertyChanged(nameof(ParentTreeItem.SortedItems));
                    break;
                case nameof(PinSortIdx):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.PinnedItems));
                    break;
                case nameof(IsLinkedToSelectedClipTile):

                    OnPropertyChanged(nameof(TagTextHexColor));
                    break;
                case nameof(IsSortDescending):
                case nameof(SortType):
                    if (!IsSelected) {
                        break;
                    }
                    if (e.PropertyName == nameof(IsSortDescending)) {
                        MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending = IsSortDescending;
                    } else {
                        MpAvClipTileSortFieldViewModel.Instance.SelectedSortType = SortType;
                    }
                    break;
            }
        }

        private void CopyItemIdsNeedingView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Remove) {
                SelfAndAllAncestors
                    .Cast<MpAvTagTileViewModel>()
                    .ForEach(x => x.OnPropertyChanged(nameof(x.BadgeCount)));
            }
        }


        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Remove) {
                Task.WhenAll(
                    SelfAndAllAncestors
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.UpdateClipCountAsync())).FireAndForgetSafeAsync();
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.RequeryCompleted:
                    UpdateBadge();
                    if (IsActiveTag && IsQueryTag) {
                        // repopulate count for query tag
                        UpdateClipCountAsync().FireAndForgetSafeAsync(this);
                    }
                    break;
                case MpMessageType.TrayScrollChanged:
                case MpMessageType.JumpToIdxCompleted:
                    UpdateBadge();
                    break;
                case MpMessageType.TraySelectionChanged:

                    break;
                case MpMessageType.QuerySortChanged:
                    if (!IsQueryTag || !IsActiveTag) {
                        break;
                    }
                    IgnoreHasModelChanged = true;
                    IsSortDescending = MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending;
                    SortType = MpAvClipTileSortFieldViewModel.Instance.SelectedSortType;
                    IgnoreHasModelChanged = false;
                    break;
            }
        }

        private async Task UpdateClipCountAsync() {
            if (IsGroupTag) {
                return;
            }
            await Dispatcher.UIThread.InvokeAsync(async () => {
                if (IsAllTag) {
                    if (TagClipCount == null) {
                        // startup case
                        TagClipCount = await MpDataModelProvider.GetTotalCopyItemCountAsync();
                    } else {
                        // ignored, all updated handled in db callbacks
                    }
                } else {
                    if (IsLinkTag) {
                        TagClipCount = await MpDataModelProvider.GetTotalCopyItemCountForTagAndAllDescendantsAsync(TagId);

                    } else {
                        // query tag
                        if (IsActiveTag) {
                            if (!MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive) {
                                // keeping this here for debugging,
                                // when this eval's update is called again when active though
                                //Debugger.Break();
                                TagClipCount = null;
                            } else {
                                TagClipCount =
                                    MpPlatform.Services.Query.TotalAvailableItemsInQuery;
                                //MpAvSearchCriteriaItemCollectionViewModel.Instance.TotalAvailableItemsInQuery;
                            }
                        } else {
                            TagClipCount = null;
                        }
                    }

                    //TagClipCount = 
                    //    SelfAndAllDescendants.Cast<MpAvTagTileViewModel>().SelectMany(x => x.LinkedCopyItemIds).Distinct().Count();
                }
                OnPropertyChanged(nameof(TagClipCountText));
            });
        }

        private void UpdateBadge() {
            Dispatcher.UIThread.VerifyAccess();

            var idsSeen = new List<int>();
            foreach (int ciid in CopyItemIdsNeedingView) {
                var civm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                if (civm != null) {
                    if (civm.IsPinned) {
                        // only mark item as seen if viewed in its query tray
                        continue;
                    }
                    if (civm.IsAnyQueryCornerVisible && IsSelected) {
                        idsSeen.Add(ciid);
                    }
                }

            }
            foreach (var idToRemove in idsSeen) {
                if (!CopyItemIdsNeedingView.Remove(idToRemove)) {
                    // how?
                    Debugger.Break();
                }
            }
        }

        private async Task NotifyTriggersAsync(int ciid, bool isLink) {
            IsBusy = true;

            if (isLink && OnCopyItemLinked.HasInvokers()) {
                var linked_ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(ciid);
                OnCopyItemLinked?.Invoke(this, linked_ci);
            } else if (!isLink && OnCopyItemUnlinked.HasInvokers()) {
                var unlinked_ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(ciid);
                OnCopyItemUnlinked?.Invoke(this, unlinked_ci);
            }

            IsBusy = false;
        }


        private async Task LinkOrUnlinkCopyItemAsync(int ciid, bool isLink) {
            IsBusy = true;

            if (isLink) {
                // try to create link, if it was created (and didn't already exist) notify any triggers
                int linkCount = await MpDataModelProvider.GetCopyItemCountForTagAsync(TagId);
                var cit = await MpCopyItemTag.Create(TagId, ciid, linkCount);

                if (!cit.WasDupOnCreate) {
                    CopyItemIdsNeedingView.Add(ciid);
                }

            } else {
                var cit = await MpDataModelProvider.GetCopyItemTagForTagAsync(ciid, TagId);

                if (cit != null) {
                    // only delete link/notify if exists
                    await cit.DeleteFromDatabaseAsync();
                }
                CopyItemIdsNeedingView.Remove(ciid);
            }

            Dispatcher.UIThread.VerifyAccess();

            foreach (MpAvTagTileViewModel this_or_ancestor_ttvm in SelfAndAllAncestors) {
                await this_or_ancestor_ttvm.UpdateClipCountAsync();

            }

            // await notify so IsBusy doesn't trip
            await NotifyTriggersAsync(ciid, isLink);

            if (MpAvClipTrayViewModel.Instance.PersistantSelectedItemId == ciid) {
                // trigger selection changed message to notify tag association change
                MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
            }
            MpAvClipTrayViewModel.Instance.UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();
            IsBusy = false;
        }


        #region Sync Event Handlers
        private void MpDbObject_SyncDelete(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            //throw new NotImplementedException();
        }

        private void MpDbObject_SyncUpdate(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            Dispatcher.UIThread.Post(() => {
                if (sender is MpTag t) {
                    if (Tag == null) {
                        return;
                    }
                    if (t.Guid == Tag.Guid) {
                        t.StartSync(e.SourceGuid);
                        Tag = t;
                        t.EndSync();
                    }
                }
            });
        }

        private void MpDbObject_SyncAdd(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            Dispatcher.UIThread.Post(
                async () => {
                    if (sender is MpCopyItemTag cit) {
                        if (TagId == cit.TagId) {
                            cit.StartSync(e.SourceGuid);
                            var dupCheck = await MpDataModelProvider.GetCopyItemTagForTagAsync(cit.TagId, cit.CopyItemId);
                            if (dupCheck != null) {
                                MpConsole.WriteTraceLine(@"Warning, copyItemTag was duplicate: " + cit.ToString());
                            }
                            await cit.WriteToDatabaseAsync();
                            cit.EndSync();
                        }
                    }
                });
        }

        #endregion

        #endregion

        #region Commands

        public ICommand SelectTagCommand => new MpCommand(
            () => {
                Parent.SelectTagCommand.Execute(this);
            }, () => {
                return IsTagNameReadOnly;
            });

        public ICommand AssignHotkeyCommand => new MpCommand(
            async () => {
                await MpAvShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                            $"Select '{TagName}' Collection",
                            Parent.SelectTagCommand,
                            ShortcutType,
                            TagId.ToString(),
                            ShortcutKeyString);
                OnPropertyChanged(nameof(ShortcutKeyString));
            }, () => CanHotkey);

        public ICommand ChangeColorCommand => new MpCommand<object>(
            (args) => {
                TagHexColor = args.ToString();

            });

        public ICommand CancelRenameTagCommand => new MpCommand(
            () => {
                TagName = _originalTagName;
                IsTagNameReadOnly = true;
            });

        public ICommand FinishRenameTagCommand => new MpAsyncCommand(
            async () => {
                IsTagNameReadOnly = true;
                await Tag.WriteToDatabaseAsync();
            });

        public ICommand RenameTagCommand => new MpCommand(
             () => {
                 _originalTagName = TagName;
                 IsTagNameReadOnly = false;
             },
            () => {
                return !IsTagReadOnly;
            });

        public ICommand AddNewChildTagCommand => new MpAsyncCommand<object>(
             async (args) => {
                 if (!IsExpanded) {
                     IsExpanded = true;
                 }
                 bool isNew = false;
                 MpTag t = null;
                 MpTagType childTagType = TagType;

                 if (args is Control control && AddChildPopupMenuItemViewModel != null) {
                     // show popup menu calling this command w/ tag type as parameter 
                     MpAvMenuExtension.ShowMenu(control, AddChildPopupMenuItemViewModel, null, PlacementMode.Right);
                     return;
                 } else if (args is MpTagType) {
                     // coming from plus button or plus popup menu
                     childTagType = (MpTagType)args;
                     isNew = true;
                 } else if (args is int pendingTagId) {
                     // coming from save SavePendingQueryCommand
                     if (pendingTagId <= 0) {
                         // should be already saved
                         Debugger.Break();
                     } else {
                         // added when pending query is confirmed
                         t = await MpDataModelProvider.GetItemAsync<MpTag>(pendingTagId);
                         isNew = true;
                     }
                 } else if (TagType == MpTagType.Group) {
                     // need to make sure type is passed cause child type isn't clear
                     Debugger.Break();
                 }

                 if (t == null) {
                     // only occurs from plus button
                     bool? sortDir = childTagType == MpTagType.Query ?
                        MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending : null;
                     MpContentSortType? sortType = childTagType == MpTagType.Query ?
                        MpAvClipTileSortFieldViewModel.Instance.SelectedSortType : null;

                     t = await MpTag.CreateAsync(
                             parentTagId: TagId,
                             treeSortIdx: Items.Count,
                             tagType: childTagType,
                             isSortDescending: sortDir,
                             sortType: sortType);

                 } else if (t.ParentTagId != TagId) {
                     // NOTE only update sort if not already child
                     // (allowing redundant add for simplicity)
                     t.TreeSortIdx = Items.Count;
                     t.ParentTagId = TagId;
                     await t.WriteToDatabaseAsync();
                 }
                 MpAvTagTileViewModel ttvm = Parent.Items.FirstOrDefault(x => x.TagId == t.Id);
                 if (ttvm == null) {
                     ttvm = await CreateChildTagTileViewModel(t);
                 } else {
                     ttvm.ParentTreeItem.Items.Remove(ttvm);
                 }

                 Items.Add(ttvm);

                 OnPropertyChanged(nameof(SortedItems));
                 Parent.OnPropertyChanged(nameof(Parent.Items));
                 if (isNew) {
                     await Task.Delay(300);
                     ttvm.RenameTagCommand.Execute(null);
                 }

             }, (args) => CanAddChild);

        public ICommand DeleteChildTagCommand => new MpAsyncCommand<object>(
            async (args) => {
                var child_ttvm_to_remove = args as MpAvTagTileViewModel;
                var deleteTasks =
                    child_ttvm_to_remove.SelfAndAllDescendants
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.Tag.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                Items.Remove(child_ttvm_to_remove);

                UpdateTreeSortOrder();
                OnPropertyChanged(nameof(SortedItems));
                Parent.OnPropertyChanged(nameof(Parent.PinnedItems));

                Parent.SelectTagCommand.Execute(this);
            });

        public ICommand DeleteThisTagCommand => new MpCommand(
            () => {
                ParentTreeItem.DeleteChildTagCommand.Execute(this);
            }, () => !IsTagReadOnly);


        public ICommand LinkCopyItemCommand => new MpAsyncCommand<object>(
            async (ciidArg) => {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    await LinkOrUnlinkCopyItemAsync((int)ciidArg, true);
                });
            }, (ciidArg) => {
                if (IsAllTag) {
                    return false;
                }
                if (!CanLinkContent) {
                    return false;
                }
                if (ciidArg is not int) {
                    return false;
                }
                int ciid = (int)ciidArg;
                if (ciid == 0) {
                    MpConsole.WriteTraceLine("Cannot link CopyItemId 0 to Tag: " + TagName + " Id: " + TagId);
                    return false;
                }
                return true;
            });

        public ICommand UnlinkCopyItemCommand => new MpAsyncCommand<object>(
            async (ciidArg) => {
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    await LinkOrUnlinkCopyItemAsync((int)ciidArg, false);
                });
            }, (ciidArg) => {
                if (IsAllTag) {
                    return false;
                }
                if (ciidArg is not int) {
                    return false;
                }
                int ciid = (int)ciidArg;
                if (ciid == 0) {
                    MpConsole.WriteTraceLine("Cannot unlink CopyItemId 0 to Tag: " + TagName + " Id: " + TagId);
                    return false;
                }
                return true;
            });


        public ICommand ToggleLinkToSelectedClipTileCommand => new MpCommand(
            () => {
                if (MpAvClipTrayViewModel.Instance.SelectedItem == null) {
                    return;
                }
                int ciid = MpAvClipTrayViewModel.Instance.SelectedItem.CopyItemId;

                if (IsLinkedToSelectedClipTile.IsTrue()) {
                    UnlinkCopyItemCommand.Execute(ciid);
                } else if (IsLinkedToSelectedClipTile.IsFalseOrNull()) {
                    LinkCopyItemCommand.Execute(ciid);
                }
            }, () => {
                return !IsAllTag;
            });

        #endregion
    }
}
