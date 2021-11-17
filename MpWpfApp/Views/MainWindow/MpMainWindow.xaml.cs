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

namespace MpWpfApp {
    public partial class MpMainWindow : Window {
        Stopwatch sw;
        public MpMainWindow() {
            sw = new Stopwatch();
            sw.Start();
            DataContext = MpMainWindowViewModel.Instance;
            InitializeComponent();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            await LoadMainWindow();
        }

        private async Task LoadMainWindow() {
            WindowInteropHelper wndHelper = new WindowInteropHelper((MpMainWindow)Application.Current.MainWindow);
            int exStyle = (int)WinApi.GetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

            // MpPreferences.Instance.ThisAppDip = (double)MpScreenInformation.RawDpi / 96;//VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip;

            var mwvm = DataContext as MpMainWindowViewModel;

            
            mwvm.OnMainWindowShow += Mwvm_OnMainWindowShow;
            mwvm.OnMainWindowHide += Mwvm_OnMainWindowHide;

            // MpPasteToAppPathViewModelCollection.Instance.Init();

            int totalItems = await MpDataModelProvider.Instance.GetTotalCopyItemCountAsync();
            await Task.Delay(3000);
            MpStandardBalloonViewModel.ShowBalloon(
                    "Monkey Paste",
                    "Successfully loaded w/ " + totalItems + " items",
                    Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");

            sw.Stop();

            //while(mwvm.IsMainWindowLoading) {
            //    await Task.Delay(100);
            //}

           // Application.Current.Resources["TagTrayViewModel"] = MpTagTrayViewModel.Instance;
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
            if(mwvm.IsResizing) {
                return;
            }
            mwvm.HideWindowCommand.Execute(null);
        }

        private void MainWindowCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (Mouse.GetPosition(MainWindowGrid).Y < -10) {
                var mwvm = DataContext as MpMainWindowViewModel;
                mwvm.HideWindowCommand.Execute(null);
            }
        }

        private void Image_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var mwvm = DataContext as MpMainWindowViewModel;
            //mwvm.HideWindowCommand.Execute(null);
        }

        private uint _blurOpacity;
        public double BlurOpacity {
            get { return _blurOpacity; }
            set { _blurOpacity = (uint)value; EnableBlur(); }
        }

        void EnableBlur() {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
            accent.GradientColor = 0x80804000;//(_blurOpacity << 24) | (_blurBackgroundColor & 0xFFFFFF);

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        void DisableBlur() {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT;
            accent.GradientColor = 0;//(_blurOpacity << 24) | (_blurBackgroundColor & 0xFFFFFF);

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;
            
            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
    }
}
