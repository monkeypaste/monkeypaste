namespace MpWpfApp {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
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
    using GalaSoft.MvvmLight.CommandWpf;
    using GongSolutions.Wpf.DragDrop.Utilities;
    using NativeCode;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Storage;

    public class MpClipTileViewModel : MpViewModelBase {
        #region Private Variables

        private RichTextBox _rtb = null;

        private int _detailIdx = 0;

        private List<string> _tempFileList = new List<string>();

        private string _origClipRichText = string.Empty;

        private bool _wasEditConfirmed = true;
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

        public ObservableCollection<MpFileListItemViewModel> FileListViewModels {
            get {
                if (CopyItem == null) {
                    return new ObservableCollection<MpFileListItemViewModel>();
                }
                var fileListViewModels = new ObservableCollection<MpFileListItemViewModel>();
                foreach (var path in CopyItem.GetFileList()) {
                    fileListViewModels.Add(new MpFileListItemViewModel(this, path));
                }
                return fileListViewModels;
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
                    translateLanguageMenuItems.Add(new MpClipTileContextMenuItemViewModel(languageName, TranslateClipTextCommand, languageName, false));
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
        private double _rtbTop = 0;
        public double RtbTop {
            get {
                return _rtbTop;
            }
            set {
                if (_rtbTop != value) {
                    _rtbTop = value;
                    OnPropertyChanged(nameof(RtbTop));
                }
            }
        }

        private double _rtbBottom = MpMeasurements.Instance.ClipTileContentHeight;
        public double RtbBottom {
            get {
                return _rtbBottom;
            }
            set {
                if (_rtbBottom != value) {
                    _rtbBottom = value;
                    OnPropertyChanged(nameof(RtbBottom));
                }
            }
        }

        public double RtbHeight {
            get {
                //double h = MpMeasurements.Instance.ClipTileContentHeight;
                //if (IsEditingTile) {
                //    h -= EditRichTextBoxToolbarViewModel.TileContentEditToolbarHeight;
                //}
                //if (PasteTemplateToolbarViewModel.TemplateToolbarVisibility == Visibility.Visible) {
                //    h -= PasteTemplateToolbarViewModel.PasteTemplateToolbarHeight;
                //}
                //return h;
                return RtbBottom - RtbTop;
            }
        }

        private double _editToolbarTop = -MpMeasurements.Instance.ClipTileEditToolbarHeight;
        public double EditToolbarTop {
            get {
                return _editToolbarTop;
            }
            set {
                if (_editToolbarTop != value) {
                    _editToolbarTop = value;
                    OnPropertyChanged(nameof(EditToolbarTop));
                }
            }
        }       

        public double EditTemplateToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTilePasteToolbarHeight;
            }
        }

        public double EditRichTextBoxToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTileEditToolbarHeight;
            }
        }

        public double PasteTemplateToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTilePasteToolbarHeight;
            }
        }

        private double _pasteTemplateToolbarTop = MpMeasurements.Instance.ClipTileContentHeight;
        public double PasteTemplateToolbarTop {
            get {
                return _pasteTemplateToolbarTop;
            }
            set {
                if (_pasteTemplateToolbarTop != value) {
                    _pasteTemplateToolbarTop = value;
                    OnPropertyChanged(nameof(PasteTemplateToolbarTop));
                }
            }
        }

        private double _editTemplateToolbarTop = MpMeasurements.Instance.ClipTileContentHeight;
        public double EditTemplateToolbarTop {
            get {
                return _editTemplateToolbarTop;
            }
            set {
                if (_editTemplateToolbarTop != value) {
                    _editTemplateToolbarTop = value;
                    OnPropertyChanged(nameof(EditTemplateToolbarTop));
                }
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

        public double TileContentHeight {
            get {
                return MpMeasurements.Instance.ClipTileContentHeight;
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

        public Visibility EditToolbarVisibility {
            get {
                if (IsEditingTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility EditTemplateToolbarVisibility {
            get {
                if (IsEditingTemplate) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility PasteTemplateToolbarVisibility {
            get {
                if (IsPastingTemplateTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public ScrollBarVisibility RtbHorizontalScrollbarVisibility {
            get {
                if(IsEditingTile) {
                    return ScrollBarVisibility.Auto;
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility RtbVerticalScrollbarVisibility {
            get {
                if (IsEditingTile) {
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

        private Visibility _tileVisibility = Visibility.Visible;
        public Visibility TileVisibility {
            get {
                return _tileVisibility;
            }
            set {
                //if (_tileVisibility != value) 
                {
                    _tileVisibility = value;
                    OnPropertyChanged(nameof(TileVisibility));
                }
            }
        }
        #endregion

        #region Business Logic Properties

        //private bool _isLoading = true;
        public bool IsLoading {
            get {
                return CopyItem == null || CopyItem.CopyItemId == 0;
            }
            //set {
            //    if(_isLoading != value) {
            //        _isLoading = value;
            //        OnPropertyChanged(nameof(IsLoading));
            //        OnPropertyChanged(nameof(LoadingSpinnerVisibility));
            //        OnPropertyChanged(nameof(ContentVisibility));
            //        OnPropertyChanged(nameof(RtbVisibility));
            //        OnPropertyChanged(nameof(ImgVisibility));
            //        OnPropertyChanged(nameof(FileListVisibility));
            //    }
            //} 
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

        public ObservableCollection<TextRange> LastContentHighlightRangeList { get; set; } = new ObservableCollection<TextRange>();

        public ObservableCollection<TextRange> LastTitleHighlightRangeList { get; set; } = new ObservableCollection<TextRange>();

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
                if (IsSelected) {
                    return Brushes.Red;
                }
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region State Properties        
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
                    OnPropertyChanged(nameof(EditToolbarVisibility));
                    OnPropertyChanged(nameof(RtbHeight));
                    //OnPropertyChanged(nameof(PasteTemplateToolbarVisibility));
                    OnPropertyChanged(nameof(RtbHorizontalScrollbarVisibility));
                    OnPropertyChanged(nameof(RtbVerticalScrollbarVisibility));
                    OnPropertyChanged((nameof(CopyItemRichText)));
                    OnPropertyChanged(nameof(CopyItem));
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
                    OnPropertyChanged(nameof(RtbHeight));
                    OnPropertyChanged(nameof(EditTemplateToolbarVisibility));
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
                    OnPropertyChanged(nameof(PasteTemplateToolbarVisibility));
                    OnPropertyChanged(nameof(RtbHeight));
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
                }
            }
        }

        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                //if (_isHovering != value) 
                {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(DetailTextColor));
                }
            }
        }
        #endregion

        #region Focus Properties
        private bool _isClipTitleTextBoxFocused = false;
        public bool IsClipTitleTextBoxFocused {
            get {
                return _isClipTitleTextBoxFocused;
            }
            set {
                //omitting duplicate check to enforce change in ui
                //if (_isClipItemFocused != value) 
                {
                    _isClipTitleTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsClipTitleTextBoxFocused));
                }
            }
        }

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

        #region Text Editor Properties
        

        #endregion

        #region Model Properties
        public Brush TitleColor {
            get {
                if (CopyItem == null) {
                    return Brushes.Red;
                }
                return new SolidColorBrush(CopyItem.ItemColor.Color);
            }
            set {
                if (CopyItem.ItemColor.Color != ((SolidColorBrush)value).Color) {
                    CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(TitleColor));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

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
                if (CopyItem.Title != value) {
                    CopyItem.Title = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemTitle));
                    //OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public string CopyItemPlainText {
            get {
                if (CopyItem == null) {
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
                if (CopyItem.ItemRichText != value) {
                    //value should be raw rtf where templates are encoded into #name#color# groups
                    CopyItem.SetData(value);
                    CopyItem.WriteToDatabase();
                    //OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CopyItemRichText));
                    //OnPropertyChanged(nameof(TemplateTokenList));
                    //OnPropertyChanged(nameof(TemplateTokenLookupDictionary));
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
            get; set;
        }

        public BitmapSource TitleSwirl {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                return CopyItem.ItemTitleSwirl;
            }
            set {
                if (CopyItem.ItemTitleSwirl != value) {
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

        public string CopyItemAppName {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.App.AppName;
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
                if (CopyItem.CopyCount != value) {
                    CopyItem.CopyCount = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyCount));
                }
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
                if (CopyItem.CopyDateTime != value) {
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
                if (_copyItem != value) {
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
                    OnPropertyChanged(nameof(DetectedImageObjectCollectionViewModel));
                    OnPropertyChanged(nameof(LoadingSpinnerVisibility));
                    OnPropertyChanged(nameof(ContentVisibility));
                    OnPropertyChanged(nameof(IsLoading));
                    //CopyItem.WriteToDatabase();
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileViewModel(MpCopyItem ci) : base() {
            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(IsSelected):
                        if (IsSelected) {
                            //this check ensures that as user types in search that 
                            //resetselection doesn't take the focus from the search box
                            if (!MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                                IsClipItemFocused = true;
                            }
                        } else {
                            IsEditingTile = false;
                        }
                        break;
                }
            };
            
            object lockObj = new object();
            object lockObj2 = new object();
            BindingOperations.EnableCollectionSynchronization(LastTitleHighlightRangeList, lockObj);
            BindingOperations.EnableCollectionSynchronization(LastContentHighlightRangeList, lockObj2);
            SetCopyItem(ci);

            TemplateHyperlinkCollectionViewModel = new MpTemplateHyperlinkCollectionViewModel(this);
            EditTemplateToolbarViewModel = new MpEditTemplateToolbarViewModel(this);
            PasteTemplateToolbarViewModel = new MpPasteTemplateToolbarViewModel(this);
            EditRichTextBoxToolbarViewModel = new MpEditRichTextBoxToolbarViewModel(this);

            //TemplateHyperlinkCollectionViewModel.CollectionChanged += (s, e) => {
            //    if(e.NewItems != null) {
            //        foreach(MpTemplateHyperlinkViewModel nthlvm in e.NewItems) {
            //            if(!CopyItem.TemplateList.Contains(nthlvm.CopyItemTemplate)) {
            //                CopyItem.TemplateList.Add(nthlvm.CopyItemTemplate);
            //            }
            //        }
            //    }
            //    if (e.OldItems != null) {
            //        foreach (MpTemplateHyperlinkViewModel nthlvm in e.OldItems) {
            //            if (CopyItem.TemplateList.Contains(nthlvm.CopyItemTemplate)) {
            //                CopyItem.TemplateList.Remove(nthlvm.CopyItemTemplate);
            //            }
            //        }
            //    }
            //};
        }

        public void SetCopyItem(MpCopyItem ci) {
            if (ci == null) {
                CopyItem = new MpCopyItem();
                return;
            }
            if (ci.CopyItemId == 0) {
                ci.WriteToDatabase();
            }

            CopyItem = ci;
        }

        public RichTextBox GetRtb() {
            return _rtb;
        }

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTileBorder = (Grid)sender;
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
                if (e6.Key == Key.Down) {
                    rtb.FontSize -= 1;
                    e.Handled = true;
                } else if (e6.Key == Key.Up) {
                    rtb.FontSize += 1;
                    e.Handled = true;
                }
            };
            //clipTileBorder.PreviewMouseRightButtonUp += (s, e4) => {
            //    var p = e4.MouseDevice.GetPosition(clipTileBorder);
            //    var hitTestResult = VisualTreeHelper.HitTest(clipTileBorder, p);
            //    if (hitTestResult != null) {
            //        MessageBox.Show(hitTestResult.VisualHit.ToString());
            //    }
            //};
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
            clipTileTitleTextBlock.PreviewMouseLeftButtonDown += (s, e7) => {
                IsEditingTitle = true;
                e7.Handled = true;
            };

            var clipTileTitleTextBox = (TextBox)titleCanvas.FindName("ClipTileTitleTextBox");
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

            var titleIconImageButton = (Button)titleCanvas.FindName("ClipTileAppIconImageButton");
            Canvas.SetLeft(titleIconImageButton, TileBorderWidth - TileTitleHeight - 10);
            Canvas.SetTop(titleIconImageButton, 2);
            titleIconImageButton.PreviewMouseUp += (s, e7) => {
                // TODO (somehow) force mainwindow to stay active when switching or opening app process
                // TODO check if shift is down if so perform paste into target application
                // TODO check if App is running if it is switch to it or start its process

                MpHelpers.OpenUrl(CopyItem.App.AppPath);

            };

            var titleDetailTextBlock = (TextBlock)titleCanvas.FindName("ClipTileTitleDetailTextBlock");
            titleDetailTextBlock.MouseEnter += (s, e5) => {
                if (++_detailIdx > 2) {
                    _detailIdx = 0;
                }
                titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
            };
            titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
            Canvas.SetLeft(titleDetailTextBlock, 5);
            Canvas.SetTop(titleDetailTextBlock, TileTitleHeight - 14);
        }

        public void ClipTileRichTextStackPanel_Loaded(object sender, RoutedEventArgs e) {
            if (RtbVisibility == Visibility.Collapsed) {
                return;
            }
            var rtbc = (Canvas)sender;
            var rtb = (RichTextBox)rtbc.FindName("ClipTileRichTextBox");
            
            var hb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);
            var hfb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightFocusedHexColorString);

            //rtb.ContextMenu = (ContextMenu)cb.FindName("ClipTile_ContextMenu");
            //rtb.CreateHyperlinks();
            rtb.Document.PageWidth = rtb.Width - rtb.Padding.Left - rtb.Padding.Right;
            rtb.Document.PageHeight = rtb.Height - rtb.Padding.Top - rtb.Padding.Bottom;
            _rtb = rtb;
                       
            //not sure why but calling this is only way templates are shown when loaded
            //TemplateTokenList = rtb.GetTemplateHyperlinkList();
            //HasTemplate = rtb.GetTemplateHyperlinkList().Count > 0;

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
                            IsPastingTemplateTile = false;

                            //rtb.ClearHyperlinks();

                            //clear any search highlighting when saving the document then restore after save
                            MpHelpers.ApplyBackgroundBrushToRangeList(LastTitleHighlightRangeList, Brushes.Transparent);
                            MpHelpers.ApplyBackgroundBrushToRangeList(LastContentHighlightRangeList, Brushes.Transparent);
                            CopyItemRichText = MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);
                            MpHelpers.ApplyBackgroundBrushToRangeList(LastTitleHighlightRangeList, hb);
                            MpHelpers.ApplyBackgroundBrushToRangeList(LastContentHighlightRangeList, hb);
                        }
                        rtb.ScrollToHome();
                        rtb.CaretPosition = rtb.Document.ContentStart;
                        rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);
                        break;
                    case nameof(CurrentHighlightMatchIdx):
                        int maxIdx = LastContentHighlightRangeList.Count + LastTitleHighlightRangeList.Count - 1;
                        if (CurrentHighlightMatchIdx < 0) {
                            _currentHighlightMatchIdx = maxIdx;
                        } else if (CurrentHighlightMatchIdx > maxIdx) {
                            _currentHighlightMatchIdx = 0;
                        }
                        MpHelpers.ApplyBackgroundBrushToRangeList(LastTitleHighlightRangeList, hb);
                        MpHelpers.ApplyBackgroundBrushToRangeList(LastContentHighlightRangeList, hb);
                        if (CurrentHighlightMatchIdx < LastTitleHighlightRangeList.Count) {
                            LastTitleHighlightRangeList[CurrentHighlightMatchIdx].ApplyPropertyValue(TextElement.BackgroundProperty, hfb);
                        } else if (CurrentHighlightMatchIdx < LastContentHighlightRangeList.Count) {
                            int contentIdx = CurrentHighlightMatchIdx - LastTitleHighlightRangeList.Count;
                            LastContentHighlightRangeList[contentIdx].ApplyPropertyValue(TextElement.BackgroundProperty, hfb);
                            //rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                            //rtb.CaretPosition = LastContentHighlightRangeList[contentIdx].End;
                            //Rect r = LastContentHighlightRangeList[contentIdx].End.GetCharacterRect(LogicalDirection.Backward);
                            //rtb.ScrollToVerticalOffset(r.Y);
                            var characterRect = LastContentHighlightRangeList[contentIdx].End.GetCharacterRect(LogicalDirection.Forward);
                            rtb.ScrollToHorizontalOffset(rtb.HorizontalOffset + characterRect.Left - rtb.ActualWidth / 2d);
                            rtb.ScrollToVerticalOffset(rtb.VerticalOffset + characterRect.Top - rtb.ActualHeight / 2d);

                        }
                        break;
                }
            };
            #endregion
        }

        public void ClipTileImageCanvas_Loaded(object sender, RoutedEventArgs e) {
            if (ImgVisibility == Visibility.Collapsed) {
                return;
            }
            var ic = (FrameworkElement)sender;
            var ctcc = ic.GetVisualAncestor<Canvas>();
            var vb = (Viewbox)ic.FindName("ClipTileImageItemsControlViewBox");

            vb.ContextMenu = ctcc.ContextMenu = ic.ContextMenu = (ContextMenu)((FrameworkElement)sender).GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");

            //CopyItem.ImageItemObjectList.Clear();
            //CopyItem.ImageItemObjectList.Add(new MpDetectedImageObject(0, CopyItemId, 0, 0, 0, 100, 100, "test"));
            DetectedImageObjectCollectionViewModel = new MpDetectedImageObjectCollectionViewModel(CopyItem);
        }

        public void ClipTileFileListBox_Loaded(object sender, RoutedEventArgs e) {
            if (FileListVisibility == Visibility.Collapsed) {
                return;
            }
            var flb = (ListBox)sender;
            flb.ContextMenu = (ContextMenu)flb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
        }

        public void ClipTile_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)sender;
            MenuItem cmi = null;
            foreach (MenuItem mi in cm.Items) {
                if (mi.Name == "ClipTileColorContextMenuItem") {
                    cmi = mi;
                    break;
                }
            }
            MpHelpers.SetColorChooserMenuItem(
                cm,
                cmi,
                (s, e1) => {
                    MainWindowViewModel.ClipTrayViewModel.ChangeSelectedClipsColorCommand.Execute(((Border)s).Background);
                },
                MpHelpers.GetColorColumn(TitleColor),
                MpHelpers.GetColorRow(TitleColor)
            );
        }

        public void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            //if (CopyItemType == MpCopyItemType.RichText) {
            //    var cm = (ContextMenu)sender;
            //    RichTextBox rtb = null;
            //    if (cm.PlacementTarget is Grid) {
            //        rtb = (RichTextBox)((Grid)cm.PlacementTarget).FindName("ClipTileRichTextBox");
            //    } else {
            //        rtb = (RichTextBox)cm.PlacementTarget;
            //    }
            //    if (IsEditingTile) {
            //        RichTextBox tempRtb = new RichTextBox();
            //        rtb.ContextMenu = tempRtb.ContextMenu;
            //        return;
            //    } else {
            //        rtb.ContextMenu = rtb.GetVisualAncestor<MpClipBorder>().ContextMenu;
            //    }
            //} 
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
            ConvertClipTypes.Clear();
            switch (MainWindowViewModel.ClipTrayViewModel.GetSelectedClipsType()) {
                case MpCopyItemType.None:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Text", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.RichText, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("File List", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.FileList, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Image", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.Image, false));
                    break;
                case MpCopyItemType.FileList:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Text", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.RichText, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Image", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.Image, false));
                    break;
                case MpCopyItemType.Image:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Text", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.RichText, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("File List", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.FileList, false));
                    break;
                case MpCopyItemType.RichText:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("File List", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.FileList, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Image", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.Image, false));
                    break;
            }
        }

        public void AppendContent(MpClipTileViewModel octvm) {
            CopyItem.Combine(octvm.CopyItem);
            OnPropertyChanged(nameof(CopyItem));
        }

        public async Task<string> GetPastableRichText() {
            if (HasTemplate) {
                IsEditingTile = false;
                IsPastingTemplateTile = true;
                var temp = CopyItemRichText;
                await Task.Run(() => {
                    while (string.IsNullOrEmpty(TemplateRichText)) {
                        System.Threading.Thread.Sleep(500);
                    }
                    //TemplateRichText is set in PasteTemplateCommand
                });
                CopyItemRichText = temp;
                return TemplateRichText;
            }
            return CopyItemRichText;
        }

        public void DeleteTempFiles() {
            foreach (var f in _tempFileList) {
                if (File.Exists(f)) {
                    File.Delete(f);
                }
            }
        }

        public void ConvertContent(MpCopyItemType newType) {
            CopyItem.ConvertType(newType);
            OnPropertyChanged(nameof(CopyItem));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchGoogle() {
            MpHelpers.OpenUrl(@"https://www.google.com/search?q=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchBing() {
            MpHelpers.OpenUrl(@"https://www.bing.com/search?q=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchDuckDuckGo() {
            MpHelpers.OpenUrl(@"https://duckduckgo.com/?q=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchYandex() {
            MpHelpers.OpenUrl(@"https://yandex.com/search/?text=" + System.Uri.EscapeDataString(CopyItemPlainText));
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
                        info = "(" + (int)CopyItemBmp.Width + ") x (" + (int)CopyItemBmp.Height + ")";
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

        private RelayCommand<string> _translateClipTextCommand;
        public ICommand TranslateClipTextCommand {
            get {
                if (_translateClipTextCommand == null) {
                    _translateClipTextCommand = new RelayCommand<string>(TranslateClipText, CanTranslateClipText);
                }
                return _translateClipTextCommand;
            }
        }
        private bool CanTranslateClipText(string toLanguage) {
            return CopyItemType == MpCopyItemType.RichText;
        }
        private async void TranslateClipText(string toLanguage) {
            var translatedText = await MpLanguageTranslator.Instance.Translate(CopyItemPlainText, toLanguage, false);
            if (!string.IsNullOrEmpty(translatedText)) {
                CopyItemRichText = MpHelpers.ConvertPlainTextToRichText(translatedText);
                //RtbFontFamily = new FontFamily("Arial");

            }
        }

        private RelayCommand _pasteTemplateCommand;
        public ICommand PasteTemplateCommand {
            get {
                if (_pasteTemplateCommand == null) {
                    _pasteTemplateCommand = new RelayCommand(PasteTemplate, CanPasteTemplate);
                }
                return _pasteTemplateCommand;
            }
        }
        private bool CanPasteTemplate() {
            //only allow template to be pasted once all template types have been viewed
            return PasteTemplateToolbarViewModel.IsTemplateReadyToPaste;
        }
        private void PasteTemplate() {
            // TODO use code below to output templated rich text

            //var tempRtb = new RichTextBox();
            //tempRtb.Document = MpHelpers.ConvertRichTextToFlowDocument(CopyItemRichText);
            //tempRtb.CreateHyperlinks();

            //foreach (var templateCollection in PasteTemplateToolbarViewModel) {
            //    foreach(var templateLink in templateCollection) {
            //        //TextRange tr = new TextRange(templateLink.ElementStart, templateLink.ElementEnd);
            //        ///tr.Text = string.Empty;
            //        Span span = new Span(templateLink.TemplateStart, templateLink.TemplateEnd);
            //        span.Inlines.Clear();
            //        span.Inlines.Add(new Run(templateCollection.TemplateText));
            //    }
            //}
            TemplateRichText = MpHelpers.ConvertFlowDocumentToRichText(_rtb.Document);
            //Returned to GetPastableTemplate
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
            MpHelpers.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, CopyItemTitle, CopyItemPlainText));
            //MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            //IsSelected = true;
            //MpHelpers.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
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
                  (CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Image) &&
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
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1;
        }
        private void ShareClip() {
            MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.IsShowingDialog = true;
            IntPtr windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            var dtmHelper = new DataTransferManagerHelper(windowHandle);
            dtmHelper.DataTransferManager.DataRequested += (s, e) => {
                DataPackage dp = e.Request.Data;
                dp.Properties.Title = CopyItemTitle;
                switch (CopyItemType) {
                    case MpCopyItemType.RichText:
                        dp.SetText(MpHelpers.ConvertRichTextToPlainText(this.CopyItemRichText));
                        break;
                    case MpCopyItemType.FileList:
                    case MpCopyItemType.Image:
                        var filesToShare = new List<IStorageItem>();
                        foreach (string path in CopyItem.GetFileList()) {
                            //StorageFile sf = StorageFile.
                            //filesToShare.Add(sf);
                        }

                        dp.SetStorageItems(filesToShare);
                        break;
                }
            };

            dtmHelper.ShowShareUI();
            //MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.IsShowingDialog = false;
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

        private RelayCommand _assignHotkeyCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if (_assignHotkeyCommand == null) {
                    _assignHotkeyCommand = new RelayCommand(AssignHotkey);
                }
                return _assignHotkeyCommand;
            }
        }
        private void AssignHotkey() {
            ShortcutKeyList = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(this, "Paste " + CopyItemTitle, ShortcutKeyList, PasteClipCommand);
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
            MpHelpers.RunInShell(CopyItemPlainText, false, false, IntPtr.Zero);
        }
        #endregion

        #region Overrides

        public override string ToString() {
            return CopyItem.ItemPlainText;
        }

        #endregion
    }
}
