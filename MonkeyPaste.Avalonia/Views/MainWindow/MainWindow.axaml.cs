using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
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

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MainWindow : Window {
        public static Window? Instance { get; private set; } = null;
        public MainWindow() {
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


            InitAsync().FireAndForgetSafeAsync(MpCommandErrorHandler.Instance);
        }

        private void MainWindow_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            if (Design.IsDesignMode) {
                return;
            }
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

            //DataContext = MpAvMainWindowViewModel.Instance;
            //await MpAvMainWindowViewModel.Instance.InitializeAsync();

            MpAvMainWindowViewModel.Instance.OnMainWindowOpened += Instance_OnMainWindowOpened;
            MpAvMainWindowViewModel.Instance.OnMainWindowClosed += Instance_OnMainWindowClosed;
            
            MpPlatformWrapper.Services.ClipboardMonitor.StartMonitor();
            //string cef_path = "/Users/tkefauver/Downloads/CefNet-master/cef/Release/Chromium Embedded Framework.framework";
            //var settings = new CefSettings();
            //settings.NoSandbox = true;
            //settings.MultiThreadedMessageLoop = false; // or true
            //settings.WindowlessRenderingEnabled = true;
            //settings.LocalesDirPath = Path.Combine(
            //    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "locales"); //Path.Combine(cef_path,"Resources", "locales");// @"/Resources/en.lproj");
            ////settings.ResourcesDirPath = Path.Combine(
            ////    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "locales"); //Path.Combine(cef_path, "Resources");
            
            //var app = new CefNetApplication();
            //app.Initialize(cef_path, settings);
            //Debugger.Break();

            MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
        }

        private void MainWindow_Activated(object? sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = true;
        }

        private void MainWindow_Deactivated(object? sender, System.EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsMainWindowActive = false;
            MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
        }

        private void MainWindowGrid_PointerPressed(object? sender, global::Avalonia.Input.PointerPressedEventArgs e) {
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
