namespace MpWpfApp {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
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

        private MpFileListItemCollectionViewModel _fileListCollectionViewModel = null;
        public MpFileListItemCollectionViewModel FileListCollectionViewModel {
            get {
                if (CopyItem == null || CopyItemType != MpCopyItemType.FileList) {
                    return new MpFileListItemCollectionViewModel();
                }
                if (_fileListCollectionViewModel == null) {
                    _fileListCollectionViewModel = new MpFileListItemCollectionViewModel();
                    foreach (var path in CopyItem.GetFileList()) {
                        _fileListCollectionViewModel.Add(new MpFileListItemViewModel(this, path));
                    }
                }
                return _fileListCollectionViewModel;
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

        #region Appearance
        
        #endregion

        #region Layout

        public Rect TileRect {
            get {
                return MainWindowViewModel.ClipTrayViewModel.GetListBoxItemRect(MainWindowViewModel.ClipTrayViewModel.IndexOf(this));
                //if(ClipBorder == null || ClipBorder.Parent == null) {
                //    return new Rect(new Point(double.MinValue,double.MinValue),new Size());
                //}
                //return ClipBorder.TransformToAncestor((Visual)ClipBorder.Parent).TransformBounds(LayoutInformation.GetLayoutSlot(ClipBorder));

                //return ClipBorder.TransformToVisual(MainWindowViewModel.ClipTrayViewModel.ListBox).TransformBounds(LayoutInformation.GetLayoutSlot(ClipBorder));
                
                //if (ClipBorder == null ||
                //    MainWindowViewModel.ClipTrayViewModel.ListBox == null ||
                //    !ClipBorder.IsVisualDescendant(MainWindowViewModel.ClipTrayViewModel.ListBox)) {
                //    return new Rect(new Point(double.MinValue, double.MinValue), new Size(0, 0));
                //}
                //try {
                //    return ClipBorder.TransformToVisual(MainWindowViewModel.ClipTrayViewModel.ListBox).TransformBounds(LayoutInformation.GetLayoutSlot(ClipBorder));

                //}
                //catch (Exception ex) {
                //    Console.WriteLine("ItemRect error: " + ex);
                //    return new Rect(new Point(double.MinValue, double.MinValue), new Size(0, 0));
                //}

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

        private double _tileContentHeight = MpMeasurements.Instance.ClipTileContentHeight;
        public double TileContentHeight {
            get {
                return _tileContentHeight;
            }
            set {
                if(_tileContentHeight != value) {
                    _tileContentHeight = value;
                    OnPropertyChanged(nameof(TileContentHeight));
                }
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

        #region Visibility        
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
                return (MainWindowViewModel.ClipTrayViewModel.IsScrolling || IsSelected) ? Visibility.Collapsed : Visibility.Visible;
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

        public Visibility WebBrowserFileViewerVisibility { 
            get {
                if(CopyItemType != MpCopyItemType.FileList) {
                    return Visibility.Collapsed;
                }
                if(FileListCollectionViewModel.Count == 1 && 
                   System.IO.Path.GetExtension(FileListCollectionViewModel[0].ItemPath).ToLower() == @".pdf") {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility FileListStackPanelVisibility {
            get {
                return WebBrowserFileViewerVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
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

        #region Business Logic
        private IDataObject _dragDataObject = null;
        public IDataObject DragDataObject {
            get {
                return _dragDataObject;
            }
            set {
                if(_dragDataObject != value) {
                    _dragDataObject = value;
                    OnPropertyChanged(nameof(DragDataObject));
                }
            }
        }

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

        public Rect TileBorderBrushRect {
            get {
                if (RichTextBoxViewModelCollection == null || /*
                   RichTextBoxViewModelCollection.SubSelectedRtbvmList.Count == 0 ||*/
                   !IsContextMenuOpened) {
                    return new Rect(50,0,50,50);
                }
                if(IsContextMenuOpened && RichTextBoxViewModelCollection.Count > 1) {
                    return new Rect(0, 0, 50, 50);
                }
                return new Rect(50, 0, 50, 50);
            }
        }

        public Brush TileBorderBrush {
            get {
                if (MainWindowViewModel.ClipTrayViewModel.PrimarySelectedClipTile == this &&
                    MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count > 1) {
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
        public bool IsDroppingOntoNotepad { get; set; } = false;

        public Point MouseDownPosition = new Point();

        private bool _isClipDragging = false;
        public bool IsClipDragging {
            get {
                return _isClipDragging;
            }
            set {
                if (_isClipDragging != value) {
                    _isClipDragging = value;
                    OnPropertyChanged(nameof(IsClipDragging));
                }
            }
        }

        private bool _isDropping = false;
        public bool IsClipDropping {
            get {
                return _isDropping;
            }
            set {
                if (_isDropping != value) {
                    _isDropping = value;
                    OnPropertyChanged(nameof(IsClipDropping));
                }
            }
        }
        
        public bool IsClipOrAnySubItemDragging {
            get {
                return IsClipDragging || IsAnySubItemDragging;
            }
        }

        public bool IsAnySubItemDragging {
            get {
                return IsClipDragging || RichTextBoxViewModelCollection.Any(x => x.IsSubDragging);
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

        private bool _isContextMenuOpened = false;
        public bool IsContextMenuOpened {
            get {
                return _isContextMenuOpened;
            }
            set {
                if (_isContextMenuOpened != value) {
                    _isContextMenuOpened = value;
                    OnPropertyChanged(nameof(IsContextMenuOpened));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(TileBorderBrushRect));
                }
            }
        }

        //private bool _isPrimarySelected = false;
        //public bool IsPrimarySelected {
        //    get {
        //        return _isPrimarySelected;
        //    }
        //    set {
        //        if (_isPrimarySelected != value) {
        //            _isPrimarySelected = value;
        //            OnPropertyChanged(nameof(IsPrimarySelected));
        //            OnPropertyChanged(nameof(TileBorderBrush));
        //            OnPropertyChanged(nameof(TileBorderBrushRect));
        //        }
        //    }
        //}

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) 
                    {
                    // && (!MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded || IsExpanded || (!IsExpanded && value == false))) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    //MainWindowViewModel.ClipTrayViewModel.OnPropertyChanged(nameof(MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles));
                    OnPropertyChanged(nameof(ToolTipVisibility));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(DetailTextColor));
                    OnPropertyChanged(nameof(TileDetectedImageItemsVisibility));
                    OnPropertyChanged(nameof(ToggleEditModeButtonVisibility));
                    OnPropertyChanged(nameof(SelectionOverlayGridVisibility));
                    OnPropertyChanged(nameof(TileBorderBrushRect));
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
                    OnPropertyChanged(nameof(TileBorderBrushRect));
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
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public List<string> CopyItemFileDropList {
            get {
                if (CopyItem == null || MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null) {
                    return new List<string>();
                }
                if(CopyItemType == MpCopyItemType.FileList && FileListCollectionViewModel.Any(x=>x.IsSubSelected)) {
                    var subSelectedFileList = new List<string>();
                    foreach(var flvm in FileListCollectionViewModel) {
                        if(flvm.IsSubSelected) {
                            subSelectedFileList.Add(flvm.ItemPath);
                        }
                    }
                    return subSelectedFileList;
                }
                if (CopyItemType == MpCopyItemType.Composite && RichTextBoxViewModelCollection.Any(x => x.IsSubSelected)) {
                    var subSelectedCompositeItemList = new List<string>();
                    foreach (var rtbvm in RichTextBoxViewModelCollection) {
                        if (rtbvm.IsSubSelected) {
                            subSelectedCompositeItemList.Add(rtbvm.CopyItemFileDropList[0]);
                        }
                    }
                    return subSelectedCompositeItemList;
                }
                return CopyItem.GetFileList();// string.Empty, MainWindowViewModel.ClipTrayViewModel.GetTargetFileType());
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
                       HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.HighlightType == MpHighlightType.App) {
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
                    OnPropertyChanged(nameof(WebBrowserFileViewerVisibility));
                    OnPropertyChanged(nameof(FileListStackPanelVisibility));
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
                    OnPropertyChanged(nameof(FileListCollectionViewModel));
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
                            if(RichTextBoxViewModelCollection.Count > 0) {
                                RichTextBoxViewModelCollection.ClearSubSelection();
                            }
                            //LastSelectedDateTime = DateTime.Now;
                        } else {
                            //LastSelectedDateTime = DateTime.MaxValue;
                        }
                        
                        RefreshCommands();
                        break;
                    case nameof(IsHovering):                        
                        if(IsHovering) {
                            if (MainWindowViewModel.ClipTrayViewModel.IsScrolling) {
                                IsHovering = false;
                                foreach(var rtbvm in RichTextBoxViewModelCollection) {
                                    rtbvm.IsSubHovering = false;
                                }
                            }
                            foreach (var ctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
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
                            MainWindowViewModel.ExpandClipTile(this);
                        } else {
                            SaveToDatabase(); 
                            //ContentPreviewToolTipBmpSrc = null;
                            //OnPropertyChanged(nameof(ContentPreviewToolTipBmpSrc));
                            MainWindowViewModel.ShrinkClipTile(this);                            
                        }
                        RichTextBoxViewModelCollection.Refresh();
                        break;
                    case nameof(IsEditingTemplate):

                        break;
                    case nameof(IsPastingTemplate):
                        //RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(0, false, true);
                        break;
                    case nameof(IsClipDropping):
                        foreach (var rtbvm in RichTextBoxViewModelCollection) {
                            rtbvm.UpdateLayout();
                        }
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

            CopyItem = ci;

            TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);
            RichTextBoxViewModelCollection = new MpClipTileRichTextBoxViewModelCollection(this);
            EditRichTextBoxToolbarViewModel = new MpEditRichTextBoxToolbarViewModel(this);
            EditTemplateToolbarViewModel = new MpEditTemplateToolbarViewModel(this);
            PasteTemplateToolbarViewModel = new MpPasteTemplateToolbarViewModel(this);
            HighlightTextRangeViewModelCollection = new MpHighlightTextRangeViewModelCollection(this);

        }

        #region Loading Initializers
        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
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
                if (ctvm != null && !IsClipDragging) {
                    ctvm.IsHovering = false;
                }
            };
            ClipBorder.LostFocus += (s, e4) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (!ctvm.IsSelected) {
                    ctvm.IsEditingTitle = false;
                }
            };

            #region Drag/Drop
            
            int minDragDist = 5;
            //ClipBorder.PreviewGiveFeedback += (s, e3) => {
            //    var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
            //    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
            //        Application.Current.MainWindow.Cursor = Cursors.Cross; 
            //        Application.Current.MainWindow.ForceCursor = true;
            //    }
            //};
            //ClipBorder.PreviewQueryContinueDrag += (s, e3) => {

            //};
            ClipBorder.PreviewMouseUp += (s, e9) => {
                Application.Current.MainWindow.ForceCursor = false;
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                ctvm.MouseDownPosition = new Point();
                ctvm.IsClipDragging = false;
                ctvm.DragDataObject = null;
                if (e9.MouseDevice.DirectlyOver != null && e9.MouseDevice.DirectlyOver.GetType().IsSubclassOf(typeof(UIElement))) {
                    if (((UIElement)e9.MouseDevice.DirectlyOver).GetType() == typeof(Thumb)) {
                        //ensures scrollbar interaction isn't treated as drag and drop
                        var sb = (ScrollBar)((Thumb)e9.MouseDevice.DirectlyOver).TemplatedParent;
                        if (sb.Orientation == Orientation.Vertical) {
                            ctvm.RichTextBoxViewModelCollection.IsMouseOverVerticalScrollBar = false;
                        } else {
                            ctvm.RichTextBoxViewModelCollection.IsMouseOverHorizontalScrollBar = false;
                        }
                        return;
                    }
                }                
            };
            //ClipBorder.PreviewMouseDown += (s, e9) => {
            //    var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
            //    if (MpHelpers.Instance.IsMultiSelectKeyDown() && !ctvm.IsSelected) {
            //        ctvm.IsSelected = true;
            //        e9.Handled = true;
            //    }
            //};
            
            ClipBorder.MouseDown += (s, e7) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (e7.MouseDevice.DirectlyOver != null && e7.MouseDevice.DirectlyOver.GetType().IsSubclassOf(typeof(UIElement))) {
                    if (((UIElement)e7.MouseDevice.DirectlyOver).GetType() == typeof(Thumb)) {
                        //ensures scrollbar interaction isn't treated as drag and drop
                        var sb = (ScrollBar)((Thumb)e7.MouseDevice.DirectlyOver).TemplatedParent;
                        if (sb.Orientation == Orientation.Vertical) {
                            ctvm.RichTextBoxViewModelCollection.IsMouseOverVerticalScrollBar = true;
                        } else {
                            ctvm.RichTextBoxViewModelCollection.IsMouseOverHorizontalScrollBar = true;
                        }
                        return;
                    }
                }
                
            };
            ClipBorder.MouseMove += (s, e7) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (e7.MouseDevice.LeftButton == MouseButtonState.Pressed) {                    
                    if(ctvm.IsExpanded || ctvm.RichTextBoxViewModelCollection.IsMouseOverScrollBar) {
                        return;
                    }
                    if (ctvm.MouseDownPosition == new Point()) {
                        ctvm.MouseDownPosition = e7.GetPosition(ctvm.ClipBorder);
                    }
                    if (MpHelpers.Instance.DistanceBetweenPoints(ctvm.MouseDownPosition, e7.GetPosition(ctvm.ClipBorder)) < minDragDist) {
                        return;
                    }
                    ctvm.IsClipDragging = true;
                    ctvm.IsSelected = true;
                    ctvm.RichTextBoxViewModelCollection.SubSelectAll();
                    if(ctvm.DragDataObject == null) {
                        ctvm.DragDataObject = MainWindowViewModel.ClipTrayViewModel.GetDataObjectFromSelectedClips(true).Result;
                    }

                    DragDrop.DoDragDrop(
                               ((FrameworkElement)s),
                               ctvm.DragDataObject,
                               DragDropEffects.Copy | DragDropEffects.Move);         
                } 
            };            
            ClipBorder.DragLeave += (s2, e1) => {
                var ctvm = ((FrameworkElement)s2).DataContext as MpClipTileViewModel;
                ctvm.DragDataObject = null;
                ctvm.IsClipDropping = false;
                ctvm.RichTextBoxViewModelCollection.RtbLbAdornerLayer?.Update();
                ctvm.RichTextBoxViewModelCollection.ScrollViewer?.ScrollToHome();
            };
            ClipBorder.PreviewDragEnter += (s, e3) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                ctvm.IsClipDropping = false;
                e3.Effects = DragDropEffects.None;
                ctvm.RichTextBoxViewModelCollection.RtbLbAdornerLayer?.Update();

                MainWindowViewModel.ClipTrayViewModel.AutoScrollByMouse();

                if (ctvm.IsDragDataInvalid(e3.Data)) {
                    e3.Handled = true;
                    return;
                }
            };
            ClipBorder.PreviewDragOver += (s2, e1) => {
                MainWindowViewModel.ClipTrayViewModel.IsTrayDropping = false;
                MainWindowViewModel.ClipTrayViewModel.ClipTrayAdornerLayer.Update();

                var ctvm = ((FrameworkElement)s2).DataContext as MpClipTileViewModel;
                
                ctvm.IsClipDropping = false;
                e1.Effects = DragDropEffects.None;
                ctvm.RichTextBoxViewModelCollection.RtbLbAdornerLayer?.Update();

                MainWindowViewModel.ClipTrayViewModel.AutoScrollByMouse();

                if(ctvm.IsDragDataInvalid(e1.Data)) { 
                    e1.Handled = true;
                    return;
                }
                if (e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName) ||
                    e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileSubItemDragDropFormat)) {
                    int dropIdx = ctvm.GetDropIdx(MpHelpers.Instance.GetMousePosition(ctvm.RichTextBoxViewModelCollection.RichTextBoxListBox));
                    if (dropIdx >= 0 && dropIdx <= ctvm.RichTextBoxViewModelCollection.Count) {
                        if (dropIdx < ctvm.RichTextBoxViewModelCollection.Count) {
                            if(!ctvm.RichTextBoxViewModelCollection.IsListBoxItemVisible(dropIdx)) {
                                ctvm.RichTextBoxViewModelCollection.RichTextBoxListBox?.ScrollIntoView(ctvm.RichTextBoxViewModelCollection[dropIdx]);
                            } else if(dropIdx > 0 && dropIdx - 1 < ctvm.RichTextBoxViewModelCollection.Count) {
                                ctvm.RichTextBoxViewModelCollection.RichTextBoxListBox?.ScrollIntoView(ctvm.RichTextBoxViewModelCollection[dropIdx-1]);
                            }
                        } else {
                            //only can be count + 1
                            if (!ctvm.RichTextBoxViewModelCollection.IsListBoxItemVisible(dropIdx-1)) {
                                ctvm.RichTextBoxViewModelCollection.RichTextBoxListBox?.ScrollIntoView(ctvm.RichTextBoxViewModelCollection[dropIdx-1]);
                            }
                        }
                        ctvm.RichTextBoxViewModelCollection.DropLeftPoint = ctvm.RichTextBoxViewModelCollection.GetAdornerPoints(dropIdx)[0];
                        ctvm.RichTextBoxViewModelCollection.DropRightPoint = ctvm.RichTextBoxViewModelCollection.GetAdornerPoints(dropIdx)[1];
                        ctvm.IsClipDropping = true;
                        e1.Effects = DragDropEffects.Move;
                        e1.Handled = true;

                        MainWindowViewModel.ClipTrayViewModel.IsTrayDropping = false;
                        MainWindowViewModel.ClipTrayViewModel.ClipTrayAdornerLayer?.Update();
                    }                    
                }
                ctvm.RichTextBoxViewModelCollection.RtbLbAdornerLayer?.Update();
            };

            ClipBorder.PreviewDrop += async (s3, e2) => {
                var ctvm = ((FrameworkElement)s3).DataContext as MpClipTileViewModel;
                bool wasDropped = false;
                List<MpRtbListBoxItemRichTextBoxViewModel> drtbvml = null;
                List<MpClipTileViewModel> dctvml = null;
                int itemCount = 0;
                if (e2.Data.GetDataPresent(Properties.Settings.Default.ClipTileSubItemDragDropFormat)) {
                    drtbvml = (List<MpRtbListBoxItemRichTextBoxViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileSubItemDragDropFormat);
                    itemCount = drtbvml.Count;
                } else if (e2.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                    dctvml = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                    itemCount = dctvml.Count;
                }
                if (itemCount > 0) {
                    //int dropIdx = ctvm.RichTextBoxViewModelCollection.IndexUnderDragCursor;
                    int dropIdx = ctvm.GetDropIdx(MpHelpers.Instance.GetMousePosition(ctvm.RichTextBoxViewModelCollection.RichTextBoxListBox));
                    if (dropIdx >= 0) {
                        MainWindowViewModel.ClipTrayViewModel.ClearClipSelection(false);
                        if (!ctvm.IsSelected) {
                            ctvm.IsSelected = true;
                        }
                        ctvm.RichTextBoxViewModelCollection.ClearSubSelection();
                        var dcil = new List<MpCopyItem>();
                        for (int i = 0; i < itemCount; i++) {
                            var dci = drtbvml == null ? dctvml[i].CopyItem : drtbvml[i].CopyItem;
                            dcil.Add(dci);
                        }
                        await ctvm.MergeClipAsync(dcil, dropIdx);
                        wasDropped = true;
                    }
                }
                if (!wasDropped) {
                    e2.Effects = DragDropEffects.None;
                    e2.Handled = true;
                } else {
                    if (drtbvml != null) {
                        foreach (var rtbvm in drtbvml) {
                            rtbvm.IsSubDragging = false;
                            rtbvm.HostClipTileViewModel.IsClipDragging = false;
                        }
                    } else if (dctvml != null) {
                        foreach (var dctvm in dctvml) {
                            dctvm.IsClipDragging = false;
                            foreach (var rtbvm in dctvm.RichTextBoxViewModelCollection) {
                                rtbvm.IsSubDragging = false;
                            }
                        }
                    }
                }
                ctvm.IsClipDropping = false;
                ctvm.RichTextBoxViewModelCollection.RtbLbAdornerLayer?.Update();
            };
            #endregion
        }

        public void ClipTileDetailGrid_Loaded(object sender, RoutedEventArgs e) {
            var detailGrid = (Grid)sender;
            var titleDetailTextBlock = (TextBlock)detailGrid.FindName("ClipTileTitleDetailTextBlock");

            titleDetailTextBlock.MouseEnter += (s, e5) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                ctvm.OnPropertyChanged(nameof(ctvm.DetailText));
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
                if (MainWindowViewModel.ClipTrayViewModel.IsScrolling) {
                    return;
                }
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
                if (MainWindowViewModel.ClipTrayViewModel.IsScrolling) {
                    return;
                }
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
            var ctvm = ((FrameworkElement)sender).DataContext as MpClipTileViewModel;
            if (ctvm.ImgVisibility == Visibility.Collapsed) {
                return;
            }
            
            var ic = (FrameworkElement)sender;
            var ctcc = ic.GetVisualAncestor<Canvas>();
            var vb = (Viewbox)ic.FindName("ClipTileImageItemsControlViewBox");

            //vb.ContextMenu = ctcc.ContextMenu = ic.ContextMenu = (ContextMenu)((FrameworkElement)sender).GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");

            ctvm.DetectedImageObjectCollectionViewModel = new MpDetectedImageObjectCollectionViewModel(CopyItem);
        }

        public void ClipTileFileListBox_Loaded(object sender, RoutedEventArgs e) {
            var ctvm = ((FrameworkElement)sender).DataContext as MpClipTileViewModel;
            if (ctvm.FileListVisibility == Visibility.Collapsed) {
                return;
            }
            ctvm.OnPropertyChanged(nameof(ctvm.FileListCollectionViewModel));

            ctvm.FileListBox = (ListBox)sender;
            ctvm.FileListBox.ContextMenu = (ContextMenu)ctvm.FileListBox.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");            
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
            var cm = (ContextMenu)sender;
            var ctvm = cm.DataContext as MpClipTileViewModel;
            if (ctvm is MpRtbListBoxItemRichTextBoxViewModel) {
                ctvm = (ctvm as MpRtbListBoxItemRichTextBoxViewModel).HostClipTileViewModel;
            }
            ctvm.IsContextMenuOpened = false;

            ctvm.RichTextBoxViewModelCollection.ClearSubSelection();
        }

        public void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {            
            var cm = (ContextMenu)sender;
            var ctvm = cm.DataContext as MpClipTileViewModel;

            if(ctvm is MpRtbListBoxItemRichTextBoxViewModel) {
                //only allow context menu for ONE subitem
                (ctvm as MpRtbListBoxItemRichTextBoxViewModel).HostClipTileViewModel.IsContextMenuOpened = true;
                //(ctvm as MpRtbListBoxItemRichTextBoxViewModel).RichTextBoxViewModelCollection.ClearSubSelection();
                //(ctvm as MpRtbListBoxItemRichTextBoxViewModel).IsSubSelected = true;
            }

            if(ctvm.CopyItemType == MpCopyItemType.RichText || 
               ctvm.CopyItemType == MpCopyItemType.Composite) {
                cm = MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(cm);
            }
            if (ctvm.CopyItemType == MpCopyItemType.Composite) {
                //MpRtbListBoxItemRichTextBoxViewModel rtbvmUnderMouse = null;
                //for (int i = 0; i < ctvm.RichTextBoxViewModelCollection.Count; i++) {
                //    if (MpHelpers.Instance.IsMouseOver(ctvm.RichTextBoxViewModelCollection.GetListBoxItem(i))) {
                //        rtbvmUnderMouse = ctvm.RichTextBoxViewModelCollection[i];
                //        break;
                //    }
                //}
                //if (rtbvmUnderMouse != null) {
                //    //only allow context menu for ONE subitem
                //    ctvm.RichTextBoxViewModelCollection.ClearSubSelection();
                //    rtbvmUnderMouse.IsSubSelected = true;
                //    cm.DataContext = rtbvmUnderMouse;
                //}
            } else if (ctvm.CopyItemType == MpCopyItemType.Image && !string.IsNullOrEmpty(ctvm.CopyItemPlainText)) {
                var cmi = new MenuItem();
                cmi.Header = "Convert to Text";
                cmi.Click += (s, e1) => System.Windows.Clipboard.SetText(ctvm.CopyItemPlainText);
                cm.Items.Add(cmi);
            }

            RefreshCommands();

            ctvm.TagMenuItems.Clear();
            foreach (var tagTile in MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel) {
                if (tagTile.IsSudoTag) {
                    continue;
                }
                ctvm.TagMenuItems.Add(
                    new MpClipTileContextMenuItemViewModel(
                        tagTile.TagName,
                        MainWindowViewModel.ClipTrayViewModel.LinkTagToCopyItemCommand,
                        tagTile,
                        tagTile.IsLinkedWithClipTile(ctvm)));
            }
            ctvm.IsContextMenuOpened = true;
        }
        #endregion       

        public bool IsDragDataInvalid(IDataObject data) {
            if(CopyItemType == MpCopyItemType.Image || CopyItemType == MpCopyItemType.FileList) {
                return true;
            }
            if(data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                var dctvml = (List<MpClipTileViewModel>)data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                foreach(var dctvm in dctvml) {
                    if(dctvm == this || 
                       dctvm.CopyItemType == MpCopyItemType.Image || 
                       dctvm.CopyItemType == MpCopyItemType.FileList) {
                        return true;
                    }
                }
                return false;
            } else if(data.GetDataPresent(Properties.Settings.Default.ClipTileSubItemDragDropFormat)) {
                var drtbvml = (List<MpRtbListBoxItemRichTextBoxViewModel>)data.GetData(Properties.Settings.Default.ClipTileSubItemDragDropFormat);
                if(RichTextBoxViewModelCollection.Count == 1 && 
                   drtbvml.Count == 1 && 
                   RichTextBoxViewModelCollection[0] == drtbvml[0]) {
                    return true;
                }
                return false;
            }
            return false;
        }

        public void Resize(
            double deltaWidth,
            double deltaHeight,
            double deltaEditToolbarTop) {
            TileBorderWidth += deltaWidth;
            TileContentWidth += deltaWidth;

            TileBorderHeight += deltaHeight;
            TileContentHeight += deltaHeight;

            EditRichTextBoxToolbarViewModel.Resize(deltaEditToolbarTop, deltaWidth);

            RichTextBoxViewModelCollection.Resize(deltaEditToolbarTop, deltaWidth, deltaHeight);

            EditTemplateToolbarViewModel.Resize(deltaHeight);

            PasteTemplateToolbarViewModel.Resize(deltaHeight);
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
            MainWindowViewModel.ClipTrayViewModel.MergeSelectedClipsCommand.RaiseCanExecuteChanged();
        }


        public async Task MergeClipAsync(List<MpCopyItem> ocil, int forceIdx = -1, DispatcherPriority priority = DispatcherPriority.Background) {
            var sw = new Stopwatch();
            sw.Start();
            foreach (var oci in ocil) {
                //Application.Current.MainWindow.Cursor = Cursors.Wait;
                //IsBusy = true;
                
                if (oci.CopyItemId == CopyItemId) {
                    return;
                }
                var octvm = MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(oci.CopyItemId);
                if (octvm != null) {
                    //if copyitem is an existing tile remove it
                    await MainWindowViewModel.ClipTrayViewModel.RemoveAsync(octvm, true, priority);
                } else {
                    //otherwise check if it is a composite within a tile
                    MpRtbListBoxItemRichTextBoxViewModel ortbvm = null;
                    foreach (var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                        ortbvm = ctvm.RichTextBoxViewModelCollection.GetRtbItemByCopyItemId(oci.CopyItemId);
                        if (ortbvm != null) {
                            break;
                        }
                    }
                    if (ortbvm != null) {
                        if (ortbvm.HostClipTileViewModel == this && forceIdx >= 0) {
                            //occurs when rtbvmc items are resorted
                            forceIdx = Math.Min(this.RichTextBoxViewModelCollection.Count - 1, forceIdx);
                            this.RichTextBoxViewModelCollection.Move(this.RichTextBoxViewModelCollection.IndexOf(ortbvm), forceIdx);
                            this.RichTextBoxViewModelCollection.UpdateSortOrder();
                            //MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
                            //IsSelected = true;
                            //this.RichTextBoxViewModelCollection.ClearSubSelection();
                            //ortbvm.IsSubSelected = true;
                            return;
                        } else {
                            //if copyitem is part of composite remove it 
                            await ortbvm.RichTextBoxViewModelCollection.RemoveAsync(ortbvm, false, true, priority);
                        }
                    }
                }
                CopyItem = await MpCopyItem.MergeAsync(oci, CopyItem, false, false, forceIdx, priority);                
                //IsBusy = false;
            }
            MainWindowViewModel.TagTrayViewModel.RefreshAllCounts();

            RichTextBoxViewModelCollection.SyncItemsWithModel();

            MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            IsSelected = true;

            //MainWindowViewModel.ClipTrayViewModel.Refresh();

            SaveToDatabase();

            RichTextBoxViewModelCollection.Refresh();

            MainWindowViewModel.ClipTrayViewModel.Refresh();
            sw.Stop();
            Console.WriteLine("Merge: " + sw.ElapsedMilliseconds + "ms");
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
            var sw = new Stopwatch();
            sw.Start();
            if(RichTextBoxViewModelCollection.RichTextBoxListBox != null) {
                RichTextBoxViewModelCollection.Refresh();
            }
            if (FileListBox != null) {
                FileListBox.Items.Refresh();
            }
            sw.Stop();
            Console.WriteLine("ClipTile(VIdx:"+MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles.IndexOf(this)+") Refreshed (" + sw.ElapsedMilliseconds + "ms)");
        }

        public void SaveToDatabase() {
            var sw = new Stopwatch();
            sw.Start();
            //remove links to update model rich text
            RichTextBoxViewModelCollection.ClearAllHyperlinks();

            //clear any search highlighting when saving the document then restore after save
            HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(null);

            var rtsw = new Stopwatch();
            rtsw.Start();
            foreach(var rtbvm in RichTextBoxViewModelCollection) {
                if(rtbvm.Rtb == null) {
                    continue;
                }
                //property change will write the copyitem to the database
                rtbvm.CopyItemRichText = rtbvm.Rtb.Document.ToRichText();
            }
            rtsw.Stop();
            Console.WriteLine("Saving rich text from rtb's time: " + rtsw.ElapsedMilliseconds + "ms");

            CopyItemRichText = RichTextBoxViewModelCollection.FullDocument.ToRichText();
            HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(null);
            RichTextBoxViewModelCollection.CreateAllHyperlinks();
            //CopyItem.WriteToDatabase();

            var cipcsw = new Stopwatch();
            cipcsw.Start();
            //if(CopyItemType == MpCopyItemType.RichText) {
            //    CopyItemBmp = RichTextBoxViewModelCollection[0].CopyItemBmp;
            //}

            CopyItemBmp = CopyItem.GetSeparatedCompositeFlowDocument().ToBitmapSource();
            OnPropertyChanged(nameof(CopyItem));
            cipcsw.Stop();
            Console.WriteLine("Saving cliptile copyitem propertychanged time: " + cipcsw.ElapsedMilliseconds + "ms");

            sw.Stop();
            Console.WriteLine("Saving(VIdx:"+MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles.IndexOf(this)+"): " + sw.ElapsedMilliseconds + "ms");
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
                            MainWindowViewModel.ExpandClipTile(this);

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
                    FileListBox.ScrollIntoView(FileListCollectionViewModel[0]);
                    break;
            }
        }
        #endregion

        #region Private Methods              
        private int GetDropIdx(Point mp) {
            double mdy = mp.Y;
            double minDist = double.MaxValue;
            int dropIdx = -1;
            foreach (var rtbvm in RichTextBoxViewModelCollection) {
                double lbity = rtbvm.ItemRect.Top;//lbi.TranslatePoint(new Point(0.0, 0.0), RichTextBoxListBox).Y;
                double lbiby = rtbvm.ItemRect.Bottom;//lbi.TranslatePoint(new Point(rtbvm.Rtbc.ActualHeight, 0), RichTextBoxListBox).Y;
                double tDist = Math.Abs(mdy - lbity);
                double bDist = Math.Abs(mdy - lbiby);
                double dist = Math.Min(tDist, bDist);
                if (dist < minDist) {
                    minDist = dist;
                    if(minDist == tDist) {
                        dropIdx = RichTextBoxViewModelCollection.IndexOf(rtbvm);
                    } else {
                        dropIdx = RichTextBoxViewModelCollection.IndexOf(rtbvm) + 1;
                    }
                }
            }
            //var overRect = RichTextBoxViewModelCollection[dropIdx].ItemRect;
            //double overMidY = overRect.Top + (overRect.Height / 2);
            //if (mp.Y > overMidY) {
            //    dropIdx++;
            //}
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
            ClipBorder = null;
            TitleTextBox = null;
            TitleTextBlock = null;
            Image = null;
            FileListBox = null;
            ClipTileSelectionOverlayGrid = null;
            ClipBorder = null;
            ClipBorderTranslateTransform = null;
    }

        #endregion
    }
}
