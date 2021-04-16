using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpClipboardManager : IDisposable {
        private static readonly Lazy<MpClipboardManager> _Lazy = new Lazy<MpClipboardManager>(() => new MpClipboardManager());
        public static MpClipboardManager Instance { get { return _Lazy.Value; } }

        private const int WM_DRAWCLIPBOARD = 0x0308;
        private const int WM_CHANGECBCHAIN = 0x030D;
        private const int WM_PASTE = 0x0302;

        public MpLastWindowWatcher LastWindowWatcher { get; set; }

        public bool IgnoreClipboardChangeEvent { get; set; }

        private HwndSourceHook hook;
        private HwndSource hwndSource;
        private IntPtr _nextClipboardViewer;
        private bool _wasLastDataCsv = false;
        private object _lastDataObject = null;

        private MpClipboardManager() {
        }

        public void Init() {
            HwndSource hwnd = (HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow);
            LastWindowWatcher = new MpLastWindowWatcher(hwnd.Handle);

            hwndSource = hwnd;
            hook = new HwndSourceHook(WndProc);
            hwndSource.AddHook(hook);

            IgnoreClipboardChangeEvent = true;
            _nextClipboardViewer = WinApi.SetClipboardViewer(hwndSource.Handle);
            IgnoreClipboardChangeEvent = false;
        }

        public event EventHandler ClipboardChanged;
        protected virtual void OnClipboardChanged() => ClipboardChanged?.Invoke(this, EventArgs.Empty);

        public void PasteDataObject(IDataObject dataObject, IntPtr handle) {
            IgnoreClipboardChangeEvent = true;
            try {
                if(!string.IsNullOrEmpty(Clipboard.GetText())) {
                    _lastDataObject = Clipboard.GetText();
                    _wasLastDataCsv = MpHelpers.Instance.IsStringCsv(_lastDataObject.ToString());
                }
                Clipboard.Clear();
                Clipboard.SetDataObject(dataObject);
                WinApi.SetForegroundWindow(handle);
                WinApi.SetActiveWindow(handle);
                System.Windows.Forms.SendKeys.SendWait("^v");
                //WinApi.SendMessage(LastWindowWatcher.ThisAppHandle, WM_PASTE, IntPtr.Zero, IntPtr.Zero);
                if (_lastDataObject != null) {
                    //Thread.Sleep(500);
                    // from https://stackoverflow.com/a/52438404/105028
                    _remainingTryCount = 100;
                    var clipboardThread = new Thread(new ThreadStart(GetClipboard));
                    clipboardThread.SetApartmentState(ApartmentState.STA);
                    clipboardThread.Start();
                    Thread.Sleep(500);
                }
                IgnoreClipboardChangeEvent = false;
            }
            catch (Exception e) {
                Console.WriteLine("ClipboardMonitor error during paste: " + e.ToString());
            }
        }

        private int _remainingTryCount = 0;

        private void GetClipboard() {
            if(_remainingTryCount < 0) {
                return;
            }
            try {
                if (_lastDataObject is string) {
                    Clipboard.SetText(_lastDataObject.ToString());
                } else if(_lastDataObject is StringCollection) {
                    Clipboard.SetFileDropList((StringCollection)_lastDataObject);
                } else if(_lastDataObject is BitmapSource) {
                    Clipboard.SetImage((BitmapSource)_lastDataObject);
                } else if (_lastDataObject is InteropBitmap) {
                    Clipboard.SetImage((InteropBitmap)_lastDataObject);
                 } else {
                    Console.WriteLine("Warning could reset clipboard data object with data: " + _lastDataObject.ToString());
                }
            }
            catch(Exception ex) {
                Console.WriteLine("Error reseting clipboard " + _remainingTryCount);
                Console.WriteLine(ex.ToString());
                _remainingTryCount--;
                GetClipboard();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_DRAWCLIPBOARD:
                    if (IgnoreClipboardChangeEvent) {
                        //do nothing
                    } else {
                        if(((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).AppModeViewModel.IsAppPaused) {
                            Console.WriteLine("App Paused, ignoring copy");
                        }
                        else if(MpApp.IsAppRejectedByHandle(LastWindowWatcher.LastHandle)) {
                            Console.WriteLine("Clipboard Monitor: Ignoring app '" + MpHelpers.Instance.GetProcessPath(hwnd) + "' with handle: " + hwnd);
                        } else {
                            OnClipboardChanged();
                        }
                    }
                    WinApi.SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (wParam == _nextClipboardViewer) {
                        _nextClipboardViewer = lParam;
                    } else {
                        WinApi.SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    }
                    break;
                //case WM_PASTE:
                //    Console.WriteLine("Pasted");
                    
                //    break;
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
