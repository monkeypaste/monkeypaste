using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;
using MonkeyPaste;
using static MonoMac.Darwin.Message;
using System.Linq;
using Avalonia.Threading;
using System;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvNotificationWindow : Window, MpINotificationBalloonView {
        private static ObservableCollection<MpAvNotificationWindow> _windows;
        public MpAvNotificationWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            if (_windows == null) {
                _windows = new ObservableCollection<MpAvNotificationWindow>();
                _windows.CollectionChanged += _windows_CollectionChanged;
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        private void _windows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateWindows();
        }
        private static void UpdateWindows() {
            foreach (var window in _windows.ToList()) {
                window.SetWindowToBottomRightOfScreen();
            }
        }
        private void SetWindowToBottomRightOfScreen() {
            if (!_windows.Contains(this)) {
                return;
            }
            var screens = new MpAvScreenInfoCollection();

            var primaryScreen = screens.Screens.FirstOrDefault(x => x.IsPrimary);
            
            double pad = 10;
            double x = primaryScreen.WorkArea.Right - this.Bounds.Width - pad;
            double offsetY = _windows.Where(x => _windows.IndexOf(x) < _windows.IndexOf(this)).Sum(x => x.Bounds.Height + pad);
            offsetY += this.Bounds.Height + pad;
            double y = primaryScreen.WorkArea.Bottom - offsetY;

            //if(OperatingSystem.IsWindows()) 
            {
                x *= primaryScreen.PixelDensity;
                y *= primaryScreen.PixelDensity;
            }

            this.Position = new PixelPoint((int)x, (int)y);
        }

        private void CloseButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            HideWindow(DataContext as MpNotificationViewModelBase);
        }

        private void MpAvNotificationWindow_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            UpdateWindows();
        }
        private void FadeIn_Completed(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = true;
        }

        private void FadeOut_Completed(object sender, EventArgs e) {
            MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
            _windows.Remove(this);
            Hide();
            
        }
        private void MpAvNotificationWindow_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            if (MpAvMainWindow.Instance == null || !MpAvMainWindow.Instance.IsInitialized) {
                return;
            }
            if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }
            MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
        }

        public void ShowWindow(object dc) {
            Dispatcher.UIThread.Post(() => {
                DataContext = dc;
                Opacity = 1;
                App.Desktop.MainWindow = this;

                if (dc is MpNotificationViewModelBase nvmb && !nvmb.IsVisible) {
                    nvmb.IsVisible = true;
                }
                this.Show();

                MpAvMainWindowViewModel.Instance.IsShowingDialog = true;

                //var fisb = nw.Resources["FadeIn"] as Storyboard;
                //Storyboard.SetTarget(fisb, nw);

                ////nw.Show();

                //fisb.Begin();

                FadeIn_Completed(this, EventArgs.Empty);

                _windows.Add(this);
            });
        }

        public void HideWindow(object dc) {
            Dispatcher.UIThread.Post(() => {
                var nw = _windows.FirstOrDefault(x => x.DataContext == dc);
                //var fisb = nw.Resources["FadeOut"] as Storyboard;
                //if (nw != null) {
                //    Storyboard.SetTarget(fisb, nw);
                //    fisb.Begin();
                //}
                FadeOut_Completed(this, EventArgs.Empty);
            });
        }

        private void ContentControl_DataContextChanged(object sender, System.EventArgs e) {
            SetWindowToBottomRightOfScreen();
        }
        private void MpAvNotificationWindow_Closed(object sender, System.EventArgs e) {
        }



        private void MpAvNotificationWindow_Opened(object sender, System.EventArgs e) {
            if(MpAvMainWindowViewModel.Instance.IsMainWindowOpen && MpAvMainWindow.Instance.Topmost) {
                MpAvMainWindow.Instance.Topmost = false;
            }
            UpdateWindows();
        }
        public void SetDataContext(object dataContext) {
            DataContext = dataContext;
        }
    }
}
