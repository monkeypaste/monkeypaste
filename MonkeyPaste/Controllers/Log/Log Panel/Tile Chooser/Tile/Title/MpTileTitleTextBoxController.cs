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
        public MpTileTitleLabel TileTitleLabel { get; set; }
        //public MpTileTitleLabel TileTitleLabelShadow {get;set;}

        private int _copyItemId;

        public MpTileTitleTextBoxController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            _copyItemId = ci.CopyItemId;
            if(ci.Title.Trim() == string.Empty) {
                ci.Title = "Empty";
                ci.WriteToDatabase();
            }

            TileTitleLabel = new MpTileTitleLabel(tileId,panelId) {
                Text = ci.Title,
                //Angle = 0,
                //XOffset = MpTileTitlePanelController.ShadowOffset,
                //YOffset = MpTileTitlePanelController.ShadowOffset,
                //ShadowColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.White : Color.Black,
                //BackColor = Color.Transparent,
                //Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None,
                //ForeColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.Black : Color.White
            };
            TileTitleLabel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleLabel.Click += _titleLabel_Click;

            //TileTitleLabelShadow = new MpTileTitleLabel(tileId,panelId) {
            //    Text = ci.Title,
            //    BackColor = Color.Transparent,
            //    Margin = Padding.Empty,
            //    Padding = Padding.Empty,
            //    BorderStyle = BorderStyle.None,
            //    ForeColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.White : Color.Black
            //};

            TileTitleTextBox = new MpTileTitleTextBox(tileId,panelId) {
                Text = ci.Title,
                ReadOnly = false,
                Visible = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BorderStyle = BorderStyle.None,
                MaxLength = Properties.Settings.Default.MaxTitleLength
            };
            TileTitleTextBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleTextBox.KeyUp += TileTitleTextBox_KeyUp;
            TileTitleTextBox.LostFocus += _titleTextBox_LostFocus;

            Link(new List<MpIView> { TileTitleTextBox,TileTitleLabel });
        }

        public override void Update() {
            //tile padding
            int tp = Properties.Settings.Default.TileItemPadding;
            //tile item padding
            int tip = Properties.Settings.Default.TileItemPadding;
            //tile title panel rect
            Rectangle ttpr = ((MpTileTitlePanelController)Find("MpTileTitlePanelController")).TileTitlePanel.Bounds;

            float fontSize = (Properties.Settings.Default.TileTitleHeightFontRatio * (float)(ttpr.Height)) - (float)tp;
            fontSize = fontSize < 1.0f ? 10.0f:fontSize;
            TileTitleTextBox.Font = new Font(Properties.Settings.Default.TileTitleFont,fontSize,GraphicsUnit.Pixel);
            TileTitleLabel.Font = TileTitleTextBox.Font;
            //TileTitleLabelShadow.Font = TileTitleLabel.Font;

            //tile title textbox size
            Size tttbs = TextRenderer.MeasureText(TileTitleTextBox.Text,TileTitleTextBox.Font);

            Rectangle titleBounds = new Rectangle(tp * 2,10,tttbs.Width,tttbs.Height);
            TileTitleTextBox.Bounds = titleBounds;
            TileTitleLabel.Bounds = TileTitleTextBox.Bounds;
            //TileTitleLabelShadow.Bounds = titleBounds;
            //TileTitleLabelShadow.Location = new Point(titleBounds.X + MpTileTitlePanelController.ShadowOffset,titleBounds.Y + MpTileTitlePanelController.ShadowOffset);

            if(TileTitleLabel.Visible) {
                TileTitleLabel.BringToFront();
                TileTitleTextBox.Visible = false;
                //TileTitleLabelShadow.SendToBack();
            } else {
                TileTitleTextBox.BringToFront();
            }
            //TileTitleLabelShadow.Invalidate();
            //TileTitleTextBox.Invalidate();
            TileTitleLabel.Invalidate();
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
            ReadMode();
        }

        private void _titleLabel_Click(object sender,EventArgs e) {
            EditMode();
        }
        private void EditMode() {
            TileTitleLabel.Visible = false;
            TileTitleTextBox.Visible = true;

            //TileTitleTextBox.BorderStyle = BorderStyle.Fixed3D;
            //((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor = Color.White;
            TileTitleTextBox.BackColor = Color.White;
            TileTitleTextBox.ForeColor = Color.Black;
            TileTitleTextBox.Focus();
            ((MpTileTitlePanelController)Parent).Update();
        }
        private void ReadMode() {
            TileTitleLabel.Visible = true;
            TileTitleTextBox.Visible = false;

            //TileTitleTextBox.BorderStyle = BorderStyle.None;
            TileTitleLabel.BackColor = ((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor;
            TileTitleLabel.ForeColor = MpHelperSingleton.Instance.IsBright(((MpTileTitlePanelController)Parent).TileTitlePanel.BackColor) ? Color.Black : Color.White;
            if(TileTitleTextBox.Text == string.Empty) {
                TileTitleTextBox.Text = "     ";
            }
            TileTitleTextBox.Invalidate();
            TileTitleLabel.Invalidate();
            ((MpTileTitlePanelController)Parent).Update();
        }
    }
}
