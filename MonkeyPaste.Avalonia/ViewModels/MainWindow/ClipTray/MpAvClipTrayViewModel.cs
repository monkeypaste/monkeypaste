using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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

    public class MpAvClipTrayViewModel :
        MpAvSelectorViewModelBase<object, MpAvClipTileViewModel>,
        MpIBootstrappedItem,
        MpIPagingScrollViewerViewModel,
        MpIActionComponent,
        MpIBoundSizeViewModel,
        MpIContextMenuViewModel,
        MpIContentQueryTools,
        MpIProgressIndicatorViewModel {
        #region Private Variables

        //private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;

        //private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        //private int _appendCopyItemId;
        private List<MpCopyItem> _newModels = new List<MpCopyItem>();

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
                if (OperatingSystem.IsAndroid()) {
                    string uri = System.IO.Path.Combine(MpPlatform.Services.PlatformInfo.StorageDir, "MonkeyPaste.Editor", "test.html").ToFileSystemUriFromPath();
                    return uri;
                }
                return MpAvCefNetApplication.GetEditorPath().ToFileSystemUriFromPath();
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

        IEnumerable<int> MpIContentQueryTools.GetOmittedContentIds() =>
            PinnedItems.Select(x => x.CopyItemId);

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
                            Header = @"Cut",
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("ScissorsImage") as string,
                            Command = MpAvShortcutCollectionViewModel.Instance.SimulateKeyStrokeCommand,
                            CommandParameter = MpPlatform.Services.PlatformShorcuts.CutKeys,
                            ShortcutArgs = new object[] { MpShortcutType.PasteHere },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Copy",
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("CopyImage") as string,
                            Command = CutSelectionFromContextMenuCommand,
                            ShortcutArgs = new object[] { MpShortcutType.CopySelection },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Paste Here",
                            AltNavIdx = 6,
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("PasteImage") as string,
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
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("DeleteImage") as string,
                            Command = DeleteSelectedClipsCommand,
                            ShortcutArgs = new object[] { MpShortcutType.DeleteSelectedItems },
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Rename",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("RenameImage") as string,
                            Command = EditSelectedTitleCommand,
                            ShortcutArgs = new object[] { MpShortcutType.EditTitle },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Edit",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("EditContentImage") as string,
                            Command = EditSelectedContentCommand,
                            ShortcutArgs = new object[] { MpShortcutType.EditContent },
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Transform",
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("ToolsImage") as string,
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = @"Find and Replace",
                                    AltNavIdx = 0,
                                    IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("SearchImage") as string,
                                    Command = EnableFindAndReplaceForSelectedItem,
                                    ShortcutArgs = new object[] { MpShortcutType.FindAndReplaceSelectedItem },
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Duplicate",
                                    AltNavIdx = 0,
                                    IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("DuplicateImage") as string,
                                    Command = DuplicateSelectedClipsCommand,
                                    ShortcutArgs = new object[] { MpShortcutType.Duplicate },
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To Web Search",
                                    IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("WebImage") as string,
                                    SubItems = new List<MpMenuItemViewModel>() {
                                        new MpMenuItemViewModel() {
                                            Header = "Google",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("GoogleImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.google.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "Bing",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("BingImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.bing.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "DuckDuckGo",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("DuckImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://duckduckgo.com/?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "Yandex",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("YandexImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://yandex.com/search/?text="
                                        },
                                        new MpMenuItemViewModel() { IsSeparator = true},
                                        new MpMenuItemViewModel() {
                                            Header = "Manage...",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("CogImage") as string
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
                            IconResourceKey = MpPlatform.Services.PlatformResource.GetResource("PinToCollectionImage") as string,
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

        public Orientation ListOrientation =>
            MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ? Orientation.Horizontal : Orientation.Vertical;

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

        public double PinTrayScreenWidth { get; set; }

        public double PinTrayScreenHeight { get; set; }

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
        public bool IsThumbDraggingX { get; set; } = false;
        public bool IsThumbDraggingY { get; set; } = false;
        public bool IsThumbDragging => IsThumbDraggingX || IsThumbDraggingY;

        public bool IsScrollJumping { get; set; }

        public void FindTotalTileSize() {
            // NOTE this is to avoid making TotalTile Width/Height auto
            // and should only be called on a requery or on content resize (or event better only on resize complete)
            MpSize totalTileSize = MpSize.Empty;
            if (MpPlatform.Services.Query.TotalAvailableItemsInQuery > 0) {
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
            // For TileRect<MpRect>:  0 <= queryOffsetIdx < MpPlatform.Services.Query.TotalAvailableItemsInQuery and scrollOffsets == -1
            // For TileQueryIdx<[]{int,MpRect}>: queryoffsetIdx < 0 and both scrollOffset > 0

            bool isGrid = LayoutType == MpClipTrayLayoutType.Grid;
            bool isStack = !isGrid;

            bool isFindTileIdx = scrollOffsetX >= 0 && scrollOffsetY >= 0;
            bool isFindTileRect = !isFindTileIdx && queryOffsetIdx >= 0;
            bool isFindTotalSize = !isFindTileRect;

            int totalTileCount = MpPlatform.Services.Query.TotalAvailableItemsInQuery;
            queryOffsetIdx = isFindTotalSize ? MpPlatform.Services.Query.TotalAvailableItemsInQuery - 1 : queryOffsetIdx;
            if (queryOffsetIdx >= totalTileCount) {
                return null;
            }

            int startIdx = 0;// prevOffsetRect == null ? 0 : queryOffsetIdx;

            var total_size = MpSize.Empty;
            int gridFixedCount = -1;

            MpRect last_rect = null;// prevOffsetRect;

            for (int i = startIdx; i <= queryOffsetIdx; i++) {
                int tileId = MpPlatform.Services.Query.PageTools.GetItemId(i);
                MpSize tile_size = new MpSize(DefaultQueryItemWidth, DefaultQueryItemHeight);
                if (MpAvPersistentClipTilePropertiesHelper.TryGetByPersistentSize_ById(tileId, out double uniqueSize)) {
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
                return MpPlatform.Services.Query.TotalAvailableItemsInQuery - 1;
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

        public double DefaultQueryItemWidth {
            get {
                double defaultWidth = ListOrientation == Orientation.Horizontal ?
                                    (QueryTrayScreenHeight * ZoomFactor) :
                                    (QueryTrayScreenWidth * ZoomFactor);
                double scrollBarSize = ScrollBarFixedAxisSize;// IsHorizontalScrollBarVisible ? 30:0;
                return Math.Clamp(defaultWidth - scrollBarSize, 0, MaxTileWidth);
            }
        }

        public double DefaultQueryItemHeight {
            get {
                double defaultHeight = ListOrientation == Orientation.Horizontal ?
                                    (QueryTrayScreenHeight * ZoomFactor) :
                                    (QueryTrayScreenWidth * ZoomFactor);
                //defaultHeight = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds.Width *
                //            ZoomFactor * MIN_SIZE_ZOOM_FACTOR_COEFF;
                double scrollBarSize = ScrollBarFixedAxisSize;// IsVerticalScrollBarVisible ? 30 : 0;
                return Math.Clamp(defaultHeight - scrollBarSize, 0, MaxTileHeight);
            }
        }

        public double DefaultPinItemWidth => DEFAULT_ITEM_SIZE;
        public double DefaultPinItemHeight => DEFAULT_ITEM_SIZE;

        public double DefaultEditableItemWidth =>
            EDITOR_TOOLBAR_MIN_WIDTH;

        public double ScrollBarFixedAxisSize =>
            30;

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

        public int MaxClipTrayQueryIdx => MpPlatform.Services.Query.TotalAvailableItemsInQuery - 1;
        public int MinClipTrayQueryIdx => 0;

        public bool CanThumbDragY => QueryTrayScreenHeight < QueryTrayTotalHeight;
        public bool CanThumbDragX => QueryTrayScreenWidth < QueryTrayTotalWidth;

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

        public MpAvClipTileViewModel ModalClipTileViewModel { get; private set; }

        public MpAvClipTileViewModel AppendClipTileViewModel =>
            AllItems.FirstOrDefault(x => x.IsAppendNotifier);

        public IEnumerable<MpAvClipTileViewModel> QueryItems =>
            Items.Where(x => !x.IsPlaceholder);

        public IEnumerable<MpAvClipTileViewModel> SortOrderedItems =>
            Items.Where(x => x.QueryOffsetIdx >= 0).OrderBy(x => x.QueryOffsetIdx);

        public IEnumerable<MpAvClipTileViewModel> PlaceholderItems =>
            Items.Where(x => x.IsPlaceholder);
        public IEnumerable<MpAvClipTileViewModel> AllItems {
            get {
                foreach (var ctvm in Items) {
                    yield return ctvm;
                }
                foreach (var pctvm in PinnedItems) {
                    yield return pctvm;
                }
                if (ModalClipTileViewModel != null) {
                    yield return ModalClipTileViewModel;
                }
            }
        }
        public MpAvClipTileViewModel HeadItem =>
            SortOrderedItems.ElementAtOrDefault(0);

        public MpAvClipTileViewModel TailItem =>
            SortOrderedItems.ElementAtOrDefault(SortOrderedItems.Count() - 1);

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

        public override MpAvClipTileViewModel SelectedItem {
            get {
                if (MpAvAppendNotificationWindow.Instance != null &&
                    MpAvAppendNotificationWindow.Instance.IsVisible) {
                    // only visible if mw is not open
                    return ModalClipTileViewModel;
                }

                return AllItems.FirstOrDefault(x => x.IsSelected);
            }
            set {
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
        public double ObservedContainerScreenWidth { get; set; }
        public double ObservedContainerScreenHeight { get; set; }
        public double ObservedPinTrayScreenWidth { get; set; }
        public double ObservedPinTrayScreenHeight { get; set; }
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

        public double MinClipTrayScreenWidth =>
            MinClipOrPinTrayScreenWidth;
        public double MinClipTrayScreenHeight =>
            MinClipOrPinTrayScreenHeight;
        public double MinClipOrPinTrayScreenWidth =>
            50;
        public double MinClipOrPinTrayScreenHeight =>
            50;
        public double MaxTileWidth =>
            double.PositiveInfinity;// Math.Max(0, QueryTrayScreenWidth - MAX_TILE_SIZE_CONTAINER_PAD);
        public double MaxTileHeight =>
            double.PositiveInfinity;// Math.Max(0, QueryTrayScreenHeight - MAX_TILE_SIZE_CONTAINER_PAD);

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
                if (MpPlatform.Services == null ||
                    MpPlatform.Services.StartupState == null) {
                    return string.Empty;
                }
                if (!IsQueryEmpty ||
                    !MpPlatform.Services.StartupState.IsPlatformLoaded) {
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
        public List<MpCopyItem> PendingNewModels => _newModels;
        public MpClipboardModeFlags ClipboardModeFlags {
            get {
                MpClipboardModeFlags cmf = MpClipboardModeFlags.None;
                if (!IsAppPaused) {
                    cmf |= MpClipboardModeFlags.ListeningForChanges;
                }
                if (IsAppendLineMode) {
                    cmf |= MpClipboardModeFlags.AppendBlock;
                }
                if (IsAppendMode) {
                    cmf |= MpClipboardModeFlags.AppendInline;
                }
                if (IsRightClickPasteMode) {
                    cmf |= MpClipboardModeFlags.RightClickPaste;
                }
                if (IsAutoCopyMode) {
                    cmf |= MpClipboardModeFlags.AutoCopy;
                }

                return cmf;
            }
        }

        public string AppendData { get; private set; } = null;

        private MpAppendModeFlags _appendModeFlags = MpAppendModeFlags.None;
        public MpAppendModeFlags AppendModeStateFlags {
            get => _appendModeFlags;
            set {
                UpdateAppendModeStateFlags(value, "property");
            }
        }


        public bool IsAppendMode { get; set; }

        public bool IsAppendLineMode { get; set; }

        public bool IsAppendManualMode { get; set; }

        public bool IsAnyAppendMode => IsAppendMode || IsAppendLineMode;
        #endregion

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

        public string MouseModeImageSourcePath {
            get {
                if (IsRightClickPasteMode && IsAutoCopyMode) {
                    return MpPlatform.Services.PlatformResource.GetResource("BothClickImage") as string;
                }
                if (IsRightClickPasteMode) {
                    return MpPlatform.Services.PlatformResource.GetResource("RightClickImage") as string;
                }
                if (IsAutoCopyMode) {
                    return MpPlatform.Services.PlatformResource.GetResource("LeftClickImage") as string;
                }
                return MpPlatform.Services.PlatformResource.GetResource("NoneClickImage") as string;
            }
        }

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
            MpPlatform.Services == null ||
            MpPlatform.Services.Query == null ||
            MpPlatform.Services.Query.TotalAvailableItemsInQuery == 0;

        public bool IsPinTrayEmpty =>
            PinnedItems.Count == 0;

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
        public bool HasScrollVelocity => Math.Abs(ScrollVelocityX) + Math.Abs(ScrollVelocityY) > 0.1d;

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

            MpPlatform.Services.ContentQueryTools = this;

            MpPlatform.Services.ClipboardMonitor.OnClipboardChanged += ClipboardChanged;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            OnPropertyChanged(nameof(LayoutType));

            ModalClipTileViewModel = await CreateClipTileViewModel(null);

            Items.Clear();
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

            OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
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
            MpAvClipTileViewModel ctvm = null;
            if (obj is MpAvClipTileViewModel) {
                ctvm = obj as MpAvClipTileViewModel;
            } else if (obj is int ciid) {
                ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                if (ctvm == null) {
                    int ciid_query_idx = MpPlatform.Services.Query.PageTools.GetItemOffsetIdx(ciid);
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
            MpRect svr = new MpRect(0, 0, QueryTrayScreenWidth, QueryTrayScreenHeight);
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

            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Clear();
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
            bool is_change_ignored = MpAvMainWindowViewModel.Instance.IsMainWindowLoading ||
                                        IsAppPaused ||
                                        (MpPrefViewModel.Instance.IgnoreInternalClipboardChanges && MpPlatform.Services.ProcessWatcher.IsThisAppActive);
            if (is_change_ignored) {
                MpConsole.WriteLine("Clipboard Change Ignored by tray");
                MpConsole.WriteLine($"IsMainWindowLoading: {MpAvMainWindowViewModel.Instance.IsMainWindowLoading}");
                MpConsole.WriteLine($"IsAppPaused: {IsAppPaused}");
                MpConsole.WriteLine($"IgnoreInternalClipboardChanges: {MpPrefViewModel.Instance.IgnoreInternalClipboardChanges} IsThisAppActive: {MpPlatform.Services.ProcessWatcher.IsThisAppActive}");
                return;
            }

            Dispatcher.UIThread.Post(async () => {
                await AddItemFromClipboard(mpdo);
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

            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels = new List<MpCopyItem>() { ctvm.CopyItem };
        }

        public void RestoreSelectionState(MpAvClipTileViewModel tile) {
            var prevSelectedItems = MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels
                                        .Where(y => y.Id == tile.CopyItemId).ToList();
            if (prevSelectedItems.Count == 0) {
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
                pasted_app_url = MpPlatform.Services.SourceRefBuilder.ConvertToRefUrl(avm.App);
            }
            if (string.IsNullOrEmpty(pasted_app_url)) {
                // f'd
                Debugger.Break();
                return;
            }

            MpPlatform.Services.TransactionBuilder.ReportTransactionAsync(
                copyItemId: sctvm.CopyItemId,
                reqType: MpJsonMessageFormatType.DataObject,
                req: mpdo.SerializeData(),
                respType: MpJsonMessageFormatType.None,
                resp: null,
                ref_urls: new[] { pasted_app_url },
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

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                if (ci.Id == ModalClipTileViewModel.CopyItemId &&
                    IsAnyAppendMode) {

                    DeactivateAppendModeAsync().FireAndForgetSafeAsync();
                }
                MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Remove(ci);
                MpAvPersistentClipTilePropertiesHelper.RemovePersistentSize_ById(ci.Id);

                MpPlatform.Services.Query.PageTools.RemoveItemId(ci.Id);
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

                CheckLoadMore();
                RefreshQueryTrayLayout();

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

                    if (MpPlatform.Services.Query.PageTools.RemoveItemId(cit.CopyItemId)) {
                        MpPlatform.Services.Query.NotifyQueryChanged();
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
                case nameof(ModalClipTileViewModel):
                    if (ModalClipTileViewModel == null) {
                        return;
                    }
                    ModalClipTileViewModel.OnPropertyChanged(nameof(ModalClipTileViewModel.CopyItemId));

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
                case nameof(HasScrollVelocity):
                    //MpPlatformWrapper.Services.Cursor.IsCursorFrozen = HasScrollVelocity;

                    if (HasScrollVelocity) {
                        MpPlatform.Services.Cursor.UnsetCursor(null);
                    } else {
                        var hctvm = Items.FirstOrDefault(x => x.IsHovering);
                        if (IsAnyBusy) {
                            OnPropertyChanged(nameof(IsBusy));
                        }
                    }
                    break;
                //case nameof(DefaultQueryItemSize):
                case nameof(DefaultQueryItemWidth):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.MinWidth)));
                    break;
                case nameof(DefaultQueryItemHeight):
                    //if(!MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening &&
                    //    MpAvMainWindowViewModel.Instance.IsMainWindowOpening) {
                    //    // since spring animation is clamped along screen edge when 
                    //    // it springs mw stretches and makes tiles bounce so reject tile 
                    //    // update because size will return to original
                    //    // (its kinda cool but is too much processing)
                    //    break;
                    //}
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.MinHeight)));
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


                    //case nameof(IsAnyTileDragging):
                    //    MpAvMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpAvMainWindowViewModel.Instance.IsAnyItemDragging));
                    //    break;
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
                    OnPropertyChanged(nameof(MpPlatform.Services.Query.TotalAvailableItemsInQuery));
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

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();
            if (e.OldItems != null) {
                foreach (MpAvClipTileViewModel octvm in e.OldItems) {
                    octvm.DisposeViewModel();
                }
            }
        }

        private void PinnedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync();
        }

        public async Task UpdateEmptyPropertiesAsync() {
            // send signal immediatly but also wait and send for busy dependants
            OnPropertyChanged(nameof(IsPinTrayEmpty));
            OnPropertyChanged(nameof(IsQueryEmpty));
            OnPropertyChanged(nameof(EmptyQueryTrayText));
            OnPropertyChanged(nameof(IsQueryTrayEmpty));
            OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsVerticalScrollBarVisible));

            while (IsAnyBusy) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(IsQueryEmpty));
            OnPropertyChanged(nameof(IsPinTrayEmpty));
            OnPropertyChanged(nameof(EmptyQueryTrayText));
            OnPropertyChanged(nameof(IsQueryTrayEmpty));
            OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
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
                    if (!IsAnyTileListBoxItemFocused) {
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

        private async Task AddItemFromClipboard(MpPortableDataObject cd) {
            if (IsAddingClipboardItem) {
                MpConsole.WriteLine("Warning! New Clipboard item detected while already adding one (seems to only occur internally). Ignoring this one.");
                return;
            }

            IsAddingClipboardItem = true;

            var newCopyItem = await MpPlatform.Services.CopyItemBuilder.BuildAsync(
                pdo: cd,
                transType: MpTransactionType.Created);

            if (newCopyItem == null || newCopyItem.Id < 1) {
                //this occurs if the copy item is not a known format or app init
                MpConsole.WriteTraceLine("Unable to create copy item from clipboard!");
                IsAddingClipboardItem = false;
                return;
            }

            if (MpPrefViewModel.Instance.NotificationDoCopySound) {
                //MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
            }
            if (MpPrefViewModel.Instance.IsTrialExpired) {
                MpNotificationBuilder.ShowMessageAsync(
                    title: "Trial Expired",
                    body: "Please update your membership to use Monkey Paste",
                    msgType: MpNotificationType.TrialExpired,
                    iconSourceObj: MpPrefViewModel.Instance.AbsoluteResourcesPath + @"/Images/monkey (2).png")
                    .FireAndForgetSafeAsync(this);
            }


            await AddUpdateOrAppendCopyItemAsync(newCopyItem);

            IsAddingClipboardItem = false;
        }

        private async Task AddUpdateOrAppendCopyItemAsync(MpCopyItem ci, int force_pin_idx = 0) {
            if (ci.WasDupOnCreate) {
                //item is a duplicate
                MpConsole.WriteLine("Duplicate item detected, incrementing copy count and updating copydatetime");
                ci.CopyCount++;
                // reseting CopyDateTime will move item to top of recent list
                ci.CopyDateTime = DateTime.Now;
                await ci.WriteToDatabaseAsync();
            }

            if (ModalClipTileViewModel.IsPlaceholder) {
                _newModels.Add(ci);
            }
            bool wasAppended = false;
            if (IsAnyAppendMode) {
                wasAppended = await UpdateAppendModeAsync(ci);
            }

            if (!wasAppended) {
                //if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                //    MpAvTagTrayViewModel.Instance.AllTagViewModel.LinkCopyItemCommand.Execute(ci.Id);
                //}
                AddNewItemsCommand.Execute(null);
                OnCopyItemAdd?.Invoke(this, ci);
            }
        }



        private async Task PasteClipTileAsync(MpAvClipTileViewModel ctvm) {
            MpPortableDataObject mpdo = null;
            ctvm.IsPasting = true;
            var cv = ctvm.GetContentView();
            if (cv == null) {
                if (ctvm.CopyItem != null) {
                    mpdo = ctvm.CopyItem.ToPortableDataObject();
                }
            } else if (cv is MpAvIDragSource ds) {
                mpdo = await ds.GetDataObjectAsync(true);
            }
            MpPortableProcessInfo pi = null;
            if (mpdo == null) {
                // is none selected?
                Debugger.Break();
            } else {
                pi = MpPlatform.Services.ProcessWatcher.LastProcessInfo;
                // NOTE paste success is very crude, false positive is likely
                bool success = await MpPlatform.Services.ExternalPasteHandler.PasteDataObjectAsync(mpdo, pi);
                if (!success) {
                    // clear pi to ignore paste history
                    pi = null;
                }
            }

            CleanupAfterPaste(ctvm, pi, mpdo);
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
                     scroll_delta.X = QueryTrayScreenWidth;
                 } else {
                     scroll_delta.Y = QueryTrayScreenHeight;
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
                    scroll_delta.X = QueryTrayScreenWidth;
                } else {
                    scroll_delta.Y = QueryTrayScreenHeight;
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

        public ICommand PinTileCommand => new MpAsyncCommand<object>(
             async (args) => {
                 int pin_idx = 0;
                 MpAvClipTileViewModel ctvm_to_pin = null;
                 if (args is MpAvClipTileViewModel) {
                     // pinning new or query tray tile from overlay button
                     ctvm_to_pin = args as MpAvClipTileViewModel;
                 } else if (args is object[] argParts) {
                     // dnd pin tray drop 
                     ctvm_to_pin = argParts[0] as MpAvClipTileViewModel;
                     pin_idx = (int)argParts[1];
                 }

                 if (ctvm_to_pin == null || ctvm_to_pin.IsPlaceholder) {
                     MpConsole.WriteTraceLine("PinTile error, tile is either already pinned or placeholder");
                     Debugger.Break();
                     return;
                 }

                 //if (MpPlatform.Services.Query.PageTools.RemoveItemId(ctvm_to_pin.CopyItemId)) {
                 //    // tile was part of query tray
                 //    if (Items.Contains(ctvm_to_pin)) {
                 //        int ctvm_to_pin_qidx = ctvm_to_pin.QueryOffsetIdx;

                 //        // trigger PublicHandle change to unload view
                 //        ctvm_to_pin.QueryOffsetIdx = -1;
                 //        Items.Remove(ctvm_to_pin);
                 //        Items.Where(x => x.QueryOffsetIdx > ctvm_to_pin_qidx).ForEach(x => x.QueryOffsetIdx = x.QueryOffsetIdx - 1);
                 //    }
                 //}

                 if (Items.Contains(ctvm_to_pin)) {
                     // create temp tile w/ model ref
                     var temp_ctvm = await CreateClipTileViewModel(ctvm_to_pin.CopyItem);
                     // unload query tile
                     ctvm_to_pin.TriggerUnloadedNotification(true);
                     // use temp tile to pin
                     ctvm_to_pin = temp_ctvm;
                 } else if (ctvm_to_pin.QueryOffsetIdx >= 0) {
                     MpPlatform.Services.Query.PageTools.RemoveItemId(ctvm_to_pin.CopyItemId);
                 }

                 if (ctvm_to_pin.IsPinned) {
                     // for drop from pin tray or new duplicate was in pin tray
                     int cur_pin_idx = PinnedItems.IndexOf(ctvm_to_pin);
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
                 SelectedItem = ctvm_to_pin;


                 OnPropertyChanged(nameof(Items));
                 OnPropertyChanged(nameof(PinnedItems));
                 OnPropertyChanged(nameof(MinPinTrayScreenWidth));
                 OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                 OnPropertyChanged(nameof(QueryTrayScreenWidth));
                 OnPropertyChanged(nameof(QueryTrayScreenHeight));
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
                 OnPropertyChanged(nameof(QueryTrayScreenWidth));
                 OnPropertyChanged(nameof(QueryTrayScreenHeight));

                 ClearClipSelection(false);
                 // perform inplace requery to potentially put unpinned tile back
                 QueryCommand.Execute(string.Empty);
                 while (IsAnyBusy) {
                     await Task.Delay(100);
                 }
                 await Task.Delay(300);
                 var unpinned_ctvm = Items.FirstOrDefault(x => x.CopyItemId == unpinnedId);

                 if (unpinned_ctvm != null) {
                     // if unpinned tile is in current page select it
                     SelectedItem = unpinned_ctvm;
                 } else if (IsPinTrayEmpty) {
                     // select left most visible tile if pin tray empty
                     SelectedItem = VisibleItems.AggregateOrDefault((a, b) => a.QueryOffsetIdx < b.QueryOffsetIdx ? a : b);
                 } else {
                     // prefer select neighbor pin tile 
                     int sel_idx = Math.Min(PinnedItems.Count - 1, Math.Max(0, unpinned_ctvm_idx));
                     SelectedItem = PinnedItems[sel_idx];
                 }

                 UpdateEmptyPropertiesAsync().FireAndForgetSafeAsync(this);
             },
            (args) => args != null && args is MpAvClipTileViewModel ctvm && ctvm.IsPinned);

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
                UnpinTileCommand.Execute(PinnedItems[--pin_count]);
            }
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

        private bool _canQuery = true;
        public ICommand QueryCommand => new MpCommand<object>(
            (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) => {
                MpConsole.WriteLine($"Query called. Arg: '{offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg}'");
                //Dispatcher.UIThread.VerifyAccess();
                _canQuery = false;
                Dispatcher.UIThread.Post(async () => {
                    IsBusy = true;
                    IsQuerying = true;
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
                        await MpPlatform.Services.Query.QueryForTotalCountAsync();

                        FindTotalTileSize();

                        OnPropertyChanged(nameof(MpPlatform.Services.Query.TotalAvailableItemsInQuery));
                        OnPropertyChanged(nameof(QueryTrayTotalWidth));
                        OnPropertyChanged(nameof(MaxScrollOffsetX));
                        OnPropertyChanged(nameof(MaxScrollOffsetY));
                        OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                        OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
                    }

                    loadOffsetIdx = Math.Max(0, loadOffsetIdx);

                    if (loadCount == 0) {
                        // is not an LoadMore Query
                        loadCount = Math.Min(DefaultLoadCount, MpPlatform.Services.Query.TotalAvailableItemsInQuery);
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
                                var ctvm = await CreateClipTileViewModel(null);
                                Items.Add(ctvm);
                                itemCountDiff++;
                            }
                        }
                    }
                    #endregion

                    #region Fetch Data & Create Init Tasks

                    var cil = await MpPlatform.Services.Query
                        .FetchItemsByQueryIdxListAsync(
                        fetchQueryIdxList, new int[] { });// Items.Where(x=>x.CopyItemId > 0).Select(x=>x.CopyItemId));
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
                        if (isSubQuery && MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Any(x => x.Id == cil[i].Id)) {
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

                    OnPropertyChanged(nameof(MpPlatform.Services.Query.TotalAvailableItemsInQuery));
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
                            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count == 0 &&
                            MpPlatform.Services.Query.TotalAvailableItemsInQuery > 0) {
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
                        ValidateQueryTray();
                    });
                    #endregion
                    _canQuery = true;
                });
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
                string pt = string.Join(
                            Environment.NewLine,
                            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(x => MpPlatform.Services.StringTools.ToPlainText(x.ItemData)));

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
                    ctvm = await CreateClipTileViewModel(ci);
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

        public ICommand ToggleIsAppPausedCommand => new MpCommand(
            () => {
                IsAppPaused = !IsAppPaused;
            });

        public ICommand ToggleRightClickPasteCommand => new MpCommand(
            () => {
                IsRightClickPasteMode = !IsRightClickPasteMode;
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("RIGHT CLICK PASTE MODE: {0}", IsRightClickPasteMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);

        public ICommand ToggleAutoCopyModeCommand => new MpCommand(
            () => {
                IsAutoCopyMode = !IsAutoCopyMode;
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("AUTO-COPY SELECTION MODE: {0}", IsAutoCopyMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);



        public ICommand EnableFindAndReplaceForSelectedItem => new MpCommand(
            () => {
                SelectedItem.IsFindAndReplaceVisible = true;
            }, () => SelectedItem != null && !SelectedItem.IsFindAndReplaceVisible && SelectedItem.IsTextItem);



        #region Append
        private bool IsCopyItemAppendable(MpCopyItem ci) {
            if (ci == null || ci.Id < 1 || ci.ItemType == MpCopyItemType.Image) {
                return false;
            }
            if (ModalClipTileViewModel.IsPlaceholder) {
                return true;
            }
            return ModalClipTileViewModel.CopyItemType == ci.ItemType;
        }
        private async Task AssignAppendClipTileAsync() {
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
            if (!ModalClipTileViewModel.IsPlaceholder) {
                return;
            }
            MpAvClipTileViewModel append_ctvm = null;
            int append_ciid = 0;
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                if (SelectedItem != null && IsCopyItemAppendable(SelectedItem.CopyItem)) {
                    append_ctvm = SelectedItem;
                    append_ciid = SelectedItem.CopyItemId;
                }
            } else if (PendingNewModels.Count > 0) {
                var most_recent_ci = PendingNewModels[PendingNewModels.Count - 1];
                if (IsCopyItemAppendable(most_recent_ci)) {
                    append_ciid = most_recent_ci.Id;
                }
            } else {
                // activate w/o item and wait (show AppMode change msg)
            }

            if (append_ciid > 0) {
                var append_ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(append_ciid);
                await ModalClipTileViewModel.InitializeAsync(append_ci);
                while (IsBusy) {
                    await Task.Delay(100);
                }
                if (append_ctvm != null) {
                    // pin (or move to front) if exists 
                    //PinTileCommand.Execute(append_ctvm);
                    append_ctvm.OnPropertyChanged(nameof(append_ctvm.IsAppendTrayItem));
                }
            }

            //OnPropertyChanged(nameof(ModalClipTileViewModel));
        }

        private void UpdateAppendModeStateFlags(MpAppendModeFlags flags, string source) {
            IsAppendLineMode = flags.HasFlag(MpAppendModeFlags.AppendLine);
            IsAppendMode = flags.HasFlag(MpAppendModeFlags.Append);
            IsAppendManualMode = flags.HasFlag(MpAppendModeFlags.Manual);
            _appendModeFlags = flags;
            OnPropertyChanged(nameof(AppendModeStateFlags));
        }

        private async Task ActivateAppendModeAsync(bool isAppendLine, bool isManualMode) {
            Dispatcher.UIThread.VerifyAccess();
            while (IsAddingClipboardItem) {
                // if new item is being added, its important to wait for it
                await Task.Delay(100);
            }

            if (ModalClipTileViewModel.IsPlaceholder) {
                // append mode was just toggled ON (param was null)
                await AssignAppendClipTileAsync();
            }
            bool was_append_already_enabled = IsAnyAppendMode && !ModalClipTileViewModel.IsPlaceholder;

            MpAppendModeFlags amf = MpAppendModeFlags.None;
            if (isAppendLine) {
                amf |= MpAppendModeFlags.AppendLine;
            } else {
                amf |= MpAppendModeFlags.Append;
            }
            if (isManualMode) {
                amf |= MpAppendModeFlags.Manual;
            }
            UpdateAppendModeStateFlags(amf, "command");

            MpAppendNotificationViewModel.Instance.OnPropertyChanged(nameof(MpAppendNotificationViewModel.Instance.Title));

            if (was_append_already_enabled) {
                // don't trigger if already activated, the AppendDataChanged() timesout because IsContentLoaded doesn't goto false
                return;
            }

            if (ModalClipTileViewModel.IsPlaceholder) {
                // no item assigned yet so just show enable message
                MpNotificationBuilder.ShowMessageAsync(
                       title: $"MODE CHANGED",
                       body: $"Append{(IsAppendLineMode ? "-Line" : "")} Mode Activated",
                       msgType: MpNotificationType.AppModeChange,
                       iconSourceObj: "NoEntryImage").FireAndForgetSafeAsync();
            } else {
                MpNotificationBuilder.ShowNotificationAsync(MpNotificationType.AppendChanged).FireAndForgetSafeAsync();
            }
        }
        public async Task DeactivateAppendModeAsync(bool isSilent = false) {
            Dispatcher.UIThread.VerifyAccess();

            bool wasAppendLineMode = IsAppendLineMode;

            //var append_tile = AllItems.FirstOrDefault(x => x.IsAppendTrayItem);
            UpdateAppendModeStateFlags(MpAppendModeFlags.None, "command");

            //if (append_tile != null) {
            //    append_tile.OnPropertyChanged(nameof(append_tile.IsAppendTrayItem));
            //}
            if (!isSilent) {

                OnPropertyChanged(nameof(AppendModeStateFlags));
            }


            await ModalClipTileViewModel.InitializeAsync(null);

            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen ||
                isSilent) {
                return;
            }

            MpAvNotificationWindowManager.Instance.HideNotification(MpAppendNotificationViewModel.Instance);

            await MpNotificationBuilder.ShowMessageAsync(
                       title: $"MODE CHANGED",
                       body: $"Append{(wasAppendLineMode ? "-Line" : "")} Mode Deactivated",
                       msgType: MpNotificationType.AppModeChange,
                       iconSourceObj: "NoEntryImage");
        }

        private async Task<bool> UpdateAppendModeAsync(MpCopyItem aci, bool isNew = true) {
            Dispatcher.UIThread.VerifyAccess();
            // NOTE only called in AdddItemFromClipboard when IsAnyAppendMode == true

            if (!IsAnyAppendMode) {
                return false;
            }
            if (ModalClipTileViewModel.IsPlaceholder) {
                await AssignAppendClipTileAsync();
                if (ModalClipTileViewModel.IsPlaceholder) {
                    return false;
                }
            }
            if (!IsCopyItemAppendable(aci)) {
                return false;
            }

            string append_data = aci.ItemData;

            if (ModalClipTileViewModel.CopyItemType == MpCopyItemType.FileList) {
                append_data = await MpAvFileItemCollectionViewModel.CreateFileListEditorFragment(aci);
            }
            if (isNew &&
                MpPrefViewModel.Instance.IgnoreAppendedItems &&
                ModalClipTileViewModel.CopyItemId != aci.Id) {
                aci.DeleteFromDatabaseAsync().FireAndForgetSafeAsync();
            }
            //Task.Run(async () => {
            //    // no need to wait for source updates
            //    if (AppendNotifierViewModel.CopyItemId == aci.Id) {
            //        // ignore self ref source info
            //        return;
            //    }
            //    // clone items sources into append item
            //    var aci_sources = await MpDataModelProvider.GetCopyItemSources(aci.Id);
            //    foreach (var aci_source in aci_sources) {
            //        await MpTransactionSource.CreateAsync(
            //               copyItemId: AppendNotifierViewModel.CopyItemId,
            //               sourceObjId: aci_source.SourceObjId,
            //               sourceType: aci_source.CopyItemSourceType);
            //    }
            //    if (aci.WasDupOnCreate) {
            //        // also ref if exisiting item
            //        await MpTransactionSource.CreateAsync(
            //                copyItemId: AppendNotifierViewModel.CopyItemId,
            //                sourceObjId: aci.Id,
            //                sourceType: MpCopyItemSourceType.CopyItem);
            //    } else {
            //        // delete redundant new item
            //        await aci.DeleteFromDatabaseAsync();
            //    }
            //}).FireAndForgetSafeAsync();

            //while(AppendNotifierViewModel.AppendData != null) {
            //    // probably won't happen but clipboard could change quickly so wait here
            //    // i guess for last item to process
            //    await Task.Delay(100);
            //}
            SetAppendDataCommand.Execute(append_data);
            return true;
        }
        public ICommand ToggleAppendModeCommand => new MpCommand(
            () => {
                if (IsAppendMode) {
                    DeactivateAppendModeAsync().FireAndForgetSafeAsync();
                } else {
                    ActivateAppendModeAsync(false, IsAppendManualMode).FireAndForgetSafeAsync();
                }

            });

        public ICommand ToggleAppendLineModeCommand => new MpCommand(
            () => {
                if (IsAppendLineMode) {
                    DeactivateAppendModeAsync().FireAndForgetSafeAsync();
                } else {
                    ActivateAppendModeAsync(true, IsAppendManualMode).FireAndForgetSafeAsync();
                }
            });
        public ICommand ToggleAppendManualModeCommand => new MpCommand(
            () => {
                bool new_manual_state = !IsAppendManualMode;
                bool append_state = IsAppendLineMode;
                if (!IsAnyAppendMode && new_manual_state) {
                    // append line by default
                    append_state = true;
                }

                ActivateAppendModeAsync(append_state, new_manual_state).FireAndForgetSafeAsync();
            });

        public ICommand SetAppendDataCommand => new MpCommand<object>(
            (dataArg) => {
                var append_data_str = dataArg as string;
                if (string.IsNullOrEmpty(append_data_str)) {
                    return;
                }
                AppendData = append_data_str;
            });

        #endregion


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
