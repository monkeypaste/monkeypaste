namespace MpWpfApp {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
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
    using Newtonsoft.Json;
    using MonkeyPaste;
using System.Speech.Synthesis;

    public class MpClipTileViewModel : MpViewModelBase<MpClipTrayViewModel> {
        #region Private Variables
        private int _detailIdx = 1;
        private List<string> _tempFileList = new List<string>();


        //container

        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;
        private List<MpClipTileViewModel> _hiddenTiles = new List<MpClipTileViewModel>();
        #endregion


        #region Properties

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

        private MpClipTileTitleSwirlViewModel _clipTileTitleSwirlViewModel = null;
        public MpClipTileTitleSwirlViewModel TitleSwirlViewModel {
            get {
                return _clipTileTitleSwirlViewModel;
            }
            set {
                if (_clipTileTitleSwirlViewModel != value) {
                    _clipTileTitleSwirlViewModel = value;
                    OnPropertyChanged(nameof(TitleSwirlViewModel));
                }
            }
        }

        private ObservableCollection<MpContentItemViewModel> _itemViewModels;
        public ObservableCollection<MpContentItemViewModel> ItemViewModels {
            get {
                return _itemViewModels;
            }
            private set {
                if (_itemViewModels != value) {
                    _itemViewModels = value;
                    OnPropertyChanged(nameof(ItemViewModels));
                    OnPropertyChanged(nameof(PrimaryItem));
                }
            }
        }

        //below from content container

        public MpContentItemViewModel HeadItem {
            get {
                if (ItemViewModels == null || ItemViewModels.Count == 0) {
                    return null;
                }
                return ItemViewModels.OrderBy(x => x.CopyItem.CompositeSortOrderIdx).ToList()[0];
            }
        }

        public MpContentItemViewModel TailItem {
            get {
                if (ItemViewModels == null || ItemViewModels.Count == 0) {
                    return null;
                }
                return ItemViewModels.OrderByDescending(x => x.CopyItem.CompositeSortOrderIdx).ToList()[0];
            }
        }

        public MpContentItemViewModel PrimaryItem {
            get {
                if (ItemViewModels == null || ItemViewModels.Count == 0) {
                    return null;
                }
                return SelectedItem == null ? HeadItem : SelectedItem;
            }
        }

        public MpContentItemViewModel SelectedItem {
            get {
                if (SelectedItems == null || SelectedItems.Count == 0) {
                    return null;
                }
                return SelectedItems[0];
            }
        }

        public List<MpContentItemViewModel> SelectedItems {
            get {
                return ItemViewModels.Where(x => x.IsSelected == true).OrderBy(x => x.LastSubSelectedDateTime).ToList();
            }
        }

        public List<MpContentItemViewModel> VisibleItems {
            get {
                return ItemViewModels.Where(x => x.ItemVisibility == Visibility.Visible).ToList();
            }
        }

        public List<string> FileList {
            get {
                var fl = new List<string>();
                var ivml = SelectedItems.Count == 0 ? ItemViewModels.ToList() : SelectedItems;
                foreach (var ivm in ivml) {
                    fl.AddRange(ivm.GetFileList());
                }
                return fl;
            }
        }

        #endregion

        #region Appearance

        #endregion

        #region Layout

        public double PasteTemplateToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
            }
        }

        public double TileTitleIconBorderSize {
            get {
                return MpMeasurements.Instance.ClipTileTitleIconBorderSize;
            }
        }

        public double TileTitleIconInnerBorderSize {
            get {
                return TileTitleIconBorderSize * 0.8;
            }
        }

        public double TileBorderMinWidth {
            get {
                return MpMeasurements.Instance.ClipTileBorderMinSize;
            }
        }

        public double TileBorderMaxWidth {
            get {
                var ds = TotalExpandedSize;
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
                if (_tileTitleTextGridWidth != value) {
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
                if (_tileContentHeight != value) {
                    _tileContentHeight = value;
                    OnPropertyChanged(nameof(TileContentHeight));
                }
            }
        }

        public double TileDetailHeight {
            get {
                //if(DetailGridVisibility != Visibility.Visible) {
                //    return 0;
                //}
                return MpMeasurements.Instance.ClipTileDetailHeight;
            }
        }

        public double TileContentMaxWidth {
            get {
                return TileBorderMaxWidth - MpMeasurements.Instance.ClipTileContentMargin;
            }
        }

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
                return Math.Max(MpMeasurements.Instance.ClipTileEditModeMinWidth, TotalExpandedSize.Width);
            }
        }

        public double TitleFontSize {
            get {
                return MpMeasurements.Instance.ClipTileTitleFontSize;
            }
        }


        //content container
        public Size ContainerSize {
            get {
                var cs = new Size(MpMeasurements.Instance.ClipTileScrollViewerWidth, 0);
                if (Parent == null) {
                    return cs;
                }
                double ch = MpMeasurements.Instance.ClipTileContentHeight;
                if (IsEditingContent) {
                    ch -= MpMeasurements.Instance.ClipTileEditToolbarHeight;
                }
                if (IsPastingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                }
                if (IsEditingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTileEditTemplateToolbarHeight;
                }
                if (Count == 1) {
                    cs.Height = ch;
                } else {
                    double h = IsExpanded ? TotalExpandedSize.Height : TotalUnexpandedSize.Height;
                    cs.Height = Math.Max(MpMeasurements.Instance.ClipTileScrollViewerWidth, Math.Max(ch, h));
                }
                return cs;
            }
        }

        public Size ContentSize {
            get {
                return new Size(
                    ContainerSize.Width - MpMeasurements.Instance.ClipTileBorderThickness,
                    ContainerSize.Height - MpMeasurements.Instance.ClipTileBorderThickness);
            }
        }

        public double ContainerWidth {
            get {
                return ContainerSize.Width;
            }
        }

        public double ContentHeight {
            get {
                return ContentSize.Height;
            }
        }

        public double ContentWidth {
            get {
                return ContentSize.Width;
            }
        }

        public double ContainerHeight {
            get {
                return ContainerSize.Height;
            }
        }

        public Size TotalExpandedSize {
            get {
                var ts = new Size(
                MpMeasurements.Instance.ClipTileEditModeMinWidth,
                0);
                foreach (var ivm in ItemViewModels) {
                    var ivs = ivm.GetExpandedSize();
                    ts.Width = Math.Max(ts.Width, ivs.Width);
                    ts.Height += ivs.Height;
                }
                return ts;
            }
        }


        public Size TotalUnexpandedSize {
            get {
                return new Size(
                MpMeasurements.Instance.ClipTileContentMinWidth,
                MpMeasurements.Instance.ClipTileContentHeight);
                //foreach (var ivm in ItemViewModels) {
                //    var ivs = ivm.GetExpandedSize();
                //    ts.Width = Math.Max(ts.Width, ivs.Width);
                //}
                //return ts;
            }
        }

        #endregion

        #region Visibility        
        public Visibility DetailGridVisibility {
            get {
                return IsEditingTemplate || IsPastingTemplate ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility AppIconHighlightBorderVisibility {
            get {
                return HighlightTextRangeViewModelCollection.HasAppMatch ? Visibility.Visible : Visibility.Hidden;
            }
        }
        public Visibility ToolTipVisibility {
            get {
                if (HeadItem == null || !Properties.Settings.Default.ShowItemPreview) {
                    return Visibility.Collapsed;
                }
                return (MpClipTrayViewModel.Instance.IsScrolling || IsSelected) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility ToggleEditModeButtonVisibility {
            get {
                return ((IsHovering || IsExpanded) &&
                        MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1) ?
                        Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility ClipTileTitleAppIconButtonVisibility {
            get {
                return IsExpanded ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public Visibility ClipTileTitleAppIconImageVisibility {
            get {
                if (MainWindowViewModel == null || MpClipTrayViewModel.Instance == null || !IsSelected) {
                    return Visibility.Visible;
                }
                if (MpClipTrayViewModel.Instance.SelectedClipTiles.Count > 1 &&
                   !IsHovering) {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }

        public Visibility LoadingSpinnerVisibility {
            get {
                if (IsLoading || IsBusy) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ContentVisibility {
            get {
                if (IsLoading || IsBusy) {
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
                if (IsSelected) {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
        }

        public Visibility TileDetailGridVisibility {
            get {
                if (IsEditingContent || IsEditingTemplate || IsPastingTemplate) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
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
                       (MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1 ||
                        MpClipTrayViewModel.Instance.IsAnyTileExpanded)) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility MultiSelectOrderMarkerVisibility {
            get {
                if (MainWindowViewModel == null || MpClipTrayViewModel.Instance == null) {
                    return Visibility.Hidden;
                }
                if (IsSelected && MpClipTrayViewModel.Instance.SelectedClipTiles.Count > 1) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
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
                if (HeadItem == null ||
                   !IsAnySubContextMenuOpened) {
                    return new Rect(50, 0, 50, 50);
                }
                if (IsAnySubContextMenuOpened) {
                    return new Rect(0, 0, 50, 50);
                }
                return new Rect(50, 0, 50, 50);
            }
        }

        public Brush TileBorderBrush {
            get {
                if (MpClipTrayViewModel.Instance.PrimarySelectedClipTile == this &&
                    MpClipTrayViewModel.Instance.SelectedClipTiles.Count > 1) {
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
                if (IsHoveringOnTitleTextGrid && !IsEditingTitle) {
                    return new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Business Logic
        public string TemplateRichText { get; set; }

        private bool _wasAddedAtRuntime = false;
        public bool WasAddedAtRuntime {
            get {
                return _wasAddedAtRuntime;
            }
            set {
                if (_wasAddedAtRuntime != value) {
                    _wasAddedAtRuntime = value;
                    OnPropertyChanged(nameof(WasAddedAtRuntime));
                }
            }
        }

        public bool IsLoading {
            get {
                return HeadItem == null || HeadItem.CopyItem.Id == 0;
            }
        }

        public int SortOrderIdx {
            get {
                if (MpClipTrayViewModel.Instance == null || ItemVisibility != Visibility.Visible) {
                    return -1;
                }
                return MpClipTrayViewModel.Instance.VisibileClipTiles.IndexOf(this);
            }
        }

        public bool IsTextItem {
            get {
                return HeadItem.CopyItem.ItemType == MpCopyItemType.Csv ||
                        HeadItem.CopyItem.ItemType == MpCopyItemType.Html ||
                        HeadItem.CopyItem.ItemType == MpCopyItemType.RichText;
            }
        }
        #endregion

        #region State Properties 
        public string MultiSelectedOrderIdxDisplayValue {
            get {
                if (MainWindowViewModel == null || MpClipTrayViewModel.Instance == null || !IsSelected) {
                    return string.Empty;
                }
                int multiIdx = MpClipTrayViewModel.Instance.GetSelectionOrderIdxForItem(this);
                if (multiIdx < 0) {
                    return string.Empty;
                }
                multiIdx++;
                return multiIdx.ToString();
            }
        }

        public bool IsDroppingOntoNotepad { get; set; } = false;

        public Point MouseDownPosition { get; set; } = new Point();

        public IDataObject DragDataObject { get; set; }

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
                return IsClipDragging || ItemViewModels.Any(x => x.IsSubDragging);
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

        private bool _isEditingContent = false;
        public bool IsEditingContent {
            get {
                return _isEditingContent;
            }
            set {
                if (_isEditingContent != value) {
                    _isEditingContent = value;
                    OnPropertyChanged(nameof(IsEditingContent));
                    OnPropertyChanged(nameof(IsRtbReadOnly));
                    OnPropertyChanged(nameof(HeadItem));
                    OnPropertyChanged(nameof(TileDetailGridVisibility));
                    OnPropertyChanged(nameof(TileContentHeight));
                    OnPropertyChanged(nameof(IsExpanded));
                    OnPropertyChanged(nameof(ClipTileTitleAppIconButtonVisibility));
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
                    OnPropertyChanged(nameof(ClipTileTitleAppIconButtonVisibility));
                    OnPropertyChanged(nameof(DetailGridVisibility));
                    OnPropertyChanged(nameof(TileDetailHeight));
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
                    OnPropertyChanged(nameof(ClipTileTitleAppIconButtonVisibility));
                    OnPropertyChanged(nameof(SelectionOverlayGridVisibility));
                    OnPropertyChanged(nameof(DetailGridVisibility));
                    OnPropertyChanged(nameof(TileDetailHeight));
                }
            }
        }

        private bool _isExpanding = false;
        public bool IsExpanding {
            get {
                return _isExpanding;
            }
            set {
                if (_isExpanding != value) {
                    _isExpanding = value;
                    OnPropertyChanged(nameof(IsExpanding));
                    OnPropertyChanged(nameof(ContentVisibility));
                }
            }
        }

        public bool IsExpanded {
            get {
                if (IsPastingTemplate || IsEditingTemplate || IsEditingContent) {
                    return true;
                }
                return false;
            }
        }

        public bool IsRtbReadOnly {
            get {
                return !IsEditingContent;
            }
        }

        private DateTime _lastSelectedDateTime;
        public DateTime LastSelectedDateTime {
            get {
                return _lastSelectedDateTime;
            }
            set {
                if (_lastSelectedDateTime != value) {
                    _lastSelectedDateTime = value;
                    OnPropertyChanged(nameof(LastSelectedDateTime));
                }
            }
        }

        public bool IsAnySubContextMenuOpened {
            get {
                return IsAnyContextMenuOpened;
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
                if (_isSelected != value) {
                    // && (!MpClipTrayViewModel.Instance.IsAnyTileExpanded || IsExpanded || (!IsExpanded && value == false))) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    //MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.SelectedClipTiles));
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
                if (_isHovering != value && (!MpClipTrayViewModel.Instance.IsAnyTileExpanded || IsExpanded)) {
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

        private bool _isPlaceholder = false;
        public bool IsPlaceholder {
            get {
                return _isPlaceholder;
            }
            set {
                if (_isPlaceholder != value) {
                    _isPlaceholder = value;
                    OnPropertyChanged(nameof(IsPlaceholder));
                }
            }
        }


        #endregion

        #region Model
        //public BitmapSource CopyItemFavIcon {
        //    get {
        //        if (CopyItemUrlDomain == null) {
        //            return null;
        //        }
        //        return CopyItemUrlDomain.FavIcon.IconImage.ImageBase64.ToBitmapSource();
        //    }
        //    set {
        //        if (CopyItemUrlDomain != null) {
        //            CopyItemUrlDomain.FavIcon.IconImage.ImageBase64 = value.ToBase64String();
        //            CopyItemUrlDomain.FavIcon.IconImage.WriteToDatabase();
        //            OnPropertyChanged(nameof(CopyItemFavIcon));
        //            OnPropertyChanged(nameof(AppIcon));

        //            OnPropertyChanged(nameof(TileTitleIconSize));
        //            OnPropertyChanged(nameof(TileTitleIconBorderSize));
        //        }
        //    }
        //}

        public string DetailText {
            get {
                if (HeadItem == null) {
                    return string.Empty;
                }
                _detailIdx++;
                if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    _detailIdx = 1;
                }
                // TODO this should aggregate details over all sub items 
                return PrimaryItem.GetDetail((MpCopyItemDetailType)_detailIdx);
            }
        }

        public int LineCount { get; set; }

        public int CharCount { get; set; }

        public int FileCount { get; set; }

        public double DataSizeInMb { get; set; }

        public Brush TitleBackgroundColor {
            get {
                if (PrimaryItem.CopyItem == null) {
                    return Brushes.Transparent;
                }
                return new SolidColorBrush(MpHelpers.Instance.ConvertHexToColor(PrimaryItem.CopyItem.ItemColor));
            }
            set {
                if (PrimaryItem.CopyItem != null && PrimaryItem.CopyItem.ItemColor != MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color)) {
                    PrimaryItem.CopyItem.ItemColor = MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color);
                    PrimaryItem.CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(TitleBackgroundColor));
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


        #endregion

        #endregion

        #region Events
        public event EventHandler OnTileSelected;

        public event EventHandler<string> OnSearchRequest;
        #endregion

        #region Public Methods
        public MpClipTileViewModel() : base(null) { }

        public MpClipTileViewModel(bool isPlaceholder) : this() {
            if (isPlaceholder) {
                IsBusy = true;
                //PrimaryItem.CopyItem = null;
            }
        }

        public MpClipTileViewModel(MpClipTrayViewModel parent, MpCopyItem ci) : base(parent) {
            PropertyChanged += MpClipTileViewModel_PropertyChanged;
            InitContent(ci);
        }

        private void MpClipTileViewModel_PropertyChanged(object s, System.ComponentModel.PropertyChangedEventArgs e1) {
            var ctvm = s as MpClipTileViewModel;
            switch (e1.PropertyName) {
                //case nameof(ctvm.PrimaryItem.CopyItem):
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemId));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemType));
                //    ctvm.OnPropertyChanged(nameof(ctvm.ImgVisibility));
                //    ctvm.OnPropertyChanged(nameof(ctvm.RtbVisibility));
                //    ctvm.OnPropertyChanged(nameof(ctvm.FileListVisibility));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemTitle));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemPlainText));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemRichText));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemDescription));
                //    //ctvm.OnPropertyChanged(nameof(ctvm.CopyItemFileDropList));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemAppName));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemAppPath));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemUsageScore));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemAppId));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyItemCreatedDateTime));
                //    ctvm.OnPropertyChanged(nameof(ctvm.DetailText));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CopyCount));
                //    ctvm.OnPropertyChanged(nameof(ctvm.PasteCount));
                //    ctvm.OnPropertyChanged(nameof(ctvm.CharCount));
                //    ctvm.OnPropertyChanged(nameof(ctvm.LineCount));
                //    ctvm.OnPropertyChanged(nameof(ctvm.FileCount));
                //    ctvm.OnPropertyChanged(nameof(ctvm.DataSizeInMb));
                //    ctvm.OnPropertyChanged(nameof(ctvm.IsLoading));
                //    ctvm.OnPropertyChanged(nameof(ctvm.LoadingSpinnerVisibility));
                //    ctvm.OnPropertyChanged(nameof(ctvm.ContentVisibility));
                //    ctvm.OnPropertyChanged(nameof(ctvm.TrialOverlayVisibility));
                //    ctvm.OnPropertyChanged(nameof(ctvm.TitleFontSize));
                //    ctvm.OnPropertyChanged(nameof(ctvm.TileTitleIconSize));
                //    ctvm.OnPropertyChanged(nameof(ctvm.TileTitleIconBorderSize));
                //    break;
                case nameof(ctvm.IsSelected):
                    if (ctvm.IsSelected) {
                        if (ctvm.ItemVisibility != Visibility.Visible && !MpClipTrayViewModel.Instance.IsPastingHotKey) {
                            ctvm.IsSelected = false;
                            break;
                        }
                        //if(ctvm.ContentContainerViewModel.SubSelectedContentItems.Count == 0) {
                        //    ctvm.ContentContainerViewModel.ItemViewModels[0].IsSelected = true;
                        //}
                        ctvm.LastSelectedDateTime = DateTime.Now;
                        OnTileSelected?.Invoke(this, null);
                    } else {
                        ctvm.ClearSubSelection();
                        ctvm.LastSelectedDateTime = DateTime.MaxValue;

                        //multi-select label stuff (disabled)
                        //ctvm.OnPropertyChanged(nameof(ctvm.MultiSelectOrderMarkerVisibility));
                        //ctvm.OnPropertyChanged(nameof(ctvm.ClipTileTitleAppIconImageVisibility));

                        //foreach(var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                        //    rtbvm.OnPropertyChanged(nameof(rtbvm.MultiSelectOrderMarkerVisibility));
                        //    rtbvm.OnPropertyChanged(nameof(rtbvm.AppIconImageVisibility));
                        //}
                    }

                    ctvm.RefreshAsyncCommands();
                    break;
                case nameof(ctvm.IsHovering):
                    if (ctvm.IsHovering) {
                        if (MpClipTrayViewModel.Instance.IsScrolling) {
                            ctvm.IsHovering = false;
                            ctvm.ClearSubHovering();
                        }
                        foreach (var vctvm in MpClipTrayViewModel.Instance.VisibileClipTiles) {
                            if (vctvm != this) {
                                vctvm.IsHovering = false;
                                vctvm.ClearSubHovering();
                            }
                        }
                    } else {
                        //_detailIdx = 1;
                    }
                    break;
                case nameof(ctvm.IsEditingContent):
                    if (ctvm.IsEditingContent) {
                        MainWindowViewModel.ExpandClipTile(this);
                    } else {
                        ctvm.SaveToDatabase();
                        MainWindowViewModel.ShrinkClipTile(this);
                    }
                    break;
                case nameof(ctvm.IsEditingTemplate):
                    if (ctvm.IsEditingTemplate) {
                        ctvm.TileContentHeight += ctvm.TileDetailHeight;
                    } else {
                        ctvm.TileContentHeight -= ctvm.TileDetailHeight;
                    }
                    break;
                case nameof(ctvm.IsPastingTemplate):
                    if (ctvm.IsPastingTemplate) {
                        ctvm.TileContentHeight += ctvm.TileDetailHeight;
                    } else {
                        ctvm.TileContentHeight -= ctvm.TileDetailHeight;
                    }
                    break;
                case nameof(ctvm.IsClipDropping):
                    //foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                    //    rtbvm.UpdateLayout();
                    //}
                    break;
                case nameof(ctvm.IsBusy):
                    ctvm.OnPropertyChanged(nameof(ctvm.ContentVisibility));
                    ctvm.OnPropertyChanged(nameof(ctvm.LoadingSpinnerVisibility));
                    break;
                case nameof(ctvm.ItemVisibility):
                    if (ctvm.ItemVisibility == Visibility.Collapsed) {
                        return;
                    }
                    if (ctvm.IsPlaceholder) {
                        ctvm.ItemVisibility = Visibility.Collapsed;
                    }
                    break;
            }
        }

        public void RequestSearch(string st) {
            OnSearchRequest?.Invoke(this, st);
        }

        #region Loading Initializers
        public void InitContent(MpCopyItem ci) {
            if (ci == null) {
                //throw new Exception("MpClipTileViewModel error, cannot set null copyitem");
                //CopyItem = null;
                IsBusy = true;
                return;
            }
            if (ci.Id == 0 && !MpMainWindowViewModel.IsMainWindowLoading) {
                //ci.WriteToDatabase();
                IsBusy = true;
                _wasAddedAtRuntime = true;
            }

            OnSubSelectionChanged += ContentContainerViewModel_OnSubSelectionChanged;

            Initialize(ci);

            TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);
            HighlightTextRangeViewModelCollection = new MpHighlightTextRangeViewModelCollection(this);

            OnPropertyChanged(nameof(PrimaryItem));
        }

        #endregion
  

        

        public void Resize(
            double deltaWidth,
            double deltaHeight,
            double deltaEditToolbarTop) {
            TileBorderWidth += deltaWidth;
            TileContentWidth += deltaWidth;

            TileBorderHeight += deltaHeight;
            TileContentHeight += deltaHeight;

            foreach(var civm in ItemViewModels) {
                civm.Resize(new Rect());
            }
           // ContentContainerViewModel.Resize(deltaEditToolbarTop, deltaWidth, deltaHeight);

            //EditTemplateToolbarViewModel.Resize(deltaHeight,deltaWidth);

        }

        public void RefreshAsyncCommands() {
            if (MpMainWindowViewModel.IsMainWindowLoading) {
                return;
            }

            MpClipTrayViewModel.Instance.HotkeyPasteCommand.RaiseCanExecuteChanged();
            MpClipTrayViewModel.Instance.BringSelectedClipTilesToFrontCommand.RaiseCanExecuteChanged();
            MpClipTrayViewModel.Instance.SendSelectedClipTilesToBackCommand.RaiseCanExecuteChanged();
            MpClipTrayViewModel.Instance.SpeakSelectedClipsCommand.RaiseCanExecuteChanged();
            MpClipTrayViewModel.Instance.MergeSelectedClipsCommand.RaiseCanExecuteChanged();
        }

        public MpEventEnabledFlowDocument GetSeparatedCompositeFlowDocument(string separatorChar = "- ") {
            var ccil = MpCopyItem.GetCompositeChildren(HeadItem.CopyItem);
            if (ccil.Count == 0) {
                return HeadItem.CopyItem.ItemData.ToFlowDocument();
            }
            int maxCols = int.MinValue;
            foreach (var cci in ccil) {
                maxCols = Math.Max(maxCols, MpHelpers.Instance.GetColCount(cci.ItemData.ToPlainText()));
            }
            string separatorLine = string.Empty;
            for (int i = 0; i < maxCols; i++) {
                separatorLine += separatorChar;
            }
            var separatorDocument = separatorLine.ToRichText().ToFlowDocument();
            var fullDocument = string.Empty.ToRichText().ToFlowDocument();
            for (int i = 0; i < ccil.Count; i++) {
                var cci = ccil[i];
                if (i != 0) {
                    MpHelpers.Instance.CombineFlowDocuments(
                    separatorDocument,
                    fullDocument,
                    false);
                }
                MpHelpers.Instance.CombineFlowDocuments(
                    cci.ItemData.ToFlowDocument(),
                    fullDocument,
                    false);
            }

            var ps = fullDocument.GetDocumentSize();
            fullDocument.PageWidth = ps.Width;
            fullDocument.PageHeight = ps.Height;
            return fullDocument;
        }

        public void MergeCopyItemList(
            List<MpCopyItem> ocil,
            int forceIdx = -1) {
            InsertRange(forceIdx, ocil);
            //IsBusy = true;
            //foreach (var oci in ocil) {
            //    if(oci == null) {
            //        continue;
            //    }
            //    //if (oci.Id == CopyItemId) {
            //    //    return;
            //    //}
            //    var ovm = MpClipTrayViewModel.Instance.GetCopyItemViewModelById(oci.Id);
            //    if(ovm == null) {
            //        //MonkeyPaste.MpConsole.WriteLine(@"ClipTIle.Merge error cannot find copy item w/ id: " + oci.CopyItemId+ " so ignoring" );
            //        //continue;
            //        ovm = new MpClipTileViewModel(oci);
            //    }
            //    if (ovm is MpClipTileViewModel octvm) {
            //        //if copyitem is an existing tile remove it
            //        MpClipTrayViewModel.Instance.ClipTileViewModels.Remove(octvm);
            //        octvm.IsClipDragging = false;
            //        //if(CopyItemType == MpCopyItemType.RichText) {
            //        //    CopyItem = MpCopyItem.Merge(oci, CopyItem, false, false, forceIdx);
            //        //}
            //        var ortbvm = octvm.ContentContainerViewModel.ItemViewModels[0];
            //        octvm.ContentContainerViewModel.ItemViewModels.Remove(ortbvm, true);
            //        ContentContainerViewModel.Add(ortbvm,forceIdx, true);
            //    } else if(ovm is MpContentListItemViewModel ortbvm) {
            //        if (ortbvm.HostClipTileViewModel == this && forceIdx >= 0) {
            //            //occurs when rtbvmc items are resorted
            //            forceIdx = Math.Min(ContentContainerViewModel.Count - 1, forceIdx);
            //            ContentContainerViewModel.Move(ContentContainerViewModel.IndexOf(ortbvm), forceIdx);
            //            ContentContainerViewModel.UpdateSortOrder();
            //            continue;
            //        } else {
            //            //if copyitem is part of composite remove it 
            //            ortbvm.ContentContainerViewModel.Remove(ortbvm, true);
            //            ContentContainerViewModel.Add(ortbvm, forceIdx, true);
            //        }
            //    }

            //    //CopyItem = MpCopyItem.Merge(oci, CopyItem, false, false, forceIdx);
            //}
            //var scvml = MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == CopyItemId).ToList();
            //if (scvml.Count > 0) {
            //    ShortcutKeyString = scvml[0].KeyString;
            //} else {
            //    ShortcutKeyString = string.Empty;
            //}

            MainWindowViewModel.TagTrayViewModel.RefreshAllCounts();

            //RichTextBoxViewModelCollection.SyncItemsWithModel();

            //MpClipTrayViewModel.Instance.ClearClipSelection();
            //IsSelected = true;

            SaveToDatabase();

            //IsBusy = false;
            ClearSubSelection();
            RequestUiUpdate();
            MpClipTrayViewModel.Instance.Refresh();
        }
        public async Task MergeCopyItemListAsync(
            List<MpCopyItem> ocil,
            int forceIdx = -1,
            DispatcherPriority priority = DispatcherPriority.Background) {
            await Application.Current.Dispatcher.BeginInvoke(priority, (Action)(() => { MergeCopyItemList(ocil, forceIdx); }));
        }

        public void ClearDragDropState() {
            IsClipDragging = false;
            IsClipDropping = false;
            MouseDownPosition = new Point();
            DragDataObject = null;

            ClearAllSubDragDropState();
        }
        public async Task GatherAnalytics() {
            var analyticsTask = new List<Task>();
            Task itemTask = null;
            foreach(var ivm in ItemViewModels) {
                itemTask = ivm.GatherAnalytics();
                analyticsTask.Add(itemTask);
            }
            await Task.WhenAll(analyticsTask.ToArray());
        }

        //public void FadeIn(double bt = 0, double ms = 1000) {
        //    MpHelpers.Instance.AnimateVisibilityChange(
        //        ClipBorder,
        //        Visibility.Visible,
        //        (s, e) => {
        //            ItemVisibility = Visibility.Visible;
        //        },
        //        ms, bt);
        //}

        //public void FadeOut(Visibility outVisibility = Visibility.Collapsed, double bt = 0, double ms = 1000) {
        //    MpHelpers.Instance.AnimateVisibilityChange(
        //        ClipBorder,
        //        outVisibility,
        //        (s, e) => {
        //            ItemVisibility = outVisibility;
        //        },
        //        ms, bt);
        //}


        public void Refresh() {
            var sw = new Stopwatch();
            sw.Start();
            RequestUiUpdate();
            //if (FileListBox != null) {
            //    FileListBox.Items.Refresh();
            //}
            sw.Stop();
            MonkeyPaste.MpConsole.WriteLine("ClipTile(VIdx:" + MpClipTrayViewModel.Instance.VisibileClipTiles.IndexOf(this) + ") Refreshed (" + sw.ElapsedMilliseconds + "ms)");
        }

        public void ClearClipSelection() {
            IsSelected = false;
            LastSelectedDateTime = DateTime.MaxValue;
            ClearSubSelection();
        }

        public void SaveToDatabase() {
            var sw = new Stopwatch();
            sw.Start();

            TextSelection rtbSelection = null;
            if (SelectedItems.Count == 1 && IsEditingContent) {
                //rtbSelection = ContentContainerViewModel.SubSelectedContentItems[0].Rtb.Selection;
                MonkeyPaste.MpConsole.WriteLine("(AddTemplate)Selection Text: " + rtbSelection.Text);
            }

            //remove links to update model rich text
            //RichTextBoxViewModelCollection.ClearAllHyperlinks();

            //clear any search highlighting when saving the document then restore after save
            HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(null);

            var rtsw = new Stopwatch();
            rtsw.Start();
            foreach (var ivm in ItemViewModels) {
                ivm.SaveToDatabase();
            }
            rtsw.Stop();
            MonkeyPaste.MpConsole.WriteLine("Saving rich text from rtb's time: " + rtsw.ElapsedMilliseconds + "ms");

            //CopyItemRichText = RichTextBoxViewModelCollection.FullDocument.ToRichText();
            HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(null);
            //RichTextBoxViewModelCollection.CreateAllHyperlinks();
            //CopyItem.WriteToDatabase();

            var cipcsw = new Stopwatch();
            cipcsw.Start();
            //if(CopyItemType == MpCopyItemType.RichText) {
            //    CopyItemBmp = RichTextBoxViewModelCollection[0].CopyItemBmp;
            //}

            //CopyItemBmp = GetSeparatedCompositeFlowDocument().ToBitmapSource();
            //OnPropertyChanged(nameof(CopyItem));
            cipcsw.Stop();
            MonkeyPaste.MpConsole.WriteLine("Saving cliptile copyitem propertychanged time: " + cipcsw.ElapsedMilliseconds + "ms");

            sw.Stop();
            MonkeyPaste.MpConsole.WriteLine("Saving(VIdx:" + MpClipTrayViewModel.Instance.VisibileClipTiles.IndexOf(this) + "): " + sw.ElapsedMilliseconds + "ms");

            if (rtbSelection != null && SelectedItems.Count == 1) {
                //ContentContainerViewModel.SubSelectedContentItems[0].Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }


        public async Task<string> GetSubSelectedPastableRichText(bool isToExternalApp = false) {
            if(IsTextItem) {
                if (SelectedItems.Count == 0) {
                    SubSelectAll();
                }
                bool isPastingTemplate = SelectedItems.Any(x => x.HasTokens);
                if (isPastingTemplate) {
                    IsPastingTemplate = true;
                    TemplateRichText = string.Empty.ToRichText();
                    if (!MpMainWindowViewModel.IsMainWindowOpen) {
                        MainWindowViewModel.ShowWindowCommand.Execute(null);
                    }
                    RequestUiUpdate();
                    //if(!Application.Current.MainWindow.IsActive) {
                    //    bool result = Application.Current.MainWindow.Activate();
                    //    Application.Current.MainWindow.IsManipulationEnabled = true;
                    //    Application.Current.MainWindow.Focus();                        
                    //}


                    //await ContentContainerViewModel.FillAllTemplates();
                }
                //var sb = new StringBuilder();
                //sb.Append(string.Empty.ToRichText());
                if (isPastingTemplate) {
                    Application.Current.MainWindow.Cursor = Cursors.Wait;
                    Application.Current.MainWindow.ForceCursor = true;
                }
                var sw = new Stopwatch();
                sw.Start();
                string rtf = string.Empty.ToRichText();
                foreach (var rtbvm in SelectedItems) {
                    if (rtbvm.HasTokens) {
                        rtbvm.IsSelected = true;
                        //(ContentContainerViewModel as MpRtbItemCollectionViewModel).PasteTemplateToolbarViewModel.SubSelectedRtbViewModel = rtbvm as MpContentItemViewModel;
                        //(ContentContainerViewModel as MpRtbItemCollectionViewModel).PasteTemplateToolbarViewModel.PasteTemplateCommand.Execute(null);
                        string rtbvmrtf = rtbvm.TemplateRichText;
                        rtf = MpHelpers.Instance.CombineRichText(rtbvmrtf, rtf, true);
                        //rtbvm.TemplateRichText = string.Empty;
                        rtbvm.TokenCollection.ResetAll();
                    } else {
                        rtf = MpHelpers.Instance.CombineRichText(rtbvm.CopyItem.ItemData.ToRichText(), rtf, true);
                    }
                }
                sw.Stop();
                MonkeyPaste.MpConsole.WriteLine(@"Time to combine richtext: " + sw.ElapsedMilliseconds + "ms");
                if (isPastingTemplate) {
                    Application.Current.MainWindow.Cursor = Cursors.Arrow;
                    Application.Current.MainWindow.ForceCursor = true;
                }
                return rtf;
            }

            return string.Empty;
            //both return to ClipTray.GetDataObjectFromSelectedClips
        }

        public void DeleteTempFiles() {
            foreach (var f in _tempFileList) {
                if (File.Exists(f)) {
                    File.Delete(f);
                }
            }
        }

        public void ResetContentScroll() {
            RequestScrollToHome();
        }
        #endregion

        #region Private Methods           
        private void ContentContainerViewModel_OnSubSelectionChanged(object sender, object e) {
            if (e != null && !IsSelected) {
                IsSelected = true;
            }
        }
        #endregion


        #region Overrides

        //public override string ToString() {
        //    return CopyItemPlainText;
        //}

        public void Dispose(bool isMerge = false) {
            if (MpClipTrayViewModel.Instance.ClipTileViewModels.Contains(this)) {
                MpClipTrayViewModel.Instance.Remove(this);
            }
            if (!isMerge) {
                foreach (var rtbvm in ItemViewModels) {
                    //remove any shortcuts associated with clip
                    var scvmToRemoveList = new List<MpShortcutViewModel>();
                    foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Shortcuts.Where(x => x.CopyItemId == rtbvm.CopyItem.Id).ToList()) {
                        scvmToRemoveList.Add(scvmToRemove);
                    }
                    foreach (var scvmToRemove in scvmToRemoveList) {
                        MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
                    }
                    rtbvm.CopyItem.DeleteFromDatabase();
                }
            }

            //ClipBorder = null;
            //TitleTextBox = null;
            //TitleTextBlock = null;
            //Image = null;
            //FileListBox = null;
            //ClipTileSelectionOverlayGrid = null;
            //ClipBorder = null;
            //ClipBorderTranslateTransform = null;
        }
        #endregion



        #region Content Container stuff

        #region Private Variables
        #endregion

        #region Properties

        #region Visibility
        public ScrollBarVisibility HorizontalScrollbarVisibility {
            get {
                if (Parent == null) {
                    return ScrollBarVisibility.Hidden;
                }
                if (IsExpanded) {
                    if (TotalExpandedSize.Width > ContentWidth) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility VerticalScrollbarVisibility {
            get {
                if (Parent == null) {
                    return ScrollBarVisibility.Hidden;
                }
                if (IsExpanded) {
                    if (TotalExpandedSize.Height > ContainerSize.Height) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }
        #endregion

        #region Layout
        
        #endregion

        #region State

        public bool IsAnyContentDragging {
            get {
                return ItemViewModels.Any(x => x.IsSubDragging);
            }
        }

        public bool IsAnyContentDropping {
            get {
                return ItemViewModels.Any(x => x.IsSubDropping);
            }
        }

        public bool IsAnyContextMenuOpened {
            get {
                return ItemViewModels.Any(x => x.IsContextMenuOpen);
            }
        }

        public bool IsAnySelected {
            get {
                return ItemViewModels.Any(x => x.IsSelected);
            }
        }

        public bool HasTokens {
            get {
                return ItemViewModels.Any(x => x.HasTokens);
            }
        }

        public int Count {
            get {
                return ItemViewModels.Count;
            }
        }

        public bool IsAnyEditingContent {
            get {
                if (Parent == null) {
                    return false;
                }
                return ItemViewModels.Any(x => x.IsEditingContent);
            }
        }

        public bool IsAnyEditingTitle {
            get {
                return ItemViewModels.Any(x => x.IsEditingTitle);
            }
        }

        public bool IsAnyPastingTemplate {
            get {
                return ItemViewModels.Any(x => x.IsPastingTemplate);
            }
        }
        #endregion

        #endregion

        #region Events

        public event EventHandler OnUiUpdateRequest;
        public event EventHandler<object> OnScrollIntoViewRequest;
        public event EventHandler OnScrollToHomeRequest;
        public event EventHandler<object> OnSubSelectionChanged;

        #endregion


        #region Public Methods

        public void Initialize(MpCopyItem headItem) {
            var ccil = MpCopyItem.GetCompositeChildren(headItem);
            ccil.Insert(0, headItem);

            var civml = new List<MpContentItemViewModel>();
            foreach (var cci in ccil) {
                civml.Add(new MpContentItemViewModel(this, cci));
            }

            ItemViewModels = new ObservableCollection<MpContentItemViewModel>(
                civml.OrderBy(x => x.CopyItem.CompositeSortOrderIdx).ToList());

            foreach (var ivm in ItemViewModels) {
                BindItemEvents(ivm);
            }

            ResetSubSelection();
        }

        private void BindItemEvents(MpContentItemViewModel ivm) {
            ivm.OnSubSelected += ItemViewModel_OnSubSelected;
        }

        private void ItemViewModel_OnSubSelected(object sender, EventArgs e) {
            OnPropertyChanged(nameof(SelectedItems));
            OnPropertyChanged(nameof(SelectedItem));
            OnSubSelectionChanged?.Invoke(this, sender);
        }

        #region View Event Invokers
        public void RequestScrollIntoView(object obj) {
            OnScrollIntoViewRequest?.Invoke(this, obj);
        }

        public void RequestScrollToHome() {
            OnScrollToHomeRequest?.Invoke(this, null);
        }

        public void RequestUiUpdate() {
            OnUiUpdateRequest?.Invoke(this, null);
        }

        #endregion

        public async Task UserPreparingDynamicPaste() {
            await Task.Delay(1);
        }

        public void ClearAllSubDragDropState() {
            foreach (var ivm in ItemViewModels) {
                ivm.ClearSubDragState();
            }
        }

        public void ResetSubSelection(List<MpContentItemViewModel> origSel = null) {
            ClearSubSelection();
            if (VisibleItems.Count > 0) {
                if (origSel == null) {
                    VisibleItems[0].IsSelected = true;
                } else {
                    foreach (var sivm in origSel) {
                        var ivm = ItemViewModels.Where(x => x.CopyItem.Id == sivm.CopyItem.Id).FirstOrDefault();
                        if (ivm == null) {
                            continue;
                        }
                        ivm.IsSelected = true;
                    }
                }

            }
        }

        public void ClearSubSelection() {
            foreach (var ivm in ItemViewModels) {
                ivm.IsSelected = false;
            }
        }

        public void ClearSubHovering() {
            foreach (var ivm in ItemViewModels) {
                ivm.IsHovering = false;
            }
        }

        public void SubSelectAll() {
            foreach (var ivm in ItemViewModels) {
                ivm.IsSelected = true;
            }
        }

        public MpContentItemViewModel GetItemByCopyItemId(int copyItemId) {
            foreach (var rtbvm in ItemViewModels) {
                if (rtbvm.CopyItem.Id == copyItemId) {
                    return rtbvm;
                }
            }
            return null;
        }

        public MpContentItemViewModel GetContentItemByCopyItemId(int ciid) {
            return ItemViewModels.Where(x => x.CopyItem.Id == ciid).FirstOrDefault();
        }

        public string GetDetailText(MpCopyItemDetailType detailType) {
            return HeadItem.GetDetail(detailType);
        }

        public void InsertRange(int idx, List<MpCopyItem> models) {
            idx = idx < 0 ? 0 : idx >= ItemViewModels.Count ? ItemViewModels.Count : idx;
            foreach (var ci in models) {
                switch (ci.ItemType) {
                    case MpCopyItemType.RichText:
                        ItemViewModels.Insert(idx, new MpContentItemViewModel(this, ci));
                        break;
                }
            }
        }


        public void RemoveRange(List<MpCopyItem> models) {
            for (int i = 0; i < models.Count; i++) {
                var ivm = ItemViewModels.Where(x => x.CopyItem.Id == models[i].Id).FirstOrDefault();
                if (ivm != null) {
                    ItemViewModels.Remove(ivm);
                }
            }
        }

        public void DeleteAll() {
            foreach (var ivm in ItemViewModels) {
                ivm.RemoveFromDatabase();
            }
        }

        #endregion
        //public abstract string GetItemRtf();
        //public abstract string GetItemPlainText();
        //public abstract string GetItemQuillHtml();
        //public abstract string[] GetItemFileList();

        #region Private Methods
        private void MpContentContainerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                // case nameof(SubSele)
            }
        }
        #endregion

        #region Commands
        //private RelayCommand _createQrCodeFromClipCommand;
        //public ICommand CreateQrCodeFromClipCommand {
        //    get {
        //        if (_createQrCodeFromClipCommand == null) {
        //            _createQrCodeFromClipCommand = new RelayCommand(CreateQrCodeFromClip, CanCreateQrCodeFromClip);
        //        }
        //        return _createQrCodeFromClipCommand;
        //    }
        //}
        //private bool CanCreateQrCodeFromClip() {
        //    return CopyItemType == MpCopyItemType.RichText && CopyItemPlainText.Length <= Properties.Settings.Default.MaxQrCodeCharLength;
        //}
        //private void CreateQrCodeFromClip() {
        //    var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(CopyItemPlainText);
        //    System.Windows.Clipboard.SetImage(bmpSrc);
        //}

        //private RelayCommand _sendClipToEmailCommand;
        //public ICommand SendClipToEmailCommand {
        //    get {
        //        if (_sendClipToEmailCommand == null) {
        //            _sendClipToEmailCommand = new RelayCommand(SendClipToEmail, CanSendClipToEmail);
        //        }
        //        return _sendClipToEmailCommand;
        //    }
        //}
        //private bool CanSendClipToEmail() {
        //    return !IsEditingContent;
        //}
        //private void SendClipToEmail() {
        //    MpHelpers.Instance.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, CopyItemTitle, CopyItemPlainText));
        //    //MpClipTrayViewModel.Instance.ClearClipSelection();
        //    //IsSelected = true;
        //    //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        //}




        //private RelayCommand _excludeApplicationCommand;
        //public ICommand ExcludeApplicationCommand {
        //    get {
        //        if (_excludeApplicationCommand == null) {
        //            _excludeApplicationCommand = new RelayCommand(ExcludeApplication, CanExcludeApplication);
        //        }
        //        return _excludeApplicationCommand;
        //    }
        //}
        //private bool CanExcludeApplication() {
        //    return MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1;
        //}
        //private void ExcludeApplication() {
        //    MpAppCollectionViewModel.Instance.UpdateRejection(MpAppCollectionViewModel.Instance.GetAppViewModelByAppId(CopyItemAppId), true);
        //}

        //private RelayCommand<object> _pasteClipCommand;
        //public ICommand PasteClipCommand {
        //    get {
        //        if (_pasteClipCommand == null) {
        //            _pasteClipCommand = new RelayCommand<object>(PasteClip);
        //        }
        //        return _pasteClipCommand;
        //    }
        //}
        //private void PasteClip(object args) {
        //    MpClipTrayViewModel.Instance.ClearClipSelection();
        //    IsSelected = true;
        //    MpClipTrayViewModel.Instance.PasteSelectedClipsCommand.Execute(args);
        //}

        //private RelayCommand _assignHotkeyCommand;
        //public ICommand AssignHotkeyCommand {
        //    get {
        //        if (_assignHotkeyCommand == null) {
        //            _assignHotkeyCommand = new RelayCommand(AssignHotkey);
        //        }
        //        return _assignHotkeyCommand;
        //    }
        //}
        //private void AssignHotkey() {
        //    ShortcutKeyString = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(
        //        this,
        //        "Paste " + CopyItemTitle,
        //        ShortcutKeyString,
        //        MpClipTrayViewModel.Instance.HotkeyPasteCommand, CopyItemId);
        //}
        #endregion

        #region Commands - From Content Container
        private RelayCommand _editTitleCommand;
        public ICommand EditTitleCommand {
            get {
                if (_editTitleCommand == null) {
                    _editTitleCommand = new RelayCommand(EditTitle, CanEditTitle);
                }
                return _editTitleCommand;
            }
        }
        private bool CanEditTitle() {
            if (MpMainWindowViewModel.IsMainWindowLoading) {
                return false;
            }
            return MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1 &&
                   SelectedItems.Count <= 1;
        }
        private void EditTitle() {
            if (SelectedItems.Count == 0) {
                IsEditingTitle = !IsEditingTitle;
            } else {
                SelectedItems[0].EditSubTitleCommand.Execute(null);
            }
        }

        private RelayCommand _editContentCommand;
        public ICommand EditContentCommand {
            get {
                if (_editContentCommand == null) {
                    _editContentCommand = new RelayCommand(EditContent, CanEditContent);
                }
                return _editContentCommand;
            }
        }
        private bool CanEditContent() {
            if (MpMainWindowViewModel.IsMainWindowLoading) {
                return false;
            }
            return MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1 &&
                   MpClipTrayViewModel.Instance.SelectedClipTiles[0].IsTextItem;
        }
        private void EditContent() {
            if (!IsEditingContent) {
                IsEditingContent = true;
            }
            if (SelectedItems.Count == 0) {
                ItemViewModels[0].IsSelected = true;
            }
            SelectedItems[0].EditSubContentCommand.Execute(null);
        }


        private RelayCommand<object> _toggleEditSubSelectedItemCommand;
        public ICommand ToggleEditSubSelectedItemCommand {
            get {
                if (_toggleEditSubSelectedItemCommand == null) {
                    _toggleEditSubSelectedItemCommand = new RelayCommand<object>(ToggleEditSubSelectedItem, CanToggleEditSubSelectedItem);
                }
                return _toggleEditSubSelectedItemCommand;
            }
        }
        private bool CanToggleEditSubSelectedItem(object args) {
            if (MpMainWindowViewModel.IsMainWindowLoading) {
                return false;
            }
            return MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1 &&
                   SelectedItems.Count == 1;
        }
        private void ToggleEditSubSelectedItem(object args) {
            var selectedRtbvm = SelectedItems[0];
            if (!IsEditingContent) {
                IsEditingContent = true;
            }
            selectedRtbvm.IsSelected = true;
        }

        private RelayCommand _selectNextItemCommand;
        public ICommand SelectNextItemCommand {
            get {
                if (_selectNextItemCommand == null) {
                    _selectNextItemCommand = new RelayCommand(SelectNextItem, CanSelectNextItem);
                }
                return _selectNextItemCommand;
            }
        }
        private bool CanSelectNextItem() {
            return SelectedItems.Count > 0 &&
                   SelectedItems.Any(x => VisibleItems.IndexOf(x) != VisibleItems.Count - 1);
        }
        private void SelectNextItem() {
            var maxItem = SelectedItems.Max(x => VisibleItems.IndexOf(x));
            ClearSubSelection();
            VisibleItems[maxItem + 1].IsSelected = true;
        }

        private RelayCommand _selectPreviousItemCommand;
        public ICommand SelectPreviousItemCommand {
            get {
                if (_selectPreviousItemCommand == null) {
                    _selectPreviousItemCommand = new RelayCommand(SelectPreviousItem, CanSelectPreviousItem);
                }
                return _selectPreviousItemCommand;
            }
        }
        private bool CanSelectPreviousItem() {
            return SelectedItems.Count > 0 && SelectedItems.Any(x => VisibleItems.IndexOf(x) != 0);
        }
        private void SelectPreviousItem() {
            var minItem = SelectedItems.Min(x => VisibleItems.IndexOf(x));
            ClearSubSelection();
            VisibleItems[minItem - 1].IsSelected = true;
        }

        private RelayCommand _selectAllCommand;
        public ICommand SelectAllCommand {
            get {
                if (_selectAllCommand == null) {
                    _selectAllCommand = new RelayCommand(SelectAll);
                }
                return _selectAllCommand;
            }
        }
        private void SelectAll() {
            ClearSubSelection();
            foreach (var ctvm in VisibleItems) {
                ctvm.IsSelected = true;
            }
        }

        private AsyncCommand<Brush> _changeSubSelectedClipsColorCommand;
        public IAsyncCommand<Brush> ChangeSubSelectedClipsColorCommand {
            get {
                if (_changeSubSelectedClipsColorCommand == null) {
                    _changeSubSelectedClipsColorCommand = new AsyncCommand<Brush>(ChangeSubSelectedClipsColor);
                }
                return _changeSubSelectedClipsColorCommand;
            }
        }
        private async Task ChangeSubSelectedClipsColor(Brush brush) {
            if (brush == null) {
                return;
            }
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            foreach (var sctvm in SelectedItems) {
                                sctvm.CopyItem.ItemColor = MpHelpers.Instance.ConvertColorToHex((brush as SolidColorBrush).Color);
                            }
                        }));
            }
            finally {
                IsBusy = false;
            }
        }

        private RelayCommand<object> _pasteSubSelectedClipsCommand;
        public ICommand PasteSubSelectedClipsCommand {
            get {
                if (_pasteSubSelectedClipsCommand == null) {
                    _pasteSubSelectedClipsCommand = new RelayCommand<object>(PasteSubSelectedClips, CanPasteSubSelectedClips);
                }
                return _pasteSubSelectedClipsCommand;
            }
        }
        private bool CanPasteSubSelectedClips(object ptapId) {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsAnyEditingContent &&
                !IsAnyEditingTitle &&
                !IsAnyPastingTemplate &&
                !IsTrialExpired;
        }
        private void PasteSubSelectedClips(object ptapId) {
            if (ptapId != null && ptapId.GetType() == typeof(int) && (int)ptapId > 0) {
                //when pasting to a user defined application
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = MpPasteToAppPathViewModelCollection.Instance.Where(x => x.PasteToAppPathId == (int)ptapId).ToList()[0];
            } else if (ptapId != null && ptapId.GetType() == typeof(IntPtr) && (IntPtr)ptapId != IntPtr.Zero) {
                //when pasting to a running application
                _selectedPasteToAppPathWindowHandle = (IntPtr)ptapId;
                _selectedPasteToAppPathViewModel = null;
            } else {
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = null;
            }
            //In order to paste the app must hide first 
            //this triggers hidewindow to paste selected items
            MainWindowViewModel.HideWindowCommand.Execute(true);
        }

        private AsyncCommand _bringSubSelectedClipTilesToFrontCommand;
        public IAsyncCommand BringSubSelectedClipTilesToFrontCommand {
            get {
                if (_bringSubSelectedClipTilesToFrontCommand == null) {
                    _bringSubSelectedClipTilesToFrontCommand = new AsyncCommand(BringSubSelectedClipTilesToFront, CanBringSubSelectedClipTilesToFront);
                }
                return _bringSubSelectedClipTilesToFrontCommand;
            }
        }
        private bool CanBringSubSelectedClipTilesToFront(object arg) {
            if (IsBusy || MpMainWindowViewModel.IsMainWindowLoading || VisibleItems.Count == 0) {
                return false;
            }
            bool canBringForward = false;
            for (int i = 0; i < SelectedItems.Count && i < VisibleItems.Count; i++) {
                if (!SelectedItems.Contains(VisibleItems[i])) {
                    canBringForward = true;
                    break;
                }
            }
            return canBringForward;
        }
        private async Task BringSubSelectedClipTilesToFront() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SelectedItems;
                            ClearSubSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                ItemViewModels.Move(ItemViewModels.IndexOf(sctvm), 0);
                                sctvm.IsSelected = true;
                            }
                            RequestScrollIntoView(SelectedItems[0]);
                        }));
            }
            finally {
                IsBusy = false;
            }
        }

        private AsyncCommand _sendSubSelectedClipTilesToBackCommand;
        public IAsyncCommand SendSubSelectedClipTilesToBackCommand {
            get {
                if (_sendSubSelectedClipTilesToBackCommand == null) {
                    _sendSubSelectedClipTilesToBackCommand = new AsyncCommand(SendSubSelectedClipTilesToBack, CanSendSubSelectedClipTilesToBack);
                }
                return _sendSubSelectedClipTilesToBackCommand;
            }
        }
        private bool CanSendSubSelectedClipTilesToBack(object args) {
            if (IsBusy || MpMainWindowViewModel.IsMainWindowLoading || VisibleItems.Count == 0) {
                return false;
            }
            bool canSendBack = false;
            for (int i = 0; i < SelectedItems.Count && i < VisibleItems.Count; i++) {
                if (!SelectedItems.Contains(VisibleItems[VisibleItems.Count - 1 - i])) {
                    canSendBack = true;
                    break;
                }
            }
            return canSendBack;
        }
        private async Task SendSubSelectedClipTilesToBack() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SelectedItems;
                            ClearSubSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                ItemViewModels.Move(ItemViewModels.IndexOf(sctvm), ItemViewModels.Count - 1);
                                sctvm.IsSelected = true;
                            }
                            RequestScrollIntoView(SelectedItems[SelectedItems.Count - 1]);
                        }));
            }
            finally {
                IsBusy = false;
            }
        }


        private RelayCommand<object> _searchWebCommand;
        public ICommand SearchWebCommand {
            get {
                if (_searchWebCommand == null) {
                    _searchWebCommand = new RelayCommand<object>(SearchWeb);
                }
                return _searchWebCommand;
            }
        }
        private void SearchWeb(object args) {
            if (args == null || args.GetType() != typeof(string)) {
                return;
            }
            MpHelpers.Instance.OpenUrl(args.ToString() + System.Uri.EscapeDataString(HeadItem.CopyItem.ItemData.ToPlainText()));
        }

        private RelayCommand _deleteSubSelectedClipsCommand;
        public ICommand DeleteSubSelectedClipsCommand {
            get {
                if (_deleteSubSelectedClipsCommand == null) {
                    _deleteSubSelectedClipsCommand = new RelayCommand(DeleteSubSelectedClips, CanDeleteSubSelectedClips);
                }
                return _deleteSubSelectedClipsCommand;
            }
        }
        private bool CanDeleteSubSelectedClips() {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsAnyEditingContent &&
                !IsAnyEditingTitle &&
                !IsAnyPastingTemplate;
        }
        private void DeleteSubSelectedClips() {
            int lastSelectedClipTileIdx = -1;
            foreach (var ct in SelectedItems) {
                lastSelectedClipTileIdx = VisibleItems.IndexOf(ct);
                ItemViewModels.Remove(ct);
            }
            ClearSubSelection();
            if (VisibleItems.Count > 0) {
                if (lastSelectedClipTileIdx <= 0) {
                    VisibleItems[0].IsSelected = true;
                } else if (lastSelectedClipTileIdx < VisibleItems.Count) {
                    VisibleItems[lastSelectedClipTileIdx].IsSelected = true;
                } else {
                    VisibleItems[lastSelectedClipTileIdx - 1].IsSelected = true;
                }
            }
        }

        private RelayCommand<MpTagTileViewModel> _linkTagToSubSelectedClipsCommand;
        public ICommand LinkTagToSubSelectedClipsCommand {
            get {
                if (_linkTagToSubSelectedClipsCommand == null) {
                    _linkTagToSubSelectedClipsCommand = new RelayCommand<MpTagTileViewModel>(LinkTagToSubSelectedClips, CanLinkTagToSubSelectedClips);
                }
                return _linkTagToSubSelectedClipsCommand;
            }
        }
        private bool CanLinkTagToSubSelectedClips(MpTagTileViewModel tagToLink) {
            //this checks the selected clips association with tagToLink
            //and only returns if ALL selecteds clips are linked or unlinked 
            if (tagToLink == null || SelectedItems == null || SelectedItems.Count == 0) {
                return false;
            }
            if (SelectedItems.Count == 1) {
                return true;
            }
            bool isLastClipTileLinked = tagToLink.IsLinked(SelectedItems[0].CopyItem);
            foreach (var srtbvm in SelectedItems) {
                if (tagToLink.IsLinked(srtbvm) != isLastClipTileLinked) {
                    return false;
                }
            }
            return true;
        }
        private void LinkTagToSubSelectedClips(MpTagTileViewModel tagToLink) {
            bool isUnlink = tagToLink.IsLinked(SelectedItems[0].CopyItem);
            foreach (var srtbvm in SelectedItems) {
                if (isUnlink) {
                    tagToLink.RemoveClip(srtbvm);
                } else {
                    tagToLink.AddClip(srtbvm);
                }
            }
            MainWindowViewModel.TagTrayViewModel.RefreshAllCounts();
            MainWindowViewModel.TagTrayViewModel.UpdateTagAssociation();
        }

        private RelayCommand _assignHotkeyCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if (_assignHotkeyCommand == null) {
                    _assignHotkeyCommand = new RelayCommand(AssignHotkey, CanAssignHotkey);
                }
                return _assignHotkeyCommand;
            }
        }
        private bool CanAssignHotkey() {
            return SelectedItems.Count == 1;
        }
        private void AssignHotkey() {
            SelectedItems[0].AssignHotkeyCommand.Execute(null);
        }

        private RelayCommand _invertSubSelectionCommand;
        public ICommand InvertSubSelectionCommand {
            get {
                if (_invertSubSelectionCommand == null) {
                    _invertSubSelectionCommand = new RelayCommand(InvertSubSelection, CanSubInvertSelection);
                }
                return _invertSubSelectionCommand;
            }
        }
        private bool CanSubInvertSelection() {
            return SelectedItems.Count != VisibleItems.Count;
        }
        private void InvertSubSelection() {
            var sctvml = SelectedItems;
            ClearSubSelection();
            foreach (var vctvm in VisibleItems) {
                if (!sctvml.Contains(vctvm)) {
                    vctvm.IsSelected = true;
                }
            }
        }

        private AsyncCommand _speakSubSelectedClipsAsyncCommand;
        public IAsyncCommand SpeakSubSelectedClipsAsyncCommand {
            get {
                if (_speakSubSelectedClipsAsyncCommand == null) {
                    _speakSubSelectedClipsAsyncCommand = new AsyncCommand(SpeakSubSelectedClipsAsync, CanSpeakSubSelectedClipsAsync);
                }
                return _speakSubSelectedClipsAsyncCommand;
            }
        }
        private bool CanSpeakSubSelectedClipsAsync(object args) {
            foreach (var sctvm in SelectedItems) {
                if (!string.IsNullOrEmpty(sctvm.CopyItem.ItemData.ToPlainText())) {
                    return true;
                }
            }
            return false;
        }
        private async Task SpeakSubSelectedClipsAsync() {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                var speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.SetOutputToDefaultAudioDevice();
                string voiceName = speechSynthesizer.GetInstalledVoices()[3].VoiceInfo.Name;
                if (!string.IsNullOrEmpty(Properties.Settings.Default.SpeechSynthVoiceName)) {
                    var voice = speechSynthesizer.GetInstalledVoices().Where(x => x.VoiceInfo.Name.ToLower().Contains(Properties.Settings.Default.SpeechSynthVoiceName.ToLower())).FirstOrDefault();
                    if (voice != null) {
                        voiceName = voice.VoiceInfo.Name;
                    }
                }
                speechSynthesizer.SelectVoice(voiceName);

                speechSynthesizer.Rate = 0;
                speechSynthesizer.SpeakCompleted += (s, e) => {
                    speechSynthesizer.Dispose();
                };
                // Create a PromptBuilder object and append a text string.
                PromptBuilder promptBuilder = new PromptBuilder();

                foreach (var sctvm in SelectedItems) {
                    //speechSynthesizer.SpeakAsync(sctvm.CopyItemPlainText);
                    promptBuilder.AppendText(Environment.NewLine + sctvm.CopyItem.ItemData.ToPlainText());
                }

                // Speak the contents of the prompt asynchronously.
                speechSynthesizer.SpeakAsync(promptBuilder);

            }, DispatcherPriority.Background);
        }

        private RelayCommand _duplicateSubSelectedClipsCommand;
        public ICommand DuplicateSubSelectedClipsCommand {
            get {
                if (_duplicateSubSelectedClipsCommand == null) {
                    _duplicateSubSelectedClipsCommand = new RelayCommand(DuplicateSubSelectedClips);
                }
                return _duplicateSubSelectedClipsCommand;
            }
        }
        private void DuplicateSubSelectedClips() {
            var tempSubSelectedRtbvml = SelectedItems;
            ClearSubSelection();
            foreach (var srtbvm in tempSubSelectedRtbvml) {
                var clonedCopyItem = (MpCopyItem)srtbvm.CopyItem.Clone();
                clonedCopyItem.WriteToDatabase();
                var rtbvm = new MpContentItemViewModel(this, clonedCopyItem);
                //MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
                ItemViewModels.Add(rtbvm);
                rtbvm.IsSelected = true;
            }
        }
        #endregion
        #endregion
    }
}
