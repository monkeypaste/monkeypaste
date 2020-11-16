using GalaSoft.MvvmLight.CommandWpf;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTagTileViewModel : MpViewModelBase {
        #region Private Variables

        #endregion

        #region View Models
        private MpTagTrayViewModel _tagTrayViewModel;
        public MpTagTrayViewModel TagTrayViewModel {
            get {
                return _tagTrayViewModel;
            }
            set {
                if (_tagTrayViewModel != value) {
                    _tagTrayViewModel = value;
                    OnPropertyChanged(nameof(TagTrayViewModel));
                }
            }
        }
        #endregion

        #region Properties
        public bool IsNew {
            get {
                return Tag == null || Tag.TagId <= 0;
            }
        }

        private int _tagId = 0;
        public int TagId {
            get {
                return _tagId;
            }
            set {
                if (_tagId != value) {
                    _tagId = value;
                    OnPropertyChanged(nameof(TagId));
                }
            }
        }

        private MpShortcut _shortcut = null;
        public MpShortcut Shortcut {
            get {
                return _shortcut;
            }
            set {
                if (_shortcut != value) {
                    _shortcut = value;
                    OnPropertyChanged(nameof(Shortcut));
                }
            }
        }

        private string _shortcutKeyList = string.Empty;
        public string ShortcutKeyList {
            get {
                return _shortcutKeyList;
            }
            set {
                if(_shortcutKeyList != value) {
                    _shortcutKeyList = value;
                    OnPropertyChanged(nameof(ShortcutKeyList));
                }
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
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
                }
            }
        }

        private Brush _tagBorderBackgroundBrush = Brushes.Transparent;
        public Brush TagBorderBackgroundBrush {
            get {
                return _tagBorderBackgroundBrush;
            }
            set {
                if (_tagBorderBackgroundBrush != value) {
                    _tagBorderBackgroundBrush = value;
                    OnPropertyChanged(nameof(TagBorderBackgroundBrush));
                }
            }
        }

        private Brush _tagTextColor = Brushes.White;
        public Brush TagTextColor {
            get {
                return _tagTextColor;
            }
            set {
                if (_tagTextColor != value) {
                    _tagTextColor = value;
                    OnPropertyChanged(nameof(TagTextColor));
                }
            }
        }

        private Brush _tagCountTextColor = Brushes.White;
        public Brush TagCountTextColor {
            get {
                return _tagCountTextColor;
            }
            set {
                if (_tagCountTextColor != value) {
                    _tagCountTextColor = value;
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
                }
            }
        }

        private string _tagName;
        public string TagName {
            get {
                return _tagName;
            }
            set {
                if (_tagName != value) {
                    _tagName = value;
                    OnPropertyChanged(nameof(TagName));
                }
            }
        }

        private Brush _tagColor;
        public Brush TagColor {
            get {
                return _tagColor;
            }
            set {
                if (_tagColor != value) {
                    _tagColor = value;
                    OnPropertyChanged(nameof(TagColor));
                }
            }
        }

        private Visibility _textBoxVisibility = Visibility.Collapsed;
        public Visibility TextBoxVisibility {
            get {
                return _textBoxVisibility;
            }
            set {
                if (_textBoxVisibility != value) {
                    _textBoxVisibility = value;
                    OnPropertyChanged(nameof(TextBoxVisibility));
                }
            }
        }

        private Visibility _textBlockVisibility = Visibility.Visible;
        public Visibility TextBlockVisibility {
            get {
                return _textBlockVisibility;
            }
            set {
                if (_textBlockVisibility != value) {
                    _textBlockVisibility = value;
                    OnPropertyChanged(nameof(TextBlockVisibility));
                }
            }
        }

        private int _tagClipCount = 0;
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

        public bool IsHistory() {
            return Tag.TagId == 1;
        }
        #endregion

        #region Public Methods
        public MpTagTileViewModel(MpTag tag, MpTagTrayViewModel parent) {
            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(TagId):
                        Tag.TagId = TagId;
                        break;
                    case nameof(TagColor):
                        Tag.TagColor.Color = ((SolidColorBrush)TagColor).Color;
                        Tag.WriteToDatabase();
                        TagCountTextColor = MpHelpers.IsBright(Tag.TagColor.Color) ? Brushes.Black : Brushes.White;
                        break;
                    case nameof(IsEditing):
                        if (IsEditing) {
                            //show textbox and select all text
                            TextBoxVisibility = Visibility.Visible;
                            TextBlockVisibility = Visibility.Collapsed;
                            IsFocused = true;
                            //TagTextBox?.SelectAll();
                        } else {
                            //tag names cannot be blank so don't allow the textblock to reappear and change name back to 'untitled'
                            if (TagName.Trim() == string.Empty) {
                                TagName = "Untitled";
                                //to trigger selectall unfocus and refocus tag textbox
                                //IsTextBoxFocused = true;
                                IsEditing = true;
                                return;
                            }
                            TextBoxVisibility = Visibility.Collapsed;
                            TextBlockVisibility = Visibility.Visible;
                            Tag.TagName = TagName;
                            Tag.WriteToDatabase();
                            IsFocused = false;
                        }
                        break;
                    case nameof(IsSelected):
                        if (IsSelected) {
                            TagTextColor = Brushes.White;
                            TagBorderBackgroundBrush = Brushes.DimGray;
                        } else {
                            TagBorderBackgroundBrush = Brushes.Transparent;
                            TagTextColor = Brushes.LightGray;
                        }
                        break;
                    case nameof(IsHovering):
                        if (!IsSelected) {
                            if (IsHovering) {
                                TagBorderBackgroundBrush = Brushes.LightGray;
                                TagTextColor = Brushes.Black;
                            } else {
                                TagBorderBackgroundBrush = Brushes.Transparent;
                                TagTextColor = Brushes.White;
                            }
                        }
                        break;
                }
            };

            Tag = tag;
            TagId = Tag.TagId;
            TagTrayViewModel = parent;
            TagColor = new SolidColorBrush(Tag.TagColor.Color);
            TagName = Tag.TagName;
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
                }
            };

            var tagTextBox = (TextBox)tagBorder.FindName("TagTextBox");
            //this is called 
            tagTextBox.GotFocus += (s, e1) => {
                //TagTextBox.SelectAll();
            };
            tagTextBox.LostFocus += (s, e2) => {
                IsEditing = false;
            };
            tagTextBox.PreviewKeyDown += MainWindowViewModel.MainWindow_PreviewKeyDown;
            //if tag is created at runtime show tbox w/ all selected
            if (!MainWindowViewModel.IsLoading) {
                RenameTagCommand.Execute(null);
            } else {
                foreach (var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                    if (Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                        TagClipCount++;
                    }
                }
            }
        }

        public void AddClip(MpClipTileViewModel ctvm) {
            Tag.LinkWithCopyItem(ctvm.CopyItem);
            TagClipCount++;
        }
        #endregion

        #region Private Methods

        //public void LinkToClipTile(MpClipTileViewModel clipTileToLink) {
        //    if(!Tag.IsLinkedWithCopyItem(clipTileToLink.CopyItem)) {
        //        Tag.LinkWithCopyItem(clipTileToLink.CopyItem);
        //        TagClipCount++;
        //    }
        //}

        //public void UnlinkWithClipTile(MpClipTileViewModel clipTileToLink) {
        //    if (Tag.IsLinkedWithCopyItem(clipTileToLink.CopyItem)) {
        //        Tag.UnlinkWithCopyItem(clipTileToLink.CopyItem);
        //        TagClipCount--;
        //    }
        //}
        #endregion


        #region Commands
        private RelayCommand _assignHotkeyCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if (_assignHotkeyCommand == null) {
                    _assignHotkeyCommand = new RelayCommand(AssignHotkey);
                }
                return _assignHotkeyCommand;
            }
        }
        private void AssignHotkey() {
            MainWindowViewModel.IsShowingDialog = true;
            ShortcutKeyList = MpAssignShortcutModalWindowViewModel.ShowAssignShortcutWindow("Select " + TagName, ShortcutKeyList, SelectTagCommand);

            var sccvm = MainWindowViewModel.ShortcutCollectionViewModel;
            if (ShortcutKeyList == null) {
                //if assignment was canceled ignore but reset skl
                ShortcutKeyList = string.Empty;
            } else if (ShortcutKeyList == string.Empty) {
                //if an empty assignment was ok'd check if exists 
                var scvml = sccvm.Where(x => x.Command == SelectTagCommand).ToList();
                //if it does clear, save and unregister
                if (scvml != null && scvml.Count > 0) {
                    foreach (var scvm in scvml) {
                        scvm.ClearKeyList();
                        scvm.Shortcut.WriteToDatabase();
                        scvm.Unregister();
                    }
                } else {
                    //nothing to do since no shortcut created
                }
            } else {
                //check sc if exists 
                var scvml = sccvm.Where(x => x.Command == SelectTagCommand).ToList();
                //if it does update the keylist
                if (scvml != null && scvml.Count > 0) {
                    foreach (var scvm in scvml) {
                        scvm.KeyList = ShortcutKeyList;
                        scvm.Shortcut.WriteToDatabase();
                        scvm.Register();
                    }
                } else {
                    //add new shortcut to collection
                    sccvm.Add(
                        new MpShortcutViewModel(
                            new MpShortcut(
                                0,
                                TagId,
                                ShortcutKeyList,
                                "Select " + TagName),
                            SelectTagCommand));
                }
            }
            MainWindowViewModel.IsShowingDialog = false;
        }

        private RelayCommand _changeTagColorCommand;
        public ICommand ChangeTagColorCommand {
            get {
                if (_changeTagColorCommand == null) {
                    _changeTagColorCommand = new RelayCommand(ChangeTagColor);
                }
                return _changeTagColorCommand;
            }
        }
        private void ChangeTagColor() {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            cd.ShowHelp = true;
            cd.Color = MpHelpers.ConvertSolidColorBrushToWinFormsColor((SolidColorBrush)TagColor);
            cd.CustomColors = Properties.Settings.Default.UserCustomColorIdxArray;

            var mw = (MpMainWindow)Application.Current.MainWindow;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = true;
            // Update the text box color if the user clicks OK 
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                TagColor = MpHelpers.ConvertWinFormsColorToSolidColorBrush(cd.Color);                   
            }
            Properties.Settings.Default.UserCustomColorIdxArray = cd.CustomColors;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = false;
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
            return TagName != Properties.Settings.Default.HistoryTagTitle;
        }
        private void RenameTag() {
            TagTrayViewModel.ClearTagSelection();
            IsSelected = true;
            IsFocused = true;
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
            TagTrayViewModel.ClearTagSelection();
            IsSelected = true;
            IsFocused = true;
        }

        #endregion
    }
}
