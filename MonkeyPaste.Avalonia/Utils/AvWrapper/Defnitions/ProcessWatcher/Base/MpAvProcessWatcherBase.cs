using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonoMac.AppKit;
using System.Collections.Generic;
using System.IO;

namespace MonkeyPaste.Avalonia {
    
    public abstract class MpAvProcessWatcherBase : MpIProcessWatcher {
        #region Private Variables
        
        //protected Tuple<string, string, IntPtr>? _lastProcessTuple = default;
        private DispatcherTimer _timer;

        #endregion

        #region Properties

        public bool IsProcessTreePolled {
            get {
                if(OperatingSystem.IsWindows()) {
                    return true;
                }
                return false;
            }
        }
        public IntPtr ThisAppHandle {
            get {
                if(App.Desktop == null || App.Desktop.MainWindow == null) {
                    return IntPtr.Zero;
                }
                return App.Desktop.MainWindow.PlatformImpl.Handle.Handle;
            }
        }

        private bool _isThisAppActive = false;
        public bool IsThisAppActive { 
            get {
                if(IsProcessTreePolled) {
                    return _isThisAppActive;
                }
                var active_info = GetActiveProcessInfo();
                if(active_info == null) {
                    return false;
                }
                return active_info.Handle == ThisAppHandle;
            }
            set {
                if(!IsProcessTreePolled) {
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
                if(IsProcessTreePolled) {
                    return _lastProcessInfo;
                }

                var active_info = GetActiveProcessInfo();
                if(active_info == null) {
                    return _lastProcessInfo;
                }
                if(active_info.Handle == ThisAppHandle) {
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
                if(_fileSystemProcessInfo == null) {
                    _fileSystemProcessInfo = GetProcessPathProcessInfo(MpPlatformWrapper.Services.OsInfo.OsFileManagerPath);
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



        #endregion

        #region Public Methods

        public void StartWatcher() {
            if(IsProcessTreePolled) {
                CreateRunningProcessLookup();
                if (_timer == null) {
                    _timer = new DispatcherTimer(DispatcherPriority.Background) {
                        Interval = TimeSpan.FromMilliseconds(300)
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


        public virtual string GetProcessApplicationName(IntPtr hWnd) {
            string mwTitle = GetProcessTitle(hWnd);
            string appName = ParseTitleForApplicationName(mwTitle);

            if (string.IsNullOrWhiteSpace(appName)) {
                // NOTE trying to enforce app name to not be empty or end up
                // being file name when window title is normal pattern
                string processPath = GetProcessPath(hWnd);
                return Path.GetFileName(processPath);
            }
            return appName;
        }

        public virtual string GetProcessTitle(IntPtr handle) {
            return GetProcessApplicationName(handle);
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
            if (handle == null || handle == IntPtr.Zero) {
                return false;
            }
            return RunningProcessLookup.Any(x => x.Value.Contains(handle));
        }
        #endregion

        #region Protected Methods

        protected abstract MpPortableProcessInfo GetActiveProcessInfo();
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
            if(activeProcessInfo.Handle == ThisAppHandle) {
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
            if(kvp.Equals(default(KeyValuePair<string,ObservableCollection<IntPtr>>)) ||
                kvp.Value == null || kvp.Value.Count == 0) {
                return null;
            }
            return new MpPortableProcessInfo() {
                Handle = kvp.Value[0],
                ProcessPath = processPath,
                MainWindowTitle = GetProcessTitle(kvp.Value[0])
            };
        }


        #endregion
    }
}

