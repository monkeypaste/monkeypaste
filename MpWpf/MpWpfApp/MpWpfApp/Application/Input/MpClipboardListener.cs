using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste
{
    public class MpClipboardListener : Form {
        private MpLastWindowWatcher _lastWindowWatcher;
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
        public delegate void ClipboardChanged(object sender,MpCopyItem copyItem);
        public event ClipboardChanged ClipboardChangedEvent;

        public MpClipboardListener() : base() {
            this.Load += MpClipboardController_Load;
        }
        public void Init() {
            this.Show();
            this.Hide();
        }

        public void PasteCopyItem(MpCopyItem copyItem) {
            IgnoreNextClipboardEvent = true;
            //MpCopyItem copyItem = LogFormPanelController.TileChooserPanelController.SelectedTilePanelController.CopyItem;

            if (copyItem.CopyItemType == MpCopyItemType.Text) {
                Clipboard.SetData(DataFormats.Text, (string)copyItem.GetData());
            } else if (copyItem.CopyItemType == MpCopyItemType.RichText) {
                Clipboard.SetData(DataFormats.Text, (string)copyItem.GetData());
            } else if (copyItem.CopyItemType == MpCopyItemType.HTMLText) {
                Clipboard.SetData(DataFormats.Text, (string)copyItem.GetData());
            } else if (copyItem.CopyItemType == MpCopyItemType.Image) {
                Clipboard.SetImage((Image/*BitmapSource*/)copyItem.GetData());
            } else if (copyItem.CopyItemType == MpCopyItemType.FileList) {
                Clipboard.SetFileDropList((StringCollection)copyItem.GetData());
            }
            //WinApi.SetActiveWindow(ClipboardController.GetLastWindowWatcher().LastHandle);
            SendKeys.Send("^v");

            IgnoreNextClipboardEvent = false;

            //only create to write to db

            MpPasteHistory pasteHistory = new MpPasteHistory(copyItem, GetLastWindowWatcher().LastHandle);

            MpSingletonController.Instance.AppendItem = null;
        }
        public MpLastWindowWatcher GetLastWindowWatcher() {
            return _lastWindowWatcher;
        }
        private void MpClipboardController_Load(object sender,EventArgs e) {
            _lastWindowWatcher = new MpLastWindowWatcher(this.Handle);
            IgnoreNextClipboardEvent = true;
            //AddClipboardFormatListener(this.Handle);    // Add our window to the clipboard's format listener list.
            this.SetBounds(0,0,0,0);
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
                            string sourcePath = MpHelperSingleton.Instance.GetProcessPath(_lastWindowWatcher.LastHandle);
                            Color itemColor = MpHelperSingleton.Instance.GetRandomColor();

                            if(iData.GetDataPresent(DataFormats.Bitmap)) {
                                ci = MpCopyItem.CreateCopyItem(MpCopyItemType.Image,(Image)iData.GetData(DataFormats.Bitmap,true),sourcePath,itemColor);// CreateCopyItem(MpCopyItemType.None,null,IntPtr.Zero);// ((Image)iData.GetData(DataFormats.Bitmap,true),sourceHandle);
                            }
                            else if(iData.GetDataPresent(DataFormats.FileDrop)) {
                                ci = MpCopyItem.CreateCopyItem(MpCopyItemType.FileList,(string[])iData.GetData(DataFormats.FileDrop,true),sourcePath,itemColor);
                            }
                            else if(iData.GetDataPresent(DataFormats.Rtf)) {
                                ci = MpCopyItem.CreateCopyItem(MpCopyItemType.Text,(string)iData.GetData(DataFormats.Text),sourcePath,itemColor);
                            }
                            else if(iData.GetDataPresent(DataFormats.Html)) {
                                ci = MpCopyItem.CreateCopyItem(MpCopyItemType.Text,(string)iData.GetData(DataFormats.Text),sourcePath,itemColor);
                            }
                            else if(iData.GetDataPresent(DataFormats.Text)) {
                                ci = MpCopyItem.CreateCopyItem(MpCopyItemType.Text,(string)iData.GetData(DataFormats.Text),sourcePath,itemColor);
                            }
                            else {
                                Console.WriteLine("MpData error clipboard data is not known format");
                                return;
                            }
                            if(ci != null && ci.CopyItemType == MpCopyItemType.Text && (ci.GetData() == null || (string)ci.GetData() == string.Empty)) {
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
