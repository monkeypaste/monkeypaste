using Avalonia.Controls;
using System.Collections.Generic;
using MonkeyPaste;
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
                if (MpAvMainWindow.Instance != null &&
                    MpAvMainWindow.Instance.Topmost) {
                    return MpAvMainWindow.Instance;
                }
                return _ntfWindows.FirstOrDefault(x => x.Topmost);
            }
        }

        #endregion

        #region Constructors
        public MpAvTopmostSelector() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            MpAvNotificationWindowManager.Instance.OnNotificationWindowIsVisibleChanged += Instance_OnNotificationIsVisibleChanged;

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
            if(!IsEnabled) {
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
                if(_ntfWindows.Count > 0) {
                    TrySetTopmost(_ntfWindows[_ntfWindows.Count - 1]);
                } else if (MpAvSettingsWindowViewModel.Instance.IsVisible) {
                    TrySetTopmost(MpAvSettingsWindow.Instance);
                } else if(MpAvMainWindowViewModel.Instance.IsMainWindowLocked) {
                    TrySetTopmost(MpAvMainWindow.Instance);
                }
            }

        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            if (!IsEnabled) {
                return;
            }
            switch (msg) {
                case MpMessageType.MainWindowLocked:
                    TrySetTopmost(MpAvMainWindow.Instance);
                    break;
                case MpMessageType.MainWindowUnlocked:
                    UnsetTopmost(MpAvMainWindow.Instance);
                    break;
                case MpMessageType.MainWindowOpened:
                    if (MpAvMainWindowViewModel.Instance.AnimateShowWindow) {
                        TrySetTopmost(MpAvMainWindow.Instance);
                    }

                    break;
                case MpMessageType.MainWindowHid:
                case MpMessageType.MainWindowClosing:
                    if (MpAvMainWindowViewModel.Instance.AnimateHideWindow) {
                        UnsetTopmost(MpAvMainWindow.Instance);
                    }
                    break;
            }
        }

        private int GetWindowTopmostPriority(Window w) {
            if(w == null) {
                return -1;
            }
            if(w == MpAvMainWindow.Instance) {
                return 1;
            }
            if(w == MpAvSettingsWindow.Instance) {
                return 2;
            }
            if(w.DataContext is MpNotificationViewModelBase) {
                if(w.DataContext is not MpAppendNotificationViewModel) {
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
