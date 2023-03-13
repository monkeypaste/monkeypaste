using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvTopmostSelector {
        #region Private Variables

        private List<Window> _ntfWindows = new List<Window>();

        private List<Window> _forcedUnset = new List<Window>();
        #endregion

        #region Properties

        public bool IsEnabled { get; set; } = true;

        public Window TopmostWindow {
            get {
                if (MpAvSettingsWindow.Instance != null &&
                    MpAvSettingsWindow.Instance.Topmost) {
                    return MpAvSettingsWindow.Instance;
                }
                if (App.MainWindow != null &&
                    App.MainWindow.GetVisualAncestor<Window>() is Window mw) {
                    return mw;
                }
                return _ntfWindows.FirstOrDefault(x => x.Topmost);
            }
        }

        #endregion

        #region Constructors
        public MpAvTopmostSelector() {
            //MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            //MpAvNotificationWindowManager.Instance.OnNotificationWindowIsVisibleChanged += Instance_OnNotificationIsVisibleChanged;

        }
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private bool TrySetTopmost(Window w) {
            if (w == null) {
                return false;
            }
            if (w.Topmost) {
                return true;
            }
            if (TopmostWindow == null) {
                w.Topmost = true;
                return true;
            }
            if (GetWindowTopmostPriority(w) >= GetWindowTopmostPriority(TopmostWindow)) {
                bool needsActivate = UnsetTopmost(TopmostWindow);

                //if (!_forcedUnset.Contains(TopmostWindow)) {
                //    _forcedUnset.Add(TopmostWindow);
                //}
                //UnsetTopmost(TopmostWindow);
                w.Topmost = true;
                if (needsActivate) {
                    w.Activate();
                }
                //UpdateForcedUnsets();
                //if(_forcedUnset.Count > 0) {
                //    w.Activate();
                //}
                return true;
            }
            return false;
        }

        private bool UnsetTopmost(Window w) {
            bool wasActivated = false;

            if (w != null && w.Topmost) {
                w.Topmost = false;
                if (w.IsVisible) {
                    // i think this works around avalonia SetTopmost SWP_NOACTIVATE flag

                    w.Activate();
                    wasActivated = true;
                }
            }

            return wasActivated;
        }
        private void Instance_OnNotificationIsVisibleChanged(object sender, Window w) {
            if (!IsEnabled) {
                return;
            }
            if (w.IsVisible) {
                if (!_ntfWindows.Contains(w)) {
                    _ntfWindows.Add(w);
                }
                TrySetTopmost(w);
            } else {
                _ntfWindows.Remove(w);
                UnsetTopmost(w);
                if (_ntfWindows.Count > 0) {
                    TrySetTopmost(_ntfWindows[_ntfWindows.Count - 1]);
                } else if (MpAvSettingsViewModel.Instance.IsVisible) {
                    TrySetTopmost(MpAvSettingsWindow.Instance);
                } else if (MpAvMainWindowViewModel.Instance.IsMainWindowLocked) {
                    TrySetTopmost(App.MainWindow);
                }
            }

        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            if (!IsEnabled) {
                return;
            }
            switch (msg) {
                case MpMessageType.ChildWindowClosed:
                case MpMessageType.MainWindowLocked:
                    TrySetTopmost(App.MainWindow);
                    break;
                case MpMessageType.MainWindowUnlocked:
                    UnsetTopmost(App.MainWindow);
                    break;
                case MpMessageType.MainWindowOpening:
                case MpMessageType.MainWindowOpened:
                    //case MpMessageType.MainWindowClosing:
                    if (MpAvMainWindowViewModel.Instance.AnimateShowWindow) {
                        TrySetTopmost(App.MainWindow);
                    }
                    break;
                case MpMessageType.MainWindowClosed:
                    if (MpAvMainWindowViewModel.Instance.AnimateHideWindow) {
                        UnsetTopmost(App.MainWindow);
                    }
                    break;
            }
        }

        private int GetWindowTopmostPriority(Window w) {
            if (w == null) {
                return -1;
            }
            if (w == App.MainWindow) {
                return 1;
            }
            if (w == MpAvSettingsWindow.Instance) {
                return 2;
            }
            if (w.DataContext is MpNotificationViewModelBase) {
                if (w.DataContext is not MpAppendNotificationViewModel) {
                    return 3;
                }
                return 4;
            }
            throw new System.Exception("Unknown window: " + w.Title);
        }

        private void UpdateForcedUnsets() {
            var to_remove = _forcedUnset.Where(x => !x.IsVisible).ToList();
            for (int i = 0; i < to_remove.Count; i++) {
                _forcedUnset.Remove(to_remove[i]);
            }
            _forcedUnset.ForEach(x => x.Activate());
        }
        #endregion

        #region Commands
        #endregion
    }
}
