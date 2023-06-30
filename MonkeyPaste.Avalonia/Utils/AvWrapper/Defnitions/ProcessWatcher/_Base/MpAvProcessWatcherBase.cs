using Avalonia.Threading;
using MonkeyPaste.Common;
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
        #endregion

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

        #region Properties
        public bool IsWatching {
            get {
                if (IsProcessTreePolled) {
                    if (_timer == null ||
                        !_timer.IsEnabled) {
                        return false;
                    }
                }
                return true;
            }
        }
        public int PollIntervalMs { get; set; } = 300;
        public bool IsProcessTreePolled {
            get {
                if (OperatingSystem.IsWindows()) {
                    return true;
                }
                return false;
            }
        }
        public IntPtr ThisAppHandle {
            get {
                if (App.MainView == null) {
                    return IntPtr.Zero;
                }
                return App.MainView.Handle;
            }
        }

        private bool _isThisAppActive = false;
        public bool IsThisAppActive {
            get {
                if (IsProcessTreePolled) {
                    return _isThisAppActive;
                }
                var active_info = GetActiveProcessInfo();
                if (active_info == null) {
                    return false;
                }
                return IsProcessPathEqual(ThisAppHandle, active_info.Handle);
            }
            set {
                if (!IsProcessTreePolled) {
                    throw new Exception("Cannot set when not polling");
                }
                _isThisAppActive = value;
            }
        }
        public virtual bool CanWatchProcesses() {
            // overridden on linux
            return true;
        }

        private MpPortableProcessInfo _lastProcessInfo;
        public MpPortableProcessInfo LastProcessInfo {
            get {
                if (IsProcessTreePolled) {
                    return _lastProcessInfo;
                }

                var active_info = GetActiveProcessInfo();
                if (active_info == null) {
                    return _lastProcessInfo;
                }
                if (IsProcessPathEqual(ThisAppHandle, active_info.Handle)) {
                    return _lastProcessInfo;
                }
                _lastProcessInfo = active_info;
                return _lastProcessInfo;
            }
            set {
                _lastProcessInfo = value;
            }
        }

        private MpPortableProcessInfo _fileSystemProcessInfo;
        public MpPortableProcessInfo FileSystemProcessInfo {
            get {
                if (_fileSystemProcessInfo == null) {
                    _fileSystemProcessInfo = GetProcessPathProcessInfo(Mp.Services.PlatformInfo.OsFileManagerPath);
                }
                return _fileSystemProcessInfo;
            }
            private set {
                _fileSystemProcessInfo = value;
            }
        }

        public ConcurrentDictionary<string, ObservableCollection<IntPtr>> RunningProcessLookup { get; protected set; } = new ConcurrentDictionary<string, ObservableCollection<IntPtr>>();

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
            if (IsProcessTreePolled) {
                CreateRunningProcessLookup();
                if (_timer == null) {
                    _timer = new DispatcherTimer(DispatcherPriority.Background) {
                        Interval = TimeSpan.FromMilliseconds(PollIntervalMs)
                    };
                    _timer.Tick += ProcessWatcherTimer_tick;
                } else {
                    _timer.Stop();
                }
                _timer.Start();
            }

        }
        public void StopWatcher() {
            _timer?.Stop();
        }

        public string ParseTitleForApplicationName(string windowTitle) {
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
        public virtual IntPtr GetLastActiveInstance(string path) {
            if (RunningProcessLookup.TryGetValue(path.ToLower(), out var handles) && handles.Count > 0) {
                return handles[0];
            }
            return IntPtr.Zero;
        }

        public abstract IntPtr GetParentHandleAtPoint(MpPoint poIntPtr);
        public abstract IntPtr SetActiveProcess(IntPtr handle);
        public abstract IntPtr SetActiveProcess(IntPtr handle, ProcessWindowStyle windowStyle);

        public abstract bool IsAdmin(object handleIdOrTitle);
        public abstract ProcessWindowStyle GetWindowStyle(object handleIdOrTitle);

        public virtual string GetProcessApplicationName(IntPtr hWnd) {
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
            return Path.GetFileName(processPath);
        }

        public virtual string GetProcessTitle(IntPtr handle) {
            return string.Empty;
        }

        public virtual string GetProcessPath(IntPtr handle) {
            foreach (var kvp in RunningProcessLookup) {
                if (kvp.Value.Contains(handle)) {
                    return kvp.Key;
                }
            }
            return string.Empty;
        }

        public virtual bool IsHandleRunningProcess(IntPtr handle) {
            if (handle == IntPtr.Zero) {
                return false;
            }
            return RunningProcessLookup.Any(x => x.Value.Contains(handle));
        }


        public virtual Process GetProcess(object handleIdOrTitle) {
            if (handleIdOrTitle is IntPtr handle) {
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

        #endregion

        #region Protected Methods

        public abstract MpPortableProcessInfo GetActiveProcessInfo();
        // protected abstract IEnumerable<MpPortableProcessInfo> GetRunningProcessInfos();

        protected abstract MpPortableProcessInfo RefreshRunningProcessLookup();

        protected abstract void CreateRunningProcessLookup();

        protected virtual void ProcessWatcherTimer_tick(object sender, EventArgs e) {
            bool didActiveChange = false;
            var activeProcessInfo = RefreshRunningProcessLookup();

            IsThisAppActive = false;

            if (activeProcessInfo == null) {
                return;
            }

            if (IsProcessPathEqual(ThisAppHandle, activeProcessInfo.Handle)) {
                // this app active, may have already been
                IsThisAppActive = true;
                return;
            }

            if (activeProcessInfo.ProcessPath != LastProcessInfo?.ProcessPath) {
                didActiveChange = true;
            }
            LastProcessInfo = activeProcessInfo;
            if (didActiveChange) {
                MpConsole.WriteLine(string.Format(@"Last Window: {0} '{1}' ({2})", LastProcessInfo.MainWindowTitle, LastProcessInfo.ProcessPath, LastProcessInfo.Handle));
                OnAppActivated?.Invoke(this, LastProcessInfo);
            }
        }

        #endregion

        #region Private Methods

        private MpPortableProcessInfo GetProcessPathProcessInfo(string processPath) {
            var kvp = RunningProcessLookup.FirstOrDefault(x => x.Key.ToLower() == processPath.ToLower());
            if (kvp.Equals(default(KeyValuePair<string, ObservableCollection<IntPtr>>)) ||
                kvp.Value == null || kvp.Value.Count == 0) {
                return null;
            }
            return new MpPortableProcessInfo() {
                Handle = kvp.Value[0],
                ProcessPath = processPath,
                MainWindowTitle = GetProcessTitle(kvp.Value[0])
            };
        }


        private Process FindProcess(IntPtr handle) => FindProcess(p => p.Handle == handle);
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

        private bool IsProcessPathEqual(IntPtr h1, IntPtr h2) {
            return GetProcessPath(h1) == GetProcessPath(h2);
        }


        #endregion
    }
}

