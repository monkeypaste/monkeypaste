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
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace MpWpfApp {
    public class MpTagTileViewModel : 
        MpViewModelBase<MpTagTrayViewModel>, 
        MpITreeItemViewModel, 
        MpIShortcutCommand, 
        MpIHasNotification, 
        MpIMatchTrigger {

        #region Private Variables
        private int _tagClipCount = 0;
        private string _originalTagName = string.Empty;
        private bool _wasEditingName = false;
        #endregion

        #region Properties

        #region View Models
        
        public MpITreeItemViewModel ParentTreeItem { get; set; }

        public ObservableCollection<MpITreeItemViewModel> Children { get; set; } = new ObservableCollection<MpITreeItemViewModel>();

        public ObservableCollection<MpMatcherViewModel> Matchers => new ObservableCollection<MpMatcherViewModel>(
                    MpMatcherCollectionViewModel.Instance.Matchers.Where(x =>
                        x.TriggerActionType == MpMatchActionType.Classify && x.Matcher.TriggerActionObjId == TagId).ToList());

        public void UnregisterMatcher(MpMatcherViewModel mvm) {
            OnCopyItemLinked -= mvm.OnMatcherTrigggered;
            MpConsole.WriteLine($"Matcher {mvm.Title} Unregistered from {TagName} TagAdd");
        }
        #endregion

        #region MpIShortcutCommand Implementation

        public MpShortcutType ShortcutType => MpShortcutType.SelectTag;

        public MpShortcutViewModel ShortcutViewModel {
            get {
                if (Parent == null || Tag == null) {
                    return null;
                }
                var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.CommandId == TagId && x.ShortcutType == ShortcutType);

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

        public bool HasNotification { get; set; } = false;

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
                return Tag.Id == MpTag.AllTagId || Tag.Id == MpTag.RecentTagId;
            }
        }

        public bool IsUserTag => !IsSudoTag;

        public bool IsRecentTag {
            get {
                if(Tag == null) {
                    return false;
                }
                return Tag.Id == MpTag.RecentTagId;
            }
        }

        public bool IsAllTag {
            get {
                if (Tag == null) {
                    return false;
                }
                return Tag.Id == MpTag.AllTagId;
            }
        }

        public bool IsSelected { get; set; }

        public bool IsExpanded { get; set; }

        private bool _isEditing = false;
        public bool IsEditing {
            get {
                return _isEditing;
            }
            set {
                if (_isEditing != value) {
                    _isEditing = value;

                    OnPropertyChanged(nameof(IsEditing));
                }
            }
        }

        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(TagBorderBackgroundBrush));
                    OnPropertyChanged(nameof(TagTextColor));
                }
            }
        }

        private bool _isAssociated = false;
        public bool IsAssociated {
            get {
                return _isAssociated;
            }
            set {
                if (_isAssociated != value) {
                    _isAssociated = value;
                    OnPropertyChanged(nameof(IsAssociated));
                    OnPropertyChanged(nameof(TagBorderBackgroundBrush));
                    OnPropertyChanged(nameof(TagBorderBrush));
                    OnPropertyChanged(nameof(TagTextColor));
                }
            }
        }

        public bool IsContextMenuOpened { get; set; } = false;

        #endregion

        #region Visibility
        #endregion

        #region Appearance

        public Brush TagBorderBackgroundBrush {
            get {
                if (IsSelected) {
                    return Brushes.DimGray;
                }
                if(IsHovering) {
                    return Brushes.LightGray;
                }
                return Brushes.Transparent;
            }
        }

        public Brush TagBorderBrush {
            get {
                if(IsContextMenuOpened) {
                    return Brushes.Red;
                }
                if (IsAssociated) {
                    return TagBrush;
                }
                return Brushes.Transparent;
            }
        }

        public Brush TagTextColor {
            get {
                if(IsSelected) {
                    return Brushes.White;
                }
                if(IsHovering) {
                    return Brushes.Black;
                }
                return Brushes.White;
            }
        }

        public Brush TagCountTextColor {
            get {
                return MpHelpers.Instance.IsBright(((SolidColorBrush)TagBrush).Color) ? Brushes.Black : Brushes.White; ;
            }
        }

        public int TagClipCount {
            get {
                return _tagClipCount;
            }
            set {
                if (_tagClipCount != value) {
                    _tagClipCount = value;
                    OnPropertyChanged(nameof(TagClipCount));
                }
            }
        }

        public double TagHeight {
            get {
                //assumes Tag Margin is 5
                return MpMeasurements.Instance.FilterMenuHeight - (5 * 2);
            }
        }

        public double TagFontSize {
            get {
                return TagHeight * 0.5;
            }
        }
        #endregion

        #region Model

        //public bool IsPinned => Parent != null &&
        //                        Parent.PinnedItems.Any(x => x.TagId == TagId);

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
                    Task.Run(async () => {
                        await Tag.WriteToDatabaseAsync();
                    });
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
                    Task.Run(async () => {
                        await Tag.WriteToDatabaseAsync();
                    });
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
                    Task.Run(async () => {
                        await Tag.WriteToDatabaseAsync();
                    });
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
                if (Tag.TagName != value) {
                    Tag.TagName = value;
                    if (Tag.TagName.Trim() == string.Empty) {
                        Tag.TagName = "Untitled";
                        IsEditing = true;
                    }                    
                    OnPropertyChanged(nameof(TagName));
                }
            }
        }

        public Brush TagBrush {
            get {
                if(Tag == null) {
                    return Brushes.Red;
                }
                return new SolidColorBrush(MpHelpers.Instance.ConvertHexToColor(Tag.HexColor));
            }
            set {
                if (new SolidColorBrush(MpHelpers.Instance.ConvertHexToColor(Tag.HexColor)) != value) {
                    Tag.HexColor = MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color);
                    //Task.Run(async () => {
                    //    await Tag.WriteToDatabaseAsync();
                    //});
                    OnPropertyChanged(nameof(TagBrush));
                    OnPropertyChanged(nameof(TagCountTextColor));
                }
            }
        }

        //public Color TagColor => ((SolidColorBrush)TagBrush).Color;

        private MpTag _tag;
        public MpTag Tag {
            get {
                return _tag;
            }
            set {
                if (_tag != value) {
                    _tag = value;
                    OnPropertyChanged(nameof(TagBrush));
                    OnPropertyChanged(nameof(TagName));
                    OnPropertyChanged(nameof(TagId));
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }


        #endregion

        #endregion

        #region Events

        public event EventHandler OnRequestSelectAll;

        public event EventHandler<MpCopyItem> OnCopyItemLinked;
        public event EventHandler<MpCopyItem> OnCopyItemUnlinked;

        #endregion

        #region Public Methods

        public MpTagTileViewModel() : base(null) { }

        public MpTagTileViewModel(MpTagTrayViewModel parent) : base(parent) {
            MonkeyPaste.MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;

            PropertyChanged += MpTagTileViewModel_PropertyChanged;
        }

        public virtual async Task InitializeAsync(MpTag tag) {
            PropertyChanged -= MpTagTileViewModel_PropertyChanged;
            PropertyChanged += MpTagTileViewModel_PropertyChanged;

            IsBusy = true;

            Tag = tag;

            await Task.Delay(1);
            IsBusy = false;
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
                case nameof(Tag):
                    break;
                case nameof(IsSelected):
                    //if (IsSelected) {
                    //    Task.Run(async()=> {
                    //        await MpClipTrayViewModel.Instance.RefreshTiles(); 
                    //    });
                    //}
                    if(IsSelected) {
                        if (!IsExpanded) {
                            IsExpanded = true;
                        }
                        Parent.SelectTagCommand.Execute(TagId);
                    }
                    //MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayBackgroundBrush));
                    break;
                case nameof(HasModelChanged):
                    if(IsBusy) {
                        return;
                    }
                    Task.Run(async () => {
                        await Tag.WriteToDatabaseAsync();
                        HasModelChanged = false;
                    });
                    break;
            }
        }

        public List<MpTagTileViewModel> FindChildren() {
            var cl = new List<MpTagTileViewModel>();
            foreach(var cttvm in Children.Cast<MpTagTileViewModel>()) {
                cl.Add(cttvm);
                cl.AddRange(cttvm.Children.Cast<MpTagTileViewModel>());
            }
            return cl;
        }

        public void RegisterMatcher(MpMatcherViewModel mvm) {
            OnCopyItemLinked += mvm.OnMatcherTrigggered;
            MpConsole.WriteLine($"TagTile {TagName} Registered {mvm.Title} matcher");
        }

        public async Task AddContentItem(int ciid) {
            var ncit = await MpCopyItemTag.Create(TagId, ciid);
            await ncit.WriteToDatabaseAsync();
        }

        public async Task RemoveContentItem(int ciid) {
            var cit = await MpDataModelProvider.Instance.GetCopyItemTagForTagAsync(ciid, TagId);
            if(cit == null) {
                MpConsole.WriteLine($"Tag {TagName} doesn't contain a link with CopyItem Id {ciid} so cannot remove");
                return;
            }
            await cit.DeleteFromDatabaseAsync();
        }

        public async Task<bool> IsLinked(MpCopyItem ci) {
            if (ci == null || ci.Id == 0 || Tag == null ||  Tag.Id == 0) {
                return false;
            }
            bool isLinked;

            if (IsAllTag) {
                isLinked = true;
            } else if (IsRecentTag) {
                isLinked = await MpDataModelProvider.Instance.IsCopyItemInRecentTag(ci.Id);
            } else {
                isLinked = await MpDataModelProvider.Instance.IsTagLinkedWithCopyItem(Tag.Id, ci.Id);
            }

            return isLinked;
        }

        public async Task<bool> IsLinked(MpClipTileViewModel ctvm) {
            foreach(var civm in ctvm.ItemViewModels) {
                bool isLinked = await IsLinked(civm);
                if(isLinked) {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> IsLinked(MpContentItemViewModel rtbvm) {
            var result = await IsLinked(rtbvm.CopyItem);
            return result;
        }

        public override void Dispose() {
            base.Dispose();
            MonkeyPaste.MpDb.Instance.SyncAdd -= MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.Instance.SyncUpdate -= MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.Instance.SyncDelete -= MpDbObject_SyncDelete;

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
            } else if (e is MpCopyItemTag cit && cit.TagId == TagId) {
                TagClipCount++;

                Task.Run(async () => {
                    var ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(cit.CopyItemId);
                OnCopyItemLinked?.Invoke(this, ci);
                });
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
            } else if (e is MpCopyItemTag cit && cit.TagId == TagId) {
                TagClipCount--;
                Task.Run(async () => {
                    var ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(cit.CopyItemId);
                    OnCopyItemUnlinked?.Invoke(this, ci);
                });
                
            } 
        }
        #endregion

        #endregion

        #region Private Methods

        #region Sync Event Handlers
        private void MpDbObject_SyncDelete(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            //throw new NotImplementedException();
        }

        private void MpDbObject_SyncUpdate(object sender, MonkeyPaste.MpDbSyncEventArgs e) {
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
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
            MpHelpers.Instance.RunOnMainThread(
                async() => {
                if (sender is MpCopyItemTag cit) {
                    if(TagId == cit.TagId) {
                        cit.StartSync(e.SourceGuid);
                            var dupCheck = await MpDataModelProvider.Instance.GetCopyItemTagForTagAsync(cit.TagId, cit.CopyItemId);
                        if (dupCheck != null) {
                            MonkeyPaste.MpConsole.WriteTraceLine(@"Warning, copyItemTag was duplicate: " + cit.ToString());
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
                            "Select " + TagName,
                            Parent.SelectTagCommand, 
                            ShortcutType,
                            TagId,
                            ShortcutKeyString);
                OnPropertyChanged(nameof(ShortcutKeyString));
            });

        public ICommand ChangeColorCommand => new RelayCommand<object>(
            async (args) => {
                var newBrush = args as Brush;
                if (newBrush != null) {
                    TagBrush = newBrush;
                    await Tag.WriteToDatabaseAsync();

                    await Task.Delay(50);

                    await Task.WhenAll(MpClipTrayViewModel.Instance.Items.SelectMany(x => x.ItemViewModels).Select(x => x.UpdateColorPallete()));
                }
            });

        public ICommand CancelRenameTagCommand => new RelayCommand(
            () => {
                TagName = _originalTagName;
                IsEditing = false;
            });

        public ICommand FinishRenameTagCommand => new RelayCommand(
            async() => {
                IsEditing = false;
                await Tag.WriteToDatabaseAsync();
            });

        public ICommand RenameTagCommand => new RelayCommand(
             () => {
                _originalTagName = TagName;
                IsEditing = true;
                OnRequestSelectAll?.Invoke(this, null);
            },
            () => {
                return !IsTagReadOnly;
            });

        #endregion
    }
}
