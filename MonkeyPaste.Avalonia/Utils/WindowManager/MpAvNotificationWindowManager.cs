using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvNotificationWindowManager : MpINotificationManager {
        #region Private Variables
        private List<MpAvMessageNotificationWindow> _pendingMessages = new List<MpAvMessageNotificationWindow>();
        private ObservableCollection<Window> _windows = new ObservableCollection<Window>();
        private MpAvNotificationPositioner _positioner;
        #endregion

        #region Statics

        private static MpAvNotificationWindowManager _instance;
        public static MpAvNotificationWindowManager Instance => _instance ?? (_instance = new MpAvNotificationWindowManager());


        #endregion

        #region Interfaces

        #region MpINotificationManager Implementation
        public void ShowNotification(object dc) {
            var nvmb = dc as MpAvNotificationViewModelBase;
            if (nvmb == null) {
                // somethigns wrong
                MpDebug.Break();
            }
            if (Mp.Services.PlatformInfo.IsDesktop) {
                ShowDesktopNotification(nvmb);
            } else {
                // TODO need to merge or handle mobile ntf
                if (nvmb is MpAvLoaderNotificationViewModel lnvm) {
                    Dispatcher.UIThread.Post(async () => {

                        await lnvm.ProgressLoader.BeginLoaderAsync();
                        await lnvm.ProgressLoader.FinishLoaderAsync();
                    });
                }
            }
        }
        public void HideNotification(object dc) {
            if (dc is not MpAvNotificationViewModelBase nvmb) {
                return;
            }
            nvmb.IsClosing = true;
        }

        #endregion

        #endregion

        #region Properties


        public bool IsAnyNotificationVisible => _windows.Any(x => x.IsVisible);

        public bool IsAnyNotificationActive => _windows.Any(x => x.IsActive);
        public Window HeadNotificationWindow {
            get {
                if (!IsAnyNotificationVisible) {
                    return null;
                }
                return _windows.AggregateOrDefault((a, b) => a.Position.Y < b.Position.Y ? a : b);
            }
        }

        #endregion

        #region Events

        //public event EventHandler<Window> OnNotificationWindowIsVisibleChanged;

        #endregion

        #region Constructors
        private MpAvNotificationWindowManager() {
            _positioner = new MpAvNotificationPositioner();
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        private void ShowDesktopNotification(MpAvNotificationViewModelBase nvmb) {
            Dispatcher.UIThread.Post(() => {
                MpAvWindow nw = null;
                var layoutType = MpAvNotificationViewModelBase.GetLayoutTypeFromNotificationType(nvmb.NotificationType);
                switch (layoutType) {
                    case MpNotificationLayoutType.Welcome:
                        nw = new MpAvWelcomeWindow() {
                            DataContext = nvmb
                        };
                        break;
                    case MpNotificationLayoutType.Loader:
                        nw = new MpAvLoaderNotificationWindow() {
                            DataContext = nvmb
                        };
                        if (Mp.Services.PlatformInfo.IsDesktop) {
                            App.Current.SetMainWindow(nw);
                        }
                        break;
                    case MpNotificationLayoutType.ErrorWithOption:
                    case MpNotificationLayoutType.UserAction:
                    case MpNotificationLayoutType.ErrorAndShutdown:
                        nw = new MpAvUserActionNotificationWindow() {
                            DataContext = nvmb
                        };
                        break;
                    default:
                        nw = new MpAvMessageNotificationWindow() {
                            DataContext = nvmb,
                        };
                        break;
                }
                if (nw == null) {
                    // somethings wrong
                    MpDebug.Break();
                }

#if WINDOWS

                if (nvmb is not MpAvWelcomeNotificationViewModel) {

                    MpAvToolWindow_Win32.InitToolWindow(nw.TryGetPlatformHandle().Handle);
                }
#endif
                nw.Closed += Nw_Closed;

                nw.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnNotificationWindowIsVisibleChangedHandler(nw));

                BeginOpen(nw);
            });
        }
        private void BeginOpen(MpAvWindow nw) {
            var nvmb = nw.DataContext as MpAvNotificationViewModelBase;
            nvmb.IsClosing = false;

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow is not MpAvMainWindow) {
                desktop.MainWindow = nw;
            }
            if (!nvmb.IsModal) {
                nw.Position = MpAvNotificationPositioner.GetSystemTrayWindowPosition(nw);
            }
            if (nvmb.CanMoveWindow) {
                MpAvMoveWindowExtension.SetIsEnabled(nw, true);
                if (nvmb.RejectedMoveControlTypes != null) {
                    MpAvMoveWindowExtension.SetRejectedControlTypeNames(nw, string.Join("|", nvmb.RejectedMoveControlTypes.Select(x => x.ToString())));
                }
            }
            if (nvmb.Owner is not Window &&
                nvmb.Owner is Control owner_c &&
                TopLevel.GetTopLevel(owner_c) is Window owner_w) {
                // owner is some control so swap to its window
                // and adjust startup to be center of that control
                if (owner_w.WindowState == WindowState.Minimized) {
                    // remove owner
                    nvmb.Owner = null;
                    nvmb.AnchorTarget = null;
                    nw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                } else {
                    nvmb.Owner = owner_w;
                    MpDebug.Assert(nvmb.AnchorTarget == null, $"Use owner not anchorTarget");
                    nvmb.AnchorTarget = owner_c;
                    nw.WindowStartupLocation = WindowStartupLocation.Manual;
                    nw.Opened += (s, e) => {
                        var anchor_s_origin = owner_c.PointToScreen(new Point());
                        var anchor_s_size = owner_c.Bounds.Size.ToAvPixelSize(owner_c.VisualPixelDensity());
                        var nw_s_size = nw.Bounds.Size.ToAvPixelSize(owner_c.VisualPixelDensity());
                        double nw_x = anchor_s_origin.X + (anchor_s_size.Width / 2) - (nw_s_size.Width / 2);
                        double nw_y = anchor_s_origin.Y + (anchor_s_size.Height / 2) - (nw_s_size.Height / 2);
                        var s_size = owner_w.Screens.ScreenFromVisual(owner_w).WorkingArea.Size;
                        nw_x = Math.Clamp(nw_x, 0, s_size.Width - nw_s_size.Width);
                        nw_y = Math.Clamp(nw_y, 0, s_size.Height - nw_s_size.Height);
                        nw.Position = new PixelPoint((int)nw_x, (int)nw_y);
                    };
                }

            }

            if (nvmb.Owner is Window w &&
                nvmb.AnchorTarget == null) {
                // let anchor to precedence over owner for positioning
                nw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            if (!_windows.Contains(nw)) {
                _windows.Add(nw);
                _positioner.AddWindow(nw);
            }
            bool is_platform_loaded = Mp.Services != null &&
                     Mp.Services.StartupState != null &&
                     Mp.Services.StartupState.IsPlatformLoaded;
            if (is_platform_loaded) {
                // flag ntf activating to prevent mw hide 
                MpAvMainWindowViewModel.Instance.IsAnyNotificationActivating = true;
            }
            if (nw.WindowStartupLocation != WindowStartupLocation.CenterScreen &&
                nw.WindowStartupLocation != WindowStartupLocation.CenterOwner &&
                nw.WindowStartupLocation != WindowStartupLocation.Manual
                ) {
                //MpIPlatformScreenInfo primaryScreen = Mp.Services.ScreenInfoCollection.Screens.FirstOrDefault(x => x.IsPrimary);
                //if (primaryScreen != null) {
                //    // since msgs slide out and window positioning is handled after opening (to account for its size)
                //    // messages need to start off screen or it will blink when first shown
                //    nw.Position = primaryScreen.Bounds.BottomRight.ToAvPixelPoint(primaryScreen.Scaling);
                //}
                nw.Position = MpAvNotificationPositioner.GetSystemTrayWindowPosition(nw);
            }
            try {
                if (nvmb.Owner is Window ow) {
                    nw.Show(ow);
                } else {
                    nw.Show();
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error showing window '{nvmb}', window likely closed. ", ex);
            }
            if (nw is MpAvLoaderNotificationWindow &&
                        MpAvWindowManager.AllWindows.FirstOrDefault(x => x.DataContext is MpAvWelcomeNotificationViewModel) is MpAvWindow wwv && wwv.DataContext is MpAvWelcomeNotificationViewModel wwvm) {
                //desktop.MainWindow = nw;
                // wait for loader to get set to mw before closing welcome
                wwvm.HideNotification();
                nw.Position = MpAvNotificationPositioner.GetSystemTrayWindowPosition(nw);
            }
            return;
        }

        private void FinishClose(MpAvWindow w) {
            if (w is MpAvLoaderNotificationWindow) {
                // ignore, in mainview ctor mainwindow is swapped and loader is closed
            }
            //else if (w == MpAvAppendNotificationWindow.Instance) {
            //    w.Hide();
            //} 
            else {
                try {
                    w.Close();
                }
                catch (NullReferenceException nex) {
                    MpConsole.WriteTraceLine($"Close window exception ({w.DataContext})", nex);
                }
            }
            _windows.Remove(w);
            _positioner.RemoveWindow(w);
        }

        //private async Task ForceTopmostAsync(MpAvWindow w) {
        //    while(true) {
        //        if(w == null) {
        //            break;
        //        }
        //        if()
        //    }
        //} 

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowOpening:
                    //if (MpAvAppendNotificationWindow.Instance != null &&
                    //    MpAvAppendNotificationWindow.Instance.IsVisible) {
                    //    HideNotification(MpAppendNotificationViewModel.Instance);
                    //}
                    break;

            }
        }

        #region Window Events


        private void Nw_Closed(object sender, EventArgs e) {
            //MpConsole.WriteLine($"fade out complete for: '{(sender as Control).DataContext}'");

            var w = sender as MpAvWindow;
            FinishClose(w);
        }


        private async void OnNotificationWindowIsVisibleChangedHandler(MpAvWindow w) {
            //OnNotificationWindowIsVisibleChanged?.Invoke(this, w);

            if (w.IsVisible) {
                await Task.Delay(1000);
                MpAvMainWindowViewModel.Instance.IsAnyNotificationActivating = false;
                return;
            }
            if (!w.IsVisible &&
                w.DataContext is MpAvNotificationViewModelBase nvmb &&
                nvmb.IsClosing) {
                FinishClose(w);
            }
        }

        #endregion

        #endregion

        #region Commands
        #endregion
    }
}
