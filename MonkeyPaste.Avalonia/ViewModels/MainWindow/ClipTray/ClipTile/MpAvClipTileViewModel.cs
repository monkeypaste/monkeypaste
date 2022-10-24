using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileViewModel : MpViewModelBase<MpAvClipTrayViewModel>,
        MpISelectableViewModel,
        MpISelectorItemViewModel,
        MpIShortcutCommandViewModel,
        MpIScrollIntoView,
        MpIUserColorViewModel,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpIRtfSelectionRange,
        MpIContextMenuViewModel,
        //MpIFindAndReplaceViewModel,
        MpITooltipInfoViewModel,
        MpISizeViewModel {

        #region Private Variables

        private List<string> _tempFileList = new List<string>();
        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        //private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;


        private DispatcherTimer _timer;

        private int _detailIdx = 0;

        private string _originalTitle;

        #endregion

        #region Properties

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation
        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        public MpISelectorViewModel Selector => Parent;

        #endregion

        #region MpIContextMenuViewModel Implementation

        public MpMenuItemViewModel ContextMenuViewModel => IsSelected ? Parent.ContextMenuViewModel : null;

        #endregion

        #region MpIScrollIntoView Implementation

        void MpIScrollIntoView.ScrollIntoView() {
            if(Parent == null) {
                return;
            }
            Parent.ScrollIntoView(this);
        }

        #endregion

        //#region MpISelectorItemViewModel<MpAvClipTileViewModel> Implementation
        //MpISelectorViewModel<MpAvClipTileViewModel> MpISelectorItemViewModel<MpAvClipTileViewModel>.Selector => Parent;

        //#endregion

        #region Appearance

        public int[] TitleLayerZIndexes { get; private set; } = Enumerable.Range(1, 3).ToArray();
        public string[] TitleLayerHexColors { get; private set; } = Enumerable.Repeat(MpSystemColors.Transparent, 4).ToArray();


        public string TileBorderHexColor {
            get {
                if (IsResizing) {
                    return MpSystemColors.pink;
                }
                if (CanResize) {
                    return MpSystemColors.orange1;
                }
                if (IsSelected) {
                    return MpSystemColors.Red;//.AdjustAlpha(0.7);
                }
                if (Parent.HasScrollVelocity) {
                    return MpSystemColors.Transparent;
                }
                if (IsHovering) {
                    return MpSystemColors.Yellow;//.AdjustAlpha(0.7);
                }
                return MpSystemColors.Transparent;
            }
        }

        public string TileBackgroundHexColor {
            get {
                if (IsTitleVisible) {
                    return MpSystemColors.oldlace.AdjustAlpha(MpPrefViewModel.Instance.MainWindowOpacity);
                }
                return CopyItemHexColor.AdjustAlpha(MpPrefViewModel.Instance.MainWindowOpacity);
            }
        }

        public string DetailTextHexColor {
            get {
                if (IsSelected || IsHovering) {
                    return MpSystemColors.Black;//MpSystemColors.DimGray;
                }

                return MpSystemColors.Transparent;
            }
        }

        public string TileTitleTextGridBackgroundHexColor {
            get {
                if (IsHoveringOnTitleTextGrid && !IsTitleReadOnly) {
                    return MpSystemColors.White.AdjustAlpha(0.25);
                }
                return MpSystemColors.Transparent;
            }
        }

        public string TitleTextColor {
            get {
                if (IsHoveringOnTitleTextGrid) {
                    return MpSystemColors.DarkGray;
                }
                return MpSystemColors.White;
            }
        }

        public string SelectedTextHexColor {
            get {
                if (ItemType == MpCopyItemType.Text) {
                    return MpSystemColors.lightblue;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string InactiveSelectedTextHexColor {
            get {
                if (ItemType == MpCopyItemType.Text) {
                    return MpSystemColors.purple;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string CaretBrushHexColor {
            get {
                if (IsContentReadOnly) {
                    if (IsSubSelectionEnabled || IsFindAndReplaceVisible) {
                        return MpSystemColors.Red;
                    }
                    return MpSystemColors.Transparent;
                }
                return MpSystemColors.Black;
            }
        }


        public string PinIconSourcePath {
            get {
                string path = "PinImage";
                if (IsPinned) {
                    if (IsOverPinButton) {
                        path = "PinDownOverImage";
                    } else {
                        path = "PinDownImage";
                    }
                } else {
                    if (IsOverPinButton) {
                        path = "PinOverImage";
                    }
                }
                return MpPlatformWrapper.Services.PlatformResource.GetResource(path) as string;
            }
        }

        public string HideTitleIconSourcePath {
            get {
                string path = "OpenEyeImage";
                if (IsTitleVisible) {
                    if (IsOverHideTitleButton) {
                        path = "OpenEyeImage";
                    } else {
                        path = "OpenEyeImage";
                    }
                } else {
                    if (IsOverPinButton) {
                        path = "ClosedEyeImage";
                    } else {
                        path = "ClosedEyeImage";
                    }
                }
                return MpPlatformWrapper.Services.PlatformResource.GetResource(path) as string;
            }
        }

        public double TileBorderBrushTranslateOffsetX { get; set; }

        public Rect TileBorderBrushRect {
            get {
                if (IsTileDragging || IsContextMenuOpen) {
                    return new Rect(); //MpMeasurements.Instance.DottedBorderRect;
                }
                return new Rect(); //MpMeasurements.Instance.SolidBorderRect;
            }
        }


        #endregion

        #region Layout

        public MpRect ObservedBounds { get; set; }
        public double OuterSpacing => 5;
        public double InnerSpacing => 0;
        public MpSize MinSize => Parent == null ? MpSize.Empty : Parent.DefaultItemSize;

        public double MinWidth => MinSize.Width;
        public double MinHeight => MinSize.Height;


        public double MaxWidth => double.PositiveInfinity;// Parent.ClipTrayScreenWidth - 50;
        public double MaxHeight => double.PositiveInfinity;// Parent.ClipTrayScreenHeight - 50;

        private double _titleHeight = 0;
        public double TitleHeight {
            get => _titleHeight;
            set {
                if(TitleHeight != value) {
                    _titleHeight = value;
                    OnPropertyChanged(nameof(TitleHeight));
                }
            }
        }
        public double MaxTitleHeight => 40;

        public double TitleFontSize => Math.Max(8,(TitleHeight / 2) - 2);
        //public double TrayX {
        //    get {
        //        double trayX = 0;
        //        if (Parent == null) {
        //            return trayX;
        //        }
        //        trayX = MinSize * ColIdx;

        //        return trayX;
        //    }
        //}

        //private double _trayX;
        //public double TrayX {
        //    get => _trayX;
        //    set {
        //        if (_trayX != value) {
        //            _trayX = value;
        //            OnPropertyChanged(nameof(TrayX));
        //        }
        //    }
        //}

        //public double TrayY {
        //    get {
        //        double trayY = 0;
        //        if (Parent == null) {
        //            return trayY;
        //        }
        //        trayY = MinSize * RowIdx;
        //        return trayY;
        //    }
        //}

        //private double _trayY;
        //public double TrayY {
        //    get => _trayY;
        //    set {
        //        if (_trayY != value) {
        //            _trayY = value;
        //            OnPropertyChanged(nameof(TrayY));
        //        }
        //    }
        //}

        public double TrayX => TrayLocation.X;
        public double TrayY => TrayLocation.Y;
        public MpPoint TrayLocation { get; set; } = MpPoint.Zero;

        public double ObservedWidth { get; set; }
        public double ObservedHeight { get; set; }
        public double BoundWidth {
            get => BoundSize.Width;
            set {
                if (BoundWidth != value) {
                    BoundSize.Width = value;
                    OnPropertyChanged(nameof(BoundSize));
                }
            }
        }
        public double BoundHeight {
            get => BoundSize.Height;
            set {
                if(BoundHeight != value) {
                    BoundSize.Height = value;
                    OnPropertyChanged(nameof(BoundSize));
                }
            }
        }
        public MpSize BoundSize { get; set; } = MpSize.Empty;

        public MpRect TrayRect => new MpRect(TrayLocation,BoundSize);

        public MpRect ScreenRect => Parent == null ? MpRect.Empty : new MpRect(TrayLocation - Parent.ScrollOffset, BoundSize);


        public double ReadOnlyWidth => MinWidth;
        public double ReadOnlyHeight => MinHeight;
        public MpSize ReadOnlySize => new MpSize(ReadOnlyWidth, ReadOnlyHeight);

        public double EditableWidth {
            get {
                if (Parent == null) {
                    return 0;
                }
                double w = Parent.DefaultEditableItemWidth;// Math.Min(Parent.DefaultEditableItemWidth, UnformattedContentSize.Width);
                return w;
            }
        }

        public double EditableHeight {
            get {
                if (Parent == null) {
                    return 0;
                }
                return Parent.DefaultEditableItemSize.Height;
            }
        }
        public MpSize EditableSize => new MpSize(EditableWidth, EditableHeight);

        public MpRect ParentScreenRect {
            get {
                if(Parent == null || IsPlaceholder) {
                    return MpRect.Empty;
                }
                if(IsPinned) {
                    return Parent.PinTrayScreenRect;
                }
                return Parent.ClipTrayScreenRect;
            }
        }
        #endregion

        #region State

        public bool HasContentDataChanged { get; set; }
        
        public string CachedState { get; set; } = null;
        public bool IsReloading => !string.IsNullOrEmpty(CachedState);
        public bool IsWaitingForDomLoad { get; set; } = false;

        public bool IsAnyCornerVisible => Parent == null ? false : ScreenRect.IsAnyPointWithinOtherRect(Parent.ScreenRect);

        public bool IsDevToolsVisible { get; set; } = false;

        public bool IsViewLoaded { get; set; } = false;

        public bool IsTitleReadOnly { get; set; } = true;

        private bool _isContentReadOnly = true;
        public bool IsContentReadOnly {
            get => _isContentReadOnly;
            set {

                if(IsContentReadOnly != value) {
                    if(!value && !MpAvCefNetApplication.UseCefNet) {
                        // this circumvents standard property changes (if user hasn't added to ignore) 
                        // so content isn't degraded in edit mode (and just to keep it simpler its on mode change not data change)
                        DisableReadOnlyInPlainTextHandlerAsync().FireAndForgetSafeAsync();
                        return;
                    }
                    _isContentReadOnly = value;
                    OnPropertyChanged(nameof(IsContentReadOnly));
                }
            }
        }

        public bool IsSubSelectionEnabled { get; set; } = false;

        public bool IsVerticalScrollbarVisibile {
            get {
                if (IsContentReadOnly && !IsSubSelectionEnabled) {
                    return false;
                }
                // true makes auto
                return true;
                //return EditableContentSize.Height > ContentHeight;
            }
        }

        public int QueryOffsetIdx { get; set; } = -1;

        //public int PinnedPlaceholderQueryOffsetIdx { get; set; } = -1;

        //public int RowIdx {
        //    get {
        //        int rowIdx = 0;
        //        if (Parent == null) {
        //            return rowIdx;
        //        }
        //        if (Parent.LayoutType == MpAvClipTrayLayoutType.Stack) {
        //            rowIdx = Parent.ListOrientation == Orientation.Horizontal ?
        //                            0 : QueryOffsetIdx;
        //        } else {
        //            rowIdx = (int)((double)QueryOffsetIdx / (double)Parent.ColCount);
        //        }

        //        return rowIdx;
        //    }
        //}

        //public int ColIdx {
        //    get {
        //        int colIdx = 0;
        //        if (Parent == null) {
        //            return colIdx;
        //        }
        //        if (Parent.LayoutType == MpAvClipTrayLayoutType.Stack) {
        //            colIdx = Parent.ListOrientation == Orientation.Horizontal ?
        //                            QueryOffsetIdx : 0;
        //        } else {
        //            colIdx = QueryOffsetIdx - (RowIdx * Parent.ColCount);
        //        }

        //        return colIdx;
        //    }
        //}

        public bool IsTileOnScreen {
            get {
                return ParentScreenRect.IsAnyPointWithinOtherRect(ObservedBounds);
            }
        }

        public bool IsHorizontalScrollbarVisibile {
            get {
                if (!IsContentReadOnly) {
                    // NOTE from margin padding auto false positives for horizontal and has been working
                    return EditableContentSize.Width > ContentWidth;
                }
                return false;
            }
        }


        public bool IsPinButtonVisible {
            get {
                return IsSelected || IsHovering ? true : false;
            }
        }


        public bool IsTooltipVisible {
            get {
                if (!MpPrefViewModel.Instance.ShowItemPreview) {
                    return false;
                }
                return (Parent.HasScrollVelocity || IsSelected) ? false : true;
            }
        }

        public bool TrialOverlayVisibility {
            get {
                return MpPrefViewModel.Instance.IsTrialExpired ? true : false;
            }
        }

        public int LineCount { get; private set; } = -1;
        public int CharCount { get; private set; } = -1;

        public bool IsAnyBusy {
            get {
                if (IsBusy) {
                    return true;
                }
                if (DetectedImageObjectCollectionViewModel != null && DetectedImageObjectCollectionViewModel.IsAnyBusy) {
                    return true;
                }
                if (TemplateCollection != null && TemplateCollection.IsAnyBusy) {
                    return true;
                }
                if (SourceViewModel != null) {
                    if (AppViewModel != null && AppViewModel.IsBusy) {
                        return true;
                    }
                    if (UrlViewModel != null && UrlViewModel.IsBusy) {
                        return true;
                    }
                    if (SourceViewModel != null && SourceViewModel.IsBusy) {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool HasDetectedObjects => DetectedImageObjectCollectionViewModel != null && DetectedImageObjectCollectionViewModel.Items.Count > 0;

        public bool IsOverHyperlink { get; set; } = false;

        public MpCopyItemDetailType CurDetailType {
            get {
                return (MpCopyItemDetailType)_detailIdx;
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
                    OnPropertyChanged(nameof(TileTitleTextGridBackgroundHexColor));
                    OnPropertyChanged(nameof(TitleTextColor));
                }
            }
        }

        public bool HasBeenSeen { get; set; } = false;


        #region Scroll

        public double NormalizedVerticalScrollOffset { get; set; } = 0;

        public bool IsScrolledToHome => Math.Abs(NormalizedVerticalScrollOffset) <= 0.1;

        public bool IsScrolledToEnd => Math.Abs(NormalizedVerticalScrollOffset) >= 0.9;

        public double KeyboardScrollAmount { get; set; } = 0.2;

        #endregion

        public bool CanEdit => IsSelected && IsTextItem;

        public bool IsInitializing { get; private set; } = false;

        public bool IsContextMenuOpen { get; set; } = false;


        public bool IsTitleFocused { get; set; } = false;


        public bool IsEditingTemplate {
            get {
                if (CopyItem == null || TemplateCollection == null) {
                    return false;
                }

                return TemplateCollection.Items.Any(x => x.IsEditingTemplate);
            }
        }

        public bool IsPasting { get; set; } = false;

        public bool IsPastingTemplate => IsPasting && HasTemplates;

        public bool IsPastingUserInputTemplate => IsPastingTemplate && SelectedTextTemplateViewModels.Any(x => x.IsInputRequiredForPaste);

        public bool HasTemplates {
            get {
                return TemplateCollection.Items.Count > 0;
            }
        }

        public int ItemIdx {
            get {
                if (Parent == null) {
                    return -1;
                }
                return Parent.Items.IndexOf(this);
            }
        }


        public bool IsCustomWidth => Parent == null ? false : MpAvPersistentClipTilePropertiesHelper.IsTileHaveUniqueSize(CopyItemId);
        public bool IsPlaceholder => CopyItem == null;

        #region Drag & Drop

        public bool IsTileDragging { get; set; } = false;
        public bool IsCurrentDropTarget { get; set; } = false;

        #endregion

        public bool IsContentFocused { get; set; } = false;

        public bool IsOverPinButton { get; set; } = false;

        public bool IsOverHideTitleButton { get; set; } = false;

        public bool IsPinned => Parent != null &&
                                Parent.PinnedItems.Any(x => x.CopyItemId == CopyItemId);


        public bool CanVerticallyScroll => !IsContentReadOnly ?
                                                EditableContentSize.Height > TileContentHeight :
                                                UnformattedContentSize.Height > TileContentHeight;

        public bool CanResize { get; set; } = false;

        public bool IsResizing { get; set; } = false;


        public bool IsFileListItem => ItemType == MpCopyItemType.FileList;

        public bool IsTextItem => ItemType == MpCopyItemType.Text;

        public bool IsTitleVisible { get; set; } = true;

        public bool IsDetailGridVisibile {
            get {
                if (Parent.HasScrollVelocity) {
                    return false;
                }
                if (IsFindAndReplaceVisible) {
                    return false;
                }


                if (!IsContentReadOnly) {
                    if (IsEditingTemplate ||
                        IsPastingTemplate) {
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




        public bool IsContentAndTitleReadOnly => IsContentReadOnly && IsTitleReadOnly;


        public bool IsContextMenuOpened { get; set; }

        public bool AllowMultiSelect { get; set; } = false;

        #endregion

        #region Model

        public string EditorFormattedItemData {
            get {
                if(IsPlaceholder) {
                    return string.Empty;
                }
                switch(ItemType) {
                    case MpCopyItemType.FileList:
                        var fl_frag = new MpQuillFileListDataFragment() {
                            fileItems = FileItems.Select(x => new MpQuillFileListItemDataFragmentMessage() {
                                filePath = x.Path,
                                fileIconBase64 = x.IconBase64
                            }).ToList()
                        };
                        var itemData = fl_frag.SerializeJsonObjectToBase64();
                        return itemData;
                    default:
                        return CopyItemData;
                }
            }
        }
        public MpSize UnformattedContentSize {
            get {
                if (CopyItem == null) {
                    return new MpSize();
                }
                return CopyItem.ItemSize;
            }
            set {
                if (UnformattedContentSize != value) {
                    CopyItem.ItemSize = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(UnformattedContentSize));
                }
            }
        }

        public DateTime CopyItemCreatedDateTime {
            get {
                if (CopyItem == null) {
                    return DateTime.MinValue;
                }
                return CopyItem.CopyDateTime;
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
                    // NOTE title is not automatically synced w/ model
                    OnPropertyChanged(nameof(CopyItemTitle));
                }
            }
        }

        public MpPortableDataFormat PreferredFormat {
            get {
                if (CopyItem == null) {
                    return null;
                }
                return CopyItem.PreferredFormat;
            }
            set {
                if (CopyItem != null && CopyItem.PreferredFormat != value) {
                    CopyItem.PreferredFormat = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(PreferredFormat));
                }
            }
        }

        public MpCopyItemType ItemType {
            get {
                if (CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.ItemType;
            }
            set {
                if (CopyItem != null && CopyItem.ItemType != value) {
                    CopyItem.ItemType = value;
                    OnPropertyChanged(nameof(ItemType));
                }
            }
        }

        public string CopyItemGuid {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Guid;
            }
        }

        public int DataObjectId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.DataObjectId;
            }
        }

        public int CopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.Id;
            }
            set {
                if (CopyItem != null && CopyItem.Id != value) {
                    CopyItem.Id = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        public string PublicHandle {
            get {
                if(CopyItem == null || CopyItemId == 0 || string.IsNullOrEmpty(CopyItemGuid)) {
                    return string.Empty;
                }
                return CopyItem.PublicHandle;
            }
        }

        public int SourceId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.SourceId;
            }
        }

        public string CopyItemData {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.ItemData;
            }
            set {
                if (CopyItem != null && CopyItem.ItemData != value) {
                    CopyItem.ItemData = value;
                    HasContentDataChanged = true;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemData));
                }
            }
        }


        public int IconId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                if (CopyItem.IconId > 0) {
                    return CopyItem.IconId;
                }
                if (SourceViewModel == null || SourceViewModel.PrimarySourceViewModel == null) {
                    // BUG currently when plugin creates new content it is not setting source info
                    // so return app icon

                    return MpPrefViewModel.Instance.ThisAppIcon.Id;
                }

                return SourceViewModel.PrimarySourceViewModel.IconId;
            }
            set {
                if (IconId != value) {
                    CopyItem.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IconId));
                }
            }
        }

        private string _curItemRandomHexColor;
        public string CopyItemHexColor {
            get {
                if (CopyItem == null || string.IsNullOrEmpty(CopyItem.ItemColor)) {
                    if (string.IsNullOrEmpty(_curItemRandomHexColor)) {
                        _curItemRandomHexColor = MpColorHelpers.GetRandomHexColor();
                    }
                    return _curItemRandomHexColor;
                }
                return CopyItem.ItemColor;
            }
            set {
                if (CopyItemHexColor != value) {
                    CopyItem.ItemColor = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemHexColor));
                }
            }
        }


        public MpCopyItem CopyItem { get; set; }


        #endregion


        #endregion

        #region Public Methods

        public override string ToString() {
            return $"Tile[{QueryOffsetIdx}] {CopyItemTitle}";
        }
        #endregion

        #region Private Methods

        private void ReceivedGlobalMessage(MpMessageType msg) {

        }

        #endregion

        #region Ported from wpf


        #region Statics

        //public static double DefaultBorderWidth = MpMeasurements.Instance.ClipTileMinSize - MpMeasurements.Instance.ClipTileMargin;
        //public static double DefaultBorderHeight = MpMeasurements.Instance.ClipTileMinSize;

        public static ObservableCollection<string> EditorToolbarIcons => new ObservableCollection<string>() {

        };
        #endregion

        #region Properties

        #region Property Reflection Referencer
        //public object this[string propertyName] {
        //    get {
        //        // probably faster without reflection:
        //        // like:  return MpJsonPreferenceIO.Instance.PropertyValues[propertyName] 
        //        // instead of the following
        //        Type myType = typeof(MpAvClipTileViewModel);
        //        PropertyInfo myPropInfo = myType.GetProperty(propertyName);
        //        if (myPropInfo == null) {
        //            throw new Exception("Unable to find property: " + propertyName);
        //        }
        //        return myPropInfo.GetValue(this, null);
        //    }
        //    set {
        //        Type myType = typeof(MpAvClipTileViewModel);
        //        PropertyInfo myPropInfo = myType.GetProperty(propertyName);
        //        myPropInfo.SetValue(this, value, null);
        //    }
        //}
        #endregion

        #region MpIConditionalContentReadOnlyViewModel Implementation
        //bool MpIConditionalContentReadOnlyViewModel.IsReadOnly { 
        //    get; 
        //    set; 
        //}

        //object MpIConditionalContentReadOnlyViewModel.Content { 
        //    get; 
        //    set; 
        //}
        //object MpIConditionalTitleReadOnlyViewModel.Title { get; set; }
        //#endregion

        //#region MpIConditionalTitleReadOnlyViewModel Implementation
        //bool MpIConditionalTitleReadOnlyViewModel.IsReadOnly {
        //    get;
        //    set;
        //}
        #endregion

        #region MpITooltipInfoViewModel Implementation

        public object Tooltip { get; set; }

        #endregion

        #region MpISizeViewModel Implementation

        double MpISizeViewModel.Width => UnformattedContentSize.Width;
        double MpISizeViewModel.Height => UnformattedContentSize.Height;

        #endregion

        #region MpIRtfSelectionRangeViewModel Implementation 

        public int SelectionStart {
            get {
                var cv = GetContentView();
                if (cv == null) {
                    return 0;
                }
                return cv.Selection.Start.Offset;
            }
        }
        public int SelectionLength {
            get {
                var cv = GetContentView();
                if (cv == null) {
                    return 0;
                }
                return cv.Selection.Length;
            }
        }

        public string SelectedPlainText {
            get {
                var cv = GetContentView();
                if (cv == null) {
                    return string.Empty;
                }
                return cv.Selection.Text;
            }
            set {
                var cv = GetContentView();
                if (cv == null) {
                    Debugger.Break();
                }
                if(cv.Selection.Text != value) {
                    cv.Selection.Text = value;
                }
            }
        }

        public string SelectedRichText => null;// MpContentDocumentRtfExtension.GetSelectedRichText(this);

        public string SelectionBackgroundColor {
            get;// => MpContentDocumentRtfExtension.GetSelectionBackgroundColor(this);
            set;// => MpContentDocumentRtfExtension.SetSelectionBackgroundColor(this, value);
        }
        public string SelectionForegroundColor {
            get;// => MpContentDocumentRtfExtension.GetSelectionForegroundColor(this);
            set;// => MpContentDocumentRtfExtension.SetSelectionForegroundColor(this, value);
        }

        public MpRichTextFormatInfoFormat SelectedRichTextFormat {
            get => null;//MpContentDocumentRtfExtension.GetSelectionFormat(this);
        }

        public IEnumerable<MpAvTextTemplateViewModelBase> SelectedTextTemplateViewModels {
            get {
                if (!IsPastingTemplate) {
                    return new List<MpAvTextTemplateViewModelBase>();
                }

                if (string.IsNullOrEmpty(SelectedPlainText)) {
                    // when selection is empty give em all
                    return TemplateCollection.Items;
                }

                var tvml = new List<MpAvTextTemplateViewModelBase>();
                string spt = SelectedPlainText;

                var mc = MpRegEx.RegExLookup[MpRegExType.Guid].Matches(SelectedPlainText);

                foreach (Match m in mc) {
                    var tvm = TemplateCollection.Items.FirstOrDefault(x => x.TextTemplateGuid == m.Value);
                    if (tvm != null) {
                        tvml.Add(tvm);
                    }
                }
                return tvml.Distinct();
            }
        }

        public bool IsTableSelected {
            get {
                //if (TableViewModel != null &&
                //    TableViewModel.SelectedTables != null &&
                //    TableViewModel.SelectedTables.Count() > 0) {
                //    return true;
                //}
                return false;
            }
        }

        #endregion

        #region MpIUserColorViewModel Implementation
        public string UserHexColor {
            get => CopyItemHexColor;
            set => CopyItemHexColor = value;
        }

        #endregion

        //#region MpIFindAndReplaceViewModel Implementation

        //private int _currentMatchIdx = 0;
        //private List<TextRange> _matches;
        //private MpRtbHighlightBehavior _rtbHighligher {
        //    get {
        //        var cv = MpAvMainWindow.Instance.MainWindowGrid.GetVisualDescendants<MpAvClipTileView>().FirstOrDefault(x => x.DataContext == this);
        //        if (cv == null) {
        //            return null;
        //        }
        //        return cv.RtbHighlightBehavior;
        //    }
        //}
        public bool IsFindAndReplaceVisible { get; set; } = false;

        //private string _findText;
        //public string FindText {
        //    get {
        //        if (string.IsNullOrEmpty(_findText) && !IsFindTextBoxFocused) {
        //            return FindPlaceholderText;
        //        }
        //        return _findText;
        //    }
        //    set {
        //        if (_findText != value && value != FindPlaceholderText) {
        //            _findText = value;
        //        }
        //        OnPropertyChanged(nameof(FindText));
        //        OnPropertyChanged(nameof(IsFindValid));
        //    }
        //}
        //private string _replaceText;
        //public string ReplaceText {
        //    get {
        //        if (string.IsNullOrEmpty(_replaceText) && !IsReplaceTextBoxFocused) {
        //            return ReplacePlaceholderText;
        //        }
        //        return _replaceText;
        //    }
        //    set {
        //        if (_replaceText != value && value != ReplacePlaceholderText) {
        //            _replaceText = value;
        //        }
        //        OnPropertyChanged(nameof(ReplaceText));
        //    }
        //}

        //public string FindPlaceholderText => "Find...";
        //public string ReplacePlaceholderText => "Replace...";

        //private bool _isFindTextBoxFocused;
        //public bool IsFindTextBoxFocused {
        //    get => _isFindTextBoxFocused;
        //    set {
        //        if (_isFindTextBoxFocused != value) {
        //            _isFindTextBoxFocused = value;
        //            if (IsFindTextBoxFocused && FindText == FindPlaceholderText) {
        //                OnPropertyChanged(nameof(FindText));
        //            }
        //            OnPropertyChanged(nameof(IsFindTextBoxFocused));
        //        }
        //    }
        //}
        //private bool _isReplaceTextBoxFocused;
        //public bool IsReplaceTextBoxFocused {
        //    get => _isReplaceTextBoxFocused;
        //    set {
        //        if (_isReplaceTextBoxFocused != value) {
        //            _isReplaceTextBoxFocused = value;
        //            if (IsReplaceTextBoxFocused && ReplaceText == ReplacePlaceholderText) {
        //                OnPropertyChanged(nameof(ReplaceText));
        //            }
        //            OnPropertyChanged(nameof(IsReplaceTextBoxFocused));
        //        }
        //    }
        //}

        //public bool HasFindText => !string.IsNullOrEmpty(FindText) && FindText != FindPlaceholderText;
        //public bool HasReplaceText => !string.IsNullOrEmpty(ReplaceText) && ReplaceText != ReplacePlaceholderText;

        //public bool IsFindMode { get; set; }
        //public bool IsReplaceMode { get; set; }
        //public bool IsFindValid => string.IsNullOrEmpty(_findText) || (!string.IsNullOrEmpty(_findText) && HasMatch);
        //public bool IsReplaceValid => !IsReplaceMode || (IsReplaceMode && _replaceText != null);
        //public bool HasMatch => _matches != null && _matches.Count > 0;
        //public bool MatchCase { get; set; }
        //public bool MatchWholeWord { get; set; }
        //public bool UseRegEx { get; set; }

        //public ObservableCollection<string> RecentFindTexts {
        //    get => new ObservableCollection<string>(MpPrefViewModel.Instance.RecentFindTexts.Split(new string[] { MpPrefViewModel.STRING_ARRAY_SPLIT_TOKEN }, StringSplitOptions.RemoveEmptyEntries));
        //    set => MpPrefViewModel.Instance.RecentFindTexts = string.Join(MpPrefViewModel.STRING_ARRAY_SPLIT_TOKEN, value);
        //}
        //public ObservableCollection<string> RecentReplaceTexts {
        //    get => new ObservableCollection<string>(MpPrefViewModel.Instance.RecentReplaceTexts.Split(new string[] { MpPrefViewModel.STRING_ARRAY_SPLIT_TOKEN }, StringSplitOptions.RemoveEmptyEntries));
        //    set => MpPrefViewModel.Instance.RecentReplaceTexts = string.Join(MpPrefViewModel.STRING_ARRAY_SPLIT_TOKEN, value);
        //}
        //public ICommand ToggleFindAndReplaceVisibleCommand => new MpCommand(
        //    () => {
        //        IsFindAndReplaceVisible = !IsFindAndReplaceVisible;

        //        if (IsFindAndReplaceVisible) {
        //            if (IsContentReadOnly) {
        //                MpContentDocumentRtfExtension.ExpandContent(this);
        //            }
        //        } else {
        //            if (IsContentReadOnly) {
        //                MpContentDocumentRtfExtension.UnexpandContent(this);

        //                MpContentDocumentRtfExtension.SaveTextContent(
        //                    MpContentDocumentRtfExtension.FindRtbByViewModel(this))
        //                .FireAndForgetSafeAsync(this);
        //            }
        //            if (!string.IsNullOrEmpty(MpDataModelProvider.QueryInfo.SearchText)) {
        //                _rtbHighligher.Reset();
        //            }
        //            IsFindMode = false;
        //            IsReplaceMode = false;
        //        }

        //    }, IsTextItem);

        //public void UpdateFindAndReplaceMatches() {
        //    _currentMatchIdx = -1;
        //    if (_findText == null) {
        //        _matches = new List<TextRange>();
        //    } else {
        //        _matches = MpContentDocumentRtfExtension.FindContent(this, _findText, MatchCase, MatchWholeWord, UseRegEx);
        //    }
        //    _rtbHighligher.Reset();
        //    if (HasMatch) {
        //        _rtbHighligher.InitLocalHighlighting(_matches);
        //        FindNextCommand.Execute(null);
        //    }
        //    OnPropertyChanged(nameof(IsFindValid));
        //}
        //public ICommand UpdateFindAndReplaceRecentsCommand => new MpCommand(() => {
        //    UpdateFindAndReplaceMatches();

        //    if (!string.IsNullOrEmpty(_findText)) {
        //        var rftl = RecentFindTexts;
        //        int recentFindIdx = rftl.IndexOf(_findText);
        //        if (recentFindIdx < 0) {
        //            rftl.Insert(0, _findText);
        //            rftl = new ObservableCollection<string>(rftl.Take(MpPrefViewModel.Instance.MaxRecentTextsCount));
        //        } else {
        //            rftl.RemoveAt(recentFindIdx);
        //            rftl.Insert(0, _findText);
        //        }
        //        RecentFindTexts = rftl;
        //    }


        //    if (IsReplaceMode && _replaceText != null) {
        //        var rrtl = RecentReplaceTexts.ToList();
        //        int recentReplaceIdx = rrtl.IndexOf(_replaceText);
        //        if (recentReplaceIdx < 0) {
        //            rrtl.Insert(0, _replaceText);
        //        } else {
        //            rrtl.RemoveAt(recentReplaceIdx);
        //            rrtl.Insert(0, _replaceText);
        //        }
        //        RecentReplaceTexts = new ObservableCollection<string>(rrtl.Take(MpPrefViewModel.Instance.MaxRecentTextsCount));
        //    }
        //});
        //public ICommand FindNextCommand => new MpCommand(
        //    () => {
        //        if (_matches == null || _matches.Count == 0) {
        //            return;
        //        }
        //        _currentMatchIdx++;
        //        if (_currentMatchIdx >= _matches.Count) {
        //            _currentMatchIdx = 0;
        //        }
        //        _rtbHighligher.SelectedIdx = _currentMatchIdx;
        //        _rtbHighligher.ApplyHighlighting();
        //        MpContentDocumentRtfExtension.SetTextSelection(this, _matches[_currentMatchIdx]);

        //    }, HasMatch);

        //public ICommand FindPreviousCommand => new MpCommand(
        //    () => {
        //        if (_matches == null || _matches.Count == 0) {
        //            return;
        //        }
        //        _currentMatchIdx--;
        //        if (_currentMatchIdx < 0) {
        //            _currentMatchIdx = _matches.Count - 1;
        //        }
        //        _rtbHighligher.SelectedIdx = _currentMatchIdx;
        //        _rtbHighligher.ApplyHighlighting();
        //        MpContentDocumentRtfExtension.SetTextSelection(this, _matches[_currentMatchIdx]);
        //    }, HasMatch);

        //public ICommand ReplaceNextCommand => new MpCommand(
        //    () => {
        //        FindNextCommand.Execute(null);

        //        SelectedPlainText = _replaceText;

        //        //MpTextSelectionRangeExtension.SetSelectionText(this,_replaceText);

        //    }, HasMatch && IsReplaceValid);

        //public ICommand ReplacePreviousCommand => new MpCommand(
        //    () => {
        //        FindPreviousCommand.Execute(null);

        //        SelectedPlainText = _replaceText;
        //        //MpTextSelectionRangeExtension.SetSelectionText(this, _replaceText);
        //    }, HasMatch && IsReplaceValid);
        //public ICommand ReplaceAllCommand => new MpCommand(
        //    () => {
        //        _currentMatchIdx = -1;
        //        for (int i = 0; i < _matches.Count; i++) {
        //            ReplaceNextCommand.Execute(null);
        //        }

        //    }, HasMatch && IsReplaceValid);

        //#endregion

        #region View Models

        public ObservableCollection<MpAvFileDataObjectItemViewModel> FileItems { get; set; } = new ObservableCollection<MpAvFileDataObjectItemViewModel>();
        public MpImageAnnotationCollectionViewModel DetectedImageObjectCollectionViewModel { get; set; }

        public MpAvTemplateCollectionViewModel TemplateCollection { get; set; }

        public MpContentTableViewModel TableViewModel { get; set; }

        public MpAvSourceViewModel SourceViewModel {
            get {
                var svm = MpAvSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == SourceId);
                if (svm == null) {
                    return MpAvSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == MpPrefViewModel.Instance.ThisAppSourceId);
                }
                return svm;
            }
        }

        public MpAvAppViewModel AppViewModel => SourceViewModel == null ? null : SourceViewModel.AppViewModel;

        public MpAvUrlViewModel UrlViewModel => SourceViewModel == null ? null : SourceViewModel.UrlViewModel;

        public MpAvClipTileViewModel Next {
            get {
                if (IsPlaceholder || Parent == null) {
                    return null;
                }
                if (IsPinned) {
                    int pinIdx = Parent.PinnedItems.IndexOf(this);
                    return Parent.PinnedItems.FirstOrDefault(x => Parent.PinnedItems.IndexOf(x) == pinIdx + 1);
                }
                return Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == QueryOffsetIdx + 1);
            }
        }

        public MpAvClipTileViewModel Prev {
            get {
                if (IsPlaceholder || Parent == null || QueryOffsetIdx == 0) {
                    return null;
                }
                if (IsPinned) {
                    int pinIdx = Parent.PinnedItems.IndexOf(this);
                    return Parent.PinnedItems.FirstOrDefault(x => Parent.PinnedItems.IndexOf(x) == pinIdx - 1);
                }
                return Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == QueryOffsetIdx - 1);
            }
        }

        public MpColorPalletePopupMenuViewModel SelectionFgColorPopupViewModel { get; private set; } = new MpColorPalletePopupMenuViewModel();

        public MpColorPalletePopupMenuViewModel SelectionBgColorPopupViewModel { get; private set; } = new MpColorPalletePopupMenuViewModel();
        #endregion

        //#region MpAvIShortcutCommand Implementation

        //public MpShortcutType ShortcutType => MpShortcutType.PasteCopyItem;

        //public MpShortcutViewModel ShortcutViewModel {
        //    get {
        //        if (Parent == null || CopyItem == null) {
        //            return null;
        //        }
        //        var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.CommandId == CopyItemId && x.ShortcutType == ShortcutType);

        //        if (scvm == null) {
        //            scvm = new MpShortcutViewModel(MpShortcutCollectionViewModel.Instance);
        //        }

        //        return scvm;
        //    }
        //}

        //public string ShortcutKeyString => ShortcutViewModel == null ? string.Empty : ShortcutViewModel.KeyString;

        //public ICommand AssignCommand => AssignHotkeyCommand;

        //#endregion

        #region MpIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType => MpShortcutType.PasteCopyItem;
        public string ShortcutLabel => "Paste " + CopyItemTitle;
        public int ModelId => CopyItemId;
        public ICommand ShortcutCommand => Parent == null ? null : Parent.PasteCopyItemByIdCommand;
        public object ShortcutCommandParameter => CopyItemId;

        #endregion

        #region Layout

        //public MpRect ObservedBounds { get; set; }
        public Thickness ContentMarginThickness {
            get {
                if (IsTitleVisible) {
                    return new Thickness();
                }
                double bottomMargin = IsDetailGridVisibile ? 0 : 10;
                return new Thickness(5, 0, 5, bottomMargin);
            }
        }

        public double TileTitleHeight => IsTitleVisible ? 100 : 0;
        public double TileDetailHeight => 25;// MpMeasurements.Instance.ClipTileDetailHeight;

        private double _tileEditToolbarHeight = 40;// MpMeasurements.Instance.ClipTileEditToolbarDefaultHeight;
        public double TileEditToolbarHeight {
            get {
                if (IsContentReadOnly) {
                    return 0;
                }
                return _tileEditToolbarHeight;
            }
            set {
                if (_tileEditToolbarHeight != value) {
                    _tileEditToolbarHeight = value;
                    OnPropertyChanged(nameof(TileEditToolbarHeight));
                }
            }
        }

        public double TileContentWidth =>
            BoundWidth -
            (InnerSpacing * 2);
            //MpMeasurements.Instance.ClipTileContentMargin -
            //(MpMeasurements.Instance.ClipTileMargin * 2);

        public double TileContentHeight =>
            BoundHeight -
            TileTitleHeight -
            (InnerSpacing * 2) -
            //MpMeasurements.Instance.ClipTileMargin -
            //MpMeasurements.Instance.ClipTileBorderThickness -
            TileDetailHeight;


        //private double _tileBorderHeight = DefaultBorderHeight;
        //public double BoundHeight {
        //    get {
        //        //return _tileBorderHeight;
        //        return MinSize;
        //    }
        //    //set {
        //    //    if (_tileBorderHeight != value) {
        //    //        _tileBorderHeight = Math.Max(0, value);
        //    //        OnPropertyChanged(nameof(BoundHeight));
        //    //        //OnPropertyChanged(nameof(TileBorderWidth));
        //    //        //OnPropertyChanged(nameof(TileContentWidth));
        //    //        OnPropertyChanged(nameof(TileContentHeight));
        //    //        //OnPropertyChanged(nameof(TrayX));
        //    //        OnPropertyChanged(nameof(EditorHeight));
        //    //    }
        //    //}
        //}

        //public double TrayX {
        //    get {
        //        if (IsPlaceholder) {
        //            return 0;
        //        }
        //        if(Prev == null) {
        //            return 0;
        //        }
        //        return Prev.TrayX + Prev.TileBorderWidth + (MpMeasurements.Instance.ClipTileMargin * 2);
        //        //return MpDataModelProvider.AllFetchedAndSortedTileOffsets[QueryOffsetIdx];
        //    }
        //}

        //public double TrayX {
        //    get {
        //        if (IsPinned || Parent == null) {
        //            return 0;
        //        }
        //        return Parent.FindTileOffsetX(QueryOffsetIdx);
        //    }
        //}

        
        public double EditTemplateToolbarHeight => 40;
        public double PasteTemplateToolbarHeight => 40;// MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;


        public double LoadingSpinnerSize => 100;// MpMeasurements.Instance.ClipTileLoadingSpinnerSize;


        //content container
        //public Size ContainerSize {
        //    get {
        //        var cs = new Size(MpMeasurements.Instance.ClipTileScrollViewerWidth, 0);
        //        if (Parent == null) {
        //            return cs;
        //        }
        //        double ch = MpMeasurements.Instance.ClipTileContentDefaultHeight;
        //        if (!IsContentReadOnly) {
        //            ch -= TileEditToolbarHeight;
        //        }
        //        if (IsPastingTemplate) {
        //            ch -= PasteTemplateToolbarHeight;
        //        }
        //        if (IsEditingTemplate) {
        //            ch -= EditTemplateToolbarHeight;
        //        }
        //        cs.Height = ch;
        //        return cs;
        //    }
        //}

        public MpSize ContentSize => IsContentReadOnly ? ReadOnlyContentSize : UnformattedContentSize;
            //new Size(
            //        ContainerSize.Width - MpMeasurements.Instance.ClipTileBorderThickness,
            //        ContainerSize.Height - MpMeasurements.Instance.ClipTileBorderThickness);

        public double ContainerWidth {
            get {
                return BoundWidth;
            }
        }

        public double ContentHeight => ContentSize.Height;

        public double ContentWidth => ContentSize.Width;


        public MpSize ReadOnlyContentSize => new MpSize(TileContentWidth, TileContentHeight);
                    //MpMeasurements.Instance.ClipTileContentDefaultWidth,
                    //MpMeasurements.Instance.ClipTileContentDefaultHeight);


        public MpSize EditableContentSize {
            get {
                if (Parent == null || CopyItem == null) {
                    return new MpSize();
                }
                //get contents actual size
                var ds = UnformattedContentSize;//CopyItemData.ToFlowDocument().GetDocumentSize();

                //if item's content is larger than expanded width make sure it gets that width (will show scroll bars)
                double w = Math.Max(ds.Width, MinWidth);// MpMeasurements.Instance.ClipTileContentMinMaxWidth);

                //let height in expanded mode match content's height
                double h = ds.Height;

                return new MpSize(w, h);
            }
        }

        public MpSize CurrentSize {
            get {
                if (Parent == null) {
                    return new MpSize();
                }
                if (!IsContentReadOnly) {
                    return EditableContentSize;
                }
                return ReadOnlyContentSize;
            }
        }



        #endregion

        #region Visibility

        
        #endregion


        #region Business Logic

        public string TemplateRichHtml { get; set; }

        public string DetailText { get; set; }

        #endregion

        #region Icons


        #endregion


        #endregion

        #region Events

        public event EventHandler OnUiUpdateRequest;
        public event EventHandler OnScrollToHomeRequest;
        //public event EventHandler OnFocusRequest;
        public event EventHandler OnSyncModels;

        //public event EventHandler<Point> OnScrollOffsetRequest;
        public event EventHandler<object> OnPastePortableDataObject;

        public event EventHandler<double> OnScrollWheelRequest;
        //public event EventHandler OnFitContentRequest;
        //public event EventHandler OnSubSelected;

        public event EventHandler OnMergeRequest;
        //public event EventHandler<bool> OnUiResetRequest;
        public event EventHandler OnClearTemplatesRequest;
        //public event EventHandler OnCreateTemplatesRequest;
        #endregion


        #region Constructors

        public MpAvClipTileViewModel() : this(null) {
            IsBusy = true;
        }

        public MpAvClipTileViewModel(MpAvClipTrayViewModel parent) : base(parent) {
            PropertyChanged += MpClipTileViewModel_PropertyChanged;
            //MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            IsBusy = true;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpCopyItem ci, int queryOffset = -1, bool isRestoringSelection = false) { 
            _curItemRandomHexColor = string.Empty;

            IsBusy = true;
            IsInitializing = true;

            if (ci != null && MpAvPersistentClipTilePropertiesHelper.TryGetByPersistentSize_ById(ci.Id, out double uniqueWidth)) {
                BoundSize = new MpSize(uniqueWidth, MinHeight);
            } else {
                BoundSize = MinSize;
            }

            CopyItem = ci;
            QueryOffsetIdx = queryOffset < 0 && ci != null ? QueryOffsetIdx : queryOffset;


            //MpMessenger.Unregister<MpMessageType>(typeof(MpDragDropManager), ReceivedDragDropManagerMessage);

            if (ItemType == MpCopyItemType.FileList) {
                // BUG will need to check source here... pretty much most places using env.newLine to parse right i think
                //  or substitute for 'portableNewLine' where necessary
                var ci_dobil = await MpDataModelProvider.GetDataObjectItemsByDataObjectId(DataObjectId);
                var fivml = await Task.WhenAll(ci_dobil.Select(x=>CreateFileItemViewModel(x)));
                FileItems = new ObservableCollection<MpAvFileDataObjectItemViewModel>(fivml);
            }else {
                FileItems.Clear();
            }
            
            TemplateCollection = new MpAvTemplateCollectionViewModel(this);

            //if(ci != null) {
            //    InitTitleLayers().FireAndForgetSafeAsync(this);
            //}
            CycleDetailCommand.Execute(null);

            if (ItemType == MpCopyItemType.Image) {
                DetectedImageObjectCollectionViewModel = new MpImageAnnotationCollectionViewModel(this);
                await DetectedImageObjectCollectionViewModel.InitializeAsync(CopyItem);
                OnPropertyChanged(nameof(HasDetectedObjects));
            } else if (ItemType == MpCopyItemType.Text) {
                TableViewModel = new MpContentTableViewModel(this);

                SelectionFgColorPopupViewModel.OnColorChanged -= SelectionFgColorPopupViewModel_OnColorChanged;
                SelectionFgColorPopupViewModel.OnColorChanged += SelectionFgColorPopupViewModel_OnColorChanged;

                SelectionBgColorPopupViewModel.OnColorChanged -= SelectionBgColorPopupViewModel_OnColorChanged;
                SelectionBgColorPopupViewModel.OnColorChanged += SelectionBgColorPopupViewModel_OnColorChanged;
            }

            if(isRestoringSelection) {
                Parent.RestoreSelectionState(this);
            }

            OnPropertyChanged(nameof(TileBorderBrushRect));

            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(TrayX));
            OnPropertyChanged(nameof(TileBorderHexColor));
            OnPropertyChanged(nameof(CanVerticallyScroll));
            OnPropertyChanged(nameof(IsTextItem));
            OnPropertyChanged(nameof(IsFileListItem));
            OnPropertyChanged(nameof(TileBackgroundHexColor));
            OnPropertyChanged(nameof(ContentMarginThickness));
            OnPropertyChanged(nameof(Next));
            OnPropertyChanged(nameof(Prev));

            RequestUiUpdate();

            MpMessenger.Send<MpMessageType>(MpMessageType.ContentItemsChanged, this);

            OnPropertyChanged(nameof(CopyItemTitle));

            OnPropertyChanged(nameof(CopyItemData));

            if(MpAvPersistentClipTilePropertiesHelper.IsPersistentTileEditable_ById(CopyItemId)) {
                IsContentReadOnly = false;
            }

            IsInitializing = false;
            IsBusy = false;
        }

        public async Task InitTitleLayers() {
            bool wasBusy = IsBusy;
            IsBusy = true;

            int layerCount = 4;

            bool HasUserDefinedColor = !string.IsNullOrEmpty(CopyItem.ItemColor);

            List<string> hexColors = new List<string>();

            if (IconId > 0 && !HasUserDefinedColor) {
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == IconId);
                if (ivm == null) {
                    var icon = await MpDb.GetItemAsync<MpIcon>(IconId);
                    hexColors = icon.HexColors;
                } else {
                    hexColors = ivm.PrimaryIconColorList.ToList();
                }
            } else if (HasUserDefinedColor) {
                hexColors = Enumerable.Repeat(CopyItemHexColor, layerCount).ToList();
            } else {
                var tagColors = await MpDataModelProvider.GetTagColorsForCopyItemAsync(CopyItemId);
                tagColors.ForEach(x => hexColors.Insert(0, x));
            }

            if (hexColors.Count == 0) {
                hexColors = Enumerable.Repeat(MpColorHelpers.GetRandomHexColor(), layerCount).ToList();
            }
            hexColors = hexColors.Take(layerCount).ToList();

            TitleLayerHexColors = hexColors.Select((x, i) => x.AdjustAlpha((double)MpRandom.Rand.Next(40, 120) / 255)).ToArray();
            TitleLayerZIndexes = new List<int> { 1, 2, 3 }.Randomize().ToArray();

            IsBusy = wasBusy;
        }

        public MpAvIContentView GetContentView() {
            var ctcv = MpAvClipTrayContainerView.Instance.GetVisualDescendants<MpAvClipTileContentView>().FirstOrDefault(x => x.DataContext == this);
            if (ctcv == null) {
                Debugger.Break();
            }
            return ctcv.ContentView;
        }

        private async Task<MpAvFileDataObjectItemViewModel> CreateFileItemViewModel(MpDataObjectItem dobjItem) {
            var fivm = new MpAvFileDataObjectItemViewModel(this);
            await fivm.InitializeAsync(dobjItem);
            return fivm;
        }
        public void ResetSubSelection(bool clearEditing = true, bool reqFocus = false) {
            Dispatcher.UIThread.Post(() => {
                ClearSelection(clearEditing);
                Parent.IsSelectionReset = true;
                IsSelected = true;
                Parent.IsSelectionReset = false;
                if (reqFocus) {
                    IsContentFocused = true;
                }
            });
        }

        public async Task<MpAvClipTileViewModel> GetNeighborByRowOffsetAsync(int row_offset) {
            var items = IsPinned ? Parent.PinnedItems : Parent.Items;
            MpAvClipTileViewModel target_ctvm = null;
            if(row_offset < 0) {
                var pre_items =
                    items
                    .Where(x => x != this && x.ObservedBounds.Y < ObservedBounds.Y)
                    .OrderByDescending(x => x.ObservedBounds.Y);
                if(pre_items.Count() > 0) {
                    target_ctvm =
                       pre_items.Aggregate( (a, b) =>
                               a.ObservedBounds.Location.Distance(ObservedBounds.Location) <
                               b.ObservedBounds.Location.Distance(ObservedBounds.Location) ? a : b);
                }
               
                if(target_ctvm == null) {
                    if (IsPinned || Parent.LayoutType == MpAvClipTrayLayoutType.Stack || Parent.HeadQueryIdx == 0) {
                        // fallback and treat up as left
                        target_ctvm = await GetNeighborByColumnOffsetAsync(row_offset);
                    } else {
                        Parent.ScrollToPreviousPageCommand.Execute(null);
                        await Task.Delay(100);
                        while (Parent.IsAnyBusy) { await Task.Delay(100); }
                        target_ctvm = await GetNeighborByRowOffsetAsync(row_offset);
                    }
                }
            } else {
                var post_items =
                    items
                    .Where(x => x != this && x.ObservedBounds.Y > ObservedBounds.Y)
                    .OrderBy(x => x.ObservedBounds.Y);
                if(post_items.Count() > 0) {
                    target_ctvm =
                        post_items.Aggregate((a, b) =>
                            a.ObservedBounds.Location.Distance(ObservedBounds.Location) <
                            b.ObservedBounds.Location.Distance(ObservedBounds.Location) ? a : b);
                }
                if(target_ctvm == null) {
                    if(IsPinned || Parent.LayoutType == MpAvClipTrayLayoutType.Stack || Parent.TailQueryIdx == Parent.MaxClipTrayQueryIdx) {
                        // fallback and treat down as right
                        target_ctvm = await GetNeighborByColumnOffsetAsync(row_offset);
                    } else {
                        Parent.ScrollToNextPageCommand.Execute(null);
                        await Task.Delay(100);
                        while (Parent.IsAnyBusy) { await Task.Delay(100); }
                        target_ctvm = await GetNeighborByRowOffsetAsync(row_offset);
                    }
                }
            }
            return target_ctvm;
        }

        public async Task<MpAvClipTileViewModel> GetNeighborByColumnOffsetAsync(int col_offset) {
            int target_idx = -1;
            if(IsPinned) {
                target_idx = Parent.PinnedItems.IndexOf(this) + col_offset;
                if(target_idx < 0) {
                    return null;
                }
                if(target_idx >= Parent.PinnedItems.Count) {
                    target_idx = target_idx - Parent.PinnedItems.Count;
                    if(target_idx < Parent.VisibleItems.Count()) {
                        if(Parent.DefaultScrollOrientation == Orientation.Horizontal) {
                            return Parent.VisibleItems.OrderBy(x => TrayX).ElementAt(target_idx);
                        }
                        return Parent.VisibleItems.OrderBy(x => TrayY).ElementAt(target_idx);
                    }
                    return null;
                }
                return Parent.PinnedItems[target_idx];
            }
            target_idx = QueryOffsetIdx + col_offset;
            if (target_idx < 0) {
                if (Parent.IsPinTrayEmpty) {
                    return null;
                }
                target_idx = Parent.PinnedItems.Count + target_idx;
                if (target_idx < 0) {
                    return null;
                }
                return Parent.PinnedItems[target_idx];
            } else if (target_idx >= Parent.TotalTilesInQuery) {
                return null;
            }
            var neighbor_ctvm = Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == target_idx);
            if(neighbor_ctvm == null) {
                int neighbor_ciid = MpDataModelProvider.AvailableQueryCopyItemIds[target_idx];
                Parent.ScrollIntoView(neighbor_ciid);
                await Task.Delay(100);
                while(Parent.IsAnyBusy) { await Task.Delay(100); }
                return Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == target_idx);
            }
            return neighbor_ctvm;
        }

        public void SubSelectAll() {
            AllowMultiSelect = true;
            IsSelected = true;
            AllowMultiSelect = false;
        }

        public void ResetExpensiveDetails() {
            LineCount = CharCount = -1;
        }

        private string GetDetailText(MpCopyItemDetailType detailType) {
            if (CopyItem == null) {
                return string.Empty;
            }

            switch (detailType) {
                //created
                case MpCopyItemDetailType.DateTimeCreated:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc

                    return "Copied " + CopyItemCreatedDateTime.ToReadableTimeSpan();
                //chars/lines
                case MpCopyItemDetailType.DataSize:
                    if (CopyItem.ItemType == MpCopyItemType.Image) {
                        return "(" + (int)UnformattedContentSize.Width + "px) x (" + (int)UnformattedContentSize.Height + "px)";
                    } else if (CopyItem.ItemType == MpCopyItemType.Text) {
                        if (LineCount < 0 && CharCount < 0) {
                            //Line and Char count are set to -1 when initialized so they're lazy loaded
                            var textTuple = new Tuple<int, int>(0, 0);//MpContentDocumentRtfExtension.GetLineAndCharCount(this);
                            LineCount = textTuple.Item1;
                            CharCount = textTuple.Item2;
                        }

                        return CharCount + " chars | " + LineCount + " lines";
                    } else if (CopyItem.ItemType == MpCopyItemType.FileList) {
                        break;// return GetDetailText((MpCopyItemDetailType)(++_detailIdx));
                    }
                    break;
                //# copies/# pastes
                case MpCopyItemDetailType.UsageStats:
                    return CopyItem.CopyCount + " copies | " + CopyItem.PasteCount + " pastes";
                case MpCopyItemDetailType.UrlInfo:
                    if (UrlViewModel == null) {
                        break;// return GetDetailText((MpCopyItemDetailType)(++_detailIdx));
                    }
                    return UrlViewModel.UrlPath;
                //case MpCopyItemDetailType.AppInfo:
                //    if (AppViewModel == null) {
                //        return GetDetailText((MpCopyItemDetailType)(++_detailIdx));
                //    }
                //    return AppViewModel.AppPath;
                default:
                    break;
            }

            return string.Empty;
        }

        #region View Event Invokers


        public void RequestScrollToHome() {
            OnScrollToHomeRequest?.Invoke(this, null);
        }

        public void RequestUiUpdate() {
            OnUiUpdateRequest?.Invoke(this, null);
        }


        public void RequestSyncModel() {
            OnSyncModels?.Invoke(this, null);
        }

        //public void RequestScrollOffset(Point p) {
        //    OnScrollOffsetRequest?.Invoke(this, p);
        //}

        public void RequestPastePortableDataObject(object portableDataObjectOrCopyItem) {
            OnPastePortableDataObject?.Invoke(this, portableDataObjectOrCopyItem);
        }

        //public void RequestFitContent() {
        //    OnFitContentRequest?.Invoke(this, null);
        //}


        public void RequestMerge() {
            OnMergeRequest?.Invoke(this, null);
        }

        public void RequestClearHyperlinks() {
            OnClearTemplatesRequest?.Invoke(this, null);
        }

        //public void RequestCreateHyperlinks() {
        //    OnCreateTemplatesRequest?.Invoke(this, null);
        //}

        public void RequestScrollWheelChange(double delta) {
            OnScrollWheelRequest?.Invoke(this, delta);
        }


        #endregion

        public void ClearSelection(bool clearEditing = true) {
            IsSelected = false;
            LastSelectedDateTime = DateTime.MaxValue;
            if (clearEditing) {
                ClearEditing();
            }
        }

        public void ClearEditing() {
            IsTitleReadOnly = true;
            IsContentReadOnly = true;
            TemplateCollection?.ClearAllEditing();
            if (IsPasting) {
                IsPasting = false;
                //Parent.RequestUnexpand();
            }
        }


        public async Task<MpAvDataObject> ConvertToDataObject(bool fillTempalates) {
            MpAvDataObject d = new MpAvDataObject();

            var wv = GetContentView() as MpAvCefNetWebView;
            string qhtml = string.Empty;
            string pt = string.Empty;
            string bmpBase64 = string.Empty;
            var sctfl = new List<string>();

            bool isInUi = Parent.GetClipTileViewModelById(CopyItemId) != null;
            bool isSelectionEmpty = string.IsNullOrEmpty(SelectedPlainText) || !isInUi;
            bool needsTemplateData = fillTempalates && HasTemplates;

            if (needsTemplateData) {
                IsPasting = true;
                needsTemplateData = SelectedTextTemplateViewModels.Count() > 0;
            }
            if (needsTemplateData) {
                IsSelected = true;

                if (!MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);

                    while (MpAvMainWindowViewModel.Instance.IsMainWindowOpening) {
                        await Task.Delay(100);
                    }
                } else {
                    MpAvMainWindow.Instance.Activate();
                    MpAvMainWindow.Instance.Focus();
                    MpAvMainWindow.Instance.Topmost = true;
                    //MpAvMainWindow.Instance.Top = 0;
                }
                IsSelected = true;

                await Task.Delay(MpAvClipTrayViewModel.DISABLE_READ_ONLY_DELAY_MS);
                IsContentReadOnly = false;

                TemplateRichHtml = null;

                TemplateCollection.BeginPasteTemplateCommand.Execute(null);
                bool wasCanceled = false;
                await Task.Run(async () => {
                    while (string.IsNullOrEmpty(TemplateRichHtml)) {
                        if (IsContentReadOnly) {
                            wasCanceled = true;
                            break;
                        }
                        await Task.Delay(100);
                    }
                });
                if (wasCanceled) {
                    Parent.CleanupAfterPaste(this);
                    return null;
                }

                qhtml = TemplateRichHtml;

                if (!IsContentReadOnly) {
                    ClearEditing();
                }

            } else if (ItemType == MpCopyItemType.Text) {
                if (isInUi) {
                    qhtml = await MpAvCefNetWebViewExtension.GetEncodedContentAsync(
                               wv,
                               ignoreSubSelection: isSelectionEmpty);
                } else {
                    // handle special case when pasting item by id (like from a hotkey)
                    // and it has no templates (if it did tray would set manual query and show it)
                    // so since its not in ui need to use model data which is ok because it won't have any modifications
                    //rtf = CopyItemData.ToContentRichText();
                    qhtml = CopyItemData;
                }
            }
            switch (ItemType) {
                case MpCopyItemType.Text:
                    // NOTE must use rtf here which already is based on selection and any templated input
                    pt = qhtml.ToPlainText();
                    //if (wv != null) {
                    //    var respStr = await wv.EvaluateJavascriptAsync("getContentImageBase64Async_ext()");
                    //    var respObj = MpJsonObject.DeserializeBase64Object<MpQuillGetEditorScreenshotResponseMessage>(respStr);
                    //    bmpBase64 = respObj.base64ImgStr;
                    //}

                    break;
                case MpCopyItemType.Image:
                    await Task.Run(() => {
                        var bmpSrc = CopyItemData.ToAvBitmap();
                        pt = bmpSrc.ToAsciiImage();
                        qhtml = bmpSrc.ToRichHtmlImage();
                    });

                    bmpBase64 = CopyItemData;
                    break;
                case MpCopyItemType.FileList:
                    if (FileItems.All(x => x.IsSelected == false)) {
                        FileItems.ForEach(x => x.IsSelected = true);
                    }
                    pt = string.Join(Environment.NewLine, FileItems.Select(x => x.Path));
                    break;
            }

            foreach (string format in MpPortableDataFormats.RegisteredFormats) {
                switch (format) {
                    case MpPortableDataFormats.AvFileNames:
                    //case MpPortableDataFormats.FileDrop:
                        switch (ItemType) {
                            case MpCopyItemType.Text:
                                sctfl.Add(qhtml.ToFile(null, CopyItemTitle));
                                break;
                            case MpCopyItemType.Image:
                                sctfl.Add(bmpBase64.ToFile(null, CopyItemTitle));
                                break;
                            case MpCopyItemType.FileList:
                                foreach (var fp in FileItems.Where(x => x.IsSelected).Select(x => x.Path)) {
                                    if (fp.IsFileOrDirectory()) {
                                        sctfl.Add(fp);
                                    }
                                }
                                break;
                        }
                        //d.SetData(MpPortableDataFormats.FileDrop, string.Join(Environment.NewLine, sctfl));
                        break;
                    case MpPortableDataFormats.AvHtml_bytes:
                        if (string.IsNullOrEmpty(qhtml)) {
                            break;
                        }
                        d.SetData(MpPortableDataFormats.AvHtml_bytes, qhtml);
                        break;
                    // TODO add rtf conversion here...
                    case MpPortableDataFormats.Text:
                        if (string.IsNullOrEmpty(pt)) {
                            break;
                        }
                        d.SetData(MpPortableDataFormats.Text, pt);
                        break;
                    case MpPortableDataFormats.AvPNG:
                        if (string.IsNullOrEmpty(bmpBase64)) {
                            break;
                        }
                        d.SetData(MpPortableDataFormats.AvPNG, bmpBase64);
                        break;
                    case MpPortableDataFormats.Csv:
                        switch (ItemType) {
                            case MpCopyItemType.Text:

                                bool hasCsv = await MpDataModelProvider.IsDataObjectContainFormatAsync(DataObjectId, MpPortableDataFormats.Csv);
                                if(hasCsv) {
                                    d.SetData(MpPortableDataFormats.Csv, CopyItemData.ToCsv());
                                }
                                
                                break;
                            case MpCopyItemType.Image:

                                break;
                            case MpCopyItemType.FileList:
                                d.SetData(
                                    MpPortableDataFormats.Csv,
                                    string.Join(
                                        ",",
                                        FileItems.Where(x => x.IsSelected).Select(x => x.Path)));
                                break;
                        }
                        break;
                    default:
                        continue;
                }
            }
            d.MapAllPseudoFormats();
            return d;
        }

        public async Task<List<string>> GetSupportedDataFormatsAsync() {
            await Task.Delay(1);
            var sdfl = new List<string>();
            sdfl.Add(MpPortableDataFormats.Text);
            //sdfl.Add(MpPortableDataFormats.AvFileNames);
            //if (ItemType == MpCopyItemType.Text) {
            //    sdfl.Add(MpPortableDataFormats.Text);
            //}
            return sdfl;
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
            if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }

            //(Parent.BringSelectedClipTilesToFrontCommand as MpCommand).NotifyCanExecuteChanged();
            //(Parent.SendSelectedClipTilesToBackCommand as MpCommand).NotifyCanExecuteChanged();
            //(Parent.SpeakSelectedClipsCommand as MpCommand).NotifyCanExecuteChanged();
            //(Parent.MergeSelectedClipsCommand as MpCommand).NotifyCanExecuteChanged();
            //(Parent.TranslateSelectedClipTextAsyncCommand as MpCommand<string>).NotifyCanExecuteChanged();
            //(Parent.CreateQrCodeFromSelectedClipsCommand as MpCommand).NotifyCanExecuteChanged();
        }

        #region IDisposable

        public override void Dispose() {
            //base.Dispose();
            //PropertyChanged -= MpClipTileViewModel_PropertyChanged;
            //SelectionBgColorPopupViewModel.OnColorChanged -= SelectionBgColorPopupViewModel_OnColorChanged;
            //SelectionFgColorPopupViewModel.OnColorChanged -= SelectionFgColorPopupViewModel_OnColorChanged;
            ClearSelection();
            //TemplateCollection.Dispose();
        }

        #endregion

        #endregion

        #region Protected Methods

        #region DB Overrides

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == CopyItemId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(SelfBindingRef));
                }
            } else if (e is MpImageAnnotation dio) {
                if (dio.CopyItemId == CopyItemId) {
                    Dispatcher.UIThread.Post(async () => {
                        if (DetectedImageObjectCollectionViewModel == null) {
                            DetectedImageObjectCollectionViewModel = new MpImageAnnotationCollectionViewModel(this);
                        }
                        await DetectedImageObjectCollectionViewModel.InitializeAsync(CopyItem);
                        OnPropertyChanged(nameof(HasDetectedObjects));
                    });
                }
            }
        }

        protected override  void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == CopyItemId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(SelfBindingRef));
                }
            } else if (e is MpCopyItem ci && ci.Id == CopyItemId) {
                if (HasModelChanged) {
                    // this means the model has been updated from the view model so ignore
                } else {
                    Dispatcher.UIThread.Post(async () => {
                        await InitializeAsync(ci);
                    });
                }
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            //if (MpAvDragDropManager.IsDragAndDrop) {
            //    return;
            //}
            if (e is MpCopyItem ci && CopyItemId == ci.Id) {

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
                case nameof(IsHovering):
                    Parent.OnPropertyChanged(nameof(Parent.CanScroll));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyHovering));
                    break;
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        // BUGGY this clears the alternate selection if tile is pinned/unpinned probably bad
                        //if (IsPinned) {
                        //    Parent.ClearClipSelection(false);
                        //} else {
                        //    Parent.ClearPinnedSelection(false);
                        //}
                        if (Parent.SelectedItem != this) {
                            Parent.SelectedItem = this;
                        }

                        Parent.ScrollIntoView(this);
                        //if (!IsTitleFocused && !Parent.IsPasting) {
                        //    // NOTE checking Parent.IsPasting because setting focus will clear current selection
                        //    IsContentFocused = true;
                        //}
                        if (!Parent.IsRestoringSelection) {
                            Parent.StoreSelectionState(this);
                        }
                    } else {
                        if (IsContentReadOnly) {
                            if (IsSubSelectionEnabled) {
                                IsSubSelectionEnabled = false;
                                RequestUiUpdate();
                            }
                        }
                        FileItems.ForEach(x => x.IsSelected = false);
                        //LastSelectedDateTime = DateTime.MinValue;
                        //ClearSelection();
                    }

                    Parent.NotifySelectionChanged();
                    OnPropertyChanged(nameof(TileBorderHexColor));
                    break;
                case nameof(CopyItem):
                    if (CopyItem == null) {
                        break;
                    }
                    OnPropertyChanged(nameof(CopyItemData));
                    OnPropertyChanged(nameof(CurrentSize));
                    //UpdateDetails();
                    RequestUiUpdate();
                    break;
                case nameof(IsEditingTemplate):
                    //OnPropertyChanged(nameof(De))
                    break;
                case nameof(IsOverPinButton):
                case nameof(IsPinned):
                    OnPropertyChanged(nameof(PinIconSourcePath));
                    OnPropertyChanged(nameof(IsPlaceholder));
                    break;
                case nameof(IsOverHideTitleButton):
                case nameof(IsTitleVisible):
                    OnPropertyChanged(nameof(HideTitleIconSourcePath));
                    OnPropertyChanged(nameof(TileTitleHeight));
                    OnPropertyChanged(nameof(BoundHeight));
                    OnPropertyChanged(nameof(TileContentHeight));
                    break;
                case nameof(IsSubSelectionEnabled):
                    Parent.OnPropertyChanged(nameof(Parent.CanScroll));
                    OnPropertyChanged(nameof(IsHorizontalScrollbarVisibile));
                    OnPropertyChanged(nameof(IsVerticalScrollbarVisibile));

                    break;
                case nameof(IsContentFocused):
                    if (IsContentFocused) {
                        if (IsEditingTemplate) {
                            TemplateCollection.Items.FirstOrDefault(x => x.IsEditingTemplate).FinishEditTemplateCommand.Execute(null);
                        }
                    }
                    break;
                case nameof(IsTitleReadOnly):
                    if (!IsTitleReadOnly) {
                        _originalTitle = CopyItemTitle;
                        IsTitleFocused = true;
                        IsSelected = true;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTitle));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTile));
                    break;
                case nameof(IsContentReadOnly):
                    if (!IsContentReadOnly && !IsSelected) {
                        IsSelected = true;
                    }
                    if(IsContentReadOnly) {
                        MpAvPersistentClipTilePropertiesHelper.RemovePersistentEditableTile_ById(CopyItemId);
                    } else {
                        MpAvPersistentClipTilePropertiesHelper.AddPersistentEditableTile_ById(CopyItemId);
                    }
                    MpMessenger.Send<MpMessageType>(IsContentReadOnly ? MpMessageType.IsReadOnly : MpMessageType.IsEditable, this);
                    Parent.OnPropertyChanged(nameof(Parent.IsHorizontalScrollBarVisible));
                    Parent.OnPropertyChanged(nameof(Parent.IsVerticalScrollBarVisible));

                    OnPropertyChanged(nameof(IsHorizontalScrollbarVisibile));
                    OnPropertyChanged(nameof(IsVerticalScrollbarVisibile));
                    OnPropertyChanged(nameof(CanVerticallyScroll));
                    IsSubSelectionEnabled = !IsContentReadOnly;
                    OnPropertyChanged(nameof(IsSubSelectionEnabled));
                    //OnPropertyChanged(nameof(IsContentEditable));

                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTile));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTile));

                    if(Next == null) {
                        break;
                    }
                    
                    //if (!IsContentReadOnly) {
                    //    IsFindMode = false;
                    //    IsReplaceMode = false;
                    //}
                    break;
                case nameof(IsContextMenuOpen):
                    OnPropertyChanged(nameof(TileBorderHexColor));
                    //Parent.OnPropertyChanged(nameof(Parent.TileBorderBrush));
                    OnPropertyChanged(nameof(TileBorderBrushRect));
                    OnPropertyChanged(nameof(IsContextMenuOpen));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyTileContextMenuOpened));
                    break;
                case nameof(IsPasting):
                    Parent.OnPropertyChanged(nameof(Parent.IsPasting));
                    break;
                case nameof(IsTileDragging):
                    if (IsTileDragging) {
                        StartAnimation();
                        if (!IsSubSelectionEnabled) {
                            // BUG checking selection length here (when IsSubSelectionEnabled=false)
                            // to see if partial selection always returns some size when
                            // none is actually selected. So force it to select all and 
                            // make sure selection extension updates ui of selection
                            IsContentFocused = true;
                            //MpContentDocumentRtfExtension.SelectAll(this);
                        }

                        TemplateCollection.ClearAllEditing();
                    } else {
                        StopAnimation();
                    }
                    OnPropertyChanged(nameof(TileBorderHexColor));
                    OnPropertyChanged(nameof(TileBorderBrushRect));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyTileDragging));
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        if(!MpAvCefNetApplication.UseCefNet && HasContentDataChanged) {
                            if(IsInitializing) {
                                MpConsole.WriteLine("Ignoring plain text mode initialize data overwrite");
                                HasContentDataChanged = false;
                                HasModelChanged = false;
                                return;
                            }
                        }
                        Task.Run(async () => {
                            await CopyItem.WriteToDatabaseAsync();
                            HasModelChanged = false;
                            HasContentDataChanged = false;
                        });
                    }
                    break;
                case nameof(CopyItemHexColor):
                    InitTitleLayers().FireAndForgetSafeAsync(this);
                    break;
                case nameof(CopyItemData):
                    ResetExpensiveDetails();
                    break;

                case nameof(CanResize):
                    OnPropertyChanged(nameof(TileBorderHexColor));
                    Parent.OnPropertyChanged(nameof(Parent.CanAnyResize));
                    break;
                case nameof(IsResizing):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyResizing));
                    if (!IsResizing) {
                        Parent.RefreshLayout();
                    }
                    break;
                case nameof(MinSize):
                    OnPropertyChanged(nameof(MinWidth));
                    OnPropertyChanged(nameof(MinHeight));
                    BoundSize = new MpSize(IsCustomWidth ? BoundWidth : MinSize.Width, MinSize.Height);
                    break;
                case nameof(ObservedWidth):
                    BoundWidth = ObservedWidth;
                    break;
                case nameof(ObservedHeight):
                    BoundHeight = ObservedHeight;
                    break;
                case nameof(BoundSize):
                    if (IsResizing) {
                        //this occurs when mainwindow is resized and user gives tile unique width
                        MpAvPersistentClipTilePropertiesHelper.AddOrReplacePersistentSize_ById(CopyItemId, BoundWidth);
                    }
                    if (Next == null) {
                        break;
                    }
                    OnPropertyChanged(nameof(BoundWidth));
                    OnPropertyChanged(nameof(BoundHeight));
                    OnPropertyChanged(nameof(MaxWidth));
                    OnPropertyChanged(nameof(MaxHeight));
                    Parent.UpdateTileRectCommand.Execute(new object[] { Next, TrayRect });
                    break;
                case nameof(TrayLocation):
                    if(QueryOffsetIdx == 0 && TrayLocation.X > 0) {
                        Debugger.Break();
                    }
                    if (Next == null) {
                        //if (Parent.IsAnyResizing) {
                        //    // TrayX is only changed on layout change OR resize
                        //    // only update list when next is null (tail)
                        //    Parent.RefreshLayout();
                        //}
                        break;
                    }
                    OnPropertyChanged(nameof(TrayX));
                    OnPropertyChanged(nameof(TrayY));

                    OnPropertyChanged(nameof(BoundWidth));
                    OnPropertyChanged(nameof(BoundHeight));
                    OnPropertyChanged(nameof(MaxWidth));
                    OnPropertyChanged(nameof(MaxHeight));
                    Parent.UpdateTileRectCommand.Execute(new object[] { Next, TrayRect });
                    //Next.OnPropertyChanged(nameof(Next.TrayX));
                    break;
                case nameof(QueryOffsetIdx):
                    if (IsPlaceholder || Parent.IsUnpinning || Parent.IsBatchOffsetChange) {
                        break;
                    }
                    //MpRect prevRect = Prev == null ? null : Prev.TrayRect;
                    //Parent.UpdateTileRectCommand.Execute(new object[] { this, prevRect });
                    Parent.UpdateTileRectCommand.Execute(this);
                    //OnPropertyChanged(nameof(TrayX));
                    break;
                    //case nameof(IsFindMode):
                    //    if (IsFindMode) {
                    //        if (IsReplaceMode) {
                    //            IsReplaceMode = false;
                    //        } else {
                    //            ToggleFindAndReplaceVisibleCommand.Execute(null);
                    //        }
                    //    } else if (!IsReplaceMode) {
                    //        //ToggleFindAndReplaceVisibleCommand.Execute(null);
                    //        if (!IsContentReadOnly) {
                    //            IsFindAndReplaceVisible = false;
                    //        }
                    //    }
                    //    break;
                    //case nameof(IsReplaceMode):
                    //    if (IsReplaceMode) {
                    //        if (IsFindMode) {
                    //            IsFindMode = false;
                    //        } else {
                    //            ToggleFindAndReplaceVisibleCommand.Execute(null);
                    //        }
                    //    } else if (!IsFindMode) {
                    //        //ToggleFindAndReplaceVisibleCommand.Execute(null);
                    //        if (!IsContentReadOnly) {
                    //            IsFindAndReplaceVisible = false;
                    //        }
                    //    }
                    //    break;
                    //case nameof(IsFindAndReplaceVisible):
                    //    if (!IsFindAndReplaceVisible && !IsContentReadOnly) {
                    //        IsFindMode = IsReplaceMode = false;
                    //    }
                    //    break;
                    //case nameof(FindText):
                    //case nameof(ReplaceText):
                    //case nameof(MatchCase):
                    //case nameof(UseRegEx):
                    //case nameof(MatchWholeWord):
                    //    UpdateFindAndReplaceMatches();
                    //    break;

            }
        }

        private async Task DisableReadOnlyInPlainTextHandlerAsync() {
            Dispatcher.UIThread.VerifyAccess();

            var result = await MpNotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ContentFormatDegradation,
                                    title: "Data Degradation Warning",
                                    msg: $"Editing in comptability mode will remove all rich formatting. Are you sure you wish to modify this?");

            if(result == MpNotificationDialogResultType.Ok) {
                _isContentReadOnly = false;
                OnPropertyChanged(nameof(IsContentReadOnly));
            }
        }
        private void SelectionBgColorPopupViewModel_OnColorChanged(object sender, string e) {
            SelectionBackgroundColor = e;
        }

        private void SelectionFgColorPopupViewModel_OnColorChanged(object sender, string e) {
            SelectionForegroundColor = e;
        }

        private void StartAnimation() {
            //return;
            if (_timer == null) {
                _timer = new DispatcherTimer(DispatcherPriority.Render);
                _timer.Interval = TimeSpan.FromMilliseconds(30);
                _timer.Tick += _timer_Tick;
            }
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e) {
            if (!IsTileDragging) {
                StopAnimation();
                return;
            }
            if (TileBorderBrushTranslateOffsetX > 50) {
                TileBorderBrushTranslateOffsetX = 0;
            }
            TileBorderBrushTranslateOffsetX += 0.01d;
        }

        private void StopAnimation() {
            TileBorderBrushTranslateOffsetX = 0.0d;
            OnPropertyChanged(nameof(TileBorderBrushRect));
            _timer.Stop();
        }
        #endregion

        #region Commands

        public ICommand TileDragBeginCommand => new MpCommand(
            () => {
                //MpAvDragDropManager.StartDragCheck(this);
            });

        public ICommand EnableSubSelectionCommand => new MpCommand(
            () => {
                IsSubSelectionEnabled = !IsSubSelectionEnabled;
            },()=>IsContentReadOnly);

        public ICommand ChangeColorCommand => new MpCommand<string>(
            (b) => {
                CopyItemHexColor = b;// b.ToHex();
            });
        public ICommand SendSubSelectedToEmailCommand => new MpCommand(
            () => {
                //MpHelpers.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, CopyItemTitle, CopyItemData.ToPlainText()));
            });

        public ICommand ResetTileSizeToDefaultCommand => new MpCommand(
            () => {
                IsResizing = true;

                MpAvPersistentClipTilePropertiesHelper.RemovePersistentSize_ById(CopyItemId);
                BoundSize = Parent.DefaultItemSize;

                IsResizing = false;
            }, () => {
                return Parent != null && MpAvPersistentClipTilePropertiesHelper.IsTileHaveUniqueSize(CopyItemId);
            });



        private MpCommand<object> _searchWebCommand;
        public ICommand SearchWebCommand {
            get {
                if (_searchWebCommand == null) {
                    _searchWebCommand = new MpCommand<object>(SearchWeb);
                }
                return _searchWebCommand;
            }
        }
        private void SearchWeb(object args) {
            if (args == null || args.GetType() != typeof(string)) {
                return;
            }
            //MpHelpers.OpenUrl(args.ToString() + System.Uri.EscapeDataString(CopyItem.ItemData.ToPlainText()));
        }

        public ICommand RefreshDocumentCommand {
            get {
                return new MpCommand(
                    () => {
                        RequestSyncModel();
                        //MessageBox.Show(TemplateCollection.ToString());
                    },
                    () => {
                        return true;// HasModelChanged
                    });
            }
        }

        public ICommand CycleDetailCommand => new MpCommand(
            () => {
                if(IsPlaceholder) {
                    _detailIdx = -1;
                    DetailText = String.Empty;
                    return;
                }

                do {
                    _detailIdx++;
                    if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                        _detailIdx = 1;
                    }

                    // TODO this should aggregate details over all sub items 
                    DetailText = GetDetailText((MpCopyItemDetailType)_detailIdx);
                } while (string.IsNullOrEmpty(DetailText));
            });

                public ICommand ToggleEditContentCommand => new MpCommand(
            () => {
                if (!IsSelected && IsContentReadOnly) {
                    Parent.SelectedItem = this;
                }
                IsContentReadOnly = !IsContentReadOnly;

            }, ()=>IsTextItem);

        public ICommand ToggleHideTitleCommand => new MpCommand(
            () => {
                IsTitleVisible = !IsTitleVisible;
            }, ()=> !IsPlaceholder);

        public ICommand CancelEditTitleCommand => new MpCommand(
            () => {
                CopyItemTitle = _originalTitle;
                IsTitleReadOnly = true;
            });

        public ICommand FinishEditTitleCommand => new MpCommand(
            () => {
                IsTitleReadOnly = true;
                CopyItem.WriteToDatabaseAsync().FireAndForgetSafeAsync(this);
            });

        public ICommand ShowCefDevToolsCommand => new MpCommand(
            () => {
                if(GetContentView() is MpAvCefNetWebView wv) {
                    wv.ShowDevTools();
                }
            });
        #endregion

        #endregion
    }
}
