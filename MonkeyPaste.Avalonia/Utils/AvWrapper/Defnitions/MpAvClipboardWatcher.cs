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
//using MonkeyPaste.Common.Wpf;
using WinApi = MonkeyPaste.Common.Avalonia.WinApi;
namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardWatcher : MpIClipboardMonitor, MpIPlatformDataObjectRegistrar {
        #region Private Variables

        private MpPortableDataObject _lastCbo;

        private object _lockObj = new object();

        private DispatcherTimer _timer;


        private List<string> _rejectedFormats = new List<string>() {
            "FileContents",
            "EnterpriseDataProtectionId"
        };

        #endregion

        #region Properties

        public bool IgnoreClipboardChanges { get; set; }
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
            if(IgnoreClipboardChanges) {
                _lastCbo = null;
            }
            
            Dispatcher.UIThread.Post(async () => { await CheckClipboardHelper(); });
        }
        private async Task CheckClipboardHelper() {
            if (_lastCbo == null) {
                //setting last here will ensure item on cb isn't added when starting
                _lastCbo = await ConvertManagedFormats();
                return;
            }

            var cbo = await ConvertManagedFormats();
            if (HasChanged(cbo)) {
                MpConsole.WriteLine("Cb changed");
                 _lastCbo = cbo;
                OnClipboardChanged?.Invoke(typeof(MpAvClipboardWatcher).ToString(), cbo);
            }
        }

        private async Task<MpPortableDataObject> ConvertManagedFormats() {
            MpPortableDataObject ndo = await MpAvClipboardHandlerCollectionViewModel.Instance.ReadClipboardOrDropObjectAsync();
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
                try {
                    if (!_lastCbo.DataFormatLookup.ContainsKey(nce.Key)) {
                        return true;
                    }
                    if (nce.Value is byte[] newBytes &&
                        _lastCbo.DataFormatLookup[nce.Key] is byte[] oldBytes) {
                        if (!newBytes.SequenceEqual(oldBytes)) {
                            return true;
                        }
                    } else if (nce.Value is IEnumerable<string> valStrs &&
                                _lastCbo.DataFormatLookup[nce.Key] is IEnumerable<string> lastStrs) { 
                        // must check actual string entries since the ref is always different 
                        if(valStrs.Count() != lastStrs.Count()) {
                            return true;
                        }
                        return valStrs.Any(x => !lastStrs.Contains(x));
                    } else {
                        if (!_lastCbo.DataFormatLookup[nce.Key].Equals(nce.Value)) {
                            return true;
                        }
                    }
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine("Error comparing clipbaord data. ", ex);
                }
                
                
            }
            return false;
        }


        #endregion
    }
}