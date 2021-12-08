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

namespace MpWpfApp {
    public class MpTagTileViewModel : MpViewModelBase<MpTagTrayViewModel> {
        #region Private Variables
        private int _tagClipCount = 0;
        private string _originalTagName = string.Empty;
        private bool _wasEditingName = false;
        #endregion

        #region Properties

        #region View Models

        public MpTagPropertyCollectionViewModel TagProperties { get; set; } = new MpTagPropertyCollectionViewModel();
        #endregion

        #region State

        public bool HasProperties => TagProperties != null && TagProperties.TagProperties.Count > 0;

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
                return Tag.Id == MpTag.AllTagId || Tag.Id == MpTag.RecentTagId || Tag.Id == MpTag.HelpTagId;
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

        private string _shortcutKeyString = string.Empty;
        public string ShortcutKeyString {
            get {
                return _shortcutKeyString;
            }
            set {
                if (_shortcutKeyString != value) {
                    _shortcutKeyString = value;
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
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
                    Task.Run(async () => {
                        await Tag.WriteToDatabaseAsync();
                    });
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
            Tag = tag;

            IsBusy = true;

            TagProperties = new MpTagPropertyCollectionViewModel(this);
            await TagProperties.InitializeAsync(Tag);

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
                    MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.ClipTrayBackgroundBrush));
                    break;
            }
        }        

        public async Task AddContentItem(MpContentItemViewModel rtbvm) {
            var dupCheck = await MpDataModelProvider.Instance.GetCopyItemTagForTagAsync(TagId,rtbvm.CopyItemId);
            if(dupCheck != null) {
                MpConsole.WriteLine($"Tag {TagName} already contains a link with CopyItem {rtbvm.CopyItemTitle}, ignoring");
                return;
            }
            var ncit = new MpCopyItemTag() {
                TagId = TagId,
                CopyItemId = rtbvm.CopyItemId,
                CopyItemTagGuid = Guid.NewGuid()
            };
            await MpDb.Instance.AddOrUpdateAsync<MpCopyItemTag>(ncit);
        }

        public async Task RemoveContentItem(MpContentItemViewModel rtbvm) {
            var cit = await MpDataModelProvider.Instance.GetCopyItemTagForTagAsync(rtbvm.CopyItemId, TagId);
            if(cit == null) {
                MpConsole.WriteLine($"Tag {TagName} doesn't contain a link with CopyItem {rtbvm.CopyItemTitle} so cannot remove");
                return;
            }
            await MpDb.Instance.DeleteItemAsync<MpCopyItemTag>(cit);
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
                if (sc.TagId == TagId) {
                    ShortcutKeyString = sc.KeyString;
                }
            } 
            //else if(e is MpCopyItem ci && 
            //          ci.ItemType == MpCopyItemType.FileList &&
            //          TagId == MpTag.FavoritesTagId) {
            //    string pinToDir = Path.Combine(
            //        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            //        @"pintest");

            //    foreach(var fli in ci.ItemData.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)) {
            //        try {
            //            bool isFile = File.Exists(fli);
            //            string fname = Path.GetFileName(fli);

            //            string pinToPath = Path.Combine(pinToDir, fname);
            //            bool isNew = isFile ? File.Exists(pinToPath) : Directory.Exists(pinToPath);

            //            if (isNew) {
            //                File.Copy(fli, pinToPath, false);
            //            } else {
            //                string msgStr = isFile ? "File " : "Directory ";
            //                msgStr += " already exists";
            //                string result = MpMessageBox.ShowCustomMessageBox(
            //                    "Warning", 
            //                    msgStr, 
            //                    "/Images/warning.png", 
            //                    new string[]{ "Rename", "Overwrite", "Ignore"});
            //                MpConsole.WriteLine(result);
            //            }
            //        } catch(Exception ex) {
            //            MpConsole.WriteTraceLine(ex);
            //            continue;
            //        }
            //    }
                
            //}
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.TagId == TagId) {
                    ShortcutKeyString = sc.KeyString;
                }
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.TagId == TagId) {
                    ShortcutKeyString = string.Empty;
                }
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
                ShortcutKeyString = await MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                this,
                "Select " + TagName,
                ShortcutKeyString,
                Parent.SelectTagCommand, 
                TagId);
            });

        public ICommand ChangeColorCommand => new RelayCommand<object>(
            async (args) => {
                var newBrush = args as Brush;
                if (newBrush != null) {
                    TagBrush = newBrush;
                    await Tag.WriteToDatabaseAsync();
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
                //MpTagTrayViewModel.Instance.ClearTagSelection();
                //IsSelected = true;
                IsEditing = true;
            },
            () => {
                return !IsTagReadOnly;
            });

        #endregion
    }
}
