using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MonkeyPaste.Avalonia {

    public abstract class MpAvProcessWatcherBase : MpIProcessWatcher {
        #region Private Variables
        private DispatcherTimer _timer;
        private nint _lastActiveHandle;
        #endregion

        #region Constants

        protected const int POLL_INTERVAL_MS = 300;
        #endregion

        #region Interfaces

        #region MpIActionComponent Implementation

        public void RegisterActionComponent(MpIInvokableAction mvm) {
            OnAppActivated += mvm.OnActionInvoked;
            MpConsole.WriteLine($"ProcessWatcher Registered {mvm.Label} trigger");
        }

        public void UnregisterActionComponent(MpIInvokableAction mvm) {
            OnAppActivated -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"Trigger {mvm.Label} Unregistered from ProcessWatcher");
        }
        #endregion

        #endregion

        #region Properties
        public bool IsWatching =>
            _timer != null && _timer.IsEnabled;

        private nint _thisAppHandle = nint.Zero;
        protected nint ThisAppHandle {
            get {
                if (_thisAppHandle == nint.Zero) {
                    //if (App.MainView == null) {
                    //    return nint.Zero;
                    //}
                    //return App.MainView.Handle;
                    if (MpAvWindowManager.MainWindow != null) {
                        _thisAppHandle = MpAvWindowManager.MainWindow.TryGetPlatformHandle().Handle;
                    }
                }
                return _thisAppHandle;
            }
        }

        //private bool _isThisAppActive = false;
        //public bool IsThisAppActive {
        //    get {
        //        if (IsProcessTreePolled) {
        //            return _isThisAppActive;
        //        }
        //        var active_info = GetActiveProcessInfo();
        //        if (active_info == null) {
        //            return false;
        //        }
        //        return IsProcessPathEqual(ThisAppHandle, active_info.Handle);
        //    }
        //    set {
        //        if (!IsProcessTreePolled) {
        //            throw new Exception("Cannot set when not polling");
        //        }
        //        _isThisAppActive = value;
        //    }
        //}


        private MpPortableProcessInfo _thisAppProcessInfo;
        public MpPortableProcessInfo ThisAppProcessInfo =>
            _thisAppProcessInfo ?? (_thisAppProcessInfo = GetProcessInfoByHandle(ThisAppHandle));

        public MpPortableProcessInfo LastProcessInfo =>
            GetProcessInfoByHandle(_lastActiveHandle);

        #endregion

        #region Events


        public event EventHandler<MpPortableProcessInfo> OnAppActivated;

        #endregion

        #region Constructors

        public MpAvProcessWatcherBase() {
            StartWatcher();
        }

        #endregion

        #region Public Methods

        public void StartWatcher() {

            if (_timer == null) {
                // initial start

                var active_pi = GetActiveProcessInfo();
                var this_pi = ThisAppProcessInfo;
                _timer = new DispatcherTimer(DispatcherPriority.Background) {
                    Interval = TimeSpan.FromMilliseconds(POLL_INTERVAL_MS)
                };
                _timer.Tick += ProcessWatcherTimer_tick;
            } else {
                _timer.Stop();
            }
            _timer.Start();
        }
        public void StopWatcher() {
            _timer?.Stop();
        }

        public MpPortableProcessInfo GetProcessInfoFromScreenPoint(MpPoint screenPoint) {
            var handle = GetParentHandleAtPoint(screenPoint);
            return GetProcessInfoByHandle(handle);
        }
        public abstract nint SetActiveProcess(nint handle);

        #endregion

        #region Protected Methods
        protected abstract nint GetParentHandleAtPoint(MpPoint ponint);
        protected abstract nint SetActiveProcess(nint handle, ProcessWindowStyle windowStyle);

        protected abstract bool IsAdmin(object handleIdOrTitle);
        protected abstract ProcessWindowStyle GetWindowStyle(object handleIdOrTitle);

        protected virtual string GetProcessApplicationName(nint hWnd) {
            string process_path = GetProcessPath(hWnd);
            if (process_path.IsFile() &&
                FileVersionInfo.GetVersionInfo(process_path) is FileVersionInfo fvi) {

                if (!string.IsNullOrWhiteSpace(fvi.FileDescription)) {
                    return fvi.FileDescription;
                }
                if (!string.IsNullOrWhiteSpace(fvi.ProductName)) {
                    return fvi.ProductName;
                }
            }
            string processPath = GetProcessPath(hWnd);
            return Path.GetFileNameWithoutExtension(processPath);
        }

        protected abstract string GetProcessTitle(nint handle);

        protected abstract string GetProcessPath(nint handle);
        protected virtual bool CanWatchProcesses() {
            // overridden on linux to verify xdotool exists
            return true;
        }

        protected virtual Process GetProcess(object handleIdOrTitle) {
            if (handleIdOrTitle is nint handle) {
                return FindProcess(handle);
            }
            if (handleIdOrTitle is int id) {
                return FindProcess(id);
            }
            if (handleIdOrTitle is string title) {
                return FindProcess(title);
            }
            return null;
        }
        protected abstract MpPortableProcessInfo GetActiveProcessInfo();
        protected abstract MpPortableProcessInfo GetProcessInfoByHandle(nint handle);

        protected virtual void ProcessWatcherTimer_tick(object sender, EventArgs e) {
            var activeProcessInfo = GetActiveProcessInfo();
            if (activeProcessInfo == null) {
                return;
            }
            if (activeProcessInfo.Handle == _lastActiveHandle) {
                return;
            }
            if (IsProcessPathEqual(ThisAppHandle, activeProcessInfo.Handle)) {
                return;
            }

            bool is_new_active_process = !IsProcessPathEqual(activeProcessInfo.Handle, _lastActiveHandle);
            _lastActiveHandle = activeProcessInfo.Handle;

            if (!is_new_active_process) {
                // ignore inner-process window changes not relevant for this event
                return;
            }
            MpConsole.WriteLine(string.Format(@"Last Window: {0} '{1}' ({2})", LastProcessInfo.MainWindowTitle, LastProcessInfo.ProcessPath, LastProcessInfo.Handle));

            OnAppActivated?.Invoke(this, LastProcessInfo);
        }

        #endregion

        #region Private Methods

        private string ParseTitleForApplicationName(string windowTitle) {
            string mwt = windowTitle;
            if (string.IsNullOrEmpty(mwt)) {
                return mwt;
            }
            var mwta = mwt.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (mwta.Length == 1) {
                if (string.IsNullOrEmpty(mwta[0])) {
                    return "Explorer";
                }
                return mwta[0];
            }
            return mwta[mwta.Length - 1].Trim();
        }


        private Process FindProcess(nint handle) => FindProcess(p => p.Handle == handle);
        private Process FindProcess(int id) => FindProcess(p => p.Id == id);
        private Process FindProcess(string title) => FindProcess(p => p.MainWindowTitle == title);
        private Process FindProcess(Func<Process, bool> comparer) {
            foreach (Process p in Process.GetProcesses()) {
                if (comparer(p)) {
                    return p;
                }
            }
            return null;
        }

        private bool IsProcessPathEqual(nint h1, nint h2) {
            return GetProcessPath(h1) == GetProcessPath(h2);
        }


        #endregion
    }
}

