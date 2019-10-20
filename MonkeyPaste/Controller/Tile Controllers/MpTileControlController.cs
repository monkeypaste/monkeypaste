using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileControlController : MpController {
        public Control ItemControl { get; set; }

        public MpTileControlController(MpCopyItem ci,MpController parentController) : base(parentController) {
            switch(ci.copyItemTypeId) {
                case MpCopyItemType.HTMLText:
                case MpCopyItemType.RichText:
                case MpCopyItemType.Text:
                    ItemControl = new TextBox() { 
                        WordWrap = true,
                        Enabled = false,
                        ScrollBars = ScrollBars.Both,
                        Multiline = true,
                        Text = (string)ci.GetData(),
                        AutoSize = false,
                        ReadOnly = true,
                        Cursor = Cursors.Arrow,                        
                        BorderStyle = BorderStyle.None
                    };
                    ItemControl.BringToFront();                    
                    break;
                case MpCopyItemType.FileList:
                    ItemControl = new ScrollableControl() {
                        AutoScroll = true,
                        AutoSize = false,
                    };
                    if(ci.GetData().GetType() == typeof(string[])) {
                        int maxWidth = int.MinValue;
                        foreach(string fileOrPathStr in (string[])ci.GetData()) {
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
                    ItemControl = new PictureBox() {
                        Image = MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])ci.GetData()),
                        BorderStyle = BorderStyle.None,
                        AutoSize = false,                  
                        SizeMode = PictureBoxSizeMode.StretchImage
                    };
                    break;
            }
            ItemControl.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
                        
            UpdateBounds();
        }

        public override void UpdateBounds() {
            //get tile rect
            Rectangle tr = ((MpTilePanelController)ParentController).TilePanel.Bounds;
            //itemcontrol padding
            int icp = (int)((float)MpSingletonController.Instance.GetSetting("TilePadWidthRatio") * (float)tr.Width);
            //get tile title rect
            Rectangle ttr = ((MpTilePanelController)ParentController).TileTitlePanelController.TileTitlePanel.Bounds;
            ItemControl.SetBounds(icp,icp+ttr.Height,tr.Width - (icp*2),tr.Height-ttr.Height-(icp*2));

            if(ItemControl.GetType() == typeof(TextBox)) {
                int minLineCount = (int)MpSingletonController.Instance.GetSetting("TileMinLineCount");
                float fontSize = ItemControl.Height / minLineCount;
                ((TextBox)ItemControl).Font = new Font((string)MpSingletonController.Instance.GetSetting("TileFont"),fontSize,GraphicsUnit.Pixel);
            }
            ItemControl.Refresh();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
        /*private void ItemControl_MouseDoubleClick(object sender,MouseEventArgs e) {
            //perform paste
            ((MpLogFormController)((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)ParentController).ParentController).ParentController).PasteCopyItem();
            // return tile to readonly state
           if(((MpCopyItemTileController)ParentController).IsEditable) {
                ((TextBox)ItemControl).ReadOnly = true;
            }
        }
        private void OverlayPanel_Click(object sender,EventArgs e) {
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
        }
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

        private void MpTextItemTileController_DocumentCompleted(object sender,WebBrowserDocumentCompletedEventArgs e) {
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
        }
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
        }*/
    }
}
