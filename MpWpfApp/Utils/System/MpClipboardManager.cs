using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;

namespace MpWpfApp {
    public class MpClipboardManager : IDisposable {
        private const int WM_DRAWCLIPBOARD = 0x0308;
        private const int WM_CHANGECBCHAIN = 0x030D;
        private const int WM_PASTE = 0x0302;

        public MpLastWindowWatcher LastWindowWatcher { get; set; }

        public bool IgnoreClipboardChangeEvent { get; set; }

        private readonly HwndSourceHook hook;
        private readonly HwndSource hwndSource;
        private IntPtr _nextClipboardViewer;

        public MpClipboardManager(HwndSource hwnd) {
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

        public void PasteDataObject(IDataObject dataObject) {
            IgnoreClipboardChangeEvent = true;
            try {
                Clipboard.Clear();
                Clipboard.SetDataObject(dataObject);
                WinApi.SetForegroundWindow(LastWindowWatcher.LastHandle);
                System.Windows.Forms.SendKeys.SendWait("^v");
            }
            catch (Exception e) {
                Console.WriteLine("ClipboardMonitor error during paste: " + e.ToString());
            }
            IgnoreClipboardChangeEvent = false;
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_DRAWCLIPBOARD:
                    if (IgnoreClipboardChangeEvent) {
                        //do nothing
                    } else {
                        if(MpApp.IsAppRejectedByHandle(LastWindowWatcher.LastHandle)) {
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
