using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItemTileTitlePanelController {
        private MpCopyItemTileTitlePanel _copyItemTileTitlePanel { get; set; }
        public MpCopyItemTileTitlePanel CopyItemTileTitlePanel { get { return _copyItemTileTitlePanel; } set { _copyItemTileTitlePanel = value; } }
        
        private MpCopyItemTileTitleIconPanelController _copyItemTileTitleIconPanelController { get; set; }
        public MpCopyItemTileTitleIconPanelController CopyItemTileTitleIconPanelController { get { return _copyItemTileTitleIconPanelController; } set { _copyItemTileTitleIconPanelController = value; } }

        private TextBox _copyItemTitleTextBox { get; set; }
        public TextBox CopyItemTitleTextBox { get { return _copyItemTitleTextBox; } set { _copyItemTitleTextBox = value; } }

        private string _orgTitle;
        private int _copyItemId;

        public MpCopyItemTileTitlePanelController(int tileSize,MpCopyItem ci,Color tileColor) {
            _orgTitle = ci.Title;
            _copyItemId = ci.copyItemId;

            CopyItemTileTitlePanel = new MpCopyItemTileTitlePanel() {
                //FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                BackColor = tileColor,
                BorderStyle = BorderStyle.None
                //Anchor = AnchorStyles.Top,
                //Location = new Point(),
                //AutoSize = true,
                //Size = new Size(tileSize,(int)((float)tileSize* (float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleRatio")))
            };
            CopyItemTileTitlePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener; 

            CopyItemTileTitleIconPanelController = new MpCopyItemTileTitleIconPanelController((int)((float)tileSize * (float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleRatio")),ci);
            CopyItemTileTitlePanel.Controls.Add(CopyItemTileTitleIconPanelController.CopyItemTileTitleIconPanel);

            //float conScale = 1.0f;
            Font titleFont = new Font((string)MpSingletonController.Instance.GetSetting("LogPanelTileTitleFontFace"),(float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleFontSize"));
            CopyItemTitleTextBox = new TextBox() {
                Text = ci.Title,
                //Anchor = AnchorStyles.Right,                
                ReadOnly = true,
                BackColor = tileColor,
                Font = titleFont,
                BorderStyle = BorderStyle.None
            };
            CopyItemTitleTextBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            CopyItemTitleTextBox.MouseEnter += _titleTextBox_MouseEnter;
            //CopyItemTitleTextBox.Enter += _titleTextBox_LostFocus;
            CopyItemTitleTextBox.MouseLeave += _titleTextBox_LostFocus;
            CopyItemTitleTextBox.Click += _titleTextBox_Click;
            CopyItemTitleTextBox.Leave += _titleTextBox_MouseLeave;

            CopyItemTileTitlePanel.Controls.Add(CopyItemTitleTextBox);
            
            CopyItemTitleTextBox.BringToFront();

            UpdateTileSize(tileSize);
        }
        public void UpdateTileSize(int tileSize) {
            int tp = (int)((float)MpSingletonController.Instance.GetSetting("LogPanelDefaultTilePadRatio") * (float)tileSize);
            int ts = tileSize;
            int tth = (int)((float)ts * (float)MpSingletonController.Instance.GetSetting("LogPanelTileTitleRatio"));

            CopyItemTileTitlePanel.Location = new Point(tp,tp);
            CopyItemTileTitlePanel.Size = new Size(ts-tp,tth);

            CopyItemTitleTextBox.Location = new Point(tth+tp,(int)((CopyItemTileTitlePanel.Height/2)-(CopyItemTitleTextBox.Height/2)));
            CopyItemTitleTextBox.Size = new Size(ts-tth-tp-tp-tp,tth);

            CopyItemTileTitleIconPanelController.UpdatePanelSize(tth-tp);
        }
        private void _titleTextBox_LostFocus(object sender,EventArgs e) {
            if(_orgTitle != _copyItemTitleTextBox.Text) {
                MpCopyItem ci = MpSingletonController.Instance.GetMpData().GetMpCopyItem(_copyItemId);
                ci.Title = CopyItemTitleTextBox.Text;                
                ci.WriteToDatabase();
            }
            if(!CopyItemTitleTextBox.Focused) {
                CopyItemTitleTextBox.Cursor = Cursors.Arrow;
                CopyItemTitleTextBox.BorderStyle = BorderStyle.None;
                CopyItemTitleTextBox.BackColor = CopyItemTileTitlePanel.BackColor;
                CopyItemTitleTextBox.ReadOnly = true;
                CopyItemTileTitlePanel.Focus();
            }            
        }

        private void _titleTextBox_Click(object sender,EventArgs e) {
            _orgTitle = CopyItemTitleTextBox.Text;
            CopyItemTitleTextBox.ReadOnly = false;
            CopyItemTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
            CopyItemTitleTextBox.BackColor = (Color)MpSingletonController.Instance.GetSetting("LogPanelTileTitleTextBoxBgColor");
        }

        private void _titleTextBox_MouseLeave(object sender,EventArgs e) {
            CopyItemTitleTextBox.Cursor = Cursors.Arrow;
            CopyItemTitleTextBox.BorderStyle = BorderStyle.None;
            CopyItemTitleTextBox.ReadOnly = true;
            CopyItemTitleTextBox.BackColor = CopyItemTileTitlePanel.BackColor;
            CopyItemTileTitlePanel.Focus();
        }

        private void _titleTextBox_MouseEnter(object sender,EventArgs e) {
            CopyItemTitleTextBox.Cursor = Cursors.IBeam;
            CopyItemTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
        }
    }
}
