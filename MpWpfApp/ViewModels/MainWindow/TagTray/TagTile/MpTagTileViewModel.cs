using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpTagTileViewModel : MpViewModelBase {
        #region Private Variables
        private int _tagClipCount = 0;
        private string _originalTagName = string.Empty;
        private bool _wasEditingName = false;
        #endregion

        #region Properties

        #region View Models
        public List<MpClipTileViewModel> LinkedClipTiles {
            get {
                if(MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null || Tag == null) {
                    return new List<MpClipTileViewModel>();
                }
                return MainWindowViewModel.ClipTrayViewModel.ClipTileViewModels.Where(x => IsLinkedWithClipTile(x)).ToList();
            }
        }
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
                return Tag.Id == 1 || Tag.Id == 2 || Tag.Id == 4;
            }
        }

        public bool IsRecentTag {
            get {
                if(Tag == null) {
                    return false;
                }
                return Tag.Id == 2;
            }
        }

        public bool IsAllTag {
            get {
                if (Tag == null) {
                    return false;
                }
                return Tag.Id == 1;
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                MpHelpers.Instance.RunOnMainThread((Action)(() => {
                    if (_isSelected != value || MainWindowViewModel.ClipTrayViewModel.IsFilteringByApp) {
                        _isSelected = value;
                        OnPropertyChanged(nameof(IsSelected));
                        OnPropertyChanged(nameof(TagBorderBackgroundBrush));
                        OnPropertyChanged(nameof(TagTextColor));
                    }
                }));
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

                    OnPropertyChanged(nameof(IsEditing));
                    OnPropertyChanged(nameof(TextBlockVisibility));
                    OnPropertyChanged(nameof(TextBoxVisibility));
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
                    return Brushes.Red;
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
                return MpHelpers.Instance.IsBright(((SolidColorBrush)Color).Color) ? Brushes.Black : Brushes.White; ;
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
                return Tag.Id;
            }
            set {
                if (Tag.Id != value) {
                    Tag.Id = value;
                    Tag.WriteToDatabase();
                    OnPropertyChanged(nameof(TagId));
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
                    OnPropertyChanged(nameof(TagSortIdx));
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
                    OnPropertyChanged(nameof(TagName));
                }
            }
        }

        public Brush Color {
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
                    OnPropertyChanged(nameof(Color));
                    OnPropertyChanged(nameof(TagCountTextColor));
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
                    OnPropertyChanged(nameof(Color));
                    OnPropertyChanged(nameof(TagName));
                    OnPropertyChanged(nameof(TagId));
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        
        #endregion

        #endregion

        #region Public Methods
        public MpTagTileViewModel(MpTag tag) : base() {
            MonkeyPaste.MpDb.Instance.SyncAdd += MpDbObject_SyncAdd;
            MonkeyPaste.MpDb.Instance.SyncUpdate += MpDbObject_SyncUpdate;
            MonkeyPaste.MpDb.Instance.SyncDelete += MpDbObject_SyncDelete;

            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(IsEditing):
                        if(IsEditing) {
                            _wasEditingName = true;
                            _originalTagName = TagName;
                        } else {
                            if(_wasEditingName) {
                                _wasEditingName = false;
                                if(TagName != _originalTagName) {
                                    Tag.WriteToDatabase();
                                }
                            }
                        }
                        break;
                    case nameof(Tag):
                        break;
                }
            };

            Tag = tag;
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
            //if tag is created at runtime show tbox w/ all selected
            if (IsNew) {
                RenameTagCommand.Execute(null);
            }

            OnViewModelLoaded();
        }

        public void TagTile_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)sender;
            cm.DataContext = this;
            MenuItem cmi = null;
            foreach (var mi in cm.Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if ((mi as MenuItem).Name == "ClipTileColorContextMenuItem") {
                    cmi = (MenuItem)mi;
                    break;
                }
            }
            MpHelpers.Instance.SetColorChooserMenuItem(
                    cm,
                    cmi,
                    (s, e1) => {
                        ChangeColorCommand.Execute((Brush)((Border)s).Tag);
                    },
                    MpHelpers.Instance.GetColorColumn(Color),
                    MpHelpers.Instance.GetColorRow(Color)
                );
        }

        public void TagTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            var cm = sender as ContextMenu;
            var ttvm = (MpTagTileViewModel)cm.DataContext;
            cm.Tag = ttvm;
            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(cm);
        }

        public void TagTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {
        }

        public void AddClip(MpClipTileViewModel ctvm) {
            Tag.LinkWithCopyItem(ctvm.CopyItem);    
        }

        public void AddClip(MpRtbListBoxItemRichTextBoxViewModel rtbvm) {
            Tag.LinkWithCopyItem(rtbvm.CopyItem);
        }

        public void RemoveClip(MpClipTileViewModel ctvm) {
            Tag.UnlinkWithCopyItem(ctvm.CopyItem);
        }

        public void RemoveClip(MpRtbListBoxItemRichTextBoxViewModel rtbvm) {
            Tag.UnlinkWithCopyItem(rtbvm.CopyItem);
        }

        public bool IsLinkedWithClipTile(MpClipTileViewModel ctvm) {
            if (ctvm == null || 
                ctvm.CopyItem == null || 
                ctvm.CopyItemId == 0 || 
                Tag == null || 
                Tag.Id == 0) {
                return false;
            }
            if(IsAllTag) {
                return true;
            }
            if(IsRecentTag) {
                return MainWindowViewModel.ClipTrayViewModel.ClipTileViewModels.
                    OrderByDescending(x => x.CopyDateTime).
                    Take(Properties.Settings.Default.MaxRecentClipItems).
                    Contains(ctvm);
            }
            return Tag.IsLinkedWithCopyItem(ctvm.CopyItem);
        }

        public bool IsLinkedWithRtbItem(MpRtbListBoxItemRichTextBoxViewModel rtbvm) {
            if (rtbvm == null ||
                rtbvm.CopyItem == null ||
                rtbvm.CopyItemId == 0 ||
                Tag == null ||
                Tag.Id == 0) {
                return false;
            }
            if (IsAllTag) {
                return true;
            }
            if (IsRecentTag) {
                return MainWindowViewModel.ClipTrayViewModel.ClipTileViewModels.
                    OrderByDescending(x => x.CopyDateTime).
                    Take(Properties.Settings.Default.MaxRecentClipItems).
                    Contains(rtbvm.HostClipTileViewModel);
            }
            return Tag.IsLinkedWithCopyItem(rtbvm.CopyItem);
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
                Color = newBrush;
                Tag.WriteToDatabase();
            }
        }

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
            //((MpClipTileViewModelPagedSourceProvider)MainWindowViewModel.ClipTrayViewModel.ClipTileViewModelPaginationManager.Provider).SetTag(TagId);            
            //IsTextBoxFocused = true;
        }
        #endregion
    }
}
