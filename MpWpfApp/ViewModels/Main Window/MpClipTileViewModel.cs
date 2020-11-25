namespace MpWpfApp {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Speech.Synthesis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using GalaSoft.MvvmLight.CommandWpf;
    using Gma.System.MouseKeyHook;
    using GongSolutions.Wpf.DragDrop.Utilities;
    using NativeCode;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Storage;
    using WPF.JoshSmith.ServiceProviders.UI;

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

        private ObservableCollection<MpClipTileContextMenuItemViewModel> _convertClipTypes = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
        public ObservableCollection<MpClipTileContextMenuItemViewModel> ConvertClipTypes {
            get {
                return _convertClipTypes;
            }
            set {
                if (_convertClipTypes != value) {
                    _convertClipTypes = value;
                    OnPropertyChanged(nameof(ConvertClipTypes));
                }
            }
        }

        private ObservableCollection<MpClipTileContextMenuItemViewModel> _tagMenuItems = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
        public ObservableCollection<MpClipTileContextMenuItemViewModel> TagMenuItems {
            get {
                return _tagMenuItems;
            }
            set {
                if(_tagMenuItems != value) {
                    _tagMenuItems = value;
                    OnPropertyChanged(nameof(TagMenuItems));
                }
            }
        }

        private ObservableCollection<MpFileListItemViewModel> _fileListViewModels;
        public ObservableCollection<MpFileListItemViewModel> FileListViewModels {
            get {
                if (_fileListViewModels == null) {
                    _fileListViewModels = new ObservableCollection<MpFileListItemViewModel>();
                    foreach (var path in FileDropList) {
                        _fileListViewModels.Add(new MpFileListItemViewModel(this,path));
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

        #endregion

        #region Property Reflection Referencer
        public object this[string propertyName] {
            get {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(MpClipTileViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
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

        #region Model Propertiese
        private string _shortcutKeyList = string.Empty;
        public string ShortcutKeyList {
            get {
                return _shortcutKeyList;
            }
            set {
                if (_shortcutKeyList != value) {
                    _shortcutKeyList = value;
                    OnPropertyChanged(nameof(ShortcutKeyList));
                }
            }
        }

        private int _copyItemId = 0;
        public int CopyItemId {
            get {
                return _copyItemId;
            }
            set {
                if (_copyItemId != value) {
                    _copyItemId = value;
                    OnPropertyChanged(nameof(CopyItemId));
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

        private string _xamlText = string.Empty;
        public string XamlText {
            get {
                if (_xamlText == string.Empty) {
                    _xamlText = CopyItem.ItemXaml;
                }
                return _xamlText;
            }
            set {
                if (_xamlText != value) {
                    _xamlText = value;
                    OnPropertyChanged(nameof(XamlText));
                }
            }
        }

        private MpEventEnabledFlowDocument _documentRtf = null;
        public MpEventEnabledFlowDocument DocumentRtf {
            get {
                if (_documentRtf == null) {
                    _documentRtf = CopyItem.ItemFlowDocument;
                }
                return _documentRtf;
            }
            set {
                if (_documentRtf != value) {
                    _documentRtf = value;
                    OnPropertyChanged(nameof(DocumentRtf));
                }
            }
        }

        private BitmapSource _bmp = null;
        public BitmapSource Bmp {
            get {
                if (_bmp == null) {
                    _bmp = CopyItem.ItemBitmapSource;
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
                if (_fileDropList == null) {
                    _fileDropList = CopyItem.GetFileList(string.Empty, ClipTrayViewModel.GetTargetFileType());
                }
                return _fileDropList;
            }
            set {
                if (_fileDropList != value) {
                    _fileDropList = value;
                    OnPropertyChanged(nameof(FileDropList));
                }
            }
        }

        private ObservableCollection<MpSubTextToken> _tokens = null;
        public ObservableCollection<MpSubTextToken> Tokens {
            get {
                if (_tokens == null) {
                    _tokens = new ObservableCollection<MpSubTextToken>(CopyItem.SubTextTokenList);
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

        private BitmapSource _icon = null;
        public BitmapSource Icon {
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

        public string CopyItemAppName {
            get {
                return CopyItem.App.AppName;
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

        #region Layout Properties
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

        private double _tileBorderWidth = MpMeasurements.Instance.ClipTileBorderSize;
        public double TileBorderWidth {
            get {
                return _tileBorderWidth;
            }
            set {
                if (_tileBorderWidth != value) {
                    _tileBorderWidth = value;
                    OnPropertyChanged(nameof(TileBorderWidth));
                }
            }
        }

        private double _tileBorderHeight = MpMeasurements.Instance.ClipTileSize;
        public double TileBorderHeight {
            get {
                return _tileBorderHeight;
            }
            set {
                if (_tileBorderHeight != value) {
                    _tileBorderHeight = value;
                    OnPropertyChanged(nameof(TileBorderHeight));
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

        private double _tileContentWidth = MpMeasurements.Instance.ClipTileContentWidth;
        public double TileContentWidth {
            get {
                return _tileContentWidth;
            }
            set {
                if (_tileContentWidth != value) {
                    _tileContentWidth = value;
                    OnPropertyChanged(nameof(TileContentWidth));
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

        private double _tileContentMargin = MpMeasurements.Instance.ClipTileContentMargin;
        public double TileContentMargin {
            get {
                return _tileContentMargin;
            }
            set {
                if (_tileContentMargin != value) {
                    _tileContentMargin = value;
                    OnPropertyChanged(nameof(TileContentMargin));
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
        #endregion

        #region Visibility Properties
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

        private Visibility _editToolbarVisibility = Visibility.Collapsed;
        public Visibility EditToolbarVisibility {
            get {
                return _editToolbarVisibility;
            }
            set {
                if (_editToolbarVisibility != value) {
                    _editToolbarVisibility = value;
                    OnPropertyChanged(nameof(EditToolbarVisibility));
                }
            }
        }
        #endregion

        #region Business Logic Properties
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
        #endregion

        #region Brush Properties
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
        #endregion

        #region State Properties
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

        private bool _isEditingTile = false;
        public bool IsEditingTile {
            get {
                return _isEditingTile;
            }
            set {
                if (_isEditingTile != value) {
                    _isEditingTile = value;
                    OnPropertyChanged(nameof(IsEditingTile));
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
        #endregion

        #endregion        

        #region Public Methods
        public MpClipTileViewModel() : this(new MpCopyItem(), null) { }

        public MpClipTileViewModel(MpCopyItem ci, MpClipTrayViewModel parent) {
            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(IsEditingTile):
                        if(IsEditingTile) {
                            EditToolbarVisibility = Visibility.Visible;
                        } else {
                            EditToolbarVisibility = Visibility.Collapsed;
                        }
                        break;
                    case nameof(CopyItemId):
                        CopyItem.CopyItemId = CopyItemId;
                        break;
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
                            IsEditingTile = false;
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
                    case nameof(RichText):
                        //CopyItem.ItemRichText = RichText;
                        //CopyItem.WriteToDatabase();
                        break;
                }
            };

            ClipTrayViewModel = parent;
            IsLoading = true;
            CopyItem = ci;
            CopyItemId = CopyItem.CopyItemId;
            Title = ci.Title;
            TitleColor = new SolidColorBrush(ci.ItemColor.Color);
            Icon = ci.App.Icon.IconImage;
            Tokens = new ObservableCollection<MpSubTextToken>(CopyItem.SubTextTokenList);

            InitSwirl();


            FileListVisibility = CopyItemType == MpCopyItemType.FileList ? Visibility.Visible : Visibility.Collapsed;
            ImgVisibility = CopyItemType == MpCopyItemType.Image ? Visibility.Visible : Visibility.Collapsed;
            RtbVisibility = CopyItemType == MpCopyItemType.RichText ? Visibility.Visible : Visibility.Collapsed;
            //ErtbVisibility = Visibility.Collapsed;
        }

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            MainWindowViewModel.SearchBoxViewModel.PropertyChanged += (s, e2) => {
                switch (e2.PropertyName) {
                    case nameof(MainWindowViewModel.SearchBoxViewModel.SearchText):
                        SearchText = MainWindowViewModel.SearchBoxViewModel.SearchText;
                        break;
                }
            };

            var clipTileBorder = (MpClipBorder)sender;
            clipTileBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            clipTileBorder.MouseLeave += (s, e2) => {
                IsHovering = false;
            };
            clipTileBorder.LostFocus += (s, e4) => {
                if(!IsSelected) {
                    IsEditingTitle = false;
                }
            };
            clipTileBorder.PreviewMouseLeftButtonDown += (s, e5) => {
                if (e5.ClickCount == 2) {
                    //only for richtext type
                    EditClipTextCommand.Execute(null);
                    return;
                }
            };

        }

        public void ClipTileTitle_Loaded(object sender, RoutedEventArgs e) {
            var titleCanvas = (Canvas)sender;
            
            var clipTileTitleTextBlock = (TextBlock)titleCanvas.FindName("ClipTileTitleTextBlock");
            clipTileTitleTextBlock.MouseEnter += (s, e1) => {
                Application.Current.MainWindow.Cursor = Cursors.IBeam;
            };
            clipTileTitleTextBlock.MouseLeave += (s, e7) => {
                Application.Current.MainWindow.Cursor = Cursors.Arrow;
            };
            clipTileTitleTextBlock.PreviewMouseLeftButtonUp += (s, e7) => {
                IsEditingTitle = true;
            };

            var clipTileTitleTextBox = (TextBox)titleCanvas.FindName("ClipTileTitleTextBox");
            clipTileTitleTextBox.PreviewKeyDown += ClipTrayViewModel.MainWindowViewModel.MainWindow_PreviewKeyDown;
            clipTileTitleTextBox.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };

            var titleIconImageButton = (Button)titleCanvas.FindName("ClipTileAppIconImageButton");
            Canvas.SetLeft(titleIconImageButton, TileBorderWidth - TileTitleHeight - 10);
            Canvas.SetTop(titleIconImageButton, 2);
            titleIconImageButton.PreviewMouseUp += (s, e7) => {
                // TODO (somehow) force mainwindow to stay active when switching or opening app process
                // TODO check if shift is down if so perform paste into target application
                // TODO check if App is running if it is switch to it or start its process

                System.Diagnostics.Process.Start(CopyItem.App.AppPath);
                
            };

            var titleDetailTextBlock = (TextBlock)titleCanvas.FindName("ClipTileTitleDetailTextBlock");
            titleDetailTextBlock.MouseEnter += (s, e5) => {
                if(++_detailIdx > 2) {
                    _detailIdx = 0;
                }
                titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
            };
            titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
            Canvas.SetLeft(titleDetailTextBlock, 5);
            Canvas.SetTop(titleDetailTextBlock, TileTitleHeight - 14);
        }

        public void ClipTileEditableRichTextBox_Loaded(object sender, RoutedEventArgs e) {
            var etrtb = (MpEditableTokenizedRichTextBox)sender;
            etrtb.ContextMenu = (ContextMenu)etrtb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
            etrtb.TokenizedRichTextBox.ContextMenu = etrtb.ContextMenu;
            //etrtb.TokenizedRichTextBox.Document = (MpEventEnabledFlowDocument)MpHelpers.ConvertXamlToFlowDocument(CopyItem.ItemXaml);
            //etrtb.TokenizedRichTextBox.IsDocumentEnabled = true;
            //etrtb.TokenizedRichTextBox.Document.IsEnabled = true;

            etrtb.Toolbar.IsVisibleChanged += (s, e1) => {
                var cb = (MpClipBorder)etrtb.GetVisualAncestor<MpClipBorder>();
                var trtb = (MpTokenizedRichTextBox)cb.FindName("TokenizedRichTextBox");
                var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
                var titleSwirl = (Image)cb.FindName("TitleSwirl");

                double fromWidthTile = 0;
                double toWidthTile = 0;
                double fromWidthContent = 0;
                double toWidthContent = 0;
                double scrollbarWidth = 20;

                if (etrtb.Toolbar.Visibility == Visibility.Visible) {
                    //etrtb.TokenizedRichTextBox.Document = (MpEventEnabledFlowDocument)etrtb.TokenizedRichTextBox.Document;
                    //etrtb.TokenizedRichTextBox.IsDocumentEnabled = true;
                    //etrtb.TokenizedRichTextBox.Document.IsEnabled = true;
                    fromWidthTile = MpMeasurements.Instance.ClipTileBorderSize;
                    fromWidthContent = fromWidthTile;
                    toWidthTile = Math.Max(625, etrtb.TokenizedRichTextBox.Document.GetFormattedText().WidthIncludingTrailingWhitespace);
                    toWidthContent = toWidthTile - scrollbarWidth;
                    etrtb.Focusable = true;
                    etrtb.TokenizedRichTextBox.Focusable = true;                    
                    etrtb.TokenizedRichTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    etrtb.TokenizedRichTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                    Console.WriteLine("Pre template richtext: ");
                    Console.WriteLine(RichText);
                } else {
                    // TODO add check to see if template token was added if not use convertflowdoctoricktext
                    //etrtb.TokenizedRichTextBox.Document = etrtb.TokenizedRichTextBox.GetTemplateDocument();
                    CopyItem.ItemXaml = MpHelpers.ConvertFlowDocumentToXaml(etrtb.TokenizedRichTextBox.GetTemplateDocument());
                    CopyItem.SubTextTokenList = etrtb.TokenizedRichTextBox.Tokens.ToList();
                    CopyItem.WriteToDatabase();
                    Console.WriteLine("Post template richtext: ");
                    Console.WriteLine(RichText);
                    fromWidthTile = etrtb.Width;
                    fromWidthContent = fromWidthTile - scrollbarWidth;
                    toWidthTile = MpMeasurements.Instance.ClipTileBorderSize;
                    toWidthContent = toWidthTile;
                    etrtb.Focusable = false;
                    etrtb.TokenizedRichTextBox.Focusable = false;
                    etrtb.TokenizedRichTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    etrtb.TokenizedRichTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
                double fromLeft = Canvas.GetLeft(titleIconImageButton);
                double toLeft = toWidthTile - TileTitleHeight - 10;

                DoubleAnimation twa = new DoubleAnimation();
                twa.From = fromWidthTile;
                twa.To = toWidthTile;
                twa.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
                CubicEase easing = new CubicEase();
                easing.EasingMode = EasingMode.EaseIn;
                twa.EasingFunction = easing;
                twa.Completed += (s1, e2) => {
                    ((MpMultiSelectListBox)cb.GetVisualAncestor<MpMultiSelectListBox>()).ScrollIntoView(cb);
                };
                cb.BeginAnimation(MpClipBorder.WidthProperty, twa);
                etrtb.BeginAnimation(MpEditableTokenizedRichTextBox.WidthProperty, twa);
                titleSwirl.BeginAnimation(Image.WidthProperty, twa);

                DoubleAnimation cwa = new DoubleAnimation();
                cwa.From = fromWidthContent;
                cwa.To = toWidthContent;
                cwa.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
                easing = new CubicEase();
                easing.EasingMode = EasingMode.EaseIn;
                cwa.EasingFunction = easing;
                etrtb.TokenizedRichTextBox.BeginAnimation(RichTextBox.WidthProperty, cwa);

                DoubleAnimation la = new DoubleAnimation();
                la.From = fromLeft;
                la.To = toLeft;
                la.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
                la.EasingFunction = easing;
                titleIconImageButton.BeginAnimation(Canvas.LeftProperty, la);
            };
        }

        public void ClipTileImage_Loaded(object sender, RoutedEventArgs e) {
            var img = (Image)sender;
            img.ContextMenu = (ContextMenu)img.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
            //aspect ratio
            double ar = Bmp.Width / Bmp.Height;
            double contentWidth = 0;
            double contentHeight = 0;
            if (Bmp.Width >= Bmp.Height) {
                contentWidth = TileBorderWidth;
                contentHeight = contentWidth * ar;
            } else {
                contentHeight = TileContentHeight;
                contentWidth = contentHeight * ar;
            }
            MpHelpers.ResizeBitmapSource(Bmp, new Size((int)contentWidth, (int)contentHeight));

            Canvas.SetLeft(img, (TileBorderWidth / 2) - (contentWidth / 2));
            Canvas.SetTop(img, (TileContentHeight / 2) - (contentHeight / 2));
        }

        public void ClipTileFileListBox_Loaded(object sender, RoutedEventArgs e) {
            var flb = (ListBox)sender;
            flb.ContextMenu = (ContextMenu)flb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
        }

        public void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            TagMenuItems.Clear();
            foreach (var tagTile in ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel) {
                if (tagTile.TagName == Properties.Settings.Default.HistoryTagTitle) {
                    continue;
                }
                TagMenuItems.Add(new MpClipTileContextMenuItemViewModel(tagTile.TagName, ClipTrayViewModel.LinkTagToCopyItemCommand, tagTile, tagTile.Tag.IsLinkedWithCopyItem(CopyItem)));
            }
            ConvertClipTypes.Clear();
            switch(ClipTrayViewModel.GetSelectedClipsType()) {
                case MpCopyItemType.None:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Text", ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.RichText, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("File List", ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.FileList, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Image", ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.Image, false));
                    break;
                case MpCopyItemType.FileList:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Text", ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.RichText, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Image", ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.Image, false));
                    break;
                case MpCopyItemType.Image:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Text", ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.RichText, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("File List", ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.FileList, false));
                    break;
                case MpCopyItemType.RichText:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("File List", ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.FileList, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Image", ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.Image, false));
                    break;
            }
        }

        public void AppendContent(MpClipTileViewModel octvm) {
            CopyItem.Combine(octvm.CopyItem);
            CopyItem.WriteToDatabase();

            //reinitialize item view properties
            PlainText = CopyItem.ItemPlainText;
            RichText = CopyItem.ItemRichText;
            DocumentRtf = CopyItem.ItemFlowDocument;
            Bmp = CopyItem.ItemBitmapSource;
            Tokens = new ObservableCollection<MpSubTextToken>(CopyItem.SubTextTokenList);
            FileDropList = CopyItem.GetFileList();
            FileListViewModels.Clear();
            foreach(var path in FileDropList) {
                FileListViewModels.Add(new MpFileListItemViewModel(this,path));
            }
        }

        public void InitSwirl(BitmapSource sharedSwirl = null) {
            if (sharedSwirl == null) {
                SolidColorBrush lighterColor = MpHelpers.ChangeBrushAlpha(
                                MpHelpers.ChangeBrushBrightness((SolidColorBrush)TitleColor, -0.5f), 100);
                SolidColorBrush darkerColor = MpHelpers.ChangeBrushAlpha(
                                MpHelpers.ChangeBrushBrightness((SolidColorBrush)TitleColor, -0.4f), 50);
                SolidColorBrush accentColor = MpHelpers.ChangeBrushAlpha(
                                MpHelpers.ChangeBrushBrightness((SolidColorBrush)TitleColor, -0.0f), 100);
                var path = @"pack://application:,,,/Resources/Images/";
                var swirl1 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0001.png"));
                swirl1 = MpHelpers.TintBitmapSource(swirl1, ((SolidColorBrush)TitleColor).Color);

                var swirl2 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0002.png"));
                swirl2 = MpHelpers.TintBitmapSource(swirl2, lighterColor.Color);

                var swirl3 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0003.png"));
                swirl3 = MpHelpers.TintBitmapSource(swirl3, darkerColor.Color);

                var swirl4 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0004.png"));
                swirl4 = MpHelpers.TintBitmapSource(swirl4, accentColor.Color);

                TitleSwirl = MpHelpers.MergeImages(new List<BitmapSource>() { swirl1, swirl2, swirl3, swirl4 });
            } else {
                TitleSwirl = sharedSwirl;
            }
        }

        public void DeleteTempFiles() {
            foreach (var f in _tempFileList) {
                if (File.Exists(f)) {
                    File.Delete(f);
                }
            }
        }

        public void Convert(MpCopyItemType newType) {
            CopyItem.ConvertType(newType);
            PlainText = CopyItem.ItemPlainText;
            RichText = CopyItem.ItemRichText;
            DocumentRtf = CopyItem.ItemFlowDocument;
            Bmp = CopyItem.ItemBitmapSource;
            FileListViewModels.Clear();
            foreach(var p in CopyItem.GetFileList()) {
                FileListViewModels.Add(new MpFileListItemViewModel(this,p));
            }

            FileListVisibility = CopyItemType == MpCopyItemType.FileList ? Visibility.Visible : Visibility.Collapsed;
            ImgVisibility = CopyItemType == MpCopyItemType.Image ? Visibility.Visible : Visibility.Collapsed;
            RtbVisibility = CopyItemType == MpCopyItemType.RichText ? Visibility.Visible : Visibility.Collapsed;
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
        private string GetCurrentDetail(int detailId) {
            string info = "I dunno";// string.Empty;
            switch (detailId) {
                //created
                case 0:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc
                    //TimeSpan dur = DateTime.Now - CopyItemCreatedDateTime;
                    info = "Copied " + CopyItemCreatedDateTime.ToString(); //dur.ToString();
                    break;
                //chars/lines
                case 1:
                    if (CopyItemType == MpCopyItemType.Image) {
                        info = "(" + Bmp.Width + ") x (" + Bmp.Height + ")";
                    } else if (CopyItemType == MpCopyItemType.RichText) {
                        info = CopyItem.ItemPlainText.Length + " chars | " + MpHelpers.GetRowCount(CopyItem.ItemPlainText) + " lines";
                    } else if (CopyItemType == MpCopyItemType.FileList) {
                        info = FileListViewModels.Count + " files | " + MpHelpers.FileListSize(CopyItem.GetFileList().ToArray()) + " bytes";
                    }
                    break;
                //# copies/# pastes
                case 2:
                    info = CopyItem.CopyCount + " copies | " + CopyItem.PasteCount + " pastes";
                    break;
                default:
                    info = "Unknown detailId: " + detailId;
                    break;
            }

            return info;
        }
        #endregion

        #region Commands

        private RelayCommand _editClipTextCommand;
        public ICommand EditClipTextCommand {
            get {
                if (_editClipTextCommand == null) {
                    _editClipTextCommand = new RelayCommand(EditClipText, CanEditClipText);
                }
                return _editClipTextCommand;
            }
        }
        private bool CanEditClipText() {
            return ClipTrayViewModel.SelectedClipTiles.Count == 1 && 
                  CopyItemType == MpCopyItemType.RichText && 
                  !IsEditingTile;
        }
        private void EditClipText() {
            IsEditingTile = true;
            //all other action is handled in the ertb visibility changed handler in ertb_loaded
        }

        private RelayCommand _commitEditClipTextCommand;
        public ICommand CommitEditClipTextCommand {
            get {
                if (_commitEditClipTextCommand == null) {
                    _commitEditClipTextCommand = new RelayCommand(CommitEditClipText, CanCommitEditClipText);
                }
                return _commitEditClipTextCommand;
            }
        }
        private bool CanCommitEditClipText() {
            return IsEditingTile;// !IsEditingTitle;
        }
        private void CommitEditClipText() {
            //RtbVisibility = Visibility.Visible;
            //ErtbVisibility = Visibility.Collapsed;
            IsEditingTile = false;
            //all other action is handled in the ertb visibility changed handler in ertb_loaded
        }

        private RelayCommand _shareClipCommand;
        public ICommand ShareClipCommand {
            get {
                if (_shareClipCommand == null) {
                    _shareClipCommand = new RelayCommand(ShareClip, CanShareClip);
                }
                return _shareClipCommand;
            }
        }
        private bool CanShareClip() {
            return ClipTrayViewModel.SelectedClipTiles.Count == 1;
        }
        private void ShareClip() {
            ClipTrayViewModel.MainWindowViewModel.IsShowingDialog = true;
            IntPtr windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            var dtmHelper = new DataTransferManagerHelper(windowHandle);
            dtmHelper.DataTransferManager.DataRequested += (s, e) => {
                DataPackage dp = e.Request.Data;
                dp.Properties.Title = Title;
                switch(CopyItemType) {
                    case MpCopyItemType.RichText:
                        dp.SetText(MpHelpers.ConvertRichTextToPlainText(this.RichText));
                        break;
                    case MpCopyItemType.FileList:
                    case MpCopyItemType.Image:
                        var filesToShare = new List<IStorageItem>();
                        foreach(string path in CopyItem.GetFileList()) {
                            //StorageFile sf = StorageFile.
                            //filesToShare.Add(sf);
                        }

                        dp.SetStorageItems(filesToShare);
                        break;
                }
            };

            dtmHelper.ShowShareUI();
            //ClipTrayViewModel.MainWindowViewModel.IsShowingDialog = false;
        }

        private RelayCommand _excludeApplicationCommand;
        public ICommand ExcludeApplicationCommand {
            get {
                if(_excludeApplicationCommand == null) {
                    _excludeApplicationCommand = new RelayCommand(ExcludeApplication,CanExcludeApplication);
                }
                return _excludeApplicationCommand;
            }
        }
        private bool CanExcludeApplication() {
            return ClipTrayViewModel.SelectedClipTiles.Count == 1;
        }
        private void ExcludeApplication() {
            MpAppCollectionViewModel.Instance.UpdateRejection(MpAppCollectionViewModel.Instance.GetAppViewModelByAppId(CopyItemAppId), true);
        }

        private RelayCommand _assignHotkeyCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if(_assignHotkeyCommand == null) {
                    _assignHotkeyCommand = new RelayCommand(AssignHotkey);
                }
                return _assignHotkeyCommand;
            }
        }
        private void AssignHotkey() {
            ShortcutKeyList = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(this, "Paste " + Title, ShortcutKeyList, PasteClipCommand);
        }

        private RelayCommand _pasteClipCommand;
        public ICommand PasteClipCommand {
            get {
                if (_pasteClipCommand == null) {
                    _pasteClipCommand = new RelayCommand(PasteClip);
                }
                return _pasteClipCommand;
            }
        }
        private void PasteClip() {
            ClipTrayViewModel.ClearClipSelection();
            IsSelected = true;
            ClipTrayViewModel.PasteSelectedClipsCommand.Execute(null);
        }

        private RelayCommand _runClipInShellCommand;
        public ICommand RunClipInShellCommand {
            get {
                if (_runClipInShellCommand == null) {
                    _runClipInShellCommand = new RelayCommand(RunClipInShell, CanRunClipInShell);
                }
                return _runClipInShellCommand;
            }
        }
        private bool CanRunClipInShell() {
            return CopyItemType == MpCopyItemType.RichText;
        }
        private void RunClipInShell() {
            MpHelpers.RunInShell(PlainText, false, false, IntPtr.Zero);
        }
        #endregion

        #region Overrides

        public override string ToString() {
            return CopyItem.ItemPlainText;
        }

        #endregion
    }
}
