using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Wpf;
using MonoMac.Foundation;
using MonoMac.OpenAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipTrayViewModel : 
        MpAvSelectorViewModelBase<object, MpAvClipTileViewModel>,
        MpIBootstrappedItem, 
        MpIPagingScrollViewerViewModel,
        MpIActionComponent, 
        MpIBoundSizeViewModel,
        MpIContextMenuViewModel {
        #region Private Variables

        //private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;

        //private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private List<MpCopyItem> _newModels = new List<MpCopyItem>();

        //private int _appendCopyItemId;
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

        private static MpAvClipTrayViewModel _instance;
        public static MpAvClipTrayViewModel Instance => _instance ?? (_instance = new MpAvClipTrayViewModel());


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

        #region Properties

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
                    return SelectedItem.SourceCollectionViewModel.ContextMenuViewModel;
                }
                if (MpAvTagTrayViewModel.Instance.IsAnyBusy) {
                    Debugger.Break();
                }
                var tagItems = MpAvTagTrayViewModel.Instance.AllTagViewModel.ContentMenuItemViewModel.SubItems;
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = @"Cut",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("ScissorsImage") as string,
                            Command = MpAvShortcutCollectionViewModel.Instance.SimulateKeyStrokeCommand,
                            CommandParameter = MpPlatformWrapper.Services.PlatformShorcuts.CutKeys,
                            ShortcutArgs = new object[] { MpShortcutType.PasteHere },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Copy",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("CopyImage") as string,
                            Command = CutSelectionFromContextMenuCommand,
                            ShortcutArgs = new object[] { MpShortcutType.CopySelection },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Paste Here",
                            AltNavIdx = 6,
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PasteImage") as string,
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
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DeleteImage") as string,
                            Command = DeleteSelectedClipsCommand,
                            ShortcutArgs = new object[] { MpShortcutType.DeleteSelectedItems },
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Rename",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("RenameImage") as string,
                            Command = EditSelectedTitleCommand,
                            ShortcutArgs = new object[] { MpShortcutType.EditTitle },
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Edit",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("EditContentImage") as string,
                            Command = EditSelectedContentCommand,
                            ShortcutArgs = new object[] { MpShortcutType.EditContent },
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Transform",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("ToolsImage") as string,
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = @"Find and Replace",
                                    AltNavIdx = 0,
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SearchImage") as string,
                                    Command = EnableFindAndReplaceForSelectedItem,
                                    ShortcutArgs = new object[] { MpShortcutType.FindAndReplaceSelectedItem },
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Duplicate",
                                    AltNavIdx = 0,
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DuplicateImage") as string,
                                    Command = DuplicateSelectedClipsCommand,
                                    ShortcutArgs = new object[] { MpShortcutType.Duplicate },
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To Web Search",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("WebImage") as string,
                                    SubItems = new List<MpMenuItemViewModel>() {
                                        new MpMenuItemViewModel() {
                                            Header = "Google",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("GoogleImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.google.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "Bing",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("BingImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.bing.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "DuckDuckGo",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DuckImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://duckduckgo.com/?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "Yandex",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("YandexImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://yandex.com/search/?text="
                                        },
                                        new MpMenuItemViewModel() { IsSeparator = true},
                                        new MpMenuItemViewModel() {
                                            Header = "Manage...",
                                            AltNavIdx = 0,
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("CogImage") as string
                                        },
                                    }
                                }
                            }
                        },
                        SelectedItem.SourceCollectionViewModel.ContextMenuViewModel,
                        MpAvAnalyticItemCollectionViewModel.Instance.ContextMenuItemViewModel,
                        new MpMenuItemViewModel() {IsSeparator = true},
                        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(SelectedItem),
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                            Header = @"Collections",
                            AltNavIdx = 0,
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PinToCollectionImage") as string,
                            SubItems = tagItems
                        }
                    },
                };
            }
        }

        bool MpIContextMenuViewModel.IsContextMenuOpen {
            get {
                if(SelectedItem == null) {
                    return false;
                }
                if(SelectedItem.IsContextMenuOpen || 
                    SelectedItem.SourceCollectionViewModel.IsContextMenuOpen) {
                    return true;
                }
                return false;
            }
            set {
                if(SelectedItem == null) {
                    return;
                }
                if(value) {
                    if(SelectedItem.IsHoveringOverSourceIcon) {
                        SelectedItem.SourceCollectionViewModel.IsContextMenuOpen = true;
                        SelectedItem.IsContextMenuOpen = false;
                    } else {
                        SelectedItem.SourceCollectionViewModel.IsContextMenuOpen = false;
                        SelectedItem.IsContextMenuOpen = true;
                    }
                } else {
                    SelectedItem.SourceCollectionViewModel.IsContextMenuOpen = false;
                    SelectedItem.IsContextMenuOpen = false;
                }
            }
        }

        #endregion

        #region MpIBoundSizeViewModel Implementation

        public double BoundWidth { get; set; }
        public double BoundHeight { get; set; }

        #endregion

        #region View Models

        //private MpAvClipTileViewModel _appendClipTileViewModel;
        public MpAvClipTileViewModel ModalClipTileViewModel { get; private set; }

        public MpAvClipTileViewModel AppendClipTileViewModel => AllItems.FirstOrDefault(x => x.IsAppendNotifier);
            

        public MpAvQueryInfoViewModel CurrentQuery => MpAvQueryInfoViewModel.Current;
        public IEnumerable<MpAvClipTileViewModel> SortOrderedItems => Items.Where(x=>x.QueryOffsetIdx >= 0).OrderBy(x => x.QueryOffsetIdx);

        public ObservableCollection<MpAvClipTileViewModel> PinnedItems { get; set; } = new ObservableCollection<MpAvClipTileViewModel>();

        public IEnumerable<MpAvClipTileViewModel> AllItems {
            get {
                foreach (var ctvm in Items) {
                    yield return ctvm;
                }
                foreach (var pctvm in PinnedItems) {
                    yield return pctvm;
                }
                if(ModalClipTileViewModel != null) {
                    yield return ModalClipTileViewModel;
                }
                
            }
        }
        public MpAvClipTileViewModel HeadItem => SortOrderedItems.ElementAtOrDefault(0);

        public MpAvClipTileViewModel TailItem => SortOrderedItems.ElementAtOrDefault(Items.Count - 1);

        public int PersistantSelectedItemId {
            get {
                if(SelectedItem == null) {
                    if(MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count == 0) {
                        return -1;
                    }
                    return MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels[0].Id;
                }
                return SelectedItem.CopyItemId;
            }
        }

        public override MpAvClipTileViewModel SelectedItem {
            get {
                if(MpAvAppendNotificationWindow.Instance != null &&
                    MpAvAppendNotificationWindow.Instance.IsVisible) {
                    // only visible if mw is not open
                    return ModalClipTileViewModel;
                }

                return AllItems.FirstOrDefault(x => x.IsSelected);
            }
            set {
                if(value == null) {
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
                if(SelectedItem == null || SelectedItem.IsPinned) {
                    return null;
                }
                return SelectedItem;
            }
            set {
                if(value == null || value.IsPlaceholder) {
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
        //public MpAvClipTileViewModel DragItem => AllItems.FirstOrDefault(x => x.IsTileDragging);

        //public int DragItemId => DragItem == default ? -1 : DragItem.CopyItemId;
        public IEnumerable<MpAvClipTileViewModel> VisibleItems => Items.Where(x => x.IsAnyCornerVisible && !x.IsPlaceholder);

        public Orientation DefaultScrollOrientation {
            get {
                if(ListOrientation == Orientation.Horizontal) {
                    if(IsGridLayout) {
                        return Orientation.Vertical;
                    }
                    return Orientation.Horizontal;
                }
                if(IsGridLayout) {
                    return Orientation.Horizontal;
                }
                return Orientation.Vertical;
            }
        }

        #endregion

        #region MpIPagingScrollViewer Implementation

        public double ScrollFrictionX { 
            get {
                if(LayoutType == MpClipTrayLayoutType.Stack) {
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

        //public int ClipTrayRowCount {
        //    get {
        //        if(ListOrientation == Orientation.Horizontal) {
        //            if(LayoutType == MpAvClipTrayLayoutType.Grid) {
        //                return CurGridFixedCount;
        //            }
        //            return TotalTilesInQuery;
        //        } else {
        //            if(LayoutType == MpAvClipTrayLayoutType.Grid) {
        //                return CurGridFixedCount;
        //            }
        //        }
        //    }
        //}

        public Orientation ListOrientation => MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ? Orientation.Horizontal : Orientation.Vertical;

        public bool IsHorizontalScrollBarVisible => true;// ClipTrayTotalTileWidth > ClipTrayScreenWidth;
        public bool IsVerticalScrollBarVisible => true;// ClipTrayTotalTileHeight > ClipTrayScreenHeight;

        public double LastScrollOffsetX { get; set; } = 0;
        public double LastScrollOffsetY { get; set; } = 0;

        public MpPoint LastScrollOffset => new MpPoint(LastScrollOffsetX, LastScrollOffsetY);

        private double _scrollOffsetX;
        public double ScrollOffsetX {
            get => _scrollOffsetX;
            set {
                if(ScrollOffsetX != value) {
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
                if(ScrollOffsetX != newVal.X) {
                    ScrollOffsetX = newVal.X;
                }
                if(ScrollOffsetY != newVal.Y) {
                    ScrollOffsetY = newVal.Y;
                }
            }
        }

        public double MaxScrollOffsetX {
            get {
                double maxScrollOffsetX = Math.Max(0,ClipTrayTotalTileWidth - ClipTrayScreenWidth);
                return maxScrollOffsetX;
            }
        }
        public double MaxScrollOffsetY {
            get {
                double maxScrollOffsetY = Math.Max(0,ClipTrayTotalTileHeight - ClipTrayScreenHeight);
                return maxScrollOffsetY;
            }
        }

        public MpPoint MaxScrollOffset => new MpPoint(MaxScrollOffsetX, MaxScrollOffsetY);

        public MpRect ScreenRect => new MpRect(0, 0, ClipTrayScreenWidth, ClipTrayScreenHeight);

        public MpRect TotalTileRect => new MpRect(0, 0, ClipTrayTotalTileWidth, ClipTrayTotalTileHeight);
        
        public double ClipTrayTotalTileWidth { get; private set; }
        public double ClipTrayTotalTileHeight { get; private set; }
        
        public double ClipTrayTotalWidth => Math.Max(0,Math.Max(ClipTrayScreenWidth, ClipTrayTotalTileWidth));
        public double ClipTrayTotalHeight => Math.Max(0,Math.Max(ClipTrayScreenHeight, ClipTrayTotalTileHeight));

        public double ClipTrayScreenWidth { get; set; }

        public double ClipTrayScreenHeight { get; set; }

        public MpRect PinTrayScreenRect => new MpRect(MpPoint.Zero, new MpSize(ObservedPinTrayScreenWidth, ObservedPinTrayScreenHeight));
        public MpRect ClipTrayScreenRect => new MpRect(MpPoint.Zero, new MpSize(ClipTrayScreenWidth, ClipTrayScreenHeight));
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

                if(HasScrollVelocity) {
                    // this implies mouse is/was not over a sub-selectable tile and is scrolling so ignore item scroll if already moving
                    return true;
                }
                // TODO? giving item scroll priority maybe better by checking if content exceeds visible boundaries here
                bool isItemScrollPriority = Items.Any(x => x.IsSubSelectionEnabled && x.IsHovering);
                if(isItemScrollPriority) {
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
            if(TotalTilesInQuery > 0) {
                var result = FindTileRectOrQueryIdxOrTotalTileSize_internal(
                    queryOffsetIdx: -1,
                    scrollOffsetX: -1,
                    scrollOffsetY: -1);
                if(result is MpSize) {
                    totalTileSize = (MpSize)result;
                }
            }
            
            ClipTrayTotalTileWidth = totalTileSize.Width;
            ClipTrayTotalTileHeight = totalTileSize.Height;
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

            int totalTileCount = CurrentQuery.TotalAvailableItemsInQuery;
            queryOffsetIdx = isFindTotalSize ? TotalTilesInQuery - 1 : queryOffsetIdx;
            if(queryOffsetIdx >= totalTileCount) {
                return null;
            }

            int startIdx = 0;// prevOffsetRect == null ? 0 : queryOffsetIdx;

            var total_size = MpSize.Empty;
            int gridFixedCount = -1;

            MpRect last_rect = null;// prevOffsetRect;

            for (int i = startIdx; i <= queryOffsetIdx; i++) {
                int tileId = CurrentQuery.GetItemId(i);
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
                    } else if(last_rect != null) {
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
                    } else if(last_rect != null){
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

                if(isFindTileIdx) {
                    if(tile_rect.X >= scrollOffsetX && tile_rect.Y >= scrollOffsetY) {
                        // NOTE not sure why but in this mode, find by scrollOffset returns target tile + 1 
                        // so returning previous rect when found
                        return new object[] { i, tile_rect };
                    }
                }
                last_rect = tile_rect;
            }

            if(isFindTileIdx) {
                // if not found presume offset is beyond last tile
                return TotalTilesInQuery - 1;
            }
            if(isFindTileRect) {
                return last_rect;
            }
            if(isFindTotalSize) {
                if(IsGridLayout) {
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
            if(result is MpRect tileRect) {
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
                    return ClipTrayScreenWidth;
                } else {
                    if(LayoutType == MpClipTrayLayoutType.Stack) {
                        return ClipTrayScreenWidth;
                    }
                    return double.PositiveInfinity;
                }
            }
        }

        public double DesiredMaxTileBottom {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return ClipTrayScreenHeight;
                    }
                    return double.PositiveInfinity;
                } else {
                    if (LayoutType == MpClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return ClipTrayScreenHeight;
                }
            }
        }
        
        public double DefaultItemWidth {
            get {
                double defaultWidth;
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    defaultWidth = ListOrientation == Orientation.Horizontal ?
                                    (ClipTrayScreenHeight * ZoomFactor) :
                                    (ClipTrayScreenWidth * ZoomFactor);
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
                                    (ClipTrayScreenHeight * ZoomFactor) :
                                    (ClipTrayScreenWidth * ZoomFactor);
                } else {
                    defaultHeight = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds.Width *
                                ZoomFactor * MIN_SIZE_ZOOM_FACTOR_COEFF;
                }
                double scrollBarSize = ScrollBarFixedAxisSize;// IsVerticalScrollBarVisible ? 30 : 0;
                return Math.Clamp(defaultHeight - scrollBarSize,0, MaxTileHeight);
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

        public bool CanThumbDragY => ClipTrayScreenHeight < ClipTrayTotalHeight;
        public bool CanThumbDragX => ClipTrayScreenWidth< ClipTrayTotalWidth;

        public bool CanThumbDrag => CanThumbDragX || CanThumbDragY;
        #endregion

        #endregion

        #region Layout
        public double ObservedContainerScreenWidth { get; set; }
        public double ObservedContainerScreenHeight { get; set; }
        public double ObservedPinTrayScreenWidth { get; set; }
        public double ObservedPinTrayScreenHeight { get; set; }
        public double DefaultPinTrayWidth => DefaultItemWidth * 1.4;

        //public double PinTrayPopOutObservedWidth { get; set; }
        //public double PinTrayPopOutObservedHeight { get; set; }
        public double PinTrayTotalWidth { get; set; } = 0;

        public double MinPinTrayScreenWidth {
            get {
                return IsPinTrayVisible ? MinClipOrPinTrayScreenWidth : 0;
            }
        }
        public double MinPinTrayScreenHeight {
            get {
                return IsPinTrayVisible ? MinClipOrPinTrayScreenHeight : 0;
            }
        }

        public double MaxPinTrayScreenWidth {
            get {
                if (ListOrientation == Orientation.Horizontal) {

                    return ObservedContainerScreenWidth - MinClipTrayScreenWidth;
                }
                return double.PositiveInfinity;
            }
        }
        public double MaxPinTrayScreenHeight {
            get {
                if (ListOrientation == Orientation.Horizontal) {

                    return double.PositiveInfinity;
                }
                return ObservedContainerScreenHeight - MinClipTrayScreenHeight;
            }
        }

        public double MinClipTrayScreenWidth => MinClipOrPinTrayScreenWidth;
        public double MinClipTrayScreenHeight => MinClipOrPinTrayScreenHeight;
        public double MinClipOrPinTrayScreenWidth => 0;
        public double MinClipOrPinTrayScreenHeight => 0;
        public double MaxTileWidth => double.PositiveInfinity;// Math.Max(0, ClipTrayScreenWidth - MAX_TILE_SIZE_CONTAINER_PAD);
        public double MaxTileHeight => double.PositiveInfinity;// Math.Max(0, ClipTrayScreenHeight - MAX_TILE_SIZE_CONTAINER_PAD);

        public int CurGridFixedCount { get; set; }

        public MpClipTrayLayoutType LayoutType { get; set; } = MpClipTrayLayoutType.Stack;

        #endregion

        #region State

        #region Append
        public List<MpCopyItem> PendingNewModels => _newModels;
        public MpClipboardModeFlags ClipboardModeFlags {
            get {
                MpClipboardModeFlags cmf = MpClipboardModeFlags.None;
                if(!IsAppPaused) {
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
                if(IsAutoCopyMode) {
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
                UpdateAppendModeStateFlags(value,"property");
            }
        }


        public bool IsAppendMode { get; set; }

        public bool IsAppendLineMode { get; set; }

        public bool IsAppendManualMode { get; set; }

        public bool IsAnyAppendMode => IsAppendMode || IsAppendLineMode;
        #endregion
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

        public int TotalTilesInQuery => MpAvQueryInfoViewModel.Current.TotalAvailableItemsInQuery;

        public int DefaultLoadCount {
            get {
                if (LayoutType == MpClipTrayLayoutType.Stack) {
                    return 20;
                } else {
                    return 40;
                    // for grid try to make default load count so it lands at the end of the fixed side
                    //double length = ListOrientation != Orientation.Horizontal ? ClipTrayScreenHeight : ClipTrayScreenWidth;
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

        #region Mouse Modes

        public bool IsAnyMouseModeEnabled => IsAutoCopyMode || IsRightClickPasteMode;


        public bool IsAutoCopyMode { get; set; }

        public bool IsRightClickPasteMode { get; set; }

        public string MouseModeImageSourcePath {
            get {
                if (IsRightClickPasteMode && IsAutoCopyMode) {
                    return MpPlatformWrapper.Services.PlatformResource.GetResource("BothClickImage") as string;
                }
                if (IsRightClickPasteMode) {
                    return MpPlatformWrapper.Services.PlatformResource.GetResource("RightClickImage") as string;
                }
                if (IsAutoCopyMode) {
                    return MpPlatformWrapper.Services.PlatformResource.GetResource("LeftClickImage") as string;
                }
                return MpPlatformWrapper.Services.PlatformResource.GetResource("NoneClickImage") as string;
            }
        }

        #endregion

        public bool IsAppPaused { get; set; } = false;

        public bool IsRestoringSelection { get; private set; } = false;

        public bool IsArrowSelecting { get; set; } = false;


        public bool IsTrayEmpty => Items.Count == 0 &&
                                   !IsRequery && !MpAvMainWindowViewModel.Instance.IsMainWindowLoading;// || Items.All(x => x.IsPlaceholder);

        public bool IsSelectionReset { get; set; } = false;

        public bool IgnoreSelectionReset { get; set; } = false;



        private bool _isFilteringByApp = false;
        public bool IsFilteringByApp {
            get {
                return _isFilteringByApp;
            }
            set {
                if (_isFilteringByApp != value) {
                    _isFilteringByApp = value;
                    OnPropertyChanged(nameof(IsFilteringByApp));
                }
            }
        }
        public bool IsEmpty => TotalTilesInQuery == 0;

        public bool IsPinTrayEmpty => PinnedItems.Count == 0;

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

        public bool IsRequery { get; set; } = false;

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
        public async Task InitAsync() {
            LogPropertyChangedEvents = false;

            IsBusy = true;

            //while (MpAvSourceCollectionViewModel.Instance.IsAnyBusy) {
            //    await Task.Delay(100);
            //}

            PropertyChanged += MpAvClipTrayViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;

            MpDb.SyncAdd += MpDbObject_SyncAdd;
            MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.SyncDelete += MpDbObject_SyncDelete;

            MpPlatformWrapper.Services.ClipboardMonitor.OnClipboardChanged += ClipboardChanged;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            ModalClipTileViewModel = await CreateClipTileViewModel(null);
            //for (int i = 1; i <= 1000; i++) {
            //    var ci = new MpCopyItem() {
            //        ItemType = MpCopyItemType.Text,
            //        ItemData = "This is test " + i,
            //        Title = "Test" + i,
            //        SourceId = 1,
            //        CopyDateTime = DateTime.Now
            //    };
            //    await ci.WriteToDatabaseAsync();
            //}

            //OnPropertyChanged(nameof(Items));

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

        public void RefreshQueryTrayLayout(MpAvClipTileViewModel fromItem = null) {
            FindTotalTileSize();

            fromItem = fromItem == null ? HeadItem : fromItem;
            UpdateTileRectCommand.Execute(fromItem);

            AllItems.ForEach(x => x.OnPropertyChanged(nameof(x.MinSize)));

            OnPropertyChanged(nameof(ClipTrayTotalHeight));
            OnPropertyChanged(nameof(ClipTrayTotalWidth));

            OnPropertyChanged(nameof(MaxScrollOffsetX));
            OnPropertyChanged(nameof(MaxScrollOffsetY));

            OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
            OnPropertyChanged(nameof(ClipTrayTotalTileHeight));

            OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
        }
        public void ForceScrollOffsetX(double newOffsetX, bool isSilent = false) {
            if (newOffsetX < 0 || newOffsetX > MaxScrollOffsetX) {
                //Debugger.Break();
            }
            //newOffsetX = Math.Min(MaxScrollOffsetX, Math.Max(0, newOffsetX));
            _scrollOffsetX = LastScrollOffsetX = newOffsetX;
            if(isSilent) {
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
            if (IsEmpty) {
                _anchor_query_idx = -1;
                return;
            }
            if (isLayoutChangeToGrid) {
                // when change is to grid change anchor to first element on fixed dimension of what the anchor was
                var anchor_loc = Items.FirstOrDefault(x => x.QueryOffsetIdx == _anchor_query_idx).TrayLocation;
                if (ListOrientation == Orientation.Horizontal) {
                    _anchor_query_idx = Items.Where(x=>Math.Abs(x.TrayY - anchor_loc.Y) < 3).Aggregate((a, b) => a.TrayX < b.TrayX ? a : b).QueryOffsetIdx;
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
            if(_anchor_query_idx < 0) {
                return;
            }
            MpPoint anchor_offset = MpPoint.Zero;
            var anchor_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == _anchor_query_idx);            
            if(anchor_ctvm == null) {
                // this occurs in a scroll jump (at least), set anchor to page head
                //if(HeadItem != null) {
                //    anchor_offset = HeadItem.TrayLocation;
                //} else {
                //    // what's going on here? is the list empty?
                //    Debugger.Break();
                //}
                Debugger.Break();
            } else {
                anchor_offset = anchor_ctvm.TrayLocation;
            }

            ForceScrollOffset(anchor_offset);
        }

        #region MpIMatchTrigger Implementation

        public void RegisterActionComponent(MpIInvokableAction mvm) {
            OnCopyItemAdd += mvm.OnActionInvoked;
            MpConsole.WriteLine($"ClipTray Registered {mvm.Label} matcher");
        }

        public void UnregisterActionComponent(MpIInvokableAction mvm) {
            OnCopyItemAdd -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"Matcher {mvm.Label} Unregistered from OnCopyItemAdded");
        }

        #endregion

        #region View Invokers

        public void ScrollIntoView(object obj) {
            MpAvClipTileViewModel ctvm = null;
            if (obj is MpAvClipTileViewModel) {
                ctvm = obj as MpAvClipTileViewModel;
            } else if (obj is int ciid) {
                ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                if (ctvm == null) {
                    int ciid_query_idx = CurrentQuery.GetItemOffsetIdx(ciid);
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
                    if (IsTrayEmpty) {
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
            MpRect svr = new MpRect(0, 0, ClipTrayScreenWidth, ClipTrayScreenHeight);
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

        public async Task UpdateSortOrder(bool fromModel = false) {
            if (fromModel) {
                //ClipTileViewModels.Sort(x => x.CopyItem.CompositeSortOrderIdx);
            } else {
                bool isDesc = CurrentQuery.IsDescending;
                int tagId = CurrentQuery.TagId;
                var citl = await MpDataModelProvider.GetCopyItemTagsForTagAsync(tagId);

                if (tagId == MpTag.AllTagId) {
                    //ignore sorting for sudo tags
                    return;
                }

                int count = isDesc ? citl.Count : 1;
                //loop through available tiles and reset tag's sort order, 
                //removing existing items from known ones and creating new ones if that's the case (it shouldn't)
                foreach (var ctvm in Items) {
                    MpCopyItemTag cit = citl.Where(x => x.CopyItemId == ctvm.CopyItem.Id).FirstOrDefault();
                    if (cit == null) {
                        cit = await MpCopyItemTag.Create(tagId, (int)ctvm.CopyItem.Id, count);
                    } else {
                        cit.CopyItemSortIdx = count;
                        citl.Remove(cit);
                    }
                    await cit.WriteToDatabaseAsync();
                    if (isDesc) {
                        count--;
                    } else {
                        count++;
                    }
                }
                //sort remaining unavailables by their sort order
                if (isDesc) {
                    citl = citl.OrderByDescending(x => x.CopyItemSortIdx).ToList();
                } else {
                    citl = citl.OrderBy(x => x.CopyItemSortIdx).ToList();
                }
                //update remaining unavailable items going with last count
                foreach (var cit in citl) {
                    cit.CopyItemSortIdx = count;
                    await cit.WriteToDatabaseAsync();
                    if (isDesc) {
                        count--;
                    } else {
                        count++;
                    }
                }
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
            //if (!MpSearchBoxViewModel.Instance.IsTextBoxFocused) {
            //    RequestFocus(SelectedItems[0]);
            //}

            RequestScrollToHome();

            //});
            IsSelectionReset = false;
        }


        public void ClipboardChanged(object sender, MpPortableDataObject mpdo) {
            bool is_change_ignored = MpAvMainWindowViewModel.Instance.IsMainWindowLoading ||
                                        IsAppPaused ||
                                        (MpPrefViewModel.Instance.IgnoreInternalClipboardChanges && MpPlatformWrapper.Services.ProcessWatcher.IsThisAppActive);
            if (is_change_ignored) {
                MpConsole.WriteLine("Clipboard Change Ignored by tray");
                MpConsole.WriteLine($"IsMainWindowLoading: {MpAvMainWindowViewModel.Instance.IsMainWindowLoading}");
                MpConsole.WriteLine($"IsAppPaused: {IsAppPaused}");
                MpConsole.WriteLine($"IgnoreInternalClipboardChanges: {MpPrefViewModel.Instance.IgnoreInternalClipboardChanges} IsThisAppActive: {MpPlatformWrapper.Services.ProcessWatcher.IsThisAppActive}");
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

        public void CleanupAfterPaste(MpAvClipTileViewModel sctvm) {
            IsPasting = false;
            //clean up pasted items state after paste
            sctvm.PasteCount++;
            sctvm.IsPasting = false;
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
                if(ci.Id == ModalClipTileViewModel.CopyItemId &&
                    IsAnyAppendMode) {

                    DeactivateAppendModeAsync().FireAndForgetSafeAsync();
                }
                MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Remove(ci);
                MpAvPersistentClipTilePropertiesHelper.RemovePersistentSize_ById(ci.Id);

                CurrentQuery.RemoveItemId(ci.Id);
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
                    }
                }

                CheckLoadMore();
                RefreshQueryTrayLayout();

                OnPropertyChanged(nameof(TotalTilesInQuery));
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

                    if (CurrentQuery.RemoveItemId(cit.CopyItemId)) {
                        CurrentQuery.NotifyQueryChanged();
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

                case nameof(ClipTrayScreenHeight):
                    if (ClipTrayScreenHeight < 0) {
                        Debugger.Break();
                        ClipTrayScreenHeight = 0;
                    }
                    break;
                case nameof(ClipTrayScreenWidth):
                    if(ClipTrayScreenWidth < 0) {
                        Debugger.Break();
                        ClipTrayScreenWidth = 0;
                    }
                    //RefreshLayout();
                    break;
                case nameof(ClipTrayTotalTileWidth):
                case nameof(ClipTrayTotalTileHeight):
                    if (ClipTrayTotalTileWidth < 0 || ClipTrayTotalTileHeight < 0) {
                        Debugger.Break();
                        ClipTrayScreenWidth = 0;
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
                    if(!IsThumbDragging) {
                        QueryCommand.Execute(ScrollOffset);
                    }
                    break;
                case nameof(IsScrollJumping):
                    if(IsScrollJumping) {
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

                    if(HasScrollVelocity) {
                        MpPlatformWrapper.Services.Cursor.UnsetCursor(null);
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
               
                case nameof(IsAnyTilePinned):
                    MpMessenger.SendGlobal(MpMessageType.PinTrayEmptyOrHasTile);
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
                    if(_isMainWindowOrientationChanging) {
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
                case MpMessageType.MainWindowHid:
                    // reset so tray will autosize/bringIntoView on ListBox items changed (since actual size is not bound)
                    HasUserAlteredPinTrayWidthSinceWindowShow = false;
                    break;

                // REQUERY

                case MpMessageType.RequeryCompleted:
                    if(IsInitialQuery) {
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
                    QueryCommand.Execute(ScrollOffset);
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
            if(!CanCheckLoadMore()) {
                return;
            }
            double dx = 0, dy = 0;
            bool isLessZoom = false;
            if(isZoomCheck) {
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

            if(checkHi && checkLo) {
                MpConsole.WriteLine("LoadMore infinite check detected, calling refresh query to prevent");
                CurrentQuery.NotifyQueryChanged();
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
                //                    hi_ctvm.TrayRect.Right - ScrollOffsetX - ClipTrayScreenWidth :
                //                    hi_ctvm.TrayRect.Bottom - ScrollOffsetY - ClipTrayScreenHeight;
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
            OnPropertyChanged(nameof(IsTrayEmpty));
            OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
            OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
            if (e.OldItems != null) { //if (e.Action == NotifyCollectionChangedAction.Move && IsLoadingMore) {
                foreach (MpAvClipTileViewModel octvm in e.OldItems) {
                    octvm.Dispose();
                }
            }
        }


        private bool CanTileNavigate() {
            bool canNavigate = !IsAnyBusy && !IsArrowSelecting &&

                  !HasScrollVelocity &&
                  !IsScrollingIntoView;

            if (canNavigate) {
                if (SelectedItem != null &&
                    SelectedItem.IsSubSelectionEnabled ||
                    (SelectedItem != null && !SelectedItem.IsTitleReadOnly && SelectedItem.IsTitleFocused)) {
                    canNavigate = false;
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
                    if (IsTrayEmpty) {
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

            var newCopyItem = await MpPlatformWrapper.Services.CopyItemBuilder.CreateAsync(cd);

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

        private async Task AddUpdateOrAppendCopyItemAsync(MpCopyItem ci) {
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
                if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    MpAvTagTrayViewModel.Instance.AllTagViewModel.LinkCopyItemCommand.Execute(ci.Id);
                }
                AddNewItemsCommand.Execute(null);
                OnCopyItemAdd?.Invoke(this, ci);
            }
        }
        


        private async Task PasteClipTileAsync(MpAvClipTileViewModel ctvm) {
            ctvm.IsPasting = true;

            var ds = ctvm.GetDragSource();
            if (ds == null) {
                Debugger.Break();
                return;
            }
            MpAvDataObject mpdo = await ds.GetDataObjectAsync(true);
            var pi = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;
            await MpPlatformWrapper.Services.ExternalPasteHandler.PasteDataObject(mpdo, pi);

            CleanupAfterPaste(ctvm);
        }

        


        #endregion

        #region Commands

        public ICommand UpdateTileRectCommand => new MpCommand<object>(
            (args) => {
                MpAvClipTileViewModel ctvm = null;
                MpRect prevOffsetRect = null;
                if(args is MpAvClipTileViewModel) {
                    ctvm = args as MpAvClipTileViewModel;
                } else if (args is object[] argParts) {
                    ctvm = argParts[0] as MpAvClipTileViewModel;
                    prevOffsetRect = argParts[1] as MpRect;
                }
                if(ctvm == null) {
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
                     scroll_delta.X = ClipTrayScreenWidth;
                 } else {
                     scroll_delta.Y = ClipTrayScreenHeight;
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
                    scroll_delta.X = ClipTrayScreenWidth;
                } else {
                    scroll_delta.Y = ClipTrayScreenHeight;
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
                     // pinning query tray tile from overlay button
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

                 if (CurrentQuery.RemoveItemId(ctvm_to_pin.CopyItemId)) {
                     // tile was part of query tray
                     if (Items.Contains(ctvm_to_pin)) {
                         int ctvm_to_pin_qidx = ctvm_to_pin.QueryOffsetIdx;

                         // trigger PublicHandle change to unload view
                         ctvm_to_pin.QueryOffsetIdx = -1;
                         Items.Remove(ctvm_to_pin);
                         Items.Where(x => x.QueryOffsetIdx > ctvm_to_pin_qidx).ForEach(x => x.QueryOffsetIdx = x.QueryOffsetIdx - 1);
                     }
                 }

                 if(ctvm_to_pin.IsPinned) {
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
                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 OnPropertyChanged(nameof(MinPinTrayScreenWidth));
                 OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayScreenHeight));
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
                     PinTrayTotalWidth = 0;
                     ObservedPinTrayScreenWidth = 0;
                 }

                 OnPropertyChanged(nameof(PinnedItems));
                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 OnPropertyChanged(nameof(MinPinTrayScreenWidth));
                 OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayScreenHeight));

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
                     SelectedItem = VisibleItems.Aggregate((a, b) => a.QueryOffsetIdx < b.QueryOffsetIdx ? a : b);
                 } else {
                     // prefer select neighbor pin tile 
                     int sel_idx = Math.Min(PinnedItems.Count - 1, Math.Max(0, unpinned_ctvm_idx));
                     SelectedItem = PinnedItems[sel_idx];
                 }
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
            (args) => args != null);


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
                    if(nctvm == null) {
                        nctvm = await CreateClipTileViewModel(ci);
                    }
                    while (nctvm.IsBusy) {
                        await Task.Delay(100);
                    }
                    ToggleTileIsPinnedCommand.Execute(nctvm);
                }

                _newModels.Clear();
                if (selectedId >= 0) {
                    while (IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    var selectedVm = AllItems.FirstOrDefault(x => x.CopyItemId == selectedId);
                    if (selectedVm != null) {
                        selectedVm.IsSelected = true;
                    }
                }

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
                //if(!string.IsNullOrEmpty(MpSearchBoxViewModel.Instance.LastSearchText)) {
                //    return false;
                //}
                //if (CurrentQuery.SortType == MpContentSortType.Manual) {
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
                    IsBusy = IsRequery = true;
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

                            await MpAvQueryInfoViewModel.Current.QueryForTotalCountAsync(
                                PinnedItems.Select(x => x.CopyItemId),
                                MpAvTagTrayViewModel.Instance.SelectedItem
                                .SelfAndAllDescendants
                                .Cast<MpAvTagTileViewModel>().Select(x => x.TagId));
                        }
                    } else {
                        // new query all content and offsets are re-initialized

                        ClearClipSelection();

                        await MpAvQueryInfoViewModel.Current.QueryForTotalCountAsync(
                            PinnedItems.Select(x => x.CopyItemId),
                            MpAvTagTrayViewModel.Instance.SelectedItem
                            .SelfAndAllDescendants
                            .Cast<MpAvTagTileViewModel>().Select(x => x.TagId));

                        // trigger unload event to wipe js eval's that maybe pending 
                        Items.ForEach(x => x.TriggerUnloadedNotification());

                        //Items.Clear();

                        MpAvPersistentClipTilePropertiesHelper.ClearPersistentWidths();

                        //RefreshLayout();
                        FindTotalTileSize();

                        OnPropertyChanged(nameof(TotalTilesInQuery));
                        OnPropertyChanged(nameof(ClipTrayTotalWidth));
                        OnPropertyChanged(nameof(MaxScrollOffsetX));
                        OnPropertyChanged(nameof(MaxScrollOffsetY));
                        OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                        OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
                    }

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

                    var cil = await MpAvQueryInfoViewModel.Current.FetchCopyItemsByQueryIdxListAsync(fetchQueryIdxList);

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
                        if (cur_ctvm == null || (isLoadMore && cur_ctvm.IsAnyCornerVisible)) {
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
                    //if(LayoutType == MpAvClipTrayLayoutType.Stack) {
                    //    await Task.WhenAll(initTasks.ToArray());
                    //} else {
                    //foreach (var initTask in initTasks) {
                    //    initTask.FireAndForgetSafeAsync(this);
                    //}
                    //}
                    Task.WhenAll(initTasks).FireAndForgetSafeAsync();

                    //while (Items.Any(x => x.IsBusy)) {
                    //    await Task.Delay(100);
                    //}

                    #endregion

                    #region Finalize State & Measurements


                    OnPropertyChanged(nameof(TotalTilesInQuery));
                    OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                    OnPropertyChanged(nameof(ClipTrayTotalWidth));
                    OnPropertyChanged(nameof(MaxScrollOffsetX));
                    OnPropertyChanged(nameof(MaxScrollOffsetY));
                    OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                    OnPropertyChanged(nameof(IsVerticalScrollBarVisible));

                    if (Items.Count == 0) {
                        ScrollOffsetX = LastScrollOffsetX = ScrollOffsetY = LastScrollOffsetY = 0;
                    }

                    IsBusy = false;
                    IsRequery = false;
                    OnPropertyChanged(nameof(IsAnyBusy));
                    OnPropertyChanged(nameof(IsEmpty));


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
                                while(IsAnyBusy) {
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
                            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));

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
                    .Execute(MpPlatformWrapper.Services.PlatformShorcuts.CutKeys);
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
                    .Execute(MpPlatformWrapper.Services.PlatformShorcuts.PasteKeys);            },
            (args) => {
                return SelectedItem != null && SelectedItem.IsSubSelectionEnabled;
            });

        public ICommand PasteFromClipTilePasteButtonCommand => new MpCommand<object>(
            (args) => {
                PasteClipTileAsync(args as MpAvClipTileViewModel).FireAndForgetSafeAsync();
            },
            (args)=> {
                if(args is MpAvClipTileViewModel ctvm) {
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
                var mpdo = await MpPlatformWrapper.Services.DataObjectHelperAsync.GetPlatformClipboardDataObjectAsync(false);

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
            async(presetIdObj) => {
                var preset = await MpDataModelProvider.GetItemAsync<MpPluginPreset>((int)presetIdObj);
                var analyticItemVm = MpAvAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.PluginGuid == preset.PluginGuid);

                EventHandler<MpCopyItem> analysisCompleteHandler = null;
                analysisCompleteHandler = (s, e) => {
                    analyticItemVm.OnAnalysisCompleted -= analysisCompleteHandler;
                    if(e == null) {
                        return;
                    }
                    AddUpdateOrAppendCopyItemAsync(e).FireAndForgetSafeAsync();
                };
                
                var presetVm = analyticItemVm.Items.FirstOrDefault(x => x.Preset.Id == preset.Id);

                analyticItemVm.SelectPresetCommand.Execute(presetVm);
                if(analyticItemVm.ExecuteAnalysisCommand.CanExecute(null)) {
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
            return ModalClipTileViewModel.ItemType == ci.ItemType;
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
            while(IsAddingClipboardItem) {
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
            }else {
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

            if(ModalClipTileViewModel.IsPlaceholder) {
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
            if(!isSilent) {

                OnPropertyChanged(nameof(AppendModeStateFlags));
            }
            

            await ModalClipTileViewModel.InitializeAsync(null);

            if(MpAvMainWindowViewModel.Instance.IsMainWindowOpen ||
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

            if (ModalClipTileViewModel.ItemType == MpCopyItemType.FileList) {
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
            //        await MpCopyItemSource.CreateAsync(
            //               copyItemId: AppendNotifierViewModel.CopyItemId,
            //               sourceObjId: aci_source.SourceObjId,
            //               sourceType: aci_source.CopyItemSourceType);
            //    }
            //    if (aci.WasDupOnCreate) {
            //        // also ref if exisiting item
            //        await MpCopyItemSource.CreateAsync(
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
                if(!IsAnyAppendMode && new_manual_state) {
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
