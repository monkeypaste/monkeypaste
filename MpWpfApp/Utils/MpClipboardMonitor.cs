
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpClipboardMonitor : IDisposable {
        private MpLastWindowWatcher _lastWindowWatcher;
        public MpLastWindowWatcher LastWindowWatcher {
            get {

                return _lastWindowWatcher;
            }
            set {
                if (_lastWindowWatcher != value) {
                    _lastWindowWatcher = value;
                }
            }
        }

        private bool _ignoreNextClipboardEvent;
        public bool IgnoreClipboardChangeEvent {
            get {
                return _ignoreNextClipboardEvent;
            }
            set {
                if (_ignoreNextClipboardEvent != value) {
                    _ignoreNextClipboardEvent = value;
                }
            }
        }

        private readonly HwndSourceHook hook;
        private readonly HwndSource hwndSource;
        private IntPtr _nextClipboardViewer;

        public MpClipboardMonitor(HwndSource hwnd) {
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

        private const int WM_DRAWCLIPBOARD = 0x0308;
        private const int WM_CHANGECBCHAIN = 0x030D;
        private const int WM_PASTE = 0x0302;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case WM_DRAWCLIPBOARD:
                    if(IgnoreClipboardChangeEvent) {
                        //do nothing
                    } else {
                        OnClipboardChanged();                        
                    }
                    WinApi.SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (wParam == _nextClipboardViewer)
                        _nextClipboardViewer = lParam;
                    else
                        WinApi.SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                    break;
                //case WM_PASTE:
                //    Console.WriteLine("Pasted");
                //    break;
            }
            return IntPtr.Zero;
        }

        public void PasteCopyItem(MpCopyItem ci) {
            IgnoreClipboardChangeEvent = true;

            switch(ci.CopyItemType) {
                case MpCopyItemType.RichText:
                    Clipboard.SetData(DataFormats.Rtf, (string)ci.DataObject);
                    Clipboard.SetData(DataFormats.Text, ci.GetPlainText());
                    break;
                case MpCopyItemType.Image:
                    Clipboard.SetData(DataFormats.Bitmap, ci.DataObject);
                    Clipboard.SetData(DataFormats.Text, ci.GetPlainText());
                    break;
                case MpCopyItemType.FileList:
                    Clipboard.SetData(DataFormats.FileDrop,(StringCollection)ci.DataObject);
                    Clipboard.SetData(DataFormats.Text, ci.GetPlainText());
                    break;
            }
            // TODO Add other objecy type pasters here
            //} else if(itemToPaste.GetType() == typeof(string)) {
            //    Clipboard.SetData(DataFormats.Text, (string)CopyItem.DataObject);
            //} else if (copyItem.CopyItemType == MpCopyItemType.HTMLText) {
            //    Clipboard.SetData(DataFormats.Text, (string)CopyItem.DataObject);
            //} else if (copyItem.CopyItemType == MpCopyItemType.Image) {
            //    Clipboard.SetImage((Image/*BitmapSource*/)MpHelpers.ConvertByteArrayToImage((byte[])CopyItem.DataObject));
            //} else if (copyItem.CopyItemType == MpCopyItemType.FileList) {
            //    Clipboard.SetFileDropList((StringCollection)CopyItem.DataObject);
            //}

            //WinApi.SetActiveWindow(GetLastWindowWatcher().LastHandle);
            WinApi.SetForegroundWindow(LastWindowWatcher.LastHandle);
            //System.Windows.Forms.SendKeys.Send("^v");
            System.Windows.Forms.SendKeys.SendWait("^v");
            //PressKey(Keys.ControlKey, false);
            //PressKey(Keys.V, false);
            //PressKey(Keys.V, true);
            //PressKey(Keys.ControlKey, true);

            IgnoreClipboardChangeEvent = false;

            //creating history item automatically saves it to the db
            MpPasteHistory pasteHistory = new MpPasteHistory(ci, ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).ClipboardMonitor.LastWindowWatcher.LastHandle);

            //MpSingletonController.Instance.AppendItem = null;
        }
        #region Destructor
        private bool disposed;
        protected virtual void Dispose(bool disposing) {
            if (disposed) return;
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
        ~MpClipboardMonitor() {
            Dispose(false);
        }
        #endregion
    }
}
