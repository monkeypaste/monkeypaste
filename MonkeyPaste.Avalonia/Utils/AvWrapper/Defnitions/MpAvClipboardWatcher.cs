using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public class MpAvClipboardWatcher :
        MpIClipboardMonitor,
        MpIPlatformDataObjectRegistrar {
        #region Private Variables

        //private bool _isInitialStart = false;
        private MpPortableDataObject _lastCbo;
        private DispatcherTimer _timer;


        private List<string> _rejectedFormats = new List<string>() {
            "FileContents",
            "EnterpriseDataProtectionId"
        };

        #endregion

        #region Interfaces


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
            _timer != null && _timer.IsEnabled;

        public MpPortableDataObject LastClipboardDataObject =>
            _lastCbo;
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
                _timer = new DispatcherTimer(DispatcherPriority.Background) {
                    Interval = TimeSpan.FromMilliseconds(300)
                };
                _timer.Tick += _timer_Tick;
            }
            if (_timer.IsEnabled) {
                MpConsole.WriteLine("Ignoring clipboard monitor start, already started");
                return;
            }

            if (ignoreCurrentState) {
                Dispatcher.UIThread.Post(async () => {
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
            //Task.Run(async () => {
            //var process_cbo = await Mp.Services.DataObjectTools.ReadDragDropDataObjectAsync(mpdo) as MpPortableDataObject;
            OnClipboardChanged?.Invoke(typeof(MpAvClipboardWatcher).ToString(), mpdo);
            //});
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
            Dispatcher.UIThread.Post(async () => { await CheckClipboardHelper(); });
        }
        private async Task CheckClipboardHelper() {
            var cbo = await ConvertManagedFormats();

            if (IsDataNotEqual(cbo, _lastCbo)) {
                MpConsole.WriteLine("Clipboard changed");
                _lastCbo = cbo;
                var process_cbo = await Mp.Services.DataObjectTools.ReadClipboardAsync(false) as MpPortableDataObject;
                OnClipboardChanged?.Invoke(typeof(MpAvClipboardWatcher).ToString(), process_cbo);
            }
        }


        private async Task<MpPortableDataObject> ConvertManagedFormats() {
            var result = await Mp.Services.DataObjectTools.ReadClipboardAsync(true);
            return result as MpPortableDataObject;
        }

        private bool IsDataNotEqual(MpPortableDataObject dbo1, MpPortableDataObject dbo2) {
            if (dbo1 == null && dbo2 != null) {
                return true;
            }
            if (dbo1 != null && dbo2 == null) {
                return true;
            }
            if (dbo1.DataFormatLookup.Count != dbo2.DataFormatLookup.Count) {
                return true;
            }
            foreach (var nce in dbo2.DataFormatLookup) {
                try {
                    if (!dbo1.DataFormatLookup.ContainsKey(nce.Key)) {
                        return true;
                    }
                    if (nce.Value is byte[] newBytes &&
                        dbo1.DataFormatLookup[nce.Key] is byte[] oldBytes) {
                        // compare byte arrays
                        if (!newBytes.SequenceEqual(oldBytes)) {
                            return true;
                        }
                    } else if (nce.Value is IEnumerable<object> ol &&
                                dbo1.DataFormatLookup[nce.Key] is IEnumerable<object> last_ol) {
                        // compare lists
                        if (ol.Count() != last_ol.Count()) {
                            return true;
                        }
                        if (ol is IEnumerable<string> strl &&
                            last_ol is IEnumerable<string> last_strl) {
                            // compare string lists
                            return strl.Any(x => !last_strl.Contains(x));
                        } else if (ol is IEnumerable<IStorageItem> stil &&
                                    stil.Where(x => x.Path != null).Select(x => x.Path) is IEnumerable<Uri> uril &&
                                    last_ol is IEnumerable<IStorageItem> last_stil &&
                                    last_stil.Where(x => x.Path != null).Select(x => x.Path) is IEnumerable<Uri> last_uril) {
                            // compare IStorageItem lists using non-null uri
                            if (uril.Count() != last_uril.Count()) {
                                return true;
                            }
                            return uril.Any(x => !last_uril.Contains(x));
                        } else {
                            MpDebug.Break($"No list comparision found for format '{nce.Key}'");
                        }
                    } else {
                        if (!dbo1.DataFormatLookup[nce.Key].Equals(nce.Value)) {
                            return true;
                        }
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error comparing clipbaord data. ", ex);
                }


            }
            return false;
        }

        #endregion
    }
}