
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MpWpfApp {

    public class MpClipboardMonitoringWindow : Window {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private IntPtr windowHandle;

        public event EventHandler ClipboardUpdate;

        protected override void OnInitialized(EventArgs e) {
            base.OnInitialized(e);

            windowHandle = new WindowInteropHelper(this).EnsureHandle();
            HwndSource.FromHwnd(windowHandle)?.AddHook(HwndHandler);
            Start();
        }

        public static readonly DependencyProperty ClipboardUpdateCommandProperty = DependencyProperty.Register("ClipboardUpdateCommand", typeof(ICommand), typeof(MpClipboardMonitoringWindow), new FrameworkPropertyMetadata(null));

        public ICommand ClipboardUpdateCommand {
            get { return (ICommand)GetValue(ClipboardUpdateCommandProperty); }
            set { SetValue(ClipboardUpdateCommandProperty, value); }
        }

        protected virtual void OnClipboardUpdate() { }

        public void Start() {
            WinApi.AddClipboardFormatListener(windowHandle);
        }

        public void Stop() {
            WinApi.RemoveClipboardFormatListener(windowHandle);
        }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled) {
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;
            //const int WM_CLIPBOARDUPDATE = 0x031D;

            switch(msg) {
                case WM_DRAWCLIPBOARD:
                    Console.WriteLine("Clipboard update");
                    // fire event
                    this.ClipboardUpdate?.Invoke(this, new EventArgs());
                    // execute command
                    if(this.ClipboardUpdateCommand?.CanExecute(null) ?? false) {
                        this.ClipboardUpdateCommand?.Execute(null);
                    }
                    // call virtual method
                    OnClipboardUpdate();
                    break;
                case WM_CLIPBOARDUPDATE:
                    // fire event
                    this.ClipboardUpdate?.Invoke(this, new EventArgs());
                    Console.WriteLine("Clipboard: " + Clipboard.GetText()); 
                    // execute command
                    if(this.ClipboardUpdateCommand?.CanExecute(null) ?? false) {
                        this.ClipboardUpdateCommand?.Execute(null);
                    }
                    // call virtual method
                    OnClipboardUpdate();
                    break;
                //case WM_CHANGECBCHAIN:
                //    if(m.WParam == _nextClipboardViewer)
                //        _nextClipboardViewer = m.LParam;
                //    else
                //        WinApi.SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                //    break;

                //default:
                //    base.WndProc(ref m);
                //    break;
            }
            handled = false;
            return IntPtr.Zero;
        }
    }
}
