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
   public class  MpClipTileViewModel : MpViewModelBase {
        public ObservableCollection<MpClipTileTagMenuItemViewModel> TagMenuItems {
            get {
                ObservableCollection<MpClipTileTagMenuItemViewModel> tagMenuItems = new ObservableCollection<MpClipTileTagMenuItemViewModel>();
                var tagTiles = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).TagTiles;
                foreach(var tagTile in tagTiles) {
                    if(tagTile.TagName == "History") {
                        continue;
                    }
                    tagMenuItems.Add(new MpClipTileTagMenuItemViewModel(tagTile.TagName, tagTile.LinkTagToCopyItemCommand, tagTile.Tag.IsLinkedWithCopyItem(CopyItem)));
                }
                return tagMenuItems;
            }
        }

        #region Appearance Properties
        private bool _isTitleTextBoxFocused = false;
        public bool IsTitleTextBoxFocused {
            get {
                return _isTitleTextBoxFocused;
            }
            set {
                if(_isTitleTextBoxFocused != value) {
                    _isTitleTextBoxFocused = value;
                    OnPropertyChanged("IsTitleTextBoxFocused");
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
                            BorderBrush = Brushes.Yellow;
                        } else {
                            BorderBrush = Brushes.Transparent;
                        }
                    }
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
                    //if(CopyItem.Title.Trim() == string.Empty) {
                    //    Title = "Untitled";
                    //    return;
                    //}
                    _isEditingTitle = value;
                    OnPropertyChanged("IsEditingTitle");
                }
            }
        }
        
        private Brush _borderBrush = Brushes.Transparent;
        public Brush BorderBrush {
            get {
                return _borderBrush;
            }
            set {
                if(_borderBrush != value) {
                    _borderBrush = value;
                    OnPropertyChanged("BorderBrush");
                }
            }
        }
        #endregion

        #region Layout 
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

        private double _tileSize = MpMeasurements.Instance.ClipTileSize;
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

        private double _tileBorderSize = MpMeasurements.Instance.ClipTileBorderSize;
        public double TileBorderSize {
            get {
                return _tileBorderSize;
            }
            set {
                if(_tileBorderSize != value) {
                    _tileBorderSize = value;
                    OnPropertyChanged("TileBorderSize");
                }
            }
        }

        private double _tileTitleHeight = MpMeasurements.Instance.ClipTileTitleHeight;
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

        private double _tileMargin = MpMeasurements.Instance.ClipTileMargin;
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

        private double _tileDropShadowRadius = MpMeasurements.Instance.ClipTileDropShadowRadius;
        public double TileDropShadowRadius {
            get {
                return _tileDropShadowRadius;
            }
            set {
                if(_tileDropShadowRadius != value) {
                    _tileDropShadowRadius = value;
                    OnPropertyChanged("TileDropShadowRadius");
                }
            }
        }
        #endregion

        #region Model Properties
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
        #endregion

        

        #region Constructor
        public MpClipTileViewModel(MpClip ci) {
            CopyItem = ci;
            PropertyChanged += (s, e) => {
                if(e.PropertyName == "IsSelected") {
                    if(IsSelected) {
                        BorderBrush = Brushes.Red;
                    } else {
                        BorderBrush = Brushes.Transparent;
                    }
                } else if(e.PropertyName == "IsEditingTitle") {
                    if(IsEditingTitle) {
                        //show textbox and select all text
                        TextBoxVisibility = Visibility.Visible;
                        TextBlockVisibility = Visibility.Collapsed;
                        IsTitleTextBoxFocused = false;
                        IsTitleTextBoxFocused = true;
                    } else {
                        TextBoxVisibility = Visibility.Collapsed;
                        TextBlockVisibility = Visibility.Visible;
                        IsTitleTextBoxFocused = false;
                        CopyItem.WriteToDatabase();
                    }
                }
            };
        }
        #endregion

        #region View Events Handlers
        public void MouseEnter() {
            IsHovering = true;
        }

        public void MouseLeave() {
            IsHovering = false;
        }

        public void LostFocus() {
            //occurs when editing tag text
            IsEditingTitle = false;
        }
        #endregion

        #region Commands

        private DelegateCommand<KeyEventArgs> _keyDownCommand;
        public ICommand KeyDownCommand {
            get {
                if(_keyDownCommand == null) {
                    _keyDownCommand = new DelegateCommand<KeyEventArgs>(KeyDown,CanKeyDown);
                }
                return _keyDownCommand;
            }
        }
        private bool CanKeyDown(KeyEventArgs e) {
            return Visibility == Visibility.Visible;
        }
        private void KeyDown(KeyEventArgs e) {
            Key key = e.Key;
            if(key == Key.Delete || key == Key.Back && !IsEditingTitle) {
                //delete clip which shifts focus to neighbor
                DeleteClipCommand.Execute(null);
            } else if(key == Key.Enter) {
                if(IsEditingTitle) {
                    IsEditingTitle = false;
                    e.Handled = true;
                    return;
                } else {
                    //In order to paste the app must hide first
                    var mw = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext);
                    mw.HideWindowCommand.Execute(null);
                    foreach(var clipTile in mw.SelectedClipTiles) {
                        MpDataStore.Instance.ClipboardManager.PasteCopyItem(clipTile.CopyItem.Text);
                    }
                }
            }
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
        #endregion

        public override string ToString() {
            return CopyItem.ToString();
        }
    }
}
