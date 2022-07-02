using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using PropertyChanged;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MainWindow : Window {
        public static Window Instance { get; private set; } = null;
        public MainWindow() {
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
            if (MpAvMouseHook_Win32.GlobalMouseLocation.Y < MpAvMainWindowViewModel.Instance.MainWindowTop) {
                MpAvMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
            }
        }

        private async void MainWindow_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e) {
            await InitAsync();
        }


        private async Task InitAsync() {
            MpPlatformWrapper.Init(new MpAvWrapper(this));
            var bootstrapper = new MpAvBootstrapperViewModel();
            await bootstrapper.Init();

            while (!MpAvBootstrapperViewModel.IsLoaded) {
                await Task.Delay(100);
            }
            MpAvMouseHook_Win32.Init();
            MpAvToolWindow_Win32.InitToolWindow(this.PlatformImpl.Handle.Handle);            


            MpAvMouseHook_Win32.OnGlobalMouseWheelScroll += MpAvMouseHook_Win32_OnMouseWheelScroll;
            MpAvMouseHook_Win32.OnGlobalMouseMove += MpAvMouseHook_Win32_OnGlobalMouseMove;
            
            DataContext = MpAvMainWindowViewModel.Instance;
            await MpAvMainWindowViewModel.Instance.InitializeAsync();

            MpAvMainWindowViewModel.Instance.OnMainWindowOpened += Instance_OnMainWindowOpened;
            MpAvMainWindowViewModel.Instance.OnMainWindowClosed += Instance_OnMainWindowClosed;

            //var screen = MpPlatformWrapper.Services.ScreenInfoCollection.Screens.ElementAt(0);
            //var bounds = this.Bounds;
            //Debugger.Break();
        }

        private void Instance_OnMainWindowClosed(object? sender, System.EventArgs e) {
            this.Topmost = false;

        }

        private void Instance_OnMainWindowOpened(object? sender, System.EventArgs e) {
            
        }

        private void MpAvMouseHook_Win32_OnGlobalMouseMove(object? sender, Common.MpPoint e) {
            
        }

        private void MpAvMouseHook_Win32_OnMouseWheelScroll(object? sender, double e) {
            if(MpAvMouseHook_Win32.GlobalMouseLocation.Y < 10) {
                MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
            }
        }
    }
}
