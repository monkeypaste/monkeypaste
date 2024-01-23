using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipTrayViewModel :
        MpAvViewModelBase<MpAvClipTileViewModel>,
        MpIContentBuilder,
        MpIAsyncCollectionObject,
        MpIPagingScrollViewerViewModel,
        MpIActionComponent,
        MpIBoundSizeViewModel,
        MpIContextMenuViewModel,
        MpIContentQueryPage,
        MpIProgressIndicatorViewModel {
        #region Private Variables
        private int? _query_anchor_idx = null;

        private bool _isMainWindowOrientationChanging = false;
        private bool _isLayoutChanging = false;
        private object _addDataObjectContentLock = new object();
        private object _capMsgLock = new object();


        private MpAvCopyItemBuilder _copyItemBuilder = new MpAvCopyItemBuilder();
        #endregion

        #region Constants

        public const bool IS_ADD_STARTUP_CLIPBOARD_HIDDEN = true;

        public const int ADD_CONTENT_TIMEOUT_MS =
#if DEBUG
            30_000_000;
#else
            30_000;
#endif
        public const int DISABLE_READ_ONLY_DELAY_MS = 500;
        public const double MAX_TILE_SIZE_CONTAINER_PAD = 50;
        public const double MIN_SIZE_ZOOM_FACTOR_COEFF = (double)1 / (double)7;
        public const double DEFAULT_ITEM_SIZE = 260;
        public const double UNEXPANDED_HEIGHT_RATIO = 0.5d;
        public const double DEFAULT_UNEXPANDED_HEIGHT = DEFAULT_ITEM_SIZE * UNEXPANDED_HEIGHT_RATIO;

        #endregion

        #region Statics

        private static MpAvClipTrayViewModel _instance;
        public static MpAvClipTrayViewModel Instance => _instance ?? (_instance = new MpAvClipTrayViewModel());

        #endregion

        #region Interfaces

        #region MpIContentBuilder Implementation

        public async Task<MpCopyItem> BuildFromDataObjectAsync(object avOrPortableDataObject, bool is_copy, MpDataObjectSourceType sourceType = default) {
            IDataObject ido = avOrPortableDataObject as IDataObject;
            if (ido == null && avOrPortableDataObject.ToDataObject() is IDataObject cnvIdo) {
                ido = cnvIdo;
            }
            sourceType = sourceType == default ? ido.GetDataObjectSourceType() : sourceType;
            MpAvDataObject mpdo = await Mp.Services.DataObjectTools.ReadDataObjectAsync(ido, sourceType) as MpAvDataObject;

            if (mpdo == null) {
                return null;
            }
            mpdo.SetDataObjectSourceType(sourceType);

            if (mpdo.ContainsContentRef()) {
                // internal source, finalize title 
                bool is_partial_internal = mpdo.ContainsPartialContentRef();
                mpdo.FinalizeContentOleTitle(!is_partial_internal, is_copy);
            }
            //
            bool is_drop = false;
            bool remove_ext = false;
            switch (sourceType) {
                case MpDataObjectSourceType.FolderWatcher:
                    // remove ext if there 
                    mpdo.Remove(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT);
                    // add file explorer as source
                    mpdo.SetData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, new MpPortableProcessInfo(Mp.Services.PlatformInfo.OsFileManagerPath));
                    break;
                case MpDataObjectSourceType.PluginResponse:
                    // always remove external source
                    remove_ext = true;
                    break;
                case MpDataObjectSourceType.ActionDrop:
                case MpDataObjectSourceType.PinTrayDrop:
                case MpDataObjectSourceType.QueryTrayDrop:
                case MpDataObjectSourceType.TagDrop:
                case MpDataObjectSourceType.ClipTileDrop:
                    // remove ext if drag from internal
                    remove_ext = MpAvDoDragDropWrapper.IsAnyDragging;
                    is_drop = true;
                    break;
                case MpDataObjectSourceType.AppendEnabled:
                    // we don't want to remove here but is it going to be correct?
                    remove_ext = Mp.Services.ClipboardMonitor.IsStartupClipboard;
                    break;
            }

            if (remove_ext && mpdo.ContainsData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT)) {
                mpdo.Remove(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT);
            } else if (!remove_ext && !mpdo.ContainsData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT)) {
                MpPortableProcessInfo source_pi = is_drop ?
                    Mp.Services.DragProcessWatcher.DragProcess :
                    Mp.Services.ProcessWatcher.LastProcessInfo;

                if (Mp.Services.DragProcessWatcher.DragProcess == null) {
                    MpConsole.WriteLine($"Warning! no drag process found after '{sourceType}'");
                } else {
                    mpdo.SetData(MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT, source_pi.Clone());
                    MpConsole.WriteLine($"Source process '{source_pi}' ADDED for clip build type '{sourceType}'");
                }
            }

            MpCopyItem content = await AddItemFromDataObjectAsync(mpdo, is_copy);
            return content;
        }

        #endregion

        #region MpIProgressIndicatorViewModel Implementation

        public double PercentLoaded {
            get {
                int total_query_count = QueryItems.Count();
                if (total_query_count == 0) {
                    return 1;
                }
                int busy_query_count =
                    QueryItems.Where(x => x.IsAnyBusy).Count();

                return (total_query_count - busy_query_count) / total_query_count;
            }
        }
        #endregion

        #region MpIContentQueryTools Implementation

        IEnumerable<int> MpIContentQueryPage.GetOmittedContentIds() =>
            MpAvTagTrayViewModel.Instance.TrashTagViewModel == null ||
            MpAvTagTrayViewModel.Instance.TrashTagViewModel.IsSelected ?
                    new int[] { } :
                    MpAvTagTrayViewModel.Instance.TrashedCopyItemIds;

        int MpIContentQueryPage.Offset =>
            HeadQueryIdx;

        int MpIContentQueryPage.Limit =>
            TailQueryIdx;


        #endregion

        #region MpIContextMenuItemViewModel Implementation
        public MpAvMenuItemViewModel ContextMenuViewModel {
            get {
                if (SelectedItem == null) {
                    return new MpAvMenuItemViewModel();
                }
                if (SelectedItem.IsTrashed) {
                    return new MpAvMenuItemViewModel() {
                        SubItems = new List<MpAvMenuItemViewModel>() {
                            new MpAvMenuItemViewModel() {
                                Header = UiStrings.ClipTileTrashRestoreHeader,
                                IconResourceKey =
                                    MpAvAccountTools.Instance.IsContentAddPausedByAccount ?
                                        MpContentCapInfo.ADD_BLOCKED_RESOURCE_KEY :
                                        "ResetImage",
                                Command = RestoreSelectedClipCommand,
                            },
                            new MpAvMenuItemViewModel() {
                                HasLeadingSeparator = true,
                                Header = UiStrings.ClipTilePermanentlyDeleteHeader,
                                IconResourceKey = "TrashCanImage",
                                Command = DeleteSelectedClipCommand,
                                ShortcutArgs = new object[] { MpShortcutType.PermanentlyDelete },
                            },
                        }
                    };
                }

                return new MpAvMenuItemViewModel() {
                    SubItems = new List<MpAvIMenuItemViewModel>() {
#if DEBUG
                        new MpAvMenuItemViewModel() {
                            Header = @"Show Dev Tools",
                            Command = ShowDevToolsCommand,
                            IsVisible = MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled
                        },
#endif
                        new MpAvMenuItemViewModel() {
#if DEBUG
                            HasLeadingSeparator = true,
#endif
                            Header = UiStrings.CommonRefreshTooltip,
                            IconResourceKey = "ResetImage",
                            Command = ReloadSelectedItemCommand,
                            IsVisible = MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled
                        },
                        new MpAvMenuItemViewModel() {
                            HasLeadingSeparator = true,
                            Header = UiStrings.CommonCutOpLabel,
                            IconResourceKey = "ScissorsImage",
                            Command = CutSelectionFromContextMenuCommand,
                            IsVisible = false,
                            CommandParameter = true,
                            ShortcutArgs = new object[] { MpShortcutType.CutSelection },
                        },
                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.CommonCopyOpLabel,
                            IconResourceKey = "CopyImage",
                            Command = CopySelectionFromContextMenuCommand,
                            ShortcutArgs = new object[] { MpShortcutType.CopySelection },
                        },
                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.CommonDuplicateLabel,
                            //AltNavIdx = 0,
                            IconResourceKey = "DuplicateImage",
                            Command = DuplicateSelectedClipsCommand,
                            ShortcutArgs = new object[] { MpShortcutType.Duplicate },
                        },
                        new MpAvMenuItemViewModel() {
                            Header =UiStrings.ClipTilePasteHereHeaderLabel,
                            //AltNavIdx = 6,
                            IconResourceKey = "PasteImage",
                            Command = PasteHereFromContextMenuCommand,
                            IsVisible = false,
                            ShortcutArgs = new object[] { MpShortcutType.PasteSelection },
                        },
                        new MpAvMenuItemViewModel() {
                            IsVisible = CurPasteInfoMessage.infoId != null,
                            Header = CurPasteInfoMessage.pasteButtonTooltipText,
                            //AltNavIdx = 0,
                            IconSourceObj = CurPasteInfoMessage.pasteButtonIconBase64,
                            Command = PasteSelectedClipTileFromContextMenuCommand,
                            ShortcutArgs = new object[] { MpShortcutType.PasteToExternal },
                        },
                        new MpAvMenuItemViewModel() {
                            HasLeadingSeparator = true,
                            Header = UiStrings.CommonDeleteLabel,
                            //AltNavIdx = 0,
                            IconResourceKey = "TrashCanImage",
                            Command = TrashSelectedClipCommand,
                            ShortcutArgs = new object[] { MpShortcutType.DeleteSelectedItems },
                        },
                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.CommonRenameLabel,
                            IsVisible = MpAvPrefViewModel.Instance.ShowContentTitles,
                            IconResourceKey = "RenameImage",
                            Command = EditSelectedTitleCommand,
                            ShortcutArgs = new object[] { MpShortcutType.Rename },
                        },
                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.CommonEditLabel,
                            IsVisible = !IsAutoEditEnabled,
                            //AltNavIdx = 0,
                            IconResourceKey = "EditContentImage",
                            Command = EditSelectedContentCommand,
                            ShortcutArgs = new object[] { MpShortcutType.ToggleContentReadOnly },
                        },
                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.ClipTileFindReplaceHeader,
                            //AltNavIdx = 0,
                            IconResourceKey = "SearchImage",
                            Command = EnableFindAndReplaceForSelectedItem,
                            ShortcutArgs = new object[] { MpShortcutType.FindAndReplaceSelectedItem },
                        },
                        // share
                        new MpAvMenuItemViewModel() {
                            HasLeadingSeparator = true,
                            Header =UiStrings.ClipTileShareHeader,
                            IconResourceKey = "ShareImage",
                            Command = SelectedItem.ShareCommand
                        },
                        // sources
                        SelectedItem.TransactionCollectionViewModel.ContextMenuViewModel,
                        // analyzers
                        MpAvAnalyticItemCollectionViewModel.Instance.GetContentContextMenuItem(SelectedItem.CopyItemType),
                        // collections
                        MpAvTagTrayViewModel.Instance,
                        // colors                        
                        MpAvMenuItemViewModel.GetColorPalleteMenuItemViewModel(SelectedItem,true),
                    },
                };
            }
        }

        bool MpIContextMenuViewModel.IsContextMenuOpen {
            get {
                if (SelectedItem == null) {
                    return false;
                }
                if (SelectedItem.IsContextMenuOpen ||
                    SelectedItem.TransactionCollectionViewModel.IsContextMenuOpen) {
                    return true;
                }
                return false;
            }
            set {
                if (SelectedItem == null) {
                    return;
                }
                if (value) {
                    if (SelectedItem.IsHoveringOverSourceIcon) {
                        SelectedItem.TransactionCollectionViewModel.IsContextMenuOpen = true;
                        SelectedItem.IsContextMenuOpen = false;
                    } else {
                        SelectedItem.TransactionCollectionViewModel.IsContextMenuOpen = false;
                        SelectedItem.IsContextMenuOpen = true;
                    }
                } else {
                    SelectedItem.TransactionCollectionViewModel.IsContextMenuOpen = false;
                    SelectedItem.IsContextMenuOpen = false;
                }
            }
        }

        #endregion

        #region MpIBoundSizeViewModel Implementation
        // NOTE this are NOT bound in xaml, bound in mw.UpdateContentLayout
        public double ContainerBoundWidth { get; set; }
        public double ContainerBoundHeight { get; set; }

        double MpIBoundSizeViewModel.ContainerBoundWidth {
            get => ContainerBoundWidth;
            set => ContainerBoundWidth = value;
        }
        double MpIBoundSizeViewModel.ContainerBoundHeight {
            get => ContainerBoundHeight;
            set => ContainerBoundHeight = value;
        }
        #endregion

        #region MpIPagingScrollViewer Implementation
        public bool IsTouchScrolling { get; set; }
        public bool CanTouchScroll =>
            Mp.Services.PlatformInfo.IsTouchInputEnabled &&
            QueryItems.All(x => !x.IsTileDragging) &&
            QueryItems.All(x => !x.CanResize) &&
            QueryItems.All(x => !x.CanResize) &&
            (HoverItem == null ||
             HoverItem.IsPinned ||
             !HoverItem.IsSubSelectionEnabled);


        public Orientation ListOrientation =>
            MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ? Orientation.Horizontal : Orientation.Vertical;

        public bool IsVerticalOrientation =>
            ListOrientation == Orientation.Vertical;
        public bool IsQueryHorizontalScrollBarVisible =>
            QueryTrayTotalTileWidth > ObservedQueryTrayScreenWidth;
        public bool IsQueryVerticalScrollBarVisible =>
            QueryTrayTotalTileHeight > ObservedQueryTrayScreenHeight;


        public double LastScrollOffsetX { get; set; } = 0;
        public double LastScrollOffsetY { get; set; } = 0;

        public MpPoint LastScrollOffset => new MpPoint(LastScrollOffsetX, LastScrollOffsetY);

        private double _scrollOffsetX;
        public double ScrollOffsetX {
            get => _scrollOffsetX;
            set {
                if (ScrollOffsetX != value) {
                    LastScrollOffsetX = _scrollOffsetX;
                    _scrollOffsetX = value;
                    OnPropertyChanged(nameof(ScrollOffsetX));
                }
            }
        }

        private double _scrollOffsetY;
        public double ScrollOffsetY {
            get => _scrollOffsetY;
            set {
                if (ScrollOffsetY != value) {
                    LastScrollOffsetY = _scrollOffsetY;
                    _scrollOffsetY = value;
                    OnPropertyChanged(nameof(ScrollOffsetY));
                }
            }
        }
        public MpPoint ScrollOffset {
            get => new MpPoint(ScrollOffsetX, ScrollOffsetY);
            set {
                var newVal = value == null ? MpPoint.Zero : value;
                if (ScrollOffsetX != newVal.X) {
                    ScrollOffsetX = newVal.X;
                }
                if (ScrollOffsetY != newVal.Y) {
                    ScrollOffsetY = newVal.Y;
                }
            }
        }

        public double MaxScrollOffsetX {
            get {
                double maxScrollOffsetX = Math.Max(0, QueryTrayTotalWidth - ObservedQueryTrayScreenWidth);
                return maxScrollOffsetX;
            }
        }
        public double MaxScrollOffsetY {
            get {
                double maxScrollOffsetY = Math.Max(0, QueryTrayTotalHeight - ObservedQueryTrayScreenHeight);
                return maxScrollOffsetY;
            }
        }
        public MpPoint MaxScrollOffset => new(MaxScrollOffsetX, MaxScrollOffsetY);
        public MpRect QueryTrayScreenRect =>
            new MpRect(0, 0, ObservedQueryTrayScreenWidth, ObservedQueryTrayScreenHeight);


        public double QueryTrayTotalTileWidth { get; private set; }
        public double QueryTrayTotalTileHeight { get; private set; }

        public double QueryTrayTotalWidth =>
            Math.Max(0, Math.Max(ObservedQueryTrayScreenWidth, QueryTrayTotalTileWidth + QueryTrayVerticalScrollBarWidth));
        public double QueryTrayTotalHeight =>
            Math.Max(0, Math.Max(ObservedQueryTrayScreenHeight, QueryTrayTotalTileHeight + QueryTrayHorizontalScrollBarHeight));


        public double QueryTrayFixedDimensionLength =>
            ListOrientation == Orientation.Horizontal ?
                ObservedQueryTrayScreenHeight : ObservedQueryTrayScreenWidth;

        public double PinTrayFixedDimensionLength =>
            ListOrientation == Orientation.Horizontal ?
                ObservedPinTrayScreenHeight : ObservedPinTrayScreenWidth;
        public double LastZoomFactor { get; set; }
        public double ScrollVelocityX { get; set; }
        public double ScrollVelocityY { get; set; }

        public double ScrollWheelDampeningX {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 0.03d;
                }
                return 0.03d;
            }
        }

        public double ScrollWheelDampeningY {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 0.03d;
                }
                return 0.03d;
            }
        }
        public double ScrollFrictionX {
            get {
                if (Mp.Services.PlatformInfo.IsDesktop) {
                    return LayoutType == MpClipTrayLayoutType.Stack ? 0.85 : 0.5;
                }
                return 0.3;
            }
        }


        public double ScrollFrictionY {
            get {
                if (Mp.Services.PlatformInfo.IsDesktop) {
                    return LayoutType == MpClipTrayLayoutType.Stack ? 0.85 : 0.5;
                }
                return 0.0;
            }
        }

        public bool HasScrollVelocity => Math.Abs(ScrollVelocityX) + Math.Abs(ScrollVelocityY) > 0.1d;
        public MpPoint ScrollVelocity {
            get => new MpPoint(ScrollVelocityX, ScrollVelocityY);
            set {
                var newVal = value == null ? MpPoint.Zero : value;
                ScrollVelocityX = newVal.X;
                ScrollVelocityY = newVal.Y;
            }
        }

        public bool CanSelect =>
            ScrollVelocity.IsValueEqual(MpPoint.Zero);

        public bool IsScrollDisabled { get; set; }
        public bool CanScroll {
            get {
                //return true;

                if (IsScrollDisabled ||
                    MpAvMainWindowViewModel.Instance.IsMainWindowOpening ||
                   !MpAvMainWindowViewModel.Instance.IsMainWindowOpen ||
                    IsRequerying/* ||
                   IsScrollingIntoView*/) {
                    return false;
                }

                if (HasScrollVelocity) {
                    // this implies mouse is/was not over a sub-selectable tile and is scrolling so ignore item scroll if already moving
                    return true;
                }
                if (HoverItem == null) {
                    return true;
                }
                // TODO? giving item scroll priority maybe better by checking if content exceeds visible boundaries here

                if ((HoverItem.IsAnyScrollbarVisible && HoverItem.IsSubSelectionEnabled) ||
                    (HoverItem.IsAnyScrollbarVisible && HoverItem.IsContentHovering) ||
                    HoverItem.TransactionCollectionViewModel.IsTransactionPaneOpen) {
                    // when tray is not scrolling (is still) and mouse is over sub-selectable item keep tray scroll frozen
                    return false;
                }
                return true;
            }
        }

        public bool CanScrollX =>
            CanScroll &&
            QueryTrayTotalTileWidth > ObservedQueryTrayScreenWidth &&
            DefaultScrollOrientation == Orientation.Horizontal;

        public bool CanScrollY =>
            CanScroll &&
            QueryTrayTotalTileHeight > ObservedQueryTrayScreenHeight &&
            DefaultScrollOrientation == Orientation.Vertical;
        public bool IsThumbDraggingX { get; set; } = false;
        public bool IsThumbDraggingY { get; set; } = false;
        public bool IsThumbDragging => IsThumbDraggingX || IsThumbDraggingY;

        public bool IsForcingScroll { get; set; }
        private void SetTotalTileSize() {
            MpSize totalTileSize;
            int tc = Mp.Services.Query.TotalAvailableItemsInQuery;
            if (LayoutType == MpClipTrayLayoutType.Stack) {
                double dist = GetFlatDistToQueryIdx(tc, ListOrientation == Orientation.Horizontal);
                if (ListOrientation == Orientation.Horizontal) {
                    totalTileSize = new MpSize(dist, GetMaxQueryTileHeight());
                } else {
                    totalTileSize = new MpSize(GetMaxQueryTileWidth(), dist);
                }
            } else {
                var (row, col) = GetGridLocFromQueryIdx(tc);

                //adjust fixed idx so its rectangluar
                if (ListOrientation == Orientation.Horizontal) {
                    row = row + 1;
                    col = CurGridFixedCount + 1;
                } else {
                    row = CurGridFixedCount + 1;
                    col = col + 1;
                }

                totalTileSize = new MpSize(col * DefaultQueryItemWidth, row * DefaultQueryItemHeight);
            }
            QueryTrayTotalTileWidth = totalTileSize.Width;
            QueryTrayTotalTileHeight = totalTileSize.Height;
        }
        private int GetQueryIdxFromScrollOffset(double scrollx, double scrolly) {
            if (LayoutType == MpClipTrayLayoutType.Stack) {
                if (ListOrientation == Orientation.Horizontal) {
                    return (int)(scrollx / DefaultQueryItemWidth);
                }
                return (int)(scrolly / DefaultQueryItemHeight);
            }
            if (ListOrientation == Orientation.Horizontal) {
                return (int)(scrolly / DefaultQueryItemHeight) * (CurGridFixedCount);
            }
            return (int)(scrollx / DefaultQueryItemWidth) * (CurGridFixedCount);
        }

        public (int, int) GetGridLocFromQueryIdx(int qidx) {
            if (CurGridFixedCount == 0) {
                return (0, 0);
            }
            int row, col;
            if (ListOrientation == Orientation.Horizontal) {
                row = (int)Math.Floor((double)qidx / (double)CurGridFixedCount);
                col = qidx % CurGridFixedCount;
            } else {
                row = qidx % CurGridFixedCount;
                col = (int)Math.Floor((double)qidx / (double)CurGridFixedCount);
            }
            return (row, col);
        }
        private MpRect GetQueryTileRect(int queryOffsetIdx) {
            MpPoint loc = GetQueryPosition(queryOffsetIdx);
            MpSize size = GetQueryItemSize(queryOffsetIdx);
            return new MpRect(loc, size);
        }

        private int FindJumpTileIdx(double scrollOffsetX, double scrollOffsetY, out MpRect tileRect) {
            int qidx = GetQueryIdxFromScrollOffset(scrollOffsetX, scrollOffsetY);

            var actual_pos = GetQueryPosition(qidx);
            if (IsQueryItemResizeEnabled) {
                while (actual_pos.X > scrollOffsetX || actual_pos.Y > scrollOffsetY) {
                    if (qidx < 0) {
                        break;
                    }
                    actual_pos = GetQueryPosition(--qidx);
                }
            }
            MpSize size = GetQueryItemSize(qidx);
            tileRect = new MpRect(actual_pos, size);
            return qidx;
        }

        private double GetMaxQueryTileHeight() {
            if (MpAvPersistentClipTilePropertiesHelper.UniqueQueryHeights.Any()) {
                return
                    MpAvPersistentClipTilePropertiesHelper
                    .UniqueQueryHeights
                    .OrderByDescending(x => x.Item2)
                    .First()
                    .Item2;
            }
            return DefaultQueryItemHeight;
        }
        private double GetMaxQueryTileWidth() {
            if (MpAvPersistentClipTilePropertiesHelper.UniqueQueryWidths.Any()) {
                return
                    MpAvPersistentClipTilePropertiesHelper
                    .UniqueQueryWidths
                    .OrderByDescending(x => x.Item2)
                    .First()
                    .Item2;
            }
            return DefaultQueryItemWidth;
        }
        private double GetFlatDistToQueryIdx(int qidx, bool is_horiz) {
            if (qidx < 0) {
                return 0;
            }
            if (is_horiz) {
                double def_dist_x = DefaultQueryItemWidth * qidx;
                double u_dist_x =
                    MpAvPersistentClipTilePropertiesHelper.UniqueQueryWidths
                    .Where(x => x.Item1 < qidx)
                    .Sum(x => x.Item2 - DefaultQueryItemWidth);
                double result_x = def_dist_x + u_dist_x;
                return result_x;
            }
            double def_dist_y = DefaultQueryItemHeight * qidx;
            double u_dist_y =
                MpAvPersistentClipTilePropertiesHelper.UniqueQueryHeights
                .Where(x => x.Item1 < qidx)
                .Sum(x => x.Item2 - DefaultQueryItemHeight);
            double result_y = def_dist_y + u_dist_y;
            return result_y;
        }
        private MpSize GetQueryItemSize(int qidx) {
            double w =
                MpAvPersistentClipTilePropertiesHelper.TryGetUniqueWidth_ByOffsetIdx(qidx, out double uw) ?
                    uw : DefaultQueryItemWidth;
            double h =
                MpAvPersistentClipTilePropertiesHelper.TryGetUniqueHeight_ByOffsetIdx(qidx, out double uh) ?
                    uh : DefaultQueryItemHeight;
            return new MpSize(w, h);
        }
        private MpPoint GetQueryPosition(int qidx) {
            if (qidx <= 0) {
                return MpPoint.Zero;
            }
            if (LayoutType == MpClipTrayLayoutType.Stack) {
                double dist = GetFlatDistToQueryIdx(qidx, ListOrientation == Orientation.Horizontal);
                if (ListOrientation == Orientation.Horizontal) {
                    return new MpPoint(dist, 0);
                }
                return new MpPoint(0, dist);
            }
            var (row, col) = GetGridLocFromQueryIdx(qidx);
            return new MpPoint(col * DefaultQueryItemWidth, row * DefaultQueryItemHeight);
        }


        #region Default Tile Layout 

        public double DesiredMaxTileRight {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return ObservedQueryTrayScreenWidth;// - ScrollBarFixedAxisSize;
                } else {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return ObservedQueryTrayScreenWidth - ScrollBarFixedAxisSize;
                    }
                    return double.PositiveInfinity;
                }
            }
        }

        public double DesiredMaxTileBottom {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return ObservedQueryTrayScreenHeight - ScrollBarFixedAxisSize;
                    }
                    return double.PositiveInfinity;
                } else {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return ObservedQueryTrayScreenHeight - ScrollBarFixedAxisSize;
                }
            }
        }


        private double _defaultQueryItemWidth;
        public double DefaultQueryItemWidth =>
            _defaultQueryItemWidth; // NOTE updated in F

        private double _defaultQueryItemHeight;
        public double DefaultQueryItemHeight =>
            _defaultQueryItemHeight;

        private void UpdateDefaultItemSize() {
            // NOTE safe_pad keeps content slightly smaller than container
            // where padding/margin may lead to false scrollbar visibility
            // (like 1 horiz layout pin item shows vert scrollbar)
            double safe_pad = 2.0d;
            // BUG this doesn't consider layout/orientation where
            // in horiz stack vertical sb at default height bounces size around shouldn't be factored
            // just always considering it now...
            double vsbw = ScrollBarFixedAxisSize; //QueryTrayVerticalScrollBarWidth;
            double hsbh = ScrollBarFixedAxisSize; //QueryTrayVerticalScrollBarWidth;
            double w = DEFAULT_ITEM_SIZE - vsbw - safe_pad;
            double h = DEFAULT_ITEM_SIZE - hsbh - safe_pad;

            if (ListOrientation == Orientation.Vertical) {
                h = DEFAULT_UNEXPANDED_HEIGHT;
            } else if (LayoutType == MpClipTrayLayoutType.Grid &&
                        Mp.Services.Query.TotalAvailableItemsInQuery > CurGridFixedCount) {
                // when there's multiple query rows shorten height a bit to 
                // hint theres more there (if not multiple rows, don't shorten looks funny
                //h *= 0.7;
            }

            _defaultQueryItemWidth = w;
            _defaultQueryItemHeight = h;

            _defaultPinItemWidth = w;
            _defaultPinItemHeight = h;

            OnPropertyChanged(nameof(DefaultQueryItemWidth));
            OnPropertyChanged(nameof(DefaultQueryItemHeight));
            OnPropertyChanged(nameof(DefaultPinItemWidth));
            OnPropertyChanged(nameof(DefaultPinItemHeight));

            MpAvPersistentClipTilePropertiesHelper.DefQuerySize = new MpSize(DefaultQueryItemWidth, DefaultQueryItemHeight);
        }

        private double _defaultPinItemWidth;
        public double DefaultPinItemWidth =>
            _defaultPinItemWidth;

        private double _defaultPinItemHeight;
        public double DefaultPinItemHeight =>
            _defaultPinItemHeight;

        public double ScrollBarFixedAxisSize =>
            16;

        public double QueryTrayHorizontalScrollBarHeight =>
            IsQueryHorizontalScrollBarVisible ? ScrollBarFixedAxisSize : 0;

        public double QueryTrayVerticalScrollBarWidth =>
            IsQueryVerticalScrollBarVisible ? ScrollBarFixedAxisSize : 0;


        #endregion

        #region Virtual


        public int HeadQueryIdx => !SortOrderedItems.Any() ? -1 : SortOrderedItems.Min(x => x.QueryOffsetIdx);

        public int TailQueryIdx => !SortOrderedItems.Any() ? -1 : Items.Max(x => x.QueryOffsetIdx);
        public int FirstPlaceholderItemIdx =>
            Items.IndexOf(PlaceholderItems.FirstOrDefault());

        public int LastNonVisibleItemIdx {
            get {
                // get item closest to top left of screen
                if (VisibleQueryItems.FirstOrDefault() is MpAvClipTileViewModel ctvm) {
                    // find item farthest away from it
                    var farthest_ctvm = QueryItems
                        .AggregateOrDefault((a, b) => ctvm.TrayLocation.Distance(a.TrayLocation) > ctvm.TrayLocation.Distance(b.TrayLocation) ? a : b);
                    return Items.IndexOf(farthest_ctvm);
                }
                return -1;
            }
        }

        public int MaxLoadQueryIdx => Math.Max(0, MaxClipTrayQueryIdx - DefaultLoadCount + 1);

        public int MaxClipTrayQueryIdx => Mp.Services.Query.TotalAvailableItemsInQuery - 1;
        public int MinClipTrayQueryIdx => 0;

        public bool CanThumbDragY => ObservedQueryTrayScreenHeight < QueryTrayTotalHeight;
        public bool CanThumbDragX => ObservedQueryTrayScreenWidth < QueryTrayTotalWidth;

        public bool CanThumbDrag => CanThumbDragX || CanThumbDragY;
        #endregion

        #endregion

        #region MpIActionComponent Implementation

        void MpIActionComponent.RegisterActionComponent(MpIInvokableAction mvm) {
            if (OnCopyItemAdd.HasInvoker(mvm)) {
                return;
            }
            OnCopyItemAdd += mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnCopyItemAdd)} Registered {mvm.Label}");
        }

        void MpIActionComponent.UnregisterActionComponent(MpIInvokableAction mvm) {
            if (!OnCopyItemAdd.HasInvoker(mvm)) {
                return;
            }
            OnCopyItemAdd -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnCopyItemAdd)} Unregistered {mvm.Label}");
        }
        #endregion

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvClipTileViewModel> Items { get; set; } = new ObservableCollection<MpAvClipTileViewModel>();
        public ObservableCollection<MpAvClipTileViewModel> PinnedItems { get; set; } = new ObservableCollection<MpAvClipTileViewModel>();

        public IList<MpAvClipTileViewModel> InternalPinnedItems =>
            PinnedItems
            .Where(x => !x.IsWindowOpen && !x.IsAppendNotifier)
            .Take(MpAvPrefViewModel.Instance.MaxPinClipCount)
            .ToList();

        //public MpAvClipTileViewModel ModalClipTileViewModel { get; private set; }

        public MpAvClipTileViewModel AppendClipTileViewModel =>
            PinnedItems.FirstOrDefault(x => x.IsAppendNotifier);

        public IEnumerable<MpAvClipTileViewModel> QueryItems =>
            Items.Where(x => !x.IsPlaceholder);

        public IEnumerable<MpAvClipTileViewModel> UnpinnedQueryItems =>
            Items.Where(x => !x.IsAnyPlaceholder);

        public IEnumerable<MpAvClipTileViewModel> SortOrderedItems =>
            Items.Where(x => x.QueryOffsetIdx >= 0).OrderBy(x => x.QueryOffsetIdx);

        public IEnumerable<MpAvClipTileViewModel> PlaceholderItems =>
            Items.Where(x => x.IsPlaceholder);
        public IEnumerable<MpAvClipTileViewModel> AllItems =>
            Items.Union(PinnedItems);
        public IEnumerable<MpAvClipTileViewModel> AllActiveItems =>
            AllItems.Where(x => !x.IsPlaceholder);

        public MpAvClipTileViewModel HeadItem =>
            SortOrderedItems.ElementAtOrDefault(0);

        public MpAvClipTileViewModel TailItem =>
            SortOrderedItems.ElementAtOrDefault(SortOrderedItems.Count() - 1);

        public MpAvClipTileViewModel HoverItem =>
            AllActiveItems.FirstOrDefault(x => x.IsHovering);

        public int PersistantSelectedItemId {
            get {
                if (SelectedItem == null) {
                    return MpAvPersistentClipTilePropertiesHelper.GetPersistentSelectedItemId();
                }
                return SelectedItem.CopyItemId;
            }
        }

        public MpAvClipTileViewModel SelectedItem {
            get {
                //if (MpAvAppendNotificationWindow.Instance != null &&
                //    MpAvAppendNotificationWindow.Instance.IsVisible) {
                //    // only visible if mw is not open
                //    return ModalClipTileViewModel;
                //}

                return AllItems
                    .Where(x => x.IsSelected)
                    .OrderByDescending(x => x.LastSelectedDateTime)
                    .FirstOrDefault();
            }
            //private set {
            //    if (!CanSelect) {
            //        return;
            //    }
            //    if (paramValue == null) {
            //        AllItems.ForEach(scrollx => scrollx.IsSelected = false);
            //    } else {
            //        AllItems.ForEach(scrollx => scrollx.IsSelected = scrollx.CopyItemId == paramValue.CopyItemId);
            //    }
            //    OnPropertyChanged(nameof(SelectedItem));
            //    OnPropertyChanged(nameof(SelectedPinTrayItem));
            //    OnPropertyChanged(nameof(SelectedClipTrayItem));
            //}
        }
        public MpAvClipTileViewModel SelectedPinTrayItem {
            get {
                if (SelectedItem == null || !SelectedItem.IsPinned) {
                    return null;
                }
                return SelectedItem;
            }
            set {
                if (!CanSelect) {
                    return;
                }
                if (value == null || value.IsAnyPlaceholder) {
                    // BUG trying to stop case when placeholder is being treated like
                    // init'd tile and selectionState isb being stored but 
                    // presistentSelectedModel will be null and it trips lots of things up
                    // 
                    // NOTE maybe righter to set AllItems to unselected here but not sure.
                    PinnedItems.ForEach(x => x.IsSelected = false);
                } else {
                    //SelectedItem = paramValue;
                    value.IsSelected = true;
                }
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(SelectedPinTrayItem));
            }
        }
        public MpAvClipTileViewModel SelectedClipTrayItem {
            get {
                if (SelectedItem == null || SelectedItem.IsPinned) {
                    return null;
                }
                return SelectedItem;
            }
            set {
                if (!CanSelect) {
                    return;
                }
                if (value == null || value.IsAnyPlaceholder) {
                    // see SelectedPinTray comments
                    Items.ForEach(x => x.IsSelected = false);
                } else {
                    //SelectedItem = paramValue;
                    value.IsSelected = true;
                }
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(SelectedClipTrayItem));
            }
        }

        public IEnumerable<MpAvClipTileViewModel> VisibleQueryItems =>
            Items.Where(x => x.IsAnyQueryCornerVisible && !x.IsPlaceholder);

        public IEnumerable<MpAvClipTileViewModel> VisibleFromTopLeftQueryItems =>
            VisibleQueryItems
            .Where(x => x.ScreenRect.X >= 0 && x.ScreenRect.Y >= 0)
            .OrderBy(x => x.QueryOffsetIdx);

        public MpAvClipTileViewModel CurPasteOrDragItem { get; private set; }
        //Items
        //.Where(scrollx => scrollx.IsAnyQueryCornerVisible && !scrollx.IsPlaceholder)
        //.OrderBy(scrollx => scrollx.TrayX)
        //.ThenBy(scrollx => scrollx.TrayY);

        #endregion

        #region Layout

        #region Observed Dimensions
        public double ObservedContainerScreenWidth { get; set; }
        public double ObservedContainerScreenHeight { get; set; }
        public double ObservedPinTrayScreenWidth { get; set; }
        public double ObservedPinTrayScreenHeight { get; set; }

        public double ObservedQueryTrayScreenWidth { get; set; }

        public double ObservedQueryTrayScreenHeight { get; set; }

        #endregion

        #region Bound Dimensions
        #endregion

        public Orientation DefaultScrollOrientation {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (IsGridLayout) {
                        return Orientation.Vertical;
                    }
                    return Orientation.Horizontal;
                }
                if (IsGridLayout) {
                    return Orientation.Horizontal;
                }
                return Orientation.Vertical;
            }
        }
        public double DefaultPinTrayWidth =>
            ObservedContainerScreenWidth / MpAvThemeViewModel.PHI;


        public double MinPinTrayScreenWidth =>
            IsPinTrayVisible ? MinClipOrPinTrayScreenWidth : 0;
        public double MinPinTrayScreenHeight =>
            IsPinTrayVisible ? MinClipOrPinTrayScreenHeight : 0;

        public double MaxPinTrayScreenWidth {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    return Math.Max(0, ObservedContainerScreenWidth - MinClipTrayScreenWidth);
                }
                return double.PositiveInfinity;
            }
        }
        public double MaxPinTrayScreenHeight {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    return double.PositiveInfinity;
                }
                return Math.Max(0, ObservedContainerScreenHeight - MinClipTrayScreenHeight);
            }
        }

        public double MaxContainerScreenWidth {
            get {
#if MOBILE
                return double.PositiveInfinity;
#else
                if (ListOrientation == Orientation.Horizontal) {
                    return
                        MpAvMainWindowViewModel.Instance.MainWindowWidth -
                        MpAvSidebarItemCollectionViewModel.Instance.TotalSidebarWidth;
                }
                return
                    MpAvMainWindowViewModel.Instance.MainWindowWidth -
                    MpAvMainWindowTitleMenuViewModel.Instance.DefaultTitleMenuFixedLength;
#endif
            }
        }

        public double MaxContainerScreenHeight {
            get {
#if MOBILE
                return double.PositiveInfinity;
#else
                if (ListOrientation == Orientation.Horizontal) {
                    return
                        MpAvMainWindowViewModel.Instance.MainWindowHeight -
                        MpAvMainWindowTitleMenuViewModel.Instance.DefaultTitleMenuFixedLength -
                        MpAvFilterMenuViewModel.Instance.DefaultFilterMenuFixedSize;
                }
                return
                        MpAvMainWindowViewModel.Instance.MainWindowHeight -
                        MpAvSidebarItemCollectionViewModel.Instance.TotalSidebarHeight -
                        MpAvFilterMenuViewModel.Instance.DefaultFilterMenuFixedSize;
#endif
            }
        }

        public double MinClipTrayScreenWidth =>
            MinClipOrPinTrayScreenWidth;
        public double MinClipTrayScreenHeight =>
            MinClipOrPinTrayScreenHeight;
        public double MinClipOrPinTrayScreenWidth =>
            50;
        public double MinClipOrPinTrayScreenHeight =>
            50;
        public double MaxTileWidth =>
            double.PositiveInfinity;// Math.Max(0, ObservedQueryTrayScreenWidth - MAX_TILE_SIZE_CONTAINER_PAD);
        public double MaxTileHeight =>
            double.PositiveInfinity;// Math.Max(0, ObservedQueryTrayScreenHeight - MAX_TILE_SIZE_CONTAINER_PAD);

        public int CurGridFixedCount {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 1;
                }
                if (ListOrientation == Orientation.Horizontal) {
                    int fixed_cols = (int)Math.Floor(DesiredMaxTileRight / DefaultQueryItemWidth);

                    return fixed_cols;
                } else {
                    int fixed_rows = (int)Math.Floor(DesiredMaxTileBottom / DefaultQueryItemHeight);

                    return fixed_rows;
                }
            }
        }
        public bool IsQueryItemResizeEnabled =>
            LayoutType == MpClipTrayLayoutType.Stack;


        private MpClipTrayLayoutType? _layoutType;
        public MpClipTrayLayoutType LayoutType {
            get {
                if (MpAvPrefViewModel.Instance == null) {
                    return MpClipTrayLayoutType.Stack;
                }
                if (_layoutType == null) {
                    _layoutType = MpAvPrefViewModel.Instance.ClipTrayLayoutTypeName.ToEnum<MpClipTrayLayoutType>();
                }
                return _layoutType.Value;
            }
            set {
                if (LayoutType != value) {
                    _layoutType = value;
                    if (MpAvPrefViewModel.Instance != null) {
                        MpAvPrefViewModel.Instance.ClipTrayLayoutTypeName = value.ToString();
                    }
                    OnPropertyChanged(nameof(LayoutType));
                }
            }
        }

        double LeadingHeadLength {
            get {
                if (HeadItem == null) {
                    return 0;
                }
                if (ListOrientation == Orientation.Horizontal) {
                    return HeadItem.ScreenRect.Left;
                }

                return HeadItem.ScreenRect.Top;
            }
        }
        double TrailingTailLength {
            get {
                if (TailItem == null) {
                    return 0;
                }
                return HeadItem.ScreenRect.Left;
            }
        }
        #endregion

        #region Appearance

        public bool ShowTileShadow { get; set; } = true;

        public string EmptyQueryTrayText { get; private set; }
        private string GetEmptyQueryTrayText() {
            if (Mp.Services == null ||
                    Mp.Services.StartupState == null) {
                return string.Empty;
            }
            if (!IsQueryEmpty ||
                !Mp.Services.StartupState.IsPlatformLoaded) {
                return string.Empty;
            }

            string tag_name = string.Empty;
            var scicvm = MpAvSearchCriteriaItemCollectionViewModel.Instance;
            if (scicvm.IsAdvSearchActive && scicvm.IsPendingQuery) {
                tag_name = UiStrings.QueryTrayEmptyPendingTagName;
            } else {
                if (MpAvTagTrayViewModel.Instance.LastSelectedActiveItem == null) {
                    return UiStrings.QueryTrayNoSelection;
                }
                tag_name = MpAvTagTrayViewModel.Instance.LastSelectedActiveItem.TagName;
            }
            return string.Format(UiStrings.QueryTrayEmptyText, tag_name);
        }

        #region System Tray Icons

        public string PlayOrPauseLabel => IsIgnoringClipboardChanges ? UiStrings.TriggerResumeButtonLabel : UiStrings.TriggerPauseButtonLabel;

        public object PlayOrPauseIconResoureKey =>
            new object[] {
                IsIgnoringClipboardChanges ? "PlayImage" : "PauseImage",
                IsIgnoringClipboardChanges ? MpSystemColors.green3 : MpSystemColors.red4 };

        public object AutoCopySysTrayIconSourceObj =>
            new object[] {
                "MouseLeftClickImage",
                IsAutoCopyMode ?
                    MpSystemColors.limegreen :
                        MpAvThemeViewModel.Instance.IsThemeDark ?
                            MpSystemColors.White :
                            MpSystemColors.Black };

        public object RightClickPasteSysTrayIconSourceObj =>
            new object[] {
                "MouseRightClickImage",
                IsRightClickPasteMode ?
                    MpSystemColors.limegreen :
                        MpAvThemeViewModel.Instance.IsThemeDark ?
                            MpSystemColors.White :
                            MpSystemColors.Black };

        public object AppendInlineSysTrayIconSourceObj =>
            new object[] {
                "AppendRunImage",
                IsAppendInsertMode ?
                    MpSystemColors.limegreen :
                        MpAvThemeViewModel.Instance.IsThemeDark ?
                            MpSystemColors.White :
                            MpSystemColors.Black };
        public object AppendLineSysTrayIconSourceObj =>
            new object[] {
                "AppendLineImage",
                IsAppendLineMode ?
                    MpSystemColors.limegreen :
                        MpAvThemeViewModel.Instance.IsThemeDark ?
                            MpSystemColors.White :
                            MpSystemColors.Black };
        #endregion

        #endregion

        #region State
        bool IgnoreContentDelete { get; set; }
        public int PinOpCopyItemId { get; set; } = -1;

        public bool IsAutoEditEnabled =>
            !MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled;

        private Uri _editorUri;
        public Uri EditorUri {
            get {
                if (_editorUri == null) {
                    _editorUri = new Uri(Mp.Services.PlatformInfo.EditorPath.ToFileSystemUriFromPath());
                }
                return _editorUri;
            }
        }
        public bool IsQueryResizeEnabled =>
    LayoutType == MpClipTrayLayoutType.Stack;

        public bool IsRestoringSelection { get; set; }

        #region Append

        private MpAppendModeFlags _appendModeFlags = MpAppendModeFlags.None;
        public MpAppendModeFlags AppendModeStateFlags =>
            _appendModeFlags;

        public bool IsAppendInsertMode { get; set; }
        public bool IsAppendPreMode { get; set; }

        public bool IsAppendLineMode { get; set; }

        public bool IsAppendManualMode { get; set; }
        public bool IsAppendPaused { get; set; }

        public bool IsAnyAppendMode =>
            IsAppendInsertMode || IsAppendLineMode;


        #endregion

        public ObservableCollection<MpCopyItem> PendingNewModels { get; } = new ObservableCollection<MpCopyItem>();

        public bool IsEmptyQueryTextVisible {
            get {
                //if(IsAnyBusy) {
                //    return false;
                //}
                if (MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening) {
                    return false;
                }
                return IsQueryEmpty;
            }
        }

        public bool IsInitialQuery { get; private set; } = true;
        public int RemainingItemsCountThreshold {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 5;
                }
                return CurGridFixedCount * 2;
            }
        }
        public int LoadMorePageSize {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 5;
                }
                return CurGridFixedCount;
            }
        }


        public int DefaultLoadCount {
            get {
                //if (LayoutType == MpClipTrayLayoutType.Stack) {
                //    if (Mp.Services.PlatformInfo.IsDesktop) {
                //        return 20;
                //    }
                //    return 5;
                //} else {
                if (Mp.Services.PlatformInfo.IsDesktop) {
                    return 20;
                    //return LayoutType == MpClipTrayLayoutType.Grid ? 40 : 20;
                }
                return 5;
                //}
            }
        }
        public bool IsTitleLayersVisible { get; set; } = true;
        public bool IsMarqueeEnabled { get; set; } = true;

        public bool IsAddingClipboardItem { get; private set; } = false;
        public bool IsAddingStartupClipboardItem { get; private set; } = false;

        private bool _isPasting = false;
        public bool IsPasting {
            get {
                if (_isPasting) {
                    return true;
                }
                if (Items.Any(x => x.IsPasting)) {
                    // NOTE since copy items can be pasted from hot key and aren't in tray
                    // IsPasting cannot be auto-property
                    _isPasting = true;
                }
                return _isPasting;
            }
            set {
                if (IsPasting != value) {
                    _isPasting = value;
                    OnPropertyChanged(nameof(IsPasting));
                }
            }
        }

        public bool IsPinTrayBusy { get; set; }
        #region Mouse Modes

        public bool IsAnyMouseModeEnabled =>
            IsAutoCopyMode ||
            IsRightClickPasteMode ||
            MpAvExternalDropWindowViewModel.Instance.IsDropWidgetEnabled;


        public bool IsAutoCopyMode { get; set; }

        public bool IsRightClickPasteMode { get; set; }

        #endregion

        public bool CanAddItemWhileIgnoringClipboard {
            get {
                return
                    IsIgnoringClipboardChanges &&
                    Mp.Services != null &&
                    Mp.Services.ClipboardMonitor != null &&
                    Mp.Services.ClipboardMonitor.LastClipboardDataObject != null &&
                    LastAddedClipboardDataObject.IsDataNotEqual(Mp.Services.ClipboardMonitor.LastClipboardDataObject);
            }
        }

        public bool IsIgnoringClipboardChanges { get; set; }
        MpPortableDataObject LastAddedClipboardDataObject { get; set; }

        public bool IsArrowSelecting { get; set; } = false;


        public bool IsQueryTrayEmpty =>
            //MpAvMainWindowViewModel.Instance.IsMainWindowLoading ||
            (!QueryItems.Any() &&
            !IsRequerying);

        public bool IsSelectionReset { get; set; } = false;

        public bool IgnoreSelectionReset { get; set; } = false;

        public bool IsFilteringByApp { get; set; } = false;

        public bool IsQueryEmpty =>
            Mp.Services == null ||
            Mp.Services.Query == null ||
            Mp.Services.Query.TotalAvailableItemsInQuery == 0;

        public bool IsPinTrayEmpty =>
            !InternalPinnedItems.Any();

        public bool IsPinTrayVisible {
            get {
                //if (!IsPinTrayEmpty) {
                //    return true;
                //}
                ////if (IsPinTrayDropPopOutVisible) {
                ////    return true;
                ////} 
                //return false;
                return true;
            }
        }


        public bool IsScrollingIntoView { get; set; }

        public bool IsGridLayout => LayoutType == MpClipTrayLayoutType.Grid;

        public bool IsInPlaceRequerying { get; set; }
        public bool IsRequerying { get; set; } = false;
        public bool IsQuerying { get; set; } = false;
        public bool IsSubQuerying { get; set; } = false;
        public int SparseLoadMoreRemaining { get; set; }

        public MpQuillPasteButtonInfoMessage CurPasteInfoMessage { get; private set; } = new MpQuillPasteButtonInfoMessage();

        #region Drag Drop
        public bool IsAnyDropOverTrays { get; private set; }

        public bool IsDragOverPinTray { get; set; }
        public bool IsDragOverQueryTray { get; set; }

        #endregion

        #region Child Property Wrappers

        public bool IsAnyBusy =>
            AllItems.Any(x => x.IsAnyBusy) ||
            IsAddingStartupClipboardItem ||
            IsBusy;
        public bool IsAnyTileContextMenuOpened => AllItems.Any(x => x.IsContextMenuOpen);

        public bool IsAnyResizing => AllItems.Any(x => x.IsResizing);

        public bool CanAnyResize => AllItems.Any(x => x.CanResize);

        public bool IsAnyEditing => AllItems.Any(x => !x.IsContentAndTitleReadOnly);


        public bool IsAnyHovering => AllItems.Any(x => x.IsHovering);


        public bool IsAnyEditingClipTitle => AllItems.Any(x => !x.IsTitleReadOnly);

        public bool IsAnyEditingClipTile => AllItems.Any(x => !x.IsContentReadOnly);


        public bool IsAnyTilePinned => PinnedItems.Count > 0;

        public bool IsAnyTileHaveFocusWithin {
            get {
                // NOTE for performance not selecting ctvm.IsFocusWithin
                if (Mp.Services.FocusMonitor.FocusElement is Control c &&
                    c.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                    return true;
                }
                return false;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Events

        public event EventHandler<object> OnScrollIntoPinTrayViewRequest;

        public event EventHandler<MpCopyItem> OnCopyItemAdd;

        #endregion

        #region Constructors

        private MpAvClipTrayViewModel() : base(null) { }


        #endregion

        #region Public Methods
        public async Task InitializeAsync() {
            LogPropertyChangedEvents = false;

            IsBusy = true;
            PropertyChanged += MpAvClipTrayViewModel_PropertyChanged;

            Items.CollectionChanged += Items_CollectionChanged;
            PinnedItems.CollectionChanged += PinnedItems_CollectionChanged;

            MpDb.SyncAdd += MpDbObject_SyncAdd;
            MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.SyncDelete += MpDbObject_SyncDelete;

            Mp.Services.ContentQueryTools = this;
            Mp.Services.ContentBuilder = this;

            Mp.Services.ClipboardMonitor.OnClipboardChanged += ClipboardWatcher_OnClipboardChanged;
            Mp.Services.ProcessWatcher.OnAppActivated += ProcessWatcher_OnAppActivated;

            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += Instance_OnGlobalMouseReleased;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            OnPropertyChanged(nameof(LayoutType));

            if (IsIgnoringClipboardChanges == MpAvPrefViewModel.Instance.IsClipboardListeningOnStartup) {
                ToggleIsAppPausedCommand.Execute(null);
            }

            Items.Clear();
            for (int i = 0; i < DefaultLoadCount; i++) {
                var ctvm = await CreateClipTileViewModelAsync(null);
                Items.Add(ctvm);
            }

            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(IsGridLayout));


            await ProcessAccountCapsAsync(MpAccountCapCheckType.Init);
            await UpdateEmptyPropertiesAsync();

            SetCurPasteInfoMessage(Mp.Services.ProcessWatcher.LastProcessInfo);

            OnPropertyChanged(nameof(SelectedItem));
            IsBusy = false;
        }
        public async Task<MpAvClipTileViewModel> CreateClipTileViewModelAsync(MpCopyItem ci, int queryOffsetIdx = -1) {
            MpAvClipTileViewModel ctvm = new MpAvClipTileViewModel(this);
            await ctvm.InitializeAsync(ci, queryOffsetIdx);
            return ctvm;
        }
        public void ShiftQuery(int fromIdx, int deltaIdx) {
            foreach (var ctvm in Items) {
                if (ctvm.QueryOffsetIdx <= fromIdx) {
                    // behind or placeholder ignore
                    continue;
                }
                ctvm.UpdateQueryOffset(ctvm.QueryOffsetIdx + deltaIdx);
            }
        }
        public void ValidateQueryTray() {
            if (ScrollOffset.IsValueEqual(MpPoint.Zero, 1) &&
                HeadQueryIdx > 0) {
                // BUG sometimes when head is unloaded the remaining items aren't
                // shifted up. This maybe from using .ForEach extension to alter their idxs (now changed)
                // It doesn't get caught cause there's no logic to invalidate missing head idx (or idxs)
                // that's what this does for most common case but may need inner and tail detection too
                // (or using a proper loop to shift offsets will fix this entirely)
                MpDebug.Break($"Query validation failed. Ghost head detected, shifting items from former head idx: {HeadQueryIdx}", level: MpLogLevel.Debug, silent: true);
                ShiftQuery(0, -HeadQueryIdx);
            }
            // TODO? add more ghost detection

            var dups =
                Items.Where(x => x.QueryOffsetIdx >= 0 && Items.Any(y => y != x && x.QueryOffsetIdx == y.QueryOffsetIdx));
            var skips =
                Enumerable.Range(HeadQueryIdx, TailQueryIdx - HeadQueryIdx)
                .Where(x => QueryItems.All(y => y.QueryOffsetIdx != x));


            if (!dups.Any() && !skips.Any()) {
                return;
            }
            if (dups.Count() > 0) {
                MpDebug.Break($"Query validation failed. Dup idxs: {string.Join(",", dups)}", level: MpLogLevel.Debug, silent: true);
                dups
                    .OrderByDescending(x => x.TileCreatedDateTime)
                    .Skip(1)
                    .ForEach(x => x.TriggerUnloadedNotification(false));

            }
            if (skips.Any()) {
                MpDebug.Break($"Query validation failed. Skipped idxs: {string.Join(",", skips)}", level: MpLogLevel.Debug, silent: true);

                if (HasScrollVelocity) {
                    // BUG fixing skips sometimes gets caught in a cycle thats hard to detect, trying to limit its use
                    QueryCommand.Execute(new List<List<int>> { skips.ToList() });
                } else {
                    QueryCommand.Execute(null);
                }
            }
        }
        public override string ToString() {
            return $"ClipTray";
        }

        public void RefreshQueryTrayLayout(MpAvClipTileViewModel fromItem = null) {
            UpdateDefaultItemSize();
            SetTotalTileSize();

            fromItem = fromItem == null ? HeadItem : fromItem;
            QueryItems.ForEach(x => UpdateTileLocationCommand.Execute(x));

            OnPropertyChanged(nameof(DefaultQueryItemWidth));
            OnPropertyChanged(nameof(DefaultQueryItemHeight));

            OnPropertyChanged(nameof(QueryTrayTotalHeight));
            OnPropertyChanged(nameof(QueryTrayTotalWidth));

            OnPropertyChanged(nameof(MaxScrollOffsetX));
            OnPropertyChanged(nameof(MaxScrollOffsetY));

            OnPropertyChanged(nameof(QueryTrayTotalTileWidth));
            OnPropertyChanged(nameof(QueryTrayTotalTileHeight));

            OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));

            OnPropertyChanged(nameof(MaxContainerScreenWidth));
            OnPropertyChanged(nameof(MaxContainerScreenHeight));
        }


        #region View Invokers

        public void ScrollIntoView(object obj) {
            MpAvClipTileViewModel ctvm = null;
            if (obj is MpAvClipTileViewModel) {
                ctvm = obj as MpAvClipTileViewModel;
            } else if (obj is int ciid) {
                if (ciid < 0) {
                    // means nothing is selected
                    ScrollIntoView(null);
                    return;
                }
                ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                if (ctvm == null) {

                    Dispatcher.UIThread.Post(async () => {
                        //int ciid_query_idx = Mp.Services.Query.PageTools.GetItemOffsetIdx(ciid);
                        int ciid_query_idx = await Mp.Services.Query.FetchItemOffsetIdxAsync(ciid);
                        if (ciid_query_idx < 0) {
                            // ciid is neither pinned nor in query (maybe should reset query here to id but prolly not right place)
                            MpDebug.Break();
                            return;
                        }
                        QueryCommand.Execute(ciid_query_idx);
                        while (IsAnyBusy) { await Task.Delay(100); }
                        ctvm = Items.FirstOrDefault(x => x.CopyItemId == ciid);
                        if (ctvm == null) {
                            // data model provider should have come up w/ nothing here
                            MpDebug.Break();
                            return;
                        }
                        ScrollIntoView(ctvm);
                    });
                    return;
                }
            } else if (obj == null) {
                // occurs when nothing is selected
                if (IsPinTrayEmpty) {
                    if (IsQueryTrayEmpty) {
                        return;
                    }
                    ScrollIntoView(0);
                    return;
                } else {
                    ctvm = PinnedItems[0];
                }
            }

            if (IsScrollingIntoView || IsAnyBusy) {
                return;
            }
            IsScrollingIntoView = true;
            if (ctvm.IsPinned) {
                OnScrollIntoPinTrayViewRequest?.Invoke(this, obj);
                IsScrollingIntoView = false;
                return;
            }
            if (ctvm.IsAnimating) {
                Dispatcher.UIThread.Post(async () => {
                    while (ctvm.IsAnimating) {
                        await Task.Delay(100);
                    }
                    IsScrollingIntoView = false;
                    ScrollIntoView(ctvm);
                });
                return;
            }

            // Only query items from here
            double pad_origin = MpAvThemeViewModel.Instance.DefaultGridSplitterFixedDimensionLength;
            double pad_extent = ScrollBarFixedAxisSize;
            MpRect svr = new MpRect(
                pad_origin,
                pad_origin,
                ObservedQueryTrayScreenWidth - pad_extent,
                ObservedQueryTrayScreenHeight - pad_extent);

            MpRect ctvm_rect = ctvm.ScreenRect;
            MpPoint delta_scroll_offset = new MpPoint();

            if (ctvm_rect.Left < svr.Left) {
                //item is outside on left
                delta_scroll_offset.X = ctvm_rect.Left - svr.Left;
            } else if (ctvm_rect.Right > svr.Right) {
                //item is outside on right
                delta_scroll_offset.X = ctvm_rect.Right - svr.Right;
            }

            if (ctvm_rect.Top < svr.Top) {
                //item is outside above
                delta_scroll_offset.Y = ctvm_rect.Top - svr.Top;
            } else if (ctvm_rect.Bottom > svr.Bottom) {
                //item is outside below
                delta_scroll_offset.Y = ctvm_rect.Bottom - svr.Bottom;
            }

            var target_offset = ScrollOffset + delta_scroll_offset;
            ScrollVelocity = MpPoint.Zero;
            ForceScrollOffset(target_offset, "scrollIntoView");
            IsScrollingIntoView = false;
        }

        #endregion

        #region Selection & Read-Only Handlers

        public void ClearPinnedSelection(bool clearEditing = true) {
            PinnedItems.ForEach(x => x.ClearSelection(clearEditing));
        }

        public void ClearQuerySelection(bool clearEditing = true) {
            QueryItems.ForEach(x => x.ClearSelection(clearEditing));
            MpAvPersistentClipTilePropertiesHelper.ClearPersistentSelection();
        }

        public void ClearAllSelection(bool clearEditing = true) {
            ClearPinnedSelection(clearEditing);
            ClearQuerySelection(clearEditing);
        }

        public void ResetAllSelection(bool clearEditing = true) {
            IsSelectionReset = true;
            ClearQuerySelection(clearEditing);
            ClearPinnedSelection(clearEditing);

            MpAvClipTileViewModel to_select_ctvm = InternalPinnedItems.FirstOrDefault();
            if (to_select_ctvm == null) {
                to_select_ctvm = UnpinnedQueryItems.OrderBy(x => x.QueryOffsetIdx).FirstOrDefault();
            }
            AllItems.ForEach(x => x.IsSelected = x == to_select_ctvm);

            IsSelectionReset = false;
        }

        #endregion



        public void InitIntroItems() {
            //var introItem1 = new MpCopyItem(
            //        MpCopyItemType.RichText,
            //        "Welcome to MonkeyPaste!",
            //        MpHelpers.ConvertPlainTextToRichText("Take a moment to look through the available features in the following tiles, which are always available in the 'Help' pinboard"));

            //var introItem2 = new MpCopyItem(
            //    MpCopyItemType.RichText,
            //    "One place for your clipboard",
            //    MpHelpers.ConvertPlainTextToRichText(""));
            //MpJsonPreferenceIO.Instance.IsInitialLoad = false;
            //
        }

        public void NotifySelectionChanged() {
            OnPropertyChanged(nameof(SelectedPinTrayItem));
            OnPropertyChanged(nameof(SelectedClipTrayItem));
            OnPropertyChanged(nameof(SelectedItem));
            MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
        }

        public void StoreSelectionState(MpAvClipTileViewModel ctvm) {
            if (ctvm.IsAnyPlaceholder) {
                // started happening in external pin tray drop
                //MpDebug.Break();
                return;
            }
            if (!ctvm.IsSelected) {
                return;
            }

            MpAvPersistentClipTilePropertiesHelper.SetPersistentSelectedItem(ctvm.CopyItemId, ctvm.QueryOffsetIdx);
            AllItems.Where(x => x != ctvm).ForEach(x => x.IsSelected = false);
        }

        public void RestoreSelectionState(MpAvClipTileViewModel tile) {
            if (MpAvPersistentClipTilePropertiesHelper.GetPersistentSelectedItemId() != tile.CopyItemId) {
                tile.ClearSelection(false);
                return;
            }

            IsRestoringSelection = true;

            tile.IsSelected = true;

            IsRestoringSelection = false;
        }

        private async Task CleanupAfterPasteAsync(MpAvClipTileViewModel sctvm, MpPortableProcessInfo pasted_pi, MpPortableDataObject mpdo) {
            IsPasting = false;
            //clean up pasted items state after paste
            sctvm.PasteCount++;
            sctvm.IsPasting = false;

            if (pasted_pi == null) {
                return;
            }

            string pasted_app_url = await Mp.Services.SourceRefTools.FetchOrCreateAppRefUrlAsync(pasted_pi);
            if (string.IsNullOrEmpty(pasted_app_url)) {
                // f'd
                MpDebug.Break();
                return;
            }

            Mp.Services.TransactionBuilder.ReportTransactionAsync(
                copyItemId: sctvm.CopyItemId,
                reqType: MpJsonMessageFormatType.DataObject,
                req: null,//mpdo.SerializeData(),
                respType: MpJsonMessageFormatType.None,
                resp: null,
                ref_uris: new[] { pasted_app_url },
                transType: MpTransactionType.Pasted).FireAndForgetSafeAsync(this);
        }

        public void NotifyDragOverTrays(bool isOver) {
            IsAnyDropOverTrays = isOver;
            if (isOver) {
                MpMessenger.SendGlobal(MpMessageType.DropOverTraysBegin);
            } else {
                MpMessenger.SendGlobal(MpMessageType.DropOverTraysEnd);
            }
            // make sure file
            AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.CanDrop)));
        }

        public MpSize GetCurrentDefaultPinTrayRatio() {
            MpSize p_ratio = new MpSize(1, 1);
            double pin_tray_var_dim_ratio = 0.5;

            if (ListOrientation == Orientation.Vertical) {
                p_ratio.Height = pin_tray_var_dim_ratio;
            } else {
                p_ratio.Width = pin_tray_var_dim_ratio;
            }
            return p_ratio;
        }

        private bool _isProcessingCap = false;
        public async Task ProcessAccountCapsAsync(MpAccountCapCheckType source, object arg = null) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(async () => { await ProcessAccountCapsAsync(source, arg); });
                return;
            }

            _isProcessingCap = true;

            int added_ciid = 0;
            if (source == MpAccountCapCheckType.Add &&
                arg is int ciid) {
                added_ciid = ciid;
            }

            if (source == MpAccountCapCheckType.Link &&
                arg is int tid &&
                tid < 0 &&
                MpTag.TrashTagId == -tid) {
                // unlinking from trash tag should be internally treated internally as add
                source = MpAccountCapCheckType.Add;
                MpConsole.WriteLine($"Unlinking item from trash back to content, source changed from link to add.");
            }


            string last_cap_info = MpAvAccountTools.Instance.LastCapInfo.ToString();
            MpUserAccountType account_type = MpAvAccountViewModel.Instance.WorkingAccountType;
            var cap_info = await MpAvAccountTools.Instance.RefreshCapInfoAsync(account_type, source, added_ciid);
            MpConsole.WriteLine($"Account cap refreshed. SourceControl: '{source}' Args: '{arg.ToStringOrDefault()}' Info:", true);
            MpConsole.WriteLine(cap_info.ToString(), false, true);

            int cur_content_cap = MpAvAccountTools.Instance.GetContentCapacity(account_type);
            int cur_trash_cap = MpAvAccountTools.Instance.GetTrashCapacity(account_type);
            int cap_msg_timeout = 5_000;

            bool apply_changes = false;
            string cap_msg_title_suffix = string.Empty;
            string cap_msg_icon = string.Empty;
            var cap_msg_sb = new StringBuilder();
            MpNotificationType cap_msg_type = MpNotificationType.None;

            if (source == MpAccountCapCheckType.Add) {
                if (cap_info.ToBeTrashed_ciid > 0) {
                    cap_msg_icon = MpContentCapInfo.NEXT_TRASH_IMG_RESOURCE_KEY;
                    cap_msg_title_suffix = UiStrings.NtfCapContentReachedTitleSuffix;
                    cap_msg_sb.AppendLine(
                        string.Format(UiStrings.NtfCapContentMaxStorageText, account_type.EnumToUiString(), cur_content_cap));
                }
                if (cap_info.ToBeRemoved_ciid > 0) {
                    cap_msg_icon = MpContentCapInfo.NEXT_REMOVE_IMG_RESOURCE_KEY;
                    if (string.IsNullOrEmpty(cap_msg_title_suffix)) {
                        cap_msg_title_suffix = UiStrings.NtfCapTrashReachedTitleSuffix;
                    } else {
                        cap_msg_title_suffix = UiStrings.NtfCapBothReachedTitleSuffix;
                    }
                    cap_msg_sb.AppendLine(
                        string.Format(UiStrings.NtfCapTrashMaxStorageText, cur_trash_cap));
                }
                if (!string.IsNullOrEmpty(cap_msg_sb.ToString())) {
                    apply_changes = true;
                    cap_msg_type = MpNotificationType.ContentCapReached;
                    cap_msg_sb.AppendLine(string.Empty);
                    cap_msg_sb.AppendLine(UiStrings.NtfCapHideHint);
                }

            } else if (source == MpAccountCapCheckType.AddBlock || source == MpAccountCapCheckType.RestoreBlock) {
                // block refresh called BEFORE an add would occur to check favorite count again and avoid delete
                // since tag linking doesn't refresh caps, this does it when last add set account to block state
                //string block_prefix =  source.ToString().Replace("Block", string.Empty);
                string block_prefix =
                    source == MpAccountCapCheckType.AddBlock ?
                        UiStrings.CommonAddLabel : UiStrings.NtfCapBlockRestoreLabel;
                MpNotificationType block_msg_type =
                    source == MpAccountCapCheckType.AddBlock ?
                        MpNotificationType.ContentAddBlockedByAccount :
                        MpNotificationType.ContentRestoreBlockedByAccount;

                if (MpAvAccountTools.Instance.IsContentAddPausedByAccount) {
                    // no linking changes, add will be blocked
                    cap_msg_title_suffix = string.Format(UiStrings.NtfCapBlockSuffix, block_prefix);
                    cap_msg_sb.AppendLine(string.Format(UiStrings.NtfCapBlockHint1, MpReadOnlyTagType.Favorites.EnumToUiString()));
                    cap_msg_sb.AppendLine(string.Format(UiStrings.NtfCapBlockHint2, account_type.EnumToUiString(), cur_content_cap));
                    cap_msg_icon = MpContentCapInfo.ADD_BLOCKED_RESOURCE_KEY;
                    cap_msg_type = block_msg_type;
                } else {
                    // links were changed since last refresh, item will be added..
                }
            } else if (source == MpAccountCapCheckType.Link &&
                arg is int tag_id) {
                bool is_unlink = tag_id < 0;
                tag_id = Math.Abs(tag_id);
                if (tag_id == MpTag.TrashTagId) {
                    if (is_unlink) {

                    }
                }
            }

            if (apply_changes) {
                // only after add or (un)link to trash/favorites

                await TrashOrDeleteCopyItemIdAsycn(cap_info.ToBeTrashed_ciid, false, true);
                await TrashOrDeleteCopyItemIdAsycn(cap_info.ToBeRemoved_ciid, true, true);

                // refresh cap for view changes
                var updated_cap_info = await MpAvAccountTools.Instance.RefreshCapInfoAsync(account_type, MpAccountCapCheckType.Refresh);
            }
            AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsNextRemovedByAccount)));
            AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsNextTrashedByAccount)));
            AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsAnyNextCapByAccount)));
            _isProcessingCap = false;

            if (cap_msg_type == MpNotificationType.None) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var sw = Stopwatch.StartNew();
                string title = string.Format("'{0}' {1}", account_type.EnumToUiString(), cap_msg_title_suffix);
                string msg = cap_msg_sb.ToString();
                while (sw.ElapsedMilliseconds < 3_000) {
                    if (_isProcessingCap) {
                        // new cap happened, suppress this ntf
                        MpConsole.WriteLine($"Ignoring cap msg. title '{title}' msg '{msg}'");
                        return;
                    }
                    await Task.Delay(100);
                }

                Mp.Services.NotificationBuilder.ShowMessageAsync(
                           title: title,
                           body: msg,
                           msgType: cap_msg_type,
                           iconSourceObj: cap_msg_icon,
                           maxShowTimeMs: cap_msg_timeout).FireAndForgetSafeAsync();
            });
        }

        #endregion

        #region Protected Methods

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc &&
                AllActiveItems.FirstOrDefault(x => sc.IsShortcutCommand(x)) is MpAvClipTileViewModel sc_ctvm) {
                sc_ctvm.OnPropertyChanged(nameof(sc_ctvm.KeyString));
                return;
            }

            if (e is MpCopyItemAnnotation cia &&
                AllActiveItems.FirstOrDefault(x => cia.CopyItemId == x.CopyItemId) is MpAvClipTileViewModel cia_ctvm) {
                Dispatcher.UIThread.Post(async () => {
                    await cia_ctvm.InitializeAsync(cia_ctvm.CopyItem, cia_ctvm.QueryOffsetIdx);
                    //wait for model to propagate then trigger view to reload
                    if (cia_ctvm.GetContentView() is MpIContentView cv) {
                        cv.LoadContentAsync().FireAndForgetSafeAsync();
                    }
                });
                return;
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc &&
                AllActiveItems.FirstOrDefault(x => sc.IsShortcutCommand(x)) is MpAvClipTileViewModel sc_ctvm) {
                sc_ctvm.OnPropertyChanged(nameof(sc_ctvm.KeyString));
                return;
            }
            //if (e is MpCopyItem ci &&
            //    AllActiveItems.FirstOrDefault(x => x.CopyItemId == ci.Id) is MpAvClipTileViewModel ci_ctvm) {
            //    if (ci_ctvm.HasModelChanged) {
            //        // this means the model has been updated from the view model so ignore
            //    } else {
            //        // BUG this gets called when it shouldn't when user is editing
            //        // which unexpands tile or further unintended things
            //        // disabling this is as close as I can get to fixing it
            //        // may mess up some important external updates, not sure...
            //        //Dispatcher.UIThread.Post(async () => {
            //        //    await ci_ctvm.InitializeAsync(ci, ci_ctvm.QueryOffsetIdx);
            //        //    //wait for model to propagate then trigger view to reload
            //        //    if (ci_ctvm.GetContentView() is MpIContentView cv) {
            //        //        cv.LoadContentAsync().FireAndForgetSafeAsync();
            //        //    }
            //        //});
            //    }
            //    return;
            //}
        }

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc &&
                AllActiveItems.FirstOrDefault(x => sc.IsShortcutCommand(x)) is MpAvClipTileViewModel sc_ctvm) {
                sc_ctvm.OnPropertyChanged(nameof(sc_ctvm.KeyString));
                return;
            }

            if (e is MpCopyItem ci && !IgnoreContentDelete) {
                if (AppendClipTileViewModel != null &&
                    ci.Id == AppendClipTileViewModel.CopyItemId &&
                    IsAnyAppendMode) {
                    await DeactivateAppendModeAsync();
                }
                MpAvPersistentClipTilePropertiesHelper.RemoveProps(ci.Id);


                var removed_ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ci.Id);
                if (removed_ctvm != null) {
                    bool wasSelected = removed_ctvm.IsSelected;

                    if (removed_ctvm.IsPinned) {
                        var pctvm = PinnedItems.FirstOrDefault(x => x.CopyItemId == ci.Id);

                        if (pctvm != null) {
                            int pinIdx = PinnedItems.IndexOf(pctvm);
                            UnpinTileCommand.Execute(pctvm);

                        }
                    } else {
                        int removedQueryOffsetIdx = removed_ctvm.QueryOffsetIdx;
                        removed_ctvm.TriggerUnloadedNotification(true);

                        if (wasSelected) {
                            var sel_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == removedQueryOffsetIdx);
                            if (sel_ctvm == null) {
                                // when tail
                                sel_ctvm = Items.OrderBy(x => x.QueryOffsetIdx).Last();
                            }
                            AllItems.ForEach(x => x.IsSelected = x == sel_ctvm);
                        }
                    }
                }
                while (!QueryCommand.CanExecute(string.Empty)) {
                    await Task.Delay(100);
                }
                QueryCommand.Execute(string.Empty);

                OnPropertyChanged(nameof(IsQueryEmpty));
            }
            //else if (e is MpCopyItemTag cit &&
            //            MpAvTagTrayViewModel.Instance.LastSelectedActiveItem is MpAvTagTileViewModel sttvm &&
            //            sttvm.IsLinkTag &&
            //            !sttvm.IsTrashTag &&
            //            !sttvm.IsAllTag) {

            //    // check if unlink is part of current query
            //    bool is_part_of_query =
            //        sttvm
            //        .SelfAndAllDescendants
            //        .Cast<MpAvTagTileViewModel>()
            //        .Select(x => x.TagId)
            //        .Any(x => x == cit.TagId);

            //    if (is_part_of_query) {
            //        // when unlinked item is part of current query remove its qidx and do a reset query
            //        Mp.Services.Query.NotifyQueryChanged();
            //        //QueryCommand.Execute(string.Empty);
            //    }
            //}
        }

        #region Sync Events

        private void MpDbObject_SyncDelete(object sender, MpDbSyncEventArgs e) {
            Dispatcher.UIThread.Post((Action)(() => {
                if (sender is MpCopyItem ci) {
                    var ctvmToRemove = AllItems.FirstOrDefault(x => x.CopyItemId == ci.Id);
                    if (ctvmToRemove != null) {
                        ctvmToRemove.CopyItem.StartSync(e.SourceGuid);
                        //ctvmToRemove.CopyItem.Color.StartSync(e.SourceGuid);
                        Items.Remove(ctvmToRemove);
                        ctvmToRemove.CopyItem.EndSync();
                        //ctvmToRemove.CopyItem.Color.EndSync();
                    }
                }
            }));
        }

        private void MpDbObject_SyncUpdate(object sender, MpDbSyncEventArgs e) {
            //Dispatcher.UIThread.Post((Action)(() => {
            //}));
        }

        private void MpDbObject_SyncAdd(object sender, MpDbSyncEventArgs e) {
            //Dispatcher.UIThread.Post(async () => {
            //    if (sender is MpCopyItem ci) {
            //ci.StartSync(e.SourceGuid);

            //var svm = MpAvSourceCollectionViewModel.Instance.Items.FirstOrDefault(scrollx => scrollx.SourceId == ci.SourceId);

            //var app = svm.AppViewModel.App;
            //app.StartSync(e.SourceGuid);
            ////ci.Source.App.Icon.StartSync(e.SourceGuid);
            ////ci.Source.App.Icon.IconImage.StartSync(e.SourceGuid);

            //var dupCheck = this.GetClipTileViewModelById((int)ci.Id);
            //if (dupCheck == null) {
            //    if (ci.Id == 0) {
            //        await ci.WriteToDatabaseAsync();
            //    }
            //    _newModels.Add(ci);
            //    //AddNewTiles();
            //} else {
            //    MpConsole.WriteTraceLine(@"Warning, attempting to add existing copy item: " + dupCheck.CopyItem.ItemData + " ignoring and updating existing.");
            //    //dupCheck.CopyItem = ci;
            //}
            //app.EndSync();
            ////ci.Source.App.Icon.EndSync();
            ////ci.Source.App.Icon.IconImage.EndSync();
            //ci.EndSync();

            //ResetClipSelection();
            //    }
            //});
        }

        #endregion

        #endregion

        #endregion

        #region Private Methods

        private void MpAvClipTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(LastAddedClipboardDataObject):
                    OnPropertyChanged(nameof(CanAddItemWhileIgnoringClipboard));
                    break;
                case nameof(PinOpCopyItemId):
                    AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsPinOpTile)));
                    break;
                case nameof(ScrollVector):
                    QueryCommand.Execute(ScrollVector);
                    break;
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                //case nameof(IsAnyBusy):
                //    if (!IsAnyBusy) {
                //        break;
                //    }
                //    Dispatcher.UIThread.Post(async () => {
                //        while (true) {
                //            if (PercentLoaded >= 1) {
                //                return;
                //            }
                //            await Task.Delay(100);
                //        }
                //    });
                //    break;
                //case nameof(ModalClipTileViewModel):
                //    if (ModalClipTileViewModel == null) {
                //        return;
                //    }
                //    ModalClipTileViewModel.OnPropertyChanged(nameof(ModalClipTileViewModel.CopyItemId));

                //    break;
                case nameof(SelectedItem):
                    MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(CanScroll));
                    break;
                case nameof(CurGridFixedCount):
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        break;
                    }
                    ScrollToAnchor();
                    break;
                case nameof(QueryTrayTotalTileWidth):
                case nameof(QueryTrayTotalTileHeight):
                    if (QueryTrayTotalTileWidth < 0 || QueryTrayTotalTileHeight < 0) {
                        MpDebug.Break();
                        ObservedQueryTrayScreenWidth = 0;
                    }
                    OnPropertyChanged(nameof(MaxScrollOffsetX));
                    OnPropertyChanged(nameof(MaxScrollOffsetY));

                    OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
                    OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));
                    break;
                case nameof(ObservedQueryTrayScreenWidth):
                case nameof(ObservedQueryTrayScreenHeight):
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        break;
                    }
                    RefreshQueryTrayLayout();
                    break;
                case nameof(ListOrientation):
                    OnPropertyChanged(nameof(IsVerticalOrientation));
                    break;
                case nameof(ScrollOffsetX):
                case nameof(ScrollOffsetY):
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayScrollChanged);
                    break;
                case nameof(MaxScrollOffsetX):
                case nameof(MaxScrollOffsetY):
                    break;
                case nameof(IsThumbDraggingX):
                case nameof(IsThumbDraggingY):
                    OnPropertyChanged(nameof(IsThumbDragging));
                    break;
                case nameof(IsThumbDragging):
                    if (!IsThumbDragging) {
                        QueryCommand.Execute(ScrollOffset);
                    }
                    break;
                case nameof(HasScrollVelocity):
                    if (HasScrollVelocity) {

                    }
                    //MpPlatformWrapper.Services.Cursor.IsCursorFrozen = HasScrollVelocity;

                    //if (HasScrollVelocity) {
                    //    Mp.Services.Cursor.UnsetCursor(null);
                    //} else {
                    //    var hctvm = Items.FirstOrDefault(scrollx => scrollx.IsHovering);
                    //    if (IsAnyBusy) {
                    //        OnPropertyChanged(nameof(IsBusy));
                    //    }
                    //}
                    break;

                case nameof(MaxTileHeight):
                case nameof(MaxTileWidth):
                case nameof(QueryTrayVerticalScrollBarWidth):
                case nameof(QueryTrayHorizontalScrollBarHeight):
                case nameof(QueryTrayFixedDimensionLength):
                case nameof(PinTrayFixedDimensionLength):
                    UpdateDefaultItemSize();
                    break;
                case nameof(DefaultQueryItemWidth):
                    QueryItems.ForEach(x => x.OnPropertyChanged(nameof(x.MinWidth)));
                    break;
                case nameof(DefaultQueryItemHeight):
                    QueryItems.ForEach(x => x.OnPropertyChanged(nameof(x.MinHeight)));
                    break;

                case nameof(DefaultPinItemWidth):
                    PinnedItems.ForEach(x => x.OnPropertyChanged(nameof(x.MinWidth)));
                    break;
                case nameof(DefaultPinItemHeight):
                    PinnedItems.ForEach(x => x.OnPropertyChanged(nameof(x.MinHeight)));
                    break;

                case nameof(IsAnyTilePinned):
                    MpMessenger.SendGlobal(MpMessageType.PinTrayEmptyOrHasTile);
                    break;
                case nameof(ObservedContainerScreenHeight):
                    OnPropertyChanged(nameof(MaxPinTrayScreenHeight));
                    break;
                case nameof(ObservedContainerScreenWidth):
                    OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                    break;
                case nameof(CanTouchScroll):
                    if (!CanTouchScroll) {
                        IsTouchScrolling = false;
                    }
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                // CONTENT RESIZE
                case MpMessageType.ContentResized:
                    ScrollToAnchor();
                    break;
                case MpMessageType.SelectedSidebarItemChangeBegin:
                    //SetScrollAnchor();
                    break;
                case MpMessageType.SelectedSidebarItemChangeEnd:
                    //ScrollToAnchor();
                    RefreshQueryTrayLayout();
                    break;
                case MpMessageType.SidebarItemSizeChangeBegin:
                    //SetScrollAnchor();
                    break;
                case MpMessageType.SidebarItemSizeChanged:
                    //RefreshQueryTrayLayout();
                    break;
                case MpMessageType.SidebarItemSizeChangeEnd:
                    //ScrollToAnchor();
                    RefreshQueryTrayLayout();
                    break;
                // LAYOUT CHANGE
                case MpMessageType.PinTrayResizeBegin:
                    //SetScrollAnchor();
                    break;
                case MpMessageType.PinTraySizeChanged:
                    //RefreshQueryTrayLayout();
                    break;
                case MpMessageType.PinTrayResizeEnd:
                    RefreshQueryTrayLayout();
                    break;
                case MpMessageType.MainWindowInitialOpenComplete:
                    ResetTraySplitterCommand.Execute(null);
                    break;
                case MpMessageType.PreTrayLayoutChange:
                    _isLayoutChanging = true;
                    SetScrollAnchor();
                    ResetItemSizes(true, false);
                    break;
                case MpMessageType.PostTrayLayoutChange:
                    ScrollToAnchor();
                    _isLayoutChanging = false;
                    break;
                // MAIN WINDOW SIZE
                case MpMessageType.MainWindowSizeChangeBegin:
                    SetScrollAnchor();
                    break;
                case MpMessageType.MainWindowSizeChanged:
                    ScrollToAnchor();
                    break;
                case MpMessageType.MainWindowSizeChangeEnd:
                    // NOTE Size reset doesn't call changed so treat end as changed too
                    ScrollToAnchor();
                    break;

                // MAIN WINDOW ORIENTATION
                case MpMessageType.MainWindowOrientationChangeBegin:
                    _isMainWindowOrientationChanging = true;
                    SetScrollAnchor();
                    break;
                case MpMessageType.MainWindowOrientationChangeEnd:

                    ResetItemSizes(true, true);
                    OnPropertyChanged(nameof(ListOrientation));
                    AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsExpanded)));
                    AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.MaxTitleHeight)));
                    AllActiveItems
                        .Where(x => x.GetContentView() != null)
                        .Select(x => x.GetContentView())
                        .OfType<MpAvContentWebView>()
                        .ForEach(x => x.ResizerControl = null);

                    //RefreshQueryTrayLayout();
                    ScrollToAnchor();
                    _isMainWindowOrientationChanging = false;
                    break;

                // SCROLL JUMP

                case MpMessageType.JumpToIdxCompleted:
                    RefreshQueryTrayLayout();
                    break;

                // SELECTION

                case MpMessageType.TraySelectionChanged:
                    OnPropertyChanged(nameof(CanScroll));
                    break;

                case MpMessageType.TrayScrollChanged:
                    if (_isMainWindowOrientationChanging ||
                        _isLayoutChanging ||
                        IsForcingScroll) {
                        break;
                    }
                    CheckLoadMore();
                    break;
                case MpMessageType.MainWindowOpened:
                    if (SelectedItem == null) {
                        ResetAllSelection(false);
                    }
                    AddNewItemsCommand.Execute(null);
                    break;
                // QUERY

                case MpMessageType.RequeryCompleted:
                    UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();
                    RefreshQueryTrayLayout();

                    if (IsInitialQuery) {
                        IsInitialQuery = false;
                        Dispatcher.UIThread.Post(async () => {
                            while (IsAnyBusy) {
                                await Task.Delay(100);
                            }
                            // BUG this works around initial tile size being tiny and triggering resize fits
                            // them right
                            MpMessenger.SendGlobal(MpMessageType.MainWindowSizeChangeEnd);
                        });
                    }

                    break;
                case MpMessageType.QueryChanged:
                    QueryCommand.Execute(null);
                    break;
                case MpMessageType.SubQueryChanged:
                    QueryCommand.Execute(string.Empty);
                    break;
                case MpMessageType.TotalQueryCountChanged:
                    OnPropertyChanged(nameof(IsQueryEmpty));
                    OnPropertyChanged(nameof(Mp.Services.Query.TotalAvailableItemsInQuery));
                    break;

                // DND
                case MpMessageType.DropWidgetEnabledChanged:
                    OnPropertyChanged(nameof(IsAnyMouseModeEnabled));
                    break;
                case MpMessageType.ItemDragBegin:
                    CurPasteOrDragItem = SelectedItem;
                    break;
                case MpMessageType.ItemDragEnd:
                case MpMessageType.ItemDragCanceled:
                case MpMessageType.ContentPasted:
                    if (CurPasteOrDragItem != null &&
                        CurPasteOrDragItem.GetContentView() is MpAvContentWebView cwv) {
                        cwv.SendMessage($"pasteOrDropCompleteResponse_ext()");
                    }
                    CurPasteOrDragItem = null;
                    break;
            }
        }

        private void Instance_OnGlobalMouseReleased(object sender, bool e) {
            // BUG in some cases dragend/dragcancel/drop isn't getting reported
            // and corner buttons stop showing up this shall fix it

            // TODO if any dragging or dropping, wait 3 secs,
            // if still, cancel all and log. May need to reinit clip trays


            IsAnyDropOverTrays = false;
            // TODO Add disableDrag() called at end of contentChange in editor.
            // See if cef startDrag or js dragEnter gets called.
            // Both should never get called, cut cef out of dnd.
            // Render/browser process crap is jamming it up, too many layers  

        }
        private void ProcessWatcher_OnAppActivated(object sender, MpPortableProcessInfo e) {
            Dispatcher.UIThread.Post(() => SetCurPasteInfoMessage(e), DispatcherPriority.Background);
        }
        private void SetCurPasteInfoMessage(MpPortableProcessInfo e) {
            if (!MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                // no paste toolbar so ignore
                // TODO? add plain text paste toolbar? (tip of an iceburg)
                return;
            }
            if (e == null) {
                // unknown paste app
                CurPasteInfoMessage = new MpQuillPasteButtonInfoMessage() {
                    pasteButtonTooltipHtml = UiStrings.ClipTilePasteButtonDisabledTooltip
                };
            } else {
                bool is_custom =
                    MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(e)
                        is MpAvAppViewModel avm &&
                    avm != null &&
                    !avm.OleFormatInfos.IsDefault;

                CurPasteInfoMessage = new MpQuillPasteButtonInfoMessage() {
                    pasteButtonTooltipHtml =
                        string.Format(
                            UiStrings.ClipTilePasteButtonTooltipHtml,
                        string.IsNullOrEmpty(e.ApplicationName) ?
                            e.MainWindowTitle :
                            e.ApplicationName),

                    pasteButtonTooltipText =
                        string.Format(
                            UiStrings.ClipTilePasteButtonTooltipText,
                        string.IsNullOrEmpty(e.ApplicationName) ?
                            e.MainWindowTitle :
                            e.ApplicationName),
                    pasteButtonIconBase64 = e.MainWindowIconBase64,
                    infoId = e.ProcessPath,
                    isFormatDefault = !is_custom
                };
            }

            string msg = $"enableSubSelection_ext('{CurPasteInfoMessage.SerializeObjectToBase64()}')";

            var to_notify_ctvml =
                AllActiveItems
                    .Where(x => x.IsSubSelectionEnabled)
                    .Select(x => x.GetContentView() as MpAvContentWebView)
                    .Where(x => x != null);
            if (to_notify_ctvml.IsNullOrEmpty()) {
                // no sub-selectable items to ntf
                return;
            }
            to_notify_ctvml
                .ForEach(x => x.SendMessage(msg));

            MpConsole.WriteLine($"{to_notify_ctvml.Count()} items notified of active app change");
        }

        private void ClipboardWatcher_OnClipboardChanged(object sender, MpPortableDataObject mpdo) {
            // NOTE this is on a bg thread

            Dispatcher.UIThread.Post(() => OnPropertyChanged(nameof(CanAddItemWhileIgnoringClipboard)));

            bool is_startup_ido = Mp.Services.ClipboardMonitor.IsStartupClipboard;

            bool is_ext_change = !MpAvWindowManager.IsAnyActive || is_startup_ido;

            bool is_change_ignored =
                !is_startup_ido &&
                (IsIgnoringClipboardChanges ||
                 (MpAvPrefViewModel.Instance.IgnoreInternalClipboardChanges && !is_ext_change));

            if (is_startup_ido && !is_change_ignored && !MpAvPrefViewModel.Instance.AddClipboardOnStartup) {
                // ignore startup item
                is_change_ignored = true;
            }

            if (is_change_ignored) {
                MpConsole.WriteLine("Clipboard Change Ignored by tray", true);
                MpConsole.WriteLine($"Mp.Services.StartupState.IsReady: {Mp.Services.StartupState.IsReady}");
                MpConsole.WriteLine($"IsIgnoringClipboardChanges: {IsIgnoringClipboardChanges}");
                MpConsole.WriteLine($"IsThisAppActive: {MpAvWindowManager.IsAnyActive}");
                MpConsole.WriteLine($"is_startup_ido: {is_startup_ido}");
                MpConsole.WriteLine($"IgnoreInternalClipboardChanges: {MpAvPrefViewModel.Instance.IgnoreInternalClipboardChanges}", false, true);
                return;
            }

            Dispatcher.UIThread.Post(async () => {
                IsAddingStartupClipboardItem = is_startup_ido;
                await BuildFromDataObjectAsync(mpdo as MpAvDataObject, false, MpDataObjectSourceType.ClipboardWatcher);
                //await AddItemFromDataObjectAsync(mpdo as MpAvDataObject);

                IsAddingStartupClipboardItem = false;
            });
        }

        private async Task TrashOrDeleteCopyItemIdAsycn(int ciid, bool isDelete, bool ignorePostSel = false) {
            if (ciid == 0) {
                return;
            }

            if (AllActiveItems.FirstOrDefault(x => x.CopyItemId == ciid)
                    is MpAvClipTileViewModel tod_ctvm) {
                // flag fade out in view
                if (!ignorePostSel && tod_ctvm.NearestNonPlaceholderNeighbor != null) {
                    //tod_ctvm.NearestNonPlaceholderNeighbor.IsSelected = true;
                    await tod_ctvm.NearestNonPlaceholderNeighbor.FocusContainerAsync(NavigationMethod.Directional); ;
                }
                RemoveItemCommand.Execute(tod_ctvm);
            }
            if (isDelete) {
                //while (IsBusy) { await Task.Delay(100); }

                //IsBusy = true;
                await MpDataModelProvider.DeleteItemAsync<MpCopyItem>(ciid);
                await ProcessAccountCapsAsync(MpAccountCapCheckType.Remove, ciid);

            } else {
                // add link to trash tag which caches ciid
                await MpAvTagTrayViewModel.Instance.TrashTagViewModel
                    .LinkCopyItemCommand.ExecuteAsync(ciid);
            }
        }

        private async Task<MpAvClipTileViewModel> CreateOrRetrieveClipTileViewModelAsync(object ci_or_ciid) {

            MpAvClipTileViewModel nctvm = null;
            MpCopyItem ci = ci_or_ciid as MpCopyItem;
            if (ci == null) {
                if (ci_or_ciid is int ciid) {
                    ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(ciid);
                }
            }
            if (ci == null) {
                if (ci_or_ciid is int missing_ciid && missing_ciid > 0) {
                    MpDebug.Break($"Missing ciid cannot retrieve. Was it deleted? {missing_ciid}");
                }
                //} else if (ci.WasDupOnCreate) {
            } else {
                nctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ci.Id);
            }
            if (nctvm == null) {
                nctvm = await CreateClipTileViewModelAsync(ci);
            }
            var sw = Stopwatch.StartNew();
            while (nctvm.IsBusy) {
                // NOTE don't wait for anything but tile since view won't be loaded
                await Task.Delay(100);
                if (sw.ElapsedMilliseconds > 5_000) {
                    MpDebug.Break($"CreateOrRetrieveClipTileViewModelAsync timeout reached, unbusying and returning tile: '{nctvm}'");
                    nctvm.IsBusy = false;
                    break;
                }
            }
            return nctvm;
        }
        private void ResetItemSizes(bool query, bool pin) {
            if (query &&
                QueryItems.Where(x => MpAvPersistentClipTilePropertiesHelper.HasUniqueSize(x.CopyItemId, x.QueryOffsetIdx)) is IEnumerable<MpAvClipTileViewModel> to_clear_query) {
                to_clear_query.ForEach(x => x.ResetTileSizeToDefaultCommand.Execute(null));
            }
            if (pin &&
                InternalPinnedItems.Where(x => MpAvPersistentClipTilePropertiesHelper.HasUniqueSize(x.CopyItemId, x.QueryOffsetIdx)) is IEnumerable<MpAvClipTileViewModel> to_clear_pinned) {
                to_clear_pinned.ForEach(x => x.ResetTileSizeToDefaultCommand.Execute(null));
            }
        }
        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();
            if (e.OldItems != null) {
                foreach (MpAvClipTileViewModel octvm in e.OldItems) {
                    octvm.DisposeViewModel();
                }
            }
        }

        private void PinnedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(InternalPinnedItems));
            OnPropertyChanged(nameof(IsPinTrayEmpty));
            OnPropertyChanged(nameof(IsAnyTilePinned));

            // NOTE tile init is before added to pin collection,
            // need to refresh size when added/removed
            if (e.NewItems != null &&
                e.NewItems.Cast<MpAvClipTileViewModel>() is IEnumerable<MpAvClipTileViewModel> npctvml) {
                npctvml.ForEach(x => x.OnPropertyChanged(nameof(x.IsPinned)));
            }
            if (e.OldItems != null &&
                e.OldItems.Cast<MpAvClipTileViewModel>() is IEnumerable<MpAvClipTileViewModel> opctvml) {
                opctvml.ForEach(x => x.OnPropertyChanged(nameof(x.IsPinned)));
            }
            UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();
        }

        public async Task UpdateEmptyPropertiesAsync() {
            // send signal immediatly but also wait and send for busy dependants
            OnPropertyChanged(nameof(IsPinTrayEmpty));
            OnPropertyChanged(nameof(IsQueryEmpty));
            OnPropertyChanged(nameof(IsQueryTrayEmpty));
            OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));

            var sw = Stopwatch.StartNew();
            while (IsAnyBusy) {
                if (sw.ElapsedMilliseconds > 3_000) {
                    break;
                }
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(IsQueryEmpty));
            OnPropertyChanged(nameof(IsPinTrayEmpty));
            OnPropertyChanged(nameof(IsQueryTrayEmpty));
            OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));
            EmptyQueryTrayText = GetEmptyQueryTrayText();
        }

        #region Scroll Offset
        private void ForceScrollOffsetX(double newOffsetX) {
            LastScrollOffsetX = newOffsetX;
            _scrollOffsetX = newOffsetX;
            OnPropertyChanged(nameof(ScrollOffsetX));
        }

        private void ForceScrollOffsetY(double newOffsetY) {
            LastScrollOffsetY = newOffsetY;
            _scrollOffsetY = newOffsetY;
            OnPropertyChanged(nameof(ScrollOffsetY));
        }
        private MpPoint ClampScrollOffsetToFixedAxis(MpPoint offset) {
            if (LayoutType == MpClipTrayLayoutType.Stack) {
                if (ListOrientation == Orientation.Horizontal) {
                    offset.Y = 0;
                } else {
                    offset.X = 0;
                }
            } else {
                if (ListOrientation == Orientation.Horizontal) {
                    offset.X = 0;
                } else {
                    offset.Y = 0;
                }
            }

            return offset;
        }
        private void ForceScrollOffset(MpPoint newOffset, string source) {
            if ((newOffset - ScrollOffset).Length < 1) {
                // avoid double query
                //MpConsole.WriteLine($"Force ScrollOffset reject length: {(newOffset - ScrollOffset).Length}");
                return;
            }

            if (source == "iprq") {
                newOffset = ClampScrollOffsetToFixedAxis(newOffset);
            }
            var old_offset = ScrollOffset;
            IsForcingScroll = true;
            ForceScrollOffsetX(newOffset.X);
            ForceScrollOffsetY(newOffset.Y);
            IsForcingScroll = false;
            MpAvPagingListBoxExtension.ForceScrollOffset(newOffset);
            //MpConsole.WriteLine($"ScrollOffset forced from '{old_offset}' to '{newOffset}'");
        }
        #endregion

        #region Scroll Anchor

        private int FindCurScrollAnchor() {
            int anchor_idx = 0;
            // NOTE omitting below, I think this is what causes ghost head
            //if (SelectedItem != null &&
            //    !SelectedItem.IsPinned) {
            //    // prefer to anchor to selection
            //    anchor_idx = SelectedItem.QueryOffsetIdx;
            //} else 
            if (VisibleFromTopLeftQueryItems.FirstOrDefault() is MpAvClipTileViewModel anchor_ctvm) {
                // anchor to item with top left visible closest to top left
                anchor_idx = anchor_ctvm.QueryOffsetIdx;
            } else if (MaxClipTrayQueryIdx > 0) {
                anchor_idx = HeadQueryIdx;
            }
            if (anchor_idx > 0) {
                // set anchor to start edge 
                anchor_idx = GetEdgeQueryIdxByDir(anchor_idx, -1);
            }
            return anchor_idx;
        }
        private bool CanScrollToAnchor() {
            if (!_query_anchor_idx.HasValue) {
                // no anchor set
                return false;
            }

            if (_query_anchor_idx.Value == FindCurScrollAnchor() &&
                VisibleFromTopLeftQueryItems.FirstOrDefault() is MpAvClipTileViewModel origin_ctvm &&
                origin_ctvm.QueryOffsetIdx == _query_anchor_idx.Value
                ) {
                // already at anchor

                //return false;
            }
            return true;
        }
        private void SetScrollAnchor() {
            if (_query_anchor_idx.HasValue) {
                //MpConsole.WriteLine($"SetScrollAnchor ignored, anchor already set.");
            }
            _query_anchor_idx = FindCurScrollAnchor();
            //MpConsole.WriteLine($"[SET] Anchor idx: {_query_anchor_idx.Value}");
        }

        private void ScrollToAnchor() {
            Dispatcher.UIThread.Post(async () => {
                RefreshQueryTrayLayout();

                while (true) {
                    if (!CanScrollToAnchor()) {
                        _query_anchor_idx = null;
                        CheckLoadMore();
                        return;
                    }
                    if (QueryCommand.CanExecute(new MpPoint())) {
                        break;
                    }
                    await Task.Delay(100);
                }
                while (!QueryCommand.CanExecute(_query_anchor_idx.Value)) {
                    await Task.Delay(100);
                }
                await QueryCommand.ExecuteAsync(_query_anchor_idx.Value);
                _query_anchor_idx = null;
                RefreshQueryTrayLayout();

                CheckLoadMore();
            });
        }

        #endregion

        #region Keyboard Tile Navigation
        public bool CanTileNavigate() {
            bool canNavigate =
                    //!IsAnyBusy &&
                    !IsRequerying &&
                    !IsArrowSelecting &&
                    !HasScrollVelocity &&
                    !IsScrollingIntoView;

            if (!canNavigate ||
                Mp.Services.FocusMonitor.FocusElement is not Control fe) {
                return false;
            }

            bool is_tile_focused = fe.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out var focus_ctvm);
            if (!is_tile_focused) {
                // tile not ancestor of current focus, reject
                return false;
            }

            if (SelectedItem == null) {
                return true;
            }

            bool is_editor_nav =
                SelectedItem.IsSubSelectionEnabled && SelectedItem.GetContentView() is Control cv && (cv.IsFocused || cv.IsKeyboardFocusWithin);
            bool is_title_nav =
                !SelectedItem.IsTitleReadOnly && SelectedItem.IsTitleFocused;


            if (is_editor_nav ||
                is_title_nav) {
                return false;
            }

            return true;
        }

        private async Task SelectNeighborHelperAsync(int y_dir, int x_dir) {
            if (y_dir != 0 && x_dir != 0) {
                // NO! should only be one or the other
                MpDebug.Break();
                return;
            }
            if (y_dir == 0 && x_dir == 0) {
                return;
            }

            IsArrowSelecting = true;
            ScrollIntoView(PersistantSelectedItemId);
            await Task.Delay(100);
            while (IsAnyBusy) { await Task.Delay(100); }
            if (SelectedItem == null) {
                ResetAllSelection(false);
                IsArrowSelecting = false;
                return;
            }

            MpAvClipTileViewModel cur_ctvm = SelectedItem;
            MpAvClipTileViewModel target_ctvm = null;

            while (true) {
                // iterate neighbors until result isn't pin placeholder
                if (cur_ctvm == null) {
                    break;
                }
                if (y_dir != 0) {
                    target_ctvm = await GetNeighborByRowOffsetAsync(cur_ctvm, y_dir > 0);
                } else {
                    target_ctvm = await GetNeighborByColumnOffsetAsync(cur_ctvm, x_dir > 0);
                }

                if (target_ctvm != null &&
                    !target_ctvm.IsPinPlaceholder) {
                    break;
                }
                cur_ctvm = target_ctvm;
            }


            if (target_ctvm != null) {
                target_ctvm.IsSelected = true;
                await target_ctvm.FocusContainerAsync(NavigationMethod.Directional);
            }
            IsArrowSelecting = false;
        }
        private async Task<MpAvClipTileViewModel> GetNeighborByRowOffsetAsync(MpAvClipTileViewModel ctvm, bool is_next) {
            int row_offset = is_next ? 1 : -1;

            if (ctvm.IsPinned) {
                return
                        InternalPinnedItems
                            .Where(x => is_next ? x.ObservedBounds.Y > ctvm.ObservedBounds.Y : x.ObservedBounds.Y < ctvm.ObservedBounds.Y)
                            .OrderBy(x => x.ObservedBounds.Location.Distance(ctvm.ObservedBounds.Location))
                            .FirstOrDefault();
            }
            if (LayoutType == MpClipTrayLayoutType.Stack &&
                ListOrientation == Orientation.Horizontal) {
                // horizontal stack, no row change
                return null;
            }
            if (ListOrientation == Orientation.Horizontal &&
                LayoutType == MpClipTrayLayoutType.Grid) {
                // horizontal grid qidx's are left-to-right 
                // adj row qidx by fixed row count
                row_offset *= CurGridFixedCount;
            }
            int target_qidx = ctvm.QueryOffsetIdx + row_offset;
            if (target_qidx < 0 ||
                target_qidx >= Mp.Services.Query.TotalAvailableItemsInQuery) {
                // target outside range
                return null;
            }
            MpAvClipTileViewModel target_ctvm = QueryItems.FirstOrDefault(x => x.QueryOffsetIdx == target_qidx);
            if (target_ctvm != null) {
                return target_ctvm;
            }
            if (is_next) {
                await ScrollToNextPageCommand.ExecuteAsync();
            } else {
                await ScrollToPreviousPageCommand.ExecuteAsync();
            }
            return QueryItems.FirstOrDefault(x => x.QueryOffsetIdx == target_qidx);
        }

        private async Task<MpAvClipTileViewModel> GetNeighborByColumnOffsetAsync(MpAvClipTileViewModel ctvm, bool is_next) {
            int col_offset = is_next ? 1 : -1;
            if (ctvm.IsPinned) {
                // find col neighbor of pinned tile
                int target_pidx = InternalPinnedItems.IndexOf(ctvm) + col_offset;
                if (target_pidx < 0 ||
                    target_pidx >= InternalPinnedItems.Count) {
                    // target outside internal pin item range
                    return null;
                }
                return InternalPinnedItems[target_pidx];
            }
            // find col neighbor of query tile
            if (ListOrientation == Orientation.Vertical &&
                LayoutType == MpClipTrayLayoutType.Grid) {
                // vertical grid qidx's are top-to-bottom, left-to-right 
                // adj qidx by fixed row count
                col_offset *= CurGridFixedCount;
            }
            int target_qidx = ctvm.QueryOffsetIdx + col_offset;
            if (target_qidx < 0 ||
                target_qidx >= Mp.Services.Query.TotalAvailableItemsInQuery) {
                // target outside range
                return null;
            }
            var target_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == target_qidx);
            if (target_ctvm == null) {
                // target is outside current query page
                while (target_ctvm == null) {
                    // perform load more in target dir
                    QueryCommand.Execute(col_offset > 0);
                    while (IsQuerying) { await Task.Delay(100); }
                    target_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == target_qidx);
                }
            }
            return target_ctvm;
        }
        #endregion

        private async Task<MpCopyItem> AddItemFromDataObjectAsync(MpAvDataObject avdo, bool is_copy = false) {
            LastAddedClipboardDataObject = avdo;
            try {
                await MpFifoAsyncQueue.WaitByConditionAsync(
                    lockObj: _addDataObjectContentLock,
                    time_out_ms: ADD_CONTENT_TIMEOUT_MS,
                    waitWhenTrueFunc: () => {
                        bool is_waiting =
                            IsAddingClipboardItem ||
                            MpAvPlainHtmlConverter.Instance.IsBusy ||
                            !MpAvPlainHtmlConverter.Instance.IsLoaded ||
                            !Mp.Services.StartupState.IsCoreLoaded;
                        if (is_waiting) {
                            MpConsole.WriteLine($"waiting to add item to cliptray...(IsAddingClipboardItem:{IsAddingClipboardItem},MpAvPlainHtmlConverter.Instance.IsBusy:{MpAvPlainHtmlConverter.Instance.IsBusy},Mp.Services.StartupState.IsCoreLoaded:{Mp.Services.StartupState.IsCoreLoaded})");
                        }
                        return is_waiting;
                    },
                    debug_label: "Add Content Queue Item");
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Add content ex. Probably too hot outside. ", ex);
                return null;
            }

            IsAddingClipboardItem = true;
            if (MpAvAccountTools.Instance.IsContentAddPausedByAccount) {
                MpConsole.WriteLine($"Add content blocked, acct capped. Ensuring accuracy...");
                await ProcessAccountCapsAsync(MpAccountCapCheckType.AddBlock, avdo);
                if (MpAvAccountTools.Instance.IsContentAddPausedByAccount) {
                    MpConsole.WriteLine($"Add content blocked confirmed.");
                    IsAddingClipboardItem = false;
                    return null;
                }
            }

            try {
                MpCopyItem newCopyItem = await _copyItemBuilder.BuildAsync(
                avdo: avdo,
                transType: MpTransactionType.Created,
                force_allow_dup: is_copy);

                MpCopyItem processed_result = await AddUpdateOrAppendCopyItemAsync(newCopyItem);

                IsAddingClipboardItem = false;

                return processed_result;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error adding item from dataobject: '{avdo}'.", ex);
                IsAddingClipboardItem = false;
            }
            return null;
        }

        public async Task<MpCopyItem> AddUpdateOrAppendCopyItemAsync(MpCopyItem ci) {
            if (ci == null) {
                MpConsole.WriteLine("Could not build copyitem, cannot add");
                OnCopyItemAdd?.Invoke(this, null);
                return null;
            }

            if (ci.WasDupOnCreate) {
                //item is a duplicate
                MpConsole.WriteLine("Duplicate item detected, incrementing copy count and updating copydatetime");

                ci.CopyCount++;
                // reseting CopyDateTime will move item to top of recent list
                ci.CopyDateTime = DateTime.Now;
                await ci.WriteToDatabaseAsync();

                if (MpAvTagTrayViewModel.Instance.TrashedCopyItemIds.Contains(ci.Id)) {
                    await MpAvTagTrayViewModel.Instance.TrashTagViewModel.UnlinkCopyItemCommand.ExecuteAsync(ci.Id);
                    MpConsole.WriteLine($"Duplicate item '{ci.Title}' unlinked from trash");
                }
                // update cap in view
                ProcessAccountCapsAsync(MpAccountCapCheckType.Refresh).FireAndForgetSafeAsync();
            } else if (IsModelToBeAdded(ci)) {
                await ProcessAccountCapsAsync(MpAccountCapCheckType.Add, ci.Id);
                MpAvTagTrayViewModel.Instance.AllTagViewModel.UpdateClipCountAsync().FireAndForgetSafeAsync();
            }

            if (AppendClipTileViewModel == null &&
                PendingNewModels.All(x => x.Id != ci.Id)) {
                PendingNewModels.Add(ci);
            }

            bool wasAppended = false;
            if (IsAnyAppendMode && ci.DataObjectSourceType.IsAppendableSourceType()) {
                wasAppended = await UpdateAppendModeAsync(ci);
                if (!wasAppended && PendingNewModels.All(x => x.Id != ci.Id)) {
                    PendingNewModels.Add(ci);
                }
            } else if (PendingNewModels.FirstOrDefault(x => x.Id == ci.Id) is
                        MpCopyItem existing_pending_ci) {
                // when thi item is already pending remove old pending
                // since this has most accurate info
                PendingNewModels.Remove(existing_pending_ci);
                PendingNewModels.Add(ci);
            } else {
                PendingNewModels.Add(ci);
            }

            MpCopyItem result_ci = ci;
            if (wasAppended) {
                MpMessenger.SendGlobal(MpMessageType.AppendBufferChanged);
                if (MpAvPrefViewModel.Instance.IgnoreAppendedItems) {
                    // when item was appended and append items are ignored
                    // the appended item is deleted after data and sources
                    // are transferred to append host item so creating 
                    // a new ref to append host model and flagging 
                    // WasDupOnCreate since it alraedy existed and don't
                    // want to alter the view model's copyitem instance 

                    result_ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(AppendClipTileViewModel.CopyItemId);
                    result_ci.WasDupOnCreate = true;
                }

            } else {
                MpMessenger.SendGlobal(MpMessageType.ContentAdded);
                AddNewItemsCommand.Execute(null);
            }
            OnCopyItemAdd?.Invoke(this, result_ci);
            return result_ci;
        }

        private readonly object _pasteLockObj = new object();
        private async Task PasteClipTileAsync(MpAvClipTileViewModel ctvm, MpPasteSourceType pasteSource) {
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
            if (IsPasting) {
                try {
                    await MpFifoAsyncQueue.WaitByConditionAsync(
                        lockObj: _pasteLockObj,
                        waitWhenTrueFunc: () => {
                            return IsPasting;
                        },
                        debug_label: $"Paste {ctvm}");
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Paste for item '{ctvm}' FAILED.", ex);
                    return;
                }
                // re-silent lock since last will unlock
                MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
            }

            MpPortableDataObject mpdo = null;
            ctvm.IsPasting = true;
            CurPasteOrDragItem = ctvm;

            MpPortableProcessInfo pi = Mp.Services.ProcessWatcher.LastProcessInfo;
            var cv = ctvm.GetContentView();
            if (cv == null) {
                if (ctvm.CopyItem != null) {
                    mpdo = ctvm.GetDataObjectByModel(false, pi);
                }
            } else if (cv is MpAvIContentDragSource ds) {
                mpdo = await ds.GetDataObjectAsync(
                    formats: ctvm.GetOleFormats(false, pi),
                    use_placeholders: false,
                    ignore_selection: pasteSource == MpPasteSourceType.Hotkey);
            }

            // NOTE paste success is very crude, false positive is likely
            bool success = await Mp.Services.ExternalPasteHandler.PasteDataObjectAsync(mpdo, pi);
            MpMessenger.SendGlobal(MpMessageType.ContentPasted);
            if (!success) {
                // clear pi to ignore paste history
                pi = null;
                MpMessenger.SendGlobal(MpMessageType.AppError);
            }

            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
            await CleanupAfterPasteAsync(ctvm, pi, mpdo);
        }
        private void CutOrCopySelection(bool isCut) {
            string keys = isCut ?
            Mp.Services.PlatformShorcuts.CutKeys : Mp.Services.PlatformShorcuts.CopyKeys;

            Mp.Services.KeyStrokeSimulator.SimulateKeyStrokeSequence(keys);
        }

        #endregion

        #region Commands

        public ICommand UpdateTileLocationCommand => new MpCommand<object>(
            (args) => {
                if (args is not MpAvClipTileViewModel ctvm) {
                    return;
                }

                var ctvm_loc = GetQueryPosition(ctvm.QueryOffsetIdx);
                ctvm.TrayY = ctvm_loc.Y;
                ctvm.TrayX = ctvm_loc.X;

                //if (ctvm.QueryOffsetIdx == TailQueryIdx) {
                //    // when tail position changed re-measure/assign placeholder previews from tail
                //    int tq_idx = TailQueryIdx;
                //    int preview_count = 0;
                //    foreach (var ph_ctvm in PlaceholderItems) {
                //        int preview_qidx = tq_idx + 1 + preview_count;
                //        // only make previews for actual items
                //        ph_ctvm.IsPreviewPlaceholder = preview_qidx < Mp.Services.Query.TotalAvailableItemsInQuery;
                //        MpRect ph_rect =
                //            ph_ctvm.IsPreviewPlaceholder ?
                //                GetQueryTileRect(preview_qidx) :
                //                MpRect.Empty;

                //        ph_ctvm.TrayY = ph_rect.Location.Y;
                //        ph_ctvm.TrayX = ph_rect.Location.X;
                //        ph_ctvm.BoundWidth = ph_rect.Width;
                //        ph_ctvm.BoundHeight = ph_rect.Height;

                //        preview_count++;
                //    }
                //}
            });

        public ICommand ToggleLayoutTypeCommand => new MpCommand(
            () => {
                MpMessenger.SendGlobal(MpMessageType.PreTrayLayoutChange);

                LayoutType = IsGridLayout ? MpClipTrayLayoutType.Stack : MpClipTrayLayoutType.Grid;


                MpMessenger.SendGlobal(MpMessageType.PostTrayLayoutChange);

                OnPropertyChanged(nameof(IsGridLayout));

            });

        public ICommand ScrollToHomeCommand => new MpCommand(
            () => {
                QueryCommand.Execute(0);
            },
            () => {
                return CanTileNavigate();
            });

        public ICommand ScrollToEndCommand => new MpCommand(
            () => {
                QueryCommand.Execute(MaxClipTrayQueryIdx);
            }, () => {
                return CanTileNavigate();
            });

        public MpIAsyncCommand ScrollToNextPageCommand => new MpAsyncCommand(
             async () => {
                 MpPoint scroll_delta = MpPoint.Zero;
                 if (DefaultScrollOrientation == Orientation.Horizontal) {
                     scroll_delta.X = ObservedQueryTrayScreenWidth;
                 } else {
                     scroll_delta.Y = ObservedQueryTrayScreenHeight;
                 }
                 var nextPageOffset = (ScrollOffset + scroll_delta);
                 await QueryCommand.ExecuteAsync(nextPageOffset);
             },
            () => {
                return CanTileNavigate();
            });

        public MpIAsyncCommand ScrollToPreviousPageCommand => new MpAsyncCommand(
            async () => {
                MpPoint scroll_delta = MpPoint.Zero;
                if (DefaultScrollOrientation == Orientation.Horizontal) {
                    scroll_delta.X = ObservedQueryTrayScreenWidth;
                } else {
                    scroll_delta.Y = ObservedQueryTrayScreenHeight;
                }
                var prevPageOffset = (ScrollOffset - scroll_delta);
                await QueryCommand.ExecuteAsync(prevPageOffset);
            },
            () => {
                return CanTileNavigate();
            });

        public MpIAsyncCommand SelectNextRowItemCommand => new MpAsyncCommand(
            async () => {
                await SelectNeighborHelperAsync(1, 0);
            }, () => {
                return CanTileNavigate();
            });

        public ICommand SelectPreviousRowItemCommand => new MpAsyncCommand(
            async () => {
                await SelectNeighborHelperAsync(-1, 0);
            }, () => {
                return CanTileNavigate();
            });

        public ICommand SelectNextColumnItemCommand => new MpAsyncCommand(
            async () => {
                await SelectNeighborHelperAsync(0, 1);
            },
            () => {
                return CanTileNavigate();
            });

        public ICommand SelectPreviousColumnItemCommand => new MpCommand(
            async () => {
                await SelectNeighborHelperAsync(0, -1);
            },
            () => {
                return CanTileNavigate();
            });


        public MpIAsyncCommand<object> PinTileCommand => new MpAsyncCommand<object>(
             async (args) => {
                 MpPinType pinType = MpPinType.Internal;
                 MpAppendModeType appendType = MpAppendModeType.None;
                 int pin_idx = 0;
                 bool? pin_as_editable = null;
                 MpAvClipTileViewModel ctvm_to_pin = null;
                 if (args is MpAvClipTileViewModel) {
                     // pinning new or query tray tile from overlay button
                     ctvm_to_pin = args as MpAvClipTileViewModel;
                 } else if (args is object[] argParts) {
                     ctvm_to_pin = argParts[0] as MpAvClipTileViewModel;
                     if (argParts[1] is int) {
                         // dnd pin tray drop
                         pin_idx = (int)argParts[1];
                     } else {
                         // pop out
                         pinType = (MpPinType)argParts[1];
                         int cur_pin_idx = PinnedItems.IndexOf(ctvm_to_pin);
                         if (cur_pin_idx >= 0) {
                             // for pop out of already existing items retain current idx
                             pin_idx = cur_pin_idx;
                         }
                         if (argParts.Length > 2 &&
                            argParts[2] is bool make_editable) {
                             pin_as_editable = make_editable;
                         }
                     }
                     if (pinType == MpPinType.Append) {
                         if (argParts.Length <= 2) {
                             appendType = MpAppendModeType.Line;
                         } else {
                             appendType = (MpAppendModeType)argParts[2];
                         }
                     }
                 }
                 pin_idx = Math.Clamp(pin_idx, 0, Math.Max(0, PinnedItems.Count - 1));

                 if (ctvm_to_pin == null || ctvm_to_pin.IsAnyPlaceholder) {
                     MpConsole.WriteTraceLine("PinTile error, tile is either already pinned or placeholder");
                     MpDebug.Break();
                     return;
                 }
                 PinOpCopyItemId = ctvm_to_pin.CopyItemId;

                 string persist_args = pin_as_editable.IsTrue() ? "editable" : null;
                 if (persist_args == null && ctvm_to_pin.IsSubSelectionEnabled) {
                     persist_args = "subselectable";
                 }

                 await ctvm_to_pin.PersistContentStateCommand.ExecuteAsync(persist_args);

                 int ctvm_to_pin_query_idx = -1;
                 MpAvClipTileViewModel query_ctvm_to_pin = QueryItems.FirstOrDefault(x => x.CopyItemId == ctvm_to_pin.CopyItemId);
                 if (query_ctvm_to_pin != null) {
                     // item to pin is query item in current page
                     ctvm_to_pin_query_idx = ctvm_to_pin.QueryOffsetIdx;
                     // create temp tile w/ model ref
                     var temp_ctvm = await CreateClipTileViewModelAsync(ctvm_to_pin.CopyItem);
                     // unload query tile
                     query_ctvm_to_pin.TriggerUnloadedNotification(false, false, true);
                     //await query_ctvm_to_pin.InitializeAsync(null, query_ctvm_to_pin.QueryOffsetIdx);
                     // use temp tile to pin
                     ctvm_to_pin = temp_ctvm;
                 }

                 if (ctvm_to_pin.IsPinned) {
                     // cases:
                     // 1. drop from pin tray (sort)
                     // 2. new duplicate was in pin tray

                     int cur_pin_idx = PinnedItems.IndexOf(ctvm_to_pin);
                     if (cur_pin_idx < 0) {
                         cur_pin_idx = PinnedItems.IndexOf(PinnedItems.FirstOrDefault(x => x.CopyItemId == ctvm_to_pin.CopyItemId));
                         if (cur_pin_idx < 0) {
                             MpDebug.Break("something wrong pinning item...adding instead. check for duplicate content");
                             PinnedItems.Add(ctvm_to_pin);
                         }
                     }
                     PinnedItems.Move(cur_pin_idx, pin_idx);
                 } else if (pin_idx == PinnedItems.Count) {
                     // new item or user pinned query item
                     PinnedItems.Add(ctvm_to_pin);
                 } else {
                     // for drop from external or query tray
                     PinnedItems.Insert(pin_idx, ctvm_to_pin);
                 }

                 if (pinType == MpPinType.Window || pinType == MpPinType.Append) {
                     ctvm_to_pin.OpenPopOutWindow(
                         pinType == MpPinType.Window ?
                            MpAppendModeType.None :
                            appendType);
                     var sw = Stopwatch.StartNew();
                     while (true) {
                         // wait for window to actually open
                         if (PinnedItems.Where(x => x.CopyItemId == PinOpCopyItemId).Any(x => x.IsWindowOpen)) {
                             break;
                         }
                         if (sw.ElapsedMilliseconds > 5_000) {
                             // timeout
                             MpDebug.Break("Tile popout time out reached");
                             break;
                         }
                         await Task.Delay(100);
                     }
                 } else {
                     // reset pinned item size (may have been missed if init was before adding to 1 of the item collections)
                     ctvm_to_pin.ResetTileSizeToDefaultCommand.Execute(null);
                 }

                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 ctvm_to_pin.OnPropertyChanged(nameof(ctvm_to_pin.IsPinned));
                 ctvm_to_pin.OnPropertyChanged(nameof(ctvm_to_pin.IsPlaceholder));
                 ctvm_to_pin.OnPropertyChanged(nameof(ctvm_to_pin.IsPinPlaceholder));

                 if (query_ctvm_to_pin != null &&
                    ctvm_to_pin_query_idx >= 0
                    ) {
                     // re-init queyr item to become pin-placeholder
                     await query_ctvm_to_pin.InitializeAsync(ctvm_to_pin.CopyItem, ctvm_to_pin_query_idx);
                 }
                 await Task.Delay(200);
                 if (SelectedItem == null ||
                    SelectedItem == query_ctvm_to_pin) {
                     // only select if no sel or pin tile source was selected
                     ctvm_to_pin.IsSelected = true;
                 }

                 PinOpCopyItemId = -1;

                 if (ctvm_to_pin_query_idx >= 0) {
                     while (!QueryCommand.CanExecute(string.Empty)) {
                         await Task.Delay(100);
                     }
                     QueryCommand.Execute(string.Empty);
                 }

                 OnPropertyChanged(nameof(Items));
                 OnPropertyChanged(nameof(PinnedItems));
                 OnPropertyChanged(nameof(MinPinTrayScreenWidth));
                 OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                 OnPropertyChanged(nameof(ObservedQueryTrayScreenWidth));
                 OnPropertyChanged(nameof(ObservedQueryTrayScreenHeight));
                 UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync(this);
             },
            (args) => args != null);

        public MpIAsyncCommand<object> UnpinTileCommand => new MpAsyncCommand<object>(
             async (args) => {

                 MpAvClipTileViewModel pin_placeholder_ctvm = null;
                 MpAvClipTileViewModel unpinned_ctvm = null;

                 if (args is MpAvClipTileViewModel arg_ctvm) {
                     if (arg_ctvm.IsPinPlaceholder) {
                         // unpinning from query tray pin placeholder (lbi double click)
                         pin_placeholder_ctvm = arg_ctvm;
                         unpinned_ctvm = pin_placeholder_ctvm.PinnedItemForThisPlaceholder;
                     } else {
                         // unpinning from corner button or popout closed
                         unpinned_ctvm = arg_ctvm;
                         pin_placeholder_ctvm = unpinned_ctvm.PlaceholderForThisPinnedItem;
                     }
                 }
                 if (unpinned_ctvm == null) {
                     MpDebug.Break($"No pin tile found for placeholder ciid {pin_placeholder_ctvm.PinPlaceholderCopyItemId} at queryIdx {pin_placeholder_ctvm.QueryOffsetIdx}");
                     return;
                 }

                 unpinned_ctvm.StoreSelectionStateCommand.Execute(null);
                 await unpinned_ctvm.PersistContentStateCommand.ExecuteAsync(null);

                 int unpinned_ciid = unpinned_ctvm.CopyItemId;
                 int unpinned_ctvm_idx = PinnedItems.IndexOf(unpinned_ctvm);


                 PinOpCopyItemId = unpinned_ctvm.CopyItemId;

                 PinnedItems.Remove(unpinned_ctvm);
                 if (unpinned_ctvm.IsWindowOpen) {
                     bool was_closed = await unpinned_ctvm.ClosePopoutAsync();
                     if (!was_closed) {
                         // cancel unpin (must be appending)
                         return;
                     }
                 }
                 unpinned_ctvm.IsContentReadOnly = true;

                 //if (!IsAnyTilePinned) {
                 //    ObservedPinTrayScreenWidth = 0;
                 //}

                 OnPropertyChanged(nameof(PinnedItems));
                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 OnPropertyChanged(nameof(MinPinTrayScreenWidth));
                 OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                 OnPropertyChanged(nameof(ObservedQueryTrayScreenWidth));
                 OnPropertyChanged(nameof(ObservedQueryTrayScreenHeight));

                 ClearAllSelection(false);
                 MpAvClipTileViewModel to_select_ctvm = null;

                 if (pin_placeholder_ctvm == null) {
                     // unpinned tile no longer in any view, remove its persistent props 
                     unpinned_ctvm.TriggerUnloadedNotification(false);
                 } else {
                     // unpinned tile is part of current query page, load into pin placeholder
                     int pin_placeholder_query_idx = pin_placeholder_ctvm.QueryOffsetIdx;

                     await pin_placeholder_ctvm.InitializeAsync(null, pin_placeholder_query_idx);

                     await pin_placeholder_ctvm.InitializeAsync(
                         unpinned_ctvm.CopyItem,
                         pin_placeholder_query_idx);

                     to_select_ctvm = pin_placeholder_ctvm;
                 }
                 int to_select_ciid = to_select_ctvm == null ? 0 : to_select_ctvm.CopyItemId;
                 while (!QueryCommand.CanExecute(string.Empty)) { await Task.Delay(100); }
                 QueryCommand.Execute(string.Empty);
                 to_select_ctvm = to_select_ciid == 0 ? null : AllItems.FirstOrDefault(x => x.CopyItemId == to_select_ciid);

                 if (to_select_ctvm == null) {
                     // unpinned tile not in query page, try to select next pinned tile
                     if (IsPinTrayEmpty) {
                         var sw = Stopwatch.StartNew();
                         while (IsAnyBusy) {
                             // query returns before sub tasks complete and updated offsets are needed
                             await Task.Delay(100);
                             if (sw.ElapsedMilliseconds > 5_000) {
                                 MpDebug.Break($"Unpin timeout");
                                 break;
                             }
                         }
                         // select left most visible tile if pin tray empty
                         to_select_ctvm =
                            VisibleQueryItems
                            .Where(x => !x.IsPinPlaceholder)
                            .AggregateOrDefault((a, b) => a.QueryOffsetIdx < b.QueryOffsetIdx ? a : b);
                     } else {
                         // prefer select prev pinned neighbor tile
                         to_select_ctvm =
                            InternalPinnedItems
                            .AggregateOrDefault((a, b) => unpinned_ctvm_idx - a.ItemIdx < unpinned_ctvm_idx - b.ItemIdx ? a : b);
                     }
                 }

                 if (to_select_ctvm == null) {
                     // should probably not happen or will have no effect (empty query) but in case
                     ResetAllSelection(false);
                 } else {
                     to_select_ctvm.IsSelected = true;
                 }
                 PinOpCopyItemId = -1;
                 UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync(this);
             },
            (args) => {
                if (args is MpAvClipTileViewModel ctvm) {
                    return ctvm.IsPinned || ctvm.IsPinPlaceholder;
                }
                return false;
            });

        public MpIAsyncCommand ToggleSelectedTileIsPinnedCommand => new MpAsyncCommand(
            async () => {
                await ToggleTileIsPinnedCommand.ExecuteAsync(SelectedItem);
            });

        public MpIAsyncCommand<object> ToggleTileIsPinnedCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpAvClipTileViewModel pctvm = null;
                if (args is MpAvClipTileViewModel) {
                    pctvm = args as MpAvClipTileViewModel;
                } else if (args is object[] argParts) {
                    pctvm = argParts[0] as MpAvClipTileViewModel;
                }

                if (pctvm.IsPinned) {
                    await UnpinTileCommand.ExecuteAsync(args);
                    return;
                } else {
                    await PinTileCommand.ExecuteAsync(args);
                }
            },
            (args) => {
                return args != null;
            });
        public MpIAsyncCommand UnpinAllCommand => new MpAsyncCommand(
            async () => {
                //int pin_count = PinnedItems.Count;
                //while (pin_count > 0) {
                //    var to_unpin_ctvm = PinnedItems[--pin_count];
                //    if (to_unpin_ctvm.IsWindowOpen ||
                //        to_unpin_ctvm.IsAppendNotifier) {
                //        continue;
                //    }
                //    await UnpinTileCommand.ExecuteAsync(to_unpin_ctvm);
                //}
                var to_unpin_ciidl = InternalPinnedItems.Select(x => x.CopyItemId).ToList();
                for (int i = 0; i < to_unpin_ciidl.Count; i++) {
                    if (PinnedItems.FirstOrDefault(x => x.CopyItemId == to_unpin_ciidl[i]) is { } to_unpin_ctvm) {
                        PinnedItems.Remove(to_unpin_ctvm);
                    }
                }
                while (!QueryCommand.CanExecute(string.Empty)) {
                    await Task.Delay(50);
                }
                await QueryCommand.ExecuteAsync(string.Empty);
            });

        public ICommand OpenSelectedTileInWindowCommand => new MpCommand(
            () => {
                SelectedItem.PinToPopoutWindowCommand.Execute(null);
            }, () => {
                return SelectedItem != null && !SelectedItem.IsWindowOpen;
            });
        public ICommand DuplicateSelectedClipsCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;
                var avdo = SelectedItem.CopyItem.ToAvDataObject();
                await BuildFromDataObjectAsync(avdo, true);

                IsBusy = false;
            }, () => SelectedItem != null && SelectedItem.IsContentReadOnly);

        public MpIAsyncCommand AddNewItemsCommand => new MpAsyncCommand(
            async () => {
                var sw = Stopwatch.StartNew();
                while (IsPinTrayBusy) {
                    if (sw.ElapsedMilliseconds > 5_000) {
                        MpConsole.WriteLine($"AddNewItems cmd timeout, adding anyway");
                        IsPinTrayBusy = false;
                        break;
                    }
                }
                if (!PendingNewModels.Any()) {
                    // nothing to add (at least now)
                    return;
                }
                IsPinTrayBusy = true;

                int selectedId = -1;
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLocked && SelectedItem != null) {
                    selectedId = SelectedItem.CopyItemId;
                }

                // NOTE only adding most recent to not clog up pin tray, all badge will convey other added items
                var most_recent_pending_ci = PendingNewModels.OrderByDescending(x => x.CopyDateTime).FirstOrDefault();
                if (PendingNewModels.Where(x => x.Id != most_recent_pending_ci.Id) is IEnumerable<MpCopyItem> other_pending_cil &&
                    other_pending_cil.Any() &&
                    MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == MpTag.AllTagId) is MpAvTagTileViewModel all_ttvm &&
                    other_pending_cil.Where(x => !all_ttvm.CopyItemIdsNeedingView.Contains(x.Id)).Select(x => x.Id) is IEnumerable<int> other_pending_ciids_neeeding_view) {
                    // NOTE this worksaround 'All' tag being a pseudo link tag to allow badge count w/o 
                    // interrupting linking logic
                    all_ttvm.CopyItemIdsNeedingView.AddRange(other_pending_ciids_neeeding_view);
                }
                if (most_recent_pending_ci == null || most_recent_pending_ci.Id <= 0) {
                    // i think this is how the phantom tiles are getting added, 
                    IsPinTrayBusy = false;
                    return;
                }
                MpAvClipTileViewModel nctvm = await CreateOrRetrieveClipTileViewModelAsync(most_recent_pending_ci);
                await PinTileCommand.ExecuteAsync(nctvm);

                PendingNewModels.Clear();
                if (selectedId >= 0) {
                    var selectedVm = AllItems.FirstOrDefault(x => x.CopyItemId == selectedId);
                    if (selectedVm != null) {
                        selectedVm.IsSelected = true;
                    }
                }
                IsPinTrayBusy = false;
                //using tray scroll changed so tile drop behaviors update their drop rects
            },
            () => {
                if (PendingNewModels.Count == 0) {
                    return false;
                }
                if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    return true;
                }
                return false;
            });
        private int GetEdgeQueryIdxByDir(int qidx, int dir) {
            if (LayoutType == MpClipTrayLayoutType.Stack) {
                // no change for stack
                return qidx;
            }

            // adjust loadOffset to be at beginning of fixed axis
            // if middle item is selected as anchor it'll ignore leading items
            // when refreshing so move it to the first
            var (row, col) = GetGridLocFromQueryIdx(qidx);
            int q_fixed_idx = ListOrientation == Orientation.Horizontal ? col : row;
            int edge_diff = dir > 0 ? CurGridFixedCount - q_fixed_idx - 1 : q_fixed_idx;
            return Math.Clamp(qidx + edge_diff, 0, MaxClipTrayQueryIdx);
        }
        private (int, int) GetLoadRangeFromQueryIdx(int qidx, int dir, int desiredCount) {
            // get first idx by dir from given idx
            int firstIdx = qidx + dir;
            if (firstIdx < 0 || firstIdx >= MaxClipTrayQueryIdx) {
                // if first load idx is out-of-range theres nothing to load
                return default;
            }
            // get last desired idx by dir clamped to range
            int lastIdx = Math.Clamp(firstIdx + ((desiredCount - 1) * dir), 0, MaxClipTrayQueryIdx);
            //clamp result so lastIdx is first/last of row/col 
            // to keep load even
            lastIdx = GetEdgeQueryIdxByDir(lastIdx, dir);
            int count = Math.Abs(lastIdx - firstIdx);
            return (Math.Min(firstIdx, lastIdx), count == 0 ? 0 : count + 1);
        }

        private bool CanCheckLoadMore() {
            if (IsThumbDragging ||
                //IsAnyBusy ||
                IsAnyResizing ||
                //IsForcingScroll ||
                //_isLayoutChanging ||
                //_isMainWindowOrientationChanging ||
                IsQueryTrayEmpty ||
                MpAvMainWindowViewModel.Instance.IsResizing ||
                // Mp.Services.Query.TotalAvailableItemsInQuery == 0 ||
                //!MpAvTagTrayViewModel.Instance.IsAnyTagActive ||
                Items.Count <= RemainingItemsCountThreshold) {
                //MpConsole.WriteLine($"Can't check load more", true);
                //MpConsole.WriteLine($"IsThumbDragging: {IsThumbDragging}");
                //MpConsole.WriteLine($"IsAnyBusy: {IsAnyBusy}");
                //MpConsole.WriteLine($"MpAvMainWindowViewModel.Instance.IsResizing: {MpAvMainWindowViewModel.Instance.IsResizing}");
                //MpConsole.WriteLine($"Items.Count <= RemainingItemsCountThreshold: {Items.Count} <= {RemainingItemsCountThreshold}");
                return false;
            }
            return true;
        }
        private bool CheckLoadMore(bool isZoomCheck = false) {
            if (!CanCheckLoadMore()) {
                return false;
            }

            IOrderedEnumerable<MpAvClipTileViewModel> vis_lbil = QueryItems
                            .Where(x => x.IsAnyQueryCornerVisible)
                            .OrderBy(x => x.QueryOffsetIdx);

            if (!vis_lbil.Any()) {
                // no visible items, in place requery
                if (Mp.Services.Query.TotalAvailableItemsInQuery < DefaultLoadCount && LayoutType == MpClipTrayLayoutType.Grid) {
                    // HACK when items less than 1 page requery will clear the tray for some reason, this prevents
                    return false;
                }
                int new_head_idx = FindJumpTileIdx(ScrollOffsetX, ScrollOffsetY, out _);
                if (new_head_idx < 0) {
                    return false;
                }
                bool can_query = QueryCommand.CanExecute(ScrollOffset);
                MpConsole.WriteLine($"Empty query screen detected. Can fix (iprq): {can_query}");
                if (can_query) {
                    QueryCommand.Execute(ScrollOffset);
                    return true;
                }
                return false;
            }

            var scroll_diff = ScrollOffset - LastScrollOffset;
            int scroll_dir =
                Math.Abs(scroll_diff.X).IsFuzzyZero() ?
                    Math.Abs(scroll_diff.Y).IsFuzzyZero() ?
                        0 :
                        scroll_diff.Y > 0 ? 1 : -1 :
                scroll_diff.X > 0 ? 1 : -1;

            (int, int) head_range = default, tail_range = default;

            if (scroll_dir <= 0) {
                // decreasing query qidx (or no delta)
                int head_remaining = vis_lbil.First().QueryOffsetIdx - HeadQueryIdx;
                int head_to_load = RemainingItemsCountThreshold - head_remaining;
                if (head_to_load > 0) {
                    head_range = GetLoadRangeFromQueryIdx(HeadQueryIdx, -1, LoadMorePageSize);
                }
            }
            if (scroll_dir >= 0) {
                // increasing query qidx (or no delta)
                int tail_remaining = TailQueryIdx - vis_lbil.Last().QueryOffsetIdx;
                int tail_to_load = RemainingItemsCountThreshold - tail_remaining;
                if (tail_to_load > 0) {
                    tail_range = GetLoadRangeFromQueryIdx(TailQueryIdx, 1, LoadMorePageSize);
                }
            }
            if (head_range.Item2 == 0 && tail_range.Item2 == 0) {
                return false;
            }
            if (!QueryCommand.CanExecute(null)) {
                return true;
            }
            List<List<int>> ranges = new List<List<int>>();
            if (head_range.Item2 > 0) {
                ranges.Add(Enumerable.Range(head_range.Item1, head_range.Item2).ToList());
                MpConsole.WriteLine($"Load More HEAD Qidxs: {string.Join(",", ranges[0])}");
            }
            if (tail_range.Item2 > 0) {
                ranges.Add(Enumerable.Range(tail_range.Item1, tail_range.Item2).ToList());
                MpConsole.WriteLine($"Load More TAIL Qidxs: {string.Join(",", ranges[ranges.Count - 1])}");
            }
            QueryCommand.Execute(ranges);
            return true;
        }

        private MpPoint AdjustTileLocationToScrollOffset(MpPoint loc) {
            if (IsGridLayout) {
                // NOTE if list is somewhat scrolled into and then toggled to grid
                // the fixed axis will still be scrolled to extent, need to reset to home
                if (ListOrientation == Orientation.Horizontal) {
                    loc.X = 0;
                } else {
                    loc.Y = 0;
                }
            }
            return loc;
        }

        private async Task PerformQueryAsync(object offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) {
            //MpConsole.WriteLine($"Query called. Arg: '{offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg}'");
            Dispatcher.UIThread.VerifyAccess();

            var sw = new Stopwatch();
            sw.Start();

            #region Gather Args

            IsSubQuerying = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg != null;
            bool isRequery = !IsSubQuerying;
            bool isScrollJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is MpPoint;
            bool isOffsetJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is int;
            bool isLoadMore = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is bool;
            bool isLoadRange = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is List<int>;
            bool isInPlaceRequery = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is string;

            int loadOffsetIdx = 0;
            int loadCount = 0;

            bool isLoadMoreTail = false;

            MpPoint newScrollOffset = default;

            #endregion

            #region Reject Empty Query

            bool is_empty_query =
                !MpAvTagTrayViewModel.Instance.IsAnyTagActive &&
                Mp.Services.Query.Infos.All(x => string.IsNullOrEmpty(x.MatchValue));

            if (is_empty_query) {
                // cases:
                // -startup clipboard item added at cap and oldest linked to trash (triggers iprq)
                // -after selected tag delete and parent is group tag
                Items.ForEach(x => x.TriggerUnloadedNotification(true, true));
                await UpdateEmptyPropertiesAsync();
                if (isRequery) {
                    MpMessenger.SendGlobal(MpMessageType.RequeryCompleted);
                }
                MpMessenger.SendGlobal(MpMessageType.QueryCompleted);
                return;
            }

            #endregion

            #region TotalCount Query & Offset Calc

            IsInPlaceRequerying = isInPlaceRequery;
            IsBusy = !isLoadMore && !isLoadRange && !isInPlaceRequery && !is_empty_query;
            IsQuerying = true;

            if (IsSubQuerying) {
                // sub-query of visual, data-specific or incremental qidx 

                if (isOffsetJump) {
                    // sub-query to data-specific (query Idx) qidx (for anchor query)

                    loadOffsetIdx = (int)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg;
                    var loadTileRect = GetQueryTileRect(loadOffsetIdx);
                    newScrollOffset = AdjustTileLocationToScrollOffset(loadTileRect.Location);
                } else if (isScrollJump) {
                    // sub-query to visual (scroll position) qidx 

                    newScrollOffset = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg as MpPoint;
                    loadOffsetIdx = FindJumpTileIdx(newScrollOffset.X, newScrollOffset.Y, out MpRect offsetTileRect);
                    newScrollOffset = offsetTileRect.Location;
                } else if (isLoadMore) {
                    // sub-query either forward (true) or backward (false) based on current qidx

                    loadCount = LoadMorePageSize;

                    if ((bool)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) {
                        //load More to tail
                        isLoadMoreTail = true;
                        loadOffsetIdx = TailQueryIdx + 1;
                        if (loadOffsetIdx > MaxClipTrayQueryIdx) {
                            IsQuerying = false;
                            IsBusy = false;
                            return;
                        }
                    } else {
                        //load more to head
                        isLoadMoreTail = false;
                        loadOffsetIdx = HeadQueryIdx - loadCount;
                        if (loadOffsetIdx < MinClipTrayQueryIdx) {
                            IsQuerying = false;
                            IsBusy = false;
                            return;
                        }
                    }
                } else if (isInPlaceRequery) {
                    // total count query then fetch and set scroll to current state
                    newScrollOffset = ScrollOffset;
                    loadOffsetIdx = HeadQueryIdx;
                } else if (isLoadRange) {
                    // recycle items to head, tail or both (both when zoom changes or is pointless not sure)
                    isLoadMore = true;
                    loadOffsetIdx = ((List<int>)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg).First();
                    loadCount = ((List<int>)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg).Count;
                    isLoadMoreTail = loadOffsetIdx > TailQueryIdx;
                }
            } else {
                IsRequerying = true;
                // new query all content and offsets are re-initialized
                ClearQuerySelection();

                // trigger unload event to wipe js eval's that maybe pending 
                UnpinnedQueryItems.ForEach(x => x.TriggerUnloadedNotification(false));

                MpAvPersistentClipTilePropertiesHelper.ClearPersistentQuerySizes();
            }

            if (isRequery || isInPlaceRequery) {
                await Mp.Services.Query.QueryForTotalCountAsync(true);

                SetTotalTileSize();

                OnPropertyChanged(nameof(Mp.Services.Query.TotalAvailableItemsInQuery));
                OnPropertyChanged(nameof(QueryTrayTotalWidth));
                OnPropertyChanged(nameof(MaxScrollOffsetX));
                OnPropertyChanged(nameof(MaxScrollOffsetY));
                OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
                OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));
            }

            loadOffsetIdx = Math.Max(0, loadOffsetIdx);

            if (loadCount == 0) {
                // is not an LoadMore Query
                loadCount = Math.Min(DefaultLoadCount, Mp.Services.Query.TotalAvailableItemsInQuery);
            } else if (loadOffsetIdx < 0) {
                loadCount = 0;
            }

            if (loadOffsetIdx + loadCount > MaxClipTrayQueryIdx) {
                // clamp load qidx to max query total count
                //loadOffsetIdx = MaxLoadQueryIdx;

            }

            #endregion

            #region Normalize Items To Load Count
            // Occurs when LayoutType changes and default load counts differ

            List<int> fetchQueryIdxList = Enumerable.Range(loadOffsetIdx, loadCount).ToList();
            if (!isLoadMore) {
                // Cleanup Tray item count depending on last query 
                int itemCountDiff = Items.Count - fetchQueryIdxList.Count;
                if (itemCountDiff > 0) {
                    while (itemCountDiff > 0) {
                        // keep unneeded items as placeholders
                        Items[--itemCountDiff].TriggerUnloadedNotification(false, false);
                    }
                } else if (itemCountDiff < 0) {
                    while (itemCountDiff < 0) {
                        var ctvm = await CreateClipTileViewModelAsync(null);
                        Items.Add(ctvm);
                        itemCountDiff++;
                    }
                }
            }
            #endregion

            #region Fetch Data & Init Items

            var ciidl = await Mp.Services.Query.FetchPageIdsAsync(loadOffsetIdx, loadCount);
            if (isLoadMore && !isLoadMoreTail) {
                // when loading to head reverse order
                ciidl.Reverse();
                fetchQueryIdxList.Reverse();
            }
            //since tiles watch for their model changes, remove any items

            var recycle_idxs = GetLoadItemIdxs(isLoadMore ? isLoadMoreTail : null, ciidl.Count);
            int dir = isLoadMoreTail ? 1 : -1;

            for (int i = 0; i < recycle_idxs.Count; i++) {
                MpAvClipTileViewModel cur_ctvm = Items[recycle_idxs[i]];

                if (cur_ctvm.IsSelected) {
                    StoreSelectionState(cur_ctvm);
                    cur_ctvm.ClearSelection(false);
                }
                bool needsRestore = false;
                if (IsSubQuerying && MpAvPersistentClipTilePropertiesHelper.GetPersistentSelectedItemId() == ciidl[i]) {
                    needsRestore = true;
                }

                await cur_ctvm.InitializeAsync(await MpDataModelProvider.GetItemAsync<MpCopyItem>(ciidl[i]), fetchQueryIdxList[i], needsRestore);
            }

            #endregion

            #region Finalize State & Measurements

            OnPropertyChanged(nameof(Mp.Services.Query.TotalAvailableItemsInQuery));
            OnPropertyChanged(nameof(QueryTrayTotalTileWidth));
            OnPropertyChanged(nameof(QueryTrayTotalWidth));
            OnPropertyChanged(nameof(MaxScrollOffsetX));
            OnPropertyChanged(nameof(MaxScrollOffsetY));
            OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));

            if (SparseLoadMoreRemaining > 0) {
                MpDebug.Assert(isLoadMore, $"Load more locked out w/ {SparseLoadMoreRemaining} loads remaining");
                if (isLoadMore) {
                    // treat sparse loads as 1 query but only finalize after last
                    return;
                }

                // BUG Sparse count should only have paramValue during a loadmore
                // not sure how this doesn't get decremented to 0 but it'll block 
                // any subsequent query from completing if not done
                SparseLoadMoreRemaining = 0;
            }

            IsBusy = false;
            IsQuerying = false;

            OnPropertyChanged(nameof(IsAnyBusy));
            OnPropertyChanged(nameof(IsQueryEmpty));

            sw.Stop();
            MpConsole.WriteLine($"Update tray of {recycle_idxs.Count} items took: " + sw.ElapsedMilliseconds);

            if (isRequery) {
                ForceScrollOffset(MpPoint.Zero, "requery");
                IsRequerying = false;
                MpMessenger.SendGlobal(MpMessageType.RequeryCompleted);

                if (SelectedItem == null &&
                    !MpAvPersistentClipTilePropertiesHelper.HasPersistentSelection() &&
                    Mp.Services.Query.TotalAvailableItemsInQuery > 0) {
                    Dispatcher.UIThread.Post(async () => {
                        while (IsAnyBusy) {
                            await Task.Delay(100);
                        }
                        ResetAllSelection();
                    });
                }
            } else {
                if (isOffsetJump || isScrollJump || isInPlaceRequery) {
                    ForceScrollOffset(newScrollOffset, "iprq");
                    if (isInPlaceRequery) {
                        MpMessenger.SendGlobal(MpMessageType.InPlaceRequeryCompleted);
                    } else {
                        MpMessenger.SendGlobal(MpMessageType.JumpToIdxCompleted);
                    }

                } else {
                    if (isLoadMore) {
                        //if (CheckLoadMore()) {
                        //    return;
                        //}
                    } else {
                        RefreshQueryTrayLayout();
                    }
                }
            }

            if (isRequery || isInPlaceRequery) {
                UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();
            }

            Dispatcher.UIThread.Post(async () => {
                while (IsAnyBusy) {
                    await Task.Delay(100);
                }
                if (!isLoadMore) {
                    // wait for all loading to finish before unflagging iprq busy
                    // so busy spinners stay hidden
                    IsInPlaceRequerying = false;
                    MpMessenger.SendGlobal(MpMessageType.QueryCompleted);
                }

                ValidateQueryTray();
            });
            #endregion
        }

        #region Repeater Test
        public Vector ScrollVector { get; set; }
        List<MpCopyItem> _allQueryModels = new List<MpCopyItem>();

        private async Task RepeaterQueryAsync(object offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) {
            if (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg == null) {
                IsRequerying = true;
                await Mp.Services.Query.QueryForTotalCountAsync(true);
                IsRequerying = false;
                Items.ForEach(x => x.TriggerUnloadedNotification(true));
                ScrollVector = new Vector();
            } else if (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is not Vector) {
                return;
            }
            IsQuerying = true;
            int sidx = GetQueryIdxFromScrollOffset(ScrollVector.X, ScrollVector.Y);
            int eidx = GetQueryIdxFromScrollOffset(ScrollVector.X + ObservedQueryTrayScreenWidth, ScrollVector.Y + ObservedQueryTrayScreenHeight);
            int actual_eidx = eidx;
            eidx += (eidx - sidx) * 2;
            eidx = Math.Min(Mp.Services.Query.TotalAvailableItemsInQuery - 1, eidx);

            MpConsole.WriteLine($"sidx: {sidx} scr eidx: {actual_eidx} load eidx: {eidx}");
            int to_add = eidx - Items.Count;
            while (to_add > 0) {
                Items.Add(new MpAvClipTileViewModel(this));
                to_add--;
            }
            var cil = await Mp.Services.Query.FetchPageAsync(sidx, eidx);
            for (int i = sidx; i < eidx; i++) {
                if (IsRequerying) {
                    return;
                }
                await Items[i].InitializeAsync(cil[i - sidx], i);
            }

            IsQuerying = false;
            IsRequerying = false;
        }

        #endregion

        public MpIAsyncCommand<object> QueryCommand => new MpAsyncCommand<object>(
            async (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) => {
                if (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is not List<List<int>> ranges) {
                    await PerformQueryAsync(offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg);
                    return;
                }
                SparseLoadMoreRemaining = ranges.Count;
                while (SparseLoadMoreRemaining > 0) {
                    SparseLoadMoreRemaining--;
                    await PerformQueryAsync(ranges[SparseLoadMoreRemaining]);
                }
            },
            (offsetIdx_Or_ScrollOffset_Arg) => {
                return
                    !IsRequerying &&
                    !IsQuerying;
            });

        private List<int> GetLoadItemIdxs(bool? isLoadMoreTail, int count) {
            return Items
                .OrderByDescending(x => x.GetRecyclePriority(isLoadMoreTail))
                    .Take(count)
                    .Select(x => Items.IndexOf(x))
                    .ToList();
        }

        public MpIAsyncCommand CopySelectedClipFromShortcutCommand => new MpAsyncCommand(
            async () => {
                await SelectedItem.CopyToClipboardCommand.ExecuteAsync();
            },
            () => {
                bool canCopy =
                    SelectedItem != null &&
                    SelectedItem.IsFocusWithin &&
                    SelectedItem.IsHostWindowActive;
                MpConsole.WriteLine("CopySelectedClipFromShortcutCommand CanExecute: " + canCopy);
                if (!canCopy) {
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));
                    MpConsole.WriteLine("IsFocusWithin: " + (SelectedItem == null ? "NO" : SelectedItem.IsFocusWithin.ToString()));
                    MpConsole.WriteLine("IsHostWindowActive: " + (SelectedItem == null ? "NO" : SelectedItem.IsHostWindowActive.ToString()));
                }
                return canCopy;
            });


        public ICommand CutSelectionFromContextMenuCommand => new MpCommand<object>(
            (args) => {
                CutOrCopySelection(true);
            },
            (args) => {
                return SelectedItem != null && SelectedItem.IsSubSelectionEnabled;
            });

        public ICommand CopySelectionFromContextMenuCommand => new MpCommand<object>(
            (args) => {
                CutOrCopySelection(false);
            },
            (args) => {
                return SelectedItem != null;
            });

        public ICommand PasteSelectedClipTileFromShortcutCommand => new MpCommand<object>(
            (args) => {
                bool fromEditorButton = false;
                if (args is bool) {
                    fromEditorButton = (bool)args;
                }
                PasteClipTileAsync(SelectedItem, MpPasteSourceType.Shortcut).FireAndForgetSafeAsync();
            },
            (args) => {

                bool can_paste =
                    SelectedItem != null &&
                    SelectedItem.IsHostWindowActive;// &&
                                                    //!MpAvMainWindowViewModel.Instance.IsAnyMainWindowTextBoxFocused &&
                                                    //!MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen &&
                                                    //!IsAnyEditingClipTile &&
                                                    //!IsAnyEditingClipTitle &&;

                if (!can_paste &&
                SelectedItem == null &&
                    AllItems
                    .Where(x => x.LastDeselectedDateTime.HasValue)
                    .OrderByDescending(x => x.LastDeselectedDateTime.Value)
                    .FirstOrDefault() is MpAvClipTileViewModel last_deselected_ctvm) {
                    // HACK since shortcut is ctrl enter
                    last_deselected_ctvm.IsSelected = true;
                    if (SelectedItem == last_deselected_ctvm) {
                        MpConsole.WriteLine($"ctrl+enter hack successfull for tile '{last_deselected_ctvm}'");
                        can_paste = true;
                    } else {
                        MpConsole.WriteLine($"ctrl+enter hack failed for tile '{last_deselected_ctvm}'");
                    }
                }
                MpConsole.WriteLine("PasteSelectedClipTileFromShortcutCommand CanExecute: " + can_paste);
                if (!can_paste) {
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));
                    if (SelectedItem != null) {

                        MpConsole.WriteLine("IsHostWindowActive: " + SelectedItem.IsHostWindowActive);
                    }
                }
                return can_paste;

            });
        public ICommand PasteSelectedClipTileFromContextMenuCommand => new MpCommand<object>(
            (args) => {
                bool fromEditorButton = false;
                if (args is bool) {
                    fromEditorButton = (bool)args;
                }
                PasteClipTileAsync(SelectedItem, MpPasteSourceType.ContextMenu).FireAndForgetSafeAsync();
            },
            (args) => {
                return SelectedItem != null;
            });

        public ICommand PasteHereFromContextMenuCommand => new MpCommand<object>(
            (args) => {
                Mp.Services.KeyStrokeSimulator
                .SimulateKeyStrokeSequence(Mp.Services.PlatformShorcuts.PasteKeys);
            },
            (args) => {
                return SelectedItem != null && SelectedItem.IsSubSelectionEnabled;
            });

        public ICommand PasteFromClipTilePasteButtonCommand => new MpCommand<object>(
            (args) => {
                PasteClipTileAsync(args as MpAvClipTileViewModel, MpPasteSourceType.PasteButton).FireAndForgetSafeAsync();
            },
            (args) => {
                if (args is MpAvClipTileViewModel ctvm) {
                    return true;
                }
                return false;
            });

        public ICommand PasteCurrentClipboardIntoSelectedTileCommand => new MpAsyncCommand(
            async () => {
                while (IsAddingClipboardItem) {
                    // wait in case tray is still processing the data
                    await Task.Delay(100);
                }

                // NOTE even though re-creating paste object here the copy item
                // builder should recognize it as a duplicate and use original (just created)
                var mpdo = await Mp.Services.DataObjectTools.ReadClipboardAsync(false);

                SelectedItem.RequestPastePortableDataObject(mpdo);
            }, () => {
                return SelectedItem != null && !SelectedItem.IsAnyPlaceholder;
            });

        public ICommand PasteCopyItemByIdFromShortcutCommand => new MpAsyncCommand<object>(
            async (args) => {
                int copyItemId = 0;
                if (args is int ciid) {
                    copyItemId = ciid;
                } else if (args is string ciidStr) {
                    try {
                        copyItemId = int.Parse(ciidStr);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error pasting copyitem by id. cannot parse id from string '{ciidStr}'.", ex);
                    }
                }
                if (copyItemId == 0) {
                    return;
                }

                MpAvClipTileViewModel ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == copyItemId);
                if (ctvm == null) {
                    var ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(copyItemId);
                    if (ci == null) {
                        // if this is coming from a shortcut, shortcut should have been deleted
                        // otherwise huh?
                        MpDebug.Break();
                        return;
                    }

                    ctvm = await CreateClipTileViewModelAsync(ci);
                    if (ctvm.CopyItemType == MpCopyItemType.Text &&
                        MpAvTemplateModelHelper.Instance.HasHtmlTemplate(ctvm.CopyItemData)) {
                        // NOTE this matcher may have false positives but not miss item's with templates

                        // TODO it maybe better to just paste the contents and not get new values
                        // TODO2 would probably be better to show this in a system tray window like append but not sure if 
                        // this even a good idea yet
                        PinTileCommand.Execute(new object[] { ctvm, MpPinType.Window });

                        // don't actually paste templates from hot key needs to happen from toolbar button
                        return;
                    }
                }
                PasteClipTileAsync(ctvm, MpPasteSourceType.Hotkey).FireAndForgetSafeAsync(this);
            },
            (args) => {
                return args is int || args is string;
            });

        public ICommand RemoveItemCommand => new MpCommand<object>(
            (args) => {
                MpAvClipTileViewModel ctvm_to_remove = args as MpAvClipTileViewModel;
                if (ctvm_to_remove is null && args is int ciid) {
                    ctvm_to_remove = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                }
                if (ctvm_to_remove == null) {
                    return;
                }

                if (ctvm_to_remove.IsPinned) {
                    if (ctvm_to_remove.PlaceholderForThisPinnedItem != null &&
                        ctvm_to_remove.PlaceholderForThisPinnedItem.QueryOffsetIdx is int ppqidx) {
                        Items.Remove(ctvm_to_remove.PlaceholderForThisPinnedItem);
                        Items.Where(x => x.QueryOffsetIdx > ppqidx).ForEach(x => x.UpdateQueryOffset(x.QueryOffsetIdx - 1));
                        Items.Add(new MpAvClipTileViewModel(this));
                    }
                    PinnedItems.Remove(ctvm_to_remove);
                } else {
                    int qidx = ctvm_to_remove.QueryOffsetIdx;
                    Items.Remove(ctvm_to_remove);
                    Items.Where(x => x.QueryOffsetIdx > qidx).ForEach(x => x.UpdateQueryOffset(x.QueryOffsetIdx - 1));
                    Items.Add(new MpAvClipTileViewModel(this));
                }
            });
        public ICommand RestoreSelectedClipCommand => new MpAsyncCommand(
            async () => {
                int restore_ciid = SelectedItem.CopyItemId;
                if (MpAvAccountTools.Instance.IsContentAddPausedByAccount) {
                    // user at cblock
                    ProcessAccountCapsAsync(MpAccountCapCheckType.RestoreBlock, restore_ciid).FireAndForgetSafeAsync(this);
                    return;
                }
                RemoveItemCommand.Execute(restore_ciid);

                // unlink from trash tag 
                await MpAvTagTrayViewModel.Instance.TrashTagViewModel
                .UnlinkCopyItemCommand.ExecuteAsync(restore_ciid);

            },
            () => {
                bool can_restore =
                    SelectedItem != null &&
                    SelectedItem.IsTrashed;

                if (!can_restore) {
                    MpConsole.WriteLine("RestoreSelectedClipCommand CanExecute: " + can_restore);
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));
                    MpConsole.WriteLine($"SelectedItem: Trashed: {(SelectedItem == null ? "NULL" : SelectedItem.IsTrashed)}");
                }
                return can_restore;
            });

        public MpIAsyncCommand TrashSelectedClipCommand => new MpAsyncCommand(
            async () => {
                await TrashOrDeleteCopyItemIdAsycn(SelectedItem.CopyItemId, false);
            },
            () => {
                bool can_trash = SelectedItem != null && !SelectedItem.IsTrashed;
                if (!can_trash) {
                    MpConsole.WriteLine("TrashSelectedClipCommand CanExecute: " + can_trash);
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));
                    if (SelectedItem != null) {
                        MpConsole.WriteLine($"SelectedItem: Trashed: {SelectedItem.IsTrashed}");
                    }
                }
                return can_trash;
            });
        public MpIAsyncCommand DeleteSelectedClipCommand => new MpAsyncCommand(
            async () => {
                await TrashOrDeleteCopyItemIdAsycn(SelectedItem.CopyItemId, true);
            },
            () => {
                bool can_delete =
                    SelectedItem != null &&
                    SelectedItem.IsTrashed;
                if (!can_delete) {
                    MpConsole.WriteLine("DeleteSelectedClipCommand CanExecute: " + can_delete);
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));
                }
                return can_delete;
            });

        public ICommand PermanentlyDeleteSelectedClipFromShortcutCommand => new MpAsyncCommand(
            async () => {
                await TrashOrDeleteCopyItemIdAsycn(SelectedItem.CopyItemId, true);
            },
            () => {
                bool can_delete =
                    SelectedItem != null;
                if (!can_delete) {
                    MpConsole.WriteLine("DeleteSelectedClipCommand CanExecute: " + can_delete);
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));
                }
                return can_delete;
            });

        public MpIAsyncCommand TrashOrDeleteSelectedClipFromShortcutCommand => new MpAsyncCommand(
             async () => {
                 // NOTE 
                 if (DeleteSelectedClipCommand.CanExecute(null)) {
                     await DeleteSelectedClipCommand.ExecuteAsync();
                 } else {
                     await TrashSelectedClipCommand.ExecuteAsync();
                 }
             },
            () => {
                bool can_delete =
                    SelectedItem != null &&
                    SelectedItem.IsHostWindowActive &&
                    SelectedItem.IsTitleReadOnly &&
                    SelectedItem.IsContentReadOnly &&
                    !SelectedItem.IsSubSelectionEnabled &&
                    SelectedItem.IsFocusWithin;
                if (!can_delete) {
                    MpConsole.WriteLine("TrashOrDeleteSelectedClipFromShortcutCommand CanExecute: " + can_delete);
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));

                    if (SelectedItem != null) {
                        MpConsole.WriteLine("IsHostWindowActive: " + SelectedItem.IsHostWindowActive);
                        MpConsole.WriteLine("IsTitleReadOnly: " + SelectedItem.IsTitleReadOnly);
                        MpConsole.WriteLine("IsContentReadOnly: " + SelectedItem.IsContentReadOnly);
                        MpConsole.WriteLine("IsSubSelectionEnabled: " + SelectedItem.IsSubSelectionEnabled);
                        MpConsole.WriteLine("IsFocusWithin: " + SelectedItem.IsFocusWithin);
                    }
                }
                return can_delete;
            });

        public ICommand ToggleLinkTagToSelectedItemCommand => new MpAsyncCommand<MpAvTagTileViewModel>(
            async (ttvm) => {
                var ctvm = SelectedItem;
                bool isUnlink = await ttvm.IsCopyItemLinkedAsync(ctvm.CopyItemId);

                if (isUnlink) {
                    // NOTE item is removed from ui from db ondelete event
                    ttvm.UnlinkCopyItemCommand.Execute(ctvm.CopyItemId);
                } else {
                    ttvm.LinkCopyItemCommand.Execute(ctvm.CopyItemId);
                }
                while (ttvm.IsBusy) {
                    await Task.Delay(100);
                }


                await ctvm.InitTitleLayersAsync();

                // trigger selection changed message to notify tag and parents of association change

                MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
            },
            (tagToLink) => {
                //this checks the selected clips association with tagToLink
                //and only returns if ALL selecteds clips are linked or unlinked 
                if (tagToLink == null || SelectedItem == null) {
                    return false;
                }
                return true;
            });

        public ICommand EditSelectedTitleCommand => new MpCommand(
            () => {
                SelectedItem.IsTitleReadOnly = false;
            },
            () => {
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading ||
                    !MpAvPrefViewModel.Instance.ShowContentTitles) {
                    return false;
                }
                return SelectedItem != null;
            });

        public ICommand ToggleIsSelectedContentReadOnlyCommand => new MpCommand(
             () => {
                 SelectedItem.ToggleIsContentReadOnlyCommand.Execute(null);
             },
            () => {
                if (SelectedItem == null) {
                    return false;
                }
                return SelectedItem.ToggleIsContentReadOnlyCommand.CanExecute(null);
            });

        public ICommand EditSelectedContentCommand => new MpAsyncCommand(
            async () => {
                if (SelectedItem.IsSubSelectionEnabled) {
                    // BUG FIX when spacebar is shortcut to edit and sub-selection is enabled
                    // the space is passed to the editor so pausing toggling for space to get out ur system
                    await Task.Delay(DISABLE_READ_ONLY_DELAY_MS);
                }
                SelectedItem.ToggleEditContentCommand.Execute(null);
            },
            () => {
                if (SelectedItem == null) {
                    return false;
                }
                return SelectedItem.DisableContentReadOnlyCommand.CanExecute(null);
            });

        public ICommand AnalyzeSelectedItemCommand => new MpAsyncCommand<object>(
            async (presetIdObj) => {
                int presetId = 0;
                if (presetIdObj is int) {
                    presetId = (int)presetIdObj;
                } else if (presetIdObj is string presetIdStr) {
                    try {
                        presetId = int.Parse(presetIdStr);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error converting '{(presetIdObj == null ? "NULL" : presetIdObj.ToString())}' to presetId", ex);
                        return;
                    }
                }

                if (presetId <= 0) {
                    MpConsole.WriteLine($"Error presetId not provided");
                    return;
                }

                if (MpAvAnalyticItemCollectionViewModel.Instance.Items
                    .FirstOrDefault(x => x.Items.Any(x => x.AnalyticItemPresetId == presetId))
                    is not MpAvAnalyticItemViewModel aivm ||
                    aivm.Items.FirstOrDefault(x => x.AnalyticItemPresetId == presetId) is not MpAvAnalyticItemPresetViewModel aipvm) {
                    return;
                }
                // store sel before potentially long running task
                int sel_ciid = SelectedItem.CopyItemId;
                var sel_ctvm = SelectedItem;
                sel_ctvm.IsBusy = true;

                await aivm.PerformAnalysisCommand.ExecuteAsync(aipvm);

                if (sel_ctvm != null) {
                    sel_ctvm.IsBusy = false;
                }
            }, (args) => {
                return SelectedItem != null;
            });

        public ICommand ToggleIsAppPausedCommand => new MpCommand(
            () => {
                IsIgnoringClipboardChanges = !IsIgnoringClipboardChanges;
            }, () => {
                if (IsAnyAppendMode && !IsIgnoringClipboardChanges) {
                    // no change if appending
                    return false;
                }
                return true;
            });

        public ICommand ToggleRightClickPasteCommand => new MpCommand(
            () => {
                IsRightClickPasteMode = !IsRightClickPasteMode;
                OnPropertyChanged(nameof(RightClickPasteSysTrayIconSourceObj));
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                    title: UiStrings.MouseModeChangeNtfTitle,
                    body: string.Format(UiStrings.MouseModeRightClickPasteNtfText, IsRightClickPasteMode ? UiStrings.CommonOnLabel : UiStrings.CommonOffLabel),
                    msgType: MpNotificationType.AppModeChange).FireAndForgetSafeAsync(this);
                MpMessenger.SendGlobal(IsRightClickPasteMode ? MpMessageType.RightClickPasteEnabled : MpMessageType.RightClickPasteDisabled);
            }, () => !IsIgnoringClipboardChanges);

        public ICommand ToggleAutoCopyModeCommand => new MpCommand(
            () => {
                IsAutoCopyMode = !IsAutoCopyMode;
                OnPropertyChanged(nameof(AutoCopySysTrayIconSourceObj));

                Mp.Services.NotificationBuilder.ShowMessageAsync(
                    title: UiStrings.MouseModeChangeNtfTitle,
                    body: string.Format(UiStrings.MouseModeAutoCopyNtfText, IsAutoCopyMode ? UiStrings.CommonOnLabel : UiStrings.CommonOffLabel),
                    msgType: MpNotificationType.AppModeChange).FireAndForgetSafeAsync(this);
                MpMessenger.SendGlobal(IsAutoCopyMode ? MpMessageType.AutoCopyEnabled : MpMessageType.AutoCopyDisabled);
            }, () => !IsIgnoringClipboardChanges);

        public ICommand EnableFindAndReplaceForSelectedItem => new MpCommand(
            () => {
                SelectedItem.IsFindAndReplaceVisible = true;
            }, () => SelectedItem != null && !SelectedItem.IsFindAndReplaceVisible && SelectedItem.IsTextItem);

        public ICommand SelectClipTileCommand => new MpCommand<object>(
            (args) => {
                MpAvClipTileViewModel ctvm = null;
                if (args is MpAvClipTileViewModel) {
                    ctvm = args as MpAvClipTileViewModel;
                } else if (args is int ciid) {
                    ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                }

                if (ctvm != null) {
                    ctvm.IsSelected = true;
                }
            });

        public ICommand ResetTraySplitterCommand => new MpCommand<object>(
            (args) => {
                if (MpAvMainView.Instance is not MpAvMainView mv) {
                    return;
                }
                var mgs = args as MpAvMovableGridSplitter;
                if (mgs == null) {
                    mgs = mv.GetVisualDescendant<MpAvMovableGridSplitter>();
                    if (mgs == null) {
                        return;
                    }
                }

                var trg = mgs.Parent as Grid;
                if (trg == null) {
                    return;
                }

                double trg_w = 0;
                double trg_h = 0;
                if (trg.ColumnDefinitions.Any()) {
                    trg_w = trg.ColumnDefinitions[0].ActualWidth;
                } else {
                    trg_w = trg.Bounds.Width;
                }

                if (trg.RowDefinitions.Any()) {
                    trg_h = trg.RowDefinitions[0].ActualHeight;
                } else {
                    trg_h = trg.Bounds.Height;
                }

                var p_ratio = GetCurrentDefaultPinTrayRatio();

                double dw = (trg.Bounds.Width * p_ratio.Width) - trg_w;
                double dh = (trg.Bounds.Height * p_ratio.Height) - trg_h;
                MpConsole.WriteLine($"Tray Splitter reset. Ratio: {p_ratio} Delta: {dw},{dh}");
                mgs.ApplyDelta(new Vector(dw, dh));
            });

        public ICommand SelectClipTileTransactionNodeCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not object[] argParts) {
                    return;
                }
                int ciid = (int)argParts[0];
                string anguid = argParts[1] as string;

                var ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                if (ctvm == null) {
                    return;
                }
                if (!ctvm.IsWindowOpen) {
                    await ctvm.PinToPopoutWindowCommand.ExecuteAsync();
                    // get new dc
                    ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                }
                if (ctvm == null) {
                    // timing problems,
                    if (PinnedItems.FirstOrDefault(x => x.IsWindowOpen && x.IsAnyPlaceholder) is { }
                        bad_ctvm) {
                        //  find placeholder popout and re-init
                        var ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(ciid);
                        await bad_ctvm.InitializeAsync(ci);
                        ctvm = bad_ctvm;
                    } else {
                        return;
                    }
                }
                ctvm.TransactionCollectionViewModel.SelectChildCommand.Execute(anguid);
            }, (args) => {
                return args != null;
            });

        public MpIAsyncCommand AddItemWhileIgnoringClipboardCommand => new MpAsyncCommand(
            async () => {
                // BUG cmd execute isn't updating when visibility changes via CanAddItemWhileIgnoringClipboard
                await BuildFromDataObjectAsync(Mp.Services.ClipboardMonitor.LastClipboardDataObject as MpAvDataObject, false, MpDataObjectSourceType.ClipboardWatcher);
            });

        public MpIAsyncCommand DeleteAllContentCommand => new MpAsyncCommand(
            async () => {
                var result = await Mp.Services.PlatformMessageBox.ShowYesNoMessageBoxAsync(
                    title: UiStrings.CommonConfirmLabel,
                    message: UiStrings.ClipTrayDeleteAllMessageText,
                    iconResourceObj: "WarningImage");
                if (!result) {
                    //cancel
                    return;
                }
                // clear trays (retaining query placeholders)
                PinnedItems.Clear();
                while (QueryItems.Any()) {
                    QueryItems.FirstOrDefault().TriggerUnloadedNotification(false, true, true);
                }

                // clear paste clip shortcuts
                var to_remove_scidl = MpAvShortcutCollectionViewModel.Instance.Items.Where(x => x.ShortcutType == MpShortcutType.PasteCopyItem).Select(x => x.ShortcutId).ToList();
                foreach (int to_remove_scid in to_remove_scidl) {
                    if (MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == to_remove_scid) is { } scvm) {
                        MpAvShortcutCollectionViewModel.Instance.Items.Remove(scvm);
                    }
                }
                MpAvShortcutCollectionViewModel.Instance.RefreshFilters();

                // clear tag badges
                var to_clear_ttvm = MpAvTagTrayViewModel.Instance.Items.Where(x => x.BadgeCount > 0);
                foreach (var ttvm in to_clear_ttvm) {
                    ttvm.CopyItemIdsNeedingView.Clear();
                    ttvm.OnPropertyChanged(nameof(ttvm.BadgeCount));
                    ttvm.OnPropertyChanged(nameof(ttvm.CopyItemIdsNeedingView));
                }

                // delete content
                await MpDataModelProvider.DeleteAllContentAsync();

                // refresh tag counts
                await Task.WhenAll(MpAvTagTrayViewModel.Instance.Items.Select(x => x.UpdateClipCountAsync()));

                // refresh query
                await QueryCommand.ExecuteAsync(null);
                //MpAvAppRestarter.ShutdownWithRestartTask("Deleted all content");
            });

        public ICommand ReloadSelectedItemCommand => new MpAsyncCommand(
            async () => {

                if (SelectedItem != null) {
                    await SelectedItem.ReloadAsync();
                }
            });

        public ICommand ReloadAllContentCommand => new MpAsyncCommand(
            async () => {
                // store all items content state
                await Task.WhenAll(
                    AllActiveItems
                    .Select(x => x.PersistContentStateCommand.ExecuteAsync(null)));

                AllActiveItems.ForEach(x => x.IsEditorLoaded = false);
                AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsAnyBusy)));

                // get all content content controls
                var ctccl =
                    MpAvWindowManager.AllWindows
                        .SelectMany(x => x.GetVisualDescendants<MpAvClipTileContentView>())
                        .Select(x => x.FindControl<ContentControl>("ClipTileContentControl"));

                // trigger data context change
                ctccl.ForEach(x => x.ReloadDataContext());


                while (AllActiveItems.Any(x => x.IsAnyBusy)) {
                    await Task.Delay(100);
                }

            });
        public MpIAsyncCommand ReloadAllCommand => new MpAsyncCommand(
            async () => {
                MpConsole.WriteLine($"Content web views to reload: {AllItems.Count()}");
                var sw = Stopwatch.StartNew();
                await Task.WhenAll(AllItems.Select(x => x.ReloadAsync()));
                while (IsAnyBusy) {
                    await Task.Delay(100);
                    if (sw.ElapsedMilliseconds > 5_000) {
                        MpConsole.WriteLine($"Content web view reload timeout reached");
                        break;
                    }
                }
                MpConsole.WriteLine($"Content web view reload DONE {sw.ElapsedMilliseconds}ms");
            });
        public MpIAsyncCommand DisposeAndReloadAllCommand => new MpAsyncCommand(
            async () => {
                await Task.WhenAll(
                    AllActiveItems
                    .Select(x => x.PersistContentStateCommand.ExecuteAsync(null)));

                // store basic pin tile info
                int append_ciid = AppendClipTileViewModel == null ? -1 : AppendClipTileViewModel.CopyItemId;
                var popout_ciidl = PinnedItems.Where(x => x.IsWindowOpen && x.CopyItemId != append_ciid).Select(x => x.CopyItemId).ToList();
                var pinned_ciidl = PinnedItems.Where(x => !popout_ciidl.Contains(x.CopyItemId) && x.CopyItemId != append_ciid).Select(x => x.CopyItemId).ToList();
                int head_query_idx = HeadQueryIdx;

                var sw = Stopwatch.StartNew();
                MpConsole.WriteLine($"Content webviews to clear: {(Items.Count + PinnedItems.Count)}");

                // unpin ALL pinned tiles (not just pin tray tiles)
                await Task.WhenAll(PinnedItems.Where(x => x.IsWindowOpen).Select(x => UnpinTileCommand.ExecuteAsync(x)));
                PinnedItems.Clear();
                Items.Clear();
                await Task.Delay(300);
                var all_wvl = MpAvWindowManager.AllWindows.SelectMany(x => x.GetLogicalDescendants<MpAvContentWebView>()).ToList();

                MpConsole.WriteLine($"Content webviews remaining: {all_wvl.Count}");
                all_wvl.ForEach((x, idx) => MpConsole.WriteLine($"WebView #{idx}: '{x}'"));
                all_wvl.ForEach(x => x.FinishDisposal());

                // ensure its full query
                await QueryCommand.ExecuteAsync(null);
                // now reload offset
                await QueryCommand.ExecuteAsync(head_query_idx);

                var all_pinned_ciidl = pinned_ciidl.Union(popout_ciidl).Union(new[] { append_ciid }).Distinct().Where(x => x > 0).ToList();
                foreach (var ciid_to_pin in all_pinned_ciidl) {
                    var ctvm_to_pin = await CreateOrRetrieveClipTileViewModelAsync(ciid_to_pin);
                    MpPinType pinType =
                    ciid_to_pin == append_ciid ?
                        MpPinType.Append :
                        popout_ciidl.Contains(ciid_to_pin) ?
                            MpPinType.Window :
                            MpPinType.Internal;

                    object args = new object[] { ctvm_to_pin, pinType };
                    await PinTileCommand.ExecuteAsync(args);
                }
                MpConsole.WriteLine($"Content webviews restored: {(Items.Count + PinnedItems.Count)} Total time: {sw.ElapsedMilliseconds}ms");
            });

        #region Append
        public MpQuillAppendStateChangedMessage GetAppendStateMessage(string data) {
            return new MpQuillAppendStateChangedMessage() {
                isAppendLineMode = IsAppendLineMode,
                isAppendInsertMode = IsAppendInsertMode,
                isAppendManualMode = IsAppendManualMode,
                isAppendPaused = IsAppendPaused,
                isAppendPreMode = IsAppendPreMode,
                appendData = data
            };
        }
        private bool IsCopyItemAppendable(MpCopyItem ci) {
            if (ci == null || ci.Id < 1 || ci.ItemType == MpCopyItemType.Image) {
                return false;
            }
            if (AppendClipTileViewModel == null) {
                return true;
            }
            return AppendClipTileViewModel.CopyItemType == ci.ItemType;
        }
        private async Task AssignAppendClipTileAsync(MpAppendModeType appendType) {
            // use cases
            // 1. app is hidden and user hits hot key or enables in system tray
            //  1.1 no new clipboard items have been created 
            //      -wait till appendable item is created to assign
            //  1.2 there's N new items 
            //      -only assign if most recent item is appendable
            // 2. app is open and user enables append
            //  2.1 no items are selected
            //      -wait till appendable item is created to assign
            //  2.2 selected item is not appendable
            //      -wait till appendable item is created to assign
            //  2.3 selected item is appendable
            //      -assign selected item
            Dispatcher.UIThread.VerifyAccess();
            if (AppendClipTileViewModel != null) {
                return;
            }
            MpAvClipTileViewModel append_ctvm = null;
            if (MpAvWindowManager.ActiveWindow != null &&
                MpAvWindowManager.ActiveWindow.DataContext is MpAvClipTileViewModel active_ctvm) {
                MpDebug.Assert(SelectedItem == active_ctvm, $"Append activate sel/active item mismatch");
                append_ctvm = active_ctvm;
            }
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                if (SelectedItem != null) {
                    append_ctvm = SelectedItem;
                }
            } else if (PendingNewModels.Count > 0) {
                var most_recent_ci = PendingNewModels[PendingNewModels.Count - 1];
                PendingNewModels.RemoveAt(0);
                append_ctvm = await CreateOrRetrieveClipTileViewModelAsync(most_recent_ci);
            } else if (Mp.Services.ClipboardMonitor.LastClipboardDataObject != null) {
                var cb_ci = await BuildFromDataObjectAsync(Mp.Services.ClipboardMonitor.LastClipboardDataObject, false, MpDataObjectSourceType.AppendEnabled);
                if (cb_ci != null) {
                    // one case this happens is activating append right after startup w/o altering clipboard
                    append_ctvm = await CreateOrRetrieveClipTileViewModelAsync(cb_ci);
                }
            } else {
                // TODO see maybes about better way
                // activate w/o item and wait (show AppMode change msg)
            }

            if (append_ctvm != null &&
                !IsCopyItemAppendable(append_ctvm.CopyItem)) {
                // invalidate and wait
                append_ctvm = null;
            }
            if (append_ctvm == null) {
                return;
            }
            // NOTE reset append count to speed up initial load (to ignore processing initial content change)
            append_ctvm.AppendCount = 0;
            await PinTileCommand.ExecuteAsync(new object[] { append_ctvm, MpPinType.Append, appendType });
        }

        private async Task UpdateAppendModeStateFlagsAsync(MpAppendModeFlags flags, string source, bool silent = false) {
            IsAppendLineMode = flags.HasFlag(MpAppendModeFlags.AppendLine);
            IsAppendInsertMode = flags.HasFlag(MpAppendModeFlags.AppendInsert);
            IsAppendManualMode = flags.HasFlag(MpAppendModeFlags.Manual);
            IsAppendPaused = flags.HasFlag(MpAppendModeFlags.Paused);
            IsAppendPreMode = flags.HasFlag(MpAppendModeFlags.Pre);


            var last_flags = _appendModeFlags;
            _appendModeFlags = flags;
            OnPropertyChanged(nameof(AppendModeStateFlags));
            OnPropertyChanged(nameof(AppendInlineSysTrayIconSourceObj));
            OnPropertyChanged(nameof(AppendLineSysTrayIconSourceObj));

            if (silent) {
                // is silent is so append stateis set BEFORE popout window is created 
                // because it will attach append msg but no state is set so silent avoids relaying message
                // to let loadContent handle it
                return;
            }

            if (AppendClipTileViewModel == null ||
                AppendClipTileViewModel.WindowState == WindowState.Minimized) {

                ShowEmptyOrMinimizedAppendNotifications(_appendModeFlags, last_flags);
            }

            if (AppendClipTileViewModel != null &&
                AppendClipTileViewModel.GetContentView() is MpAvContentWebView wv) {
                await wv.ProcessAppendStateChangedMessageAsync(GetAppendStateMessage(null), source);
            }
        }

        private void ShowEmptyOrMinimizedAppendNotifications(MpAppendModeFlags cur_flags, MpAppendModeFlags last_flags) {
            // NOTE activate/deactivate not handled here

            if (cur_flags.HasFlag(MpAppendModeFlags.Manual) != last_flags.HasFlag(MpAppendModeFlags.Manual)) {
                string title = cur_flags.HasFlag(MpAppendModeFlags.Manual) ? UiStrings.AppendManualNtfTitle : UiStrings.AppendExtentNtfTitle;
                string body = cur_flags.HasFlag(MpAppendModeFlags.Manual) ? UiStrings.AppendManualNtfText : UiStrings.AppendExtentNtfText;
                string icon_key = cur_flags.HasFlag(MpAppendModeFlags.Manual) ? "IbeamImage" : "ManualImage";
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                       title: title,
                       body: body,
                       msgType: MpNotificationType.AppendModeChanged,
                       iconSourceObj: icon_key).FireAndForgetSafeAsync();
            }

            if (cur_flags.HasFlag(MpAppendModeFlags.Paused) != last_flags.HasFlag(MpAppendModeFlags.Paused)) {
                string title = cur_flags.HasFlag(MpAppendModeFlags.Paused) ? UiStrings.AppendPausedNtfTitle : UiStrings.AppendResumedNtfTitle;
                string body = cur_flags.HasFlag(MpAppendModeFlags.Paused) ? UiStrings.AppendPausedNtfText : UiStrings.AppendResumedNtfText;
                string icon_key = cur_flags.HasFlag(MpAppendModeFlags.Paused) ? "PauseImage" : "PlayImage";
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                       title: title,
                       body: body,
                       msgType: MpNotificationType.AppendModeChanged,
                       iconSourceObj: icon_key).FireAndForgetSafeAsync();
            }

            if (cur_flags.HasFlag(MpAppendModeFlags.Pre) != last_flags.HasFlag(MpAppendModeFlags.Pre)) {
                string title = cur_flags.HasFlag(MpAppendModeFlags.Pre) ? UiStrings.AppendBeforeNtfTitle : UiStrings.AppendAfterNtfTitle;
                string body = cur_flags.HasFlag(MpAppendModeFlags.Pre) ? UiStrings.AppendBeforeNtfText : UiStrings.AppendAfterNtfText;
                string icon_key = cur_flags.HasFlag(MpAppendModeFlags.Pre) ? "BringToFrontImage" : "SendToBackImage";
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                       title: title,
                       body: body,
                       msgType: MpNotificationType.AppendModeChanged,
                       iconSourceObj: icon_key).FireAndForgetSafeAsync();
            }
        }

        private async Task ActivateAppendModeAsync(bool isAppendLine, bool isManualMode) {
            Dispatcher.UIThread.VerifyAccess();
            while (IsAddingClipboardItem) {
                // if new item is being added, its important to wait for it
                await Task.Delay(100);
            }

            MpAppendModeFlags amf = _appendModeFlags;
            if (isAppendLine) {
                amf.AddFlag(MpAppendModeFlags.AppendLine);
                amf.RemoveFlag(MpAppendModeFlags.AppendInsert);
            } else {
                amf.AddFlag(MpAppendModeFlags.AppendInsert);
                amf.RemoveFlag(MpAppendModeFlags.AppendLine);
            }
            if (isManualMode) {
                amf.AddFlag(MpAppendModeFlags.Manual);
            } else {
                amf.RemoveFlag(MpAppendModeFlags.Manual);
            }
            bool was_append_already_enabled = IsAnyAppendMode;
            // NOTE update is silent here
            await UpdateAppendModeStateFlagsAsync(amf, "command", true);

            if (AppendClipTileViewModel == null) {
                // append mode was just toggled ON (param was null)
                await AssignAppendClipTileAsync(isAppendLine ? MpAppendModeType.Line : MpAppendModeType.Insert);
            }

            await UpdateAppendModeStateFlagsAsync(amf, "command", false);

            if (was_append_already_enabled) {
                return;
            }

            if (IsIgnoringClipboardChanges) {
                // ntf append won't work w/o clipboard listener
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                    msgType: MpNotificationType.Message,
                    title: UiStrings.AppendCannotActivateTitle,
                    body: UiStrings.AppendCannotActivateText,
                    iconSourceObj: "WarningImage",
                    maxShowTimeMs: 10_000).FireAndForgetSafeAsync();
            }

            MpMessenger.SendGlobal(MpMessageType.AppendModeActivated);
            if (AppendClipTileViewModel != null) {
                // append popout itself is the notification
                return;
            }

            // no item assigned yet so just show enable message
            string type_str = IsAppendLineMode ? UiStrings.AppendBlockLabel : UiStrings.AppendInlineLabel;
            string manual_str = IsAppendManualMode ? $"({UiStrings.AppendManualLabel}) " : string.Empty;
            string icon_key = IsAppendLineMode ? "AppendLineImage" : "AppendImage";
            Mp.Services.NotificationBuilder.ShowMessageAsync(
                   title: string.Format(UiStrings.AppendActivateTitle, type_str, manual_str),
                   body: UiStrings.AppendActivateText,
                   msgType: MpNotificationType.AppendModeChanged,
                   iconSourceObj: icon_key).FireAndForgetSafeAsync();
        }
        private async Task DeactivateAppendModeAsync() {
            Dispatcher.UIThread.VerifyAccess();

            await UpdateAppendModeStateFlagsAsync(MpAppendModeFlags.None, "command");

            MpMessenger.SendGlobal(MpMessageType.AppendModeDeactivated);
            if (AppendClipTileViewModel == null) {
                // only show deactivate ntf if no windows there
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                           title: UiStrings.AppendDeactivatedTitle,
                           body: UiStrings.AppendDeactivatedText,
                           msgType: MpNotificationType.AppendModeChanged,
                           iconSourceObj: "NoEntryImage").FireAndForgetSafeAsync();
            } else {
                var deactivate_append_ctvm = AppendClipTileViewModel;
                deactivate_append_ctvm.IsAppendNotifier = false;
            }
        }

        private bool IsModelToBeAdded(MpCopyItem aci) {
            // this prevents cap from treating temporary append only items from 'Add' processing
            if (aci.WasDupOnCreate) {
                return false;
            }
            if (
                !IsAnyAppendMode ||
                AppendClipTileViewModel == null ||
                !aci.DataObjectSourceType.IsAppendableSourceType() ||
                !IsCopyItemAppendable(aci)) {
                // not going to be appended
                return true;
            }
            // item WILL be appended, so added determined by pref
            return !MpAvPrefViewModel.Instance.IgnoreAppendedItems;
        }

        private async Task<bool> UpdateAppendModeAsync(MpCopyItem aci) {
            // returns true if item was appended

            Dispatcher.UIThread.VerifyAccess();
            if (IsAppendPaused) {
                // treat as new item
                MpConsole.WriteLine($"Append paused, ignoring and marking new item as not appended");
                return false;
            }
            // NOTE only called in AdddItemFromClipboard when IsAnyAppendMode == true

            if (!IsAnyAppendMode) {
                return false;
            }
            if (AppendClipTileViewModel == null) {
                await AssignAppendClipTileAsync(IsAppendLineMode ? MpAppendModeType.Line : MpAppendModeType.Insert);
                if (AppendClipTileViewModel == null) {
                    return false;
                }
            }
            if (!IsCopyItemAppendable(aci)) {
                return false;
            }

            bool isNew = !aci.WasDupOnCreate;
            string append_data = aci.ItemData;
            if (AppendClipTileViewModel.CopyItemType == MpCopyItemType.FileList) {
                append_data = await MpAvFileItemCollectionViewModel.CreateFileListEditorFragment(aci);
            }

            if (AppendClipTileViewModel.CopyItemId != aci.Id) {
                Dispatcher.UIThread.Post(async () => {
                    // no need to wait for source updates
                    // get appended items transactions which should be only 1 since new and add a paste transaction to append item with its create sources
                    var aci_citl = await MpDataModelProvider.GetCopyItemTransactionsByCopyItemIdAsync(aci.Id);

                    var aci_create_cit = aci_citl.FirstOrDefault(x => x.TransactionType == MpTransactionType.Created);
                    if (aci_create_cit == null) {
                        MpDebug.Break($"append error, no create transaction found for appened item");
                    } else {
                        var aci_create_sources = await MpDataModelProvider.GetSourceRefsByCopyItemTransactionIdAsync(aci_create_cit.Id);

                        if (isNew || !MpAvPrefViewModel.Instance.IgnoreAppendedItems) {
                            // if item is not new or will persist include ref to it
                            // NOTE item ref is added to END to keep primary source at front
                            aci_create_sources.Add(aci);
                        }
                        //// NOTE ignoring msg data here since its only used for analyze transactions for now
                        await Mp.Services.TransactionBuilder.ReportTransactionAsync(
                            copyItemId: AppendClipTileViewModel.CopyItemId,
                            reqType: MpJsonMessageFormatType.DataObject,
                            respType: MpJsonMessageFormatType.Delta,
                            ref_uris: aci_create_sources.Select(x => x.ToSourceUri()),
                            transType: MpTransactionType.Appended);
                    }

                    if (isNew &&
                        MpAvPrefViewModel.Instance.IgnoreAppendedItems) {
                        aci.DeleteFromDatabaseAsync().FireAndForgetSafeAsync();
                    }
                });
            }

            AppendDataCommand.Execute(append_data);
            return true;
        }
        public MpIAsyncCommand DeactivateAppendModeCommand => new MpAsyncCommand(
            async () => {
                if (IsAppendInsertMode) {
                    await ToggleAppendInsertModeCommand.ExecuteAsync();
                } else if (IsAppendLineMode) {
                    await ToggleAppendLineModeCommand.ExecuteAsync();
                }
            }, () => {
                return IsAnyAppendMode;
            });

        public MpIAsyncCommand ToggleAppendInsertModeCommand => new MpAsyncCommand(
            async () => {
                if (IsAppendInsertMode) {
                    await DeactivateAppendModeAsync();
                } else {
                    await ActivateAppendModeAsync(false, IsAppendManualMode);
                }
            });

        public MpIAsyncCommand ToggleAppendLineModeCommand => new MpAsyncCommand(
            async () => {
                if (IsAppendLineMode) {
                    await DeactivateAppendModeAsync();
                } else {
                    await ActivateAppendModeAsync(true, IsAppendManualMode);
                }
            });

        public MpIAsyncCommand ToggleAppendManualModeCommand => new MpAsyncCommand(
            async () => {
                bool new_is_manual = !IsAppendManualMode;
                bool cur_or_new_is_line_mode = IsAppendLineMode;
                if (!IsAnyAppendMode && new_is_manual) {
                    // append line by default
                    cur_or_new_is_line_mode = true;
                }

                await ActivateAppendModeAsync(cur_or_new_is_line_mode, new_is_manual);
            }, () => {
                return IsAnyAppendMode;
            });

        public MpIAsyncCommand ToggleAppendPausedCommand => new MpAsyncCommand(
            async () => {
                var toggled_flags = AppendModeStateFlags;
                if (toggled_flags.HasFlag(MpAppendModeFlags.Paused)) {
                    toggled_flags.RemoveFlag(MpAppendModeFlags.Paused);
                } else {
                    toggled_flags.AddFlag(MpAppendModeFlags.Paused);
                }
                await UpdateAppendModeStateFlagsAsync(toggled_flags, "command");
            }, () => {
                return IsAnyAppendMode;
            });

        public MpIAsyncCommand ToggleAppendPreModeCommand => new MpAsyncCommand(
            async () => {
                if (!IsAnyAppendMode) {
                    // enable line mode by default
                    await ToggleAppendLineModeCommand.ExecuteAsync();
                }
                var toggled_flags = AppendModeStateFlags;
                if (toggled_flags.HasFlag(MpAppendModeFlags.Pre)) {
                    toggled_flags.RemoveFlag(MpAppendModeFlags.Pre);
                } else {
                    toggled_flags.AddFlag(MpAppendModeFlags.Pre);
                }
                await UpdateAppendModeStateFlagsAsync(toggled_flags, "command");
            });

        public MpIAsyncCommand<object> AppendDataCommand => new MpAsyncCommand<object>(
            async (args) => {
                AppendClipTileViewModel.IsWindowOpen = true;
                AppendClipTileViewModel.AppendCount++;
                if (AppendClipTileViewModel.GetContentView() is MpAvContentWebView wv) {
                    await wv.ProcessAppendStateChangedMessageAsync(GetAppendStateMessage(args as string), "command");
                }

            }, (args) => {
                if (AppendClipTileViewModel == null || !IsAnyAppendMode) {
                    return false;
                }
                return args is string argStr && !string.IsNullOrEmpty(argStr);
            });

        public ICommand ShowDevToolsCommand => new MpAsyncCommand(
            async () => {
                if (SelectedItem != null && SelectedItem.GetContentView() is MpAvContentWebView wv) {
                    MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
                    wv.OpenDevTools();
                    // wait for dev window to activate..
                    await Task.Delay(500);
                    MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;

                }
            });



        public ICommand UpdatePasteInfoMessageCommand => new MpCommand<object>(
            (args) => {
                var pi = args as MpPortableProcessInfo;
                if (pi == null && args is MpAvAppViewModel avm) {
                    pi = avm.ToProcessInfo();
                }
                if (pi == null) {
                    if (Mp.Services.ProcessWatcher.LastProcessInfo is { } lpi) {
                        pi = lpi;
                    } else {
                        return;
                    }
                }
                SetCurPasteInfoMessage(pi);
            });

        #endregion

        //public ICommand SendToEmailCommand => new MpCommand(
        //    () => {
        //        // for gmail see https://stackoverflow.com/a/60741242/105028
        //        string pt = string.Join(Environment.NewLine, MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(scrollx => scrollx.ItemData.ToPlainText()));
        //        //MpHelpers.OpenUrl(
        //        //    string.Format("mailto:{0}?subject={1}&body={2}",
        //        //    string.Empty, SelectedItem.CopyItem.Title,
        //        //    pt));
        //        //MpAvClipTrayViewModel.Instance.ClearClipSelection();
        //        //IsSelected = true;
        //        //MpHelpers.CreateEmail(MpJsonPreferenceIO.Instance.AccountEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        //    },
        //    () => {
        //        return !IsAnyEditingClipTile && SelectedItem != null;
        //    });

        #endregion
    }
}
