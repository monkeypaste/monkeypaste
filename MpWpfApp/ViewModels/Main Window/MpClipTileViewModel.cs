using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpClipTileViewModel : MpViewModelBase {
        #region Private Variables
        private int _detailIdx = 0;
        private List<string> _tempFileList = new List<string>();
        #endregion

        #region View Models
        private MpClipTrayViewModel _clipTrayViewModel;
        public MpClipTrayViewModel ClipTrayViewModel {
            get {
                return _clipTrayViewModel;
            }
            set {
                if (_clipTrayViewModel != value) {
                    _clipTrayViewModel = value;
                    OnPropertyChanged(nameof(ClipTrayViewModel));
                }
            }
        }

        private MpClipTileContextMenuViewModel _clipTileContextMenu = null;
        public MpClipTileContextMenuViewModel ClipTileContextMenuViewModel {
            get {
                return _clipTileContextMenu;
            }
            set {
                if (_clipTileContextMenu != value) {
                    _clipTileContextMenu = value;
                    OnPropertyChanged(nameof(ClipTileContextMenuViewModel));
                }
            }
        }

        private MpClipTileTitleViewModel _clipTileTitleViewModel;
        public MpClipTileTitleViewModel ClipTileTitleViewModel {
            get {
                return _clipTileTitleViewModel;
            }
            set {
                if (_clipTileTitleViewModel != value) {
                    _clipTileTitleViewModel = value;
                    OnPropertyChanged(nameof(ClipTileTitleViewModel));
                }
            }
        }

        private MpClipTileContentViewModel _clipTileContentViewModel;
        public MpClipTileContentViewModel ClipTileContentViewModel {
            get {
                return _clipTileContentViewModel;
            }
            set {
                if (_clipTileContentViewModel != value) {
                    _clipTileContentViewModel = value;
                    OnPropertyChanged(nameof(ClipTileContentViewModel));
                }
            }
        }

        private ObservableCollection<MpClipTileContextMenuItemViewModel> _tagMenuItems = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
        public ObservableCollection<MpClipTileContextMenuItemViewModel> TagMenuItems {
            get {
                _tagMenuItems = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
                foreach (var tagTile in ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel) {
                    if (tagTile.TagName == Properties.Settings.Default.HistoryTagTitle) {
                        continue;
                    }
                    _tagMenuItems.Add(new MpClipTileContextMenuItemViewModel(tagTile.TagName, ClipTrayViewModel.LinkTagToCopyItemCommand, tagTile, tagTile.Tag.IsLinkedWithCopyItem(CopyItem), null));
                }
                return _tagMenuItems;
            }
            set {
                if (_tagMenuItems != value) {
                    _tagMenuItems = value;
                    OnPropertyChanged(nameof(TagMenuItems));
                }
            }
        }

        #endregion

        #region Property Reflection Referencer
        public object this[string propertyName] {
            get {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(MpClipTileViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                if(myPropInfo == null) {
                    myType = typeof(MpClipTileTitleViewModel);
                    myPropInfo = myType.GetProperty(propertyName);
                }
                if (myPropInfo == null) {
                    myType = typeof(MpClipTileContentViewModel);
                    myPropInfo = myType.GetProperty(propertyName);
                }
                if (myPropInfo == null) {
                    throw new Exception("Unable to find property: " + propertyName);
                }
                return myPropInfo.GetValue(this, null);
            }
            set {
                Type myType = typeof(MpClipTileViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }
        #endregion

        #region Properties
        
        public string Title {
            get {
                return ClipTileTitleViewModel.Title;
            }
            set {
                if(ClipTileTitleViewModel.Title != value) {
                    ClipTileTitleViewModel.Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Content {
            get {
                return ClipTileContentViewModel.PlainText;
            }
        }

        public bool IsNew {
            get {
                return CopyItem.CopyItemId == 0;
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

        private Brush _tileBorderBrush = Brushes.Transparent;
        public Brush TileBorderBrush {
            get {
                return _tileBorderBrush;
            }
            set {
                if (_tileBorderBrush != value) {
                    _tileBorderBrush = value;
                    OnPropertyChanged(nameof(TileBorderBrush));
                }
            }
        }

        private int _sortOrderIdx = -1;
        public int SortOrderIdx {
            get {
                return _sortOrderIdx;
            }
            set {
                if (_sortOrderIdx != value) {
                    _sortOrderIdx = value;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        private Visibility _tileVisibility = Visibility.Visible;
        public Visibility TileVisibility {
            get {
                return _tileVisibility;
            }
            set {
                if (_tileVisibility != value) {
                    _tileVisibility = value;
                    OnPropertyChanged(nameof(TileVisibility));
                }
            }
        }

        private double _tileSize = MpMeasurements.Instance.ClipTileSize;
        public double TileSize {
            get {
                return _tileSize;
            }
            set {
                if (_tileSize != value) {
                    _tileSize = value;
                    OnPropertyChanged(nameof(TileSize));
                }
            }
        }

        private double _tileBorderSize = MpMeasurements.Instance.ClipTileBorderSize;
        public double TileBorderSize {
            get {
                return _tileBorderSize;
            }
            set {
                if (_tileBorderSize != value) {
                    _tileBorderSize = value;
                    OnPropertyChanged(nameof(TileBorderSize));
                }
            }
        }

        private double _tileTitleHeight = MpMeasurements.Instance.ClipTileTitleHeight;
        public double TileTitleHeight {
            get {
                return _tileTitleHeight;
            }
            set {
                if (_tileTitleHeight != value) {
                    _tileTitleHeight = value;
                    OnPropertyChanged(nameof(TileTitleHeight));
                }
            }
        }

        private double _tileContentHeight = MpMeasurements.Instance.ClipTileContentHeight;
        public double TileContentHeight {
            get {
                return _tileContentHeight;
            }
            set {
                if (_tileContentHeight != value) {
                    _tileContentHeight = value;
                    OnPropertyChanged(nameof(TileContentHeight));
                }
            }
        }

        private double _tileBorderThickness = MpMeasurements.Instance.ClipTileBorderThickness;
        public double TileBorderThickness {
            get {
                return _tileBorderThickness;
            }
            set {
                if (_tileBorderThickness != value) {
                    _tileBorderThickness = value;
                    OnPropertyChanged(nameof(TileBorderThickness));
                }
            }
        }

        private double _tileMargin = MpMeasurements.Instance.ClipTileMargin;
        public double TileMargin {
            get {
                return _tileMargin;
            }
            set {
                if (_tileMargin != value) {
                    _tileMargin = value;
                    OnPropertyChanged(nameof(TileMargin));
                }
            }
        }

        private double _tileDropShadowRadius = MpMeasurements.Instance.ClipTileDropShadowRadius;
        public double TileDropShadowRadius {
            get {
                return _tileDropShadowRadius;
            }
            set {
                if (_tileDropShadowRadius != value) {
                    _tileDropShadowRadius = value;
                    OnPropertyChanged(nameof(TileDropShadowRadius));
                }
            }
        }

        public int CopyItemUsageScore {
            get {
                return CopyItem.RelevanceScore;
            }
        }

        public int CopyItemAppId {
            get {
                return CopyItem.AppId;
            }
        }

        public MpCopyItemType CopyItemType {
            get {
                return CopyItem.CopyItemType;
            }
        }

        public DateTime CopyItemCreatedDateTime {
            get {
                return CopyItem.CopyDateTime;
            }
        }

        private MpCopyItem _copyItem;
        public MpCopyItem CopyItem {
            get {
                return _copyItem;
            }
            set {
                if (_copyItem != value) {
                    _copyItem = value;
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }
        #endregion        

        #region Public Methods
        public MpClipTileViewModel(MpCopyItem ci, MpClipTrayViewModel parent) {
            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(IsSelected):
                        if (IsSelected) {
                            TileBorderBrush = Brushes.Red;
                            ClipTileTitleViewModel.DetailTextColor = Brushes.Red;
                            //this check ensures that as user types in search that 
                            //resetselection doesn't take the focus from the search box
                            if (!ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.IsFocused) {
                                IsFocused = true;
                            }
                        } else {
                            TileBorderBrush = Brushes.Transparent;
                            ClipTileTitleViewModel.DetailTextColor = Brushes.Transparent;
                            //below must be called to clear focus when deselected (it may not have focus)
                            IsFocused = false;
                        }
                        break;
                    case nameof(IsHovering):
                        if (!IsSelected) {
                            if (IsHovering) {
                                TileBorderBrush = Brushes.Yellow;
                                ClipTileTitleViewModel.DetailTextColor = Brushes.DarkKhaki;
                                //this is necessary for dragdrop re-sorting
                            } else {
                                TileBorderBrush = Brushes.Transparent;
                                ClipTileTitleViewModel.DetailTextColor = Brushes.Transparent;
                            }
                        }
                        break;
                }
            };
            CopyItem = ci;
            ClipTrayViewModel = parent;
            //ClipTileContextMenuViewModel = new MpClipTileContextMenuViewModel(ClipTrayViewModel);
            ClipTileTitleViewModel = new MpClipTileTitleViewModel(CopyItem, this);
            ClipTileContentViewModel = new MpClipTileContentViewModel(CopyItem, this);
        }

        public void AppendContent(MpClipTileViewModel ctvm) {
            CopyItem.Combine(ctvm.CopyItem);
            ClipTileContentViewModel = new MpClipTileContentViewModel(CopyItem, this);
        }

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTileBorder = (MpClipBorder)sender;
            clipTileBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            clipTileBorder.MouseLeave += (s, e2) => {
                IsHovering = false;
            };
            clipTileBorder.LostFocus += (s, e4) => {
                ClipTileTitleViewModel.IsEditingTitle = false;
            };
        }

        public void DeleteTempFiles() {
            foreach (var f in _tempFileList) {
                if (File.Exists(f)) {
                    File.Delete(f);
                }
            }
        }

        public void ContextMenuMouseLeftButtonUpOnSearchGoogle() {
            System.Diagnostics.Process.Start(@"https://www.google.com/search?q=" + System.Uri.EscapeDataString(ClipTileContentViewModel.PlainText));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchBing() {
            System.Diagnostics.Process.Start(@"https://www.bing.com/search?q=" + System.Uri.EscapeDataString(ClipTileContentViewModel.PlainText));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchDuckDuckGo() {
            System.Diagnostics.Process.Start(@"https://duckduckgo.com/?q=" + System.Uri.EscapeDataString(ClipTileContentViewModel.PlainText));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchYandex() {
            System.Diagnostics.Process.Start(@"https://yandex.com/search/?text=" + System.Uri.EscapeDataString(ClipTileContentViewModel.PlainText));
        }

        #endregion

        #region Private Methods       

        #endregion

        #region Commands

        private RelayCommand _changeClipColorCommand;
        public ICommand ChangeClipColorCommand {
            get { 
                if (_changeClipColorCommand == null) {
                    _changeClipColorCommand = new RelayCommand(ChangeClipColor);
                }
                return _changeClipColorCommand;
            }
        }
        private void ChangeClipColor() {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            cd.ShowHelp = true;
            cd.Color = MpHelpers.ConvertSolidColorBrushToWinFormsColor((SolidColorBrush)ClipTileTitleViewModel.TitleColor);
            cd.CustomColors = Properties.Settings.Default.UserCustomColorIdxArray;

            var mw = (MpMainWindow)Application.Current.MainWindow;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = true;
            // Update the text box color if the user clicks OK 
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                ClipTileTitleViewModel.TitleColor = MpHelpers.ConvertWinFormsColorToSolidColorBrush(cd.Color);
                ClipTileTitleViewModel.InitSwirl();
                CopyItem.WriteToDatabase();
            }
            Properties.Settings.Default.UserCustomColorIdxArray = cd.CustomColors;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = false;
        }

        #endregion

        #region Overrides
        public override string ToString() {
            return CopyItem.GetPlainText();
        }
        #endregion
    }
}
