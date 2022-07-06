using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.X11;
using MonkeyPaste.Common;
using PropertyChanged;
using SharpHook;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MainWindow : Window {
        public static Window? Instance { get; private set; } = null;
        public MainWindow() {
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
            var canvas = this.FindControl<Canvas>("MainWindowContainerCanvas");
            canvas.PointerPressed += Canvas_PointerPressed;
            this.Activated += MainWindow_Activated;
            this.Deactivated += MainWindow_Deactivated;
        }

        private void MainWindow_Activated(object? sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = true;
        }

        private void MainWindow_Deactivated(object? sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = false;
            MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
        }

        private void Canvas_PointerPressed(object? sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            var mwg = this.FindControl<Grid>("MainWindowGrid");
            if(mwg == null) {
                return;
            }
            if(!mwg.Bounds.Contains(e.GetPosition(mwg.Parent))) {
                if (e.Pointer.Captured is Border b && b.Name == "MainWindowResizeBorder") {
                    MpConsole.WriteTraceLine("Mouse captured and rejecting out of mainwindow click. Capturer: " + e.Pointer.Captured.GetType().ToString());
                    return;
                }
                MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
            }
        }

        private async void MainWindow_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            if(Design.IsDesignMode) {
                return;
            }
            await InitAsync();
        }


        private async Task InitAsync() {
            await MpPlatformWrapper.InitAsync(new MpAvWrapper(this));
            var bootstrapper = new MpAvBootstrapperViewModel();
            await bootstrapper.InitAsync();

            while (!MpBootstrapperViewModelBase.IsLoaded) {
                await Task.Delay(100);
            }
            MpAvGlobalMouseHook.Init(((IRenderRoot)this).RenderScaling);

            MpAvGlobalMouseHook.OnGlobalMouseWheelScroll += MpAvMouseHook_Win32_OnMouseWheelScroll;
            MpAvGlobalMouseHook.OnGlobalMouseMove += MpAvMouseHook_Win32_OnGlobalMouseMove;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {

                MpAvToolWindow_Win32.InitToolWindow(this.PlatformImpl.Handle.Handle);

                //MpAvMouseHook_Win32.Init();
                //MpAvMouseHook_Win32.OnGlobalMouseWheelScroll += MpAvMouseHook_Win32_OnMouseWheelScroll;
                //MpAvMouseHook_Win32.OnGlobalMouseMove += MpAvMouseHook_Win32_OnGlobalMouseMove;
            }

            DataContext = MpAvMainWindowViewModel.Instance;
            await MpAvMainWindowViewModel.Instance.InitializeAsync();

            MpAvMainWindowViewModel.Instance.OnMainWindowOpened += Instance_OnMainWindowOpened;
            MpAvMainWindowViewModel.Instance.OnMainWindowClosed += Instance_OnMainWindowClosed;

            MpPlatformWrapper.Services.ClipboardMonitor.StartMonitor();
        }

        

        private void Instance_OnMainWindowClosed(object? sender, System.EventArgs e) {
            this.Topmost = false;

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
