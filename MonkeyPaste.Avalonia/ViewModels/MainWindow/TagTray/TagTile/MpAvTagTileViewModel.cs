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

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTileViewModel : 
        MpAvSelectorViewModelBase<MpAvTagTrayViewModel, MpAvTagTileViewModel>, 
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpITreeItemViewModel<MpAvTagTileViewModel>,
        MpAvIShortcutCommand, 
        MpIBadgeNotificationViewModel,
        MpIUserColorViewModel,
        MpIActionComponent,
        MpIContextMenuViewModel {

        #region Private Variables
        private ObservableCollection<int> _copyItemIdsNeedingView = new ObservableCollection<int>();

        private string _originalTagName = string.Empty;
        private bool _wasEditingName = false;
        #endregion

        #region Properties

        #region View Models

        #endregion

        #region MpITreeItemViewModel Implementation

        public ObservableCollection<MpAvTagTileViewModel> Children => new ObservableCollection<MpAvTagTileViewModel>(Items.OrderBy(x => x.TagSortIdx));
        public bool IsExpanded { get; set; }

        public MpAvTagTileViewModel ParentTreeItem { get; set; }

        MpITreeItemViewModel MpITreeItemViewModel.ParentTreeItem => ParentTreeItem;
        ObservableCollection<MpITreeItemViewModel> MpITreeItemViewModel.Children => new ObservableCollection<MpITreeItemViewModel>(Children.Cast<MpITreeItemViewModel>());

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
                int totalCount = 1;// MpAvClipTrayViewModel.Instance.SelectedModels.Count;
                int linkCount = IsCopyItemLinked(MpAvClipTrayViewModel.Instance.SelectedItem.CopyItem.Id) ? 1 : 0;//MpAvClipTrayViewModel.Instance.SelectedModels.Where(x => IsLinked(x)).Count();
                return new MpMenuItemViewModel() {
                    Header = TagName,
                    Command = MpAvClipTrayViewModel.Instance.ToggleLinkTagToCopyItemCommand,
                    CommandParameter = this,
                    IsChecked = totalCount == linkCount && totalCount > 0,
                    IsPartiallySelected = linkCount != totalCount && totalCount > 0,
                    IconHexStr = TagHexColor,
                    ShortcutObjId = TagId,
                    ShortcutType = MpShortcutType.SelectTag,
                    SubItems = Items.Select(x => x.ContentMenuItemViewModel).ToList()
                };
            }
        }

        public MpMenuItemViewModel ContextMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = "_Rename",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("RenameImage") as string, //MpPlatformWrapper.Services.PlatformResource.GetResource("RenameIcon") as string,
                            Command = RenameTagCommand,
                            IsVisible = RenameTagCommand.CanExecute(null)
                        },
                        new MpMenuItemViewModel() {
                            Header = "_Assign Hotkey",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("HotkeyImage") as string,
                            Command = AssignHotkeyCommand,
                            ShortcutObjId = TagId,
                            ShortcutType = MpShortcutType.SelectTag
                        },
                        new MpMenuItemViewModel() {
                            Header = IsModelPinned ? "_Unpin" : "_Pin",
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

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpITriggerActionViewModel Implementation

        public void RegisterActionComponent(MpIActionTrigger mvm) {
            OnCopyItemLinked += mvm.OnActionTriggered;
            MpConsole.WriteLine($"TagTile {TagName} Registered {mvm.Label} matcher");
        }

        public void UnregisterActionComponent(MpIActionTrigger mvm) {
            OnCopyItemLinked -= mvm.OnActionTriggered;
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

        #region MpAvIBadgeNotifierViewModel Implementation

        bool MpIBadgeNotificationViewModel.HasBadgeNotification { get; set; } = false;

        #endregion

        #region State

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

        public bool IsAllTag => TagId == MpTag.AllTagId;
        public bool IsFavoriteTag => TagId == MpTag.FavoritesTagId;
        public bool IsHelpTag => TagId == MpTag.HelpTagId;

        public bool IsTagNameReadOnly { get; set; } = true;


        public bool IsTagNameTextBoxFocused { get; set; } = false;


        public bool IsLinkedToSelectedClipTile { get; set; }

        public bool IsContextMenuOpened { get; set; } = false;

        #endregion

        #region Appearance

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
                if(IsContextMenuOpened) {
                    return MpSystemColors.red1;
                }
                if (IsSelected && IsLinkedToSelectedClipTile) {
                    return TagHexColor;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string TagTextHexColor {
            get {
                if(IsSelected) {
                    return MpSystemColors.White;
                }
                if(IsHovering) {
                    return MpSystemColors.Black;
                }
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

        public double TagTileTrayWidth { get; set; }

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
            MonkeyPaste.MpDb.SyncAdd += MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.SyncDelete += MpDbObject_SyncDelete;

            PropertyChanged += MpTagTileViewModel_PropertyChanged;
        }

        public virtual async Task InitializeAsync(MpTag tag, bool isPinnedItem = false) {
            PropertyChanged -= MpTagTileViewModel_PropertyChanged;
            PropertyChanged += MpTagTileViewModel_PropertyChanged;

            IsBusy = true;

            Tag = tag;

            if (!isPinnedItem) {
                var ctl = await MpDataModelProvider.GetChildTagsAsync(TagId);

                foreach (var ct in ctl.OrderBy(x => x.TagSortIdx)) {
                    var ttvm = await CreateChildTagTileViewModel(ct);
                    Items.Add(ttvm);
                }

                while (Items.Any(x => x.IsBusy)) {
                    await Task.Delay(100);
                }
            }

            MpMessenger.UnregisterGlobal(ReceivedGlobalMessage);
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            _copyItemIdsNeedingView.CollectionChanged -= _copyItemIdsNeedingView_CollectionChanged;
            _copyItemIdsNeedingView.CollectionChanged += _copyItemIdsNeedingView_CollectionChanged;

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(IsTagNameReadOnly));

            if(Parent.Items.All(x=>x.TagId != TagId)) {
                Parent.Items.Add(this);
            }

            IsBusy = false;
        }

        public async Task<MpAvTagTileViewModel> CreateChildTagTileViewModel(MpTag tag) {
            MpAvTagTileViewModel ttvm = new MpAvTagTileViewModel(Parent);
            await ttvm.InitializeAsync(tag);
            ttvm.ParentTreeItem = this;
            return ttvm;
        }

        public bool IsCopyItemLinked(int ciid) {
            if (ciid == 0 || Tag == null || Tag.Id == 0) {
                return false;
            }
            bool isLinked = MpDataModelProvider.IsTagLinkedWithCopyItem(Tag.Id, ciid);

            return isLinked;
        }

        public async Task<bool> IsCopyItemLinkedAsync(int ciid) {
            if (ciid == 0 || Tag == null ||  Tag.Id == 0) {
                return false;
            }
            bool isLinked = await MpDataModelProvider.IsTagLinkedWithCopyItemAsync(Tag.Id, ciid);
            return isLinked;
        }

        public bool IsChildOfTag(int tid, bool recursive) {
            if(recursive) {
                var curTagItem = ParentTreeItem;
                while (curTagItem != null) {
                    if (curTagItem.TagId == tid) {
                        return true;
                    }
                    curTagItem = curTagItem.ParentTreeItem;
                }
                return false;
            }
            return tid == ParentTagId;
        }

        public bool IsParentOfTag(int tid, bool recursive) {
            if(recursive) {
                return this.FindAllChildren().Any(x => x.TagId == tid);
            }
            return Children.Any(x => x.TagId == tid);
        }


        public override void Dispose() {
            base.Dispose();
            MonkeyPaste.MpDb.SyncAdd -= MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.SyncUpdate -= MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.SyncDelete -= MpDbObject_SyncDelete;

            PropertyChanged -= MpTagTileViewModel_PropertyChanged;
        }


        #endregion

        #region Protected Methods

        #region Db Events
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == TagId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if (e is MpCopyItemTag cit) {
                if(cit.TagId == TagId) {
                    //link command was already called
                    if(!_copyItemIdsNeedingView.Contains(cit.CopyItemId)) {
                        _copyItemIdsNeedingView.Add(cit.CopyItemId);
                    }
                    Dispatcher.UIThread.Post(() => {
                        TagClipCount++;
                    });
                } else if(IsParentOfTag(cit.TagId,false)) {
                    LinkCopyItemCommand.Execute(cit.CopyItemId);
                }                
            } else if(e is MpCopyItem ci && IsAllTag) {
                LinkCopyItemCommand.Execute(ci.Id);
            } 
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
                });
            }
    }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == TagId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if (e is MpCopyItemTag cit) {
                if(cit.TagId == TagId) {
                    // unlink command was already called
                    if (_copyItemIdsNeedingView.Contains(cit.CopyItemId)) {
                        _copyItemIdsNeedingView.Add(cit.CopyItemId);
                    }
                    Dispatcher.UIThread.Post(() => {
                        TagClipCount--;
                    });
                } else if(IsParentOfTag(cit.TagId,false)) {
                    UnlinkCopyItemCommand.Execute(cit.CopyItemId);
                }
            } else if (e is MpCopyItem ci && IsCopyItemLinked(ci.Id)) {
                UnlinkCopyItemCommand.Execute(ci.Id);
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
                case nameof(TagTileTrayWidth):
                    if (Parent == null) {
                        return;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsNavButtonsVisible));
                    Parent.OnPropertyChanged(nameof(Parent.TagTrayScreenWidth));
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.TrayScrollChanged:
                case MpMessageType.RequeryCompleted:
                case MpMessageType.JumpToIdxCompleted:
                    Dispatcher.UIThread.Post(UpdateNotifier);
                    break;
            }
        }

        private async Task UpdateTreeSortOrder() {
            Items.ForEach(x => x.TagSortIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Tag.WriteToDatabaseAsync()));
        }


        private void UpdateNotifier() {
            var idsSeen = new List<int>();
            foreach (int ciid in _copyItemIdsNeedingView.ToList()) {
                var civm = MpAvClipTrayViewModel.Instance.GetClipTileViewModelById(ciid);
                if (civm != null) {
                    if (civm.IsPinned) {
                        // only mark item as seen if viewed in its query tray
                        continue;
                    }
                    if (civm.IsVisible && IsSelected) {
                        idsSeen.Add(ciid);
                    }
                }

            }
            int idsToRemoveCount = idsSeen.Count;
            while (idsToRemoveCount > 0) {
                _copyItemIdsNeedingView.Remove(idsSeen[idsToRemoveCount - 1]);
                idsToRemoveCount--;
            }

            (this as MpIBadgeNotificationViewModel).HasBadgeNotification = _copyItemIdsNeedingView.Count > 0;
        }

        private void _copyItemIdsNeedingView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            Dispatcher.UIThread.Post(UpdateNotifier);
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

        public ICommand AddChildTagCommand => new MpAsyncCommand<object>(
             async (arg) => {
                 //only called in All Tag
                 MpAvTagTileViewModel ttvm = null;

                 if(arg == null) {
                     MpTag t = await MpTag.Create(
                     parentTagId: Parent.SelectedItem.TagId,
                     sortIdx: Parent.SelectedItem.Items.Count);

                     ttvm = await Parent.SelectedItem.CreateChildTagTileViewModel(t);
                 } else if(arg is MpAvTagTileViewModel) {
                     ttvm = arg as MpAvTagTileViewModel;
                     if(ttvm.ParentTreeItem != null) {
                         ttvm.ParentTreeItem.Items.Remove(ttvm);
                     }
                 }

                 Parent.SelectedItem.Items.Add(ttvm);

                 Parent.SelectedItem.SelectedItem = ttvm;

                 Parent.SelectedItem.OnPropertyChanged(nameof(Parent.SelectedItem.Items));
                 Parent.OnPropertyChanged(nameof(Parent.Items));
             },(arg) => CanAddChild);

        public ICommand DeleteChildTagCommand => new MpAsyncCommand<object>(
            async (args) => {
                var ttvm = args as MpAvTagTileViewModel;
                if (ttvm.IsModelPinned) {
                    var pttvm_toRemove = Parent.PinnedItems.FirstOrDefault(x => x.TagId == ttvm.TagId);
                    Parent.PinnedItems.Remove(pttvm_toRemove);
                }
                
                var deleteTasks = ttvm.FindAllChildren().Select(x => x.Tag.DeleteFromDatabaseAsync()).ToList();
                deleteTasks.Add(ttvm.Tag.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                var ttvm_toRemove = Parent.PinnedItems.FirstOrDefault(x => x.TagId == ttvm.TagId);
                Parent.PinnedItems.Remove(ttvm_toRemove);
                Items.Remove(ttvm_toRemove);
                

                await UpdateTreeSortOrder();
                OnPropertyChanged(nameof(Items));

                Parent.SelectTagCommand.Execute(this);                
            });

        public ICommand DeleteThisTagCommand => new MpCommand(
            () => {
                ParentTreeItem.DeleteChildTagCommand.Execute(this);
            }, ()=> !IsTagReadOnly);

        public ICommand LinkCopyItemCommand => new MpAsyncCommand<object>(
            async (ciidArg) => {
                if(ciidArg is int ciid) {
                    if (ciid == 0) {
                        MpConsole.WriteTraceLine("Cannot add CopyItemId 0 to Tag: " + TagName + " Id: " + TagId);
                        return;
                    }
                    IsBusy = true;

                    bool isLinked = await IsCopyItemLinkedAsync(ciid);
                    if (!isLinked) {
                        var ncit = await MpCopyItemTag.Create(TagId, ciid);
                        await ncit.WriteToDatabaseAsync();
                    }

                    IsBusy = false;
                }
            });

        public ICommand UnlinkCopyItemCommand => new MpAsyncCommand<object>(
            async (ciidArg) => {
                if (ciidArg is int ciid) {
                    if (ciid == 0) {
                        MpConsole.WriteTraceLine("Cannot remove CopyItemId 0 to Tag: " + TagName + " Id: " + TagId);
                        return;
                    }
                    IsBusy = true;

                    var cit = await MpDataModelProvider.GetCopyItemTagForTagAsync(ciid, TagId);
                    if (cit == null) {
                        MpConsole.WriteLine($"Tag {TagName} doesn't contain a link with CopyItem Id {ciid} so cannot remove");
                        return;
                    }
                    await cit.DeleteFromDatabaseAsync();

                    IsBusy = false;
                }
            });

        #endregion
    }
}
