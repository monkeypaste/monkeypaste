using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleTextBoxController : MpController {
        public TextBox TileTitleTextBox { get; set; }
        private string _orgTitle;
        private int _copyItemId;

        public MpTileTitleTextBoxController(MpCopyItem ci,MpController parentController) : base(parentController) {
            _orgTitle = ci.Title;
            _copyItemId = ci.copyItemId;

            TileTitleTextBox = new TextBox() {
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
        }

        public override void UpdateBounds() {
            //tile title panel rect
            Rectangle ttpr = ((MpTileTitlePanelController)ParentController).TileTitlePanel.Bounds;
            //tile title icon panel rect
            Rectangle ttipr = ((MpTileTitlePanelController)ParentController).TileTitleIconPanelController.TileTitleIconPanel.Bounds;
            //header icon pad
            int hip = (int)((float)ttpr.Width * (float)MpSingletonController.Instance.Settings.GetSetting("TilePadWidthRatio"));

            float fontSize = (float)MpSingletonController.Instance.GetSetting("TileTitleFontRatio") * (float)(ttpr.Height-hip*2);
            TileTitleTextBox.SetBounds(ttipr.Right+hip,hip,ttpr.Width-ttipr.Width-(hip*3),ttpr.Height-hip-hip);
            TileTitleTextBox.Font = new Font((string)MpSingletonController.Instance.GetSetting("TileTitleFont"),fontSize,GraphicsUnit.Pixel);

            TileTitleTextBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)ParentController).ParentController).TilePanel.BackColor;
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
                TileTitleTextBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)ParentController).ParentController).TilePanel.BackColor;
                TileTitleTextBox.ReadOnly = true;
                ((MpTileTitlePanelController)ParentController).TileTitlePanel.Focus();
            }
        }

        private void _titleTextBox_Click(object sender,EventArgs e) {
            _orgTitle = TileTitleTextBox.Text;
            TileTitleTextBox.ReadOnly = false;
            TileTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
            TileTitleTextBox.BackColor = Color.White;
        }

        private void _titleTextBox_MouseLeave(object sender,EventArgs e) {
            TileTitleTextBox.Cursor = Cursors.Arrow;
            TileTitleTextBox.BorderStyle = BorderStyle.None;
            TileTitleTextBox.ReadOnly = true;
            TileTitleTextBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)ParentController).ParentController).TilePanel.BackColor;
            ((MpTileTitlePanelController)ParentController).TileTitlePanel.Focus();
        }

        private void _titleTextBox_MouseEnter(object sender,EventArgs e) {
            TileTitleTextBox.Cursor = Cursors.IBeam;
            TileTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
        }
    }
}
