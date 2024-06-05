using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipTileViewModel : MpAvViewModelBase<MpAvClipTrayViewModel>,
        MpIConditionalSelectableViewModel,
        MpICloseWindowViewModel,
        MpIWindowStateViewModel,
        MpIZoomFactorViewModel,
        MpIDraggable,
        MpIAnimatable,
        MpILocatorItem,
        MpIIconResourceViewModel,
        MpIAsyncCollectionObject,
        MpIHighlightTextRangesInfoViewModel,
        MpIDisposableObject,
        MpIShortcutCommandViewModel,
        MpIWantsTopmostWindowViewModel,
        MpIScrollIntoView,
        MpIUserColorViewModel,
        MpIColorPalettePickerViewModel,
        MpIHoverableViewModel,
        MpIResizableViewModel,
        MpITextContentViewModel,
        MpIContextMenuViewModel,
        MpAvIHeaderMenuViewModel{

        #region Private Variables

        private string _originalTitle;

        #endregion

        #region Constants

        public const int AUTO_CYCLE_DETAIL_DELAY_MS = 5000;
        public const string TABLE_WRAPPER_CLASS_NAME = "quill-better-table-wrapper";

        public const double EDITOR_TOOLBAR_MIN_WIDTH = 780.0d;
        public const double PASTE_APPEND_TOOLBAR_MIN_WIDTH = 290.0d;
        public const double PASTE_TEMPLATE_TOOLBAR_MIN_WIDTH = 850.0d;

        public const double PASTE_BUTTON_WIDTH = 93 + 32; // button + expander
        public const double APPEND_UNEXPANDED_WIDTH = 20;
        public const double APPEND_EXPANDED_WIDTH = 146;
        public const double MAX_TEMPLATE_TOOLBAR_WIDTH = 770;

        #endregion

        #region Statics
        public static ObservableCollection<string> EditorToolbarIcons => new ObservableCollection<string>() {

        };
        #endregion

        #region Interfaces

        #region MpILoadableViewModel Implementation
        public override bool IsLoadable => 
            true;
        #endregion

        #region MpAvIHeaderMenuViewModel Implementation
        IBrush MpAvIHeaderMenuViewModel.HeaderBackground =>
           DisplayColor.ToAvBrush(force_alpha: 1);
        IBrush MpAvIHeaderMenuViewModel.HeaderForeground =>
            (this as MpAvIHeaderMenuViewModel).HeaderBackground.ToHex().ToContrastForegoundColor().ToAvBrush();

        string MpAvIHeaderMenuViewModel.HeaderTitle =>
            CopyItemTitle;
        public IEnumerable<MpAvIMenuItemViewModel> HeaderMenuItems =>
            [
                new MpAvMenuItemViewModel() {
                    IconSourceObj = IsPinned ? "PinnedImage": "PinImage",
                    IconTintHexStr = IsPinned ? MpSystemColors.limegreen : null,
                    Command = MpAvClipTrayViewModel.Instance.PinTileCommand,
                    CommandParameter = this
                },
            new MpAvMenuItemViewModel() {
                    IconSourceObj = "EditImage",
                    Command = ToggleIsContentReadOnlyCommand,
                    IsVisible = !IsWindowOpen
                },
                new MpAvMenuItemViewModel() {
                    IconSourceObj = "Dots3x1Image",
                    Command = ShowContextMenuCommand
                }
            ];
        ICommand MpAvIHeaderMenuViewModel.BackCommand =>
            null;
        object MpAvIHeaderMenuViewModel.BackCommandParameter =>
            null;

        #endregion

        #region MpIZoomFactorViewModel Implementation
        public double MinZoomFactor =>
            MpCopyItem.ZOOM_FACTOR_MIN;
        public double MaxZoomFactor =>
            MpCopyItem.ZOOM_FACTOR_MAX;
        public double DefaultZoomFactor =>
            MpCopyItem.ZOOM_FACTOR_DEFAULT;
        public double StepDelta =>
            MpCopyItem.ZOOM_FACTOR_STEP;

        #endregion

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
        public ObservableCollection<MpTextRange> HighlightRanges { get; set; } = [];
        public int ActiveHighlightIdx { get; set; } = -1;


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
                MpAvMenuView.CloseMenu();
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

        bool CanFocus {
            get {
                if (MpAvFocusManager.Instance.IsTextInputControlFocused) {
                    return false;
                }
                return !IsFocusWithin && !MpAvSearchBoxViewModel.Instance.IsAnySearchControlFocused;
            }
        }

        public bool CanScrollX { get; set; }
        public bool CanScrollY { get; set; }
        public bool CanSelect =>
            !IsPinPlaceholder;

        private bool _isSelected;
        public bool IsSelected {
            get => _isSelected;
            set {
                if (_isSelected != value) {
                    // NOTE always triggering prop change when selecting
                    // to update LastSelectedDateTime to ensure
                    // tray's selected item is this one
                    _isSelected = value;
                }
                if (IsSelected && !CanSelect) {
                    MpDebug.Break("PinPlaceholder error, shouldn't be selectable", silent: true);
                }
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public bool CanDrag {
            get {
                if(MpAvThemeViewModel.Instance.IsMultiWindow) {
                    return true;
                }
                return IsSelected && IsSubSelectionEnabled;
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

        public MpAvClipTileViewModel NextNonPlaceholder {
            get {
                var next_nn = Next;
                while (true) {
                    if (next_nn == null) {
                        break;
                    }
                    if (next_nn.IsPinPlaceholder) {
                        next_nn = next_nn.Next;
                        continue;
                    }
                    return next_nn;
                }
                return null;
            }
        }
        public MpAvClipTileViewModel PrevNonPlaceholder {
            get {
                var next_nn = Next;
                while (true) {
                    if (next_nn == null) {
                        break;
                    }
                    if (next_nn.IsPinPlaceholder) {
                        next_nn = next_nn.Next;
                        continue;
                    }
                    return next_nn;
                }
                return null;
            }
        }

        public MpAvClipTileViewModel NearestNonPlaceholderNeighbor =>
            NextNonPlaceholder == null ? PrevNonPlaceholder : NextNonPlaceholder;

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
                if (IsPlaceholder) {
                    return string.Empty;
                }
                string detailText = string.Empty;
                switch ((MpCopyItemDetailType)SelectedDetailIdx) {
                    //created
                    case MpCopyItemDetailType.DateTimeCreated:
                        DetailTooltipText = MpAvDateTimeToStringConverter.Instance.Convert(CopyDateTime, null, null, null) as string;
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
                                if (CopyItemSize1 == 0 && CopyItemSize2 == 0 && FileItemCollectionViewModel != null) {
                                    FileItemCollectionViewModel.SetDetailInfo();
                                }
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

        public string WindowTitle {
            get {
                string wt = CopyItemTitle.ToWindowTitleText();
                if (IsSelected) {
                    wt = $"[{UiStrings.CommonSelectedText}] {wt}";
                }
                return wt;
            }
        }

        public string DisplayColor =>
            CopyItemHexColor.IsNullOrEmpty() ?
                TitleLayerHexColors.FirstOrDefault().AdjustAlpha(1) :
                CopyItemHexColor;

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
                if(MpAvThemeViewModel.Instance.IsMobileOrWindowed && IsWindowOpen) {
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

        public double DesiredWidth {
            get {

                if (Parent == null) {
                    return 0;
                }
                if (Parent.LayoutType == MpClipTrayLayoutType.Grid && !IsWindowOpen && !IsPinned) {
                    return BoundWidth;
                }
                if (!IsSubSelectionEnabled) {// || !IsWindowOpen) {
                    return IsPinned ? Parent.DefaultPinItemWidth : Parent.DefaultQueryItemWidth;
                }
                double dw = PASTE_BUTTON_WIDTH;
                if (IsAppendNotifier) {
                    dw += APPEND_EXPANDED_WIDTH;
                } else {
                    dw += APPEND_UNEXPANDED_WIDTH;
                }
                if (HasTemplates) {
                    dw += MAX_TEMPLATE_TOOLBAR_WIDTH;
                }
                if (IsContentReadOnly) {
                    return dw;
                }
                dw = Math.Max(dw, EDITOR_TOOLBAR_MIN_WIDTH);
                if (IsWindowOpen) {
                    if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                        return Mp.Services.ScreenInfoCollection.Primary.WorkingArea.Width;
                    }
                    return dw;
                }
                if (IsPinned) {
                    return Math.Min(dw, Parent.ObservedPinTrayScreenWidth - Parent.ScrollBarFixedAxisSize);
                }
                return Math.Min(dw, Parent.ObservedQueryTrayScreenWidth - Parent.ScrollBarFixedAxisSize);
            }
        }
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
                if (Parent.LayoutType == MpClipTrayLayoutType.Grid && !IsWindowOpen) {
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

        public bool IsSugarWv =>
#if SUGAR_WV
            true;
#else
            false;
#endif
        public MpCopyItemType LastCopyItemType { get; private set; }

        private string _searchableText = string.Empty;
        public string SearchableText { get; set; } = string.Empty;
        public bool IsAnimating { get; set; }
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

        public bool WasInternallyPinned { get; set; }

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
            MpAvAccountTools.Instance.LastCapInfo.ToBeTrashed_ciid == CopyItemId;

        public bool IsNextRemovedByAccount =>
            Parent != null &&
            CopyItemId != 0 &&
            MpAvAccountTools.Instance.LastCapInfo.ToBeRemoved_ciid == CopyItemId;

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
                string.Format(UiStrings.ClipShortcutUnassignedTooltip, CopyItemTitle) :
                UiStrings.ClipShortcutTooltip;


        public bool IsResizerEnabled =>
            MpAvThemeViewModel.Instance.IsMultiWindow &&
            !IsWindowOpen &&
            !IsFrozen &&
            (IsPinned || (Parent != null && Parent.IsQueryItemResizeEnabled));

        public bool IsEditPopOutOnly =>
#if SUGAR_WV || CEFNET_WV
            true;
#else
            Parent != null && Parent.LayoutType == MpClipTrayLayoutType.Grid;
#endif

        private int SelectedDetailIdx { get; set; } = 0;

        public bool IsOverDetailGrid { get; set; }
        public bool IsHovering { get; set; }
        public bool IsPasteBarHovering { get; set; }
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
                    MpAvWindowManager.GetTopLevel(c) is MpAvWindow w) {
                    return w.IsActive;
                }
                return false;
            }
        }

        public WindowState WindowState { get; set; }

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
        #endregion

        #region Appearance


        #endregion

        #endregion
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

        public bool IsPinOpTile {
            get {
                if (Parent == null) {
                    return false;
                }
                if (IsPinPlaceholder) {
                    return Parent.PinOpCopyItemId == PinPlaceholderCopyItemId;
                }
                return Parent.PinOpCopyItemId == CopyItemId;
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
                return !IsAppendNotifier && !IsPinOpTile;
            }
        }

        public bool IsSubSelectionEnabled { get; set; } = false;


        private int _queryOffsetIdx = -1;
        public int QueryOffsetIdx =>
            _queryOffsetIdx;

        public bool IsHorizontalScrollbarVisibile { get; set; }

        public bool IsVerticalScrollbarVisibile { get; set; }

        public bool IsAnyScrollbarVisible =>
            // NOTE keeping CanScrollX/Y separate since they are bound to rowv
            CanScrollX ||
            CanScrollY ||
            IsHorizontalScrollbarVisibile ||
            IsVerticalScrollbarVisibile;

        public bool IsAnyDropDownOpen { get; set; }

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
                    (!IsPinned &&
                        (MpAvTagTrayViewModel.Instance.TrashTagViewModel != null &&
                         MpAvTagTrayViewModel.Instance.TrashTagViewModel.IsSelected)) ||
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
#if MOBILE_OR_WINDOWED
                return IsSelected;
#else
                if (IsFrozen) {
                    return false;
                }                
                if (IsWindowOpen || IsSubSelectionEnabled || IsSelected || (IsHovering && !Parent.IsAnyDropOverTrays)) {
                    return true;
                }
                return false;
#endif
            }
        }

        public bool IsContentAndTitleReadOnly => IsContentReadOnly && IsTitleReadOnly;

        public bool IsContextMenuOpen { get; set; } = false;

        public DateTime TileCreatedDateTime { get; set; }
        #endregion

        #region Model

        public double ZoomFactor {
            get {
                if (CopyItem == null) {
                    return MpCopyItem.ZOOM_FACTOR_DEFAULT;
                }
                return CopyItem.ZoomFactor;
            }
            set {
                if (ZoomFactor != value && !IsPlaceholder) {
                    CopyItem.ZoomFactor = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ZoomFactor));
                }
            }
        }
        public int CopyCount {
            get {
                if (IsAnyPlaceholder) {
                    return 0;
                }
                return CopyItem.CopyCount;
            }
            set {
                if (CopyCount != value) {
                    //CopyItem.CopyCount = paramValue;
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
                    //CopyItem.PasteCount = paramValue;
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
                        var fl_frag = CopyItem.ToFileListDataFragmentMessage();
                        var itemData = fl_frag.SerializeObjectToBase64();
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
            set {
                if(CopyItem != null && CopyItemIconId != value) {
                    CopyItem.IconId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CopyItemIconId));
                    OnPropertyChanged(nameof(IconResourceObj));
                }
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
                if (CopyItem != null && CopyItemSize1 != value && value >= 0) {
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
                if (CopyItem != null && CopyItemSize2 != value && value >= 0) {
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
                    NotifyModelChanged(CopyItem, nameof(CopyItem.ItemData), value);
                    OnPropertyChanged(nameof(CopyItemData));
                }
            }
        }

        public object IconResourceObj {
            get {
                if (CopyItemType == MpCopyItemType.FileList &&
                    FileItemCollectionViewModel != null) {
                    return FileItemCollectionViewModel.PrimaryIconSourceObj;
                }
                if (CopyItemIconId == 0) {
                    if(CopyItemId > 0) {

                    }
                    return MpBase64Images.QuestionMark;
                }
                return CopyItemIconId;
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
                    //CopyItem.ItemColor = paramValue;
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
            //IsBusy = true;
        }

        public MpAvClipTileViewModel(MpAvClipTrayViewModel parent) : base(parent) {
            TileCreatedDateTime = DateTime.Now;
            PropertyChanged += MpAvClipTileViewModel_PropertyChanged;
            FileItemCollectionViewModel = new MpAvFileItemCollectionViewModel(this);
            //IsBusy = true;
        }

        #endregion

        #region Public Methods
        public async Task InitializeAsync(
            MpCopyItem ci,
            int queryOffset = -1,
            bool isRestoringSelection = false) {
            //IsBusy = true;
            await Task.Delay(1);


            bool is_query_item = queryOffset >= 0;
            bool is_pinned_item = !is_query_item && ci != null && ci.Id > 0;
            bool is_reload =
                (CopyItemId == 0 && ci == null) ||
                (ci != null && CopyItemId == ci.Id) ||
                (ci != null && PinPlaceholderCopyItemId == ci.Id && queryOffset >= 0);

            SearchableText = null;
            _contentView = null;
            if (!is_reload) {
                IsWindowOpen = false;
                if (CopyItemType != MpCopyItemType.None) {
                    LastCopyItemType = CopyItemType;
                }
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

                if (isRestoringSelection && Parent != null) {
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
            OnPropertyChanged(nameof(ZoomFactor));

            bool trigger_self_ref_change =
                //#if SUGAR_WV
                //                    CopyItemType != LastCopyItemType && CopyItemType != MpCopyItemType.None;
                //#else
                !MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled ||
                SelfRef == null;
            //#endif
            if (trigger_self_ref_change) {
                ResetDataTemplate();
            }
#if SUGAR_WV
            if (CopyItemType == MpCopyItemType.Text) {
                SearchableText = CopyItemData.ToPlainText("html");
            } else if (CopyItemType == MpCopyItemType.FileList) {
                SearchableText = CopyItemData;
            }
#endif
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
                char let = PublicHandle.ToUpperInvariant()[i];
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
                Parent.ShiftQuery(QueryOffsetIdx, -1);
            }
            PinPlaceholderCopyItemId = unloadAsPlaceholder ? CopyItemId : 0;
            CopyItem = null;
            UpdateQueryOffset(-1);
            OnPropertyChanged(nameof(IsPlaceholder));
            OnPropertyChanged(nameof(CopyItemId));
            OnPropertyChanged(nameof(QueryOffsetIdx));
            OnPropertyChanged(nameof(IsPinPlaceholder));
        }

        public void OpenPopOutWindow(MpAppendModeType amt, MpAvClipTileView cached_view) {
            IsAppendNotifier = amt != MpAppendModeType.None;

            if (!IsWindowOpen || (IsWindowOpen && IsAppendNotifier)) {
                // create popout or use existing if changing to append
                MpAvPersistentClipTilePropertiesHelper.RemoveUniqueSize_ById(CopyItemId, QueryOffsetIdx);

                var pow = MpAvWindowManager.LocateWindow(this, scanDescendants: false) ?? CreatePopoutWindow(cached_view);
                var ws = amt == MpAppendModeType.None ? new Size(500,500) : new Size(350, 250);
                pow.Width = ws.Width;
                pow.Height = ws.Height;
                pow.InvalidateMeasure();
                if (amt == MpAppendModeType.None) {
                    pow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                } else {
                    pow.WindowStartupLocation = WindowStartupLocation.Manual;
                    pow.Classes.Add("toast");
                    pow.Position = MpAvWindowPositioner.GetSystemTrayWindowPosition(pow);
                }
                pow.Show();
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
                MpPortableDataFormats.Files,
                MpPortableDataFormats.Xhtml,
                MpPortableDataFormats.Html,
            };
            if (isDnd && MpAvExternalDropWindowViewModel.Instance.IsDropWidgetEnabled) {
                // initialize target with all possible formats set to null
                req_formats = MpDataFormatRegistrar.RegisteredFormats.ToList();

            } else if (MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                // initialize target with default format for type
                switch (itemTypem) {
                    case MpCopyItemType.Text:
                        break;
                    case MpCopyItemType.Image:
                        req_formats.Add(MpPortableDataFormats.Image);
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
                    !IsPinPlaceholder &&
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

        public async Task ReloadAsync() {
            if (GetContentView() is not MpAvContentWebView cv) {
                return;
            }

            await cv.ReloadAsync();
            await Task.Delay(100);
            await ReloadModelAsync();
            await Task.Delay(100);
            await cv.LoadEditorAsync();
            await cv.LoadContentAsync();
        }

        public async Task ReloadModelAsync() {
            while (HasModelChanged) {
                await Task.Delay(100);
            }
            var ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(CopyItemId);
            await InitializeAsync(ci, QueryOffsetIdx);
        }
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
            //PropertyChanged -= MpAvClipTileViewModel_PropertyChanged;
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
        private void MpAvClipTileViewModel_PropertyChanged(object s, System.ComponentModel.PropertyChangedEventArgs e1) {
            switch (e1.PropertyName) {
                case nameof(IsAnimating):
                    if(!IsAnimating && IsWindowOpen) {
                        FinishChildWindowOpen();
                    }
                    break;
                case nameof(IsAppendNotifier):
                    if (IsAppendNotifier) {
                        IsSubSelectionEnabled = true;
                    } 
                    break;
                case nameof(IsAnyBusy):
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    }
                    break;
                case nameof(IsEditorLoaded):
                    // true = recv'd notifyLoadComplete
                    // false = PublicHandle changed

                    OnPropertyChanged(nameof(IsAnyBusy));
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
                        if (!IsPinOpTile) {
                            Parent.ScrollIntoView(this);
                        }

                        if (!Parent.IsRestoringSelection) {
                            Parent.StoreSelectionState(this);
                        }
                        if (CanFocus) {
                            // only focus tile if search isn't focused cause search as you type will take focus from search box
                            FocusContainerAsync(NavigationMethod.Pointer).FireAndForgetSafeAsync();
                        }
                    } else {
                        LastDeselectedDateTime = DateTime.Now;
                        if (!IsWindowOpen &&
                            IsContentReadOnly &&
                            IsSubSelectionEnabled &&
                            !IsPinOpTile) {
                            DisableSubSelectionCommand.Execute(null);
                        }
                    }
                    MpAvMainWindowViewModel.Instance.SetHeaderMenu(IsSelected ? this : null);
                    OnPropertyChanged(nameof(IsCornerButtonsVisible));
                    Parent.NotifySelectionChanged();
                    break;
                case nameof(CopyItem):
                    OnPropertyChanged(nameof(IsPlaceholder));
                    OnPropertyChanged(nameof(CopyItemId));
                    break;
                case nameof(PinPlaceholderCopyItemId):
                    OnPropertyChanged(nameof(IsPlaceholder));
                    break;
                case nameof(IsPinned):
                    OnPropertyChanged(nameof(HeaderMenuItems));
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

                        //show most recent img transactions
                        TransactionCollectionViewModel.ShowMostRecentRuntimeTransactionCommand.Execute(null);
                    } else {
                        MpAvMainWindowViewModel.Instance.LastDecreasedFocusLevelDateTime = DateTime.Now;
                        MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsSubSelectableTile_ById(CopyItemId, QueryOffsetIdx);

                        //show most recent img transactions
                        TransactionCollectionViewModel.HideTransactionsCommand.Execute(null);
                    }
                    OnPropertyChanged(nameof(MinWidth));

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
                        if (!IsSubSelectionEnabled) {
                            IsSubSelectionEnabled = true;
                        }
                        MpAvPersistentClipTilePropertiesHelper.AddPersistentIsContentEditableTile_ById(CopyItemId, QueryOffsetIdx);
                        IsTitleReadOnly = true;
                        OnPropertyChanged(nameof(IsTitleVisible));
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsQueryHorizontalScrollBarVisible));
                    Parent.OnPropertyChanged(nameof(Parent.IsQueryVerticalScrollBarVisible));

                    //IsSubSelectionEnabled = !IsContentReadOnly;
                    OnPropertyChanged(nameof(IsSubSelectionEnabled));
                    OnPropertyChanged(nameof(MinWidth));

                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTile));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingClipTile));
                    break;
                case nameof(IsHorizontalScrollbarVisibile):
                case nameof(IsVerticalScrollbarVisibile):
                    break;
                case nameof(IsContextMenuOpen):
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyTileContextMenuOpened));
                    break;
                case nameof(IsPasting):
                    Parent.OnPropertyChanged(nameof(Parent.IsPasting));
                    break;
                case nameof(HasTemplates):
                    OnPropertyChanged(nameof(MinWidth));
                    break;
                case nameof(IsDropOverTile):
                    //MpConsole.WriteLine($"Drop Over: {IsDropOverTile} '{this}'");

