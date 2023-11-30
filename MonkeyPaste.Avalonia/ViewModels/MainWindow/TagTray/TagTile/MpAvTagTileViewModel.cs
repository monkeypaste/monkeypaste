using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTileViewModel :
        MpAvTreeSelectorViewModelBase<MpAvTagTrayViewModel, MpAvTagTileViewModel>,
        MpAvIMenuItemViewModel,
        MpIBadgeCountViewModel,
        MpIDraggable,
        MpIHoverableViewModel,
        MpIConditionalSelectableViewModel,
        MpIShortcutCommandViewModel,
        MpIUserColorViewModel,
        MpIColorPalettePickerViewModel,
        MpIActionComponent,
        MpIContextMenuViewModel,
        MpIPopupMenuViewModel,
        MpIPopupMenuPicker,
        MpIProgressIndicatorViewModel {

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

        #region MpAvIMenuItemViewModel Implementation

        ICommand MpAvIMenuItemViewModel.Command => ToggleLinkToSelectedClipTileCommand;
        object MpAvIMenuItemViewModel.CommandParameter => null;
        string MpAvIMenuItemViewModel.Header => TagName;
        object MpAvIMenuItemViewModel.IconSourceObj => TagHexColor;
        string MpAvIMenuItemViewModel.InputGestureText { get; }
        bool MpAvIMenuItemViewModel.StaysOpenOnClick => true;
        bool MpAvIMenuItemViewModel.HasLeadingSeparator => false;
        bool MpAvIMenuItemViewModel.IsVisible => true;
        bool? MpAvIMenuItemViewModel.IsChecked =>
            IsLinkedToSelectedClipTile;
        bool MpAvIMenuItemViewModel.IsThreeState => Items.Count > 0;
        bool MpAvIMenuItemViewModel.IsSubMenuOpen { get; set; }
        MpMenuItemType MpAvIMenuItemViewModel.MenuItemType =>
            MpMenuItemType.CheckableWithIcon;
        IEnumerable<MpAvIMenuItemViewModel> MpAvIMenuItemViewModel.SubItems =>
            Items;

        #endregion

        #region MpIDraggableViewModel Implementation
        bool MpIDraggable.IsDragging {
            get => IsDragging || IsPinTagDragging;
            set => throw new NotImplementedException();
        }

        #endregion

        #region MpIBadgeCountViewModel Implementation

        int MpIBadgeCountViewModel.BadgeCount =>
            BadgeCount;

        #endregion

        #region MpAvTreeSelectorViewModelBase Implementation

        public override MpAvTagTileViewModel ParentTreeItem => Parent.Items.FirstOrDefault(x => x.TagId == ParentTagId);
        public override IEnumerable<MpAvTagTileViewModel> Children => SortedItems;

        #endregion

        #region MpIPopupMenuPicker Implementation

        public MpAvMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedTagIds, bool recursive) {
            return new MpAvMenuItemViewModel() {
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
                if (value && CanSelect) {
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

        #region MpIColorPalettePickerViewModel Implementation

        public MpIAsyncCommand<object> PaletteColorPickedCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not string paletteItemData) {
                    return;
                }
                string new_color = string.Empty;
                if (paletteItemData == "custom") {
                    new_color = await Mp.Services.CustomColorChooserMenuAsync.ShowCustomColorMenuAsync(
                        selectedColor: TagHexColor);
                } else {
                    new_color = paletteItemData;
                }
                if (!new_color.IsStringHexColor()) {
                    return;
                }

                MpAvMenuView.CloseMenu();
                TagHexColor = new_color;
            });

        #endregion

        #region MpIProgressIndicatorViewModel Implementation
        public double PercentLoaded =>
            TotalAnalysisCount == 0 ? 1 : (double)CompletedAnalysisCount / (double)TotalAnalysisCount;
        #endregion

        #region MpIContextMenuItemViewModel Implementation

        //content menu item
        public MpAvMenuItemViewModel ContentMenuItemViewModel {
            get {
                return new MpAvMenuItemViewModel() {
                    Header = TagName,
                    Command = ToggleLinkToSelectedClipTileCommand,
                    IsChecked = IsLinkedToSelectedClipTile,
                    IconHexStr = TagHexColor,
                    IconShape = MenuIconShape,
                    ShortcutArgs = new object[] { MpShortcutType.SelectTag, this },
                    SubItems = Items.Select(x => x.ContentMenuItemViewModel).ToList()
                };
                //return GetTagMenu(ToggleLinkToSelectedClipTileCommand, IsLinkedToSelectedClipTile, new object[] { MpShortcutType.SelectTag, TagId });
            }
        }

        public MpAvMenuItemViewModel ContextMenuViewModel {
            get {
                return new MpAvMenuItemViewModel() {
                    SubItems = new List<MpAvMenuItemViewModel>() {
                        new MpAvMenuItemViewModel() {
                            IsVisible = !IsTagReadOnly,
                            Header = UiStrings.CommonRenameLabel,
                            AltNavIdx = 0,
                            IconResourceKey = Mp.Services.PlatformResource.GetResource("RenameImage") as string, //MpPlatformWrapper.Services.PlatformResource.GetResource("RenameIcon") as string,
                            Command = RenameTagCommand,
                        },
                        new MpAvMenuItemViewModel() {
                            IsVisible = CanHotkey,
                            Header = UiStrings.CommonAssignShortcutLabel,
                            AltNavIdx = 0,
                            IconResourceKey = Mp.Services.PlatformResource.GetResource("JoystickImage") as string,
                            Command = MpAvShortcutCollectionViewModel.Instance.ShowAssignShortcutDialogCommand,
                            CommandParameter = this,
                            ShortcutArgs = new object[] { MpShortcutType.SelectTag, this },
                        },
                        new MpAvMenuItemViewModel() {
                            IsVisible = CanPin,
                            Header = IsModelPinned ? UiStrings.CommonUnpinItemLabel : UiStrings.CommonPinItemLabel,
                            AltNavIdx = 0,
                            IconResourceKey = Mp.Services.PlatformResource.GetResource("PinImage") as string,
                            Command = Parent.ToggleTileIsPinnedCommand,
                            CommandParameter = this
                        },

                        new MpAvMenuItemViewModel() {
                            IsVisible = IsTrashTag,
                            Header = UiStrings.TagTilePermDeleteAllHeader,
                            IconResourceKey = Mp.Services.PlatformResource.GetResource("DeleteImage") as string,
                            Command = Parent.EmptyTrashCommand
                        },
                        new MpAvMenuItemViewModel() {
                            IsVisible = IsTrashTag,
                            HasLeadingSeparator = true,
                            Header = UiStrings.TagTileRestoreAllHeader,
                            IconResourceKey = Mp.Services.PlatformResource.GetResource("ResetImage") as string,
                            Command = Parent.RestoreAllTrashCommand
                        },
                        MpAvMenuItemViewModel.GetColorPalleteMenuItemViewModel(this,true),
                        new MpAvMenuItemViewModel() {
                            IsVisible = !IsTagReadOnly,
                            HasLeadingSeparator = true,
                            Header = UiStrings.CommonDeleteLabel,
                            IconResourceKey = Mp.Services.PlatformResource.GetResource("DeleteImage") as string,
                            Command = DeleteThisTagCommand
                        }
                    }
                };
            }
        }
        public bool IsContextMenuOpen { get; set; } = false;

        #endregion

        #region MpIPopUpMenuViewModel Implementation

        MpAvMenuItemViewModel MpIPopupMenuViewModel.PopupMenuViewModel => ContentMenuItemViewModel;
        bool MpIPopupMenuViewModel.IsPopupMenuOpen {
            get => IsContextMenuOpen;
            set => IsContextMenuOpen = value;
        }
        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpITriggerActionViewModel Implementation

        void MpIActionComponent.RegisterActionComponent(MpIInvokableAction mvm) {
            if (OnCopyItemLinked.HasInvoker(mvm)) {
                return;
            }
            OnCopyItemLinked += mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnCopyItemLinked)} Registered {mvm.Label}");
        }

        void MpIActionComponent.UnregisterActionComponent(MpIInvokableAction mvm) {
            if (!OnCopyItemLinked.HasInvoker(mvm)) {
                return;
            }
            OnCopyItemLinked -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnCopyItemLinked)} Unregistered {mvm.Label}");
        }

        #endregion

        #region MpIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType =>
            MpShortcutType.SelectTag;
        public string KeyString =>
            MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutKeyString(this);

        public object ShortcutCommandParameter =>
            TagId;
        ICommand MpIShortcutCommandViewModel.ShortcutCommand =>
            Parent == null ? null : Parent.SelectTagCommand;

        #endregion

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvTagTileViewModel> SortedItems =>
            Items.OrderBy(x => x.TreeSortIdx)
            .ToList();

        public MpAvMenuItemViewModel AddChildPopupMenuItemViewModel {
            get {
                if (!IsGroupTag ||
                    IsCollectionsTag) {
                    return null;
                }
                return new MpAvMenuItemViewModel() {
                    SubItems = new List<MpAvMenuItemViewModel>() {
                        new MpAvMenuItemViewModel() {
                            IconSourceObj = "FolderImage",
                            Header = UiStrings.TagGroupTypeName,
                            Command = AddNewChildTagCommand,
                            CommandParameter = MpTagType.Group
                        },
                        new MpAvMenuItemViewModel() {
                            IconSourceObj = "BinocularsTiltedImage",
                            Header = UiStrings.TagQueryTypeName,
                            Command = AddNewChildTagCommand,
                            CommandParameter = MpTagType.Query
                        },
                    }
                };
            }
        }
        #endregion

        #region State

        public bool IsActiveTag {
            get {
                if (IsGroupTag || Parent == null) {
                    return false;
                }
                return Parent.LastSelectedActiveItem == this;
            }
        }

        public bool IsAnyBusy =>
            IsBusy || Children.Any(x => x.IsAnyBusy);

        public ObservableCollection<int> CopyItemIdsNeedingView { get; set; } = new ObservableCollection<int>();
        //public int BadgeCount =>
        //    CopyItemIdsNeedingView.Count;
        public int BadgeCount =>
            SelfAndAllDescendants
            .Cast<MpAvTagTileViewModel>()
            .SelectMany(x => x.CopyItemIdsNeedingView)
            .Distinct()
            .Count();

        public bool CanAddChild =>
            !IsQueryTag &&
            !IsFormatGroupTag &&
            !IsFavoritesTag &&
            !IsAllTag &&
            !IsTrashTag;

        public bool CanSelect =>
            // from marquee select, reje
            IsTagNameReadOnly;// && IsSelected;

        public bool CanPin =>
            !IsGroupTag && !IsRootLevelTag;

        public bool CanTreeMove =>
            !IsRootLevelTag;

        public bool CanHotkey =>
            !IsGroupTag &&
            !IsTrashTag &&
            Mp.Services.PlatformInfo.IsDesktop;

        public bool CanLinkContent =>
            IsLinkTag;

        public bool IsNew =>
            Tag == null || Tag.Id <= 0;

        public bool IsLeaf =>
            Items.Count == 0;

        public bool IsTail =>
            ParentTreeItem != null &&
            TreeSortIdx == ParentTreeItem.Items.Count - 1;

        public bool IsTagReadOnly =>
            TagId <= MpTag.MAX_READ_ONLY_TAG_ID;

        public bool IsContentDragOverTag { get; set; }
        public bool IsContentDragOverTagValid { get; set; }
        public bool IsDragging { get; set; }
        public bool IsTagDragOverTop { get; set; }
        public bool IsTagDragOverBottom { get; set; }
        public bool IsTagDragValid { get; set; }
        public bool IsTagDragOverCopy { get; set; }
        public bool IsTagDragLeafChildDrop { get; set; }
        public bool IsPinTagDragging { get; set; }

        public MpTagType[] ValidChildDropTagTypes {
            get {
                if (IsAllTag ||
                    IsTrashTag ||
                    IsQueryTag ||
                    IsFormatGroupTag ||
                    IsFavoritesTag ||
                    IsRecentTag) {
                    return new[] { MpTagType.None };
                }

                if (IsCollectionsTag) {
                    return new[] { MpTagType.Link };
                }
                if (IsGroupTag) {
                    return new[] { MpTagType.Group, MpTagType.Query };
                }
                if (IsLinkTag) {
                    return new[] { MpTagType.Link };
                }
                MpDebug.Break($"Unhandled valid child drop tag ");
                return new[] { MpTagType.None };
            }
        }
        public MpTagType[] ValidSiblingDropTagTypes {
            get {
                if (IsRootLevelTag) {
                    return new[] { MpTagType.None };
                }
                if (IsTextFormatTag ||
                    IsImageFormatTag ||
                    IsFileFormatTag) {
                    return new[] { MpTagType.None };
                }
                if (IsLinkTag) {
                    return new[] { MpTagType.Link };
                }
                if (IsQueryTag || IsGroupTag) {
                    return new[] { MpTagType.Query, MpTagType.Group };
                }
                MpDebug.Break($"Unhandled valid sibling drop tag ");
                return new[] { MpTagType.None };
            }
        }
        public MpTagType[] ValidParentDragTagTypes {
            get {
                if (IsRootLevelTag) {
                    return new[] { MpTagType.None };
                }
                if (IsTextFormatTag ||
                    IsImageFormatTag ||
                    IsFileFormatTag) {
                    return new[] { MpTagType.None };
                }
                if (IsAllTag ||
                    IsFormatGroupTag) {
                    return new[] { MpTagType.Group };
                }
                if (IsLinkTag) {
                    return new[] { MpTagType.Link, MpTagType.Group };
                }
                if (IsQueryTag || IsGroupTag) {
                    return new[] { MpTagType.Group };
                }
                MpDebug.Break($"Unhandled valid sibling drop tag ");
                return new[] { MpTagType.None };
            }
        }


        public bool IsCollectionsTag =>
            TagId == MpTag.CollectionsTagId;
        public bool IsAllTag =>
            TagId == MpTag.AllTagId;
        public bool IsFavoritesTag =>
            TagId == MpTag.FavoritesTagId;
        public bool IsTrashTag =>
            TagId == MpTag.TrashTagId;

        public bool IsFiltersTag =>
            TagId == MpTag.FiltersTagId;
        public bool IsFormatGroupTag =>
            TagId == MpTag.FormatsTagId;
        public bool IsTextFormatTag =>
            TagId == MpTag.TextFormatTagId;
        public bool IsImageFormatTag =>
            TagId == MpTag.ImageFormatTagId;
        public bool IsFileFormatTag =>
            TagId == MpTag.FileFormatTagId;
        public bool IsRecentTag =>
            TagId == MpTag.TodayTagId;

        public bool IsLinkTag =>
            !IsQueryTag && !IsGroupTag;
        public bool IsQueryTag =>
            TagType == MpTagType.Query;

        public bool IsRootLevelTag =>
            ParentTagId == 0;
        public bool IsGroupTag =>
            TagType == MpTagType.Group;
        public bool IsNotGroupTag =>
            IsLinkTag || IsQueryTag;

        public bool IsTagNameReadOnly { get; set; } = true;
        public bool IsTagNameTextBoxFocused { get; set; } = false;
        public bool? IsLinkedToSelectedClipTile { get; set; } = false;

        private int? _tagClipCount;
        public int? TagClipCount {
            get {
                if (!IsTrashTag || IsSelected || IsHovering) {
                    // only show trash count when selected or hovering
                    return _tagClipCount;
                }
                return null;
            }
            set {
                if (_tagClipCount != value) {
                    _tagClipCount = value;
                    OnPropertyChanged(nameof(TagClipCount));
                }
            }
        }

        public MpShape MenuIconShape =>
            IsLinkTag ? null :
            IsQueryTag ? QUERY_SHAPE : IsGroupTag ? GROUP_SHAPE : null;

        public int CompletedAnalysisCount { get; set; }
        public int TotalAnalysisCount { get; set; }

        #endregion

        #region Layout

        public double TagNameWidth { get; set; }
        //public double TagNameWidth =>
        //    TagName.Length * 16d;

        public double TagNameLength =>
            TagName.Length;
        #endregion

        #region Appearance

        public string TagTextHexColor {
            get {
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return MpSystemColors.White;
                }
                if (!IsSelected &&
                    IsLinkedToSelectedClipTile.IsTrueOrNull() &&
                    !IsAllTag &&
                    !IsCollectionsTag) {
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
                if (IsTagReadOnly) {
                    return ((MpReadOnlyTagType)TagId).EnumToUiString();
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
            OnPropertyChanged(nameof(IsGroupTag));
            OnPropertyChanged(nameof(IsQueryTag));

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

        public bool IsCopyItemLinked(int ciid) {
            // NOTE only used for content drop dragOver to keep dragOver synchronous
            if (ciid == 0 || Tag == null || Tag.Id == 0) {
                return false;
            }
            if (IsAllTag) {
                return true;
            }
            bool isLinked = MpDataModelProvider.IsTagLinkedWithCopyItem(Tag.Id, ciid);
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
        public async Task UpdateClipCountAsync() {
            if (IsGroupTag) {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(async () => {
                if (IsAllTag) {
                    TagClipCount = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(MpTag.AllTagId);
                } else {
                    if (IsLinkTag) {
                        // NOTE omit trashed ids on all tags EXCEPT trashed which has empty omits
                        TagClipCount = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(
                            tid: TagId,
                            ignore_descendants: false);

                    } else {
                        // query tag
                        if (IsActiveTag) {
                            if (!MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive) {
                                // keeping this here for debugging,
                                // when this eval's update is called again when active though
                                //MpDebug.Break();
                                TagClipCount = null;
                            } else {
                                TagClipCount =
                                    Mp.Services.Query.TotalAvailableItemsInQuery;
                                //MpAvSearchCriteriaItemCollectionViewModel.Instance.TotalAvailableItemsInQuery;
                            }
                        } else {
                            TagClipCount = null;
                        }
                    }
                }
            });
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
            if (e is MpShortcut sc && sc.IsShortcutCommand(this)) {
                OnPropertyChanged(nameof(KeyString));
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc && sc.IsShortcutCommand(this)) {
                OnPropertyChanged(nameof(KeyString));
            } else if (e is MpTag t && t.Id == TagId) {
                Dispatcher.UIThread.Post(async () => {
                    // stupid property changed null ref issue so trying to wait here?
                    await Task.Delay(50);
                    OnPropertyChanged(nameof(TagName));
                    OnPropertyChanged(nameof(TagHexColor));
                    OnPropertyChanged(nameof(TagTextHexColor));
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc && sc.IsShortcutCommand(this)) {
                OnPropertyChanged(nameof(KeyString));
            } else if (e is MpCopyItemTag cit && cit.TagId == TagId) {
                UnlinkCopyItemCommand.Execute(cit.CopyItemId);
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

                        if (IsQueryTag) {
                            MpAvClipTileSortDirectionViewModel.Instance.IsSortDescending = IsSortDescending;
                            MpAvClipTileSortFieldViewModel.Instance.SelectedSortType = SortType;
                        }
                        if (IsGroupTag) {
                            IsExpanded = true;
                        }
                        Parent.SelectTagCommand.Execute(this);
                    } else if (!IsTagNameTextBoxFocused) {
                        IsTagNameReadOnly = true;
                        OnPropertyChanged(nameof(IsActiveTag));
                    }
                    break;
                case nameof(IsActiveTag):
                    if (IsActiveTag) {
                        break;
                    }
                    if (!IsGroupTag) {
                        UpdateClipCountAsync().FireAndForgetSafeAsync(this);
                    }
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.LastSelectedActiveItem));
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
                        break;
                    }
                    if (HasModelChanged) {
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
                        await Task.WhenAll(MpAvClipTrayViewModel.Instance.Items.Select(x => x.InitTitleLayersAsync()));
                    });
                    break;
                //case nameof(TagTileTrayWidth):
                case nameof(ObservedTagTrayBounds):
                    if (Parent == null) {
                        return;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsNavButtonsVisible));
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
                    if (this is MpAvIMenuItemViewModel imivm) {
                        imivm.OnPropertyChanged(nameof(imivm.IsChecked));
                    }
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
                case nameof(TagClipCount):
                    if (ParentTreeItem == null) {
                        break;
                    }
                    ParentTreeItem.UpdateClipCountAsync().FireAndForgetSafeAsync(this);
                    break;
                case nameof(IsDragging):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyDragging));
                    break;
                case nameof(IsPinTagDragging):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyDragging));
                    break;
                case nameof(CompletedAnalysisCount):
                case nameof(TotalAnalysisCount):
                    if (TotalAnalysisCount > 0) {
                        MpConsole.WriteLine($"Current percent set to: {PercentLoaded}");
                    }
                    Dispatcher.UIThread.Post(() => { OnPropertyChanged(nameof(PercentLoaded)); });
                    break;
                case nameof(TagName):
                    OnPropertyChanged(nameof(TagNameWidth));
                    OnPropertyChanged(nameof(TagNameLength));
                    break;
                case nameof(BadgeCount):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.BadgeCount));
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
            OnPropertyChanged(nameof(CopyItemIdsNeedingView));
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
                case MpMessageType.InPlaceRequeryCompleted:
                case MpMessageType.RequeryCompleted:
                    UpdateBadge();
                    if (IsActiveTag) {
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

            bool is_cap_related = IsTrashTag || IsFavoritesTag;
            bool report_link = false;
            if (isLink) {
                // try to create link, if it was created (and didn't already exist) notify any triggers
                int linkCount = await MpDataModelProvider.GetCopyItemCountByTagIdAsync(TagId);
                var cit = await MpCopyItemTag.CreateAsync(
                    tagId: TagId,
                    copyItemId: ciid,
                    sortIdx: linkCount);

                if (!cit.WasDupOnCreate) {
                    CopyItemIdsNeedingView.Add(ciid);
                    report_link = true;
                }
                if (IsTrashTag && !Parent.TrashedCopyItemIds.Contains(ciid)) {
                    Parent.TrashedCopyItemIds.Add(ciid);
                    MpAvClipTrayViewModel.Instance.AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsTrashed)));
                }

            } else {
                var cit = await MpDataModelProvider.GetCopyItemTagForTagAsync(ciid, TagId);

                if (cit != null) {
                    // only delete link/notify if exists
                    report_link = true;
                    await cit.DeleteFromDatabaseAsync();
                }
                CopyItemIdsNeedingView.Remove(ciid);

                if (IsTrashTag && Parent.TrashedCopyItemIds.Contains(ciid)) {
                    Parent.TrashedCopyItemIds.Remove(ciid);
                    MpAvClipTrayViewModel.Instance.AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsTrashed)));
                }
            }

            if (report_link) {

                MpCopyItem ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(ciid);
                if (ci == null) {
                    // NOTE item will no longer exist if unlink is part of copyitem.delete
                    MpDebug.Assert(!isLink, $"Error adding link to unknown item ciid {ciid}. Can only be null if unlinking");
                    if (isLink) {
                        // this seems to happen on ci builder error, delete link since item doesn't exist
                        var cit = await MpDataModelProvider.GetCopyItemTagForTagAsync(ciid, TagId);
                        if (cit != null) {
                            await MpDataModelProvider.DeleteItemAsync<MpCopyItemTag>(cit.Id);
                        }

                    }
                    return;
                }
                Mp.Services.TransactionBuilder.ReportTransactionAsync(
                    copyItemId: ciid,
                    reqType: MpJsonMessageFormatType.Link,
                    req: ciid.ToString(),
                    respType: MpJsonMessageFormatType.Link,
                    resp: TagId.ToString(),
                    ref_uris: new[] { Mp.Services.SourceRefTools.ConvertToInternalUrl(MpTransactionSourceType.CopyItem, ciid) },
                    transType: isLink ? MpTransactionType.Linked : MpTransactionType.Unlinked)
                    .FireAndForgetSafeAsync(this);

                if (is_cap_related) {
                    ci.LastCapRelatedDateTime = DateTime.Now;
                    await ci.WriteToDatabaseAsync();
                }

            }

            Dispatcher.UIThread.VerifyAccess();

            if (is_cap_related) {
                await Parent.UpdateAllClipCountsAsync();
            } else {
                foreach (MpAvTagTileViewModel this_or_ancestor_ttvm in SelfAndAllAncestors) {
                    await this_or_ancestor_ttvm.UpdateClipCountAsync();
                }
            }

            // await notify so IsBusy doesn't trip
            await NotifyTriggersAsync(ciid, isLink);

            if (MpAvClipTrayViewModel.Instance.PersistantSelectedItemId == ciid) {
                // trigger selection changed message to notify tag association change
                MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
            }
            IsBusy = false;

            Dispatcher.UIThread.Post(async () => {
                // always do in place requery to catch the many cases for placeholder changes
                await MpAvClipTrayViewModel.Instance.UpdateEmptyPropertiesAsync();
                if (is_cap_related) {
                    // NOTE infer link/unlink by +/- id
                    await MpAvClipTrayViewModel.Instance.ProcessAccountCapsAsync(MpAccountCapCheckType.Link, TagId * (isLink ? 1 : -1));
                }
                while (!MpAvClipTrayViewModel.Instance.QueryCommand.CanExecute(string.Empty)) { await Task.Delay(100); }
                MpAvClipTrayViewModel.Instance.QueryCommand.Execute(string.Empty);

            });

        }

        private void UpdateBadge() {
            Dispatcher.UIThread.VerifyAccess();

            var idsSeen = new List<int>();
            foreach (int ciid in CopyItemIdsNeedingView) {
                var civm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                if (civm != null) {
                    //if (civm.IsPinned) {
                    //    // only mark item as seen if viewed in its query tray
                    //    continue;
                    //}
                    if (civm.IsAnyQueryCornerVisible && IsSelected) {
                        idsSeen.Add(ciid);
                    }
                }

            }
            foreach (var idToRemove in idsSeen) {
                if (!CopyItemIdsNeedingView.Remove(idToRemove)) {
                    // how?
                    MpDebug.Break();
                }
            }
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

        public ICommand SelectThisTagCommand => new MpCommand<object>(
            (args) => {
                if (IsGroupTag &&
                    args is Control c &&
                    c.GetVisualAncestor<MpAvTagView>() is MpAvTagView tv &&
                    tv.IsPinTrayTagView()) {
                    ShowPinnedGroupChildSelectorContextMenuCommand.Execute(args);
                    return;
                }
                Parent.SelectTagCommand.Execute(TagId);
            }, (args) => {
                return IsTagNameReadOnly;
            });

        public ICommand BringIntoTreeViewCommand => new MpCommand(
            () => {
                Dispatcher.UIThread.Post(async () => {
                    TreeViewItem ttvi = null;
                    while (ttvi == null) {
                        if (App.MainView is Control mv &&
                            mv.GetVisualDescendant<MpAvTagTreeView>() is MpAvTagTreeView ttrv &&
                            ttrv.GetVisualDescendants<TreeViewItem>() is IEnumerable<TreeViewItem> tvil &&
                            tvil.FirstOrDefault(x => x.DataContext == this) is TreeViewItem tvi) {
                            ttvi = tvi;
                        } else {
                            await Task.Delay(100);
                        }
                    }
                    ttvi.BringIntoView();
                });
            }, () => {
                // only bring into view when sidebar is tag tree
                return MpAvTagTrayViewModel.Instance.IsSelected;
            });

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

        public ICommand RenameTagCommand => new MpCommand<object>(
             (args) => {
                 _originalTagName = TagName;
                 IsTagNameReadOnly = false;
             },
            (args) => {
                if (args != null &&
                    !Mp.Services.PlatformInfo.IsDesktop) {
                    // disable double click cmd on mobile to allow hold event to take priority
                    // in pointerPress extension
                    return false;
                }
                return !IsTagReadOnly;
            });

        public MpIAsyncCommand<object> AddNewChildTagCommand => new MpAsyncCommand<object>(
             async (args) => {
                 if (!IsExpanded) {
                     IsExpanded = true;
                 }
                 bool isNew = false;
                 MpTag t = null;
                 MpTagType childTagType = TagType;

                 if (args is Control control && AddChildPopupMenuItemViewModel != null) {
                     // show popup menu calling this command w/ tag type as parameter 

                     MpAvMenuView.ShowMenu(
                         target: control,
                         dc: AddChildPopupMenuItemViewModel);
                     return;
                 } else if (args is MpTagType) {
                     // coming from plus button or plus popup menu
                     childTagType = (MpTagType)args;
                     isNew = true;
                 } else if (args is int pendingTagId) {
                     // coming from save SavePendingQueryCommand
                     if (pendingTagId <= 0) {
                         // should be already saved
                         MpDebug.Break();
                     } else {
                         // added when pending query is confirmed
                         t = await MpDataModelProvider.GetItemAsync<MpTag>(pendingTagId);
                         isNew = true;
                     }
                 } else if (IsCollectionsTag) {
                     // special case for collection tag from add btn view cmd..
                     childTagType = MpTagType.Link;
                 } else if (TagType == MpTagType.Group) {
                     // need to make sure type is passed cause child type isn't clear
                     MpDebug.Break();
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
                bool was_child_active =
                    Parent.LastSelectedActiveItem != null &&
                    Parent.LastSelectedActiveItem.TagId == child_ttvm_to_remove.TagId;
                if (child_ttvm_to_remove.IsModelPinned) {
                    // NOTE I think this is a listbox collection problem but
                    // deleting pinned tile doesn't remove pinned lbi so unpinning first
                    // and letting it save before deleting
                    Parent.ToggleTileIsPinnedCommand.Execute(child_ttvm_to_remove);
                    await Task.Delay(20);
                    while (child_ttvm_to_remove.IsBusy) { await Task.Delay(100); }
                }
                var deleteTasks =
                    child_ttvm_to_remove.SelfAndAllDescendants
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.Tag.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                Items.Remove(child_ttvm_to_remove);
                Parent.Items.Remove(child_ttvm_to_remove);
                SortedItems.ToList().ForEach((x, idx) => x.TreeSortIdx = idx);
                OnPropertyChanged(nameof(SortedItems));
                Parent.OnPropertyChanged(nameof(Parent.PinnedItems));
                Parent.OnPropertyChanged(nameof(Parent.LastSelectedActiveItem));

                var ttvm_to_select = this;
                if (IsGroupTag && was_child_active) {
                    // NOTE selecting group tag won't update query
                    // and configuring state to no tag could be problematic 
                    // so just auto-selecting 'All' for this case
                    ttvm_to_select = Parent.Items.FirstOrDefault(x => x.TagId == MpTag.AllTagId);

                }
                Parent.SelectTagCommand.Execute(ttvm_to_select);

            });

        public ICommand DeleteThisTagCommand => new MpCommand(
            () => {
                ParentTreeItem.DeleteChildTagCommand.Execute(this);
            }, () => !IsTagReadOnly);


        public MpIAsyncCommand<object> LinkCopyItemCommand => new MpAsyncCommand<object>(
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

        public MpIAsyncCommand<object> UnlinkCopyItemCommand => new MpAsyncCommand<object>(
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

        public ICommand ShowContextMenuCommand => new MpCommand<object>(
            (args) => {
                var control = args as Control;
                if (control == null) {
                    return;
                }
                MpAvMenuView.ShowMenu(control, ContextMenuViewModel);
            },
            (args) => {
                return IsTagNameReadOnly;
            });

        public ICommand ShowPinnedGroupChildSelectorContextMenuCommand => new MpCommand<object>(
            (args) => {

                //if (IsContextMenuOpen) {
                //    // toggle menu closed
                //    MpAvMenuView.CloseMenu();
                //    //IsContextMenuOpen = false;
                //    Parent.IsSelecting = false;
                //    return;
                //}

                //Parent.IsSelecting = true;
                var child_mivm = new MpAvMenuItemViewModel() {
                    ParentObj = this,
                    SubItems = Items.Select(x => x.GetMenu(x.SelectThisTagCommand, null, new[] { Parent.SelectedItemId }, true)).ToList()
                };

                var control = args as Control;
                if (control == null) {
                    return;
                }

                MpAvMenuView.ShowMenu(
                    target: control,
                    dc: child_mivm);
            },
            (args) => {
                if (args is not Control ||
                    !IsGroupTag) {
                    return false;
                }
                return true;
            });

        public MpIAsyncCommand<object> MoveOrCopyThisTagCommand => new MpAsyncCommand<object>(
            async (args) => {
                Parent.IsBusy = true;

                var argParts = args as object[];

                int old_parent_tag_id = ParentTagId;
                int new_parent_tag_id = (int)argParts[0];
                int new_sort_idx = (int)argParts[1];
                bool is_pinning = (bool)argParts[2];
                bool is_copy = (bool)argParts[3];

                if (is_copy) {
                    // when duplicating clone model/vm w/o changing parent then call 
                    // this cmd w/ new vm
                    var temp_parent_vm = ParentTreeItem;
                    if (temp_parent_vm == null) {
                        // when cloning all or root group
                        // make new child of it not parent
                        temp_parent_vm = this;
                    }
                    var cloned_tag = await Tag.CloneDbModelAsync();
                    cloned_tag.ParentTagId = temp_parent_vm.TagId;
                    // append 'Copy' to title
                    cloned_tag.TagName = string.Format(UiStrings.CommonTitledCopyTitleText, cloned_tag.TagName);
                    // clear pin idx
                    cloned_tag.PinSortIdx = -1;
                    // reset clones treeIdx to tail 
                    cloned_tag.TreeSortIdx = temp_parent_vm.Items.Count;
                    await cloned_tag.WriteToDatabaseAsync();

                    var cloned_tag_vm = await temp_parent_vm.CreateChildTagTileViewModel(cloned_tag);
                    temp_parent_vm.Items.Add(cloned_tag_vm);

                    while (temp_parent_vm.IsAnyBusy) {
                        // make sure its added
                        await Task.Delay(100);
                    }

                    // move clone with provided args w/ false copy arg
                    argParts[3] = false;
                    await cloned_tag_vm.MoveOrCopyThisTagCommand.ExecuteAsync(argParts);
                    return;
                }

                if (is_pinning) {
                    var pinned_vml = Parent.PinnedItems.ToList();
                    if (pinned_vml.Any(x => x.TagId == TagId)) {
                        // already pinned just move it
                        pinned_vml.Move(PinSortIdx, new_sort_idx);
                    } else {
                        pinned_vml.Insert(new_sort_idx, this);
                    }
                    pinned_vml.ForEach((x, idx) => x.PinSortIdx = idx);
                    while (Parent.Items.Any(x => x.IsBusy)) {
                        // wait for pinned db stuff
                        await Task.Delay(100);
                    }
                    Parent.OnPropertyChanged(nameof(Parent.PinnedItems));
                    Parent.IsBusy = false;
                    return;
                }

                // remove from old parent (even if same)
                var old_parent_vm = Parent.Items.FirstOrDefault(x => x.TagId == ParentTagId);
                old_parent_vm.Items.Remove(this);
                old_parent_vm.SortedItems.ToList().ForEach((x, idx) => x.TreeSortIdx = idx);
                while (old_parent_vm.IsAnyBusy) {
                    // wait for old parent db stuff
                    await Task.Delay(100);
                }
                old_parent_vm.OnPropertyChanged(nameof(old_parent_vm.SortedItems));

                // add to new parent 
                var new_parent_vm = Parent.Items.FirstOrDefault(x => x.TagId == new_parent_tag_id);
                ParentTagId = new_parent_tag_id;
                if (new_sort_idx < 0) {
                    // negative sort implies end of list
                    new_sort_idx = new_parent_vm.Items.Count;
                }
                TreeSortIdx = new_sort_idx;
                new_parent_vm.Items.Add(this);

                // update sort order using tree sort idx as primary sort then by if its this item
                new_parent_vm.Items
                    .OrderBy(x => x.TreeSortIdx)
                    .ThenBy(x => x.TagId != TagId)
                    .ToList()
                    .ForEach((x, idx) => x.TreeSortIdx = idx);

                while (new_parent_vm.IsAnyBusy) {
                    // wait for new parent db stuff
                    await Task.Delay(100);
                }
                new_parent_vm.OnPropertyChanged(nameof(new_parent_vm.SortedItems));
                // update old/new ancestor counts
                List<MpAvTagTileViewModel> count_update_vml = new List<MpAvTagTileViewModel>();

                count_update_vml.AddRange(
                        old_parent_vm
                        .SelfAndAllAncestors
                        .Cast<MpAvTagTileViewModel>());

                count_update_vml.AddRange(
                        new_parent_vm
                        .SelfAndAllAncestors
                        .Cast<MpAvTagTileViewModel>());

                await Task.WhenAll(
                    count_update_vml
                        .Distinct()
                        .ToList()
                        .Select(x => x.UpdateClipCountAsync()));

                Parent.IsBusy = false;
            },
            (args) => {
                return args is object[];
            });


        #endregion
    }
}
