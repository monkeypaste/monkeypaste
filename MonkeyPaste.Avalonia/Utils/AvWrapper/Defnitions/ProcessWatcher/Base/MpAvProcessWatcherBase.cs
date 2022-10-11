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

namespace MonkeyPaste.Avalonia {
    
    public abstract class MpAvProcessWatcherBase : MpIProcessWatcher {
        #region Private Variables
        
        //protected Tuple<string, string, IntPtr>? _lastProcessTuple = default;
        private DispatcherTimer _timer;

        #endregion

        #region Properties

        //private IntPtr _thisAppHandle;
        //public virtual IntPtr ThisAppHandle {
        //    get => _thisAppHandle;
        //    set {
        //        if(_thisAppHandle != value) {
        //            _thisAppHandle = value;
        //            ThisAppProcessPath = GetProcessPath(_thisAppHandle);
        //        }
        //    }
        //}

        public IntPtr ThisAppHandle {
            get {
                if(App.Desktop == null || App.Desktop.MainWindow == null) {
                    return IntPtr.Zero;
                }
                return App.Desktop.MainWindow.PlatformImpl.Handle.Handle;
            }
        }

        
        public virtual bool CanWatchProcesses() {
            // overridden on linux
            return true;
        }

        public IntPtr LastHandle => LastProcessInfo == null ? IntPtr.Zero : LastProcessInfo.Handle;

        public string LastProcessPath => LastProcessInfo == null ? string.Empty : LastProcessInfo.ProcessPath;

        public string LastMainWindowTitle => LastProcessInfo == null ? string.Empty : LastProcessInfo.MainWindowTitle;

        private MpPortableProcessInfo _lastProcessInfo;
        public MpPortableProcessInfo LastProcessInfo {
            get {
                var active_info = GetActiveProcessInfo();
                if(active_info == null) {
                    return null;
                }
                if(active_info.Handle == ThisAppHandle) {
                    return _lastProcessInfo;
                }
                _lastProcessInfo = active_info;
                return _lastProcessInfo;
            }
        }

        public ConcurrentDictionary<string, ObservableCollection<IntPtr>> RunningProcessLookup { get; protected set; } = new ConcurrentDictionary<string, ObservableCollection<IntPtr>>();

        #endregion

        #region Events


        public event EventHandler<MpProcessActivatedEventArgs> OnAppActivated;

        #endregion

        #region Constructors



        #endregion

        #region Public Methods

        public void StartWatcher() {
            //CreateRunningProcessLookup();
            //if (_timer == null) {
            //    _timer = new DispatcherTimer(DispatcherPriority.Background) {
            //        Interval = TimeSpan.FromMilliseconds(300)
            //    };
            //    _timer.Tick += ProcessWatcherTimer_tick;
            //} else {
            //    _timer.Stop();
            //}
            //_timer.Start();
        }
        public void StopWatcher() {
           // _timer.Stop();
        }

        public virtual IntPtr GetLastActiveInstance(string path) {
            if (RunningProcessLookup.TryGetValue(path.ToLower(), out var handles) && handles.Count > 0) {
                return handles[0];
            }
            return IntPtr.Zero;
        }

        public abstract IntPtr GetParentHandleAtPoint(MpPoint poIntPtr);

        public abstract void SetActiveProcess(IntPtr handle);

        public void SetActiveProcess(MpPortableProcessInfo pi, int waitForInputIdleTimeout = 30000) {
            // is this necessary? 
        }

        public virtual string GetProcessApplicationName(IntPtr handle) {
            foreach(var kvp in RunningProcessLookup) {
                if(kvp.Value.Contains(handle)) {
                    return kvp.Key;
                }
            }
            return String.Empty;
        }

        public virtual string GetProcessMainWindowTitle(IntPtr handle) {
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
            //if (ThisAppHandle == IntPtr.Zero) {
            //    if (App.Desktop.MainWindow != null) {
            //        ThisAppHandle = App.Desktop.MainWindow.PlatformImpl.Handle.Handle;
            //    }
            //} else {
            //    if (ThisAppHandle != App.Desktop.MainWindow.PlatformImpl.Handle.Handle) {
            //        // issue from loader/mainwindow swap?
            //        //Debugger.Break();
            //        MpConsole.WriteLine($"mw handle swapped from {ThisAppHandle} to {App.Desktop.MainWindow.PlatformImpl.Handle.Handle}");
            //        ThisAppHandle = App.Desktop.MainWindow.PlatformImpl.Handle.Handle;
            //    }
            //}
            if(OperatingSystem.IsLinux()) {
                // needs more filtering and is slow or certain process states aren't accounted for
                // so just ignoring
                //return;
            }

            bool didActiveChange = false;
            var activeProcessInfo = RefreshRunningProcessLookup();

            if (activeProcessInfo == null ||
               activeProcessInfo.Handle == ThisAppHandle) {
                return;
            }

            if (activeProcessInfo.ProcessPath != LastProcessInfo?.ProcessPath) {
                didActiveChange = true;
            }

            //LastProcessInfo = activeProcessInfo;
            if (didActiveChange) {
                MpConsole.WriteLine(string.Format(@"Last Window: {0} '{1}' ({2})", LastMainWindowTitle, LastProcessPath, LastHandle));
                OnAppActivated?.Invoke(
                    this,
                    new MpProcessActivatedEventArgs() {
                        ApplicationName = LastMainWindowTitle,
                        ProcessPath = LastProcessPath,
                        Handle = LastHandle
                    });
            }
        }

        #endregion

        #region Private Methods




        #endregion
    }
}

