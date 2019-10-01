using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItemTileTitleIconPanelController {
        private MpCopyItemTileTitleIconPanel _copyItemTileTitleIconPanel { get; set; }
        public MpCopyItemTileTitleIconPanel CopyItemTileTitleIconPanel { get { return _copyItemTileTitleIconPanel; } set { _copyItemTileTitleIconPanel = value; } }

        private PictureBox _copyItemTitleIconBox { get; set; }
        public PictureBox CopyItemTitleIconBox { get { return _copyItemTitleIconBox; } set { _copyItemTitleIconBox = value; } }
        
        public MpCopyItemTileTitleIconPanelController(int titleHeight,MpCopyItem ci) {
            CopyItemTileTitleIconPanel = new MpCopyItemTileTitleIconPanel() {
                //FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                BackColor = (Color)MpSingletonController.Instance.GetSetting("LogPanelTileTitleIconBorderColor"),
                //Anchor = AnchorStyles.Top,
                //Location = new Point(),
                //AutoSize = true,
                //Size = new Size(tileSize,(int)((float)tileSize* (float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleRatio")))
            };
            CopyItemTileTitleIconPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            CopyItemTitleIconBox = new PictureBox() {
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.StretchImage,
                //BorderStyle = BorderStyle.FixedSingle,
                //Margin = new Padding(10),
                //Anchor = AnchorStyles.Left,
                Image = MpSingletonController.Instance.GetMpData().GetMpIcon(MpSingletonController.Instance.GetMpData().GetMpApp(ci.appId).iconId).IconImage
            };
            CopyItemTitleIconBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            CopyItemTileTitleIconPanel.Controls.Add(CopyItemTitleIconBox);
            CopyItemTitleIconBox.BringToFront();

            UpdatePanelSize(titleHeight);
        }

        public void UpdatePanelSize(int titleHeight) {            
            int ip = (int)((float)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePadRatio") * 0.5f);

            CopyItemTileTitleIconPanel.Location = new Point(ip,ip);
            CopyItemTileTitleIconPanel.Size = new Size(titleHeight - ip*2,titleHeight - ip*2);

            CopyItemTitleIconBox.Location = new Point(ip,ip);
            CopyItemTitleIconBox.Size = new Size(titleHeight - (ip * 4),titleHeight - (ip * 4));
        }
    }
}
