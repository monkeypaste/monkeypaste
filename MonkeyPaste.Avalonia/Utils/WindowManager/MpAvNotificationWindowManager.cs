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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvNotificationWindowManager : MpINotificationManager {
        #region Private Variables
        private List<MpAvMessageNotificationWindow> _pendingMessages = new List<MpAvMessageNotificationWindow>();
        private ObservableCollection<Window> _windows = new ObservableCollection<Window>();
        #endregion

        #region Statics

        private static MpAvNotificationWindowManager _instance;
        public static MpAvNotificationWindowManager Instance => _instance ?? (_instance = new MpAvNotificationWindowManager());


        #endregion

        #region Interfaces

        #region MpINotificationManager Implementation
        public void ShowNotification(MpNotificationViewModelBase nvmb) {
            if (nvmb == null) {
                // somethigns wrong
                MpDebug.Break();
            }
            if (Mp.Services.PlatformInfo.IsDesktop) {
                ShowDesktopNotification(nvmb);
            } else {
                if (nvmb is MpLoaderNotificationViewModel lnvm) {
                    Dispatcher.UIThread.Post(async () => {

                        await lnvm.ProgressLoader.BeginLoaderAsync();
                        await lnvm.ProgressLoader.FinishLoaderAsync();
                    });
                }
            }
        }
        public void HideNotification(MpNotificationViewModelBase nvmb) {
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
                return _windows.Aggregate((a, b) => a.Position.Y < b.Position.Y ? a : b);
            }
        }

        #endregion

        #region Events

        public event EventHandler<Window> OnNotificationWindowIsVisibleChanged;

        #endregion

        #region Constructors
        private MpAvNotificationWindowManager() { }
        #endregion

        #region Public Methods
        public void Init() {
            _ = new MpAvNotificationPositioner();
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }


        #endregion

        #region Private Methods
        private void ShowDesktopNotification(MpNotificationViewModelBase nvmb) {
            Dispatcher.UIThread.Post(() => {
                MpAvWindow nw = null;
                var layoutType = MpNotificationViewModelBase.GetLayoutTypeFromNotificationType(nvmb.NotificationType);
                switch (layoutType) {
                    case MpNotificationLayoutType.Loader:
                        nw = new MpAvLoaderNotificationWindow() {
                            DataContext = nvmb
                        };
                        break;
                    case MpNotificationLayoutType.ErrorWithOption:
                    case MpNotificationLayoutType.UserAction:
                    case MpNotificationLayoutType.ErrorAndShutdown:
                        nw = new MpAvUserActionNotificationWindow() {
                            DataContext = nvmb
                        };
                        break;
                    //case MpNotificationLayoutType.Append:
                    //    nw = MpAvAppendNotificationWindow.Instance;
                    //    break;
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
                MpAvToolWindow_Win32.InitToolWindow(nw.TryGetPlatformHandle().Handle);
#endif
                nw.Closed += Nw_Closed;

                nw.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnNotificationWindowIsVisibleChangedHandler(nw));

                BeginOpen(nw);
            });
        }
        private void BeginOpen(MpAvWindow nw) {
            var nvmb = nw.DataContext as MpNotificationViewModelBase;
            nvmb.IsClosing = false;

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                    desktop.MainWindow == null) {
                // occurs on startup
                desktop.MainWindow = nw;
            }

            if (nvmb.Owner is Window w &&
                nvmb.AnchorTarget == null) {
                // let anchor to precedence over owner for positioning
                nw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            if (!_windows.Contains(nw)) {
                _windows.Add(nw);
            }
            bool is_platform_loaded = Mp.Services != null &&
                     Mp.Services.StartupState != null &&
                     Mp.Services.StartupState.IsPlatformLoaded;
            if (is_platform_loaded) {
                // flag ntf activating to prevent mw hide 
                MpAvMainWindowViewModel.Instance.IsAnyNotificationActivating = true;
            }
            if (nw is MpAvMessageNotificationWindow) {
                MpIPlatformScreenInfo primaryScreen = Mp.Services.ScreenInfoCollection.Screens.FirstOrDefault(x => x.IsPrimary);
                if (primaryScreen != null) {
                    // since msgs slide out and window positioning is handled after opening (to account for its size)
                    // messages need to start off screen or it will blink when first shown
                    nw.Position = primaryScreen.Bounds.BottomRight.ToAvPixelPoint(primaryScreen.Scaling);
                }
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
            return;
        }

        private void FinishClose(Window w) {
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

            var w = sender as Window;
            FinishClose(w);
        }


        private async void OnNotificationWindowIsVisibleChangedHandler(Window w) {
            OnNotificationWindowIsVisibleChanged?.Invoke(this, w);
            if (w.IsVisible) {
                await Task.Delay(1000);
                MpAvMainWindowViewModel.Instance.IsAnyNotificationActivating = false;
                return;
            }
            if (!w.IsVisible &&
                w.DataContext is MpNotificationViewModelBase nvmb &&
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
