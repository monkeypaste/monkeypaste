using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace MonkeyPaste {
    public class MpClipboardHelper : Form {
        #region WIN32 API  
        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll",CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove,IntPtr hWndNewNext);

        [DllImport("user32.dll",CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd,int wMsg,IntPtr wParam,IntPtr lParam);

        /// <summary>
        /// Places the given window in the system-maintained clipboard format listener list.
        /// </summary>
        [DllImport("user32.dll",SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AddClipboardFormatListener(IntPtr hwnd);
        /// <summary>
        /// Removes the given window from the system-maintained clipboard format listener list.
        /// </summary>
        [DllImport("user32.dll",SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// Sent when the contents of the clipboard have changed.
        /// </summary>
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        #endregion

        private MpLastWindowWatcher _lastWindowWatcher;
        private IntPtr _nextClipboardViewer;

        private System.ComponentModel.IContainer _components = null;

        public MpClipboardHelper() : base() {
            this.Load += MpClipboardController_Load;
        }
        public void Init() {
            this.Show();
            this.Hide();
        }
        public MpLastWindowWatcher GetLastWindowWatcher() {
            return _lastWindowWatcher;
        }
        private void MpClipboardController_Load(object sender,EventArgs e) {
            _lastWindowWatcher = new MpLastWindowWatcher();
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
            //AddClipboardFormatListener(this.Handle);    // Add our window to the clipboard's format listener list.
            this.SetBounds(0,0,0,0);
            _nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);
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
                        if(MpSingletonController.Instance.GetIgnoreNextClipboardEvent()) {
                            //MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);
                            //SendMessage(_nextClipboardViewer,m.Msg,m.WParam,m.LParam);
                            //base.WndProc(ref m);
                            return;
                            //break;
                        }
                        IDataObject iData = new DataObject();
                        try {
                            iData = Clipboard.GetDataObject();
                        }
                        catch(Exception e) {
                            Console.WriteLine("Error reading clipboard object: "+e.ToString());
                            return;
                        }
                        if(iData != null) {
                            MpSingletonController.Instance.GetMpData().AddMpCopyItem(iData,_lastWindowWatcher.LastHandle);
                        }
                    }
                    catch(Exception e) {
                        Console.WriteLine(e.ToString());
                    }
                    SendMessage(_nextClipboardViewer,m.Msg,m.WParam,m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if(m.WParam == _nextClipboardViewer)
                        _nextClipboardViewer = m.LParam;
                    else
                        SendMessage(_nextClipboardViewer,m.Msg,m.WParam,m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }            
        }
        protected override void Dispose(bool disposing) {
            ChangeClipboardChain(this.Handle,_nextClipboardViewer);
            //RemoveClipboardFormatListener(this.Handle);
            if(disposing && (_components != null)) {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent() {
            this.SuspendLayout();
            // 
            // MpClipboardController
            // 
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MpClipboardController";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }
    }
}
