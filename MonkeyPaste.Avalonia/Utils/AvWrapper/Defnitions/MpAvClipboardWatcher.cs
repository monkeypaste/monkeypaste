using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardWatcher : MpIClipboardMonitor, MpIPlatformDataObjectRegistrar {
        #region Private Variables

        private bool _isInitialStart = false;
        private MpPortableDataObject _lastCbo;

        private object _lockObj = new object();

        private DispatcherTimer _timer;


        private List<string> _rejectedFormats = new List<string>() {
            "FileContents",
            "EnterpriseDataProtectionId"
        };

        #endregion

        #region MpIActionComponent Implementation

        public void RegisterActionComponent(MpIInvokableAction mvm) {
            OnClipboardChanged += mvm.OnActionInvoked;
            MpConsole.WriteLine($"ClipboardWatcher Registered {mvm.Label} trigger");
        }

        public void UnregisterActionComponent(MpIInvokableAction mvm) {
            OnClipboardChanged -= mvm.OnActionInvoked;
            MpConsole.WriteLine($"ClipboardWatcher {mvm.Label} Unregistered from ClipboardWatcher");
        }
        #endregion

        #region Properties

        //public bool IgnoreClipboardChanges { get; set; }
        #endregion

        #region Events

        public event EventHandler<MpPortableDataObject> OnClipboardChanged;

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public int RegisterFormat(string format) {
            //return (int)WinApi.RegisterClipboardFormatA(format);
            return MpRandom.Rand.Next();
        }

        public void StartMonitor() {
            if(_timer == null) {
                _timer = new DispatcherTimer(DispatcherPriority.Background) {
                    Interval = TimeSpan.FromMilliseconds(300)
                };
                _timer.Tick += _timer_Tick;
            }
            if(_timer.IsEnabled) {
                return;
            }

            _timer.Start();
            MpConsole.WriteLine("Clipboard watcher started");
        }

        public void StopMonitor() {
            if (_timer != null) {
                _timer.Stop();
                _lastCbo = null;
                MpConsole.WriteLine("Clipboard watcher stopped");
            }
        }

        #endregion

        #region Private Methods

        private void _timer_Tick(object sender, EventArgs e) {            
            CheckClipboard();
        }

        private void CheckClipboard() {            
            if(MpPlatformWrapper.Services.DataObjectHelperAsync.IsOleBusy) {
                // don't bother it
                return;
            }
            Dispatcher.UIThread.Post(async () => { await CheckClipboardHelper(); });
        }
        private async Task CheckClipboardHelper() {
            if (_lastCbo == null) {
                if(_isInitialStart) {
                    _isInitialStart = false;
                } else {
                    // this ensures start/stop ignores last change
                    _lastCbo = await ConvertManagedFormats();
                    return;
                }
            }

            var cbo = await ConvertManagedFormats();

            if (MpPortableDataObject.IsDataNotEqual(_lastCbo,cbo)) {
                MpConsole.WriteLine("Cb changed");
                 _lastCbo = cbo;
                var process_cbo = await MpPlatformWrapper.Services.DataObjectHelperAsync.GetPlatformClipboardDataObjectAsync(false) as MpPortableDataObject;
                OnClipboardChanged?.Invoke(typeof(MpAvClipboardWatcher).ToString(), process_cbo);
            }
        }

        private async Task<MpPortableDataObject> ConvertManagedFormats() {
            var result = await MpPlatformWrapper.Services.DataObjectHelperAsync.GetPlatformClipboardDataObjectAsync(true);
            return result as MpPortableDataObject;
        }

        #endregion
    }
}