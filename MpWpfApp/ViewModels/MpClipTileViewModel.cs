using MpWinFormsClassLibrary;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
   public class MpClipTileViewModel  : MpViewModelBase {
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
                        ClipBorderBrush = Brushes.Red;
                    } else {
                        ClipBorderBrush = Brushes.Transparent;
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
                            ClipBorderBrush = Brushes.Yellow;
                        } else {
                            ClipBorderBrush = Brushes.Transparent;
                        }
                    }
                }
            }
        }

        private Visibility _visibility = Visibility.Visible;
        public Visibility Visibility {
            get {
                return _visibility;
            }
            set {
                if(_visibility != value) {
                    _visibility = value;
                    OnPropertyChanged("Visibility");
                }
            }
        }

        private MpClip _copyItem;
        public MpClip CopyItem {
            get {
                return _copyItem;
            }
            set {
                if(_copyItem != value) {
                    _copyItem = value;
                    OnPropertyChanged("CopyItem");
                }
            }
        }

        private Brush _clipBorderBrush = Brushes.Transparent;
        public Brush ClipBorderBrush {
            get {
                return _clipBorderBrush;
            }
            set {
                if(_clipBorderBrush != value) {
                    _clipBorderBrush = value;
                    OnPropertyChanged("ClipBorderBrush");
                }
            }
        }

        private double _tileSize = MpMeasurements.Instance.TileSize;
        public double TileSize {
            get {
                return _tileSize;
            }
            set {
                if(_tileSize != value) {
                    _tileSize = value;
                    OnPropertyChanged("TileSize");
                }
            }
        }

        private bool _isEditingTitle = false;
        public bool IsEditingTitle {
            get {
                return _isEditingTitle;
            }
            set {
                if(_isEditingTitle != value) {
                    //tag names cannot be blank so don't allow the textblock to reappear and change name back to 'untitled'
                    if(CopyItem.Title.Trim() == string.Empty) {
                        Title = "Untitled";
                        return;
                    }
                    _isEditingTitle = value;
                    OnPropertyChanged("IsEditingTitle");
                    if(_isEditingTitle) {
                        //show textbox and select all text
                        TextBoxVisibility = Visibility.Visible;
                        TextBlockVisibility = Visibility.Collapsed;
                    } else {
                        TextBoxVisibility = Visibility.Collapsed;
                        TextBlockVisibility = Visibility.Visible;
                        CopyItem.WriteToDatabase();
                    }
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

        private double _tileTitleHeight = MpMeasurements.Instance.TileTitleHeight;
        public double TileTitleHeight {
            get {
                return _tileTitleHeight;
            }
            set {
                if(_tileTitleHeight != value) {
                    _tileTitleHeight = value;
                    OnPropertyChanged("TileTitleHeight");
                }
            }
        }

        private double _tileContentHeight = MpMeasurements.Instance.TileContentHeight;
        public double TileContentHeight {
            get {
                return _tileContentHeight;
            }
            set {
                if(_tileTitleHeight != value) {
                    _tileTitleHeight = value;
                    OnPropertyChanged("TileTitleHeight");
                }
            }
        }

        private double _tileMargin = MpMeasurements.Instance.TileMargin;
        public double TileMargin {
            get {
                return _tileMargin;
            }
            set {
                if(_tileMargin != value) {
                    _tileMargin = value;
                    OnPropertyChanged("TileMargin");
                }
            }
        }
        public string Title {
            get {
                return CopyItem.Title;
            } 
            set {
                if(CopyItem.Title != value) {
                    CopyItem.Title = value;
                    OnPropertyChanged("Title");
                }
            }
        }
        public Brush TitleTextColor {
            get {
                return Brushes.White;
            }
        }

        public Color TitleTextShadowColor {
            get {
                return Colors.Black;
            }
        }

        public Brush TitleColor {
            get {
                return new SolidColorBrush(CopyItem.ItemColor.Color);
            }
            set {
                CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                CopyItem.ItemColor.WriteToDatabase();
                CopyItem.ColorId = CopyItem.ItemColor.ColorId;
                OnPropertyChanged("TitleColor");
            }
        }

        public string Text {
            get {
                return CopyItem.Text;
            }
            set {
                CopyItem.SetData(value);
                OnPropertyChanged("Text");
            }
        }

        public ImageSource Icon {
            get {
                return CopyItem.App.Icon.IconImage;
            }
        }

        //private ObservableCollection<MpClipTileTagMenuItemViewModel> _tagMenuItems = new ObservableCollection<MpClipTileTagMenuItemViewModel>();
        public ObservableCollection<MpClipTileTagMenuItemViewModel> TagMenuItems {
            get {
                ObservableCollection<MpClipTileTagMenuItemViewModel> tagMenuItems = new ObservableCollection<MpClipTileTagMenuItemViewModel>();
                var tagTiles = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).TagTiles;
                foreach(var tagTile in tagTiles) {
                    if(tagTile.TagName == "History") {
                        continue;
                    }
                    tagMenuItems.Add(new MpClipTileTagMenuItemViewModel(tagTile.TagName, tagTile.LinkTagToCopyItemCommand,tagTile.Tag.IsLinkedWithCopyItem(CopyItem)));
                }
                return tagMenuItems;
            }
        }

        public MpClipTileViewModel(MpClip ci) {
            CopyItem = ci;
        }

        private DelegateCommand _deleteClipCommand;
        public ICommand DeleteClipCommand {
            get {
                if(_deleteClipCommand == null) {
                    _deleteClipCommand = new DelegateCommand(DeleteClip);
                }
                return _deleteClipCommand;
            }
        }
        private void DeleteClip() {
            //this removal triggers mainwindowviewmodel to delete ALL SELECTED the cliptiles
            var selectedClipTiles = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).ClipTiles.Where(ct => ct.IsSelected).ToList();
            foreach(var ct in selectedClipTiles) {
                ct.CopyItem.DeleteFromDatabase();
                MpDataStore.Instance.ClipList.Remove(ct.CopyItem);
            }
        }

        private DelegateCommand _renameClipCommand;
        public ICommand RenameClipCommand {
            get {
                if(_renameClipCommand == null) {
                    _renameClipCommand = new DelegateCommand(RenameClip);
                }
                return _renameClipCommand;
            }
        }
        private void RenameClip() {
            IsEditingTitle = true;
        }

        public override string ToString() {
            return CopyItem.ToString();
        }
    }
}
