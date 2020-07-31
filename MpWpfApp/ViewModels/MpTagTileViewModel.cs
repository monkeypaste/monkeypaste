using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTagTileViewModel : MpViewModelBase {
        #region Private Variables

        private bool _isNew;

        #endregion
        #region Apperance Properties
        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if(_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));                    
                }
            }
        }

        private bool _isTextBoxFocused = false;
        public bool IsTextBoxFocused {
            get {
                return _isTextBoxFocused;
            }
            set {
                _isTextBoxFocused = value;
                OnPropertyChanged(nameof(IsTextBoxFocused));
            }
        }

        private bool _isEditing = false;
        public bool IsEditing {
            get {
                return _isEditing;
            }
            set {
                if(_isEditing != value) {                    
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
                if(_isHovering != value) {
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
                if(_tagBorderBackgroundBrush != value) {
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
                if(_tagTextColor != value) {
                    _tagTextColor = value;
                    OnPropertyChanged(nameof(TagTextColor));
                }
            }
        }
        #endregion

        #region Layout Properties
        private Visibility _textBoxVisibility = Visibility.Collapsed;
        public Visibility TextBoxVisibility {
            get {
                return _textBoxVisibility;
            }
            set {
                if(_textBoxVisibility != value) {
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
                if(_textBlockVisibility != value) {
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
                if(_tagClipCount != value) {
                    _tagClipCount = value;
                    OnPropertyChanged(nameof(TagClipCount));
                }
            }
        }
        public double TagHeight {
            get {
                return MpMeasurements.Instance.FilterMenuHeight;

            }
        }

        public double TagFontSize {
            get {
                return TagHeight * 0.75;
            }
        }
        #endregion

        #region Business Logic Properties
        public bool IsHistory() {
            return Tag.TagId == 1;
        }
        #endregion

        #region Exposed Model Properties
        private MpTag _tag;
        public MpTag Tag {
            get {
                return _tag;
            }
            set {
                if(_tag != value) {
                    _tag = value;
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        public string TagName {
            get {
                return Tag.TagName;
            }
            set {
                Tag.TagName = value;
                OnPropertyChanged(nameof(TagName));
            }
        }

        private Brush _tagColor;
        public Brush TagColor {
            get {
                return _tagColor;
            }
            set {
                if(_tagColor != value) {
                    _tagColor = value;
                    OnPropertyChanged(nameof(TagColor));
                }
            }
        }

        private MpMainWindowViewModel _mainWindowViewModel;
        public MpMainWindowViewModel MainWindowViewModel {
            get {
                return _mainWindowViewModel;
            }
            set {
                if (_mainWindowViewModel != value) {
                    _mainWindowViewModel = value;
                    OnPropertyChanged(nameof(MainWindowViewModel));
                }
            }
        }
        #endregion

        #region Constructor
        public MpTagTileViewModel(MpTag tag,MpMainWindowViewModel mainWindowViewModel,bool isNew) {
            DisplayName = "MpTagTileViewModel";
            Tag = tag;
            MainWindowViewModel = mainWindowViewModel;
            _isNew = isNew;
        }
        #endregion

        #region Private Methods

        public void LinkToClipTile(MpClipTileViewModel clipTileToLink) {
            if(!Tag.IsLinkedWithCopyItem(clipTileToLink.CopyItem)) {
                Tag.LinkWithCopyItem(clipTileToLink.CopyItem);
                TagClipCount++;
            }
        }

        public void UnlinkWithClipTile(MpClipTileViewModel clipTileToLink) {
            if (Tag.IsLinkedWithCopyItem(clipTileToLink.CopyItem)) {
                Tag.UnlinkWithCopyItem(clipTileToLink.CopyItem);
                TagClipCount--;
            }
        }
        #endregion

        #region View Event Handlers
        public TextBox TagTextBox;
        public void TagTile_Loaded(object sender, RoutedEventArgs e) {
            PropertyChanged += (s, e1) => {
                switch(e1.PropertyName) {
                    case nameof(IsEditing):
                        if (IsEditing) {
                            //show textbox and select all text
                            TextBoxVisibility = Visibility.Visible;
                            TextBlockVisibility = Visibility.Collapsed;
                            IsTextBoxFocused = true;
                            TagTextBox?.SelectAll();
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
                            Tag.WriteToDatabase();
                            IsTextBoxFocused = false;
                        }
                        break;
                    case nameof(IsSelected):
                        if (IsSelected) {
                            //TagBorderBackgroundBrush = Brushes.Red;
                            //TagColor = new SolidColorBrush(Tag.TagColor.Color);
                            TagTextColor = Brushes.White;  

                        } else {
                            //TagBorderBackgroundBrush = Brushes.White;
                            //TagColor = Brushes.Black;
                            TagTextColor = Brushes.LightGray;
                        }
                        break;
                    case nameof(IsHovering):
                        if (!IsSelected) {
                            if (IsHovering) {
                                TagBorderBackgroundBrush = Brushes.DimGray;

                            } else {
                                TagBorderBackgroundBrush = Brushes.Transparent;
                            }
                        }
                        break;
                }
            };

            TagColor = new SolidColorBrush(Tag.TagColor.Color);

            var tagBorder = ((Border)sender);
            tagBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            tagBorder.MouseLeave += (s, e1) => {
                IsHovering = false;
            };

            TagTextBox = (TextBox)tagBorder.FindName("TagTextBox");
            //this is called 
            TagTextBox.GotFocus += (s, e1) => {
                //TagTextBox.SelectAll();
            };
            TagTextBox.LostFocus += (s, e2) => {
                IsEditing = false;
            };
            TagTextBox.PreviewKeyDown += (s, e3) => {
                if (e3.Key == Key.Enter && IsEditing) {
                    IsEditing = false;
                }
            };
            if (_isNew) {
                IsEditing = true;
            }
        }
        #endregion

        #region Commands

        private RelayCommand _renameTagCommand;
        public ICommand RenameTagCommand {
            get {
                if(_renameTagCommand == null) {
                    _renameTagCommand = new RelayCommand(RenameTag,CanRenameTag);
                }
                return _renameTagCommand;
            }
        }
        private bool CanRenameTag() {
            return TagName != Properties.Settings.Default.HistoryTagTitle;
        }
        private void RenameTag() {
            IsEditing = true;
        }

        #endregion
    }
}
