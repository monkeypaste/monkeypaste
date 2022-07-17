using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
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
        MpIPagingScrollViewerViewModel {
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

        public async Task InitializeAsync() {
            LogPropertyChangedEvents = false;

            IsBusy = true;
            
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

        public void OnPostMainWindowLoaded() {
            //int totalItems = MpTagTrayViewModel.Instance.AllTagViewModel.TagClipCount;

            //MpSystemTrayViewModel.Instance.TotalItemCountLabel = string.Format(@"{0} total entries", totalItems);

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

            //        MpTagTrayViewModel.Instance.SelectTagCommand.Execute(qi.TagId);

            //        MpSearchBoxViewModel.Instance.SearchText = qi.SearchText;
            //        // NOTE Filter flags already set from Preferences

            //        MpPlatformWrapper.Services.QueryInfo = qi;


            //        MpDataModelProvider.Init();
            //    }
            //}
            //MpAvMainWindowViewModel.Instance.IsMainWindowLoading = false;


            //MpDataModelProvider.QueryInfo.NotifyQueryChanged(true);
        }
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
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayScrollChanged);
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
    }
}
