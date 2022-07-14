using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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
        MpIPagingScrollViewer {
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
        public override ObservableCollection<MpAvClipTileViewModel> Items { get => base.Items; set => base.Items = value; }
        #endregion

        #region MpIPagingScrollViewer Implementation
        public Orientation ListOrientation => MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ? Orientation.Horizontal : Orientation.Vertical;

        public double ScrollOffsetX { get; set; }
        public double ScrollOffsetY { get; set; }
        public double MaxScrollOffsetX {
            get {
                if (ListOrientation == Orientation.Vertical) {
                    return 0;
                }
                return ClipTrayTotalTileWidth - ClipTrayScreenWidth;
            }
        }

        public double MaxScrollOffsetY {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    return 0;
                }
                return ClipTrayTotalTileHeight - ClipTrayScreenHeight;
            }
        }

        public double ClipTrayTotalTileWidth {
            get {
                if (ListOrientation == Orientation.Vertical) {
                    return ClipTrayScreenWidth;
                }
                return Items.Last().TrayX + Items.Last().MinSize;// + Items.Last().Spacing;
            }
        }
        public double ClipTrayTotalTileHeight {
            get {
                if (ListOrientation == Orientation.Horizontal) {
                    return ClipTrayScreenHeight;
                }
                return Items.Last().TrayY + Items.Last().MinSize;// + Items.Last().Spacing;
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

        #endregion
        #region Layout



        #endregion

        #region Appearance

        public ScrollBarVisibility HorizontalScrollBarVisibility {
            get {
                return ListOrientation == Orientation.Horizontal ?
                                        ScrollBarVisibility.Auto :
                                        ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility VerticalScrollBarVisibility {
            get {
                return ListOrientation == Orientation.Horizontal ?
                                        ScrollBarVisibility.Hidden :
                                        ScrollBarVisibility.Auto;
            }
        }

        public MpAvClipTrayLayoutType LayoutType { get; set; } = MpAvClipTrayLayoutType.Stack;

        #endregion

        #region State



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
            IsBusy = true;

            for (int i = 1; i <= 100; i++) {
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


            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);


            IsBusy = false;
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

                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.ContentResized);
                    break;
                case nameof(ClipTrayScreenWidth):
                case nameof(ClipTrayScreenHeight):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.MinSize)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayY)));

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
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.TrayLayoutChanged:
                case MpMessageType.ContentResized:
                    UpdateScrollProperties();
                    break;
                case MpMessageType.MainWindowSizeReset:
                    ZoomFactor = 1.0d;
                    break;
            }
        }

        private void UpdateScrollProperties() {
            OnPropertyChanged(nameof(LayoutType));

            OnPropertyChanged(nameof(ListOrientation));

            OnPropertyChanged(nameof(ClipTrayScreenWidth));
            OnPropertyChanged(nameof(ClipTrayScreenHeight));

            OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
            OnPropertyChanged(nameof(ClipTrayTotalTileHeight));

            OnPropertyChanged(nameof(ClipTrayTotalWidth));
            OnPropertyChanged(nameof(ClipTrayTotalHeight));


            OnPropertyChanged(nameof(MaxScrollOffsetX));
            OnPropertyChanged(nameof(MaxScrollOffsetY));

            OnPropertyChanged(nameof(HorizontalScrollBarVisibility));
            OnPropertyChanged(nameof(VerticalScrollBarVisibility));
        }

        #endregion

        #region Commands

        public ICommand ToggleLayoutTypeCommand => new MpCommand(
            () => {
                if (IsGridLayout) {
                    LayoutType = MpAvClipTrayLayoutType.Grid;
                } else {
                    LayoutType = MpAvClipTrayLayoutType.Stack;
                }
                MpMessenger.SendGlobal(MpMessageType.TrayLayoutChanged);
            });
        #endregion
    }
}
