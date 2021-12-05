using MonkeyPaste;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using WPFSpark;
using static MpWpfApp.WinApi;
using static Standard.NtDll;

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

            //EnableBlur();
            
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
            var mwvm = DataContext as MpMainWindowViewModel;
            if(mwvm.IsResizing || MpDragDropManager.Instance.IsDragAndDrop) {
                return;
            }
            mwvm.HideWindowCommand.Execute(null);
        }

        private void MainWindowCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (Mouse.GetPosition(this).Y < 0) {
                var mwvm = DataContext as MpMainWindowViewModel;
                mwvm.HideWindowCommand.Execute(null);
            }
        }

        private void Image_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var mwvm = DataContext as MpMainWindowViewModel;
            //mwvm.HideWindowCommand.Execute(null);
        }

        void EnableBlur() {
            SetAccentPolicy(WinApi.AccentState.ACCENT_ENABLE_BLURBEHIND);
        }

        private void SetAccentPolicy(AccentState accentState) {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new WinApi.AccentPolicy {
                AccentState = accentState,
                AccentFlags = GetAccentFlagsForTaskbarPosition()
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);


            var data = new WinApi.WindowCompositionAttributeData {
                Attribute = WinApi.WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            WinApi.SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private WinApi.AccentFlags GetAccentFlagsForTaskbarPosition() {
            return WinApi.AccentFlags.DrawAllBorders;
        }

        public void DisableBlur() {
            SetAccentPolicy(WinApi.AccentState.ACCENT_DISABLED);
        }
    }
}
