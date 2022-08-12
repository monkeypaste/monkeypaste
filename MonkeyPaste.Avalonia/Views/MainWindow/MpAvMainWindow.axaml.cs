using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using MonkeyPaste.Avalonia.Utils.ToolWindow.Win;
using MonkeyPaste.Common;
using PropertyChanged;
using SharpHook;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WebViewControl;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;
using System.Linq;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMainWindow : Window {
        public static MpAvMainWindow? Instance { get; private set; } = null;

        public Grid MainWindowGrid { get; private set; }

        static MpAvMainWindow() {
            BoundsProperty.Changed.AddClassHandler<MpAvMainWindow>((x,y) => x.BoundsChangedHandler(y as AvaloniaPropertyChangedEventArgs<Rect>));
        }
        public MpAvMainWindow() {

            while (!Debugger.IsAttached) {
                Thread.Sleep(100);
            }
            if (Instance == null) {
                Instance = this;
            }

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.Activated += MainWindow_Activated;
            this.Deactivated += MainWindow_Deactivated;
            this.PointerMoved += MainWindow_PointerMoved;
            this.PointerLeave += MainWindow_PointerLeave;
            MainWindowGrid = this.FindControl<Grid>("MainWindowGridRef");

            var sidebarSplitter = this.FindControl<GridSplitter>("SidebarGridSplitter");
            sidebarSplitter.GetObservable(GridSplitter.IsVisibleProperty).Subscribe(value => SidebarSplitter_isVisibleChange(sidebarSplitter, value));

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
            
            Dispatcher.UIThread.Post(async () => {
                await InitAsync();
            });
        }
        private void SidebarSplitter_isVisibleChange(GridSplitter splitter, bool isVisible) {
            UpdateSidebarGridsplitter();
        }

        

        void BoundsChangedHandler(AvaloniaPropertyChangedEventArgs<Rect> e) {
            var oldAndNewVals = e.GetOldAndNewValue<Rect>();
            MpAvMainWindowViewModel.Instance.LastMainWindowRect = oldAndNewVals.oldValue.ToPortableRect();
            MpAvMainWindowViewModel.Instance.MainWindowRect = oldAndNewVals.newValue.ToPortableRect();
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowSizeChanged: {
                        var mwvm = MpAvMainWindowViewModel.Instance;
                        if (mwvm.MainWindowOrientationType == MpMainWindowOrientationType.Top) {
                            // can't figure out how to make resizer align to bottom so have to manually translate to bottom

                            var resizerView = this.FindControl<MpAvMainWindowResizerView>("MainWindowResizerView");
                            var resizerTransform = resizerView.RenderTransform as TranslateTransform;
                            resizerTransform.Y = mwvm.MainWindowHeight - resizerView.Height;
                        }
                        break;
                    }

                case MpMessageType.MainWindowOrientationChanged: {

                        UpdateContentOrientation();
                        UpdateResizerOrientation();


                        break;
                    }
            }
        }

        private async Task InitAsync() {
            while (!MpBootstrapperViewModelBase.IsLoaded) {
                MpConsole.WriteLine("MainWindow waiting to open...");
                await Task.Delay(100);
            }

            while (!this.IsInitialized) {
                MpConsole.WriteLine("MainWindow waiting to initialize...");
                await Task.Delay(100);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                //MpAvToolWindow_Win32.InitToolWindow(this.PlatformImpl.Handle.Handle);
            }

            
            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.MainWindowSizeChanged);


            MpAvMainWindowViewModel.Instance.IsMainWindowLoading = false;
            MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);

            // Need to delay or resizer thinks bounds are empty on initial show
            await Task.Delay(300);
            ReceivedGlobalMessage(MpMessageType.MainWindowOrientationChanged);

            MpMessenger.SendGlobal(MpMessageType.MainWindowLoadComplete);
        }


        private int? _origResizerIdx;
        private void MainWindow_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {
            var mwvm = MpAvMainWindowViewModel.Instance;
            if (mwvm.IsResizing) {
                mwvm.IsResizerVisible = true;
            } else {
                var mw_mp = e.GetCurrentPoint(Parent).Position;
                var titleView = this.FindControl<MpAvMainWindowTitleMenuView>("MainWindowTitleView");
                if (titleView.Bounds.Contains(e.GetCurrentPoint(titleView.Parent).Position) &&
                    mwvm.MainWindowOrientationType != MpMainWindowOrientationType.Bottom) {
                    mwvm.IsResizerVisible = false;
                } else {
                    mwvm.IsResizerVisible = !this.Bounds.Deflate(mwvm.ResizerLength).Contains(mw_mp);
                }

                var resizerView = this.FindControl<MpAvMainWindowResizerView>("MainWindowResizerView");
                if (_origResizerIdx == null) {
                    _origResizerIdx = resizerView.ZIndex;
                }

                if (mwvm.IsResizerVisible) {
                    resizerView.ZIndex = 1000;
                } else {
                    resizerView.ZIndex = _origResizerIdx.Value;
                }

            }

        }
        private void MainWindow_PointerLeave(object sender, global::Avalonia.Input.PointerEventArgs e) {
            MpAvMainWindowViewModel.Instance.IsResizerVisible = false;
        }

        private void MainWindow_Activated(object? sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = true;
        }

        private void MainWindow_Deactivated(object? sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = false;
            MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
        }



        private void UpdateResizerOrientation() {
            var mwvm = MpAvMainWindowViewModel.Instance; 
            
            var resizerView = this.FindControl<MpAvMainWindowResizerView>("MainWindowResizerView");
            var resizerHandle = resizerView.FindControl<Border>("MainWindowResizeOuterBorder");
            var resizerTransform = resizerView.RenderTransform as TranslateTransform;

            double resizer_long_side = mwvm.IsHorizontalOrientation ? mwvm.MainWindowWidth : mwvm.MainWindowHeight;
            double resizer_short_side = mwvm.ResizerLength;

            switch (mwvm.MainWindowOrientationType) {
                case MpMainWindowOrientationType.Bottom:
                    resizerHandle.Width = resizer_long_side;
                    resizerHandle.Height = resizer_short_side;
                    resizerHandle.HorizontalAlignment = HorizontalAlignment.Center;
                    resizerHandle.VerticalAlignment = VerticalAlignment.Stretch;

                    resizerView.Width = resizer_long_side;
                    resizerView.Height = resizer_short_side;
                    resizerView.HorizontalAlignment = HorizontalAlignment.Center;
                    resizerView.VerticalAlignment = VerticalAlignment.Top;

                    resizerTransform.Y = 0;

                    resizerView.Background = Brushes.Transparent;
                    break;
                case MpMainWindowOrientationType.Top:
                    resizerHandle.Width = resizer_long_side;
                    resizerHandle.Height = resizer_short_side;
                    resizerHandle.HorizontalAlignment = HorizontalAlignment.Center;
                    resizerHandle.VerticalAlignment = VerticalAlignment.Stretch;

                    resizerView.Width = resizer_long_side;
                    resizerView.Height = resizer_short_side;
                    resizerView.HorizontalAlignment = HorizontalAlignment.Stretch;
                    resizerView.VerticalAlignment = VerticalAlignment.Top;

                    resizerTransform.Y = mwvm.MainWindowHeight - resizerView.Height;

                    resizerView.Background = new SolidColorBrush() {
                        Color = Colors.White,
                        Opacity = 0.5
                    };
                    break;
                case MpMainWindowOrientationType.Left:
                    resizerHandle.Width = resizer_short_side;
                    resizerHandle.Height = resizer_long_side;
                    resizerHandle.HorizontalAlignment = HorizontalAlignment.Stretch;
                    resizerHandle.VerticalAlignment = VerticalAlignment.Center;

                    resizerView.Width = resizer_short_side;
                    resizerView.Height = mwvm.MainWindowHeight;
                    resizerView.HorizontalAlignment = HorizontalAlignment.Right;
                    resizerView.VerticalAlignment = VerticalAlignment.Top;

                    resizerTransform.Y = 0;

                    resizerView.Background = new SolidColorBrush() {
                        Color = Colors.White,
                        Opacity = 0.5
                    };
                    break;
                case MpMainWindowOrientationType.Right:
                    resizerHandle.Width = resizer_short_side;
                    resizerHandle.Height = resizer_long_side;
                    resizerHandle.HorizontalAlignment = HorizontalAlignment.Stretch;
                    resizerHandle.VerticalAlignment = VerticalAlignment.Center;

                    resizerView.Width = resizer_short_side;
                    resizerView.Height = mwvm.MainWindowHeight;
                    resizerView.HorizontalAlignment = HorizontalAlignment.Left;
                    resizerView.VerticalAlignment = VerticalAlignment.Top;

                    resizerTransform.Y = 0;

                    resizerView.Background = new SolidColorBrush() {
                        Color = Colors.White,
                        Opacity = 0.5
                    };
                    break;
            }
        }
        private void UpdateContentOrientation() {
            var mwvm = MpAvMainWindowViewModel.Instance;

            var mwtg = this.FindControl<Grid>("MainWindowTrayGrid");
            var sbv = this.FindControl<MpAvSidebarView>("SidebarView");
            var ctrv = this.FindControl<MpAvClipTrayView>("ClipTrayView");
            var ttv = this.FindControl<MpAvTagTreeView>("TagTreeView");
            var sbgs = this.FindControl<GridSplitter>("SidebarGridSplitter");

            EventHandler<EffectiveViewportChangedEventArgs> trayViewportChangedHandler = null;
            trayViewportChangedHandler = (s, e) => {
                ctrv.EffectiveViewportChanged -= trayViewportChangedHandler;
                //MpAvClipTrayViewModel.Instance.ListOrientationChangeEnd();
            };

            ctrv.EffectiveViewportChanged += trayViewportChangedHandler;

           // MpAvClipTrayViewModel.Instance.ListOrientationChangeBegin();

            if (mwvm.IsHorizontalOrientation) {
                mwtg.RowDefinitions.Clear();
                mwtg.ColumnDefinitions = new ColumnDefinitions("40,Auto,*");
                // sidebar columns
                Grid.SetRow(sbv, 0);
                Grid.SetColumn(sbv, 0);

                // sidebar splitter
                Grid.SetRow(sbgs, 0);
                Grid.SetColumn(sbgs, 1);
                sbgs.Height = double.NaN;
                sbgs.VerticalAlignment = VerticalAlignment.Stretch;
                sbgs.Width = 5.0d;
                sbgs.HorizontalAlignment = HorizontalAlignment.Right;

                sbgs.ResizeDirection = GridResizeDirection.Columns;
                MpAvIsHoveringExtension.SetHoverCursor(sbgs, MpCursorType.ResizeWE);

                // Add Sidebar items here
                Grid.SetRow(ttv, 0);
                Grid.SetColumn(ttv, 1);

                // cliptray container view
                Grid.SetRow(ctrv, 0);
                Grid.SetColumn(ctrv, 2);
            } else {
                mwtg.RowDefinitions = new RowDefinitions("*,Auto,40");
                mwtg.ColumnDefinitions.Clear();
                // sidebar columns
                Grid.SetRow(sbv, 2);
                Grid.SetColumn(sbv, 0);

                // sidebar splitter
                Grid.SetRow(sbgs, 1);
                Grid.SetColumn(sbgs, 0);
                sbgs.Height = 5.0d;
                sbgs.VerticalAlignment = VerticalAlignment.Top;
                sbgs.Width = double.NaN;
                sbgs.HorizontalAlignment = HorizontalAlignment.Stretch;
                sbgs.ResizeDirection = GridResizeDirection.Rows;
                MpAvIsHoveringExtension.SetHoverCursor(sbgs, MpCursorType.ResizeNS);

                // Add Sidebar items here
                Grid.SetRow(ttv, 1);
                Grid.SetColumn(ttv, 0);

                // cliptray container view
                Grid.SetRow(ctrv, 0);
                Grid.SetColumn(ctrv, 0);
            }

            UpdateSidebarGridsplitter();

            
            //mwtg.InvalidateMeasure();
        }

        private void UpdateSidebarGridsplitter() {
            //only reset when isVisibilityChanged = true, isVisibilityChanged = false is orientation change
            var sbgs = this.FindControl<GridSplitter>("SidebarGridSplitter");
            var containerGrid = sbgs.GetVisualAncestor<Grid>();

            if(MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                if (sbgs.IsVisible) {
                    containerGrid.ColumnDefinitions[1].Width = new GridLength(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel.DefaultSidebarWidth);
                } else {
                    containerGrid.ColumnDefinitions[1].Width = new GridLength(0);
                }
            } else {
                if (sbgs.IsVisible) {
                    containerGrid.RowDefinitions[1].Height = new GridLength(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel.DefaultSidebarHeight);
                } else {
                    containerGrid.RowDefinitions[1].Height = new GridLength(0);
                }
            }

        }
    }
}
