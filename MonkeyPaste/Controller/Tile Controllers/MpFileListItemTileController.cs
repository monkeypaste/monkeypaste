using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpFileListItemTileController : MpCopyItemTileController {
        private List<MpFileListItemRowPanelController> _fileListItemRowPanelList = new List<MpFileListItemRowPanelController>();

        public MpFileListItemTileController(int h,MpCopyItem ci) : base(h,ci) {
            _itemControl = new ScrollableControl() {
                AutoScroll = true
            };
            int maxWidth = int.MinValue;
            foreach(string fileOrPathStr in (string[])ci.DataObject) {
                MpFileListItemRowPanelController newFlirp = new MpFileListItemRowPanelController(_fileListItemRowPanelList.Count,fileOrPathStr,h);
                Point p = newFlirp.GetFileListItemRowPanel().Location;
                p.Y = newFlirp.GetFileListItemRowPanel().Bottom * _fileListItemRowPanelList.Count;
                newFlirp.GetFileListItemRowPanel().Location = p;

                _fileListItemRowPanelList.Add(newFlirp);

                _itemControl.Controls.Add(newFlirp.GetFileListItemRowPanel());

                if(newFlirp.GetFileListItemRowPanel().Width > maxWidth) {
                    maxWidth = newFlirp.GetFileListItemRowPanel().Width;
                }
            }
            UpdateTileSize(maxWidth);
            _contentPanel.Controls.Add(_itemControl);

            _itemControl.DoubleClick += itemControl_DoubleClick;
        }
        public override void UpdateTileSize(int tileSize) {
            int tp = (int)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePad");
            int ts = tileSize - (tp * 2);
            int tth = (int)((float)ts * (float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleRatio"));
            int x = ((TotalTileCount - _tileId) * ts) + ((TotalTileCount - _tileId + 1) * tp);
            int y = tp;
            _copyItemTilePanel.SetBounds(x,y,ts,ts);
            _titlePanel.SetBounds(x,y,ts,tth);
            _titleTextBox.SetBounds(x,y,ts,tth);
            _contentPanel.SetBounds(tp,tp + tth + tp,ts - (tp * 2),ts - tp * 2 - tth);
            _itemControl.SetBounds(0,0,ts - (tp * 2),ts - tp * 2 - tth);
            _itemControl.BringToFront();

            Font f = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileFontSize"));

            foreach(MpFileListItemRowPanelController flirpc in _fileListItemRowPanelList) {
                Rectangle b = flirpc.GetFileListItemRowPanel().Bounds;
                b = new Rectangle(b.X,b.Y,tileSize,b.Height);
                flirpc.GetFileListItemRowPanel().Bounds = b;

                flirpc.GetPathRichTextBox().Font = f;
                b = flirpc.GetPathRichTextBox().Bounds;
                b = new Rectangle(b.X,b.Y,tileSize - flirpc.GetIconPictureBox().Width,b.Height);
                flirpc.GetPathRichTextBox().Bounds = b;
                flirpc.GetPathRichTextBox().BringToFront();

                flirpc.GetFileListItemRowPanel().Scale(new SizeF(1.0f,1.0f));
                flirpc.GetPathRichTextBox().Scale(new SizeF(1.0f,1.0f));
                flirpc.GetIconPictureBox().Scale(new SizeF(1.0f,1.0f));
            }
        }
        private void itemControl_DoubleClick(object sender,EventArgs e) {
            ((RichTextBox)_itemControl).ReadOnly = false; 
        }
    }
}
