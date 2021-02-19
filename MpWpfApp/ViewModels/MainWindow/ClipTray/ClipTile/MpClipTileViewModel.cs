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
    using AsyncAwaitBestPractices.MVVM;
    using GalaSoft.MvvmLight.CommandWpf;
    using GongSolutions.Wpf.DragDrop.Utilities;

    public class MpClipTileViewModel : MpUndoableViewModelBase<MpClipTileViewModel>, IDisposable {        
        #region Private Variables

        private RichTextBox _rtb;
        private TextBlock _tb;
        private ListBox _flb;
        private MpClipBorder _cb;

        private int _detailIdx = 0;

        private List<string> _tempFileList = new List<string>();

        private string _origClipRichText = string.Empty;

        private bool _wasAddedAtRuntime = false;

        private bool _wasEditConfirmed = true;

        #endregion

        #region Statics
        #endregion

        #region View Models
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
                if(_fileListViewModels == null) {
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

        private MpTemplateHyperlinkCollectionViewModel _templateHyperlinkCollectionViewModel = null;
        public MpTemplateHyperlinkCollectionViewModel TemplateHyperlinkCollectionViewModel {
            get {
                return _templateHyperlinkCollectionViewModel;
            }
            set {
                if (_templateHyperlinkCollectionViewModel != value) {
                    _templateHyperlinkCollectionViewModel = value;
                    OnPropertyChanged(nameof(TemplateHyperlinkCollectionViewModel));
                }
            }
        }

        private MpEditTemplateToolbarViewModel _editTemplateToolbarViewModel = null;
        public MpEditTemplateToolbarViewModel EditTemplateToolbarViewModel {
            get {
                return _editTemplateToolbarViewModel;
            }
            set {
                if(_editTemplateToolbarViewModel != value) {
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
                if(_pasteTemplateToolbarViewModel != value) {
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
                if(TileDetailGridVisibility == Visibility.Visible) {
                    return MpMeasurements.Instance.ClipTileContentHeight;
                }
                return MpMeasurements.Instance.ClipTileContentHeight + TileDetailHeight;
            }
        }

        public double TileRtbHeight {
            get {
                double ch = MpMeasurements.Instance.ClipTileContentHeight;
                if (IsEditingTile) {
                    ch -= MpMeasurements.Instance.ClipTileEditToolbarHeight;
                }
                if(IsPastingTemplateTile) {
                    ch -= MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                }
                if (IsEditingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTileEditTemplateToolbarHeight;
                }
                return ch;
            }
        }

        public double TileDetailHeight {
            get {
                return MpMeasurements.Instance.ClipTileDetailHeight;
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
        #endregion

        #region Visibility Properties
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
                if(IsEditingTile || IsPastingTemplateTile) {
                    return ScrollBarVisibility.Auto;
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility RtbVerticalScrollbarVisibility {
            get {
                if (IsEditingTile || IsPastingTemplateTile) {
                    return ScrollBarVisibility.Auto;
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
                if (!IsLoading && CopyItemType == MpCopyItemType.RichText) {
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
                }
                return Visibility.Visible;
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
        public bool IsLoading {
            get {
                return CopyItem == null || CopyItem.CopyItemId == 0;
            }
        }

        public string DetailText {
            get {
                return GetCurrentDetail(_detailIdx);
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

        public MpObservableCollection<TextRange> LastContentHighlightRangeList { get; set; } = new MpObservableCollection<TextRange>();

        public MpObservableCollection<TextRange> LastTitleHighlightRangeList { get; set; } = new MpObservableCollection<TextRange>();

        public Cursor ContentCursor {
            get {
                if (IsEditingTile) {
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
                }
            }
        }

        private int _currentHighlightMatchIdx = -1;
        public int CurrentHighlightMatchIdx {
            get {
                return _currentHighlightMatchIdx;
            }
            set {
                //if (_currentHighlightMatchIdx != value) 
                {
                    _currentHighlightMatchIdx = value;
                    OnPropertyChanged(nameof(CurrentHighlightMatchIdx));
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
                    OnPropertyChanged(nameof(TileRtbHeight));
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
                    OnPropertyChanged(nameof(TileRtbHeight));
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
                    OnPropertyChanged(nameof(TileRtbHeight));
                }
            }
        }        

        public bool HasTemplate {
            get {
                return TemplateHyperlinkCollectionViewModel.Count > 0;
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

        #region Model Properties
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

        public Brush TitleColor {
            get {
                if (CopyItem == null) {
                    return Brushes.Red;
                }
                return new SolidColorBrush(CopyItem.ItemColor.Color);
            }
            set {
                if (CopyItem != null && CopyItem.ItemColor.Color != ((SolidColorBrush)value).Color) {
                    CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                    //CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(TitleColor));
                    OnPropertyChanged(nameof(CopyItem));
                }
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
                if (CopyItem == null) {
                    return new List<string>();
                }
                return CopyItem.GetFileList(string.Empty, MainWindowViewModel.ClipTrayViewModel.GetTargetFileType());
            }
        }

        public string TemplateRichText {
            get; 
            set;
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
                return _copyItem;
            }
            private set {
                //if (_copyItem != value) 
                {
                    _copyItem = value;
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
                    //CopyItem.WriteToDatabase();
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileViewModel(bool isPlaceHolder = true) : base() {
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
                            IsPastingTemplateTile = false;
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
                }
            };
            TemplateHyperlinkCollectionViewModel = new MpTemplateHyperlinkCollectionViewModel(this);
            EditRichTextBoxToolbarViewModel = new MpEditRichTextBoxToolbarViewModel(this);
            EditTemplateToolbarViewModel = new MpEditTemplateToolbarViewModel(this);
            PasteTemplateToolbarViewModel = new MpPasteTemplateToolbarViewModel(this);

            if(isPlaceHolder) {
                SetCopyItem(null);
            }
        }

        public MpClipTileViewModel(MpCopyItem ci) : this(false) {
            SetCopyItem(ci);
        }
        
        public static async Task<MpClipTileViewModel> LoadClipTileViewModel(MpCopyItem ci, CancellationToken ct) {
            var ctvm = new MpClipTileViewModel(ci);
            await Task.Run(() => {
                while (ctvm.IsLoading) {
                    System.Threading.Thread.Sleep(100);
                }
            }, ct);
            return ctvm;
        }

        public void SetCopyItem(MpCopyItem ci) {
            if (ci == null) {
                //throw new Exception("MpClipTileViewModel error, cannot set null copyitem");
                CopyItem = ci;
                return;
            }
            if (ci.CopyItemId == 0 && !MainWindowViewModel.IsLoading) {
                ci.WriteToDatabase();
                _wasAddedAtRuntime = true;
            }
            VirtualizationManager.Instance.RunOnUI(() => {
                CopyItem = ci;
                if (MainWindowViewModel != null) {
                    //is null during loading and the refresh isn't needed
                    MainWindowViewModel.ClipTrayViewModel.GetClipTray().Items.Refresh();
                }
            });            
        }

        public MpClipBorder GetBorder() {
            return _cb;
        }

        public RichTextBox GetRtb() {
            return _rtb;
        }

        public TextBlock GetTitleTextBlock() {
            return _tb;
        }

        public ListBox GetFileListBox() {
            return _flb;
        }

        public void RefreshCommands() {
            MainWindowViewModel.ClipTrayViewModel.BringSelectedClipTilesToFrontCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.SendSelectedClipTilesToBackCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.SpeakSelectedClipsAsyncCommand.RaiseCanExecuteChanged();
        }

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTileBorder = (MpClipBorder)sender;
            _cb = clipTileBorder;//clipTileBorder.GetVisualAncestor<MpClipBorder>();

            clipTileBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            clipTileBorder.MouseLeave += (s, e2) => {
                IsHovering = false;
            };
            clipTileBorder.LostFocus += (s, e4) => {
                if (!IsSelected) {
                    IsEditingTitle = false;
                }
            };
            clipTileBorder.PreviewMouseLeftButtonDown += (s, e5) => {
                if (e5.ClickCount == 2 && !IsEditingTile) {
                    //only for richtext type
                    EditClipCommand.Execute(null);
                    e5.Handled = true;
                    return;
                }
            };
            clipTileBorder.PreviewKeyDown += (s, e6) => {
                if (CopyItemType != MpCopyItemType.RichText) {
                    return;
                }
                var rtb = (RichTextBox)((FrameworkElement)s).FindName("ClipTileRichTextBox");
                //if (e6.Key == Key.Down) {
                //    rtb.FontSize -= 1;
                //    e.Handled = true;
                //} else if (e6.Key == Key.Up) {
                //    rtb.FontSize += 1;
                //    e.Handled = true;
                //}
            };
            //clipTileBorder.PreviewMouseRightButtonUp += (s, e4) => {
            //    var p = e4.MouseDevice.GetPosition(clipTileBorder);
            //    var hitTestResult = VisualTreeHelper.HitTest(clipTileBorder, p);
            //    if (hitTestResult != null) {
            //        MessageBox.Show(hitTestResult.VisualHit.ToString());
            //    }
            //};
        }

        public void ClipTileDetailGrid_Loaded(object sender, RoutedEventArgs e) {
            var detailGrid = (Grid)sender;
            var titleDetailTextBlock = (TextBlock)detailGrid.FindName("ClipTileTitleDetailTextBlock");

            titleDetailTextBlock.MouseEnter += (s, e5) => {
                if (++_detailIdx > 2) {
                    _detailIdx = 0;
                }
                titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
            };
            titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
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
            
            _tb = clipTileTitleTextBlock;

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
            var rtbc = (Canvas)sender;
            var rtb = (RichTextBox)rtbc.FindName("ClipTileRichTextBox");
            _rtb = rtb;

            var hb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);
            var hfb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightFocusedHexColorString);

            GetRtb().Document = MpHelpers.Instance.ConvertRichTextToFlowDocument(CopyItemRichText);
            GetRtb().CreateHyperlinks();
            GetRtb().Document.PageWidth = GetRtb().Width - GetRtb().Padding.Left - GetRtb().Padding.Right;
            GetRtb().Document.PageHeight = GetRtb().Height - GetRtb().Padding.Top - GetRtb().Padding.Bottom;

            if(_wasAddedAtRuntime) {
                //force new items to have left alignment
                GetRtb().SelectAll();
                GetRtb().Selection.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                GetRtb().CaretPosition = GetRtb().Document.ContentStart;
            }

            #region Search
            LastContentHighlightRangeList.CollectionChanged += (s, e9) => {
                //var oldNavButtonPanelVisibility = MainWindowViewModel.SearchBoxViewModel.SearchNavigationButtonPanelVisibility;
                if (LastContentHighlightRangeList.Count > 1 || LastTitleHighlightRangeList.Count > 1) {
                    //show search match nav buttons when any clip tile has more than one match
                    MainWindowViewModel.SearchBoxViewModel.SearchNavigationButtonPanelVisibility = Visibility.Visible;
                } else {
                    //confirm all visible clip tiles have one or less matches then hide nav buttons
                    bool showMatchNav = false;
                    foreach (var vctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
                        if (vctvm.LastContentHighlightRangeList.Count > 1 || vctvm.LastTitleHighlightRangeList.Count > 1) {
                            showMatchNav = true;
                        }
                    }
                    if (showMatchNav) {
                        MainWindowViewModel.SearchBoxViewModel.SearchNavigationButtonPanelVisibility = Visibility.Visible;
                    } else {
                        MainWindowViewModel.SearchBoxViewModel.SearchNavigationButtonPanelVisibility = Visibility.Collapsed;
                    }
                }
            };

            LastTitleHighlightRangeList.CollectionChanged += (s, e9) => {
                bool showMatchNav = false;
                //var oldNavButtonPanelVisibility = MainWindowViewModel.SearchBoxViewModel.SearchNavigationButtonPanelVisibility;
                if (LastContentHighlightRangeList.Count > 1 || LastTitleHighlightRangeList.Count > 1) {
                    //show search match nav buttons when any clip tile has more than one match
                    MainWindowViewModel.SearchBoxViewModel.SearchNavigationButtonPanelVisibility = Visibility.Visible;
                    showMatchNav = true;
                } else {
                    //confirm all visible clip tiles have one or less matches then hide nav buttons
                    foreach (var vctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
                        if (vctvm.LastContentHighlightRangeList.Count > 1 || vctvm.LastTitleHighlightRangeList.Count > 1) {
                            showMatchNav = true;
                        }
                    }
                    if (showMatchNav) {
                        MainWindowViewModel.SearchBoxViewModel.SearchNavigationButtonPanelVisibility = Visibility.Visible;
                    } else {
                        MainWindowViewModel.SearchBoxViewModel.SearchNavigationButtonPanelVisibility = Visibility.Collapsed;
                    }
                }
            };

            MainWindowViewModel.SearchBoxViewModel.PrevMatchClicked += (s, e11) => {
                CurrentHighlightMatchIdx--;
            };

            MainWindowViewModel.SearchBoxViewModel.NextMatchClicked += (s, e11) => {
                CurrentHighlightMatchIdx++;
            };

            PropertyChanged += (s, e2) => {
                switch (e2.PropertyName) {
                    case nameof(IsEditingTile):
                        if(!IsEditingTile) {
                            //IsPastingTemplateTile = false;

                            SaveToDatabase();
                        }
                        GetRtb().ScrollToHome();
                        GetRtb().CaretPosition = GetRtb().Document.ContentStart;
                        GetRtb().Selection.Select(GetRtb().Document.ContentStart, GetRtb().Document.ContentStart);
                        break;
                    case nameof(CurrentHighlightMatchIdx):
                        if(LastContentHighlightRangeList.Count == 0 && LastTitleHighlightRangeList.Count == 0) {
                            _currentHighlightMatchIdx = 0;
                            return;
                        }
                        int maxIdx = LastContentHighlightRangeList.Count + LastTitleHighlightRangeList.Count - 1;
                        if (CurrentHighlightMatchIdx < 0) {
                            _currentHighlightMatchIdx = maxIdx;
                        } else if (CurrentHighlightMatchIdx > maxIdx) {
                            _currentHighlightMatchIdx = 0;
                        }
                        MpHelpers.Instance.ApplyBackgroundBrushToRangeList(LastTitleHighlightRangeList, hb, new CancellationToken());
                        MpHelpers.Instance.ApplyBackgroundBrushToRangeList(LastContentHighlightRangeList, hb, new CancellationToken());
                        if (CurrentHighlightMatchIdx < LastTitleHighlightRangeList.Count) {
                            LastTitleHighlightRangeList[CurrentHighlightMatchIdx].ApplyPropertyValue(TextElement.BackgroundProperty, hfb);
                        } else if (CurrentHighlightMatchIdx < LastContentHighlightRangeList.Count) {
                            int contentIdx = CurrentHighlightMatchIdx - LastTitleHighlightRangeList.Count;
                            LastContentHighlightRangeList[contentIdx].ApplyPropertyValue(TextElement.BackgroundProperty, hfb);
                            var characterRect = LastContentHighlightRangeList[contentIdx].End.GetCharacterRect(LogicalDirection.Forward);
                            GetRtb().ScrollToHorizontalOffset(GetRtb().HorizontalOffset + characterRect.Left - GetRtb().ActualWidth / 2d);
                            GetRtb().ScrollToVerticalOffset(GetRtb().VerticalOffset + characterRect.Top - GetRtb().ActualHeight / 2d);

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

            _flb = flb;
        }

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
                    MpHelpers.Instance.GetColorColumn(TitleColor),
                    MpHelpers.Instance.GetColorRow(TitleColor)
                );
        }

        public void ClipTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {
            ((MpClipTileViewModel)((FrameworkElement)sender).DataContext).SaveToDatabase();
        }

        public void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)sender;
            if(CopyItemType == MpCopyItemType.RichText) {
                MenuItem ptamir = null;
                foreach(MenuItem mi in cm.Items) {
                    if(mi.Name == "PasteToAppPathMenuItem") {
                        ptamir = mi;
                    }
                }
                if(ptamir == null) {
                    return;
                }
                ptamir.Items.Clear();
                bool addedSeperator = false;
                foreach(var ptamivmc in MpPasteToAppPathViewModelCollection.Instance.MenuItemViewModels) {
                    if(ptamivmc.Count == 0) {
                        continue;
                    }
                    if(ptamivmc[0].IsRuntime) {
                        bool areAllHidden = true;
                        foreach(var ptamivm in ptamivmc) {
                            if(!ptamivm.IsHidden) {
                                areAllHidden = false;
                            }
                        }
                        if(areAllHidden) {
                            continue;
                        }
                        var ptamip = new MenuItem();
                        ptamip.Header = MpHelpers.Instance.GetProcessApplicationName(ptamivmc[0].Handle);
                        ptamip.Icon = new Image() { Source = ptamivmc[0].AppIcon };
                        foreach(var ptamivm in ptamivmc) {
                            if(ptamivm.IsHidden) {
                                continue;
                            }
                            var ptami = new MenuItem();
                            var l = new Label();
                            l.Content = MpHelpers.Instance.GetProcessMainWindowTitle(ptamivm.Handle) + (ptamivm.IsAdmin ? " (Admin)" : string.Empty);
                            
                            var eyeOpenImg = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/eye.png")) };
                            var eyeClosedImg = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/eye_closed.png")) };
                            var btn = new Button() { Cursor = Cursors.Hand, Content = eyeOpenImg, BorderThickness = new Thickness(0), Background = Brushes.Transparent, Width = 20, Height = 20 , HorizontalAlignment = HorizontalAlignment.Right/*, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center*/ };
                            bool isOverButton = false;
                            btn.MouseEnter += (s, e2) => {
                                btn.Content = eyeClosedImg;
                                isOverButton = true;
                            };
                            btn.MouseLeave += (s, e2) => {
                                btn.Content = eyeOpenImg;
                                isOverButton = false;
                            };
                            btn.Click += (s, e2) => {
                                ptamivm.IsHidden = true;
                                ptamip.Items.Remove(ptami);
                                if(ptamip.Items.Count == 0) {
                                    ptamir.Items.Remove(ptamip);
                                }
                            };

                            var sp = new StackPanel() { Orientation = Orientation.Horizontal};
                            sp.Children.Add(l);
                            sp.Children.Add(btn);

                            ptami.Header = sp;   
                            ptami.Icon = new Image() { Source = ptamivm.AppIcon };
                            //ptami.Command = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
                            //ptami.CommandParameter = ptamivm.Handle;
                            ptami.Click += (s, e2) => {
                                if(!isOverButton) {
                                    MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand.Execute(ptamivm.Handle);
                                }
                            };
                            ptamip.Items.Add(ptami);
                        }
                        ptamir.Items.Add(ptamip);
                    } else {
                        if(!addedSeperator) {
                            ptamir.Items.Add(new Separator());
                            addedSeperator = true;
                        }
                        var ptaumi = new MenuItem();
                        ptaumi.Header = ptamivmc[0].AppName;// + (ptamivmc[0].IsAdmin ? " (Admin)" : string.Empty) + (ptamivmc[0].IsSilent ? " (Silent)" : string.Empty);
                        ptaumi.Icon = new Image() { Source = ptamivmc[0].AppIcon };
                        ptaumi.Command = MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand;
                        ptaumi.CommandParameter = ptamivmc[0].PasteToAppPathId;

                        ptamir.Items.Add(ptaumi);
                    }
                }
                var addNewMenuItem = new MenuItem();
                addNewMenuItem.Header = "Add Application...";
                addNewMenuItem.Icon = new Image() { Source = (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Icons/Silk/icons/add.png")) };
                addNewMenuItem.Click += (s, e3) => {                    
                    MainWindowViewModel.SystemTrayViewModel.ShowSettingsWindowCommand.Execute(1);
                };
                ptamir.Items.Add(addNewMenuItem);
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

        public void AppendContent(MpClipTileViewModel octvm) {
            CopyItem.CombineAsync(octvm.CopyItem);
            //since appending only happens for richtext types
            OnPropertyChanged(nameof(CopyItemRichText));
            GetRtb().Document = MpHelpers.Instance.ConvertRichTextToFlowDocument(CopyItemRichText);
        }

        public void SyncModelWithView() {
            GetRtb().ClearHyperlinks();
            var hb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);

            //clear any search highlighting when saving the document then restore after save
            MpHelpers.Instance.ApplyBackgroundBrushToRangeList(LastTitleHighlightRangeList, Brushes.Transparent, new CancellationToken());
            MpHelpers.Instance.ApplyBackgroundBrushToRangeList(LastContentHighlightRangeList, Brushes.Transparent, new CancellationToken());
            CopyItemRichText = MpHelpers.Instance.ConvertFlowDocumentToRichText(GetRtb().Document);
            MpHelpers.Instance.ApplyBackgroundBrushToRangeList(LastTitleHighlightRangeList, hb, new CancellationToken());
            MpHelpers.Instance.ApplyBackgroundBrushToRangeList(LastContentHighlightRangeList, hb, new CancellationToken());

            GetRtb().CreateHyperlinks();
        }

        public void SaveToDatabase() {
            if(CopyItemType == MpCopyItemType.RichText) {
                MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(CopyItemId).SyncModelWithView();
            }
            MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(CopyItemId).CopyItem.WriteToDatabase();
        }

        public async Task<string> GetPastableRichText() {
            if (HasTemplate) {
                IsPastingTemplateTile = true;
                TemplateRichText = string.Empty;
                var temp = CopyItemRichText;
                await Task.Run(() => {
                    while (string.IsNullOrEmpty(TemplateRichText)) {
                        System.Threading.Thread.Sleep(500);
                    }
                    //TemplateRichText is set in PasteTemplateCommand
                });
                CopyItemRichText = temp;
                GetRtb().Document = MpHelpers.Instance.ConvertRichTextToFlowDocument(CopyItemRichText);
                GetRtb().ClearHyperlinks();
                CopyItemRichText = MpHelpers.Instance.ConvertFlowDocumentToRichText(GetRtb().Document);
                GetRtb().CreateHyperlinks();
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

        public async Task PerformHighlight(MpClipTileViewModel ctvm, string hlt, CancellationToken ct) {
            ctvm.MainWindowViewModel.ClipTrayViewModel.HighlightTaskCount++;
            if (ctvm.MainWindowViewModel.IsLoading || ctvm.IsLoading) {
                ctvm.TileVisibility = Visibility.Visible;
                ctvm.MainWindowViewModel.ClipTrayViewModel.HighlightTaskCount--;
                return;
            }

            var sttvm = ctvm.MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;
            if (!sttvm.IsLinkedWithClipTile(ctvm)) {
                Console.WriteLine("Clip tile w/ title " + ctvm.CopyItemTitle + " is not linked with current tag");
                ctvm.TileVisibility = Visibility.Collapsed;
                ctvm.MainWindowViewModel.ClipTrayViewModel.HighlightTaskCount--;
                return;
            }

            if (string.IsNullOrEmpty(hlt.Trim()) || hlt == Properties.Settings.Default.SearchPlaceHolderText) {
                //if search text is empty clear any highlights and show clip (if associated w/ current tag)
                ctvm.TileVisibility = Visibility.Visible;
                MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, Brushes.Transparent, ct);
                ctvm.LastTitleHighlightRangeList.Clear();

                MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, Brushes.Transparent, ct);
                ctvm.LastContentHighlightRangeList.Clear();
                ctvm.MainWindowViewModel.ClipTrayViewModel.HighlightTaskCount--;

                ctvm.GetRtb().ScrollToHome();
                return;
            }

            await PerformHighlightAsync(ctvm, hlt, ct);
            ctvm.MainWindowViewModel.ClipTrayViewModel.HighlightTaskCount--;
        }
        #endregion

        #region Private Methods      

        private async Task PerformHighlightAsync(MpClipTileViewModel ctvm, string hlt, CancellationToken ct) {
            await Dispatcher.CurrentDispatcher.InvokeAsync(
                (Action)(() => {
                    var cb = ctvm.GetBorder();
                    var ttb = ctvm.GetTitleTextBlock();
                    var hb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);
                    var hfb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightFocusedHexColorString);
                    var ctbb = Brushes.Transparent;

                    RegexOptions rot = RegexOptions.None;
                    RegexOptions roc = RegexOptions.None;
                    if(Properties.Settings.Default.SearchByIsCaseSensitive) {
                        rot = RegexOptions.ExplicitCapture;
                        roc = RegexOptions.ExplicitCapture | RegexOptions.Multiline;
                    } else {
                        rot = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
                        roc = RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline;
                    }
                    var mct = Regex.Matches(ctvm.CopyItemTitle, hlt, rot);
                    var mcc = Regex.Matches(ctvm.CopyItemPlainText, hlt, roc);
                    
                    if (mct.Count == 0 && mcc.Count == 0) {
                        ctvm.TileVisibility = Visibility.Collapsed;
                        return;
                    }
                    
                    Console.WriteLine("Beginning highlight clip with title: " + ctvm.CopyItemTitle + " with highlight text: " + hlt);

                    ctvm.TileVisibility = Visibility.Visible;

                    MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, ctbb, ct);
                    ctvm.LastTitleHighlightRangeList.Clear();

                    MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, ctbb, ct);
                    ctvm.LastContentHighlightRangeList.Clear();


                    //highlight title 
                    if (ttb.Text.ContainsByCaseSetting(hlt)) {
                        var trl = MpHelpers.Instance.FindStringRangesFromPosition(ttb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                        foreach (var mr in trl) {
                            ctvm.LastTitleHighlightRangeList.Add(mr);
                        }

                        MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, hb, ct);
                    }
                    switch (ctvm.CopyItemType) {
                        case MpCopyItemType.RichText:
                            var rtb = ctvm.GetRtb();

                            rtb.BeginChange();
                            var trl = MpHelpers.Instance.FindStringRangesFromPosition(rtb.Document.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                            foreach (var mr in trl) {
                                ctvm.LastContentHighlightRangeList.Add(mr);
                            }
                            if (ctvm.LastContentHighlightRangeList.Count > 0) {
                                MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, hb, ct);
                            } else if (ctvm.LastTitleHighlightRangeList.Count == 0) {
                                ctvm.TileVisibility = Visibility.Collapsed;
                            }
                            if (ctvm.LastContentHighlightRangeList.Count > 0 || ctvm.LastTitleHighlightRangeList.Count > 0) {
                                ctvm.CurrentHighlightMatchIdx = 0;
                            }
                            rtb.EndChange();
                            break;
                        case MpCopyItemType.Image:
                            foreach (var diovm in ctvm.DetectedImageObjectCollectionViewModel) {
                                if (diovm.ObjectTypeName.ContainsByCaseSetting(hlt)) {
                                    ctvm.TileVisibility = Visibility.Visible;
                                    return;
                                }
                            }
                            if (ctvm.LastContentHighlightRangeList.Count == 0) {
                                ctvm.TileVisibility = Visibility.Collapsed;
                            }
                            break;
                        case MpCopyItemType.FileList:
                            var flb = ctvm.GetFileListBox();
                            if (ctvm.LastContentHighlightRangeList != null) {
                                foreach (var lhr in ctvm.LastContentHighlightRangeList) {
                                    lhr.ApplyPropertyValue(TextElement.BackgroundProperty, ctbb);
                                }
                            }
                            foreach (var fivm in ctvm.FileListViewModels) {
                                if (fivm.ItemPath.ContainsByCaseSetting(hlt)) {
                                    var container = flb.ItemContainerGenerator.ContainerFromItem(fivm) as FrameworkElement;
                                    if (container != null) {
                                        var fitb = (TextBlock)container.FindName("FileListItemTextBlock");
                                        if (fitb != null) {
                                            var hlr = MpHelpers.Instance.FindStringRangeFromPosition(fitb.ContentStart, hlt, Properties.Settings.Default.SearchByIsCaseSensitive);
                                            if (hlr != null) {
                                                hlr.ApplyPropertyValue(TextBlock.BackgroundProperty, hb);
                                                ctvm.LastContentHighlightRangeList.Add(hlr);
                                            }
                                        }
                                    }
                                }
                            }
                            if (ctvm.LastContentHighlightRangeList.Count == 0) {
                                ctvm.TileVisibility = Visibility.Collapsed;
                            } else {
                                ctvm.TileVisibility = Visibility.Visible;
                            }
                            break;
                    }
                    Console.WriteLine("Ending highlighting clip with title: " + ctvm.CopyItemTitle);
                }),
                DispatcherPriority.Background);

            //if (HighlightTasks.Count > 0) {
            //    HighlightTasks.RemoveAt(0);
            //}
        }
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
                        info = "(" + (int)CopyItemBmp.Width + ") x (" + (int)CopyItemBmp.Height + ")";
                    } else if (CopyItemType == MpCopyItemType.RichText) {
                        info = CharCount + " chars | " + LineCount + " lines";
                    } else if (CopyItemType == MpCopyItemType.FileList) {
                        info = FileCount + " files | " + DataSizeInMb + " MB";
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

        private RelayCommand _editClipCommand;
        public ICommand EditClipCommand {
            get {
                if (_editClipCommand == null) {
                    _editClipCommand = new RelayCommand(EditClip, CanEditClip);
                }
                return _editClipCommand;
            }
        }
        private bool CanEditClip() {
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 &&
                  CopyItemType == MpCopyItemType.RichText && !IsPastingTemplateTile &&
                  !IsEditingTile;
        }
        private void EditClip() {
            IsEditingTile = true;
            //IsPastingTemplateTile = true;
            IsSelected = true;
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
