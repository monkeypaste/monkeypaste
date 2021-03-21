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
    using AsyncAwaitBestPractices.MVVM;
    using GalaSoft.MvvmLight.CommandWpf;
    using GongSolutions.Wpf.DragDrop.Utilities;

    public class MpClipTileViewModel : MpUndoableViewModelBase<MpClipTileViewModel>, IDisposable {        
        #region Private Variables

        private int _detailIdx = 1;
        private List<string> _tempFileList = new List<string>();        

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
        private MpClipTileTitleSwirlViewModel _clipTileTitleSwirlViewModel = null;
        public MpClipTileTitleSwirlViewModel TitleSwirlViewModel {
            get {
                return _clipTileTitleSwirlViewModel;
            }
            set {
                if(_clipTileTitleSwirlViewModel != value) {
                    _clipTileTitleSwirlViewModel = value;
                    OnPropertyChanged(nameof(TitleSwirlViewModel));
                }
            }
        }

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
        public TextBox TitleTextBox;

        public TextBlock TitleTextBlock;

        public Image Image;

        public ListBox FileListBox;

        public Grid ClipTileSelectionOverlayGrid;

        public MpClipBorder ClipBorder;

        public TranslateTransform ClipBorderTranslateTransform;
        #endregion

        #region Layout Properties
        public Rect TileRect {
            get {
                return ClipBorder.TransformToVisual(MainWindowViewModel.ClipTrayViewModel.ClipTrayListView).TransformBounds(LayoutInformation.GetLayoutSlot(ClipBorder));
                if (ClipBorder == null ||
                    MainWindowViewModel.ClipTrayViewModel.ClipTrayListView == null ||
                    !ClipBorder.IsVisualDescendant(MainWindowViewModel.ClipTrayViewModel.ClipTrayListView)) {
                    return new Rect(new Point(double.MinValue, double.MinValue), new Size(0, 0));
                }
                try {
                    return ClipBorder.TransformToVisual(MainWindowViewModel.ClipTrayViewModel.ClipTrayListView).TransformBounds(LayoutInformation.GetLayoutSlot(ClipBorder));

                }
                catch (Exception ex) {
                    Console.WriteLine("ItemRect error: " + ex);
                    return new Rect(new Point(double.MinValue, double.MinValue), new Size(0, 0));
                }
                //var idx = MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles.IndexOf(this);
                //if (idx >= 0) {
                //    double w = TileBorderWidth + (TileMargin * 2 ) + (TileBorderThickness);
                //    double h = TileBorderHeight + (TileMargin) + (TileBorderThickness);
                //    return new Rect(idx * w, 0, (idx + 1) * w, h);
                //}
                //return new Rect();
            }
        }

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
                if (CopyItem == null) {
                    return MpMeasurements.Instance.ClipTileBorderMinMaxSize;
                }
                var ds = RichTextBoxViewModelCollection.FullDocument.GetDocumentSize();
                return Math.Max(MpMeasurements.Instance.ClipTileEditModeMinWidth, ds.Width);
            }
        }

        //public double TileBorderWidth {
        //    get {
        //        if(IsExpanded) {
        //            return TileBorderMaxWidth;
        //        }
        //        return TileBorderMinWidth;
        //    }
        //}

        private double _tileBorderWidth = MpMeasurements.Instance.ClipTileBorderMinSize;
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



        private double _tileBorderHeight = MpMeasurements.Instance.ClipTileMinSize;
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
                return MpMeasurements.Instance.ClipTileContentMinWidth;
            }
        }

        public double TileContentMaxWidth {
            get {
                return TileBorderMaxWidth - MpMeasurements.Instance.ClipTileContentMargin;
            }
        }


        //public double TileContentWidth {
        //    get {
        //        return IsExpanded ? TileContentMaxWidth : TileContentMinWidth;
        //    }
        //}

        private double _tileContentWidth = MpMeasurements.Instance.ClipTileContentMinWidth;
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

        public Visibility AppIconHighlightBorderVisibility {
            get {
                return HighlightTextRangeViewModelCollection.HasAppMatch ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public Visibility ToolTipVisibility {
            get {
                if (CopyItem == null) {
                    return Visibility.Collapsed;
                }
                return IsSelected ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility ToggleEditModeButtonVisibility {
            get {
                return ((IsHovering || IsExpanded) && 
                        MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1) ? 
                        Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility ClipTileTitleAppIconVisibility {
            get {
                return IsExpanded ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private Visibility _editToolbarVisibility = Visibility.Visible;
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

        private Visibility _editTemplateToolbarVisibility = Visibility.Visible;
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

        private Visibility _pasteTemplateToolbarVisibility = Visibility.Visible;
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
                if (IsEditingTile || IsEditingTemplate || IsPastingTemplate) {
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

        public Visibility SelectionOverlayGridVisibility {
            get {
                return (IsSelected && 
                       (MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 ||
                        MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded)) ? Visibility.Collapsed : Visibility.Visible;
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
        public bool IsLoading {
            get {
                return CopyItem == null || CopyItem.CopyItemId == 0;
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
        public bool IsDragging = false;

        public bool IsDropping = false;

        public bool IsAnyDragging {
            get {
                return IsDragging || RichTextBoxViewModelCollection.Any(x => x.IsDragging);
            }
        }

        public bool IsAnyOverDragButton {
            get {
                return RichTextBoxViewModelCollection.Any(x => x.IsOverDragButton);
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
                    OnPropertyChanged((nameof(CopyItemRichText)));
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(TileDetailGridVisibility));
                    OnPropertyChanged(nameof(TileContentHeight));
                    OnPropertyChanged(nameof(IsExpanded));
                    OnPropertyChanged(nameof(ClipTileTitleAppIconVisibility));
                    OnPropertyChanged(nameof(ToggleEditModeButtonVisibility));
                    OnPropertyChanged(nameof(SelectionOverlayGridVisibility));
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

        private bool _isPastingTemplate = false;
        public bool IsPastingTemplate {
            get {
                return _isPastingTemplate;
            }
            set {
                if (_isPastingTemplate != value) {
                    _isPastingTemplate = value;
                    OnPropertyChanged(nameof(IsPastingTemplate));
                    OnPropertyChanged(nameof(TileDetailGridVisibility));
                    OnPropertyChanged(nameof(TileContentHeight));
                    OnPropertyChanged(nameof(IsExpanded));
                    OnPropertyChanged(nameof(ClipTileTitleAppIconVisibility));
                    OnPropertyChanged(nameof(SelectionOverlayGridVisibility));
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
                if(ClipBorder == null) {
                    return false;
                }
                if(IsPastingTemplate || IsEditingTemplate || IsEditingTile) {
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
                if (_isSelected != value && (!MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded || IsExpanded || (!IsExpanded && value == false))) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    MainWindowViewModel.ClipTrayViewModel.OnPropertyChanged(nameof(MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles));
                    OnPropertyChanged(nameof(ToolTipVisibility));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(DetailTextColor));
                    OnPropertyChanged(nameof(TileDetectedImageItemsVisibility));
                    OnPropertyChanged(nameof(ToggleEditModeButtonVisibility));
                    OnPropertyChanged(nameof(SelectionOverlayGridVisibility));                    
                }                
            }
        }

        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value && (!MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded || IsExpanded)) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(DetailTextColor));                    
                    OnPropertyChanged(nameof(ToggleEditModeButtonVisibility));
                    OnPropertyChanged(nameof(SelectionOverlayGridVisibility));
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
                    _detailIdx = 1;
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
                    OnPropertyChanged(nameof(CopyItem));
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
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItem));
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
                    OnPropertyChanged(nameof(CopyItem));
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
                    //OnPropertyChanged(nameof(CopyItemRichText));
                    //OnPropertyChanged(nameof(CharCount));
                    //OnPropertyChanged(nameof(LineCount));
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
            set {
                if(CopyItem.ItemBitmapSource != value) {
                    CopyItem.ItemBitmapSource = value;
                    CopyItem.WriteToDatabase();
                    //OnPropertyChanged(nameof(CopyItemBmp));
                    OnPropertyChanged(nameof(CopyItem));
                }
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

        public BitmapSource CopyItemAppIcon {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                return CopyItem.App.IconImage;
            }
        }

        public BitmapSource CopyItemAppIconHighlightBorder {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                OnPropertyChanged(nameof(AppIconHighlightBorderVisibility));
                if (HighlightTextRangeViewModelCollection.HasAppMatch) {
                    if (HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel != null &&
                       HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.IsAppRange) {
                        return CopyItem.App.IconSelectedHighlightBorderImage;
                    }
                    return CopyItem.App.IconHighlightBorderImage;
                }
                return CopyItem.App.IconHighlightBorderImage;
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

        public string CopyItemAppPath {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.App.AppPath;
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
                    OnPropertyChanged(nameof(CopyItem));
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
                //if(_copyItem == null) {
                //    //only happens for non-composite richtext types
                //    if(RichTextBoxViewModelCollection != null &&
                //       RichTextBoxViewModelCollection.Count > 0) {
                //        //for non-composite clip tiles use rtblb's first (and only) element
                //        return RichTextBoxViewModelCollection[0].CopyItem;
                //    }
                //}
                return _copyItem;
            }
            set {
                //if (_copyItem != value) 
                {
                    //if(CopyItem != null && 
                    //   CopyItemType == MpCopyItemType.RichText) {
                    //    if(value != null && value.CopyItemType == MpCopyItemType.Composite) {
                    //        _copyItem = value;
                    //    } else {
                    //        RichTextBoxViewModelCollection[0].CopyItem = value;
                    //    }                            
                    //} else {
                    //    _copyItem = value;
                    //}
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
                    OnPropertyChanged(nameof(CopyItemAppPath));
                    OnPropertyChanged(nameof(CopyItemUsageScore));
                    OnPropertyChanged(nameof(CopyItemAppId));
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
                    CopyItem.WriteToDatabase();
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileViewModel() : base() { }

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
                            //IsEditingTile = false;
                            //IsEditingTemplate = false;
                            //foreach(var rtbvm in RichTextBoxViewModelCollection) {
                            //    rtbvm.IsEditingSubTitle = false;
                            //}
                            //IsPastingTemplateTile = false;
                        }
                        RefreshCommands();
                        break;
                    case nameof(IsHovering):
                        if(IsHovering) {
                            foreach(var ctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
                                if(ctvm != this) {
                                    ctvm.IsHovering = false;
                                }
                            }
                        }
                        break;
                    case nameof(IsEditingTile):
                        if (IsEditingTile) {
                            //if(!IsSelected) {
                            //    MainWindowViewModel.ClipTrayViewModel.ClearClipSelection(false);
                            //    IsSelected = true;
                            //    foreach(var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                            //        ctvm.IsPrimarySelected = false;
                            //    }
                            //}
                            //
                            MainWindowViewModel.ClipTrayViewModel.ExpandClipTile(this);
                        } else {
                            SaveToDatabase(); 
                            //ContentPreviewToolTipBmpSrc = null;
                            //OnPropertyChanged(nameof(ContentPreviewToolTipBmpSrc));
                            MainWindowViewModel.ClipTrayViewModel.ShrinkClipTile(this);                            
                        }

                        foreach (var rtbvm in RichTextBoxViewModelCollection) {
                            rtbvm.ResetRtb();
                        }
                        break;
                    case nameof(IsEditingTemplate):

                        break;
                    case nameof(IsPastingTemplate):
                        //RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(0, false, true);
                        break;
                }
            };

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
            //if (ci.CopyItemType == MpCopyItemType.RichText) {
            //    if (RichTextBoxViewModelCollection.Count == 0) {
            //        RichTextBoxViewModelCollection.Add(new MpRtbListBoxItemRichTextBoxViewModel(this, ci));
            //        OnPropertyChanged(nameof(CopyItem));
            //    }
            //} else {
            //    CopyItem = ci;
            //}
            CopyItem = ci;

            TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);
            RichTextBoxViewModelCollection = new MpClipTileRichTextBoxViewModelCollection(this);
            EditRichTextBoxToolbarViewModel = new MpEditRichTextBoxToolbarViewModel(this);
            EditTemplateToolbarViewModel = new MpEditTemplateToolbarViewModel(this);
            PasteTemplateToolbarViewModel = new MpPasteTemplateToolbarViewModel(this);
            HighlightTextRangeViewModelCollection = new MpHighlightTextRangeViewModelCollection(this);

            if (MainWindowViewModel != null) {
                //is null during loading and the refresh isn't needed
                //MainWindowViewModel.ClipTrayViewModel.Refresh();
            }
        }

        #region Loading Initializers
        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            //if (ClipBorder != null || ((MpClipBorder)sender).DataContext.GetType().ToString() == "MS.Internal.NamedObject") {
            //    return;
            //}
            ClipBorder = (MpClipBorder)sender;
            ClipBorderTranslateTransform = (TranslateTransform)ClipBorder.FindName("ClipTileBorderTranslateTransform");
            ClipTileSelectionOverlayGrid = (Grid)ClipBorder.FindName("ClipTileSelectionOverlayGrid");
            var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");
            
            ClipBorder.MouseEnter += (s, e1) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                ctvm.IsHovering = true;
            };
            ClipBorder.MouseLeave += (s, e2) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if(ctvm != null) {
                    ctvm.IsHovering = true;
                }                
            };
            ClipBorder.LostFocus += (s, e4) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (!ctvm.IsSelected) {
                    ctvm.IsEditingTitle = false;
                }
            };

            #region Drag/Drop
            var mouseDownPosition = new Point();
            int minDragDist = 20;
            ClipBorder.PreviewMouseUp += (s, e9) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                mouseDownPosition = new Point();
                ctvm.IsDragging = false;
            };
            ClipBorder.MouseMove += (s, e7) => {
                if (e7.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                    var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                    if (mouseDownPosition == new Point()) {
                        mouseDownPosition = e7.GetPosition(ClipBorder);
                    }
                    if (MpHelpers.Instance.DistanceBetweenPoints(mouseDownPosition, e7.GetPosition(ClipBorder)) < minDragDist) {
                        return;
                    }
                    ctvm.IsDragging = true;
                    ctvm.IsSelected = true;
                    DragDrop.DoDragDrop(
                                ClipBorder,
                                MainWindowViewModel.ClipTrayViewModel.GetDataObjectFromSelectedClips(true).Result,
                                DragDropEffects.Copy | DragDropEffects.Move);
                }
            };
            ClipBorder.DragLeave += (s2, e1) => {                
                //IsDropping = false;
                IsDropping = false;
                RichTextBoxViewModelCollection.RtbLbAdornerLayer.Update();
            };
            ClipBorder.PreviewDragOver += (s2, e1) => {
                //var ctvm = ((FrameworkElement)s2).DataContext as MpClipTileViewModel;
                IsDropping = false;
                e1.Effects = DragDropEffects.None;
                RichTextBoxViewModelCollection.RtbLbAdornerLayer.Update();
                if (IsDragging) {
                    return;
                }
                if (e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName) ||
                    e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileSubItemDragDropFormat)) {
                    var mp = e1.GetPosition(RichTextBoxViewModelCollection.RichTextBoxListBox);
                    int overIdx = GetDropIdx(mp);
                    if (overIdx >= 0) {
                        overIdx = overIdx >= RichTextBoxViewModelCollection.Count ? overIdx - 1 : overIdx;
                        var overRect = RichTextBoxViewModelCollection[overIdx].ItemRect;
                        double overMidY = overRect.Top + (overRect.Height / 2);
                        if (mp.Y > overMidY) {
                            RichTextBoxViewModelCollection.DropLeftPoint = overRect.BottomLeft;
                            RichTextBoxViewModelCollection.DropRightPoint = overRect.BottomRight;
                        } else {
                            RichTextBoxViewModelCollection.DropLeftPoint = overRect.TopLeft;
                            RichTextBoxViewModelCollection.DropRightPoint = overRect.TopRight;
                        }
                        IsDropping = true;
                        e1.Effects = DragDropEffects.Move;
                        e1.Handled = true;
                    }
                }
                RichTextBoxViewModelCollection.RtbLbAdornerLayer.Update();
            };

            ClipBorder.PreviewDrop += (s3, e2) => {
                bool wasDropped = false;
                List<MpRtbListBoxItemRichTextBoxViewModel> drtbvml = null;
                List<MpClipTileViewModel> dctvml = null;
                int itemCount = 0;
                Point mp = e2.GetPosition(RichTextBoxViewModelCollection.RichTextBoxListBox);
                if (e2.Data.GetDataPresent(Properties.Settings.Default.ClipTileSubItemDragDropFormat)) {
                    drtbvml = (List<MpRtbListBoxItemRichTextBoxViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileSubItemDragDropFormat);
                    itemCount = drtbvml.Count;
                } else if (e2.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                    dctvml = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                    itemCount = dctvml.Count;
                }
                if (itemCount > 0) {
                    int dropIdx = GetDropIdx(mp);
                    if (dropIdx >= 0) {
                        dropIdx = dropIdx >= RichTextBoxViewModelCollection.Count ? dropIdx - 1 : dropIdx;
                        MainWindowViewModel.ClipTrayViewModel.ClearClipSelection(false);
                        if(!IsSelected) {
                            IsSelected = true;
                        }
                        RichTextBoxViewModelCollection.ClearSubSelection();
                        for (int i = 0; i < itemCount; i++) {
                            var dci = drtbvml == null ? dctvml[i].CopyItem : drtbvml[i].CopyItem;
                            MergeClip(dci, dropIdx);
                        }
                    }
                }
                if (!wasDropped) {
                    e2.Effects = DragDropEffects.None;
                    e2.Handled = true;
                }
                IsDropping = false;
                RichTextBoxViewModelCollection.RtbLbAdornerLayer.Update();
            };
            #endregion
        }

        public void ClipTileDetailGrid_Loaded(object sender, RoutedEventArgs e) {
            var detailGrid = (Grid)sender;
            var titleDetailTextBlock = (TextBlock)detailGrid.FindName("ClipTileTitleDetailTextBlock");

            titleDetailTextBlock.MouseEnter += (s, e5) => {
                OnPropertyChanged(nameof(DetailText));
            };
        }        

        public void ClipTileTitle_Loaded(object sender, RoutedEventArgs e) {
            var lctvm = ((FrameworkElement)sender).DataContext as MpClipTileViewModel;

            var titleCanvas = (Canvas)sender;
            var titleTextGrid = (Grid)titleCanvas.FindName("ClipTileTitleTextGrid");
            var clipTileTitleTextBlock = (TextBlock)titleCanvas.FindName("ClipTileTitleTextBlock");
            var clipTileTitleTextBox = (TextBox)titleCanvas.FindName("ClipTileTitleTextBox");
            var titleIconImageButton = (Button)titleCanvas.FindName("ClipTileAppIconImageButton");
            var titleIconImageButtonRotateTransform = (RotateTransform)titleIconImageButton.FindName("ClipTileAppIconImageButtonRotateTransform");
            var titleIconBorderImage = (Image)titleCanvas.FindName("ClipTileAppIconBorderImage");
            var titleIconBorderImageScaleTransform = (ScaleTransform)titleCanvas.FindName("ClipTileAppIconBorderImageScaleTransform");
            var titleIconHighlightBorderImage = (Image)titleCanvas.FindName("ClipTileAppIconHighlightedBorderImage");

            RenderOptions.SetBitmapScalingMode(titleIconBorderImage, BitmapScalingMode.LowQuality);

            TitleTextBlock = clipTileTitleTextBlock;
            TitleTextBox = clipTileTitleTextBox;

            titleTextGrid.MouseEnter += (s, e1) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                Application.Current.MainWindow.Cursor = Cursors.IBeam;
                ctvm.IsHoveringOnTitleTextGrid = true;
            };
            titleTextGrid.MouseLeave += (s, e7) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                Application.Current.MainWindow.Cursor = Cursors.Arrow;
                ctvm.IsHoveringOnTitleTextGrid = false;
            };
            titleTextGrid.PreviewMouseLeftButtonDown += (s, e7) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                ctvm.IsEditingTitle = true;
                e7.Handled = true;
            };
            Canvas.SetLeft(titleTextGrid, MpMeasurements.Instance.ClipTileTitleTextGridCanvasLeft);
            Canvas.SetTop(titleTextGrid, MpMeasurements.Instance.ClipTileTitleTextGridCanvasTop);
            titleTextGrid.Width = MpMeasurements.Instance.ClipTileTitleTextGridWidth;

            clipTileTitleTextBox.IsVisibleChanged += (s, e9) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (ctvm.TileTitleTextBoxVisibility == Visibility.Collapsed) {
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                tbx.SelectAll();
            };
            clipTileTitleTextBox.LostFocus += (s, e4) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                ctvm.IsEditingTitle = false;
            };
            clipTileTitleTextBox.PreviewKeyDown += (s, e5) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (e5.Key == Key.Enter || e5.Key == Key.Escape) {
                    ctvm.IsEditingTitle = false;
                }
            };
            
            Canvas.SetLeft(titleIconImageButton, MpMeasurements.Instance.ClipTileTitleIconCanvasLeft);
            Canvas.SetTop(titleIconImageButton, 2);

            var diff = (TileTitleIconBorderSize - TileTitleIconSize) / 2;
            Canvas.SetLeft(titleIconHighlightBorderImage, MpMeasurements.Instance.ClipTileTitleIconCanvasLeft-diff);
            Canvas.SetTop(titleIconHighlightBorderImage, 2-diff);

            titleIconImageButton.MouseEnter += (s, e3) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (ctvm.IsEditingTemplate || ctvm.IsPastingTemplate) {
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
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (ctvm.IsEditingTemplate || ctvm.IsPastingTemplate) {
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
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
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

        public void ClipTileImageCanvas_Loaded(object sender, RoutedEventArgs e) {
            var lctvm = ((FrameworkElement)sender).DataContext as MpClipTileViewModel;
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
            var lctvm = ((FrameworkElement)sender).DataContext as MpClipTileViewModel;
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
            //((MpClipTileViewModel)((FrameworkElement)sender).DataContext).SaveToDatabase();
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
                if (tagTile.IsSudoTag) {
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

        public void Resize(
            double deltaWidth,
            double deltaEditToolbarTop,
            double deltaTemplateTop,
            double deltaContentHeight) {
            TileBorderWidth += deltaWidth;
            TileContentWidth += deltaWidth;

            EditRichTextBoxToolbarViewModel.Resize(deltaEditToolbarTop);

            RichTextBoxViewModelCollection.Resize(
                deltaEditToolbarTop,
                deltaContentHeight);

            //EditTemplateToolbarViewModel.Resize(
            //    deltaTemplateTop,
            //    tt,
            //    null,
            //    fps,
            //    priority);

            PasteTemplateToolbarViewModel.Resize(
                deltaTemplateTop);
        }
        public void Animate(
            double deltaWidth,
            double deltaEditToolbarTop,
            double deltaTemplateTop,
            double deltaContentHeight,
            double tt, 
            EventHandler onCompleted,
            double fps = 60,
            DispatcherPriority priority = DispatcherPriority.Normal) {

            double fromWidth = TileBorderWidth;//TileBorderMinWidth;
            double toWidth = fromWidth + deltaWidth;//TileBorderMaxWidth;            
            double dw = (deltaWidth / tt) / fps;

            var timer = new DispatcherTimer(priority);
            timer.Interval = TimeSpan.FromMilliseconds(fps);

            timer.Tick += (s, e32) => {
                if (MpHelpers.Instance.DistanceBetweenValues(TileBorderWidth, toWidth) > 0.5) {
                    TileBorderWidth += dw;
                    TileContentWidth += dw;
                    foreach (var rtbvm in RichTextBoxViewModelCollection) {
                        rtbvm.OnPropertyChanged(nameof(rtbvm.RtbCanvasWidth));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.RtbWidth));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.RtbPageWidth));
                    }
                } else {
                    timer.Stop();
                    if (onCompleted != null) {
                        onCompleted.BeginInvoke(this, new EventArgs(), null, null);
                        //Dispatcher.CurrentDispatcher.Invoke(() => onCompleted);
                    }                                     
                }         
            };
            EditRichTextBoxToolbarViewModel.Animate(
                        deltaEditToolbarTop,
                        tt,
                        null,
                        fps,
                        priority);

            RichTextBoxViewModelCollection.Animate(
                deltaEditToolbarTop,
                deltaContentHeight,
                tt,
                null,
                fps,
                priority);

            //EditTemplateToolbarViewModel.Animate(
            //    deltaTemplateTop,
            //    tt,
            //    null,
            //    fps,
            //    priority);

            PasteTemplateToolbarViewModel.Animate(
                deltaTemplateTop,
                tt,
                null,
                fps,
                priority);

            timer.Start();
        }

        public void RefreshCommands() {
            MainWindowViewModel.ClipTrayViewModel.BringSelectedClipTilesToFrontCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.SendSelectedClipTilesToBackCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.SpeakSelectedClipsAsyncCommand.RaiseCanExecuteChanged();
        }

        public void MergeClip(MpCopyItem oci, int forceIdx = -1) {
            var octvm = MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(oci.CopyItemId);
            if (octvm != null) {
                //if copyitem is an existing tile remove it
                MainWindowViewModel.ClipTrayViewModel.Remove(octvm,true);
            } else {
                //otherwise check if it is a composite within a tile
                MpRtbListBoxItemRichTextBoxViewModel ortbvm = null;
                foreach(var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                    ortbvm = ctvm.RichTextBoxViewModelCollection.GetRtbItemByCopyItemId(oci.CopyItemId);
                    if(ortbvm != null) {
                        break;
                    }
                }
                if(ortbvm != null) {
                    if(ortbvm.HostClipTileViewModel == this && forceIdx >= 0) {
                        //occurs when rtbvmc items are resorted
                        this.RichTextBoxViewModelCollection.Move(this.RichTextBoxViewModelCollection.IndexOf(ortbvm), forceIdx);
                        this.RichTextBoxViewModelCollection.UpdateSortOrder();
                        MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
                        IsSelected = true;
                        this.RichTextBoxViewModelCollection.ClearSubSelection();
                        ortbvm.IsSubSelected = true;
                        return;
                    } else {
                        //if copyitem is part of composite remove it 
                        ortbvm.RichTextBoxViewModelCollection.Remove(ortbvm,false,true);
                    }
                }
            }
            CopyItem = MpCopyItem.Merge(oci, CopyItem, false, true,false,false,forceIdx);

            MainWindowViewModel.TagTrayViewModel.RefreshAllCounts();

            RichTextBoxViewModelCollection.SyncItemsWithModel();

            MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            IsSelected = true;

            MainWindowViewModel.ClipTrayViewModel.Refresh();
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
            if(RichTextBoxViewModelCollection.RichTextBoxListBox != null) {
                RichTextBoxViewModelCollection.Refresh();
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
                if(rtbvm.Rtb == null) {
                    continue;
                }
                //property change will write the copyitem to the database
                rtbvm.CopyItemRichText = rtbvm.Rtb.Document.ToRichText();
            }

            CopyItemRichText = RichTextBoxViewModelCollection.FullDocument.ToRichText();

            HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(null);

            RichTextBoxViewModelCollection.CreateAllHyperlinks();

            CopyItem.WriteToDatabase();

            OnPropertyChanged(nameof(CopyItem));

            if(CopyItemType == MpCopyItemType.RichText) {
                CopyItemBmp = RichTextBoxViewModelCollection[0].CopyItemBmp;

                OnPropertyChanged(nameof(CopyItem));
            }
        }

        public async Task<string> GetPastableRichText() {
            if (HasTemplate) {
                bool hasExpanded = false;
                IsPastingTemplate = true;
                TemplateRichText = string.Empty.ToRichText();
                foreach(var rtbvm in RichTextBoxViewModelCollection) {
                    if(rtbvm.HasTemplate) {
                        if(!hasExpanded) {
                            //tile will be shrunk in on completed of hide window
                            MainWindowViewModel.ClipTrayViewModel.ExpandClipTile(this);

                            //RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(rtbvm, false, true);
                            hasExpanded = true;
                        } else {
                            //RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(rtbvm, false, false);
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
                    if(RichTextBoxViewModelCollection.RichTextBoxListBox == null) {
                        //no idea why this happens but the rtblb is null upon
                        //searchbox focus
                        break;
                    }
                    RichTextBoxViewModelCollection.RichTextBoxListBox.ScrollIntoView(RichTextBoxViewModelCollection[0]);
                    foreach(var rtbvm in RichTextBoxViewModelCollection) {
                        rtbvm.ResetRtb();
                    }
                    break;
                case MpCopyItemType.FileList:
                    FileListBox.ScrollIntoView(FileListViewModels[0]);
                    break;
            }
        }
        #endregion

        #region Private Methods              
        private int GetDropIdx(Point mp) {
            double mdy = mp.Y;
            double minDist = double.MaxValue;
            int dropIdx = 0;
            foreach (var rtbvm in RichTextBoxViewModelCollection) {
                double lbity = rtbvm.ItemRect.Top;//lbi.TranslatePoint(new Point(0.0, 0.0), RichTextBoxListBox).Y;
                double lbiby = rtbvm.ItemRect.Bottom;//lbi.TranslatePoint(new Point(rtbvm.Rtbc.ActualHeight, 0), RichTextBoxListBox).Y;
                double tDist = Math.Abs(mdy - lbity);
                double bDist = Math.Abs(mdy - lbiby);
                double dist = Math.Min(tDist, bDist);
                if (tDist < minDist) {
                    minDist = tDist;
                    dropIdx = RichTextBoxViewModelCollection.IndexOf(rtbvm);
                }
            }
            var overRect = RichTextBoxViewModelCollection[dropIdx].ItemRect;
            double overMidY = overRect.Top + (overRect.Height / 2);
            if (mp.Y > overMidY) {
                dropIdx++;
            }
            return dropIdx;
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
            if (MainWindowViewModel.ClipTrayViewModel.Contains(this)) {
                MainWindowViewModel.ClipTrayViewModel.Remove(this);
            }
            CopyItem.DeleteFromDatabase();
        }

        #endregion
    }
}
