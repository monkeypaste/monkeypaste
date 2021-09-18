using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using MonkeyPaste;

namespace MonkeyPaste.UWP {
    public class MpClipboardListener {
        #region Singleton
        private static readonly Lazy<MpClipboardListener> _Lazy = new Lazy<MpClipboardListener>(() => new MpClipboardListener());
        public static MpClipboardListener Instance { get { return _Lazy.Value; } }
        #endregion

        #region Private Varibles
        //private System.Timers.Timer _timer;
        private bool _isStopped = false;
        private Dictionary<string,object> _lastCbo;
        //private ManualResetEvent _resetEvent = new ManualResetEvent(true);
        private Thread workThread;
        #endregion

        #region Properties
        public bool IgnoreNextClipboardChange = false;
        #endregion

        #region Events
        public event EventHandler<object> OnClipboardChanged;
        #endregion

        private MpClipboardListener() {
            workThread = new Thread(new ThreadStart(CheckClipboard));
            workThread.SetApartmentState(ApartmentState.STA);
            workThread.IsBackground = true;

            //_timer = new System.Timers.Timer();
            //_timer.Interval = 100;
            //_timer.AutoReset = false;
            //_timer.Elapsed += _timer_Elapsed;
        }

        public void Start() {
            if(workThread.IsAlive) {
                _isStopped = false;
            } else {
                workThread.Start();
            }
        }

        public void Stop() {
            _isStopped = false;
        }

        private void CheckClipboard() {
            while(true) {
                while(_isStopped) {
                    Thread.Sleep(100);
                } 
                var cbo = ConvertDpv(Clipboard.GetContent());
                if (HasChanged(cbo)) {
                    _lastCbo = cbo;
                    if (IgnoreNextClipboardChange) {
                        IgnoreNextClipboardChange = false;
                       // _timer.Start();
                        return;
                    }

                    OnClipboardChanged?.Invoke(this, _lastCbo);
                } 
                Thread.Sleep(1000);
            }    
        }

        private Dictionary<string,object> ConvertDpv(DataPackageView dpv) {
            var formats = new string[] { StandardDataFormats.Text, StandardDataFormats.Html, StandardDataFormats.Rtf, StandardDataFormats.Bitmap, StandardDataFormats.StorageItems };
            var cbDict = new Dictionary<string, object>();
            if(dpv == null) {
                return cbDict;
            }
            foreach (var af in formats) {
                if (dpv.Contains(af)) {

                    // TODO add checks for files and Images and convert: files to string seperated by NewLine, images to base 64
                    var data = MonkeyPaste.MpAsyncHelpers.RunSync<object>(() => dpv.GetDataAsync(af).AsTask());
                    cbDict.Add(af, data);
                }
                //var cbe = await cbo.GetDataAsync(af);
                //cbDict.Add(af, cbe);
            }
            return cbDict;
        }

        private bool HasChanged(Dictionary<string, object> nco) {
            if(_lastCbo == null && nco != null) {
                return true;
            }
            if(_lastCbo != null && nco == null) {
                return true;
            }
            if(_lastCbo.Count != nco.Count) {
                return true;
            }
            foreach(var nce in nco) {
                if(!_lastCbo.ContainsKey(nce.Key)) {
                    return true;
                }
                if(!_lastCbo[nce.Key].ToString().Equals(nce.Value)) {
                    return true;
                }
            }
            return false;
        }
    }
}
