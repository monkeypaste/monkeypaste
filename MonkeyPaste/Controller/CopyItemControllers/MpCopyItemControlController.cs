using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItemControlController {
        private Control _itemControl { get; set; }
        public Control ItemControl { get { return _itemControl; } set { _itemControl = value; } }

        private MpCopyItem _copyItem { get; set; }
        public MpCopyItem CopyItem { get { return _copyItem; } set { _copyItem = value; } }

        public MpCopyItemControlController(int tileSize,MpCopyItem ci) {
            CopyItem = ci;
            switch(CopyItem.copyItemTypeId) {
                case MpCopyItemType.RichText:
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
                    CopyItem.DataRichText = ((RichTextBox)ItemControl).Rtf;
                    CopyItem.DataText = ((RichTextBox)ItemControl).Text;
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
                        MpFileListItemRowPanelController newFlirp = new MpFileListItemRowPanelController(ItemControl.Controls.Count,fileOrPathStr);
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
            UpdateTileSize(tileSize);
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
    }
}
