using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleTextBoxController:MpController {
        public MpTileTitleTextBox TileTitleTextBox { get; set; }

        private int _copyItemId;
        private Color _panelBackColor;
        private bool _isEditMode = false;
        public MpTileTitleTextBoxController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            _copyItemId = ci.copyItemId;

            TileTitleTextBox = new MpTileTitleTextBox(tileId,panelId) {
                Text = ci.Title == string.Empty ? "Empty":ci.Title,
                ReadOnly = true,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None,
                MaxLength = Properties.Settings.Default.MaxTitleLength
            };
            if(ci.Title.Trim() == string.Empty) {
                ci.Title = "Empty";
                ci.WriteToDatabase();
            }
            TileTitleTextBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleTextBox.Click += _titleTextBox_Click;
            TileTitleTextBox.KeyUp += TileTitleTextBox_KeyUp;
            TileTitleTextBox.LostFocus += _titleTextBox_LostFocus;

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
            Size tttbs = TextRenderer.MeasureText(TileTitleTextBox.Text,TileTitleTextBox.Font);

            TileTitleTextBox.SetBounds(tp * 2,10,tttbs.Width,tttbs.Height);
            //TileTitleTextBox.BackColor = ((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor;
            //TileTitleTextBox.ForeColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.Black : Color.White;
            TileTitleTextBox.Invalidate();
        }
        private void TileTitleTextBox_KeyUp(object sender,KeyEventArgs e) {
            ((MpTileTitlePanelController)Parent).Update();
            TileTitleTextBox.Focus();
        }
        private void _titleTextBox_LostFocus(object sender,EventArgs e) {
            MpCopyItem ci = new MpCopyItem(_copyItemId);
            if(ci.Title != TileTitleTextBox.Text) {
                ci.Title = TileTitleTextBox.Text;
                ci.WriteToDatabase();
            }
            if(!TileTitleTextBox.ReadOnly) {
                ReadMode();
            }
        }

        private void _titleTextBox_Click(object sender,EventArgs e) {
            if(TileTitleTextBox.ReadOnly) {
                EditMode();
            }
            TileTitleTextBox.Focus();
        }
        private void EditMode() {
            TileTitleTextBox.ReadOnly = false;
            //TileTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
            //((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor = Color.White;
            TileTitleTextBox.BackColor = Color.White;
            TileTitleTextBox.ForeColor = Color.Black;
            ((MpTileTitlePanelController)Parent).Update();
        }
        private void ReadMode() {
            TileTitleTextBox.ReadOnly = true;
            //TileTitleTextBox.BorderStyle = BorderStyle.None;
            TileTitleTextBox.BackColor = ((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor;
            TileTitleTextBox.ForeColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.Black : Color.White;
            if(TileTitleTextBox.Text == string.Empty) {
                TileTitleTextBox.Text = "     ";
            }
                ((MpTileTitlePanelController)Parent).Update();
        }
    }
}
