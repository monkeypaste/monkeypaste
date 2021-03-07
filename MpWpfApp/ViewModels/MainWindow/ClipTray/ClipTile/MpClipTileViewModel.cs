namespace MpWpfApp {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interactivity;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using System.Windows.Threading;
    using AlphaChiTech.Virtualization;
    using AlphaChiTech.VirtualizingCollection;
    using AsyncAwaitBestPractices.MVVM;
    using GalaSoft.MvvmLight.CommandWpf;
    using GongSolutions.Wpf.DragDrop.Utilities;

    public class MpClipTileViewModel : MpUndoableViewModelBase<MpClipTileViewModel>, IDisposable {        
        #region Private Variables

        private int _detailIdx = 0;

        private List<string> _tempFileList = new List<string>();

        private string _origClipRichText = string.Empty;


        private bool _wasEditConfirmed = true;

        #endregion

        #region Statics
        #endregion

        #region Properties

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

        #region View Models
        private MpHighlightTextRangeViewModelCollection _highlightTextRangeViewModelCollection = new MpHighlightTextRangeViewModelCollection();
        public MpHighlightTextRangeViewModelCollection HighlightTextRangeViewModelCollection {
            get {
                return _highlightTextRangeViewModelCollection;
            }
            set {
                if (_highlightTextRangeViewModelCollection != value) {
                    _highlightTextRangeViewModelCollection = value;
                    OnPropertyChanged(nameof(HighlightTextRangeViewModelCollection));
                }
            }
        }

        private MpClipTileRichTextBoxViewModelCollection _richTextBoxViewModels = new MpClipTileRichTextBoxViewModelCollection();
        public MpClipTileRichTextBoxViewModelCollection RichTextBoxViewModelCollection {
            get {
                return _richTextBoxViewModels;
            }
            set {
                if (_richTextBoxViewModels != value) {
                    _richTextBoxViewModels = value;
                    OnPropertyChanged(nameof(RichTextBoxViewModelCollection));
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
                if (_tagMenuItems != value) {
                    _tagMenuItems = value;
                    OnPropertyChanged(nameof(TagMenuItems));
                }
            }
        }

        private ObservableCollection<MpFileListItemViewModel> _fileListViewModels = null;
        public ObservableCollection<MpFileListItemViewModel> FileListViewModels {
            get {
                if (CopyItem == null || CopyItemType != MpCopyItemType.FileList) {
                    return new ObservableCollection<MpFileListItemViewModel>();
                }
                if (_fileListViewModels == null) {
                    _fileListViewModels = new ObservableCollection<MpFileListItemViewModel>();
                    foreach (var path in CopyItem.GetFileList()) {
                        _fileListViewModels.Add(new MpFileListItemViewModel(this, path));
                    }
                }

                return _fileListViewModels;
            }
        }

        private MpDetectedImageObjectCollectionViewModel _detectedImageObjectCollectionViewModel = null;
        public MpDetectedImageObjectCollectionViewModel DetectedImageObjectCollectionViewModel {
            get {
                return _detectedImageObjectCollectionViewModel;
            }
            set {
                if (_detectedImageObjectCollectionViewModel != value) {
                    _detectedImageObjectCollectionViewModel = value;
                    OnPropertyChanged(nameof(DetectedImageObjectCollectionViewModel));
                }
            }
        }

        private MpEditTemplateToolbarViewModel _editTemplateToolbarViewModel = null;
        public MpEditTemplateToolbarViewModel EditTemplateToolbarViewModel {
            get {
                return _editTemplateToolbarViewModel;
            }
            set {
                if (_editTemplateToolbarViewModel != value) {
                    _editTemplateToolbarViewModel = value;
                    OnPropertyChanged(nameof(EditTemplateToolbarViewModel));
                }
            }
        }

        private MpPasteTemplateToolbarViewModel _pasteTemplateToolbarViewModel = null;
        public MpPasteTemplateToolbarViewModel PasteTemplateToolbarViewModel {
            get {
                return _pasteTemplateToolbarViewModel;
            }
            set {
                if (_pasteTemplateToolbarViewModel != value) {
                    _pasteTemplateToolbarViewModel = value;
                    OnPropertyChanged(nameof(PasteTemplateToolbarViewModel));
                }
            }
        }

        private MpEditRichTextBoxToolbarViewModel _editRichTextBoxToolbarViewModel = new MpEditRichTextBoxToolbarViewModel();
        public MpEditRichTextBoxToolbarViewModel EditRichTextBoxToolbarViewModel {
            get {
                return _editRichTextBoxToolbarViewModel;
            }
            set {
                if (_editRichTextBoxToolbarViewModel != value) {
                    _editRichTextBoxToolbarViewModel = value;
                    OnPropertyChanged(nameof(EditRichTextBoxToolbarViewModel));
                }
            }
        }

        public ObservableCollection<MpClipTileContextMenuItemViewModel> TranslateLanguageMenuItems {
            get {
                var translateLanguageMenuItems = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
                foreach (var languageName in MpLanguageTranslator.Instance.LanguageList) {
                    translateLanguageMenuItems.Add(new MpClipTileContextMenuItemViewModel(languageName, TranslateClipTextAsyncCommand, languageName, false));
                }
                return translateLanguageMenuItems;
            }
        }
        #endregion

        #region Controls
        private TextBox _titleTextBox;
        public TextBox TitleTextBox {
            get {
                return _titleTextBox;
            }
            set {
                if (_titleTextBox != value) {
                    _titleTextBox = value;
                    OnPropertyChanged(nameof(TitleTextBox));
                }
            }
        }

        private TextBlock _titleTextBlock;
        public TextBlock TitleTextBlock {
            get {
                return _titleTextBlock;
            }
            set {
                if (_titleTextBlock != value) {
                    _titleTextBlock = value;
                    OnPropertyChanged(nameof(TitleTextBlock));
                }
            }
        }

        private Image _image;
        public Image Image {
            get {
                return _image;
            }
            set {
                if(_image != value) {
                    _image = value;
                    OnPropertyChanged(nameof(Image));
                }
            }
        }

        private ListBox _richTextBoxListBox;
        public ListBox RichTextBoxListBox {
            get {
                return _richTextBoxListBox;
            }
            set {
                if(_richTextBoxListBox != value) {
                    _richTextBoxListBox = value;
                    OnPropertyChanged(nameof(RichTextBoxListBox));
                }
            }
        }

        private ListBox _fileListBox;
        public ListBox FileListBox {
            get {
                return _fileListBox;
            }
            set {
                if (_fileListBox != value) {
                    _fileListBox = value;
                    OnPropertyChanged(nameof(FileListBox));
                }
            }
        }

        public Canvas RtbListBoxCanvas { get; set; }

        private MpClipBorder _clipBorder;
        public MpClipBorder ClipBorder {
            get {
                return _clipBorder;
            }
            set {
                if(_clipBorder != value) {
                    _clipBorder = value;
                    OnPropertyChanged(nameof(ClipBorder));
                }
            }
        }
        #endregion

        #region Layout Properties
        public double EditTemplateToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTileEditTemplateToolbarHeight;
            }
        }

        public double EditRichTextBoxToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTileEditToolbarHeight;
            }
        }

        public double PasteTemplateToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
            }
        }        

        private FontFamily _rtbFontFamily = null;
        public FontFamily RtbFontFamily {
            get {
                return _rtbFontFamily;
            }
            set {
                if (_rtbFontFamily != value) {
                    _rtbFontFamily = value;
                    OnPropertyChanged(nameof(RtbFontFamily));
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

        private double _tileTitleIconBorderSize = MpMeasurements.Instance.ClipTileTitleIconBorderSize;
        public double TileTitleIconBorderSize {
            get {
                return _tileTitleIconBorderSize;
            }
            set {
                if (_tileTitleIconBorderSize != value) {
                    _tileTitleIconBorderSize = value;
                    OnPropertyChanged(nameof(TileTitleIconBorderSize));
                }
            }
        }

        public double TileBorderMinWidth {
            get {
                return MpMeasurements.Instance.ClipTileBorderMinSize;
            }
        }

        public double TileBorderMaxWidth {
            get {
                return MpMeasurements.Instance.ClipTileBorderMinSize;
            }
        }

        public double TileBorderWidth {
            get {
                if(IsExpanded) {
                    return TileBorderMaxWidth;
                }
                return TileBorderMinWidth;
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

        private double _tileTitleTextGridWidth;
        public double TileTitleTextGridWidth {
            get {
                return _tileTitleTextGridWidth;
            }
            set {
                if(_tileTitleTextGridWidth != value) {
                    _tileTitleTextGridWidth = value;
                    OnPropertyChanged(nameof(TileTitleTextGridWidth));
                }
            }
        }

        public double TileContentHeight {
            get {
                return MpMeasurements.Instance.ClipTileContentHeight;
            }
        }

        public double TileDetailHeight {
            get {
                return MpMeasurements.Instance.ClipTileDetailHeight;
            }
        }

        public double TileContentMinWidth {
            get {
                return MpMeasurements.Instance.ClipTileContentWidth;
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

        public double LoadingSpinnerSize {
            get {
                return MpMeasurements.Instance.ClipTileLoadingSpinnerSize;
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

        public double TileWidthMax {
            get {
                if(CopyItem == null) {
                    return 0;
                }
                return Math.Max(MpMeasurements.Instance.ClipTileEditModeMinWidth, RichTextBoxViewModelCollection.RelativeWidthMax);
            }
        }

        public double TitleFontSize {
            get {
                return MpMeasurements.Instance.ClipTileTitleFontSize;
            }
        }
        #endregion

        #region Visibility Properties
        public Visibility ClipTileTitleAppIconVisibility {
            get {
                return IsExpanded ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private Visibility _editToolbarVisibility = Visibility.Collapsed;
        public Visibility EditToolbarVisibility {
            get {
                return _editToolbarVisibility;
            }
            set {
                if(_editToolbarVisibility != value) {
                    _editToolbarVisibility = value;
                    OnPropertyChanged(nameof(EditToolbarVisibility));
                }
            }
        }

        private Visibility _editTemplateToolbarVisibility = Visibility.Collapsed;
        public Visibility EditTemplateToolbarVisibility {
            get {
                return _editTemplateToolbarVisibility;
            }
            set {
                if(_editTemplateToolbarVisibility != value) {
                    _editTemplateToolbarVisibility = value;
                    OnPropertyChanged(nameof(EditTemplateToolbarVisibility));
                }
            }
        }

        private Visibility _pasteTemplateToolbarVisibility = Visibility.Collapsed;
        public Visibility PasteTemplateToolbarVisibility {
            get {
                return _pasteTemplateToolbarVisibility;
            }
            set {
                if(_pasteTemplateToolbarVisibility != value) {
                    _pasteTemplateToolbarVisibility = value;
                    OnPropertyChanged(nameof(PasteTemplateToolbarVisibility));
                }
            }
        }

        public ScrollBarVisibility RtbHorizontalScrollbarVisibility {
            get {
                if(IsExpanded) {
                    if(RichTextBoxViewModelCollection.RelativeWidthMax > ClipBorder.ActualWidth) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility RtbVerticalScrollbarVisibility {
            get {
                if (IsExpanded) {
                    if (RichTextBoxViewModelCollection.TotalItemHeight > RichTextBoxListBox.ActualHeight - EditRichTextBoxToolbarHeight) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public Visibility ImgVisibility {
            get {
                if (!IsLoading && CopyItemType == MpCopyItemType.Image) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility FileListVisibility {
            get {
                if (!IsLoading && CopyItemType == MpCopyItemType.FileList) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility RtbVisibility {
            get {
                if (!IsLoading && (CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Composite)) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility LoadingSpinnerVisibility {
            get {
                if (IsLoading) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

         public Visibility ContentVisibility {
            get {
                if (IsLoading) {
                    return Visibility.Collapsed;
                } else if (IsExpanding) {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }
        private Visibility _containerVisibility = Visibility.Visible;
        public Visibility ContainerVisibility {
            get {
                return _containerVisibility;
            }
            set {
                if (_containerVisibility != value) {
                    _containerVisibility = value;
                    OnPropertyChanged(nameof(ContainerVisibility));
                }
            }
        }
        public Visibility TileTitleTextBlockVisibility {
            get {
                if (IsEditingTitle) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public Visibility TileTitleTextBoxVisibility {
            get {
                if (IsEditingTitle) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility TileDetectedImageItemsVisibility {
            get {
                if(IsSelected) {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
        }

        public Visibility TileDetailGridVisibility {
            get {
                if (IsEditingTile || IsEditingTemplate || IsPastingTemplateTile) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        private Visibility _tileVisibility = Visibility.Visible;
        public Visibility TileVisibility {
            get {
                return _tileVisibility;
            }
            set {
                if (_tileVisibility != value) 
                {
                    _tileVisibility = value;
                    OnPropertyChanged(nameof(TileVisibility));
                }
            }
        }

        public Visibility TrialOverlayVisibility {
            get {
                return IsTrialExpired ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility MenuOverlayVisibility {
            get {
                return Visibility.Collapsed;
                //return IsHovering ? Visibility.Visible : Visibility.Hidden;
            }
        }

        #endregion

        #region Business Logic Properties
        public string TemplateRichText { get; set; }

        private bool _wasAddedAtRuntime = false;
        public bool WasAddedAtRuntime {
            get {
                return _wasAddedAtRuntime;
            }
            set {
                if(_wasAddedAtRuntime != value) {
                    _wasAddedAtRuntime = value;
                    OnPropertyChanged(nameof(WasAddedAtRuntime));
                }
            }
        }

        //private bool _isLoading = true;
        public bool IsLoading {
            get {
                //if(MainWindowViewModel != null && MainWindowViewModel.IsLoading) {
                //    return true;
                //}
                //if(MainWindowViewModel != null && MainWindowViewModel.ClipTrayViewModel != null && MainWindowViewModel.ClipTrayViewModel.IsLoading) {
                //    return true;
                //}
                //if( CopyItem == null || CopyItem.CopyItemId == 0) {
                //    return true;
                //}
                //return _isLoading;
                return CopyItem == null || CopyItem.CopyItemId == 0;
            }
            //set {
            //    if(_isLoading != value) {
            //        _isLoading = value;
            //        OnPropertyChanged(nameof(IsLoading));
            //    }
            //}
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

        public Cursor ContentCursor {
            get {
                if (IsSelected && (CopyItemType == MpCopyItemType.RichText || 
                    CopyItemType == MpCopyItemType.Composite)) {
                    return Cursors.IBeam;
                }
                return Cursors.Arrow;
            }
        }
        #endregion

        #region Brush Properties        
        public Brush DetailTextColor {
            get {
                if (IsSelected) {
                    return Brushes.DarkGray;
                }
                if (IsHovering) {
                    return Brushes.DimGray;
                }
                return Brushes.Transparent;
            }
        }

        public Brush TileBorderBrush {
            get {
                if(IsPrimarySelected) {
                    return Brushes.Blue;
                }
                if (IsSelected) {
                    return Brushes.Red;
                }
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }

        public Brush TileTitleTextGridBackgroundBrush {
            get {
                if(IsHoveringOnTitleTextGrid && !IsEditingTitle) {
                    return new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region State Properties           

        public bool IsDragging { get; set; } = false;
                
        public Point StartDragPoint;
        
        private bool _isMouseDown = false;
        public bool IsMouseDown {
            get {
                return _isMouseDown;
            }
            set {
                if (_isMouseDown != value) {
                    _isMouseDown = value;
                    OnPropertyChanged(nameof(IsMouseDown));
                }
            }
        }

        private bool _isHoveringOnTitleTextGrid = false;
        public bool IsHoveringOnTitleTextGrid {
            get {
                return _isHoveringOnTitleTextGrid;
            }
            set {
                if (_isHoveringOnTitleTextGrid != value) {
                    _isHoveringOnTitleTextGrid = value;
                    OnPropertyChanged(nameof(IsHoveringOnTitleTextGrid));
                    OnPropertyChanged(nameof(TileTitleTextGridBackgroundBrush));
                    OnPropertyChanged(nameof(TitleTextColor));
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
                    OnPropertyChanged(nameof(TileTitleTextBlockVisibility));
                    OnPropertyChanged(nameof(TileTitleTextBoxVisibility));
                    OnPropertyChanged(nameof(TileTitleTextGridBackgroundBrush));
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
                    OnPropertyChanged(nameof(IsRtbReadOnly));
                    OnPropertyChanged(nameof(ContentCursor));
                    OnPropertyChanged((nameof(CopyItemRichText)));
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(TileDetailGridVisibility));
                    OnPropertyChanged(nameof(TileContentHeight));
                    OnPropertyChanged(nameof(IsExpanded));
                    OnPropertyChanged(nameof(ClipTileTitleAppIconVisibility));
                }
            }
        }

        private bool _isEditingTemplate = false;
        public bool IsEditingTemplate {
            get {
                return _isEditingTemplate;
            }
            set {
                if (_isEditingTemplate != value) {
                    _isEditingTemplate = value;
                    OnPropertyChanged(nameof(IsEditingTemplate));
                    OnPropertyChanged(nameof(TileDetailGridVisibility));
                    OnPropertyChanged(nameof(TileContentHeight));
                    OnPropertyChanged(nameof(IsExpanded));
                    OnPropertyChanged(nameof(ClipTileTitleAppIconVisibility));
                }
            }
        }

        private bool _isPastingTemplateTile = false;
        public bool IsPastingTemplateTile {
            get {
                return _isPastingTemplateTile;
            }
            set {
                if (_isPastingTemplateTile != value) {
                    _isPastingTemplateTile = value;
                    OnPropertyChanged(nameof(IsPastingTemplateTile));
                    OnPropertyChanged(nameof(TileDetailGridVisibility));
                    OnPropertyChanged(nameof(TileContentHeight));
                    OnPropertyChanged(nameof(IsExpanded));
                    OnPropertyChanged(nameof(ClipTileTitleAppIconVisibility));
                }
            }
        }

        private bool _isExpanding = false;
        public bool IsExpanding {
            get {
                return _isExpanding;
            }
            set {
                if(_isExpanding != value) {
                    _isExpanding = value;
                    OnPropertyChanged(nameof(IsExpanding));
                    OnPropertyChanged(nameof(ContentVisibility));
                }
            }
        }

        public bool IsExpanded {
            get {
                if(IsPastingTemplateTile || IsEditingTemplate || IsEditingTile) {
                    return true;
                }
                return false;
            }
        }

        public bool HasTemplate {
            get {
                foreach(var rtbvm in RichTextBoxViewModelCollection) {
                    if(rtbvm.HasTemplate) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsRtbReadOnly {
            get {
                return !IsEditingTile;
            }
        }

        private DateTime _lastSelectedDateTime;
        public DateTime LastSelectedDateTime {
            get {
                return _lastSelectedDateTime;
            }
            set {
                if(_lastSelectedDateTime != value) {
                    _lastSelectedDateTime = value;
                    OnPropertyChanged(nameof(LastSelectedDateTime));
                }
            }
        }

        private bool _isPrimarySelected = false;
        public bool IsPrimarySelected {
            get {
                return _isPrimarySelected;
            }
            set {
                if (_isPrimarySelected != value) {
                    _isPrimarySelected = value;
                    OnPropertyChanged(nameof(IsPrimarySelected));
                    OnPropertyChanged(nameof(TileBorderBrush));
                }
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
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(DetailTextColor));
                    OnPropertyChanged(nameof(TileDetectedImageItemsVisibility));
                    OnPropertyChanged(nameof(ContentCursor));
                }                
            }
        }

        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value && !IsEditingTitle) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(DetailTextColor));
                    OnPropertyChanged(nameof(MenuOverlayVisibility));
                }
            }
        }
        #endregion

        #region Focus Properties
        private bool _isClipRichTextBoxFocused = false;
        public bool IsClipRichTextBoxFocused {
            get {
                return _isClipRichTextBoxFocused;
            }
            set {
                //omitting duplicate check to enforce change in ui
                //if (_isClipItemFocused != value) 
                {
                    _isClipRichTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsClipRichTextBoxFocused));
                }
            }
        }

        private bool _isClipItemFocused = false;
        public bool IsClipItemFocused {
            get {
                return _isClipItemFocused;
            }
            set {
                //omitting duplicate check to enforce change in ui
                //if (_isClipItemFocused != value) 
                {
                    _isClipItemFocused = value;
                    OnPropertyChanged(nameof(IsClipItemFocused));
                }
            }
        }
        #endregion

        #region Model
        public string DetailText {
            get {
                if(CopyItem == null) {
                    return string.Empty;
                }
                _detailIdx++;
                if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    _detailIdx = 0;
                }
                return CopyItem.GetDetail((MpCopyItemDetailType)_detailIdx);
            }
        }

        public int CopyCount {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CopyCount;
            }
            set {
                if(CopyItem != null && CopyItem.CopyCount != value) {
                    CopyItem.CopyCount = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyCount));
                }
            }
        }

        public int PasteCount {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.PasteCount;
            }
            set {
                if(CopyItem != null && CopyItem.PasteCount != value) {
                    CopyItem.PasteCount = value;
                    OnPropertyChanged(nameof(PasteCount));
                }
            }
        }

        public int LineCount {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.LineCount;
            }
        }

        public int CharCount {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CharCount;
            }
        }

        public int FileCount {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.FileCount;
            }
        }

        public double DataSizeInMb {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.DataSizeInMb;
            }
        }

        public Brush TitleBackgroundColor {
            get {
                if (CopyItem == null) {
                    return Brushes.Transparent;
                }
                return new SolidColorBrush(CopyItem.ItemColor.Color);
            }
            set {
                if (CopyItem != null && CopyItem.ItemColor.Color != ((SolidColorBrush)value).Color) {
                    CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                    //CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(TitleBackgroundColor));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public Brush TitleTextColor {
            get {
                if (IsHoveringOnTitleTextGrid) {
                    return Brushes.DarkGray;
                }
                return Brushes.White;
            }
        }

        private string _shortcutKeyString = string.Empty;
        public string ShortcutKeyString {
            get {
                return _shortcutKeyString;
            }
            set {
                if (_shortcutKeyString != value) {
                    _shortcutKeyString = value;
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            }
        }

        public int CopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CopyItemId;
            }
        }

        public string CopyItemTitle {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Title;
            }
            set {
                if (CopyItem != null && CopyItem.Title != value) {
                    AddUndo(this, nameof(CopyItemTitle), CopyItem.Title, value);
                    CopyItem.Title = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemTitle));
                }
            }
        }

        public string CopyItemPlainText {
            get {
                if (CopyItem == null || CopyItem.ItemPlainText == null) {
                    return string.Empty;
                }
                return CopyItem.ItemPlainText;
            }
        }

        public string CopyItemRichText {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(CopyItem.ItemRichText)) {
                    return string.Empty.ToRichText();
                }
                return CopyItem.ItemRichText;
            }
            set {
                if (CopyItem != null && CopyItem.ItemRichText != value) {
                    //value should be raw rtf where templates are encoded into #name#color# groups
                    CopyItem.SetData(value);
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemRichText));
                    OnPropertyChanged(nameof(CharCount));
                    OnPropertyChanged(nameof(LineCount));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public BitmapSource CopyItemBmp {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                return CopyItem.ItemBitmapSource;
            }
        }

        public List<string> CopyItemFileDropList {
            get {
                if (CopyItem == null || MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null) {
                    return new List<string>();
                }
                return CopyItem.GetFileList(string.Empty, MainWindowViewModel.ClipTrayViewModel.GetTargetFileType());
            }
        }

        public BitmapSource TitleSwirl {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                return CopyItem.ItemTitleSwirl;
            }
            set {
                if (CopyItem != null && CopyItem.ItemTitleSwirl != value) {
                    CopyItem.ItemTitleSwirl = value;
                    OnPropertyChanged(nameof(TitleSwirl));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public BitmapSource CopyItemAppIcon {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                return CopyItem.App.IconImage;
            }
        }

        public BitmapSource CopyItemAppIconBorder {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                return CopyItem.App.IconBorderImage;
            }
        }

        public string CopyItemAppName {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.App.AppName;
            }
        }

        public DateTime CopyDateTime {
            get {
                if (CopyItem == null) {
                    return DateTime.Now;
                }
                return CopyItem.CopyDateTime;
            }
            set {
                if (CopyItem != null && CopyItem.CopyDateTime != value) {
                    CopyItem.CopyDateTime = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyCount));
                }
            }
        }
        public int CopyItemUsageScore {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.RelevanceScore;
            }
        }

        public int CopyItemAppId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.App.AppId;
            }
        }

        public MpCopyItemType CopyItemType {
            get {
                if (CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.CopyItemType;
            }
        }

        public DateTime CopyItemCreatedDateTime {
            get {
                if (CopyItem == null) {
                    return DateTime.Now;
                }
                return CopyItem.CopyDateTime;
            }
        }

        private MpCopyItem _copyItem = null;
        public MpCopyItem CopyItem {
            get {
                if(_copyItem == null) {
                    //only happens for non-composite richtext types
                    if(RichTextBoxViewModelCollection != null &&
                       RichTextBoxViewModelCollection.Count > 0) {
                        //for non-composite clip tiles use rtblb's first (and only) element
                        return RichTextBoxViewModelCollection[0].CopyItem;
                    }
                }
                return _copyItem;
            }
            private set {
                //if (_copyItem != value) 
                {
                    if(CopyItem != null && 
                       CopyItemType == MpCopyItemType.RichText) {
                        if(value != null && value.CopyItemType == MpCopyItemType.Composite) {
                            _copyItem = value;
                        } else {
                            RichTextBoxViewModelCollection[0].CopyItem = value;
                        }                            
                    } else {
                        _copyItem = value;
                    }
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CopyItemId));
                    OnPropertyChanged(nameof(CopyItemType));
                    OnPropertyChanged(nameof(ImgVisibility));
                    OnPropertyChanged(nameof(RtbVisibility));
                    OnPropertyChanged(nameof(FileListVisibility));
                    OnPropertyChanged(nameof(CopyItemTitle));
                    OnPropertyChanged(nameof(CopyItemPlainText));
                    OnPropertyChanged(nameof(CopyItemRichText));
                    OnPropertyChanged(nameof(CopyItemBmp));
                    OnPropertyChanged(nameof(CopyItemFileDropList));
                    OnPropertyChanged(nameof(CopyItemAppIcon));
                    OnPropertyChanged(nameof(CopyItemAppName));
                    OnPropertyChanged(nameof(CopyItemUsageScore));
                    OnPropertyChanged(nameof(CopyItemAppId));
                    OnPropertyChanged(nameof(TitleSwirl));
                    OnPropertyChanged(nameof(CopyItemCreatedDateTime));
                    OnPropertyChanged(nameof(DetailText));
                    OnPropertyChanged(nameof(FileListViewModels));
                    OnPropertyChanged(nameof(CopyCount));
                    OnPropertyChanged(nameof(PasteCount));
                    OnPropertyChanged(nameof(CharCount));
                    OnPropertyChanged(nameof(LineCount));
                    OnPropertyChanged(nameof(FileCount));
                    OnPropertyChanged(nameof(DataSizeInMb));
                    OnPropertyChanged(nameof(IsLoading));
                    OnPropertyChanged(nameof(DetectedImageObjectCollectionViewModel));
                    OnPropertyChanged(nameof(LoadingSpinnerVisibility));
                    OnPropertyChanged(nameof(ContentVisibility));
                    OnPropertyChanged(nameof(TrialOverlayVisibility));
                    OnPropertyChanged(nameof(RichTextBoxViewModelCollection));
                    OnPropertyChanged(nameof(TitleFontSize));
                    //CopyItem.WriteToDatabase();
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileViewModel(bool isPlaceholder) : base() {
            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(IsSelected):
                        if (IsSelected) {
                            LastSelectedDateTime = DateTime.Now;
                            //this check ensures that as user types in search that 
                            //resetselection doesn't take the focus from the search box
                            if (!MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                                IsClipItemFocused = true;
                            }
                        } else {
                            IsEditingTile = false;
                            IsEditingTemplate = false;
                            foreach(var rtbvm in RichTextBoxViewModelCollection) {
                                rtbvm.IsEditingSubTitle = false;
                            }
                            //IsPastingTemplateTile = false;
                        }
                        RefreshCommands();
                        break;
                    case nameof(IsHovering):
                        if(IsHovering) {
                            foreach(MpClipTileViewModel ctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
                                if(ctvm != this) {
                                    ctvm.IsHovering = false;
                                }
                            }
                        }
                        break;
                    case nameof(IsEditingTile):
                        //if (IsEditingTile) {
                        //    //RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(0, true, false);
                        //    MainWindowViewModel.ClipTrayViewModel.ExpandClipTile(this, false);
                        //} else {
                        //    MainWindowViewModel.ClipTrayViewModel.ShrinkClipTile(this, false);
                        //}
                        break;
                    case nameof(IsPastingTemplateTile):
                        //RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(0, false, true);
                        break;
                }
            };
            RichTextBoxViewModelCollection = new MpClipTileRichTextBoxViewModelCollection(this);
            EditRichTextBoxToolbarViewModel = new MpEditRichTextBoxToolbarViewModel(this);
            EditTemplateToolbarViewModel = new MpEditTemplateToolbarViewModel(this);
            PasteTemplateToolbarViewModel = new MpPasteTemplateToolbarViewModel(this);
            HighlightTextRangeViewModelCollection = new MpHighlightTextRangeViewModelCollection(this);

            if (isPlaceholder) {
                CopyItem = null;
            }
        }

        public MpClipTileViewModel(MpCopyItem ci) : this(false) {
            if (ci == null) {
                //throw new Exception("MpClipTileViewModel error, cannot set null copyitem");
                CopyItem = ci;
                return;
            }
            if (ci.CopyItemId == 0 && !MainWindowViewModel.IsLoading) {
                ci.WriteToDatabase();
                _wasAddedAtRuntime = true;
            }
            if (ci.CopyItemType == MpCopyItemType.RichText) {
                if (RichTextBoxViewModelCollection.Count == 0) {
                    RichTextBoxViewModelCollection.Add(new MpRtbListBoxItemRichTextBoxViewModel(this, ci));
                    OnPropertyChanged(nameof(CopyItem));
                }
            } else {
                CopyItem = ci;
            }
            if (MainWindowViewModel != null) {
                //is null during loading and the refresh isn't needed
                MainWindowViewModel.ClipTrayViewModel.Refresh();
            }
        }

        #region Loading Initializers
        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            ClipBorder = (MpClipBorder)sender;
            var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");

            ClipBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            ClipBorder.MouseLeave += (s, e2) => {
                IsHovering = false;
            };
            ClipBorder.LostFocus += (s, e4) => {
                if (!IsSelected) {
                    IsEditingTitle = false;
                }
            };
            //ClipBorder.PreviewMouseLeftButtonDown += (s, e5) => {
            //    if (e5.ClickCount == 2 && !IsEditingTile) {
            //        //only for richtext type
            //        ToggleEditClipCommand.Execute(null);
            //        e5.Handled = true;
            //        return;
            //    }
            //};
            
            //ClipBorder.PreviewKeyDown += (s, e6) => {
            //    if (CopyItemType != MpCopyItemType.RichText) {
            //        return;
            //    }
            //    //var rtb = (RichTextBox)((FrameworkElement)s).FindName("ClipTileRichTextBox");
            //    //if (e6.Key == Key.Down) {
            //    //    rtb.FontSize -= 1;
            //    //    e.Handled = true;
            //    //} else if (e6.Key == Key.Up) {
            //    //    rtb.FontSize += 1;
            //    //    e.Handled = true;
            //    //}
            //};

            #region Tile Drag & Drop (all unused atm)
            ClipBorder.PreviewMouseLeftButtonDown += (s, e5) => {
                return;
                IsMouseDown = true;
                StartDragPoint = e5.GetPosition(clipTray);
                //_dragClipBorderElement = (MpClipBorder)VisualTreeHelper.HitTest(clipTray, StartDragPoint).VisualHit.GetVisualAncestor<MpClipBorder>(); ;
            };
            //Initiate Selected Clips Drag/Drop, Copy/Paste and Export (to file or csv)
            //Strategy: ALL selected items, regardless of type will have text,rtf,img, and file representations
            //          that are appended as text and filelists but  merged into images (by default)
            // TODO Have option to append items to one long image
            ClipBorder.PreviewMouseMove += (s, e7) => {
                return;
                var curDragPoint = e7.GetPosition(clipTray);
                //these tests ensure tile is not being dragged INTO another clip tile or outside tray
                //var testBorder = (MpClipBorder)VisualTreeHelper.HitTest(clipTray, curDragPoint).VisualHit.GetVisualAncestor<MpClipBorder>();
                //var testTray = (ListBox)VisualTreeHelper.HitTest(clipTray, curDragPoint).VisualHit.GetVisualAncestor<ListBox>();
                if (IsMouseDown &&
                    !IsDragging &&
                    (MainWindowViewModel != null && 
                     MainWindowViewModel.ClipTrayViewModel != null && 
                     !MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded) &&
                    e7.MouseDevice.LeftButton == MouseButtonState.Pressed &&
                    (Math.Abs(curDragPoint.Y - StartDragPoint.Y) > 5 || Math.Abs(curDragPoint.X - StartDragPoint.X) > 5) /*&&
                   // s.GetType() == typeof(MpClipBorder) &&
                    //_dragClipBorderElement != testBorder &&
                    testBorder == null &&
                    testTray != null*/) {
                    DragDrop.DoDragDrop(clipTray, MainWindowViewModel.ClipTrayViewModel.GetDataObjectFromSelectedClips(true), DragDropEffects.Copy | DragDropEffects.Move);
                    IsDragging = true;
                } else if (IsDragging) {
                    IsMouseDown = false;
                    IsDragging = false;
                    StartDragPoint = new Point();
                    //_dragClipBorderElement = null;
                }
            };

            ClipBorder.PreviewMouseLeftButtonUp += (s, e8) => {
                return;
                IsMouseDown = false;
                IsDragging = false;
                StartDragPoint = new Point();
                //_dragClipBorderElement = null;
            };
            //clipTileBorder.PreviewMouseRightButtonUp += (s, e4) => {
            //    var p = e4.MouseDevice.GetPosition(clipTileBorder);
            //    var hitTestResult = VisualTreeHelper.HitTest(clipTileBorder, p);
            //    if (hitTestResult != null) {
            //        MessageBox.Show(hitTestResult.VisualHit.ToString());
            //    }
            //};
            #endregion

            #region Composite Drag & Drop
            //clipTileBorder.PreviewMouseMove += RichTextBoxViewModelCollection.ClipTileRichTextBoxViewModel_PreviewMouseMove;
            //clipTileBorder.GiveFeedback += RichTextBoxViewModelCollection.ClipTileRichTextBoxViewModel_GiveFeedback;
            //clipTileBorder.Drop += RichTextBoxViewModelCollection.ClipTileRichTextBoxViewModel_Drop;
            #endregion

            HighlightTextRangeViewModelCollection.Init();
        }

        public void ClipTileDetailGrid_Loaded(object sender, RoutedEventArgs e) {
            var detailGrid = (Grid)sender;
            var titleDetailTextBlock = (TextBlock)detailGrid.FindName("ClipTileTitleDetailTextBlock");

            titleDetailTextBlock.MouseEnter += (s, e5) => {
                OnPropertyChanged(nameof(DetailText));
            };
        }        

        public void ClipTileTitle_Loaded(object sender, RoutedEventArgs e) {
            var titleCanvas = (Canvas)sender;
            var titleTextGrid = (Grid)titleCanvas.FindName("ClipTileTitleTextGrid");
            var clipTileTitleTextBlock = (TextBlock)titleCanvas.FindName("ClipTileTitleTextBlock");
            var clipTileTitleTextBox = (TextBox)titleCanvas.FindName("ClipTileTitleTextBox");
            var titleIconImageButton = (Button)titleCanvas.FindName("ClipTileAppIconImageButton");
            var titleIconImageButtonRotateTransform = (RotateTransform)titleIconImageButton.FindName("ClipTileAppIconImageButtonRotateTransform");
            var titleIconBorderImage = (Image)titleCanvas.FindName("ClipTileAppIconBorderImage");
            var titleIconBorderImageScaleTransform = (ScaleTransform)titleCanvas.FindName("ClipTileAppIconBorderImageScaleTransform");

            RenderOptions.SetBitmapScalingMode(titleIconBorderImage, BitmapScalingMode.LowQuality);

            TitleTextBlock = clipTileTitleTextBlock;
            TitleTextBox = clipTileTitleTextBox;

            titleTextGrid.MouseEnter += (s, e1) => {
                Application.Current.MainWindow.Cursor = Cursors.IBeam;
                IsHoveringOnTitleTextGrid = true;
            };
            titleTextGrid.MouseLeave += (s, e7) => {
                Application.Current.MainWindow.Cursor = Cursors.Arrow;
                IsHoveringOnTitleTextGrid = false;
            };
            titleTextGrid.PreviewMouseLeftButtonDown += (s, e7) => {
                IsEditingTitle = true;
                e7.Handled = true;
            };
            Canvas.SetLeft(titleTextGrid, MpMeasurements.Instance.ClipTileTitleTextGridCanvasLeft);
            Canvas.SetTop(titleTextGrid, MpMeasurements.Instance.ClipTileTitleTextGridCanvasTop);
            titleTextGrid.Width = MpMeasurements.Instance.ClipTileTitleTextGridWidth;

            clipTileTitleTextBox.IsVisibleChanged += (s, e9) => {
                if (TileTitleTextBoxVisibility == Visibility.Collapsed) {
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                tbx.SelectAll();
            };
            clipTileTitleTextBox.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };
            clipTileTitleTextBox.PreviewKeyDown += (s, e5) => {
                if(e5.Key == Key.Enter || e5.Key == Key.Escape) {
                    IsEditingTitle = false;
                }
            };
            
            Canvas.SetLeft(titleIconImageButton, MpMeasurements.Instance.ClipTileTitleIconCanvasLeft);
            Canvas.SetTop(titleIconImageButton, 2);
            
            titleIconImageButton.MouseEnter += (s, e3) => {
                if(IsEditingTemplate || IsPastingTemplateTile) {
                    return;
                }
                double t = 100;
                double angle = 15;
                var a = new DoubleAnimation(0, angle, new Duration(TimeSpan.FromMilliseconds(t)));
                a.Completed += (s1, e1) => {
                    var b = new DoubleAnimation(angle, -angle, new Duration(TimeSpan.FromMilliseconds(t * 2)));
                    b.Completed += (s2, e2) => {
                        var c = new DoubleAnimation(-angle, 0, new Duration(TimeSpan.FromMilliseconds(t)));
                        titleIconImageButtonRotateTransform.BeginAnimation(RotateTransform.AngleProperty, c);
                    };
                    titleIconImageButtonRotateTransform.BeginAnimation(RotateTransform.AngleProperty, b);
                };

                titleIconImageButtonRotateTransform.BeginAnimation(RotateTransform.AngleProperty, a);

                titleIconBorderImage.Visibility = Visibility.Visible;
                double fromScale = 1;
                double toScale = 1.1;
                double st = 300;
                var sa = new DoubleAnimation(fromScale, toScale, new Duration(TimeSpan.FromMilliseconds(st)));
                var easing = new CubicEase();
                easing.EasingMode = EasingMode.EaseOut;
                sa.EasingFunction = easing;
                titleIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                titleIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
            };
            titleIconImageButton.MouseLeave += (s, e3) => {
                if (IsEditingTemplate || IsPastingTemplateTile) {
                    return;
                }
                double fromScale = 1.15;
                double toScale = 1;
                double st = 300;
                var sa = new DoubleAnimation(fromScale, toScale, new Duration(TimeSpan.FromMilliseconds(st)));
                sa.Completed += (s1, e31) => {
                    titleIconBorderImage.Visibility = Visibility.Hidden;
                };
                var easing = new CubicEase();
                easing.EasingMode = EasingMode.EaseIn;
                sa.EasingFunction = easing;
                titleIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                titleIconBorderImageScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
            };
            titleIconImageButton.PreviewMouseLeftButtonUp += (s, e7) => {
                // TODO (somehow) force mainwindow to stay active when switching or opening app process
                // TODO check if shift is down if so perform paste into target application
                // TODO check if App is running if it is switch to it or start its process

                //MpHelpers.Instance.OpenUrl(CopyItem.App.AppPath);
                MainWindowViewModel.ClipTrayViewModel.IsFilteringByApp = true;
                foreach (var vctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
                    if(vctvm.CopyItemAppId != CopyItemAppId) {
                        vctvm.TileVisibility = Visibility.Collapsed;
                    }
                }
            };
        }

        public void ClipTileRichTextStackPanel_Loaded(object sender, RoutedEventArgs e) {
            if (RtbVisibility == Visibility.Collapsed) {
                return;
            }
            RtbListBoxCanvas = (Canvas)sender;
            var rtblb = (ListBox)RtbListBoxCanvas.FindName("ClipTileRichTextBoxListBox");

            RichTextBoxListBox = rtblb;
            RichTextBoxListBox.RequestBringIntoView += (s, e65) => { e65.Handled = true; };
            RichTextBoxListBox.PreviewMouseLeftButtonDown += (s, e4) => {     
                if(IsSelected) {
                    return;
                }
                var newarg = new MouseButtonEventArgs(e4.MouseDevice, e4.Timestamp,
                                          e4.ChangedButton, e4.StylusDevice);
                newarg.RoutedEvent = ListViewItem.MouseDownEvent;
                newarg.Source = sender;
                ClipBorder.RaiseEvent(newarg);                
            };

            //after pasting template rtb's are duplicated so clear them upon refresh
            if (CopyItemType == MpCopyItemType.Composite) {
                RichTextBoxViewModelCollection.Clear();
                foreach (var cci in CopyItem.CompositeItemList) {
                    RichTextBoxViewModelCollection.Add(new MpRtbListBoxItemRichTextBoxViewModel(this, cci));
                }
            }

            #region Search
            PropertyChanged += (s, e2) => {
                switch (e2.PropertyName) {
                    case nameof(IsEditingTile):
                        if (!IsEditingTile) {
                            //IsPastingTemplateTile = false;

                            SaveToDatabase();
                        }
                        foreach(var rtbvm in RichTextBoxViewModelCollection) {
                            var rtb = rtbvm.Rtb;
                            rtb.ScrollToHome();
                            rtb.CaretPosition = rtb.Document.ContentStart;
                            rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);
                        }
                        
                        break;
                }
            };

            OnPropertyChanged(nameof(LoadingSpinnerVisibility));
            OnPropertyChanged(nameof(ContentVisibility));
            #endregion
        }

        public void ClipTileImageCanvas_Loaded(object sender, RoutedEventArgs e) {
            if (ImgVisibility == Visibility.Collapsed) {
                return;
            }
            var ic = (FrameworkElement)sender;
            var ctcc = ic.GetVisualAncestor<Canvas>();
            var vb = (Viewbox)ic.FindName("ClipTileImageItemsControlViewBox");

            //vb.ContextMenu = ctcc.ContextMenu = ic.ContextMenu = (ContextMenu)((FrameworkElement)sender).GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");

            DetectedImageObjectCollectionViewModel = new MpDetectedImageObjectCollectionViewModel(CopyItem);
        }

        public void ClipTileFileListBox_Loaded(object sender, RoutedEventArgs e) {
            if (FileListVisibility == Visibility.Collapsed) {
                return;
            }
            OnPropertyChanged(nameof(FileListViewModels));

            var flb = (ListBox)sender;
            flb.ContextMenu = (ContextMenu)flb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");

            FileListBox = flb;
        }
        #endregion

        #region Context Menu
        public void ClipTile_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)sender;
            cm.DataContext = this;
            MenuItem cmi = null;
            foreach (MenuItem mi in cm.Items) {
                if (mi.Name == "ClipTileColorContextMenuItem") {
                    cmi = mi;
                    break;
                }
            }
            MpHelpers.Instance.SetColorChooserMenuItem(
                    cm,
                    cmi,
                    (s, e1) => {
                        MainWindowViewModel.ClipTrayViewModel.ChangeSelectedClipsColorCommand.Execute((Brush)((Border)s).Tag);
                        foreach(var sctvm in MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles) {
                            sctvm.CopyItem.WriteToDatabase();
                        }
                    },
                    MpHelpers.Instance.GetColorColumn(TitleBackgroundColor),
                    MpHelpers.Instance.GetColorRow(TitleBackgroundColor)
                );
        }

        public void ClipTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {
            ((MpClipTileViewModel)((FrameworkElement)sender).DataContext).SaveToDatabase();
        }

        public void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)sender;
            if(IsExpanded && CopyItemType == MpCopyItemType.Composite) {
                //cm.DataContext = RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel;
            }
            if(CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Composite) {
                cm = MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(cm);
            }

            if (CopyItemType == MpCopyItemType.Image && !string.IsNullOrEmpty(CopyItemPlainText)) {
                var cmi = new MenuItem();
                cmi.Header = "Convert to Text";
                cmi.Click += (s, e1) => {
                    System.Windows.Clipboard.SetText(CopyItemPlainText);
                };
                cm.Items.Add(cmi);
            }

            RefreshCommands();

            TagMenuItems.Clear();
            foreach (var tagTile in MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel) {
                if (tagTile.TagName == Properties.Settings.Default.HistoryTagTitle) {
                    continue;
                }
                TagMenuItems.Add(
                    new MpClipTileContextMenuItemViewModel(
                        tagTile.TagName,
                        MainWindowViewModel.ClipTrayViewModel.LinkTagToCopyItemCommand,
                        tagTile,
                        tagTile.IsLinkedWithClipTile(this)));
            }            
        }
        #endregion

        public void RefreshCommands() {
            MainWindowViewModel.ClipTrayViewModel.BringSelectedClipTilesToFrontCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.SendSelectedClipTilesToBackCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.SpeakSelectedClipsAsyncCommand.RaiseCanExecuteChanged();
        }

        public void MergeClip(MpClipTileViewModel octvm, bool mergeTags = false) {
            CopyItem = MpCopyItem.Merge(octvm.CopyItem,CopyItem,false,true);

            OnPropertyChanged(nameof(CopyItem));

            if (mergeTags) {
                MainWindowViewModel.TagTrayViewModel.RefreshAllCounts();
            }
            //clear rtbvms so when item is reloading it adds the merged ci
            RichTextBoxViewModelCollection.Clear();

            //when initially converting richtext item into composite it needs to be relinked to history
            MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(this,false);
            MainWindowViewModel.ClipTrayViewModel.Refresh();
            Refresh();
        }

        public void FadeIn(double bt = 0,double ms = 1000) {
            MpHelpers.Instance.AnimateVisibilityChange(
                ClipBorder,
                Visibility.Visible,
                (s,e) => {
                    TileVisibility = Visibility.Visible;
                },
                ms,bt);
        }

        public void FadeOut(Visibility outVisibility = Visibility.Collapsed, double bt = 0, double ms = 1000) {
            MpHelpers.Instance.AnimateVisibilityChange(
                ClipBorder,
                outVisibility,
                (s, e) => {
                    TileVisibility = outVisibility;
                },
                ms,bt);
        }


        public void Refresh() {
            if(RichTextBoxListBox != null) {
                RichTextBoxListBox.Items.Refresh();
            }
            if (FileListBox != null) {
                FileListBox.Items.Refresh();
            }
        }

        public void SaveToDatabase() {
            //remove links to update model rich text
            RichTextBoxViewModelCollection.ClearAllHyperlinks();

            //clear any search highlighting when saving the document then restore after save
            HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(null);
            
            foreach(var rtbvm in RichTextBoxViewModelCollection) {
                //property change will write the copyitem to the database
                rtbvm.CopyItemRichText = MpHelpers.Instance.ConvertFlowDocumentToRichText(rtbvm.Rtb.Document);
            }
            //CopyItemRichText = MpHelpers.Instance.ConvertFlowDocumentToRichText(RichTextBoxViewModelCollection.FullDocument);
            HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(null);

            RichTextBoxViewModelCollection.CreateAllHyperlinks();

            CopyItem.WriteToDatabase();
        }

        public async Task<string> GetPastableRichText() {
            if (HasTemplate) {
                bool hasExpanded = false;
                IsPastingTemplateTile = true;
                TemplateRichText = string.Empty.ToRichText();
                foreach(var rtbvm in RichTextBoxViewModelCollection) {
                    if(rtbvm.HasTemplate) {
                        if(!hasExpanded) {
                            //tile will be shrunk in on completed of hide window
                            MainWindowViewModel.ClipTrayViewModel.ExpandClipTile(this, true);

                            RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(rtbvm, false, true);
                            hasExpanded = true;
                        } else {
                            RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(rtbvm, false, false);
                        }                        
                    }
                    var rtbvmrt = await rtbvm.GetPastableRichText();
                    TemplateRichText = MpHelpers.Instance.CombineRichText(rtbvmrt, TemplateRichText, true);
                }
                return TemplateRichText;
            }
            return CopyItemRichText;

            //both return to ClipTray.GetDataObjectFromSelectedClips
        }

        public void DeleteTempFiles() {
            foreach (var f in _tempFileList) {
                if (File.Exists(f)) {
                    File.Delete(f);
                }
            }
        }

        public void ContextMenuMouseLeftButtonUpOnSearchGoogle() {
            MpHelpers.Instance.OpenUrl(@"https://www.google.com/search?q=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchBing() {
            MpHelpers.Instance.OpenUrl(@"https://www.bing.com/search?q=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchDuckDuckGo() {
            MpHelpers.Instance.OpenUrl(@"https://duckduckgo.com/?q=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchYandex() {
            MpHelpers.Instance.OpenUrl(@"https://yandex.com/search/?text=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ResetContentScroll() {
            switch(CopyItemType) {
                case MpCopyItemType.Composite:
                case MpCopyItemType.RichText:
                    RichTextBoxListBox.ScrollIntoView(RichTextBoxViewModelCollection[0]);
                    foreach(var rtbvm in RichTextBoxViewModelCollection) {
                        rtbvm.Rtb.ScrollToHome();
                    }
                    break;
                case MpCopyItemType.FileList:
                    FileListBox.ScrollIntoView(FileListViewModels[0]);
                    break;
            }
        }
        #endregion

        #region Private Methods              
        
        
        #endregion

        #region Commands
        private RelayCommand _createQrCodeFromClipCommand;
        public ICommand CreateQrCodeFromClipCommand {
            get {
                if (_createQrCodeFromClipCommand == null) {
                    _createQrCodeFromClipCommand = new RelayCommand(CreateQrCodeFromClip, CanCreateQrCodeFromClip);
                }
                return _createQrCodeFromClipCommand;
            }
        }
        private bool CanCreateQrCodeFromClip() {
            return CopyItemType == MpCopyItemType.RichText && CopyItemPlainText.Length <= Properties.Settings.Default.MaxQrCodeCharLength;
        }
        private void CreateQrCodeFromClip() {
            var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(CopyItemPlainText);
            System.Windows.Clipboard.SetImage(bmpSrc);
        }

        private AsyncCommand<string> _translateClipTextAsyncCommand;
        public IAsyncCommand<string> TranslateClipTextAsyncCommand {
            get {
                if (_translateClipTextAsyncCommand == null) {
                    _translateClipTextAsyncCommand = new AsyncCommand<string>(TranslateClipTextAsync, CanTranslateClipText);
                }
                return _translateClipTextAsyncCommand;
            }
        }
        private bool CanTranslateClipText(object args) {
            return CopyItemType == MpCopyItemType.RichText;
        }
        private async Task TranslateClipTextAsync(string toLanguage) {
            var translatedText = await MpLanguageTranslator.Instance.Translate(CopyItemPlainText, toLanguage, false);
            if (!string.IsNullOrEmpty(translatedText)) {
                CopyItemRichText = MpHelpers.Instance.ConvertPlainTextToRichText(translatedText);
            }
        }

        private RelayCommand _sendClipToEmailCommand;
        public ICommand SendClipToEmailCommand {
            get {
                if (_sendClipToEmailCommand == null) {
                    _sendClipToEmailCommand = new RelayCommand(SendClipToEmail, CanSendClipToEmail);
                }
                return _sendClipToEmailCommand;
            }
        }
        private bool CanSendClipToEmail() {
            return !IsEditingTile;
        }
        private void SendClipToEmail() {
            MpHelpers.Instance.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, CopyItemTitle, CopyItemPlainText));
            //MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            //IsSelected = true;
            //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        }

        private RelayCommand _toggleEditTitleCommand;
        public ICommand ToggleEditTitleCommand {
            get {
                if (_toggleEditTitleCommand == null) {
                    _toggleEditTitleCommand = new RelayCommand(ToggleEditTitle, CanToggleEditTitle);
                }
                return _toggleEditTitleCommand;
            }
        }
        private bool CanToggleEditTitle() {
            if (MainWindowViewModel.IsLoading) {
                return false;
            }
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1;
        }
        private void ToggleEditTitle() {
            IsEditingTitle = !IsEditingTitle;
        }

        private RelayCommand<object> _toggleEditClipCommand;
        public ICommand ToggleEditClipCommand {
            get {
                if (_toggleEditClipCommand == null) {
                    _toggleEditClipCommand = new RelayCommand<object>(ToggleEditClip, CanToggleEditClip);
                }
                return _toggleEditClipCommand;
            }
        }
        private bool CanToggleEditClip(object args) {
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 &&
                  (CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Composite) && 
                  !IsPastingTemplateTile;
        }
        private void ToggleEditClip(object args) {
            //if(IsEditingTile == false && !IsSelected) {
            //    IsSelected = true;
            //}
            if(args == null) {
                //happens when toggled from context menu and from code not button in detail grid
                IsEditingTile = !IsEditingTile;
            }

            if (IsEditingTile) {
                MainWindowViewModel.ClipTrayViewModel.ExpandClipTile(this, false);
            } else {
                MainWindowViewModel.ClipTrayViewModel.ShrinkClipTile(this, false);
            }
        }

        private RelayCommand _cancelEditClipTextCommand;
        public ICommand CancelEditClipTextCommand {
            get {
                if (_cancelEditClipTextCommand == null) {
                    _cancelEditClipTextCommand = new RelayCommand(CancelEditClipText, CanCancelEditClipText);
                }
                return _cancelEditClipTextCommand;
            }
        }
        private bool CanCancelEditClipText() {
            return IsEditingTile;
        }
        private void CancelEditClipText() {
            _wasEditConfirmed = false;
            IsEditingTile = false;
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
            _wasEditConfirmed = true;
            IsEditingTile = false;
            //all other action is handled in the ertb visibility changed handler in ertb_loaded
        }

        private RelayCommand _excludeApplicationCommand;
        public ICommand ExcludeApplicationCommand {
            get {
                if (_excludeApplicationCommand == null) {
                    _excludeApplicationCommand = new RelayCommand(ExcludeApplication, CanExcludeApplication);
                }
                return _excludeApplicationCommand;
            }
        }
        private bool CanExcludeApplication() {
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1;
        }
        private void ExcludeApplication() {
            MpAppCollectionViewModel.Instance.UpdateRejection(MpAppCollectionViewModel.Instance.GetAppViewModelByAppId(CopyItemAppId), true);
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
            MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            IsSelected = true; 
            MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand.Execute(null);
        }

        #endregion

        #region Overrides

        //public override string ToString() {
        //    return CopyItemPlainText;
        //}

        public void Dispose() {
            MainWindowViewModel.ClipTrayViewModel.Remove(this);
            CopyItem.DeleteFromDatabase();
        }

        #endregion
    }
}
