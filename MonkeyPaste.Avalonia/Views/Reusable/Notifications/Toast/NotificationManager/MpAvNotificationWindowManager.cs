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

        private MpAvNotificationPositioner _positioner;
        private MpAvTopmostSelector _topmostSelector;

        private ObservableCollection<Window> _windows = new ObservableCollection<Window>();

        //private Window _wvMessageWindow;

        #endregion

        #region Properties

        private static MpAvNotificationWindowManager _instance;
        public static MpAvNotificationWindowManager Instance => _instance ?? (_instance = new MpAvNotificationWindowManager());


        #endregion

        #region Events

        public event EventHandler<Window> OnNotificationWindowIsVisibleChanged;

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
                    case MpNotificationLayoutType.Append:
                        nw = MpAvAppendNotificationWindow.Instance;
                        break;
                    default:
                        nw = new MpAvMessageNotificationWindow();
                        nw.DataContext = nvmb;
                        break;
                }
                if(nw == null) {
                    // somethings wrong
                    Debugger.Break();
                }
                
                nw.Closed += Nw_Closed;
                nw.PointerReleased += Nw_PointerReleased;

                nw.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnNotificationWindowIsVisibleChangedHandler(nw));
                
                if (App.Desktop.MainWindow == null) {
                    // occurs on startup
                    App.Desktop.MainWindow = nw;
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
        private MpAvNotificationWindowManager() { }
        #endregion

        #region Public Methods
        public void Init() {
            _positioner = new MpAvNotificationPositioner();
            _topmostSelector = new MpAvTopmostSelector();
        }
        #endregion

        #region Private Methods

        private void BeginOpen(Window nw) {
            if (!_windows.Contains(nw)) {
                _windows.Add(nw);
            }

            if(nw == MpAvAppendNotificationWindow.Instance && 
                MpAvMainWindowViewModel.Instance.IsMainWindowOpen) {
                return;
            }
            MpAvMainWindowViewModel.Instance.IsAnyNotificationActivating = true;
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
        }

        private void FinishClose(Window w) {
            //var nvmb = w.DataContext as MpNotificationViewModelBase;
            if (w.DataContext is MpAvLoaderNotificationWindow) {
                // ignore so bootstrapper can swap main window
            } else if (w == MpAvAppendNotificationWindow.Instance) {
                w.Hide();
            } else {
                w.Close();
            }
            _windows.Remove(w);
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch(msg) {
                case MpMessageType.MainWindowOpening:
                    if(MpAvAppendNotificationWindow.Instance.IsVisible) {
                        HideNotification(MpAppendNotificationViewModel.Instance);
                    }
                    break;
            }
        }

        #region Window Events

        private void Nw_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            if(sender == MpAvAppendNotificationWindow.Instance ||
                sender is Window w && w.DataContext is MpNotificationViewModelBase nvmb &&
                nvmb.IsOverOptionsButton) {
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
