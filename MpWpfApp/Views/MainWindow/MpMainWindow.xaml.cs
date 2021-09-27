using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public partial class MpMainWindow : Window {        
        public MpMainWindow() {
            InitializeComponent();
        }

        private void MainWindow_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
                      
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Properties.Settings.Default.ThisAppDip = VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip;

            WindowInteropHelper wndHelper = new WindowInteropHelper((MpMainWindow)Application.Current.MainWindow);
            int exStyle = (int)WinApi.GetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(wndHelper.Handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

            
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null) {
                MpPasteToAppPathViewModelCollection.Instance.Init();

                MpShortcutCollectionViewModel.Instance.Init();

                MpSoundPlayerGroupCollectionViewModel.Instance.Init();

                int totalItems = MpDb.Instance.GetItems<MpCopyItem>().Count;

                MpSoundPlayerGroupCollectionViewModel.Instance.PlayLoadedSoundCommand.Execute(null);
                MpStandardBalloonViewModel.ShowBalloon(
                   "Monkey Paste",
                   "Successfully loaded w/ " + totalItems + " items",
                   Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");

                MpMainWindowViewModel.IsMainWindowLoading = false;
            }
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

        
    }
}
