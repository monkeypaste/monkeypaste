using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.Diagnostics;

namespace MpWpfApp {
    public class MpTagTileViewModel : 
        MpSelectorViewModelBase<MpTagTrayViewModel, MpTagTileViewModel>, 
        MpIHoverableViewModel,
        MpISelectableViewModel,
        //MpITreeItemViewModel<MpTagTileViewModel>, 
        MpIHierarchialViewModel<MpTagTileViewModel>,
        //MpITreeItemViewModel,
        MpIShortcutCommand, 
        MpIBadgeNotificationViewModel,
        MpIUserColorViewModel,
        MpIActionComponent,
        MpIMenuItemViewModel{

        #region Private Variables
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
        public string BorderHexColor => TagTrayBorderHexColor;

        public ICommand AddChildCommand => AddChildTagCommand;

        //MpITreeItemViewModel MpITreeItemViewModel.ParentTreeItem => ParentTreeItem;
        //ObservableCollection<MpITreeItemViewModel> MpITreeItemViewModel.Children => new ObservableCollection<MpITreeItemViewModel>(Children);
        public ObservableCollection<MpTagTileViewModel> Children => Items;

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; }

        public MpTagTileViewModel ParentTreeItem { get; set; }

        //public IList<MpTagTileViewModel> Children => Items;

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

        #region MpIMenuItemViewModel Implementation

