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
    using GongSolutions.Wpf.DragDrop;
    using GongSolutions.Wpf.DragDrop.Utilities;

    public class MpClipTileViewModel : MpUndoableObservableCollectionViewModel<MpClipTileViewModel, MpClipTileViewModel>, IDisposable, ICloneable, IDropTarget {        
        #region Private Variables

        private int _detailIdx = 0;

        private List<string> _tempFileList = new List<string>();

        private string _origClipRichText = string.Empty;


        private bool _wasEditConfirmed = true;

        #endregion

        #region Properties

        public object ParentObject { get; set; }

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
        public MpObservableCollection<MpClipTileViewModel> ParentCollection {
            get {
                if(IsSubClipTile) {
                    return ParentOrSelfClipTileViewModel;
                }
                return MainWindowViewModel.ClipTrayViewModel;
            }
        }

        public MpClipTileViewModel ParentOrSelfClipTileViewModel {
            get {
                if(ParentObject == null || ParentObject is MpClipTrayListBoxViewModel) {
                    return this;
                }
                return (MpClipTileViewModel)ParentObject;
            }
        }

        public MpClipTileViewModel SubSelectedClipTileViewModel {
            get {
                var ssctvml = this.Where(x => x.IsSelected).ToList();
                if(ssctvml == null || ssctvml.Count == 0) {
                    if(this.Count > 0) {
                        this[0].IsSelected = true;
                        return this[0];
                    }
                    return null;
                }
                return ssctvml[0];
            }
        }

        public MpClipTileViewModel Next {
            get {
                if (ParentOrSelfClipTileViewModel == null ||
                   ParentOrSelfClipTileViewModel.Count == 0) {
                    return null;
                }
                int nextIdx = CompositeSortOrderIdx + 1;
                if (nextIdx >= ParentOrSelfClipTileViewModel.Count) {
                    return null;
                }
                return ParentOrSelfClipTileViewModel[nextIdx];
            }
        }

        public MpClipTileViewModel Previous {
            get {
                if (ParentOrSelfClipTileViewModel == null ||
                   ParentOrSelfClipTileViewModel.Count == 0) {
                    return null;
                }
                int prevIdx = CompositeSortOrderIdx - 1;
                if (prevIdx < 0) {
                    return null;
                }
                return ParentOrSelfClipTileViewModel[prevIdx];
            }
        }

        private MpClipTileOverlayViewModel _clipTileOverlayViewModel;
        public MpClipTileOverlayViewModel ClipTileOverlayViewModel {
            get {
                return _clipTileOverlayViewModel;
            }
            set {
                if (_clipTileOverlayViewModel != value) {
                    _clipTileOverlayViewModel = value;
                    OnPropertyChanged(nameof(ClipTileOverlayViewModel));
                }
            }
        }

        private MpTemplateHyperlinkCollectionViewModel _templateHyperlinkCollectionViewModel = new MpTemplateHyperlinkCollectionViewModel();
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

        private MpObservableCollection<MpClipTileContextMenuItemViewModel> _convertClipTypes = new MpObservableCollection<MpClipTileContextMenuItemViewModel>();
        public MpObservableCollection<MpClipTileContextMenuItemViewModel> ConvertClipTypes {
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

        private MpObservableCollection<MpClipTileContextMenuItemViewModel> _tagMenuItems = new MpObservableCollection<MpClipTileContextMenuItemViewModel>();
        public MpObservableCollection<MpClipTileContextMenuItemViewModel> TagMenuItems {
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

        private MpObservableCollection<MpFileListItemViewModel> _fileListViewModels = null;
        public MpObservableCollection<MpFileListItemViewModel> FileListViewModels {
            get {
                if (CopyItem == null || CopyItemType != MpCopyItemType.FileList) {
                    return new MpObservableCollection<MpFileListItemViewModel>();
                }
                if (_fileListViewModels == null) {
                    _fileListViewModels = new MpObservableCollection<MpFileListItemViewModel>();
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
        public AdornerLayer RtbcAdornerLayer { get; set; }

        public TextBox TitleTextBox { get; set; }

        public TextBlock TitleTextBlock { get; set; }

        private RichTextBox _rtb;
        public RichTextBox Rtb { 
            get {
                if(IsCompositeParent) {
                    return SubSelectedClipTileViewModel.Rtb;
                }
                return _rtb;
            }
            private set {
                if(_rtb != value) {
                    _rtb = value;
                    OnPropertyChanged(nameof(Rtb));
                }
            }
        }

        public Image Image { get; set; }

        public ListBox RichTextBoxListBox { get; set; }

        public ListBox FileListBox { get; set; }

        public MpClipBorder ClipBorder { get; set; }
        #endregion

        #region Layout Properties
        public double DragButtonSize {
            get {
                return MpMeasurements.Instance.ClipTileSubItemMinHeight;
            }
        }

        public Thickness RtbMargin {
            get {
                if (ParentOrSelfClipTileViewModel == null) {
                    return new Thickness(0);
                }
                if (IsHovering && IsSubClipTile) {
                    return new Thickness(DragButtonSize + 5, TitleFontSize + 5, 0, 0);
                }
                return new Thickness(0);
            }
        }

        public double RelativeContentWidthMax {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                if(IsCompositeParent) {
                    double maxWidth = 0;
                    foreach (var ctvm in this) {
                        maxWidth = Math.Max(maxWidth, ctvm.RelativeContentWidthMax);
                    }
                    return maxWidth;
                }
                return CopyItem.ItemFlowDocument.GetDocumentSize().Width;
            }
        }
        public double ClipTrayListBoxItemHeight {
            get {
                double ch = MpMeasurements.Instance.ClipTileContentHeight;
                if (IsEditingTile) {
                    ch -= MpMeasurements.Instance.ClipTileEditToolbarHeight;
                }
                if (IsPastingTemplateTile) {
                    ch -= MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                }
                if (IsEditingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTileEditTemplateToolbarHeight;
                }
                return ch;
            }
        }

        public double RelativeContentHeight {
            get {
                if (!IsCompositeParent && !IsExpanded) {
                    //if tile just has one item and is not expanded
                    return ParentOrSelfClipTileViewModel.ClipTrayListBoxItemHeight;
                }
                if (ParentOrSelfClipTileViewModel.IsExpanded) {
                    if (IsCompositeParent) {
                        return Math.Max(CopyItem.ItemFlowDocument.GetDocumentSize().Height, ParentOrSelfClipTileViewModel.ClipTrayListBoxItemHeight);
                    }
                    return CopyItem.ItemFlowDocument.GetDocumentSize().Height;
                }
                if(IsCompositeParent) {
                    double totalHeight = 0;
                    foreach (var ctvm in this) {
                        totalHeight += ctvm.RelativeContentHeight;
                    }
                    return totalHeight;
                }
                var doc = CopyItem.ItemFlowDocument;
                doc.PageWidth = ParentOrSelfClipTileViewModel.TileContentWidth;
                double itemHeight = Math.Max(MpMeasurements.Instance.ClipTileSubItemMinHeight, doc.GetDocumentSize().Height);
                return itemHeight + RtbMargin.Top + RtbMargin.Bottom;
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
                    OnPropertyChanged(nameof(TileSubItemOverlayBorderWidth));
                }
            }
        }

        public double TileSubItemOverlayBorderWidth {
            get {
                return TileContentWidth - MpMeasurements.Instance.ClipTileSubItemOverlayMargin;
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
                return Math.Max(MpMeasurements.Instance.ClipTileEditModeMinWidth, RelativeContentWidthMax);
            }
        }

        public double TitleFontSize {
            get {
                if(IsSubClipTile) {
                    return 14;
                }
                return 20;
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
                if(IsExpanded) {
                    if(RelativeContentWidthMax > ClipBorder.ActualWidth) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility RtbVerticalScrollbarVisibility {
            get {
                if (IsExpanded) {
                    if (RelativeContentHeight > RichTextBoxListBox.ActualHeight - EditRichTextBoxToolbarHeight) {
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

        public double SubItemOverlayOpacity {
            get {
                if(IsSubClipTile && IsHovering) {
                    return 1;
                }
                return 0;
            }
        }
        #endregion

        #region Business Logic Properties
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
                    if(IsSubClipTile) {
                        return Brushes.Black;
                    }
                    return Brushes.Yellow;
                }
                return Brushes.Orange;
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
        public bool IsSubClipTile {
            get {
                if(CopyItem == null) {
                    return false;
                }
                return CopyItem.IsSubCompositeItem;
            }
        }

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
                    if(IsSubClipTile) {
                        OnPropertyChanged(nameof(DetailText));
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
                    OnPropertyChanged(nameof(ClipTrayListBoxItemHeight));
                    OnPropertyChanged(nameof(ClipTrayListBoxItemHeight));
                    OnPropertyChanged(nameof(IsExpanded));
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
                    OnPropertyChanged(nameof(ClipTrayListBoxItemHeight));
                    OnPropertyChanged(nameof(ClipTrayListBoxItemHeight));
                    OnPropertyChanged(nameof(IsExpanded));
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
                    OnPropertyChanged(nameof(ClipTrayListBoxItemHeight));
                    OnPropertyChanged(nameof(ClipTrayListBoxItemHeight));
                    OnPropertyChanged(nameof(IsExpanded));
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
                if(IsCompositeParent) {
                    foreach (var ctvm in this) {
                        if (ctvm.HasTemplate) {
                            return true;
                        }
                    }
                }
                
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

        private bool _isSubSelected = false;
        public bool IsSubSelected {
            get {
                return _isSubSelected;
            }
            set {
                if (_isSubSelected != value) {
                    _isSubSelected = value;
                    OnPropertyChanged(nameof(IsSubSelected));
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
                    OnPropertyChanged(nameof(SubItemOverlayOpacity));
                }
            }
        }

        private bool _isSubHovering = false;
        public bool IsSubHovering {
            get {
                return _isSubHovering;
            }
            set {
                if (_isSubHovering != value) {
                    _isSubHovering = value;
                    IsHovering = _isSubHovering;
                    OnPropertyChanged(nameof(IsSubHovering));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(DetailTextColor));
                    OnPropertyChanged(nameof(MenuOverlayVisibility));
                    OnPropertyChanged(nameof(SubItemOverlayOpacity));
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
        public MpEventEnabledFlowDocument Document {
            get {
                if(CopyItem == null) {
                    return new MpEventEnabledFlowDocument();
                }
                if(IsSubClipTile || CopyItemType != MpCopyItemType.Composite) {
                    return CopyItem.ItemFlowDocument;
                }
                var fullDocument = MpHelpers.Instance.ConvertRichTextToFlowDocument(MpHelpers.Instance.ConvertPlainTextToRichText(string.Empty));

                foreach (var ctvm in this) {
                    MpHelpers.Instance.CombineFlowDocuments((MpEventEnabledFlowDocument)ctvm.Rtb.Document, fullDocument, true);
                }
                return fullDocument;
            }
        }

        public bool IsCompositeParent {
            get {
                if (CopyItem == null) {
                    return false;
                }
                return CopyItem.IsCompositeParent;
            }
        }

        private string _copyItemFilePath = string.Empty;
        public string CopyItemFilePath {
            get {
                if (CopyItem == null || MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(_copyItemFilePath)) {
                    _copyItemFilePath = CopyItem.GetFileList()[0];
                }
                return _copyItemFilePath;
            }
            set {
                if (_copyItemFilePath != value) {
                    _copyItemFilePath = value;
                    OnPropertyChanged(nameof(CopyItemFilePath));
                }
            }
        }

        public int CompositeParentCopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeParentCopyItemId;
            }
            set {
                if (CopyItem != null && CopyItem.CompositeParentCopyItemId != value) {
                    CopyItem.CompositeParentCopyItemId = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    //HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItem));
                }
            }
        }

        public int CompositeSortOrderIdx {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeSortOrderIdx;
            }
            set {
                if (CopyItem != null && CopyItem.CompositeSortOrderIdx != value) {
                    CopyItem.CompositeSortOrderIdx = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    if(IsSubClipTile) {
                        ParentOrSelfClipTileViewModel.OnPropertyChanged(nameof(ParentOrSelfClipTileViewModel.CopyItem));
                    }
                }
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
                    return Brushes.Red;
                }
                if (IsSubClipTile) {
                    var scb = new SolidColorBrush(CopyItem.ItemColor.Color);
                    scb.Opacity = 0.25;
                    return scb;
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
                if(!IsSubClipTile) {
                    return Brushes.White;
                }
                if (IsHoveringOnTitleTextGrid) {
                    return Brushes.DarkGray;
                }
                return Brushes.Black;
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
                    OnPropertyChanged(nameof(IsSubClipTile));
                    OnPropertyChanged(nameof(SubItemOverlayOpacity));
                    OnPropertyChanged(nameof(TitleFontSize));
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    OnPropertyChanged(nameof(Next));
                    OnPropertyChanged(nameof(Previous));
                    OnPropertyChanged(nameof(IsCompositeParent));
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
                        if(ParentOrSelfClipTileViewModel != null) {
                            //ParentClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(HostClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel));
                            //ParentClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(HostClipTileViewModel.RichTextBoxViewModelCollection.SelectedRtb));

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
                        if (IsSubClipTile) {
                            ParentOrSelfClipTileViewModel.IsHovering = IsHovering;
                        }
                        OnPropertyChanged(nameof(RtbMargin));
                        OnPropertyChanged(nameof(RelativeContentHeight));
                        UpdateLayout();
                        break;
                }
            };
            EditRichTextBoxToolbarViewModel = new MpEditRichTextBoxToolbarViewModel(this);
            EditTemplateToolbarViewModel = new MpEditTemplateToolbarViewModel(this);
            PasteTemplateToolbarViewModel = new MpPasteTemplateToolbarViewModel(this);
            HighlightTextRangeViewModelCollection = new MpHighlightTextRangeViewModelCollection(this);
            TemplateHyperlinkCollectionViewModel = new MpTemplateHyperlinkCollectionViewModel(this);
            ClipTileOverlayViewModel = new MpClipTileOverlayViewModel(this);

            this.CollectionChanged += (s, e) => {
                if(CopyItemType == MpCopyItemType.Composite) {
                    foreach (var ctvm in this) {
                        ctvm.CompositeParentCopyItemId = CopyItemId;
                        ctvm.CompositeSortOrderIdx = this.IndexOf(ctvm);
                        ctvm.CopyItem.WriteToDatabase();
                    }
                }
                //if(this.Count > 0) {
                //    SelectRichTextBoxViewModel(0);
                //}
            };
            if (isPlaceHolder) {
                SetCopyItem(null);
            }
        }

        public MpClipTileViewModel(MpCopyItem ci, object parentViewModel) : this(false) {
            ParentObject = parentViewModel;
            SetCopyItem(ci);
        }

        #region Loaded Event Handlers
        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            ClipBorder = (MpClipBorder)sender;
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
            ClipBorder.PreviewMouseLeftButtonDown += (s, e5) => {
                if (e5.ClickCount == 2 && !IsEditingTile) {
                    //only for richtext type
                    EditClipCommand.Execute(null);
                    e5.Handled = true;
                    return;
                }
            };           

            HighlightTextRangeViewModelCollection.Init();
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

        public void SubClipTileTitle_Loaded(object sender, RoutedEventArgs args) {
            if(!IsSubClipTile) {
                return;
            }
            ClipBorder = (MpClipBorder)sender;
            var titleTextGrid = (Grid)ClipBorder.FindName("ClipTileRichTextBoxListItemOverlayBorderGrid");
            TitleTextBlock = (TextBlock)titleTextGrid.FindName("ClipTileTitleTextBlock");
            TitleTextBox = (TextBox)titleTextGrid.FindName("ClipTileTitleTextBox");

            ClipBorder.MouseEnter += (s, e1) => {
                IsSubHovering = true;
            };
            ClipBorder.MouseLeave += (s, e2) => {
                IsSubHovering = false;
            };

            TitleTextBlock.MouseEnter += (s, e1) => {
                if (++_detailIdx > 2) {
                    _detailIdx = 0;
                }
                //must be set after detailIdx incremented to update
                //detailtext 
                IsHoveringOnTitleTextGrid = true;
            };
            
            TitleTextBlock.MouseLeave += (s, e7) => {
                IsHoveringOnTitleTextGrid = false;
            };
            TitleTextBlock.PreviewMouseLeftButtonDown += (s, e7) => {
                IsEditingTitle = true;
                e7.Handled = true;
            };

            TitleTextBox.IsVisibleChanged += (s, e9) => {
                if (TileTitleTextBoxVisibility == Visibility.Collapsed) {
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                tbx.SelectAll();
            };
            TitleTextBox.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };
            TitleTextBox.PreviewKeyDown += (s, e5) => {
                if (e5.Key == Key.Enter || e5.Key == Key.Escape) {
                    IsEditingTitle = false;
                }
            };

            OnPropertyChanged(nameof(SubItemOverlayOpacity));
            OnPropertyChanged(nameof(TileSubItemOverlayBorderWidth));
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

        public void ClipTileRichTextBoxListBoxCanvas_Loaded(object sender, RoutedEventArgs e) {
            if (RtbVisibility == Visibility.Collapsed) {
                return;
            }
            var rtbc = (Canvas)sender;
            var rtblb = (ListBox)rtbc.FindName("ClipTileRichTextBoxListBox");

            RichTextBoxListBox = rtblb;

            //after pasting template rtb's are duplicated so clear them upon refresh
            if (this.Count == 0) {
                if (CopyItemType == MpCopyItemType.RichText) {
                    this.Add(this);
                } else {
                    foreach (var cci in CopyItem.CompositeItemList) {
                        this.Add(new MpClipTileViewModel(cci,this));
                    }
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
                        foreach(var ctvm in this) {
                            var rtb = ctvm.Rtb;
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

        public void ClipTileRichTextBoxListItemCanvas_Loaded(object sender, RoutedEventArgs e) {
            var rtbc = (Canvas)sender;
            var ctvm = rtbc.DataContext as MpClipTileViewModel;
            ctvm.Rtb = (RichTextBox)rtbc.FindName("ClipTileRichTextBox");
            ctvm.Rtb.Document = MpHelpers.Instance.ConvertRichTextToFlowDocument(CopyItemRichText);

            ctvm.TemplateHyperlinkCollectionViewModel.CreateHyperlinks();

            ctvm.UpdateLayout();

            #region Drag & Drop
            #endregion

            ctvm.Rtb.GotFocus += (s, e2) => {
                SetSubSelection(true);
            };

            if (ctvm.WasAddedAtRuntime) {
                //force new items to have left alignment
                ctvm.Rtb.SelectAll();
                ctvm.Rtb.Selection.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                ctvm.Rtb.CaretPosition = ctvm.Rtb.Document.ContentStart;
            }

            if (ctvm.CompositeSortOrderIdx <= 0) {
                ctvm.SetSubSelection(true);
            }

            ctvm.RtbcAdornerLayer = AdornerLayer.GetAdornerLayer(rtbc);
            ctvm.RtbcAdornerLayer.Add(new MpRichTextBoxOverlayAdorner(rtbc));

            IsHovering = true;
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

        #region Context Menu Event Handlers
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
                        foreach (var sctvm in MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles) {
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
        #endregion

        public void AnimateItems(double fromWidth, double toWidth, double fromHeight, double toHeight, double fromTop, double toTop, double fromBottom, double toBottom) {
            if (toWidth > 0) {
                foreach (var ctvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            fromWidth,
                            toWidth,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            new List<FrameworkElement> { ctvm.Rtb },
                            FrameworkElement.WidthProperty,
                            (s1, e44) => {
                                ctvm.UpdateLayout();
                            });
                }
            }
            if (toHeight > 0) {

            }
            if (toTop > 0) {
                foreach (var ctvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            fromTop,
                            toTop,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            new List<FrameworkElement> { ctvm.Rtb },
                            Canvas.TopProperty,
                            (s1, e44) => {

                            });
                    fromTop += ctvm.RelativeContentHeight;
                    toTop += ctvm.RelativeContentHeight;
                }
            }
            if (toBottom > 0) {

            }
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
            VirtualizationManager.Instance.RunOnUi(() => {
                CopyItem = ci;
                if (MainWindowViewModel != null) {
                    //is null during loading and the refresh isn't needed
                    MainWindowViewModel.ClipTrayViewModel.Refresh();
                }
            });
        }

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
            this.Clear();

            //when initially converting richtext item into composite it needs to be relinked to history
            MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(this,false);
            MainWindowViewModel.ClipTrayViewModel.Refresh();
            Refresh();
        }

        


        public void ClearSubSelection() {
            foreach (var ctvm in this) {
                ctvm.SetSubSelection(false);
            }
        }

        public void ResetSubSelection() {
            ClearSubSelection();
            if (this.Count > 0) {
                this[0].SetSubSelection(true);
            }
        }

        public void SetSubSelection(bool newSelection) {
            if (IsSelected == newSelection) {
                return;
            }
            IsSelected = newSelection;
            if (IsSelected) {
                ParentOrSelfClipTileViewModel.EditRichTextBoxToolbarViewModel.InitWithRichTextBox(Rtb);
                ParentOrSelfClipTileViewModel.EditTemplateToolbarViewModel.InitWithRichTextBox(Rtb);
                ParentOrSelfClipTileViewModel.PasteTemplateToolbarViewModel.InitWithRichTextBox(Rtb);
            }
        }

        public void ClearAllHyperlinks() {
            TemplateHyperlinkCollectionViewModel.ClearHyperlinks();
            if(IsCompositeParent) {
                foreach (var ctvm in this) {
                    ctvm.TemplateHyperlinkCollectionViewModel.ClearHyperlinks();
                }
            }            
        }

        public void CreateAllHyperlinks() {
            TemplateHyperlinkCollectionViewModel.CreateHyperlinks();
            if(IsCompositeParent) {
                foreach (var ctvm in this) {
                    ctvm.TemplateHyperlinkCollectionViewModel.CreateHyperlinks();
                }
            }            
        }

        public void UpdateLayout() {
            Rtb.Document.PageWidth = Rtb.Width - Rtb.Padding.Left - Rtb.Padding.Right - Rtb.Margin.Left - Rtb.Margin.Right;
            Rtb.Document.PageHeight = Rtb.Height - Rtb.Padding.Top - Rtb.Padding.Bottom - Rtb.Margin.Top - Rtb.Margin.Bottom;
            if (ParentOrSelfClipTileViewModel.IsEditingTile) {
                Rtb.Document.PageWidth -= (MpMeasurements.Instance.ClipTileEditModeContentMargin * 2) + 5;
            }
            OnPropertyChanged(nameof(RelativeContentHeight));
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
            this.ClearAllHyperlinks();

            //clear any search highlighting when saving the document then restore after save
            HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(null);
            CopyItemRichText = MpHelpers.Instance.ConvertFlowDocumentToRichText(this.Document);
            HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(null);

            this.CreateAllHyperlinks();

            CopyItem.WriteToDatabase();
        }

        public async Task<string> GetPastableRichText() {
            if (HasTemplate) {
                IsPastingTemplateTile = true;
                TemplateRichText = string.Empty.ToRichText();
                if(IsCompositeParent) {
                    foreach (var ctvm in this) {
                        var rtbvmrt = await ctvm.GetPastableRichText();
                        TemplateRichText = MpHelpers.Instance.CombineRichText(rtbvmrt, TemplateRichText, true);
                    }
                } else {
                    ParentOrSelfClipTileViewModel.PasteTemplateToolbarViewModel.InitWithRichTextBox(this.Rtb);
                    await Task.Run(() => {
                        while (string.IsNullOrEmpty(TemplateRichText)) {
                            System.Threading.Thread.Sleep(500);
                        }
                        //TemplateRichText is set in PasteTemplateCommand
                    });
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
                    RichTextBoxListBox.ScrollIntoView(this[0]);
                    foreach(var ctvm in this) {
                        ctvm.Rtb.ScrollToHome();
                    }
                    break;
                case MpCopyItemType.FileList:
                    FileListBox.ScrollIntoView(FileListViewModels[0]);
                    break;
            }
        }
        #endregion

        #region Methods

        #region Drag & Drop
        void IDropTarget.DragOver(IDropInfo dropInfo) {
            //var sourceItem = dropInfo.Data as MpClipTileRichTextBoxViewModel;
            //MpClipTileRichTextBoxViewModelCollection targetRtbVmCollection = null;
            //MpClipTileRichTextBoxViewModel targetRtbVm = null;
            //if(dropInfo.TargetItem is MpClipTileRichTextBoxViewModel) {
            //    targetRtbVm = dropInfo.TargetItem as MpClipTileRichTextBoxViewModel;
            //    targetRtbVmCollection = targetRtbVm.RichTextBoxViewModelCollection;
            //} else if (dropInfo.TargetItem is MpClipTileRichTextBoxViewModelCollection) {                
            //    targetRtbVmCollection = dropInfo.TargetItem as MpClipTileRichTextBoxViewModelCollection;
            //    if(targetRtbVmCollection.Count > 0) {
            //        if (dropInfo.DropPosition.Y < 0) {
            //            targetRtbVm = targetRtbVmCollection[0];
            //        } else {
            //            targetRtbVm = targetRtbVmCollection[targetRtbVmCollection.Count - 1];
            //        }
            //    }                
            //}
            //if (sourceItem != null && targetRtbVm != null) {
            //    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            //    dropInfo.Effects = DragDropEffects.Move;
            //}

            var sourceItem = dropInfo.Data as MpClipTileViewModel;
            var targetItem = dropInfo.TargetItem as MpClipTileViewModel;

            if (sourceItem != null && targetItem != null) {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo) {
            var sourceRtbvm = dropInfo.Data as MpClipTileViewModel;
            if (sourceRtbvm == null) {
                return;
            }
            var sourceCollection = sourceRtbvm.ParentCollection;

            MpObservableCollection<MpClipTileViewModel> targetCollection = null;
            MpClipTileViewModel targetRtbVm = null;
            if (dropInfo.TargetItem is MpClipTileViewModel) {
                targetRtbVm = dropInfo.TargetItem as MpClipTileViewModel;
                targetCollection = targetRtbVm.ParentCollection;
            } else if (dropInfo.TargetItem is MpClipTrayListBoxViewModel) {
                targetCollection = dropInfo.TargetItem as MpClipTrayListBoxViewModel;
                if (targetCollection.Count > 0) {
                    if (dropInfo.DropPosition.Y < 0) {
                        targetRtbVm = targetCollection[0];
                    } else {
                        targetRtbVm = targetCollection[targetCollection.Count - 1];
                    }
                }
            }

            if (targetCollection != null) {
                sourceCollection.Remove(sourceRtbvm);

                if (sourceCollection != targetCollection) {
                    if(sourceCollection is MpClipTileViewModel) {
                        ((MpClipTileViewModel)sourceCollection).UpdateSortOrder();
                    }
                }
                int targetIdx = targetCollection.IndexOf(targetRtbVm);

                targetCollection.Insert(targetIdx, sourceRtbvm);

                if (targetCollection is MpClipTileViewModel) {
                    ((MpClipTileViewModel)targetCollection).UpdateSortOrder();
                }
            }
        }

        public void UpdateSortOrder() {
            ParentOrSelfClipTileViewModel.CopyItem.CompositeItemList.Clear();
            foreach (var ctvm in this) {
                ctvm.CompositeSortOrderIdx = this.IndexOf(ctvm);
                ctvm.CompositeParentCopyItemId = ParentOrSelfClipTileViewModel.CopyItemId;
                ParentOrSelfClipTileViewModel.CopyItem.CompositeItemList.Add(ctvm.CopyItem);
            }
            ParentOrSelfClipTileViewModel.CopyItem.WriteToDatabase();
        }
        #endregion
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
                  (CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Composite) && !IsPastingTemplateTile &&
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
        public object Clone() {
            var nrtbvm = new MpClipTileViewModel(CopyItem, ParentObject);
            nrtbvm.Rtb = new RichTextBox();
            nrtbvm.Rtb.Document = Rtb.Document.Clone();
            return nrtbvm;
        }

        public void Dispose() {
            MainWindowViewModel.ClipTrayViewModel.Remove(this);
            CopyItem.DeleteFromDatabase();
        }

        #endregion
    }
}
