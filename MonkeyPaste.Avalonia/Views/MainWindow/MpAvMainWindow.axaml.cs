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
using Avalonia.Data;
using Avalonia.Input;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMainWindow : Window {
        #region Private Variables

        private int? _origResizerIdx;

        #endregion

        #region Statics
        public static MpAvMainWindow? Instance { get; private set; } = null;
        static MpAvMainWindow() {
            BoundsProperty.Changed.AddClassHandler<MpAvMainWindow>((x, y) => x.BoundsChangedHandler(y as AvaloniaPropertyChangedEventArgs<Rect>));
        }

        #endregion

        #region Properties
        //public MpAvExternalDropBehavior ExternalDropBehavior { get; private set; }

        #endregion

        #region Constructors

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
            this.AttachedToVisualTree += MpAvMainWindow_AttachedToVisualTree;

            var sidebarSplitter = this.FindControl<GridSplitter>("SidebarGridSplitter");
            sidebarSplitter.GetObservable(GridSplitter.IsVisibleProperty).Subscribe(value => SidebarSplitter_isVisibleChange(sidebarSplitter, value));

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);

            Dispatcher.UIThread.Post(async () => {
                await InitAsync();
            });
        }

        #endregion

        #region Public Methods
        public void UpdateResizerOrientation() {
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
        public void UpdateContentOrientation() {
            var mwvm = MpAvMainWindowViewModel.Instance;

            var mwtg = this.FindControl<Grid>("MainWindowTrayGrid");
            var sbv = this.FindControl<MpAvSidebarView>("SidebarView");
            var ctrcv = this.FindControl<MpAvClipTrayContainerView>("ClipTrayContainerView");
            var ctrcv_cg = ctrcv.FindControl<Grid>("ClipTrayContainerGrid");
            var ctrcv_ptrv = ctrcv.FindControl<MpAvPinTrayView>("PinTrayView");
            var ctrcv_ptr_lb = ctrcv_ptrv.FindControl<ListBox>("PinTrayListBox");
            var ctrcv_gs = ctrcv.FindControl<GridSplitter>("ClipTraySplitter");
            var ctrcv_ctrv = ctrcv.FindControl<MpAvClipTrayView>("ClipTrayView");
            var ttv = this.FindControl<MpAvTagTreeView>("TagTreeView");
            var sbgs = this.FindControl<GridSplitter>("SidebarGridSplitter");

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
                Grid.SetRow(ctrcv, 0);
                Grid.SetColumn(ctrcv, 2);

                // cliptraycontainer column definitions (horizontal)
                ctrcv_cg.RowDefinitions.Clear();

                // pintray column definition
                var ptrv_cd = new ColumnDefinition(new GridLength(0, GridUnitType.Auto));
                ptrv_cd.Bind(
                    ColumnDefinition.MinWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinPinTrayScreenWidth)
                    });
                ptrv_cd.Bind(
                    ColumnDefinition.MaxWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MaxPinTrayScreenWidth)
                    });

                // cliptray column definition
                var ctrv_cd = new ColumnDefinition(new GridLength(1, GridUnitType.Star));
                ctrv_cd.Bind(
                    ColumnDefinition.MinWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinClipTrayScreenWidth)
                    });

                ctrcv_cg.ColumnDefinitions = new ColumnDefinitions() {
                     ptrv_cd,
                     ctrv_cd
                };

                //pin tray view margin (horizontal)
                ctrcv_ptrv.Margin = new Thickness(0, 0, 5, 0);

                // pin tray listbox padding (horizontal) for head/tail drop adorners
                ctrcv_ptr_lb.Padding = new Thickness(10, 0, 10, 0);

                // pin tray min/max size (horizontal)
                // pintray min width/height
                ctrcv_ptrv.Bind(
                    MpAvPinTrayView.MinWidthProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinPinTrayScreenWidth)
                    });
                ctrcv_ptrv.Bind(
                    MpAvPinTrayView.MinHeightProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinPinTrayScreenHeight)
                    });


                // clip/pin tray grid splitter
                ctrcv_gs.HorizontalAlignment = HorizontalAlignment.Right;
                ctrcv_gs.VerticalAlignment = VerticalAlignment.Stretch;
                ctrcv_gs.Width = 5;
                ctrcv_gs.Height = double.NaN;
                ctrcv_gs.ResizeDirection = GridResizeDirection.Columns;
                ctrcv_gs.Cursor = new Cursor(StandardCursorType.SizeWestEast);

                Grid.SetRow(ctrcv_ptrv, 0);
                Grid.SetColumn(ctrcv_ptrv, 0);

                Grid.SetRow(ctrcv_gs, 0);
                Grid.SetColumn(ctrcv_gs, 0);

                Grid.SetRow(ctrcv_ctrv, 0);
                Grid.SetColumn(ctrcv_ctrv, 1);
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
                Grid.SetRow(ctrcv, 0);
                Grid.SetColumn(ctrcv, 0);

                // cliptraycontainer column definitions (vertical)
                ctrcv_cg.ColumnDefinitions.Clear();

                // pintray row definitions
                var ptrv_rd = new RowDefinition(new GridLength(0, GridUnitType.Auto));
                ptrv_rd.Bind(
                    RowDefinition.MinHeightProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinPinTrayScreenHeight)
                    });
                ptrv_rd.Bind(
                    RowDefinition.MaxHeightProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MaxPinTrayScreenHeight)
                    });

                //cliptray row definitions
                var ctrv_rd = new RowDefinition(new GridLength(1, GridUnitType.Star));
                ctrv_rd.Bind(
                    RowDefinition.MinHeightProperty,
                    new Binding() {
                        Source = MpAvClipTrayViewModel.Instance,
                        Path = nameof(MpAvClipTrayViewModel.Instance.MinClipTrayScreenHeight)
                    });

                ctrcv_cg.RowDefinitions = new RowDefinitions() {
                     ptrv_rd,
                     ctrv_rd
                };

                //pin tray (vertical)
                ctrcv_ptrv.Margin = new Thickness(0, 5, 0, 5);
                // pin tray listbox padding (vertical) for head/tail drop adorners
                ctrcv_ptr_lb.Padding = new Thickness(0, 10, 0, 10);

                // clip/pin tray grid splitter
                ctrcv_gs.HorizontalAlignment = HorizontalAlignment.Stretch;
                ctrcv_gs.VerticalAlignment = VerticalAlignment.Bottom;
                ctrcv_gs.Width = double.NaN;
                ctrcv_gs.Height = 5;
                ctrcv_gs.ResizeDirection = GridResizeDirection.Rows;
                ctrcv_gs.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);

                Grid.SetRow(ctrcv_ptrv, 0);
                Grid.SetColumn(ctrcv_ptrv, 0);

                Grid.SetRow(ctrcv_gs, 0);
                Grid.SetColumn(ctrcv_gs, 0);

                Grid.SetRow(ctrcv_ctrv, 1);
                Grid.SetColumn(ctrcv_ctrv, 0);
            }

            UpdateResizerOrientation();
            //UpdateSidebarGridsplitter();

            mwtg.InvalidateMeasure();
        }
        public Control GetResizerControl() {
            var mwrv = this.GetVisualDescendant<MpAvMainWindowResizerView>();
            var resize_control = mwrv.FindControl<Control>("MainWindowResizeBorder");
            return resize_control;
        }

        #endregion

        #region Private Methods
        private async Task InitAsync() {
            while (!MpBootstrapperViewModelBase.IsCoreLoaded) {
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
            //ReceivedGlobalMessage(MpMessageType.MainWindowOrientationChanged);
            MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute(null);

            MpMessenger.SendGlobal(MpMessageType.MainWindowLoadComplete);
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

                    //case MpMessageType.MainWindowOrientationChanged: {

                    //        UpdateContentOrientation();
                    //        UpdateResizerOrientation();


                    //        break;
                    //    }
            }
        }

        private void UpdateSidebarGridsplitter() {
            //only reset when isVisibilityChanged = true, isVisibilityChanged = false is orientation change
            var sbgs = this.FindControl<GridSplitter>("SidebarGridSplitter");
            var containerGrid = sbgs.GetVisualAncestor<Grid>();

            if (MpAvMainWindowViewModel.Instance.IsHorizontalOrientation) {
                if (containerGrid.ColumnDefinitions.Count == 0) {
                    UpdateContentOrientation();
                }
                if (sbgs.IsVisible) {
                    containerGrid.ColumnDefinitions[1].Width = new GridLength(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel.DefaultSidebarWidth);
                } else {
                    containerGrid.ColumnDefinitions[1].Width = new GridLength(0);
                }
            } else {
                if (containerGrid.RowDefinitions.Count == 0) {
                    UpdateContentOrientation();
                }
                if (sbgs.IsVisible) {
                    containerGrid.RowDefinitions[1].Height = new GridLength(MpAvMainWindowViewModel.Instance.SelectedSidebarItemViewModel.DefaultSidebarHeight);
                } else {
                    containerGrid.RowDefinitions[1].Height = new GridLength(0);
                }
            }
        }

        #region Event Handlers
        private void SidebarSplitter_isVisibleChange(GridSplitter splitter, bool isVisible) {
            UpdateSidebarGridsplitter();
        }

        private void BoundsChangedHandler(AvaloniaPropertyChangedEventArgs<Rect> e) {
            var oldAndNewVals = e.GetOldAndNewValue<Rect>();
            MpAvMainWindowViewModel.Instance.LastMainWindowRect = oldAndNewVals.oldValue.ToPortableRect();
            MpAvMainWindowViewModel.Instance.MainWindowRect = oldAndNewVals.newValue.ToPortableRect();
        }
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
        private void MpAvMainWindow_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            //pAvExternalDropBehavior.Instance = new MpAvExternalDropBehavior();
            //ExternalDropBehavior.Attach(this);
        }

        #endregion

        #endregion
    }
}
