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

        #region Private Varibles

        private readonly string[] _managedDataFormats = { 
            DataFormats.UnicodeText, 
            DataFormats.Text, 
            DataFormats.Html, 
            DataFormats.Rtf, 
            DataFormats.Bitmap, 
            DataFormats.FileDrop,
            DataFormats.CommaSeparatedValue
        };

        private InputSimulator sim = null;

        private bool _isStopped = false;
        private Dictionary<string, string> _lastCbo;
        private Thread _workThread;

        private IDictionary<string, object> _lastDataObject = null;
        #endregion

        #region Children

        public MpLastWindowWatcher LastWindowWatcher { get; set; }

        public MpCopyHelper Copy { get; private set; }
        public MpPasteHelper Paste { get; private set; }

        #endregion

        #region Properties

        public bool IgnoreClipboardChangeEvent { get; set; } = false;

        #endregion

        #region Events

        public event EventHandler<Dictionary<string, string>> ClipboardChanged;

        #endregion

        #region Constructor

        private MpClipboardManager() : base() {
            _workThread = new Thread(new ThreadStart(CheckClipboard));
            _workThread.SetApartmentState(ApartmentState.STA);
            _workThread.IsBackground = true;
        }

        #endregion

        #region Public Methods
        public void Init() {
            sim = new InputSimulator();

            UIPermission clipBoard = new UIPermission(PermissionState.Unrestricted);
            clipBoard.Clipboard = UIPermissionClipboard.AllClipboard;

            LastWindowWatcher = new MpLastWindowWatcher();

            Start();

            Copy = new MpCopyHelper();
            Paste = new MpPasteHelper();
        }

        public void Start() {
            if (_workThread.IsAlive) {
                _isStopped = false;
            } else {
                //setting last here will ensure item on cb isn't added when starting
                _lastCbo = ConvertManagedFormats(Clipboard.GetDataObject());
                _workThread.Start();
            }
        }

        public void Stop() {
            _isStopped = false;
        }

        #region IDataObject Wrappers

        public System.Windows.Forms.IDataObject GetDataObjectWrapper() {
            MpConsole.WriteLine($"Accessing cb at {DateTime.Now}");
            return System.Windows.Forms.Clipboard.GetDataObject();
            //return Clipboard.GetDataObject();
        }

        public void SetDataObjectWrapper(object iDataObject, bool copy = false, int retryTimes = 5, int retryDelay = 0) {
            if(retryTimes == 0) {
                MpConsole.WriteTraceLine("Could not open clipboard ignoring setting data object");
                return;
            }
            try {
                if (iDataObject is IDataObject ido) {
                    //throw new Exception("Try converting to win forms version");
                    Clipboard.SetDataObject(iDataObject as IDataObject, copy);
                } else if (iDataObject is System.Windows.Forms.IDataObject wf_ido) {
                    System.Windows.Forms.Clipboard.SetDataObject(wf_ido, copy, retryTimes--, retryDelay);
                }
            } catch(Exception ex) {
                MpConsole.WriteTraceLine(ex);
                SetDataObjectWrapper(iDataObject, copy, retryTimes, retryDelay);
            }
        }

        public void SetImageWrapper(BitmapSource bmpSrc) {
            MpClipboardManager.Instance.SetImageWrapper(bmpSrc);
        }

        public BitmapSource GetImageWrapper() {
            return Clipboard.GetImage();
        }

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
            //to prevent cb listener thread from thinking there's a new item
            _lastCbo = ConvertManagedFormats(dataObject);
            //Mouse.OverrideCursor = Cursors.Wait;
            IgnoreClipboardChangeEvent = true;
            try {
                if (MpPreferences.Instance.ResetClipboardAfterMonkeyPaste) {
                    _lastDataObject = GetClipboardData();
                }

                SetDataObjectWrapper(dataObject);
                WinApi.SetForegroundWindow(handle);
                WinApi.SetActiveWindow(handle);
                System.Windows.Forms.SendKeys.SendWait("^v");

                if (MpPreferences.Instance.ResetClipboardAfterMonkeyPaste) {
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
                SetDataObjectWrapper(dataObject, true);
            });
        }

        #endregion

        #endregion

        #region Private Methods

        #region IDataObject Wrapper Helper methods

        private void CheckClipboard() {
            while (true) {
                while (_isStopped || IgnoreClipboardChangeEvent) {
                    Thread.Sleep(100);
                }
                var cbo = ConvertManagedFormats(Clipboard.GetDataObject());
                if (HasChanged(cbo)) {
                    _lastCbo = cbo;
                    //if (IgnoreClipboardChangeEvent) {
                    //    return;
                    //}
                    if (MpAppModeViewModel.Instance.IsAppPaused) {
                        MpConsole.WriteLine("App Paused, ignoring copy");
                    } else  {
                        IntPtr hwnd = LastWindowWatcher.LastHandle;
                        string processPath = MpHelpers.Instance.GetProcessPath(hwnd);
                        if(MpAppCollectionViewModel.Instance.IsAppRejected(processPath)) {
                            MpConsole.WriteLine("Clipboard Monitor: Ignoring app '" + MpHelpers.Instance.GetProcessPath(hwnd) + "' with handle: " + hwnd);
                        } else {
                            ClipboardChanged?.Invoke(this, cbo);
                            // NOTE word 2007 does weird stuff and alters cb after read
                            // this attempts to circumvent that by waiting a second
                            // then replacing _last with current
                            Thread.Sleep(1000);
                            _lastCbo = ConvertManagedFormats(Clipboard.GetDataObject());
                        }                        
                    }
                }
                Thread.Sleep(500);
            }
        }

        private Dictionary<string, string> ConvertManagedFormats(IDataObject ido, int retryCount = 5) {
            var cbDict = new Dictionary<string, string>();
            if(retryCount == 0) {
                MpConsole.WriteLine("Exceeded retry limit accessing clipboard, ignoring");
                return cbDict;
            }
            try {
                if(ido == null) {
                    ido = Clipboard.GetDataObject();
                }
                foreach (var af in _managedDataFormats) {
                    if (ido.GetDataPresent(af)) {
                        object data = ido.GetData(af, false);
                        if (data == null) {
                            data = ido.GetData(af, true);
                        }
                        if (data != null) {
                            if (af == DataFormats.FileDrop) {
                                var sa = data as string[];
                                data = string.Join(Environment.NewLine, sa);
                            } else if (af == DataFormats.Bitmap && data is BitmapSource bmpSrc) {
                                data = bmpSrc.ToBase64String();
                            }
                            cbDict.Add(af, data.ToString());
                        }
                    }
                }
                return cbDict;
            } catch(Exception ex) {
                MpConsole.WriteLine($"Error accessing clipboard {retryCount} attempts remaining", ex);
                Thread.Sleep((5 - retryCount) * 100);
                return ConvertManagedFormats(ido, retryCount--);
            }
        }

        private bool HasChanged(Dictionary<string, string> nco) {
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
                if (!_lastCbo[nce.Key].Equals(nce.Value)) {
                    if(nce.Key == DataFormats.Rtf) {
                        // NOTE when clipboard has data from MS Word (maybe other office apps too)
                        // it alters the rtf returned (probably makes unique for each return).
                        // To account for this convert each operand to plain text and compare...
                        string lastPt = _lastCbo[nce.Key].ToPlainText();
                        string curPt = nce.Value.ToPlainText();
                        if (!lastPt.Equals(curPt)) {
                            return true;
                        }
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        private IDictionary<string, object> GetClipboardData() {
            var dict = new Dictionary<string, object>();
            var dataObject = GetDataObjectWrapper();
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
            SetDataObjectWrapper(d, true, 10, 100);
            Thread.Sleep(1000);
            IgnoreClipboardChangeEvent = false;
        }

        private void GetClipboard() {
            SetClipboardData(_lastDataObject);
        }

        #endregion

        #endregion

        #region IDispose & Destructor

        private bool _disposed;
        protected virtual void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                if (_workThread != null) {
                    _workThread.Abort();
                }
            }
            _disposed = true;
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
