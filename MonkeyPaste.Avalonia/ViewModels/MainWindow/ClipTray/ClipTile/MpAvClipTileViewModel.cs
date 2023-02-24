using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        MpITextContentViewModel,
        MpIAppendTitleViewModel,
        //MpIRtfSelectionRange,
        MpIContextMenuViewModel,
        //MpIFindAndReplaceViewModel,
        MpITooltipInfoViewModel,
        MpISizeViewModel {

        #region Private Variables

        private List<string> _tempFileList = new List<string>();
        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        //private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;
        private string _originalTitle;

        #endregion

        #region Constants
        public const int AUTO_CYCLE_DETAIL_DELAY_MS = 3000;

        #endregion

        #region Statics
        public static ObservableCollection<string> EditorToolbarIcons => new ObservableCollection<string>() {

        };
        #endregion

        #region Interfaces

        #region MpIContextMenuViewModel Implementation

        public MpMenuItemViewModel ContextMenuViewModel => IsSelected ? Parent.ContextMenuViewModel : null;

        #endregion

        #region MpIAppendTitleViewModel Implementation

        public string AppendTitle {
            get {
                if (!IsAppendNotifier || Parent == null || !Parent.IsAnyAppendMode) {
                    return string.Empty;
                }
                return $"Append [{(Parent.IsAppendLineMode ? "Line" : "Inline")}] - {CopyItemTitle}";
            }
        }

        #endregion

        #region MpITextContentViewModel Implementation

        string MpITextContentViewModel.PlainText {
            get {
                if (CopyItemType == MpCopyItemType.Image) {
                    return string.Empty;
                }
                return CopyItemData.ToPlainText();
            }
        }
        #endregion

        #region MpITooltipInfoViewModel Implementation

        public object Tooltip { get; set; }

        #endregion

        #region MpISizeViewModel Implementation

        double MpISizeViewModel.Width => UnconstrainedContentDimensions.Width;
        double MpISizeViewModel.Height => UnconstrainedContentDimensions.Height;

        #endregion

        #region MpIUserColorViewModel Implementation
        public string UserHexColor {
            get => CopyItemHexColor;
            set => CopyItemHexColor = value;
        }

        #endregion

        #region MpIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType => MpShortcutType.PasteCopyItem;
        public string ShortcutLabel => "Paste " + CopyItemTitle;
        public int ModelId => CopyItemId;
        public ICommand ShortcutCommand => Parent == null ? null : Parent.PasteCopyItemByIdCommand;
        public object ShortcutCommandParameter => CopyItemId;

        #endregion

        #region MpISelectableViewModel Implementation
        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        public MpISelectorViewModel Selector => Parent;

        #endregion

        //#region MpIContextMenuViewModel Implementation

        //public MpMenuItemViewModel ContextMenuViewModel => IsSelected ? Parent.ContextMenuViewModel : null;

        //#endregion

        #region MpIScrollIntoView Implementation

        void MpIScrollIntoView.ScrollIntoView() {
            if (Parent == null) {
                return;
            }
            Parent.ScrollIntoView(this);
        }

        #endregion

        #endregion

        #region Properties

        #region View Models

        public MpAvFileItemCollectionViewModel FileItemCollectionViewModel { get; private set; }

        private MpAvClipTileTransactionCollectionViewModel _transactionCollectionViewModel;
        public MpAvClipTileTransactionCollectionViewModel TransactionCollectionViewModel {
            get {
                if (_transactionCollectionViewModel == null) {
                    _transactionCollectionViewModel = new MpAvClipTileTransactionCollectionViewModel(this);
                }
                return _transactionCollectionViewModel;
            }
        }

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

        #endregion        

        #region Appearance

        public int[] TitleLayerZIndexes { get; private set; } = Enumerable.Range(1, 3).ToArray();
        public string[] TitleLayerHexColors { get; private set; } = Enumerable.Repeat(MpSystemColors.Transparent, 4).ToArray();

        public string DetailText {
            get {
                string detailText = string.Empty;
                switch ((MpCopyItemDetailType)SelectedDetailIdx) {
                    //created
                    case MpCopyItemDetailType.DateTimeCreated:
                        detailText = "Copied " + CopyItemCreatedDateTime.ToReadableTimeSpan();
                        break;
                    case MpCopyItemDetailType.DataSize:
                        switch (CopyItemType) {
                            case MpCopyItemType.Image:
                                detailText = $"({(int)UnconstrainedContentDimensions.Width}px) | ({(int)UnconstrainedContentDimensions.Height}px)";
                                break;
                            case MpCopyItemType.Text:
                                detailText = $"{CharCount} chars | {LineCount} lines";
                                break;
                            case MpCopyItemType.FileList:

                                detailText = $"{LineCount} files | {CharCount} MBs";
                                break;
                        }
                        break;
                    //# copies/# pastes
                    case MpCopyItemDetailType.UsageStats:
                        detailText = $"{CopyCount} copies | {PasteCount} pastes";
                        break;
                    default:
                        break;
                }
                return detailText;
            }
        }
        #endregion

        #region Layout

        private MpSize _unconstrainedContentSize = MpSize.Empty;
        public MpSize UnconstrainedContentDimensions {
            get => _unconstrainedContentSize;
            set {
                if (UnconstrainedContentDimensions != value) {
                    _unconstrainedContentSize = value;
                    OnPropertyChanged(nameof(UnconstrainedContentDimensions));
                }
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
            TransactionCollectionViewModel.BoundWidth;

        public double TileContentHeight =>
            BoundHeight -
            TileTitleHeight -
            TileDetailHeight;


        public MpSize ContentSize => IsContentReadOnly ? ReadOnlyContentSize : UnconstrainedContentDimensions;
        public double ContentWidth => ContentSize.Width;
        public MpSize ReadOnlyContentSize => new MpSize(TileContentWidth, TileContentHeight);


        public MpSize EditableContentSize {
            get {
                if (Parent == null || CopyItem == null) {
                    return new MpSize();
                }
                //get contents actual size
                var ds = UnconstrainedContentDimensions;//CopyItemData.ToFlowDocument().GetDocumentSize();

                //if item's content is larger than expanded width make sure it gets that width (will show scroll bars)
                double w = Math.Max(ds.Width, MinWidth);// MpMeasurements.Instance.ClipTileContentMinMaxWidth);

                //let height in expanded mode match content's height
                double h = ds.Height;

                return new MpSize(w, h);
            }
        }

        public MpRect ObservedBounds { get; set; }
        public double MinWidth =>
            Parent == null ? 0 : IsPinned ? Parent.DefaultPinItemWidth : Parent.DefaultQueryItemWidth;
        public double MinHeight =>
            Parent == null ? 0 : IsPinned ? Parent.DefaultPinItemHeight : Parent.DefaultQueryItemHeight;


        public double MaxWidth => double.PositiveInfinity;
        public double MaxHeight => double.PositiveInfinity;

        private double _titleHeight = 0;
        public double TitleHeight {
            get => _titleHeight;
            set {
                if (TitleHeight != value) {
                    _titleHeight = value;
                    OnPropertyChanged(nameof(TitleHeight));
                }
            }
        }
        public double MaxTitleHeight => 40;

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
                if (BoundHeight != value) {
                    BoundSize.Height = value;
                    OnPropertyChanged(nameof(BoundSize));
                }
            }
        }
        public MpSize BoundSize { get; set; } = MpSize.Empty;

        public MpRect TrayRect =>
            new MpRect(TrayLocation, BoundSize);

        public MpRect ScreenRect =>
            Parent == null ? MpRect.Empty : new MpRect(TrayLocation - Parent.ScrollOffset, BoundSize);


        public double ReadOnlyWidth => MinWidth;
        public double ReadOnlyHeight => MinHeight;


        public double EditableWidth {
            get {
                if (Parent == null) {
                    return 0;
                }
                double w = Parent.DefaultEditableItemWidth;
                return w;
            }
        }

        public double EditableHeight {
            get {
                if (Parent == null) {
                    return 0;
                }
                return Parent.DefaultQueryItemHeight;
            }
        }

        #endregion

        #region State

        public int RecyclePriority {
            get {
                if (Parent == null) {
                    return 0;
                }
                if (IsPlaceholder) {
                    return int.MaxValue;
                }
                if (QueryOffsetIdx < 0) {
                    // should be caught by placeholder but they arent 
                    // exclusively depenendant...
                    return int.MaxValue - 1;
                }
                // farther from top-left, higher the priority
                return (int)ScreenRect.Location.Length;
            }
        }
        private int SelectedDetailIdx { get; set; } = 0;

        public bool IsHovering { get; set; }
        public bool IsHoveringOverSourceIcon { get; set; } = false;
        public bool IsOverDetail { get; set; } = false;

        #region Append
        public bool IsAppendTrayItem {
            get {
                if (Parent == null || Parent.ModalClipTileViewModel == null) {
                    return false;
                }
                return Parent.ModalClipTileViewModel.CopyItemId == CopyItemId && !IsAppendNotifier;
            }
        }

        public bool IsAppendNotifier {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.ModalClipTileViewModel == this;
            }
        }

        //public bool HasAppendModel => IsAppendNotifier || IsAppendClipTile;


        #endregion

        //public string AnnotationsJsonStr { get; set; }

        public bool CanShowContextMenu { get; set; } = true;


        public bool HasTemplates { get; set; } = false;
        public bool IsFindAndReplaceVisible { get; set; } = false;
        public string TemplateRichHtml { get; set; }

        public bool IsAnyQueryCornerVisible => Parent == null ? false : ScreenRect.IsAnyPointWithinOtherRect(Parent.QueryTrayScreenRect);
        public bool IsAllQueryCornersVisible => Parent == null ? false : ScreenRect.IsAllPointWithinOtherRect(Parent.QueryTrayScreenRect);

        public bool IsDevToolsVisible { get; set; } = false;

        private bool _isViewLoaded;
        public bool IsViewLoaded {
            get => _isViewLoaded;
            set {
                if (_isViewLoaded != value) {
                    _isViewLoaded = value;
                    OnPropertyChanged(nameof(IsViewLoaded));
                }
            }
        }


        public bool IsTitleReadOnly { get; set; } = true;

        private bool _isContentReadOnly = true;
        public bool IsContentReadOnly {
            get => _isContentReadOnly;
            set {

                if (IsContentReadOnly != value) {
                    if (!value && !MpPrefViewModel.Instance.IsRichHtmlContentEnabled) {
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

        private int _queryOffsetIdx = -1;
        public int QueryOffsetIdx =>
            _queryOffsetIdx;

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

        public bool IsTrialOverlayVisibile {
            get {
                return MpPrefViewModel.Instance.IsTrialExpired ? true : false;
            }
        }

        public int LineCount { get; set; }
        public int CharCount { get; set; }

        public bool IsAnyBusy {
            get {
                if (IsBusy) {
                    return true;
                }
                //bool isplaceHolder = IsPlaceholder && !IsAppendNotifier;
                //var cv = GetContentView();
                //if(cv == null || !cv.IsViewLoaded) {
                //    return true;
                //}
                if (!IsPlaceholder && !IsViewLoaded) {//IsAnyQueryCornerVisible) {
                    return true;
                }


                if (FileItemCollectionViewModel != null && FileItemCollectionViewModel.IsAnyBusy) {
                    return true;
                }

                if (TransactionCollectionViewModel.IsAnyBusy) {
                    return true;
                }
                if (IsAppendNotifier) {
                    return false;
                }
                //if (SourceViewModel != null) {
                //    if (AppViewModel != null && AppViewModel.IsBusy) {
                //        return true;
                //    }
                //    if (UrlViewModel != null && UrlViewModel.IsBusy) {
                //        return true;
                //    }
                //    if (SourceViewModel != null && SourceViewModel.IsBusy) {
                //        return true;
                //    }
                //}
                return false;
            }
        }

        #region Scroll

        public double NormalizedVerticalScrollOffset { get; set; } = 0;

        public bool IsScrolledToHome => Math.Abs(NormalizedVerticalScrollOffset) <= 0.1;

        public bool IsScrolledToEnd => Math.Abs(NormalizedVerticalScrollOffset) >= 0.9;

        public double KeyboardScrollAmount { get; set; } = 0.2;

        #endregion

        public bool CanEdit => IsSelected && IsTextItem;
        public bool IsListBoxItemFocused { get; set; } = false;
        public bool IsTitleFocused { get; set; } = false;

        public bool IsAnyFocused =>
            IsListBoxItemFocused ||
            IsTitleFocused;

        public bool IsPasting { get; set; } = false;

        public bool IsCustomWidth => Parent == null ? false : MpAvPersistentClipTilePropertiesHelper.IsTileHaveUniqueSize(CopyItemId);
        public bool IsPlaceholder => CopyItem == null;

        #region Drag & Drop

        public bool IsDropOverTile { get; set; } = false;

        //public bool IsTileDragging { get; set; } = false;

        #endregion

        public bool IsPinned => Parent != null &&
                                Parent.PinnedItems.Any(x => x.CopyItemId == CopyItemId);
        public bool CanVerticallyScroll => !IsContentReadOnly ?
                                                EditableContentSize.Height > TileContentHeight :
                                                UnconstrainedContentDimensions.Height > TileContentHeight;


        public bool IsResizable => !IsAppendNotifier;
        public bool CanResize { get; set; } = false;

        public bool IsResizing { get; set; } = false;


        public bool IsFileListItem => CopyItemType == MpCopyItemType.FileList;

        public bool IsTextItem => CopyItemType == MpCopyItemType.Text;

        private bool _isTitleVisible = true;
        public bool IsTitleVisible {
            get {
                if (IsAppendNotifier ||
                    !IsContentReadOnly ||
                    (TransactionCollectionViewModel != null && TransactionCollectionViewModel.IsTransactionPaneOpen)) {
                    return false;
                }
                return _isTitleVisible;
            }
            set {
                if (IsTitleVisible != value) {
                    _isTitleVisible = value;
                    OnPropertyChanged(nameof(IsTitleVisible));
                }
            }
        }


        public bool IsCornerButtonsVisible {
            get {
                if (MpPlatform.Services.PlatformInfo.IsDesktop) {
                    if (IsHovering && !IsAppendNotifier && !Parent.IsAnyDropOverTrays) {
                        return true;
                    }
                } else if (IsSelected) {
                    return true;
                }
                return false;

            }
        }

        private bool _isDetailVisible = true;
        public bool IsDetailVisible {
            get {
                if (IsAppendNotifier) {
                    return false;
                }
                return _isDetailVisible;
            }
            set {
                if (IsDetailVisible != value) {
                    _isDetailVisible = value;
                    OnPropertyChanged(nameof(IsDetailVisible));
                }
            }
        }

        private bool _isHeaderAndFooterVisible = true;
        public bool IsHeaderAndFooterVisible {
            get {
                if (IsAppendNotifier) {
                    return false;
                }
                return _isHeaderAndFooterVisible;
            }
            set {
                if (IsAppendNotifier) {
                    return;
                }
                if (IsHeaderAndFooterVisible != value) {
                    _isHeaderAndFooterVisible = value;
                    IsDetailVisible = value;
                    IsTitleVisible = value;
                    OnPropertyChanged(nameof(IsHeaderAndFooterVisible));
                    OnPropertyChanged(nameof(IsCornerButtonsVisible));
                }
            }
        }

        public bool IsContentAndTitleReadOnly => IsContentReadOnly && IsTitleReadOnly;

        public bool IsContextMenuOpen { get; set; } = false;

        public bool AllowMultiSelect { get; set; } = false;

        public DateTime TileCreatedDateTime { get; set; }
        #endregion

        #region Model

        public int CopyCount {
            get {
                if (IsPlaceholder) {
                    return 0;
                }
                return CopyItem.CopyCount;
            }
            set {
                if (CopyCount != value) {
                    //CopyItem.CopyCount = value;
                    //HasModelChanged = true;
                    NotifyModelChanged(CopyItem, nameof(CopyItem.CopyCount), value);
                    OnPropertyChanged(nameof(CopyCount));
                }
            }
        }

        public int PasteCount {
            get {
                if (IsPlaceholder) {
                    return 0;
                }
                return CopyItem.PasteCount;
            }
            set {
                if (PasteCount != value) {
                    //CopyItem.PasteCount = value;
                    //HasModelChanged = true;
                    NotifyModelChanged(CopyItem, nameof(CopyItem.PasteCount), value);
                    OnPropertyChanged(nameof(PasteCount));
                }
            }
        }

        public string EditorFormattedItemData {
            get {
                if (IsPlaceholder) {
                    return string.Empty;
                }
                switch (CopyItemType) {
                    case MpCopyItemType.FileList:
                        var fl_frag = new MpQuillFileListDataFragment() {
                            fileItems = FileItemCollectionViewModel.Items.Select(x => new MpQuillFileListItemDataFragmentMessage() {
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

        public MpPortableDataFormat DataFormat {
            get {
                if (CopyItem == null) {
                    return null;
                }
                if (!MpPortableDataFormats.RegisteredFormats.All(x => x != CopyItem.DataFormat)) {
                    MpPortableDataFormats.RegisterDataFormat(CopyItem.DataFormat);
                }
                return MpPortableDataFormats.GetDataFormat(CopyItem.DataFormat);
            }
            set {
                if (CopyItem != null && CopyItem.DataFormat != value.Name) {
                    CopyItem.DataFormat = value.Name;

                    if (!MpPortableDataFormats.RegisteredFormats.All(x => x != CopyItem.DataFormat)) {
                        MpPortableDataFormats.RegisterDataFormat(CopyItem.DataFormat);
                    }
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(DataFormat));
                }
            }
        }

        public MpSize CopyItemSize {
            get {
                if (CopyItem == null) {
                    return MpSize.Empty;
                }
                return new MpSize(
                    CopyItem.ItemSize1,
                    CopyItem.ItemSize2);
            }
            set {
                if (CopyItem != null &&
                   CopyItemSize != value) {
                    MpSize sizeVal = value ?? MpSize.Empty;
                    if (CopyItemSize.Width == sizeVal.Width &&
                        CopyItemSize.Height == sizeVal.Height) {
                        return;
                    }
                    CopyItem.ItemSize1 = (int)sizeVal.Width;
                    CopyItem.ItemSize2 = (int)sizeVal.Height;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemSize));
                }
            }
        }

        public MpCopyItemType CopyItemType {
            get {
                if (CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.ItemType;
            }
            set {
                if (CopyItem != null &&
                    CopyItem.ItemType != value) {
                    CopyItem.ItemType = value;
                    OnPropertyChanged(nameof(CopyItemType));
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
                if (CopyItem == null || CopyItemId == 0 || string.IsNullOrEmpty(CopyItemGuid)) {
                    return string.Empty;
                }
                return CopyItem.PublicHandle;
            }
        }

        //public int SourceId {
        //    get {
        //        if (CopyItem == null) {
        //            return 0;
        //        }
        //        return CopyItem.SourceId;
        //    }
        //}

        public string CopyItemData {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.ItemData;
            }
            set {
                if (CopyItem != null && CopyItem.ItemData != value) {
                    //CopyItem.ItemData = value;
                    //HasModelChanged = true;

                    NotifyModelChanged(CopyItem, nameof(CopyItem.ItemData), value);
                    OnPropertyChanged(nameof(CopyItemData));
                }
            }
        }

        public object IconResourceObj {
            get {
                if (CopyItemType == MpCopyItemType.FileList &&
                    FileItemCollectionViewModel != null &&
                    FileItemCollectionViewModel.Items.Count > 0) {
                    return FileItemCollectionViewModel.Items.FirstOrDefault().IconBase64;
                }
                return TransactionCollectionViewModel.PrimaryItem == null ?
                            MpDefaultDataModelTools.ThisAppIconId :
                            TransactionCollectionViewModel.PrimaryItem.IconSourceObj;
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
                    //CopyItem.ItemColor = value;
                    //HasModelChanged = true;

                    NotifyModelChanged(CopyItem, nameof(CopyItem.ItemColor), value);
                    OnPropertyChanged(nameof(CopyItemHexColor));
                }
            }
        }


        public MpCopyItem CopyItem { get; set; }


        #endregion

        #endregion

        #region Events

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
            TileCreatedDateTime = DateTime.Now;

            PropertyChanged += MpClipTileViewModel_PropertyChanged;
            FileItemCollectionViewModel = new MpAvFileItemCollectionViewModel(this);
            //DetailCollectionViewModel = new MpAvClipTileDetailCollectionViewModel(this);
            IsBusy = true;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpCopyItem ci, int queryOffset = -1, bool isRestoringSelection = false) {
            _curItemRandomHexColor = string.Empty;
            _contentView = null;

            IsBusy = true;

            await Task.Delay(1);

            if (ci != null &&
                MpAvPersistentClipTilePropertiesHelper.TryGetByPersistentSize_ById(ci.Id, out double uniqueWidth)) {
                //BoundSize = new MpSize(uniqueWidth, MinHeight);
                BoundWidth = uniqueWidth;
                BoundHeight = MinHeight;
            } else if (ci != null && ci.Id > 0) {
                //BoundSize = MinSize;
                BoundWidth = MinWidth;
                BoundHeight = MinHeight;
            } else {
                BoundWidth = 0;
                BoundHeight = 0;
            }
            // NOTE FileItems are init'd before ciid is set so Items are busy when WebView is loading content
            FileItemCollectionViewModel.InitializeAsync(ci).FireAndForgetSafeAsync(this);

            CopyItem = ci;
            //QueryOffsetIdx = queryOffset < 0 && ci != null ? QueryOffsetIdx : queryOffset;

            CycleDetailCommand.Execute(0);
            await TransactionCollectionViewModel.InitializeAsync(CopyItemId);

            //var cial = await MpDataModelProvider.GetCopyItemAnnotationsAsync(CopyItemId);
            //if(cial == null || cial.Count == 0) {
            //    AnnotationsJsonStr = string.Empty;
            //} else {
            //    AnnotationsJsonStr = JsonConvert.SerializeObject(cial.Select(x => x.AnnotationJsonStr).ToList());
            //}

            if (isRestoringSelection) {
                Parent.RestoreSelectionState(this);
            }

            OnPropertyChanged(nameof(IconResourceObj));
            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(TrayX));
            OnPropertyChanged(nameof(CanVerticallyScroll));
            OnPropertyChanged(nameof(IsTextItem));
            OnPropertyChanged(nameof(IsFileListItem));
            OnPropertyChanged(nameof(Next));
            OnPropertyChanged(nameof(Prev));
            OnPropertyChanged(nameof(CopyItemId));
            OnPropertyChanged(nameof(IsAnyBusy));
            //OnPropertyChanged(nameof(SourceViewModel));

            //MpMessenger.Send<MpMessageType>(MpMessageType.ContentItemsChanged, this);

            if (MpAvPersistentClipTilePropertiesHelper.IsPersistentTileContentEditable_ById(CopyItemId)) {
                IsContentReadOnly = false;
            }
            if (MpAvPersistentClipTilePropertiesHelper.IsPersistentTileTitleEditable_ById(CopyItemId)) {
                IsTitleReadOnly = false;
            }
            UpdateQueryOffset();
            //if(Parent.IsAnyAppendMode && HasAppendModel) {
            //    OnPropertyChanged(nameof(HasAppendModel));
            //}
            IsBusy = false;
        }

        public async Task InitTitleLayers() {
            if (IsPlaceholder) {
                return;
            }
            while (TransactionCollectionViewModel.IsAnyBusy) {
                await Task.Delay(100);
            }
            if (IsPlaceholder) {
                return;
            }

            int layerCount = 4;

            bool hasUserDefinedColor = !string.IsNullOrEmpty(CopyItem.ItemColor);

            List<string> hexColors = new List<string>();

            if (IconResourceObj is int iconId && iconId > 0 && !hasUserDefinedColor) {
                while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                    await Task.Delay(100);
                }
                var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);
                if (ivm == null) {
                    var icon = await MpDataModelProvider.GetItemAsync<MpIcon>(iconId);
                    hexColors = icon.HexColors;
                } else {
                    hexColors = ivm.PrimaryIconColorList.ToList();
                }
            } else if (hasUserDefinedColor) {
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

            //IsBusy = wasBusy;
        }

        public void TriggerUnloadedNotification(bool removeQueryItem) {
            int unloaded_id = CopyItemId;
            CopyItem = null;
            if (removeQueryItem) {

                MpPlatform.Services.Query.PageTools.RemoveItemId(unloaded_id);
            }
            _queryOffsetIdx = -1;
            //QueryOffsetIdx = -1;
            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(CopyItemId));
            OnPropertyChanged(nameof(QueryOffsetIdx));
        }

        private MpIContentView _contentView;
        public MpIContentView GetContentView() {
            if (_contentView != null) {
                return _contentView;
            }
            if (MpPlatform.Services.ContentViewLocator == null) {
                // may need to reorganize load or block in a task to get this guy
                Debugger.Break();
            }
            if (IsAppendNotifier) {
                _contentView = MpPlatform.Services.ContentViewLocator.LocateModalContentView();
            } else {
                _contentView = MpPlatform.Services.ContentViewLocator.LocateContentView(CopyItemId);
            }

            return _contentView;
        }

        public async Task<MpAvClipTileViewModel> GetNeighborByRowOffsetAsync(int row_offset) {
            var items = IsPinned ? Parent.PinnedItems : Parent.Items;
            MpAvClipTileViewModel target_ctvm = null;
            if (row_offset < 0) {
                var pre_items =
                    items
                    .Where(x => x != this && x.ObservedBounds.Y < ObservedBounds.Y)
                    .OrderByDescending(x => x.ObservedBounds.Y);
                if (pre_items.Count() > 0) {
                    target_ctvm =
                       pre_items.Aggregate((a, b) =>
                               a.ObservedBounds.Location.Distance(ObservedBounds.Location) <
                               b.ObservedBounds.Location.Distance(ObservedBounds.Location) ? a : b);
                }

                if (target_ctvm == null) {
                    if (IsPinned || Parent.LayoutType == MpClipTrayLayoutType.Stack || Parent.HeadQueryIdx == 0) {
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
                if (post_items.Count() > 0) {
                    target_ctvm =
                        post_items.Aggregate((a, b) =>
                            a.ObservedBounds.Location.Distance(ObservedBounds.Location) <
                            b.ObservedBounds.Location.Distance(ObservedBounds.Location) ? a : b);
                }
                if (target_ctvm == null) {
                    if (IsPinned || Parent.LayoutType == MpClipTrayLayoutType.Stack || Parent.TailQueryIdx == Parent.MaxClipTrayQueryIdx) {
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
            if (IsPinned) {
                target_idx = Parent.PinnedItems.IndexOf(this) + col_offset;
                if (target_idx < 0) {
                    return null;
                }
                if (target_idx >= Parent.PinnedItems.Count) {
                    target_idx = target_idx - Parent.PinnedItems.Count;
                    if (target_idx < Parent.VisibleItems.Count()) {
                        if (Parent.DefaultScrollOrientation == Orientation.Horizontal) {
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
            } else if (target_idx >= MpPlatform.Services.Query.TotalAvailableItemsInQuery) {
                return null;
            }
            var neighbor_ctvm = Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == target_idx);
            if (neighbor_ctvm == null) {
                int neighbor_ciid = MpPlatform.Services.Query.PageTools.GetItemId(target_idx);
                Parent.ScrollIntoView(neighbor_ciid);
                await Task.Delay(100);
                while (Parent.IsAnyBusy) { await Task.Delay(100); }
                return Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == target_idx);
            }
            return neighbor_ctvm;
        }

        public void SubSelectAll() {
            AllowMultiSelect = true;
            IsSelected = true;
            AllowMultiSelect = false;
        }


        #region View Event Invokers


        public void RequestScrollToHome() {
            OnScrollToHomeRequest?.Invoke(this, null);
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
            if (IsPasting) {
                IsPasting = false;
                //Parent.RequestUnexpand();
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

        public void UpdateQueryOffset() {
            _queryOffsetIdx = MpPlatform.Services.Query.PageTools.GetItemOffsetIdx(CopyItemId);
            OnPropertyChanged(nameof(QueryOffsetIdx));
        }
        #region IDisposable

        public override void DisposeViewModel() {
            //base.Dispose();
            //PropertyChanged -= MpClipTileViewModel_PropertyChanged;
            //SelectionBgColorPopupViewModel.OnColorChanged -= SelectionBgColorPopupViewModel_OnColorChanged;
            //SelectionFgColorPopupViewModel.OnColorChanged -= SelectionFgColorPopupViewModel_OnColorChanged;
            ClearSelection();
            //TemplateCollection.Dispose();
        }

        #endregion
        public override string ToString() {
            return $"Tile[{QueryOffsetIdx}] {CopyItemTitle}";
        }
        #endregion

        #region Protected Methods

        #region DB Overrides

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc) {
                if (sc.CommandParameter == CopyItemId.ToString() && sc.ShortcutType == ShortcutType) {
                    OnPropertyChanged(nameof(SelfBindingRef));
                }
            } else if (e is MpCopyItemAnnotation cia && cia.CopyItemId == CopyItemId) {
                Dispatcher.UIThread.Post(async () => {
                    await InitializeAsync(CopyItem);
                    //wait for model to propagate then trigger view to reload
                    if (GetContentView() is MpIContentView cv) {
                        cv.LoadContentAsync().FireAndForgetSafeAsync();
                        //wv.PerformLoadContentRequestAsync().FireAndForgetSafeAsync();
                    }
                });
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
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
                        //wait for model to propagate then trigger view to reload
                        if (GetContentView() is MpIContentView cv) {
                            cv.LoadContentAsync().FireAndForgetSafeAsync();
                            //wv.PerformLoadContentRequestAsync().FireAndForgetSafeAsync();
                        }
                    });
                }
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
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
                case nameof(IsViewLoaded):
                    // true = recv'd notifyLoadComplete
                    // false = PublicHandle changed

                    OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(IsHovering):
                    Parent.OnPropertyChanged(nameof(Parent.CanScroll));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyHovering));
                    OnPropertyChanged(nameof(IsCornerButtonsVisible));
                    if (IsHovering) {
                        AutoCycleDetailsAsync().FireAndForgetSafeAsync(this);
                        Dispatcher.UIThread.Post(async () => {
                            while (IsHovering) {
                                OnPropertyChanged(nameof(CopyItemTitle));
                                await Task.Delay(3000);
                            }
                        });
                    }
                    break;
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        if (Parent.SelectedItem != this) {
                            Parent.SelectedItem = this;
                        }

                        Parent.ScrollIntoView(this);
                        if (!Parent.IsRestoringSelection) {
                            Parent.StoreSelectionState(this);
                        }
                    } else {
                        if (IsContentReadOnly) {
                            if (IsSubSelectionEnabled) {
                                IsSubSelectionEnabled = false;
                            }
                        }
                    }
                    OnPropertyChanged(nameof(IsCornerButtonsVisible));
                    Parent.NotifySelectionChanged();
                    break;
                case nameof(CopyItem):
                    //DetailCollectionViewModel.InitializeAsync().FireAndForgetSafeAsync();
                    if (CopyItem == null) {
                        break;
                    }
                    OnPropertyChanged(nameof(CopyItemData));
                    OnPropertyChanged(nameof(IsPlaceholder));
                    //UpdateDetails();
                    break;
                case nameof(IsPinned):
                    OnPropertyChanged(nameof(IsPlaceholder));
                    break;
                case nameof(IsTitleVisible):
                    OnPropertyChanged(nameof(TileTitleHeight));
                    OnPropertyChanged(nameof(BoundHeight));
                    OnPropertyChanged(nameof(TileContentHeight));
                    break;
                case nameof(IsSubSelectionEnabled):
                    Parent.OnPropertyChanged(nameof(Parent.CanScroll));
                    OnPropertyChanged(nameof(IsHorizontalScrollbarVisibile));
                    OnPropertyChanged(nameof(IsVerticalScrollbarVisibile));
                    if (!IsSubSelectionEnabled) {
                        MpAvMainWindowViewModel.Instance.LastDecreasedFocusLevelDateTime = DateTime.Now;
                    }
                    break;
                case nameof(IsTitleReadOnly):
                    if (IsTitleReadOnly) {
                        MpAvMainWindowViewModel.Instance.LastDecreasedFocusLevelDateTime = DateTime.Now;
                        MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsTitleEditableTile_ById(CopyItemId);
                        if (CopyItemTitle != _originalTitle) {
                            HasModelChanged = true;
                        }
                    } else {
                        MpAvPersistentClipTilePropertiesHelper.AddPersistentIsTitleEditableTile_ById(CopyItemId);
                        _originalTitle = CopyItemTitle;
                        IsTitleFocused = true;
                        IsSelected = true;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTitle));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTile));
                    break;
                case nameof(IsTitleFocused):
                    if (IsTitleReadOnly) {
                        break;
                    }
                    IsTitleReadOnly = true;
                    break;
                case nameof(IsContentReadOnly):
                    if (IsContentReadOnly) {
                        MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsContentEditableTile_ById(CopyItemId);
                    } else {
                        MpAvMainWindowViewModel.Instance.LastDecreasedFocusLevelDateTime = DateTime.Now;
                        if (!IsSelected) {
                            IsSelected = true;
                        }
                        MpAvPersistentClipTilePropertiesHelper.AddPersistentIsContentEditableTile_ById(CopyItemId);
                        IsTitleReadOnly = true;
                        OnPropertyChanged(nameof(IsTitleVisible));
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
                    break;
                case nameof(IsContextMenuOpen):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyTileContextMenuOpened));
                    break;
                case nameof(IsPasting):
                    Parent.OnPropertyChanged(nameof(Parent.IsPasting));
                    break;
                case nameof(IsDropOverTile):
                    if (IsDropOverTile && !IsSubSelectionEnabled) {
                        IsSubSelectionEnabled = true;
                    }
                    if (IsDropOverTile) {
                        Parent.NotifyDragOverTrays(true);
                    }

                    break;
                case nameof(IgnoreHasModelChanged):
                    if (IgnoreHasModelChanged) {
                        break;
                    }
                    if (HasModelChanged) {
                        MpConsole.WriteLine($"CopyItem '{CopyItem}' IgnoreHasModelChange flagged unset when model changed. Triggering write.");
                        OnPropertyChanged(nameof(HasModelChanged));
                    }
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        if (IgnoreHasModelChanged) {
                            // model batch updating in contentChanged response
                            MpConsole.WriteLine($"CopyItem '{CopyItem}' IgnoreHasModelChange flagged during model change, ignoring write.");
                            return;
                        }
                        //HasModelChanged = false;
                        //return;
                        if (CopyItemData.IsEmptyRichHtmlString()) {
                            // what IS this nasty shit??
                            Debugger.Break();

                            return;
                        }
                        if (CopyItemType == MpCopyItemType.Image && CopyItemData.StartsWith("<p>")) {
                            Debugger.Break();
                        }
                        //if(!MpAvCefNetApplication.UseCefNet && HasContentDataChanged) {
                        //    if(IsInitializing) {
                        //        MpConsole.WriteLine("Ignoring plain text mode initialize data overwrite");
                        //        HasContentDataChanged = false;
                        //        HasModelChanged = false;
                        //        return;
                        //    }
                        //}
                        if (!MpPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                            MpConsole.WriteLine("Ignoring plain text mode copyitem write for " + this);
                            HasModelChanged = false;
                            return;
                        }
                        Task.Run(async () => {
                            await CopyItem.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
                case nameof(CopyItemHexColor):
                    InitTitleLayers().FireAndForgetSafeAsync(this);
                    break;
                case nameof(IconResourceObj):
                    InitTitleLayers().FireAndForgetSafeAsync(this);
                    break;
                case nameof(CopyItemData):
                    OnPropertyChanged(nameof(EditorFormattedItemData));
                    break;

                case nameof(CanResize):
                    Parent.OnPropertyChanged(nameof(Parent.CanAnyResize));
                    break;
                case nameof(IsResizing):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyResizing));
                    if (!IsResizing) {
                        Parent.RefreshQueryTrayLayout();
                    }
                    break;
                case nameof(MinHeight):
                    BoundHeight = MinHeight;
                    TransactionCollectionViewModel.OnPropertyChanged(nameof(TransactionCollectionViewModel.DefaultTransactionPanelHeight));
                    break;
                case nameof(MinWidth):
                    BoundWidth = IsCustomWidth ? BoundWidth : MinWidth;

                    TransactionCollectionViewModel.OnPropertyChanged(nameof(TransactionCollectionViewModel.DefaultTransactionPanelWidth));
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
                    TransactionCollectionViewModel.OnPropertyChanged(nameof(TransactionCollectionViewModel.MaxWidth));
                    Parent.UpdateTileRectCommand.Execute(new object[] { Next, TrayRect });
                    break;
                case nameof(TrayLocation):
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
                    if (IsPlaceholder) {
                        break;
                    }
                    //if(Parent != null) {
                    //    Parent.ValidateQueryTray();
                    //}
                    //MpRect prevRect = Prev == null ? null : Prev.TrayRect;
                    //Parent.UpdateTileRectCommand.Execute(new object[] { this, prevRect });
                    Parent.UpdateTileRectCommand.Execute(this);
                    //OnPropertyChanged(nameof(TrayX));
                    break;
                case nameof(UnconstrainedContentDimensions):
                    if (CopyItemType != MpCopyItemType.Image ||
                        UnconstrainedContentDimensions == null ||
                        UnconstrainedContentDimensions.IsEmpty()) {
                        break;
                    }
                    CopyItemSize = UnconstrainedContentDimensions;
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
                                    notificationType: MpNotificationType.ModalContentFormatDegradation,
                                    title: "Data Degradation Warning",
                                    body: $"Editing in comptability mode will remove all rich formatting. Are you sure you wish to modify this?");

            if (result == MpNotificationDialogResultType.Ok) {
                _isContentReadOnly = false;
                OnPropertyChanged(nameof(IsContentReadOnly));
            }
        }

        private async Task AutoCycleDetailsAsync() {
            if (IsPlaceholder) {
                return;
            }
            DateTime lastCycle = DateTime.Now;
            TimeSpan cycle_delay = TimeSpan.FromMilliseconds(AUTO_CYCLE_DETAIL_DELAY_MS);
            while (IsHovering) {
                if (DateTime.Now - lastCycle > cycle_delay) {
                    lastCycle = DateTime.Now;
                    CycleDetailCommand.Execute(null);
                }
                await Task.Delay(100);
            }
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
            }, () => IsContentReadOnly);

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
                //BoundSize = Parent.DefaultQueryItemSize;
                BoundWidth = MinWidth;

                IsResizing = false;
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


        public ICommand ToggleEditContentCommand => new MpCommand(
            () => {
                if (!IsSelected && IsContentReadOnly) {
                    Parent.SelectedItem = this;
                }
                IsContentReadOnly = !IsContentReadOnly;

            }, () => IsTextItem);

        public ICommand ToggleHideTitleCommand => new MpCommand(
            () => {
                IsTitleVisible = !IsTitleVisible;
            }, () => !IsPlaceholder);

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

        public ICommand CopyToClipboardCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;
                var ds = GetContentView() as MpAvIDragSource;
                if (ds == null) {
                    Debugger.Break();
                    return;
                }
                var mpdo = await ds.GetDataObjectAsync(true);
                if (mpdo == null) {
                    // is none selected?
                    Debugger.Break();
                    IsBusy = false;
                    return;
                }
                await MpPlatform.Services.DataObjectHelperAsync.SetPlatformClipboardAsync(mpdo, true);

                // wait extra for cb watcher to know about data
                await Task.Delay(300);
                IsBusy = false;
            });

        public ICommand CycleDetailCommand => new MpCommand<object>(
            (args) => {
                if (args is int intArg) {
                    SelectedDetailIdx = intArg;
                } else if (args is MpCopyItemDetailType dt) {
                    SelectedDetailIdx = (int)dt;
                } else {
                    SelectedDetailIdx++;
                    if (args is string) {
                        // click on detail grid
                    }
                }

                if (SelectedDetailIdx >= Enum.GetNames(typeof(MpCopyItemDetailType)).Length) {
                    SelectedDetailIdx = 0;
                }
                OnPropertyChanged(nameof(DetailText));
            });

        public ICommand ShowContextMenuCommand => new MpCommand<object>(
            (args) => {
                var control = args as Control;
                if (control == null) {
                    return;
                }
                if (!IsSelected) {
                    IsSelected = true;
                }
                MpAvMenuExtension.ShowMenu(control, ContextMenuViewModel);
            }, (args) => {
                return CanShowContextMenu;
            });
        #endregion
    }
}
