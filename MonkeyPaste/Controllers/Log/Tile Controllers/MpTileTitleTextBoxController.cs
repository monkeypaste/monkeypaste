using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleTextBoxController : MpController {
        public MpTileTitleTextBox TileTitleTextBox { get; set; }
        private string _orgTitle;
        private int _copyItemId;

        public MpTileTitleTextBoxController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            _orgTitle = ci.Title;
            _copyItemId = ci.copyItemId;

            TileTitleTextBox = new MpTileTitleTextBox(tileId,panelId) {
                Text = ci.Title,               
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            TileTitleTextBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleTextBox.MouseEnter += _titleTextBox_MouseEnter;
            //TileTitleTextBox.Enter += _titleTextBox_LostFocus;
            TileTitleTextBox.MouseLeave += _titleTextBox_LostFocus;
            TileTitleTextBox.Click += _titleTextBox_Click;
            TileTitleTextBox.Leave += _titleTextBox_MouseLeave;

            Link(new List<MpIView> { TileTitleTextBox });
        }

        public override void Update() {
            //tile  rect
            Rectangle tr = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.Bounds;
            //tile padding
            int tp = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width);
            //tile title panel rect
            Rectangle ttpr = ((MpTileTitlePanelController)Parent).TileTitlePanel.Bounds;
            //tile title icon panel rect
            Rectangle ttipr = ((MpTileTitlePanelController)Parent).TileTitleIconPanelController.TileTitleIconPanel.Bounds;
            //header icon pad
            int hip = (int)((float)ttpr.Height * Properties.Settings.Default.TilePadWidthRatio);

            float fontSize = Properties.Settings.Default.TileTitleFontRatio * (float)(ttpr.Height);
            MpSingletonController.Instance.TileTitleFontSize = fontSize;
            TileTitleTextBox.SetBounds(ttipr.Right + hip,(int)(fontSize/2), ttpr.Width-ttipr.Width-(tp*4),ttpr.Height-hip-hip);
            TileTitleTextBox.Font = new Font(Properties.Settings.Default.TileTitleFont,fontSize,GraphicsUnit.Pixel);            
            TileTitleTextBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.BackColor;
            TileTitleTextBox.ForeColor = MpHelperSingleton.Instance.IsBright(TileTitleTextBox.BackColor) ? Color.Black : Color.White;
        }
        private void _escKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            throw new NotImplementedException();
        }
        private void _titleTextBox_LostFocus(object sender,EventArgs e) {
            if(_orgTitle != TileTitleTextBox.Text) {
                MpCopyItem ci = MpSingletonController.Instance.GetMpData().GetMpCopyItem(_copyItemId);
                if(ci.Title != TileTitleTextBox.Text) {
                    ci.Title = TileTitleTextBox.Text;
                    ci.WriteToDatabase();
                }
            }
            if(!TileTitleTextBox.Focused) {
                TileTitleTextBox.Cursor = Cursors.Arrow;
                TileTitleTextBox.BorderStyle = BorderStyle.None;
                TileTitleTextBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.BackColor;
                TileTitleTextBox.ReadOnly = true;
                ((MpTileTitlePanelController)Parent).TileTitlePanel.Focus();
            }
        }

        private void _titleTextBox_Click(object sender,EventArgs e) {
            _orgTitle = TileTitleTextBox.Text;
            TileTitleTextBox.ReadOnly = false;
            TileTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
            TileTitleTextBox.BackColor = TileTitleTextBox.ForeColor == Color.White ? Color.Black:Color.White;
        }

        private void _titleTextBox_MouseLeave(object sender,EventArgs e) {
            TileTitleTextBox.Cursor = Cursors.Arrow;
            TileTitleTextBox.BorderStyle = BorderStyle.None;
            TileTitleTextBox.ReadOnly = true;
            TileTitleTextBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.BackColor;
            ((MpTileTitlePanelController)Parent).TileTitlePanel.Focus();
        }

        private void _titleTextBox_MouseEnter(object sender,EventArgs e) {
            TileTitleTextBox.Cursor = Cursors.IBeam;
            TileTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
            TileTitleTextBox.BackColor = MpHelperSingleton.Instance.ChangeColorBrightness(TileTitleTextBox.BackColor,0.5f);
        }
    }
}
