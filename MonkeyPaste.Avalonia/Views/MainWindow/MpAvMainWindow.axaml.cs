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

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMainWindow : Window {
        public static Window? Instance { get; private set; } = null;
        static MpAvMainWindow() {
            BoundsProperty.Changed.AddClassHandler<MpAvMainWindow>((x,y) => x.BoundsChangedHandler(y as AvaloniaPropertyChangedEventArgs<Rect>));
        }
        public MpAvMainWindow() {
            WebView.Settings.OsrEnabled = false;
            WebView.Settings.LogFile = "ceflog.txt";

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

            //var bounds = this.GetObservable(Window.BoundsProperty);
            //bounds.Subscribe((e) => {

            //});
            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
            InitAsync().FireAndForgetSafeAsync(MpCommandErrorHandler.Instance);
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
                        var mwvm = MpAvMainWindowViewModel.Instance;
                        var titleView = this.FindControl<MpAvMainWindowTitleMenuView>("MainWindowTitleView");
                        var contentGrid = this.FindControl<Grid>("FilterAndTrayGrid");
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
                        break;
                    }
            }
        }

        private async Task InitAsync() {

            while (!MpBootstrapperViewModelBase.IsLoaded) {
                await Task.Delay(100);
            }

            MpAvGlobalInputHook.Instance.OnGlobalMouseWheelScroll += MpAvGlobalInputHook_OnMouseWheelScroll;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                MpAvToolWindow_Win32.InitToolWindow(this.PlatformImpl.Handle.Handle);

                //MpAvMouseHook_Win32.Init();
                //MpAvMouseHook_Win32.OnGlobalMouseWheelScroll += MpAvMouseHook_Win32_OnMouseWheelScroll;
                //MpAvMouseHook_Win32.OnGlobalMouseMove += MpAvMouseHook_Win32_OnGlobalMouseMove;
            }

            MpAvMainWindowViewModel.Instance.OnMainWindowOpened += Instance_OnMainWindowOpened;
            MpAvMainWindowViewModel.Instance.OnMainWindowClosed += Instance_OnMainWindowClosed;

            MpPlatformWrapper.Services.ClipboardMonitor.StartMonitor();

            MpAvMainWindowViewModel.Instance.IsMainWindowLoading = false;
            MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);

            
            ReceivedGlobalMessage(MpMessageType.MainWindowOrientationChanged);
            MpMessenger.SendGlobal<MpMessageType>(MpMessageType.MainWindowSizeChanged);
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

        private void Instance_OnMainWindowClosed(object? sender, System.EventArgs e) {
        }

        private void Instance_OnMainWindowOpened(object? sender, System.EventArgs e) {

        }

        private void MpAvGlobalInputHook_OnMouseWheelScroll(object? sender, MouseWheelHookEventArgs e) {
            if (MpAvGlobalInputHook.Instance.GlobalMouseLocation.Y < 10) {
                MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
            }
        }
    }
}
