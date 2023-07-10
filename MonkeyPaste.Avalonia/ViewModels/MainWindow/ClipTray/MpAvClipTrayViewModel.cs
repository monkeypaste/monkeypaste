using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CefNet;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonoMac.Foundation;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using FocusManager = Avalonia.Input.FocusManager;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipTrayViewModel :
        MpViewModelBase<MpAvClipTileViewModel>,
        MpIContentBuilder,
        MpIAsyncCollectionObject,
        MpIPagingScrollViewerViewModel,
        MpIActionComponent,
        MpIBoundSizeViewModel,
        MpIContextMenuViewModel,
        MpIContentQueryPage,
        MpIProgressIndicatorViewModel {
        #region Private Variables

        //private double? _query_anchor_percent = null;
        private int? _query_anchor_idx = null;

        private bool _isMainWindowOrientationChanging = false;
        private bool _isLayoutChanging = false;
        private object _addDataObjectContentLock = new object();

        #endregion

        #region Constants


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

        async Task<MpCopyItem> MpIContentBuilder.BuildFromDataObject(object avOrPortableDataObject, bool is_copy) {
            MpAvDataObject mpdo = await Mp.Services.DataObjectHelperAsync.ReadDragDropDataObjectAsync(avOrPortableDataObject) as MpAvDataObject;

            if (mpdo == null) {
                return null;
            }

            if (mpdo.ContainsContentRef()) {
                // internal source, finalize title 
                bool is_partial_internal = mpdo.ContainsPartialContentRef();
                mpdo.FinalizeContentOleTitle(!is_partial_internal, is_copy);
            }
            //string source_ctvm_pub_handle = mpdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT) as string;
            //if (!string.IsNullOrEmpty(source_ctvm_pub_handle)) {
            //    // ido from internal content source
            //    var source_ctvm = AllItems.FirstOrDefault(x => x.PublicHandle == source_ctvm_pub_handle);
            //    if (source_ctvm != null) {
            //        // sub-selection ido
            //        mpdo.SetData(MpPortableDataFormats.LinuxUriList, new string[] { Mp.Services.SourceRefTools.ConvertToInternalUrl(source_ctvm.CopyItem) });
            //    }
            //}
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
        public MpMenuItemViewModel ContextMenuViewModel {
            get {
                if (SelectedItem == null) {
                    return new MpMenuItemViewModel();
                }
                if (SelectedItem.IsTrashed) {
                    return new MpMenuItemViewModel() {
                        SubItems = new List<MpMenuItemViewModel>() {
                            new MpMenuItemViewModel() {
                                Header = @"Restore",
                                IconResourceKey = "ResetImage",
                                Command = RestoreSelectedClipCommand,
                            },
                            new MpMenuItemViewModel() {
                                HasLeadingSeperator = true,
                                Header = @"Permanently Delete",
                                IconResourceKey = "TrashCanImage",
                                Command = DeleteSelectedClipCommand
                            },
                        }
                    };
                }

                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
#if DEBUG
                        new MpMenuItemViewModel() {
                            Header = @"Show Dev Tools",
                            Command = ShowDevToolsCommand
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Reload",
                            Command = ReloadSelectedItemCommand
                        },
                        new MpMenuItemViewModel() { IsSeparator = true },
#endif
                        new MpMenuItemViewModel() {
                            Header = @"Cut",
                            IconResourceKey = "ScissorsImage",
                            Command = CutSelectionFromContextMenuCommand,
                            IsVisible = false,
                            CommandParameter = true,
                            ShortcutArgs = new object[] { MpShortcutType.CutSelection },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Copy",
                            IconResourceKey = "CopyImage",
                            Command = CopySelectionFromContextMenuCommand,
                            ShortcutArgs = new object[] { MpShortcutType.CopySelection },
                        },
                        //new MpMenuItemViewModel() {
                        //    IsSeparator = true,
                        //    Header = "TEST"
                        //},
                        new MpMenuItemViewModel() {
                            Header = "Duplicate",
                            AltNavIdx = 0,
                            IconResourceKey = "DuplicateImage",
                            Command = DuplicateSelectedClipsCommand,
                            ShortcutArgs = new object[] { MpShortcutType.Duplicate },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Paste Here",
                            AltNavIdx = 6,
                            IconResourceKey = "PasteImage",
                            Command = PasteHereFromContextMenuCommand,
                            IsVisible = false,
                            ShortcutArgs = new object[] { MpShortcutType.PasteHere },
                        },
                        new MpMenuItemViewModel() {
                            Header = $"Paste To '{MpAvAppCollectionViewModel.Instance.LastActiveAppViewModel.AppName}'",
                            AltNavIdx = 0,
                            IconId = MpAvAppCollectionViewModel.Instance.LastActiveAppViewModel.IconId,
                            Command = PasteSelectedClipTileFromContextMenuCommand,
                            ShortcutArgs = new object[] { MpShortcutType.PasteSelectedItems },
                        },
                        new MpMenuItemViewModel() {
                            HasLeadingSeperator = true,
                            Header = @"Delete",
                            AltNavIdx = 0,
                            IconResourceKey = "TrashCanImage",
                            Command = TrashSelectedClipCommand,
                            ShortcutArgs = new object[] { MpShortcutType.DeleteSelectedItems },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Rename",
                            AltNavIdx = 0,
                            IconResourceKey = "RenameImage",
                            Command = EditSelectedTitleCommand,
                            ShortcutArgs = new object[] { MpShortcutType.Rename },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Edit",
                            AltNavIdx = 0,
                            IconResourceKey = "EditContentImage",
                            Command = EditSelectedContentCommand,
                            ShortcutArgs = new object[] { MpShortcutType.ToggleContentReadOnly },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Find and Replace...",
                            AltNavIdx = 0,
                            IconResourceKey = "SearchImage",
                            Command = EnableFindAndReplaceForSelectedItem,
                            ShortcutArgs = new object[] { MpShortcutType.FindAndReplaceSelectedItem },
                        },
                        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel2(SelectedItem,true),
                        new MpMenuItemViewModel() {IsSeparator = true},
                        SelectedItem.TransactionCollectionViewModel.ContextMenuViewModel,
                        new MpMenuItemViewModel() {IsSeparator = true},
                        MpAvAnalyticItemCollectionViewModel.Instance.GetContentContextMenuItem(SelectedItem.CopyItemType),
                        new MpMenuItemViewModel() {
                            HasLeadingSeperator = true,
                            Header = @"Collections",
                            AltNavIdx = 0,
                            IconResourceKey = "PinToCollectionImage",
                            SubItems =
                                MpAvTagTrayViewModel.Instance
                                    .RootLinkableItems
                                    .Select(x=>x.ContentMenuItemViewModel)
                                    .ToList()
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
        public bool IsTouchScrolling { get; set; }
        public bool CanTouchScroll =>
            Mp.Services.PlatformInfo.IsTouchInputEnabled &&
            QueryItems.All(x => !x.IsTileDragging) &&
            QueryItems.All(x => !x.CanResize) &&
            QueryItems.All(x => !x.CanResize) &&
            (HoverItem == null ||
             HoverItem.IsPinned ||
             !HoverItem.IsSubSelectionEnabled);

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
        public MpPoint MaxScrollOffset => new(MaxScrollOffsetX, MaxScrollOffsetY);
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
                if (HoverItem == null) {
                    return true;
                }
                // TODO? giving item scroll priority maybe better by checking if content exceeds visible boundaries here
                if ((HoverItem.IsVerticalScrollbarVisibile && HoverItem.IsSubSelectionEnabled) ||
                    HoverItem.TransactionCollectionViewModel.IsTransactionPaneOpen) {
                    // when tray is not scrolling (is still) and mouse is over sub-selectable item keep tray scroll frozen
                    return false;
                }
                return true;
            }
        }

        public bool CanScrollX =>
            CanScroll && DefaultScrollOrientation == Orientation.Horizontal; //(LayoutType == MpClipTrayLayoutType.Grid || ListOrientation == Orientation.Horizontal);

        public bool CanScrollY =>
            CanScroll && DefaultScrollOrientation == Orientation.Vertical; //(LayoutType == MpClipTrayLayoutType.Grid || ListOrientation == Orientation.Vertical);
        public bool IsThumbDraggingX { get; set; } = false;
        public bool IsThumbDraggingY { get; set; } = false;
        public bool IsThumbDragging => IsThumbDraggingX || IsThumbDraggingY;

        public bool IsForcingScroll { get; set; }

        private void FindTotalTileSize() {
            // NOTE this is to avoid making TotalTile Width/Height auto
            // and should only be called on a requery or on content resize (or event better only on resize complete)
            //MpSize totalTileSize = MpSize.Empty;
            //if (Mp.Services.Query.TotalAvailableItemsInQuery > 0) {
            //    var result = FindTileRectOrQueryIdxOrTotalTileSize_internal(
            //        queryOffsetIdx: -1,
            //        scrollOffsetX: -1,
            //        scrollOffsetY: -1);
            //    if (result is MpSize) {
            //        totalTileSize = (MpSize)result;
            //    }
            //}

            MpSize totalTileSize = GetQueryPosition(Mp.Services.Query.TotalAvailableItemsInQuery).ToPortableSize();
            QueryTrayTotalTileWidth = totalTileSize.Width;
            QueryTrayTotalTileHeight = totalTileSize.Height;
        }

        private MpRect GetQueryTileRect(int queryOffsetIdx, MpRect prevOffsetRect) {
            //object result = FindTileRectOrQueryIdxOrTotalTileSize_internal(
            //                    queryOffsetIdx: queryOffsetIdx,
            //                    scrollOffsetX: -1,
            //                    scrollOffsetY: -1,
            //                    prevOffsetRect: prevOffsetRect);
            //if (result is MpRect tileRect) {
            //    return tileRect;
            //}
            //return MpRect.Empty;
            MpPoint loc = GetQueryPosition(queryOffsetIdx);
            MpSize size = GetQueryItemSize(queryOffsetIdx);
            return new MpRect(loc, size);
        }

        private int FindJumpTileIdx(double scrollOffsetX, double scrollOffsetY, out MpRect tileRect) {
            //object result = FindTileRectOrQueryIdxOrTotalTileSize_internal(
            //                    queryOffsetIdx: -1,
            //                    scrollOffsetX: scrollOffsetX,
            //                    scrollOffsetY: scrollOffsetY);
            //if (result is object[] resultParts) {
            //    tileRect = resultParts[1] as MpRect;
            //    return (int)resultParts[0];
            //}
            //tileRect = MpRect.Empty;
            //return -1;
            int qidx = 0;
            if (LayoutType == MpClipTrayLayoutType.Stack) {
                if (ListOrientation == Orientation.Horizontal) {
                    qidx = (int)(scrollOffsetX / DefaultQueryItemWidth);
                } else {
                    qidx = (int)(scrollOffsetY / DefaultQueryItemHeight);
                }
            } else {
                if (ListOrientation == Orientation.Horizontal) {
                    qidx = (int)(scrollOffsetY / DefaultQueryItemWidth) * (CurGridFixedCount - 1);
                } else {
                    qidx = (int)(scrollOffsetX / DefaultQueryItemHeight) * (CurGridFixedCount - 1);
                }
            }
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
            int row, col;
            if (ListOrientation == Orientation.Horizontal) {
                //row = (int)Math.Round((DefaultQueryItemWidth * qidx) / DesiredMaxTileRight);
                //col = qidx % CurGridFixedCount;
                row = (int)Math.Floor((double)qidx / (double)CurGridFixedCount);
                col = qidx % CurGridFixedCount;
            } else {
                //row = qidx % CurGridFixedCount;
                //col = (int)Math.Round((DefaultQueryItemHeight * qidx) / DesiredMaxTileBottom);
                row = qidx % CurGridFixedCount;
                col = (int)Math.Floor((double)qidx / (double)CurGridFixedCount);
            }
            return new MpPoint(col * DefaultQueryItemWidth, row * DefaultQueryItemHeight);
        }


        #region Default Tile Layout 

        public double DesiredMaxTileRight {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return ObservedQueryTrayScreenWidth - ScrollBarFixedAxisSize;
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
            double qw = DEFAULT_ITEM_SIZE - QueryTrayVerticalScrollBarWidth - safe_pad;
            double qh = DEFAULT_ITEM_SIZE - QueryTrayHorizontalScrollBarHeight - safe_pad;
            double pw = DEFAULT_ITEM_SIZE - safe_pad;
            double ph = DEFAULT_ITEM_SIZE - safe_pad;

            if (ListOrientation == Orientation.Vertical) {
                qh = DEFAULT_UNEXPANDED_HEIGHT;
                ph = DEFAULT_UNEXPANDED_HEIGHT;
            } else if (LayoutType == MpClipTrayLayoutType.Grid &&
                        Mp.Services.Query.TotalAvailableItemsInQuery > CurGridFixedCount) {
                // when there's multiple query rows shorten height a bit to 
                // hint theres more there (if not multiple rows, don't shorten looks funny
                qh *= 0.7;
            }

            _defaultQueryItemWidth = qw;
            _defaultQueryItemHeight = qh;

            _defaultPinItemWidth = pw;
            _defaultPinItemHeight = ph;

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
        public ObservableCollection<MpAvClipTileViewModel> Items { get; set; } = new ObservableCollection<MpAvClipTileViewModel>();
        public ObservableCollection<MpAvClipTileViewModel> PinnedItems { get; set; } = new ObservableCollection<MpAvClipTileViewModel>();

        public IList<MpAvClipTileViewModel> InternalPinnedItems =>
            PinnedItems
            .Where(x => !x.IsWindowOpen && !x.IsAppendNotifier)
            .Take(MpPrefViewModel.Instance.MaxStagedClipCount)
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
            //    if (value == null) {
            //        AllItems.ForEach(x => x.IsSelected = false);
            //    } else {
            //        AllItems.ForEach(x => x.IsSelected = x.CopyItemId == value.CopyItemId);
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
                    //SelectedItem = value;
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
                    //SelectedItem = value;
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

        //Items
        //.Where(x => x.IsAnyQueryCornerVisible && !x.IsPlaceholder)
        //.OrderBy(x => x.TrayX)
        //.ThenBy(x => x.TrayY);

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

        public int CurGridFixedCount {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 1;
                }
                if (ListOrientation == Orientation.Horizontal) {
                    return (int)(DesiredMaxTileRight / DefaultQueryItemWidth);
                } else {
                    return (int)(DesiredMaxTileBottom / DefaultQueryItemHeight);
                }
            }
        }

        public bool IsQueryItemResizeEnabled =>
            LayoutType == MpClipTrayLayoutType.Stack;


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
                    if (MpAvTagTrayViewModel.Instance.LastSelectedActiveItem == null) {
                        return "No selection.";
                    }
                    tag_name = MpAvTagTrayViewModel.Instance.LastSelectedActiveItem.TagName;
                }
                return $"'{tag_name}' has no results.";
            }
        }

        #endregion

        #region State

        public bool IsQueryResizeEnabled =>
            LayoutType == MpClipTrayLayoutType.Stack;

        public bool IsRestoringSelection { get; set; }

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
                    return 40;
                }
                return 5;
                //}
            }
        }
        public bool IsTitleLayersVisible { get; set; } = true;
        public bool IsMarqueeEnabled { get; set; } = true;

        // this is to help keep new items added pin tray visible when created
        // but avoid overriding user splitter changes DURING one of their workflows
        // and presuming that unless the window hides its still a workflow
        public bool HasUserAlteredPinTrayWidthSinceWindowShow { get; set; } = false;

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

        public bool IsAppPaused { get; set; }


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

        public bool IsRequerying { get; set; } = false;
        public bool IsQuerying { get; set; } = false;
        public bool IsSubQuerying { get; set; } = false;
        public int SparseLoadMoreRemaining { get; set; }

        public MpQuillPasteButtonInfoMessage CurPasteInfoMessage { get; private set; }

        #region Drag Drop
        public bool IsAnyDropOverTrays { get; private set; }
        //public bool IsAnyTileDragging => 
        //    AllItems.Any(x => x.IsTileDragging) || 
        //    (MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count > 0 &&
        //     MpAvPersistentClipTilePropertiesHelper.IsPersistentTileDraggingEditable_ById(
        //         MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels[0].Id));

        //public bool IsExternalDragOverClipTrayContainer { get; set; }
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

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            OnPropertyChanged(nameof(LayoutType));

            if (IsAppPaused == MpPrefViewModel.Instance.IsClipboardListeningOnStartup) {
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

            await SetCurPasteInfoMessageAsync(Mp.Services.ProcessWatcher.LastProcessInfo);

            IsBusy = false;
        }


        public async Task<MpAvClipTileViewModel> CreateClipTileViewModelAsync(MpCopyItem ci, int queryOffsetIdx = -1) {
            MpAvClipTileViewModel ctvm = new MpAvClipTileViewModel(this);
            await ctvm.InitializeAsync(ci, queryOffsetIdx);
            return ctvm;
        }

        public void ValidateQueryTray() {
            var dups =
                Items.Where(x => x.QueryOffsetIdx >= 0 && Items.Any(y => y != x && x.QueryOffsetIdx == y.QueryOffsetIdx));
            var skips =
                Enumerable.Range(HeadQueryIdx, TailQueryIdx - HeadQueryIdx)
                .Where(x => QueryItems.All(y => y.QueryOffsetIdx != x));

            if (!dups.Any() && !skips.Any()) {
                return;
            }
            MpDebug.Break($"Query validation failed. Either offsets skipped or duplicated", true);
            if (dups.Count() > 0) {
                dups
                    .OrderByDescending(x => x.TileCreatedDateTime)
                    .Skip(1)
                    .ForEach(x => x.TriggerUnloadedNotification(false));

            }
            if (skips.Any()) {

            }
        }
        public override string ToString() {
            return $"ClipTray";
        }

        public void RefreshQueryTrayLayout(MpAvClipTileViewModel fromItem = null) {
            UpdateDefaultItemSize();
            FindTotalTileSize();

            fromItem = fromItem == null ? HeadItem : fromItem;
            QueryItems.ForEach(x => UpdateTileRectCommand.Execute(x));

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

            double pad = 0;
            MpRect svr = new MpRect(0, 0, ObservedQueryTrayScreenWidth, ObservedQueryTrayScreenHeight);
            MpRect ctvm_rect = ctvm.ScreenRect;

            MpPoint delta_scroll_offset = new MpPoint();
            //if (DefaultScrollOrientation == Orientation.Horizontal) {
            if (ctvm_rect.Left < svr.Left) {
                //item is outside on left
                delta_scroll_offset.X = ctvm_rect.Left - svr.Left - pad;
            } else if (ctvm_rect.Right > svr.Right) {
                //item is outside on right
                delta_scroll_offset.X = ctvm_rect.Right - svr.Right + pad;
            }
            //} else {
            if (ctvm_rect.Top < svr.Top) {
                //item is outside above
                delta_scroll_offset.Y = ctvm_rect.Top - svr.Top - pad;
            } else if (ctvm_rect.Bottom > svr.Bottom) {
                //item is outside below
                delta_scroll_offset.Y = ctvm_rect.Bottom - svr.Bottom + pad;
            }
            //}

            var target_offset = ScrollOffset + delta_scroll_offset;
            ScrollVelocity = MpPoint.Zero;
            ForceScrollOffset(target_offset);
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
                MpDebug.Break();
            } else {
                pasted_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(avm.App);
            }
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
        }

        public MpSize GetCurrentDefaultPinTrayRatio() {
            MpSize p_ratio = new MpSize(1, 1);
            //double pin_tray_var_dim_ratio = 0.95;
            //if (!IsQueryTrayEmpty) {
            //    if (IsPinTrayEmpty) {
            //        pin_tray_var_dim_ratio = 0.1;
            //    } else {
            //        pin_tray_var_dim_ratio = 0.5;
            //    }
            //}
            //if (!MpAvTagTrayViewModel.Instance.IsAnyTagActive) {
            //    pin_tray_var_dim_ratio = 0.5d;
            //}
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

            if (_isProcessingCap) {
                MpConsole.WriteLine($"Account cap refreshed IGNORED (already processing). Source: '{source}' Args: '{arg.ToStringOrDefault()}'");
                return;
            }
            _isProcessingCap = true;

            string last_cap_info = Mp.Services.AccountTools.LastCapInfo.ToString();
            var cap_info = await Mp.Services.AccountTools.RefreshCapInfoAsync();
            MpConsole.WriteLine($"Account cap refreshed. Source: '{source}' Args: '{arg.ToStringOrDefault()}' Info:", true);
            MpConsole.WriteLine(cap_info.ToString(), false, true);

            // triggers:
            // init
            // content add
            // content delete
            // (un)link tag
            // block

            MpUserAccountType account_type = Mp.Services.AccountTools.CurrentAccountType;
            int cur_content_cap = Mp.Services.AccountTools.GetContentCapacity(account_type);
            int cur_trash_cap = Mp.Services.AccountTools.GetTrashCapacity(account_type);

            bool apply_changes = false;
            string cap_msg_title_suffix = string.Empty;
            string cap_msg_icon = string.Empty;
            var cap_msg_sb = new StringBuilder();
            MpNotificationType cap_msg_type = MpNotificationType.None;

            if (source == MpAccountCapCheckType.Add) {
                if (cap_info.ToBeTrashed_ciid > 0) {
                    cap_msg_icon = MpContentCapInfo.NEXT_TRASH_IMG_RESOURCE_KEY;
                    cap_msg_title_suffix = $"Content Capacity Reached!";
                    cap_msg_sb.AppendLine(
                        $"Max '{account_type.ToString()}' storage is {cur_content_cap}.");
                }
                if (cap_info.ToBeRemoved_ciid > 0) {
                    cap_msg_icon = MpContentCapInfo.NEXT_REMOVE_IMG_RESOURCE_KEY;
                    if (string.IsNullOrEmpty(cap_msg_title_suffix)) {
                        cap_msg_title_suffix = $"Archive Capacity Reached!";
                    } else {
                        cap_msg_title_suffix = $"Content & Archive Capacity Reached!";
                    }
                    cap_msg_sb.AppendLine(
                        $"Max archive is {cur_trash_cap}.");
                }
                if (!string.IsNullOrEmpty(cap_msg_sb.ToString())) {
                    apply_changes = true;
                    cap_msg_type = MpNotificationType.ContentCapReached;
                    cap_msg_sb.AppendLine(string.Empty);
                    if (IsAddingStartupClipboardItem) {
                        cap_msg_sb.AppendLine($"* To prevent add on startup, uncheck '{nameof(MpPrefViewModel.Instance.AddClipboardOnStartup).ToLabel()}' or '{nameof(MpPrefViewModel.Instance.IsClipboardListeningOnStartup).ToLabel()}' ");
                    } else {
                        cap_msg_sb.AppendLine($"* You can hide these warnings by clicking 'hide all' from the options menu above 😉");
                    }
                }

            } else if (source == MpAccountCapCheckType.Block) {
                // block refresh called BEFORE an add would occur to check favorite count again and avoid delete
                // since tag linking doesn't refresh caps, this does it when last add set account to block state
                if (Mp.Services.AccountTools.IsContentAddPausedByAccount) {
                    // no linking changes, add will be blocked
                    cap_msg_title_suffix = "Add Blocked";
                    cap_msg_sb.AppendLine($"Delete or unlink something from 'Favorites' to add more.");
                    cap_msg_sb.AppendLine($"Max '{account_type.ToString()}' storage is {cur_content_cap}.");
                    cap_msg_icon = MpContentCapInfo.ADD_BLOCKED_RESOURCE_KEY;
                    cap_msg_type = MpNotificationType.ContentAddBlockedByAccount;
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

                await TrashItemByCopyItemIdAsync(cap_info.ToBeTrashed_ciid);
                await DeleteItemByCopyItemIdAsync(cap_info.ToBeRemoved_ciid);
            }
            AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IconResourceObj)));
            AllActiveItems.ForEach(x => x.OnPropertyChanged(nameof(x.IsAnyNextCapByAccount)));
            _isProcessingCap = false;
            if (cap_msg_type == MpNotificationType.None) {
                return;
            }
            Mp.Services.NotificationBuilder.ShowMessageAsync(
                       title: $"'{account_type}' {cap_msg_title_suffix}",
                       body: cap_msg_sb.ToString(),
                       msgType: cap_msg_type,
                       iconSourceObj: cap_msg_icon,
                       maxShowTimeMs: MpContentCapInfo.MAX_CAP_NTF_SHOW_TIME_MS).FireAndForgetSafeAsync();
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
                    await cia_ctvm.InitializeAsync(cia_ctvm.CopyItem);
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
            if (e is MpCopyItem ci &&
                AllActiveItems.FirstOrDefault(x => x.CopyItemId == ci.Id) is MpAvClipTileViewModel ci_ctvm) {
                if (ci_ctvm.HasModelChanged) {
                    // this means the model has been updated from the view model so ignore
                } else {
                    Dispatcher.UIThread.Post(async () => {
                        await ci_ctvm.InitializeAsync(ci);
                        //wait for model to propagate then trigger view to reload
                        if (ci_ctvm.GetContentView() is MpIContentView cv) {
                            cv.LoadContentAsync().FireAndForgetSafeAsync();
                        }
                    });
                }
                return;
            }
        }

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpShortcut sc &&
                AllActiveItems.FirstOrDefault(x => sc.IsShortcutCommand(x)) is MpAvClipTileViewModel sc_ctvm) {
                sc_ctvm.OnPropertyChanged(nameof(sc_ctvm.KeyString));
                return;
            }

            if (e is MpCopyItem ci) {
                if (AppendClipTileViewModel != null &&
                    ci.Id == AppendClipTileViewModel.CopyItemId &&
                    IsAnyAppendMode) {
                    DeactivateAppendMode();
                }
                MpAvPersistentClipTilePropertiesHelper.RemoveProps(ci.Id);

                //Mp.Services.Query.PageTools.AddIdToOmit(ci.Id);
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
                            AllItems.ForEach(x => x.IsSelected = x == sel_ctvm);
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
            } else if (e is MpCopyItemTag cit &&
                        MpAvTagTrayViewModel.Instance.LastSelectedActiveItem is MpAvTagTileViewModel sttvm &&
                        sttvm.IsLinkTag &&
                        !sttvm.IsAllTag) {

                // check if unlink is part of current query
                bool is_part_of_query =
                    sttvm
                    .SelfAndAllDescendants
                    .Cast<MpAvTagTileViewModel>()
                    .Select(x => x.TagId)
                    .Any(x => x == cit.TagId);

                if (is_part_of_query) {
                    // when unlinked item is part of current query remove its offset and do a reset query
                    Mp.Services.Query.NotifyQueryChanged();
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
                    //MpPlatformWrapper.Services.Cursor.IsCursorFrozen = HasScrollVelocity;

                    //if (HasScrollVelocity) {
                    //    Mp.Services.Cursor.UnsetCursor(null);
                    //} else {
                    //    var hctvm = Items.FirstOrDefault(x => x.IsHovering);
                    //    if (IsAnyBusy) {
                    //        OnPropertyChanged(nameof(IsBusy));
                    //    }
                    //}
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
                    //RefreshQueryTrayLayout();
                    ScrollToAnchor();
                    //CheckLoadMore();
                    //SetScrollAnchor();
                    break;
                case MpMessageType.SelectedSidebarItemChangeBegin:
                    SetScrollAnchor();
                    break;
                case MpMessageType.SelectedSidebarItemChangeEnd:
                    //RefreshQueryTrayLayout();
                    ScrollToAnchor();
                    //CheckLoadMore();
                    break;
                case MpMessageType.PinTrayResizeBegin:
                    SetScrollAnchor();
                    break;
                case MpMessageType.PinTrayResizeEnd:
                    ScrollToAnchor();
                    //CheckLoadMore();
                    //SetScrollAnchor();
                    break;
                case MpMessageType.SidebarItemSizeChanged:
                    OnPropertyChanged(nameof(MaxContainerScreenWidth));
                    OnPropertyChanged(nameof(MaxContainerScreenHeight));
                    break;
                // LAYOUT CHANGE
                case MpMessageType.MainWindowInitialOpenComplete:
                    ResetTraySplitterCommand.Execute(null);
                    break;
                case MpMessageType.PreTrayLayoutChange:
                    _isLayoutChanging = true;
                    SetScrollAnchor();
                    ResetItemSizes(true, false);
                    break;
                case MpMessageType.PostTrayLayoutChange:
                    //RefreshQueryTrayLayout();
                    ScrollToAnchor();
                    _isLayoutChanging = false;
                    break;
                // MAIN WINDOW SIZE
                case MpMessageType.MainWindowSizeChangeBegin:
                    SetScrollAnchor();
                    break;
                case MpMessageType.MainWindowSizeChanged:
                    //RefreshQueryTrayLayout();
                    ScrollToAnchor();
                    //CheckLoadMore();
                    break;
                case MpMessageType.MainWindowSizeChangeEnd:
                    // NOTE Size reset doesn't call changed so treat end as changed too
                    //RefreshQueryTrayLayout();
                    ScrollToAnchor();
                    //CheckLoadMore();
                    //SetScrollAnchor();
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

                // TRAY ZOOM
                case MpMessageType.TrayZoomFactorChangeBegin:
                    MpConsole.WriteLine("Zoom change begin: " + ZoomFactor);
                    SetScrollAnchor();
                    break;
                case MpMessageType.TrayZoomFactorChanged:
                    MpConsole.WriteLine("Zoom changed: " + ZoomFactor);
                    //RefreshQueryTrayLayout();
                    ScrollToAnchor();
                    //CheckLoadMore(true);
                    break;
                case MpMessageType.TrayZoomFactorChangeEnd:
                    MpConsole.WriteLine("Zoom change end: " + ZoomFactor);
                    //RefreshQueryTrayLayout();
                    ScrollToAnchor();
                    //CheckLoadMore(true);

                    //SetScrollAnchor();
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
                case MpMessageType.MainWindowClosed:
                    // reset so tray will autosize/bringIntoView on ListBox items changed (since actual size is not bound)
                    HasUserAlteredPinTrayWidthSinceWindowShow = false;
                    break;

                // QUERY

                case MpMessageType.RequeryCompleted:
                    OnPropertyChanged(nameof(EmptyQueryTrayText));
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
                    } else {
                        RefreshQueryPageOffsetsAsync().FireAndForgetSafeAsync();
                    }

                    break;
                case MpMessageType.QueryChanged:
                    QueryCommand.Execute(null);
                    break;
                case MpMessageType.SubQueryChanged:
                    QueryCommand.Execute(ScrollOffset);
                    break;
                case MpMessageType.TotalQueryCountChanged:
                    OnPropertyChanged(nameof(IsQueryEmpty));
                    OnPropertyChanged(nameof(Mp.Services.Query.TotalAvailableItemsInQuery));
                    //AllItems.ForEach(x => x.UpdateQueryOffset());
                    RefreshQueryPageOffsetsAsync().FireAndForgetSafeAsync();
                    break;

                // DND
                case MpMessageType.DropWidgetEnabledChanged:
                    OnPropertyChanged(nameof(IsAnyMouseModeEnabled));
                    break;
                    //case MpMessageType.ItemDragBegin:
                    //    OnPropertyChanged(nameof(IsAnyTileDragging));
                    //    if(DragItem == null) {
                    //        // shant be true
                    //        MpDebug.Break();
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
        private void ProcessWatcher_OnAppActivated(object sender, MpPortableProcessInfo e) {
            Dispatcher.UIThread.Post(async () => {
                await SetCurPasteInfoMessageAsync(e);
            }, DispatcherPriority.Background);
        }
        private async Task SetCurPasteInfoMessageAsync(MpPortableProcessInfo e) {

            while (MpAvAppCollectionViewModel.Instance.IsAnyBusy) {
                // wait if app new/db updating 
                await Task.Delay(100);
            }
            CurPasteInfoMessage = new MpQuillPasteButtonInfoMessage();
            var active_avm = MpAvAppCollectionViewModel.Instance.GetAppByProcessInfo(e);
            if (active_avm == null) {
                // let editor use fallback
            } else {
                CurPasteInfoMessage.pasteButtonTooltipText = string.IsNullOrEmpty(e.ApplicationName) ? e.MainWindowTitle : e.ApplicationName;
                CurPasteInfoMessage.pasteButtonIconBase64 = await MpDataModelProvider.GetDbImageBase64ByIconIdAsync(active_avm.IconId);
            }

            string msg = $"enableSubSelection_ext('{CurPasteInfoMessage.SerializeJsonObjectToBase64()}')";

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
            bool is_startup_ido = MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening;

            bool is_ext_change = !MpAvWindowManager.IsAnyActive || is_startup_ido;

            bool is_change_ignored =
                !is_startup_ido &&
                (IsAppPaused ||
                 (MpPrefViewModel.Instance.IgnoreInternalClipboardChanges && !is_ext_change));
            if (is_startup_ido && !is_change_ignored && !MpPrefViewModel.Instance.AddClipboardOnStartup) {
                // ignore startup item
                is_change_ignored = true;
            }

            if (is_change_ignored) {
                MpConsole.WriteLine("Clipboard Change Ignored by tray", true);
                MpConsole.WriteLine($"IsMainWindowLoading: {MpAvMainWindowViewModel.Instance.IsMainWindowLoading}");
                MpConsole.WriteLine($"IsAppPaused: {IsAppPaused}");
                MpConsole.WriteLine($"IsThisAppActive: {MpAvWindowManager.IsAnyActive}");
                MpConsole.WriteLine($"is_startup_ido: {is_startup_ido}");
                MpConsole.WriteLine($"IgnoreInternalClipboardChanges: {MpPrefViewModel.Instance.IgnoreInternalClipboardChanges}", false, true);
                return;
            }

            Dispatcher.UIThread.Post(async () => {
                if (is_startup_ido) {
                    IsAddingStartupClipboardItem = true;
                    //await Task.Delay(500);
                    //while (IsAnyBusy) {
                    //    await Task.Delay(100);
                    //}
                }
                await AddItemFromDataObjectAsync(mpdo);
                IsAddingStartupClipboardItem = false;
            });
        }

        private async Task TrashItemByCopyItemIdAsync(int ciid) {
            if (ciid == 0) {
                return;
            }
            // add link to trash tag which caches ciid
            await MpAvTagTrayViewModel.Instance.TrashTagViewModel
                .LinkCopyItemCommand.ExecuteAsync(ciid);

            MpAvTagTrayViewModel.Instance.UpdateAllClipCountsAsync().FireAndForgetSafeAsync(this);


            var trashed_ctvm = AllActiveItems.FirstOrDefault(x => x.CopyItemId == ciid);
            if (trashed_ctvm == null) {
                return;
            }


            if (trashed_ctvm.IsPinned) {

                bool needs_query_refresh =
                    trashed_ctvm.HasPinPlaceholder;
                UnpinTileCommand.Execute(trashed_ctvm);
                if (needs_query_refresh) {
                    // needs requery to remove placeholder
                    // or the frozen trash item (when query is not trash query)
                    // is used
                    while (IsAnyBusy) { await Task.Delay(100); }
                } else {
                    return;
                }
            }
            // trigger in place requery to remove trashed item
            QueryCommand.Execute(string.Empty);
        }
        private async Task RefreshQueryPageOffsetsAsync() {
            return;
            Dispatcher.UIThread.VerifyAccess();

            while (IsAnyBusy) {
                await Task.Delay(100);
            }
            int head_offset_idx = -1;
            if (!IsQueryTrayEmpty) {
                var ordered_query_item_ids = QueryItems.OrderBy(x => x.QueryOffsetIdx).Select(x => x.IsPinPlaceholder ? x.PinPlaceholderCopyItemId : x.CopyItemId).ToList();
                head_offset_idx = await Mp.Services.Query.FetchItemOffsetIdxAsync(ordered_query_item_ids.First());
                for (int i = 0; i < ordered_query_item_ids.Count; i++) {
                    int new_offset_idx = head_offset_idx + i;
                    var ctvm = QueryItems.FirstOrDefault(x => x.IsPinPlaceholder ? x.PinPlaceholderCopyItemId == ordered_query_item_ids[i] : x.CopyItemId == ordered_query_item_ids[i]);
                    if (ctvm == null) {
                        continue;
                    }
                    ctvm.UpdateQueryOffset(new_offset_idx);
                }

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
        private async Task InitDefaultPlaceholdersAsync() {
            Items.Clear();
            for (int i = 0; i < DefaultLoadCount; i++) {
                var ctvm = await CreateClipTileViewModelAsync(null);
                Items.Add(ctvm);
            }

            while (Items.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
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
            OnPropertyChanged(nameof(IsAnyTilePinned));
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

            var sw = Stopwatch.StartNew();
            while (IsAnyBusy) {
                if (sw.ElapsedMilliseconds > 3_000) {
                    break;
                }
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(IsQueryEmpty));
            OnPropertyChanged(nameof(IsPinTrayEmpty));
            OnPropertyChanged(nameof(EmptyQueryTrayText));
            OnPropertyChanged(nameof(IsQueryTrayEmpty));
            OnPropertyChanged(nameof(IsQueryHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsQueryVerticalScrollBarVisible));
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
        private void ForceScrollOffset(MpPoint newOffset) {
            if ((newOffset - ScrollOffset).Length < 1) {
                // avoid double query
                MpConsole.WriteLine($"Force ScrollOffset reject length: {(newOffset - ScrollOffset).Length}");
                return;
            }
            var old_offset = ScrollOffset;
            IsForcingScroll = true;
            ForceScrollOffsetX(newOffset.X);
            ForceScrollOffsetY(newOffset.Y);
            IsForcingScroll = false;
            MpAvPagingListBoxExtension.ForceScrollOffset(newOffset);
            MpConsole.WriteLine($"ScrollOffset forced from '{old_offset}' to '{newOffset}'");
        }
        #endregion

        #region Scroll Anchor

        private int FindCurScrollAnchor() {
            if (SelectedItem != null &&
                !SelectedItem.IsPinned) {
                // prefer to anchor to selection
                return SelectedItem.QueryOffsetIdx;
            }
            if (VisibleFromTopLeftQueryItems.FirstOrDefault() is MpAvClipTileViewModel anchor_ctvm) {
                // anchor to item with top left visible closest to top left
                return anchor_ctvm.QueryOffsetIdx;
            }
            return 0;
        }
        private bool CanScrollToAnchor() {
            if (!_query_anchor_idx.HasValue) {
                // no anchor set
                return false;
            }

            if (_query_anchor_idx.Value == FindCurScrollAnchor() &&
                VisibleFromTopLeftQueryItems.Any(x => x.QueryOffsetIdx == _query_anchor_idx.Value)) {
                // already at anchor

                return false;
            }
            return true;
        }
        private void SetScrollAnchor() {
            if (_query_anchor_idx.HasValue) {
                MpConsole.WriteLine($"SetScrollAnchor ignored, anchor already set.");
            }
            _query_anchor_idx = FindCurScrollAnchor();
            MpConsole.WriteLine($"[SET] Anchor idx: {_query_anchor_idx.Value}");
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

                //var anchor_offset = MaxScrollOffset * _query_anchor_idx.Value;
                QueryCommand.Execute(_query_anchor_idx.Value);
                _query_anchor_idx = null;
                CheckLoadMore();
            });
        }

        #endregion

        #region Keyboard Tile Navigation
        private bool CanTileNavigate() {
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
            //if (SelectedItem != focus_ctvm) {
            //    MpConsole.WriteLine($"TileNav check overriding selected item from '{SelectedItem}' to focus item '{focus_ctvm}'");
            //    focus_ctvm.IsSelected = true;
            //    MpDebug.Assert(SelectedItem == focus_ctvm, $"Selection not updating.");
            //    return false;
            //}

            bool is_editor_nav =
                SelectedItem != null && SelectedItem.IsSubSelectionEnabled && SelectedItem.GetContentView() is Control cv && (cv.IsFocused || cv.IsKeyboardFocusWithin);
            bool is_title_nav =
                SelectedItem != null && !SelectedItem.IsTitleReadOnly && SelectedItem.IsTitleFocused;

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
                //SelectedItem = target_ctvm;
                target_ctvm.IsSelected = true;
            } else {

            }
            IsArrowSelecting = false;
        }

        private async Task<MpAvClipTileViewModel> GetNeighborByRowOffsetAsync(MpAvClipTileViewModel ctvm, bool is_next) {
            int row_offset = is_next ? 1 : -1;
            double pad = 10 * (row_offset > 0 ? 1 : -1);
            double offset_y = row_offset > 0 ? ctvm.ScreenRect.Bottom : ctvm.ScreenRect.Top;
            var compare_loc = ctvm.ScreenRect.Centroid() + new MpPoint(0, offset_y + pad);
            MpAvClipTileViewModel target_ctvm = null;
            if (ctvm.IsPinned) {

                target_ctvm = InternalPinnedItems.FirstOrDefault(x => x.ScreenRect.Contains(compare_loc));
                if (target_ctvm != null) {
                    return target_ctvm;
                }
                if (ListOrientation == Orientation.Horizontal ||
                        row_offset < 0) {
                    // no tile above or below
                    return null;
                }
                // at bottom of vert pin tray, select nearest query item
                return VisibleQueryItems.OrderBy(x => x.ScreenRect.Centroid().Distance(compare_loc)).FirstOrDefault();
            }
            if (LayoutType == MpClipTrayLayoutType.Stack &&
                ListOrientation == Orientation.Horizontal) {
                // horizontal stack, no row change
                return null;
            }
            if (ListOrientation == Orientation.Horizontal &&
                LayoutType == MpClipTrayLayoutType.Grid) {
                // horizontal grid qidx's are left-to-right 
                // adj row offset by fixed row count
                row_offset *= CurGridFixedCount;
            }
            int target_qidx = ctvm.QueryOffsetIdx + row_offset;
            if (target_qidx >= Mp.Services.Query.TotalAvailableItemsInQuery) {
                // this last item
                return null;
            }
            if (target_qidx < 0) {
                // select nearest pin item
                return InternalPinnedItems.OrderBy(x => x.ScreenRect.Centroid().Distance(compare_loc)).FirstOrDefault();
            }
            target_ctvm = QueryItems.FirstOrDefault(x => x.QueryOffsetIdx == target_qidx);
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
                if (target_pidx < 0) {
                    // target is before all pinned items
                    return null;
                }
                if (target_pidx >= InternalPinnedItems.Count) {
                    // neighbor is beyond pinned items
                    target_pidx = target_pidx - InternalPinnedItems.Count;
                    if (target_pidx < VisibleQueryItems.Count()) {
                        if (DefaultScrollOrientation == Orientation.Horizontal) {
                            return VisibleQueryItems.OrderBy(x => ctvm.TrayX).ElementAt(target_pidx);
                        }
                        return VisibleQueryItems.OrderBy(x => ctvm.TrayY).ElementAt(target_pidx);
                    }
                    return null;
                }
                return InternalPinnedItems[target_pidx];
            }
            // find col neighbor of query tile
            if (ListOrientation == Orientation.Vertical &&
                LayoutType == MpClipTrayLayoutType.Grid) {
                // vertical grid qidx's are top-to-bottom, left-to-right 
                // adj offset by fixed row count
                col_offset *= CurGridFixedCount;
            }
            int target_qidx = ctvm.QueryOffsetIdx + col_offset;
            if (target_qidx < 0) {
                if (ListOrientation == Orientation.Vertical) {
                    // no tile to left
                    return null;
                }
                // target is before query tray
                if (IsPinTrayEmpty) {
                    return null;
                }
                target_qidx = InternalPinnedItems.Count + target_qidx;
                if (target_qidx < 0) {
                    return null;
                }
                return InternalPinnedItems[target_qidx];
            }
            if (target_qidx >= Mp.Services.Query.TotalAvailableItemsInQuery) {
                if (ListOrientation == Orientation.Vertical) {
                    // no tile to right
                    return null;
                }
                // target is after all query items
                return null;
            }
            var neighbor_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == target_qidx);
            if (neighbor_ctvm == null) {
                // target is outside current query page
                while (neighbor_ctvm == null) {
                    // perform load more in target dir
                    QueryCommand.Execute(col_offset > 0);
                    while (IsQuerying) { await Task.Delay(100); }
                    neighbor_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == target_qidx);
                }
                //ScrollIntoView(neighbor_ctvm);
                //await Task.Delay(100);
                //while (IsAnyBusy) { await Task.Delay(100); }
                //return Items.FirstOrDefault(x => x.QueryOffsetIdx == target_idx);
            }
            return neighbor_ctvm;
        }
        #endregion

        private async Task<MpCopyItem> AddItemFromDataObjectAsync(MpPortableDataObject mpdo, bool is_copy = false) {
            while (//MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening ||
                    MpAvPlainHtmlConverter.Instance.IsBusy ||
                    !Mp.Services.StartupState.IsPlatformLoaded) {
                await Task.Delay(100);
            }

            await MpFifoAsyncQueue.WaitByConditionAsync(
                lockObj: _addDataObjectContentLock,
                waitWhenTrueFunc: () => {
                    MpConsole.WriteLine("waiting to add item to cliptray...");
                    return IsAddingClipboardItem;
                });

            if (Mp.Services.AccountTools.IsContentAddPausedByAccount) {
                MpConsole.WriteLine($"Add content blocked, acct capped. Ensuring accuracy...");
                await ProcessAccountCapsAsync(MpAccountCapCheckType.Block, mpdo);
                if (Mp.Services.AccountTools.IsContentAddPausedByAccount) {
                    MpConsole.WriteLine($"Add content blocked confirmed.");
                    return null;
                }
            }
            bool force_ext = true;
            if (mpdo is IDataObject ido) {
                // should always be avdo but trying to keep portable interfaces...
                force_ext = !ido.ContainsContentRef();
            }

            IsAddingClipboardItem = true;

            MpCopyItem newCopyItem = await Mp.Services.CopyItemBuilder.BuildAsync(
                pdo: mpdo,
                transType: MpTransactionType.Created,
                force_ext_sources: force_ext,
                force_allow_dup: is_copy);

            MpCopyItem processed_result = await AddUpdateOrAppendCopyItemAsync(newCopyItem);

            IsAddingClipboardItem = false;

            return processed_result;
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
                    MpAvTagTrayViewModel.Instance.TrashTagViewModel.UnlinkCopyItemCommand.Execute(ci.Id);
                    MpConsole.WriteLine($"Duplicate item '{ci.Title}' unlinked from trash");
                }
            } else {
                await ProcessAccountCapsAsync(MpAccountCapCheckType.Add);
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

            MpCopyItem result_ci = ci;
            if (wasAppended) {
                MpMessenger.SendGlobal(MpMessageType.AppendBufferChanged);
                if (MpPrefViewModel.Instance.IgnoreAppendedItems) {
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
        private async Task PasteClipTileAsync(MpAvClipTileViewModel ctvm, bool fromKeyboard = false) {
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

            var cv = ctvm.GetContentView();
            if (cv == null) {
                if (ctvm.CopyItem != null) {
                    mpdo = ctvm.CopyItem.ToPortableDataObject();
                }
            } else if (cv is MpAvIDragSource ds) {
                mpdo = await ds.GetDataObjectAsync(
                    formats: ctvm.GetOleFormats(false),
                    use_placeholders: false,
                    ignore_selection: false);
            }

            MpPortableProcessInfo pi = null;
            if (mpdo == null) {
                // is none selected?
                MpDebug.Break();
            } else {
                pi = Mp.Services.ProcessWatcher.LastProcessInfo;

                // NOTE paste success is very crude, false positive is likely
                bool success = await Mp.Services.ExternalPasteHandler.PasteDataObjectAsync(mpdo, pi, fromKeyboard);
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
                var trayRect = GetQueryTileRect(ctvm.QueryOffsetIdx, prevOffsetRect);
                ctvm.TrayY = trayRect.Location.Y;
                ctvm.TrayX = trayRect.Location.X;
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

        public ICommand ResetZoomFactorCommand => new MpCommand(
            () => {
                ZoomFactor = 1.0d;
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

                 if (ctvm_to_pin == null || ctvm_to_pin.IsAnyPlaceholder) {
                     MpConsole.WriteTraceLine("PinTile error, tile is either already pinned or placeholder");
                     MpDebug.Break();
                     return;
                 }

                 await ctvm_to_pin.PersistContentStateCommand.ExecuteAsync(pin_as_editable.HasValue ? pin_as_editable.Value : null);

                 int ctvm_to_pin_query_idx = -1;
                 MpAvClipTileViewModel query_ctvm_to_pin = QueryItems.FirstOrDefault(x => x.CopyItemId == ctvm_to_pin.CopyItemId);
                 if (query_ctvm_to_pin != null) {
                     // item to pin is query item in current page
                     ctvm_to_pin_query_idx = ctvm_to_pin.QueryOffsetIdx;
                     // create temp tile w/ model ref
                     var temp_ctvm = await CreateClipTileViewModelAsync(ctvm_to_pin.CopyItem);
                     // unload query tile
                     query_ctvm_to_pin.TriggerUnloadedNotification(false, false);
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
                 }

                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 ctvm_to_pin.OnPropertyChanged(nameof(ctvm_to_pin.IsPinned));
                 ctvm_to_pin.OnPropertyChanged(nameof(ctvm_to_pin.IsPlaceholder));

                 if (query_ctvm_to_pin != null &&
                    ctvm_to_pin_query_idx >= 0) {
                     await query_ctvm_to_pin.InitializeAsync(ctvm_to_pin.CopyItem, ctvm_to_pin_query_idx);
                     //QueryCommand.Execute(string.Empty);
                     //await Task.Delay(100);
                     //while (IsQuerying) {
                     //    await Task.Delay(100);
                     //}

                 }
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
            (args) => args != null);

        public MpIAsyncCommand<object> UnpinTileCommand => new MpAsyncCommand<object>(
             async (args) => {
                 MpAvClipTileViewModel pin_placeholder_ctvm = null;
                 MpAvClipTileViewModel unpinned_ctvm = null;
                 if (args is MpAvClipTileViewModel arg_ctvm) {
                     if (arg_ctvm.IsPinPlaceholder) {
                         // unpinning from query tray pin placeholder (lbi double click)
                         pin_placeholder_ctvm = arg_ctvm;
                         unpinned_ctvm = PinnedItems.FirstOrDefault(x => x.CopyItemId == pin_placeholder_ctvm.PinPlaceholderCopyItemId);
                     } else {
                         // unpinning from corner button or popout closed
                         unpinned_ctvm = arg_ctvm;
                         pin_placeholder_ctvm =
                            QueryItems.FirstOrDefault(x => x.PinPlaceholderCopyItemId == unpinned_ctvm.CopyItemId);
                     }
                 }
                 if (unpinned_ctvm == null) {
                     MpDebug.Break($"No pin tile found for placeholder ciid {pin_placeholder_ctvm.PinPlaceholderCopyItemId} at queryIdx {pin_placeholder_ctvm.QueryOffsetIdx}");
                     return;
                 }

                 await unpinned_ctvm.PersistContentStateCommand.ExecuteAsync(null);

                 int unpinned_ciid = unpinned_ctvm.CopyItemId;
                 int unpinned_ctvm_idx = PinnedItems.IndexOf(unpinned_ctvm);

                 if (unpinned_ctvm.IsWindowOpen) {
                     await unpinned_ctvm.TransactionCollectionViewModel.CloseTransactionPaneCommand.ExecuteAsync();
                     MpAvPersistentClipTilePropertiesHelper.RemoveUniqueSize_ById(unpinned_ciid, unpinned_ctvm_idx);
                     unpinned_ctvm.IsWindowOpen = false;
                 }

                 PinnedItems.Remove(unpinned_ctvm);

                 if (!IsAnyTilePinned) {
                     ObservedPinTrayScreenWidth = 0;
                 }

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
                         while (IsAnyBusy) {
                             // query returns before sub tasks complete and updated offsets are needed
                             await Task.Delay(100);
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
                            .Aggregate((a, b) => unpinned_ctvm_idx - a.ItemIdx < unpinned_ctvm_idx - b.ItemIdx ? a : b);
                     }
                 }

                 if (to_select_ctvm == null) {
                     // should probably not happen or will have no effect (empty query) but in case
                     ResetAllSelection(false);
                 } else {
                     to_select_ctvm.IsSelected = true;
                 }

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
                int pin_count = PinnedItems.Count;
                while (pin_count > 0) {
                    var to_unpin_ctvm = PinnedItems[--pin_count];
                    if (to_unpin_ctvm.IsWindowOpen ||
                        to_unpin_ctvm.IsAppendNotifier) {
                        continue;
                    }
                    await UnpinTileCommand.ExecuteAsync(to_unpin_ctvm);
                }
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

                await AddItemFromDataObjectAsync(SelectedItem.CopyItem.ToPortableDataObject());

                IsBusy = false;
            }, () => SelectedItem != null);


        public ICommand AddNewItemsCommand => new MpAsyncCommand<object>(
            async (tagDropCopyItemOnlyArg) => {

                DispatcherPriority dp = DispatcherPriority.Normal;
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLocked &&
                    !MpAvMainWindowViewModel.Instance.IsMainWindowActive) {
                    // this case can cause system lag so lower priority
                    // TODO find better priority this is too low i think
                    //dp = DispatcherPriority.ApplicationIdle;
                    dp = DispatcherPriority.Normal;
                }
                await Dispatcher.UIThread.InvokeAsync(async () => {
                    IsPinTrayBusy = true;

                    int selectedId = -1;
                    if (MpAvMainWindowViewModel.Instance.IsMainWindowLocked && SelectedItem != null) {
                        selectedId = SelectedItem.CopyItemId;
                    }

                    //for (int i = 0; i < PendingNewModels.Count; i++) {
                    //    var ci = PendingNewModels[i];
                    //    MpAvClipTileViewModel nctvm = await CreateOrRetrieveClipTileViewModelAsync(ci);
                    //    ToggleTileIsPinnedCommand.Execute(nctvm);
                    //}

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
                    MpAvClipTileViewModel nctvm = await CreateOrRetrieveClipTileViewModelAsync(most_recent_pending_ci);
                    await ToggleTileIsPinnedCommand.ExecuteAsync(nctvm);

                    PendingNewModels.Clear();
                    //while (IsAnyBusy) {
                    //    await Task.Delay(100);
                    //}
                    if (selectedId >= 0) {
                        var selectedVm = AllItems.FirstOrDefault(x => x.CopyItemId == selectedId);
                        if (selectedVm != null) {
                            selectedVm.IsSelected = true;
                        }
                    }
                    IsPinTrayBusy = false;
                    //using tray scroll changed so tile drop behaviors update their drop rects
                }, dp);
            },
            (tagDropCopyItemOnlyArg) => {
                if (tagDropCopyItemOnlyArg is MpCopyItem tag_drop_ci) {
                    MpDebug.Assert(PendingNewModels.All(x => x.Id != tag_drop_ci.Id), "AddNewItems cmd error. should only happen once from drop in tag view");
                    PendingNewModels.Add(tag_drop_ci);
                }
                if (PendingNewModels.Count == 0) {
                    return false;
                }
                if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    return true;
                }
                return false;
            });
        private int GetLoadCountFromOffset(int offset, int dir, int desiredCount) {
            if (dir > 0) {
                int result_offset = offset + desiredCount;
                if (result_offset >= Mp.Services.Query.TotalAvailableItemsInQuery) {
                    result_offset = Mp.Services.Query.TotalAvailableItemsInQuery - 1;
                }
                return result_offset - offset;
            }
            if (dir < 0) {
                int result_offset = offset - desiredCount;
                if (result_offset < 0) {
                    result_offset = 0;
                }
                return offset - result_offset;
            }
            return 0;
        }

        private bool CanCheckLoadMore() {
            if (IsThumbDragging ||
                //IsAnyBusy ||
                IsAnyResizing ||
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
                int new_head_idx = FindJumpTileIdx(ScrollOffsetX, ScrollOffsetY, out _);
                if (new_head_idx < 0) {
                    return false;
                }
                if (QueryCommand.CanExecute(ScrollOffset)) {
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

            List<List<int>> ranges = null;

            if (scroll_dir <= 0) {
                // decreasing query offset (or no delta)
                int head_remaining = vis_lbil.First().QueryOffsetIdx - HeadQueryIdx;
                int head_to_load = RemainingItemsCountThreshold - head_remaining;
                if (head_to_load > 0) {
                    int head_load_count = GetLoadCountFromOffset(HeadQueryIdx, -1, LoadMorePageSize);
                    int end_head_load_idx = HeadQueryIdx - 1;
                    int start_head_load_idx = end_head_load_idx - head_load_count + 1;
                    if (start_head_load_idx >= 0 && head_load_count >= 0) {
                        var pre_range = Enumerable.Range(start_head_load_idx, head_load_count).ToList();
                        if (pre_range.Any()) {
                            ranges = new() { pre_range };
                        }
                    }
                }
            }
            if (scroll_dir >= 0) {
                // increasing query offset (or no delta)
                int tail_remaining = TailQueryIdx - vis_lbil.Last().QueryOffsetIdx;
                int tail_to_load = RemainingItemsCountThreshold - tail_remaining;
                if (tail_to_load > 0) {
                    int start_tail_load_idx = TailQueryIdx + 1;
                    int tail_load_count = GetLoadCountFromOffset(start_tail_load_idx, 1, LoadMorePageSize);
                    if (start_tail_load_idx >= 0 && tail_load_count >= 0) {
                        var post_range = Enumerable.Range(start_tail_load_idx, tail_load_count).ToList();
                        if (post_range.Any()) {
                            if (ranges == null) {
                                ranges = new() { post_range };
                            } else {
                                ranges.Add(post_range);
                            }
                        }
                    }
                }
            }
            if (ranges == null) {
                return false;
            }

            QueryCommand.Execute(ranges);
            return true;
        }

        private async Task PerformQueryAsync(object offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) {
            //MpConsole.WriteLine($"Query called. Arg: '{offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg}'");
            Dispatcher.UIThread.VerifyAccess();
            bool is_empty_query =
                !MpAvTagTrayViewModel.Instance.IsAnyTagActive &&
                Mp.Services.Query.Infos.All(x => string.IsNullOrEmpty(x.MatchValue));

            if (is_empty_query) {
                // intermittently occurs on startup and query throws exception
                return;
            }

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

            #region TotalCount Query & Offset Calc

            IsBusy = !isLoadMore && !isLoadRange;
            IsQuerying = true;

            if (IsSubQuerying) {
                // sub-query of visual, data-specific or incremental offset 

                if (isOffsetJump) {
                    // sub-query to data-specific (query Idx) offset

                    loadOffsetIdx = (int)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg;
                    var loadTileRect = GetQueryTileRect(loadOffsetIdx, null);
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

            var cil = await Mp.Services.Query.FetchPageAsync(loadOffsetIdx, loadCount);
            if (isLoadMore && !isLoadMoreTail) {
                // when loading to head reverse order
                cil.Reverse();
                fetchQueryIdxList.Reverse();
            }
            //since tiles watch for their model changes, remove any items

            var recycle_idxs = GetLoadItemIdxs(isLoadMore ? isLoadMoreTail : null, cil.Count);
            int dir = isLoadMoreTail ? 1 : -1;

            for (int i = 0; i < cil.Count; i++) {
                MpAvClipTileViewModel cur_ctvm = Items[recycle_idxs[i]];

                if (cur_ctvm.IsSelected) {
                    StoreSelectionState(cur_ctvm);
                    cur_ctvm.ClearSelection(false);
                }
                bool needsRestore = false;
                if (IsSubQuerying && MpAvPersistentClipTilePropertiesHelper.GetPersistentSelectedItemId() == cil[i].Id) {
                    needsRestore = true;
                }
                await cur_ctvm.InitializeAsync(cil[i], fetchQueryIdxList[i], needsRestore);
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
                // treat sparse loads as 1 query but only finalize after last
                return;
            }

            //if (Items.All(x => x.IsPlaceholder)) {
            //    // reset scroll if empty 
            //    ScrollOffsetX = 0;
            //    LastScrollOffsetX = 0;
            //    ScrollOffsetY = 0;
            //    LastScrollOffsetY = 0;
            //}

            IsBusy = false;
            IsQuerying = false;

            OnPropertyChanged(nameof(IsAnyBusy));
            OnPropertyChanged(nameof(IsQueryEmpty));

            sw.Stop();
            MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);

            if (isRequery) {
                ForceScrollOffset(MpPoint.Zero);
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
                    ForceScrollOffset(newScrollOffset);
                    MpMessenger.SendGlobal(MpMessageType.JumpToIdxCompleted);
                } else {
                    //recheck loadMore once done for rejected scroll change events
                    //while (IsAnyBusy) {
                    //    await Task.Delay(100);
                    //}
                    if (isLoadMore) {
                        if (CheckLoadMore()) {
                            return;
                        }
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
        }
        public MpIAsyncCommand<object> QueryCommand => new MpAsyncCommand<object>(
            async (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) => {

                if (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is not List<List<int>> ranges) {
                    await PerformQueryAsync(offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg);
                    return;
                }
                SparseLoadMoreRemaining = ranges.Count;
                for (int i = 0; i < SparseLoadMoreRemaining; i++) {
                    SparseLoadMoreRemaining--;
                    await PerformQueryAsync(ranges[i]);
                }
            },
            (offsetIdx_Or_ScrollOffset_Arg) => {
                return
                    //!IsAnyBusy && 
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

        public ICommand CopySelectedClipFromShortcutCommand => new MpCommand(
            () => {
                SelectedItem.CopyToClipboardCommand.Execute(null);
            },
            () => {
                bool canCopy =
                    SelectedItem != null &&
                    SelectedItem.IsListBoxItemFocused &&
                    SelectedItem.IsHostWindowActive;
                MpConsole.WriteLine("CopySelectedClipFromShortcutCommand CanExecute: " + canCopy);
                if (!canCopy) {
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));
                    MpConsole.WriteLine("IsHostWindowActive: " + (SelectedItem == null ? "NO" : SelectedItem.IsHostWindowActive.ToString()));
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
                return SelectedItem != null;
            });

        public ICommand PasteSelectedClipTileFromShortcutCommand => new MpCommand<object>(
            (args) => {
                bool fromEditorButton = false;
                if (args is bool) {
                    fromEditorButton = (bool)args;
                }
                PasteClipTileAsync(SelectedItem, true).FireAndForgetSafeAsync();
            },
            (args) => {
                bool can_paste =
                    SelectedItem != null &&
                    SelectedItem.IsHostWindowActive &&
                    //!MpAvMainWindowViewModel.Instance.IsAnyMainWindowTextBoxFocused &&
                    //!MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen &&
                    //!IsAnyEditingClipTile &&
                    //!IsAnyEditingClipTitle &&
                    !MpPrefViewModel.Instance.IsTrialExpired;

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
            }, () => {
                return SelectedItem != null && !SelectedItem.IsAnyPlaceholder;
            });

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
                PasteClipTileAsync(ctvm, true).FireAndForgetSafeAsync(this);
            },
            (args) => {
                return args is int || args is string;
            });


        public ICommand RestoreSelectedClipCommand => new MpAsyncCommand(
            async () => {
                // add link to trash tag which caches ciid
                await MpAvTagTrayViewModel.Instance.TrashTagViewModel
                .UnlinkCopyItemCommand.ExecuteAsync(SelectedItem.CopyItemId);

                MpAvTagTrayViewModel.Instance.UpdateAllClipCountsAsync().FireAndForgetSafeAsync(this);
                // trigger in place requery to restore trashed item
                QueryCommand.Execute(string.Empty);

            },
            () => {
                bool can_restore =
                    SelectedItem != null &&
                    SelectedItem.IsTrashed &&
                    !Mp.Services.AccountTools.IsContentAddPausedByAccount;

                if (!can_restore) {
                    MpConsole.WriteLine("RestoreSelectedClipCommand CanExecute: " + can_restore);
                    MpConsole.WriteLine("SelectedItem: " + (SelectedItem == null ? "IS NULL" : "NOT NULL"));
                    MpConsole.WriteLine($"SelectedItem: Trashed: {SelectedItem.IsTrashed}");
                    MpConsole.WriteLine($"SIsContentAddPausedByAccount: {Mp.Services.AccountTools.IsContentAddPausedByAccount}");
                }
                return can_restore;
            });

        public ICommand TrashSelectedClipCommand => new MpAsyncCommand(
            async () => {
                await TrashItemByCopyItemIdAsync(SelectedItem.CopyItemId);
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
        private async Task DeleteItemByCopyItemIdAsync(int ciid) {
            if (ciid == 0) {
                return;
            }
            while (IsBusy) { await Task.Delay(100); }

            IsBusy = true;

            var to_delete_ctvm = AllActiveItems.FirstOrDefault(x => x.CopyItemId == ciid);
            if (to_delete_ctvm == null) {
                var to_delete_ci = await MpDataModelProvider.GetItemAsync<MpCopyItem>(ciid);
                if (to_delete_ci != null) {
                    await to_delete_ci.DeleteFromDatabaseAsync();
                }
            } else {
                await to_delete_ctvm.CopyItem.DeleteFromDatabaseAsync();
            }

            await ProcessAccountCapsAsync(MpAccountCapCheckType.Remove, ciid);

            //db delete event is handled in clip tile
            IsBusy = false;
        }
        public ICommand DeleteSelectedClipCommand => new MpAsyncCommand(
            async () => {
                await DeleteItemByCopyItemIdAsync(SelectedItem.CopyItemId);
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

        public ICommand TrashOrDeleteSelectedClipFromShortcutCommand => new MpCommand(
             () => {
                 // NOTE 
                 if (DeleteSelectedClipCommand.CanExecute(null)) {
                     DeleteSelectedClipCommand.Execute(null);
                 } else {
                     TrashSelectedClipCommand.Execute(null);
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

                var preset = await MpDataModelProvider.GetItemAsync<MpPluginPreset>(presetId);
                var analyticItemVm =
                    MpAvAnalyticItemCollectionViewModel.Instance
                    .Items.FirstOrDefault(x => x.PluginGuid == preset.PluginGuid);
                int selected_ciid = SelectedItem.CopyItemId;
                var presetVm = analyticItemVm.Items.FirstOrDefault(x => x.Preset.Id == preset.Id);

                analyticItemVm.SelectPresetCommand.Execute(presetVm);
                if (analyticItemVm.ExecuteAnalysisCommand.CanExecute(null)) {
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
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                    title: "MODE CHANGED",
                    body: $"RIGHT CLICK PASTE MODE: {(IsRightClickPasteMode ? "ON" : "OFF")}",
                    msgType: MpNotificationType.AppModeChange).FireAndForgetSafeAsync(this);
                MpMessenger.SendGlobal(IsRightClickPasteMode ? MpMessageType.RightClickPasteEnabled : MpMessageType.RightClickPasteDisabled);
            }, () => !IsAppPaused);

        public ICommand ToggleAutoCopyModeCommand => new MpCommand(
            () => {
                IsAutoCopyMode = !IsAutoCopyMode;

                Mp.Services.NotificationBuilder.ShowMessageAsync(
                    title: "MODE CHANGED",
                    body: $"AUTO-COPY SELECTION MODE: {(IsAutoCopyMode ? "ON" : "OFF")}",
                    msgType: MpNotificationType.AppModeChange).FireAndForgetSafeAsync(this);
                MpMessenger.SendGlobal(IsAutoCopyMode ? MpMessageType.AutoCopyEnabled : MpMessageType.AutoCopyDisabled);
            }, () => !IsAppPaused);



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
                var mgs = args as MpAvMovableGridSplitter;
                if (mgs == null) {
                    mgs = MpAvWindowManager.MainWindow.GetVisualDescendant<MpAvMovableGridSplitter>();
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
                ctvm.TransactionCollectionViewModel.SelectChildCommand.Execute(anguid);
            }, (args) => {
                return args != null;
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

        private void UpdateAppendModeStateFlags(MpAppendModeFlags flags, string source, bool silent = false) {
            IsAppendLineMode = flags.HasFlag(MpAppendModeFlags.AppendLine);
            IsAppendInsertMode = flags.HasFlag(MpAppendModeFlags.AppendInsert);
            IsAppendManualMode = flags.HasFlag(MpAppendModeFlags.Manual);
            IsAppendPaused = flags.HasFlag(MpAppendModeFlags.Paused);
            IsAppendPreMode = flags.HasFlag(MpAppendModeFlags.Pre);

            var last_flags = _appendModeFlags;
            _appendModeFlags = flags;
            OnPropertyChanged(nameof(AppendModeStateFlags));

            if (silent) {
                // is silent is so append stateis set BEFORE popout window is created 
                // because it will attach append msg but no state is set so silent avoids relaying message
                // to let loadContent handle it
                return;
            }

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
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                       title: $"{manual_change_str} Append Mode Activated",
                       body: detail_str,
                       msgType: MpNotificationType.AppendModeChanged,
                       iconSourceObj: icon_key).FireAndForgetSafeAsync();
            }

            if (cur_flags.HasFlag(MpAppendModeFlags.Paused) != last_flags.HasFlag(MpAppendModeFlags.Paused)) {
                string pause_change_str = cur_flags.HasFlag(MpAppendModeFlags.Paused) ? "Paused" : "Resumed";
                string detail_str = cur_flags.HasFlag(MpAppendModeFlags.Paused) ? "Clipboard accumulation halted" : "Clipboard accumulation resumed";
                string icon_key = cur_flags.HasFlag(MpAppendModeFlags.Paused) ? "PauseImage" : "PlayImage";
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                       title: $"Append {pause_change_str}",
                       body: detail_str,
                       msgType: MpNotificationType.AppendModeChanged,
                       iconSourceObj: icon_key).FireAndForgetSafeAsync();
            }

            if (cur_flags.HasFlag(MpAppendModeFlags.Pre) != last_flags.HasFlag(MpAppendModeFlags.Pre)) {
                string manual_change_str = cur_flags.HasFlag(MpAppendModeFlags.Pre) ? "Before" : "After";
                string detail_str = cur_flags.HasFlag(MpAppendModeFlags.Pre) ? "Clipboard changes will now be prepended" : "Clipboard changes will now be appended";
                string icon_key = cur_flags.HasFlag(MpAppendModeFlags.Pre) ? "BringToFrontImage" : "SendToBackImage";
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                       title: $"{manual_change_str} Append Mode Activated",
                       body: detail_str,
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
                if (amf.HasFlag(MpAppendModeFlags.AppendInsert)) {
                    amf ^= MpAppendModeFlags.AppendInsert;
                }

                amf |= MpAppendModeFlags.AppendLine;
            } else {
                if (amf.HasFlag(MpAppendModeFlags.AppendLine)) {
                    amf ^= MpAppendModeFlags.AppendLine;
                }
                amf |= MpAppendModeFlags.AppendInsert;
            }
            if (isManualMode) {
                amf |= MpAppendModeFlags.Manual;
            }
            bool was_append_already_enabled = IsAnyAppendMode;
            // NOTE update is silent here
            UpdateAppendModeStateFlags(amf, "command", true);

            if (AppendClipTileViewModel == null) {
                // append mode was just toggled ON (param was null)
                await AssignAppendClipTileAsync(isAppendLine ? MpAppendModeType.Line : MpAppendModeType.Insert);
            }

            UpdateAppendModeStateFlags(amf, "command", false);

            if (was_append_already_enabled) {
                return;
            }

            MpMessenger.SendGlobal(MpMessageType.AppendModeActivated);
            if (AppendClipTileViewModel != null) {
                // append popout itself is the notification
                return;
            }

            // no item assigned yet so just show enable message
            string type_str = IsAppendLineMode ? "Block" : "Inline";
            string manual_str = IsAppendManualMode ? "(Manual) " : string.Empty;
            string icon_key = IsAppendLineMode ? "AppendLineImage" : "AppendImage";
            Mp.Services.NotificationBuilder.ShowMessageAsync(
                   title: $"Append {type_str} {manual_str}Mode Activated",
                   body: "Copy text or file(s) to apply.",
                   msgType: MpNotificationType.AppendModeChanged,
                   iconSourceObj: icon_key).FireAndForgetSafeAsync();
        }
        private void DeactivateAppendMode() {
            Dispatcher.UIThread.VerifyAccess();

            UpdateAppendModeStateFlags(MpAppendModeFlags.None, "command");

            MpMessenger.SendGlobal(MpMessageType.AppendModeDeactivated);
            if (AppendClipTileViewModel == null) {
                // only show deactivate ntf if no windows there
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                           title: $"Append Deactivated",
                           body: $"Normal clipboard behavior has been restored",
                           msgType: MpNotificationType.AppendModeChanged,
                           iconSourceObj: "ClipboardImage").FireAndForgetSafeAsync();
            } else {
                var deactivate_append_ctvm = AppendClipTileViewModel;
                deactivate_append_ctvm.IsAppendNotifier = false;
            }
        }
        private async Task<bool> UpdateAppendModeAsync(MpCopyItem aci, bool isNew = true) {
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
                AppendClipTileViewModel.IsWindowOpen = true;
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

        public ICommand ReloadSelectedItemCommand => new MpCommand(
            () => {
                if (SelectedItem != null && SelectedItem.GetContentView() is MpAvContentWebView wv) {
                    wv.ReloadAsync().FireAndForgetSafeAsync();
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
