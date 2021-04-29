using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTagTileViewModel : MpViewModelBase {
        #region Private Variables
        private int _tagClipCount = 0;
        private string _originalTagName = string.Empty;
        #endregion

        #region Properties

        #region View Models
        public List<MpClipTileViewModel> LinkedClipTiles {
            get {
                if(MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null || Tag == null) {
                    return new List<MpClipTileViewModel>();
                }
                return MainWindowViewModel.ClipTrayViewModel.Where(x => IsLinkedWithClipTile(x)).ToList();
            }
        }
        #endregion

        #region State
        public bool IsNew {
            get {
                return Tag == null || Tag.TagId <= 0;
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
                return Tag.TagId == 1 || Tag.TagId == 2 || Tag.TagId == 4;
            }
        }

        public bool IsRecentTag {
            get {
                if(Tag == null) {
                    return false;
                }
                return Tag.TagId == 2;
            }
        }

        public bool IsAllTag {
            get {
                if (Tag == null) {
                    return false;
                }
                return Tag.TagId == 1;
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value || MainWindowViewModel.ClipTrayViewModel.IsFilteringByApp)                    {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(TagBorderBackgroundBrush));
                    OnPropertyChanged(nameof(TagTextColor));
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
                return Tag.TagId;
            }
            set {
                if (Tag.TagId != value) {
                    Tag.TagId = value;
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
                    Tag.WriteToDatabase();
                    OnPropertyChanged(nameof(TagName));
                }
            }
        }

        public Brush TagColor {
            get {
                return new SolidColorBrush(Tag.TagColor.Color);
            }
            set {
                if (new SolidColorBrush(Tag.TagColor.Color) != value) {
                    Tag.TagColor.Color = ((SolidColorBrush)value).Color;
                    Tag.TagColor.WriteToDatabase();
                    OnPropertyChanged(nameof(TagColor));
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
                    OnPropertyChanged(nameof(Tag));
                    OnPropertyChanged(nameof(TagColor));
                    OnPropertyChanged(nameof(TagName));
                    OnPropertyChanged(nameof(TagId));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpTagTileViewModel(MpTag tag) : base() {
            //PropertyChanged += (s, e1) => {
            //    switch (e1.PropertyName) {
            //    }
            //};

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
                        ChangeTagColorCommand.Execute((Brush)((Border)s).Tag);
                    },
                    MpHelpers.Instance.GetColorColumn(TagColor),
                    MpHelpers.Instance.GetColorRow(TagColor)
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
                Tag.TagId == 0) {
                return false;
            }
            if(IsAllTag) {
                return true;
            }
            if(IsRecentTag) {
                return MainWindowViewModel.ClipTrayViewModel.
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
                Tag.TagId == 0) {
                return false;
            }
            if (IsAllTag) {
                return true;
            }
            if (IsRecentTag) {
                return MainWindowViewModel.ClipTrayViewModel.
                    OrderByDescending(x => x.CopyDateTime).
                    Take(Properties.Settings.Default.MaxRecentClipItems).
                    Contains(rtbvm.HostClipTileViewModel);
            }
            return Tag.IsLinkedWithCopyItem(rtbvm.CopyItem);
        }
        #endregion

        #region Private Methods
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

        private RelayCommand<Brush> _changeTagColorCommand;
        public ICommand ChangeTagColorCommand {
            get {
                if (_changeTagColorCommand == null) {
                    _changeTagColorCommand = new RelayCommand<Brush>(ChangeTagColor);
                }
                return _changeTagColorCommand;
            }
        }
        private void ChangeTagColor(Brush newBrush) {
            if(newBrush != null) {
                TagColor = newBrush;
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
