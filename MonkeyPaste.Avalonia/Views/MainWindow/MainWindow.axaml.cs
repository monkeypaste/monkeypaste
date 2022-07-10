using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.Layout;
using Avalonia.X11;
using MonkeyPaste;
using MonkeyPaste.Common;
using PropertyChanged;
using SharpHook;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WebViewControl;
using Avalonia.Media;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MainWindow : Window {
        public static Window? Instance { get; private set; } = null;

        public MainWindow() {
            WebView.Settings.OsrEnabled = false;
            WebView.Settings.LogFile = "ceflog.txt";
            
            //while (!Debugger.IsAttached) {
            //    Thread.Sleep(100);
            //}
            if (Instance == null) {
                Instance = this;
            }

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.Activated += MainWindow_Activated;
            this.Deactivated += MainWindow_Deactivated;
            this.AttachedToVisualTree += MainWindow_AttachedToVisualTree;

            MpMessenger.Register<MpMessageType>(null, ReceivedGlobalMessage);
            InitAsync().FireAndForgetSafeAsync(MpCommandErrorHandler.Instance);
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowOrientationChanged:
                    var mwvm = MpAvMainWindowViewModel.Instance;
                    var titleView = this.FindControl<MpAvMainWindowTitleMenuView>("MainWindowTitleView");
                    var contentGrid = this.FindControl<Grid>("FilterAndTrayGrid");
                    var rt = titleView.RenderTransform as RotateTransform;

                    switch (mwvm.MainWindowOrientationType) {
                        case MpMainWindowOrientationType.Top:
                        case MpMainWindowOrientationType.Bottom:
                            titleView.Width = this.Width;
                            titleView.Height = 20;


                            bool isBottom = mwvm.MainWindowOrientationType == MpMainWindowOrientationType.Bottom;

                            rt.Angle = isBottom ? 0 : 180;

                            RelativePanel.SetAlignTopWithPanel(titleView, isBottom);
                            RelativePanel.SetAlignRightWithPanel(titleView, true);
                            RelativePanel.SetAlignLeftWithPanel(titleView, true);
                            RelativePanel.SetAlignBottomWithPanel(titleView, !isBottom);

                            RelativePanel.SetAlignTopWithPanel(contentGrid, false);
                            RelativePanel.SetAlignRightWithPanel(contentGrid, true);
                            RelativePanel.SetAlignLeftWithPanel(contentGrid, true);
                            RelativePanel.SetAlignBottomWithPanel(contentGrid, false);

                            if(isBottom) {
                                RelativePanel.SetBelow(contentGrid, titleView);                                
                            } else {
                                RelativePanel.SetAbove(contentGrid, titleView);
                            }

                            break;
                        case MpMainWindowOrientationType.Left:
                        case MpMainWindowOrientationType.Right:                            
                            titleView.Width = 20;
                            titleView.Height = this.Height;

                            bool isRight = mwvm.MainWindowOrientationType == MpMainWindowOrientationType.Right;

                            rt.Angle = isRight ? 270 : 90;

                            RelativePanel.SetAlignTopWithPanel(titleView, false);
                            RelativePanel.SetAlignRightWithPanel(titleView, !isRight);
                            RelativePanel.SetAlignLeftWithPanel(titleView, isRight);
                            RelativePanel.SetAlignBottomWithPanel(titleView, false);

                            RelativePanel.SetAlignTopWithPanel(contentGrid, true);
                            RelativePanel.SetAlignRightWithPanel(contentGrid, false);
                            RelativePanel.SetAlignLeftWithPanel(contentGrid, false);
                            RelativePanel.SetAlignBottomWithPanel(contentGrid, true);

                            if (isRight) {
                                RelativePanel.SetRightOf(contentGrid, titleView);
                            } else {
                                RelativePanel.SetLeftOf(contentGrid, titleView);
                            }

                            break;
                    }
                    break;
            }
        }
        private void MainWindow_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            //if (Design.IsDesignMode) {
            //    return;
            //}
            
        }

        private async Task InitAsync() {

            while (!MpBootstrapperViewModelBase.IsLoaded) {
                await Task.Delay(100);
            }

            MpAvGlobalMouseHook.OnGlobalMouseWheelScroll += MpAvMouseHook_Win32_OnMouseWheelScroll;
            MpAvGlobalMouseHook.OnGlobalMouseMove += MpAvMouseHook_Win32_OnGlobalMouseMove;

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
        }

        private void MainWindow_Activated(object? sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = true;
        }

        private void MainWindow_Deactivated(object? sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = false;
            MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
        }

        //private void MainWindowGrid_PointerPressed(object? sender, global::Avalonia.Input.PointerPressedEventArgs e) {
        //    var mwg = this.FindControl<Grid>("MainWindowGrid");
        //    if(mwg == null) {
        //        return;
        //    }
        //    if(!mwg.Bounds.Contains(e.GetPosition(mwg.Parent))) {
        //        if (e.Pointer.Captured is Border b && b.Name == "MainWindowResizeBorder") {
        //            MpConsole.WriteTraceLine("Mouse captured and rejecting out of mainwindow click. Capturer: " + e.Pointer.Captured.GetType().ToString());
        //            return;
        //        }
        //        MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
        //    }
        //}

        

        private void Instance_OnMainWindowClosed(object? sender, System.EventArgs e) {
        }

        private void Instance_OnMainWindowOpened(object? sender, System.EventArgs e) {
            
        }

        private void MpAvMouseHook_Win32_OnGlobalMouseMove(object? sender, Common.MpPoint e) {
            
        }

        private void MpAvMouseHook_Win32_OnMouseWheelScroll(object? sender, double e) {
            if(MpAvGlobalMouseHook.GlobalMouseLocation.Y < 10) {
                MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
            }
        }
    }
}
