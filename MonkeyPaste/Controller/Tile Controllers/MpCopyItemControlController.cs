using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItemControlController : MpController {
        private MpRichTextEditorControlController _rtEditorControlController { get; set; }
        public MpRichTextEditorControlController RtEditorControlController { get { return _rtEditorControlController; } set { _rtEditorControlController = value; } }

        private MpImageEditorControlController _imageEditorControlController { get; set; }
        public MpImageEditorControlController ImageEditorControlController { get { return _imageEditorControlController; } set { _imageEditorControlController = value; } }

        private Control _itemControl { get; set; }
        public Control ItemControl { get { return _itemControl; } set { _itemControl = value; } }

        private MpRoundedPanel _overlayPanel { get; set; }
        public MpRoundedPanel OverlayPanel { get { return _overlayPanel; } set { _overlayPanel = value; } }

        private MpCopyItem _copyItem { get; set; }
        public MpCopyItem CopyItem { get { return _copyItem; } set { _copyItem = value; } }

        private bool _hasFocus { get; set; }
        public bool HasFocus {
            get {
                if(OverlayPanel == null) {
                    OverlayPanel = new MpRoundedPanel();
                }
                return _hasFocus;
            }
            set {
                _hasFocus = value;
            }
        }

        private MpKeyboardHook _escKeyHook,_spaceKeyHook;

        public MpCopyItemControlController(int tileSize,MpCopyItem ci,MpController parentController) : base(parentController) {
            HasFocus = false;
            CopyItem = ci;
            switch(CopyItem.copyItemTypeId) {
                case MpCopyItemType.RichText:
                    //RtEditorControlController = new MpRichTextEditorControlController(ci.GetText(),this);
                    //ItemControl = RtEditorControlController.RichTextEditorPanel;

                    ItemControl = new RichTextBox() {
                        Font = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize")),
                        Rtf = ci.GetText(),
                        //Anchor = AnchorStyles.Bottom,
                        ReadOnly = true,
                        SelectionProtected = true,
                        WordWrap = false,
                        Cursor = Cursors.Arrow,
                        AutoSize = true,
                        BorderStyle = BorderStyle.None
                    };
                    break;
                case MpCopyItemType.HTMLText:
                    string dataStr = (string)ci.DataObject;
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
                    ((WebBrowser)ItemControl).DocumentCompleted += MpTextItemTileController_DocumentCompleted;
                    //_copyItemTile.AutoScroll = false;
                    //_contentPanel.AutoScroll = false;
                    break;
                case MpCopyItemType.Text:
                    //RtEditorControlController = new MpRichTextEditorControlController(ci.GetText(),this);
                    //ItemControl = RtEditorControlController.RichTextEditorPanel;

                    ItemControl = new RichTextBox() {
                        Font = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize")),
                        Text = ci.GetText(),
                        WordWrap = false,
                        AutoSize = true,
                        //Bounds = _contentPanel.Bounds,
                        //Dock = DockStyle.Fill,
                        ReadOnly = true,
                        SelectionProtected = true,
                        //Anchor = AnchorStyles.Bottom,
                        Cursor = Cursors.Arrow,
                        BorderStyle = BorderStyle.None
                    };
                    RichTextBox temp = new RichTextBox() {
                        Text = ci.GetText()
                    };
                    CopyItem.DataRichText = temp.Rtf;
                    CopyItem.DataText = temp.Text;
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
                    ImageEditorControlController = new MpImageEditorControlController(MpHelperFunctions.Instance.ConvertByteArrayToImage((byte[])CopyItem.DataObject),this);
                    ItemControl = ImageEditorControlController.ImageEditorControl;
                    ItemControl = new PictureBox() {
                        Image = MpHelperFunctions.Instance.ConvertByteArrayToImage((byte[])CopyItem.DataObject),
                        BorderStyle = BorderStyle.None,
                        AutoSize = true,
                        //Anchor = AnchorStyles.Bottom,
                        SizeMode = PictureBoxSizeMode.StretchImage
                    };
                    break;
            }
            ItemControl.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            ItemControl.MouseClick += ItemControl_MouseClick;
            ItemControl.MouseDoubleClick += ItemControl_MouseDoubleClick;

            _escKeyHook = new MpKeyboardHook();
            _escKeyHook.KeyPressed += _escKeyHook_KeyPressed;
            _spaceKeyHook = new MpKeyboardHook();
            _spaceKeyHook.KeyPressed += _spaceKeyHook_KeyPressed;

            UpdateTileSize(tileSize);
        }

        private void ItemControl_MouseDoubleClick(object sender,MouseEventArgs e) {
            //perform paste
            ((MpLogFormController)((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)ParentController).ParentController).ParentController).PasteCopyItem();
            // return tile to readonly state
           if(((MpCopyItemTileController)ParentController).IsEditable) {
                ((RichTextBox)ItemControl).ReadOnly = true;
            }
        }

        private void ItemControl_MouseClick(object sender,MouseEventArgs e) {
            //if tile is already selected and single clicked, make it editable
            if(((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)ParentController).ParentController).SelectedCopyItemTileController == (MpCopyItemTileController)ParentController) {
                if(((MpCopyItemTileController)ParentController).IsEditable) {
                    ((RichTextBox)ItemControl).ReadOnly = false;
                }
            } else {
                //otherwise make this the selected tile
                ((MpCopyItemTileChooserPanelController)((MpCopyItemTileController)ParentController).ParentController).SelectedCopyItemTileController = (MpCopyItemTileController)ParentController;
            }         
        }

        public void UpdateTileSize(int tileSize) {
            int tp = (int)((float)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePadRatio")*(float)tileSize);
            int ts = tileSize;
            int tth = (int)((float)ts * (float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleRatio"));
            ItemControl.Size = new Size(ts-tp,ts - tp*2 - tth);
            ItemControl.Location = new Point(tp,tth+tp);
            ItemControl.BringToFront();
        }
        private void MpTextItemTileController_DocumentCompleted(object sender,WebBrowserDocumentCompletedEventArgs e) {
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
            ((WebBrowser)ItemControl).Document.ExecCommand("SelectAll",false,null);
            ((WebBrowser)ItemControl).Document.ExecCommand("Copy",false,null);
            MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);
            RichTextBox temp = new RichTextBox();
            temp.Paste();
            CopyItem.DataText = temp.Text;
            CopyItem.DataRichText = temp.Rtf;
            ((WebBrowser)ItemControl).Document.ExecCommand("UNSELECT",false,Type.Missing);
        }
        private void _spaceKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(((MpCopyItemTileController)ParentController).IsEditable) {
                ((RichTextBox)ItemControl).ReadOnly = false;
            }
        }

        private void _escKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            if(((MpCopyItemTileController)ParentController).IsEditable) {
                ((RichTextBox)ItemControl).ReadOnly = true;
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
