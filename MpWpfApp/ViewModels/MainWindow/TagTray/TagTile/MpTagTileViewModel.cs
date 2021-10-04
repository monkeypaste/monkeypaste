using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;
using Microsoft.WindowsAPICodePack.Shell;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpTagTileViewModel : MpViewModelBase<MpTagTrayViewModel> {
        #region Private Variables
        private int _tagClipCount = 0;
        private string _originalTagName = string.Empty;
        private bool _wasEditingName = false;
        #endregion

        #region Properties

        #region View Models
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
                return Tag.Id == MpTag.AllTagId || Tag.Id == MpTag.RecentTagId || Tag.Id == MpTag.HelpTagId;
            }
        }

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

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) {// || MpClipTrayViewModel.Instance.IsFilteringByApp) {
                    _isSelected = value;
                    OnPropertyChanged_old(nameof(IsSelected));
                    OnPropertyChanged_old(nameof(TagBorderBackgroundBrush));
                    OnPropertyChanged_old(nameof(TagTextColor));
                }
            }
        }

        private bool _isEditing = false;
        public bool IsEditing {
            get {
                return _isEditing;
            }
            set {
                if (_isEditing != value) {
                    _isEditing = value;

                    OnPropertyChanged_old(nameof(IsEditing));
                    OnPropertyChanged_old(nameof(TextBlockVisibility));
                    OnPropertyChanged_old(nameof(TextBoxVisibility));
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
                    OnPropertyChanged_old(nameof(IsHovering));
                    OnPropertyChanged_old(nameof(TagBorderBackgroundBrush));
                    OnPropertyChanged_old(nameof(TagTextColor));
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
                    OnPropertyChanged_old(nameof(IsAssociated));
                    OnPropertyChanged_old(nameof(TagBorderBackgroundBrush));
                    OnPropertyChanged_old(nameof(TagBorderBrush));
                    OnPropertyChanged_old(nameof(TagTextColor));
                }
            }
        }
        #endregion

        #region Visibility
        public Visibility TextBoxVisibility {
            get {
                if(IsEditing) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility TextBlockVisibility {
            get {
                if (IsEditing) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }
        #endregion

        #region Visual
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
                if (IsAssociated) {
                    return TagColor;
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
                return MpHelpers.Instance.IsBright(((SolidColorBrush)TagColor).Color) ? Brushes.Black : Brushes.White; ;
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
                    OnPropertyChanged_old(nameof(ShortcutKeyString));
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
                    OnPropertyChanged_old(nameof(TagClipCount));
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
                return Tag.Id;
            }
            set {
                if (Tag.Id != value) {
                    Tag.Id = value;
                    Tag.WriteToDatabase();
                    OnPropertyChanged_old(nameof(TagId));
                }
            }
        }

        public int TagSortIdx {
            get {
                return Tag.TagSortIdx;
            }
            set {
                if (Tag.TagSortIdx != value) {
                    Tag.TagSortIdx = value;
                    Tag.WriteToDatabase();
                    OnPropertyChanged_old(nameof(TagSortIdx));
                }
            }
        }

        public string TagName {
            get {
                return Tag.TagName;
            }
            set {
                if (Tag.TagName != value) {
                    Tag.TagName = value;
                    if (Tag.TagName.Trim() == string.Empty) {
                        Tag.TagName = "Untitled";
                        IsEditing = true;
                    }                    
                    OnPropertyChanged_old(nameof(TagName));
                }
            }
        }

        public Brush TagColor {
            get {
                if(Tag == null) {
                    return Brushes.Red;
                }
                return new SolidColorBrush(MpHelpers.Instance.ConvertHexToColor(Tag.HexColor));
            }
            set {
                if (new SolidColorBrush(MpHelpers.Instance.ConvertHexToColor(Tag.HexColor)) != value) {
                    Tag.HexColor = MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color);
                    Tag.WriteToDatabase();
                    OnPropertyChanged_old(nameof(TagColor));
                    OnPropertyChanged_old(nameof(TagCountTextColor));
                }
            }
        }

        private MpTag _tag;
        public MpTag Tag {
            get {
                return _tag;
            }
            set {
                if (_tag != value) {
                    _tag = value;
                    OnPropertyChanged_old(nameof(TagColor));
                    OnPropertyChanged_old(nameof(TagName));
                    OnPropertyChanged_old(nameof(TagId));
                    OnPropertyChanged_old(nameof(Tag));
                }
            }
        }

        
        #endregion

        #endregion

        #region Public Methods
        public MpTagTileViewModel(MpTagTrayViewModel parent, MpTag tag) : base(parent) {
            MonkeyPaste.MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;

            PropertyChanged += MpTagTileViewModel_PropertyChanged;

            Tag = tag;
        }

        private async void MpTagTileViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsEditing):
                    if (IsEditing) {
                        _wasEditingName = true;
                        _originalTagName = TagName;
                    } else {
                        if (_wasEditingName) {
                            _wasEditingName = false;
                            if (TagName != _originalTagName) {
                                Tag.WriteToDatabase();
                            }
                        }
                    }
                    break;
                case nameof(Tag):
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        await MpClipTrayViewModel.Instance.RefreshTiles();
                    }
                    break;
            }
        }

        public void TagTile_Loaded(object sender, RoutedEventArgs e) {
            var tagBorder = (MpClipBorder)sender;
            tagBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            tagBorder.MouseLeave += (s, e1) => {
                IsHovering = false;
            };
            tagBorder.LostFocus += (s, e4) => {
                if (!IsSelected) {
                    IsEditing = false;
                }
            };
            tagBorder.PreviewMouseLeftButtonDown += (s, e7) => {
                if(e7.ClickCount == 2) {
                    RenameTagCommand.Execute(null);
                } else {
                    SelectTagCommand.Execute(null);
                }
            };

            var tagTextBox = (TextBox)tagBorder.FindName("TagTextBox");
            //this is called 
            tagTextBox.IsVisibleChanged += (s, e1) => {
                if(TextBoxVisibility == Visibility.Visible) {
                    tagTextBox.Focus();
                    tagTextBox.SelectAll();
                } else {
                    Tag.WriteToDatabase();
                }
            };
            tagTextBox.LostFocus += (s, e2) => {
                IsEditing = false;
            };
            tagTextBox.PreviewKeyDown += (s, e3) => {
                if(e3.Key == Key.Enter) {
                    IsEditing = false;
                } else if(e3.Key == Key.Escape) {
                    TagName = _originalTagName;
                    IsEditing = false;
                }
            };
            

            OnViewModelLoaded();
        }


        public void AddClip(MpContentItemViewModel rtbvm) {
            Tag.LinkWithCopyItem(rtbvm.CopyItem);
        }

        public void RemoveClip(MpContentItemViewModel rtbvm) {
            Tag.UnlinkWithCopyItem(rtbvm.CopyItem);
        }

        public bool IsLinked(MpCopyItem ci) {
            if (ci == null ||
                ci.Id == 0 ||
                Tag == null ||
                Tag.Id == 0) {
                return false;
            }
            if (IsAllTag) {
                return true;
            }
            if (IsRecentTag) {
                //                (from ci in MpDb.Instance.GetItems<MpCopyItem>()
                //select ci)
                //                                 .OrderByDescending(x => x.GetType().GetProperty(sortColumn).GetValue(x))
                //                                 .Take(count)
                //                                 .Skip(start)
                //                                 .ToList();
                return MpDb.Instance.GetItems<MpCopyItem>()
                             .OrderByDescending(x => x.CopyDateTime)
                             .Take(MpMeasurements.Instance.MaxRecentClipItems)
                             .Any(y => y.Id == ci.Id);
            }
            return Tag.IsLinkedWithCopyItem(ci);
        }

        public async Task<bool> IsLinkedAsync(MpCopyItem ci) {
            if (ci == null ||
                ci.Id == 0 ||
                Tag == null ||
                Tag.Id == 0) {
                return false;
            }
            if (IsAllTag) {
                return true;
            }
            if (IsRecentTag) {
                //                (from ci in MpDb.Instance.GetItems<MpCopyItem>()
                //select ci)
                //                                 .OrderByDescending(x => x.GetType().GetProperty(sortColumn).GetValue(x))
                //                                 .Take(count)
                //                                 .Skip(start)
                //                                 .ToList();
                var result = await MpDb.Instance.QueryAsync("MpCopyItem","select Id from MpCopyItem orderby CopyDateTime limit ?", MpMeasurements.Instance.MaxRecentClipItems);
                //.OrderByDescending(x => x.CopyDateTime)
                //.Take(MpMeasurements.Instance.MaxRecentClipItems)
                //.Any(y => y.Id == ci.Id);
                return result.Contains(ci.Id);
            }
            return Tag.IsLinkedWithCopyItem(ci);
        }

        public bool IsLinked(MpClipTileViewModel ctvm) {
            foreach(var civm in ctvm.ItemViewModels) {
                if(IsLinked(civm)) {
                    return true;
                }
            }
            return false;
        }

        public bool IsLinked(MpContentItemViewModel rtbvm) {
            return IsLinked(rtbvm.CopyItem);
        }
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
            MpHelpers.Instance.RunOnMainThread((Action)(() => {
                if (sender is MpCopyItemTag cit) {
                    if(TagId == cit.TagId) {
                        cit.StartSync(e.SourceGuid);
                        var dupCheck = MpDb.Instance.GetItems<MpCopyItemTag>().Where(x=>x.TagId == cit.TagId && x.CopyItemId == cit.CopyItemId).FirstOrDefault();
                        if (dupCheck != null) {
                            MonkeyPaste.MpConsole.WriteTraceLine(@"Warning, copyItemTag was duplicate: " + cit.ToString());
                        }
                        cit.WriteToDatabase();
                        cit.EndSync();
                    }                    
                }
            }));
        }



        #endregion

        #endregion

        #region Commands
        private RelayCommand<object> _assignHotkeyCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if (_assignHotkeyCommand == null) {
                    _assignHotkeyCommand = new RelayCommand<object>(AssignHotkey);
                }
                return _assignHotkeyCommand;
            }
        }
        private void AssignHotkey(object args) {
            ShortcutKeyString = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(
                this, 
                "Select " + TagName, 
                ShortcutKeyString, 
                SelectTagCommand, null);
        }

        private RelayCommand<Brush> _changeColorCommand;
        public ICommand ChangeColorCommand {
            get {
                if (_changeColorCommand == null) {
                    _changeColorCommand = new RelayCommand<Brush>(ChangeColor);
                }
                return _changeColorCommand;
            }
        }
        private void ChangeColor(Brush newBrush) {
            if(newBrush != null) {
                TagColor = newBrush;
                Tag.WriteToDatabase();
            }
        }

        public ICommand CancelRenameTagCommand => new RelayCommand(
            () => {
                TagName = _originalTagName;
                IsEditing = false;
            });

        public ICommand FinishRenameTagCommand => new RelayCommand(
            () => {
                IsEditing = false;
                Tag.WriteToDatabase();
            });

        private RelayCommand _renameTagCommand;
        public ICommand RenameTagCommand {
            get {
                if (_renameTagCommand == null) {
                    _renameTagCommand = new RelayCommand(RenameTag, CanRenameTag);
                }
                return _renameTagCommand;
            }
        }
        private bool CanRenameTag() {
            return !IsTagReadOnly;
        }
        private void RenameTag() {
            _originalTagName = TagName;
            MainWindowViewModel.TagTrayViewModel.ClearTagSelection();
            IsSelected = true;
            IsEditing = true;
        }

        private RelayCommand _selectTagCommand;
        public ICommand SelectTagCommand {
            get {
                if (_selectTagCommand == null) {
                    _selectTagCommand = new RelayCommand(SelectTag);
                }
                return _selectTagCommand;
            }
        }
        private void SelectTag() {
            MainWindowViewModel.TagTrayViewModel.ClearTagSelection();
            IsSelected = true;
            //((MpClipTileViewModelPagedSourceProvider)MpClipTrayViewModel.Instance.ClipTileViewModelPaginationManager.Provider).SetTag(TagId);            
            //IsTextBoxFocused = true;
        }
        #endregion
    }
}
