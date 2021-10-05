using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static MpWpfApp.WinApi;

namespace MpWpfApp {
    public partial class MpMainWindow : Window {        
        public MpMainWindow() {
            InitializeComponent();
        }

        private void MainWindow_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
                      
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
           // MpPreferences.Instance.ThisAppDip = (double)MpScreenInformation.RawDpi / 96;//VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip;

            WindowInteropHelper wndHelper = new WindowInteropHelper((MpMainWindow)Application.Current.MainWindow);
            int exStyle = (int)WinApi.GetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null && DataContext is MpMainWindowViewModel mwvm) {
                mwvm.OnMainWindowShow += Mwvm_OnMainWindowShow;
                mwvm.OnMainWindowHide += Mwvm_OnMainWindowHide;
                MpClipTrayViewModel.Instance.ViewModelLoaded += Instance_ViewModelLoaded;

                MpPasteToAppPathViewModelCollection.Instance.Init();

                MpShortcutCollectionViewModel.Instance.Init();

                MpSoundPlayerGroupCollectionViewModel.Instance.Init();
            }
        }

        private void Instance_ViewModelLoaded(object sender, EventArgs e) {
            //MpSoundPlayerGroupCollectionViewModel.Instance.PlayLoadedSoundCommand.Execute(null);

            int totalItems = MpDb.Instance.GetItems<MpCopyItem>().Count;


            MpStandardBalloonViewModel.ShowBalloon(
               "Monkey Paste",
               "Successfully loaded w/ " + totalItems + " items",
               Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");

            MpClipTrayViewModel.Instance.ViewModelLoaded -= Instance_ViewModelLoaded;
        }

        private void Mwvm_OnMainWindowHide(object sender, EventArgs e) {
            //DisableBlur();
        }

        private void Mwvm_OnMainWindowShow(object sender, EventArgs e) {
            //EnableBlur();
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(SystemParameters.WorkArea)) {
                this.Dispatcher.Invoke(() => {
                    //SetupMainWindowRect();
                });
            }
        }

        private void MainWindow_Deactivated(object sender, EventArgs e) {
            var mwvm = DataContext as MpMainWindowViewModel;
            mwvm.HideWindowCommand.Execute(null);
        }

        private void MainWindowCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var hitTest = VisualTreeHelper.HitTest(MainWindowCanvas, e.GetPosition(MainWindowCanvas));
            if (hitTest != null && hitTest.VisualHit != null) {
                if (hitTest.VisualHit == MainWindowCanvas) {
                    var mwvm = DataContext as MpMainWindowViewModel;
                    mwvm.HideWindowCommand.Execute(null);
                }
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
