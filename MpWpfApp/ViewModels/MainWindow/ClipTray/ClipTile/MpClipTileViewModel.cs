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
    using GalaSoft.MvvmLight.CommandWpf;
    using GongSolutions.Wpf.DragDrop.Utilities;
    using Newtonsoft.Json;
    using MonkeyPaste;
using System.Speech.Synthesis;

    public class MpClipTileViewModel : MpViewModelBase<MpClipTrayViewModel> {
        #region Private Variables

        private double _unexpandedHeight = 0;
        private List<string> _tempFileList = new List<string>();
        //container

        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;
        private List<MpClipTileViewModel> _hiddenTiles = new List<MpClipTileViewModel>();

        #endregion

        #region Statics

        public static double DefaultBorderWidth = MpMeasurements.Instance.ClipTileMinSize - MpMeasurements.Instance.ClipTileMargin;
        public static double DefaultBorderHeight = MpMeasurements.Instance.ClipTileMinSize;

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

        private MpHighlightTextRangeViewModelCollection _highlightTextRangeViewModelCollection;
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

        
        private ObservableCollection<MpContentItemViewModel> _itemViewModels = new ObservableCollection<MpContentItemViewModel>();
        [MpChildViewModel(typeof(MpContentItemViewModel),true)]
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
                return ItemViewModels.Where(x=>x.CompositeParentCopyItemId == 0).FirstOrDefault();
            }
        }

        public MpContentItemViewModel TailItem {
            get {
                if (ItemViewModels == null || ItemViewModels.Count == 0) {
                    return null;
                }
                return ItemViewModels.OrderByDescending(x => x.CompositeSortOrderIdx).ToList()[0];
            }
        }

        [MpDependsOnChild("IsHovering")]
        public MpContentItemViewModel HoverItem {
            get {
                if (ItemViewModels == null || ItemViewModels.Count == 0) {
                    return null;
                }
                return ItemViewModels.Where(x => x.IsHovering).FirstOrDefault();
            }
        }

        [MpDependsOnChild("IsHovering")]
        public MpContentItemViewModel PrimaryItem {
            get {
                if (ItemViewModels == null || ItemViewModels.Count == 0) {
                    return null;
                }
                if (HoverItem != null) {
                    return HoverItem;
                }

                if (SelectedItem != null) {
                    return SelectedItem;
                }
                return HeadItem;
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
                return ItemViewModels.Where(x => !x.IsPlaceholder).ToList();
            }
        }

        public List<string> FileList {
            get {
                DoCommandSelection();
                var ivml = SelectedItems;
                var cil = ivml.Select(x => x.CopyItem).ToList();
                return MpAsyncHelpers.RunSync<string[]>(()=>MpCopyItemMerger.Instance.MergeFilePaths(cil)).ToList();
            }
        }

        #endregion

        #region Layout

        //public double TileBorderWidth => IsExpanded ? 
        //                    MpMainWindowViewModel.Instance.MainWindowWidth - (MpMeasurements.Instance.AppStateButtonPanelWidth * 2) :
        //                    TileBorderHeight - (MpMeasurements.Instance.ClipTileMargin * 2);
        private double _tileBorderWidth = DefaultBorderWidth;
        public double TileBorderWidth {
            get {
                return _tileBorderWidth;
            }
            set {
                if (_tileBorderWidth != value) {
                    _tileBorderWidth = Math.Max(0, value);
                    OnPropertyChanged(nameof(TileContentWidth));
                    OnPropertyChanged(nameof(TileBorderWidth));
                    //OnPropertyChanged(nameof(TrayX));
                    //ItemViewModels.ForEach(x => x.OnPropertyChanged(nameof(x.EditorHeight)));
                }
            }
        }

        public double TileContentWidth => TileBorderWidth - MpMeasurements.Instance.ClipTileContentMargin - (MpMeasurements.Instance.ClipTileMargin * 2);


        private double _tileBorderHeight = DefaultBorderHeight;
        public double TileBorderHeight {
            get {
                return _tileBorderHeight;
            }
            set {
                if (_tileBorderHeight != value) {
                    _tileBorderHeight = Math.Max(0, value);
                    OnPropertyChanged(nameof(TileBorderHeight));
                    //OnPropertyChanged(nameof(TileBorderWidth));
                    //OnPropertyChanged(nameof(TileContentWidth));
                    OnPropertyChanged(nameof(TileContentHeight));
                    //OnPropertyChanged(nameof(TrayX));
                    ItemViewModels.ForEach(x => x.OnPropertyChanged(nameof(x.EditorHeight)));
                }
            }
        }        

        public double TileContentHeight => TileBorderHeight - TileTitleHeight - MpMeasurements.Instance.ClipTileMargin - MpMeasurements.Instance.ClipTileBorderThickness - TileDetailHeight;


        public double TrayX {
            get {
                if(IsExpanded) {
                    return MpMeasurements.Instance.ClipTileExpandedMargin;
                }
                if(HeadItem == null || IsPinned) {
                    return 0;
                }
                return MpPagingListBoxBehavior.Instance.FindTileOffsetX(QueryOffsetIdx);

                //return QueryOffsetIdx* TileBorderHeight;
            }
        }

        public double PasteTemplateToolbarHeight => MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
               
        

        public double TileTitleHeight => MpMeasurements.Instance.ClipTileTitleHeight;

        
        public double TileDetailHeight => MpMeasurements.Instance.ClipTileDetailHeight;

        public double LoadingSpinnerSize => MpMeasurements.Instance.ClipTileLoadingSpinnerSize;


        //content container
        public Size ContainerSize {
            get {
                var cs = new Size(MpMeasurements.Instance.ClipTileScrollViewerWidth, 0);
                if (Parent == null) {
                    return cs;
                }
                double ch = MpMeasurements.Instance.ClipTileContentHeight;
                if (IsAnyEditingContent) {
                    ch -= MpMeasurements.Instance.ClipTileEditToolbarHeight;
                }
                if (IsAnyPastingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                }
                if (IsAnyEditingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTileEditTemplateToolbarHeight;
                }
                if (Count == 1) {
                    cs.Height = ch;
                } else {
                    double h = IsExpanded ? ExpandedContentSize.Height : UnexpandedSize.Height;
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

        public Size ExpandedContentSize {
            get {
                var ts = new Size(
                MpMeasurements.Instance.ClipTileEditModeMinWidth,
                0);
                foreach (var ivm in ItemViewModels) {
                    var ivs = ivm.ExpandedSize;
                    ts.Width = Math.Max(ts.Width, ivs.Width);
                    ts.Height += ivs.Height;
                }
                //ts.Height += MpMeasurements.Instance.ClipTileDetailHeight + MpMeasurements.Instance.ClipTileTitleHeight;
                return ts;
            }
        }

        public Size UnexpandedSize {
            get {
                return new Size(
                    MpMeasurements.Instance.ClipTileContentMinWidth,
                    MpMeasurements.Instance.ClipTileContentHeight);
            }
        }

        #endregion

        #region Visibility

        public Visibility FrontVisibility { get; set; } = Visibility.Visible;

        public Visibility BackVisibility { get; set; } = Visibility.Collapsed;

        public Visibility SideVisibility { get; set; } = Visibility.Collapsed;

        public ScrollBarVisibility HorizontalScrollbarVisibility {
            get {
                if (Parent == null) {
                    return ScrollBarVisibility.Hidden;
                }
                if (IsExpanded) {
                    if (ExpandedContentSize.Width > ContentWidth) {
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
                    if (ExpandedContentSize.Height > ContainerSize.Height) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public Visibility FlipButtonVisibility {
            get {
                if(IsExpanded) {
                    return Visibility.Hidden;
                }
                return IsSelected || IsHovering ? Visibility.Visible : Visibility.Hidden;
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
                return (Parent.IsScrolling || IsSelected) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility ToggleEditModeButtonVisibility {
            get {
                return ((IsHovering || IsExpanded) &&
                        Parent.SelectedItems.Count == 1) ?
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
                if (MpMainWindowViewModel.Instance == null || Parent == null || !IsSelected) {
                    return Visibility.Visible;
                }
                if (Parent.SelectedItems.Count > 1 &&
                   !IsHovering) {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
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

        public Visibility TrialOverlayVisibility {
            get {
                return MpPreferences.Instance.IsTrialExpired ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility SelectionOverlayGridVisibility {
            get {
                return (IsSelected &&
                       (Parent.SelectedItems.Count == 1 ||
                        Parent.IsAnyTileExpanded)) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility MultiSelectOrderMarkerVisibility {
            get {
                if (MpMainWindowViewModel.Instance == null || Parent == null) {
                    return Visibility.Hidden;
                }
                if (IsSelected && Parent.SelectedItems.Count > 1) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Appearance       

        public Rect TileBorderBrushRect {
            get {
                if (IsAnyItemDragging || IsAnyItemContextMenuOpened) {
                    return MpMeasurements.Instance.DottedBorderRect;
                }
                return MpMeasurements.Instance.SolidBorderRect;
            }
        }

        [MpDependsOnParent("PrimaryItem","SelectedItems")]
        [MpDependsOnSibling("IsSelected")]
        public Brush TileBorderBrush {
            get {
                if(IsResizing) {
                    return Brushes.MediumVioletRed;
                }
                if(CanResize) {
                    return Brushes.Orange;
                }
                if (Parent.PrimaryItem == this &&
                    Parent.SelectedItems.Count > 1) {
                    return Brushes.Blue;
                }
                if (IsSelected) {
                    return Brushes.Red;
                }
                if (Parent.IsScrolling || Parent.HasScrollVelocity) {
                    return Brushes.Transparent;
                }
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }

        public Brush TestBgBrush {
            get {
                return IsAnyBusy ? Brushes.HotPink : Brushes.LightBlue;
            }
        }
        #endregion

        #region State Properties

        public bool IsPinned => Parent != null && HeadItem != null && Parent.PinnedItems.Any(x => x.HeadItem.CopyItemId == HeadItem.CopyItemId);

        public bool CanVerticallyScroll => IsExpanded ?
                                                ExpandedContentSize.Height > TileContentHeight :
                                                ItemViewModels.Sum(x => x.UnformattedContentSize.Height) > TileContentHeight;

        public bool CanResize { get; set; } = false;

        public bool IsResizing { get; set; } = false;

        public int QueryOffsetIdx { get; set; } = 0;

        public bool IsLoading {
            get {
                return HeadItem == null || HeadItem.CopyItem.Id == 0;
            }
        }

        public int TileIdx {
            get {
                if (Parent == null || IsPlaceholder) {
                    return -1;
                }
                return Parent.Items.IndexOf(this);
            }
        }

        public bool IsFileListItem {
            get {
                if (HeadItem == null) {
                    return false;
                }
                return HeadItem.CopyItemType == MpCopyItemType.FileList;
            }
        }

        public bool IsTextItem {
            get {
                if (HeadItem == null) {
                    return false;
                }
                return //HeadItem.CopyItemType == MpCopyItemType.Csv ||
                        HeadItem.CopyItemType == MpCopyItemType.Html ||
                        HeadItem.CopyItemType == MpCopyItemType.RichText;
            }
        }

        public bool IsAnyBusy => ItemViewModels.Any(x => x.IsBusy) || IsBusy;

        public bool IsFlipping { get; set; } = false;

        public bool IsFlipped { get; set; } = false;

        public bool IsDetailGridVisibile {
            get {
                if(Parent.HasScrollVelocity) {
                    return false;
                }

                if (IsExpanded) {
                    if (IsAnyEditingTemplate ||
                        IsAnyPastingTemplate) {
                        return false;
                    }
                } else {
                    if (!IsSelected && !IsHovering) {
                        return false;
                    }
                }
                return true;
            }
        }

        public string MultiSelectedOrderIdxDisplayValue {
            get {
                if (MpMainWindowViewModel.Instance == null || Parent == null || !IsSelected) {
                    return string.Empty;
                }
                int multiIdx = Parent.GetSelectionOrderIdxForItem(this);
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

        //[MpDependsOnChild("IsItemDragging")]
        public bool IsAnyItemDragging => ItemViewModels.Any(x => x.IsItemDragging);


        [MpDependsOnChild("IsContextMenuOpen")]
        public bool IsAnyItemContextMenuOpened => ItemViewModels.Any(x => x.IsContextMenuOpen);

        public bool IsAnySelected => ItemViewModels.Any(x => x.IsSelected);

        public bool HasTemplates => ItemViewModels.Any(x => x.HasTemplates);

        public int Count => ItemViewModels.Count;

        public bool IsAnyEditingContent => ItemViewModels.Any(x => x.IsEditingContent);

        public bool IsAnyEditingTitle => ItemViewModels.Any(x => x.IsEditingTitle);

        public bool IsAnyEditingTemplate => ItemViewModels.Any(x => x.IsEditingTemplate);

        public bool IsAnyPastingTemplate => ItemViewModels.Any(x => x.IsPastingTemplate);

        [MpAffectsSibling]
        public bool IsExpanded { get; set; } = false;

        public DateTime LastSelectedDateTime { get; set; }

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

        public bool AllowMultiSelect { get; set; } = false;

        [MpAffectsChild]
        [MpAffectsSibling]
        [MpDependsOnChild("IsSelected")]
        public bool IsSelected { //get; set; }
            get {
                return ItemViewModels.Any(x => x.IsSelected);
            }
            set {
                if (value) {
                    if (!IsSelected) {
                        if (Parent.IsSelectionReset) {
                            if (HeadItem != null) {
                                HeadItem.IsSelected = value;
                            }
                        } else {
                            SubSelectAll();
                        }
                    }
                } else {
                    SelectedItems.ForEach(x => x.IsSelected = false);
                }
                OnPropertyChanged(nameof(TileBorderBrush));
            }
        }
                

        private bool _isHovering = false;
        [MpDependsOnParent("IsScrolling")]
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(HoverItem));
                    OnPropertyChanged(nameof(PrimaryItem));
                    OnPropertyChanged(nameof(ToggleEditModeButtonVisibility));
                    OnPropertyChanged(nameof(SelectionOverlayGridVisibility));
                    OnPropertyChanged(nameof(TileBorderBrushRect));
                    OnPropertyChanged(nameof(IsDetailGridVisibile));
                }
            }
        }

        [MpDependsOnParent("IsAnyTileExpanded")]
        [MpDependsOnChild("IsPlaceholder")]
        [MpDependsOnSibling("IsExpanded")]
        public bool IsPlaceholder {
            get {
                if(Parent == null || ItemViewModels.Count == 0) {
                    return true;
                }
                if(IsPinned) {
                    return true;
                }
                return Parent.IsAnyTileExpanded && !IsExpanded;
            }
        }

        public int ItemIdx {
            get {
                if(Parent == null) {
                    return 0;
                }
                return Parent.Items.IndexOf(this);
            }
        }

        public bool IsLoadMoreTile { get; set; } = false;

        #endregion

        #region Model
        public MpCopyItemType ItemType {
            get {
                if(PrimaryItem == null) {
                    return MpCopyItemType.None;
                }
                return PrimaryItem.CopyItemType;
            }
        }
        public DateTime CopyItemCreatedDateTime {
            get {
                if (PrimaryItem == null) {
                    return DateTime.MinValue;
                }
                return PrimaryItem.CopyItemCreatedDateTime;
            }
        }
        
        #endregion

        #endregion

        #region Events

        public event EventHandler<string> OnSearchRequest;

        public event EventHandler OnListBoxRefresh;
        public event EventHandler OnUiUpdateRequest;
        public event EventHandler<object> OnScrollIntoViewRequest;
        public event EventHandler OnScrollToHomeRequest;
        public event EventHandler OnFocusRequest;

        #endregion

        #region Static Builder


        #endregion

        #region Constructors

        public MpClipTileViewModel() : base(null) {
            IsBusy = true;
        }

        public MpClipTileViewModel(MpClipTrayViewModel parent) : base(parent) {
            IsBusy = true;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpCopyItem headItem, int queryOffset = -1) {
            PropertyChanged -= MpClipTileViewModel_PropertyChanged;
            PropertyChanged += MpClipTileViewModel_PropertyChanged;
            QueryOffsetIdx = queryOffset;
            IsBusy = true;

            ItemViewModels.Clear();
            if(headItem != null && Parent.PersistentUniqueWidthTileLookup.TryGetValue(headItem.Id, out double uniqueWidth)) {
                TileBorderWidth = uniqueWidth;
            } else {
                TileBorderWidth = DefaultBorderHeight;
            }

            if (headItem != null) {
                var ccil = await MpDataModelProvider.Instance.GetCompositeChildrenAsync(headItem.Id);
                ccil.Insert(0, headItem);

                for (int i = 0; i < ccil.OrderBy(x=>x.CompositeSortOrderIdx).Count(); i++) {
                    ccil[i].CompositeParentCopyItemId = i == 0 ? 0 : ccil[0].Id;
                    ccil[i].CompositeSortOrderIdx = i;
                    var civm = await CreateContentItemViewModel(ccil[i]);
                    ItemViewModels.Add(civm);
                }

                HighlightTextRangeViewModelCollection = new MpHighlightTextRangeViewModelCollection(this);

                RequestUiUpdate();

                MpMessenger.Instance.Send<MpMessageType>(MpMessageType.ContentListItemsChanged, this);
            }

            ItemViewModels.ForEach(y => y.OnPropertyChanged(nameof(y.ItemSeparatorBrush)));
            ItemViewModels.ForEach(y => y.OnPropertyChanged(nameof(y.EditorHeight)));

            OnPropertyChanged(nameof(ItemViewModels));
            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(PrimaryItem));
            OnPropertyChanged(nameof(TrayX));
            OnPropertyChanged(nameof(TileBorderBrush));
            OnPropertyChanged(nameof(CanVerticallyScroll));

            IsBusy = false;
        }

        public async Task<MpContentItemViewModel> CreateContentItemViewModel(MpCopyItem ci) {
            var civm = new MpContentItemViewModel(this);
            await civm.InitializeAsync(ci);
            return civm;
        }

        public async Task UserPreparingDynamicPaste() {
            await Task.Delay(1);
        }

        public void ResetSubSelection(bool clearEditing = true, bool reqFocus = false) {
            ClearSelection(clearEditing);
            Parent.IsSelectionReset = true;
            IsSelected = true;
            Parent.IsSelectionReset = false;
            if(reqFocus) {
                RequestFocus();
            }
        }

        public void ClearSubHovering() {
            foreach (var ivm in ItemViewModels) {
                ivm.IsHovering = false;
            }
        }

        public void SubSelectAll() {
            AllowMultiSelect = true;
            foreach (var ivm in ItemViewModels) {
                ivm.IsSelected = true;
            }
            AllowMultiSelect = false;
        }

        public void DoCommandSelection() {
            //called before all commands (assuming passes CanExecute checks)
            //to select all items if none are selected for tile based commands
            if (!IsSelected) {
                IsSelected = true;
            }
            if (SelectedItems.Count == 0) {
                SubSelectAll();
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
            return HeadItem.GetDetailText(detailType);
        }

        public async Task InsertRange(int idx, List<MpCopyItem> models) {
            await MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                var curModels = ItemViewModels.Where(x => x.CopyItem != null).Select(x => x.CopyItem).ToList();

                idx = idx < 0 ? 0 : idx >= curModels.Count ? curModels.Count : idx;

                curModels.InsertRange(idx, models);
                for (int i = 0; i < curModels.Count; i++) {
                    curModels[i].CompositeSortOrderIdx = i;
                    if (i == 0) {
                        curModels[i].CompositeParentCopyItemId = 0;
                    } else {
                        curModels[i].CompositeParentCopyItemId = curModels[0].Id;
                    }
                    await curModels[i].WriteToDatabaseAsync();
                }

                //only will occur during drag & drop
                await InitializeAsync(curModels[0]);
            });
        }

        public async Task RemoveRange(List<MpCopyItem> models) {
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                for (int i = 0; i < models.Count; i++) {
                    var ivm = ItemViewModels.Where(x => x.CopyItem.Id == models[i].Id).FirstOrDefault();
                    if (ivm != null) {
                        ItemViewModels.Remove(ivm);
                    }
                }
                //if (ItemViewModels.Count == 0) {
                //    IsPlaceholder = true;
                //    Parent.ClipTileViewModels.Move(Parent.ClipTileViewModels.IndexOf(this), Parent.ClipTileViewModels.Count - 1);
                //} else {
                //    UpdateSortOrder();
                //}
            });
        }

        public async Task UpdateSortOrderAsync(bool fromModel = false) {
            if (fromModel) {
                ItemViewModels.Sort(x => x.CompositeSortOrderIdx);
            } else {
                foreach (var ivm in ItemViewModels) {
                    ivm.CompositeSortOrderIdx = ItemViewModels.IndexOf(ivm);
                    if (ivm.CompositeSortOrderIdx == 0) {
                        ivm.CompositeParentCopyItemId = 0;
                    } else {
                        ivm.CompositeParentCopyItemId = ItemViewModels[0].CopyItemId;
                    }
                    await ivm.CopyItem.WriteToDatabaseAsync();
                }
            }
            RequestUiUpdate();
        }

        public async Task ClearContent() {
            await InitializeAsync(null);
        } 

        public void RefreshTile() {
            if(HeadItem == null) {
                return;
            }
            Task.Run(()=>InitializeAsync(HeadItem.CopyItem));
        }

        #region View Event Invokers

        public void RequestListRefresh() {
            OnListBoxRefresh?.Invoke(this, null);
        }

        public void RequestScrollIntoView(object obj) {
            OnScrollIntoViewRequest?.Invoke(this, obj);
        }

        public void RequestScrollToHome() {
            OnScrollToHomeRequest?.Invoke(this, null);
        }

        public void RequestUiUpdate() {
            OnUiUpdateRequest?.Invoke(this, null);
        }

        public void RequestSearch(string st) {
            OnSearchRequest?.Invoke(this, st);
        }

        public void RequestFocus() {
            OnFocusRequest?.Invoke(this, null);
        }

        #endregion


        public async Task GatherAnalytics() {
            var analyticsTask = new List<Task>();
            Task itemTask = null;
            foreach(var ivm in ItemViewModels) {
                itemTask = ivm.GatherAnalytics();
                analyticsTask.Add(itemTask);
            }
            await Task.WhenAll(analyticsTask.ToArray());
        }

        public void ClearSelection(bool clearEditing = true) {
            IsSelected = false;
            LastSelectedDateTime = DateTime.MaxValue;
            if(clearEditing) {
                ClearEditing();
            }
            foreach(var civm in ItemViewModels) {
                civm.IsSelected = false;
            }
        }

        public void ClearEditing() {
            if(IsExpanded) {
                IsExpanded = false;
            }
            foreach(var ivm in ItemViewModels) {
                ivm.ClearEditing();
            }
        }

        public async Task<string> GetSubSelectedPastableRichText(bool isToExternalApp = false) {
            if(IsTextItem) {
                if (SelectedItems.Count == 0) {
                    SubSelectAll();
                }
                bool isPastingTemplate = SelectedItems.Any(x => x.HasTemplates);
                if (isPastingTemplate) {
                    SelectedItems.Where(x => x.HasTemplates).Select(y => y.IsPastingTemplate = true);
                    if (!MpMainWindowViewModel.Instance.IsMainWindowOpen) {
                        MpMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                    }
                    await FillAllTemplates();
                }


                var sw = new Stopwatch();
                sw.Start();
                string rtf = string.Empty.ToRichText();
                foreach (var rtbvm in SelectedItems) {
                    if (rtbvm.HasTemplates) {
                        string rtbvmrtf = rtbvm.TemplateRichText;
                        rtf = MpHelpers.Instance.CombineRichText(rtbvmrtf, rtf, true);
                    } else {
                        rtf = MpHelpers.Instance.CombineRichText(rtbvm.CopyItem.ItemData.ToRichText(), rtf, true);
                    }
                }
                sw.Stop();
                MonkeyPaste.MpConsole.WriteLine(@"Time to combine richtext: " + sw.ElapsedMilliseconds + "ms");

                if(IsExpanded) {
                    IsExpanded = false;
                }
                return rtf;
            }

            return string.Empty;
            //both return to ClipTray.GetDataObjectFromSelectedClips
        }

        public async Task FillAllTemplates() {
            bool hasExpanded = false;
            foreach (var rtbvm in SelectedItems) {
                if (rtbvm.HasTemplates) {
                    rtbvm.IsSelected = true;
                    rtbvm.IsPastingTemplate = true;
                    if (!hasExpanded) {
                        //tile will be shrunk in on completed of hide window
                        IsExpanded = true;
                        rtbvm.OnPropertyChanged(nameof(rtbvm.IsEditingContent));
                        rtbvm.TemplateCollection.UpdateCommandsCanExecute();
                        rtbvm.TemplateCollection.OnPropertyChanged(nameof(rtbvm.TemplateCollection.Templates));
                        rtbvm.TemplateCollection.OnPropertyChanged(nameof(rtbvm.TemplateCollection.HasMultipleTemplates));
                        hasExpanded = true;
                    }
                    rtbvm.TemplateCollection.Templates[0].IsSelected = true;
                    rtbvm.TemplateRichText = null;
                    await Task.Run(async() => {
                        while (string.IsNullOrEmpty(rtbvm.TemplateRichText)) {
                            await Task.Delay(100);
                        }
                    });

                    rtbvm.TemplateCollection.ClearSelection();
                }

            }
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

        public void RefreshAsyncCommands() {
            if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }

            //(Parent.FlipTileCommand as RelayCommand<object>).NotifyCanExecuteChanged();
            //(Parent.PerformHotkeyPasteCommand as RelayCommand<object>).NotifyCanExecuteChanged();
            //(Parent.BringSelectedClipTilesToFrontCommand as RelayCommand).NotifyCanExecuteChanged();
            //(Parent.SendSelectedClipTilesToBackCommand as RelayCommand).NotifyCanExecuteChanged();
            //(Parent.SpeakSelectedClipsCommand as RelayCommand).NotifyCanExecuteChanged();
            //(Parent.MergeSelectedClipsCommand as RelayCommand).NotifyCanExecuteChanged();
            //(Parent.TranslateSelectedClipTextAsyncCommand as RelayCommand<string>).NotifyCanExecuteChanged();
            //(Parent.CreateQrCodeFromSelectedClipsCommand as RelayCommand).NotifyCanExecuteChanged();
        }

        #region IDisposable

        public override void Dispose() {
            base.Dispose();
            PropertyChanged -= MpClipTileViewModel_PropertyChanged;
            ClearSelection();
            ItemViewModels.ForEach(x => x.Dispose());
            ItemViewModels.Clear();
        }

        #endregion

        #endregion

        #region Protected Methods

        #region DB Overrides

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                var civm = ItemViewModels.Where(x => x.CopyItemId == ci.Id);

            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                //DragDrop Cases
                // 1. Item Resorted

                //var civm = ItemViewModels.FirstOrDefault(x => x.CopyItemId == ci.Id);
                //if (civm != null) {
                //    if(ci.CompositeParentCopyItemId == 0) {
                //        if(civm)
                //    }
                //}
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            //throw new NotImplementedException();
        }


        #endregion

        #endregion

        #region Private Methods

        private void MpClipTileViewModel_PropertyChanged(object s, System.ComponentModel.PropertyChangedEventArgs e1) {
            switch (e1.PropertyName) {
                case nameof(IsAnyBusy):
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    }
                    break;
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(IsSelected):
                    if (!IsSelected) {
                        if (IsFlipped) {
                            Parent.FlipTileCommand.Execute(this);
                        }
                        //ClearSelection();
                    } else {
                        LastSelectedDateTime = DateTime.Now;
                        RequestFocus();
                    }
                    ItemViewModels.ForEach(x => x.OnPropertyChanged(nameof(x.ItemSeparatorBrush)));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    break;
                case nameof(IsExpanded):
                    MpMessenger.Instance.Send<MpMessageType>(IsExpanded ? MpMessageType.Expand : MpMessageType.Unexpand, this);

                    ItemViewModels.ForEach(x => x.OnPropertyChanged(nameof(x.IsEditingContent)));
                    MpClipTrayViewModel.Instance.Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsPlaceholder)));
                    OnPropertyChanged(nameof(TileBorderWidth));
                    OnPropertyChanged(nameof(FlipButtonVisibility));

                    Parent.OnPropertyChanged(nameof(Parent.IsAnyTileExpanded));
                    Parent.OnPropertyChanged(nameof(Parent.IsHorizontalScrollBarVisible));
                    Parent.OnPropertyChanged(nameof(Parent.ClipTrayScreenWidth));
                    MpAppModeViewModel.Instance.OnPropertyChanged(nameof(MpAppModeViewModel.Instance.AppModeButtonGridWidth));

                    if (IsExpanded) {
                        Parent.ScrollOffset = Parent.LastScrollOfset = 0;

                        if (SelectedItems.Count == 0) {
                            PrimaryItem.IsSelected = true;
                        }

                        _unexpandedHeight = MpMainWindowViewModel.Instance.MainWindowHeight;
                        MpMainWindowResizeBehavior.Instance.Resize(Math.Max(TileBorderHeight, ExpandedContentSize.Height - TileBorderHeight));

                        Keyboard.AddKeyDownHandler(Application.Current.MainWindow, ExpandedKeyDown_Handler);
                    } else {
                        Keyboard.RemoveKeyDownHandler(Application.Current.MainWindow, ExpandedKeyDown_Handler);
                        MpMainWindowResizeBehavior.Instance.Resize(_unexpandedHeight - MpMainWindowViewModel.Instance.MainWindowHeight);
                    }
                    OnPropertyChanged(nameof(TrayX));

                    MpMessenger.Instance.Send<MpMessageType>(IsExpanded ? MpMessageType.Expand : MpMessageType.Unexpand, this);
                    ItemViewModels.ForEach(x => x.OnPropertyChanged(nameof(x.EditorHeight)));
                    
                    OnPropertyChanged(nameof(CanVerticallyScroll));
                    break;
                case nameof(IsFlipping):
                    if (IsFlipping) {
                        FrontVisibility = Visibility.Collapsed;
                        BackVisibility = Visibility.Collapsed;
                    }
                    break;
                case nameof(IsFlipped):
                    FrontVisibility = IsFlipped ? Visibility.Collapsed : Visibility.Visible;
                    BackVisibility = IsFlipped ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case nameof(IsAnyEditingTemplate):
                    //OnPropertyChanged(nameof(De))
                    break;
                case nameof(TileBorderWidth):
                    if (HeadItem != null && Parent.PersistentUniqueWidthTileLookup.TryGetValue(HeadItem.CopyItemId, out double uniqueWidth)) {
                        //this occurs when mainwindow is resized and user gives tile unique width
                        Parent.PersistentUniqueWidthTileLookup[HeadItem.CopyItemId] = TileBorderWidth;
                    } 
                    break;
            }
        }

        private void ExpandedKeyDown_Handler(object sender, KeyEventArgs e) {
            if(MpDragDropManager.Instance.IsDragAndDrop) {
                return;
            }
            if(e.Key == Key.Escape) {
                ToggleExpandedCommand.Execute(null);
            }
        }
        #endregion

        #region Commands

        public ICommand ScrollUpCommand => new RelayCommand(
             () => {
                 if(SelectedItems.Count == 0) {
                     ResetSubSelection(false);
                 }
                 int selectedIdx = ItemViewModels.IndexOf(SelectedItems[0]);
                 while(selectedIdx >= 0) {
                     if (ItemViewModels[selectedIdx].IsScrolledToHome) {
                         selectedIdx--;
                         if(selectedIdx < 0) {
                             break;
                         }
                         ItemViewModels[selectedIdx].IsSelected = true;
                         ItemViewModels[selectedIdx].ScrollToEndCommand.Execute(null);
                     } else {
                         break;
                     }
                 }
                 if(selectedIdx >= 0) {
                     ItemViewModels[selectedIdx].ScrollUpCommand.Execute(null);
                 }
             },
             ()=>IsSelected);

        public ICommand ScrollDownCommand => new RelayCommand(
            async () => {
                if (SelectedItems.Count == 0) {
                    ResetSubSelection(false);
                }
                int selectedIdx = ItemViewModels.IndexOf(SelectedItems[0]);
                while (selectedIdx >= ItemViewModels.Count - 1) {
                    if (ItemViewModels[selectedIdx].IsScrolledToEnd) {
                        selectedIdx++;
                        if (selectedIdx >= ItemViewModels.Count) {
                            break;
                        }
                        ItemViewModels[selectedIdx].IsSelected = true;
                        ItemViewModels[selectedIdx].ScrollToHomeCommand.Execute(null);
                    } else {
                        break;
                    }
                }
                if (selectedIdx < ItemViewModels.Count) {
                    ItemViewModels[selectedIdx].ScrollDownCommand.Execute(null);
                }
            }, () => IsSelected);

        public ICommand ToggleExpandedCommand => new RelayCommand(
            () => {
                if(!IsSelected && !IsExpanded) {
                    ResetSubSelection(false);
                }
                IsExpanded = !IsExpanded;
            });

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
        //    System.Windows.MpClipboardManager.Instance.SetImageWrapper(bmpSrc);
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
        //    //Parent.ClearClipSelection();
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
        //    return Parent.SelectedItems.Count == 1;
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
        //    Parent.ClearClipSelection();
        //    IsSelected = true;
        //    Parent.PasteSelectedClipsCommand.Execute(args);
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
        //        Parent.HotkeyPasteCommand, CopyItemId);
        //}

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
            if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return false;
            }
            return Parent.SelectedItems.Count == 1 &&
                   SelectedItems.Count <= 1;
        }
        private void EditTitle() {

            PrimaryItem.EditSubTitleCommand.Execute(null);
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
            ClearSelection();
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
            ClearSelection();
            VisibleItems[minItem - 1].IsSelected = true;
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
                !MpPreferences.Instance.IsTrialExpired;
        }
        private void PasteSubSelectedClips(object ptapId) {
            if (ptapId != null && ptapId.GetType() == typeof(int) && (int)ptapId > 0) {
                //when pasting to a user defined application
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = MpPasteToAppPathViewModelCollection.Instance.FindById((int)ptapId);
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
            MpMainWindowViewModel.Instance.HideWindowCommand.Execute(true);
        }

        public ICommand BringToFrontCommand {
            get {
                return new RelayCommand(
                    () => {
                        try {
                            IsBusy = true;
                            MpHelpers.Instance.RunOnMainThread(
                                    (Action)(() => {
                                        var tempSelectedClipTiles = SelectedItems;
                                        ClearSelection();

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
                    },
                    () => {
                        if (IsBusy || MpMainWindowViewModel.Instance.IsMainWindowLoading || VisibleItems.Count == 0) {
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
                    });
            }
        }

        public ICommand SendSubSelectedClipTilesToBackCommand => new RelayCommand(
            () => {
                try {
                    IsBusy = true;
                    var tempSelectedClipTiles = SelectedItems;
                    ClearSelection();

                    foreach (var sctvm in tempSelectedClipTiles) {
                        ItemViewModels.Move(ItemViewModels.IndexOf(sctvm), ItemViewModels.Count - 1);
                        sctvm.IsSelected = true;
                    }
                    RequestScrollIntoView(SelectedItems[SelectedItems.Count - 1]);
                }
                finally {
                    IsBusy = false;
                }
            },
            () => {
                if (IsBusy || MpMainWindowViewModel.Instance.IsMainWindowLoading || VisibleItems.Count == 0) {
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
            });

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
            ClearSelection();
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

        //public ICommand LinkTagToSubSelectedClipsCommand => new RelayCommand<MpTagTileViewModel>(
        //    async (tagToLink) => {
        //        bool isUnlink = tagToLink.IsLinked(SelectedItems[0].CopyItem);
        //        foreach (var srtbvm in SelectedItems) {
        //            if (isUnlink) {
        //                tagToLink.RemoveClip(srtbvm);
        //            } else {
        //                tagToLink.AddClip(srtbvm);
        //            }
        //        }
        //        await MpTagTrayViewModel.Instance.RefreshAllCounts();
        //        await MpTagTrayViewModel.Instance.UpdateTagAssociation();
        //    },
        //    async (tagToLink) => {
        //        //this checks the selected clips association with tagToLink
        //        //and only returns if ALL selecteds clips are linked or unlinked 
        //        if (tagToLink == null || SelectedItems == null || SelectedItems.Count == 0) {
        //            return false;
        //        }
        //        if (SelectedItems.Count == 1) {
        //            return true;
        //        }
        //        bool isLastClipTileLinked = await tagToLink.IsLinked(SelectedItems[0].CopyItem);
        //        foreach (var srtbvm in SelectedItems) {
        //            bool isLinked = await tagToLink.IsLinked(srtbvm);
        //            if (isLinked != isLastClipTileLinked) {
        //                return false;
        //            }
        //        }
        //        return true;
        //    });

        public ICommand AssignHotkeyCommand => new RelayCommand(
            () => {
                SelectedItems[0].AssignHotkeyCommand.Execute(null);
            },
            () => SelectedItems.Count == 1);

        public ICommand InvertSubSelectionCommand => new RelayCommand(
            () => {
                var sctvml = SelectedItems;
                ClearSelection();
                foreach (var vctvm in VisibleItems) {
                    if (!sctvml.Contains(vctvm)) {
                        vctvm.IsSelected = true;
                    }
                }
            },
            SelectedItems.Count != VisibleItems.Count);

        public ICommand SpeakSubSelectedClipsAsyncCommand => new RelayCommand(
            async () => {
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

                    EventHandler<SpeakCompletedEventArgs> speakCompleted = null;
                    speakCompleted = (s, e) => {
                        speechSynthesizer.SpeakCompleted -= speakCompleted;
                        speechSynthesizer.Dispose();
                    };
                    speechSynthesizer.SpeakCompleted += speakCompleted;

                    // Create a PromptBuilder object and append a text string.
                    PromptBuilder promptBuilder = new PromptBuilder();

                    foreach (var sctvm in SelectedItems) {
                        //speechSynthesizer.SpeakAsync(sctvm.CopyItemPlainText);
                        promptBuilder.AppendText(Environment.NewLine + sctvm.CopyItem.ItemData.ToPlainText());
                    }

                    // Speak the contents of the prompt asynchronously.
                    speechSynthesizer.SpeakAsync(promptBuilder);

                }, DispatcherPriority.Background);
            },
            () => {
                return IsTextItem;
            });

        public ICommand DuplicateSubSelectedClipsCommand => new RelayCommand(
            async () => {
                var tempSubSelectedRtbvml = SelectedItems;
                ClearSelection();
                foreach (var srtbvm in tempSubSelectedRtbvml) {
                    var clonedCopyItem = (MpCopyItem)await srtbvm.CopyItem.Clone(true);
                    var rtbvm = await CreateContentItemViewModel(clonedCopyItem);
                    ItemViewModels.Add(rtbvm);
                    rtbvm.IsSelected = true;
                }
            });

        public ICommand SelectAllCommand {
            get {
                return new RelayCommand(
                    () => {
                        SubSelectAll();
                    });
            }
        }

        public ICommand ChangeColorCommand {
            get {
                return new RelayCommand<Brush>(
                    (b) => {
                        DoCommandSelection();
                        ItemViewModels.ForEach(x => x.ChangeColorCommand.Execute(b));
                    });
            }
        }

        public ICommand CopyCommand {
            get {
                return new RelayCommand(
                    () => {
                        DoCommandSelection();
                        var cil = ItemViewModels.Select(x => x.CopyItem).ToList();
                        MpClipboardManager.Instance.CopyItemsToClipboard(cil);
                    });
            }
        }

        #endregion
    }
}
