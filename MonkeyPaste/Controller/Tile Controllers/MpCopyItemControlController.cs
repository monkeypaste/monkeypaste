using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItemControlController : MpController {
        private PictureBox _pb { get; set; }
        public PictureBox Pb { get { return _pb; } set { _pb = value; } }

        private TextBox _tb{ get; set; }
        public TextBox Tb { get { return _tb; } set { _tb = value; } }

       // private MpRichTextEditorControlController _rtEditorControlController { get; set; }
       // public MpRichTextEditorControlController RtEditorControlController { get { return _rtEditorControlController; } set { _rtEditorControlController = value; } }

        private MpImageEditorControlController _imageEditorControlController { get; set; }
        public MpImageEditorControlController ImageEditorControlController { get { return _imageEditorControlController; } set { _imageEditorControlController = value; } }

        private Control _itemControl { get; set; }
        public Control ItemControl { get { return _itemControl; } set { _itemControl = value; } }

        /*private MpRoundedPanel _overlayPanel { get; set; }
        public MpRoundedPanel OverlayPanel { get { return _overlayPanel; } set { _overlayPanel = value; } }*/

        private MpCopyItem _copyItem { get; set; }
        public MpCopyItem CopyItem { get { return _copyItem; } set { _copyItem = value; } }

       // private bool _hasFocus = false;
        /*public bool HasFocus {
            get {
                return _hasFocus;
            }
            set {
                if(OverlayPanel == null) {
                    OverlayPanel = new MpRoundedPanel() {
                        AutoSize = false,
                        Bounds = ItemControl.Bounds,
                        BackColor = Color.FromArgb(200,128,128,0)
                    };
                    ItemControl.Controls.Add(OverlayPanel);
                    OverlayPanel.Click += OverlayPanel_Click;
                }
                _hasFocus = value;
                OverlayPanel.Bounds = ItemControl.Bounds;
                if(_hasFocus) {
                    OverlayPanel.SendToBack();
                }
                else {
                    OverlayPanel.BringToFront();
                }
                OverlayPanel.BringToFront();
            }
        }*/

        private MpKeyboardHook _escKeyHook,_spaceKeyHook;

        public MpCopyItemControlController(int tileSize,MpCopyItem ci,MpController parentController) : base(parentController) {
            CopyItem = ci;
            switch(CopyItem.copyItemTypeId) {
                //RtEditorControlController = new MpRichTextEditorControlController((string)ci.GetData(),this);

                /*ItemControl = new TextBox() {
                    Font = new Font((string)MpSingletonController.Instance.GetSetting("LogFont"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize")),
                    Rtf = ci.GetData(),
                    //Anchor = AnchorStyles.Bottom,
                    ReadOnly = true,
                    SelectionProtected = true,
                    WordWrap = false,
                    Cursor = Cursors.Arrow,
                    AutoSize = true,
                    BorderStyle = BorderStyle.None
                };

                ItemControl = Rtb;
                break;8
            case MpCopyItemType.HTMLText:
                string dataStr = (string)ci.GetData();
                int idx0 = dataStr.IndexOf("<html>") < 0 ? 0 : dataStr.IndexOf("<html>");
                int idx1 = dataStr.IndexOf("/<html>") < 0 ? dataStr.Length - 1 : dataStr.IndexOf("/<html>");
                dataStr = dataStr.Substring(idx0,idx1 - idx0);
                dataStr.Insert(dataStr.IndexOf("<html>") + 4," style='border:none;'>");
                ItemControl = new WebBrowser() {
                    //Anchor = AnchorStyles.Bottom,
                    //Dock = DockStyle.Fill,
                    //Bounds = _contentPanel.Bounds,
                    AutoSize = true,
                    DocumentText = dataStr
                };
                //((WebBrowser)ItemControl).DocumentCompleted += MpTextItemTileController_DocumentCompleted;
                //_copyItemTile.AutoScroll = false;
                //_contentPanel.AutoScroll = false;
                break;*/
                case MpCopyItemType.HTMLText:
                case MpCopyItemType.RichText:
                case MpCopyItemType.Text:
                    //RtEditorControlController = new MpRichTextEditorControlController((string)ci.GetData(),this);
                    //ItemControl = RtEditorControlController.RichTextEditorPanel;

                    ItemControl = new TextBox() {
                        Font = new Font((string)MpSingletonController.Instance.GetSetting("LogFont"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize")),
                        Text = (string)ci.GetData(),
                        WordWrap = true,
                        Multiline = true,
                        AutoSize = true,
                        
                        ReadOnly = true,
                        Cursor = Cursors.Arrow,                        
                        BorderStyle = BorderStyle.None
                    };
                    ItemControl.BringToFront();
                    //CopyItem.DataRichText = Rtb.Rtf;
                    //ItemControl = Tb;
                    break;
                case MpCopyItemType.FileList:
                    ItemControl = new ScrollableControl() {
                        AutoScroll = true,
                        //Anchor = AnchorStyles.Bottom,
                        AutoSize = true,
                        //FlowDirection = FlowDirection.TopDown
                    };
                    if(CopyItem.GetData().GetType() == typeof(string[])) {
                        int maxWidth = int.MinValue;
                        foreach(string fileOrPathStr in (string[])CopyItem.GetData()) {
                            MpFileListItemRowPanelController newFlirp = new MpFileListItemRowPanelController(ItemControl.Controls.Count,fileOrPathStr,this);
                            Point p = newFlirp.GetFileListItemRowPanel().Location;
                            p.Y = newFlirp.GetFileListItemRowPanel().Bottom * ItemControl.Controls.Count;
                            newFlirp.GetFileListItemRowPanel().Location = p;
                            ItemControl.Controls.Add(newFlirp.GetFileListItemRowPanel());

                            if(newFlirp.GetFileListItemRowPanel().Width > maxWidth) {
                                maxWidth = newFlirp.GetFileListItemRowPanel().Width;
                            }
                        }
                    }                    
                    break;
                case MpCopyItemType.Image:
                    ImageEditorControlController = new MpImageEditorControlController(MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])CopyItem.GetData()),this);
                    //ItemControl = ImageEditorControlController.ImageEditorControl;

                    Pb = new PictureBox() {
                        Image = MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])CopyItem.GetData()),
                        BorderStyle = BorderStyle.None,
                        AutoSize = true,
                        //Anchor = AnchorStyles.Bottom,
                        SizeMode = PictureBoxSizeMode.StretchImage
                    };
                    ItemControl = Pb;
                    break;
            }
            ItemControl.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            ItemControl.MouseClick += ItemControl_MouseClick;
            //ItemControl.MouseDoubleClick += ItemControl_MouseDoubleClick;

            _escKeyHook = new MpKeyboardHook();
            _escKeyHook.KeyPressed += _escKeyHook_KeyPressed;
            _spaceKeyHook = new MpKeyboardHook();
            _spaceKeyHook.KeyPressed += _spaceKeyHook_KeyPressed;

            UpdateBounds();
        }

        public override void UpdateBounds() {
            //get tile rect
            Rectangle tr = ((MpCopyItemTileController)ParentController).CopyItemTilePanel.Bounds;
            //itemcontrol padding
            int icp = (int)((float)MpSingletonController.Instance.GetSetting("TileOuterPadScreenWidthRatio") * (float)tr.Width);
            //get tile rect
            Rectangle tir = ((MpCopyItemTileController)ParentController).CopyItemTileTitlePanelController.CopyItemTileTitlePanel.Bounds;
            ItemControl.SetBounds(icp,icp+tir.Height,tr.Width - (icp*2),tr.Height-tir.Height-(icp*2));

        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
        private void ItemControl_MouseDoubleClick(object sender,MouseEventArgs e) {
            //perform paste
            ((MpLogFormController)((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)ParentController).ParentController).ParentController).PasteCopyItem();
            // return tile to readonly state
           if(((MpCopyItemTileController)ParentController).IsEditable) {
                ((TextBox)ItemControl).ReadOnly = true;
            }
        }
        /*private void OverlayPanel_Click(object sender,EventArgs e) {
            HasFocus = !HasFocus;
            switch(CopyItem.copyItemTypeId) {
                case MpCopyItemType.RichText:
                    if(HasFocus) {
                        RtEditorControlController.RtfBoxWpfUserControl.SetRTF(CopyItem.DataRichText);
                        ItemControl = RtEditorControlController.RichTextEditorPanel;
                    } else {
                        Rtb.Rtf = RtEditorControlController.RtfBoxWpfUserControl.GetRTF();
                        ItemControl = Rtb;
                    }

                    break;
                case MpCopyItemType.HTMLText:
                    break;
                case MpCopyItemType.Text:
                    if(HasFocus) {
                        RtEditorControlController.RtfBoxWpfUserControl.SetRTF(CopyItem.GetText());
                        ItemControl = RtEditorControlController.RichTextEditorPanel;
                    }
                    else {
                        Rtb.Rtf = RtEditorControlController.RtfBoxWpfUserControl.GetRTF();
                        ItemControl = Rtb;
                    }
                    break;
                case MpCopyItemType.FileList:
                    ItemControl = new ScrollableControl() {
                        AutoScroll = true,
                        //Anchor = AnchorStyles.Bottom,
                        AutoSize = true,
                        //FlowDirection = FlowDirection.TopDown
                    };
                    int maxWidth = int.MinValue;
                    foreach(string fileOrPathStr in (string[])CopyItem.DataObject) {
                        MpFileListItemRowPanelController newFlirp = new MpFileListItemRowPanelController(ItemControl.Controls.Count,fileOrPathStr,this);
                        Point p = newFlirp.GetFileListItemRowPanel().Location;
                        p.Y = newFlirp.GetFileListItemRowPanel().Bottom * ItemControl.Controls.Count;
                        newFlirp.GetFileListItemRowPanel().Location = p;
                        ItemControl.Controls.Add(newFlirp.GetFileListItemRowPanel());

                        if(newFlirp.GetFileListItemRowPanel().Width > maxWidth) {
                            maxWidth = newFlirp.GetFileListItemRowPanel().Width;
                        }
                    }
                    break;
                case MpCopyItemType.Image:
                    if(HasFocus) {
                        ImageEditorControlController = new MpImageEditorControlController(MpHelperFunctions.Instance.ConvertByteArrayToImage((byte[])CopyItem.DataObject),this);
                        ItemControl = RtEditorControlController.RichTextEditorPanel;
                    }
                    else {
                        Pb.Image = ImageEditorControlController.ImageEditorControl.Image;
                        ItemControl = Rtb;
                    }
                    break;
            }
            if(HasFocus) {
                OverlayPanel.SendToBack();
            }
        }**/
        private void ItemControl_MouseClick(object sender,MouseEventArgs e) {
            //if tile is already selected and single clicked, make it editable
            if(((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)ParentController).ParentController).SelectedCopyItemTileController == (MpCopyItemTileController)ParentController) {
                if(((MpCopyItemTileController)ParentController).IsEditable) {
                    ((TextBox)ItemControl).ReadOnly = false;
                }
            } else {
                //otherwise make this the selected tile
                ((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)ParentController).ParentController).SelectedCopyItemTileController = (MpCopyItemTileController)ParentController;
            }         
        }

        /*private void MpTextItemTileController_DocumentCompleted(object sender,WebBrowserDocumentCompletedEventArgs e) {
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
            ((WebBrowser)ItemControl).Document.ExecCommand("SelectAll",false,null);
            ((WebBrowser)ItemControl).Document.ExecCommand("Copy",false,null);
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);
            TextBox temp = new TextBox();
            temp.Paste();
            CopyItem.DataText = temp.Text;
            //CopyItem.DataText = temp.Text;
            //CopyItem.DataRichText = temp.Rtf;
            ((WebBrowser)ItemControl).Document.ExecCommand("UNSELECT",false,Type.Missing);
        }*/
        private void _spaceKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(((MpCopyItemTileController)ParentController).IsEditable) {
                ((TextBox)ItemControl).ReadOnly = false;
            }
        }

        private void _escKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(((MpCopyItemTileController)ParentController).IsEditable) {
                ((TextBox)ItemControl).ReadOnly = true;
            }
        }
        public void ActivateEscapeHook() {
            _escKeyHook.RegisterHotKey(ModifierKeys.None,Keys.Escape);
        }
        public void ActivateSpaceHook() {
            _spaceKeyHook.RegisterHotKey(ModifierKeys.None,Keys.Space);
        }
        public void DeactivateEscapeHook() {
            _escKeyHook.UnregisterHotKey();
        }
        public void DeactivateSpaceHook() {
            _spaceKeyHook.UnregisterHotKey();
        }
    }
}
