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
                //return Tag.TagName;
                return _tagName;
            }
            set {
                //Tag.TagName = value;
                if(_tagName != value) {
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
                //assumes Tag Margin is 5
                return MpMeasurements.Instance.FilterMenuHeight - (5*2);

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

        #region Constructor/Initializers
        public MpTagTileViewModel(MpTag tag, MpTagTrayViewModel parent, bool isNew) {
            Tag = tag;
            TagTrayViewModel = parent; 
            TagColor = new SolidColorBrush(Tag.TagColor.Color);
            TagCountTextColor = MpHelpers.IsBright(Tag.TagColor.Color) ? Brushes.Black : Brushes.White;
            TagName = Tag.TagName;
            _isNew = isNew;

            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
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
        }

        public void TagTile_Loaded(object sender, RoutedEventArgs e) {
            
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
                RenameTagCommand.Execute(null);
            }
        }
        #endregion
        #region View Event Handlers
        public TextBox TagTextBox;
        
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
            TagTrayViewModel.ClearTagSelection();
            IsSelected = true;
            IsFocused = true;
            IsEditing = true;
        }

        #endregion
    }
}
