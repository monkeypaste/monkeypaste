using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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

    public class MpAvClipTrayViewModel : MpSelectorViewModelBase<object, MpAvClipTileViewModel>,
        MpIBootstrappedItem, 
        MpIPagingScrollViewerViewModel,
        MpIActionComponent,
        MpIContextMenuViewModel {
        #region Private Variables

        #endregion

        #region Statics

        private static MpAvClipTrayViewModel _instance;
        public static MpAvClipTrayViewModel Instance => _instance ?? (_instance = new MpAvClipTrayViewModel());

        #endregion

        #region Properties

        #region MpIBoostrappedItem Implementation

        string MpIBootstrappedItem.Label => "Content Tray";
        #endregion

        #region View Models
        // NOTE have to override ObservableCollection from pcl because of .netcore issue w/ module
        public override ObservableCollection<MpAvClipTileViewModel> Items { get => base.Items; set => base.Items = value; }
        #endregion

        public int RowCount {
            get {
                if(IsEmpty) {
                    return 0;
                }
                if (LayoutType == MpAvClipTrayLayoutType.Stack) {
                    if(ListOrientation == Orientation.Horizontal) {
                        return 1;
                    }
                    return Items.Count;
                }
                //double totalFlatWidth = Items.Sum(x => x.MinSize);
                //int rowCount = (int)Math.Floor(totalFlatWidth / ClipTrayScreenWidth);
                //return rowCount;
                int rowCount = (int)Math.Ceiling((double)Items.Count / (double)ColCount);
                return rowCount;
            }
        }

        public int ColCount {
            get {
                if(IsEmpty) {
                    return 0;
                }

                if (LayoutType == MpAvClipTrayLayoutType.Stack) {
                    if (ListOrientation == Orientation.Horizontal) {
                        return Items.Count;
                    }
                    return 1;
                }
                int colCount = (int)Math.Max(1.0d,Math.Floor(ClipTrayScreenWidth / Items.First().MinSize));
                return colCount;
            }
        }

        #region MpIPagingScrollViewer Implementation

        public Orientation ListOrientation => MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ? Orientation.Horizontal : Orientation.Vertical;

        public double ScrollOffsetX { get; set; }
        public double ScrollOffsetY { get; set; }
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

        public double ClipTrayTotalTileWidth {
            get {
                if(IsEmpty) {
                    return 0;
                }
                double totalTileWidth = ColCount * Items.First().MinSize;
                return totalTileWidth;
            }
        }
        public double ClipTrayTotalTileHeight {
            get {
                if(IsEmpty) {
                    return 0;
                }
                double totalTileHeight = RowCount * Items.First().MinSize;
                return totalTileHeight;
            }
        }

        public double ClipTrayTotalWidth => Math.Max(ClipTrayScreenWidth, ClipTrayTotalTileWidth);
        public double ClipTrayTotalHeight => Math.Max(ClipTrayScreenHeight, ClipTrayTotalTileHeight);

        public double ClipTrayScreenWidth { get; set; }

        public double ClipTrayScreenHeight { get; set; }

        public double ZoomFactor { get; set; } = 1;
        public double ScrollVelocityX { get; set; }
        public double ScrollVelocityY { get; set; }

        public bool CanScroll {
            get {
                return true;

                if (MpAvMainWindowViewModel.Instance.IsMainWindowOpening ||
                   !MpAvMainWindowViewModel.Instance.IsMainWindowOpen ||
                    IsRequery ||
                   IsScrollingIntoView) {
                    return false;
                }
                if (SelectedItem == null) {
                    return true;
                }
                if (SelectedItem.IsVerticalScrollbarVisibile &&
                    SelectedItem.IsHovering &&
                    SelectedItem.IsVisible) {
                    return false;
                }
                return true;
            }
        }
        public bool IsThumbDragging { get; set; } = false;


        public Size HorizontalScrollBarDesiredSize { get; set; }

        public Size VerticalScrollBarDesiredSize { get; set; }

        #endregion

        #region Layout


        #endregion

        #region Appearance

        public ScrollBarVisibility HorizontalScrollBarVisibility {
            get {
                return ScrollBarVisibility.Visible;
                //return ClipTrayTotalTileWidth > ClipTrayScreenWidth ?
                //        ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility {
            get {
                return ScrollBarVisibility.Visible;
                //return ClipTrayTotalTileHeight > ClipTrayScreenHeight ?
                //        ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
            }
        }

        public MpAvClipTrayLayoutType LayoutType { get; set; } = MpAvClipTrayLayoutType.Stack;

        #endregion

        #region State

        public bool IsEmpty => Items.Count == 0;

        public bool HasScrollVelocity => Math.Abs(ScrollVelocityX) + Math.Abs(ScrollVelocityY) > 0.1d;

        public bool IsScrollingIntoView { get; set; }

        public bool IsGridLayout { get; set; }        

        public bool IsRequery { get; set; } = false;

        #endregion


        #endregion

        #region Constructors

        private MpAvClipTrayViewModel() : base() {
            PropertyChanged += MpAvClipTrayViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public override string ToString() {
            return $"ClipTray";
        }
        #endregion

        #region Private Methods

        private async Task<MpAvClipTileViewModel> CreateClipTileViewModel(MpCopyItem ci, int queryOffsetIdx = -1) {
            MpAvClipTileViewModel ctvm = new MpAvClipTileViewModel(this);
            await ctvm.InitializeAsync(ci, queryOffsetIdx);
            return ctvm;
        }


        private void MpAvClipTrayViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //MpConsole.WriteLine($"Name: {e.PropertyName} Value: {this.GetPropertyValue(e.PropertyName)?.ToString()}");
            switch (e.PropertyName) {
                case nameof(Items):
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(CanScroll));
                    break;
                case nameof(IsGridLayout):
                    ToggleLayoutTypeCommand.Execute(null);
                    break;
                case nameof(ZoomFactor):
                case nameof(LayoutType):
                case nameof(ClipTrayScreenWidth):
                case nameof(ClipTrayScreenHeight):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.MinSize)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayY)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.RowIdx)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.ColIdx)));

                    OnPropertyChanged(nameof(ClipTrayTotalHeight));
                    OnPropertyChanged(nameof(ClipTrayTotalWidth));

                    OnPropertyChanged(nameof(MaxScrollOffsetX));
                    OnPropertyChanged(nameof(MaxScrollOffsetY));

                    OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                    OnPropertyChanged(nameof(ClipTrayTotalTileHeight));
                    break;
                case nameof(ClipTrayTotalTileWidth):
                    OnPropertyChanged(nameof(MaxScrollOffsetX));
                    OnPropertyChanged(nameof(MaxScrollOffsetY));
                    break;
                case nameof(ScrollOffsetX):
                case nameof(ScrollOffsetY):
                    if (IsThumbDragging) {
                        break;
                    }
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayScrollChanged);
                    break;
                case nameof(HasScrollVelocity):
                    if (!HasScrollVelocity) {
                        var hctvm = Items.FirstOrDefault(x => x.IsHovering);
                        if (hctvm != null) {
                            hctvm.OnPropertyChanged(nameof(hctvm.TileBorderBrush));
                        }
                    }
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TrayScrollChanged:

                    break;
                case MpMessageType.MainWindowOrientationChanged:
                    OnPropertyChanged(nameof(ListOrientation));
                    break;
                case MpMessageType.TrayLayoutChanged:
                case MpMessageType.MainWindowSizeReset:
                    ResetZoomFactorCommand.Execute(null);
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ToggleLayoutTypeCommand => new MpCommand(
            () => {
                ScrollToHomeCommand.Execute(null);

                if (IsGridLayout) {
                    LayoutType = MpAvClipTrayLayoutType.Grid;
                } else {
                    LayoutType = MpAvClipTrayLayoutType.Stack;
                }
                MpMessenger.SendGlobal(MpMessageType.TrayLayoutChanged);
            });

        public ICommand ScrollToHomeCommand => new MpCommand(
            () => {
                ScrollOffsetX = 0;
                ScrollOffsetY = 0;
            });

        public ICommand ScrollToEndCommand => new MpCommand(
            () => {
                ScrollOffsetX = MaxScrollOffsetX;
                ScrollOffsetY = MaxScrollOffsetY;
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

        private int _pageSize = 0;

        private Dictionary<int, int> _manualSortOrderLookup = null;


        private MpCopyItem _currentClipboardItem;

        #endregion

        #region Constants

        public const int DISABLE_READ_ONLY_DELAY_MS = 500;

        #endregion

        #region Properties

        #region View Models

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
        public MpAvClipTileViewModel HeadItem => Items.Count > 0 ? Items[0] : null;

        public MpAvClipTileViewModel TailItem => Items.Count > 0 ? Items[Items.Count - 1] : null;

        public override MpAvClipTileViewModel SelectedItem {
            get {
                if (PinnedItems.Any(x => x.IsSelected)) {
                    return PinnedItems.FirstOrDefault(x => x.IsSelected);
                }
                return Items.FirstOrDefault(x => x.IsSelected);
            }
            set {
                if (SelectedItem != value && value != null) {
                    if (value != null && PinnedItems.Any(x => x.CopyItemId == value.CopyItemId)) {
                        PinnedItems.ForEach(x => x.IsSelected = x.CopyItemId == value.CopyItemId);
                        Items.ForEach(x => x.IsSelected = false);
                    } else if (value != null && Items.Any(x => x.CopyItemId == value.CopyItemId)) {
                        Items.ForEach(x => x.IsSelected = x.CopyItemId == value.CopyItemId);
                        PinnedItems.ForEach(x => x.IsSelected = false);
                    } else {
                        SelectedItem = null;
                    }

                }
                OnPropertyChanged(nameof(SelectedItem));
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
                var dragItem = Items.FirstOrDefault(x => x.IsItemDragging);
                if (dragItem == null) {
                    return PinnedItems.FirstOrDefault(x => x.IsItemDragging);
                }
                return dragItem;
            }
        }

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
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("CopyIcon") as string,
                            Command = CopySelectedClipsCommand,
                            ShortcutType = MpShortcutType.CopySelection
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Paste",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PasteIcon") as string,
                            Command = PasteSelectedClipsCommand,
                            ShortcutType = MpShortcutType.PasteSelectedItems
                        },
                        new MpMenuItemViewModel() {
                            Header = @"Paste _Here",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PasteIcon") as string,
                            Command = PasteCurrentClipboardIntoSelectedTileCommand,
                            ShortcutType = MpShortcutType.PasteSelectedItems
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },

                        new MpMenuItemViewModel() {
                            Header = @"_Delete",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DeleteIcon") as string,
                            Command = DeleteSelectedClipsCommand,
                            ShortcutType = MpShortcutType.DeleteSelectedItems
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Rename",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("RenameIcon") as string,
                            Command = EditSelectedTitleCommand,
                            ShortcutType = MpShortcutType.EditTitle
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Edit",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("EditContentIcon") as string,
                            Command = EditSelectedContentCommand,
                            ShortcutType = MpShortcutType.EditContent
                        },
                        new MpMenuItemViewModel() {
                            IsSeparator = true
                        },
                        new MpMenuItemViewModel() {
                            Header = @"_Transform",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("ToolsIcon") as string,
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = @"_Find and Replace",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SearchIcon") as string,
                                    Command = FindAndReplaceSelectedItem,
                                    ShortcutType = MpShortcutType.FindAndReplaceSelectedItem
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Duplicate",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DuplicateIcon") as string,
                                    Command = DuplicateSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.Duplicate
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Merge",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("MergeIcon") as string,
                                    Command = MergeSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.MergeSelectedItems
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Email",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("EmailIcon") as string,
                                    Command = SendToEmailCommand,
                                    ShortcutType = MpShortcutType.SendToEmail
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Qr Code",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("QrIcon") as string,
                                    Command = CreateQrCodeFromSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.CreateQrCode
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Audio",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SpeakIcon") as string,
                                    Command = SpeakSelectedClipsCommand,
                                    ShortcutType = MpShortcutType.SpeakSelectedItem
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Web Search",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("WebIcon") as string,
                                    SubItems = new List<MpMenuItemViewModel>() {
                                        new MpMenuItemViewModel() {
                                            Header = "_Google",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("GoogleIcon") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.google.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "_Bing",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("BingIcon") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://www.bing.com/search?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "_DuckDuckGo",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("DuckGo") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://duckduckgo.com/?q="
                                        },
                                        new MpMenuItemViewModel() {
                                            Header = "_Yandex",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("YandexIcon") as string,
                                            Command = SearchWebCommand,
                                            CommandParameter=@"https://yandex.com/search/?text="
                                        },
                                        new MpMenuItemViewModel() { IsSeparator = true},
                                        new MpMenuItemViewModel() {
                                            Header = "_Manage...",
                                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("CogIcon") as string
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
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("RobotClawIcon") as string,
                                    Command = MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand,
                                    CommandParameter = this
                                },
                                new MpMenuItemViewModel() {
                                    Header = "To _Shorcut",
                                    ShortcutType = MpShortcutType.PasteCopyItem,
                                    ShortcutObjId = SelectedItem.CopyItemId,
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("HotkeyIcon") as string,
                                    Command = MpSystemTrayViewModel.Instance.ShowSettingsWindowCommand,
                                    CommandParameter = this
                                },
                            }
                        },
                        MpAnalyticItemCollectionViewModel.Instance.MenuItemViewModel,
                        new MpMenuItemViewModel() {
                            Header = "_Select",
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SelectionIcon") as string,
                            SubItems = new List<MpMenuItemViewModel>() {
                                new MpMenuItemViewModel() {
                                    Header = "_Bring to Front",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("BringToFrontIcon") as string,
                                    Command = BringSelectedClipTilesToFrontCommand,
                                    ShortcutType = MpShortcutType.BringSelectedToFront
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Send to Back",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SendToBackIcon") as string,
                                    Command = SendSelectedClipTilesToBackCommand,
                                    ShortcutType = MpShortcutType.SendSelectedToBack
                                },
                                new MpMenuItemViewModel() {
                                    Header = "Select _All",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("SelectAllIcon") as string,
                                    Command = SelectAllCommand,
                                    ShortcutType = MpShortcutType.SelectAll
                                },
                                new MpMenuItemViewModel() {
                                    Header = "_Invert Selection",
                                    IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("InvertSelectionIcon") as string,
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
                            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("PinToCollectionIcon") as string,
                            SubItems = tagItems
                        }
                    },
                };
            }
        }

        #endregion

        #endregion

        #region Layout

        public double PinTrayScreenWidth { get; set; }

        public double PinTrayTotalWidth { get; set; } = 0;

        public double MinPinTrayScreenWidth => IsAnyTilePinned ? MinClipTrayScreenWidth : 0;
        public double MinClipTrayScreenWidth => 30;

        public double MaxPinTrayWidth => ClipTrayScreenWidth - MinClipTrayScreenWidth;

        // NOTE ClipTrayScreenWidth is only set on initial load but then set by OneWayToSource Binding in MpClipTrayContainerView

        public double MaxTileWidth => ClipTrayScreenWidth - 50;// (MpMeasurements.Instance.ClipTileMaxWidthPadding * 2);

        #endregion

        #region Appearance

        public string PinTrayDropForegroundHexColor => IsDragOverPinTray ? MpSystemColors.Yellow : MpSystemColors.oldlace;
        public int MaxTotalVisibleClipTiles {
            get {
                return 6;// (int)Math.Ceiling(ClipTrayScreenWidth / MpMeasurements.Instance.ClipTileBorderMinWidth);
            }
        }

        #endregion

        #region Business Logic


        public int RemainingItemsCountThreshold { get; private set; }

        public int TotalTilesInQuery => MpDataModelProvider.TotalTilesInQuery;

        public int DefaultLoadCount => MaxTotalVisibleClipTiles + 3;


        #endregion

        #region State

        public bool HasUserAlteredPinTrayWidth { get; set; } = false;

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

        #region Virtual

        //set in civm IsSelected property change, DragDrop.Drop (copy mode)
        public List<MpCopyItem> PersistentSelectedModels { get; set; } = new List<MpCopyItem>();


        private Dictionary<int, double> _persistentUniqueWidthTileLookup { get; set; } = new Dictionary<int, double>();
        private Dictionary<int, double> _persistentUniqueWidthQueryOffsetLookup { get; set; } = new Dictionary<int, double>();
        //<CopyItemId, Unique ItemWidth> unique is != to MpMeausrements.Instance.ClipTileMinSize
        //public Dictionary<int, double> PersistentUniqueWidthTileLookup => _persistentUniqueWidthTileLookup;

        // THIS Lookup is passive and only changed when PersistentUniqueWidthTileLookup is changed
        //<QueryOffsetIdx, Unique ItemWidth> unique is != to MpMeausrements.Instance.ClipTileMinSize
        //public Dictionary<int, double> PersistentUniqueWidthQueryOffsetLookup => _persistentUniqueWidthQueryOffsetLookup;

        public void AddOrReplacePersistentWidthById(int ciid, double uniqueWidth) {
            _persistentUniqueWidthTileLookup.AddOrReplace(ciid, uniqueWidth);

            int queryOffset = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.FastIndexOf(ciid);
            if (queryOffset < 0) {
                return;
            }
            _persistentUniqueWidthQueryOffsetLookup.AddOrReplace(queryOffset, uniqueWidth);
        }

        public void RemovePersistentWidthById(int ciid) {
            if (_persistentUniqueWidthTileLookup.ContainsKey(ciid)) {
                _persistentUniqueWidthTileLookup.Remove(ciid);
            }
            int queryOffset = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.FastIndexOf(ciid);
            if (queryOffset < 0) {
                return;
            }
            _persistentUniqueWidthQueryOffsetLookup.Remove(queryOffset);
        }

        public bool TryGetByPersistentWidthById(int ciid, out double uniqueWidth) {
            return _persistentUniqueWidthTileLookup.TryGetValue(ciid, out uniqueWidth);
        }

        public void ClearPersistentWidths() {
            _persistentUniqueWidthQueryOffsetLookup.Clear();
            _persistentUniqueWidthTileLookup.Clear();
        }


        public int HeadQueryIdx => Items.Count == 0 ? -1 : Items.Where(x => x.QueryOffsetIdx >= 0).Min(x => x.QueryOffsetIdx);// Math.Max(0, Items.Min(x => x.QueryOffsetIdx));

        public int TailQueryIdx => Items.Count == 0 ? -1 : Items.Max(x => x.QueryOffsetIdx);// Math.Min(TotalTilesInQuery - 1, Items.Max(x => x.QueryOffsetIdx));

        public int MaxLoadQueryIdx => Math.Max(0, MaxClipTrayQueryIdx - DefaultLoadCount + 1);

        public int MaxClipTrayQueryIdx {
            get {
                int maxClipTrayQueryIdx = TotalTilesInQuery - 1;
                if (maxClipTrayQueryIdx < 0) {
                    return maxClipTrayQueryIdx;
                }
                while (PinnedItems.Any(x => x.QueryOffsetIdx == maxClipTrayQueryIdx)) {
                    if (maxClipTrayQueryIdx < 0) {
                        return maxClipTrayQueryIdx;
                    }
                    maxClipTrayQueryIdx--;
                }
                return maxClipTrayQueryIdx;
            }
        }

        public int MinClipTrayQueryIdx {
            get {
                if (TotalTilesInQuery == 0) {
                    return -1;
                }
                int minClipTrayQueryIdx = 0;

                while (PinnedItems.Any(x => x.QueryOffsetIdx == minClipTrayQueryIdx)) {
                    if (minClipTrayQueryIdx >= TotalTilesInQuery) {
                        return -1;
                    }
                    minClipTrayQueryIdx++;
                }
                return minClipTrayQueryIdx;
            }
        }
        #endregion

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
                    return MpPlatformWrapper.Services.PlatformResource.GetResource("BothClickIcon") as string;
                }
                if (IsRightClickPasteMode) {
                    return MpPlatformWrapper.Services.PlatformResource.GetResource("RightClickIcon") as string;
                }
                if (IsAutoCopyMode) {
                    return MpPlatformWrapper.Services.PlatformResource.GetResource("LeftClickIcon") as string;
                }
                return MpPlatformWrapper.Services.PlatformResource.GetResource("NoneClickIcon") as string;
            }
        }

        #endregion

        public bool IsAppPaused { get; set; } = false;

        public bool IsRestoringSelection { get; private set; } = false;

        public double LastScrollOffsetX { get; set; } = 0;

        public bool IsHorizontalScrollBarVisible => ClipTrayTotalTileWidth > ClipTrayScreenWidth; //TotalTilesInQuery > MpMeasurements.Instance.DefaultTotalVisibleClipTiles;


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

        #region Child Property Wrappers
        public bool IsAnyBusy => Items.Any(x => x.IsAnyBusy) || PinnedItems.Any(x => x.IsAnyBusy) || IsBusy;
        public bool IsAnyTileContextMenuOpened => Items.Any(x => x.IsContextMenuOpen) || PinnedItems.Any(x => x.IsContextMenuOpen);

        public bool IsAnyTileFlipped => Items.Any(x => x.IsFlipped || x.IsFlipping) || PinnedItems.Any(x => x.IsFlipped || x.IsFlipping);

        public bool IsAnyResizing => Items.Any(x => x.IsResizing) || PinnedItems.Any(x => x.IsResizing);

        public bool CanAnyResize => Items.Any(x => x.CanResize) || PinnedItems.Any(x => x.CanResize);

        public bool IsAnyEditing => Items.Any(x => !x.IsContentAndTitleReadOnly) || PinnedItems.Any(x => !x.IsContentAndTitleReadOnly);


        public bool IsAnyHovering => Items.Any(x => x.IsHovering) || PinnedItems.Any(x => x.IsHovering);


        public bool IsAnyEditingClipTitle => Items.Any(x => !x.IsTitleReadOnly) || PinnedItems.Any(x => !x.IsTitleReadOnly);

        public bool IsAnyEditingClipTile => Items.Any(x => !x.IsContentReadOnly) || PinnedItems.Any(x => !x.IsContentReadOnly);

        public bool IsAnyPastingTemplate => Items.Any(x => x.IsPastingTemplate) || PinnedItems.Any(x => x.IsPastingTemplate);

        public bool IsAnyItemDragging => Items.Any(x => x.IsItemDragging) ||
                                         PinnedItems.Any(x => x.IsItemDragging) ||
                                         MpAvDragDropManager.IsDraggingFromExternal;

        public bool IsAnyTilePinned => PinnedItems.Count > 0;

        public bool IsDragOverPinTray { get; set; }

        #endregion

        #endregion

        #region Visibility

        #endregion

        #endregion

        #region Events

        public event EventHandler<object> OnFocusRequest;
        public event EventHandler OnUiRefreshRequest;
        public event EventHandler<object> OnScrollIntoViewRequest;
        public event EventHandler<double> OnScrollToXRequest;
        public event EventHandler OnScrollToHomeRequest;

        public event EventHandler<MpCopyItem> OnCopyItemAdd;

        #endregion

        #region Constructors


        public async Task InitAsync() {
            IsBusy = true;

            while (MpSourceCollectionViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            PropertyChanged += MpAvClipTrayViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;

            MpDataModelProvider.AllFetchedAndSortedCopyItemIds.CollectionChanged += AllFetchedAndSortedCopyItemIds_CollectionChanged;

            MpDb.SyncAdd += MpDbObject_SyncAdd;
            MpDb.SyncUpdate += MpDbObject_SyncUpdate;
            MpDb.SyncDelete += MpDbObject_SyncDelete;

            _pageSize = 1;
            RemainingItemsCountThreshold = 1;
            _oldMainWindowHeight = MpAvMainWindowViewModel.Instance.MainWindowHeight;
            //DefaultLoadCount = MpMeasurements.Instance.DefaultTotalVisibleClipTiles * 1 + 2;

            MpMessenger.Register<MpMessageType>(
                MpDataModelProvider.QueryInfo, ReceivedQueryInfoMessage);

            MpMessenger.Register<MpMessageType>(
                nameof(MpAvDragDropManager), ReceivedDragDropManagerMessage);

            LogPropertyChangedEvents = false;


            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            for (int i = 1; i <= 10; i++) {
                var test_ctvm = await CreateClipTileViewModel(
                    new MpCopyItem() {
                        Id = i,
                        ItemType = MpCopyItemType.Text,
                        ItemData = "This is test " + i,
                        Title = "Test" + i
                    }, i - 1);
                Items.Add(test_ctvm);
            }

            SelectedItem = Items[0];

            OnPropertyChanged(nameof(Items));

            
            IsBusy = false;




        }

        private void AllFetchedAndSortedCopyItemIds_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            MpConsole.WriteLine("Query Ids Changed:");
            MpConsole.WriteLine("ACTION: " + e.Action);
            MpConsole.WriteLine("NEW ITEMS: " + string.Join(",", e.NewItems));
            MpConsole.WriteLine("OLD ITEMS: " + string.Join(",", e.OldItems));
            MpConsole.WriteLine("NEW START IDX: " + e.NewStartingIndex);
            MpConsole.WriteLine("OLD START IDX: " + e.OldStartingIndex);
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:

                    break;
            }
        }

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

        #region MpIClipboardContentDataProvider Implementation

        public async Task<string> GetClipboardContentData() {
            while (IsAddingClipboardItem) {
                try {
                    await Task.Delay(100).TimeoutAfter(TimeSpan.FromMilliseconds(3000));
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    return null;
                }
            }
            if (_currentClipboardItem == null) {
                return null;
            }
            return _currentClipboardItem.ItemData;
        }

        #endregion

        #region View Invokers
        public void RequestScrollToX(double xoffset) {
            OnScrollToXRequest?.Invoke(this, xoffset);
        }

        public void RequestScrollIntoView(object obj) {
            OnScrollIntoViewRequest?.Invoke(this, obj);
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
            Dispatcher.UIThread.Post((Action)(() => {
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

                PersistentSelectedModels.Clear();
            }));
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

            if (Items.Count > 0 && Items[0] != null) {

                Items[0].IsSelected = true;
                //if (!MpSearchBoxViewModel.Instance.IsTextBoxFocused) {
                //    RequestFocus(SelectedItems[0]);
                //}
            }
            RequestScrollToHome();

            //});
            IsSelectionReset = false;
        }

        public void UnFlipAllTiles() {
            // TODO make async and do Unflip here
            foreach (var ctvm in Items) {
                if (ctvm.IsFlipped) {
                    FlipTileCommand.Execute(ctvm);
                }
            }
        }

        public void RefreshAllCommands() {
            foreach (MpAvClipTileViewModel ctvm in Items) {
                ctvm.RefreshAsyncCommands();
            }
        }


        public void ClipboardChanged(object sender, MpPortableDataObject mpdo) {
            if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading || IsAppPaused) {
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
            MpMessenger.SendGlobal(MpMessageType.TraySelectionChanged);
        }

        public void StoreSelectionState(MpAvClipTileViewModel tile) {
            if (!tile.IsSelected) {
                return;
            }

            PersistentSelectedModels = new List<MpCopyItem>() { tile.CopyItem };
        }

        public void RestoreSelectionState(MpAvClipTileViewModel tile) {
            var prevSelectedItems = PersistentSelectedModels
                                        .Where(y => y.Id == tile.CopyItemId).ToList();
            if (prevSelectedItems.Count == 0) {
                tile.ClearSelection();
                return;
            }

            IsRestoringSelection = true;

            SelectedItem = tile;

            IsRestoringSelection = false;
        }
        //public double FindTileOffsetX2(int queryOffsetIdx) {
        //    int totalTileCount = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Count;
        //    if (totalTileCount <= 0) {
        //        return 0;
        //    }
        //    queryOffsetIdx = Math.Max(0, Math.Min(queryOffsetIdx, totalTileCount - 1));

        //    var headItemIds = MpDataModelProvider.AllFetchedAndSortedCopyItemIds;

        //    double offsetX = 0;
        //    for (int i = 1; i <= queryOffsetIdx; i++) {
        //        int tileHeadId = headItemIds[i - 1];
        //        if (PinnedItems.Any(x => x.CopyItemId == tileHeadId)) {
        //            continue;
        //        }
        //        offsetX += MpMeasurements.Instance.ClipTileMargin * 2;

        //        if (TryGetByPersistentWidthById(tileHeadId, out double uniqueWidth)) {
        //            offsetX += uniqueWidth;
        //        } else {
        //            offsetX += MpAvClipTileViewModel.DefaultBorderWidth;

        //        }
        //    }
        //    return offsetX;
        //}
        public double FindTileOffsetX(int queryOffsetIdx) {
            // get count of all items in current query that can affect offset
            //int totalTileCount = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Count;
            //if (totalTileCount <= 0 || queryOffsetIdx <= 0) {
            //    // offset 0 will always be at x = 0
            //    return 0;
            //}

            //var item = Items.FirstOrDefault(x => x.QueryOffsetIdx == queryOffsetIdx && x.CopyItemTitle == "Untitled2636");

            //// ensure the check is clamped to within the count
            //queryOffsetIdx = Math.Max(0, Math.Min(queryOffsetIdx, totalTileCount - 1));

            //var relevantUniqueWidthLookup = _persistentUniqueWidthQueryOffsetLookup
            //                                    .Where(x =>
            //                                        PinnedItems.All(y => y.QueryOffsetIdx != x.Key) &&
            //                                        x.Key < queryOffsetIdx)
            //                                    .ToDictionary(k => k.Key, v => v.Value);

            //int uniqueWidthCount = relevantUniqueWidthLookup.Count;

            //int relevantPinnedDefaultWidthCount = PinnedItems.Where(x => x.QueryOffsetIdx >= 0 && x.QueryOffsetIdx < queryOffsetIdx).Count();
            //double uniqueWidthSum = (uniqueWidthCount * (MpMeasurements.Instance.ClipTileMargin * 2)) + relevantUniqueWidthLookup.Sum(x => x.Value);

            //int defaultWidthCount = Math.Max(0, queryOffsetIdx - uniqueWidthCount - relevantPinnedDefaultWidthCount);

            //double defaultItemWidthWithMargin = (MpMeasurements.Instance.ClipTileMargin * 2) + MpAvClipTileViewModel.DefaultBorderWidth;
            //double defaultWidthSum = defaultWidthCount * defaultItemWidthWithMargin;

            //double offsetX = uniqueWidthSum + defaultWidthSum;
            //return offsetX;
            return 0;
        }


        public int FindJumpTileIdx(double trackValue) {

            //int totalTileCount = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.Count;
            //var headItemIds = MpDataModelProvider.AllFetchedAndSortedCopyItemIds;

            //double offsetX = 0;
            //for (int i = 0; i < totalTileCount; i++) {
            //    if (PinnedItems.Any(x => x.QueryOffsetIdx == i)) {
            //        continue;
            //    }
            //    offsetX += MpMeasurements.Instance.ClipTileMargin;


            //    if (_persistentUniqueWidthQueryOffsetLookup.TryGetValue(i, out double uniqueWidth)) {
            //        offsetX += uniqueWidth;
            //    } else {
            //        offsetX += MpAvClipTileViewModel.DefaultBorderWidth;
            //    }

            //    if (offsetX >= trackValue) {
            //        return i;
            //    }
            //    offsetX += MpMeasurements.Instance.ClipTileMargin;
            //}

            //return totalTileCount - 1;
            return 0;
        }

        public void AdjustScrollOffsetToResize(double oldHeadTrayX, double oldScrollOfset) {
            double oldScrollOffsetDiffWithHead = oldScrollOfset - oldHeadTrayX;

            double newHeadTrayX = HeadItem == null ? 0 : HeadItem.TrayX;
            double headOffsetRatio = newHeadTrayX / oldHeadTrayX;
            headOffsetRatio = double.IsNaN(headOffsetRatio) ? 1 : headOffsetRatio;
            double newScrollOfsetDiffWithHead = headOffsetRatio * oldScrollOffsetDiffWithHead;
            double newScrollOfset = FindTileOffsetX(HeadQueryIdx) + newScrollOfsetDiffWithHead;

            //if(newScrollOfset < 100 || Math.Abs(newScrollOfset - oldScrollOfset) > 200) {
            //    Debugger.Break();
            //}
            ScrollOffsetX = newScrollOfset;
        }

        public void ForceScrollOffset(double newOffset) {
            //_scrollOffset = LastScrollOffsetX = newOffset;
            OnPropertyChanged(nameof(ScrollOffsetX));
        }


        public void CleanupAfterPaste(MpAvClipTileViewModel sctvm) {
            IsPasting = false;
            //clean up pasted items state after paste

            sctvm.IsPasting = false;
            if (sctvm.HasTemplates) {
                sctvm.ClearEditing();
                sctvm.TemplateCollection.Reset();
                sctvm.TemplateRichText = string.Empty;
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
                PersistentSelectedModels.Remove(ci);
                RemovePersistentWidthById(ci.Id);
                MpDataModelProvider.RemoveQueryItem(ci.Id);

                var removed_ctvm = GetClipTileViewModelById(ci.Id);
                if (removed_ctvm != null) {
                    bool wasSelected = removed_ctvm.IsSelected;

                    if (removed_ctvm.IsPinned) {
                        var pctvm = PinnedItems.FirstOrDefault(x => x.CopyItemId == ci.Id);


                        if (pctvm != null) {
                            int pinIdx = PinnedItems.IndexOf(pctvm);
                            // Flag QueryOffsetIdx = -1 so it tray doesn't attempt to return it to tray
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
                            if (TryGetByPersistentWidthById(ctvm.CopyItemId, out double uniqueWidth)) {
                                _persistentUniqueWidthQueryOffsetLookup.Remove(ctvm.QueryOffsetIdx + 1);
                                _persistentUniqueWidthQueryOffsetLookup.Add(ctvm.QueryOffsetIdx, uniqueWidth);
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
                bool isAssociated = await ttvm.IsLinkedAsync(ctvm);
                if (isAssociated) {
                    return;
                }
                MpDataModelProvider.RemoveQueryItem(cit.CopyItemId);
                MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
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

                    var svm = MpSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == ci.SourceId);

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
            if (e.OldItems != null) { //if (e.Action == NotifyCollectionChangedAction.Move && IsLoadingMore) {
                foreach (MpAvClipTileViewModel octvm in e.OldItems) {
                    octvm.Dispose();
                }
            }
        }

        private double _oldMainWindowHeight = 0;

        

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

        public void OnPostMainWindowLoaded() {
            return;
            //int totalItems = MpAvTagTrayViewModel.Instance.AllTagViewModel.TagClipCount;

            //MpAvSystemTrayViewModel.Instance.TotalItemCountLabel = string.Format(@"{0} total entries", totalItems);



            //MpPlatformWrapper.Services.ClipboardMonitor.OnClipboardChanged += ClipboardChanged;
            //MpPlatformWrapper.Services.ClipboardMonitor.StartMonitor();
            ////await Task.Delay(3000);

            //if (!string.IsNullOrEmpty(MpPrefViewModel.Instance.LastQueryInfoJson)) {
            //    var qi = JsonConvert.DeserializeObject<MpWpfQueryInfo>(MpPrefViewModel.Instance.LastQueryInfoJson);
            //    if (qi != null) {
            //        MpClipTileSortViewModel.Instance.SelectedSortType =
            //            MpClipTileSortViewModel.Instance.SortTypes
            //                .FirstOrDefault(x => x.SortType == qi.SortType);
            //        MpClipTileSortViewModel.Instance.IsSortDescending = qi.IsDescending;

            //        MpAvTagTrayViewModel.Instance.SelectTagCommand.Execute(qi.TagId);

            //        MpSearchBoxViewModel.Instance.SearchText = qi.SearchText;
            //        // NOTE Filter flags already set from Preferences

            //        MpPlatformWrapper.Services.QueryInfo = qi;


            //        MpDataModelProvider.Init();
            //    }
            //}
            //MpAvMainWindowViewModel.Instance.IsMainWindowLoading = false;


            //MpDataModelProvider.QueryInfo.NotifyQueryChanged(true);
        }


        private void ReceivedQueryInfoMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.QueryChanged:
                    QueryCommand.Execute(null);
                    break;
                case MpMessageType.SubQueryChanged:
                    QueryCommand.Execute(HeadQueryIdx);
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
                    OnPropertyChanged(nameof(IsAnyItemDragging));
                    break;
            }
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


            var newCopyItem = await MpAvCopyItemBuilder.CreateFromDataObject(cd, IsAnyAppendMode && _appendModeCopyItem != null);

            MpConsole.WriteLine("CreateFromClipboardAsync: " + createItemSw.ElapsedMilliseconds + "ms");

            if (newCopyItem == null) {
                //this occurs if the copy item is not a known format or app init
                MpConsole.WriteTraceLine("Unable to create copy item from clipboard!");
                IsAddingClipboardItem = false;
                return;
            }

            bool isDup = newCopyItem.Id < 0;
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

                        //    MpNotificationCollectionViewModel.Instance.ShowMessageAsync(
                        //        title: "Append Buffer",
                        //        msg: _appendModeCopyItem.ItemData.ToPlainText(),
                        //        msgType: MpNotificationDialogType.AppendBuffer)
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
                    MpNotificationCollectionViewModel.Instance.ShowMessageAsync(
                        title: "Trial Expired",
                        msg: "Please update your membership to use Monkey Paste",
                        msgType: MpNotificationDialogType.TrialExpired,
                        iconResourceKey: MpPrefViewModel.Instance.AbsoluteResourcesPath + @"/Images/monkey (2).png")
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
            } else if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                if (newCopyItem.Id != 0) {
                    _newModels.Add(newCopyItem);
                }

                MpAvTagTrayViewModel.Instance.AllTagViewModel.TagClipCount++;

                if (IsAppendMode) {
                    AppendNewItemsCommand.Execute(null);
                } else {
                    AddNewItemsCommand.Execute(null);
                }
            }

            while (IsBusy) {
                await Task.Delay(100);
            }
            if (_appendModeCopyItem != null) {
                _currentClipboardItem = _appendModeCopyItem;
            } else {
                _currentClipboardItem = newCopyItem;
            }
            IsAddingClipboardItem = false;

            OnCopyItemAdd?.Invoke(this, newCopyItem);

            MpAvTagTrayViewModel.Instance.AllTagViewModel.NotifyAllTagItemLinked(newCopyItem);

            totalAddSw.Stop();
            MpConsole.WriteLine("Time to create new copyitem: " + totalAddSw.ElapsedMilliseconds + " ms");


        }

        #region Sync Events

        #endregion

        #endregion

        #region Commands

        public ICommand PinTileCommand => new MpAsyncCommand<object>(
             async (args) => {
                 var pctvm = args as MpAvClipTileViewModel;

                 var trayItemToRemove = Items.FirstOrDefault(x => x.CopyItemId == pctvm.CopyItemId);
                 if (trayItemToRemove != null) {
                     //swap to-be-pinned item w/ a new placeholder
                     Items.Remove(trayItemToRemove);
                     var sub_tray_ctvm = await CreateClipTileViewModel(null);
                     Items.Add(sub_tray_ctvm);

                 }

                 if (pctvm.QueryOffsetIdx >= 0) {
                     // only recreate tiles that are already in the tray (they won't be if new)
                     pctvm = await CreateClipTileViewModel(pctvm.CopyItem, pctvm.QueryOffsetIdx);
                 }
                 PinnedItems.Add(pctvm);
                 pctvm.OnPropertyChanged(nameof(pctvm.IsPinned));
                 pctvm.OnPropertyChanged(nameof(pctvm.IsPlaceholder));

                 Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));

                 OnPropertyChanged(nameof(Items));
                 OnPropertyChanged(nameof(PinnedItems));
                 OnPropertyChanged(nameof(IsAnyTilePinned));
                 OnPropertyChanged(nameof(ClipTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                 OnPropertyChanged(nameof(ClipTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayTotalWidth));
                 OnPropertyChanged(nameof(MaxScrollOffsetX));
                 OnPropertyChanged(nameof(MaxScrollOffsetY));

                 //SelectedItem = pctvm;
                 pctvm.IsSelected = true;
             },
            (args) =>
            args != null &&
            args is MpAvClipTileViewModel ctvm &&
            !ctvm.IsPinned &&
            !ctvm.IsPlaceholder);

        public ICommand UnpinTileCommand => new MpAsyncCommand<object>(
             async (args) => {
                 var upctvm = args as MpAvClipTileViewModel;
                 int unpinnedId = upctvm.CopyItemId;

                 PinnedItems.Remove(upctvm);
                 OnPropertyChanged(nameof(IsAnyTilePinned));

                 if (!IsAnyTilePinned) {
                     PinTrayTotalWidth = PinTrayScreenWidth = 0;
                 }

                 upctvm.OnPropertyChanged(nameof(upctvm.IsPinned));
                 upctvm.OnPropertyChanged(nameof(upctvm.IsPlaceholder));

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
                 QueryCommand.Execute(ScrollOffsetX);
                 while (IsAnyBusy) {
                     await Task.Delay(100);
                 }

                 Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));

                 OnPropertyChanged(nameof(ClipTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                 OnPropertyChanged(nameof(ClipTrayScreenWidth));
                 OnPropertyChanged(nameof(ClipTrayTotalWidth));
                 OnPropertyChanged(nameof(MaxScrollOffsetX));
                 OnPropertyChanged(nameof(MaxScrollOffsetY));

                 upctvm = GetClipTileViewModelById(unpinnedId);
                 if (upctvm != null) {
                     upctvm.IsSelected = true;
                 }
             },
            (args) => args != null && args is MpAvClipTileViewModel ctvm && ctvm.IsPinned);

        public ICommand ToggleTileIsPinnedCommand => new MpCommand<object>(
            (args) => {
                var pctvm = args as MpAvClipTileViewModel;
                if (pctvm.IsPinned) {
                    UnpinTileCommand.Execute(args);
                } else {
                    PinTileCommand.Execute(args);
                }
            },
            (args) => args != null && args is MpAvClipTileViewModel);


        public ICommand DuplicateSelectedClipsCommand => new MpCommand(
            async () => {
                IsBusy = true;
                var clonedCopyItem = (MpCopyItem)await SelectedItem.CopyItem.Clone(true);

                await clonedCopyItem.WriteToDatabaseAsync();
                _newModels.Add(clonedCopyItem);

                AddNewItemsCommand.Execute(true);

                IsBusy = false;
            });

        public ICommand AppendNewItemsCommand => new MpCommand(
            async () => {
                IsBusy = true;

                var amctvm = GetClipTileViewModelById(_appendModeCopyItem.Id);
                if (amctvm != null) {
                    await amctvm.InitializeAsync(amctvm.CopyItem, amctvm.QueryOffsetIdx);
                }

                IsBusy = false;
            },
            ()=>_appendModeCopyItem != null);

        public ICommand AddNewItemsCommand => new MpCommand(
            async () => {
                int selectedId = -1;
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLocked) {
                    selectedId = SelectedItem.CopyItemId;
                }
                for (int i = 0; i < _newModels.Count; i++) {
                    var ci = _newModels[i];
                    var nctvm = await CreateClipTileViewModel(ci);
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
                //MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayScrollChanged);
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
                bool isScrollJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is double;
                bool isOffsetJump = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is int;
                bool isLoadMore = offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is bool;

                int loadOffsetIdx = 0;
                double newScrollOffset = default;
                int loadCount = 0;

                if (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg != null) {
                    // sub-query of visual, data-specific or incremental offset 

                    if (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is int) {
                        // sub-query to data-specific (Id) offset
                        loadOffsetIdx = (int)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg;
                    } else if (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is double) {
                        // sub-query to visual (scroll position) offset 
                        newScrollOffset = (double)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg;
                        loadOffsetIdx = FindJumpTileIdx(newScrollOffset);
                    } else if (offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg is bool) {
                        // sub-query either forward (true) or backward (false) based on current offset
                        newScrollOffset = ScrollOffsetX;

                        loadCount = _pageSize;

                        if ((bool)offsetIdx_Or_ScrollOffset_Or_AddToTail_Arg) {
                            //load More to tail
                            loadOffsetIdx = TailQueryIdx + 1;
                            if (loadOffsetIdx > MaxClipTrayQueryIdx) {
                                IsRequery = IsBusy = false;
                                return;
                            }
                        } else {
                            //load more to head
                            loadOffsetIdx = HeadQueryIdx - 1;
                            if (loadOffsetIdx < MinClipTrayQueryIdx) {
                                IsRequery = IsBusy = false;
                                return;
                            }
                        }
                    }

                    if (loadOffsetIdx + DefaultLoadCount > MaxClipTrayQueryIdx) {
                        // clamp load offset to max query total count
                        loadOffsetIdx = MaxLoadQueryIdx;
                    }
                } else {
                    // new query all content and offsets are re-initialized

                    newScrollOffset = 0;
                    ClearClipSelection();
                    MpDataModelProvider.ResetQuery();

                    await MpDataModelProvider.RequeryAsync(
                        0,
                        MaxTileWidth,
                        0);// MpMeasurements.Instance.ClipTileMargin * 2);



                    Items.Clear();
                    ClearPersistentWidths();

                    OnPropertyChanged(nameof(TotalTilesInQuery));
                    OnPropertyChanged(nameof(ClipTrayTotalWidth));
                    OnPropertyChanged(nameof(MaxScrollOffsetX));
                    OnPropertyChanged(nameof(MaxScrollOffsetY));
                    OnPropertyChanged(nameof(IsHorizontalScrollBarVisible));
                }

                if (loadCount == 0) {
                    // is not an LoadMore Query
                    loadCount = Math.Min(DefaultLoadCount, TotalTilesInQuery);
                } else if (loadOffsetIdx < 0) {
                    loadCount = 0;
                }

                // make list of select idx's
                List<int> fetchQueryIdxList = Enumerable.Range(loadOffsetIdx, loadCount).ToList();
                if (fetchQueryIdxList.Count > 0) {
                    //clean up pinned items if requerying and not present in this query
                    PinnedItems.ForEach(x => x.QueryOffsetIdx = MpDataModelProvider.AllFetchedAndSortedCopyItemIds.FastIndexOf(x.CopyItemId));

                    foreach (var pctvm in PinnedItems.Where(x => x.QueryOffsetIdx >= 0)) {
                        //when pinned item is part of this fetch remove and next to
                        //tail if available or prev to head so load count is constant
                        int pinLoadOffsetIdx = fetchQueryIdxList.FastIndexOf(pctvm.QueryOffsetIdx);
                        if (pinLoadOffsetIdx < 0) {
                            continue;
                        }
                        fetchQueryIdxList.RemoveAt(pinLoadOffsetIdx);

                        if (fetchQueryIdxList.Count == 0) {
                            IsBusy = IsRequery = false;
                            return;
                        }
                        int newTailFetchIdx = fetchQueryIdxList.Last() + 1;
                        while (newTailFetchIdx <= MaxClipTrayQueryIdx && PinnedItems.Any(x => x.QueryOffsetIdx == newTailFetchIdx)) {
                            newTailFetchIdx++;
                        }
                        if (newTailFetchIdx > MaxClipTrayQueryIdx) {
                            int newHeadFetchIdx = fetchQueryIdxList.First() - 1;
                            while (newHeadFetchIdx >= MinClipTrayQueryIdx && PinnedItems.Any(x => x.QueryOffsetIdx == newHeadFetchIdx)) {
                                newHeadFetchIdx--;
                            }
                            if (newHeadFetchIdx >= MinClipTrayQueryIdx) {
                                fetchQueryIdxList.Insert(0, newHeadFetchIdx);
                            }
                        } else {
                            fetchQueryIdxList.Add(newTailFetchIdx);
                        }
                    }
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

                    for (int i = 0; i < cil.Count; i++) {
                        if (isLoadMore) {
                            int loadMoreSwapIdx_from = fetchQueryIdxList[i] > TailQueryIdx ? 0 : Items.Count - 1;
                            int loadMoreSwapIdx_to = fetchQueryIdxList[i] > TailQueryIdx ? Items.Count - 1 : 0;

                            if (Items[loadMoreSwapIdx_from].IsSelected) {
                                StoreSelectionState(Items[loadMoreSwapIdx_from]);
                                Items[loadMoreSwapIdx_from].ClearSelection();
                            }
                            Items.Move(loadMoreSwapIdx_from, loadMoreSwapIdx_to);
                            await Items[loadMoreSwapIdx_to].InitializeAsync(cil[i], fetchQueryIdxList[i]);

                            RestoreSelectionState(Items[loadMoreSwapIdx_to]);
                        } else {

                            await Items[i].InitializeAsync(cil[i], fetchQueryIdxList[i]);

                            if (isSubQuery) {
                                RestoreSelectionState(Items[i]);
                            }

                        }
                    }
                }

                while (Items.Any(x => x.IsAnyBusy)) {
                    await Task.Delay(100);
                }

                if (isSubQuery) {
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
                } else {
                    HasUserAlteredPinTrayWidth = false;

                    if (SelectedItem == null &&
                        PersistentSelectedModels.Count == 0 &&
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
                if (Items.Count == 0) {
                    ScrollOffsetX = LastScrollOffsetX = 0;
                }
                if (!isSubQuery) {
                    //_scrollOffset = LastScrollOffsetX = 0;
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.RequeryCompleted);

                } else if (isOffsetJump) {
                    //_scrollOffset = LastScrollOffsetX = FindTileOffsetX(HeadQueryIdx);
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.JumpToIdxCompleted);
                } else if (isScrollJump) {
                    //_scrollOffset = LastScrollOffsetX = Math.Min(MaxScrollOffsetX, newScrollOffset);
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.JumpToIdxCompleted);
                }
                OnPropertyChanged(nameof(ScrollOffsetX));

                IsBusy = IsRequery = false;
                sw.Stop();
                MpConsole.WriteLine($"Update tray of {Items.Count} items took: " + sw.ElapsedMilliseconds);
            },
            (offsetIdx_Or_ScrollOffset_Arg) => !IsAnyBusy && !IsRequery);

        public ICommand FlipTileCommand => new MpCommand<object>(
            async (tileToFlip) => {
                var ctvm = tileToFlip as MpAvClipTileViewModel;
                ctvm.IsBusy = true;
                if (ctvm.IsFlipped) {
                    //ClearClipSelection();
                    //ctvm.IsSelected = true;
                    ctvm.IsFlipping = true;
                    while (ctvm.IsFlipping) {
                        await Task.Delay(100);
                    }
                } else {
                    var flippedCtvm = Items.FirstOrDefault(x => x.IsFlipped);
                    if (flippedCtvm != null) {
                        flippedCtvm.IsFlipping = true;
                        while (flippedCtvm.IsFlipping) {
                            await Task.Delay(100);
                        }
                    }
                    ClearClipSelection();
                    ctvm.IsFlipping = true;
                    while (ctvm.IsFlipping) {
                        await Task.Delay(100);
                    }
                    ctvm.IsSelected = true;
                    //await ctvm.PrimaryItem.AnalyticItemCollectionViewModel.Init();
                    //ctvm.IsFlipping = true;
                }

                ctvm.IsBusy = false;
            });

        public ICommand ExcludeSubSelectedItemApplicationCommand => new MpAsyncCommand(
            async () => {
                var avm = MpAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == SelectedItem.AppViewModel.AppId);
                if (avm == null) {
                    return;
                }
                await avm.RejectApp();
            },
            ()=>SelectedItem != null);

        public ICommand ExcludeSubSelectedItemUrlDomainCommand => new MpAsyncCommand(
            async () => {
                var uvm = MpUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == SelectedItem.UrlViewModel.UrlId);
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
                            PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));

                //MpHelpers.OpenUrl(args.ToString() + Uri.EscapeDataString(pt));
            }, (args) => args != null && args is string);

        //public ICommand ScrollToHomeCommand => new MpCommand(
        //     () => {
        //         QueryCommand.Execute(0d);
        //     },
        //    () => ScrollOffset > 0 && !IsAnyBusy);

        //public ICommand ScrollToEndCommand => new MpCommand(
        //    () => {
        //        QueryCommand.Execute(MaximumScrollOfset);
        //    },
        //    () => ScrollOffset < MaximumScrollOfset && !IsAnyBusy);

        public ICommand ScrollToNextPageCommand => new MpCommand(
             () => {
                 //int nextPageOffset = Math.Min(TotalTilesInQuery - 1, TailQueryIdx + 1);
                 //JumpToQueryIdxCommand.Execute(nextPageOffset);
                 double nextPageOffset = Math.Min(ScrollOffsetX + ClipTrayScreenWidth, MaxScrollOffsetX);
                 QueryCommand.Execute(nextPageOffset);
                 //await Task.Delay(100);
                 //while (IsAnyBusy) { await Task.Delay(10); }
                 //if(Items.Where(x=>!x.IsPlaceholder).Count() == 0) {
                 //    return;
                 //}
                 //Items[0].IsSelected = true;                 
             },
            () => ScrollOffsetX < MaxScrollOffsetX && !IsAnyBusy);

        public ICommand ScrollToPreviousPageCommand => new MpAsyncCommand(
            async () => {
                //int prevPageOffset = Math.Max(0, HeadQueryIdx - 1);
                //JumpToQueryIdxCommand.Execute(prevPageOffset);
                //await Task.Delay(100);
                //while (IsScrollJumping) { await Task.Delay(10); }
                ////ScrollOffset = LastScrollOfset = ClipTrayTotalWidth;
                //if (Items.Where(x => !x.IsPlaceholder).Count() == 0) {
                //    return;
                //}
                //Items[0].IsSelected = true;
                double prevPageOffset = Math.Max(0, ScrollOffsetX - ClipTrayScreenWidth);
                QueryCommand.Execute(prevPageOffset);
            },
            () => ScrollOffsetX > 0 && !IsAnyBusy);


        public ICommand SelectNextItemCommand => new MpAsyncCommand(
            async () => {
                IsArrowSelecting = true;

                bool needJump = false;
                int curRightMostSelectQueryIdx = -1;
                int nextSelectQueryIdx = -1;

                if (SelectedItem != null) {
                    curRightMostSelectQueryIdx = SelectedItem.QueryOffsetIdx;
                    nextSelectQueryIdx = curRightMostSelectQueryIdx + 1;

                } else if (PersistentSelectedModels.Count > 0) {
                    needJump = true;
                    curRightMostSelectQueryIdx = PersistentSelectedModels.
                        Select(x =>
                            MpDataModelProvider.AllFetchedAndSortedCopyItemIds.IndexOf(x.Id))
                                .Max();
                    nextSelectQueryIdx = curRightMostSelectQueryIdx + 1;
                } else if (SelectedItem == null) {
                    nextSelectQueryIdx = 0;
                } else {
                    // should be caught by CanExecute
                    Debugger.Break();
                }


                if (nextSelectQueryIdx < 0) {
                    // selected item is in a different query so select query head
                    nextSelectQueryIdx = 0;
                }
                while (PinnedItems.Any(x => x.QueryOffsetIdx == nextSelectQueryIdx)) {
                    nextSelectQueryIdx++;
                }
                if (nextSelectQueryIdx <= MaxClipTrayQueryIdx) {
                    if (needJump) {
                        QueryCommand.Execute(nextSelectQueryIdx);
                    } else if (nextSelectQueryIdx > TailQueryIdx) {
                        //QueryCommand.Execute(true);                        
                        ScrollOffsetX = Math.Min(MaxScrollOffsetX, ScrollOffsetX + 0.1);
                    }

                    while (IsAnyBusy) { await Task.Delay(100); }

                    var nctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == nextSelectQueryIdx);
                    if (nctvm != null) {
                        //ClearClipSelection(false);
                        //nctvm.IsSelected = true;
                        int idx = Items.IndexOf(nctvm);
                        ClearClipSelection(false);
                        Items[idx].ResetSubSelection();
                        StoreSelectionState(Items[idx]);
                    }
                }
                await Task.Delay(100);
                IsArrowSelecting = false;
            },
            () => !IsAnyBusy && !IsArrowSelecting &&
                  !HasScrollVelocity &&
                  !IsScrollingIntoView &&
                  (SelectedItem == null || (SelectedItem != null && !SelectedItem.IsPinned)));

        public ICommand SelectPreviousItemCommand => new MpCommand(
            async () => {
                IsArrowSelecting = true;

                bool needJump = false;
                int curLeftMostSelectQueryIdx = -1;
                int prevSelectQueryIdx = -1;
                if (SelectedItem != null) {
                    curLeftMostSelectQueryIdx = SelectedItem.QueryOffsetIdx;
                    prevSelectQueryIdx = curLeftMostSelectQueryIdx - 1;
                } else if (PersistentSelectedModels.Count > 0) {
                    needJump = true;
                    curLeftMostSelectQueryIdx = PersistentSelectedModels.
                        Select(x =>
                            MpDataModelProvider.AllFetchedAndSortedCopyItemIds.IndexOf(x.Id))
                                .Min();
                    prevSelectQueryIdx = curLeftMostSelectQueryIdx - 1;
                } else {
                    // always if none is selected selected query head
                    prevSelectQueryIdx = 0;
                }

                if (prevSelectQueryIdx < 0) {
                    // last selected must be in another query so use default
                    prevSelectQueryIdx = 0;
                }
                while (PinnedItems.Any(x => x.QueryOffsetIdx == prevSelectQueryIdx)) {
                    prevSelectQueryIdx--;
                }

                if (prevSelectQueryIdx >= MinClipTrayQueryIdx) {
                    if (needJump) {
                        QueryCommand.Execute(prevSelectQueryIdx);

                    } else if (prevSelectQueryIdx < HeadQueryIdx) {
                        //QueryCommand.Execute(false);
                        ScrollOffsetX = Math.Max(0, ScrollOffsetX - 0.1);
                    }
                    while (IsAnyBusy) { await Task.Delay(100); }


                    var pctvm = Items.FirstOrDefault(x => x.QueryOffsetIdx == prevSelectQueryIdx);
                    if (pctvm != null) {
                        int idx = Items.IndexOf(pctvm);
                        ClearClipSelection(false);
                        Items[idx].ResetSubSelection();
                        StoreSelectionState(Items[idx]);
                    }
                }
                await Task.Delay(100);
                IsArrowSelecting = false;
            },
            () => !IsAnyBusy && !HasScrollVelocity && !IsScrollingIntoView && !IsArrowSelecting &&
            (SelectedItem == null || (SelectedItem != null && !SelectedItem.IsPinned)));

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
                var mpdo = await SelectedItem.ConvertToPortableDataObject(false);
                MpPlatformWrapper.Services.DataObjectHelper.SetPlatformClipboard(mpdo, true);
            }, ()=>SelectedItem != null);

        public ICommand PasteSelectedClipsCommand => new MpAsyncCommand<object>(
            async (args) => {
                //IsPasting = true;
                //var pi = new MpProcessInfo() {
                //    Handle = MpProcessManager.LastHandle,
                //    ProcessPath = MpProcessManager.LastProcessPath
                //};
                //MpPortableDataObject mpdo = null;

                //MpPasteToAppPathViewModel ptapvm = null;
                //if (args != null && args is int appId && appId > 0) {
                //    //when pasting to a user defined application
                //    pi.Handle = IntPtr.Zero;
                //    ptapvm = MpPasteToAppPathViewModelCollection.Instance.FindById(appId);
                //    if (ptapvm != null) {
                //        pi.ProcessPath = ptapvm.AppPath;
                //        pi.IsAdmin = ptapvm.IsAdmin;
                //        pi.IsSilent = ptapvm.IsSilent;
                //        pi.ArgumentList = new List<string>() { ptapvm.Args };
                //        pi.WindowState = ptapvm.WindowState;
                //    }
                //} else if (args != null && args is IntPtr handle && handle != IntPtr.Zero) {
                //    // TODO Currently only place passing handle to this command is external drop
                //    // should probably either alter procesInfo object and add IsDragDrop or make another command for it
                //    // but for now just flagging that this is drag drop

                //    //when pasting to a running application
                //    pi.Handle = handle;
                //    ptapvm = null;
                //} else if (args is MpPortableDataObject) {
                //    mpdo = args as MpPortableDataObject;
                //}

                ////In order to paste the app must hide first 
                ////this triggers hidewindow to paste selected items

                //if (mpdo == null) {
                //    //is non-null for external template drop
                //    mpdo = await SelectedItem.ConvertToPortableDataObject(true);
                //    if (mpdo == null) {
                //        // paste was canceled
                //        return;
                //    }
                //}

                //await MpPlatformWrapper.Services.ExternalPasteHandler.PasteDataObject(
                //    mpdo, pi, ptapvm == null ? false : ptapvm.PressEnter);

                //CleanupAfterPaste(SelectedItem);
            },
            (args) => {
                return MpAvMainWindowViewModel.Instance.IsShowingDialog == false &&
                        SelectedItem != null &&
                    !IsAnyEditingClipTile &&
                    !IsAnyEditingClipTitle &&
                    !IsAnyPastingTemplate &&
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
                var mpdo = MpPlatformWrapper.Services.DataObjectHelper.GetPlatformClipboardDataObject();

                SelectedItem.RequestPastePortableDataObject(mpdo);
            }, ()=>SelectedItem != null && !SelectedItem.IsPlaceholder);

        public ICommand PasteCopyItemByIdCommand => new MpAsyncCommand<object>(
            async (args) => {
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
                        SelectedModels.Count > 0 &&
                        !IsAnyEditingClipTile &&
                        !IsAnyEditingClipTitle &&
                        !IsAnyPastingTemplate;
            });

        public ICommand LinkTagToCopyItemCommand => new MpAsyncCommand<MpAvTagTileViewModel>(
            async (tagToLink) => {
                var civm = SelectedItem;
                bool isUnlink = await tagToLink.IsLinkedAsync(civm);

                if (isUnlink) {
                    // NOTE item is removed from ui from db ondelete event
                    await tagToLink.RemoveContentItem(civm.CopyItemId);
                } else {
                    await tagToLink.AddContentItem(civm.CopyItemId);
                }


                await civm.TitleSwirlViewModel.InitializeAsync();
                await MpAvTagTrayViewModel.Instance.UpdateTagAssociation();
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
                MpShortcutCollectionViewModel.Instance.ShowAssignShortcutDialogCommand.Execute(SelectedItem);
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
                string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
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
                     string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
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
                string pt = string.Join(Environment.NewLine, PersistentSelectedModels.Select(x => x.ItemData.ToPlainText()));
                return (GetSelectedClipsType() == MpCopyItemType.Text) &&
                    pt.Length <= MpPrefViewModel.Instance.MaxQrCodeCharLength;
            });

        public ICommand SpeakSelectedClipsCommand => new MpAsyncCommand(
            async () => {
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
                MpNotificationCollectionViewModel.Instance.ShowMessageAsync("MODE CHANGED", string.Format("RIGHT CLICK PASTE MODE: {0}", IsRightClickPasteMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, ()=>!IsAppPaused);

        public ICommand ToggleAutoCopyModeCommand => new MpCommand(
            () => {
                IsAutoCopyMode = !IsAutoCopyMode;
                MpNotificationCollectionViewModel.Instance.ShowMessageAsync("MODE CHANGED", string.Format("AUTO-COPY SELECTION MODE: {0}", IsAutoCopyMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);

        public ICommand ToggleAppendModeCommand => new MpCommand(
            () => {
                IsAppendMode = !IsAppendMode;
                if (IsAppendMode && IsAppendLineMode) {
                    ToggleAppendLineModeCommand.Execute(null);
                }
                MpNotificationCollectionViewModel.Instance.ShowMessageAsync("MODE CHANGED", string.Format("APPEND MODE: {0}", IsAppendMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);

        public ICommand ToggleAppendLineModeCommand => new MpCommand(
            () => {
                IsAppendLineMode = !IsAppendLineMode;
                if (IsAppendLineMode && IsAppendMode) {
                    ToggleAppendModeCommand.Execute(null);
                }
                MpNotificationCollectionViewModel.Instance.ShowMessageAsync("MODE CHANGED", string.Format("APPEND LINE MODE: {0}", IsAppendLineMode ? "ON" : "OFF")).FireAndForgetSafeAsync(this);
            }, () => !IsAppPaused);

        public ICommand FindAndReplaceSelectedItem => new MpCommand(
            () => {
                //SelectedItem.ToggleFindAndReplaceVisibleCommand.Execute(null);
            }, () => SelectedItem != null && !SelectedItem.IsFindAndReplaceVisible && SelectedItem.IsTextItem);
        #endregion
        #endregion
    }
}
