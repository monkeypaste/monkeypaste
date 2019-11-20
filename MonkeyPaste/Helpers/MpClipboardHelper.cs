using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace MonkeyPaste {
    public class MpClipboardHelper : Form {
        private MpLastWindowWatcher _lastWindowWatcher;
        private IntPtr _nextClipboardViewer;

        private System.ComponentModel.IContainer _components = null;

        public delegate void ClipboardChanged(object sender,MpCopyItem copyItem);
        public event ClipboardChanged ClipboardChangedEvent;

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
            _lastWindowWatcher = new MpLastWindowWatcher(this.Handle);
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
            //AddClipboardFormatListener(this.Handle);    // Add our window to the clipboard's format listener list.
            this.SetBounds(0,0,0,0);
            _nextClipboardViewer = (IntPtr)WinApi.SetClipboardViewer((int)this.Handle);
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
                            WinApi.SendMessage(_nextClipboardViewer,m.Msg,m.WParam,m.LParam);
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
                            MpCopyItem ci = null;
                            if(iData.GetDataPresent(DataFormats.Bitmap)) {
                                ci = MpCopyItem.CreateCopyItem(0,MpCopyItemType.Image,MpLogFormController.Db.Client.MpClientId,0,(Image)iData.GetData(DataFormats.Bitmap,true),_lastWindowWatcher.LastHandle);// CreateCopyItem(0,MpCopyItemType.None,0,0,null,IntPtr.Zero);// ((Image)iData.GetData(DataFormats.Bitmap,true),sourceHandle);
                            }
                            else if(iData.GetDataPresent(DataFormats.FileDrop)) {
                                ci = MpCopyItem.CreateCopyItem(0,MpCopyItemType.FileList,MpLogFormController.Db.Client.MpClientId,0,(string[])iData.GetData(DataFormats.FileDrop,true),_lastWindowWatcher.LastHandle);
                            }
                            else if(iData.GetDataPresent(DataFormats.Rtf)) {
                                ci = MpCopyItem.CreateCopyItem(0,MpCopyItemType.Text,MpLogFormController.Db.Client.MpClientId,0,(string)iData.GetData(DataFormats.Text),_lastWindowWatcher.LastHandle);
                            }
                            else if(iData.GetDataPresent(DataFormats.Html)) {
                                ci = MpCopyItem.CreateCopyItem(0,MpCopyItemType.Text,MpLogFormController.Db.Client.MpClientId,0,(string)iData.GetData(DataFormats.Text),_lastWindowWatcher.LastHandle);
                            }
                            else if(iData.GetDataPresent(DataFormats.Text)) {
                                ci = MpCopyItem.CreateCopyItem(0,MpCopyItemType.Text,MpLogFormController.Db.Client.MpClientId,0,(string)iData.GetData(DataFormats.Text),_lastWindowWatcher.LastHandle);
                            }
                            else {
                                Console.WriteLine("MpData error clipboard data is not known format");
                                return;
                            }
                            if(ci.copyItemTypeId == MpCopyItemType.Text && (ci.GetData() == null || (string)ci.GetData() == string.Empty)) {
                                ci = null;
                            }
                            if(ci != null) {
                                ClipboardChangedEvent(this,ci);
                            }
                        }
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