        //content menu item
        public MpMenuItemViewModel ContentMenuItemViewModel {
            get {
                int totalCount = MpClipTrayViewModel.Instance.SelectedModels.Count;
                int linkCount = MpClipTrayViewModel.Instance.SelectedModels.Where(x => IsLinked(x)).Count();
                return new MpMenuItemViewModel() {
                    Header = TagName,
                    Command = MpClipTrayViewModel.Instance.LinkTagToCopyItemCommand,
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

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = "_Rename",
                            IconResourceKey = Application.Current.Resources["RenameIcon"] as string,
                            Command = RenameTagCommand,
                            CommandParameter = IsTreeContextMenuOpened
                        },
                        new MpMenuItemViewModel() {
                            Header = "_Assign Hotkey",
                            IconResourceKey = Application.Current.Resources["HotkeyIcon"] as string,
                            Command = AssignHotkeyCommand,
                            ShortcutObjId = TagId,
                            ShortcutType = MpShortcutType.SelectTag
                        },
                        new MpMenuItemViewModel() {
                            Header = IsPinned ? "_Unpin" : "_Pin",
                            IconResourceKey = Application.Current.Resources["PinIcon"] as string,
                            Command = Parent.ToggleTileIsPinnedCommand,
                            CommandParameter = this
                        },
                        new MpMenuItemViewModel() {IsSeparator = true},
                        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(this),
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                            Header = "_Delete",
                            IconResourceKey = Application.Current.Resources["DeleteIcon"] as string,
                            Command = Parent.DeleteTagCommand,
                            CommandParameter = TagId
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

        #region MpIShortcutCommand Implementation

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

        #region MpIHasNotification Implementation

        public bool HasBadgeNotification { get; set; } = false;

        #endregion

        #region State

        public bool IsNew {
            get {
                return Tag == null || Tag.Id <= 0;
            }
        }

        public bool IsTagReadOnly {
            get {
                return TagId != 0 && TagId <= 4;
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


        public bool IsAllTag {
            get {
                if (Tag == null) {
                    return false;
                }
                return Tag.Id == MpTag.AllTagId;
            }
        }

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


        private bool _isAssociated = false;
        public bool IsAssociated {
            get {
                return _isAssociated;
            }
            set {
                if (_isAssociated != value) {
                    _isAssociated = value;
                    OnPropertyChanged(nameof(IsAssociated));
                    OnPropertyChanged(nameof(TagBorderBackgroundHexColor));
                    OnPropertyChanged(nameof(TagTrayBorderHexColor));
                    OnPropertyChanged(nameof(TagTreeBorderHexColor));
                    OnPropertyChanged(nameof(TagTextHexColor));
                }
            }
        }

        public bool IsTrayContextMenuOpened { get; set; } = false;

        public bool IsTreeContextMenuOpened { get; set; } = false;

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

        public string TagTrayBorderHexColor {
            get {
                if(IsTrayContextMenuOpened) {
                    return MpSystemColors.red1;
                }
                if (IsAssociated) {
                    return TagHexColor;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string TagTreeBorderHexColor {
            get {
                if (IsTreeContextMenuOpened) {
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
                return MpMeasurements.Instance.FilterMenuDefaultHeight - (5 * 2);
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

        public MpTagTileViewModel() : base(null) { }

        public MpTagTileViewModel(MpTagTrayViewModel parent) : base(parent) {
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
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(IsTagNameTrayReadOnly));
            OnPropertyChanged(nameof(IsTagNameTreeReadOnly));

            IsBusy = false;
        }

        public async Task<MpTagTileViewModel> CreateChildTagTileViewModel(MpTag tag) {
            MpTagTileViewModel ttvm = new MpTagTileViewModel(Parent);
            await ttvm.InitializeAsync(tag);
            ttvm.ParentTreeItem = this;
            return ttvm;
        }

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
                    if(IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        if (!IsExpanded) {
                            IsExpanded = true;
                        }
                        Parent.SelectTagCommand.Execute(this);
                    } else {
                        IsTagNameTrayReadOnly = true;
                        IsTagNameTreeReadOnly = true;
                    }
                    //MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayBackgroundBrush));
                    break;
                case nameof(IsTagNameTreeReadOnly):
                    if(!IsTagNameTreeReadOnly) {
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
                    if(!IsTagNameTreeTextBoxFocused && !IsTagNameTrayTextBoxFocused) {
                        FinishRenameTagCommand.Execute(null);
                    }
                    break;
                case nameof(HasModelChanged):
                    if(IsBusy) {
                        return;
                    }
                    if(HasModelChanged) {
                        Task.Run(async () => {
                            await Tag.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    
                    break;
                case nameof(TagHexColor):
                    MpHelpers.RunOnMainThread(async () => {
                        while (HasModelChanged) {
                            await Task.Delay(100);
                        }
                        await Task.WhenAll(MpClipTrayViewModel.Instance.Items.Select(x => x.TitleSwirlViewModel.InitializeAsync()));
                    });
                    break;
                case nameof(TagTileTrayWidth):
                    if(Parent == null) {
                        return;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsNavButtonsVisible));
                    break;
            }
        }

        public async Task AddContentItem(int ciid) {
            if(ciid == 0) {
                MpConsole.WriteTraceLine("Cannot add CopyItemId 0 to Tag: " + TagName + " Id: " + TagId);
                return;
            }
            if(!MpDataModelProvider.IsTagLinkedWithCopyItem(TagId,ciid)) {
                var ncit = await MpCopyItemTag.Create(TagId, ciid);
                await ncit.WriteToDatabaseAsync();
            }
            if(ParentTreeItem != null && ParentTreeItem.TagId != MpTag.AllTagId) {
                await ParentTreeItem.AddContentItem(ciid);
            }
        }

        public async Task RemoveContentItem(int ciid) {
            if (ciid == 0) {
                MpConsole.WriteTraceLine("Cannot remove CopyItemId 0 to Tag: " + TagName + " Id: " + TagId);
                return;
            }
            var cit = await MpDataModelProvider.GetCopyItemTagForTagAsync(ciid, TagId);
            if(cit == null) {
                MpConsole.WriteLine($"Tag {TagName} doesn't contain a link with CopyItem Id {ciid} so cannot remove");
                return;
            }
            await cit.DeleteFromDatabaseAsync();
            if (ParentTreeItem != null) {
                await ParentTreeItem.RemoveContentItem(ciid);
            }
        }

        public bool IsLinked(MpCopyItem ci) {
            if (ci == null || ci.Id == 0 || Tag == null || Tag.Id == 0) {
                return false;
            }
            bool isLinked;

            if (IsAllTag) {
                isLinked = true;
            } else {
                isLinked = MpDataModelProvider.IsTagLinkedWithCopyItem(Tag.Id, ci.Id);
            }

            return isLinked;
        }

        public async Task<bool> IsLinkedAsync(MpCopyItem ci) {
            if (ci == null || ci.Id == 0 || Tag == null ||  Tag.Id == 0) {
                return false;
            }
            bool isLinked;

            if (IsAllTag) {
                isLinked = true;
            } else {
                isLinked = await MpDataModelProvider.IsTagLinkedWithCopyItemAsync(Tag.Id, ci.Id);
            }

            return isLinked;
        }

        public async Task<bool> IsLinkedAsync(MpClipTileViewModel ctvm) {
            bool isLinked = await IsLinkedAsync(ctvm.CopyItem);
            if (isLinked) {
                return true;
            }
            return false;
        }

        public void NotifyAllTagItemLinked(MpCopyItem ci) { 
            if(!IsAllTag) {
                Debugger.Break();
            }
            OnCopyItemLinked?.Invoke(this, ci);
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
            } else if (e is MpCopyItemTag cit && cit.TagId == TagId) {
                TagClipCount++;

                Task.Run(async () => {
                    var ci = await MpDataModelProvider.GetCopyItemByIdAsync(cit.CopyItemId);
                    OnCopyItemLinked?.Invoke(this, ci);
                });
            } else if(e is MpCopyItem ci && IsAllTag) {
                TagClipCount++;
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == TagId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } 
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == TagId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            } else if (e is MpCopyItemTag cit && cit.TagId == TagId) {
                TagClipCount--;
                Task.Run(async () => {                    
                    var ci = await MpDataModelProvider.GetCopyItemByIdAsync(cit.CopyItemId);
                    if(ci != null) {
                        OnCopyItemUnlinked?.Invoke(this, ci);
                    }
                });                
            } else if (e is MpCopyItem ci && IsLinked(ci)) {
                Task.Run(async () => {
                    var ct = await MpDataModelProvider.GetCopyItemTagForTagAsync(ci.Id, TagId);
                    if(ct != null) {
                        await ct.DeleteFromDatabaseAsync();
                    }
                });
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private async Task UpdateSortOrder() {
            Items.ForEach(x => x.TagSortIdx = Items.IndexOf(x));
            await Task.WhenAll(Items.Select(x => x.Tag.WriteToDatabaseAsync()));
        }

        #region Sync Event Handlers
        private void MpDbObject_SyncDelete(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            //throw new NotImplementedException();
        }

        private void MpDbObject_SyncUpdate(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.RunOnMainThread((Action)(() => {
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
            }));
        }

        private void MpDbObject_SyncAdd(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.RunOnMainThread(
                async() => {
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

        public ICommand AssignHotkeyCommand => new RelayCommand<object>(
            async (args) => {
                await MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                            $"Select '{TagName}' Collection",
                            Parent.SelectTagCommand, 
                            ShortcutType,
                            TagId,
                            ShortcutKeyString);
                OnPropertyChanged(nameof(ShortcutKeyString));
            });

        public ICommand ChangeColorCommand => new RelayCommand<object>(
             (args) => {
                TagHexColor = args.ToString();
               
            });

        public ICommand CancelRenameTagCommand => new RelayCommand(
            () => {
                TagName = _originalTagName;
                IsTagNameTrayReadOnly = true;
                IsTagNameTreeReadOnly = true;
            });

        public ICommand FinishRenameTagCommand => new RelayCommand(
            async() => {
                IsTagNameTrayReadOnly = true;
                IsTagNameTreeReadOnly = true;
                await Tag.WriteToDatabaseAsync();
            });

        public ICommand RenameTagCommand => new RelayCommand<bool>(
             (isFromTree) => {
                _originalTagName = TagName;
                if(isFromTree) {
                     IsTagNameTreeReadOnly = false;
                     IsTagNameTrayReadOnly = true;
                 } else {
                     IsTagNameTrayReadOnly = false;
                     IsTagNameTreeReadOnly = true;
                 }
            },
            (isFromTree) => {
                return !IsTagReadOnly;
            });

        public ICommand AddChildTagCommand => new RelayCommand<object>(
             async (arg) => {
                 //only called in All Tag
                 MpTagTileViewModel ttvm = null;

                 if(arg == null) {
                     MpTag t = await MpTag.Create(
                     parentTagId: Parent.SelectedTagTile.TagId,
                     sortIdx: Parent.SelectedTagTile.Items.Count);

                     ttvm = await Parent.SelectedTagTile.CreateChildTagTileViewModel(t);
                 } else if(arg is MpTagTileViewModel) {
                     ttvm = arg as MpTagTileViewModel;
                     if(ttvm.ParentTreeItem != null) {
                         ttvm.ParentTreeItem.Items.Remove(ttvm);
                     }
                 }

                 Parent.SelectedTagTile.Items.Add(ttvm);

                 Parent.SelectedTagTile.SelectedItem = ttvm;

                 Parent.SelectedTagTile.OnPropertyChanged(nameof(Parent.SelectedTagTile.Items));
                 Parent.OnPropertyChanged(nameof(Parent.Items));
             });

        public ICommand DeleteChildTagCommand => new RelayCommand<object>(
            async (args) => {
                var ttvm = args as MpTagTileViewModel;
                var deleteTasks = ttvm.FindAllChildren().Select(x => (x as MpTagTileViewModel).Tag.DeleteFromDatabaseAsync()).ToList();
                deleteTasks.Add(ttvm.Tag.DeleteFromDatabaseAsync());
                await Task.WhenAll(deleteTasks);

                Items.Remove(ttvm);

                await UpdateSortOrder();
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItem));
            });

        public ICommand DeleteThisTagCommand => new RelayCommand(
            () => {
                ParentTreeItem.DeleteChildTagCommand.Execute(this);
            }, !IsTagReadOnly);




        #endregion
    }
}
