using MonkeyPaste;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.Windows.Controls.Primitives;

namespace MpWpfApp {
    public partial class MpMainWindow : Window {
        Stopwatch sw;
        public MpMainWindow() {
            sw = new Stopwatch();
            sw.Start();
            InitializeComponent();
        }

        private  void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            LoadMainWindow();
        }

        private void LoadMainWindow() {
            WindowInteropHelper wndHelper = new WindowInteropHelper((MpMainWindow)Application.Current.MainWindow);
            MpProcessHelper.MpProcessManager.ThisAppHandle = wndHelper.Handle;
            MpClipboardHelper.MpClipboardManager.ThisAppHandle = wndHelper.Handle;

            int exStyle = (int)WinApi.GetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

            var mwvm = DataContext as MpMainWindowViewModel;

            
            mwvm.OnMainWindowShow += Mwvm_OnMainWindowShow;
            mwvm.OnMainWindowHidden += Mwvm_OnMainWindowHide;

            sw.Stop();

            MpConsole.WriteLine($"Mainwindow loading: {sw.ElapsedMilliseconds} ms");


            HwndSource Source;
            Source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            Source.AddHook(new HwndSourceHook(Window_Proc));


            //Application.Current.MainWindow.Height = mwvm.MainWindowContainerHeight;
            //Application.Current.MainWindow.Top = mwvm.MainWindowContainerTop;
        }

        private IntPtr Window_Proc(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam, ref bool Handled) {
            Win32.COPYDATASTRUCT CopyData;
            string Path;

            if (Msg == Win32.WM_COPYDATA) {
                CopyData = (Win32.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.COPYDATASTRUCT));
                Path = Marshal.PtrToStringUni(CopyData.lpData, CopyData.cbData / 2);

                if (WindowState == WindowState.Minimized) {
                    // Restore window from tray
                }

                // Do whatever we want with information

                Activate();
                Focus();
            }

            if (Msg == App.MessageId) {
                Handled = true;
                return new IntPtr(App.MessageId);
            }

            return IntPtr.Zero;
        }
        private void Mwvm_OnMainWindowHide(object sender, EventArgs e) {
            //DisableBlur();
        }

        private void Mwvm_OnMainWindowShow(object sender, EventArgs e) {
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(SystemParameters.WorkArea)) {
                var mwvm = DataContext as MpMainWindowViewModel;
                this.Dispatcher.Invoke(() => {
                    mwvm.SetupMainWindowRect();
                });
            }
        }

        private void MainWindow_Deactivated(object sender, EventArgs e) {
            PerformMainWindowHide();
        }

        private void PerformMainWindowHide() {
            var mwvm = DataContext as MpMainWindowViewModel;
            if (mwvm.IsResizing || MpDragDropManager.IsDragAndDrop || MpContextMenuView.Instance.IsOpen) {
                return;
            }
            mwvm.HideWindowCommand.Execute(null);
        }

        private void MainWindowCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (Mouse.GetPosition(this).Y < 0) {
                PerformMainWindowHide();
            }
        }

        private void SidebarGridSplitter_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var splitter = sender as GridSplitter;
            var containerGrid = splitter.GetVisualAncestor<Grid>();

            if (!(bool)e.NewValue) {                
                containerGrid.ColumnDefinitions[1].Width = new GridLength(0);
            } else {
                containerGrid.ColumnDefinitions[1].Width = GridLength.Auto;
            }
        }

        private void ToggleButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;
            var bb = sender as ButtonBase;
            bb.Command.Execute(bb.CommandParameter);
        }
    }
}
