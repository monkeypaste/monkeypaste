using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
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

        public Orientation ListOrientation => MpAvMainWindowViewModel.Instance.IsHorizontalOrientation ? Orientation.Horizontal : Orientation.Vertical;

        public double LastScrollOffsetX { get; set; }
        public double LastScrollOffsetY { get; set; }
        public Vector LastScrollOffset => new Vector(LastScrollOffsetX, LastScrollOffsetY);

        public Vector ScrollOffset => new Vector(ScrollOffsetX, ScrollOffsetY);

        private double _scrollOffsetX;
        public double ScrollOffsetX {
            get => _scrollOffsetX;
            set {
                LastScrollOffsetX = _scrollOffsetX;
                _scrollOffsetX = value;
            }
        }
        private double _scrollOffsetY;
        public double ScrollOffsetY {
            get => _scrollOffsetY;
            set {
                LastScrollOffsetY = _scrollOffsetY;
                _scrollOffsetY = value;
            }
        }

        public double MaxScrollOffsetX => ClipTrayTotalTileWidth - ClipTrayScreenWidth;

        public double MaxScrollOffsetY { get; set; }

        public double ZoomFactor { get; set; } = 1;

        public double ClipTrayTotalTileWidth => Items.Last().TrayX + Items.Last().MinSize + Items.Last().Spacing;
                

        public double ClipTrayTotalWidth => Math.Max(ClipTrayScreenWidth, ClipTrayTotalTileWidth);

        public double ClipTrayScreenWidth { get; set; }

        public double ClipTrayScreenHeight { get; set; }

        public Size ClipTrayExtentSize => new(ClipTrayTotalWidth, ClipTrayScreenHeight);

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

        public MpPoint ScrollVelocity {
            get {
                return new MpPoint(ScrollVelocityX, ScrollVelocityY);
            }
        }

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

                    MpMessenger.SendGlobal<MpMessageType>(MpMessageType.TrayLayoutChanged);
                    break;
                case nameof(ClipTrayScreenWidth):
                case nameof(ClipTrayScreenHeight):
                    OnPropertyChanged(nameof(ClipTrayViewportSize));
                    break;
                case nameof(ClipTrayTotalTileWidth):
                    OnPropertyChanged(nameof(ClipTrayExtentSize));
                    OnPropertyChanged(nameof(MaxScrollOffsetX));
                    OnPropertyChanged(nameof(MaxScrollOffsetY));
                    break;
                case nameof(ScrollOffset):
                    MpConsole.WriteLine("Last scroll: " + LastScrollOffset);
                    MpConsole.WriteLine("Cur scroll: " + ScrollOffset);
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
                //case MpMessageType.TrayScrollChanged:
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.MinSize)));                    
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayX)));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.TrayY)));

                    OnPropertyChanged(nameof(LayoutType));
                    OnPropertyChanged(nameof(ListOrientation));
                    OnPropertyChanged(nameof(ClipTrayTotalTileWidth));
                    OnPropertyChanged(nameof(ClipTrayTotalWidth));
                    OnPropertyChanged(nameof(ClipTrayViewportSize));
                    OnPropertyChanged(nameof(ClipTrayExtentSize));

                    var mwvm = MpAvMainWindowViewModel.Instance;

                    //double rw = mwvm.MainWindowRect.Width / mwvm.LastMainWindowRect.Width;
                    //double rh = mwvm.MainWindowRect.Height / mwvm.LastMainWindowRect.Height;
                    //rw = rw.IsNumber() ? rw : 1;
                    //rh = rh.IsNumber() ? rh : 1;

                    //double rx = ScrollOffsetX / LastScrollOffsetX;
                    //double ry = ScrollOffsetY / LastScrollOffsetY;
                    //rx = rx.IsNumber() ? rx : 1;
                    //ry = ry.IsNumber() ? ry : 1;

                    //double rw = LastScrollOffsetX / ScrollOffsetX;
                    //double rh = LastScrollOffsetY / ScrollOffsetY;
                    //rw = rw.IsNumber() ? rw : 1;
                    //rh = rh.IsNumber() ? rh : 1;

                    //double rx = ScrollOffsetX / LastScrollOffsetX;
                    //double ry = ScrollOffsetY / LastScrollOffsetY;
                    //rx = rx.IsNumber() ? rx : 1;
                    //ry = ry.IsNumber() ? ry : 1;


                    //ScrollOffsetX *= rw;
                    //ScrollOffsetY *= rh;
                    double oldHeadTrayX = Items.Last().TrayX;
                    double oldScrollOffsetDiffWithHead = LastScrollOffsetX - oldHeadTrayX;

                    //double newHeadTrayX = HeadItem == null ? 0 : HeadItem.TrayX;
                    //double headOffsetRatio = newHeadTrayX / oldHeadTrayX;
                    //headOffsetRatio = double.IsNaN(headOffsetRatio) ? 1 : headOffsetRatio;
                    //double newScrollOfsetDiffWithHead = headOffsetRatio * oldScrollOffsetDiffWithHead;
                    //double newScrollOfset = FindTileOffsetX(HeadQueryIdx) + newScrollOfsetDiffWithHead;

                    //if(newScrollOfset < 100 || Math.Abs(newScrollOfset - oldScrollOfset) > 200) {
                    //    Debugger.Break();
                    //}
                    //ScrollOffset = newScrollOfset;
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
