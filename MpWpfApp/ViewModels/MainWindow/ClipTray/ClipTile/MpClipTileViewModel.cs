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

        private ObservableCollection<MpContextMenuItemViewModel> _convertClipTypes = new ObservableCollection<MpContextMenuItemViewModel>();
        public ObservableCollection<MpContextMenuItemViewModel> ConvertClipTypes {
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

        private ObservableCollection<MpContextMenuItemViewModel> _tagMenuItems = new ObservableCollection<MpContextMenuItemViewModel>();
        public ObservableCollection<MpContextMenuItemViewModel> TagMenuItems {
            get {
                _tagMenuItems.Clear();
                foreach (var tagTile in MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel) {
                    if (tagTile.IsSudoTag) {
                        continue;
                    }
                    bool isTagAssociated = tagTile.IsLinkedWithClipTile(this);
                    string checkIconPath = string.Empty;
                    if(isTagAssociated) {
                        checkIconPath = Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/minus.png";
                    }
                    _tagMenuItems.Add(
                        new MpContextMenuItemViewModel(
                            tagTile.TagName,
                            MainWindowViewModel.ClipTrayViewModel.LinkTagToCopyItemCommand,
                            tagTile,
                            isTagAssociated,
                            checkIconPath,
                            null,
                            tagTile.ShortcutKeyString,
                            tagTile.TagColor));
                }
                return _tagMenuItems;
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
                if(string.IsNullOrEmpty(ShortcutKeyString)) {
                    return new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath+@"/Images/joystick.png"));
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

        public BitmapSource AppIcon {
            get {
                if(MainWindowViewModel == null || 
                   MainWindowViewModel.ClipTrayViewModel == null ||
                  !MainWindowViewModel.ClipTrayViewModel.IsFilteringByApp) {
                    if(CopyItem == null) {
                        return new BitmapImage();
                    }
                    if(CopyItemFavIcon != null) {
                        return CopyItemFavIcon;
                    }
                    if(RichTextBoxViewModelCollection.Count == 1 && RichTextBoxViewModelCollection[0].CopyItemFavIcon != null) {
                        return RichTextBoxViewModelCollection[0].CopyItemFavIcon;
                    }
                    return CopyItemAppIcon;
                }
                return MainWindowViewModel.ClipTrayViewModel.FilterByAppIcon;
            }
        }
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

        public double TileTitleIconSize {
            get {
                if(CopyItem.ItemFavIcon == null) {
                    return MpMeasurements.Instance.ClipTileTitleIconSize;
                }
                return MpMeasurements.Instance.ClipTileTitleFavIconSize;
            }
        }

        public double TileTitleIconBorderSize {
            get {
                if (CopyItem.ItemFavIcon == null) {
                    return MpMeasurements.Instance.ClipTileTitleIconBorderSize;
                }
                return MpMeasurements.Instance.ClipTileTitleFavIconBorderSize;
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

        public Visibility ClipTileTitleAppIconButtonVisibility {
            get {
                return IsExpanded ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public Visibility ClipTileTitleAppIconImageVisibility {
            get {
                if(MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null || !IsSelected) {
                    return Visibility.Visible;
                }
                if(MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count > 1 &&
                   !IsHovering) {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
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

        public Visibility MultiSelectOrderMarkerVisibility {
            get {
                if(MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null) {
                    return Visibility.Hidden;
                }
                if(IsSelected && MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count > 1) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Business Logic
        public bool IsStubItem {
            get {
                return CopyItemId == 0 && TileVisibility != Visibility.Visible;
            }
        }

        public bool IsTextItem {
            get {
                return CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Composite;
            }
        }

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

        public new bool IsLoading {
            get {
                return CopyItem == null || CopyItem.CopyItemId == 0;
            }
        }

        public int SortOrderIdx {
            get {
                if(MainWindowViewModel.ClipTrayViewModel == null || TileVisibility != Visibility.Visible) {
                    return -1;
                }
                return MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles.IndexOf(this);
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
                if (RichTextBoxViewModelCollection == null ||
                   !IsAnySubContextMenuOpened) {
                    return new Rect(50,0,50,50);
                }
                if(IsAnySubContextMenuOpened) {
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
        public string MultiSelectedOrderIdxDisplayValue {
            get {
                if(MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null || !IsSelected) {
                    return string.Empty;
                }
                int multiIdx = MainWindowViewModel.ClipTrayViewModel.GetSelectionOrderIdxForItem(this);
                if(multiIdx < 0) {
                    return string.Empty;
                }
                multiIdx++;
                return multiIdx.ToString();
            }
        }

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

        public bool IsAnySubContextMenuOpened {
            get {
                return RichTextBoxViewModelCollection.Any(x => x.IsSubContextMenuOpened && RichTextBoxViewModelCollection.Count > 1);
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
        public BitmapSource CopyItemFavIcon {
            get {
                if(CopyItem == null) {
                    return null;
                }
                if(CopyItem.ItemFavIcon == null && RichTextBoxViewModelCollection.Count == 1) {
                    CopyItem.ItemFavIcon = RichTextBoxViewModelCollection[0].CopyItemFavIcon;
                    OnPropertyChanged(nameof(CopyItem));
                    
                }
                return CopyItem.ItemFavIcon;
            }
            set {
                if (CopyItem != null && CopyItem.ItemFavIcon != value) {
                    CopyItem.ItemFavIcon = value;
                    //CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(AppIcon));
                    OnPropertyChanged(nameof(TileTitleIconSize));
                    OnPropertyChanged(nameof(TileTitleIconBorderSize));
                }
            }
        }

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
                    //CopyItem.WriteToDatabase();
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
                    //CopyItem.WriteToDatabase();
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
                    OnPropertyChanged(nameof(HotkeyIconSource));
                    OnPropertyChanged(nameof(HotkeyIconTooltip));
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
                    //CopyItem.WriteToDatabase();
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
            set {
                if (CopyItem != null && CopyItem.ItemPlainText != value) {
                    CopyItem.ItemPlainText = value;
                    //CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItem));
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
                if(CopyItem != null && CopyItem.ItemDescription != value) {
                    CopyItem.ItemDescription = value;
                    //CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItem));
                }
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
                    //CopyItem.WriteToDatabase();
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
                   // CopyItem.WriteToDatabase();
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
                       HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.HighlightType == MpHighlightType.App &&
                       (RichTextBoxViewModelCollection.Count == 1 || HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.RtbItemViewModel == null)) {
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

        public string CopyItemUrl {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.ItemUrl;
            }
            set {
                if(CopyItem != null && CopyItem.ItemUrl != value) {
                    CopyItem.ItemUrl = value;
                    //CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItem));
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
                if (CopyItem != null && CopyItem.CopyDateTime != value) {
                    CopyItem.CopyDateTime = value;
                    //CopyItem.WriteToDatabase();
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
                    if(CopyItem != null) {
                        CopyItem.WriteToDatabase();
                    }
                    
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
                    OnPropertyChanged(nameof(CopyItemDescription));
                    OnPropertyChanged(nameof(CopyItemBmp));
                    OnPropertyChanged(nameof(CopyItemFileDropList));
                    OnPropertyChanged(nameof(CopyItemUrl));
                    OnPropertyChanged(nameof(CopyItemFavIcon));
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
                    OnPropertyChanged(nameof(AppIcon));
                    OnPropertyChanged(nameof(TileTitleIconSize));
                    OnPropertyChanged(nameof(TileTitleIconBorderSize));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileViewModel() : base() {
            PropertyChanged += (s, e1) => {
                var ctvm = s as MpClipTileViewModel;
                switch (e1.PropertyName) {
                    case nameof(ctvm.IsSelected):
                        if (ctvm.IsSelected) {
                            if (ctvm.TileVisibility != Visibility.Visible && !MainWindowViewModel.ClipTrayViewModel.IsPastingHotKey) {
                                ctvm.IsSelected = false;
                                break;
                            }
                            ctvm.LastSelectedDateTime = DateTime.Now;
                            foreach(var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                                rtbvm.IsSubSelected = true;
                            }
                            //RichTextBoxViewModelCollection.SubSelectAll();
                        } else {
                            ctvm.RichTextBoxViewModelCollection.ClearSubSelection();
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
                            if (MainWindowViewModel.ClipTrayViewModel.IsScrolling) {
                                ctvm.IsHovering = false;
                                foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                                    rtbvm.IsSubHovering = false;
                                }
                            }
                            foreach (var vctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
                                if (vctvm != this) {
                                    vctvm.IsHovering = false;
                                    foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                                        rtbvm.IsSubHovering = false;
                                    }
                                }
                            }
                        } else {
                            _detailIdx = 1;
                        }
                        break;
                    case nameof(ctvm.IsEditingTile):
                        if (ctvm.IsEditingTile) {
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
                        foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                            rtbvm.UpdateLayout();
                        }
                        break;
                    case nameof(ctvm.IsBusy):
                        ctvm.OnPropertyChanged(nameof(ctvm.ContentVisibility));
                        ctvm.OnPropertyChanged(nameof(ctvm.LoadingSpinnerVisibility));
                        break;
                }
            };

            ViewModelLoaded += async (s, e) => {
                if (!MainWindowViewModel.IsLoading) {
                    await GatherAnalytics();
                }
                OnPropertyChanged(nameof(AppIcon));
            };
        }

        public MpClipTileViewModel(bool isPlaceholder) : this() {
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

            #region Selection
            ClipBorder.MouseEnter += (s, e1) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                ctvm.IsHovering = true;                
            };
            ClipBorder.MouseMove += (s, e1) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if(!ctvm.IsSelected || MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count <= 1) {
                    return;
                }
                var mp = e1.GetPosition(ctvm.RichTextBoxViewModelCollection.ListBox);
                bool isOverSubSelectedDragButton = false;
                foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                    var itemRect = ctvm.RichTextBoxViewModelCollection.GetListBoxItemRect(ctvm.RichTextBoxViewModelCollection.IndexOf(rtbvm));
                    rtbvm.IsSubHovering = itemRect.Contains(mp);
                    if (rtbvm.IsSubHovering) {
                        var irmp = e1.GetPosition(rtbvm.Rtbc);
                        var dragButtonRect = rtbvm.DragButtonRect;
                        if (dragButtonRect.Contains(irmp)) {
                            isOverSubSelectedDragButton = true;
                            ctvm.RichTextBoxViewModelCollection.SyncMultiSelectDragButton(true, e1.MouseDevice.LeftButton == MouseButtonState.Pressed);
                        }
                    }
                }
                if (!isOverSubSelectedDragButton) {
                    ctvm.RichTextBoxViewModelCollection.SyncMultiSelectDragButton(false, e1.MouseDevice.LeftButton == MouseButtonState.Pressed);
                }
            };
            ClipBorder.PreviewMouseUp += (s, e3) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel; 
                if (!ctvm.IsSelected || MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count <= 1) {
                    return;
                }
                var mp = e3.GetPosition(ctvm.RichTextBoxViewModelCollection.ListBox);
                bool isSubSelection = false;
                if(ctvm.IsSelected && ctvm.RichTextBoxViewModelCollection.Count > 1) {
                    foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                        if (rtbvm.IsSubHovering) {
                            isSubSelection = true;
                            ctvm.RichTextBoxViewModelCollection.UpdateExtendedSelection(ctvm.RichTextBoxViewModelCollection.IndexOf(rtbvm));
                        }
                    }
                }
                if(isSubSelection) {
                    e3.Handled = true;
                } else {
                    //MainWindowViewModel.ClipTrayViewModel.UpdateExtendedSelection(MainWindowViewModel.ClipTrayViewModel.IndexOf(this));
                }

            };
            //ClipBorder.PreviewMouseUp += (s, e3) => {
            //    var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
            //    if (IsSelected &&
            //       MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count > 1) {
            //        var mp = e3.GetPosition(RichTextBoxViewModelCollection.ListBox);
            //        foreach (var rtbvm in RichTextBoxViewModelCollection) {
            //            if (rtbvm.IsSubHovering) {
            //                RichTextBoxViewModelCollection.UpdateExtendedSelection(RichTextBoxViewModelCollection.IndexOf(rtbvm), false);
            //            }
            //        }
            //    }
            //};
            ClipBorder.MouseLeave += (s, e2) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (ctvm != null && !ctvm.IsClipDragging) {
                    ctvm.IsHovering = false;
                    foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                        rtbvm.IsSubHovering = false;
                    }
                }
            };
            ClipBorder.LostFocus += (s, e4) => {
                var ctvm = ((FrameworkElement)s).DataContext as MpClipTileViewModel;
                if (!ctvm.IsSelected) {
                    ctvm.IsEditingTitle = false;
                }
            };
            #endregion

            #region Drag/Drop

            int minDragDist = 15;
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
                    //ctvm.IsSelected = true;
                    // ctvm.RichTextBoxViewModelCollection.SubSelectAll();
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
                if (e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                    int dropIdx = ctvm.GetDropIdx(MpHelpers.Instance.GetMousePosition(ctvm.RichTextBoxViewModelCollection.ListBox));
                    if (dropIdx >= 0 && dropIdx <= ctvm.RichTextBoxViewModelCollection.Count) {
                        if (dropIdx < ctvm.RichTextBoxViewModelCollection.Count) {
                            if(!ctvm.RichTextBoxViewModelCollection.IsListBoxItemVisible(dropIdx)) {
                                ctvm.RichTextBoxViewModelCollection.ListBox?.ScrollIntoView(ctvm.RichTextBoxViewModelCollection[dropIdx]);
                            } else if(dropIdx > 0 && dropIdx - 1 < ctvm.RichTextBoxViewModelCollection.Count) {
                                ctvm.RichTextBoxViewModelCollection.ListBox?.ScrollIntoView(ctvm.RichTextBoxViewModelCollection[dropIdx-1]);
                            }
                        } else {
                            //only can be count + 1
                            if (!ctvm.RichTextBoxViewModelCollection.IsListBoxItemVisible(dropIdx-1)) {
                                ctvm.RichTextBoxViewModelCollection.ListBox?.ScrollIntoView(ctvm.RichTextBoxViewModelCollection[dropIdx-1]);
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
                ctvm.RichTextBoxViewModelCollection.UpdateAdorners();
            };

            ClipBorder.PreviewDrop += (s3, e2) => {
                var ctvm = ((FrameworkElement)s3).DataContext as MpClipTileViewModel;
                bool wasDropped = false;
                var dctvml = new List<MpClipTileViewModel>();
                if (e2.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                    dctvml = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                    int dropIdx = ctvm.GetDropIdx(MpHelpers.Instance.GetMousePosition(ctvm.RichTextBoxViewModelCollection.ListBox));
                    if (dropIdx >= 0) {
                        /*
                         On tile drop: 
                         0. order same as tray drop 
                         1. take all non composite copyitems from sctvm and remove empty tiles if NOT drop tile. 
                         2. Then insert at dropidx where copyitems will still be hctvm selected datetime order. 
                        */
                        var dcil = new List<MpCopyItem>();
                        foreach(var dctvm in dctvml) {
                            bool wasEmptySelection = dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 0;
                            if (wasEmptySelection) {
                                dctvm.RichTextBoxViewModelCollection.SubSelectAll();
                            }
                            if (dctvm.RichTextBoxViewModelCollection.Count == 0) {
                                dcil.Add(dctvm.CopyItem);
                            } else {
                                foreach (var ssrtbvm in dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems) {
                                    dcil.Add(ssrtbvm.CopyItem);
                                }
                            }
                        }
                        dcil.Reverse();
                        ctvm.MergeClip(dcil, dropIdx);
                        wasDropped = true;
                    }
                }
                if (!wasDropped) {
                    e2.Effects = DragDropEffects.None;
                }
                e2.Handled = true;
                ctvm.IsClipDropping = false;
                ctvm.RichTextBoxViewModelCollection.RtbLbAdornerLayer?.Update();
            };
            #endregion

            var scvml = MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == CopyItemId).ToList();
            if(scvml.Count > 0) {
                ShortcutKeyString = scvml[0].KeyString;
            } else {
                ShortcutKeyString = string.Empty;
            }
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

        public void ClipTileTitle_Loaded(object sender, RoutedEventArgs e) {
            var lctvm = ((FrameworkElement)sender).DataContext as MpClipTileViewModel;

            var titleCanvas = (Grid)sender;
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
            //Canvas.SetLeft(titleTextGrid, MpMeasurements.Instance.ClipTileTitleTextGridCanvasLeft);
            //Canvas.SetTop(titleTextGrid, MpMeasurements.Instance.ClipTileTitleTextGridCanvasTop);

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
            
            //Canvas.SetLeft(titleIconImageButton, MpMeasurements.Instance.ClipTileTitleIconCanvasLeft);
            //Canvas.SetTop(titleIconImageButton, 2);

            //var diff = (TileTitleIconBorderSize - TileTitleIconSize) / 2;
            //Canvas.SetLeft(titleIconHighlightBorderImage, MpMeasurements.Instance.ClipTileTitleIconCanvasLeft-diff);
            //Canvas.SetTop(titleIconHighlightBorderImage, 2-diff);

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
                if (MainWindowViewModel.ClipTrayViewModel.IsScrolling || IsContextMenuOpened) {
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

                //MpHelpers.Instance.OpenUrl(CopyItem.App.AppPath);
                MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
                ctvm.IsSelected = true;
                foreach (var vctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
                    if(vctvm.CopyItemAppId != ctvm.CopyItemAppId) {
                        bool hasSubItemWithApp = false;
                        if(vctvm.RichTextBoxViewModelCollection.Count > 1) {
                            foreach(var vrtbvm in vctvm.RichTextBoxViewModelCollection) {
                                if(vrtbvm.CopyItemAppId != ctvm.CopyItemAppId) {
                                    vrtbvm.SubItemVisibility = Visibility.Collapsed;
                                } else {
                                    hasSubItemWithApp = true;
                                }
                            }
                        }
                        if(!hasSubItemWithApp) {
                            vctvm.TileVisibility = Visibility.Collapsed;
                        }
                    }
                }
                //this triggers clip tray to swap out the app icons for the filtered app
                MainWindowViewModel.ClipTrayViewModel.FilterByAppIcon = ctvm.CopyItemAppIcon;
                MainWindowViewModel.ClipTrayViewModel.IsFilteringByApp = true;
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

            ctvm.DetectedImageObjectCollectionViewModel = new MpDetectedImageObjectCollectionViewModel(CopyItem);

            Console.WriteLine("Image Analysis: " + CopyItemDescription);
            //ImagePreview = new MpImageAnalysisDocument();
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
            foreach (var mi in cm.Items) {
                if(mi == null || mi is Separator) {
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
            //if (ctvm is MpRtbListBoxItemRichTextBoxViewModel) {
            //    ctvm = (ctvm as MpRtbListBoxItemRichTextBoxViewModel).HostClipTileViewModel;
            //}
            ctvm.IsContextMenuOpened = false;

            //ctvm.RichTextBoxViewModelCollection.ClearSubSelection();
        }

        public void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            ContextMenu cm = sender as ContextMenu;
            MpClipTileViewModel ctvm = cm.DataContext as MpClipTileViewModel;
            cm.Tag = ctvm;
            ctvm.IsContextMenuOpened = false;

            if (ctvm.CopyItemType == MpCopyItemType.RichText ||
               ctvm.CopyItemType == MpCopyItemType.Composite) {
                cm = MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(cm);
            }

            MenuItem eami = null;
            foreach (var mi in cm.Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if((mi as MenuItem).Name == @"ToolsMenuItem") {
                    foreach(var smi in (mi as MenuItem).Items) {
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

        public bool IsDragDataInvalid(IDataObject data) {
            if(CopyItemType == MpCopyItemType.Image || CopyItemType == MpCopyItemType.FileList) {
                return true;
            }
            if(data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                var dctvml = (List<MpClipTileViewModel>)data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                foreach(var dctvm in dctvml) {
                    if((dctvm == this && !IsAnySubItemDragging) || 
                       dctvm.CopyItemType == MpCopyItemType.Image || 
                       dctvm.CopyItemType == MpCopyItemType.FileList) {
                        return true;
                    }
                }
                return false;
            } 
            return true;
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

        public void RefreshAsyncCommands() {
            MainWindowViewModel.ClipTrayViewModel.HotkeyPasteCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.BringSelectedClipTilesToFrontCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.SendSelectedClipTilesToBackCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.SpeakSelectedClipsAsyncCommand.RaiseCanExecuteChanged();
            MainWindowViewModel.ClipTrayViewModel.MergeSelectedClipsCommand.RaiseCanExecuteChanged();
        }

        public void MergeClip(
            List<MpCopyItem> ocil,
            int forceIdx = -1) {
            //IsBusy = true;
            foreach (var oci in ocil) {
                if (oci.CopyItemId == CopyItemId) {
                    return;
                }
                var octvm = MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(oci.CopyItemId);
                if (octvm != null) {
                    //if copyitem is an existing tile remove it
                    MainWindowViewModel.ClipTrayViewModel.Remove(octvm, true);
                    //if(CopyItemType == MpCopyItemType.RichText) {
                    //    CopyItem = MpCopyItem.Merge(oci, CopyItem, false, false, forceIdx);
                    //}
                    //RichTextBoxViewModelCollection.Add(new MpRtbListBoxItemRichTextBoxViewModel(this, octvm.CopyItem));
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
                        } else {
                            //if copyitem is part of composite remove it 
                            ortbvm.RichTextBoxViewModelCollection.Remove(ortbvm, true);

                            //RichTextBoxViewModelCollection.Add(ortbvm, forceIdx, true);
                        }
                    }
                }

                CopyItem = MpCopyItem.Merge(oci, CopyItem, false, false, forceIdx);
            }
            var scvml = MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == CopyItemId).ToList();
            if (scvml.Count > 0) {
                ShortcutKeyString = scvml[0].KeyString;
            } else {
                ShortcutKeyString = string.Empty;
            }

            MainWindowViewModel.TagTrayViewModel.RefreshAllCounts();

            RichTextBoxViewModelCollection.SyncItemsWithModel();

            //MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            //IsSelected = true;

            SaveToDatabase();

            //IsBusy = false;

            RichTextBoxViewModelCollection.UpdateLayout();
            RichTextBoxViewModelCollection.Refresh();
            MainWindowViewModel.ClipTrayViewModel.Refresh();
        }
        public async Task MergeClipAsync(
            List<MpCopyItem> ocil, 
            int forceIdx = -1, 
            DispatcherPriority priority = DispatcherPriority.Background) {
            await Application.Current.Dispatcher.BeginInvoke(priority, (Action) (()=> { MergeClip(ocil, forceIdx); }));
        }

        public async Task GatherAnalytics() {
            var analyticTasks = new List<Task>();
            Task<string> urlTask = null, ocrTask = null, cvTask = null;
            if (CopyItem.ItemScreenshot != null) {
                urlTask = MpBrowserUrlDetector.Instance.FindUrlAddressFromScreenshot(CopyItem.ItemScreenshot);
                analyticTasks.Add(urlTask);                
            }

            if(CopyItemType == MpCopyItemType.Image) {
                ocrTask = MpImageOcr.Instance.OcrImageForText(CopyItem.ItemBmpByteArray);
                analyticTasks.Add(ocrTask);
                cvTask = MpImageAnalyzer.Instance.AnalyzeImage(CopyItem.ItemBmpByteArray);
                analyticTasks.Add(cvTask);
            }

            await Task.WhenAll(analyticTasks.ToArray());

            if(urlTask != null) {
                string detectedUrl = await urlTask;
                if (!string.IsNullOrEmpty(detectedUrl)) {
                    CopyItemUrl = detectedUrl;
                    CopyItemFavIcon = MpHelpers.Instance.GetUrlFavicon(detectedUrl);
                }
                Console.WriteLine("Detected Browser Address: " + detectedUrl);
            }

            if(ocrTask != null) {
                CopyItemPlainText = await ocrTask;
            }

            if(cvTask != null) {
                CopyItemDescription = await cvTask;
                //var imgAnalysis = JsonConvert.DeserializeObject<MpImageAnalysis>(cvContent);
            }

            OnPropertyChanged(nameof(AppIcon));
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
            if(RichTextBoxViewModelCollection.ListBox != null) {
                RichTextBoxViewModelCollection.Refresh();
            }
            if (FileListBox != null) {
                FileListBox.Items.Refresh();
            }
            sw.Stop();
            Console.WriteLine("ClipTile(VIdx:"+MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles.IndexOf(this)+") Refreshed (" + sw.ElapsedMilliseconds + "ms)");
        }

        public void ClearClipSelection() {
            IsSelected = false;
            LastSelectedDateTime = DateTime.MaxValue;
            RichTextBoxViewModelCollection.ClearSubSelection();
        }

        public void SaveToDatabase() {
            var sw = new Stopwatch();
            sw.Start();

            TextSelection rtbSelection = null;
            if (RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 1 && IsEditingTile) {
                rtbSelection = RichTextBoxViewModelCollection.SubSelectedClipItems[0].Rtb.Selection;
                Console.WriteLine("(AddTemplate)Selection Text: " + rtbSelection.Text);
            }
            
            //remove links to update model rich text
            RichTextBoxViewModelCollection.ClearAllHyperlinks();

            //clear any search highlighting when saving the document then restore after save
            HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(null);

            var rtsw = new Stopwatch();
            rtsw.Start();
            foreach (var rtbvm in RichTextBoxViewModelCollection) {
                if (rtbvm.Rtb == null) {
                    continue;
                }
                //property change will write the copyitem to the database
                rtbvm.CopyItemRichText = rtbvm.Rtb.Document.ToRichText();
            }
            rtsw.Stop();
            Console.WriteLine("Saving rich text from rtb's time: " + rtsw.ElapsedMilliseconds + "ms");

            //CopyItemRichText = RichTextBoxViewModelCollection.FullDocument.ToRichText();
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
            Console.WriteLine("Saving(VIdx:" + MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles.IndexOf(this) + "): " + sw.ElapsedMilliseconds + "ms");

            if (rtbSelection != null && RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 1) {
                RichTextBoxViewModelCollection.SubSelectedClipItems[0].Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }
                

        public async Task<string> GetPastableRichText() {
            if(IsTextItem) {
                if(RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 0) {
                    RichTextBoxViewModelCollection.SubSelectAll();
                }
                bool isPastingTemplate = RichTextBoxViewModelCollection.SubSelectedClipItems.Any(x => x.HasTemplate);
                if(isPastingTemplate) {
                    IsPastingTemplate = true;
                    TemplateRichText = string.Empty.ToRichText();
                    await RichTextBoxViewModelCollection.FillAllTemplates();
                }
                //var sb = new StringBuilder();
                //sb.Append(string.Empty.ToRichText());
                string rtf = string.Empty.ToRichText();
                foreach (var rtbvm in RichTextBoxViewModelCollection.SubSelectedClipItems) {
                    if (rtbvm.HasTemplate) {
                        //rtbvm.IsSubSelected = true;
                        PasteTemplateToolbarViewModel.SetTemplate(rtbvm.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder[0].TemplateName);
                        PasteTemplateToolbarViewModel.PasteTemplateCommand.Execute(null);
                        string rtbvmrtf = rtbvm.TemplateRichText;
                        rtf = MpHelpers.Instance.CombineRichText(rtbvmrtf, rtf, true);
                        rtbvm.TemplateRichText = string.Empty;
                    } else {
                        rtf = MpHelpers.Instance.CombineRichText(rtbvm.CopyItemRichText, rtf, true);
                    }
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
            switch(CopyItemType) {
                case MpCopyItemType.Composite:
                case MpCopyItemType.RichText:
                    if(RichTextBoxViewModelCollection.ListBox == null) {
                        //no idea why this happens but the rtblb is null upon
                        //searchbox focus
                        break;
                    }
                    RichTextBoxViewModelCollection.ListBox.ScrollIntoView(RichTextBoxViewModelCollection[0]);
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
                var itemRect = RichTextBoxViewModelCollection.GetListBoxItemRect(RichTextBoxViewModelCollection.IndexOf(rtbvm));
                itemRect.Height = MpMeasurements.Instance.RtbCompositeItemMinHeight;
                double lbity = itemRect.Top;//lbi.TranslatePoint(new Point(0.0, 0.0), RichTextBoxListBox).Y;
                double lbiby = itemRect.Bottom;//lbi.TranslatePoint(new Point(rtbvm.Rtbc.ActualHeight, 0), RichTextBoxListBox).Y;
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
            if (MainWindowViewModel.IsLoading) {
                return false;
            }
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 &&
                   RichTextBoxViewModelCollection.SubSelectedClipItems.Count <= 1;
        }
        private void EditTitle() {
            if(RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 0) {
                IsEditingTitle = !IsEditingTitle;
            } else {
                RichTextBoxViewModelCollection.SubSelectedClipItems[0].EditSubTitleCommand.Execute(null);
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
            if (MainWindowViewModel.IsLoading) {
                return false;
            }
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 &&
                   MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles[0].IsTextItem;
        }
        private void EditContent() {
            if(!IsEditingTile) {
                IsEditingTile = true;
            }
            if(RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 0) {
                RichTextBoxViewModelCollection[0].IsSubSelected = true;
            }
            RichTextBoxViewModelCollection.SubSelectedClipItems[0].EditSubContentCommand.Execute(null);
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
            MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            IsSelected = true; 
            MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand.Execute(args);
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
                MainWindowViewModel.ClipTrayViewModel.HotkeyPasteCommand, CopyItemId);
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
            var rtbvmToRemove = RichTextBoxViewModelCollection;
            RichTextBoxViewModelCollection.Clear();
            foreach(var rtbvm in rtbvmToRemove) {
                rtbvm.Dispose();
            }
            //remove any shortcuts associated with clip
            var scvmToRemoveList = new List<MpShortcutViewModel>();
            foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == CopyItemId).ToList()) {
                scvmToRemoveList.Add(scvmToRemove);
            }
            foreach (var scvmToRemove in scvmToRemoveList) {
                MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
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
