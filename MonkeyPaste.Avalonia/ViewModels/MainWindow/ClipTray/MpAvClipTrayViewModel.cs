using Avalonia;
using Avalonia.Controls.Primitives;
using MonkeyPaste.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpAvClipTrayLayoutType {
        Stack,
        Grid
    }

    public class MpAvClipTrayViewModel : MpSelectorViewModelBase<object, MpAvClipTileViewModel>,
        MpIBootstrappedItem {
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

        #region Layout
        

        public Vector ScrollOffset {
            get => new Vector(ScrollOffsetX, ScrollOffsetY);
            set {
                ScrollOffsetX = value.X;
                ScrollOffsetY = value.Y;
            }
        }

        public double ScrollOffsetX { get; set; }

        public double ScrollOffsetY { get; set; }

        public double MaxScrollOffsetX => ClipTrayTotalTileWidth - ClipTrayScreenWidth;

        public double MaxScrollOffsetY { get; set; }

        public double ZoomFactor { get; set; } = 1;

        public double ClipTrayTotalTileWidth => Items.Sum(x => x.MinSize);

        

        public double ClipTrayTotalWidth => Math.Max(ClipTrayScreenWidth, ClipTrayTotalTileWidth);

        public double ClipTrayScreenWidth { get; set; }

        public double ClipTrayScreenHeight { get; set; }

        public Size ClipTrayExtentSize => new Size(ClipTrayTotalWidth, ClipTrayScreenHeight);

        public Size ClipTrayViewportSize => new Size(ClipTrayScreenWidth, ClipTrayScreenHeight);
        //public double ZoomFactorY { get; set; } = 250;

        #endregion

        #region Appearance

        public ScrollBarVisibility HorizontalScrollBarVisibility {
            get {
                //var mwvm = MpAvMainWindowViewModel.Instance;
                //if(LayoutType == MpAvClipTrayLayoutType.Stack) {
                //    if(mwvm.MainWindowOrientationType == MpMainWindowOrientationType.)
                //}
                return ScrollBarVisibility.Auto;
            }
        }

        public MpAvClipTrayLayoutType LayoutType { get; set; } = MpAvClipTrayLayoutType.Stack;

        #endregion

        #region State

        public MpPoint ScrollVelocity => new MpPoint(ScrollVelocityX, ScrollVelocityY);

        public double ScrollVelocityX { get; set; }

        public double ScrollVelocityY { get; set; }

        public bool HasScrollVelocity => ScrollVelocity.Length > 0;

        public bool IsScrollingIntoView { get; set; }

        public bool IsGridLayout { get; set; }

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

        public bool IsRequery { get; set; } = false;

        public bool IsThumbDragging { get; set; } = false;

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
            MpConsole.WriteLine($"Name: {e.PropertyName} Value: {this.GetPropertyValue(e.PropertyName)?.ToString()}");
            switch (e.PropertyName) {
                case nameof(Items):
                case nameof(SelectedItem):
                    OnPropertyChanged(nameof(CanScroll));
                    break;
                case nameof(IsGridLayout):
                    ToggleLayoutTypeCommand.Execute(null);
                    break;
                case nameof(ZoomFactor):

                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayLayoutChanged);
                    break;
                case nameof(ClipTrayScreenWidth):
                case nameof(ClipTrayScreenHeight):
                    OnPropertyChanged(nameof(ClipTrayViewportSize));
                    break;
                case nameof(ClipTrayTotalTileWidth):
                    OnPropertyChanged(nameof(ClipTrayExtentSize));
                    break;
                case nameof(ScrollOffsetX):
                case nameof(ScrollOffsetY):
                    OnPropertyChanged(nameof(ScrollOffset));
                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayScrollChanged);
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowOrientationChanged:
                case MpMessageType.MainWindowSizeChanged:
                case MpMessageType.TrayLayoutChanged:
                case MpMessageType.TrayScrollChanged:
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.MinSize)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayY)));

                    OnPropertyChanged(nameof(ClipTrayViewportSize));
                    OnPropertyChanged(nameof(ClipTrayExtentSize));
                    break;
            }
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
