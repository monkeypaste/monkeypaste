using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FocusManager = Avalonia.Input.FocusManager;

namespace MonkeyPaste.Avalonia {
    public enum MpPinType {
        None = 0,
        Internal,
        Window,
        Append
    }
    public class MpAvClipTrayViewModel :
        MpAvSelectorViewModelBase<object, MpAvClipTileViewModel>,
        MpIBootstrappedItem,
        MpIPagingScrollViewerViewModel,
        MpIActionComponent,
        MpIBoundSizeViewModel,
        MpIContextMenuViewModel,
        MpIContentQueryPage,
        MpIProgressIndicatorViewModel {
        #region Private Variables

        private int _anchor_query_idx { get; set; } = -1;
        private bool _isMainWindowOrientationChanging = false;

        #endregion

        #region Constants


        public const int DISABLE_READ_ONLY_DELAY_MS = 500;
        public const double MAX_TILE_SIZE_CONTAINER_PAD = 50;
        public const double MIN_SIZE_ZOOM_FACTOR_COEFF = (double)1 / (double)7;
        public const double EDITOR_TOOLBAR_MIN_WIDTH = 830.0d;
        public const double DEFAULT_ITEM_SIZE = 260;

        #endregion

        #region Statics

        private static MpAvClipTrayViewModel _instance;
        public static MpAvClipTrayViewModel Instance => _instance ?? (_instance = new MpAvClipTrayViewModel());


        public static string EditorUri {
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
                //                string uri;

                //#if DESKTOP

                //uri = MpAvCefNetApplication.GetEditorPath().ToFileSystemUriFromPath();
                //#else
                //                uri = System.IO.Path.Combine(Mp.Services.PlatformInfo.StorageDir, "MonkeyPaste.Editor", "index.html").ToFileSystemUriFromPath();
                //#endif
                //                return uri;
                string uri;
                if (OperatingSystem.IsBrowser()) {
                    uri = Mp.Services.PlatformInfo.EditorPath;
                } else {
                    uri = Mp.Services.PlatformInfo.EditorPath.ToFileSystemUriFromPath();
                }
                return uri;
            }
        }

        #endregion

        #region Interfaces

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
            PinnedItems.Select(x => x.CopyItemId);
        int MpIContentQueryPage.Offset =>
            HeadQueryIdx;

        int MpIContentQueryPage.Limit =>
            TailQueryIdx;

        IEnumerable<int> MpIContentQueryPage.ContentIds =>
            QueryItems
            .OrderBy(x => x.QueryOffsetIdx)
            .Select(x => x.CopyItemId);

        #endregion

        #region MpIBoostrappedItem Implementation

        string MpIBootstrappedItem.Label => "Content Tray";
        #endregion

