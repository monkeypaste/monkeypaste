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

    public class AnimatedVisibilityFadeBehavior : Behavior<MpClipBorder> {
        public Duration AnimationDuration { get; set; }
        public Visibility InitialState { get; set; }

        DoubleAnimation m_animationOut;
        DoubleAnimation m_animationIn;

        protected override void OnAttached() {
            base.OnAttached();

            m_animationIn = new DoubleAnimation(1, AnimationDuration, FillBehavior.HoldEnd);
            m_animationOut = new DoubleAnimation(0, AnimationDuration, FillBehavior.HoldEnd);
            m_animationOut.Completed += (sender, args) => {
                AssociatedObject.SetCurrentValue(Border.VisibilityProperty, Visibility.Collapsed);
            };

            AssociatedObject.SetCurrentValue(Border.VisibilityProperty,
                                             InitialState == Visibility.Collapsed
                                                ? Visibility.Collapsed
                                                : Visibility.Visible);

            Binding.AddTargetUpdatedHandler(AssociatedObject, Updated);
        }

        private void Updated(object sender, DataTransferEventArgs e) {
            var value = (Visibility)AssociatedObject.GetValue(Border.VisibilityProperty);
            switch (value) {
                case Visibility.Collapsed:
                    AssociatedObject.SetCurrentValue(Border.VisibilityProperty, Visibility.Visible);
                    AssociatedObject.BeginAnimation(Border.OpacityProperty, m_animationOut);
                    break;
                case Visibility.Visible:
                    AssociatedObject.BeginAnimation(Border.OpacityProperty, m_animationIn);
                    break;
            }
        }
    }
    public class MpClipTileViewModel : MpViewModelBase {
        #region Private Variables

        private int _detailIdx = 0;

        private List<string> _tempFileList = new List<string>();

        private ToggleButton _selectedAlignmentButton = null;
        private ToggleButton _selectedListButton = null;

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
                if(CopyItem == null) {
                    return new ObservableCollection<MpFileListItemViewModel>();
                }
                var fileListViewModels = new ObservableCollection<MpFileListItemViewModel>();
                foreach (var path in CopyItem.GetFileList()) {
                    fileListViewModels.Add(new MpFileListItemViewModel(this, path));
                }
                return fileListViewModels;
            }
        }

        private ObservableCollection<MpDetectedImageObjectViewModel> _detectedImageObjectViewModels = new ObservableCollection<MpDetectedImageObjectViewModel>();
        public ObservableCollection<MpDetectedImageObjectViewModel> DetectedImageObjectViewModels {
            get {
                //if (CopyItem == null) {
                //    return new ObservableCollection<MpDetectedImageObjectViewModel>();
                //}
                //var diovms = new ObservableCollection<MpDetectedImageObjectViewModel>();
                //foreach(var dio in CopyItem.ImageItemObjectList) {
                //    diovms.Add(new MpDetectedImageObjectViewModel(dio));
                //}
                //return diovms;
                return _detectedImageObjectViewModels;
            }
            set {
                if (_detectedImageObjectViewModels != value) {
                    _detectedImageObjectViewModels = value;
                    OnPropertyChanged(nameof(DetectedImageObjectViewModels));
                }
            }
        }

        private ObservableCollection<MpTemplateHyperlinkViewModel> _templateTokens = new ObservableCollection<MpTemplateHyperlinkViewModel>();
        public ObservableCollection<MpTemplateHyperlinkViewModel> TemplateTokens {
            get {
                return _templateTokens;
            }
            set {
                if (_templateTokens != value) {
                    _templateTokens = value;
                    OnPropertyChanged(nameof(TemplateTokens));
                }
            }
        }

        //private ObservableCollection<MpClipTileContextMenuItemViewModel> _translateLanguageMenuItems = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
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

        public double RichTextBoxHeight {
            get {
                double h = MpMeasurements.Instance.ClipTileContentHeight;
                if (EditToolbarVisibility == Visibility.Visible) {
                    h -= TileContentEditToolbarHeight;
                }
                if (PasteTemplateToolbarVisibility == Visibility.Visible) {
                    h -= TileContentPasteToolbarHeight;
                }
                return h;
            }
        }

        public double TileContentEditToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTileEditToolbarHeight;
            }
        }

        public double TileContentPasteToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTileEditToolbarHeight;
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
                if(IsLoading) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ContentVisibility {
            get {
                if(IsLoading) {
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
                    OnPropertyChanged(nameof(RichTextBoxHeight));
                    OnPropertyChanged(nameof(PasteTemplateToolbarVisibility));
                    OnPropertyChanged((nameof(CopyItemRichText)));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        private bool _isTemplateReadyToPaste = false;
        public bool IsTemplateReadyToPaste {
            get {
                return _isTemplateReadyToPaste;
            }
            set {
                if (_isTemplateReadyToPaste != value) {
                    _isTemplateReadyToPaste = value;
                    OnPropertyChanged(nameof(IsTemplateReadyToPaste));
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
                    OnPropertyChanged(nameof(IsEditingTile));
                    OnPropertyChanged(nameof(IsRtbReadOnly));
                    OnPropertyChanged(nameof(ContentCursor));
                    OnPropertyChanged(nameof(EditToolbarVisibility));
                    OnPropertyChanged(nameof(PasteTemplateToolbarVisibility));
                    OnPropertyChanged(nameof(RichTextBoxHeight));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public bool IsTileAnimating { get; set; } = false;

        public bool HasTemplate { get; set; } = false;

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
                //if (_isSelected != value) 
                {
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

        #region Template Toolbar Properties
        public Visibility PasteTemplateToolbarVisibility {
            get {
                if (IsPastingTemplateTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ClearAllTemplateToolbarButtonVisibility {
            get {
                foreach (var templateLookup in TemplateTokenLookupDictionary) {
                    if (!string.IsNullOrEmpty(templateLookup.Value)) {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ClearCurrentTemplateTextboxButtonVisibility {
            get {
                if (CurrentTemplateText.Length > 0 &&
                    CurrentTemplateText != CurrentTemplateTextBoxPlaceHolderText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility TemplateNavigationButtonStackVisibility {
            get {
                if (TemplateTokenLookupDictionary.Count > 0) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private int _currentTemplateLookupIdx = 0;
        public int CurrentTemplateLookupIdx {
            get {
                return _currentTemplateLookupIdx;
            }
            set {
                if (_currentTemplateLookupIdx != value) {
                    _currentTemplateLookupIdx = value;
                    OnPropertyChanged(nameof(CurrentTemplateLookupIdx));
                    OnPropertyChanged(nameof(CurrentTemplateText));
                    OnPropertyChanged(nameof(CurrentTemplateTextBoxFontStyle));
                    OnPropertyChanged(nameof(CurrentTemplateTextBrush));
                    OnPropertyChanged(nameof(ClearCurrentTemplateTextboxButtonVisibility));
                    OnPropertyChanged(nameof(CurrentTemplateTextBoxPlaceHolderText));
                }
            }
        }

        public string CurrentTemplateText {
            get {
                if (TemplateTokenLookupDictionary.Count == 0) {
                    return string.Empty;
                }
                var curTemplateText = TemplateTokenLookupDictionary.ElementAt(CurrentTemplateLookupIdx).Value;
                if (string.IsNullOrEmpty(curTemplateText) && !IsCurrentTemplateTextBoxFocused) {
                    return CurrentTemplateTextBoxPlaceHolderText;
                }
                return curTemplateText;
            }
            set {
                if (!string.IsNullOrEmpty(value) && TemplateTokenLookupDictionary.Count > 0 && TemplateTokenLookupDictionary.ElementAt(CurrentTemplateLookupIdx).Value != value) {
                    var templateName = TemplateTokenLookupDictionary.ElementAt(CurrentTemplateLookupIdx).Key;
                    TemplateTokenLookupDictionary[templateName] = value;

                    bool canPaste = true;
                    foreach (var thlvm in TemplateTokens) {
                        if (thlvm.TemplateName == templateName) {
                            thlvm.IsEditMode = true;
                            thlvm.IsSelected = true;
                            thlvm.WasTypeViewed = true;
                            thlvm.TemplateText = TemplateTokenLookupDictionary[templateName];
                        } else {
                            thlvm.IsEditMode = false;
                            thlvm.IsSelected = false;
                        }
                        if (thlvm.WasTypeViewed == false) {
                            canPaste = false;
                        }
                    }
                    if (canPaste) {
                        IsTemplateReadyToPaste = true;
                    }
                    OnPropertyChanged(nameof(CurrentTemplateText));
                    OnPropertyChanged(nameof(TemplateTokens));
                    OnPropertyChanged(nameof(CurrentTemplateTextBoxFontStyle));
                    OnPropertyChanged(nameof(CurrentTemplateTextBrush));
                    OnPropertyChanged(nameof(ClearCurrentTemplateTextboxButtonVisibility));
                    OnPropertyChanged(nameof(ClearAllTemplateToolbarButtonVisibility));
                }
            }
        }

        public string CurrentTemplateTextBoxPlaceHolderText {
            get {
                if (TemplateTokenLookupDictionary.Count == 0) {
                    return string.Empty;
                }
                return TemplateTokenLookupDictionary.ElementAt(CurrentTemplateLookupIdx).Key + "...";
            }
        }

        public Brush CurrentTemplateTextBrush {
            get {
                if (CurrentTemplateText != CurrentTemplateTextBoxPlaceHolderText) {
                    return Brushes.Black;
                }
                return Brushes.DimGray;
            }
        }

        public FontStyle CurrentTemplateTextBoxFontStyle {
            get {
                if (CurrentTemplateText == CurrentTemplateTextBoxPlaceHolderText) {
                    return FontStyles.Italic;
                }
                return FontStyles.Normal;
            }
        }

        private Dictionary<string, string> _templateTokenLookupDictionary = new Dictionary<string, string>();
        public Dictionary<string, string> TemplateTokenLookupDictionary {
            get {
                return _templateTokenLookupDictionary;
            }
            set {
                if (_templateTokenLookupDictionary != value) {
                    _templateTokenLookupDictionary = value;
                    OnPropertyChanged(nameof(TemplateTokenLookupDictionary));
                    OnPropertyChanged(nameof(TemplateNavigationButtonStackVisibility));
                }
            }
        }

        public bool IsPasteToolbarAnimating { get; set; } = false;
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

        private bool _isCurrentTemplateTextBoxFocused = false;
        public bool IsCurrentTemplateTextBoxFocused {
            get {
                return _isCurrentTemplateTextBoxFocused;
            }
            set {
                if (_isCurrentTemplateTextBoxFocused != value) {
                    _isCurrentTemplateTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsCurrentTemplateTextBoxFocused));
                    OnPropertyChanged(nameof(CurrentTemplateText));
                    OnPropertyChanged(nameof(CurrentTemplateTextBoxFontStyle));
                    OnPropertyChanged(nameof(CurrentTemplateTextBrush));
                }
            }
        }
        #endregion

        #region Text Editor Properties
        public ICollection<FontFamily> SystemFonts {
            get {
                return Fonts.SystemFontFamilies;
            }
        }

        private List<string> _fontSizeList = null;
        public List<string> FontSizeList {
            get {
                if (_fontSizeList == null) {
                    _fontSizeList = new List<string>() {
                        "10",
                        "12",
                        "14",
                        "18",
                        "24",
                        "36"
                    };
                }
                return _fontSizeList;
            }
            set {
                if (_fontSizeList != value) {
                    _fontSizeList = value;
                    OnPropertyChanged(nameof(FontSizeList));
                }
            }
        }

        public Cursor ContentCursor {
            get {
                if (IsEditingTile) {
                    return Cursors.IBeam;
                }
                return Cursors.Arrow;
            }
        }

        public Visibility EditToolbarVisibility {
            get {
                if (IsEditingTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private Visibility _confirmEditButtonVisibility = Visibility.Visible;
        public Visibility ConfirmEditButtonVisibility {
            get {
                return _confirmEditButtonVisibility;
            }
            set {
                if (_confirmEditButtonVisibility != value) {
                    _confirmEditButtonVisibility = value;
                    OnPropertyChanged(nameof(ConfirmEditButtonVisibility));
                }
            }
        }

        private bool _isAddTemplateButtonEnabled = true;
        public bool IsAddTemplateButtonEnabled {
            get {
                return _isAddTemplateButtonEnabled;
            }
            set {
                if (_isAddTemplateButtonEnabled != value) {
                    _isAddTemplateButtonEnabled = value;
                    OnPropertyChanged(nameof(IsAddTemplateButtonEnabled));
                    OnPropertyChanged(nameof(AddTemplateButtonBackgroundBrush));
                }
            }
        }

        public Brush AddTemplateButtonBackgroundBrush {
            get {
                if (IsAddTemplateButtonEnabled) {
                    return Brushes.Transparent;
                }
                return Brushes.LightGray;
            }
        }

        public bool IsEditToolbarAnimating { get; set; } = false;
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
                    //CopyItem.ItemTitleSwirl = CopyItem.InitSwirl();
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

        private BitmapSource _viewBmp = null;
        public BitmapSource ViewBmp {
            get {
                return _viewBmp;
            }
            set {
                if(_viewBmp != value) {
                    _viewBmp = value;
                    OnPropertyChanged(nameof(ViewBmp));
                    OnPropertyChanged(nameof(DetectedImageObjectViewModels));
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
                if(CopyItem.CopyCount != value) {
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
                if(CopyItem == null) {
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
                if (_copyItem != value) 
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
                    OnPropertyChanged(nameof(DetectedImageObjectViewModels));
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
        public MpClipTileViewModel() : this(null) {
            //empty constructor only directly called when creating from the clipboard

            
        }

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
                            //below must be called to clear focus when deselected (it may not have focus)
                            //IsClipItemFocused = false;
                            //IsClipRichTextBoxFocused = false;
                            //IsClipTitleTextBoxFocused = false;
                            //IsCurrentTemplateTextBoxFocused = false;
                            IsEditingTile = false;
                            //IsPastingTemplateTile = false;
                        }
                        if (TemplateTokens != null) {
                            foreach (var tthlvm in TemplateTokens) {
                                //tthlvm.OnPropertyChanged(nameof(tthlvm.DeleteTemplateTextButtonVisibility));
                                tthlvm.DeleteTemplateTextButtonVisibility = IsSelected ? Visibility.Visible : Visibility.Hidden;
                            }
                        }
                        break;
                    case nameof(IsEditingTile):
                        MainWindowViewModel.SearchBoxViewModel.IsSearchEnabled = IsEditingTile;
                        var hb = IsEditingTile ? Brushes.Transparent : (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);
                        MpHelpers.ApplyBackgroundBrushToRangeList(LastTitleHighlightRangeList, hb);
                        MpHelpers.ApplyBackgroundBrushToRangeList(LastContentHighlightRangeList, hb);
                        break;
                }
            };
            object lockObj = new object();
            object lockObj2 = new object();
            BindingOperations.EnableCollectionSynchronization(LastTitleHighlightRangeList, lockObj);
            BindingOperations.EnableCollectionSynchronization(LastContentHighlightRangeList, lockObj2);
            SetCopyItem(ci);
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

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTileBorder = (Grid)sender;
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
                if (e5.ClickCount == 2 && !IsEditingTile) {
                    //only for richtext type
                    EditClipTextCommand.Execute(null);
                    e5.Handled = true;
                    return;
                }
            };
            clipTileBorder.KeyDown += (s, e6) => {
                if(CopyItemType != MpCopyItemType.RichText) {
                    return;
                }
                var rtb = (RichTextBox)((FrameworkElement)s).FindName("ClipTileRichTextBox");
                if(e6.Key == Key.Down) {
                    rtb.FontSize -= 1;
                } else if(e6.Key == Key.Up) {
                    rtb.FontSize += 1;
                }
            };
            //clipTileBorder.IsVisibleChanged += (s, e7) => {
            //    double startOpacity, endOpacity;
            //    if (TileVisibility == Visibility.Visible) {
            //        startOpacity = 0;
            //        endOpacity = 1;
            //    } else {
            //        startOpacity = 1;
            //        endOpacity = 0;
            //    }
            //    MpHelpers.AnimateDoubleProperty(
            //        startOpacity, 
            //        endOpacity,
            //        Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
            //        clipTileBorder,
            //        FrameworkElement.OpacityProperty, null);
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
                if(TileTitleTextBoxVisibility == Visibility.Collapsed) {
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
                if(++_detailIdx > 2) {
                    _detailIdx = 0;
                }
                titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
            };
            titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
            Canvas.SetLeft(titleDetailTextBlock, 5);
            Canvas.SetTop(titleDetailTextBlock, TileTitleHeight - 14);
        }

        public void ClipTileRichTextStackPanel_Loaded(object sender, RoutedEventArgs e) {
            if(RtbVisibility == Visibility.Collapsed) {
                return;
            }
            var sp = (StackPanel)sender;
            var ct = (MpMultiSelectListBox)sp.GetVisualAncestor<MpMultiSelectListBox>();
            var cb = (MpClipBorder)sp.GetVisualAncestor<MpClipBorder>();
            var rtb = (RichTextBox)sp.FindName("ClipTileRichTextBox");
            var et = (Border)sp.FindName("ClipTileEditorToolbar");
            var cet = (Border)sp.FindName("ClipTileConfirmEditToolbar");
            var pt = (Border)sp.FindName("ClipTilePasteTemplateToolbar");
            var catb = (Button)pt.FindName("ClearAllTemplatesButton");
            var cttb = (TextBox)pt.FindName("CurrentTemplateTextBox");
            var prvtb = (Button)pt.FindName("PreviousTemplateButton");
            var ntb = (Button)pt.FindName("NextTemplateButton");
            var ptb = (Button)pt.FindName("PasteTemplateButton");
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var hb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);
            var hfb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightFocusedHexColorString);

            rtb.ContextMenu = (ContextMenu)cb.FindName("ClipTile_ContextMenu");
            rtb.Document.PageWidth = rtb.Width - rtb.Padding.Left - rtb.Padding.Right;
            rtb.Document.PageHeight = rtb.Height - rtb.Padding.Top - rtb.Padding.Bottom;

            //not sure why but calling this is only way templates are shown when loaded
            //TemplateTokenList = rtb.GetTemplateHyperlinkList();
            HasTemplate = rtb.GetTemplateHyperlinkList().Count > 0;

            #region Editor 

            var fontFamilyComboBox = (ComboBox)et.FindName("FontFamilyCombo");
            fontFamilyComboBox.SelectionChanged += (s, e1) => {
                if (fontFamilyComboBox.SelectedItem == null) {
                    return;
                }
                var fontFamily = fontFamilyComboBox.SelectedItem.ToString();
                var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
            };

            var fontSizeCombo = (ComboBox)et.FindName("FontSizeCombo");
            fontSizeCombo.SelectionChanged += (s, e1) => {
                // Exit if no selection
                if (fontSizeCombo.SelectedItem == null) {
                    return;
                }

                // clear selection if value unset
                if (fontSizeCombo.SelectedItem.ToString() == "{DependencyProperty.UnsetValue}") {
                    fontSizeCombo.SelectedItem = null;
                    return;
                }

                // Process selection
                var pointSize = fontSizeCombo.SelectedItem.ToString();
                var pixelSize = System.Convert.ToDouble(pointSize) * (96 / 72);
                var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                textRange.ApplyPropertyValue(TextElement.FontSizeProperty, pixelSize);
            };

            var leftAlignmentButton = (ToggleButton)et.FindName("LeftButton");
            var centerAlignmentButton = (ToggleButton)et.FindName("CenterButton");
            var rightAlignmentButton = (ToggleButton)et.FindName("RightButton");
            var justifyAlignmentButton = (ToggleButton)et.FindName("JustifyButton");
            leftAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, _selectedAlignmentButton, buttonGroup, true);
                _selectedAlignmentButton = clickedButton;
            };
            centerAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, _selectedAlignmentButton, buttonGroup, true);
                _selectedAlignmentButton = clickedButton;
            };
            rightAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, _selectedAlignmentButton, buttonGroup, true);
                _selectedAlignmentButton = clickedButton;
            };
            justifyAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, _selectedAlignmentButton, buttonGroup, true);
                _selectedAlignmentButton = clickedButton;
            };

            var bulletsButton = (ToggleButton)et.FindName("BulletsButton");
            var numberingButton = (ToggleButton)et.FindName("NumberingButton");
            bulletsButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new[] { bulletsButton, numberingButton };
                this.SetButtonGroupSelection(clickedButton, _selectedListButton, buttonGroup, false);
                _selectedListButton = clickedButton;
            };
            numberingButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new[] { bulletsButton, numberingButton };
                this.SetButtonGroupSelection(clickedButton, _selectedListButton, buttonGroup, false);
                _selectedListButton = clickedButton;
            };

            var hiddenClipTiles = new List<MpClipTileViewModel>();
            et.IsVisibleChanged += (s, e1) => {
                double fromBottomEditToolbar = 0;
                double toBottomEditToolbar = 24;

                double fromTopConfirmEditToolbar = MpMeasurements.Instance.ClipTileBorderSize;
                double toTopConfirmEditToolbar = fromTopConfirmEditToolbar - 30;

                double fromWidthTile = cb.ActualWidth;
                double toWidthTile = 0;

                if (et.Visibility == Visibility.Visible) {
                    //hide and store all other clip tiles
                    foreach (var vctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
                        if (vctvm != this) {
                            hiddenClipTiles.Add(vctvm);
                            vctvm.TileVisibility = Visibility.Collapsed;
                        }
                    }

                    toWidthTile = Math.Max(625, rtb.Document.GetFormattedText().WidthIncludingTrailingWhitespace);

                    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    rtb.ScrollToHome();
                    rtb.CaretPosition = rtb.Document.ContentStart;

                    //clear all other visible
                    

                } else {
                    //restore all other clip tiles
                    foreach (var vctvm in hiddenClipTiles) {
                        vctvm.TileVisibility = Visibility.Visible;
                    }
                    hiddenClipTiles.Clear();
                    toWidthTile = MpMeasurements.Instance.ClipTileBorderSize;
                    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

                    rtb.ClearHyperlinks();
                    //clear any search highlighting when saving the document then restore after save
                    MpHelpers.ApplyBackgroundBrushToRangeList(LastTitleHighlightRangeList, Brushes.Transparent);
                    MpHelpers.ApplyBackgroundBrushToRangeList(LastContentHighlightRangeList, Brushes.Transparent);
                    CopyItemRichText = MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);
                    MpHelpers.ApplyBackgroundBrushToRangeList(LastTitleHighlightRangeList, hb);
                    MpHelpers.ApplyBackgroundBrushToRangeList(LastContentHighlightRangeList, hb);
                }
                IsEditToolbarAnimating = true;
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
                    //Point p = new Point();
                    //p = cb.TranslatePoint(p, ct);
                    //Rect rectangleBounds = cb.RenderTransform.TransformBounds(new Rect(p.X, p.Y, cb.Width, cb.Height));
                    //ct.ScrollViewer.ScrollToHorizontalOffset(rectangleBounds.Right);
                    IsEditToolbarAnimating = false;
                };
                cb.BeginAnimation(MpClipBorder.WidthProperty, twa);
                titleSwirl.BeginAnimation(Image.WidthProperty, twa);
                rtb.BeginAnimation(RichTextBox.WidthProperty, twa);
                et.BeginAnimation(Border.WidthProperty, twa);
                pt.BeginAnimation(Border.WidthProperty, twa);
                sp.BeginAnimation(StackPanel.WidthProperty, twa);

                DoubleAnimation la = new DoubleAnimation();
                la.From = fromLeft;
                la.To = toLeft;
                la.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
                la.EasingFunction = easing;
                titleIconImageButton.BeginAnimation(Canvas.LeftProperty, la);

                DoubleAnimation da = new DoubleAnimation();
                da.From = fromBottomEditToolbar;
                da.To = toBottomEditToolbar;
                la.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
                la.EasingFunction = easing;
                //et.BeginAnimation(Border.Ma, la);
            };

            rtb.SelectionChanged += (s, e6) => {
                // Set font family combo
                var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                var fontFamily = textRange.GetPropertyValue(TextElement.FontFamilyProperty);
                fontFamilyComboBox.SelectedItem = fontFamily;

                // Set font size combo
                var fontSize = textRange.GetPropertyValue(TextElement.FontSizeProperty);
                fontSizeCombo.Text = fontSize.ToString();

                // Set Font buttons
                ((ToggleButton)et.FindName("BoldButton")).IsChecked = textRange.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
                ((ToggleButton)et.FindName("ItalicButton")).IsChecked = textRange.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
                ((ToggleButton)et.FindName("UnderlineButton")).IsChecked = textRange?.GetPropertyValue(Inline.TextDecorationsProperty)?.Equals(TextDecorations.Underline);

                // Set Alignment buttons
                leftAlignmentButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
                centerAlignmentButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
                rightAlignmentButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);
                justifyAlignmentButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Justify);

                IsAddTemplateButtonEnabled = true;
                foreach (var templateHyperlink in rtb.GetTemplateHyperlinkList()) {
                    if (!textRange.Start.IsInSameDocument(templateHyperlink.ContentStart) ||
                       !textRange.Start.IsInSameDocument(templateHyperlink.ContentEnd)) {
                        continue;
                    }
                    if ((textRange.Start.CompareTo(templateHyperlink.ContentStart) >= 0 &&
                        textRange.Start.CompareTo(templateHyperlink.ContentEnd) <= 0) ||
                       (textRange.End.CompareTo(templateHyperlink.ContentStart) >= 0 &&
                        textRange.End.CompareTo(templateHyperlink.ContentEnd) <= 0)) {
                        IsAddTemplateButtonEnabled = false;
                    }
                }
                OnPropertyChanged(nameof(IsAddTemplateButtonEnabled));
            };

            //rtb.LostFocus += (s, e3) => {
            //    IsEditingTile = false;
            //};
            //rtb.LostKeyboardFocus += (s, e7) => {
            //    e7.Handled = true;
            //};

            //rtb.LostFocus += (s, e7) => {
            //    e7.Handled = true;
            //};
            
            rtb.TextChanged += (s, e8) => {

            };
            #endregion

            #region Templates
            var addTemplateButton = (Button)et.FindName("AddTemplateButton");
            addTemplateButton.PreviewMouseDown += (s, e3) => {
                e3.Handled = true;
                //rtb.Focus();
                // note! must be preview mouse down to retain rtb selection
                //TextRange ts = rtb.Selection;
                //gather unique list of all templates
                rtb.ClearHyperlinks();
                rtb.CreateHyperlinks();
                var templateTokens = rtb.GetTemplateHyperlinkList(true);
                //rtb.Selection.Select(ts.Start,ts.End);
                if (templateTokens.Count == 0) {
                    //if tags are NOT in the clip yet add one w/ default name
                    MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(rtb, null, true);
                } else {
                    var templateContextMenu = new ContextMenu();
                    foreach (var tl in templateTokens) {
                        DockPanel dp = new DockPanel();
                        Rectangle rect = new Rectangle();
                        rect.Fill = tl.Background;
                        rect.Width = 14;
                        rect.Height = 14;
                        rect.VerticalAlignment = VerticalAlignment.Center;
                        rect.HorizontalAlignment = HorizontalAlignment.Left;
                        dp.Children.Add(rect);
                        rect.SetValue(DockPanel.DockProperty, Dock.Left);
                        TextBlock tb = new TextBlock();
                        tb.Text = tl.TargetName;
                        tb.FontSize = 14;
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        tb.VerticalAlignment = VerticalAlignment.Center;
                        dp.Children.Add(tb);
                        tb.SetValue(DockPanel.DockProperty, Dock.Right);
                        MenuItem tmi = new MenuItem();
                        tmi.Header = dp;
                        tmi.Click += (s1, e5) => {
                            MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(rtb, tl, false);
                        };
                        templateContextMenu.Items.Add(tmi);
                    }
                    var addNewMenuItem = new MenuItem();
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = "Add New...";
                    tb2.FontSize = 14;
                    tb2.HorizontalAlignment = HorizontalAlignment.Left;
                    tb2.VerticalAlignment = VerticalAlignment.Center;
                    addNewMenuItem.Header = tb2;
                    addNewMenuItem.Click += (s1, e5) => {
                        MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(rtb, null, true);
                    };
                    templateContextMenu.Items.Add(addNewMenuItem);
                    addTemplateButton.ContextMenu = templateContextMenu;
                    templateContextMenu.PlacementTarget = addTemplateButton;
                    templateContextMenu.IsOpen = true;
                }
                rtb.ScrollToHome();
                rtb.CaretPosition = rtb.Document.ContentStart;
            };

            cttb.GotFocus += (s, e4) => {
                if (string.IsNullOrEmpty(CurrentTemplateText) || 
                    CurrentTemplateText == CurrentTemplateTextBoxPlaceHolderText) {
                    CurrentTemplateText = string.Empty;
                }

                IsCurrentTemplateTextBoxFocused = true;
            };
            cttb.LostFocus += (s, e5) => {
                //ensures current template textbox show the template name as a placeholder when input is empty
                IsCurrentTemplateTextBoxFocused = false;
                if (string.IsNullOrEmpty(CurrentTemplateText) ||
                    CurrentTemplateText == CurrentTemplateTextBoxPlaceHolderText) {
                    CurrentTemplateText = CurrentTemplateTextBoxPlaceHolderText;
                }
            };
            cttb.IsVisibleChanged += (s, e9) => {
                //this is used to capture the current template text box when the paste toolbar is shown
                //to
                if (PasteTemplateToolbarVisibility == Visibility.Collapsed) {
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                //tbx.SelectAll();
            };

            catb.PreviewMouseLeftButtonUp += (s, e2) => {
                //when clear all is clicked it performs the ClearAllTemplate Command and this switches focus to 
                //first template tbx
                cttb.Focus();
                e2.Handled = false;
            };

            prvtb.MouseLeftButtonUp += (s, e1) => {
                if (IsTemplateReadyToPaste) {
                    //when navigating and all templates are filled set the fox to the 
                    //paste button which is enabled by IsTemplateReadyToPaste also
                    ptb.Focus();
                } else {
                    cttb.Focus();
                }
            };
            ntb.MouseLeftButtonUp += (s, e1) => {                
                if(IsTemplateReadyToPaste) {
                    //when navigating and all templates are filled set the fox to the 
                    //paste button which is enabled by IsTemplateReadyToPaste also
                    ptb.Focus();
                } else {
                    cttb.Focus();
                }
            };

            pt.IsVisibleChanged += (s, e1) => {
                if(pt.Visibility == Visibility.Visible) {
                    //this only occurs when templates exist in the clip and here they're dynamically gathered
                    //and the asycn TemplateRichText is cleared so GetPastableRichText awaits its setting
                    TemplateRichText = string.Empty;
                    HasTemplate = rtb.Tag != null && ((List<Hyperlink>)rtb.Tag).Count > 0;
                    TemplateTokenLookupDictionary = new Dictionary<string, string>();
                    var templatePattern = string.Format(
                        @"[{0}].*?[{0}].*?[{0}]",
                        Properties.Settings.Default.TemplateTokenMarker);
                    MatchCollection mc = Regex.Matches(
                        CopyItemPlainText,
                        templatePattern,
                        RegexOptions.IgnoreCase |
                        RegexOptions.Compiled |
                        RegexOptions.ExplicitCapture |
                        RegexOptions.Multiline);
                    foreach (Match m in mc) {
                        foreach (Group mg in m.Groups) {
                            foreach (Capture c in mg.Captures) {
                                var templateName = c.Value.Split(new string[] { Properties.Settings.Default.TemplateTokenMarker }, StringSplitOptions.RemoveEmptyEntries)[0];
                                if (TemplateTokenLookupDictionary.ContainsKey(templateName)) {
                                    continue;
                                } else {
                                    TemplateTokenLookupDictionary.Add(templateName, string.Empty);
                                }
                            }
                        }
                    }
                    OnPropertyChanged(nameof(TemplateNavigationButtonStackVisibility));

                    TemplateTokens.Clear();
                    foreach(var thl in rtb.GetTemplateHyperlinkList()) {
                        TemplateTokens.Add((MpTemplateHyperlinkViewModel)thl.DataContext);
                    }

                    //cttb.Focus();
                    //IsCurrentTemplateTextBoxFocused = true;
                } else {
                    IsPastingTemplateTile = false;
                }
            };
            #endregion

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
                switch(e2.PropertyName) {
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
            if(ImgVisibility == Visibility.Collapsed) {
                return;
            }
            var ic = (Canvas)sender;
            var img = (Image)ic.FindName("ClipTileImage");
            var doc = (Canvas)ic.FindName("ClipTileImageDetectedObjectsCanvas");

            img.ContextMenu = (ContextMenu)((FrameworkElement)sender).GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");

            Console.WriteLine("Image loaded");
            DetectedImageObjectViewModels.Add(
                new MpDetectedImageObjectViewModel(
                    new MpDetectedImageObject(0, CopyItemId, 0, 0, 0, 100, 100, "test")));

            //foreach (var dio in MpHelpers.DetectObjects(MpHelpers.ConvertBitmapSourceToByteArray(ViewBmp))) {
            //    DetectedImageObjectViewModels.Add(new MpDetectedImageObjectViewModel(dio));
            //}

            foreach(var dio in DetectedImageObjectViewModels) {
                Border b = new Border();
                //b.Width = dio.Width;
                //b.Height = dio.Height;
                b.BorderThickness = new Thickness(3);
                b.Background = Brushes.Green;
                b.Loaded += dio.ClipTileImageDetectedObjectCanvas_Loaded;

                MpHelpers.CreateBinding(dio, new PropertyPath(nameof(dio.BorderBrush)), b, Border.BorderBrushProperty);
                MpHelpers.CreateBinding(dio, new PropertyPath(nameof(dio.Width)), b, Border.WidthProperty);
                MpHelpers.CreateBinding(dio, new PropertyPath(nameof(dio.Height)), b, Border.HeightProperty);
                MpHelpers.CreateBinding(dio, new PropertyPath(nameof(dio.X)), b, Canvas.LeftProperty);
                MpHelpers.CreateBinding(dio, new PropertyPath(nameof(dio.Y)), b, Canvas.TopProperty);

                TextBox tb = new TextBox();
                tb.Background = Brushes.Black;
                tb.Foreground = Brushes.White;
                tb.HorizontalContentAlignment = HorizontalAlignment.Center;
                tb.VerticalContentAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.FontSize = 24;

                MpHelpers.CreateBinding(dio, new PropertyPath(nameof(dio.ObjectTypeName)), tb, TextBox.TextProperty, BindingMode.TwoWay);

                b.Child = tb;

                doc.Children.Add(b);
            }
            
            OnPropertyChanged(nameof(DetectedImageObjectViewModels));
            return;
            //var ac = (AdornedControl)((Grid)sender).FindName("ClipTileImageDetectedObjectAdornedControl");
            //var ic = (ItemsControl)((Grid)sender).FindName("ClipTileImageDetectedObjectItemscontrol");

            ////this resizes image to clip content so its longest side matches its respective content side's size
            ////and then the other side is adjusted based off the original image's aspect ratio


            //double contentWidth, contentHeight, offsetX = 0, offsetY = 0;
            //if (CopyItemBmp.Width >= CopyItemBmp.Height) {
            //    double ar = CopyItemBmp.Height / CopyItemBmp.Width;
            //    contentWidth = TileContentWidth;
            //    contentHeight = contentWidth * ar;
            //    offsetX = (TileContentWidth / 2) - (contentWidth / 2);
            //} else {
            //    double ar = CopyItemBmp.Width / CopyItemBmp.Height;
            //    contentHeight = TileContentHeight;
            //    contentWidth = contentHeight * ar;
            //    offsetY = (TileContentHeight / 2) - (contentHeight / 2);
            //}
            //ViewBmp = MpHelpers.ResizeBitmapSource(CopyItemBmp, new Size((int)contentWidth, (int)contentHeight));
            //ic.Width = contentWidth;
            //ic.Height = contentHeight;

            //this offset assume image is center aligned both horizontally and vertically



            //Canvas.SetTop(img, offsetY);
            //Canvas.SetTop(ic, offsetY);
            //Canvas.SetLeft(img, offsetX);
            //Canvas.SetLeft(ic, offsetX);
            
        }

        public void ClipTileFileListBox_Loaded(object sender, RoutedEventArgs e) {
            if(FileListVisibility == Visibility.Collapsed) {
                return;
            }
            var flb = (ListBox)sender;
            flb.ContextMenu = (ContextMenu)flb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");

        }

        public void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
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
            switch(MainWindowViewModel.ClipTrayViewModel.GetSelectedClipsType()) {
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

        public async Task<string> GetPastableRichTextTemplate(bool isPasting) {
            //if(TemplateTokenLookupDictionary != null && TemplateTokenLookupDictionary.Count > 0) {
            if(HasTemplate) { 
                IsEditingTile = true;
                IsPastingTemplateTile = true;
                IsTemplateReadyToPaste = false;
                foreach(var tthlvm in TemplateTokens) {
                    tthlvm.IsPasteMode = true;
                    tthlvm.IsEditMode = false;
                    tthlvm.WasEdited = false;
                    tthlvm.WasTypeViewed = false;
                    tthlvm.TemplateText = string.Empty;
                }
                await Task.Run(() => {
                    while (string.IsNullOrEmpty(TemplateRichText)) {
                        System.Threading.Thread.Sleep(500);
                    }

                    //TemplateRichText is set in PasteTemplateCommand
                });
                
                IsEditingTile = false;
                IsPastingTemplateTile = false;
                IsTemplateReadyToPaste = false;
                foreach (var tthlvm in TemplateTokens) {
                    tthlvm.IsPasteMode = false;
                    tthlvm.IsEditMode = false;
                    tthlvm.WasEdited = false;
                    tthlvm.WasTypeViewed = false;
                    tthlvm.TemplateText = string.Empty;
                }
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

        public void Convert(MpCopyItemType newType) {
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
        private void SetButtonGroupSelection(ToggleButton clickedButton, ToggleButton currentSelectedButton, IEnumerable<ToggleButton> buttonGroup, bool ignoreClickWhenSelected) {
            /* In some cases, if the user clicks the currently-selected button, we want to ignore
             * the click; for example, when a text alignment button is clicked. In other cases, we
             * want to deselect the button, but do nothing else; for example, when a list butteting
             * or numbering button is clicked. The ignoreClickWhenSelected variable controls that
             * behavior. */

            // Exit if currently-selected button is clicked
            if (clickedButton == currentSelectedButton) {
                if (ignoreClickWhenSelected) {
                    clickedButton.IsChecked = true;
                }
                return;
            }

            // Deselect all buttons
            foreach (var button in buttonGroup) {
                button.IsChecked = false;
            }

            // Select the clicked button
            clickedButton.IsChecked = true;
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
            if(!string.IsNullOrEmpty(translatedText)) {
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
            foreach(var thlvm in TemplateTokens) {
                if(!thlvm.WasTypeViewed) {
                    return false;
                }
            }
            return true;
        }
        private void PasteTemplate() {
            // TODO use code below to output templated rich text

            var tempRtb = new RichTextBox();
            tempRtb.Document = MpHelpers.ConvertRichTextToFlowDocument(CopyItemRichText);
            tempRtb.CreateHyperlinks();
            
            foreach (var templateLink in tempRtb.GetTemplateHyperlinkList()) {
                //TextRange tr = new TextRange(templateLink.ElementStart, templateLink.ElementEnd);
                ///tr.Text = string.Empty;
                Span span = new Span(templateLink.ElementStart, templateLink.ElementEnd);
                span.Inlines.Clear();
                span.Inlines.Add(new Run(TemplateTokenLookupDictionary[templateLink.TargetName]));
            }
            TemplateRichText = MpHelpers.ConvertFlowDocumentToRichText(tempRtb.Document);
            //Returned to GetPastableTemplate
        }

        private RelayCommand _clearAllTemplatesCommand;
        public ICommand ClearAllTemplatesCommand {
            get {
                if (_clearAllTemplatesCommand == null) {
                    _clearAllTemplatesCommand = new RelayCommand(ClearAllTemplates, CanClearAllTemplates);
                }
                return _clearAllTemplatesCommand;
            }
        }
        private bool CanClearAllTemplates() {
            return ClearAllTemplateToolbarButtonVisibility == Visibility.Visible;
        }
        private void ClearAllTemplates() {
            for (int i = 0; i < TemplateTokenLookupDictionary.Count; i++) {
                TemplateTokenLookupDictionary[TemplateTokenLookupDictionary.ElementAt(i).Key] = string.Empty;
            }

            foreach(var thlvm in TemplateTokens) {
                thlvm.TemplateText = string.Empty;
                thlvm.WasTypeViewed = false;
            }
            CurrentTemplateLookupIdx = 0;
        }

        private RelayCommand _clearCurrentTemplatesCommand;
        public ICommand ClearCurrentTemplatesCommand {
            get {
                if (_clearCurrentTemplatesCommand == null) {
                    _clearCurrentTemplatesCommand = new RelayCommand(ClearCurrentTemplates, CanClearCurrentTemplates);
                }
                return _clearCurrentTemplatesCommand;
            }
        }
        private bool CanClearCurrentTemplates() {
            return ClearCurrentTemplateTextboxButtonVisibility == Visibility.Visible;
        }
        private void ClearCurrentTemplates() {
            IsCurrentTemplateTextBoxFocused = true;
            CurrentTemplateText = string.Empty;
        }

        private RelayCommand _nextTemplateTokenCommand;
        public ICommand NextTemplateTokenCommand {
            get {
                if (_nextTemplateTokenCommand == null) {
                    _nextTemplateTokenCommand = new RelayCommand(NextTemplateToken, CanNextTemplateToken);
                }
                return _nextTemplateTokenCommand;
            }
        }
        private bool CanNextTemplateToken() {
            return TemplateTokenLookupDictionary.Count > 1;
        }
        private void NextTemplateToken() {
            CurrentTemplateLookupIdx = CurrentTemplateLookupIdx + 1 >= TemplateTokenLookupDictionary.Count ? 0 : CurrentTemplateLookupIdx + 1;
        }

        private RelayCommand _previousTemplateTokenCommand;
        public ICommand PreviousTemplateTokenCommand {
            get {
                if (_previousTemplateTokenCommand == null) {
                    _previousTemplateTokenCommand = new RelayCommand(PreviousTemplateToken, CanPreviousTemplateToken);
                }
                return _previousTemplateTokenCommand;
            }
        }
        private bool CanPreviousTemplateToken() {
            return TemplateTokenLookupDictionary.Count > 1;
        }
        private void PreviousTemplateToken() {
            CurrentTemplateLookupIdx = CurrentTemplateLookupIdx - 1 < 0 ? TemplateTokenLookupDictionary.Count - 1 : CurrentTemplateLookupIdx - 1;
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
            MpHelpers.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, CopyItemTitle,CopyItemPlainText));
            //MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            //IsSelected = true;
            //MpHelpers.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        }
                
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
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 && 
                  CopyItemType == MpCopyItemType.RichText && 
                  !IsEditingTile &&
                  !IsEditToolbarAnimating;
        }
        private void EditClipText() {
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
                switch(CopyItemType) {
                    case MpCopyItemType.RichText:
                        dp.SetText(MpHelpers.ConvertRichTextToPlainText(this.CopyItemRichText));
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
            //MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.IsShowingDialog = false;
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
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1;
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
