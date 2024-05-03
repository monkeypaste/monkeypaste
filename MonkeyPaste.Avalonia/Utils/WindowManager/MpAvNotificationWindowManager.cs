using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvNotificationWindowManager {
        #region Private Variables
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
                ShowWindowedNotification(nvmb);
            }
        }
        public void HideNotification(object dc) {
            if (MpAvWindowManager.LocateWindow(dc) is not MpAvWindow w) {
                return;
            }
            w.Close();
        }

        #endregion

        #endregion

        #region Properties

        #endregion

        #region Constructors
        private MpAvNotificationWindowManager() { }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        private void ShowWindowedNotification(MpAvNotificationViewModelBase nvmb) {
            Control nw = null;
            var layoutType = MpAvNotificationViewModelBase.GetLayoutTypeFromNotificationType(nvmb.NotificationType);
            switch (layoutType) {
                case MpNotificationLayoutType.Welcome:
                    //nw = new MpAvWelcomeWindow() {
                    //    DataContext = nvmb
                    //};
                    break;
                case MpNotificationLayoutType.Loader:
                    var mlv = new MpAvMobileLoaderView() {
                        DataContext = nvmb,
                        //Width = Mp.Services.ScreenInfoCollection.Screens.FirstOrDefault(x=>x.IsPrimary).WorkArea.Width,
                        //Height = Mp.Services.ScreenInfoCollection.Screens.FirstOrDefault(x=>x.IsPrimary).WorkArea.Height
                    };
                    App.SetPrimaryView(mlv);
                    break;
                case MpNotificationLayoutType.ErrorWithOption:
                case MpNotificationLayoutType.UserAction:
                case MpNotificationLayoutType.ErrorAndShutdown:
                    //nw = new MpAvUserActionNotificationWindow() {
                    //    DataContext = nvmb
                    //};
                    break;
                default:
                    //nw = new MpAvMessageNotificationWindow() {
                    //    DataContext = nvmb,
                    //};
                    break;
            }
            if (nw == null) {
                return;
            }
        }
        private void ShowDesktopNotification(MpAvNotificationViewModelBase nvmb) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => ShowDesktopNotification(nvmb));
                return;
            }
            // BUG setting owner seems locks everything up, don't know
            // if its or avalonia but just ignoring it for now
            MpAvWindow nw = null;
            switch (nvmb) {
                case MpAvWelcomeNotificationViewModel:
                    nw = new MpAvWelcomeWindow() {
                        DataContext = nvmb
                    };
                    break;
                case MpAvLoaderNotificationViewModel:
                    nvmb.IsVisible = true;
                    nw = new MpAvLoaderNotificationWindow() {
                        DataContext = nvmb,
                        Topmost = true,
                        ShowActivated = true
                    };

#if WINDOWED
                    App.Current.SetMainWindow(MpAvRootWindow.Instance);
#else
                    App.Current.SetMainWindow(nw); 
#endif
                    break;
                default:
                    nw = new MpAvUserActionNotificationWindow() {
                        DataContext = nvmb,
                    };
                    break;
            }
            if (nw == null) {
                // somethings wrong
                return;
            }

#if WINDOWS

            if (nvmb is not MpAvWelcomeNotificationViewModel) {

                MpAvToolWindow_Win32.SetAsToolWindow(nw.TryGetPlatformHandle().Handle);
            }
#endif
            BeginOpen(nw);
        }
        private void BeginOpen(MpAvWindow nw) {
            var nvmb = nw.DataContext as MpAvNotificationViewModelBase;

            if(nvmb.Title.IsNullOrEmpty()) {
                // give title for debugging/compliance
                nvmb.Title = nvmb.ToString().ToWindowTitleText();
            }
            if (nvmb.CanMoveWindow) {
                MpAvMoveWindowExtension.SetIsEnabled(nw, true);
            }

            nvmb.Owner = MpAvWindowManager.CurrentOwningWindow;
            if (nvmb.AnchorTarget == null && MpAvFocusManager.Instance.FocusElement is Visual v) {
                nvmb.AnchorTarget = v;
            }

            if (nvmb.IsModal) {
                if (nvmb.Owner == null) {
                    nw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    nvmb.AnchorTarget = null;
                } else {
                    nw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
            } else {
                nvmb.Owner = null;
                nvmb.AnchorTarget = null;
                if (!nw.Classes.Contains("toast")) {
                    nw.Classes.Add("toast");
                }
                nw.WindowStartupLocation = WindowStartupLocation.Manual;
                nw.Position = MpAvWindowPositioner.GetSystemTrayWindowPosition(nw);
            }
            if (nvmb.AnchorTarget != null) {
                nw.WindowStartupLocation = WindowStartupLocation.Manual;
            }

            var disp = nw.GetObservable(Control.BoundsProperty).Subscribe(value => PositionAllNtfs());

            void Nw_EffectiveViewportChanged(object sender, EffectiveViewportChangedEventArgs e) {
                PositionAllNtfs();
            }

            void OnWindowClosed(object sender, EventArgs e) {
                nw.Closed -= OnWindowClosed;
                nw.EffectiveViewportChanged -= Nw_EffectiveViewportChanged;
                disp.Dispose();
                PositionAllNtfs();
            }
            nw.Closed += OnWindowClosed;
            nw.EffectiveViewportChanged += Nw_EffectiveViewportChanged;
            try {
                if (nvmb.Owner != null) {
                    nw.Show(nvmb.Owner);
                } else {
                    nw.Show();
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error showing window '{nvmb}', window likely closed. ", ex);
            }
            return;
        }
        private void PositionAllNtfs() {
            foreach (var nw in MpAvWindowManager.Notifications) {
                if (nw.WindowStartupLocation != WindowStartupLocation.Manual ||
                    nw.DataContext is not MpAvNotificationViewModelBase nvmb) {
                    continue;
                }
                if (nvmb.IsToast) {
                    nw.Position = MpAvWindowPositioner.GetSystemTrayWindowPosition(nw);
                } else {
                    nw.Position = MpAvWindowPositioner.GetWindowPositionByAnchorVisual(nw, nvmb.AnchorTarget);
                }
            }
        }

        #endregion

        #region Commands
        #endregion
    }
}
