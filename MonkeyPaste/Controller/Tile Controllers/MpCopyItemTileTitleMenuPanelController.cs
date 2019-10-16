using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItemTileTitleMenuPanelController : MpController {
        private MpCopyItemTileTitleMenuPanel _cittmp { get; set; }
        public MpCopyItemTileTitleMenuPanel Cittmp { get { return _cittmp; } set { _cittmp = value; } }

        private Button b1, b2, b3;

        public MpCopyItemTileTitleMenuPanelController(MpController parentController) : base(parentController) {
            Cittmp = new MpCopyItemTileTitleMenuPanel();
            b1 = new Button() {
                Text = "EDIT"
            };
            b2 = new Button() {
                Text = "APPEND"
            };
            b3 = new Button() {
                Text = "DELETE"
            };
            Cittmp.Controls.Add(b1);
            Cittmp.Controls.Add(b2);
            Cittmp.Controls.Add(b3);

            UpdateBounds();

            Cittmp.Visible = false;
        }

        public override void UpdateBounds() {
            //get title rect
            Rectangle tr = ((MpCopyItemTileTitlePanelController)ParentController).CopyItemTileTitlePanel.Bounds;
            //get icon rect
            Rectangle ir = ((MpCopyItemTileTitlePanelController)ParentController).CopyItemTileTitleIconPanelController.CopyItemTileTitleIconPanel.Bounds;
            Cittmp.SetBounds(ir.Right,0,tr.Width - ir.Width,tr.Height);

            b1.Font = new Font((string)MpSingletonController.Instance.GetSetting("LogFont"),(float)Cittmp.Height*(float)MpSingletonController.Instance.GetSetting("TileMenuFontRatio"),GraphicsUnit.Pixel);
        }
    }
}