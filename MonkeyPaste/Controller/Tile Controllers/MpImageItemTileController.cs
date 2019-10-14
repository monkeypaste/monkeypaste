using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpImageItemTileController : MpCopyItemTileController {
        public MpImageItemTileController(int h,MpCopyItem ci) : base(h,ci) {
            _itemControl = new PictureBox() {
                Image = MpHelperFunctions.Instance.ConvertByteArrayToImage((byte[])ci.DataObject),
                Bounds = _contentPanel.Bounds,
                BorderStyle = BorderStyle.None,
                Location = new Point(),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            _contentPanel.Controls.Add(_itemControl);
        }
        public override void UpdateTileSize(int tileSize) {
            int tp = (int)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePad");
            int ts = tileSize - (tp * 2);
            int tth = (int)((float)ts * (float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleRatio"));
            int x = ((TotalTileCount - _tileId) * ts) + ((TotalTileCount - _tileId + 1) * tp);
            int y = tp;
            _copyItemTilePanel.SetBounds(x,y,ts,ts);
            //_titlePanel.SetBounds(x-tp,y-tp,tileSize,tth);
            _titleTextBox.SetBounds(tp,tp,ts - tp * 2,tileSize);
            _contentPanel.SetBounds(tp,tth,ts - (tp * 2),ts - tp * 2 - tth);
            _itemControl.SetBounds(0,0,ts - (tp * 2),ts - tp * 2 - tth);
            _itemControl.BringToFront();
        }
    }
}
