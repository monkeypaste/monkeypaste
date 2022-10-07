using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonoMac.AppKit;

namespace MonkeyPaste.Avalonia {
    
    public abstract class MpAvProcessWatcherBase : MpIProcessWatcher {
        #region Private Variables
        
        protected Tuple<string, string, IntPtr>? _lastProcessTuple = default;
        private DispatcherTimer _timer;

        #endregion

        #region Properties

        private IntPtr _thisAppHandle;
        public virtual IntPtr ThisAppHandle {
            get => _thisAppHandle;
            set {
                if(_thisAppHandle != value) {
                    _thisAppHandle = value;
                    ThisAppProcessPath = GetProcessPath(_thisAppHandle);
                }
            }
        }
        public virtual string ThisAppProcessPath { get; set; }

        public IntPtr LastHandle => _lastProcessTuple == null ? IntPtr.Zero : _lastProcessTuple.Item3;

        public string LastProcessPath => _lastProcessTuple == null ? string.Empty : _lastProcessTuple.Item1;

        public string LastMainWindowTitle => _lastProcessTuple == null ? string.Empty : _lastProcessTuple.Item2;

        public ConcurrentDictionary<string, ObservableCollection<IntPtr>> RunningProcessLookup { get; protected set; } = new ConcurrentDictionary<string, ObservableCollection<IntPtr>>();

        #endregion

        #region Events


        public event EventHandler<MpProcessActivatedEventArgs> OnAppActivated;

        #endregion

        #region Constructors

        public MpAvProcessWatcherBase() {
            CreateRunningProcessLookup();
            if (_timer == null) {
                _timer = new DispatcherTimer() {
                    Interval = TimeSpan.FromMilliseconds(300)
                };
                _timer.Tick += ProcessWatcherTimer_tick;
            } else {
                _timer.Stop();
            }
            _timer.Start();
        }


        #endregion

        #region Public Methods

        public void StartWatcher() {
            _timer.Start();
        }
        public void StopWatcher() {
            _timer.Stop();
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
            foreach (var kvp in RunningProcessLookup) {
                if (kvp.Value.Contains(handle)) {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Protected Methods
        protected abstract Tuple<string, string, IntPtr> RefreshRunningProcessLookup();

        protected abstract void CreateRunningProcessLookup();

        protected virtual void ProcessWatcherTimer_tick(object sender, EventArgs e) {
            if (ThisAppHandle == IntPtr.Zero) {
                if (App.Desktop.MainWindow != null) {
                    ThisAppHandle = App.Desktop.MainWindow.PlatformImpl.Handle.Handle;
                }
            } else {
                if (ThisAppHandle != App.Desktop.MainWindow.PlatformImpl.Handle.Handle) {
                    // issue from loader/mainwindow swap?
                    //Debugger.Break();
                    MpConsole.WriteLine($"mw handle swapped from {ThisAppHandle} to {App.Desktop.MainWindow.PlatformImpl.Handle.Handle}");
                    ThisAppHandle = App.Desktop.MainWindow.PlatformImpl.Handle.Handle;
                }
            }
            if(OperatingSystem.IsLinux()) {
                // needs more filtering and is slow or certain process states aren't accounted for
                // so just ignoring
                return;
            }

            bool didActiveChange = false;
            var activeProcessTuple = RefreshRunningProcessLookup();

            if (activeProcessTuple == null ||
               activeProcessTuple.Item3 == ThisAppHandle) {
                return;
            }

            if (activeProcessTuple.Item1 != _lastProcessTuple?.Item1) {
                didActiveChange = true;
            }

            _lastProcessTuple = activeProcessTuple;
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