        #region MpIContextMenuItemViewModel Implementation
        public MpMenuItemViewModel ContextMenuViewModel {
            get {
                if (SelectedItem == null) {
                    return new MpMenuItemViewModel();
                }
                //if(SelectedItem.IsTableSelected) {
                //    return SelectedItem.TableViewModel.ContextMenuViewModel;
                //}
                if (SelectedItem.IsHoveringOverSourceIcon) {
                    return SelectedItem.TransactionCollectionViewModel.ContextMenuViewModel;
                }
                if (MpAvTagTrayViewModel.Instance.IsAnyBusy) {
                    Debugger.Break();
                }
                var tagItems = MpAvTagTrayViewModel.Instance.AllTagViewModel.ContentMenuItemViewModel.SubItems;
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = @"Show Dev Tools",
                            Command = ShowDevToolsCommand,
                            IsVisible =
#if DEBUG
                            true,
#else
                            false,
#endif
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Cut",
                            IconResourceKey = "ScissorsImage",
                            Command = CutSelectionFromContextMenuCommand,
                            CommandParameter = true,
                            ShortcutArgs = new object[] { MpShortcutType.CutSelection },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Copy",
                            IconResourceKey = "CopyImage",
                            Command = CopySelectionFromContextMenuCommand,
                            ShortcutArgs = new object[] { MpShortcutType.CopySelection },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Paste Here",
                            AltNavIdx = 6,
                            IconResourceKey = "PasteImage",
                            Command = PasteHereFromContextMenuCommand,
                            ShortcutArgs = new object[] { MpShortcutType.PasteHere },
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = $"Paste To '{MpAvAppCollectionViewModel.Instance.LastActiveAppViewModel.AppName}'",
                            AltNavIdx = 0,
                            IconId = MpAvAppCollectionViewModel.Instance.LastActiveAppViewModel.IconId,
                            Command = PasteSelectedClipTileFromContextMenuCommand,
                            ShortcutArgs = new object[] { MpShortcutType.PasteSelectedItems },
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Delete",
                            AltNavIdx = 0,
                            IconResourceKey = "DeleteImage",
                            Command = DeleteSelectedClipsCommand,
                            ShortcutArgs = new object[] { MpShortcutType.DeleteSelectedItems },
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Rename",
                            AltNavIdx = 0,
                            IconResourceKey = "RenameImage",
                            Command = EditSelectedTitleCommand,
                            ShortcutArgs = new object[] { MpShortcutType.EditTitle },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Edit",
                            AltNavIdx = 0,
                            IconResourceKey = "EditContentImage",
                            Command = EditSelectedContentCommand,
                            ShortcutArgs = new object[] { MpShortcutType.EditContent },
                        },
                        new MpMenuItemViewModel() {
                            Header = SelectedItem.IsPinned ? "Un-pin":"Pin",
                            AltNavIdx = 0,
                            IconResourceKey = "PinImage",
                            Command = ToggleSelectedTileIsPinnedCommand,
                            ShortcutArgs = new object[] { MpShortcutType.TogglePinned },
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Transform",
                            IconResourceKey = "ToolsImage",
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = @"Find and Replace",
                                    AltNavIdx = 0,
                                    IconResourceKey = "SearchImage",
                                    Command = EnableFindAndReplaceForSelectedItem,
                                    ShortcutArgs = new object[] { MpShortcutType.FindAndReplaceSelectedItem },
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Duplicate",
                                    AltNavIdx = 0,
                                    IconResourceKey = "DuplicateImage",
                                    Command = DuplicateSelectedClipsCommand,
                                    ShortcutArgs = new object[] { MpShortcutType.Duplicate },
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To Web Search",
                                    IconResourceKey = "WebImage",
                                    SubItems = new List<MpMenuItemViewModel>() {
                                        new MpMenuItemViewModel() {
                                            Header = "Google",
                                            AltNavIdx = 0,
                                            IconResourceKey = "GoogleImage",
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.google.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "Bing",
                                            AltNavIdx = 0,
                                            IconResourceKey = "BingImage",
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.bing.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "DuckDuckGo",
                                            AltNavIdx = 0,
                                            IconResourceKey = "DuckImage",
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://duckduckgo.com/?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "Yandex",
                                            AltNavIdx = 0,
                                            IconResourceKey = "YandexImage",
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://yandex.com/search/?text="
                                        },
                                        new MpMenuItemViewModel() { IsSeparator = true},
                                        new MpMenuItemViewModel() {
                                            Header = "Manage...",
                                            AltNavIdx = 0,
                                            IconResourceKey = "CogImage"
                                        },
                                    }
                                }
                            }
                        },
                        //SelectedItem.TransactionCollectionViewModel.ContextMenuViewModel,
                        MpAvAnalyticItemCollectionViewModel.Instance.GetContentContextMenuItem(SelectedItem.CopyItemType),
                        new MpMenuItemViewModel() {IsSeparator = true},
                        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(SelectedItem),
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                            Header = @"Collections",
                            AltNavIdx = 0,
                            IconResourceKey = "PinToCollectionImage",
                            SubItems = tagItems
                        }
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

        public double ScrollWheelDampeningX {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 0.03d;
                }
                return 0.01d;
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

        public Orientation ListOrientation =>
            MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ? Orientation.Horizontal : Orientation.Vertical;

        public bool IsQueryHorizontalScrollBarVisible => true;// QueryTrayTotalTileWidth > ObservedQueryTrayScreenWidth;
        public bool IsQueryVerticalScrollBarVisible => true;// QueryTrayTotalTileHeight > ObservedQueryTrayScreenHeight;

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
                double maxScrollOffsetX = Math.Max(0, QueryTrayTotalTileWidth - ObservedQueryTrayScreenWidth);
                return maxScrollOffsetX;
            }
        }
        public double MaxScrollOffsetY {
            get {
                double maxScrollOffsetY = Math.Max(0, QueryTrayTotalTileHeight - ObservedQueryTrayScreenHeight);
                return maxScrollOffsetY;
            }
        }

        public MpRect QueryTrayScreenRect =>
            new MpRect(0, 0, ObservedQueryTrayScreenWidth, ObservedQueryTrayScreenHeight);


        public double QueryTrayTotalTileWidth { get; private set; }
        public double QueryTrayTotalTileHeight { get; private set; }

        public double QueryTrayTotalWidth =>
            Math.Max(0, Math.Max(ObservedQueryTrayScreenWidth, QueryTrayTotalTileWidth));
        public double QueryTrayTotalHeight =>
            Math.Max(0, Math.Max(ObservedQueryTrayScreenHeight, QueryTrayTotalTileHeight));


        public double QueryTrayFixedDimensionLength =>
            ListOrientation == Orientation.Horizontal ?
                ObservedQueryTrayScreenHeight : ObservedQueryTrayScreenWidth;

        public double PinTrayFixedDimensionLength =>
            ListOrientation == Orientation.Horizontal ?
                ObservedPinTrayScreenHeight : ObservedPinTrayScreenWidth;
        public double LastZoomFactor { get; set; }

        public double DefaultZoomFactor { get; set; } = 1.0;

        private double _zoomFactor = 1.0;
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
        public double MinZoomFactor => 0.25;
        public double MaxZoomFactor => 3;
        public double ScrollVelocityX { get; set; }
        public double ScrollVelocityY { get; set; }

        public double ScrollFrictionX =>
            Mp.Services.PlatformInfo.IsDesktop ?
            0.85 : 0.3;

        public double ScrollFrictionY =>
            Mp.Services.PlatformInfo.IsDesktop ?
            0.75 : 0.0;

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

        public bool CanScroll {
            get {
                //return true;

                if (MpAvMainWindowViewModel.Instance.IsMainWindowOpening ||
                   !MpAvMainWindowViewModel.Instance.IsMainWindowOpen ||
                    IsRequerying/* ||
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

        public bool CanScrollX =>
            CanScroll && (LayoutType == MpClipTrayLayoutType.Grid || ListOrientation == Orientation.Horizontal);

        public bool CanScrollY =>
            CanScroll && (LayoutType == MpClipTrayLayoutType.Grid || ListOrientation == Orientation.Vertical);
        public bool IsThumbDraggingX { get; set; } = false;
        public bool IsThumbDraggingY { get; set; } = false;
        public bool IsThumbDragging => IsThumbDraggingX || IsThumbDraggingY;

        public bool IsScrollJumping { get; set; }

        public void FindTotalTileSize() {
            // NOTE this is to avoid making TotalTile Width/Height auto
            // and should only be called on a requery or on content resize (or event better only on resize complete)
            MpSize totalTileSize = MpSize.Empty;
            if (Mp.Services.Query.TotalAvailableItemsInQuery > 0) {
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

        private object FindTileRectOrQueryIdxOrTotalTileSize_internal(int queryOffsetIdx, double scrollOffsetX, double scrollOffsetY, MpRect prevOffsetRect = null) {
            // For TotalTileSize<MpSize>: all params -1
            // For TileRect<MpRect>:  0 <= queryOffsetIdx < MpPlatform.Services.Query.TotalAvailableItemsInQuery and scrollOffsets == -1
            // For TileQueryIdx<[]{int,MpRect}>: queryoffsetIdx < 0 and both scrollOffset > 0

            bool isGrid = LayoutType == MpClipTrayLayoutType.Grid;
            bool isStack = !isGrid;

            bool isFindTileIdx = scrollOffsetX >= 0 && scrollOffsetY >= 0;
            bool isFindTileRect = !isFindTileIdx && queryOffsetIdx >= 0;
            bool isFindTotalSize = !isFindTileRect;

            int totalTileCount = Mp.Services.Query.TotalAvailableItemsInQuery;
            queryOffsetIdx = isFindTotalSize ? Mp.Services.Query.TotalAvailableItemsInQuery - 1 : queryOffsetIdx;
            if (queryOffsetIdx >= totalTileCount) {
                return null;
            }

            int startIdx = 0;// prevOffsetRect == null ? 0 : queryOffsetIdx;

            var total_size = MpSize.Empty;
            int gridFixedCount = -1;

            MpRect last_rect = null;// prevOffsetRect;

            UpdateDefaultItemSize();

            for (int i = startIdx; i <= queryOffsetIdx; i++) {
                MpSize tile_size = new MpSize(DefaultQueryItemWidth, DefaultQueryItemHeight);
                if (MpAvPersistentClipTilePropertiesHelper.TryGetUniqueWidth_ByOffsetIdx(i, out double uniqueSize)) {
                    tile_size.Width = uniqueSize;
                }

                MpPoint tile_offset;
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
                return Mp.Services.Query.TotalAvailableItemsInQuery - 1;
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



        #region Default Tile Layout 

        public double DesiredMaxTileRight {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return ObservedQueryTrayScreenWidth;
                } else {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return ObservedQueryTrayScreenWidth;
                    }
                    return double.PositiveInfinity;
                }
            }
        }

        public double DesiredMaxTileBottom {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return ObservedQueryTrayScreenHeight;
                    }
                    return double.PositiveInfinity;
                } else {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return ObservedQueryTrayScreenHeight;
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
            //double query_item_length = QueryTrayFixedDimensionLength * ZoomFactor;

            //double scrollBarSize_x = ScrollBarFixedAxisSize + (IsQueryHorizontalScrollBarVisible ? 30 : 0);
            //_defaultQueryItemWidth = Math.Clamp(query_item_length - HorizontalScrollBarHeight, 0, MaxTileWidth);

            //double scrollBarSize_y = ScrollBarFixedAxisSize + (IsQueryVerticalScrollBarVisible ? 30 : 0);
            //_defaultQueryItemHeight = Math.Clamp(query_item_length - scrollBarSize_y, 0, MaxTileHeight);

            //double query_item_length = QueryTrayFixedDimensionLength * ZoomFactor;
            //double pin_item_length = PinTrayFixedDimensionLength * ZoomFactor;
            //if (ListOrientation == Orientation.Horizontal) {
            //    _defaultQueryItemWidth = query_item_length - QueryTrayVerticalScrollBarWidth;
            //    _defaultQueryItemHeight = QueryTrayFixedDimensionLength - QueryTrayHorizontalScrollBarHeight;

            //    _defaultPinItemWidth = pin_item_length;
            //    _defaultPinItemHeight = PinTrayFixedDimensionLength;
            //} else {
            //    _defaultQueryItemWidth = QueryTrayFixedDimensionLength - QueryTrayVerticalScrollBarWidth;
            //    _defaultQueryItemHeight = query_item_length - QueryTrayHorizontalScrollBarHeight;

            //    _defaultPinItemWidth = PinTrayFixedDimensionLength;
            //    _defaultPinItemHeight = pin_item_length;
            //}
            double s = DEFAULT_ITEM_SIZE * ZoomFactor;
            _defaultQueryItemWidth = _defaultQueryItemHeight = _defaultPinItemWidth = _defaultPinItemHeight = s;
            OnPropertyChanged(nameof(DefaultQueryItemWidth));
            OnPropertyChanged(nameof(DefaultQueryItemHeight));
            OnPropertyChanged(nameof(DefaultPinItemWidth));
            OnPropertyChanged(nameof(DefaultPinItemHeight));
        }

        private double _defaultPinItemWidth;
        public double DefaultPinItemWidth =>
            _defaultPinItemWidth;

        private double _defaultPinItemHeight;
        public double DefaultPinItemHeight =>
            _defaultPinItemHeight;

        public double DefaultEditableItemWidth =>
            EDITOR_TOOLBAR_MIN_WIDTH;

        public double ScrollBarFixedAxisSize =>
            30;

        public double QueryTrayHorizontalScrollBarHeight =>
            IsQueryHorizontalScrollBarVisible ? ScrollBarFixedAxisSize : 0;

        public double QueryTrayVerticalScrollBarWidth =>
            IsQueryVerticalScrollBarVisible ? ScrollBarFixedAxisSize : 0;

        #endregion

        #region Virtual


        public int HeadQueryIdx => SortOrderedItems.Count() == 0 ? -1 : SortOrderedItems.Min(x => x.QueryOffsetIdx);

        public int TailQueryIdx => SortOrderedItems.Count() == 0 ? -1 : Items.Max(x => x.QueryOffsetIdx);
        public int FirstPlaceholderItemIdx =>
            Items.IndexOf(PlaceholderItems.FirstOrDefault());

        public int LastNonVisibleItemIdx {
            get {
                // get item closest to top left of screen
                if (VisibleItems.FirstOrDefault() is MpAvClipTileViewModel ctvm) {
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

        public void RegisterActionComponent(MpIInvokableAction mvm) {
            OnCopyItemAdd += mvm.OnActionInvoked;
            MpConsole.WriteLine($"ClipTray Registered {mvm.Label} matcher");
        }

        public void UnregisterActionComponent(MpIInvokableAction mvm) {
            OnCopyItemAdd -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"Matcher {mvm.Label} Unregistered from OnCopyItemAdded");
        }

        #endregion

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvClipTileViewModel> PinnedItems { get; set; } = new ObservableCollection<MpAvClipTileViewModel>();

        public IEnumerable<MpAvClipTileViewModel> InternalPinnedItems =>
            PinnedItems
            .Where(x => !x.IsPopOutVisible && !x.IsAppendNotifier)
            .Take(MpPrefViewModel.Instance.MaxStagedClipCount)
            .ToList();

        //public MpAvClipTileViewModel ModalClipTileViewModel { get; private set; }

        public MpAvClipTileViewModel AppendClipTileViewModel =>
            PinnedItems.FirstOrDefault(x => x.IsAppendNotifier);

        public IEnumerable<MpAvClipTileViewModel> QueryItems =>
            Items.Where(x => !x.IsPlaceholder);

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

        public int PersistantSelectedItemId {
            get {
                if (SelectedItem == null) {
                    return MpAvPersistentClipTilePropertiesHelper.GetPersistentSelectedItemId();
                }
                return SelectedItem.CopyItemId;
            }
        }

        public override MpAvClipTileViewModel SelectedItem {
            get {
                //if (MpAvAppendNotificationWindow.Instance != null &&
                //    MpAvAppendNotificationWindow.Instance.IsVisible) {
                //    // only visible if mw is not open
                //    return ModalClipTileViewModel;
                //}

                return AllItems.FirstOrDefault(x => x.IsSelected);
            }
            set {
                if (!CanSelect) {
                    return;
                }
                if (value == null) {
                    AllItems.ForEach(x => x.IsSelected = false);
                } else {
                    AllItems.ForEach(x => x.IsSelected = x.CopyItemId == value.CopyItemId);
                }
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged(nameof(SelectedPinTrayItem));
                OnPropertyChanged(nameof(SelectedClipTrayItem));
            }
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
                if (value == null || value.IsPlaceholder) {
                    // BUG trying to stop case when placeholder is being treated like
                    // init'd tile and selectionState isb being stored but 
                    // presistentSelectedModel will be null and it trips lots of things up
                    // 
                    // NOTE maybe righter to set AllItems to unselected here but not sure.
                    PinnedItems.ForEach(x => x.IsSelected = false);
                } else {
                    SelectedItem = value;
                }
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
                if (value == null || value.IsPlaceholder) {
                    // see SelectedPinTray comments
                    Items.ForEach(x => x.IsSelected = false);
                } else {
                    SelectedItem = value;
                }
                OnPropertyChanged(nameof(SelectedClipTrayItem));
            }
        }

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

        public IEnumerable<MpAvClipTileViewModel> VisibleItems =>
            Items.Where(x => x.IsAnyQueryCornerVisible && !x.IsPlaceholder);
        //Items
        //.Where(x => x.IsAnyQueryCornerVisible && !x.IsPlaceholder)
        //.OrderBy(x => x.TrayX)
        //.ThenBy(x => x.TrayY);

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

        public double DefaultPinTrayWidth =>
            DefaultQueryItemWidth * 1.4;

        public double DesiredPinTrayWidth { get; set; }
        public double DesiredPinTrayHeight { get; set; }

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
                if (ListOrientation == Orientation.Horizontal) {
                    return
                        MpAvMainWindowViewModel.Instance.MainWindowWidth -
                        MpAvSidebarItemCollectionViewModel.Instance.TotalSidebarWidth;
                }
                return
                    MpAvMainWindowViewModel.Instance.MainWindowWidth -
                    MpAvMainWindowTitleMenuViewModel.Instance.DefaultTitleMenuFixedLength;
            }
        }

        public double MaxContainerScreenHeight {
            get {
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

        public int CurGridFixedCount { get; set; }

        private MpClipTrayLayoutType? _layoutType;
        public MpClipTrayLayoutType LayoutType {
            get {
                if (MpPrefViewModel.Instance == null) {
                    return MpClipTrayLayoutType.Stack;
                }
                if (_layoutType == null) {
                    _layoutType = MpPrefViewModel.Instance.ClipTrayLayoutTypeName.ToEnum<MpClipTrayLayoutType>();
                }
                return _layoutType.Value;
            }
            set {
                if (LayoutType != value) {
                    _layoutType = value;
                    if (MpPrefViewModel.Instance != null) {
                        MpPrefViewModel.Instance.ClipTrayLayoutTypeName = value.ToString();
                    }
                    OnPropertyChanged(nameof(LayoutType));
                }
            }
        }

        #endregion

        #region Appearance

        public string EmptyQueryTrayText {
            get {
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
                    tag_name = "Untitled";
                } else {
                    tag_name = MpAvTagTrayViewModel.Instance.LastSelectedActiveItem.TagName;
                }
                return $"'{tag_name}' has no results.";
            }
        }

        #endregion

        #region State

        #region Append

        private MpAppendModeFlags _appendModeFlags = MpAppendModeFlags.None;
        public MpAppendModeFlags AppendModeStateFlags {
            get => _appendModeFlags;
            set {
                UpdateAppendModeStateFlags(value, "property");
            }
        }

        public bool IsAppendInsertMode { get; set; }
        public bool IsAppendPreMode { get; set; }

        public bool IsAppendLineMode { get; set; }

        public bool IsAppendManualMode { get; set; }

        public bool IsAnyAppendMode =>
            IsAppendInsertMode || IsAppendLineMode;

        public bool IsAppendPaused { get; set; }

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
        public string PlayOrPauseLabel => IsAppPaused ? "Resume" : "Pause";
        public string PlayOrPauseIconResoureKey => IsAppPaused ? "PlayImage" : "PauseImage";

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


        public int DefaultLoadCount {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    if (Mp.Services.PlatformInfo.IsDesktop) {
                        return 20;
                    }
                    return 5;
                } else {
                    if (Mp.Services.PlatformInfo.IsDesktop) {
                        return 40;
                    }
                    return 5;
                }
            }
        }
        public bool IsTitleLayersVisible { get; set; } = true;
        public bool IsMarqueeEnabled { get; set; } = true;

        // this is to help keep new items added pin tray visible when created
        // but avoid overriding user splitter changes DURING one of their workflows
        // and presuming that unless the window hides its still a workflow
        public bool HasUserAlteredPinTrayWidthSinceWindowShow { get; set; } = false;

        public bool IsAddingClipboardItem { get; private set; } = false;

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

        public bool IsAnyMouseModeEnabled => IsAutoCopyMode || IsRightClickPasteMode;


        public bool IsAutoCopyMode { get; set; }

        public bool IsRightClickPasteMode { get; set; }

        #endregion

        public bool IsAppPaused { get; set; } = false;

        public bool IsRestoringSelection { get; private set; } = false;

        public bool IsArrowSelecting { get; set; } = false;


        public bool IsQueryTrayEmpty =>
            QueryItems.Count() == 0 &&
            IsRequerying &&
            !MpAvMainWindowViewModel.Instance.IsMainWindowLoading;// || Items.All(x => x.IsPlaceholder);

        public bool IsSelectionReset { get; set; } = false;

        public bool IgnoreSelectionReset { get; set; } = false;

        public bool IsFilteringByApp { get; set; } = false;

        public bool IsQueryEmpty =>
            Mp.Services == null ||
            Mp.Services.Query == null ||
            Mp.Services.Query.TotalAvailableItemsInQuery == 0;

        public bool IsPinTrayEmpty =>
            InternalPinnedItems.Count() == 0;

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

        public bool IsGridLayout { get; set; }

        public bool IsRequerying { get; set; } = false;
        public bool IsQuerying { get; set; } = false;

        #region Drag Drop
        public bool IsAnyDropOverTrays { get; private set; }
        //public bool IsAnyTileDragging => 
        //    AllItems.Any(x => x.IsTileDragging) || 
        //    (MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count > 0 &&
        //     MpAvPersistentClipTilePropertiesHelper.IsPersistentTileDraggingEditable_ById(
        //         MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels[0].Id));

        //public bool IsExternalDragOverClipTrayContainer { get; set; }
        public bool IsDragOverPinTray { get; set; }

        #endregion

        #region Child Property Wrappers

        public bool IsAnyBusy => AllItems.Any(x => x.IsAnyBusy) || IsBusy;
        public bool IsAnyTileContextMenuOpened => AllItems.Any(x => x.IsContextMenuOpen);

        public bool IsAnyResizing => AllItems.Any(x => x.IsResizing);

        public bool CanAnyResize => AllItems.Any(x => x.CanResize);

        public bool IsAnyEditing => AllItems.Any(x => !x.IsContentAndTitleReadOnly);


        public bool IsAnyHovering => AllItems.Any(x => x.IsHovering);


        public bool IsAnyEditingClipTitle => AllItems.Any(x => !x.IsTitleReadOnly);

        public bool IsAnyEditingClipTile => AllItems.Any(x => !x.IsContentReadOnly);



        public bool IsAnyTilePinned => PinnedItems.Count > 0;

        public bool IsAnyTileListBoxItemFocused =>
            AllItems.Any(x => x.IsListBoxItemFocused);

        #endregion

        #endregion

        #endregion

        #region Events

        public event EventHandler<object> OnScrollIntoPinTrayViewRequest;
        public event EventHandler OnScrollToHomeRequest;

        public event EventHandler<MpCopyItem> OnCopyItemAdd;

        #endregion

        #region Constructors

        private MpAvClipTrayViewModel() : base() {
        }


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

            Mp.Services.ClipboardMonitor.OnClipboardChanged += ClipboardChanged;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            OnPropertyChanged(nameof(LayoutType));

            //ModalClipTileViewModel = await CreateClipTileViewModel(null);

            Items.Clear();
            for (int i = 0; i < DefaultLoadCount; i++) {
                var ctvm = await CreateClipTileViewModelAsync(null);
                Items.Add(ctvm);
            }

            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            IsGridLayout = LayoutType == MpClipTrayLayoutType.Grid;
            IsBusy = false;
        }

        public async Task<MpAvClipTileViewModel> CreateClipTileViewModelAsync(MpCopyItem ci, int queryOffsetIdx = -1) {
            MpAvClipTileViewModel ctvm = new MpAvClipTileViewModel(this);
            await ctvm.InitializeAsync(ci, queryOffsetIdx);
            return ctvm;
        }

        public void ValidateQueryTray() {
            var dups = Items.Where(x => x.QueryOffsetIdx >= 0 && Items.Any(y => y != x && x.QueryOffsetIdx == y.QueryOffsetIdx));
            if (dups.Count() > 0) {
                Debugger.Break();
                dups
                    .OrderByDescending(x => x.TileCreatedDateTime)
                    .Skip(1)
                    .ForEach(x => x.TriggerUnloadedNotification(false));

            }
        }
        public override string ToString() {
            return $"ClipTray";
        }

        public void RefreshQueryTrayLayout(MpAvClipTileViewModel fromItem = null) {
            FindTotalTileSize();

            fromItem = fromItem == null ? HeadItem : fromItem;
            UpdateTileRectCommand.Execute(fromItem);

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
        public void ForceScrollOffsetX(double newOffsetX, bool isSilent = false) {
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

        public void ForceScrollOffsetY(double newOffsetY, bool isSilent = false) {
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
        public void ForceScrollOffset(MpPoint newOffset, bool isSilent = false) {
            ForceScrollOffsetX(newOffset.X, isSilent);
            ForceScrollOffsetY(newOffset.Y, isSilent);
        }

        public void SetScrollAnchor(bool isLayoutChangeToGrid = false) {
            // this keeps track of the first screen visible tile 
            // so when orientation or zoom is changed relative offset is retained
            // in conjunction w/ drop scroll anchor
            if (IsQueryEmpty) {
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

        public void LockScrollToAnchor() {
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

        #region View Invokers

        public void ScrollIntoView(object obj) {
            //return;
            MpAvClipTileViewModel ctvm = null;
            if (obj is MpAvClipTileViewModel) {
                ctvm = obj as MpAvClipTileViewModel;
            } else if (obj is int ciid) {
                ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                if (ctvm == null) {
                    int ciid_query_idx = Mp.Services.Query.PageTools.GetItemOffsetIdx(ciid);
                    if (ciid_query_idx < 0) {
                        if (ciid < 0) {
                            // means nothing is selected
                            ScrollIntoView(null);
                            return;
                        }
                        // ciid is neither pinned nor in query (maybe should reset query here to id but prolly not right place)
                        Debugger.Break();
                        return;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        QueryCommand.Execute(ciid_query_idx);
                        while (IsAnyBusy) { await Task.Delay(100); }
                        ctvm = Items.FirstOrDefault(x => x.CopyItemId == ciid);
                        if (ctvm == null) {
                            // data model provider should have come up w/ nothing here
                            Debugger.Break();
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

            double pad = 0;
            MpRect svr = new MpRect(0, 0, ObservedQueryTrayScreenWidth, ObservedQueryTrayScreenHeight);
            MpRect ctvm_rect = ctvm.ScreenRect;

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

        public void ClearClipEditing() {
            foreach (var ctvm in Items) {
                if (ctvm == null) {
                    //occurs on first hide w/ async virtal items
                    continue;
                }
                ctvm.ClearEditing();
            }
        }

        public void ClearPinnedEditing() {
            foreach (var ctvm in PinnedItems) {
                if (ctvm == null) {
                    //occurs on first hide w/ async virtal items
                    continue;
                }
                ctvm.ClearEditing();
            }
        }

        public void ClearAllEditing() {
            ClearClipEditing();
            ClearPinnedEditing();
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

            MpAvPersistentClipTilePropertiesHelper.ClearPersistentSelection();
            //}));
        }
        public void ClearAllSelection(bool clearEditing = true) {
            //Dispatcher.UIThread.Post((Action)(() => {
            if (clearEditing) {
                ClearClipEditing();
                ClearPinnedEditing();
            }
            ClearClipSelection();
            ClearPinnedSelection();
            //}));
        }

        public void ClearPinnedSelection(bool clearEditing = true) {
            // Dispatcher.UIThread.Post((Action)(() => {
            if (clearEditing) {
                ClearPinnedEditing();
            }
            foreach (var ctvm in PinnedItems) {
                if (ctvm == null) {
                    //occurs on first hide w/ async virtal items
                    continue;
                }
                ctvm.ClearSelection();
            }
            //}));
        }

        public void ResetClipSelection(bool clearEditing = true) {
            IsSelectionReset = true;
            // Dispatcher.UIThread.Post(() => {
            ClearClipSelection(clearEditing);
            ClearPinnedSelection(clearEditing);

            SelectedItem = HeadItem;
            //Items[0].IsSelected = true;
            //if (!MpAvSearchBoxViewModel.Instance.IsTextBoxFocused) {
            //    RequestFocus(SelectedItems[0]);
            //}

            RequestScrollToHome();

            //});
            IsSelectionReset = false;
        }


        public void ClipboardChanged(object sender, MpPortableDataObject mpdo) {
            bool is_startup_ido = MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening;
            bool is_change_ignored =
                !is_startup_ido &&
                (IsAppPaused ||
                 (MpPrefViewModel.Instance.IgnoreInternalClipboardChanges && Mp.Services.ProcessWatcher.IsThisAppActive));

            if (is_change_ignored) {
                MpConsole.WriteLine("Clipboard Change Ignored by tray");
                MpConsole.WriteLine($"IsMainWindowLoading: {MpAvMainWindowViewModel.Instance.IsMainWindowLoading}");
                MpConsole.WriteLine($"IsAppPaused: {IsAppPaused}");
                MpConsole.WriteLine($"IgnoreInternalClipboardChanges: {MpPrefViewModel.Instance.IgnoreInternalClipboardChanges} IsThisAppActive: {Mp.Services.ProcessWatcher.IsThisAppActive}");
                return;
            }

            Dispatcher.UIThread.Post(async () => {
                while (MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening ||
                        MpAvPlainHtmlConverter.Instance.IsBusy) {
                    await Task.Delay(100);
                }
                if (is_startup_ido) {
                    await Task.Delay(3000);
                }
                await AddItemFromDataObjectAsync(mpdo);
            });
        }

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
            MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
        }

        public void StoreSelectionState(MpAvClipTileViewModel ctvm) {
            if (ctvm.IsPlaceholder) {
                // started happening in external pin tray drop
                //Debugger.Break();
                return;
            }
            if (!ctvm.IsSelected) {
                return;
            }

            MpAvPersistentClipTilePropertiesHelper.SetPersistentSelectedItem(ctvm.CopyItemId, ctvm.QueryOffsetIdx);
        }

        public void RestoreSelectionState(MpAvClipTileViewModel tile) {
            //var prevSelectedItems = MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels
            //                            .Where(y => y.Id == tile.CopyItemId).ToList();
            //if (prevSelectedItems.Count == 0) {
            if (MpAvPersistentClipTilePropertiesHelper.GetPersistentSelectedItemId() == tile.CopyItemId) {
                tile.ClearSelection();
                return;
            }

            IsRestoringSelection = true;

            SelectedItem = tile;

            IsRestoringSelection = false;
        }

        public void CleanupAfterPaste(MpAvClipTileViewModel sctvm, MpPortableProcessInfo pasted_pi, MpPortableDataObject mpdo) {
            IsPasting = false;
            //clean up pasted items state after paste
            sctvm.PasteCount++;
            sctvm.IsPasting = false;

            if (pasted_pi == null) {
                return;
            }

            string pasted_app_url = null;
            var avm = MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(pasted_pi);
            if (avm == null) {
                // we're f'd
                Debugger.Break();
            } else {
                pasted_app_url = Mp.Services.SourceRefTools.ConvertToRefUrl(avm.App);
            }
            if (string.IsNullOrEmpty(pasted_app_url)) {
                // f'd
                Debugger.Break();
                return;
            }

            Mp.Services.TransactionBuilder.ReportTransactionAsync(
                copyItemId: sctvm.CopyItemId,
                reqType: MpJsonMessageFormatType.DataObject,
                req: mpdo.SerializeData(),
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

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if (AppendClipTileViewModel != null &&
                    ci.Id == AppendClipTileViewModel.CopyItemId &&
                    IsAnyAppendMode) {
                    DeactivateAppendMode();
                }
                MpAvPersistentClipTilePropertiesHelper.RemoveProps(ci.Id);

                Mp.Services.Query.PageTools.RemoveItemId(ci.Id);
                //MpDataModelProvider.AvailableQueryCopyItemIds.Remove(ci.Id);

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
                        //IsBatchOffsetChange = true;
                        int removedQueryOffsetIdx = removed_ctvm.QueryOffsetIdx;
                        removed_ctvm.TriggerUnloadedNotification(true);
                        //Items.Where(x => x.QueryOffsetIdx > removedQueryOffsetIdx).ForEach(x => x.QueryOffsetIdx--);
                        //IsBatchOffsetChange = false;

                        if (wasSelected) {
                            var sel_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == removedQueryOffsetIdx);
                            if (sel_ctvm == null) {
                                // when tail
                                sel_ctvm = Items.OrderBy(x => x.QueryOffsetIdx).Last();
                            }
                            SelectedItem = sel_ctvm;
                        }
                    }
                }

                //CheckLoadMore();
                //RefreshQueryTrayLayout();
                while (!QueryCommand.CanExecute(string.Empty)) {
                    await Task.Delay(100);
                }
                QueryCommand.Execute(string.Empty);

                OnPropertyChanged(nameof(IsQueryEmpty));
            } else if (e is MpCopyItemTag cit) {
                var sttvm = MpAvTagTrayViewModel.Instance.SelectedItem;
                // check if unlink is part of current query
                bool is_part_of_query =
                    sttvm
                    .SelfAndAllDescendants
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.TagId)
                    .Any(x => x == cit.TagId);

                if (is_part_of_query && !sttvm.IsAllTag) {
                    // when unlinked item is part of current query remove its offset and do a reset query

                    if (Mp.Services.Query.PageTools.RemoveItemId(cit.CopyItemId)) {
                        Mp.Services.Query.NotifyQueryChanged();
                    } else {
                        // where/when was item removed from query?
                        Debugger.Break();
                    }
                }
            }
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

            //var svm = MpAvSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == ci.SourceId);

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
                case nameof(IsBusy):
                    OnPropertyChanged(nameof(IsAnyBusy));
                    break;
                case nameof(IsAnyBusy):
                    if (!IsAnyBusy) {
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        while (true) {
                            if (PercentLoaded >= 1) {
                                return;
                            }
                            await Task.Delay(100);
                        }
                    });
                    break;
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
                case nameof(IsGridLayout):
                    ToggleLayoutTypeCommand.Execute(null);
                    break;
                case nameof(QueryTrayTotalTileWidth):
                case nameof(QueryTrayTotalTileHeight):
                    if (QueryTrayTotalTileWidth < 0 || QueryTrayTotalTileHeight < 0) {
                        Debugger.Break();
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
                case nameof(HasScrollVelocity):
                    //MpPlatformWrapper.Services.Cursor.IsCursorFrozen = HasScrollVelocity;

                    if (HasScrollVelocity) {
                        Mp.Services.Cursor.UnsetCursor(null);
                    } else {
                        var hctvm = Items.FirstOrDefault(x => x.IsHovering);
                        if (IsAnyBusy) {
                            OnPropertyChanged(nameof(IsBusy));
                        }
                    }
                    break;

                case nameof(ZoomFactor):
                    UpdateDefaultItemSize();
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayZoomFactorChanged);
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
                case MpMessageType.SidebarItemSizeChanged:
                    OnPropertyChanged(nameof(MaxContainerScreenWidth));
                    OnPropertyChanged(nameof(MaxContainerScreenHeight));
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
                case MpMessageType.MainWindowClosed:
                    // reset so tray will autosize/bringIntoView on ListBox items changed (since actual size is not bound)
                    HasUserAlteredPinTrayWidthSinceWindowShow = false;
                    break;

                // QUERY

                case MpMessageType.RequeryCompleted:
                    OnPropertyChanged(nameof(EmptyQueryTrayText));
                    RefreshQueryTrayLayout();
                    //CheckLoadMore();
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
                    } else {
                        Dispatcher.UIThread.Post(async () => {
                            while (IsAnyBusy) {
                                await Task.Delay(100);
                            }
                            // BUG this works around initial tile size being tiny and triggering resize fits
                            // them right
                            Items.ForEach(x => x.UpdateQueryOffset());
                        });
                    }

                    break;
                case MpMessageType.QueryChanged:
                    QueryCommand.Execute(null);
                    break;
                case MpMessageType.SubQueryChanged:
                    QueryCommand.Execute(ScrollOffset);
                    break;
                case MpMessageType.TotalQueryCountChanged:
                    OnPropertyChanged(nameof(Mp.Services.Query.TotalAvailableItemsInQuery));
                    AllItems.ForEach(x => x.UpdateQueryOffset());
                    break;

                    // DND
                    //case MpMessageType.ItemDragBegin:
                    //    OnPropertyChanged(nameof(IsAnyTileDragging));
                    //    if(DragItem == null) {
                    //        // shant be true
                    //        Debugger.Break();
                    //        return;
                    //    }
                    //    MpAvPersistentClipTilePropertiesHelper.AddPersistentIsTileDraggingTile_ById(DragItem.CopyItemId);
                    //    break;
                    //case MpMessageType.ItemDragEnd:
                    //    OnPropertyChanged(nameof(IsAnyTileDragging));

                    //    MpAvPersistentClipTilePropertiesHelper.ClearPersistentIsTileDragging();
                    //    break;
            }
        }

        private async Task<MpAvClipTileViewModel> CreateOrRetrieveClipTileViewModelAsync(MpCopyItem ci) {
            MpAvClipTileViewModel nctvm = null;
            if (ci.WasDupOnCreate) {
                nctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ci.Id);
            }
            if (nctvm == null) {
                nctvm = await CreateClipTileViewModelAsync(ci);
            }
            while (nctvm.IsBusy) {
                // NOTE don't wait for anything but tile since view won't be loaded
                await Task.Delay(100);
            }
            return nctvm;
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
                Mp.Services.Query.NotifyQueryChanged();
                return;
            }

            if (checkHi) {
                int hi_thresholdQueryIdx = Math.Max(0, TailQueryIdx - RemainingItemsCountThreshold);

                MpAvClipTileViewModel hi_ctvm = VisibleItems.Count() == 0 ? null : VisibleItems.Aggregate((a, b) => (isScrollHorizontal ? a.TrayX > b.TrayX : a.TrayY > b.TrayY) ? a : b);
                //var hi_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == hi_thresholdQueryIdx);
                if (hi_ctvm == null) {
                    // Debugger.Break();
                    //QueryCommand.Execute(true);
                    PerformLoadMore(true);
                    return;
                }
                //double hi_diff = isScrollHorizontal ?
                //                    hi_ctvm.TrayRect.Right - ScrollOffsetX - ObservedQueryTrayScreenWidth :
                //                    hi_ctvm.TrayRect.Bottom - ScrollOffsetY - ObservedQueryTrayScreenHeight;
                //bool addMoreToTail = hi_diff < 0;
                bool addMoreToTail = QueryTrayScreenRect.Contains(hi_ctvm.ScreenRect.BottomRight);
                bool addMoreToTail2 = hi_ctvm == null || hi_ctvm.QueryOffsetIdx >= hi_thresholdQueryIdx;
                if (addMoreToTail || addMoreToTail2) {
                    //MpConsole.WriteLine("Load more to tail");
                    // when right (horizontal scroll) or bottom (vertical scroll) of high threshold tile is within viewport
                    //QueryCommand.Execute(true);
                    //if (!isLessZoom) {
                    //    return;
                    //}
                    PerformLoadMore(true);
                    return;
                }
            }
            if (checkLo) {
                int lo_thresholdQueryIdx = Math.Max(0, HeadQueryIdx + RemainingItemsCountThreshold);
                MpAvClipTileViewModel lo_ctvm = VisibleItems.Count() == 0 ? null : VisibleItems.Aggregate((a, b) => (isScrollHorizontal ? a.TrayX < b.TrayX : a.TrayY < b.TrayY) ? a : b);
                //var lo_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == lo_thresholdQueryIdx);
                if (lo_ctvm == null) {
                    //Debugger.Break();
                    //QueryCommand.Execute(false);
                    PerformLoadMore(false);
                    return;
                }
                //double lo_diff = isScrollHorizontal ?
                //                    lo_ctvm.TrayRect.Right - ScrollOffsetX :
                //                    lo_ctvm.TrayRect.Top - ScrollOffsetY;
                //bool addMoreToHead = lo_diff > 0;
                bool addMoreToHead = QueryTrayScreenRect.Contains(lo_ctvm.ScreenRect.TopLeft);
                bool addMoreToHead2 = lo_ctvm == null || lo_ctvm.QueryOffsetIdx <= lo_thresholdQueryIdx;

                if (addMoreToHead || addMoreToHead2) {
                    //MpConsole.WriteLine("Load more to head");
                    // when right (horizontal scroll) or bottom (vertical scroll) of high threshold tile is within viewport
                    //QueryCommand.Execute(false);
                    //if (!isLessZoom) {
                    //    return;
                    //}
                    PerformLoadMore(false);
                    return;
                }
            }
        }

        private void PerformLoadMore(bool isHi) {
            if (QueryCommand.CanExecute(isHi)) {
                QueryCommand.Execute(isHi);
                return;
            }
            //Dispatcher.UIThread.Post(async () => {
            //    while (!QueryCommand.CanExecute(isHi)) {
            //        await Task.Delay(100);
            //    }
            //    QueryCommand.Execute(isHi);
            //});
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
            UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();
        }

        public async Task UpdateEmptyPropertiesAsync() {
            // send signal immediatly but also wait and send for busy dependants
            OnPropertyChanged(nameof(IsPinTrayEmpty));
            OnPropertyChanged(nameof(IsQueryEmpty));
            OnPropertyChanged(nameof(EmptyQueryTrayText));
            OnPropertyChanged(nameof(IsQueryTrayEmpty));
            OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));

            while (IsAnyBusy) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(IsQueryEmpty));
            OnPropertyChanged(nameof(IsPinTrayEmpty));
            OnPropertyChanged(nameof(EmptyQueryTrayText));
            OnPropertyChanged(nameof(IsQueryTrayEmpty));
            OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));
        }

        private bool CanTileNavigate() {
            bool canNavigate =
                !IsAnyBusy &&
                !IsArrowSelecting &&

                  !HasScrollVelocity &&
                  !IsScrollingIntoView;

            if (canNavigate) {
                if (SelectedItem != null &&
                    SelectedItem.IsSubSelectionEnabled ||
                    (SelectedItem != null && !SelectedItem.IsTitleReadOnly && SelectedItem.IsTitleFocused)) {
                    canNavigate = false;
                }
                if (canNavigate) {
                    var cf = FocusManager.Instance.Current;
                    if (!IsAnyTileListBoxItemFocused && (cf != null && cf != App.MainView as IInputElement && cf is not MpAvContentWebView)) {

                        canNavigate = false;
                    }
                }
            }

            return canNavigate;
        }

        private async Task SelectNeighborHelperAsync(int row_offset, int col_offset) {
            if (row_offset != 0 && col_offset != 0) {
                // NO! should only be one or the other
                Debugger.Break();
                return;
            }
            if (row_offset == 0 && col_offset == 0) {
                return;
            }

            IsArrowSelecting = true;
            ScrollIntoView(PersistantSelectedItemId);
            await Task.Delay(100);
            while (IsAnyBusy) { await Task.Delay(100); }
            if (SelectedItem == null) {
                if (IsPinTrayEmpty) {
                    if (IsQueryTrayEmpty) {
                        IsArrowSelecting = false;
                        return;
                    }
                    SelectedItem = HeadItem;
                    IsArrowSelecting = false;
                    return;
                }
                SelectedItem = PinnedItems[0];
                IsArrowSelecting = false;
                return;
            }
            MpAvClipTileViewModel target_ctvm = null;
            if (row_offset != 0) {
                target_ctvm = await SelectedItem.GetNeighborByRowOffsetAsync(row_offset);
            } else {
                target_ctvm = await SelectedItem.GetNeighborByColumnOffsetAsync(col_offset);
            }

            if (target_ctvm != null) {
                SelectedItem = target_ctvm;
            }
            IsArrowSelecting = false;
        }

        private async Task AddItemFromDataObjectAsync(MpPortableDataObject cd) {
            while (IsAddingClipboardItem) {
                await Task.Delay(100);
                MpConsole.WriteLine("waiting to add item to cliptray...");
            }

            IsAddingClipboardItem = true;

            var newCopyItem = await Mp.Services.CopyItemBuilder.BuildAsync(
                pdo: cd,
                transType: MpTransactionType.Created);

            await AddUpdateOrAppendCopyItemAsync(newCopyItem);

            IsAddingClipboardItem = false;
        }

        private async Task AddUpdateOrAppendCopyItemAsync(MpCopyItem ci, int force_pin_idx = 0) {
            if (ci == null) {
                MpConsole.WriteLine("Could not build copyitem, cannot add");
                OnCopyItemAdd?.Invoke(this, null);
                return;
            }
            if (ci.WasDupOnCreate) {
                //item is a duplicate
                MpConsole.WriteLine("Duplicate item detected, incrementing copy count and updating copydatetime");
                ci.CopyCount++;
                // reseting CopyDateTime will move item to top of recent list
                ci.CopyDateTime = DateTime.Now;
                await ci.WriteToDatabaseAsync();
            }

            if (AppendClipTileViewModel == null &&
                PendingNewModels.All(x => x.Id != ci.Id)) {
                PendingNewModels.Add(ci);
            }

            bool wasAppended = false;
            if (IsAnyAppendMode) {
                wasAppended = await UpdateAppendModeAsync(ci);
                if (!wasAppended && PendingNewModels.All(x => x.Id != ci.Id)) {
                    PendingNewModels.Add(ci);
                }
            } else if (PendingNewModels.All(x => x.Id != ci.Id)) {
                PendingNewModels.Add(ci);
            }

            MpCopyItem arg_ci = ci;
            if (wasAppended) {
                MpMessenger.SendGlobal(MpMessageType.AppendBufferChanged);
                arg_ci = null;
            } else {
                MpPrefViewModel.Instance.UniqueContentItemIdx++;
                MpMessenger.SendGlobal(MpMessageType.ContentAdded);
                AddNewItemsCommand.Execute(null);
            }
            OnCopyItemAdd?.Invoke(this, arg_ci);
        }

        private async Task PasteClipTileAsync(MpAvClipTileViewModel ctvm) {
            MpPortableDataObject mpdo = null;
            ctvm.IsPasting = true;

            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = true;
            var cv = ctvm.GetContentView();
            if (cv == null) {
                if (ctvm.CopyItem != null) {
                    mpdo = ctvm.CopyItem.ToPortableDataObject();
                }
            } else if (cv is MpAvIDragSource ds) {
                mpdo = await ds.GetDataObjectAsync();
            }
            MpPortableProcessInfo pi = null;
            if (mpdo == null) {
                // is none selected?
                Debugger.Break();
            } else {
                pi = Mp.Services.ProcessWatcher.LastProcessInfo;
                // NOTE paste success is very crude, false positive is likely
                bool success = await Mp.Services.ExternalPasteHandler.PasteDataObjectAsync(mpdo, pi);
                if (success) {
                    MpMessenger.SendGlobal(MpMessageType.ContentPasted);
                } else {
                    // clear pi to ignore paste history
                    pi = null;
                    MpMessenger.SendGlobal(MpMessageType.AppError);
                }
            }

            CleanupAfterPaste(ctvm, pi, mpdo);
            MpAvMainWindowViewModel.Instance.IsMainWindowSilentLocked = false;
        }
        private async Task CutOrCopySelectionAsync(bool isCut) {
            string keys = isCut ?
            Mp.Services.PlatformShorcuts.CutKeys : Mp.Services.PlatformShorcuts.CopyKeys;

            await Mp.Services.KeyStrokeSimulator
            .SimulateKeyStrokeSequenceAsync(keys);
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
                    if (argParts[1] is MpAvClipTileViewModel prev_ctvm) {
                        prevOffsetRect = prev_ctvm.TrayRect;
                    }

                }
                if (ctvm == null) {
                    return;
                }
                var trayRect = FindTileRect(ctvm.QueryOffsetIdx, prevOffsetRect);
                ctvm.TrayY = trayRect.Location.Y;
                ctvm.TrayX = trayRect.Location.X;
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

        public ICommand ScrollToNextPageCommand => new MpCommand(
             () => {
                 MpPoint scroll_delta = MpPoint.Zero;
                 if (DefaultScrollOrientation == Orientation.Horizontal) {
                     scroll_delta.X = ObservedQueryTrayScreenWidth;
                 } else {
                     scroll_delta.Y = ObservedQueryTrayScreenHeight;
                 }
                 var nextPageOffset = (ScrollOffset + scroll_delta);
                 QueryCommand.Execute(nextPageOffset);
             },
            () => {
                return CanTileNavigate();
            });

        public ICommand ScrollToPreviousPageCommand => new MpCommand(
            () => {
                MpPoint scroll_delta = MpPoint.Zero;
                if (DefaultScrollOrientation == Orientation.Horizontal) {
                    scroll_delta.X = ObservedQueryTrayScreenWidth;
                } else {
                    scroll_delta.Y = ObservedQueryTrayScreenHeight;
                }
                var prevPageOffset = (ScrollOffset - scroll_delta);
                QueryCommand.Execute(prevPageOffset);
            },
            () => {
                return CanTileNavigate();
            });

        public ICommand SelectNextRowItemCommand => new MpAsyncCommand(
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

        public ICommand ResetZoomFactorCommand => new MpCommand(
            () => {
                ZoomFactor = 1.0d;
            });

        public MpIAsyncCommand<object> PinTileCommand => new MpAsyncCommand<object>(
             async (args) => {
                 MpPinType pinType = MpPinType.Internal;
                 MpAppendModeType appendType = MpAppendModeType.None;
                 int pin_idx = 0;
                 MpAvClipTileViewModel ctvm_to_pin = null;
                 if (args is MpAvClipTileViewModel) {
                     // pinning new or query tray tile from overlay button
                     ctvm_to_pin = args as MpAvClipTileViewModel;
                 } else if (args is object[] argParts) {
                     // dnd pin tray drop 
                     ctvm_to_pin = argParts[0] as MpAvClipTileViewModel;
                     if (argParts[1] is int) {
                         pin_idx = (int)argParts[1];
                     } else {
                         pinType = (MpPinType)argParts[1];
                     }
                     if (pinType == MpPinType.Append) {
                         if (argParts.Length <= 2) {
                             appendType = MpAppendModeType.Line;
                         } else {
                             appendType = (MpAppendModeType)argParts[2];
                         }
                     }

                 }

                 if (ctvm_to_pin == null || ctvm_to_pin.IsPlaceholder) {
                     MpConsole.WriteTraceLine("PinTile error, tile is either already pinned or placeholder");
                     Debugger.Break();
                     return;
                 }

                 if (Items.Where(x => x.CopyItemId == ctvm_to_pin.CopyItemId) is IEnumerable<MpAvClipTileViewModel> query_items &&
                     query_items.Any()) {
                     // create temp tile w/ model ref
                     var temp_ctvm = await CreateClipTileViewModelAsync(ctvm_to_pin.CopyItem);
                     // unload query tile
                     query_items.ForEach(x => x.TriggerUnloadedNotification(true));
                     // use temp tile to pin
                     ctvm_to_pin = temp_ctvm;
                 } else if (ctvm_to_pin.QueryOffsetIdx >= 0) {
                     //Mp.Services.Query.PageTools.RemoveItemId(ctvm_to_pin.CopyItemId);
                 }
                 Mp.Services.Query.PageTools.RemoveItemId(ctvm_to_pin.CopyItemId);
                 if (pinType == MpPinType.Window) {
                     ctvm_to_pin.OpenPopOutWindow();
                 } else if (pinType == MpPinType.Append) {
                     ctvm_to_pin.OpenAppendWindow(appendType);
                 }

                 if (ctvm_to_pin.IsPinned) {
                     // for drop from pin tray or new duplicate was in pin tray
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

                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 ctvm_to_pin.OnPropertyChanged(nameof(ctvm_to_pin.IsPinned));
                 ctvm_to_pin.OnPropertyChanged(nameof(ctvm_to_pin.IsPlaceholder));

                 await Task.Delay(100);
                 while (IsAnyBusy) {
                     await Task.Delay(100);
                 }
                 RefreshQueryTrayLayout();
                 await Task.Delay(200);
                 //SelectedItem = ctvm_to_pin;
                 ctvm_to_pin.IsSelected = true;

                 OnPropertyChanged(nameof(Items));
                 OnPropertyChanged(nameof(PinnedItems));
                 OnPropertyChanged(nameof(MinPinTrayScreenWidth));
                 OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                 OnPropertyChanged(nameof(ObservedQueryTrayScreenWidth));
                 OnPropertyChanged(nameof(ObservedQueryTrayScreenHeight));
                 UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync(this);
             },
            (args) =>
            args != null);

        public ICommand UnpinTileCommand => new MpAsyncCommand<object>(
             async (args) => {
                 var upctvm = args as MpAvClipTileViewModel;
                 int unpinnedId = upctvm.CopyItemId;
                 int unpinned_ctvm_idx = PinnedItems.IndexOf(upctvm);
                 PinnedItems.Remove(upctvm);
                 OnPropertyChanged(nameof(IsAnyTilePinned));

                 if (!IsAnyTilePinned) {
                     ObservedPinTrayScreenWidth = 0;
                 }

                 OnPropertyChanged(nameof(PinnedItems));
                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 OnPropertyChanged(nameof(MinPinTrayScreenWidth));
                 OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                 OnPropertyChanged(nameof(ObservedQueryTrayScreenWidth));
                 OnPropertyChanged(nameof(ObservedQueryTrayScreenHeight));

                 ClearClipSelection(false);
                 // perform inplace requery to potentially put unpinned tile back
                 await QueryCommand.ExecuteAsync(string.Empty);

                 //await Task.Delay(300);
                 MpAvClipTileViewModel to_select_ctvm =
                    Items.FirstOrDefault(x => x.CopyItemId == unpinnedId);

                 if (to_select_ctvm == null) {
                     if (IsPinTrayEmpty) {
                         while (IsAnyBusy) {
                             // query returns before sub tasks complete and updated offsets are needed
                             await Task.Delay(100);
                         }
                         // select left most visible tile if pin tray empty
                         to_select_ctvm = VisibleItems.AggregateOrDefault((a, b) => a.QueryOffsetIdx < b.QueryOffsetIdx ? a : b);
                     } else {
                         // prefer select prev pinned neighbor tile
                         //int sel_idx = Math.Min(PinnedItems.Count - 1, Math.Max(0, unpinned_ctvm_idx));
                         //to_select_ctvm = PinnedItems[sel_idx];
                         //
                         to_select_ctvm =
                            InternalPinnedItems
                            .Aggregate((a, b) => unpinned_ctvm_idx - a.ItemIdx < unpinned_ctvm_idx - b.ItemIdx ? a : b);
                     }
                 }
                 if (to_select_ctvm == null) {
                     // should probably not happen or will have no effect (empty query) but in case
                     ResetClipSelection(false);
                 } else {
                     to_select_ctvm.IsSelected = true;
                 }

                 UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync(this);
             },
            (args) => args != null && args is MpAvClipTileViewModel ctvm && ctvm.IsPinned);

        public ICommand ToggleSelectedTileIsPinnedCommand => new MpCommand(
            () => {
                ToggleTileIsPinnedCommand.Execute(SelectedItem);
            });

        public ICommand ToggleTileIsPinnedCommand => new MpCommand<object>(
            (args) => {
                MpAvClipTileViewModel pctvm = null;
                if (args is MpAvClipTileViewModel) {
                    pctvm = args as MpAvClipTileViewModel;
                } else if (args is object[] argParts) {
                    pctvm = argParts[0] as MpAvClipTileViewModel;
                }

                if (pctvm.IsPinned) {
                    UnpinTileCommand.Execute(args);
                } else {
                    PinTileCommand.Execute(args);
                }
            },
            (args) => {
                return args != null;
            });
        public ICommand UnpinAllCommand => new MpCommand(() => {
            int pin_count = PinnedItems.Count;
            while (pin_count > 0) {
                var to_unpin_ctvm = PinnedItems[--pin_count];
                if (to_unpin_ctvm.IsPopOutVisible ||
                    to_unpin_ctvm.IsAppendNotifier) {
                    continue;
                }
                UnpinTileCommand.Execute(to_unpin_ctvm);
            }
        });

        public ICommand OpenSelectedTileInWindowCommand => new MpCommand(
            () => {
                SelectedItem.PinToPopoutWindowCommand.Execute(null);
            }, () => {
                return SelectedItem != null && !SelectedItem.IsPopOutVisible;
            });
        public ICommand DuplicateSelectedClipsCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;
                var clonedCopyItem = await SelectedItem.CopyItem.CloneDbModelAsync(
                    deepClone: true);

                await clonedCopyItem.WriteToDatabaseAsync();
                PendingNewModels.Add(clonedCopyItem);

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
                for (int i = 0; i < PendingNewModels.Count; i++) {
                    var ci = PendingNewModels[i];
                    MpAvClipTileViewModel nctvm = await CreateOrRetrieveClipTileViewModelAsync(ci);
                    ToggleTileIsPinnedCommand.Execute(nctvm);
                }

                PendingNewModels.Clear();
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
                    if (PendingNewModels.Any(x => x.Id == tag_drop_ci.Id)) {
                        // should only happen once from drop in tag view
                        Debugger.Break();
                    } else {
                        PendingNewModels.Add(tag_drop_ci);
                    }
                }
                if (PendingNewModels.Count == 0) {
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

        private bool _canQuery = true;
        public MpIAsyncCommand<object> QueryCommand => new MpAsyncCommand<object>(
            async (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) => {
                MpConsole.WriteLine($"Query called. Arg: '{offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg}'");
                Dispatcher.UIThread.VerifyAccess();
                _canQuery = false;
                //Dispatcher.UIThread.Post(async () => {
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

                IsBusy = !isLoadMore;
                IsQuerying = true;
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
                        newScrollOffset = ScrollOffset;
                        loadOffsetIdx = HeadQueryIdx;
                    }
                } else {
                    // new query all content and offsets are re-initialized
                    ClearClipSelection();

                    // trigger unload event to wipe js eval's that maybe pending 
                    Items.Where(x => !x.IsPlaceholder).ForEach(x => x.TriggerUnloadedNotification(false));

                    MpAvPersistentClipTilePropertiesHelper.ClearPersistentWidths();
                }

                if (isRequery || isInPlaceRequery) {
                    await Mp.Services.Query.QueryForTotalCountAsync();

                    FindTotalTileSize();

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
                            Items[--itemCountDiff].TriggerUnloadedNotification(false);
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

                #region Fetch Data & Create Init Tasks

                var cil = await Mp.Services.Query
                    .FetchPageAsync(loadOffsetIdx, loadCount);
                // fetchQueryIdxList, new int[] { });// Items.Where(x=>x.CopyItemId > 0).Select(x=>x.CopyItemId));
                //since tiles watch for their model changes, remove any items

                //var recycle_order_items = Items.OrderByDescending(x => x.RecyclePriority).ToList();
                //var recycle_idxs = GetLoadItemIdxs(isLoadMore ? isLoadMoreTail : null, cil.Count);
                int recycle_base_query_idx = isLoadMoreTail ? HeadQueryIdx : TailQueryIdx;
                int dir = isLoadMoreTail ? 1 : -1;
                List<Task> initTasks = new List<Task>();

                for (int i = 0; i < cil.Count; i++) {
                    MpAvClipTileViewModel cur_ctvm = null;
                    if (isLoadMore) {
                        int cur_query_idx = recycle_base_query_idx + (dir * i);
                        cur_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == cur_query_idx);
                    } else {
                        cur_ctvm = Items[i]; //recycle_order_items[i];
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
                    if (isSubQuery && MpAvPersistentClipTilePropertiesHelper.GetPersistentSelectedItemId() == cil[i].Id) {
                        needsRestore = true;
                    }
                    initTasks.Add(cur_ctvm.InitializeAsync(cil[i], fetchQueryIdxList[i], needsRestore));


                }

                #endregion

                #region Initialize Items

                //if(IsRequerying) {
                //    await Task.WhenAll(initTasks);
                //} else {
                Task.WhenAll(initTasks).FireAndForgetSafeAsync();
                //}
                //

                #endregion

                #region Finalize State & Measurements

                OnPropertyChanged(nameof(Mp.Services.Query.TotalAvailableItemsInQuery));
                OnPropertyChanged(nameof(QueryTrayTotalTileWidth));
                OnPropertyChanged(nameof(QueryTrayTotalWidth));
                OnPropertyChanged(nameof(MaxScrollOffsetX));
                OnPropertyChanged(nameof(MaxScrollOffsetY));
                OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
                OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));

                if (Items.Where(x => !x.IsPlaceholder).Count() == 0) {
                    ScrollOffsetX = 0;
                    LastScrollOffsetX = 0;
                    ScrollOffsetY = 0;
                    LastScrollOffsetY = 0;
                }

                IsBusy = false;
                IsQuerying = false;
                OnPropertyChanged(nameof(IsAnyBusy));
                OnPropertyChanged(nameof(IsQueryEmpty));

                sw.Stop();
                MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);

                if (isRequery) {
                    //_scrollOffset = LastScrollOffsetX = 0;
                    //ForceScrollOffset(MpPoint.Zero);
                    MpMessenger.SendGlobal(MpMessageType.RequeryCompleted);

                    if (SelectedItem == null &&
                        !MpAvPersistentClipTilePropertiesHelper.HasPersistenSelection() &&
                        Mp.Services.Query.TotalAvailableItemsInQuery > 0) {
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

                Dispatcher.UIThread.Post(async () => {
                    while (IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    if (!isLoadMore) {

                        MpMessenger.SendGlobal(MpMessageType.QueryCompleted);
                    }
                    ValidateQueryTray();
                });
                #endregion
                _canQuery = true;
                //});
            },
            (offsetIdx_Or_ScrollOffset_Arg) => {
                return !IsAnyBusy && !IsQuerying && !IsRequerying && _canQuery;
            });

        private List<int> GetLoadItemIdxs(bool? isLoadMoreTail, int count) {
            List<int> idxs = new List<int>();
            if (isLoadMoreTail == null) {
                idxs = Items
                    .OrderByDescending(x => x.RecyclePriority)
                    .Take(count)
                    .Select(x => Items.IndexOf(x))
                    .ToList();
                count -= idxs.Count;
                if (count > 0) {
                    Debugger.Break();
                }
            } else {
                int recycle_base_query_idx = isLoadMoreTail.Value ? HeadQueryIdx : TailQueryIdx;
                int dir = isLoadMoreTail.Value ? 1 : -1;
                for (int i = 0; i < count; i++) {
                    int cur_query_idx = recycle_base_query_idx + (dir * i);
                    idxs.Add(cur_query_idx);
                }
            }
            return idxs;
        }
        public ICommand SearchWebCommand => new MpCommand<object>(
            (args) => {
                //string pt = string.Join(
                //            Environment.NewLine,
                //            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(x => Mp.Services.StringTools.ToPlainText(x.ItemData)));
                string pt = Mp.Services.StringTools.ToPlainText(SelectedItem.CopyItemData);
                //MpHelpers.OpenUrl(args.ToString() + Uri.EscapeDataString(pt));
            }, (args) => args != null && args is string);


        public ICommand ChangeSelectedClipsColorCommand => new MpCommand<object>(
             (hexStrOrBrush) => {
                 string hexStr = string.Empty;
                 if (hexStrOrBrush is SolidColorBrush scb && scb.Color is Color color) {
                     hexStr = new MpColor(color.A, color.R, color.G, color.B).ToHex(); //scb.ToHex();
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
                CutOrCopySelectionAsync(true).FireAndForgetSafeAsync(this);
            },
            (args) => {
                return SelectedItem != null && SelectedItem.IsSubSelectionEnabled;
            });

        public ICommand CopySelectionFromContextMenuCommand => new MpCommand<object>(
            (args) => {
                CutOrCopySelectionAsync(false).FireAndForgetSafeAsync(this);
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
                return //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen == false &&
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
                Mp.Services.KeyStrokeSimulator
                .SimulateKeyStrokeSequenceAsync(Mp.Services.PlatformShorcuts.PasteKeys)
                .FireAndForgetSafeAsync(this);
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
                var mpdo = await Mp.Services.DataObjectHelperAsync.GetPlatformClipboardDataObjectAsync(false);

                SelectedItem.RequestPastePortableDataObject(mpdo);
            }, () => SelectedItem != null && !SelectedItem.IsPlaceholder);

        public ICommand PasteCopyItemByIdCommand => new MpAsyncCommand<object>(
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
                        Debugger.Break();
                        return;
                    }
                    ctvm = await CreateClipTileViewModelAsync(ci);
                }
                PasteClipTileAsync(ctvm).FireAndForgetSafeAsync(this);
            },
            (args) => {
                return args is int || args is string;
            });


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
                return //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen == false &&
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

        public ICommand AssignShortcutToSelectedItemCommand => new MpCommand(
            () => {
                MpAvShortcutCollectionViewModel.Instance
                .ShowAssignShortcutDialogCommand.Execute(SelectedItem);
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
                var analyticItemVm =
                    MpAvAnalyticItemCollectionViewModel.Instance
                    .Items.FirstOrDefault(x => x.PluginGuid == preset.PluginGuid);
                int selected_ciid = SelectedItem.CopyItemId;

                EventHandler<MpCopyItem> analysisCompleteHandler = null;
                analysisCompleteHandler = (s, e) => {
                    analyticItemVm.OnAnalysisCompleted -= analysisCompleteHandler;
                    AllItems.FirstOrDefault(x => x.CopyItemId == selected_ciid).IsBusy = false;

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

        public ICommand ToggleIsAppPausedCommand => new MpCommand(
            () => {
                IsAppPaused = !IsAppPaused;
            });

        public ICommand ToggleRightClickPasteCommand => new MpCommand(
            () => {
                IsRightClickPasteMode = !IsRightClickPasteMode;
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("RIGHT CLICK PASTE MODE: {0}", IsRightClickPasteMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
                MpMessenger.SendGlobal(IsRightClickPasteMode ? MpMessageType.RightClickPasteEnabled : MpMessageType.RightClickPasteDisabled);
            }, () => !IsAppPaused);

        public ICommand ToggleAutoCopyModeCommand => new MpCommand(
            () => {
                IsAutoCopyMode = !IsAutoCopyMode;
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("AUTO-COPY SELECTION MODE: {0}", IsAutoCopyMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
                MpMessenger.SendGlobal(IsAutoCopyMode ? MpMessageType.AutoCopyEnabled : MpMessageType.AutoCopyDisabled);
            }, () => !IsAppPaused);



        public ICommand EnableFindAndReplaceForSelectedItem => new MpCommand(
            () => {
                SelectedItem.IsFindAndReplaceVisible = true;
            }, () => SelectedItem != null && !SelectedItem.IsFindAndReplaceVisible && SelectedItem.IsTextItem);

        public ICommand SelectClipTileCommand => new MpCommand<object>(
            (args) => {
                //Dispatcher.UIThread.Post(() => {
                MpAvClipTileViewModel ctvm = null;
                if (args is MpAvClipTileViewModel) {
                    ctvm = args as MpAvClipTileViewModel;
                } else if (args is int ciid) {
                    ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                }
                //if (ctvm != null) {
                //    if (ctvm.IsSelected) {
                //        return;
                //    }
                //    ctvm.IsSelected = true;
                //}

                SelectedItem = ctvm;
                //if (ctvm == null) {
                //    SelectedItem = null;
                //} else {
                //    ctvm.IsSelected = true;
                //}
                //});
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
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                if (SelectedItem != null) {
                    append_ctvm = SelectedItem;
                }
            } else if (PendingNewModels.Count > 0) {
                var most_recent_ci = PendingNewModels[PendingNewModels.Count - 1];
                PendingNewModels.RemoveAt(0);
                append_ctvm = await CreateOrRetrieveClipTileViewModelAsync(most_recent_ci);
            } else if (SelectedItem != null) {
                append_ctvm = SelectedItem;
            } else {
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
            await PinTileCommand.ExecuteAsync(new object[] { append_ctvm, MpPinType.Append, appendType });
        }

        private void UpdateAppendModeStateFlags(MpAppendModeFlags flags, string source) {
            IsAppendLineMode = flags.HasFlag(MpAppendModeFlags.AppendLine);
            IsAppendInsertMode = flags.HasFlag(MpAppendModeFlags.AppendInsert);
            IsAppendManualMode = flags.HasFlag(MpAppendModeFlags.Manual);
            IsAppendPaused = flags.HasFlag(MpAppendModeFlags.Paused);
            IsAppendPreMode = flags.HasFlag(MpAppendModeFlags.Pre);

            var last_flags = _appendModeFlags;
            _appendModeFlags = flags;
            OnPropertyChanged(nameof(AppendModeStateFlags));

            if (AppendClipTileViewModel == null ||
                AppendClipTileViewModel.PopOutWindowState == WindowState.Minimized) {

                ShowEmptyOrMinimizedAppendNotifications(_appendModeFlags, last_flags);
            }

            if (AppendClipTileViewModel != null &&
                AppendClipTileViewModel.GetContentView() is MpAvContentWebView wv) {
                wv.ProcessAppendStateChangedMessage(GetAppendStateMessage(null), source);
            }
        }

        private void ShowEmptyOrMinimizedAppendNotifications(MpAppendModeFlags cur_flags, MpAppendModeFlags last_flags) {
            // NOTE activate/deactivate not handled here

            if (cur_flags.HasFlag(MpAppendModeFlags.Manual) != last_flags.HasFlag(MpAppendModeFlags.Manual)) {
                string manual_change_str = cur_flags.HasFlag(MpAppendModeFlags.Manual) ? "Manual" : "Extent";
                string detail_str = cur_flags.HasFlag(MpAppendModeFlags.Manual) ? "Appends added where you select" : "Appends added to top or bottom of the clip";
                string icon_key = cur_flags.HasFlag(MpAppendModeFlags.Manual) ? "CaretImage" : "DoubleSidedArrowSolidImage";
                MpNotificationBuilder.ShowMessageAsync(
                       title: $"{manual_change_str} Append Mode Activated",
                       body: detail_str,
                       msgType: MpNotificationType.AppModeChange,
                       iconSourceObj: icon_key).FireAndForgetSafeAsync();
            }

            if (cur_flags.HasFlag(MpAppendModeFlags.Paused) != last_flags.HasFlag(MpAppendModeFlags.Paused)) {
                string pause_change_str = cur_flags.HasFlag(MpAppendModeFlags.Paused) ? "Paused" : "Resumed";
                string detail_str = cur_flags.HasFlag(MpAppendModeFlags.Paused) ? "Clipboard accumulation halted" : "Clipboard accumulation resumed";
                string icon_key = cur_flags.HasFlag(MpAppendModeFlags.Paused) ? "PauseImage" : "PlayImage";
                MpNotificationBuilder.ShowMessageAsync(
                       title: $"Append {pause_change_str}",
                       body: detail_str,
                       msgType: MpNotificationType.AppModeChange,
                       iconSourceObj: icon_key).FireAndForgetSafeAsync();
            }

            if (cur_flags.HasFlag(MpAppendModeFlags.Pre) != last_flags.HasFlag(MpAppendModeFlags.Pre)) {
                string manual_change_str = cur_flags.HasFlag(MpAppendModeFlags.Pre) ? "Before" : "After";
                string detail_str = cur_flags.HasFlag(MpAppendModeFlags.Pre) ? "Clipboard changes will now be prepended" : "Clipboard changes will now be appended";
                string icon_key = cur_flags.HasFlag(MpAppendModeFlags.Pre) ? "BringToFrontImage" : "SendToBackImage";
                MpNotificationBuilder.ShowMessageAsync(
                       title: $"{manual_change_str} Append Mode Activated",
                       body: detail_str,
                       msgType: MpNotificationType.AppModeChange,
                       iconSourceObj: icon_key).FireAndForgetSafeAsync();
            }
        }

        private async Task ActivateAppendModeAsync(bool isAppendLine, bool isManualMode) {
            Dispatcher.UIThread.VerifyAccess();
            while (IsAddingClipboardItem) {
                // if new item is being added, its important to wait for it
                await Task.Delay(100);
            }

            if (AppendClipTileViewModel == null) {
                // append mode was just toggled ON (param was null)
                await AssignAppendClipTileAsync(isAppendLine ? MpAppendModeType.Line : MpAppendModeType.Insert);
            }
            bool was_append_already_enabled = IsAnyAppendMode && AppendClipTileViewModel != null;

            MpAppendModeFlags amf = MpAppendModeFlags.None;
            if (isAppendLine) {
                amf |= MpAppendModeFlags.AppendLine;
            } else {
                amf |= MpAppendModeFlags.AppendInsert;
            }
            if (isManualMode) {
                amf |= MpAppendModeFlags.Manual;
            }
            UpdateAppendModeStateFlags(amf, "command");

            if (was_append_already_enabled) {
                // don't trigger if already activated, the AppendDataChanged() timesout because IsContentLoaded doesn't goto false
                return;
            }

            MpMessenger.SendGlobal(MpMessageType.AppendModeActivated);
            if (AppendClipTileViewModel == null) {
                // no item assigned yet so just show enable message
                string type_str = IsAppendLineMode ? "Block" : "Inline";
                string manual_str = IsAppendManualMode ? "(Manual) " : string.Empty;
                string icon_key = IsAppendLineMode ? "AppendLineImage" : "AppendImage";
                MpNotificationBuilder.ShowMessageAsync(
                       title: $"Append {type_str} {manual_str}Mode Activated",
                       body: "Copy text or file(s) to apply.",
                       msgType: MpNotificationType.AppModeChange,
                       iconSourceObj: icon_key).FireAndForgetSafeAsync();
            }
        }
        private void DeactivateAppendMode() {
            Dispatcher.UIThread.VerifyAccess();

            UpdateAppendModeStateFlags(MpAppendModeFlags.None, "command");

            MpMessenger.SendGlobal(MpMessageType.AppendModeDeactivated);
            if (AppendClipTileViewModel == null) {
                // only show deactivate ntf if no windows there
                MpNotificationBuilder.ShowMessageAsync(
                           title: $"Append Deactivated",
                           body: $"Normal clipboard behavior has been restored",
                           msgType: MpNotificationType.AppModeChange,
                           iconSourceObj: "ClipboardImage").FireAndForgetSafeAsync();
            } else {
                var deactivate_append_ctvm = AppendClipTileViewModel;
                deactivate_append_ctvm.IsAppendNotifier = false;
                if (deactivate_append_ctvm.WasCloseAppendWindowConfirmed) {
                    // only close window if closed from title button
                    deactivate_append_ctvm.PopInTileCommand.Execute(null);
                }
            }
        }
        private async Task<bool> UpdateAppendModeAsync(MpCopyItem aci, bool isNew = true) {
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

                        if (!isNew || !MpPrefViewModel.Instance.IgnoreAppendedItems) {
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
                        MpPrefViewModel.Instance.IgnoreAppendedItems) {
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
                    DeactivateAppendMode();
                } else {
                    await ActivateAppendModeAsync(false, IsAppendManualMode);
                }
            });

        public MpIAsyncCommand ToggleAppendLineModeCommand => new MpAsyncCommand(
            async () => {
                if (IsAppendLineMode) {
                    DeactivateAppendMode();
                } else {
                    await ActivateAppendModeAsync(true, IsAppendManualMode);
                }
            });

        public ICommand ToggleAppendManualModeCommand => new MpAsyncCommand(
            async () => {
                bool new_is_manual = !IsAppendManualMode;
                bool cur_or_new_is_line_mode = IsAppendLineMode;
                if (!IsAnyAppendMode && new_is_manual) {
                    // append line by default
                    cur_or_new_is_line_mode = true;
                }

                await ActivateAppendModeAsync(cur_or_new_is_line_mode, new_is_manual);
            });
        public ICommand ToggleAppendPausedCommand => new MpCommand(
            () => {
                if (!IsAnyAppendMode) {
                    return;
                }
                var toggled_flags = AppendModeStateFlags;
                if (toggled_flags.HasFlag(MpAppendModeFlags.Paused)) {
                    toggled_flags ^= MpAppendModeFlags.Paused;
                } else {
                    toggled_flags |= MpAppendModeFlags.Paused;
                }
                UpdateAppendModeStateFlags(toggled_flags, "command");
            });

        public ICommand ToggleAppendPreModeCommand => new MpAsyncCommand(
            async () => {
                if (!IsAnyAppendMode) {
                    // enable line mode by default
                    await ToggleAppendLineModeCommand.ExecuteAsync();
                }
                var toggled_flags = AppendModeStateFlags;
                if (toggled_flags.HasFlag(MpAppendModeFlags.Pre)) {
                    toggled_flags ^= MpAppendModeFlags.Pre;
                } else {
                    toggled_flags |= MpAppendModeFlags.Pre;
                }
                UpdateAppendModeStateFlags(toggled_flags, "command");
            });

        public ICommand AppendDataCommand => new MpCommand<object>(
            (args) => {
                AppendClipTileViewModel.IsPopOutVisible = true;
                if (AppendClipTileViewModel.GetContentView() is MpAvContentWebView wv) {
                    wv.ProcessAppendStateChangedMessage(GetAppendStateMessage(args as string), "command");
                }

            }, (args) => {
                if (AppendClipTileViewModel == null || !IsAnyAppendMode) {
                    return false;
                }
                return args is string argStr && !string.IsNullOrEmpty(argStr);
            });

        //public ICommand ShowAppendDevToolsCommand => new MpCommand(
        //    () => {
        //        if (MpAvAppendNotificationWindow.Instance == null) {
        //            return;
        //        }
        //        MpAvAppendNotificationWindow.Instance.ShowNotifierDevToolsCommand.Execute(null);
        //    });

        public ICommand ShowDevToolsCommand => new MpCommand(
            () => {
                if (SelectedItem != null && SelectedItem.GetContentView() is MpAvContentWebView wv) {
                    wv.ShowDevTools();
                }
            });

        #endregion




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

        #endregion
    }
}
