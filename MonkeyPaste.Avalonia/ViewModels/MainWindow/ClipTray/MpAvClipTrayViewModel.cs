using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonoMac.Foundation;
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
    public enum MpAvClipTrayLayoutType {
        Stack,
        Grid
    }

    public class MpAvClipTrayViewModel : 
        MpAvSelectorViewModelBase<object, MpAvClipTileViewModel>,
        MpIBootstrappedItem, 
        MpIPagingScrollViewerViewModel,
        MpIActionComponent,
        MpIContextMenuViewModel,
        MpIQueryInfoProvider{
        #region Private Variables
        private int _anchor_query_idx { get; set; } = -1;

        private bool _isMainWindowOrientationChanging = false;

        #endregion

        #region Constants

        public const double MIN_SIZE_ZOOM_FACTOR_COEFF = (double)1 / (double)7;

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

        #region MpIQueryInfoProvider Implementation
        public void RestoreQueryInfo() {
            // unimplemented
        }

        public void SetQueryInfo() {
            MpAvQueryInfoViewModel.Current.TotalItemsInQuery = TotalTilesInQuery;
        }

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

                var tagItems = MpAvTagTrayViewModel.Instance.AllTagViewModel.ContentMenuItemViewModel.SubItems;
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = @"_Copy",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("CopyImage") as string,
                            Command = CopySelectedClipsCommand,
                            ShortcutType = MpShortcutType.CopySelection
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Paste",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PasteImage") as string,
                            Command = PasteSelectedClipsCommand,
                            ShortcutType = MpShortcutType.PasteSelectedItems
                        },
                        //new MpMenuItemViewModel() {
                        //    Header = @"Paste _Here",
                        //    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PasteImage") as string,
                        //    Command = PasteCurrentClipboardIntoSelectedTileCommand,
                        //    ShortcutType = MpShortcutType.PasteSelectedItems
                        //},
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },

                        new MpMenuItemViewModel() {
                            Header = @"_Delete",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DeleteImage") as string,
                            Command = DeleteSelectedClipsCommand,
                            ShortcutType = MpShortcutType.DeleteSelectedItems
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Rename",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("RenameImage") as string,
                            Command = EditSelectedTitleCommand,
                            ShortcutType = MpShortcutType.EditTitle
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Edit",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("EditContentImage") as string,
                            Command = EditSelectedContentCommand,
                            ShortcutType = MpShortcutType.EditContent
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Transform",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("ToolsImage") as string,
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = @"_Find and Replace",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SearchImage") as string,
                                    Command = FindAndReplaceSelectedItem,
                                    ShortcutType = MpShortcutType.FindAndReplaceSelectedItem
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Duplicate",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DuplicateImage") as string,
                                    Command = DuplicateSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.Duplicate
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Merge",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("MergeImage") as string,
                                    Command = MergeSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.MergeSelectedItems
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Email",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("EmailImage") as string,
                                    Command = SendToEmailCommand,
                                    ShortcutType = MpShortcutType.SendToEmail
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Qr Code",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("QrImage") as string,
                                    Command = CreateQrCodeFromSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.CreateQrCode
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Audio",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SpeakImage") as string,
                                    Command = SpeakSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.SpeakSelectedItem
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To Web Search",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("WebImage") as string,
                                    SubItems = new List<MpMenuItemViewModel>() {
                                        new MpMenuItemViewModel() {
                                            Header = "_Google",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("GoogleImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.google.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "_Bing",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("BingImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.bing.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "_DuckDuckGo",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DuckImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://duckduckgo.com/?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "_Yandex",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("YandexImage") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://yandex.com/search/?text="
                                        },
                                        new MpMenuItemViewModel() { IsSeparator = true},
                                        new MpMenuItemViewModel() {
                                            Header = "_Manage...",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("CogImage") as string
                                        },
                                    }
                                },
                                new MpMenuItemViewModel() {
                                    Header = $"'{SelectedItem.AppViewModel.AppName}' to _Excluded App",
                                    IconId = SelectedItem.AppViewModel.AppId,
                                    Command = ExcludeSubSelectedItemApplicationCommand
                                },
                                new MpMenuItemViewModel() {
                                    Header = SelectedItem.SourceViewModel == null ||
                                             SelectedItem.UrlViewModel == null?
                                                null :
                                                $"'{SelectedItem.UrlViewModel.UrlDomainPath}' to _Excluded Domain",
                                    IconId = SelectedItem.UrlViewModel == null ?
                                                0 :
                                                SelectedItem.UrlViewModel.IconId,
                                    IsVisible = SelectedItem.UrlViewModel != null,
                                    Command = ExcludeSubSelectedItemUrlDomainCommand
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Into _Macro",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("RobotClawImage") as string,
                                    Command = MpAvSystemTrayViewModel.Instance.ShowSettingsWindowCommand,
                                    CommandParameter = this
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Shorcut",
                                    ShortcutType = MpShortcutType.PasteCopyItem,
                                    ShortcutObjId = SelectedItem.CopyItemId,
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("HotkeyImage") as string,
                                    Command = MpAvSystemTrayViewModel.Instance.ShowSettingsWindowCommand,
                                    CommandParameter = this
                                },
                            }
                        },
                        MpAnalyticItemCollectionViewModel.Instance.ContextMenuItemViewModel,
                        new MpMenuItemViewModel() {
                            Header = "_Select",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SelectionImage") as string,
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = "_Bring to Front",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("BringToFrontImage") as string,
                                    Command = BringSelectedClipTilesToFrontCommand,
                                    ShortcutType = MpShortcutType.BringSelectedToFront
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Send to Back",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SendToBackImage") as string,
                                    Command = SendSelectedClipTilesToBackCommand,
                                    ShortcutType = MpShortcutType.SendSelectedToBack
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Select _All",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SelectAllImage") as string,
                                    Command = SelectAllCommand,
                                    ShortcutType = MpShortcutType.SelectAll
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Invert Selection",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("InvertSelectionImage") as string,
                                    Command = InvertSelectionCommand,
                                    ShortcutType = MpShortcutType.InvertSelection
                                },
                            }
                        },
                        new MpMenuItemViewModel() {IsSeparator = true},
                        MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(SelectedItem),
                        new MpMenuItemViewModel() {IsSeparator = true},
                        new MpMenuItemViewModel() {
                            Header = @"Pin To _Collection",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PinToCollectionImage") as string,
                            SubItems = tagItems
                        }
                    },
                };
            }
        }

        #endregion
                
        #region View Models

        public IEnumerable<MpAvClipTileViewModel> SortOrderedItems => Items.OrderBy(x => x.QueryOffsetIdx);

        public ObservableCollection<MpAvClipTileViewModel> PinnedItems { get; set; } = new ObservableCollection<MpAvClipTileViewModel>();

        public IEnumerable<MpAvClipTileViewModel> AllItems {
            get {
                foreach (var ctvm in Items) {
                    yield return ctvm;
                }
                foreach (var pctvm in PinnedItems) {
                    yield return pctvm;
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
                //if (PinnedItems.Any(x => x.IsSelected)) {
                //    return PinnedItems.FirstOrDefault(x => x.IsSelected);
                //}
                return AllItems.FirstOrDefault(x => x.IsSelected);
            }
            set {
                //if (SelectedItem != value && value != null) {
                //    if (PinnedItems.Any(x => x.CopyItemId == value.CopyItemId)) {
                //        PinnedItems.ForEach(x => x.IsSelected = x.CopyItemId == value.CopyItemId);
                //        Items.ForEach(x => x.IsSelected = false);
                //    } else if (Items.Any(x => x.CopyItemId == value.CopyItemId)) {
                //        Items.ForEach(x => x.IsSelected = x.CopyItemId == value.CopyItemId);
                //        PinnedItems.ForEach(x => x.IsSelected = false);
                //    } else {
                //        SelectedItem = null;
                //    }

                //}
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

        public MpAvClipTileViewModel DragItem {
            get {
                var dragItem = Items.FirstOrDefault(x => x.IsTileDragging);
                if (dragItem == null) {
                    return PinnedItems.FirstOrDefault(x => x.IsTileDragging);
                }
                return dragItem;
            }
        }

        public IEnumerable<MpAvClipTileViewModel> VisibleItems => Items.Where(x => x.IsAnyCornerVisible);

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
                if(LayoutType == MpAvClipTrayLayoutType.Stack) {
                    return 0.85d;
                }
                return 0.85d;
            }
        }

        public double ScrollWheelDampeningX {
            get {
                if (LayoutType == MpAvClipTrayLayoutType.Stack) {
                    return 0.03d;
                }
                return 0.01d;
            }
        }

        public double ScrollFrictionY {
            get {
                if (LayoutType == MpAvClipTrayLayoutType.Stack) {
                    return 0.85d;
                }
                return 0.85d;
            }
        }

        public double ScrollWheelDampeningY {
            get {
                if (LayoutType == MpAvClipTrayLayoutType.Stack) {
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

            bool isGrid = LayoutType == MpAvClipTrayLayoutType.Grid;
            bool isStack = !isGrid;

            bool isFindTileIdx = scrollOffsetX >= 0 && scrollOffsetY >= 0;
            bool isFindTileRect = !isFindTileIdx && queryOffsetIdx >= 0;
            bool isFindTotalSize = !isFindTileRect;
            
            int totalTileCount = MpDataModelProvider.AvailableQueryCopyItemIds.Count;
            queryOffsetIdx = isFindTotalSize ? TotalTilesInQuery - 1 : queryOffsetIdx;
            if(queryOffsetIdx >= totalTileCount) {
                return null;
            }

            int startIdx = 0;// prevOffsetRect == null ? 0 : queryOffsetIdx;

            var total_size = MpSize.Empty;
            int gridFixedCount = -1;

            MpRect last_rect = null;// prevOffsetRect;

            for (int i = startIdx; i <= queryOffsetIdx; i++) {
                int tileId = MpDataModelProvider.AvailableQueryCopyItemIds[i];
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
                    if (LayoutType == MpAvClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return ClipTrayScreenWidth;
                } else {
                    if(LayoutType == MpAvClipTrayLayoutType.Stack) {
                        return ClipTrayScreenWidth;
                    }
                    return double.PositiveInfinity;
                }
            }
        }

        public double DesiredMaxTileBottom {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    if (LayoutType == MpAvClipTrayLayoutType.Stack) {
                        return ClipTrayScreenHeight;
                    }
                    return double.PositiveInfinity;
                } else {
                    if (LayoutType == MpAvClipTrayLayoutType.Stack) {
                        return double.PositiveInfinity;
                    }
                    return ClipTrayScreenHeight;
                }
            }
        }
        
        public double DefaultItemWidth {
            get {
                double defaultWidth;
                if (LayoutType == MpAvClipTrayLayoutType.Stack) {
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
                if (LayoutType == MpAvClipTrayLayoutType.Stack) {
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

        public double DefaultEditableItemWidth => 1130.0d; // based on edit toolbar width

        public MpSize DefaultEditableItemSize => new MpSize(DefaultEditableItemWidth, DefaultItemHeight);
        public double ScrollBarFixedAxisSize => 30;

        #endregion

        #region Virtual

        //set in civm IsSelected property change, DragDrop.Drop (copy mode)
        
        


        public int HeadQueryIdx => Items.Count == 0 ? -1 : Items.Where(x => x.QueryOffsetIdx >= 0).Min(x => x.QueryOffsetIdx);// Math.Max(0, Items.Min(x => x.QueryOffsetIdx));

        public int TailQueryIdx => Items.Count == 0 ? -1 : Items.Max(x => x.QueryOffsetIdx);// Math.Min(TotalTilesInQuery - 1, Items.Max(x => x.QueryOffsetIdx));

        public int HeadItemIdx => HeadItem == null ? -1 : Items.IndexOf(HeadItem);
        public int TailItemIdx => TailItem == null ? -1 : Items.IndexOf(TailItem);
        public int MaxLoadQueryIdx => Math.Max(0, MaxClipTrayQueryIdx - DefaultLoadCount + 1);

        public int MaxClipTrayQueryIdx => TotalTilesInQuery - 1; 
        public int MinClipTrayQueryIdx => 0;
        #endregion

        #endregion

        #region Layout

        public double PlayPauseButtonWidth { get; set; }
        public MpRect PlayPauseButtonBounds { get; set; } = new MpRect();

        #endregion

        #region Appearance

        public MpAvClipTrayLayoutType LayoutType { get; set; } = MpAvClipTrayLayoutType.Stack;

        public string PinTrayBackgroundHexColor => IsDragOverPinTray ?
            MpSystemColors.lightcyan1.AdjustAlpha(MpPrefViewModel.Instance.MainWindowOpacity) :
            MpSystemColors.salmon.AdjustAlpha(MpPrefViewModel.Instance.MainWindowOpacity);

        public string ClipTrayBackgroundHexColor => 
            MpSystemColors.darkviolet.AdjustAlpha(MpPrefViewModel.Instance.MainWindowOpacity);

        #endregion

        #region State

        public bool IsEmpty => Items.Count == 0;

        public bool IsPinTrayEmpty => PinnedItems.Count == 0;

        public bool IsPinTrayVisible {
            get {
                if (!IsPinTrayEmpty) {
                    return true;
                }
                //if (IsPinTrayDropPopOutVisible) {
                //    return true;
                //} 
                return false;
            }
        }
        public bool HasScrollVelocity => Math.Abs(ScrollVelocityX) + Math.Abs(ScrollVelocityY) > 0.1d;

        public bool IsScrollingIntoView { get; set; }

        public bool IsGridLayout { get; set; }        

        public bool IsRequery { get; set; } = false;

        #region Drag Drop

        public bool IsAnyTileDragging {
            get {
                return Items.Any(x => x.IsTileDragging) || PinnedItems.Any(x => x.IsTileDragging);
            }
        }

        //public bool IsExternalDragOverClipTrayContainer { get; set; }
        public bool IsDragOverPinTray { get; set; }

        #endregion

        #region Child Property Wrappers

        public bool IsAnyBusy => Items.Any(x => x.IsAnyBusy) || PinnedItems.Any(x => x.IsAnyBusy) || IsBusy;
        public bool IsAnyTileContextMenuOpened => Items.Any(x => x.IsContextMenuOpen) || PinnedItems.Any(x => x.IsContextMenuOpen);

        public bool IsAnyResizing => Items.Any(x => x.IsResizing) || PinnedItems.Any(x => x.IsResizing);

        public bool CanAnyResize => Items.Any(x => x.CanResize) || PinnedItems.Any(x => x.CanResize);

        public bool IsAnyEditing => Items.Any(x => !x.IsContentAndTitleReadOnly) || PinnedItems.Any(x => !x.IsContentAndTitleReadOnly);


        public bool IsAnyHovering => Items.Any(x => x.IsHovering) || PinnedItems.Any(x => x.IsHovering);


        public bool IsAnyEditingClipTitle => Items.Any(x => !x.IsTitleReadOnly) || PinnedItems.Any(x => !x.IsTitleReadOnly);

        public bool IsAnyEditingClipTile => Items.Any(x => !x.IsContentReadOnly) || PinnedItems.Any(x => !x.IsContentReadOnly);

        public bool IsAnyPastingTemplate => Items.Any(x => x.IsPastingTemplate) || PinnedItems.Any(x => x.IsPastingTemplate);

        

        public bool IsAnyTilePinned => PinnedItems.Count > 0;


        public bool IsUnpinning { get; set; }

        public bool IsBatchOffsetChange { get; set; } = false;
        #endregion

        #endregion


        #endregion

        #region Constructors

        private MpAvClipTrayViewModel() : base() {
        }


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            LogPropertyChangedEvents = false;

            IsBusy = true;

            while (MpAvSourceCollectionViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            MpAvQueryInfoViewModel.Current.RegisterProvider(this);

            PropertyChanged += MpAvClipTrayViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;

            //MpDataModelProvider.AvailableQueryCopyItemIds.CollectionChanged += AllFetchedAndSortedCopyItemIds_CollectionChanged;

            MpDb.SyncAdd += MpDbObject_SyncAdd;
            MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.SyncDelete += MpDbObject_SyncDelete;

            MpPlatformWrapper.Services.ClipboardMonitor.OnClipboardChanged += ClipboardChanged;

            //DefaultLoadCount = MpMeasurements.Instance.DefaultTotalVisibleClipTiles * 1 + 2;

            MpMessenger.Register<MpMessageType>(MpDataModelProvider.QueryInfo, ReceivedQueryInfoMessage);

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

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

        public void RefreshLayout(MpAvClipTileViewModel fromItem = null) {
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
            //QueryCommand.Execute(anchor_query_idx);
            var anchor_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == _anchor_query_idx);
            if(anchor_ctvm == null) {
                Debugger.Break();
                return;
            }

            ForceScrollOffset(anchor_ctvm.TrayLocation);
        }

        #endregion

        #region Private Methods

        


        private void MpAvClipTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(PlayPauseButtonWidth):
                    MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.TagTrayScreenWidth));
                    break;
                case nameof(PlayPauseButtonBounds):
                    MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.TagTrayScreenWidth));
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
                case nameof(DefaultItemSize):
                    //AllItems.ForEach(x => x.OnPropertyChanged(nameof(x.MinSize)));
                    break;
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
                case nameof(IsDragOverPinTray):
                    //notify pin tray to pop out if no item pinned
                    OnPropertyChanged(nameof(PinTrayBackgroundHexColor));
                    break;
               
                case nameof(IsAnyTilePinned):
                    MpMessenger.SendGlobal(MpMessageType.PinTrayEmptyOrHasTile);
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                // LAYOUT CHANGE
                case MpMessageType.TrayLayoutChanged:                    
                    RefreshLayout();
                    if (LayoutType == MpAvClipTrayLayoutType.Grid) {

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
                    RefreshLayout();
                    LockScrollToAnchor();
                    CheckLoadMore();
                    break;
                case MpMessageType.MainWindowSizeChangeEnd:
                    // NOTE Size reset doesn't call changed so treat end as changed too
                    RefreshLayout();
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
                    RefreshLayout();
                    LockScrollToAnchor();
                    break;

                // TRAY ZOOM
                case MpMessageType.TrayZoomFactorChangeBegin:
                    MpConsole.WriteLine("Zoom change begin: " + ZoomFactor);
                    SetScrollAnchor();
                    break;
                case MpMessageType.TrayZoomFactorChanged:
                    MpConsole.WriteLine("Zoom changed: " + ZoomFactor);
                    RefreshLayout();
                    LockScrollToAnchor();
                    CheckLoadMore(true);
                    break;
                case MpMessageType.TrayZoomFactorChangeEnd:
                    MpConsole.WriteLine("Zoom change end: " + ZoomFactor);
                    RefreshLayout();
                    LockScrollToAnchor();
                    CheckLoadMore(true);

                    SetScrollAnchor();
                    break;

                // SCROLL JUMP
                case MpMessageType.JumpToIdxCompleted:
                    SetScrollAnchor();
                    break;

                // REQUERY
                case MpMessageType.RequeryCompleted:
                    SetQueryInfo();
                    break;

                // Selection
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
                   // Dispatcher.UIThread.Post(() => {
                        if (SelectedItem == null) {
                            ResetClipSelection(false);
                        }
                        AddNewItemsCommand.Execute(null);
                    //});
                    break;
                case MpMessageType.MainWindowHid:
                    // reset so tray will autosize/bringIntoView on ListBox items changed (since actual size is not bound)
                    HasUserAlteredPinTrayWidthSinceWindowShow = false;
                    break;
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

            bool isScrollHorizontal = (LayoutType == MpAvClipTrayLayoutType.Stack && ListOrientation == Orientation.Horizontal) ||
                                      (LayoutType == MpAvClipTrayLayoutType.Grid && ListOrientation == Orientation.Vertical);

            if(checkHi && checkLo) {
                MpConsole.WriteLine("LoadMore infinite check detected, calling refresh query to prevent");
                MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
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
                    LayoutType = MpAvClipTrayLayoutType.Grid;
                } else {
                    LayoutType = MpAvClipTrayLayoutType.Stack;
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

        public ICommand ScrollToNextPageCommand => new MpAsyncCommand(
             async () => {
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

        public ICommand ScrollToPreviousPageCommand => new MpAsyncCommand(
            async() => {
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
        #endregion


        #region Ported from wpf

        #region Private Variables      

        //private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;

        //private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        private List<MpCopyItem> _newModels = new List<MpCopyItem>();

        private MpCopyItem _appendModeCopyItem;

        private Dictionary<int, int> _manualSortOrderLookup = null;


        #endregion

        #region Constants

        public const int DISABLE_READ_ONLY_DELAY_MS = 500;

        public const double MAX_TILE_SIZE_CONTAINER_PAD = 50;
        #endregion

        #region Properties

        #region Layout

        public double DefaultPinTrayWidth => DefaultItemWidth*1.4;
        public double ObservedPinTrayScreenWidth { get; set; }
        public double ObservedPinTrayScreenHeight { get; set; }

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
                if(ListOrientation == Orientation.Horizontal) {

                    return ClipTrayContainerScreenWidth - MinClipTrayScreenWidth;
                }
                return double.PositiveInfinity;
            }
        }
        public double MaxPinTrayScreenHeight {
            get {
                if (ListOrientation == Orientation.Horizontal) {

                    return double.PositiveInfinity;
                }
                return ClipTrayContainerScreenHeight - MinClipTrayScreenHeight;
            }
        }


        public double MinClipTrayScreenWidth => MinClipOrPinTrayScreenWidth;
        public double MinClipTrayScreenHeight => MinClipOrPinTrayScreenHeight;

        public double MinClipOrPinTrayScreenWidth => 0;
        public double MinClipOrPinTrayScreenHeight => 0;


        public double ClipTrayContainerScreenWidth { get; set; }
        public double ClipTrayContainerScreenHeight { get; set; }
        public double MaxTileWidth => Math.Max(0, ClipTrayScreenWidth - MAX_TILE_SIZE_CONTAINER_PAD);
        public double MaxTileHeight => double.PositiveInfinity;// Math.Max(0, ClipTrayScreenHeight - MAX_TILE_SIZE_CONTAINER_PAD);

        public int CurGridFixedCount { get; set; }

        #endregion

        #region Appearance

        public int MaxTotalVisibleClipTiles {
            get {
                return 10;// (int)Math.Ceiling(ClipTrayScreenWidth / MpMeasurements.Instance.ClipTileBorderMinWidth);
            }
        }

        #endregion

        #region Business Logic


        public int RemainingItemsCountThreshold {
            get {
                if (LayoutType == MpAvClipTrayLayoutType.Stack) {
                    return 5;
                }
                return CurGridFixedCount * 2;
            }
        }
        public int LoadMorePageSize {
            get {
                if(LayoutType == MpAvClipTrayLayoutType.Stack) {
                    return 1;
                }
                return CurGridFixedCount;
            }
        }

        public int TotalTilesInQuery => MpDataModelProvider.TotalTilesInQuery;

        public int DefaultLoadCount {
            get {
                if(LayoutType == MpAvClipTrayLayoutType.Stack) {
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


        #endregion

        #region State

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

        public bool IsAppendMode { get; set; }

        public bool IsAppendLineMode { get; set; }

        public bool IsAnyAppendMode => IsAppendMode || IsAppendLineMode;

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

        #endregion

        #region Visibility

        #endregion

        #endregion

        #region Events

        public event EventHandler<object> OnFocusRequest;
        public event EventHandler OnUiRefreshRequest;
        public event EventHandler<object> OnScrollIntoPinTrayViewRequest;
        public event EventHandler OnScrollToHomeRequest;

        public event EventHandler<MpCopyItem> OnCopyItemAdd;

        #endregion

        #region Constructors


        #endregion

        #region Public Methods

        #region MpIMatchTrigger Implementation

        public void RegisterActionComponent(MpIActionTrigger mvm) {
            OnCopyItemAdd += mvm.OnActionTriggered;
            MpConsole.WriteLine($"ClipTray Registered {mvm.Label} matcher");
        }

        public void UnregisterActionComponent(MpIActionTrigger mvm) {
            OnCopyItemAdd -= mvm.OnActionTriggered;
            MpConsole.WriteLine($"Matcher {mvm.Label} Unregistered from OnCopyItemAdded");
        }

        #endregion

        #region View Invokers

        public void ScrollIntoView(object obj) {
            MpAvClipTileViewModel ctvm = null;
            if(obj is MpAvClipTileViewModel) {
                ctvm = obj as MpAvClipTileViewModel;
            } else if(obj is int ciid) {
                ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == ciid);
                if(ctvm == null) {
                    int ciid_query_idx = MpDataModelProvider.AvailableQueryCopyItemIds.IndexOf(ciid);
                    if(ciid_query_idx < 0) {
                        if(ciid < 0) {
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
                        while(IsAnyBusy) { await Task.Delay(100); }
                        ctvm = Items.FirstOrDefault(x => x.CopyItemId == ciid);
                        if(ctvm == null) {
                            // data model provider should have come up w/ nothing here
                            Debugger.Break();
                            return;
                        }
                        ScrollIntoView(ctvm);
                    });
                    return;
                }
            } else if(obj == null) {
                // occurs when nothing is selected
                if(IsPinTrayEmpty) {
                    if(IsTrayEmpty) {
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

        public void RequestFocus(object obj) {
            OnFocusRequest?.Invoke(this, obj);
        }

        public void RequestUiRefresh() {
            OnUiRefreshRequest?.Invoke(this, null);
        }
        #endregion      
        public async Task UpdateSortOrder(bool fromModel = false) {
            if (fromModel) {
                //ClipTileViewModels.Sort(x => x.CopyItem.CompositeSortOrderIdx);
            } else {
                bool isManualSort = MpDataModelProvider.QueryInfo.SortType == MpContentSortType.Manual;

                if (isManualSort) {
                    _manualSortOrderLookup = new Dictionary<int, int>();
                    foreach (var ctvm in Items) {
                        if (_manualSortOrderLookup.ContainsKey(ctvm.CopyItemId)) {
                            continue;
                        }
                        _manualSortOrderLookup.Add(ctvm.CopyItemId, Items.IndexOf(ctvm));
                    }
                }

                bool isDesc = MpDataModelProvider.QueryInfo.IsDescending;
                int tagId = MpDataModelProvider.QueryInfo.TagId;
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
        public MpCopyItemType GetSelectedClipsType() {
            //returns none if all clips aren't the same type
            if (SelectedItem == null) {
                return MpCopyItemType.None;
            }
            return SelectedItem.ItemType;
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

        public void RefreshAllCommands() {
            foreach (MpAvClipTileViewModel ctvm in Items) {
                ctvm.RefreshAsyncCommands();
            }
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


        public MpAvClipTileViewModel GetClipTileViewModelById(int ciid) {
            var pctvm = PinnedItems.FirstOrDefault(x => x.CopyItemId == ciid);
            if (pctvm != null) {
                return pctvm;
            }
            return Items.FirstOrDefault(x => x.CopyItemId == ciid);
        }

        public MpAvClipTileViewModel GetClipTileViewModelByGuid(string ciguid) {
            var pctvm = PinnedItems.FirstOrDefault(x => x.CopyItemGuid == ciguid);
            if (pctvm != null) {
                return pctvm;
            }
            return Items.FirstOrDefault(x => x.CopyItemGuid == ciguid);
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

        public void StoreSelectionState(MpAvClipTileViewModel tile) {
            if(tile.IsPlaceholder) {
                // started happening in external pin tray drop
                Debugger.Break();
                return;
            }
            if (!tile.IsSelected) {
                return;
            }

            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels = new List<MpCopyItem>() { tile.CopyItem };
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

            sctvm.IsPasting = false;
            if (sctvm.HasTemplates) {
                sctvm.ClearEditing();
                sctvm.TemplateCollection.Reset();
                sctvm.TemplateRichHtml = string.Empty;
                //sctvm.RequestUiUpdate();
                //sctvm.RequestScrollToHome();
            }

        }

        #endregion

        #region Db Events

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                //_allTiles.Add(CreateClipTileViewModel(ci));
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                var ivm = GetClipTileViewModelById(ci.Id);
                //ivm.CopyItem = ci;
            }
        }

        protected override async void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpCopyItem ci) {
                MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Remove(ci);
                MpAvPersistentClipTilePropertiesHelper.RemovePersistentSize_ById(ci.Id);

                MpDataModelProvider.AvailableQueryCopyItemIds.Remove(ci.Id);
                //MpDataModelProvider.RemoveQueryItem(ci.Id);

                var removed_ctvm = GetClipTileViewModelById(ci.Id);
                if (removed_ctvm != null) {
                    bool wasSelected = removed_ctvm.IsSelected;

                    if (removed_ctvm.IsPinned) {
                        var pctvm = PinnedItems.FirstOrDefault(x => x.CopyItemId == ci.Id);


                        if (pctvm != null) {
                            int pinIdx = PinnedItems.IndexOf(pctvm);
                            // Flag QueryOffsetIdx = -1 so doesn't attempt to return it to tray
                            pctvm.QueryOffsetIdx = -1;
                            await Dispatcher.UIThread.InvokeAsync(async () => {
                                ToggleTileIsPinnedCommand.Execute(pctvm);

                                while (IsAnyBusy) {
                                    await Task.Delay(100);
                                }

                                if (PinnedItems.Count == 0) {
                                    return;
                                }
                                pinIdx = Math.Min(pinIdx, PinnedItems.Count - 1);
                                PinnedItems[pinIdx].IsSelected = true;
                            });
                        }
                    } else {
                        int removedTrayIdx = removed_ctvm.ItemIdx;
                        int removedQueryOffsetIdx = removed_ctvm.QueryOffsetIdx;
                        bool wasTail = removedQueryOffsetIdx == TailQueryIdx;

                        Items[removedTrayIdx] = await CreateClipTileViewModel(null);
                        Items.Move(removedTrayIdx, Items.Count - 1);

                        foreach (var ctvm in Items) {
                            if (ctvm.QueryOffsetIdx <= removedQueryOffsetIdx) {
                                continue;
                            }
                            ctvm.QueryOffsetIdx--;
                            if(MpAvPersistentClipTilePropertiesHelper.IsTileHaveUniqueSize(ctvm.CopyItemId)) {
                                MpAvPersistentClipTilePropertiesHelper.ShiftPersistentSize(ctvm.CopyItemId, ctvm.QueryOffsetIdx + 1, ctvm.QueryOffsetIdx);
                            }
                            
                            ctvm.OnPropertyChanged(nameof(ctvm.TrayX));
                        }

                        if (wasSelected) {
                            MpAvClipTileViewModel newSelected_ctvm = null;
                            if (wasTail) {
                                newSelected_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == TailQueryIdx);
                            } else {
                                newSelected_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == removedQueryOffsetIdx);
                            }
                            if (newSelected_ctvm == null && Items.Where(x => !x.IsPlaceholder).Count() > 0) {
                                newSelected_ctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == HeadQueryIdx);
                            }
                            if (newSelected_ctvm != null) {
                                newSelected_ctvm.IsSelected = true;
                            }
                        }
                        //MpConsole.Write(string.Join(Environment.NewLine,Items.OrderBy(x => x.QueryOffsetIdx).Select(x => "Query Offset: " + x.QueryOffsetIdx)));

                        // NOTE since queryOffsetIdx's were decremented, now must notify at this queryIdx
                        //Items.Where(x => x.QueryOffsetIdx >= removedQueryOffsetIdx).ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
                    }
                }

                OnPropertyChanged(nameof(TotalTilesInQuery));
            } else if (e is MpCopyItemTag cit && Items.Any(x => x.CopyItemId == cit.CopyItemId)) {
                var ctvm = Items.FirstOrDefault(x => x.CopyItemId == cit.CopyItemId);
                if (ctvm == null) {
                    return;
                }
                var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == cit.TagId);
                if (ttvm == null || !ttvm.IsSelected) {
                    return;
                }
                bool isAssociated = await ttvm.IsCopyItemLinkedAsync(ctvm.CopyItemId);
                if (isAssociated) {
                    return;
                }
                //MpDataModelProvider.RemoveQueryItem(cit.CopyItemId);
                if(MpDataModelProvider.AvailableQueryCopyItemIds.Remove(cit.CopyItemId)) {
                    MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                }                
            }
        }

        #region Sync Events

        private void MpDbObject_SyncDelete(object sender, MpDbSyncEventArgs e) {
            Dispatcher.UIThread.Post((Action)(() => {
                if (sender is MpCopyItem ci) {
                    var ctvmToRemove = GetClipTileViewModelById(ci.Id);
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
            Dispatcher.UIThread.Post((Action)(() => {
            }));
        }

        private void MpDbObject_SyncAdd(object sender, MpDbSyncEventArgs e) {
            Dispatcher.UIThread.Post(async () => {
                if (sender is MpCopyItem ci) {
                    ci.StartSync(e.SourceGuid);

                    var svm = MpAvSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == ci.SourceId);

                    var app = svm.AppViewModel.App;
                    app.StartSync(e.SourceGuid);
                    //ci.Source.App.Icon.StartSync(e.SourceGuid);
                    //ci.Source.App.Icon.IconImage.StartSync(e.SourceGuid);

                    var dupCheck = this.GetClipTileViewModelById((int)ci.Id);
                    if (dupCheck == null) {
                        if (ci.Id == 0) {
                            await ci.WriteToDatabaseAsync();
                        }
                        _newModels.Add(ci);
                        //AddNewTiles();
                    } else {
                        MpConsole.WriteTraceLine(@"Warning, attempting to add existing copy item: " + dupCheck.CopyItem.ItemData + " ignoring and updating existing.");
                        //dupCheck.CopyItem = ci;
                    }
                    app.EndSync();
                    //ci.Source.App.Icon.EndSync();
                    //ci.Source.App.Icon.IconImage.EndSync();
                    ci.EndSync();

                    ResetClipSelection();
                }
            });
        }

        #endregion

        #endregion

        #region Private Methods


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

        public void ValidateItemsTrayX() {
            var orderedItems = Items.OrderBy(x => x.QueryOffsetIdx).ToList();
            for (int i = 0; i < Items.Count; i++) {
                if (i == 0) {
                    continue;
                }
                if (orderedItems[i].TrayX < orderedItems[i - 1].TrayX) {
                    Debugger.Break();
                }
            }
        }


        private void ReceivedQueryInfoMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.QueryChanged:
                    QueryCommand.Execute(null);
                    break;
                case MpMessageType.SubQueryChanged:
                    QueryCommand.Execute(ScrollOffset);
                    break;
            }

            //if(MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {

            //    Dispatcher.UIThread.InvokeAsync(OnPostMainWindowLoaded);
            //}
        }

        private void ReceivedDragDropManagerMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ItemDragBegin:
                case MpMessageType.ItemDragEnd:
                    OnPropertyChanged(nameof(IsAnyTileDragging));
                    break;
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

        private async Task SelectNeighborHelperAsync(int row_offset,int col_offset) {
            if(row_offset != 0 && col_offset != 0) {
                // NO! should only be one or the other
                Debugger.Break();
                return;
            }
            if(row_offset == 0 && col_offset == 0) {
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
            if(row_offset != 0) {
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

            var totalAddSw = new Stopwatch();
            totalAddSw.Start();

            var createItemSw = new Stopwatch();
            createItemSw.Start();


            var newCopyItem = await MpPlatformWrapper.Services.CopyItemBuilder.CreateAsync(cd, false, IsAnyAppendMode && _appendModeCopyItem != null);

            MpConsole.WriteLine("CreateFromClipboardAsync: " + createItemSw.ElapsedMilliseconds + "ms");

            if (newCopyItem == null) {
                //this occurs if the copy item is not a known format or app init
                MpConsole.WriteTraceLine("Unable to create copy item from clipboard!");
                IsAddingClipboardItem = false;
                return;
            }

            bool isDup = newCopyItem.Id < 0;
            if(isDup) {
                MpConsole.WriteLine("Duplicate copy item detected, item: " + newCopyItem);
            }
            newCopyItem.Id = isDup ? -newCopyItem.Id : newCopyItem.Id;

            if (IsAppendMode || IsAppendLineMode) {
                if (isDup) {
                    //when duplicate copied in append mode treat item as new and don't unlink original 
                    isDup = false;
                    newCopyItem.Id = 0;
                    newCopyItem.CopyDateTime = DateTime.Now;
                    //await newCopyItem.WriteToDatabaseAsync();
                }
                if (_appendModeCopyItem != null && newCopyItem.Id > 0) {
                    Debugger.Break();
                }
                //when in append mode just append the new items text to selecteditem
                if (_appendModeCopyItem == null) {
                    if (_newModels != null &&
                       _newModels.Where(x => x.ItemType == MpCopyItemType.Text).Count() > 0 &&
                       (SelectedItem == null || SelectedItem.ItemType != MpCopyItemType.Text ||
                       (SelectedItem != null && _newModels.Where(x => x.ItemType == MpCopyItemType.Text).Max(x => x.CopyDateTime) > SelectedItem.LastSelectedDateTime))) {
                        // when new models are pending (to be added to pin tray) check if they are more
                        // recent then last selected item and prefer newest model when append is enabled
                        _appendModeCopyItem = _newModels.Where(x => x.ItemType == MpCopyItemType.Text).Aggregate((a, b) => a.CopyDateTime > b.CopyDateTime ? a : b);

                    } else if ((SelectedItem == null ||
                        (SelectedItem != null && (SelectedItem.ItemType != MpCopyItemType.Text || SelectedItem.LastSelectedDateTime < newCopyItem.CopyDateTime))) && newCopyItem.ItemType == MpCopyItemType.Text) {
                        // when no pending items are available and this item was created after last selected (and type is text)
                        _appendModeCopyItem = newCopyItem;
                    } else if (SelectedItem.ItemType == MpCopyItemType.Text) {
                        _appendModeCopyItem = SelectedItem.CopyItem;
                    }
                }
                if (_appendModeCopyItem != null && newCopyItem.ItemType == MpCopyItemType.Text) {
                    if (_appendModeCopyItem != newCopyItem) {

                        //var am_ctvm = GetClipTileViewModelById(_appendModeCopyItem.Id);
                        //if (am_ctvm != null) {
                        //    var am_cv = Application.Current.MainWindow.GetVisualDescendents<MpRtbContentView>().FirstOrDefault(x => x.DataContext == am_ctvm);

                        //    if (am_cv != null) {
                        //        am_cv.Rtb.Document = (MpEventEnabledFlowDocument)am_cv.Rtb.Document.Combine(
                        //        newCopyItem.ItemData.ToFlowDocument(), null, IsAppendLineMode);

                        //        await MpContentDocumentRtfExtension.SaveTextContent(am_cv.Rtb);
                        //    }
                        //} else {
                        //    var cfd = _appendModeCopyItem.ItemData.ToFlowDocument().Combine(newCopyItem.ItemData.ToFlowDocument(), null, IsAppendLineMode);
                        //    _appendModeCopyItem.ItemData = cfd.ToRichText();
                        //    await _appendModeCopyItem.WriteToDatabaseAsync();

                        //    int newIdx = _newModels.IndexOf(_newModels.FirstOrDefault(x => x.Id == _appendModeCopyItem.Id));
                        //    _newModels.RemoveAt(newIdx);
                        //    _newModels.Insert(newIdx, _appendModeCopyItem);
                        //}

                        //if (MpPrefViewModel.Instance.NotificationShowAppendBufferToast) {
                        //    // TODO now composite item doesn't roll up children so the buffer needs to be created here
                        //    // if I use this at all

                        //    MpNotificationBuilder.ShowMessageAsync(
                        //        title: "Append Buffer",
                        //        msg: _appendModeCopyItem.ItemData.ToPlainText(),
                        //        msgType: MpNotificationType.AppendBuffer)
                        //        .FireAndForgetSafeAsync(this);
                        //}

                        //if (MpPrefViewModel.Instance.NotificationDoCopySound) {
                        //    MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                        //}
                    }
                }
            } else {
                _appendModeCopyItem = null;
                if (MpPrefViewModel.Instance.NotificationDoCopySound) {
                    //MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                }
                if (MpPrefViewModel.Instance.IsTrialExpired) {
                    MpNotificationBuilder.ShowMessageAsync(
                        title: "Trial Expired",
                        msg: "Please update your membership to use Monkey Paste",
                        msgType: MpNotificationType.TrialExpired,
                        iconSourceStr: MpPrefViewModel.Instance.AbsoluteResourcesPath + @"/Images/monkey (2).png")
                        .FireAndForgetSafeAsync(this);
                }
            }
            if (isDup) {
                //item is a duplicate
                MpConsole.WriteLine("Duplicate item detected, incrementing copy count and updating copydatetime");
                newCopyItem.CopyCount++;
                // reseting CopyDateTime will move item to top of recent list
                newCopyItem.CopyDateTime = DateTime.Now;
                await newCopyItem.WriteToDatabaseAsync();

                var dup_ctvm = AllItems.FirstOrDefault(x => x.CopyItemId == newCopyItem.Id);
                if(dup_ctvm == null) {
                    // duplicate is not in the current query page
                    int dup_query_offset_idx = MpDataModelProvider.AvailableQueryCopyItemIds.IndexOf(newCopyItem.Id);
                    if(dup_query_offset_idx < 0) {
                        // duplicate is not in the current query at all so treat like a new model (pin tray or appeneded
                        _newModels.Add(newCopyItem);                        
                    } else {
                        // dup is in current query 
                        if(dup_query_offset_idx >= HeadQueryIdx && dup_query_offset_idx <= TailQueryIdx) {
                            // this shouldn't happen, dup_ctvm should of been found in page
                            Debugger.Break();
                        } else {
                            // remove dup from query since its pinning

                            MpDataModelProvider.AvailableQueryCopyItemIds.RemoveAt(dup_query_offset_idx);
                            if (HeadQueryIdx > dup_query_offset_idx) {
                                // pinning dup alters current pages offsets so down tick them...

                                IsBatchOffsetChange = true;
                                Items.ForEach(x => x.QueryOffsetIdx--);
                                IsBatchOffsetChange = false;
                                RefreshLayout();
                            } else {
                                // dup is after current page so no need to adjust this page's offsets
                            }

                            _newModels.Add(newCopyItem);
                        }
                    }
                } else {
                    // dup is in current query page or already pinned
                    if(dup_ctvm.IsPinned) {
                        // if already pinned move to head and select
                        int dup_pin_idx = PinnedItems.IndexOf(dup_ctvm);
                        
                        if(dup_pin_idx < 0) {
                            // shouldn't happen
                            Debugger.Break();
                        } else if(dup_pin_idx > 0){
                            // if 0 then its where it should be
                            PinnedItems.Move(dup_pin_idx, 0);
                        }
                        //reacquire tile sents moving can do funky thinks to vm references
                        dup_ctvm = PinnedItems.FirstOrDefault(x => x.CopyItemId == newCopyItem.Id);
                        SelectedItem = dup_ctvm;
                    } else {
                        // dup is in query page 
                        // to stay out of ui flip id back to negative and AddNewCommand will notice (or Append does abs val) and call toggle pin on window open
                        newCopyItem.Id *= -1;
                        _newModels.Add(newCopyItem);
                    }
                }
            } else if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                if (newCopyItem.Id != 0) {
                    _newModels.Add(newCopyItem);
                }

                MpAvTagTrayViewModel.Instance.AllTagViewModel.TagClipCount++;

               
            }
            if (IsAppendMode) {
                AppendNewItemsCommand.Execute(null);
            } else {
                AddNewItemsCommand.Execute(null);
            }

            
            IsAddingClipboardItem = false;

            OnCopyItemAdd?.Invoke(this, newCopyItem);

            //MpAvTagTrayViewModel.Instance.AllTagViewModel.NotifyAllTagItemLinked(newCopyItem);

            totalAddSw.Stop();
            MpConsole.WriteLine("Time to create new copyitem: " + totalAddSw.ElapsedMilliseconds + " ms");


        }

        #region Sync Events

        #endregion

        #endregion

        #region Commands

        public ICommand PinTileCommand => new MpAsyncCommand<object>(
             async (args) => {
                 int pin_idx = 0;// PinnedItems.Count;
                 MpAvClipTileViewModel pctvm = null;
                 if (args is MpAvClipTileViewModel) {
                     pctvm = args as MpAvClipTileViewModel;
                     if(pctvm.IsPinned || pctvm.IsPlaceholder) {
                         MpConsole.WriteTraceLine("PinTile error, tile is either already pinned or placeholder");
                         return;
                     }
                 } else if (args is object[] argParts) {
                     pctvm = argParts[0] as MpAvClipTileViewModel;
                     pin_idx = (int)argParts[1];
                 }
                 MpDataModelProvider.AvailableQueryCopyItemIds.Remove(pctvm.CopyItemId);

                 Items.Remove(pctvm);
                 Items.Where(x => x.QueryOffsetIdx > pctvm.QueryOffsetIdx).ForEach(x => x.QueryOffsetIdx = x.QueryOffsetIdx - 1);
                 pctvm.QueryOffsetIdx = -1;
                 if(pin_idx == PinnedItems.Count) {
                     PinnedItems.Add(pctvm);
                 } else {
                     PinnedItems.Insert(pin_idx, pctvm);
                 }

                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 pctvm.OnPropertyChanged(nameof(pctvm.IsPinned));
                 pctvm.OnPropertyChanged(nameof(pctvm.IsPlaceholder));

                 

                 MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                 await Task.Delay(100);
                 while(IsAnyBusy) {
                     await Task.Delay(100);
                 }
                 RefreshLayout();
                 await Task.Delay(200);
                 SelectedItem = pctvm;


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

                 PinnedItems.Remove(upctvm);
                 OnPropertyChanged(nameof(IsAnyTilePinned));

                 if (!IsAnyTilePinned) {
                     PinTrayTotalWidth = ObservedPinTrayScreenWidth = 0;
                 }
                 int queryIdx = MpDataModelProvider.AvailableQueryCopyItemIds.FastIndexOf(upctvm.CopyItemId);
                 if(queryIdx > 0) {
                     // if unpinned tile is part of current query find nearest previous unpinned idx
                     int prevAvailIdx = queryIdx - 1;
                     while(prevAvailIdx >= 0) {
                         int prevId = MpDataModelProvider.AvailableQueryCopyItemIds[prevAvailIdx];
                         if(PinnedItems.All(x=>x.CopyItemId != prevId)) {
                             // prevId is nearest previous unpinned item
                             // get it's query offset
                             prevAvailIdx = MpDataModelProvider.AvailableQueryCopyItemIds.FastIndexOf(prevId);
                             break;
                         }
                         prevAvailIdx--;
                     }
                     queryIdx = prevAvailIdx + 1;
                 }
                 if(queryIdx >= 0) {
                     SetScrollAnchor();

                     // when unpinned tile is part of current query add to available at right idx
                     MpDataModelProvider.AvailableQueryCopyItemIds.Insert(queryIdx, upctvm.CopyItemId);
                     // offset items after unpinned item 

                     IsUnpinning = true;
                     Items.Where(x => x.QueryOffsetIdx >= queryIdx)
                          .ForEach(x => x.QueryOffsetIdx = x.QueryOffsetIdx + 1);


                     if (queryIdx >= HeadQueryIdx - 1 && queryIdx <= TailQueryIdx + 1) {
                         // when unpinned tile is part of current visible part of query
                         Items.Add(upctvm);
                         upctvm.QueryOffsetIdx = queryIdx;
                     } else if(queryIdx < HeadQueryIdx) {
                         // when unpinned item comes before current visible update positions
                     }

                     IsUnpinning = false;

                     MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                     await Task.Delay(100);
                     while (IsAnyBusy) {
                         await Task.Delay(100);
                     }
                     upctvm = Items.FirstOrDefault(x => x.CopyItemId == upctvm.CopyItemId);
                     
                 }

                 RefreshLayout();
                 OnPropertyChanged(nameof(Items));
                 OnPropertyChanged(nameof(PinnedItems));
                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 OnPropertyChanged(nameof(MinPinTrayScreenWidth));
                 OnPropertyChanged(nameof(MaxPinTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayScreenHeight));

                 LockScrollToAnchor();
                 var ctvm_to_select = AllItems.FirstOrDefault(x => x.CopyItemId == unpinnedId);
                 if (ctvm_to_select != default) {
                     SelectedItem = ctvm_to_select;

                     //OnPropertyChanged(nameof(SelectedPinTrayItem));
                     //OnPropertyChanged(nameof(SelectedClipTrayItem));
                 }

                 //upctvm.OnPropertyChanged(nameof(upctvm.IsPinned));
                 //upctvm.OnPropertyChanged(nameof(upctvm.IsPlaceholder));

                 // if (upctvm.QueryOffsetIdx >= 0) {
                 //    //resultTile = await CreateClipTileViewModel(pctvm.Items.Select(x => x.CopyItem).ToList(), pctvm.QueryOffsetIdx);
                 //    if (upctvm.QueryOffsetIdx >= HeadQueryIdx && upctvm.QueryOffsetIdx <= TailQueryIdx) {
                 //        var insertBeforeItem = Items.Aggregate((a, b) => a.QueryOffsetIdx > b.QueryOffsetIdx && a.QueryOffsetIdx < upctvm.QueryOffsetIdx ? a : b);
                 //        if (insertBeforeItem == null) {
                 //            Items.Add(upctvm);
                 //        } else {
                 //            int insertIdx = Items.IndexOf(insertBeforeItem);
                 //            Items.Insert(insertIdx, upctvm);
                 //        }
                 //    }
                 //} else {
                 //     // either the pinned tile is not part of this query or its new or was added to query while pinned
                 //     // so for now requery because unpinning seems to mismatch view/view model...
                 //     QueryCommand.Execute(ScrollOffset);
                 //     while(IsAnyBusy) {
                 //         await Task.Delay(100);
                 //     }
                 // }
                 //QueryCommand.Execute(ScrollOffsetX);
                 //while (IsAnyBusy) {
                 //    await Task.Delay(100);
                 //}

                 //Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));

                 //OnPropertyChanged(nameof(ClipTrayScreenWidth));
                 //OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                 //OnPropertyChanged(nameof(ClipTrayScreenWidth));
                 //OnPropertyChanged(nameof(ClipTrayTotalWidth));
                 //OnPropertyChanged(nameof(MaxScrollOffsetX));
                 //OnPropertyChanged(nameof(MaxScrollOffsetY));

                 //upctvm = GetClipTileViewModelById(unpinnedId);
                 //if (upctvm != null) {
                 //    upctvm.IsSelected = true;
                 //}
             },
            (args) => args != null && args is MpAvClipTileViewModel ctvm && ctvm.IsPinned);

        public ICommand ToggleTileIsPinnedCommand => new MpCommand<object>(
            (args) => {
                MpAvClipTileViewModel pctvm = null;
                if(args is MpAvClipTileViewModel) {
                    pctvm = args as MpAvClipTileViewModel;
                } else if(args is object[] argParts) {
                    pctvm = argParts[0] as MpAvClipTileViewModel;
                }

                if (pctvm.IsPinned) {
                    UnpinTileCommand.Execute(args);
                } else {
                    PinTileCommand.Execute(args);
                }
            },
            (args) => args != null);


        public ICommand DuplicateSelectedClipsCommand => new MpCommand(
            async () => {
                IsBusy = true;
                var clonedCopyItem = (MpCopyItem)await SelectedItem.CopyItem.Clone(true);

                await clonedCopyItem.WriteToDatabaseAsync();
                _newModels.Add(clonedCopyItem);

                AddNewItemsCommand.Execute(true);

                IsBusy = false;
            },()=> SelectedItem != null);

        public ICommand AppendNewItemsCommand => new MpCommand(
            async () => {
                IsBusy = true;
                // note abs id val if ClipboardChanged flagged this as a dup item (maybe negative of real id)
                var amctvm = GetClipTileViewModelById((int)Math.Abs(_appendModeCopyItem.Id));
                if (amctvm != null) {
                    await amctvm.InitializeAsync(amctvm.CopyItem, amctvm.QueryOffsetIdx);
                    _appendModeCopyItem = amctvm.CopyItem;
                }

                IsBusy = false;
            },
            ()=>_appendModeCopyItem != null);

        public ICommand AddNewItemsCommand => new MpCommand(
            async () => {
                int selectedId = -1;
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLocked && SelectedItem != null) {
                    selectedId = SelectedItem.CopyItemId;
                }
                for (int i = 0; i < _newModels.Count; i++) {
                    var ci = _newModels[i];
                    MpAvClipTileViewModel nctvm = null;
                    if(ci.Id < 0) {
                        // special case for dup in current query page, grab tile instead of create
                        nctvm = Items.FirstOrDefault(x => x.CopyItemId == -ci.Id);
                        if(nctvm == null) {
                            // something went wrong...will jsut creat instead
                            Debugger.Break();
                            nctvm = await CreateClipTileViewModel(ci);
                        }
                    } else {
                        nctvm = await CreateClipTileViewModel(ci);
                    }
                    while (nctvm.IsAnyBusy) {
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
            () => {
                if (_newModels.Count == 0) {
                    return false;
                }
                //if(!string.IsNullOrEmpty(MpSearchBoxViewModel.Instance.LastSearchText)) {
                //    return false;
                //}
                //if (MpDataModelProvider.QueryInfo.SortType == MpContentSortType.Manual) {
                //    return false;
                //}
                if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    return true;
                }
                return false;
            });

        public ICommand QueryCommand => new MpAsyncCommand<object>(
            async (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) => {
                if (!Dispatcher.UIThread.CheckAccess()) {
                    Dispatcher.UIThread.Post(() => QueryCommand.Execute(offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg));
                    return;
                }

                IsBusy = IsRequery = true;
                var sw = new Stopwatch();
                sw.Start();

                bool isSubQuery = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg != null;
                bool isScrollJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is MpPoint;
                bool isOffsetJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is int;
                bool isLoadMore = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is bool;
                bool isRequery = !isSubQuery;

                int loadOffsetIdx = 0;
                int loadCount = 0;

                bool isLoadMoreTail = false;

                MpPoint newScrollOffset = default;

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
                        loadOffsetIdx = FindJumpTileIdx(newScrollOffset.X,newScrollOffset.Y, out MpRect offsetTileRect);
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
                    }

                } else {
                    // new query all content and offsets are re-initialized

                    ClearClipSelection();
                    MpDataModelProvider.ResetQuery();

                    await MpDataModelProvider.QueryForTotalCountAsync(PinnedItems.Select(x=>x.CopyItemId));

                    Items.Clear();
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

                // make list of select idx's
                List<int> fetchQueryIdxList = Enumerable.Range(loadOffsetIdx, loadCount).ToList();
                if (fetchQueryIdxList.Count > 0) {
                    if (!isLoadMore) {
                        // Cleanup Tray item count depending on last query
                        int itemCountDiff = Items.Count - fetchQueryIdxList.Count;
                        if (itemCountDiff > 0) {
                            while (itemCountDiff > 0) {
                                Items.RemoveAt(0);
                                itemCountDiff--;
                            }
                        } else if (itemCountDiff < 0) {
                            while (itemCountDiff < 0) {
                                var ctvm = await CreateClipTileViewModel(null);
                                Items.Add(ctvm);
                                itemCountDiff++;
                            }
                        }
                    }

                    var cil = await MpDataModelProvider.FetchCopyItemsByQueryIdxListAsync(fetchQueryIdxList);

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
                        if(cur_ctvm == null || (isLoadMore && cur_ctvm.IsAnyCornerVisible)) {
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
                    //await Task.WhenAll(initTasks.ToArray());
                    foreach (var initTask in initTasks) {
                        initTask.FireAndForgetSafeAsync(this);
                    }
                }

                //while (Items.Any(x => x.IsAnyBusy)) {
                //    await Task.Delay(100);
                //}

                if (isRequery) {
                    if (SelectedItem == null &&
                        MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Count == 0 &&
                        TotalTilesInQuery > 0) {
                        ResetClipSelection();
                    }
                }

                OnPropertyChanged(nameof(TotalTilesInQuery));
                OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                OnPropertyChanged(nameof(ClipTrayTotalWidth));
                OnPropertyChanged(nameof(MaxScrollOffsetX));
                OnPropertyChanged(nameof(MaxScrollOffsetY));
                OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                OnPropertyChanged(nameof(IsVerticalScrollBarVisible));
                OnPropertyChanged(nameof(IsEmpty));

                if (Items.Count == 0) {
                    ScrollOffsetX = LastScrollOffsetX = ScrollOffsetY = LastScrollOffsetY = 0;
                }

                IsBusy = IsRequery = false;

                if (isRequery) {
                    //_scrollOffset = LastScrollOffsetX = 0;
                    //ForceScrollOffset(MpPoint.Zero);
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.RequeryCompleted);
                } else if (isOffsetJump || isScrollJump) {
                    ForceScrollOffset(newScrollOffset);
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.JumpToIdxCompleted);
                } 


                sw.Stop();
                MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);

                if (isLoadMore) {
                    //recheck loadMore once done for rejected scroll change events
                    while(IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    CheckLoadMore();
                } 
            },
            (offsetIdx_Or_ScrollOffset_Arg) => {
                return !IsAnyBusy && !IsRequery;
            });

        public ICommand ExcludeSubSelectedItemApplicationCommand => new MpAsyncCommand(
            async () => {
                var avm = MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == SelectedItem.AppViewModel.AppId);
                if (avm == null) {
                    return;
                }
                await avm.RejectApp();
            },
            ()=>SelectedItem != null);

        public ICommand ExcludeSubSelectedItemUrlDomainCommand => new MpAsyncCommand(
            async () => {
                var uvm = MpAvUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == SelectedItem.UrlViewModel.UrlId);
                if (uvm == null) {
                    MpConsole.WriteTraceLine("Error cannot find url id: " + SelectedItem.UrlViewModel.UrlId);
                    return;
                }
                await uvm.RejectUrlOrDomain(true);
            },
            () => SelectedItem != null && SelectedItem.UrlViewModel != null);

        public ICommand SearchWebCommand => new MpCommand<object>(
            (args) => {
                string pt = string.Join(
                            Environment.NewLine,
                            MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));

                //MpHelpers.OpenUrl(args.ToString() + Uri.EscapeDataString(pt));
            }, (args) => args != null && args is string);

        

        public ICommand SelectAllCommand => new MpCommand(
            () => {
                ClearClipSelection();
                foreach (var ctvm in Items) {
                    ctvm.IsSelected = true;
                }
            });

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

        public ICommand CopySelectedClipsCommand => new MpAsyncCommand(
            async () => {
                MpPlatformWrapper.Services.ClipboardMonitor.IgnoreClipboardChanges = true;
                var mpdo = await SelectedItem.GetContentView().Document.GetDataObjectAsync(true, false); //.ConvertToDataObject(false);
                await MpPlatformWrapper.Services.DataObjectHelperAsync.SetPlatformClipboardAsync(mpdo);

                // wait extra for cb watcher to know about data
                await Task.Delay(300);
                MpPlatformWrapper.Services.ClipboardMonitor.IgnoreClipboardChanges = false;
            }, ()=>SelectedItem != null);

        public ICommand PasteSelectedClipsCommand => new MpAsyncCommand<object>(
            async (args) => {
                
                SelectedItem.IsPasting = true;

                MpAvDataObject mpdo = await SelectedItem.GetContentView().Document.GetDataObjectAsync(false,true);

                await Task.Delay(100);
                var pi = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;
                await MpPlatformWrapper.Services.ExternalPasteHandler.PasteDataObject(
                    mpdo, pi);

                CleanupAfterPaste(SelectedItem);
            },
            (args) => {
                bool isFromEditorButton = args != null ? (bool)args : false;

                return MpAvMainWindowViewModel.Instance.IsShowingDialog == false &&
                        SelectedItem != null &&
                        MpAvMainWindowViewModel.Instance.IsMainWindowActive &&
                        (isFromEditorButton || (
                            !isFromEditorButton &&
                            !IsAnyEditingClipTile &&
                            !IsAnyEditingClipTitle &&
                            !IsAnyPastingTemplate)) &&
                        !MpPrefViewModel.Instance.IsTrialExpired;
            });

        public ICommand PasteCurrentClipboardIntoSelectedTileCommand => new MpAsyncCommand(
            async () => {
                while (IsAddingClipboardItem) {
                    // wait in case tray is still processing the data
                    await Task.Delay(100);
                }

                // NOTE even though re-creating paste object here the copy item
                // builder should recognize it as a duplicate and use original (just created)
                var mpdo = await MpPlatformWrapper.Services.DataObjectHelperAsync.GetPlatformClipboardDataObjectAsync();

                SelectedItem.RequestPastePortableDataObject(mpdo);
            }, ()=>SelectedItem != null && !SelectedItem.IsPlaceholder);

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

        public ICommand BringSelectedClipTilesToFrontCommand => new MpCommand(
            () => {

            });

        public ICommand SendSelectedClipTilesToBackCommand => new MpCommand(
             () => {
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
                return MpAvMainWindowViewModel.Instance.IsShowingDialog == false &&
                        MpAvMainWindowViewModel.Instance.IsMainWindowActive &&
                        SelectedModels.Count > 0 &&
                        !IsAnyEditingClipTile &&
                        !IsAnyEditingClipTitle &&
                        !IsAnyPastingTemplate;
            });

        public ICommand ToggleLinkTagToCopyItemCommand => new MpAsyncCommand<MpAvTagTileViewModel>(
            async (ttvm) => {
                var ctvm = SelectedItem;
                bool isUnlink = await ttvm.IsCopyItemLinkedAsync(ctvm.CopyItemId);

                if (isUnlink) {
                    // NOTE item is removed from ui from db ondelete event
                    ttvm.UnlinkCopyItemCommand.Execute(ctvm.CopyItemId);
                } else {
                    ttvm.LinkCopyItemCommand.Execute(ctvm.CopyItemId);
                }
                while(ttvm.IsBusy) {
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

        public ICommand EnableFilterByAppCommand => new MpCommand<object>(
            (targetCtvmArg) => {
                var targetCtvm = targetCtvmArg as MpAvClipTileViewModel;
                if(targetCtvm == null) {
                    return;
                }

                //MpHelpers.OpenUrl(CopyItem.Source.App.AppPath);
                ClearClipSelection();
                targetCtvm.IsSelected = true;
                //this triggers clip tray to swap out the app icons for the filtered app
                //MpClipTrayViewModel.Instance.FilterByAppIcon = ctvm.CopyItem.Source.PrimarySource.SourceIcon.IconImage.ImageBase64.ToBitmapSource();
                IsFilteringByApp = true;
            }, (targetCtvmArg) => {
                return targetCtvmArg is MpAvClipTileViewModel tctvm && !tctvm.IsPinned;
            });
        public ICommand AssignHotkeyCommand => new MpCommand(
            () => {
                MpAvShortcutCollectionViewModel.Instance.ShowAssignShortcutDialogCommand.Execute(SelectedItem);
            },
            () => SelectedItem != null);

        public ICommand InvertSelectionCommand => new MpCommand(
            () => {
            });

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

        public ICommand SendToEmailCommand => new MpCommand(
            () => {
                // for gmail see https://stackoverflow.com/a/60741242/105028
                string pt = string.Join(Environment.NewLine, MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                //MpHelpers.OpenUrl(
                //    string.Format("mailto:{0}?subject={1}&body={2}",
                //    string.Empty, SelectedItem.CopyItem.Title,
                //    pt));
                //MpAvClipTrayViewModel.Instance.ClearClipSelection();
                //IsSelected = true;
                //MpHelpers.CreateEmail(MpJsonPreferenceIO.Instance.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
            },
            () => {
                return !IsAnyEditingClipTile && SelectedItem != null;
            });

        public ICommand MergeSelectedClipsCommand => new MpCommand(
            () => {
                SelectedItem.RequestMerge();
            },
            () => SelectedItem != null);

        //public ICommand SummarizeCommand => new MpCommand(
        //    async () => {
        //        var result = await MpOpenAi.Instance.Summarize(SelectedModels[0].ItemData.ToPlainText());
        //        SelectedModels[0].ItemDescription = result;
        //        await SelectedModels[0].WriteToDatabaseAsync();
        //    },
        //    () => SelectedItem != null && SelectedItem.IsTextItem);

        public ICommand CreateQrCodeFromSelectedClipsCommand => new MpCommand(
             () => {
                 Dispatcher.UIThread.InvokeAsync(() => {
                     //BitmapSource bmpSrc = null;
                     string pt = string.Join(Environment.NewLine, MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                     //bmpSrc = MpHelpers.ConvertUrlToQrCode(pt);
                     //MpClipboardHelper.MpClipboardManager.SetDataObjectWrapper(
                     //    new MpDataObject() {
                     //        DataFormatLookup = new Dictionary<MpClipboardFormatType, string>() { 
                     //            { 
                     //                MpClipboardFormatType.Bitmap, 
                     //                bmpSrc.ToBase64String() 
                     //            } 
                     //        }
                     //});
                 });
             },
            () => {
                string pt = string.Join(Environment.NewLine, MpAvPersistentClipTilePropertiesHelper.PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                return (GetSelectedClipsType() == MpCopyItemType.Text) &&
                    pt.Length <= MpPrefViewModel.Instance.MaxQrCodeCharLength;
            });

        public ICommand SpeakSelectedClipsCommand => new MpAsyncCommand(
            async () => {
                await Task.Delay(1);
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
            },
            () => {
                return SelectedItem != null && SelectedItem.IsTextItem;
            });

        public ICommand AnalyzeSelectedItemCommand => new MpAsyncCommand<object>(
            async (presetIdObj) => {
                var preset = await MpDb.GetItemAsync<MpPluginPreset>((int)presetIdObj);
                var analyticItemVm = MpAnalyticItemCollectionViewModel.Instance.Items.FirstOrDefault(x => x.PluginGuid == preset.PluginGuid);
                var presetVm = analyticItemVm.Items.FirstOrDefault(x => x.Preset.Id == preset.Id);

                analyticItemVm.SelectPresetCommand.Execute(presetVm);
                analyticItemVm.ExecuteAnalysisCommand.Execute(null);
            });

        public ICommand ToggleIsAppPausedCommand => new MpCommand(
            () => {
                IsAppPaused = !IsAppPaused;
            });

        public ICommand ToggleRightClickPasteCommand => new MpCommand(
            () => {
                IsRightClickPasteMode = !IsRightClickPasteMode;
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("RIGHT CLICK PASTE MODE: {0}", IsRightClickPasteMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, ()=>!IsAppPaused);

        public ICommand ToggleAutoCopyModeCommand => new MpCommand(
            () => {
                IsAutoCopyMode = !IsAutoCopyMode;
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("AUTO-COPY SELECTION MODE: {0}", IsAutoCopyMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);

        public ICommand ToggleAppendModeCommand => new MpCommand(
            () => {
                IsAppendMode = !IsAppendMode;
                if (IsAppendMode && IsAppendLineMode) {
                    ToggleAppendLineModeCommand.Execute(null);
                }
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("APPEND MODE: {0}", IsAppendMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);

        public ICommand ToggleAppendLineModeCommand => new MpCommand(
            () => {
                IsAppendLineMode = !IsAppendLineMode;
                if (IsAppendLineMode && IsAppendMode) {
                    ToggleAppendModeCommand.Execute(null);
                }
                MpNotificationBuilder.ShowMessageAsync("MODE CHANGED", string.Format("APPEND LINE MODE: {0}", IsAppendLineMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);

        public ICommand FindAndReplaceSelectedItem => new MpCommand(
            () => {
                //SelectedItem.ToggleFindAndReplaceVisibleCommand.Execute(null);
            }, () => SelectedItem != null && !SelectedItem.IsFindAndReplaceVisible && SelectedItem.IsTextItem);
        #endregion
        #endregion
    }
}
