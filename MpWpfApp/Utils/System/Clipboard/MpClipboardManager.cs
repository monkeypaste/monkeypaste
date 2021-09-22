using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WindowsInput;
using WindowsInput.Native;
using MonkeyPaste;

namespace MpWpfApp {
    public enum MpPasteDataFormats {
        None = 0,
        PlainText,
        Csv,
        RichText,
        Html,
        Bitmap,
        FileList
    }

    public class MpClipboardManager : IDisposable {
        private static readonly Lazy<MpClipboardManager> _Lazy = new Lazy<MpClipboardManager>(() => new MpClipboardManager());
        public static MpClipboardManager Instance { get { return _Lazy.Value; } }

        private InputSimulator sim = null;

        private const int WM_DRAWCLIPBOARD = 0x0308;
        private const int WM_CHANGECBCHAIN = 0x030D;
        private const int WM_PASTE = 0x0302;

        public MpLastWindowWatcher LastWindowWatcher { get; set; }

        public bool IgnoreClipboardChangeEvent { get; set; }

        public MpCopyHelper Copy { get; private set; }
        public MpPasteHelper Paste { get; private set; }

        private HwndSourceHook hook;
        private HwndSource hwndSource;
        private IntPtr _nextClipboardViewer;
        private IDictionary<string, object> _lastDataObject = null;

        private MpClipboardManager() {
        }

        public void Init() {
            sim = new InputSimulator();

            UIPermission clipBoard = new UIPermission(PermissionState.None);
            clipBoard.Clipboard = UIPermissionClipboard.AllClipboard;

            HwndSource hwnd = (HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow);
            LastWindowWatcher = new MpLastWindowWatcher(hwnd.Handle);

            hwndSource = hwnd;
            hook = new HwndSourceHook(WndProc);
            hwndSource.AddHook(hook);

            IgnoreClipboardChangeEvent = true;
            _nextClipboardViewer = WinApi.SetClipboardViewer(hwndSource.Handle);
            IgnoreClipboardChangeEvent = false;

            Copy = new MpCopyHelper();
            Paste = new MpPasteHelper();
        }

        public event EventHandler ClipboardChanged;
        protected virtual void OnClipboardChanged() => ClipboardChanged?.Invoke(this, EventArgs.Empty);

        public void CopyItemsToClipboard(List<MpCopyItem> cil, params object[] formatOrder) {
            if (formatOrder.Length == 0) {
                if (cil.All(x => x.ItemType == MpCopyItemType.Image)) {
                    formatOrder = new object[] {
                        DataFormats.Bitmap
                    };
                } else {
                    formatOrder = new object[] {
                        DataFormats.Rtf,
                        DataFormats.Text
                    };
                }
            }
            IDataObject ido = new DataObject();
            foreach (string format in formatOrder) {
                if (format == DataFormats.FileDrop) {
                    ido.SetData(format, MpCopyItemMerger.Instance.MergeFilePaths(cil));
                } else if (format == DataFormats.Bitmap) {
                    ido.SetData(format, MpCopyItemMerger.Instance.MergeBitmaps(cil));
                } else if (format == DataFormats.Rtf) {
                    ido.SetData(format, MpCopyItemMerger.Instance.MergeRtf(cil));
                } else if (format == DataFormats.Text) {
                    ido.SetData(format, MpCopyItemMerger.Instance.MergePlainText(cil));
                }
            }

            SetDataObject(ido);
        }

        public void PasteDataObject(IDataObject dataObject, IntPtr handle) {
            //Mouse.OverrideCursor = Cursors.Wait;
            IgnoreClipboardChangeEvent = true;
            try {
                if(Properties.Settings.Default.ResetClipboardAfterMonkeyPaste) {
                    _lastDataObject = GetClipboardData();
                }

                Clipboard.SetDataObject(dataObject);
                WinApi.SetForegroundWindow(handle);
                WinApi.SetActiveWindow(handle);
                System.Windows.Forms.SendKeys.SendWait("^v");

                if(Properties.Settings.Default.ResetClipboardAfterMonkeyPaste) {
                    //from https://stackoverflow.com/a/52438404/105028
                    var clipboardThread = new Thread(new ThreadStart(GetClipboard));
                    clipboardThread.SetApartmentState(ApartmentState.STA);
                    clipboardThread.Start();
                }
                IgnoreClipboardChangeEvent = false;
            }
            catch (Exception e) {
                MonkeyPaste.MpConsole.WriteLine("ClipboardMonitor error during paste: " + e.ToString());
            }
            //Mouse.OverrideCursor = null;
        }

        public void SetDataObject(IDataObject dataObject) {
            MpHelpers.Instance.RunOnMainThread(() => {
                Clipboard.SetDataObject(dataObject, true);
            });            
        }

        private IDictionary<string, object> GetClipboardData() {
            var dict = new Dictionary<string, object>();
            var dataObject = Clipboard.GetDataObject();
            foreach (var format in dataObject.GetFormats()) {
                dict.Add(format, dataObject.GetData(format));
            }
            return dict;
        }

        private void SetClipboardData(IDictionary<string, object> dict) {
            var d = new DataObject();
            foreach (var kvp in dict) {
                d.SetData(kvp.Key, kvp.Value);
            }
            System.Windows.Forms.Clipboard.SetDataObject(d, true, 10, 100);
            Thread.Sleep(1000);
            IgnoreClipboardChangeEvent = false;
        }

        private void GetClipboard() {
            SetClipboardData(_lastDataObject);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_DRAWCLIPBOARD:
                    if (IgnoreClipboardChangeEvent) {
                        //do nothing
                    } else {
                        if(MpAppModeViewModel.Instance.IsAppPaused) {
                            MonkeyPaste.MpConsole.WriteLine("App Paused, ignoring copy");
                        }
                        else if(MpApp.IsAppRejectedByPath(MpHelpers.Instance.GetProcessPath(LastWindowWatcher.LastHandle))) {
                            MonkeyPaste.MpConsole.WriteLine("Clipboard Monitor: Ignoring app '" + MpHelpers.Instance.GetProcessPath(hwnd) + "' with handle: " + hwnd);
                        } else {
                            //MpHelpers.Instance.RunOnMainThread(OnClipboardChanged);
                            Task.Run(OnClipboardChanged);
                        }
                    }
                    if(_nextClipboardViewer != LastWindowWatcher.ThisAppHandle) {
                        WinApi.SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    }
                    
                    break;
                case WM_CHANGECBCHAIN:
                    if (wParam == _nextClipboardViewer) {
                        _nextClipboardViewer = lParam;
                    } else {
                        if (_nextClipboardViewer != LastWindowWatcher.ThisAppHandle) {
                            WinApi.SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                        }                            
                    }
                    break;
            }
            return IntPtr.Zero;
        }
                
        #region Destructor
        private bool disposed;
        protected virtual void Dispose(bool disposing) {
            if (disposed) {
                return;
            }
            if (disposing) {
                WinApi.ChangeClipboardChain(hwndSource.Handle, _nextClipboardViewer);
                hwndSource.RemoveHook(hook);
            }
            disposed = true;
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~MpClipboardManager() {
            Dispose(false);
        }
        #endregion
    }
}
