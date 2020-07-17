using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTagTileViewModel : MpViewModelBase {
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

        private Brush _tagBorderBrush = Brushes.White;
        public Brush TagBorderBrush {
            get {
                return _tagBorderBrush;
            }
            set {
                if(_tagBorderBrush != value) {
                    _tagBorderBrush = value;
                    OnPropertyChanged(nameof(TagBorderBrush));
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

        public double TagHeight {
            get {
                return MpMeasurements.Instance.FilterMenuHeight * 0.85;

            }
        }

        public double TagFontSize {
            get {
                return TagHeight / 1.5;
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
                    OnPropertyChanged("Tag");
                }
            }
        }

        public string TagName {
            get {
                return Tag.TagName;
            }
            set {
                Tag.TagName = value;
                OnPropertyChanged("TagName");
            }
        }

        private Brush _tagColor = Brushes.Black;
        public Brush TagColor {
            get {
                return _tagColor;
            }
            set {
                if(_tagColor != value) {
                    _tagColor = value;
                    OnPropertyChanged("TagColor");
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
        private bool _isNew;
        public MpTagTileViewModel(MpTag tag,MpMainWindowViewModel mainWindowViewModel,bool isNew) {
            DisplayName = "MpTagTileViewModel";
            Tag = tag;
            MainWindowViewModel = mainWindowViewModel;
            _isNew = isNew;
            PropertyChanged += (s, e) => {
                if(e.PropertyName == nameof(IsEditing)) {
                    if(IsEditing) {
                        //show textbox and select all text
                        TextBoxVisibility = Visibility.Visible;
                        TextBlockVisibility = Visibility.Collapsed;
                        IsTextBoxFocused = true;
                        TagTextBox?.SelectAll();
                    } else {
                        //tag names cannot be blank so don't allow the textblock to reappear and change name back to 'untitled'
                        if(TagName.Trim() == string.Empty) {
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
                } else if(e.PropertyName == nameof(IsSelected)) {
                    if(IsSelected) {
                        TagBorderBrush = Brushes.Red;
                        TagColor = new SolidColorBrush(Tag.TagColor.Color);
                        TagTextColor = MpHelpers.IsBright(Tag.TagColor.Color) ? Brushes.Black : Brushes.White;

                    } else {
                        TagBorderBrush = Brushes.White;
                        TagColor = Brushes.Black;
                        TagTextColor = Brushes.White;
                    }
                } else if(e.PropertyName == nameof(IsHovering)) {
                    if(!IsSelected) {
                        if(IsHovering) {
                            TagBorderBrush = Brushes.Yellow;
                        } else {
                            TagBorderBrush = Brushes.White;
                        }
                    }
                }
            };
        }
        #endregion

        #region View Event Handlers
        public TextBox TagTextBox;
        public void TagLoaded(object sender, RoutedEventArgs e) {
            Console.WriteLine("Loaded");
            TagTextBox = (TextBox)((Border)sender).FindName("TagTextBox");
            //this is called 
            TagTextBox.GotFocus += (s, e1) => {
                //TagTextBox.SelectAll();
            };

            if (_isNew) {
                IsEditing = true;
            }
        }
        public void Test(object sender) {
            IsHovering = true;
        }
        public void MouseEnter() {
            IsHovering = true;
        }

        public void MouseLeave() {
            IsHovering = false;
        }

        public void LostFocus() {
            //occurs when editing title or content text
            IsEditing = false;
        }
        #endregion

        #region Commands
        private RelayCommand<KeyEventArgs> _keyDownCommand;
        public ICommand KeyDownCommand {
            get {
                if(_keyDownCommand == null) {
                    _keyDownCommand = new RelayCommand<KeyEventArgs>(KeyDown);
                }
                return _keyDownCommand;
            }
        }
        private void KeyDown(KeyEventArgs e) {
            Key key = e.Key;
            if(key == Key.Delete || key == Key.Back && MainWindowViewModel.DeleteTagCommand.CanExecute(null)) {
                MainWindowViewModel.DeleteTagCommand.Execute(null);
            } else if(key == Key.Enter && IsEditing) {
                IsEditing = false;
            }
        }

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
            return TagName != "History";
        }
        private void RenameTag() {
            IsEditing = true;
        }

        private RelayCommand _linkTagToCopyItemCommand;
        public RelayCommand LinkTagToCopyItemCommand {
            get {
                if(_linkTagToCopyItemCommand == null) {
                    _linkTagToCopyItemCommand = new RelayCommand(LinkTagToCopyItem);
                }
                return _linkTagToCopyItemCommand;
            }
        }
        private void LinkTagToCopyItem() {
            //tags and clips have 1-to-1 relationship so remove all other links if it exists before creating new one
            //so loop through all selected clips and sub-loop through all tags and remove links if found
            var clipsToLink = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).ClipTiles.Where(ct => ct.IsSelected).ToList();
            foreach(var clipToRemoveOldLink in clipsToLink) {
                foreach(var tagTile in ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).TagTiles) {
                    if(tagTile.Tag.IsLinkedWithCopyItem(clipToRemoveOldLink.CopyItem) && tagTile.TagName != "History") {
                        tagTile.Tag.UnlinkWithCopyItem(clipToRemoveOldLink.CopyItem);
                    }
                }
            }
            //now loop over all selected clips and link to this tag
            foreach (var clipToLink in clipsToLink) {
                Tag.LinkWithCopyItem(clipToLink.CopyItem);
                clipToLink.TitleColor = new SolidColorBrush(Tag.TagColor.Color);
            }
        }
        #endregion
    }
}
