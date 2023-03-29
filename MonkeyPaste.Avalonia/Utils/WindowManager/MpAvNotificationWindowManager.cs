using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MonkeyPaste.Common;
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

        private MpAvNotificationPositioner _positioner;
        //private MpAvTopmostSelector _topmostSelector;

        private ObservableCollection<Window> _windows = new ObservableCollection<Window>();

        //private Window _wvMessageWindow;

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
                Debugger.Break();
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
            _positioner = new MpAvNotificationPositioner();
            //_topmostSelector = new MpAvTopmostSelector();
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }


        #endregion

        #region Private Methods
        private void ShowDesktopNotification(MpNotificationViewModelBase nvmb) {
            Dispatcher.UIThread.Post(() => {
                Window nw = null;
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
                    case MpNotificationLayoutType.Append:
                        nw = MpAvAppendNotificationWindow.Instance;
                        break;
                    default:
                        nw = new MpAvMessageNotificationWindow() {
                            DataContext = nvmb
                        };
                        break;
                }
                if (nw == null) {
                    // somethings wrong
                    Debugger.Break();
                }

                nw.Closed += Nw_Closed;
                nw.PointerReleased += Nw_PointerReleased;

                nw.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnNotificationWindowIsVisibleChangedHandler(nw));

                BeginOpen(nw);
            });
        }
        private void BeginOpen(Window nw) {
            var nvmb = nw.DataContext as MpNotificationViewModelBase;
            nvmb.IsClosing = false;

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                    desktop.MainWindow == null) {
                // occurs on startup
                desktop.MainWindow = nw;
                Mp.Services.ScreenInfoCollection = new MpAvDesktopScreenInfoCollection(nw);
            }

            if (!_windows.Contains(nw)) {
                _windows.Add(nw);
            }
            bool is_platform_loaded = Mp.Services != null &&
                     Mp.Services.StartupState != null &&
                     Mp.Services.StartupState.IsPlatformLoaded;
            if (is_platform_loaded) {
                if (nw == MpAvAppendNotificationWindow.Instance &&
                MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                    return;
                }
                MpAvMainWindowViewModel.Instance.IsAnyNotificationActivating = true;
            }

            if (nvmb.IsModal) {
                //bool wasLocked = MpAvMainWindowViewModel.Instance.IsMainWindowLocked;
                //if (!wasLocked) {
                //    MpAvMainWindowViewModel.Instance.ToggleMainWindowLockCommand.Execute(null);
                //}
                //if (MpAvWindowManager.ActiveWindow is Window w) {
                //    if (nw is MpAvWindow cw) {
                //        cw.ShowChildDialogAsync(w).FireAndForgetSafeAsync();
                //    } else {

                //        nw.Show(w);
                //    }
                //} else {
                //    nw.Show();
                //}
                nw.Show();
                //if (!wasLocked) {
                //    MpAvMainWindowViewModel.Instance.ToggleMainWindowLockCommand.Execute(null);
                //}
            } else {
                nw.Show();
            }


            if (!nw.IsVisible) {
                Debugger.Break();
            }
        }

        private void FinishClose(Window w) {
            if (w is MpAvLoaderNotificationWindow) {
                // ignore so bootstrapper can swap main window
            } else if (w == MpAvAppendNotificationWindow.Instance) {
                w.Hide();
            } else {
                w.Close();
            }
            _windows.Remove(w);
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowOpening:
                    if (MpAvAppendNotificationWindow.Instance != null &&
                        MpAvAppendNotificationWindow.Instance.IsVisible) {
                        HideNotification(MpAppendNotificationViewModel.Instance);
                    }
                    break;

            }
        }

        #region Window Events

        private void Nw_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            if (sender == MpAvAppendNotificationWindow.Instance ||
                sender is Window w && w.DataContext is MpNotificationViewModelBase nvmb &&
                nvmb.IsOverOptionsButton) {
                return;
            }

            if (MpAvMainView.Instance == null || !MpAvMainView.Instance.IsInitialized) {
                return;
            }
            if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }
            MpAvMainWindowViewModel.Instance.ShowMainWindowCommand.Execute(null);
        }

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
            if (!w.IsVisible && w.DataContext is MpNotificationViewModelBase nvmb && nvmb.IsClosing) {
                FinishClose(w);
            }
        }

        #endregion

        #endregion

        #region Commands
        #endregion
    }
}
