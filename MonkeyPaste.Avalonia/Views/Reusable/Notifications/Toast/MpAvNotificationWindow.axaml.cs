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
using MonkeyPaste.Common.Avalonia;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvNotificationWindow : Window, MpINotificationBalloonView, MpINotificationViewer {
        #region Statics


        private static ObservableCollection<MpAvNotificationWindow> _windows;

        static MpAvNotificationWindow() {
            _windows = new ObservableCollection<MpAvNotificationWindow>();
            _windows.CollectionChanged += _windows_CollectionChanged;
        }

        private static MpAvNotificationWindow _instance;
        public static MpAvNotificationWindow Instance => _instance ?? (_instance = new MpAvNotificationWindow());

        private static void _UpdateWindows() {
            _windows.ForEach(x => x.PositionWindowByNotificationType());
        }

        private static void _ShowWindow(MpAvNotificationWindow callingWindow, object dc) {
            var nvmb = dc as MpNotificationViewModelBase;
            if (nvmb == null) {
                // somethigns wrong
                Debugger.Break();
            }

            Dispatcher.UIThread.Post(() => {
                MpAvNotificationWindow nw = null;

                if (App.Desktop.MainWindow == null) {
                    // occurs on startup
                    nw = callingWindow;
                    App.Desktop.MainWindow = nw;
                } else {
                    nw = new MpAvNotificationWindow();
                }
                nw.DataContext = nvmb;

                //nw.Opacity = 1;

                if (nvmb.IsModal) {
                    bool wasLocked = MpAvMainWindowViewModel.Instance.IsMainWindowLocked;
                    if(!wasLocked) {
                        MpAvMainWindowViewModel.Instance.ToggleMainWindowLockCommand.Execute(null);
                    }
                    
                    nw.Show();
                    if (!wasLocked) {
                        MpAvMainWindowViewModel.Instance.ToggleMainWindowLockCommand.Execute(null);
                    }
                } else {
                    nw.Show();
                }

                _UpdateWindows();
            });
        }

        private static void _HideWindow(object dc) {
            var nvmb = dc as MpNotificationViewModelBase;
            if (nvmb == null) {
                // somethigns wrong
                Debugger.Break();
            } 
            if(nvmb is MpUserActionNotificationViewModel uanvm) {
                var uaw = _windows.FirstOrDefault(x => x.DataContext == uanvm);
                uaw.Close();
                return;
            }
            // this triggers fade out which ends w/ IsVisible=false
            nvmb.IsClosing = true;
        }

        private static void _windows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            _UpdateWindows();
            MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = _windows.Count > 0;
            if (!MpAvMainWindowViewModel.Instance.IsAnyDialogOpen) {
                _instance = null;
            }
        }
        #endregion

        #region MpINotificationBalloonView Implementation

        public void ShowWindow(object dc) {
            _ShowWindow(this,dc);
        }

        public void HideWindow(object dc) {
            Dispatcher.UIThread.Post(() => {
                _HideWindow(dc);
            });
        }

        public void SetDataContext(object dataContext) {
            DataContext = dataContext;
        }

        #endregion

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
            
            this.Opened += MpAvNotificationWindow_Opened;
            this.Closed += MpAvNotificationWindow_Closed;
            this.EffectiveViewportChanged += MpAvNotificationWindow_EffectiveViewportChanged;

            var nwcg = this.FindControl<Control>("NotificationWindowContainerGrid");
            nwcg.PointerReleased += MpAvNotificationWindow_PointerReleased;

            var ncc = this.FindControl<Control>("NotificationContentControl");
            ncc.DataContextChanged += ContentControl_DataContextChanged;

            var cb = this.FindControl<Button>("CloseButton");
            cb.Click += CloseButton_Click;

            this.GetObservable(Window.BoundsProperty).Subscribe(value => MpAvNotificationWindow_BoundsChangedHandler());
            this.GetObservable(Window.IsVisibleProperty).Subscribe(value => MpAvNotificationWindow_IsVisibleChangedHandler());
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        #region Event Handlers
        private void MpAvNotificationWindow_Closed(object sender, System.EventArgs e) {
            //MpConsole.WriteLine($"fade out complete for: '{(sender as Control).DataContext}'");
            this.Hide();
            _windows.Remove(this);
        }

        private void MpAvNotificationWindow_Opened(object sender, System.EventArgs e) {
            if (MpAvMainWindowViewModel.Instance.IsMainWindowOpen && 
                MpAvMainWindow.Instance.Topmost) {
                MpAvMainWindow.Instance.Topmost = false;
            }
            _UpdateWindows();
        }
        private void ContentControl_DataContextChanged(object sender, System.EventArgs e) {
            //int this_idx = _windows.IndexOf(this);
            //PositionWindowToSystemTray(this_idx);
            var cc = sender as ContentControl;
            if (cc.DataContext is MpNotificationViewModelBase nvmb) {
                nvmb.OnPropertyChanged(nameof(nvmb.IconSourceStr));
            }
            _UpdateWindows();
        }

        private void CloseButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            //HideWindow(DataContext as MpNotificationViewModelBase);
            this.GetVisualAncestor<Window>().Close();
        }

        private void MpAvNotificationWindow_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            _UpdateWindows();
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

        private void MpAvNotificationWindow_BoundsChangedHandler() {
            if(!_windows.Contains(this)) {
                _windows.Add(this);
            }
            _UpdateWindows();
        }

        private void MpAvNotificationWindow_IsVisibleChangedHandler() {
            if(this.IsVisible) {
                MpAvNotificationWindow_Opened(this, null);
                return;
            }
            if(this.DataContext is MpNotificationViewModelBase nvmb && nvmb.IsClosing) {
                MpAvNotificationWindow_Closed(this, null);
            }
            
        }

        #endregion


        private void PositionWindowByNotificationType() {
            var nvmb = DataContext as MpNotificationViewModelBase;
            if(nvmb == null) {
                return;
            }

            MpNotificationPlacementType placement = nvmb.PlacementType;
            switch(placement) {
                case MpNotificationPlacementType.SystemTray:
                    PositionWindowToSystemTray();
                    return;
                case MpNotificationPlacementType.CenterActiveScreen:
                    PositionWindowCenterActiveScreen();
                    return;
            }
        }

        private void PositionWindowCenterActiveScreen() {
            var s = SetupSize();

            var primaryScreen = new MpAvScreenInfoCollection().Screens.FirstOrDefault(x => x.IsPrimary);
            if (primaryScreen == null) {
                // happens before loader attached
                return;
            }
            double screen_mid_x = primaryScreen.WorkArea.Left + ((primaryScreen.WorkArea.Right - primaryScreen.WorkArea.Left) / 2);
            double screen_mid_y = primaryScreen.WorkArea.Top + ((primaryScreen.WorkArea.Bottom - primaryScreen.WorkArea.Top) / 2);

            double window_hw = s.Width / 2;
            double window_hh = s.Height / 2;

            MpPoint window_position = new MpPoint(screen_mid_x - window_hw, screen_mid_y - window_hh);
            this.Position = window_position.ToAvPixelPoint(primaryScreen.PixelDensity);
            //MpConsole.WriteLine($"Notification Idx {_windows.IndexOf(this)} density {primaryScreen.PixelDensity} x {this.Position.X} y {this.Position.Y}  width {s.Width} height {s.Height}");
        }

        private void PositionWindowToSystemTray() {
            // TODO this should somehow know where system tray is on device, it just assumes its bottom right (windows)
            var s = SetupSize();


            var primaryScreen = new MpAvScreenInfoCollection().Screens.FirstOrDefault(x => x.IsPrimary); //this.PlatformImpl.Screen.AllScreens.FirstOrDefault(x => x.Primary);
            if(primaryScreen == null) {
                // happens before loader attached
                return;
            }
            double pad = 10;
            
            double x = primaryScreen.WorkArea.Right - s.Width - pad;
            double offsetY = _windows.Where(x => _windows.IndexOf(x) < _windows.IndexOf(this)).Sum(x => x.Bounds.Height + pad);
            offsetY += s.Height + pad;
            double y = primaryScreen.WorkArea.Bottom - offsetY;

            //if(OperatingSystem.IsWindows()) 
            {
                x *= primaryScreen.PixelDensity;
                y *= primaryScreen.PixelDensity;
            }

            // when y is less than 0 i think it screws up measuring mw dimensions so its a baby
            y = Math.Max(0, y);

            this.Position = new PixelPoint((int)x, (int)y);
            MpConsole.WriteLine($"Notification Idx {_windows.IndexOf(this)} density {primaryScreen.PixelDensity} x {this.Position.X} y {this.Position.Y}  width {s.Width} height {s.Height}");
        }

        private MpSize SetupSize() {
            //double w, h;
            //if (DataContext is MpAvLoaderNotificationView) {
            //    w = 350;
            //    h = 200;
            //    this.MaxHeight = h;
            //    this.MaxWidth = w;
            //    this.Width = w;
            //    this.Height = h;

            //    //this.SizeToContent = SizeToContent.Manual;
            //} else {
            //    w = 350;
            //    h = 200;
            //    this.MinWidth = 350;
            //    this.MaxWidth = 800;
            //    this.MinHeight = 200;
            //    this.MaxHeight = 500;
            //    //this.SizeToContent = SizeToContent.Manual;
            //}
            //this.InvalidateAll();
            //return new MpSize(350, 200);

            double w = this.Bounds.Width.IsNumber() && this.Bounds.Width != 0 ? this.Bounds.Width : 350;
            double h = this.Bounds.Height.IsNumber() && this.Bounds.Height != 0 ? this.Bounds.Height : 150;
            return new MpSize(w, h);
        }

        public Task<MpNotificationDialogResultType> ShowNotificationAsync(MpNotificationFormat nf) {
            throw new NotImplementedException();
        }
    }
}
