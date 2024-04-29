using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipboardWatcher :
        MpIClipboardMonitor,
        MpIPlatformDataObjectRegistrar {
        #region Private Variables

        private MpPortableDataObject _lastCbo = null;
        //private DispatcherTimer _timer = null;
        private Timer _timer = null;

        private string[] _ignoredFormats = [
#if LINUX
            MpPortableDataFormats.MimeText
#endif
            ];
        #endregion

        #region Interfaces


        #region MpIPlatformDataObjectRegistrar Implementation
        

        #endregion
        
        #region MpIActionComponent Implementation
        void MpIActionComponent.RegisterActionComponent(MpIInvokableAction mvm) {
            if (OnClipboardChanged.HasInvoker(mvm)) {
                return;
            }
            OnClipboardChanged += mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnClipboardChanged)} Registered {mvm.Label}");
        }

        void MpIActionComponent.UnregisterActionComponent(MpIInvokableAction mvm) {
            if (!OnClipboardChanged.HasInvoker(mvm)) {
                return;
            }
            OnClipboardChanged -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"{nameof(OnClipboardChanged)} Unregistered {mvm.Label}");
        }

        #endregion

        #endregion

        #region Properties
        public bool IsMonitoring =>
            _timer != null && _timer.Enabled;

        public MpPortableDataObject LastClipboardDataObject =>
            _lastCbo;
        public bool IsStartupClipboard { get; private set; }
        #endregion

        #region Events

        public event EventHandler<MpPortableDataObject> OnClipboardChanged;

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public int RegisterFormat(string format) {
            return MpRandom.Rand.Next();
        }

        public void StartMonitor(bool ignoreCurrentState) {
#if MOBILE
            return;
#else
            if (_timer == null) {
                _timer = new Timer(TimeSpan.FromMilliseconds(300).TotalMilliseconds);
                _timer.Elapsed += _timer_Tick;
            }
            if (_timer.Enabled) {
                MpConsole.WriteLine("Ignoring clipboard monitor start, already started");
                return;
            }

            if (ignoreCurrentState) {
                Task.Run(async () => {
                    _lastCbo = await ConvertManagedFormats();
                    _timer.Start();
                    MpConsole.WriteLine("Clipboard watcher started. Current state ignored.");
                });
                return;
            }

            _timer.Start();
            MpConsole.WriteLine("Clipboard watcher started");
#endif
        }

        public void StopMonitor() {
            if (_timer == null) {
                return;
            }
            _timer.Stop();
            _lastCbo = null;
            MpConsole.WriteLine("Clipboard watcher stopped");
        }

        public void ForceChange(MpPortableDataObject mpdo) {
            OnClipboardChanged?.Invoke(typeof(MpAvClipboardWatcher).ToString(), mpdo);
        }
        #endregion

        #region Private Methods

        private void _timer_Tick(object sender, EventArgs e) {
            CheckClipboard();
        }

        private void CheckClipboard() {
            if (Mp.Services.DataObjectTools.IsOleBusy) {
                // don't bother it
                return;
            }
            Task.Run(CheckClipboardHelper);
        }
        private async Task CheckClipboardHelper() {
            var cbo = await ConvertManagedFormats();

            if (cbo.IsDataNotEqual(_lastCbo, fast_check: true, ignoredFormats: _ignoredFormats)) {
                IsStartupClipboard = _lastCbo == null;
                MpConsole.WriteLine($"Clipboard changed. Startup clipboard: {IsStartupClipboard}");
                _lastCbo = cbo;
                var process_cbo = await Mp.Services.DataObjectTools.ReadClipboardAsync(false) as MpPortableDataObject;
                OnClipboardChanged?.Invoke(typeof(MpAvClipboardWatcher).ToString(), process_cbo);
            }
        }

        private async Task<MpPortableDataObject> ConvertManagedFormats() {
            var result = await Mp.Services.DataObjectTools.ReadClipboardAsync(true);
            return result as MpPortableDataObject;
        }



        #endregion
    }
}