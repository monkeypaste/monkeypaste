using Avalonia.Layout;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvCopyItemCollectionViewModel : 
        MpViewModelBase<MpAvClipTrayCollectionViewModel>,
        MpIBoundSizeViewModel,
        MpIPagingScrollViewerViewModel {
        #region Private Variable

        private List<MpCopyItem> _newModels = new List<MpCopyItem>();

        private int _anchor_query_idx { get; set; } = -1;

        private bool _isMainWindowOrientationChanging = false;
        #endregion

        #region Constants

        public const int DISABLE_READ_ONLY_DELAY_MS = 500;

        public const double MAX_TILE_SIZE_CONTAINER_PAD = 50;
        public const double MIN_SIZE_ZOOM_FACTOR_COEFF = (double)1 / (double)7;
        public const double EDITOR_TOOLBAR_MIN_WIDTH = 830.0d;

        #endregion

        #region Statics

        public static string EditorPath {
            get {
                //file:///Volumes/BOOTCAMP/Users/tkefauver/Source/Repos/MonkeyPaste/MonkeyPaste/Resources/Html/Editor/index.html
                //string editorPath = Path.Combine(Environment.CurrentDirectory, "Resources", "Html", "Editor", "index.html");
                //string editorPath = @"file:///C:/Users/tkefauver/Source/Repos/MonkeyPaste/MonkeyPaste/Resources/Html/Editor/index.html";
                //if (OperatingSystem.IsWindows()) {
                //    return editorPath;
                //}
                //if (OperatingSystem.IsMacOS()) {
                //    return @"file:///Volumes/BOOTCAMP/Users/tkefauver/Source/Repos/MonkeyPaste/MonkeyPaste/Resources/Html/Editor/index.html";
                //}
                //if(OperatingSystem.IsLinux()) {

                //}
                var uri = new Uri(MpAvCefNetApplication.GetEditorPath(), UriKind.Absolute);
                string uriStr = uri.AbsoluteUri;
                return uriStr;
            }
        }
        #endregion

        #region Interfaces

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

        public double ScrollFrictionX {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 0.85d;
                }
                return 0.85d;
            }
        }

        public double ScrollWheelDampeningX {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 0.03d;
                }
                return 0.01d;
            }
        }

        public double ScrollFrictionY {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 0.85d;
                }
                return 0.85d;
            }
        }

        public double ScrollWheelDampeningY {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 0.03d;
                }
                return 0.01d;
            }
        }

        public Orientation ListOrientation => MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ? Orientation.Horizontal : Orientation.Vertical;

        public bool IsHorizontalScrollBarVisible => true;// QueryTrayTotalTileWidth > QueryTrayScreenWidth;
        public bool IsVerticalScrollBarVisible => true;// QueryTrayTotalTileHeight > QueryTrayScreenHeight;

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
                double maxScrollOffsetX = Math.Max(0, QueryTrayTotalTileWidth - QueryTrayScreenWidth);
                return maxScrollOffsetX;
            }
        }
        public double MaxScrollOffsetY {
            get {
                double maxScrollOffsetY = Math.Max(0, QueryTrayTotalTileHeight - QueryTrayScreenHeight);
                return maxScrollOffsetY;
            }
        }

        public MpRect QueryTrayScreenRect => new MpRect(0, 0, QueryTrayScreenWidth, QueryTrayScreenHeight);


        public double QueryTrayTotalTileWidth { get; private set; }
        public double QueryTrayTotalTileHeight { get; private set; }

        public double QueryTrayTotalWidth => Math.Max(0, Math.Max(QueryTrayScreenWidth, QueryTrayTotalTileWidth));
        public double QueryTrayTotalHeight => Math.Max(0, Math.Max(QueryTrayScreenHeight, QueryTrayTotalTileHeight));

        public double QueryTrayScreenWidth { get; set; }

        public double QueryTrayScreenHeight { get; set; }

        public double LastZoomFactor { get; set; }

        private double _zoomFactor = 1;
        public double ZoomFactor {
            get => _zoomFactor;
            set {
                if (ZoomFactor != value) {
                    LastZoomFactor = _zoomFactor;
                    _zoomFactor = value;
                    OnPropertyChanged(nameof(ZoomFactor));
                }
            }
        }
        public double ScrollVelocityX { get; set; }
        public double ScrollVelocityY { get; set; }

        public MpPoint ScrollVelocity {
            get => new MpPoint(ScrollVelocityX, ScrollVelocityY);
            set {
                var newVal = value == null ? MpPoint.Zero : value;
                ScrollVelocityX = newVal.X;
                ScrollVelocityY = newVal.Y;
            }
        }

        public bool CanScroll {
            get {
                //return true;

                if (MpAvMainWindowViewModel.Instance.IsMainWindowOpening ||
                   !MpAvMainWindowViewModel.Instance.IsMainWindowOpen ||
                    IsRequery/* ||
                   IsScrollingIntoView*/) {
                    return false;
                }

                if (HasScrollVelocity) {
                    // this implies mouse is/was not over a sub-selectable tile and is scrolling so ignore item scroll if already moving
                    return true;
                }
                // TODO? giving item scroll priority maybe better by checking if content exceeds visible boundaries here
                bool isItemScrollPriority = Items.Any(x => x.IsSubSelectionEnabled && x.IsHovering);
                if (isItemScrollPriority) {
                    // when tray is not scrolling (is still) and mouse is over sub-selectable item keep tray scroll frozen
                    return false;
                }

                if (SelectedItem == null) {
                    return true;
                }
                if (SelectedItem.IsVerticalScrollbarVisibile &&
                    SelectedItem.IsHovering) {
                    return false;
                }
                return true;
            }
        }
        public bool IsThumbDraggingX { get; set; } = false;
        public bool IsThumbDraggingY { get; set; } = false;
        public bool IsThumbDragging => IsThumbDraggingX || IsThumbDraggingY;

        public bool IsScrollJumping { get; set; }

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
                    return 1;
                }
                return CurGridFixedCount;
            }
        }

        #region Paging Helper Methods
        private void ForceScrollOffsetX(double newOffsetX, bool isSilent = false) {
            if (newOffsetX < 0 || newOffsetX > MaxScrollOffsetX) {
                //Debugger.Break();
            }
            //newOffsetX = Math.Min(MaxScrollOffsetX, Math.Max(0, newOffsetX));
            _scrollOffsetX = LastScrollOffsetX = newOffsetX;
            if (isSilent) {
                return;
            }
            OnPropertyChanged(nameof(ScrollOffsetX));
        }

        private void ForceScrollOffsetY(double newOffsetY, bool isSilent = false) {
            if (newOffsetY < 0 || newOffsetY > MaxScrollOffsetY) {
                //Debugger.Break();
            }
            //newOffsetY = Math.Min(MaxScrollOffsetY, Math.Max(0, newOffsetY));
            _scrollOffsetY = LastScrollOffsetY = newOffsetY;
            if (isSilent) {
                return;
            }
            OnPropertyChanged(nameof(ScrollOffsetY));
        }
        private void ForceScrollOffset(MpPoint newOffset, bool isSilent = false) {
            ForceScrollOffsetX(newOffset.X, isSilent);
            ForceScrollOffsetY(newOffset.Y, isSilent);
        }

        private void SetScrollAnchor(bool isLayoutChangeToGrid = false) {
            // this keeps track of the first screen visible tile 
            // so when orientation or zoom is changed relative offset is retained
            // in conjunction w/ drop scroll anchor
            if (IsTrayEmpty) {
                _anchor_query_idx = -1;
                return;
            }
            if (isLayoutChangeToGrid) {
                // when change is to grid change anchor to first element on fixed dimension of what the anchor was
                var anchor_loc = Items.FirstOrDefault(x => x.QueryOffsetIdx == _anchor_query_idx).TrayLocation;
                if (ListOrientation == Orientation.Horizontal) {
                    _anchor_query_idx = Items.Where(x => Math.Abs(x.TrayY - anchor_loc.Y) < 3).Aggregate((a, b) => a.TrayX < b.TrayX ? a : b).QueryOffsetIdx;
                } else {
                    _anchor_query_idx = Items.Where(x => Math.Abs(x.TrayX - anchor_loc.X) < 3).Aggregate((a, b) => a.TrayY < b.TrayY ? a : b).QueryOffsetIdx;
                }
                return;
            }
            if (VisibleItems.Count() == 0) {
                //Debugger.Break();
                LockScrollToAnchor();
                if (VisibleItems.Count() == 0) {
                    return;
                }
            }
            // finds visible tile closest to origin of tray
            _anchor_query_idx = VisibleItems.Aggregate((a, b) => a.TrayLocation.Distance(MpPoint.Zero) < b.TrayLocation.Distance(MpPoint.Zero) ? a : b).QueryOffsetIdx;
            MpConsole.WriteLine("AnchorIdx: " + _anchor_query_idx);
        }

        private void LockScrollToAnchor() {
            // this only performs scrolling to a translated scroll offset, items are already loaded but scroll offset needs this adjustment
            if (_anchor_query_idx < 0) {
                return;
            }
            MpPoint anchor_offset = MpPoint.Zero;
            var anchor_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == _anchor_query_idx);
            if (anchor_ctvm == null) {
                // this occurs in a scroll jump (at least), set anchor to page head
                //if(HeadItem != null) {
                //    anchor_offset = HeadItem.TrayLocation;
                //} else {
                //    // what's going on here? is the list empty?
                //    Debugger.Break();
                //}

                //Debugger.Break();
            } else {
                anchor_offset = anchor_ctvm.TrayLocation;
            }

            ForceScrollOffset(anchor_offset);
        }
        private void FindTotalTileSize() {
            // NOTE this is to avoid making TotalTile Width/Height auto
            // and should only be called on a requery or on content resize (or event better only on resize complete)
            MpSize totalTileSize = MpSize.Empty;
            if (TotalTilesInQuery > 0) {
                var result = FindTileRectOrQueryIdxOrTotalTileSize_internal(
                    queryOffsetIdx: -1,
                    scrollOffsetX: -1,
                    scrollOffsetY: -1);
                if (result is MpSize) {
                    totalTileSize = (MpSize)result;
                }
            }

            QueryTrayTotalTileWidth = totalTileSize.Width;
            QueryTrayTotalTileHeight = totalTileSize.Height;
        }

        private object FindTileRectOrQueryIdxOrTotalTileSize_internal(int queryOffsetIdx, double scrollOffsetX, double scrollOffsetY, MpRect prevOffsetRect = null) {
            // For TotalTileSize<MpSize>: all params -1
            // For TileRect<MpRect>:  0 <= queryOffsetIdx < TotalTilesInQuery and scrollOffsets == -1
            // For TileQueryIdx<[]{int,MpRect}>: queryoffsetIdx < 0 and both scrollOffset > 0

            bool isGrid = LayoutType == MpClipTrayLayoutType.Grid;
            bool isStack = !isGrid;

            bool isFindTileIdx = scrollOffsetX >= 0 && scrollOffsetY >= 0;
            bool isFindTileRect = !isFindTileIdx && queryOffsetIdx >= 0;
            bool isFindTotalSize = !isFindTileRect;

            int totalTileCount = MpPlatform.Services.Query.TotalAvailableItemsInQuery;
            queryOffsetIdx = isFindTotalSize ? TotalTilesInQuery - 1 : queryOffsetIdx;
            if (queryOffsetIdx >= totalTileCount) {
                return null;
            }

            int startIdx = 0;// prevOffsetRect == null ? 0 : queryOffsetIdx;

            var total_size = MpSize.Empty;
            int gridFixedCount = -1;

            MpRect last_rect = null;// prevOffsetRect;

            for (int i = startIdx; i <= queryOffsetIdx; i++) {
                int tileId = MpPlatform.Services.Query.PageTools.GetItemId(i);
                MpSize tile_size = DefaultItemSize;
                if (MpAvPersistentClipTilePropertiesHelper.TryGetByPersistentSize_ById(tileId, out double uniqueSize)) {
                    tile_size.Width = uniqueSize;
                }

                MpPoint tile_offset = null;
                if (last_rect == null) {
                    // initial case
                    tile_offset = MpPoint.Zero;
                } else {
                    tile_offset = last_rect.Location;
                    if (ListOrientation == Orientation.Horizontal) {
                        tile_offset.X = last_rect.Right;
                    } else {
                        tile_offset.Y = last_rect.Bottom;
                    }
                }

                MpRect tile_rect = new MpRect(tile_offset, tile_size);
                bool is_tile_wrapped = false;
                if (tile_rect.Right > DesiredMaxTileRight) {
                    // when tile projected rect is beyond desired max width (grid horizontal/stack vertical)

                    if (isStack) {
                        // this means based on tray orientation/layout it can't contain this tile
                        // so it will need to overflow 
                        // always occurs in stack so ignored
                    } else if (last_rect != null) {
                        // wrap to next linez
                        tile_rect.X = 0;
                        tile_rect.Y = last_rect.Bottom;

                        //tile_rect = new MpRect(0,last_rect.Bottom, tile_size.Width, tile_size.Height);
                        is_tile_wrapped = true;
                    } else {
                        // edge case 1st tile is a biggin
                        // NOTE not sure if wrapped should be flagged here, just reacting to exception for last_rect being null
                    }
                }

                if (tile_rect.Bottom > DesiredMaxTileBottom) {
                    // when tile projected rect is beyond desired max height (grid vertical/stack horizontal)

                    if (isStack) {
                        // this means based on tray orientation/layout it can't contain this tile
                        // so it will need to overflow 
                        // always occurs in stack
                    } else if (last_rect != null) {
                        // wrap to next line
                        tile_rect.X = last_rect.Right;
                        tile_rect.Y = 0;

                        //tile_rect = new MpRect(last_rect.Right, 0, tile_size.Width, tile_size.Height);
                        is_tile_wrapped = true;
                    } else {
                        // edge case 1st tile is a biggin
                        // NOTE not sure if wrapped should be flagged here, just reacting to exception for last_rect being null
                    }
                }

                if (is_tile_wrapped && gridFixedCount < 0) {
                    gridFixedCount = i;
                }

                total_size.Width = Math.Max(tile_rect.Right, total_size.Width);
                total_size.Height = Math.Max(tile_rect.Bottom, total_size.Height);

                if (isFindTileIdx) {
                    if (tile_rect.X >= scrollOffsetX && tile_rect.Y >= scrollOffsetY) {
                        // NOTE not sure why but in this mode, find by scrollOffset returns target tile + 1 
                        // so returning previous rect when found
                        return new object[] { i, tile_rect };
                    }
                }
                last_rect = tile_rect;
            }

            if (isFindTileIdx) {
                // if not found presume offset is beyond last tile
                return TotalTilesInQuery - 1;
            }
            if (isFindTileRect) {
                return last_rect;
            }
            if (isFindTotalSize) {
                if (IsGridLayout) {
                    CurGridFixedCount = gridFixedCount;
                } else {
                    CurGridFixedCount = 0;
                }
                return total_size;
            }
            return null;
        }

        public MpRect FindTileRect(int queryOffsetIdx, MpRect prevOffsetRect) {

            object result = FindTileRectOrQueryIdxOrTotalTileSize_internal(
                                queryOffsetIdx: queryOffsetIdx,
                                scrollOffsetX: -1,
                                scrollOffsetY: -1,
                                prevOffsetRect: prevOffsetRect);
            if (result is MpRect tileRect) {
                return tileRect;
            }
            return MpRect.Empty;
        }

        public int FindJumpTileIdx(double scrollOffsetX, double scrollOffsetY, out MpRect tileRect) {
            object result = FindTileRectOrQueryIdxOrTotalTileSize_internal(
                                queryOffsetIdx: -1,
                                scrollOffsetX: scrollOffsetX,
                                scrollOffsetY: scrollOffsetY);
            if (result is object[] resultParts) {
                tileRect = resultParts[1] as MpRect;
                return (int)resultParts[0];
            }
            tileRect = MpRect.Empty;
            return -1;
        }


        private bool CanCheckLoadMore() {
            if (IsThumbDragging ||
                IsAnyBusy ||
                IsAnyResizing ||
                MpAvMainWindowViewModel.Instance.IsResizing ||
                Items.Count <= RemainingItemsCountThreshold) {
                return false;
            }
            return true;
        }

        private void CheckLoadMore(bool isZoomCheck = false) {
            if (!CanCheckLoadMore()) {
                return;
            }
            double dx = 0, dy = 0;
            bool isLessZoom = false;
            if (isZoomCheck) {
                isLessZoom = ZoomFactor - LastZoomFactor < 0;
            } else {
                dx = ScrollOffsetX - LastScrollOffsetX;
                dy = ScrollOffsetY - LastScrollOffsetY;
            }

            // NOTE when zooming the anchor item SHOULD be locked to lo edge so won't need to check lo
            bool checkLo = (dx < 0 || dy < 0) && HeadQueryIdx > 0;
            bool checkHi = (dx > 0 || dy > 0 || isLessZoom) && TailQueryIdx < MaxClipTrayQueryIdx;

            bool isScrollHorizontal = (LayoutType == MpClipTrayLayoutType.Stack && ListOrientation == Orientation.Horizontal) ||
                                      (LayoutType == MpClipTrayLayoutType.Grid && ListOrientation == Orientation.Vertical);

            if (checkHi && checkLo) {
                MpConsole.WriteLine("LoadMore infinite check detected, calling refresh query to prevent");
                MpPlatform.Services.Query.NotifyQueryChanged();
                return;
            }

            if (checkHi) {
                int hi_thresholdQueryIdx = Math.Max(0, TailQueryIdx - RemainingItemsCountThreshold);

                MpAvClipTileViewModel hi_ctvm = VisibleItems.Count() == 0 ? null : VisibleItems.Aggregate((a, b) => a.TrayX > b.TrayX ? a : b);
                //var hi_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == hi_thresholdQueryIdx);
                if (hi_ctvm == null) {
                    // Debugger.Break();
                }
                //double hi_diff = isScrollHorizontal ?
                //                    hi_ctvm.TrayRect.Right - ScrollOffsetX - QueryTrayScreenWidth :
                //                    hi_ctvm.TrayRect.Bottom - ScrollOffsetY - QueryTrayScreenHeight;
                //bool addMoreToTail = hi_diff < 0;
                //bool addMoreToTail = ScreenRect.Contains(hi_ctvm.ScreenRect.BottomRight);
                bool addMoreToTail = hi_ctvm == null || hi_ctvm.QueryOffsetIdx >= hi_thresholdQueryIdx;
                if (addMoreToTail) {
                    //MpConsole.WriteLine("Load more to tail");
                    // when right (horizontal scroll) or bottom (vertical scroll) of high threshold tile is within viewport
                    QueryCommand.Execute(true);
                    //if (!isLessZoom) {
                    //    return;
                    //}
                    return;
                }
            }
            if (checkLo) {
                int lo_thresholdQueryIdx = Math.Max(0, HeadQueryIdx + RemainingItemsCountThreshold);
                MpAvClipTileViewModel lo_ctvm = VisibleItems.Count() == 0 ? null : VisibleItems.Aggregate((a, b) => a.TrayX < b.TrayX ? a : b);
                //var lo_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == lo_thresholdQueryIdx);
                if (lo_ctvm == null) {
                    //Debugger.Break();
                }
                bool addMoreToHead = lo_ctvm == null || lo_ctvm.QueryOffsetIdx <= lo_thresholdQueryIdx;
                //double lo_diff = isScrollHorizontal ?
                //                    lo_ctvm.TrayRect.Right - ScrollOffsetX :
                //                    lo_ctvm.TrayRect.Top - ScrollOffsetY;
                //bool addMoreToHead = lo_diff > 0;
                //bool addMoreToHead = ScreenRect.Contains(lo_ctvm.ScreenRect.TopLeft);

                if (addMoreToHead) {
                    //MpConsole.WriteLine("Load more to head");
                    // when right (horizontal scroll) or bottom (vertical scroll) of high threshold tile is within viewport
                    QueryCommand.Execute(false);
                    //if (!isLessZoom) {
                    //    return;
                    //}
                    return;
                }
            }
        }
        #endregion

        #region Default Tile Layout 

        public double DesiredMaxTileRight {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return QueryTrayScreenWidth;
                } else {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return QueryTrayScreenWidth;
                    }
                    return double.PositiveInfinity;
                }
            }
        }

        public double DesiredMaxTileBottom {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return QueryTrayScreenHeight;
                    }
                    return double.PositiveInfinity;
                } else {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return QueryTrayScreenHeight;
                }
            }
        }

        public double DefaultItemWidth {
            get {
                double defaultWidth;
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    defaultWidth = ListOrientation == Orientation.Horizontal ?
                                    (QueryTrayScreenHeight * ZoomFactor) :
                                    (QueryTrayScreenWidth * ZoomFactor);
                } else {
                    defaultWidth = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds.Width *
                                ZoomFactor * MIN_SIZE_ZOOM_FACTOR_COEFF;
                }
                double scrollBarSize = ScrollBarFixedAxisSize;// IsHorizontalScrollBarVisible ? 30:0;
                return Math.Clamp(defaultWidth - scrollBarSize, 0, MaxTileWidth);
            }
        }

        public double DefaultItemHeight {
            get {
                double defaultHeight;
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    defaultHeight = ListOrientation == Orientation.Horizontal ?
                                    (QueryTrayScreenHeight * ZoomFactor) :
                                    (QueryTrayScreenWidth * ZoomFactor);
                } else {
                    defaultHeight = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds.Width *
                                ZoomFactor * MIN_SIZE_ZOOM_FACTOR_COEFF;
                }
                double scrollBarSize = ScrollBarFixedAxisSize;// IsVerticalScrollBarVisible ? 30 : 0;
                return Math.Clamp(defaultHeight - scrollBarSize, 0, MaxTileHeight);
            }
        }

        public MpSize DefaultItemSize => new MpSize(DefaultItemWidth, DefaultItemHeight);

        public double DefaultEditableItemWidth => EDITOR_TOOLBAR_MIN_WIDTH;

        public MpSize DefaultEditableItemSize => new MpSize(DefaultEditableItemWidth, DefaultItemHeight);
        public double ScrollBarFixedAxisSize => 30;

        #endregion

        #region Virtual

        public int HeadQueryIdx => SortOrderedItems.Count() == 0 ? -1 : SortOrderedItems.Min(x => x.QueryOffsetIdx);

        public int TailQueryIdx => SortOrderedItems.Count() == 0 ? -1 : Items.Max(x => x.QueryOffsetIdx);

        public int MaxLoadQueryIdx => Math.Max(0, MaxClipTrayQueryIdx - DefaultLoadCount + 1);

        public int MaxClipTrayQueryIdx => TotalTilesInQuery - 1;
        public int MinClipTrayQueryIdx => 0;

        public bool CanThumbDragY => QueryTrayScreenHeight < QueryTrayTotalHeight;
        public bool CanThumbDragX => QueryTrayScreenWidth < QueryTrayTotalWidth;

        public bool CanThumbDrag => CanThumbDragX || CanThumbDragY;
        #endregion

        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvClipTileViewModel> Items { get; private set; } = new ObservableCollection<MpAvClipTileViewModel>();
        public IEnumerable<MpAvClipTileViewModel> SortOrderedItems => Items.Where(x => x.QueryOffsetIdx >= 0).OrderBy(x => x.QueryOffsetIdx);

        public IEnumerable<MpAvClipTileViewModel> ActiveItems =>
            SortOrderedItems.Where(x => !x.IsPlaceholder);
        public IEnumerable<MpAvClipTileViewModel> VisibleItems => 
            Items.Where(x => x.IsAnyQueryCornerVisible && !x.IsPlaceholder);

        public MpAvClipTileViewModel HeadItem => SortOrderedItems.ElementAtOrDefault(0);

        public MpAvClipTileViewModel TailItem => SortOrderedItems.ElementAtOrDefault(Items.Count - 1);
        public int PersistantSelectedItemId {
            get {
                if (SelectedItem == null) {
                    if (MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count == 0) {
                        return -1;
                    }
                    return MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels[0].Id;
                }
                return SelectedItem.CopyItemId;
            }
        }
        public MpAvClipTileViewModel SelectedItem { get; set; }


        public List<MpCopyItem> SelectedModels {
            get {
                if (SelectedItem == null) {
                    return new List<MpCopyItem>();
                }
                return new List<MpCopyItem>() {
                    SelectedItem.CopyItem
                };
            }
        }

        #endregion

        #region Layout
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
        public int CurGridFixedCount { get; set; }
        public double MaxTileWidth => double.PositiveInfinity;// Math.Max(0, QueryTrayScreenWidth - MAX_TILE_SIZE_CONTAINER_PAD);
        public double MaxTileHeight => double.PositiveInfinity;// Math.Max(0, QueryTrayScreenHeight - MAX_TILE_SIZE_CONTAINER_PAD);


        private MpClipTrayLayoutType? _layoutType;
        public MpClipTrayLayoutType LayoutType {
            get {
                if (_layoutType == null) {
                    _layoutType = MpPrefViewModel.Instance.ClipTrayLayoutTypeName.ToEnum<MpClipTrayLayoutType>();
                }
                return _layoutType.Value;
            }
            set {
                if (LayoutType != value) {
                    _layoutType = value;
                    MpPrefViewModel.Instance.ClipTrayLayoutTypeName = value.ToString();
                    OnPropertyChanged(nameof(LayoutType));
                }
            }
        }

        #endregion

        #region Appearance
        public string EmptyQueryTrayText {
            get {
                if(Parent == null) {
                    return string.Empty;
                }
                bool isAllPinnwd = Parent.PinCollection != this && Parent.IsQueryAllPinned;
                return
                    isAllPinnwd ? " All Pinned" :
                    MpAvSearchCriteriaItemCollectionViewModel.Instance.PendingQueryTagId > 0 ? $" Has No Results" :
                    " Empty";
            }
        }

        #endregion

        #region State


        public bool IsSelectionReset { get; set; } = false;
        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsAnyBusy);
        public bool IsAnyResizing => Items.Any(x => x.IsResizing);
        public bool IsTrayEmpty => TotalTilesInQuery == 0;
        public int DefaultLoadCount {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 20;
                } else {
                    return 40;
                    // for grid try to make default load count so it lands at the end of the fixed side
                    //double length = ListOrientation != Orientation.Horizontal ? QueryTrayScreenHeight : QueryTrayScreenWidth;
                    //double item_length = ListOrientation != Orientation.Horizontal ? DefaultItemSize.Height : DefaultItemSize.Width;
                    //double items_per_length = length / item_length;

                    //return (int)items_per_length * 2;

                    //double count_val = (double)CurGridFixedCount * items_per_length;
                    //int count = (int)count_val;
                    //while(count % CurGridFixedCount != 0) {
                    //    count--;
                    //    if(count <= 0) {
                    //        Debugger.Break();
                    //    }
                    //}
                    //return count * 3;
                }
            }
        }
        public int TotalTilesInQuery { get; private set; }
        public bool HasScrollVelocity => Math.Abs(ScrollVelocityX) + Math.Abs(ScrollVelocityY) > 0.1d;

        public bool IsScrollingIntoView { get; set; }

        public bool IsGridLayout { get; set; }

        public bool IsRequery { get; set; } = false;

        #endregion

        #endregion

        #region Events

        public event EventHandler<object> OnScrollIntoPinTrayViewRequest;
        public event EventHandler OnScrollToHomeRequest;

        #endregion

        #region Constructors
        public MpAvCopyItemCollectionViewModel(MpAvClipTrayCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAvClipTrayViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;
        }
        #endregion

        #region Public Methods
        public async Task InitializeAsync() {
            LogPropertyChangedEvents = false;

            IsBusy = true;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
            Items.Clear();
            int idx = 0;
            for (int i = 0; i < DefaultLoadCount; i++) {
                var ctvm = await CreateClipTileViewModel(null);
                Items.Add(ctvm);
            }
            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            IsGridLayout = LayoutType == MpClipTrayLayoutType.Grid;
            IsBusy = false;
        }

        public async Task<MpAvClipTileViewModel> CreateClipTileViewModel(MpCopyItem ci, int queryOffsetIdx = -1) {
            MpAvClipTileViewModel ctvm = new MpAvClipTileViewModel(this);
            await ctvm.InitializeAsync(ci, queryOffsetIdx);
            return ctvm;
        }

        public override string ToString() {
            return $"ClipTray";
        }

        public async Task UpdateEmptyPropertiesAsync() {
            // send signal immediatly but also wait and send for busy dependants
            OnPropertyChanged(nameof(IsTrayEmpty));
            OnPropertyChanged(nameof(EmptyQueryTrayText));
            OnPropertyChanged(nameof(IsTrayEmpty));
            OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsVerticalScrollBarVisible));

            while (IsAnyBusy) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(IsTrayEmpty));
            OnPropertyChanged(nameof(EmptyQueryTrayText));
            OnPropertyChanged(nameof(IsTrayEmpty));
            OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
        }

        public void RefreshQueryTrayLayout(MpAvClipTileViewModel fromItem = null) {
            FindTotalTileSize();

            fromItem = fromItem == null ? HeadItem : fromItem;
            UpdateTileRectCommand.Execute(fromItem);

            Items.ForEach(x => x.OnPropertyChanged(nameof(x.MinSize)));

            OnPropertyChanged(nameof(QueryTrayTotalHeight));
            OnPropertyChanged(nameof(QueryTrayTotalWidth));

            OnPropertyChanged(nameof(MaxScrollOffsetX));
            OnPropertyChanged(nameof(MaxScrollOffsetY));

            OnPropertyChanged(nameof(QueryTrayTotalTileWidth));
            OnPropertyChanged(nameof(QueryTrayTotalTileHeight));

            OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
        }

        public void ScrollIntoView(MpRect rect) {
            IsScrollingIntoView = true;

            double pad = 0;
            MpRect svr = new MpRect(0, 0, QueryTrayScreenWidth, QueryTrayScreenHeight);
            MpRect ctvm_rect = rect;

            MpPoint delta_scroll_offset = new MpPoint();
            if (DefaultScrollOrientation == Orientation.Horizontal) {
                if (ctvm_rect.Left < svr.Left) {
                    //item is outside on left
                    delta_scroll_offset.X = ctvm_rect.Left - svr.Left - pad;
                } else if (ctvm_rect.Right > svr.Right) {
                    //item is outside on right
                    delta_scroll_offset.X = ctvm_rect.Right - svr.Right + pad;
                }
            } else {
                if (ctvm_rect.Top < svr.Top) {
                    //item is outside above
                    delta_scroll_offset.Y = ctvm_rect.Top - svr.Top - pad;
                } else if (ctvm_rect.Bottom > svr.Bottom) {
                    //item is outside below
                    delta_scroll_offset.Y = ctvm_rect.Bottom - svr.Bottom + pad;
                }
            }

            var target_offset = ScrollOffset + delta_scroll_offset;
            ScrollVelocity = MpPoint.Zero;
            ForceScrollOffset(target_offset);
            IsScrollingIntoView = false;

        }

        public void RequestScrollToHome() {
            OnScrollToHomeRequest?.Invoke(this, null);
        }
        #endregion

        #region Protected Methods

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            //if (e is MpCopyItem ci) {
            //_allTiles.Add(CreateClipTileViewModel(ci));
            // }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                var removed_ctvm = Items.FirstOrDefault(x => x.CopyItemId == ci.Id);
                if (removed_ctvm != null) {
                    bool wasSelected = removed_ctvm.IsSelected;

                    int removedQueryOffsetIdx = removed_ctvm.QueryOffsetIdx;
                    removed_ctvm.TriggerUnloadedNotification();
                    Items.Where(x => x.QueryOffsetIdx > removedQueryOffsetIdx).ForEach(x => x.QueryOffsetIdx--);
                    //IsBatchOffsetChange = false;

                    if (wasSelected) {
                        var sel_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == removedQueryOffsetIdx);
                        if (sel_ctvm == null) {
                            // when tail
                            sel_ctvm = Items.OrderBy(x => x.QueryOffsetIdx).Last();
                        }
                        SelectedItem = sel_ctvm;
                    }

                    CheckLoadMore();
                    RefreshQueryTrayLayout();

                    OnPropertyChanged(nameof(TotalTilesInQuery));
                }

            }
        }


        #endregion
        #endregion

        #region Private Methods
        private void MpAvClipTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(SelectedItem):
                    MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
                    break;
                case nameof(Items):
                    OnPropertyChanged(nameof(CanScroll));
                    break;
                case nameof(IsGridLayout):
                    ToggleLayoutTypeCommand.Execute(null);
                    break;
                case nameof(ListOrientation):
                    break;
                case nameof(ZoomFactor):
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayZoomFactorChanged);
                    break;
                case nameof(LayoutType):

                case nameof(QueryTrayScreenHeight):
                    if (QueryTrayScreenHeight < 0) {
                        Debugger.Break();
                        QueryTrayScreenHeight = 0;
                    }
                    break;
                case nameof(QueryTrayScreenWidth):
                    if (QueryTrayScreenWidth < 0) {
                        Debugger.Break();
                        QueryTrayScreenWidth = 0;
                    }
                    //RefreshLayout();
                    break;
                case nameof(QueryTrayTotalTileWidth):
                case nameof(QueryTrayTotalTileHeight):
                    if (QueryTrayTotalTileWidth < 0 || QueryTrayTotalTileHeight < 0) {
                        Debugger.Break();
                        QueryTrayScreenWidth = 0;
                    }
                    OnPropertyChanged(nameof(MaxScrollOffsetX));
                    OnPropertyChanged(nameof(MaxScrollOffsetY));

                    OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                    OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
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
                case nameof(IsScrollJumping):
                    if (IsScrollJumping) {
                        IsScrollJumping = false;
                        QueryCommand.Execute(ScrollOffset);
                    }
                    break;
                //case nameof(DefaultItemWidth):
                //case nameof(DefaultItemHeight):
                //case nameof(DefaultItemSize):
                //    AllItems.ForEach(x => x.OnPropertyChanged(nameof(x.MinSize)));
                //    break;
                case nameof(HasScrollVelocity):
                    //MpPlatformWrapper.Services.Cursor.IsCursorFrozen = HasScrollVelocity;

                    if (HasScrollVelocity) {
                        MpPlatform.Services.Cursor.UnsetCursor(null);
                    } else {
                        var hctvm = Items.FirstOrDefault(x => x.IsHovering);
                        if (hctvm != null) {
                            hctvm.OnPropertyChanged(nameof(hctvm.TileBorderHexColor));
                        }
                        if (IsAnyBusy) {
                            OnPropertyChanged(nameof(IsBusy));
                        }
                    }
                    break;
                case nameof(DefaultItemSize):
                case nameof(DefaultItemWidth):
                case nameof(DefaultItemHeight):
                    if (!MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening &&
                        MpAvMainWindowViewModel.Instance.IsMainWindowOpening) {
                        // since spring animation is clamped along screen edge when 
                        // it springs mw stretches and makes tiles bounce so reject tile 
                        // update because size will return to original
                        // (its kinda cool but is too much processing)
                        break;
                    }
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.MinSize)));
                    break;

                    //case nameof(IsAnyTileDragging):
                    //    MpAvMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpAvMainWindowViewModel.Instance.IsAnyItemDragging));
                    //    break;
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


        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                // CONTENT RESIZE
                case MpMessageType.ContentResized:
                    RefreshQueryTrayLayout();
                    LockScrollToAnchor();
                    CheckLoadMore();
                    SetScrollAnchor();
                    break;
                // LAYOUT CHANGE
                case MpMessageType.TrayLayoutChanged:
                    RefreshQueryTrayLayout();
                    if (LayoutType == MpClipTrayLayoutType.Grid) {

                        SetScrollAnchor(true);
                        QueryCommand.Execute(_anchor_query_idx);
                    } else {
                        LockScrollToAnchor();
                        CheckLoadMore();
                    }
                    break;
                // MAIN WINDOW SIZE
                case MpMessageType.MainWindowSizeChangeBegin:
                    SetScrollAnchor();
                    break;
                case MpMessageType.MainWindowSizeChanged:
                    RefreshQueryTrayLayout();
                    LockScrollToAnchor();
                    CheckLoadMore();
                    break;
                case MpMessageType.MainWindowSizeChangeEnd:
                    // NOTE Size reset doesn't call changed so treat end as changed too
                    RefreshQueryTrayLayout();
                    LockScrollToAnchor();
                    CheckLoadMore();
                    SetScrollAnchor();
                    break;

                // MAIN WINDOW ORIENTATION
                case MpMessageType.MainWindowOrientationChangeBegin:
                    _isMainWindowOrientationChanging = true;
                    SetScrollAnchor();
                    break;
                case MpMessageType.MainWindowOrientationChangeEnd:
                    _isMainWindowOrientationChanging = false;
                    OnPropertyChanged(nameof(ListOrientation));
                    RefreshQueryTrayLayout();
                    LockScrollToAnchor();
                    break;

                // TRAY ZOOM
                case MpMessageType.TrayZoomFactorChangeBegin:
                    MpConsole.WriteLine("Zoom change begin: " + ZoomFactor);
                    SetScrollAnchor();
                    break;
                case MpMessageType.TrayZoomFactorChanged:
                    MpConsole.WriteLine("Zoom changed: " + ZoomFactor);
                    RefreshQueryTrayLayout();
                    LockScrollToAnchor();
                    CheckLoadMore(true);
                    break;
                case MpMessageType.TrayZoomFactorChangeEnd:
                    MpConsole.WriteLine("Zoom change end: " + ZoomFactor);
                    RefreshQueryTrayLayout();
                    LockScrollToAnchor();
                    CheckLoadMore(true);

                    SetScrollAnchor();
                    break;

                // SCROLL JUMP

                case MpMessageType.JumpToIdxCompleted:
                    RefreshQueryTrayLayout();
                    //LockScrollToAnchor();
                    CheckLoadMore();

                    //SetScrollAnchor();
                    break;

                // SELECTION

                case MpMessageType.TraySelectionChanged:
                    OnPropertyChanged(nameof(CanScroll));
                    break;

                case MpMessageType.TrayScrollChanged:
                    if (_isMainWindowOrientationChanging) {
                        break;
                    }
                    CheckLoadMore();
                    break;
                case MpMessageType.MainWindowOpened:
                    if (SelectedItem == null) {
                        ResetClipSelection(false);
                    }
                    AddNewItemsCommand.Execute(null);
                    break;
            }
        }
        public void ClearClipEditing() {
            foreach (var ctvm in Items) {
                if (ctvm == null) {
                    //occurs on first hide w/ async virtal items
                    continue;
                }
                ctvm.ClearEditing();
            }
        }


        public void ClearClipSelection(bool clearEditing = true) {
            //Dispatcher.UIThread.Post((Action)(() => {
            if (clearEditing) {
                ClearClipEditing();
            }
            foreach (var ctvm in Items) {
                if (ctvm == null) {
                    //occurs on first hide w/ async virtal items
                    continue;
                }
                ctvm.ClearSelection();
            }

            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Clear();
            //}));
        }
        public void ClearAllSelection(bool clearEditing = true) {
            //Dispatcher.UIThread.Post((Action)(() => {
            if (clearEditing) {
                ClearClipEditing();
            }
            ClearClipSelection();
            //}));
        }

        public void ResetClipSelection(bool clearEditing = true) {
            IsSelectionReset = true;
            ClearClipSelection(clearEditing);

            SelectedItem = HeadItem;
            //Items[0].IsSelected = true;
            //if (!MpAvSearchBoxViewModel.Instance.IsTextBoxFocused) {
            //    RequestFocus(SelectedItems[0]);
            //}

            RequestScrollToHome();

            //});
            IsSelectionReset = false;
        }

        #endregion

        #region Commands

        public ICommand UpdateTileRectCommand => new MpCommand<object>(
            (args) => {
                MpAvClipTileViewModel ctvm = null;
                MpRect prevOffsetRect = null;
                if (args is MpAvClipTileViewModel) {
                    ctvm = args as MpAvClipTileViewModel;
                } else if (args is object[] argParts) {
                    ctvm = argParts[0] as MpAvClipTileViewModel;
                    prevOffsetRect = argParts[1] as MpRect;
                }
                if (ctvm == null) {
                    return;
                }
                var trayRect = FindTileRect(ctvm.QueryOffsetIdx, prevOffsetRect);
                ctvm.TrayLocation = trayRect.Location;
            });

        public ICommand ToggleLayoutTypeCommand => new MpCommand(
            () => {
                //ScrollToHomeCommand.Execute(null);
                SetScrollAnchor();

                if (IsGridLayout) {
                    LayoutType = MpClipTrayLayoutType.Grid;
                } else {
                    LayoutType = MpClipTrayLayoutType.Stack;
                }
                MpMessenger.SendGlobal(MpMessageType.TrayLayoutChanged);
            });

        

        public ICommand ResetZoomFactorCommand => new MpCommand(
            () => {
                ZoomFactor = 1.0d;
            });

       

        public ICommand DuplicateSelectedClipsCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;
                var clonedCopyItem = (MpCopyItem)await SelectedItem.CopyItem.Clone(true);

                await clonedCopyItem.WriteToDatabaseAsync();
                _newModels.Add(clonedCopyItem);

                AddNewItemsCommand.Execute(true);

                IsBusy = false;
            }, () => SelectedItem != null);

        public ICommand AddNewItemsCommand => new MpAsyncCommand<object>(
            async (tagDropCopyItemOnlyArg) => {
                IsPinTrayBusy = true;

                int selectedId = -1;
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLocked && SelectedItem != null) {
                    selectedId = SelectedItem.CopyItemId;
                }
                for (int i = 0; i < _newModels.Count; i++) {
                    var ci = _newModels[i];
                    MpAvClipTileViewModel nctvm = null;
                    if (ci.WasDupOnCreate) {
                        nctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ci.Id);
                    }
                    if (nctvm == null) {
                        nctvm = await CreateClipTileViewModel(ci);
                    }
                    while (nctvm.IsBusy) {
                        await Task.Delay(100);
                    }
                    ToggleTileIsPinnedCommand.Execute(nctvm);
                }

                _newModels.Clear();
                while (IsAnyBusy) {
                    await Task.Delay(100);
                }
                if (selectedId >= 0) {
                    var selectedVm = AllItems.FirstOrDefault(x => x.CopyItemId == selectedId);
                    if (selectedVm != null) {
                        selectedVm.IsSelected = true;
                    }
                }
                IsPinTrayBusy = false;
                //using tray scroll changed so tile drop behaviors update their drop rects
            },
            (tagDropCopyItemOnlyArg) => {
                if (tagDropCopyItemOnlyArg is MpCopyItem tag_drop_ci) {
                    if (_newModels.Any(x => x.Id == tag_drop_ci.Id)) {
                        // should only happen once from drop in tag view
                        Debugger.Break();
                    } else {
                        _newModels.Add(tag_drop_ci);
                    }
                }
                if (_newModels.Count == 0) {
                    return false;
                }
                //if(!string.IsNullOrEmpty(MpAvSearchBoxViewModel.Instance.LastSearchText)) {
                //    return false;
                //}
                //if (MpPlatform.Services.QueryInfo.SortType == MpContentSortType.Manual) {
                //    return false;
                //}
                if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    return true;
                }
                return false;
            });

        public ICommand QueryCommand => new MpCommand<object>(
            (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) => {
                Dispatcher.UIThread.Post(async () => {
                    IsBusy = true;
                    IsRequery = true;
                    var sw = new Stopwatch();
                    sw.Start();

                    #region Gather Args

                    bool isSubQuery = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg != null;
                    bool isScrollJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is MpPoint;
                    bool isOffsetJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is int;
                    bool isLoadMore = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is bool;
                    bool isInPlaceRequery = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is string;
                    bool isRequery = !isSubQuery;

                    int loadOffsetIdx = 0;
                    int loadCount = 0;

                    bool isLoadMoreTail = false;

                    MpPoint newScrollOffset = default;

                    #endregion

                    #region TotalCount Query & Offset Calc

                    if (isSubQuery) {
                        // sub-query of visual, data-specific or incremental offset 

                        if (isOffsetJump) {
                            // sub-query to data-specific (query Idx) offset

                            loadOffsetIdx = (int)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg;
                            var loadTileRect = FindTileRect(loadOffsetIdx, null);
                            newScrollOffset = loadTileRect.Location;
                        } else if (isScrollJump) {
                            // sub-query to visual (scroll position) offset 

                            newScrollOffset = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg as MpPoint;
                            loadOffsetIdx = FindJumpTileIdx(newScrollOffset.X, newScrollOffset.Y, out MpRect offsetTileRect);
                            newScrollOffset = offsetTileRect.Location;
                        } else if (isLoadMore) {
                            // sub-query either forward (true) or backward (false) based on current offset

                            loadCount = LoadMorePageSize;

                            if ((bool)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) {
                                //load More to tail
                                isLoadMoreTail = true;
                                loadOffsetIdx = TailQueryIdx + 1;
                                if (loadOffsetIdx > MaxClipTrayQueryIdx) {
                                    IsRequery = IsBusy = false;
                                    return;
                                }
                            } else {
                                //load more to head
                                isLoadMoreTail = false;
                                loadOffsetIdx = HeadQueryIdx - loadCount;
                                if (loadOffsetIdx < MinClipTrayQueryIdx) {
                                    IsRequery = IsBusy = false;
                                    return;
                                }
                            }
                        } else if (isInPlaceRequery) {
                            newScrollOffset = ScrollOffset;
                            loadOffsetIdx = HeadQueryIdx;
                        }
                    } else {
                        // new query all content and offsets are re-initialized
                        ClearClipSelection();

                        // trigger unload event to wipe js eval's that maybe pending 
                        Items.Where(x => !x.IsPlaceholder).ForEach(x => x.TriggerUnloadedNotification());

                        MpAvPersistentClipTilePropertiesHelper.ClearPersistentWidths();
                    }

                    if (isRequery || isInPlaceRequery) {
                        await MpPlatform.Services.Query.QueryForTotalCountAsync();

                        FindTotalTileSize();

                        OnPropertyChanged(nameof(TotalTilesInQuery));
                        OnPropertyChanged(nameof(QueryTrayTotalWidth));
                        OnPropertyChanged(nameof(MaxScrollOffsetX));
                        OnPropertyChanged(nameof(MaxScrollOffsetY));
                        OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                        OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
                    }

                    loadOffsetIdx = Math.Max(0, loadOffsetIdx);

                    if (loadCount == 0) {
                        // is not an LoadMore Query
                        loadCount = Math.Min(DefaultLoadCount, TotalTilesInQuery);
                    } else if (loadOffsetIdx < 0) {
                        loadCount = 0;
                    }

                    if (loadOffsetIdx + loadCount > MaxClipTrayQueryIdx) {
                        // clamp load offset to max query total count
                        loadOffsetIdx = MaxLoadQueryIdx;
                    }

                    #endregion

                    #region Normalize Items By Load Count
                    // make list of select idx's
                    List<int> fetchQueryIdxList = Enumerable.Range(loadOffsetIdx, loadCount).ToList();
                    if (!isLoadMore) {
                        // Cleanup Tray item count depending on last query 
                        int itemCountDiff = Items.Count - fetchQueryIdxList.Count;
                        if (itemCountDiff > 0) {
                            while (itemCountDiff > 0) {
                                // keep unneeded items as placeholders (maybe good to cap to some limit...)
                                //Items.RemoveAt(0);
                                Items[--itemCountDiff].TriggerUnloadedNotification();
                            }
                        } else if (itemCountDiff < 0) {
                            while (itemCountDiff < 0) {
                                var ctvm = await CreateClipTileViewModel(null);
                                Items.Add(ctvm);
                                itemCountDiff++;
                            }
                        }
                    }
                    #endregion

                    #region Fetch Data & Create Init Tasks

                    var cil = await MpPlatform.Services.Query.FetchIdsByQueryIdxListAsync(fetchQueryIdxList);

                    int recycle_base_query_idx = isLoadMoreTail ? HeadQueryIdx : TailQueryIdx;
                    int dir = isLoadMoreTail ? 1 : -1;
                    List<Task> initTasks = new List<Task>();
                    //MpAvClipTileViewModel ctvm_needs_restore = null;

                    for (int i = 0; i < cil.Count; i++) {
                        MpAvClipTileViewModel cur_ctvm = null;
                        if (isLoadMore) {
                            int cur_query_idx = recycle_base_query_idx + (dir * i);
                            cur_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == cur_query_idx);
                        } else {
                            cur_ctvm = Items[i];
                        }
                        if (cur_ctvm == null || (isLoadMore && cur_ctvm.IsAnyQueryCornerVisible)) {
                            //Debugger.Break();
                            cur_ctvm = new MpAvClipTileViewModel(this);
                            Items.Add(cur_ctvm);
                        }

                        if (cur_ctvm.IsSelected) {
                            StoreSelectionState(cur_ctvm);
                            cur_ctvm.ClearSelection();
                        }
                        bool needsRestore = false;
                        if (isSubQuery && MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Any(x => x.Id == cil[i].Id)) {
                            needsRestore = true;
                        }
                        initTasks.Add(cur_ctvm.InitializeAsync(cil[i], fetchQueryIdxList[i], needsRestore));


                    }

                    #endregion

                    #region Initialize Items

                    Task.WhenAll(initTasks).FireAndForgetSafeAsync();

                    #endregion

                    #region Finalize State & Measurements


                    OnPropertyChanged(nameof(TotalTilesInQuery));
                    OnPropertyChanged(nameof(QueryTrayTotalTileWidth));
                    OnPropertyChanged(nameof(QueryTrayTotalWidth));
                    OnPropertyChanged(nameof(MaxScrollOffsetX));
                    OnPropertyChanged(nameof(MaxScrollOffsetY));
                    OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                    OnPropertyChanged(nameof(IsVerticalScrollBarVisible));

                    if (Items.Where(x => !x.IsPlaceholder).Count() == 0) {
                        ScrollOffsetX = 0;
                        LastScrollOffsetX = 0;
                        ScrollOffsetY = 0;
                        LastScrollOffsetY = 0;
                    }

                    IsBusy = false;
                    IsRequery = false;
                    OnPropertyChanged(nameof(IsAnyBusy));
                    OnPropertyChanged(nameof(IsTrayEmpty));


                    sw.Stop();
                    MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);

                    if (isRequery) {
                        //_scrollOffset = LastScrollOffsetX = 0;
                        //ForceScrollOffset(MpPoint.Zero);
                        MpMessenger.SendGlobal(MpMessageType.RequeryCompleted);

                        if (SelectedItem == null &&
                            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count == 0 &&
                            TotalTilesInQuery > 0) {
                            Dispatcher.UIThread.Post(async () => {
                                while (IsAnyBusy) {
                                    await Task.Delay(100);
                                }
                                ResetClipSelection();
                            });
                        }
                    } else {

                        if (isOffsetJump || isScrollJump || isInPlaceRequery) {
                            ForceScrollOffset(newScrollOffset);
                            MpMessenger.SendGlobal(MpMessageType.JumpToIdxCompleted);
                        } else {
                            //recheck loadMore once done for rejected scroll change events
                            while (IsAnyBusy) {
                                await Task.Delay(100);
                            }
                            if (isLoadMore) {
                                CheckLoadMore();
                            } else {
                                RefreshQueryTrayLayout();
                            }
                        }
                    }

                    if (isRequery || isInPlaceRequery) {
                        UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();
                    }
                    #endregion
                });
            },
            (offsetIdx_Or_ScrollOffset_Arg) => {
                return !IsAnyBusy && !IsRequery;
            });


        public ICommand SearchWebCommand => new MpCommand<object>(
            (args) => {
                string pt = string.Join(
                            Environment.NewLine,
                            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(x => x.ItemData.RtfToPlainText()));

                //MpHelpers.OpenUrl(args.ToString() + Uri.EscapeDataString(pt));
            }, (args) => args != null && args is string);


        public ICommand ChangeSelectedClipsColorCommand => new MpCommand<object>(
             (hexStrOrBrush) => {
                 string hexStr = string.Empty;
                 if (hexStrOrBrush is Brush b) {
                     hexStr = b.ToHex();
                 } else if (hexStrOrBrush is string) {
                     hexStr = (string)hexStrOrBrush;
                 }
                 SelectedItem.ChangeColorCommand.Execute(hexStr.ToString());
             });

        public ICommand CopySelectedClipsCommand => new MpCommand(
            () => {
                SelectedItem.CopyToClipboardCommand.Execute(null);
            },
            () => {
                bool canCopy =
                    SelectedItem != null &&
                    MpAvMainWindowViewModel.Instance.IsMainWindowActive;
                MpConsole.WriteLine("CopySelectedClipsCommand CanExecute: " + canCopy);
                if (!canCopy) {
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));
                    MpConsole.WriteLine("IsMainWindowActive: " + MpAvMainWindowViewModel.Instance.IsMainWindowActive);
                }
                return canCopy;
            });

        public ICommand CutSelectionFromContextMenuCommand => new MpCommand<object>(
            (args) => {
                MpAvShortcutCollectionViewModel.Instance.SimulateKeyStrokeCommand
                    .Execute(MpPlatform.Services.PlatformShorcuts.CutKeys);
            },
            (args) => {
                return SelectedItem != null && SelectedItem.IsSubSelectionEnabled;
            });

        public ICommand PasteSelectedClipTileFromShortcutCommand => new MpCommand<object>(
            (args) => {
                bool fromEditorButton = false;
                if (args is bool) {
                    fromEditorButton = (bool)args;
                }
                PasteClipTileAsync(SelectedItem).FireAndForgetSafeAsync();
            },
            (args) => {
                return MpAvMainWindowViewModel.Instance.IsAnyDialogOpen == false &&
                    SelectedItem != null &&
                    MpAvMainWindowViewModel.Instance.IsMainWindowActive &&
                    !MpAvMainWindowViewModel.Instance.IsAnyMainWindowTextBoxFocused &&
                    !MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen &&
                    !IsAnyEditingClipTile &&
                    !IsAnyEditingClipTitle &&
                    !MpPrefViewModel.Instance.IsTrialExpired;
            });
        public ICommand PasteSelectedClipTileFromContextMenuCommand => new MpCommand<object>(
            (args) => {
                bool fromEditorButton = false;
                if (args is bool) {
                    fromEditorButton = (bool)args;
                }
                PasteClipTileAsync(SelectedItem).FireAndForgetSafeAsync();
            },
            (args) => {
                return SelectedItem != null;
            });

        public ICommand PasteHereFromContextMenuCommand => new MpCommand<object>(
            (args) => {
                MpAvShortcutCollectionViewModel.Instance.SimulateKeyStrokeCommand
                    .Execute(MpPlatform.Services.PlatformShorcuts.PasteKeys);
            },
            (args) => {
                return SelectedItem != null && SelectedItem.IsSubSelectionEnabled;
            });

        public ICommand PasteFromClipTilePasteButtonCommand => new MpCommand<object>(
            (args) => {
                PasteClipTileAsync(args as MpAvClipTileViewModel).FireAndForgetSafeAsync();
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
                var mpdo = await MpPlatform.Services.DataObjectHelperAsync.GetPlatformClipboardDataObjectAsync(false);

                SelectedItem.RequestPastePortableDataObject(mpdo);
            }, () => SelectedItem != null && !SelectedItem.IsPlaceholder);

        public ICommand PasteCopyItemByIdCommand => new MpAsyncCommand<object>(
            async (args) => {
                await Task.Delay(1);
                //if (args is int ciid) {
                //    IsPasting = true;
                //    var pi = new MpProcessInfo() {
                //        Handle = MpProcessManager.LastHandle,
                //        ProcessPath = MpProcessManager.LastProcessPath
                //    };

                //    MpAvClipTileViewModel ctvm = GetClipTileViewModelById(ciid);
                //    if (ctvm == null) {
                //        var ci = await MpDataModelProvider.GetCopyItemByIdAsync(ciid);
                //        var templates = await MpDataModelProvider.ParseTextTemplatesByCopyItemIdAsync(ci);
                //        if (templates != null && templates.Count > 0) {
                //            // this item needs to be loaded into ui in order to paste it
                //            // trigger query change before showing main window may need to tweak...

                //            MpDataModelProvider.SetManualQuery(new List<int>() { ciid });
                //            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen == false) {
                //                MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
                //                while (MpAvMainWindowViewModel.Instance.IsMainWindowOpen == false) {
                //                    await Task.Delay(100);
                //                }
                //            }
                //            await Task.Delay(50); //wait for clip tray to get query changed message
                //            while (IsAnyBusy) {
                //                await Task.Delay(100);
                //            }
                //            ctvm = GetClipTileViewModelById(ciid);
                //        } else {
                //            ctvm = await CreateClipTileViewModel(ci);
                //        }
                //    }

                //    if (ctvm == null) {
                //        Debugger.Break();
                //    }
                //    var mpdo = await ctvm.ConvertToPortableDataObject(true);
                //    if (mpdo == null) {
                //        // paste was canceled
                //        return;
                //    }
                //    await MpPlatformWrapper.Services.ExternalPasteHandler.PasteDataObject(mpdo, pi);

                //    CleanupAfterPaste(ctvm);

                //}
            },
            (args) => args is int);


        public ICommand DeleteSelectedClipsCommand => new MpAsyncCommand(
            async () => {
                while (IsBusy) { await Task.Delay(100); }

                IsBusy = true;

                //await MpDataModelProvider.RemoveQueryItem(PrimaryItem.PrimaryItem.CopyItemId);


                await Task.WhenAll(SelectedModels.Select(x => x.DeleteFromDatabaseAsync()));

                //db delete event is handled in clip tile
                IsBusy = false;
            },
            () => {
                return MpAvMainWindowViewModel.Instance.IsAnyDialogOpen == false &&
                        MpAvMainWindowViewModel.Instance.IsMainWindowActive &&
                        SelectedModels.Count > 0 &&
                        !IsAnyEditingClipTile &&
                        !IsAnyEditingClipTitle;
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


                await ctvm.InitTitleLayers();

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

        public ICommand AssignHotkeyCommand => new MpCommand(
            () => {
                MpAvShortcutCollectionViewModel.Instance.ShowAssignShortcutDialogCommand.Execute(SelectedItem);
            },
            () => SelectedItem != null);


        public ICommand EditSelectedTitleCommand => new MpCommand(
            () => {
                SelectedItem.IsTitleReadOnly = false;
            },
            () => {
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return false;
                }
                return SelectedItem != null;
            });

        public ICommand EditSelectedContentCommand => new MpAsyncCommand(
            async () => {
                ClearAllEditing();
                if (SelectedItem.IsSubSelectionEnabled) {
                    // BUG FIX when spacebar is shortcut to edit and sub-selection is enabled
                    // the space is passed to the editor so pausing toggling for space to get out ur system
                    await Task.Delay(DISABLE_READ_ONLY_DELAY_MS);
                }
                SelectedItem.ToggleEditContentCommand.Execute(null);
            },
            () => SelectedItem != null && SelectedItem.IsContentReadOnly);


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

                var preset = await MpDataModelProvider.GetItemAsync<MpPluginPreset>(presetId);
                var analyticItemVm = MpAvAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.PluginGuid == preset.PluginGuid);
                int selected_ciid = SelectedItem.CopyItemId;

                EventHandler<MpCopyItem> analysisCompleteHandler = null;
                analysisCompleteHandler = (s, e) => {
                    analyticItemVm.OnAnalysisCompleted -= analysisCompleteHandler;
                    AllItems.FirstOrDefault(x => x.CopyItemId == selected_ciid).IsBusy = false;

                    if (e == null) {
                        return;
                    }
                    AddUpdateOrAppendCopyItemAsync(e).FireAndForgetSafeAsync();
                };

                var presetVm = analyticItemVm.Items.FirstOrDefault(x => x.Preset.Id == preset.Id);

                analyticItemVm.SelectPresetCommand.Execute(presetVm);
                if (analyticItemVm.ExecuteAnalysisCommand.CanExecute(null)) {
                    AllItems.FirstOrDefault(x => x.CopyItemId == selected_ciid).IsBusy = true;
                    analyticItemVm.OnAnalysisCompleted += analysisCompleteHandler;
                    analyticItemVm.ExecuteAnalysisCommand.Execute(null);
                }
            });




        public ICommand EnableFindAndReplaceForSelectedItem => new MpCommand(
            () => {
                SelectedItem.IsFindAndReplaceVisible = true;
            }, () => SelectedItem != null && !SelectedItem.IsFindAndReplaceVisible && SelectedItem.IsTextItem);



        //public ICommand SpeakSelectedClipsCommand => new MpAsyncCommand(
        //    async () => {
        //        await Task.Delay(1);
        //await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
        //    var speechSynthesizer = new SpeechSynthesizer();
        //    speechSynthesizer.SetOutputToDefaultAudioDevice();
        //    if (string.IsNullOrEmpty(MpPrefViewModel.Instance.SpeechSynthVoiceName)) {
        //        speechSynthesizer.SelectVoice(speechSynthesizer.GetInstalledVoices()[0].VoiceInfo.Name);
        //    } else {
        //        speechSynthesizer.SelectVoice(MpPrefViewModel.Instance.SpeechSynthVoiceName);
        //    }
        //    speechSynthesizer.Rate = 0;
        //    speechSynthesizer.SpeakCompleted += (s, e) => {
        //        speechSynthesizer.Dispose();
        //    };
        //    // Create a PromptBuilder object and append a text string.
        //    PromptBuilder promptBuilder = new PromptBuilder();

        //    promptBuilder.AppendText(Environment.NewLine + SelectedItem.CopyItem.ItemData.ToPlainText());

        //    // Speak the contents of the prompt asynchronously.
        //    speechSynthesizer.SpeakAsync(promptBuilder);

        //}, DispatcherPriority.Background);
        //},
        //() => {
        //    return SelectedItem != null && SelectedItem.IsTextItem;
        //});

        //public ICommand SendToEmailCommand => new MpCommand(
        //    () => {
        //        // for gmail see https://stackoverflow.com/a/60741242/105028
        //        string pt = string.Join(Environment.NewLine, MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
        //        //MpHelpers.OpenUrl(
        //        //    string.Format("mailto:{0}?subject={1}&body={2}",
        //        //    string.Empty, SelectedItem.CopyItem.Title,
        //        //    pt));
        //        //MpAvClipTrayViewModel.Instance.ClearClipSelection();
        //        //IsSelected = true;
        //        //MpHelpers.CreateEmail(MpJsonPreferenceIO.Instance.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        //    },
        //    () => {
        //        return !IsAnyEditingClipTile && SelectedItem != null;
        //    });


        //new MpMenuItemViewModel() {
        //    Header = "Merge",
        //                            AltNavIdx = 0,
        //                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("MergeImage") as string,
        //                            Command = MergeSelectedClipsCommand,
        //                            ShortcutArgs = new object[] { MpShortcutType.MergeSelectedItems },
        //                        },
        //                        new MpMenuItemViewModel() {
        //    Header = "To Email",
        //                            AltNavIdx = 3,
        //                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("EmailImage") as string,
        //                            Command = SendToEmailCommand,
        //                            ShortcutArgs = new object[] { MpShortcutType.SendToEmail },
        //                        },
        //                        new MpMenuItemViewModel() {
        //    Header = "To Qr Code",
        //                            AltNavIdx = 3,
        //                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("QrImage") as string,
        //                            Command = CreateQrCodeFromSelectedClipsCommand,
        //                            ShortcutArgs = new object[] { MpShortcutType.CreateQrCode },
        //                        },
        //                        new MpMenuItemViewModel() {
        //    Header = "To Audio",
        //                            AltNavIdx = 3,
        //                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SpeakImage") as string,
        //                            Command = SpeakSelectedClipsCommand,
        //                            ShortcutArgs = new object[] { MpShortcutType.SpeakSelectedItem },
        //                        },


        #endregion
    }
}
