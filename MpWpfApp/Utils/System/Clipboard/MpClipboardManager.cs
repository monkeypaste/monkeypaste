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
using System.Text;

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

    public class MpClipboardManager  {
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
            _workThread = new Thread(new ThreadStart(CheckClipboardThread));
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
                _workThread.Start();
            }

        }

        private void ClipboardMonitor_OnClipboardChange(object sender, object data) {
            var dobj = data as System.Windows.DataObject;
            if (dobj == null) {
                return;
            }
            var cbo = ConvertManagedFormats(dobj);
            if (HasChanged(cbo)) {
                _lastCbo = cbo;
                //if (IgnoreClipboardChangeEvent) {
                //    return;
                //}
                if (MpAppModeViewModel.Instance.IsAppPaused) {
                    MpConsole.WriteLine("App Paused, ignoring copy");
                } else {
                    IntPtr hwnd = LastWindowWatcher.LastHandle;
                    string processPath = MpHelpers.Instance.GetProcessPath(hwnd);
                    if (MpAppCollectionViewModel.Instance.IsAppRejected(processPath)) {
                        MpConsole.WriteLine("Clipboard Monitor: Ignoring app '" + MpHelpers.Instance.GetProcessPath(hwnd) + "' with handle: " + hwnd);
                    } else {
                        ClipboardChanged?.Invoke(this, cbo);
                        // NOTE word 2007 does weird stuff and alters cb after read
                        // this attempts to circumvent that by waiting a second
                        // then replacing _last with current
                        // NOTE 2 commenting this out because it itermittently
                        // creates duplicates...
                        //Thread.Sleep(1000);
                        //_lastCbo = ConvertManagedFormats(Clipboard.GetDataObject());
                    }
                }
            }
        }

        public void Stop() {
            _isStopped = false;
        }

        #region IDataObject Wrappers

        public IDataObject GetDataObjectWrapper() {
            MpConsole.WriteLine($"Accessing cb at {DateTime.Now}");
            return Clipboard.GetDataObject();
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
                } else if (iDataObject is IDataObject wf_ido) {
                    //Clipboard.SetDataObject(wf_ido, copy, retryTimes--, retryDelay);
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

        public async Task PasteDataObject(MpData dataObject, IntPtr handle) {
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

                await Task.Delay(300);
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

        public void SetDataObject(object dataObject) {
            MpHelpers.Instance.RunOnMainThread(() => {
                SetDataObjectWrapper(dataObject, true);
            });
        }

        #endregion

        #endregion

        #region Private Methods

        #region IDataObject Wrapper Helper methods
        

        private void CheckClipboardThread() {

            //setting last here will ensure item on cb isn't added when starting
            _lastCbo = ConvertManagedFormats(Clipboard.GetDataObject());

            //MpClipboardMonitor.OnClipboardChange += ClipboardMonitor_OnClipboardChange;
            //MpClipboardMonitor.Start();


            while (true) {
                continue;
                while (_isStopped || IgnoreClipboardChangeEvent) {
                    Thread.Sleep(100);
                }
                Thread.Sleep(500);

                //string test = GetOpenClipboardWindowText();
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
                            // NOTE 2 commenting this out because it itermittently
                            // creates duplicates...
                            //Thread.Sleep(1000);
                            //_lastCbo = ConvertManagedFormats(Clipboard.GetDataObject());
                        }                        
                    }
                }
                Thread.Sleep(500);
            }
        }

        private Dictionary<string, string> ConvertManagedFormats(object ido, int retryCount = 5) {
            /*
            from: https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.dataobject?view=windowsdesktop-6.0&viewFallbackFrom=net-5.0
            Special considerations may be necessary when using the metafile format with the Clipboard. 
            Due to a limitation in the current implementation of the DataObject class, the metafile format 
            used by the .NET Framework may not be recognized by applications that use an older metafile format.
            In this case, you must interoperate with the Win32 Clipboard application programming interfaces (APIs).

            An object must be serializable for it to be put on the Clipboard. 
            See System.Runtime.Serialization for more information on serialization. 
            If your target application requires a very specific data format, the headers 
            added to the data in the serialization process may prevent the application from 
            recognizing your data. To preserve your data format, add your data as a Byte array 
            to a MemoryStream and pass the MemoryStream to the SetData method.
            */
            var cbDict = new Dictionary<string, string>();
            if(retryCount == 0) {
                MpConsole.WriteLine("Exceeded retry limit accessing clipboard, ignoring");
                return cbDict;
            }
            try {
                if(ido==null) {
                    ido = Clipboard.GetDataObject();
                }
                DataObject dobj = (DataObject)ido;
                if(dobj == null) {
                    return cbDict;
                }
                foreach (var af in _managedDataFormats) {
                    object data = null;
                    if (dobj.GetDataPresent(af)) {
                        switch(af) {
                            case nameof(DataFormats.Text):
                                data = dobj.GetText(TextDataFormat.Text);
                                break;
                            case nameof(DataFormats.UnicodeText):
                                data = dobj.GetText(TextDataFormat.UnicodeText);
                                break;
                            case nameof(DataFormats.Rtf):
                                data = dobj.GetText(TextDataFormat.Rtf);
                                break;
                            case nameof(DataFormats.CommaSeparatedValue):
                                data = dobj.GetText(TextDataFormat.CommaSeparatedValue);
                                break;
                            case nameof(DataFormats.Html):
                                data = dobj.GetText(TextDataFormat.Html);
                                break;
                            case nameof(DataFormats.Bitmap):
                                data = dobj.GetImage().ToBase64String();
                                break;
                            case nameof(DataFormats.FileDrop):
                                StringCollection sc =dobj.GetFileDropList();
                                data = string.Join(Environment.NewLine, sc.ToArray());
                                break;
                        }
                        if (data == null) {
                            data = dobj.GetData(af, true);
                        }
                        if (data != null) {
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
