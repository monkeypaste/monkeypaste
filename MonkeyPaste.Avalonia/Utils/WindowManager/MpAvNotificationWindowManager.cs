using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using Microsoft.Maui.Graphics;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;
using System.Threading.Tasks;
using static Avalonia.Animation.PageSlide;

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

            if (MpAvThemeViewModel.Instance.IsDesktop) {
                ShowDesktopNotification(nvmb);
            } else {
                ShowMobileNotification(nvmb);
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
        private void ShowMobileNotification(MpAvNotificationViewModelBase nvmb) {
            Control nw = null;
            var layoutType = MpAvNotificationViewModelBase.GetLayoutTypeFromNotificationType(nvmb.NotificationType);
            switch (layoutType) {
                case MpNotificationLayoutType.Welcome:
                    // ignored
                    break;
                case MpNotificationLayoutType.Loader:
                    var mlv = new MpAvMobileLoaderView() {
                        DataContext = nvmb
                    };
                    App.SetPrimaryView(mlv);
                    break;
                default:
                    MpDebug.Assert(nvmb.Body is string, $"Unhandled mobile ntf '{nvmb.NotificationType}'");

                    if(MpAvDeviceWrapper.Instance == null ||
                        nvmb.Body is not string text) {
                        break;
                    }
                    MpAvDeviceWrapper.Instance.PlatformToastNotification.ShowToast(
                        title: nvmb.Title,
                        text: text,
                        icon: nvmb.IconSourceObj,
                        accentHexColor: nvmb.BorderHexColor);
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
            MpAvWindow nw = null;
            switch (nvmb) {
                case MpAvWelcomeNotificationViewModel:
                    if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                        return;
                    }
                    nw = new MpAvWelcomeWindow() {
                        DataContext = nvmb
                    };
                    break;
                case MpAvLoaderNotificationViewModel:
#if WINDOWED
                    var mwo = MpAvPrefViewModel.Instance.MainWindowOrientationStr.ToEnum<MpMainWindowOrientationType>();
                    bool is_vert = mwo == MpMainWindowOrientationType.Left || mwo == MpMainWindowOrientationType.Right;
                    var w = new Window() {
                        Width = is_vert ? 360:740,
                        Height = is_vert ? 740:360,
                        DataContext = nvmb,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    w.Classes.Add("windowed-mode");
                    if (Mp.Services != null && Mp.Services.ScreenInfoCollection == null) {
                        Mp.Services.ScreenInfoCollection = new MpAvDesktopScreenInfoCollection(w);
                    }

                    nw = new MpAvWindow() {
                        DataContext = nvmb,
                        Content = new MpAvMobileLoaderView() {
                            DataContext = nvmb
                        }
                    };
                    w.Content = nw;
                    App.Current.SetMainWindow(w);
#else
                    nw = new MpAvLoaderNotificationWindow() {
                        DataContext = nvmb,
                        Topmost = true,
                        ShowActivated = true
                    };
                    if (Mp.Services != null && Mp.Services.ScreenInfoCollection == null) {
                        Mp.Services.ScreenInfoCollection = new MpAvDesktopScreenInfoCollection(nw);
                    }
                    App.Current.SetMainWindow(nw); 
#endif
                    break;
                default:
                    nw = new MpAvUserActionNotificationWindow() {
                        DataContext = nvmb,
                    };
                    break;
            }
            if (MpAvThemeViewModel.Instance.IsMultiWindow &&
                nvmb is not MpAvWelcomeNotificationViewModel &&
                nw != null &&
                nw.TryGetPlatformHandle() is { } ph) {
#if WINDOWS
                MpAvToolWindow_Win32.SetAsToolWindow(ph.Handle);
#endif
                // BUG ntf styles don't seem to be registering, manually setting system decoration
                nw.SystemDecorations = SystemDecorations.None;

            }



            BeginOpen(nw);
        }
        private void BeginOpen(MpAvWindow nw) {
            if(nw == null) {
                return;
            }
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
                if(MpAvThemeViewModel.Instance.IsMobileOrWindowed) {
                    nw.WindowStartupLocation = WindowStartupLocation.Manual;
                    nvmb.AnchorTarget = MpAvMainView.Instance;
                    nw.Position = MpAvWindowPositioner.GetWindowPositionByAnchorVisual(nw, nvmb.AnchorTarget);
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

            void Nw_EffectiveViewportChanged(object sender, EventArgs e) {
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
            nw.Opened += Nw_EffectiveViewportChanged;

#if MOBILE_OR_WINDOWED
            if (nvmb is MpAvUserActionNotificationViewModel uavm) {
                    if (nvmb.IsModal) {
                        nw.OpenTransition = MpChildWindowTransition.FadeIn;
                        nw.CloseTransition = MpChildWindowTransition.FadeOut;
                    } else {
                        nw.OpenTransition = MpChildWindowTransition.SlideInFromTop;
                        nw.CloseTransition = MpChildWindowTransition.SlideOutToTop;
                    }
                    nw.HeightRatio = -1; // use default height
                } else if (nvmb is MpAvLoaderNotificationViewModel) {
                    nw.OpenTransition = MpChildWindowTransition.FadeIn;
                    nw.CloseTransition = MpChildWindowTransition.FadeOut | MpChildWindowTransition.SlideOutToTop;
                }
#endif
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
