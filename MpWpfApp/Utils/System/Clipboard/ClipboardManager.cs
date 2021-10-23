using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonkeyPaste;

namespace MpWpfApp {
    public class ClipboardManager : IDisposable {
        public EventHandler<IDataObject> ClipboardChanged;
        private ClipboardListener _Listener;

        public MpLastWindowWatcher LastWindowWatcher { get; set; }
        public bool IgnoreClipboardChangeEvent { get; set; }

        private class ClipboardListener : NativeWindow {
            private readonly Action _Action;
            private const int HWND_MESSAGE = -3;
            private const int WM_CLIPBOARDUPDATE = 0x031D;


            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool AddClipboardFormatListener(IntPtr hWnd);


            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool RemoveClipboardFormatListener(IntPtr hWnd);


            protected override void WndProc(ref Message m) {
                base.WndProc(ref m);
                if (m.Msg == WM_CLIPBOARDUPDATE)
                    _Action.Invoke();
            }


            protected override void OnHandleChange() {
                base.OnHandleChange();
                AddClipboardFormatListener(this.Handle);
            }


            public override void DestroyHandle() {
                RemoveClipboardFormatListener(this.Handle);
                base.DestroyHandle();
            }


            public ClipboardListener(Action action) {
                _Action = action;
                _Action.Invoke();
                this.CreateHandle(new CreateParams { Parent = new IntPtr(HWND_MESSAGE) });
            }
        }


        private void OnUpdate() {
            MpHelpers.Instance.RunOnMainThread(async () => {
                if (IgnoreClipboardChangeEvent) {
                    //do nothing
                } else {
                    if (MpAppModeViewModel.Instance.IsAppPaused) {
                        MonkeyPaste.MpConsole.WriteLine("App Paused, ignoring copy");
                    }
                    // TODO uncomment below, its commented out trying to debug ucrt issue
                    else if (MpApp.IsAppRejectedByPath(MpHelpers.Instance.GetProcessPath(LastWindowWatcher.LastHandle))) {
                        var hwnd = LastWindowWatcher.LastHandle;
                        MonkeyPaste.MpConsole.WriteLine("Clipboard Monitor: Ignoring app '" + MpHelpers.Instance.GetProcessPath(hwnd) + "' with handle: " + hwnd);
                    } else {
                        await Task.Delay(1000);
                        ClipboardChanged?.Invoke(this, Clipboard.GetDataObject());
                        //MpHelpers.Instance.RunOnMainThread(OnClipboardChanged);
                        //Task.Run(OnClipboardChanged);
                    }
                }
            });       
        }


        public void Init(IntPtr thisAppHandle) {
            _Listener = new ClipboardListener(OnUpdate);
        }

        public void Dispose() {
            _Listener.DestroyHandle();
        }
    }

    public class MpClipboardListener : IDisposable {

        #region Private Varibles
        private bool _isStopped = false;
        private Dictionary<string, object> _lastCbo;
        private Thread _workThread;
        #endregion

        #region Properties
        public MpLastWindowWatcher LastWindowWatcher { get; set; }

        public bool IgnoreClipboardChangeEvent { get; set; } = false;
        #endregion

        #region Events
        public event EventHandler<IDataObject> ClipboardChanged;
        #endregion

        public MpClipboardListener() {
            _workThread = new Thread(new ThreadStart(CheckClipboard));
            _workThread.SetApartmentState(ApartmentState.STA);
            _workThread.IsBackground = true;
        }

        public void Start() {
            if (_workThread.IsAlive) {
                _isStopped = false;
            } else {
                //setting last here will ensure item on cb isn't added when starting
                _lastCbo = ConvertDpv(Clipboard.GetDataObject());
                _workThread.Start();
            }
        }

        public void Stop() {
            _isStopped = false;
        }

        private void CheckClipboard() {
            while (true) {
                while (_isStopped) {
                    Thread.Sleep(100);
                }
                var cbo = ConvertDpv(Clipboard.GetDataObject());
                if (HasChanged(cbo)) {
                    _lastCbo = cbo;
                    if (IgnoreClipboardChangeEvent) {
                        return;
                    } else {
                        if (MpAppModeViewModel.Instance.IsAppPaused) {
                            MonkeyPaste.MpConsole.WriteLine("App Paused, ignoring copy");
                        }
                        // TODO uncomment below, its commented out trying to debug ucrt issue
                        else if (MpApp.IsAppRejectedByPath(MpHelpers.Instance.GetProcessPath(LastWindowWatcher.LastHandle))) {
                            var hwnd = LastWindowWatcher.LastHandle;
                            MonkeyPaste.MpConsole.WriteLine("Clipboard Monitor: Ignoring app '" + MpHelpers.Instance.GetProcessPath(hwnd) + "' with handle: " + hwnd);
                        } else {
                            ClipboardChanged?.Invoke(this, Clipboard.GetDataObject());
                            //MpHelpers.Instance.RunOnMainThread(OnClipboardChanged);
                            //Task.Run(OnClipboardChanged);
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private Dictionary<string, object> ConvertDpv(IDataObject ido) {
            var formats = new string[] { DataFormats.Text, DataFormats.Html, DataFormats.Rtf, DataFormats.Bitmap, DataFormats.FileDrop };
            var cbDict = new Dictionary<string, object>();
            if (ido == null) {
                return cbDict;
            }
            foreach (var af in formats) {
                if (ido.GetDataPresent(af)) {

                    // TODO add checks for files and Images and convert: files to string seperated by NewLine, images to base 64
                    var data = ido.GetData(af, false);//MonkeyPaste.MpAsyncHelpers.RunSync<object>(() => dpv.GetDataAsync(af).AsTask());
                    cbDict.Add(af, data);
                }
                //var cbe = await cbo.GetDataAsync(af);
                //cbDict.Add(af, cbe);
            }
            return cbDict;
        }

        private bool HasChanged(Dictionary<string, object> nco) {
            if (_lastCbo == null && nco != null) {
                return true;
            }
            if (_lastCbo != null && nco == null) {
                return true;
            }
            if (_lastCbo.Count != nco.Count) {
                return true;
            }
            foreach (var nce in nco) {
                if (!_lastCbo.ContainsKey(nce.Key)) {
                    return true;
                }
                if (!_lastCbo[nce.Key].ToString().Equals(nce.Value)) {
                    return true;
                }
            }
            return false;
        }

        public virtual void Dispose() {
            if(_workThread != null) {
                _workThread.Abort();
            }
        }
    }
}
