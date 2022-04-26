namespace MpWpfApp {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using GalaSoft.MvvmLight.CommandWpf;
    using MonkeyPaste;
    using MonkeyPaste.Plugin;
    using System.Speech.Synthesis;

    public class MpClipTileViewModel : 
        MpViewModelBase<MpClipTrayViewModel>, 
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpITextSelectionRange {
        #region Private Variables

        private List<string> _tempFileList = new List<string>();
        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private int _lastQueryOffset = -1;

        #endregion

        #region Statics

        public static double DefaultBorderWidth = MpMeasurements.Instance.ClipTileMinSize - MpMeasurements.Instance.ClipTileMargin;
        public static double DefaultBorderHeight = MpMeasurements.Instance.ClipTileMinSize;

        #endregion

        #region Properties

        #region Property Reflection Referencer
        //public object this[string propertyName] {
        //    get {
        //        // probably faster without reflection:
        //        // like:  return MpPreferences.PropertyValues[propertyName] 
        //        // instead of the following
        //        Type myType = typeof(MpClipTileViewModel);
        //        PropertyInfo myPropInfo = myType.GetProperty(propertyName);
        //        if (myPropInfo == null) {
        //            throw new Exception("Unable to find property: " + propertyName);
        //        }
        //        return myPropInfo.GetValue(this, null);
        //    }
        //    set {
        //        Type myType = typeof(MpClipTileViewModel);
        //        PropertyInfo myPropInfo = myType.GetProperty(propertyName);
        //        myPropInfo.SetValue(this, value, null);
        //    }
        //}
        #endregion

        #region MpITextSelectionRangeViewModel Implementation 

        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }

        public bool IsAllSelected { get; set; }

        #endregion

        #region View Models

        public ObservableCollection<MpContentItemViewModel> Items { get; private set; } = new ObservableCollection<MpContentItemViewModel>();
        //below from content container

        public MpContentItemViewModel HeadItem => Items.FirstOrDefault(x => x.CompositeParentCopyItemId == 0);

        public MpContentItemViewModel TailItem => Items.OrderByDescending(x => x.CompositeSortOrderIdx).Last();

        public MpContentItemViewModel LastSubSelectedItem => Items.Aggregate((a, b) => a.LastSubSelectedDateTime > b.LastSubSelectedDateTime ? a : b);

        //[MpDependsOnChild("IsHovering")]
        public MpContentItemViewModel HoverItem => Items.FirstOrDefault(x => x.IsHovering);

        //[MpDependsOnChild("IsHovering")]
        public MpContentItemViewModel PrimaryItem {
            get {
                if (Items == null || Items.Count == 0) {
                    return null;
                }

                if (SelectedItem != null) {
                    return SelectedItem;
                }

                if (HoverItem != null) {
                    return HoverItem;
                }

                return LastSubSelectedItem;
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
                return Items.Where(x => x.IsSelected == true).OrderBy(x => x.LastSubSelectedDateTime).ToList();
            }
        }

        public List<MpContentItemViewModel> VisibleItems {
            get {
                return Items.Where(x => !x.IsPlaceholder).ToList();
            }
        }

        public List<string> FileList {
            get {
                DoCommandSelection();
                var ivml = SelectedItems;
                var cil = ivml.Select(x => x.CopyItem).ToList();
                return MpCopyItemMerger.Instance.MergeFilePaths(cil).ToList();
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
                    //Items.ForEach(x => x.OnPropertyChanged(nameof(x.EditorHeight)));
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
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.EditorHeight)));
                }
            }
        }        

        public double TileContentHeight => TileBorderHeight - TileTitleHeight - MpMeasurements.Instance.ClipTileMargin - MpMeasurements.Instance.ClipTileBorderThickness - TileDetailHeight;


        public double TrayX {
            get {
                if(HeadItem == null || IsPinned || Parent == null) {
                    return 0;
                }
                return Parent.FindTileOffsetX(QueryOffsetIdx);

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
                double ch = MpMeasurements.Instance.ClipTileContentDefaultHeight;
                if (!IsContentReadOnly) {
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
                    double h = IsContentReadOnly ? ReadOnlyContentSize.Height : EditableContentSize.Height;
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

        public Size EditableContentSize {
            get {
                var ts = new Size(MpMeasurements.Instance.ClipTileEditModeMinWidth,0);
                foreach (var ivm in Items) {
                    var ivs = ivm.EditableContentSize;
                    ts.Width = Math.Max(ts.Width, ivs.Width);
                    if(Parent.PersistentUniqueWidthTileLookup.ContainsKey(HeadItem.CopyItemId)) {
                        if(Parent.PersistentUniqueWidthTileLookup[HeadItem.CopyItemId] > ts.Width) {
                            ts.Width = Parent.PersistentUniqueWidthTileLookup[HeadItem.CopyItemId];
                        }
                    }
                    ts.Height += ivs.Height;
                }
                //ts.Height += MpMeasurements.Instance.ClipTileDetailHeight + MpMeasurements.Instance.ClipTileTitleHeight;
                return ts;
            }
        }

        public Size ReadOnlyContentSize {
            get {
                return new Size(
                    MpMeasurements.Instance.ClipTileContentDefaultWidth,
                    MpMeasurements.Instance.ClipTileContentDefaultHeight);
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
                if (!IsContentReadOnly) {
                     if (EditableContentSize.Width > ContentWidth) {
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
                if (!IsContentReadOnly) {
                    if (EditableContentSize.Height > ContainerSize.Height) {
                        return ScrollBarVisibility.Visible;
                    }
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public Visibility PinButtonVisibility {
            get {
                return IsSelected || IsHovering ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility ToolTipVisibility {
            get {
                if (HeadItem == null || !MpPreferences.ShowItemPreview) {
                    return Visibility.Collapsed;
                }
                return (Parent.IsScrolling || IsSelected) ? Visibility.Collapsed : Visibility.Visible;
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
                return MpPreferences.IsTrialExpired ? Visibility.Visible : Visibility.Collapsed;
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

        public string CaretBrushHexColor {
            get {
                if(IsContentReadOnly) {
                    return MpSystemColors.Red;
                }
                return MpSystemColors.Black;
            }
        }

        public Size UnformattedAndDecodedContentSize {
            get {
                if(IsPlaceholder) {
                    return new Size();
                }
                return HeadItem.UnformattedContentSize;
            }
        }

        public string PinIconSourcePath {
            get {
                string path = "PinIcon";
                if(IsPinned) {
                    if(IsOverPinButton) {
                        path = "PinDownOverIcon";
                    } else {
                        path = "PinDownIcon";
                    }
                } else {
                    if(IsOverPinButton) {
                        path = "PinOverIcon";
                    }
                }
                return Application.Current.Resources[path] as string;
            }
        }

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
                    return Brushes.Pink;
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

        #endregion

        #region State 

        public bool IsSubSelectionEnabled { get; set; } = false;

        public bool IsContentFocused { get; set; } = false;

        public bool IsOverPinButton { get; set; } = false;

        public bool IsPinned => Parent != null && 
                                HeadItem != null && 
                                Parent.PinnedItems.Any(x => x.HeadItem.CopyItemId == HeadItem.CopyItemId);

        public bool CanVerticallyScroll => !IsContentReadOnly ?
                                                EditableContentSize.Height > TileContentHeight :
                                                Items.Sum(x => x.UnformattedContentSize.Height) > TileContentHeight;

        public bool CanResize { get; set; } = false;

        public bool IsResizing { get; set; } = false;

        public int QueryOffsetIdx { 
            get {
                if(IsPlaceholder) {
                    return -1;
                }
                return MpDataModelProvider.AllFetchedAndSortedCopyItemIds.FastIndexOf(HeadItem.CopyItemId);
            }
        }

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
                        //HeadItem.CopyItemType == MpCopyItemType.Html ||
                        HeadItem.CopyItemType == MpCopyItemType.Text;
            }
        }

        public bool IsAnyBusy => Items.Any(x => x.IsBusy) || IsBusy;

        public bool IsFlipping { get; set; } = false;

        public bool IsFlipped { get; set; } = false;

        public bool IsDetailGridVisibile {
            get {
                if(Parent.HasScrollVelocity) {
                    return false;
                }

                if (!IsContentReadOnly) {
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
        public bool IsAnyItemDragging => Items.Any(x => x.IsItemDragging);


        [MpDependsOnChild("IsContextMenuOpen")]
        public bool IsAnyItemContextMenuOpened => Items.Any(x => x.IsContextMenuOpen);

        public bool IsAnySelected => Items.Any(x => x.IsSelected);

        public bool HasTemplates => Items.Any(x => x.HasTemplates);

        public int Count => Items.Count;

        public bool IsContentReadOnly { get; set; } = true;

        public bool IsTitleReadOnly => Items.All(x => x.IsTitleReadOnly);

        public bool IsContentAndTitleReadOnly => IsContentReadOnly && IsTitleReadOnly;
        //public bool IsAnyEditingContent => Items.Any(x => x.IsEditingContent);

       // public bool IsAnyEditingTitle => Items.Any(x => x.IsEditingTitle);

        public bool IsAnyEditingTemplate => Items.Any(x => x.IsEditingTemplate);

        public bool IsAnyPasting => Items.Any(x => x.IsPasting);
        public bool IsAnyPastingTemplate => Items.Any(x => x.IsPasting && x.HasTemplates);

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
                return Items.Any(x => x.IsSelected);
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
                    OnPropertyChanged(nameof(TileBorderBrushRect));
                    OnPropertyChanged(nameof(IsDetailGridVisibile));
                }
            }
        }


        [MpDependsOnChild("IsPlaceholder")]
        public bool IsPlaceholder {
            get {
                if (Parent == null || Items.Count == 0 ||  HeadItem == null || IsPinned) {
                    return true;
                }
                return false;
                //return RootCopyItem == null;
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

        public string MergedItemData {
            get {
                return "This is a test".ToRichText();
            }
        }

        //public MpCopyItem RootCopyItem { get; private set; }
        
        #endregion

        #endregion

        #region Events

        public event EventHandler OnUiUpdateRequest;
        public event EventHandler OnScrollToHomeRequest;
        public event EventHandler OnFocusRequest;
        public event EventHandler OnSyncModels;

        public event EventHandler<Point> OnScrollOffsetRequest;
        public event EventHandler<object> OnPastePortableDataObject;
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

        public async Task InitializeAsync(List<MpCopyItem> items, int queryOffset = -1) {
            PropertyChanged -= MpClipTileViewModel_PropertyChanged;
            PropertyChanged += MpClipTileViewModel_PropertyChanged;

            //QueryOffsetIdx = queryOffset < 0 ? QueryOffsetIdx : queryOffset;
            IsBusy = true;

            if(items != null && items.Count > 0 && !string.IsNullOrEmpty(items[0].RootCopyItemGuid)) {
                // NOTE this ensures root item is first so it becomes the head
                var headItem = items.FirstOrDefault(x => string.IsNullOrEmpty(x.RootCopyItemGuid));
                if(headItem == null) {
                    throw new Exception("There should be a root item...");
                }
                items.Remove(headItem);
                items.Insert(0, headItem);
            }
            if( items != null && items.Count > 0 && queryOffset >= 0) {
                if(MpDataModelProvider.AllFetchedAndSortedCopyItemIds.FastIndexOf(items[0].Id) != queryOffset) {
                    Debugger.Break();
                }
            }
            Items.Clear();
            if (items != null && items.Count > 0 && Parent.PersistentUniqueWidthTileLookup.TryGetValue(items[0].Id, out double uniqueWidth)) {
                TileBorderWidth = uniqueWidth;
            } else {
                TileBorderWidth = DefaultBorderHeight;
            }

            if (items != null && items.Count > 0) {
                for (int i = 0; i < items.Count; i++) {
                    if(i > 0 && string.IsNullOrEmpty(items[i].RootCopyItemGuid)) {
                        MpConsole.WriteLine("warning, initializing tile w/ head item " + items[0].CopyItemSourceGuid + " fragment item " + items[i].Guid + " did not have root guid set, fixing...");
                        items[i].RootCopyItemGuid = items[0].Guid;
                        await items[i].WriteToDatabaseAsync();
                    }
                    if (i > 0 && items[i].CompositeParentCopyItemId == 0 && !string.IsNullOrEmpty(items[i].RootCopyItemGuid)) {
                        MpConsole.WriteLine("warning, initializing tile w/ head item " + items[0].CopyItemSourceGuid + " fragment item " + items[i].Guid + " did not have parent id set, fixing...");
                        items[i].CompositeParentCopyItemId = items[0].Id;
                        await items[i].WriteToDatabaseAsync();
                    }
                    if (i > 0 && items[i].CompositeSortOrderIdx == 0) {
                        MpConsole.WriteLine("warning, initializing tile w/ head item " + items[0].CopyItemSourceGuid + " fragment item " + items[i].Guid + " did not have non-zero sort idx, fixing...");
                        items[i].CompositeSortOrderIdx = i;
                        await items[i].WriteToDatabaseAsync();
                    }

                     var civm = await CreateContentItemViewModel(items[i]);
                    Items.Add(civm);
                }
                OnPropertyChanged(nameof(QueryOffsetIdx)); 
                RequestUiUpdate();

                MpMessenger.Send<MpMessageType>(MpMessageType.ContentItemsChanged, this);
            }

            Items.ForEach(y => y.OnPropertyChanged(nameof(y.ItemSeparatorBrush)));
            Items.ForEach(y => y.OnPropertyChanged(nameof(y.EditorHeight)));

            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(PrimaryItem));
            OnPropertyChanged(nameof(TrayX));
            OnPropertyChanged(nameof(TileBorderBrush));
            OnPropertyChanged(nameof(CanVerticallyScroll));
            OnPropertyChanged(nameof(HeadItem));
            if (HeadItem != null) {
                // BUG titles not updating on load more
                HeadItem.OnPropertyChanged(nameof(HeadItem.CopyItemTitle));
            }
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
            foreach (var ivm in Items) {
                ivm.IsHovering = false;
            }
        }

        public void SubSelectAll() {
            AllowMultiSelect = true;
            foreach (var ivm in Items) {
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
            foreach (var rtbvm in Items) {
                if (rtbvm.CopyItem.Id == copyItemId) {
                    return rtbvm;
                }
            }
            return null;
        }

        public MpContentItemViewModel GetContentItemByCopyItemId(int ciid) {
            return Items.Where(x => x.CopyItem.Id == ciid).FirstOrDefault();
        }

        public string GetDetailText(MpCopyItemDetailType detailType) {
            return HeadItem.GetDetailText(detailType);
        }

        public async Task UpdateSortOrderAsync(bool fromModel = false) {
            if (fromModel) {
                Items.Sort(x => x.CompositeSortOrderIdx);
            } else {
                for(int i = 0;i < Items.Count;i++) {
                    var ivm = Items[i];
                    ivm.CompositeSortOrderIdx = Items.IndexOf(ivm);
                    if (ivm.CompositeSortOrderIdx == 0) {
                        ivm.CompositeParentCopyItemId = 0;
                    } else {
                        ivm.CompositeParentCopyItemId = Items[0].CopyItemId;
                    }
                    await ivm.CopyItem.WriteToDatabaseAsync();
                }
            }
            RequestUiUpdate();
        }

        #region View Event Invokers

        public void RequestListRefresh() {
            CollectionViewSource.GetDefaultView(Items).Refresh();
        }

        public void RequestScrollToHome() {
            OnScrollToHomeRequest?.Invoke(this, null);
        }

        public void RequestUiUpdate() {
            OnUiUpdateRequest?.Invoke(this, null);
        }

        public void RequestFocus() {
            OnFocusRequest?.Invoke(this, null);
        }

        public void RequestSyncModel() {
            OnSyncModels?.Invoke(this, null);
        }

        public void RequestScrollOffset(Point p) {
            OnScrollOffsetRequest?.Invoke(this, p);
        }

        public void RequestPastePortableDataObject(object portableDataObjectOrCopyItem) {
            OnPastePortableDataObject?.Invoke(this, portableDataObjectOrCopyItem);
        }

        #endregion

        public void ClearSelection(bool clearEditing = true) {
            IsSelected = false;
            LastSelectedDateTime = DateTime.MaxValue;
            if(clearEditing) {
                ClearEditing();
            }
            foreach(var civm in Items) {
                civm.IsSelected = false;
            }
        }

        public void ClearEditing() {
            Items.ForEach(x => x.ClearEditing());
            OnPropertyChanged(nameof(IsContentReadOnly));
        }

        public async Task<string> GetSubSelectedPastableRichText(bool isToExternalApp = false) {
            if(IsTextItem) {
                if (SelectedItems.Count == 0) {
                    SubSelectAll();
                }
                bool isPastingTemplate = SelectedItems.Any(x => x.HasTemplates);
                if (isPastingTemplate) {
                    SelectedItems.Where(x => x.HasTemplates).ForEach(y => y.IsPasting = true);
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
                        rtf = rtf.ToFlowDocument().Combine(rtbvmrtf.ToFlowDocument(),null, true).ToRichText();
                    } else {
                        rtf = rtf.ToFlowDocument().Combine(rtbvm.CopyItem.ItemData.ToFlowDocument(),null, true).ToRichText();
                    }
                }
                sw.Stop();
                MpConsole.WriteLine(@"Time to combine richtext: " + sw.ElapsedMilliseconds + "ms");

                if (!IsContentReadOnly) {
                    ClearEditing();
                }
                return rtf;
            }

            return string.Empty;
            //both return to ClipTray.GetDataObjectFromSelectedClips
        }

        public async Task FillAllTemplates() {
            bool hasExpanded = false;
            foreach (var rtbvm in SelectedItems) {
                rtbvm.IsPasting = true;
                if (rtbvm.HasTemplates) {
                    rtbvm.IsSelected = true;
                    if (!hasExpanded) {
                        //tile will be shrunk in on completed of hide window
                        rtbvm.Parent.IsContentReadOnly = false;
                        //rtbvm.OnPropertyChanged(nameof(rtbvm.IsEditingContent));
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
            Items.ForEach(x => x.Dispose());
            Items.Clear();
        }

        #endregion

        #endregion

        #region Protected Methods

        #region DB Overrides

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if(Items.Any(x=>x.CopyItemId == ci.CompositeParentCopyItemId || ci.Id == x.CompositeParentCopyItemId)) {

                }
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci && Items.Any(x => x.CopyItemId == ci.Id)) {
                //MpContentItemViewModel itemToRemove = null;
                //if (Parent.GetClipTileViewModelByGuid(ci.Guid) != this) {
                //    itemToRemove = Items.FirstOrDefault(x => x.CopyItemId == ci.Id);
                //}
                //Items.Remove(itemToRemove);

                //if (Items.Count == 0) {
                //    OnPropertyChanged(nameof(IsPlaceholder));
                //    MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                //} else {
                //    RequestListRefresh();
                //}
            }
        }

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpCopyItem ci && Items.Any(x=>x.CopyItemId == ci.Id)) {                
                var rcivm = Items.FirstOrDefault(x => x.CopyItemId == ci.Id);
                Items.Remove(rcivm);

                if(Items.Count == 0) {
                    int qIdx = Parent.Items.IndexOf(this);
                    await MpDataModelProvider.RemoveQueryItem(ci.Id);
                    MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                    while(Parent.IsBusy) {
                        await Task.Delay(100);
                    }
                    if(qIdx < Parent.Items.Count - 1) {
                        Parent.Items[qIdx].IsSelected = true;
                    } else if(Parent.Items.Count > 0) {
                        Parent.Items[qIdx-1].IsSelected = true;
                    }
                    
                } else {
                    RequestListRefresh();
                }
            }
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
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        //RequestFocus();
                        if (IsPinned) {
                            Parent.ClearClipSelection(false);
                        } else {
                            Parent.ClearPinnedSelection(false);
                            Parent.RequestScrollIntoView(this);
                        }                        
                    } else {
                        if (IsFlipped) {
                            Parent.FlipTileCommand.Execute(this);
                        }
                        if(IsContentReadOnly) {
                            IsSubSelectionEnabled = false;
                        }
                        //ClearSelection();
                    }
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.ItemSeparatorBrush)));
                    OnPropertyChanged(nameof(TileBorderBrush));
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
                case nameof(CanResize):
                    OnPropertyChanged(nameof(TileBorderBrush));
                    break;
                case nameof(TileBorderWidth):
                    if (HeadItem != null && Parent.PersistentUniqueWidthTileLookup.TryGetValue(HeadItem.CopyItemId, out double uniqueWidth)) {
                        //this occurs when mainwindow is resized and user gives tile unique width
                        Parent.PersistentUniqueWidthTileLookup[HeadItem.CopyItemId] = TileBorderWidth;
                    } 
                    break;
                case nameof(IsOverPinButton):
                case nameof(IsPinned):
                    OnPropertyChanged(nameof(PinIconSourcePath));
                    OnPropertyChanged(nameof(IsPlaceholder));
                    break;
                case nameof(IsContentReadOnly):
                    //Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsContentReadOnly)));
                    MpMessenger.Send<MpMessageType>(IsContentReadOnly ? MpMessageType.IsReadOnly : MpMessageType.IsEditable, this);

                    //Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsEditingContent)));
                    //MpClipTrayViewModel.Instance.Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsPlaceholder)));
                    //OnPropertyChanged(nameof(TileBorderWidth));
                    //OnPropertyChanged(nameof(PinButtonVisibility));

                    //Parent.OnPropertyChanged(nameof(Parent.IsAnyTileExpanded));
                    Parent.OnPropertyChanged(nameof(Parent.IsHorizontalScrollBarVisible));

                    OnPropertyChanged(nameof(HorizontalScrollbarVisibility));
                    OnPropertyChanged(nameof(VerticalScrollbarVisibility));
                    //if (IsContentReadOnly) {
                    //    Items.ForEach(x => x.ClearEditing());
                    //}
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.EditorHeight)));

                    OnPropertyChanged(nameof(CanVerticallyScroll));
                    IsSubSelectionEnabled = !IsContentReadOnly;
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(PrimaryItem));
                    break;
            }
        }


        private void ExpandedKeyDown_Handler(object sender, KeyEventArgs e) {
            if(MpDragDropManager.IsDragAndDrop) {
                return;
            }
             if(e.Key == Key.Escape) {
                //ToggleReadOnlyCommand.Execute(null);
                ClearEditing();
            }
        }
        #endregion

        #region Commands

        public ICommand SendSubSelectedToEmailCommand => new RelayCommand(
            () => {
                MpHelpers.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, HeadItem.CopyItemTitle, HeadItem.CopyItemData.ToPlainText()));
            });

        public ICommand ScrollUpCommand => new RelayCommand(
             () => {
                 if(SelectedItems.Count == 0) {
                     ResetSubSelection(false);
                 }
                 int selectedIdx = Items.IndexOf(SelectedItems[0]);
                 while(selectedIdx >= 0) {
                     if (Items[selectedIdx].IsScrolledToHome) {
                         selectedIdx--;
                         if(selectedIdx < 0) {
                             break;
                         }
                         Items[selectedIdx].IsSelected = true;
                         Items[selectedIdx].ScrollToEndCommand.Execute(null);
                     } else {
                         break;
                     }
                 }
                 if(selectedIdx >= 0) {
                     Items[selectedIdx].ScrollUpCommand.Execute(null);
                 }
             },
             ()=>IsSelected);

        public ICommand ScrollDownCommand => new RelayCommand(
            () => {
                if (SelectedItems.Count == 0) {
                    ResetSubSelection(false);
                }
                int selectedIdx = Items.IndexOf(SelectedItems[0]);
                while (selectedIdx >= Items.Count - 1) {
                    if (Items[selectedIdx].IsScrolledToEnd) {
                        selectedIdx++;
                        if (selectedIdx >= Items.Count) {
                            break;
                        }
                        Items[selectedIdx].IsSelected = true;
                        Items[selectedIdx].ScrollToHomeCommand.Execute(null);
                    } else {
                        break;
                    }
                }
                if (selectedIdx < Items.Count) {
                    Items[selectedIdx].ScrollDownCommand.Execute(null);
                }
            }, () => IsSelected);

        //public ICommand ToggleReadOnlyCommand => new RelayCommand(
        //    () => {
        //        if(!IsSelected && IsReadOnly) {
        //            ResetSubSelection(false);
        //        }
        //        IsReadOnly = !IsReadOnly;
        //    });

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
            return MpMainWindowViewModel.Instance.IsShowingDialog == false &&
                IsContentReadOnly &&
                IsTitleReadOnly &&
                !IsAnyPastingTemplate &&
                !MpPreferences.IsTrialExpired;
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
            MpHelpers.OpenUrl(args.ToString() + System.Uri.EscapeDataString(HeadItem.CopyItem.ItemData.ToPlainText()));
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
            return MpMainWindowViewModel.Instance.IsShowingDialog == false &&
                IsContentReadOnly &&
                IsTitleReadOnly &&
                !IsAnyPastingTemplate;
        }
        private void DeleteSubSelectedClips() {
            int lastSelectedClipTileIdx = -1;
            foreach (var ct in SelectedItems) {
                lastSelectedClipTileIdx = VisibleItems.IndexOf(ct);
                Items.Remove(ct);
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

        public ICommand RefreshDocumentCommand {
            get {
                return new RelayCommand(
                    () => {
                        RequestSyncModel();
                        //MessageBox.Show(TemplateCollection.ToString());
                    },
                    () => {
                        return true;// HasModelChanged
                    });
            }
        }
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
                    if (!string.IsNullOrEmpty(MpPreferences.SpeechSynthVoiceName)) {
                        var voice = speechSynthesizer.GetInstalledVoices().Where(x => x.VoiceInfo.Name.ToLower().Contains(MpPreferences.SpeechSynthVoiceName.ToLower())).FirstOrDefault();
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
                    Items.Add(rtbvm);
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
                        Items.ForEach(x => x.ChangeColorCommand.Execute(b));
                    });
            }
        }

        public ICommand CopyCommand {
            get {
                return new RelayCommand(
                    () => {
                        DoCommandSelection();
                        var cil = Items.Select(x => x.CopyItem).ToList();
                        //MpClipboardManager.Instance.CopyItemsToClipboard(cil);
                    });
            }
        }


        #endregion
    }
}
