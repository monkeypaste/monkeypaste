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

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardWatcher : MpIClipboardMonitor, MpIPlatformDataObjectRegistrar {
        #region Private Variables

        private MpPortableDataObject _lastCbo;

        private object _lockObj = new object();

        private DispatcherTimer _timer;

        private bool _isCheckingClipboard = false;
        

        #endregion

        #region Properties
        public bool IgnoreNextClipboardChangeEvent { get; set; } = false;

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
        }

        public void StopMonitor() {
            if (_timer != null) {
                _timer.Stop();
            }
        }

        #endregion

        #region Private Methods

        private void _timer_Tick(object sender, EventArgs e) {
            CheckClipboard();
        }

        private void CheckClipboard() {
            if(_isCheckingClipboard) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                //while (MpClipboardManager.ThisAppHandle == null || MpClipboardManager.ThisAppHandle == IntPtr.Zero) {
                //    Thread.Sleep(100);
                //}
                //setting last here will ensure item on cb isn't added when starting
                if (_lastCbo == null) {
                    _lastCbo = await ConvertManagedFormats();
                    return;
                }

                var cbo = await ConvertManagedFormats();
                if (HasChanged(cbo)) {
                    _lastCbo = cbo;
                    // TODO Add plugin handling here

                    OnClipboardChanged?.Invoke(typeof(MpAvClipboardWatcher).ToString(), cbo);
                }
            });
        }

        private async Task<MpPortableDataObject> ConvertManagedFormats() {
            _isCheckingClipboard = true;

            var ndo = new MpPortableDataObject();
            string[] formats = await Application.Current.Clipboard.GetFormatsAsync();

            foreach (var format in formats.Where(x=>!x.StartsWith("Unknown_Format"))) {
                object formatData = await Application.Current.Clipboard.GetDataAsync(format);
                try {
                    ndo.SetData(format, formatData);
                }catch(MpUnregisteredDataFormatException){                    
                    MpPortableDataFormats.RegisterDataFormat(format);
                    ndo.SetData(format, formatData);
                }
            }

            _isCheckingClipboard = false;
            return ndo;
        }

        private bool HasChanged(MpPortableDataObject nco) {
            if (_lastCbo == null && nco != null) {
                return true;
            }
            if (_lastCbo != null && nco == null) {
                return true;
            }
            if (_lastCbo.DataFormatLookup.Count != nco.DataFormatLookup.Count) {
                return true;
            }
            foreach (var nce in nco.DataFormatLookup) {
                if (!_lastCbo.DataFormatLookup.ContainsKey(nce.Key)) {
                    return true;
                }
                if(nce.Value is byte[] newBytes && 
                    _lastCbo.DataFormatLookup[nce.Key] is byte[] oldBytes) {
                    if(!newBytes.SequenceEqual(oldBytes)) {
                        return true;
                    }
                } else {
                    if (!_lastCbo.DataFormatLookup[nce.Key].Equals(nce.Value)) {
                        return true;
                    }
                }
                
            }
            return false;
        }

        #endregion
    }
}