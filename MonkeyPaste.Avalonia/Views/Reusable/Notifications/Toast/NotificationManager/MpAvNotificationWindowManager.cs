using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using Avalonia;
using MonkeyPaste.Common.Avalonia;
using Org.BouncyCastle.Asn1.Mozilla;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvNotificationWindowManager : MpINotificationManager {
        #region Private Variables

        private ObservableCollection<Window> _windows = new ObservableCollection<Window>();

        //private Window _wvMessageWindow;

        #endregion

        #region Statics
        private static MpAvNotificationWindowManager _instance;
        public static MpAvNotificationWindowManager Instance => _instance ?? (_instance = new MpAvNotificationWindowManager());


        #endregion

        #region MpINotificationManager Implementation
        public void ShowNotification(MpNotificationViewModelBase nvmb) {
            if (nvmb == null) {
                // somethigns wrong
                Debugger.Break();
            }

            Dispatcher.UIThread.Post(async() => {
                Window nw = null;
                var layoutType = MpNotificationViewModelBase.GetLayoutTypeFromNotificationType(nvmb.NotificationType);
                switch(layoutType) {
                    case MpNotificationLayoutType.Loader:
                        nw = new MpAvLoaderNotificationWindow();
                        nw.DataContext = nvmb;
                        break;
                    case MpNotificationLayoutType.ErrorWithOption:
                    case MpNotificationLayoutType.WarningWithOption:
                    case MpNotificationLayoutType.ErrorAndShutdown:
                        nw = new MpAvUserActionNotificationWindow();
                        nw.DataContext = nvmb;
                        break;
                    default:
                        if(nvmb.BodyFormat == MpTextContentFormat.RichHtml) {
                            nw = MpAvMessageNotificationWindow.WebViewInstance;
                        } else {
                            nw = new MpAvMessageNotificationWindow();
                            nw.DataContext = nvmb;
                        }
                        break;
                }
                if(nw == null) {
                    // somethings wrong
                    Debugger.Break();
                }
                if(_windows.Contains(nw)) {
                    _windows.Add(nw);
                }
                
                nw.Closed += Nw_Closed;
                nw.DataContextChanged += Nw_DataContextChanged;
                nw.EffectiveViewportChanged += Nw_EffectiveViewportChanged;
                nw.PointerReleased += Nw_PointerReleased;
                nw.Deactivated += Nw_Deactivated;

                nw.GetObservable(Window.BoundsProperty).Subscribe(value => OnNotificationWindowBoundsChangedHandler(nw));
                nw.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnNotificationWindowIsVisibleChangedHandler(nw));
                nw.GetObservable(Window.TopmostProperty).Subscribe(value => OnNotificationWindowTopmostChanged(nw));

                //if (nw == MpAvMessageNotificationWindow.WebViewInstance &&
                //    nw.DataContext != nvmb) {
                //    // how did it get changed?
                //    Debugger.Break();
                //}
                //if (nw != MpAvMessageNotificationWindow.WebViewInstance) {
                //    // wv has fixed data context
                //    nw.DataContext = nvmb;
                //}
                //nw.DataContext = nvmb;
                var cb = nw.FindControl<Button>("CloseButton");
                if(cb != null) {
                    cb.Click += CloseButton_Click;
                }
                if (App.Desktop.MainWindow == null) {
                    // occurs on startup
                    App.Desktop.MainWindow = nw;
                } else {
                    //App.Desktop.MainWindow.Topmost = false;
                }

                if (nw == MpAvMessageNotificationWindow.WebViewInstance &&
                    nw.DataContext is MpMessageNotificationViewModel mnvm) {
                    var wv = nw.GetVisualDescendant<MpAvCefNetWebView>();
                    if(!wv.IsContentLoaded) {
                        await mnvm.InitializeAsync(nvmb.NotificationFormat);

                        await wv.PerformLoadContentRequestAsync();
                    }
                    
                }

                BeginOpen(nw);
                
            });
        }

        public void HideNotification(MpNotificationViewModelBase nvmb) {
            var wl = _windows.Where(x => x.DataContext == nvmb).ToList();
            if (wl.Count != 1) {
                // equality conflict?
                Debugger.Break();

            }
            if (wl.Count > 0) {
                if (nvmb is MpUserActionNotificationViewModel uanvm) {
                    FinishClose(wl[0]);
                    return;
                }
                // this triggers fade out which ends w/ IsVisible=false
                nvmb.IsClosing = true;
            }
        }

        #endregion

        #region Properties

        public bool IsAnyNotificationVisible => _windows.Any(x => x.IsVisible);

        public bool IsAnyNotificationActive => _windows.Any(x => x.IsActive);
        public Window HeadNotificationWindow {
            get {
                if(!IsAnyNotificationVisible) {
                    return null;
                }
                return _windows.Aggregate((a, b) => a.Position.Y < b.Position.Y ? a : b);
            }
        }

        #endregion

        #region Constructors
        private MpAvNotificationWindowManager() {
            _windows.CollectionChanged += _windows_CollectionChanged;
        }
        #endregion

        #region Public Methods

        public async Task InitAsync() {
            await MpAvMessageNotificationWindow.CreateWebViewInstanceAsync();
            if(MpAvMessageNotificationWindow.WebViewInstance != null) {
                MpAvMessageNotificationWindow.WebViewInstance.Show();
            }
        }

        public void UpdateTopmost(Window newVisibleWindow = null, bool wasDeactivated = false) {
            var w = newVisibleWindow == null ? HeadNotificationWindow : newVisibleWindow;
            if(w == null) {
                return;
            }
            if(MpAvMainWindow.Instance != null) {
                MpAvMainWindow.Instance.Topmost = false;
            }
            
            //if (!wasDeactivated) {

            //}
            w.Activate();
            w.Topmost = true;
            w.Focus();

            w.Activate();
            w.Topmost = true;
            w.Focus();

        }
        #endregion

        #region Private Methods

        private void _windows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateWindowPositions();
            //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = _windows.Count > 0;
        }
        private void UpdateWindowPositions() {
            _windows.ForEach(x => PositionWindowByNotificationType(x));
        }

        private void BeginOpen(Window nw) {
            if (!_windows.Contains(nw)) {
                _windows.Add(nw);
            }

            var nvmb = nw.DataContext as MpNotificationViewModelBase;
            if (nvmb.IsModal) {
                bool wasLocked = MpAvMainWindowViewModel.Instance.IsMainWindowLocked;
                if (!wasLocked) {
                    MpAvMainWindowViewModel.Instance.ToggleMainWindowLockCommand.Execute(null);
                }

                nw.Show();
                if (!wasLocked) {
                    MpAvMainWindowViewModel.Instance.ToggleMainWindowLockCommand.Execute(null);
                }
            } else {
                nw.Show();
            }

            UpdateWindowPositions();
        }

        private void FinishClose(Window w) {
            //var nvmb = w.DataContext as MpNotificationViewModelBase;
            if (w.DataContext is MpAvLoaderNotificationWindow) {
                // ignore so bootstrapper can swap main window
            } else if (w == MpAvMessageNotificationWindow.WebViewInstance) {
                w.Hide();
            } else {
                w.Close();
            }
            _windows.Remove(w);
        }

        #region Window Events

        private void Nw_Deactivated(object sender, EventArgs e) {
            var w = sender as Window;
            w.Topmost = false;
            w.Topmost = true;
            //UpdateTopmost(w, true);
        }

        private void Nw_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            if(sender == MpAvMessageNotificationWindow.WebViewInstance) {
                return;
            }
            if (MpAvMainWindow.Instance == null || !MpAvMainWindow.Instance.IsInitialized) {
                return;
            }
            if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }
            MpAvMainWindowViewModel.Instance.ShowWindowCommand.Execute(null);
        }

        private void Nw_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            var w = sender as Window;
            
            UpdateWindowPositions();
        }

        private void Nw_DataContextChanged(object sender, EventArgs e) {
            var w = sender as Window;
            if (w.DataContext is MpNotificationViewModelBase nvmb) {
                nvmb.OnPropertyChanged(nameof(nvmb.IconSourceStr));
            }
            UpdateWindowPositions();
        }

        private void Nw_Closed(object sender, EventArgs e) {
            //MpConsole.WriteLine($"fade out complete for: '{(sender as Control).DataContext}'");
            
            var w = sender as Window;
            FinishClose(w);
        }


        private void CloseButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            //HideWindow(DataContext as MpNotificationViewModelBase);
            var control = sender as Control;
            if(control.GetVisualAncestor<Window>() is Window w &&
                w.DataContext is MpNotificationViewModelBase nvmb) {
                HideNotification(nvmb);
            }
        }

        private void OnNotificationWindowTopmostChanged(Window w) {
            if(!w.IsVisible) {
                return;
            }
            if(_windows.All(x=>!x.Topmost)) {
                w.Topmost = true;
            }
        }

        private void OnNotificationWindowBoundsChangedHandler(Window w) {
            if (!_windows.Contains(w)) {
                _windows.Add(w);
            }
            UpdateWindowPositions();
        }

        private void OnNotificationWindowIsVisibleChangedHandler(Window w) {
            if(w.IsVisible) {
                w.Activate();
                UpdateTopmost(w);
                return;
            }
            if (!w.IsVisible && w.DataContext is MpNotificationViewModelBase nvmb && nvmb.IsClosing) {
                FinishClose(w);
            }

        }

        #endregion

        #region Positioning

        public void PositionWindowByNotificationType(Window w) {
            var nvmb = w.DataContext as MpNotificationViewModelBase;
            if (nvmb == null) {
                return;
            }

            MpNotificationPlacementType placement = nvmb.PlacementType;
            switch (placement) {
                case MpNotificationPlacementType.SystemTray:
                    PositionWindowToSystemTray(w);
                    return;
                case MpNotificationPlacementType.CenterActiveScreen:
                    PositionWindowCenterActiveScreen(w);
                    return;
            }
        }

        private void PositionWindowCenterActiveScreen(Window w) {
            var s = SetupSize(w);

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
            w.Position = window_position.ToAvPixelPoint(primaryScreen.PixelDensity);
        }

        private void PositionWindowToSystemTray(Window w) {
            // TODO this should somehow know where system tray is on device, it just assumes its bottom right (windows)
            var s = SetupSize(w);


            var primaryScreen = new MpAvScreenInfoCollection().Screens.FirstOrDefault(x => x.IsPrimary); 
            if (primaryScreen == null) {
                // happens before loader attached
                return;
            }
            double pad = 10;

            double x = primaryScreen.WorkArea.Right - s.Width - pad;
            double offsetY = _windows.Where(x => _windows.IndexOf(x) < _windows.IndexOf(w)).Sum(x => x.Bounds.Height + pad);
            offsetY += s.Height + pad;
            double y = primaryScreen.WorkArea.Bottom - offsetY;

            //if(OperatingSystem.IsWindows()) 
            {
                x *= primaryScreen.PixelDensity;
                y *= primaryScreen.PixelDensity;
            }

            // when y is less than 0 i think it screws up measuring mw dimensions so its a baby
            y = Math.Max(0, y);

            w.Position = new PixelPoint((int)x, (int)y);
            //MpConsole.WriteLine($"Notification Idx {_windows.IndexOf(this)} density {primaryScreen.PixelDensity} x {this.Position.X} y {this.Position.Y}  width {s.Width} height {s.Height}");
        }

        private MpSize SetupSize(Window w) {
            double width = w.Width.IsNumber() && w.Bounds.Width != 0 ? w.Bounds.Width : 350;
            double height = w.Bounds.Height.IsNumber() && w.Bounds.Height != 0 ? w.Bounds.Height : 150;
            return new MpSize(width, height);
        }
        #endregion


        #endregion

        #region Commands
        #endregion
    }
}
