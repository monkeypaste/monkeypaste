using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MpWinFormsClassLibrary {
    public class MpClipboardManager : Form {
        private MpLastWindowWatcher _lastWindowWatcher;
        public MpLastWindowWatcher LastWindowWatcher {
            get {

                return _lastWindowWatcher;
            }
            set {
                if(_lastWindowWatcher != value) {
                    _lastWindowWatcher = value;
                }
            }
        }
        private IntPtr _nextClipboardViewer;

        private System.ComponentModel.IContainer _components = null;


        private bool _ignoreNextClipboardEvent;
        public bool IgnoreNextClipboardEvent {
            get {
                return _ignoreNextClipboardEvent;
            }
            set {
                if(_ignoreNextClipboardEvent != value) {
                    _ignoreNextClipboardEvent = value;
                }
            }
        }

        public delegate void ClipboardChanged();
        public event ClipboardChanged ClipboardChangedEvent;

        public MpClipboardManager() : base() {
            this.SetBounds(0, 0, 0, 0);
            this.Load += MpClipboardController_Load;
        }
        public void Init() {
            this.Show();
            this.Hide();
        }


        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        public static void PressKey(Keys key, bool up) {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            if(up) {
                keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            } else {
                keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            }
        }

        public void PasteCopyItem(object itemToPaste) {
            IgnoreNextClipboardEvent = true;

            if(itemToPaste.GetType() == typeof(string)) {
                Clipboard.SetData(DataFormats.Text, (string)itemToPaste);
            }
            // TODO Add other objecy type pasters here
            //} else if(itemToPaste.GetType() == typeof(string)) {
            //    Clipboard.SetData(DataFormats.Text, (string)copyItem.GetData());
            //} else if (copyItem.CopyItemType == MpCopyItemType.HTMLText) {
            //    Clipboard.SetData(DataFormats.Text, (string)copyItem.GetData());
            //} else if (copyItem.CopyItemType == MpCopyItemType.Image) {
            //    Clipboard.SetImage((Image/*BitmapSource*/)MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])copyItem.GetData()));
            //} else if (copyItem.CopyItemType == MpCopyItemType.FileList) {
            //    Clipboard.SetFileDropList((StringCollection)copyItem.GetData());
            //}

            //WinApi.SetActiveWindow(GetLastWindowWatcher().LastHandle);
            //WinApi.SetForegroundWindow(LastWindowWatcher.LastHandle);
            SendKeys.SendWait("^v");
            //PressKey(Keys.ControlKey, false);
            //PressKey(Keys.V, false);
            //PressKey(Keys.V, true);
            //PressKey(Keys.ControlKey, true);
            
            IgnoreNextClipboardEvent = false;

            //only create to write to db

            //MpPasteHistory pasteHistory = new MpPasteHistory(copyItem, GetLastWindowWatcher().LastHandle);

            //MpSingletonController.Instance.AppendItem = null;
        }
        private void MpClipboardController_Load(object sender,EventArgs e) {
            LastWindowWatcher = new MpLastWindowWatcher(this.Handle);
            IgnoreNextClipboardEvent = true;
            //AddClipboardFormatListener(this.Handle);    // Add our window to the clipboard's format listener list.
            
            _nextClipboardViewer = (IntPtr)WinApi.SetClipboardViewer((int)this.Handle);
            IgnoreNextClipboardEvent = false;
        }
        
        protected override void WndProc(ref Message m) {          
            // defined in winuser.h
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;
            //const int WM_CLIPBOARDUPDATE = 0x031D;

            switch(m.Msg) {
                case WM_DRAWCLIPBOARD:                    
                    //Add clipboard item to copy list and create its copyItemPanel
                    try {
                        if(IgnoreNextClipboardEvent) {
                            break;
                        }
                        ClipboardChangedEvent();
                    }
                    catch(Exception e) {
                        Console.WriteLine(e.ToString());
                    }
                    WinApi.SendMessage(_nextClipboardViewer,m.Msg,m.WParam,m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if(m.WParam == _nextClipboardViewer)
                        _nextClipboardViewer = m.LParam;
                    else
                        WinApi.SendMessage(_nextClipboardViewer,m.Msg,m.WParam,m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }            
        }
        protected override void Dispose(bool disposing) {
            WinApi.ChangeClipboardChain(this.Handle,_nextClipboardViewer);
            //RemoveClipboardFormatListener(this.Handle);
            if(disposing && (_components != null)) {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // MpClipboardManager
            // 
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MpClipboardManager";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.ResumeLayout(false);

        }
    }
}
