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
    public class MpAvNotificationWindowManager {
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
                ShowMobileNotification(nvmb);
            }
        }
        public void HideNotification(object dc) {
            if (_windows.FirstOrDefault(x => x.DataContext == dc) is not MpAvWindow w) {
                return;
            }
            w.Close();
        }

        #endregion

        #endregion

        #region Properties

        #endregion

        #region Constructors
        private MpAvNotificationWindowManager() {
            _positioner = new MpAvNotificationPositioner();
        }
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        private void ShowMobileNotification(MpAvNotificationViewModelBase nvmb) {
            //if (!Dispatcher.UIThread.CheckAccess()) {
            //    Dispatcher.UIThread.Post(() => ShowMobileNotification(nvmb));
            //    return;
            //}
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

                    //#if WINDOWS
                    App.Current.SetMainWindow(nw);
                    //#endif

                    break;
                case MpAvUserActionNotificationViewModel:
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
                return;
            }

#if WINDOWS

            if (nvmb is not MpAvWelcomeNotificationViewModel) {

                MpAvToolWindow_Win32.SetAsToolWindow(nw.TryGetPlatformHandle().Handle);
            }
#endif
            nw.Closed += Nw_Closed;

            BeginOpen(nw);
        }
        private void BeginOpen(MpAvWindow nw) {
            var nvmb = nw.DataContext as MpAvNotificationViewModelBase;
            //nvmb.IsClosing = false;

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow is not MpAvMainWindow) {
                desktop.MainWindow = nw;
            }
            if (nvmb.CanMoveWindow) {
                MpAvMoveWindowExtension.SetIsEnabled(nw, true);
            }

            MpAvWindow owner = MpAvWindowManager.CurrentOwningWindow;
            Visual anchor = nvmb.AnchorTarget as Visual;
            if (anchor == null && MpAvFocusManager.Instance.FocusElement is Visual v) {
                anchor = v;
            }

            if (nvmb.IsModal) {
                if (owner == null) {
                    nw.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    anchor = null;
                } else {
                    nw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
            } else {
                owner = null;
                anchor = null;
                if (!nw.Classes.Contains("toast")) {
                    nw.Classes.Add("toast");
                }
                nw.WindowStartupLocation = WindowStartupLocation.Manual;
                nw.Position = MpAvNotificationPositioner.GetSystemTrayWindowPosition(nw);

            }
            if (anchor != null) {
                nw.WindowStartupLocation = WindowStartupLocation.Manual;
            }


            void OnWindowOpened(object sender, EventArgs e) {
                nw.Opened -= OnWindowOpened;
                if (anchor == null) {
                    if (!nvmb.IsModal) {
                        nw.Position = MpAvNotificationPositioner.GetSystemTrayWindowPosition(nw);
                    }
                    return;
                }
                nw.Position = MpAvNotificationPositioner.GetWindowPositionByAnchorVisual(nw, anchor);
            }
            nw.Opened += OnWindowOpened;
            nvmb.Owner = owner;

            if (!_windows.Contains(nw)) {
                _windows.Add(nw);
                _positioner.AddWindow(nw);
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

        #region Window Events


        private void Nw_Closed(object sender, EventArgs e) {
            //MpConsole.WriteLine($"fade out complete for: '{(sender as Control).DataContext}'");

            var w = sender as MpAvWindow;
            FinishClose(w);
        }


        #endregion

        #endregion

        #region Commands
        #endregion
    }
}
