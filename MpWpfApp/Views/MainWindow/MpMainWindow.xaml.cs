using MonkeyPaste;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MpWpfApp {
    public partial class MpMainWindow : Window {
        Stopwatch sw;
        public MpMainWindow() {
            sw = new Stopwatch();
            sw.Start();
            DataContext = MpMainWindowViewModel.Instance;
            InitializeComponent();
        }

        private  void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            LoadMainWindow();
        }

        private void LoadMainWindow() {
            WindowInteropHelper wndHelper = new WindowInteropHelper((MpMainWindow)Application.Current.MainWindow);
            int exStyle = (int)WinApi.GetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

            var mwvm = DataContext as MpMainWindowViewModel;

            
            mwvm.OnMainWindowShow += Mwvm_OnMainWindowShow;
            mwvm.OnMainWindowHide += Mwvm_OnMainWindowHide;

            // MpPasteToAppPathViewModelCollection.Instance.Init();

            sw.Stop();

            MpConsole.WriteLine($"Mainwindow loading: {sw.ElapsedMilliseconds} ms");
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
            if (mwvm.IsResizing || MpDragDropManager.Instance.IsDragAndDrop) {
                return;
            }
            mwvm.HideWindowCommand.Execute(null);
        }

        private void MainWindowCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (Mouse.GetPosition(this).Y < 0) {
                PerformMainWindowHide();
            }
        }

        private void Image_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var mwvm = DataContext as MpMainWindowViewModel;
            //mwvm.HideWindowCommand.Execute(null);
        }

        private void GridSplitter_MouseEnter(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.ResizeWE;
        }

        private void GridSplitter_MouseLeave(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }
    }
}
