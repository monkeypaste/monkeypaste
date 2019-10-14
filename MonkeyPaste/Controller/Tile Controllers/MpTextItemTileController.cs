using System;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTextItemControlController : MpCopyItemControlController {
        public MpTextItemControlController(MpCopyItem ci) : base(ci) {
            if(ci.copyItemTypeId == MpCopyItemType.RichText) {
                ItemControl = new RichTextBox() {
                    Font = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize")),
                    Rtf = (string)ci.DataObject,
                    //Dock = DockStyle.Fill,
                    //Bounds = _contentPanel.Bounds,
                    Location = new Point(),
                    ReadOnly = true,
                    SelectionProtected = true,
                    WordWrap = false,
                    Cursor = Cursors.Arrow,
                    BorderStyle = BorderStyle.None
                };
                CopyItem.DataRichText = ((RichTextBox)ItemControl).Rtf;
                CopyItem.DataText = ((RichTextBox)ItemControl).Text;
            } else if(ci.copyItemTypeId == MpCopyItemType.Text) {
                ItemControl = new RichTextBox() {
                    Font = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize")),                    
                    Text = (string)ci.DataObject,
                    WordWrap = false,
                    //Bounds = _contentPanel.Bounds,
                    //Dock = DockStyle.Fill,
                    Location = new Point(),
                    ReadOnly = true,
                    SelectionProtected = true,
                    Cursor = Cursors.Arrow,
                    BorderStyle = BorderStyle.None
                };
                CopyItem.DataRichText = ((RichTextBox)ItemControl).Rtf;
                CopyItem.DataText = ((RichTextBox)ItemControl).Text;
            }  else if(ci.copyItemTypeId == MpCopyItemType.HTMLText) {
                string dataStr = (string)ci.DataObject;
                int idx0 = dataStr.IndexOf("<html>") < 0 ? 0 : dataStr.IndexOf("<html>");
                int idx1 = dataStr.IndexOf("/<html>") < 0 ? dataStr.Length - 1 : dataStr.IndexOf("/<html>");
                dataStr = dataStr.Substring(idx0,idx1 - idx0);
                dataStr.Insert(dataStr.IndexOf("<html>") + 4," style='border:none;'>");
                ItemControl = new WebBrowser() {
                    Location = new Point(),
                    //Dock = DockStyle.Fill,
                    //Bounds = _contentPanel.Bounds,
                    DocumentText = dataStr
                };
                ((WebBrowser)ItemControl).DocumentCompleted += MpTextItemTileController_DocumentCompleted;
                //_copyItemTile.AutoScroll = false;
                //_contentPanel.AutoScroll = false;
            }
            //_contentPanel.Controls.Add(_itemControl);
            ItemControl.BringToFront();

            /*_copyItemTile.Click += itemControl_DoubleClick;
            _contentPanel.Click += itemControl_DoubleClick;
            _itemControl.Click += itemControl_DoubleClick;*/

            //_copyItemTile.LostFocus += ItemControl_LostFocus;
            //_contentPanel.LostFocus += ItemControl_LostFocus;
            ItemControl.LostFocus += ItemControl_LostFocus;
            //UpdateTileSize(h);
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

        /*public override void UpdateTileSize(int tileSize) {
            int tp = (int)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePad");
            int ts = tileSize - (tp * 2);
            int tth = (int)((float)ts * (float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleRatio"));
            int x = ((TotalTileCount - _tileId) * ts) + ((TotalTileCount - _tileId + 1) * tp);
            int y = tp;
            _copyItemTile.SetBounds(x,y,ts,ts);
            //_titlePanel.SetBounds(x-tp,y-tp,tileSize,tth);
            _titleTextBox.SetBounds(tp,tp,ts-tp*2,tileSize);
            _contentPanel.SetBounds(tp,tth,ts - (tp * 2),ts - tp * 2 - tth);
            _itemControl.SetBounds(0,0,ts - (tp * 2),ts - tp * 2 - tth);
            _itemControl.BringToFront();

            _titleTextBox.Font = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileTitleFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleFontSize"));

            Font f = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize"));
            //_titleTextBox.Scale(new SizeF(1.0f,1.0f));
            if(_itemControl.GetType() == typeof(RichTextBox)) {
                ((RichTextBox)_itemControl).Font = f;
                ((RichTextBox)_itemControl).Scale(new SizeF(1.0f,1.0f));
            }
            else if(_itemControl.GetType() == typeof(TextBox)) {
                ((TextBox)_itemControl).Font = f;
                ((TextBox)_itemControl).Scale(new SizeF(1.0f,1.0f));
            }
            else if(_itemControl.GetType() == typeof(WebBrowser)) {
                ((WebBrowser)_itemControl).Font = f;
                ((WebBrowser)_itemControl).Scale(new SizeF(1.0f,1.0f));
            }
        }*/
        private void ItemControl_LostFocus(object sender,EventArgs e) {
            ((RichTextBox)ItemControl).Cursor = Cursors.Arrow;
            ((RichTextBox)ItemControl).ReadOnly = true;
        }

        private void itemControl_DoubleClick(object sender,EventArgs e) {
            ((RichTextBox)ItemControl).Cursor = Cursors.IBeam;
            ((RichTextBox)ItemControl).ReadOnly = false;
        }
    }
}
