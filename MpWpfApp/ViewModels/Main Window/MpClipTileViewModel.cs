using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
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
using System.Windows.Media.Imaging;

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

        private ObservableCollection<MpFileListItemViewModel> _fileListViewModels;
        public ObservableCollection<MpFileListItemViewModel> FileListViewModels {
            get {
                if(_fileListViewModels == null) {
                    _fileListViewModels = new ObservableCollection<MpFileListItemViewModel>();
                    foreach (var path in FileDropList) {
                        _fileListViewModels.Add(new MpFileListItemViewModel(path));
                    }
                }
                return _fileListViewModels;
            }
            set {
                if (_fileListViewModels != value) {
                    _fileListViewModels = value;
                    OnPropertyChanged(nameof(FileListViewModels));
                }
            }
        }

        private double _contentHeight = 0;
        public double ContentHeight {
            get {
                return _contentHeight;
            }
            set {
                if (_contentHeight != value) {
                    _contentHeight = value;
                    OnPropertyChanged(nameof(ContentHeight));
                }
            }
        }

        private double _contentWidth = 0;
        public double ContentWidth {
            get {
                return _contentWidth;
            }
            set {
                if (_contentWidth != value) {
                    _contentWidth = value;
                    OnPropertyChanged(nameof(ContentWidth));
                }
            }
        }

        private Visibility _imgVisibility = Visibility.Visible;
        public Visibility ImgVisibility {
            get {
                return _imgVisibility;
            }
            set {
                if (_imgVisibility != value) {
                    _imgVisibility = value;
                    OnPropertyChanged(nameof(ImgVisibility));
                }
            }
        }

        private Visibility _fileListVisibility = Visibility.Visible;
        public Visibility FileListVisibility {
            get {
                return _fileListVisibility;
            }
            set {
                if (_fileListVisibility != value) {
                    _fileListVisibility = value;
                    OnPropertyChanged(nameof(FileListVisibility));
                }
            }
        }

        private Visibility _rtbVisibility = Visibility.Visible;
        public Visibility RtbVisibility {
            get {
                return _rtbVisibility;
            }
            set {
                if (_rtbVisibility != value) {
                    _rtbVisibility = value;
                    OnPropertyChanged(nameof(RtbVisibility));
                }
            }
        }

        //for drag/drop,export and paste not view
        private string _plainText = string.Empty;
        public string PlainText {
            get {
                if (_plainText == string.Empty) {
                    _plainText = CopyItem.ItemPlainText;
                }
                return _plainText;
            }
            set {
                if (_plainText != value) {
                    _plainText = value;
                    OnPropertyChanged(nameof(PlainText));
                }
            }
        }

        //for drag/drop,export and paste not view
        private string _richText = string.Empty;
        public string RichText {
            get {
                if (_richText == string.Empty) {
                    _richText = CopyItem.ItemRichText;
                }
                return _richText;
            }
            set {
                if (_richText != value) {
                    _richText = value;
                    OnPropertyChanged(nameof(RichText));
                }
            }
        }

        private BitmapSource _bmp = null;
        public BitmapSource Bmp {
            get {
                if (_bmp == null) {
                    _bmp = CopyItem.ItemImage;
                }
                return _bmp;
            }
            set {
                if (_bmp != value) {
                    _bmp = value;
                    OnPropertyChanged(nameof(Bmp));
                }
            }
        }

        private List<string> _fileDropList = null;
        public List<string> FileDropList {
            get {
                if(_fileDropList == null) {
                    _fileDropList = CopyItem.GetFileList(string.Empty, ClipTrayViewModel.GetTargetFileType());
                }
                return _fileDropList;
            }
            set {
                if(_fileDropList != value) {
                    _fileDropList = value;
                    OnPropertyChanged(nameof(FileDropList));
                }
            }
        }
        private ObservableCollection<MpSubTextToken> _tokens = null;
        public ObservableCollection<MpSubTextToken> Tokens {
            get {
                if (_tokens == null) {
                    _tokens = (IList<MpSubTextToken>)CopyItem.SubTextTokenList as ObservableCollection<MpSubTextToken>;
                }
                return _tokens;
            }
            set {
                if (_tokens != value) {
                    _tokens = value;
                    OnPropertyChanged(nameof(Tokens));
                }
            }
        }        

        private string _searchText = string.Empty;
        public string SearchText {
            get {
                return _searchText;
            }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        public bool IsLoading { get; set; } = false;

        private string _detailText = "This is empty detail text";
        public string DetailText {
            get {
                return _detailText;
            }
            set {
                if (_detailText != value) {
                    _detailText = value;
                    OnPropertyChanged(nameof(DetailText));
                }
            }
        }

        private Brush _detailTextColor = Brushes.Transparent;
        public Brush DetailTextColor {
            get {
                return _detailTextColor;
            }
            set {
                if (_detailTextColor != value) {
                    _detailTextColor = value;
                    OnPropertyChanged(nameof(DetailTextColor));
                }
            }
        }

        private ImageSource _icon = null;
        public ImageSource Icon {
            get {
                return _icon;
            }
            set {
                if (_icon != value) {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        public string Title {
            get {
                return CopyItem.Title;
            }
            set {
                if (CopyItem.Title != value) {
                    CopyItem.Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public Brush TitleColor {
            get {
                return new SolidColorBrush(CopyItem.ItemColor.Color);
            }
            set {
                if (CopyItem.ItemColor.Color != ((SolidColorBrush)value).Color) {
                    CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                    OnPropertyChanged(nameof(TitleColor));
                }
            }
        }

        public Brush TitleColorLighter {
            get {
                return MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(
                        TitleColor,
                        -0.5f),
                    100);
            }
        }

        public Brush TitleColorDarker {
            get {
                return MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(
                        TitleColor,
                        -0.4f),
                    50);
            }
        }

        public Brush TitleColorAccent {
            get {
                return MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(
                        TitleColor,
                        -0.0f),
                    100);
            }
        }

        private BitmapSource _titleSwirl = null;
        public BitmapSource TitleSwirl {
            get {
                return _titleSwirl;
            }
            set {
                if (_titleSwirl != value) {
                    _titleSwirl = value;
                    OnPropertyChanged(nameof(TitleSwirl));
                }
            }
        }

        private bool _isTitleTextBoxFocused = false;
        public bool IsTitleTextBoxFocused {
            get {
                return _isTitleTextBoxFocused;
            }
            set {
                if (_isTitleTextBoxFocused != value) {
                    _isTitleTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsTitleTextBoxFocused));
                }
            }
        }

        private bool _isEditingTitle = false;
        public bool IsEditingTitle {
            get {
                return _isEditingTitle;
            }
            set {
                if (_isEditingTitle != value) {
                    _isEditingTitle = value;
                    OnPropertyChanged(nameof(IsEditingTitle));
                }
            }
        }

        private Visibility _tileTitleTextBlockVisibility = Visibility.Visible;
        public Visibility TileTitleTextBlockVisibility {
            get {
                return _tileTitleTextBlockVisibility;
            }
            set {
                if (_tileTitleTextBlockVisibility != value) {
                    _tileTitleTextBlockVisibility = value;
                    OnPropertyChanged(nameof(TileTitleTextBlockVisibility));
                }
            }
        }

        private Visibility _tileTitleTextBoxVisibility = Visibility.Collapsed;
        public Visibility TileTitleTextBoxVisibility {
            get {
                return _tileTitleTextBoxVisibility;
            }
            set {
                if (_tileTitleTextBoxVisibility != value) {
                    _tileTitleTextBoxVisibility = value;
                    OnPropertyChanged(nameof(TileTitleTextBoxVisibility));
                }
            }
        }

        private double _tileTitleIconSize = MpMeasurements.Instance.ClipTileTitleIconSize;
        public double TileTitleIconSize {
            get {
                return _tileTitleIconSize;
            }
            set {
                if (_tileTitleIconSize != value) {
                    _tileTitleIconSize = value;
                    OnPropertyChanged(nameof(TileTitleIconSize));
                }
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
                    case nameof(IsEditingTitle):
                        if (IsEditingTitle) {
                            //show textbox and select all text
                            TileTitleTextBoxVisibility = Visibility.Visible;
                            TileTitleTextBlockVisibility = Visibility.Collapsed;
                            IsTitleTextBoxFocused = false;
                            IsTitleTextBoxFocused = true;
                        } else {
                            TileTitleTextBoxVisibility = Visibility.Collapsed;
                            TileTitleTextBlockVisibility = Visibility.Visible;
                            IsTitleTextBoxFocused = false;
                            CopyItem.WriteToDatabase();
                        }
                        break;
                    case nameof(IsSelected):
                        if (IsSelected) {
                            TileBorderBrush = Brushes.Red;
                            DetailTextColor = Brushes.Red;
                            //this check ensures that as user types in search that 
                            //resetselection doesn't take the focus from the search box
                            if (!ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.IsFocused) {
                                IsFocused = true;
                            }
                        } else {
                            TileBorderBrush = Brushes.Transparent;
                            DetailTextColor = Brushes.Transparent;
                            //below must be called to clear focus when deselected (it may not have focus)
                            IsFocused = false;
                        }
                        break;
                    case nameof(IsHovering):
                        if (!IsSelected) {
                            if (IsHovering) {
                                TileBorderBrush = Brushes.Yellow;
                                DetailTextColor = Brushes.DarkKhaki;
                                //this is necessary for dragdrop re-sorting
                            } else {
                                TileBorderBrush = Brushes.Transparent;
                                DetailTextColor = Brushes.Transparent;
                            }
                        }
                        break;
                }
            };
            IsLoading = true;
            CopyItem = ci;
            ClipTrayViewModel = parent;
            Title = ci.Title;
            TitleColor = new SolidColorBrush(ci.ItemColor.Color);
            Icon = ci.App.Icon.IconImage;
            Tokens = new ObservableCollection<MpSubTextToken>(CopyItem.SubTextTokenList);
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
                IsEditingTitle = false;
            };
        }

        public void ClipTileTitle_Loaded(object sender, RoutedEventArgs e) {
            if (TitleSwirl == null) {
                InitSwirl();
            }

            var titleCanvas = (Canvas)sender;
            var clipTileTitleTextBox = (TextBox)titleCanvas.FindName("ClipTileTitleTextBox");
            clipTileTitleTextBox.PreviewKeyDown += ClipTrayViewModel.MainWindowViewModel.MainWindow_PreviewKeyDown;
            clipTileTitleTextBox.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };

            var titleIconImage = (Image)titleCanvas.FindName("ClipTileAppIconImage");
            Canvas.SetLeft(titleIconImage, TileBorderSize - TileTitleHeight - 10);
            Canvas.SetTop(titleIconImage, 2);

            var titleDetailTextBlock = (TextBlock)titleCanvas.FindName("ClipTileTitleDetailTextBlock");
            Canvas.SetLeft(titleDetailTextBlock, 5);
            Canvas.SetTop(titleDetailTextBlock, TileTitleHeight - 14);
        }

        public void ContentCanvas_Loaded(object sender, RoutedEventArgs e) {
            switch (CopyItemType) {
                case MpCopyItemType.FileList:
                    var flb = (ListBox)((Canvas)sender)?.FindName("ClipTileFileListBox");
                    flb.ContextMenu = (ContextMenu)flb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");

                    ContentWidth = Bmp.Width;
                    ContentHeight = Bmp.Height;

                    FileListVisibility = Visibility.Visible;
                    ImgVisibility = Visibility.Collapsed;
                    RtbVisibility = Visibility.Collapsed;
                    break;
                case MpCopyItemType.Image:
                    var img = (Image)((Canvas)sender)?.FindName("ClipTileImage");
                    img.ContextMenu = (ContextMenu)img.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
                    //aspect ratio
                    double ar = Bmp.Width / Bmp.Height;
                    if (Bmp.Width >= Bmp.Height) {
                        ContentWidth = TileBorderSize;
                        ContentHeight = ContentWidth * ar;
                    } else {
                        ContentHeight = TileContentHeight;
                        ContentWidth = ContentHeight * ar;
                    }
                    MpHelpers.ResizeBitmapSource(Bmp, new Size((int)ContentWidth, (int)ContentHeight));

                    Canvas.SetLeft(img, (TileBorderSize / 2) - (ContentWidth / 2));
                    Canvas.SetTop(img, (TileContentHeight / 2) - (ContentHeight / 2));

                    FileListVisibility = Visibility.Collapsed;
                    ImgVisibility = Visibility.Visible;
                    RtbVisibility = Visibility.Collapsed;
                    break;
                case MpCopyItemType.RichText:
                    var rtb = (MpTokenizedRichTextBox)((Canvas)sender)?.FindName("ClipTileRichTextBox");
                    rtb.ContextMenu = (ContextMenu)rtb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
                    ContentWidth = rtb.RenderSize.Width;
                    ContentHeight = rtb.RenderSize.Height;
                    rtb.Document.PageWidth = rtb.Width - rtb.Padding.Left - rtb.Padding.Right;
                    rtb.Document.PageHeight = rtb.Height - rtb.Padding.Top - rtb.Padding.Bottom;

                    //Tokens = new ObservableCollection<MpSubTextToken>(CopyItem.SubTextTokenList.OrderBy(stt => stt.BlockIdx).ThenBy(stt => stt.StartIdx).ToList());
                    //foreach (var sortedToken in sortedTokenList) {
                    //    rtb.AddSubTextToken(sortedToken);
                    //}
                    rtb.SearchText = string.Empty;

                    FileListVisibility = Visibility.Collapsed;
                    ImgVisibility = Visibility.Collapsed;
                    RtbVisibility = Visibility.Visible;
                    break;
            }
        }

        public void AppendContent(MpClipTileViewModel octvm) {
            CopyItem.Combine(octvm.CopyItem);

            //reinitialize item view properties
            PlainText = CopyItem.ItemPlainText;

            RichText = CopyItem.ItemRichText;

            Bmp = CopyItem.ItemImage;

            FileDropList = CopyItem.GetFileList();

            FileListViewModels.Clear();
            foreach(var path in FileDropList) {
                FileListViewModels.Add(new MpFileListItemViewModel(path));
            }
        }

        public void DeleteTempFiles() {
            foreach (var f in _tempFileList) {
                if (File.Exists(f)) {
                    File.Delete(f);
                }
            }
        }

        public void ContextMenuMouseLeftButtonUpOnSearchGoogle() {
            System.Diagnostics.Process.Start(@"https://www.google.com/search?q=" + System.Uri.EscapeDataString(PlainText));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchBing() {
            System.Diagnostics.Process.Start(@"https://www.bing.com/search?q=" + System.Uri.EscapeDataString(PlainText));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchDuckDuckGo() {
            System.Diagnostics.Process.Start(@"https://duckduckgo.com/?q=" + System.Uri.EscapeDataString(PlainText));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchYandex() {
            System.Diagnostics.Process.Start(@"https://yandex.com/search/?text=" + System.Uri.EscapeDataString(PlainText));
        }

        #endregion

        #region Private Methods       

        private void InitSwirl() {
            var swirl1 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0001.png"));
            swirl1 = MpHelpers.TintBitmapSource(swirl1, ((SolidColorBrush)TitleColor).Color);
            var swirl2 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0002.png"));
            swirl2 = MpHelpers.TintBitmapSource(swirl2, ((SolidColorBrush)TitleColorLighter).Color);
            var swirl3 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0003.png"));
            swirl3 = MpHelpers.TintBitmapSource(swirl3, ((SolidColorBrush)TitleColorDarker).Color);
            var swirl4 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0004.png"));
            swirl4 = MpHelpers.TintBitmapSource(swirl4, ((SolidColorBrush)TitleColorAccent).Color);

            TitleSwirl = MpHelpers.MergeImages(new List<BitmapSource>() { swirl1, swirl2, swirl3, swirl4 });
        }

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
            cd.Color = MpHelpers.ConvertSolidColorBrushToWinFormsColor((SolidColorBrush)TitleColor);
            cd.CustomColors = Properties.Settings.Default.UserCustomColorIdxArray;

            var mw = (MpMainWindow)Application.Current.MainWindow;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = true;
            // Update the text box color if the user clicks OK 
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                TitleColor = MpHelpers.ConvertWinFormsColorToSolidColorBrush(cd.Color);
                InitSwirl();
                CopyItem.WriteToDatabase();
            }
            Properties.Settings.Default.UserCustomColorIdxArray = cd.CustomColors;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = false;
        }

        #endregion

        #region Overrides
        public override string ToString() {
            return CopyItem.ItemPlainText;
        }
        #endregion
    }
}
