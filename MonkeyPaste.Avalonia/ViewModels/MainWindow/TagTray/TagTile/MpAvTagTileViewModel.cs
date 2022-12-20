using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MonkeyPaste;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using System.Diagnostics;
using Avalonia.Threading;
using Avalonia.Controls;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTileViewModel : 
        MpAvTreeSelectorViewModelBase<MpAvTagTrayViewModel, MpAvTagTileViewModel>, 
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpAvIShortcutCommand, 
        MpIUserColorViewModel,
        MpIActionComponent,
        MpIContextMenuViewModel {

        #region Private Variables
        private object _notifierLock = new object();

        private string _originalTagName = string.Empty;
        private bool _wasEditingName = false;
        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvTagTileViewModel> SortedItems => Items.OrderBy(x => x.TagSortIdx);

        #endregion

        #region MpAvTreeSelectorViewModelBase Implementation

        public override MpAvTagTileViewModel ParentTreeItem => Parent.Items.FirstOrDefault(x => x.TagId == ParentTagId);
        public override IEnumerable<MpAvTagTileViewModel> Children => SortedItems;

        #endregion

        #region MpISelectableViewModel Implementation

        private bool _isSelected = false;
        public bool IsSelected {
            get => _isSelected;
            set {
                if(IsSelected != value) {
                    if(!IsSelected && !IsTagNameReadOnly) {
                        return;
                    }
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
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
                //int totalCount = 1;// MpAvClipTrayViewModel.Instance.SelectedModels.Count;
                //int linkCount = IsCopyItemLinked(MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem.Id) ? 1 : 0;//MpAvClipTrayViewModel.Instance.SelectedModels.Where(x => IsLinked(x)).Count();
                return new MpMenuItemViewModel() {
                    Header = TagName,
                    Command = ToggleLinkToSelectedClipTileCommand,
                    //IsChecked = totalCount == linkCount && totalCount > 0,
                    //IsPartiallySelected = linkCount != totalCount && totalCount > 0,
                    IsChecked = IsLinkedToSelectedClipTile,
                    IconHexStr = TagHexColor,
                    //ShortcutObjId = TagId,
                    //ShortcutType = MpShortcutType.SelectTag,
                    ShortcutArgs = new object[] { MpShortcutType.SelectTag, TagId },
                    SubItems = Items.Select(x => x.ContentMenuItemViewModel).ToList()
                };
            }
        }

        public MpMenuItemViewModel ContextMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = "Rename",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("RenameImage") as string, //MpPlatformWrapper.Services.PlatformResource.GetResource("RenameIcon") as string,
                            Command = RenameTagCommand,
                            IsVisible = RenameTagCommand.CanExecute(null)
                        },
                        new MpMenuItemViewModel() {
                            Header = "Assign Hotkey",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("HotkeyImage") as string,
                            Command = AssignHotkeyCommand,
                            //ShortcutObjId = TagId,
                            //ShortcutType = MpShortcutType.SelectTag
                            ShortcutArgs = new object[] { MpShortcutType.SelectTag, TagId },
                        },
                        new MpMenuItemViewModel() {
                            Header = IsModelPinned ? "Unpin" : "Pin",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PinImage") as string,
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
                            Header = "_Delete",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DeleteImage") as string,
                            Command = DeleteThisTagCommand,
                            IsVisible = !IsTagReadOnly
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

        #region MpAvIShortcutCommand Implementation

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

        #region State

        public IEnumerable<int> LinkedCopyItemIds { get; private set; } = new List<int>();
        public ObservableCollection<int> CopyItemIdsNeedingView { get; set; } = new ObservableCollection<int>();
        public int BadgeCount => CopyItemIdsNeedingView.Count;

        public bool CanAddChild {
            get {
                if(IsHelpTag) {
                    return false;
                }
                return true;
            }
        }
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
                if(Tag == null) {
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
        public bool IsTagNameReadOnly { get; set; } = true;
        public bool IsTagNameTextBoxFocused { get; set; } = false;
        public bool? IsLinkedToSelectedClipTile { get; set; }


        #endregion

        #region Appearance
        public double[] TagBorderDashArray {
            get {
                if (IsDragOverTag) {
                    return new double[] { 2,2 };
                }
                return null;
            }
        }
        public double TagBorderDashOffset {
            get {
                if(IsDragOverTag) {
                    return 2;
                }
                return 0;
            }
        }

        public string TagBorderBackgroundHexColor {
            get { 
                if (IsSelected) {
                    return MpSystemColors.dimgray;
                }
                if(IsHovering) {
                    return MpSystemColors.lightgray;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string TagBorderHexColor {
            get {
                if(IsDragOverTag) {
                    if(IsDragOverTagValid) {
                        return MpSystemColors.limegreen;
                    }
                    return MpSystemColors.red1;
                }

                if(IsContextMenuOpen) {
                    return MpSystemColors.red1;
                }
                
                return MpSystemColors.Transparent;
            }
        }

        public string TagTextHexColor {
            get {
                var mwvm = MpAvMainWindowViewModel.Instance;
                var ctrvm = MpAvClipTrayViewModel.Instance;
                var ttrvm = MpAvTagTrayViewModel.Instance;
                if(MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
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

        public string TagCountTextHexColor {
            get {
                return MpColorHelpers.IsBright(TagHexColor) ? MpSystemColors.black : MpSystemColors.White; ;
            }
        }

        public int TagClipCount { get; set; }

        public double TagHeight {
            get {
                //assumes Tag Margin is 5
                return 20;// MpMeasurements.Instance.FilterMenuDefaultHeight - (5 * 2);
            }
        }

        public double TagFontSize {
            get {
                return TagHeight * 0.5;
            }
        }

        //public double TagTileTrayWidth { get; set; }
        public MpRect ObservedTagTrayBounds { get; set; }

        #endregion

        #region Model

        public bool IsModelPinned {
            get {
                if(Tag == null) {
                    return false;
                }
                return Tag.IsPinned;
            }
            set {
                if(IsModelPinned != value) {
                    Tag.IsPinned = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsModelPinned));
                    Parent.OnPropertyChanged(nameof(Parent.PinnedItems));
                }
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

        public int TagSortIdx {
            get {
                if (Tag == null) {
                    return 0;
                }
                return Tag.TagSortIdx;
            }
            set {
                if (Tag.TagSortIdx != value) {
                    Tag.TagSortIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TagSortIdx));
                }
            }
        }

        public int TagTraySortIdx {
            get {
                if (Tag == null) {
                    return 0;
                }
                return Tag.TagTraySortIdx;
            }
            set {
                if (Tag.TagTraySortIdx != value) {
                    Tag.TagTraySortIdx = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TagTraySortIdx));
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
                        if(IsTagNameTextBoxFocused) {
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
        }


        public virtual async Task InitializeAsync(MpTag tag) {
            //PropertyChanged -= MpTagTileViewModel_PropertyChanged;
            //PropertyChanged += MpTagTileViewModel_PropertyChanged;

            IsBusy = true;

            Tag = tag;


            var ctl = await MpDataModelProvider.GetChildTagsAsync(TagId);

            foreach (var ct in ctl.OrderBy(x => x.TagSortIdx)) {
                var ttvm = await CreateChildTagTileViewModel(ct);
                Items.Add(ttvm);
            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }


            await UpdateClipCountAsync();
            //MpMessenger.UnregisterGlobal(ReceivedGlobalMessage);
            //MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            OnPropertyChanged(nameof(Items));
            //OnPropertyChanged(nameof(Children));
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
            //ttvm.ParentTreeItem = this;
            return ttvm;
        }

        //public bool IsCopyItemLinked(int ciid) {
        //    if (ciid == 0 || Tag == null || Tag.Id == 0) {
        //        return false;
        //    }
        //    bool isLinked = MpDataModelProvider.IsTagLinkedWithCopyItem(Tag.Id, ciid);

        //    return isLinked;
        //}

        public async Task<bool> IsCopyItemLinkedAsync(int ciid) {
            if (ciid == 0 || Tag == null ||  Tag.Id == 0) {
                return false;
            }
            bool isLinked = await MpDataModelProvider.IsTagLinkedWithCopyItemAsync(Tag.Id, ciid);
            return isLinked;
        }

        public void UpdateLinkToSelectedClipTile(IEnumerable<int> assocTagIds) {
            if(assocTagIds.Any(x=>x == TagId)) {
                IsLinkedToSelectedClipTile = true;
            } else if(AllDescendants.Cast<MpAvTagTileViewModel>().Any(x=>assocTagIds.Any(y=>y == x.TagId))) {
                IsLinkedToSelectedClipTile = null;
            } else {
                IsLinkedToSelectedClipTile = false;
            }
        }


        public override void Dispose() {
            base.Dispose();
            MpDb.SyncAdd -= MpDbObject_SyncAdd;
            MpDb.SyncUpdate -= MpDbObject_SyncUpdate;
            MpDb.SyncDelete -= MpDbObject_SyncDelete;

            PropertyChanged -= MpTagTileViewModel_PropertyChanged;
        }

        public override string ToString() {
            return TagName;
        }


        #endregion

        #region Protected Methods

        #region Db Events
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == TagId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } 
            //else if(e is MpCopyItem ci && IsAllTag) { // && !IsThisThePinnedViewModel
            //    LinkCopyItemCommand.Execute(ci.Id);
            //} 
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == TagId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if(e is MpTag t && t.Id == TagId) {
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
            }// else if (e is MpCopyItem ci && LinkedCopyItemIds.Contains(ci.Id)) {
            //    UnlinkCopyItemCommand.Execute(ci.Id);
            //}
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

                        if (!IsExpanded) {
                            IsExpanded = true;
                        }
                        Parent.SelectTagCommand.Execute(this);
                    } else {
                        IsTagNameReadOnly = true;
                    }
                    //Parent.OnPropertyChanged(nameof(Parent.SelectedPinnedItem));

                    //OnPropertyChanged(nameof(TagBorderBackgroundHexColor));
                    //MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.ClipTrayBackgroundBrush));
                    break;
                case nameof(IsTagNameTextBoxFocused):

                    if (!IsTagNameTextBoxFocused) {
                        FinishRenameTagCommand.Execute(null);
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
                case nameof(TagSortIdx):
                    if(ParentTreeItem == null) {
                        break;
                    }
                    ParentTreeItem.OnPropertyChanged(nameof(ParentTreeItem.SortedItems));
                    break;
                case nameof(TagTraySortIdx):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.PinnedItems));
                    break;
                case nameof(IsLinkedToSelectedClipTile):

                    OnPropertyChanged(nameof(TagTextHexColor));
                    break;
            }
        }

        private void CopyItemIdsNeedingView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if(e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Remove) {
                SelfAndAllAncestors.Cast<MpAvTagTileViewModel>().ForEach(x => x.OnPropertyChanged(nameof(x.BadgeCount)));
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.TrayScrollChanged:
                case MpMessageType.RequeryCompleted:
                case MpMessageType.JumpToIdxCompleted:
                    UpdateBadge();
                    break;
                case MpMessageType.TraySelectionChanged:

                    break;
            }
        }

        private async Task UpdateTreeSortOrder() {
            Items.ForEach(x => x.TagSortIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Tag.WriteToDatabaseAsync()));
        }

        private async Task UpdateClipCountAsync() {
            LinkedCopyItemIds = await MpDataModelProvider.GetCopyItemIdsForTagAsync(TagId);
            TagClipCount = SelfAndAllDescendants.Cast<MpAvTagTileViewModel>().SelectMany(x => x.LinkedCopyItemIds).Distinct().Count();
            //TagClipCount = this_tag_link_count + Items.Sum(x => x.TagClipCount);
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
                    if (civm.IsAnyCornerVisible && IsSelected) {
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

            if(isLink && OnCopyItemLinked.HasInvokers()) {
                var linked_ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(ciid);
                OnCopyItemLinked?.Invoke(this, linked_ci);
            } else if(!isLink && OnCopyItemUnlinked.HasInvokers()) {
                var unlinked_ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(ciid);
                OnCopyItemUnlinked?.Invoke(this, unlinked_ci);
            }

            IsBusy = false;
        }


        private async Task LinkOrUnlinkCopyItemAsync(int ciid, bool isLink) {
            IsBusy = true;
            bool affectedDb = false;
            
            if(isLink) {
                // try to create link, if it was created (and didn't already exist) notify any triggers
                var cit = await MpCopyItemTag.Create(TagId, ciid, LinkedCopyItemIds.Count());

                if (!cit.WasDupOnCreate) {
                    affectedDb = true;
                    CopyItemIdsNeedingView.Add(ciid);
                }
                
            } else {
                var cit = await MpDataModelProvider.GetCopyItemTagForTagAsync(ciid, TagId);

                if (cit != null) {
                    // only delete link/notify if exists
                    await MpDb.DeleteItemAsync(cit);
                    affectedDb = true;
                }
                CopyItemIdsNeedingView.Remove(ciid);
            }
            
            if(affectedDb) {                
                Dispatcher.UIThread.VerifyAccess();
                //TagClipCount += isLink ? 1 : -1;

                foreach(MpAvTagTileViewModel this_or_ancestor_ttvm in SelfAndAllAncestors) {
                    await this_or_ancestor_ttvm.UpdateClipCountAsync();
                    
                }

                // await notify so IsBusy doesn't trip
                await NotifyTriggersAsync(ciid, isLink);

                if(MpAvClipTrayViewModel.Instance.PersistantSelectedItemId == ciid) {
                    // trigger selection changed message to notify tag association change
                    MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
                }
            }
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
                    if(TagId == cit.TagId) {
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
            async() => {
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

        public ICommand AddNewChildTagCommand => new MpAsyncCommand(
             async () => {
                 MpTag t = await MpTag.Create(
                     parentTagId: Parent.SelectedItem.TagId,
                     sortIdx: Parent.SelectedItem.Items.Count);

                 MpAvTagTileViewModel ttvm = await CreateChildTagTileViewModel(t);

                 Parent.SelectedItem.Items.Add(ttvm);
                 Parent.SelectedItem.SelectedItem = ttvm;
                 Parent.SelectedItem.OnPropertyChanged(nameof(Parent.SelectedItem.Items));
                 Parent.OnPropertyChanged(nameof(Parent.Items));
             },() => CanAddChild);

        public ICommand DeleteChildTagCommand => new MpAsyncCommand<object>(
            async (args) => {
                var child_ttvm_to_remove = args as MpAvTagTileViewModel;
                var deleteTasks = (child_ttvm_to_remove.SelfAndAllDescendants.Cast<MpAvTagTileViewModel>().Select(x => x.Tag.DeleteFromDatabaseAsync()));
                await Task.WhenAll(deleteTasks);

                Items.Remove(child_ttvm_to_remove);                

                await UpdateTreeSortOrder();
                OnPropertyChanged(nameof(Items));
                Parent.OnPropertyChanged(nameof(Parent.PinnedItems));

                Parent.SelectTagCommand.Execute(this);                
            });

        public ICommand DeleteThisTagCommand => new MpCommand(
            () => {
                ParentTreeItem.DeleteChildTagCommand.Execute(this);
            }, ()=> !IsTagReadOnly);


        public ICommand LinkCopyItemCommand => new MpCommand<object>(
            (ciidArg) => {
                LinkOrUnlinkCopyItemAsync((int)ciidArg, true).FireAndForgetSafeAsync(this);
            }, (ciidArg) => {
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
                await LinkOrUnlinkCopyItemAsync((int)ciidArg, false);

            }, (ciidArg) => {
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
            });

        #endregion
    }
}
