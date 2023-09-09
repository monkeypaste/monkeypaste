using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipTileViewModel : MpAvViewModelBase<MpAvClipTrayViewModel>,
        MpIConditionalSelectableViewModel,
        MpICloseWindowViewModel,
        MpIDraggable,
        MpILocatorItem,
        MpIIconResource,
        MpIAsyncCollectionObject,
        MpIHighlightTextRangesInfoViewModel,
        MpIWindowHandlesClosingViewModel,
        MpIDisposableObject,
        MpIShortcutCommandViewModel,
        MpIWantsTopmostWindowViewModel,
        MpIScrollIntoView,
        MpIUserColorViewModel,
        MpIColorPalettePickerViewModel,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpITextContentViewModel,
        MpIContextMenuViewModel {

        #region Private Variables

        private string _originalTitle;

        #endregion

        #region Constants

        public const int AUTO_CYCLE_DETAIL_DELAY_MS = 5000;
        public const string TABLE_WRAPPER_CLASS_NAME = "quill-better-table-wrapper";

        public const double EDITOR_TOOLBAR_MIN_WIDTH = 870.0d;
        public const double PASTE_APPEND_TOOLBAR_MIN_WIDTH = 290.0d;
        public const double PASTE_TEMPLATE_TOOLBAR_MIN_WIDTH = 850.0d;

        #endregion

        #region Statics
        public static ObservableCollection<string> EditorToolbarIcons => new ObservableCollection<string>() {

        };
        #endregion

        #region Interfaces

        #region MpIDraggableViewModel Implementation
        bool MpIDraggable.IsDragging {
            get => IsTileDragging;
            set => IsTileDragging = value;
        }

        #endregion
        #region MpIDbModelId Implementation
        int MpILocatorItem.LocationId =>
            IsPinPlaceholder ? PinPlaceholderCopyItemId : CopyItemId;

        #endregion

        #region MpIHighlightTextRangesInfoViewModel Implementation
        ObservableCollection<MpTextRange> MpIHighlightTextRangesInfoViewModel.HighlightRanges { get; } = new ObservableCollection<MpTextRange>();
        int MpIHighlightTextRangesInfoViewModel.ActiveHighlightIdx { get; set; } = -1;

        #endregion

        #region MpIDisposableObject Implementation
        void MpIDisposableObject.Dispose() {
            TriggerUnloadedNotification(false);
        }
        #endregion

        #region MpIChildWindowViewModel Implementation

        public MpWindowType WindowType =>
            IsAppendNotifier ? MpWindowType.Append : MpWindowType.PopOut;

        public bool IsWindowOpen { get; set; }
        #endregion

        #region MpIWantsTopmostWindowViewModel Implementation

        public bool WantsTopmost =>
            true;

        #endregion

        #region MpIWindowHandlesClosingViewModel Implementation

        public bool IsWindowCloseHandled =>
            //(IsAppendNotifier && !WasCloseAppendWindowConfirmed) && 
            !IsFinalClosingState;

        #endregion

        #region MpIContextMenuViewModel Implementation

        public MpAvMenuItemViewModel ContextMenuViewModel =>
            IsSelected ? Parent.ContextMenuViewModel : null;

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

        #region MpIUserColorViewModel Implementation
        public string UserHexColor {
            get => CopyItemHexColor;
            set => CopyItemHexColor = value;
        }

        #endregion

        #region MpIColorPalettePickerViewModel Implementation

        public MpIAsyncCommand<object> PaletteColorPickedCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not string paletteItemData) {
                    return;
                }
                string new_color = string.Empty;
                if (paletteItemData == "custom") {
                    new_color = await Mp.Services.CustomColorChooserMenuAsync.ShowCustomColorMenuAsync(
                        selectedColor: CopyItemHexColor);
                } else {
                    new_color = paletteItemData;
                }
                if (!new_color.IsStringHexColor()) {
                    return;
                }
                MpAvMenuExtension.CloseMenu();
                CopyItemHexColor = new_color;
                await InitTitleLayersAsync();
            });

        #endregion

        #region MpIShortcutCommandViewModel Implementation

        public MpShortcutType ShortcutType =>
            MpShortcutType.PasteCopyItem;
        public string KeyString =>
            MpAvShortcutCollectionViewModel.Instance.GetViewModelCommandShortcutKeyString(this);

        public object ShortcutCommandParameter =>
            CopyItemId;
        ICommand MpIShortcutCommandViewModel.ShortcutCommand =>
            Parent == null ? null : Parent.PasteCopyItemByIdFromShortcutCommand;

        #endregion

        #region MpIConditionalSelectableViewModel Implementation

        public bool CanSelect =>
            !IsPinPlaceholder;

        private bool _isSelected;
        public bool IsSelected {
            get => _isSelected;
            set {
                if (CopyItemTitle == "Text 333") {

                }
                if (_isSelected != value) {
                    // NOTE always triggering prop change when selecting
                    // to update LastSelectedDateTime to ensure
                    // tray's selected item is this one
                    _isSelected = value;
                }
                if (IsSelected && !CanSelect) {
                    MpDebug.Break("PinPlaceholder error, shouldn't be selectable");
                }
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public DateTime LastSelectedDateTime { get; set; }
        public DateTime? LastDeselectedDateTime { get; set; }


        #endregion

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
        public MpAvClipTileViewModel SelfRef { get; private set; }

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
                if (IsPlaceholder || Parent == null || IsWindowOpen) {
                    return null;
                }
                if (IsPinned) {
                    int pinIdx = Parent.InternalPinnedItems.IndexOf(this);
                    return Parent.InternalPinnedItems.FirstOrDefault(x => Parent.InternalPinnedItems.IndexOf(x) == pinIdx + 1);
                }
                if (IsQueryItem) {
                    // during pin tile create, it doesn't know its pinned yet so need to make sure its a query item here
                    return Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == QueryOffsetIdx + 1);
                }
                return null;
            }
        }

        public MpAvClipTileViewModel Prev {
            get {
                if (IsPlaceholder || Parent == null || QueryOffsetIdx == 0 || IsWindowOpen) {
                    return null;
                }
                if (IsPinned) {
                    int pinIdx = Parent.InternalPinnedItems.IndexOf(this);
                    return Parent.InternalPinnedItems.FirstOrDefault(x => Parent.InternalPinnedItems.IndexOf(x) == pinIdx - 1);
                }
                if (IsQueryItem) {
                    // during pin tile create, it doesn't know its pinned yet so need to make sure its a query item here
                    return Parent.Items.FirstOrDefault(x => x.QueryOffsetIdx == QueryOffsetIdx - 1);
                }
                return null;
            }
        }

        public MpAvClipTileViewModel PinnedItemForThisPlaceholder =>
            IsPinPlaceholder &&
            Parent != null &&
            Parent.PinnedItems.FirstOrDefault(x => x.CopyItemId == PinPlaceholderCopyItemId)
                is MpAvClipTileViewModel pin_ctvm ?
                pin_ctvm : null;

        public MpAvClipTileViewModel PlaceholderForThisPinnedItem =>
            IsPinned &&
            Parent.QueryItems.FirstOrDefault(x => x.PinPlaceholderCopyItemId == CopyItemId)
                is MpAvClipTileViewModel pin_placeholder_ctvm ?
                pin_placeholder_ctvm : null;

        #endregion

        #region Appearance

        public string PinPlaceholderLabel =>
            PinnedItemForThisPlaceholder == null ?
                string.Empty :
                PinnedItemForThisPlaceholder.CopyItemTitle;
        public string CapToolTipText =>
            IsNextTrashedByAccount ?
                UiStrings.AccountNextTrashToolTipText :
                IsNextRemovedByAccount ?
                    UiStrings.AccountNextRemoveToolTipText :
            string.Empty;

        public int[] TitleLayerZIndexes { get; private set; } = Enumerable.Range(1, 3).ToArray();
        public string[] TitleLayerHexColors { get; private set; } = Enumerable.Repeat(MpSystemColors.Transparent, 4).ToArray();

        public string DetailTooltipText { get; set; }
        public string DetailText {
            get {
                DetailTooltipText = string.Empty;
                string detailText = string.Empty;
                switch ((MpCopyItemDetailType)SelectedDetailIdx) {
                    //created
                    case MpCopyItemDetailType.DateTimeCreated:
                        DetailTooltipText = MpAvDateTimeToStringConverter.Instance.Convert(CopyDateTime, null, MpAvDateTimeToStringConverter.LITERAL_DATE_TIME_FORMAT, null) as string;
                        detailText = string.Format(UiStrings.ClipTileDetailCreated, CopyDateTime.ToReadableTimeSpan());
                        break;
                    case MpCopyItemDetailType.DataSize:
                        switch (CopyItemType) {
                            case MpCopyItemType.Image:
                                detailText = string.Format(UiStrings.ClipTileDetailDimImage, CopyItemSize1, CopyItemSize2);
                                break;
                            case MpCopyItemType.Text:
                                detailText = string.Format(UiStrings.ClipTileDetailDimText, CopyItemSize1, CopyItemSize2);
                                break;
                            case MpCopyItemType.FileList:
                                detailText = string.Format(UiStrings.ClipTileDetailDimFiles, CopyItemSize1, CopyItemSize2);
                                break;
                        }
                        break;
                    //# copies/# pastes
                    case MpCopyItemDetailType.UsageStats:
                        detailText = string.Format(UiStrings.ClipTileDetailUsage, CopyCount, PasteCount);
                        break;
                    default:
                        break;
                }
                return detailText;
            }
        }

        #endregion

        #region Layout

        public double ActualContentHeight { get; set; }
        public double MaxTitleHeight =>
            IsExpanded ? 40 : 20;

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

        public MpRect ObservedBounds { get; set; }

        //public double MinWidth =>
        //    IsPinned ? Parent.DefaultPinItemWidth : Parent.DefaultQueryItemWidth;
        public double MinWidth {
            get {
                if (Parent == null) {
                    return 0;
                }
                if (!IsSubSelectionEnabled || !IsWindowOpen) {
                    return IsPinned ? Parent.DefaultPinItemWidth : Parent.DefaultQueryItemWidth;
                }
                if (IsContentReadOnly) {
                    if (HasTemplates) {
                        return PASTE_TEMPLATE_TOOLBAR_MIN_WIDTH;
                    }
                    if (IsAppendNotifier) {
                        return PASTE_APPEND_TOOLBAR_MIN_WIDTH;
                    }
                    if (IsWindowOpen) {
                        return 50;
                    }
                }
                return EDITOR_TOOLBAR_MIN_WIDTH;
            }
        }
        public double MinHeight =>
            Parent == null ? 0 : IsPinned ? Parent.DefaultPinItemHeight : Parent.DefaultQueryItemHeight;


        public double MaxWidth =>
             double.PositiveInfinity;
        public double MaxHeight =>
            double.PositiveInfinity;

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

        public double TrayX { get; set; }// => TrayLocation.X;
        public double TrayY { get; set; }// => TrayLocation.Y;
        public MpPoint TrayLocation =>
            new MpPoint(TrayX, TrayY);
        //public MpPoint TrayLocation { get; set; } = MpPoint.Zero;

        public double ObservedWidth { get; set; }
        public double ObservedHeight { get; set; }
        public double BoundWidth { get; set; }
        public double BoundHeight { get; set; }
        public MpRect TrayRect =>
            new MpRect(TrayX, TrayY, BoundWidth, BoundHeight);

        public MpRect ScreenRect =>
            Parent == null ? MpRect.Empty : new MpRect(TrayLocation - Parent.ScrollOffset, new MpSize(BoundWidth, BoundHeight));


        public double ReadOnlyWidth => MinWidth;
        public double ReadOnlyHeight => MinHeight;


        public double EditableWidth {
            get {
                if (Parent == null) {
                    return 0;
                }
                if (HasTemplates) {
                    return PASTE_TEMPLATE_TOOLBAR_MIN_WIDTH;
                }
                if (IsPinned) {
                    if (IsWindowOpen) {
                        return BoundWidth;
                    }
                    return Math.Min(EDITOR_TOOLBAR_MIN_WIDTH, Parent.ObservedPinTrayScreenWidth);
                }
                if (Parent.LayoutType == MpClipTrayLayoutType.Grid) {
                    return BoundWidth;
                }
                return Math.Min(EDITOR_TOOLBAR_MIN_WIDTH, Parent.ObservedQueryTrayScreenWidth);
            }
        }

        public double EditableHeight {
            get {
                if (Parent == null) {
                    return 0;
                }
                if (IsExpanded) {
                    return BoundHeight;
                }
                if (IsPinned) {
                    return Math.Max(Parent.PinTrayFixedDimensionLength, BoundHeight);
                }

                return Math.Max(Parent.QueryTrayFixedDimensionLength, BoundHeight);
            }
        }
        #endregion

        #region State

        bool IsContentChangeModelChange { get; set; }
        public bool CanDrop {
            get {
                if (MpAvDoDragDropWrapper.DragDataObject == null) {
                    return true;
                }
                if (CopyItemType == MpCopyItemType.Text) {
                    return true;
                }
                if (CopyItemType == MpCopyItemType.FileList &&
                    MpAvDoDragDropWrapper.SourceControl != null &&
                    MpAvDoDragDropWrapper.SourceControl.DataContext == this) {
                    return true;
                }
                return false;
            }
        }
        public object[] ContentTemplateParam =>
            new object[] { CopyItemType, this };

        public double PinButtonAngle =>
            IsPinButtonHovering ||
            IsPinned ?
                -45 : 0;
        public bool IsPinButtonHovering { get; set; }
        public int PinPlaceholderCopyItemId { get; set; }
        public bool IsPinPlaceholder =>
            PinPlaceholderCopyItemId > 0;

        public bool HasPinPlaceholder =>
            PlaceholderForThisPinnedItem != null;

        public bool IsPlaceholder =>
           CopyItem == null &&
           !IsPinPlaceholder;

        public bool IsAnyPlaceholder =>
            IsPlaceholder ||
            IsPinPlaceholder;

        public bool IsFrozen =>
            IsPinPlaceholder || IsTrashed;

        public bool IsNextTrashedByAccount =>
            Parent != null &&
            CopyItemId != 0 &&
            Mp.Services.AccountTools.LastCapInfo.NextToBeTrashed_ciid == CopyItemId;

        public bool IsNextRemovedByAccount =>
            Parent != null &&
            CopyItemId != 0 &&
            Mp.Services.AccountTools.LastCapInfo.NextToBeRemoved_ciid == CopyItemId;

        public bool IsAnyNextCapByAccount =>
            IsNextTrashedByAccount || IsNextRemovedByAccount;

        public bool IsTrashed =>
            MpAvTagTrayViewModel.Instance != null &&
            MpAvTagTrayViewModel.Instance.TrashedCopyItemIds.Contains(CopyItemId);
        public bool IsExpanded {
            get {
                //if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ||
                //    IsPinned) {
                //    return true;
                //}
                //return IsSelected;
                if (IsWindowOpen) {
                    return true;
                }
                return MpAvMainWindowViewModel.Instance.IsHorizontalOrientation;
            }
        }

        public string ShortcutTooltipText =>
            string.IsNullOrEmpty(KeyString) ?
                $"Assign Global Paste Shortcut for '{CopyItemTitle}'" :
                "Global Paste";


        public bool IsResizerEnabled =>
            //MpAvThemeViewModel.Instance.IsDesktop &&
            !IsWindowOpen &&
            !IsFrozen &&
            (IsPinned || (Parent != null && Parent.IsQueryItemResizeEnabled));

        public MpIEmbedHost EmbedHost =>
            GetContentView() as MpIEmbedHost;

        private int SelectedDetailIdx { get; set; } = 0;

        public bool IsOverDetailGrid { get; set; }
        public bool IsHovering { get; set; }
        public bool IsContentHovering { get; set; }

        public bool IsPlaceholderForThisPinnedItemHovering =>
            PlaceholderForThisPinnedItem != null &&
            PlaceholderForThisPinnedItem.IsHovering;

        public bool IsPinnedItemForThisPlaceholderHovering =>
            PinnedItemForThisPlaceholder != null &&
            PinnedItemForThisPlaceholder.IsHovering;

        public bool IsImplicitHover =>
            IsPlaceholderForThisPinnedItemHovering ||
            IsPinnedItemForThisPlaceholderHovering;
        public bool IsHoveringOverSourceIcon { get; set; } = false;

        public bool IsHostWindowActive {
            get {
                if (GetContentView() is Control c &&
                    TopLevel.GetTopLevel(c) is Window w) {
                    return w.IsActive;
                }
                return false;
            }
        }

        public WindowState PopOutWindowState { get; set; }

        #region Append
        //public bool IsAppendTrayItem {
        //    get {
        //        if (Parent == null || Parent.ModalClipTileViewModel == null) {
        //            return false;
        //        }
        //        return Parent.ModalClipTileViewModel.CopyItemId == CopyItemId && !IsAppendNotifier;
        //    }
        //}

        #region State
        public int AppendCount { get; set; } = 0;
        public bool IsAppendNotifier { get; set; }

        public bool WasCloseAppendWindowConfirmed { get; set; }
        #endregion

        #region Appearance


        #endregion

        #endregion

        public bool IsTrashing { get; set; }
        public bool IsDeleting { get; set; }
        public bool IsTrashOrDeleting =>
            IsTrashing || IsDeleting;
        public bool IsFinalClosingState { get; set; }
        //public string AnnotationsJsonStr { get; set; }
        public bool CanShowContextMenu { get; set; } = true;

        public bool HasTemplates { get; set; } = false;
        public bool HasEditableTable { get; set; }
        public bool IsFindAndReplaceVisible { get; set; } = false;
        public string TemplateRichHtml { get; set; }

        public bool IsAnyQueryCornerVisible =>
            Parent == null ? false : ScreenRect.IsAnyPointWithinOtherRect(Parent.QueryTrayScreenRect);
        public bool IsAllQueryCornersVisible =>
            Parent == null ? false : ScreenRect.IsAllPointWithinOtherRect(Parent.QueryTrayScreenRect);

        public bool IsDevToolsVisible { get; set; } = false;

        private bool _isEditorLoaded;
        public bool IsEditorLoaded {
            get => MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled ? _isEditorLoaded : true;
            set {
                if (_isEditorLoaded != value) {
                    _isEditorLoaded = value;
                    OnPropertyChanged(nameof(IsEditorLoaded));
                }
            }
        }

        public int ItemIdx {
            get {
                if (IsPinned) {
                    return Parent.InternalPinnedItems.IndexOf(this);
                }
                return Parent.Items.IndexOf(this);
            }
        }
        public bool IsTitleReadOnly { get; set; } = true;

        private bool _isContentReadOnly = true;
        public bool IsContentReadOnly {
            get => _isContentReadOnly;
            set {

                if (IsContentReadOnly != value) {
                    if (!value && !MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {
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

        public bool CanDisableSubSelection {
            get {
                return !IsAppendNotifier;
            }
        }

        public bool IsSubSelectionEnabled { get; set; } = false;


        private int _queryOffsetIdx = -1;
        public int QueryOffsetIdx =>
            _queryOffsetIdx;

        public bool IsHorizontalScrollbarVisibile { get; set; }

        public bool IsVerticalScrollbarVisibile { get; set; }

        public bool IsAnyScrollbarVisible =>
            IsHorizontalScrollbarVisibile ||
            IsVerticalScrollbarVisibile;

        public bool IsPinButtonVisible {
            get {
                return IsSelected || IsHovering ? true : false;
            }
        }

        public bool IsAnyBusy {
            get {
                if (IsBusy) {
                    return true;
                }
                //if (!IsAnyPlaceholder && !IsEditorLoaded && (!IsQueryItem || (IsQueryItem && IsAnyQueryCornerVisible))) {
                //    //if (GetContentView() is MpIContentView cv) {
                //    //    if (cv.IsViewInitialized) {
                //    //        cv.LoadContentAsync().FireAndForgetSafeAsync(this);
                //    //    } else {
                //    //        Dispatcher.UIThread.Post(async () => {
                //    //            if (cv is MpAvContentWebView cwv) {
                //    //                await cwv.LoadEditorAsync();
                //    //                await cwv.LoadContentAsync();
                //    //                OnPropertyChanged(nameof(IsAnyBusy));
                //    //            }
                //    //        });
                //    //    }
                //    //}
                //    return true;
                //}

                if (FileItemCollectionViewModel != null && FileItemCollectionViewModel.IsAnyBusy) {
                    return true;
                }

                if (TransactionCollectionViewModel.IsTransactionPaneOpen &&
                    TransactionCollectionViewModel.IsAnyBusy) {
                    return true;
                }
                if (IsAppendNotifier) {
                    return false;
                }
                return false;
            }
        }

        #region Scroll

        public double NormalizedVerticalScrollOffset { get; set; } = 0;

        public bool IsScrolledToHome => Math.Abs(NormalizedVerticalScrollOffset) <= 0.1;

        public bool IsScrolledToEnd => Math.Abs(NormalizedVerticalScrollOffset) >= 0.9;

        public double KeyboardScrollAmount { get; set; } = 0.2;

        #endregion

        public bool CanEdit =>
            IsTextItem;
        public bool IsTitleFocused { get; set; } = false;

        public bool IsFocusWithin {
            get {
                if (Mp.Services.FocusMonitor.FocusElement is Control c) {
                    return c.GetSelfOrAncestorDataContext<MpAvClipTileViewModel>() == this;
                }
                return false;
            }
        }

        public bool IsPasting { get; set; } = false;

        public bool IsCustomWidth =>
            Parent == null ?
            false :
            MpAvPersistentClipTilePropertiesHelper.IsTileHaveUniqueWidth(CopyItemId, QueryOffsetIdx);

        public bool IsCustomHeight =>
            Parent == null ?
            false :
            MpAvPersistentClipTilePropertiesHelper.IsTileHaveUniqueHeight(CopyItemId, QueryOffsetIdx);


        #region Drag & Drop

        public bool IsDropOverTile { get; set; } = false;

        public bool IsTileDragging { get; set; } = false;

        #endregion

        public bool IsQueryItem =>
            QueryOffsetIdx >= 0;
        public bool IsPinned =>
            Parent != null &&
            Parent.PinnedItems.Any(x => x.CopyItemId == CopyItemId);

        public bool IsResizable =>
            !IsAppendNotifier &&
            !IsFrozen;
        public bool CanResize { get; set; } = false;

        public bool IsResizing { get; set; } = false;


        public bool IsFileListItem => CopyItemType == MpCopyItemType.FileList;

        public bool IsTextItem => CopyItemType == MpCopyItemType.Text;

        private bool _isTitleVisible = true;
        public bool IsTitleVisible {
            get {
                if (!MpAvPrefViewModel.Instance.ShowContentTitles) {
                    return false;
                }
                if (IsAppendNotifier ||
                    IsFrozen ||
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
                if (IsFrozen) {
                    return false;
                }
                if (Mp.Services.PlatformInfo.IsDesktop) {
                    if (IsHovering && !IsAppendNotifier && !Parent.IsAnyDropOverTrays) {
                        return true;
                    }
                } else if (IsSelected) {
                    return true;
                }
                return false;

            }
        }

        public bool IsContentAndTitleReadOnly => IsContentReadOnly && IsTitleReadOnly;

        public bool IsContextMenuOpen { get; set; } = false;

        public DateTime TileCreatedDateTime { get; set; }
        #endregion

        #region Model

        public int CopyCount {
            get {
                if (IsAnyPlaceholder) {
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
                if (IsAnyPlaceholder) {
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
                if (IsAnyPlaceholder) {
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

        public DateTime CopyDateTime {
            get {
                if (CopyItem == null) {
                    return DateTime.MinValue;
                }
                return CopyItem.CopyDateTime;
            }
            set {
                if (CopyDateTime != value) {
                    CopyItem.CopyDateTime = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyDateTime));
                }
            }
        }

        public int CopyItemIconId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.IconId;
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

        public int CopyItemSize1 {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.ItemSize1;
            }
            set {
                if (CopyItemSize1 != value) {
                    CopyItem.ItemSize1 = value;
                    OnPropertyChanged(nameof(CopyItemSize1));
                }
            }
        }

        public int CopyItemSize2 {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.ItemSize2;
            }
            set {
                if (CopyItemSize2 != value) {
                    CopyItem.ItemSize2 = value;
                    OnPropertyChanged(nameof(CopyItemSize2));
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
                    if (CopyItemType != MpCopyItemType.Text && !MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {

                    }
                    //CopyItem.ItemData = value;
                    //HasModelChanged = true;

                    NotifyModelChanged(CopyItem, nameof(CopyItem.ItemData), value);
                    OnPropertyChanged(nameof(CopyItemData));
                }
            }
        }

        public object IconResourceObj {
            get {
                if (IsNextTrashedByAccount) {
                    return MpContentCapInfo.NEXT_TRASH_IMG_RESOURCE_KEY;
                }
                if (IsNextRemovedByAccount) {
                    return MpContentCapInfo.NEXT_REMOVE_IMG_RESOURCE_KEY;
                }
                if (CopyItemType == MpCopyItemType.FileList &&
                    FileItemCollectionViewModel != null &&
                    FileItemCollectionViewModel.Items.Count > 0) {
                    return FileItemCollectionViewModel.Items.FirstOrDefault().IconBase64;
                }
                if (CopyItemIconId == 0) {
                    return MpBase64Images.QuestionMark;
                }
                return CopyItemIconId;
                //return TransactionCollectionViewModel.CreateTransaction == null ?
                //            MpDefaultDataModelTools.ThisAppIconId :
                //            TransactionCollectionViewModel.CreateTransaction.IconSourceObj;
            }
        }

        public string CopyItemHexColor {
            get {
                if (CopyItem == null ||
                    string.IsNullOrEmpty(CopyItem.ItemColor)) {
                    return string.Empty;
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

        public event EventHandler OnSyncModels;
        public event EventHandler<object> OnPastePortableDataObject;
        public event EventHandler<double> OnScrollWheelRequest;
        public event EventHandler OnMergeRequest;
        public event EventHandler OnClearTemplatesRequest;
        #endregion

        #region Constructors

        public MpAvClipTileViewModel() : this(null) {
            IsBusy = true;
        }

        public MpAvClipTileViewModel(MpAvClipTrayViewModel parent) : base(parent) {
            TileCreatedDateTime = DateTime.Now;
            PropertyChanged += MpClipTileViewModel_PropertyChanged;
            FileItemCollectionViewModel = new MpAvFileItemCollectionViewModel(this);
            IsBusy = true;

            //this.WhenActivated((CompositeDisposable disposables) => {
            //    /* handle activation */
            //    Disposable
            //        .Create(() => { /* handle deactivation */ })
            //        .DisposeWith(disposables);
            //});
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(
            MpCopyItem ci,
            int queryOffset = -1,
            bool isRestoringSelection = false) {
            await Task.Delay(1);
            IsBusy = true;

            bool is_query_item = queryOffset >= 0;
            bool is_pinned_item = !is_query_item && ci != null && ci.Id > 0;
            bool is_reload =
                (CopyItemId == 0 && ci == null) ||
                (ci != null && CopyItemId == ci.Id) ||
                (ci != null && PinPlaceholderCopyItemId == ci.Id && queryOffset >= 0);

            _contentView = null;
            if (!is_reload) {
                IsWindowOpen = false;
            }

            if (ci != null &&
                queryOffset >= 0 &&
                Parent != null &&
                Parent.PinnedItems.Any(x => x.CopyItemId == ci.Id)) {
                // pin placeholder item
                PinPlaceholderCopyItemId = ci.Id;
                // NOTE ensure model is null on pin placeholders
                CopyItem = null;
            } else {
                // normal tile (or placeholder)
                PinPlaceholderCopyItemId = 0;
                // NOTE FileItems are init'd before ciid is set so Items are busy when WebView is loading content
                FileItemCollectionViewModel.InitializeAsync(ci).FireAndForgetSafeAsync(this);

                CopyItem = ci;
                CycleDetailCommand.Execute(0);
                TransactionCollectionViewModel.InitializeAsync(CopyItemId).FireAndForgetSafeAsync(this);
                InitTitleLayersAsync().FireAndForgetSafeAsync(this);

                if (isRestoringSelection) {
                    Parent.RestoreSelectionState(this);
                }
            }

            UpdateQueryOffset(queryOffset);
            RestorePersistentState();


            OnPropertyChanged(nameof(IconResourceObj));
            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(IsTextItem));
            OnPropertyChanged(nameof(IsFileListItem));
            OnPropertyChanged(nameof(CopyItemId));
            OnPropertyChanged(nameof(IsAnyBusy));
            OnPropertyChanged(nameof(KeyString));

            OnPropertyChanged(nameof(TrayX));
            OnPropertyChanged(nameof(TrayY));
            OnPropertyChanged(nameof(Next));
            OnPropertyChanged(nameof(Prev));
            OnPropertyChanged(nameof(IsPinPlaceholder));
            OnPropertyChanged(nameof(IsFrozen));
            OnPropertyChanged(nameof(IsPinned));
            OnPropertyChanged(nameof(PinPlaceholderLabel));
            OnPropertyChanged(nameof(IsResizable));
            OnPropertyChanged(nameof(IsTrashed));
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(IsImplicitHover));

            if (!MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled ||
                SelfRef == null) {
                // NOTE in compatibility mode content template must be reselected
                // and for overall efficiency re-setting datacontext is better than
                // locating this tile from the view side (changing contentControl.content to id)
                //OnPropertyChanged(nameof(ContentTemplateParam));
                SelfRef = null;
                SelfRef = this;
            }
            IsBusy = false;
        }

        public async Task InitTitleLayersAsync() {
            TitleLayerZIndexes = Enumerable.Range(1, 3).ToArray();
            TitleLayerHexColors = Enumerable.Repeat(MpSystemColors.Transparent, 4).ToArray();

            if (IsAnyPlaceholder) {
                return;
            }

            int layer_seed = -1;
            List<string> hexColors = await GetTitleColorsAsync();
            if (IsAnyPlaceholder) {
                return;
            }
            for (int i = 0; i < hexColors.Count; i++) {
                // randomize alpha and layer order so its constant but unique for item
                char let = PublicHandle.ToUpper()[i];
                int seed;
                if (let <= '9') {
                    seed = (int)(let - '0');
                } else {
                    seed = (int)(let - 'A') + 10;
                }
                // seed is 0-15

                int alpha_range = 120 - 40;
                int alpha = 40 + (int)((double)alpha_range * ((double)seed / (double)15));
                hexColors[i] = hexColors[i].AdjustAlpha((double)alpha / 255d);

                if (i == 0) {
                    // just use first seed for layer groups

                    // group seed into 6 possible layer orientations
                    // 1,2,3 | 1,3,2 | 2,1,3 | 2,3,1 | 3,1,2 | 3,2,1
                    layer_seed = (int)((((double)seed / (double)15) * 18) / 3);
                }
            }
            TitleLayerHexColors = hexColors.ToArray();
            //MpConsole.WriteLine($"Layer Group for '{this}' is {layer_seed}");
            switch (layer_seed) {
                case 0:
                    TitleLayerZIndexes = new int[] { 1, 2, 3 };
                    break;
                case 1:
                    TitleLayerZIndexes = new int[] { 1, 3, 2 };
                    break;
                case 2:
                    TitleLayerZIndexes = new int[] { 2, 1, 3 };
                    break;
                case 3:
                    TitleLayerZIndexes = new int[] { 2, 3, 1 };
                    break;
                case 4:
                    TitleLayerZIndexes = new int[] { 3, 1, 2 };
                    break;
                case 5:
                    TitleLayerZIndexes = new int[] { 3, 2, 1 };
                    break;
            }
            //TitleLayerHexColors = hexColors.Select((x, i) => x.AdjustAlpha((double)MpRandom.Rand.Next(40, 120) / 255)).ToArray();
            //TitleLayerZIndexes = new List<int> { 1, 2, 3 }.Randomize().ToArray();
        }

        private async Task<List<string>> GetTitleColorsAsync() {
            // layer notes:
            // -colors decided by item color, source colors and link tag colors
            // -IconId is primary source icon
            int layerCount = 4;

            List<string> hexColors = new List<string>();
            var tagColors = await MpDataModelProvider.GetTagColorsForCopyItemAsync(CopyItemId);
            if (tagColors.Any()) {
                hexColors.AddRange(tagColors);
            }
            if (hexColors.Count < layerCount) {
                while (MpAvIconCollectionViewModel.Instance.IsAnyBusy) {
                    await Task.Delay(100);
                }
                if (MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == CopyItemIconId) is MpAvIconViewModel ivm) {
                    hexColors.AddRange(ivm.PrimaryIconColorList);
                } else {
                    var icon = await MpDataModelProvider.GetItemAsync<MpIcon>(CopyItemIconId);
                    if (icon != null) {
                        hexColors.AddRange(icon.HexColors);
                    }
                }
            }
            int to_add = layerCount - hexColors.Count;
            while (to_add > 0) {
                hexColors.Add(MpColorHelpers.GetRandomHexColor());
                to_add--;
            }

            // BUG very intermittently hexColors is list of nulls I think
            // its race condition something or other w/ transaction vm but
            // select fixes null here
            hexColors =
                hexColors
                .Take(layerCount)
                //.Select(x => string.IsNullOrEmpty(x) ? MpColorHelpers.GetRandomHexColor() : x)
                .ToList();

            return hexColors;
        }

        public void TriggerUnloadedNotification(bool removeQueryItem, bool clearPersistentProps = true, bool unloadAsPlaceholder = false) {
            if (clearPersistentProps && !IsPinPlaceholder) {
                MpAvPersistentClipTilePropertiesHelper.RemoveProps(CopyItemId);
            } else {
                // don't clear, occurs when query tile is pinning
            }

            if (removeQueryItem && QueryOffsetIdx >= 0) {
                // query item deleted
                Parent.QueryItems.Where(x => x.QueryOffsetIdx > QueryOffsetIdx).ForEach(x => x.UpdateQueryOffset(x.QueryOffsetIdx - 1));
            }
            PinPlaceholderCopyItemId = unloadAsPlaceholder ? CopyItemId : 0;
            CopyItem = null;
            UpdateQueryOffset(-1);
            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(CopyItemId));
            OnPropertyChanged(nameof(QueryOffsetIdx));
            OnPropertyChanged(nameof(IsPinPlaceholder));
        }

        public void OpenPopOutWindow(MpAppendModeType amt) {
            IsAppendNotifier = amt != MpAppendModeType.None;
            if (!IsWindowOpen) {
                MpAvPersistentClipTilePropertiesHelper.RemoveUniqueSize_ById(CopyItemId, QueryOffsetIdx);

                var pow = CreatePopoutWindow();
                var ws = amt == MpAppendModeType.None ? new Size(500, 500) : new Size(350, 250);
                pow.Width = ws.Width;
                pow.Height = ws.Height;
                if (amt == MpAppendModeType.None) {
                    pow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                } else {
                    pow.WindowStartupLocation = WindowStartupLocation.Manual;
                    pow.Position = MpAvNotificationPositioner.GetSystemTrayWindowPosition(pow);
                }
                pow.ShowChild();
            }

            OnPropertyChanged(nameof(IsWindowOpen));
            OnPropertyChanged(nameof(IsResizerEnabled));
            OnPropertyChanged(nameof(IsTitleVisible));
            OnPropertyChanged(nameof(WantsTopmost));

            if (this is MpIWantsTopmostWindowViewModel wtwvm) {
                wtwvm.OnPropertyChanged(nameof(wtwvm.WantsTopmost));
            }
            if (this is MpICloseWindowViewModel cwvm) {
                cwvm.OnPropertyChanged(nameof(cwvm.IsWindowOpen));
            }
        }

        public MpAvDataObject GetDataObjectByModel(bool isDnd, MpPortableProcessInfo target_pi) {
            // TODO need to figure out this flow w/ app paste infos...
            return CopyItem.ToAvDataObject(forceFormats: GetOleFormats(isDnd, target_pi));
        }

        public static List<string> GetDefaultOleFormats(MpCopyItemType itemTypem, bool isDnd) {
            List<string> req_formats = new() {
                MpPortableDataFormats.Text,
                MpPortableDataFormats.AvFiles,
                MpPortableDataFormats.AvHtml_bytes,
                MpPortableDataFormats.CefHtml,
            };
            if (isDnd && MpAvExternalDropWindowViewModel.Instance.IsDropWidgetEnabled) {
                // initialize target with all possible formats set to null
                req_formats = MpPortableDataFormats.RegisteredFormats.ToList();

            } else if (MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                // initialize target with default format for type
                switch (itemTypem) {
                    case MpCopyItemType.Text:
                        break;
                    case MpCopyItemType.Image:
                        req_formats.Add(MpPortableDataFormats.AvPNG);
                        break;
                    case MpCopyItemType.FileList:
                        break;
                }
            }
            return req_formats;
        }
        public string[] GetOleFormats(bool isDnd, MpPortableProcessInfo target_pi = null) {
            IEnumerable<string> req_formats = GetDefaultOleFormats(CopyItemType, isDnd);
            //if (target_pi != null &&
            //    MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(target_pi)
            //        is MpAvAppViewModel avm &&
            //   !avm.OleFormatInfos.IsEmpty) {
            //    // override item type defaults and return formats by app 
            //    return avm.OleFormatInfos.Items.Select(x => x.FormatName).ToArray();
            //}
            if (target_pi != null) {
                var preset_ids = MpAvAppCollectionViewModel.Instance.GetAppCustomOlePresetsByProcessInfo(target_pi, false);
                if (preset_ids != null) {
                    req_formats =
                        MpAvClipboardHandlerCollectionViewModel.Instance
                        .AllWriterPresets
                        .Where(x => preset_ids.Contains(x.PresetId))
                        .Select(x => x.ClipboardFormat.formatName);
                }
            }
            return req_formats.ToArray();
        }

        private MpIContentView _contentView;
        public MpIContentView GetContentView() {
            if (_contentView != null) {
                // view already stored, verify this is data context or reset
                if (_contentView is Control c &&
                    c.DataContext == this) {
                    return _contentView;
                }
                _contentView = null;
            }
            if (Mp.Services.ContentViewLocator == null) {
                // may need to reorganize load or block in a task to get this guy
                //MpDebug.Break();
                return null;
            }
            _contentView = Mp.Services.ContentViewLocator.LocateContentView(CopyItemId);

            return _contentView;
        }


        #region View Event Invokers

        public void RequestSyncModel() {
            OnSyncModels?.Invoke(this, null);
        }

        public void RequestPastePortableDataObject(object portableDataObjectOrCopyItem) {
            OnPastePortableDataObject?.Invoke(this, portableDataObjectOrCopyItem);
        }


        public void RequestMerge() {
            OnMergeRequest?.Invoke(this, null);
        }

        public void RequestClearHyperlinks() {
            OnClearTemplatesRequest?.Invoke(this, null);
        }

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

        public void UpdateQueryOffset(int forceOffsetIdx) {
            // NOTE can't signify default by -1 since pinned tiles have -1 so using -2
            _queryOffsetIdx = forceOffsetIdx;
            MpAvPersistentClipTilePropertiesHelper.UpdateQueryOffsetIdx(CopyItemId, _queryOffsetIdx);
            OnPropertyChanged(nameof(QueryOffsetIdx));
        }
        public int GetRecyclePriority(bool? isLoadMoreTail) {
            if (Parent == null) {
                return 0;
            }
            if (IsPlaceholder ||
                !isLoadMoreTail.HasValue ||
                QueryOffsetIdx < 0) {
                return int.MaxValue;
            }
            if (isLoadMoreTail.Value) {
                // lowest idx has highest priority
                return int.MaxValue - QueryOffsetIdx;
            }
            // highest idx has highest priority
            return QueryOffsetIdx;
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

            if (IsPinPlaceholder) {
                return $"[PIN HOLDER]Tile Idx: {QueryOffsetIdx} Id: {PinPlaceholderCopyItemId} Title: '{PinPlaceholderLabel}'";
            }
            return $"Tile Idx: {QueryOffsetIdx} Id: {CopyItemId} Title: '{CopyItemTitle}'";
        }
        #endregion

        #region Protected Methods

        protected override void NotifyModelChanged(object model, string changedPropName, object newVal) {
            if (changedPropName == nameof(CopyItem.ItemData)) {
                // NOTE this is always unset with HasModelChanged and used to reduce processing (maybe unnecessary but just seems safer)
                IsContentChangeModelChange = true;
            }
            base.NotifyModelChanged(model, changedPropName, newVal);
        }
        #region DB Overrides
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
                case nameof(IsEditorLoaded):
                    // true = recv'd notifyLoadComplete
                    // false = PublicHandle changed

                    OnPropertyChanged(nameof(IsAnyBusy));
                    OnPropertyChanged(nameof(EmbedHost));
                    break;
                case nameof(CopyItemTitle):
                    if (Parent != null &&
                       IsPinned &&
                       Parent.QueryItems.FirstOrDefault(x => x.PinPlaceholderCopyItemId == CopyItemId)
                            is MpAvClipTileViewModel pp_ctvm) {
                        pp_ctvm.OnPropertyChanged(nameof(pp_ctvm.PinPlaceholderLabel));
                    }
                    break;
                case nameof(IsHovering):
                    // refresh busy
                    OnPropertyChanged(nameof(IsAnyBusy));
                    OnPropertyChanged(nameof(KeyString));
                    Parent.OnPropertyChanged(nameof(Parent.CanScroll));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyHovering));
                    Parent.OnPropertyChanged(nameof(Parent.CanTouchScroll));
                    OnPropertyChanged(nameof(IsCornerButtonsVisible));
                    if (PinnedItemForThisPlaceholder != null) {
                        PinnedItemForThisPlaceholder.OnPropertyChanged(nameof(PinnedItemForThisPlaceholder.IsPinnedItemForThisPlaceholderHovering));
                    }
                    if (PlaceholderForThisPinnedItem != null) {
                        PlaceholderForThisPinnedItem.OnPropertyChanged(nameof(PlaceholderForThisPinnedItem.IsPlaceholderForThisPinnedItemHovering));
                    }
                    break;
                case nameof(IsPinnedItemForThisPlaceholderHovering):
                case nameof(IsPlaceholderForThisPinnedItemHovering):
                    OnPropertyChanged(nameof(IsImplicitHover));
                    break;
                case nameof(IsOverDetailGrid):
                    if (!IsOverDetailGrid) {
                        break;
                    }

                    CycleDetailCommand.Execute(null);
                    break;
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                        if (Parent.SelectedItem != this) {
                            Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                        }

                        Parent.ScrollIntoView(this);
                        if (!Parent.IsRestoringSelection) {
                            Parent.StoreSelectionState(this);
                        }
                    } else {
                        LastDeselectedDateTime = DateTime.Now;
                        if (IsContentReadOnly) {
                            if (IsSubSelectionEnabled) {
                                DisableSubSelectionCommand.Execute(null);
                            }
                        }
                    }
                    OnPropertyChanged(nameof(IsCornerButtonsVisible));
                    Parent.NotifySelectionChanged();
                    break;
                case nameof(CopyItem):
                    OnPropertyChanged(nameof(IsPlaceholder));
                    break;
                case nameof(PinPlaceholderCopyItemId):
                    OnPropertyChanged(nameof(IsPlaceholder));
                    break;
                case nameof(IsPinned):
                    OnPropertyChanged(nameof(PinButtonAngle));
                    OnPropertyChanged(nameof(IsPlaceholder));
                    ResetTileSizeToDefaultCommand.Execute(null);
                    break;

                case nameof(IsPinButtonHovering):
                    OnPropertyChanged(nameof(PinButtonAngle));
                    break;
                case nameof(IsTitleVisible):
                    OnPropertyChanged(nameof(BoundHeight));
                    break;
                case nameof(IsSubSelectionEnabled):
                    Parent.OnPropertyChanged(nameof(Parent.CanScroll));
                    Parent.OnPropertyChanged(nameof(Parent.CanTouchScroll));
                    OnPropertyChanged(nameof(IsHorizontalScrollbarVisibile));
                    OnPropertyChanged(nameof(IsVerticalScrollbarVisibile));
                    if (IsSubSelectionEnabled) {
                        MpAvPersistentClipTilePropertiesHelper.AddPersistentIsSubSelectableTile_ById(CopyItemId, QueryOffsetIdx);
                    } else {
                        MpAvMainWindowViewModel.Instance.LastDecreasedFocusLevelDateTime = DateTime.Now;
                        MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsSubSelectableTile_ById(CopyItemId, QueryOffsetIdx);
                    }

                    break;
                case nameof(IsTitleReadOnly):
                    if (IsTitleReadOnly) {
                        MpAvMainWindowViewModel.Instance.LastDecreasedFocusLevelDateTime = DateTime.Now;
                        MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsTitleEditableTile_ById(CopyItemId, QueryOffsetIdx);
                        if (CopyItemTitle != _originalTitle) {
                            HasModelChanged = true;
                        }
                    } else {
                        MpAvPersistentClipTilePropertiesHelper.AddPersistentIsTitleEditableTile_ById(CopyItemId, QueryOffsetIdx);
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
                        MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsContentEditableTile_ById(CopyItemId, QueryOffsetIdx);
                    } else {
                        MpAvMainWindowViewModel.Instance.LastDecreasedFocusLevelDateTime = DateTime.Now;
                        if (!IsSelected) {
                            IsSelected = true;
                        }
                        MpAvPersistentClipTilePropertiesHelper.AddPersistentIsContentEditableTile_ById(CopyItemId, QueryOffsetIdx);
                        IsTitleReadOnly = true;
                        OnPropertyChanged(nameof(IsTitleVisible));
                    }
                    MpMessenger.Send<MpMessageType>(IsContentReadOnly ? MpMessageType.IsReadOnly : MpMessageType.IsEditable, this);
                    Parent.OnPropertyChanged(nameof(Parent.IsQueryHorizontalScrollBarVisible));
                    Parent.OnPropertyChanged(nameof(Parent.IsQueryVerticalScrollBarVisible));

                    OnPropertyChanged(nameof(IsHorizontalScrollbarVisibile));
                    OnPropertyChanged(nameof(IsVerticalScrollbarVisibile));
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
                        if (CopyItemData.IsNullOrWhitespaceHtmlString()) {
                            // what IS this nasty shit??
                            MpDebug.Break($"Empty content write ignored", !MpCopyItem.IS_EMPTY_HTML_CHECK_ENABLED);

                            return;
                        }
                        if (CopyItemType == MpCopyItemType.Image && CopyItemData.StartsWith("<p>")) {
                            MpDebug.Break($"Image should not contain paragraph");
                        }
                        //if(!MpAvCefNetApplication.UseCefNet && HasContentDataChanged) {
                        //    if(IsInitializing) {
                        //        MpConsole.WriteLine("Ignoring plain text mode initialize data overwrite");
                        //        HasContentDataChanged = false;
                        //        HasModelChanged = false;
                        //        return;
                        //    }
                        //}

                        //if (!MpPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                        //    MpConsole.WriteLine("Ignoring plain text mode copyitem write for " + this);
                        //    HasModelChanged = false;
                        //    return;
                        //}
                        Task.Run(async () => {
                            await CopyItem.WriteToDatabaseAsync(IsContentChangeModelChange);
                            Dispatcher.UIThread.Post(() => {
                                // BUG i think this is a preview5 bug
                                // something w/ WeakEvent has no obj ref
                                // so set this back on ui thread
                                IsContentChangeModelChange = false;
                                HasModelChanged = false;

                            });
                        });
                    }
                    break;
                case nameof(CopyItemData):
                    OnPropertyChanged(nameof(EditorFormattedItemData));
                    break;

                case nameof(CanResize):
                    Parent.OnPropertyChanged(nameof(Parent.CanAnyResize));
                    Parent.OnPropertyChanged(nameof(Parent.CanTouchScroll));
                    break;
                case nameof(IsResizing):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyResizing));
                    if (!IsResizing) {
                        Parent.RefreshQueryTrayLayout();
                    }
                    break;
                case nameof(MinHeight):
                    BoundHeight = IsCustomHeight ? BoundHeight : MinHeight;
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
                case nameof(BoundWidth):
                case nameof(BoundHeight):
                    if (QueryOffsetIdx == 0) {

                    }
                    if (IsResizing) {
                        //this occurs when mainwindow is resized or user gives tile unique width
                        if (e1.PropertyName == nameof(BoundWidth)) {

                            MpAvPersistentClipTilePropertiesHelper.AddOrReplaceUniqueWidth_ById(CopyItemId, BoundWidth, QueryOffsetIdx);
                        } else {

                            MpAvPersistentClipTilePropertiesHelper.AddOrReplaceUniqueHeight_ById(CopyItemId, BoundHeight, QueryOffsetIdx);
                        }
                    }

                    if (Next == null) {
                        break;
                    }
                    TransactionCollectionViewModel.OnPropertyChanged(nameof(TransactionCollectionViewModel.MaxWidth));
                    Parent.UpdateTileLocationCommand.Execute(Next);
                    break;
                case nameof(TrayX):
                case nameof(TrayY):
                    if (Next == null) {
                        break;
                    }
                    Parent.UpdateTileLocationCommand.Execute(Next);
                    break;
                case nameof(QueryOffsetIdx):
                    if (IsPlaceholder) {
                        break;
                    }
                    Parent.UpdateTileLocationCommand.Execute(this);
                    break;
                case nameof(IsWindowOpen):
                    break;
                case nameof(CopyItemSize1):
                case nameof(CopyItemSize2):
                case nameof(CopyCount):
                case nameof(PasteCount):
                    OnPropertyChanged(nameof(DetailText));
                    break;
                case nameof(IsExpanded):
                    if (Parent != null && Parent.ListOrientation == Orientation.Horizontal) {
                        break;
                    }

                    break;
                case nameof(IsTileDragging):
                    Parent.OnPropertyChanged(nameof(Parent.CanTouchScroll));
                    break;
                case nameof(IsFrozen):
                    OnPropertyChanged(nameof(IsResizerEnabled));
                    break;
                case nameof(IsTrashing):
                case nameof(IsDeleting):
                    OnPropertyChanged(nameof(IsTrashOrDeleting));
                    break;
            }
        }

        private MpAvWindow CreatePopoutWindow() {
            int orig_ciid = CopyItemId;

            var pow = new MpAvWindow() {
                DataContext = this,
                ShowInTaskbar = true,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("AppIcon", null, null, null) as WindowIcon,
                Content = new MpAvClipTileView(),
                Background = Brushes.Transparent,
                CornerRadius = Mp.Services.PlatformResource.GetResource<CornerRadius>("TileCornerRadius")
            };
            //pow.Classes.Add("tileWindow");
            //pow.Classes.Add("fadeIn");
            //pow.Classes.Add("fadeOut");

            #region Window Bindings

            pow.Bind(
                Window.WindowStateProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(PopOutWindowState),
                    Mode = BindingMode.TwoWay
                });

            pow.Bind(
                Window.MinWidthProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(MinWidth)
                });

            pow.Bind(
                Window.TitleProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(CopyItemTitle),
                    Converter = MpAvStringToWindowTitleConverter.Instance
                });

            if (pow.Content is Control c) {
                // BUG hover doesn't work binding to window
                c.Bind(
                    MpAvIsHoveringExtension.IsHoveringProperty,
                    new Binding() {
                        Source = this,
                        Path = nameof(IsHovering),
                        Mode = BindingMode.TwoWay
                    });
                MpAvIsHoveringExtension.SetIsEnabled(c, true);
            }
            #endregion

            #region Selection
            EventHandler activate_handler = (s, e) => {
                Parent.SelectClipTileCommand.Execute(orig_ciid); ;
            };

            #endregion

            EventHandler open_handler = null;
            open_handler = (s, e) => {
                if (GetContentView() is MpIContentView cv &&
                    !IsContentReadOnly) {
                    OnPropertyChanged(nameof(IsTitleVisible));
                    cv.LoadContentAsync().FireAndForgetSafeAsync(this);
                }
                pow.Opened -= open_handler;
            };
            #region CLOSE

            EventHandler<WindowClosingEventArgs> closing_handler = null;
            closing_handler = (s, e) => {
                MpConsole.WriteLine($"tile popout closing called. reason '{e.CloseReason}' programmatic '{e.IsProgrammatic}' final: {IsFinalClosingState}");
                if (IsFinalClosingState) {
                    // handled in secondary closing
                    return;
                }
                if (Parent == null ||
                    !IsAppendNotifier ||
                    WasCloseAppendWindowConfirmed) {
                    // closing pop out confirmed but need to store state before view is disposed
                    // which is async so store it, then retrigger close knowing its final
                    IsFinalClosingState = true;
                    pow.Closing -= closing_handler;
                    e.Cancel = true;
                    IsBusy = false;

                    Dispatcher.UIThread.Post(async () => {
                        await Parent.UnpinTileCommand.ExecuteAsync(this);
                        pow.Close();
                    });
                    return;
                }
                // reject close and show confirm ntf
                IsBusy = true;
                e.Cancel = true;
                pow.IsHitTestVisible = false;
                Dispatcher.UIThread.Post(async () => {
                    var result = await
                        Mp.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync(
                            title: UiStrings.CommonNtfConfirmTitle,
                            message: "Are you sure you want to finish appending?",
                            iconResourceObj: "QuestionMarkImage",
                            owner: pow,
                            ntfType: MpNotificationType.ConfirmEndAppend);
                    if (result) {
                        // allow close
                        WasCloseAppendWindowConfirmed = true;
                        await Parent.DeactivateAppendModeCommand.ExecuteAsync();
                        IsWindowOpen = false;
                        return;
                    }
                    pow.IsHitTestVisible = true;
                    IsBusy = false;
                });
            };
            EventHandler close_handler = null;
            close_handler = (s, e) => {
                pow.Activated -= activate_handler;
                pow.Closed -= close_handler;
                pow.Closing -= closing_handler;
                IsFinalClosingState = false;
                WasCloseAppendWindowConfirmed = false;

                if (_contentView is MpAvContentWebView wv) {
                    // NOTE for some reason even canceling closing webview tries to dispose
                    // so it is ignored from final closing state. here is the only place it'll dispose
                    wv.FinishDisposal();
                }
                _contentView = null;
            };
            #endregion

            pow.Activated += activate_handler;
            pow.Closed += close_handler;
            pow.Opened += open_handler;
            pow.Closing += closing_handler;

            _contentView = null;
            return pow;
        }
        private async Task DisableReadOnlyInPlainTextHandlerAsync() {
            Dispatcher.UIThread.VerifyAccess();

            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalContentFormatDegradation,
                                    title: "Data Degradation Warning",
                                    body: $"Editing in comptability mode will remove all rich formatting. Are you sure you wish to modify this?");

            if (result == MpNotificationDialogResultType.Cancel) {
                return;
            }

            _isContentReadOnly = false;
            OnPropertyChanged(nameof(IsContentReadOnly));
        }

        private void RestorePersistentState() {
            if (MpAvPersistentClipTilePropertiesHelper.TryGetUniqueWidth_ById(CopyItemId, QueryOffsetIdx, out double uw) &&
                Math.Abs(uw - BoundWidth) >= 1 && IsResizerEnabled) {
                BoundWidth = uw;
            } else {
                MpAvPersistentClipTilePropertiesHelper.RemoveUniqueWidth_ById(CopyItemId, QueryOffsetIdx);
                BoundWidth = MinWidth;
            }

            if (MpAvPersistentClipTilePropertiesHelper.TryGetUniqueHeight_ById(CopyItemId, QueryOffsetIdx, out double uh) &&
                Math.Abs(uh - BoundHeight) >= 1 && IsResizerEnabled) {
                BoundHeight = uh;
            } else {
                MpAvPersistentClipTilePropertiesHelper.RemoveUniqueHeight_ById(CopyItemId, QueryOffsetIdx);
                BoundHeight = MinHeight;
            }

            IsTitleReadOnly = !MpAvPersistentClipTilePropertiesHelper.IsPersistentTileTitleEditable_ById(CopyItemId, QueryOffsetIdx);

            if (MpAvPersistentClipTilePropertiesHelper.IsPersistentTileContentEditable_ById(CopyItemId, QueryOffsetIdx) &&
                IsResizerEnabled) {
                IsContentReadOnly = false;
            } else {
                MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsContentEditableTile_ById(CopyItemId, QueryOffsetIdx);
                IsContentReadOnly = true;
            }

            IsSubSelectionEnabled = MpAvPersistentClipTilePropertiesHelper.IsPersistentIsSubSelectable_ById(CopyItemId, QueryOffsetIdx);
        }
        #endregion

        #region Commands

        public ICommand TileDragBeginCommand => new MpCommand(
            () => {
                //MpAvDragDropManager.StartDragCheck(this);
            });

        public ICommand DoubleLeftClickHandlerCommand => new MpCommand(
            () => {
                if (IsPinPlaceholder) {
                    Parent.UnpinTileCommand.Execute(this);
                    return;
                }
                EnableSubSelectionCommand.Execute(null);
            }, () => {
                if (IsPinPlaceholder) {
                    return true;
                }
                return EnableSubSelectionCommand.CanExecute(null);
            });

        public ICommand EnableSubSelectionCommand => new MpCommand(
            () => {
                IsSubSelectionEnabled = true;
            }, () => {
                return !IsSubSelectionEnabled && !IsFrozen;
            });
        public ICommand SendSubSelectedToEmailCommand => new MpCommand(
            () => {
                //MpHelpers.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, CopyItemTitle, CopyItemData.ToPlainText()));
            });

        public ICommand ResetTileSizeToDefaultCommand => new MpCommand<object>(
            (args) => {
                bool do_width =
                    MpAvPersistentClipTilePropertiesHelper.IsTileHaveUniqueWidth(CopyItemId, QueryOffsetIdx) &&
                    (args == null || args.ToString() == "width");

                bool do_height =
                    MpAvPersistentClipTilePropertiesHelper.IsTileHaveUniqueHeight(CopyItemId, QueryOffsetIdx) &&
                    (args == null || args.ToString() == "height");

                if (!do_width && !do_height) {
                    return;
                }
                IsResizing = true;

                if (do_width) {
                    MpAvPersistentClipTilePropertiesHelper.RemoveUniqueWidth_ById(CopyItemId, QueryOffsetIdx);
                    BoundWidth = MinWidth;
                }
                if (do_height) {
                    MpAvPersistentClipTilePropertiesHelper.RemoveUniqueHeight_ById(CopyItemId, QueryOffsetIdx);
                    BoundHeight = MinHeight;
                }

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
                //IsBusy = true;
                var ds = GetContentView() as MpAvIContentDragSource;
                if (ds == null) {
                    MpDebug.Break();
                    return;
                }
                var mpdo = await ds.GetDataObjectAsync();
                if (mpdo == null) {
                    // is none selected?
                    MpDebug.Break();
                    IsBusy = false;
                    return;
                }
                await Mp.Services.DataObjectTools.WriteToClipboardAsync(mpdo, true, null);

                // wait extra for cb watcher to know about data
                //await Task.Delay(300);
                //IsBusy = false;
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

                IsSelected = true;
                MpAvMenuExtension.ShowMenu(control, ContextMenuViewModel);
            }, (args) => {
                return CanShowContextMenu && !IsPinPlaceholder;
            });

        public MpIAsyncCommand<object> PersistContentStateCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is bool make_editable && make_editable) {
                    // query tile edit in grid mode so popout tile init picks up editable state
                    MpAvPersistentClipTilePropertiesHelper.AddPersistentIsContentEditableTile_ById(CopyItemId, QueryOffsetIdx);
                }

                if (GetContentView() is not MpAvContentWebView wv) {
                    return;
                }
                // store cur sel state
                var sel_state = await wv.GetSelectionStateAsync();
                MpAvPersistentClipTilePropertiesHelper.AddPersistentSubSelectionState(CopyItemId, QueryOffsetIdx, sel_state);
            }, (args) => {
                return !IsAnyPlaceholder;
            });

        public MpIAsyncCommand PinToPopoutWindowCommand => new MpAsyncCommand(
            async () => {
                if (!IsSelected) {
                    IsSelected = true;
                }
                if (Mp.Services.PlatformInfo.IsDesktop) {
                    await Parent.PinTileCommand.ExecuteAsync(new object[] { this, MpPinType.Window });
                } else {
                    // Some kinda view nav here
                    // see https://github.com/AvaloniaUI/Avalonia/discussions/9818

                }
            }, () => {
                return !IsWindowOpen && Parent != null;
            });

        public ICommand DisableSubSelectionCommand => new MpCommand(
            () => {
                // rejected for append tile
                IsSubSelectionEnabled = false;
            },
            () => {
                return CanDisableSubSelection;
            });

        public ICommand EnableContentReadOnlyCommand => new MpCommand(
            () => {
                IsContentReadOnly = true;
            },
            () => {
                return CanEdit && !IsContentReadOnly;
            });

        public ICommand DisableContentReadOnlyCommand => new MpCommand(
            () => {
                IsContentReadOnly = false;
            },
            () => {
                return CanEdit && IsContentReadOnly;
            });

        public ICommand ToggleIsContentReadOnlyCommand => new MpAsyncCommand(
            async () => {
                if (IsResizerEnabled || !IsContentReadOnly || IsPinned) {
                    IsContentReadOnly = !IsContentReadOnly;
                    return;
                }
                // when disabling read-only from query tray in grid
                // mode pop tile out since tile isn't resizable
                int ciid = CopyItemId;
                await Parent.PinTileCommand.ExecuteAsync(new object[] { this, MpPinType.Window, true });
            },
            () => {
                if (IsContentReadOnly) {
                    return DisableContentReadOnlyCommand.CanExecute(null);
                }
                return EnableContentReadOnlyCommand.CanExecute(null);
            });

        public ICommand ToggleEditContentCommand => new MpCommand(
            () => {
                if (!IsSelected && IsContentReadOnly) {
                    IsSelected = true;
                }
                IsContentReadOnly = !IsContentReadOnly;

            }, () => IsTextItem);

        public MpIAsyncCommand<object> ShareCommand => new MpAsyncCommand<object>(
            async (args) => {
                string pt = CopyItemData.ToPlainText("html");
                await Mp.Services.ShareTools.ShareTextAsync(CopyItemTitle, pt);
            });

        #endregion
    }
}
