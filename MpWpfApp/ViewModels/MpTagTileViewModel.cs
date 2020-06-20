using MpWinFormsClassLibrary;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TextBox = System.Windows.Controls.TextBox;

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
                    OnPropertyChanged("IsSelected");                    
                }
            }
        }

        private bool _isTextBoxFocused = false;
        public bool IsTextBoxFocused {
            get {
                return _isTextBoxFocused;
            }
            set {
                if(_isTextBoxFocused != value) {
                    _isTextBoxFocused = value;
                    OnPropertyChanged("IsTextBoxFocused");
                }
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
                    OnPropertyChanged("IsEditing");
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
                    OnPropertyChanged("IsHovering");
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
                    OnPropertyChanged("TagBorderBrush");
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
                    OnPropertyChanged("TagTextColor");
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
                    OnPropertyChanged("TextBoxVisibility");
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
                    OnPropertyChanged("TextBlockVisibility");
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
        #endregion

        #region Constructor
        public MpTagTileViewModel(MpTag tag) {
            DisplayName = "MpTagTileViewModel";
            Tag = tag;
            PropertyChanged += (s, e) => {
                if(e.PropertyName == "IsEditing") {
                    if(IsEditing) {
                        //show textbox and select all text
                        TextBoxVisibility = Visibility.Visible;
                        TextBlockVisibility = Visibility.Collapsed;
                        IsTextBoxFocused = false;
                        IsTextBoxFocused = true;
                    } else {
                        //tag names cannot be blank so don't allow the textblock to reappear and change name back to 'untitled'
                        if(TagName.Trim() == string.Empty) {
                            TagName = "Untitled";
                            //to trigger selectall unfocus and refocus tag textbox
                            IsTextBoxFocused = false;
                            IsTextBoxFocused = true;
                            IsEditing = true;
                            return;
                        }
                        TextBoxVisibility = Visibility.Collapsed;
                        TextBlockVisibility = Visibility.Visible;
                        Tag.WriteToDatabase();
                        IsTextBoxFocused = false;
                    }
                } else if(e.PropertyName == "IsSelected") {
                    if(IsSelected) {
                        TagBorderBrush = Brushes.Red;
                        TagColor = new SolidColorBrush(Tag.TagColor.Color);
                        TagTextColor = MpHelperSingleton.Instance.IsBright(Tag.TagColor.Color) ? Brushes.Black : Brushes.White;

                    } else {
                        TagBorderBrush = Brushes.White;
                        TagColor = Brushes.Black;
                        TagTextColor = Brushes.White;
                    }
                } else if(e.PropertyName == "IsHovering") {
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
        private DelegateCommand<KeyEventArgs> _keyDownCommand;
        public ICommand KeyDownCommand {
            get {
                if(_keyDownCommand == null) {
                    _keyDownCommand = new DelegateCommand<KeyEventArgs>(KeyDown);
                }
                return _keyDownCommand;
            }
        }
        private void KeyDown(KeyEventArgs e) {
            Key key = e.Key;
            if(key == Key.Delete || key == Key.Back && DeleteTagCommand.CanExecute(null)) {
                DeleteTagCommand.Execute(null);
            } else if(key == Key.Enter && IsEditing) {
                IsEditing = false;
            }
        }

        private DelegateCommand _deleteTagCommand;
        public ICommand DeleteTagCommand {
            get {
                if(_deleteTagCommand == null) {
                    _deleteTagCommand = new DelegateCommand(DeleteTag, CanDeleteTag);
                }
                return _deleteTagCommand;
            }
        }
        private bool CanDeleteTag() {
            //allow delete if any tag besides history tag is selected, delete method will ignore history
            var selectedTagTiles = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).TagTiles.Where(ct => ct.IsSelected).ToList();
            if(selectedTagTiles.Count == 1 && selectedTagTiles[0].TagName == "History") {
                return false;
            }
            return selectedTagTiles.Count > 0;
        }
        private void DeleteTag() {
            //this removal triggers mainwindowviewmodel to delete the tagtile            
            var selectedTagTiles = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).TagTiles.Where(ct => ct.IsSelected).ToList();
            foreach(var ct in selectedTagTiles) {
                //do not delete history tag
                if(ct.Tag.TagName == "History") {
                    continue;
                }
                MpDataStore.Instance.TagList.Remove(ct.Tag);
                ct.Tag.DeleteFromDatabase();
            }
        }

        private DelegateCommand _renameTagCommand;
        public ICommand RenameTagCommand {
            get {
                if(_renameTagCommand == null) {
                    _renameTagCommand = new DelegateCommand(RenameTag,CanRenameTag);
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

        private DelegateCommand _linkTagToCopyItemCommand;
        public DelegateCommand LinkTagToCopyItemCommand {
            get {
                if(_linkTagToCopyItemCommand == null) {
                    _linkTagToCopyItemCommand = new DelegateCommand(LinkTagToCopyItem);
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
