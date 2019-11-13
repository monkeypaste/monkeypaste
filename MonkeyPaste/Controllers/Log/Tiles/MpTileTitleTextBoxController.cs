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
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None
            };
            TileTitleTextBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleTextBox.MouseHover += _titleTextBox_MouseHover;
            TileTitleTextBox.MouseLeave += _titleTextBox_LostFocus;
            TileTitleTextBox.Click += _titleTextBox_Click;
            TileTitleTextBox.Leave += _titleTextBox_MouseLeave;

            Link(new List<MpIView> { TileTitleTextBox });
        }
        public override void Update() {
            //tile  rect
            Rectangle tr = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.Bounds;
            //tile padding
            int tp = Properties.Settings.Default.TileItemPadding;
            //tile item padding
            int tip = Properties.Settings.Default.TileItemPadding;
            //tile title panel rect
            Rectangle ttpr = ((MpTileTitlePanelController)Parent).TileTitlePanel.Bounds;

            float fontSize = (Properties.Settings.Default.TileTitleHeightFontRatio * (float)(ttpr.Height)) - (float)tp;
            MpSingletonController.Instance.TileTitleFontSize = fontSize;
            TileTitleTextBox.Font = new Font(Properties.Settings.Default.TileTitleFont,fontSize,GraphicsUnit.Pixel);

            //tile title textbox size
            Size tttbs = MpHelperSingleton.Instance.GetTextSize(_orgTitle,TileTitleTextBox.Font);

            TileTitleTextBox.SetBounds(tp*2,0,tttbs.Width,tttbs.Height);
            TileTitleTextBox.BackColor = ((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor;
        }
        private void _escKeyHook_KeyPressed(object sender,KeyPressedEventArgs e) {
            object stb = (MpSearchBox)Find("MpLogMenuSearchBox");
            if(stb != null) {
                ((MpSearchBox)stb).Focus();
            }
            _titleTextBox_LostFocus(this,null);
        }
        private void _titleTextBox_LostFocus(object sender,EventArgs e) {
            if(_orgTitle != TileTitleTextBox.Text) {
                MpCopyItem ci = new MpCopyItem(_copyItemId);
                if(ci.Title != TileTitleTextBox.Text) {
                    ci.Title = TileTitleTextBox.Text;
                    ci.WriteToDatabase();
                }
            }
            if(!TileTitleTextBox.Focused) {
                TileTitleTextBox.Cursor = Cursors.Arrow;
                //TileTitleTextBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.BackColor;
                TileTitleTextBox.ReadOnly = true;
                ((MpTileTitlePanelController)Parent).TileTitlePanel.Focus();
            }
        }

        private void _titleTextBox_Click(object sender,EventArgs e) {
            _orgTitle = TileTitleTextBox.Text;
            TileTitleTextBox.ReadOnly = false;
            TileTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
            //TileTitleTextBox.BackColor = TileTitleTextBox.ForeColor == Color.White ? Color.Black:Color.White;
        }

        private void _titleTextBox_MouseLeave(object sender,EventArgs e) {
            TileTitleTextBox.Cursor = Cursors.Arrow;
            TileTitleTextBox.ReadOnly = true;
            //TileTitleTextBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.BackColor;
            ((MpTileTitlePanelController)Parent).TileTitlePanel.Focus();
        }

        private void _titleTextBox_MouseHover(object sender,EventArgs e) {
            TileTitleTextBox.Cursor = Cursors.IBeam;
            //TileTitleTextBox.BackColor = MpHelperSingleton.Instance.ChangeColorBrightness(TileTitleTextBox.BackColor,0.5f);
        }
    }
}
