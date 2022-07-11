using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonoMac.AppKit;

namespace MonkeyPaste.Avalonia {
    
    public abstract class MpAvProcessWatcherBase : MpIProcessWatcher {
        #region Private Variables
        
        private Tuple<string, string, IntPtr>? _lastProcessTuple = default;
        private DispatcherTimer _timer;

        #endregion

        #region Properties

        public IntPtr ThisAppHandle => MpAvMainWindow.Instance == null ? IntPtr.Zero : MpAvMainWindow.Instance.PlatformImpl.Handle.Handle;

        public IntPtr LastHandle => _lastProcessTuple == null ? IntPtr.Zero : _lastProcessTuple.Item3;

        public string LastProcessPath => _lastProcessTuple == null ? string.Empty : _lastProcessTuple.Item1;

        public string LastMainWindowTitle => _lastProcessTuple == null ? string.Empty : _lastProcessTuple.Item2;

        public ConcurrentDictionary<string, ObservableCollection<IntPtr>> RunningProcessLookup { get; protected set; }

        #endregion

        #region Constructors

        public MpAvProcessWatcherBase() {
            InitPlatform();

            CreateRunningProcessLookup();

            if (_timer == null) {
                _timer = new DispatcherTimer() {
                    Interval = TimeSpan.FromMilliseconds(300)
                };
                _timer.Tick += _timer_Tick;
            } else {
                _timer.Stop();
            }
            _timer.Start();
        }


        #endregion

        #region Public Methods
        public virtual void InitPlatform() { }

        public event EventHandler<MpProcessActivatedEventArgs> OnAppActivated;

        public abstract IntPtr GetLastActiveInstance(string path);

        public abstract IntPtr GetParentHandleAtPoint(MpPoint poIntPtr);

        public abstract string GetProcessApplicationName(IntPtr handle);

        public abstract string GetProcessMainWindowTitle(IntPtr handle);

        public abstract string GetProcessPath(IntPtr handle);

        public abstract bool IsHandleRunningProcess(IntPtr handle);

        #endregion

        #region Protected Methods
        protected abstract Tuple<string, string, IntPtr> RefreshRunningProcessLookup();

        protected abstract void CreateRunningProcessLookup();

        #endregion

        #region Private Methods

        private void _timer_Tick(object sender, EventArgs e) {
            bool didActiveChange = false;
            var activeProcessTuple = RefreshRunningProcessLookup();
            if(activeProcessTuple.Item3 == ThisAppHandle) {
                return;
            }

            if (activeProcessTuple.Item1 != _lastProcessTuple?.Item1) {
                didActiveChange = true;
            }

            _lastProcessTuple = activeProcessTuple;
            if(didActiveChange) {
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
    }
}

