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

    public class MpClipTileViewModel : MpUndoableViewModelBase<MpClipTileViewModel> {
        #region Private Variables
        private int _detailIdx = 1;
        private List<string> _tempFileList = new List<string>();
        #endregion

        #region Statics
        //public static MpClipTileViewModel CreateEmptyClipTile() {
        //    var eci = new MpCopyItem(MpCopyItemType.RichText, @"Empty Tile", MpHelpers.Instance.GetThisAppHandle());
        //    eci.CopyItemId = -1;
        //    eci.CopyDateTime = DateTime.MinValue;
        //    var ectvm = new MpClipTileViewModel(eci);
        //    ectvm.TileVisibility = Visibility.Collapsed;
        //    ectvm.IsPlaceholder = true;
        //    return ectvm;
        //}
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

        private MpContentContainerViewModel _contentContainerViewModel;
        public MpContentContainerViewModel ContentContainerViewModel { 
            get {
                return _contentContainerViewModel;
            }
            private set {
                if(_contentContainerViewModel != value) {
                    _contentContainerViewModel = value;
                    OnPropertyChanged(nameof(ContentContainerViewModel));
                }
            }
        }


        private ObservableCollection<MpContextMenuItemViewModel> _tagMenuItems = new ObservableCollection<MpContextMenuItemViewModel>();
        public ObservableCollection<MpContextMenuItemViewModel> TagMenuItems {
            get {
                if(MainWindowViewModel == null || MainWindowViewModel.TagTrayViewModel == null) {
                    return new ObservableCollection<MpContextMenuItemViewModel>();
                }
                _tagMenuItems.Clear();
                foreach (var tagTile in MpTagTrayViewModel.Instance.TagTileViewModels) {
                    if (tagTile.IsSudoTag) {
                        continue;
                    }
                    bool isTagAssociated = tagTile.IsLinked(this);
                    string checkIconPath = string.Empty;
                    if (isTagAssociated) {
                        checkIconPath = Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/minus.png";
                    }
                    _tagMenuItems.Add(
                        new MpContextMenuItemViewModel(
                            tagTile.TagName,
                            MpClipTrayViewModel.Instance.LinkTagToCopyItemCommand,
                            tagTile,
                            isTagAssociated,
                            checkIconPath,
                            null,
                            tagTile.ShortcutKeyString,
                            tagTile.Color));
                }
                return _tagMenuItems;
            }
        }


        private MpFileListItemCollectionViewModel _fileListCollectionViewModel = null;
        public MpFileListItemCollectionViewModel FileListCollectionViewModel {
            get {
                if (CopyItem == null || CopyItemType != MpCopyItemType.FileList) {
                    return new MpFileListItemCollectionViewModel(this);
                }
                if (_fileListCollectionViewModel == null) {
                    _fileListCollectionViewModel = new MpFileListItemCollectionViewModel(this);
                    foreach (var path in ContentContainerViewModel.FileList) {
                        _fileListCollectionViewModel.Add(new MpFileListItemViewModel(this, path));
                    }
                }
                return _fileListCollectionViewModel;
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
        public BitmapSource HotkeyIconSource {
            get {
                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                    return new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/joystick.png"));
                }
                return new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/joystickactive.png"));
            }
        }

        public string HotkeyIconTooltip {
            get {
                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                    return @"Assign Shortcut";
                }
                return ShortcutKeyString;
            }
        }

        //public BitmapSource AppIcon {
        //    get {
        //        if (MainWindowViewModel == null ||
        //           MpClipTrayViewModel.Instance == null ||
        //          !MpClipTrayViewModel.Instance.IsFilteringByApp) {
        //            if (CopyItem == null) {
        //                return new BitmapImage();
        //            }
        //            if (CopyItemFavIcon != null) {
        //                return CopyItemFavIcon;
        //            }
        //            if (RichTextBoxViewModelCollection.Count == 1 && RichTextBoxViewModelCollection[0].CopyItemFavIcon != null) {
        //                return RichTextBoxViewModelCollection[0].CopyItemFavIcon;
        //            }
        //            return CopyItemAppIcon;
        //        }
        //        return MpClipTrayViewModel.Instance.FilterByAppIcon;
        //    }
        //}
        #endregion

        #region Layout

       // public Rect TileRect {
            //get {
                //return MpClipTrayViewModel.Instance.ClipTileViewModels.GetListBoxItemRect(MpClipTrayViewModel.Instance.ClipTileViewModels.IndexOf(this));
                //if(ClipBorder == null || ClipBorder.Parent == null) {
                //    return new Rect(new Point(double.MinValue,double.MinValue),new Size());
                //}
                //return ClipBorder.TransformToAncestor((Visual)ClipBorder.Parent).TransformBounds(LayoutInformation.GetLayoutSlot(ClipBorder));

                //return ClipBorder.TransformToVisual(MpClipTrayViewModel.Instance.ListBox).TransformBounds(LayoutInformation.GetLayoutSlot(ClipBorder));

                //if (ClipBorder == null ||
                //    MpClipTrayViewModel.Instance.ListBox == null ||
                //    !ClipBorder.IsVisualDescendant(MpClipTrayViewModel.Instance.ListBox)) {
                //    return new Rect(new Point(double.MinValue, double.MinValue), new Size(0, 0));
                //}
                //try {
                //    return ClipBorder.TransformToVisual(MpClipTrayViewModel.Instance.ListBox).TransformBounds(LayoutInformation.GetLayoutSlot(ClipBorder));

                //}
                //catch (Exception ex) {
                //    MonkeyPaste.MpConsole.WriteLine("ItemRect error: " + ex);
                //    return new Rect(new Point(double.MinValue, double.MinValue), new Size(0, 0));
                //}

                //var idx = MpClipTrayViewModel.Instance.VisibileClipTiles.IndexOf(this);
                //if (idx >= 0) {
                //    double w = TileBorderWidth + (TileMargin * 2 ) + (TileBorderThickness);
                //    double h = TileBorderHeight + (TileMargin) + (TileBorderThickness);
                //    return new Rect(idx * w, 0, (idx + 1) * w, h);
                //}
                //return new Rect();
           // }
        //}

        

        public double PasteTemplateToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
            }
        }


        public double TileTitleIconSize {
            get {
                return MpMeasurements.Instance.ClipTileTitleIconSize;
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
                if (ContentContainerViewModel == null) {
                    return MpMeasurements.Instance.ClipTileBorderMinMaxSize;
                }

                var ds = ContentContainerViewModel.TotalExpandedSize;
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
                if (ContentContainerViewModel == null) {
                    return 0;
                }
                return Math.Max(MpMeasurements.Instance.ClipTileEditModeMinWidth, ContentContainerViewModel.TotalExpandedSize.Width);
            }
        }

        public double TitleFontSize {
            get {
                return MpMeasurements.Instance.ClipTileTitleFontSize;
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
                if (CopyItem == null || !Properties.Settings.Default.ShowItemPreview) {
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
                if (CopyItemType != MpCopyItemType.FileList) {
                    return Visibility.Collapsed;
                }
                if (FileListCollectionViewModel.Count == 1 &&
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
                if (!IsLoading && CopyItemType == MpCopyItemType.RichText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
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
                if (IsEditingTile || IsEditingTemplate || IsPastingTemplate) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public Visibility TileVisibility {
            get {
                return ItemVisibility;
            }
            set {
                if (ItemVisibility != value) {
                    ItemVisibility = value;
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

        #region Business Logic
        public bool HasTemplate {
            get {
                return ContentContainerViewModel.IsDynamicPaste;
            }
        }

        public bool IsStubItem {
            get {
                return CopyItemId == 0 && TileVisibility != Visibility.Visible;
            }
        }

        public bool IsTextItem {
            get {
                return CopyItemType == MpCopyItemType.RichText;
            }
        }

        private IDataObject _dragDataObject = null;
        public IDataObject DragDataObject {
            get {
                return _dragDataObject;
            }
            set {
                if (_dragDataObject != value) {
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
                if (_wasAddedAtRuntime != value) {
                    _wasAddedAtRuntime = value;
                    OnPropertyChanged(nameof(WasAddedAtRuntime));
                }
            }
        }

        public new bool IsLoading {
            get {
                return CopyItem == null || CopyItem.Id == 0;
            }
        }

        public int SortOrderIdx {
            get {
                if (MpClipTrayViewModel.Instance == null || TileVisibility != Visibility.Visible) {
                    return -1;
                }
                return MpClipTrayViewModel.Instance.VisibileClipTiles.IndexOf(this);
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
                if (CopyItem == null ||
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
                return IsClipDragging || ContentContainerViewModel.ItemViewModels.Any(x => x.IsSubDragging);
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
                if (ClipBorder == null) {
                    return false;
                }
                if (IsPastingTemplate || IsEditingTemplate || IsEditingTile) {
                    return true;
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
                if (_lastSelectedDateTime != value) {
                    _lastSelectedDateTime = value;
                    OnPropertyChanged(nameof(LastSelectedDateTime));
                }
            }
        }

        public bool IsAnySubContextMenuOpened {
            get {
                return ContentContainerViewModel.IsAnySubContextMenuOpened;
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
                if (CopyItem == null) {
                    return string.Empty;
                }
                _detailIdx++;
                if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    _detailIdx = 1;
                }
                // TODO this should aggregate details over all sub items 
                return ContentContainerViewModel.HeadItem.GetDetail((MpCopyItemDetailType)_detailIdx);
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
                if (CopyItem != null && CopyItem.CopyCount != value) {
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
                if (CopyItem != null && CopyItem.PasteCount != value) {
                    CopyItem.PasteCount = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(PasteCount));
                }
            }
        }

        public int LineCount { get; set; }

        public int CharCount { get; set; }

        public int FileCount { get; set; }

        public double DataSizeInMb { get; set; }

        public Brush TitleBackgroundColor {
            get {
                if (CopyItem == null) {
                    return Brushes.Transparent;
                }
                return new SolidColorBrush(MpHelpers.Instance.ConvertHexToColor(CopyItem.ItemColor));
            }
            set {
                if (CopyItem != null && CopyItem.ItemColor != MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color)) {
                    CopyItem.ItemColor = MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color);
                    CopyItem.WriteToDatabase();
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

        private string _shortcutKeyString = string.Empty;
        public string ShortcutKeyString {
            get {
                return _shortcutKeyString;
            }
            set {
                if (_shortcutKeyString != value) {
                    _shortcutKeyString = value;
                    OnPropertyChanged(nameof(ShortcutKeyString));
                    OnPropertyChanged(nameof(HotkeyIconSource));
                    OnPropertyChanged(nameof(HotkeyIconTooltip));
                }
            }
        }

        public int RelevanceScore {
            get {
                if(CopyItem == null) {
                    return 0;
                }
                return CopyCount + PasteCount;
            }
        }


        public int CopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.Id;
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
                if (CopyItem == null || CopyItem.ItemData == null) {
                    return string.Empty;
                }
                return CopyItem.ItemData.ToPlainText();
            }
            set {
                if (CopyItem != null && CopyItem.ItemData != value) {
                    CopyItem.ItemData = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemPlainText));
                }
            }
        }

        public string CopyItemDescription {
            get {
                if (CopyItem == null || CopyItem.ItemDescription == null) {
                    return string.Empty;
                }
                return CopyItem.ItemDescription;
            }
            set {
                if (CopyItem != null && CopyItem.ItemDescription != value) {
                    CopyItem.ItemDescription = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemDescription));
                }
            }
        }

        public string CopyItemRichText {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(CopyItem.ItemData)) {
                    return string.Empty.ToRichText();
                }
                return CopyItem.ItemData.ToRichText();
            }
            set {
                if (CopyItem != null && CopyItem.ItemData != value) {
                    //value should be raw rtf where templates are encoded into #name#color# groups
                    CopyItem.ItemData = value;
                    CopyItem.WriteToDatabase();
                    //OnPropertyChanged(nameof(CopyItemRichText));
                    //OnPropertyChanged(nameof(CharCount));
                    //OnPropertyChanged(nameof(LineCount));
                    OnPropertyChanged(nameof(CopyItemRichText));
                }
            }
        }

        //public BitmapSource CopyItemBmp {
        //    get {
        //        if (CopyItem == null) {
        //            return new BitmapImage();
        //        }
        //        return CopyItem.ItemData.ToBitmapSource(); ;
        //    }
        //    set {
        //        if (CopyItem.ItemData != value.ToBase64String()) {
        //            CopyItem.ItemData = value.ToBase64String();
        //            CopyItem.WriteToDatabase();
        //            OnPropertyChanged(nameof(CopyItemBmp));
        //        }
        //    }
        //}

        //public BitmapSource CopyItemAppIcon {
        //    get {
        //        if (CopyItem == null) {
        //            return new BitmapImage();
        //        }
        //        return CopyItem.Source.App.Icon.IconImage.ImageBase64.ToBitmapSource();
        //    }
        //}

        //public BitmapSource CopyItemAppIconHighlightBorder {
        //    get {
        //        if (CopyItem == null) {
        //            return new BitmapImage();
        //        }
        //        OnPropertyChanged(nameof(AppIconHighlightBorderVisibility));
        //        if (HighlightTextRangeViewModelCollection.HasAppMatch) {
        //            if (HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel != null &&
        //               HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.HighlightType == MpHighlightType.App &&
        //               (RichTextBoxViewModelCollection.Count == 1 || HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.RtbItemViewModel == null)) {
        //                if (CopyItemFavIcon != null) {
        //                    return CopyItemUrlDomain.FavIcon.IconBorderHighlightSelectedImage.ImageBase64.ToBitmapSource();
        //                }
        //                return CopyItem.Source.App.Icon.IconBorderHighlightSelectedImage.ImageBase64.ToBitmapSource();
        //            }
        //            if (CopyItemFavIcon != null) {
        //                return CopyItemUrlDomain.FavIcon.IconBorderHighlightImage.ImageBase64.ToBitmapSource();
        //            }
        //            return CopyItem.Source.App.Icon.IconBorderHighlightImage.ImageBase64.ToBitmapSource();
        //        }
        //        if (CopyItemFavIcon != null) {
        //            return CopyItemUrlDomain.FavIcon.IconBorderHighlightImage.ImageBase64.ToBitmapSource();
        //        }
        //        return CopyItem.Source.App.Icon.IconBorderHighlightImage.ImageBase64.ToBitmapSource();
        //    }
        //}

        //public BitmapSource CopyItemAppIconBorder {
        //    get {
        //        if (CopyItem == null) {
        //            return new BitmapImage();
        //        }
        //        if (CopyItemFavIcon != null) {
        //            return CopyItemUrlDomain.FavIcon.IconBorderImage.ImageBase64.ToBitmapSource();
        //        }
        //        return CopyItem.Source.App.Icon.IconBorderImage.ImageBase64.ToBitmapSource();
        //    }
        //}

        public string CopyItemAppName {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Source.App.AppName;
            }
        }

        public string CopyItemAppPath {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Source.App.AppPath;
            }
        }

        //public MpUrlDomain CopyItemUrlDomain {
        //    get {
        //        if (CopyItemUrl == null) {
        //            return null;
        //        }
        //        return CopyItemUrl.UrlDomain;
        //    }
        //    set {
        //        if (CopyItemUrl != null && CopyItemUrl.UrlDomain != value) {
        //            CopyItemUrl.UrlDomain = value;
        //            CopyItemUrl.UrlDomain.WriteToDatabase();
        //            OnPropertyChanged(nameof(CopyItemUrlDomain));
        //        }
        //    }
        //}

        //public MpUrl CopyItemUrl {
        //    get {
        //        if (CopyItem == null || CopyItem.Source == null || CopyItem.Source.Url == null) {
        //            return null;
        //        }
        //        return CopyItem.Source.Url;
        //    }
        //    set {
        //        if (CopyItem != null && CopyItem.Source.Url != value) {
        //            CopyItem.Source.Url = value;
        //            CopyItemUrlDomain = CopyItemUrl.UrlDomain;
        //            CopyItem.Source.Url.WriteToDatabase();
        //            CopyItem.WriteToDatabase();
        //            OnPropertyChanged(nameof(CopyItemUrl));
        //            OnPropertyChanged(nameof(CopyItemUrlDomain));
        //            OnPropertyChanged(nameof(CopyItemFavIcon));
        //            OnPropertyChanged(nameof(AppIcon));
        //            OnPropertyChanged(nameof(TileTitleIconSize));
        //            OnPropertyChanged(nameof(TileTitleIconBorderSize));
        //        }
        //    }
        //}

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
                    OnPropertyChanged(nameof(CopyDateTime));
                }
            }
        }
        public int CopyItemUsageScore {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return RelevanceScore;
            }
        }

        public int CopyItemAppId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.Source.App.Id;
            }
        }

        public MpCopyItemType CopyItemType {
            get {
                if (CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.ItemType;
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

        public MpCopyItem CopyItem {
            get {
                if(ContentContainerViewModel != null && ContentContainerViewModel.ItemViewModels.Count > 0) {
                    return ContentContainerViewModel.HeadItem.CopyItem;
                }
                return null;
            }
           // set {
                //if (_copyItem != value) 
               // {
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


                    //bool updateVms = false;
                    //if(_copyItem != value && _copyItem != null && value != null) {
                    //    updateVms = true;
                    //}
                    //_copyItem = value;
                    //if (CopyItem != null && _wasAddedAtRuntime) {
                    //    CopyItem.WriteToDatabase();
                    //}

                   // OnPropertyChanged(nameof(CopyItem));

                    //if(updateVms) {
                    //    TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);
                    //    RichTextBoxViewModelCollection = new MpClipTileRichTextBoxViewModelCollection(this);
                    //    EditRichTextBoxToolbarViewModel = new MpEditRichTextBoxToolbarViewModel(this);
                    //    EditTemplateToolbarViewModel = new MpEditTemplateToolbarViewModel(this);
                    //    PasteTemplateToolbarViewModel = new MpPasteTemplateToolbarViewModel(this);
                    //    HighlightTextRangeViewModelCollection = new MpHighlightTextRangeViewModelCollection(this);
                    //}
               // }
            //}
        }
        #endregion

        #endregion

        #region Events
        public event EventHandler OnTileSelected;
        public event EventHandler OnTileExpand;
        public event EventHandler OnTileUnexpand;
        #endregion

        #region Public Methods
        public MpClipTileViewModel() : base() {
            PropertyChanged += (s, e1) => {
                var ctvm = s as MpClipTileViewModel;
                switch (e1.PropertyName) {
                    case nameof(ctvm.CopyItem):
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemId));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemType));
                        ctvm.OnPropertyChanged(nameof(ctvm.ImgVisibility));
                        ctvm.OnPropertyChanged(nameof(ctvm.RtbVisibility));
                        ctvm.OnPropertyChanged(nameof(ctvm.FileListVisibility));
                        ctvm.OnPropertyChanged(nameof(ctvm.WebBrowserFileViewerVisibility));
                        ctvm.OnPropertyChanged(nameof(ctvm.FileListStackPanelVisibility));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemTitle));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemPlainText));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemRichText));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemDescription));
                        //ctvm.OnPropertyChanged(nameof(ctvm.CopyItemFileDropList));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemAppName));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemAppPath));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemUsageScore));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemAppId));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyItemCreatedDateTime));
                        ctvm.OnPropertyChanged(nameof(ctvm.DetailText));
                        ctvm.OnPropertyChanged(nameof(ctvm.FileListCollectionViewModel));
                        ctvm.OnPropertyChanged(nameof(ctvm.CopyCount));
                        ctvm.OnPropertyChanged(nameof(ctvm.PasteCount));
                        ctvm.OnPropertyChanged(nameof(ctvm.CharCount));
                        ctvm.OnPropertyChanged(nameof(ctvm.LineCount));
                        ctvm.OnPropertyChanged(nameof(ctvm.FileCount));
                        ctvm.OnPropertyChanged(nameof(ctvm.DataSizeInMb));
                        ctvm.OnPropertyChanged(nameof(ctvm.IsLoading));
                        ctvm.OnPropertyChanged(nameof(ctvm.LoadingSpinnerVisibility));
                        ctvm.OnPropertyChanged(nameof(ctvm.ContentVisibility));
                        ctvm.OnPropertyChanged(nameof(ctvm.TrialOverlayVisibility));
                        ctvm.OnPropertyChanged(nameof(ctvm.TitleFontSize));
                        ctvm.OnPropertyChanged(nameof(ctvm.TileTitleIconSize));
                        ctvm.OnPropertyChanged(nameof(ctvm.TileTitleIconBorderSize));
                        break;
                    case nameof(ctvm.IsSelected):
                        if (ctvm.IsSelected) {
                            if (ctvm.TileVisibility != Visibility.Visible && !MpClipTrayViewModel.Instance.IsPastingHotKey) {
                                ctvm.IsSelected = false;
                                break;
                            }
                            //if(ctvm.ContentContainerViewModel.SubSelectedContentItems.Count == 0) {
                            //    ctvm.ContentContainerViewModel.ItemViewModels[0].IsSubSelected = true;
                            //}
                            ctvm.LastSelectedDateTime = DateTime.Now;
                            OnTileSelected?.Invoke(this, null);                 
                        } else {
                            ctvm.ContentContainerViewModel.ClearSubSelection();
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
                                ctvm.ContentContainerViewModel.ClearSubHovering();
                            }
                            foreach (var vctvm in MpClipTrayViewModel.Instance.VisibileClipTiles) {
                                if (vctvm != this) {
                                    vctvm.IsHovering = false;
                                    vctvm.ContentContainerViewModel.ClearSubHovering();
                                }
                            }
                        } else {
                            //_detailIdx = 1;
                        }
                        break;
                    case nameof(ctvm.IsEditingTile):                        
                        if (ctvm.IsEditingTile) {
                            OnTileExpand?.Invoke(this, null);
                            MainWindowViewModel.ExpandClipTile(this);
                        } else {
                            OnTileUnexpand?.Invoke(this, null);
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
                    case nameof(ctvm.TileVisibility):
                        if(ctvm.TileVisibility == Visibility.Collapsed) {
                            return;
                        }
                        if(ctvm.IsPlaceholder) {
                            ctvm.TileVisibility = Visibility.Collapsed;
                        }
                        break;
                }
            };

            ViewModelLoaded += (s, e) => {
                if (!MpMainWindowViewModel.IsMainWindowLoading && !IsTextItem) {
                    //Task.Run(GatherAnalytics);
                }
                //OnPropertyChanged(nameof(AppIcon));
            };
        }

        public MpClipTileViewModel(bool isPlaceholder) : this() {
            if (isPlaceholder) {
                IsBusy = true;
                //CopyItem = null;
            }
        }

        public MpClipTileViewModel(MpCopyItem ci) : this(false) {
            InitContent(ci);
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

            ContentContainerViewModel = MpContentContainerViewModel.Create(this, ci);

            ContentContainerViewModel.OnSubSelectionChanged += ContentContainerViewModel_OnSubSelectionChanged;

            TitleSwirlViewModel = new MpClipTileTitleSwirlViewModel(this);
            HighlightTextRangeViewModelCollection = new MpHighlightTextRangeViewModelCollection(ContentContainerViewModel.HeadItem);

            OnPropertyChanged(nameof(CopyItem));
        }

        

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            ClipBorder = (MpClipBorder)sender;
            ClipBorderTranslateTransform = (TranslateTransform)ClipBorder.FindName("ClipTileBorderTranslateTransform");
            ClipTileSelectionOverlayGrid = (Grid)ClipBorder.FindName("ClipTileSelectionOverlayGrid");
            var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");


            #region Shortcut
            var scvml = MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == CopyItemId).ToList();
            if (scvml.Count > 0) {
                ShortcutKeyString = scvml[0].KeyString;
            } else {
                ShortcutKeyString = string.Empty;
            }
            #endregion

            #region Anyalytics
            if (ContentContainerViewModel.ItemViewModels.Count == 1) {
                //CopyItemUrl = RichTextBoxViewModelCollection[0].CopyItemUrl;
            }
            #endregion

            OnViewModelLoaded();
        }

        public void ClipTileDetailGrid_Loaded(object sender, RoutedEventArgs e) {
            var detailGrid = (Grid)sender;
            var titleDetailTextBlock = (TextBlock)detailGrid.FindName("ClipTileTitleDetailTextBlock");

            titleDetailTextBlock.MouseEnter += (s, e5) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                ctvm.OnPropertyChanged(nameof(ctvm.DetailText));
            };
        }

        public MpImageAnalysisDocument ImagePreview { get; set; }

        public void ClipTileImageCanvas_Loaded(object sender, RoutedEventArgs e) {
            var ctvm = ((FrameworkElement)sender).DataContext as MpClipTileViewModel;
            if (ctvm.ImgVisibility == Visibility.Collapsed) {
                return;
            }

            var ic = (FrameworkElement)sender;
            var ctcc = ic.GetVisualAncestor<Canvas>();
            var vb = (Viewbox)ic.FindName("ClipTileImageItemsControlViewBox");

            //vb.ContextMenu = ctcc.ContextMenu = ic.ContextMenu = (ContextMenu)((FrameworkElement)sender).GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");

            MonkeyPaste.MpConsole.WriteLine("Image Analysis: " + CopyItemDescription);
            //ImagePreview = new MpImageAnalysisDocument();
        }

        public void ClipTileFileListBox_Loaded(object sender, RoutedEventArgs e) {
            var ctvm = (((FrameworkElement)sender).DataContext as MpFileListItemCollectionViewModel).HostClipTileViewModel;
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
            foreach (var mi in cm.Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if ((mi as MenuItem).Name == "ClipTileColorContextMenuItem") {
                    cmi = (MenuItem)mi;
                    break;
                }
            }
            MpHelpers.Instance.SetColorChooserMenuItem(
                    cm,
                    cmi,
                    (s, e1) => {
                        MpClipTrayViewModel.Instance.ChangeSelectedClipsColorCommand.Execute((Brush)((Border)s).Tag);
                        foreach (var sctvm in MpClipTrayViewModel.Instance.SelectedClipTiles) {
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
            //if (ctvm is MpRtbViewModel) {
            //    ctvm = (ctvm as MpRtbViewModel).HostClipTileViewModel;
            //}
            ctvm.IsContextMenuOpened = false;

            //ctvm.RichTextBoxViewModelCollection.ClearSubSelection();
        }

        public void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            ContextMenu cm = sender as ContextMenu;
            MpClipTileViewModel ctvm = cm.DataContext as MpClipTileViewModel;
            cm.Tag = ctvm;
            ctvm.IsContextMenuOpened = false;

            if (ctvm.CopyItemType == MpCopyItemType.RichText) {
                cm = MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(cm);
            }

            MenuItem eami = null;
            foreach (var mi in cm.Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if ((mi as MenuItem).Name == @"ToolsMenuItem") {
                    foreach (var smi in (mi as MenuItem).Items) {
                        if (smi == null || smi is Separator) {
                            continue;
                        }
                        if ((smi as MenuItem).Name == "ExcludeApplication") {
                            eami = smi as MenuItem;
                        }
                    }
                }
            }
            if (eami != null) {
                eami.Header = @"Exclude Application '" + CopyItemAppName + "'";
            }

            RefreshAsyncCommands();

            OnPropertyChanged(nameof(TagMenuItems));

            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(cm);
        }
        #endregion       

        public bool IsDragDataValid(IDataObject data) {
            if (CopyItemType == MpCopyItemType.Image || CopyItemType == MpCopyItemType.FileList) {
                return false;
            }
            if (data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                var dctvml = (List<MpClipTileViewModel>)data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                foreach (var dctvm in dctvml) {
                    if ((dctvm == this && !IsAnySubItemDragging) ||
                       dctvm.CopyItemType == MpCopyItemType.Image ||
                       dctvm.CopyItemType == MpCopyItemType.FileList) {
                        return false;
                    }
                }
                return true;
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


            ContentContainerViewModel.Resize(deltaEditToolbarTop, deltaWidth, deltaHeight);

            //EditTemplateToolbarViewModel.Resize(deltaHeight,deltaWidth);

        }

        public void RefreshAsyncCommands() {
            if(MpMainWindowViewModel.IsMainWindowLoading) {
                return;
            }

            MpClipTrayViewModel.Instance.HotkeyPasteCommand.RaiseCanExecuteChanged();
            MpClipTrayViewModel.Instance.BringSelectedClipTilesToFrontCommand.RaiseCanExecuteChanged();
            MpClipTrayViewModel.Instance.SendSelectedClipTilesToBackCommand.RaiseCanExecuteChanged();
            MpClipTrayViewModel.Instance.SpeakSelectedClipsCommand.RaiseCanExecuteChanged();
            MpClipTrayViewModel.Instance.MergeSelectedClipsCommand.RaiseCanExecuteChanged();
        }

        public MpEventEnabledFlowDocument GetSeparatedCompositeFlowDocument(string separatorChar = "- ") {
            var ccil = MpCopyItem.GetCompositeChildren(CopyItem);
            if (ccil.Count == 0) {
                return CopyItem.ItemData.ToFlowDocument();
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
            ContentContainerViewModel.InsertRange(forceIdx, ocil);
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
            ContentContainerViewModel.ClearSubSelection();
            ContentContainerViewModel.RequestUiUpdate();
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

            ContentContainerViewModel.ClearAllSubDragDropState();
        }
        public async Task GatherAnalytics() {
            var analyticTasks = new List<Task>();
            Task<string> urlTask = null, ocrTask = null, cvTask = null;
            if (CopyItem.SsDbImage != null) {
                urlTask = MpBrowserUrlDetector.Instance.FindUrlAddressFromScreenshot(CopyItem.SsDbImage.ImageBase64.ToBitmapSource());
                analyticTasks.Add(urlTask);
            }

            if (CopyItemType == MpCopyItemType.Image) {
                var itemBmpBytes = MpHelpers.Instance.ConvertBitmapSourceToByteArray(CopyItem.ItemData.ToBitmapSource());
                ocrTask = MpImageOcr.Instance.OcrImageForText(itemBmpBytes);
                analyticTasks.Add(ocrTask);
                cvTask = MpImageAnalyzer.Instance.AnalyzeImage(itemBmpBytes);
                analyticTasks.Add(cvTask);
            }

            await Task.WhenAll(analyticTasks.ToArray());

            if (urlTask != null) {
                string detectedUrl = await urlTask;
                if (!string.IsNullOrEmpty(detectedUrl)) {
                    string urlTitle = await MpHelpers.Instance.GetUrlTitle(detectedUrl);
                    //CopyItemUrl = MpUrl.Create(detectedUrl, urlTitle);
                    //if (CopyItemUrlDomain == null) {
                    //    string urlDomain = MpHelpers.Instance.GetUrlDomain(detectedUrl);
                    //    string urlDomainTitle = await MpHelpers.Instance.GetUrlTitle(urlDomain);
                    //    CopyItemUrlDomain = MpUrlDomain.Create(urlDomain,  urlDomainTitle);
                    //}
                }
                MonkeyPaste.MpConsole.WriteLine("Detected Browser Address: " + detectedUrl);
            }

            if (ocrTask != null) {
                CopyItemPlainText = await ocrTask;
            }

            if (cvTask != null) {
                CopyItemDescription = await cvTask;
                //var imgAnalysis = JsonConvert.DeserializeObject<MpImageAnalysis>(cvContent);
            }

            //OnPropertyChanged(nameof(AppIcon));
        }

        public void FadeIn(double bt = 0, double ms = 1000) {
            MpHelpers.Instance.AnimateVisibilityChange(
                ClipBorder,
                Visibility.Visible,
                (s, e) => {
                    TileVisibility = Visibility.Visible;
                },
                ms, bt);
        }

        public void FadeOut(Visibility outVisibility = Visibility.Collapsed, double bt = 0, double ms = 1000) {
            MpHelpers.Instance.AnimateVisibilityChange(
                ClipBorder,
                outVisibility,
                (s, e) => {
                    TileVisibility = outVisibility;
                },
                ms, bt);
        }


        public void Refresh() {
            var sw = new Stopwatch();
            sw.Start();
            ContentContainerViewModel.RequestUiUpdate();
            if (FileListBox != null) {
                FileListBox.Items.Refresh();
            }
            sw.Stop();
            MonkeyPaste.MpConsole.WriteLine("ClipTile(VIdx:" + MpClipTrayViewModel.Instance.VisibileClipTiles.IndexOf(this) + ") Refreshed (" + sw.ElapsedMilliseconds + "ms)");
        }

        public void ClearClipSelection() {
            IsSelected = false;
            LastSelectedDateTime = DateTime.MaxValue;
            ContentContainerViewModel.ClearSubSelection();
        }

        public void SaveToDatabase() {
            var sw = new Stopwatch();
            sw.Start();

            TextSelection rtbSelection = null;
            if (ContentContainerViewModel.SubSelectedContentItems.Count == 1 && IsEditingTile) {
                //rtbSelection = ContentContainerViewModel.SubSelectedContentItems[0].Rtb.Selection;
                MonkeyPaste.MpConsole.WriteLine("(AddTemplate)Selection Text: " + rtbSelection.Text);
            }

            //remove links to update model rich text
            //RichTextBoxViewModelCollection.ClearAllHyperlinks();

            //clear any search highlighting when saving the document then restore after save
            HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(null);

            var rtsw = new Stopwatch();
            rtsw.Start();
            ContentContainerViewModel.SaveAll();
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
            OnPropertyChanged(nameof(CopyItem));
            cipcsw.Stop();
            MonkeyPaste.MpConsole.WriteLine("Saving cliptile copyitem propertychanged time: " + cipcsw.ElapsedMilliseconds + "ms");

            sw.Stop();
            MonkeyPaste.MpConsole.WriteLine("Saving(VIdx:" + MpClipTrayViewModel.Instance.VisibileClipTiles.IndexOf(this) + "): " + sw.ElapsedMilliseconds + "ms");

            if (rtbSelection != null && ContentContainerViewModel.SubSelectedContentItems.Count == 1) {
                //ContentContainerViewModel.SubSelectedContentItems[0].Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }


        public async Task<string> GetSubSelectedPastableRichText(bool isToExternalApp = false) {
            if (IsTextItem) {
                if (ContentContainerViewModel.SubSelectedContentItems.Count == 0) {
                    ContentContainerViewModel.SubSelectAll();
                }
                bool isPastingTemplate = ContentContainerViewModel.SubSelectedContentItems.Any(x => (x as MpRtbItemViewModel).IsDynamicPaste);
                if (isPastingTemplate) {
                    IsPastingTemplate = true;
                    TemplateRichText = string.Empty.ToRichText();
                    if (!MpMainWindowViewModel.IsMainWindowOpen) {
                        MainWindowViewModel.ShowWindowCommand.Execute(null);
                    }
                    ContentContainerViewModel.RequestUiUpdate();
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
                foreach (var rtbvm in ContentContainerViewModel.SubSelectedContentItems) {
                    if ((rtbvm as MpRtbItemViewModel).IsDynamicPaste) {
                        //rtbvm.IsSubSelected = true;
                        (ContentContainerViewModel as MpRtbItemCollectionViewModel).PasteTemplateToolbarViewModel.SubSelectedRtbViewModel = rtbvm as MpRtbItemViewModel;
                        (ContentContainerViewModel as MpRtbItemCollectionViewModel).PasteTemplateToolbarViewModel.PasteTemplateCommand.Execute(null);
                        string rtbvmrtf = (rtbvm as MpRtbItemViewModel).TemplateRichText;
                        rtf = MpHelpers.Instance.CombineRichText(rtbvmrtf, rtf, true);
                        //rtbvm.TemplateRichText = string.Empty;
                        (rtbvm as MpRtbItemViewModel).TemplateHyperlinkCollectionViewModel.Reset();
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
            switch (CopyItemType) {
                case MpCopyItemType.RichText:
                   // if (RichTextBoxViewModelCollection.ListBox == null) {
                        //no idea why this happens but the rtblb is null upon
                        //searchbox focus
                        //break;
                   // }
                    ContentContainerViewModel.RequestScrollIntoView(ContentContainerViewModel.ItemViewModels[0]);
                    foreach (MpRtbItemViewModel rtbvm in ContentContainerViewModel.ItemViewModels) {
                        rtbvm.RequestRtbReset();
                    }
                    break;
                case MpCopyItemType.FileList:
                    FileListBox.ScrollIntoView(FileListCollectionViewModel[0]);
                    break;
            }
        }
        #endregion

        #region Private Methods           
        private void ContentContainerViewModel_OnSubSelectionChanged(object sender, object e) {
            if(e != null && !IsSelected) {
                IsSelected = true;
            }
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
            //MpClipTrayViewModel.Instance.ClearClipSelection();
            //IsSelected = true;
            //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        }

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
                   ContentContainerViewModel.SubSelectedContentItems.Count <= 1;
        }
        private void EditTitle() {
            if (ContentContainerViewModel.SubSelectedContentItems.Count == 0) {
                IsEditingTitle = !IsEditingTitle;
            } else {
                ContentContainerViewModel.SubSelectedContentItems[0].EditSubTitleCommand.Execute(null);
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
            if (!IsEditingTile) {
                IsEditingTile = true;
            }
            if (ContentContainerViewModel.SubSelectedContentItems.Count == 0) {
                ContentContainerViewModel.ItemViewModels[0].IsSubSelected = true;
            }
            ContentContainerViewModel.SubSelectedContentItems[0].EditSubContentCommand.Execute(null);
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
            return MpClipTrayViewModel.Instance.SelectedClipTiles.Count == 1;
        }
        private void ExcludeApplication() {
            MpAppCollectionViewModel.Instance.UpdateRejection(MpAppCollectionViewModel.Instance.GetAppViewModelByAppId(CopyItemAppId), true);
        }

        private RelayCommand<object> _pasteClipCommand;
        public ICommand PasteClipCommand {
            get {
                if (_pasteClipCommand == null) {
                    _pasteClipCommand = new RelayCommand<object>(PasteClip);
                }
                return _pasteClipCommand;
            }
        }
        private void PasteClip(object args) {
            MpClipTrayViewModel.Instance.ClearClipSelection();
            IsSelected = true;
            MpClipTrayViewModel.Instance.PasteSelectedClipsCommand.Execute(args);
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
            ShortcutKeyString = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(
                this,
                "Paste " + CopyItemTitle,
                ShortcutKeyString,
                MpClipTrayViewModel.Instance.HotkeyPasteCommand, CopyItemId);
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
            if(!isMerge) {
                foreach (var rtbvm in ContentContainerViewModel.ItemViewModels) {
                    //remove any shortcuts associated with clip
                    var scvmToRemoveList = new List<MpShortcutViewModel>();
                    foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == rtbvm.CopyItem.Id).ToList()) {
                        scvmToRemoveList.Add(scvmToRemove);
                    }
                    foreach (var scvmToRemove in scvmToRemoveList) {
                        MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
                    }
                    rtbvm.CopyItem.DeleteFromDatabase();
                }
            }
            
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