#if !SUGAR_WV
                    if (IsDropOverTile && !IsSubSelectionEnabled) {
                        IsSubSelectionEnabled = true;
                    }
#endif

                    if (IsDropOverTile) {
                        Parent.NotifyDragOverTrays(true);
                        Parent.ScrollIntoView(this);
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
                        if (IsContentReadOnly && CopyItemData.IsNullOrWhitespaceHtmlString()) {
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
                            if (CopyItem != null) {

                                await CopyItem.WriteToDatabaseAsync(IsContentChangeModelChange, IsContentChangeModelChange ? SearchableText : null);
                            }
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
#if SUGAR_WV
                    if (!IsWindowOpen) {
                        ResetDataTemplate();
                    }
#endif
                    OnPropertyChanged(nameof(IsCornerButtonsVisible));
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
                case nameof(IsPlaceholder):

                    break;
                case nameof(IsTileDragging):
                    Parent.OnPropertyChanged(nameof(Parent.CanTouchScroll));

                    break;
                case nameof(IsFrozen):
                    OnPropertyChanged(nameof(IsResizerEnabled));
                    break;
                case nameof(IsTrashed):
                    if (IsTrashed) {
                        IsContentReadOnly = true;
                        IsSubSelectionEnabled = false;
                    }
                    break;
                case nameof(ZoomFactor):
                    MpMessenger.SendGlobal(MpMessageType.ContentZoomFactorChanged);
                    break;
            }
        }

        #region Popout Window
        private MpAvWindow CreatePopoutWindow(MpAvClipTileView cached_view) {
            int orig_ciid = CopyItemId;

            var pow = new MpAvWindow() {
                DataContext = this,
                ShowInTaskbar = true,
                Background = 
                    MpAvThemeViewModel.Instance.IsMobileOrWindowed ? 
                        Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveBgColor) : 
                        Brushes.Transparent,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("AppIcon", typeof(MpAvWindowIcon), null, null) as MpAvWindowIcon,
                Content = cached_view ?? new MpAvClipTileView(),
                CornerRadius = 
                    MpAvThemeViewModel.Instance.IsMobileOrWindowed ?
                        new CornerRadius() :
                        Mp.Services.PlatformResource.GetResource<CornerRadius>("TileCornerRadius")
            };
            if (pow.Content is MpAvClipTileView ctv &&
                ctv.Content is MpAvClipBorder cb) {
                cb.CornerRadius = new CornerRadius(0);
                // ensure dc not mismatched
                ctv.DataContext = this;
            }
            pow.Classes.Add("content-window");
            //pow.Classes.Add("fadeIn");
            //pow.Classes.Add("fadeOut");

            #region Window Bindings

            pow.Bind(
                Control.MinWidthProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(MinWidth)
                });

            pow.Bind(
                MpAvWindow.TitleProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(WindowTitle)
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

            pow.Activated += activate_handler;
            pow.Loaded += loaded_handler;
            pow.Closing += closing_handler;
            pow.Closed += close_handler;

            _contentView = null;
            return pow;
        }


        public async Task<bool> FocusContainerAsync(NavigationMethod focusType) {
            if (GetContentView() is not Control c) {
                return false;
            }
            Control to_focus = null;
            if (c.GetVisualAncestor<ListBoxItem>() is ListBoxItem lbi) {
                to_focus = lbi;
            } else if (c.GetVisualAncestor<MpAvWindow>() is MpAvWindow w) {
                to_focus = w;
            }
            if (to_focus == null) {
                return false;
            }
            bool success = await to_focus.TrySetFocusAsync(focusType);
            if (success && !IsSelected) {
                IsSelected = true;
            }
            MpConsole.WriteLine($"Focusing '{this}' with method '{focusType}' {success.ToTestResultLabel()}");
            return success;
        }

        public async Task<bool> ClosePopoutAsync() {
            // called from unpin and window closed ha
            if(IsPinPlaceholder && PinnedItemForThisPlaceholder != null) {
                // shouldn't really happen but in case data contexts get mismatched somehow
                bool result = await PinnedItemForThisPlaceholder.ClosePopoutAsync();
                return result;
            }
            if (IsAppendNotifier) {
                // shouldn't need to check this since only append needs confirm
                // but if others do later good to check, all this is confusing
                await Parent.DeactivateAppendModeCommand.ExecuteAsync();
            }
            IsWindowOpen = false;

            await TransactionCollectionViewModel.CloseTransactionPaneCommand.ExecuteAsync();
            MpAvPersistentClipTilePropertiesHelper.RemoveUniqueSize_ById(CopyItemId, QueryOffsetIdx);

            return true;
        }

        #region Event Wrappers
        private void HandleThisPopoutInitialized(MpAvWindow pow, EventArgs e) {
            OnPropertyChanged(nameof(IsTitleVisible));
            if (pow.GetLogicalDescendants<MpAvContentWebView>().FirstOrDefault() is not { } wv) {
                return;
            }
        }

        private void HandleThisPopoutLoaded(MpAvWindow pow, EventArgs e) {
            if (pow.GetLogicalDescendants<MpAvContentWebView>().FirstOrDefault() is not { } wv) {
                return;
            }
            void EditorInitialized(object sender, EventArgs e2) {
                wv.EditorInitialized -= EditorInitialized;
                wv.LoadContentAsync().FireAndForgetSafeAsync(this);
            }
            void ContentLoaded(object sender, EventArgs e2) {
                wv.ContentLoaded -= ContentLoaded;
                wv.FocusEditor();
            }
            wv.EditorInitialized += EditorInitialized;
            wv.ContentLoaded += ContentLoaded;
        }
        private void HandleThisPopoutActivate(MpAvWindow pow, EventArgs e) {
            Parent.SelectClipTileCommand.Execute(CopyItemId);
        }

        private void HandleThisPopoutClosing(MpAvWindow pow, CancelEventArgs e) {

            if (e is WindowClosingEventArgs ce &&
                ce.IsProgrammatic) {
                MpConsole.WriteLine($"tile popout closing called. reason: '{ce.CloseReason}' programmatic: '{ce.IsProgrammatic}'");
                if (ce.IsProgrammatic) {
                    // state already stored
                    return;
                }
                e.Cancel = true;
            }

            Dispatcher.UIThread.Post(async () => {
                IsBusy = true;
                await MpAvClipTrayViewModel.Instance.UnpinTileCommand.ExecuteAsync(this);
                pow.Close();
                IsBusy = false;
            });
        }

        private void HandleThisPopoutClosed(MpAvWindow pow, EventArgs e) {
            // IsClosePopoutConfirmFinished = false;
            pow.Activated -= activate_handler;
            pow.Loaded -= loaded_handler;
            pow.Closing -= closing_handler;
            pow.Closed -= close_handler;

            if (GetContentView() is MpAvContentWebView wv) {
                // NOTE for some reason even canceling closing webview tries to dispose
                // so it is ignored from final closing state. here is the only place it'll dispose
                wv.FinishDisposal();
            }
            _contentView = null;
        }
        #endregion

        #region Event Handlers
        private void loaded_handler(object sender, EventArgs e) {
            if (sender is not MpAvWindow pow ||
                pow.DataContext is not MpAvClipTileViewModel popout_ctvm) {
                return;
            }
            popout_ctvm.HandleThisPopoutLoaded(pow, e);
        }
        private void activate_handler(object sender, EventArgs e) {
            if (sender is not MpAvWindow pow ||
                pow.DataContext is not MpAvClipTileViewModel popout_ctvm) {
                return;
            }
            popout_ctvm.HandleThisPopoutActivate(pow, e);
        }
        private void closing_handler(object sender, CancelEventArgs e) {
            if (sender is not MpAvWindow pow ||
                pow.DataContext is not MpAvClipTileViewModel popout_ctvm) {
                return;
            }
            popout_ctvm.HandleThisPopoutClosing(pow, e);
        }
        private void close_handler(object sender, EventArgs e) {
            if (sender is not MpAvWindow pow ||
                pow.DataContext is not MpAvClipTileViewModel popout_ctvm) {
                return;
            }
            popout_ctvm.HandleThisPopoutClosed(pow, e);
        }
        #endregion

        #endregion

        private async Task DisableReadOnlyInPlainTextHandlerAsync() {
            Dispatcher.UIThread.VerifyAccess();

            var result = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                    notificationType: MpNotificationType.ModalContentFormatDegradation,
                                    title: UiStrings.ClipTileDataDegradeNtfTitle,
                                    body: UiStrings.ClipTileDataDegradeNtfText);

            if (result == MpNotificationDialogResultType.Cancel) {
                return;
            }

            _isContentReadOnly = false;
            OnPropertyChanged(nameof(IsContentReadOnly));
        }
        private void FinishChildWindowOpen() {
           // HACK webview only animates halfway out for some reason, toggling alignment fixes it
            if(MpAvThemeViewModel.Instance.IsMultiWindow ||
                GetContentView() is not Control cwv) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                cwv.HorizontalAlignment = HorizontalAlignment.Left;
                await Task.Delay(250);
                cwv.HorizontalAlignment = HorizontalAlignment.Stretch;
            });
        }
        private void ResetDataTemplate() {
            // NOTE in compatibility mode content template must be reselected
            // and for overall efficiency re-setting datacontext is better than
            // locating this tile from the view side (changing contentControl.content to id)
            //OnPropertyChanged(nameof(ContentTemplateParam));
            SelfRef = null;
            SelfRef = this;
        }
        private void StorePersistentState() {
            if (BoundWidth != MinWidth) {
                MpAvPersistentClipTilePropertiesHelper.AddOrReplaceUniqueWidth_ById(CopyItemId, BoundWidth, QueryOffsetIdx);
            } else {
                MpAvPersistentClipTilePropertiesHelper.RemoveUniqueWidth_ById(CopyItemId, QueryOffsetIdx);
            }
            if (BoundHeight != MinHeight) {
                MpAvPersistentClipTilePropertiesHelper.AddOrReplaceUniqueHeight_ById(CopyItemId, BoundHeight, QueryOffsetIdx);
            } else {
                MpAvPersistentClipTilePropertiesHelper.RemoveUniqueHeight_ById(CopyItemId, QueryOffsetIdx);
            }
            if (IsTitleReadOnly) {
                MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsTitleEditableTile_ById(CopyItemId, QueryOffsetIdx);
            } else {
                MpAvPersistentClipTilePropertiesHelper.AddPersistentIsTitleEditableTile_ById(CopyItemId, QueryOffsetIdx);
            }
            if (IsContentReadOnly) {
                MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsContentEditableTile_ById(CopyItemId, QueryOffsetIdx);
            } else {
                MpAvPersistentClipTilePropertiesHelper.AddPersistentIsContentEditableTile_ById(CopyItemId, QueryOffsetIdx);
            }
            if (IsSubSelectionEnabled) {
                MpAvPersistentClipTilePropertiesHelper.AddPersistentIsSubSelectableTile_ById(CopyItemId, QueryOffsetIdx);
            } else {
                MpAvPersistentClipTilePropertiesHelper.RemovePersistentSubSelectionState(CopyItemId, QueryOffsetIdx);
            }
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
            if (IsAnyPlaceholder) {
                return;
            }
            IsTitleReadOnly = !MpAvPersistentClipTilePropertiesHelper.IsPersistentTileTitleEditable_ById(CopyItemId, QueryOffsetIdx);

            if (MpAvPersistentClipTilePropertiesHelper.IsPersistentTileContentEditable_ById(CopyItemId, QueryOffsetIdx)) {
                IsContentReadOnly = false;
            } else {
                MpAvPersistentClipTilePropertiesHelper.RemovePersistentIsContentEditableTile_ById(CopyItemId, QueryOffsetIdx);
                IsContentReadOnly = true;
            }

            IsSubSelectionEnabled =
                MpAvPersistentClipTilePropertiesHelper.IsPersistentIsSubSelectable_ById(CopyItemId, QueryOffsetIdx);
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
        public MpIAsyncCommand CopyToClipboardCommand => new MpAsyncCommand(
            async () => {
                if (IsTitleFocused) {
                    return;
                }
                //IsBusy = true;
                var ds = GetContentView() as MpAvIContentDragSource;
                if (ds == null) {
                    MpDebug.Break();
                    return;
                }
                var mpdo = await ds.GetDataObjectAsync(
                    formats: GetOleFormats(false));

                if (mpdo == null) {
                    // is none selected?
                    MpDebug.Break();
                    IsBusy = false;
                    return;
                }
                await Mp.Services.DataObjectTools.WriteToClipboardAsync(mpdo, true);

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
                MpAvMenuView.ShowMenu(
                    target: control,
                    dc: ContextMenuViewModel);
            }, (args) => {
                return
                    !IsTitleFocused &&
                    CanShowContextMenu &&
                    !IsPinPlaceholder;
            });
        public MpIAsyncCommand<object> PersistContentStateCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is string persist_state) {
                    if (persist_state == "editable") {
                        // query tile edit in grid mode so popout tile init picks up editable state
                        MpAvPersistentClipTilePropertiesHelper.AddPersistentIsContentEditableTile_ById(CopyItemId, QueryOffsetIdx);
                    } else if (persist_state == "subselectable") {
                        MpAvPersistentClipTilePropertiesHelper.AddPersistentIsSubSelectableTile_ById(CopyItemId, QueryOffsetIdx);
                    }
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
                //if (!IsSelected) {
                //    IsSelected = true;
                //}
                await Parent.PinTileCommand.ExecuteAsync(new object[] { this, MpPinType.Window });
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
                if (!IsContentReadOnly) {
                    IsContentReadOnly = true;
                    return;
                }
                if (!IsEditPopOutOnly) {
                    IsContentReadOnly = false;
                    return;
                }

                if (IsWindowOpen) {
                    IsContentReadOnly = false;
                    return;
                }
                await Parent.PinTileCommand.ExecuteAsync(new object[] { this, MpPinType.Window, true });
            },
            () => {
                if (IsContentReadOnly) {
                    return DisableContentReadOnlyCommand.CanExecute(null);
                }
                return EnableContentReadOnlyCommand.CanExecute(null);
            });
        public MpIAsyncCommand<object> ShareCommand => new MpAsyncCommand<object>(
            async (args) => {
                string pt = CopyItemData.ToPlainText("html");
                await Mp.Services.ShareTools.ShareTextAsync(CopyItemTitle, pt);
            });
        public ICommand StoreSelectionStateCommand => new MpCommand(
            () => {
                StorePersistentState();
            }, () => {
                return !IsAnyPlaceholder;
            });
        public ICommand RestoreSelectionStateCommand => new MpCommand(
            () => {
                RestorePersistentState();
            }, () => {
                return !IsAnyPlaceholder;
            });

        public ICommand ZoomInCommand => new MpCommand(
            () => {
                ZoomFactor = Math.Min(MaxZoomFactor, ZoomFactor + StepDelta);
            });
        public ICommand ZoomOutCommand => new MpCommand(
            () => {
                ZoomFactor = Math.Max(MinZoomFactor, ZoomFactor - StepDelta);
            });

        public ICommand ResetZoomCommand => new MpCommand(
            () => {
                ZoomFactor = DefaultZoomFactor;
            });
        public ICommand SetZoomCommand => new MpCommand<object>(
            (args) => {
                if (args is not double newZoomFactor) {
                    return;
                }
                ZoomFactor = Math.Clamp(newZoomFactor, MinZoomFactor, MaxZoomFactor);
            });
        public ICommand DragEnterCommand => new MpCommand(() => {
            if (IsTileDragging) {
                // don't flip view for self drop
                return;
            }
            IsDropOverTile = true;
            if (IsPinPlaceholder &&
                PinnedItemForThisPlaceholder != null &&
                PinnedItemForThisPlaceholder.IsWindowOpen &&
                PinnedItemForThisPlaceholder.WindowState == WindowState.Minimized) {
                // on drag enter for minimized popout pin placeholders, show pop out
                PinnedItemForThisPlaceholder.WindowState = WindowState.Normal;
            }
        });
        public ICommand DragLeaveCommand => new MpCommand(() => {
            //IsDropOverTile = false;
        });
        #endregion
    }
}
