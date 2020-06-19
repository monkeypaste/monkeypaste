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
        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if(_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                    if(_isSelected) {
                        TagBorderBrush = Brushes.Red;
                        TagColor = new SolidColorBrush(Tag.TagColor.Color);
                        TagTextColor = MpHelperSingleton.Instance.IsBright(Tag.TagColor.Color) ? Brushes.Black:Brushes.White;

                    } else {
                        TagBorderBrush = Brushes.White;
                        TagColor = Brushes.Black;
                        TagTextColor = Brushes.White;
                    }
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
                    //tag names cannot be blank so don't allow the textblock to reappear and change name back to 'untitled'
                    if(TagName.Trim() == string.Empty) {
                        TagName = "Untitled";
                        return;
                    }
                    _isEditing = value;
                    OnPropertyChanged("IsEditing");
                    if(_isEditing) {
                        //show textbox and select all text
                        TextBoxVisibility = Visibility.Visible;
                        TextBlockVisibility = Visibility.Collapsed;
                    } else {
                        TextBoxVisibility = Visibility.Collapsed;
                        TextBlockVisibility = Visibility.Visible;
                        Tag.WriteToDatabase();
                    }
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
                    if(!IsSelected) {
                        if(_isHovering) {
                            TagBorderBrush = Brushes.Yellow;
                        } else {
                            TagBorderBrush = Brushes.White;
                        }
                    }
                }
            }
        }

        public string TagName {
            get {
                return _tag.TagName;
            }
            set {
                _tag.TagName = value;
                OnPropertyChanged("TagName");
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

        public MpTagTileViewModel(MpTag tag) {
            DisplayName = "MpTagTileViewModel";
            Tag = tag;
        }
    }
}
