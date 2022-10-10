using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;
using MonkeyPaste;
using MonkeyPaste.Common;
using System.Linq;
using Avalonia.Threading;
using System;
using PropertyChanged;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvNotificationWindow : Window, MpINotificationBalloonView {
        private static List<MpAvNotificationWindow> _windows = new List<MpAvNotificationWindow>();

        private static MpAvNotificationWindow _instance;
        public static MpAvNotificationWindow Instance => _instance ?? (_instance = new MpAvNotificationWindow());

        public MpAvNotificationWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            if(_instance == null) {
                _instance = this;
                if(MpPlatformWrapper.Services != null) {
                    // this is set in wrapper init but subsequent ref's from wrapper need
                    // a non-disposed window so this update the wrapper ref
                    MpPlatformWrapper.Services.NotificationView = _instance;
                }
            }

            _windows.Add(this);

            this.GetObservable(Window.BoundsProperty).Subscribe(value => BoundsChangedHandler());

        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void BoundsChangedHandler() {
            if(!_windows.Contains(this)) {
                _windows.Add(this);
            }
            UpdateWindows();
        }
        private void _windows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateWindows();
        }
        private static void UpdateWindows() {
            _windows.ForEach(x => x.SetWindowToBottomRightOfScreen());
            //int winCount = _windows.Count();
            //for (int i = 0; i < winCount;i++) {
            //    _windows[i].SetWindowToBottomRightOfScreen(i);
            //}
        }
        private void SetWindowToBottomRightOfScreen() {
            //if (this_idx < 0) {
            //    MpConsole.WriteLine("Cannot set window position, not in stack");
            //    return;
            //}
            //if(this_idx > 0) {
            //   // Debugger.Break();
            //}
            //var desc_y_windows = _windows.OrderByDescending(x => x.Position.Y).ToList();
            //int this_idx = desc_y_windows.IndexOf(this);


            var primaryScreen = new MpAvScreenInfoCollection().Screens.FirstOrDefault(x => x.IsPrimary); //this.PlatformImpl.Screen.AllScreens.FirstOrDefault(x => x.Primary);
            if(primaryScreen == null) {
                // happens before loader attached
                return;
            }
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

            // when y is less than 0 i think it screws up measuring mw dimensions so its a baby
            y = Math.Max(0, y);

            this.Position = new PixelPoint((int)x, (int)y);
            MpConsole.WriteLine($"Notification Idx {_windows.IndexOf(this)} density {primaryScreen.PixelDensity} x {this.Position.X} y {this.Position.Y}  width {this.Bounds.Width} height {this.Bounds.Height}");
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
            if(sender is MpAvNotificationWindow nw) {
                MpConsole.WriteLine("fade out complete on 1 ovf em");
                _windows.Remove(nw);
                if(nw.DataContext is MpLoaderNotificationViewModel) {
                    // closed in bootstrapper
                    nw.Hide();
                } else {
                    //nw.Close();
                    nw.Hide();
                }
                
                if (_windows.Count == 0) {
                    MpAvMainWindowViewModel.Instance.IsShowingDialog = false;
                    _instance = null;
                }
            }
            
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
                MpAvNotificationWindow nw = null;
                
                if(App.Desktop.MainWindow == null) {
                    // occurs on startup
                    nw = this; 
                    App.Desktop.MainWindow = nw;
                } else {
                    nw = new MpAvNotificationWindow();
                }
                nw.DataContext = dc;
                nw.Opacity = 1;

                if (dc is MpNotificationViewModelBase nvmb && !nvmb.IsVisible) {
                    nvmb.IsVisible = true;
                }

                nw.Show();

                //var fisb = nw.Resources["FadeIn"] as Storyboard;
                //Storyboard.SetTarget(fisb, nw);

                ////nw.Show();

                //fisb.Begin();

                FadeIn_Completed(nw, EventArgs.Empty);

                UpdateWindows();
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
                FadeOut_Completed(nw, EventArgs.Empty);
            });
        }

        private void ContentControl_DataContextChanged(object sender, System.EventArgs e) {
            //int this_idx = _windows.IndexOf(this);
            //SetWindowToBottomRightOfScreen(this_idx);
            var cc = sender as ContentControl;
            if(cc.DataContext is MpNotificationViewModelBase nvmb) {
                nvmb.OnPropertyChanged(nameof(nvmb.IconSourceObj));
            }
            UpdateWindows();
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
