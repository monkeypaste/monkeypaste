﻿using System;
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

namespace MonkeyPaste.Avalonia {
    public class MpAvTagTileViewModel : 
        MpAvSelectorViewModelBase<MpAvTagTrayViewModel, MpAvTagTileViewModel>, 
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpIHierarchialViewModel<MpAvTagTileViewModel>,
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

        #region MpIHierarchialViewModel Implementation

        public string Label {
            get => TagName;
            set => TagName = value;
        }

        public bool IsFocused { get; set; }
        public double LabelFontSize => TagHeight * 0.5;
        public string LabelForegroundHexColor => TagTextHexColor;
        public bool ShowAddButton => IsAllTag;
        public double ScreenWidth {
            get => TagTileTrayWidth;
            set => TagTileTrayWidth = value;
        }
        public double ScreenHeight => TagHeight;

        public string IconHexColor => TagHexColor;
        public string IconTextOrResourceKey => TagClipCount.ToString();
        public string IconLabelHexColor => TagCountTextHexColor;
        public string BackgroundHexColor => TagHexColor;
        public string BorderHexColor => TagBorderHexColor;

        public ICommand AddChildCommand => AddChildTagCommand;

        //MpITreeItemViewModel MpITreeItemViewModel.ParentTreeItem => ParentTreeItem;
        //ObservableCollection<MpITreeItemViewModel> MpITreeItemViewModel.Children => new ObservableCollection<MpITreeItemViewModel>(Children);
        public ObservableCollection<MpAvTagTileViewModel> Children => new ObservableCollection<MpAvTagTileViewModel>(Items.OrderBy(x=>x.TagSortIdx));

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; }

        public MpAvTagTileViewModel ParentTreeItem { get; set; }

        //public IList<MpAvTagTileViewModel> Children => Items;

        #endregion


        MpITreeItemViewModel MpITreeItemViewModel.ParentTreeItem { get; }
        ObservableCollection<MpITreeItemViewModel> MpITreeItemViewModel.Children { get; }

        #endregion

        #region MpISelectableViewModel Implementation

        private bool _isSelected = false;
        public bool IsSelected {
            get => _isSelected;
            set {
                if(IsSelected != value) {
                    if(!IsSelected && IsEditing) {
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
                    //Command = MpAvClipTrayViewModel.Instance.LinkTagToCopyItemCommand,
                    CommandParameter = this,
                    IsSelected = totalCount == linkCount && totalCount > 0,
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
                            CommandParameter = IsTreeContextMenuOpened,
                            IsVisible = RenameTagCommand.CanExecute(IsTreeContextMenuOpened)
                        },
                        new MpMenuItemViewModel() {
                            Header = "_Assign Hotkey",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("HotkeyImage") as string,
                            Command = AssignHotkeyCommand,
                            ShortcutObjId = TagId,
                            ShortcutType = MpShortcutType.SelectTag
                        },
                        new MpMenuItemViewModel() {
                            Header = IsPinned ? "_Unpin" : "_Pin",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PinImage") as string,
                            Command = Parent.ToggleTileIsPinnedCommand,
                            CommandParameter = this
                        },
                        new MpMenuItemViewModel() {IsSeparator = true},
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

        public MpShortcutViewModel ShortcutViewModel {
            get {
                if (Parent == null || Tag == null) {
                    return null;
                }
                var scvm = MpShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.CommandId == TagId && x.ShortcutType == ShortcutType);

                if (scvm == null) {
                    scvm = new MpShortcutViewModel(MpShortcutCollectionViewModel.Instance);
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

        public bool IsEditing => !IsTagNameTrayReadOnly || !IsTagNameTreeReadOnly;

        private bool _isTagNameTreeReadOnly = true;
        public bool IsTagNameTreeReadOnly {
            get {
                if(IsTagReadOnly) {
                    return true;
                }
                return _isTagNameTreeReadOnly;
            }
            set {
                if(IsTagReadOnly) {
                    return;
                }
                if (_isTagNameTreeReadOnly != value) {
                    _isTagNameTreeReadOnly = value;
                    OnPropertyChanged(nameof(IsTagNameTreeReadOnly));
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }
        private bool _isTagNameTrayReadOnly = true;
        public bool IsTagNameTrayReadOnly {
            get {
                if (IsTagReadOnly) {
                    return true;
                }
                return _isTagNameTrayReadOnly;
            }
            set {
                if (IsTagReadOnly) {
                    return;
                }
                if (_isTagNameTrayReadOnly != value) {
                    _isTagNameTrayReadOnly = value;
                    OnPropertyChanged(nameof(IsTagNameTrayReadOnly));
                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

        public bool IsTagNameTrayTextBoxFocused { get; set; } = false;
        public bool IsTagNameTreeTextBoxFocused { get; set; } = false;


        public bool IsAssociated { get; private set; }

        public bool IsTreeContextMenuOpened { get; set; } = false;
        public bool IsTrayContextMenuOpened { get; set; } = false;

        #endregion

        #region Visibility
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
                if(IsTrayContextMenuOpened || IsTreeContextMenuOpened) {
                    return MpSystemColors.red1;
                }
                if (IsAssociated) {
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

        public bool IsPinned {
            get {
                if(Tag == null) {
                    return false;
                }
                return Tag.IsPinned;
            }
            set {
                if(IsPinned != value) {
                    Tag.IsPinned = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsPinned));
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
                        if(IsTagNameTreeTextBoxFocused) {
                            IsTagNameTreeReadOnly = false;
                        } else if(IsTagNameTrayTextBoxFocused) {
                            IsTagNameTrayReadOnly = false;
                        }
                    }
                    HasModelChanged = true;
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

        public virtual async Task InitializeAsync(MpTag tag) {
            PropertyChanged -= MpTagTileViewModel_PropertyChanged;
            PropertyChanged += MpTagTileViewModel_PropertyChanged;

            IsBusy = true;

            Tag = tag;

            var ctl = await MpDataModelProvider.GetChildTagsAsync(TagId);

            foreach(var ct in ctl.OrderBy(x=>x.TagSortIdx)) {
                var ttvm = await CreateChildTagTileViewModel(ct);
                Items.Add(ttvm);
            }

            while(Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            _copyItemIdsNeedingView.CollectionChanged += _copyItemIdsNeedingView_CollectionChanged;

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(IsTagNameTrayReadOnly));

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
                if (sc.CommandId == TagId && sc.ShortcutType == ShortcutType) {
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
                if (sc.CommandId == TagId && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } 
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandId == TagId && sc.ShortcutType == ShortcutType) {
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
                case nameof(IsEditing):
                    if (IsEditing) {
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
                        IsTagNameTrayReadOnly = true;
                        IsTagNameTreeReadOnly = true;
                    }
                    //OnPropertyChanged(nameof(TagBorderBackgroundHexColor));
                    //MpAvClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvClipTrayViewModel.Instance.ClipTrayBackgroundBrush));
                    break;
                case nameof(IsTagNameTreeReadOnly):
                    if (!IsTagNameTreeReadOnly) {
                        IsTagNameTrayTextBoxFocused = false;
                        IsTagNameTreeTextBoxFocused = true;
                        //IsSelected = true;
                    }
                    break;
                case nameof(IsTagNameTrayReadOnly):
                    if (!IsTagNameTrayReadOnly) {
                        IsTagNameTreeTextBoxFocused = false;
                        IsTagNameTrayTextBoxFocused = true;
                        //IsSelected = true;
                    }
                    break;
                case nameof(IsTagNameTrayTextBoxFocused):
                case nameof(IsTagNameTreeTextBoxFocused):
                    if (!IsTagNameTreeTextBoxFocused && !IsTagNameTrayTextBoxFocused) {
                        FinishRenameTagCommand.Execute(null);
                    }
                    break;
                case nameof(HasModelChanged):
                    if (IsBusy) {
                        return;
                    }
                    if (HasModelChanged) {
                        Task.Run(async () => {
                            await Tag.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }

                    break;
                case nameof(TagHexColor):
                    Dispatcher.UIThread.Post(async () => {
                        while (HasModelChanged) {
                            await Task.Delay(100);
                        }
                        await Task.WhenAll(MpAvClipTrayViewModel.Instance.Items.Select(x => x.TitleSwirlViewModel.InitializeAsync()));
                    });
                    break;
                case nameof(TagTileTrayWidth):
                    if (Parent == null) {
                        return;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsNavButtonsVisible));
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.TraySelectionChanged:
                    Dispatcher.UIThread.InvokeAsync(UpdateAssociationAsync).FireAndForgetSafeAsync(this);
                    break;
                case MpMessageType.TrayScrollChanged:
                case MpMessageType.RequeryCompleted:
                case MpMessageType.JumpToIdxCompleted:
                    Dispatcher.UIThread.Post(UpdateNotifier);
                    break;
            }
        }

        private async Task UpdateSortOrder() {
            Items.ForEach(x => x.TagSortIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Tag.WriteToDatabaseAsync()));
        }

        private async Task UpdateAssociationAsync() {
            if(MpAvClipTrayViewModel.Instance.SelectedItem == null) {
                IsAssociated = false;
            } else {
                IsAssociated = await IsCopyItemLinkedAsync(MpAvClipTrayViewModel.Instance.SelectedItem.CopyItemId);
            }
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

        public ICommand AssignHotkeyCommand => new MpCommand<object>(
            async (args) => {
                await MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                            $"Select '{TagName}' Collection",
                            Parent.SelectTagCommand, 
                            ShortcutType,
                            TagId,
                            ShortcutKeyString);
                OnPropertyChanged(nameof(ShortcutKeyString));
            });

        public ICommand ChangeColorCommand => new MpCommand<object>(
            async (args) => {
                TagHexColor = args.ToString();
               
            });

        public ICommand CancelRenameTagCommand => new MpCommand(
            () => {
                TagName = _originalTagName;
                IsTagNameTrayReadOnly = true;
                IsTagNameTreeReadOnly = true;
            });

        public ICommand FinishRenameTagCommand => new MpAsyncCommand(
            async() => {
                IsTagNameTrayReadOnly = true;
                IsTagNameTreeReadOnly = true;
                await Tag.WriteToDatabaseAsync();
            });

        public ICommand RenameTagCommand => new MpCommand<object>(
             (isFromTreeArg) => {
                _originalTagName = TagName;
                if(isFromTreeArg is bool isFromTree && isFromTree) {
                     IsTagNameTreeReadOnly = false;
                     IsTagNameTrayReadOnly = true;
                 } else {
                     IsTagNameTrayReadOnly = false;
                     IsTagNameTreeReadOnly = true;
                 }
            },
            (isFromTreeArg) => {
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
             });

        public ICommand DeleteChildTagCommand => new MpAsyncCommand<object>(
            async (args) => {
                var ttvm = args as MpAvTagTileViewModel;
                var deleteTasks = ttvm.FindAllChildren().Select(x => x.Tag.DeleteFromDatabaseAsync()).ToList();
                deleteTasks.Add(ttvm.Tag.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                Items.Remove(ttvm);

                await UpdateSortOrder();
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