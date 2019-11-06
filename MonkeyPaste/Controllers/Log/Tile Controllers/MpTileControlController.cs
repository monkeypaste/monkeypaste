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
        public Font ItemFont { get; set; }
        public Panel ItemControlPanel { get; set; }
        public Panel ItemPanel { get; set; }
        private Point _lastMouseLoc = Point.Empty;

        public MpTileControlController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            switch(ci.copyItemTypeId) {
                case MpCopyItemType.HTMLText:
                case MpCopyItemType.RichText:
                case MpCopyItemType.Text:
                    ItemControl = new MpTileControlRichTextBox(tileId,panelId) { 
                        WordWrap = false,
                        Enabled = false,
                        Multiline = true,
                        Text = (string)ci.GetData(),
                        AutoSize = false,
                        ReadOnly = true,
                        Cursor = Cursors.Arrow,                        
                        BorderStyle = BorderStyle.None
                    };
                    MpHelperSingleton.Instance.SetPadding((TextBoxBase)ItemControl,new Padding(Properties.Settings.Default.TileItemPadding));
                    //ItemControl.BringToFront();                    
                    break;
                case MpCopyItemType.FileList:
                    ItemControl = new MpTileControlScrollableControl(tileId,panelId) {
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
                    ItemControl = new MpTileControlPictureBox(tileId,panelId) {
                        Image = MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])ci.GetData()),
                        BorderStyle = BorderStyle.None,                       
                        AutoSize = false,                  
                        SizeMode = PictureBoxSizeMode.StretchImage
                    };
                    break;
            }            
            ItemControlPanel= new Panel() {
                BorderStyle = BorderStyle.None,
                BackColor = ItemControl.BackColor,
                AutoSize = false
            };
            ItemControlPanel.Controls.Add(ItemControl);

            ItemPanel = new Panel() {
                BorderStyle = BorderStyle.None,
                BackColor = ItemControl.BackColor,
                AutoSize = false
            };
            ItemPanel.Controls.Add(ItemControlPanel);

            //ItemControl.BringToFront();
            ItemControl.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            Link(new List<MpIView> { (MpIView)ItemControl});
        }


        public override void Update() {
            //get tile rect
            Rectangle tr = ((MpTilePanelController)Parent).TilePanel.Bounds;
            //itemcontrol padding
            int tp = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width);
            //get tile details rect
            Rectangle tdr = ((MpTilePanelController)Parent).TileDetailsPanelController.TileDetailsPanel.Bounds;
            //get tile title rect
            Rectangle ttr = ((MpTilePanelController)Parent).TileTitlePanelController.TileTitlePanel.Bounds;

            ItemPanel.SetBounds(tp,tp+ttr.Height,tr.Width - (tp*2),tr.Height-tdr.Height-ttr.Height-(tp*4));

            if(ItemControl.GetType().IsSubclassOf(typeof(TextBoxBase))) {
                float fontSize = ItemPanel.Height * Properties.Settings.Default.TileFontSizeRatio;
                ItemFont = new Font(Properties.Settings.Default.TileFont,fontSize,GraphicsUnit.Pixel);
                ((TextBoxBase)ItemControl).Font = ItemFont;
                ItemControl.Location = Point.Empty;
                Size textSize = MpHelperSingleton.Instance.GetTextSize(((TextBoxBase)ItemControl).Text,ItemFont);
                if(textSize.Width < ItemPanel.Width) {
                    textSize.Width = ItemPanel.Width;
                }
                if(textSize.Height < ItemPanel.Height) {
                    textSize.Height = ItemPanel.Height;
                }
                ItemControl.Size = textSize;
                ItemControlPanel.Size = textSize;
               // ((TextBoxBase)ItemControl).Refresh();
            } else {
                ItemFont = null;
                ItemControl.Bounds = ItemPanel.Bounds;
                ItemControlPanel.Bounds = ItemPanel.Bounds;
            }
            ItemPanel.SendToBack();
            //ItemControl.Refresh();
        }
        public void TraverseItem(Point ml) {
            return;
            if(ItemControl.GetType().IsSubclassOf(typeof(TextBoxBase))) {
                //item control size
                Size ics = MpHelperSingleton.Instance.GetTextSize(((TextBoxBase)ItemControl).Text,ItemFont);
                //item panel size
                Size ips = ItemPanel.Size;
                Point p = ItemControlPanel.Location;
                if(ics.Width > ips.Width) {
                    p.X = ItemControlPanel.Location.X - (int)((((float)ml.X / (float)ips.Width) * (float)(ics.Width-ips.Width)));
                }
                if(ics.Height > ips.Height) {
                    p.Y = ItemControlPanel.Location.Y - (int)((((float)ml.Y / (float)ips.Height) * (float)(ics.Height-ips.Height)));
                }
                
                ItemControlPanel.Location = p;
                ItemControlPanel.Refresh();
                Console.WriteLine("Traversing item: "+ControllerName+" P:"+p.ToString()+" Item Dimensions:"+ics.ToString());
            }
            //_lastMouseLoc = ml;
        }
        /*
         * private void ItemControl_MouseDoubleClick(object sender,MouseEventArgs e) {
            //perform paste
            ((MpLogFormController)((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)Parent).Parent).Parent).PasteCopyItem();
            // return tile to readonly state
           if(((MpCopyItemTileController)Parent).IsEditable) {
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
            if(((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)Parent).Parent).SelectedCopyItemTileController == (MpCopyItemTileController)Parent) {
                if(((MpCopyItemTileController)Parent).IsEditable) {
                    ((TextBox)ItemControl).ReadOnly = false;
                }
            } else {
                //otherwise make this the selected tile
                ((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)Parent).Parent).SelectedCopyItemTileController = (MpCopyItemTileController)Parent;
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
            if(((MpCopyItemTileController)Parent).IsEditable) {
                ((TextBox)ItemControl).ReadOnly = false;
            }
        }

        private void _escKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(((MpCopyItemTileController)Parent).IsEditable) {
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
